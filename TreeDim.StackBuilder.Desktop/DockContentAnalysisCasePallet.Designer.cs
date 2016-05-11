namespace treeDiM.StackBuilder.Desktop
{
    partial class DockContentAnalysisCasePallet
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DockContentAnalysisCasePallet));
            this.toolStripAnalysis = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonReport = new System.Windows.Forms.ToolStripButton();
            this.chkbInterlayer = new System.Windows.Forms.CheckBox();
            this.cbInterlater = new System.Windows.Forms.ComboBox();
            this.bnSymmetryX = new System.Windows.Forms.Button();
            this.bnSymetryY = new System.Windows.Forms.Button();
            this.cbLayerType = new System.Windows.Forms.ComboBox();
            this.graphCtrlSolution = new treeDiM.StackBuilder.Graphics.Graphics3DControl();
            this.label1 = new System.Windows.Forms.Label();
            this.gbLayer = new System.Windows.Forms.GroupBox();
            this.rtb = new System.Windows.Forms.RichTextBox();
            this.tabCtrl = new System.Windows.Forms.TabControl();
            this.tabPagePalletCorners = new System.Windows.Forms.TabPage();
            this.tabPagePalletCap = new System.Windows.Forms.TabPage();
            this.tabPagePalletFilm = new System.Windows.Forms.TabPage();
            this.tbClickLayer = new System.Windows.Forms.TextBox();
            this.toolStripAnalysis.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.graphCtrlSolution)).BeginInit();
            this.gbLayer.SuspendLayout();
            this.tabCtrl.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripAnalysis
            // 
            this.toolStripAnalysis.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonReport});
            this.toolStripAnalysis.Location = new System.Drawing.Point(0, 0);
            this.toolStripAnalysis.Name = "toolStripAnalysis";
            this.toolStripAnalysis.Size = new System.Drawing.Size(684, 25);
            this.toolStripAnalysis.TabIndex = 1;
            this.toolStripAnalysis.Text = "toolStrip1";
            // 
            // toolStripButtonReport
            // 
            this.toolStripButtonReport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonReport.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonReport.Image")));
            this.toolStripButtonReport.ImageTransparentColor = System.Drawing.Color.White;
            this.toolStripButtonReport.Name = "toolStripButtonReport";
            this.toolStripButtonReport.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonReport.Text = "toolStripButton1";
            // 
            // chkbInterlayer
            // 
            this.chkbInterlayer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkbInterlayer.AutoSize = true;
            this.chkbInterlayer.Location = new System.Drawing.Point(6, 19);
            this.chkbInterlayer.Name = "chkbInterlayer";
            this.chkbInterlayer.Size = new System.Drawing.Size(69, 17);
            this.chkbInterlayer.TabIndex = 3;
            this.chkbInterlayer.Text = "Interlayer";
            this.chkbInterlayer.UseVisualStyleBackColor = true;
            this.chkbInterlayer.Click += new System.EventHandler(this.onChkbInterlayerClicked);
            // 
            // cbInterlater
            // 
            this.cbInterlater.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbInterlater.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbInterlater.FormattingEnabled = true;
            this.cbInterlater.Location = new System.Drawing.Point(116, 17);
            this.cbInterlater.Name = "cbInterlater";
            this.cbInterlater.Size = new System.Drawing.Size(121, 21);
            this.cbInterlater.TabIndex = 4;
            this.cbInterlater.SelectedIndexChanged += new System.EventHandler(this.onInterlayerChanged);
            // 
            // bnSymmetryX
            // 
            this.bnSymmetryX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bnSymmetryX.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnSymmetryX.Image = ((System.Drawing.Image)(resources.GetObject("bnSymmetryX.Image")));
            this.bnSymmetryX.Location = new System.Drawing.Point(6, 67);
            this.bnSymmetryX.Name = "bnSymmetryX";
            this.bnSymmetryX.Size = new System.Drawing.Size(42, 38);
            this.bnSymmetryX.TabIndex = 5;
            this.bnSymmetryX.UseVisualStyleBackColor = true;
            this.bnSymmetryX.Click += new System.EventHandler(this.onReflectionX);
            // 
            // bnSymetryY
            // 
            this.bnSymetryY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bnSymetryY.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bnSymetryY.Image = ((System.Drawing.Image)(resources.GetObject("bnSymetryY.Image")));
            this.bnSymetryY.Location = new System.Drawing.Point(54, 67);
            this.bnSymetryY.Name = "bnSymetryY";
            this.bnSymetryY.Size = new System.Drawing.Size(42, 38);
            this.bnSymetryY.TabIndex = 6;
            this.bnSymetryY.UseVisualStyleBackColor = true;
            this.bnSymetryY.Click += new System.EventHandler(this.onReflectionY);
            // 
            // cbLayerType
            // 
            this.cbLayerType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbLayerType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbLayerType.FormattingEnabled = true;
            this.cbLayerType.Location = new System.Drawing.Point(116, 44);
            this.cbLayerType.Name = "cbLayerType";
            this.cbLayerType.Size = new System.Drawing.Size(121, 21);
            this.cbLayerType.TabIndex = 7;
            this.cbLayerType.SelectedIndexChanged += new System.EventHandler(this.onLayerTypeChanged);
            // 
            // graphCtrlSolution
            // 
            this.graphCtrlSolution.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.graphCtrlSolution.Location = new System.Drawing.Point(-4, 28);
            this.graphCtrlSolution.Name = "graphCtrlSolution";
            this.graphCtrlSolution.Size = new System.Drawing.Size(683, 412);
            this.graphCtrlSolution.TabIndex = 0;
            this.graphCtrlSolution.Viewer = null;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Layer pattern";
            // 
            // gbLayer
            // 
            this.gbLayer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.gbLayer.Controls.Add(this.chkbInterlayer);
            this.gbLayer.Controls.Add(this.label1);
            this.gbLayer.Controls.Add(this.cbInterlater);
            this.gbLayer.Controls.Add(this.cbLayerType);
            this.gbLayer.Controls.Add(this.bnSymmetryX);
            this.gbLayer.Controls.Add(this.bnSymetryY);
            this.gbLayer.Location = new System.Drawing.Point(4, 446);
            this.gbLayer.Name = "gbLayer";
            this.gbLayer.Size = new System.Drawing.Size(246, 113);
            this.gbLayer.TabIndex = 9;
            this.gbLayer.TabStop = false;
            this.gbLayer.Text = "Double-click a layer...";
            // 
            // rtb
            // 
            this.rtb.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.rtb.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.rtb.Location = new System.Drawing.Point(256, 446);
            this.rtb.Name = "rtb";
            this.rtb.Size = new System.Drawing.Size(140, 113);
            this.rtb.TabIndex = 10;
            this.rtb.Text = "";
            // 
            // tabCtrl
            // 
            this.tabCtrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabCtrl.Controls.Add(this.tabPagePalletCorners);
            this.tabCtrl.Controls.Add(this.tabPagePalletCap);
            this.tabCtrl.Controls.Add(this.tabPagePalletFilm);
            this.tabCtrl.Location = new System.Drawing.Point(403, 446);
            this.tabCtrl.Multiline = true;
            this.tabCtrl.Name = "tabCtrl";
            this.tabCtrl.SelectedIndex = 0;
            this.tabCtrl.Size = new System.Drawing.Size(280, 113);
            this.tabCtrl.TabIndex = 11;
            // 
            // tabPagePalletCorners
            // 
            this.tabPagePalletCorners.Location = new System.Drawing.Point(4, 22);
            this.tabPagePalletCorners.Name = "tabPagePalletCorners";
            this.tabPagePalletCorners.Padding = new System.Windows.Forms.Padding(3);
            this.tabPagePalletCorners.Size = new System.Drawing.Size(272, 87);
            this.tabPagePalletCorners.TabIndex = 0;
            this.tabPagePalletCorners.Text = "Pallet Corners";
            this.tabPagePalletCorners.UseVisualStyleBackColor = true;
            // 
            // tabPagePalletCap
            // 
            this.tabPagePalletCap.Location = new System.Drawing.Point(4, 22);
            this.tabPagePalletCap.Name = "tabPagePalletCap";
            this.tabPagePalletCap.Padding = new System.Windows.Forms.Padding(3);
            this.tabPagePalletCap.Size = new System.Drawing.Size(272, 87);
            this.tabPagePalletCap.TabIndex = 1;
            this.tabPagePalletCap.Text = "Pallet Cap";
            this.tabPagePalletCap.UseVisualStyleBackColor = true;
            // 
            // tabPagePalletFilm
            // 
            this.tabPagePalletFilm.Location = new System.Drawing.Point(4, 22);
            this.tabPagePalletFilm.Name = "tabPagePalletFilm";
            this.tabPagePalletFilm.Padding = new System.Windows.Forms.Padding(3);
            this.tabPagePalletFilm.Size = new System.Drawing.Size(272, 87);
            this.tabPagePalletFilm.TabIndex = 2;
            this.tabPagePalletFilm.Text = "Pallet Film";
            this.tabPagePalletFilm.UseVisualStyleBackColor = true;
            // 
            // tbClickLayer
            // 
            this.tbClickLayer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tbClickLayer.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbClickLayer.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbClickLayer.Location = new System.Drawing.Point(4, 477);
            this.tbClickLayer.Multiline = true;
            this.tbClickLayer.Name = "tbClickLayer";
            this.tbClickLayer.Size = new System.Drawing.Size(246, 57);
            this.tbClickLayer.TabIndex = 12;
            this.tbClickLayer.Text = "Double-click a layer to edit pattern / orientation / interlayer.";
            // 
            // DockContentAnalysisCasePallet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 561);
            this.Controls.Add(this.tabCtrl);
            this.Controls.Add(this.rtb);
            this.Controls.Add(this.gbLayer);
            this.Controls.Add(this.toolStripAnalysis);
            this.Controls.Add(this.graphCtrlSolution);
            this.Controls.Add(this.tbClickLayer);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DockContentAnalysisCasePallet";
            this.Text = "Case/Pallet analysis...";
            this.toolStripAnalysis.ResumeLayout(false);
            this.toolStripAnalysis.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.graphCtrlSolution)).EndInit();
            this.gbLayer.ResumeLayout(false);
            this.gbLayer.PerformLayout();
            this.tabCtrl.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Graphics.Graphics3DControl graphCtrlSolution;
        private System.Windows.Forms.ToolStrip toolStripAnalysis;
        private System.Windows.Forms.CheckBox chkbInterlayer;
        private System.Windows.Forms.ComboBox cbInterlater;
        private System.Windows.Forms.Button bnSymmetryX;
        private System.Windows.Forms.Button bnSymetryY;
        private System.Windows.Forms.ComboBox cbLayerType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox gbLayer;
        private System.Windows.Forms.RichTextBox rtb;
        private System.Windows.Forms.TabControl tabCtrl;
        private System.Windows.Forms.TabPage tabPagePalletCorners;
        private System.Windows.Forms.TabPage tabPagePalletCap;
        private System.Windows.Forms.ToolStripButton toolStripButtonReport;
        private System.Windows.Forms.TabPage tabPagePalletFilm;
        private System.Windows.Forms.TextBox tbClickLayer;

    }
}