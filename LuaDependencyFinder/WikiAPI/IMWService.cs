using LuaDependencyFinder.Models;

namespace LuaDependencyFinder.WikiAPI
{
    internal interface IMWService
    {
        Task<IEnumerable<WikiPage>> GetDependencies(IEnumerable<string> pages);

        Task<IEnumerable<PageRevision>> GetRevisionHistory(IEnumerable<string> pages);
    }
}
