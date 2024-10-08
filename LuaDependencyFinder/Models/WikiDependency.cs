using System.Text.Json.Serialization;

namespace LuaDependencyFinder.Models
{
    public class WikiDependency
    {
        public string WikiPage { get; }
        public DateTime Timestamp { get; }
        public string Path { get; }

        [JsonIgnore]
        public bool Tracking { get; }

        // Constructor to initialize the properties
        public WikiDependency(string wikiPage, DateTime timestamp, string path, bool tracking = true)
        {
            WikiPage = wikiPage;
            Timestamp = timestamp;
            Path = path;
            Tracking = tracking;
        }
    }
}
