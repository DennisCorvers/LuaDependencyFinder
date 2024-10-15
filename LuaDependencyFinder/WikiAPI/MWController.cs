using LuaDependencyFinder.Config;
using LuaDependencyFinder.Logging;
using LuaDependencyFinder.Models;
using LuaDependencyFinder.WikiAPI.Models;
using System.Security.Cryptography.X509Certificates;

namespace LuaDependencyFinder.WikiAPI
{
    public class MWController
    {
        private readonly HttpClient m_httpClient;
        private readonly IWikiConfig m_config;
        private readonly Uri m_apiUri;
        private readonly Uri m_articleUrl;
        private readonly ILogger m_logger;

        public MWController(IWikiConfig config, ILogger logger)
        {
            m_logger = logger;
            m_httpClient = new HttpClient();
            m_config = config;
            m_apiUri = new Uri(new Uri(config.WikiDomain), config.ApiPath);
            m_articleUrl = new Uri(new Uri(m_config.WikiDomain), m_config.ArticlePathFixed);
        }

        public async Task<MediaWikiRevision?> GetRevisionHistory(IEnumerable<string> pages)
        {
            var distinctPages = pages
                .Distinct()
                .Chunk(50);

            var revisionChunksTasks = distinctPages.Select(GetRevisionHistoryChunk);
            var revisionChunks = await Task.WhenAll(revisionChunksTasks);

            if (!revisionChunks.Any())
            {
                return null;
            }

            return revisionChunks
                    .Where(chunk => chunk != null)
                    .Aggregate((first, next) =>
                    {
                        foreach (var page in next!.Query.Pages)
                        {
                            first!.Query.Pages[page.Key] = page.Value;
                        }
                        return first;
                    });
        }

        private async Task<MediaWikiRevision?> GetRevisionHistoryChunk(IEnumerable<string> pages)
        {
            var pageQuery = string.Join("|", pages);
            var url = new UriBuilder(m_apiUri)
            {
                Query = $"?action=query&prop=revisions&titles={pageQuery}&rvprop=timestamp&format=json",
            }.Uri;

            var response = await m_httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Error retrieving data from MediaWiki: {errorMessage}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = System.Text.Json.JsonSerializer.Deserialize<MediaWikiRevision>(json);

            return result;
        }

        public async Task<WikiPage?> DownloadDependency(string page)
        {
            var pageUrl = new UriBuilder(m_articleUrl + page)
            {
                Query = "?action=raw",
            }.Uri;
            m_logger.Log($"Downloading contents from {pageUrl}");

            var response = await m_httpClient.GetAsync(pageUrl);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                m_logger.Log($"Page \"{pageUrl}\" does not exist.");
                return null;
            }

            var contents = await response.Content.ReadAsStringAsync();

            return new WikiPage(page, DateTime.UtcNow, contents);
        }
    }
}
