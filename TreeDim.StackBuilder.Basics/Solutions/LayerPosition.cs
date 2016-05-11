#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sharp3D.Math.Core;
#endregion

namespace treeDiM.StackBuilder.Basics
{
    public class LayerPosition
    {
        #region Constructor
        public LayerPosition(Vector3D position, HalfAxis.HAxis lengthAxis, HalfAxis.HAxis widthAxis)
        {
            _position = position;
            _lengthAxis = lengthAxis;
            _widthAxis = widthAxis;
        }
        public LayerPosition(LayerPosition pos)
        {
            _position = pos._position;
            _lengthAxis = pos._lengthAxis;
            _widthAxis = pos._widthAxis;
        }
        #endregion

        #region Static methods
        public static bool Intersect(LayerPosition p1, LayerPosition p2, double boxLength, double boxWidth)
        {
            Vector2D v1Min, v1Max, v2Min, v2Max;
            p1.MinMax(boxLength, boxWidth, out v1Min, out v1Max);
            p2.MinMax(boxLength, boxWidth, out v2Min, out v2Max);

            if (v1Max.X <= v2Min.X || v2Max.X <= v1Min.X || v1Max.Y <= v2Min.Y || v2Max.Y <= v1Min.Y)
                return false;
            return true;
        }
        public void MinMax(double boxLength, double boxWidth, out Vector2D vMin, out Vector2D vMax)
        {
            Vector3D[] pts = new Vector3D[4];
            pts[0] = new Vector3D(_position.X, _position.Y, 0.0);
            pts[1] = new Vector3D(_position.X, _position.Y, 0.0) + HalfAxis.ToVector3D(_lengthAxis) * boxLength;
            pts[2] = new Vector3D(_position.X, _position.Y, 0.0) + HalfAxis.ToVector3D(_widthAxis) * boxWidth;
            pts[3] = new Vector3D(_position.X, _position.Y, 0.0) + HalfAxis.ToVector3D(_lengthAxis) * boxLength + HalfAxis.ToVector3D(_widthAxis) * boxWidth;

            vMin = new Vector2D(double.MaxValue, double.MaxValue);
            vMax = new Vector2D(double.MinValue, double.MinValue);
            foreach (Vector3D v in pts)
            {
                vMin.X = Math.Min(v.X, vMin.X);
                vMin.Y = Math.Min(v.Y, vMin.Y);
                vMax.X = Math.Max(v.X, vMax.X);
                vMax.Y = Math.Max(v.Y, vMax.Y);
            }
        }
        #endregion

        #region Public properties
        public Vector3D Position
        {
            get { return _position; }
            set { _position = value; }
        }
        public HalfAxis.HAxis LengthAxis
        {
            get { return _lengthAxis; }
            set { _lengthAxis = value; }
        }
        public HalfAxis.HAxis WidthAxis
        {
            get { return _widthAxis; }
            set { _widthAxis = value; }
        }
        public HalfAxis.HAxis HeightAxis
        {
            get
            {
                if (!IsValid)
                    throw new Exception("Invalid position -> Can not compute DirectionHeight.");
                return HalfAxis.ToHalfAxis(
                    Vector3D.CrossProduct(HalfAxis.ToVector3D(_lengthAxis), HalfAxis.ToVector3D(_widthAxis))
                    );
            }
        }
        public bool IsValid
        {
            get { return _lengthAxis != _widthAxis; }
        }

        public LayerPosition Adjusted(Vector3D dimensions)
        {
            LayerPosition layerPosTemp = new LayerPosition(this);

            // reverse if oriented to Z- (AXIS_Z_N)
            if (HeightAxis == HalfAxis.HAxis.AXIS_Z_N)
            {
                if (LengthAxis == HalfAxis.HAxis.AXIS_X_P)
                {
                    layerPosTemp.WidthAxis = HalfAxis.HAxis.AXIS_Y_P;
                    layerPosTemp.Position += new Vector3D(0.0, -dimensions.Y, -dimensions.Z);
                }
                else if (LengthAxis == HalfAxis.HAxis.AXIS_Y_P)
                {
                    layerPosTemp.WidthAxis = HalfAxis.HAxis.AXIS_X_N;
                    layerPosTemp.Position += new Vector3D(dimensions.Y, 0.0, -dimensions.Z);
                }
                else if (LengthAxis == HalfAxis.HAxis.AXIS_X_N)
                {
                    layerPosTemp.LengthAxis = HalfAxis.HAxis.AXIS_X_P;
                    layerPosTemp.Position += new Vector3D(-dimensions.X, 0.0, -dimensions.Z);
                }
                else if (LengthAxis == HalfAxis.HAxis.AXIS_Y_N)
                {
                    layerPosTemp.WidthAxis = HalfAxis.HAxis.AXIS_X_P;
                    layerPosTemp.Position += new Vector3D(-dimensions.Y, 0.0, -dimensions.Z);
                }
            }

            return layerPosTemp;
        }
        #endregion

        #region Data members
        private Vector3D _position;
        private HalfAxis.HAxis _lengthAxis, _widthAxis;
        #endregion
    }
}
