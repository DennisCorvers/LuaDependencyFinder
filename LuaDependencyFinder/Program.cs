using LuaDependencyFinder.Analysing;
using LuaDependencyFinder.Config;
using LuaDependencyFinder.Logging;
using LuaDependencyFinder.Storage;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

namespace LuaDependencyFinder
{
    internal class Program
    {
        private static readonly ApplicationContext m_context = new ApplicationContext();

        static async Task Main(string[] args)
        {
            try
            {
                SetupServices(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            try
            {
                IApplicationMode applicationMode = m_context.GetService<IApplicationMode>();
                await applicationMode.Execute();
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error has occured: {e.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("Exiting application...");

            await Task.Delay(1000);
        }

        // Some cheap dependency setup.
        private static void SetupServices(string[] args)
        {
            ILogger logger = new ConsoleLogger();
            var configLoader = new ConfigLoader(logger);

            if (!configLoader.TryLoadConfig(out IWikiConfig? config) || config == null)
            {
                throw new Exception("Unable to load configuration.");
            }

            m_context.AddService<ILogger>(logger);
            m_context.AddService<IWikiConfig>(config);

            if (args.Length > 0)
            {
                var file = args[0];
                var root = Utils.StringUtils.GetFileRoot(file);

                m_context.AddService<IFileRepository>(new FileRepository(config, logger, root));
                m_context.AddService<IApplicationMode>(new CmdRunner(m_context, file));
            }
            else
            {
                m_context.AddService<IFileRepository>(new FileRepository(config, logger));
                m_context.AddService<IApplicationMode>(new CommandRunner(m_context));
            }
        }
    }
}

