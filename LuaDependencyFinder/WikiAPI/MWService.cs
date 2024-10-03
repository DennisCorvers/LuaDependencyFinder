using LuaDependencyFinder.Config;
using LuaDependencyFinder.Logging;
using LuaDependencyFinder.Models;
using System.Collections.Immutable;

namespace LuaDependencyFinder.WikiAPI
{
    public class MWService
    {
        private readonly WikiConfig m_config;
        private readonly HttpClient m_httpClient;
        private readonly ILogger m_logger;
        private readonly Uri m_articleUrl;

        public MWService(WikiConfig config, ILogger logger)
        {
            m_config = config;
            m_httpClient = new HttpClient();
            m_logger = logger;
            m_articleUrl = new Uri(new Uri(m_config.WikiDomain), m_config.ArticlePathFixed);
        }

        public async Task<IEnumerable<WikiPage>> DownloadDependencies(IEnumerable<string> pages)
        {
            var workerTasks = pages.Select(async page =>
            {
                try
                {
                    return await DownloadDependency(page);
                }
                catch (Exception e)
                {
                    m_logger.Log($"Unable to download page: {e.Message}.");
                    return default;
                }
            });

            var results = await Task.WhenAll(workerTasks);
            return results.Where(x => x != null)!;
        }

        public async Task<WikiPage> DownloadDependency(string page)
        {
            var pageUrl = new UriBuilder(m_articleUrl + page)
            {
                Query = "?action=raw",
            }.Uri;
            m_logger.Log($"Downloading contents from {pageUrl}");

            var contents = await m_httpClient.GetStringAsync(pageUrl);

            return new WikiPage(page, DateTime.UtcNow, contents);
        }

        public async Task<IEnumerable<PageRevision>> GetRevisionHistory(IEnumerable<string> pages)
        {
            m_logger.Log($"Getting revision history for pages: {string.Join('|', pages)}");

            var controller = new MWController(m_httpClient, m_config);
            var result = await controller.GetRevisionHistory(pages);

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
    }
}
