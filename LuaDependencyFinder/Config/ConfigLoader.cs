using LuaDependencyFinder.Logging;

namespace LuaDependencyFinder.Config
{
    internal class ConfigLoader
    {
        private readonly ILogger m_logger;

        public ConfigLoader(ILogger logger)
        {
            m_logger = logger;
        }

        public static bool HasConfigFile
            => File.Exists(WikiConfig.FileName);

        public IWikiConfig? CreateNewConfig(string wikiDomain)
        {
            IWikiConfig config = WikiConfig.Create(wikiDomain);

            try
            {
                config.Persist();
            }
            catch (Exception e)
            {
                m_logger.Log("Unable to create new configuration.");
                m_logger.Log(e.Message);
                return null;
            }

            return config;
        }

        public bool TryLoadConfig(out IWikiConfig? config)
        {
            config = null;
            if (!HasConfigFile)
            {
                return false;
            }

            try
            {
                config = WikiConfig.Load();
                if (config == null)
                {
                    m_logger.Log("Unable to load config.");
                    return false;
                }

                if (!Utils.StringUtils.IsValidUrl(config.WikiDomain))
                {
                    m_logger.Log($"Invalid url found in configuration: \"{config.WikiDomain}\"");
                    m_logger.Log("Edit the configuration so that the url is a valid mediawiki address.");
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                m_logger.Log("Unable to load configuration.");
                m_logger.Log(e.Message);
                return false;
            }
        }
    }
}
