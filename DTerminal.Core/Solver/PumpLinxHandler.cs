using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTerminal.Core.Solver
{
    public static class PumpLinxHandler
    {
        /// <summary>
        /// 提取结果，并进行平均处理
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="percentage">有效数据百分比</param>
        /// <returns></returns>
        public static Dictionary<string, double> ExtractResultByAverage(string filePath, double percentage = 0.9, char split = '\t')
        {
            var lines = System.IO.File.ReadAllLines(filePath).Where(x => !string.IsNullOrEmpty(x)).ToArray();
            var title = lines.First().Split(split);
            double[] datas = new double[title.Length];
            int star = Convert.ToInt32(lines.Length * percentage);
            int end = lines.Length;
            for (int row = star; row < end; row++)
            {
                var cells = lines[row].Split(split).Select(x => Convert.ToDouble(x)).ToArray();
                for (int col = 0; col < datas.Length; col++)
                {
                    datas[col] += cells[col];
                }
            }
            Dictionary<string, double> ds = new Dictionary<string, double>();
            for (int col = 0; col < datas.Length; col++)
            {
                datas[col] = datas[col] / (end - star);
                ds.Add(title[col], datas[col]);
            }
            return ds;
        }
    }
}
