using System;
using System.Drawing;
using Enterprise.Customs.SG.V4.Business.CMDMessaging;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.SG.V4.GUI.CMDMessaging
{
	public partial class CMDShipmentUserControl : ZUserControl
	{
		#region Auto Generated

		private ZTextBox messageTextTextBox;
		private ZGroupBox historyGroupBox;
		private ZTextBox messageSummaryTextBox;
		private ZPanel radioButtonPanel;
		private ZLabel historyFilterLabel;
		private ZRadioButton activeOnlyRadioButton;
		private ZRadioButton allRadioButton;
		private ZGroupBox messageTextGroupBox;
		private ZGroupBox messageSummaryGroupBox;
		private ZGrid messageGrid;
		private ZGroupBox messageReplyGroupBox;
		private ZTextBox messageReplyTextBox;
		private ZTextBox currentStatusTextBox;
		private ZLabel currentStatusLabel;
		private ZButton customsEntryNumbersButton;
		private ZTextBox customsEntryNumbersTextBox;
		private ZLabel customsEntryNumberLabel;
		private ZPanel leftPanel;
		private CargoWise.Windows.UI.KSplitter leftPanelSplitter;
		private ZPanel rightPanel;
		private CargoWise.Windows.UI.KSplitter rightPanelSplitter;
		private ZPanel mainPanel;
		private CargoWise.Windows.UI.KSplitter mainPanelSplitter;
		private readonly System.ComponentModel.Container components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			this.messageGrid = new ZGrid();
			this.messageTextTextBox = new ZTextBox();
			this.historyGroupBox = new ZGroupBox();
			this.radioButtonPanel = new ZPanel();
			this.allRadioButton = new ZRadioButton();
			this.activeOnlyRadioButton = new ZRadioButton();
			this.historyFilterLabel = new ZLabel();
			this.currentStatusLabel = new ZLabel();
			this.currentStatusTextBox = new ZTextBox();
			this.messageTextGroupBox = new ZGroupBox();
			this.messageSummaryTextBox = new ZTextBox();
			this.messageReplyGroupBox = new ZGroupBox();
			this.messageReplyTextBox = new ZTextBox();
			this.messageSummaryGroupBox = new ZGroupBox();
			this.customsEntryNumberLabel = new ZLabel();
			this.customsEntryNumbersTextBox = new ZTextBox();
			this.customsEntryNumbersButton = new ZButton();
			this.leftPanel = new ZPanel();
			this.leftPanelSplitter = new CargoWise.Windows.UI.KSplitter();
			this.rightPanel = new ZPanel();
			this.rightPanelSplitter = new CargoWise.Windows.UI.KSplitter();
			this.mainPanel = new ZPanel();
			this.mainPanelSplitter = new CargoWise.Windows.UI.KSplitter();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.messageGrid)).BeginInit();
			this.historyGroupBox.SuspendLayout();
			this.radioButtonPanel.SuspendLayout();
			this.messageTextGroupBox.SuspendLayout();
			this.messageReplyGroupBox.SuspendLayout();
			this.messageSummaryGroupBox.SuspendLayout();
			this.leftPanel.SuspendLayout();
			this.rightPanel.SuspendLayout();
			this.mainPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CMDShipmentWrapper);
			// 
			// MessageGrid
			// 
			this.messageGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.messageGrid, "Messages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((CMDShipmentWrapper)(null)).Messages)));
			this.messageGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("BBFFC167-5B43-4808-8126-DE38D4E22449", "Message No.");
			zTextBoxColumnStyleInfo1.ColumnName = "EM_MessageNum";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("30559FAF-5824-44D7-969A-BCF621147BAC", "Message Type");
			zTextBoxColumnStyleInfo2.ColumnName = "EM_MessageType";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("544A83D7-EF3C-4CA6-84EF-4183D59810FA", "Message Status");
			zTextBoxColumnStyleInfo3.ColumnName = "EM_Status";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("657584AC-ABD8-47B3-BB36-F729F45EC1F8", "Interchange Status");
			zTextBoxColumnStyleInfo4.ColumnName = "EM_InterchangeStatus";
			zTextBoxColumnStyleInfo4.IsMandatory = true;
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("1FE3BBA5-32FB-4AC9-9E81-5525D63AACB6", "User");
			zTextBoxColumnStyleInfo5.ColumnName = "EM_User";
			zTextBoxColumnStyleInfo5.IsMandatory = true;
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("348D3115-A797-4219-ABE2-67523758D742", "Status Last Changed");
			zDateEditColumnStyleInfo1.ColumnName = "EM_StatusDateTime";
			zDateEditColumnStyleInfo1.IsMandatory = true;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.messageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.messageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.messageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.messageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.messageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.messageGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.messageGrid.GridId = "79f133ff-f96d-488a-a187-3e8bb949d046";
			this.messageGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messageGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.messageGrid.IsWholeRowSelectedOnClick = true;
			this.messageGrid.LayoutKey = "MessageGrid";
			this.messageGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 40, true);
			this.messageGrid.Name = "MessageGrid";
			this.messageGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 286, true);
			this.messageGrid.TabIndex = 0;
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((CMDEDIMessage)(((object)(((CMDShipmentWrapper)(null)).Messages)))).EM_MessageNum)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CMDEDIMessage)(((object)(((CMDShipmentWrapper)(null)).Messages)))).EM_MessageNumInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((CMDEDIMessage)(((object)(((CMDShipmentWrapper)(null)).Messages)))).EM_MessageType)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CMDEDIMessage)(((object)(((CMDShipmentWrapper)(null)).Messages)))).EM_MessageTypeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((CMDEDIMessage)(((object)(((CMDShipmentWrapper)(null)).Messages)))).EM_Status)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CMDEDIMessage)(((object)(((CMDShipmentWrapper)(null)).Messages)))).EM_StatusInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((CMDEDIMessage)(((object)(((CMDShipmentWrapper)(null)).Messages)))).EM_InterchangeStatus)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CMDEDIMessage)(((object)(((CMDShipmentWrapper)(null)).Messages)))).EM_InterchangeStatusInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((CMDEDIMessage)(((object)(((CMDShipmentWrapper)(null)).Messages)))).EM_User)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CMDEDIMessage)(((object)(((CMDShipmentWrapper)(null)).Messages)))).EM_UserInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((CMDEDIMessage)(((object)(((CMDShipmentWrapper)(null)).Messages)))).EM_StatusDateTime)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((CMDEDIMessage)(((object)(((CMDShipmentWrapper)(null)).Messages)))).EM_StatusDateTimeInfo)));
			// 
			// MessageTextTextBox
			// 
			this.BindingSource.SetBindingMember(this.messageTextTextBox, "Messages.EM_MessageText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((CMDEDIMessage)(((System.Collections.IList)(((CMDShipmentWrapper)(null)).Messages)).SyncRoot)).EM_MessageText)));
			this.messageTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.messageTextTextBox.Multiline = true;
			this.messageTextTextBox.Name = "MessageTextTextBox";
			this.messageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(279, 223, true);
			this.messageTextTextBox.TabIndex = 0;
			// 
			// HistoryGroupBox
			// 
			this.historyGroupBox.Controls.Add(this.messageGrid);
			this.historyGroupBox.Controls.Add(this.radioButtonPanel);
			this.historyGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.historyGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.historyGroupBox.Name = "HistoryGroupBox";
			this.historyGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 329, true);
			this.historyGroupBox.TabIndex = 0;
			this.historyGroupBox.TabStop = false;
			this.historyGroupBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("19C4B669-B52A-43A9-9557-4763282BF889", "History");
			// 
			// RadioButtonPanel
			// 
			this.radioButtonPanel.Controls.Add(this.allRadioButton);
			this.radioButtonPanel.Controls.Add(this.activeOnlyRadioButton);
			this.radioButtonPanel.Controls.Add(this.historyFilterLabel);
			this.radioButtonPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.radioButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.radioButtonPanel.Name = "RadioButtonPanel";
			this.radioButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 24, true);
			this.radioButtonPanel.TabIndex = 4;
			// 
			// AllRadioButton
			// 
			this.allRadioButton.AutoCheck = false;
			this.allRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.allRadioButton, "ShowAll");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((CMDShipmentWrapper)(null)).ShowAll)));
			this.allRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.allRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(181, 0, true);
			this.allRadioButton.Name = "AllRadioButton";
			this.allRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 17, true);
			this.allRadioButton.TabIndex = 2;
			this.allRadioButton.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("88C8BE95-1FA9-4C48-956E-902FC40BA613", "All");
			// 
			// ActiveOnlyRadioButton
			// 
			this.activeOnlyRadioButton.AutoCheck = false;
			this.activeOnlyRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.activeOnlyRadioButton, "ShowActiveMessagesOnly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((CMDShipmentWrapper)(null)).ShowActiveMessagesOnly)));
			this.activeOnlyRadioButton.Checked = true;
			this.activeOnlyRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.activeOnlyRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(45, 0, true);
			this.activeOnlyRadioButton.Name = "ActiveOnlyRadioButton";
			this.activeOnlyRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 17, true);
			this.activeOnlyRadioButton.TabIndex = 1;
			this.activeOnlyRadioButton.TabStop = true;
			this.activeOnlyRadioButton.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("1F3EEA42-C6E6-4353-A449-8ADF97518436", "Active Messages Only");
			// 
			// HistoryFilterLabel
			// 
			this.historyFilterLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.historyFilterLabel.Name = "HistoryFilterLabel";
			this.historyFilterLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 23, true);
			this.historyFilterLabel.TabIndex = 0;
			this.historyFilterLabel.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("C2CA7031-288D-4F7B-B79F-7B56CF5DA478", "Show:");
			// 
			// CurrentStatusLabel
			// 
			this.currentStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 5, true);
			this.currentStatusLabel.Name = "CurrentStatusLabel";
			this.currentStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 23, true);
			this.currentStatusLabel.TabIndex = 0;
			this.currentStatusLabel.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("D1F60900-E612-4FCC-A69E-71F3D37DCA99", "Current Status:");
			// 
			// CurrentStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.currentStatusTextBox, "CurrentCMDStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((CMDShipmentWrapper)(null)).CurrentCMDStatus)));
			this.currentStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 5, true);
			this.currentStatusTextBox.Name = "CurrentStatusTextBox";
			this.currentStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 20, true);
			this.currentStatusTextBox.TabIndex = 1;
			// 
			// MessageTextGroupBox
			// 
			this.messageTextGroupBox.Controls.Add(this.messageTextTextBox);
			this.messageTextGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.messageTextGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.messageTextGroupBox.Name = "MessageTextGroupBox";
			this.messageTextGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 242, true);
			this.messageTextGroupBox.TabIndex = 0;
			this.messageTextGroupBox.TabStop = false;
			this.messageTextGroupBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("F8360541-E7E2-4051-A71A-2D2284F0055C", "Message Text");
			// 
			// MessageSummaryTextBox
			// 
			this.BindingSource.SetBindingMember(this.messageSummaryTextBox, "Messages.EM_MessageSummary");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((CMDEDIMessage)(((System.Collections.IList)(((CMDShipmentWrapper)(null)).Messages)).SyncRoot)).EM_MessageSummary)));
			this.messageSummaryTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messageSummaryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.messageSummaryTextBox.Multiline = true;
			this.messageSummaryTextBox.Name = "MessageSummaryTextBox";
			this.messageSummaryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 164, true);
			this.messageSummaryTextBox.TabIndex = 0;
			// 
			// MessageReplyGroupBox
			// 
			this.messageReplyGroupBox.Controls.Add(this.messageReplyTextBox);
			this.messageReplyGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messageReplyGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 242, true);
			this.messageReplyGroupBox.Name = "MessageReplyGroupBox";
			this.messageReplyGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 270, true);
			this.messageReplyGroupBox.TabIndex = 0;
			this.messageReplyGroupBox.TabStop = false;
			this.messageReplyGroupBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("8F6BE761-3B83-41FD-8C0F-1EA49FD0ECDD", "Message Reply");
			// 
			// MessageReplyTextBox
			// 
			this.BindingSource.SetBindingMember(this.messageReplyTextBox, "Messages.EM_ReplySummary");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((CMDEDIMessage)(((System.Collections.IList)(((CMDShipmentWrapper)(null)).Messages)).SyncRoot)).EM_ReplySummary)));
			this.messageReplyTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messageReplyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.messageReplyTextBox.Multiline = true;
			this.messageReplyTextBox.Name = "MessageReplyTextBox";
			this.messageReplyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(279, 251, true);
			this.messageReplyTextBox.TabIndex = 0;
			// 
			// MessageSummaryGroupBox
			// 
			this.messageSummaryGroupBox.Controls.Add(this.messageSummaryTextBox);
			this.messageSummaryGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messageSummaryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 329, true);
			this.messageSummaryGroupBox.Name = "MessageSummaryGroupBox";
			this.messageSummaryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 183, true);
			this.messageSummaryGroupBox.TabIndex = 5;
			this.messageSummaryGroupBox.TabStop = false;
			this.messageSummaryGroupBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("F92DA2F2-42A2-476A-AA30-050E42A18A14", "Message Summary");
			// 
			// CustomsEntryNumberLabel
			// 
			this.customsEntryNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 26, true);
			this.customsEntryNumberLabel.Name = "CustomsEntryNumberLabel";
			this.customsEntryNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 23, true);
			this.customsEntryNumberLabel.TabIndex = 2;
			this.customsEntryNumberLabel.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("7C46212B-8888-4610-BBDE-7368B033676E", "Permit / Exemption:");
			// 
			// CustomsEntryNumbersTextBox
			// 
			this.BindingSource.SetBindingMember(this.customsEntryNumbersTextBox, "PermitAndExemptionDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((CMDShipmentWrapper)(null)).PermitAndExemptionDetails)));
			this.customsEntryNumbersTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 28, true);
			this.customsEntryNumbersTextBox.Name = "CustomsEntryNumbersTextBox";
			this.customsEntryNumbersTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 20, true);
			this.customsEntryNumbersTextBox.TabIndex = 3;
			// 
			// CustomsEntryNumbersButton
			// 
			this.customsEntryNumbersButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(477, 27, true);
			this.customsEntryNumbersButton.Name = "CustomsEntryNumbersButton";
			this.customsEntryNumbersButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 23, true);
			this.customsEntryNumbersButton.TabIndex = 4;
			this.customsEntryNumbersButton.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("64CEE092-F563-45D8-84D0-E6042B31D755", "More...");
			this.customsEntryNumbersButton.UseVisualStyleBackColor = true;
			this.customsEntryNumbersButton.Click += new EventHandler(this.SetCustomEntryNumbersButton_Click);
			// 
			// LeftPanel
			// 
			this.leftPanel.Controls.Add(this.leftPanelSplitter);
			this.leftPanel.Controls.Add(this.messageSummaryGroupBox);
			this.leftPanel.Controls.Add(this.historyGroupBox);
			this.leftPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.leftPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.leftPanel.Name = "LeftPanel";
			this.leftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(664, 512, true);
			this.leftPanel.TabIndex = 9;
			// 
			// LeftPanelSplitter
			// 
			this.leftPanelSplitter.Dock = System.Windows.Forms.DockStyle.Top;
			this.leftPanelSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 329, true);
			this.leftPanelSplitter.Name = "LeftPanelSplitter";
			this.leftPanelSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(664, 3, true);
			this.leftPanelSplitter.TabIndex = 1;
			this.leftPanelSplitter.TabStop = false;
			// 
			// RightPanel
			// 
			this.rightPanel.Controls.Add(this.rightPanelSplitter);
			this.rightPanel.Controls.Add(this.messageReplyGroupBox);
			this.rightPanel.Controls.Add(this.messageTextGroupBox);
			this.rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rightPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(534, 0, true);
			this.rightPanel.Name = "RightPanel";
			this.rightPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 512, true);
			this.rightPanel.TabIndex = 10;
			// 
			// RightPanelSplitter
			// 
			this.rightPanelSplitter.Dock = System.Windows.Forms.DockStyle.Top;
			this.rightPanelSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 242, true);
			this.rightPanelSplitter.Name = "RightPanelSplitter";
			this.rightPanelSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 3, true);
			this.rightPanelSplitter.TabIndex = 1;
			this.rightPanelSplitter.TabStop = false;
			// 
			// MainPanel
			// 
			this.mainPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.mainPanel.Controls.Add(this.mainPanelSplitter);
			this.mainPanel.Controls.Add(this.rightPanel);
			this.mainPanel.Controls.Add(this.leftPanel);
			this.mainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 52, true);
			this.mainPanel.Name = "MainPanel";
			this.mainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(819, 512, true);
			this.mainPanel.TabIndex = 11;
			// 
			// MainPanelSplitter
			// 
			this.mainPanelSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(534, 0, true);
			this.mainPanelSplitter.Name = "MainPanelSplitter";
			this.mainPanelSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3, 512, true);
			this.mainPanelSplitter.TabIndex = 0;
			this.mainPanelSplitter.TabStop = false;
			// 
			// CMDShipmentUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.mainPanel);
			this.Controls.Add(this.customsEntryNumbersButton);
			this.Controls.Add(this.customsEntryNumbersTextBox);
			this.Controls.Add(this.currentStatusTextBox);
			this.Controls.Add(this.currentStatusLabel);
			this.Controls.Add(this.customsEntryNumberLabel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(827, 568, true);
			this.Name = "CMDShipmentUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(827, 568, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.messageGrid)).EndInit();
			this.historyGroupBox.ResumeLayout(false);
			this.radioButtonPanel.ResumeLayout(false);
			this.radioButtonPanel.PerformLayout();
			this.messageTextGroupBox.ResumeLayout(false);
			this.messageTextGroupBox.PerformLayout();
			this.messageReplyGroupBox.ResumeLayout(false);
			this.messageReplyGroupBox.PerformLayout();
			this.messageSummaryGroupBox.ResumeLayout(false);
			this.messageSummaryGroupBox.PerformLayout();
			this.leftPanel.ResumeLayout(false);
			this.rightPanel.ResumeLayout(false);
			this.mainPanel.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

		#endregion
	}
}
