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
    internal class IncrementSummary
    {
        public double Increment { get; set; }
        public List<NodeOutputValue> NodeValues { get; set; } = new List<NodeOutputValue>();
    }
}
