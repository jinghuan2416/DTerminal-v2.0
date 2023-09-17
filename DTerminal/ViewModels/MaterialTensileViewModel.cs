using CommunityToolkit.Mvvm.ComponentModel;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot;
using CommunityToolkit.Mvvm.Input;
using OxyPlot.Series;
using System.Text;
using System.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using DTerminal.Core;
using DTerminal.Core.Material;

namespace DTerminal.ViewModels
{
    internal partial class MaterialTensileViewModel : ObservableObject
    {
        private LineSeries lineSeriesEngineering = new LineSeries() { Title = "工程 应力-应变" };
        private LineSeries lineSeriesTrue = new LineSeries() { Title = "真实 应力-应变" };
        private LineSeries lineSeriesTruePlastic = new LineSeries() { Title = "真实 应力-塑性应变" };
        private LineSeries lineSeriesRO = new LineSeries() { Title = "真实 应力-应变 (RambergOsgood)" };
        private LineSeries lineSeriesROPlastic = new LineSeries() { Title = "真实 应力-塑性应变 (RambergOsgood)" };
        private bool isAverage = true;
        private double diameter = 10;
        private double gaugeLength = 50;
        private double displacementScale = 1;
        private double forceScale = 1000;
        private double elasticModulus = 210000;
        private double truncationStrain = 1E-5;
        private double ro_N = 0.1;
        private double ro_H = 1000;
        private double tolerance = 1e-8;
        private int maxIterations = 1000;
        private int maxStress = 1000;
        private TensileData[] tensileDataCollection = { };
        private StressStrain[] truePlasticCollection = { };
        private StressStrain[] rambergOsgoodStrainStress = { };
        private StressStrain[] rambergOsgoodPlasticStrainStress = { };

        public bool IsAverage { get => isAverage; set => this.SetProperty(ref isAverage,value); }
        public double Diameter { get => diameter; set => this.SetProperty(ref diameter, value); }
        public double GaugeLength { get => gaugeLength; set => this.SetProperty(ref gaugeLength, value); }
        public double DisplacementScale { get => displacementScale; set => this.SetProperty(ref displacementScale, value); }
        public double ForceScale { get => forceScale; set => this.SetProperty(ref forceScale, value); }
        public double ElasticModulus { get => elasticModulus; set => this.SetProperty(ref elasticModulus, value); }
        public double TruncationStrain { get => truncationStrain; set => this.SetProperty(ref truncationStrain, value); }
        public double Ro_N { get => ro_N; set => this.SetProperty(ref ro_N, value); }
        public double Ro_H { get => ro_H; set => this.SetProperty(ref ro_H, value); }
        public double Tolerance { get => tolerance; set => this.SetProperty(ref tolerance, value); }
        public int MaxIterations { get => maxIterations; set => this.SetProperty(ref maxIterations, value); }
        public int MaxStress { get => maxStress; set => this.SetProperty(ref maxStress, value); }
        public TensileData[] TensileDataCollection { get => tensileDataCollection; set => this.SetProperty(ref tensileDataCollection, value); }
        public StressStrain[] TruePlasticCollection { get => truePlasticCollection; set => this.SetProperty(ref truePlasticCollection, value); }
        public StressStrain[] RambergOsgoodStrainStress { get => rambergOsgoodStrainStress; set => this.SetProperty(ref rambergOsgoodStrainStress, value); }
        public StressStrain[] RambergOsgoodPlasticStrainStress { get => rambergOsgoodPlasticStrainStress; set => this.SetProperty(ref rambergOsgoodPlasticStrainStress, value); }
        public PlotModel PlotModel { get; } = new PlotModel();

        public MaterialTensileViewModel()
        {
            PlotModel.Axes.Add(new LinearAxis() { Position = AxisPosition.Left });
            PlotModel.Axes.Add(new LinearAxis() { Position = AxisPosition.Bottom });
            PlotModel.Legends.Add(new Legend() { LegendPosition = LegendPosition.RightBottom });
            PlotModel.Series.Add(lineSeriesEngineering);
            PlotModel.Series.Add(lineSeriesTrue);
            PlotModel.Series.Add(lineSeriesTruePlastic);
            PlotModel.Series.Add(lineSeriesRO);
            PlotModel.Series.Add(lineSeriesROPlastic);
        }

        [RelayCommand]
        private void OffsetZero()
        {
            var ps = TensileDataCollection.ToArray();
            TensileDataCollection = new TensileData[] { };
        }

        [RelayCommand]
        private void ConvertToSS()
        {
            TensileDataCollection = TensileDataCollection.Select(p =>
            {
                StressStrain strainStress = new() { Strain = p.Displacement / GaugeLength, Stress = p.Force / (Math.PI * (Diameter / 2.0) * (Diameter / 2.0)) };
                var data = new TensileData()
                {
                    Displacement = p.Displacement,
                    Force = p.Force,
                    Engineering = new StressStrain(strainStress.Strain, strainStress.Stress)
                };
                strainStress = strainStress.ConvertToTrueIfEngineering();
                data.True = new StressStrain(strainStress.Strain, strainStress.Stress);
                return data;
            }).ToArray();

            lineSeriesEngineering.Points.Clear();
            lineSeriesTrue.Points.Clear();

            lineSeriesEngineering.Points.AddRange(TensileDataCollection.Select(x => new DataPoint(x.Engineering.Strain, x.Engineering.Stress)));
            lineSeriesTrue.Points.AddRange(TensileDataCollection.Select(x => new DataPoint(x.True.Strain, x.True.Stress)));

            PlotModel.InvalidatePlot(true);
        }

        [RelayCommand]
        private void ConvertToPlastic()
        {
            var rs = TensileDataCollection.Select(x => new StressStrain(x.True.Strain, x.True.Stress).RemoveElasticity(ElasticModulus)).Where(x => x.Strain > TruncationStrain).ToArray();
            lineSeriesTruePlastic.Points.Clear();
            if (rs.Length > 2)
            {
                var line = MathNet.Numerics.Interpolation.LinearSpline.Interpolate(new double[] { rs[0].Strain, rs[1].Strain }, new double[] { rs[0].Stress, rs[1].Stress });
                lineSeriesTruePlastic.Points.Add(new DataPoint(0, line.Interpolate(0)));
            }
            lineSeriesTruePlastic.Points.AddRange(rs.Select(x => new DataPoint(x.Strain, x.Stress)));
            TruePlasticCollection = lineSeriesTruePlastic.Points.Select(p => new StressStrain(p.X, p.Y)).ToArray();
            PlotModel.InvalidatePlot(true);
        }

        [RelayCommand]
        private void RambergOsgoodFitting()
        {
            try
            {
                double[] xs = TruePlasticCollection.Skip(1).Select(x => Math.Log10(x.Strain)).ToArray();
                double[] ys = TruePlasticCollection.Skip(1).Select(x => Math.Log10(x.Stress)).ToArray();
                var (p0, p1) = MathNet.Numerics.Fit.Curve(xs, ys, (p0, p1, x) => p0 * x + p1, Ro_N, Ro_H, Tolerance, MaxIterations);
                Ro_N = p0;
                Ro_H = Math.Pow(10, p1);

                lineSeriesRO.Points.Clear();
                List<DataPoint> temps = new List<DataPoint>();
                for (int i = 0; i <= MaxStress; i++)
                {
                    var s = new StressStrain() { Strain = i / ElasticModulus + Math.Pow(i / Ro_H, 1 / Ro_N), Stress = i };
                    lineSeriesRO.Points.Add(new DataPoint(s.Strain, s.Stress));
                    s = s.RemoveElasticity(ElasticModulus);
                    if (s.Strain > TruncationStrain) { temps.Add(new DataPoint(s.Strain, s.Stress)); }
                }
                RambergOsgoodStrainStress = lineSeriesRO.Points.Select(p => new StressStrain(p.X, p.Y)).ToArray();

                lineSeriesROPlastic.Points.Clear();
                if (temps.Count > 2)
                {
                    var line = MathNet.Numerics.Interpolation.LinearSpline.Interpolate(new double[] { temps[0].X, temps[1].X }, new double[] { temps[0].Y, temps[1].Y });
                    lineSeriesROPlastic.Points.Add(new DataPoint(0, line.Interpolate(0)));
                }
                lineSeriesROPlastic.Points.AddRange(temps);
                RambergOsgoodPlasticStrainStress = lineSeriesROPlastic.Points.Select(p => new StressStrain(p.X, p.Y)).ToArray();

                PlotModel.InvalidatePlot(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        [RelayCommand]
        private void CopyTruePlastictoClipboard()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("真实塑性应变\t真实应力");
            foreach (var p in TruePlasticCollection)
            {
                sb.AppendLine(string.Join("\t", p.Strain, p.Stress));
            }
            System.Windows.Clipboard.SetDataObject(sb.ToString());
        }

        [RelayCommand]
        private void CopyTensileDatatoClipboard()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("位移\t力\t工程应变\t工程应力\t真实应变\t真实应力");
            foreach (var p in TensileDataCollection)
            {
                sb.AppendLine(string.Join("\t", p.Displacement, p.Force, p.Engineering.Strain, p.Engineering.Stress, p.True.Strain, p.True.Stress));
            }
            System.Windows.Clipboard.SetDataObject(sb.ToString());
        }
        [RelayCommand]
        private void CopyRambergOsgoodStrainStressToClipboard()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Ramberg-Osgood模型");
            sb.AppendLine("真实塑性应变\t真实应力");
            foreach (var p in RambergOsgoodStrainStress)
            {
                sb.AppendLine(string.Join("\t", p.Strain, p.Stress));
            }
            System.Windows.Clipboard.SetDataObject(sb.ToString());
        }
        [RelayCommand]
        private void CopyRambergOsgoodPlasticStrainStressToClipboard()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Ramberg-Osgood模型");
            sb.AppendLine("真实塑性应变\t真实应力");
            foreach (var p in RambergOsgoodPlasticStrainStress)
            {
                sb.AppendLine(string.Join("\t", p.Strain, p.Stress));
            }
            System.Windows.Clipboard.SetDataObject(sb.ToString());
        }

        [RelayCommand]
        private void PasteDispForceFromClipboard()
        {
            var str = Clipboard.GetText();
            if (!string.IsNullOrEmpty(str))
            {
                var ps = str.Split("\r\n").ConvertTwoDouble().Select(p => new { Dispment = p.Item1, Force = p.Item2 });
                if (IsAverage)
                {
                    ps = ps.GroupBy(x => x.Dispment).
                        Select(x => new { Dispment = x.Key, Force = x.Average(xx => xx.Force) }).ToArray();
                }
                TensileDataCollection = ps.Select(p => new TensileData() { Displacement = p.Dispment * DisplacementScale, Force = p.Force * ForceScale }).ToArray();
            }
        }
        [RelayCommand]
        private void ReadDispForceFromClipboard()
        {
            var op = new Microsoft.Win32.OpenFileDialog();
            if (op.ShowDialog() == true)
            {
                var str = System.IO.File.ReadAllText(op.FileName);
                if (!string.IsNullOrEmpty(str))
                {
                    var ps = str.Split("\r\n").ConvertTwoDouble().Select(p => new { Dispment = p.Item1, Force = p.Item2 });
                    if (IsAverage)
                    {
                        ps = ps.GroupBy(x => x.Dispment).
                            Select(x => new { Dispment = x.Key, Force = x.Average(xx => xx.Force) }).ToArray();
                    }
                    TensileDataCollection = ps.Select(p => new TensileData() { Displacement = p.Dispment * DisplacementScale, Force = p.Force * ForceScale }).ToArray();
                }
            }

        }
    }
}
