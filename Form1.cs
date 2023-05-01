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
using System.Runtime.InteropServices;

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
            csv_path = csv_file_name,
            ch_name_to_save;
        private List<channel> Channels = new List<channel>();
        char csv_delimiter = ';';
        bool is_first_col_sr = true,
            is_csv_empty_on_init = false,
            is_drawing_busy = false,
            is_from_listbox = false;
        int TAB_PAGE_I_WANT = -1; //ВСЕМ КОСТЫЛЯМ КОСТЫЛЬ. Каждый раз при смене tabpage он заходит на нулевой.
                                  //Эта хрень нужна чтобы он так не делал. ИБО Я НЕ ЗНАЮ КАК ЭТО ФИКСИТЬ
        NumericUpDown nud_Min = new NumericUpDown() { Width = 70 };
        NumericUpDown nud_Max = new NumericUpDown() { Width = 70 };

        private void Form1_Load(object sender, EventArgs e)
        {
            nud_Min.Hide();
            nud_Max.Hide();

            RegistryKey key = Registry.CurrentUser.OpenSubKey("SignalMaker");

            if (key != null)
            {
                ini_path = (string)key.GetValue("INI", ini_file_name);
                csv_path = (string)key.GetValue("CSV", csv_file_name);
                ini_file_name = ini_path.Substring(ini_path.LastIndexOf('\\') + 1, ini_path.Length - ini_path.LastIndexOf('\\') - 1);
                csv_file_name = csv_path.Substring(csv_path.LastIndexOf('\\') + 1, csv_path.Length - csv_path.LastIndexOf('\\') - 1);

                if (ini_path.First() == '\\')
                    ini_path = ini_path.Substring(1, ini_path.Length - 1);
                if (csv_path.First() == '\\')
                    csv_path = csv_path.Substring(1, csv_path.Length - 1);
            }

            this.iNIFilePathToolStripMenuItem.ToolTipText = "Current INI file path: " + ini_path;
            this.cSVFilePathToolStripMenuItem.ToolTipText = "Current CSV file path: " + csv_path;

            HookupMouseEnterEvents(this);

            init_frame(sender, e);
        }

        #region SuppFuncs

        private void init_frame(object sender, EventArgs e)
        {
            if (read_ini_file() < 0)
            {
                IniFile MyIni = new IniFile(ini_path); //если не нашли файла - создаем
            }

            SaveAllChannelsButton_Click(sender, e);

            ChannelsListBox_fill();

            if (ChannelsListBox.Items.Count <= 0)
            {
                RampChannelName.Text = "NewChannel_0";
                ImpulseChannelName.Text = "NewChannel_0";
                CustomChannelName.Text = "NewChannel_0";

                double[] data = new double[0];
                save_csv_channel(RampChannelName.Text, ref data);

                WholeSignalChart.Series.Clear();
                if (WholeSignalChart.Series.IsUniqueName(RampChannelName.Text))
                    WholeSignalChart.Series.Add(RampChannelName.Text);
                ChannelsListBox.selected_item = 0;

                RampTabPage_Enter(sender, e);

                is_csv_empty_on_init = true;
            }
            else
            {
                RampChannelName.Text = "NewChannel_" + Channels.Count.ToString();
                ImpulseChannelName.Text = "NewChannel_" + Channels.Count.ToString();
                CustomChannelName.Text = "NewChannel_" + Channels.Count.ToString();

                redraw_whole_chart();

                ChannelsListBox.selected_item = 0;
                read_ini_channel(ChannelsListBox.Items[0]);
            }

            RampPeakTrackerLabel.Text = ((double)RampPeakTrackBar.Value / 100 * (double)RampPeriodValue.Value).ToString();

            SeveralSlidersImpulseTrackBar.Max = ImpulsePeriodValue.Value;
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
            redraw_whole_chart();

            if (ChannelsListBox.selected_item < 0 || ChannelsListBox.Items.Count <= 0)
                return;

            int si = ChannelsListBox.selected_item;
            ChannelsListBox.clear_selection();
            ChannelsListBox.selected_item = si;

            switch (SignalTypeTabControl.SelectedIndex)
            {
                case 0:
                    ramp_graph(sender, e);
                    break;
                case 1:
                    impulse_graph(sender, e);
                    break;
                case 2:
                    break;
                case 3:
                    custom_graph(sender, e);
                    break;
                default:
                    break;
            }
        }

        private void redraw_whole_chart()
        {
            IniFile MyIni = new IniFile(ini_path);

            if (WholeSignalChart.Series.Count() > 0)
                WholeSignalChart.Series.Clear();

            for (int i = 0; i < Channels.Count; ++i)
            {
                if (Channels[i] == null || !MyIni.KeyExists("Type", Channels[i].name))
                    continue;

                if (WholeSignalChart.Series.IsUniqueName(Channels[i].name))
                {
                    WholeSignalChart.recalc_chart_area = true;
                    WholeSignalChart.Series.Add(Channels[i].name);
                }
                int index = WholeSignalChart.FindSettingIndexByName(Channels[i].name);
                WholeSignalChart.Series.Last().Color = WholeSignalChart.settings[index].color;
                WholeSignalChart.Series.Last().BorderWidth = (int)WholeSignalChart.settings[index].width;

                switch (Convert.ToInt32(MyIni.Read("Type", Channels[i].name)))
                {
                    case 0:

                        RampBeginValue.Value = Convert.ToDecimal(MyIni.Read("BeginValue", Channels[i].name));
                        RampPeakValue.Value = Convert.ToDecimal(MyIni.Read("PeakValue", Channels[i].name));
                        RampPeriodValue.Value = Convert.ToDecimal(MyIni.Read("Period", Channels[i].name));
                        RampPeakTrackerLabel.Text = MyIni.Read("PeakPos", Channels[i].name);
                        RampPeakTrackBar.Value = (int)(Convert.ToDecimal(RampPeakTrackerLabel.Text) /
                            Convert.ToDecimal(RampPeriodValue.Value) * 100);
                        ZeroEndingRampCheckBox.Checked = Convert.ToInt32(MyIni.Read("IsZeroEnding", Channels[i].name)) == 1;

                        redraw_ramp_wholegraph(Channels[i].name);
                        break;
                    case 1:

                        ImpulseBaseValue.Value = Convert.ToDecimal(MyIni.Read("BaseValue", Channels[i].name));
                        ImpulseLevelValue.Value = Convert.ToDecimal(MyIni.Read("LevelValue", Channels[i].name));
                        ImpulsePeriodValue.Value = Convert.ToDecimal(MyIni.Read("Period", Channels[i].name));
                        SeveralSlidersImpulseTrackBar.Min = Convert.ToDecimal(MyIni.Read("Min", Channels[i].name));
                        SeveralSlidersImpulseTrackBar.Max = Convert.ToDecimal(MyIni.Read("Max", Channels[i].name));
                        SeveralSlidersImpulseTrackBar.SelectedMin = Convert.ToDecimal(MyIni.Read("LeftPos", Channels[i].name));
                        SeveralSlidersImpulseTrackBar.SelectedMax = Convert.ToDecimal(MyIni.Read("RightPos", Channels[i].name));
                        ZeroEndingImpulseCheckBox.Checked = Convert.ToInt32(MyIni.Read("IsZeroEnding", Channels[i].name)) == 1;

                        redraw_impulse_wholegraph(Channels[i].name);
                        break;
                    case 2:
                        break;
                    case 3:

                        CustomPeriodValue.Value = Convert.ToDecimal(MyIni.Read("Period", Channels[i].name));
                        ZeroEndingCustomCheckBox.Checked = Convert.ToInt32(MyIni.Read("IsZeroEnding", Channels[i].name)) == 1;

                        DataPoint[] dataset = new DataPoint[Convert.ToInt32(MyIni.Read("Length", Channels[i].name))];

                        for (int j = 0; j < dataset.Count(); ++j)
                            dataset[j] = new DataPoint(Convert.ToDouble(MyIni.Read("XP" + j, Channels[i].name)),
                                Convert.ToDouble(MyIni.Read("YP" + j, Channels[i].name)));

                        pointCoordinatesListBox.Set_DataSet(dataset);

                        redraw_custom_wholegraph(Channels[i].name);
                        break;
                    default:
                        return;
                }
            }
        }

        private void bgw_SaveOneChannel(object sender, DoWorkEventArgs e)
        {
            IniFile MyIni = new IniFile(ini_path);

            if (!MyIni.KeyExists("Type", ch_name_to_save))
                return;

            double[] data = new double[0];

            switch (Convert.ToInt32(MyIni.Read("Type", ch_name_to_save)))
            {
                case 0:
                    {
                        double[] controls = {Convert.ToDouble(MyIni.Read("SignalDuration", "Main")),
                                            Convert.ToDouble(MyIni.Read("SamplingRate", "Main")),
                                            Convert.ToDouble(MyIni.Read("Period", ch_name_to_save)),
                                            (int)(Convert.ToDecimal(RampPeakTrackerLabel.Text) /
                                            Convert.ToDecimal(RampPeriodValue.Value) * 100),
                                            Convert.ToDouble(MyIni.Read("BeginValue", ch_name_to_save)),
                                            Convert.ToDouble(MyIni.Read("PeakValue", ch_name_to_save)),
                                            Convert.ToInt32(MyIni.Read("IsZeroEnding", ch_name_to_save)) == 1 ? 1 : 0};

                        if (calculate_ramp_channel(ref data, ref controls) == 0)
                            save_csv_channel(ch_name_to_save, ref data);
                        break;
                    }
                case 1:
                    {
                        double[] controls = {Convert.ToDouble(MyIni.Read("SignalDuration", "Main")),
                                            Convert.ToDouble(MyIni.Read("SamplingRate", "Main")),
                                            Convert.ToDouble(MyIni.Read("Period", ch_name_to_save)),
                                            Convert.ToDouble(MyIni.Read("LeftPos", ch_name_to_save)),
                                            Convert.ToDouble(MyIni.Read("RightPos", ch_name_to_save)),
                                            Convert.ToDouble(MyIni.Read("Min", ch_name_to_save)),
                                            Convert.ToDouble(MyIni.Read("Max", ch_name_to_save)),
                                            Convert.ToDouble(MyIni.Read("BaseValue", ch_name_to_save)),
                                            Convert.ToDouble(MyIni.Read("LevelValue", ch_name_to_save)),
                                            Convert.ToInt32(MyIni.Read("IsZeroEnding", ch_name_to_save)) == 1 ? 1 : 0};

                        if (calculate_impulse_channel(ref data, ref controls) == 0)
                            save_csv_channel(ch_name_to_save, ref data);
                        break;
                    }
                case 2:
                    break;
                case 3:
                    {
                        double[] controls = {Convert.ToDouble(MyIni.Read("SignalDuration", "Main")),
                                            Convert.ToDouble(MyIni.Read("SamplingRate", "Main")),
                                            Convert.ToDouble(MyIni.Read("Period", ch_name_to_save)),
                                            Convert.ToInt32(MyIni.Read("IsZeroEnding", ch_name_to_save)) == 1 ? 1 : 0};

                        DataPoint[] dataset = new DataPoint[Convert.ToInt32(MyIni.Read("Length", ch_name_to_save))];

                        for (int j = 0; j < dataset.Count(); ++j)
                            dataset[j] = new DataPoint(Convert.ToDouble(MyIni.Read("XP" + j, ch_name_to_save)),
                                Convert.ToDouble(MyIni.Read("YP" + j, ch_name_to_save)));

                        if (calculate_custom_channel(ref data, ref controls, ref dataset) == 0)
                            save_csv_channel(ch_name_to_save, ref data);
                    }
                    break;
                default:
                    return;
            }

            write_csv_file();
        }

        private void MyChart_X_margin_needed(object sender, EventArgs e)
        {
            ((MyChart)sender).point_value_X_margin = 1 / (double)SamplingRateValue.Value;
        }

        private void MyChart_FinishedDrawing(object sender, EventArgs e)
        {
            is_drawing_busy = false;
        }

        private void PointCoordinatesListBox_X_margin_needed(object sender, EventArgs e)
        {
            pointCoordinatesListBox.point_value_X_margin = 1 / (double)SamplingRateValue.Value;
        }

        private void PointCoordinatesListBox_PointChanged(DataPoint point, int index)
        {
            //is_drawing_busy = false;
            custom_graph(null, null);
        }

        private void PointCoordinatesListBox_PointAdded(DataPoint point, int index)
        {
            is_drawing_busy = false;
            custom_graph(null, null);
        }

        private void PointCoordinatesListBox_PointDeleted(DataPoint point, int index)
        {
            is_drawing_busy = false;
            custom_graph(null, null);
        }

        private void OneSegmentChart_PointChanged(DataPoint point, int index)
        {
            TAB_PAGE_I_WANT = 3;
            SignalTypeTabControl.SelectedIndex = 3;
            TAB_PAGE_I_WANT = -1;

            pointCoordinatesListBox.Set_ChangedPoint(point, index);
        }

        private void OneSegmentChart_PointAdded(DataPoint point, int index)
        {
            if (SignalTypeTabControl.SelectedIndex != 3)
            {
                TAB_PAGE_I_WANT = 3;
                SignalTypeTabControl.SelectedIndex = 3;
                TAB_PAGE_I_WANT = -1;
            }
            else
            {
                pointCoordinatesListBox.is_from_outside = true;
                pointCoordinatesListBox.Set_AddedPoint(point, index);
            }

            redraw_custom_wholegraph();
        }

        private void OneSegmentChart_PointDeleted(DataPoint point, int index)
        {
            if (SignalTypeTabControl.SelectedIndex != 3)
            {
                TAB_PAGE_I_WANT = 3;
                SignalTypeTabControl.SelectedIndex = 3;
                TAB_PAGE_I_WANT = -1;
            }
            else
            {
                pointCoordinatesListBox.is_from_outside = true;
                pointCoordinatesListBox.Set_DeletedPoint(point, index);
            }

            redraw_custom_wholegraph();
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
                for (int i = 0; i < (SamplingRateValue.Value * SignalDurationValue.Value); ++i)
                {
                    string line = is_first_col_sr ? SamplingRateValue.Value.ToString() + ";" : "";
                    for (int j = 0; j < Channels.Count; ++j)
                        line += ((i < Channels[j].data.Length) ? Channels[j].data[i].ToString() : "")
                            + csv_delimiter.ToString();
                    sw.WriteLine(line.Trim(csv_delimiter));
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

                    MyIni.Write("Index", ChannelsListBox.Items.IndexOf(name).ToString(), name);
                    MyIni.Write("Type", SignalTypeTabControl.SelectedIndex.ToString(), name);
                    MyIni.Write("BeginValue", RampBeginValue.Value.ToString(), name);
                    MyIni.Write("PeakValue", RampPeakValue.Value.ToString(), name);
                    MyIni.Write("Period", RampPeriodValue.Value.ToString(), name);
                    MyIni.Write("PeakPos", RampPeakTrackerLabel.Text, name);
                    MyIni.Write("IsZeroEnding", (ZeroEndingRampCheckBox.Checked) ? "1" : "0", name);

                    break;
                case 1:

                    MyIni.Write("Index", ChannelsListBox.Items.IndexOf(name).ToString(), name);
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
                case 3:

                    MyIni.Write("Index", ChannelsListBox.Items.IndexOf(name).ToString(), name);
                    MyIni.Write("Type", SignalTypeTabControl.SelectedIndex.ToString(), name);

                    DataPoint[] dataset = pointCoordinatesListBox.Get_DataSet();
                    if (dataset == null)
                        return;

                    MyIni.Write("Length", dataset.Count().ToString(), name);

                    for (int i = 0; i < dataset.Count(); ++i)
                    {
                        MyIni.Write("XP" + i, dataset[i].XValue.ToString(), name);
                        MyIni.Write("YP" + i, dataset[i].YValues[0].ToString(), name);
                    }

                    MyIni.Write("Period", CustomPeriodValue.Value.ToString(), name);
                    MyIni.Write("IsZeroEnding", (ZeroEndingCustomCheckBox.Checked) ? "1" : "0", name);

                    break;
                default:
                    return;
            }
        }

        private void read_ini_channel(string name)
        {
            IniFile MyIni = new IniFile(ini_path);
            
            if (!MyIni.KeyExists("Type", name))
                return;

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

                    TAB_PAGE_I_WANT = 0;
                    SignalTypeTabControl.SelectedIndex = 0;
                    TAB_PAGE_I_WANT = -1;

                    redraw_ramp();
                    redraw_ramp_wholegraph();

                    break;
                case 1:
                    
                    ImpulseBaseValue.Value = Convert.ToDecimal(MyIni.Read("BaseValue", name));
                    ImpulseLevelValue.Value = Convert.ToDecimal(MyIni.Read("LevelValue", name));
                    ImpulsePeriodValue.Value = Convert.ToDecimal(MyIni.Read("Period", name));
                    SeveralSlidersImpulseTrackBar.Min = Convert.ToDecimal(MyIni.Read("Min", name));
                    SeveralSlidersImpulseTrackBar.Max = Convert.ToDecimal(MyIni.Read("Max", name));
                    SeveralSlidersImpulseTrackBar.SelectedMin = Convert.ToDecimal(MyIni.Read("LeftPos", name));
                    SeveralSlidersImpulseTrackBar.SelectedMax = Convert.ToDecimal(MyIni.Read("RightPos", name));
                    ZeroEndingImpulseCheckBox.Checked = Convert.ToInt32(MyIni.Read("IsZeroEnding", name)) == 1;

                    TAB_PAGE_I_WANT = 1;
                    SignalTypeTabControl.SelectedIndex = 1;
                    TAB_PAGE_I_WANT = -1;

                    redraw_impulse();
                    redraw_impulse_wholegraph();

                    break;
                case 2:
                    break;
                case 3:

                    CustomPeriodValue.Value = Convert.ToDecimal(MyIni.Read("Period", name));
                    ZeroEndingCustomCheckBox.Checked = Convert.ToInt32(MyIni.Read("IsZeroEnding", name)) == 1;

                    DataPoint[] dataset = new DataPoint[Convert.ToInt32(MyIni.Read("Length", name))];

                    for (int i = 0; i < dataset.Count(); ++i)
                        dataset[i] = new DataPoint(Convert.ToDouble(MyIni.Read("XP" + i, name)), Convert.ToDouble(MyIni.Read("YP" + i, name)));

                    TAB_PAGE_I_WANT = 3;
                    SignalTypeTabControl.SelectedIndex = 3;
                    TAB_PAGE_I_WANT = -1;

                    pointCoordinatesListBox.Set_DataSet(dataset);

                    redraw_custom();
                    redraw_custom_wholegraph();

                    break;
                default:
                    return;
            }
        }

        private int read_ini_file()
        {
            Channels.Clear();

            if (!File.Exists(ini_path))
                return -1;

            IniFile MyIni = new IniFile(ini_path);
            if (MyIni.KeyExists("SignalDuration", "Main") &&
                MyIni.KeyExists("SamplingRate", "Main"))
            {
                SignalDurationValue.Value = Convert.ToDecimal(MyIni.Read("SignalDuration", "Main"));
                SamplingRateValue.Value = Convert.ToDecimal(MyIni.Read("SamplingRate", "Main"));
            }

            string[] Sections = MyIni.SectionNames();
                string err = "";
            if (Sections.Count() <= 1)
                return -1;

            Channels.AddRange(new channel[Sections.Count() - 1]);

            for (int i = 1; i < Sections.Count(); ++i)
            {
                try
                {
                    Channels[Convert.ToInt32(MyIni.Read("Index", Sections[i]))] = new channel { name = Sections[i], data = new double[0] };
                }
                catch
                {
                    err += "Ошибка при чтении ключа \"Index\" в секции " + "\"" + Sections[i] + "\"\n";
                }
            }

            for (int i = 0, j = Sections.Count() - 1; i < j; ++i)
                if (Channels[i] == null)
                {
                    Channels.Remove(Channels[i]);
                    j--;
                    i--;
                }

            if (err.Length > 0)
            {
                MessageBox.Show("В файле \"" + ini_path + "\":\n\n" + err);
                return -1;
            }

            return 0;
        }

        #endregion

        #region TabPage_Enter

        private void RampTabPage_Enter(object sender, EventArgs e)
        {
            if (TAB_PAGE_I_WANT > 0)
                return;

            SignalDurationValue.Minimum = RampPeriodValue.Value;

            is_drawing_busy = false;
            ramp_graph(sender, e);

            OneSegmentChart.set_default_chart_coord(0, (double)RampPeriodValue.Value,
                OneSegmentChart.ChartAreas[0].AxisY.Minimum,
                OneSegmentChart.ChartAreas[0].AxisY.Maximum);
        }

        private void ImpulseTabPage_Enter(object sender, EventArgs e)
        {
            SignalDurationValue.Minimum = ImpulsePeriodValue.Value;

            is_drawing_busy = false;
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

            OneSegmentChart.set_default_chart_coord(0, (double)ImpulsePeriodValue.Value,
                OneSegmentChart.ChartAreas[0].AxisY.Minimum,
                OneSegmentChart.ChartAreas[0].AxisY.Maximum);
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

        private void CustomTabPage_Enter(object sender, EventArgs e)
        {
            SignalDurationValue.Minimum = CustomPeriodValue.Value;

            DataPoint[] dataset = new DataPoint[OneSegmentChart.Series[0].Points.Count];
            for (int i = 0; i < dataset.Count(); ++i)
                dataset[i] = new DataPoint(OneSegmentChart.Series[0].Points[i].XValue, OneSegmentChart.Series[0].Points[i].YValues[0]);

            if (!is_from_listbox)
            {
                CustomPeriodValue.Value = (decimal)OneSegmentChart.Series[0].Points.Last().XValue;
            }

            pointCoordinatesListBox.Set_DataSet(dataset);

            is_drawing_busy = false;
            custom_graph(sender, e);

            OneSegmentChart.set_default_chart_coord(0, (double)CustomPeriodValue.Value,
                OneSegmentChart.ChartAreas[0].AxisY.Minimum,
                OneSegmentChart.ChartAreas[0].AxisY.Maximum);
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
            RampPeriodValue.Maximum = SignalDurationValue.Value;
            RampPeakTrackerLabel.Text = ((double)RampPeakTrackBar.Value / 100 * (double)RampPeriodValue.Value).ToString();
            ramp_graph(sender, e);
        }

        private void ramp_graph(object sender, EventArgs e)
        {
            if (!is_drawing_busy)
            {
                is_drawing_busy = true;
                redraw_ramp();
                redraw_ramp_wholegraph();
            }
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

            for (int i = 0; i < xarr.Count(); ++i)
                if (xarr[i] > (double)RampPeriodValue.Value)
                {
                    xarr[i] = (double)RampPeriodValue.Value;
                    yarr[i] = yarr[i - 1];
                }

            OneSegmentChart.Series[0].Points.DataBindXY(xarr, yarr);

            OneSegmentChart.ChartAreas[0].AxisX.Minimum = xarr.First();
            OneSegmentChart.ChartAreas[0].AxisX.Maximum = xarr.Last();

            OneSegmentChart.ChartAreas[0].AxisX.Interval = (xarr.Last() - xarr.First()) / 10;
            //OneSegmentChart.ChartAreas[0].AxisY.Interval = 0.1;
        }

        private void redraw_ramp_wholegraph(string name = "")
        {
            double segments = (double)SignalDurationValue.Value / (double)RampPeriodValue.Value;
            if (segments <= 0)
                return;

            int points_per_segment = 3,
                chart_points = (int)Math.Ceiling(segments) * points_per_segment + 1, i;

            Series series;
            if (name == "")
            {
                if (ChannelsListBox.selected_item < 0 || ChannelsListBox.Items.Count <= 0)
                    return;
                try
                {
                    series = ChannelsListBox.selected_item > 0 ? WholeSignalChart.Series[ChannelsListBox.Items
                        [ChannelsListBox.selected_item].ToString()] : WholeSignalChart.Series[0];
                }
                catch
                {
                    return;
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
                xarr[2] = (double)RampPeriodValue.Value, i = points_per_segment; i < chart_points - 1; ++i)
            {
                switch (i % points_per_segment)
                {
                    case 0:
                        xarr[i] = xarr[i - points_per_segment] + (double)RampPeriodValue.Value;
                        break;
                    case 1:
                        xarr[i] = xarr[i - points_per_segment] + (double)RampPeriodValue.Value;
                        break;
                    case 2:
                        xarr[i] = xarr[i - points_per_segment] + (double)RampPeriodValue.Value;
                        break;
                    default:
                        break;
                }
            }

            for (yarr[0] = ((double)RampPeakTrackBar.Value / 100 <= 0) ? (double)RampPeakValue.Value : (double)RampBeginValue.Value,
                yarr[1] = ((double)RampPeakTrackBar.Value / 100 >= 0) ? (double)RampPeakValue.Value : (double)RampBeginValue.Value,
                yarr[2] = ((double)RampPeakTrackBar.Value / 100 * (double)RampPeriodValue.Value >= (double)RampPeriodValue.Value) ?
                            (double)RampPeakValue.Value : (double)RampBeginValue.Value, i = points_per_segment; i < chart_points - 1; ++i)
            {
                switch (i % points_per_segment)
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

            for (i = 0; i < chart_points; ++i)
                if ((decimal)xarr[i] > SignalDurationValue.Value)
                {
                    double time_part = xarr[i] - xarr[i - 1],
                        time_to_fill = ((double)SignalDurationValue.Value - xarr[i - 1]) / time_part;

                    yarr[i] = yarr[i - 1] + (yarr[i] - yarr[i - 1]) * time_to_fill;

                    xarr[i] = (double)SignalDurationValue.Value;

                    break;
                }
            for (; i < chart_points; ++i)
                if ((decimal)xarr[i] > SignalDurationValue.Value)
                {
                    xarr[i] = (double)SignalDurationValue.Value;
                    yarr[i] = yarr[i - 1];
                }

            if (ZeroEndingRampCheckBox.Checked)
            {
                xarr[xarr.Length - 1] = (double)SignalDurationValue.Value;
                yarr[yarr.Length - 1] = 0;
            }

            for (i = xarr.Length - 1; i > points_per_segment - 1; --i)
                if (xarr[i] == 0)
                {
                    xarr[i] = (double)SignalDurationValue.Value;
                    yarr[i] = yarr[i - 1];
                }

            series.Points.DataBindXY(xarr, yarr);

            WholeSignalChart.ChartAreas[0].AxisX.Minimum = 0;
            WholeSignalChart.ChartAreas[0].AxisX.Maximum = (double)SignalDurationValue.Value;
            WholeSignalChart.ChartAreas[0].AxisY.Minimum = Double.NaN;
            WholeSignalChart.ChartAreas[0].AxisY.Maximum = Double.NaN;

            WholeSignalChart.ChartAreas[0].AxisX.Interval = (double)SignalDurationValue.Value / 10;
            //WholeSignalChart.ChartAreas[0].AxisY.Interval = 0.1;
        }

        private void SaveRampChannelButton_Click(object sender, EventArgs e)
        {
            SaveAllChannelsProgress.Style = ProgressBarStyle.Marquee;

            BackgroundWorker bgw = new BackgroundWorker();
            bgw.WorkerSupportsCancellation = true;  
            bgw.DoWork += new DoWorkEventHandler(bgw_SaveOneChannel);
            bgw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgw_WorkComplete);

            IniFile MyIni = new IniFile(ini_path);
            MyIni.Write("SignalDuration", SignalDurationValue.Value.ToString(), "Main");
            MyIni.Write("SamplingRate", SamplingRateValue.Value.ToString(), "Main");

            ch_name_to_save = RampChannelName.Text;
            write_ini_channel(RampChannelName.Text);
            double[] r = new double[0];
            save_csv_channel(RampChannelName.Text, ref r);
            MyIni.Write("Index", ChannelsListBox.Items.IndexOf(RampChannelName.Text).ToString(), RampChannelName.Text);

            bgw.RunWorkerAsync();

            ChannelsListBox.clear_selection();
            ChannelsListBox.selected_item = ChannelsListBox.FindByName(RampChannelName.Text);
        }

        private void ZeroEndingRampCheckBox_CheckedChange(object sender, EventArgs e)
        {
            ramp_graph(sender, e);
        }

        private void DeleteRampChannelButton_Click(object sender, EventArgs e)
        {            
            SaveAllChannelsProgress.Style = ProgressBarStyle.Marquee;

            BackgroundWorker bgw = new BackgroundWorker();
            bgw.DoWork += new DoWorkEventHandler(bgw_SaveAllChannels);
            bgw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgw_WorkComplete);

            IniFile MyIni = new IniFile(ini_path);
            MyIni.Write("SignalDuration", SignalDurationValue.Value.ToString(), "Main");
            MyIni.Write("SamplingRate", SamplingRateValue.Value.ToString(), "Main");

            MyIni.DeleteSection(RampChannelName.Text);
            delete_csv_channel(RampChannelName.Text);

            bgw.RunWorkerAsync();

            ChannelsListBox.clear_selection();
            ChannelsListBox.selected_item = 0;
        }

        #endregion

        #region ImpulseTab

        private void impulse_graph(object sender, EventArgs e)
        {
            if (!is_drawing_busy)
            {
                is_drawing_busy = true;
                redraw_impulse();
                redraw_impulse_wholegraph();
            }
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

            for (int i = 0; i < xarr.Count(); ++i)
                if (xarr[i] > (double)ImpulsePeriodValue.Value)
                {
                    xarr[i] = (double)ImpulsePeriodValue.Value;
                    yarr[i] = yarr[i - 1];
                }

            OneSegmentChart.Series[0].Points.DataBindXY(xarr, yarr);

            OneSegmentChart.ChartAreas[0].AxisX.Minimum = xarr.First();
            OneSegmentChart.ChartAreas[0].AxisX.Maximum = xarr.Last();

            OneSegmentChart.ChartAreas[0].AxisX.Interval = (xarr.Last() - xarr.First()) / 10;
            //OneSegmentChart.ChartAreas[0].AxisY.Interval = 0.1;
        }

        private void redraw_impulse_wholegraph(string name = "")
        {
            double segments = (double)SignalDurationValue.Value / (double)ImpulsePeriodValue.Value;
            if (segments <= 0)
                return;

            int points_per_segment = 6,
                chart_points = (int)Math.Ceiling(segments) * points_per_segment + 1, i;

            Series series;
            if (name == "")
            {
                if (ChannelsListBox.selected_item < 0 || ChannelsListBox.Items.Count <= 0)
                    return;
                try
                {
                    series = ChannelsListBox.selected_item > 0 ? WholeSignalChart.Series[ChannelsListBox.Items
                        [ChannelsListBox.selected_item].ToString()] : WholeSignalChart.Series[0];
                }
                catch
                {
                    return;
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
                xarr[5] = (double)ImpulsePeriodValue.Value, i = points_per_segment; i < chart_points - 1; ++i)
            {
                switch (i % points_per_segment)
                {
                    case 0:
                        xarr[i] = xarr[i - points_per_segment] + (double)ImpulsePeriodValue.Value;
                        break;
                    case 1:
                        xarr[i] = xarr[i - points_per_segment] + (double)ImpulsePeriodValue.Value;
                        break;
                    case 2:
                        xarr[i] = xarr[i - points_per_segment] + (double)ImpulsePeriodValue.Value;
                        break;
                    case 3:
                        xarr[i] = xarr[i - points_per_segment] + (double)ImpulsePeriodValue.Value;
                        break;
                    case 4:
                        xarr[i] = xarr[i - points_per_segment] + (double)ImpulsePeriodValue.Value;
                        break;
                    case 5:
                        xarr[i] = xarr[i - points_per_segment] + (double)ImpulsePeriodValue.Value;
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
                            (double)ImpulseLevelValue.Value : (double)ImpulseBaseValue.Value, i = points_per_segment; i < chart_points - 1; ++i)
            {
                switch (i % points_per_segment)
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

            for (i = 0; i < chart_points; ++i)
                if (xarr[i] > (double)SignalDurationValue.Value)
                {
                    xarr[i] = (double)SignalDurationValue.Value;
                    yarr[i] = yarr[i - 1];
                }

            if (ZeroEndingImpulseCheckBox.Checked)
            {
                xarr[xarr.Length - 1] = (double)SignalDurationValue.Value;
                yarr[yarr.Length - 1] = 0;
            }

            for (i = xarr.Length - 1; i > points_per_segment - 1; --i)
                if (xarr[i] == 0)
                {
                    xarr[i] = (double)SignalDurationValue.Value;
                    yarr[i] = yarr[i - 1];
                }

            series.Points.DataBindXY(xarr, yarr);

            WholeSignalChart.ChartAreas[0].AxisX.Minimum = 0;
            WholeSignalChart.ChartAreas[0].AxisX.Maximum = (double)SignalDurationValue.Value;
            WholeSignalChart.ChartAreas[0].AxisY.Minimum = Double.NaN;
            WholeSignalChart.ChartAreas[0].AxisY.Maximum = Double.NaN;

            WholeSignalChart.ChartAreas[0].AxisX.Interval = (double)SignalDurationValue.Value / 10;
            //WholeSignalChart.ChartAreas[0].AxisY.Interval = 0.1;
        }

        private void ImpulsePeriodValue_ValueChanged(object sender, EventArgs e)
        {
            SeveralSlidersImpulseTrackBar.Max = ImpulsePeriodValue.Value;
            SignalDurationValue.Minimum = ImpulsePeriodValue.Value;
            ImpulsePeriodValue.Maximum = SignalDurationValue.Value;

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
            SaveAllChannelsProgress.Style = ProgressBarStyle.Marquee;

            BackgroundWorker bgw = new BackgroundWorker();
            bgw.DoWork += new DoWorkEventHandler(bgw_SaveOneChannel);
            bgw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgw_WorkComplete);

            IniFile MyIni = new IniFile(ini_path);
            MyIni.Write("SignalDuration", SignalDurationValue.Value.ToString(), "Main");
            MyIni.Write("SamplingRate", SamplingRateValue.Value.ToString(), "Main");

            ch_name_to_save = ImpulseChannelName.Text;
            write_ini_channel(ImpulseChannelName.Text);
            double[] r = new double[0];
            save_csv_channel(ImpulseChannelName.Text, ref r);
            MyIni.Write("Index", ChannelsListBox.Items.IndexOf(ImpulseChannelName.Text).ToString(), ImpulseChannelName.Text);

            bgw.RunWorkerAsync();

            ChannelsListBox.clear_selection();
            ChannelsListBox.selected_item = ChannelsListBox.FindByName(ImpulseChannelName.Text);
        }

        private void ZeroEndingImpulseCheckBox_CheckedChange(object sender, EventArgs e)
        {
            impulse_graph(sender, e);
        }

        private void DeleteImpulseChannelButton_Click(object sender, EventArgs e)
        {
            SaveAllChannelsProgress.Style = ProgressBarStyle.Marquee;

            BackgroundWorker bgw = new BackgroundWorker();
            bgw.DoWork += new DoWorkEventHandler(bgw_SaveAllChannels);
            bgw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgw_WorkComplete);

            IniFile MyIni = new IniFile(ini_path);
            MyIni.Write("SignalDuration", SignalDurationValue.Value.ToString(), "Main");
            MyIni.Write("SamplingRate", SamplingRateValue.Value.ToString(), "Main");

            MyIni.DeleteSection(ImpulseChannelName.Text);
            delete_csv_channel(ImpulseChannelName.Text);

            bgw.RunWorkerAsync();

            ChannelsListBox.clear_selection();
            ChannelsListBox.selected_item = 0;
        }

        #endregion

        #region CustomTab

        private void custom_graph(object sender, EventArgs e)
        {
            if (!is_drawing_busy)
            {
                is_drawing_busy = true;
                redraw_custom();
                redraw_custom_wholegraph();
            }
        }

        private void redraw_custom()
        {
            OneSegmentChart.Series[0].Points.Clear();
            OneSegmentChart.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;

            DataPoint[] dataset = pointCoordinatesListBox.Get_DataSet();
            if (dataset == null)
                return;

            double[] xarr = new double[dataset.Count()];
            double[] yarr = new double[dataset.Count()];

            for (int i = 0; i < dataset.Count(); ++i)
            {
                xarr[i] = dataset[i].XValue;
                yarr[i] = dataset[i].YValues[0];
            }

            for (int i = 0; i < xarr.Count(); ++i)
                if (xarr[i] > (double)CustomPeriodValue.Value)
                {
                    xarr[i] = (double)CustomPeriodValue.Value;
                    yarr[i] = yarr[i - 1];
                }

            OneSegmentChart.Series[0].Points.DataBindXY(xarr, yarr);

            OneSegmentChart.ChartAreas[0].AxisX.Minimum = xarr.First();
            OneSegmentChart.ChartAreas[0].AxisX.Maximum = xarr.Last();

            OneSegmentChart.ChartAreas[0].AxisX.Interval = (xarr.Last() - xarr.First()) / 10;
            //OneSegmentChart.ChartAreas[0].AxisY.Interval = 0.1;
        }

        private void redraw_custom_wholegraph(string name = "")
        {
            double segments = (double)SignalDurationValue.Value / (double)CustomPeriodValue.Value;
            if (segments <= 0)
                return;

            DataPoint[] dataset = pointCoordinatesListBox.Get_DataSet();
            if (dataset == null)
                return;

            int points_per_segment = dataset.Count(),
                chart_points = (int)Math.Ceiling(segments) * points_per_segment + 1, i, j;

            Series series;
            if (name == "")
            {
                if (ChannelsListBox.selected_item < 0 || ChannelsListBox.Items.Count <= 0)
                    return;
                try
                {
                    series = ChannelsListBox.selected_item > 0 ? WholeSignalChart.Series[ChannelsListBox.Items
                        [ChannelsListBox.selected_item].ToString()] : WholeSignalChart.Series[0];
                }
                catch
                {
                    return;
                }
            }
            else series = WholeSignalChart.Series.FindByName(name);
            if (series == null)
                return;

            series.Points.Clear();
            series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;

            double[] xarr = new double[chart_points];
            double[] yarr = new double[chart_points];

            for (i = 0; i < points_per_segment; ++i)
            {
                xarr[i] = dataset[i].XValue;
            }

            for (i = points_per_segment; i < chart_points - 1; ++i)
            {
                xarr[i] = xarr[i - points_per_segment] + (double)CustomPeriodValue.Value;
            }

            for (i = 0, j = 0; i < chart_points - 1; ++i, j = j < points_per_segment - 1 ? ++j : 0)
            {
                yarr[i] = dataset[j].YValues[0];
            }

            for (i = 0; i < chart_points; ++i)
                if ((decimal)xarr[i] > SignalDurationValue.Value)
                {
                    double time_part = xarr[i] - xarr[i - 1],
                        time_to_fill = ((double)SignalDurationValue.Value - xarr[i - 1]) / time_part;

                    yarr[i] = yarr[i - 1] + (yarr[i] - yarr[i - 1]) * time_to_fill;
                    
                    xarr[i] = (double)SignalDurationValue.Value;

                    break;
                }
            for (; i < chart_points; ++i)
                if ((decimal)xarr[i] > SignalDurationValue.Value)
                {
                    xarr[i] = (double)SignalDurationValue.Value;
                    yarr[i] = yarr[i - 1];
                }

            if (ZeroEndingCustomCheckBox.Checked)
            {
                xarr[xarr.Length - 1] = (double)SignalDurationValue.Value;
                yarr[yarr.Length - 1] = 0;
            }

            for (i = xarr.Length - 1; i > points_per_segment - 1; --i)
                if (xarr[i] == 0)
                {
                    xarr[i] = (double)SignalDurationValue.Value;
                    yarr[i] = yarr[i - 1];
                }

            series.Points.DataBindXY(xarr, yarr);

            WholeSignalChart.ChartAreas[0].AxisX.Minimum = 0;
            WholeSignalChart.ChartAreas[0].AxisX.Maximum = (double)SignalDurationValue.Value;
            WholeSignalChart.ChartAreas[0].AxisY.Minimum = Double.NaN;
            WholeSignalChart.ChartAreas[0].AxisY.Maximum = Double.NaN;

            WholeSignalChart.ChartAreas[0].AxisX.Interval = (double)SignalDurationValue.Value / 10;
            //WholeSignalChart.ChartAreas[0].AxisY.Interval = 0.1;
        }

        private void CustomPeriodValue_ValueChanged(object sender, EventArgs e)
        {
            DataPoint[] old_dataset = pointCoordinatesListBox.Get_DataSet(),
                new_dataset = old_dataset;

            if (old_dataset == null)
                return;

            for (int i = 0; i < new_dataset.Count(); ++i)
            {
                new_dataset[i].XValue = (double)CustomPeriodValue.Value / (old_dataset[old_dataset.Count() - 1].XValue - old_dataset[0].XValue)
                    * old_dataset[i].XValue;
            }

            pointCoordinatesListBox.Set_DataSet(new_dataset);

            custom_graph(sender, e);
        }

        private void SaveCustomChannelButton_Click(object sender, EventArgs e)
        {
            SaveAllChannelsProgress.Style = ProgressBarStyle.Marquee;

            BackgroundWorker bgw = new BackgroundWorker();
            bgw.DoWork += new DoWorkEventHandler(bgw_SaveOneChannel);
            bgw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgw_WorkComplete);

            IniFile MyIni = new IniFile(ini_path);
            MyIni.Write("SignalDuration", SignalDurationValue.Value.ToString(), "Main");
            MyIni.Write("SamplingRate", SamplingRateValue.Value.ToString(), "Main");

            ch_name_to_save = CustomChannelName.Text;
            write_ini_channel(CustomChannelName.Text);
            double[] r = new double[0];
            save_csv_channel(CustomChannelName.Text, ref r);
            MyIni.Write("Index", ChannelsListBox.Items.IndexOf(CustomChannelName.Text).ToString(), CustomChannelName.Text);

            bgw.RunWorkerAsync();

            ChannelsListBox.clear_selection();
            ChannelsListBox.selected_item = ChannelsListBox.FindByName(CustomChannelName.Text);
        }

        private void ZeroEndingCustomCheckBox_CheckedChange(object sender, EventArgs e)
        {
            custom_graph(sender, e);
        }

        private void DeleteCustomChannelButton_Click(object sender, EventArgs e)
        {
            SaveAllChannelsProgress.Style = ProgressBarStyle.Marquee;

            BackgroundWorker bgw = new BackgroundWorker();
            bgw.DoWork += new DoWorkEventHandler(bgw_SaveAllChannels);
            bgw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgw_WorkComplete);

            IniFile MyIni = new IniFile(ini_path);
            MyIni.Write("SignalDuration", SignalDurationValue.Value.ToString(), "Main");
            MyIni.Write("SamplingRate", SamplingRateValue.Value.ToString(), "Main");

            MyIni.DeleteSection(CustomChannelName.Text);
            delete_csv_channel(CustomChannelName.Text);

            bgw.RunWorkerAsync();

            ChannelsListBox.clear_selection();
            ChannelsListBox.selected_item = 0;
        }

        #endregion

        #region SelectionRangeSlider

        private void SelectionRangeSlider_MouseClickMin(object sender, EventArgs e)
        {
            nud_Min.DecimalPlaces = MathDecimals.GetDecimalPlaces(SeveralSlidersImpulseTrackBar.Max / 100);

            var buff = (decimal)SeveralSlidersImpulseTrackBar.SelectedMin;
            nud_Min.Minimum = (decimal)SeveralSlidersImpulseTrackBar.Min;
            nud_Min.Maximum = (decimal)SeveralSlidersImpulseTrackBar.SelectedMax;
            nud_Min.Value = buff;

            nud_Min.Location = PointToClient(MousePosition).X + nud_Min.Width < this.Width ? 
                new Point(PointToClient(MousePosition).X, PointToClient(MousePosition).Y) :
                new Point(PointToClient(MousePosition).X - nud_Min.Width, PointToClient(MousePosition).Y);

            nud_Min.Show();
        }

        private void SelectionRangeSlider_MouseClickMax(object sender, EventArgs e)
        {
            nud_Max.DecimalPlaces = MathDecimals.GetDecimalPlaces(SeveralSlidersImpulseTrackBar.Max / 100);

            var buff = (decimal)SeveralSlidersImpulseTrackBar.SelectedMax;
            nud_Max.Minimum = (decimal)SeveralSlidersImpulseTrackBar.SelectedMin;
            nud_Max.Maximum = (decimal)SeveralSlidersImpulseTrackBar.Max;
            nud_Max.Value = buff;

            nud_Max.Location = PointToClient(MousePosition).X + nud_Max.Width < this.Width ?
                new Point(PointToClient(MousePosition).X, PointToClient(MousePosition).Y) :
                new Point(PointToClient(MousePosition).X - nud_Max.Width, PointToClient(MousePosition).Y);

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
            SeveralSlidersImpulseTrackBar.SelectedMin = nud_Min.Value;
        }

        private void nud_Max_ValueChange(object sender, EventArgs e)
        {
            SeveralSlidersImpulseTrackBar.SelectedMax = nud_Max.Value;
        }

        #endregion

        #region ChannelsList

        private void ChannelsListBox_DeleteItem(object sender, EventArgs e)
        {
            switch (SignalTypeTabControl.SelectedIndex)
            {
                case 0:
                    DeleteRampChannelButton_Click(sender, e);
                    break;
                case 1:
                    DeleteImpulseChannelButton_Click(sender, e);
                    break;
                case 2:
                    break;
                case 3:
                    DeleteCustomChannelButton_Click(sender, e);
                    break;
                default:
                    break;
            }
        }

        private void ChannelsListBox_RenameItem(string old_name, string new_name)
        {
            Channels[ChannelsListBox.selected_item].name = new_name;
            WholeSignalChart.Series[old_name].Name = new_name;
            WholeSignalChart.settings[WholeSignalChart.FindSettingIndexByName(old_name)].name = new_name;

            IniFile MyIni = new IniFile(ini_path);
            MyIni.DeleteSection(old_name);
            write_ini_channel(new_name); 
        }

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
                if (Channels[i] != null)
                    s[i] = Channels[i].name;

            ChannelsListBox.Items.Clear();
            ChannelsListBox.Items.AddRange(s);
        }

        private void ChannelsListBox_SelectionChanged(object sender, EventArgs e)
        {
            if (ChannelsListBox.selected_item < 0 || ChannelsListBox.Items.Count <= 0)
            {
                WholeSignalChart.SetSelectedSeries(ChannelsListBox.selected_item);
                return;
            }

            is_from_listbox = true;

            read_ini_channel(Channels[ChannelsListBox.selected_item].name);

            try
            {
                RampChannelName.Text = Channels[ChannelsListBox.selected_item].name;
                ImpulseChannelName.Text = Channels[ChannelsListBox.selected_item].name;
                CustomChannelName.Text = Channels[ChannelsListBox.selected_item].name;

                OneSegmentChart.Series[0].Name = Channels[ChannelsListBox.selected_item].name;
                OneSegmentChart.Series[0].Color = Color.FromArgb(255, WholeSignalChart.Series[ChannelsListBox.Items
                        [ChannelsListBox.selected_item].ToString()].Color);
                OneSegmentChart.ChartAreas[0].AxisY.Minimum = Double.NaN;
                OneSegmentChart.ChartAreas[0].AxisY.Maximum = Double.NaN;

                WholeSignalChart.SetSelectedSeries(WholeSignalChart.Series.IndexOf(Channels[ChannelsListBox.selected_item].name));
            }
            catch
            {
                is_from_listbox = false;
                return;
            }

            is_from_listbox = false;
        }

        private void ChannelsListBox_MoveSelectedUp(object sender, EventArgs e)
        {
            SaveAllChannelsProgress.Style = ProgressBarStyle.Marquee;

            int si = ChannelsListBox.get_item_to_move();

            BackgroundWorker bgw = new BackgroundWorker();
            bgw.DoWork += new DoWorkEventHandler(bgw_SaveAllChannels);
            bgw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgw_WorkComplete);

            IniFile MyIni = new IniFile(ini_path);
            MyIni.Write("SignalDuration", SignalDurationValue.Value.ToString(), "Main");
            MyIni.Write("SamplingRate", SamplingRateValue.Value.ToString(), "Main");
            MyIni.Write("Index", (si - 1).ToString(), ChannelsListBox.Items[si - 1].ToString());
            MyIni.Write("Index", (si).ToString(), ChannelsListBox.Items[si].ToString());

            this.Swap(ref Channels, si, si - 1);

            bgw.RunWorkerAsync();
        }

        private void ChannelsListBox_MoveSelectedDown(object sender, EventArgs e)
        {
            SaveAllChannelsProgress.Style = ProgressBarStyle.Marquee;

            int si = ChannelsListBox.get_item_to_move();

            BackgroundWorker bgw = new BackgroundWorker();
            bgw.DoWork += new DoWorkEventHandler(bgw_SaveAllChannels);
            bgw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgw_WorkComplete);

            IniFile MyIni = new IniFile(ini_path);
            MyIni.Write("SignalDuration", SignalDurationValue.Value.ToString(), "Main");
            MyIni.Write("SamplingRate", SamplingRateValue.Value.ToString(), "Main");
            MyIni.Write("Index", (si + 1).ToString(), ChannelsListBox.Items[si + 1].ToString());
            MyIni.Write("Index", (si).ToString(), ChannelsListBox.Items[si].ToString());

            this.Swap(ref Channels, si, si + 1);

            bgw.RunWorkerAsync();
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
                position_ss.X < SeveralSlidersImpulseTrackBar.sMin.X + (decimal)SeveralSlidersImpulseTrackBar.sMin.rect.Width + nud_Min.Height &&
                position_ss.Y > SeveralSlidersImpulseTrackBar.sMin.Y - nud_Min.Height &&
                position_ss.Y < SeveralSlidersImpulseTrackBar.sMin.Y + (decimal)SeveralSlidersImpulseTrackBar.sMin.rect.Height + nud_Min.Height))
                SelectionRangeSlider_CursorLeftMin(sender, e);

            if (!(position.X > nud_Max.Location.X - nud_Max.Height &&
                position.X < nud_Max.Location.X + nud_Max.Width + nud_Max.Height &&
                position.Y > nud_Max.Location.Y - nud_Max.Height &&
                position.Y < nud_Max.Location.Y + nud_Max.Height + nud_Max.Height)
                &&
                !(position_ss.X > SeveralSlidersImpulseTrackBar.sMax.X - nud_Max.Height &&
                position_ss.X < SeveralSlidersImpulseTrackBar.sMax.X + (decimal)SeveralSlidersImpulseTrackBar.sMax.rect.Width + nud_Max.Height &&
                position_ss.Y > SeveralSlidersImpulseTrackBar.sMax.Y - nud_Max.Height &&
                position_ss.Y < SeveralSlidersImpulseTrackBar.sMax.Y + (decimal)SeveralSlidersImpulseTrackBar.sMax.rect.Height + nud_Max.Height))
                SelectionRangeSlider_CursorLeftMax(sender, e);
        }

        private void OnMouseClick(object sender, EventArgs e)
        {
            var position = PointToClient(MousePosition);
            var position_ss = SeveralSlidersImpulseTrackBar.PointToClient(MousePosition);

            if (!(position.X > nud_Min.Location.X && position.X < nud_Min.Location.X + nud_Min.Width &&
                position.Y > nud_Min.Location.Y && position.Y < nud_Min.Location.Y + nud_Min.Height)
                &&
                !(position_ss.X > SeveralSlidersImpulseTrackBar.sMin.X &&
                position_ss.X < SeveralSlidersImpulseTrackBar.sMin.X + (decimal)SeveralSlidersImpulseTrackBar.sMin.rect.Width &&
                position_ss.Y > SeveralSlidersImpulseTrackBar.sMin.Y &&
                position_ss.Y < SeveralSlidersImpulseTrackBar.sMin.Y + (decimal)SeveralSlidersImpulseTrackBar.sMin.rect.Height))
                nud_Min.Hide();

            if (!(position.X > nud_Max.Location.X && position.X < nud_Max.Location.X + nud_Max.Width &&
                position.Y > nud_Max.Location.Y && position.Y < nud_Max.Location.Y + nud_Max.Height)
                &&
                !(position_ss.X > SeveralSlidersImpulseTrackBar.sMax.X &&
                position_ss.X < SeveralSlidersImpulseTrackBar.sMax.X + (decimal)SeveralSlidersImpulseTrackBar.sMax.rect.Width &&
                position_ss.Y > SeveralSlidersImpulseTrackBar.sMax.Y &&
                position_ss.Y < SeveralSlidersImpulseTrackBar.sMax.Y + (decimal)SeveralSlidersImpulseTrackBar.sMax.rect.Height))
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
                for (data[i++] = y, ++j; j < l_slope_resolution && i < data.Length; data[i] = data[i - 1] + l_increment, ++i, ++j) ;
                for (; j < points_per_segment && i < data.Length; data[i] = data[i - 1] + r_increment, ++i, ++j) ;
            }

            if (controls[6] == 1)
                data[data.Length - 1] = 0;

            return 0;
        }

        private int calculate_impulse_channel(ref double[] data, ref double[] controls)
        {
            data = new double[(int)(controls[0] * controls[1])];

            double segments = controls[0] / controls[2];
            int points_per_segment = (int)((double)data.Length / segments), i, j, k;
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

        private int calculate_custom_channel(ref double[] data, ref double[] controls, ref DataPoint[] points)
        {
            if (points == null)
                return - 1;

            data = new double[(int)(controls[0] * controls[1])];

            double segments = controls[0] / controls[2];
            int points_per_segment = (int)((double)data.Length / segments), i, j, k, h;
            if (points_per_segment <= 0)
                return -1;

            for (i = 0, j = 0, k = 1; ; ++k)
            {
                if (k >= points.Count()) k = 1;
                if (j >= points_per_segment) j = 0;
                int points_between = (int)Math.Ceiling((points[k].XValue - points[k - 1].XValue) /
                    (points[points.Count() - 1].XValue - points[0].XValue) * points_per_segment);
                double increment = (points[k].YValues[0] - points[k - 1].YValues[0]) / points_between;
                for (h = 0, data[i] = points[k - 1].YValues[0], ++i, ++j; h < points_between; ++i, ++h, ++j)
                    if (j < points_per_segment && i < data.Count())
                        data[i] = data[i - 1] + increment;
                    else break;
                if (i >= data.Count() - 1)
                    break;
            }

            if (controls[3] == 1)
                data[data.Length - 1] = 0;

            return 0;
        }

        #endregion

        #region SaveAllChannels

        private void SaveAllChannelsButton_Click(object sender, EventArgs e)
        {
            SaveAllChannelsProgress.Style = ProgressBarStyle.Marquee;

            BackgroundWorker bgw = new BackgroundWorker();
            bgw.DoWork += new DoWorkEventHandler(bgw_SaveAllChannels);
            bgw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgw_WorkComplete);

            IniFile MyIni = new IniFile(ini_path);
            MyIni.Write("SignalDuration", SignalDurationValue.Value.ToString(), "Main");
            MyIni.Write("SamplingRate", SamplingRateValue.Value.ToString(), "Main");

            bgw.RunWorkerAsync();
        }

        private void bgw_SaveAllChannels(object sender, DoWorkEventArgs e)
        {
            IniFile MyIni = new IniFile(ini_path);

            for (int i = 0; i < Channels.Count; ++i)
            {
                if (!MyIni.KeyExists("Type", Channels[i].name))
                    continue;

                double[] data = new double[0];

                switch (Convert.ToInt32(MyIni.Read("Type", Channels[i].name)))
                {
                    case 0:
                        {
                            MyIni.Write("Index", ChannelsListBox.Items.IndexOf(Channels[i].name).ToString(), Channels[i].name);
                            double[] controls = {Convert.ToDouble(MyIni.Read("SignalDuration", "Main")),
                                                Convert.ToDouble(MyIni.Read("SamplingRate", "Main")),
                                                Convert.ToDouble(MyIni.Read("Period", Channels[i].name)),
                                                (int)(Convert.ToDecimal(RampPeakTrackerLabel.Text) /
                                                Convert.ToDecimal(RampPeriodValue.Value) * 100),
                                                Convert.ToDouble(MyIni.Read("BeginValue", Channels[i].name)),
                                                Convert.ToDouble(MyIni.Read("PeakValue", Channels[i].name)),
                                                Convert.ToInt32(MyIni.Read("IsZeroEnding", Channels[i].name)) == 1 ? 1 : 0};

                            if (calculate_ramp_channel(ref data, ref controls) == 0)
                                save_csv_channel(Channels[i].name, ref data);
                            break;
                        }
                    case 1:
                        {
                            MyIni.Write("Index", ChannelsListBox.Items.IndexOf(Channels[i].name).ToString(), Channels[i].name);
                            double[] controls = {Convert.ToDouble(MyIni.Read("SignalDuration", "Main")),
                                                Convert.ToDouble(MyIni.Read("SamplingRate", "Main")),
                                                Convert.ToDouble(MyIni.Read("Period", Channels[i].name)),
                                                Convert.ToDouble(MyIni.Read("LeftPos", Channels[i].name)),
                                                Convert.ToDouble(MyIni.Read("RightPos", Channels[i].name)),
                                                Convert.ToDouble(MyIni.Read("Min", Channels[i].name)),
                                                Convert.ToDouble(MyIni.Read("Max", Channels[i].name)),
                                                Convert.ToDouble(MyIni.Read("BaseValue", Channels[i].name)),
                                                Convert.ToDouble(MyIni.Read("LevelValue", Channels[i].name)),
                                                Convert.ToInt32(MyIni.Read("IsZeroEnding", Channels[i].name)) == 1 ? 1 : 0};

                            if (calculate_impulse_channel(ref data, ref controls) == 0)
                                save_csv_channel(Channels[i].name, ref data);
                            break;
                        }
                    case 2:
                        break;
                    case 3:
                        {
                            MyIni.Write("Index", ChannelsListBox.Items.IndexOf(Channels[i].name).ToString(), Channels[i].name);
                            double[] controls = {Convert.ToDouble(MyIni.Read("SignalDuration", "Main")),
                                                Convert.ToDouble(MyIni.Read("SamplingRate", "Main")),
                                                Convert.ToDouble(MyIni.Read("Period", Channels[i].name)),
                                                Convert.ToInt32(MyIni.Read("IsZeroEnding", Channels[i].name)) == 1 ? 1 : 0};

                            DataPoint[] dataset = new DataPoint[Convert.ToInt32(MyIni.Read("Length", Channels[i].name))];

                            for (int j = 0; j < dataset.Count(); ++j)
                                dataset[j] = new DataPoint(Convert.ToDouble(MyIni.Read("XP" + j, Channels[i].name)),
                                    Convert.ToDouble(MyIni.Read("YP" + j, Channels[i].name)));

                            if (calculate_custom_channel(ref data, ref controls, ref dataset) == 0)
                                save_csv_channel(Channels[i].name, ref data);
                        }
                        break;
                    default:
                        return;
                }
            }
            
            write_csv_file();
        }

        private void bgw_WorkComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            SaveAllChannelsProgress.Style = ProgressBarStyle.Continuous;
        }

        #endregion

        #region MenuStrip

        private void cSVFilePathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                InitialDirectory = csv_path.Remove(csv_path.LastIndexOf('\\') + 1, csv_file_name.Count()),
                DefaultExt = ".csv",
                Filter = "csv files (*.csv)|*.csv",
                AddExtension = true,
                Multiselect = false,
                SupportMultiDottedExtensions = false
            };
            if (ofd.ShowDialog() != DialogResult.OK ||
                !ofd.SafeFileName.EndsWith(".csv"))
                return;

            csv_path = ofd.FileName;
            csv_file_name = ofd.SafeFileName;

            RegistryKey key = Registry.CurrentUser.CreateSubKey("SignalMaker", true);
            key.SetValue("CSV", csv_path);

            this.cSVFilePathToolStripMenuItem.ToolTipText = "Current CSV file path: " + csv_path;

            key.Close();

            init_frame(sender, e);

            ChannelsListBox.Invalidate();
        }
        
        private void iNIFilePathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            { 
                InitialDirectory = ini_path.Remove(ini_path.LastIndexOf('\\') + 1, ini_file_name.Count()),
                DefaultExt = ".ini",
                Filter = "ini files (*.ini)|*.ini",
                AddExtension = true,
                Multiselect = false,
                SupportMultiDottedExtensions = false,
                CheckFileExists = false
            };
            if (ofd.ShowDialog() != DialogResult.OK ||
                !ofd.SafeFileName.EndsWith(".ini"))
                return;

            ini_path = ofd.FileName;
            ini_file_name = ofd.SafeFileName;
            
            RegistryKey key = Registry.CurrentUser.CreateSubKey("SignalMaker", true);
            key.SetValue("INI", ini_path);

            this.iNIFilePathToolStripMenuItem.ToolTipText = "Current INI file path: " + ini_path;

            key.Close();

            try
            {
                SignalDurationValue.Value = Convert.ToDecimal(new IniFile(ini_path).Read("SignalDuration", "Main"));
                SamplingRateValue.Value = Convert.ToDecimal(new IniFile(ini_path).Read("SamplingRate", "Main"));
            }
            catch { }

            init_frame(sender, e);

            ChannelsListBox.Invalidate();
        }

        #endregion
    }
}