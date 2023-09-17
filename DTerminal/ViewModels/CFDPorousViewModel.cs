using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTerminal.Core;
using DTerminal.Core.Solver.CFD;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DTerminal.ViewModels
{
    internal partial class CFDPorousViewModel : ObservableObject
    {
        public StarCCMPorousParameter StarCCMPorousParameter { get => starCCMPorousParameter; set => this.SetProperty(ref starCCMPorousParameter, value); }
        public FluentPorousParameter FluentPorousParameter { get => fluentPorousParameter; set => this.SetProperty(ref fluentPorousParameter, value); }
        /// <summary>
        /// 一次系数
        /// </summary>
        public double P0 { get => p0; set => this.SetProperty(ref p0, value); }
        /// <summary>
        /// 二次系数
        /// </summary>
        public double P1 { get => p1; set => this.SetProperty(ref p1, value); }
        public double Denstity { get => denstity; set => this.SetProperty(ref denstity, value); }
        public double DynamicViscosity { get => dynamicViscosity; set => this.SetProperty(ref dynamicViscosity, value); }
        public double Length { get => length; set => this.SetProperty(ref length, value); }
        public string UserContent { get => userContent; set => this.SetProperty(ref userContent, value); }
        public double Tolerance { get => tolerance; set => this.SetProperty(ref tolerance, value); }
        public int MaxIterations { get => maxIterations; set => this.SetProperty(ref maxIterations, value); }
        public string Status { get => status; set => this.SetProperty(ref status, value); }
        public PlotModel PlotModel { get; } = new PlotModel();
        private LineSeries lineSeriesUser = new LineSeries() { Title = "用户数据" };
        private LineSeries lineSeriesFit = new LineSeries() { Title = "拟合数据" };
        private StarCCMPorousParameter starCCMPorousParameter;
        private FluentPorousParameter fluentPorousParameter;
        private double p0 = 100;
        private double p1 = 100;
        private double denstity = 1.225;
        private double dynamicViscosity = 1e-5;
        private double length = 1;
        private string userContent = "1  ,116\r\n10 ,2870\r\n19 ,8702\r\n28 ,17612\r\n37 ,29600\r\n46 ,44666\r\n55 ,62810\r\n64 ,84032";
        private double tolerance = 1E-8;
        private int maxIterations = 1000;
        private string status = string.Empty;

        public CFDPorousViewModel()
        {
            PlotModel.Axes.Add(new LinearAxis() { Position = AxisPosition.Left });
            PlotModel.Axes.Add(new LinearAxis() { Position = AxisPosition.Bottom });
            PlotModel.Legends.Add(new Legend() { LegendPosition = LegendPosition.LeftTop });
            PlotModel.Series.Add(lineSeriesUser);
            PlotModel.Series.Add(lineSeriesFit);
        }

        [RelayCommand]
        private void Cal()
        {
            var ds = this.ConvertDPoints(this.UserContent);
            var r = ds.FitCurve(P0, P1, this.Tolerance, this.MaxIterations);
            this.P0 = r.p0;
            this.P1 = r.p1;
            StarCCMPorousParameter = PorousHandler.GetStarCCMPorousParameter(r.p0, r.p1, this.Length);
            FluentPorousParameter = PorousHandler.GetFluentResult(r.p0, r.p1, this.Denstity,this.DynamicViscosity,this.Length);
            lineSeriesUser.Points.Clear();
            lineSeriesFit.Points.Clear();
            lineSeriesUser.Points.AddRange(ds.Select(p => new DataPoint(p.X, p.Y)));
            lineSeriesFit.Points.AddRange(ds.Select(p => new DataPoint(p.X, p.X * p.X * this.P1 + p.X * this.P0)));
            this.Status = $"一次系数：{this.P0}\t二次系数：{this.P1}";
            PlotModel.InvalidatePlot(true);

        }

        /// <summary>
        /// 将只当字符串转为数据集合
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private IEnumerable<DPoint> ConvertDPoints(string content)
        {
            List<DPoint> points = new();
            var lines = content.Trim().Split("\r\n");
            foreach (var line in lines)
            {
                var ns = line.Split(",");
                if (ns.Length == 1) { ns = line.Split("，"); }
                if (ns.Length == 2)
                {
                    if (double.TryParse(ns[0].Trim(), out double x) && double.TryParse(ns[1].Trim(), out double y))
                    {
                        points.Add(new DPoint(x, y));
                    }
                }
            }
            return points;
        }

    }
}
