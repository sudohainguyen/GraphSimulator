using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphSimulator.Helpers
{
    public enum RunningMode
    {
        ShowTheResult,
        StepByStep,
        Visualization
    }

    public enum Direction
    {
        None = 0,
        OneWay,
        OneWayReserved,
        TwoWay
    }

    public enum Operation
    {
        ADD,
        DELETE
    }
}
