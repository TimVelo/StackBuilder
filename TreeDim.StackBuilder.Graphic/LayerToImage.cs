#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using treeDiM.StackBuilder.Basics;
#endregion

namespace treeDiM.StackBuilder.Graphics
{
    public class LayerToImage
    {
        public static Bitmap Draw(Layer2D layer, BProperties bProperties, double height, Size size, bool selected)
        {
            Graphics2DImage graphics = new Graphics2DImage(size);
            using (SolutionViewerLayer solViewer = new SolutionViewerLayer(layer))
            {   solViewer.Draw(graphics, bProperties, height, selected); }
            return graphics.Bitmap;
        }
    }
}
