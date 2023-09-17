using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTerminal.Core;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DTerminal.ViewModels
{
    internal partial class FourierTransformViewModel : ObservableObject
    {
        public FourierTransformViewModel()
        {
            DFTExcuteCommand = new AsyncRelayCommand(DFTExcute);

            PlotModelSource.Axes.Add(new LinearAxis() { Position = AxisPosition.Left });
            PlotModelSource.Axes.Add(new LinearAxis() { Position = AxisPosition.Bottom });
            PlotModelSource.Series.Add(lineSeriesSource);

            PlotModelDFT.Axes.Add(new LinearAxis() { Position = AxisPosition.Left });
            PlotModelDFT.Axes.Add(new LinearAxis() { Position = AxisPosition.Bottom });
            PlotModelDFT.Series.Add(lineSeriesDFT);
        }


        private readonly LineSeries lineSeriesSource = new LineSeries();
        private readonly LineSeries lineSeriesDFT = new LineSeries();
        private double frequency = 1000;
        private int sampleCount = 0;
        private bool isDFT = true;

        public ObservableCollection<DPoint> SourceDatas { get; set; } = new ObservableCollection<DPoint>();
        public ObservableCollection<DPoint> DFTDatas { get; set; } = new ObservableCollection<DPoint>();
        public PlotModel PlotModelSource { get; set; } = new PlotModel();
        public PlotModel PlotModelDFT { get; set; } = new PlotModel();
        public AsyncRelayCommand DFTExcuteCommand { get; set; }
        public double Frequency { get => frequency; set =>this.SetProperty(ref frequency,value); }
        public int SampleCount { get => sampleCount; set => this.SetProperty(ref sampleCount,value); }
        public bool IsDFT { get => isDFT; set => this.SetProperty(ref isDFT, value); }


        [RelayCommand]
        private void PasteSourceDataFromClipboard()
        {
            var str = Clipboard.GetText();
            if (!string.IsNullOrEmpty(str))
            {
                SourceDatas.Clear(); lineSeriesSource.Points.Clear();
                var lines = str.Split("\r\n").ConvertTwoDouble();
                foreach (var (x, y) in lines)
                {
                    SourceDatas.Add(new DPoint(x, y)); lineSeriesSource.Points.Add(new DataPoint(x, y));
                }
                if (SourceDatas.Count > 2)
                {
                    this.Frequency = 1 / (SourceDatas[1].X - SourceDatas[0].X);
                }
                PlotModelSource.InvalidatePlot(true);
                this.SampleCount = SourceDatas.Count;
            }
        }

        [RelayCommand]
        private void CopyDFTDataToClipboard()
        {
            var ss = string.Join("\r\n", DFTDatas.Select(p => $"{p.X}\t{p.Y}").ToArray());
            Clipboard.SetDataObject(ss);
        }


        private async Task DFTExcute()
        {
            DFTDatas.Clear();
            lineSeriesDFT.Points.Clear();

            if (SourceDatas.Count > 10)
            {
                DPoint[] points = { };
                if (SourceDatas.Count % 2 > 0) { points = SourceDatas.Skip(1).ToArray(); }
                else { points = SourceDatas.ToArray(); }

                if (IsDFT)
                {
                    points = await Task.Run(() => points.DFT(Frequency));
                }
                else
                {
                    points = await Task.Run(() => points. FFT( Frequency));
                }


                foreach (var p in points)
                {
                    DFTDatas.Add(new DPoint(p.X, p.Y));
                    lineSeriesDFT.Points.Add(new DataPoint(p.X, p.Y));
                }
                PlotModelSource.InvalidatePlot(true);
                PlotModelDFT.InvalidatePlot(true);
            }
            else
            {
                throw new Exception("数据点太少，无法进行计算");
            }
        }


    }
}
