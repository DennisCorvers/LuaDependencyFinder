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

        public string Root
            => Directory.GetCurrentDirectory();

        public FileRepository(IWikiConfig config, ILogger logger)
        {
            m_config = config;
            m_logger = logger;
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
            var files = Directory.GetFiles(Root, "*.lua", SearchOption.AllDirectories);

            var metadata = new List<WikiDependency>();

            foreach (var file in files)
            {
                var path = Path.GetRelativePath(Root, file);
                var fixedName = Mapping.PathToWikiPage(path);

                if (m_config.TryFind(fixedName, out var dependency))
                    metadata.Add(dependency);
                else
                    metadata.Add(new WikiDependency(fixedName, default, path, false));
            }

            return metadata;
        }

        private async Task StorePage(WikiPage page)
        {
            var relativePath = Mapping.WikiPageToPath(page.Page);
            var outputPath = Path.Combine(Root, relativePath);
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
                m_logger.Log($"Unable to save page: {e.Message}.");
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
                var outputPath = Path.Combine(Root, Mapping.WikiPageToPath(page.Page));
                var directoryPath = Path.GetDirectoryName(outputPath)!;
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
            }
        }
    }
}
