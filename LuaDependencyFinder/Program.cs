using LuaDependencyFinder.Analysing;
using LuaDependencyFinder.Config;
using LuaDependencyFinder.Logging;
using LuaDependencyFinder.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace LuaDependencyFinder
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            ServiceProvider? serviceProvider;

            try
            {
                serviceProvider = ConfigureServices(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
                return;
            }

            try
            {
                // Try to fetch the config to verify that it is correct.
                _ = serviceProvider.GetRequiredService<IWikiConfig>();

                var applicationMode = serviceProvider.GetRequiredService<IApplicationMode>();
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

        private static ServiceProvider ConfigureServices(string[] args)
        {
            var services = new ServiceCollection()
                .AddSingleton<ILogger, ConsoleLogger>()
                .AddSingleton<ConfigLoader>()
                .AddTransient<DepFinder>()
                .AddTransient<IWikiConfig>(serviceProvider =>
                {
                    var configLoader = serviceProvider.GetRequiredService<ConfigLoader>();
                    if (!configLoader.TryLoadConfig(out IWikiConfig? config) || config == null)
                    {
                        throw new Exception("Unable to load configuration.");
                    }

                    return config;
                });


            if (args.Length > 0)
            {
                // If a file argument is passed, use the headless application mode.
                var file = args[0];
                var root = Utils.StringUtils.GetFileRoot(file);

                services.AddSingleton<IFileRepository>(serviceProvider =>
                    new FileRepository(
                        serviceProvider.GetRequiredService<IWikiConfig>(),
                        serviceProvider.GetRequiredService<ILogger>(),
                        root));

                services.AddSingleton<IApplicationMode>(serviceProvider =>
                    new CmdRunner(serviceProvider, file));
            }
            else
            {
                services.AddSingleton<IFileRepository, FileRepository>();
                services.AddSingleton<IApplicationMode, CommandRunner>();
            }

            return services.BuildServiceProvider();
        }
    }
}

