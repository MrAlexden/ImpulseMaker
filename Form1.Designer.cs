﻿using System;

namespace ImpulseMaker
{
    public partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.SignalTypeTabControl = new System.Windows.Forms.TabControl();
            this.RampTabPage = new System.Windows.Forms.TabPage();
            this.ZeroEndingRampCheckBox = new System.Windows.Forms.CheckBox();
            this.RampChannelName = new System.Windows.Forms.TextBox();
            this.DeleteRampChannelButton = new System.Windows.Forms.Button();
            this.SaveRampChannelButton = new System.Windows.Forms.Button();
            this.RampPeakTrackerLabel = new System.Windows.Forms.Label();
            this.RampPeakTrackBar = new System.Windows.Forms.TrackBar();
            this.RampPeriodValueLabel = new System.Windows.Forms.Label();
            this.SaveRampChannelLabel = new System.Windows.Forms.Label();
            this.RampPeakTrackBarLabel = new System.Windows.Forms.Label();
            this.RampPeakLabel = new System.Windows.Forms.Label();
            this.RampBeginLabel = new System.Windows.Forms.Label();
            this.ImpulseTabPage = new System.Windows.Forms.TabPage();
            this.DeleteImpulseChannelButton = new System.Windows.Forms.Button();
            this.ZeroEndingImpulseCheckBox = new System.Windows.Forms.CheckBox();
            this.ImpulseChannelName = new System.Windows.Forms.TextBox();
            this.SaveImpulseChannelLabel = new System.Windows.Forms.Label();
            this.ImpulsePeriodValueLabel = new System.Windows.Forms.Label();
            this.ImpulseTrackBarLabel = new System.Windows.Forms.Label();
            this.ImpulseLevelLabel = new System.Windows.Forms.Label();
            this.ImpulseBaseLabel = new System.Windows.Forms.Label();
            this.SaveImpulseChannelButton = new System.Windows.Forms.Button();
            this.SineTabPage = new System.Windows.Forms.TabPage();
            this.SinePeriodValueLabel = new System.Windows.Forms.Label();
            this.SineAmplitudeLabel = new System.Windows.Forms.Label();
            this.SineLevelLabel = new System.Windows.Forms.Label();
            this.DeleteSineChannelButton = new System.Windows.Forms.Button();
            this.ZeroEndingSineCheckBox = new System.Windows.Forms.CheckBox();
            this.SineChannelName = new System.Windows.Forms.TextBox();
            this.SaveSineChannelLabel = new System.Windows.Forms.Label();
            this.SaveSineChannelButton = new System.Windows.Forms.Button();
            this.SineAngleTrackerLabel = new System.Windows.Forms.Label();
            this.SineAngleTrackBar = new System.Windows.Forms.TrackBar();
            this.SineAngleTrackBarLabel = new System.Windows.Forms.Label();
            this.CustomTabPage = new System.Windows.Forms.TabPage();
            this.CustomPeriodValueLabel = new System.Windows.Forms.Label();
            this.ZeroEndingCustomCheckBox = new System.Windows.Forms.CheckBox();
            this.DeleteCustomChannelButton = new System.Windows.Forms.Button();
            this.CustomChannelName = new System.Windows.Forms.TextBox();
            this.SaveCustomChannelLabel = new System.Windows.Forms.Label();
            this.SaveCustomChannelButton = new System.Windows.Forms.Button();
            this.SamplingRateLabel = new System.Windows.Forms.Label();
            this.SignalDurationLabel = new System.Windows.Forms.Label();
            this.SaveAllChannelsProgress = new System.Windows.Forms.ProgressBar();
            this.SaveAllChannelsLabel = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.iNIFilePathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cSVFilePathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.toolTip2 = new System.Windows.Forms.ToolTip(this.components);
            this.ChannelsListBox = new ImpulseMaker.MyListBox();
            this.SignalDurationValue = new ImpulseMaker.MyNumericUpDown();
            this.SamplingRateValue = new ImpulseMaker.MyNumericUpDown();
            this.RampBeginValue = new ImpulseMaker.MyNumericUpDown();
            this.RampPeriodValue = new ImpulseMaker.MyNumericUpDown();
            this.RampPeakValue = new ImpulseMaker.MyNumericUpDown();
            this.ImpulsePeriodValue = new ImpulseMaker.MyNumericUpDown();
            this.ImpulseBaseValue = new ImpulseMaker.MyNumericUpDown();
            this.ImpulseLevelValue = new ImpulseMaker.MyNumericUpDown();
            this.SeveralSlidersImpulseTrackBar = new ImpulseMaker.SeveralSlidersTrackBar();
            this.SinePeriodValue = new ImpulseMaker.MyNumericUpDown();
            this.SineAmplitudeValue = new ImpulseMaker.MyNumericUpDown();
            this.SineLevelValue = new ImpulseMaker.MyNumericUpDown();
            this.CustomPeriodValue = new ImpulseMaker.MyNumericUpDown();
            this.pointCoordinatesListBox = new ImpulseMaker.PointCoordinatesListBox();
            this.WholeSignalChart = new ImpulseMaker.MyChart();
            this.OneSegmentChart = new ImpulseMaker.MyChart();
            this.SignalTypeTabControl.SuspendLayout();
            this.RampTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RampPeakTrackBar)).BeginInit();
            this.ImpulseTabPage.SuspendLayout();
            this.SineTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SineAngleTrackBar)).BeginInit();
            this.CustomTabPage.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SignalDurationValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SamplingRateValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.RampBeginValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.RampPeriodValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.RampPeakValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ImpulsePeriodValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ImpulseBaseValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ImpulseLevelValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SinePeriodValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SineAmplitudeValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SineLevelValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CustomPeriodValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.WholeSignalChart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.OneSegmentChart)).BeginInit();
            this.SuspendLayout();
            // 
            // SignalTypeTabControl
            // 
            this.SignalTypeTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SignalTypeTabControl.Controls.Add(this.RampTabPage);
            this.SignalTypeTabControl.Controls.Add(this.ImpulseTabPage);
            this.SignalTypeTabControl.Controls.Add(this.SineTabPage);
            this.SignalTypeTabControl.Controls.Add(this.CustomTabPage);
            this.SignalTypeTabControl.Location = new System.Drawing.Point(822, 34);
            this.SignalTypeTabControl.Multiline = true;
            this.SignalTypeTabControl.Name = "SignalTypeTabControl";
            this.SignalTypeTabControl.SelectedIndex = 0;
            this.SignalTypeTabControl.Size = new System.Drawing.Size(262, 377);
            this.SignalTypeTabControl.TabIndex = 5;
            // 
            // RampTabPage
            // 
            this.RampTabPage.Controls.Add(this.ZeroEndingRampCheckBox);
            this.RampTabPage.Controls.Add(this.RampChannelName);
            this.RampTabPage.Controls.Add(this.DeleteRampChannelButton);
            this.RampTabPage.Controls.Add(this.SaveRampChannelButton);
            this.RampTabPage.Controls.Add(this.RampPeakTrackerLabel);
            this.RampTabPage.Controls.Add(this.RampPeakTrackBar);
            this.RampTabPage.Controls.Add(this.RampPeriodValueLabel);
            this.RampTabPage.Controls.Add(this.SaveRampChannelLabel);
            this.RampTabPage.Controls.Add(this.RampPeakTrackBarLabel);
            this.RampTabPage.Controls.Add(this.RampPeakLabel);
            this.RampTabPage.Controls.Add(this.RampBeginLabel);
            this.RampTabPage.Controls.Add(this.RampBeginValue);
            this.RampTabPage.Controls.Add(this.RampPeriodValue);
            this.RampTabPage.Controls.Add(this.RampPeakValue);
            this.RampTabPage.Location = new System.Drawing.Point(4, 22);
            this.RampTabPage.Name = "RampTabPage";
            this.RampTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.RampTabPage.Size = new System.Drawing.Size(254, 351);
            this.RampTabPage.TabIndex = 1;
            this.RampTabPage.Text = "Пила";
            this.RampTabPage.UseVisualStyleBackColor = true;
            this.RampTabPage.Enter += new System.EventHandler(this.RampTabPage_Enter);
            // 
            // ZeroEndingRampCheckBox
            // 
            this.ZeroEndingRampCheckBox.AutoSize = true;
            this.ZeroEndingRampCheckBox.Checked = true;
            this.ZeroEndingRampCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ZeroEndingRampCheckBox.Location = new System.Drawing.Point(9, 251);
            this.ZeroEndingRampCheckBox.Name = "ZeroEndingRampCheckBox";
            this.ZeroEndingRampCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.ZeroEndingRampCheckBox.Size = new System.Drawing.Size(163, 17);
            this.ZeroEndingRampCheckBox.TabIndex = 14;
            this.ZeroEndingRampCheckBox.Text = "Заканчивать сигнал нулем";
            this.ZeroEndingRampCheckBox.UseVisualStyleBackColor = true;
            this.ZeroEndingRampCheckBox.CheckedChanged += new System.EventHandler(this.ZeroEndingRampCheckBox_CheckedChange);
            // 
            // RampChannelName
            // 
            this.RampChannelName.Location = new System.Drawing.Point(9, 317);
            this.RampChannelName.Name = "RampChannelName";
            this.RampChannelName.Size = new System.Drawing.Size(181, 20);
            this.RampChannelName.TabIndex = 6;
            // 
            // DeleteRampChannelButton
            // 
            this.DeleteRampChannelButton.BackgroundImage = global::ImpulseMaker.Properties.Resources.png_transparent_icon_design_trash_red_line_area_material_rectangle;
            this.DeleteRampChannelButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.DeleteRampChannelButton.Location = new System.Drawing.Point(225, 313);
            this.DeleteRampChannelButton.Name = "DeleteRampChannelButton";
            this.DeleteRampChannelButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.DeleteRampChannelButton.Size = new System.Drawing.Size(25, 25);
            this.DeleteRampChannelButton.TabIndex = 5;
            this.DeleteRampChannelButton.UseVisualStyleBackColor = true;
            this.DeleteRampChannelButton.Click += new System.EventHandler(this.DeleteRampChannelButton_Click);
            // 
            // SaveRampChannelButton
            // 
            this.SaveRampChannelButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("SaveRampChannelButton.BackgroundImage")));
            this.SaveRampChannelButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.SaveRampChannelButton.Location = new System.Drawing.Point(195, 313);
            this.SaveRampChannelButton.Name = "SaveRampChannelButton";
            this.SaveRampChannelButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.SaveRampChannelButton.Size = new System.Drawing.Size(25, 25);
            this.SaveRampChannelButton.TabIndex = 5;
            this.SaveRampChannelButton.UseVisualStyleBackColor = true;
            this.SaveRampChannelButton.Click += new System.EventHandler(this.SaveRampChannelButton_Click);
            // 
            // RampPeakTrackerLabel
            // 
            this.RampPeakTrackerLabel.AutoSize = true;
            this.RampPeakTrackerLabel.Location = new System.Drawing.Point(213, 167);
            this.RampPeakTrackerLabel.Name = "RampPeakTrackerLabel";
            this.RampPeakTrackerLabel.Size = new System.Drawing.Size(22, 13);
            this.RampPeakTrackerLabel.TabIndex = 4;
            this.RampPeakTrackerLabel.Text = "0,5";
            // 
            // RampPeakTrackBar
            // 
            this.RampPeakTrackBar.Location = new System.Drawing.Point(9, 185);
            this.RampPeakTrackBar.Maximum = 100;
            this.RampPeakTrackBar.Name = "RampPeakTrackBar";
            this.RampPeakTrackBar.Size = new System.Drawing.Size(229, 45);
            this.RampPeakTrackBar.TabIndex = 3;
            this.RampPeakTrackBar.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.RampPeakTrackBar.Value = 50;
            this.RampPeakTrackBar.ValueChanged += new System.EventHandler(this.RampPeakTrackBar_ValueChanged);
            this.RampPeakTrackBar.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ramp_graph);
            // 
            // RampPeriodValueLabel
            // 
            this.RampPeriodValueLabel.AutoSize = true;
            this.RampPeriodValueLabel.Location = new System.Drawing.Point(6, 96);
            this.RampPeriodValueLabel.Name = "RampPeriodValueLabel";
            this.RampPeriodValueLabel.Size = new System.Drawing.Size(72, 13);
            this.RampPeriodValueLabel.TabIndex = 2;
            this.RampPeriodValueLabel.Text = "Период, сек:";
            // 
            // SaveRampChannelLabel
            // 
            this.SaveRampChannelLabel.AutoSize = true;
            this.SaveRampChannelLabel.Location = new System.Drawing.Point(6, 301);
            this.SaveRampChannelLabel.Name = "SaveRampChannelLabel";
            this.SaveRampChannelLabel.Size = new System.Drawing.Size(99, 13);
            this.SaveRampChannelLabel.TabIndex = 2;
            this.SaveRampChannelLabel.Text = "Название канала:";
            // 
            // RampPeakTrackBarLabel
            // 
            this.RampPeakTrackBarLabel.AutoSize = true;
            this.RampPeakTrackBarLabel.Location = new System.Drawing.Point(6, 167);
            this.RampPeakTrackBarLabel.Name = "RampPeakTrackBarLabel";
            this.RampPeakTrackBarLabel.Size = new System.Drawing.Size(109, 13);
            this.RampPeakTrackBarLabel.TabIndex = 2;
            this.RampPeakTrackBarLabel.Text = "Положение излома:";
            // 
            // RampPeakLabel
            // 
            this.RampPeakLabel.AutoSize = true;
            this.RampPeakLabel.Location = new System.Drawing.Point(6, 55);
            this.RampPeakLabel.Name = "RampPeakLabel";
            this.RampPeakLabel.Size = new System.Drawing.Size(117, 13);
            this.RampPeakLabel.TabIndex = 2;
            this.RampPeakLabel.Text = "Пиковое значение, В:";
            // 
            // RampBeginLabel
            // 
            this.RampBeginLabel.AutoSize = true;
            this.RampBeginLabel.Location = new System.Drawing.Point(6, 14);
            this.RampBeginLabel.Name = "RampBeginLabel";
            this.RampBeginLabel.Size = new System.Drawing.Size(128, 13);
            this.RampBeginLabel.TabIndex = 2;
            this.RampBeginLabel.Text = "Начальное значение, В:";
            // 
            // ImpulseTabPage
            // 
            this.ImpulseTabPage.Controls.Add(this.DeleteImpulseChannelButton);
            this.ImpulseTabPage.Controls.Add(this.ZeroEndingImpulseCheckBox);
            this.ImpulseTabPage.Controls.Add(this.ImpulseChannelName);
            this.ImpulseTabPage.Controls.Add(this.SaveImpulseChannelLabel);
            this.ImpulseTabPage.Controls.Add(this.ImpulsePeriodValueLabel);
            this.ImpulseTabPage.Controls.Add(this.ImpulseTrackBarLabel);
            this.ImpulseTabPage.Controls.Add(this.ImpulseLevelLabel);
            this.ImpulseTabPage.Controls.Add(this.ImpulseBaseLabel);
            this.ImpulseTabPage.Controls.Add(this.SaveImpulseChannelButton);
            this.ImpulseTabPage.Controls.Add(this.ImpulsePeriodValue);
            this.ImpulseTabPage.Controls.Add(this.ImpulseBaseValue);
            this.ImpulseTabPage.Controls.Add(this.ImpulseLevelValue);
            this.ImpulseTabPage.Controls.Add(this.SeveralSlidersImpulseTrackBar);
            this.ImpulseTabPage.Location = new System.Drawing.Point(4, 22);
            this.ImpulseTabPage.Name = "ImpulseTabPage";
            this.ImpulseTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.ImpulseTabPage.Size = new System.Drawing.Size(254, 351);
            this.ImpulseTabPage.TabIndex = 2;
            this.ImpulseTabPage.Text = "Прямоугольник";
            this.ImpulseTabPage.UseVisualStyleBackColor = true;
            this.ImpulseTabPage.Enter += new System.EventHandler(this.ImpulseTabPage_Enter);
            // 
            // DeleteImpulseChannelButton
            // 
            this.DeleteImpulseChannelButton.BackgroundImage = global::ImpulseMaker.Properties.Resources.png_transparent_icon_design_trash_red_line_area_material_rectangle;
            this.DeleteImpulseChannelButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.DeleteImpulseChannelButton.Location = new System.Drawing.Point(225, 313);
            this.DeleteImpulseChannelButton.Name = "DeleteImpulseChannelButton";
            this.DeleteImpulseChannelButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.DeleteImpulseChannelButton.Size = new System.Drawing.Size(25, 25);
            this.DeleteImpulseChannelButton.TabIndex = 14;
            this.DeleteImpulseChannelButton.UseVisualStyleBackColor = true;
            this.DeleteImpulseChannelButton.Click += new System.EventHandler(this.DeleteImpulseChannelButton_Click);
            // 
            // ZeroEndingImpulseCheckBox
            // 
            this.ZeroEndingImpulseCheckBox.AutoSize = true;
            this.ZeroEndingImpulseCheckBox.Location = new System.Drawing.Point(9, 251);
            this.ZeroEndingImpulseCheckBox.Name = "ZeroEndingImpulseCheckBox";
            this.ZeroEndingImpulseCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.ZeroEndingImpulseCheckBox.Size = new System.Drawing.Size(163, 17);
            this.ZeroEndingImpulseCheckBox.TabIndex = 13;
            this.ZeroEndingImpulseCheckBox.Text = "Заканчивать сигнал нулем";
            this.ZeroEndingImpulseCheckBox.UseVisualStyleBackColor = true;
            this.ZeroEndingImpulseCheckBox.CheckedChanged += new System.EventHandler(this.ZeroEndingImpulseCheckBox_CheckedChange);
            // 
            // ImpulseChannelName
            // 
            this.ImpulseChannelName.Location = new System.Drawing.Point(9, 317);
            this.ImpulseChannelName.Name = "ImpulseChannelName";
            this.ImpulseChannelName.Size = new System.Drawing.Size(181, 20);
            this.ImpulseChannelName.TabIndex = 12;
            // 
            // SaveImpulseChannelLabel
            // 
            this.SaveImpulseChannelLabel.AutoSize = true;
            this.SaveImpulseChannelLabel.Location = new System.Drawing.Point(6, 301);
            this.SaveImpulseChannelLabel.Name = "SaveImpulseChannelLabel";
            this.SaveImpulseChannelLabel.Size = new System.Drawing.Size(99, 13);
            this.SaveImpulseChannelLabel.TabIndex = 10;
            this.SaveImpulseChannelLabel.Text = "Название канала:";
            // 
            // ImpulsePeriodValueLabel
            // 
            this.ImpulsePeriodValueLabel.AutoSize = true;
            this.ImpulsePeriodValueLabel.Location = new System.Drawing.Point(6, 96);
            this.ImpulsePeriodValueLabel.Name = "ImpulsePeriodValueLabel";
            this.ImpulsePeriodValueLabel.Size = new System.Drawing.Size(72, 13);
            this.ImpulsePeriodValueLabel.TabIndex = 9;
            this.ImpulsePeriodValueLabel.Text = "Период, сек:";
            // 
            // ImpulseTrackBarLabel
            // 
            this.ImpulseTrackBarLabel.AutoSize = true;
            this.ImpulseTrackBarLabel.Location = new System.Drawing.Point(6, 167);
            this.ImpulseTrackBarLabel.Name = "ImpulseTrackBarLabel";
            this.ImpulseTrackBarLabel.Size = new System.Drawing.Size(115, 13);
            this.ImpulseTrackBarLabel.TabIndex = 7;
            this.ImpulseTrackBarLabel.Text = "Положения изломов:";
            // 
            // ImpulseLevelLabel
            // 
            this.ImpulseLevelLabel.AutoSize = true;
            this.ImpulseLevelLabel.Location = new System.Drawing.Point(6, 55);
            this.ImpulseLevelLabel.Name = "ImpulseLevelLabel";
            this.ImpulseLevelLabel.Size = new System.Drawing.Size(55, 13);
            this.ImpulseLevelLabel.TabIndex = 5;
            this.ImpulseLevelLabel.Text = "Полка, В:";
            // 
            // ImpulseBaseLabel
            // 
            this.ImpulseBaseLabel.AutoSize = true;
            this.ImpulseBaseLabel.Location = new System.Drawing.Point(6, 14);
            this.ImpulseBaseLabel.Name = "ImpulseBaseLabel";
            this.ImpulseBaseLabel.Size = new System.Drawing.Size(79, 13);
            this.ImpulseBaseLabel.TabIndex = 6;
            this.ImpulseBaseLabel.Text = "Основание, В:";
            // 
            // SaveImpulseChannelButton
            // 
            this.SaveImpulseChannelButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("SaveImpulseChannelButton.BackgroundImage")));
            this.SaveImpulseChannelButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.SaveImpulseChannelButton.Location = new System.Drawing.Point(195, 313);
            this.SaveImpulseChannelButton.Name = "SaveImpulseChannelButton";
            this.SaveImpulseChannelButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.SaveImpulseChannelButton.Size = new System.Drawing.Size(25, 25);
            this.SaveImpulseChannelButton.TabIndex = 11;
            this.SaveImpulseChannelButton.UseVisualStyleBackColor = true;
            this.SaveImpulseChannelButton.Click += new System.EventHandler(this.SaveImpulseChannelButton_Click);
            // 
            // SineTabPage
            // 
            this.SineTabPage.Controls.Add(this.SinePeriodValueLabel);
            this.SineTabPage.Controls.Add(this.SineAmplitudeLabel);
            this.SineTabPage.Controls.Add(this.SineLevelLabel);
            this.SineTabPage.Controls.Add(this.DeleteSineChannelButton);
            this.SineTabPage.Controls.Add(this.ZeroEndingSineCheckBox);
            this.SineTabPage.Controls.Add(this.SineChannelName);
            this.SineTabPage.Controls.Add(this.SaveSineChannelLabel);
            this.SineTabPage.Controls.Add(this.SaveSineChannelButton);
            this.SineTabPage.Controls.Add(this.SineAngleTrackerLabel);
            this.SineTabPage.Controls.Add(this.SineAngleTrackBar);
            this.SineTabPage.Controls.Add(this.SineAngleTrackBarLabel);
            this.SineTabPage.Controls.Add(this.SinePeriodValue);
            this.SineTabPage.Controls.Add(this.SineAmplitudeValue);
            this.SineTabPage.Controls.Add(this.SineLevelValue);
            this.SineTabPage.Location = new System.Drawing.Point(4, 22);
            this.SineTabPage.Name = "SineTabPage";
            this.SineTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.SineTabPage.Size = new System.Drawing.Size(254, 351);
            this.SineTabPage.TabIndex = 0;
            this.SineTabPage.Text = "Синус";
            this.SineTabPage.UseVisualStyleBackColor = true;
            this.SineTabPage.Enter += new System.EventHandler(this.SineTabPage_Enter);
            // 
            // SinePeriodValueLabel
            // 
            this.SinePeriodValueLabel.AutoSize = true;
            this.SinePeriodValueLabel.Location = new System.Drawing.Point(6, 96);
            this.SinePeriodValueLabel.Name = "SinePeriodValueLabel";
            this.SinePeriodValueLabel.Size = new System.Drawing.Size(72, 13);
            this.SinePeriodValueLabel.TabIndex = 22;
            this.SinePeriodValueLabel.Text = "Период, сек:";
            // 
            // SineAmplitudeLabel
            // 
            this.SineAmplitudeLabel.AutoSize = true;
            this.SineAmplitudeLabel.Location = new System.Drawing.Point(6, 55);
            this.SineAmplitudeLabel.Name = "SineAmplitudeLabel";
            this.SineAmplitudeLabel.Size = new System.Drawing.Size(78, 13);
            this.SineAmplitudeLabel.TabIndex = 20;
            this.SineAmplitudeLabel.Text = "Амплитуда, В:";
            // 
            // SineLevelLabel
            // 
            this.SineLevelLabel.AutoSize = true;
            this.SineLevelLabel.Location = new System.Drawing.Point(6, 14);
            this.SineLevelLabel.Name = "SineLevelLabel";
            this.SineLevelLabel.Size = new System.Drawing.Size(67, 13);
            this.SineLevelLabel.TabIndex = 21;
            this.SineLevelLabel.Text = "Уровень, В:";
            // 
            // DeleteSineChannelButton
            // 
            this.DeleteSineChannelButton.BackgroundImage = global::ImpulseMaker.Properties.Resources.png_transparent_icon_design_trash_red_line_area_material_rectangle;
            this.DeleteSineChannelButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.DeleteSineChannelButton.Location = new System.Drawing.Point(225, 313);
            this.DeleteSineChannelButton.Name = "DeleteSineChannelButton";
            this.DeleteSineChannelButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.DeleteSineChannelButton.Size = new System.Drawing.Size(25, 25);
            this.DeleteSineChannelButton.TabIndex = 19;
            this.DeleteSineChannelButton.UseVisualStyleBackColor = true;
            this.DeleteSineChannelButton.Click += new System.EventHandler(this.DeleteSineChannelButton_Click);
            // 
            // ZeroEndingSineCheckBox
            // 
            this.ZeroEndingSineCheckBox.AutoSize = true;
            this.ZeroEndingSineCheckBox.Location = new System.Drawing.Point(9, 251);
            this.ZeroEndingSineCheckBox.Name = "ZeroEndingSineCheckBox";
            this.ZeroEndingSineCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.ZeroEndingSineCheckBox.Size = new System.Drawing.Size(163, 17);
            this.ZeroEndingSineCheckBox.TabIndex = 18;
            this.ZeroEndingSineCheckBox.Text = "Заканчивать сигнал нулем";
            this.ZeroEndingSineCheckBox.UseVisualStyleBackColor = true;
            this.ZeroEndingSineCheckBox.CheckedChanged += new System.EventHandler(this.ZeroEndingSineCheckBox_CheckedChange);
            // 
            // SineChannelName
            // 
            this.SineChannelName.Location = new System.Drawing.Point(9, 317);
            this.SineChannelName.Name = "SineChannelName";
            this.SineChannelName.Size = new System.Drawing.Size(181, 20);
            this.SineChannelName.TabIndex = 17;
            // 
            // SaveSineChannelLabel
            // 
            this.SaveSineChannelLabel.AutoSize = true;
            this.SaveSineChannelLabel.Location = new System.Drawing.Point(6, 301);
            this.SaveSineChannelLabel.Name = "SaveSineChannelLabel";
            this.SaveSineChannelLabel.Size = new System.Drawing.Size(99, 13);
            this.SaveSineChannelLabel.TabIndex = 15;
            this.SaveSineChannelLabel.Text = "Название канала:";
            // 
            // SaveSineChannelButton
            // 
            this.SaveSineChannelButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("SaveSineChannelButton.BackgroundImage")));
            this.SaveSineChannelButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.SaveSineChannelButton.Location = new System.Drawing.Point(195, 313);
            this.SaveSineChannelButton.Name = "SaveSineChannelButton";
            this.SaveSineChannelButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.SaveSineChannelButton.Size = new System.Drawing.Size(25, 25);
            this.SaveSineChannelButton.TabIndex = 16;
            this.SaveSineChannelButton.UseVisualStyleBackColor = true;
            this.SaveSineChannelButton.Click += new System.EventHandler(this.SaveSineChannelButton_Click);
            // 
            // SineAngleTrackerLabel
            // 
            this.SineAngleTrackerLabel.AutoSize = true;
            this.SineAngleTrackerLabel.Location = new System.Drawing.Point(213, 167);
            this.SineAngleTrackerLabel.Name = "SineAngleTrackerLabel";
            this.SineAngleTrackerLabel.Size = new System.Drawing.Size(25, 13);
            this.SineAngleTrackerLabel.TabIndex = 7;
            this.SineAngleTrackerLabel.Text = "180";
            // 
            // SineAngleTrackBar
            // 
            this.SineAngleTrackBar.Location = new System.Drawing.Point(9, 185);
            this.SineAngleTrackBar.Maximum = 72;
            this.SineAngleTrackBar.Name = "SineAngleTrackBar";
            this.SineAngleTrackBar.Size = new System.Drawing.Size(229, 45);
            this.SineAngleTrackBar.TabIndex = 6;
            this.SineAngleTrackBar.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.SineAngleTrackBar.Value = 36;
            this.SineAngleTrackBar.ValueChanged += new System.EventHandler(this.SineAngleTrackBar_ValueChanged);
            // 
            // SineAngleTrackBarLabel
            // 
            this.SineAngleTrackBarLabel.AutoSize = true;
            this.SineAngleTrackBarLabel.Location = new System.Drawing.Point(6, 167);
            this.SineAngleTrackBarLabel.Name = "SineAngleTrackBarLabel";
            this.SineAngleTrackBarLabel.Size = new System.Drawing.Size(85, 13);
            this.SineAngleTrackBarLabel.TabIndex = 5;
            this.SineAngleTrackBarLabel.Text = "Угол поворота:";
            // 
            // CustomTabPage
            // 
            this.CustomTabPage.Controls.Add(this.CustomPeriodValueLabel);
            this.CustomTabPage.Controls.Add(this.ZeroEndingCustomCheckBox);
            this.CustomTabPage.Controls.Add(this.DeleteCustomChannelButton);
            this.CustomTabPage.Controls.Add(this.CustomChannelName);
            this.CustomTabPage.Controls.Add(this.SaveCustomChannelLabel);
            this.CustomTabPage.Controls.Add(this.SaveCustomChannelButton);
            this.CustomTabPage.Controls.Add(this.CustomPeriodValue);
            this.CustomTabPage.Controls.Add(this.pointCoordinatesListBox);
            this.CustomTabPage.Location = new System.Drawing.Point(4, 22);
            this.CustomTabPage.Margin = new System.Windows.Forms.Padding(2);
            this.CustomTabPage.Name = "CustomTabPage";
            this.CustomTabPage.Size = new System.Drawing.Size(254, 351);
            this.CustomTabPage.TabIndex = 3;
            this.CustomTabPage.Text = "Кастом";
            this.CustomTabPage.UseVisualStyleBackColor = true;
            this.CustomTabPage.Enter += new System.EventHandler(this.CustomTabPage_Enter);
            // 
            // CustomPeriodValueLabel
            // 
            this.CustomPeriodValueLabel.AutoSize = true;
            this.CustomPeriodValueLabel.Location = new System.Drawing.Point(6, 239);
            this.CustomPeriodValueLabel.Name = "CustomPeriodValueLabel";
            this.CustomPeriodValueLabel.Size = new System.Drawing.Size(72, 13);
            this.CustomPeriodValueLabel.TabIndex = 21;
            this.CustomPeriodValueLabel.Text = "Период, сек:";
            // 
            // ZeroEndingCustomCheckBox
            // 
            this.ZeroEndingCustomCheckBox.AutoSize = true;
            this.ZeroEndingCustomCheckBox.Location = new System.Drawing.Point(7, 281);
            this.ZeroEndingCustomCheckBox.Name = "ZeroEndingCustomCheckBox";
            this.ZeroEndingCustomCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.ZeroEndingCustomCheckBox.Size = new System.Drawing.Size(163, 17);
            this.ZeroEndingCustomCheckBox.TabIndex = 19;
            this.ZeroEndingCustomCheckBox.Text = "Заканчивать сигнал нулем";
            this.ZeroEndingCustomCheckBox.UseVisualStyleBackColor = true;
            this.ZeroEndingCustomCheckBox.CheckedChanged += new System.EventHandler(this.ZeroEndingCustomCheckBox_CheckedChange);
            // 
            // DeleteCustomChannelButton
            // 
            this.DeleteCustomChannelButton.BackgroundImage = global::ImpulseMaker.Properties.Resources.png_transparent_icon_design_trash_red_line_area_material_rectangle;
            this.DeleteCustomChannelButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.DeleteCustomChannelButton.Location = new System.Drawing.Point(225, 313);
            this.DeleteCustomChannelButton.Name = "DeleteCustomChannelButton";
            this.DeleteCustomChannelButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.DeleteCustomChannelButton.Size = new System.Drawing.Size(25, 25);
            this.DeleteCustomChannelButton.TabIndex = 18;
            this.DeleteCustomChannelButton.UseVisualStyleBackColor = true;
            this.DeleteCustomChannelButton.Click += new System.EventHandler(this.DeleteCustomChannelButton_Click);
            // 
            // CustomChannelName
            // 
            this.CustomChannelName.Location = new System.Drawing.Point(9, 317);
            this.CustomChannelName.Name = "CustomChannelName";
            this.CustomChannelName.Size = new System.Drawing.Size(181, 20);
            this.CustomChannelName.TabIndex = 17;
            // 
            // SaveCustomChannelLabel
            // 
            this.SaveCustomChannelLabel.AutoSize = true;
            this.SaveCustomChannelLabel.Location = new System.Drawing.Point(6, 301);
            this.SaveCustomChannelLabel.Name = "SaveCustomChannelLabel";
            this.SaveCustomChannelLabel.Size = new System.Drawing.Size(99, 13);
            this.SaveCustomChannelLabel.TabIndex = 15;
            this.SaveCustomChannelLabel.Text = "Название канала:";
            // 
            // SaveCustomChannelButton
            // 
            this.SaveCustomChannelButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("SaveCustomChannelButton.BackgroundImage")));
            this.SaveCustomChannelButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.SaveCustomChannelButton.Location = new System.Drawing.Point(195, 313);
            this.SaveCustomChannelButton.Name = "SaveCustomChannelButton";
            this.SaveCustomChannelButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.SaveCustomChannelButton.Size = new System.Drawing.Size(25, 25);
            this.SaveCustomChannelButton.TabIndex = 16;
            this.SaveCustomChannelButton.UseVisualStyleBackColor = true;
            this.SaveCustomChannelButton.Click += new System.EventHandler(this.SaveCustomChannelButton_Click);
            // 
            // SamplingRateLabel
            // 
            this.SamplingRateLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SamplingRateLabel.AutoSize = true;
            this.SamplingRateLabel.Location = new System.Drawing.Point(640, 80);
            this.SamplingRateLabel.Name = "SamplingRateLabel";
            this.SamplingRateLabel.Size = new System.Drawing.Size(167, 13);
            this.SamplingRateLabel.TabIndex = 8;
            this.SamplingRateLabel.Text = "Частота дискретизации, 1/сек:";
            // 
            // SignalDurationLabel
            // 
            this.SignalDurationLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SignalDurationLabel.AutoSize = true;
            this.SignalDurationLabel.Location = new System.Drawing.Point(640, 39);
            this.SignalDurationLabel.Name = "SignalDurationLabel";
            this.SignalDurationLabel.Size = new System.Drawing.Size(151, 13);
            this.SignalDurationLabel.TabIndex = 9;
            this.SignalDurationLabel.Text = "Длительность сигнала, сек:";
            // 
            // SaveAllChannelsProgress
            // 
            this.SaveAllChannelsProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SaveAllChannelsProgress.Location = new System.Drawing.Point(643, 138);
            this.SaveAllChannelsProgress.MarqueeAnimationSpeed = 10;
            this.SaveAllChannelsProgress.Name = "SaveAllChannelsProgress";
            this.SaveAllChannelsProgress.Size = new System.Drawing.Size(163, 6);
            this.SaveAllChannelsProgress.Step = 1;
            this.SaveAllChannelsProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.SaveAllChannelsProgress.TabIndex = 12;
            // 
            // SaveAllChannelsLabel
            // 
            this.SaveAllChannelsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SaveAllChannelsLabel.AutoSize = true;
            this.SaveAllChannelsLabel.Location = new System.Drawing.Point(640, 122);
            this.SaveAllChannelsLabel.Name = "SaveAllChannelsLabel";
            this.SaveAllChannelsLabel.Size = new System.Drawing.Size(67, 13);
            this.SaveAllChannelsLabel.TabIndex = 8;
            this.SaveAllChannelsLabel.Text = "Сохранение";
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(4, 1, 0, 1);
            this.menuStrip1.Size = new System.Drawing.Size(1084, 24);
            this.menuStrip1.TabIndex = 13;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.iNIFilePathToolStripMenuItem,
            this.cSVFilePathToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 22);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // iNIFilePathToolStripMenuItem
            // 
            this.iNIFilePathToolStripMenuItem.Name = "iNIFilePathToolStripMenuItem";
            this.iNIFilePathToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.iNIFilePathToolStripMenuItem.Text = "INI file path";
            this.iNIFilePathToolStripMenuItem.Click += new System.EventHandler(this.iNIFilePathToolStripMenuItem_Click);
            // 
            // cSVFilePathToolStripMenuItem
            // 
            this.cSVFilePathToolStripMenuItem.Name = "cSVFilePathToolStripMenuItem";
            this.cSVFilePathToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.cSVFilePathToolStripMenuItem.Text = "CSV file path";
            this.cSVFilePathToolStripMenuItem.Click += new System.EventHandler(this.cSVFilePathToolStripMenuItem_Click);
            // 
            // ChannelsListBox
            // 
            this.ChannelsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ChannelsListBox.BackColor = System.Drawing.SystemColors.Control;
            this.ChannelsListBox.Location = new System.Drawing.Point(642, 151);
            this.ChannelsListBox.Name = "ChannelsListBox";
            this.ChannelsListBox.selected_item = -1;
            this.ChannelsListBox.Size = new System.Drawing.Size(164, 259);
            this.ChannelsListBox.TabIndex = 10;
            this.ChannelsListBox.MoveSelectedUp += new System.EventHandler(this.ChannelsListBox_MoveSelectedUp);
            this.ChannelsListBox.MoveSelectedDown += new System.EventHandler(this.ChannelsListBox_MoveSelectedDown);
            this.ChannelsListBox.SelectionChanged += new System.EventHandler(this.ChannelsListBox_SelectionChanged);
            this.ChannelsListBox.ItemDeleted += new System.EventHandler(this.ChannelsListBox_DeleteItem);
            this.ChannelsListBox.ItemRenamed += new ImpulseMaker.MyListBox.CustomEventHandler(this.ChannelsListBox_RenameItem);
            // 
            // SignalDurationValue
            // 
            this.SignalDurationValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SignalDurationValue.DecimalPlaces = 2;
            this.SignalDurationValue.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.SignalDurationValue.Location = new System.Drawing.Point(643, 56);
            this.SignalDurationValue.Name = "SignalDurationValue";
            this.SignalDurationValue.Size = new System.Drawing.Size(120, 20);
            this.SignalDurationValue.TabIndex = 6;
            this.SignalDurationValue.ThousandsSeparator = true;
            this.SignalDurationValue.Value = new decimal(new int[] {
            25,
            0,
            0,
            65536});
            this.SignalDurationValue.ValueChangedBeforeLeave += new System.EventHandler(this.SaveAllChannels);
            this.SignalDurationValue.ValueChanged += new System.EventHandler(this.SignalDuration_ValueChanged);
            // 
            // SamplingRateValue
            // 
            this.SamplingRateValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SamplingRateValue.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SamplingRateValue.Increment = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.SamplingRateValue.Location = new System.Drawing.Point(643, 97);
            this.SamplingRateValue.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.SamplingRateValue.Minimum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.SamplingRateValue.Name = "SamplingRateValue";
            this.SamplingRateValue.Size = new System.Drawing.Size(120, 20);
            this.SamplingRateValue.TabIndex = 7;
            this.SamplingRateValue.Value = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            this.SamplingRateValue.ValueChangedBeforeLeave += new System.EventHandler(this.SaveAllChannels);
            // 
            // RampBeginValue
            // 
            this.RampBeginValue.DecimalPlaces = 2;
            this.RampBeginValue.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.RampBeginValue.Location = new System.Drawing.Point(9, 31);
            this.RampBeginValue.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.RampBeginValue.Name = "RampBeginValue";
            this.RampBeginValue.Size = new System.Drawing.Size(120, 20);
            this.RampBeginValue.TabIndex = 1;
            this.RampBeginValue.ThousandsSeparator = true;
            this.RampBeginValue.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.RampBeginValue.ValueChanged += new System.EventHandler(this.ramp_graph);
            // 
            // RampPeriodValue
            // 
            this.RampPeriodValue.DecimalPlaces = 3;
            this.RampPeriodValue.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.RampPeriodValue.Location = new System.Drawing.Point(9, 113);
            this.RampPeriodValue.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.RampPeriodValue.Name = "RampPeriodValue";
            this.RampPeriodValue.Size = new System.Drawing.Size(120, 20);
            this.RampPeriodValue.TabIndex = 1;
            this.RampPeriodValue.Value = new decimal(new int[] {
            2,
            0,
            0,
            131072});
            this.RampPeriodValue.ValueChanged += new System.EventHandler(this.RampPeriodValue_ValueChanged);
            // 
            // RampPeakValue
            // 
            this.RampPeakValue.DecimalPlaces = 2;
            this.RampPeakValue.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.RampPeakValue.Location = new System.Drawing.Point(9, 72);
            this.RampPeakValue.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.RampPeakValue.Name = "RampPeakValue";
            this.RampPeakValue.Size = new System.Drawing.Size(120, 20);
            this.RampPeakValue.TabIndex = 1;
            this.RampPeakValue.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.RampPeakValue.ValueChanged += new System.EventHandler(this.ramp_graph);
            // 
            // ImpulsePeriodValue
            // 
            this.ImpulsePeriodValue.DecimalPlaces = 3;
            this.ImpulsePeriodValue.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.ImpulsePeriodValue.Location = new System.Drawing.Point(9, 113);
            this.ImpulsePeriodValue.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.ImpulsePeriodValue.Name = "ImpulsePeriodValue";
            this.ImpulsePeriodValue.Size = new System.Drawing.Size(120, 20);
            this.ImpulsePeriodValue.TabIndex = 8;
            this.ImpulsePeriodValue.Value = new decimal(new int[] {
            2,
            0,
            0,
            131072});
            this.ImpulsePeriodValue.ValueChanged += new System.EventHandler(this.ImpulsePeriodValue_ValueChanged);
            // 
            // ImpulseBaseValue
            // 
            this.ImpulseBaseValue.DecimalPlaces = 2;
            this.ImpulseBaseValue.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.ImpulseBaseValue.Location = new System.Drawing.Point(9, 31);
            this.ImpulseBaseValue.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.ImpulseBaseValue.Name = "ImpulseBaseValue";
            this.ImpulseBaseValue.Size = new System.Drawing.Size(120, 20);
            this.ImpulseBaseValue.TabIndex = 3;
            this.ImpulseBaseValue.ThousandsSeparator = true;
            this.ImpulseBaseValue.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.ImpulseBaseValue.ValueChanged += new System.EventHandler(this.impulse_graph);
            // 
            // ImpulseLevelValue
            // 
            this.ImpulseLevelValue.DecimalPlaces = 2;
            this.ImpulseLevelValue.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.ImpulseLevelValue.Location = new System.Drawing.Point(9, 72);
            this.ImpulseLevelValue.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.ImpulseLevelValue.Name = "ImpulseLevelValue";
            this.ImpulseLevelValue.Size = new System.Drawing.Size(120, 20);
            this.ImpulseLevelValue.TabIndex = 4;
            this.ImpulseLevelValue.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.ImpulseLevelValue.ValueChanged += new System.EventHandler(this.impulse_graph);
            // 
            // SeveralSlidersImpulseTrackBar
            // 
            this.SeveralSlidersImpulseTrackBar.Location = new System.Drawing.Point(1, 185);
            this.SeveralSlidersImpulseTrackBar.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.SeveralSlidersImpulseTrackBar.Max = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.SeveralSlidersImpulseTrackBar.Min = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.SeveralSlidersImpulseTrackBar.Name = "SeveralSlidersImpulseTrackBar";
            this.SeveralSlidersImpulseTrackBar.SelectedMax = new decimal(new int[] {
            15,
            0,
            0,
            196608});
            this.SeveralSlidersImpulseTrackBar.SelectedMin = new decimal(new int[] {
            8,
            0,
            0,
            196608});
            this.SeveralSlidersImpulseTrackBar.Size = new System.Drawing.Size(249, 58);
            this.SeveralSlidersImpulseTrackBar.TabIndex = 1;
            this.SeveralSlidersImpulseTrackBar.margin_needed += new System.EventHandler(this.SeveralSlidersTrackBar_margin_needed);
            this.SeveralSlidersImpulseTrackBar.SelectionChanged += new System.EventHandler(this.impulse_graph);
            this.SeveralSlidersImpulseTrackBar.MinClicked += new System.EventHandler(this.SelectionRangeSlider_MouseClickMin);
            this.SeveralSlidersImpulseTrackBar.MaxClicked += new System.EventHandler(this.SelectionRangeSlider_MouseClickMax);
            // 
            // SinePeriodValue
            // 
            this.SinePeriodValue.DecimalPlaces = 3;
            this.SinePeriodValue.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.SinePeriodValue.Location = new System.Drawing.Point(9, 113);
            this.SinePeriodValue.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.SinePeriodValue.Name = "SinePeriodValue";
            this.SinePeriodValue.Size = new System.Drawing.Size(120, 20);
            this.SinePeriodValue.TabIndex = 8;
            this.SinePeriodValue.Value = new decimal(new int[] {
            2,
            0,
            0,
            131072});
            this.SinePeriodValue.ValueChanged += new System.EventHandler(this.SinePeriodValue_ValueChanged);
            // 
            // SineAmplitudeValue
            // 
            this.SineAmplitudeValue.DecimalPlaces = 2;
            this.SineAmplitudeValue.Location = new System.Drawing.Point(9, 72);
            this.SineAmplitudeValue.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.SineAmplitudeValue.Name = "SineAmplitudeValue";
            this.SineAmplitudeValue.Size = new System.Drawing.Size(120, 20);
            this.SineAmplitudeValue.TabIndex = 0;
            this.SineAmplitudeValue.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.SineAmplitudeValue.ValueChanged += new System.EventHandler(this.sine_graph);
            // 
            // SineLevelValue
            // 
            this.SineLevelValue.DecimalPlaces = 2;
            this.SineLevelValue.Location = new System.Drawing.Point(9, 31);
            this.SineLevelValue.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.SineLevelValue.Name = "SineLevelValue";
            this.SineLevelValue.Size = new System.Drawing.Size(120, 20);
            this.SineLevelValue.TabIndex = 0;
            this.SineLevelValue.ValueChanged += new System.EventHandler(this.sine_graph);
            // 
            // CustomPeriodValue
            // 
            this.CustomPeriodValue.DecimalPlaces = 3;
            this.CustomPeriodValue.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.CustomPeriodValue.Location = new System.Drawing.Point(9, 256);
            this.CustomPeriodValue.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.CustomPeriodValue.Name = "CustomPeriodValue";
            this.CustomPeriodValue.Size = new System.Drawing.Size(120, 20);
            this.CustomPeriodValue.TabIndex = 20;
            this.CustomPeriodValue.Value = new decimal(new int[] {
            2,
            0,
            0,
            131072});
            this.CustomPeriodValue.ValueChanged += new System.EventHandler(this.CustomPeriodValue_ValueChanged);
            // 
            // pointCoordinatesListBox
            // 
            this.pointCoordinatesListBox.Location = new System.Drawing.Point(3, 3);
            this.pointCoordinatesListBox.Name = "pointCoordinatesListBox";
            this.pointCoordinatesListBox.point_value_X_margin = 0.1D;
            this.pointCoordinatesListBox.Size = new System.Drawing.Size(248, 233);
            this.pointCoordinatesListBox.TabIndex = 0;
            this.pointCoordinatesListBox.Text = "pointCoordinatesListBox";
            this.pointCoordinatesListBox.X_margin_needed += new System.EventHandler(this.PointCoordinatesListBox_X_margin_needed);
            this.pointCoordinatesListBox.PointChanged += new ImpulseMaker.PointCoordinatesListBox.PointEventHandler(this.PointCoordinatesListBox_PointChanged);
            this.pointCoordinatesListBox.PointAdded += new ImpulseMaker.PointCoordinatesListBox.PointEventHandler(this.PointCoordinatesListBox_PointAdded);
            this.pointCoordinatesListBox.PointDeleted += new ImpulseMaker.PointCoordinatesListBox.PointEventHandler(this.PointCoordinatesListBox_PointDeleted);
            // 
            // WholeSignalChart
            // 
            this.WholeSignalChart.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.WholeSignalChart.BackColor = System.Drawing.Color.Transparent;
            this.WholeSignalChart.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.WholeSignalChart.BorderlineColor = System.Drawing.Color.Transparent;
            chartArea1.AxisX.MajorGrid.LineColor = System.Drawing.Color.Silver;
            chartArea1.AxisX.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.AxisY.MajorGrid.LineColor = System.Drawing.Color.Silver;
            chartArea1.AxisY.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.Name = "ChartArea1";
            this.WholeSignalChart.ChartAreas.Add(chartArea1);
            this.WholeSignalChart.enable_add_point = false;
            this.WholeSignalChart.is_able_to_choose = true;
            legend1.IsTextAutoFit = false;
            legend1.Name = "Legend1";
            this.WholeSignalChart.Legends.Add(legend1);
            this.WholeSignalChart.Location = new System.Drawing.Point(0, 413);
            this.WholeSignalChart.Name = "WholeSignalChart";
            this.WholeSignalChart.point_value_X_margin = 0.007D;
            this.WholeSignalChart.point_value_Y_margin = 0.1D;
            series1.BorderWidth = 3;
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.WholeSignalChart.Series.Add(series1);
            this.WholeSignalChart.Size = new System.Drawing.Size(1084, 280);
            this.WholeSignalChart.TabIndex = 0;
            this.WholeSignalChart.Text = "OneSegmentChart";
            this.WholeSignalChart.X_margin_needed += new System.EventHandler(this.MyChart_X_margin_needed);
            this.WholeSignalChart.Y_margin_needed += new System.EventHandler(this.MyChart_Y_margin_needed);
            // 
            // OneSegmentChart
            // 
            this.OneSegmentChart.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OneSegmentChart.BackColor = System.Drawing.Color.Transparent;
            this.OneSegmentChart.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.OneSegmentChart.BorderlineColor = System.Drawing.Color.Transparent;
            chartArea2.AxisX.MajorGrid.LineColor = System.Drawing.Color.Silver;
            chartArea2.AxisX.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea2.AxisY.MajorGrid.LineColor = System.Drawing.Color.Silver;
            chartArea2.AxisY.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea2.Name = "ChartArea1";
            this.OneSegmentChart.ChartAreas.Add(chartArea2);
            this.OneSegmentChart.enable_add_point = true;
            this.OneSegmentChart.is_able_to_choose = false;
            legend2.Name = "Legend1";
            this.OneSegmentChart.Legends.Add(legend2);
            this.OneSegmentChart.Location = new System.Drawing.Point(0, 34);
            this.OneSegmentChart.Name = "OneSegmentChart";
            this.OneSegmentChart.point_value_X_margin = 0.007D;
            this.OneSegmentChart.point_value_Y_margin = 0.1D;
            series2.BorderWidth = 3;
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series2.Color = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(140)))), ((int)(((byte)(240)))));
            series2.Legend = "Legend1";
            series2.Name = "Series1";
            this.OneSegmentChart.Series.Add(series2);
            this.OneSegmentChart.Size = new System.Drawing.Size(637, 371);
            this.OneSegmentChart.TabIndex = 0;
            this.OneSegmentChart.Text = "OneSegmentChart";
            this.OneSegmentChart.X_margin_needed += new System.EventHandler(this.MyChart_X_margin_needed);
            this.OneSegmentChart.Y_margin_needed += new System.EventHandler(this.MyChart_Y_margin_needed);
            this.OneSegmentChart.PointChanged += new ImpulseMaker.MyChart.PointEventHandler(this.OneSegmentChart_PointChanged);
            this.OneSegmentChart.PointAdded += new ImpulseMaker.MyChart.PointEventHandler(this.OneSegmentChart_PointAdded);
            this.OneSegmentChart.PointDeleted += new ImpulseMaker.MyChart.PointEventHandler(this.OneSegmentChart_PointDeleted);
            this.OneSegmentChart.FinishedDrawing += new System.EventHandler(this.MyChart_FinishedDrawing);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1084, 690);
            this.Controls.Add(this.SaveAllChannelsProgress);
            this.Controls.Add(this.ChannelsListBox);
            this.Controls.Add(this.SaveAllChannelsLabel);
            this.Controls.Add(this.SamplingRateLabel);
            this.Controls.Add(this.SignalDurationLabel);
            this.Controls.Add(this.SignalDurationValue);
            this.Controls.Add(this.SamplingRateValue);
            this.Controls.Add(this.SignalTypeTabControl);
            this.Controls.Add(this.WholeSignalChart);
            this.Controls.Add(this.OneSegmentChart);
            this.Controls.Add(this.menuStrip1);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(698, 688);
            this.Name = "Form1";
            this.Text = "SignalMaker";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.SignalTypeTabControl.ResumeLayout(false);
            this.RampTabPage.ResumeLayout(false);
            this.RampTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RampPeakTrackBar)).EndInit();
            this.ImpulseTabPage.ResumeLayout(false);
            this.ImpulseTabPage.PerformLayout();
            this.SineTabPage.ResumeLayout(false);
            this.SineTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SineAngleTrackBar)).EndInit();
            this.CustomTabPage.ResumeLayout(false);
            this.CustomTabPage.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SignalDurationValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SamplingRateValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.RampBeginValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.RampPeriodValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.RampPeakValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ImpulsePeriodValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ImpulseBaseValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ImpulseLevelValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SinePeriodValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SineAmplitudeValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SineLevelValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CustomPeriodValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.WholeSignalChart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.OneSegmentChart)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MyChart OneSegmentChart;
        private System.Windows.Forms.TabControl SignalTypeTabControl;
        private System.Windows.Forms.TabPage SineTabPage;
        private System.Windows.Forms.TabPage RampTabPage;
        private System.Windows.Forms.TabPage ImpulseTabPage;
        private ImpulseMaker.MyNumericUpDown SineLevelValue;
        private ImpulseMaker.MyNumericUpDown SineAmplitudeValue;
        private ImpulseMaker.MyNumericUpDown RampPeakValue;
        private System.Windows.Forms.Label RampBeginLabel;
        private ImpulseMaker.MyNumericUpDown RampBeginValue;
        private System.Windows.Forms.Label RampPeakLabel;
        private System.Windows.Forms.TrackBar RampPeakTrackBar;
        private System.Windows.Forms.Label RampPeakTrackBarLabel;
        private System.Windows.Forms.Label RampPeakTrackerLabel;
        private System.Windows.Forms.Label RampPeriodValueLabel;
        private ImpulseMaker.MyNumericUpDown RampPeriodValue;
        private System.Windows.Forms.Label ImpulsePeriodValueLabel;
        private ImpulseMaker.MyNumericUpDown ImpulsePeriodValue;
        private System.Windows.Forms.Label ImpulseTrackBarLabel;
        private System.Windows.Forms.Label ImpulseLevelLabel;
        private System.Windows.Forms.Label ImpulseBaseLabel;
        private ImpulseMaker.MyNumericUpDown ImpulseBaseValue;
        private ImpulseMaker.MyNumericUpDown ImpulseLevelValue;
        private SeveralSlidersTrackBar SeveralSlidersImpulseTrackBar;
        private MyChart WholeSignalChart;
        private System.Windows.Forms.Label SamplingRateLabel;
        private System.Windows.Forms.Label SignalDurationLabel;
        private ImpulseMaker.MyNumericUpDown SignalDurationValue;
        private ImpulseMaker.MyNumericUpDown SamplingRateValue;
        private System.Windows.Forms.TextBox RampChannelName;
        private System.Windows.Forms.Button SaveRampChannelButton;
        private System.Windows.Forms.Label SaveRampChannelLabel;
        private System.Windows.Forms.TextBox ImpulseChannelName;
        private System.Windows.Forms.Button SaveImpulseChannelButton;
        private System.Windows.Forms.Label SaveImpulseChannelLabel;
        private System.Windows.Forms.CheckBox ZeroEndingRampCheckBox;
        private System.Windows.Forms.CheckBox ZeroEndingImpulseCheckBox;
        private System.Windows.Forms.Button DeleteRampChannelButton;
        private System.Windows.Forms.Button DeleteImpulseChannelButton;
        private System.Windows.Forms.ProgressBar SaveAllChannelsProgress;
        private System.Windows.Forms.Label SaveAllChannelsLabel;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem iNIFilePathToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cSVFilePathToolStripMenuItem;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolTip toolTip2;
        private MyListBox ChannelsListBox;
        private System.Windows.Forms.TabPage CustomTabPage;
        private PointCoordinatesListBox pointCoordinatesListBox;
        private System.Windows.Forms.Label CustomPeriodValueLabel;
        private MyNumericUpDown CustomPeriodValue;
        private System.Windows.Forms.CheckBox ZeroEndingCustomCheckBox;
        private System.Windows.Forms.Button DeleteCustomChannelButton;
        private System.Windows.Forms.TextBox CustomChannelName;
        private System.Windows.Forms.Label SaveCustomChannelLabel;
        private System.Windows.Forms.Button SaveCustomChannelButton;
        private MyNumericUpDown SinePeriodValue;
        private System.Windows.Forms.Label SineAngleTrackerLabel;
        private System.Windows.Forms.TrackBar SineAngleTrackBar;
        private System.Windows.Forms.Label SineAngleTrackBarLabel;
        private System.Windows.Forms.Button DeleteSineChannelButton;
        private System.Windows.Forms.CheckBox ZeroEndingSineCheckBox;
        private System.Windows.Forms.TextBox SineChannelName;
        private System.Windows.Forms.Label SaveSineChannelLabel;
        private System.Windows.Forms.Button SaveSineChannelButton;
        private System.Windows.Forms.Label SinePeriodValueLabel;
        private System.Windows.Forms.Label SineAmplitudeLabel;
        private System.Windows.Forms.Label SineLevelLabel;
    }
}

