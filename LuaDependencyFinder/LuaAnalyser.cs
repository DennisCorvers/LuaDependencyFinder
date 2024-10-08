using LuaDependencyFinder.Config;
using LuaDependencyFinder.Models;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace LuaDependencyFinder
{
    internal class LuaAnalyser : ILuaAnalyser
    {
        private readonly Regex m_requireRegex;

        private const string requirePattern = @"\brequire\s*[\(\s]*['""]([^'""]+)['""]\s*[\)]?";
        private const string loadJsonDataPattern = @"\bmw\.loadJsonData\s*[\(\s]*['""]([^'""]+)['""]\s*[\)]?";

        public LuaAnalyser()
        {
            m_requireRegex = new Regex($@"\b(?:{requirePattern}|{loadJsonDataPattern})");
        }

        public IEnumerable<AnalyserResult> AnalyseLuaFile(string luaCode)
        {
            var result = new List<AnalyserResult>();

            var sr = new StringReader(luaCode);
            var lineNumber = 0;
            string? line;

            for (; (line = sr.ReadLine()) != null; lineNumber++)
            {
                var match = m_requireRegex.Match(line);
                if (match.Success)
                {
                    var requireIndex = match.Index;
                    var beforeRequire = line.Substring(0, requireIndex);

                    if (beforeRequire.Contains("--"))
                        continue;

                    var group = match.Groups[match.Groups.Count - 1];
                    var analyserResult = new AnalyserResult(group.Index, group.Length, lineNumber, group.Value);
                    result.Add(analyserResult);
                }
            }

            return result;
        }

        public WikiPage PatchDependency(WikiPage wikiDependency)
        {
            var analyserResult = AnalyseLuaFile(wikiDependency.Contents)
                .Select(x => x.LineNumber)
                .ToHashSet();

            var result = new StringBuilder(wikiDependency.Contents.Length);

            // No dependencies found to patch, we can just return the page as is.
            if (!analyserResult.Any())
            {
                return wikiDependency;
            }

            Utils.StringUtils.SpanToLines(wikiDependency.Contents, (line, i) =>
            {
                if (analyserResult.Contains(i))
                {
                    var sline = new string(line).Replace("Module:", "");
                    result.AppendLine(sline);
                }
                else
                {
                    result.Append(line);
                    result.AppendLine();
                }
            });

            return new WikiPage(wikiDependency.Page, wikiDependency.TimeStamp, result.ToString());
        }
    }
}
