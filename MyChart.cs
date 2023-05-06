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
using System.Runtime.InteropServices;

namespace ImpulseMaker
{
    internal class MyChart : Chart
    {
        public event EventHandler SelectedSeriesChanged, HighligtedPointChanged, X_margin_needed;
        public delegate void CustomEventHandler(object sender, CustomEventArgs eventArgs);
        public event CustomEventHandler ColorChanged;
        public delegate void PointEventHandler(DataPoint point, int index);
        public event PointEventHandler PointChanged, PointAdded, PointDeleted;
        public event EventHandler FinishedDrawing;

        int highlightbutton = -1,
            item_to_highlight = -1,
            s_s = -1,
            point_inner_size = 4,
            p_to_h = -1,
            potentially_new_point_index = -1,
            point_to_delete = -1;
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
            e_a_p = true,
            is_a_t_ch = true,
            is_selection_set_outside = false,
            is_resized = false,
            move_highlighted_point = false;
        float highligt_x,
            highligt_y,
            tozoom_x,
            tozoom_y,
            toshift_x,
            toshift_y;
        double p_v_X_m = 0.01,
            p_v_Y_m = 0.01;
        List<chart_coord> zoom_history = new List<chart_coord>();
        public series_setting[] settings = new series_setting[0];
        public bool recalc_chart_area = false;
        chart_coord default_chart_coord;
        SizeF text_size = new SizeF();
        ContextMenuStrip ChartSeriesContextMenuStrip = new ContextMenuStrip(),
            PointPositionContextMenuStrip = new ContextMenuStrip();

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

        int point_to_highlight
        {
            get { return p_to_h; }
            set
            {
                if (p_to_h != value)
                {
                    p_to_h = value;
                    Invalidate();
                    if (HighligtedPointChanged != null)
                        HighligtedPointChanged(this, null);
                }
            }
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
            set { e_a_p = value; recalc_chart_area = true; Invalidate(); }
        }

        public bool is_able_to_choose
        {
            get { return is_a_t_ch; }
            set { is_a_t_ch = value; recalc_chart_area = true; Invalidate(); }
        }

        public double point_value_X_margin
        {
            get { return p_v_X_m; }
            set { p_v_X_m = value; Invalidate(); }
        }

        public double point_value_Y_margin
        {
            get { return p_v_Y_m; }
            set { p_v_Y_m = value; Invalidate(); }
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

            ToolStripMenuItem Position = new ToolStripMenuItem("Установить значения");

            PointPositionContextMenuStrip.Items.Add(Position);

            Position.Click += new EventHandler(MyChart_PointPositionChoose);

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
           
            /* Если на графике есть одна серия и ее нельзя выбрать - ставим ее выбранной, чтобы можно было перетаскивать узлы */
            if (!is_able_to_choose && this.Series.Count == 1) selected_series = 0;

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
                    //(float)(this.Height * 0.85),
                    chart_area.Y + chart_area.Height,
                    enable_add_point ? 46 : 23, 20);
                zoom_options_area = new RectangleF((float)(options_area.X + 1), (float)(options_area.Y - 46), 20, 46);
                series_selection_area = new RectangleF(chart_area.X + chart_area.Width +
                                                       (this.Width - chart_area.X - chart_area.Width) * 0.5f -
                                                       (text_size.Width + 30) * 0.5f,
                                                       //this.Legends[0].Position.Y - (this.Legends[0].Position.Y - chart_area.Y) * 0.5f + 3,
                                                       chart_area.Y - 6,
                                                       text_size.Width + 45,
                                                       text_size.Height * this.Series.Count);
                if (this.Cursor == Cursors.Default)
                    default_chart_coord = new chart_coord((double)this.ChartAreas[0].AxisX.Minimum,
                                                    (double)this.ChartAreas[0].AxisX.Maximum,
                                                    (double)this.ChartAreas[0].AxisY.Minimum,
                                                    (double)this.ChartAreas[0].AxisY.Maximum);

                point_value_X_margin = (default_chart_coord.x_max - default_chart_coord.x_min) / 1000;
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

            /* Рисуем помогающий узел для добавления узлов на график */
            if (is_in_chart_area && this.Cursor == Cursors.Cross && potentially_new_point_index >= 0 && point_to_highlight < 0)
            {
                float[] dashValues = { 5, 5 };
                Pen pen = new Pen(Series[selected_series].Color, Series[selected_series].BorderWidth);
                pen.DashPattern = dashValues;

                e.ChartGraphics.Graphics.DrawLine(pen,
                    (float)ChartAreas[0].AxisX.ValueToPixelPosition(Series[selected_series].Points[potentially_new_point_index - 1].XValue),
                    (float)ChartAreas[0].AxisY.ValueToPixelPosition(Series[selected_series].Points[potentially_new_point_index - 1].YValues[0]),
                    highligt_x, highligt_y);
                e.ChartGraphics.Graphics.DrawLine(pen, highligt_x, highligt_y,
                    (float)ChartAreas[0].AxisX.ValueToPixelPosition(Series[selected_series].Points[potentially_new_point_index].XValue),
                    (float)ChartAreas[0].AxisY.ValueToPixelPosition(Series[selected_series].Points[potentially_new_point_index].YValues[0]));

                e.ChartGraphics.Graphics.FillEllipse(new SolidBrush(Series[selected_series].Color),
                            new RectangleF(highligt_x - point_inner_size,
                            highligt_y - point_inner_size,
                            point_inner_size * 2,
                            point_inner_size * 2));
            }

            /* Рисуем направляющие если курсор в области графика */
            if (is_in_chart_area && !is_left)
            {
                float[] dashValues = { 5, 5 };
                Pen pen = new Pen(Color.Black, 0.5f);
                pen.DashPattern = dashValues;

                e.ChartGraphics.Graphics.DrawLine(pen, chart_area.X, highligt_y, chart_area.X + chart_area.Width, highligt_y);
                e.ChartGraphics.Graphics.DrawLine(pen, highligt_x, chart_area.Y, highligt_x, chart_area.Y + chart_area.Height);

                /* Направляющую по Х округляем до шага частоты дискретизации */
                e.ChartGraphics.Graphics.DrawString(RoundWithXMargin(this.ChartAreas[0].AxisX.PixelPositionToValue(highligt_x)).ToString(),
                    this.Font, Brushes.Black, highligt_x, chart_area.Y - 20);
                e.ChartGraphics.Graphics.DrawString(RoundWithYMargin(this.ChartAreas[0].AxisY.PixelPositionToValue(highligt_y)).ToString(),
                    this.Font, Brushes.Black, chart_area.X + chart_area.Width + 5, highligt_y);
            }

            /* Отрисовываем область приближения для наглядности, если пользователь выбрал эту функцию */
            if (is_in_chart_area && this.Cursor == Cursors.NoMove2D && is_L_mouse_down)
            {
                e.ChartGraphics.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(25, Color.Red)), zoom_area);
            }

            /* Выделяем узлы выбранной серии на графике если есть настройка добавлять точки */
            if (enable_add_point && selected_series >= 0)
            {
                for (int j = 0; j < Series[selected_series].Points.Count; ++j)
                {
                    e.ChartGraphics.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(j == point_to_highlight ? 255 : 75, Series[selected_series].Color)),
                        new RectangleF((float)ChartAreas[0].AxisX.ValueToPixelPosition(Series[selected_series].Points[j].XValue) - point_inner_size * 2,
                        (float)ChartAreas[0].AxisY.ValueToPixelPosition(Series[selected_series].Points[j].YValues[0]) - point_inner_size * 2,
                        point_inner_size * 4,
                        point_inner_size * 4));
                    e.ChartGraphics.Graphics.FillEllipse(new SolidBrush(Series[selected_series].Color),
                        new RectangleF((float)ChartAreas[0].AxisX.ValueToPixelPosition(Series[selected_series].Points[j].XValue) - point_inner_size,
                        (float)ChartAreas[0].AxisY.ValueToPixelPosition(Series[selected_series].Points[j].YValues[0]) - point_inner_size,
                        point_inner_size * 2,
                        point_inner_size * 2));
                    e.ChartGraphics.Graphics.DrawString((j + 1).ToString(),
                    this.Font, Brushes.Black,
                    (float)ChartAreas[0].AxisX.ValueToPixelPosition(Series[selected_series].Points[j].XValue),
                    (float)ChartAreas[0].AxisY.ValueToPixelPosition(Series[selected_series].Points[j].YValues[0]) - point_inner_size * 5);
                }
            }

            /* Рисуем указатель об удалении узла */
            if (this.Cursor == Cursors.Cross && point_to_highlight > 0 && point_to_highlight < Series[selected_series].Points.Count - 1)
            {
                e.ChartGraphics.Graphics.DrawLine(new Pen(Color.Red, 5),
                    (float)ChartAreas[0].AxisX.ValueToPixelPosition(Series[selected_series].Points[point_to_highlight].XValue) - point_inner_size * 3,
                    (float)ChartAreas[0].AxisY.ValueToPixelPosition(Series[selected_series].Points[point_to_highlight].YValues[0]) - point_inner_size * 3,
                    (float)ChartAreas[0].AxisX.ValueToPixelPosition(Series[selected_series].Points[point_to_highlight].XValue) + point_inner_size * 3,
                    (float)ChartAreas[0].AxisY.ValueToPixelPosition(Series[selected_series].Points[point_to_highlight].YValues[0]) + point_inner_size * 3);
                e.ChartGraphics.Graphics.DrawLine(new Pen(Color.Red, 5),
                    (float)ChartAreas[0].AxisX.ValueToPixelPosition(Series[selected_series].Points[point_to_highlight].XValue) - point_inner_size * 3,
                    (float)ChartAreas[0].AxisY.ValueToPixelPosition(Series[selected_series].Points[point_to_highlight].YValues[0]) + point_inner_size * 3,
                    (float)ChartAreas[0].AxisX.ValueToPixelPosition(Series[selected_series].Points[point_to_highlight].XValue) + point_inner_size * 3,
                    (float)ChartAreas[0].AxisY.ValueToPixelPosition(Series[selected_series].Points[point_to_highlight].YValues[0]) - point_inner_size * 3);

                point_to_delete = point_to_highlight;
            }
            else point_to_delete = -1;

            /* Эвент об окончании отрисовки, нужен для экономии ресурсов прцессора
             * (нужно поставить условие не рисовать заново пока не произойдет этот эвент) */
            if (FinishedDrawing != null)
                FinishedDrawing(this, null);
        }

        void MyChart_MouseMove(object sender, MouseEventArgs e)
        {
            /* Зашел ли курсор в область легенды */
            if (series_selection_area.Contains(e.Location) && is_able_to_choose)
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

            /* Зашел ли курсор в область графика. нужно для отрисовки направляющих */
            if (chart_area.Contains(e.Location))
            {
                if (!is_in_chart_area)
                {
                    /* Запрашиваем извне шаг, с которым можно изменять значения по Х */
                    if (X_margin_needed != null)
                        X_margin_needed(this, null);
                    is_in_chart_area = true;
                }
                highligt_x = e.X;
                highligt_y = e.Y;

                Invalidate();
            }  
            else is_in_chart_area = false;

            /* Если пользуемся функцией приближения, то пересчитываем отрисовку области зума(для наглядности) */
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
                    MathDecimals.GetDecimalPlaces(this.ChartAreas[0].AxisX.Maximum) + 2);
                this.ChartAreas[0].AxisX.Minimum += toshift_x > e.X ? v1 : -v1;
                this.ChartAreas[0].AxisX.Maximum += toshift_x > e.X ? v1 : -v1;

                double v2 = Math.Round(Math.Abs(this.ChartAreas[0].AxisY.PixelPositionToValue(e.Y) -
                    this.ChartAreas[0].AxisY.PixelPositionToValue(toshift_y)),
                    MathDecimals.GetDecimalPlaces(this.ChartAreas[0].AxisY.Maximum) + 2);

                this.ChartAreas[0].AxisY.Minimum += toshift_y > e.Y ? -v2 : v2;
                this.ChartAreas[0].AxisY.Maximum += toshift_y > e.Y ? -v2 : v2;

                toshift_x = e.X;
                toshift_y = e.Y;

                this.ChartAreas[0].AxisX.Interval = (this.ChartAreas[0].AxisX.Maximum - this.ChartAreas[0].AxisX.Minimum) / 10;
                //this.ChartAreas[0].AxisY.Interval = 0.1;

                recalc_chart_area = true;
            }

            /* Двигаем выбранный пользователем узел */
            if (move_highlighted_point && this.ClientRectangle.Contains(e.Location) && point_to_delete < 0)
            {
                /* Если крайние левый или правый узлы (изменение Y) */
                if ((point_to_highlight == 0 || point_to_highlight == Series[selected_series].Points.Count - 1) &&
                    this.ChartAreas[0].AxisY.PixelPositionToValue(e.Y) > default_chart_coord.y_min &&
                    this.ChartAreas[0].AxisY.PixelPositionToValue(e.Y) < default_chart_coord.y_max)
                {
                    Series[selected_series].Points[point_to_highlight] = new DataPoint(
                        Series[selected_series].Points[point_to_highlight].XValue,
                        RoundWithYMargin(this.ChartAreas[0].AxisY.PixelPositionToValue(e.Y)));
                }

                /* Любой узел кроме крайних (изменение X) */
                if (point_to_highlight != 0 && point_to_highlight != Series[selected_series].Points.Count - 1 &&
                    this.ChartAreas[0].AxisX.PixelPositionToValue(e.X) > Series[selected_series].Points[point_to_highlight - 1].XValue &&
                    this.ChartAreas[0].AxisX.PixelPositionToValue(e.X) < Series[selected_series].Points[point_to_highlight + 1].XValue &&
                    this.ChartAreas[0].AxisY.PixelPositionToValue(e.Y) > default_chart_coord.y_min &&
                    this.ChartAreas[0].AxisY.PixelPositionToValue(e.Y) < default_chart_coord.y_max)
                {
                    Series[selected_series].Points[point_to_highlight] = new DataPoint(
                        RoundWithXMargin(this.ChartAreas[0].AxisX.PixelPositionToValue(e.X)),
                        RoundWithYMargin(this.ChartAreas[0].AxisY.PixelPositionToValue(e.Y)));
                }
                /* Если уперлись в крайние положения по X (изменение Y) */
                else if (point_to_highlight != 0 && point_to_highlight != Series[selected_series].Points.Count - 1 && 
                    this.ChartAreas[0].AxisY.PixelPositionToValue(e.Y) > default_chart_coord.y_min &&
                    this.ChartAreas[0].AxisY.PixelPositionToValue(e.Y) < default_chart_coord.y_max)
                {
                    Series[selected_series].Points[point_to_highlight] = new DataPoint(
                        Series[selected_series].Points[point_to_highlight].XValue,
                        RoundWithYMargin(this.ChartAreas[0].AxisY.PixelPositionToValue(e.Y)));
                }
                /* Если уперлись в крайние положения по Y (изменение X) */
                else if (point_to_highlight != 0 && point_to_highlight != Series[selected_series].Points.Count - 1 && 
                    this.ChartAreas[0].AxisX.PixelPositionToValue(e.X) > Series[selected_series].Points[point_to_highlight - 1].XValue &&
                    this.ChartAreas[0].AxisX.PixelPositionToValue(e.X) < Series[selected_series].Points[point_to_highlight + 1].XValue)
                {
                    Series[selected_series].Points[point_to_highlight] = new DataPoint(
                        RoundWithXMargin(this.ChartAreas[0].AxisX.PixelPositionToValue(e.X)),
                        Series[selected_series].Points[point_to_highlight].YValues[0]);
                }

                /* TODO: Рескейлинг оси Y при подходе к крайним занчениям */
                //if (Series[selected_series].Points[point_to_highlight].YValues[0] >
                //    default_chart_coord.y_min + (default_chart_coord.y_max - default_chart_coord.y_min) * 0.9)
                //{
                //    this.ChartAreas[0].AxisY.Maximum = default_chart_coord.y_min + (default_chart_coord.y_max - default_chart_coord.y_min) * 1.01;
                //    default_chart_coord.y_max = default_chart_coord.y_min + (default_chart_coord.y_max - default_chart_coord.y_min) * 1.01;
                //}

                if (PointChanged != null)
                    PointChanged(Series[selected_series].Points[point_to_highlight], point_to_highlight);
            }

            /* Проверяем наведен ли курсор на какой либо узел серии (кроме случаев когда уже двигаем узел и только вошли на график) */
            if (enable_add_point && !is_enter && !move_highlighted_point && selected_series >= 0)
            {
                for (int j = 0; j < Series[selected_series].Points.Count; ++j)
                {
                    if (e.X > (float)ChartAreas[0].AxisX.ValueToPixelPosition(Series[selected_series].Points[j].XValue) - point_inner_size * 2 &&
                        e.X < (float)ChartAreas[0].AxisX.ValueToPixelPosition(Series[selected_series].Points[j].XValue) - point_inner_size * 2 + point_inner_size * 4 &&
                        e.Y > (float)ChartAreas[0].AxisY.ValueToPixelPosition(Series[selected_series].Points[j].YValues[0]) - point_inner_size * 2 &&
                        e.Y < (float)ChartAreas[0].AxisY.ValueToPixelPosition(Series[selected_series].Points[j].YValues[0]) - point_inner_size * 2 + point_inner_size * 4)
                    {
                        point_to_highlight = j;
                        break;
                    }
                    else
                    {
                        point_to_highlight = -1;
                    }
                }
            }

            /* Проверяем между какими точками пользователь хочет добавить новый узел, чтобы затем отрисовать подсказывающий узел */
            if (is_in_chart_area && this.Cursor == Cursors.Cross && selected_series >= 0)
            {
                for (int i = 1; i < Series[selected_series].Points.Count; ++i)
                    if (this.ChartAreas[0].AxisX.PixelPositionToValue(e.X) > Series[selected_series].Points[i - 1].XValue &&
                        this.ChartAreas[0].AxisX.PixelPositionToValue(e.X) < Series[selected_series].Points[i].XValue)
                        potentially_new_point_index = i;
            }
        }

        void MyChart_MouseClick(object sender, MouseEventArgs e)
        {
            /* Какую серию выбирает пользователь при нажатии на легенду */
            if (item_to_highlight >= 0 && is_able_to_choose)
                selected_series = item_to_highlight;

            /* Был ли клик правой кнопкой мыши по подсвечиваемой серии в легенде чтобы вызвать контекстное меню */
            if (e.X > series_selection_area.X && e.X < series_selection_area.X + series_selection_area.Width &&
                e.Y > series_selection_area.Y + item_to_highlight * text_size.Height && 
                e.Y < series_selection_area.Y + item_to_highlight * text_size.Height + text_size.Height &&
                e.Button == MouseButtons.Right)
            {
                ChartSeriesContextMenuStrip.Show(this, e.X, e.Y);
            }

            /* Был ли клик ПКМ по подсвечиваемому узлу на графике чтобы вызвать контекстное меню */
            if (point_to_highlight != -1 &&
                selected_series >= 0 &&
                e.Button == MouseButtons.Right)
            {
                PointPositionContextMenuStrip.Show(this, e.X, e.Y);
            }

            /* Было ли нажатие вне легенды, вне области опций и вне области графика чтобы снять выделение серии */
            if (!(series_selection_area.Contains(e.Location) ||
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
                    //default_chart_coord = new chart_coord((double)this.ChartAreas[0].AxisX.Minimum,
                    //                            (double)this.ChartAreas[0].AxisX.Maximum,
                    //                            (double)this.ChartAreas[0].AxisY.Minimum,
                    //                            (double)this.ChartAreas[0].AxisY.Maximum);
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

            /* Было ли нажатие на функцию добавить точки */
            if (e.X > options_area.X + 2 + 23 && e.X < options_area.X + 20 + 23 &&
                e.Y > options_area.Y + 1 && e.Y < options_area.Y + 19 && e.Button == MouseButtons.Left)
            {
                if (this.Cursor != Cursors.Cross)
                {
                    this.Cursor = Cursors.Cross;
                    if ((selected_series < 0 || !is_able_to_choose) && this.Series.Count > 0)
                        selected_series = 0;
                }
                else
                    this.Cursor = Cursors.Default;
            }

            /* Было ли ЛКМ во время добавления узла на график */
            if (is_in_chart_area && this.Cursor == Cursors.Cross && e.Button == MouseButtons.Left && point_to_highlight < 0)
            {
                Series[selected_series].Points.Insert(potentially_new_point_index,
                    new DataPoint(RoundWithXMargin(this.ChartAreas[0].AxisX.PixelPositionToValue(e.X)),
                                    RoundWithYMargin(this.ChartAreas[0].AxisY.PixelPositionToValue(e.Y))));

                if (PointAdded != null)
                    PointAdded(Series[selected_series].Points[potentially_new_point_index], potentially_new_point_index);
            }

            /* Было ли нажатие ЛКМ чтобы удалить узел */
            if (point_to_delete != -1 && e.Button == MouseButtons.Left)
            {
                Series[selected_series].Points.RemoveAt(point_to_delete);

                if (PointDeleted != null)
                    PointDeleted(null, point_to_delete);
            }

            /* Было ли ПКМ во время добавления узла на график чтобы убрать инструмент добавления */
            if (is_in_chart_area && this.Cursor == Cursors.Cross && e.Button == MouseButtons.Right)
            {
                this.Cursor = Cursors.Default;
            }

            /* Было ли нажатие где-либо вне хитбокса опций и вне легенды, чтобы отказаться от выбора инструмента */
            if (!((e.X > options_area.X + 2 && e.X < options_area.X + 20 &&
                e.Y > options_area.Y + 1 && e.Y < options_area.Y + 19) ||
                (e.X > options_area.X + 2 + 23 && e.X < options_area.X + 20 + 23 &&
                e.Y > options_area.Y + 1 && e.Y < options_area.Y + 19) ||
                (e.X > zoom_options_area.X + 1 && e.X < zoom_options_area.X + 19 &&
                e.Y > zoom_options_area.Y + 2 && e.Y < zoom_options_area.Y + 20) ||
                (e.X > zoom_options_area.X + 1 && e.X < zoom_options_area.X + 19 &&
                e.Y > zoom_options_area.Y + 23 && e.Y < zoom_options_area.Y + 41) ||
                (series_selection_area.Contains(e.Location) && is_able_to_choose))
                && !is_in_chart_area)
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
            /* Отслеживаем нажатие по графику чтобы начать отрисовывть область приближения */
            if (is_in_chart_area && this.Cursor == Cursors.NoMove2D && e.Button == MouseButtons.Left)
            {
                tozoom_x = e.X;
                tozoom_y = e.Y;
                is_L_mouse_down = true;
            }

            /* Отслеживаем нажатие по графику правой кнопкой мыши чтобы начать двигать все серии */
            if (is_in_chart_area && this.Cursor == Cursors.NoMove2D && e.Button == MouseButtons.Right)
            {
                this.Cursor = Cursors.Hand;
                toshift_x = e.X;
                toshift_y = e.Y;
                is_R_mouse_down = true;
            }

            /* Отслеживаем нажатие ЛКМ по узлу графика чтобы начать его двигать */
            if (point_to_highlight != -1 &&
                e.Button == MouseButtons.Left &&
                this.Cursor != Cursors.NoMove2D &&
                selected_series >= 0)
            {
                move_highlighted_point = true;
            }
            else move_highlighted_point = false;
        }

        void MyChart_MouseUp(object sender, MouseEventArgs e)
        {
            /* Отслеживаем отжатие левой кнопки мыши чтобы перестать двигать выбранный узел */
            if (point_to_highlight != -1 &&
                selected_series >= 0 && e.Button == MouseButtons.Left)
            {
                move_highlighted_point = false;
            }

            /* Устанавливаем приближение после того как ЛКМ отжата */
            if (is_in_chart_area && this.Cursor == Cursors.NoMove2D && e.Button == MouseButtons.Left)
            {
                zoom_history.Add(new chart_coord((double)this.ChartAreas[0].AxisX.Minimum,
                                                (double)this.ChartAreas[0].AxisX.Maximum,
                                                (double)this.ChartAreas[0].AxisY.Minimum,
                                                (double)this.ChartAreas[0].AxisY.Maximum));

                double v1 = Math.Round(this.ChartAreas[0].AxisX.PixelPositionToValue(zoom_area.X),
                    MathDecimals.GetDecimalPlaces(point_value_X_margin));
                double v2 = Math.Round(this.ChartAreas[0].AxisX.PixelPositionToValue(zoom_area.X + zoom_area.Width),
                    MathDecimals.GetDecimalPlaces(point_value_X_margin));

                if (v1 == v2)
                {
                    is_L_mouse_down = false;
                    return;
                }

                this.ChartAreas[0].AxisX.Minimum = v1;
                this.ChartAreas[0].AxisX.Maximum = v2;

                double v3 = Math.Round(this.ChartAreas[0].AxisY.PixelPositionToValue(zoom_area.Y + zoom_area.Height),
                    MathDecimals.GetDecimalPlaces(point_value_Y_margin));
                double v4 = Math.Round(this.ChartAreas[0].AxisY.PixelPositionToValue(zoom_area.Y),
                    MathDecimals.GetDecimalPlaces(point_value_Y_margin));

                if (v3 == v4)
                {
                    is_L_mouse_down = false;
                    return;
                }

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

        public void set_default_chart_coord(double x_min_in, double x_max_in, double y_min_in, double y_max_in)
        {
            default_chart_coord = new chart_coord(x_min_in, x_max_in, y_min_in, y_max_in);
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

        double RoundWithXMargin(double value)
        {
            double res = point_value_X_margin * (int)Math.Round(value / point_value_X_margin,
                MathDecimals.GetDecimalPlaces(point_value_X_margin));
            return res <= default_chart_coord.x_min ? res + point_value_X_margin : 
                (res >= default_chart_coord.x_max ? res - point_value_X_margin : res);
        }

        double RoundWithYMargin(double value)
        {
            return point_value_Y_margin * (int)Math.Round(value / point_value_Y_margin,
                MathDecimals.GetDecimalPlaces(point_value_Y_margin));
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
            LineWidthSelectionDialog d = new LineWidthSelectionDialog((int)series_selection_area.X - 100,
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

        class LineWidthSelectionDialog : Control
        {
            MyChart _parent;
            MyNumericUpDown input_box = new MyNumericUpDown() { Left = 0, Top = 0, Width = 100 };
            Button ok_button = new Button() { Text = "Ok", Left = 0, Top = 20, Width = 50 };
            Button cancel_button = new Button() { Text = "Cancel", Left = 50, Top = 20, Width = 50 };

            public LineWidthSelectionDialog(int X, int Y, MyChart parent)
            {
                parent.Controls.Add(this);
                _parent = parent;

                this.Width = 100;
                this.Height = 42;
                this.Location = new Point(X, Y);

                this.Controls.AddRange(new Control[] { input_box, ok_button, cancel_button });

                ok_button.Click += new EventHandler(LineWidthSelectionDialog_Ok);
                cancel_button.Click += new EventHandler(LineWidthSelectionDialog_Cancel);
                parent.SelectedSeriesChanged += new EventHandler(LineWidthSelectionDialog_Close);

                input_box.DecimalPlaces = 0;
                input_box.Value = parent.Series[parent.selected_series].BorderWidth;
            }

            void LineWidthSelectionDialog_Ok(object sender, EventArgs e)
            {
                _parent.Series[_parent.selected_series].BorderWidth = (int)input_box.Value;
                _parent.settings[_parent.FindSettingIndexByName(_parent.Series[_parent.selected_series].Name)].width = (uint)input_box.Value;
                this.Dispose();
            }

            void LineWidthSelectionDialog_Cancel(object sender, EventArgs e)
            {
                this.Dispose();
            }

            void LineWidthSelectionDialog_Close(object sender, EventArgs e)
            {
                this.Dispose();
            }
        }

        void MyChart_PointPositionChoose(object sender, EventArgs e)
        {
            PointValuesSelection d = new PointValuesSelection((int)highligt_x, (int)highligt_y, this);
            d.Invalidate();
        }

        class PointValuesSelection : Control
        {
            MyChart _parent;
            Label X_label = new Label() { Text = "X", Left = 70, Top = 3, Width = 14, Height = 13 };
            Label Y_label = new Label() { Text = "Y", Left = 70, Top = 25, Width = 14, Height = 13 };
            MyNumericUpDown input_box_X = new MyNumericUpDown() { Left = 0, Top = 0, Width = 100 };
            MyNumericUpDown input_box_Y = new MyNumericUpDown() { Left = 0, Top = 21, Width = 100 };
            Button ok_button = new Button() { Text = "Ok", Left = 100, Top = -1, Height = 22, Width = 50 };
            Button cancel_button = new Button() { Text = "Cancel", Left = 100, Top = 20, Height = 22, Width = 50 };
            int _selected_series, _point_to_highlight; 

            public PointValuesSelection(int X, int Y, MyChart parent)
            {
                parent.Controls.Add(this);
                _parent = parent;
                _selected_series = parent.selected_series;
                _point_to_highlight = parent.point_to_highlight;

                this.Width = 150;
                this.Height = 41;
                this.Location = new Point(X, Y);

                this.Controls.AddRange(new Control[] { X_label, Y_label, input_box_X, input_box_Y, ok_button, cancel_button });

                ok_button.Click += new EventHandler(PointValuesSelection_Ok);
                cancel_button.Click += new EventHandler(PointValuesSelection_Cancel);
                parent.Leave += new EventHandler(PointValuesSelection_Close);
                parent.MouseMove += new MouseEventHandler(PointValuesSelection_MouseMove);

                if (parent.X_margin_needed != null)
                    parent.X_margin_needed(parent, null);

                if (_point_to_highlight == 0 ||
                    _point_to_highlight == parent.Series[_selected_series].Points.Count - 1)
                {
                    input_box_X.Value = (decimal)parent.Series[_selected_series].Points[_point_to_highlight].XValue;
                    input_box_X.Enabled = false;
                }
                else
                {
                    input_box_X.Minimum = (decimal)parent.Series[_selected_series].Points[_point_to_highlight - 1].XValue + (decimal)parent.point_value_X_margin;
                    input_box_X.Maximum = (decimal)parent.Series[_selected_series].Points[_point_to_highlight + 1].XValue - (decimal)parent.point_value_X_margin;
                    input_box_X.Increment = (decimal)parent.point_value_X_margin;
                    input_box_X.Value = (decimal)parent.Series[_selected_series].Points[_point_to_highlight].XValue;
                }

                input_box_Y.Minimum = -999999999999;
                input_box_Y.Maximum = 999999999999;
                input_box_Y.Increment = 0.1m;
                input_box_Y.Value = (decimal)parent.Series[_selected_series].Points[_point_to_highlight].YValues[0];

                input_box_X.DecimalPlaces = MathDecimals.GetDecimalPlaces(parent.point_value_X_margin);
                input_box_Y.DecimalPlaces = MathDecimals.GetDecimalPlaces(parent.point_value_X_margin);
            }

            void PointValuesSelection_Ok(object sender, EventArgs e)
            {
                if (_parent.X_margin_needed != null)
                    _parent.X_margin_needed(_parent, null);

                _parent.Series[_selected_series].Points[_point_to_highlight].XValue = _parent.RoundWithXMargin((double)input_box_X.Value);
                _parent.Series[_selected_series].Points[_point_to_highlight].YValues[0] = _parent.RoundWithYMargin((double)input_box_Y.Value);
                _parent.ChartAreas[0].AxisY.Minimum = (double)input_box_Y.Value < _parent.ChartAreas[0].AxisY.Minimum ?
                    (double)input_box_Y.Value : _parent.ChartAreas[0].AxisY.Minimum;
                _parent.ChartAreas[0].AxisY.Maximum = (double)input_box_Y.Value > _parent.ChartAreas[0].AxisY.Maximum ?
                    (double)input_box_Y.Value : _parent.ChartAreas[0].AxisY.Maximum;

                if (_parent.PointChanged != null)
                    _parent.PointChanged(_parent.Series[_selected_series].Points[_point_to_highlight], _point_to_highlight);

                this.Dispose();
            }

            void PointValuesSelection_Cancel(object sender, EventArgs e)
            {
                this.Dispose();
            }

            void PointValuesSelection_Close(object sender, EventArgs e)
            {
                this.Dispose();
            }

            void PointValuesSelection_MouseMove(object sender, MouseEventArgs e)
            {
                if (!(e.X > this.Location.X - 30 && e.X < this.Location.X + this.Width + 30 &&
                    e.Y > this.Location.Y - 30 && e.Y < this.Location.Y + this.Height + 30))
                    this.Dispose();
            }
        }
    }
}
