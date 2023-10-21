using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTerminal.Core.Util.Rename
{
    public class AddOperation : OperationBase
    {
        public string? Content { get; set; }
        public int Star { get; set; }

        public override string? GetResult(string? source)
        {
            if (Star <= 0)
            {
                return Content + source;
            }
            else if (Star > 0 && Star < source?.Length)
            {
                return source?.Insert(Star, Content ?? string.Empty);
            }
            else if (Star >= source?.Length)
            {
                return source + Content;
            }
            else
            {
                return Content + source;
            }
        }
    }
}
