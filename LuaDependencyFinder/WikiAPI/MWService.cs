using LuaDependencyFinder.Config;
using LuaDependencyFinder.Logging;
using LuaDependencyFinder.Models;
using LuaDependencyFinder.WikiAPI.Models;
using System.Collections.Immutable;

namespace LuaDependencyFinder.WikiAPI
{
    public class MWService : IMWService
    {
        private readonly ILogger m_logger;
        private readonly MWController m_controller;

        public MWService(IWikiConfig config, ILogger logger)
        {
            m_logger = logger;
            m_controller = new MWController(config, logger);
        }

        public async Task<IEnumerable<WikiPage>> GetDependencies(IEnumerable<string> pages)
        {
            var workerTasks = pages.Select(async page =>
            {
                return await m_controller.DownloadDependency(page);
            });

            var results = await Task.WhenAll(workerTasks);
            return results.Where(x => x != null)!;
        }

        public async Task<IEnumerable<PageRevision>> GetRevisionHistory(IEnumerable<string> pages)
        {
            m_logger.Log($"Getting revision history for pages: {string.Join('|', pages)}");

            var result = await m_controller.GetRevisionHistory(pages);

            if (result == null || result.Query == null)
            {
                throw new InvalidOperationException("No response received.");
            }

            var pagesResult = result.Query.Pages;
            return pagesResult
                .Select(x => Map(x.Value))
                .ToImmutableList();

            static PageRevision Map(Page page)
            {
                var latestRevision = page.Revisions?.FirstOrDefault()?.Timestamp;

                return new PageRevision(
                    page.PageId,
                    page.Namespace,
                    page.Title,
                    latestRevision
                    );
            }
        }

        public async Task<IWikiConfig> DiscoverEndpoints()
        {
            throw new NotImplementedException();
        }
    }
}
