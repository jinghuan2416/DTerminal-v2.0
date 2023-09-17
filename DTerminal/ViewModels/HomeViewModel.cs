using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTerminal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTerminal.ViewModels
{
    internal class HomeViewModel : ObservableObject
    {
        public string Title { get; set; } = "HELLO WPF";

        private NavMenuItem[] navMenuItems = { };
        public NavMenuItem[] NavMenuItems { get => navMenuItems; set => this.SetProperty(ref navMenuItems, value); }
        public HomeViewModel()
        {
            this.NavMenuItems = System.Reflection.Assembly.GetExecutingAssembly().GetNavMenuItems().Skip(1).ToArray();

        }

    }
}
