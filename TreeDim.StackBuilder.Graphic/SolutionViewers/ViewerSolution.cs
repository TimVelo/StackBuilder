#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using treeDiM.StackBuilder.Basics;

using Sharp3D.Math.Core;

using log4net;
#endregion

namespace treeDiM.StackBuilder.Graphics
{
    public abstract class Viewer : IDisposable
    {
        #region Data members
        /// <summary>
        /// list of picking boxes with id
        /// </summary>
        private List<Tuple<BBox3D, uint>> _listPickingBox = new List<Tuple<BBox3D, uint>>();
        private Transform3D _currentTransf;
        private Vector3D _viewDir;
        #endregion

        #region Abstract methods
        public abstract void Draw(Graphics3D graphics, bool showDimensions);
        public abstract void Draw(Graphics2D graphics, bool showDimensions);
        #endregion

        #region Picking
        protected void AddPickingBox(BBox3D bbox, uint id)
        {
            _listPickingBox.Add( new Tuple<BBox3D, uint>(bbox, id) );
        }
        protected Ray GetPickingRay(int x, int y)
        {
            // normalised_x = 2 * mouse_x / win_width - 1
            // normalised_y = 1 - 2 * mouse_y / win_height
            // note the y pos is inverted, so +y is at the top of the screen
            // unviewMat = (projectionMat * modelViewMat).inverse()
            // near_point = unviewMat * Vec(normalised_x, normalised_y, 0, 1)
            // camera_pos = ray_origin = modelViewMat.inverse().col(4)
            // ray_dir = near_point - camera_pos
            Transform3D eye2worldTransf = CurrentTransformation.Inverse();
            Vector3D vNear = eye2worldTransf.transform(new Vector3D((double)x, (double)y, 0.0));
            Vector3D vFar = eye2worldTransf.transform(new Vector3D((double)x, (double)y, 1.0));
            return new Ray(vNear, vFar);
        }
        public Transform3D CurrentTransformation
        {
            get { return _currentTransf; }
            set { _currentTransf = value; }
        }
        public Vector3D ViewDir
        {
            get { return _viewDir; }
            set { _viewDir = value; }
        }

        public bool TryPicking(int x, int y, out uint index)
        {
            Ray ray = GetPickingRay(x, y);
            index = 0;
            foreach (Tuple<BBox3D, uint> tBox in _listPickingBox)
            {
                index = tBox.Item2;
                Vector3D ptMin = tBox.Item1.PtMin;
                Vector3D ptMax = tBox.Item1.PtMax;

                Vector3D[] vertices = {
                    new Vector3D(ptMin.X, ptMin.Y, ptMin.Z)       // 0
                    , new Vector3D(ptMax.X, ptMin.Y, ptMin.Z)     // 1
                    , new Vector3D(ptMax.X, ptMax.Y, ptMin.Z)     // 2
                    , new Vector3D(ptMin.X, ptMax.Y, ptMin.Z)     // 3
                    , new Vector3D(ptMin.X, ptMin.Y, ptMax.Z)     // 4
                    , new Vector3D(ptMax.X, ptMin.Y, ptMax.Z)     // 5
                    , new Vector3D(ptMax.X, ptMax.Y, ptMax.Z)     // 6
                    , new Vector3D(ptMin.X, ptMax.Y, ptMax.Z)     // 7
                };
                Face[] faces = new Face[]
                {
                    new Face(0, vertices[0], vertices[1], vertices[5], vertices[4], false),
                    new Face(0, vertices[1], vertices[2], vertices[6], vertices[5], false),
                    new Face(0, vertices[2], vertices[3], vertices[7], vertices[6], false),
                    new Face(0, vertices[3], vertices[0], vertices[4], vertices[7], false),
                };

                foreach (Face f in faces)
                {
                    Vector3D ptInter;
                    if (f.IsVisible(ViewDir) && f.RayIntersect(ray, out ptInter))
                        return true;
                }
            }
            return false;
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
        }
        #endregion
    }

    public class ViewerSolution : Viewer
    {
        #region Data members
        private Solution _solution;
        #endregion

        #region Constructor
        public ViewerSolution(Solution solution)
        {
            _solution = solution;
        }
        #endregion

        #region Viewer
        public override void Draw(Graphics3D graphics, bool showDimensions)
        {
            if (null == _solution) return;
            AnalysisCasePallet analysisCasePallet = _solution.Analysis as AnalysisCasePallet;

            // ### draw pallet
            Pallet pallet = new Pallet(analysisCasePallet.PalletProperties);
            pallet.Draw(graphics, Transform3D.Identity);

            // ### draw solution
            uint layerId = 0, pickId = 0;
            List<ILayer> layers = _solution.Layers;
            foreach (ILayer layer in layers)
            {
                // ### layer of boxes
                BoxLayer blayer = layer as BoxLayer;
                if (null != blayer)
                {
                    BBox3D bbox = new BBox3D();
                    foreach (BoxPosition bPosition in blayer)
                    {
                        Box b = new Box(pickId++, analysisCasePallet.BProperties, bPosition);
                        graphics.AddBox(b);
                        bbox.Extend(b.BBox);
                    }
                    // add layer BBox
                    AddPickingBox(bbox, layerId);
                    // draw bounding box around selected layer
                    if (layerId == _solution.SelectedLayerIndex)
                        DrawLayerBoundingBox(graphics, bbox);
                    ++layerId;
                }
                // ### intetrlayer
                InterlayerPos interlayerPos = layer as InterlayerPos;
                if (null != interlayerPos)
                {
                    InterlayerProperties interlayerProp = _solution.Interlayers[interlayerPos.TypeId];//analysisCasePallet.Interlayer(interlayerPos.TypeId);
                    if (null != interlayerProp)
                    {
                        Box box = new Box(pickId++, interlayerProp);
                        box.Position = new Vector3D(
                            0.5 * (analysisCasePallet.PalletProperties.Length - interlayerProp.Length)
                            , 0.5 * (analysisCasePallet.PalletProperties.Width - interlayerProp.Width)
                            , interlayerPos.ZLow
                            );
                        graphics.AddBox(box);
                    }
                }
            }
            BBox3D loadBBox = _solution.BBoxLoad;
            BBox3D loadBBoxWDeco = _solution.BBoxLoadWDeco;
            #region Pallet corners
            // ### pallet corners : Begin
            Corner[] corners = new Corner[4];
            if (analysisCasePallet.HasPalletCorners)
            {
                // positions
                Vector3D[] cornerPositions =
                {
                    loadBBox.PtMin
                    , new Vector3D(loadBBox.PtMax.X, loadBBox.PtMin.Y, loadBBox.PtMin.Z)
                    , new Vector3D(loadBBox.PtMax.X, loadBBox.PtMax.Y, loadBBox.PtMin.Z)
                    , new Vector3D(loadBBox.PtMin.X, loadBBox.PtMax.Y, loadBBox.PtMin.Z)
                };
                // length axes
                HalfAxis.HAxis[] lAxes =
                {
                    HalfAxis.HAxis.AXIS_X_P,
                    HalfAxis.HAxis.AXIS_Y_P,
                    HalfAxis.HAxis.AXIS_X_N,
                    HalfAxis.HAxis.AXIS_Y_N
                };
                // width axes
                HalfAxis.HAxis[] wAxes =
                {
                    HalfAxis.HAxis.AXIS_Y_P,
                    HalfAxis.HAxis.AXIS_X_N,
                    HalfAxis.HAxis.AXIS_Y_N,
                    HalfAxis.HAxis.AXIS_X_P
                };
                // corners
                if (analysisCasePallet.HasPalletCorners)
                {
                    for (int i = 0; i < 4; ++i)
                    {
                        corners[i] = new Corner(0, analysisCasePallet.PalletCornerProperties);
                        corners[i].Height = Math.Min(analysisCasePallet.PalletCornerProperties.Length, loadBBox.Height);
                        corners[i].SetPosition(cornerPositions[i], lAxes[i], wAxes[i]);
                        corners[i].DrawBegin(graphics);
                    }
                }
            }
            #endregion

            #region Pallet cap
            #endregion

            #region Pallet film
            // ### pallet film
            Film film = null;
            if (analysisCasePallet.HasPalletFilm)
            {
                PalletFilmProperties palletFilmProperties = analysisCasePallet.PalletFilmProperties;
                film = new Film(
                    palletFilmProperties.Color,
                    palletFilmProperties.UseTransparency,
                    palletFilmProperties.UseHatching,
                    palletFilmProperties.HatchSpacing,
                    palletFilmProperties.HatchAngle);
                film.AddRectangle(new FilmRectangle(loadBBoxWDeco.PtMin,
                    HalfAxis.HAxis.AXIS_X_P, HalfAxis.HAxis.AXIS_Z_P, new Vector2D(loadBBoxWDeco.Length, loadBBoxWDeco.Height), 0.0));
                film.AddRectangle(new FilmRectangle(loadBBoxWDeco.PtMin + loadBBoxWDeco.Length * Vector3D.XAxis,
                    HalfAxis.HAxis.AXIS_Y_P, HalfAxis.HAxis.AXIS_Z_P, new Vector2D(loadBBoxWDeco.Width, loadBBoxWDeco.Height), 0.0));
                film.AddRectangle(new FilmRectangle(loadBBoxWDeco.PtMin + loadBBoxWDeco.Length * Vector3D.XAxis + loadBBoxWDeco.Width * Vector3D.YAxis,
                    HalfAxis.HAxis.AXIS_X_N, HalfAxis.HAxis.AXIS_Z_P, new Vector2D(loadBBoxWDeco.Length, loadBBoxWDeco.Height), 0.0));
                film.AddRectangle(new FilmRectangle(loadBBoxWDeco.PtMin + loadBBoxWDeco.Width * Vector3D.YAxis,
                    HalfAxis.HAxis.AXIS_Y_N, HalfAxis.HAxis.AXIS_Z_P, new Vector2D(loadBBoxWDeco.Width, loadBBoxWDeco.Height), 0.0));
                film.AddRectangle(new FilmRectangle(loadBBoxWDeco.PtMin + loadBBoxWDeco.Height * Vector3D.ZAxis,
                    HalfAxis.HAxis.AXIS_X_P, HalfAxis.HAxis.AXIS_Y_P, new Vector2D(loadBBoxWDeco.Length, loadBBoxWDeco.Width),
                    UnitsManager.ConvertLengthFrom(200.0, UnitsManager.UnitSystem.UNIT_METRIC1)));
                film.DrawBegin(graphics);
            }
            #endregion

            // ### dimensions
            if (showDimensions)
            {
                graphics.AddDimensions(
                    new DimensionCube(BoundingBoxDim(Properties.Settings.Default.DimCasePalletSol1)
                    , Color.Black, false));
                graphics.AddDimensions(
                    new DimensionCube(BoundingBoxDim(Properties.Settings.Default.DimCasePalletSol2)
                    , Color.Red, true));
            }

            #region Pallet corners
            // pallet corners : End
            if (analysisCasePallet.HasPalletCorners)
            {
                for (int i = 0; i < 4; ++i)
                    corners[i].DrawEnd(graphics);
            }
            #endregion

            #region Pallet Cap
            // ### pallet cap
            if (analysisCasePallet.HasPalletCap)
            {
                PalletCapProperties capProperties = analysisCasePallet.PalletCapProperties;
                Vector3D pos = new Vector3D(
                    0.5 * (analysisCasePallet.PalletProperties.Length - capProperties.Length),
                    0.5 * (analysisCasePallet.PalletProperties.Width - capProperties.Width),
                    loadBBox.PtMax.Z - capProperties.InsideHeight);
                PalletCap cap = new PalletCap(0, capProperties, pos);
                cap.DrawEnd(graphics);
            }
            #endregion

            #region Pallet film
            // pallet film : End
            if (analysisCasePallet.HasPalletFilm)
                film.DrawEnd(graphics);
            #endregion
        }

        public override void Draw(Graphics2D graphics, bool showDimensions)
        {
            if (null == _solution) return;
        }
        #endregion

        #region Helpers
        private BBox3D BoundingBoxDim(int index)
        {
            AnalysisCasePallet analysisCasePallet = _solution.Analysis as AnalysisCasePallet;

            switch (index)
            {
                case 0: return _solution.BBoxGlobal;
                case 1: return _solution.BBoxLoadWDeco;
                case 2: return analysisCasePallet.PalletProperties.BoundingBox;
                case 3: return new BBox3D(0.0, 0.0, 0.0, analysisCasePallet.PalletProperties.Length, analysisCasePallet.PalletProperties.Width, 0.0);
                default: return _solution.BBoxGlobal;
            }
        }

        private void DrawLayerBoundingBox(Graphics3D graphics, BBox3D bbox)
        {
            Vector3D ptMin = bbox.PtMin;
            Vector3D ptMax = bbox.PtMax;

            Vector3D[] vertices = {
                            new Vector3D(ptMin.X, ptMin.Y, ptMin.Z)       // 0
                            , new Vector3D(ptMax.X, ptMin.Y, ptMin.Z)     // 1
                            , new Vector3D(ptMax.X, ptMax.Y, ptMin.Z)     // 2
                            , new Vector3D(ptMin.X, ptMax.Y, ptMin.Z)     // 3
                            , new Vector3D(ptMin.X, ptMin.Y, ptMax.Z)     // 4
                            , new Vector3D(ptMax.X, ptMin.Y, ptMax.Z)     // 5
                            , new Vector3D(ptMax.X, ptMax.Y, ptMax.Z)     // 6
                            , new Vector3D(ptMin.X, ptMax.Y, ptMax.Z)     // 7
                        };
            Face[] faces = {
                            new Face(0, vertices[0], vertices[1], vertices[5], vertices[4], false),
                            new Face(0, vertices[1], vertices[2], vertices[6], vertices[5], false),
                            new Face(0, vertices[2], vertices[3], vertices[7], vertices[6], false),
                            new Face(0, vertices[3], vertices[0], vertices[4], vertices[7], false),
                        };

            foreach (Face f in faces)
            {
                if (f.IsVisible(ViewDir))
                {
                    Vector3D[] points = f.Points;
                    graphics.AddSegment(new Segment(points[0], points[1], Color.Red));
                    graphics.AddSegment(new Segment(points[1], points[2], Color.Red));
                    graphics.AddSegment(new Segment(points[2], points[3], Color.Red));
                    graphics.AddSegment(new Segment(points[3], points[0], Color.Red));
                }
            }
        }
        #endregion
    }
}
