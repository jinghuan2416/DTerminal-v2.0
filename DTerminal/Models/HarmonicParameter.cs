using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTerminal.Models
{
    public class HarmonicParameter
    {
        public double Amplitude { get; set; } = 1;

        public double Frequency { get; set; } = 1;

        public double Phase { get; set; } = 0;

        public double Offset { get; set; } = 0;
    }
}
