#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sharp3D.Math.Core;

using treeDiM.StackBuilder.Basics;
#endregion

namespace treeDiM.StackBuilder.Engine
{

    public class LayerSolver : ILayerSolver
    {
        #region Static data members
        #endregion

        #region Public methods
        public List<Layer2D> BuildLayers(Vector3D dimBox, Vector2D dimContainer, ConstraintSetAbstract constraintSet, bool keepOnlyBest)
        {
            // instantiate list of layers
            List<Layer2D> listLayers0 = new List<Layer2D>();

            // loop through all patterns
            foreach (LayerPattern pattern in LayerPattern.All)
            {
                // loop through all orientation
                foreach (HalfAxis.HAxis axisOrtho in HalfAxis.All)
                {
                    // is orientation allowed
                    if ( !constraintSet.AllowOrientation(axisOrtho) )
                        continue;
                    // not swapped vs swapped pattern
                    for (int iSwapped = 0; iSwapped < 1; ++iSwapped)
                    {
                        // does swapping makes sense for this layer pattern ?
                        if (!pattern.CanBeSwapped && (iSwapped == 1))
                            continue;
                        // instantiate layer
                        Layer2D layer = new Layer2D(dimBox, dimContainer, axisOrtho, iSwapped == 1);
                        layer.PatternName = pattern.Name;
                        double actualLength = 0.0, actualWidth = 0.0;
                        if (!pattern.GetLayerDimensionsChecked(layer, out actualLength, out actualWidth))
                            continue;
                        pattern.GenerateLayer(layer, actualLength, actualWidth);
                        if (0 == layer.Count)
                            continue;
                        listLayers0.Add(layer);
                    }
                }
            }

            // keep only best layers
            if (keepOnlyBest)
            {
                // 1. get best count
                int bestCount = 0;
                foreach (Layer2D layer in listLayers0)
                    bestCount = Math.Max(layer.CountInHeight(constraintSet.OptMaxHeight.Value), bestCount);

                // 2. remove any layer that does not match the best count given its orientation
                List<Layer2D> listLayers1 = new List<Layer2D>();
                foreach (Layer2D layer in listLayers0)
                {
                    if (layer.CountInHeight(constraintSet.OptMaxHeight.Value) >= bestCount)
                        listLayers1.Add(layer);
                }
                // 3. copy back in original list
                listLayers0.Clear();
                listLayers0.AddRange(listLayers1);
            }
            if (constraintSet.OptMaxHeight.Activated)
                listLayers0.Sort(new LayerComparerCount(constraintSet.OptMaxHeight.Value));

            return listLayers0;
        }

        public Layer2D BuildLayer(Vector3D dimBox, Vector2D dimContainer, LayerDesc layerDesc)
        {
            // instantiate layer
            Layer2D layer = new Layer2D(dimBox, dimContainer, layerDesc.AxisOrtho, false);
            // get layer pattern
            LayerPattern pattern = LayerPattern.GetByName(layerDesc.PatternName);
            // dimensions
            double actualLength = 0.0, actualWidth = 0.0;
            if (!pattern.GetLayerDimensionsChecked(layer, out actualLength, out actualWidth))
                return null;
            pattern.GenerateLayer(layer, actualLength, actualWidth);
            return layer;
        }

        public Layer2D BuildLayer(Vector3D dimBox, Vector2D dimContainer, LayerDesc layerDesc, Vector2D actualDimensions)
        { 
            // instantiate layer
            Layer2D layer = new Layer2D(dimBox, dimContainer, layerDesc.AxisOrtho, false);
            // get layer pattern
            LayerPattern pattern = LayerPattern.GetByName(layerDesc.PatternName);
            // build layer
            pattern.GenerateLayer(layer, actualDimensions.X, actualDimensions.Y);
            return layer;
        }

        public bool GetDimensions(List<LayerDesc> layers, Vector3D dimBox, Vector2D dimContainer, out Vector2D actualDimensions)
        {
            actualDimensions = new Vector2D();
            foreach (LayerDesc layerDesc in layers)
            { 
                // instantiate layer
                Layer2D layer = new Layer2D(dimBox, dimContainer, layerDesc.AxisOrtho, false);
                // get layer pattern
                LayerPattern pattern = LayerPattern.GetByName(layerDesc.PatternName);
                // dimensions
                double actualLength = 0.0, actualWidth = 0.0;
                if (!pattern.GetLayerDimensionsChecked(layer, out actualLength, out actualWidth))
                    return false;

                actualDimensions.X = Math.Max(actualDimensions.X, actualLength);
                actualDimensions.Y = Math.Max(actualDimensions.Y, actualWidth);
            }
            return true;
        }
        #endregion
    }
}
