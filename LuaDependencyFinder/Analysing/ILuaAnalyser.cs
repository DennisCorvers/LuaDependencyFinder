using LuaDependencyFinder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDependencyFinder.Analysing
{
    internal interface ILuaAnalyser
    {
        IEnumerable<AnalyserResult> AnalyseLuaFile(string luaCode);

        WikiPage PatchDependency(WikiPage wikiDependency);
    }
}
