using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTerminal.Core.Util.Rename
{

    public class SplitOperation : OperationBase
    {
        public string? Separator { get; set; }
        public int Target { get; set; } = 1;
        public override string? GetResult(string? source)
        {
            var cells = source?.Split(Separator);
            if (cells?.Length >= Target && Target > 0)
            {
                return cells[Target - 1];
            }
            else
            {
                return source;
            }
        }
    }

}
