using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTerminal.Core.Solver.Optistruct
{
    public class FrequencyResponseData: IFrequencyResponseMainData
    {
        public double Frequency { get; set; }
        public BaseData? R1 { get; set; }
        public BaseData? R2 { get; set; }
        public BaseData? R3 { get; set; }
        public BaseData? R4 { get; set; }
    }
}
