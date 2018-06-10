using GraphSimulator.User_Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphSimulator.Helpers
{
    public class RouteEngine
    {
        private static RouteEngine _instance;
        private static readonly object Padlock = new object();

        public static RouteEngine Instance
        {
            get
            {
                lock (Padlock)
                {
                    if (_instance is null)
                        _instance = new RouteEngine();
                }
                return _instance;
            }
        }

        public List<Connection> Connections { get; set; } = new List<Connection>();
        public Dictionary<char, Node> Nodes { get; set; } = new Dictionary<char, Node>();
        public List<Action> Actions { get; set; }
    }
}
