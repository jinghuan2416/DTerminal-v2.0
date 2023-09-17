using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTerminal.Models;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace DTerminal.ViewModels
{
    internal partial class HarmonicWaveViewModel : ObservableObject
    {
        public ObservableCollection<HarmonicParameter> HarmonicParameters { get; set; } = new ObservableCollection<HarmonicParameter>();
        public ObservableCollection<Point> HarmonicPoints { get; set; } = new ObservableCollection<Point>();
        public PlotModel PlotModel { get; } = new PlotModel();

        private readonly LineSeries lineSeries = new LineSeries();
        private HarmonicParameter? selectedHarmonicParameter;
        private double star = 0;
        private double end = 10;
        private double step = 0.01;
        private double tolerance = 1E-8;
        private double sampleCount = 1000;
        public HarmonicParameter? SelectedHarmonicParameter { get => selectedHarmonicParameter; set => this.SetProperty(ref selectedHarmonicParameter, value); }
        public double Star { get => star; set => this.SetProperty(ref star, value); }
        public double End { get => end; set => this.SetProperty(ref end, value); }
        public double Step { get => step; set => this.SetProperty(ref step, value); }
        public double Tolerance { get => tolerance = 1E-8; set => this.SetProperty(ref tolerance , value); }
        public double SampleCount { get => sampleCount; set => this.SetProperty(ref sampleCount, value); }


        public HarmonicWaveViewModel()
        {
            HarmonicParameters.Add(new HarmonicParameter());
            PlotModel.Axes.Add(new LinearAxis() { Position = AxisPosition.Left });
            PlotModel.Axes.Add(new LinearAxis() { Position = AxisPosition.Bottom });
            PlotModel.Series.Add(lineSeries);
        }

        [RelayCommand]
        private void AddItem()
        {
            HarmonicParameters.Add(new HarmonicParameter());
            SelectedHarmonicParameter = null;
        }

        [RelayCommand]
        private void RemoveItem()
        {
            if (SelectedHarmonicParameter is not null)
            {
                HarmonicParameters.Remove(SelectedHarmonicParameter);
                SelectedHarmonicParameter = null;
            }
        }

        [RelayCommand]
        private void ClearItems()
        {
            HarmonicParameters.Clear();
            SelectedHarmonicParameter = null;
        }


        [RelayCommand]
        private void Generate()
        {
            this.lineSeries.Points.Clear();
            this.HarmonicPoints.Clear();
            for (double i = Star; i < End; i = (double)((decimal)i + (decimal)Step))
            {
                double x = i;
                double y = 0;
                foreach (var p in HarmonicParameters)
                {
                    double temp = Math.Sin((i * p.Frequency * 2 * Math.PI + p.Phase * 2 * Math.PI)) * p.Amplitude + p.Offset;
                    if (Math.Abs(temp) < Tolerance) { temp = 0; }
                    y += temp;

                }
                this.lineSeries.Points.Add(new DataPoint(x, y));
            }
            foreach (var p in this.lineSeries.Points.Select(p => new Point(p.X, p.Y)))
            {
                HarmonicPoints.Add(p);
            }
            SampleCount = HarmonicPoints.Count;
            this.PlotModel.InvalidatePlot(true);
        }

        [RelayCommand]
        private void CopyDataToClipboard()
        {
            var ss = string.Join("\r\n", HarmonicPoints.Select(p => $"{p.X}\t{p.Y}").ToArray());
            Clipboard.SetDataObject(ss);
        }
    }
}
