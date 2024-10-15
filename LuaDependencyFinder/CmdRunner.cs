using LuaDependencyFinder.Analysing;
using LuaDependencyFinder.Config;
using LuaDependencyFinder.Logging;
using LuaDependencyFinder.Storage;

namespace LuaDependencyFinder
{
    internal class CmdRunner : IApplicationMode
    {
        private readonly IWikiConfig m_config;
        private readonly ILogger m_logger;
        private readonly IFileRepository m_fileRepository;

        private readonly string m_targetFile;

        public CmdRunner(ApplicationContext context, string file)
        {
            m_config = context.GetService<IWikiConfig>();
            m_fileRepository = context.GetService<IFileRepository>();
            m_logger = context.GetService<ILogger>();
            m_targetFile = file;
        }

        public Task Execute()
        {
            var depFinder = new DepFinder(m_config, m_logger, m_fileRepository);
            var files = m_fileRepository.GetLocalDependencies(new[] { m_targetFile });

            return depFinder.DownloadDependencies(files);
        }
    }
}
