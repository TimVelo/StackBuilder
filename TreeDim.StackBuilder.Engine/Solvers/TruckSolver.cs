#region Using directives
using System;
using System.Collections.Generic;
using System.Text;

using treeDiM.StackBuilder.Basics;
using Sharp3D.Math.Core;

using log4net;
#endregion

namespace treeDiM.StackBuilder.Engine
{
    public class TruckSolver : ITruckSolver
    {
        #region Data members
        static readonly ILog _log = LogManager.GetLogger(typeof(TruckSolver));
        #endregion

        #region TruckSolver
        public TruckSolver()
        {
        }
        #endregion

        #region Public methods
        public void ProcessAnalysis(TruckAnalysis truckAnalysis)
        {
            truckAnalysis.Solutions = GenerateSolutions(truckAnalysis);
        }
        #endregion

        #region Private methods
        private List<TruckSolution> GenerateSolutions(TruckAnalysis truckAnalysis)
        {
            List<TruckSolution> solutions = new List<TruckSolution>();

            HalfAxis.HAxis[] axis = { HalfAxis.HAxis.AXIS_Z_N, HalfAxis.HAxis.AXIS_Z_P};

            // build layer using truck length / width
            foreach (LayerPattern pattern in LayerPattern.All)
            {
                for (int swapPos = 0; swapPos < (pattern.CanBeSwapped ? 2 : 1); ++swapPos)
                {
                    for (int orientation = 0; orientation < 2; ++orientation)
                    {
                        try
                        {
                            Layer2D layer = BuildLayer(truckAnalysis.ParentSolution, truckAnalysis.TruckProperties, truckAnalysis.ConstraintSet
                                , axis[orientation], swapPos == 1);
                            double actualLength = 0.0, actualWidth = 0.0;
                            if (!pattern.GetLayerDimensionsChecked(layer, out actualLength, out actualWidth))
                                continue;
                            pattern.GenerateLayer(layer, actualLength, actualWidth);

                            TruckSolution sol = new TruckSolution("sol", truckAnalysis);

                            BoxLayer boxLayer = new BoxLayer(0.0, 0);
                            foreach (LayerPosition layerPos in layer)
                                boxLayer.AddPosition(layerPos.Position, layerPos.LengthAxis, layerPos.WidthAxis);

                            sol.Layer = boxLayer;

                            // insert solution
                            if (sol.PalletCount > 0)
                                solutions.Add(sol);
                        }
                        catch (Exception ex)
                        {
                            _log.Error(string.Format("Exception caught: {0}", ex.ToString()));
                        }
                    }
                }
            }

            // sort solutions
            solutions.Sort();

            return solutions;
        }
        #endregion

        #region Build layer
        Layer2D BuildLayer(CasePalletSolution palletSolution, TruckProperties truckProperties, TruckConstraintSet constraintSet
            ,HalfAxis.HAxis axisOrtho, bool swapped)
        { 
            return new Layer2D(new Vector3D(palletSolution.PalletLength, palletSolution.PalletWidth, palletSolution.PalletHeight)
                                , new Vector2D(truckProperties.Length, truckProperties.Width)
                                , axisOrtho, swapped);
        }
        #endregion
    }
}
