namespace treeDiM.StackBuilder.Graphics
{
    partial class uCtrlCaseOrientation
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pictureBoxX = new System.Windows.Forms.PictureBox();
            this.checkBoxX = new System.Windows.Forms.CheckBox();
            this.pictureBoxY = new System.Windows.Forms.PictureBox();
            this.checkBoxY = new System.Windows.Forms.CheckBox();
            this.checkBoxZ = new System.Windows.Forms.CheckBox();
            this.pictureBoxZ = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxZ)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBoxX
            // 
            this.pictureBoxX.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxX.Name = "pictureBoxX";
            this.pictureBoxX.Size = new System.Drawing.Size(90, 90);
            this.pictureBoxX.TabIndex = 0;
            this.pictureBoxX.TabStop = false;
            // 
            // checkBoxX
            // 
            this.checkBoxX.AutoSize = true;
            this.checkBoxX.Location = new System.Drawing.Point(1, 92);
            this.checkBoxX.Margin = new System.Windows.Forms.Padding(1);
            this.checkBoxX.Name = "checkBoxX";
            this.checkBoxX.Size = new System.Drawing.Size(33, 17);
            this.checkBoxX.TabIndex = 1;
            this.checkBoxX.Text = "X";
            this.checkBoxX.UseVisualStyleBackColor = true;
            this.checkBoxX.CheckedChanged += new System.EventHandler(this.onCheckedChanged);
            // 
            // pictureBoxY
            // 
            this.pictureBoxY.Location = new System.Drawing.Point(95, 0);
            this.pictureBoxY.Name = "pictureBoxY";
            this.pictureBoxY.Size = new System.Drawing.Size(90, 90);
            this.pictureBoxY.TabIndex = 2;
            this.pictureBoxY.TabStop = false;
            // 
            // checkBoxY
            // 
            this.checkBoxY.AutoSize = true;
            this.checkBoxY.Location = new System.Drawing.Point(96, 92);
            this.checkBoxY.Margin = new System.Windows.Forms.Padding(1);
            this.checkBoxY.Name = "checkBoxY";
            this.checkBoxY.Size = new System.Drawing.Size(33, 17);
            this.checkBoxY.TabIndex = 3;
            this.checkBoxY.Text = "Y";
            this.checkBoxY.UseVisualStyleBackColor = true;
            this.checkBoxY.CheckedChanged += new System.EventHandler(this.onCheckedChanged);
            // 
            // checkBoxZ
            // 
            this.checkBoxZ.AutoSize = true;
            this.checkBoxZ.Location = new System.Drawing.Point(191, 92);
            this.checkBoxZ.Margin = new System.Windows.Forms.Padding(1);
            this.checkBoxZ.Name = "checkBoxZ";
            this.checkBoxZ.Size = new System.Drawing.Size(33, 17);
            this.checkBoxZ.TabIndex = 4;
            this.checkBoxZ.Text = "Z";
            this.checkBoxZ.UseVisualStyleBackColor = true;
            this.checkBoxZ.CheckedChanged += new System.EventHandler(this.onCheckedChanged);
            // 
            // pictureBoxZ
            // 
            this.pictureBoxZ.Location = new System.Drawing.Point(190, 0);
            this.pictureBoxZ.Name = "pictureBoxZ";
            this.pictureBoxZ.Size = new System.Drawing.Size(90, 90);
            this.pictureBoxZ.TabIndex = 5;
            this.pictureBoxZ.TabStop = false;
            // 
            // uCtrlCaseOrientation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pictureBoxZ);
            this.Controls.Add(this.checkBoxZ);
            this.Controls.Add(this.checkBoxY);
            this.Controls.Add(this.pictureBoxY);
            this.Controls.Add(this.checkBoxX);
            this.Controls.Add(this.pictureBoxX);
            this.Name = "uCtrlCaseOrientation";
            this.Size = new System.Drawing.Size(280, 110);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.onPaint);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxZ)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxX;
        private System.Windows.Forms.CheckBox checkBoxX;
        private System.Windows.Forms.PictureBox pictureBoxY;
        private System.Windows.Forms.CheckBox checkBoxY;
        private System.Windows.Forms.CheckBox checkBoxZ;
        private System.Windows.Forms.PictureBox pictureBoxZ;
    }
}
