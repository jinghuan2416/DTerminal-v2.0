using System;
using System.Collections.Generic;
using System.Linq;

namespace DTerminal.Core.Material
{
    public class RambergOsgood
    {
        public double ElasticModulus { get; set; } = 210000;
        public StressStrain P1 { get; set; } = new StressStrain();
        public StressStrain P2 { get; set; } = new StressStrain();

        public StressStrain[] GenerateCurve(double maxStress = 10000)
        {
            double n = Math.Log10(P2.Stress / P1.Stress) / Math.Log10(P2.Strain / P1.Strain);
            double h = P1.Stress / Math.Pow(P1.Strain, n);
            List<StressStrain> ps = new List<StressStrain>();
            for (int stress = 0; stress < maxStress; stress++)
            {
                double ¦Å = stress / ElasticModulus + Math.Pow(stress / h, 1 / n);
                ps.Add(new StressStrain(¦Å, stress));
            }
            return ps.Where(x => x.Strain < 1).ToArray();
        }
        public StressStrain[] GeneratePlasticCurve(double maxStress = 10000, double truncation = 1e-5)
        {
            double n = Math.Log10(P2.Stress / P1.Stress) / Math.Log10(P2.Strain / P1.Strain);
            double h = P1.Stress / Math.Pow(P1.Strain, n);
            List<StressStrain> ps = new List<StressStrain>();
            for (int stress = 1; stress <= maxStress; stress++)
            {
                double ¦Å = stress / ElasticModulus + Math.Pow(stress / h, 1 / n);
                var p = new StressStrain(¦Å, stress);
                ps.Add(p.RemoveElasticity(ElasticModulus));
            }
            var rs = ps.Where(x => x.Strain > truncation && x.Strain < 1).ToList();
            if (rs.Count > 2)
            {
                StressStrain f1 = rs[0];
                StressStrain f2 = rs[1];
                StressStrain f0 = new StressStrain(0, 0);
                double k = (f2.Stress - f1.Stress) / (f2.Strain - f1.Strain);
                f0.Stress = f1.Stress - k * f1.Strain;
                rs.Insert(0, f0);
            }
            return rs.ToArray();
        }
    }
}
