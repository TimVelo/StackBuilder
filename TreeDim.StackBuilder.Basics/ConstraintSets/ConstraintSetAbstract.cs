#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sharp3D.Math.Core;
#endregion

namespace treeDiM.StackBuilder.Basics
{
    public abstract class ConstraintSetAbstract
    {
        public abstract bool AllowOrientation(HalfAxis.HAxis axisOrtho);
        public abstract OptDouble OptMaxHeight { get; set; }
        public abstract OptDouble OptMaxWeight { get; set; }
        public abstract OptInt OptMaxNumber { get; set; }
        public abstract bool Valid { get; }
    }
}
