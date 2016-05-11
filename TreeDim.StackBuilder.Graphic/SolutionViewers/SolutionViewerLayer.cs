#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using Sharp3D.Math.Core;

using treeDiM.StackBuilder.Basics;
#endregion

namespace treeDiM.StackBuilder.Graphics
{
    public class SolutionViewerLayer : IDisposable
    {
        #region Data members
        private Layer2D _layer;
        private static int _fontSize = 9;
        #endregion

        #region Constructor
        public SolutionViewerLayer(Layer2D layer)
        {
            _layer = layer;
        }
        #endregion

        #region Implementation IDispose
        public void Dispose()
        { 
        }
        #endregion

        #region Public methods
        public void Draw(Graphics2D graphics, BProperties bProperties, double height, bool selected)
        {
            graphics.NumberOfViews = 1;
            graphics.Graphics.Clear(selected ? Color.LightBlue : Color.White);
            graphics.SetViewport(0.0f, 0.0f, (float)_layer.PalletLength, (float)_layer.PalletWidth);

            if (_layer != null)
            {
                graphics.SetCurrentView(0);
                graphics.DrawRectangle(Vector2D.Zero, new Vector2D(_layer.PalletLength, _layer.PalletWidth), Color.Black);
                uint pickId = 0;
                foreach (LayerPosition bPosition in _layer)
                    graphics.DrawBox(new Box(pickId++, bProperties, bPosition));

                // draw axes
                bool showAxis = false;
                if (showAxis)
                {
                    // draw axis X
                    graphics.DrawLine(Vector2D.Zero, new Vector2D(_layer.PalletLength, 0.0), Color.Red);
                    // draw axis Y
                    graphics.DrawLine(Vector2D.Zero, new Vector2D(0.0, _layer.PalletWidth), Color.Green);
                }
            }

            Size s = graphics.Size;

            // *** Annotate : begin ***
            string annotation = string.Format(
                "{0}*{1}={2}"
                , _layer.Count
                , _layer.NoLayers(height)
                , _layer.CountInHeight(height) );
            Font tfont = new Font("Arial", _fontSize);
            Color brushColor = Color.White;
            Color backgroundColor = Color.Black;
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Far;
            sf.LineAlignment = StringAlignment.Far;
            System.Drawing.Graphics g = graphics.Graphics; 
            Size txtSize = g.MeasureString(annotation, tfont).ToSize();
            g.FillRectangle(new SolidBrush(backgroundColor), new Rectangle(s.Width - txtSize.Width - 2, s.Height - txtSize.Height - 2, txtSize.Width + 2, txtSize.Height + 2));
            g.DrawString(annotation, tfont, new SolidBrush(brushColor), new Point(s.Width - 3, s.Height - 3), sf);
            // *** Annotate : end ***
        }
        public void Draw(Graphics3D graphics, BProperties bProperties, double height, bool selected)
        { 
        }
        #endregion
    }
}
