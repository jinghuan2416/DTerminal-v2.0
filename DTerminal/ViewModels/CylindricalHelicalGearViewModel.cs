using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTerminal.Core.Gear;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTerminal.ViewModels
{
    internal partial class CylindricalHelicalGearViewModel : ObservableObject
    {
        private CylindricalHelicalGear cylindricalHelicalGear = new CylindricalHelicalGear();
        private MeshingForce meshingForce = new MeshingForce();
        private double torsion = 4500000;
        private string? result = string.Empty;

        public CylindricalHelicalGear CylindricalHelicalGear { get => cylindricalHelicalGear; set => this.SetProperty(ref cylindricalHelicalGear,value); }
        public MeshingForce MeshingForce { get => meshingForce; set => this.SetProperty(ref meshingForce, value); }
        public double Torsion { get => torsion; set => this.SetProperty(ref torsion, value); }
        public string? Result { get => result; set => this.SetProperty(ref result, value); }

        [RelayCommand]
        private void Compute()
        {
            this.MeshingForce = this.CylindricalHelicalGear.GetMeshingForce(this.Torsion);
        }
    }
}
