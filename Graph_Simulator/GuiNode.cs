using RouteEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graph_Simulator
{
    public class GuiNode : Node
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool Selected { get; set; }
        public int Diameter => 30;
    }
}
