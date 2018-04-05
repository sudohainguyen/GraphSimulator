﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouteEngine
{
    public class Connection
    {
        public Node A { get; set; }
        public Node B { get; set; }
        public int Cost { get; set; }
        public bool Selected { get; set; }
        public bool IsTwoWay { get; set; } = false;
    }
}
