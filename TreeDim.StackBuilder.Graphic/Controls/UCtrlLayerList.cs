#region Using directives
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using log4net;

using treeDiM.StackBuilder.Basics;
#endregion

namespace treeDiM.StackBuilder.Graphics
{
    public partial class UCtrlLayerList : UserControl
    {
        #region Constants
        private Size szButtons = new Size(150, 150);
        #endregion

        #region Data members
        protected static readonly ILog _log = LogManager.GetLogger(typeof(UCtrlLayerList));
        private List<Layer2D> _layerList = new List<Layer2D>();
        private BProperties _bProperties;
        private int _index;
        private int _x, _y;
        private ToolTip tooltip = new ToolTip();
        private double _contHeight = 0.0;
        #endregion

        #region Delegate
        public delegate void LayerButtonClicked(object sender, EventArgs e);
        #endregion

        #region Event handlers
        public event LayerButtonClicked LayerSelected; 
        #endregion

        #region Constructor
        public UCtrlLayerList()
        {
            InitializeComponent();
            AutoScroll = true;
        }
        #endregion

        #region Overrides user control
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            AutoScrollPosition = Point.Empty;
            int x = 0, y = 0;
            foreach (Control cntl in Controls)
            {
                cntl.Location = new Point(x, y) + (Size)AutoScrollPosition;
                AdjustXY(ref x, ref y);
            }
        }
        #endregion

        #region Public properties
        [Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<Layer2D> LayerList
        {
            get { return _layerList; }
            set
            {
                _layerList = value;
                Start();
            }
        }
        [Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double ContainerHeight
        {
            get { return _contHeight; }
            set
            {
                _contHeight = value;
                Start();
            }
        }
        public BProperties BProperties
        {
            set { _bProperties = value; }
        }
        public Size ButtonSizes
        {
            get { return szButtons; }
            set
            {
                szButtons = value;
                Start();
            }
        }
        public Layer2D[] Selected
        {
            get
            {
                List<Layer2D> layers = new List<Layer2D>();
                foreach (Control ctrl in this.Controls)
                {
                    Button button = ctrl as Button;
                    if (null != button)
                    {
                        LayerItem item = button.Tag as LayerItem;
                        if (item.Selected)
                            layers.Add(item.Layer);
                    }
                }
                return layers.ToArray();
            }
        }
        #endregion

        #region Event handler
        private void Start()
        {
            Controls.Clear();
            _index = 0; _x = 0; _y = 0;
            _timer.Interval = 50;
            _timer.Start();        
        }
        private void onTimerTick(object sender, EventArgs e)
        {
            if (_index == _layerList.Count)
            {
                _timer.Stop();
                return;
            }
            Layer2D layer = _layerList[_index];
            // create button and add to panel
            Button btn = new Button();
            btn.Image = LayerToImage.Draw(_layerList[_index], _bProperties, _contHeight, szButtons, false);//bitmap;
            btn.Location = new Point(_x, _y) + (Size)AutoScrollPosition;
            btn.Size = szButtons;
            btn.Tag = new LayerItem(layer, false);
            btn.Click += onLayerSelected;
            Controls.Add(btn);

            // give button a tooltip
            tooltip.SetToolTip(btn
                , String.Format("{0} * {1} = {2}\n {3} | {4}"
                , layer.BoxCount
                , layer.NoLayers(_contHeight)
                , layer.CountInHeight(_contHeight)
                , HalfAxis.ToString(layer.AxisOrtho)
                , layer.PatternName));

            // adjust i, x and y for next image
            AdjustXY(ref _x, ref _y);
            ++_index;       
        }
        private void onLayerSelected(object sender, EventArgs e)
        {
            Button bn = sender as Button;
            LayerItem lItem = bn.Tag as LayerItem;
            bool selected = !lItem.Selected;
            bn.Image = LayerToImage.Draw(lItem.Layer, _bProperties, _contHeight, szButtons, selected);
            bn.Tag = new LayerItem(lItem.Layer, selected);
            LayerSelected(this, e);
        }
        #endregion

        #region Helper methods
        /// <summary>
        /// computes position of next button
        /// </summary>
        /// <param name="x">Abscissa</param>
        /// <param name="y">Ordinate</param>
        private void AdjustXY(ref int x, ref int y)
        {
            x += szButtons.Width;
            if (x + szButtons.Width > Width - SystemInformation.VerticalScrollBarWidth)
            {
                x = 0;
                y += szButtons.Height;
            }
        }
        #endregion
    }
    #region LayerItem
    internal class LayerItem
    {
        public LayerItem(Layer2D layer, bool selected) { Layer = layer; Selected = selected; }
        public Layer2D Layer { get; set; }
        public bool Selected { get; set; }
    }
    #endregion
}
