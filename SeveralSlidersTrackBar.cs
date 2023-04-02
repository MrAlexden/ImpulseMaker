using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ImpulseMaker
{
    public static class MathDecimals
    {
        public static int GetDecimalPlaces(decimal n)
        {
            n = Math.Abs(n); //make sure it is positive.
            n -= (int)n;     //remove the integer part of the number.
            var decimalPlaces = 0;
            while (n > 0)
            {
                decimalPlaces++;
                n *= 10;
                n -= (int)n;
            }
            return decimalPlaces;
        }
    }

    /// <summary>
    /// Very basic slider control with selection range.
    /// </summary>
    [Description("Very basic slider control with selection range.")]
    public partial class SeveralSlidersTrackBar : UserControl
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

        /// <summary>
        /// Minimum value of the slider.
        /// </summary>
        [Description("Minimum value of the slider.")]
        public float Min
        {
            get { return min; }
            set { min = value; Invalidate(); }
        }
        float min = 0;
        /// <summary>
        /// Maximum value of the slider.
        /// </summary>
        [Description("Maximum value of the slider.")]
        public float Max
        {
            get { return max; }
            set
            {
                max_buff = max;
                max = value;
                if (MaxChanged != null)
                    MaxChanged(this, null);
                Invalidate();
            }
        }
        float max = 100;
        private float max_buff = 100;
        /// <summary>
        /// Minimum value of the selection range.
        /// </summary>
        [Description("Minimum value of the selection range.")]
        public float SelectedMin
        {
            get { return selectedMin; }
            set
            {
                selectedMin = value;
                if (SelectionChanged != null)
                    SelectionChanged(this, null);
                Invalidate();
            }
        }
        float selectedMin = 0;
        /// <summary>
        /// Maximum value of the selection range.
        /// </summary>
        [Description("Maximum value of the selection range.")]
        public float SelectedMax
        {
            get { return selectedMax; }
            set
            {
                selectedMax = value;
                if (SelectionChanged != null)
                    SelectionChanged(this, null);
                Invalidate();
            }
        }
        float selectedMax = 100;
        /// <summary>
        /// Fired when SelectedMin or SelectedMax changes.
        /// </summary>
        [Description("Fired when SelectedMin or SelectedMax changes.")]
        public event EventHandler SelectionChanged;
        /// <summary>
        /// Fired when Max changes.
        /// </summary>
        [Description("Fired when Max changes.")]
        public event EventHandler MaxChanged;
        /// <summary>
        /// Fired when mouse clicked on MinStringBox.
        /// </summary>
        [Description("Fired when mouse clicked on MinStringBox.")]
        public event EventHandler MinClicked;
        /// <summary>
        /// Fired when mouse clicked on MaxStringBox.
        /// </summary>
        [Description("Fired when mouse clicked on MaxStringBox.")]
        public event EventHandler MaxClicked;
        /// <summary>
        /// Fired when cursor left MinStringBox.
        /// </summary>
        [Description("Fired when cursor left MinStringBox.")]
        public event EventHandler CursorLeftMin;
        /// <summary>
        /// Fired when cursor left MaxStringBox.
        /// </summary>
        [Description("Fired when cursor left MaxStringBox.")]
        public event EventHandler CursorLeftMax;

        public struct string_rect
        {
            public SizeF rect;
            public float X;
            public float Y;
        }
        public string_rect sMin, sMax;

        public SeveralSlidersTrackBar()
        {
            InitializeComponent();

            //avoid flickering
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        public void OnMaxChanged(object sender, EventArgs e)
        {
            selectedMin = Min + selectedMin / (max_buff - Min) * (Max - Min);
            selectedMin = selectedMin < Min ? Min : (selectedMin > Max ? Max : selectedMin);

            selectedMax = Min + selectedMax / (max_buff - Min) * (Max - Min);
            selectedMax = selectedMax < Min ? Min : (selectedMax > Max ? Max : selectedMax);
        }

        void SelectionRangeSlider_Paint(object sender, PaintEventArgs e)
        {
            //fill min string rectangle data
            var font = new Font("Arial", 12);
            sMin.rect = e.Graphics.MeasureString(selectedMin.ToString(), font);
            sMin.X = Width * selectedMin / (Max - Min) < Width - sMin.rect.Width ?
                Width * selectedMin / (Max - Min) :
                Width * selectedMin / (Max - Min) - sMin.rect.Width;
            sMin.Y = Height - sMin.rect.Height;
            //fill max string rectangle data
            sMax.rect = e.Graphics.MeasureString(selectedMax.ToString(), font);
            sMax.X = Width * selectedMax / (Max - Min) < sMax.rect.Width ?
                Width * selectedMax / (Max - Min) :
                Width * selectedMax / (Max - Min) - sMax.rect.Width;
            sMax.Y = 0;

            //paint selection range in blue
            Rectangle selectionRect = new Rectangle(
                (int)((selectedMin - Min) * Width / (Max - Min)),
                (int)(sMax.rect.Height) + 1,
                (int)((selectedMax - selectedMin) * Width / (Max - Min)),
                (int)(Height - sMin.rect.Height - sMax.rect.Height + 1));
            e.Graphics.FillRectangle(Brushes.LightGray, selectionRect);

            //draw a black frame around our control
            e.Graphics.DrawRectangle(Pens.Black, 0, sMax.rect.Height, Width - 1, Height - sMin.rect.Height - sMax.rect.Height);

            e.Graphics.DrawLine(Pens.Blue,
                Width * selectedMin / (Max - Min), sMax.rect.Height,
                Width * selectedMin / (Max - Min), Height);
            e.Graphics.DrawLine(Pens.Red,
                Width * selectedMax / (Max - Min) - 1, 0,
                Width * selectedMax / (Max - Min) - 1, Height - sMin.rect.Height);

            //draw strings with min and max values
            e.Graphics.FillRectangle(Brushes.Blue, sMin.X, sMin.Y, sMin.rect.Width, sMin.rect.Height);
            e.Graphics.DrawString(selectedMin.ToString(),
                font,
                Brushes.Black,
                sMin.X,
                sMin.Y);
            e.Graphics.FillRectangle(Brushes.Red, sMax.X, sMax.Y, sMax.rect.Width, sMax.rect.Height);
            e.Graphics.DrawString(selectedMax.ToString(),
                font, 
                Brushes.Black,
                sMax.X,
                sMax.Y);
        }

        void SelectionRangeSlider_MouseClick(object sender, MouseEventArgs e)
        {
            if ((e.X > sMin.X) && (e.X < sMin.X + sMin.rect.Width) &&
                (e.Y > sMin.Y) && (e.Y < sMin.Y + sMin.rect.Height))
            {
                if (MinClicked != null)
                    MinClicked(this, null);
            }
            else if ((e.X > sMax.X) && (e.X < sMax.X + sMax.rect.Width) &&
                    (e.Y > sMax.Y) && (e.Y < sMax.Y + sMax.rect.Height))
            {
                if (MaxClicked != null)
                    MaxClicked(this, null);
            }
        }

        void SelectionRangeSlider_MouseDown(object sender, MouseEventArgs e)
        {
            //check where the user clicked so we can decide which thumb to move
            float pointedValue = Min + e.X * (Max - Min) / Width;

            float distMin = Math.Abs(pointedValue - selectedMin);
            float distMax = Math.Abs(pointedValue - selectedMax);

            if (distMin < distMax)
                movingMode = MovingMode.MovingMin;
            else if (distMin > distMax)
                movingMode = MovingMode.MovingMax;
            else if (distMin == distMax)
                movingMode = pointedValue < selectedMax ? MovingMode.MovingMin : MovingMode.MovingMax;

            //call this to refreh the position of the selected thumb
            SelectionRangeSlider_MouseMove(sender, e);
        }

        void SelectionRangeSlider_MouseMove(object sender, MouseEventArgs e)
        {
            //detecting whether cursor on MinMaxString boxes or not
            if (!((e.X > sMin.X - sMin.rect.Height) &&
                (e.X < sMin.X + sMin.rect.Width + sMin.rect.Height) &&
                (e.Y > sMin.Y - sMin.rect.Height) &&
                (e.Y < sMin.Y + sMin.rect.Height + sMin.rect.Height)))
            {
                if (CursorLeftMin != null)
                    CursorLeftMin(this, null);
            }
            if (!((e.X > sMax.X - sMin.rect.Height) &&
                (e.X < sMax.X + sMax.rect.Width + sMin.rect.Height) &&
                (e.Y > sMax.Y - sMin.rect.Height) &&
                (e.Y < sMax.Y + sMax.rect.Height + sMin.rect.Height)))
            {
                if (CursorLeftMax != null)
                    CursorLeftMax(this, null);
            }

            //if the left button is pushed, move the selected thumb
            if (e.Button != MouseButtons.Left)
                return;
            if (!(e.Y > sMax.rect.Height && e.Y < Height - sMin.rect.Height))
                return;

            float pointedValue = Min + e.X * (Max - Min) / Width;

            if (movingMode == MovingMode.MovingMin)
                SelectedMin = 
                    (float)Math.Round((double)(pointedValue < Min ? Min : pointedValue > selectedMax ? selectedMax : pointedValue),
                    MathDecimals.GetDecimalPlaces((decimal)(Max / 100)));
            else if (movingMode == MovingMode.MovingMax)
                SelectedMax = 
                    (float)Math.Round((double)(pointedValue > Max ? Max : pointedValue < selectedMin ? selectedMin : pointedValue),
                    MathDecimals.GetDecimalPlaces((decimal)(Max / 100)));
        }

        /// <summary>
        /// To know which thumb is moving
        /// </summary>
        enum MovingMode { MovingMin, MovingMax }
        MovingMode movingMode;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // SeveralSlidersTrackBar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "SeveralSlidersTrackBar";
            this.Size = new System.Drawing.Size(332, 105);
            this.ResumeLayout(false);
            this.Paint += new PaintEventHandler(SelectionRangeSlider_Paint);
            this.MouseDown += new MouseEventHandler(SelectionRangeSlider_MouseDown);
            this.MouseMove += new MouseEventHandler(SelectionRangeSlider_MouseMove);
            this.MaxChanged += new EventHandler(OnMaxChanged);
            this.MouseClick += new MouseEventHandler(SelectionRangeSlider_MouseClick);
        }

        #endregion
    }
}
