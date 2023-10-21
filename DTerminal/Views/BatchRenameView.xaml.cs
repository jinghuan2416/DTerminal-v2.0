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
    [View("批量重命名")]
    /// <summary>
    /// BatchRenameView.xaml 的交互逻辑
    /// </summary>
    public partial class BatchRenameView : UserControl
    {
        private BatchRenameViewModel viewModel;
        public BatchRenameView(BatchRenameViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            this.DataContext = viewModel;
        }
    }
}
