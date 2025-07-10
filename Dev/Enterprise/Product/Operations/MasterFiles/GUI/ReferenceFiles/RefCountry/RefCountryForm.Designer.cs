using System;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefCountryForm
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.MainTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.isMarkSanctionedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.isActiveCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.isSystemCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AddressValidationRuleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.AWBCurrencyCodeBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.RN_PostcodeValidationRuleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RN_StateProvinceValidationRuleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RN_AddressFormattingRuleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RN_EconomicGroupingDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ISOTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StatesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ZGridStates = new Enterprise.ZArchitecture.ZGrid();
			this.RN_RX_NKLocalCurrencyBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.NameTextBox = new Enterprise.ZArchitecture.ZTranslatableTextControl();
			this.zRulesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.JL_OutturnCommentTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RulesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zRequiredDocuments = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zLabel6 = new Enterprise.ZArchitecture.ZLabel();
			this.RequiredDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zNonWorkingDaysTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.WeekendsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NonWorkingDayMondayCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.NonWorkingDayWednesdayCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.NonWorkingDaySundayCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.NonWorkingDaySaturdayCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.NonWorkingDayFridayCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.NonWorkingDayTuesdayCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.NonWorkingDayThursdayCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.HolidaysGridGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ZGridNonWorkingDayHolidays = new Enterprise.ZArchitecture.ZGrid();
			this.zStmNoteTabPage1 = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.zLogsTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			this.zComplianceRulesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PostingButtonsUserControl.SuspendLayout();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.AddressValidationRuleDropEdit.SuspendLayout();
			this.AWBCurrencyCodeBoundGuidFindBox.SuspendLayout();
			this.RN_PostcodeValidationRuleDropEdit.SuspendLayout();
			this.RN_StateProvinceValidationRuleDropEdit.SuspendLayout();
			this.RN_AddressFormattingRuleDropEdit.SuspendLayout();
			this.RN_EconomicGroupingDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ZGridStates)).BeginInit();
			this.ZGridStates.SuspendLayout();
			this.RN_RX_NKLocalCurrencyBoundGuidFindBox.SuspendLayout();
			this.NameTextBox.SuspendLayout();
			this.zRulesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RulesGrid)).BeginInit();
			this.RulesGrid.SuspendLayout();
			this.zRequiredDocuments.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RequiredDocumentsGrid)).BeginInit();
			this.RequiredDocumentsGrid.SuspendLayout();
			this.zNonWorkingDaysTabPage.SuspendLayout();
			this.WeekendsGroupBox.SuspendLayout();
			this.HolidaysGridGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ZGridNonWorkingDayHolidays)).BeginInit();
			this.ZGridNonWorkingDayHolidays.SuspendLayout();
			this.zStmNoteTabPage1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 512, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(649, 20, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 2;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(649);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefCountry);
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(342, 483, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 25, true);
			this.PostingButtonsUserControl.TabIndex = 1;
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.MainTabPage);
			this.MainTabControl.Controls.Add(this.zRulesTabPage);
			this.MainTabControl.Controls.Add(this.zComplianceRulesTabPage);
			this.MainTabControl.Controls.Add(this.zRequiredDocuments);
			this.MainTabControl.Controls.Add(this.zNonWorkingDaysTabPage);
			this.MainTabControl.Controls.Add(this.zStmNoteTabPage1);
			this.MainTabControl.Controls.Add(this.zLogsTabPage1);
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 7, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(643, 471, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryForm|78b3fb9a-f028-4af6-90c6-5d957edb3622", "Country/Region");
			this.MainTabPage.Controls.Add(this.isMarkSanctionedCheckBox);
			this.MainTabPage.Controls.Add(this.isActiveCheckbox);
			this.MainTabPage.Controls.Add(this.isSystemCheckbox);
			this.MainTabPage.Controls.Add(this.AddressValidationRuleDropEdit);
			this.MainTabPage.Controls.Add(this.zTextBox2);
			this.MainTabPage.Controls.Add(this.zTextBox1);
			this.MainTabPage.Controls.Add(this.AWBCurrencyCodeBoundGuidFindBox);
			this.MainTabPage.Controls.Add(this.RN_PostcodeValidationRuleDropEdit);
			this.MainTabPage.Controls.Add(this.RN_StateProvinceValidationRuleDropEdit);
			this.MainTabPage.Controls.Add(this.RN_AddressFormattingRuleDropEdit);
			this.MainTabPage.Controls.Add(this.RN_EconomicGroupingDropEdit);
			this.MainTabPage.Controls.Add(this.ISOTextBox);
			this.MainTabPage.Controls.Add(this.StatesLabel);
			this.MainTabPage.Controls.Add(this.ZGridStates);
			this.MainTabPage.Controls.Add(this.RN_RX_NKLocalCurrencyBoundGuidFindBox);
			this.MainTabPage.Controls.Add(this.NameTextBox);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.MainTabPage.Name = "MainTabPage";
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(637, 448, true);
			this.MainTabPage.TabIndex = 0;
			// 
			// isMarkSanctionedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.isMarkSanctionedCheckBox, "RN_IsSanctioned");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCountry)(null)).RN_IsSanctioned)));
			this.isMarkSanctionedCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("420043cb-a3fa-4a56-948d-26dfb78a4c94", "Is Sanctioned");
			this.isMarkSanctionedCheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.isMarkSanctionedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(505, 63, true);
			this.isMarkSanctionedCheckBox.Name = "isMarkSanctionedCheckBox";
			this.isMarkSanctionedCheckBox.ReadOnly = true;
			this.isMarkSanctionedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.isMarkSanctionedCheckBox.TabIndex = 4;
			this.isMarkSanctionedCheckBox.UseVisualStyleBackColor = true;
			// 
			// isActiveCheckbox
			// 
			this.BindingSource.SetBindingMember(this.isActiveCheckbox, "RN_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCountry)(null)).RN_IsActive)));
			this.isActiveCheckbox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d66ede8b-5a06-49c3-b782-4fe71d9fef9e", "Is Active");
			this.isActiveCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(505, 13, true);
			this.isActiveCheckbox.Name = "isActiveCheckbox";
			this.isActiveCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 24, true);
			this.isActiveCheckbox.TabIndex = 1;
			// 
			// isSystemCheckbox
			// 
			this.BindingSource.SetBindingMember(this.isSystemCheckbox, "RN_IsSystem");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCountry)(null)).RN_IsSystem)));
			this.isSystemCheckbox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("B3AAC09A-1033-4198-9DCA-F902C77B6A7F", "Is System");
			this.isSystemCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(505, 37, true);
			this.isSystemCheckbox.Name = "isSystemCheckbox";
			this.isSystemCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 24, true);
			this.isSystemCheckbox.TabIndex = 1;
			// 
			// AddressValidationRuleDropEdit
			// 
			this.AddressValidationRuleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AddressValidationRuleDropEdit, "RN_ValidationStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefCountry)(null)).RN_ValidationStatus)));
			this.AddressValidationRuleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 212, true);
			this.AddressValidationRuleDropEdit.Name = "AddressValidationRuleDropEdit";
			this.AddressValidationRuleDropEdit.PreBoundMaxLength = 3;
			this.AddressValidationRuleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 18, true);
			this.AddressValidationRuleDropEdit.TabIndex = 9;
			this.AddressValidationRuleDropEdit.Visible = false;
			// 
			// zTextBox2
			// 
			this.zTextBox2.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.zTextBox2, "RN_IsoNumericUNM49Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCountry)(null)).RN_IsoNumericUNM49Code)));
			this.zTextBox2.CaptionResourceString = null;
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(347, 212, true);
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 18, true);
			this.zTextBox2.TabIndex = 11;
			// 
			// zTextBox1
			// 
			this.zTextBox1.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.zTextBox1, "RN_IsoAlpha3Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCountry)(null)).RN_IsoAlpha3Code)));
			this.zTextBox1.CaptionResourceString = null;
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 212, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 18, true);
			this.zTextBox1.TabIndex = 10;
			// 
			// AWBCurrencyCodeBoundGuidFindBox
			// 
			this.AWBCurrencyCodeBoundGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AWBCurrencyCodeBoundGuidFindBox, "RN_RX_NKAirWaybillCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCountry)(null)).RN_RX_NKAirWaybillCurrency)));
			this.AWBCurrencyCodeBoundGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryForm|cdb5926f-deb6-47d1-9559-4981123f5334", "AWB Currency");
			this.AWBCurrencyCodeBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 89, true);
			this.AWBCurrencyCodeBoundGuidFindBox.Name = "AWBCurrencyCodeBoundGuidFindBox";
			this.AWBCurrencyCodeBoundGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.AWBCurrencyCodeBoundGuidFindBox.ParentType = null;
			this.AWBCurrencyCodeBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 18, true);
			this.AWBCurrencyCodeBoundGuidFindBox.TabIndex = 4;
			this.AWBCurrencyCodeBoundGuidFindBox.TabStop = false;
			// 
			// RN_PostcodeValidationRuleDropEdit
			// 
			this.RN_PostcodeValidationRuleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RN_PostcodeValidationRuleDropEdit, "RN_PostcodeValidationRule");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefCountry)(null)).RN_PostcodeValidationRule)));
			this.RN_PostcodeValidationRuleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 188, true);
			this.RN_PostcodeValidationRuleDropEdit.Name = "RN_PostcodeValidationRuleDropEdit";
			this.RN_PostcodeValidationRuleDropEdit.PreBoundMaxLength = 3;
			this.RN_PostcodeValidationRuleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 18, true);
			this.RN_PostcodeValidationRuleDropEdit.TabIndex = 8;
			// 
			// RN_StateProvinceValidationRuleDropEdit
			// 
			this.RN_StateProvinceValidationRuleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RN_StateProvinceValidationRuleDropEdit, "RN_StateProvinceValidationRule");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefCountry)(null)).RN_StateProvinceValidationRule)));
			this.RN_StateProvinceValidationRuleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 164, true);
			this.RN_StateProvinceValidationRuleDropEdit.Name = "RN_StateProvinceValidationRuleDropEdit";
			this.RN_StateProvinceValidationRuleDropEdit.PreBoundMaxLength = 3;
			this.RN_StateProvinceValidationRuleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 18, true);
			this.RN_StateProvinceValidationRuleDropEdit.TabIndex = 7;
			// 
			// RN_AddressFormattingRuleDropEdit
			// 
			this.RN_AddressFormattingRuleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RN_AddressFormattingRuleDropEdit, "RN_AddressFormattingRule");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefCountry)(null)).RN_AddressFormattingRule)));
			this.RN_AddressFormattingRuleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 139, true);
			this.RN_AddressFormattingRuleDropEdit.Name = "RN_AddressFormattingRuleDropEdit";
			this.RN_AddressFormattingRuleDropEdit.PreBoundMaxLength = 3;
			this.RN_AddressFormattingRuleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 18, true);
			this.RN_AddressFormattingRuleDropEdit.TabIndex = 6;
			// 
			// RN_EconomicGroupingDropEdit
			// 
			this.RN_EconomicGroupingDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RN_EconomicGroupingDropEdit, "RN_EconomicGrouping");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefCountry)(null)).RN_EconomicGrouping)));
			this.RN_EconomicGroupingDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 115, true);
			this.RN_EconomicGroupingDropEdit.Name = "RN_EconomicGroupingDropEdit";
			this.RN_EconomicGroupingDropEdit.PreBoundMaxLength = 3;
			this.RN_EconomicGroupingDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 18, true);
			this.RN_EconomicGroupingDropEdit.TabIndex = 5;
			// 
			// ISOTextBox
			// 
			this.ISOTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ISOTextBox, "RN_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCountry)(null)).RN_Code)));
			this.ISOTextBox.CaptionResourceString = null;
			this.ISOTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 15, true);
			this.ISOTextBox.Name = "ISOTextBox";
			this.ISOTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 18, true);
			this.ISOTextBox.TabIndex = 0;
			// 
			// StatesLabel
			// 
			this.StatesLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryForm|e3cb5d46-1822-410e-8bea-503025780407", "States:");
			this.StatesLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.StatesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 238, true);
			this.StatesLabel.Name = "StatesLabel";
			this.StatesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
			this.StatesLabel.TabIndex = 999;
			// 
			// ZGridStates
			// 
			this.ZGridStates.AllowNavigation = false;
			this.ZGridStates.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ZGridStates, "States");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountry)(null)).States)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCountryStates)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountry)(null)).States)).SyncRoot)).RW_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.MasterFiles.Business.RefCountryStates)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountry)(null)).States)).SyncRoot)).RW_DescriptionMultilingual)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCountryStates)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountry)(null)).States)).SyncRoot)).RW_IsActive)));
			this.ZGridStates.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "RW_Code";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(47);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryStates|3CF68B7B-01AE-44A1-BCC4-447647E881D6", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "RW_DescriptionMultilingual";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zCheckBoxColumnStyleInfo1.ColumnName = "RW_IsActive";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			this.ZGridStates.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ZGridStates.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ZGridStates.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ZGridStates.GridId = "157f8b99-678f-443e-a4fd-1c16fce92b92";
			this.ZGridStates.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ZGridStates.LayoutKey = "zGrid1";
			this.ZGridStates.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 256, true);
			this.ZGridStates.Name = "ZGridStates";
			this.ZGridStates.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ZGridStates.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(625, 190, true);
			this.ZGridStates.TabIndex = 11;
			// 
			// RN_RX_NKLocalCurrencyBoundGuidFindBox
			// 
			this.RN_RX_NKLocalCurrencyBoundGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RN_RX_NKLocalCurrencyBoundGuidFindBox, "RN_RX_NKLocalCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCountry)(null)).RN_RX_NKLocalCurrency)));
			this.RN_RX_NKLocalCurrencyBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 63, true);
			this.RN_RX_NKLocalCurrencyBoundGuidFindBox.Name = "RN_RX_NKLocalCurrencyBoundGuidFindBox";
			this.RN_RX_NKLocalCurrencyBoundGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.RN_RX_NKLocalCurrencyBoundGuidFindBox.ParentType = null;
			this.RN_RX_NKLocalCurrencyBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 18, true);
			this.RN_RX_NKLocalCurrencyBoundGuidFindBox.TabIndex = 3;
			// 
			// NameTextBox
			// 
			this.NameTextBox.AcceptsReturn = false;
			this.NameTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NameTextBox, "RN_Desc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCountry)(null)).RN_Desc)));
			this.NameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.NameTextBox.GridCurrent = null;
			this.NameTextBox.GridMember = null;
			this.NameTextBox.IsLanguageEditingEnabled = true;
			this.NameTextBox.IsMultiLine = false;
			this.NameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 39, true);
			this.NameTextBox.Name = "NameTextBox";
			this.NameTextBox.ReadOnly = false;
			this.NameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 20, true);
			this.NameTextBox.TabIndex = 2;
			// 
			// zRulesTabPage
			// 
			this.zRulesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryForm|7ab76b8e-c8b3-4991-a103-bd47b5889551", "Rules");
			this.zRulesTabPage.Controls.Add(this.JL_OutturnCommentTextBox);
			this.zRulesTabPage.Controls.Add(this.RulesGrid);
			this.zRulesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.zRulesTabPage.Name = "zRulesTabPage";
			this.zRulesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.zRulesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(637, 448, true);
			this.zRulesTabPage.TabIndex = 3;
			// 
			// JL_OutturnCommentTextBox
			// 
			this.JL_OutturnCommentTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.JL_OutturnCommentTextBox, "Rules.R7_Notes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCountryRules)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountry)(null)).Rules)).SyncRoot)).R7_Notes)));
			this.JL_OutturnCommentTextBox.CaptionResourceString = null;
			this.JL_OutturnCommentTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.JL_OutturnCommentTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.JL_OutturnCommentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 286, true);
			this.JL_OutturnCommentTextBox.Multiline = true;
			this.JL_OutturnCommentTextBox.Name = "JL_OutturnCommentTextBox";
			this.JL_OutturnCommentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(625, 73, true);
			this.JL_OutturnCommentTextBox.TabIndex = 2;
			// 
			// RulesGrid
			// 
			this.RulesGrid.AllowNavigation = false;
			this.RulesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RulesGrid, "Rules");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountry)(null)).Rules)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCountryRules)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountry)(null)).Rules)).SyncRoot)).R7_RN_NKOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCountryRules)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountry)(null)).Rules)).SyncRoot)).R7_RN_NKDestination)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.RefCountryRules)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountry)(null)).Rules)).SyncRoot)).R7_RS)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCountryRules)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountry)(null)).Rules)).SyncRoot)).R7_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCountryRules)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountry)(null)).Rules)).SyncRoot)).R7_IsClientVisible)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCountryRules)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountry)(null)).Rules)).SyncRoot)).R7_IsValidationRule)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCountryRules)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountry)(null)).Rules)).SyncRoot)).R7_IsError)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCountryRules)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountry)(null)).Rules)).SyncRoot)).R7_UltimateConsigneeRule)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCountryRules)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountry)(null)).Rules)).SyncRoot)).R7_ShowInner)));
			this.RulesGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "R7_RN_NKOrigin";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "R7_RN_NKDestination";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "R7_RS";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo1.ColumnName = "R7_TransportMode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryRules|FA0B07C7-84F2-4F67-B230-87DF943B85DE", "Is Client Visible");
			zCheckBoxColumnStyleInfo2.ColumnName = "R7_IsClientVisible";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryRules|f2caf859-bf76-4193-86b4-a7b1472cb346", "Is Validation Rule");
			zCheckBoxColumnStyleInfo3.ColumnName = "R7_IsValidationRule";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryRules|8a99d5ed-76a7-4e40-b607-6f2980d034ff", "Is Error");
			zCheckBoxColumnStyleInfo4.ColumnName = "R7_IsError";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("26f91dc0-f196-4dcd-ab1b-3a9c1f1a41b4", "Ult. C\'nee. for eSI", "Ultimate Consignee Required for eSI/MBL", "Specifies if the Ultimate Consignee is required for the eSI/MBL.");
			zDropEditColumnStyleInfo2.ColumnName = "R7_UltimateConsigneeRule";
			zDropEditColumnStyleInfo2.IsVisible = false;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo11.CaptionResourceString = Res.GetData("RefCountryRules|5619ab1b-6b5e-4ace-bfce-1cb52b02be04", "Show Inners");
			zCheckBoxColumnStyleInfo11.ColumnName = "R7_ShowInner";
			zCheckBoxColumnStyleInfo11.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo12.CaptionResourceString = Res.GetData("RefCountryRules|55c7ce8d-1944-4962-83be-7c002c98c3ee", "eBL Not Supported");
			zCheckBoxColumnStyleInfo12.ColumnName = "R7_IsEBLNotSupported";
			zCheckBoxColumnStyleInfo12.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.RulesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.RulesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.RulesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.RulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.RulesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.RulesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.RulesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.RulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.RulesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo11);
			this.RulesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo12);
			this.RulesGrid.GridId = "c5e7339d-ad87-4fb7-a3b3-cd3b872c971c";
			this.RulesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RulesGrid.LayoutKey = "zGrid1";
			this.RulesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.RulesGrid.Name = "RulesGrid";
			this.RulesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(625, 245, true);
			this.RulesGrid.TabIndex = 0;
			// 
			// zRequiredDocuments
			// 
			this.zRequiredDocuments.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryForm|7720b146-d954-4f3e-943a-7bfea2f2a162", "Required Documents");
			this.zRequiredDocuments.Controls.Add(this.zLabel6);
			this.zRequiredDocuments.Controls.Add(this.RequiredDocumentsGrid);
			this.zRequiredDocuments.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.zRequiredDocuments.Name = "zRequiredDocuments";
			this.zRequiredDocuments.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(637, 448, true);
			this.zRequiredDocuments.TabIndex = 4;
			// 
			// zLabel6
			// 
			this.zLabel6.AutoSize = true;
			this.zLabel6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryForm|a2299292-e3a3-4b92-86c2-a8b73cd0ab26", "Note: Leave Origin And/Or Dest. blank if the document is applicable to all countries/regions.", "Note: Leave Origin And/Or Destination blank if the document is applicable to all countries/regions.");
			this.zLabel6.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 9, true);
			this.zLabel6.Name = "zLabel6";
			this.zLabel6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(427, 14, true);
			this.zLabel6.TabIndex = 1;
			// 
			// RequiredDocumentsGrid
			// 
			this.RequiredDocumentsGrid.AllowNavigation = false;
			this.RequiredDocumentsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RequiredDocumentsGrid, "RequiredDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountry)(null)).RequiredDocuments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCountryRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountry)(null)).RequiredDocuments)).SyncRoot)).RD_DocType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCountryRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountry)(null)).RequiredDocuments)).SyncRoot)).DocTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCountryRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountry)(null)).RequiredDocuments)).SyncRoot)).RD_RN_NKOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCountryRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountry)(null)).RequiredDocuments)).SyncRoot)).RD_RN_NKDestination)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCountryRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountry)(null)).RequiredDocuments)).SyncRoot)).RD_DocUsage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCountryRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountry)(null)).RequiredDocuments)).SyncRoot)).RD_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCountryRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountry)(null)).RequiredDocuments)).SyncRoot)).RD_OnConsol)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCountryRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountry)(null)).RequiredDocuments)).SyncRoot)).RD_OnShipment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCountryRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountry)(null)).RequiredDocuments)).SyncRoot)).RD_OnBrokerage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCountryRequiredDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountry)(null)).RequiredDocuments)).SyncRoot)).RD_OnOrder)));
			this.RequiredDocumentsGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo3.ColumnName = "RD_DocType";
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryForm|cf567c39-9337-4469-80d2-bb6eb31d0602", "Description");
			zTextBoxColumnStyleInfo3.ColumnName = "DocTypeDescription";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(135);
			zCodeFindBoxColumnStyleInfo4.ColumnName = "RD_RN_NKOrigin";
			zCodeFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCodeFindBoxColumnStyleInfo5.ColumnName = "RD_RN_NKDestination";
			zCodeFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo3.ColumnName = "RD_DocUsage";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo4.ColumnName = "RD_TransportMode";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo5.ColumnName = "RD_OnConsol";
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCheckBoxColumnStyleInfo6.ColumnName = "RD_OnShipment";
			zCheckBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo7.ColumnName = "RD_OnBrokerage";
			zCheckBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zCheckBoxColumnStyleInfo8.ColumnName = "RD_OnOrder";
			zCheckBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.RequiredDocumentsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.RequiredDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.RequiredDocumentsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo4);
			this.RequiredDocumentsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo5);
			this.RequiredDocumentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.RequiredDocumentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.RequiredDocumentsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.RequiredDocumentsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			this.RequiredDocumentsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo7);
			this.RequiredDocumentsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo8);
			this.RequiredDocumentsGrid.GridId = "bd10753e-08f6-4555-8f0d-125251376f7c";
			this.RequiredDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RequiredDocumentsGrid.LayoutKey = "zGrid2";
			this.RequiredDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 32, true);
			this.RequiredDocumentsGrid.Name = "RequiredDocumentsGrid";
			this.RequiredDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 299, true);
			this.RequiredDocumentsGrid.TabIndex = 0;
			// 
			// zNonWorkingDaysTabPage
			// 
			this.zNonWorkingDaysTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryForm|Holidays", "Holidays");
			this.zNonWorkingDaysTabPage.Controls.Add(this.WeekendsGroupBox);
			this.zNonWorkingDaysTabPage.Controls.Add(this.HolidaysGridGroupBox);
			this.zNonWorkingDaysTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zNonWorkingDaysTabPage.Name = "zNonWorkingDaysTabPage";
			this.zNonWorkingDaysTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.zNonWorkingDaysTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(635, 444, true);
			this.zNonWorkingDaysTabPage.TabIndex = 1;
			// 
			// WeekendsGroupBox
			// 
			this.WeekendsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.WeekendsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryForm|Weekends", "Weekends");
			this.WeekendsGroupBox.Controls.Add(this.NonWorkingDayMondayCheckBox);
			this.WeekendsGroupBox.Controls.Add(this.NonWorkingDayWednesdayCheckBox);
			this.WeekendsGroupBox.Controls.Add(this.NonWorkingDaySundayCheckBox);
			this.WeekendsGroupBox.Controls.Add(this.NonWorkingDaySaturdayCheckBox);
			this.WeekendsGroupBox.Controls.Add(this.NonWorkingDayFridayCheckBox);
			this.WeekendsGroupBox.Controls.Add(this.NonWorkingDayTuesdayCheckBox);
			this.WeekendsGroupBox.Controls.Add(this.NonWorkingDayThursdayCheckBox);
			this.WeekendsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.WeekendsGroupBox.Name = "WeekendsGroupBox";
			this.WeekendsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 60, true);
			this.WeekendsGroupBox.TabIndex = 2;
			this.WeekendsGroupBox.TabStop = false;
			// 
			// NonWorkingDayMondayCheckBox
			// 
			this.BindingSource.SetBindingMember(this.NonWorkingDayMondayCheckBox, "IsMondayNonWorkingDay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCountry)(null)).IsMondayNonWorkingDay)));
			this.NonWorkingDayMondayCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryForm|NonWorkingDays|Monday", "Mon");
			this.NonWorkingDayMondayCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 25, true);
			this.NonWorkingDayMondayCheckBox.Name = "NonWorkingDayMondayCheckBox";
			this.NonWorkingDayMondayCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 24, true);
			this.NonWorkingDayMondayCheckBox.TabIndex = 2;
			// 
			// NonWorkingDayWednesdayCheckBox
			// 
			this.BindingSource.SetBindingMember(this.NonWorkingDayWednesdayCheckBox, "IsWednesdayNonWorkingDay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCountry)(null)).IsWednesdayNonWorkingDay)));
			this.NonWorkingDayWednesdayCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryForm|NonWorkingDays|Wednesday", "Wed");
			this.NonWorkingDayWednesdayCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 25, true);
			this.NonWorkingDayWednesdayCheckBox.Name = "NonWorkingDayWednesdayCheckBox";
			this.NonWorkingDayWednesdayCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 24, true);
			this.NonWorkingDayWednesdayCheckBox.TabIndex = 3;
			// 
			// NonWorkingDaySundayCheckBox
			// 
			this.BindingSource.SetBindingMember(this.NonWorkingDaySundayCheckBox, "IsSundayNonWorkingDay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCountry)(null)).IsSundayNonWorkingDay)));
			this.NonWorkingDaySundayCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryForm|NonWorkingDays|Sunday", "Sun");
			this.NonWorkingDaySundayCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(314, 25, true);
			this.NonWorkingDaySundayCheckBox.Name = "NonWorkingDaySundayCheckBox";
			this.NonWorkingDaySundayCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 24, true);
			this.NonWorkingDaySundayCheckBox.TabIndex = 7;
			// 
			// NonWorkingDaySaturdayCheckBox
			// 
			this.BindingSource.SetBindingMember(this.NonWorkingDaySaturdayCheckBox, "IsSaturdayNonWorkingDay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCountry)(null)).IsSaturdayNonWorkingDay)));
			this.NonWorkingDaySaturdayCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryForm|NonWorkingDays|Saturday", "Sat");
			this.NonWorkingDaySaturdayCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(263, 25, true);
			this.NonWorkingDaySaturdayCheckBox.Name = "NonWorkingDaySaturdayCheckBox";
			this.NonWorkingDaySaturdayCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 24, true);
			this.NonWorkingDaySaturdayCheckBox.TabIndex = 6;
			// 
			// NonWorkingDayFridayCheckBox
			// 
			this.BindingSource.SetBindingMember(this.NonWorkingDayFridayCheckBox, "IsFridayNonWorkingDay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCountry)(null)).IsFridayNonWorkingDay)));
			this.NonWorkingDayFridayCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryForm|NonWorkingDays|Friday", "Fri");
			this.NonWorkingDayFridayCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(212, 25, true);
			this.NonWorkingDayFridayCheckBox.Name = "NonWorkingDayFridayCheckBox";
			this.NonWorkingDayFridayCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 24, true);
			this.NonWorkingDayFridayCheckBox.TabIndex = 5;
			// 
			// NonWorkingDayTuesdayCheckBox
			// 
			this.BindingSource.SetBindingMember(this.NonWorkingDayTuesdayCheckBox, "IsTuesdayNonWorkingDay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCountry)(null)).IsTuesdayNonWorkingDay)));
			this.NonWorkingDayTuesdayCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryForm|NonWorkingDays|Tuesday", "Tue");
			this.NonWorkingDayTuesdayCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 25, true);
			this.NonWorkingDayTuesdayCheckBox.Name = "NonWorkingDayTuesdayCheckBox";
			this.NonWorkingDayTuesdayCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 24, true);
			this.NonWorkingDayTuesdayCheckBox.TabIndex = 2;
			// 
			// NonWorkingDayThursdayCheckBox
			// 
			this.BindingSource.SetBindingMember(this.NonWorkingDayThursdayCheckBox, "IsThursdayNonWorkingDay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCountry)(null)).IsThursdayNonWorkingDay)));
			this.NonWorkingDayThursdayCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryForm|NonWorkingDays|Thursday", "Thu");
			this.NonWorkingDayThursdayCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 25, true);
			this.NonWorkingDayThursdayCheckBox.Name = "NonWorkingDayThursdayCheckBox";
			this.NonWorkingDayThursdayCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 24, true);
			this.NonWorkingDayThursdayCheckBox.TabIndex = 4;
			// 
			// HolidaysGridGroupBox
			// 
			this.HolidaysGridGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right) | System.Windows.Forms.AnchorStyles.Bottom));
			this.HolidaysGridGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryForm|HolidaysGrid", "Holidays");
			this.HolidaysGridGroupBox.Controls.Add(this.ZGridNonWorkingDayHolidays);
			this.HolidaysGridGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 70, true);
			this.HolidaysGridGroupBox.Name = "HolidaysGridGroupBox";
			this.HolidaysGridGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(621, 280, true);
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountry)(null)).Holidays)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbHoliday)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountry)(null)).Holidays)).SyncRoot)).GH_HolidayName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.GlbHoliday)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountry)(null)).Holidays)).SyncRoot)).GH_Date)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.GlbHoliday)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountry)(null)).Holidays)).SyncRoot)).GH_Recurring)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.GlbHoliday)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountry)(null)).Holidays)).SyncRoot)).GH_IsWorkingDay)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbHoliday)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCountry)(null)).Holidays)).SyncRoot)).CountryStatesApplicability)));
			this.ZGridNonWorkingDayHolidays.CaptionVisible = false;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountry|NonWorkingDays|Description", "Description");
			zTextBoxColumnStyleInfo4.ColumnName = "GH_HolidayName";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountry|NonWorkingDays|Date", "Date");
			zDateEditColumnStyleInfo1.ColumnName = "GH_Date";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo9.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountry|NonWorkingDays|Recurring", "Recurring");
			zCheckBoxColumnStyleInfo9.ColumnName = "GH_Recurring";
			zCheckBoxColumnStyleInfo9.IsReadOnly = true;
			zCheckBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo10.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountry|NonWorkingDays|IsWorkingDay", "Working Day");
			zCheckBoxColumnStyleInfo10.ColumnName = "GH_IsWorkingDay";
			zCheckBoxColumnStyleInfo10.IsReadOnly = true;
			zCheckBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountry|NonWorkingDays|Applicability", "Applicability");
			zTextBoxColumnStyleInfo5.ColumnName = "CountryStatesApplicability";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.ZGridNonWorkingDayHolidays.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ZGridNonWorkingDayHolidays.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ZGridNonWorkingDayHolidays.ColumnStyles.Add(zCheckBoxColumnStyleInfo9);
			this.ZGridNonWorkingDayHolidays.ColumnStyles.Add(zCheckBoxColumnStyleInfo10);
			this.ZGridNonWorkingDayHolidays.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ZGridNonWorkingDayHolidays.GridId = "3a123977-f5f5-4578-be7c-b3d48573184e";
			this.ZGridNonWorkingDayHolidays.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ZGridNonWorkingDayHolidays.LayoutKey = "ZGridNonWorkingDayHolidays";
			this.ZGridNonWorkingDayHolidays.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.ZGridNonWorkingDayHolidays.Name = "ZGridNonWorkingDayHolidays";
			this.ZGridNonWorkingDayHolidays.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(607, 260, true);
			this.ZGridNonWorkingDayHolidays.TabIndex = 9;
			this.ZGridNonWorkingDayHolidays.DoubleClick += new System.EventHandler(this.ZGridNonWorkingDayHolidays_DoubleClick);
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(637, 448, true);
			this.zStmNoteTabPage1.TabIndex = 1;
			// 
			// zLogsTabPage1
			// 
			this.zLogsTabPage1.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.zLogsTabPage1.Name = "zLogsTabPage1";
			this.zLogsTabPage1.ShouldBeReadOnlyInViewMode = false;
			this.zLogsTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(637, 448, true);
			this.zLogsTabPage1.TabIndex = 2;
			// 
			// zComplianceRulesTabPage
			// 
			this.zComplianceRulesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.zComplianceRulesTabPage.Name = "zComplianceRulesTabPage";
			this.zComplianceRulesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(637, 448, true);
			this.zComplianceRulesTabPage.TabIndex = 6;
			this.zComplianceRulesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("814D6B76-E044-4D70-A7BE-5A6040B6270D", "Compliance Rules");
			// 
			// RefCountryForm
			//
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCountryForm|ef70ed1b-fe6b-4b1d-8913-074b4ac59026", "Country/Region");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(649, 532, true);
			this.Controls.Add(this.MainTabControl);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefCountry);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(612, 468, true);
			this.Name = "RefCountryForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.AddressValidationRuleDropEdit.ResumeLayout(true);
			this.AddressValidationRuleDropEdit.PerformLayout();
			this.AWBCurrencyCodeBoundGuidFindBox.ResumeLayout(true);
			this.AWBCurrencyCodeBoundGuidFindBox.PerformLayout();
			this.RN_PostcodeValidationRuleDropEdit.ResumeLayout(true);
			this.RN_PostcodeValidationRuleDropEdit.PerformLayout();
			this.RN_StateProvinceValidationRuleDropEdit.ResumeLayout(true);
			this.RN_StateProvinceValidationRuleDropEdit.PerformLayout();
			this.RN_AddressFormattingRuleDropEdit.ResumeLayout(true);
			this.RN_AddressFormattingRuleDropEdit.PerformLayout();
			this.RN_EconomicGroupingDropEdit.ResumeLayout(true);
			this.RN_EconomicGroupingDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ZGridStates)).EndInit();
			this.ZGridStates.ResumeLayout(false);
			this.ZGridStates.PerformLayout();
			this.RN_RX_NKLocalCurrencyBoundGuidFindBox.ResumeLayout(true);
			this.RN_RX_NKLocalCurrencyBoundGuidFindBox.PerformLayout();
			this.NameTextBox.ResumeLayout(true);
			this.NameTextBox.PerformLayout();
			this.zRulesTabPage.ResumeLayout(false);
			this.zRulesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RulesGrid)).EndInit();
			this.RulesGrid.ResumeLayout(false);
			this.RulesGrid.PerformLayout();
			this.zRequiredDocuments.ResumeLayout(false);
			this.zRequiredDocuments.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RequiredDocumentsGrid)).EndInit();
			this.RequiredDocumentsGrid.ResumeLayout(false);
			this.RequiredDocumentsGrid.PerformLayout();
			this.zNonWorkingDaysTabPage.ResumeLayout(false);
			this.zNonWorkingDaysTabPage.PerformLayout();
			this.WeekendsGroupBox.ResumeLayout(false);
			this.WeekendsGroupBox.PerformLayout();
			this.HolidaysGridGroupBox.ResumeLayout(false);
			this.HolidaysGridGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ZGridNonWorkingDayHolidays)).EndInit();
			this.ZGridNonWorkingDayHolidays.ResumeLayout(false);
			this.ZGridNonWorkingDayHolidays.PerformLayout();
			this.zStmNoteTabPage1.ResumeLayout(false);
			this.zStmNoteTabPage1.PerformLayout();
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

		#endregion

		private Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private Enterprise.ZArchitecture.ZTextBox ISOTextBox;
		private Enterprise.ZArchitecture.ZTranslatableTextControl NameTextBox;
		private Enterprise.ZArchitecture.ZLabel StatesLabel;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox RN_RX_NKLocalCurrencyBoundGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl MainTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage MainTabPage;
		private Enterprise.ZArchitecture.GUI.ZStmNoteTabPage zStmNoteTabPage1;
		private Enterprise.ZArchitecture.GUI.ZLogsTabPage zLogsTabPage1;
		private Enterprise.ZArchitecture.GUI.ZDropEdit RN_EconomicGroupingDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit RN_PostcodeValidationRuleDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit RN_StateProvinceValidationRuleDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit RN_AddressFormattingRuleDropEdit;
		private Enterprise.ZArchitecture.GUI.ZTabPage zRulesTabPage;
		private Enterprise.ZArchitecture.ZTextBox JL_OutturnCommentTextBox;
		private Enterprise.ZArchitecture.GUI.ZTabPage zRequiredDocuments;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox AWBCurrencyCodeBoundGuidFindBox;
		private Enterprise.ZArchitecture.ZTextBox zTextBox2;
		private Enterprise.ZArchitecture.ZTextBox zTextBox1;
		private Enterprise.ZArchitecture.ZGrid ZGridStates;
		private Enterprise.ZArchitecture.ZGrid RulesGrid;
		private Enterprise.ZArchitecture.ZGrid RequiredDocumentsGrid;
		private Enterprise.ZArchitecture.GUI.ZDropEdit AddressValidationRuleDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox isActiveCheckbox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox isSystemCheckbox;
		private Enterprise.ZArchitecture.ZLabel zLabel6;
		private Enterprise.ZArchitecture.GUI.ZCheckBox isMarkSanctionedCheckBox;
		private Enterprise.ZArchitecture.GUI.ZTabPage zNonWorkingDaysTabPage;
		private Enterprise.ZArchitecture.GUI.ZCheckBox NonWorkingDayMondayCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox NonWorkingDayTuesdayCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox NonWorkingDayWednesdayCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox NonWorkingDayThursdayCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox NonWorkingDayFridayCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox NonWorkingDaySaturdayCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox NonWorkingDaySundayCheckBox;
		private Enterprise.ZArchitecture.ZGrid ZGridNonWorkingDayHolidays;
		private ZArchitecture.GUI.ZGroupBox WeekendsGroupBox;
		private ZArchitecture.GUI.ZGroupBox HolidaysGridGroupBox;
		private ZArchitecture.GUI.ZTabPage zComplianceRulesTabPage;
	}
}
