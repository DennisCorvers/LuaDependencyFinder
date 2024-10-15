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
                m_logger.Log("Unable to create new configuration.", e);
                return null;
            }

            return config;
        }

        public bool TryLoadConfig(out IWikiConfig? config)
        {
            config = null;

            // Try and create a new configuration file if none is present.
            if (!HasConfigFile)
            {
                m_logger.Log("No valid configuration file found. Creating a new configuration file...");

                CreateNewConfig("");

                m_logger.Log("New configuration was created.");
                m_logger.Log("Fill out the configuration file and restart the application. Visit the \"Special:Version\" and look for the \"Entry point URLs\" section for additional information on how to fill out the config file.");

                return false;
            }

            try
            {
                config = WikiConfig.Load();
                ValidateConfiguration(config);
            }
            catch (Exception e)
            {
                m_logger.Log($"Error trying to load configuration.", e);
                return false;
            }

            return true;
        }

        private static void ValidateConfiguration(IWikiConfig? config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config), "Configuration file could not be loaded. Try recreating the file.");
            }

            if (!Utils.StringUtils.IsValidUrl(config.WikiDomain))
            {
                throw new InvalidOperationException($"Invalid url found in configuration file for: \"{config.WikiDomain}\"");
            }

            if (string.IsNullOrWhiteSpace(config.ApiPath))
            {
                throw new InvalidDataException("Api path variable is empty.");
            }
        }
    }
}