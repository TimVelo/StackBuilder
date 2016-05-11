#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

using Sharp3D.Math.Core;
using treeDiM.StackBuilder.Basics;
using treeDiM.StackBuilder.Engine;
using treeDiM.StackBuilder.Graphics;
#endregion

namespace treeDiM.StackBuilder.Engine.TestLayerBuilder
{
    class Program
    {
        static void Main(string[] args)
        {

            bool bestLayersOnly = true;
            Vector3D dimBox = new Vector3D(400.0, 300.0, 150.0);
            Vector2D dimContainer = new Vector2D(1200.0, 1000.0);
            ConstraintSetCasePallet constraintSet = new ConstraintSetCasePallet();
            constraintSet.SetMaxHeight(1200.0); 

            try
            {
                LayerSolver solver = new LayerSolver();
                List<Layer2D> layers = solver.BuildLayers(dimBox, dimContainer, constraintSet, bestLayersOnly);

                int solIndex = 0;
                foreach (Layer2D layer in layers)
                {
                    string fileName = string.Format("{0}_{1}.bmp", layer.Name, solIndex++);
                    string filePath = Path.Combine(Path.GetTempPath(), fileName);
                    Console.WriteLine(string.Format("Generating {0}...", filePath));

                    Graphics2DImage graphics = new Graphics2DImage( new Size(150, 150) );
                    SolutionViewerLayer solViewer = new SolutionViewerLayer(layer);
                    BoxProperties bProperties = new BoxProperties(null, 400.0, 300.0, 150.0);
                    bProperties.SetColor(Color.Brown);
                    solViewer.Draw(graphics, bProperties, 1500.0);
                    graphics.SaveAs(filePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
