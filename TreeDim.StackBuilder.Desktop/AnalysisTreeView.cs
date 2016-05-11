﻿#region Using directives
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using treeDiM.StackBuilder.Basics;
using treeDiM.StackBuilder.Engine;
using treeDiM.StackBuilder.Desktop.Properties;

using log4net;
#endregion

namespace treeDiM.StackBuilder.Desktop
{
    #region AnalysisTreeView
    /// <summary>
    /// AnalysisTreeView : left frame treeview control
    /// </summary>
    public partial class AnalysisTreeView
        : System.Windows.Forms.TreeView, IDocumentListener
    {
        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public AnalysisTreeView()
        {
            try
            {
                // build image list for tree
                ImageList = new ImageList();
                ImageList.Images.Add(AnalysisTreeView.CLSDFOLD);                    // 0
                ImageList.Images.Add(AnalysisTreeView.OPENFOLD);                    // 1
                ImageList.Images.Add(AnalysisTreeView.DOC);                         // 2
                ImageList.Images.Add(AnalysisTreeView.Box);                         // 3
                ImageList.Images.Add(AnalysisTreeView.Case);                        // 4
                ImageList.Images.Add(AnalysisTreeView.Bundle);                      // 5
                ImageList.Images.Add(AnalysisTreeView.Cylinder);                    // 6
                ImageList.Images.Add(AnalysisTreeView.Pallet);                      // 7
                ImageList.Images.Add(AnalysisTreeView.Interlayer);                  // 8
                ImageList.Images.Add(AnalysisTreeView.Truck);                       // 9
                ImageList.Images.Add(AnalysisTreeView.PalletCorners);               // 10
                ImageList.Images.Add(AnalysisTreeView.PalletCap);                   // 11
                ImageList.Images.Add(AnalysisTreeView.PalletFilm);                  // 12
                ImageList.Images.Add(AnalysisTreeView.Analysis);                    // 13
                ImageList.Images.Add(AnalysisTreeView.AnalysisBundle);              // 14
                ImageList.Images.Add(AnalysisTreeView.Solution);                    // 15
                ImageList.Images.Add(AnalysisTreeView.TruckAnalysis);               // 16
                ImageList.Images.Add(AnalysisTreeView.CaseAnalysis);                // 17
                ImageList.Images.Add(AnalysisTreeView.CaseOfBoxes);                 // 18
                ImageList.Images.Add(AnalysisTreeView.AnalysisStackingStrength);    // 19
                ImageList.Images.Add(AnalysisTreeView.CylinderPalletAnalysis);      // 20
                ImageList.Images.Add(AnalysisTreeView.HCylinderPalletAnalysis);     // 21
                ImageList.Images.Add(AnalysisTreeView.Pack);                        // 22
                ImageList.Images.Add(AnalysisTreeView.PackAnalysis);                // 23
               // instantiate context menu
                this.ContextMenuStrip = new ContextMenuStrip();
                // attach event handlers
                this.NodeMouseClick += new TreeNodeMouseClickEventHandler(AnalysisTreeView_NodeMouseClick);
                this.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler(AnalysisTreeView_NodeMouseDoubleClick);
                this.ContextMenuStrip.Opening += new CancelEventHandler(ContextMenuStrip_Opening);
                this.DrawMode = TreeViewDrawMode.OwnerDrawText;
                this.DrawNode += new DrawTreeNodeEventHandler(AnalysisTreeView_DrawNode);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // AnalysisTreeView
            // 
            this.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            this.ResumeLayout(false);

        }
        #endregion

        #region Context menu strip
        /// <summary>
        /// Handler for ContextMenu.Popup event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                // retrieve node which was clicked
                TreeNode node = GetNodeAt(PointToClient(Cursor.Position));
                if (node == null) return; // user might right click no valid node
                SelectedNode = node;
                // clear previous items
                this.ContextMenuStrip.Items.Clear();
                // let the provider do his work
                NodeTag nodeTag = node.Tag as NodeTag;
                if (null != nodeTag)
                    QueryContextMenuItems(nodeTag, this.ContextMenuStrip);
                // set Cancel to false. 
                // it is optimized to true based on empty entry.
                e.Cancel = !(this.ContextMenuStrip.Items.Count > 0);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
            }
        }

        private void QueryContextMenuItems(NodeTag nodeTag, ContextMenuStrip contextMenuStrip)
        {
            if (nodeTag.Type == NodeTag.NodeType.NT_DOCUMENT)
            {
                contextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.ID_ADDNEWBOX, AnalysisTreeView.Box         , new EventHandler(onCreateNewBox)));
                contextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.ID_ADDNEWCASE, AnalysisTreeView.Case, new EventHandler(onCreateNewCase)));
                contextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.ID_ADDNEWCYLINDER, AnalysisTreeView.Cylinder, new EventHandler(onCreateNewCylinder)));
                contextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.ID_ADDNEWPALLET, AnalysisTreeView.Pallet      , new EventHandler(onCreateNewPallet)));
                contextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.ID_ADDNEWINTERLAYER, AnalysisTreeView.Interlayer, new EventHandler(onCreateNewInterlayer)));
                contextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.ID_ADDNEWBUNDLE, AnalysisTreeView.Bundle      , new EventHandler(onCreateNewBundle)));
                contextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.ID_ADDNEWPALLETCORNERS, AnalysisTreeView.PalletCorners, new EventHandler(onCreateNewPalletCorners)));
                contextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.ID_ADDNEWPALLETCAP, AnalysisTreeView.PalletCap, new EventHandler(onCreateNewPalletCap)));
                contextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.ID_ADDNEWPALLETFILM, AnalysisTreeView.PalletFilm, new EventHandler(onCreateNewPalletFilm)));
                contextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.ID_ADDNEWTRUCK, AnalysisTreeView.Truck       , new EventHandler(onCreateNewTruck)));

                if (((DocumentSB)nodeTag.Document).CanCreateCasePalletAnalysis || ((DocumentSB)nodeTag.Document).CanCreateBundlePalletAnalysis || ((DocumentSB)nodeTag.Document).CanCreateBoxCasePalletAnalysis)
                    contextMenuStrip.Items.Add(new ToolStripSeparator());
                if (((DocumentSB)nodeTag.Document).CanCreateCasePalletAnalysis)
                    contextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.ID_ADDNEWANALYSIS, AnalysisTreeView.Analysis, new EventHandler(onCreateNewCasePalletAnalysis)));
                if (((DocumentSB)nodeTag.Document).CanCreatePackPalletAnalysis)
                    contextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.ID_ADDNEWPACKANALYSIS, AnalysisTreeView.PackAnalysis, new EventHandler(onCreateNewPackPalletAnalysis)));
                if (((DocumentSB)nodeTag.Document).CanCreateBundlePalletAnalysis)
                    contextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.ID_ADDNEWBUNDLEANALYSIS, AnalysisTreeView.AnalysisBundle, new EventHandler(onCreateNewBundleAnalysis)));
                if (((DocumentSB)nodeTag.Document).CanCreateBoxCaseAnalysis)
                    contextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.ID_ADDNEWBOXCASEANALYSIS, AnalysisTreeView.BoxCaseAnalysis, new EventHandler(onCreateNewBoxCaseAnalysis)));
                if (((DocumentSB)nodeTag.Document).CanCreateBoxCasePalletAnalysis)
                    contextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.ID_ADDNEWCASEANALYSIS, AnalysisTreeView.CaseAnalysis, new EventHandler(onCreateNewCaseAnalysis)));
                contextMenuStrip.Items.Add(new ToolStripSeparator());
                contextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.ID_CLOSE, null, new EventHandler(onDocumentClose)));
            }
            if (nodeTag.Type == NodeTag.NodeType.NT_BOX
                || nodeTag.Type == NodeTag.NodeType.NT_CASE
                || nodeTag.Type == NodeTag.NodeType.NT_PACK
                || nodeTag.Type == NodeTag.NodeType.NT_CASEOFBOXES
                || nodeTag.Type == NodeTag.NodeType.NT_CYLINDER
                || nodeTag.Type == NodeTag.NodeType.NT_PALLET
                || nodeTag.Type == NodeTag.NodeType.NT_BUNDLE
                || nodeTag.Type == NodeTag.NodeType.NT_INTERLAYER
                || nodeTag.Type == NodeTag.NodeType.NT_TRUCK
                || nodeTag.Type == NodeTag.NodeType.NT_PALLETCORNERS
                || nodeTag.Type == NodeTag.NodeType.NT_PALLETCAP
                || nodeTag.Type == NodeTag.NodeType.NT_PALLETFILM
                )
            {
                string message = string.Format(Resources.ID_DELETE, nodeTag.ItemProperties.Name);
                contextMenuStrip.Items.Add(new ToolStripMenuItem(message, AnalysisTreeView.DELETE, new EventHandler(onDeleteBaseItem)));
            }
            else if (nodeTag.Type == NodeTag.NodeType.NT_CASEPALLETANALYSIS)
            {
                string message = string.Format(Resources.ID_EDIT, nodeTag.CasePalletAnalysis.Name);
                contextMenuStrip.Items.Add(new ToolStripMenuItem(message, null, new EventHandler(onEditCasePalletAnalysis)));
                message = string.Format(Resources.ID_DELETE, nodeTag.CasePalletAnalysis.Name);
                contextMenuStrip.Items.Add(new ToolStripMenuItem(message, AnalysisTreeView.DELETE, new EventHandler(onDeleteCasePalletAnalysis)));
            }
            else if (nodeTag.Type == NodeTag.NodeType.NT_PACKPALLETANALYSIS)
            { 
                string message = string.Format(Resources.ID_EDIT, nodeTag.PackPalletAnalysis.Name);
                contextMenuStrip.Items.Add(new ToolStripMenuItem(message, null, new EventHandler(onEditPackPalletAnalysis)));
                message = string.Format(Resources.ID_DELETE, nodeTag.PackPalletAnalysis.Name);
                contextMenuStrip.Items.Add(new ToolStripMenuItem(message, AnalysisTreeView.DELETE, new EventHandler(onDeletePackPalletAnalysis)));
            }
            else if (nodeTag.Type == NodeTag.NodeType.NT_CYLINDERPALLETANALYSIS)
            {
                string message = string.Format(Resources.ID_EDIT, nodeTag.CylinderPalletAnalysis.Name);
                contextMenuStrip.Items.Add(new ToolStripMenuItem(message, null, new EventHandler(onEditCylinderPalletAnalysis)));
                message = string.Format(Resources.ID_DELETE, nodeTag.CylinderPalletAnalysis.Name);
                contextMenuStrip.Items.Add(new ToolStripMenuItem(message, null, new EventHandler(onDeleteCylinderPalletAnalysis)));
            }
            else if (nodeTag.Type == NodeTag.NodeType.NT_HCYLINDERPALLETANALYSIS)
            {
                string message = string.Format(Resources.ID_EDIT, nodeTag.HCylinderPalletAnalysis.Name);
                contextMenuStrip.Items.Add(new ToolStripMenuItem(message, null, new EventHandler(onEditHCylinderPalletAnalysis)));
                message = string.Format(Resources.ID_DELETE, nodeTag.HCylinderPalletAnalysis.Name);
                contextMenuStrip.Items.Add(new ToolStripMenuItem(message, null, new EventHandler(onDeleteHCylinderPalletAnalysis)));
            }
            else if (nodeTag.Type == NodeTag.NodeType.NT_BOXCASEANALYSIS)
            {
                string message = string.Format(Resources.ID_EDIT, nodeTag.BoxCaseAnalysis.Name);
                contextMenuStrip.Items.Add(new ToolStripMenuItem(message, null, new EventHandler(onEditBoxCaseAnalysis)));
                message = string.Format(Resources.ID_DELETE, nodeTag.BoxCaseAnalysis.Name);
                contextMenuStrip.Items.Add(new ToolStripMenuItem(message, AnalysisTreeView.DELETE, new EventHandler(onDeleteBoxCaseAnalysis)));
            }
            else if (nodeTag.Type == NodeTag.NodeType.NT_TRUCKANALYSIS)
            {
                string message = string.Format(Resources.ID_EDIT, nodeTag.TruckAnalysis.Name);
                contextMenuStrip.Items.Add(new ToolStripMenuItem(message, null, new EventHandler(onEditTruckAnalysis)));
                message = string.Format(Resources.ID_DELETE, nodeTag.TruckAnalysis.Name);
                contextMenuStrip.Items.Add(new ToolStripMenuItem(message, AnalysisTreeView.DELETE, new EventHandler(onDeleteTruckAnalysis)));
            }
            else if (nodeTag.Type == NodeTag.NodeType.NT_ECTANALYSIS)
            {
                string message = string.Format(Resources.ID_EDIT, nodeTag.ECTAnalysis.Name);
                contextMenuStrip.Items.Add(new ToolStripMenuItem(message, null, new EventHandler(onEditECTAnalysis)));
                message = string.Format(Resources.ID_DELETE, nodeTag.ECTAnalysis.Name);
                contextMenuStrip.Items.Add(new ToolStripMenuItem(message, AnalysisTreeView.DELETE, new EventHandler(onDeleteECTAnalysis)));
            }
            else if (nodeTag.Type == NodeTag.NodeType.NT_LISTBOX)
                contextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.ID_ADDNEWBOX, AnalysisTreeView.Box, new EventHandler(onCreateNewBox)));
            else if (nodeTag.Type == NodeTag.NodeType.NT_LISTCASE)
                contextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.ID_ADDNEWCASE, AnalysisTreeView.Case, new EventHandler(onCreateNewCase)));
            else if (nodeTag.Type == NodeTag.NodeType.NT_LISTCYLINDER)
                contextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.ID_ADDNEWCYLINDER, AnalysisTreeView.Cylinder, new EventHandler(onCreateNewCylinder)));
            else if (nodeTag.Type == NodeTag.NodeType.NT_LISTPALLET)
                contextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.ID_ADDNEWPALLET, AnalysisTreeView.Pallet, new EventHandler(onCreateNewPallet)));
            else if (nodeTag.Type == NodeTag.NodeType.NT_LISTINTERLAYER)
                contextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.ID_ADDNEWINTERLAYER, AnalysisTreeView.Interlayer, new EventHandler(onCreateNewInterlayer)));
            else if (nodeTag.Type == NodeTag.NodeType.NT_LISTBUNDLE)
                contextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.ID_ADDNEWBUNDLE, AnalysisTreeView.Bundle, new EventHandler(onCreateNewBundle)));
            else if (nodeTag.Type == NodeTag.NodeType.NT_LISTTRUCK)
                contextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.ID_ADDNEWTRUCK, AnalysisTreeView.Truck, new EventHandler(onCreateNewTruck)));
            else if (nodeTag.Type == NodeTag.NodeType.NT_LISTPALLETCORNERS)
                contextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.ID_ADDNEWPALLETCORNERS, AnalysisTreeView.PalletCorners, new EventHandler(onCreateNewPalletCorners)));
            else if (nodeTag.Type == NodeTag.NodeType.NT_LISTPALLETCAP)
                contextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.ID_ADDNEWPALLETCAP, AnalysisTreeView.PalletCap, new EventHandler(onCreateNewPalletCap)));
            else if (nodeTag.Type == NodeTag.NodeType.NT_LISTPALLETFILM)
                contextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.ID_ADDNEWPALLETFILM, AnalysisTreeView.PalletFilm, new EventHandler(onCreateNewPalletFilm)));
            else if (nodeTag.Type == NodeTag.NodeType.NT_LISTANALYSIS)
            {
                if (nodeTag.Document.CanCreateCasePalletAnalysis)
                    contextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.ID_ADDNEWANALYSIS, AnalysisTreeView.Analysis, new EventHandler(onCreateNewCasePalletAnalysis)));
                if (nodeTag.Document.CanCreateBundlePalletAnalysis)
                    contextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.ID_ADDNEWBUNDLEANALYSIS, AnalysisTreeView.AnalysisBundle, new EventHandler(onCreateNewBundleAnalysis)));
                if (nodeTag.Document.CanCreatePackPalletAnalysis)
                    contextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.ID_ADDNEWPACKANALYSIS, AnalysisTreeView.PackAnalysis, new EventHandler(onCreateNewPackPalletAnalysis)));
                if (nodeTag.Document.CanCreateBoxCaseAnalysis)
                    contextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.ID_ADDNEWBOXCASEANALYSIS, AnalysisTreeView.BoxCaseAnalysis, new EventHandler(onCreateNewBoxCaseAnalysis)));
                if (nodeTag.Document.CanCreateBoxCasePalletAnalysis)
                    contextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.ID_ADDNEWCASEANALYSIS, AnalysisTreeView.CaseAnalysis, new EventHandler(onCreateNewCaseAnalysis)));
                if (nodeTag.Document.CanCreateCylinderPalletAnalysis)
                    contextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.ID_ADDNEWCYLINDERANALYSIS, AnalysisTreeView.CylinderPalletAnalysis, new EventHandler(onCreateNewCylinderPalletAnalysis)));
            }
            else if (nodeTag.Type == NodeTag.NodeType.NT_CASEPALLETANALYSISSOLUTION)
            {
                contextMenuStrip.Items.Add(new ToolStripMenuItem(string.Format(Resources.ID_UNSELECTSOLUTION, nodeTag.SelSolution.Solution.Title), AnalysisTreeView.DELETE, new EventHandler(onUnselectCasePalletAnalysisSolution)));
                if (nodeTag.Document.Trucks.Count > 0 && !nodeTag.SelSolution.HasDependingAnalyses)
                    contextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.ID_ADDNEWTRUCKANALYSIS, AnalysisTreeView.TruckAnalysis, new EventHandler(onCreateNewTruckAnalysis)));
                if (!nodeTag.SelSolution.HasECTAnalyses)
                    contextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.ID_ADDNEWECTANALYSIS, AnalysisTreeView.AnalysisStackingStrength, new EventHandler(onCreateNewECTAnalysis)));
                if (nodeTag.CasePalletAnalysis.IsBoxAnalysis)
                {
                    BoxProperties bProperties = nodeTag.CasePalletAnalysis.BProperties as BoxProperties;
                    if (bProperties.HasInsideDimensions) // BoxProperties must also have inside dimensions
                        contextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.ID_SENDTODATABASE, AnalysisTreeView.Database, new EventHandler(onSendSolutionToDatabase)));
                }
                string message = string.Format(Resources.ID_GENERATEREPORTHTML, nodeTag.SelSolution.Name);
                contextMenuStrip.Items.Add(new ToolStripMenuItem(message, AnalysisTreeView.HTML, new EventHandler(onAnalysisReportHTML)));
                message = string.Format(Resources.ID_GENERATEREPORTMSWORD, nodeTag.SelSolution.Name);
                contextMenuStrip.Items.Add(new ToolStripMenuItem(message, AnalysisTreeView.WORD, new EventHandler(onAnalysisReportMSWord)));
                message = string.Format(Resources.ID_GENERATEREPORTPDF, nodeTag.SelSolution.Name);
                if (nodeTag.CasePalletAnalysis.IsBoxAnalysis && Settings.Default.ShowLogConsole)
                {
                    message = string.Format(Resources.ID_GENERATECOLLADA, nodeTag.SelSolution.Name);
                    contextMenuStrip.Items.Add(new ToolStripMenuItem(message, AnalysisTreeView.COLLADAWEBGL, new EventHandler(onAnalysisExportCollada)));
                }
            }
            else if (nodeTag.Type == NodeTag.NodeType.NT_PACKPALLETANALYSISSOLUTION)
            { 
                string message = string.Format(Resources.ID_GENERATEREPORTHTML, nodeTag.SelPackPalletSolution.Name);
                contextMenuStrip.Items.Add(new ToolStripMenuItem(message, AnalysisTreeView.HTML, new EventHandler(onAnalysisReportHTML)));
                message = string.Format(Resources.ID_GENERATEREPORTMSWORD, nodeTag.SelPackPalletSolution.Name);
                contextMenuStrip.Items.Add(new ToolStripMenuItem(message, AnalysisTreeView.WORD, new EventHandler(onAnalysisReportMSWord)));
                message = string.Format(Resources.ID_GENERATEREPORTPDF, nodeTag.SelPackPalletSolution.Name);
            }
            else if (nodeTag.Type == NodeTag.NodeType.NT_CYLINDERPALLETANALYSISSOLUTION)
            {
                contextMenuStrip.Items.Add(new ToolStripMenuItem(string.Format(Resources.ID_UNSELECTSOLUTION, nodeTag.SelCylinderPalletSolution.Solution.Title)
                    , AnalysisTreeView.DELETE, new EventHandler(onUnselectCylinderPalletAnalysisSolution)));
                string message = string.Format(Resources.ID_GENERATEREPORTHTML, nodeTag.SelCylinderPalletSolution.Name);
                contextMenuStrip.Items.Add(new ToolStripMenuItem(message, AnalysisTreeView.HTML, new EventHandler(onAnalysisReportHTML)));
                message = string.Format(Resources.ID_GENERATEREPORTMSWORD, nodeTag.SelCylinderPalletSolution.Name);
                contextMenuStrip.Items.Add(new ToolStripMenuItem(message, AnalysisTreeView.WORD, new EventHandler(onAnalysisReportMSWord)));
            }
            else if (nodeTag.Type == NodeTag.NodeType.NT_HCYLINDERPALLETANALYSISSOLUTION)
            {
                contextMenuStrip.Items.Add(new ToolStripMenuItem(string.Format(Resources.ID_UNSELECTSOLUTION, nodeTag.SelHCylinderPalletSolution.Solution.Title)
                    , AnalysisTreeView.DELETE, new EventHandler(onUnselectHCylinderPalletAnalysisSolution)));
                string message = string.Format(Resources.ID_GENERATEREPORTHTML, nodeTag.SelHCylinderPalletSolution.Name);
                contextMenuStrip.Items.Add(new ToolStripMenuItem(message, AnalysisTreeView.HTML, new EventHandler(onAnalysisReportHTML)));
                message = string.Format(Resources.ID_GENERATEREPORTMSWORD, nodeTag.SelHCylinderPalletSolution.Name);
                contextMenuStrip.Items.Add(new ToolStripMenuItem(message, AnalysisTreeView.WORD, new EventHandler(onAnalysisReportMSWord)));
            }
            else if (nodeTag.Type == NodeTag.NodeType.NT_BOXCASEANALYSISSOLUTION)
            {
                contextMenuStrip.Items.Add(new ToolStripMenuItem(string.Format(Resources.ID_UNSELECTSOLUTION, nodeTag.SelBoxCaseSolution.Solution.Title), AnalysisTreeView.DELETE, new EventHandler(onUnselectBoxCaseAnalysisSolution)));
                string message = string.Format(Resources.ID_GENERATEREPORTHTML, nodeTag.SelBoxCaseSolution.Name);
                contextMenuStrip.Items.Add(new ToolStripMenuItem(message, AnalysisTreeView.HTML, new EventHandler(onAnalysisReportHTML)));
                message = string.Format(Resources.ID_GENERATEREPORTMSWORD, nodeTag.SelBoxCaseSolution.Name);
                contextMenuStrip.Items.Add(new ToolStripMenuItem(message, AnalysisTreeView.WORD, new EventHandler(onAnalysisReportMSWord)));
            }
            else if (nodeTag.Type == NodeTag.NodeType.NT_BOXCASEPALLETANALYSIS)
            {
                contextMenuStrip.Items.Add(new ToolStripMenuItem(string.Format(Resources.ID_EDIT, nodeTag.CaseAnalysis.Name), null, new EventHandler(onEditCaseAnalysis)));
                contextMenuStrip.Items.Add(new ToolStripMenuItem(string.Format(Resources.ID_DELETE, nodeTag.CaseAnalysis.Name), AnalysisTreeView.DELETE, new EventHandler(onDeleteCaseAnalysis)));
            }
            else if (nodeTag.Type == NodeTag.NodeType.NT_CASESOLUTION)
            {
                contextMenuStrip.Items.Add(new ToolStripMenuItem(string.Format(Resources.ID_UNSELECTSOLUTION, nodeTag.SelBoxCasePalletSolution.Solution.Title), AnalysisTreeView.DELETE, new EventHandler(onUnselectBoxCasePalletAnalysisSolution)));
                string message = string.Format(Resources.ID_GENERATEREPORTHTML, nodeTag.SelBoxCasePalletSolution.Name);
                contextMenuStrip.Items.Add(new ToolStripMenuItem(message, AnalysisTreeView.HTML, new EventHandler(onAnalysisReportHTML)));
                message = string.Format(Resources.ID_GENERATEREPORTMSWORD, nodeTag.SelBoxCasePalletSolution.Name);
                contextMenuStrip.Items.Add(new ToolStripMenuItem(message, AnalysisTreeView.WORD, new EventHandler(onAnalysisReportMSWord)));
            }
        }
        #endregion

        #region Handling context menus
        private void onDeleteBaseItem(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;                
                tag.Document.RemoveItem(tag.ItemProperties);
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onEditCasePalletAnalysis(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                ((DocumentSB)tag.Document).EditCasePalletAnalysis(tag.CasePalletAnalysis);
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onEditPackPalletAnalysis(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                ((DocumentSB)tag.Document).EditPackPalletAnalsyis(tag.PackPalletAnalysis);
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }

        private void onEditCylinderPalletAnalysis(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                ((DocumentSB)tag.Document).EditCylinderPalletAnalysis(tag.CylinderPalletAnalysis);
            }
            catch (Exception ex) { _log.Error(ex.ToString()); } 
        }
        private void onEditHCylinderPalletAnalysis(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                ((DocumentSB)tag.Document).EditHCylinderPalletAnalysis(tag.HCylinderPalletAnalysis);
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onEditBoxCaseAnalysis(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                ((DocumentSB)tag.Document).EditBoxCaseAnalysis(tag.BoxCaseAnalysis);
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onEditCaseAnalysis(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                ((DocumentSB)tag.Document).EditCaseAnalysis(tag.CaseAnalysis);
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onDeleteCasePalletAnalysis(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                tag.Document.RemoveItem(tag.CasePalletAnalysis);
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onDeletePackPalletAnalysis(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                tag.Document.RemoveItem(tag.PackPalletAnalysis);
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onDeleteCylinderPalletAnalysis(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                tag.Document.RemoveItem(tag.CylinderPalletAnalysis);
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onDeleteHCylinderPalletAnalysis(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                tag.Document.RemoveItem(tag.HCylinderPalletAnalysis);
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onDeleteBoxCaseAnalysis(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                tag.Document.RemoveItem(tag.BoxCaseAnalysis);
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onAnalysisReportMSWord(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                SolutionReportMSWordClicked(this, new AnalysisTreeViewEventArgs(tag));
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onAnalysisReportPdf(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                SolutionReportPdfClicked(this, new AnalysisTreeViewEventArgs(tag));
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onAnalysisReportHTML(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                SolutionReportHtmlClicked(this, new AnalysisTreeViewEventArgs(tag));
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onAnalysisExportCollada(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                SolutionColladaExportClicked(this, new AnalysisTreeViewEventArgs(tag));
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onEditTruckAnalysis(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                ((DocumentSB)tag.Document).EditTruckAnalysis(tag.TruckAnalysis);
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onEditECTAnalysis(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                AnalysisNodeClicked(this, new AnalysisTreeViewEventArgs(tag));
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onDeleteTruckAnalysis(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                tag.SelSolution.RemoveTruckAnalysis(tag.TruckAnalysis);
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onDeleteECTAnalysis(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                tag.SelSolution.RemoveECTAnalysis(tag.ECTAnalysis);
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onDeleteCaseAnalysis(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                tag.Document.RemoveItem(tag.CaseAnalysis);
            }
            catch (Exception ex) { _log.Error(ex.ToString()); } 
        }
        private void onCreateNewBox(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                ((DocumentSB)tag.Document).CreateNewBoxUI();
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onCreateNewCase(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                ((DocumentSB)tag.Document).CreateNewCaseUI();
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onCreateNewPack(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                ((DocumentSB)tag.Document).CreateNewPackUI();
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onCreateNewCylinder(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                ((DocumentSB)tag.Document).CreateNewCylinderUI();
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onCreateNewPallet(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                ((DocumentSB)tag.Document).CreateNewPalletUI();
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onCreateNewInterlayer(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                ((DocumentSB)tag.Document).CreateNewInterlayerUI();
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onCreateNewBundle(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                ((DocumentSB)tag.Document).CreateNewBundleUI();
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onCreateNewTruck(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                ((DocumentSB)tag.Document).CreateNewTruckUI();
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onCreateNewPalletCorners(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                ((DocumentSB)tag.Document).CreateNewPalletCornersUI();
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onCreateNewPalletCap(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                ((DocumentSB)tag.Document).CreateNewPalletCapUI();
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onCreateNewPalletFilm(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                ((DocumentSB)tag.Document).CreateNewPalletFilmUI();
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onCreateNewCasePalletAnalysis(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                ((DocumentSB)tag.Document).CreateNewCasePalletAnalysisUI();
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onCreateNewPackPalletAnalysis(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                ((DocumentSB)tag.Document).CreateNewPackPalletAnalysisUI();
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onCreateNewBundleAnalysis(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                ((DocumentSB)tag.Document).CreateNewBundlePalletAnalysisUI();
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onCreateNewBoxCaseAnalysis(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                ((DocumentSB)tag.Document).CreateNewBoxCaseAnalysisUI();
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onCreateNewCylinderPalletAnalysis(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                ((DocumentSB)tag.Document).CreateNewCylinderPalletAnalysisUI();
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onCreateNewHCylinderPalletAnalysis(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                ((DocumentSB)tag.Document).CreateNewHCylinderPalletAnalysisUI();
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onCreateNewTruckAnalysis(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;

                if (tag.SelSolution.HasDependingAnalyses)
                    return;

                FormNewTruckAnalysis form = new FormNewTruckAnalysis(tag.Document);
                form.Trucks = tag.Document.Trucks.ToArray();
                if (DialogResult.OK == form.ShowDialog())
                {
                    TruckConstraintSet constraintSet = new TruckConstraintSet();
                    constraintSet.MultilayerAllowed = form.AllowSeveralPalletLayers;
                    constraintSet.AllowPalletOrientationX = form.AllowPalletOrientationX;
                    constraintSet.AllowPalletOrientationY = form.AllowPalletOrientationY;
                    constraintSet.MinDistancePalletTruckWall = form.MinDistancePalletTruckWall;
                    constraintSet.MinDistancePalletTruckRoof = form.MinDistancePalletTruckRoof;

                    TruckAnalysis truckAnalysis = tag.SelSolution.CreateNewTruckAnalysis(form.SelectedTruck.Name, string.Empty, form.SelectedTruck, constraintSet, new TruckSolver());
                    if (null != truckAnalysis)
                        FormMain.GetInstance().CreateOrActivateViewTruckAnalysis(truckAnalysis);
                }
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }       
        }
        private void onSendSolutionToDatabase(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                // get PalletSolutionDatabase instance
                PalletSolutionDatabase db = PalletSolutionDatabase.Instance;
                // check that a comparable solution is not already in database
                BoxProperties boxProperties = tag.CasePalletAnalysis.BProperties as BoxProperties;
                PalletProperties palletProperties = tag.CasePalletAnalysis.PalletProperties;
                PalletConstraintSet constraintSet = tag.CasePalletAnalysis.ConstraintSet;
                // show form and get friendly name
                FormAppendSolutionToDB form = new FormAppendSolutionToDB();
                // warn user : keep or replace similar solutions
                form.ShowSimilarSolutionQuestion =db.AlreadyHasSimilarSolution(tag.SelSolution);
                // show dialog
                if (DialogResult.Cancel == form.ShowDialog())
                    return;
                // save in database index
                db.Append(tag.SelSolution, form.FriendlyName, form.KeepSimilarSolutions); 
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }

        private void onCreateNewECTAnalysis(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;

                if (tag.SelSolution.HasECTAnalyses)
                    return;

                SelCasePalletSolution selSolution = tag.SelSolution;
                CasePalletAnalysis analysis = selSolution.Analysis;

                ECTAnalysis ectAnalysis = selSolution.CreateNewECTAnalysis(analysis.Name, analysis.Description);
                if (null != ectAnalysis)
                    FormMain.GetInstance().CreateOrActivateViewECTAnalysis(ectAnalysis);
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }

        private void onCreateNewCaseAnalysis(object sender, EventArgs e)
        {
            NodeTag tag = SelectedNode.Tag as NodeTag;
            ((DocumentSB)tag.Document).CreateNewBoxCasePalletOptimizationUI(); 
        }
        private void onUnselectCasePalletAnalysisSolution(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                tag.CasePalletAnalysis.UnSelectSolution(tag.SelSolution);
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onUnselectCylinderPalletAnalysisSolution(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                tag.CylinderPalletAnalysis.UnSelectSolution(tag.SelCylinderPalletSolution);
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onUnselectHCylinderPalletAnalysisSolution(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                tag.HCylinderPalletAnalysis.UnSelectSolution(tag.SelHCylinderPalletSolution);
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onUnselectBoxCaseAnalysisSolution(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                tag.BoxCaseAnalysis.UnSelectSolution(tag.SelBoxCaseSolution);
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onUnselectBoxCasePalletAnalysisSolution(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                tag.CaseAnalysis.UnSelectSolution(tag.SelBoxCasePalletSolution);
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        private void onDocumentClose(object sender, EventArgs e)
        {
            try
            {
                NodeTag tag = SelectedNode.Tag as NodeTag;
                CancelEventArgs cea = new CancelEventArgs();
                FormMain.GetInstance().CloseDocument((DocumentSB)tag.Document, cea); ;
            }
            catch (Exception ex) { _log.Error(ex.ToString()); }
        }
        #endregion

        #region Event handlers
        void AnalysisTreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                SelectedNode = e.Node;
                // handle only left mouse button click
                if (e.Button != MouseButtons.Left) return;
                NodeTag tag = CurrentTag;
                NodeTag.NodeType tagType = tag.Type;
                if (null != AnalysisNodeClicked &&
                    (tag.Type == NodeTag.NodeType.NT_CASEPALLETANALYSIS)
                    || (tag.Type == NodeTag.NodeType.NT_PACKPALLETANALYSIS)
                    || (tag.Type == NodeTag.NodeType.NT_TRUCKANALYSIS)
                    || (tag.Type == NodeTag.NodeType.NT_BOXCASEANALYSIS)
                    || (tag.Type == NodeTag.NodeType.NT_BOXCASEPALLETANALYSIS)
                    || (tag.Type == NodeTag.NodeType.NT_ECTANALYSIS)
                    || (tag.Type == NodeTag.NodeType.NT_ANALYSISBOX)
                    || (tag.Type == NodeTag.NodeType.NT_ANALYSISPALLET)
                    || (tag.Type == NodeTag.NodeType.NT_CYLINDERPALLETANALYSIS)
                    || (tag.Type == NodeTag.NodeType.NT_HCYLINDERPALLETANALYSIS)
                    || (tag.Type == NodeTag.NodeType.NT_ANALYSISINTERLAYER)
                    || (tag.Type == NodeTag.NodeType.NT_BOX)
                    || (tag.Type == NodeTag.NodeType.NT_CASE)
                    || (tag.Type == NodeTag.NodeType.NT_PACK)
                    || (tag.Type == NodeTag.NodeType.NT_BUNDLE)
                    || (tag.Type == NodeTag.NodeType.NT_CYLINDER)
                    || (tag.Type == NodeTag.NodeType.NT_CASEOFBOXES)
                    || (tag.Type == NodeTag.NodeType.NT_PALLET)
                    || (tag.Type == NodeTag.NodeType.NT_INTERLAYER)
                    || (tag.Type == NodeTag.NodeType.NT_PALLETCORNERS)
                    || (tag.Type == NodeTag.NodeType.NT_PALLETCAP)
                    || (tag.Type == NodeTag.NodeType.NT_PALLETFILM)
                    || (tag.Type == NodeTag.NodeType.NT_TRUCK)
                    )
                {
                    AnalysisNodeClicked(this, new AnalysisTreeViewEventArgs(tag));
                    e.Node.Expand();
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
            }
        }
        void AnalysisTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
        }
        void AnalysisTreeView_DrawNode(object sender, System.Windows.Forms.DrawTreeNodeEventArgs e)
        {
            try
            {
                // get NodeTag
                NodeTag tag = e.Node.Tag as NodeTag;
                if (null == tag)
                    throw new Exception(string.Format("Node {0} has null tag", e.Node.Text));
                Rectangle nodeBounds = e.Node.Bounds;
                if (null != tag.ItemProperties)
                    TextRenderer.DrawText(e.Graphics, tag.ItemProperties.Name, Font, nodeBounds, System.Drawing.Color.Black, Color.Transparent, TextFormatFlags.VerticalCenter | TextFormatFlags.NoClipping);
                else
                    TextRenderer.DrawText(e.Graphics, e.Node.Text, Font, nodeBounds, System.Drawing.Color.Black, Color.Transparent, TextFormatFlags.VerticalCenter | TextFormatFlags.NoClipping);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
            }
        }
        #endregion

        #region Helpers
        internal NodeTag CurrentTag
        {
            get
            {
                TreeNode currentNode = this.SelectedNode;
                if (null == currentNode)
                    throw new Exception("No node selected");
                return currentNode.Tag as NodeTag;
            }
        }
        internal TreeNode FindNode(TreeNode node, NodeTag nodeTag)
        {
            // check with node itself
            if (null != node)
            {
                NodeTag tag = node.Tag as NodeTag;
                if (null == tag)
                {
                    _log.Error(string.Format("Node {0} has no valid NodeTag", node.Text));
                    return null;
                }
                if (tag.Equals(nodeTag))
                    return node;
            }
            // check with child nodes
            TreeNodeCollection tnCollection = null == node ? Nodes : node.Nodes;
            foreach (TreeNode tn in tnCollection)
            {
                TreeNode tnResult = FindNode(tn, nodeTag);
                if (null != tnResult)
                    return tnResult;
            }
            return null;
        }
        #endregion

        #region Delegates
        /// <summary>
        /// is a prototype for event handlers of AnalysisNodeClicked / SolutionReportNodeClicked
        /// </summary>
        /// <param name="sender">sending object (tree)</param>
        /// <param name="eventArg">contains NodeTag to identify clicked TreeNode</param>
        public delegate void AnalysisNodeClickHandler(object sender, AnalysisTreeViewEventArgs eventArg);
        #endregion

        #region Events
        /// <summary>
        /// event raised when an analysis node is clicked
        /// </summary>
        public event AnalysisNodeClickHandler AnalysisNodeClicked;
        /// <summary>
        /// event raised when a selected solution node is clicked
        /// </summary>
        public event AnalysisNodeClickHandler SolutionReportMSWordClicked;
        public event AnalysisNodeClickHandler SolutionReportHtmlClicked;
        public event AnalysisNodeClickHandler SolutionReportPdfClicked;
        public event AnalysisNodeClickHandler SolutionColladaExportClicked;
        #endregion

        #region IDocumentListener implementation
        /// <summary>
        /// handles new document creation
        /// </summary>
        /// <param name="doc"></param>
        public void OnNewDocument(Document doc)
        {
            // add document node
            TreeNode nodeDoc = new TreeNode(doc.Name, 2, 2);
            nodeDoc.Tag = new NodeTag(NodeTag.NodeType.NT_DOCUMENT, doc);
            this.Nodes.Add(nodeDoc);
            // add box list node
            TreeNode nodeBoxes = new TreeNode(Resources.ID_NODE_BOXES, 0, 1);
            nodeBoxes.Tag = new NodeTag(NodeTag.NodeType.NT_LISTBOX, doc);
            nodeDoc.Nodes.Add(nodeBoxes);
            // add case list node
            TreeNode nodeCases = new TreeNode(Resources.ID_NODE_CASES, 0, 1);
            nodeCases.Tag = new NodeTag(NodeTag.NodeType.NT_LISTCASE, doc);
            nodeDoc.Nodes.Add(nodeCases);
            // add pack list node
            TreeNode nodePacks = new TreeNode(Resources.ID_NODE_PACKS, 0, 1);
            nodePacks.Tag = new NodeTag(NodeTag.NodeType.NT_LISTPACK, doc);
            nodeDoc.Nodes.Add(nodePacks);
            // add bundle list node
            TreeNode nodeBundles = new TreeNode(Resources.ID_NODE_BUNDLES, 0, 1);
            nodeBundles.Tag = new NodeTag(NodeTag.NodeType.NT_LISTBUNDLE, doc);
            nodeDoc.Nodes.Add(nodeBundles);
            // add cylinder list node
            TreeNode nodeCylinders = new TreeNode(Resources.ID_NODE_CYLINDERS, 0, 1);
            nodeCylinders.Tag = new NodeTag(NodeTag.NodeType.NT_LISTCYLINDER, doc);
            nodeDoc.Nodes.Add(nodeCylinders);
            // add pallet list node
            TreeNode nodeInterlayers = new TreeNode(Resources.ID_NODE_INTERLAYERS, 0, 1);
            nodeInterlayers.Tag = new NodeTag(NodeTag.NodeType.NT_LISTINTERLAYER, doc);
            nodeDoc.Nodes.Add(nodeInterlayers);
            // add pallet list node
            TreeNode nodePallets = new TreeNode(Resources.ID_NODE_PALLETS, 0, 1);
            nodePallets.Tag = new NodeTag(NodeTag.NodeType.NT_LISTPALLET, doc);
            nodeDoc.Nodes.Add(nodePallets);
            // add pallet corners list node
            TreeNode nodePalletCorners = new TreeNode(Resources.ID_NODE_PALLETCORNERS, 0, 1);
            nodePalletCorners.Tag = new NodeTag(NodeTag.NodeType.NT_LISTPALLETCORNERS, doc);
            nodeDoc.Nodes.Add(nodePalletCorners);
            // add pallet cap node
            TreeNode nodePalletCaps = new TreeNode(Resources.ID_NODE_PALLETCAPS, 0, 1);
            nodePalletCaps.Tag = new NodeTag(NodeTag.NodeType.NT_LISTPALLETCAP, doc);
            nodeDoc.Nodes.Add(nodePalletCaps);
            // add pallet film node
            TreeNode nodePalletFilms = new TreeNode(Resources.ID_NODE_PALLETFILMS, 0, 1);
            nodePalletFilms.Tag = new NodeTag(NodeTag.NodeType.NT_LISTPALLETFILM, doc);
            nodeDoc.Nodes.Add(nodePalletFilms);
            // add truck list node
            TreeNode nodeTrucks = new TreeNode(Resources.ID_NODE_TRUCKS, 0, 1);
            nodeTrucks.Tag = new NodeTag(NodeTag.NodeType.NT_LISTTRUCK, doc);
            nodeDoc.Nodes.Add(nodeTrucks);
            // add analysis list node
            TreeNode nodeAnalyses = new TreeNode(Resources.ID_NODE_ANALYSES, 0, 1);
            nodeAnalyses.Tag = new NodeTag(NodeTag.NodeType.NT_LISTANALYSIS, doc);
            nodeDoc.Nodes.Add(nodeAnalyses);
            nodeDoc.Expand();
        }
        /// <summary>
        /// handles new type creation
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="itemProperties"></param>
        public void OnNewTypeCreated(Document doc, ItemBase itemProperties)
        {
            int iconIndex = 0;
            NodeTag.NodeType nodeType = NodeTag.NodeType.NT_BOX;
            NodeTag.NodeType parentNodeType = NodeTag.NodeType.NT_LISTBOX;
            if (itemProperties.GetType() == typeof(CaseOfBoxesProperties))
            {
                iconIndex = 17;
                nodeType = NodeTag.NodeType.NT_CASEOFBOXES;
                parentNodeType = NodeTag.NodeType.NT_LISTCASE;
            }
            else if (itemProperties.GetType() == typeof(BoxProperties))
            {
                BoxProperties boxProperties = itemProperties as BoxProperties;
                if (boxProperties.HasInsideDimensions)
                {
                    iconIndex = 4;
                    nodeType = NodeTag.NodeType.NT_CASE;
                    parentNodeType = NodeTag.NodeType.NT_LISTCASE;
                }
                else 
                {
                    iconIndex = 3;
                    nodeType = NodeTag.NodeType.NT_BOX;
                    parentNodeType = NodeTag.NodeType.NT_LISTBOX;
                }                
            }
            else if (itemProperties.GetType() == typeof(BundleProperties))
            {
                iconIndex = 5;
                nodeType = NodeTag.NodeType.NT_BUNDLE;
                parentNodeType = NodeTag.NodeType.NT_LISTBUNDLE;
            }
            else if (itemProperties.GetType() == typeof(CylinderProperties))
            {
                iconIndex = 6;
                nodeType = NodeTag.NodeType.NT_CYLINDER;
                parentNodeType = NodeTag.NodeType.NT_LISTCYLINDER;
            }
            else if (itemProperties.GetType() == typeof(PalletProperties))
            {
                iconIndex = 7;
                nodeType = NodeTag.NodeType.NT_PALLET;
                parentNodeType = NodeTag.NodeType.NT_LISTPALLET;
            }
            else if (itemProperties.GetType() == typeof(InterlayerProperties))
            {
                iconIndex = 8;
                nodeType = NodeTag.NodeType.NT_INTERLAYER;
                parentNodeType = NodeTag.NodeType.NT_LISTINTERLAYER;
            }
            else if (itemProperties.GetType() == typeof(TruckProperties))
            {
                iconIndex = 9;
                nodeType = NodeTag.NodeType.NT_TRUCK;
                parentNodeType = NodeTag.NodeType.NT_LISTTRUCK;
            }
            else if (itemProperties.GetType() == typeof(PalletCornerProperties))
            {
                iconIndex = 10;
                nodeType = NodeTag.NodeType.NT_PALLETCORNERS;
                parentNodeType = NodeTag.NodeType.NT_LISTPALLETCORNERS;
            }
            else if (itemProperties.GetType() == typeof(PalletCapProperties))
            {
                iconIndex = 11;
                nodeType = NodeTag.NodeType.NT_PALLETCAP;
                parentNodeType = NodeTag.NodeType.NT_LISTPALLETCAP;
            }
            else if (itemProperties.GetType() == typeof(PalletFilmProperties))
            {
                iconIndex = 12;
                nodeType = NodeTag.NodeType.NT_PALLETFILM;
                parentNodeType = NodeTag.NodeType.NT_LISTPALLETFILM;
            }
            else if (itemProperties.GetType() == typeof(PackProperties))
            {
                iconIndex = 22;
                nodeType = NodeTag.NodeType.NT_PACK;
                parentNodeType = NodeTag.NodeType.NT_LISTPACK;
            }
            else
            {
                Debug.Assert(false);
                _log.Error("AnalysisTreeView.OnNewTypeCreated() -> unknown type!");
                return;
            }
            // get parent node
            TreeNode parentNode = FindNode(null, new NodeTag(parentNodeType, doc));
            if (null == parentNode)
            { 
                _log.Error(string.Format("Failed to load parentNode for {0}", itemProperties.Name));
                return;
            }
            // instantiate node
            TreeNode nodeItem = new TreeNode(itemProperties.Name, iconIndex, iconIndex);
            // set node tag
            nodeItem.Tag = new NodeTag(nodeType, doc, itemProperties);
            // insert
            parentNode.Nodes.Add(nodeItem);
            parentNode.Expand();
            // if item is CaseOfBoxesProperties
            if (itemProperties is CaseOfBoxesProperties)
            {
                // insert sub node
                CaseOfBoxesProperties caseOfBoxesProperties = itemProperties as CaseOfBoxesProperties;
                TreeNode subNode = new TreeNode(caseOfBoxesProperties.InsideBoxProperties.Name, 3, 3);
                subNode.Tag = new NodeTag(NodeTag.NodeType.NT_BOX, doc, caseOfBoxesProperties.InsideBoxProperties);
                nodeItem.Nodes.Add(subNode);
            }
        }
        public void OnNewAnalysisCreated(Document doc, Analysis analysis)
        { 
        }

        #region Case/Pallet analyses
        /// <summary>
        /// handles new analysis created
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="analysis"></param>
        public void OnNewCasePalletAnalysisCreated(Document doc, CasePalletAnalysis analysis)
        {
            // get parent node
            TreeNode parentNode = FindNode(null, new NodeTag(NodeTag.NodeType.NT_LISTANALYSIS, doc));
            // insert analysis node
            int indexIconAnalysis = analysis.IsBoxAnalysis ? 13 : 14;
            TreeNode nodeAnalysis = new TreeNode(analysis.Name, indexIconAnalysis, indexIconAnalysis);
            nodeAnalysis.Tag = new NodeTag(NodeTag.NodeType.NT_CASEPALLETANALYSIS, doc, analysis);
            parentNode.Nodes.Add(nodeAnalysis);
            parentNode.Expand();
            CasePalletAnalysis_InsertSubNodes(doc, analysis, nodeAnalysis);
            // add event handlers for solution selection
            analysis.Modified += new CasePalletAnalysis.ModifyAnalysis(onCasePalletAnalysisModified);
            analysis.SolutionSelected += new CasePalletAnalysis.SelectSolution(onPalletAnalysisSolutionSelected);
            analysis.SolutionSelectionRemoved += new CasePalletAnalysis.SelectSolution(onPalletAnalysisSolutionSelectionRemoved);
        }
        void CasePalletAnalysis_InsertSubNodes(Document doc, CasePalletAnalysis analysis, TreeNode nodeAnalysis)
        {
            // sanity check
            if (null == nodeAnalysis) return;
            // remove any existing subnodes
            nodeAnalysis.Nodes.Clear();
            // insert sub box node
            int indexIconBoxAnalysis = 4;
            if (analysis.BProperties is CaseOfBoxesProperties)
                indexIconBoxAnalysis = 18;
            else if (analysis.BProperties is BoxProperties)
                indexIconBoxAnalysis = 4;
            else if (analysis.BProperties is BundleProperties)
                indexIconBoxAnalysis = 5;
            TreeNode subBoxNode = new TreeNode(analysis.BProperties.Name, indexIconBoxAnalysis, indexIconBoxAnalysis);
            subBoxNode.Tag = new NodeTag(NodeTag.NodeType.NT_ANALYSISBOX, doc, analysis, analysis.BProperties);
            nodeAnalysis.Nodes.Add(subBoxNode);
            // insert sub pallet node
            TreeNode subPalletNode = new TreeNode(analysis.PalletProperties.Name, 7, 7);
            subPalletNode.Tag = new NodeTag(NodeTag.NodeType.NT_ANALYSISPALLET, doc, analysis, analysis.PalletProperties);
            nodeAnalysis.Nodes.Add(subPalletNode);
            // insert sub interlayer node if any
            if (analysis.HasInterlayer)
            {
                TreeNode subInterlayer = new TreeNode(analysis.InterlayerProperties.Name, 8, 8);
                subInterlayer.Tag = new NodeTag(NodeTag.NodeType.NT_ANALYSISINTERLAYER, doc, analysis, analysis.InterlayerProperties);
                nodeAnalysis.Nodes.Add(subInterlayer);
            }
            if (analysis.HasInterlayerAntiSlip && (analysis.InterlayerProperties != analysis.InterlayerPropertiesAntiSlip))
            {
                TreeNode subInterlayer = new TreeNode(analysis.InterlayerPropertiesAntiSlip.Name, 8, 8);
                subInterlayer.Tag = new NodeTag(NodeTag.NodeType.NT_ANALYSISINTERLAYER, doc, analysis, analysis.InterlayerPropertiesAntiSlip);
                nodeAnalysis.Nodes.Add(subInterlayer);            
            }
            if (analysis.HasPalletCorners)
            {
                TreeNode subPalletCorners = new TreeNode(analysis.PalletCornerProperties.Name, 10, 10);
                subPalletCorners.Tag = new NodeTag(NodeTag.NodeType.NT_ANALYSISPALLETCORNERS, doc, analysis, analysis.PalletCornerProperties);
                nodeAnalysis.Nodes.Add(subPalletCorners);
            }
            if (analysis.HasPalletCap)
            {
                TreeNode subPalletCap = new TreeNode(analysis.PalletCapProperties.Name, 11, 11);
                subPalletCap.Tag = new NodeTag(NodeTag.NodeType.NT_ANALYSISPALLETCAP, doc, analysis, analysis.PalletCapProperties);
                nodeAnalysis.Nodes.Add(subPalletCap);
            }
            if (analysis.HasPalletFilm)
            {
                TreeNode subPalletFilm = new TreeNode(analysis.PalletFilmProperties.Name, 12, 12);
                subPalletFilm.Tag = new NodeTag(NodeTag.NodeType.NT_ANALYSISPALLETFILM, doc, analysis, analysis.PalletFilmProperties);
                nodeAnalysis.Nodes.Add(subPalletFilm);
            }
            nodeAnalysis.Expand();        
        }
        #endregion
        #region PackPalletAnalysis
        public void OnNewPackPalletAnalysisCreated(Document doc, PackPalletAnalysis analysis)
        {
            // get parent node
            TreeNode parentNode = FindNode(null, new NodeTag(NodeTag.NodeType.NT_LISTANALYSIS, doc));
            // insert analysis node
            int indexIconAnalysis = 23;
            TreeNode nodeAnalysis = new TreeNode(analysis.Name, indexIconAnalysis, indexIconAnalysis);
            nodeAnalysis.Tag = new NodeTag(NodeTag.NodeType.NT_PACKPALLETANALYSIS, doc, analysis);
            parentNode.Nodes.Add(nodeAnalysis);
            parentNode.Expand();
            PackPalletAnalysis_InsertSubNodes(doc, analysis, nodeAnalysis);
            // add event handlers for solution selection
            analysis.Modified += new PackPalletAnalysis.ModifyAnalysis(onPackPalletAnalysisModified);
            analysis.SolutionSelected += new PackPalletAnalysis.SelectSolution(onPackPalletAnalysisSolutionSelected);
            analysis.SolutionSelectionRemoved += new PackPalletAnalysis.SelectSolution(onPackPalletAnalysisSolutionSelectionRemoved);
        }

        void PackPalletAnalysis_InsertSubNodes(Document doc, PackPalletAnalysis analysis, TreeNode nodeAnalysis)
        {
            // sanity check
            if (null == nodeAnalysis) return;
            // remove any existing subnodes
            nodeAnalysis.Nodes.Clear();
            // insert sub pack node
            TreeNode subPackNode = new TreeNode(analysis.PackProperties.Name, 22, 22);
            subPackNode.Tag = new NodeTag(NodeTag.NodeType.NT_ANALYSISPACK, doc, analysis, analysis.PackProperties);
            nodeAnalysis.Nodes.Add(subPackNode);
            TreeNode subPalletNode = new TreeNode(analysis.PalletProperties.Name, 7, 7);
            subPalletNode.Tag = new NodeTag(NodeTag.NodeType.NT_ANALYSISPALLET, doc, analysis, analysis.PalletProperties);
            nodeAnalysis.Nodes.Add(subPalletNode);
            if (analysis.HasInterlayer)
            {
                TreeNode subInterlayerNode = new TreeNode(analysis.InterlayerProperties.Name, 8, 8);
                subInterlayerNode.Tag = new NodeTag(NodeTag.NodeType.NT_ANALYSISINTERLAYER, doc, analysis, analysis.InterlayerProperties);
                nodeAnalysis.Nodes.Add(subInterlayerNode);
            }
            nodeAnalysis.Expand();
        }
        #endregion
        #region Cylinder/Pallet analyses
        /// <summary>
        /// handles new cylinder/pallet analysis creation
        /// </summary>
        public void OnNewCylinderPalletAnalysisCreated(Document doc, CylinderPalletAnalysis analysis)
        {
            // get parent node
            TreeNode parentNode = FindNode(null, new NodeTag(NodeTag.NodeType.NT_LISTANALYSIS, doc));
            // insert analysis node
            int indexIconAnalysis = 20;
            TreeNode nodeAnalysis = new TreeNode(analysis.Name, indexIconAnalysis, indexIconAnalysis);
            nodeAnalysis.Tag = new NodeTag(NodeTag.NodeType.NT_CYLINDERPALLETANALYSIS, doc, analysis);
            parentNode.Nodes.Add(nodeAnalysis);
            parentNode.Expand();
            CylinderPalletAnalysis_InsertSubNodes(doc, analysis, nodeAnalysis);
            // add event handlers for solution selection
            analysis.Modified += new CylinderPalletAnalysis.ModifyAnalysis(onCylinderAnalysisModified);
            analysis.SolutionSelected += new CylinderPalletAnalysis.SelectSolution(onCylinderAnalysisSolutionSelected);
            analysis.SolutionSelectionRemoved += new CylinderPalletAnalysis.SelectSolution(onCylinderPalletSolutionSelectionRemoved);
        }
        void CylinderPalletAnalysis_InsertSubNodes(Document doc, CylinderPalletAnalysis analysis, TreeNode nodeAnalysis)
        { 
            // sanity check
            if (null == nodeAnalysis) return;
            // remove any existing subnodes
            nodeAnalysis.Nodes.Clear();
            // insert sub cylinder node
            int indexIconBoxAnalysis = 6;
            TreeNode subCylNode = new TreeNode(analysis.CylinderProperties.Name, indexIconBoxAnalysis, indexIconBoxAnalysis);
            subCylNode.Tag = new NodeTag(NodeTag.NodeType.NT_ANALYSISBOX, doc, analysis, analysis.CylinderProperties);
            nodeAnalysis.Nodes.Add(subCylNode);
            int indexIconPalletAnalysis = 7;
            TreeNode subPalletNode = new TreeNode(analysis.PalletProperties.Name, indexIconPalletAnalysis, indexIconPalletAnalysis);
            subPalletNode.Tag = new NodeTag(NodeTag.NodeType.NT_ANALYSISPALLET, doc, analysis, analysis.PalletProperties);
            nodeAnalysis.Nodes.Add(subPalletNode);
            // insert sub interlayer node if any
            if (analysis.HasInterlayer)
            {
                TreeNode subInterlayer = new TreeNode(analysis.InterlayerProperties.Name, 8, 8);
                subInterlayer.Tag = new NodeTag(NodeTag.NodeType.NT_ANALYSISINTERLAYER, doc, analysis, analysis.InterlayerProperties);
                nodeAnalysis.Nodes.Add(subInterlayer);
            }
            nodeAnalysis.Expand();        
        }
        public void OnNewHCylinderPalletAnalysisCreated(Document doc, HCylinderPalletAnalysis analysis)
        {
            // get parent node
            TreeNode parentNode = FindNode(null, new NodeTag(NodeTag.NodeType.NT_LISTANALYSIS, doc));
            // insert analysis node
            int indexIconAnalysis = 21;
            TreeNode nodeAnalysis = new TreeNode(analysis.Name, indexIconAnalysis, indexIconAnalysis);
            nodeAnalysis.Tag = new NodeTag(NodeTag.NodeType.NT_HCYLINDERPALLETANALYSIS, doc, analysis);
            parentNode.Nodes.Add(nodeAnalysis);
            parentNode.Expand();
            HCylinderPalletAnalysis_InsertSubNodes(doc, analysis, nodeAnalysis);
            // add event handlers for solution selection
            analysis.Modified += new HCylinderPalletAnalysis.ModifyAnalysis(onHCylinderAnalysisModified);
            analysis.SolutionSelected += new HCylinderPalletAnalysis.SelectSolution(onHCylinderAnalysisSolutionSelected);
            analysis.SolutionSelectionRemoved += new HCylinderPalletAnalysis.SelectSolution(onHCylinderPalletSolutionSelectionRemoved);
        }
        void HCylinderPalletAnalysis_InsertSubNodes(Document doc, HCylinderPalletAnalysis analysis, TreeNode nodeAnalysis)
        {
            // sanity check
            if (null == nodeAnalysis) return;
            // remove any existing subnodes
            nodeAnalysis.Nodes.Clear();
            // insert sub cylinder node
            int indexIconBoxAnalysis = 6;
            TreeNode subCylNode = new TreeNode(analysis.CylinderProperties.Name, indexIconBoxAnalysis, indexIconBoxAnalysis);
            subCylNode.Tag = new NodeTag(NodeTag.NodeType.NT_ANALYSISBOX, doc, analysis, analysis.CylinderProperties);
            nodeAnalysis.Nodes.Add(subCylNode);
            int indexIconPalletAnalysis = 7;
            TreeNode subPalletNode = new TreeNode(analysis.PalletProperties.Name, indexIconPalletAnalysis, indexIconPalletAnalysis);
            subPalletNode.Tag = new NodeTag(NodeTag.NodeType.NT_ANALYSISPALLET, doc, analysis, analysis.PalletProperties);
            nodeAnalysis.Nodes.Add(subPalletNode);
            nodeAnalysis.Expand();
        }
        #endregion
        #region Box/Case analyses
        /// <summary>
        /// handles new box case analysis creation
        /// </summary>
        /// <param name="doc">Document</param>
        /// <param name="analysis">Analysis</param>
        public void OnNewBoxCaseAnalysisCreated(Document doc, BoxCaseAnalysis analysis)
        {
            // get parent node
            TreeNode parentNode = FindNode(null, new NodeTag(NodeTag.NodeType.NT_LISTANALYSIS, doc));
            // get parent node
            int indexIconAnalysis = 17;
            TreeNode nodeAnalysis = new TreeNode(analysis.Name, indexIconAnalysis, indexIconAnalysis);
            nodeAnalysis.Tag = new NodeTag(NodeTag.NodeType.NT_BOXCASEANALYSIS, doc, analysis);
            parentNode.Nodes.Add(nodeAnalysis);
            parentNode.Expand();
            BoxCaseAnalysis_InsertSubNodes(doc, analysis, nodeAnalysis);
            // add event handlers for solution selection
            analysis.Modified += new Basics.BoxCaseAnalysis.ModifyAnalysis(onBoxCaseAnalysisModified);
            analysis.SolutionSelected += new BoxCaseAnalysis.SelectSolution(onBoxCaseAnalysisSolutionSelected);
            analysis.SolutionSelectionRemoved += new Basics.BoxCaseAnalysis.SelectSolution(onBoxCaseAnalysisSolutionSelectionRemoved);
        }

        public void BoxCaseAnalysis_InsertSubNodes(Document doc, BoxCaseAnalysis analysis, TreeNode nodeAnalysis)
        {
            // sanity check
            if (null == nodeAnalysis) return;
            // remove existing sub nodes
            nodeAnalysis.Nodes.Clear();
            // insert sub box node
            int indexIcon = 3;
            TreeNode subBoxNode = new TreeNode(analysis.BProperties.Name, indexIcon, indexIcon);
            subBoxNode.Tag = new NodeTag(NodeTag.NodeType.NT_BOXCASEANALYSISBOX, doc, analysis.BProperties);
            nodeAnalysis.Nodes.Add(subBoxNode);
            // insert sub case node
            indexIcon = 4;
            TreeNode subCaseNode = new TreeNode(analysis.CaseProperties.Name, indexIcon, indexIcon);
            subCaseNode.Tag = new NodeTag(NodeTag.NodeType.NT_BOXCASEANALYSISCASE, doc, analysis.CaseProperties);
            nodeAnalysis.Nodes.Add(subCaseNode);
            nodeAnalysis.Expand();
        }
        #endregion
        #region Box/Case/Pallet analyses
        /// <summary>
        /// handles new analysis created
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="caseAnalysis"></param>
        public void OnNewBoxCasePalletAnalysisCreated(Document doc, BoxCasePalletAnalysis caseAnalysis)
        { 
            // get parent node
            TreeNode parentNode = FindNode(null, new NodeTag(NodeTag.NodeType.NT_LISTANALYSIS, doc));
            // insert case analysis node
            TreeNode nodeAnalysis = new TreeNode(caseAnalysis.Name, 17, 17);
            nodeAnalysis.Tag = new NodeTag(NodeTag.NodeType.NT_BOXCASEPALLETANALYSIS, doc, caseAnalysis);
            parentNode.Nodes.Add(nodeAnalysis);
            parentNode.Expand();
            // insert sub box node
            TreeNode subBoxNode = new TreeNode(caseAnalysis.BoxProperties.Name, 3, 3);
            subBoxNode.Tag = new NodeTag(NodeTag.NodeType.NT_ANALYSISBOX, doc, caseAnalysis, caseAnalysis.BoxProperties);
            nodeAnalysis.Nodes.Add(subBoxNode);
            nodeAnalysis.Expand();

            caseAnalysis.SolutionSelected += new Basics.BoxCasePalletAnalysis.SelectSolution(onCaseAnalysisSolutionSelected);
            caseAnalysis.SolutionSelectionRemoved += new Basics.BoxCasePalletAnalysis.SelectSolution(onCaseAnalysisSolutionSelectionRemoved);
        }
        #endregion
        #region Truck analyses
        /// <summary>
        /// handles new truck analysis created
        /// </summary>
        public void OnNewTruckAnalysisCreated(Document doc, CasePalletAnalysis analysis, SelCasePalletSolution selSolution, TruckAnalysis truckAnalysis)
        {
            // get parent node
            TreeNode parentNode = FindNode(null, new NodeTag(NodeTag.NodeType.NT_CASEPALLETANALYSISSOLUTION, doc, analysis, selSolution));
            // insert truckAnalysis node
            TreeNode nodeTruckAnalysis = new TreeNode(truckAnalysis.Name, 16, 16);
            nodeTruckAnalysis.Tag = new NodeTag(NodeTag.NodeType.NT_TRUCKANALYSIS, doc, analysis, selSolution, truckAnalysis);
            parentNode.Nodes.Add(nodeTruckAnalysis);
            // expand parent tree node
            parentNode.Expand();
        }
        #endregion
        #region ECT analyses
        /// <summary>
        /// handles new ECT analysis created
        /// </summary>
        public void OnNewECTAnalysisCreated(Document doc, CasePalletAnalysis analysis, SelCasePalletSolution selSolution, ECTAnalysis ectAnalysis)
        {
            // get parent node
            TreeNode parentNode = FindNode(null, new NodeTag(NodeTag.NodeType.NT_CASEPALLETANALYSISSOLUTION, doc, analysis, selSolution));
            // insert truckAnalysis node
            TreeNode nodeECTAnalysis = new TreeNode(ectAnalysis.Name, 19, 19);
            nodeECTAnalysis.Tag = new NodeTag(NodeTag.NodeType.NT_ECTANALYSIS, doc, analysis, selSolution, ectAnalysis);
            parentNode.Nodes.Add(nodeECTAnalysis);
            // expand parent tree node
            parentNode.Expand();
        }
        #endregion
        #endregion

        #region Remove functions
        /// <summary>
        /// handles new type removed
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="itemBase"></param>
        public void OnTypeRemoved(Document doc, ItemBase itemBase)
        {
            NodeTag.NodeType nodeType = NodeTag.NodeType.NT_UNKNOWN;
            if (itemBase.GetType() == typeof(BoxProperties))
            {
                BoxProperties box = itemBase as BoxProperties;
                if (box.HasInsideDimensions)
                    nodeType = NodeTag.NodeType.NT_CASE;
                else
                    nodeType = NodeTag.NodeType.NT_BOX;
            }
            else if (itemBase.GetType() == typeof(BundleProperties))
                nodeType = NodeTag.NodeType.NT_BUNDLE;
            else if (itemBase.GetType() == typeof(PackProperties))
                nodeType = NodeTag.NodeType.NT_PACK;
            else if (itemBase.GetType() == typeof(CaseOfBoxesProperties))
                nodeType = NodeTag.NodeType.NT_CASEOFBOXES;
            else if (itemBase.GetType() == typeof(InterlayerProperties))
                nodeType = NodeTag.NodeType.NT_INTERLAYER;
            else if (itemBase.GetType() == typeof(PalletCornerProperties))
                nodeType = NodeTag.NodeType.NT_PALLETCORNERS;
            else if (itemBase.GetType() == typeof(PalletCapProperties))
                nodeType = NodeTag.NodeType.NT_PALLETCAP;
            else if (itemBase.GetType() == typeof(PalletFilmProperties))
                nodeType = NodeTag.NodeType.NT_PALLETFILM;
            else if (itemBase.GetType() == typeof(PalletProperties))
                nodeType = NodeTag.NodeType.NT_PALLET;
            else if (itemBase.GetType() == typeof(TruckProperties))
                nodeType = NodeTag.NodeType.NT_TRUCK;
            else if (itemBase.GetType() == typeof(CylinderProperties))
                nodeType = NodeTag.NodeType.NT_CYLINDER;
            Debug.Assert(nodeType != NodeTag.NodeType.NT_UNKNOWN);
            if (nodeType == NodeTag.NodeType.NT_UNKNOWN)
                return; // ->not found exit
            // get node
            TreeNode typeNode = FindNode(null, new NodeTag(nodeType, doc, itemBase));
            // remove node
            if (null != typeNode)
                Nodes.Remove(typeNode);
        }

        public void OnAnalysisRemoved(Document doc, ItemBase analysis)
        {
            NodeTag.NodeType nodeType = NodeTag.NodeType.NT_UNKNOWN;
            if (analysis.GetType() == typeof(CasePalletAnalysis)) nodeType = NodeTag.NodeType.NT_CASEPALLETANALYSIS;
            else if (analysis.GetType() == typeof(PackPalletAnalysis)) nodeType = NodeTag.NodeType.NT_PACKPALLETANALYSIS;
            else if (analysis.GetType() == typeof(CylinderPalletAnalysis)) nodeType = NodeTag.NodeType.NT_CYLINDERPALLETANALYSIS;
            else if (analysis.GetType() == typeof(HCylinderPalletAnalysis)) nodeType = NodeTag.NodeType.NT_HCYLINDERPALLETANALYSIS;
            else if (analysis.GetType() == typeof(BoxCaseAnalysis)) nodeType = NodeTag.NodeType.NT_BOXCASEANALYSIS;
            else if (analysis.GetType() == typeof(BoxCasePalletAnalysis)) nodeType = NodeTag.NodeType.NT_BOXCASEPALLETANALYSIS;

            // get node
            TreeNode analysisNode = FindNode(null, new NodeTag(nodeType, doc, analysis));
            // test
            if (null == analysisNode)
            {
                _log.Warn(string.Format("Failed to find a valid tree node for analysis {0}", analysis.Name));
                return;
            }
            // remove node
            Nodes.Remove(analysisNode);
        }
        /// <summary>
        /// handles case solution unselected : actually removed selected solution node from case analysis node
        /// </summary>
        public void OnCaseAnalysisSolutionRemoved(Document doc, BoxCasePalletAnalysis caseAnalysis, SelBoxCasePalletSolution selSolution)
        {
            // get node
            TreeNode selSolutionNode = FindNode(null, new NodeTag(NodeTag.NodeType.NT_CASESOLUTION, doc, caseAnalysis, selSolution));
            // test
            if (null == selSolutionNode)
            {
                _log.Error(string.Format("Failed to find a valid tree node for selSolution {0}", selSolution.Name));
                return;
            }
            // remove node
            Nodes.Remove(selSolutionNode);
        }

        public void onBoxCaseAnalysisSolutionRemoved(Document doc, BoxCaseAnalysis boxCaseAnalysis, SelBoxCaseSolution selSolution)
        { 
            // get node
            TreeNode selSolutionNode = FindNode(null, new NodeTag(NodeTag.NodeType.NT_CASESOLUTION, doc, boxCaseAnalysis, selSolution));
            // test
            if (null == selSolutionNode)
            {
                _log.Error(string.Format("Failed to find a valid tree node for selSolution {0}", selSolution.Name));
                return;
            }
            // remove node
            Nodes.Remove(selSolutionNode);
        }

        /// <summary>
        /// handles truck analysis removal : removed truck analysis node from 
        /// </summary>
        public void OnTruckAnalysisRemoved(Document doc, CasePalletAnalysis analysis, SelCasePalletSolution selSolution, TruckAnalysis truckAnalysis)
        {
            // get node
            TreeNode truckAnalysisNode = FindNode(null, new NodeTag(NodeTag.NodeType.NT_TRUCKANALYSIS, doc, analysis, selSolution, truckAnalysis));
            // test
            if (null == truckAnalysisNode)
            {
                _log.Warn(string.Format("Failed to find a valid tree node for truck analysis {0}", truckAnalysis.Name));
                return;
            }
            // remove node
            Nodes.Remove(truckAnalysisNode);  
        }
        /// <summary>
        /// handles ECT analysis removal
        /// </summary>
        public void OnECTAnalysisRemoved(Document doc, CasePalletAnalysis analysis, SelCasePalletSolution selSolution, ECTAnalysis ectAnalysis)
        { 
            // get node
            TreeNode ectAnalysisNode = FindNode(null, new NodeTag(NodeTag.NodeType.NT_ECTANALYSIS, doc, analysis, selSolution, ectAnalysis));
            // test
            if (null == ectAnalysisNode)
            {
                _log.Warn(string.Format("Failed to find a valid tree node for truck analysis {0}", ectAnalysis.Name));
            }
            // remove node
            Nodes.Remove(ectAnalysisNode);
        }
        /// <summary>
        /// handles document closing event by removing the corresponding document node in TreeView
        /// </summary>
        public void OnDocumentClosed(Document doc)
        {
            NodeTag.NodeType nodeType = NodeTag.NodeType.NT_DOCUMENT;
            // get node
            TreeNode docNode = FindNode(null, new NodeTag(nodeType, doc));
            // remove node
            Nodes.Remove(docNode);
        }
        #endregion

        #region PalletAnalysis/CaseAnalysis solution added/removed Handlers
        private void onCaseAnalysisSolutionSelectionRemoved(BoxCasePalletAnalysis caseAnalysis, SelBoxCasePalletSolution selSolution)
        {
            // retrieve parent document
            Document doc = caseAnalysis.ParentDocument;
            // get node
            TreeNode caseAnalysisNode = FindNode(null, new NodeTag(NodeTag.NodeType.NT_BOXCASEPALLETANALYSIS, doc, caseAnalysis));
            // test
            if (null == caseAnalysisNode)
            {
                _log.Warn(string.Format("Failed to find a valid tree node for caseAnalysis {0}", caseAnalysis.Name));
                return;
            }
            // remove node
            Nodes.Remove(caseAnalysisNode);
        }

        private void onCaseAnalysisSolutionSelected(BoxCasePalletAnalysis caseAnalysis, SelBoxCasePalletSolution selSolution)
        {
            // retrieve document
            Document doc = caseAnalysis.ParentDocument;
            // get parent node
            TreeNode parentNode = FindNode(null, new NodeTag(NodeTag.NodeType.NT_BOXCASEPALLETANALYSIS, doc, caseAnalysis));
            if (null == parentNode) throw new Exception("Failed to locate Analysis (NT_BOXCASEPALLETANALYSIS) node...");
            // insert selected solution node
            TreeNode nodeSelSolution = new TreeNode(selSolution.Name, 15, 15);
            nodeSelSolution.Tag = new NodeTag(NodeTag.NodeType.NT_CASESOLUTION, doc, caseAnalysis, selSolution);
            parentNode.Nodes.Add(nodeSelSolution);
            // expand tree node
            parentNode.Expand();
        }

        private void onCasePalletAnalysisModified(CasePalletAnalysis analysis)
        { 
            // retrieve parent document
            Document doc = analysis.ParentDocument;
            // get parent node
            TreeNode parentNode = FindNode(null, new NodeTag(NodeTag.NodeType.NT_CASEPALLETANALYSIS, doc, analysis));
            if (null == parentNode) throw new Exception("Failed to locate Analysis (NT_CASEPALLETANALYSIS) node...");
            // insert case/pallet/interlayer node
            CasePalletAnalysis_InsertSubNodes(doc, analysis, parentNode);
            // expand tree node
            parentNode.Expand();
        }
        private void onPalletAnalysisSolutionSelected(CasePalletAnalysis analysis, SelCasePalletSolution selSolution)
        {
            // retrieve parent document
            Document doc = analysis.ParentDocument;
            // get parent node
            TreeNode parentNode = FindNode(null, new NodeTag(NodeTag.NodeType.NT_CASEPALLETANALYSIS, doc, analysis));
            if (null == parentNode) throw new Exception("Failed to locate Analysis (NT_CASEPALLETANALYSIS) node...");
            // insert selected solution node
            TreeNode nodeSelSolution = new TreeNode(selSolution.Name, 15, 15);
            nodeSelSolution.Tag = new NodeTag(NodeTag.NodeType.NT_CASEPALLETANALYSISSOLUTION, doc, analysis, selSolution);
            parentNode.Nodes.Add(nodeSelSolution);
            // expand tree nodes
            parentNode.Expand();
        }
        private void onPackPalletAnalysisModified(PackPalletAnalysis analysis)
        { 
            // retrieve parent document
            Document doc = analysis.ParentDocument;
            // get parent node
            TreeNode parentNode = FindNode(null, new NodeTag(NodeTag.NodeType.NT_PACKPALLETANALYSIS, doc, analysis));
            if (null == parentNode) throw new Exception("Failed to locate Analysis (NT_PACKPALLETANALYSIS) node...");
            // insert case/pallet/interlayer node
            PackPalletAnalysis_InsertSubNodes(doc, analysis, parentNode);
            // expand tree node
            parentNode.Expand();
        }
        private void onPackPalletAnalysisSolutionSelected(PackPalletAnalysis analysis, SelPackPalletSolution selSolution)
        {
            // retrieve parent document
            Document doc = analysis.ParentDocument;
            // get parent node
            TreeNode parentNode = FindNode(null, new NodeTag(NodeTag.NodeType.NT_PACKPALLETANALYSIS, doc, analysis));
            if (null == parentNode) throw new Exception("Failed to locate Analysis (NT_PACKPALLETANALYSIS) node...");
            // insert selected solution node
            TreeNode nodeSelSolution = new TreeNode(selSolution.Name, 15, 15);
            nodeSelSolution.Tag = new NodeTag(NodeTag.NodeType.NT_PACKPALLETANALYSISSOLUTION, doc, analysis, selSolution);
            parentNode.Nodes.Add(nodeSelSolution);
            // expand tree nodes
            parentNode.Expand();
        }
        private void onCylinderAnalysisModified(CylinderPalletAnalysis analysis)
        {
            // retrieve parent document
            Document doc = analysis.ParentDocument;
            // get parent node
            TreeNode parentNode = FindNode(null, new NodeTag(NodeTag.NodeType.NT_CYLINDERPALLETANALYSIS, doc, analysis));
            // remove & insert sub nodes
            CylinderPalletAnalysis_InsertSubNodes(doc, analysis, parentNode);
            // expand tree node
            parentNode.Expand();
        }
        private void onCylinderAnalysisSolutionSelected(CylinderPalletAnalysis analysis, SelCylinderPalletSolution selSolution)
        { 
            // retrieve parent document
            Document doc = analysis.ParentDocument;
            // get parent node
            TreeNode parentNode = FindNode(null, new NodeTag(NodeTag.NodeType.NT_CYLINDERPALLETANALYSIS, doc, analysis));
            // inserted selected solution node
            TreeNode nodeSelSolution = new TreeNode(selSolution.Name, 15, 15);
            nodeSelSolution.Tag = new NodeTag(NodeTag.NodeType.NT_CYLINDERPALLETANALYSISSOLUTION, doc, analysis, selSolution);
            parentNode.Nodes.Add(nodeSelSolution);
            // expand tree nodes
            parentNode.Expand();
        }
        private void onHCylinderAnalysisModified(HCylinderPalletAnalysis analysis)
        {
            // retrieve parent document
            Document doc = analysis.ParentDocument;
            // get parent node
            TreeNode parentNode = FindNode(null, new NodeTag(NodeTag.NodeType.NT_HCYLINDERPALLETANALYSIS, doc, analysis));
            // remove & insert sub nodes
            HCylinderPalletAnalysis_InsertSubNodes(doc, analysis, parentNode);
            // expand tree node
            parentNode.Expand();
        }
        private void onHCylinderAnalysisSolutionSelected(HCylinderPalletAnalysis analysis, SelHCylinderPalletSolution selSolution)
        {
            // retrieve parent document
            Document doc = analysis.ParentDocument;
            // get parent node
            TreeNode parentNode = FindNode(null, new NodeTag(NodeTag.NodeType.NT_HCYLINDERPALLETANALYSIS, doc, analysis));
            // inserted selected solution node
            TreeNode nodeSelSolution = new TreeNode(selSolution.Name, 15, 15);
            nodeSelSolution.Tag = new NodeTag(NodeTag.NodeType.NT_HCYLINDERPALLETANALYSISSOLUTION, doc, analysis, selSolution);
            parentNode.Nodes.Add(nodeSelSolution);
            // expand tree nodes
            parentNode.Expand();
        }
        private void onBoxCaseAnalysisModified(BoxCaseAnalysis analysis)
        {
            // retrieve parent document
            Document doc = analysis.ParentDocument;
            // get parent node
            TreeNode parentNode = FindNode(null, new NodeTag(NodeTag.NodeType.NT_BOXCASEANALYSIS, doc, analysis));
            // remove & insert sub nodes
            BoxCaseAnalysis_InsertSubNodes(doc, analysis, parentNode);
        }
        private void onBoxCaseAnalysisSolutionSelected(BoxCaseAnalysis analysis, SelBoxCaseSolution selSolution)
        {
            // retrieve parent document
            Document doc = analysis.ParentDocument;
            // get parent node
            TreeNode parentNode = FindNode(null, new NodeTag(NodeTag.NodeType.NT_BOXCASEANALYSIS, doc, analysis));
            // insert selected solution node
            TreeNode nodeSelSolution = new TreeNode(selSolution.Name, 15, 15);
            nodeSelSolution.Tag = new NodeTag(NodeTag.NodeType.NT_BOXCASEANALYSISSOLUTION, doc, analysis, selSolution);
            parentNode.Nodes.Add(nodeSelSolution);
            // expand tree nodes
            parentNode.Expand();
        }
        private void onPalletAnalysisSolutionSelectionRemoved(CasePalletAnalysis analysis, SelCasePalletSolution selSolution)
        {
            // retrieve parent document
            Document doc = analysis.ParentDocument;
            // get node
            TreeNode selSolutionNode = FindNode(null, new NodeTag(NodeTag.NodeType.NT_CASEPALLETANALYSISSOLUTION, doc, analysis, selSolution));
            // test
            if (null == selSolutionNode)
            {
                _log.Error(string.Format("Failed to find a valid tree node for selSolution {0}", selSolution.Name));
                return;
            }
            // remove node
            Nodes.Remove(selSolutionNode);
        }
        private void onPackPalletAnalysisSolutionSelectionRemoved(PackPalletAnalysis analysis, SelPackPalletSolution selSolution)
        { 
            // retrieve parent document
            Document doc = analysis.ParentDocument;
            // get node
            TreeNode selSolutionNode = FindNode(null, new NodeTag(NodeTag.NodeType.NT_PACKPALLETANALYSISSOLUTION, doc, analysis, selSolution));
            // test
            if (null == selSolutionNode)
            {
                _log.Error(string.Format("Failed to find a valid tree node for selSolution {0}", selSolution.Name));
                return;
            }
            // remove node
            Nodes.Remove(selSolutionNode);
        }
        private void onCylinderPalletSolutionSelectionRemoved(CylinderPalletAnalysis analysis, SelCylinderPalletSolution selSolution)
        { 
            // retrieve parent document
            Document doc = analysis.ParentDocument;
            // get node
            TreeNode selSolutionNode = FindNode(null, new NodeTag(NodeTag.NodeType.NT_CYLINDERPALLETANALYSISSOLUTION, doc, analysis, selSolution));
            // test
            if (null == selSolutionNode)
            {
                _log.Error(string.Format("Failed to find a valid tree node for selSolution {0}", selSolution.Name));
                return;
            }
            // remove node
            Nodes.Remove(selSolutionNode);
        }
        private void onHCylinderPalletSolutionSelectionRemoved(HCylinderPalletAnalysis analysis, SelHCylinderPalletSolution selSolution)
        {
            // retrieve parent document
            Document doc = analysis.ParentDocument;
            // get node
            TreeNode selSolutionNode = FindNode(null, new NodeTag(NodeTag.NodeType.NT_HCYLINDERPALLETANALYSISSOLUTION, doc, analysis, selSolution));
            // test
            if (null == selSolutionNode)
            {
                _log.Error(string.Format("Failed to find a valid tree node for selSolution {0}", selSolution.Name));
                return;
            }
            // remove node
            Nodes.Remove(selSolutionNode);       
        }
        private void onBoxCaseAnalysisSolutionSelectionRemoved(BoxCaseAnalysis analysis, SelBoxCaseSolution selSolution)
        {
            // retrieve parent document
            Document doc = analysis.ParentDocument;
            // get node
            TreeNode selSolutionNode = FindNode(null, new NodeTag(NodeTag.NodeType.NT_BOXCASEANALYSISSOLUTION, doc, analysis, selSolution));
            // test
            if (null == selSolutionNode)
            {
                _log.Error(string.Format("Failed to find a valid tree node for selSolution {0}", selSolution.Name));
                return;
            }
            // remove node
            Nodes.Remove(selSolutionNode);
        }
        #endregion

        #region Data members
        static readonly ILog _log = LogManager.GetLogger(typeof(AnalysisTreeView));
        #endregion
    }
    #endregion

    #region NodeTag class
    /// <summary>
    /// NodeTag will be used for each TreeNode.Tag
    /// </summary>
    public class NodeTag
    {
        #region Enums
        /// <summary>
        /// AnalysisTreeView node types
        /// </summary>
        public enum NodeType
        {
            /// <summary>
            /// document
            /// </summary>
            NT_DOCUMENT,
            /// <summary>
            /// list of boxes
            /// </summary>
            NT_LISTBOX,
            /// <summary>
            /// list of cases
            /// </summary>
            NT_LISTCASE,
            /// <summary>
            /// list of pack
            /// </summary>
            NT_LISTPACK,
            /// <summary>
            /// list of cylinders
            /// </summary>
            NT_LISTCYLINDER,
            /// <summary>
            /// list of bundles
            /// </summary>
            NT_LISTBUNDLE,
            /// <summary>
            /// list of palets
            /// </summary>
            NT_LISTPALLET,
            /// <summary>
            /// list of interlayers
            /// </summary>
            NT_LISTINTERLAYER,
            /// <summary>
            /// list of trucks
            /// </summary>
            NT_LISTTRUCK,
            /// <summary>
            /// list of analyses
            /// </summary>
            NT_LISTANALYSIS,
            /// <summary>
            /// list of pallet corners
            /// </summary>
            NT_LISTPALLETCORNERS,
            /// <summary>
            /// list of pallet cap
            /// </summary>
            NT_LISTPALLETCAP,
            /// <summary>
            /// list of pallet film
            /// </summary>
            NT_LISTPALLETFILM,
            /// <summary>
            /// box
            /// </summary>
            NT_BOX,
            /// <summary>
            /// case
            /// </summary>
            NT_CASE,
            /// <summary>
            /// pack
            /// </summary>
            NT_PACK,
            /// <summary>
            /// case of boxes
            /// </summary>
            NT_CASEOFBOXES,
            /// <summary>
            /// bundle
            /// </summary>
            NT_BUNDLE,
            /// <summary>
            /// cylinder
            /// </summary>
            NT_CYLINDER,
            /// <summary>
            /// palet
            /// </summary>
            NT_PALLET,
            /// <summary>
            /// interlayer
            /// </summary>
            NT_INTERLAYER,
            /// <summary>
            /// truck
            /// </summary>
            NT_TRUCK,
            /// <summary>
            /// pallet corners
            /// </summary>
            NT_PALLETCORNERS,
            /// <summary>
            /// pallet cap
            /// </summary>
            NT_PALLETCAP,
            /// <summary>
            /// pallet film
            /// </summary>
            NT_PALLETFILM,
            /// <summary>
            /// analysis
            /// </summary>
            NT_CASEPALLETANALYSIS,
            /// <summary>
            /// pack/pallet analysis
            /// </summary>
            NT_PACKPALLETANALYSIS,
            /// <summary>
            /// cylinder/pallet analysis
            /// </summary>
            NT_CYLINDERPALLETANALYSIS,
            /// <summary>
            /// hcylinder/pallet analysis
            /// </summary>
            NT_HCYLINDERPALLETANALYSIS,
            /// <summary>
            /// analysis box
            /// </summary>
            NT_ANALYSISBOX,
            /// <summary>
            /// analysis pack
            /// </summary>
            NT_ANALYSISPACK,
            /// <summary>
            /// analysis pallet
            /// </summary>
            NT_ANALYSISPALLET,
            /// <summary>
            /// analysis interlayer
            /// </summary>
            NT_ANALYSISINTERLAYER,
            /// <summary>
            /// analysis pallet corners
            /// </summary>
            NT_ANALYSISPALLETCORNERS,
            /// <summary>
            /// analysis pallet cap
            /// </summary>
            NT_ANALYSISPALLETCAP,
            /// <summary>
            /// analysis pallet film
            /// </summary>
            NT_ANALYSISPALLETFILM,
            /// <summary>
            /// analysis solution
            /// </summary>
            NT_CASEPALLETANALYSISSOLUTION,
            /// <summary>
            /// pack/pallet analysis solution
            /// </summary>
            NT_PACKPALLETANALYSISSOLUTION,
            /// <summary>
            /// cylinder pallet analysis solution
            /// </summary>
            NT_CYLINDERPALLETANALYSISSOLUTION,
            /// <summary>
            /// hcylinder pallet analysis solution
            /// </summary>
            NT_HCYLINDERPALLETANALYSISSOLUTION,
            /// <summary>
            /// analysis report
            /// </summary>
            NT_ANALYSISSOLREPORT,
            /// <summary>
            /// truck analysis
            /// </summary>
            NT_TRUCKANALYSIS,
            /// <summary>
            /// truck analysis solution
            /// </summary>
            NT_TRUCKANALYSISSOL,
            /// <summary>
            /// case analysis
            /// </summary>
            NT_BOXCASEPALLETANALYSIS,
            /// <summary>
            /// case analysis solution
            /// </summary>
            NT_CASESOLUTION,
            /// <summary>
            /// ECT analysis (Edge Crush Test)
            /// </summary>
            NT_ECTANALYSIS,
            /// <summary>
            /// box/case analysis
            /// </summary>
            NT_BOXCASEANALYSIS,
            /// <summary>
            /// box/case analysis case
            /// </summary>
            NT_BOXCASEANALYSISCASE,
            /// <summary>
            /// box/case analysis box
            /// </summary>
            NT_BOXCASEANALYSISBOX,
            /// <summary>
            /// box/case analysis solution
            /// </summary>
            NT_BOXCASEANALYSISSOLUTION,
            /// <summary>
            /// unknown
            /// </summary>
            NT_UNKNOWN
        }
        #endregion

        #region Data members
        private NodeType _type;
        private Document _document;
        private ItemBase _itemProperties;
        private CasePalletAnalysis _casePalletAnalysis;
        private PackPalletAnalysis _packPalletAnalysis;
        private CylinderPalletAnalysis _cylinderPalletAnalysis;
        private HCylinderPalletAnalysis _hCylinderPalletAnalysis;
        private SelCasePalletSolution _selSolution;
        private SelPackPalletSolution _selPackPalletSolution;
        private SelCylinderPalletSolution _selCylinderPalletSolution;
        private SelHCylinderPalletSolution _selHCylinderPalletSolution;
        private BoxCaseAnalysis _boxCaseAnalysis;
        private SelBoxCaseSolution _selBoxCaseSolution;
        private TruckAnalysis _truckAnalysis;
        private BoxCasePalletAnalysis _boxCasePalletAnalysis;
        private SelBoxCasePalletSolution _selBoxCasePalletSolution;
        private ECTAnalysis _ectAnalysis;
        #endregion

        #region Constructor
        public NodeTag(NodeType type, Document document)
        {
            _type = type;
            _document = document;       
        }
        public NodeTag(NodeType type, Document document, ItemBase itemProperties)
        {
            _type = type;
            _document = document;
            _itemProperties = itemProperties;
            if (_type == NodeType.NT_CASEPALLETANALYSIS && itemProperties is CasePalletAnalysis)
            {
                _casePalletAnalysis = itemProperties as CasePalletAnalysis;
                _itemProperties = null;
            }
            else if (_type == NodeType.NT_PACKPALLETANALYSIS && itemProperties is PackPalletAnalysis)
            {
                _packPalletAnalysis = itemProperties as PackPalletAnalysis;
                _itemProperties = null;
            }
            else if (_type == NodeType.NT_BOXCASEANALYSIS && itemProperties is BoxCaseAnalysis)
            {
                _boxCaseAnalysis = itemProperties as BoxCaseAnalysis;
                _itemProperties = null;
            }
            else if (_type == NodeType.NT_BOXCASEPALLETANALYSIS && itemProperties is BoxCasePalletAnalysis)
            {
                _boxCasePalletAnalysis = itemProperties as BoxCasePalletAnalysis;
                _itemProperties = null;
            }
            else if (_type == NodeType.NT_CYLINDERPALLETANALYSIS && itemProperties is CylinderPalletAnalysis)
            {
                _cylinderPalletAnalysis = itemProperties as CylinderPalletAnalysis;
                _itemProperties = null;
            }
            else if (_type == NodeType.NT_HCYLINDERPALLETANALYSIS && itemProperties is HCylinderPalletAnalysis)
            {
                _hCylinderPalletAnalysis = itemProperties as HCylinderPalletAnalysis;
                _itemProperties = null;
            }

        }
        public NodeTag(NodeType type, Document document, CasePalletAnalysis analysis, ItemBase itemProperties)
        {
            _type = type;
            _document = document;
            _itemProperties = itemProperties;
            _casePalletAnalysis = analysis;
        }
        public NodeTag(NodeType type, Document document, CasePalletAnalysis analysis, SelCasePalletSolution selSolution)
        {
            _type = type;
            _document = document;
            _itemProperties = null;
            _casePalletAnalysis = analysis;
            _selSolution = selSolution;
        }
        public NodeTag(NodeType type, Document document, CasePalletAnalysis analysis, SelCasePalletSolution selSolution, TruckAnalysis truckAnalysis)
        {
            _type = type;
            _document = document;
            _itemProperties = null;
            _casePalletAnalysis = analysis;
            _selSolution = selSolution;
            _truckAnalysis = truckAnalysis;
        }
        public NodeTag(NodeType type, Document document, PackPalletAnalysis analysis, ItemBase itemProperties)
        {
            _type = type;
            _document = document;
            _itemProperties = itemProperties;
            _packPalletAnalysis = analysis;
        }
        public NodeTag(NodeType type, Document document, PackPalletAnalysis analysis, SelPackPalletSolution selSolution)
        {
            _type = type;
            _document = document;
            _itemProperties = null;
            _packPalletAnalysis = analysis;
            _selPackPalletSolution = selSolution;
        }
        public NodeTag(NodeType type, Document document, CasePalletAnalysis analysis, SelCasePalletSolution selSolution, ECTAnalysis ectAnalysis)
        {
            _type = type;
            _document = document;
            _itemProperties = null;
            _casePalletAnalysis = analysis;
            _selSolution = selSolution;
            _ectAnalysis = ectAnalysis;
        }
        public NodeTag(NodeType type, Document document, CylinderPalletAnalysis analysis)
        {
            _type = type;
            _document = document;
            _itemProperties = null;
            _cylinderPalletAnalysis = analysis;
        }
        public NodeTag(NodeType type, Document document, CylinderPalletAnalysis analysis, ItemBase itemProperties)
        {
            _type = type;
            _document = document;
            _itemProperties = itemProperties;
            _cylinderPalletAnalysis = analysis;
        }
        public NodeTag(NodeType type, Document document, CylinderPalletAnalysis analysis, SelCylinderPalletSolution selSolution)
        {
            _type = type;
            _document = document;
            _itemProperties = null;
            _cylinderPalletAnalysis = analysis;
            _selCylinderPalletSolution = selSolution;
        }
        public NodeTag(NodeType type, Document document, CylinderPalletAnalysis analysis, SelCylinderPalletSolution selSolution, TruckAnalysis truckAnalysis)
        {
            _type = type;
            _document = document;
            _itemProperties = null;
            _cylinderPalletAnalysis = analysis;
            _selCylinderPalletSolution = selSolution;
            _truckAnalysis = truckAnalysis;
        }
        public NodeTag(NodeType type, Document document, HCylinderPalletAnalysis analysis)
        {
            _type = type;
            _document = document;
            _itemProperties = null;
            _hCylinderPalletAnalysis = analysis;
        }
        public NodeTag(NodeType type, Document document, HCylinderPalletAnalysis analysis, ItemBase itemProperties)
        {
            _type = type;
            _document = document;
            _itemProperties = itemProperties;
            _hCylinderPalletAnalysis = analysis;
        }
        public NodeTag(NodeType type, Document document, HCylinderPalletAnalysis analysis, SelHCylinderPalletSolution selSolution)
        {
            _type = type;
            _document = document;
            _itemProperties = null;
            _hCylinderPalletAnalysis = analysis;
            _selHCylinderPalletSolution = selSolution;
        }
        public NodeTag(NodeType type, Document document, HCylinderPalletAnalysis analysis, SelHCylinderPalletSolution selSolution, TruckAnalysis truckAnalysis)
        {
            _type = type;
            _document = document;
            _itemProperties = null;
            _hCylinderPalletAnalysis = analysis;
            _selHCylinderPalletSolution = selSolution;
            _truckAnalysis = truckAnalysis;
        }
        public NodeTag(NodeType type, Document doc, BoxCaseAnalysis boxCaseAnalysis)
        {        
            _type = type;
            _document = doc;
            _boxCaseAnalysis = boxCaseAnalysis;
        }
        public NodeTag(NodeType type, Document doc, BoxCaseAnalysis boxCaseAnalysis, SelBoxCaseSolution selBoxCaseSolution)
        { 
            _type = type;
            _document = doc;
            _boxCaseAnalysis = boxCaseAnalysis;
            _selBoxCaseSolution = selBoxCaseSolution;
        }
        public NodeTag(NodeType type, Document doc, BoxCasePalletAnalysis caseAnalysis)
        { 
            _type = type;
            _document = doc;
            _boxCasePalletAnalysis = caseAnalysis;       
        }
        public NodeTag(NodeType type, Document doc, BoxCasePalletAnalysis caseAnalysis, ItemBase itemProperties)
        {
            _type = type;
            _document = doc;
            _boxCasePalletAnalysis = caseAnalysis;
            _itemProperties = itemProperties;
            _selSolution = null;
        }
        public NodeTag(NodeType type, Document document, BoxCasePalletAnalysis caseAnalysis, SelBoxCasePalletSolution selBoxCasePalletSolution)
        {
            _type = type;
            _document = document;
            _boxCasePalletAnalysis = caseAnalysis;
            _selBoxCasePalletSolution = selBoxCasePalletSolution;
        }
        #endregion

        #region Object method overrides
        public override bool Equals(object obj)
        {
            NodeTag nodeTag = obj as NodeTag;
            if (null == nodeTag) return false;
            return _type == nodeTag._type
                && _document == nodeTag._document
                && _itemProperties == nodeTag._itemProperties
                // analyses
                && _casePalletAnalysis == nodeTag._casePalletAnalysis
                && _packPalletAnalysis == nodeTag._packPalletAnalysis
                && _cylinderPalletAnalysis == nodeTag._cylinderPalletAnalysis
                && _hCylinderPalletAnalysis == nodeTag._hCylinderPalletAnalysis
                && _boxCasePalletAnalysis == nodeTag._boxCasePalletAnalysis
                && _boxCaseAnalysis == nodeTag._boxCaseAnalysis
                && _truckAnalysis == nodeTag._truckAnalysis
                // solutions
                && _selSolution == nodeTag._selSolution
                && _selPackPalletSolution == nodeTag._selPackPalletSolution
                && _selCylinderPalletSolution == nodeTag._selCylinderPalletSolution
                && _selHCylinderPalletSolution == nodeTag._selHCylinderPalletSolution
                && _selBoxCasePalletSolution == nodeTag._selBoxCasePalletSolution
                && _selBoxCaseSolution == nodeTag._selBoxCaseSolution;
                
        }
        public override int GetHashCode()
        {
            return _type.GetHashCode()
                ^ _document.GetHashCode()
                ^ _itemProperties.GetHashCode()
                ^ _casePalletAnalysis.GetHashCode()
                ^ _selSolution.GetHashCode()
                ^ _truckAnalysis.GetHashCode();
        }
        #endregion

        #region Public properties
        /// <summary>
        /// returns node type
        /// </summary>
        public NodeType Type { get { return _type; } }
        /// <summary>
        /// returns document adressed 
        /// </summary>
        public Document Document { get { return _document; } }
        /// <summary>
        /// returns itempProperties (box/palet/interlayer)
        /// </summary>
        public ItemBase ItemProperties { get { return _itemProperties; } }
        /// <summary>
        /// returns analysis if any
        /// </summary>
        public CasePalletAnalysis CasePalletAnalysis { get { return _casePalletAnalysis; } }
        /// <summary>
        /// returns pack pallet analysis
        /// </summary>
        public PackPalletAnalysis PackPalletAnalysis { get { return _packPalletAnalysis; } }
        /// <summary>
        /// returns pack pallet solution
        /// </summary>
        public SelPackPalletSolution SelPackPalletSolution { get { return _selPackPalletSolution; } }
        /// <summary>
        /// return cylinder/pallet analysis if any
        /// </summary>
        public CylinderPalletAnalysis CylinderPalletAnalysis { get { return _cylinderPalletAnalysis; } }
        /// <summary>
        /// return hcylinder/pallet analysis if any
        /// </summary>
        public HCylinderPalletAnalysis HCylinderPalletAnalysis { get { return _hCylinderPalletAnalysis; } }
        /// <summary>
        ///  returns selected solution if any
        /// </summary>
        public SelCasePalletSolution SelSolution { get { return _selSolution; } }
        /// <summary>
        /// returns selected hcylinder/pallet solution if any
        /// </summary>
        public SelHCylinderPalletSolution SelHCylinderPalletSolution { get { return _selHCylinderPalletSolution; } }
        /// <summary>
        /// returns selected cylinder/pallet solution if any
        /// </summary>
        public SelCylinderPalletSolution SelCylinderPalletSolution { get { return _selCylinderPalletSolution; } }
        /// <summary>
        /// returns truck analysis of selected solution
        /// </summary>
        public TruckAnalysis TruckAnalysis { get { return _truckAnalysis; } }
        /// <summary>
        /// returns box case analysis
        /// </summary>
        public BoxCaseAnalysis BoxCaseAnalysis { get { return _boxCaseAnalysis; } }
        /// <summary>
        /// return selected box/case solution
        /// </summary>
        public SelBoxCaseSolution SelBoxCaseSolution { get { return _selBoxCaseSolution; } }
        /// <summary>
        /// returns case analysis
        /// </summary>
        public BoxCasePalletAnalysis CaseAnalysis { get { return _boxCasePalletAnalysis; } }
        /// <summary>
        /// returns selected case solution if any
        /// </summary>
        public SelBoxCasePalletSolution SelBoxCasePalletSolution { get { return _selBoxCasePalletSolution; } }
        /// <summary>
        /// returns ECT analysis
        /// </summary>
        public ECTAnalysis ECTAnalysis { get { return _ectAnalysis; } }
        #endregion
    }
    #endregion

    #region AnalysisTreeViewEventArgs class
    /// <summary>
    /// EventArg inherited class used as AnalysisNodeClickHandler delegate argument
    /// Encapsulates a reference to a NodeTag
    /// </summary>
    public class AnalysisTreeViewEventArgs : EventArgs
    {
        #region Data members
        private NodeTag _nodeTag;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor takes the clicked node tag as argument
        /// </summary>
        /// <param name="nodeTag"></param>
        public AnalysisTreeViewEventArgs(NodeTag nodeTag)
        {
            _nodeTag = nodeTag;
        }
        #endregion

        #region Public properties
        /// <summary>
        /// Document
        /// </summary>
        public Document Document { get { return _nodeTag.Document; } }
        /// <summary>
        /// Analysis
        /// </summary>
        public CasePalletAnalysis Analysis { get { return _nodeTag.CasePalletAnalysis; } }
        /// <summary>
        /// PackPammetAnalysis
        /// </summary>
        public PackPalletAnalysis PackPalletAnalysis { get { return _nodeTag.PackPalletAnalysis; } }
        /// <summary>
        /// Cylinder/pallet analysis
        /// </summary>
        public CylinderPalletAnalysis CylinderAnalysis { get { return _nodeTag.CylinderPalletAnalysis; } }
        /// <summary>
        /// HCylinder/pallet analysis
        /// </summary>
        public HCylinderPalletAnalysis HCylinderAnalysis { get { return _nodeTag.HCylinderPalletAnalysis; } }
        /// <summary>
        /// ItemBase (BoxProperties \ PaletProperties \ Interlayer properties)
        /// </summary>
        public ItemBase ItemBase { get { return _nodeTag.ItemProperties; } }
        /// <summary>
        /// Selected solution
        /// </summary>
        public SelCasePalletSolution SelSolution { get { return _nodeTag.SelSolution; } }
        /// <summary>
        /// Selected pack/pallet solution
        /// </summary>
        public SelPackPalletSolution SelPackPalletSolution { get { return _nodeTag.SelPackPalletSolution; } }
        /// <summary>
        /// Selected cylinder/pallet solution
        /// </summary>
        public SelCylinderPalletSolution SelCylinderPalletSolution { get { return _nodeTag.SelCylinderPalletSolution; } }
        /// <summary>
        /// Selected hcylinder/pallet solution
        /// </summary>
        public SelHCylinderPalletSolution SelHCylinderPalletSolution { get { return _nodeTag.SelHCylinderPalletSolution; } }
        /// <summary>
        /// Truck analysis
        /// </summary>
        public TruckAnalysis TruckAnalysis { get { return _nodeTag.TruckAnalysis; } }
        /// <summary>
        /// Box/Case/Pallet analysis
        /// </summary>
        public BoxCasePalletAnalysis BoxCasePalletAnalysis { get { return _nodeTag.CaseAnalysis; } }
        /// <summary>
        /// Selected box/case/pallet solution
        /// </summary>
        public SelBoxCasePalletSolution SelBoxCasePalletSolution { get { return _nodeTag.SelBoxCasePalletSolution; } }
        /// <summary>
        /// Box/Case analysis
        /// </summary>
        public BoxCaseAnalysis BoxCaseAnalysis { get { return _nodeTag.BoxCaseAnalysis; } }
        public SelBoxCaseSolution SelBoxCaseSolution { get { return _nodeTag.SelBoxCaseSolution;  } }
        /// <summary>
        /// ECTAnalysis
        /// </summary>
        public ECTAnalysis ECTAnalysis { get { return _nodeTag.ECTAnalysis; } }
        #endregion
    }
    #endregion
}
