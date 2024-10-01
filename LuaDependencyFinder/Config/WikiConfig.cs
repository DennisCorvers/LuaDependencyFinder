using System.Text.Json;
using System.Text.Json.Serialization;

namespace LuaDependencyFinder.Config
{
    public record WikiConfig : IJsonOnSerializing
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
        };

        private const string FileName = "DepConfig.json";

        [JsonPropertyName("Dependencies")]
        [JsonPropertyOrder(3)]
        public WikiDependency[]? Deps { get; private set; }

        [JsonPropertyOrder(0)]
        public string WikiDomain { get; }

        [JsonPropertyOrder(1)]
        public string ArticlePath { get; }

        [JsonPropertyOrder(2)]
        public string ApiPath { get; }

        [JsonIgnore]
        public string ArticlePathFixed
        {
            get
            {
                var fixedPath = ArticlePath;
                if (ArticlePath.EndsWith("$1"))
                {
                    fixedPath = ArticlePath.Substring(0, ArticlePath.Length - 2);
                }

                if (!fixedPath.EndsWith("/"))
                    fixedPath += "/";

                return fixedPath;
            }
        }

        [JsonIgnore]
        public Dictionary<string, WikiDependency> Dependencies { get; private set; }

        [JsonConstructor]
        public WikiConfig(string wikiDomain, string articlePath, string apiPath, WikiDependency[]? deps)
        {
            WikiDomain = wikiDomain;
            if (deps != null)
            {
                Dependencies = deps.ToDictionary(
                    k => k.WikiPage,
                    v => v);
            }
            else
            {
                Dependencies = new();
            }

            ArticlePath = articlePath ?? string.Empty;
            ApiPath = apiPath ?? string.Empty;
        }

        public void OnSerializing()
        {
            Deps = Dependencies.Values.ToArray();
        }

        public static WikiConfig Create(string wikiDomain)
        {
            return new WikiConfig(wikiDomain, "", "", null);
        }

        public static WikiConfig? Load()
        {
            using FileStream fileStream = new(FileName, FileMode.Open, FileAccess.Read);
            return JsonSerializer.Deserialize<WikiConfig>(fileStream, JsonSerializerOptions);
        }

        public void Save()
        {
            try
            {
                using FileStream filestream = new(FileName, FileMode.Create, FileAccess.Write);
                JsonSerializer.Serialize<WikiConfig>(filestream, this, JsonSerializerOptions);
            }
            catch { }
        }
    }
}
