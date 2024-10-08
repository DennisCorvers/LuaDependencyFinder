using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDependencyFinder.WikiAPI.Models
{
    internal static class Mapping
    {
        internal static string WikiPageToPath(string wikiPage)
        {
            int colonIndex = wikiPage.IndexOf(':');
            if (colonIndex != -1)
            {
                wikiPage = wikiPage.Substring(colonIndex + 1);
            }

            return wikiPage.Replace('/', '\\') + ".lua";
        }

        internal static string PathToWikiPage(string path, string prefix = "Module:")
        {
            path = path.Replace('\\', '/');
            return prefix + Path.ChangeExtension(path, null);
        }
    }
}
