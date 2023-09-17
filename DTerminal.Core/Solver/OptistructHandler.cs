using DTerminal.Core.Solver.Optistruct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DTerminal.Core.Solver
{
    public class OptistructHandler
    {
        /// <summary>
        /// 生成频率响应分析模板
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static string GenerateDynamicStiffnessTemplate(FrequencyResponseParameter parameter)
        {
            string[] ord = { "x", "y", "z" };

            System.Text.StringBuilder sb = new StringBuilder();
            sb.AppendLine($"SUBCASE {parameter.SubcaseID}");
            sb.AppendLine($"  LABEL normal modes");
            sb.AppendLine($"ANALYSIS MODES");
            sb.AppendLine($"  METHOD(STRUCTURE) = {parameter.EigrlID}");
            switch (parameter.IsCombineLoad)
            {
                case true:
                    for (int i = 0; i < parameter.NodeCount; i++)
                    {
                        sb.AppendLine($"SUBCASE {parameter.SubcaseID + 1 + i}");
                        sb.AppendLine($"  LABEL freq.resp_{(i + 1).ToString("00")}");
                        sb.AppendLine($"ANALYSIS MFREQ");
                        sb.AppendLine($"  METHOD(STRUCTURE) = {parameter.EigrlID}");
                        sb.AppendLine($"  FREQUENCY = {parameter.FreqID}");
                        sb.AppendLine($"  DLOAD = {parameter.RloadID + i}");
                        sb.AppendLine($"  ACCELERATION(SORT2,PUNCH,PHASE) = {parameter.SetGridID + i}");
                        sb.AppendLine($"  DISPLACEMENT(SORT2,PUNCH,PHASE) = {parameter.SetGridID + i}");
                    };
                    break;
                case false:
                    for (int i = 0; i < parameter.NodeCount; i++)
                    {
                        for (int j = 1; j <= ord.Length; j++)
                        {
                            sb.AppendLine($"SUBCASE {(parameter.SubcaseID + 1 + i) * 10 + j}");
                            sb.AppendLine($"  LABEL freq.resp_{(i + 1).ToString("00")}.{ord[j - 1]}");
                            sb.AppendLine($"ANALYSIS MFREQ");
                            sb.AppendLine($"  METHOD(STRUCTURE) = {parameter.EigrlID}");
                            sb.AppendLine($"  FREQUENCY = {parameter.FreqID}");
                            sb.AppendLine($"  DLOAD = {(parameter.RloadID + i) * 10 + j}");
                            sb.AppendLine($"  ACCELERATION(SORT2,PUNCH,PHASE) = {parameter.SetGridID + i}");
                            sb.AppendLine($"  DISPLACEMENT(SORT2,PUNCH,PHASE) = {parameter.SetGridID + i}");
                        }
                    };
                    break;
            }
            sb.AppendLine($"BEGIN BULK");
            sb.AppendLine($"PARAM,G,{parameter.ParamG}");
            sb.AppendLine($"TABLED1,{parameter.Tabled1ID},LINEAR,LINEAR");
            sb.AppendLine($"+,0,1,{parameter.TargetFrequency},1,ENDT");
            sb.AppendLine($"EIGRL,{parameter.EigrlID},0,{parameter.TargetFrequency},,,,,MASS");
            sb.AppendLine($"FREQ1,{parameter.FreqID},{parameter.StarFrequency},{parameter.NumberofFrequencySweeps},{(int)((parameter.TargetFrequency - parameter.StarFrequency) / parameter.NumberofFrequencySweeps)}");
            sb.AppendLine($"FREQ3,{parameter.FreqID},{parameter.StarFrequency},{parameter.TargetFrequency},LOG,{parameter.FRQE3NEF},{parameter.FRQE3CLUSTER}");

            switch (parameter.IsCombineLoad)
            {
                case true:
                    for (int i = 0; i < parameter.NodeCount; i++)
                    {
                        sb.AppendLine($"DAREA,{parameter.DareaID + i},{parameter.StarNode + i},1,1");
                        sb.AppendLine($"DAREA,{parameter.DareaID + i},{parameter.StarNode + i},2,1");
                        sb.AppendLine($"DAREA,{parameter.DareaID + i},{parameter.StarNode + i},3,1");
                        sb.AppendLine($"RLOAD2,{parameter.RloadID + i},{parameter.DareaID + i},,,{parameter.Tabled1ID},,");
                        sb.AppendLine($"SET,{parameter.SetGridID + i},GRID,LIST");
                        sb.AppendLine($"+,{parameter.StarNode + i}");
                    }
                    ; break;
                case false:
                    for (int i = 0; i < parameter.NodeCount; i++)
                    {
                        for (int j = 1; j <= ord.Length; j++)
                        {
                            sb.AppendLine($"SUBCASE {(parameter.SubcaseID + 1 + i) * 10 + j}");
                            sb.AppendLine($"  LABEL freq.resp_{(i + 1).ToString("00")}.{ord[j - 1]}");
                            sb.AppendLine($"ANALYSIS MFREQ");
                            sb.AppendLine($"  METHOD(STRUCTURE) = {parameter.EigrlID}");
                            sb.AppendLine($"  FREQUENCY = {parameter.FreqID}");
                            sb.AppendLine($"  DLOAD = {(parameter.RloadID + i) * 10 + j}");
                            sb.AppendLine($"  ACCELERATION(SORT2,PUNCH,PHASE) = {parameter.SetGridID + i}");
                            sb.AppendLine($"  DISPLACEMENT(SORT2,PUNCH,PHASE) = {parameter.SetGridID + i}");
                        }
                    }
                    ; break;
            }
            sb.AppendLine($"ENDDATA");
            return sb.ToString();
        }


        /// <summary>
        /// 读取punch文件
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static FrequencyResponsePunch[] ReadPunch(string file)
        {
            List<FrequencyResponsePunch> Punches = new List<FrequencyResponsePunch>();
            string[] lines = System.IO.File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("$TITLE") && lines[i + 6].Contains("$POINT ID"))
                {
                    var p = new FrequencyResponsePunch
                    {
                        Title = lines[i++].Split('=').Last().Trim(),
                        SubTitle = lines[i++].Split('=').Last().Trim(),
                        Label = lines[i++].Split('=').Last().Trim(),
                        ResultType = lines[i++].Split('=').Last().Replace("$", "").Trim(),
                        ResultFormat = lines[i++].Split('=').Last().Replace("$", "").Trim(),
                        SubcaseID = int.Parse(lines[i++].Split('=').Last()),
                        PointID = int.Parse(lines[i++].Split('=').Last())
                    };
                    var ds = new List<FrequencyResponseData>();
                    for (int j = i; j < lines.Length; j++)
                    {
                        if (lines[j].Contains("$TITLE")) { i = j - 1; break; }
                        var d = new FrequencyResponseData();
                        d.Frequency = double.Parse(lines[j].Substring(0, 16));
                        d.R1 = new BaseData()
                        {
                            X = double.Parse(lines[j].Substring(23, 13)),
                            Y = double.Parse(lines[j].Substring(41, 13)),
                            Z = double.Parse(lines[j++].Substring(59, 13))
                        };
                        d.R2 = new BaseData()
                        {
                            X = double.Parse(lines[j].Substring(23, 13)),
                            Y = double.Parse(lines[j].Substring(41, 13)),
                            Z = double.Parse(lines[j++].Substring(59, 13))
                        };
                        d.R3 = new BaseData()
                        {
                            X = double.Parse(lines[j].Substring(23, 13)),
                            Y = double.Parse(lines[j].Substring(41, 13)),
                            Z = double.Parse(lines[j++].Substring(59, 13))
                        };
                        d.R4 = new BaseData()
                        {
                            X = double.Parse(lines[j].Substring(23, 13)),
                            Y = double.Parse(lines[j].Substring(41, 13)),
                            Z = double.Parse(lines[j].Substring(59, 13))
                        };
                        ds.Add(d);
                    }
                    p.Value = ds.ToArray();
                    Punches.Add(p);
                }
            }
            return Punches.ToArray();
        }


        /// <summary>
        /// 将原始数据转换为动刚度数据
        /// </summary>
        /// <param name="punch"></param>
        /// <returns></returns>
        public static IFrequencyResponseMainData[] ConvertToDynamicStiffness(FrequencyResponsePunch punch)
        {
            List<IFrequencyResponseMainData> datas = new List<IFrequencyResponseMainData>();
            if (punch.Value is not null)
            {
                if (punch.ResultType == "DISPLACEMENTS")
                {
                    foreach (var v in punch.Value)
                    {
                        if (v.R1 is not null)
                        {
                            BaseData baseData = new BaseData()
                            {
                                X = 1 / v.R1.X,
                                Y = 1 / v.R1.Y,
                                Z = 1 / v.R1.Z,
                            };
                            datas.Add(new FrequencyResponseData() { Frequency = v.Frequency, R1 = baseData });
                        }
                    }
                }
                else
                {
                    foreach (var v in punch.Value)
                    {
                        if (v.R1 is not null)
                        {
                            BaseData baseData = new BaseData()
                            {
                                X = 4 * Math.PI * Math.PI * v.Frequency * v.Frequency / v.R1.X,
                                Y = 4 * Math.PI * Math.PI * v.Frequency * v.Frequency / v.R1.Y,
                                Z = 4 * Math.PI * Math.PI * v.Frequency * v.Frequency / v.R1.Z,
                            };
                            datas.Add(new FrequencyResponseData() { Frequency = v.Frequency, R1 = baseData });
                        }
                    }
                }
            }
            return datas.ToArray();
        }
    }
}
