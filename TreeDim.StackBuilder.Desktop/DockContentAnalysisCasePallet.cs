#region Using directives
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
// Docking
using WeifenLuo.WinFormsUI.Docking;
// log4net
using log4net;
using Sharp3D.Math.Core;
using treeDiM.StackBuilder.Basics;
using treeDiM.StackBuilder.Graphics;
using treeDiM.StackBuilder.Desktop.Properties;
#endregion

namespace treeDiM.StackBuilder.Desktop
{
    public partial class DockContentAnalysisCasePallet : DockContent, IView, IItemListener
    {
        #region Data members
        /// <summary>
        /// document
        /// </summary>
        private IDocument _document;
        /// <summary>
        /// analysis
        /// </summary>
        private AnalysisCasePallet _analysis;
        /// <summary>
        /// solution
        /// </summary>
        private Solution _solution;

        /// <summary>
        /// logger
        /// </summary>
        static readonly ILog _log = LogManager.GetLogger(typeof(DockContentAnalysisCasePallet));
        #endregion

        #region Constructor
        public DockContentAnalysisCasePallet(IDocument document, AnalysisCasePallet analysis)
        {
            _document = document;

            _analysis = analysis;
            _analysis.AddListener(this);

            _solution = analysis.Solutions[0];

            InitializeComponent();

        }
        #endregion

        #region Form override
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // initialize drawing container
            graphCtrlSolution.Viewer = new ViewerSolution(_solution);
            graphCtrlSolution.Invalidate();

            graphCtrlSolution.VolumeSelected += onLayerSelected;

            FillLayerControls();
            UpdateControls();
        }

        void onLayerSelected(int id)
        {
            _solution.SelectLayer(id);
            UpdateControls();
        }
        #endregion

        #region IItemListener implementation
        public void Update(ItemBase item)
        {
            graphCtrlSolution.Invalidate();
        }
        public void Kill(ItemBase item)
        {
            Close();
            _analysis.RemoveListener(this);
        }
        #endregion

        #region IView implementation
        public IDocument Document
        {
            get { return _document; }
        }
        #endregion

        #region IDrawingContainer
        public void Draw(Graphics3DControl ctrl, Graphics3D graphics)
        {
            bool showDimensions = true;
            ViewerSolution sv = new ViewerSolution( Solution);
            sv.Draw(graphics, showDimensions);
        }
        #endregion

        #region Public properties
        public AnalysisCasePallet Analysis
        {
            get { return _analysis; }
        }
        public Solution Solution
        {
            get { return _solution; }
            set { _solution = value; }
        }
        #endregion

        #region Event handlers
        private void onLayerIndexChanged(object sender, EventArgs e)
        {
            // get index of layer type
            int layerIndex = cbLayerType.SelectedIndex;
            if (-1 == layerIndex)
                return;
            // set on current layer
            _solution.SelectLayer(layerIndex);
            graphCtrlSolution.Invalidate();
            // update controls
            UpdateControls();
        }
        private void onLayerTypeChanged(object sender, EventArgs e)
        {
            int iLayerType = cbLayerType.SelectedIndex;
            // get selected layer
            _solution.SetLayerTypeOnSelected(iLayerType);
            // redraw
            graphCtrlSolution.Invalidate();
        }
        private void onInterlayerChanged(object sender, EventArgs e)
        {
            // get index of interlayer
            InterlayerProperties interlayer = null;
            if (chkbInterlayer.Checked)
            {
                ItemBaseCB itemInterlayer = cbInterlater.SelectedItem as ItemBaseCB;
                if (null != itemInterlayer)
                    interlayer = itemInterlayer.Item as InterlayerProperties;
            }
            _solution.SetInterlayerOnSelected(interlayer);
            graphCtrlSolution.Invalidate();
        }
        private void onReflectionX(object sender, EventArgs e)
        {
            _solution.ApplySymetryOnSelected(0);
            graphCtrlSolution.Invalidate();
        }
        private void onReflectionY(object sender, EventArgs e)
        {
            _solution.ApplySymetryOnSelected(1);
            graphCtrlSolution.Invalidate();
        }
        private void onChkbInterlayerClicked(object sender, EventArgs e)
        {
            cbInterlater.Enabled = chkbInterlayer.Checked;
            onInterlayerChanged(null, null);
        }
        #endregion

        #region Layer controls
        private void FillLayerControls()
        {
            // layer combo box
            foreach (LayerDesc layerDesc in _solution.LayerDescriptors)
                cbLayerType.Items.Add(layerDesc);
            if (cbLayerType.Items.Count > 0)
                cbLayerType.SelectedIndex = 0;

            // interlayer combo box
            Document doc = _document as Document;
            if (null == doc) return;
            ItemBase[] interlayers = doc.ListByType( typeof(InterlayerProperties) ).ToArray();
            ComboBoxHelpers.FillCombo(interlayers, cbInterlater, null);
        }
        private void UpdateControls()
        {
            int index = _solution.SelectedLayerIndex;
            bnSymmetryX.Enabled = (index != -1);
            bnSymetryY.Enabled = (index != -1);
            cbLayerType.Enabled = (index != -1);
            chkbInterlayer.Enabled = (index != -1) && (cbInterlater.Items.Count > 0);

            gbLayer.Text = index != -1 ? string.Format("Selected layer : {0}", index) : "Double-click a layer";

            if (index != -1)
            {
                tbClickLayer.Hide();
                gbLayer.Show();

                // get selected solution item
                SolutionItem selItem = _solution.SelectedSolutionItem;
                // set current layer
                cbLayerType.SelectedIndex = selItem.LayerIndex;
                // set interlayer
                chkbInterlayer.Checked = selItem.HasInterlayer;
                onChkbInterlayerClicked(null, null);
                // select combo box
                int selIndex = 0;
                foreach (object o in cbInterlater.Items)
                {
                    InterlayerProperties interlayerProp = o as InterlayerProperties;
                    if (interlayerProp == _solution.SelectedInterlayer)
                        break;
                    ++selIndex;
                }
                if (selIndex < cbInterlater.Items.Count)
                    cbInterlater.SelectedIndex = selIndex;
            }
            else
            {
                gbLayer.Hide();
                tbClickLayer.Show();
            }
        }

        #endregion
    }
}
