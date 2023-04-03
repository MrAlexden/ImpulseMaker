using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Globalization;
using System.IO;
using CsvHelper.Configuration;
using System.Runtime.Remoting.Channels;
using static System.Windows.Forms.LinkLabel;
using System.Xml.Linq;
using System.Threading;
using Microsoft.Win32;

namespace ImpulseMaker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private class channel
        {
            public string name;
            public double[] data;
        }

        static string ini_file_name = "settings.ini",
            csv_file_name = "scenario.csv";
        string ini_path = ini_file_name,
            csv_path = csv_file_name;
        private List<channel> Channels = new List<channel>();
        char csv_delimiter = ';';
        bool is_first_col_sr = true,
            is_csv_empty_on_init = false;
        Random RandGen = new Random();
        NumericUpDown nud_Min = new NumericUpDown();
        NumericUpDown nud_Max = new NumericUpDown();

        private void Form1_Load(object sender, EventArgs e)
        {
            nud_Min.Hide();
            nud_Max.Hide();

            RegistryKey key = Registry.CurrentUser.OpenSubKey("SignalMaker");

            if (key != null)
            {
                ini_path = (string)key.GetValue("INI", ini_file_name);
                csv_path = (string)key.GetValue("CSV", csv_file_name);

                if (ini_path.First() == '\\')
                    ini_path = ini_path.Substring(1, ini_path.Length - 1);
                if (csv_path.First() == '\\')
                    csv_path = csv_path.Substring(1, csv_path.Length - 1);
            }

            this.iNIFilePathToolStripMenuItem.ToolTipText = "Current INI file path: " + ini_path;
            this.cSVFilePathToolStripMenuItem.ToolTipText = "Current CSV file path: " + csv_path;

            HookupMouseEnterEvents(this);

            IniFile MyIni = new IniFile(ini_path);
            if (MyIni.KeyExists("SignalDuration", "Main") &&
                MyIni.KeyExists("SamplingRate", "Main"))

            {
                SignalDurationValue.Value = Convert.ToDecimal(MyIni.Read("SignalDuration", "Main"));
                SamplingRateValue.Value = Convert.ToDecimal(MyIni.Read("SamplingRate", "Main"));
            }

            init_frame(sender, e);
        }

        #region SuppFuncs

        private void init_frame(object sender, EventArgs e)
        {
            read_csv_file();

            ChannelsListBox_fill();

            if (ChannelsListBox.Items.Count <= 0)
            {
                RampChannelName.Text = "NewChannel_0";
                ImpulseChannelName.Text = "NewChannel_0";

                double[] data = new double[0];
                save_csv_channel(RampChannelName.Text, ref data);

                WholeSignalChart.Series.Clear();
                if (WholeSignalChart.Series.IsUniqueName(RampChannelName.Text))
                    WholeSignalChart.Series.Add(RampChannelName.Text);
                ChannelsListBox.SelectedIndex = 0;

                RampTabPage_Enter(sender, e);

                is_csv_empty_on_init = true;
            }
            else
            {
                RampChannelName.Text = "NewChannel_" + Channels.Count.ToString();
                ImpulseChannelName.Text = "NewChannel_" + Channels.Count.ToString();

                redraw_whole_chart();

                ChannelsListBox.SelectedIndex = 0;
            }

            RampPeakTrackerLabel.Text = ((double)RampPeakTrackBar.Value / 100 * (double)RampPeriodValue.Value).ToString();

            SeveralSlidersImpulseTrackBar.Max = (float)ImpulsePeriodValue.Value;
        }

        public void Swap<T>(ref List<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
        }

        private void HookupMouseEnterEvents(Control control)
        {
            foreach (Control childControl in control.Controls)
            {
                childControl.MouseMove += new MouseEventHandler(OnMouseMove);
                childControl.MouseClick += new MouseEventHandler(OnMouseClick);

                // Recurse on this child to get all of its descendents.
                HookupMouseEnterEvents(childControl);
            }
        }

        private void SignalDuration_ValueChanged(object sender, EventArgs e)
        {
            if (ChannelsListBox.SelectedIndex < 0 || ChannelsListBox.Items.Count <= 0)
                return;

            redraw_whole_chart();

            int si = ChannelsListBox.SelectedIndex;
            ChannelsListBox.ClearSelected();
            ChannelsListBox.SelectedIndex = si;

            IniFile MyIni = new IniFile(ini_path);
            switch (SignalTypeTabControl.SelectedIndex
                /*Convert.ToInt32(MyIni.Read("Type", Channels[ChannelsListBox.SelectedIndex].name))*/)
            {
                case 0:
                    ramp_graph(sender, e);
                    break;
                case 1:
                    impulse_graph(sender, e);
                    break;
                case 2:
                    break;
                default:
                    break;
            }
        }

        private void redraw_whole_chart()
        {
            IniFile MyIni = new IniFile(ini_path);

            WholeSignalChart.Series.Clear();
            foreach (var ch in Channels)
            {
                if (!MyIni.KeyExists("Type", ch.name))
                    continue;

                if (WholeSignalChart.Series.IsUniqueName(ch.name))
                    WholeSignalChart.Series.Add(ch.name);
                WholeSignalChart.Series.Last().Color =
                    Color.FromArgb(RandGen.Next(40, 210), RandGen.Next(60, 245), RandGen.Next(50, 220));

                switch (Convert.ToInt32(MyIni.Read("Type", ch.name)))
                {
                    case 0:

                        RampBeginValue.Value = Convert.ToDecimal(MyIni.Read("BeginValue", ch.name));
                        RampPeakValue.Value = Convert.ToDecimal(MyIni.Read("PeakValue", ch.name));
                        RampPeriodValue.Value = Convert.ToDecimal(MyIni.Read("Period", ch.name));
                        RampPeakTrackerLabel.Text = MyIni.Read("PeakPos", ch.name);
                        RampPeakTrackBar.Value = (int)(Convert.ToDecimal(RampPeakTrackerLabel.Text) /
                            Convert.ToDecimal(RampPeriodValue.Value) * 100);
                        ZeroEndingRampCheckBox.Checked = Convert.ToInt32(MyIni.Read("IsZeroEnding", ch.name)) == 1;

                        redraw_ramp_wholegraph(ch.name);
                        break;
                    case 1:

                        ImpulseBaseValue.Value = Convert.ToDecimal(MyIni.Read("BaseValue", ch.name));
                        ImpulseLevelValue.Value = Convert.ToDecimal(MyIni.Read("LevelValue", ch.name));
                        ImpulsePeriodValue.Value = Convert.ToDecimal(MyIni.Read("Period", ch.name));
                        SeveralSlidersImpulseTrackBar.Min = (float)Convert.ToDecimal(MyIni.Read("Min", ch.name));
                        SeveralSlidersImpulseTrackBar.Max = (float)Convert.ToDecimal(MyIni.Read("Max", ch.name));
                        SeveralSlidersImpulseTrackBar.SelectedMin = (float)Convert.ToDouble(MyIni.Read("LeftPos", ch.name));
                        SeveralSlidersImpulseTrackBar.SelectedMax = (float)Convert.ToDouble(MyIni.Read("RightPos", ch.name));
                        ZeroEndingImpulseCheckBox.Checked = Convert.ToInt32(MyIni.Read("IsZeroEnding", ch.name)) == 1;

                        redraw_impulse_wholegraph(ch.name);
                        break;
                    case 2:
                        break;
                    default:
                        return;
                }
            }
        }

        #endregion

        #region CSV

        private void add_csv_channel(string name, ref double[] data)
        {
            Channels.Add(new channel { name = name, data = data });
            ChannelsListBox_fill();
            redraw_whole_chart();
        }

        private void delete_csv_channel(string name)
        {
            foreach (var ch in Channels)
                if (ch.name == name)
                {
                    Channels.Remove(ch);
                    ChannelsListBox_fill();
                    redraw_whole_chart();
                    return;
                }
        }

        private void save_csv_channel(string name, ref double[] data)
        {
            if (is_csv_empty_on_init)
            {
                Channels.Clear();
                Channels.Add(new channel { name = name, data = data });
                ChannelsListBox_fill();
                redraw_whole_chart();
                is_csv_empty_on_init = false;
                return;
            }

            foreach (var ch in Channels)
                if (ch.name == name)
                {
                    ch.data = data;
                    return;
                }

            add_csv_channel(name, ref data);
        }

        private void write_csv_file()
        {
            using (StreamWriter sw = File.CreateText(csv_path))
            {
                int max_size = 0;
                string line = is_first_col_sr ? SamplingRateValue.Value.ToString() + ";" : "";
                for (int j = 0; j < Channels.Count; ++j)
                {
                    line += Channels[j].name + csv_delimiter.ToString();
                    if (Channels[j].data.Length > max_size)
                        max_size = Channels[j].data.Length;
                }
                sw.WriteLine(line.Trim(csv_delimiter));

                for (int i = 0; i < max_size; ++i)
                {
                    line = is_first_col_sr ? SamplingRateValue.Value.ToString() + ";" : "";
                    for (int j = 0; j < Channels.Count; ++j)
                        line += ((i < Channels[j].data.Length) ? Channels[j].data[i].ToString() : "")
                            + csv_delimiter.ToString();
                    sw.WriteLine(line.Trim(csv_delimiter));
                }
            }
        }

        private void read_csv_file()
        {
            Channels.Clear();

            if (!File.Exists(csv_path))
            {
                //MessageBox.Show("Файл \"" + csv_path + "\" не найден");
                return;
            }  

            string[] lines = File.ReadAllLines(csv_path);
            if (lines.Length <= 1)
                return;

            for (int i = 0,
                num_channels = lines[0].Split(csv_delimiter).Count() - (is_first_col_sr ? 1 : 0);
                i < num_channels; ++i)
            {
                Channels.Add(new channel());
                Channels[i].name = lines[0].Split(csv_delimiter)[i + (is_first_col_sr ? 1 : 0)];
                Channels[i].data = new double[lines.Length - 1];
            }

            for (int i = 1; i < lines.Length; ++i)
            {
                for (int j = 0; j < Channels.Count; ++j)
                {
                    try
                    {
                        Channels[j].data[i - 1] = Convert.ToDouble(lines[i].Split(csv_delimiter)[j + (is_first_col_sr ? 1 : 0)]);
                    }
                    catch
                    {
                        return;
                    }
                }
            }
        }

        #endregion

        #region INI
        
        private void write_ini_channel(string name)
        {
            IniFile MyIni = new IniFile(ini_path);

            MyIni.DeleteSection(name);

            MyIni.Write("SignalDuration", SignalDurationValue.Value.ToString(), "Main");
            MyIni.Write("SamplingRate", SamplingRateValue.Value.ToString(), "Main");

            switch (SignalTypeTabControl.SelectedIndex)
            {
                case 0:
                    MyIni.Write("IsCustom", "0", name);
                    MyIni.Write("Type", SignalTypeTabControl.SelectedIndex.ToString(), name);
                    MyIni.Write("BeginValue", RampBeginValue.Value.ToString(), name);
                    MyIni.Write("PeakValue", RampPeakValue.Value.ToString(), name);
                    MyIni.Write("Period", RampPeriodValue.Value.ToString(), name);
                    MyIni.Write("PeakPos", RampPeakTrackerLabel.Text, name);
                    MyIni.Write("IsZeroEnding", (ZeroEndingRampCheckBox.Checked) ? "1" : "0", name);
                    break;
                case 1:
                    MyIni.Write("IsCustom", "0", name);
                    MyIni.Write("Type", SignalTypeTabControl.SelectedIndex.ToString(), name);
                    MyIni.Write("BaseValue", ImpulseBaseValue.Value.ToString(), name);
                    MyIni.Write("LevelValue", ImpulseLevelValue.Value.ToString(), name);
                    MyIni.Write("Period", ImpulsePeriodValue.Value.ToString(), name);
                    MyIni.Write("Min", SeveralSlidersImpulseTrackBar.Min.ToString(), name);
                    MyIni.Write("Max", SeveralSlidersImpulseTrackBar.Max.ToString(), name);
                    MyIni.Write("LeftPos", SeveralSlidersImpulseTrackBar.SelectedMin.ToString(), name);
                    MyIni.Write("RightPos", SeveralSlidersImpulseTrackBar.SelectedMax.ToString(), name);
                    MyIni.Write("IsZeroEnding", (ZeroEndingImpulseCheckBox.Checked) ? "1" : "0", name);
                    break;
                case 2:
                    break;
                default:
                    return;
            }
        }

        private void read_ini_channel(string name)
        {
            IniFile MyIni = new IniFile(ini_path);

            if (!MyIni.KeyExists("Type", name))
            {
                //MessageBox.Show("Ключ \"" + name + "\" в файле \"" + ini_path +  "\" не найден");
                return;
            }

            //SignalDurationValue.Value = Convert.ToDecimal(MyIni.Read("SignalDuration", "Main"));
            //SamplingRateValue.Value = Convert.ToDecimal(MyIni.Read("SamplingRate", "Main"));

            switch (Convert.ToInt32(MyIni.Read("Type", name)))
            {
                case 0:
                    
                    RampBeginValue.Value = Convert.ToDecimal(MyIni.Read("BeginValue", name));
                    RampPeakValue.Value = Convert.ToDecimal(MyIni.Read("PeakValue", name));
                    RampPeriodValue.Value = Convert.ToDecimal(MyIni.Read("Period", name));
                    RampPeakTrackerLabel.Text = MyIni.Read("PeakPos", name);
                    RampPeakTrackBar.Value = (int)(Convert.ToDecimal(RampPeakTrackerLabel.Text) /
                        Convert.ToDecimal(RampPeriodValue.Value) * 100);
                    ZeroEndingRampCheckBox.Checked = Convert.ToInt32(MyIni.Read("IsZeroEnding", name)) == 1;
                    
                    SignalTypeTabControl.SelectedIndex = 0;

                    redraw_ramp();
                    redraw_ramp_wholegraph();

                    break;
                case 1:
                    
                    ImpulseBaseValue.Value = Convert.ToDecimal(MyIni.Read("BaseValue", name));
                    ImpulseLevelValue.Value = Convert.ToDecimal(MyIni.Read("LevelValue", name));
                    ImpulsePeriodValue.Value = Convert.ToDecimal(MyIni.Read("Period", name));
                    SeveralSlidersImpulseTrackBar.Min = (float)Convert.ToDecimal(MyIni.Read("Min", name));
                    SeveralSlidersImpulseTrackBar.Max = (float)Convert.ToDecimal(MyIni.Read("Max", name));
                    SeveralSlidersImpulseTrackBar.SelectedMin = (float)Convert.ToDecimal(MyIni.Read("LeftPos", name));
                    SeveralSlidersImpulseTrackBar.SelectedMax = (float)Convert.ToDecimal(MyIni.Read("RightPos", name));
                    ZeroEndingImpulseCheckBox.Checked = Convert.ToInt32(MyIni.Read("IsZeroEnding", name)) == 1;
                    
                    SignalTypeTabControl.SelectedIndex = 1;

                    redraw_impulse();
                    redraw_impulse_wholegraph();

                    break;
                case 2:
                    break;
                default:
                    return;
            }
        }

        #endregion

        #region TabPage_Enter

        private void RampTabPage_Enter(object sender, EventArgs e)
        {
            SignalDurationValue.Minimum = RampPeriodValue.Value;

            ramp_graph(sender, e);
        }

        private void ImpulseTabPage_Enter(object sender, EventArgs e)
        {
            SignalDurationValue.Minimum = ImpulsePeriodValue.Value;

            impulse_graph(sender, e);

            this.Controls.Add(nud_Min);
            nud_Min.BringToFront();

            nud_Min.Minimum = (decimal)SeveralSlidersImpulseTrackBar.Min;
            nud_Min.Maximum = (decimal)SeveralSlidersImpulseTrackBar.SelectedMax;
            
            this.Controls.Add(nud_Max);
            nud_Max.BringToFront();

            nud_Max.Minimum = (decimal)SeveralSlidersImpulseTrackBar.SelectedMin;
            nud_Max.Maximum = (decimal)SeveralSlidersImpulseTrackBar.Max;

            nud_Min.Value = (decimal)SeveralSlidersImpulseTrackBar.SelectedMin;
            nud_Max.Value = (decimal)SeveralSlidersImpulseTrackBar.SelectedMax;

            nud_Min.ValueChanged += new EventHandler(nud_Min_ValueChange);
            nud_Max.ValueChanged += new EventHandler(nud_Max_ValueChange);
        }

        private void SineTabPage_Enter(object sender, EventArgs e)
        {
            OneSegmentChart.Series[0].Points.Clear();
            OneSegmentChart.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;

            double[] xarr = new double[100];
            double[] yarr = new double[100];
            int i = 1;

            for (xarr[0] = 0.0, yarr[0] = Math.Sin(xarr[0]); i < 100; yarr[i] = Math.Sin(xarr[i]), ++i)
                xarr[i] = xarr[i - 1] + 0.01;
            OneSegmentChart.Series[0].Points.DataBindXY(xarr, yarr);
        }

        #endregion

        #region RampTab

        private void RampPeakTrackBar_ValueChanged(object sender, EventArgs e)
        {
            RampPeakTrackerLabel.Text = ((double)RampPeakTrackBar.Value / 100 * (double)RampPeriodValue.Value).ToString();
            ramp_graph(sender, e);
        }

        private void RampPeriodValue_ValueChanged(object sender, EventArgs e)
        {
            SignalDurationValue.Minimum = RampPeriodValue.Value;
            RampPeakTrackerLabel.Text = ((double)RampPeakTrackBar.Value / 100 * (double)RampPeriodValue.Value).ToString();
            ramp_graph(sender, e);
        }

        private void ramp_graph(object sender, EventArgs e)
        {
            redraw_ramp();
            redraw_ramp_wholegraph();
        }

        private void redraw_ramp()
        {
            OneSegmentChart.Series[0].Points.Clear();
            OneSegmentChart.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;

            double[] xarr = { 0, (double)RampPeriodValue.Value * (double)RampPeakTrackBar.Value / 100, (double)RampPeriodValue.Value };
            double[] yarr = { ((double)RampPeakTrackBar.Value / 100 <= 0) ? (double)RampPeakValue.Value : (double)RampBeginValue.Value,
                            ((double)RampPeakTrackBar.Value / 100 >= 0) ? (double)RampPeakValue.Value : (double)RampBeginValue.Value,
                            ((double)RampPeakTrackBar.Value / 100 * (double)RampPeriodValue.Value >= (double)RampPeriodValue.Value) ?
                            (double)RampPeakValue.Value : (double)RampBeginValue.Value };

            OneSegmentChart.Series[0].Points.DataBindXY(xarr, yarr);

            OneSegmentChart.ChartAreas[0].AxisX.Minimum = xarr.First();
            OneSegmentChart.ChartAreas[0].AxisX.Maximum = xarr.Last();

            OneSegmentChart.ChartAreas[0].AxisX.Interval = (xarr.Last() - xarr.First()) / 10;
        }

        private void redraw_ramp_wholegraph(string name = "")
        {
            double segments = (double)SignalDurationValue.Value / (double)RampPeriodValue.Value;
            if (segments <= 0)
                return;

            int chart_points = (int)Math.Ceiling(segments) * 3 + 1, i;

            Series series;
            if (name == "")
            {
                if (ChannelsListBox.SelectedIndex < 0 || ChannelsListBox.Items.Count <= 0)
                    return;
                try
                {
                    series = ChannelsListBox.SelectedIndex > 0 ? WholeSignalChart.Series[ChannelsListBox.Items
                        [ChannelsListBox.SelectedIndex].ToString()] : WholeSignalChart.Series[0];
                }
                catch
                {
                    if (WholeSignalChart.Series.IsUniqueName(ChannelsListBox.Items
                        [ChannelsListBox.SelectedIndex].ToString()))
                        WholeSignalChart.Series.Add(ChannelsListBox.Items
                        [ChannelsListBox.SelectedIndex].ToString());
                    //WholeSignalChart.Series.Last().Color =
                    //    Color.FromArgb(RandGen.Next(40, 210), RandGen.Next(60, 245), RandGen.Next(50, 220));
                    series = WholeSignalChart.Series.Last();
                }
            }
            else series = WholeSignalChart.Series.FindByName(name);
            if (series == null)
                return;

            series.Points.Clear();
            series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;

            double[] xarr = new double[chart_points];
            double[] yarr = new double[chart_points];

            for (xarr[0] = 0,
                xarr[1] = (double)RampPeriodValue.Value * (double)RampPeakTrackBar.Value / 100,
                xarr[2] = (double)RampPeriodValue.Value, i = 3; i < chart_points - 1; ++i)
            {
                switch (i % 3)
                {
                    case 0:
                        xarr[i] = xarr[i - 3] + (double)RampPeriodValue.Value + 1 / (double)SamplingRateValue.Value;
                        break;
                    case 1:
                        xarr[i] = xarr[i - 3] + (double)RampPeriodValue.Value + 1 / (double)SamplingRateValue.Value;
                        break;
                    case 2:
                        xarr[i] = xarr[i - 3] + (double)RampPeriodValue.Value;
                        break;
                    default:
                        break;
                }
            }

            for (yarr[0] = ((double)RampPeakTrackBar.Value / 100 <= 0) ? (double)RampPeakValue.Value : (double)RampBeginValue.Value,
                yarr[1] = ((double)RampPeakTrackBar.Value / 100 >= 0) ? (double)RampPeakValue.Value : (double)RampBeginValue.Value,
                yarr[2] = ((double)RampPeakTrackBar.Value / 100 * (double)RampPeriodValue.Value >= (double)RampPeriodValue.Value) ?
                            (double)RampPeakValue.Value : (double)RampBeginValue.Value, i = 3; i < chart_points - 1; ++i)
            {
                switch (i % 3)
                {
                    case 0:
                        yarr[i] = ((double)RampPeakTrackBar.Value / 100 <= 0) ? (double)RampPeakValue.Value : (double)RampBeginValue.Value;
                        break;
                    case 1:
                        yarr[i] = ((double)RampPeakTrackBar.Value / 100 >= 0) ? (double)RampPeakValue.Value : (double)RampBeginValue.Value;
                        break;
                    case 2:
                        yarr[i] = ((double)RampPeakTrackBar.Value / 100 * (double)RampPeriodValue.Value >= (double)RampPeriodValue.Value) ?
                            (double)RampPeakValue.Value : (double)RampBeginValue.Value;
                        break;
                    default:
                        break;
                }
            }

            if (ZeroEndingImpulseCheckBox.Checked)
            {
                for (i = 0; i < chart_points; ++i)
                    if (xarr[i] > (double)SignalDurationValue.Value)
                    {
                        xarr[i] = (double)SignalDurationValue.Value - 2 / (double)SamplingRateValue.Value;
                        yarr[yarr.Length - 1] = 0;
                        break;
                    }
                if (i >= chart_points)
                {
                    xarr[xarr.Length - 1] = (double)SignalDurationValue.Value - 2 / (double)SamplingRateValue.Value;
                    yarr[yarr.Length - 1] = 0;
                }
            }
            else
            {
                xarr[xarr.Length - 1] = (double)SignalDurationValue.Value;
                yarr[yarr.Length - 1] = yarr[yarr.Length - 2];
            }

            for (i = xarr.Length - 1; i > 3; --i)
                if (xarr[i] == 0)
                    xarr[i] = (double)SignalDurationValue.Value;

            series.Points.DataBindXY(xarr, yarr);

            WholeSignalChart.ChartAreas[0].AxisX.Minimum = 0;
            WholeSignalChart.ChartAreas[0].AxisX.Maximum = (double)SignalDurationValue.Value;
            WholeSignalChart.ChartAreas[0].AxisY.Minimum = Double.NaN;
            WholeSignalChart.ChartAreas[0].AxisY.Maximum = Double.NaN;

            WholeSignalChart.ChartAreas[0].AxisX.Interval = (double)SignalDurationValue.Value / 10;
        }

        private void SaveRampChannelButton_Click(object sender, EventArgs e)
        {
            double[] data = new double[0];

            double[] controls = {(double)SignalDurationValue.Value,
                                (double)SamplingRateValue.Value,
                                (double)RampPeriodValue.Value,
                                (double)RampPeakTrackBar.Value,
                                (double)RampBeginValue.Value,
                                (double)RampPeakValue.Value,
                                ZeroEndingRampCheckBox.Checked ? 1 : 0};

            if (calculate_ramp_channel(ref data, ref controls) == 0)
                write_ini_channel(RampChannelName.Text);

            save_csv_channel(RampChannelName.Text, ref data);
            write_csv_file();

            ChannelsListBox.ClearSelected();
            ChannelsListBox.SelectedIndex = ChannelsListBox.FindByName(RampChannelName.Text);
        }

        private void ZeroEndingRampCheckBox_CheckedChange(object sender, EventArgs e)
        {
            ramp_graph(sender, e);
        }

        private void DeleteRampChannelButton_Click(object sender, EventArgs e)
        {
            IniFile MyIni = new IniFile(ini_path);

            MyIni.DeleteSection(RampChannelName.Text);
            delete_csv_channel(RampChannelName.Text);
            
            write_csv_file();
        }

        #endregion

        #region ImpulseTab

        private void impulse_graph(object sender, EventArgs e)
        {
            redraw_impulse();
            redraw_impulse_wholegraph();
        }

        private void redraw_impulse()
        {
            OneSegmentChart.Series[0].Points.Clear();
            OneSegmentChart.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;

            double[] xarr = { 0,
                (double)SeveralSlidersImpulseTrackBar.SelectedMin,
                (double)SeveralSlidersImpulseTrackBar.SelectedMin + 1 / (double)SamplingRateValue.Value,
                (double)SeveralSlidersImpulseTrackBar.SelectedMax,
                (double)SeveralSlidersImpulseTrackBar.SelectedMax + 1 / (double)SamplingRateValue.Value,
                (double)ImpulsePeriodValue.Value };
            double[] yarr = { ((double)SeveralSlidersImpulseTrackBar.SelectedMin <= 0) ? (double)ImpulseLevelValue.Value : (double)ImpulseBaseValue.Value,
                            ((double)SeveralSlidersImpulseTrackBar.SelectedMin <= 0) ? (double)ImpulseLevelValue.Value : (double)ImpulseBaseValue.Value,
                            (double)ImpulseLevelValue.Value,
                            (double)ImpulseLevelValue.Value,
                            ((double)SeveralSlidersImpulseTrackBar.SelectedMax >= (double)SeveralSlidersImpulseTrackBar.Max) ?
                            (double)ImpulseLevelValue.Value : (double)ImpulseBaseValue.Value,
                            ((double)SeveralSlidersImpulseTrackBar.SelectedMax >= (double)SeveralSlidersImpulseTrackBar.Max) ?
                            (double)ImpulseLevelValue.Value : (double)ImpulseBaseValue.Value };

            OneSegmentChart.Series[0].Points.DataBindXY(xarr, yarr);

            OneSegmentChart.ChartAreas[0].AxisX.Minimum = xarr.First();
            OneSegmentChart.ChartAreas[0].AxisX.Maximum = xarr.Last();

            OneSegmentChart.ChartAreas[0].AxisX.Interval = (xarr.Last() - xarr.First()) / 10;
        }

        private void redraw_impulse_wholegraph(string name = "")
        {
            double segments = (double)SignalDurationValue.Value / (double)ImpulsePeriodValue.Value;
            if (segments <= 0)
                return;

            int chart_points = (int)Math.Ceiling(segments) * 6 + 1, i;

            Series series;
            if (name == "")
            {
                if (ChannelsListBox.SelectedIndex < 0 || ChannelsListBox.Items.Count <= 0)
                    return;
                try
                {
                    series = ChannelsListBox.SelectedIndex > 0 ? WholeSignalChart.Series[ChannelsListBox.Items
                        [ChannelsListBox.SelectedIndex].ToString()] : WholeSignalChart.Series[0];
                }
                catch
                {
                    if (WholeSignalChart.Series.IsUniqueName(ChannelsListBox.Items
                        [ChannelsListBox.SelectedIndex].ToString()))
                        WholeSignalChart.Series.Add(ChannelsListBox.Items
                        [ChannelsListBox.SelectedIndex].ToString());
                    //WholeSignalChart.Series.Last().Color =
                    //    Color.FromArgb(RandGen.Next(40, 210), RandGen.Next(60, 245), RandGen.Next(50, 220));
                    series = WholeSignalChart.Series.Last();
                }
            }
            else series = WholeSignalChart.Series.FindByName(name);
            if (series == null)
                return;

            series.Points.Clear();
            series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;

            double[] xarr = new double[chart_points];
            double[] yarr = new double[chart_points];

            for (xarr[0] = 0,
                xarr[1] = (double)SeveralSlidersImpulseTrackBar.SelectedMin,
                xarr[2] = (double)SeveralSlidersImpulseTrackBar.SelectedMin + 1 / (double)SamplingRateValue.Value,
                xarr[3] = (double)SeveralSlidersImpulseTrackBar.SelectedMax,
                xarr[4] = (double)SeveralSlidersImpulseTrackBar.SelectedMax + 1 / (double)SamplingRateValue.Value,
                xarr[5] = (double)ImpulsePeriodValue.Value, i = 6; i < chart_points - 1; ++i)
            {
                switch (i % 6)
                {
                    case 0:
                        xarr[i] = xarr[i - 6] + (double)ImpulsePeriodValue.Value + 1 / (double)SamplingRateValue.Value;
                        break;
                    case 1:
                        xarr[i] = xarr[i - 6] + (double)ImpulsePeriodValue.Value + 1 / (double)SamplingRateValue.Value;
                        break;
                    case 2:
                        xarr[i] = xarr[i - 6] + (double)ImpulsePeriodValue.Value + 1 / (double)SamplingRateValue.Value;
                        break;
                    case 3:
                        xarr[i] = xarr[i - 6] + (double)ImpulsePeriodValue.Value + 1 / (double)SamplingRateValue.Value;
                        break;
                    case 4:
                        xarr[i] = xarr[i - 6] + (double)ImpulsePeriodValue.Value + 1 / (double)SamplingRateValue.Value;
                        break;
                    case 5:
                        xarr[i] = xarr[i - 6] + (double)ImpulsePeriodValue.Value;
                        break;
                    default:
                        break;
                }
            }

            for (yarr[0] = ((double)SeveralSlidersImpulseTrackBar.SelectedMin <= 0) ? (double)ImpulseLevelValue.Value : (double)ImpulseBaseValue.Value,
                yarr[1] = ((double)SeveralSlidersImpulseTrackBar.SelectedMin <= 0) ? (double)ImpulseLevelValue.Value : (double)ImpulseBaseValue.Value,
                yarr[2] = (double)ImpulseLevelValue.Value,
                yarr[3] = (double)ImpulseLevelValue.Value,
                yarr[4] = ((double)SeveralSlidersImpulseTrackBar.SelectedMax >= (double)SeveralSlidersImpulseTrackBar.Max) ?
                            (double)ImpulseLevelValue.Value : (double)ImpulseBaseValue.Value,
                yarr[5] = ((double)SeveralSlidersImpulseTrackBar.SelectedMax >= (double)SeveralSlidersImpulseTrackBar.Max) ?
                            (double)ImpulseLevelValue.Value : (double)ImpulseBaseValue.Value, i = 6; i < chart_points - 1; ++i)
            {
                switch (i % 6)
                {
                    case 0:
                        yarr[i] = ((double)SeveralSlidersImpulseTrackBar.SelectedMin <= 0) ? (double)ImpulseLevelValue.Value : (double)ImpulseBaseValue.Value;
                        break;
                    case 1:
                        yarr[i] = ((double)SeveralSlidersImpulseTrackBar.SelectedMin <= 0) ? (double)ImpulseLevelValue.Value : (double)ImpulseBaseValue.Value;
                        break;
                    case 2:
                        yarr[i] = (double)ImpulseLevelValue.Value;
                        break;
                    case 3:
                        yarr[i] = (double)ImpulseLevelValue.Value;
                        break;
                    case 4:
                        yarr[i] = ((double)SeveralSlidersImpulseTrackBar.SelectedMax >= (double)SeveralSlidersImpulseTrackBar.Max) ?
                            (double)ImpulseLevelValue.Value : (double)ImpulseBaseValue.Value;
                        break;
                    case 5:
                        yarr[i] = ((double)SeveralSlidersImpulseTrackBar.SelectedMax >= (double)SeveralSlidersImpulseTrackBar.Max) ?
                            (double)ImpulseLevelValue.Value : (double)ImpulseBaseValue.Value;
                        break;
                    default:
                        break;
                }
            }

            if (ZeroEndingImpulseCheckBox.Checked)
            {
                for(i = 0; i < chart_points; ++i)
                    if (xarr[i] > (double)SignalDurationValue.Value)
                    {
                        xarr[i] = (double)SignalDurationValue.Value - 2 / (double)SamplingRateValue.Value;
                        yarr[yarr.Length - 1] = 0;
                        break;
                    }
                if (i >= chart_points)
                {
                    xarr[xarr.Length - 1] = (double)SignalDurationValue.Value - 2 / (double)SamplingRateValue.Value;
                    yarr[yarr.Length - 1] = 0;
                }
            }
            else
            {
                xarr[xarr.Length - 1] = (double)SignalDurationValue.Value;
                yarr[yarr.Length - 1] = yarr[yarr.Length - 2];
            }

            for (i = xarr.Length - 1; i > 6; --i)
                if (xarr[i] == 0)
                    xarr[i] = (double)SignalDurationValue.Value;

            series.Points.DataBindXY(xarr, yarr);

            WholeSignalChart.ChartAreas[0].AxisX.Minimum = 0;
            WholeSignalChart.ChartAreas[0].AxisX.Maximum = (double)SignalDurationValue.Value;
            WholeSignalChart.ChartAreas[0].AxisY.Minimum = Double.NaN;
            WholeSignalChart.ChartAreas[0].AxisY.Maximum = Double.NaN;

            WholeSignalChart.ChartAreas[0].AxisX.Interval = (double)SignalDurationValue.Value / 10;
        }

        private void ImpulsePeriodValue_ValueChanged(object sender, EventArgs e)
        {
            SeveralSlidersImpulseTrackBar.Max = (float)ImpulsePeriodValue.Value;
            SignalDurationValue.Minimum = ImpulsePeriodValue.Value;

            var buff = (decimal)SeveralSlidersImpulseTrackBar.SelectedMin;
            nud_Min.Minimum = (decimal)SeveralSlidersImpulseTrackBar.Min;
            nud_Min.Maximum = (decimal)SeveralSlidersImpulseTrackBar.SelectedMax;
            nud_Min.Value = buff;

            buff = (decimal)SeveralSlidersImpulseTrackBar.SelectedMax;
            nud_Max.Minimum = (decimal)SeveralSlidersImpulseTrackBar.SelectedMin;
            nud_Max.Maximum = (decimal)SeveralSlidersImpulseTrackBar.Max;
            nud_Max.Value = buff;

            impulse_graph(sender, e);
        }

        private void SaveImpulseChannelButton_Click(object sender, EventArgs e)
        {
            double[] data = new double[0];

            double[] controls = {(double)SignalDurationValue.Value,
                                (double)SamplingRateValue.Value,
                                (double)ImpulsePeriodValue.Value,
                                (double)SeveralSlidersImpulseTrackBar.SelectedMin,
                                (double)SeveralSlidersImpulseTrackBar.SelectedMax,
                                (double)SeveralSlidersImpulseTrackBar.Min,
                                (double)SeveralSlidersImpulseTrackBar.Max,
                                (double)ImpulseBaseValue.Value,
                                (double)ImpulseLevelValue.Value,
                                ZeroEndingImpulseCheckBox.Checked ? 1 : 0};

            if (calculate_impulse_channel(ref data, ref controls) == 0)
                write_ini_channel(ImpulseChannelName.Text);

            save_csv_channel(ImpulseChannelName.Text, ref data);
            write_csv_file();

            ChannelsListBox.ClearSelected();
            ChannelsListBox.SelectedIndex = ChannelsListBox.FindByName(ImpulseChannelName.Text);
        }

        private void ZeroEndingImpulseCheckBox_CheckedChange(object sender, EventArgs e)
        {
            impulse_graph(sender, e);
        }

        private void DeleteImpulseChannelButton_Click(object sender, EventArgs e)
        {
            IniFile MyIni = new IniFile(ini_path);

            MyIni.DeleteSection(ImpulseChannelName.Text);
            delete_csv_channel(ImpulseChannelName.Text);

            write_csv_file();
        }

        #endregion

        #region SelectionRangeSlider

        private void SelectionRangeSlider_MouseClickMin(object sender, EventArgs e)
        {
            nud_Min.DecimalPlaces = MathDecimals.GetDecimalPlaces((decimal)(SeveralSlidersImpulseTrackBar.Max / 100));

            var buff = (decimal)SeveralSlidersImpulseTrackBar.SelectedMin;
            nud_Min.Minimum = (decimal)SeveralSlidersImpulseTrackBar.Min;
            nud_Min.Maximum = (decimal)SeveralSlidersImpulseTrackBar.SelectedMax;
            nud_Min.Value = buff;

            nud_Min.Location = new Point(PointToClient(MousePosition).X, PointToClient(MousePosition).Y);

            nud_Min.Show();
        }

        private void SelectionRangeSlider_MouseClickMax(object sender, EventArgs e)
        {
            nud_Max.DecimalPlaces = MathDecimals.GetDecimalPlaces((decimal)(SeveralSlidersImpulseTrackBar.Max / 100));

            var buff = (decimal)SeveralSlidersImpulseTrackBar.SelectedMax;
            nud_Max.Minimum = (decimal)SeveralSlidersImpulseTrackBar.SelectedMin;
            nud_Max.Maximum = (decimal)SeveralSlidersImpulseTrackBar.Max;
            nud_Max.Value = buff;

            nud_Max.Location = new Point(PointToClient(MousePosition).X, PointToClient(MousePosition).Y);

            nud_Max.Show();
        }

        private void SelectionRangeSlider_MouseLeave(object sender, EventArgs e)
        {
            if (!nud_Min.Focused)
                nud_Min.Hide();
            if (!nud_Max.Focused)
                nud_Max.Hide();
        }

        private void SelectionRangeSlider_CursorLeftMin(object sender, EventArgs e)
        {
            if (!nud_Min.Focused)
                nud_Min.Hide();
        }

        private void SelectionRangeSlider_CursorLeftMax(object sender, EventArgs e)
        {
            if (!nud_Max.Focused)
                nud_Max.Hide();
        }

        private void nud_Min_ValueChange(object sender, EventArgs e)
        {
            SeveralSlidersImpulseTrackBar.SelectedMin = (float)nud_Min.Value;
        }

        private void nud_Max_ValueChange(object sender, EventArgs e)
        {
            SeveralSlidersImpulseTrackBar.SelectedMax = (float)nud_Max.Value;
        }

        #endregion

        #region ChannelsList

        private void ChannelsListBox_fill()
        {
            if (Channels.Count <= 0)
            {
                ChannelsListBox.Items.Clear();
                return;
            }   

            int i = 0;
            string[] s = new string[Channels.Count];
            for (i = 0; i < s.Length; ++i)
                s[i] = Channels[i].name;

            ChannelsListBox.Items.Clear();
            ChannelsListBox.Items.AddRange(s);
        }

        private void ChannelsListBox_SelectionChanged(object sender, EventArgs e)
        {
            if (ChannelsListBox.SelectedIndex < 0 || ChannelsListBox.Items.Count <= 0)
                return;

            read_ini_channel(Channels[ChannelsListBox.SelectedIndex].name);

            try
            {
                RampChannelName.Text = Channels[ChannelsListBox.SelectedIndex].name;
                ImpulseChannelName.Text = Channels[ChannelsListBox.SelectedIndex].name;

                OneSegmentChart.Series[0].Name = Channels[ChannelsListBox.SelectedIndex].name;
                OneSegmentChart.Series[0].Color = WholeSignalChart.Series[ChannelsListBox.Items
                        [ChannelsListBox.SelectedIndex].ToString()].Color;
            }
            catch
            {
                return;
            }
        }

        private void ChannelsListBox_MoveSelectedUp(object sender, EventArgs e)
        {
            this.Swap(ref Channels, ChannelsListBox.SelectedIndex, ChannelsListBox.SelectedIndex - 1);
            redraw_whole_chart();
            write_csv_file();
        }

        private void ChannelsListBox_MoveSelectedDown(object sender, EventArgs e)
        {
            this.Swap(ref Channels, ChannelsListBox.SelectedIndex, ChannelsListBox.SelectedIndex - 1);
            redraw_whole_chart();
            write_csv_file();
        }

        #endregion

        #region MouseEvents

        private void OnMouseMove(object sender, EventArgs e)
        {
            var position = PointToClient(MousePosition);
            var position_ss = SeveralSlidersImpulseTrackBar.PointToClient(MousePosition);

            if (!(position.X > nud_Min.Location.X - nud_Min.Height &&
                position.X < nud_Min.Location.X + nud_Min.Width + nud_Min.Height &&
                position.Y > nud_Min.Location.Y - nud_Min.Height &&
                position.Y < nud_Min.Location.Y + nud_Min.Height + nud_Min.Height)
                &&
                !(position_ss.X > SeveralSlidersImpulseTrackBar.sMin.X - nud_Min.Height &&
                position_ss.X < SeveralSlidersImpulseTrackBar.sMin.X + SeveralSlidersImpulseTrackBar.sMin.rect.Width + nud_Min.Height &&
                position_ss.Y > SeveralSlidersImpulseTrackBar.sMin.Y - nud_Min.Height &&
                position_ss.Y < SeveralSlidersImpulseTrackBar.sMin.Y + SeveralSlidersImpulseTrackBar.sMin.rect.Height + nud_Min.Height))
                SelectionRangeSlider_CursorLeftMin(sender, e);

            if (!(position.X > nud_Max.Location.X - nud_Max.Height &&
                position.X < nud_Max.Location.X + nud_Max.Width + nud_Max.Height &&
                position.Y > nud_Max.Location.Y - nud_Max.Height &&
                position.Y < nud_Max.Location.Y + nud_Max.Height + nud_Max.Height)
                &&
                !(position_ss.X > SeveralSlidersImpulseTrackBar.sMax.X - nud_Max.Height &&
                position_ss.X < SeveralSlidersImpulseTrackBar.sMax.X + SeveralSlidersImpulseTrackBar.sMax.rect.Width + nud_Max.Height &&
                position_ss.Y > SeveralSlidersImpulseTrackBar.sMax.Y - nud_Max.Height &&
                position_ss.Y < SeveralSlidersImpulseTrackBar.sMax.Y + SeveralSlidersImpulseTrackBar.sMax.rect.Height + nud_Max.Height))
                SelectionRangeSlider_CursorLeftMax(sender, e);
        }

        private void OnMouseClick(object sender, EventArgs e)
        {
            var position = PointToClient(MousePosition);
            var position_ss = SeveralSlidersImpulseTrackBar.PointToClient(MousePosition);

            if (!(position.X > nud_Min.Location.X && position.X < nud_Min.Location.X + nud_Min.Width &&
                position.Y > nud_Min.Location.Y && position.Y < nud_Min.Location.Y + nud_Min.Height)
                &&
                !(position_ss.X > SeveralSlidersImpulseTrackBar.sMin.X && position_ss.X < SeveralSlidersImpulseTrackBar.sMin.X + SeveralSlidersImpulseTrackBar.sMin.rect.Width &&
                position_ss.Y > SeveralSlidersImpulseTrackBar.sMin.Y && position_ss.Y < SeveralSlidersImpulseTrackBar.sMin.Y + SeveralSlidersImpulseTrackBar.sMin.rect.Height))
                nud_Min.Hide();

            if (!(position.X > nud_Max.Location.X && position.X < nud_Max.Location.X + nud_Max.Width &&
                position.Y > nud_Max.Location.Y && position.Y < nud_Max.Location.Y + nud_Max.Height)
                &&
                !(position_ss.X > SeveralSlidersImpulseTrackBar.sMax.X && position_ss.X < SeveralSlidersImpulseTrackBar.sMax.X + SeveralSlidersImpulseTrackBar.sMax.rect.Width &&
                position_ss.Y > SeveralSlidersImpulseTrackBar.sMax.Y && position_ss.Y < SeveralSlidersImpulseTrackBar.sMax.Y + SeveralSlidersImpulseTrackBar.sMax.rect.Height))
                nud_Max.Hide();
        }

        #endregion

        #region ChannelsCalculation

        private int calculate_ramp_channel(ref double[] data, ref double[] controls)
        {
            data = new double[(int)(controls[0] * controls[1])];

            double segments = controls[0] / controls[2];
            int points_per_segment = (int)((double)data.Length / segments), i, j;
            if (points_per_segment <= 0)
                return -1;

            double
                l_slope_resolution = points_per_segment * (controls[3] / 100),
                r_slope_resolution = points_per_segment - l_slope_resolution,
                l_increment = (controls[5] - controls[4]) /
                ((l_slope_resolution <= 0) ? 1 : l_slope_resolution),
                r_increment = (controls[4] - controls[5]) /
                ((r_slope_resolution <= 0) ? 1 : r_slope_resolution),
                y = (l_slope_resolution <= 0) ? controls[5] : controls[4];

            for (i = 0, j = 0; i < data.Length; j = 0)
            {
                for (data[i++] = y; j < l_slope_resolution && i < data.Length; data[i] = data[i - 1] + l_increment, ++i, ++j) ;
                for (; j < points_per_segment - 1 && i < data.Length; data[i] = data[i - 1] + r_increment, ++i, ++j) ;
            }

            if (controls[6] == 1)
                data[data.Length - 1] = 0;

            return 0;
        }

        private int calculate_impulse_channel(ref double[] data, ref double[] controls)
        {
            data = new double[(int)(controls[0] * controls[1])];

            double segments = controls[0] / controls[2];
            int points_per_segment = (int)((double)data.Length / segments), i, j;
            if (points_per_segment <= 0)
                return -1;

            double
                l_slope_resolution = points_per_segment *
                (controls[3] / (controls[6] - controls[5])),
                r_slope_resolution = points_per_segment *
                (controls[4] / (controls[6] - controls[5])),
                y_base = controls[7],
                y_level = controls[8];

            for (i = 0, j = 0; i < data.Length; j = 0)
            {
                for (; j < l_slope_resolution && i < data.Length; data[i] = y_base, ++i, ++j) ;
                for (; j < r_slope_resolution && i < data.Length; data[i] = y_level, ++i, ++j) ;
                for (; j < points_per_segment && i < data.Length; data[i] = y_base, ++i, ++j) ;
            }

            if (controls[9] == 1)
                data[data.Length - 1] = 0;

            return 0;
        }

        #endregion

        #region SaveAllChannels

        private void SaveAllChannelsButton_Click(object sender, EventArgs e)
        {
            SaveAllChannelsProgress.Style = ProgressBarStyle.Marquee;

            BackgroundWorker bgw = new BackgroundWorker();
            bgw.DoWork += new DoWorkEventHandler(bgw_DoWork);
            bgw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgw_WorkComplete);

            IniFile MyIni = new IniFile(ini_path);
            MyIni.Write("SignalDuration", SignalDurationValue.Value.ToString(), "Main");
            MyIni.Write("SamplingRate", SamplingRateValue.Value.ToString(), "Main");

            bgw.RunWorkerAsync();
        }

        private void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            IniFile MyIni = new IniFile(ini_path);

            foreach (var ch in Channels)
            {
                if (!MyIni.KeyExists("Type", ch.name))
                    continue;

                double[] data = new double[0];

                switch (Convert.ToInt32(MyIni.Read("Type", ch.name)))
                {
                    case 0:
                        {
                            double[] controls = {Convert.ToDouble(MyIni.Read("SignalDuration", "Main")),
                                                Convert.ToDouble(MyIni.Read("SamplingRate", "Main")),
                                                Convert.ToDouble(MyIni.Read("Period", ch.name)),
                                                (int)(Convert.ToDecimal(RampPeakTrackerLabel.Text) /
                                                Convert.ToDecimal(RampPeriodValue.Value) * 100),
                                                Convert.ToDouble(MyIni.Read("BeginValue", ch.name)),
                                                Convert.ToDouble(MyIni.Read("PeakValue", ch.name)),
                                                Convert.ToInt32(MyIni.Read("IsZeroEnding", ch.name)) == 1 ? 1 : 0};

                            if (calculate_ramp_channel(ref data, ref controls) == 0)
                                save_csv_channel(ch.name, ref data);
                            break;
                        }
                    case 1:
                        {
                            double[] controls = {Convert.ToDouble(MyIni.Read("SignalDuration", "Main")),
                                                Convert.ToDouble(MyIni.Read("SamplingRate", "Main")),
                                                Convert.ToDouble(MyIni.Read("Period", ch.name)),
                                                Convert.ToDouble(MyIni.Read("LeftPos", ch.name)),
                                                Convert.ToDouble(MyIni.Read("RightPos", ch.name)),
                                                Convert.ToDouble(MyIni.Read("Min", ch.name)),
                                                Convert.ToDouble(MyIni.Read("Max", ch.name)),
                                                Convert.ToDouble(MyIni.Read("BaseValue", ch.name)),
                                                Convert.ToDouble(MyIni.Read("LevelValue", ch.name)),
                                                Convert.ToInt32(MyIni.Read("IsZeroEnding", ch.name)) == 1 ? 1 : 0};

                            if (calculate_impulse_channel(ref data, ref controls) == 0)
                                save_csv_channel(ch.name, ref data);
                            break;
                        }
                    case 2:
                        break;
                    default:
                        return;
                }

                write_csv_file();
            }
        }

        private void bgw_WorkComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            SaveAllChannelsProgress.Style = ProgressBarStyle.Continuous;
        }

        #endregion

        #region MenuStrip

        private void cSVFilePathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fb = new FolderBrowserDialog();
            fb.ShowDialog();
            if (fb.SelectedPath == "")
                return;

            RegistryKey key = Registry.CurrentUser.CreateSubKey("SignalMaker", true);
            csv_path = fb.SelectedPath + "\\" + csv_file_name;
            if (csv_path.First() == '\\')
                csv_path = csv_path.Substring(1, csv_path.Length - 1);
            key.SetValue("CSV", csv_path);

            this.cSVFilePathToolStripMenuItem.ToolTipText = "Current CSV file path: " + csv_path;

            key.Close();

            init_frame(sender, e);
        }

        private void iNIFilePathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fb = new FolderBrowserDialog();
            fb.ShowDialog();
            if (fb.SelectedPath == "")
                return;

            RegistryKey key = Registry.CurrentUser.CreateSubKey("SignalMaker", true);
            ini_path = fb.SelectedPath + "\\" + ini_file_name;
            if (ini_path.First() == '\\')
                ini_path = ini_path.Substring(1, ini_path.Length - 1);
            key.SetValue("INI", ini_path);

            this.iNIFilePathToolStripMenuItem.ToolTipText = "Current INI file path: " + ini_path;

            key.Close();

            //SignalDurationValue.Value = Convert.ToDecimal(new IniFile(ini_path).Read("SignalDuration", "Main")); TODO: убрать ошику при прочтении длина сигнала < периода пилы
            SamplingRateValue.Value = Convert.ToDecimal(new IniFile(ini_path).Read("SamplingRate", "Main"));

            init_frame(sender, e);
        }

        #endregion
    }
}