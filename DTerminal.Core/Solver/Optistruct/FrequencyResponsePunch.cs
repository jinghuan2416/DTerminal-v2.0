using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTerminal.Core.Solver.Optistruct
{
    public class FrequencyResponsePunch
    {
        public string? Title { get; set; }
        public string? SubTitle { get; set; }
        public string? Label { get; set; }
        public string? ResultType { get; set; }
        public string? ResultFormat { get; set; }
        public int SubcaseID { get; set; }
        public int PointID { get; set; }
        public FrequencyResponseData[]? Value { get; set; }
    }
}
