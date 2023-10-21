using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DTerminal.Core;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static DTerminal.ViewModels.DataMergeViewModel;

namespace DTerminal.ViewModels
{
    public class DataMergeViewModel : ObservableObject
    {
        private FileGroup[] fileGroups = Array.Empty<FileGroup>();
        private FileInfo[] fileInfos = Array.Empty<FileInfo>();

        public bool IgnoreFirstLine { get; set; } = true;
        public string? OriginalDataSeparator { get; set; } = "\\t";
        public string? OriginalDataSeparatorToolTip { get; set; } = "当源文件为文本文件时，使用正则语法，特殊符号如下：\r\n制表位：\\t\r\n空格：\\s";
        public FileGroup[] FileGroups { get => fileGroups; set => this.SetProperty(ref fileGroups, value); }
        public FileInfo[] FileInfos { get => fileInfos; set => this.SetProperty(ref fileInfos, value); }
        public string FileGroupSeparator { get; set; } = string.Empty;
        public RelayCommand GroupCommand { get; set; }
        public RelayCommand MergeCommand { get; set; }
        public RelayCommand PasteFolderPathCommand { get; set; }
        public RelayCommand ClearCommand { get; set; }
        public string XAxisTitle { get; set; } = "X";
        public string YAxisTitle { get; set; } = "Y";

        public DataMergeViewModel()
        {
            WeakReferenceMessenger.Default.Register<string[], string>(this, nameof(DataMergeViewModel), (e, filePaths) =>
            {
                FileInfos = filePaths.Select(x => new FileInfo(x, System.IO.Path.GetFileName(x))).ToArray();
                FileGroups = Array.Empty<FileGroup>();
            });

            GroupCommand = new RelayCommand(() =>
            {
                var fs = FileInfos.Select(x =>
                {
                    var cells = x.Title.Split(FileGroupSeparator);
                    FileInfo fileInfo = new(x.FilePath, string.IsNullOrEmpty(cells[0]) ? "未分类" : cells[0]);
                    return fileInfo;
                });
                FileGroups = fs.GroupBy(x => x.Title).Select(x => new FileGroup(x.Key, x.ToArray())).ToArray();
            });

            MergeCommand = new RelayCommand(() =>
            {
                try
                {
                    if (FileGroups is null) { throw new Exception("请先进行分组后再合并"); }
                    if (FileGroups.Any() == false) { throw new Exception("请先进行分组后再合并"); }
                    NPOI.XSSF.UserModel.XSSFWorkbook wb = new();
                    foreach (var g in FileGroups)
                    {
                        var st = wb.CreateSheet(g.Key);
                        st.CreateRow(0);
                        st.CreateRow(1);
                        for (int col = 0; col < g.FileInfos.Length; col++)
                        {
                            var f = g.FileInfos[col];
                            var dps = this.GetFileData(f.FilePath);

                            var irow = st.GetRow(0);
                            irow.CreateCell(col * 3).SetCellValue(System.IO.Path.GetFileName(f.FilePath));

                            irow = st.GetRow(1);
                            irow.CreateCell(col * 3).SetCellValue(XAxisTitle ?? "X");
                            irow.CreateCell(col * 3 + 1).SetCellValue(YAxisTitle ?? "Y");

                            for (int row = 0; row < dps.Length; row++)
                            {
                                irow = st.GetRow(row + 2) ?? st.CreateRow(row + 2);
                                irow.CreateCell(col * 3).SetCellValue(dps[row].X);
                                irow.CreateCell(col * 3 + 1).SetCellValue(dps[row].Y);
                            }
                        }
                    }
                    using MemoryStream ms = new();
                    wb.Write(ms);
                    Microsoft.Win32.SaveFileDialog sv = new()
                    {
                        Filter = "xlsx|*.xlsx"
                    };
                    if (sv.ShowDialog() == true)
                    {
                        System.IO.File.WriteAllBytes(sv.FileName, ms.ToArray());
                        MessageBox.Show("文件保存完成");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            });

            PasteFolderPathCommand = new RelayCommand(() =>
            {
                try
                {
                    var dic = System.Windows.Clipboard.GetText();
                    if (System.IO.Directory.Exists(dic))
                    {
                        FileInfos = System.IO.Directory.GetFiles(dic).Select(x => new FileInfo(x, System.IO.Path.GetFileName(x))).ToArray();
                        FileGroups = Array.Empty<FileGroup>();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });

            ClearCommand = new RelayCommand(() =>
            {
                FileInfos = Array.Empty<FileInfo>();
                FileGroups = Array.Empty<FileGroup>();
            });
        }

        

        private DPoint[] GetFileData(string filePath)
        {
            var ext = System.IO.Path.GetExtension(filePath).ToLower();
            var ps = Array.Empty<DPoint>();
            switch (ext)
            {
                case ".xlsx":
                    ps = GetXlsxData(filePath); break;
                case ".txt":
                    ps = GetTxtData(filePath); break;
                case ".csv":
                    ps = GetTxtData(filePath); break;
                default:
                    break;
            }
            return ps;
        }

        private DPoint[] GetXlsxData(string filePath)
        {
            NPOI.XSSF.UserModel.XSSFWorkbook wb = new NPOI.XSSF.UserModel.XSSFWorkbook(filePath);
            var st = wb.GetSheetAt(0);
            List<DPoint> ps = new List<DPoint>();

            for (int i = IgnoreFirstLine ? 1 : 0; i < st.LastRowNum; i++)
            {
                var cells = st.GetRow(i).Cells;
                if (cells.Count >= 2)
                {
                    var point = new DPoint() { X = cells[0].NumericCellValue, Y = cells[1].NumericCellValue };
                    ps.Add(point);
                }
            }
            return ps.ToArray();
        }

        private DPoint[] GetTxtData(string filePath)
        {
            List<DPoint> ps = new();
            var lines = System.IO.File.ReadAllLines(filePath).Where(x => x.Trim().Length > 0).ToArray();
            for (int i = IgnoreFirstLine ? 1 : 0; i < lines.Length; i++)
            {
                var cells = System.Text.RegularExpressions.Regex.Split(lines[i], OriginalDataSeparator ?? string.Empty);
                if (cells.Length >= 2)
                {
                    if (double.TryParse(cells[0], out double x) && double.TryParse(cells[1], out double y))
                    {
                        var point = new DPoint() { X = x, Y = y };
                        ps.Add(point);
                    }
                }
            }
            return ps.ToArray();
        }

        public record FileGroup(string Key, FileInfo[] FileInfos);

        public record FileInfo(string FilePath, string Title);

    }
}
