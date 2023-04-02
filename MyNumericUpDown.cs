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
        public MyNumericUpDown()
        {
            this.KeyPress += new KeyPressEventHandler(MyNumericUpDown_KeyPress);
        }

        void MyNumericUpDown_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar.Equals('.') || e.KeyChar.Equals(','))
            {
                e.KeyChar = ((System.Globalization.CultureInfo)System.Globalization.CultureInfo.CurrentCulture).NumberFormat.NumberDecimalSeparator.ToCharArray()[0];
            }
        }
    }
}
