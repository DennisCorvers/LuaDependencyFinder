using LuaDependencyFinder.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LuaDependencyFinder.WikiAPI
{
    public class MWController
    {
        private readonly HttpClient m_httpClient;
        private readonly WikiConfig m_config;
        private readonly Uri m_apiUri;

        public MWController(HttpClient httpClient, WikiConfig config)
        {
            m_httpClient = httpClient;
            m_config = config;
            m_apiUri = new Uri(new Uri(config.WikiDomain), config.ApiPath);
        }

        public async Task<MediaWikiResponse?> GetRevisionHistory(IEnumerable<string> pages)
        {
            if (pages.Count() > 50)
            {
                // TODO: make multiple API Calls instead
                throw new Exception("Too many pages at once");
            }
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
            var result = System.Text.Json.JsonSerializer.Deserialize<MediaWikiResponse>(json);

            return result;
        }
    }
}
