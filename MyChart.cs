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
using System.ComponentModel;
using System.Reflection;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using System.Xml.Linq;

namespace ImpulseMaker
{
    internal class MyChart : Chart
    {
        public event EventHandler SelectedSeriesChanged;
        public event CustomEventHandler ColorChanged;
        public delegate void CustomEventHandler(object sender, CustomEventArgs eventArgs);

        int highlightbutton = -1,
            item_to_highlight = -1,
            s_s = -1;
        RectangleF chart_area,
            options_area,
            zoom_options_area,
            zoom_area,
            series_selection_area;
        bool is_in_ca = false,
            is_left = true,
            is_enter = true,
            is_L_mouse_down = false,
            is_R_mouse_down = false,
            recalc_chart_area = false,
            e_a_p = true,
            is_a_t_ch = true,
            is_selection_set_outside = false,
            is_resized = false;
        float highligt_x,
            highligt_y,
            tozoom_x,
            tozoom_y,
            toshift_x,
            toshift_y;
        List<chart_coord> zoom_history = new List<chart_coord>();
        public series_setting[] settings = new series_setting[0];
        chart_coord default_chart_coord;
        SizeF text_size = new SizeF();
        ContextMenuStrip ChartSeriesContextMenuStrip = new ContextMenuStrip();

        public class CustomEventArgs : EventArgs
        {
            public CustomEventArgs(int index_in)
            {
                index = index_in;
            }

            public int index;
        }

        public struct series_setting
        {
            public string name;
            public Color color;
            public uint width;
        }

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

        int selected_series
        {
            get { return s_s; }
            set
            {
                if (s_s != value)
                {
                    s_s = value;
                    if (SelectedSeriesChanged != null && !is_selection_set_outside)
                        SelectedSeriesChanged(this, null);
                    highlight_series(value);
                }
            }
        }

        bool is_in_chart_area
        {
            get { return is_in_ca; }
            set 
            { 
                if (is_in_ca != value)
                    Invalidate();
                is_in_ca = value;
            }
        }

        public bool enable_add_point
        {
            get { return e_a_p; }
            set { e_a_p = value; Invalidate(); }
        }

        public bool is_able_to_choose
        {
            get { return is_a_t_ch; }
            set { is_a_t_ch = value; Invalidate(); }
        }

        public MyChart()
        {
            //avoid flickering
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            ToolStripMenuItem Color = new ToolStripMenuItem("Цвет");
            ToolStripMenuItem Width = new ToolStripMenuItem("Толщина");

            ChartSeriesContextMenuStrip.Items.AddRange(new[] { Color, Width });

            Color.Click += new EventHandler(MyChart_SeriesColorChoose);
            Width.Click += new EventHandler(MyChart_SeriesWidthChoose);

            this.PostPaint += new System.EventHandler<System.Windows.Forms.DataVisualization.Charting.ChartPaintEventArgs>(MyChart_PostPaint);
            this.MouseMove += new MouseEventHandler(MyChart_MouseMove);
            this.MouseLeave += new EventHandler(MyChart_MouseLeave);
            this.MouseEnter += new EventHandler(MyChart_MouseEnter);
            this.MouseClick += new MouseEventHandler(MyChart_MouseClick);
            this.MouseDown += new MouseEventHandler(MyChart_MouseDown);
            this.MouseUp += new MouseEventHandler(MyChart_MouseUp);
            this.Resize += new EventHandler(MyChart_Resized);
        }

        void MyChart_Resized(object sender, EventArgs e)
        {
            is_resized = true;
        }

        void MyChart_PostPaint(object sender, ChartPaintEventArgs e)
        {
            /* Костыль, нужный чтобы функция рисовки не вызывалась количество раз, равное количеству элементов на графике */
            if (e.ChartElement != this)
                return;

            /* При входе мыши на контрол или при команде о пересчете - пересчитываем главные области */
            if (is_enter || recalc_chart_area || is_resized)
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
                text_size = TextRenderer.MeasureText(find_longest_name(), this.Legends[0].Font);
                text_size.Height = 14;
                options_area = new RectangleF((float)(this.Width - (enable_add_point ? 80 : 57)),
                    (float)(this.Height * 0.85),
                    enable_add_point ? 46 : 23, 20);
                zoom_options_area = new RectangleF((float)(options_area.X + 1), (float)(options_area.Y - 46), 20, 46);
                series_selection_area = new RectangleF(chart_area.X + chart_area.Width +
                                                       (this.Width - chart_area.X - chart_area.Width) * 0.5f -
                                                       (text_size.Width + 30) * 0.5f,
                                                       this.Legends[0].Position.Y - (this.Legends[0].Position.Y - chart_area.Y) * 0.5f + 3,
                                                       text_size.Width + 45,
                                                       text_size.Height * this.Series.Count);

                is_enter = false;
                recalc_chart_area = false;
                is_resized = false;
            }

            /* Выделяем серию в легенде, на которую указывает курсор */
            if (item_to_highlight >= 0 && is_able_to_choose)
            {
                SizeF size = TextRenderer.MeasureText(this.Series[item_to_highlight].Name, this.Legends[0].Font);
                size.Height = 14;
                LinearGradientBrush myVerticalGradient =
                    new LinearGradientBrush(new PointF(series_selection_area.X + series_selection_area.Width, series_selection_area.Y),
                    new PointF(size.Width + 45 >= series_selection_area.Width ? series_selection_area.X + size.Width : series_selection_area.X + size.Width + 45,
                    series_selection_area.Y), Color.LightGray, this.Legends[0].BackColor);
                e.ChartGraphics.Graphics.FillRectangle(myVerticalGradient,
                    series_selection_area.X + size.Width + 46,
                    series_selection_area.Y + size.Height * item_to_highlight,
                    series_selection_area.Width - size.Width - 45,
                    size.Height);
                e.ChartGraphics.Graphics.DrawRectangle(Pens.LightGray,
                    series_selection_area.X,
                    series_selection_area.Y + size.Height * item_to_highlight,
                    series_selection_area.Width,
                    size.Height - 1);
            }

            /* Подсвечиваем выбранную серию в легенде */
            if (selected_series >= 0 && is_able_to_choose)
            {
                Rectangle choosen_series = Rectangle.Round(new RectangleF(series_selection_area.X,
                    series_selection_area.Y + selected_series * text_size.Height, series_selection_area.Width, text_size.Height));
                GraphicsPath choosen_path = RoundedRectangle.Create(choosen_series, 5);
                e.ChartGraphics.Graphics.FillPath(new SolidBrush(Color.FromArgb(25, Color.Red)), choosen_path);
            }

            /* Когда выбрана опция зума - подрисовываем дополнительные кнопки, относящиеся к этой опции */
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

            /* Рисуем область опций и кнопку зума */
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

            /* Рисуем кнопку добавления точек только если это предусмотрено настройкой */
            if (enable_add_point)
            {
                Rectangle option_addpoint = new Rectangle(new Point(option_zoom.X + 23, option_zoom.Y),
                    new Size(18, 18));
                path = RoundedRectangle.Create(option_addpoint, 3);
                e.ChartGraphics.Graphics.FillPath(highlightbutton == 1 || this.Cursor == Cursors.Cross ? Brushes.LightBlue : Brushes.WhiteSmoke, path);
                e.ChartGraphics.Graphics.DrawImage(Properties.Resources.Add_point,
                    new Rectangle(option_zoom.X + 23, option_zoom.Y + 1, 18, 18),
                    new Rectangle(0, 0, Properties.Resources.Add_point.Width, Properties.Resources.Add_point.Height),
                    GraphicsUnit.Pixel);
            }

            /* Рисуем направляющие если курсор в области графика */
            if (is_in_chart_area && !is_left)
            {
                float[] dashValues = { 5, 5 };
                Pen pen = new Pen(Color.Black, 0.5f);
                pen.DashPattern = dashValues;

                e.ChartGraphics.Graphics.DrawLine(pen, chart_area.X, highligt_y, chart_area.X + chart_area.Width, highligt_y);
                e.ChartGraphics.Graphics.DrawLine(pen, highligt_x, chart_area.Y, highligt_x, chart_area.Y + chart_area.Height);

                e.ChartGraphics.Graphics.DrawString(Math.Round(this.ChartAreas[0].AxisX.PixelPositionToValue(highligt_x),
                    MathDecimals.GetDecimalPlaces((decimal)(float)this.ChartAreas[0].AxisX.Maximum) + 2).ToString(),
                    this.Font, Brushes.Black, highligt_x, chart_area.Y - 20);
                e.ChartGraphics.Graphics.DrawString(Math.Round(this.ChartAreas[0].AxisY.PixelPositionToValue(highligt_y),
                    MathDecimals.GetDecimalPlaces((decimal)(float)this.ChartAreas[0].AxisX.Maximum) + 2).ToString(),
                    this.Font, Brushes.Black, chart_area.X + chart_area.Width + 5, highligt_y);
            }

            /* Отрисовываем область приближения для наглядности, если пользователь выбрал эту функцию */
            if (is_in_chart_area && this.Cursor == Cursors.NoMove2D && is_L_mouse_down)
            {
                e.ChartGraphics.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(25, Color.Red)), zoom_area);
            }
        }

        void MyChart_MouseMove(object sender, MouseEventArgs e)
        {
            /* Зашел ли курсор в область легенды */
            if (e.X > series_selection_area.X && e.X < series_selection_area.X + series_selection_area.Width &&
                e.Y > series_selection_area.Y && e.Y < series_selection_area.Y + series_selection_area.Height && is_able_to_choose)
            {
                item_to_highlight = (int)((float)(e.Y - series_selection_area.Y) / text_size.Height);
                Invalidate(new Rectangle((int)series_selection_area.X - 2,
                        (int)series_selection_area.Y - 2,
                        (int)series_selection_area.X + (int)series_selection_area.Width + 2,
                        (int)series_selection_area.Y + (int)series_selection_area.Height + 2));
            }
            else
            {
                if (item_to_highlight != -1)
                {
                    item_to_highlight = -1;
                    Invalidate(new Rectangle((int)series_selection_area.X - 2,
                        (int)series_selection_area.Y - 2,
                        (int)series_selection_area.X + (int)series_selection_area.Width + 2,
                        (int)series_selection_area.Y + (int)series_selection_area.Height + 2));
                }
            }

            /* Зашел ли курсор в область хитбокса опций, выбираем какую кнопку подсвечивать */
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

            /* Зашел ли курсор в область график. нужно для отрисовки направляющих */
            if (e.X > chart_area.X && e.X < chart_area.X + chart_area.Width &&
                e.Y > chart_area.Y && e.Y < chart_area.Y + chart_area.Height)
            {
                is_in_chart_area = true;
                highligt_x = e.X;
                highligt_y = e.Y;

                Invalidate();
            }  
            else is_in_chart_area = false;

            /* Если пользуемся функцией приближения, то пересчитываем отризовку области зума(для наглядности) */
            if (this.Cursor == Cursors.NoMove2D)
            {
                zoom_area = new RectangleF(tozoom_x > e.X ? e.X : tozoom_x,
                    tozoom_y > e.Y ? e.Y : tozoom_y, Math.Abs(e.X - tozoom_x), Math.Abs(e.Y - tozoom_y));
            }

            /* Если пользуемся функцией передвигания графика, то пересчитаваем границы осей */
            if (is_in_chart_area && this.Cursor == Cursors.Hand)
            {
                double v1 = Math.Round(Math.Abs(this.ChartAreas[0].AxisX.PixelPositionToValue(e.X) -
                    this.ChartAreas[0].AxisX.PixelPositionToValue(toshift_x)),
                    MathDecimals.GetDecimalPlaces((decimal)(float)this.ChartAreas[0].AxisX.Maximum) + 2);
                this.ChartAreas[0].AxisX.Minimum += toshift_x > e.X ? v1 : -v1;
                this.ChartAreas[0].AxisX.Maximum += toshift_x > e.X ? v1 : -v1;

                double v2 = Math.Round(Math.Abs(this.ChartAreas[0].AxisY.PixelPositionToValue(e.Y) -
                    this.ChartAreas[0].AxisY.PixelPositionToValue(toshift_y)),
                    MathDecimals.GetDecimalPlaces((decimal)(float)this.ChartAreas[0].AxisY.Maximum) + 2);

                this.ChartAreas[0].AxisY.Minimum += toshift_y > e.Y ? -v2 : v2;
                this.ChartAreas[0].AxisY.Maximum += toshift_y > e.Y ? -v2 : v2;

                toshift_x = e.X;
                toshift_y = e.Y;

                this.ChartAreas[0].AxisX.Interval = (this.ChartAreas[0].AxisX.Maximum - this.ChartAreas[0].AxisX.Minimum) / 10;
                //this.ChartAreas[0].AxisY.Interval = 0.1;

                recalc_chart_area = true;
            }
        }

        void MyChart_MouseClick(object sender, MouseEventArgs e)
        {
            /* Какую серию выбирает пользователь при нажатии на легенду */
            if (item_to_highlight >= 0 && is_able_to_choose)
                selected_series = item_to_highlight;

            /* Был ли клик правой кнопкой мыши по подсвечиваемой серии в легенде чтобы вызвать контекствное меню */
            if (e.X > series_selection_area.X && e.X < series_selection_area.X + series_selection_area.Width &&
                e.Y > series_selection_area.Y + item_to_highlight * text_size.Height && 
                e.Y < series_selection_area.Y + item_to_highlight * text_size.Height + text_size.Height &&
                e.Button == MouseButtons.Right)
            {
                ChartSeriesContextMenuStrip.Show(this, e.X, e.Y);
            }

            /* Было ли нажатие вне легенды, вне области опций и вне области графика чтобы снять выделение */
            if (!((e.X > series_selection_area.X && e.X < series_selection_area.X + series_selection_area.Width &&
                e.Y > series_selection_area.Y && e.Y < series_selection_area.Y + series_selection_area.Height) ||
                (e.X > options_area.X + 2 && e.X < options_area.X + 20 &&
                e.Y > options_area.Y + 1 && e.Y < options_area.Y + 19) ||
                (e.X > options_area.X + 2 + 23 && e.X < options_area.X + 20 + 23 &&
                e.Y > options_area.Y + 1 && e.Y < options_area.Y + 19) ||
                (e.X > zoom_options_area.X + 1 && e.X < zoom_options_area.X + 19 &&
                e.Y > zoom_options_area.Y + 2 && e.Y < zoom_options_area.Y + 20) ||
                (e.X > zoom_options_area.X + 1 && e.X < zoom_options_area.X + 19 &&
                e.Y > zoom_options_area.Y + 23 && e.Y < zoom_options_area.Y + 41) ||
                is_in_chart_area) && 
                e.Button == MouseButtons.Left && selected_series != -1 && is_able_to_choose)
            {
                selected_series = -1;
                Invalidate(Rectangle.Ceiling(series_selection_area));
            }

            /* Было ли нажатие на функцию приближения */
            if (e.X > options_area.X + 2 && e.X < options_area.X + 20 &&
                e.Y > options_area.Y + 1 && e.Y < options_area.Y + 19 && e.Button == MouseButtons.Left)
            {
                if (this.Cursor != Cursors.NoMove2D)
                {
                    this.Cursor = Cursors.NoMove2D;

                    zoom_history.Clear();
                    default_chart_coord = new chart_coord((double)this.ChartAreas[0].AxisX.Minimum,
                                                (double)this.ChartAreas[0].AxisX.Maximum,
                                                (double)this.ChartAreas[0].AxisY.Minimum,
                                                (double)this.ChartAreas[0].AxisY.Maximum);
                }  
                else
                    this.Cursor = Cursors.Default;
            }

            /* Было ли нажатие на функцию приближение на шаг назад */
            if (e.X > zoom_options_area.X + 1 && e.X < zoom_options_area.X + 19 &&
                e.Y > zoom_options_area.Y + 2 && e.Y < zoom_options_area.Y + 20 && e.Button == MouseButtons.Left)
            {
                if (this.Cursor == Cursors.NoMove2D && zoom_history.Count > 0)
                {
                    this.ChartAreas[0].AxisX.Minimum = zoom_history.Last().x_min;
                    this.ChartAreas[0].AxisX.Maximum = zoom_history.Last().x_max;
                    this.ChartAreas[0].AxisY.Minimum = zoom_history.Last().y_min;
                    this.ChartAreas[0].AxisY.Maximum = zoom_history.Last().y_max;

                    this.ChartAreas[0].AxisX.Interval = (this.ChartAreas[0].AxisX.Maximum - this.ChartAreas[0].AxisX.Minimum) / 10;
                    //this.ChartAreas[0].AxisY.Interval = 0.1;

                    zoom_history.RemoveAt(zoom_history.Count - 1);

                    if (zoom_history.Count <= 0)
                    {
                        this.ChartAreas[0].AxisY.Minimum = Double.NaN;
                        this.ChartAreas[0].AxisY.Maximum = Double.NaN;
                    }

                    recalc_chart_area = true;
                }
            }

            /* Было ли нажатие на функцию вернуть график к начальной области */
            if (e.X > zoom_options_area.X + 1 && e.X < zoom_options_area.X + 19 &&
                e.Y > zoom_options_area.Y + 23 && e.Y < zoom_options_area.Y + 41 && e.Button == MouseButtons.Left)
            {
                if (this.Cursor == Cursors.NoMove2D)
                {
                    this.ChartAreas[0].AxisX.Minimum = default_chart_coord.x_min;
                    this.ChartAreas[0].AxisX.Maximum = default_chart_coord.x_max;
                    this.ChartAreas[0].AxisY.Minimum = Double.NaN;
                    this.ChartAreas[0].AxisY.Maximum = Double.NaN;

                    this.ChartAreas[0].AxisX.Interval = (this.ChartAreas[0].AxisX.Maximum - this.ChartAreas[0].AxisX.Minimum) / 10;
                    //this.ChartAreas[0].AxisY.Interval = 0.1;

                    zoom_history.Clear();

                    recalc_chart_area = true;
                }
            }

            /* TODO: Было ли нажатие на функцию добавить точки */
            if (e.X > options_area.X + 2 + 23 && e.X < options_area.X + 20 + 23 &&
                e.Y > options_area.Y + 1 && e.Y < options_area.Y + 19 && e.Button == MouseButtons.Left)
            {
                if (this.Cursor != Cursors.Cross)
                    this.Cursor = Cursors.Cross;
                else
                    this.Cursor = Cursors.Default;
            }

            /* Было ли нажатие где-либо вне хитбокса опций, чтобы отказаться от выбора инструмента */
            if (!((e.X > options_area.X + 2 && e.X < options_area.X + 20 &&
                e.Y > options_area.Y + 1 && e.Y < options_area.Y + 19) ||
                (e.X > options_area.X + 2 + 23 && e.X < options_area.X + 20 + 23 &&
                e.Y > options_area.Y + 1 && e.Y < options_area.Y + 19) ||
                (e.X > zoom_options_area.X + 1 && e.X < zoom_options_area.X + 19 &&
                e.Y > zoom_options_area.Y + 2 && e.Y < zoom_options_area.Y + 20) ||
                (e.X > zoom_options_area.X + 1 && e.X < zoom_options_area.X + 19 &&
                e.Y > zoom_options_area.Y + 23 && e.Y < zoom_options_area.Y + 41) ||
                (e.X > series_selection_area.X && e.X < series_selection_area.X + series_selection_area.Width &&
                e.Y > series_selection_area.Y && e.Y < series_selection_area.Y + series_selection_area.Height && is_able_to_choose))
                && !is_in_chart_area && e.Button == MouseButtons.Left)
            {
                if (this.Cursor == Cursors.NoMove2D)
                {
                    this.ChartAreas[0].AxisX.Minimum = default_chart_coord.x_min;
                    this.ChartAreas[0].AxisX.Maximum = default_chart_coord.x_max;
                    this.ChartAreas[0].AxisY.Minimum = Double.NaN;
                    this.ChartAreas[0].AxisY.Maximum = Double.NaN;

                    this.ChartAreas[0].AxisX.Interval = (this.ChartAreas[0].AxisX.Maximum - this.ChartAreas[0].AxisX.Minimum) / 10;
                    //this.ChartAreas[0].AxisY.Interval = 0.1;

                    zoom_history.Clear();

                    recalc_chart_area = true;
                }

                this.Cursor = Cursors.Default;
            } 
        }

        void MyChart_MouseDown(object sender, MouseEventArgs e)
        {
            if (is_in_chart_area && this.Cursor == Cursors.NoMove2D && e.Button == MouseButtons.Left)
            {
                tozoom_x = e.X;
                tozoom_y = e.Y;
                is_L_mouse_down = true;
            }

            if (is_in_chart_area && this.Cursor == Cursors.NoMove2D && e.Button == MouseButtons.Right)
            {
                this.Cursor = Cursors.Hand;
                toshift_x = e.X;
                toshift_y = e.Y;
                is_R_mouse_down = true;
            }
        }

        void MyChart_MouseUp(object sender, MouseEventArgs e)
        {
            if (is_in_chart_area && this.Cursor == Cursors.NoMove2D)
            {
                zoom_history.Add(new chart_coord((double)this.ChartAreas[0].AxisX.Minimum,
                                                (double)this.ChartAreas[0].AxisX.Maximum,
                                                (double)this.ChartAreas[0].AxisY.Minimum,
                                                (double)this.ChartAreas[0].AxisY.Maximum));

                double v1 = Math.Round(this.ChartAreas[0].AxisX.PixelPositionToValue(zoom_area.X),
                    MathDecimals.GetDecimalPlaces((decimal)(float)this.ChartAreas[0].AxisX.Maximum) + 2);
                double v2 = Math.Round(this.ChartAreas[0].AxisX.PixelPositionToValue(zoom_area.X + zoom_area.Width),
                    MathDecimals.GetDecimalPlaces((decimal)(float)this.ChartAreas[0].AxisX.Maximum) + 2);
                this.ChartAreas[0].AxisX.Minimum = v1;
                this.ChartAreas[0].AxisX.Maximum = v2;

                double v3 = Math.Round(this.ChartAreas[0].AxisY.PixelPositionToValue(zoom_area.Y + zoom_area.Height),
                    MathDecimals.GetDecimalPlaces((decimal)(float)this.ChartAreas[0].AxisX.Maximum) + 2);
                double v4 = Math.Round(this.ChartAreas[0].AxisY.PixelPositionToValue(zoom_area.Y),
                    MathDecimals.GetDecimalPlaces((decimal)(float)this.ChartAreas[0].AxisX.Maximum) + 2);
                this.ChartAreas[0].AxisY.Minimum = v3;
                this.ChartAreas[0].AxisY.Maximum = v4;

                this.ChartAreas[0].AxisX.Interval = (this.ChartAreas[0].AxisX.Maximum - this.ChartAreas[0].AxisX.Minimum) / 10;
                //this.ChartAreas[0].AxisY.Interval = 0.1;

                recalc_chart_area = true;
            }

            if (e.Button == MouseButtons.Left)
                is_L_mouse_down = false;
            if (e.Button == MouseButtons.Right && this.Cursor == Cursors.Hand)
            {
                this.Cursor = Cursors.NoMove2D;
                is_R_mouse_down = false;
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
            item_to_highlight = -1;
            Invalidate();
        }

        string find_longest_name()
        {
            SizeF size = new SizeF();
            string str = "0";
            foreach (var s in Series)
            {
                if (size.Width < TextRenderer.MeasureText(s.Name, this.Legends[0].Font).Width)
                {
                    str = s.Name;
                    size = TextRenderer.MeasureText(s.Name, this.Legends[0].Font);
                }
            }

            return str;
        }

        void highlight_series(int index)
        {
            if (index >= 0)
                for (int i = 0; i < this.Series.Count; ++i)
                {
                    if (i == index)
                        this.Series[i].Color = Color.FromArgb(255, this.Series[i].Color);
                    else
                        this.Series[i].Color = Color.FromArgb(25, this.Series[i].Color);
                }
            else
                for (int i = 0; i < this.Series.Count; ++i)
                    this.Series[i].Color = Color.FromArgb(255, this.Series[i].Color);

            Invalidate(Rectangle.Ceiling(chart_area));
        }

        public void SetSelectedSeries(int index)
        {
            if (index >= this.Series.Count)
                return;

            is_selection_set_outside = true;
            selected_series = index;
            is_selection_set_outside = false;
        }

        void MyChart_SeriesColorChoose(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.ShowDialog(this);
            this.Series[selected_series].Color = colorDialog.Color;
            this.settings[FindSettingIndexByName(Series[selected_series].Name)].color = colorDialog.Color;
            if (ColorChanged != null)
                ColorChanged(this, new CustomEventArgs(selected_series));
        }

        void MyChart_SeriesWidthChoose(object sender, EventArgs e)
        {
            ValueSelectionDialog d = new ValueSelectionDialog((int)series_selection_area.X - 100,
                (int)series_selection_area.Y + (int)(selected_series * text_size.Height),
                this);
        }

        #region series_settings

        public int FindSettingIndexByName(string name)
        {
            int index = 0;
            foreach (var set in settings)
            {
                if (set.name == name)
                    return index;
                index++;
            }

            AddSettings(name, Color.FromArgb(new Random(index * index).Next(40, 210),
                new Random(index * index * index * index).Next(60, 245),
                new Random(index * index * index).Next(50, 220)));

            return index;
        }

        public void AddSettings(string name, Color color, uint width = 3)
        {
            for (int i = 0; i < settings.Length; ++i)
                if (settings[i].name == name)
                {
                    settings[i].color = color;
                    settings[i].width = width;
                    return;
                }

            series_setting[] temp = settings;
            settings = new series_setting[temp.Length + 1];
            for (int i = 0; i < temp.Count(); ++i)
                    settings[i] = temp[i];
            settings[settings.Length - 1] = new series_setting() { name = name, color = color, width = width };
        }

        public void DeleteSettings(string name)
        {
            series_setting[] temp = settings;
            settings = new series_setting[temp.Length - 1];
            for (int i = 0; i < temp.Count(); ++i)
                if (temp[i].name != name)
                    settings[i] = temp[i];
        }

        #endregion

        class ValueSelectionDialog : Control
        {
            MyChart _parent;
            NumericUpDown input_box = new NumericUpDown() { Left = 0, Top = 0, Width = 100 };
            Button ok_button = new Button() { Text = "Ok", Left = 0, Top = 20, Width = 50 };
            Button cancel_button = new Button() { Text = "Cancel", Left = 50, Top = 20, Width = 50 };

            public ValueSelectionDialog(int X, int Y, MyChart parent)
            {
                parent.Controls.Add(this);
                _parent = parent;

                this.Width = 100;
                this.Height = 42;
                this.Location = new Point(X, Y);

                this.Controls.AddRange(new Control[] { input_box, ok_button, cancel_button });

                ok_button.Click += new EventHandler(ValueSelectionDialog_Ok);
                cancel_button.Click += new EventHandler(ValueSelectionDialog_Cancel);
                parent.SelectedSeriesChanged += new EventHandler(ValueSelectionDialog_Close);

                input_box.DecimalPlaces = 0;
                input_box.Value = parent.Series[parent.selected_series].BorderWidth;
            }

            void ValueSelectionDialog_Ok(object sender, EventArgs e)
            {
                _parent.Series[_parent.selected_series].BorderWidth = (int)input_box.Value;
                _parent.settings[_parent.FindSettingIndexByName(_parent.Series[_parent.selected_series].Name)].width = (uint)input_box.Value;
                this.Dispose();
            }

            void ValueSelectionDialog_Cancel(object sender, EventArgs e)
            {
                this.Dispose();
            }

            void ValueSelectionDialog_Close(object sender, EventArgs e)
            {
                this.Dispose();
            }
        }
    }
}
