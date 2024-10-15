using LuaDependencyFinder.Config;
using LuaDependencyFinder.Logging;
using LuaDependencyFinder.Models;
using LuaDependencyFinder.Storage;
using LuaDependencyFinder.WikiAPI;
using System.Collections.Immutable;

namespace LuaDependencyFinder.Analysing
{
    internal class DepFinder
    {
        private readonly IWikiConfig m_config;
        private readonly IMWService m_mwService;
        private readonly ILogger m_logger;
        private readonly IFileRepository m_fileRepository;
        private readonly ILuaAnalyser m_luaAnalyser;

        public DepFinder(IWikiConfig config, ILogger logger, IFileRepository fileRepository)
        {
            m_config = config;
            m_mwService = new MWService(config, logger);
            m_logger = logger;
            m_fileRepository = fileRepository;
            m_luaAnalyser = new LuaAnalyser();
        }

        /// <summary>
        /// Updates all provided files to their latest version, if possible.
        /// </summary>
        public async Task PatchFiles()
        {
            m_logger.Log("Patching local files...");
            var pages = m_fileRepository.GetLocalDependencies().Select(x => x.WikiPage);
            // Check the wiki for revision history on all local files.
            var revisionHistory = await m_mwService.GetRevisionHistory(pages);

            // Find outdated pages
            var patchablePages = revisionHistory
                .Where(IsPatchablePage)
                .ToArray();

            m_logger.Log($"{patchablePages.Length} outdated or unlisted pages found.");

            // Download and store patchable pages.
            if (patchablePages.Any())
            {
                var wikiPages = await m_mwService.GetDependencies(patchablePages.Select(x => x.Title));
                wikiPages = wikiPages.Select(x => m_luaAnalyser.PatchDependency(x));

                await m_fileRepository.StorePages(wikiPages);
            }
        }

        public Task DownloadDependencies()
            => DownloadDependencies(Enumerable.Empty<WikiDependency>());

        public async Task DownloadDependencies(IEnumerable<WikiDependency> dependencies)
        {
            var localDependencies = m_fileRepository.GetLocalDependencies();
            var collectedPages = new HashSet<string>(
                localDependencies
                    .Concat(dependencies)
                    .Select(x => x.WikiPage));

            // Load local dependencies if there are no dependencies explicitely given.
            dependencies = dependencies.Any() ? dependencies : localDependencies;
            var tasks = dependencies.Select(m_fileRepository.LoadDepencency);
            var wikiPages = await Task.WhenAll(tasks);

            await DownloadAndStoreDependencies(wikiPages, collectedPages);
        }

        private async Task DownloadAndStoreDependencies(IEnumerable<WikiPage> dependencies, ISet<string> pages)
        {
            var requiredDependencies = dependencies
                .SelectMany(x => m_luaAnalyser.AnalyseLuaFile(x.Contents))
                .DistinctBy(x => x.DependencyName)
                .Where(x => !pages.Contains(x.ModuleName))
                .Select(x => x.ModuleName)
                .ToImmutableArray();

            if (!requiredDependencies.Any())
            {
                m_logger.Log("No (more) dependencies found to download.");
                return;
            }

            var wikiDeps = await m_mwService.GetDependencies(requiredDependencies);
            wikiDeps = wikiDeps.Select(x => m_luaAnalyser.PatchDependency(x));

            await m_fileRepository.StorePages(wikiDeps);

            // Add stored pages to avoid downloading them again.
            foreach (var dep in requiredDependencies)
            {
                pages.Add(dep);
            }

            // Recursively analyse nested dependencies
            await DownloadAndStoreDependencies(wikiDeps, pages);
        }

        private bool IsPatchablePage(PageRevision page)
        {
            // Page needs to exist on the Wiki.
            if (page.PageId == 0)
                return false;

            // Page is unknown. (Syncable)
            if (!m_config.TryFind(page.Title, out var dependency))
                return true;

            // Only include pages marked as trackable. (Outdated page)
            if (dependency.Tracking)
                return dependency.Timestamp < page.LatestRevision;

            return false;
        }
    }
}
