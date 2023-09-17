using DTerminal.Core.Solver;
using DTerminal.Core.Solver.Optistruct;
using DTerminal.Models;
using DTerminal.Views;
using NPOI.XSSF.UserModel;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DTerminal
{
    internal static class Extension
    {
        public static NavMenuItem[] GetNavMenuItems(this Assembly assembly)
        {
            List<NavMenuItem> items = new();
            var tps = assembly.GetTypes().
                Where(x => x.GetCustomAttribute<ViewAttribute>() is not null).ToArray();
            foreach (var t in tps)
            {
                if (t.GetCustomAttribute<ViewAttribute>() is ViewAttribute viewAttribute)
                {
                    items.Add(new NavMenuItem(viewAttribute.Title, viewAttribute.Order, t));
                }
            }
            return items.OrderBy(x => x.Order).ThenBy(x => x.Title).ToArray();
        }

        /// <summary>
        /// 根据指定的PunchDrawParameter，创建一页Sheet
        /// </summary>
        /// <param name="workbook"></param>
        /// <param name="parameter"></param>
        public static void CreateSheet(this XSSFWorkbook workbook, PunchDrawParameter parameter)
        {
            var st = workbook.CreateSheet(parameter.FrequencyResponsePunch.Label + "-" + parameter.FrequencyResponsePunch.ResultType);
            var row = st.CreateRow(0);
            row.CreateCell(0).SetCellValue("频率");
            row.CreateCell(1).SetCellValue("X");
            row.CreateCell(2).SetCellValue("Y");
            row.CreateCell(3).SetCellValue("Z");
            row.CreateCell(4).SetCellValue("DynamicStiffness X");
            row.CreateCell(5).SetCellValue("DynamicStiffness Y");
            row.CreateCell(6).SetCellValue("DynamicStiffness Z");
            if (parameter.FrequencyResponsePunch.Value is FrequencyResponseData[] datas)
            {
                var ds = OptistructHandler.ConvertToDynamicStiffness(parameter.FrequencyResponsePunch);
                for (var i = 0; i < datas.Length; i++)
                {
                    row = st.CreateRow(i + 1);
                    row.CreateCell(0).SetCellValue(datas[i].Frequency);
                    row.CreateCell(1).SetCellValue((datas[i].R1?.X).GetValueOrDefault(0));
                    row.CreateCell(2).SetCellValue((datas[i].R1?.Y).GetValueOrDefault(0));
                    row.CreateCell(3).SetCellValue((datas[i].R1?.Z).GetValueOrDefault(0));
                    row.CreateCell(4).SetCellValue((ds[i].R1?.X).GetValueOrDefault(0));
                    row.CreateCell(5).SetCellValue((ds[i].R1?.Y).GetValueOrDefault(0));
                    row.CreateCell(6).SetCellValue((ds[i].R1?.Z).GetValueOrDefault(0));
                }
            }
        }
    }
}
