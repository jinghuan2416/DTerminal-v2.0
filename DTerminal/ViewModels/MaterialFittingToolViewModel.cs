using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTerminal.Core.Material;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using System.Linq;
using System.Text;
using System.Windows;

namespace DTerminal.ViewModels
{
    internal partial class MaterialFittingToolViewModel : ObservableObject
    {
        public double Strain1 { get => strain1; set => this.SetProperty(ref strain1, value); }
        public double Stress1 { get => stress1; set => this.SetProperty(ref stress1, value); }
        public double Strain2 { get => strain2; set => this.SetProperty(ref strain2, value); }
        public double Stress2 { get => stress2; set => this.SetProperty(ref stress2, value); }
        public double ElasticModulus { get => elasticModulus; set => this.SetProperty(ref elasticModulus, value); }
        public bool IsTrueStrainStress { get => isTrueStrainStress; set => this.SetProperty(ref isTrueStrainStress, value); }
        public double TruncationStrain { get => truncationStrain; set => this.SetProperty(ref truncationStrain, value); }
        public double MaxStress { get => maxStress; set => this.SetProperty(ref maxStress, value); }
        public StressStrain[] RambergOsgoodStrainStress { get => rambergOsgoodStrainStress; set => this.SetProperty(ref rambergOsgoodStrainStress, value); }
        public StressStrain[] RambergOsgoodPlasticStrainStress { get => rambergOsgoodPlasticStrainStress; set => this.SetProperty(ref rambergOsgoodPlasticStrainStress, value); }
        public PlotModel PlotModel { get => plotModel; set => this.SetProperty(ref plotModel, value); }
        public string Status { get => status; set => this.SetProperty(ref status, value); }

        private readonly RambergOsgood rambergOsgood = new RambergOsgood();
        private readonly LineSeries lineSeriesRO = new LineSeries() { Title = "真实 应力-应变 (RambergOsgood)" };
        private readonly LineSeries lineSeriesROPlastic = new LineSeries() { Title = "真实 应力-塑性应变 (RambergOsgood)" };
        private double strain1 = 0.002;
        private double stress1 = 370;
        private double strain2 = 0.03;
        private double stress2 = 600;
        private double elasticModulus = 174000;
        private bool isTrueStrainStress = false;
        private double truncationStrain = 1e-5;
        private double maxStress = 1000;
        private StressStrain[] rambergOsgoodStrainStress = { };
        private StressStrain[] rambergOsgoodPlasticStrainStress = { };
        private PlotModel plotModel = new PlotModel();
        private string status = string.Empty;

        public MaterialFittingToolViewModel()
        {
            this.PlotModel.Axes.Add(new LinearAxis() { Position = AxisPosition.Left });
            this.PlotModel.Axes.Add(new LinearAxis() { Position = AxisPosition.Bottom });
            this.PlotModel.Legends.Add(new Legend() { LegendPosition = LegendPosition.RightBottom });
            this.PlotModel.Series.Add(this.lineSeriesRO);
            this.PlotModel.Series.Add(this.lineSeriesROPlastic);
        }

        [RelayCommand]
        private void Generate()
        {
            StringBuilder sb = new StringBuilder();
            rambergOsgood.ElasticModulus = ElasticModulus;

            if (IsTrueStrainStress)
            {
                rambergOsgood.P1 = new StressStrain(Strain1, Stress1);
                rambergOsgood.P2 = new StressStrain(Strain2, Stress2);
                Status = string.Empty;
            }
            else
            {
                rambergOsgood.P1 = new StressStrain(Strain1, Stress1).ConvertToTrueIfEngineering();
                rambergOsgood.P2 = new StressStrain(Strain2, Stress2).ConvertToTrueIfEngineering();
                Status = $"真实应变：{rambergOsgood.P2.Strain}\t真实应力：{rambergOsgood.P2.Stress}";
            }

            lineSeriesRO.Points.Clear();

            RambergOsgoodStrainStress = rambergOsgood.GenerateCurve(MaxStress);
            lineSeriesRO.Points.Clear();
            lineSeriesRO.Points.AddRange(RambergOsgoodStrainStress.Select(x => new DataPoint(x.Strain, x.Stress)));

            RambergOsgoodPlasticStrainStress = rambergOsgood.GeneratePlasticCurve(MaxStress, TruncationStrain);
            lineSeriesROPlastic.Points.Clear();
            lineSeriesROPlastic.Points.AddRange(RambergOsgoodPlasticStrainStress.Select(x => new DataPoint(x.Strain, x.Stress)));


            this.PlotModel.InvalidatePlot(true);
        }

        [RelayCommand]
        private void CopyToClipboardByInp()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("*MATERIAL, NAME=Material");
            sb.AppendLine("*ELASTIC, TYPE = ISOTROPIC");
            sb.AppendLine("210000.0   ,0.3       ,0.0    ");
            sb.AppendLine("*PLASTIC");
            foreach (var p in RambergOsgoodPlasticStrainStress)
            {
                sb.AppendLine(string.Join(",", p.Stress, p.Strain, 0));
            }
            Clipboard.SetDataObject(sb.ToString());
        }

        [RelayCommand]
        private void CopyToClipboard()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("塑性应变\t应力");
            sb.AppendLine(string.Join("\r\n", RambergOsgoodPlasticStrainStress.Select(p => $"{p.Strain}\t{p.Stress}").ToArray()));
            Clipboard.SetDataObject(sb.ToString());
        }
        [RelayCommand]
        private void CopyToClipboard2()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("应力\t塑性应变");
            sb.AppendLine(string.Join("\r\n", RambergOsgoodPlasticStrainStress.Select(p => $"{p.Stress}\t{p.Strain}").ToArray()));
            Clipboard.SetDataObject(sb.ToString());
        }

        [RelayCommand]
        private void CopyRambergOsgoodStrainStressToClipboard()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("应变\t应力");
            sb.AppendLine(string.Join("\r\n", RambergOsgoodStrainStress.Select(p => $"{p.Strain}\t{p.Stress}").ToArray()));
            Clipboard.SetDataObject(sb.ToString());
        }
    }
}
