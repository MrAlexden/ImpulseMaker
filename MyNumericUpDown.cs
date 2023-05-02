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
        bool is_value_changed = false;

        public MyNumericUpDown()
        {
            this.KeyPress += new KeyPressEventHandler(MyNumericUpDown_KeyPress);
            this.ValueChanged += new EventHandler(MyNumericUpDown_ValueChanged);
            this.MouseEnter += new EventHandler(MyNumericUpDown_MouseEnter);
            this.MouseLeave += new EventHandler(MyNumericUpDown_MouseLeave);
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
            is_value_changed = true;
        }

        void MyNumericUpDown_MouseEnter(object sender, EventArgs e)
        {
            is_value_changed = false;
        }

        void MyNumericUpDown_MouseLeave(object sender, EventArgs e)
        {
            if (is_value_changed && ValueChangedBeforeLeave != null)
                ValueChangedBeforeLeave(this, null);

            is_value_changed = false;
        }
    }
}
