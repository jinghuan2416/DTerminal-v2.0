using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTerminal.Core.Solver;
using DTerminal.Core.Solver.Optistruct;
using DTerminal.Models;
using NPOI.HPSF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DTerminal.ViewModels
{
    public class OptistructFrequencyResponseViewModel : ObservableObject
    {
        public OptistructFrequencyResponse_DynamicStiffnessViewModel DynamicStiffnessViewModel { get; set; }
        public OptistructFrequencyResponse_PunchPlotViewModel PunchPlotViewModel { get; set; }

        public OptistructFrequencyResponseViewModel(OptistructFrequencyResponse_DynamicStiffnessViewModel dynamicStiffness, OptistructFrequencyResponse_PunchPlotViewModel punchViewModel)
        {
            DynamicStiffnessViewModel = dynamicStiffness;
            PunchPlotViewModel = punchViewModel;
        }
    }

    public class OptistructFrequencyResponse_DynamicStiffnessViewModel : ObservableObject
    {
        public FrequencyResponseParameter FrequencyResponseParameter { get; set; } = new FrequencyResponseParameter();

        public RelayCommand GenerateTemplateCommand { get; set; }

        public OptistructFrequencyResponse_DynamicStiffnessViewModel()
        {
            GenerateTemplateCommand = new RelayCommand(GenerateTemplate);
        }

        private void GenerateTemplate()
        {
            try
            {
                string fem = OptistructHandler.GenerateDynamicStiffnessTemplate(FrequencyResponseParameter);
                var sv = new Microsoft.Win32.SaveFileDialog { Filter = "OptiStruct求解器文件|*.fem" };
                if (sv.ShowDialog() == true)
                {
                    System.IO.File.WriteAllText(sv.FileName, fem);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class OptistructFrequencyResponse_PunchPlotViewModel : ObservableObject
    {
        private PunchDrawParameter? selectPunchDrawParameter;
        private PunchDrawParameter[] punchDrawParameters = System.Array.Empty<PunchDrawParameter>();
        private string? filePath;
        public RelayCommand ReadFileCommand { get; set; }
        public RelayCommand SavePicCommand { get; set; }
        public RelayCommand SaveAllPicsCommand { get; set; }
        public RelayCommand SaveDataCommand { get; set; }
        public RelayCommand SaveAllDataCommand { get; set; }
        public AsyncRelayCommand SynchronizeCommand { get; set; }
        public PunchDrawParameter? SelectPunchDrawParameter { get => selectPunchDrawParameter; set => this.SetProperty(ref selectPunchDrawParameter, value); }
        public PunchDrawParameter[] PunchDrawParameters { get => punchDrawParameters; set => this.SetProperty(ref punchDrawParameters, value); }
        public int PngWidth { get; set; } = 1280;
        public int PngHeight { get; set; } = 720;
        public OptistructFrequencyResponse_PunchPlotViewModel()
        {
            ReadFileCommand = new RelayCommand(ReadFile);
            SavePicCommand = new RelayCommand(SavePic);
            SaveAllPicsCommand = new RelayCommand(SaveAllPics);
            SynchronizeCommand = new AsyncRelayCommand(Synchronize);
            SaveDataCommand = new RelayCommand(SaveData);
            SaveAllDataCommand = new RelayCommand(SaveAllData);
        }

        private void SaveAllData()
        {
            NPOI.XSSF.UserModel.XSSFWorkbook wb = new NPOI.XSSF.UserModel.XSSFWorkbook();
            foreach (var parameter in this.PunchDrawParameters)
            {
                wb.CreateSheet(parameter);
            }
            using (var ms = new MemoryStream())
            {
                wb.Write(ms);
                var sv = new Microsoft.Win32.SaveFileDialog() { Filter = "Excel文档|*.xlsx" };
                if (sv.ShowDialog() == true)
                {
                    System.IO.File.WriteAllBytes(sv.FileName, ms.ToArray());
                }
            }
        }


        private void SaveData()
        {
            if (this.SelectPunchDrawParameter is PunchDrawParameter parameter)
            {
                NPOI.XSSF.UserModel.XSSFWorkbook wb = new NPOI.XSSF.UserModel.XSSFWorkbook();
                wb.CreateSheet(parameter);
                using (var ms = new MemoryStream())
                {
                    wb.Write(ms);
                    var sv = new Microsoft.Win32.SaveFileDialog() { Filter = "Excel文档|*.xlsx", FileName = parameter.FrequencyResponsePunch.Label + "-" + parameter.FrequencyResponsePunch.ResultType };
                    if (sv.ShowDialog() == true)
                    {
                        System.IO.File.WriteAllBytes(sv.FileName, ms.ToArray());
                    }
                }
            }
        }

        private void SaveAllPics()
        {
            var png = new OxyPlot.Wpf.PngExporter();
            var root = System.IO.Path.GetDirectoryName(this.filePath) + $"\\Pic";
            png.Width = this.PngWidth;
            png.Height = this.PngHeight;
            var gs = this.PunchDrawParameters.GroupBy(x => x.FrequencyResponsePunch.ResultType);
            foreach (var g in gs)
            {
                var dic = System.IO.Directory.CreateDirectory(System.IO.Path.Combine(root, g.Key ?? Guid.NewGuid().ToString()));
                foreach (var sub in g)
                {
                    using System.IO.FileStream fs = System.IO.File.Create(dic.FullName + $"\\{sub.FrequencyResponsePunch.Label}-{sub.FrequencyResponsePunch.ResultType}.png");
                    png.Export(sub.PlotModel, fs);
                }

            }
            System.Diagnostics.Process.Start("explorer.exe", root);
        }

        private void SavePic()
        {
            if (this.SelectPunchDrawParameter is PunchDrawParameter para)
            {
                var png = new OxyPlot.Wpf.PngExporter();
                var dic = System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(this.filePath) + $"\\Pic");
                png.Width = this.PngWidth;
                png.Height = this.PngHeight;
                using System.IO.FileStream fs = System.IO.File.Create(dic.FullName + $"\\{para.FrequencyResponsePunch.Label}-{para.FrequencyResponsePunch.ResultType}.png");
                png.Export(para.PlotModel, fs);
                System.Diagnostics.Process.Start("explorer.exe", dic.FullName);
            }
        }

        private async Task Synchronize()
        {
            await Task.Run(() =>
            {
                foreach (var p in PunchDrawParameters.Where(x => x != SelectPunchDrawParameter))
                {
                    if (this.SelectPunchDrawParameter != null)
                    {
                        p.CopySettingFrom(this.SelectPunchDrawParameter);
                    }
                }
            });
        }

        private void ReadFile()
        {
            var op = new Microsoft.Win32.OpenFileDialog();
            op.Filter = "punch|*.pch";
            if (op.ShowDialog() == true)
            {
                this.filePath = op.FileName;
                var punchs = OptistructHandler.ReadPunch(this.filePath);
                this.PunchDrawParameters = punchs.Select(x => new PunchDrawParameter(x)).ToArray();
                if (punchs.Length > 0)
                {
                    this.SelectPunchDrawParameter = PunchDrawParameters.FirstOrDefault();
                }
            }
        }
    }
}
