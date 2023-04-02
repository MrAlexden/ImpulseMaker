using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ImpulseMaker
{
    internal class MyListBox : ListBox
    {
        public event EventHandler MoveSelectedUp;
        public event EventHandler MoveSelectedDown;
        //public event EventHandler HighlightChanged;

        public int item_to_highlight
        {
            get { return item_to_highlight_val; }
            set
            {
                if (item_to_highlight_val != value)
                {
                    item_to_highlight_val = value;
                    //if (HighlightChanged != null)
                    //    HighlightChanged(this, null);
                }
            }
        }
        private int item_to_highlight_val = -1;

        private int highlightbutton = -1;

        public MyListBox()
        {
            //avoid flickering
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            this.MouseMove += new MouseEventHandler(MyListBox_MouseMove);
            this.MouseLeave += new EventHandler(MyListBox_MouseLeave);
            this.MouseClick += new MouseEventHandler(MyListBox_MouseClick);
            //this.HighlightChanged += new EventHandler(MyListBox_HighlightChanged);
        }

        //void MyListBox_HighlightChanged(object sender, EventArgs e)
        //{
        //    MyListBox_Paint(this, new PaintEventArgs(this.CreateGraphics(), this.DisplayRectangle));
        //}

        void MyListBox_MouseLeave(object sender, EventArgs e)
        {
            Invalidate();
        }

        void MyListBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (Items.Count <= 0)
                return;

            int ith = item_to_highlight;

            SizeF size = TextRenderer.MeasureText("0", this.Font);

            if (item_to_highlight >= 0)
            {
                if (!(e.X > this.Width - 20 && e.Y > size.Height * item_to_highlight &&
                    e.Y < size.Height * item_to_highlight + size.Height * 2))
                    item_to_highlight = (int)((float)e.Y / size.Height);

                highlightbutton = -1;

                if (e.X > this.Width - 18 && e.X < this.Width - 6 &&
                    e.Y > size.Height * item_to_highlight + 2 &&
                    e.Y < size.Height * item_to_highlight + 2 + size.Height - 3)
                    highlightbutton = 0;

                if (e.X > this.Width - 18 && e.X < this.Width - 6 &&
                    e.Y > size.Height * item_to_highlight + size.Height + 1 &&
                    e.Y < size.Height * item_to_highlight + size.Height + 1 + size.Height - 3)
                    highlightbutton = 1;
            }
            else
            {
                item_to_highlight = (int)((float)e.Y / size.Height);
            }

            if (item_to_highlight != ith)
                Invalidate();

            MyListBox_Paint(this, new PaintEventArgs(this.CreateGraphics(), this.DisplayRectangle));
        }

        void MyListBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (item_to_highlight >= 0)
            {
                SizeF size = TextRenderer.MeasureText("0", this.Font);

                if (e.X > this.Width - 18 &&
                    e.X < this.Width - 18 + 12 &&
                    e.Y > size.Height * item_to_highlight + 2 &&
                    e.Y < size.Height * item_to_highlight + size.Height - 3)
                {
                    if (item_to_highlight > 0)
                    {
                        Swap(this.SelectedIndex, this.SelectedIndex - 1);

                        if (MoveSelectedUp != null)
                            MoveSelectedUp(this, null);
                    }   
                }

                if (e.X > this.Width - 18 &&
                    e.X < this.Width - 18 + 12 &&
                    e.Y > size.Height * item_to_highlight + size.Height + 1 &&
                    e.Y < size.Height * item_to_highlight + size.Height + 1 + size.Height - 3)
                {
                    if (item_to_highlight < this.Items.Count - 1)
                    {
                        Swap(this.SelectedIndex, this.SelectedIndex - 1);

                        if (MoveSelectedDown != null)
                            MoveSelectedDown(this, null);
                    }
                }
            }
        }

        void MyListBox_Paint(object sender, PaintEventArgs e)
        {
            if (item_to_highlight < 0 ||
                item_to_highlight >= Items.Count)
                return;

            SizeF size = TextRenderer.MeasureText(this.Items[item_to_highlight].ToString(), this.Font);

            LinearGradientBrush myVerticalGradient =
                new LinearGradientBrush(new Point(this.Width - 20, (int)(size.Height * item_to_highlight + size.Height / 2)),
                new Point((int)size.Width, (int)(size.Height * item_to_highlight + size.Height / 2)), Color.LightGray, this.BackColor);
            e.Graphics.FillRectangle(myVerticalGradient,
                size.Width + 1, size.Height * item_to_highlight, this.Width - 21 - size.Width, size.Height);

            e.Graphics.DrawRectangle(Pens.LightGray, 0, size.Height * item_to_highlight, this.Width - 20, size.Height);
            e.Graphics.FillRectangle(Brushes.LightGray, this.Width - 20, size.Height * item_to_highlight, 16, size.Height * 2);

            e.Graphics.FillRectangle(highlightbutton == 0 ? Brushes.LightBlue : Brushes.WhiteSmoke, 
                this.Width - 18, size.Height * item_to_highlight + 2, 12, size.Height - 3);
            e.Graphics.FillRectangle(highlightbutton == 1 ? Brushes.LightBlue : Brushes.WhiteSmoke,
                this.Width - 18, size.Height * item_to_highlight + size.Height + 1, 12, size.Height - 3);

            e.Graphics.FillPolygon(Brushes.Black, new Point[] {
                new Point(this.Width - 16, (int)(size.Height * item_to_highlight + size.Height - 2)),
                new Point(this.Width - 9, (int)(size.Height * item_to_highlight + size.Height - 2)),
                new Point(this.Width - 12, (int)(size.Height * item_to_highlight + size.Height - 6)) });
            e.Graphics.FillPolygon(Brushes.Black, new Point[] {
                new Point(this.Width - 15, (int)(size.Height * item_to_highlight + size.Height + 2)),
                new Point(this.Width - 9, (int)(size.Height * item_to_highlight + size.Height + 2)),
                new Point(this.Width - 13, (int)(size.Height * item_to_highlight + size.Height + 5)) });
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
    }
}
