using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDependencyFinder.Logging
{
    internal class ConsoleLogger : ILogger
    {
        private readonly ConsoleColor m_color;

        public ConsoleLogger()
        {
            m_color = Console.ForegroundColor;
        }

        public void Log(string message)
        {
            var type = "INFO";
            Log(message, type, ConsoleColor.Cyan);
        }

        public void LogException(string message, Exception exception)
        {
            var type = "ERROR";
            message += $" {exception.GetType().Name}: {exception.Message}";
            Log(message, type, ConsoleColor.Red);
        }

        private void Log(string message, string type, ConsoleColor typeColour)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");

            Console.Write(timestamp);
            Console.ForegroundColor = typeColour;
            Console.Write($" {type}: ");
            Console.ForegroundColor = m_color;
            Console.WriteLine(message);
        }
    }
}
