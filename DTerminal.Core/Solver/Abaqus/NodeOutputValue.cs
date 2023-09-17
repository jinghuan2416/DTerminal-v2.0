using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTerminal.Core.Solver.Abaqus
{
    /// <summary>
    /// abauqs dat 文件
    /// </summary>
    internal class NodeOutputValue
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public double Value { get; set; }
    }
}
