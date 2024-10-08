using System.Diagnostics.CodeAnalysis;
using LuaDependencyFinder.Models;

namespace LuaDependencyFinder.Config
{
    public interface IWikiConfig
    {
        string WikiDomain { get; }

        string ArticlePath { get; set; }

        string ApiPath { get; set; }

        string ArticlePathFixed { get; }

        IReadOnlyCollection<WikiDependency> WikiDependencies { get; }

        void AddOrUpdate(WikiDependency dependency);

        bool TryFind(string dependencyName, [NotNullWhen(true)] out WikiDependency? dependency);

        void Persist();
    }
}
