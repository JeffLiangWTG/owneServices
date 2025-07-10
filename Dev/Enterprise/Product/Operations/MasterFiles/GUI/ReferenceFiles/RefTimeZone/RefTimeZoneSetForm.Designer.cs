using CargoWise.Windows.UI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefTimeZoneSetForm
	{
		#region Windows Form Designer generated code

		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo9 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo10 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo11 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo12 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo13 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo14 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.IsSystemCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.StandardZoneNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StandardZoneGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DaylightSavingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.StandardZoneMilitaryCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StandardZoneCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StandardZoneOffsetCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DaylightSavingZoneGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EndRulesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.EndRulesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.StartRulesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.StartRulesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DSTMilitaryCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DSTStandardCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DSTOffsetCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DSTNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TimeZoneSetTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.StandardZoneGroupBox.SuspendLayout();
			this.DaylightSavingZoneGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EndRulesGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.StartRulesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(843, 608, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.StandardZoneGroupBox);
			this.MainTabPage.Controls.Add(this.DaylightSavingZoneGroupBox);
			this.MainTabPage.Controls.Add(this.TimeZoneSetTextBox);
			this.MainTabPage.Controls.Add(this.IsActiveCheckBox);
			this.MainTabPage.Controls.Add(this.IsSystemCheckBox);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 581, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(843, 24, true);
			this.MainStatusBar.TabIndex = 0;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(414);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(414);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefTimeZoneSet);
			// 
			// StandardZoneNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.StandardZoneNameTextBox, "StandardZone+R2_CivilianTimeZoneFullName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefTimeZoneSet)(null)).StandardZone.R2_CivilianTimeZoneFullName)));
			this.StandardZoneNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.StandardZoneNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 42, true);
			this.StandardZoneNameTextBox.Name = "StandardZoneNameTextBox";
			this.StandardZoneNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 17, true);
			this.StandardZoneNameTextBox.TabIndex = 5;
			// 
			// StandardZoneGroupBox
			// 
			this.StandardZoneGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.StandardZoneGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefTimeZoneSetForm|efb2190b-fc22-4a69-ad32-02fd3f689675", "Standard Time Zone Details");
			this.StandardZoneGroupBox.Controls.Add(this.DaylightSavingCheckBox);
			this.StandardZoneGroupBox.Controls.Add(this.StandardZoneMilitaryCodeTextBox);
			this.StandardZoneGroupBox.Controls.Add(this.StandardZoneCodeTextBox);
			this.StandardZoneGroupBox.Controls.Add(this.StandardZoneOffsetCalcEdit);
			this.StandardZoneGroupBox.Controls.Add(this.StandardZoneNameTextBox);
			this.StandardZoneGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 40, true);
			this.StandardZoneGroupBox.Name = "StandardZoneGroupBox";
			this.StandardZoneGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(807, 104, true);
			this.StandardZoneGroupBox.TabIndex = 2;
			this.StandardZoneGroupBox.TabStop = false;
			// 
			// DaylightSavingCheckBox
			// 
			this.DaylightSavingCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.DaylightSavingCheckBox, "HasDaylightSavings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefTimeZoneSet)(null)).HasDaylightSavings)));
			this.DaylightSavingCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefTimeZoneSetForm|1a416a60-7a0d-411a-b098-6444e34580bf", "Daylight Saving Applicable Checkbox");
			this.DaylightSavingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DaylightSavingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 68, true);
			this.DaylightSavingCheckBox.Name = "DaylightSavingCheckBox";
			this.DaylightSavingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(201, 17, true);
			this.DaylightSavingCheckBox.TabIndex = 8;
			// 
			// StandardZoneMilitaryCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.StandardZoneMilitaryCodeTextBox, "StandardZone+R2_MilitaryTimeZoneCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefTimeZoneSet)(null)).StandardZone.R2_MilitaryTimeZoneCode)));
			this.StandardZoneMilitaryCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(519, 41, true);
			this.StandardZoneMilitaryCodeTextBox.Name = "StandardZoneMilitaryCodeTextBox";
			this.StandardZoneMilitaryCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 17, true);
			this.StandardZoneMilitaryCodeTextBox.TabIndex = 7;
			// 
			// StandardZoneCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.StandardZoneCodeTextBox, "StandardZone+R2_CivilianTimeZoneCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefTimeZoneSet)(null)).StandardZone.R2_CivilianTimeZoneCode)));
			this.StandardZoneCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(519, 16, true);
			this.StandardZoneCodeTextBox.Name = "StandardZoneCodeTextBox";
			this.StandardZoneCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 17, true);
			this.StandardZoneCodeTextBox.TabIndex = 3;
			// 
			// StandardZoneOffsetCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.StandardZoneOffsetCalcEdit, "StandardZone+R2_OffsetMinutesFromUTC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefTimeZoneSet)(null)).StandardZone.R2_OffsetMinutesFromUTC)));
			this.StandardZoneOffsetCalcEdit.Decimals = 0;
			this.StandardZoneOffsetCalcEdit.DecimalPlaces = 0;
			this.StandardZoneOffsetCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 16, true);
			this.StandardZoneOffsetCalcEdit.Name = "StandardZoneOffsetCalcEdit";
			this.StandardZoneOffsetCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 17, true);
			this.StandardZoneOffsetCalcEdit.TabIndex = 1;
			this.StandardZoneOffsetCalcEdit.Text = "0";
			this.StandardZoneOffsetCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DaylightSavingZoneGroupBox
			// 
			this.DaylightSavingZoneGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.DaylightSavingZoneGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefTimeZoneSetForm|e91365bd-0f7f-4554-a398-6dc8399d30cd", "Daylight Savings Time Zone Details");
			this.DaylightSavingZoneGroupBox.Controls.Add(this.EndRulesGrid);
			this.DaylightSavingZoneGroupBox.Controls.Add(this.EndRulesLabel);
			this.DaylightSavingZoneGroupBox.Controls.Add(this.StartRulesLabel);
			this.DaylightSavingZoneGroupBox.Controls.Add(this.StartRulesGrid);
			this.DaylightSavingZoneGroupBox.Controls.Add(this.DSTMilitaryCodeTextBox);
			this.DaylightSavingZoneGroupBox.Controls.Add(this.DSTStandardCodeTextBox);
			this.DaylightSavingZoneGroupBox.Controls.Add(this.DSTOffsetCalcEdit);
			this.DaylightSavingZoneGroupBox.Controls.Add(this.DSTNameTextBox);
			this.DaylightSavingZoneGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 150, true);
			this.DaylightSavingZoneGroupBox.Name = "DaylightSavingZoneGroupBox";
			this.DaylightSavingZoneGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(807, 429, true);
			this.DaylightSavingZoneGroupBox.TabIndex = 3;
			this.DaylightSavingZoneGroupBox.TabStop = false;
			// 
			// EndRulesGrid
			// 
			this.EndRulesGrid.AllowNavigation = false;
			this.EndRulesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.EndRulesGrid, "DaylightSavingZones.EndDateRules");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DaylightSavingTimeZone)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefTimeZoneSet)(null)).DaylightSavingZones)).SyncRoot)).EndDateRules)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefTimeZoneRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DaylightSavingTimeZone)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefTimeZoneSet)(null)).DaylightSavingZones)).SyncRoot)).EndDateRules)).SyncRoot)).R4_FromYear)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefTimeZoneRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DaylightSavingTimeZone)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefTimeZoneSet)(null)).DaylightSavingZones)).SyncRoot)).EndDateRules)).SyncRoot)).R4_ToYear)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefTimeZoneRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DaylightSavingTimeZone)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefTimeZoneSet)(null)).DaylightSavingZones)).SyncRoot)).EndDateRules)).SyncRoot)).R4_DaylightSavingDayWeekDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefTimeZoneRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DaylightSavingTimeZone)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefTimeZoneSet)(null)).DaylightSavingZones)).SyncRoot)).EndDateRules)).SyncRoot)).DayNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefTimeZoneRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DaylightSavingTimeZone)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefTimeZoneSet)(null)).DaylightSavingZones)).SyncRoot)).EndDateRules)).SyncRoot)).R4_DaylightSavingDayName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefTimeZoneRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DaylightSavingTimeZone)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefTimeZoneSet)(null)).DaylightSavingZones)).SyncRoot)).EndDateRules)).SyncRoot)).R4_DaylightSavingMonth)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefTimeZoneRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DaylightSavingTimeZone)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefTimeZoneSet)(null)).DaylightSavingZones)).SyncRoot)).EndDateRules)).SyncRoot)).DateDayNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefTimeZoneRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DaylightSavingTimeZone)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefTimeZoneSet)(null)).DaylightSavingZones)).SyncRoot)).EndDateRules)).SyncRoot)).DateMonth)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.RefTimeZoneRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DaylightSavingTimeZone)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefTimeZoneSet)(null)).DaylightSavingZones)).SyncRoot)).EndDateRules)).SyncRoot)).DaylightSavingChangeTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefTimeZoneRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DaylightSavingTimeZone)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefTimeZoneSet)(null)).DaylightSavingZones)).SyncRoot)).EndDateRules)).SyncRoot)).R4_TypeOfTime)));
			this.EndRulesGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "R4_FromYear";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.ShowGroupSeparators = false;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "R4_ToYear";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.ShowGroupSeparators = false;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zDropEditColumnStyleInfo1.ColumnName = "R4_DaylightSavingDayWeekDate";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefTimeZoneSetForm|3cde0601-3a96-4c22-abfc-31e2b401cbae", "Day Number");
			zDropEditColumnStyleInfo2.ColumnName = "DayNumber";
			zDropEditColumnStyleInfo3.ColumnName = "R4_DaylightSavingDayName";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDropEditColumnStyleInfo4.ColumnName = "R4_DaylightSavingMonth";
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefTimeZoneSetForm|45dc1d3e-4199-4981-a0f8-3a5b52277155", "Date Day");
			zDropEditColumnStyleInfo5.ColumnName = "DateDayNumber";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefTimeZoneSetForm|1f9999e2-da5d-49f8-bf2f-df919700dd0e", "Date Month");
			zDropEditColumnStyleInfo6.ColumnName = "DateMonth";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefTimeZoneSetForm|a1b87832-fad4-4d4e-b5ab-732d2c9af50d", "Change Time");
			zDateEditColumnStyleInfo1.ColumnName = "DaylightSavingChangeTime";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Time;
			zDropEditColumnStyleInfo7.ColumnName = "R4_TypeOfTime";
			this.EndRulesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.EndRulesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.EndRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.EndRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.EndRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.EndRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.EndRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.EndRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.EndRulesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.EndRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.EndRulesGrid.GridId = "29754122-17a1-4aea-bca6-cbca4139ab04";
			this.EndRulesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EndRulesGrid.LayoutKey = "zGrid1";
			this.EndRulesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 282, true);
			this.EndRulesGrid.Name = "EndRulesGrid";
			this.EndRulesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(789, 139, true);
			this.EndRulesGrid.TabIndex = 11;
			// 
			// EndRulesLabel
			// 
			this.EndRulesLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefTimeZoneSetForm|16d0c1ad-1b54-4308-8e57-a1322ce01807", "End Rules");
			this.EndRulesLabel.IsFontBold = true;
			this.EndRulesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 262, true);
			this.EndRulesLabel.Name = "EndRulesLabel";
			this.EndRulesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			this.EndRulesLabel.TabIndex = 10;
			// 
			// StartRulesLabel
			// 
			this.StartRulesLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefTimeZoneSetForm|9cd3f8d8-8542-47a2-8a9e-7bab9defe457", "Start Rules");
			this.StartRulesLabel.IsFontBold = true;
			this.StartRulesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 86, true);
			this.StartRulesLabel.Name = "StartRulesLabel";
			this.StartRulesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 18, true);
			this.StartRulesLabel.TabIndex = 8;
			// 
			// StartRulesGrid
			// 
			this.StartRulesGrid.AllowNavigation = false;
			this.StartRulesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.StartRulesGrid, "DaylightSavingZones.StartDateRules");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DaylightSavingTimeZone)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefTimeZoneSet)(null)).DaylightSavingZones)).SyncRoot)).StartDateRules)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefTimeZoneRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DaylightSavingTimeZone)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefTimeZoneSet)(null)).DaylightSavingZones)).SyncRoot)).StartDateRules)).SyncRoot)).R4_FromYear)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefTimeZoneRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DaylightSavingTimeZone)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefTimeZoneSet)(null)).DaylightSavingZones)).SyncRoot)).StartDateRules)).SyncRoot)).R4_ToYear)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefTimeZoneRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DaylightSavingTimeZone)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefTimeZoneSet)(null)).DaylightSavingZones)).SyncRoot)).StartDateRules)).SyncRoot)).R4_DaylightSavingDayWeekDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefTimeZoneRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DaylightSavingTimeZone)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefTimeZoneSet)(null)).DaylightSavingZones)).SyncRoot)).StartDateRules)).SyncRoot)).DayNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefTimeZoneRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DaylightSavingTimeZone)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefTimeZoneSet)(null)).DaylightSavingZones)).SyncRoot)).StartDateRules)).SyncRoot)).R4_DaylightSavingDayName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefTimeZoneRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DaylightSavingTimeZone)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefTimeZoneSet)(null)).DaylightSavingZones)).SyncRoot)).StartDateRules)).SyncRoot)).R4_DaylightSavingMonth)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefTimeZoneRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DaylightSavingTimeZone)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefTimeZoneSet)(null)).DaylightSavingZones)).SyncRoot)).StartDateRules)).SyncRoot)).DateDayNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefTimeZoneRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DaylightSavingTimeZone)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefTimeZoneSet)(null)).DaylightSavingZones)).SyncRoot)).StartDateRules)).SyncRoot)).DateMonth)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.RefTimeZoneRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DaylightSavingTimeZone)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefTimeZoneSet)(null)).DaylightSavingZones)).SyncRoot)).StartDateRules)).SyncRoot)).DaylightSavingChangeTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefTimeZoneRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.DaylightSavingTimeZone)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefTimeZoneSet)(null)).DaylightSavingZones)).SyncRoot)).StartDateRules)).SyncRoot)).R4_TypeOfTime)));
			this.StartRulesGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "R4_FromYear";
			zCalcEditColumnStyleInfo3.Decimals = 0;
			zCalcEditColumnStyleInfo3.ShowGroupSeparators = false;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "R4_ToYear";
			zCalcEditColumnStyleInfo4.Decimals = 0;
			zCalcEditColumnStyleInfo4.ShowGroupSeparators = false;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zDropEditColumnStyleInfo8.ColumnName = "R4_DaylightSavingDayWeekDate";
			zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zDropEditColumnStyleInfo9.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefTimeZoneSetForm|0b15a92c-d516-4e92-a1be-b8a6efd88010", "Day Number");
			zDropEditColumnStyleInfo9.ColumnName = "DayNumber";
			zDropEditColumnStyleInfo10.ColumnName = "R4_DaylightSavingDayName";
			zDropEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDropEditColumnStyleInfo11.ColumnName = "R4_DaylightSavingMonth";
			zDropEditColumnStyleInfo12.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefTimeZoneSetForm|5fc43697-2895-4bb9-9cb8-d6c9773f2611", "Date Day");
			zDropEditColumnStyleInfo12.ColumnName = "DateDayNumber";
			zDropEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zDropEditColumnStyleInfo13.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefTimeZoneSetForm|ff1b50ec-e6e2-4c54-86a0-507dccad1308", "Date Month");
			zDropEditColumnStyleInfo13.ColumnName = "DateMonth";
			zDropEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefTimeZoneSetForm|ad5b81a9-5ea5-4429-9df9-8e9b9fe8c139", "Change Time");
			zDateEditColumnStyleInfo2.ColumnName = "DaylightSavingChangeTime";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Time;
			zDropEditColumnStyleInfo14.ColumnName = "R4_TypeOfTime";
			this.StartRulesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.StartRulesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.StartRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.StartRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo9);
			this.StartRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo10);
			this.StartRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo11);
			this.StartRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo12);
			this.StartRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo13);
			this.StartRulesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.StartRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo14);
			this.StartRulesGrid.GridId = "7687d794-0abf-422f-9344-96242247cbb1";
			this.StartRulesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.StartRulesGrid.LayoutKey = "zGrid1";
			this.StartRulesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 110, true);
			this.StartRulesGrid.Name = "StartRulesGrid";
			this.StartRulesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(789, 138, true);
			this.StartRulesGrid.TabIndex = 9;
			// 
			// DSTMilitaryCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.DSTMilitaryCodeTextBox, "DaylightSavingZone+R2_MilitaryTimeZoneCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefTimeZoneSet)(null)).DaylightSavingZone.R2_MilitaryTimeZoneCode)));
			this.DSTMilitaryCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(519, 50, true);
			this.DSTMilitaryCodeTextBox.Name = "DSTMilitaryCodeTextBox";
			this.DSTMilitaryCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 17, true);
			this.DSTMilitaryCodeTextBox.TabIndex = 7;
			// 
			// DSTStandardCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.DSTStandardCodeTextBox, "DaylightSavingZone+R2_CivilianTimeZoneCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefTimeZoneSet)(null)).DaylightSavingZone.R2_CivilianTimeZoneCode)));
			this.DSTStandardCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(519, 23, true);
			this.DSTStandardCodeTextBox.Name = "DSTStandardCodeTextBox";
			this.DSTStandardCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 17, true);
			this.DSTStandardCodeTextBox.TabIndex = 3;
			// 
			// DSTOffsetCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DSTOffsetCalcEdit, "DaylightSavingZone+R2_OffsetMinutesFromUTC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefTimeZoneSet)(null)).DaylightSavingZone.R2_OffsetMinutesFromUTC)));
			this.DSTOffsetCalcEdit.Decimals = 0;
			this.DSTOffsetCalcEdit.DecimalPlaces = 0;
			this.DSTOffsetCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 24, true);
			this.DSTOffsetCalcEdit.Name = "DSTOffsetCalcEdit";
			this.DSTOffsetCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 17, true);
			this.DSTOffsetCalcEdit.TabIndex = 1;
			this.DSTOffsetCalcEdit.Text = "0";
			this.DSTOffsetCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DSTNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.DSTNameTextBox, "DaylightSavingZone+R2_CivilianTimeZoneFullName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefTimeZoneSet)(null)).DaylightSavingZone.R2_CivilianTimeZoneFullName)));
			this.DSTNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DSTNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 50, true);
			this.DSTNameTextBox.Name = "DSTNameTextBox";
			this.DSTNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 17, true);
			this.DSTNameTextBox.TabIndex = 5;
			// 
			// TimeZoneSetTextBox
			// 
			this.BindingSource.SetBindingMember(this.TimeZoneSetTextBox, "R3_TimeZoneSetName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefTimeZoneSet)(null)).R3_TimeZoneSetName)));
			this.TimeZoneSetTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TimeZoneSetTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 13, true);
			this.TimeZoneSetTextBox.Name = "TimeZoneSetTextBox";
			this.TimeZoneSetTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(382, 17, true);
			this.TimeZoneSetTextBox.TabIndex = 1;
			// 
			// IsActiveCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsActiveCheckBox, "R3_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefTimeZoneSet)(null)).R3_IsActive)));
			this.IsActiveCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefTimeZoneSetForm|3E5228C7-96FB-4654-BFB9-27E293C2179F", "Is Active");
			this.IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(529, 13, true);
			this.IsActiveCheckBox.Name = "IsActiveCheckBox";
			this.IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 17, true);
			this.IsActiveCheckBox.TabIndex = 2;
			this.IsActiveCheckBox.SelectNextControlNonTabStopNonReadOnly(IsSystemCheckBox, true, true, true);
			// 
			// IsSystemCheckBox
			// 
			this.IsSystemCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.IsSystemCheckBox.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.IsSystemCheckBox, "R3_IsSystem");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefTimeZoneSet)(null)).R3_IsSystem)));
			this.IsSystemCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefTimeZoneSetForm|8bd7320d-e46b-4c0e-9d71-4d03d09a1e17", "Is System");
			this.IsSystemCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsSystemCheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.IsSystemCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(615, 13, true);
			this.IsSystemCheckBox.Name = "IsSystemCheckBox";
			this.IsSystemCheckBox.ReadOnly = true;
			this.IsSystemCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 17, true);
			this.IsSystemCheckBox.TabIndex = 3;
			this.IsSystemCheckBox.UseVisualStyleBackColor = true;
			// 
			// RefTimeZoneSetForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(843, 664, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefTimeZoneSetForm|846e62d1-0544-4d16-ac58-d62cf5eb296b", "Time Zone");
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefTimeZoneSet);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(851, 700, true);
			this.Name = "RefTimeZoneSetForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Load += new System.EventHandler(this.RefTimeZoneSetForm_Load);
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.StandardZoneGroupBox.ResumeLayout(false);
			this.StandardZoneGroupBox.PerformLayout();
			this.DaylightSavingZoneGroupBox.ResumeLayout(false);
			this.DaylightSavingZoneGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EndRulesGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.StartRulesGrid)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion

		private Enterprise.ZArchitecture.ZTextBox StandardZoneNameTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox StandardZoneGroupBox;
		private Enterprise.ZArchitecture.ZCalcEdit StandardZoneOffsetCalcEdit;
		private Enterprise.ZArchitecture.ZTextBox StandardZoneCodeTextBox;
		private Enterprise.ZArchitecture.ZTextBox StandardZoneMilitaryCodeTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox DaylightSavingZoneGroupBox;
		private Enterprise.ZArchitecture.ZTextBox DSTMilitaryCodeTextBox;
		private Enterprise.ZArchitecture.ZTextBox DSTStandardCodeTextBox;
		private Enterprise.ZArchitecture.ZCalcEdit DSTOffsetCalcEdit;
		private Enterprise.ZArchitecture.ZTextBox DSTNameTextBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox DaylightSavingCheckBox;
		private Enterprise.ZArchitecture.ZTextBox TimeZoneSetTextBox;
		private Enterprise.ZArchitecture.ZGrid StartRulesGrid;
		private Enterprise.ZArchitecture.ZLabel EndRulesLabel;
		private Enterprise.ZArchitecture.ZLabel StartRulesLabel;
		private Enterprise.ZArchitecture.ZGrid EndRulesGrid;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsSystemCheckBox;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox IsActiveCheckBox;
	}
}
