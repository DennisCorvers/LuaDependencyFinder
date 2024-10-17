using LuaDependencyFinder.Config;
using LuaDependencyFinder.Logging;
using LuaDependencyFinder.Models;
using LuaDependencyFinder.WikiAPI.Models;

namespace LuaDependencyFinder.Storage
{
    internal class FileRepository : IFileRepository
    {
        private readonly IWikiConfig m_config;
        private readonly ILogger m_logger;

        public string RootDirectory
        { get; private set; }

        public FileRepository(IWikiConfig config, ILogger logger, string? root = null)
        {
            m_config = config;
            m_logger = logger;
            RootDirectory = root ?? Directory.GetCurrentDirectory();
        }

        public async Task StorePages(IEnumerable<WikiPage> pages)
        {
            // Make sure directories exist first
            CreateDirectories(pages);

            // Store file(s) and update dependency information
            var workers = pages.Select(StorePage);

            await Task.WhenAll(workers);

            if (pages.Any())
            {
                m_logger.Log("Updating dependency config.");
                m_config.Persist();
            }
        }

        public IEnumerable<WikiDependency> GetLocalDependencies()
        {
            // Finds all .lua and .json files in the (sub)directory.
            var files = Directory.GetFiles(RootDirectory, "*.lua", SearchOption.AllDirectories);
            return GetLocalDependencies(files);
        }

        public IEnumerable<WikiDependency> GetLocalDependencies(IEnumerable<string> files)
        {
            var metadata = new List<WikiDependency>();

            foreach (var file in files)
            {
                var path = Path.GetRelativePath(RootDirectory, file);
                var fixedName = Mapping.PathToWikiPage(path);

                if (m_config.TryFind(fixedName, out var dependency))
                    metadata.Add(dependency);
                else
                    metadata.Add(new WikiDependency(fixedName, default, path, false));
            }

            return metadata;
        }

        public async Task<WikiPage> LoadDepencency(WikiDependency dependency)
        {
            using (var sr = new StreamReader(dependency.Path, System.Text.Encoding.UTF8))
            {
                var contents = await sr.ReadToEndAsync();
                sr.Close();

                return new WikiPage(dependency.WikiPage, default, contents);
            }
        }

        private async Task StorePage(WikiPage page)
        {
            var relativePath = Mapping.WikiPageToPath(page.Page);
            var outputPath = Path.Combine(RootDirectory, relativePath);
            var success = false;

            try
            {
                m_logger.Log($"Storing dependency \"{relativePath}\" to disk.");

                using (var streamWriter = new StreamWriter(outputPath, false))
                {
                    await streamWriter.WriteAsync(page.Contents);
                }

                success = true;
            }
            catch (Exception e)
            {
                m_logger.LogException($"Unable to save page.", e);
            }

            if (!success)
            {
                return;
            }

            // Update config with newly downloaded depencency.
            var wikiDep = new WikiDependency(page.Page, page.TimeStamp, relativePath);
            m_config.AddOrUpdate(wikiDep);
        }

        private void CreateDirectories(IEnumerable<WikiPage> pages)
        {
            foreach (var page in pages)
            {
                var outputPath = Path.Combine(RootDirectory, Mapping.WikiPageToPath(page.Page));
                var directoryPath = Path.GetDirectoryName(outputPath)!;
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
            }
        }
    }
}
