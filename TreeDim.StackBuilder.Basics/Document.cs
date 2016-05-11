﻿#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Linq;

using treeDiM.StackBuilder.Basics;

using Sharp3D.Math.Core;
using log4net;
#endregion

namespace treeDiM.StackBuilder.Basics
{
    #region IDocumentListener
    /// <summary>
    /// Listener class that is notified when the document is modified
    /// </summary>
    public interface IDocumentListener
    {
        // new
        void OnNewDocument(Document doc);
        void OnNewTypeCreated(Document doc, ItemBase itemBase);
        void OnNewAnalysisCreated(Document doc, Analysis analysis);

        void OnNewCasePalletAnalysisCreated(Document doc, CasePalletAnalysis analysis);
        void OnNewPackPalletAnalysisCreated(Document doc, PackPalletAnalysis analysis);
        void OnNewCylinderPalletAnalysisCreated(Document doc, CylinderPalletAnalysis analysis);
        void OnNewHCylinderPalletAnalysisCreated(Document doc, HCylinderPalletAnalysis analysis);
        void OnNewBoxCaseAnalysisCreated(Document doc, BoxCaseAnalysis analysis);
        void OnNewBoxCasePalletAnalysisCreated(Document doc, BoxCasePalletAnalysis caseAnalysis);
        void OnNewTruckAnalysisCreated(Document doc, CasePalletAnalysis analysis, SelCasePalletSolution selSolution, TruckAnalysis truckAnalysis);
        void OnNewECTAnalysisCreated(Document doc, CasePalletAnalysis analysis, SelCasePalletSolution selSolution, ECTAnalysis ectAnalysis);
        // remove
        void OnTypeRemoved(Document doc, ItemBase itemBase);
        void OnAnalysisRemoved(Document doc, ItemBase itemBase); 
        void OnTruckAnalysisRemoved(Document doc, CasePalletAnalysis analysis, SelCasePalletSolution selSolution, TruckAnalysis truckAnalysis);
        void OnECTAnalysisRemoved(Document doc, CasePalletAnalysis analysis, SelCasePalletSolution selSolution, ECTAnalysis ectAnalysis);
        // close
        void OnDocumentClosed(Document doc);
    }
    #endregion

    #region Document
    /// <summary>
    /// Classes that encapsulates data
    /// The application is MDI and might host several Document instance
    /// </summary>
    public class Document
    {
        #region Data members
        private string _name, _description, _author;
        private DateTime _dateCreated;
        private UnitsManager.UnitSystem _unitSystem;
        private List<ItemBase> _typeList = new List<ItemBase>();
        private List<Analysis> _analyses = new List<Analysis>();

        private List<CasePalletAnalysis> _casePalletAnalyses = new List<CasePalletAnalysis>();
        private List<PackPalletAnalysis> _packPalletAnalyses = new List<PackPalletAnalysis>();
        private List<CylinderPalletAnalysis> _cylinderPalletAnalyses = new List<CylinderPalletAnalysis>();
        private List<HCylinderPalletAnalysis> _hCylinderPalletAnalyses = new List<HCylinderPalletAnalysis>();
        private List<BoxCaseAnalysis> _boxCaseAnalyses = new List<BoxCaseAnalysis>();
        private List<BoxCasePalletAnalysis> _boxCasePalletOptimizations = new List<BoxCasePalletAnalysis>();
 
        private List<IDocumentListener> _listeners = new List<IDocumentListener>();
        protected static readonly ILog _log = LogManager.GetLogger(typeof(Document));
        #endregion

        #region Constructor
        public Document(string filePath, IDocumentListener listener)
        {
            // set name from file path
            _name = Path.GetFileNameWithoutExtension(filePath);
            if (null != listener)
            {
                // add listener
                AddListener(listener);
                // notify listener of document creation
                listener.OnNewDocument(this);
            }
            // load file
            Load(filePath);            
            // rechange name to match filePath
            _name = Path.GetFileNameWithoutExtension(filePath);
        }

        public Document(string name, string description, string author, DateTime dateCreated, IDocumentListener listener)
        {
            _name = name;
            _description = description;
            _author = author;
            _dateCreated = dateCreated;
            if (null != listener)
            {
                // add listener
                AddListener(listener);
                // notify listener of document creation
                listener.OnNewDocument(this);
            }
        }
        #endregion

        #region Name checking / Getting new name
        public bool IsValidNewTypeName(string name, ItemBase itemToName)
        {
            // make sure is not empty
            if (name.Trim() == string.Empty)
                return false;
            // make sure it is not already used
            return null == _typeList.Find(
                delegate(ItemBase item)
                {
                    return (item != itemToName) && string.Equals(item.Name.Trim(), name.Trim(), StringComparison.CurrentCultureIgnoreCase);
                }
                );
        }
        public string GetValidNewTypeName(string prefix)
        {
            int index = 0;
            string name = string.Empty;
            while (!IsValidNewTypeName(name = string.Format("{0}{1}", prefix, index), null))
                ++index;
            while (!IsValidNewAnalysisName(name = string.Format("{0}{1}", prefix, index), null))
                ++index;
            return name;
        }
        public bool IsValidNewAnalysisName(string name, ItemBase analysisToRename)
        {
            string trimmedName = name.Trim();
            return (null == _casePalletAnalyses.Find(
                delegate(CasePalletAnalysis analysis)
                {
                    return analysis != analysisToRename
                        && string.Equals(analysis.Name, trimmedName, StringComparison.InvariantCultureIgnoreCase);
                }
                ))
                && (null == _cylinderPalletAnalyses.Find(
                delegate(CylinderPalletAnalysis analysis)
                {
                    return analysis != analysisToRename
                        && string.Equals(analysis.Name, trimmedName, StringComparison.InvariantCultureIgnoreCase);
                }
                ))
                && (null == _hCylinderPalletAnalyses.Find(
                delegate(HCylinderPalletAnalysis analysis)
                {
                    return analysis != analysisToRename
                        && string.Equals(analysis.Name, trimmedName, StringComparison.InvariantCultureIgnoreCase);
                }
                ))
                && (null == _boxCaseAnalyses.Find(
                delegate(BoxCaseAnalysis analysis)
                {
                    return analysis != analysisToRename
                        && string.Equals(analysis.Name, trimmedName, StringComparison.InvariantCultureIgnoreCase);
                }
                ))
                && (null == _boxCasePalletOptimizations.Find(
                delegate(BoxCasePalletAnalysis analysis)
                {
                    return analysis != analysisToRename
                        && string.Equals(analysis.Name, trimmedName, StringComparison.InvariantCultureIgnoreCase);
                }
                ))
                && (null == _packPalletAnalyses.Find(
                delegate(PackPalletAnalysis analysis)
                {
                    return analysis != analysisToRename
                        && string.Equals(analysis.Name, trimmedName, StringComparison.InvariantCultureIgnoreCase);
                }
                ));
        }
        public string GetValidNewAnalysisName(string prefix)
        {
            int index = 0;
            string name = string.Empty;
            while (!IsValidNewAnalysisName(name = string.Format("{0}{1}", prefix, index), null))
                ++index;
            return name;
        }
        #endregion

        #region Public instantiation methods
        /// <summary>
        /// Create a new box
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="length">Length</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="weight">Weight</param>
        /// <param name="colors">Name</param>
        /// <returns>created BoxProperties instance</returns>
        public BoxProperties CreateNewBox(
            string name, string description
            , double length, double width, double height
            , double weight
            , Color[] colors)
        {
            // instantiate and initialize
            BoxProperties boxProperties = new BoxProperties(this, length, width, height);
            boxProperties.Weight = weight;
            boxProperties.Name = name;
            boxProperties.Description = description;
            boxProperties.SetAllColors(colors);
            // insert in list
            _typeList.Add(boxProperties);
            // notify listeners
            NotifyOnNewTypeCreated(boxProperties);
            Modify();
            return boxProperties;
        }
        public BoxProperties CreateNewBox(BoxProperties boxProp)
        { 
            // instantiate and initialize
            BoxProperties boxPropClone = new BoxProperties(this
                , boxProp.Length
                , boxProp.Width
                , boxProp.Height);
            boxPropClone.Weight = boxProp.Weight;
            boxPropClone.NetWeight = boxProp.NetWeight;
            boxPropClone.Name = boxProp.Name;
            boxPropClone.Description = boxProp.Description;
            boxPropClone.SetAllColors(boxProp.Colors);
            // insert in list
            _typeList.Add(boxPropClone);
            // notify listeners
            NotifyOnNewTypeCreated(boxPropClone);
            Modify();

            return boxPropClone;
        }
        /// <summary>
        /// Create a new case
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="length"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="insideLength"></param>
        /// <param name="insideWidth"></param>
        /// <param name="insideHeight"></param>
        /// <param name="weight"></param>
        /// <param name="colors"></param>
        /// <returns></returns>
        public BoxProperties CreateNewCase(
            string name, string description
            , double length, double width, double height
            , double insideLength, double insideWidth, double insideHeight
            , double weight
            , Color[] colors)
        {
            // instantiate and initialize
            BoxProperties boxProperties = new BoxProperties(this, length, width, height, insideLength, insideWidth, insideHeight);
            boxProperties.Weight = weight;
            boxProperties.Name = name;
            boxProperties.Description = description;
            boxProperties.SetAllColors(colors);
            // insert in list
            _typeList.Add(boxProperties);
            // notify listeners
            NotifyOnNewTypeCreated(boxProperties);
            Modify();
            return boxProperties;
        }
        public BoxProperties CreateNewCase(BoxProperties boxProp)
        {
            // instantiate and initialize
            BoxProperties boxPropClone = new BoxProperties(this
                , boxProp.Length
                , boxProp.Width
                , boxProp.Height
                , boxProp.InsideLength
                , boxProp.InsideWidth
                , boxProp.InsideHeight);
            boxPropClone.Weight = boxProp.Weight;
            boxPropClone.NetWeight = boxProp.NetWeight;
            boxPropClone.Name = boxProp.Name;
            boxPropClone.Description = boxProp.Description;
            boxPropClone.SetAllColors(boxProp.Colors);
            boxPropClone.ShowTape = boxProp.ShowTape;
            boxPropClone.TapeWidth = boxProp.TapeWidth;
            boxPropClone.TapeColor = boxProp.TapeColor;
            // insert in list
            _typeList.Add(boxPropClone);
            // notify listeners
            NotifyOnNewTypeCreated(boxPropClone);
            Modify();
            return boxPropClone;
        }
        /// <summary>
        /// Create a new pack
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="box">Inner box</param>
        /// <param name="arrangement">Arrangement</param>
        /// <param name="axis">Axis</param>
        /// <param name="wrapper">Wrapper</param>
        /// <returns></returns>
        public PackProperties CreateNewPack(
            string name, string description
            , BoxProperties box
            , PackArrangement arrangement
            , HalfAxis.HAxis axis
            , PackWrapper wrapper)
        { 
            // instantiate and initialize
            PackProperties packProperties = new PackProperties(this
                , box
                , arrangement
                , axis
                , wrapper);
            packProperties.Name = name;
            packProperties.Description = description;
            // insert in list
            _typeList.Add(packProperties);
            // notify listeners
            NotifyOnNewTypeCreated(packProperties);
            Modify();
            return packProperties;
        }

        public CaseOfBoxesProperties CreateNewCaseOfBoxes(
            string name, string description
            , BoxProperties boxProperties
            , CaseDefinition caseDefinition
            , CaseOptimConstraintSet constraintSet)
        {
            CaseOfBoxesProperties caseProperties = new CaseOfBoxesProperties(this, boxProperties, caseDefinition, constraintSet);
            caseProperties.Name = name;
            caseProperties.Description = description;
            // insert in list
            _typeList.Add(caseProperties);
            // notify listeners
            NotifyOnNewTypeCreated(caseProperties);
            Modify();
            return caseProperties;
        }
        
        public BundleProperties CreateNewBundle(
            string name, string description
            , double length, double width, double thickness
            , double weight
            , Color color
            , int noFlats)
        {
            // instantiate and initialize
            BundleProperties bundle = new BundleProperties(this, name, description, length, width, thickness, weight, noFlats, color);
            // insert in list
            _typeList.Add(bundle);
            // notify listeners
            NotifyOnNewTypeCreated(bundle);
            Modify();
            return bundle;
        }

        public CylinderProperties CreateNewCylinder(CylinderProperties cyl)
        {
            // cylinder
            CylinderProperties cylinder = new CylinderProperties(this
                , cyl.Name, cyl.Description
                , cyl.RadiusOuter, cyl.RadiusInner, cyl.Height
                , cyl.Weight
                , cyl.ColorTop, cyl.ColorWallOuter, cyl.ColorWallInner);
            // insert in list
            _typeList.Add(cylinder);
            // notify listeners
            NotifyOnNewTypeCreated(cylinder);
            Modify();
            return cylinder;
        }

        public CylinderProperties CreateNewCylinder(
            string name, string description
            , double radiusOuter, double radiusInner, double height
            , double weight
            , Color colorTop, Color colorWallOuter, Color colorWallInner)
        {
            CylinderProperties cylinder = new CylinderProperties(this, name, description
                , radiusOuter, radiusInner, height, weight
                , colorTop, colorWallOuter, colorWallInner);
            // insert in list
            _typeList.Add(cylinder);
            // notify listeners
            NotifyOnNewTypeCreated(cylinder);
            Modify();
            return cylinder;        
        }

        public void AddType(ItemBase item)
        {
            // insert in list
            _typeList.Add(item);
            // notify listeners
            NotifyOnNewTypeCreated(item);
            Modify();
        }
        public InterlayerProperties CreateNewInterlayer(
            string name, string description
            , double length, double width, double thickness
            , double weight
            , Color color)
        { 
            // instantiate and intialize
            InterlayerProperties interlayer = new InterlayerProperties(
                this, name, description
                , length, width, thickness
                , weight, color);
            // insert in list
            _typeList.Add(interlayer);
            // notify listeners
            NotifyOnNewTypeCreated(interlayer);
            Modify();
            return interlayer;
        }
        public PalletCornerProperties CreateNewPalletCorners(string name, string description,
            double length, double width, double thickness,
            double weight,
            Color color)
        {
            // instantiate and initialize
            PalletCornerProperties palletCorners = new PalletCornerProperties(
                this,
                name, description,
                length, width, thickness,
                weight,
                color);
            // insert in list
            _typeList.Add(palletCorners);
            // notify listeners
            NotifyOnNewTypeCreated(palletCorners);
            Modify();
            return palletCorners;
        }
        public PalletCapProperties CreateNewPalletCap(PalletCapProperties palletCap)
        { 
            // instantiate and initialize
                PalletCapProperties palletCapClone = new PalletCapProperties(
                    this,
                    palletCap.Name, palletCap.Description,
                    palletCap.Length, palletCap.Width, palletCap.Height,
                    palletCap.InsideLength, palletCap.InsideWidth, palletCap.InsideHeight,
                    palletCap.Weight, palletCap.Color);
                // insert in list
                _typeList.Add(palletCapClone);
                // notify listeners
                NotifyOnNewTypeCreated(palletCapClone);
                Modify();
                return palletCapClone;
        }

        public PalletCapProperties CreateNewPalletCap(
            string name, string description,
            double length, double width, double height,
            double innerLength, double innerWidth, double innerHeight,
            double weight,
            Color color)
        {
            // instantiate and initialize
            PalletCapProperties palletCap = new PalletCapProperties(
                this,
                name, description,
                length, width, height,
                innerLength, innerWidth, innerHeight,
                weight, color);
            // insert in list
            _typeList.Add(palletCap);
            // notify listeners
            NotifyOnNewTypeCreated(palletCap);
            Modify();
            return palletCap;
        }

        public PalletFilmProperties CreateNewPalletFilm(PalletFilmProperties palletFilm)
        {
            // instantiate and initialize
            PalletFilmProperties palletFilmClone = new PalletFilmProperties(
                this,
                palletFilm.Name, palletFilm.Description,
                palletFilm.UseTransparency, palletFilm.UseHatching,
                palletFilm.HatchSpacing, palletFilm.HatchAngle,
                palletFilm.Color);
            // insert in list
            _typeList.Add(palletFilmClone);
            // notify listeners
            NotifyOnNewTypeCreated(palletFilmClone);
            Modify();
            return palletFilmClone; 
        }

        public PalletFilmProperties CreateNewPalletFilm(
            string name, string description,
            bool useTransparency,
            bool useHatching, double hatchSpacing, double hatchAngle,
            Color color)
        {
            // instantiate and initialize
            PalletFilmProperties palletFilm = new PalletFilmProperties(
                this,
                name, description,
                useTransparency,
                useHatching, hatchSpacing, hatchAngle,
                color);
            // insert in list
            _typeList.Add(palletFilm);
            // notify listeners
            NotifyOnNewTypeCreated(palletFilm);
            Modify();
            return palletFilm;
        }
        public InterlayerProperties CreateNewInterlayer(InterlayerProperties interlayerProp)
        {
            // instantiate and intialize
            InterlayerProperties interlayerClone = new InterlayerProperties(
                this, interlayerProp.Name, interlayerProp.Description
                , interlayerProp.Length, interlayerProp.Width, interlayerProp.Thickness
                , interlayerProp.Weight
                , interlayerProp.Color);
            // insert in list
            _typeList.Add(interlayerClone);
            // notify listeners
            NotifyOnNewTypeCreated(interlayerClone);
            Modify();
            return interlayerClone;       
        }
        public PalletProperties CreateNewPallet(
            string name, string description
            , string typeName
            , double length, double width, double height
            , double weight)
        {
            PalletProperties palletProperties = new PalletProperties(this, typeName, length, width, height);
            palletProperties.Name = name;
            palletProperties.Description = description;
            palletProperties.Weight = weight;
            // insert in list
            _typeList.Add(palletProperties);
            // notify listeners
            NotifyOnNewTypeCreated(palletProperties);
            Modify();
            return palletProperties;
        }

        public PalletProperties CreateNewPallet(PalletProperties palletProp)
        {
            PalletProperties palletPropClone = new PalletProperties(this, palletProp.TypeName, palletProp.Length, palletProp.Width, palletProp.Height);
            palletPropClone.Name = palletProp.Name;
            palletPropClone.Description = palletProp.Description;
            palletPropClone.Weight = palletProp.Weight;
            palletPropClone.Color = palletProp.Color;
            palletPropClone.AdmissibleLoadWeight = palletProp.AdmissibleLoadWeight;
            // insert in list
            _typeList.Add(palletPropClone);
            // notify listeners
            NotifyOnNewTypeCreated(palletPropClone);
            Modify();
            return palletPropClone;           
        }

        /// <summary>
        /// Creates a new truck in this document
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="length">Length</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="admissibleLoadWeight">AdmissibleLoadWeight</param>
        /// <param name="color">Color</param>
        /// <returns>TruckProperties</returns>
        public TruckProperties CreateNewTruck(
            string name, string description
            , double length
            , double width
            , double height
            , double admissibleLoadWeight
            , Color color)
        {
            TruckProperties truckProperties = new TruckProperties(this, length, width, height);
            truckProperties.Name = name;
            truckProperties.Description = description;
            truckProperties.AdmissibleLoadWeight = admissibleLoadWeight;
            truckProperties.Color = color;
            // insert in list
            _typeList.Add(truckProperties);
            // notify listeners
            NotifyOnNewTypeCreated(truckProperties);
            Modify();
            return truckProperties;
        }
        /// <summary>
        /// Creates a new analysis in this document + compute solutions
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="box"></param>
        /// <param name="pallet"></param>
        /// <param name="interlayer"></param>
        /// <param name="constraintSet"></param>
        /// <param name="solver">Node : analysis creation requires a solver</param>
        /// <returns>An analysis</returns>
        public CasePalletAnalysis CreateNewCasePalletAnalysis(
            string name, string description
            , BProperties box, PalletProperties pallet
            , InterlayerProperties interlayer, InterlayerProperties interlayerAntiSlip
            , PalletCornerProperties palletCorners, PalletCapProperties palletCap, PalletFilmProperties palletFilm
            , PalletConstraintSet constraintSet
            , ICasePalletAnalysisSolver solver)
        {
            CasePalletAnalysis analysis = new CasePalletAnalysis(
                box, pallet,
                interlayer, interlayerAntiSlip,
                palletCorners, palletCap, palletFilm,
                constraintSet);
            analysis.Name = name;
            analysis.Description = description;
            // insert in list
            _casePalletAnalyses.Add(analysis);
            // compute analysis
            solver.ProcessAnalysis(analysis);
            if (analysis.Solutions.Count < 1)
            {	// remove analysis from list if it has no valid solution
                _casePalletAnalyses.Remove(analysis);
                return null;
            }
            // notify listeners
            NotifyOnNewCasePalletAnalysisCreated(analysis);
            Modify();
            return analysis;
        }

        public AnalysisCasePallet CreateNewAnalysisCasePallet(
            string name, string description
            , BProperties box, PalletProperties pallet
            , List<InterlayerProperties> interlayers
            , PalletCornerProperties palletCorners, PalletCapProperties palletCap, PalletFilmProperties palletFilm
            , ConstraintSetCasePallet constraintSet
            , List<LayerDesc> layerDescs
            )
        {
            AnalysisCasePallet analysis = new AnalysisCasePallet(box, pallet, constraintSet);
            foreach (InterlayerProperties interlayer in interlayers)
                analysis.AddInterlayer(interlayer);
            analysis.PalletCornerProperties     = palletCorners;
            analysis.PalletCapProperties        = palletCap;
            analysis.PalletFilmProperties       = palletFilm;
            analysis.AddSolution(layerDescs);

            // notify listeners
            NotifyOnNewAnalysisCreated(analysis);
            Modify();

            return analysis;
        }


        public PackPalletAnalysis CreateNewPackPalletAnalysis(
            string name, string description
            , PackProperties pack, PalletProperties pallet
            , InterlayerProperties interlayer
            , PackPalletConstraintSet constraintSet
            , IPackPalletAnalysisSolver solver)
        {
            PackPalletAnalysis analysis = new PackPalletAnalysis(
                pack
                , pallet
                , interlayer
                , constraintSet);
            analysis.Name = name;
            analysis.Description = description;
            // insert in list
            _packPalletAnalyses.Add(analysis);
            // compute analysis
            solver.ProcessAnalysis(analysis);
            if (analysis.Solutions.Count < 1)
            {   // remove analysis from list if it has no valid solution
                _packPalletAnalyses.Remove(analysis);
                Modify();
                return null;
            }
            // notify listeners
            NotifyOnNewPackPalletAnalysisCreated(analysis);
            Modify();
            return analysis;
        }

        public PackPalletAnalysis CreateNewPackPalletAnalysis(
            string name, string description
            , PackProperties pack, PalletProperties pallet
            , InterlayerProperties interlayer
            , PackPalletConstraintSet constraintSet
            , List<PackPalletSolution> solutions)
        {
            PackPalletAnalysis analysis = new PackPalletAnalysis(
                pack
                , pallet
                , interlayer
                , constraintSet);
            analysis.Name = name;
            analysis.Description = description;
            // insert in list
            _packPalletAnalyses.Add(analysis);
            // set solutions
            analysis.Solutions = solutions;
            // notify listeners
            NotifyOnNewPackPalletAnalysisCreated(analysis);
            // set solution selected if it is unique
            if (solutions.Count == 1)
                analysis.SelectSolutionByIndex(0);
            return analysis;
        }

        /// <summary>
        /// Creates a new analysis without generating solutions
        /// </summary>
        /// <param name="name">Name of analysis</param>
        /// <param name="description">Description</param>
        /// <param name="box">Case</param>
        /// <param name="pallet">Pallet</param>
        /// <param name="interlayer">Interlayer</param>
        /// <param name="constraintSet">PalletConstraintSet</param>
        /// <param name="solutions">Solutions</param>
        /// <returns>CasePalletAnalysis generated using input parameters</returns>
        public CasePalletAnalysis CreateNewCasePalletAnalysis(
            string name, string description
            , BProperties box, PalletProperties pallet
            , InterlayerProperties interlayer, InterlayerProperties interlayerAntiSlip
            , PalletCornerProperties palletCorners, PalletCapProperties palletCap, PalletFilmProperties palletFilm
            , PalletConstraintSet constraintSet
            , List<CasePalletSolution> solutions)
        {
            CasePalletAnalysis analysis = new CasePalletAnalysis(
                box, pallet,
                interlayer, interlayerAntiSlip,
                palletCorners, palletCap, palletFilm,
                constraintSet);
            analysis.Name = name;
            analysis.Description = description;
            // insert in list
            _casePalletAnalyses.Add(analysis);
            // set solutions
            analysis.Solutions = solutions;
            // notify listeners
            NotifyOnNewCasePalletAnalysisCreated(analysis);
            // set solution selected if it is unique
            if (solutions.Count == 1)
                analysis.SelectSolutionByIndex(0);
            return analysis;
        }
        /// <summary>
        /// Creates a new cylinder analysis in this document + compute solutions
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="cylinder">Cylinder</param>
        /// <param name="pallet">Pallet</param>
        /// <param name="interlayer">Interlayer or null</param>
        /// <param name="constraintSet">Cylinder/pallet analysis constraint set</param>
        /// <param name="solver">Solver</param>
        /// <returns>Cylinder/pallet analysis</returns>
        public CylinderPalletAnalysis CreateNewCylinderPalletAnalysis(
            string name, string description
            , CylinderProperties cylinder, PalletProperties pallet
            , InterlayerProperties interlayer, InterlayerProperties interlayerPropertiesAntiSlip
            , CylinderPalletConstraintSet constraintSet
            , ICylinderAnalysisSolver solver)
        {
            CylinderPalletAnalysis analysis = new CylinderPalletAnalysis(
                cylinder, pallet,
                interlayer, interlayerPropertiesAntiSlip,
                constraintSet);
            analysis.Name = name;
            analysis.Description = description;
            // insert in list
            _cylinderPalletAnalyses.Add(analysis);
            // compute analysis
            solver.ProcessAnalysis(analysis);
            if (analysis.Solutions.Count < 1)
            {	// remove analysis from list if it has no valid solution
                _cylinderPalletAnalyses.Remove(analysis);
                return null;
            }
            // notify listeners
            NotifyOnNewCylinderPalletAnalysisCreated(analysis);
            Modify();
            return analysis;
        }
        public HCylinderPalletAnalysis CreateNewHCylinderPalletAnalysis(
            string name, string description,
            CylinderProperties cylinder, PalletProperties pallet,
            HCylinderPalletConstraintSet constraintSet,
            IHCylinderAnalysisSolver solver)
        {
            HCylinderPalletAnalysis analysis = new HCylinderPalletAnalysis(cylinder, pallet, constraintSet);
            analysis.Name = name;
            analysis.Description = description;
            // insert in list
            _hCylinderPalletAnalyses.Add(analysis);
            // compute analysis
            solver.ProcessAnalysis(analysis);
            if (analysis.Solutions.Count < 1)
            {   // remove analysis from list if it has no valid solution
                _hCylinderPalletAnalyses.Remove(analysis);
                return null;
            }
            // notify listeners
            NotifyOnNewHCylinderPalletAnalysisCreated(analysis);
            Modify();
            return analysis;
        }

        /// <summary>
        /// Creates a new cylinder/pallet analysis without generating solutions
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="cylinder">Cylinder</param>
        /// <param name="pallet">Pallet</param>
        /// <param name="interlayer">Interlayer or null</param>
        /// <param name="constraintSet">Cylinder/pallet analysis constraint set</param>
        /// <param name="solutions">Solutions</param>
        /// <returns>Cylinder/pallet analysis</returns>
        public CylinderPalletAnalysis CreateNewCylinderPalletAnalysis(
            string name, string description
            , CylinderProperties cylinder, PalletProperties pallet
            , InterlayerProperties interlayer, InterlayerProperties interlayerAntiSlip
            , CylinderPalletConstraintSet constraintSet
            , List<CylinderPalletSolution> solutions)
        {
            CylinderPalletAnalysis analysis = new CylinderPalletAnalysis(
                cylinder, pallet,
                interlayer, interlayerAntiSlip,
                constraintSet);
            analysis.Name = name;
            analysis.Description = description;
            // insert in list
            _cylinderPalletAnalyses.Add(analysis);
            // set solutions
            analysis.Solutions = solutions;
            // notify listeners
            NotifyOnNewCylinderPalletAnalysisCreated(analysis);
            // set solution selected if its unique
            if (solutions.Count == 1)
                analysis.SelectSolutionByIndex(0);
            return analysis;
        }

        /// <summary>
        /// Creates a new cylinder/pallet analysis without generating solutions
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="cylinder">Cylinder</param>
        /// <param name="pallet">Pallet</param>
        /// <param name="interlayer">Interlayer or null</param>
        /// <param name="constraintSet">Cylinder/pallet analysis constraint set</param>
        /// <param name="solutions">Solutions</param>
        /// <returns>Cylinder/pallet analysis</returns>
        public HCylinderPalletAnalysis CreateNewHCylinderPalletAnalysis(
            string name, string description
            , CylinderProperties cylinder, PalletProperties pallet
            , HCylinderPalletConstraintSet constraintSet
            , List<HCylinderPalletSolution> solutions)
        {
            HCylinderPalletAnalysis analysis = new HCylinderPalletAnalysis(cylinder, pallet, constraintSet);
            analysis.Name = name;
            analysis.Description = description;
            // insert in list
            _hCylinderPalletAnalyses.Add(analysis);
            // set solutions
            analysis.Solutions = solutions;
            // notify listeners
            NotifyOnNewHCylinderPalletAnalysisCreated(analysis);
            // set solution selected if its unique
            if (solutions.Count == 1)
                analysis.SelectSolutionByIndex(0);
            return analysis;
        }

        public BoxCaseAnalysis CreateNewBoxCaseAnalysis(
            string name, string description
            , BProperties boxProperties, BoxProperties caseProperties
            , BCaseConstraintSet constraintSet
            , List<BoxCaseSolution> solutions)
        {
            BoxCaseAnalysis analysis = new BoxCaseAnalysis(boxProperties, caseProperties, constraintSet);
            analysis.Name = name;
            analysis.Description = description;
            // insert in list
            _boxCaseAnalyses.Add(analysis);
            // set solutions
            analysis.Solutions = solutions;
            // notify listeners
            NotifyOnNewBoxCaseAnalysis(analysis);
            // set solution selected if it is unique
            if (solutions.Count == 1)
                analysis.SelectSolutionByIndex(0);
            return analysis;
        }

        public BoxCaseAnalysis CreateNewBoxCaseAnalysis(
            string name, string description
            , BProperties boxProperties, BoxProperties caseProperties
            , BCaseConstraintSet constraintSet
            , IBoxCaseAnalysisSolver solver)
        {
            BoxCaseAnalysis analysis = new BoxCaseAnalysis(boxProperties, caseProperties, constraintSet);
            analysis.Name = name;
            analysis.Description = description;
            // insert in list
            _boxCaseAnalyses.Add(analysis);
            // compute analysis
            if (null != solver)
            {
                solver.ProcessAnalysis(analysis);
                if (analysis.Solutions.Count < 1)
                {	// remove analysis from list if it has no valid solution
                    _boxCaseAnalyses.Remove(analysis);
                    return null;
                }
            }
            // notify listeners
            NotifyOnNewBoxCaseAnalysis(analysis);
            Modify();
            return analysis;
        }

        public BoxCasePalletAnalysis CreateNewBoxCasePalletOptimization(
            string name, string description
            , BoxProperties bProperties
            , BoxCasePalletConstraintSet constraintSet
            , List<PalletSolutionDesc> palletSolutionList
            , IBoxCasePalletAnalysisSolver solver)
        {
            BoxCasePalletAnalysis analysis = new BoxCasePalletAnalysis(bProperties, palletSolutionList, constraintSet);
            analysis.Name = name;
            analysis.Description = description;
            // insert in list
            _boxCasePalletOptimizations.Add(analysis);
            // compute analysis
            if (null != solver)
            {
                solver.ProcessAnalysis(analysis);
                if (analysis.Solutions.Count < 1)
                {	// remove analysis from list if it has no valid solution
                    _boxCasePalletOptimizations.Remove(analysis);
                    _log.InfoFormat("Failed to find any solution {0}", analysis.Name);
                    return null;
                }
            }
            // notify listeners
            NotifyOnNewCaseAnalysisCreated(analysis);
            Modify();
            return analysis;
        }
        
        public void RemoveItem(ItemBase item)
        {
            // sanity check
            if (null == item)
            {
                Debug.Assert(false);
                return;
            }
            // dispose item first as it may remove dependancies itself
            _log.Debug(string.Format("Disposing {0}...", item.Name));
            item.Dispose();

            // notify listeners / remove
            if (item.GetType() == typeof(BoxProperties)
                || item.GetType() == typeof(BundleProperties)
                || item.GetType() == typeof(CaseOfBoxesProperties)
                || item.GetType() == typeof(PackProperties)
                || item.GetType() == typeof(PalletProperties)
                || item.GetType() == typeof(InterlayerProperties)
                || item.GetType() == typeof(PalletCornerProperties)
                || item.GetType() == typeof(PalletCapProperties)
                || item.GetType() == typeof(PalletFilmProperties)
                || item.GetType() == typeof(TruckProperties)
                || item.GetType() == typeof(CylinderProperties))
            {
                NotifyOnTypeRemoved(item);
                if (!_typeList.Remove(item))
                    _log.Warn(string.Format("Failed to properly remove item {0}", item.Name));
            }
            else if (item.GetType() == typeof(CasePalletAnalysis))
            {
                NotifyOnAnalysisRemoved(item as CasePalletAnalysis);
                if (!_casePalletAnalyses.Remove(item as CasePalletAnalysis))
                    _log.Warn(string.Format("Failed to properly remove analysis {0}", item.Name));
            }
            else if (item.GetType() == typeof(PackPalletAnalysis))
            { 
                NotifyOnAnalysisRemoved(item as PackPalletAnalysis);
                if (!_packPalletAnalyses.Remove(item as PackPalletAnalysis))
                    _log.Warn(string.Format("Failed to properly remove analysis {0}", item.Name));
            }
            else if (item.GetType() == typeof(BoxCaseAnalysis))
            {
                NotifyOnAnalysisRemoved(item as BoxCaseAnalysis);
                if (!_boxCaseAnalyses.Remove(item as BoxCaseAnalysis))
                    _log.Warn(string.Format("Failed to properly remove analysis {0}", item.Name));
            }
            else if (item.GetType() == typeof(CylinderPalletAnalysis))
            {
                NotifyOnAnalysisRemoved(item as CylinderPalletAnalysis);
                if (!_cylinderPalletAnalyses.Remove(item as CylinderPalletAnalysis))
                    _log.Warn(string.Format("Failed to properly remove analysis {0}", item.Name));
            }
            else if (item.GetType() == typeof(HCylinderPalletAnalysis))
            {
                NotifyOnAnalysisRemoved(item as HCylinderPalletAnalysis);
                if (!_hCylinderPalletAnalyses.Remove(item as HCylinderPalletAnalysis))
                    _log.Warn(string.Format("Failed to properly remove analysis {0}", item.Name));
            }
            else if (item.GetType() == typeof(TruckAnalysis))
            {
                TruckAnalysis truckAnalysis = item as TruckAnalysis;
                NotifyOnTruckAnalysisRemoved(truckAnalysis.ParentSelSolution, truckAnalysis);
            }
            else if (item.GetType() == typeof(BoxCasePalletAnalysis))
            {
                BoxCasePalletAnalysis caseAnalysis = item as BoxCasePalletAnalysis;
                NotifyOnAnalysisRemoved(caseAnalysis);
                if (!_boxCasePalletOptimizations.Remove(caseAnalysis))
                    _log.Warn(string.Format("Failed to properly remove analysis {0}", item.Name));
            }
            else if (item.GetType() == typeof(ECTAnalysis))
            {
                ECTAnalysis ectAnalysis = item as ECTAnalysis;
                NotifyOnECTAnalysisRemoved(ectAnalysis.ParentSelSolution, ectAnalysis);
            }
            else if (item.GetType() == typeof(SelCasePalletSolution)) { }
            else if (item.GetType() == typeof(SelBoxCasePalletSolution)) { }
            else if (item.GetType() == typeof(SelBoxCaseSolution)) { }
            else if (item.GetType() == typeof(SelCylinderPalletSolution)) { }
            else if (item.GetType() == typeof(SelHCylinderPalletSolution)) { }
            else if (item.GetType() == typeof(SelPackPalletSolution)) { }
            else
                Debug.Assert(false);
            Modify();
        }
        #endregion

        #region Public properties
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }
        public string Author
        {
            get { return _author; }
            set { _author = value; }
        }
        public DateTime DateOfCreation
        {
            get { return _dateCreated; }
            set { _dateCreated = value; }
        }
        /// <summary>
        /// Builds and returns a list of boxes
        /// </summary>
        public List<BoxProperties> Boxes
        {
            get
            {
                List<BoxProperties> boxList = new List<BoxProperties>();
                foreach (ItemBase item in _typeList)
                {
                    BoxProperties boxProperties = item as BoxProperties;
                    if (null != boxProperties && !boxProperties.HasInsideDimensions)
                        boxList.Add(boxProperties);                        
                }
                return boxList;
            }
        }
        /// <summary>
        /// Builds and returns a list of cases
        /// </summary>
        public List<BoxProperties> Cases
        {
            get
            {
                List<BoxProperties> caseList = new List<BoxProperties>();
                foreach (ItemBase item in _typeList)
                {
                    BoxProperties boxProperties = item as BoxProperties;
                    if (null != boxProperties && boxProperties.HasInsideDimensions)
                        caseList.Add(boxProperties);
                }
                return caseList;
            }
        }
        /// <summary>
        /// Builds and returns a list of bundles
        /// </summary>
        public List<BundleProperties> Bundles
        {
            get
            {
                List<BundleProperties> bundleList = new List<BundleProperties>();
                foreach (ItemBase item in _typeList)
                {
                    BundleProperties bundleProperties = item as BundleProperties;
                    if (null != bundleProperties)
                        bundleList.Add(bundleProperties); 
                }
                return bundleList;
            }
        }
        /// <summary>
        /// Builds and return a list of cylinders
        /// </summary>
        public List<CylinderProperties> Cylinders
        {
            get
            {
                List<CylinderProperties> cylinderList = new List<CylinderProperties>();
                foreach (ItemBase item in _typeList)
                {
                    CylinderProperties cylinderProperties = item as CylinderProperties;
                    if (null != cylinderProperties)
                        cylinderList.Add(cylinderProperties);
                }
                return cylinderList;
            }
        }
        /// <summary>
        /// Builds and returns a list of pallets
        /// </summary>
        public List<PalletProperties> Pallets
        {
            get
            {
                List<PalletProperties> palletList = new List<PalletProperties>();
                foreach (ItemBase item in _typeList)
                {
                    PalletProperties palletProperties = item as PalletProperties;
                    if (null != palletProperties)
                        palletList.Add(palletProperties);
                }
                return palletList;
            }
        }
        /// <summary>
        /// Builds and returns a list of interlayers
        /// </summary>
        public List<InterlayerProperties> Interlayers
        {
            get
            {
                List<InterlayerProperties> interlayerList = new List<InterlayerProperties>();
                foreach (ItemBase item in _typeList)
                {
                    InterlayerProperties interlayerProperties = item as InterlayerProperties;
                    if (null != interlayerProperties)
                        interlayerList.Add(interlayerProperties);
                }
                return interlayerList;
            }
        }
        public List<ItemBase> ListByType(Type t)
        {
            List<ItemBase> itemList = new List<ItemBase>();
            foreach (ItemBase item in _typeList)
            {
                if (item.GetType() == t)
                    itemList.Add(item);
            }
            return itemList;
        }
        /// <summary>
        /// Build and returns a list of trucks
        /// </summary>
        public List<TruckProperties> Trucks
        {
            get
            {
                List<TruckProperties> truckPropertiesList = new List<TruckProperties>();
                foreach (ItemBase item in _typeList)
                {
                    TruckProperties truckProperties = item as TruckProperties;
                    if (null != truckProperties)
                        truckPropertiesList.Add(truckProperties);
                }
                return truckPropertiesList;
            }
        }
        /// <summary>
        /// Get list of analyses
        /// </summary>
        public List<CasePalletAnalysis> Analyses
        { get { return _casePalletAnalyses; } }
        /// <summary>
        /// Returns true if pack can be created i.e. if documents contains at at least a box
        /// </summary>
        public bool CanCreatePack
        { get { return this.Boxes.Count > 0; } }
        /// <summary>
        /// Returns true if pallet analysis can be created i.e. if documents contains at least a case and a pallet
        /// </summary>
        public bool CanCreateCasePalletAnalysis
        { get { return this.Cases.Count > 0 && this.Pallets.Count > 0; } }
        /// <summary>
        /// Returns true if a pack analysis can be created i.e. if documents contains at least a pack and a pallet
        /// </summary>
        public bool CanCreatePackPalletAnalysis
        { get { return this.ListByType(typeof(PackProperties)).Count > 0 && this.Pallets.Count > 0; } }
        /// <summary>
        /// Returns true if a bundle analysis can be created i.e. if documents contains at least a bundle and a case
        /// </summary>
        public bool CanCreateBundlePalletAnalysis
        { get { return this.Bundles.Count > 0 && this.Pallets.Count > 0; } }
        /// <summary>
        /// Returns true if a box case analysis can be created i.e. if document contains at least one box and one case
        /// </summary>
        public bool CanCreateBoxCaseAnalysis
        { get { return (this.Boxes.Count > 0 && this.Cases.Count > 0) || (this.Cases.Count > 1); } }
        /// <summary>
        /// Returns true if a bundle/case analysis can be created i.e. if document contains at least one bundle and one case
        /// </summary>
        public bool CanCreateBundleCaseAnalysis
        { get { return this.Cases.Count > 0 && this.Bundles.Count > 0; } }
        /// <summary>
        /// Returns true if a case analysis can be created i.e. if documents contains at least a box and pallet solutions database is not empty
        /// </summary>
        public bool CanCreateBoxCasePalletAnalysis
        { get { return this.Boxes.Count > 0 && !PalletSolutionDatabase.Instance.IsEmpty; } }
        /// <summary>
        /// Returns true if user can proceed to case optimization i.e. if documents contains at least one box and one pallet 
        /// </summary>
        public bool CanCreateCaseOptimization
        { get { return this.Boxes.Count > 0 && this.Pallets.Count > 0; } }
        /// <summary>
        /// Returns true if a cylinder/pallet analysis can be created i.e. if document contains at least one cylinder and one pallet
        /// </summary>
        public bool CanCreateCylinderPalletAnalysis
        { get { return this.Cylinders.Count > 0 && this.Pallets.Count > 0; } }
        #endregion

        #region Load methods
        public void Load(string filePath)
        {
            try
            {
                // instantiate XmlDocument
                XmlDocument xmlDoc = new XmlDocument();
                // load xml file in document and parse document
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
                {
                    xmlDoc.Load(fileStream);
                    XmlElement xmlRootElement = xmlDoc.DocumentElement;
                    LoadDocumentElement(xmlRootElement);
                }
            }
            catch (FileNotFoundException ex)
            {
                _log.Error("Caught FileNotFoundException in Document.Load() -> rethrowing...");
                throw ex;
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
            }
        }

        void LoadDocumentElement(XmlElement docElement)
        {
            if (docElement.HasAttribute("Name"))
                _name = docElement.Attributes["Name"].Value;
            if (docElement.HasAttribute("Description"))
                _description = docElement.Attributes["Description"].Value;
            if (docElement.HasAttribute("Description"))
                _author = docElement.Attributes["Author"].Value;
            if (docElement.HasAttribute("DateCreated"))
            {
                try
                {
                    _dateCreated = System.Convert.ToDateTime(docElement.Attributes["DateCreated"].Value, new CultureInfo("en-US"));
                }
                catch (Exception /*ex*/)
                {
                    _dateCreated = DateTime.Now;
                    _log.Debug("Failed to load date of creation correctly: Loading file generated with former version?");
                }
            }
            if (docElement.HasAttribute("UnitSystem"))
                _unitSystem = (UnitsManager.UnitSystem)int.Parse(docElement.Attributes["UnitSystem"].Value);
            else
                _unitSystem = UnitsManager.UnitSystem.UNIT_METRIC1;

            foreach (XmlNode docChildNode in docElement.ChildNodes)
            {
                // load item properties
                if (string.Equals(docChildNode.Name, "ItemProperties", StringComparison.CurrentCultureIgnoreCase))
                {
                    foreach (XmlNode itemPropertiesNode in docChildNode.ChildNodes)
                    {
                        try
                        {
                            if (string.Equals(itemPropertiesNode.Name, "BoxProperties", StringComparison.CurrentCultureIgnoreCase))
                                LoadBoxProperties(itemPropertiesNode as XmlElement);
                            else if (string.Equals(itemPropertiesNode.Name, "CylinderProperties", StringComparison.CurrentCultureIgnoreCase))
                                LoadCylinderProperties(itemPropertiesNode as XmlElement);
                            else if (string.Equals(itemPropertiesNode.Name, "CaseOfBoxesProperties", StringComparison.CurrentCultureIgnoreCase))
                                LoadCaseOfBoxesProperties(itemPropertiesNode as XmlElement);
                            else if (string.Equals(itemPropertiesNode.Name, "PalletProperties", StringComparison.CurrentCultureIgnoreCase))
                                LoadPalletProperties(itemPropertiesNode as XmlElement);
                            else if (string.Equals(itemPropertiesNode.Name, "InterlayerProperties", StringComparison.CurrentCultureIgnoreCase))
                                LoadInterlayerProperties(itemPropertiesNode as XmlElement);
                            else if (string.Equals(itemPropertiesNode.Name, "PalletCornerProperties", StringComparison.CurrentCultureIgnoreCase))
                                LoadPalletCornerProperties(itemPropertiesNode as XmlElement);
                            else if (string.Equals(itemPropertiesNode.Name, "PalletCapProperties", StringComparison.CurrentCultureIgnoreCase))
                                LoadPalletCapProperties(itemPropertiesNode as XmlElement);
                            else if (string.Equals(itemPropertiesNode.Name, "PalletFilmProperties", StringComparison.CurrentCultureIgnoreCase))
                                LoadPalletFilmProperties(itemPropertiesNode as XmlElement);
                            else if (string.Equals(itemPropertiesNode.Name, "BundleProperties", StringComparison.CurrentCultureIgnoreCase))
                                LoadBundleProperties(itemPropertiesNode as XmlElement);
                            else if (string.Equals(itemPropertiesNode.Name, "TruckProperties", StringComparison.CurrentCultureIgnoreCase))
                                LoadTruckProperties(itemPropertiesNode as XmlElement);
                            else if (string.Equals(itemPropertiesNode.Name, "PackProperties", StringComparison.CurrentCultureIgnoreCase))
                                LoadPackProperties(itemPropertiesNode as XmlElement);
                        }
                        catch (Exception ex)
                        {
                            _log.Error(ex.ToString());
                        }
                    }
                }

                // load analyses
                if (string.Equals(docChildNode.Name, "Analyses", StringComparison.CurrentCultureIgnoreCase))
                {
                    foreach (XmlNode analysisNode in docChildNode.ChildNodes)
                    {
                        try
                        {
                            LoadAnalysis(analysisNode as XmlElement);
                        }
                        catch (Exception ex)
                        {
                            _log.Error(ex.ToString());
                        }
                    }
                }
            }
        }

        #region Load containers / basics element
        private void LoadBoxProperties(XmlElement eltBoxProperties)
        {
            string sid = eltBoxProperties.Attributes["Id"].Value;
            string sname = eltBoxProperties.Attributes["Name"].Value;
            string sdescription = eltBoxProperties.Attributes["Description"].Value;
            string slength = eltBoxProperties.Attributes["Length"].Value;
            string swidth = eltBoxProperties.Attributes["Width"].Value;
            string sheight = eltBoxProperties.Attributes["Height"].Value;
            string sInsideLength = string.Empty, sInsideWidth = string.Empty, sInsideHeight = string.Empty;
            if (eltBoxProperties.HasAttribute("InsideLength"))
            {
                sInsideLength = eltBoxProperties.Attributes["InsideLength"].Value;
                sInsideWidth = eltBoxProperties.Attributes["InsideWidth"].Value;
                sInsideHeight = eltBoxProperties.Attributes["InsideHeight"].Value;
            }
            string sweight = eltBoxProperties.Attributes["Weight"].Value;
            OptDouble optNetWeight = LoadOptDouble(eltBoxProperties, "NetWeight", UnitsManager.UnitType.UT_MASS);

            bool hasInsideDimensions = eltBoxProperties.HasAttribute("InsideLength");
            if (hasInsideDimensions)
            { }

            Color[] colors = new Color[6];
            List<Pair<HalfAxis.HAxis, Texture>> listTexture = new List<Pair<HalfAxis.HAxis,Texture>>();
            bool hasTape = false;
            double tapeWidth = 0.0;
            Color tapeColor = Color.Black;
            foreach (XmlNode node in eltBoxProperties.ChildNodes)
            {
                if (string.Equals(node.Name, "FaceColors", StringComparison.CurrentCultureIgnoreCase))
                {
                    XmlElement faceColorList = node as XmlElement;
                    LoadFaceColors(faceColorList, ref colors);
                }
                else if (string.Equals(node.Name, "Textures", StringComparison.CurrentCultureIgnoreCase))
                {
                    XmlElement textureElt = node as XmlElement;
                    LoadTextureList(textureElt, ref listTexture);
                }
                else if (string.Equals(node.Name, "Tape", StringComparison.CurrentCultureIgnoreCase))
                {
                    XmlElement tapeElt = node as XmlElement;
                    hasTape = LoadTape(tapeElt, out tapeWidth,  out tapeColor);
                }
            }
            // create new BoxProperties instance
            BoxProperties boxProperties = null;
            if (!string.IsNullOrEmpty(sInsideLength)) // case
                boxProperties = CreateNewCase(
                sname
                , sdescription
                , UnitsManager.ConvertLengthFrom(System.Convert.ToDouble(slength, System.Globalization.CultureInfo.InvariantCulture), _unitSystem)
                , UnitsManager.ConvertLengthFrom(System.Convert.ToDouble(swidth, System.Globalization.CultureInfo.InvariantCulture), _unitSystem)
                , UnitsManager.ConvertLengthFrom(System.Convert.ToDouble(sheight, System.Globalization.CultureInfo.InvariantCulture), _unitSystem)
                , UnitsManager.ConvertLengthFrom(System.Convert.ToDouble(sInsideLength, System.Globalization.CultureInfo.InvariantCulture), _unitSystem)
                , UnitsManager.ConvertLengthFrom(System.Convert.ToDouble(sInsideWidth, System.Globalization.CultureInfo.InvariantCulture), _unitSystem)
                , UnitsManager.ConvertLengthFrom(System.Convert.ToDouble(sInsideHeight, System.Globalization.CultureInfo.InvariantCulture), _unitSystem)
                , UnitsManager.ConvertMassFrom(System.Convert.ToDouble(sweight, System.Globalization.CultureInfo.InvariantCulture), _unitSystem)
                , colors);
            else
                boxProperties = CreateNewBox(
                sname
                , sdescription
                , UnitsManager.ConvertLengthFrom(System.Convert.ToDouble(slength, System.Globalization.CultureInfo.InvariantCulture), _unitSystem)
                , UnitsManager.ConvertLengthFrom(System.Convert.ToDouble(swidth, System.Globalization.CultureInfo.InvariantCulture), _unitSystem)
                , UnitsManager.ConvertLengthFrom(System.Convert.ToDouble(sheight, System.Globalization.CultureInfo.InvariantCulture), _unitSystem)
                , UnitsManager.ConvertMassFrom(System.Convert.ToDouble(sweight, System.Globalization.CultureInfo.InvariantCulture), _unitSystem)
                , colors);
            boxProperties.Guid = new Guid(sid);
            boxProperties.TextureList = listTexture;
            // tape
            boxProperties.ShowTape = hasTape;
            boxProperties.TapeColor = tapeColor;
            boxProperties.TapeWidth = UnitsManager.ConvertLengthFrom(tapeWidth, _unitSystem);
            boxProperties.NetWeight = optNetWeight;
        }

        private void LoadPackProperties(XmlElement eltPackProperties)
        {
            string sid = eltPackProperties.Attributes["Id"].Value;
            string sname = eltPackProperties.Attributes["Name"].Value;
            string sdescription = eltPackProperties.Attributes["Description"].Value;
            string sBoxId = eltPackProperties.Attributes["BoxProperties"].Value;
            string sOrientation = eltPackProperties.Attributes["Orientation"].Value;
            string sArrangement = eltPackProperties.Attributes["Arrangement"].Value;
            PackWrapper wrapper = null;
            foreach (XmlElement wrapperNode in eltPackProperties.ChildNodes)
                 wrapper = LoadWrapper(wrapperNode as XmlElement);
            PackProperties packProperties = CreateNewPack(
                sname
                , sdescription
                , GetTypeByGuid(new Guid(sBoxId)) as BoxProperties
                , PackArrangement.TryParse(sArrangement)
                , HalfAxis.Parse(sOrientation)
                , wrapper);
            packProperties.Guid = new Guid(sid);
            if (eltPackProperties.HasAttribute("OuterDimensions"))
            {
                Vector3D outerDimensions = Vector3D.Parse(eltPackProperties.Attributes["OuterDimensions"].Value);
                packProperties.ForceOuterDimensions(outerDimensions);
            }
        }

        private PackWrapper LoadWrapper(XmlElement xmlWrapperElt)
        {
            if (null == xmlWrapperElt) return null;

            string sType = xmlWrapperElt.Attributes["Type"].Value;
            string sColor = xmlWrapperElt.Attributes["Color"].Value;
            string sWeight = xmlWrapperElt.Attributes["Weight"].Value;
            string sUnitThickness = xmlWrapperElt.Attributes["UnitThickness"].Value;

            double thickness = UnitsManager.ConvertLengthFrom(System.Convert.ToDouble(sUnitThickness, System.Globalization.CultureInfo.InvariantCulture), _unitSystem);
            Color wrapperColor = Color.FromArgb(System.Convert.ToInt32(sColor));
            double weight = UnitsManager.ConvertMassFrom(System.Convert.ToDouble(sWeight, System.Globalization.CultureInfo.InvariantCulture), _unitSystem);

            if (sType == "WT_POLYETHILENE")
            {
                bool transparent = bool.Parse(xmlWrapperElt.Attributes["Transparent"].Value);
                return new WrapperPolyethilene(thickness, weight, wrapperColor, transparent);
            }
            else if (sType == "WT_PAPER")
            {
                return new WrapperPaper(thickness, weight, wrapperColor);
            }
            else if (sType == "WT_CARDBOARD")
            {
                string sWalls = "1 1 1";
                if (xmlWrapperElt.HasAttribute("NumberOfWalls"))
                    sWalls = xmlWrapperElt.Attributes["NumberOfWalls"].Value;
                int[] walls = sWalls.Split(' ').Select(n => Convert.ToInt32(n)).ToArray();
                WrapperCardboard wrapper = new WrapperCardboard(thickness, weight, wrapperColor);
                wrapper.SetNoWalls(walls[0], walls[1], walls[2]);
                return wrapper;
            }
            else if (sType == "WT_TRAY")
            {
                string sWalls = "1 1 1";
                if (xmlWrapperElt.HasAttribute("NumberOfWalls"))
                    sWalls = xmlWrapperElt.Attributes["NumberOfWalls"].Value;
                int[] walls = sWalls.Split(' ').Select(n => Convert.ToInt32(n)).ToArray();

                string sHeight = xmlWrapperElt.Attributes["Height"].Value;
                double height = UnitsManager.ConvertLengthFrom(System.Convert.ToDouble(sHeight, System.Globalization.CultureInfo.InvariantCulture), _unitSystem);
                WrapperTray wrapper = new WrapperTray(thickness, weight, wrapperColor);
                wrapper.SetNoWalls(walls[0], walls[1], walls[2]);
                wrapper.Height = height;
                return wrapper;
            }
            else
                return null;
        }


        private void LoadCylinderProperties(XmlElement eltCylinderProperties)
        {
            string sid = eltCylinderProperties.Attributes["Id"].Value;
            string sname = eltCylinderProperties.Attributes["Name"].Value;
            string sdescription = eltCylinderProperties.Attributes["Description"].Value;
            string sRadiusOuter = string.Empty, sRadiusInner = string.Empty;
            if (eltCylinderProperties.HasAttribute("RadiusOuter"))
            {
                sRadiusOuter = eltCylinderProperties.Attributes["RadiusOuter"].Value;
                sRadiusInner = eltCylinderProperties.Attributes["RadiusInner"].Value;
            }
            else
            {
                sRadiusOuter = eltCylinderProperties.Attributes["Radius"].Value;
                sRadiusInner = "0.0";
            }
            string sheight = eltCylinderProperties.Attributes["Height"].Value;
            string sweight = eltCylinderProperties.Attributes["Weight"].Value;
            string sColorTop = eltCylinderProperties.Attributes["ColorTop"].Value;
            string sColorWallOuter = string.Empty, sColorWallInner = string.Empty;
            if (eltCylinderProperties.HasAttribute("ColorWall"))
            {
                sColorWallOuter = eltCylinderProperties.Attributes["ColorWall"].Value;
                sColorWallInner = eltCylinderProperties.Attributes["ColorWall"].Value;
            }
            else
            { 
                sColorWallOuter = eltCylinderProperties.Attributes["ColorWallOuter"].Value;
                sColorWallInner = eltCylinderProperties.Attributes["ColorWallInner"].Value;
            }

            CylinderProperties cylinderProperties = CreateNewCylinder(
                sname,
                sdescription,
                UnitsManager.ConvertLengthFrom(Convert.ToDouble(sRadiusOuter, System.Globalization.CultureInfo.InvariantCulture), _unitSystem),
                UnitsManager.ConvertLengthFrom(Convert.ToDouble(sRadiusInner, System.Globalization.CultureInfo.InvariantCulture), _unitSystem),
                UnitsManager.ConvertLengthFrom(Convert.ToDouble(sheight, System.Globalization.CultureInfo.InvariantCulture), _unitSystem),
                UnitsManager.ConvertMassFrom(Convert.ToDouble(sweight, System.Globalization.CultureInfo.InvariantCulture), _unitSystem),
                Color.FromArgb(System.Convert.ToInt32(sColorTop)),
                Color.FromArgb(System.Convert.ToInt32(sColorWallOuter)),
                Color.FromArgb(System.Convert.ToInt32(sColorWallInner))
                );
            cylinderProperties.Guid = new Guid(sid);
        }

        private void LoadCaseOfBoxesProperties(XmlElement eltCaseOfBoxesProperties)
        {
            string sid = eltCaseOfBoxesProperties.Attributes["Id"].Value;
            string sname = eltCaseOfBoxesProperties.Attributes["Name"].Value;
            string sdescription = eltCaseOfBoxesProperties.Attributes["Description"].Value;
            string sweight = eltCaseOfBoxesProperties.Attributes["Weight"].Value;
            string sBoxId = eltCaseOfBoxesProperties.Attributes["InsideBoxId"].Value;

            CaseDefinition caseDefinition = null;
            CaseOptimConstraintSet constraintSet = null;
            Color[] colors = new Color[6];
            List<Pair<HalfAxis.HAxis, Texture>> listTexture = new List<Pair<HalfAxis.HAxis,Texture>>();
            foreach (XmlNode node in eltCaseOfBoxesProperties.ChildNodes)
            {
                if (string.Equals(node.Name, "FaceColors", StringComparison.CurrentCultureIgnoreCase))
                {
                    XmlElement faceColorList = node as XmlElement;
                    LoadFaceColors(faceColorList, ref colors);
                }
                else if (string.Equals(node.Name, "Textures", StringComparison.CurrentCultureIgnoreCase))
                {
                    XmlElement textureElt = node as XmlElement;
                    LoadTextureList(textureElt, ref listTexture);
                }
                else if (string.Equals(node.Name, "CaseDefinition", StringComparison.CurrentCultureIgnoreCase))
                {
                    XmlElement caseDefinitionElt = node as XmlElement;
                    LoadCaseDefinition(caseDefinitionElt, out caseDefinition);
                }
                else if (string.Equals(node.Name, "OptimConstraintSet", StringComparison.CurrentCultureIgnoreCase))
                {
                    XmlElement constraintSetElt = node as XmlElement;
                    LoadOptimConstraintSet(constraintSetElt, out constraintSet);
                }
            }
            CaseOfBoxesProperties caseOfBoxProperties = CreateNewCaseOfBoxes(
                sname
                , sdescription
                , GetTypeByGuid(new Guid(sBoxId)) as BoxProperties
                , caseDefinition
                , constraintSet);
            caseOfBoxProperties.Weight = UnitsManager.ConvertMassFrom(Convert.ToDouble(sweight, System.Globalization.CultureInfo.InvariantCulture), _unitSystem);
            caseOfBoxProperties.Guid = new Guid(sid);
            caseOfBoxProperties.TextureList = listTexture;
            caseOfBoxProperties.SetAllColors( colors );
        }
        private void LoadFaceColors(XmlElement eltColors, ref Color[] colors)
        {
            foreach (XmlNode faceColorNode in eltColors.ChildNodes)
            {
                XmlElement faceColorElt = faceColorNode as XmlElement;
                string sFaceIndex = faceColorElt.Attributes["FaceIndex"].Value;
                string sColorArgb = faceColorElt.Attributes["Color"].Value;
                int iFaceIndex = System.Convert.ToInt32(sFaceIndex);
                Color faceColor = Color.FromArgb(System.Convert.ToInt32(sColorArgb));
                colors[iFaceIndex] = faceColor;
            }
        }
        private void LoadTextureList(XmlElement eltTextureList, ref List<Pair<HalfAxis.HAxis, Texture>> listTexture)
        {
            foreach (XmlNode faceTextureNode in eltTextureList.ChildNodes)
            {
                try
                {
                    XmlElement xmlFaceTexture = faceTextureNode as XmlElement;
                    // face normal
                    HalfAxis.HAxis faceNormal = HalfAxis.Parse(xmlFaceTexture.Attributes["FaceNormal"].Value);
                    // position
                    Vector2D position = Vector2D.Parse(xmlFaceTexture.Attributes["Position"].Value);
                    // size
                    Vector2D size = Vector2D.Parse(xmlFaceTexture.Attributes["Size"].Value);
                    // angle
                    double angle = Convert.ToDouble(xmlFaceTexture.Attributes["Angle"].Value);
                    // bitmap
                    Bitmap bmp = Document.StringToBitmap(xmlFaceTexture.Attributes["Bitmap"].Value);
                    // add texture pair
                    listTexture.Add(new Pair<HalfAxis.HAxis, Texture>(faceNormal
                        , new Texture(
                            bmp
                            , UnitsManager.ConvertLengthFrom(position, _unitSystem)
                            , UnitsManager.ConvertLengthFrom(size, _unitSystem)
                            , angle)));
                }
                catch (Exception ex)
                {
                    _log.Error(ex.ToString());
                }
            }
        }
        private bool LoadTape(XmlElement eltTape, out double tapeWidth, out Color tapeColor)
        {
            tapeWidth = 0.0;
            tapeColor = Color.Black;
            try
            {
                tapeWidth = Convert.ToDouble(eltTape.Attributes["TapeWidth"].Value);
                string sColorArgb = eltTape.Attributes["TapeColor"].Value;
                tapeColor = Color.FromArgb(System.Convert.ToInt32(sColorArgb));
            }
            catch (Exception /*ex*/)
            {
                return false;
            }
            return true;
        }
        private void LoadCaseDefinition(XmlElement eltCaseDefinition, out CaseDefinition caseDefinition)
        {
            string sArrangement = eltCaseDefinition.Attributes["Arrangement"].Value;
            string sDim = eltCaseDefinition.Attributes["Orientation"].Value;
            int[] iOrientation = Document.ParseInt2(sDim);
            caseDefinition = new CaseDefinition(
                PackArrangement.TryParse(sArrangement)
                , iOrientation[0]
                , iOrientation[1]);
        }
        private void LoadPalletProperties(XmlElement eltPalletProperties)
        {
            string sid = eltPalletProperties.Attributes["Id"].Value;
            string sname = eltPalletProperties.Attributes["Name"].Value;
            string sdescription = eltPalletProperties.Attributes["Description"].Value;
            string slength = eltPalletProperties.Attributes["Length"].Value;
            string swidth = eltPalletProperties.Attributes["Width"].Value;
            string sheight = eltPalletProperties.Attributes["Height"].Value;
            string sweight = eltPalletProperties.Attributes["Weight"].Value;
            string stype = eltPalletProperties.Attributes["Type"].Value;
            string sColor = eltPalletProperties.Attributes["Color"].Value;

            if ("0" == stype)
                stype = "Block";
            else if ("1" == stype)
                stype = "UK Standard";

            // create new PalletProperties instance
            PalletProperties palletProperties = CreateNewPallet(
                sname
                , sdescription
                , stype
                , UnitsManager.ConvertLengthFrom(System.Convert.ToDouble(slength, System.Globalization.CultureInfo.InvariantCulture), _unitSystem)
                , UnitsManager.ConvertLengthFrom(System.Convert.ToDouble(swidth, System.Globalization.CultureInfo.InvariantCulture), _unitSystem)
                , UnitsManager.ConvertLengthFrom(System.Convert.ToDouble(sheight, System.Globalization.CultureInfo.InvariantCulture), _unitSystem)
                , UnitsManager.ConvertMassFrom(System.Convert.ToDouble(sweight, System.Globalization.CultureInfo.InvariantCulture), _unitSystem));
            palletProperties.Color = Color.FromArgb(System.Convert.ToInt32(sColor));
            palletProperties.Guid = new Guid(sid);
        }
        private void LoadInterlayerProperties(XmlElement eltInterlayerProperties)
        {
            string sid = eltInterlayerProperties.Attributes["Id"].Value;
            string sname = eltInterlayerProperties.Attributes["Name"].Value;
            string sdescription = eltInterlayerProperties.Attributes["Description"].Value;
            string slength = eltInterlayerProperties.Attributes["Length"].Value;
            string swidth = eltInterlayerProperties.Attributes["Width"].Value;
            string sthickness = eltInterlayerProperties.Attributes["Thickness"].Value;
            string sweight = eltInterlayerProperties.Attributes["Weight"].Value;
            string sColor = eltInterlayerProperties.Attributes["Color"].Value;

            // create new InterlayerProperties instance
            InterlayerProperties interlayerProperties = CreateNewInterlayer(
                sname
                , sdescription
                , UnitsManager.ConvertLengthFrom(Convert.ToDouble(slength, System.Globalization.CultureInfo.InvariantCulture), _unitSystem)
                , UnitsManager.ConvertLengthFrom(Convert.ToDouble(swidth, System.Globalization.CultureInfo.InvariantCulture), _unitSystem)
                , UnitsManager.ConvertLengthFrom(Convert.ToDouble(sthickness, System.Globalization.CultureInfo.InvariantCulture), _unitSystem)
                , UnitsManager.ConvertMassFrom(Convert.ToDouble(sweight, System.Globalization.CultureInfo.InvariantCulture), _unitSystem)
                , Color.FromArgb(System.Convert.ToInt32(sColor)));
            interlayerProperties.Guid = new Guid(sid);
        }
        private void LoadPalletCornerProperties(XmlElement eltPalletCornerProperties)
        {
            string sid = eltPalletCornerProperties.Attributes["Id"].Value;
            string sname = eltPalletCornerProperties.Attributes["Name"].Value;
            string sdescription = eltPalletCornerProperties.Attributes["Description"].Value;
            string slength = eltPalletCornerProperties.Attributes["Length"].Value;
            string swidth = eltPalletCornerProperties.Attributes["Width"].Value;
            string sthickness = eltPalletCornerProperties.Attributes["Thickness"].Value;
            string sweight = eltPalletCornerProperties.Attributes["Weight"].Value;
            string sColor = eltPalletCornerProperties.Attributes["Color"].Value;

            // create new PalletCornerProperties instance
            PalletCornerProperties palletCornerProperties = CreateNewPalletCorners(
                sname
                , sdescription
                , UnitsManager.ConvertLengthFrom(Convert.ToDouble(slength, System.Globalization.CultureInfo.InvariantCulture), _unitSystem)
                , UnitsManager.ConvertLengthFrom(Convert.ToDouble(swidth, System.Globalization.CultureInfo.InvariantCulture), _unitSystem)
                , UnitsManager.ConvertLengthFrom(Convert.ToDouble(sthickness, System.Globalization.CultureInfo.InvariantCulture), _unitSystem)
                , UnitsManager.ConvertMassFrom(Convert.ToDouble(sweight, System.Globalization.CultureInfo.InvariantCulture), _unitSystem)
                , Color.FromArgb(System.Convert.ToInt32(sColor))
                );
            palletCornerProperties.Guid = new Guid(sid);
        }
        private void LoadPalletCapProperties(XmlElement eltPalletCapProperties)
        {
            string sid = eltPalletCapProperties.Attributes["Id"].Value;
            string sname = eltPalletCapProperties.Attributes["Name"].Value;
            string sdescription = eltPalletCapProperties.Attributes["Description"].Value;
            string slength = eltPalletCapProperties.Attributes["Length"].Value;
            string swidth = eltPalletCapProperties.Attributes["Width"].Value;
            string sheight = eltPalletCapProperties.Attributes["Height"].Value;
            string sinnerlength = eltPalletCapProperties.Attributes["InsideLength"].Value;
            string sinnerwidth = eltPalletCapProperties.Attributes["InsideWidth"].Value;
            string sinnerheight = eltPalletCapProperties.Attributes["InsideHeight"].Value;
            string sweight = eltPalletCapProperties.Attributes["Weight"].Value;
            string sColor = eltPalletCapProperties.Attributes["Color"].Value;

            PalletCapProperties palletCapProperties = CreateNewPalletCap(
                sname
                , sdescription
                , UnitsManager.ConvertLengthFrom(Convert.ToDouble(slength, System.Globalization.CultureInfo.InvariantCulture), _unitSystem)
                , UnitsManager.ConvertLengthFrom(Convert.ToDouble(swidth, System.Globalization.CultureInfo.InvariantCulture), _unitSystem)
                , UnitsManager.ConvertLengthFrom(Convert.ToDouble(sheight, System.Globalization.CultureInfo.InvariantCulture), _unitSystem)
                , UnitsManager.ConvertLengthFrom(Convert.ToDouble(sinnerlength, System.Globalization.CultureInfo.InvariantCulture), _unitSystem)
                , UnitsManager.ConvertLengthFrom(Convert.ToDouble(sinnerwidth, System.Globalization.CultureInfo.InvariantCulture), _unitSystem)
                , UnitsManager.ConvertLengthFrom(Convert.ToDouble(sinnerheight, System.Globalization.CultureInfo.InvariantCulture), _unitSystem)
                , UnitsManager.ConvertMassFrom(Convert.ToDouble(sweight, System.Globalization.CultureInfo.InvariantCulture), _unitSystem)
                , Color.FromArgb(System.Convert.ToInt32(sColor))
                );
            palletCapProperties.Guid = new Guid(sid);
        }
        private void LoadPalletFilmProperties(XmlElement eltPalletFilmProperties)
        {
            string sid = eltPalletFilmProperties.Attributes["Id"].Value;
            string sname = eltPalletFilmProperties.Attributes["Name"].Value;
            string sdescription = eltPalletFilmProperties.Attributes["Description"].Value;
            bool useTransparency = bool.Parse(eltPalletFilmProperties.Attributes["Transparency"].Value);
            bool useHatching = bool.Parse(eltPalletFilmProperties.Attributes["Hatching"].Value);
            string sHatchSpacing = eltPalletFilmProperties.Attributes["HatchSpacing"].Value;
            string sHatchAngle = eltPalletFilmProperties.Attributes["HatchAngle"].Value;
            string sColor = eltPalletFilmProperties.Attributes["Color"].Value;

            PalletFilmProperties palletFilmProperties = CreateNewPalletFilm(
                sname,
                sdescription,
                useTransparency,
                useHatching,
                UnitsManager.ConvertLengthFrom(Convert.ToDouble(sHatchSpacing, System.Globalization.CultureInfo.InvariantCulture), _unitSystem),
                Convert.ToDouble(sHatchAngle, System.Globalization.CultureInfo.InvariantCulture),
                Color.FromArgb(System.Convert.ToInt32(sColor))
                );
            palletFilmProperties.Guid = new Guid(sid);
        }
        private void LoadBundleProperties(XmlElement eltBundleProperties)
        {
            string sid = eltBundleProperties.Attributes["Id"].Value;
            string sname = eltBundleProperties.Attributes["Name"].Value;
            string sdescription = eltBundleProperties.Attributes["Description"].Value;
            double length = double.Parse(eltBundleProperties.Attributes["Length"].Value, System.Globalization.CultureInfo.InvariantCulture);
            double width = double.Parse(eltBundleProperties.Attributes["Width"].Value, System.Globalization.CultureInfo.InvariantCulture);
            double unitThickness = double.Parse(eltBundleProperties.Attributes["UnitThickness"].Value, System.Globalization.CultureInfo.InvariantCulture);
            double unitWeight = double.Parse(eltBundleProperties.Attributes["UnitWeight"].Value, System.Globalization.CultureInfo.InvariantCulture);
            Color color = Color.FromArgb(Int32.Parse(eltBundleProperties.Attributes["Color"].Value));
            int noFlats = int.Parse(eltBundleProperties.Attributes["NumberFlats"].Value);
            BundleProperties bundleProperties = CreateNewBundle(
                sname
                , sdescription
                , UnitsManager.ConvertLengthFrom(length, _unitSystem)
                , UnitsManager.ConvertLengthFrom(width, _unitSystem)
                , UnitsManager.ConvertLengthFrom(unitThickness, _unitSystem)
                , UnitsManager.ConvertMassFrom(unitWeight, _unitSystem)
                , color
                , noFlats);
            bundleProperties.Guid = new Guid(sid);
        }
        private void LoadTruckProperties(XmlElement eltTruckProperties)
        {
            string sid = eltTruckProperties.Attributes["Id"].Value;
            string sName = eltTruckProperties.Attributes["Name"].Value;
            string sDescription = eltTruckProperties.Attributes["Description"].Value;
            string slength = eltTruckProperties.Attributes["Length"].Value;
            string swidth = eltTruckProperties.Attributes["Width"].Value;
            string sheight = eltTruckProperties.Attributes["Height"].Value;
            string sadmissibleLoadWeight = eltTruckProperties.Attributes["AdmissibleLoadWeight"].Value;
            string sColor = eltTruckProperties.Attributes["Color"].Value;

            // create new truck
            TruckProperties truckProperties = CreateNewTruck(
                sName
                , sDescription
                , UnitsManager.ConvertLengthFrom(Convert.ToDouble(slength), _unitSystem)
                , UnitsManager.ConvertLengthFrom(Convert.ToDouble(swidth), _unitSystem)
                , UnitsManager.ConvertLengthFrom(Convert.ToDouble(sheight), _unitSystem)
                , UnitsManager.ConvertMassFrom(Convert.ToDouble(sadmissibleLoadWeight), _unitSystem)
                , Color.FromArgb(System.Convert.ToInt32(sColor)));
            truckProperties.Guid = new Guid(sid);
        }
        #endregion

        #region Load case optimisation
        private void LoadOptimConstraintSet(XmlElement eltConstraintSet, out CaseOptimConstraintSet constraintSet)
        {
            string sNoWalls = eltConstraintSet.Attributes["NumberOfWalls"].Value;
            int[] iNoWalls = ParseInt3(sNoWalls);
            double wallThickness = UnitsManager.ConvertLengthFrom(
                Convert.ToDouble(eltConstraintSet.Attributes["WallThickness"].Value, System.Globalization.CultureInfo.InvariantCulture)
                , _unitSystem);
            double wallSurfaceMass = UnitsManager.ConvertSurfaceMassFrom(
                Convert.ToDouble(eltConstraintSet.Attributes["WallSurfaceMass"].Value, System.Globalization.CultureInfo.InvariantCulture)
                , _unitSystem);
            constraintSet = new CaseOptimConstraintSet(iNoWalls, wallThickness, wallSurfaceMass, Vector3D.Zero, Vector3D.Zero, false); 
        }
        #endregion

        #region Load analysis
        private void LoadAnalysis(XmlElement eltAnalysis)
        {
            string sName = eltAnalysis.Attributes["Name"].Value;
            string sDescription = eltAnalysis.Attributes["Description"].Value;
            string sInterlayerId = string.Empty;
            if (eltAnalysis.HasAttribute("InterlayerId"))
                sInterlayerId = eltAnalysis.Attributes["InterlayerId"].Value;
            string sInterlayerAntiSlipId = string.Empty;
            if (eltAnalysis.HasAttribute("InterlayerAntiSlipId"))
                sInterlayerAntiSlipId = eltAnalysis.Attributes["InterlayerAntiSlipId"].Value;
            string sPalletCornerId = string.Empty;
            if (eltAnalysis.HasAttribute("PalletCornerId"))
                sPalletCornerId = eltAnalysis.Attributes["PalletCornerId"].Value;
            string sPalletCapId = string.Empty;
            if (eltAnalysis.HasAttribute("PalletCapId"))
                sPalletCapId = eltAnalysis.Attributes["PalletCapId"].Value;
            string sPalletFilmId = string.Empty;
            if (eltAnalysis.HasAttribute("PalletFilmId"))
                sPalletFilmId = eltAnalysis.Attributes["PalletFilmId"].Value;

            if (string.Equals(eltAnalysis.Name, "AnalysisPallet", StringComparison.CurrentCultureIgnoreCase))
            {
                string sBoxId = eltAnalysis.Attributes["BoxId"].Value;
                string sPalletId = eltAnalysis.Attributes["PalletId"].Value;

                // load constraint set / solution list
                PalletConstraintSet constraintSet = null;
                List<CasePalletSolution> solutions = new List<CasePalletSolution>();
                List<int> selectedIndices = new List<int>();

                foreach (XmlNode node in eltAnalysis.ChildNodes)
                {
                    // load constraint set
                    if (string.Equals(node.Name, "ConstraintSetBox", StringComparison.CurrentCultureIgnoreCase))
                        constraintSet = LoadCasePalletConstraintSet_Box(node as XmlElement);
                    else if (string.Equals(node.Name, "ConstraintSetBundle", StringComparison.CurrentCultureIgnoreCase))
                        constraintSet = LoadCasePalletConstraintSet_Bundle(node as XmlElement);
                    // load solutions
                    else if (string.Equals(node.Name, "Solutions", StringComparison.CurrentCultureIgnoreCase))
                    {
                        int indexSol = 0;
                        foreach (XmlNode solutionNode in node.ChildNodes)
                        {
                            XmlElement eltSolution = solutionNode as XmlElement;
                            solutions.Add(LoadCasePalletSolution(eltSolution));
                            // is solution selected ?
                            if (null != eltSolution.Attributes["Selected"] && "true" == eltSolution.Attributes["Selected"].Value)
                                selectedIndices.Add(indexSol);
                            ++indexSol;
                        }
                    }
                    if (null == constraintSet)
                        throw new Exception("Failed to load a valid ConstraintSet");
                }

                // instantiate analysis
                CasePalletAnalysis analysis = CreateNewCasePalletAnalysis(
                    sName
                    , sDescription
                    , GetTypeByGuid(new Guid(sBoxId)) as BProperties
                    , GetTypeByGuid(new Guid(sPalletId)) as PalletProperties
                    , string.IsNullOrEmpty(sInterlayerId) ? null : GetTypeByGuid(new Guid(sInterlayerId)) as InterlayerProperties
                    , string.IsNullOrEmpty(sInterlayerAntiSlipId) ? null : GetTypeByGuid( new Guid(sInterlayerAntiSlipId) ) as InterlayerProperties
                    , string.IsNullOrEmpty(sPalletCornerId) ? null : GetTypeByGuid( new Guid(sPalletCornerId) ) as PalletCornerProperties
                    , string.IsNullOrEmpty(sPalletCapId) ? null : GetTypeByGuid( new Guid(sPalletCapId) ) as PalletCapProperties
                    , string.IsNullOrEmpty(sPalletFilmId) ? null : GetTypeByGuid( new Guid(sPalletFilmId) ) as PalletFilmProperties
                    , constraintSet
                    , solutions);
                // save selected solutions
                foreach (int indexSol in selectedIndices)
                    analysis.SelectSolutionByIndex(indexSol);


                // reprocess for truck analyses
                // Note : this is quite complicated, see later if it could be simplified 
                foreach (XmlNode node in eltAnalysis.ChildNodes)
                {
                    if (string.Equals(node.Name, "Solutions", StringComparison.CurrentCultureIgnoreCase))
                    {
                        int indexSol = 0;
                        foreach (XmlNode solutionNode in node.ChildNodes)
                        {
                            foreach (XmlNode solutionInnerNode in solutionNode.ChildNodes)
                            {
                                if (string.Equals("TruckAnalyses", solutionInnerNode.Name, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    XmlElement truckAnalysesElt = solutionInnerNode as XmlElement;
                                    foreach (XmlNode truckAnalysisNode in truckAnalysesElt.ChildNodes)
                                    {
                                        if (string.Equals("TruckAnalysis", truckAnalysisNode.Name, StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            XmlElement truckAnalysisElt = truckAnalysisNode as XmlElement;
                                            SelCasePalletSolution selSolution = analysis.GetSelSolutionBySolutionIndex(indexSol);
                                            LoadTruckAnalysis(truckAnalysisElt, selSolution);
                                        }
                                    }
                                }
                                else if (string.Equals("ECTAnalyses", solutionInnerNode.Name, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    XmlElement ectAnalysesElt = solutionInnerNode as XmlElement;
                                    foreach (XmlNode ectAnalysisNode in ectAnalysesElt.ChildNodes)
                                    {
                                        if (string.Equals("EctAnalysis", ectAnalysisNode.Name, StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            XmlElement ectAnalysisElt = ectAnalysisNode as XmlElement;
                                            SelCasePalletSolution selSolution = analysis.GetSelSolutionBySolutionIndex(indexSol);
                                            LoadECTAnalysis(ectAnalysisElt, selSolution);
                                        }
                                    }
                                }
                            }
                            ++indexSol;
                        }
                    }
                }
            }
            else if (string.Equals(eltAnalysis.Name, "PackPalletAnalysis", StringComparison.CurrentCultureIgnoreCase))
            {
                string sPackId = eltAnalysis.Attributes["PackId"].Value;
                string sPalletId = eltAnalysis.Attributes["PalletId"].Value;

                // load constraint set / solution list
                PackPalletConstraintSet constraintSet = null;
                List<PackPalletSolution> solutions = new List<PackPalletSolution>();
                List<int> selectedIndices = new List<int>();

                foreach (XmlNode node in eltAnalysis.ChildNodes)
                { 
                    // load constraint set
                    if (string.Equals(node.Name, "ConstraintSet", StringComparison.CurrentCultureIgnoreCase))
                        constraintSet = LoadPackPalletConstraintSet(node as XmlElement);
                    // load solutions
                    else if (string.Equals(node.Name, "Solutions", StringComparison.CurrentCultureIgnoreCase))
                    {
                        int indexSol = 0;
                        foreach (XmlNode solutionNode in node.ChildNodes)
                        {
                            XmlElement eltSolution = solutionNode as XmlElement;
                            solutions.Add( LoadPackPalletSolution(eltSolution) );
                            // is solution selected ?
                            if (null != eltSolution.Attributes["Selected"] && "true" == eltSolution.Attributes["Selected"].Value)
                                selectedIndices.Add(indexSol);
                            ++indexSol;
                        }
                    }
                }
                PackPalletAnalysis analysis = CreateNewPackPalletAnalysis(
                    sName
                    , sDescription
                    , GetTypeByGuid(new Guid(sPackId)) as PackProperties
                    , GetTypeByGuid(new Guid(sPalletId)) as PalletProperties
                    , string.IsNullOrEmpty(sInterlayerId) ? null : GetTypeByGuid(new Guid(sInterlayerId)) as InterlayerProperties
                    , constraintSet
                    , solutions
                    );
                // save selected solutions
                foreach (int indexSol in selectedIndices)
                    analysis.SelectSolutionByIndex(indexSol);
            }
            else if (string.Equals(eltAnalysis.Name, "CylinderPalletAnalysis", StringComparison.CurrentCultureIgnoreCase))
            {
                string sCylinderId = eltAnalysis.Attributes["CylinderId"].Value;
                string sPalletId = eltAnalysis.Attributes["PalletId"].Value;

                // load constraint set / solution list
                CylinderPalletConstraintSet constraintSet = null;
                List<CylinderPalletSolution> solutions = new List<CylinderPalletSolution>();
                List<int> selectedIndices = new List<int>();

                foreach (XmlNode node in eltAnalysis.ChildNodes)
                {
                    // load constraint set
                    if (string.Equals(node.Name, "ConstraintSet", StringComparison.CurrentCultureIgnoreCase))
                        constraintSet = LoadCylinderPalletConstraintSet(node as XmlElement);
                    // load solutions
                    else if (string.Equals(node.Name, "Solutions", StringComparison.CurrentCultureIgnoreCase))
                    {
                        int indexSol = 0;
                        foreach (XmlNode solutionNode in node.ChildNodes)
                        {
                            XmlElement eltSolution = solutionNode as XmlElement;
                            solutions.Add(LoadCylinderPalletSolution(eltSolution));
                            // is solution selected ?
                            if (null != eltSolution.Attributes["Selected"] && "true" == eltSolution.Attributes["Selected"].Value)
                                selectedIndices.Add(indexSol);
                            ++indexSol;
                        }
                    }
                }

                // instantiate analysis
                CylinderPalletAnalysis analysis = CreateNewCylinderPalletAnalysis(
                    sName
                    , sDescription
                    , GetTypeByGuid(new Guid(sCylinderId)) as CylinderProperties
                    , GetTypeByGuid(new Guid(sPalletId)) as PalletProperties
                    , string.IsNullOrEmpty(sInterlayerId) ? null : GetTypeByGuid(new Guid(sInterlayerId)) as InterlayerProperties
                    , string.IsNullOrEmpty(sInterlayerAntiSlipId) ? null : GetTypeByGuid(new Guid(sInterlayerAntiSlipId)) as InterlayerProperties
                    , constraintSet
                    , solutions);
                // save selected solutions
                foreach (int indexSol in selectedIndices)
                    analysis.SelectSolutionByIndex(indexSol);
            }
            else if (string.Equals(eltAnalysis.Name, "HCylinderPalletAnalysis", StringComparison.CurrentCultureIgnoreCase))
            {
                string sCylinderId = eltAnalysis.Attributes["CylinderId"].Value;
                string sPalletId = eltAnalysis.Attributes["PalletId"].Value;

                // load constraint set / solution list
                HCylinderPalletConstraintSet constraintSet = null;
                List<HCylinderPalletSolution> solutions = new List<HCylinderPalletSolution>();
                List<int> selectedIndices = new List<int>();

                foreach (XmlNode node in eltAnalysis.ChildNodes)
                {
                    // load constraint set
                    if (string.Equals(node.Name, "ConstraintSet", StringComparison.CurrentCultureIgnoreCase))
                        constraintSet = LoadHCylinderPalletConstraintSet(node as XmlElement);
                    // load solutions
                    else if (string.Equals(node.Name, "Solutions", StringComparison.CurrentCultureIgnoreCase))
                    {
                        int indexSol = 0;
                        foreach (XmlNode solutionNode in node.ChildNodes)
                        {
                            XmlElement eltSolution = solutionNode as XmlElement;
                            solutions.Add(LoadHCylinderPalletSolution(eltSolution));
                            // is solution selected ?
                            if (null != eltSolution.Attributes["Selected"] && "true" == eltSolution.Attributes["Selected"].Value)
                                selectedIndices.Add(indexSol);
                            ++indexSol;
                        }
                    }
                }

                // instantiate analysis
                HCylinderPalletAnalysis analysis = CreateNewHCylinderPalletAnalysis(
                    sName
                    , sDescription
                    , GetTypeByGuid(new Guid(sCylinderId)) as CylinderProperties
                    , GetTypeByGuid(new Guid(sPalletId)) as PalletProperties
                    , constraintSet
                    , solutions);
                // save selected solutions
                foreach (int indexSol in selectedIndices)
                    analysis.SelectSolutionByIndex(indexSol);
            }
            else if (string.Equals(eltAnalysis.Name, "AnalysisCase", StringComparison.CurrentCultureIgnoreCase))
            {
                string sBoxId = eltAnalysis.Attributes["BoxId"].Value;
                // load constraint set / pallet solutions descriptors / solution list
                BoxCasePalletConstraintSet constraintSet = null;
                List<PalletSolutionDesc> palletSolutionDescriptors = new List<PalletSolutionDesc>();
                XmlElement caseSolutionsElt = null;

                // first load ConstraintSetCase / PalletSolutionDescriptors / CaseSolutions
                foreach (XmlNode node in eltAnalysis.ChildNodes)
                {
                    // load constraint set
                    if (string.Equals(node.Name, "ConstraintSetCase", StringComparison.CurrentCultureIgnoreCase))
                        constraintSet = LoadCaseConstraintSet(node as XmlElement);
                    // load pallet solutions descriptors
                    else if (string.Equals(node.Name, "PalletSolutionDescriptors", StringComparison.CurrentCultureIgnoreCase))
                    {
                        foreach (XmlNode palletSolutionNode in node.ChildNodes)
                            palletSolutionDescriptors.Add(LoadPalletSolutionDescriptor(palletSolutionNode as XmlElement));
                    }
                    // load solutions
                    else if (string.Equals(node.Name, "CaseSolutions", StringComparison.CurrentCultureIgnoreCase))
                    {
                        caseSolutionsElt = node as XmlElement;
                    }
                }

                // instantiate caseAnalysis
                BoxCasePalletAnalysis caseAnalysis = CreateNewBoxCasePalletOptimization(
                    sName
                    , sDescription
                    , GetTypeByGuid(new Guid(sBoxId)) as BoxProperties
                    , constraintSet
                    , palletSolutionDescriptors
                    , null
                    );

                // second : solutions
                List<BoxCasePalletSolution> caseSolutions = new List<BoxCasePalletSolution>();
                int indexSol = 0;
                List<int> selectedIndices = new List<int>();
                foreach (XmlNode solutionNode in caseSolutionsElt.ChildNodes)
                {
                    XmlElement eltSolution = solutionNode as XmlElement;
                    caseSolutions.Add(LoadBoxCasePalletSolution(eltSolution, caseAnalysis));

                    // is solution selected ?
                    if (null != eltSolution.Attributes["Selected"] && "true" == eltSolution.Attributes["Selected"].Value)
                        selectedIndices.Add(indexSol);
                    ++indexSol;
                }
                caseAnalysis.Solutions = caseSolutions;

                foreach (int index in selectedIndices)
                    caseAnalysis.SelectSolutionByIndex(index);
            }
            else if (string.Equals(eltAnalysis.Name, "AnalysisBoxCase", StringComparison.CurrentCultureIgnoreCase))
            {
                // load caseId
                string sBoxId = eltAnalysis.Attributes["BoxId"].Value;
                string sCaseId = eltAnalysis.Attributes["CaseId"].Value;

                // load constraint set / solution list
                BCaseConstraintSet constraintSet = null;
                List<BoxCaseSolution> solutions = new List<BoxCaseSolution>();
                List<int> selectedIndices = new List<int>();

                // first load BoxCaseConstraintSet / BoxCaseSolution(s)
                XmlElement boxCaseSolutionsElt = null;
                foreach (XmlNode node in eltAnalysis.ChildNodes)
                {
                    // load constraint set
                    if (string.Equals(node.Name, "ConstraintSetCase", StringComparison.CurrentCultureIgnoreCase))
                        constraintSet = LoadBoxCaseConstraintSet(node as XmlElement);
                    // load solutions
                    else if (string.Equals(node.Name, "Solutions", StringComparison.CurrentCultureIgnoreCase))
                    {
                        boxCaseSolutionsElt = node as XmlElement;

                        int indexSol = 0;
                        foreach (XmlNode solutionNode in boxCaseSolutionsElt.ChildNodes)
                        {
                            XmlElement eltSolution = solutionNode as XmlElement;
                            solutions.Add(LoadBoxCaseSolution(eltSolution));
                            // is solution selected ?
                            if (null != eltSolution.Attributes["Selected"] && "true" == eltSolution.Attributes["Selected"].Value)
                                selectedIndices.Add(indexSol);
                            ++indexSol;
                        }
                    }
                }

                BProperties bProperties = GetTypeByGuid(new Guid(sBoxId)) as BProperties;

                // instantiate box/case analysis
                BoxCaseAnalysis analysis = CreateNewBoxCaseAnalysis(
                         sName
                         , sDescription
                         , bProperties
                         , GetTypeByGuid(new Guid(sCaseId)) as BoxProperties
                         , constraintSet
                         , solutions
                         );

                // save selected solutions
                foreach (int indexSol in selectedIndices)
                    analysis.SelectSolutionByIndex(indexSol);
            }
        }

        private PalletSolutionDesc LoadPalletSolutionDescriptor(XmlElement palletSolutionDescriptorElt)
        {
            string palletDimensions = palletSolutionDescriptorElt.Attributes["PalletDimensions"].Value;
            string overhang = palletSolutionDescriptorElt.Attributes["PalletOverhang"].Value;
            string caseDimensions = palletSolutionDescriptorElt.Attributes["CaseDimensions"].Value;
            string caseInsideDimensions = palletSolutionDescriptorElt.Attributes["CaseInsideDimensions"].Value;
            string caseWeight = string.Empty;
            if (palletSolutionDescriptorElt.HasAttribute("CaseWeight"))
                caseWeight = palletSolutionDescriptorElt.Attributes["CaseWeight"].Value;
            else
                caseWeight = "0.0";
            string palletWeight = string.Empty;
            if (palletSolutionDescriptorElt.HasAttribute("palletWeight"))
                palletWeight = palletSolutionDescriptorElt.Attributes["PalletWeight"].Value;
            else
                palletWeight = "0.0";
            string caseCount = palletSolutionDescriptorElt.Attributes["CaseCount"].Value;
            string sGuid = palletSolutionDescriptorElt.Attributes["Id"].Value;
            string friendlyName = palletSolutionDescriptorElt.Attributes["FriendlyName"].Value;
            return new PalletSolutionDesc(PalletSolutionDatabase.Instance
                , palletDimensions
                , overhang
                , caseDimensions
                , caseInsideDimensions
                , caseWeight
                , palletWeight
                , caseCount
                , sGuid
                , friendlyName);
        }

        private BoxCasePalletConstraintSet LoadCaseConstraintSet(XmlElement eltConstraintSet)
        {
            BoxCasePalletConstraintSet constraints = new BoxCasePalletConstraintSet();
            // align layers allowed
            if (eltConstraintSet.HasAttribute("AlignedLayersAllowed"))
                constraints.AllowAlignedLayers = string.Equals(eltConstraintSet.Attributes["AlignedLayersAllowed"].Value, "true", StringComparison.CurrentCultureIgnoreCase);
            // alternate layers allowed
            if (eltConstraintSet.HasAttribute("AlternateLayersAllowed"))
                constraints.AllowAlternateLayers = string.Equals(eltConstraintSet.Attributes["AlternateLayersAllowed"].Value, "true", StringComparison.CurrentCultureIgnoreCase);
            // allowed orthogonal axes
            if (eltConstraintSet.HasAttribute("AllowedBoxPositions"))
                constraints.AllowOrthoAxisString = eltConstraintSet.Attributes["AllowedBoxPositions"].Value;
            // allowed patterns
            if (eltConstraintSet.HasAttribute("AllowedPatterns"))
                constraints.AllowedPatternString = eltConstraintSet.Attributes["AllowedPatterns"].Value;
            // stop criterions
            if (constraints.UseMaximumNumberOfItems = eltConstraintSet.HasAttribute("ManimumNumberOfItems"))
                constraints.MaximumNumberOfItems = int.Parse(eltConstraintSet.Attributes["ManimumNumberOfItems"].Value);
            // maximum case weight
            if (constraints.UseMaximumCaseWeight = eltConstraintSet.HasAttribute("MaximumCaseWeight"))
                constraints.MaximumCaseWeight = UnitsManager.ConvertMassFrom(double.Parse(eltConstraintSet.Attributes["MaximumCaseWeight"].Value), _unitSystem);
            // number of solutions to keep
            if (constraints.UseNumberOfSolutionsKept = eltConstraintSet.HasAttribute("NumberOfSolutions"))
                constraints.NumberOfSolutionsKept = int.Parse(eltConstraintSet.Attributes["NumberOfSolutions"].Value);
            // minimum number of items
            if (constraints.UseMinimumNumberOfItems = eltConstraintSet.HasAttribute("MinimumNumberOfItems"))
                constraints.MinimumNumberOfItems = int.Parse(eltConstraintSet.Attributes["MinimumNumberOfItems"].Value);
            // sanity check
            if (!constraints.IsValid)
                throw new Exception("Invalid constraint set");
            return constraints;
        }

        private BCaseConstraintSet LoadBoxCaseConstraintSet(XmlElement eltConstraintSet)
        {
            BCaseConstraintSet constraints = null;
            
            // allowed orthogonal axes
            if (eltConstraintSet.HasAttribute("AllowedBoxPositions"))
            {
                constraints = new BoxCaseConstraintSet();
                BoxCaseConstraintSet boxCaseContraintSet = constraints as BoxCaseConstraintSet;
                boxCaseContraintSet.AllowOrthoAxisString = eltConstraintSet.Attributes["AllowedBoxPositions"].Value;
            }
            else
                constraints = new BundleCaseConstraintSet();
            // maximum case weight
            if (constraints.UseMaximumCaseWeight = eltConstraintSet.HasAttribute("MaximumCaseWeight"))
                constraints.MaximumCaseWeight = UnitsManager.ConvertMassFrom(double.Parse(eltConstraintSet.Attributes["MaximumCaseWeight"].Value), _unitSystem);
            // allowed patterns
            if (constraints.UseMaximumNumberOfBoxes = eltConstraintSet.HasAttribute("ManimumNumberOfItems"))
                constraints.MaximumNumberOfBoxes = int.Parse(eltConstraintSet.Attributes["ManimumNumberOfItems"].Value);
            // sanity check
            if (!constraints.IsValid)
                throw new Exception("Invalid constraint set");
            return constraints;
        }

        private PalletConstraintSet LoadCasePalletConstraintSet_Box(XmlElement eltConstraintSet)
        {
            CasePalletConstraintSet constraints = new CasePalletConstraintSet();
            // align layers allowed
            if (eltConstraintSet.HasAttribute("AlignedLayersAllowed"))
                constraints.AllowAlignedLayers = string.Equals(eltConstraintSet.Attributes["AlignedLayersAllowed"].Value, "true", StringComparison.CurrentCultureIgnoreCase);
            // alternate layers allowed
            if (eltConstraintSet.HasAttribute("AlternateLayersAllowed"))
                constraints.AllowAlternateLayers = string.Equals(eltConstraintSet.Attributes["AlternateLayersAllowed"].Value, "true", StringComparison.CurrentCultureIgnoreCase);
            // allowed orthogonal axes
            if (eltConstraintSet.HasAttribute("AllowedBoxPositions"))
            {
                string allowedOrthoAxes = eltConstraintSet.Attributes["AllowedBoxPositions"].Value;
                string[] sAxes = allowedOrthoAxes.Split(',');
                foreach (string sAxis in sAxes)
                    constraints.SetAllowedOrthoAxis(HalfAxis.Parse(sAxis), true);
            }
            // allowed patterns
            if (eltConstraintSet.HasAttribute("AllowedPatterns"))
                constraints.AllowedPatternString = eltConstraintSet.Attributes["AllowedPatterns"].Value;
            // stop criterions
            if (constraints.UseMaximumHeight = eltConstraintSet.HasAttribute("MaximumHeight"))
                constraints.MaximumHeight = UnitsManager.ConvertLengthFrom(double.Parse(eltConstraintSet.Attributes["MaximumHeight"].Value), _unitSystem);
            if (constraints.UseMaximumNumberOfCases = eltConstraintSet.HasAttribute("ManimumNumberOfItems"))
                constraints.MaximumNumberOfItems = int.Parse(eltConstraintSet.Attributes["ManimumNumberOfItems"].Value);
            if (constraints.UseMaximumPalletWeight = eltConstraintSet.HasAttribute("MaximumPalletWeight"))
                constraints.MaximumPalletWeight = UnitsManager.ConvertMassFrom(double.Parse(eltConstraintSet.Attributes["MaximumPalletWeight"].Value), _unitSystem);
            if (constraints.UseMaximumWeightOnBox = eltConstraintSet.HasAttribute("MaximumWeightOnBox"))
                constraints.MaximumWeightOnBox = UnitsManager.ConvertMassFrom(double.Parse(eltConstraintSet.Attributes["MaximumWeightOnBox"].Value), _unitSystem);
            // overhang / underhang
            if (eltConstraintSet.HasAttribute("OverhangX"))
                constraints.OverhangX = UnitsManager.ConvertLengthFrom(double.Parse(eltConstraintSet.Attributes["OverhangX"].Value), _unitSystem);
            if (eltConstraintSet.HasAttribute("OverhangY"))
                constraints.OverhangY = UnitsManager.ConvertLengthFrom(double.Parse(eltConstraintSet.Attributes["OverhangY"].Value), _unitSystem);
            // number of solutions to keep
            if (constraints.UseNumberOfSolutionsKept = eltConstraintSet.HasAttribute("NumberOfSolutions"))
                constraints.NumberOfSolutionsKept = int.Parse(eltConstraintSet.Attributes["NumberOfSolutions"].Value);
            // pallet film turns
            if (eltConstraintSet.HasAttribute("PalletFilmTurns"))
                constraints.PalletFilmTurns = int.Parse(eltConstraintSet.Attributes["PalletFilmTurns"].Value);
            // sanity check
            if (!constraints.IsValid)
                throw new Exception("Invalid constraint set");
            return constraints;
        }
        PalletConstraintSet LoadCasePalletConstraintSet_Bundle(XmlElement eltConstraintSet)
        {
            BundlePalletConstraintSet constraints = new BundlePalletConstraintSet();
            // aligned layers allowed
            if (eltConstraintSet.HasAttribute("AlignedLayersAllowed"))
                constraints.AllowAlignedLayers = string.Equals(eltConstraintSet.Attributes["AlignedLayersAllowed"].Value, "true", StringComparison.CurrentCultureIgnoreCase);
            // alternate layers allowed
            if (eltConstraintSet.HasAttribute("AlternateLayersAllowed"))
                constraints.AllowAlternateLayers = string.Equals(eltConstraintSet.Attributes["AlternateLayersAllowed"].Value, "true", StringComparison.CurrentCultureIgnoreCase);
            // allowed patterns
            if (eltConstraintSet.HasAttribute("AllowedPatterns"))
                constraints.AllowedPatternString = eltConstraintSet.Attributes["AllowedPatterns"].Value;
            // stop criterions
            if (constraints.UseMaximumHeight = eltConstraintSet.HasAttribute("MaximumHeight"))
                constraints.MaximumHeight = UnitsManager.ConvertLengthFrom(double.Parse(eltConstraintSet.Attributes["MaximumHeight"].Value), _unitSystem);
            if (constraints.UseMaximumNumberOfCases = eltConstraintSet.HasAttribute("ManimumNumberOfItems"))
                constraints.MaximumNumberOfItems = int.Parse(eltConstraintSet.Attributes["ManimumNumberOfItems"].Value);
            if (constraints.UseMaximumPalletWeight = eltConstraintSet.HasAttribute("MaximumPalletWeight"))
                constraints.MaximumPalletWeight = UnitsManager.ConvertMassFrom(double.Parse(eltConstraintSet.Attributes["MaximumPalletWeight"].Value), _unitSystem);
            // overhang / underhang
            if (eltConstraintSet.HasAttribute("OverhangX"))
                constraints.OverhangX = UnitsManager.ConvertLengthFrom(double.Parse(eltConstraintSet.Attributes["OverhangX"].Value), _unitSystem);
            if (eltConstraintSet.HasAttribute("OverhangY"))
                constraints.OverhangY = UnitsManager.ConvertLengthFrom(double.Parse(eltConstraintSet.Attributes["OverhangY"].Value), _unitSystem);
            // number of solutions to keep
            if (constraints.UseNumberOfSolutionsKept = eltConstraintSet.HasAttribute("NumberOfSolutions"))
                constraints.NumberOfSolutionsKept = int.Parse(eltConstraintSet.Attributes["NumberOfSolutions"].Value);
            // sanity check
            if (!constraints.IsValid)
                throw new Exception("Invalid constraint set");
            return constraints;
        }
        private PackPalletConstraintSet LoadPackPalletConstraintSet(XmlElement eltContraintSet)
        {
            PackPalletConstraintSet constraints = new PackPalletConstraintSet();
            if (eltContraintSet.HasAttribute("OverhangX"))
                constraints.OverhangX = UnitsManager.ConvertLengthFrom(double.Parse(eltContraintSet.Attributes["OverhangX"].Value), _unitSystem);
            if (eltContraintSet.HasAttribute("OverhangY"))
                constraints.OverhangY = UnitsManager.ConvertLengthFrom(double.Parse(eltContraintSet.Attributes["OverhangY"].Value), _unitSystem);
            constraints.MinOverhangX = LoadOptDouble(eltContraintSet, "MinOverhangX", UnitsManager.UnitType.UT_LENGTH);
            constraints.MinOverhangY = LoadOptDouble(eltContraintSet, "MinOverhangY", UnitsManager.UnitType.UT_LENGTH);
            constraints.MinimumSpace = LoadOptDouble(eltContraintSet, "MinimumSpace", UnitsManager.UnitType.UT_LENGTH);
            constraints.MaximumSpaceAllowed = LoadOptDouble(eltContraintSet, "MaximumSpaceAllowed", UnitsManager.UnitType.UT_LENGTH);
            constraints.MaximumPalletHeight = LoadOptDouble(eltContraintSet, "MaximumPalletHeight", UnitsManager.UnitType.UT_LENGTH);
            constraints.MaximumPalletWeight = LoadOptDouble(eltContraintSet, "MaximumPalletWeight", UnitsManager.UnitType.UT_MASS);
            constraints.LayerSwapPeriod = int.Parse( eltContraintSet.Attributes["LayerSwapPeriod"].Value );
            constraints.InterlayerPeriod = int.Parse( eltContraintSet.Attributes["InterlayerPeriod"].Value );
            if (!constraints.IsValid)
                throw new Exception("Invalid constraint set");
            return constraints;
        }

        private OptDouble LoadOptDouble(XmlElement xmlElement, string attribute, UnitsManager.UnitType unitType)
        {
            if (!xmlElement.HasAttribute(attribute))
                return new OptDouble(false, 0.0);
            else
            {
                OptDouble optD = OptDouble.Parse(xmlElement.Attributes[attribute].Value);
                switch (unitType)
                {
                    case UnitsManager.UnitType.UT_LENGTH:
                        optD.Value = UnitsManager.ConvertLengthFrom(optD.Value, _unitSystem);
                        break;
                    case UnitsManager.UnitType.UT_MASS:
                        optD.Value = UnitsManager.ConvertMassFrom(optD.Value, _unitSystem);
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }
                return optD;
            }
        }

        private CylinderPalletConstraintSet LoadCylinderPalletConstraintSet(XmlElement eltConstraintSet)
        {
            CylinderPalletConstraintSet constraints = new CylinderPalletConstraintSet();
            // stop criterions
            if (constraints.UseMaximumPalletHeight = eltConstraintSet.HasAttribute("MaximumHeight"))
                constraints.MaximumPalletHeight = UnitsManager.ConvertLengthFrom(double.Parse(eltConstraintSet.Attributes["MaximumHeight"].Value), _unitSystem);
            if (constraints.UseMaximumNumberOfItems = eltConstraintSet.HasAttribute("ManimumNumberOfItems"))
                constraints.MaximumNumberOfItems = int.Parse(eltConstraintSet.Attributes["ManimumNumberOfItems"].Value);
            if (constraints.UseMaximumPalletWeight = eltConstraintSet.HasAttribute("MaximumPalletWeight"))
                constraints.MaximumPalletWeight = UnitsManager.ConvertMassFrom(double.Parse(eltConstraintSet.Attributes["MaximumPalletWeight"].Value), _unitSystem);
            if (constraints.UseMaximumLoadOnLowerCylinder = eltConstraintSet.HasAttribute("MaximumLoadOnLowerCylinder"))
                constraints.MaximumLoadOnLowerCylinder = UnitsManager.ConvertMassFrom(double.Parse(eltConstraintSet.Attributes["MaximumLoadOnLowerCylinder"].Value), _unitSystem);
            // overhang / underhang
            if (eltConstraintSet.HasAttribute("OverhangX"))
                constraints.OverhangX = UnitsManager.ConvertLengthFrom(double.Parse(eltConstraintSet.Attributes["OverhangX"].Value), _unitSystem);
            if (eltConstraintSet.HasAttribute("OverhangY"))
                constraints.OverhangY = UnitsManager.ConvertLengthFrom(double.Parse(eltConstraintSet.Attributes["OverhangY"].Value), _unitSystem);
            return constraints;
        }

        private HCylinderPalletConstraintSet LoadHCylinderPalletConstraintSet(XmlElement eltConstraintSet)
        {
            HCylinderPalletConstraintSet constraints = new HCylinderPalletConstraintSet();
            // stop criterions
            if (constraints.UseMaximumPalletHeight = eltConstraintSet.HasAttribute("MaximumHeight"))
                constraints.MaximumPalletHeight = UnitsManager.ConvertLengthFrom(double.Parse(eltConstraintSet.Attributes["MaximumHeight"].Value), _unitSystem);
            if (constraints.UseMaximumNumberOfItems = eltConstraintSet.HasAttribute("ManimumNumberOfItems"))
                constraints.MaximumNumberOfItems = int.Parse(eltConstraintSet.Attributes["ManimumNumberOfItems"].Value);
            if (constraints.UseMaximumPalletWeight = eltConstraintSet.HasAttribute("MaximumPalletWeight"))
                constraints.MaximumPalletWeight = UnitsManager.ConvertMassFrom(double.Parse(eltConstraintSet.Attributes["MaximumPalletWeight"].Value), _unitSystem);
            // overhang / underhang
            if (eltConstraintSet.HasAttribute("OverhangX"))
                constraints.OverhangX = UnitsManager.ConvertLengthFrom(double.Parse(eltConstraintSet.Attributes["OverhangX"].Value), _unitSystem);
            if (eltConstraintSet.HasAttribute("OverhangY"))
                constraints.OverhangY = UnitsManager.ConvertLengthFrom(double.Parse(eltConstraintSet.Attributes["OverhangY"].Value), _unitSystem);
            return constraints;
        }

        private CasePalletSolution LoadCasePalletSolution(XmlElement eltSolution)
        {
            // title -> instantiation
            string stitle = eltSolution.Attributes["Title"].Value;
            CasePalletSolution sol = new CasePalletSolution(null, stitle, true);
            // homogeneous layers
            if (eltSolution.HasAttribute("HomogeneousLayers"))
            {
                string sHomogeneousLayers = eltSolution.Attributes["HomogeneousLayers"].Value;
                sol.HasHomogeneousLayers = string.Equals(sHomogeneousLayers, "true", StringComparison.CurrentCultureIgnoreCase);
            }
            else
                sol.HasHomogeneousLayers = false;
            // limit reached
            if (eltSolution.HasAttribute("LimitReached"))
            {
                string sLimitReached = eltSolution.Attributes["LimitReached"].Value;
                sol.LimitReached = (CasePalletSolution.Limit)(int.Parse(sLimitReached));
            }
            // layers
            XmlElement eltLayers = eltSolution.ChildNodes[0] as XmlElement;
            foreach (XmlNode nodeLayer in eltLayers.ChildNodes)
                sol.Add( LoadLayer(nodeLayer as XmlElement));
            return sol;
        }

        private PackPalletSolution LoadPackPalletSolution(XmlElement eltSolution)
        { 
            // title -> instantiation
            string stitle = eltSolution.Attributes["Title"].Value;
            // layer and list
            ILayer layer = null;
            List<LayerDescriptor> layerDescriptors = new List<LayerDescriptor>();

            foreach (XmlNode nodeSolChild in eltSolution.ChildNodes)
            {
                XmlElement eltChild = nodeSolChild as XmlElement;
                if (null != eltChild)
                {
                    if (string.Equals(eltChild.Name, "BoxLayers", StringComparison.CurrentCultureIgnoreCase))
                    {
                        foreach (XmlNode nodeLayer in eltChild.ChildNodes)
                        {
                            XmlElement eltLayer = nodeLayer as XmlElement;
                            if (null != eltLayer)
                                layer = LoadLayer(eltLayer);
                        }
                    }
                    else if (string.Equals(eltChild.Name, "LayerRefs", StringComparison.CurrentCultureIgnoreCase))
                    {
                        foreach (XmlNode nodeLayerRef in eltChild.ChildNodes)
                        {
                            XmlElement eltLayerRef = nodeLayerRef as XmlElement;
                            if (null != eltLayerRef)
                            {
                                bool swapped = bool.Parse(eltLayerRef.Attributes["Swapped"].Value);
                                bool hasInterlayer = bool.Parse(eltLayerRef.Attributes["HasInterlayer"].Value);
                                layerDescriptors.Add(new LayerDescriptor(swapped, hasInterlayer));
                            }
                        }
                    }
                }
            }
            // create solution
            PackPalletSolution sol = new PackPalletSolution(null, stitle, layer as BoxLayer);
            foreach (LayerDescriptor desc in layerDescriptors)
                sol.AddLayer(desc.Swapped, desc.HasInterlayer);
            return sol;
        }

        private CylinderPalletSolution LoadCylinderPalletSolution(XmlElement eltSolution)
        {
            // title -> instantiation
            string stitle = eltSolution.Attributes["Title"].Value;
            CylinderPalletSolution sol = new CylinderPalletSolution(null, stitle, true);
            // layer
            if (eltSolution.HasAttribute("LimitReached"))
            {
                string sLimitReached = eltSolution.Attributes["LimitReached"].Value;
                sol.LimitReached = (Limit)(int.Parse(sLimitReached));
            }
            // layers
            XmlElement eltLayers = eltSolution.ChildNodes[0] as XmlElement;
            foreach (XmlNode nodeLayer in eltLayers.ChildNodes)
                sol.Add(LoadLayer(nodeLayer as XmlElement));
            return sol;           
        }

        private HCylinderPalletSolution LoadHCylinderPalletSolution(XmlElement eltSolution)
        {
            // title -> instantiation
            string stitle = eltSolution.Attributes["Title"].Value;
            HCylinderPalletSolution sol = new HCylinderPalletSolution(null, stitle);
            // limit reached
            if (eltSolution.HasAttribute("LimitReached"))
            {
                string sLimitReached = eltSolution.Attributes["LimitReached"].Value;
                sol.LimitReached = (Limit)(int.Parse(sLimitReached));
            }
            // cyl positions
            XmlElement eltPositions = eltSolution.ChildNodes[0] as XmlElement;
            foreach (XmlNode nodeCylPos in eltPositions.ChildNodes)
                sol.Add(LoadCylPosition(nodeCylPos as XmlElement));
            return sol;
        }

        private CylPosition LoadCylPosition(XmlElement eltCylPosition)
        {
            string sPosition = eltCylPosition.Attributes["Position"].Value;
            string sAxisDir = eltCylPosition.Attributes["AxisDir"].Value;

            return new CylPosition( Vector3D.Parse(sPosition), HalfAxis.Parse(sAxisDir));
        }

        private BoxCaseSolution LoadBoxCaseSolution(XmlElement eltSolution)
        {
            // pattern
            string patternName = eltSolution.Attributes["Pattern"].Value;
            // orientation
            HalfAxis.HAxis orthoAxis = HalfAxis.Parse(eltSolution.Attributes["OrthoAxis"].Value);
            // instantiate box case solution
            BoxCaseSolution sol = new BoxCaseSolution(null, orthoAxis, patternName);
            // limit reached
            if (eltSolution.HasAttribute("LimitReached"))
            {
                string sLimitReached = eltSolution.Attributes["LimitReached"].Value;
                sol.LimitReached = (BoxCaseSolution.Limit)(int.Parse(sLimitReached));
            }
            // layers
            XmlElement eltLayers = eltSolution.ChildNodes[0] as XmlElement;
            foreach (XmlNode nodeLayer in eltLayers.ChildNodes)
            {
                BoxLayer boxLayer = LoadLayer(nodeLayer as XmlElement) as BoxLayer;
                sol.Add(boxLayer);
            }
            return sol;
        }
        private BoxCasePalletSolution LoadBoxCasePalletSolution(XmlElement eltSolution, BoxCasePalletAnalysis analysis)
        {
            // title
            string stitle = eltSolution.Attributes["Title"].Value;
            // guid
            Guid guid = new Guid(eltSolution.Attributes["PalletSolutionId"].Value);
            // homogeneousLayers
            bool homogeneousLayers = string.Equals(eltSolution.Attributes["HomogeneousLayers"].Value, "true", StringComparison.CurrentCultureIgnoreCase);
            // instantiation
            BoxCasePalletSolution sol = new BoxCasePalletSolution(analysis, stitle, analysis.GetPalletSolutionDescByGuid(guid), homogeneousLayers);
            // layers
            XmlElement eltLayers = eltSolution.ChildNodes[0] as XmlElement;
            foreach (XmlNode nodeLayer in eltLayers.ChildNodes)
                sol.Add(LoadLayer(nodeLayer as XmlElement));
            return sol;
        }
        private ILayer LoadLayer(XmlElement eltLayer)
        {
            ILayer layer = null;
            double zLow = UnitsManager.ConvertLengthFrom(
                Convert.ToDouble(eltLayer.Attributes["ZLow"].Value, System.Globalization.CultureInfo.InvariantCulture)
                , _unitSystem);
            double maxSpace = 0.0;
            if (eltLayer.HasAttribute("MaximumSpace"))
                maxSpace = UnitsManager.ConvertLengthFrom(
                    Convert.ToDouble(eltLayer.Attributes["MaximumSpace"].Value, System.Globalization.CultureInfo.InvariantCulture)
                    , _unitSystem);
            string patternName = string.Empty;
            if (eltLayer.HasAttribute("PatternName"))
                patternName = eltLayer.Attributes["PatternName"].Value;
            if (string.Equals(eltLayer.Name, "BoxLayer", StringComparison.CurrentCultureIgnoreCase))
            {
                BoxLayer boxLayer = new BoxLayer(UnitsManager.ConvertLengthFrom(zLow, _unitSystem), 0);
                boxLayer.MaximumSpace = maxSpace;
                foreach (XmlNode nodeBoxPosition in eltLayer.ChildNodes)
                {
                    XmlElement eltBoxPosition = nodeBoxPosition as XmlElement;
                    string sPosition = eltBoxPosition.Attributes["Position"].Value;
                    string sAxisLength = eltBoxPosition.Attributes["AxisLength"].Value;
                    string sAxisWidth = eltBoxPosition.Attributes["AxisWidth"].Value;
                    try
                    {
                        boxLayer.AddPosition(UnitsManager.ConvertLengthFrom(Vector3D.Parse(sPosition), _unitSystem), HalfAxis.Parse(sAxisLength), HalfAxis.Parse(sAxisWidth));
                    }
                    catch (Exception /*ex*/)
                    {
                        _log.Error(string.Format("Exception thrown: Position = {0} | AxisLength = {1} | AxisWidth = {2}",
                            sPosition, sAxisLength, sAxisWidth ));
                    }
                }
                layer = boxLayer;
            }
            else if (string.Equals(eltLayer.Name, "CylLayer", StringComparison.CurrentCultureIgnoreCase))
            {
                CylinderLayer cylLayer = new CylinderLayer(UnitsManager.ConvertLengthFrom(zLow, _unitSystem));
                foreach (XmlNode nodePosition in eltLayer.ChildNodes)
                {
                    XmlElement eltBoxPosition = nodePosition as XmlElement;
                    string sPosition = eltBoxPosition.Attributes["Position"].Value;
                    cylLayer.Add(UnitsManager.ConvertLengthFrom(Vector3D.Parse(sPosition), _unitSystem));
                    layer = cylLayer;
                }
            }
            else if (string.Equals(eltLayer.Name, "InterLayer", StringComparison.CurrentCultureIgnoreCase))
            {
                int typeId = 0;
                if (eltLayer.HasAttribute("TypeId"))
                    typeId = Convert.ToInt32(eltLayer.Attributes["TypeId"].Value);
                layer = new InterlayerPos(UnitsManager.ConvertLengthFrom(zLow, _unitSystem), typeId);
            }

            return layer;
        }
        #endregion

        #region TruckAnalysis
        private TruckAnalysis LoadTruckAnalysis(XmlElement eltTruckAnalysis, SelCasePalletSolution selSolution)
        {
            string sName = eltTruckAnalysis.Attributes["Name"].Value;
            string sDescription = eltTruckAnalysis.Attributes["Description"].Value;
            string sTruckId = eltTruckAnalysis.Attributes["TruckId"].Value;

            TruckConstraintSet constraintSet = new TruckConstraintSet();
            List<TruckSolution> solutions = new List<TruckSolution>();
            List<int> selectedIndices = new List<int>();

            foreach (XmlNode node in eltTruckAnalysis.ChildNodes)
            { 
                // load constraint set
                if (string.Equals(node.Name, "ConstraintSet", StringComparison.CurrentCultureIgnoreCase))
                    constraintSet = LoadTruckConstraintSet(node as XmlElement);
                // load solutions
                else if (string.Equals(node.Name, "Solutions", StringComparison.CurrentCultureIgnoreCase))
                {
                    int indexSol = 0;
                    foreach (XmlNode solutionNode in node.ChildNodes)
                    {
                        XmlElement eltSolution = solutionNode as XmlElement;
                        solutions.Add(LoadTruckSolution(eltSolution));
                        // is solution selected ?
                        if (null != eltSolution.Attributes["Selected"]
                            && string.Equals("true", eltSolution.Attributes["Selected"].Value, StringComparison.CurrentCultureIgnoreCase))
                            selectedIndices.Add(indexSol);
                        ++indexSol;
                    }
                }
            }

            TruckAnalysis truckAnalysis = selSolution.CreateNewTruckAnalysis(
                sName
                , sDescription
                , GetTypeByGuid(new Guid(sTruckId)) as TruckProperties
                , constraintSet
                , solutions);
            foreach (int index in selectedIndices)
                truckAnalysis.SelectedSolutionIndex = index;
            return truckAnalysis;
        }

        private TruckConstraintSet LoadTruckConstraintSet(XmlElement eltTruckConstraintSet)
        {
            TruckConstraintSet constraintSet = new TruckConstraintSet();
            // multi layer allowed
            if (eltTruckConstraintSet.HasAttribute("MultilayerAllowed"))
                constraintSet.MultilayerAllowed = string.Equals(eltTruckConstraintSet.Attributes["MultilayerAllowed"].Value, "true", StringComparison.CurrentCultureIgnoreCase);
            if (eltTruckConstraintSet.HasAttribute("MinDistancePalletWall"))
            constraintSet.MinDistancePalletTruckWall = UnitsManager.ConvertLengthFrom(double.Parse(eltTruckConstraintSet.Attributes["MinDistancePalletWall"].Value), _unitSystem);
            if (eltTruckConstraintSet.HasAttribute("MinDistancePalletRoof"))
                constraintSet.MinDistancePalletTruckRoof = UnitsManager.ConvertLengthFrom(double.Parse(eltTruckConstraintSet.Attributes["MinDistancePalletRoof"].Value), _unitSystem);
            if (eltTruckConstraintSet.HasAttribute("AllowedPalletOrientations"))
            {
                string sAllowedPalletOrientations = eltTruckConstraintSet.Attributes["AllowedPalletOrientations"].Value;
                constraintSet.AllowPalletOrientationX = sAllowedPalletOrientations.Contains("X");
                constraintSet.AllowPalletOrientationY = sAllowedPalletOrientations.Contains("Y");
            }
            return constraintSet;
        }

        private TruckSolution LoadTruckSolution(XmlElement eltTruckSolution)
        {
            // title -> instantiation
            string stitle = string.Empty;
            if (eltTruckSolution.HasAttribute("Title"))
                stitle = eltTruckSolution.Attributes["Title"].Value;
            TruckSolution sol = new TruckSolution(stitle, null);
            // load only one BoxLayer (actually Pallet layer)
            XmlElement eltLayers = eltTruckSolution.ChildNodes[0] as XmlElement;
            XmlElement eltLayer = eltLayers.ChildNodes[0] as XmlElement;
            sol.Layer = LoadLayer(eltLayer) as BoxLayer;
            return sol;
        }
        #endregion // Load truck analysis

        #region Load ECT analysis
        private ECTAnalysis LoadECTAnalysis(XmlElement eltEctAnalysis, SelCasePalletSolution selSolution)
        {
            string name = eltEctAnalysis.Attributes["Name"].Value;
            string description = eltEctAnalysis.Attributes["Description"].Value;
            ECTAnalysis ectAnalysis = selSolution.CreateNewECTAnalysis(name, description);
            // Cardboard
            foreach (XmlNode node in eltEctAnalysis.ChildNodes)
            {
                if (string.Equals(node.Name, "Cardboard", StringComparison.CurrentCultureIgnoreCase))
                {
                    string cardboardName = string.Empty, profile = string.Empty;
                    double thickness = 0.0, ect = 0.0, stiffnessX = 0.0, stiffnessY = 0.0;

                    XmlElement eltCardboard = node as XmlElement;
                    if (eltCardboard.HasAttribute("Name"))
                        cardboardName = eltCardboard.Attributes["Name"].Value;
                    if (eltCardboard.HasAttribute("Thickness"))
                        thickness = UnitsManager.ConvertLengthFrom(double.Parse(eltCardboard.Attributes["Thickness"].Value), _unitSystem);
                    if (eltCardboard.HasAttribute("ECT"))
                        ect = double.Parse(eltCardboard.Attributes["ECT"].Value);
                    if (eltCardboard.HasAttribute("StiffnessX"))
                        stiffnessX = double.Parse(eltCardboard.Attributes["StiffnessX"].Value);
                    if (eltCardboard.HasAttribute("StiffnessY"))
                        stiffnessY = double.Parse(eltCardboard.Attributes["StiffnessY"].Value);
                    ectAnalysis.Cardboard = new EdgeCrushTest.McKeeFormula.QualityData(name, profile, thickness, ect, stiffnessX, stiffnessY);
                }                
            }
            // CaseType
            if (eltEctAnalysis.HasAttribute("CaseType"))
                ectAnalysis.CaseType = eltEctAnalysis.Attributes["CaseType"].Value;
            // PrintSurface
            if (eltEctAnalysis.HasAttribute("PrintSurface"))
                ectAnalysis.PrintSurface = eltEctAnalysis.Attributes["PrintSurface"].Value;
            // McKeeFormulaMode
            if (eltEctAnalysis.HasAttribute("McKeeFormulaMode"))
                ectAnalysis.McKeeFormulaText = eltEctAnalysis.Attributes["McKeeFormulaMode"].Value;
            return ectAnalysis;
        }
        #endregion // load ECT analysis
        #endregion // load methods

        #region Save methods
        public void Write(string filePath)
        {
            try
            {
                // instantiate XmlDocument
                XmlDocument xmlDoc = new XmlDocument();
                // let's add the XML declaration section
                XmlNode xmlnode = xmlDoc.CreateNode(XmlNodeType.XmlDeclaration, "", "");
                xmlDoc.AppendChild(xmlnode);
                // create Document (root) element
                XmlElement xmlRootElement = xmlDoc.CreateElement("Document");
                xmlDoc.AppendChild(xmlRootElement);
                // name
                XmlAttribute xmlDocNameAttribute = xmlDoc.CreateAttribute("Name");
                xmlDocNameAttribute.Value = _name;
                xmlRootElement.Attributes.Append(xmlDocNameAttribute);
                // description
                XmlAttribute xmlDocDescAttribute = xmlDoc.CreateAttribute("Description");
                xmlDocDescAttribute.Value = _description;
                xmlRootElement.Attributes.Append(xmlDocDescAttribute);
                // author
                XmlAttribute xmlDocAuthorAttribute = xmlDoc.CreateAttribute("Author");
                xmlDocAuthorAttribute.Value = _author;
                xmlRootElement.Attributes.Append(xmlDocAuthorAttribute);
                // dateCreated
                XmlAttribute xmlDateCreatedAttribute = xmlDoc.CreateAttribute("DateCreated");
                xmlDateCreatedAttribute.Value = Convert.ToString(_dateCreated, new CultureInfo("en-US"));
                xmlRootElement.Attributes.Append(xmlDateCreatedAttribute);
                // unit system
                XmlAttribute xmlUnitSystem = xmlDoc.CreateAttribute("UnitSystem");
                xmlUnitSystem.Value = string.Format("{0}", (int)UnitsManager.CurrentUnitSystem);
                xmlRootElement.Attributes.Append(xmlUnitSystem);
                // create ItemProperties element
                XmlElement xmlItemPropertiesElt = xmlDoc.CreateElement("ItemProperties");
                xmlRootElement.AppendChild(xmlItemPropertiesElt);
                foreach (ItemBase itemProperties in _typeList)
                {
                    CaseOfBoxesProperties caseOfBoxesProperties = itemProperties as CaseOfBoxesProperties;
                    if (null != caseOfBoxesProperties)
                        Save(caseOfBoxesProperties, xmlItemPropertiesElt, xmlDoc);
                    BoxProperties boxProperties = itemProperties as BoxProperties;
                    if (null != boxProperties && null == caseOfBoxesProperties)
                        Save(boxProperties, xmlItemPropertiesElt, xmlDoc);
                    BundleProperties bundleProperties = itemProperties as BundleProperties;
                    if (null != bundleProperties)
                        Save(bundleProperties, xmlItemPropertiesElt, xmlDoc);
                    CylinderProperties cylinderProperties = itemProperties as CylinderProperties;
                    if (null != cylinderProperties)
                        Save(cylinderProperties, xmlItemPropertiesElt, xmlDoc); 
                    PalletProperties palletProperties = itemProperties as PalletProperties;
                    if (null != palletProperties)
                        Save(palletProperties, xmlItemPropertiesElt, xmlDoc);
                    InterlayerProperties interlayerProperties = itemProperties as InterlayerProperties;
                    if (null != interlayerProperties)
                        Save(interlayerProperties, xmlItemPropertiesElt, xmlDoc);
                    PalletCornerProperties cornerProperties = itemProperties as PalletCornerProperties;
                    if (null != cornerProperties)
                        Save(cornerProperties, xmlItemPropertiesElt, xmlDoc);
                    PalletCapProperties capProperties = itemProperties as PalletCapProperties;
                    if (null != capProperties)
                        Save(capProperties, xmlItemPropertiesElt, xmlDoc);
                    PalletFilmProperties filmProperties = itemProperties as PalletFilmProperties;
                    if (null != filmProperties)
                        Save(filmProperties, xmlItemPropertiesElt, xmlDoc);
                    TruckProperties truckProperties = itemProperties as TruckProperties;
                    if (null != truckProperties)
                        Save(truckProperties, xmlItemPropertiesElt, xmlDoc);
                    PackProperties packProperties = itemProperties as PackProperties;
                    if (null != packProperties) {}
                }
                foreach (ItemBase itemProperties in _typeList)
                {
                    PackProperties packProperties = itemProperties as PackProperties;
                    if (null != packProperties)
                        Save(packProperties, xmlItemPropertiesElt, xmlDoc);
                }

                // create Analyses element
                XmlElement xmlAnalysesElt = xmlDoc.CreateElement("Analyses");
                xmlRootElement.AppendChild(xmlAnalysesElt);
                foreach (CasePalletAnalysis analysis in _casePalletAnalyses)
                    SavePalletAnalysis(analysis, xmlAnalysesElt, xmlDoc);
                foreach (PackPalletAnalysis analysis in _packPalletAnalyses)
                    SavePackPalletAnalysis(analysis, xmlAnalysesElt, xmlDoc);
                foreach (CylinderPalletAnalysis analysis in _cylinderPalletAnalyses)
                    SaveCylinderPalletAnalysis(analysis, xmlAnalysesElt, xmlDoc);
                foreach (HCylinderPalletAnalysis analysis in _hCylinderPalletAnalyses)
                    SaveHCylinderPalletAnalysis(analysis, xmlAnalysesElt, xmlDoc);
                foreach (BoxCaseAnalysis analysis in _boxCaseAnalyses)
                    SaveBoxCaseAnalysis(analysis, xmlAnalysesElt, xmlDoc);
                foreach (BoxCasePalletAnalysis analysis in _boxCasePalletOptimizations)
                    SaveCaseAnalysis(analysis, xmlAnalysesElt, xmlDoc);


                // finally save XmlDocument
                xmlDoc.Save(filePath);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
            }
        }
        public void WriteSolution(SelCasePalletSolution selSolution, string filePath)
        {
            try
            {
                // retrieve solution
                CasePalletSolution sol = selSolution.Solution;
                // retrieve analysis
                CasePalletAnalysis analysis = sol.Analysis;
                // instantiate XmlDocument
                XmlDocument xmlDoc = new XmlDocument();
                // let's add the XML declaration section
                XmlNode xmlnode = xmlDoc.CreateNode(XmlNodeType.XmlDeclaration, "", "");
                xmlDoc.AppendChild(xmlnode);
                // create Document (root) element
                XmlElement xmlRootElement = xmlDoc.CreateElement("Document");
                xmlDoc.AppendChild(xmlRootElement);
                // name
                XmlAttribute xmlDocNameAttribute = xmlDoc.CreateAttribute("Name");
                xmlDocNameAttribute.Value = _name;
                xmlRootElement.Attributes.Append(xmlDocNameAttribute);
                // description
                XmlAttribute xmlDocDescAttribute = xmlDoc.CreateAttribute("Description");
                xmlDocDescAttribute.Value = _description;
                xmlRootElement.Attributes.Append(xmlDocDescAttribute);
                // author
                XmlAttribute xmlDocAuthorAttribute = xmlDoc.CreateAttribute("Author");
                xmlDocAuthorAttribute.Value = _author;
                xmlRootElement.Attributes.Append(xmlDocAuthorAttribute);
                // dateCreated
                XmlAttribute xmlDateCreatedAttribute = xmlDoc.CreateAttribute("DateCreated");
                xmlDateCreatedAttribute.Value = string.Format("{0}", _dateCreated);
                xmlRootElement.Attributes.Append(xmlDateCreatedAttribute);
                // create ItemProperties element
                XmlElement xmlItemPropertiesElt = xmlDoc.CreateElement("ItemProperties");
                xmlRootElement.AppendChild(xmlItemPropertiesElt);

                BoxProperties boxProperties = sol.Analysis.BProperties as BoxProperties;
                if (null != boxProperties)
                    Save(boxProperties, xmlItemPropertiesElt, xmlDoc);
                BundleProperties bundleProperties = sol.Analysis.BProperties as BundleProperties;
                if (null != bundleProperties)
                    Save(bundleProperties, xmlItemPropertiesElt, xmlDoc);
                PalletProperties palletProperties = sol.Analysis.PalletProperties as PalletProperties;
                if (null != palletProperties)
                    Save(palletProperties, xmlItemPropertiesElt, xmlDoc);
                InterlayerProperties interlayerProperties = sol.Analysis.InterlayerProperties as InterlayerProperties;
                if (null != interlayerProperties)
                    Save(interlayerProperties, xmlItemPropertiesElt, xmlDoc);

                if (null != selSolution && selSolution.TruckAnalyses.Count > 0)
                {
                    TruckProperties truckProperties = selSolution.TruckAnalyses[0].TruckProperties;
                    if (null != truckProperties)
                        Save(truckProperties, xmlItemPropertiesElt, xmlDoc);
                }
                // create Analyses element
                XmlElement xmlAnalysesElt = xmlDoc.CreateElement("Analyses");
                xmlRootElement.AppendChild(xmlAnalysesElt);
                SaveCasePalletAnalysis(sol.Analysis, sol, selSolution, xmlAnalysesElt, xmlDoc);
                // finally save XmlDocument
                xmlDoc.Save(filePath);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                throw ex;
            }
        }         
        public void Save(BoxProperties boxProperties, XmlElement parentElement, XmlDocument xmlDoc)
        {
            // create xmlBoxProperties element
            XmlElement xmlBoxProperties = xmlDoc.CreateElement("BoxProperties");
            parentElement.AppendChild(xmlBoxProperties);
            // Id
            XmlAttribute guidAttribute = xmlDoc.CreateAttribute("Id");
            guidAttribute.Value = boxProperties.Guid.ToString();
            xmlBoxProperties.Attributes.Append(guidAttribute);
            // name
            XmlAttribute nameAttribute = xmlDoc.CreateAttribute("Name");
            nameAttribute.Value = boxProperties.Name;
            xmlBoxProperties.Attributes.Append(nameAttribute);
            // description
            XmlAttribute descAttribute = xmlDoc.CreateAttribute("Description");
            descAttribute.Value = boxProperties.Description;
            xmlBoxProperties.Attributes.Append(descAttribute);
            // length
            XmlAttribute lengthAttribute = xmlDoc.CreateAttribute("Length");
            lengthAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", boxProperties.Length);
            xmlBoxProperties.Attributes.Append(lengthAttribute);
            // width
            XmlAttribute widthAttribute = xmlDoc.CreateAttribute("Width");
            widthAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", boxProperties.Width);
            xmlBoxProperties.Attributes.Append(widthAttribute);
            // height
            XmlAttribute heightAttribute = xmlDoc.CreateAttribute("Height");
            heightAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", boxProperties.Height);
            xmlBoxProperties.Attributes.Append(heightAttribute);
            // inside dimensions
            if (boxProperties.HasInsideDimensions)
            {
                // length
                XmlAttribute insideLengthAttribute = xmlDoc.CreateAttribute("InsideLength");
                insideLengthAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", boxProperties.InsideLength);
                xmlBoxProperties.Attributes.Append(insideLengthAttribute);
                // width
                XmlAttribute insideWidthAttribute = xmlDoc.CreateAttribute("InsideWidth");
                insideWidthAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", boxProperties.InsideWidth);
                xmlBoxProperties.Attributes.Append(insideWidthAttribute);
                // height
                XmlAttribute insideHeightAttribute = xmlDoc.CreateAttribute("InsideHeight");
                insideHeightAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", boxProperties.InsideHeight);
                xmlBoxProperties.Attributes.Append(insideHeightAttribute);
            }
            // weight
            XmlAttribute weightAttribute = xmlDoc.CreateAttribute("Weight");
            weightAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", boxProperties.Weight);
            xmlBoxProperties.Attributes.Append(weightAttribute);
            // net weight
            XmlAttribute netWeightAttribute = xmlDoc.CreateAttribute("NetWeight");
            netWeightAttribute.Value = boxProperties.NetWeight.ToString();
            xmlBoxProperties.Attributes.Append(netWeightAttribute);
            // colors
            SaveColors(boxProperties.Colors, xmlBoxProperties, xmlDoc);
            // texture
            SaveTextures(boxProperties.TextureList, xmlBoxProperties, xmlDoc);
            // tape
            XmlAttribute tapeAttribute = xmlDoc.CreateAttribute("ShowTape");
            tapeAttribute.Value = string.Format("{0}", boxProperties.ShowTape);
            xmlBoxProperties.Attributes.Append(tapeAttribute);
            if (boxProperties.ShowTape)
            {
                XmlElement tapeElt = xmlDoc.CreateElement("Tape");
                xmlBoxProperties.AppendChild(tapeElt);

                XmlAttribute tapeWidthAttribute = xmlDoc.CreateAttribute("TapeWidth");
                tapeWidthAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", boxProperties.TapeWidth);
                tapeElt.Attributes.Append(tapeWidthAttribute);

                XmlAttribute tapeColorAttribute = xmlDoc.CreateAttribute("TapeColor");
                tapeColorAttribute.Value = string.Format("{0}", boxProperties.TapeColor.ToArgb());
                tapeElt.Attributes.Append(tapeColorAttribute);
            }
        }
        public void Save(PackProperties packProperties, XmlElement parentElement, XmlDocument xmlDoc)
        {
            // create xmlPackProperties element
            XmlElement xmlPackProperties = xmlDoc.CreateElement("PackProperties");
            parentElement.AppendChild(xmlPackProperties);
            // Id
            XmlAttribute guidAttribute = xmlDoc.CreateAttribute("Id");
            guidAttribute.Value = packProperties.Guid.ToString();
            xmlPackProperties.Attributes.Append(guidAttribute);
            // name
            XmlAttribute nameAttribute = xmlDoc.CreateAttribute("Name");
            nameAttribute.Value = packProperties.Name;
            xmlPackProperties.Attributes.Append(nameAttribute);
            // description
            XmlAttribute descAttribute = xmlDoc.CreateAttribute("Description");
            descAttribute.Value = packProperties.Description;
            xmlPackProperties.Attributes.Append(descAttribute);
            // boxProperties
            XmlAttribute boxPropAttribute = xmlDoc.CreateAttribute("BoxProperties");
            boxPropAttribute.Value = packProperties.Box.Guid.ToString();
            xmlPackProperties.Attributes.Append(boxPropAttribute);
            // box orientation
            XmlAttribute orientationAttribute = xmlDoc.CreateAttribute("Orientation");
            orientationAttribute.Value = HalfAxis.ToString( packProperties.BoxOrientation );
            xmlPackProperties.Attributes.Append(orientationAttribute);
            // arrangement
            XmlAttribute arrAttribute = xmlDoc.CreateAttribute("Arrangement");
            arrAttribute.Value = packProperties.Arrangement.ToString();
            xmlPackProperties.Attributes.Append(arrAttribute);
            // wrapper
            XmlElement wrapperElt = xmlDoc.CreateElement("Wrapper");
            xmlPackProperties.AppendChild(wrapperElt);

            PackWrapper packWrapper = packProperties.Wrap;
            SaveWrapper(packWrapper as WrapperPolyethilene, wrapperElt, xmlDoc);
            SaveWrapper(packWrapper as WrapperPaper, wrapperElt, xmlDoc);
            SaveWrapper(packWrapper as WrapperCardboard, wrapperElt, xmlDoc);

            // outer dimensions
            if (packProperties.HasForcedOuterDimensions)
            {
                XmlAttribute outerDimAttribute = xmlDoc.CreateAttribute("OuterDimensions");
                outerDimAttribute.Value = packProperties.OuterDimensions.ToString();
                xmlPackProperties.Attributes.Append(outerDimAttribute);
            }
        }

        #region Save Wrappers
        private void SaveWrapperBase(PackWrapper wrapper, XmlElement wrapperElt, XmlDocument xmlDoc)
        {
            if (null == wrapper) return;
            // type
            XmlAttribute typeAttrib = xmlDoc.CreateAttribute("Type");
            typeAttrib.Value = wrapper.Type.ToString();
            wrapperElt.Attributes.Append(typeAttrib);
            // color
            XmlAttribute colorAttrib = xmlDoc.CreateAttribute("Color");
            colorAttrib.Value = string.Format("{0}", wrapper.Color.ToArgb());
            wrapperElt.Attributes.Append(colorAttrib);
            // weight
            XmlAttribute weightAttrib = xmlDoc.CreateAttribute("Weight");
            weightAttrib.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", wrapper.Weight);
            wrapperElt.Attributes.Append(weightAttrib);
            // thickness
            XmlAttribute thicknessAttrib = xmlDoc.CreateAttribute("UnitThickness");
            thicknessAttrib.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", wrapper.UnitThickness);
            wrapperElt.Attributes.Append(thicknessAttrib);
        }
        private void SaveWrapper(WrapperPolyethilene wrapper, XmlElement wrapperElt, XmlDocument xmlDoc)
        {
            if (null == wrapper) return;
            SaveWrapperBase(wrapper, wrapperElt, xmlDoc);
            // transparency
            XmlAttribute transparentAttrib = xmlDoc.CreateAttribute("Transparent");
            transparentAttrib.Value = wrapper.Transparent.ToString();
            wrapperElt.Attributes.Append(transparentAttrib);
        }
        private void SaveWrapper(WrapperPaper wrapper, XmlElement wrapperElt, XmlDocument xmlDoc)
        {
            if (null == wrapper) return;
            SaveWrapperBase(wrapper, wrapperElt, xmlDoc);
        }
        private void SaveWrapper(WrapperCardboard wrapper, XmlElement wrapperElt, XmlDocument xmlDoc)
        {
            if (null == wrapper) return;
           SaveWrapperBase(wrapper, wrapperElt, xmlDoc);
           // wall distribution
           XmlAttribute wallDistribAttrib = xmlDoc.CreateAttribute("NumberOfWalls");
           wallDistribAttrib.Value = string.Format("{0} {1} {2}", wrapper.Wall(0), wrapper.Wall(1), wrapper.Wall(2));
           wrapperElt.Attributes.Append(wallDistribAttrib);
           // tray specific
           WrapperTray wrapperTray = wrapper as WrapperTray;
           if (null != wrapperTray)
           {
               XmlAttribute heightAttrib = xmlDoc.CreateAttribute("Height");
               heightAttrib.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", wrapperTray.Height);
               wrapperElt.Attributes.Append(heightAttrib);
           }
        }
        #endregion

        public void Save(CylinderProperties cylinderProperties, XmlElement parentElement, XmlDocument xmlDoc)
        {
            // create xmlBoxProperties element
            XmlElement xmlBoxProperties = xmlDoc.CreateElement("CylinderProperties");
            parentElement.AppendChild(xmlBoxProperties);
            // Id
            XmlAttribute guidAttribute = xmlDoc.CreateAttribute("Id");
            guidAttribute.Value = cylinderProperties.Guid.ToString();
            xmlBoxProperties.Attributes.Append(guidAttribute);
            // name
            XmlAttribute nameAttribute = xmlDoc.CreateAttribute("Name");
            nameAttribute.Value = cylinderProperties.Name;
            xmlBoxProperties.Attributes.Append(nameAttribute);
            // description
            XmlAttribute descAttribute = xmlDoc.CreateAttribute("Description");
            descAttribute.Value = cylinderProperties.Description;
            xmlBoxProperties.Attributes.Append(descAttribute);
            // radius outer
            XmlAttribute radiusOuterAttribute = xmlDoc.CreateAttribute("RadiusOuter");
            radiusOuterAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", cylinderProperties.RadiusOuter);
            xmlBoxProperties.Attributes.Append(radiusOuterAttribute);
            // radius inner
            XmlAttribute radiusInnerAttribute = xmlDoc.CreateAttribute("RadiusInner");
            radiusInnerAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", cylinderProperties.RadiusInner);
            xmlBoxProperties.Attributes.Append(radiusInnerAttribute);
            // height
            XmlAttribute heightAttribute = xmlDoc.CreateAttribute("Height");
            heightAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", cylinderProperties.Height);
            xmlBoxProperties.Attributes.Append(heightAttribute);
            // weight
            XmlAttribute weightAttribute = xmlDoc.CreateAttribute("Weight");
            weightAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", cylinderProperties.Weight);
            xmlBoxProperties.Attributes.Append(weightAttribute);
            // colorTop
            XmlAttribute topAttribute = xmlDoc.CreateAttribute("ColorTop");
            topAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", cylinderProperties.ColorTop.ToArgb());
            xmlBoxProperties.Attributes.Append(topAttribute);
            // colorWall
            XmlAttribute outerWallAttribute = xmlDoc.CreateAttribute("ColorWallOuter");
            outerWallAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", cylinderProperties.ColorWallOuter.ToArgb());
            xmlBoxProperties.Attributes.Append(outerWallAttribute);
            // color inner wall
            XmlAttribute innerWallAttribute = xmlDoc.CreateAttribute("ColorWallInner");
            innerWallAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", cylinderProperties.ColorWallInner.ToArgb());
            xmlBoxProperties.Attributes.Append(innerWallAttribute);
        }

        public void Save(CaseOfBoxesProperties caseOfBoxesProperties, XmlElement parentElement, XmlDocument xmlDoc)
        {
            // create xmlBoxProperties element
            XmlElement xmlBoxProperties = xmlDoc.CreateElement("CaseOfBoxesProperties");
            parentElement.AppendChild(xmlBoxProperties);
            // Id
            XmlAttribute guidAttribute = xmlDoc.CreateAttribute("Id");
            guidAttribute.Value = caseOfBoxesProperties.Guid.ToString();
            xmlBoxProperties.Attributes.Append(guidAttribute);
            // name
            XmlAttribute nameAttribute = xmlDoc.CreateAttribute("Name");
            nameAttribute.Value = caseOfBoxesProperties.Name;
            xmlBoxProperties.Attributes.Append(nameAttribute);
            // description
            XmlAttribute descAttribute = xmlDoc.CreateAttribute("Description");
            descAttribute.Value = caseOfBoxesProperties.Description;
            xmlBoxProperties.Attributes.Append(descAttribute);
            // weight
            XmlAttribute weightAttribute = xmlDoc.CreateAttribute("Weight");
            weightAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", caseOfBoxesProperties.Weight);
            xmlBoxProperties.Attributes.Append(weightAttribute);
            // save inside ref to box properties
            XmlAttribute insideBoxId = xmlDoc.CreateAttribute("InsideBoxId");
            insideBoxId.Value = caseOfBoxesProperties.InsideBoxProperties.Guid.ToString();
            xmlBoxProperties.Attributes.Append(insideBoxId);
            // save case definition
            SaveCaseDefinition(caseOfBoxesProperties.CaseDefinition, xmlBoxProperties, xmlDoc);
            // save optim constraintset
            SaveCaseOptimConstraintSet(caseOfBoxesProperties.CaseOptimConstraintSet, xmlBoxProperties, xmlDoc);
            // colors
            SaveColors(caseOfBoxesProperties.Colors, xmlBoxProperties, xmlDoc);
            // texture
            SaveTextures(caseOfBoxesProperties.TextureList, xmlBoxProperties, xmlDoc);
        }
        private void SaveCaseDefinition(CaseDefinition caseDefinition, XmlElement xmlBoxProperties, XmlDocument xmlDoc)
        {
            XmlElement xmlCaseDefElement = xmlDoc.CreateElement("CaseDefinition");
            xmlBoxProperties.AppendChild(xmlCaseDefElement);
            // case arrangement
            XmlAttribute xmlArrangement = xmlDoc.CreateAttribute("Arrangement");
            xmlArrangement.Value = caseDefinition.Arrangement.ToString();
            xmlCaseDefElement.Attributes.Append(xmlArrangement);
            // box orientation
            XmlAttribute xmlOrientation = xmlDoc.CreateAttribute("Orientation");
            xmlOrientation.Value = string.Format("{0} {1}", caseDefinition.Dim0, caseDefinition.Dim1);
            xmlCaseDefElement.Attributes.Append(xmlOrientation);
        }
        private void SaveCaseOptimConstraintSet(CaseOptimConstraintSet caseOptimConstraintSet, XmlElement xmlBoxProperties, XmlDocument xmlDoc)
        {
            XmlElement xmlCaseOptimConstraintSet = xmlDoc.CreateElement("OptimConstraintSet");
            xmlBoxProperties.AppendChild(xmlCaseOptimConstraintSet);
            // wall thickness
            XmlAttribute xmlWallThickness = xmlDoc.CreateAttribute("WallThickness");
            xmlWallThickness.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", caseOptimConstraintSet.WallThickness);
            xmlCaseOptimConstraintSet.Attributes.Append(xmlWallThickness);
            // wall surface mass
            XmlAttribute xmlWallSurfaceMass = xmlDoc.CreateAttribute("WallSurfaceMass");
            xmlWallSurfaceMass.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", caseOptimConstraintSet.WallSurfaceMass);
            xmlCaseOptimConstraintSet.Attributes.Append(xmlWallSurfaceMass);
            // no walls
            XmlAttribute xmlNumberOfWalls = xmlDoc.CreateAttribute("NumberOfWalls");
            xmlNumberOfWalls.Value = string.Format("{0} {1} {2}"
                , caseOptimConstraintSet.NoWalls[0]
                , caseOptimConstraintSet.NoWalls[1]
                , caseOptimConstraintSet.NoWalls[2]);
            xmlCaseOptimConstraintSet.Attributes.Append(xmlNumberOfWalls);
        }
        private void SaveTextures(List<Pair<HalfAxis.HAxis, Texture>> textureList, XmlElement xmlBoxProperties, XmlDocument xmlDoc)
        { 
            XmlElement xmlTexturesElement = xmlDoc.CreateElement("Textures");
            xmlBoxProperties.AppendChild(xmlTexturesElement);
            foreach (Pair<HalfAxis.HAxis, Texture> texPair in textureList)
            {
                XmlElement xmlFaceTexture = xmlDoc.CreateElement("FaceTexture");
                xmlTexturesElement.AppendChild(xmlFaceTexture);
                // face index
                XmlAttribute xmlFaceNormal = xmlDoc.CreateAttribute("FaceNormal");
                xmlFaceNormal.Value = HalfAxis.ToString(texPair.first);
                xmlFaceTexture.Attributes.Append(xmlFaceNormal);
                // texture position
                XmlAttribute xmlPosition = xmlDoc.CreateAttribute("Position");
                xmlPosition.Value = texPair.second.Position.ToString();
                xmlFaceTexture.Attributes.Append(xmlPosition);
                // texture size
                XmlAttribute xmlSize = xmlDoc.CreateAttribute("Size");
                xmlSize.Value = texPair.second.Size.ToString();
                xmlFaceTexture.Attributes.Append(xmlSize);
                // angle
                XmlAttribute xmlAngle = xmlDoc.CreateAttribute("Angle");
                xmlAngle.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", texPair.second.Angle);
                xmlFaceTexture.Attributes.Append(xmlAngle);
                // bitmap
                XmlAttribute xmlBitmap = xmlDoc.CreateAttribute("Bitmap");
                xmlBitmap.Value = Document.BitmapToString(texPair.second.Bitmap);
                xmlFaceTexture.Attributes.Append(xmlBitmap);
            }
        }

        private void SaveColors(Color[] colors, XmlElement eltBoxProperties, XmlDocument xmlDoc)
        { 
            // face colors
            XmlElement xmlFaceColors = xmlDoc.CreateElement("FaceColors");
            eltBoxProperties.AppendChild(xmlFaceColors);
            short i = 0;
            foreach (Color color in colors)
            {
                XmlElement xmlFaceColor = xmlDoc.CreateElement("FaceColor");
                xmlFaceColors.AppendChild(xmlFaceColor);
                // face index
                XmlAttribute xmlFaceIndex = xmlDoc.CreateAttribute("FaceIndex");
                xmlFaceIndex.Value = string.Format("{0}", i);
                xmlFaceColor.Attributes.Append(xmlFaceIndex);
                // color
                XmlAttribute xmlColor = xmlDoc.CreateAttribute("Color");
                xmlColor.Value = string.Format("{0}", color.ToArgb());
                xmlFaceColor.Attributes.Append(xmlColor);
                ++i;
            }
        }

        public void Save(PalletProperties palletProperties, XmlElement parentElement, XmlDocument xmlDoc)
        {
            // create xmlPalletProperties element
            XmlElement xmlPalletProperties = xmlDoc.CreateElement("PalletProperties");
            parentElement.AppendChild(xmlPalletProperties);
            // Id
            XmlAttribute guidAttribute = xmlDoc.CreateAttribute("Id");
            guidAttribute.Value = palletProperties.Guid.ToString();
            xmlPalletProperties.Attributes.Append(guidAttribute);
            // name
            XmlAttribute nameAttribute = xmlDoc.CreateAttribute("Name");
            nameAttribute.Value = palletProperties.Name;
            xmlPalletProperties.Attributes.Append(nameAttribute);
            // description
            XmlAttribute descAttribute = xmlDoc.CreateAttribute("Description");
            descAttribute.Value = palletProperties.Description;
            xmlPalletProperties.Attributes.Append(descAttribute);
            // length
            XmlAttribute lengthAttribute = xmlDoc.CreateAttribute("Length");
            lengthAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", palletProperties.Length);
            xmlPalletProperties.Attributes.Append(lengthAttribute);
            // width
            XmlAttribute widthAttribute = xmlDoc.CreateAttribute("Width");
            widthAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", palletProperties.Width);
            xmlPalletProperties.Attributes.Append(widthAttribute);
            // height
            XmlAttribute heightAttribute = xmlDoc.CreateAttribute("Height");
            heightAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", palletProperties.Height);
            xmlPalletProperties.Attributes.Append(heightAttribute);
            // weight
            XmlAttribute weightAttribute = xmlDoc.CreateAttribute("Weight");
            weightAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", palletProperties.Weight);
            xmlPalletProperties.Attributes.Append(weightAttribute);
            // admissible load weight
            XmlAttribute admLoadWeightAttribute = xmlDoc.CreateAttribute("AdmissibleLoadWeight");
            admLoadWeightAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", palletProperties.AdmissibleLoadWeight);
            xmlPalletProperties.Attributes.Append(admLoadWeightAttribute);
            // admissible load height
            XmlAttribute admLoadHeightAttribute = xmlDoc.CreateAttribute("AdmissibleLoadHeight");
            admLoadHeightAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", palletProperties.AdmissibleLoadHeight);
            xmlPalletProperties.Attributes.Append(admLoadHeightAttribute);
            // type
            XmlAttribute typeAttribute = xmlDoc.CreateAttribute("Type");
            typeAttribute.Value = string.Format("{0}", palletProperties.TypeName);
            xmlPalletProperties.Attributes.Append(typeAttribute);
            // color
            XmlAttribute colorAttribute = xmlDoc.CreateAttribute("Color");
            colorAttribute.Value = string.Format("{0}", palletProperties.Color.ToArgb());
            xmlPalletProperties.Attributes.Append(colorAttribute);
        }
        public void Save(InterlayerProperties interlayerProperties, XmlElement parentElement, XmlDocument xmlDoc)
        {
            // create xmlPalletProperties element
            XmlElement xmlInterlayerProperties = xmlDoc.CreateElement("InterlayerProperties");
            parentElement.AppendChild(xmlInterlayerProperties);
            // Id
            XmlAttribute guidAttribute = xmlDoc.CreateAttribute("Id");
            guidAttribute.Value = interlayerProperties.Guid.ToString();
            xmlInterlayerProperties.Attributes.Append(guidAttribute);
            // name
            XmlAttribute nameAttribute = xmlDoc.CreateAttribute("Name");
            nameAttribute.Value = interlayerProperties.Name;
            xmlInterlayerProperties.Attributes.Append(nameAttribute);
            // description
            XmlAttribute descAttribute = xmlDoc.CreateAttribute("Description");
            descAttribute.Value = interlayerProperties.Description;
            xmlInterlayerProperties.Attributes.Append(descAttribute);
            // length
            XmlAttribute lengthAttribute = xmlDoc.CreateAttribute("Length");
            lengthAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", interlayerProperties.Length);
            xmlInterlayerProperties.Attributes.Append(lengthAttribute);
            // width
            XmlAttribute widthAttribute = xmlDoc.CreateAttribute("Width");
            widthAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", interlayerProperties.Width);
            xmlInterlayerProperties.Attributes.Append(widthAttribute);
            // height
            XmlAttribute heightAttribute = xmlDoc.CreateAttribute("Thickness");
            heightAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", interlayerProperties.Thickness);
            xmlInterlayerProperties.Attributes.Append(heightAttribute);
            // weight
            XmlAttribute weightAttribute = xmlDoc.CreateAttribute("Weight");
            weightAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", interlayerProperties.Weight);
            xmlInterlayerProperties.Attributes.Append(weightAttribute);
            // color
            XmlAttribute colorAttribute = xmlDoc.CreateAttribute("Color");
            colorAttribute.Value = string.Format("{0}", interlayerProperties.Color.ToArgb());
            xmlInterlayerProperties.Attributes.Append(colorAttribute);
        }

        public void Save(PalletCornerProperties cornerProperties, XmlElement parentElement, XmlDocument xmlDoc)
        {
            // create PalletCornerProperties element
            XmlElement xmlCornerProperties = xmlDoc.CreateElement("PalletCornerProperties");
            parentElement.AppendChild(xmlCornerProperties);
            // Id
            XmlAttribute guidAttribute = xmlDoc.CreateAttribute("Id");
            guidAttribute.Value = cornerProperties.Guid.ToString();
            xmlCornerProperties.Attributes.Append(guidAttribute);
            // name
            XmlAttribute nameAttribute = xmlDoc.CreateAttribute("Name");
            nameAttribute.Value = cornerProperties.Name;
            xmlCornerProperties.Attributes.Append(nameAttribute);
            // description
            XmlAttribute descAttribute = xmlDoc.CreateAttribute("Description");
            descAttribute.Value = cornerProperties.Description;
            xmlCornerProperties.Attributes.Append(descAttribute);
            // length
            XmlAttribute lengthAttribute = xmlDoc.CreateAttribute("Length");
            lengthAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", cornerProperties.Length);
            xmlCornerProperties.Attributes.Append(lengthAttribute);
            // width
            XmlAttribute widthAttribute = xmlDoc.CreateAttribute("Width");
            widthAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", cornerProperties.Width);
            xmlCornerProperties.Attributes.Append(widthAttribute);
            // height
            XmlAttribute heightAttribute = xmlDoc.CreateAttribute("Thickness");
            heightAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", cornerProperties.Thickness);
            xmlCornerProperties.Attributes.Append(heightAttribute);
            // weight
            XmlAttribute weightAttribute = xmlDoc.CreateAttribute("Weight");
            weightAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", cornerProperties.Weight);
            xmlCornerProperties.Attributes.Append(weightAttribute);
            // color
            XmlAttribute colorAttribute = xmlDoc.CreateAttribute("Color");
            colorAttribute.Value = string.Format("{0}", cornerProperties.Color.ToArgb());
            xmlCornerProperties.Attributes.Append(colorAttribute);
        }

        public void Save(PalletCapProperties capProperties, XmlElement parentElement, XmlDocument xmlDoc)
        {
            // create PalletCornerProperties element
            XmlElement xmlCapProperties = xmlDoc.CreateElement("PalletCapProperties");
            parentElement.AppendChild(xmlCapProperties);
            // Id
            XmlAttribute guidAttribute = xmlDoc.CreateAttribute("Id");
            guidAttribute.Value = capProperties.Guid.ToString();
            xmlCapProperties.Attributes.Append(guidAttribute);
            // name
            XmlAttribute nameAttribute = xmlDoc.CreateAttribute("Name");
            nameAttribute.Value = capProperties.Name;
            xmlCapProperties.Attributes.Append(nameAttribute);
            // description
            XmlAttribute descAttribute = xmlDoc.CreateAttribute("Description");
            descAttribute.Value = capProperties.Description;
            xmlCapProperties.Attributes.Append(descAttribute);
            // length
            XmlAttribute lengthAttribute = xmlDoc.CreateAttribute("Length");
            lengthAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", capProperties.Length);
            xmlCapProperties.Attributes.Append(lengthAttribute);
            // width
            XmlAttribute widthAttribute = xmlDoc.CreateAttribute("Width");
            widthAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", capProperties.Width);
            xmlCapProperties.Attributes.Append(widthAttribute);
            // height
            XmlAttribute heightAttribute = xmlDoc.CreateAttribute("Height");
            heightAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", capProperties.Height);
            xmlCapProperties.Attributes.Append(heightAttribute);
            // inside length
            XmlAttribute insideLengthAttribute = xmlDoc.CreateAttribute("InsideLength");
            insideLengthAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", capProperties.Length);
            xmlCapProperties.Attributes.Append(insideLengthAttribute);
            // inside width
            XmlAttribute insideWidthAttribute = xmlDoc.CreateAttribute("InsideWidth");
            insideWidthAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", capProperties.Width);
            xmlCapProperties.Attributes.Append(insideWidthAttribute);
            // inside height
            XmlAttribute insideHeightAttribute = xmlDoc.CreateAttribute("InsideHeight");
            insideHeightAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", capProperties.Height);
            xmlCapProperties.Attributes.Append(insideHeightAttribute);
            // weight
            XmlAttribute weightAttribute = xmlDoc.CreateAttribute("Weight");
            weightAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", capProperties.Weight);
            xmlCapProperties.Attributes.Append(weightAttribute);
            // color
            XmlAttribute colorAttribute = xmlDoc.CreateAttribute("Color");
            colorAttribute.Value = string.Format("{0}", capProperties.Color.ToArgb());
            xmlCapProperties.Attributes.Append(colorAttribute); 
        }

        public void Save(PalletFilmProperties filmProperties, XmlElement parentElement, XmlDocument xmlDoc)
        {
            // create PalletFilmProperties element
            XmlElement xmlFilmProperties = xmlDoc.CreateElement("PalletFilmProperties");
            parentElement.AppendChild(xmlFilmProperties);
            // Id
            XmlAttribute guidAttribute = xmlDoc.CreateAttribute("Id");
            guidAttribute.Value = filmProperties.Guid.ToString();
            xmlFilmProperties.Attributes.Append(guidAttribute);
            // Name
            XmlAttribute nameAttribute = xmlDoc.CreateAttribute("Name");
            nameAttribute.Value = filmProperties.Name;
            xmlFilmProperties.Attributes.Append(nameAttribute);
            // Description
            XmlAttribute descAttribute = xmlDoc.CreateAttribute("Description");
            descAttribute.Value = filmProperties.Description;
            xmlFilmProperties.Attributes.Append(descAttribute);
            // Transparency
            XmlAttribute transparencyAttribute = xmlDoc.CreateAttribute("Transparency");
            transparencyAttribute.Value = filmProperties.UseTransparency.ToString();
            xmlFilmProperties.Attributes.Append(transparencyAttribute);
            // Hatching
            XmlAttribute hatchingAttribute = xmlDoc.CreateAttribute("Hatching");
            hatchingAttribute.Value = filmProperties.UseHatching.ToString();
            xmlFilmProperties.Attributes.Append(hatchingAttribute);
            // HatchSpacing
            XmlAttribute hatchSpacingAttribute = xmlDoc.CreateAttribute("HatchSpacing");
            hatchSpacingAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", filmProperties.HatchSpacing);
            xmlFilmProperties.Attributes.Append(hatchSpacingAttribute);
            // HatchAngle
            XmlAttribute hatchAngleAttribute = xmlDoc.CreateAttribute("HatchAngle");
            hatchAngleAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", filmProperties.HatchAngle);
            xmlFilmProperties.Attributes.Append(hatchAngleAttribute);
            // Color
            XmlAttribute colorAttribute = xmlDoc.CreateAttribute("Color");
            colorAttribute.Value = string.Format("{0}", filmProperties.Color.ToArgb());
            xmlFilmProperties.Attributes.Append(colorAttribute); 
        }

        public void Save(BundleProperties bundleProperties, XmlElement parentElement, XmlDocument xmlDoc)
        {
            // create xmlPalletProperties element
            XmlElement xmlBundleProperties = xmlDoc.CreateElement("BundleProperties");
            parentElement.AppendChild(xmlBundleProperties);
            // Id
            XmlAttribute guidAttribute = xmlDoc.CreateAttribute("Id");
            guidAttribute.Value = bundleProperties.Guid.ToString();
            xmlBundleProperties.Attributes.Append(guidAttribute);
            // name
            XmlAttribute nameAttribute = xmlDoc.CreateAttribute("Name");
            nameAttribute.Value = bundleProperties.Name;
            xmlBundleProperties.Attributes.Append(nameAttribute);
            // description
            XmlAttribute descAttribute = xmlDoc.CreateAttribute("Description");
            descAttribute.Value = bundleProperties.Description;
            xmlBundleProperties.Attributes.Append(descAttribute);
            // length
            XmlAttribute lengthAttribute = xmlDoc.CreateAttribute("Length");
            lengthAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", bundleProperties.Length);
            xmlBundleProperties.Attributes.Append(lengthAttribute);
            // width
            XmlAttribute widthAttribute = xmlDoc.CreateAttribute("Width");
            widthAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", bundleProperties.Width);
            xmlBundleProperties.Attributes.Append(widthAttribute);
            // height
            XmlAttribute heightAttribute = xmlDoc.CreateAttribute("UnitThickness");
            heightAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", bundleProperties.UnitThickness);
            xmlBundleProperties.Attributes.Append(heightAttribute);
            // weight
            XmlAttribute weightAttribute = xmlDoc.CreateAttribute("UnitWeight");
            weightAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", bundleProperties.UnitWeight);
            xmlBundleProperties.Attributes.Append(weightAttribute);
            // color
            XmlAttribute colorAttribute = xmlDoc.CreateAttribute("Color");
            colorAttribute.Value = string.Format("{0}", bundleProperties.Color.ToArgb());
            xmlBundleProperties.Attributes.Append(colorAttribute);
            // numberFlats
            XmlAttribute numberFlatsAttribute = xmlDoc.CreateAttribute("NumberFlats");
            numberFlatsAttribute.Value = string.Format("{0}", bundleProperties.NoFlats);
            xmlBundleProperties.Attributes.Append(numberFlatsAttribute);
        }

        public void Save(TruckProperties truckProperties, XmlElement parentElement, XmlDocument xmlDoc)
        {
            // create xmlPalletProperties element
            XmlElement xmlTruckProperties = xmlDoc.CreateElement("TruckProperties");
            parentElement.AppendChild(xmlTruckProperties);
            // Id
            XmlAttribute guidAttribute = xmlDoc.CreateAttribute("Id");
            guidAttribute.Value = truckProperties.Guid.ToString();
            xmlTruckProperties.Attributes.Append(guidAttribute);
            // name
            XmlAttribute nameAttribute = xmlDoc.CreateAttribute("Name");
            nameAttribute.Value = truckProperties.Name;
            xmlTruckProperties.Attributes.Append(nameAttribute);
            // description
            XmlAttribute descAttribute = xmlDoc.CreateAttribute("Description");
            descAttribute.Value = truckProperties.Description;
            xmlTruckProperties.Attributes.Append(descAttribute);
            // length
            XmlAttribute lengthAttribute = xmlDoc.CreateAttribute("Length");
            lengthAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", truckProperties.Length);
            xmlTruckProperties.Attributes.Append(lengthAttribute);
            // width
            XmlAttribute widthAttribute = xmlDoc.CreateAttribute("Width");
            widthAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", truckProperties.Width);
            xmlTruckProperties.Attributes.Append(widthAttribute);
            // height
            XmlAttribute heightAttribute = xmlDoc.CreateAttribute("Height");
            heightAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", truckProperties.Height);
            xmlTruckProperties.Attributes.Append(heightAttribute);
            // weight
            XmlAttribute weightAttribute = xmlDoc.CreateAttribute("AdmissibleLoadWeight");
            weightAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", truckProperties.AdmissibleLoadWeight);
            xmlTruckProperties.Attributes.Append(weightAttribute);
            // color
            XmlAttribute colorAttribute = xmlDoc.CreateAttribute("Color");
            colorAttribute.Value = string.Format("{0}", truckProperties.Color.ToArgb());
            xmlTruckProperties.Attributes.Append(colorAttribute);
        }
        private void SaveCaseAnalysis(BoxCasePalletAnalysis analysis, XmlElement parentElement, XmlDocument xmlDoc)
        { 
            // create analysis element
            XmlElement xmlAnalysisElt = xmlDoc.CreateElement("AnalysisCase");
            parentElement.AppendChild(xmlAnalysisElt);
            // Name
            XmlAttribute analysisNameAttribute = xmlDoc.CreateAttribute("Name");
            analysisNameAttribute.Value = analysis.Name;
            xmlAnalysisElt.Attributes.Append(analysisNameAttribute);
            // Description
            XmlAttribute analysisDescriptionAttribute = xmlDoc.CreateAttribute("Description");
            analysisDescriptionAttribute.Value = analysis.Description;
            xmlAnalysisElt.Attributes.Append(analysisDescriptionAttribute);
            // BoxId
            XmlAttribute boxIdAttribute = xmlDoc.CreateAttribute("BoxId");
            boxIdAttribute.Value = string.Format("{0}", analysis.BoxProperties.Guid);
            xmlAnalysisElt.Attributes.Append(boxIdAttribute);
            // ConstraintSet : beg
            XmlElement constraintSetElement = xmlDoc.CreateElement("ConstraintSetCase");
            xmlAnalysisElt.AppendChild(constraintSetElement);
            XmlAttribute alignedLayersAttribute = xmlDoc.CreateAttribute("AlignedLayersAllowed");
            alignedLayersAttribute.Value = string.Format("{0}", analysis.ConstraintSet.AllowAlignedLayers);
            constraintSetElement.Attributes.Append(alignedLayersAttribute);
            XmlAttribute alternateLayersAttribute = xmlDoc.CreateAttribute("AlternateLayersAllowed");
            alternateLayersAttribute.Value = string.Format("{0}", analysis.ConstraintSet.AllowAlternateLayers);
            constraintSetElement.Attributes.Append(alternateLayersAttribute);
            // allowed box positions
            XmlAttribute allowedAxisAttribute = xmlDoc.CreateAttribute("AllowedBoxPositions");
            allowedAxisAttribute.Value = analysis.ConstraintSet.AllowOrthoAxisString;
            constraintSetElement.Attributes.Append(allowedAxisAttribute);
            // allowed layer patterns
            XmlAttribute allowedPatternAttribute = xmlDoc.CreateAttribute("AllowedPatterns");
            allowedPatternAttribute.Value = analysis.ConstraintSet.AllowedPatternString;
            constraintSetElement.Attributes.Append(allowedPatternAttribute);
            // stop criterions
            if (analysis.ConstraintSet.UseMaximumCaseWeight)
            {
                XmlAttribute maximumWeightAttribute = xmlDoc.CreateAttribute("MaximumCaseWeight");
                maximumWeightAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", analysis.ConstraintSet.MaximumCaseWeight);
                constraintSetElement.Attributes.Append(maximumWeightAttribute);
            }
            if (analysis.ConstraintSet.UseMaximumNumberOfItems)
            {
                XmlAttribute maximumNumberOfItems = xmlDoc.CreateAttribute("ManimumNumberOfItems");
                maximumNumberOfItems.Value = string.Format("{0}", analysis.ConstraintSet.MaximumNumberOfItems);
                constraintSetElement.Attributes.Append(maximumNumberOfItems);
            }
            // solution filtering
            if (analysis.ConstraintSet.UseMinimumNumberOfItems)
            {
                XmlAttribute minimumNumberOfItems = xmlDoc.CreateAttribute("MinimumNumberOfItems");
                minimumNumberOfItems.Value = string.Format("{0}", analysis.ConstraintSet.MinimumNumberOfItems);
                constraintSetElement.Attributes.Append(minimumNumberOfItems);
            }
            // number of solutions to keep
            if (analysis.ConstraintSet.UseNumberOfSolutionsKept)
            {
                XmlAttribute numberOfSolutionsKept = xmlDoc.CreateAttribute("NumberOfSolutions");
                numberOfSolutionsKept.Value = string.Format("{0}", analysis.ConstraintSet.NumberOfSolutionsKept);
                constraintSetElement.Attributes.Append(numberOfSolutionsKept);
            }
            // ConstraintSet : end

            // Pallet solution descriptors
            XmlElement palletSolutionsElement = xmlDoc.CreateElement("PalletSolutionDescriptors");
            xmlAnalysisElt.AppendChild(palletSolutionsElement);
            foreach (PalletSolutionDesc desc in analysis.PalletSolutionsList)
                SavePalletSolutionDescriptor(desc, palletSolutionsElement, xmlDoc);
            // Solutions
            XmlElement solutionsElt = xmlDoc.CreateElement("CaseSolutions");
            xmlAnalysisElt.AppendChild(solutionsElt);
            int solIndex = 0;
            foreach (BoxCasePalletSolution caseSolution in analysis.Solutions)
                SaveCaseSolution(caseSolution, analysis.GetSelCaseSolutionBySolutionIndex(solIndex++), solutionsElt, xmlDoc);
        }

        private void SavePalletSolutionDescriptor(PalletSolutionDesc desc, XmlElement parentElement, XmlDocument xmlDoc)
        {
            // pallet solution descriptor element
            XmlElement palletSolutionDescElt = xmlDoc.CreateElement("PalletSolutionDescriptor");
            parentElement.AppendChild(palletSolutionDescElt);
            // pallet dimensions
            XmlAttribute palletDimensionsAttribute = xmlDoc.CreateAttribute("PalletDimensions");
            palletDimensionsAttribute.Value = desc.Key.PalletDimensions;
            palletSolutionDescElt.Attributes.Append(palletDimensionsAttribute);
            // overhang
            XmlAttribute palletOverhangAttribute = xmlDoc.CreateAttribute("PalletOverhang");
            palletOverhangAttribute.Value = desc.Key.Overhang;
            palletSolutionDescElt.Attributes.Append(palletOverhangAttribute);
            // guid
            XmlAttribute idAttribute = xmlDoc.CreateAttribute("Id");
            idAttribute.Value = desc.Guid.ToString();
            palletSolutionDescElt.Attributes.Append(idAttribute);
            // friendly name
            XmlAttribute friendlyNameAttribute = xmlDoc.CreateAttribute("FriendlyName");
            friendlyNameAttribute.Value = desc.FriendlyName;
            palletSolutionDescElt.Attributes.Append(friendlyNameAttribute);
            // case dimensions
            XmlAttribute dimensionsAttribute = xmlDoc.CreateAttribute("CaseDimensions");
            dimensionsAttribute.Value = desc.CaseDimensionsString;
            palletSolutionDescElt.Attributes.Append(dimensionsAttribute);
            // case inside dimensions
            XmlAttribute insideDimensionsAttribute = xmlDoc.CreateAttribute("CaseInsideDimensions");
            insideDimensionsAttribute.Value = desc.CaseInsideDimensionsString;
            palletSolutionDescElt.Attributes.Append(insideDimensionsAttribute);
            // case weight
            XmlAttribute caseWeightAttribute = xmlDoc.CreateAttribute("CaseWeight");
            caseWeightAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", desc.CaseWeight);
            palletSolutionDescElt.Attributes.Append(caseWeightAttribute);
            // pallet weight
            XmlAttribute palletWeightAttribute = xmlDoc.CreateAttribute("PalletWeight");
            palletWeightAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", desc.PalletWeight);
            palletSolutionDescElt.Attributes.Append(palletWeightAttribute);
            // case count
            XmlAttribute caseCountAttribute = xmlDoc.CreateAttribute("CaseCount");
            caseCountAttribute.Value = desc.CaseCount.ToString();
            palletSolutionDescElt.Attributes.Append(caseCountAttribute);
            // case orientation
            XmlAttribute caseOrientationAttribute = xmlDoc.CreateAttribute("CaseOrientation");
            caseOrientationAttribute.Value = desc.CaseOrientation;
            palletSolutionDescElt.Attributes.Append(caseOrientationAttribute);
        }

        private void SaveCaseSolution(BoxCasePalletSolution sol, SelBoxCasePalletSolution selSolution, XmlElement parentElement, XmlDocument xmlDoc)
        {
            // create case solution element
            XmlElement solutionElt = xmlDoc.CreateElement("CaseSolution");
            parentElement.AppendChild(solutionElt);
            // title
            XmlAttribute titleAttribute = xmlDoc.CreateAttribute("Title");
            titleAttribute.Value = sol.Title;
            solutionElt.Attributes.Append(titleAttribute);
            // homogeneousLayers ?
            XmlAttribute homogeneousLayersAttribute = xmlDoc.CreateAttribute("HomogeneousLayers");
            homogeneousLayersAttribute.Value = sol.HasHomogeneousLayers ? "true" : "false";
            solutionElt.Attributes.Append(homogeneousLayersAttribute);
            // pallet solution id
            XmlAttribute palletSolutionAttribute = xmlDoc.CreateAttribute("PalletSolutionId");
            palletSolutionAttribute.Value = sol.PalletSolutionDesc.Guid.ToString();
            solutionElt.Attributes.Append(palletSolutionAttribute);
            // layers
            XmlElement layersElt = xmlDoc.CreateElement("Layers");
            solutionElt.AppendChild(layersElt);

            foreach (ILayer layer in sol)
            {
                BoxLayer boxLayer = layer as BoxLayer;
                if (null != boxLayer)
                    Save(boxLayer, layersElt, xmlDoc);

                InterlayerPos interlayerPos = layer as InterlayerPos;
                if (null != interlayerPos)
                {
                    // Interlayer
                    XmlElement interlayerElt = xmlDoc.CreateElement("Interlayer");
                    layersElt.AppendChild(interlayerElt);
                    // ZLow
                    XmlAttribute zlowAttribute = xmlDoc.CreateAttribute("ZLow");
                    zlowAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", interlayerPos.ZLow);
                    interlayerElt.Attributes.Append(zlowAttribute);
                }
            }

            // Is selected ?
            if (null != selSolution)
            {
                // selected attribute
                XmlAttribute selAttribute = xmlDoc.CreateAttribute("Selected");
                selAttribute.Value = "true";
                solutionElt.Attributes.Append(selAttribute);
            }
        }

        private void SavePalletAnalysis(CasePalletAnalysis analysis, XmlElement parentElement, XmlDocument xmlDoc)
        {
            // create analysis element
            XmlElement xmlAnalysisElt = xmlDoc.CreateElement("AnalysisPallet");
            parentElement.AppendChild(xmlAnalysisElt);
            // Name
            XmlAttribute analysisNameAttribute = xmlDoc.CreateAttribute("Name");
            analysisNameAttribute.Value = analysis.Name;
            xmlAnalysisElt.Attributes.Append(analysisNameAttribute);
            // Description
            XmlAttribute analysisDescriptionAttribute = xmlDoc.CreateAttribute("Description");
            analysisDescriptionAttribute.Value = analysis.Description;
            xmlAnalysisElt.Attributes.Append(analysisDescriptionAttribute);
            // BoxId
            XmlAttribute boxIdAttribute = xmlDoc.CreateAttribute("BoxId");
            boxIdAttribute.Value = string.Format("{0}", analysis.BProperties.Guid);
            xmlAnalysisElt.Attributes.Append(boxIdAttribute);
            // PalletId
            XmlAttribute palletIdAttribute = xmlDoc.CreateAttribute("PalletId");
            palletIdAttribute.Value = string.Format("{0}", analysis.PalletProperties.Guid);
            xmlAnalysisElt.Attributes.Append(palletIdAttribute);
            // InterlayerId
            if (null != analysis.InterlayerProperties)
            {
                XmlAttribute interlayerIdAttribute = xmlDoc.CreateAttribute("InterlayerId");
                interlayerIdAttribute.Value = string.Format("{0}", analysis.InterlayerProperties.Guid);
                xmlAnalysisElt.Attributes.Append(interlayerIdAttribute);
            }
            // InterlayerAntiSlipId
            if (null != analysis.InterlayerPropertiesAntiSlip)
            {
                XmlAttribute interlayerIdAttribute = xmlDoc.CreateAttribute("InterlayerAntiSlipId");
                interlayerIdAttribute.Value = string.Format("{0}", analysis.InterlayerPropertiesAntiSlip.Guid);
                xmlAnalysisElt.Attributes.Append(interlayerIdAttribute);
            }
            // PalletCornerId
            if (null != analysis.PalletCornerProperties)
            {
                XmlAttribute palletCornerAttribute = xmlDoc.CreateAttribute("PalletCornerId");
                palletCornerAttribute.Value = string.Format("{0}", analysis.PalletCornerProperties.Guid);
                xmlAnalysisElt.Attributes.Append(palletCornerAttribute);
            }
            // PalletCapId
            if (null != analysis.PalletCapProperties)
            {
                XmlAttribute palletCapIdAttribute = xmlDoc.CreateAttribute("PalletCapId");
                palletCapIdAttribute.Value = string.Format("{0}", analysis.PalletCapProperties.Guid);
                xmlAnalysisElt.Attributes.Append(palletCapIdAttribute);
            }
            // PalletFilmId
            if (null != analysis.PalletFilmProperties)
            {
                XmlAttribute palletFilmIdAttribute = xmlDoc.CreateAttribute("PalletFilmId");
                palletFilmIdAttribute.Value = string.Format("{0}", analysis.PalletFilmProperties.Guid);
                xmlAnalysisElt.Attributes.Append(palletFilmIdAttribute);
            }
            // ###
            // ConstraintSet
            bool bundleAnalysis = (analysis.ConstraintSet.GetType() == typeof(BundlePalletConstraintSet));
            XmlElement constraintSetElement = xmlDoc.CreateElement(bundleAnalysis ? "ConstraintSetBundle":"ConstraintSetBox");
            XmlAttribute alignedLayersAttribute = xmlDoc.CreateAttribute("AlignedLayersAllowed");
            alignedLayersAttribute.Value = string.Format("{0}", analysis.ConstraintSet.AllowAlignedLayers);
            constraintSetElement.Attributes.Append(alignedLayersAttribute);
            XmlAttribute alternateLayersAttribute = xmlDoc.CreateAttribute("AlternateLayersAllowed");
            alternateLayersAttribute.Value = string.Format("{0}", analysis.ConstraintSet.AllowAlternateLayers);
            constraintSetElement.Attributes.Append(alternateLayersAttribute);
            if (!bundleAnalysis)
            {
                // allowed box positions
                XmlAttribute allowedAxisAttribute = xmlDoc.CreateAttribute("AllowedBoxPositions");
                HalfAxis.HAxis[] axes = { HalfAxis.HAxis.AXIS_X_P, HalfAxis.HAxis.AXIS_Y_P, HalfAxis.HAxis.AXIS_Z_P };
                string allowedAxes = string.Empty;
                foreach (HalfAxis.HAxis axis in axes)
                    if (analysis.ConstraintSet.AllowOrthoAxis(axis))
                    {
                        if (!string.IsNullOrEmpty(allowedAxes))
                            allowedAxes += ",";
                        allowedAxes += HalfAxis.ToString(axis);
                    }
                allowedAxisAttribute.Value = allowedAxes;
                constraintSetElement.Attributes.Append(allowedAxisAttribute);
            }
            // allowed layer patterns
            XmlAttribute allowedPatternAttribute = xmlDoc.CreateAttribute("AllowedPatterns");
            allowedPatternAttribute.Value = analysis.ConstraintSet.AllowedPatternString;
            constraintSetElement.Attributes.Append(allowedPatternAttribute);
            // stop criterions
            if (analysis.ConstraintSet.UseMaximumHeight)
            { 
                XmlAttribute maximumHeightAttribute = xmlDoc.CreateAttribute("MaximumHeight");
                maximumHeightAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", analysis.ConstraintSet.MaximumHeight);
                constraintSetElement.Attributes.Append(maximumHeightAttribute);
            }
            if (analysis.ConstraintSet.UseMaximumNumberOfCases)
            {
                XmlAttribute maximumNumberOfItems = xmlDoc.CreateAttribute("ManimumNumberOfItems");
                maximumNumberOfItems.Value = string.Format("{0}", analysis.ConstraintSet.MaximumNumberOfItems);
                constraintSetElement.Attributes.Append(maximumNumberOfItems);
            }
            if (analysis.ConstraintSet.UseMaximumPalletWeight)
            {
                XmlAttribute maximumPalletWeight = xmlDoc.CreateAttribute("MaximumPalletWeight");
                maximumPalletWeight.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", analysis.ConstraintSet.MaximumPalletWeight);
                constraintSetElement.Attributes.Append(maximumPalletWeight);
            }
            if (analysis.ConstraintSet.UseMaximumWeightOnBox)
            {
                XmlAttribute maximumWeightOnBox = xmlDoc.CreateAttribute("MaximumWeightOnBox");
                maximumWeightOnBox.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", analysis.ConstraintSet.MaximumWeightOnBox);
                constraintSetElement.Attributes.Append(maximumWeightOnBox);
            }
            // overhang / underhang
            XmlAttribute overhangX = xmlDoc.CreateAttribute("OverhangX");
            overhangX.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", analysis.ConstraintSet.OverhangX);
            constraintSetElement.Attributes.Append(overhangX);
            XmlAttribute overhangY = xmlDoc.CreateAttribute("OverhangY");
            overhangY.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", analysis.ConstraintSet.OverhangY);
            constraintSetElement.Attributes.Append(overhangY);
            // number of solutions to keep
            if (analysis.ConstraintSet.UseNumberOfSolutionsKept)
            {
                XmlAttribute numberOfSolutionsKept = xmlDoc.CreateAttribute("NumberOfSolutions");
                numberOfSolutionsKept.Value = string.Format("{0}", analysis.ConstraintSet.NumberOfSolutionsKept);
                constraintSetElement.Attributes.Append(numberOfSolutionsKept);
            }
            // pallet film turns
            if (analysis.HasPalletFilm)
            {
                XmlAttribute palletFilmTurns = xmlDoc.CreateAttribute("PalletFilmTurns");
                palletFilmTurns.Value = string.Format("{0}", analysis.ConstraintSet.PalletFilmTurns);
                constraintSetElement.Attributes.Append(palletFilmTurns);
            }
            xmlAnalysisElt.AppendChild(constraintSetElement);

            // Solutions
            int solIndex = 0;
            XmlElement solutionsElt = xmlDoc.CreateElement("Solutions");
            xmlAnalysisElt.AppendChild(solutionsElt);
            foreach (CasePalletSolution sol in analysis.Solutions)
            {
                SaveCasePalletSolution(
                    analysis
                    , sol
                    , analysis.GetSelSolutionBySolutionIndex(solIndex) // null if not selected
                    , false /*unique*/
                    , solutionsElt
                    , xmlDoc);
                ++solIndex;
            }
        }

        public void SavePackPalletAnalysis(PackPalletAnalysis analysis, XmlElement parentElement, XmlDocument xmlDoc)
        {
            // create analysis element
            XmlElement xmlAnalysisElt = xmlDoc.CreateElement("PackPalletAnalysis");
            parentElement.AppendChild(xmlAnalysisElt);
            // Name
            XmlAttribute analysisNameAttribute = xmlDoc.CreateAttribute("Name");
            analysisNameAttribute.Value = analysis.Name;
            xmlAnalysisElt.Attributes.Append(analysisNameAttribute);
            // Description
            XmlAttribute analysisDescriptionAttribute = xmlDoc.CreateAttribute("Description");
            analysisDescriptionAttribute.Value = analysis.Description;
            xmlAnalysisElt.Attributes.Append(analysisDescriptionAttribute);
            // BoxId
            XmlAttribute packIdAttribute = xmlDoc.CreateAttribute("PackId");
            packIdAttribute.Value = string.Format("{0}", analysis.PackProperties.Guid);
            xmlAnalysisElt.Attributes.Append(packIdAttribute);
            // PalletId
            XmlAttribute palletIdAttribute = xmlDoc.CreateAttribute("PalletId");
            palletIdAttribute.Value = string.Format("{0}", analysis.PalletProperties.Guid);
            xmlAnalysisElt.Attributes.Append(palletIdAttribute);
            // InterlayerId
            if (null != analysis.InterlayerProperties)
            {
                XmlAttribute interlayerIdAttribute = xmlDoc.CreateAttribute("InterlayerId");
                interlayerIdAttribute.Value = string.Format("{0}", analysis.InterlayerProperties.Guid);
                xmlAnalysisElt.Attributes.Append(interlayerIdAttribute);
            }
            // Constraint set
            XmlElement constraintSetElt = xmlDoc.CreateElement("ConstraintSet");
            xmlAnalysisElt.AppendChild(constraintSetElt);
            SaveDouble(analysis.ConstraintSet.OverhangX, xmlDoc, constraintSetElt, "OverhangX");
            SaveDouble(analysis.ConstraintSet.OverhangY, xmlDoc, constraintSetElt, "OverhangY");
            SaveOptDouble(analysis.ConstraintSet.MinOverhangX, xmlDoc, constraintSetElt, "MinOverhangX");
            SaveOptDouble(analysis.ConstraintSet.MinOverhangY, xmlDoc, constraintSetElt, "MinOverhangY");
            SaveOptDouble(analysis.ConstraintSet.MinimumSpace, xmlDoc, constraintSetElt, "MinimumSpace");
            SaveOptDouble(analysis.ConstraintSet.MaximumSpaceAllowed, xmlDoc, constraintSetElt, "MaximumSpaceAllowed");
            SaveOptDouble(analysis.ConstraintSet.MaximumPalletHeight, xmlDoc, constraintSetElt, "MaximumPalletHeight");
            SaveOptDouble(analysis.ConstraintSet.MaximumPalletWeight, xmlDoc, constraintSetElt, "MaximumPalletWeight");
            SaveOptDouble(analysis.ConstraintSet.MaximumLayerWeight, xmlDoc, constraintSetElt, "MaximumLayerWeight");
            SaveInt(analysis.ConstraintSet.LayerSwapPeriod, xmlDoc, constraintSetElt, "LayerSwapPeriod");
            SaveInt(analysis.ConstraintSet.InterlayerPeriod, xmlDoc, constraintSetElt, "InterlayerPeriod");
            // solutions
            XmlElement solutionsElt = xmlDoc.CreateElement("Solutions");
            xmlAnalysisElt.AppendChild(solutionsElt);
            int solIndex = 0;
            foreach (PackPalletSolution sol in analysis.Solutions)
            {
                SavePackPalletSolution(
                    sol
                    , analysis.GetSelSolutionBySolutionIndex(solIndex) // null if not selected
                    , solutionsElt
                    , xmlDoc);
                ++solIndex;
            }
        }
        private void SaveInt(int i, XmlDocument xmlDoc, XmlElement xmlElement, string attributeName)
        {
            XmlAttribute att = xmlDoc.CreateAttribute(attributeName);
            att.Value = i.ToString();
            xmlElement.Attributes.Append(att);
        }

        private void SaveDouble(double d, XmlDocument xmlDoc, XmlElement xmlElement, string attributeName)
        {
            XmlAttribute att = xmlDoc.CreateAttribute(attributeName);
            att.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", d);
            xmlElement.Attributes.Append(att);
        }

        private void SaveOptDouble(OptDouble optD, XmlDocument xmlDoc, XmlElement xmlElement, string attributeName)
        {
            XmlAttribute att = xmlDoc.CreateAttribute(attributeName);
            att.Value = optD.ToString();
            xmlElement.Attributes.Append(att);
        }

        public void SaveCylinderPalletAnalysis(CylinderPalletAnalysis analysis, XmlElement parentElement, XmlDocument xmlDoc)
        { 
            // create analysis element
            XmlElement xmlAnalysisElt = xmlDoc.CreateElement("CylinderPalletAnalysis");
            parentElement.AppendChild(xmlAnalysisElt);
            // Name
            XmlAttribute analysisNameAttribute = xmlDoc.CreateAttribute("Name");
            analysisNameAttribute.Value = analysis.Name;
            xmlAnalysisElt.Attributes.Append(analysisNameAttribute);
            // Description
            XmlAttribute analysisDescriptionAttribute = xmlDoc.CreateAttribute("Description");
            analysisDescriptionAttribute.Value = analysis.Description;
            xmlAnalysisElt.Attributes.Append(analysisDescriptionAttribute);
            // BoxId
            XmlAttribute cylinderIdAttribute = xmlDoc.CreateAttribute("CylinderId");
            cylinderIdAttribute.Value = string.Format("{0}", analysis.CylinderProperties.Guid);
            xmlAnalysisElt.Attributes.Append(cylinderIdAttribute);
            // PalletId
            XmlAttribute palletIdAttribute = xmlDoc.CreateAttribute("PalletId");
            palletIdAttribute.Value = string.Format("{0}", analysis.PalletProperties.Guid);
            xmlAnalysisElt.Attributes.Append(palletIdAttribute);
            // InterlayerId
            if (null != analysis.InterlayerProperties)
            {
                XmlAttribute interlayerIdAttribute = xmlDoc.CreateAttribute("InterlayerId");
                interlayerIdAttribute.Value = string.Format("{0}", analysis.InterlayerProperties.Guid);
                xmlAnalysisElt.Attributes.Append(interlayerIdAttribute);
            }
            if (null != analysis.InterlayerPropertiesAntiSlip)
            {
                XmlAttribute interlayerIdAttribute = xmlDoc.CreateAttribute("InterlayerAntiSlipId");
                interlayerIdAttribute.Value = string.Format("{0}", analysis.InterlayerPropertiesAntiSlip.Guid);
                xmlAnalysisElt.Attributes.Append(interlayerIdAttribute);
            }
            XmlElement constraintSetElement = xmlDoc.CreateElement("ConstraintSet");
            xmlAnalysisElt.AppendChild(constraintSetElement);
            // stop criterions
            if (analysis.ConstraintSet.UseMaximumPalletHeight)
            {
                XmlAttribute maximumHeightAttribute = xmlDoc.CreateAttribute("MaximumHeight");
                maximumHeightAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", analysis.ConstraintSet.MaximumPalletHeight);
                constraintSetElement.Attributes.Append(maximumHeightAttribute);
            }
            if (analysis.ConstraintSet.UseMaximumNumberOfItems)
            {
                XmlAttribute maximumNumberOfItems = xmlDoc.CreateAttribute("ManimumNumberOfItems");
                maximumNumberOfItems.Value = string.Format("{0}", analysis.ConstraintSet.MaximumNumberOfItems);
                constraintSetElement.Attributes.Append(maximumNumberOfItems);
            }
            if (analysis.ConstraintSet.UseMaximumPalletWeight)
            {
                XmlAttribute maximumPalletWeight = xmlDoc.CreateAttribute("MaximumPalletWeight");
                maximumPalletWeight.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", analysis.ConstraintSet.MaximumPalletWeight);
                constraintSetElement.Attributes.Append(maximumPalletWeight);
            }
            if (analysis.ConstraintSet.UseMaximumLoadOnLowerCylinder)
            {
                XmlAttribute maximumWeightOnBox = xmlDoc.CreateAttribute("MaximumLoadOnLowerCylinder");
                maximumWeightOnBox.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", analysis.ConstraintSet.MaximumLoadOnLowerCylinder);
                constraintSetElement.Attributes.Append(maximumWeightOnBox);
            }
            // overhang / underhang
            XmlAttribute overhangX = xmlDoc.CreateAttribute("OverhangX");
            overhangX.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", analysis.ConstraintSet.OverhangX);
            constraintSetElement.Attributes.Append(overhangX);
            XmlAttribute overhangY = xmlDoc.CreateAttribute("OverhangY");
            overhangY.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", analysis.ConstraintSet.OverhangY);
            constraintSetElement.Attributes.Append(overhangY);
            // solutions
            XmlElement solutionsElt = xmlDoc.CreateElement("Solutions");
            xmlAnalysisElt.AppendChild(solutionsElt);
            int solIndex = 0;
            foreach (CylinderPalletSolution sol in analysis.Solutions)
            {
                SaveCylinderPalletSolution(
                    analysis
                    , sol
                    , analysis.GetSelSolutionBySolutionIndex(solIndex) // null if not selected
                    , solutionsElt
                    , xmlDoc);
                ++solIndex;
            }
        }

        public void SaveHCylinderPalletAnalysis(HCylinderPalletAnalysis analysis, XmlElement parentElement, XmlDocument xmlDoc)
        {
            // create analysis element
            XmlElement xmlAnalysisElt = xmlDoc.CreateElement("HCylinderPalletAnalysis");
            parentElement.AppendChild(xmlAnalysisElt);
            // Name
            XmlAttribute analysisNameAttribute = xmlDoc.CreateAttribute("Name");
            analysisNameAttribute.Value = analysis.Name;
            xmlAnalysisElt.Attributes.Append(analysisNameAttribute);
            // Description
            XmlAttribute analysisDescriptionAttribute = xmlDoc.CreateAttribute("Description");
            analysisDescriptionAttribute.Value = analysis.Description;
            xmlAnalysisElt.Attributes.Append(analysisDescriptionAttribute);
            // BoxId
            XmlAttribute cylinderIdAttribute = xmlDoc.CreateAttribute("CylinderId");
            cylinderIdAttribute.Value = string.Format("{0}", analysis.CylinderProperties.Guid);
            xmlAnalysisElt.Attributes.Append(cylinderIdAttribute);
            // PalletId
            XmlAttribute palletIdAttribute = xmlDoc.CreateAttribute("PalletId");
            palletIdAttribute.Value = string.Format("{0}", analysis.PalletProperties.Guid);
            xmlAnalysisElt.Attributes.Append(palletIdAttribute);
            XmlElement constraintSetElement = xmlDoc.CreateElement("ConstraintSet");
            xmlAnalysisElt.AppendChild(constraintSetElement);
            // stop criterions
            if (analysis.ConstraintSet.UseMaximumPalletHeight)
            {
                XmlAttribute maximumHeightAttribute = xmlDoc.CreateAttribute("MaximumHeight");
                maximumHeightAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", analysis.ConstraintSet.MaximumPalletHeight);
                constraintSetElement.Attributes.Append(maximumHeightAttribute);
            }
            if (analysis.ConstraintSet.UseMaximumNumberOfItems)
            {
                XmlAttribute maximumNumberOfItems = xmlDoc.CreateAttribute("ManimumNumberOfItems");
                maximumNumberOfItems.Value = string.Format("{0}", analysis.ConstraintSet.MaximumNumberOfItems);
                constraintSetElement.Attributes.Append(maximumNumberOfItems);
            }
            if (analysis.ConstraintSet.UseMaximumPalletWeight)
            {
                XmlAttribute maximumPalletWeight = xmlDoc.CreateAttribute("MaximumPalletWeight");
                maximumPalletWeight.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", analysis.ConstraintSet.MaximumPalletWeight);
                constraintSetElement.Attributes.Append(maximumPalletWeight);
            }
            // overhang / underhang
            XmlAttribute overhangX = xmlDoc.CreateAttribute("OverhangX");
            overhangX.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", analysis.ConstraintSet.OverhangX);
            constraintSetElement.Attributes.Append(overhangX);
            XmlAttribute overhangY = xmlDoc.CreateAttribute("OverhangY");
            overhangY.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", analysis.ConstraintSet.OverhangY);
            constraintSetElement.Attributes.Append(overhangY);
            // solutions
            int solIndex = 0;
            XmlElement solutionsElt = xmlDoc.CreateElement("Solutions");
            xmlAnalysisElt.AppendChild(solutionsElt);
            foreach (HCylinderPalletSolution sol in analysis.Solutions)
            {
                SaveHCylinderPalletSolution(
                    analysis
                    , sol
                    , analysis.GetSelSolutionBySolutionIndex(solIndex) // null if not selected
                    , solutionsElt
                    , xmlDoc);
                ++solIndex;
            }
        }

        public void SaveBoxCaseAnalysis(BoxCaseAnalysis analysis, XmlElement parentElement, XmlDocument xmlDoc)
        { 
            // create analysis element
            XmlElement xmlAnalysisElt = xmlDoc.CreateElement("AnalysisBoxCase");
            parentElement.AppendChild(xmlAnalysisElt);
            // Name
            XmlAttribute analysisNameAttribute = xmlDoc.CreateAttribute("Name");
            analysisNameAttribute.Value = analysis.Name;
            xmlAnalysisElt.Attributes.Append(analysisNameAttribute);
            // Description
            XmlAttribute analysisDescriptionAttribute = xmlDoc.CreateAttribute("Description");
            analysisDescriptionAttribute.Value = analysis.Description;
            xmlAnalysisElt.Attributes.Append(analysisDescriptionAttribute);
            // BoxId
            XmlAttribute boxIdAttribute = xmlDoc.CreateAttribute("BoxId");
            boxIdAttribute.Value = string.Format("{0}", analysis.BProperties.Guid);
            xmlAnalysisElt.Attributes.Append(boxIdAttribute);
            // PalletId
            XmlAttribute palletIdAttribute = xmlDoc.CreateAttribute("CaseId");
            palletIdAttribute.Value = string.Format("{0}", analysis.CaseProperties.Guid);
            xmlAnalysisElt.Attributes.Append(palletIdAttribute);
            // Constraint set
            SaveBoxCaseConstraintSet(analysis.ConstraintSet, xmlAnalysisElt, xmlDoc);
            // Solutions
            int solIndex = 0;
            XmlElement solutionsElt = xmlDoc.CreateElement("Solutions");
            xmlAnalysisElt.AppendChild(solutionsElt);
            if (null != analysis.Solutions)
                foreach (BoxCaseSolution sol in analysis.Solutions)
                {
                    SaveBoxCaseSolution(
                        analysis
                        , sol
                        , analysis.GetSelSolutionBySolutionIndex(solIndex) // null if not selected
                        , solutionsElt
                        , xmlDoc);
                    ++solIndex;
                }
        }

        public void SaveBoxCaseConstraintSet(BCaseConstraintSet constraintSet, XmlElement xmlAnalysisElt, XmlDocument xmlDoc)
        { 
            // ConstraintSet
            XmlElement constraintSetElement = xmlDoc.CreateElement("ConstraintSetCase");
            xmlAnalysisElt.AppendChild(constraintSetElement);
            BoxCaseConstraintSet boxCaseContraintSet = constraintSet as BoxCaseConstraintSet;
            if (null != boxCaseContraintSet)
            {
                // allowed box positions
                XmlAttribute allowedAxisAttribute = xmlDoc.CreateAttribute("AllowedBoxPositions");
                constraintSetElement.Attributes.Append(allowedAxisAttribute);
                allowedAxisAttribute.Value = boxCaseContraintSet.AllowOrthoAxisString;
            }
            // stop criterions
            // 1. maximum number of boxes
            if (constraintSet.UseMaximumNumberOfBoxes)
            {
                XmlAttribute maximumNumberOfBoxes = xmlDoc.CreateAttribute("ManimumNumberOfBoxes");
                maximumNumberOfBoxes.Value = string.Format("{0}", constraintSet.MaximumNumberOfBoxes);
                constraintSetElement.Attributes.Append(maximumNumberOfBoxes);
            }
            // 2. maximum case weight
            if (constraintSet.UseMaximumCaseWeight)
            {
                XmlAttribute maximumPalletWeight = xmlDoc.CreateAttribute("MaximumCaseWeight");
                maximumPalletWeight.Value = string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "{0}",
                    constraintSet.MaximumCaseWeight);
                constraintSetElement.Attributes.Append(maximumPalletWeight);
            }
            xmlAnalysisElt.AppendChild(constraintSetElement);
        }

        public void SaveBoxCaseSolution(BoxCaseAnalysis analysis, BoxCaseSolution sol, SelBoxCaseSolution selSolution, XmlElement solutionsElt, XmlDocument xmlDoc)
        {
            // Solution
            XmlElement solutionElt = xmlDoc.CreateElement("Solution");
            solutionsElt.AppendChild(solutionElt);
            // Pattern name
            XmlAttribute patternNameAttribute = xmlDoc.CreateAttribute("Pattern");
            patternNameAttribute.Value = sol.PatternName;
            solutionElt.Attributes.Append(patternNameAttribute);
            // ortho axis
            XmlAttribute orthoAxisAttribute = xmlDoc.CreateAttribute("OrthoAxis");
            orthoAxisAttribute.Value = HalfAxis.ToString(sol.OrthoAxis);
            solutionElt.Attributes.Append(orthoAxisAttribute);
            // limit
            XmlAttribute limitReached = xmlDoc.CreateAttribute("LimitReached");
            limitReached.Value = string.Format("{0}", (int)sol.LimitReached);
            solutionElt.Attributes.Append(limitReached);
            // layers
            XmlElement layersElt = xmlDoc.CreateElement("Layers");
            solutionElt.AppendChild(layersElt);

            foreach (BoxLayer boxLayer in sol)
                Save(boxLayer, layersElt, xmlDoc);

            // Is selected ?
            if (null != selSolution)
            {
                // selected attribute
                XmlAttribute selAttribute = xmlDoc.CreateAttribute("Selected");
                selAttribute.Value = "true";
                solutionElt.Attributes.Append(selAttribute);
            }
        }

        public void SaveCasePalletAnalysis(CasePalletAnalysis analysis, CasePalletSolution sol, SelCasePalletSolution selSolution, XmlElement parentElement, XmlDocument xmlDoc)
        {
            // create analysis element
            XmlElement xmlAnalysisElt = xmlDoc.CreateElement("AnalysisPallet");
            parentElement.AppendChild(xmlAnalysisElt);
            // Name
            XmlAttribute analysisNameAttribute = xmlDoc.CreateAttribute("Name");
            analysisNameAttribute.Value = analysis.Name;
            xmlAnalysisElt.Attributes.Append(analysisNameAttribute);
            // Description
            XmlAttribute analysisDescriptionAttribute = xmlDoc.CreateAttribute("Description");
            analysisDescriptionAttribute.Value = analysis.Description;
            xmlAnalysisElt.Attributes.Append(analysisDescriptionAttribute);
            // BoxId
            XmlAttribute boxIdAttribute = xmlDoc.CreateAttribute("BoxId");
            boxIdAttribute.Value = string.Format("{0}", analysis.BProperties.Guid);
            xmlAnalysisElt.Attributes.Append(boxIdAttribute);
            // PalletId
            XmlAttribute palletIdAttribute = xmlDoc.CreateAttribute("PalletId");
            palletIdAttribute.Value = string.Format("{0}", analysis.PalletProperties.Guid);
            xmlAnalysisElt.Attributes.Append(palletIdAttribute);
            // InterlayerId
            if (null != analysis.InterlayerProperties)
            {
                XmlAttribute interlayerIdAttribute = xmlDoc.CreateAttribute("InterlayerId");
                interlayerIdAttribute.Value = string.Format("{0}", analysis.InterlayerProperties.Guid);
                xmlAnalysisElt.Attributes.Append(interlayerIdAttribute);
            }
            if (null != analysis.InterlayerPropertiesAntiSlip)
            {
                XmlAttribute interlayerIdAttribute = xmlDoc.CreateAttribute("InterlayerAntiSlipId");
                interlayerIdAttribute.Value = string.Format("{1}", analysis.InterlayerPropertiesAntiSlip.Guid);
                xmlAnalysisElt.Attributes.Append(interlayerIdAttribute);
            }
            // ###
            // ConstraintSet
            bool bundleAnalysis = (analysis.ConstraintSet.GetType() == typeof(BundlePalletConstraintSet));
            XmlElement constraintSetElement = xmlDoc.CreateElement(bundleAnalysis ? "ConstraintSetBundle" : "ConstraintSetBox");
            XmlAttribute alignedLayersAttribute = xmlDoc.CreateAttribute("AlignedLayersAllowed");
            alignedLayersAttribute.Value = string.Format("{0}", analysis.ConstraintSet.AllowAlignedLayers);
            constraintSetElement.Attributes.Append(alignedLayersAttribute);
            XmlAttribute alternateLayersAttribute = xmlDoc.CreateAttribute("AlternateLayersAllowed");
            alternateLayersAttribute.Value = string.Format("{0}", analysis.ConstraintSet.AllowAlternateLayers);
            constraintSetElement.Attributes.Append(alternateLayersAttribute);
            if (!bundleAnalysis)
            {
                // allowed box positions
                XmlAttribute allowedAxisAttribute = xmlDoc.CreateAttribute("AllowedBoxPositions");
                HalfAxis.HAxis[] axes = { HalfAxis.HAxis.AXIS_X_P, HalfAxis.HAxis.AXIS_Y_P, HalfAxis.HAxis.AXIS_Z_P };
                string allowedAxes = string.Empty;
                foreach (HalfAxis.HAxis axis in axes)
                    if (analysis.ConstraintSet.AllowOrthoAxis(axis))
                    {
                        if (!string.IsNullOrEmpty(allowedAxes))
                            allowedAxes += ",";
                        allowedAxes += HalfAxis.ToString(axis);
                    }
                allowedAxisAttribute.Value = allowedAxes;
                constraintSetElement.Attributes.Append(allowedAxisAttribute);
            }
            // allowed layer patterns
            XmlAttribute allowedPatternAttribute = xmlDoc.CreateAttribute("AllowedPatterns");
            allowedPatternAttribute.Value = analysis.ConstraintSet.AllowedPatternString;
            constraintSetElement.Attributes.Append(allowedPatternAttribute);
            // stop criterions
            if (analysis.ConstraintSet.UseMaximumHeight)
            {
                XmlAttribute maximumHeightAttribute = xmlDoc.CreateAttribute("MaximumHeight");
                maximumHeightAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", analysis.ConstraintSet.MaximumHeight);
                constraintSetElement.Attributes.Append(maximumHeightAttribute);
            }
            if (analysis.ConstraintSet.UseMaximumNumberOfCases)
            {
                XmlAttribute maximumNumberOfItems = xmlDoc.CreateAttribute("ManimumNumberOfItems");
                maximumNumberOfItems.Value = string.Format("{0}", analysis.ConstraintSet.MaximumNumberOfItems);
                constraintSetElement.Attributes.Append(maximumNumberOfItems);
            }
            if (analysis.ConstraintSet.UseMaximumPalletWeight)
            {
                XmlAttribute maximumPalletWeight = xmlDoc.CreateAttribute("MaximumPalletWeight");
                maximumPalletWeight.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", analysis.ConstraintSet.MaximumPalletWeight);
                constraintSetElement.Attributes.Append(maximumPalletWeight);
            }
            if (analysis.ConstraintSet.UseMaximumWeightOnBox)
            {
                XmlAttribute maximumWeightOnBox = xmlDoc.CreateAttribute("MaximumWeightOnBox");
                maximumWeightOnBox.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", analysis.ConstraintSet.MaximumWeightOnBox);
                constraintSetElement.Attributes.Append(maximumWeightOnBox);
            }
            // overhang / underhang
            XmlAttribute overhangX = xmlDoc.CreateAttribute("OverhangX");
            overhangX.Value = string.Format("{0}", analysis.ConstraintSet.OverhangX);
            constraintSetElement.Attributes.Append(overhangX);
            XmlAttribute overhangY = xmlDoc.CreateAttribute("OverhangY");
            overhangY.Value = string.Format("{0}", analysis.ConstraintSet.OverhangY);
            constraintSetElement.Attributes.Append(overhangY);
            // number of solutions to keep
            if (analysis.ConstraintSet.UseNumberOfSolutionsKept)
            {
                XmlAttribute numberOfSolutionsKept = xmlDoc.CreateAttribute("NumberOfSolutions");
                numberOfSolutionsKept.Value = "1";
                constraintSetElement.Attributes.Append(numberOfSolutionsKept);
            }

            xmlAnalysisElt.AppendChild(constraintSetElement);
            // ###
            // Solutions
            XmlElement solutionsElt = xmlDoc.CreateElement("Solutions");
            xmlAnalysisElt.AppendChild(solutionsElt);
            SaveCasePalletSolution(analysis, sol, selSolution, true /* unique */, solutionsElt, xmlDoc );
        }

        public void SaveCasePalletSolution(CasePalletAnalysis analysis, CasePalletSolution sol, SelCasePalletSolution selSolution, bool unique, XmlElement solutionsElt, XmlDocument xmlDoc)
        {
            // Solution
            XmlElement solutionElt = xmlDoc.CreateElement("Solution");
            solutionsElt.AppendChild(solutionElt);
            // title
            XmlAttribute titleAttribute = xmlDoc.CreateAttribute("Title");
            titleAttribute.Value = sol.Title;
            solutionElt.Attributes.Append(titleAttribute);
            // homogeneousLayers ?
            XmlAttribute homogeneousLayersAttribute = xmlDoc.CreateAttribute("HomogeneousLayers");
            homogeneousLayersAttribute.Value = sol.HasHomogeneousLayers ? "true" : "false";
            solutionElt.Attributes.Append(homogeneousLayersAttribute);
            // limit
            XmlAttribute limitReached = xmlDoc.CreateAttribute("LimitReached");
            limitReached.Value = string.Format("{0}", (int)sol.LimitReached);
            solutionElt.Attributes.Append(limitReached);
            // layers
            XmlElement layersElt = xmlDoc.CreateElement("Layers");
            solutionElt.AppendChild(layersElt);

            foreach (ILayer layer in sol)
            {
                BoxLayer boxLayer = layer as BoxLayer;
                if (null != boxLayer)
                    Save(boxLayer, layersElt, xmlDoc);

                InterlayerPos interlayerPos = layer as InterlayerPos;
                if (null != interlayerPos)
                {
                    // Interlayer
                    XmlElement interlayerElt = xmlDoc.CreateElement("Interlayer");
                    layersElt.AppendChild(interlayerElt);
                    // ZLow
                    XmlAttribute zlowAttribute = xmlDoc.CreateAttribute("ZLow");
                    zlowAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", interlayerPos.ZLow);
                    interlayerElt.Attributes.Append(zlowAttribute);
                }
            }

            // Is selected ?
            if (null != selSolution)
            {
                // selected attribute
                XmlAttribute selAttribute = xmlDoc.CreateAttribute("Selected");
                selAttribute.Value = "true";
                solutionElt.Attributes.Append(selAttribute);

                // truck analyses
                XmlElement truckAnalysesElt = xmlDoc.CreateElement("TruckAnalyses");
                solutionElt.AppendChild(truckAnalysesElt);

                foreach (TruckAnalysis truckAnalysis in selSolution.TruckAnalyses)
                    Save(truckAnalysis, unique, truckAnalysesElt, xmlDoc);

                // ect analyses
                XmlElement ectAnalysesElt = xmlDoc.CreateElement("EctAnalyses");
                solutionElt.AppendChild(ectAnalysesElt);

                foreach (ECTAnalysis ectAnalysis in selSolution.EctAnalyses)
                    Save(ectAnalysis, unique, ectAnalysesElt, xmlDoc);
            }
        }

        private void SavePackPalletSolution(
            PackPalletSolution sol
            , SelPackPalletSolution selSolution
            , XmlElement solutionsElt
            , XmlDocument xmlDoc)
        { 
            // solution
            XmlElement solutionElt = xmlDoc.CreateElement("Solution");
            solutionsElt.AppendChild(solutionElt);
            // title
            XmlAttribute titleAttribute = xmlDoc.CreateAttribute("Title");
            titleAttribute.Value = sol.Title;
            solutionElt.Attributes.Append(titleAttribute);
            // layers
            XmlElement boxLayersElt = xmlDoc.CreateElement("BoxLayers");
            solutionElt.AppendChild(boxLayersElt);
            Save(sol.Layer, boxLayersElt, xmlDoc);
            // layerRefs
            XmlElement layerRefsElt = xmlDoc.CreateElement("LayerRefs");
            solutionElt.AppendChild(layerRefsElt);
            // layers
            foreach (LayerDescriptor layerDesc in sol.Layers)
            {
                XmlElement layerRefElt = xmlDoc.CreateElement("LayerRef");
                layerRefsElt.AppendChild(layerRefElt);
                XmlAttribute attributeSwapped = xmlDoc.CreateAttribute("Swapped");
                attributeSwapped.Value = layerDesc.Swapped.ToString();
                layerRefElt.Attributes.Append(attributeSwapped);
                XmlAttribute attributeHasInterlayer = xmlDoc.CreateAttribute("HasInterlayer");
                attributeHasInterlayer.Value = layerDesc.HasInterlayer.ToString();
                layerRefElt.Attributes.Append(attributeHasInterlayer);
            }
            // Is selected ?
            if (null != selSolution)
            {
                // selected attribute
                XmlAttribute selAttribute = xmlDoc.CreateAttribute("Selected");
                selAttribute.Value = "true";
                solutionElt.Attributes.Append(selAttribute);
            }
        }

        public void SaveCylinderPalletSolution(
            CylinderPalletAnalysis analysis
            , CylinderPalletSolution sol
            , SelCylinderPalletSolution selSolution
            , XmlElement solutionsElt
            , XmlDocument xmlDoc)
        {
            // Solution
            XmlElement solutionElt = xmlDoc.CreateElement("Solution");
            solutionsElt.AppendChild(solutionElt);
            // title
            XmlAttribute titleAttribute = xmlDoc.CreateAttribute("Title");
            titleAttribute.Value = sol.Title;
            solutionElt.Attributes.Append(titleAttribute);
            // limit
            XmlAttribute limitReached = xmlDoc.CreateAttribute("LimitReached");
            limitReached.Value = string.Format("{0}", (int)sol.LimitReached);
            solutionElt.Attributes.Append(limitReached);
            // layers
            XmlElement layersElt = xmlDoc.CreateElement("Layers");
            solutionElt.AppendChild(layersElt);

            foreach (ILayer layer in sol)
            {
                CylinderLayer cylLayer = layer as CylinderLayer;
                if (null != cylLayer)
                    Save(cylLayer, layersElt, xmlDoc);

                InterlayerPos interlayerPos = layer as InterlayerPos;
                if (null != interlayerPos)
                {
                    // Interlayer
                    XmlElement interlayerElt = xmlDoc.CreateElement("Interlayer");
                    layersElt.AppendChild(interlayerElt);
                    // ZLow
                    XmlAttribute zlowAttribute = xmlDoc.CreateAttribute("ZLow");
                    zlowAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", interlayerPos.ZLow);
                    interlayerElt.Attributes.Append(zlowAttribute);
                }
            }
            if (null != selSolution)
            {
                // selected attribute
                XmlAttribute selAttribute = xmlDoc.CreateAttribute("Selected");
                selAttribute.Value = "true";
                solutionElt.Attributes.Append(selAttribute);            
            }
        }

        public void SaveHCylinderPalletSolution(
            HCylinderPalletAnalysis analysis
            , HCylinderPalletSolution sol
            , SelHCylinderPalletSolution selSolution
            , XmlElement solutionsElt
            , XmlDocument xmlDoc)
        {
            // Solution
            XmlElement solutionElt = xmlDoc.CreateElement("Solution");
            solutionsElt.AppendChild(solutionElt);
            // title
            XmlAttribute titleAttribute = xmlDoc.CreateAttribute("Title");
            titleAttribute.Value = sol.Title;
            solutionElt.Attributes.Append(titleAttribute);
            // limit
            XmlAttribute limitReached = xmlDoc.CreateAttribute("LimitReached");
            limitReached.Value = string.Format("{0}", (int)sol.LimitReached);
            solutionElt.Attributes.Append(limitReached);
            // layers
            XmlElement positionsElt = xmlDoc.CreateElement("CylPositions");
            solutionElt.AppendChild(positionsElt);
            foreach (CylPosition cylPos in sol)
            {
                // CylPosition
                XmlElement positionElt = xmlDoc.CreateElement("CylPosition");
                positionsElt.AppendChild(positionElt);
                // XmlAttribute
                XmlAttribute attPosition = xmlDoc.CreateAttribute("Position");
                attPosition.Value = cylPos.XYZ.ToString();
                positionElt.Attributes.Append(attPosition);
                XmlAttribute attAxisDir = xmlDoc.CreateAttribute("AxisDir");
                attAxisDir.Value = HalfAxis.ToString(cylPos.Direction);
                positionElt.Attributes.Append(attAxisDir);
            }
            if (null != selSolution)
            {
                // selected attribute
                XmlAttribute selAttribute = xmlDoc.CreateAttribute("Selected");
                selAttribute.Value = "true";
                solutionElt.Attributes.Append(selAttribute);
            }
        }

        public void Save(TruckAnalysis truckAnalysis, bool unique, XmlElement truckAnalysesElt, XmlDocument xmlDoc)
        {
            XmlElement truckAnalysisElt = xmlDoc.CreateElement("TruckAnalysis");
            truckAnalysesElt.AppendChild(truckAnalysisElt);
            // name
            XmlAttribute nameAttribute = xmlDoc.CreateAttribute("Name");
            nameAttribute.Value = truckAnalysis.Name;
            truckAnalysisElt.Attributes.Append(nameAttribute);
            // description
            XmlAttribute descriptionAttribute = xmlDoc.CreateAttribute("Description");
            descriptionAttribute.Value = truckAnalysis.Description;
            truckAnalysisElt.Attributes.Append(descriptionAttribute);
            // truckId
            XmlAttribute truckIdAttribute = xmlDoc.CreateAttribute("TruckId");
            truckIdAttribute.Value = string.Format("{0}", truckAnalysis.TruckProperties.Guid);
            truckAnalysisElt.Attributes.Append(truckIdAttribute);
            // constraint set
            XmlElement contraintSetElt = xmlDoc.CreateElement("ConstraintSet");
            truckAnalysesElt.AppendChild(contraintSetElt);            
            // multilayer allowed
            XmlAttribute multilayerAllowedAttribute = xmlDoc.CreateAttribute("MultilayerAllowed");
            multilayerAllowedAttribute.Value = truckAnalysis.ConstraintSet.MultilayerAllowed ? "True" : "False";
            contraintSetElt.Attributes.Append(multilayerAllowedAttribute);
            // allowed pallet orientation
            XmlAttribute palletOrientationsAttribute = xmlDoc.CreateAttribute("AllowedPalletOrientations");
            string sAllowedPalletOrientations = string.Empty;
            if (truckAnalysis.ConstraintSet.AllowPalletOrientationX)
                sAllowedPalletOrientations += "X";
            if (truckAnalysis.ConstraintSet.AllowPalletOrientationY)
            {
                if (!string.IsNullOrEmpty(sAllowedPalletOrientations))
                    sAllowedPalletOrientations += ",";
                sAllowedPalletOrientations += "Y";
            }
            palletOrientationsAttribute.Value = sAllowedPalletOrientations;
            contraintSetElt.Attributes.Append(palletOrientationsAttribute);
            // min distance pallet / truck wall
            XmlAttribute minDistancePalletWallAttribute = xmlDoc.CreateAttribute("MinDistancePalletWall");
            minDistancePalletWallAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", truckAnalysis.ConstraintSet.MinDistancePalletTruckWall);
            contraintSetElt.Attributes.Append(minDistancePalletWallAttribute);
            // min distance pallet / truck roof
            XmlAttribute minDistancePalletRoofAttribute = xmlDoc.CreateAttribute("MinDistancePalletRoof");
            minDistancePalletRoofAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", truckAnalysis.ConstraintSet.MinDistancePalletTruckWall);
            contraintSetElt.Attributes.Append(minDistancePalletRoofAttribute);
            // solutions
            XmlElement truckSolutionsElt = xmlDoc.CreateElement("Solutions");
            truckAnalysisElt.AppendChild(truckSolutionsElt);
            int solutionIndex = 0;
            foreach (TruckSolution truckSolution in truckAnalysis.Solutions)
            {
                if (!unique || truckAnalysis.HasSolutionSelected(solutionIndex))
                {
                    XmlElement truckSolutionElt = xmlDoc.CreateElement("Solution");
                    truckSolutionsElt.AppendChild(truckSolutionElt);
                    // title
                    XmlAttribute titleAttribute = xmlDoc.CreateAttribute("Title");
                    titleAttribute.Value = truckSolution.Title;
                    truckSolutionsElt.Attributes.Append(titleAttribute);
                    // selected
                    XmlAttribute selectedAttribute = xmlDoc.CreateAttribute("Selected");
                    selectedAttribute.Value = truckAnalysis.HasSolutionSelected(solutionIndex) ? "True" : "False";
                    truckSolutionElt.Attributes.Append(selectedAttribute);
                    // layer
                    XmlElement layersElt = xmlDoc.CreateElement("Layers");
                    truckSolutionElt.AppendChild(layersElt);
                    Save(truckSolution.Layer, layersElt, xmlDoc);
                }
                // increment index
                ++solutionIndex;
            }
        }

        public void Save(ECTAnalysis ectAnalysis, bool unique, XmlElement ectAnalysesElt, XmlDocument xmlDoc)
        {
            XmlElement ectAnalysisElt = xmlDoc.CreateElement("EctAnalysis");
            ectAnalysesElt.AppendChild(ectAnalysisElt);
            // name
            XmlAttribute nameAttribute = xmlDoc.CreateAttribute("Name");
            nameAttribute.Value = ectAnalysis.Name;
            ectAnalysisElt.Attributes.Append(nameAttribute);
            // description
            XmlAttribute descriptionAttribute = xmlDoc.CreateAttribute("Description");
            descriptionAttribute.Value = ectAnalysis.Description;
            ectAnalysisElt.Attributes.Append(descriptionAttribute);
            // cardboard
            XmlElement cardboardElt = xmlDoc.CreateElement("Cardboard");
            ectAnalysesElt.AppendChild(cardboardElt);
            // - name
            XmlAttribute nameCardboardAttribute = xmlDoc.CreateAttribute("Name");
            nameCardboardAttribute.Value = ectAnalysis.Cardboard.Name;
            cardboardElt.Attributes.Append(nameCardboardAttribute);
            // - thickness
            XmlAttribute thicknessAttribute = xmlDoc.CreateAttribute("Thickness");
            thicknessAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", ectAnalysis.Cardboard.Thickness);
            cardboardElt.Attributes.Append(thicknessAttribute);
             // - ect
            XmlAttribute ectAttribute = xmlDoc.CreateAttribute("ECT");
            ectAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", ectAnalysis.Cardboard.ECT);
            cardboardElt.Attributes.Append(ectAttribute);
            // - stiffnessX
            XmlAttribute stiffnessAttributeX = xmlDoc.CreateAttribute("StiffnessX");
            stiffnessAttributeX.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", ectAnalysis.Cardboard.RigidityDX);
            cardboardElt.Attributes.Append(stiffnessAttributeX);
            // - stiffnessY
            XmlAttribute stiffnessAttributeY = xmlDoc.CreateAttribute("StiffnessY");
            stiffnessAttributeY.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", ectAnalysis.Cardboard.RigidityDY);
            cardboardElt.Attributes.Append(stiffnessAttributeY);
            // case type
            XmlAttribute caseTypeAttribute = xmlDoc.CreateAttribute("CaseType");
            caseTypeAttribute.Value = ectAnalysis.CaseType;
            ectAnalysisElt.Attributes.Append(caseTypeAttribute);
            // print surface
            XmlAttribute printSurfaceAttribute = xmlDoc.CreateAttribute("PrintSurface");
            printSurfaceAttribute.Value = ectAnalysis.PrintSurface;
            ectAnalysesElt.Attributes.Append(printSurfaceAttribute);
            // mc kee formula mode
            XmlAttribute mcKeeFormulaAttribute = xmlDoc.CreateAttribute("McKeeFormulaMode");
            mcKeeFormulaAttribute.Value = ectAnalysis.McKeeFormulaText;
            ectAnalysisElt.Attributes.Append(mcKeeFormulaAttribute);
        }

        public void Save(BoxLayer boxLayer, XmlElement layersElt, XmlDocument xmlDoc)
        {
            // BoxLayer
            XmlElement boxlayerElt = xmlDoc.CreateElement("BoxLayer");
            layersElt.AppendChild(boxlayerElt);
            // ZLow
            XmlAttribute zlowAttribute = xmlDoc.CreateAttribute("ZLow");
            zlowAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", boxLayer.ZLow);
            boxlayerElt.Attributes.Append(zlowAttribute);
            // maximum space
            XmlAttribute attributeMaxSpace = xmlDoc.CreateAttribute("MaximumSpace");
            attributeMaxSpace.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", boxLayer.MaximumSpace);
            boxlayerElt.Attributes.Append(attributeMaxSpace);

            foreach (BoxPosition boxPosition in boxLayer)
            {
                // BoxPosition
                XmlElement boxPositionElt = xmlDoc.CreateElement("BoxPosition");
                boxlayerElt.AppendChild(boxPositionElt);
                // Position
                XmlAttribute positionAttribute = xmlDoc.CreateAttribute("Position");
                positionAttribute.Value = boxPosition.Position.ToString();
                boxPositionElt.Attributes.Append(positionAttribute);
                // AxisLength
                XmlAttribute axisLengthAttribute = xmlDoc.CreateAttribute("AxisLength");
                axisLengthAttribute.Value = HalfAxis.ToString(boxPosition.DirectionLength);
                boxPositionElt.Attributes.Append(axisLengthAttribute);
                // AxisWidth
                XmlAttribute axisWidthAttribute = xmlDoc.CreateAttribute("AxisWidth");
                axisWidthAttribute.Value = HalfAxis.ToString(boxPosition.DirectionWidth);
                boxPositionElt.Attributes.Append(axisWidthAttribute);
            }
        }

        public void Save(CylinderLayer cylLayer, XmlElement layersElt, XmlDocument xmlDoc)
        {
            // BoxLayer
            XmlElement cylLayerElt = xmlDoc.CreateElement("CylLayer");
            layersElt.AppendChild(cylLayerElt);
            // ZLow
            XmlAttribute zlowAttribute = xmlDoc.CreateAttribute("ZLow");
            zlowAttribute.Value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", cylLayer.ZLow);
            cylLayerElt.Attributes.Append(zlowAttribute);
            foreach (Vector3D boxPosition in cylLayer)
            {
                // BoxPosition
                XmlElement cylPositionElt = xmlDoc.CreateElement("CylPosition");
                cylLayerElt.AppendChild(cylPositionElt);
                // Position
                XmlAttribute positionAttribute = xmlDoc.CreateAttribute("Position");
                positionAttribute.Value = boxPosition.ToString();
                cylPositionElt.Attributes.Append(positionAttribute);
            }            
        }
        #endregion

        #region Close
        public virtual void Close()
        {
            // remove all analysis and items
            // -> this should close any listening forms
            while (_boxCasePalletOptimizations.Count > 0)
                RemoveItem(_boxCasePalletOptimizations[0]);
            while (_casePalletAnalyses.Count > 0)
                RemoveItem(_casePalletAnalyses[0]);
            while (_typeList.Count > 0)
                RemoveItem(_typeList[0]);
            NotifyOnDocumentClosed();
        }
        #endregion

        #region Helpers
        private ItemBase GetTypeByGuid(Guid guid)
        {
            foreach (ItemBase type in _typeList)
                if (type.Guid == guid)
                    return type;
            throw new Exception(string.Format("No type with Guid = {0}", guid.ToString()));
        }
        private static string BitmapToString(Bitmap bmp)
        {
            byte[] bmpBytes;
            using (MemoryStream ms = new MemoryStream())
            {
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                bmpBytes = ms.GetBuffer();
                ms.Close();
            }
            return System.Convert.ToBase64String(bmpBytes);
        }
        private static Bitmap StringToBitmap(string bmpData)
        {
            byte[] bytes = System.Convert.FromBase64String(bmpData);
            return new Bitmap(new System.IO.MemoryStream(bytes));
        }
        private static int[] ParseInt2(string value)
        {
            string regularExp = "(?<i1>.*) (?<i2>.*)";
            Regex r = new Regex(regularExp, RegexOptions.Singleline);
            Match m = r.Match(value);
            if (m.Success)
            {
                int[] iArray = new int[2];
                iArray[0] = int.Parse(m.Result("${i1}"));
                iArray[1] = int.Parse(m.Result("${i2}"));
                return iArray;
            }
            else
                throw new Exception("Failed parsing int[2] from " + value);
        }
        private static int[] ParseInt3(string value)
        {
            string regularExp = "(?<i1>.*) (?<i2>.*) (?<i3>.*)";
            Regex r = new Regex(regularExp, RegexOptions.Singleline);
            Match m = r.Match(value);
            if (m.Success)
            {
                int[] iArray = new int[3];
                iArray[0] = int.Parse(m.Result("${i1}"));
                iArray[1] = int.Parse(m.Result("${i2}"));
                iArray[2] = int.Parse(m.Result("${i3}"));
                return iArray;
            }
            else
                throw new Exception("Failed parsing int[3] from " + value);
        }
        #endregion

        #region Methods to be overriden
        public virtual void Modify()
        {
        }
        #endregion

        #region Listener notification methods
        public void AddListener(IDocumentListener listener)
        {
            _listeners.Add(listener);
        }
        private void NotifyOnNewTypeCreated(ItemBase item)
        {
            foreach (IDocumentListener listener in _listeners)
                listener.OnNewTypeCreated(this, item);
        }
        private void NotifyOnNewAnalysisCreated(Analysis analysis)
        {
            foreach (IDocumentListener listener in _listeners)
                listener.OnNewAnalysisCreated(this, analysis);
        }
        private void NotifyOnNewCasePalletAnalysisCreated(CasePalletAnalysis analysis)
        {
            foreach (IDocumentListener listener in _listeners)
                listener.OnNewCasePalletAnalysisCreated(this, analysis);
        }
        private void NotifyOnNewPackPalletAnalysisCreated(PackPalletAnalysis analysis)
        {
            foreach (IDocumentListener listener in _listeners)
                listener.OnNewPackPalletAnalysisCreated(this, analysis);
        }
        private void NotifyOnNewCylinderPalletAnalysisCreated(CylinderPalletAnalysis analysis)
        {
            foreach (IDocumentListener listener in _listeners)
                listener.OnNewCylinderPalletAnalysisCreated(this, analysis);
        }
        private void NotifyOnNewHCylinderPalletAnalysisCreated(HCylinderPalletAnalysis analysis)
        {
            foreach (IDocumentListener listener in _listeners)
                listener.OnNewHCylinderPalletAnalysisCreated(this, analysis);
        }
        private void NotifyOnNewBoxCaseAnalysis(BoxCaseAnalysis analysis)
        {
            foreach (IDocumentListener listener in _listeners)
                listener.OnNewBoxCaseAnalysisCreated(this, analysis);
        }
        private void NotifyOnNewCaseAnalysisCreated(BoxCasePalletAnalysis caseAnalysis)
        { 
            foreach (IDocumentListener listener in _listeners)
                listener.OnNewBoxCasePalletAnalysisCreated(this, caseAnalysis);
        }
        internal void NotifyOnNewTruckAnalysisCreated(CasePalletAnalysis analysis, SelCasePalletSolution selSolution, TruckAnalysis truckAnalysis)
        {
            foreach (IDocumentListener listener in _listeners)
                listener.OnNewTruckAnalysisCreated(this, analysis, selSolution, truckAnalysis);
        }
        internal void NotifyOnNewECTAnalysisCreated(CasePalletAnalysis analysis, SelCasePalletSolution selSolution, ECTAnalysis ectAnalysis)
        { 
            foreach (IDocumentListener listener in _listeners)
                listener.OnNewECTAnalysisCreated(this, analysis, selSolution, ectAnalysis);
        }
        private void NotifyOnDocumentClosed()
        {
            foreach (IDocumentListener listener in _listeners)
                listener.OnDocumentClosed(this);
        }
        private void NotifyOnTypeRemoved(ItemBase item)
        {
            foreach (IDocumentListener listener in _listeners)
                listener.OnTypeRemoved(this, item);
        }
        private void NotifyOnAnalysisRemoved(ItemBase analysis)
        {
            foreach (IDocumentListener listener in _listeners)
                listener.OnAnalysisRemoved(this, analysis);
        }
        internal void NotifyOnTruckAnalysisRemoved(SelCasePalletSolution selSolution, TruckAnalysis truckAnalysis)
        {
            foreach (IDocumentListener listener in _listeners)
                listener.OnTruckAnalysisRemoved(this, selSolution.Analysis, selSolution, truckAnalysis);
        }
        internal void NotifyOnECTAnalysisRemoved(SelCasePalletSolution selSolution, ECTAnalysis ectAnalysis)
        {
            foreach (IDocumentListener listener in _listeners)
                listener.OnECTAnalysisRemoved(this, selSolution.Analysis, selSolution, ectAnalysis);
        }
        #endregion
    }
    #endregion
}
