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
        private readonly string m_domain;

        public MWController(HttpClient httpClient, string domain)
        {
            m_httpClient = httpClient;
            m_domain = domain;
        }

        public async Task<MediaWikiResponse?> GetRevisionHistory(IEnumerable<string> pages)
        {
            if (pages.Count() > 50)
            {
                // TODO: make multiple API Calls instead
                throw new Exception("Too many pages at once");
            }
            var pageQuery = string.Join("|", pages);
            var url = $"{m_domain}/api.php?action=query&prop=revisions&titles={pageQuery}&rvlimit=1&rvprop=timestamp&format=json";
            

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
