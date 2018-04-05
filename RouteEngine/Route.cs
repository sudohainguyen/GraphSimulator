using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouteEngine
{
    public class Route
    {
        private char _identifier;

        public int TotalCost { get; set; } = int.MaxValue;
        public List<Connection> Connections { get; set; } = new List<Connection>();

        public Route(char id)
        {
            _identifier = id;
        }

        public override string ToString()
        {
            return $"Id: {_identifier} - Total cost: {TotalCost}";
        }
    }
}
