using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTerminal.Core.Solver;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static DTerminal.ViewModels.PumpLinxResultExtractViewModel;

namespace DTerminal.ViewModels
{
    public class PumpLinxResultExtractViewModel : ObservableObject
    {
        public PumpLinxResultExtractViewModel(PumpLinxResultExtract_BatchViewModel batchViewModel, PumpLinxResultExtract_SingleViewModel singleViewModel)
        {
            BatchViewModel = batchViewModel;
            SingleViewModel = singleViewModel;
        }

        public PumpLinxResultExtract_BatchViewModel BatchViewModel { get; set; }
        public PumpLinxResultExtract_SingleViewModel SingleViewModel { get; set; }


    }

    public class PumpLinxResultExtract_BatchViewModel : ObservableObject
    {
        public RelayCommand ExtractMultipleByAverageCommand { get; set; }
        public PumpLinxResultExtract_BatchViewModel()
        {
            ExtractMultipleByAverageCommand = new RelayCommand(ExtractMultipleByAverage);
        }
        private void ExtractMultipleByAverage()
        {
            try
            {
                var op = new OpenFileDialog() { Filter = "pumplinx结果文件|*.txt", Multiselect = true };
                if (op.ShowDialog() == true)
                {
                    var ds = op.FileNames.Select(x => PumpLinxHandler.ExtractResultByAverage(x)).ToArray();
                    NPOI.XSSF.UserModel.XSSFWorkbook wb = new NPOI.XSSF.UserModel.XSSFWorkbook();
                    var st = wb.CreateSheet();

                    var row = st.CreateRow(0);
                    row.CreateCell(1).SetCellValue(System.IO.Path.GetFileNameWithoutExtension(op.FileNames[0]));

                    var d = ds[0].ToArray();
                    for (int j = 0; j < d.Length; j++)
                    {
                        row = st.CreateRow(j + 1);
                        row.CreateCell(0).SetCellValue(d[j].Key);
                        row.CreateCell(1).SetCellValue(d[j].Value); ;
                    }
                    for (int i = 1; i < ds.Length; i++)
                    {
                        st.GetRow(0).CreateCell(1 + 1).SetCellValue(System.IO.Path.GetFileNameWithoutExtension(op.FileNames[i]));
                        d = ds[i].ToArray();
                        if (ds[i].Count != ds[i - 1].Count) { throw new Exception($"文件列数不对等：\r\n1:\t{System.IO.Path.GetFileNameWithoutExtension(op.FileNames[i-1])}\r\n2:\t{System.IO.Path.GetFileNameWithoutExtension(op.FileNames[i])}"); }
                        for (int j = 0; j < d.Length; j++)
                        {
                            row = st.GetRow(j + 1);
                            row.CreateCell(i + 1).SetCellValue(d[j].Value); ;
                        }
                    }
                    using MemoryStream memoryStream = new MemoryStream();
                    wb.Write(memoryStream);
                    System.IO.File.WriteAllBytes(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(op.FileName) ?? "", "MultipleResult.xlsx"), memoryStream.ToArray());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
               
            }


        }
    }

    public class PumpLinxResultExtract_SingleViewModel : ObservableObject
    {
        private Dictionary<string, double> results = new Dictionary<string, double>();
        private KeyValuePair<string, double>? selectedResult;
        private string? filePath;

        public RelayCommand ExtractByAverageCommand { get; set; }
        public RelayCommand CopyValueCommand { get; set; }
        public RelayCommand CopyAllCommand { get; set; }
        public Dictionary<string, double> Results { get => results; set => this.SetProperty(ref results, value); }
        public string? FilePath { get => filePath; set => this.SetProperty(ref filePath, value); }
        public KeyValuePair<string, double>? SelectedResult { get => selectedResult; set => this.SetProperty(ref selectedResult, value); }
        public PumpLinxResultExtract_SingleViewModel()
        {
            ExtractByAverageCommand = new RelayCommand(ExtractByAverage);
            CopyValueCommand = new RelayCommand(CopyValue);
            CopyAllCommand = new RelayCommand(CopyAll);

        }

        private void CopyAll()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{System.IO.Path.GetFileNameWithoutExtension(FilePath)}");
            foreach (var item in Results)
            {
                sb.AppendLine($"{item.Key}\t{item.Value}");
            }
            System.Windows.Clipboard.SetDataObject(sb.ToString(), true);
        }
        private void CopyValue()
        {
            if (this.SelectedResult is KeyValuePair<string, double> k)
            {
                System.Windows.Clipboard.SetDataObject(k.Value.ToString(), true);
            }
        }
        private void ExtractByAverage()
        {
            var op = new OpenFileDialog() { Filter = "pumplinx结果文件|*.txt" };
            if (op.ShowDialog() == true)
            {
                Results = PumpLinxHandler.ExtractResultByAverage(op.FileName);
                SelectedResult = null;
                this.FilePath = op.FileName;
            }
        }
    }
}
