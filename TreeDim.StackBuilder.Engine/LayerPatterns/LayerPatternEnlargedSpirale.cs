﻿#region Using directives
using System;
using System.Collections.Generic;
using System.Text;

using Sharp3D.Math.Core;
using treeDiM.StackBuilder.Basics;
#endregion

namespace treeDiM.StackBuilder.Engine
{
    class LayerPatternEnlargedSpirale : LayerPattern
    {
        #region Implementation of LayerPattern abstract properties and methods
        public override string Name
        {
            get { return "Enlarged spiral"; }
        }

        public override bool GetLayerDimensions(Layer2D layer, out double actualLength, out double actualWidth)
        {
            double boxLength = layer.BoxLength;
            double boxWidth = layer.BoxWidth;
            double palletLength = GetPalletLength(layer);
            double palletWidth = GetPalletWidth(layer);

            // compute optimal layout
            int sizeX_area1 = 0, sizeY_area1 = 0
                , sizeX_area2 = 0, sizeY_area2 = 0
                , sizeX_area3 = 0, sizeY_area3 = 0
                , dir_area3 = 0;

            GetOptimalSizesXY(
            boxLength, boxWidth, palletLength, palletWidth
            , out sizeX_area1, out sizeY_area1
            , out sizeX_area2, out sizeY_area2
            , out sizeX_area3, out sizeY_area3
            , out dir_area3);

            actualLength = sizeX_area1 * boxLength + sizeX_area2 * boxWidth;
            actualWidth = sizeY_area1 * boxWidth + sizeY_area2 * boxLength;

            if (2.0 * sizeX_area2 * boxWidth + sizeX_area3 * ((dir_area3 == 0) ? boxLength : boxWidth) > actualLength)
                actualLength = 2.0 * sizeX_area2 * boxWidth + sizeX_area3 * ((dir_area3 == 0) ? boxLength : boxWidth);
            if (2.0 * sizeY_area1 * boxWidth + sizeY_area3 * ((dir_area3 == 0) ? boxWidth : boxLength) > actualWidth)
                actualWidth = 2.0 * sizeY_area1 * boxWidth + sizeY_area3 * ((dir_area3 == 0) ? boxWidth : boxLength);

            return sizeX_area1 > 0 && sizeY_area1 > 0
                && sizeX_area2 > 0 && sizeY_area2 > 0;
        }

        public override void GenerateLayer(Layer2D layer, double actualLength, double actualWidth)
        {
            // initialization
            layer.Clear();

            double boxLength = layer.BoxLength;
            double boxWidth = layer.BoxWidth;
            double palletLength = GetPalletLength(layer);
            double palletWidth = GetPalletWidth(layer);

            // compute optimal layout
            int sizeX_area1 = 0, sizeY_area1 = 0
                , sizeX_area2 = 0, sizeY_area2 = 0
                , sizeX_area3 = 0, sizeY_area3 = 0
                , dir_area3 = 0;

            GetOptimalSizesXY(
            boxLength, boxWidth, palletLength, palletWidth
            , out sizeX_area1, out sizeY_area1
            , out sizeX_area2, out sizeY_area2
            , out sizeX_area3, out sizeY_area3
            , out dir_area3);

            // compute offsets
            double offsetX = 0.5 * (palletLength - actualLength);
            double offsetY = 0.5 * (palletWidth - actualWidth);

            // area1
            for (int i = 0; i < sizeX_area1; ++i)
                for (int j = 0; j < sizeY_area1; ++j)
                {
                    AddPosition(layer
                        , new Vector2D(offsetX + i * boxLength, offsetY + j * boxWidth)
                        , HalfAxis.HAxis.AXIS_X_P, HalfAxis.HAxis.AXIS_Y_P);
                    AddPosition(layer
                        , new Vector2D(palletLength - offsetX - i * boxLength, palletWidth - offsetY - j * boxWidth)
                        , HalfAxis.HAxis.AXIS_X_N, HalfAxis.HAxis.AXIS_Y_N);
                }
            double spaceX_area1 = actualLength - 2 * sizeX_area1 * boxLength;
            double spaceY_area1 = actualWidth - 2 * sizeY_area1 * boxWidth;
            // area2
            for (int i = 0; i < sizeX_area2; ++i)
                for (int j = 0; j < sizeY_area2; ++j)
                {
                    AddPosition(layer
                        , new Vector2D(palletLength - offsetX - i * boxWidth, offsetY + j * boxLength)
                        , HalfAxis.HAxis.AXIS_Y_P, HalfAxis.HAxis.AXIS_X_N);
                    AddPosition(layer
                        , new Vector2D(offsetX + i * boxWidth, palletWidth - offsetY - j * boxLength)
                        , HalfAxis.HAxis.AXIS_Y_N, HalfAxis.HAxis.AXIS_X_P);
                }
            double spaceX_area2 = actualLength - 2 * sizeX_area2 * boxWidth;
            double spaceY_area2 = actualWidth - 2 * sizeY_area2 * boxLength;
            // area3
            for (int i = 0; i < sizeX_area3; ++i)
                for (int j = 0; j < sizeY_area3; ++j)
                {
                    if (dir_area3 == 0)
                        AddPosition(layer
                        , new Vector2D(
                            offsetX + 0.5 * (actualLength  - sizeX_area3 * boxLength) + i * boxLength
                            , offsetY + 0.5 * (actualWidth - sizeY_area3 * boxWidth) + j * boxWidth
                            )
                        , HalfAxis.HAxis.AXIS_X_P, HalfAxis.HAxis.AXIS_Y_P);
                    else
                        AddPosition(layer
                            , new Vector2D(
                            offsetX + 0.5 * (actualLength - sizeX_area3 * boxWidth) + (i+1) * boxWidth
                            , offsetY + 0.5 * (actualWidth - sizeY_area3 * boxLength) + j * boxLength
                            )
                            , HalfAxis.HAxis.AXIS_Y_P, HalfAxis.HAxis.AXIS_X_N);
                }

            double spaceX_area3 = 0.0, spaceY_area3 = 0.0;
            if (dir_area3 == 0)
            {
                spaceX_area3 = 0.5 * (actualLength - sizeX_area3 * boxLength - 2.0 * (spaceX_area1 > 0 ? sizeX_area1 * boxLength : sizeX_area2 * boxWidth));
                spaceY_area3 = 0.5 * (actualWidth - sizeY_area3 * boxWidth - 2.0 * (spaceY_area1 > 0 ? sizeY_area1 * boxWidth : sizeY_area2 * boxLength));
            }
            else
            {
                spaceX_area3 = 0.5 * (actualLength - sizeX_area3 * boxWidth - 2.0 * (spaceX_area1 > 0 ? sizeX_area1 * boxLength : sizeX_area2 * boxWidth));
                spaceY_area3 = 0.5 * (actualWidth - sizeY_area3 * boxLength - 2.0 * (spaceY_area1 > 0 ? sizeY_area1 * boxWidth : sizeY_area2 * boxLength));
            }
            // set spacing
            layer.UpdateMaxSpace(spaceX_area3);
            layer.UpdateMaxSpace(spaceY_area3);
        }
        public override int GetNumberOfVariants(Layer2D layer) { return 1; }
        public override bool CanBeSwapped { get { return true; } }
        public override bool CanBeInverted { get { return true; } }
        #endregion

        #region Helpers
        private void GetOptimalSizesXY(
            double boxLength, double boxWidth
            , double palletLength, double palletWidth
            , out int sizeX_area1, out int sizeY_area1
            , out int sizeX_area2, out int sizeY_area2
            , out int sizeX_area3, out int sizeY_area3
            , out int dir_area3)
        {
            // get optimum combination of sizeXLength, sizeYLength
            int sizeXLengthMax = (int)Math.Floor((palletLength - boxWidth) / boxLength);
            int sizeYLengthMax = (int)Math.Floor((palletWidth - boxLength) / boxWidth);

            // initialization
            int iBoxNumberMax = 0;
            sizeX_area1 = 0;
            sizeY_area1 = 0;
            sizeX_area2 = 0;
            sizeY_area2 = 0;
            sizeX_area3 = 0;
            sizeY_area3 = 0;
            dir_area3 = 0;

            for (int i1 = 1; i1 <= sizeXLengthMax; ++i1)
                for (int j1 = 1; j1 <= sizeYLengthMax; ++j1)
                {
                    double L1 = i1 * boxLength;
                    double H1 = j1 * boxWidth;

                    if ( (L1 > 0.5 * palletLength && H1 > 0.5 * palletWidth)
                        || (L1 < 0.5 * palletLength && H1 < 0.5 * palletWidth) )
                        continue;

                    int i2 = (int)Math.Floor((palletLength - L1) / boxWidth);
                    int j2 = (int)Math.Floor((palletWidth - H1) / boxLength);

                    for (int iDir = 0; iDir < 2; ++iDir)
                    {
                        int i3 = (int)Math.Floor((palletLength - 2 * i2 * boxWidth) / (iDir == 0 ? boxLength : boxWidth));
                        int j3 = (int)Math.Floor((palletWidth - 2 * j1 * boxWidth) / (iDir == 0 ? boxWidth : boxLength));

                        if ((iBoxNumberMax < 2 * (i1 * j1 + i2 * j2) + i3 * j3) && (i3 * j3 > 0))
                        {
                            sizeX_area1 = i1;
                            sizeY_area1 = j1;
                            sizeX_area2 = i2;
                            sizeY_area2 = j2;
                            sizeX_area3 = i3;
                            sizeY_area3 = j3;
                            dir_area3 = iDir;
                        }
                    }
                }
        }
        #endregion
    }
}
