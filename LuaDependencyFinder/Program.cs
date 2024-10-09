using LuaDependencyFinder.Analysing;
using LuaDependencyFinder.Config;
using LuaDependencyFinder.Logging;
using System.Security.Cryptography.X509Certificates;

namespace LuaDependencyFinder
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            ILogger logger = new ConsoleLogger();
            var configLoader = new ConfigLoader(logger);

            if (!configLoader.TryLoadConfig(out IWikiConfig? config))
            {
                return;
            }

            logger.Log($"Loaded configuration for: {config!.WikiDomain}");

            try
            {
                var program = new Program(logger, config);
                await program.Execute();
            }
            catch (Exception e)
            {
                logger.Log("Unhandled exception.", e);
            }

            logger.BlankLike();
            logger.Log("Exiting application...");

            await Task.Delay(1000);
        }

        private readonly CommandManager m_commandManager;
        private readonly ILogger m_logger;
        private IWikiConfig m_config;
        private bool m_isRunning;

        public Program(ILogger logger, IWikiConfig config)
        {
            m_commandManager = new CommandManager();
            SetupCommands();
            m_logger = logger;
            m_config = config;
            m_isRunning = true;
        }

        private void SetupCommands()
        {
            m_commandManager.AddCommand(new Command("Quit", Quit, "Exit the application."));
            m_commandManager.AddCommand(new Command("Reload", ReloadConfig, "Reload the configuration."));
            m_commandManager.AddCommand(new Command("Patch", PatchLocalFiles, "Patch all local files and bring them up to date."));
            m_commandManager.AddCommand(new Command("Download", DownloadDependencies, "Download all missing dependencies required by local files."));
        }

        private async Task Execute()
        {
            Console.WriteLine();

            ListCommands();

            while (m_isRunning)
            {
                Console.Write("Enter action: ");
                var userCommand = Console.ReadLine() ?? string.Empty;
                if (!m_commandManager.TryGetAction(userCommand, out var command))
                {
                    Console.WriteLine("Unknown command");
                    continue;
                }

                await command.Invoke();

                Console.WriteLine();

                if (m_isRunning)
                {
                    ListCommands();
                }
            }
        }

        private void ReloadConfig()
        {
            try
            {
                var newConfig = WikiConfig.Load();
                if (newConfig != null)
                {
                    m_config = newConfig;
                    m_logger.Log("Reloaded configuration file.");
                }
            }
            catch (Exception e)
            {
                m_logger.Log("Unable to reload configuration file.", e);
            }
        }

        private async Task PatchLocalFiles()
        {
            var finder = new DepFinder(m_config, m_logger);
            await finder.PatchFiles();
        }

        private async Task DownloadDependencies()
        {
            var finder = new DepFinder(m_config, m_logger);
            await finder.DownloadDependencies();
        }

        private void Quit()
        {
            m_isRunning = false;
        }

        private void ListCommands()
        {
            foreach (var cmd in m_commandManager.Commands)
            {
                var commandText = string.Join(", ", cmd.Item2.OrderBy(x => x.Length));
                Console.WriteLine(string.Format("{0,-" + (m_commandManager.LongestCommand + 6) + "}: {1}", commandText, cmd.Item1.Details));
            }
        }
    }
}

