using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTerminal.Core.Solver.Optistruct;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTerminal.Models
{
    public class PunchDrawParameter : ObservableObject
    {
        public FrequencyResponsePunch FrequencyResponsePunch { get; set; }
        public bool IsShowX { get; set; } = true;
        public bool IsShowY { get; set; } = true;
        public bool IsShowZ { get; set; } = true;
        public bool IsShowStander { get; set; } = true;
        public double StanderVlaue { get; set; } = 10000;
        public bool IsYLogarithmic { get; set; } = false;
        public bool IsDynamicStiffness { get; set; } = false;
        public double FontSize { get; set; } = 12;
        public double MarkMinValue { get; set; } = 2000;
        public double MarkMaxValue { get; set; } = 10000;
        public bool IsMarkMin { get; set; } = false;
        public LineSeries? SelectLineSeries { get => selectLineSeries; set => this.SetProperty(ref selectLineSeries, value); }
        public TrackerHitResult TrackerHitResult { get => trackerHitResult; set => this.SetProperty(ref trackerHitResult, value); }
        public RelayCommand InvalidatePlotCommand { get; set; }

        public PlotModel PlotModel { get; } = new PlotModel();

        private LineSeries lineSeriesX = new LineSeries() { Title = "X" };
        private LineSeries lineSeriesY = new LineSeries() { Title = "Y" };
        private LineSeries lineSeriesZ = new LineSeries() { Title = "Z" };
        private LineSeries lineSeriesStander = new LineSeries() { Title = "Stander" };
        private LineSeries? selectLineSeries;
        private TrackerHitResult trackerHitResult = new TrackerHitResult();

        public PunchDrawParameter(FrequencyResponsePunch mFREQPunch)
        {
            FrequencyResponsePunch = mFREQPunch;
            this.InvalidatePlot();
            InvalidatePlotCommand = new RelayCommand(InvalidatePlot);

            PlotModel.TrackerChanged += PlotModel_TrackerChanged;
        }

        private void PlotModel_TrackerChanged(object? sender, TrackerEventArgs e)
        {
            if (e.HitResult != null)
            {
                TrackerHitResult = e.HitResult;
            }
        }

        public void InvalidatePlot()
        {
            PlotModel.Series.Clear();
            PlotModel.Axes.Clear();
            PlotModel.Legends.Clear();
            PlotModel.Annotations.Clear();

            PlotModel.Axes.Add(IsYLogarithmic ? new LogarithmicAxis() { Position = AxisPosition.Left, FontSize = FontSize } : new LinearAxis() { Position = AxisPosition.Left, FontSize = FontSize });
            PlotModel.Axes.Add(new LinearAxis() { Position = AxisPosition.Bottom, FontSize = FontSize });
            PlotModel.Legends.Add(new Legend() { LegendPosition = LegendPosition.RightBottom, FontSize = FontSize });
            PlotModel.DefaultFontSize = FontSize;

            if (IsShowX) { lineSeriesX.Points.Clear(); this.PlotModel.Series.Add(lineSeriesX); }
            if (IsShowY) { lineSeriesY.Points.Clear(); this.PlotModel.Series.Add(lineSeriesY); }
            if (IsShowZ) { lineSeriesZ.Points.Clear(); this.PlotModel.Series.Add(lineSeriesZ); }
            if (IsShowStander) { lineSeriesStander.Points.Clear(); this.PlotModel.Series.Add(lineSeriesStander); }

            if (FrequencyResponsePunch.Value is not null)
            {
                if (this.IsDynamicStiffness)
                {
                    if (FrequencyResponsePunch.ResultType == "DISPLACEMENTS")
                    {
                        foreach (var v in FrequencyResponsePunch.Value)
                        {
                            if (v.R1 is not null)
                            {
                                if (IsShowX) lineSeriesX.Points.Add(new OxyPlot.DataPoint(v.Frequency, 1 / v.R1.X));
                                if (IsShowY) lineSeriesY.Points.Add(new OxyPlot.DataPoint(v.Frequency, 1 / v.R1.Y));
                                if (IsShowZ) lineSeriesZ.Points.Add(new OxyPlot.DataPoint(v.Frequency, 1 / v.R1.Z));
                            }
                        }
                    }
                    else
                    {
                        foreach (var v in FrequencyResponsePunch.Value)
                        {
                            if (v.R1 is not null)
                            {
                                if (IsShowX) lineSeriesX.Points.Add(new OxyPlot.DataPoint(v.Frequency, 4 * Math.PI * Math.PI * v.Frequency * v.Frequency / v.R1.X));
                                if (IsShowY) lineSeriesY.Points.Add(new OxyPlot.DataPoint(v.Frequency, 4 * Math.PI * Math.PI * v.Frequency * v.Frequency / v.R1.Y));
                                if (IsShowZ) lineSeriesZ.Points.Add(new OxyPlot.DataPoint(v.Frequency, 4 * Math.PI * Math.PI * v.Frequency * v.Frequency / v.R1.Z));
                            }
                        }
                    }
                }
                else
                {
                    foreach (var v in FrequencyResponsePunch.Value)
                    {
                        if (v.R1 is not null)
                        {
                            if (IsShowX) lineSeriesX.Points.Add(new OxyPlot.DataPoint(v.Frequency, v.R1.X));
                            if (IsShowY) lineSeriesY.Points.Add(new OxyPlot.DataPoint(v.Frequency, v.R1.Y));
                            if (IsShowZ) lineSeriesZ.Points.Add(new OxyPlot.DataPoint(v.Frequency, v.R1.Z));
                        }
                    }
                }
            }
            double max = new double[] { lineSeriesX.Points.Max(x => x.X), lineSeriesY.Points.Max(x => x.X), lineSeriesZ.Points.Max(x => x.X) }.Max();
            lineSeriesStander.Points.Add(new DataPoint(0, StanderVlaue));
            lineSeriesStander.Points.Add(new DataPoint(max, StanderVlaue));

            if (IsMarkMin)
            {
                List<DataPoint> points = new List<DataPoint>();
                if (IsShowX) { points.AddRange(lineSeriesX.Points.Where(p => p.X >= MarkMinValue && p.X <= MarkMaxValue)); }
                if (IsShowY) { points.AddRange(lineSeriesY.Points.Where(p => p.X >= MarkMinValue && p.X <= MarkMaxValue)); }
                if (IsShowZ) { points.AddRange(lineSeriesZ.Points.Where(p => p.X >= MarkMinValue && p.X <= MarkMaxValue)); }
                var ps = points.OrderBy(p => p.Y).ToArray();
                if (ps.Length > 0)
                {
                    var p = ps.First();
                    OxyPlot.Annotations.PointAnnotation mark = new OxyPlot.Annotations.PointAnnotation()
                    {
                        X = p.X,
                        Y = p.Y,
                        Size = 5,
                        Fill = OxyPlot.OxyColor.FromRgb(255, 0, 0),
                        Text = $"X = {Math.Round(p.X, 2)} Hz\r\nY = {Math.Round(p.Y, 2)}",
                        FontSize = this.FontSize,
                    };
                    this.PlotModel.Annotations.Add(mark);
                }
            }

            this.PlotModel.InvalidatePlot(true);
        }

        public void CopySettingFrom(PunchDrawParameter parameter)
        {
            this.IsShowX = parameter.IsShowX;
            this.IsShowY = parameter.IsShowY;
            this.IsShowZ = parameter.IsShowZ;
            this.IsShowStander = parameter.IsShowStander;
            this.StanderVlaue = parameter.StanderVlaue;
            this.IsYLogarithmic = parameter.IsYLogarithmic;
            this.IsDynamicStiffness = parameter.IsDynamicStiffness;
            this.FontSize = parameter.FontSize;
            this.MarkMinValue = parameter.MarkMinValue;
            this.MarkMaxValue = parameter.MarkMaxValue;
            this.IsMarkMin = parameter.IsMarkMin;
            this.InvalidatePlot();

        }
    }
}
