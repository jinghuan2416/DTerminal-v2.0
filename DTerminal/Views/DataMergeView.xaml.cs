using DTerminal.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DTerminal.Views
{
    [View("数据合并")]
    /// <summary>
    /// DataMergeView.xaml 的交互逻辑
    /// </summary>
    public partial class DataMergeView : UserControl
    {
        private DataMergeViewModel viewModel;
        public DataMergeView(DataMergeViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            this.DataContext = viewModel;
        }

        private void GroupBox_PreviewDrop(object sender, DragEventArgs e)
        {
            var paths = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (paths is not null)
            {
                var dics = paths.Where(x => System.IO.Directory.Exists(x)).ToArray();
                var files= paths.Where(x => System.IO.File.Exists(x)).ToList();
                foreach (var dic in dics)
                {
                    files.AddRange(System.IO.Directory.GetFiles(dic));
                }
                CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(files.ToArray(), nameof(DataMergeViewModel));
            }
        }
    }
}
