using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Numerics;

namespace ImpulseMaker
{
    internal class MyChart : Chart
    {
        int highlightbutton = -1;
        RectangleF chart_area,
            options_area,
            zoom_options_area,
            zoom_area;
        bool is_in_ca = false,
            is_left = true,
            is_enter = true,
            is_mousedown = false,
            recalc_chart_area = false;
        float highligt_x,
            highligt_y,
            tozoom_x,
            tozoom_y;
        List<chart_coord> zoom_history = new List<chart_coord>();

        struct chart_coord
        {
            public chart_coord(double x_min_in, double x_max_in, double y_min_in, double y_max_in)
            {
                x_min = x_min_in;
                x_max = x_max_in;
                y_min = y_min_in;
                y_max = y_max_in;
            }
            public double x_min;
            public double x_max;
            public double y_min;
            public double y_max;
        }

        public bool is_in_chart_area
        {
            get { return is_in_ca; }
            set 
            { 
                if (is_in_ca != value)
                    Invalidate();
                is_in_ca = value;
            }
        }

        public MyChart()
        {
            //avoid flickering
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            this.PostPaint += new System.EventHandler<System.Windows.Forms.DataVisualization.Charting.ChartPaintEventArgs>(MyChart_PostPaint);
            this.MouseMove += new MouseEventHandler(MyChart_MouseMove);
            this.MouseLeave += new EventHandler(MyChart_MouseLeave);
            this.MouseEnter += new EventHandler(MyChart_MouseEnter);
            this.MouseClick += new MouseEventHandler(MyChart_MouseClick);
            this.MouseDown += new MouseEventHandler(MyChart_MouseDown);
            this.MouseUp += new MouseEventHandler(MyChart_MouseUp);
        }

        void MyChart_PostPaint(object sender, System.Windows.Forms.DataVisualization.Charting.ChartPaintEventArgs e)
        {
            if (e.ChartElement != this)
                return;

            if (is_enter)
            {
                options_area = new RectangleF((float)(this.Width - 80), (float)(this.Height * 0.85), 46, 20);
                zoom_options_area = new RectangleF((float)(this.Width - 80 + 1), (float)(this.Height * 0.85 - 46), 20, 46);
            }

            if (is_enter || recalc_chart_area)
            {
                float x_left = (float)e.ChartGraphics.GetPositionFromAxis(this.ChartAreas[0].Name,
                    System.Windows.Forms.DataVisualization.Charting.AxisName.X, this.ChartAreas[0].AxisX.Minimum);
                float x_right = (float)e.ChartGraphics.GetPositionFromAxis(this.ChartAreas[0].Name,
                    System.Windows.Forms.DataVisualization.Charting.AxisName.X, this.ChartAreas[0].AxisX.Maximum);
                float y_top = (float)e.ChartGraphics.GetPositionFromAxis(this.ChartAreas[0].Name,
                    System.Windows.Forms.DataVisualization.Charting.AxisName.Y, this.ChartAreas[0].AxisY.Maximum);
                float y_bottom = (float)e.ChartGraphics.GetPositionFromAxis(this.ChartAreas[0].Name,
                    System.Windows.Forms.DataVisualization.Charting.AxisName.Y, this.ChartAreas[0].AxisY.Minimum);
                chart_area = e.ChartGraphics.GetAbsoluteRectangle(new RectangleF(new PointF(x_left, y_top),
                    new SizeF(x_right - x_left, y_bottom - y_top)));

                is_enter = false;
                recalc_chart_area = false;
            }

            if (this.Cursor == Cursors.NoMove2D)
            {
                Rectangle zoom_options = Rectangle.Round(zoom_options_area);
                GraphicsPath zoom_path = RoundedRectangle.Create(zoom_options, 3);
                e.ChartGraphics.Graphics.FillPath(Brushes.LightGray, zoom_path);

                Rectangle option_backzoom = new Rectangle(new Point(zoom_options.X + 1, zoom_options.Y + 2),
                    new Size(18, 18));
                zoom_path = RoundedRectangle.Create(option_backzoom, 3);
                e.ChartGraphics.Graphics.FillPath(highlightbutton == 2 ? Brushes.LightBlue : Brushes.WhiteSmoke, zoom_path);
                e.ChartGraphics.Graphics.DrawImage(Properties.Resources.Zoom_back,
                    new Rectangle(zoom_options.X + 1, zoom_options.Y + 2, 18, 18),
                    new Rectangle(0, 0, Properties.Resources.Zoom_back.Width, Properties.Resources.Zoom_back.Height),
                    GraphicsUnit.Pixel);

                Rectangle option_returnarea = new Rectangle(new Point(zoom_options.X + 1, zoom_options.Y + 23),
                    new Size(18, 18));
                zoom_path = RoundedRectangle.Create(option_returnarea, 3);
                e.ChartGraphics.Graphics.FillPath(highlightbutton == 3 ? Brushes.LightBlue : Brushes.WhiteSmoke, zoom_path);                
                e.ChartGraphics.Graphics.DrawImage(Properties.Resources.Zoom_Back_To_Whole,
                    new Rectangle(zoom_options.X + 1, zoom_options.Y + 23, 18, 18),
                    new Rectangle(0, 0, Properties.Resources.Zoom_Back_To_Whole.Width, Properties.Resources.Zoom_Back_To_Whole.Height),
                    GraphicsUnit.Pixel);
            }

            Rectangle options = Rectangle.Round(options_area);
            GraphicsPath path = RoundedRectangle.Create(options, 3);
            e.ChartGraphics.Graphics.FillPath(Brushes.LightGray, path);

            Rectangle option_zoom = new Rectangle(new Point(options.X + 2, options.Y + 1),
                new Size(18, 18));
            path = RoundedRectangle.Create(option_zoom, 3);
            e.ChartGraphics.Graphics.FillPath(highlightbutton == 0 || this.Cursor == Cursors.NoMove2D ? Brushes.LightBlue : Brushes.WhiteSmoke, path);
            e.ChartGraphics.Graphics.DrawImage(Properties.Resources.Zoom_in,
                new Rectangle(options.X + 2, options.Y + 1, 18, 18),
                new Rectangle(0, 0, Properties.Resources.Zoom_in.Width, Properties.Resources.Zoom_in.Height),
                GraphicsUnit.Pixel);

            Rectangle option_addpoint = new Rectangle(new Point(option_zoom.X + 23, option_zoom.Y),
                new Size(18, 18));
            path = RoundedRectangle.Create(option_addpoint, 3);
            e.ChartGraphics.Graphics.FillPath(highlightbutton == 1 || this.Cursor == Cursors.Cross ? Brushes.LightBlue : Brushes.WhiteSmoke, path);
            e.ChartGraphics.Graphics.DrawImage(Properties.Resources.Add_point,
                new Rectangle(option_zoom.X + 23, option_zoom.Y + 1, 18, 18),
                new Rectangle(0, 0, Properties.Resources.Add_point.Width, Properties.Resources.Add_point.Height),
                GraphicsUnit.Pixel);

            if (is_in_chart_area && !is_left)
            {
                float[] dashValues = { 5, 5 };
                Pen pen = new Pen(Color.Black, 0.5f);
                pen.DashPattern = dashValues;

                e.ChartGraphics.Graphics.DrawLine(pen, chart_area.X, highligt_y, chart_area.X + chart_area.Width, highligt_y);
                e.ChartGraphics.Graphics.DrawLine(pen, highligt_x, chart_area.Y, highligt_x, chart_area.Y + chart_area.Height);

                e.ChartGraphics.Graphics.DrawString(Math.Round(this.ChartAreas[0].AxisX.PixelPositionToValue(highligt_x),
                    MathDecimals.GetDecimalPlaces((decimal)this.ChartAreas[0].AxisX.Maximum) + 2).ToString(),
                    this.Font, Brushes.Black, highligt_x, chart_area.Y - 20);
                e.ChartGraphics.Graphics.DrawString(Math.Round(this.ChartAreas[0].AxisY.PixelPositionToValue(highligt_y),
                    MathDecimals.GetDecimalPlaces((decimal)this.ChartAreas[0].AxisX.Maximum) + 2).ToString(),
                    this.Font, Brushes.Black, chart_area.X + chart_area.Width + 5, highligt_y);
            }

            if (is_in_chart_area && this.Cursor == Cursors.NoMove2D && is_mousedown)
            {
                e.ChartGraphics.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(25, Color.Red)), zoom_area);
            }
        }

        void MyChart_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.X > options_area.X + 2 && e.X < options_area.X + 20 &&
                e.Y > options_area.Y + 1 && e.Y < options_area.Y + 19)
            {
                highlightbutton = 0;
                Invalidate(new Rectangle((int)options_area.X,
                    (int)zoom_options_area.Y,
                    (int)(options_area.X + options_area.Width),
                    (int)(zoom_options_area.Y + zoom_options_area.Height)));
            }
            else if (e.X > options_area.X + 2 + 23 && e.X < options_area.X + 20 + 23 &&
                    e.Y > options_area.Y + 1 && e.Y < options_area.Y + 19)
            {
                highlightbutton = 1;
                Invalidate(new Rectangle((int)options_area.X,
                    (int)zoom_options_area.Y,
                    (int)(options_area.X + options_area.Width),
                    (int)(zoom_options_area.Y + zoom_options_area.Height)));
            }
            else if (e.X > zoom_options_area.X + 1 && e.X < zoom_options_area.X + 19 &&
                    e.Y > zoom_options_area.Y + 2 && e.Y < zoom_options_area.Y + 20)
            {
                highlightbutton = 2;
                Invalidate(new Rectangle((int)options_area.X,
                    (int)zoom_options_area.Y,
                    (int)(options_area.X + options_area.Width),
                    (int)(zoom_options_area.Y + zoom_options_area.Height)));
            }
            else if (e.X > zoom_options_area.X + 1 && e.X < zoom_options_area.X + 19 &&
                    e.Y > zoom_options_area.Y + 23 && e.Y < zoom_options_area.Y + 41)
            {
                highlightbutton = 3;
                Invalidate(new Rectangle((int)options_area.X,
                    (int)zoom_options_area.Y,
                    (int)(options_area.X + options_area.Width),
                    (int)(zoom_options_area.Y + zoom_options_area.Height)));
            }
            else
            {
                highlightbutton = -1;
                Invalidate(new Rectangle((int)options_area.X,
                    (int)zoom_options_area.Y,
                    (int)(options_area.X + options_area.Width),
                    (int)(zoom_options_area.Y + zoom_options_area.Height)));
            }

            if (e.X > chart_area.X && e.X < chart_area.X + chart_area.Width &&
                e.Y > chart_area.Y && e.Y < chart_area.Y + chart_area.Height)
            {
                is_in_chart_area = true;
                highligt_x = e.X;
                highligt_y = e.Y;

                Invalidate();
            }  
            else is_in_chart_area = false;

            if (this.Cursor == Cursors.NoMove2D)
            {
                zoom_area = new RectangleF(tozoom_x > e.X ? e.X : tozoom_x,
                    tozoom_y > e.Y ? e.Y : tozoom_y, Math.Abs(e.X - tozoom_x), Math.Abs(e.Y - tozoom_y));
            }
        }

        void MyChart_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.X > options_area.X + 2 && e.X < options_area.X + 20 &&
                e.Y > options_area.Y + 1 && e.Y < options_area.Y + 19)
            {
                if (this.Cursor != Cursors.NoMove2D)
                {
                    this.Cursor = Cursors.NoMove2D;

                    zoom_history.Clear();
                }  
                else
                    this.Cursor = Cursors.Default;
            }

            if (e.X > zoom_options_area.X + 1 && e.X < zoom_options_area.X + 19 &&
                e.Y > zoom_options_area.Y + 2 && e.Y < zoom_options_area.Y + 20)
            {
                if (this.Cursor == Cursors.NoMove2D && zoom_history.Count > 0)
                {
                    this.ChartAreas[0].AxisX.Minimum = zoom_history.Last().x_min;
                    this.ChartAreas[0].AxisX.Maximum = zoom_history.Last().x_max;
                    this.ChartAreas[0].AxisY.Minimum = zoom_history.Last().y_min;
                    this.ChartAreas[0].AxisY.Maximum = zoom_history.Last().y_max;

                    this.ChartAreas[0].AxisX.Interval = (this.ChartAreas[0].AxisX.Maximum - this.ChartAreas[0].AxisX.Minimum) / 10;

                    zoom_history.RemoveAt(zoom_history.Count - 1);

                    if (zoom_history.Count <= 0)
                    {
                        this.ChartAreas[0].AxisY.Minimum = Double.NaN;
                        this.ChartAreas[0].AxisY.Maximum = Double.NaN;
                    }

                    recalc_chart_area = true;
                }
            }

            if (e.X > zoom_options_area.X + 1 && e.X < zoom_options_area.X + 19 &&
                e.Y > zoom_options_area.Y + 23 && e.Y < zoom_options_area.Y + 41)
            {
                if (this.Cursor == Cursors.NoMove2D && zoom_history.Count > 0)
                {
                    this.ChartAreas[0].AxisX.Minimum = zoom_history.First().x_min;
                    this.ChartAreas[0].AxisX.Maximum = zoom_history.First().x_max;
                    this.ChartAreas[0].AxisY.Minimum = Double.NaN;
                    this.ChartAreas[0].AxisY.Maximum = Double.NaN;

                    this.ChartAreas[0].AxisX.Interval = (this.ChartAreas[0].AxisX.Maximum - this.ChartAreas[0].AxisX.Minimum) / 10;

                    zoom_history.Clear();

                    recalc_chart_area = true;
                }
            }

            if (e.X > options_area.X + 2 + 23 && e.X < options_area.X + 20 + 23 &&
                e.Y > options_area.Y + 1 && e.Y < options_area.Y + 19)
            {
                if (this.Cursor != Cursors.Cross)
                    this.Cursor = Cursors.Cross;
                else
                    this.Cursor = Cursors.Default;
            }

            if (!((e.X > options_area.X + 2 && e.X < options_area.X + 20 &&
                e.Y > options_area.Y + 1 && e.Y < options_area.Y + 19) ||
                (e.X > options_area.X + 2 + 23 && e.X < options_area.X + 20 + 23 &&
                e.Y > options_area.Y + 1 && e.Y < options_area.Y + 19) ||
                (e.X > zoom_options_area.X + 1 && e.X < zoom_options_area.X + 19 &&
                e.Y > zoom_options_area.Y + 2 && e.Y < zoom_options_area.Y + 20) ||
                (e.X > zoom_options_area.X + 1 && e.X < zoom_options_area.X + 19 &&
                e.Y > zoom_options_area.Y + 23 && e.Y < zoom_options_area.Y + 41)) && !is_in_chart_area)
                this.Cursor = Cursors.Default;
        }

        void MyChart_MouseDown(object sender, MouseEventArgs e)
        {
            if (is_in_chart_area && this.Cursor == Cursors.NoMove2D)
            {
                tozoom_x = e.X;
                tozoom_y = e.Y;
                is_mousedown = true;
            }
        }

        void MyChart_MouseUp(object sender, MouseEventArgs e)
        {
            is_mousedown = false;

            if (is_in_chart_area && this.Cursor == Cursors.NoMove2D)
            {
                zoom_history.Add(new chart_coord((double)this.ChartAreas[0].AxisX.Minimum,
                                                (double)this.ChartAreas[0].AxisX.Maximum,
                                                (double)this.ChartAreas[0].AxisY.Minimum,
                                                (double)this.ChartAreas[0].AxisY.Maximum));

                double v1 = Math.Round(this.ChartAreas[0].AxisX.PixelPositionToValue(zoom_area.X),
                    MathDecimals.GetDecimalPlaces((decimal)this.ChartAreas[0].AxisX.Maximum) + 2);
                double v2 = Math.Round(this.ChartAreas[0].AxisX.PixelPositionToValue(zoom_area.X + zoom_area.Width),
                    MathDecimals.GetDecimalPlaces((decimal)this.ChartAreas[0].AxisX.Maximum) + 2);
                this.ChartAreas[0].AxisX.Minimum = v1;
                this.ChartAreas[0].AxisX.Maximum = v2;

                double v3 = Math.Round(this.ChartAreas[0].AxisY.PixelPositionToValue(zoom_area.Y + zoom_area.Height),
                    MathDecimals.GetDecimalPlaces((decimal)this.ChartAreas[0].AxisX.Maximum) + 2);
                double v4 = Math.Round(this.ChartAreas[0].AxisY.PixelPositionToValue(zoom_area.Y),
                    MathDecimals.GetDecimalPlaces((decimal)this.ChartAreas[0].AxisX.Maximum) + 2);
                this.ChartAreas[0].AxisY.Minimum = v3;
                this.ChartAreas[0].AxisY.Maximum = v4;

                this.ChartAreas[0].AxisX.Interval = (this.ChartAreas[0].AxisX.Maximum - this.ChartAreas[0].AxisX.Minimum) / 10;

                recalc_chart_area = true;
            }
        }

        void MyChart_MouseEnter(object sender, EventArgs e)
        {
            is_left = false;
            is_enter = true;
        }

        void MyChart_MouseLeave(object sender, EventArgs e)
        {
            is_left = true;
            is_enter = false;
            Invalidate();
        }
    }
}
