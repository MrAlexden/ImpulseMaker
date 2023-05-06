using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImpulseMaker
{
    internal class MyNumericUpDown : NumericUpDown
    {
        public event EventHandler ValueChangedBeforeLeave;
        bool is_value_changed_manually = false,
            is_mouse_in = false;

        public MyNumericUpDown()
        {
            this.KeyPress += new KeyPressEventHandler(MyNumericUpDown_KeyPress);
            this.ValueChanged += new EventHandler(MyNumericUpDown_ValueChanged);
            this.MouseEnter += new EventHandler(MyNumericUpDown_MouseEnter);
            this.MouseLeave += new EventHandler(MyNumericUpDown_MouseLeave);
            this.GotFocus += new EventHandler(MyNumericUpDown_GotFocus);
            this.LostFocus += new EventHandler(MyNumericUpDown_LostFocus);
            this.KeyDown += new KeyEventHandler(MyNumericUpDown_KeyDown);
        }

        void MyNumericUpDown_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (ValueChangedBeforeLeave != null)
                    ValueChangedBeforeLeave(this, null);
            }
        }

        void MyNumericUpDown_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar.Equals('.') || e.KeyChar.Equals(','))
            {
                e.KeyChar = ((System.Globalization.CultureInfo)System.Globalization.CultureInfo.CurrentCulture).NumberFormat.NumberDecimalSeparator.ToCharArray()[0];
            }
        }

        void MyNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            is_value_changed_manually = is_mouse_in;
        }

        void MyNumericUpDown_MouseEnter(object sender, EventArgs e)
        {
            is_value_changed_manually = false;
            is_mouse_in = true;
        }

        void MyNumericUpDown_MouseLeave(object sender, EventArgs e)
        {
            if (is_value_changed_manually && ValueChangedBeforeLeave != null)
                ValueChangedBeforeLeave(this, null);

            is_value_changed_manually = false;
            is_mouse_in = false;
        }

        void MyNumericUpDown_GotFocus(object sender, EventArgs e)
        {
            is_mouse_in = true;
        }

        void MyNumericUpDown_LostFocus(object sender, EventArgs e)
        {
            is_mouse_in = false;
        }
    }
}
