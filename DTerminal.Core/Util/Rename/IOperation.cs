using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTerminal.Core.Util.Rename
{
    public interface IOperation
    {
        bool IsEnable { get; set; }
        string? GetResult(string? source);
    }
}
