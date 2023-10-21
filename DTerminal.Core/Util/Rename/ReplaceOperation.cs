using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTerminal.Core.Util.Rename
{
    public class ReplaceOperation : OperationBase
    {
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }

        public override string? GetResult(string? source)
        {
            return source?.Replace(OldValue ?? string.Empty, NewValue ?? string.Empty);
        }
    }
}
