using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LuaDependencyFinder.WikiAPI.Models
{
    public class MediaWikiRevision
    {
        [JsonPropertyName("query")]
        public Query? Query { get; set; }
    }

    public class Query
    {
        [JsonPropertyName("pages")]
        public Dictionary<string, Page> Pages { get; set; } = new();
    }

    public class Page
    {
        [JsonPropertyName("pageid")]
        public int PageId { get; set; }

        [JsonPropertyName("ns")]
        public int Namespace { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("revisions")]
        public List<Revision>? Revisions { get; set; }
    }

    public class Revision
    {
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
