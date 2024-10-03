using LuaDependencyFinder.WikiAPI;

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
