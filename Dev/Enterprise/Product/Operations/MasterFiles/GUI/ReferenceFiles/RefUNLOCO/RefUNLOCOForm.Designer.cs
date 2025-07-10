namespace Enterprise.MasterFiles.GUI
{
	public partial class RefUNLOCOForm
	{
		#region Windows Form Designer generated code

		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.MainTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DaylightSavingsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LocalDateTimeCodeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.UNLOCOCodeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.LocationDateTimeDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.LocalDateTimeDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.LocalDateTimeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.LocationDateTimeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.UNLOCOIdentifiersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RL_HasDischargeBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RL_HasOutportBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RL_HasTerminalBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RL_HasStoreBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RL_HasRoadBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RL_HasRailBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RL_HasSeaportBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RL_HasAirportBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RL_HasUnloadBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RL_HasCustomsLodgeBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RL_HasPostBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.UNLOCODetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RL_IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RL_IsSystemCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RL_IsUpdatableBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CoordinatesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TimeZoneGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.CoordinateEntryControl = new Enterprise.MasterFiles.GUI.CoordinateEntryControl();
			this.StateGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.RL_RN_NKCountryCodeBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.RL_NameWithDiacriticalsBoundText = new Enterprise.ZArchitecture.ZTextBox();
			this.RL_PortNameBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RL_IATABoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RL_IATARegionCode = new Enterprise.ZArchitecture.ZTextBox();
			this.RL_CodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SettingsTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.LocalCodesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.RefLocoMapsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zStmNoteTabPage1 = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.zLogsTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			this.zPreviousNextControl1 = new Enterprise.ZArchitecture.GUI.ZPreviousNextControl();
			this.TopMaintainUNLOCOPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.DaylightSavingsGroupBox.SuspendLayout();
			this.LocationDateTimeDateEdit.SuspendLayout();
			this.LocalDateTimeDateEdit.SuspendLayout();
			this.UNLOCOIdentifiersGroupBox.SuspendLayout();
			this.UNLOCODetailsGroupBox.SuspendLayout();
			this.TimeZoneGuidFindBox.SuspendLayout();
			this.CoordinateEntryControl.SuspendLayout();
			this.StateGuidFindBox.SuspendLayout();
			this.RL_RN_NKCountryCodeBoundCodeFindBox.SuspendLayout();
			this.SettingsTabControl.SuspendLayout();
			this.LocalCodesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RefLocoMapsGrid)).BeginInit();
			this.RefLocoMapsGrid.SuspendLayout();
			this.zStmNoteTabPage1.SuspendLayout();
			this.zPreviousNextControl1.SuspendLayout();
			this.TopMaintainUNLOCOPanel.SuspendLayout();
			this.ButtonsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 535, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(914, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(914);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefUNLOCO);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.MainTabPage);
			this.MainTabControl.Controls.Add(this.zStmNoteTabPage1);
			this.MainTabControl.Controls.Add(this.zLogsTabPage1);
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(908, 485, true);
			this.MainTabControl.TabIndex = 4;
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefUNLOCOForm|f554b335-9479-4f0f-a8cf-8dfcc832f2fd", "UNLOCO");
			this.MainTabPage.Controls.Add(this.DaylightSavingsGroupBox);
			this.MainTabPage.Controls.Add(this.UNLOCOIdentifiersGroupBox);
			this.MainTabPage.Controls.Add(this.UNLOCODetailsGroupBox);
			this.MainTabPage.Controls.Add(this.SettingsTabControl);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MainTabPage.Name = "MainTabPage";
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 458, true);
			this.MainTabPage.TabIndex = 0;
			// 
			// DaylightSavingsGroupBox
			// 
			this.DaylightSavingsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefUNLOCOForm|7ac6f48b-36d2-4de8-9887-261b36c0b126", "Date/Time Conversion");
			this.DaylightSavingsGroupBox.Controls.Add(this.LocalDateTimeCodeLabel);
			this.DaylightSavingsGroupBox.Controls.Add(this.UNLOCOCodeLabel);
			this.DaylightSavingsGroupBox.Controls.Add(this.LocationDateTimeDateEdit);
			this.DaylightSavingsGroupBox.Controls.Add(this.LocalDateTimeDateEdit);
			this.DaylightSavingsGroupBox.Controls.Add(this.LocalDateTimeLabel);
			this.DaylightSavingsGroupBox.Controls.Add(this.LocationDateTimeLabel);
			this.DaylightSavingsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(342, 309, true);
			this.DaylightSavingsGroupBox.Name = "DaylightSavingsGroupBox";
			this.DaylightSavingsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 128, true);
			this.DaylightSavingsGroupBox.TabIndex = 2;
			this.DaylightSavingsGroupBox.TabStop = false;
			// 
			// LocalDateTimeCodeLabel
			// 
			this.BindingSource.SetBindingMember(this.LocalDateTimeCodeLabel, "CurrentBranchCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefUNLOCO)(null)).CurrentBranchCode)));
			this.LocalDateTimeCodeLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefUNLOCOForm|bd13ccb0-1e8d-42bc-b78c-dbefa32369c7", "XXXXX");
			this.LocalDateTimeCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 17, true);
			this.LocalDateTimeCodeLabel.Name = "LocalDateTimeCodeLabel";
			this.LocalDateTimeCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 13, true);
			this.LocalDateTimeCodeLabel.TabIndex = 14;
			this.LocalDateTimeCodeLabel.Text = "XXXXX";
			// 
			// UNLOCOCodeLabel
			// 
			this.BindingSource.SetBindingMember(this.UNLOCOCodeLabel, "RL_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefUNLOCO)(null)).RL_Code)));
			this.UNLOCOCodeLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefUNLOCOForm|fddbc76a-db17-4941-b7ca-093ec72326ee", "XXXXX");
			this.UNLOCOCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 61, true);
			this.UNLOCOCodeLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.UNLOCOCodeLabel.Name = "UNLOCOCodeLabel";
			this.UNLOCOCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 15, true);
			this.UNLOCOCodeLabel.TabIndex = 12;
			this.UNLOCOCodeLabel.Text = "XXXXX";
			// 
			// LocationDateTimeDateEdit
			// 
			this.LocationDateTimeDateEdit.AllowDrop = true;
			this.LocationDateTimeDateEdit.AutoCompleteMonthThreshold = 1;
			this.LocationDateTimeDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LocationDateTimeDateEdit, "LocationDateTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefUNLOCO)(null)).LocationDateTime)));
			this.LocationDateTimeDateEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefUNLOCOForm|46d7c113-36f0-4ffd-a39c-4852faf50ade", "", "The UNLOCO calculated date and time");
			this.LocationDateTimeDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.LocationDateTimeDateEdit, false);
			this.LocationDateTimeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 102, true);
			this.LocationDateTimeDateEdit.Name = "LocationDateTimeDateEdit";
			this.LocationDateTimeDateEdit.TabIndex = 1;
			// 
			// LocalDateTimeDateEdit
			// 
			this.LocalDateTimeDateEdit.AutoCompleteMonthThreshold = 1;
			this.LocalDateTimeDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LocalDateTimeDateEdit, "LocalDateTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefUNLOCO)(null)).LocalDateTime)));
			this.LocalDateTimeDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.LocalDateTimeDateEdit, false);
			this.LocalDateTimeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 37, true);
			this.LocalDateTimeDateEdit.Name = "LocalDateTimeDateEdit";
			this.LocalDateTimeDateEdit.TabIndex = 0;
			// 
			// LocalDateTimeLabel
			// 
			this.LocalDateTimeLabel.AutoSize = true;
			this.LocalDateTimeLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefUNLOCOForm|0323e84b-ae47-4c1a-8425-a0f8b3dd7fc1", "Date/Time in");
			this.LocalDateTimeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 17, true);
			this.LocalDateTimeLabel.Name = "LocalDateTimeLabel";
			this.LocalDateTimeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 13, true);
			this.LocalDateTimeLabel.TabIndex = 7;
			// 
			// LocationDateTimeLabel
			// 
			this.LocationDateTimeLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefUNLOCOForm|107c008c-dd60-467e-b06a-cab43ac78d2d", "Date/Time in", "Date/Time in .");
			this.LocationDateTimeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 60, true);
			this.LocationDateTimeLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.LocationDateTimeLabel.Name = "LocationDateTimeLabel";
			this.LocationDateTimeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 16, true);
			this.LocationDateTimeLabel.TabIndex = 9;
			// 
			// UNLOCOIdentifiersGroupBox
			// 
			this.UNLOCOIdentifiersGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefUNLOCOForm|17d6fb27-232c-452f-9dbd-151e4adc3209", "Identifiers");
			this.UNLOCOIdentifiersGroupBox.Controls.Add(this.RL_HasDischargeBoundCheckEdit);
			this.UNLOCOIdentifiersGroupBox.Controls.Add(this.RL_HasOutportBoundCheckEdit);
			this.UNLOCOIdentifiersGroupBox.Controls.Add(this.RL_HasTerminalBoundCheckEdit);
			this.UNLOCOIdentifiersGroupBox.Controls.Add(this.RL_HasStoreBoundCheckEdit);
			this.UNLOCOIdentifiersGroupBox.Controls.Add(this.RL_HasRoadBoundCheckEdit);
			this.UNLOCOIdentifiersGroupBox.Controls.Add(this.RL_HasRailBoundCheckEdit);
			this.UNLOCOIdentifiersGroupBox.Controls.Add(this.RL_HasSeaportBoundCheckEdit);
			this.UNLOCOIdentifiersGroupBox.Controls.Add(this.RL_HasAirportBoundCheckEdit);
			this.UNLOCOIdentifiersGroupBox.Controls.Add(this.RL_HasUnloadBoundCheckEdit);
			this.UNLOCOIdentifiersGroupBox.Controls.Add(this.RL_HasCustomsLodgeBoundCheckEdit);
			this.UNLOCOIdentifiersGroupBox.Controls.Add(this.RL_HasPostBoundCheckEdit);
			this.UNLOCOIdentifiersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 309, true);
			this.UNLOCOIdentifiersGroupBox.Name = "UNLOCOIdentifiersGroupBox";
			this.UNLOCOIdentifiersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(326, 128, true);
			this.UNLOCOIdentifiersGroupBox.TabIndex = 1;
			this.UNLOCOIdentifiersGroupBox.TabStop = false;
			// 
			// RL_HasDischargeBoundCheckEdit
			// 
			this.RL_HasDischargeBoundCheckEdit.AutoSize = true;
			this.RL_HasDischargeBoundCheckEdit.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.RL_HasDischargeBoundCheckEdit, "RL_HasDischarge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefUNLOCO)(null)).RL_HasDischarge)));
			this.RL_HasDischargeBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RL_HasDischargeBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(212, 19, true);
			this.RL_HasDischargeBoundCheckEdit.Name = "RL_HasDischargeBoundCheckEdit";
			this.RL_HasDischargeBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			this.RL_HasDischargeBoundCheckEdit.TabIndex = 8;
			this.RL_HasDischargeBoundCheckEdit.UseVisualStyleBackColor = false;
			// 
			// RL_HasOutportBoundCheckEdit
			// 
			this.BindingSource.SetBindingMember(this.RL_HasOutportBoundCheckEdit, "RL_HasOutport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefUNLOCO)(null)).RL_HasOutport)));
			this.RL_HasOutportBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RL_HasOutportBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(211, 72, true);
			this.RL_HasOutportBoundCheckEdit.Name = "RL_HasOutportBoundCheckEdit";
			this.RL_HasOutportBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 17, true);
			this.RL_HasOutportBoundCheckEdit.TabIndex = 10;
			// 
			// RL_HasTerminalBoundCheckEdit
			// 
			this.RL_HasTerminalBoundCheckEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RL_HasTerminalBoundCheckEdit, "RL_HasTerminal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefUNLOCO)(null)).RL_HasTerminal)));
			this.RL_HasTerminalBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RL_HasTerminalBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 99, true);
			this.RL_HasTerminalBoundCheckEdit.Name = "RL_HasTerminalBoundCheckEdit";
			this.RL_HasTerminalBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 17, true);
			this.RL_HasTerminalBoundCheckEdit.TabIndex = 7;
			// 
			// RL_HasStoreBoundCheckEdit
			// 
			this.RL_HasStoreBoundCheckEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RL_HasStoreBoundCheckEdit, "RL_HasStore");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefUNLOCO)(null)).RL_HasStore)));
			this.RL_HasStoreBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RL_HasStoreBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 72, true);
			this.RL_HasStoreBoundCheckEdit.Name = "RL_HasStoreBoundCheckEdit";
			this.RL_HasStoreBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 17, true);
			this.RL_HasStoreBoundCheckEdit.TabIndex = 6;
			// 
			// RL_HasRoadBoundCheckEdit
			// 
			this.RL_HasRoadBoundCheckEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RL_HasRoadBoundCheckEdit, "RL_HasRoad");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefUNLOCO)(null)).RL_HasRoad)));
			this.RL_HasRoadBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RL_HasRoadBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 47, true);
			this.RL_HasRoadBoundCheckEdit.Name = "RL_HasRoadBoundCheckEdit";
			this.RL_HasRoadBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 17, true);
			this.RL_HasRoadBoundCheckEdit.TabIndex = 5;
			// 
			// RL_HasRailBoundCheckEdit
			// 
			this.RL_HasRailBoundCheckEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RL_HasRailBoundCheckEdit, "RL_HasRail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefUNLOCO)(null)).RL_HasRail)));
			this.RL_HasRailBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RL_HasRailBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 20, true);
			this.RL_HasRailBoundCheckEdit.Name = "RL_HasRailBoundCheckEdit";
			this.RL_HasRailBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 17, true);
			this.RL_HasRailBoundCheckEdit.TabIndex = 4;
			// 
			// RL_HasSeaportBoundCheckEdit
			// 
			this.RL_HasSeaportBoundCheckEdit.AutoSize = true;
			this.RL_HasSeaportBoundCheckEdit.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.RL_HasSeaportBoundCheckEdit, "RL_HasSeaport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefUNLOCO)(null)).RL_HasSeaport)));
			this.RL_HasSeaportBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RL_HasSeaportBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(211, 46, true);
			this.RL_HasSeaportBoundCheckEdit.Name = "RL_HasSeaportBoundCheckEdit";
			this.RL_HasSeaportBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 17, true);
			this.RL_HasSeaportBoundCheckEdit.TabIndex = 9;
			this.RL_HasSeaportBoundCheckEdit.UseVisualStyleBackColor = false;
			// 
			// RL_HasAirportBoundCheckEdit
			// 
			this.RL_HasAirportBoundCheckEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RL_HasAirportBoundCheckEdit, "RL_HasAirport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefUNLOCO)(null)).RL_HasAirport)));
			this.RL_HasAirportBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RL_HasAirportBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 99, true);
			this.RL_HasAirportBoundCheckEdit.Name = "RL_HasAirportBoundCheckEdit";
			this.RL_HasAirportBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 17, true);
			this.RL_HasAirportBoundCheckEdit.TabIndex = 3;
			// 
			// RL_HasUnloadBoundCheckEdit
			// 
			this.RL_HasUnloadBoundCheckEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RL_HasUnloadBoundCheckEdit, "RL_HasUnload");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefUNLOCO)(null)).RL_HasUnload)));
			this.RL_HasUnloadBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RL_HasUnloadBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 72, true);
			this.RL_HasUnloadBoundCheckEdit.Name = "RL_HasUnloadBoundCheckEdit";
			this.RL_HasUnloadBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 17, true);
			this.RL_HasUnloadBoundCheckEdit.TabIndex = 2;
			// 
			// RL_HasCustomsLodgeBoundCheckEdit
			// 
			this.RL_HasCustomsLodgeBoundCheckEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RL_HasCustomsLodgeBoundCheckEdit, "RL_HasCustomsLodge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefUNLOCO)(null)).RL_HasCustomsLodge)));
			this.RL_HasCustomsLodgeBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RL_HasCustomsLodgeBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 45, true);
			this.RL_HasCustomsLodgeBoundCheckEdit.Name = "RL_HasCustomsLodgeBoundCheckEdit";
			this.RL_HasCustomsLodgeBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 17, true);
			this.RL_HasCustomsLodgeBoundCheckEdit.TabIndex = 1;
			// 
			// RL_HasPostBoundCheckEdit
			// 
			this.RL_HasPostBoundCheckEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RL_HasPostBoundCheckEdit, "RL_HasPost");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefUNLOCO)(null)).RL_HasPost)));
			this.RL_HasPostBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RL_HasPostBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 19, true);
			this.RL_HasPostBoundCheckEdit.Name = "RL_HasPostBoundCheckEdit";
			this.RL_HasPostBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 17, true);
			this.RL_HasPostBoundCheckEdit.TabIndex = 0;
			// 
			// UNLOCODetailsGroupBox
			// 
			this.UNLOCODetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefUNLOCOForm|84c35517-9269-44b4-8e78-7e8a47c6542a", "Location Details");
			this.UNLOCODetailsGroupBox.Controls.Add(this.RL_IsActiveCheckBox);
			this.UNLOCODetailsGroupBox.Controls.Add(this.RL_IsSystemCheckBox);
			this.UNLOCODetailsGroupBox.Controls.Add(this.RL_IsUpdatableBox);
			this.UNLOCODetailsGroupBox.Controls.Add(this.CoordinatesLabel);
			this.UNLOCODetailsGroupBox.Controls.Add(this.TimeZoneGuidFindBox);
			this.UNLOCODetailsGroupBox.Controls.Add(this.CoordinateEntryControl);
			this.UNLOCODetailsGroupBox.Controls.Add(this.StateGuidFindBox);
			this.UNLOCODetailsGroupBox.Controls.Add(this.RL_RN_NKCountryCodeBoundCodeFindBox);
			this.UNLOCODetailsGroupBox.Controls.Add(this.RL_NameWithDiacriticalsBoundText);
			this.UNLOCODetailsGroupBox.Controls.Add(this.RL_PortNameBoundTextBox);
			this.UNLOCODetailsGroupBox.Controls.Add(this.RL_IATABoundTextBox);
			this.UNLOCODetailsGroupBox.Controls.Add(this.RL_IATARegionCode);
			this.UNLOCODetailsGroupBox.Controls.Add(this.RL_CodeBoundTextBox);
			this.UNLOCODetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.UNLOCODetailsGroupBox.Name = "UNLOCODetailsGroupBox";
			this.UNLOCODetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(485, 295, true);
			this.UNLOCODetailsGroupBox.TabIndex = 0;
			this.UNLOCODetailsGroupBox.TabStop = false;
			// 
			// RL_IsActiveCheckBox
			// 
			this.RL_IsActiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RL_IsActiveCheckBox, "RL_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefUNLOCO)(null)).RL_IsActive)));
			this.RL_IsActiveCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefUNLOCOForm|69d83ca6-1d48-48b6-a2f9-31eab98c3a58", "Is Active");
			this.RL_IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RL_IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(278, 20, true);
			this.RL_IsActiveCheckBox.Name = "RL_IsActiveCheckBox";
			this.RL_IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.RL_IsActiveCheckBox.TabIndex = 3;
			this.RL_IsActiveCheckBox.UseVisualStyleBackColor = true;
			// 
			// RL_IsSystemCheckBox
			// 
			this.RL_IsSystemCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RL_IsSystemCheckBox, "RL_IsSystem");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefUNLOCO)(null)).RL_IsSystem)));
			this.RL_IsSystemCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RL_IsSystemCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(278, 67, true);
			this.RL_IsSystemCheckBox.Name = "RL_IsSystemCheckBox";
			this.RL_IsSystemCheckBox.Enabled = false;
			this.RL_IsSystemCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.RL_IsSystemCheckBox.TabIndex = 5;
			// 
			// RL_IsUpdatableBox
			// 
			this.RL_IsUpdatableBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RL_IsUpdatableBox, "RL_IsUpdatable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefUNLOCO)(null)).RL_IsUpdatable)));
			this.RL_IsUpdatableBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RL_IsUpdatableBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(278, 44, true);
			this.RL_IsUpdatableBox.Name = "RL_IsUpdatableBox";
			this.RL_IsUpdatableBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.RL_IsUpdatableBox.TabIndex = 4;
			this.RL_IsUpdatableBox.UseVisualStyleBackColor = true;
			// 
			// CoordinatesLabel
			// 
			this.CoordinatesLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefUNLOCOForm|33d04bcd-85de-49ff-a6c4-fb2ab7387b9c", "Coordinates");
			this.CoordinatesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(34, 222, true);
			this.CoordinatesLabel.Name = "CoordinatesLabel";
			this.CoordinatesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			this.CoordinatesLabel.TabIndex = 12;
			// 
			// TimeZoneGuidFindBox
			// 
			this.TimeZoneGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TimeZoneGuidFindBox, "RL_R3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.RefUNLOCO)(null)).RL_R3)));
			this.TimeZoneGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 185, true);
			this.TimeZoneGuidFindBox.Name = "TimeZoneGuidFindBox";
			this.TimeZoneGuidFindBox.PopupCaption = null;
			this.TimeZoneGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 20, true);
			this.TimeZoneGuidFindBox.TabIndex = 10;
			// 
			// CoordinateEntryControl
			// 
			this.CoordinateEntryControl.AllowDrop = true;
			this.CoordinateEntryControl.BindToTest = "Coordinates";
			this.CoordinateEntryControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 213, true);
			this.CoordinateEntryControl.Name = "CoordinateEntryControl";
			this.CoordinateEntryControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 73, true);
			this.CoordinateEntryControl.TabIndex = 11;
			// 
			// StateGuidFindBox
			// 
			this.StateGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StateGuidFindBox, "RL_RW");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.RefUNLOCO)(null)).RL_RW)));
			this.StateGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 162, true);
			this.StateGuidFindBox.Name = "StateGuidFindBox";
			this.StateGuidFindBox.PopupCaption = null;
			this.StateGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 20, true);
			this.StateGuidFindBox.TabIndex = 9;
			// 
			// RL_RN_NKCountryCodeBoundCodeFindBox
			// 
			this.RL_RN_NKCountryCodeBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RL_RN_NKCountryCodeBoundCodeFindBox, "RL_RN_NKCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefUNLOCO)(null)).RL_RN_NKCountryCode)));
			this.RL_RN_NKCountryCodeBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 139, true);
			this.RL_RN_NKCountryCodeBoundCodeFindBox.Name = "RL_RN_NKCountryCodeBoundCodeFindBox";
			this.RL_RN_NKCountryCodeBoundCodeFindBox.PopupCaption = null;
			this.RL_RN_NKCountryCodeBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 20, true);
			this.RL_RN_NKCountryCodeBoundCodeFindBox.TabIndex = 8;
			// 
			// RL_NameWithDiacriticalsBoundText
			// 
			this.RL_NameWithDiacriticalsBoundText.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.RL_NameWithDiacriticalsBoundText, "RL_NameWithDiacriticals");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefUNLOCO)(null)).RL_NameWithDiacriticals)));
			this.RL_NameWithDiacriticalsBoundText.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RL_NameWithDiacriticalsBoundText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 116, true);
			this.RL_NameWithDiacriticalsBoundText.Name = "RL_NameWithDiacriticalsBoundText";
			this.RL_NameWithDiacriticalsBoundText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 20, true);
			this.RL_NameWithDiacriticalsBoundText.TabIndex = 7;
			// 
			// RL_PortNameBoundTextBox
			// 
			this.RL_PortNameBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.RL_PortNameBoundTextBox, "RL_PortName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefUNLOCO)(null)).RL_PortName)));
			this.RL_PortNameBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RL_PortNameBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 93, true);
			this.RL_PortNameBoundTextBox.Name = "RL_PortNameBoundTextBox";
			this.RL_PortNameBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 20, true);
			this.RL_PortNameBoundTextBox.TabIndex = 6;
			// 
			// RL_IATABoundTextBox
			// 
			this.RL_IATABoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.RL_IATABoundTextBox, "RL_IATA");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefUNLOCO)(null)).RL_IATA)));
			this.RL_IATABoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 44, true);
			this.RL_IATABoundTextBox.Name = "RL_IATABoundTextBox";
			this.RL_IATABoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.RL_IATABoundTextBox.TabIndex = 1;
			// 
			// RL_IATARegionCode
			// 
			this.RL_IATARegionCode.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.RL_IATARegionCode, "RL_IATARegionCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefUNLOCO)(null)).RL_IATARegionCode)));
			this.RL_IATARegionCode.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("95A8D663-146A-43F0-8B6C-DE80D7EB7B33", "IATA Region Code");
			this.RL_IATARegionCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 70, true);
			this.RL_IATARegionCode.Name = "RL_IATARegionCode";
			this.RL_IATARegionCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.RL_IATARegionCode.TabIndex = 2;
			// 
			// RL_CodeBoundTextBox
			// 
			this.RL_CodeBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.RL_CodeBoundTextBox, "RL_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefUNLOCO)(null)).RL_Code)));
			this.RL_CodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 18, true);
			this.RL_CodeBoundTextBox.Name = "RL_CodeBoundTextBox";
			this.RL_CodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.RL_CodeBoundTextBox.TabIndex = 0;
			// 
			// SettingsTabControl
			// 
			this.SettingsTabControl.Controls.Add(this.LocalCodesTabPage);
			this.SettingsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(499, 8, true);
			this.SettingsTabControl.Name = "SettingsTabControl";
			this.SettingsTabControl.SelectedIndex = 0;
			this.SettingsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 429, true);
			this.SettingsTabControl.TabIndex = 3;
			// 
			// LocalCodesTabPage
			// 
			this.LocalCodesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefUNLOCOForm|0262cc7b-f96e-460f-b2bf-e676f711dc58", "Local Codes");
			this.LocalCodesTabPage.Controls.Add(this.RefLocoMapsGrid);
			this.LocalCodesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LocalCodesTabPage.Name = "LocalCodesTabPage";
			this.LocalCodesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(387, 402, true);
			this.LocalCodesTabPage.TabIndex = 1;
			// 
			// RefLocoMapsGrid
			// 
			this.RefLocoMapsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RefLocoMapsGrid, "RefLocoMaps");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefUNLOCO)(null)).RefLocoMaps)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.RefLocoMap)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefUNLOCO)(null)).RefLocoMaps)).SyncRoot)).RY_RN)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefLocoMap)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefUNLOCO)(null)).RefLocoMaps)).SyncRoot)).RY_SystemUsage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefLocoMap)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefUNLOCO)(null)).RefLocoMaps)).SyncRoot)).RY_LocalPortCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefLocoMap)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefUNLOCO)(null)).RefLocoMaps)).SyncRoot)).RY_RL_NKLocoPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefLocoMap)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefUNLOCO)(null)).RefLocoMaps)).SyncRoot)).RY_IsSystem)));
			this.RefLocoMapsGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "RY_RN";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefUNLOCOForm|223e41c4-9ed7-4326-a8cf-3d71b810917d", "Usage");
			zDropEditColumnStyleInfo1.ColumnName = "RY_SystemUsage";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiControlColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefUNLOCOForm|eba6080c-af61-430b-9d9f-35655d2649ae", "Code");
			zMultiControlColumnStyleInfo1.ColumnName = "RY_LocalPortCode";
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "LocalPortCodeFiledType";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefUNLOCOForm|9828d49f-cddb-4c09-b426-b0ea92c5c80e", "UNLOCO");
			zTextBoxColumnStyleInfo2.ColumnName = "RY_RL_NKLocoPort";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefUNLOCOForm|C0D4909B-219D-4BDA-809A-9583AD198696", "Is System");
			zTextBoxColumnStyleInfo3.ColumnName = "RY_IsSystem";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.RefLocoMapsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.RefLocoMapsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.RefLocoMapsGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.RefLocoMapsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.RefLocoMapsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.RefLocoMapsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RefLocoMapsGrid.GridId = "2dccfb9b-a48e-4de4-a85a-1786cbed5d90";
			this.RefLocoMapsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RefLocoMapsGrid.LayoutKey = "RefLocoMapsGrid";
			this.RefLocoMapsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RefLocoMapsGrid.Name = "RefLocoMapsGrid";
			this.RefLocoMapsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(387, 402, true);
			this.RefLocoMapsGrid.TabIndex = 0;
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 458, true);
			this.zStmNoteTabPage1.TabIndex = 1;
			// 
			// zLogsTabPage1
			// 
			this.zLogsTabPage1.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zLogsTabPage1.Name = "zLogsTabPage1";
			this.zLogsTabPage1.ShouldBeReadOnlyInViewMode = false;
			this.zLogsTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 458, true);
			this.zLogsTabPage1.TabIndex = 2;
			// 
			// zPreviousNextControl1
			// 
			this.zPreviousNextControl1.AllowDrop = true;
			this.zPreviousNextControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 502, true);
			this.zPreviousNextControl1.Name = "zPreviousNextControl1";
			this.zPreviousNextControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 26, true);
			this.zPreviousNextControl1.TabIndex = 1;
			this.zPreviousNextControl1.TabStop = true;
			this.zPreviousNextControl1.Visible = false;
			// 
			// TopMaintainUNLOCOPanel
			// 
			this.TopMaintainUNLOCOPanel.Controls.Add(this.MainTabControl);
			this.TopMaintainUNLOCOPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopMaintainUNLOCOPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopMaintainUNLOCOPanel.Name = "TopMaintainUNLOCOPanel";
			this.TopMaintainUNLOCOPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(914, 491, true);
			this.TopMaintainUNLOCOPanel.TabIndex = 0;
			// 
			// ButtonsUserControl
			// 
			this.ButtonsUserControl.AllowDrop = true;
			this.ButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(605, 502, true);
			this.ButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.ButtonsUserControl.Name = "ButtonsUserControl";
			this.ButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 25, true);
			this.ButtonsUserControl.TabIndex = 0;
			// 
			// RefUNLOCOForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefUNLOCOForm|e108a46e-5ae4-4b9b-8b61-2d26465371a8", "UNLOCO");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(914, 559, true);
			this.Controls.Add(this.zPreviousNextControl1);
			this.Controls.Add(this.TopMaintainUNLOCOPanel);
			this.Controls.Add(this.ButtonsUserControl);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefUNLOCO);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 547, true);
			this.Name = "RefUNLOCOForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.TopMaintainUNLOCOPanel, 0);
			this.Controls.SetChildIndex(this.zPreviousNextControl1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.DaylightSavingsGroupBox.ResumeLayout(false);
			this.DaylightSavingsGroupBox.PerformLayout();
			this.LocationDateTimeDateEdit.ResumeLayout(true);
			this.LocationDateTimeDateEdit.PerformLayout();
			this.LocalDateTimeDateEdit.ResumeLayout(true);
			this.LocalDateTimeDateEdit.PerformLayout();
			this.UNLOCOIdentifiersGroupBox.ResumeLayout(false);
			this.UNLOCOIdentifiersGroupBox.PerformLayout();
			this.UNLOCODetailsGroupBox.ResumeLayout(false);
			this.UNLOCODetailsGroupBox.PerformLayout();
			this.TimeZoneGuidFindBox.ResumeLayout(true);
			this.TimeZoneGuidFindBox.PerformLayout();
			this.CoordinateEntryControl.ResumeLayout(true);
			this.CoordinateEntryControl.PerformLayout();
			this.StateGuidFindBox.ResumeLayout(true);
			this.StateGuidFindBox.PerformLayout();
			this.RL_RN_NKCountryCodeBoundCodeFindBox.ResumeLayout(true);
			this.RL_RN_NKCountryCodeBoundCodeFindBox.PerformLayout();
			this.SettingsTabControl.ResumeLayout(false);
			this.SettingsTabControl.PerformLayout();
			this.LocalCodesTabPage.ResumeLayout(false);
			this.LocalCodesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RefLocoMapsGrid)).EndInit();
			this.RefLocoMapsGrid.ResumeLayout(false);
			this.RefLocoMapsGrid.PerformLayout();
			this.zStmNoteTabPage1.ResumeLayout(false);
			this.zStmNoteTabPage1.PerformLayout();
			this.zPreviousNextControl1.ResumeLayout(true);
			this.zPreviousNextControl1.PerformLayout();
			this.TopMaintainUNLOCOPanel.ResumeLayout(false);
			this.TopMaintainUNLOCOPanel.PerformLayout();
			this.ButtonsUserControl.ResumeLayout(true);
			this.ButtonsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl MainTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage MainTabPage;
		private Enterprise.ZArchitecture.GUI.ZGroupBox DaylightSavingsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit LocationDateTimeDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit LocalDateTimeDateEdit;
		private Enterprise.ZArchitecture.ZLabel LocalDateTimeCodeLabel;
		private Enterprise.ZArchitecture.ZLabel UNLOCOCodeLabel;
		private Enterprise.ZArchitecture.ZLabel LocalDateTimeLabel;
		private Enterprise.ZArchitecture.ZLabel LocationDateTimeLabel;
		private Enterprise.ZArchitecture.GUI.ZGroupBox UNLOCOIdentifiersGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox RL_HasDischargeBoundCheckEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox RL_HasOutportBoundCheckEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox RL_HasTerminalBoundCheckEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox RL_HasStoreBoundCheckEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox RL_HasRoadBoundCheckEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox RL_HasRailBoundCheckEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox RL_HasSeaportBoundCheckEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox RL_HasAirportBoundCheckEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox RL_HasUnloadBoundCheckEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox RL_HasCustomsLodgeBoundCheckEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox RL_HasPostBoundCheckEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox UNLOCODetailsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox TimeZoneGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox StateGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox RL_RN_NKCountryCodeBoundCodeFindBox;
		private Enterprise.ZArchitecture.ZTextBox RL_NameWithDiacriticalsBoundText;
		private Enterprise.ZArchitecture.ZTextBox RL_PortNameBoundTextBox;
		private Enterprise.ZArchitecture.ZTextBox RL_IATABoundTextBox;
		private Enterprise.ZArchitecture.ZTextBox RL_IATARegionCode;
		private Enterprise.ZArchitecture.ZTextBox RL_CodeBoundTextBox;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl SettingsTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage LocalCodesTabPage;
		private Enterprise.ZArchitecture.ZGrid RefLocoMapsGrid;
		private Enterprise.ZArchitecture.GUI.ZStmNoteTabPage zStmNoteTabPage1;
		private Enterprise.ZArchitecture.GUI.ZLogsTabPage zLogsTabPage1;
		private Enterprise.ZArchitecture.GUI.ZPanel TopMaintainUNLOCOPanel;
		private Enterprise.ZArchitecture.GUI.ZPreviousNextControl zPreviousNextControl1;
		private Enterprise.Core.Forms.ZPostingButtonsUserControl ButtonsUserControl;
		private Enterprise.ZArchitecture.ZLabel CoordinatesLabel;
		private Enterprise.ZArchitecture.GUI.ZCheckBox RL_IsActiveCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox RL_IsUpdatableBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox RL_IsSystemCheckBox;
		private CoordinateEntryControl CoordinateEntryControl;
	}
}
