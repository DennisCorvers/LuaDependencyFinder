using LuaDependencyFinder.WikiAPI;

namespace LuaDependencyFinder
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var service = new MWService("https://wiki.melvoridle.com/");
            var pages = new List<string>()
            {
                "Module:Icons",
            };
            var result = await service.GetRevisionHistory(pages);
            Console.WriteLine("Hello, World!");
        }
    }
}

