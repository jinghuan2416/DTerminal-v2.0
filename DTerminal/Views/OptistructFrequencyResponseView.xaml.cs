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
    [View("频率响应分析(Optistruct)")]
    /// <summary>
    /// OptistructDynamicStiffnessView.xaml 的交互逻辑
    /// </summary>
    public partial class OptistructFrequencyResponseView : UserControl
    {
        public OptistructFrequencyResponseView(OptistructFrequencyResponseViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
