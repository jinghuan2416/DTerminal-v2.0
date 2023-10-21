using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTerminal.Core.Util.Rename
{
    public abstract class OperationBase : IOperation
    {
        public bool IsEnable { get; set; } = true;

        public abstract string? GetResult(string? source);
    }
}
