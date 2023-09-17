using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTerminal.Core
{
    public static class Extension
    {
        /// <summary>
        /// 将源对象的所有属性，复制到目标对象
        /// </summary>
        /// <param name="source">源对象</param>
        /// <param name="target">目标对象</param>
        public static void CopyPropertyTo(this object source, object target)
        {
            foreach (var item in source.GetType().GetProperties())
            {
                if (target.GetType().GetProperty(item.Name) is System.Reflection.PropertyInfo propertyInfo && propertyInfo.CanWrite == true)
                {
                    propertyInfo.SetValue(target, item.GetValue(source), null);
                }
            }
        }

        /// <summary>
        /// 将源对象的所有属性，复制到目标对象
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <param name="source">源对象</param>
        public static void CopyPropertyFrom(this object target, object source)
        {
            foreach (var item in source.GetType().GetProperties())
            {
                if (target.GetType().GetProperty(item.Name) is System.Reflection.PropertyInfo propertyInfo && propertyInfo.CanWrite == true)
                {
                    propertyInfo.SetValue(target, item.GetValue(source), null);
                }
            }
        }

        public static (double, double)[] ConvertTwoDouble(this IEnumerable<string> lines)
        {
            List<(double, double)> ds = new();
            foreach (var line in lines)
            {
                var cells = line.Split('\t');
                if (cells.Length != 2) { cells = line.Split(','); }
                if (cells.Length != 2) { cells = line.Split(' '); }
                if (cells.Length != 2) { continue; }
                if (double.TryParse(cells[0], out double x) && double.TryParse(cells[1], out double y))
                {
                    ds.Add((x, y));
                }
            }
            return ds.ToArray();
        }
        public static double[] ConvertOneDouble(this IEnumerable<string> lines)
        {
            List<double> ds = new();
            foreach (var line in lines)
            {
                if (double.TryParse(line, out double x))
                {
                    ds.Add(x);
                }
            }
            return ds.ToArray();
        }


        public static string[] AutoSplit(this string line, int numberOfTargetColumns)
        {
            var cells = line.Split('\t');
            if (cells.Length != numberOfTargetColumns) { cells = line.Split(','); }
            if (cells.Length != numberOfTargetColumns) { cells = line.Split(' '); }
            return cells.ToArray();
        }

        public static string[][] AutoSplit(this IEnumerable<string> lines, int numberOfTargetColumns)
        {
            List<string[]> ds = new();
            foreach (var line in lines)
            {
                ds.Add(line.AutoSplit(numberOfTargetColumns));
            }
            return ds.ToArray();
        }


    }
}
