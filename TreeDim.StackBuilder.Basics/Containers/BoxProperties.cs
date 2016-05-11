﻿#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using Sharp3D.Math.Core;
#endregion

namespace treeDiM.StackBuilder.Basics
{
    /// <summary>
    /// Box properties (dimensions, colors, textures)
    /// </summary>
    public class BoxProperties : BProperties
    {
        #region Data members
        private double _height;
        private bool _hasInsideDimensions;
        private double _insideLength, _insideWidth, _insideHeight;
        private Color[] _colors = new Color[6];
        private List<Pair<HalfAxis.HAxis, Texture>> _textures = new List<Pair<HalfAxis.HAxis, Texture>>();
        // net weight
        private OptDouble _netWeight = OptDouble.Zero;
        // tape
        private bool _showTape;
        private double _tapeWidth;
        private Color _tapeColor;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor 1
        /// </summary>
        /// <param name="document">Parent document</param>
        public BoxProperties(Document document)
            : base(document)
        {
            _length = 0.0; _width = 0.0; _height = 0.0;
            _hasInsideDimensions = true;
        }
        /// <summary>
        /// Constructor 2
        /// </summary>
        /// <param name="document">Parent document</param>
        /// <param name="length">Outer length</param>
        /// <param name="width">Outer width</param>
        /// <param name="height">Outer height</param>
        public BoxProperties(Document document, double length, double width, double height)
            : base(document)
        {
            _length     = length;
            _width      = width;
            _height     = height;
            _hasInsideDimensions = false;
            _showTape = false;
        }
        /// <summary>
        /// Constructor 3
        /// </summary>
        /// <param name="document">Parent document</param>
        /// <param name="length">Outer length</param>
        /// <param name="width">Outer width</param>
        /// <param name="height">Outer height</param>
        /// <param name="insideLength">Inner length</param>
        /// <param name="insideWidth">Inner width</param>
        /// <param name="insideHeight">Inner height</param>
        public BoxProperties(Document document, double length, double width, double height, double insideLength, double insideWidth, double insideHeight)
            : base(document)
        {
            _length     = length;
            _width      = width;
            _height     = height;
            _insideLength = insideLength;
            _insideWidth = insideWidth;
            _insideHeight = insideHeight;
            _hasInsideDimensions = true;
        }
        #endregion

        #region Height
        public override double Height
        {
            get { return _height; }
            set { _height = value; Modify(); }
        }
        #endregion

        #region NetWeight
        public OptDouble NetWeight
        {
            get { return _netWeight; }
            set { _netWeight = value; Modify(); }
        }
        #endregion

        #region InsideDimensions
        public bool HasInsideDimensions
        {
            get { return _hasInsideDimensions; }
        }
        public double InsideLength
        {
            get { return _hasInsideDimensions ? _insideLength : _length; }
            set { _insideLength = value; Modify(); }
        }
        public double InsideWidth
        {
            get { return _hasInsideDimensions ? _insideWidth : _width; }
            set { _insideWidth = value; Modify(); }
        }
        public double InsideHeight
        {
            get { return _hasInsideDimensions ? _insideHeight : _height; }
            set { _insideHeight = value; Modify(); }
        }
        public double InsideVolume
        {
            get { return InsideLength * InsideWidth * InsideHeight; }
        }
        public override Vector3D OuterDimensions
        {   get { return new Vector3D(_length, _width, _height); } }
        public virtual Vector3D InsideDimensions
        {   get { return new Vector3D( InsideLength, InsideWidth, InsideHeight); } }
        public virtual double[] OuterDimensionsArray
        { get { return new double[] { _length, _width, _height }; } }
        public virtual double[] InsideDimensionsArray
        {   get { return new double[] { InsideLength, InsideWidth, InsideHeight }; } }

        public bool FitsIn(BoxProperties caseProperties)
        {
            double[] dimItem = OuterDimensionsArray;
            Array.Sort(dimItem);
            double[] dimCase = caseProperties.InsideDimensionsArray;
            Array.Sort(dimCase);
            return dimItem[0] <= dimCase[0]
                && dimItem[1] <= dimCase[1]
                && dimItem[2] <= dimCase[2];        
        }
        public bool FitsIn(BoxProperties caseProperties, bool allowVerticalX, bool allowVerticalY, bool allowVerticalZ)
        {
            double[] dimItem = OuterDimensionsArray;
            double[] dimCase = caseProperties.InsideDimensionsArray;
            if (allowVerticalX)
            {
                if (
                    (dimItem[0] <= dimCase[0]) && (
                        ( (dimItem[1] <= dimCase[1]) && (dimItem[2] <= dimCase[2]) )
                    || ( (dimItem[1] <= dimCase[2]) && (dimItem[2] <= dimCase[1]) ) )
                    )
                return true;
            }
            if (allowVerticalY)
            {
                if (
                    (dimItem[1] <= dimCase[1]) && (
                        ((dimItem[0] <= dimCase[0]) && (dimItem[2] <= dimCase[2]))
                    || ((dimItem[0] <= dimCase[2]) && (dimItem[2] <= dimCase[0])))
                    )
                    return true;
            }
            if (allowVerticalZ)
            {
                if (
                    (dimItem[2] <= dimCase[2]) && (
                        ((dimItem[0] <= dimCase[0]) && (dimItem[1] <= dimCase[1]))
                    || ((dimItem[0] <= dimCase[1]) && (dimItem[1] <= dimCase[0])))
                    )
                    return true;
            }
            return false;
        }
        #endregion

        #region Colors
        public override void SetColor(Color color)
        {
            for (int i = 0; i < 6; ++i)
                _colors[i] = color;
            Modify();
        }
        public override Color GetColor(HalfAxis.HAxis axis)
        {
            return _colors[(int)axis];
        }

        public override Color[] Colors
        {
            get { return _colors; }
        }
        public void SetColor(HalfAxis.HAxis axis, Color color)
        {
            _colors[(int)axis] = color;
            Modify();
        }
        public void SetAllColors(Color[] color)
        {
            for (int i = 0; i < 6; ++i)
                _colors[i] = color[i];
            Modify();
        }
        public bool UniqueColor
        {
            get
            {
                for (int i = 1; i < 6; ++i)
                    if (_colors[0] != _colors[i])
                        return false;
                return true;
            }
        }
        #endregion

        #region Texture pairs
        /// <summary>
        /// Texture pair
        /// </summary>
        /// <param name="axis">Face normal axis</param>
        /// <param name="position">Position</param>
        /// <param name="size">Size</param>
        /// <param name="angle">Angle</param>
        /// <param name="bmp">Image used as texture</param>
        public void AddTexture(HalfAxis.HAxis axis, Vector2D position, Vector2D size, double angle, Bitmap bmp)
        {
            _textures.Add(new Pair<HalfAxis.HAxis, Texture>(axis, new Texture(bmp, position, size, angle)));
            Modify();
        }
        /// <summary>
        /// Get / set face/texture pairs
        /// </summary>
        public List<Pair<HalfAxis.HAxis, Texture>> TextureList
        {
            get
            {
                return _textures;
            }
            set
            {
                _textures.Clear();
                if (null != value)
                    _textures.AddRange(value);
            }
        }
        public List<Pair<HalfAxis.HAxis, Texture>> TextureListCopy
        {
            get
            {
                List<Pair<HalfAxis.HAxis, Texture>> list = new List<Pair<HalfAxis.HAxis, Texture>>();
                foreach (Pair<HalfAxis.HAxis, Texture> tex in _textures)
                    list.Add(new Pair< HalfAxis.HAxis, Texture >(tex.first, tex.second.Clone()));
                return list;
            }
        }
        #endregion

        #region Tape properties
        public bool ShowTape
        {
            get { return _showTape;     }
            set { _showTape = value;    }
        }
        public double TapeWidth
        {
            get { return _tapeWidth;    }
            set { _tapeWidth = value;   }
        }
        public Color TapeColor
        {
            get { return _tapeColor;    }
            set { _tapeColor = value;   }
        }
        #endregion

        #region IsBundle
        public override bool IsBundle { get { return false; } }
        #endregion
    }
}
