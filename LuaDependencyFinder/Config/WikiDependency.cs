using LuaDependencyFinder.Models;

namespace LuaDependencyFinder.Config
{
    public class WikiDependency
    {
        public string WikiPage { get; }

        public DateTime Timestamp { get; }

        public string Path { get; }

        public bool Tracking { get; }

        public WikiDependency(string wikiPage, DateTime timestamp, string path, bool tracking = true)
        {
            WikiPage = wikiPage;
            Timestamp = timestamp;
            Path = path;
            Tracking = tracking;
        }
    }
}
