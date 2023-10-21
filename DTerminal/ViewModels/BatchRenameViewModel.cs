using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTerminal.Core.Util.Rename;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static DTerminal.ViewModels.DataMergeViewModel;

namespace DTerminal.ViewModels
{
    public class BatchRenameViewModel : ObservableObject
    {
        public ObservableCollection<IOperation> Operations { get; set; } = new();
        public ObservableCollection<FileInformation> FileInfos { get; set; }=new();
        public RelayCommand ChooseFilesCommand { get; set; }
        public RelayCommand ReviewCommand { get; set; }
        public RelayCommand RenameCommand { get; set; }
        public RelayCommand<OperationType> AddOperationCommand { get; set; }
        public RelayCommand<IOperation> RemoveOperationCommand { get; set; }

        public BatchRenameViewModel()
        {
            ChooseFilesCommand = new RelayCommand(ChooseFiles);
            ReviewCommand = new RelayCommand(Review);
            AddOperationCommand = new RelayCommand<OperationType>(AddOperation);
            RemoveOperationCommand = new RelayCommand<IOperation>(RemoveOperation);

            RenameCommand = new RelayCommand(Rename);
        }

        private void Rename()
        {
            try
            {
                if (FileInfos.DistinctBy(x => x.NewName).Count() != FileInfos.Count) { throw new Exception("存在重复文件名"); }
                var fs = FileInfos.ToArray();
                FileInfos.Clear();
                foreach (var f in fs)
                {
                    System.IO.File.Move(f.FullFileName, f.GetFullNewName());
                    FileInfos.Add(new FileInformation(f.GetFullNewName()));
                }
                MessageBox.Show("文件批量改名完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Review()
        {
            try
            {
                var fs = FileInfos.ToArray();
                foreach (var f in fs)
                {
                    f.NewName = f.FileName;
                    foreach (var o in Operations)
                    {
                        if (o.IsEnable)
                        {
                            f.NewName = o.GetResult(f.NewName);
                        }
                    }
                }
                FileInfos.Clear();
                foreach (var f in fs) { FileInfos.Add(f); }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void AddOperation(OperationType type)
        {
            var tps = typeof(IOperation).Assembly.GetTypes().Where(x => x.GetInterface(nameof(IOperation)) is not null).ToArray();
            var name = type.ToString();
            var tp = tps.First(x => x.Name.Contains(name));
            var op = System.Activator.CreateInstance(tp) as IOperation;
            if (op is not null)
            {
                Operations.Add(op);
            }
        }

        private void ChooseFiles()
        {
            Microsoft.Win32.OpenFileDialog op = new Microsoft.Win32.OpenFileDialog();
            op.Multiselect = true;
            if (op.ShowDialog() == true)
            {
                FileInfos.Clear();
                var fs = op.FileNames.Select(x => new FileInformation(x)).ToArray();
                foreach (var f in fs)
                {
                    FileInfos.Add(f);
                }
            }
        }
        private void RemoveOperation(IOperation? operation)
        {
            if (operation is not null)
            {
                Operations.Remove(operation);
            }
        }

        public class FileInformation
        {
            public FileInformation(string path)
            {
                FullFileName = path;
                FileName = System.IO.Path.GetFileName(path);
                NewName = System.IO.Path.GetFileName(path);
                DirectoryName = System.IO.Path.GetDirectoryName(path);
            }

            public string? DirectoryName { get; }
            public string FullFileName { get; }
            public string FileName { get; }
            public string GetFullNewName()
            {
                if (string.IsNullOrEmpty(DirectoryName)) { throw new Exception("文件夹路径为空"); }
                if (string.IsNullOrEmpty(NewName)) { throw new Exception("新文件名为空"); }
                return System.IO.Path.Combine(DirectoryName, NewName);
            }
            public string? NewName { get; set; }


        }
    }
}
