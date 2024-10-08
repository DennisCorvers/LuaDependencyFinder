using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using LuaDependencyFinder.Models;

namespace LuaDependencyFinder.Config
{
    public record WikiConfig : IWikiConfig, IJsonOnSerializing
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        private readonly Dictionary<string, WikiDependency> m_dependencies;

        public const string FileName = "DepConfig.json";

        [JsonPropertyName("Dependencies")]
        [JsonPropertyOrder(3)]
        public WikiDependency[]? Deps { get; private set; }

        [JsonPropertyOrder(0)]
        public string WikiDomain { get; }

        [JsonPropertyOrder(1)]
        public string ArticlePath { get; set; }

        [JsonPropertyOrder(2)]
        public string ApiPath { get; set; }

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
        public IReadOnlyCollection<WikiDependency> WikiDependencies
        {
            get
            {
                lock (m_dependencies)
                {
                    return m_dependencies.Values;
                }
            }
        }

        [JsonConstructor]
        public WikiConfig(string wikiDomain, string articlePath, string apiPath, WikiDependency[]? deps)
        {
            WikiDomain = wikiDomain;
            if (deps != null)
            {
                m_dependencies = deps.ToDictionary(
                    k => k.WikiPage,
                    v => v);
            }
            else
            {
                m_dependencies = new();
            }

            ArticlePath = articlePath ?? string.Empty;
            ApiPath = apiPath ?? string.Empty;
        }

        public void OnSerializing()
        {
            Deps = m_dependencies.Values.ToArray();
        }

        public static IWikiConfig Create(string wikiDomain)
        {
            return new WikiConfig(wikiDomain, "", "", null);
        }

        public static IWikiConfig? Load()
        {
            using FileStream fileStream = new(FileName, FileMode.Open, FileAccess.Read);
            return JsonSerializer.Deserialize<WikiConfig>(fileStream, JsonSerializerOptions);
        }

        public void Persist()
        {
            try
            {
                using FileStream filestream = new(FileName, FileMode.Create, FileAccess.Write);
                JsonSerializer.Serialize<WikiConfig>(filestream, this, JsonSerializerOptions);
            }
            catch
            {
#if DEBUG
                throw;
#endif
            }
        }

        public void AddOrUpdate(WikiDependency dependency)
        {
            lock (m_dependencies)
            {
                m_dependencies[dependency.WikiPage] = dependency;
            }
        }

        public bool TryFind(string dependencyName, [NotNullWhen(true)] out WikiDependency? dependency)
        {
            lock (m_dependencies)
            {
                return m_dependencies.TryGetValue(dependencyName, out dependency);
            }
        }
    }
}
