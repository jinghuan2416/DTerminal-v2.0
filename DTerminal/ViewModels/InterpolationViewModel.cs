using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTerminal.Core;
using DTerminal.Models;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace DTerminal.ViewModels
{
    internal partial class InterpolationViewModel : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
    {
        public ObservableCollection<DPoint> SourceData { get; } = new ObservableCollection<DPoint>();
        public ObservableCollection<DPoint> TargetData { get; } = new ObservableCollection<DPoint>();

        public PlotModel PlotModel { get; } = new PlotModel();

        [ObservableProperty]
        private SequenceParameters sequenceParameters = new SequenceParameters() { Star = 1, End = 1000, Step = 1 };

        private readonly LineSeries lineSeriesSourceData = new LineSeries() { Title = "源数据" };
        private readonly LineSeries lineSeriesTargetData = new LineSeries() { Title = "目标数据" };
        public InterpolationViewModel()
        {
            PlotModel.Axes.Add(new LinearAxis() { Position = AxisPosition.Left });
            PlotModel.Axes.Add(new LinearAxis() { Position = AxisPosition.Bottom });
            PlotModel.Legends.Add(new Legend() { LegendPosition = LegendPosition.RightBottom });
            PlotModel.Series.Add(lineSeriesSourceData);
            PlotModel.Series.Add(lineSeriesTargetData);
        }



        [RelayCommand]
        private void ReadSourceData()
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                var ss = System.IO.File.ReadAllLines(ofd.FileName);
                SourceData.Clear();
                lineSeriesSourceData.Points.Clear();
                var lines = ss.ConvertTwoDouble();
                foreach (var (x, y) in lines)
                {
                    SourceData.Add(new DPoint(x, y));
                    lineSeriesSourceData.Points.Add(new DataPoint(x, y));
                }
                PlotModel.InvalidatePlot(true);
            }
        }
        [RelayCommand]
        private void ReadTargetData()
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                var ss = System.IO.File.ReadAllLines(ofd.FileName);
                TargetData.Clear();
                lineSeriesTargetData.Points.Clear();
                var lines = ss.ConvertOneDouble();
                foreach (var x in lines)
                {
                    TargetData.Add(new DPoint(x, 0));
                    lineSeriesTargetData.Points.Add(new DataPoint(x, 0));
                }
                PlotModel.InvalidatePlot(true);
            }
        }

        [RelayCommand]
        private void PasteSourceDataFromClipboard()
        {
            var str = Clipboard.GetText();
            if (!string.IsNullOrEmpty(str))
            {
                SourceData.Clear(); lineSeriesSourceData.Points.Clear();
                var lines = str.Split("\r\n").ConvertTwoDouble();
                foreach (var (x, y) in lines)
                {
                    SourceData.Add(new DPoint(x, y)); lineSeriesSourceData.Points.Add(new DataPoint(x, y));
                }
                PlotModel.InvalidatePlot(true);
            }
        }

        [RelayCommand]
        private void ExchangeSourceData()
        {
            var ps = SourceData.ToArray();
            SourceData.Clear(); lineSeriesSourceData.Points.Clear();
            foreach (var p in ps)
            {
                SourceData.Add(new DPoint(p.Y, p.X)); lineSeriesSourceData.Points.Add(new DataPoint(p.Y, p.X));
            }
            PlotModel.InvalidatePlot(true);
        }

        [RelayCommand]
        private void PasteTargetDataFromClipboard()
        {
            var str = Clipboard.GetText();
            if (!string.IsNullOrEmpty(str))
            {
                TargetData.Clear(); lineSeriesTargetData.Points.Clear();
                var lines = str.Split("\r\n").ConvertOneDouble();
                foreach (var x in lines)
                {
                    TargetData.Add(new DPoint(x, 0)); lineSeriesTargetData.Points.Add(new DataPoint(x, 0));
                }
                PlotModel.InvalidatePlot(true);
            }
        }

        [RelayCommand]
        private void GenerateTargetSequence()
        {
            TargetData.Clear();
            lineSeriesTargetData.Points.Clear();
            for (double i = SequenceParameters.Star; i <= SequenceParameters.End; i += SequenceParameters.Step)
            {
                TargetData.Add(new DPoint(i, 0)); lineSeriesTargetData.Points.Add(new DataPoint(i, 0));
            }
            PlotModel.InvalidatePlot(true);
        }

        [RelayCommand]
        private void ExchangeTargetData()
        {
            var ps = TargetData.ToArray();
            TargetData.Clear();
            lineSeriesTargetData.Points.Clear();
            foreach (var p in ps)
            {
                TargetData.Add(new DPoint(p.Y, p.X)); lineSeriesTargetData.Points.Add(new DataPoint(p.Y, p.X));
            }
            PlotModel.InvalidatePlot(true);
        }

        [RelayCommand]
        private void LinearInterpolation()
        {
            if (SourceData.Count > 2)
            {
                var source = SourceData.OrderBy(x => x.X);
                var xs = source.Select(x => x.X);
                var ys = source.Select(x => x.Y);
                var lp = MathNet.Numerics.Interpolation.LinearSpline.Interpolate(xs, ys);

                var targe = TargetData.ToArray();
                TargetData.Clear();
                lineSeriesTargetData.Points.Clear();
                foreach (var p in targe)
                {
                    DPoint point = new(p.X, lp.Interpolate(p.X));
                    TargetData.Add(point);
                    lineSeriesTargetData.Points.Add(new DataPoint(point.X, point.Y));
                }
                PlotModel.InvalidatePlot(true);
            }

        }

        [RelayCommand]
        private void CubicSplineInterpolate()
        {
            if (SourceData.Count > 5)
            {
                var source = SourceData.OrderBy(x => x.X);
                var xs = source.Select(x => x.X);
                var ys = source.Select(x => x.Y);
                var lp = MathNet.Numerics.Interpolation.CubicSpline.InterpolateNatural(xs, ys);
                var targe = TargetData.ToArray();
                TargetData.Clear();
                lineSeriesTargetData.Points.Clear();
                foreach (var p in targe)
                {
                    DPoint point = new(p.X, lp.Interpolate(p.X));
                    TargetData.Add(point);
                    lineSeriesTargetData.Points.Add(new DataPoint(point.X, point.Y));
                }
                PlotModel.InvalidatePlot(true);
            }
        }

        [RelayCommand]
        private void CopyTargetDataToClipboard()
        {
            var ss = string.Join("\r\n", TargetData.Select(p => $"{p.X}\t{p.Y}").ToArray());
            Clipboard.SetDataObject(ss);

        }
    }
}
