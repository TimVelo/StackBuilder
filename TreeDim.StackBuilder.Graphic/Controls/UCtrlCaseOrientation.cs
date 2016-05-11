#region Using directives
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using treeDiM.StackBuilder.Basics;
#endregion

namespace treeDiM.StackBuilder.Graphics
{
    public partial class uCtrlCaseOrientation : UserControl
    {
        #region constructor
        public uCtrlCaseOrientation()
        {
            InitializeComponent();
        }
        #endregion

        #region Event handlers
        private void onPaint(object sender, PaintEventArgs e)
        {
            if (null != _bProperties)
            {
                BoxToPictureBox.Draw(_bProperties, HalfAxis.HAxis.AXIS_X_P, pictureBoxX);
                BoxToPictureBox.Draw(_bProperties, HalfAxis.HAxis.AXIS_Y_P, pictureBoxY);
                BoxToPictureBox.Draw(_bProperties, HalfAxis.HAxis.AXIS_Z_P, pictureBoxZ);
            }
        }
        private void onCheckedChanged(object sender, EventArgs e)
        {
            CheckedChanged(this, e);
        }
        #endregion

        #region Delegate and events
        public delegate void CheckChanged(object sender, EventArgs e);
        public event CheckChanged CheckedChanged;
        #endregion

        #region Public properties
        public BProperties BProperties
        {
            set
            {
                _bProperties = value;
                Invalidate();
            }
        }
        public bool[] AllowedOrientations
        {
            get { return new bool[]{ checkBoxX.Checked, checkBoxY.Checked, checkBoxZ.Checked }; }
            set { checkBoxX.Checked = value[0]; checkBoxY.Checked = value[1]; checkBoxZ.Checked = value[2]; }
        }
        public bool IsOrientationAllowed(HalfAxis.HAxis axis)
        {
            if (HalfAxis.HAxis.AXIS_X_N == axis && HalfAxis.HAxis.AXIS_X_P == axis) return checkBoxX.Checked;
            else if (HalfAxis.HAxis.AXIS_Y_N == axis && HalfAxis.HAxis.AXIS_Y_P == axis) return checkBoxY.Checked;
            else if (HalfAxis.HAxis.AXIS_Z_N == axis && HalfAxis.HAxis.AXIS_Z_P == axis) return checkBoxZ.Checked;
            else return false;
        }
        #endregion

        #region Data members
        private BProperties _bProperties;
        #endregion
    }
}
