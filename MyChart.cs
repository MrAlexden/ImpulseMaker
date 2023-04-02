using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace ImpulseMaker
{
    internal class MyChart : Chart
    {
        private int highlightbutton = -1;

        public MyChart()
        {
            //avoid flickering
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            this.PostPaint += new System.EventHandler<System.Windows.Forms.DataVisualization.Charting.ChartPaintEventArgs>(MyChart_Paint);
            this.MouseMove += new MouseEventHandler(MyChart_MouseMove);
        }

        void MyChart_Paint(object sender, System.Windows.Forms.DataVisualization.Charting.ChartPaintEventArgs e)
        {
            Rectangle options = new Rectangle(new Point(this.Width - 80, (int)(this.Height * 0.85)),
                new Size(46, 20));
            GraphicsPath path = RoundedRectangle.Create(options, 3);
            e.ChartGraphics.Graphics.FillPath(Brushes.LightGray, path);

            Rectangle option_zoom = new Rectangle(new Point(options.X + 2, options.Y + 1),
                new Size(18, 18));
            path = RoundedRectangle.Create(option_zoom, 3);
            e.ChartGraphics.Graphics.FillPath(highlightbutton == 0 ? Brushes.LightBlue : Brushes.WhiteSmoke, path);

            Rectangle option_addpoint = new Rectangle(new Point(option_zoom.X + 23, option_zoom.Y),
                new Size(18, 18));
            path = RoundedRectangle.Create(option_addpoint, 3);
            e.ChartGraphics.Graphics.FillPath(highlightbutton == 1 ? Brushes.LightBlue : Brushes.WhiteSmoke, path);

            float x_left = (float)e.ChartGraphics.GetPositionFromAxis(e.Chart.ChartAreas[0].Name,
                System.Windows.Forms.DataVisualization.Charting.AxisName.X, this.ChartAreas[0].AxisX.Minimum);
            float x_right = (float)e.ChartGraphics.GetPositionFromAxis(e.Chart.ChartAreas[0].Name,
                System.Windows.Forms.DataVisualization.Charting.AxisName.X, this.ChartAreas[0].AxisX.Maximum);
            float y_top = (float)e.ChartGraphics.GetPositionFromAxis(e.Chart.ChartAreas[0].Name,
                System.Windows.Forms.DataVisualization.Charting.AxisName.Y, this.ChartAreas[0].AxisY.Maximum);
            float y_bottom = (float)e.ChartGraphics.GetPositionFromAxis(e.Chart.ChartAreas[0].Name,
                System.Windows.Forms.DataVisualization.Charting.AxisName.Y, this.ChartAreas[0].AxisY.Minimum);
            var chart_area = e.ChartGraphics.GetAbsoluteRectangle(new RectangleF(new PointF(x_left, y_top),
                new SizeF(x_right - x_left, y_bottom - y_top)));
            using (SolidBrush sb = new SolidBrush(Color.FromArgb(64, 255, 0, 0)))
                e.ChartGraphics.Graphics.FillRectangle(sb, Rectangle.Round(chart_area));
        }

        void MyChart_MouseMove(object sender, MouseEventArgs e)
        {
            highlightbutton = -1;

            if (e.X > this.Width - 80 + 2 && e.X < this.Width - 80 + 20 &&
                e.Y > this.Height * 0.85 + 1 && e.Y < this.Height * 0.85 + 19)
                highlightbutton = 0;

            if (e.X > this.Width - 80 + 2 + 23 && e.X < this.Width - 80 + 20 + 23 &&
                e.Y > this.Height * 0.85 + 1 && e.Y < this.Height * 0.85 + 19)
                highlightbutton = 1;

            Invalidate();
        }
    }
}
