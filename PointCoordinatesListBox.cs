using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection.Emit;
using System.Windows.Forms.DataVisualization.Charting;
using System.ComponentModel;
using System.Xml.Linq;
using System.Data;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace ImpulseMaker
{
    internal class PointCoordinatesListBox : Control
    {
        #region parent

        ChildContainer container;
        Button add_point;
        Button delete_point;
        int roundness = 15;
        event EventHandler RemoveGuideLines;
        public event EventHandler X_margin_needed;
        public delegate void PointEventHandler(DataPoint point, int index);
        public event PointEventHandler PointChanged, PointAdded, PointDeleted;
        double p_v_X_m = 0.1;
        public bool is_from_outside = false;

        public double point_value_X_margin
        {
            get { return p_v_X_m; }
            set
            { 
                p_v_X_m = value;
                //Invalidate();
            }
        }

        public PointCoordinatesListBox()
        {
            this.Paint += new PaintEventHandler(this.OnPaint);
            this.SizeChanged += new EventHandler(this.Add_Controls);
            this.MouseLeave += new EventHandler(this.OnMouseLeave);
            this.Click += new EventHandler(this.OnClick);
        }

        void Add_Controls(object sender, EventArgs e)
        {
            container = new ChildContainer(new Rectangle(2, roundness, Width - 4, Height - roundness - 30));
            add_point = new Button
            { 
                Location = new Point((int)(20 + (this.Width - 40) * 0.25f - (this.Width - 40) / 6), this.Height - 25),
                Width = (this.Width - 40) / 3,
                BackColor = Color.LightGray,
                Text = "Добавить"
            };
            delete_point = new Button
            {
                Location = new Point((int)(20 + (this.Width - 40) * 0.75f - (this.Width - 40) / 6), this.Height - 25),
                Width = (this.Width - 40) / 3,
                BackColor = Color.LightGray,
                Text = "Удалить"
            };

            this.Controls.AddRange(new Control[]
                {
                    container,
                    add_point,
                    delete_point
                });
            this.SizeChanged -= new EventHandler(this.Add_Controls);

            add_point.Click += new EventHandler(container.AddPoint_ButtonClick);
            delete_point.Click += new EventHandler(container.DelPoint_ButtonClick);
        }

        void OnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawPath(new Pen(Color.White, roundness - 5),
                RoundedRectangle.Create(Rectangle.Round(new RectangleF(-4, -4, this.Width - 1 + 9, this.Height - 1 + 9)), roundness));
            e.Graphics.DrawPath(Pens.LightGray,
                RoundedRectangle.Create(Rectangle.Round(new RectangleF(0, 0, this.Width - 1, this.Height - 1)), roundness));
            e.Graphics.DrawString("X:", Font, Brushes.Black, new PointF(this.Width * 0.25f, 2));
            e.Graphics.DrawString("Y:", Font, Brushes.Black, new PointF(this.Width * 0.65f, 2));
        }

        void OnMouseLeave(object sender, EventArgs e)
        {
            if (!this.container.ClientRectangle.Contains(this.container.PointToClient(System.Windows.Forms.Cursor.Position)) &&
                !this.add_point.ClientRectangle.Contains(this.add_point.PointToClient(System.Windows.Forms.Cursor.Position)) &&
                !this.delete_point.ClientRectangle.Contains(this.delete_point.PointToClient(System.Windows.Forms.Cursor.Position)) &&
                (this.Cursor == Cursors.PanEast ||
                this.container.Cursor == Cursors.PanEast))
            {
                add_point.BackColor = Color.LightGray;
                delete_point.BackColor = Color.LightGray;
                this.Cursor = Cursors.Default;
                this.container.Cursor = Cursors.Default;
                if (RemoveGuideLines != null)
                    RemoveGuideLines(this, null);
            }
        }

        void OnClick(object sender, EventArgs e)
        {
            if (this.Cursor == Cursors.PanEast || this.container.Cursor == Cursors.PanEast)
            {
                add_point.BackColor = Color.LightGray;
                delete_point.BackColor = Color.LightGray;
                this.Cursor = Cursors.Default;
                this.container.Cursor = Cursors.Default;
                if (RemoveGuideLines != null)
                    RemoveGuideLines(this, null);
            }
        }

        public void Set_DataSet(DataPoint[] dataset)
        {
            if (this.Controls.Count <= 0)
                Add_Controls(this, null);

            container.points_set.Clear();

            /* Запрашиваем извне шаг, с которым можно изменять значения по Х */
            if (X_margin_needed != null)
                X_margin_needed(this, null);

            for (int i = 0; i < dataset.Count(); ++i)
            {
                container.points_set.Add(new ChildContainer.point_coordinates_nud
                {
                    nud_X = new MyNumericUpDown
                    {
                        Location = new Point(20, i * 20),
                        Width = (this.Width - 40) / 2,
                        Minimum = Math.Round(i == 0 ? 0 : (i == dataset.Count() - 1 ? (decimal)(dataset[i].XValue) :
                            (decimal)(dataset[i - 1].XValue + point_value_X_margin)), MathDecimals.GetDecimalPlaces(point_value_X_margin)),
                        Maximum = Math.Round(i == dataset.Count() - 1 ? (decimal)(dataset[dataset.Count() - 1].XValue) :
                            (decimal)(dataset[i + 1].XValue - point_value_X_margin), MathDecimals.GetDecimalPlaces(point_value_X_margin)),
                        //Value = (decimal)dataset[i].XValue,
                        Increment = (decimal)point_value_X_margin,
                        DecimalPlaces = MathDecimals.GetDecimalPlaces(point_value_X_margin)
                    },
                    nud_Y = new MyNumericUpDown
                    {
                        Location = new Point(this.Width / 2, i * 20),
                        Width = (this.Width - 40) / 2,
                        Minimum = -100,
                        Maximum = 100,
                        Value = (decimal)dataset[i].YValues[0],
                        Increment = 0.1m,
                        DecimalPlaces = MathDecimals.GetDecimalPlaces(dataset[i].YValues[0]) + 2
                    },
                    num = new System.Windows.Forms.Label
                    {
                        Location = new Point(0, i * 20 + 2),
                        Width = 20,
                        Height = 20,
                        Text = (i + 1).ToString()
                    }
                });
                this.container.points_set[i].nud_X.Value = (decimal)dataset[i].XValue < this.container.points_set[i].nud_X.Minimum ?
                            this.container.points_set[i].nud_X.Minimum : ((decimal)dataset[i].XValue > this.container.points_set[i].nud_X.Maximum ?
                            this.container.points_set[i].nud_X.Maximum : (decimal)dataset[i].XValue);
            }

            container.AddAllPointsControlsToThisControl();
        }

        public DataPoint[] Get_DataSet()
        {
            if (container.points_set.Count <= 0)
                return null;

            DataPoint[] dataset = new DataPoint[container.points_set.Count];

            for (int i = 0; i < dataset.Count(); ++i)
                dataset[i] = new DataPoint((double)container.points_set[i].nud_X.Value, (double)container.points_set[i].nud_Y.Value);

            return dataset;
        }
        public void Set_ChangedPoint(DataPoint point, int index)
        {
            if (index < 0 || index > this.container.points_set.Count - 1)
                return;

            /* Запрашиваем извне шаг, с которым можно изменять значения по Х */
            if (X_margin_needed != null)
                X_margin_needed(this, null);

            if (index != 0)
                this.container.points_set[index - 1].nud_X.Maximum = Math.Round((decimal)(point.XValue - point_value_X_margin),
                    MathDecimals.GetDecimalPlaces(point_value_X_margin));
            if (index != this.container.points_set.Count - 1)
                this.container.points_set[index + 1].nud_X.Minimum = Math.Round((decimal)(point.XValue + point_value_X_margin),
                    MathDecimals.GetDecimalPlaces(point_value_X_margin));

            this.container.points_set[index].nud_X.Value = (decimal)point.XValue < this.container.points_set[index].nud_X.Minimum ?
                this.container.points_set[index].nud_X.Minimum : ((decimal)point.XValue > this.container.points_set[index].nud_X.Maximum ?
                this.container.points_set[index].nud_X.Maximum : (decimal)point.XValue);
            this.container.points_set[index].nud_Y.Value = (decimal)point.YValues[0];
        }

        public void Set_AddedPoint(DataPoint point, int index)
        {
            if (index < 0 || index > this.container.points_set.Count)
                return;

            /* Запрашиваем извне шаг, с которым можно изменять значения по Х */
            if (X_margin_needed != null)
                X_margin_needed(this, null);

            if (index != 0)
                this.container.points_set[index - 1].nud_X.Maximum = Math.Round((decimal)(point.XValue - point_value_X_margin),
                    MathDecimals.GetDecimalPlaces(point_value_X_margin));
            if (index != this.container.points_set.Count)
                this.container.points_set[index].nud_X.Minimum = Math.Round((decimal)(point.XValue + point_value_X_margin),
                    MathDecimals.GetDecimalPlaces(point_value_X_margin));

            this.container.Add_Point_at(index, point);
        }

        public void Set_DeletedPoint(DataPoint point, int index)
        {
            if (index < 0 || index > this.container.points_set.Count - 1)
                return;

            /* Запрашиваем извне шаг, с которым можно изменять значения по Х */
            if (X_margin_needed != null)
                X_margin_needed(this, null);

            if (index != 0)
                this.container.points_set[index - 1].nud_X.Maximum = Math.Round(this.container.points_set[index + 1].nud_X.Value -
                    (decimal)point_value_X_margin, MathDecimals.GetDecimalPlaces(point_value_X_margin));
            if (index != this.container.points_set.Count - 1)
                this.container.points_set[index + 1].nud_X.Minimum = Math.Round(this.container.points_set[index - 1].nud_X.Value +
                    (decimal)point_value_X_margin, MathDecimals.GetDecimalPlaces(point_value_X_margin));

            this.container.Del_Point_at(index);
        }

        double RoundWithXMargin(double value)//TODO
        {
            double res = point_value_X_margin * (int)(value / point_value_X_margin);
            return res <= (double)container.points_set[0].nud_X.Minimum ? res + point_value_X_margin :
                (res >= (double)container.points_set[container.points_set.Count - 1].nud_X.Maximum ? res - point_value_X_margin : res);
        }

        #endregion

        #region child

        class ChildContainer : Control
        {
            public List<point_coordinates_nud> points_set = new List<point_coordinates_nud>();
            ContainerOverlay overlay;
            bool a = true;
            int adding_deleting = -1;

            public struct point_coordinates_nud
            {
                public MyNumericUpDown nud_X;
                public MyNumericUpDown nud_Y;
                public System.Windows.Forms.Label num;
            }

            public ChildContainer(Rectangle rect)
            {
                this.Location = new Point(rect.X, rect.Y);
                this.Width = rect.Width;
                this.Height = rect.Height;

                this.MouseWheel += new MouseEventHandler(this.OnMouseWheel);
            }

            public void AddPoint_ButtonClick(object sender, EventArgs e)
            {
                if (this.Parent.Cursor != Cursors.PanEast)
                {
                    ((PointCoordinatesListBox)(this.Parent)).RemoveGuideLines += OnRemoveGuideLines;

                    ((PointCoordinatesListBox)(this.Parent)).add_point.BackColor = Color.LightGreen;
                    this.Cursor = Cursors.PanEast;
                    this.Parent.Cursor = Cursors.PanEast;

                    this.Controls.Add(overlay = new ContainerOverlay(this.ClientRectangle));
                    overlay.BringToFront();

                    adding_deleting = 0;
                }
                else
                {
                    ((PointCoordinatesListBox)(this.Parent)).RemoveGuideLines -= OnRemoveGuideLines;

                    ((PointCoordinatesListBox)(this.Parent)).add_point.BackColor = Color.LightGray;
                    this.Cursor = Cursors.Default;
                    this.Parent.Cursor = Cursors.Default;
                    this.Controls.Remove(overlay);

                    adding_deleting = -1;
                }
            }

            public void DelPoint_ButtonClick(object sender, EventArgs e)
            {
                if (this.Parent.Cursor != Cursors.PanEast && this.points_set.Count > 2)
                {
                    ((PointCoordinatesListBox)(this.Parent)).RemoveGuideLines += OnRemoveGuideLines;

                    ((PointCoordinatesListBox)(this.Parent)).delete_point.BackColor = Color.FromArgb(255, 100, 100);
                    this.Cursor = Cursors.PanEast;
                    this.Parent.Cursor = Cursors.PanEast;

                    this.Controls.Add(overlay = new ContainerOverlay(this.ClientRectangle));
                    overlay.BringToFront();

                    adding_deleting = 1;
                }
                else
                {
                    ((PointCoordinatesListBox)(this.Parent)).RemoveGuideLines -= OnRemoveGuideLines;

                    ((PointCoordinatesListBox)(this.Parent)).delete_point.BackColor = Color.LightGray;
                    this.Cursor = Cursors.Default;
                    this.Parent.Cursor = Cursors.Default;
                    this.Controls.Remove(overlay);

                    adding_deleting = -1;
                }
            }

            public void AddAllPointsControlsToThisControl()
            {
                if (this.points_set.Count <= 0)
                    return;

                this.Controls.Clear();

                Control[] X = new Control[points_set.Count];
                Control[] Y = new Control[points_set.Count];
                Control[] N = new Control[points_set.Count];

                for (int i = 0; i < X.Count(); ++i)
                {
                    X[i] = points_set[i].nud_X;
                    Y[i] = points_set[i].nud_Y;
                    N[i] = points_set[i].num;

                    points_set[i].nud_X.MouseWheel += new MouseEventHandler(this.OnMouseWheel);
                    points_set[i].nud_Y.MouseWheel += new MouseEventHandler(this.OnMouseWheel);
                    points_set[i].num.MouseWheel += new MouseEventHandler(this.OnMouseWheel);

                    points_set[i].nud_X.ValueChanged += new EventHandler(this.OnValueChanged);
                    points_set[i].nud_Y.ValueChanged += new EventHandler(this.OnValueChanged);
                }

                this.Controls.AddRange(X);
                this.Controls.AddRange(Y);
                this.Controls.AddRange(N);
            }

            void OnValueChanged(object sender, EventArgs e)
            {
                int index = FindPointControlIndex((MyNumericUpDown)sender);

                if (index >= 0 && ((PointCoordinatesListBox)(this.Parent)).PointChanged != null)
                    ((PointCoordinatesListBox)(this.Parent)).PointChanged(new DataPoint((double)points_set[index].nud_X.Value, (double)points_set[index].nud_Y.Value), index);
            }

            int FindPointControlIndex(MyNumericUpDown control)
            {
                for (int i = 0; i < points_set.Count(); ++i)
                    if (control == points_set[i].nud_X || control == points_set[i].nud_Y)
                        return i;

                return -1;
            }

            void OnMouseWheel(object sender, MouseEventArgs e)
            {
                ((HandledMouseEventArgs)e).Handled = true;

                if (e.Delta < 0)
                {
                    foreach (var c in points_set)
                    {
                        c.nud_X.Location = new Point(c.nud_X.Location.X, c.nud_X.Location.Y - c.nud_X.Height / 2);
                        c.nud_Y.Location = new Point(c.nud_Y.Location.X, c.nud_Y.Location.Y - c.nud_X.Height / 2);
                        c.num.Location = new Point(c.num.Location.X, c.num.Location.Y - c.nud_X.Height / 2);
                    }
                }
                else
                {
                    foreach (var c in points_set)
                    {
                        c.nud_X.Location = new Point(c.nud_X.Location.X, c.nud_X.Location.Y + c.nud_Y.Height / 2);
                        c.nud_Y.Location = new Point(c.nud_Y.Location.X, c.nud_Y.Location.Y + c.nud_Y.Height / 2);
                        c.num.Location = new Point(c.num.Location.X, c.num.Location.Y + c.nud_X.Height / 2);
                    }
                }
            }

            void OnRemoveGuideLines(object sender, EventArgs e)
            {
                this.Controls.Remove(overlay);

                ((PointCoordinatesListBox)(this.Parent)).RemoveGuideLines -= OnRemoveGuideLines;

                adding_deleting = -1;
            }

            public void Add_Point_at(int index, DataPoint point = null)
            {
                /* Запрашиваем извне шаг, с которым можно изменять значения по Х */
                if (((PointCoordinatesListBox)(this.Parent)).X_margin_needed != null)
                    ((PointCoordinatesListBox)(this.Parent)).X_margin_needed(this, null);

                points_set.Insert(index, new point_coordinates_nud
                {
                    nud_X = new MyNumericUpDown
                    { 
                        Width = (this.Parent.Width - 40) / 2,
                        Minimum = Math.Round(index == 0 ? 0 : points_set[index - 1].nud_X.Value + 
                            (decimal)((PointCoordinatesListBox)(this.Parent)).point_value_X_margin,
                            MathDecimals.GetDecimalPlaces(((PointCoordinatesListBox)(this.Parent)).point_value_X_margin)),
                        Maximum = Math.Round(index == points_set.Count() ? points_set[points_set.Count - 1].nud_X.Value :
                            index == 0 ? 0 :
                            points_set[index].nud_X.Value - (decimal)((PointCoordinatesListBox)(this.Parent)).point_value_X_margin,
                            MathDecimals.GetDecimalPlaces(((PointCoordinatesListBox)(this.Parent)).point_value_X_margin)),
                        Increment = (decimal)((PointCoordinatesListBox)(this.Parent)).point_value_X_margin,
                        DecimalPlaces = MathDecimals.GetDecimalPlaces(((PointCoordinatesListBox)(this.Parent)).point_value_X_margin)
                    },
                    nud_Y = new MyNumericUpDown
                    { 
                        Width = (this.Parent.Width - 40) / 2,
                        Minimum = -100,
                        Maximum = 100,
                        Increment = 0.1m,
                        Value = point != null ? (decimal)point.YValues[0] : (index == 0 ? points_set[index].nud_Y.Value :
                            (index == points_set.Count ? points_set[index - 1].nud_Y.Value :
                            points_set[index].nud_Y.Value)),
                        DecimalPlaces = MathDecimals.GetDecimalPlaces(index == 0 ? points_set[index].nud_Y.Value :
                            (index == points_set.Count ? points_set[index - 1].nud_Y.Value : points_set[index].nud_Y.Value)) + 2
                    },
                    num = new System.Windows.Forms.Label { Width = 20, Height = 20 }
                });
                points_set[index].nud_X.Value = point != null ? ((decimal)point.XValue < points_set[index].nud_X.Minimum ?
                    points_set[index].nud_X.Minimum : ((decimal)point.XValue > points_set[index].nud_X.Maximum ?
                    points_set[index].nud_X.Maximum : (decimal)point.XValue)) : (index == 0 ? points_set[index].nud_X.Minimum : (index == points_set.Count - 1 ? points_set[index - 1].nud_X.Value :
                            (points_set[index].nud_X.Minimum + (points_set[index + 1].nud_X.Value - points_set[index - 1].nud_X.Value) / 2 < 
                            points_set[index].nud_X.Minimum ? points_set[index].nud_X.Minimum : (points_set[index].nud_X.Minimum + 
                            (points_set[index + 1].nud_X.Value - points_set[index - 1].nud_X.Value) / 2) > points_set[index].nud_X.Maximum ?
                            points_set[index].nud_X.Maximum : points_set[index].nud_X.Minimum + (points_set[index + 1].nud_X.Value - points_set[index - 1].nud_X.Value) / 2)));

                if (index != 0)
                    this.points_set[index - 1].nud_X.Maximum = Math.Round(this.points_set[index].nud_X.Value -
                        (decimal)((PointCoordinatesListBox)(this.Parent)).point_value_X_margin,
                        MathDecimals.GetDecimalPlaces(((PointCoordinatesListBox)(this.Parent)).point_value_X_margin));
                if (index == 0)
                    this.points_set[index + 1].nud_X.Maximum = Math.Round(this.points_set[index + 2].nud_X.Value -
                        (decimal)((PointCoordinatesListBox)(this.Parent)).point_value_X_margin,
                        MathDecimals.GetDecimalPlaces(((PointCoordinatesListBox)(this.Parent)).point_value_X_margin));
                if (index != this.points_set.Count() - 1)
                    this.points_set[index + 1].nud_X.Minimum = Math.Round(this.points_set[index].nud_X.Value +
                        (decimal)((PointCoordinatesListBox)(this.Parent)).point_value_X_margin,
                        MathDecimals.GetDecimalPlaces(((PointCoordinatesListBox)(this.Parent)).point_value_X_margin));
                if (index == this.points_set.Count() - 1)
                    this.points_set[index - 1].nud_X.Minimum = Math.Round(this.points_set[index - 2].nud_X.Value +
                        (decimal)((PointCoordinatesListBox)(this.Parent)).point_value_X_margin,
                        MathDecimals.GetDecimalPlaces(((PointCoordinatesListBox)(this.Parent)).point_value_X_margin));

                for (int i = 0; i < points_set.Count; ++i)
                {
                    points_set[i].nud_X.Location = new Point(20, i * 20);
                    points_set[i].nud_Y.Location = new Point(this.Parent.Width / 2, i * 20);
                    points_set[i].num.Location = new Point(0, i * 20 + 2);
                    points_set[i].num.Text = (i + 1).ToString();
                }

                AddAllPointsControlsToThisControl();

                ((PointCoordinatesListBox)(this.Parent)).RemoveGuideLines -= OnRemoveGuideLines;

                ((PointCoordinatesListBox)(this.Parent)).add_point.BackColor = Color.LightGray;
                this.Cursor = Cursors.Default;
                this.Parent.Cursor = Cursors.Default;
                this.Controls.Remove(overlay);

                adding_deleting = -1;

                if (((PointCoordinatesListBox)(this.Parent)).PointAdded != null && !((PointCoordinatesListBox)(this.Parent)).is_from_outside)
                    ((PointCoordinatesListBox)(this.Parent)).PointAdded(new DataPoint((double)this.points_set[index].nud_X.Value,
                        (double)this.points_set[index].nud_Y.Value), index);

                ((PointCoordinatesListBox)(this.Parent)).is_from_outside = false;
            }

            public void Del_Point_at(int index)
            {
                if (index <= 0 || index > points_set.Count() - 1)
                    return;

                /* Запрашиваем извне шаг, с которым можно изменять значения по Х */
                if (((PointCoordinatesListBox)(this.Parent)).X_margin_needed != null)
                    ((PointCoordinatesListBox)(this.Parent)).X_margin_needed(this, null);

                points_set.RemoveAt(index);

                for (int i = 0; i < points_set.Count; ++i)
                {
                    points_set[i].nud_X.Location = new Point(20, i * 20);
                    points_set[i].nud_Y.Location = new Point(this.Width / 2, i * 20);
                    points_set[i].num.Location = new Point(0, i * 20 + 2);
                    points_set[i].num.Text = (i + 1).ToString();
                }

                if (index != 0)
                    this.points_set[index - 1].nud_X.Maximum = Math.Round(this.points_set[index].nud_X.Value -
                        (decimal)((PointCoordinatesListBox)(this.Parent)).point_value_X_margin,
                        MathDecimals.GetDecimalPlaces(((PointCoordinatesListBox)(this.Parent)).point_value_X_margin));
                if (index != this.points_set.Count - 1)
                    this.points_set[index].nud_X.Minimum = Math.Round(this.points_set[index - 1].nud_X.Value +
                        (decimal)((PointCoordinatesListBox)(this.Parent)).point_value_X_margin,
                        MathDecimals.GetDecimalPlaces(((PointCoordinatesListBox)(this.Parent)).point_value_X_margin));

                AddAllPointsControlsToThisControl();

                ((PointCoordinatesListBox)(this.Parent)).RemoveGuideLines -= OnRemoveGuideLines;

                ((PointCoordinatesListBox)(this.Parent)).delete_point.BackColor = Color.LightGray;
                this.Cursor = Cursors.Default;
                this.Parent.Cursor = Cursors.Default;
                this.Controls.Remove(overlay);

                adding_deleting = -1;

                if (((PointCoordinatesListBox)(this.Parent)).PointDeleted != null && !((PointCoordinatesListBox)(this.Parent)).is_from_outside)
                    ((PointCoordinatesListBox)(this.Parent)).PointDeleted(null, index);

                ((PointCoordinatesListBox)(this.Parent)).is_from_outside = false;
            }

            #region child_overlay

            class ContainerOverlay : Control
            {
                System.Windows.Forms.Label label;
                int index_to_insert = -1,
                    index_to_remove = -1;

                /* мегакостыль, не понимаю что это, нужно чтобы стирались старые пиксели (Parent.Invalidate())*/
                protected override CreateParams CreateParams
                {
                    get
                    {
                        CreateParams cp = base.CreateParams;
                        cp.ExStyle = cp.ExStyle | 0x20;
                        return cp;
                    }
                }

                public ContainerOverlay(Rectangle rect)
                {
                    SetStyle(ControlStyles.Opaque, true);

                    this.Location = new Point(rect.X, rect.Y);
                    this.Width = rect.Width;
                    this.Height = rect.Height;

                    this.Controls.Add(label = new System.Windows.Forms.Label() { BackColor = Color.LightBlue, Width = this.Width, Height = 2 });
                    label.BringToFront();
                    label.Visible = false;

                    this.MouseMove += new MouseEventHandler(this.OnMouseMove);  
                    this.label.MouseClick += new MouseEventHandler(this.OnClick);
                    this.MouseClick += new MouseEventHandler(this.OnClick);
                }

                void OnMouseMove(object sender, MouseEventArgs e)
                {
                    switch (((ChildContainer)this.Parent).adding_deleting)
                    {
                        case 0:
                            if (label.Height != 2)
                                label.Height = 2;

                            if (e.Y < ((ChildContainer)this.Parent).points_set[0].nud_X.Location.Y + ((ChildContainer)this.Parent).points_set[0].nud_X.Height / 2)
                                if (new Point(0, ((ChildContainer)this.Parent).points_set[0].nud_X.Location.Y) != label.Location)
                                {
                                    if (!label.Visible)
                                        label.Visible = true;

                                    Parent.Invalidate(this.Bounds, true);
                                    label.Location = new Point(0, ((ChildContainer)this.Parent).points_set[0].nud_X.Location.Y);
                                    index_to_insert = 0;
                                }

                            if (e.Y > ((ChildContainer)this.Parent).points_set[((ChildContainer)this.Parent).points_set.Count - 1].nud_X.Location.Y
                                + ((ChildContainer)this.Parent).points_set[((ChildContainer)this.Parent).points_set.Count - 1].nud_X.Height / 2)
                                if (new Point(0, ((ChildContainer)this.Parent).points_set[((ChildContainer)this.Parent).points_set.Count - 1].nud_X.Location.Y +
                                    ((ChildContainer)this.Parent).points_set[((ChildContainer)this.Parent).points_set.Count - 1].nud_X.Height) != label.Location)
                                {
                                    if (!label.Visible)
                                        label.Visible = true;

                                    Parent.Invalidate(this.Bounds, true);
                                    label.Location = new Point(0, ((ChildContainer)this.Parent).points_set[((ChildContainer)this.Parent).points_set.Count - 1].nud_X.Location.Y +
                                    ((ChildContainer)this.Parent).points_set[((ChildContainer)this.Parent).points_set.Count - 1].nud_X.Height);
                                    index_to_insert = ((ChildContainer)this.Parent).points_set.Count;
                                }

                            for (int i = 1; i < ((ChildContainer)this.Parent).points_set.Count; ++i)
                            {
                                if (e.Y > ((ChildContainer)this.Parent).points_set[i - 1].nud_X.Location.Y + ((ChildContainer)this.Parent).points_set[i - 1].nud_X.Height / 2 &&
                                    e.Y < ((ChildContainer)this.Parent).points_set[i].nud_X.Location.Y + ((ChildContainer)this.Parent).points_set[i].nud_X.Height / 2)
                                {
                                    if (new Point(0, ((ChildContainer)this.Parent).points_set[i].nud_X.Location.Y) != label.Location)
                                    {
                                        if (!label.Visible)
                                            label.Visible = true;

                                        Parent.Invalidate(this.Bounds, true);
                                        label.Location = new Point(0, ((ChildContainer)this.Parent).points_set[i].nud_X.Location.Y);
                                        index_to_insert = i;
                                    }
                                    break;
                                }
                            }
                            break;
                        case 1:
                            // TODO: при скроллинге колескиом лейбл фракталится, переделать
                            if (label.Height != 20)
                                label.Height = 20;

                            for (int i = 1; i < ((ChildContainer)this.Parent).points_set.Count - 1; ++i)
                            {
                                if (e.Y > ((ChildContainer)this.Parent).points_set[i].nud_X.Location.Y &&
                                    e.Y < ((ChildContainer)this.Parent).points_set[i].nud_X.Location.Y + ((ChildContainer)this.Parent).points_set[i].nud_X.Height)
                                {
                                    if (new Point(0, ((ChildContainer)this.Parent).points_set[i].nud_X.Location.Y) != label.Location)
                                    {
                                        if (!label.Visible)
                                            label.Visible = true;

                                        Parent.Invalidate(this.Bounds, true);
                                        label.Location = new Point(0, ((ChildContainer)this.Parent).points_set[i].nud_X.Location.Y);
                                        index_to_remove = i;
                                    }
                                    break;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }

                void OnClick(object sender, MouseEventArgs e)
                {
                    if (e.Button == MouseButtons.Left && ((ChildContainer)this.Parent).adding_deleting >= 0)
                    {
                        switch (((ChildContainer)this.Parent).adding_deleting)
                        {
                            case 0:
                                ((ChildContainer)this.Parent).Add_Point_at(index_to_insert);
                                break;
                            case 1:
                                ((ChildContainer)this.Parent).Del_Point_at(index_to_remove);
                                break;
                            default:
                                break;
                        }
                    }

                    if (e.Button == MouseButtons.Right)
                    {
                        ((PointCoordinatesListBox)((ChildContainer)this.Parent).Parent).RemoveGuideLines -=
                            ((ChildContainer)this.Parent).OnRemoveGuideLines;

                        ((ChildContainer)this.Parent).Cursor = Cursors.Default;
                        ((ChildContainer)this.Parent).Parent.Cursor = Cursors.Default;
                        
                        ((ChildContainer)this.Parent).adding_deleting = -1;

                        ((ChildContainer)this.Parent).Controls.Remove(((ChildContainer)this.Parent).overlay);
                    }
                }
            }

            #endregion

        }

        #endregion
    }
}
