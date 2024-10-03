using LuaDependencyFinder.Config;
using LuaDependencyFinder.Models;
using System.Text.RegularExpressions;

namespace LuaDependencyFinder
{
    internal class LuaAnalyser
    {
        private readonly Regex m_requireRegex;

        public LuaAnalyser()
        {
            m_requireRegex = new Regex(@"\brequire\s*[\(\s]*['""]([^'""]+)['""]\s*[\)]?");
        }

        public async Task<IEnumerable<AnalyserResult>> AnalyseLuaFile(LuaFileInfo fileInfo)
        {
            using var reader = new StreamReader(fileInfo.RelativePath);
            return AnalyseLuaFile(await reader.ReadToEndAsync());
        }

        public IEnumerable<AnalyserResult> AnalyseLuaFile(string luaCode)
        {
            var result = new List<AnalyserResult>();

            var sr = new StringReader(luaCode);
            var lineNumber = 0;
            var line = default(string);
            for (; (line = sr.ReadLine()) != null; lineNumber++)
            {
                var match = m_requireRegex.Match(line);
                if (match.Success)
                {
                    var requireIndex = match.Index;
                    var beforeRequire = line.Substring(0, requireIndex);

                    if (beforeRequire.Contains("--"))
                        continue;

                    var group = match.Groups[1];
                    var analyserResult = new AnalyserResult(group.Index, group.Length, lineNumber, group.Value);
                    result.Add(analyserResult);
                }
            }

            return result;
        }
    }
}
