using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace ImpulseMaker
{
    internal class MyListBox : Control
    {
        public event EventHandler MoveSelectedUp;
        public event EventHandler MoveSelectedDown;
        public event EventHandler SelectionChanged;
        public event EventHandler ItemDeleted;
        public event CustomEventHandler ItemRenamed;
        public delegate void CustomEventHandler(string old_name, string new_name);

        public List<string> Items = new List<string>();
        SizeF text_size = new SizeF();
        int item_to_move = -1;
        ContextMenuStrip MyListBoxContextMenuStrip = new ContextMenuStrip();

        public int selected_item
        {
            get { return selected_item_val; }
            set
            {
                if (selected_item_val != value &&
                    value < this.Items.Count)
                {
                    selected_item_val = value;
                    if (SelectionChanged != null)
                        SelectionChanged(this, null);
                    Invalidate();
                }
                if (value >= this.Items.Count &&
                    selected_item_val != -1)
                {
                    selected_item_val = -1;
                    Invalidate();
                    if (SelectionChanged != null)
                        SelectionChanged(this, null);
                }
                    
            }
        }
        int selected_item_val = -1;

        int item_to_highlight
        {
            get { return item_to_highlight_val; }
            set
            {
                if (item_to_highlight_val != value)
                {
                    item_to_highlight_val = value;
                    Invalidate();
                }
            }
        }
        int item_to_highlight_val = -1;

        int highlightbutton
        {
            get { return highlightbutton_val; }
            set
            {
                if (highlightbutton_val != value)
                {
                    highlightbutton_val = value;
                    Invalidate();
                }
            }
        }
        int highlightbutton_val = -1;

        public MyListBox()
        {
            //avoid flickering
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            this.MouseMove += new MouseEventHandler(MyListBox_MouseMove);
            this.MouseLeave += new EventHandler(MyListBox_MouseLeave);
            this.MouseClick += new MouseEventHandler(MyListBox_MouseClick);
            this.MouseEnter += new EventHandler(MyListBox_MouseEnter);
            this.Paint += new PaintEventHandler(MyListBox_Paint);

            text_size = TextRenderer.MeasureText("0", this.Font);

            ToolStripMenuItem Rename = new ToolStripMenuItem("Переименовать");
            ToolStripMenuItem Delete = new ToolStripMenuItem("Удалить");

            MyListBoxContextMenuStrip.Items.AddRange(new[] { Rename, Delete });

            Rename.Click += new EventHandler(MyListBox_RenameItem);
            Delete.Click += new EventHandler(MyListBox_DeleteItem);
        }

        void MyListBox_RenameItem(object sender, EventArgs e)
        {
            NameSelectionDialog d = new NameSelectionDialog(this.Location.X - 100,
                this.Location.Y + (int)(selected_item * text_size.Height),
                this);
        }

        void MyListBox_DeleteItem(object sender, EventArgs e)
        {
            if (ItemDeleted != null)
                ItemDeleted(this, null);
        }

        void MyListBox_MouseEnter(object sender, EventArgs e)
        {
            //text_size = TextRenderer.MeasureText("0", this.Font);
        }

        void MyListBox_MouseLeave(object sender, EventArgs e)
        {
            item_to_highlight = -1;
        }

        void MyListBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (Items.Count <= 0)
                return;

            if (item_to_highlight >= 0)
            {
                if (!(e.X > this.Width - 16 && e.Y > text_size.Height * item_to_highlight &&
                    e.Y < text_size.Height * item_to_highlight + text_size.Height * 2))
                    item_to_highlight = (int)((float)e.Y / text_size.Height);

                if (e.X > this.Width - 14 && e.X < this.Width - 2 &&
                    e.Y > text_size.Height * item_to_highlight + 2 &&
                    e.Y < text_size.Height * item_to_highlight + 2 + text_size.Height - 3)
                    highlightbutton = 0;
                else if (e.X > this.Width - 14 && e.X < this.Width - 2 &&
                    e.Y > text_size.Height * item_to_highlight + text_size.Height + 1 &&
                    e.Y < text_size.Height * item_to_highlight + text_size.Height + 1 + text_size.Height - 3)
                    highlightbutton = 1;
                else highlightbutton = -1;
            }
            else
            {
                item_to_highlight = (int)((float)e.Y / text_size.Height);
            }
        }

        void MyListBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (item_to_highlight >= 0)
            {
                if (e.X > this.Width - 14 &&
                    e.X < this.Width - 14 + 12 &&
                    e.Y > text_size.Height * item_to_highlight + 2 &&
                    e.Y < text_size.Height * item_to_highlight + text_size.Height - 3)
                {
                    if (item_to_highlight > 0)
                    {
                        Swap(item_to_highlight, item_to_highlight - 1);

                        item_to_move = item_to_highlight;
                        if (MoveSelectedUp != null)
                            MoveSelectedUp(this, null);
                    }
                }
                else if (e.X > this.Width - 14 &&
                    e.X < this.Width - 14 + 12 &&
                    e.Y > text_size.Height * item_to_highlight + text_size.Height + 1 &&
                    e.Y < text_size.Height * item_to_highlight + text_size.Height + 1 + text_size.Height - 3)
                {
                    if (item_to_highlight < this.Items.Count - 1)
                    {
                        Swap(item_to_highlight, item_to_highlight + 1);

                        item_to_move = item_to_highlight;
                        if (MoveSelectedDown != null)
                            MoveSelectedDown(this, null);
                    }
                }
                else if (item_to_highlight != selected_item)
                    selected_item = item_to_highlight;
            }
            else if(selected_item != -1)
                selected_item = -1;

            /* Был ли клик правой кнопкой мыши по подсвечиваемому элементу чтобы вызвать контекствное меню */
            if (item_to_highlight >= 0 &&
                e.Button == MouseButtons.Right)
            {
                MyListBoxContextMenuStrip.Show(this, e.X, e.Y);
            }
        }

        public void MyListBox_Paint(object sender, PaintEventArgs e)
        {
            Rectangle options = new Rectangle(0, 0, this.Width, this.Height);
            GraphicsPath path = RoundedRectangle.Create(options, 5);
            e.Graphics.FillPath(Brushes.White, path);

            for (int i = 0; i < this.Items.Count; ++i)
            {
                e.Graphics.DrawString(i.ToString(), this.Font, Brushes.Black, new PointF(0, text_size.Height * i));
                e.Graphics.DrawString(Items[i], this.Font, Brushes.Black, new PointF(20, text_size.Height * i));
            }

            if (item_to_highlight >= 0 &&
                item_to_highlight < this.Items.Count)
            {
                SizeF size = TextRenderer.MeasureText(this.Items[item_to_highlight].ToString(), this.Font);

                int x_beg = this.Width - 16,
                    y_beg = (int)(size.Height * item_to_highlight + size.Height / 2),
                    x_end = (int)size.Width + 20,
                    y_end = (int)(size.Height * item_to_highlight + size.Height / 2);

                if (x_beg == x_end) x_end--;

                LinearGradientBrush myVerticalGradient =
                    new LinearGradientBrush(new Point(x_beg, y_beg),
                    new Point(x_end, y_end), Color.LightGray, this.BackColor);
                e.Graphics.FillRectangle(myVerticalGradient,
                    size.Width + 21, size.Height * item_to_highlight, this.Width - 16 - size.Width, size.Height);

                e.Graphics.DrawRectangle(Pens.LightGray, 0, size.Height * item_to_highlight, this.Width, size.Height);
                e.Graphics.FillRectangle(Brushes.LightGray, this.Width - 16, size.Height * item_to_highlight, 16, size.Height * 2);

                e.Graphics.FillRectangle(highlightbutton == 0 ? Brushes.LightBlue : Brushes.WhiteSmoke,
                    this.Width - 14, size.Height * item_to_highlight + 2, 12, size.Height - 3);
                e.Graphics.FillRectangle(highlightbutton == 1 ? Brushes.LightBlue : Brushes.WhiteSmoke,
                    this.Width - 14, size.Height * item_to_highlight + size.Height + 1, 12, size.Height - 3);

                e.Graphics.FillPolygon(Brushes.Black, new Point[] {
                new Point(this.Width - 12, (int)(size.Height * item_to_highlight + size.Height - 2)),
                new Point(this.Width - 5, (int)(size.Height * item_to_highlight + size.Height - 2)),
                new Point(this.Width - 8, (int)(size.Height * item_to_highlight + size.Height - 6)) });
                e.Graphics.FillPolygon(Brushes.Black, new Point[] {
                new Point(this.Width - 11, (int)(size.Height * item_to_highlight + size.Height + 2)),
                new Point(this.Width - 5, (int)(size.Height * item_to_highlight + size.Height + 2)),
                new Point(this.Width - 9, (int)(size.Height * item_to_highlight + size.Height + 5)) });
            }

            if (selected_item >= 0)
            {
                options = Rectangle.Round(new RectangleF(0, text_size.Height * selected_item, this.Width, text_size.Height));
                path = RoundedRectangle.Create(options, 5);
                e.Graphics.FillPath(new SolidBrush(Color.FromArgb(25, Color.Red)), path);
            }
        }

        public int FindByName(string name)
        {
            int i = 0;
            foreach (var item in Items)
            {
                if (item.ToString() == name)
                    return i;

                i++;
            }  

            return 0;
        }

        public void Swap(int indexA, int indexB)
        {
            var tmp = this.Items[indexA];
            this.Items[indexA] = this.Items[indexB];
            this.Items[indexB] = tmp;

            Invalidate();
        }

        public void clear_selection()
        {
            selected_item = -1;
        }

        public int get_item_to_move()
        {
            return item_to_move;
        }

        class NameSelectionDialog : Control
        {
            MyListBox _parent;
            System.Windows.Forms.TextBox input_box = new System.Windows.Forms.TextBox() { Left = 0, Top = 0, Width = 100 };
            System.Windows.Forms.Button ok_button = new System.Windows.Forms.Button() { Text = "Ok", Left = 0, Top = 20, Width = 50 };
            System.Windows.Forms.Button cancel_button = new System.Windows.Forms.Button() { Text = "Cancel", Left = 50, Top = 20, Width = 50 };

            public NameSelectionDialog(int X, int Y, MyListBox parent)
            {
                parent.Parent.Controls.Add(this);
                _parent = parent;

                this.Width = 100;
                this.Height = 42;
                this.Location = new Point(X, Y);

                this.Controls.AddRange(new Control[] { input_box, ok_button, cancel_button });

                ok_button.Click += new EventHandler(NameSelectionDialog_Ok);
                cancel_button.Click += new EventHandler(NameSelectionDialog_Cancel);
                parent.SelectionChanged += new EventHandler(NameSelectionDialog_Close);

                input_box.Text = parent.Items[parent.selected_item];
                this.BringToFront();
            }

            void NameSelectionDialog_Ok(object sender, EventArgs e)
            {
                string old_name = _parent.Items[_parent.selected_item],
                    new_name = input_box.Text;
                _parent.Items[_parent.selected_item] = input_box.Text;
                this.Dispose();
                if (_parent.ItemRenamed != null)
                    _parent.ItemRenamed(old_name, new_name);
                _parent.Invalidate();
            }

            void NameSelectionDialog_Cancel(object sender, EventArgs e)
            {
                this.Dispose();
            }

            void NameSelectionDialog_Close(object sender, EventArgs e)
            {
                this.Dispose();
            }
        }
    }
}
