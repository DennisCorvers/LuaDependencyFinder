using LuaDependencyFinder.Models;

namespace LuaDependencyFinder.Storage
{
    internal interface IFileRepository
    {
        string RootDirectory { get; }

        IEnumerable<WikiDependency> GetLocalDependencies();

        Task StorePages(IEnumerable<WikiPage> pages);

        Task<WikiPage> LoadDepencency(WikiDependency dependency);
        IEnumerable<WikiDependency> GetLocalDependencies(IEnumerable<string> files);
    }
}
