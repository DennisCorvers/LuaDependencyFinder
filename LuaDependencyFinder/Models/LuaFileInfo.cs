using LuaDependencyFinder.WikiAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDependencyFinder.Models
{
    internal class LuaFileInfo
    {
        public string RelativePath { get; }

        public string Name { get; }

        public string ArticlePath { get; }

        public LuaFileInfo(string relativePath, string name)
        {
            RelativePath = relativePath;
            Name = name;

            ArticlePath = Mapping.PathToWikiPage(relativePath);
        }
    }

}
