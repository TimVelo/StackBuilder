#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sharp3D.Math.Core;
#endregion

namespace treeDiM.StackBuilder.Basics
{
    public interface ILayerSolver
    {
        List<Layer2D> BuildLayers(Vector3D dimBox, Vector2D dimContainer, ConstraintSetAbstract constraintSet, bool keepOnlyBest);
        Layer2D BuildLayer(Vector3D dimBox, Vector2D actualDimensions, LayerDesc layerDesc);
        Layer2D BuildLayer(Vector3D dimBox, Vector2D dimContainer, LayerDesc layerDesc, Vector2D actualDimensions);
        bool GetDimensions(List<LayerDesc> layers, Vector3D dimBox, Vector2D dimContainer, out Vector2D actualDimensions);
    }

    public class SolutionItem
    {
        #region Data members
        private int _indexLayer = 0, _indexInterlayer = -1;
        private bool _symmetryX = false, _symmetryY = false;
        #endregion

        #region Constructors
        public SolutionItem(int indexLayer, int indexInterlayer, bool symetryX, bool symetryY)
        {
            _indexLayer = indexLayer;
            _indexInterlayer = indexInterlayer;
            _symmetryX = symetryX;
            _symmetryY = SymmetryY;
        }
        public SolutionItem(SolutionItem solItem)
        {
            _indexLayer = solItem._indexLayer;
            _indexInterlayer = solItem._indexInterlayer;
            _symmetryX = solItem._symmetryX;
            _symmetryY = solItem._symmetryY;
        }
        #endregion

        #region Public properties
        public bool SymmetryX
        {
            get { return _symmetryX; }
            set { _symmetryX = value; }
        }
        public bool SymmetryY
        {
            get { return _symmetryY; }
            set { _symmetryY = value; }
        }
        public bool HasInterlayer   { get { return _indexInterlayer != -1; } }
        public int InterlayerIndex { get { return _indexInterlayer; } set { _indexInterlayer = value; } }
        public int LayerIndex { get { return _indexLayer; } set { _indexLayer = value; } }
        #endregion

        #region Public methods
        public void SetInterlayer(int indexInterlayer)
        {
            _indexInterlayer = indexInterlayer;
        }
        public void InverseSymetry(int axis)
        {
            if (axis == 0) _symmetryX = !_symmetryX;
            else if (axis == 1) _symmetryY = !_symmetryY;
            else throw new Exception("Invalid axis of symetry");
        }
        #endregion
    }

    public class Solution
    {
        #region Data members
        private List<LayerDesc> _layerDescriptors;
        private List<InterlayerProperties> _interlayers;
        private List<SolutionItem> _solutionItems;
        private Analysis _analysis;

        private int _selectedIndex;

        private List<Layer2D> _layers;
        private static ILayerSolver _solver;

        // cached data
        private BBox3D _bbox = BBox3D.Initial;
        #endregion

        #region Static methods
        public static void SetSolver(ILayerSolver solver)
        { _solver = solver; }
        #endregion

        #region Constructor
        public Solution(Analysis analysis, List<LayerDesc> layerDescs)
        {
            _analysis = analysis;
            _layerDescriptors = layerDescs;

            _interlayers = new List<InterlayerProperties>();
            _solutionItems = new List<SolutionItem>();

            RebuildLayers();
            InitializeSolutionItemList();
        }
        #endregion

        #region Reprocessing
        private void RebuildLayers()
        {
            // sanity checks
            if (null == _solver)
                throw new Exception("Solver not initialized");
            if (0 == _layerDescriptors.Count)
                throw new Exception("No layer descriptors available");

            _layers = new List<Layer2D>();
            // build list of used layers
            List<LayerDesc> usedLayers = new List<LayerDesc>();
            if (null != _solutionItems && _solutionItems.Count > 0)
            {
                foreach (SolutionItem item in _solutionItems)
                {
                    if (!usedLayers.Contains(_layerDescriptors[item.LayerIndex]))
                        usedLayers.Add(_layerDescriptors[item.LayerIndex]);
                }
            }
            if (0 == usedLayers.Count)
                usedLayers.Add(_layerDescriptors[0]);
            // get dimensions
            Vector2D actualDimensions = Vector2D.Zero;
            _solver.GetDimensions(usedLayers, _analysis.ContentDimensions, _analysis.ContainerDimensions, out actualDimensions);

            // actually build layers
            foreach (LayerDesc layerDesc in _layerDescriptors)
                _layers.Add(_solver.BuildLayer(_analysis.ContentDimensions, _analysis.ContainerDimensions, layerDesc, actualDimensions));
        }
        private void InitializeSolutionItemList()
        { 
            _solutionItems = new List<SolutionItem>();
            _interlayers = new List<InterlayerProperties>();

            bool criterionReached = false;
            double zTop = _analysis.Offset.Z;
            double weight = _analysis.ContainerWeight;
            while (!criterionReached)
            {
                weight += _layers[0].Count * _analysis.ContentWeight;
                zTop += _layers[0].BoxHeight;
                ConstraintSetAbstract constraintSet = _analysis.ConstraintSet;
                criterionReached = (ConstraintSet.OptMaxHeight.Activated && zTop >= constraintSet.OptMaxHeight.Value)
                    || (ConstraintSet.OptMaxWeight.Activated && weight >= constraintSet.OptMaxWeight.Value);
                if (criterionReached)
                    break;
                else
                    _solutionItems.Add( new SolutionItem(0, -1, false, false) );
            }
        }
        private void RebuildSolutionItemList()
        {
            bool criterionReached = false;
            double zTop = _analysis.Offset.Z;
            double weight = _analysis.ContainerWeight;
            foreach (SolutionItem solItem in _solutionItems)
            {
                weight += _layers[solItem.LayerIndex].Count * _analysis.ContentWeight;
                zTop += _layers[solItem.LayerIndex].BoxHeight + ((-1 != solItem.InterlayerIndex) ? _interlayers[solItem.InterlayerIndex].Thickness : 0.0);

                ConstraintSetAbstract constraintSet = _analysis.ConstraintSet;
                criterionReached = (ConstraintSet.OptMaxHeight.Activated && zTop >= constraintSet.OptMaxHeight.Value)
                    || (ConstraintSet.OptMaxWeight.Activated && weight >= constraintSet.OptMaxWeight.Value);

                if (criterionReached)
                    break;
            }

            // add layers until 
            while (!criterionReached)
            {
                SolutionItem solItem = _solutionItems.Last();

                weight += _layers[solItem.LayerIndex].Count * _analysis.ContentWeight;
                zTop += _layers[solItem.LayerIndex].BoxHeight + ((-1 != solItem.InterlayerIndex) ? _interlayers[solItem.InterlayerIndex].Thickness : 0.0);

                _solutionItems.Add(new SolutionItem(solItem));

                ConstraintSetAbstract constraintSet = _analysis.ConstraintSet;
                criterionReached = (ConstraintSet.OptMaxHeight.Activated && zTop >= constraintSet.OptMaxHeight.Value)
                    || (ConstraintSet.OptMaxWeight.Activated && weight >= constraintSet.OptMaxWeight.Value);
            }

            // remove unneeded layer 
            while (criterionReached)
            {
                SolutionItem solItem = _solutionItems.Last();

                weight -= _layers[solItem.LayerIndex].Count * _analysis.ContentWeight;
                zTop -= _layers[solItem.LayerIndex].BoxHeight + ((-1 != solItem.InterlayerIndex) ? _interlayers[solItem.InterlayerIndex].Thickness : 0.0);

                ConstraintSetAbstract constraintSet = _analysis.ConstraintSet;
                criterionReached = (ConstraintSet.OptMaxHeight.Activated && zTop >= constraintSet.OptMaxHeight.Value)
                    || (ConstraintSet.OptMaxWeight.Activated && weight >= constraintSet.OptMaxWeight.Value);

                _solutionItems.Remove(solItem);
            }
            // reset bounding box to force recompute
            _bbox.Reset();
        }
        private int GetInterlayerIndex(InterlayerProperties interlayer)
        {
            if (null == interlayer) return -1;
            if (!_interlayers.Contains(interlayer))
                _interlayers.Add(interlayer);
            return _interlayers.FindIndex(item => item == interlayer);
        }
        #endregion

        #region Apply selection
        public void SelectLayer(int index)
        {
            _selectedIndex = index;
            // rebuild layers
            RebuildLayers();
            // rebuild solution item list
            RebuildSolutionItemList();
        }
        public SolutionItem SelectedSolutionItem
        {
            get
            {
                if (!HasValidSelection) return null;
                return _solutionItems[_selectedIndex];
            }
        }
        public void SetLayerTypeOnSelected(int iLayerType)
        {
            // check selected layer
            if (!HasValidSelection) return;
            // get selected solution item
            SolutionItem item = _solutionItems[SelectedLayerIndex];
            item.LayerIndex = iLayerType;
        }
        public void SetInterlayerOnSelected(InterlayerProperties interlayer)
        {
            // check selected layer
            if (!HasValidSelection) return;
            // get solution item
            SolutionItem item = _solutionItems[SelectedLayerIndex];
            item.SetInterlayer(GetInterlayerIndex(interlayer));
            // rebuild solution item list
            RebuildSolutionItemList();
        }
        public void ApplySymetryOnSelected(int axis)
        { 
            // check selected layer
            if (!HasValidSelection) return;
            // get solution item
            SolutionItem item = _solutionItems[SelectedLayerIndex];
            item.InverseSymetry(axis);
        }
        public InterlayerProperties SelectedInterlayer
        {
            get
            {
                if (!HasValidSelection) return null;
                if (-1 == _solutionItems[_selectedIndex].InterlayerIndex) return null;
                if (_solutionItems[_selectedIndex].InterlayerIndex >= _interlayers.Count) return null;
                return _interlayers[ _solutionItems[_selectedIndex].InterlayerIndex ]; 
            }
        }
        #endregion

        #region Public properties
        public int SelectedLayerIndex
        {
            get { return _selectedIndex; }
        }
        public Analysis Analysis
        {
            get { return _analysis; }
        }
        public ConstraintSetAbstract ConstraintSet
        {
            get { return _analysis.ConstraintSet; }
        }
        public List<LayerDesc> LayerDescriptors
        {
            get { return _layerDescriptors; }
        }
        public List<InterlayerProperties> Interlayers
        {
            get { return _interlayers; } 
        }
        public List<ILayer> Layers
        {
            get
            {
                List<ILayer> llayers = new List<ILayer>();

                int iBoxCount = 0;
                int iInterlayerCount = 0;

                // starting height
                double zLayer = 0.0;
                // build layers
                foreach (SolutionItem solItem in _solutionItems)
                {
                    if (solItem.HasInterlayer)
                    {
                        InterlayerProperties interlayer = _interlayers[solItem.InterlayerIndex];//_analysis.Interlayer(solItem.InterlayerIndex);
                        llayers.Add(new InterlayerPos(zLayer + Analysis.Offset.Z, solItem.InterlayerIndex));
                        zLayer += interlayer.Thickness;
                        ++iInterlayerCount;
                    }

                    Layer2D currentLayer = _layers[solItem.LayerIndex];

                    BoxLayer boxLayer = new BoxLayer(zLayer, solItem.LayerIndex);
                    foreach (LayerPosition layerPos in currentLayer)
                    {
                        LayerPosition layerPosTemp = AdjustLayerPosition(layerPos, solItem.SymmetryX, solItem.SymmetryY);
                        boxLayer.Add(new BoxPosition(
                            layerPosTemp.Position + Analysis.Offset + zLayer * Vector3D.ZAxis
                            , layerPosTemp.LengthAxis
                            , layerPosTemp.WidthAxis
                            ));
                       
                        ++iBoxCount;
                    }
                    llayers.Add(boxLayer);

                    zLayer += currentLayer.BoxHeight;
                }
                return llayers;
            }
        }
        public BBox3D BBoxLoad
        {
            get
            {
                if (!_bbox.IsValid)
                {
                    foreach (ILayer layer in Layers)
                    { 
                        BoxLayer blayer = layer as BoxLayer;
                        if (null != blayer)
                            _bbox.Extend(blayer.BoundingBox(Analysis.ContentDimensions));
                    }
                }
                return _bbox;
            }
        }

        public BBox3D BBoxLoadWDeco
        {   get { return Analysis.BBoxLoadWDeco(BBoxLoad); } }
        public BBox3D BBoxGlobal
        {   get { return Analysis.BBoxGlobal(BBoxLoad); } }
        #endregion

        #region Helpers
        /// <summary>
        /// if box bottom oriented to Z+, reverse box
        /// </summary>
        private LayerPosition AdjustLayerPosition(LayerPosition layerPos, bool reflectionX, bool reflectionY)
        {
            
            Vector3D dimensions = Analysis.ContentDimensions;
            Vector2D containerDims = Analysis.ContainerDimensions;

            // implement change
            LayerPosition layerPosTemp = new LayerPosition(layerPos);

            // apply symetry in X
            if (reflectionX)
            {
                Matrix4D matRot = new Matrix4D(
                  1.0, 0.0, 0.0, 0.0
                  , 0.0, -1.0, 0.0, 0.0
                  , 0.0, 0.0, 1.0, 0.0
                  , 0.0, 0.0, 0.0, 1.0
                  );
                Vector3D vTranslation = new Vector3D(0.0, containerDims.Y, 0.0);
                layerPosTemp = ApplyReflection(layerPosTemp, matRot, vTranslation);
            }
            // apply symetry in Y
            if (reflectionY)
            {
                Matrix4D matRot = new Matrix4D(
                    -1.0, 0.0, 0.0, 0.0
                    , 0.0, 1.0, 0.0, 0.0
                    , 0.0, 0.0, 1.0, 0.0
                    , 0.0, 0.0, 0.0, 1.0
                    );
                Vector3D vTranslation = new Vector3D(containerDims.X, 0.0, 0.0);
                layerPosTemp = ApplyReflection(layerPosTemp, matRot, vTranslation);
            }
            return layerPosTemp.Adjusted(dimensions);
        }

        LayerPosition ApplyReflection(LayerPosition layerPos, Matrix4D matRot, Vector3D vTranslation)
        {
            Vector3D dimensions = Analysis.ContentDimensions;
            Transform3D transfRot = new Transform3D(matRot);
            HalfAxis.HAxis axisLength = HalfAxis.ToHalfAxis(transfRot.transform(HalfAxis.ToVector3D(layerPos.LengthAxis)));
            HalfAxis.HAxis axisWidth = HalfAxis.ToHalfAxis(transfRot.transform(HalfAxis.ToVector3D(layerPos.WidthAxis)));
            matRot.M14 = vTranslation[0];
            matRot.M24 = vTranslation[1];
            matRot.M34 = vTranslation[2];
            Transform3D transfRotTranslation = new Transform3D(matRot);

            Vector3D transPos = transfRotTranslation.transform( new Vector3D(layerPos.Position.X, layerPos.Position.Y, layerPos.Position.Z) );
            return new LayerPosition(
                new Vector3D(transPos.X, transPos.Y, transPos.Z)
                    - dimensions.Z * Vector3D.CrossProduct(HalfAxis.ToVector3D(axisLength), HalfAxis.ToVector3D(axisWidth))
                , axisLength
                , axisWidth);
        }

        private bool HasValidSelection
        {
            get { return _selectedIndex >= 0 && _selectedIndex < _solutionItems.Count; }
        }
        #endregion
    }
}
