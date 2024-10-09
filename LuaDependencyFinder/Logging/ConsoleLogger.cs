using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDependencyFinder.Logging
{
    internal class ConsoleLogger : ILogger
    {
        private readonly string m_categoryName;

        public ConsoleLogger()
        {
            m_categoryName = "Program";
        }

        public void Log(string message)
            => Log(message, null);

        public void Log(string message, Exception? exception)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff");
            //var logLevelString = logLevel.ToString();

            if (exception != null)
            {
                message += $" Exception: {exception.GetType().Name}: {exception.Message}";
            }

            //Console.WriteLine($"{timestamp} [{logLevelString}] {_categoryName}: {message}");
            Console.WriteLine($"{timestamp} {m_categoryName}: {message}");
        }

        public void BlankLike()
            => Console.WriteLine();
    }
}
