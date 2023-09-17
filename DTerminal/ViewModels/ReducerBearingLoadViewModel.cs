using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HtmlAgilityPack;
using System.Windows;
using DTerminal.Models;
using DTerminal.Core;
using DTerminal.Core.Solver.Optistruct;

namespace DTerminal.ViewModels
{
    internal partial class ReducerBearingLoadViewModel : ObservableObject
    {
        public int SelectedLoadAreaIndex1 { get; set; }
        public int SelectedLoadAreaIndex2 { get; set; }
        public int SelectedLoadAreaIndex3 { get; set; }
        public int SelectedLoadAreaIndex4 { get; set; }
        public int SelectedLoadAreaIndex5 { get; set; }
        public int SelectedLoadAreaIndex6 { get; set; }

        public bool SelectedLoadAreaCheck(out string mess)
        {
            mess = string.Empty;
            int[] line = { SelectedLoadAreaIndex1, SelectedLoadAreaIndex2, SelectedLoadAreaIndex3, SelectedLoadAreaIndex4, SelectedLoadAreaIndex5, SelectedLoadAreaIndex6 };
            if (line.Distinct().Count() != 6) { mess = "载荷谱排序出现重复ID"; }
            if (line.Distinct().Sum() != 15) { mess = "载荷谱排序ID不符合规定"; }
            if (line.Distinct().Max() != 5) { mess = "载荷谱排序ID不符合规定"; }
            if (line.Distinct().Min() != 0) { mess = "载荷谱排序ID不符合规定"; }
            if (string.IsNullOrEmpty(mess)) { return true; } else { return false; }
        }

        private double scaleF = 1;
        private double scaleM = 1000;
        private BearingLoadModel? selectLoadModel;
        private BearingLoadModel[] bearingLoadModels = { };

        public double ScaleF { get => scaleF; set => this.SetProperty(ref scaleF,value); }
        public double ScaleM { get => scaleM; set => this.SetProperty(ref scaleM, value); }
        public BearingLoadModel? SelectLoadModel { get => selectLoadModel; set => this.SetProperty(ref selectLoadModel, value); }
        public BearingLoadModel[] BearingLoadModels { get => bearingLoadModels; set => this.SetProperty(ref bearingLoadModels, value); }
        public int[] LoadAreas { get; } = { 1, 2, 3, 4, 5, 6 };


        /// <summary>
        /// 从指定html文件读取载荷集合
        /// </summary>
        /// <param name="content">html文件路径</param>
        /// <returns></returns>
        protected IEnumerable<BearingLoadModel> ReadLoadFromHtml(string content)
        {
            List<BearingLoadModel> list = new List<BearingLoadModel>();
            content = content.Replace("\r\n", string.Empty).ToLower();
            MatchCollection tables = Regex.Matches(content, @"<table.*?>[\s\S]*?</table>");
            foreach (Match table in tables.Cast<Match>())
            {
                string temp = Regex.Match(table.Value, @"<caption.*?>[\s\S]*?</caption>").Value;
                string caption = Regex.Replace(temp, "[(<caption.*?>)(</caption>)]", string.Empty);
                MatchCollection trs = Regex.Matches(table.Value, @"<tr.*?>[\s\S]*?</tr>");
                foreach (Match tr in trs)
                {
                    MatchCollection tds = Regex.Matches(tr.Value, @"<td.*?>[\s\S]*?</td>");
                    if (tds.Count == 13)
                    {
                        MatchCollection divs = Regex.Matches(tr.Value, @"<div.*?>[\s\S]*?</div>");

                        string note = divs[0].Value;
                        note = Regex.Replace(note, "(<div.*?>)|(</div>)", string.Empty);
                        BearingLoadModel m = new BearingLoadModel()
                        {
                            CaseName = caption.Split(':').Last(),
                            IsEnable = true,
                            Note = Regex.Replace(divs[0].Value, "(<div.*?>)|(</div>)", string.Empty),
                            Fx = double.Parse(Regex.Replace(divs[7].Value, "(<div.*?>)|(</div>)", string.Empty)),
                            Fy = double.Parse(Regex.Replace(divs[8].Value, "(<div.*?>)|(</div>)", string.Empty)),
                            Fz = double.Parse(Regex.Replace(divs[9].Value, "(<div.*?>)|(</div>)", string.Empty)),
                            Mx = double.Parse(Regex.Replace(divs[10].Value, "(<div.*?>)|(</div>)", string.Empty)),
                            My = double.Parse(Regex.Replace(divs[11].Value, "(<div.*?>)|(</div>)", string.Empty)),
                            Mz = double.Parse(Regex.Replace(divs[12].Value, "(<div.*?>)|(</div>)", string.Empty)),
                        };
                        list.Add(m);
                    }
                }
            }
            return list.ToArray();
        }

        /// <summary>
        /// 从指定CSV文件读取载荷集合
        /// </summary>
        /// <param name="fileName">CSV文件路径</param>
        /// <returns></returns>
        protected IEnumerable<BearingLoadModel> ReadLoadFromCSV(string fileName)
        {
            List<BearingLoadModel> list = new List<BearingLoadModel>();
            var lines = System.IO.File.ReadAllLines(fileName).Where(l => l.Trim().Length > 0).ToArray();
            for (int i = 1; i < lines.Length; i++)
            {
                string[] cells = lines[i].Split(',');
                BearingLoadModel model = new BearingLoadModel()
                {
                    IsEnable = true,
                    CaseName = cells[0],
                    Fx = double.Parse(cells[1]),
                    Fy = double.Parse(cells[2]),
                    Fz = double.Parse(cells[3]),
                    Mx = double.Parse(cells[4]),
                    My = double.Parse(cells[5]),
                    Mz = double.Parse(cells[6]),
                    Note = cells[7]
                };
                list.Add(model);
            }
            return list.ToArray();
        }



        [RelayCommand]
        private void ReadHtml()
        {
            var op = new Microsoft.Win32.OpenFileDialog();
            if (op.ShowDialog() == true)
            {
                List<BearingLoadModel> bearingLoadModels = new List<BearingLoadModel>();
                HtmlDocument document = new HtmlDocument();
                document.Load(op.FileName);
                var nodes = document.DocumentNode.SelectNodes("//table");
                if (nodes is not null)
                {
                    foreach (var node in nodes)
                    {
                        var cap = node.SelectSingleNode("caption");
                        var trs = node.SelectNodes("tr");
                        foreach (var tr in trs)
                        {
                            var tds = tr.SelectNodes("td");
                            if (tds is not null && tds.Count == 13)
                            {
                                var m = new BearingLoadModel
                                {
                                    Fx = double.Parse(tds[7].InnerText),
                                    Fy = double.Parse(tds[8].InnerText),
                                    Fz = double.Parse(tds[9].InnerText),
                                    Mx = double.Parse(tds[10].InnerText),
                                    My = double.Parse(tds[11].InnerText),
                                    Mz = double.Parse(tds[12].InnerText),
                                    Note = tds[0].InnerText,
                                    CaseName = "LoadStep_" + System.Text.RegularExpressions.Regex.Replace(cap.InnerText, @"[\.\(\)\s\:\[\]]", "_")
                                };
                                bearingLoadModels.Add(m);
                            }
                        }
                    }
                    this.BearingLoadModels = bearingLoadModels.ToArray();
                }
            }
        }

        [RelayCommand]
        private void PasteFromClipboard()
        {
            var content = Clipboard.GetText();
            if (!string.IsNullOrEmpty(content))
            {
                List<BearingLoadModel> list = new List<BearingLoadModel>();
                var lines = content.Split("\r\n").Where(l => l.Trim().Length > 0).ToArray();
                for (int i = 1; i < lines.Length; i++)
                {
                    string[] cells = lines[i].AutoSplit(8);
                    if (cells.Length == 8)
                    {
                        BearingLoadModel model = new BearingLoadModel()
                        {
                            CaseName = cells[0],
                            Fx = double.Parse(cells[1]),
                            Fy = double.Parse(cells[2]),
                            Fz = double.Parse(cells[3]),
                            Mx = double.Parse(cells[4]),
                            My = double.Parse(cells[5]),
                            Mz = double.Parse(cells[6]),
                            Note = cells[7]
                        };
                        list.Add(model);
                    }
                }
                this.BearingLoadModels = list.ToArray();
            }
        }

        [RelayCommand]
        private void ChangeEnable()
        {
            if (SelectLoadModel is not null)
            {
                foreach (var m in BearingLoadModels)
                {
                    if (m.CaseName == SelectLoadModel.CaseName) { m.IsEnable = !m.IsEnable; }
                }
                BearingLoadModels = BearingLoadModels.ToArray();
                SelectLoadModel = null;
            }
        }
        [RelayCommand]
        private void Remove()
        {
            if (SelectLoadModel is not null)
            {
                BearingLoadModels = BearingLoadModels.Where(x => x.CaseName != SelectLoadModel.CaseName).ToArray();
            }
        }
        [RelayCommand]
        private void RemoveCurrent()
        {
            if (SelectLoadModel is not null)
            {
                BearingLoadModels = BearingLoadModels.Where(x => x != SelectLoadModel).ToArray();
            }
        }
        [RelayCommand]
        private void CopyToClipboard()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(BearingLoadModel.GetTitleLine());
            foreach (var m in BearingLoadModels)
            {
                sb.AppendLine(m.ToString());
            }
            Clipboard.SetDataObject(sb.ToString());
        }



        [RelayCommand]
        private void Export()
        {
            try
            {
                if (BearingLoadModels == null) { throw new Exception("LoadMaps is null"); }
                if (!this.SelectedLoadAreaCheck(out string mess)) { throw new Exception(mess); }
                var sv = new Microsoft.Win32.SaveFileDialog() { Filter = "Abaqus|*.inp|Optistruct|*.fem" };
                if (sv.ShowDialog() == true)
                {
                    string content = string.Empty;
                    switch (sv.FilterIndex)
                    {
                        case 1: content = this.ExportForAbaqus(); break;
                        case 2: content = this.ExportForOptistruct(); break;
                    }
                    System.IO.File.WriteAllText(sv.FileName, content);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #region MyRegion
        private string ExportForOptistruct()
        {
            string[] casenames = BearingLoadModels.Where(m => m.IsEnable).Select(m => m.CaseName).Distinct().ToArray();
            int subcase = 10001;
            int setid = 10000;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"DISPLACEMENT(OPTI) = 10000");
            sb.AppendLine($"DISPLACEMENT(PUNCH) = 10000");
            sb.AppendLine($"CONTF(ALL) = ALL");
            sb.AppendLine($"STRAIN(CORNER) = ALL");
            sb.AppendLine($"STRESS(CORNER) = YES");
            for (int i = 0; i < casenames.Length; i++)
            {
                sb.AppendLine($"SUBCASE    {subcase + i}");
                sb.AppendLine($"  LABEL {casenames[i]}");
                sb.AppendLine($"ANALYSIS STATICS");
                sb.AppendLine($"  LOAD =    {subcase + i}");
            }
            sb.AppendLine($"BEGIN BULK");
            sb.AppendLine($"SET,{setid},GRID,LIST");
            sb.AppendLine($"+,1,2,3,4,5,6,11,12");
            sb.AppendLine($"+,13,14,15,16");
            for (int i = 0; i < casenames.Length; i++)
            {
                BearingLoadModel[] ms = BearingLoadModels.Where(m => m.CaseName == casenames[i]).ToArray();
                if (ms.Length != 6) { throw new Exception($"{casenames[i]} 工况载荷数不满足要求"); }
                sb.AppendLine($"FORCE,{subcase + i},{this.SelectedLoadAreaIndex1 + 1},,1,0,0,{ms[0].Fz * ScaleF},");
                sb.AppendLine($"FORCE,{subcase + i},{this.SelectedLoadAreaIndex2 + 1},,1,0,0,{ms[1].Fz * ScaleF},");
                sb.AppendLine($"FORCE,{subcase + i},{this.SelectedLoadAreaIndex3 + 1},,1,0,0,{ms[2].Fz * ScaleF},");
                sb.AppendLine($"FORCE,{subcase + i},{this.SelectedLoadAreaIndex4 + 1},,1,0,0,{ms[3].Fz * ScaleF},");
                sb.AppendLine($"FORCE,{subcase + i},{this.SelectedLoadAreaIndex5 + 1},,1,0,0,{ms[4].Fz * ScaleF},");
                sb.AppendLine($"FORCE,{subcase + i},{this.SelectedLoadAreaIndex6 + 1},,1,0,0,{ms[5].Fz * ScaleF},");

                sb.AppendLine($"MOMENT,{subcase + i},{this.SelectedLoadAreaIndex1 + 1 + 10},,1,0,0,{ms[0].Mz * ScaleM},");
                sb.AppendLine($"MOMENT,{subcase + i},{this.SelectedLoadAreaIndex2 + 1 + 10},,1,0,0,{ms[1].Mz * ScaleM},");
                sb.AppendLine($"MOMENT,{subcase + i},{this.SelectedLoadAreaIndex3 + 1 + 10},,1,0,0,{ms[2].Mz * ScaleM},");
                sb.AppendLine($"MOMENT,{subcase + i},{this.SelectedLoadAreaIndex4 + 1 + 10},,1,0,0,{ms[3].Mz * ScaleM},");
                sb.AppendLine($"MOMENT,{subcase + i},{this.SelectedLoadAreaIndex5 + 1 + 10},,1,0,0,{ms[4].Mz * ScaleM},");
                sb.AppendLine($"MOMENT,{subcase + i},{this.SelectedLoadAreaIndex6 + 1 + 10},,1,0,0,{ms[5].Mz * ScaleM},");

                sb.AppendLine($"FORCE,{subcase + i},{this.SelectedLoadAreaIndex1 + 1 + 10},,1,{ms[0].Fx * ScaleF},{ms[0].Fy * ScaleF},0,");
                sb.AppendLine($"FORCE,{subcase + i},{this.SelectedLoadAreaIndex2 + 1 + 10},,1,{ms[1].Fx * ScaleF},{ms[1].Fy * ScaleF},0,");
                sb.AppendLine($"FORCE,{subcase + i},{this.SelectedLoadAreaIndex3 + 1 + 10},,1,{ms[2].Fx * ScaleF},{ms[2].Fy * ScaleF},0,");
                sb.AppendLine($"FORCE,{subcase + i},{this.SelectedLoadAreaIndex4 + 1 + 10},,1,{ms[3].Fx * ScaleF},{ms[3].Fy * ScaleF},0,");
                sb.AppendLine($"FORCE,{subcase + i},{this.SelectedLoadAreaIndex5 + 1 + 10},,1,{ms[4].Fx * ScaleF},{ms[4].Fy * ScaleF},0,");
                sb.AppendLine($"FORCE,{subcase + i},{this.SelectedLoadAreaIndex6 + 1 + 10},,1,{ms[5].Fx * ScaleF},{ms[5].Fy * ScaleF},0,");
                sb.AppendLine($"MOMENT,{subcase + i},{this.SelectedLoadAreaIndex1 + 1 + 10},,1,{ms[0].Mx * ScaleM},{ms[0].My * ScaleM},0,");
                sb.AppendLine($"MOMENT,{subcase + i},{this.SelectedLoadAreaIndex2 + 1 + 10},,1,{ms[1].Mx * ScaleM},{ms[1].My * ScaleM},0,");
                sb.AppendLine($"MOMENT,{subcase + i},{this.SelectedLoadAreaIndex3 + 1 + 10},,1,{ms[2].Mx * ScaleM},{ms[2].My * ScaleM},0,");
                sb.AppendLine($"MOMENT,{subcase + i},{this.SelectedLoadAreaIndex4 + 1 + 10},,1,{ms[3].Mx * ScaleM},{ms[3].My * ScaleM},0,");
                sb.AppendLine($"MOMENT,{subcase + i},{this.SelectedLoadAreaIndex5 + 1 + 10},,1,{ms[4].Mx * ScaleM},{ms[4].My * ScaleM},0,");
                sb.AppendLine($"MOMENT,{subcase + i},{this.SelectedLoadAreaIndex6 + 1 + 10},,1,{ms[5].Mx * ScaleM},{ms[5].My * ScaleM},0,");
            }
            sb.AppendLine($"ENDDATA");

            return sb.ToString();
        }
        private string ExportForAbaqus()
        {
            StringBuilder sb = new StringBuilder();
            var gs = BearingLoadModels.Where(m => m.IsEnable).GroupBy(m => m.CaseName);
            sb.AppendLine($"*Nset, nset=SET10000");
            sb.AppendLine($"  1,  2,  3,  4,  5,  6, 11, 12, 13, 14, 15, 16");
            sb.AppendLine($"*Amplitude, name=load");
            sb.AppendLine($"            0.0,             0.0,              0.3,              1.0,             0.5,             1.5,             0.7,              2.");
            sb.AppendLine($"           0.85,             2.5,              1.0,              3.0");
            sb.AppendLine($"*Time Points, name=tps");
            sb.AppendLine($"0.0, 0.3, 0.5, 0.7, 0.85, 1.0");
            int i = 1;
            foreach (var g in gs)
            {
                if (g.Count() != 6) { throw new Exception($"{g.Key} 工况载荷数不满足要求"); }
                var ms = g.ToArray();
                sb.AppendLine($"*Step, name=LoadStep{i:00}, nlgeom=YES");
                sb.AppendLine(g.Key);
                sb.AppendLine("*Static");
                sb.AppendLine("0.3, 1.0, 1e-05, 1.0");
                sb.AppendLine($"*Cload,amplitude=load");
                sb.AppendLine($"{this.SelectedLoadAreaIndex1 + 1 + 10}, 1, {ms[0].Fx * ScaleF}");
                sb.AppendLine($"{this.SelectedLoadAreaIndex2 + 1 + 10}, 1, {ms[1].Fx * ScaleF}");
                sb.AppendLine($"{this.SelectedLoadAreaIndex3 + 1 + 10}, 1, {ms[2].Fx * ScaleF}");
                sb.AppendLine($"{this.SelectedLoadAreaIndex4 + 1 + 10}, 1, {ms[3].Fx * ScaleF}");
                sb.AppendLine($"{this.SelectedLoadAreaIndex5 + 1 + 10}, 1, {ms[4].Fx * ScaleF}");
                sb.AppendLine($"{this.SelectedLoadAreaIndex6 + 1 + 10}, 1, {ms[5].Fx * ScaleF}");
                sb.AppendLine($"{this.SelectedLoadAreaIndex1 + 1 + 10}, 2, {ms[0].Fy * ScaleF}");
                sb.AppendLine($"{this.SelectedLoadAreaIndex2 + 1 + 10}, 2, {ms[1].Fy * ScaleF}");
                sb.AppendLine($"{this.SelectedLoadAreaIndex3 + 1 + 10}, 2, {ms[2].Fy * ScaleF}");
                sb.AppendLine($"{this.SelectedLoadAreaIndex4 + 1 + 10}, 2, {ms[3].Fy * ScaleF}");
                sb.AppendLine($"{this.SelectedLoadAreaIndex5 + 1 + 10}, 2, {ms[4].Fy * ScaleF}");
                sb.AppendLine($"{this.SelectedLoadAreaIndex6 + 1 + 10}, 2, {ms[5].Fy * ScaleF}");
                sb.AppendLine($"{this.SelectedLoadAreaIndex1 + 1}, 3, {ms[0].Fz * ScaleF}");
                sb.AppendLine($"{this.SelectedLoadAreaIndex2 + 1}, 3, {ms[1].Fz * ScaleF}");
                sb.AppendLine($"{this.SelectedLoadAreaIndex3 + 1}, 3, {ms[2].Fz * ScaleF}");
                sb.AppendLine($"{this.SelectedLoadAreaIndex4 + 1}, 3, {ms[3].Fz * ScaleF}");
                sb.AppendLine($"{this.SelectedLoadAreaIndex5 + 1}, 3, {ms[4].Fz * ScaleF}");
                sb.AppendLine($"{this.SelectedLoadAreaIndex6 + 1}, 3, {ms[5].Fz * ScaleF}");
                sb.AppendLine($"{this.SelectedLoadAreaIndex1 + 1 + 10}, 4, {ms[0].Mx * ScaleM}");
                sb.AppendLine($"{this.SelectedLoadAreaIndex2 + 1 + 10}, 4, {ms[1].Mx * ScaleM}");
                sb.AppendLine($"{this.SelectedLoadAreaIndex3 + 1 + 10}, 4, {ms[2].Mx * ScaleM}");
                sb.AppendLine($"{this.SelectedLoadAreaIndex4 + 1 + 10}, 4, {ms[3].Mx * ScaleM}");
                sb.AppendLine($"{this.SelectedLoadAreaIndex5 + 1 + 10}, 4, {ms[4].Mx * ScaleM}");
                sb.AppendLine($"{this.SelectedLoadAreaIndex6 + 1 + 10}, 4, {ms[5].Mx * ScaleM}");
                sb.AppendLine($"{this.SelectedLoadAreaIndex1 + 1 + 10}, 5, {ms[0].My * ScaleM}");
                sb.AppendLine($"{this.SelectedLoadAreaIndex2 + 1 + 10}, 5, {ms[1].My * ScaleM}");
                sb.AppendLine($"{this.SelectedLoadAreaIndex3 + 1 + 10}, 5, {ms[2].My * ScaleM}");
                sb.AppendLine($"{this.SelectedLoadAreaIndex4 + 1 + 10}, 5, {ms[3].My * ScaleM}");
                sb.AppendLine($"{this.SelectedLoadAreaIndex5 + 1 + 10}, 5, {ms[4].My * ScaleM}");
                sb.AppendLine($"{this.SelectedLoadAreaIndex6 + 1 + 10}, 5, {ms[5].My * ScaleM}");
                sb.AppendLine($"{this.SelectedLoadAreaIndex1 + 1 + 10}, 6, {ms[0].Mz * ScaleM}");
                sb.AppendLine($"{this.SelectedLoadAreaIndex2 + 1 + 10}, 6, {ms[1].Mz * ScaleM}");
                sb.AppendLine($"{this.SelectedLoadAreaIndex3 + 1 + 10}, 6, {ms[2].Mz * ScaleM}");
                sb.AppendLine($"{this.SelectedLoadAreaIndex4 + 1 + 10}, 6, {ms[3].Mz * ScaleM}");
                sb.AppendLine($"{this.SelectedLoadAreaIndex5 + 1 + 10}, 6, {ms[4].Mz * ScaleM}");
                sb.AppendLine($"{this.SelectedLoadAreaIndex6 + 1 + 10}, 6, {ms[5].Mz * ScaleM}");
                sb.AppendLine($"*Output, field, variable=PRESELECT, time points=tps");
                sb.AppendLine($"*Output, history, variable=PRESELECT, time points=tps");
                sb.AppendLine($"*Output, history, time points=tps");
                sb.AppendLine($"*Node Output, nset=SET10000");
                sb.AppendLine($"U1, U2, U3, UR1, UR2, UR3");
                sb.AppendLine($"*Node Print, nset=SET10000");
                sb.AppendLine($"U1, U2, U3, UR1, UR2, UR3");
                sb.AppendLine($"*End Step");
                i++;
            }
            return sb.ToString();
        }
        #endregion
    }
}
