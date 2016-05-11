namespace treeDiM.StackBuilder.Desktop
{
    partial class FormNewAnalysisCasePallet
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormNewAnalysisCasePallet));
            this.cbPallets = new System.Windows.Forms.ComboBox();
            this.cbCases = new System.Windows.Forms.ComboBox();
            this.lbBox = new System.Windows.Forms.Label();
            this.uCtrlOptMaximumWeight = new treeDiM.StackBuilder.Basics.UCtrlOptDouble();
            this.uCtrlOptMaximumHeight = new treeDiM.StackBuilder.Basics.UCtrlOptDouble();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.uCtrlOverhang = new treeDiM.StackBuilder.Basics.UCtrlDualDouble();
            this.lbPallet = new System.Windows.Forms.Label();
            this.uCtrlLayerList = new treeDiM.StackBuilder.Graphics.UCtrlLayerList();
            this.checkBoxBestLayersOnly = new System.Windows.Forms.CheckBox();
            this.bnNext = new System.Windows.Forms.Button();
            this.bnBestCombination = new System.Windows.Forms.Button();
            this.uCtrlCaseOrientation = new treeDiM.StackBuilder.Graphics.uCtrlCaseOrientation();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cbPallets
            // 
            this.cbPallets.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbPallets.FormattingEnabled = true;
            resources.ApplyResources(this.cbPallets, "cbPallets");
            this.cbPallets.Name = "cbPallets";
            this.cbPallets.SelectedIndexChanged += new System.EventHandler(this.onInputChanged);
            // 
            // cbCases
            // 
            this.cbCases.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbCases.FormattingEnabled = true;
            resources.ApplyResources(this.cbCases, "cbCases");
            this.cbCases.Name = "cbCases";
            this.cbCases.SelectedIndexChanged += new System.EventHandler(this.onCaseChanged);
            // 
            // lbBox
            // 
            resources.ApplyResources(this.lbBox, "lbBox");
            this.lbBox.Name = "lbBox";
            // 
            // uCtrlOptMaximumWeight
            // 
            resources.ApplyResources(this.uCtrlOptMaximumWeight, "uCtrlOptMaximumWeight");
            this.uCtrlOptMaximumWeight.Minimum = new decimal(new int[] {
            10000,
            0,
            0,
            -2147483648});
            this.uCtrlOptMaximumWeight.Name = "uCtrlOptMaximumWeight";
            this.uCtrlOptMaximumWeight.Unit = treeDiM.StackBuilder.Basics.UnitsManager.UnitType.UT_MASS;
            this.uCtrlOptMaximumWeight.Value = ((treeDiM.StackBuilder.Basics.OptDouble)(resources.GetObject("uCtrlOptMaximumWeight.Value")));
            this.uCtrlOptMaximumWeight.ValueChanged += new treeDiM.StackBuilder.Basics.UCtrlOptDouble.onValueChanged(this.onInputChanged);
            // 
            // uCtrlOptMaximumHeight
            // 
            resources.ApplyResources(this.uCtrlOptMaximumHeight, "uCtrlOptMaximumHeight");
            this.uCtrlOptMaximumHeight.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            65536});
            this.uCtrlOptMaximumHeight.Name = "uCtrlOptMaximumHeight";
            this.uCtrlOptMaximumHeight.Unit = treeDiM.StackBuilder.Basics.UnitsManager.UnitType.UT_LENGTH;
            this.uCtrlOptMaximumHeight.Value = ((treeDiM.StackBuilder.Basics.OptDouble)(resources.GetObject("uCtrlOptMaximumHeight.Value")));
            this.uCtrlOptMaximumHeight.ValueChanged += new treeDiM.StackBuilder.Basics.UCtrlOptDouble.onValueChanged(this.onInputChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.uCtrlOptMaximumWeight);
            this.groupBox1.Controls.Add(this.uCtrlOptMaximumHeight);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // uCtrlOverhang
            // 
            resources.ApplyResources(this.uCtrlOverhang, "uCtrlOverhang");
            this.uCtrlOverhang.Name = "uCtrlOverhang";
            this.uCtrlOverhang.Unit = treeDiM.StackBuilder.Basics.UnitsManager.UnitType.UT_LENGTH;
            this.uCtrlOverhang.ValueX = 0D;
            this.uCtrlOverhang.ValueY = 0D;
            this.uCtrlOverhang.ValueChanged += new treeDiM.StackBuilder.Basics.UCtrlDualDouble.onValueChanged(this.onInputChanged);
            // 
            // lbPallet
            // 
            resources.ApplyResources(this.lbPallet, "lbPallet");
            this.lbPallet.Name = "lbPallet";
            // 
            // uCtrlLayerList
            // 
            resources.ApplyResources(this.uCtrlLayerList, "uCtrlLayerList");
            this.uCtrlLayerList.ButtonSizes = new System.Drawing.Size(150, 150);
            this.uCtrlLayerList.Name = "uCtrlLayerList";
            // 
            // checkBoxBestLayersOnly
            // 
            resources.ApplyResources(this.checkBoxBestLayersOnly, "checkBoxBestLayersOnly");
            this.checkBoxBestLayersOnly.Name = "checkBoxBestLayersOnly";
            this.checkBoxBestLayersOnly.UseVisualStyleBackColor = true;
            this.checkBoxBestLayersOnly.CheckedChanged += new System.EventHandler(this.onInputChanged);
            // 
            // bnNext
            // 
            resources.ApplyResources(this.bnNext, "bnNext");
            this.bnNext.Name = "bnNext";
            this.bnNext.UseVisualStyleBackColor = true;
            this.bnNext.Click += new System.EventHandler(this.onBnNextClicked);
            // 
            // bnBestCombination
            // 
            resources.ApplyResources(this.bnBestCombination, "bnBestCombination");
            this.bnBestCombination.Name = "bnBestCombination";
            this.bnBestCombination.UseVisualStyleBackColor = true;
            // 
            // uCtrlCaseOrientation
            // 
            this.uCtrlCaseOrientation.AllowedOrientations = new bool[] {
        false,
        false,
        false};
            resources.ApplyResources(this.uCtrlCaseOrientation, "uCtrlCaseOrientation");
            this.uCtrlCaseOrientation.Name = "uCtrlCaseOrientation";
            this.uCtrlCaseOrientation.CheckedChanged += new treeDiM.StackBuilder.Graphics.uCtrlCaseOrientation.CheckChanged(this.onInputChanged);
            // 
            // FormNewAnalysisCasePallet
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.bnBestCombination);
            this.Controls.Add(this.bnNext);
            this.Controls.Add(this.checkBoxBestLayersOnly);
            this.Controls.Add(this.uCtrlLayerList);
            this.Controls.Add(this.uCtrlCaseOrientation);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.uCtrlOverhang);
            this.Controls.Add(this.cbPallets);
            this.Controls.Add(this.cbCases);
            this.Controls.Add(this.lbPallet);
            this.Controls.Add(this.lbBox);
            this.Name = "FormNewAnalysisCasePallet";
            this.TopMost = true;
            this.Controls.SetChildIndex(this.lbBox, 0);
            this.Controls.SetChildIndex(this.lbPallet, 0);
            this.Controls.SetChildIndex(this.cbCases, 0);
            this.Controls.SetChildIndex(this.cbPallets, 0);
            this.Controls.SetChildIndex(this.uCtrlOverhang, 0);
            this.Controls.SetChildIndex(this.groupBox1, 0);
            this.Controls.SetChildIndex(this.uCtrlCaseOrientation, 0);
            this.Controls.SetChildIndex(this.uCtrlLayerList, 0);
            this.Controls.SetChildIndex(this.checkBoxBestLayersOnly, 0);
            this.Controls.SetChildIndex(this.bnNext, 0);
            this.Controls.SetChildIndex(this.bnBestCombination, 0);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbPallets;
        private System.Windows.Forms.ComboBox cbCases;
        private System.Windows.Forms.Label lbBox;
        private Basics.UCtrlOptDouble uCtrlOptMaximumWeight;
        private Basics.UCtrlOptDouble uCtrlOptMaximumHeight;
        private System.Windows.Forms.GroupBox groupBox1;
        private Basics.UCtrlDualDouble uCtrlOverhang;
        private System.Windows.Forms.Label lbPallet;
        private Graphics.UCtrlLayerList uCtrlLayerList;
        private System.Windows.Forms.CheckBox checkBoxBestLayersOnly;
        private System.Windows.Forms.Button bnNext;
        private System.Windows.Forms.Button bnBestCombination;
        private Graphics.uCtrlCaseOrientation uCtrlCaseOrientation;
    }
}