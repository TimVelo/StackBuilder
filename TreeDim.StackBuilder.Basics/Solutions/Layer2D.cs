#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Sharp3D.Math.Core;

using log4net;
#endregion

namespace treeDiM.StackBuilder.Basics
{

    public class LayerDesc
    {
        #region Data members
        string _patternName;
        HalfAxis.HAxis _axis;
        #endregion

        #region Constructor
        public LayerDesc(string patternName, HalfAxis.HAxis axis)
        {
            _patternName = patternName; _axis = axis;
        }
        #endregion

        #region Public properties
        public HalfAxis.HAxis AxisOrtho
        { get { return _axis; } }
        public string PatternName
        {   get { return _patternName; } }
        #endregion

        #region Object override
        public override string ToString()
        {
            return string.Format("{0} | {1}", _patternName, _axis);
        }
        public static LayerDesc Parse(string value)
        {
            Regex r = new Regex(@"(?<name>|(?<axis>))", RegexOptions.Singleline);
            Match m = r.Match(value);
            if (m.Success)
            {
                string patternName = m.Result("${name}");
                HalfAxis.HAxis axis = HalfAxis.Parse( m.Result("${axis}"));
                return new LayerDesc(patternName, axis);
            }
            else
                throw new Exception("Failed to parse LayerDesc");
        }
        #endregion
    }

    public class Layer2D : List<LayerPosition>
    {
        #region Data members
        private string _patternName = string.Empty;
        private HalfAxis.HAxis _axisOrtho = HalfAxis.HAxis.AXIS_Z_P;
        private bool _swapped = false;
        private bool _inversed = false;

        private double _forcedSpace = 0.0;
        private double _maximumSpace = 0.0;

        private Vector2D _dimContainer;
        private Vector3D _dimBox;

        private static readonly double _epsilon = 1.0e-03;

        protected static ILog _log = LogManager.GetLogger(typeof(Layer2D));
        #endregion

        #region Constructor
        public Layer2D(Vector3D dimBox, Vector2D dimContainer, HalfAxis.HAxis axisOrtho, bool swapped)
        {
            _axisOrtho = axisOrtho;
            _dimBox = dimBox;
            _dimContainer = dimContainer;
            _swapped = swapped;
        }
        #endregion

        #region Public methods
        public void AddPosition(Vector2D vPosition, HalfAxis.HAxis lengthAxis, HalfAxis.HAxis widthAxis)
        {
            // build 4D matrix
            Vector3D vAxisLength = HalfAxis.ToVector3D(lengthAxis);
            Vector3D vAxisWidth = HalfAxis.ToVector3D(widthAxis);
            Vector3D vAxisHeight = Vector3D.CrossProduct(vAxisLength, vAxisWidth);
            Matrix4D mat = Matrix4D.Identity;
            mat.M11 = vAxisLength.X;
            mat.M12 = vAxisLength.Y;
            mat.M13 = vAxisLength.Z;
            mat.M21 = vAxisWidth.X;
            mat.M22 = vAxisWidth.Y;
            mat.M23 = vAxisWidth.Z;
            mat.M31 = vAxisHeight.X;
            mat.M32 = vAxisHeight.Y;
            mat.M33 = vAxisHeight.Z;
            mat.M41 = 0.0;
            mat.M42 = 0.0;
            mat.M43 = 0.0;
            mat.M44 = 1.0;
            Transform3D localTransf = new Transform3D(mat);
            Transform3D localTransfInv = localTransf.Inverse();
            Transform3D originTranslation = Transform3D.Translation(localTransfInv.transform(VecTransf) - new Vector3D(0.5 * _forcedSpace, 0.5 * _forcedSpace, 0.0));

            Vector3D vPos = originTranslation.transform(new Vector3D(vPosition.X, vPosition.Y, 0.0) + 0.5 * _forcedSpace * vAxisLength + 0.5 * _forcedSpace * vAxisWidth);
            LayerPosition layerPos = new LayerPosition(
                originTranslation.transform(new Vector3D(vPosition.X, vPosition.Y, 0.0) + 0.5 * _forcedSpace * vAxisLength + 0.5 * _forcedSpace * vAxisWidth)
                , HalfAxis.ToHalfAxis(localTransfInv.transform(HalfAxis.ToVector3D(LengthAxis)))
                , HalfAxis.ToHalfAxis(localTransfInv.transform(HalfAxis.ToVector3D(WidthAxis)))
                );
            // add position
            Add(layerPos.Adjusted(_dimBox));
        }
        public bool IsValidPosition(Vector2D vPosition, HalfAxis.HAxis lengthAxis, HalfAxis.HAxis widthAxis)
        {
            // check layerPos
            // get 4 points
            Vector3D[] pts = new Vector3D[4];
            pts[0] = new Vector3D(vPosition.X, vPosition.Y, 0.0);
            pts[1] = new Vector3D(vPosition.X, vPosition.Y, 0.0) + HalfAxis.ToVector3D(lengthAxis) * BoxLength;
            pts[2] = new Vector3D(vPosition.X, vPosition.Y, 0.0) + HalfAxis.ToVector3D(widthAxis) * BoxWidth;
            pts[3] = new Vector3D(vPosition.X, vPosition.Y, 0.0) + HalfAxis.ToVector3D(lengthAxis) * BoxLength + HalfAxis.ToVector3D(widthAxis) * BoxWidth;

            foreach (Vector3D pt in pts)
            {
                if (pt.X < (0.0 - _epsilon) || pt.X > (_dimContainer.X + _epsilon) || pt.Y < (0.0 - _epsilon) || pt.Y > (_dimContainer.Y + _epsilon))
                    return false;
            }
            return true;
        }
        public int PerPalletCount(double zHeight)
        {
            int noLayers = (int)(zHeight / LayerHeight);
            return noLayers * Count;
        }
        public double LayerHeight
        {
            get
            {
                switch (_axisOrtho)
                {
                    case HalfAxis.HAxis.AXIS_X_N: return _dimBox.X;
                    case HalfAxis.HAxis.AXIS_X_P: return _dimBox.Y;
                    case HalfAxis.HAxis.AXIS_Y_N: return _dimBox.Y;
                    case HalfAxis.HAxis.AXIS_Y_P: return _dimBox.X;
                    case HalfAxis.HAxis.AXIS_Z_N: return _dimBox.Z;
                    case HalfAxis.HAxis.AXIS_Z_P: return _dimBox.Z;
                    default:
                        throw new Exception();
                }
            }
        }
        #endregion

        #region Maximum space
        public double MaximumSpace
        {
            get
            {
                System.Diagnostics.Debug.Assert(!double.IsNaN(_maximumSpace) && !double.IsInfinity(_maximumSpace));
                return _maximumSpace; 
            }
            set { _maximumSpace = value; }
        }
        public void UpdateMaxSpace(double space)
        {
            if (space < 0.0 - _epsilon)
            {
                _log.Error("Negative space value?");
                return;
            }
            if (double.IsInfinity(space) || double.IsNaN(space))
                return;
            _maximumSpace = Math.Max(space, _maximumSpace);
        }
        #endregion

        #region Public properties
        public string PatternName
        {
            get { return _patternName; }
            set { _patternName = value; }
        }
        public string Name
        {
            get { return string.Format("{0}_{1}_{2}", PatternName, HalfAxis.ToString(_axisOrtho), _swapped ? "t" : "f"); }
        }
        public HalfAxis.HAxis AxisOrtho
        {
            get { return _axisOrtho; }
        }
        public HalfAxis.HAxis LengthAxis
        {
            get
            {
                switch (_axisOrtho)
                {
                    case HalfAxis.HAxis.AXIS_X_N: return HalfAxis.HAxis.AXIS_Z_P;
                    case HalfAxis.HAxis.AXIS_X_P: return HalfAxis.HAxis.AXIS_Y_P;
                    case HalfAxis.HAxis.AXIS_Y_N: return HalfAxis.HAxis.AXIS_X_P;
                    case HalfAxis.HAxis.AXIS_Y_P: return HalfAxis.HAxis.AXIS_Z_P;
                    case HalfAxis.HAxis.AXIS_Z_N: return HalfAxis.HAxis.AXIS_Y_P;
                    case HalfAxis.HAxis.AXIS_Z_P: return HalfAxis.HAxis.AXIS_X_P;
                    default: throw new Exception("Invalid ortho axis");
                }
            }    
        }
        public HalfAxis.HAxis WidthAxis
        {
            get
            {
                switch (_axisOrtho)
                {
                    case HalfAxis.HAxis.AXIS_X_N: return HalfAxis.HAxis.AXIS_Y_P;
                    case HalfAxis.HAxis.AXIS_X_P: return HalfAxis.HAxis.AXIS_Z_P;
                    case HalfAxis.HAxis.AXIS_Y_N: return HalfAxis.HAxis.AXIS_Z_P;
                    case HalfAxis.HAxis.AXIS_Y_P: return HalfAxis.HAxis.AXIS_X_P;
                    case HalfAxis.HAxis.AXIS_Z_N: return HalfAxis.HAxis.AXIS_X_P;
                    case HalfAxis.HAxis.AXIS_Z_P: return HalfAxis.HAxis.AXIS_Y_P;
                    default: throw new Exception("Invalid ortho axis");
                }
            }
        }
        public Vector3D VecTransf
        {
            get
            {
                switch (_axisOrtho)
                {
                    case HalfAxis.HAxis.AXIS_X_N: return new Vector3D(_dimBox.Z, 0.0, 0.0);
                    case HalfAxis.HAxis.AXIS_X_P: return new Vector3D(0.0, 0.0, 0.0); ;
                    case HalfAxis.HAxis.AXIS_Y_N: return new Vector3D(0.0, _dimBox.Z, 0.0);
                    case HalfAxis.HAxis.AXIS_Y_P: return Vector3D.Zero;
                    case HalfAxis.HAxis.AXIS_Z_N: return new Vector3D(0.0, 0.0, _dimBox.Z);
                    case HalfAxis.HAxis.AXIS_Z_P: return Vector3D.Zero;
                    default: throw new Exception("Invalid ortho axis");
                }
            }
        }
        public double BoxLength
        {
            get
            {
                switch (_axisOrtho)
                {
                    case HalfAxis.HAxis.AXIS_X_N: return _dimBox.Z + _forcedSpace;
                    case HalfAxis.HAxis.AXIS_X_P: return _dimBox.Z + _forcedSpace;
                    case HalfAxis.HAxis.AXIS_Y_N: return _dimBox.X + _forcedSpace;
                    case HalfAxis.HAxis.AXIS_Y_P: return _dimBox.Y + _forcedSpace;
                    case HalfAxis.HAxis.AXIS_Z_N: return _dimBox.Y + _forcedSpace;
                    case HalfAxis.HAxis.AXIS_Z_P: return _dimBox.X + _forcedSpace;
                    default: throw new Exception("Invalid ortho axis");
                }
            }
        }
        public double BoxWidth
        {
            get
            {
                switch (_axisOrtho)
                { 
                    case HalfAxis.HAxis.AXIS_X_N: return _dimBox.Y + _forcedSpace;
                    case HalfAxis.HAxis.AXIS_X_P: return _dimBox.X + _forcedSpace;
                    case HalfAxis.HAxis.AXIS_Y_N: return _dimBox.Z + _forcedSpace;
                    case HalfAxis.HAxis.AXIS_Y_P: return _dimBox.Z + _forcedSpace;
                    case HalfAxis.HAxis.AXIS_Z_N: return _dimBox.X + _forcedSpace;
                    case HalfAxis.HAxis.AXIS_Z_P: return _dimBox.Y + _forcedSpace;
                    default: throw new Exception("Invalid ortho axis");
                }
            }
        }
        public double BoxHeight
        {
            get
            {
                switch (_axisOrtho)
                {
                    case HalfAxis.HAxis.AXIS_X_N: return _dimBox.X;
                    case HalfAxis.HAxis.AXIS_X_P: return _dimBox.Y;
                    case HalfAxis.HAxis.AXIS_Y_N: return _dimBox.Y;
                    case HalfAxis.HAxis.AXIS_Y_P: return _dimBox.X;
                    case HalfAxis.HAxis.AXIS_Z_N: return _dimBox.Z;
                    case HalfAxis.HAxis.AXIS_Z_P: return _dimBox.Z;
                    default: throw new Exception("Invalid ortho axis");
                }
            }
        }
        public double PalletLength
        {
            get { return _dimContainer.X; }
        }
        public double PalletWidth
        {
            get { return _dimContainer.Y; }
        }
        public bool Swapped
        {
            get { return _swapped; }
        }
        public bool Inversed
        {
            get { return _inversed; }
        }
        public int BoxCount
        {
            get { return Count; }
        }
        public int NoLayers(double height)
        {
            return (int)Math.Floor(height / BoxHeight); 
        }
        public int CountInHeight(double height)
        {
            return NoLayers(height) * Count;
        }
        public LayerDesc LayerDescriptor
        {
            get { return new LayerDesc(_patternName, _axisOrtho); }
        }
        #endregion
    }

    #region Comparers
    public class LayerComparerCount : IComparer<Layer2D>
    {
        #region Data members
        private double _height = 0; 
        #endregion

        #region Constructor
        public LayerComparerCount(double height)
        {
            _height = height;
        }
        #endregion

        #region Implement IComparer
        public int Compare(Layer2D layer0, Layer2D layer1)
        {
            int layer0Count = layer0.CountInHeight(_height);
            int layer1Count = layer1.CountInHeight(_height);

            if (layer0Count < layer1Count) return 1;
            else if (layer0Count == layer1Count)
            {
                if (layer0.AxisOrtho < layer1.AxisOrtho)
                    return 1;
                else if (layer0.AxisOrtho == layer1.AxisOrtho)
                {
                    return 0;
                }
                else return -1;
            }
            else return -1;
        }
        #endregion
    }
    #endregion
}
