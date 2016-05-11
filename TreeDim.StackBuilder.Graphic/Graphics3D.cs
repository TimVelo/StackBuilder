﻿#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

using Sharp3D.Math.Core;
using treeDiM.StackBuilder.Basics;
#endregion

namespace treeDiM.StackBuilder.Graphics
{
    public abstract class Graphics3D
    {
        #region Enums
        enum PaintingAlgorithm
        {
            ALGO_PAINTER,
            ALGO_BSPTREE
        }
        public enum FaceDir
        {
            FRONT
            , BACK
        }
        #endregion

        #region Data members
        /// <summary>
        /// Eye position
        /// </summary>
        private Vector3D _vCameraPos = new Vector3D(0.0, 0.0, -1000.0);
        /// <summary>
        /// Target position
        /// </summary>
        private Vector3D _vTarget = Vector3D.Zero;
        /// <summary>
        /// Viewport
        /// </summary>
        private float[] _viewport = new float[4];
        /// <summary>
        /// Compute viewport automatically if enabled
        /// </summary>
        private bool _autoViewport = true;
        /// <summary>
        /// Margin coefficient
        /// </summary>
        private double _margin = 0.01;
        /// <summary>
        /// Background color
        /// </summary>
        private Color _backgroundColor = Color.White;
        /// <summary>
        /// face in the background
        /// </summary>
        private List<Face> _facesBackground = new List<Face>();
        /// <summary>
        /// face buffer used for drawing
        /// </summary>
        private List<Face> _faces = new List<Face>();
        /// <summay>
        /// box buffer used for drawing
        /// </summay>
        private List<Box> _boxes = new List<Box>();
        /// <summary>
        /// cylinder buffer used for drawing
        /// </summary>
        private List<Cylinder> _cylinders = new List<Cylinder>();
        /// <summary>
        /// segments
        /// </summary>
        private List<Segment> _segments = new List<Segment>();
        private List<Segment> _segmentsBackground = new List<Segment>();
        /// <summary>
        /// dimensions cube
        /// </summary>
        private List<DimensionCube> _dimensions = new List<DimensionCube>();
        /// <summary>
        /// Current transformation
        /// </summary>
        private Transform3D _currentTransf;
        /// <summary>
        /// Show box Ids (used of debugging purposes)
        /// </summary>
        private bool _showBoxIds = false;
        private bool _showTextures = true;
        private bool _useBoxelOrderer = true;
        private uint _boxDrawingCounter = 0;
        private bool _enableFaceSorting = true;

        public static readonly Vector3D Front = new Vector3D(10000.0, 0.0, 0.0);
        public static readonly Vector3D Back = new Vector3D(-10000.0, 0.0, 0.0);
        public static readonly Vector3D Left = new Vector3D(0.0, -10000.0, 0.0);
        public static readonly Vector3D Right = new Vector3D(0.0, 10000.0, 0.0);
        public static readonly Vector3D Top = new Vector3D(0.0, 0.0, 10000);
        public static readonly Vector3D Corner_0 = new Vector3D(
                Math.Cos(45.0 * Math.PI / 180.0) * Math.Sqrt(2.0) * 10000.0
                , Math.Sin(45.0 * Math.PI / 180.0) * Math.Sqrt(2.0) * 10000.0
                , 10000.0);
        public static readonly Vector3D Corner_90 = new Vector3D(
             Math.Cos(135.0 * Math.PI / 180.0) * Math.Sqrt(2.0) * 10000.0
             , Math.Sin(135.0 * Math.PI / 180.0) * Math.Sqrt(2.0) * 10000.0
             , 10000.0);
        public static readonly Vector3D Corner_180 = new Vector3D(
             Math.Cos(225.0 * Math.PI / 180.0) * Math.Sqrt(2.0) * 10000.0
             , Math.Sin(225.0 * Math.PI / 180.0) * Math.Sqrt(2.0) * 10000.0
             , 10000.0);
        public static readonly Vector3D Corner_270 = new Vector3D(
             Math.Cos(315.0 * Math.PI / 180.0) * Math.Sqrt(2.0) * 10000.0
             , Math.Sin(315.0 * Math.PI / 180.0) * Math.Sqrt(2.0) * 10000.0
             , 10000.0);
        #endregion

        #region Constructors
        public Graphics3D()
        {
            _viewport[0] = -500.0f;
            _viewport[1] = -500.0f;
            _viewport[2] = 500.0f;
            _viewport[3] = 500.0f;
        }
        #endregion

        #region Public properties
        /// <summary>
        /// Background color
        /// </summary>
        public Color BackgroundColor
        {
            get { return _backgroundColor; }
            set { _backgroundColor = value; }
        }
        public bool EnableFaceSorting
        {
            get { return _enableFaceSorting; }
            set { _enableFaceSorting = value; }
        }
        /// <summary>
        /// View point (position of the observer's eye)
        /// </summary>
        public Vector3D CameraPosition
        {
            get { return _vCameraPos; }
            set { _vCameraPos = value; }
        }

        public Vector3D Target
        {
            get { return _vTarget; }
            set { _vTarget = value; }
        }

        public Vector3D VLight
        {
            get
            {
                Vector3D vLight = _vCameraPos - _vTarget;
                vLight.Normalize();
                return vLight;
            }
        }

        public Vector3D ViewDirection
        {
            get
            {
                Vector3D viewDir = _vTarget - _vCameraPos;
                viewDir.Normalize();
                return viewDir;
            }
        }

        /// <summary>
        /// gets or sets boolean to show box ids
        /// To be used when debugging
        /// </summary>
        public bool ShowBoxIds
        {
            get { return _showBoxIds; }
            set { _showBoxIds = value; }
        }
        /// <summary>
        /// gets or sets boxel order status
        /// If set to true the boxel order will be used when drawing box layers
        /// </summary>
        public bool UseBoxelOrderer
        {
            get { return _useBoxelOrderer; }
            set { _useBoxelOrderer = value; }
        }

        public bool ShowTextures
        {
            get { return _showTextures; }
            set { _showTextures = value; }
        }
        #endregion

        #region Helpers


        private Point[] TransformPoint(Transform3D transform, Vector3D[] points3d)
        {
            Point[] points = new Point[points3d.Length];
            int i = 0;
            foreach (Vector3D v in points3d)
            {
                Vector3D vt = transform.transform(v);
                points[i] = new Point((int)vt.X, (int)vt.Y);
                ++i;
            }
            return points;
        }

        private Point TransformPoint(Transform3D transform, Vector3D point3d)
        {
            Vector3D vt = transform.transform(point3d);
            return new Point((int)vt.X, (int)vt.Y);
        }

        public Transform3D GetWorldToEyeTransformation()
        {
            /*
            Orthographic transformation chain
            • Start with coordinates in object’s local coordinates
            • Transform into world coords (modeling transform, Mm)
            • Transform into eye coords (camera xf., Mcam = Fc–1)
            • Orthographic projection, Morth
            • Viewport transform, Mvp

            ps = Mvp*Morth*Mcam*Mm*po
            */
            Vector3D zaxis = _vCameraPos - _vTarget;
            zaxis.Normalize();
            Vector3D up = Vector3D.ZAxis;
            Vector3D xaxis = Vector3D.CrossProduct(up, zaxis);
            if (Vector3D.CrossProduct(up, zaxis).GetLengthSquared() < 0.0001)
            {
                up = Vector3D.ZAxis;
                xaxis = Vector3D.XAxis;
            }
            xaxis.Normalize();
            Vector3D yaxis = Vector3D.CrossProduct(zaxis, xaxis);
            Matrix4D Mcam = new Matrix4D(
                    xaxis.X, xaxis.Y, xaxis.Z, -Vector3D.DotProduct(_vCameraPos - _vTarget, xaxis),
                    yaxis.X, yaxis.Y, yaxis.Z, -Vector3D.DotProduct(_vCameraPos - _vTarget, yaxis),
                    -zaxis.X, -zaxis.Y, -zaxis.Z, -Vector3D.DotProduct(_vCameraPos - _vTarget, -zaxis),
                    0.0, 0.0, 0.0, 1.0
                );
            return new Transform3D(Mcam);
        }

        private Transform3D GetOrthographicProjection(Vector3D vecMin, Vector3D vecMax)
        {
            double[] sizeMin = new double[3];
            sizeMin[0] = 1.0;
            sizeMin[1] = Size.Height;
            sizeMin[2] = 0.0;

            double[] sizeMax = new double[3];
            sizeMax[0] = Size.Width;
            sizeMax[1] = 1.0;
            sizeMax[2] = 1.0;

            return Transform3D.OrthographicProjection(vecMin, vecMax, sizeMin, sizeMax);
        }
        /// <summary>
        /// Background faces
        /// </summary>
        /// <param name="face">Face to be drawn before other faces</param>
        public void AddFaceBackground(Face face)
        {
            _facesBackground.Add(face);
        }
        /// <summary>
        /// add face
        /// </summary>
        /// <param name="face">Face item</param>
        public void AddFace(Face face)
        {
            _faces.Add(face);
        }

        public void AddBox(Box box)
        {
            if (!box.IsValid)
                throw new GraphicsException("Box is invalid and cannot be drawn!");
            _boxes.Add(box);
        }

        public void AddCylinder(Cylinder cylinder)
        {
            _cylinders.Add(cylinder);
        }

        public void AddDimensions(DimensionCube dimensionCube)
        {
            _dimensions.Add(dimensionCube);
        }
        #endregion

        #region Abstract methods and properties
        abstract public Size Size { get; }
        abstract public System.Drawing.Graphics Graphics { get; }
        #endregion

        #region Public methods
        /// <summary>
        /// SetViewport
        /// </summary>
        /// <param name="xmin">xmin -> bottom</param>
        /// <param name="ymin">ymin -> left</param>
        /// <param name="xmax">xmax -> right</param>
        /// <param name="ymax">ymax -> top</param>
        public void SetViewport(float xmin, float ymin, float xmax, float ymax)
        {
            _viewport[0] = xmin;
            _viewport[1] = ymin;
            _viewport[2] = xmax;
            _viewport[3] = ymax;
        }

        /// <summary>
        /// Draw all entities stored in buffer
        /// </summary>
        public void Flush()
        {
            // initialize
            Vector3D vLight = _vCameraPos - _vTarget; vLight.Normalize();
            _boxDrawingCounter = 0;
            _currentTransf = null;
            System.Drawing.Graphics g = Graphics;
            g.Clear(_backgroundColor);

            if (EnableFaceSorting)
            {
                // sort face list
                FaceComparison faceComparer = new FaceComparison(GetWorldToEyeTransformation());
                _faces.Sort(faceComparer);
            }
            // draw background segments
            foreach (Segment s in _segmentsBackground)
                Draw(s);
            // draw background faces
            foreach (Face face in _facesBackground)
                Draw(face, FaceDir.FRONT);
            // draw all faces using solid / transparency depending on 
            foreach (Face face in _faces)
                Draw(face, FaceDir.BACK);

            // sort box list
            if (_useBoxelOrderer)
            {
                BoxelOrderer boxelOrderer = new BoxelOrderer(_boxes);
                boxelOrderer.Direction = _vTarget - _vCameraPos;
                _boxes = boxelOrderer.GetSortedList();
            }
            else
                _boxes.Sort(new BoxComparerSimplifiedPainterAlgo(GetWorldToEyeTransformation()));

            // sort cylinder list
            _cylinders.Sort(new CylinderComparerSimplifiedPainterAlgo(GetWorldToEyeTransformation()));

            if (_cylinders.Count > 0)
            {
                // sort by Z
                List<Drawable> drawableList = new List<Drawable>();
                drawableList.AddRange(_boxes);
                drawableList.AddRange(_cylinders);
                drawableList.Sort(new DrawableComparerSimplifiedPainterAlgo());

                List<Box> boxes = new List<Box>();
                List<Cylinder> cylinders = new List<Cylinder>();
                bool processingBox = drawableList[0] is Box;
                foreach (Drawable drawable in drawableList)
                {
                    Box b = drawable as Box;
                    Cylinder c = drawable as Cylinder;

                    if ((null != b) && processingBox)
                        boxes.Add(b);
                    else if ((null == b) && !processingBox)
                        cylinders.Add(c);
                    else
                    {
                        if (boxes.Count > 0)
                        {
                            BoxelOrderer boxelOrderer = new BoxelOrderer(boxes);
                            boxelOrderer.Direction = _vTarget - _vCameraPos;
                            boxes = boxelOrderer.GetSortedList();
                            // draw boxes
                            foreach (Box bb in boxes)
                                Draw(bb);
                            // clear
                            boxes.Clear();
                        }
                        if (cylinders.Count > 0)
                        {
                            cylinders.Sort(new CylinderComparerSimplifiedPainterAlgo(GetWorldToEyeTransformation()));
                            // draw cylinders
                            foreach (Cylinder cc in cylinders)
                                Draw(cc);
                            // clear
                            cylinders.Clear();
                        }
                        if (null != b)
                        {
                            boxes.Add(b);
                            processingBox = true;
                        }
                        else
                        {
                            cylinders.Add(c);
                            processingBox = false;
                        }
                    }
                }

                // remaining boxes
                BoxelOrderer boxelOrdererRem = new BoxelOrderer(boxes);
                boxelOrdererRem.Direction = _vTarget - _vCameraPos;
                boxes = boxelOrdererRem.GetSortedList();
                // draw boxes
                foreach (Box bb in boxes)
                    Draw(bb);

                // remaining cylinders
                cylinders.Sort(new CylinderComparerSimplifiedPainterAlgo(GetWorldToEyeTransformation()));
                // draw cylinders
                foreach (Cylinder cc in cylinders)
                    Draw(cc);
                // clear
                boxes.Clear();
            }
            else
            {
                // draw all boxes
                foreach (Box box in _boxes)
                    Draw(box);
            }

            // draw faces : end
            foreach (Face face in _faces)
                Draw(face, FaceDir.FRONT);

            // draw segment list (e.g. hatching)
            foreach (Segment seg in _segments)
                Draw(seg);

            // draw cotation cubes
            foreach (DimensionCube qc in _dimensions)
                qc.Draw(this);
        }

        public Transform3D GetCurrentTransformation()
        {
            if (null == _currentTransf)
            {
                // get transformations
                Transform3D world2eye = GetWorldToEyeTransformation();
                Transform3D orthographicProj = GetOrthographicProjection(
                    new Vector3D(_viewport[0], _viewport[1], -10000)
                    , new Vector3D(_viewport[2], _viewport[3], 10000));

                // build automatic viewport
                if (_autoViewport)
                {
                    Vector3D vecMin = new Vector3D(double.MaxValue, double.MaxValue, double.MaxValue);
                    Vector3D vecMax = new Vector3D(double.MinValue, double.MinValue, double.MinValue);

                    // boxes
                    foreach (Box box in _boxes)
                        foreach (Vector3D pt in box.Points)
                        {
                            Vector3D ptT = world2eye.transform(pt);
                            vecMin.X = Math.Min(vecMin.X, ptT.X);
                            vecMin.Y = Math.Min(vecMin.Y, ptT.Y);
                            vecMin.Z = Math.Min(vecMin.Z, ptT.Z);
                            vecMax.X = Math.Max(vecMax.X, ptT.X);
                            vecMax.Y = Math.Max(vecMax.Y, ptT.Y);
                            vecMax.Z = Math.Max(vecMax.Z, ptT.Z);
                        }
                    // cylinders
                    foreach (Cylinder cyl in _cylinders)
                    {
                        foreach (Vector3D pt in cyl.BottomPoints)
                        {
                            Vector3D ptT = world2eye.transform(pt);
                            vecMin.X = Math.Min(vecMin.X, ptT.X);
                            vecMin.Y = Math.Min(vecMin.Y, ptT.Y);
                            vecMin.Z = Math.Min(vecMin.Z, ptT.Z);
                            vecMax.X = Math.Max(vecMax.X, ptT.X);
                            vecMax.Y = Math.Max(vecMax.Y, ptT.Y);
                            vecMax.Z = Math.Max(vecMax.Z, ptT.Z);
                        }
                        foreach (Vector3D pt in cyl.TopPoints)
                        {
                            Vector3D ptT = world2eye.transform(pt);
                            vecMin.X = Math.Min(vecMin.X, ptT.X);
                            vecMin.Y = Math.Min(vecMin.Y, ptT.Y);
                            vecMin.Z = Math.Min(vecMin.Z, ptT.Z);
                            vecMax.X = Math.Max(vecMax.X, ptT.X);
                            vecMax.Y = Math.Max(vecMax.Y, ptT.Y);
                            vecMax.Z = Math.Max(vecMax.Z, ptT.Z);
                        }
                    }
                    // faces
                    foreach (Face face in _faces)
                        foreach (Vector3D pt in face.Points)
                        {
                            Vector3D ptT = world2eye.transform(pt);
                            vecMin.X = Math.Min(vecMin.X, ptT.X);
                            vecMin.Y = Math.Min(vecMin.Y, ptT.Y);
                            vecMin.Z = Math.Min(vecMin.Z, ptT.Z);
                            vecMax.X = Math.Max(vecMax.X, ptT.X);
                            vecMax.Y = Math.Max(vecMax.Y, ptT.Y);
                            vecMax.Z = Math.Max(vecMax.Z, ptT.Z);
                        }

                    // segments
                    foreach (Segment seg in _segments)
                        foreach (Vector3D pt in seg.Points)
                        {
                            Vector3D ptT = world2eye.transform(pt);
                            vecMin.X = Math.Min(vecMin.X, ptT.X);
                            vecMin.Y = Math.Min(vecMin.Y, ptT.Y);
                            vecMin.Z = Math.Min(vecMin.Z, ptT.Z);
                            vecMax.X = Math.Max(vecMax.X, ptT.X);
                            vecMax.Y = Math.Max(vecMax.Y, ptT.Y);
                            vecMax.Z = Math.Max(vecMax.Z, ptT.Z);
                        }

                    // cube dimensions
                    foreach (DimensionCube dimCube in _dimensions)
                        foreach (Vector3D pt in dimCube.DrawingPoints(this))
                        {
                            Vector3D ptT = world2eye.transform(pt);
                            vecMin.X = Math.Min(vecMin.X, ptT.X);
                            vecMin.Y = Math.Min(vecMin.Y, ptT.Y);
                            vecMin.Z = Math.Min(vecMin.Z, ptT.Z);
                            vecMax.X = Math.Max(vecMax.X, ptT.X);
                            vecMax.Y = Math.Max(vecMax.Y, ptT.Y);
                            vecMax.Z = Math.Max(vecMax.Z, ptT.Z);
                        }

                    Vector3D vecMin1 = vecMin, vecMax1 = vecMax;
                    // adjust width/height
                    if ((vecMax.Y - vecMin.Y) / Size.Height > (vecMax.X - vecMin.X) / Size.Width)
                    {
                        double actualWidth = (vecMax.Y - vecMin.Y) * Size.Width / Size.Height;
                        vecMin1.X = 0.5 * (vecMin.X + vecMax.X) - 0.5 * actualWidth;
                        vecMax1.X = 0.5 * (vecMin.X + vecMax.X) + 0.5 * actualWidth;
                    }
                    else
                    {
                        double actualHeight = (vecMax.X - vecMin.X) * Size.Height / Size.Width;
                        vecMin1.Y = 0.5 * (vecMin.Y + vecMax.Y) - 0.5 * actualHeight;
                        vecMax1.Y = 0.5 * (vecMin.Y + vecMax.Y) + 0.5 * actualHeight;
                    }
                    // set margins
                    double width = vecMax1.X - vecMin1.X;
                    vecMin1.X -= _margin * width;
                    vecMax1.X += _margin * width;
                    double height = vecMax1.Y - vecMin1.Y;
                    vecMin1.Y -= _margin * height;
                    vecMax1.Y += _margin * height;

                    orthographicProj = GetOrthographicProjection(vecMin1, vecMax1);
                }
                _currentTransf = orthographicProj * world2eye;
            }
            return _currentTransf;
        }
        public void AddSegmentBackgound(Segment seg)
        { 
            _segmentsBackground.Add(seg);
        }
        public void AddSegment(Segment seg)
        {
            _segments.Add(seg);
        }
        #endregion

        #region Draw box
        /// <summary>
        /// Draw a line segment
        /// </summary>
        /// <param name="seg">Segment object to be drawn</param>
        internal void Draw(Segment seg)
        {
            System.Drawing.Graphics g = Graphics;
            Brush brush = new SolidBrush(seg.Color);
            Pen pen = new Pen(brush);
            Point[] pt = TransformPoint(GetCurrentTransformation(), seg.Points);
            g.DrawLine(pen, pt[0], pt[1]);
        }
        /// <summary>
        /// Draw a text string at a 3D location
        /// </summary>
        /// <param name="text">Text to draw</param>
        /// <param name="position">3D point on which to center the text string</param>
        /// <param name="color">Color of solid brush used to draw text</param>
        /// <param name="fontSize">Size of font used to draw text</param>
        internal void Draw(string text, Vector3D position, Color color, float fontSize)
        {
            System.Drawing.Graphics g = Graphics;
            Point pt = TransformPoint(GetCurrentTransformation(), position);
            Font font = new Font("Arial", fontSize);
            SizeF sizeF = g.MeasureString(text, font);
            g.DrawString(text
                , font
                , new SolidBrush(color)
                , new Point(pt.X - (int)(0.5f * sizeF.Width), pt.Y - (int)(0.5f * sizeF.Height))
                , StringFormat.GenericDefault);
        }

        /// <summary>
        /// Draw a face
        /// </summary>
        /// <param name="face">Face object to be drawn</param>
        internal void Draw(Face face, FaceDir dir)
        {
            System.Drawing.Graphics g = Graphics;

            // test if face can actuallt be seen
            if ((Vector3D.DotProduct(face.Normal, _vCameraPos - _vTarget) > 0.0 && dir == FaceDir.BACK)
                || (Vector3D.DotProduct(face.Normal, _vCameraPos - _vTarget) < 0.0 && dir == FaceDir.FRONT))
                return;

            // compute face color
            double cosA = System.Math.Abs(Vector3D.DotProduct(face.Normal, VLight));
            Color color = Color.FromArgb(
                face.IsSolid ? 255 : (dir == FaceDir.FRONT ? 64 : 255)
                , (int)(face.ColorFill.R * cosA)
                , (int)(face.ColorFill.G * cosA)
                , (int)(face.ColorFill.B * cosA));
            Point[] pt = TransformPoint(GetCurrentTransformation(), face.Points);

            Brush brush = new SolidBrush(color);
            g.FillPolygon(brush, pt);
            // draw path
            Brush brush0 = new SolidBrush(face.ColorPath);
            int ptCount = pt.Length;
            for (int i = 1; i < ptCount; ++i)
            {
                // there is a bug here!
                // -> a polygon that result from first split will lose all edges
                // when split a second time
                g.DrawLine(new Pen(brush0, 1.5f), pt[i - 1], pt[i]);
            }
            g.DrawLine(new Pen(brush0, 1.5f), pt[ptCount - 1], pt[0]);
        }

        internal void Draw(Face face, FaceDir dir, Color colorApply, bool transparent)
        {
            System.Drawing.Graphics g = Graphics;

            // test if face can actuallt be seen
            if ((Vector3D.DotProduct(face.Normal, _vCameraPos - _vTarget) > 0.0 && dir == FaceDir.BACK)
                || (Vector3D.DotProduct(face.Normal, _vCameraPos - _vTarget) < 0.0 && dir == FaceDir.FRONT))
                return;

            // compute face color
            double cosA = System.Math.Abs(Vector3D.DotProduct(face.Normal, VLight));
            Color color = Color.FromArgb(
                transparent ? 64 : 255
                , (int)(colorApply.R * cosA)
                , (int)(colorApply.G * cosA)
                , (int)(colorApply.B * cosA));
            Point[] pt = TransformPoint(GetCurrentTransformation(), face.Points);

            Brush brush = new SolidBrush(color);
            g.FillPolygon(brush, pt);
            // draw path
            float fThickness = transparent ? 2.0f : 1.0f;
            Brush brush0 = new SolidBrush(face.ColorPath);
            int ptCount = pt.Length;
            for (int i = 1; i < ptCount; ++i)
            {
                // there is a bug here!
                // -> a polygon that result from first split will lose all edges
                // when split a second time
                g.DrawLine(new Pen(brush0, fThickness), pt[i - 1], pt[i]);
            }
            g.DrawLine(new Pen(brush0, fThickness), pt[ptCount - 1], pt[0]);
        }

        internal void Draw(Box box)
        {
            System.Drawing.Graphics g = Graphics;

            if (box is Pack)
            {
                Pack pack = box as Pack;
                pack.Draw(this);
            }
            else
            {

                Vector3D[] points = box.Points;

                Face[] faces = box.Faces;
                for (int i = 0; i < 6; ++i)
                {
                    // Face
                    Face face = faces[i];
                    // face normal
                    Vector3D normal = face.Normal;
                    // visible ?
                    if (!faces[i].IsVisible(_vTarget - _vCameraPos))
                        continue;
                    // color
                    faces[i].ColorFill = box.Colors[i];
                    double cosA = System.Math.Abs(Vector3D.DotProduct(faces[i].Normal, VLight));
                    Color color = Color.FromArgb((int)(faces[i].ColorFill.R * cosA), (int)(faces[i].ColorFill.G * cosA), (int)(faces[i].ColorFill.B * cosA));
                    // points
                    Vector3D[] points3D = faces[i].Points;
                    Point[] pt = TransformPoint(GetCurrentTransformation(), points3D);
                    //  draw solid face
                    Brush brush = new SolidBrush(color);
                    g.FillPolygon(brush, pt);
                    // draw textures
                    if (null != face.Textures && ShowTextures)
                        foreach (Texture texture in face.Textures)
                        {
                            Point[] ptsImage = TransformPoint(GetCurrentTransformation(), box.PointsImage(i, texture));
                            Point[] pts = new Point[3];
                            pts[0] = ptsImage[3];
                            pts[1] = ptsImage[2];
                            pts[2] = ptsImage[0];
                            g.DrawImage(texture.Bitmap, pts);
                        }
                    // draw path
                    Brush brushPath = new SolidBrush(faces[i].ColorPath);
                    Pen penPathThick = new Pen(brushPath, box.IsBundle ? 2.0f : 1.5f);
                    int ptCount = pt.Length;
                    for (int j = 1; j < ptCount; ++j)
                        g.DrawLine(penPathThick, pt[j - 1], pt[j]);
                    g.DrawLine(penPathThick, pt[ptCount - 1], pt[0]);
                    // draw bundle lines
                    if (box.IsBundle && i < 4)
                    {
                        Pen penPathThin = new Pen(brushPath, 1.5f);
                        int noSlice = Math.Min(box.BundleFlats, 4);
                        for (int iSlice = 0; iSlice < noSlice - 1; ++iSlice)
                        {
                            Vector3D[] ptSlice = new Vector3D[2];
                            ptSlice[0] = points3D[0] + ((double)(iSlice + 1) / (double)noSlice) * (points3D[3] - points3D[0]);
                            ptSlice[1] = points3D[1] + ((double)(iSlice + 1) / (double)noSlice) * (points3D[2] - points3D[1]);

                            Point[] pt2D = TransformPoint(GetCurrentTransformation(), ptSlice);
                            g.DrawLine(penPathThin, pt2D[0], pt2D[1]);
                        }
                    }
                }

                // draw box tape
                if (box.ShowTape && faces[5].IsVisible(_vTarget - _vCameraPos))
                {
                    // get color
                    double cosA = System.Math.Abs(Vector3D.DotProduct(faces[5].Normal, VLight));
                    Color color = Color.FromArgb((int)(box.TapeColor.R * cosA), (int)(box.TapeColor.G * cosA), (int)(box.TapeColor.B * cosA));
                    // instantiate brush
                    Brush brushTape = new SolidBrush(color);
                    // get tape points
                    Point[] pts = TransformPoint(GetCurrentTransformation(), box.TapePoints);
                    // fill polygon
                    g.FillPolygon(brushTape, pts);
                    // draw path
                    Brush brushPath = new SolidBrush(faces[5].ColorPath);
                    Pen penPathThick = new Pen(brushPath, 1.5f);
                    int ptCount = pts.Length;
                    for (int j = 1; j < ptCount; ++j)
                        g.DrawLine(penPathThick, pts[j - 1], pts[j]);
                    g.DrawLine(penPathThick, pts[ptCount - 1], pts[0]);
                }
            }
            if (_showBoxIds)
            {
                // draw box id
                Point ptId = TransformPoint(GetCurrentTransformation(), box.TopFace.Center);
                g.DrawString(
                    box.PickId.ToString()
                    , new Font("Arial", 8.0f)
                    , Brushes.Black
                    , new Rectangle(ptId.X - 15, ptId.Y - 10, 30, 20)
                    , StringFormat.GenericDefault);
                g.DrawString(
                    _boxDrawingCounter.ToString()
                    , new Font("Arial", 8.0f)
                    , Brushes.Red
                    , new Rectangle(ptId.X + 5, ptId.Y - 10, 30, 20)
                    , StringFormat.GenericDefault);
            }
            ++_boxDrawingCounter;
        }

        internal void Draw(Pack pack)
        {
            System.Drawing.Graphics g = Graphics;
            Vector3D[] points = pack.Points;
        }
        #endregion

        #region Draw cylinder
        internal void Draw(Cylinder cyl)
        {
            System.Drawing.Graphics g = Graphics;

            // build pen path
            Brush brushPath = new SolidBrush(cyl.ColorPath);
            Pen penPathThick = new Pen(brushPath, 1.7f);
            Pen penPathThin = new Pen(brushPath, 1.5f);

            // bottom (draw only path)
            Point[] ptsBottom = TransformPoint(GetCurrentTransformation(), cyl.BottomPoints);
            g.DrawPolygon(penPathThick, ptsBottom);
            // top
            Point[] ptsTop = TransformPoint(GetCurrentTransformation(), cyl.TopPoints);
            g.DrawPolygon(penPathThick, ptsTop);

            // outer wall
            Face[] facesWalls = cyl.FacesWalls;
            foreach (Face face in facesWalls)
            {
                Vector3D normal = face.Normal;
                // visible ?
                if (!face.IsVisible(_vTarget - _vCameraPos))
                    continue;

                // color
                double cosA = System.Math.Abs(Vector3D.DotProduct(face.Normal, VLight));
                if (cosA < 0 || cosA > 1) cosA = 1.0;
                Color color = Color.FromArgb((int)(face.ColorFill.R * cosA), (int)(face.ColorFill.G * cosA), (int)(face.ColorFill.B * cosA));
                // brush
                Brush brush = new SolidBrush(color);
                // draw polygon
                Point[] ptsFace = TransformPoint(GetCurrentTransformation(), face.Points);
                g.FillPolygon(brush, ptsFace);
            }
            // top
            double cosTop = System.Math.Abs(Vector3D.DotProduct(HalfAxis.ToVector3D(cyl.Position.Direction), VLight));
            Color colorTop = Color.FromArgb((int)(cyl.ColorTop.R * cosTop), (int)(cyl.ColorTop.G * cosTop), (int)(cyl.ColorTop.B * cosTop));
            Brush brushTop = new SolidBrush(colorTop);
            bool topVisible = Vector3D.DotProduct(HalfAxis.ToVector3D(cyl.Position.Direction), _vTarget - _vCameraPos) < 0;

            if (cyl.DiameterInner > 0)
            {
                Face[] facesTop = cyl.FacesTop;
                foreach (Face face in facesTop)
                {
                    Vector3D normal = face.Normal;

                    // visible ?
                    if (!face.IsVisible(_vTarget - _vCameraPos))
                        continue;
                    // color
                    // draw polygon
                    Point[] ptsFace = TransformPoint(GetCurrentTransformation(), face.Points);
                    g.FillPolygon(brushTop, ptsFace);
                }
            }
            else
            {
                if (topVisible)
                    g.FillPolygon(brushTop, ptsTop);
                else
                    g.FillPolygon(brushTop, ptsBottom);
            }

            if (topVisible)
                g.DrawPolygon(penPathThin, ptsTop);
            else
                g.DrawPolygon(penPathThin, ptsBottom);

            ++_boxDrawingCounter;
        }
        #endregion
    }
}
