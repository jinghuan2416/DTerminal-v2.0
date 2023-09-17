using System;

namespace DTerminal.Core.Material
{
    public struct StressStrain
    {
        public StressStrain(double strain, double stress)
        {
            Strain = strain;
            Stress = stress;
        }

        public double Strain { get; set; }
        public double Stress { get; set; }

        public override string ToString()
        {
            return string.Join(",", Strain, Stress);
        }
        /// <summary>
        /// 从工程应力应变转换为真实应力应变
        /// </summary>
        /// <returns></returns>
        public StressStrain ConvertToTrueIfEngineering()
        {
            var ss = new StressStrain
            {
                Strain = Math.Log(1 + Strain),
                Stress = Stress * (1 + Strain)
            };
            return ss;
        }

        /// <summary>
        /// 去除弹性
        /// </summary>
        /// <param name="elasticModulus">弹性模量</param>
        /// <returns></returns>
        public StressStrain RemoveElasticity(double elasticModulus)
        {
            double εe = Stress / elasticModulus;
            double εp = Strain - εe;
            return new StressStrain(εp, Stress);
        }
    }


}
