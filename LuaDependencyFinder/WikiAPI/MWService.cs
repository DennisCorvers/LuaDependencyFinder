using LuaDependencyFinder.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LuaDependencyFinder.WikiAPI
{
    public class MWService
    {
        private readonly string m_domain;

        public MWService(string domain)
        {
            m_domain = domain.TrimEnd('/');
        }

        public void DownloadDependency()
        {

        }

        public async Task<IEnumerable<PageInfo>> GetRevisionHistory(IEnumerable<string> pages)
        {
            var controller = new MWController(new HttpClient(), m_domain);
            var result = await controller.GetRevisionHistory(pages);

            if (result == null || result.Query == null)
            {
                throw new InvalidOperationException("No response received.");
            }

            var pagesResult = result.Query.Pages;
            return pagesResult
                .Select(x => Map(x.Value))
                .ToImmutableList();

            static PageInfo Map(Page page)
            {
                var latestRevision = page.Revisions?.FirstOrDefault()?.Timestamp;

                return new PageInfo(
                    page.PageId,
                    page.Namespace,
                    page.Title,
                    latestRevision
                    );
            }
        }
    }
}
