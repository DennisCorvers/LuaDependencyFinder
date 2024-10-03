﻿using LuaDependencyFinder.Config;
using LuaDependencyFinder.Logging;
using LuaDependencyFinder.Models;
using LuaDependencyFinder.WikiAPI;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace LuaDependencyFinder
{
    internal class DepFinder
    {
        private readonly WikiConfig m_config;
        private readonly MWService m_mwService;
        private readonly ILogger m_logger;
        private readonly string m_root = Directory.GetCurrentDirectory();

        private readonly Dictionary<string, LuaFileInfo> m_localFiles;

        public DepFinder(WikiConfig config, ILogger logger)
        {
            m_config = config;
            m_mwService = new MWService(config, logger);
            m_logger = logger;
            m_localFiles = GatherFiles();
        }

        private Dictionary<string, LuaFileInfo> GatherFiles()
        {
            // Finds all .lua and .json files in the (sub)directory.
            var files = Directory.GetFiles(m_root, "*.lua", SearchOption.AllDirectories);

            var metadata = new List<LuaFileInfo>();

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var path = Path.GetRelativePath(m_root, file);

                metadata.Add(new LuaFileInfo(path, fileName));
            }

            return metadata.ToDictionary(
                k => k.ArticlePath,
                v => v);
        }

        /// <summary>
        /// Updates all provided files to their latest version, if possible.
        /// </summary>
        public async Task PatchFiles()
        {
            m_logger.Log("Patching local files...");
            var pages = m_localFiles.Values.Select(x => x.ArticlePath);
            // Check the wiki for revision history on all local files.
            var revisionHistory = await m_mwService.GetRevisionHistory(pages);

            // Find outdated pages
            var patchablePages = revisionHistory
                .Where(IsPatchablePage)
                .ToArray();

            m_logger.Log($"{patchablePages.Length} outdated or unlisted pages found.");

            var wikiPages = await m_mwService.DownloadDependencies(patchablePages.Select(x => x.Title));
            await SyncFiles(wikiPages);
        }

        public async Task DownloadDependencies()
        {
            var analyser = new LuaAnalyser();
            var set = new HashSet<string>();

            foreach (var file in m_localFiles.Values)
            {
                var analyserInfo = await analyser.AnalyseLuaFile(file);
                if (analyserInfo.Any(x => x.ContainsModulePrefix))
                {
                    await FixModuleName(file, analyserInfo);
                }
            }
        }

        private bool IsPatchablePage(PageRevision page)
        {
            // Page needs to exist on the Wiki.
            if (page.PageId == 0)
                return false;

            // Page is unknown. (Syncable)
            if (!m_config.Dependencies.TryGetValue(page.Title, out var dependency))
                return true;

            // Only include pages marked as trackable. (Outdated page)
            if (dependency.Tracking)
                return dependency.Timestamp < page.LatestRevision;

            return false;
        }

        private async Task FixModuleName(LuaFileInfo fileInfo,)
        {

        }

        private async Task SyncFiles(IEnumerable<WikiPage> pages)
        {
            // Make sure directories exist first
            CreateDirectories(pages);

            // Store file(s) and update dependency information
            var workers = pages.Select(async page =>
            {
                try
                {
                    var relativePath = Mapping.WikiPageToPath(page.Page);
                    var outputPath = Path.Combine(m_root, relativePath);

                    m_logger.Log($"Storing dependency \"{relativePath}\" to disk.");
                    using (var streamWriter = new StreamWriter(outputPath, false))
                        await streamWriter.WriteAsync(page.Contents);

                    var wikiDep = new WikiDependency(page.Page, page.TimeStamp, relativePath);
                    lock (m_config)
                    {
                        m_config.Dependencies[page.Page] = wikiDep;
                    }
                }
                catch (Exception e)
                {
                    m_logger.Log($"Unable to save page: {e.Message}.");
                }
            });

            await Task.WhenAll(workers);

            if (pages.Any())
            {
                m_logger.Log("Updating dependency config.");
                m_config.Save();
            }
        }

        private void CreateDirectories(IEnumerable<WikiPage> pages)
        {
            foreach (var page in pages)
            {
                var outputPath = Path.Combine(m_root, Mapping.WikiPageToPath(page.Page));
                var directoryPath = Path.GetDirectoryName(outputPath)!;
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
            }
        }
    }
}
