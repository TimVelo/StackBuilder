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
    class LayerPatternInterlockedSymetric2
        : LayerPatternInterlockedSymetric
    {
        #region Override LayerPattern
        public override string Name
        {
            get { return "Symetric Interlocked 2"; }
        }
        public override void GenerateLayer(Layer2D layer, double actualLength, double actualWidth)
        {
            layer.Clear();

            double boxLength = layer.BoxLength;
            double boxWidth = layer.BoxWidth;
            double palletLength = GetPalletLength(layer);
            double palletWidth = GetPalletWidth(layer);

            int maxSizeXLength = 0, maxSizeXWidth = 0, maxSizeYLength = 0, maxSizeYWidth = 0;
            GetSizeXY(boxLength, boxWidth, palletLength, palletWidth
                , out maxSizeXLength, out maxSizeXWidth, out maxSizeYLength, out maxSizeYWidth);

            double offsetX = 0.5 * (palletLength - actualLength);
            double offsetY = 0.5 * (palletWidth - actualWidth);

            double spaceX = maxSizeXLength + maxSizeXWidth > 1 ? (actualLength - (maxSizeXLength * boxLength + maxSizeXWidth * boxWidth)) / (maxSizeXLength + maxSizeXWidth - 1) : 0.0;
            double spaceYLength = maxSizeYLength > 1 ? (actualWidth - maxSizeYLength * boxWidth) / (maxSizeYLength - 1) : 0.0;
            double spaceYWidth = maxSizeYWidth > 1 ? actualWidth - maxSizeYWidth * boxLength : 0.0;

            for (int i = 0; i < maxSizeXLength / 2; ++i)
                for (int j = 0; j < maxSizeYLength; ++j)
                {
                    AddPosition(
                        layer
                        , new Vector2D(
                            offsetX + i * (boxLength + spaceX)
                            , offsetY + j * (boxWidth + spaceYLength))
                        , HalfAxis.HAxis.AXIS_X_P, HalfAxis.HAxis.AXIS_Y_P);

                    AddPosition(
                        layer
                        , new Vector2D(
                            palletLength - offsetX - i * (boxLength + spaceX)
                            , palletWidth - offsetY - j * (boxWidth + spaceYLength))
                        , HalfAxis.HAxis.AXIS_X_N, HalfAxis.HAxis.AXIS_Y_N);
                }
            for (int i = 0; i < maxSizeXWidth; ++i)
                for (int j = 0; j < maxSizeYWidth; ++j)
                    AddPosition(
                        layer
                        , new Vector2D(
                            offsetX + maxSizeXLength / 2 * (boxLength + spaceX) + i * (boxWidth + spaceX) + boxWidth
                            , offsetY + j * (boxLength + spaceYWidth))
                        , HalfAxis.HAxis.AXIS_Y_P, HalfAxis.HAxis.AXIS_X_N);

            layer.UpdateMaxSpace(spaceX);
            layer.UpdateMaxSpace(spaceYLength);
            layer.UpdateMaxSpace(spaceYWidth);
        }
        #endregion
    }
}
