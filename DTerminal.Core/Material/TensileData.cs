using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTerminal.Core.Material
{
    public struct TensileData
    {
        public double Force { get; set; }
        public double Displacement { get; set; }
        public StressStrain Engineering { get; set; }
        public StressStrain True { get; set; }
    }
}
