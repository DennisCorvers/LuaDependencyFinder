using LuaDependencyFinder.Analysing;
using LuaDependencyFinder.Config;
using LuaDependencyFinder.Logging;
using LuaDependencyFinder.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace LuaDependencyFinder
{
    internal class CommandRunner : IApplicationMode
    {
        private readonly CommandManager m_commandManager;
        private readonly IServiceProvider m_serviceProvider;
        private bool m_isRunning;

        public CommandRunner(IServiceProvider serviceProvider)
        {
            m_commandManager = new CommandManager();
            SetupCommands();
            m_isRunning = true;

            m_serviceProvider = serviceProvider;
        }

        private void SetupCommands()
        {
            m_commandManager.AddCommand(new Command("Quit", Quit, "Exit the application."));
            m_commandManager.AddCommand(new Command("Patch", PatchLocalFiles, "Patch all local files and bring them up to date."));
            m_commandManager.AddCommand(new Command("Download", DownloadDependencies, "Download all missing dependencies required by local files."));
        }

        private async Task PatchLocalFiles()
        {
            var depFinder = m_serviceProvider.GetRequiredService<DepFinder>();
            await depFinder.PatchFiles();
        }

        private async Task DownloadDependencies()
        {
            var depFinder = m_serviceProvider.GetRequiredService<DepFinder>();
            await depFinder.DownloadDependencies();
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

        public async Task Execute()
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
    }
}
