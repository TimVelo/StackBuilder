#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using treeDiM.StackBuilder.Basics;
#endregion

namespace treeDiM.StackBuilder.Engine
{
    public abstract class Solver
    {
        public Solver() { }
        public abstract Solution BuildSolution();
    }
}
