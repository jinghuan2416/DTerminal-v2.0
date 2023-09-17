namespace DTerminal.Core.Solver.CFD
{
    public class PorousHandler
    {

        /// <summary>
        /// 计算Star CCM
        /// </summary>
        /// <param name="p0">一次系数</param>
        /// <param name="p1">二次系数</param>
        /// <param name="length">介质长度</param>
        /// <returns></returns>
        public static StarCCMPorousParameter GetStarCCMPorousParameter(double p0, double p1, double length)
        {
            var StarCCMPorousParameter = new StarCCMPorousParameter()
            {
                Pi = p1 / length,
                Pv = p0 / length
            };
            return StarCCMPorousParameter;
        }

        /// <summary>
        /// 计算Fluent
        /// </summary>
        /// <param name="p0">一次系数</param>
        /// <param name="p1">二次系数</param>
        /// <param name="denstity">密度</param>
        /// <param name="dynamicViscosity">动力粘度</param>
        /// <param name="length">介质长度</param>
        /// <returns></returns>
        public static FluentPorousParameter GetFluentResult(double p0, double p1, double denstity,double dynamicViscosity, double length)
        {
            var fluent = new FluentPorousParameter()
            {
                C2 = 2 * p1 / denstity / length,
                C1 = p0 / dynamicViscosity / length
            };
            return fluent;
        }



    }
}
