
namespace DTerminal.Core
{
    public static class MathExtension
    {
        /// <summary>
        /// 计算指定角度的正切
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static double Tan(this double angle)
        {
            return Math.Tan(angle.ToRadians());
        }

        /// <summary>
        /// 计算指定角度的正弦
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static double Sin(this double angle)
        {
            return Math.Sin(angle.ToRadians());
        }
        /// <summary>
        /// 计算指定角度的余弦
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static double Cos(this double angle)
        {
            return Math.Cos(angle.ToRadians());
        }

        /// <summary>
        /// 将角度转为弧度
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static double ToRadians(this double angle)
        {
            return Math.PI / 180 * angle;
        }

        /// <summary>
        /// 将弧度转为角度
        /// </summary>
        /// <param name="radian"></param>
        /// <returns></returns>
        public static double ToAngle(this double radian)
        {
            return 180 / Math.PI * radian;
        }

        /// <summary>
        /// 计算指定数据的幂次方
        /// </summary>
        /// <param name="d"></param>
        /// <param name="m">幂次方</param>
        /// <returns></returns>
        public static double Pow(this double d, double m)
        {
            return Math.Pow(d, m);
        }

        public static double ToMultiply(this double x, double y)
        {
            return x * y;
        }
        public static double ToAddition(this double x, double y)
        {
            return x + y;
        }

        /// <summary>
        /// 根据指定的数据点，拟合曲线
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public static (double p0, double p1) FitCurve(this IEnumerable<DPoint> datas, double p0, double p1, double tolerance, int maxIterations)
        {
            double[] xs = datas.Where(x => x.X > 0).Select(x => x.X).ToArray();
            double[] ys = datas.Where(x => x.Y > 0).Select(x => x.Y).ToArray();
            var (P0, P1) = MathNet.Numerics.Fit.Curve(xs, ys, (p0, p1, x) => p1 * x * x + p0 * x, p0, p1, tolerance, maxIterations);
            return (P0, P1);
        }

        /// <summary>
        /// 离散傅里叶变换
        /// </summary>
        /// <param name="source"></param>
        /// <param name="freq">采样频率</param>
        /// <returns></returns>
        public static DPoint[] DFT(this DPoint[] source, double freq)
        {
            int count = source.Length;

            double[] ts = source.Select(p => p.X).ToArray();
            double[] vs = source.Select(p => p.Y).ToArray();

            double[] xs = new double[count / 2];

            for (int i = 0; i < xs.Length; i++)
            {
                xs[i] = i * freq / count;
            }

            double[] ys = FFTlib.DFT.DFTv2(vs.ToArray()).Select(x => x.Magnitude).ToArray();
            ys = ys.Take(count / 2).Select(y => y * 2 / count).ToArray();

            DPoint[] rs = new DPoint[xs.Length];

            for (int i = 0; i < xs.Length; i++)
            {
                rs[i] = new DPoint(xs[i], ys[i]);
            }

            return rs;
        }

        /// <summary>
        /// 快速傅里叶变换
        /// </summary>
        /// <param name="source"></param>
        /// <param name="freq">采样频率</param>
        /// <returns></returns>
        public static DPoint[] FFT(this DPoint[] source, double freq)
        {
            int log = (int)Math.Log(source.Length, 2);
            int count = (int)Math.Pow(2, log);
            source = source.Take(count).ToArray();

            double[] xs = new double[count / 2];
            for (int i = 0; i < xs.Length; i++)
            {
                xs[i] = i * freq / count;
            }

            double[] ys = source.Select(p => p.Y).ToArray();
            ys = FFTlib.FFT.FFTN(ys).Select(p => p.Magnitude * 2.0 / count).Take(count / 2).ToArray();
            ys[0] = ys[0] / 2.0;

            DPoint[] rs = new DPoint[xs.Length];
            for (int i = 0; i < xs.Length; i++)
            {
                rs[i] = new DPoint(xs[i], ys[i]);
            }
            return rs;
        }
    }
}
