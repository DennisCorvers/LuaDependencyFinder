using LuaDependencyFinder.Models;

namespace LuaDependencyFinder.Config
{
    public class WikiDependency
    {
        public string WikiPage { get; }

        public DateTime Timestamp { get; }

        public string Path { get; }

        public WikiDependency(string wikiPage, DateTime timestamp, string path)
        {
            WikiPage = wikiPage;
            Timestamp = timestamp;
            Path = path;
        }
    }
}
