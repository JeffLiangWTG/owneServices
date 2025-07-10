using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MasterData.Common;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	partial class UXMLMatchingDiagnosticToolForm
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
			MonitoringObjects.Clear();
			DeduplicationUtils.DebuggerHubInstance.Clear();
			DeduplicationMonitoringUserControl?.Dispose();
			DeduplicationMonitoringUserControl = null;
			ThresholdsLayout?.Dispose();
			ThresholdsLayout = null;
			OrgMatchThresholdLabel?.Dispose();
			OrgMatchThresholdLabel = null;
			AddressMatchThresholdLabel?.Dispose();
			AddressMatchThresholdLabel = null;
			MatchResultTitleLabel?.Dispose();
			MatchResultTitleLabel = null;
			MatchResultLayout?.Dispose();
			MatchResultLayout = null;

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
		protected new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.LayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.InputValuePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.InputValueTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.XmlValuesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EnterValuesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MatchingLogGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MatchingLogTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DuplicationResultPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DuplicationResultGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DuplicationResultsLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.DeduplicationMonitoringUserControl = new Enterprise.MasterData.GUI.DeduplicationMonitoringUserControl();
			this.ExportResultsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MatchResultGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MatchingButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.IsFromSameSystemCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MatchAddressButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ButtonPlaceholderPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MatchOrgButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ThresholdsLayout = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.OrgMatchThresholdLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AddressMatchThresholdLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MatchResultTitleLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MatchResultLayout = new CargoWise.Windows.UI.KTableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LayoutPanel.SuspendLayout();
			this.InputValuePanel.SuspendLayout();
			this.InputValueTabControl.SuspendLayout();
			this.MatchingLogGroupBox.SuspendLayout();
			this.DuplicationResultPanel.SuspendLayout();
			this.DuplicationResultGroupBox.SuspendLayout();
			this.DuplicationResultsLayoutPanel.SuspendLayout();
			this.DeduplicationMonitoringUserControl.SuspendLayout();
			this.MatchingButtonsPanel.SuspendLayout();
			this.ThresholdsLayout.SuspendLayout();
			this.MatchResultLayout.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 686, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1050, 24, true);
			// 
			// LayoutPanel
			// 
			this.LayoutPanel.AllowDrop = true;
			this.LayoutPanel.AutoScroll = true;
			this.LayoutPanel.ColumnCount = 3;
			this.LayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33185F));
			this.LayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(99)));
			this.LayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 66.66815F));
			this.LayoutPanel.Controls.Add(this.InputValuePanel, 0, 0);
			this.LayoutPanel.Controls.Add(this.MatchingLogGroupBox, 2, 0);
			this.LayoutPanel.Controls.Add(this.DuplicationResultPanel, 2, 2);
			this.LayoutPanel.Controls.Add(this.MatchResultLayout, 2, 1);
			this.LayoutPanel.Controls.Add(this.MatchingButtonsPanel, 1, 1);
			this.LayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LayoutPanel.Name = "LayoutPanel";
			this.LayoutPanel.RowCount = 3;
			this.LayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(120)));
			this.LayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 66.66666F));
			this.LayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.LayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1050, 710, true);
			this.LayoutPanel.TabIndex = 0;
			// 
			// InputValuePanel
			// 
			this.InputValuePanel.Controls.Add(this.InputValueTabControl);
			this.InputValuePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InputValuePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.InputValuePanel.Name = "InputValuePanel";
			this.LayoutPanel.SetRowSpan(this.InputValuePanel, 3);
			this.InputValuePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 704, true);
			this.InputValuePanel.TabIndex = 0;
			// 
			// InputValueTabControl
			// 
			this.InputValueTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.InputValueTabControl.Controls.Add(this.XmlValuesTabPage);
			this.InputValueTabControl.Controls.Add(this.EnterValuesTabPage);
			this.InputValueTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InputValueTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InputValueTabControl.Name = "InputValueTabControl";
			this.InputValueTabControl.SelectedIndex = 0;
			this.InputValueTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 704, true);
			this.InputValueTabControl.TabIndex = 0;
			// 
			// XmlValuesTabPage
			// 
			this.XmlValuesTabPage.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("1dd7ef1f-466b-44f8-8884-0a563076eca9", "XML Values");
			this.XmlValuesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.XmlValuesTabPage.Name = "XmlValuesTabPage";
			this.XmlValuesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.XmlValuesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 677, true);
			this.XmlValuesTabPage.TabIndex = 0;
			this.XmlValuesTabPage.UseVisualStyleBackColor = true;
			this.XmlValuesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.XmlValuesTabPage_InitializeTab));
			// 
			// EnterValuesTabPage
			// 
			this.EnterValuesTabPage.AutoScroll = true;
			this.EnterValuesTabPage.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("eae6536f-40de-4e10-a62a-68cd327ffae5", "Enter Values");
			this.EnterValuesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EnterValuesTabPage.Name = "EnterValuesTabPage";
			this.EnterValuesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.EnterValuesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 677, true);
			this.EnterValuesTabPage.TabIndex = 1;
			this.EnterValuesTabPage.UseVisualStyleBackColor = true;
			this.EnterValuesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.EnterValuesTabPage_InitializeTab));
			// 
			// MatchingLogGroupBox
			// 
			this.MatchingLogGroupBox.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("d14010ab-1257-4146-bbcf-bb290eabf6bf", "UXML Matching Log");
			this.MatchingLogGroupBox.Controls.Add(this.MatchingLogTextBox);
			this.MatchingLogGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MatchingLogGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(418, 3, true);
			this.MatchingLogGroupBox.Name = "MatchingLogGroupBox";
			this.MatchingLogGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(629, 114, true);
			this.MatchingLogGroupBox.TabIndex = 3;
			this.MatchingLogGroupBox.TabStop = false;
			// 
			// MatchingLogTextBox
			// 
			this.MatchingLogTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.MatchingLogTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MatchingLogTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MatchingLogTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MatchingLogTextBox.Multiline = true;
			this.MatchingLogTextBox.Name = "MatchingLogTextBox";
			this.MatchingLogTextBox.ReadOnly = true;
			this.MatchingLogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.MatchingLogTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(623, 95, true);
			this.MatchingLogTextBox.TabIndex = 0;
			// 
			// DuplicationResultPanel
			// 
			this.DuplicationResultPanel.Controls.Add(this.DuplicationResultGroupBox);
			this.DuplicationResultPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DuplicationResultPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(418, 516, true);
			this.DuplicationResultPanel.Name = "DuplicationResultPanel";
			this.DuplicationResultPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(629, 191, true);
			this.DuplicationResultPanel.TabIndex = 5;
			// 
			// DuplicationResultGroupBox
			// 
			this.DuplicationResultGroupBox.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("a52aed8b-bc4d-4885-8f39-669c2fe24a63", "Duplication Results");
			this.DuplicationResultGroupBox.Controls.Add(this.DuplicationResultsLayoutPanel);
			this.DuplicationResultGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DuplicationResultGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DuplicationResultGroupBox.Name = "DuplicationResultGroupBox";
			this.DuplicationResultGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(629, 191, true);
			this.DuplicationResultGroupBox.TabIndex = 0;
			this.DuplicationResultGroupBox.TabStop = false;
			// 
			// DuplicationResultsLayoutPanel
			// 
			this.DuplicationResultsLayoutPanel.ColumnCount = 1;
			this.DuplicationResultsLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.DuplicationResultsLayoutPanel.Controls.Add(this.DeduplicationMonitoringUserControl, 0, 1);
			this.DuplicationResultsLayoutPanel.Controls.Add(this.ExportResultsButton, 0, 0);
			this.DuplicationResultsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DuplicationResultsLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DuplicationResultsLayoutPanel.Name = "DuplicationResultsLayoutPanel";
			this.DuplicationResultsLayoutPanel.RowCount = 3;
			this.DuplicationResultsLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(36)));
			this.DuplicationResultsLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.DuplicationResultsLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(20)));
			this.DuplicationResultsLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(623, 172, true);
			this.DuplicationResultsLayoutPanel.TabIndex = 5;
			// 
			// DeduplicationMonitoringUserControl
			// 
			this.DeduplicationMonitoringUserControl.AllowDrop = true;
			this.DeduplicationMonitoringUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DeduplicationMonitoringUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 39, true);
			this.DeduplicationMonitoringUserControl.Name = "DeduplicationMonitoringUserControl";
			this.DeduplicationMonitoringUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(617, 110, true);
			this.DeduplicationMonitoringUserControl.TabIndex = 4;
			// 
			// ExportResultsButton
			// 
			this.ExportResultsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ExportResultsButton.BackColor = System.Drawing.Color.DodgerBlue;
			this.ExportResultsButton.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("5e89937a-aa08-45e8-ad7d-6c6ce37e60b7", "Export Results");
			this.ExportResultsButton.ForeColor = System.Drawing.Color.White;
			this.ExportResultsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(521, 3, true);
			this.ExportResultsButton.Name = "ExportResultsButton";
			this.ExportResultsButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ExportResultsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 30, true);
			this.ExportResultsButton.TabIndex = 0;
			this.ExportResultsButton.ToolTipCaption = null;
			this.ExportResultsButton.UseVisualStyleBackColor = false;
			this.ExportResultsButton.Click += new System.EventHandler(this.ExportResultsButton_Click);
			// 
			// MatchResultGroupBox
			// 
			this.MatchResultGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MatchResultGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(418, 123, true);
			this.MatchResultGroupBox.Name = "MatchResultGroupBox";
			this.MatchResultGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(629, 387, true);
			this.MatchResultGroupBox.TabIndex = 4;
			this.MatchResultGroupBox.TabStop = false;
			// 
			// MatchingButtonsPanel
			// 
			this.MatchingButtonsPanel.Controls.Add(this.MatchAddressButton);
			this.MatchingButtonsPanel.Controls.Add(this.ButtonPlaceholderPanel);
			this.MatchingButtonsPanel.Controls.Add(this.MatchOrgButton);
			this.MatchingButtonsPanel.Controls.Add(this.IsFromSameSystemCheckBox);
			this.MatchingButtonsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MatchingButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(319, 123, true);
			this.MatchingButtonsPanel.Name = "MatchingButtonsPanel";
			this.MatchingButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 387, true);
			this.MatchingButtonsPanel.TabIndex = 19;
			// 
			// IsFromSameSystemCheckBox
			// 
			this.IsFromSameSystemCheckBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.IsFromSameSystemCheckBox.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("ad1ba4a3-d8a2-4135-a60a-b74703ff1338", "Same System", "Specifies if UXML should be treated as coming from the current system");
			this.IsFromSameSystemCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsFromSameSystemCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 50, true);
			this.IsFromSameSystemCheckBox.Name = "IsFromSameSystemCheckBox";
			this.IsFromSameSystemCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
			this.IsFromSameSystemCheckBox.TabIndex = 3;
			this.IsFromSameSystemCheckBox.UseVisualStyleBackColor = true;
			this.IsFromSameSystemCheckBox.Checked = true;
			this.IsFromSameSystemCheckBox.ReadOnly = true;
			// 
			// MatchAddressButton
			// 
			this.MatchAddressButton.BackColor = System.Drawing.Color.DodgerBlue;
			this.MatchAddressButton.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("5056b50d-7472-453c-9838-a23a8f8d0a06", "Match Address");
			this.MatchAddressButton.Dock = System.Windows.Forms.DockStyle.Top;
			this.MatchAddressButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MatchAddressButton.ForeColor = System.Drawing.Color.White;
			this.MatchAddressButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 100, true);
			this.MatchAddressButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 30, 3, 3, true);
			this.MatchAddressButton.Name = "MatchAddressButton";
			this.MatchAddressButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.MatchAddressButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 50, true);
			this.MatchAddressButton.TabIndex = 2;
			this.MatchAddressButton.ToolTipCaption = null;
			this.MatchAddressButton.UseVisualStyleBackColor = false;
			this.MatchAddressButton.Click += new System.EventHandler(this.MatchAddressButton_Click);
			// 
			// ButtonPlaceholderPanel
			// 
			this.ButtonPlaceholderPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.ButtonPlaceholderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 50, true);
			this.ButtonPlaceholderPanel.Name = "ButtonPlaceholderPanel";
			this.ButtonPlaceholderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 50, true);
			this.ButtonPlaceholderPanel.TabIndex = 20;
			// 
			// MatchOrgButton
			// 
			this.MatchOrgButton.BackColor = System.Drawing.Color.DodgerBlue;
			this.MatchOrgButton.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("51236442-af07-4077-8a7f-951bed8e7f51", "Match Organization");
			this.MatchOrgButton.Dock = System.Windows.Forms.DockStyle.Top;
			this.MatchOrgButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.MatchOrgButton.ForeColor = System.Drawing.Color.White;
			this.MatchOrgButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MatchOrgButton.Name = "MatchOrgButton";
			this.MatchOrgButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.MatchOrgButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 50, true);
			this.MatchOrgButton.TabIndex = 1;
			this.MatchOrgButton.ToolTipCaption = null;
			this.MatchOrgButton.UseVisualStyleBackColor = false;
			this.MatchOrgButton.Click += new System.EventHandler(this.MatchOrgButton_Click);
			//
			// ThresholdsLayout
			//
			this.ThresholdsLayout.Name = "ThresholdsLayout";
			this.ThresholdsLayout.AutoSize = true;
			this.ThresholdsLayout.ColumnCount = 1;
			this.ThresholdsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.ThresholdsLayout.RowCount = 0;
			//
			// OrgMatchThresholdLabel
			//
			this.OrgMatchThresholdLabel.Name = "OrgMatchThresholdLabel";
			this.OrgMatchThresholdLabel.AutoSize = true;
			this.OrgMatchThresholdLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			//
			// AddressMatchThresholdLabel
			//
			this.AddressMatchThresholdLabel.Name = "AddressMatchThresholdLabel";
			this.AddressMatchThresholdLabel.AutoSize = true;
			this.AddressMatchThresholdLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			//
			// MatchResultTitleLabel
			//
			this.MatchResultTitleLabel.Name = "MatchResultTitleLabel";
			this.MatchResultTitleLabel.AutoSize = true;
			this.MatchResultTitleLabel.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("a4ef18c2-a61b-451e-9da8-73b778630616", "Match Result");
			this.MatchResultTitleLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.MatchResultTitleLabel.IsFontBold = true;
			//
			// MatchResultLayout
			//
			this.MatchResultLayout.Name = "MatchResultLayout";
			this.MatchResultLayout.AutoSize = true;
			this.MatchResultLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MatchResultLayout.ColumnCount = 1;
			this.MatchResultLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.MatchResultLayout.RowCount = 3;
			this.MatchResultLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.MatchResultLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.MatchResultLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.MatchResultLayout.Controls.Add(MatchResultTitleLabel, 0, 0);
			this.MatchResultLayout.Controls.Add(ThresholdsLayout, 0, 1);
			this.MatchResultLayout.Controls.Add(MatchResultGroupBox, 0, 2);
			// -------- Components ends --------
			// 
			// UXMLMatchingDiagnosticToolForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("e0ac1387-854b-4b88-8366-48ddec5f48af", "UXML Matching Diagnostic Tool");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1050, 710, true);
			this.Controls.Add(this.LayoutPanel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(789, 680, true);
			this.Name = "UXMLMatchingDiagnosticToolForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Controls.SetChildIndex(this.LayoutPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LayoutPanel.ResumeLayout(false);
			this.LayoutPanel.PerformLayout();
			this.InputValuePanel.ResumeLayout(false);
			this.InputValuePanel.PerformLayout();
			this.InputValueTabControl.ResumeLayout(false);
			this.InputValueTabControl.PerformLayout();
			this.MatchingLogGroupBox.ResumeLayout(false);
			this.MatchingLogGroupBox.PerformLayout();
			this.DuplicationResultPanel.ResumeLayout(false);
			this.DuplicationResultPanel.PerformLayout();
			this.DuplicationResultGroupBox.ResumeLayout(false);
			this.DuplicationResultGroupBox.PerformLayout();
			this.DuplicationResultsLayoutPanel.ResumeLayout(false);
			this.DuplicationResultsLayoutPanel.PerformLayout();
			this.DeduplicationMonitoringUserControl.ResumeLayout(true);
			this.DeduplicationMonitoringUserControl.PerformLayout();
			this.MatchingButtonsPanel.ResumeLayout(false);
			this.MatchingButtonsPanel.PerformLayout();
			this.ThresholdsLayout.ResumeLayout(false);
			this.ThresholdsLayout.PerformLayout();
			this.MatchResultLayout.ResumeLayout(false);
			this.MatchResultLayout.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private void XmlValuesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.XMLValuesTextBox = new RichTextBox();
			this.XmlValuesTabPage.SuspendLayout();
			this.XmlValuesTabPage.Controls.Add(this.XMLValuesTextBox);
			// 
			// XMLValuesTextBox
			// 
			this.XMLValuesTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.XMLValuesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.XMLValuesTextBox.Name = "XMLValuesTextBox";
			this.XMLValuesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(289, 671, true);
			this.XMLValuesTextBox.TabIndex = 0;
			this.XMLValuesTextBox.Text = ResString.GetMultilingualString("6bbf32b2-f5d7-4ada-b1f2-d2b99ed028da", "{0} Paste your {1} element from your UXML into this section {2}", "<OrganizationAddress>", "OrganizationAddress", "</OrganizationAddress>");
			this.XMLValuesTextBox.Leave += new System.EventHandler(this.XMLValuesTextBox_Leave);
			this.XmlValuesTabPage.PerformLayout();
			this.XmlValuesTabPage.ResumeLayout(true);
		}

		private void EnterValuesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.OrganizationNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Address1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Address2TestBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OrganizationCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AddressCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CountryTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PostCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AdditionalAddressTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EmailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FaxTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PhoneTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PortTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RegNumberType = new Enterprise.ZArchitecture.ZTextBox();
			this.RegNumberCode = new Enterprise.ZArchitecture.ZTextBox();
			this.ContactNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UniversalNettingCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UniversalOfficeCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EnterValuesTabPage.SuspendLayout();
			this.EnterValuesTabPage.Controls.Add(this.ContactNameTextBox);
			this.EnterValuesTabPage.Controls.Add(this.UniversalOfficeCodeTextBox);
			this.EnterValuesTabPage.Controls.Add(this.UniversalNettingCodeTextBox);
			this.EnterValuesTabPage.Controls.Add(this.RegNumberCode);
			this.EnterValuesTabPage.Controls.Add(this.RegNumberType);
			this.EnterValuesTabPage.Controls.Add(this.PortTextBox);
			this.EnterValuesTabPage.Controls.Add(this.PhoneTextBox);
			this.EnterValuesTabPage.Controls.Add(this.FaxTextBox);
			this.EnterValuesTabPage.Controls.Add(this.EmailTextBox);
			this.EnterValuesTabPage.Controls.Add(this.StateTextBox);
			this.EnterValuesTabPage.Controls.Add(this.PostCodeTextBox);
			this.EnterValuesTabPage.Controls.Add(this.CityTextBox);
			this.EnterValuesTabPage.Controls.Add(this.CountryTextBox);
			this.EnterValuesTabPage.Controls.Add(this.Address2TestBox);
			this.EnterValuesTabPage.Controls.Add(this.Address1TextBox);
			this.EnterValuesTabPage.Controls.Add(this.OrganizationCodeTextBox);
			this.EnterValuesTabPage.Controls.Add(this.AddressCodeTextBox);
			this.EnterValuesTabPage.Controls.Add(this.AdditionalAddressTextBox);
			this.EnterValuesTabPage.Controls.Add(this.OrganizationNameTextBox);
			// 
			// OrganizationNameTextBox
			// 
			this.OrganizationNameTextBox.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("d91f09a0-1968-4fea-81ae-b90420727af6", "Organization Name");
			this.OrganizationNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 38, true);
			this.OrganizationNameTextBox.Name = "OrganizationNameTextBox";
			this.OrganizationNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.OrganizationNameTextBox.TabIndex = 2;
			// 
			// Address1TextBox
			// 
			this.Address1TextBox.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("b37f735a-5e73-40ab-a9e2-a536bab12974", "Address 1");
			this.Address1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 116, true);
			this.Address1TextBox.Name = "Address1TextBox";
			this.Address1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.Address1TextBox.TabIndex = 5;
			// 
			// Address2TestBox
			// 
			this.Address2TestBox.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("f6b9b4df-a360-4af6-afea-323e61621549", "Address 2");
			this.Address2TestBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 142, true);
			this.Address2TestBox.Name = "Address2TestBox";
			this.Address2TestBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.Address2TestBox.TabIndex = 6;
			// 
			// OrganizationCodeTextBox
			// 
			this.OrganizationCodeTextBox.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("f7488d79-ff18-4b29-896e-85db9fcad208", "Organization Code");
			this.OrganizationCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 12, true);
			this.OrganizationCodeTextBox.Name = "OrganizationCodeTextBox";
			this.OrganizationCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.OrganizationCodeTextBox.TabIndex = 1;
			// 
			// AddressCodeTextBox
			// 
			this.AddressCodeTextBox.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("8d0de2d8-9a35-4935-b74a-37d9bdcd2146", "Address Short Code");
			this.AddressCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 64, true);
			this.AddressCodeTextBox.Name = "AddressCodeTextBox";
			this.AddressCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.AddressCodeTextBox.TabIndex = 3;
			// 
			// CountryTextBox
			// 
			this.CountryTextBox.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("dc0663dd-4993-482f-8793-56f577aff013", "Country/Region Code");
			this.CountryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 168, true);
			this.CountryTextBox.Name = "CountryTextBox";
			this.CountryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.CountryTextBox.TabIndex = 7;
			// 
			// CityTextBox
			// 
			this.CityTextBox.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("b4625140-dc78-40a6-afc5-0aaf9c6d5897", "City");
			this.CityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 194, true);
			this.CityTextBox.Name = "CityTextBox";
			this.CityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.CityTextBox.TabIndex = 8;
			// 
			// PostCodeTextBox
			// 
			this.PostCodeTextBox.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("d0d9558e-a1e1-4775-b2de-6281874b5e39", "Post Code");
			this.PostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 220, true);
			this.PostCodeTextBox.Name = "PostCodeTextBox";
			this.PostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.PostCodeTextBox.TabIndex = 9;
			// 
			// StateTextBox
			// 
			this.StateTextBox.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("0973f743-2978-49b8-8279-384841278fb8", "State");
			this.StateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 246, true);
			this.StateTextBox.Name = "StateTextBox";
			this.StateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.StateTextBox.TabIndex = 10;
			// 
			// AdditionalAddressTextBox
			// 
			this.AdditionalAddressTextBox.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("88d5e451-b333-4c3a-a416-29359b2b4fb6", "Additional Address");
			this.AdditionalAddressTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 90, true);
			this.AdditionalAddressTextBox.Name = "AdditionalAddressTextBox";
			this.AdditionalAddressTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.AdditionalAddressTextBox.TabIndex = 4;
			// 
			// EmailTextBox
			// 
			this.EmailTextBox.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("a8c70683-a7e1-456e-a6c3-90eddf0a6eca", "Email");
			this.EmailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 272, true);
			this.EmailTextBox.Name = "EmailTextBox";
			this.EmailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.EmailTextBox.TabIndex = 11;
			// 
			// FaxTextBox
			// 
			this.FaxTextBox.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("79d61269-a375-4fac-afaa-7ea2d78459a2", "Fax");
			this.FaxTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 298, true);
			this.FaxTextBox.Name = "FaxTextBox";
			this.FaxTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.FaxTextBox.TabIndex = 12;
			// 
			// PhoneTextBox
			// 
			this.PhoneTextBox.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("e65331a2-847e-45d5-98ca-a37090ae1e32", "Phone");
			this.PhoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 324, true);
			this.PhoneTextBox.Name = "PhoneTextBox";
			this.PhoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.PhoneTextBox.TabIndex = 13;
			// 
			// PortTextBox
			// 
			this.PortTextBox.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("c93d5754-9b2b-4e8e-b260-df0c39228dc5", "Port Code");
			this.PortTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 350, true);
			this.PortTextBox.Name = "PortTextBox";
			this.PortTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.PortTextBox.TabIndex = 14;
			// 
			// RegNumberType
			// 
			this.RegNumberType.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("d8685cb1-fb66-4cb2-8e4e-6327906f018b", "Reg Number Type");
			this.RegNumberType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 376, true);
			this.RegNumberType.Name = "RegNumberType";
			this.RegNumberType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.RegNumberType.TabIndex = 15;
			// 
			// RegNumberCode
			// 
			this.RegNumberCode.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("10c9afb7-9dbe-4d08-8fae-fca5aeb96ac5", "Reg Number Code");
			this.RegNumberCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 402, true);
			this.RegNumberCode.Name = "RegNumberCode";
			this.RegNumberCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.RegNumberCode.TabIndex = 16;
			// 
			// ContactNameTextBox
			// 
			this.ContactNameTextBox.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("50315373-ffdd-4f72-8304-a0b440a9b080", "Contact Name");
			this.ContactNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 480, true);
			this.ContactNameTextBox.Name = "ContactNameTextBox";
			this.ContactNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.ContactNameTextBox.TabIndex = 19;
			// 
			// UniversalNettingCodeTextBox
			// 
			this.UniversalNettingCodeTextBox.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("475e5102-7489-4681-ace8-ddd7c9179731", "Universal Netting Code");
			this.UniversalNettingCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 428, true);
			this.UniversalNettingCodeTextBox.Name = "UniversalNettingCodeTextBox";
			this.UniversalNettingCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.UniversalNettingCodeTextBox.TabIndex = 17;
			// 
			// UniversalOfficeCodeTextBox
			// 
			this.UniversalOfficeCodeTextBox.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("d322b122-399a-46c2-8435-9b300fb9f76a", "Universal Office Code");
			this.UniversalOfficeCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 454, true);
			this.UniversalOfficeCodeTextBox.Name = "UniversalOfficeCodeTextBox";
			this.UniversalOfficeCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.UniversalOfficeCodeTextBox.TabIndex = 18;
			this.EnterValuesTabPage.PerformLayout();
			this.EnterValuesTabPage.ResumeLayout(true);

		}

		private KTableLayoutPanel LayoutPanel;
		private ZPanel InputValuePanel;
		private ZGroupBox MatchingLogGroupBox;
		private ZGroupBox MatchResultGroupBox;
		protected ZTextBox MatchingLogTextBox;
		private ZPanel DuplicationResultPanel;
		protected ZTabControl InputValueTabControl;
		private ZTabPage XmlValuesTabPage;
		protected ZTabPage EnterValuesTabPage;
		private ZGroupBox DuplicationResultGroupBox;
		private ZButton ExportResultsButton;
		internal DeduplicationMonitoringUserControl DeduplicationMonitoringUserControl;
		private KTableLayoutPanel DuplicationResultsLayoutPanel;
		private ZPanel MatchingButtonsPanel;
		protected ZButton MatchOrgButton;
		private ZPanel ButtonPlaceholderPanel;
		protected ZButton MatchAddressButton;
		protected ZCheckBox IsFromSameSystemCheckBox;
		protected RichTextBox XMLValuesTextBox;
		protected ZTextBox OrganizationNameTextBox;
		protected ZTextBox Address1TextBox;
		protected ZTextBox Address2TestBox;
		protected ZTextBox OrganizationCodeTextBox;
		protected ZTextBox AddressCodeTextBox;
		protected ZTextBox StateTextBox;
		protected ZTextBox PostCodeTextBox;
		protected ZTextBox CityTextBox;
		protected ZTextBox CountryTextBox;
		protected ZTextBox AdditionalAddressTextBox;
		protected ZTextBox FaxTextBox;
		protected ZTextBox EmailTextBox;
		protected ZTextBox PortTextBox;
		protected ZTextBox PhoneTextBox;
		protected ZTextBox RegNumberType;
		protected ZTextBox RegNumberCode;
		protected ZTextBox ContactNameTextBox;
		protected ZTextBox UniversalOfficeCodeTextBox;
		protected ZTextBox UniversalNettingCodeTextBox;
		protected KTableLayoutPanel ThresholdsLayout;
		protected ZLabel OrgMatchThresholdLabel;
		protected ZLabel AddressMatchThresholdLabel;
		protected ZLabel MatchResultTitleLabel;
		protected KTableLayoutPanel MatchResultLayout;
	}
}
