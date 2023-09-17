using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTerminal.Core.Solver.Optistruct
{
    public class BearingLoadModel
    {
        public bool IsEnable { get; set; } = true;
        public string CaseName { get; set; } = "default";
        public string Note { get; set; } = string.Empty;
        public double Fx { get; set; }
        public double Fy { get; set; }
        public double Fz { get; set; }
        public double Mx { get; set; }
        public double My { get; set; }
        public double Mz { get; set; }

        public override string ToString()
        {
            return string.Join("\t", CaseName, Fx, Fy, Fz, Mx, My, Mz, Note);
        }

        public static string GetTitleLine()
        {
            return string.Join("\t", "工况", "Fx", "Fy", "Fz", "Mx", "My", "Mz", "Note");
        }
    }
}
