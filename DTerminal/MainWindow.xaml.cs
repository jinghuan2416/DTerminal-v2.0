using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DTerminal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ColorZone_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.DrawerHost.OpenMode == MaterialDesignThemes.Wpf.DrawerHostOpenMode.Standard)
            {

            }
            else
            {
                this.DrawerHost.IsLeftDrawerOpen = false;
            }
        }

        private void DrawerHost_DrawerClosing(object sender, MaterialDesignThemes.Wpf.DrawerClosingEventArgs e)
        {
            OpenToggleButton.IsChecked = false;
            OpenToggleButton.Visibility = Visibility.Visible;
        }

        private void DrawerHost_DrawerOpened(object sender, MaterialDesignThemes.Wpf.DrawerOpenedEventArgs e)
        {
            OpenToggleButton.Visibility = Visibility.Collapsed;
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void ButtonMin_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }


}
