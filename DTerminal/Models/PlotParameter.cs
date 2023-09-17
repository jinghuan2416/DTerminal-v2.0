using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DTerminal.Models
{
    internal class PlotParameter
    {
        public Curve[] Curves { get; set; } = Array.Empty<Curve>();
        public double FontSize { get; set; } = 12;
        public bool IsYLogarithmic { get; set; } = false;
    }

    internal class Curve
    {
        public Curve(string title, Point[] points)
        {
            Title = title;
            Points = points;
        }

        public string Title { get; set; }
        public Point[] Points { get; set; }
    }
}
