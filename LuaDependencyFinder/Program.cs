using LuaDependencyFinder.Config;
using LuaDependencyFinder.Logging;

namespace LuaDependencyFinder
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            await Run();

            Console.WriteLine();
            Console.WriteLine("Exiting application...");
            Console.ReadLine();
        }

        static async Task Run()
        {
            ILogger logger = new ConsoleLogger();
            var configLoader = new ConfigLoader(logger);
            IWikiConfig? config;

            try
            {
                if (!configLoader.TryLoadConfig(out config))
                {
                    return;
                }
            }
            catch (FileNotFoundException)
            {
                configLoader.CreateNewConfig("");

                Console.WriteLine("New configuration was created.");
                Console.WriteLine("Fill out the configuration file and restart the application. Visit the \"Special:Version\" and look for the \"Entry point URLs\" section for additional information on how to fill out the config file.");

                return;
            }

            var finder = new DepFinder(config!, logger);
            await finder.DownloadDependencies();    
            //await finder.PatchFiles();
        }
    }
}

