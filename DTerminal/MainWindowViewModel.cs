using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTerminal.Models;
using DTerminal.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DTerminal
{
    public partial class MainWindowViewModel : ObservableRecipient
    {
        private NavMenuItem? selectNavMenuItem;
        private NavMenuItem[] navMenuItems = { };
        private object? content;
        private int selectNavMenuIndex;
        private string userTitle= "DTerminal";

        public string UserTitle { get => userTitle; set => this.SetProperty(ref userTitle, value); }
        public NavMenuItem[] NavMenuItems { get => navMenuItems; set => this.SetProperty(ref navMenuItems, value); }
        public int SelectNavMenuIndex { get => selectNavMenuIndex; set => this.SetProperty(ref selectNavMenuIndex, value); }
        public RelayCommand<NavMenuItem> OpenCommand { get; set; }
        public NavMenuItem? SelectNavMenuItem
        {
            get => selectNavMenuItem; 
            set
            {
                if (this.SetProperty(ref selectNavMenuItem, value) && value is NavMenuItem item)
                {
                    this.Content = App.ServiceProvider?.GetService(item.Type);
                    if (this.Content?.GetType().GetCustomAttribute<ViewAttribute>() is ViewAttribute attribute)
                    {
                        this.UserTitle = attribute.Title;
                    }
                    else
                    {
                        this.UserTitle = "DTerminal";
                    }
                    
                }
            }
        }
        public object? Content { get => content; set => this.SetProperty(ref content, value); }
        public MainWindowViewModel()
        {
            this.NavMenuItems = System.Reflection.Assembly.GetExecutingAssembly().GetNavMenuItems();
            OpenCommand = new RelayCommand<NavMenuItem>(Open);
        }



        [RelayCommand]
        private void MovePrev()
        {
            if (SelectNavMenuIndex > 0)
                SelectNavMenuIndex -= 1;
        }

        [RelayCommand]
        private void MoveNext()
        {
            if (SelectNavMenuIndex < NavMenuItems.Length - 1)
                SelectNavMenuIndex += 1;
        }

        [RelayCommand]
        private void Home()
        {
            SelectNavMenuIndex = 0;
        }

        private void Open(NavMenuItem? item)
        {
            this.SelectNavMenuItem = item;
        }
    }
}
