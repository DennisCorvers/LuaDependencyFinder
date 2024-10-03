using System.Reflection.Metadata.Ecma335;

namespace LuaDependencyFinder.Models
{
    internal class AnalyserResult
    {
        public int StartPosition { get; }

        public int Length { get; }

        public int LineNumber { get; }

        public string DependencyName { get; }

        public bool ContainsModulePrefix
            => DependencyName.StartsWith("Module:", StringComparison.OrdinalIgnoreCase);

        public AnalyserResult(int startPosition, int length, int lineNumber, string dependencyName)
        {
            StartPosition = startPosition;
            Length = length;
            LineNumber = lineNumber;
            DependencyName = dependencyName;
        }
    }
}
