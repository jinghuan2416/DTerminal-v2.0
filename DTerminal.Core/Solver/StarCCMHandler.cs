using System.Text;

namespace DTerminal.Core.Solver
{
    public static class StarCCMHandler
    {
        public static Dictionary<string, double> PlotResultAverage(string filePath, double per = 0.9)
        {
            var lines = System.IO.File.ReadAllLines(filePath);
            var title = lines.First().Replace("\"", string.Empty).Split(",");
            double[] datas = new double[title.Length];
            int star = Convert.ToInt32(lines.Length * per);
            int end = lines.Length;
            for (int row = star; row < end; row++)
            {
                var cells = lines[row].Split(",").Select(x => Convert.ToDouble(x)).ToArray();
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