using LuaDependencyFinder.Analysing;
using LuaDependencyFinder.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace LuaDependencyFinder
{
    internal class CmdRunner : IApplicationMode
    {
        private readonly IServiceProvider m_serviceProvider;
        private readonly string m_targetFile;

        public CmdRunner(IServiceProvider serviceProvider, string file)
        {
            m_serviceProvider = serviceProvider;
            m_targetFile = file;
        }

        public Task Execute()
        {
            var depFinder = m_serviceProvider.GetRequiredService<DepFinder>();
            var fileRepository = m_serviceProvider.GetRequiredService<IFileRepository>();
            var files = fileRepository.GetLocalDependencies(new[] { m_targetFile });

            return depFinder.DownloadDependencies(files);
        }
    }
}
