using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTerminal.Views
{
    internal class ViewAttribute : System.Attribute
    {
        public string Title { get;  }
        public int Order { get; set; } = 1;
        public ViewAttribute(string title)
        {
            Title = title;
        }

        public ViewAttribute(string title, int order) : this(title)
        {
            Order = order;
        }
    }
}
