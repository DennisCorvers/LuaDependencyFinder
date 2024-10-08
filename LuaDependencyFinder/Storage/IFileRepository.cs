using LuaDependencyFinder.Models;

namespace LuaDependencyFinder.Storage
{
    internal interface IFileRepository
    {
        IEnumerable<WikiDependency> GetLocalDependencies();

        Task StorePages(IEnumerable<WikiPage> pages);
    }
}
