using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTerminal.Core.Solver
{
    public static class Extension
    {
        public static string FormatString<T1, T2>(this Dictionary<T1, T2> results) where T1 : notnull
        {
            StringBuilder sb = new StringBuilder();
            foreach (var r in results)
            {
                sb.AppendLine($"{r.Key}:\t{r.Value}");
            }
            return sb.ToString().Trim();
        }
    }
}
