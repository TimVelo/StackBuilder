﻿#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace TreeDim.StackBuilder.Basics
{
    #region Analysis
    public class Analysis : ItemBase
    {
        #region Data members
        private BProperties _boxProperties;
        private PalletProperties _palletProperties;
        private InterlayerProperties _interlayerProperties;
        private ConstraintSet _constraintSet;
        private List<Solution> _solutions;
        private List<SelSolution> _selectedSolutions = new List<SelSolution>();
        #endregion

        #region Constructor
        public Analysis(BProperties boxProperties, PalletProperties palletProperties, InterlayerProperties interlayerProperties, ConstraintSet constraintSet)
            : base(boxProperties.ParentDocument)
        {
            // sanity check
            if (palletProperties.ParentDocument != ParentDocument
                || (interlayerProperties != null && interlayerProperties.ParentDocument != ParentDocument))
                throw new Exception();

            _boxProperties = boxProperties;
            _palletProperties = palletProperties;
            _interlayerProperties = interlayerProperties;
            _constraintSet = constraintSet;

            boxProperties.AddDependancie(this);
            palletProperties.AddDependancie(this);
            if (null != interlayerProperties)
                interlayerProperties.AddDependancie(this);
        }
        #endregion

        #region Public properties
        public List<Solution> Solutions
        {
            set { _solutions = value; }
            get { return _solutions; }
        }

        public BProperties BProperties
        {
            get { return _boxProperties; }
            set { _boxProperties = value; }
        }

        public PalletProperties PalletProperties
        {
            get { return _palletProperties; }
            set { _palletProperties = value; }
        }

        public InterlayerProperties InterlayerProperties
        {
            get { return _interlayerProperties; }
            set { _interlayerProperties = value; }
        }

        public ConstraintSet ConstraintSet
        {
            get { return _constraintSet; }
            set { _constraintSet = value; }
        }
        #endregion

        #region Solution selection
        public void SelectSolutionByIndex(int index)
        {
            if (index < 0 || index > _solutions.Count) return;  // no solution with this index
            if (HasSolutionSelected(index)) return;             // solution already selected
            // instantiate new SelSolution
            SelSolution selSolution = new SelSolution(ParentDocument, this, _solutions[index]);
            // insert in list
            _selectedSolutions.Add(selSolution);
            // notify document listeners
            ParentDocument.NotifyOnNewSolutionAdded(this, selSolution);
        }
        public void UnselectSolutionByIndex(int index)
        {
            SelSolution selSolution = GetSelSolutionBySolutionIndex(index);
            if (null == selSolution) return; // this solution not selected
            // remove from list
            _selectedSolutions.Remove(selSolution);
            // notify document listeners
            ParentDocument.NotifyOnSolutionRemoved(this, selSolution);
        }
        public bool HasSolutionSelected(int index)
        {
            return (null != GetSelSolutionBySolutionIndex(index));
        }
        private SelSolution GetSelSolutionBySolutionIndex(int index)
        {
            if (index < 0 || index > _solutions.Count) return null;  // no solution with this index
            return _selectedSolutions.Find(delegate(SelSolution selSol) { return selSol.Solution == _solutions[index]; });
        }
        #endregion

        #region Dependancies
        protected override void RemoveItselfFromDependancies()
        {
            _boxProperties.RemoveDependancie(this);
            _palletProperties.RemoveDependancie(this);
            if (null != _interlayerProperties)
                _interlayerProperties.RemoveDependancie(this);
            base.RemoveItselfFromDependancies();
        }
        public override void OnAttributeModified(ItemBase modifiedAttribute)
        {
            _solutions.Clear();
        }
        public override void OnEndUpdate(ItemBase updatedAttribute)
        {
            _solutions.Clear();
            Modify();
        }
        #endregion
    }
    #endregion

    #region IAnalysisSolver
    public interface IAnalysisSolver
    { 
        void ProcessAnalysis(Analysis analysis);
    }
    #endregion
}
