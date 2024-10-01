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

        public bool TryLoadConfig(out WikiConfig? config)
        {
            config = null;
            try
            {
                config = WikiConfig.Load();
                if (config == null)
                {
                    m_logger.Log("Unable to load config.");
                    return false;
                }
                if (!IsValidUrl(config.WikiDomain))
                {
                    m_logger.Log($"Invalid url found in configuration: \"{config.WikiDomain}\"");
                    m_logger.Log("Edit the configuration so that the url is a valid mediawiki address.");
                    return false;
                }

                return true;
            }
            catch (FileNotFoundException)
            {
                throw;
            }
            catch (Exception e)
            {
                m_logger.Log("Unable to load configuration.");
                m_logger.Log(e.Message);
                return false;
            }
        }

        public void CreateConfig()
        {
            var config = WikiConfig.Create("");

            try
            {
                config.Save();
            }
            catch (Exception e)
            {
                m_logger.Log("Unable to save new configuration.");
                m_logger.Log(e.Message);
            }
        }

        public static bool IsValidUrl(string url)
        {
            try
            {
                Uri uri = new(url);
                return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
            }
            catch (UriFormatException)
            {
                return false;
            }
        }
    }
}
