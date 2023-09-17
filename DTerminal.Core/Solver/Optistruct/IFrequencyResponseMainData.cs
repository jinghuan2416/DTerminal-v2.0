using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTerminal.Core.Solver.Optistruct
{
    public interface IFrequencyResponseMainData
    {
        public double Frequency { get; set; }
        public BaseData? R1 { get; set; }
    }
}
