using LuaDependencyFinder.Config;
using LuaDependencyFinder.Logging;
using LuaDependencyFinder.Models;

namespace LuaDependencyFinder
{
    internal class DepWorker
    {
        private readonly HttpClient m_workerClient;
        private readonly WikiConfig m_config;
        private readonly ILogger m_logger;
        private readonly Uri m_articleUrl;

        public DepWorker(WikiConfig config, ILogger logger)
        {
            m_config = config;
            m_workerClient = new HttpClient();
            m_logger = logger;
            m_articleUrl = new Uri(new Uri(m_config.WikiDomain), m_config.ArticlePathFixed);
        }

        public async Task<WikiPage> DownloadDependency(string page)
        {
            var pageUrl = new UriBuilder(m_articleUrl + page)
            {
                Query = "?action=raw",
            }.Uri;
            m_logger.Log($"Downloading contents from {pageUrl}");

            var contents = await m_workerClient.GetStringAsync(pageUrl);

            return new WikiPage(page, DateTime.UtcNow, contents);
        }
    }
}
