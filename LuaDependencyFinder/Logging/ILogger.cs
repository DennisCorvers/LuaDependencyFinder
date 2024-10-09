using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDependencyFinder.Logging
{
    public interface ILogger
    {
        void BlankLike();
        void Log(string message);
        void Log(string message, Exception? exception);
    }
}
