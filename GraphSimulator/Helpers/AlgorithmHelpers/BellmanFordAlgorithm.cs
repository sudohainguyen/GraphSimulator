using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphSimulator.User_Controls;

namespace GraphSimulator.Helpers.AlgorithmHelpers
{
    public class BellmanFordAlgorithm : IAlgorithm
    {
        public List<string> Pseudocode =>
            new List<string>
            {
                "BEGIN",
                "  d(v[1]) ← 0",
                "  FOR j = 2,..,n DO",
                "    d(v[j]) ← ∞",
                "  FOR i = 1,..,(|V|-1) DO",
                "    FOR ALL (u,v) in E DO",
                "      d(v) ← min(d(v), d(u)+l(u,v))",
                "  FOR ALL (u,v) in E DO",
                "    IF d(v) > d(u) + l(u,v) DO",
                "      Message: 'Negative Circle'",
                "END"
            };

        public bool CanRunWithGraph(Graph g, out string message)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Route> ExtractStepsWithResult(Node startNode)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Route> ShowResult(Node startNode)
        {
            throw new NotImplementedException();
        }
    }
}
