using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTerminal.Core.Util.Rename
{
    public class SubstringOperation : OperationBase
    {
        public int Star { get; set; } = 0;
        public int Length { get; set; } = 1;

        public override string? GetResult(string? source)
        {
            if (Length > 0)
            {
                return source?.Substring(Star, Length);
            }
            else
            {
                return source?.Substring(Star);
            }

        }
    }
}
