using System.Reflection.Metadata.Ecma335;

namespace LuaDependencyFinder.Models
{
    internal record AnalyserResult(int StartPosition, int Length, int LineNumber, string DependencyName)
    {
        public string ModuleName
        {
            get
            {
                if (DependencyName.StartsWith("Module:", StringComparison.OrdinalIgnoreCase))
                {
                    return DependencyName;
                }
                return "Module:" + DependencyName;
            }
        }
    }
}
