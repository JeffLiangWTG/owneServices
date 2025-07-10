using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefCountryStatesForm
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.MainTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LanguageDropButton = new Enterprise.ZArchitecture.GUI.ZDropButtonOnly();
			this.LanguageTextLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CountryCodeFindBox = new Enterprise.MasterFiles.GUI.Internal.ZCodeFindBoxFixedPreBoundMaxLength();
			this.RegionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IsSystemCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zNonWorkingDaysTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.WeekendsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NonWorkingDayMondayCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.NonWorkingDayOverrideCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.NonWorkingDayWednesdayCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.NonWorkingDaySundayCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.NonWorkingDaySaturdayCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.NonWorkingDayFridayCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.NonWorkingDayTuesdayCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.NonWorkingDayThursdayCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.HolidaysGridGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ZGridNonWorkingDayHolidays = new Enterprise.ZArchitecture.ZGrid();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LanguageDropButton)).BeginInit();
			this.CountryCodeFindBox.SuspendLayout();
			this.zNonWorkingDaysTabPage.SuspendLayout();
			this.WeekendsGroupBox.SuspendLayout();
			this.HolidaysGridGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ZGridNonWorkingDayHolidays)).BeginInit();
			this.ZGridNonWorkingDayHolidays.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 336, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(369);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(369);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefCountryStates);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.MainTabPage);
			this.MainTabControl.Controls.Add(this.zNonWorkingDaysTabPage);
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 7, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 288, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryStatesForm|78b3fb9a-f028-4af6-90c6-5d957edb3622", "State");
			this.MainTabPage.Controls.Add(this.CodeTextBox);
			this.MainTabPage.Controls.Add(this.DescriptionTextBox);
			this.MainTabPage.Controls.Add(this.LanguageDropButton);
			this.MainTabPage.Controls.Add(this.LanguageTextLabel);
			this.MainTabPage.Controls.Add(this.CountryCodeFindBox);
			this.MainTabPage.Controls.Add(this.RegionTextBox);
			this.MainTabPage.Controls.Add(this.IsSystemCheckBox);
			this.MainTabPage.Controls.Add(this.IsActiveCheckBox);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MainTabPage.Name = "MainTabPage";
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 127, true);
			this.MainTabPage.TabIndex = 0;
			// 
			// CodeTextBox
			// 
			this.CodeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CodeTextBox, "RW_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCountryStates)(null)).RW_Code)));
			this.CodeTextBox.CaptionResourceString = null;
			this.CodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(178, 7, true);
			this.CodeTextBox.Name = "CodeTextBox";
			this.CodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 20, true);
			this.CodeTextBox.TabIndex = 0;
			// 
			// DescriptionTextBox
			// 
			this.DescriptionTextBox.AllowDrop = true;
			this.DescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "RW_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCountryStates)(null)).RW_Description)));
			this.DescriptionTextBox.CaptionResourceString = null;
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(178, 33, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 20, true);
			this.DescriptionTextBox.TabIndex = 1;
			// 
			// LanguageDropButton
			// 
			this.LanguageDropButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.LanguageDropButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(420, 34, true);
			this.LanguageDropButton.Name = "LanguageDropButton";
			this.LanguageDropButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 18, true);
			this.LanguageDropButton.TabIndex = 2;
			this.LanguageDropButton.ToolTipCaption = null;
			this.LanguageDropButton.UseVisualStyleBackColor = true;
			this.LanguageDropButton.Click += new System.EventHandler(this.LanguageDropButton_Click);
			// 
			// LanguageTextLabel
			// 
			this.LanguageTextLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.LanguageTextLabel.BackColor = System.Drawing.SystemColors.Window;
			this.LanguageTextLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.LanguageTextLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("C6794DFA-DE35-43AE-AB0B-E89CE98115B9", "ENG");
			this.LanguageTextLabel.Cursor = System.Windows.Forms.Cursors.Arrow;
			this.LanguageTextLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LanguageTextLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(390, 33, true);
			this.LanguageTextLabel.Name = "LanguageTextLabel";
			this.LanguageTextLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(29, 20, true);
			this.LanguageTextLabel.TabIndex = 3;
			// 
			// CountryCodeFindBox
			// 
			this.CountryCodeFindBox.AllowDrop = true;
			this.CountryCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CountryCodeFindBox, "RW_RN_NKCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCountryStates)(null)).RW_RN_NKCountryCode)));
			this.CountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(178, 59, true);
			this.CountryCodeFindBox.Name = "CountryCodeFindBox";
			this.CountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CountryCodeFindBox.ParentType = null;
			this.CountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 20, true);
			this.CountryCodeFindBox.TabIndex = 2;
			// 
			// RegionTextBox
			// 
			this.RegionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RegionTextBox, "RW_RegionName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCountryStates)(null)).RW_RegionName)));
			this.RegionTextBox.CaptionResourceString = null;
			this.RegionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(178, 85, true);
			this.RegionTextBox.Name = "RegionTextBox";
			this.RegionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 20, true);
			this.RegionTextBox.TabIndex = 5;
			// 
			// IsSystemCheckBox
			// 
			this.IsSystemCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.IsSystemCheckBox.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.IsSystemCheckBox, "RW_IsSystem");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCountryStates)(null)).RW_IsSystem)));
			this.IsSystemCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryStatesForm|8bd7320d-e46b-4c0e-9d71-4d03d09a1e17", "Is System");
			this.IsSystemCheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.IsSystemCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(508, 9, true);
			this.IsSystemCheckBox.Name = "IsSystemCheckBox";
			this.IsSystemCheckBox.ReadOnly = true;
			this.IsSystemCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 17, true);
			this.IsSystemCheckBox.TabIndex = 6;
			this.IsSystemCheckBox.UseVisualStyleBackColor = true;
			// 
			// IsActiveCheckBox
			// 
			this.IsActiveCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.IsActiveCheckBox, "RW_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCountryStates)(null)).RW_IsActive)));
			this.IsActiveCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryStatesForm|3E5228C7-96FB-4654-BFB9-27E293C2179F", "Is Active");
			this.IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(508, 31, true);
			this.IsActiveCheckBox.Name = "IsActiveCheckBox";
			this.IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 24, true);
			this.IsActiveCheckBox.TabIndex = 7;
			// 
			// zNonWorkingDaysTabPage
			// 
			this.zNonWorkingDaysTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryStatesForm|4B524EE9-EA7F-435C-A042-5B738051A909", "Holidays");
			this.zNonWorkingDaysTabPage.Controls.Add(this.WeekendsGroupBox);
			this.zNonWorkingDaysTabPage.Controls.Add(this.HolidaysGridGroupBox);
			this.zNonWorkingDaysTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zNonWorkingDaysTabPage.Name = "zNonWorkingDaysTabPage";
			this.zNonWorkingDaysTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.zNonWorkingDaysTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 261, true);
			this.zNonWorkingDaysTabPage.TabIndex = 1;
			// 
			// WeekendsGroupBox
			// 
			this.WeekendsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.WeekendsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryStatesForm|Weekends", "Weekends");
			this.WeekendsGroupBox.Controls.Add(this.NonWorkingDayMondayCheckBox);
			this.WeekendsGroupBox.Controls.Add(this.NonWorkingDayOverrideCheckBox);
			this.WeekendsGroupBox.Controls.Add(this.NonWorkingDayWednesdayCheckBox);
			this.WeekendsGroupBox.Controls.Add(this.NonWorkingDaySundayCheckBox);
			this.WeekendsGroupBox.Controls.Add(this.NonWorkingDaySaturdayCheckBox);
			this.WeekendsGroupBox.Controls.Add(this.NonWorkingDayFridayCheckBox);
			this.WeekendsGroupBox.Controls.Add(this.NonWorkingDayTuesdayCheckBox);
			this.WeekendsGroupBox.Controls.Add(this.NonWorkingDayThursdayCheckBox);
			this.WeekendsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.WeekendsGroupBox.Name = "WeekendsGroupBox";
			this.WeekendsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(725, 60, true);
			this.WeekendsGroupBox.TabIndex = 2;
			this.WeekendsGroupBox.TabStop = false;
			// 
			// NonWorkingDayMondayCheckBox
			// 
			this.BindingSource.SetBindingMember(this.NonWorkingDayMondayCheckBox, "IsMondayNonWorkingDay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCountryStates)(null)).IsMondayNonWorkingDay)));
			this.NonWorkingDayMondayCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryStatesForm|NonWorkingDays|Monday", "Mon");
			this.NonWorkingDayMondayCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 25, true);
			this.NonWorkingDayMondayCheckBox.Name = "NonWorkingDayMondayCheckBox";
			this.NonWorkingDayMondayCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 24, true);
			this.NonWorkingDayMondayCheckBox.TabIndex = 2;
			// 
			// NonWorkingDayOverrideCheckBox
			// 
			this.BindingSource.SetBindingMember(this.NonWorkingDayOverrideCheckBox, "IsNonWorkingDaysOverrided");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCountryStates)(null)).IsNonWorkingDaysOverrided)));
			this.NonWorkingDayOverrideCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryStatesForm|NonWorkingDays|Override", "Override");
			this.NonWorkingDayOverrideCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(500, 25, true);
			this.NonWorkingDayOverrideCheckBox.Name = "NonWorkingDayOverrideCheckBox";
			this.NonWorkingDayOverrideCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 24, true);
			this.NonWorkingDayOverrideCheckBox.TabIndex = 8;
			// 
			// NonWorkingDayWednesdayCheckBox
			// 
			this.BindingSource.SetBindingMember(this.NonWorkingDayWednesdayCheckBox, "IsWednesdayNonWorkingDay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCountryStates)(null)).IsWednesdayNonWorkingDay)));
			this.NonWorkingDayWednesdayCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryStatesForm|NonWorkingDays|Wednesday", "Wed");
			this.NonWorkingDayWednesdayCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 25, true);
			this.NonWorkingDayWednesdayCheckBox.Name = "NonWorkingDayWednesdayCheckBox";
			this.NonWorkingDayWednesdayCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 24, true);
			this.NonWorkingDayWednesdayCheckBox.TabIndex = 3;
			// 
			// NonWorkingDaySundayCheckBox
			// 
			this.BindingSource.SetBindingMember(this.NonWorkingDaySundayCheckBox, "IsSundayNonWorkingDay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCountryStates)(null)).IsSundayNonWorkingDay)));
			this.NonWorkingDaySundayCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryStatesForm|NonWorkingDays|Sunday", "Sun");
			this.NonWorkingDaySundayCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(314, 25, true);
			this.NonWorkingDaySundayCheckBox.Name = "NonWorkingDaySundayCheckBox";
			this.NonWorkingDaySundayCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 24, true);
			this.NonWorkingDaySundayCheckBox.TabIndex = 7;
			// 
			// NonWorkingDaySaturdayCheckBox
			// 
			this.BindingSource.SetBindingMember(this.NonWorkingDaySaturdayCheckBox, "IsSaturdayNonWorkingDay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCountryStates)(null)).IsSaturdayNonWorkingDay)));
			this.NonWorkingDaySaturdayCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryStatesForm|NonWorkingDays|Saturday", "Sat");
			this.NonWorkingDaySaturdayCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(263, 25, true);
			this.NonWorkingDaySaturdayCheckBox.Name = "NonWorkingDaySaturdayCheckBox";
			this.NonWorkingDaySaturdayCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 24, true);
			this.NonWorkingDaySaturdayCheckBox.TabIndex = 6;
			// 
			// NonWorkingDayFridayCheckBox
			// 
			this.BindingSource.SetBindingMember(this.NonWorkingDayFridayCheckBox, "IsFridayNonWorkingDay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCountryStates)(null)).IsFridayNonWorkingDay)));
			this.NonWorkingDayFridayCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryStatesForm|NonWorkingDays|Friday", "Fri");
			this.NonWorkingDayFridayCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(212, 25, true);
			this.NonWorkingDayFridayCheckBox.Name = "NonWorkingDayFridayCheckBox";
			this.NonWorkingDayFridayCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 24, true);
			this.NonWorkingDayFridayCheckBox.TabIndex = 5;
			// 
			// NonWorkingDayTuesdayCheckBox
			// 
			this.BindingSource.SetBindingMember(this.NonWorkingDayTuesdayCheckBox, "IsTuesdayNonWorkingDay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCountryStates)(null)).IsTuesdayNonWorkingDay)));
			this.NonWorkingDayTuesdayCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryStatesForm|NonWorkingDays|Tuesday", "Tue");
			this.NonWorkingDayTuesdayCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 25, true);
			this.NonWorkingDayTuesdayCheckBox.Name = "NonWorkingDayTuesdayCheckBox";
			this.NonWorkingDayTuesdayCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 24, true);
			this.NonWorkingDayTuesdayCheckBox.TabIndex = 2;
			// 
			// NonWorkingDayThursdayCheckBox
			// 
			this.BindingSource.SetBindingMember(this.NonWorkingDayThursdayCheckBox, "IsThursdayNonWorkingDay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCountryStates)(null)).IsThursdayNonWorkingDay)));
			this.NonWorkingDayThursdayCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryStatesForm|NonWorkingDays|Thursday", "Thu");
			this.NonWorkingDayThursdayCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 25, true);
			this.NonWorkingDayThursdayCheckBox.Name = "NonWorkingDayThursdayCheckBox";
			this.NonWorkingDayThursdayCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 24, true);
			this.NonWorkingDayThursdayCheckBox.TabIndex = 4;
			// 
			// HolidaysGridGroupBox
			// 
			this.HolidaysGridGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right) | System.Windows.Forms.AnchorStyles.Bottom));
			this.HolidaysGridGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryStatesForm|HolidaysGrid", "Holidays");
			this.HolidaysGridGroupBox.Controls.Add(this.ZGridNonWorkingDayHolidays);
			this.HolidaysGridGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 70, true);
			this.HolidaysGridGroupBox.Name = "HolidaysGridGroupBox";
			this.HolidaysGridGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(725, 185, true);
			this.HolidaysGridGroupBox.TabIndex = 3;
			this.HolidaysGridGroupBox.TabStop = false;
			// 
			// ZGridNonWorkingDayHolidays
			// 
			this.ZGridNonWorkingDayHolidays.AllowNavigation = false;
			this.ZGridNonWorkingDayHolidays.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ZGridNonWorkingDayHolidays, "Holidays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountryStates)(null)).Holidays)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbHoliday)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountryStates)(null)).Holidays)).SyncRoot)).GH_HolidayName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.GlbHoliday)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountryStates)(null)).Holidays)).SyncRoot)).GH_Date)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.GlbHoliday)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountryStates)(null)).Holidays)).SyncRoot)).GH_Recurring)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.GlbHoliday)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountryStates)(null)).Holidays)).SyncRoot)).GH_IsWorkingDay)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbHoliday)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountryStates)(null)).Holidays)).SyncRoot)).CountryStatesApplicability)));
			this.ZGridNonWorkingDayHolidays.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryStates|NonWorkingDays|Description", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "GH_HolidayName";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryStates|NonWorkingDays|Date", "Date");
			zDateEditColumnStyleInfo1.ColumnName = "GH_Date";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryStates|NonWorkingDays|Recurring", "Recurring");
			zCheckBoxColumnStyleInfo1.ColumnName = "GH_Recurring";
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryStates|NonWorkingDays|IsWorkingDay", "Working Day");
			zCheckBoxColumnStyleInfo2.ColumnName = "GH_IsWorkingDay";
			zCheckBoxColumnStyleInfo2.IsReadOnly = true;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryStates|NonWorkingDays|Applicability", "Applicability");
			zTextBoxColumnStyleInfo2.ColumnName = "CountryStatesApplicability";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.ZGridNonWorkingDayHolidays.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ZGridNonWorkingDayHolidays.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ZGridNonWorkingDayHolidays.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ZGridNonWorkingDayHolidays.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.ZGridNonWorkingDayHolidays.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ZGridNonWorkingDayHolidays.GridId = "3a123977-f5f5-4578-be7c-b3d48573184e";
			this.ZGridNonWorkingDayHolidays.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ZGridNonWorkingDayHolidays.LayoutKey = "ZGridNonWorkingDayHolidays";
			this.ZGridNonWorkingDayHolidays.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.ZGridNonWorkingDayHolidays.Name = "ZGridNonWorkingDayHolidays";
			this.ZGridNonWorkingDayHolidays.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(712, 165, true);
			this.ZGridNonWorkingDayHolidays.TabIndex = 9;
			this.ZGridNonWorkingDayHolidays.DoubleClick += new System.EventHandler(this.ZGridNonWorkingDayHolidays_DoubleClick);
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(486, 304, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.TabIndex = 1;
			// 
			// RefCountryStatesForm
			// 
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryStatesForm|2127d064-948d-4e11-9324-a653fb79e4f2", "Country States");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 360, true);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.Controls.Add(this.MainTabControl);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefCountryStates);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 245, true);
			this.Name = "RefCountryStatesForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LanguageDropButton)).EndInit();
			this.CountryCodeFindBox.ResumeLayout(true);
			this.CountryCodeFindBox.PerformLayout();
			this.zNonWorkingDaysTabPage.ResumeLayout(false);
			this.zNonWorkingDaysTabPage.PerformLayout();
			this.WeekendsGroupBox.ResumeLayout(false);
			this.WeekendsGroupBox.PerformLayout();
			this.HolidaysGridGroupBox.ResumeLayout(false);
			this.HolidaysGridGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ZGridNonWorkingDayHolidays)).EndInit();
			this.ZGridNonWorkingDayHolidays.ResumeLayout(false);
			this.ZGridNonWorkingDayHolidays.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private void ZGridNonWorkingDayHolidays_DoubleClick(object sender, EventArgs e)
		{
			if (ZGridNonWorkingDayHolidays.ListManager != null && ZGridNonWorkingDayHolidays.CurrentRowIndex >= 0)
			{
				var controller = ZControllerFactory.Create(ControllerIDs.CountryStatesGlbHoliday);
				controller.SetFormsModalTo(this.FindForm());
				controller.ShowEditForm((GlbHoliday)ZGridNonWorkingDayHolidays.ListManager.List[ZGridNonWorkingDayHolidays.CurrentRowIndex]);
			}
		}

		private void LanguageDropButton_Click(object sender, System.EventArgs e)
		{
			new RefLanguageTextTranslationForm(new Business.RefLanguageTextPage(RefCountryStatesSchema.Constants.Prefix, RefCountryStates.Schema.RW_Description, this.DescriptionTextBox.Text, (BusinessObject)this.BusinessEntity, Res.GetString("8AE67611-D949-45DE-8F74-A671E86E1161", "Country/Region States"), new string[] { RefCountryStatesSchema.Constants.RW_RN_NKCountryCode })).Show();
		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl MainTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage MainTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage zNonWorkingDaysTabPage;
		private Enterprise.ZArchitecture.ZTextBox CodeTextBox;
		private Enterprise.ZArchitecture.ZTextBox DescriptionTextBox;
		private Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private Enterprise.MasterFiles.GUI.Internal.ZCodeFindBoxFixedPreBoundMaxLength CountryCodeFindBox;
		private Enterprise.ZArchitecture.ZTextBox RegionTextBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsSystemCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsActiveCheckBox;
		private Enterprise.ZArchitecture.GUI.ZDropButtonOnly LanguageDropButton;
		private Enterprise.ZArchitecture.ZLabel LanguageTextLabel;
		private Enterprise.ZArchitecture.GUI.ZCheckBox NonWorkingDayMondayCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox NonWorkingDayTuesdayCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox NonWorkingDayWednesdayCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox NonWorkingDayThursdayCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox NonWorkingDayFridayCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox NonWorkingDaySaturdayCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox NonWorkingDaySundayCheckBox;
		private Enterprise.ZArchitecture.ZGrid ZGridNonWorkingDayHolidays;
		private ZArchitecture.GUI.ZCheckBox NonWorkingDayOverrideCheckBox;
		private ZArchitecture.GUI.ZGroupBox WeekendsGroupBox;
		private ZArchitecture.GUI.ZGroupBox HolidaysGridGroupBox;
	}
}
