using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class UNDGSubstanceIATAControl
	{
		protected Enterprise.ZArchitecture.GUI.ZTemplateTabControl UNDGSubstanceTabControl;
		Enterprise.ZArchitecture.ZTextBox DG_ProperShippingNameTextBox;
		Enterprise.ZArchitecture.ZTextBox DG_VariantTextBox;
		Enterprise.ZArchitecture.ZTextBox DG_UNNumberTextBox;
		protected Enterprise.ZArchitecture.GUI.ZTabPage CrossReferencesTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTabPage QualifyingDescriptiveTextTabPage;
		ZCheckBox IsActiveCheckBox;
		ZCheckBox DG_IsSystemCheckBox;
		private ZTabPage NamesTabPage;
		private ZTextBox DG_SpecialHandlingCodesTextBox1;
		private ZGroupBox LimitedQuantityGroupBox;
		private ZTextBox DG_LQMaxAmtUQTextBox;
		private ZCalcEdit DG_LQMaxAmtCalcEdit;
		private ZDropEdit DG_ClassDropDownEdit;
		private ZTextBox DG_SubLabel1TextBox;
		private ZTextBox DG_SubLabel2TextBox;
		private ZTextBox SpecialProvisionsTextBox;
		private ZGrid NamesGrid;
		private ZGrid CrossReferencesGrid;
		private ZGrid QualifyingDescriptiveTextGrid;
		private ZDropEdit DG_PGDownEdit;
		private ZDropEdit DG_ExceptedQuantitiesDropDown;
		private ZTextBox DG_HazardsTextBox;
		private ZCheckBox DG_NOSCheckBox;
		private ZTextBox DG_SpecialHandlingCodesTextBox2;
		private ZTextBox DG_SpecialHandlingCodesTextBox3;
		private ZDropEdit DG_LQMaxAmtTypeDropDown;
		private ZGroupBox PassengerAndCargoGroupBox;
		private ZDropEdit DG_LQ2OrPaxMaxAmtTypeDropDown;
		private ZTextBox DG_LQ2OrPaxMaxAmtUQTextBox1;
		private ZTextBox DG_LQ2OrPaxMaxAmtUQTextBox2;
		private ZCalcEdit DG_LQ2OrPaxMaxAmtCalcEdit1;
		private ZCalcEdit DG_LQ2OrPaxMaxAmtCalcEdit2;
		private ZGroupBox CargoOnlyGroupBox;
		private ZDropEdit DG_CargoPackAmtTypeDropDown;
		private ZTextBox DG_CargoMaxAmtUQTextBox1;
		private ZTextBox DG_CargoMaxAmtUQTextBox2;
		private ZCalcEdit DG_CargoMaxAmtCalcEdit1;
		private ZCalcEdit DG_CargoMaxAmtCalcEdit2;
		private ZTextBox DG_PackInsTextBox;
		private ZTextBox DG_PaxPackInsTextBox1;
		private ZTextBox DG_PaxPackInsSecTextBox1;
		private ZTextBox DG_PaxPackInsTextBox2;
		private ZTextBox DG_PaxPackInsSecTextBox2;
		private ZTextBox DG_CargoPackInsTextBox1;
		private ZTextBox DG_CargoPackInsSecTextBox1;
		private ZTextBox DG_CargoPackInsTextBox2;
		private ZTextBox DG_CargoPackInsSecTextBox2;
		private ZDropEdit DetailsLanguageDropEdit;
		private ZDropEdit DG_EmergencyResponseGuideDropDown;
		private ZLabel HeaderLabel;
		private KPictureBox IconPictureBox;
		private ZDropEdit DG_TechNameDropDownEdit;
		private ZPanel DG_BorderPanel;
		Enterprise.ZArchitecture.GUI.ZTabPage CountryReferencesTabPage;
		ZModuleButtonGrid CountryReferencesModuleButtonGrid;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo2 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			this.DG_SpecialHandlingCodesTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.LimitedQuantityGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DG_PackInsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_LQMaxAmtTypeDropDown = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DG_LQMaxAmtUQTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_LQMaxAmtCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DG_SubLabel1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_SubLabel2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SpecialProvisionsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_IsSystemCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DG_ProperShippingNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_ClassDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DG_VariantTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_UNNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UNDGSubstanceTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.NamesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.NamesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CrossReferencesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CrossReferencesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.QualifyingDescriptiveTextTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.QualifyingDescriptiveTextGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DG_PGDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DG_ExceptedQuantitiesDropDown = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DG_HazardsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_NOSCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DG_EmergencyResponseGuideDropDown = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DG_SpecialHandlingCodesTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_SpecialHandlingCodesTextBox3 = new Enterprise.ZArchitecture.ZTextBox();
			this.PassengerAndCargoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DG_PaxPackInsTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_PaxPackInsSecTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_PaxPackInsTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_PaxPackInsSecTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_LQ2OrPaxMaxAmtTypeDropDown = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DG_LQ2OrPaxMaxAmtUQTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_LQ2OrPaxMaxAmtUQTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_LQ2OrPaxMaxAmtCalcEdit1 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DG_LQ2OrPaxMaxAmtCalcEdit2 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CargoOnlyGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DG_CargoPackInsTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_CargoPackInsSecTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_CargoPackInsTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_CargoPackInsSecTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_CargoPackAmtTypeDropDown = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DG_CargoMaxAmtUQTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_CargoMaxAmtUQTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_CargoMaxAmtCalcEdit1 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DG_CargoMaxAmtCalcEdit2 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DetailsLanguageDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.HeaderLabel = new Enterprise.ZArchitecture.ZLabel();
			this.IconPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.DG_TechNameDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CountryReferencesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CountryReferencesModuleButtonGrid = new CountryReferencesModuleButtonGrid();
			this.DG_BorderPanel = new ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.IconPictureBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LimitedQuantityGroupBox.SuspendLayout();
			this.DG_LQMaxAmtTypeDropDown.SuspendLayout();
			this.DG_ClassDropDownEdit.SuspendLayout();
			this.UNDGSubstanceTabControl.SuspendLayout();
			this.NamesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.NamesGrid)).BeginInit();
			this.NamesGrid.SuspendLayout();
			this.CrossReferencesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CrossReferencesGrid)).BeginInit();
			this.CrossReferencesGrid.SuspendLayout();
			this.QualifyingDescriptiveTextTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.QualifyingDescriptiveTextGrid)).BeginInit();
			this.QualifyingDescriptiveTextGrid.SuspendLayout();
			this.DG_PGDownEdit.SuspendLayout();
			this.DG_ExceptedQuantitiesDropDown.SuspendLayout();
			this.DG_EmergencyResponseGuideDropDown.SuspendLayout();
			this.PassengerAndCargoGroupBox.SuspendLayout();
			this.DG_LQ2OrPaxMaxAmtTypeDropDown.SuspendLayout();
			this.CargoOnlyGroupBox.SuspendLayout();
			this.DG_CargoPackAmtTypeDropDown.SuspendLayout();
			this.DetailsLanguageDropEdit.SuspendLayout();
			this.CountryReferencesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CountryReferencesModuleButtonGrid.InnerGrid)).BeginInit();
			this.CountryReferencesModuleButtonGrid.SuspendLayout();
			this.DG_BorderPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.UNDGSubstance);
			// 
			// DG_SpecialHandlingCodesTextBox1
			// 
			this.BindingSource.SetBindingMember(this.DG_SpecialHandlingCodesTextBox1, "DG_SpecialHandlingCode1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_SpecialHandlingCode1)));
			this.DG_SpecialHandlingCodesTextBox1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceIATAForm|FA8AF0A5-2FCA-475B-9035-8769D40C4CE9", "Special Handling Code(s)", "The Special Handling Code(s) applicable to this substance as per CargoIMP Messaging Specification.");
			this.DG_SpecialHandlingCodesTextBox1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DG_SpecialHandlingCodesTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(493, 182, true);
			this.DG_SpecialHandlingCodesTextBox1.Name = "DG_SpecialHandlingCodesTextBox1";
			this.DG_SpecialHandlingCodesTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.DG_SpecialHandlingCodesTextBox1.TabIndex = 12;
			// 
			// LimitedQuantityGroupBox
			// 
			this.LimitedQuantityGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceIATAForm|EC1CB427-3B77-4E52-B5D9-27B9DB194556", "Limited Quantity");
			this.LimitedQuantityGroupBox.Controls.Add(this.DG_PackInsTextBox);
			this.LimitedQuantityGroupBox.Controls.Add(this.DG_LQMaxAmtTypeDropDown);
			this.LimitedQuantityGroupBox.Controls.Add(this.DG_LQMaxAmtUQTextBox);
			this.LimitedQuantityGroupBox.Controls.Add(this.DG_LQMaxAmtCalcEdit);
			this.LimitedQuantityGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 250, true);
			this.LimitedQuantityGroupBox.Name = "LimitedQuantityGroupBox";
			this.LimitedQuantityGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(298, 104, true);
			this.LimitedQuantityGroupBox.TabIndex = 17;
			this.LimitedQuantityGroupBox.TabStop = false;
			// 
			// DG_BorderPanel
			//
			this.DG_BorderPanel.Name = "DG_BorderPanel";
			this.DG_BorderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(910, 1, true);
			this.DG_BorderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 40, true);
			this.DG_BorderPanel.BackColor = Color.DarkGray;
			// 
			// DG_PackInsTextBox
			// 
			this.BindingSource.SetBindingMember(this.DG_PackInsTextBox, "DG_PackIns");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_PackIns)));
			this.DG_PackInsTextBox.CaptionResourceString = null;
			this.DG_PackInsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DG_PackInsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 71, true);
			this.DG_PackInsTextBox.Name = "DG_PackInsTextBox";
			this.DG_PackInsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.DG_PackInsTextBox.TabIndex = 18;
			// 
			// DG_LQMaxAmtTypeDropDown
			// 
			this.DG_LQMaxAmtTypeDropDown.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DG_LQMaxAmtTypeDropDown, "DG_LQMaxAmtType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_LQMaxAmtType)));
			this.DG_LQMaxAmtTypeDropDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 19, true);
			this.DG_LQMaxAmtTypeDropDown.Name = "DG_LQMaxAmtTypeDropDown";
			this.DG_LQMaxAmtTypeDropDown.PreBoundMaxLength = 4;
			this.DG_LQMaxAmtTypeDropDown.ShouldResizeByMaxLength = true;
			this.DG_LQMaxAmtTypeDropDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.DG_LQMaxAmtTypeDropDown.TabIndex = 15;
			// 
			// DG_LQMaxAmtUQTextBox
			// 
			this.BindingSource.SetBindingMember(this.DG_LQMaxAmtUQTextBox, "DG_LQMaxAmtUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_LQMaxAmtUQ)));
			this.DG_LQMaxAmtUQTextBox.CaptionResourceString = null;
			this.DG_LQMaxAmtUQTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DG_LQMaxAmtUQTextBox, false);
			this.DG_LQMaxAmtUQTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(252, 45, true);
			this.DG_LQMaxAmtUQTextBox.Name = "DG_LQMaxAmtUQTextBox";
			this.DG_LQMaxAmtUQTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.DG_LQMaxAmtUQTextBox.TabIndex = 17;
			// 
			// DG_LQMaxAmtCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DG_LQMaxAmtCalcEdit, "DG_LQMaxAmt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_LQMaxAmt)));
			this.DG_LQMaxAmtCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("D7A5E498-3817-44C5-9A8A-B09E59259271", "Pack Max. Qty", "The maximum quantity per pack that this substance may be transported in limited quantities.");
			this.DG_LQMaxAmtCalcEdit.DecimalPlaces = 2;
			this.DG_LQMaxAmtCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 45, true);
			this.DG_LQMaxAmtCalcEdit.Name = "DG_LQMaxAmtCalcEdit";
			this.DG_LQMaxAmtCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			this.DG_LQMaxAmtCalcEdit.TabIndex = 16;
			this.DG_LQMaxAmtCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DG_SubLabel1TextBox
			// 
			this.BindingSource.SetBindingMember(this.DG_SubLabel1TextBox, "DG_SubLabel1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_SubLabel1)));
			this.DG_SubLabel1TextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DE1A5F0E-775A-4450-B593-7B7396D29006", "Sub Risk 1", "The subsidiary hazard risk 1.");
			this.DG_SubLabel1TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DG_SubLabel1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(182, 182, true);
			this.DG_SubLabel1TextBox.Name = "DG_SubLabel1TextBox";
			this.DG_SubLabel1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.DG_SubLabel1TextBox.TabIndex = 8;
			// 
			// DG_SubLabel2TextBox
			// 
			this.BindingSource.SetBindingMember(this.DG_SubLabel2TextBox, "DG_SubLabel2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_SubLabel2)));
			this.DG_SubLabel2TextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8FECD8A9-9075-442B-B662-036F519A98B2", "Sub Risk 2", "The subsidiary hazard risk 2.");
			this.DG_SubLabel2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DG_SubLabel2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(182, 208, true);
			this.DG_SubLabel2TextBox.Name = "DG_SubLabel2TextBox";
			this.DG_SubLabel2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.DG_SubLabel2TextBox.TabIndex = 9;
			// 
			// SpecialProvisionsTextBox
			// 
			this.BindingSource.SetBindingMember(this.SpecialProvisionsTextBox, "SpecialProvisionDescriptor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).SpecialProvisionDescriptor)));
			this.SpecialProvisionsTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0A47A2D6-7EA7-4F93-8B42-3EA85566B72D", "Special Provisions", "The Special Provision applied to this substance.");
			this.SpecialProvisionsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SpecialProvisionsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(493, 208, true);
			this.SpecialProvisionsTextBox.Name = "DG_SpecialProvisionTextBox";
			this.SpecialProvisionsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(189, 20, true);
			this.SpecialProvisionsTextBox.ReadOnly = true;
			this.SpecialProvisionsTextBox.TabIndex = 38;
			// 
			// DG_IsSystemCheckBox
			// 
			this.DG_IsSystemCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.DG_IsSystemCheckBox, "DG_IsSystem");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_IsSystem)));
			this.DG_IsSystemCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.DG_IsSystemCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DG_IsSystemCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(816, 55, true);
			this.DG_IsSystemCheckBox.Name = "DG_IsSystemCheckBox";
			this.DG_IsSystemCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.DG_IsSystemCheckBox.TabIndex = 26;
			// 
			// IsActiveCheckBox
			// 
			this.IsActiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsActiveCheckBox, "DG_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_IsActive)));
			this.IsActiveCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(955, 55, true);
			this.IsActiveCheckBox.Name = "IsActiveCheckBox";
			this.IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsActiveCheckBox.TabIndex = 27;
			// 
			// DG_ProperShippingNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.DG_ProperShippingNameTextBox, "DG_PSN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_PSN)));
			this.DG_ProperShippingNameTextBox.CaptionResourceString = null;
			this.DG_ProperShippingNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DG_ProperShippingNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(182, 78, true);
			this.DG_ProperShippingNameTextBox.Name = "DG_ProperShippingNameTextBox";
			this.DG_ProperShippingNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(788, 20, true);
			this.DG_ProperShippingNameTextBox.TabIndex = 4;
			// 
			// DG_ClassDropDownEdit
			// 
			this.DG_ClassDropDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DG_ClassDropDownEdit, "DG_Class");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_Class)));
			this.DG_ClassDropDownEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("495D4F19-A823-4D18-947F-FB8909FB4A8F", "Class", "Class/Division", "Class/Division", "The class or division number (and compatibility group in case of Class 1 Explosives).");
			this.DG_ClassDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(329, 52, true);
			this.DG_ClassDropDownEdit.Name = "DG_ClassDropDownEdit";
			this.DG_ClassDropDownEdit.PreBoundMaxLength = 4;
			this.DG_ClassDropDownEdit.ShouldResizeByMaxLength = true;
			this.DG_ClassDropDownEdit.ShowDescriptionBox = false;
			this.DG_ClassDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.DG_ClassDropDownEdit.TabIndex = 2;
			// 
			// DG_VariantTextBox
			// 
			this.BindingSource.SetBindingMember(this.DG_VariantTextBox, "DG_Variant");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_Variant)));
			this.DG_VariantTextBox.CaptionResourceString = null;
			this.DG_VariantTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(214, 52, true);
			this.DG_VariantTextBox.Name = "DG_VariantTextBox";
			this.DG_VariantTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 20, true);
			this.DG_VariantTextBox.TabIndex = 1;
			// 
			// DG_UNNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.DG_UNNumberTextBox, "DG_UNNO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_UNNO)));
			this.DG_UNNumberTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7C89223F-12CD-4335-8B6D-5EDADA320BDC", "{0} No.", "{0} Number", "{0} Number", "The United Nations or Identification number.").Format(this.GetUNNOPrefix());
			this.DG_UNNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(182, 52, true);
			this.DG_UNNumberTextBox.Name = "DG_UNNumberTextBox";
			this.DG_UNNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.DG_UNNumberTextBox.TabIndex = 0;
			// 
			// UNDGSubstanceTabControl
			// 
			this.UNDGSubstanceTabControl.Controls.Add(this.NamesTabPage);
			this.UNDGSubstanceTabControl.Controls.Add(this.CrossReferencesTabPage);
			this.UNDGSubstanceTabControl.Controls.Add(this.QualifyingDescriptiveTextTabPage);
			this.UNDGSubstanceTabControl.Controls.Add(this.CountryReferencesTabPage);
			this.UNDGSubstanceTabControl.Name = "UNDGSubstanceTabControl";
			this.UNDGSubstanceTabControl.SelectedIndex = 0;
			this.UNDGSubstanceTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(948, 127, true);
			this.UNDGSubstanceTabControl.TabIndex = 28;
			// 
			// NamesTabPage
			// 
			this.NamesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceIATAForm|1C1150D1-59D6-48A0-8BC6-B5ECF141D3DD", "Names");
			this.NamesTabPage.Controls.Add(this.NamesGrid);
			this.NamesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.NamesTabPage.Name = "NamesTabPage";
			this.NamesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 166, true);
			this.NamesTabPage.TabIndex = 7;
			this.NamesTabPage.UseVisualStyleBackColor = true;
			// 
			// NamesGrid
			// 
			this.NamesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.NamesGrid, "Names");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).Names)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ViewUNDGAttribute)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).Names)).SyncRoot)).DA_Language)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ViewUNDGAttribute)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).Names)).SyncRoot)).DA_Descriptor)));
			this.NamesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "DA_Language";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.ColumnName = "DA_Descriptor";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(800);
			this.NamesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.NamesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.NamesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NamesGrid.GridId = "9096590a-e745-4fc8-83bd-00218d17db22";
			this.NamesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.NamesGrid.LayoutKey = "zGrid3";
			this.NamesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NamesGrid.Name = "NamesGrid";
			this.NamesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 250, true);
			this.NamesGrid.TabIndex = 1;
			// 
			// CrossReferencesTabPage
			// 
			this.CrossReferencesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceIATAForm|62BC819E-305B-4EA1-AA6F-824ECB7E9473", "Cross References", "Other names by which the substance may be known. For example, when only the UN number is known, this cross reference provides a link to the proper shipping name.");
			this.CrossReferencesTabPage.Controls.Add(this.CrossReferencesGrid);
			this.CrossReferencesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CrossReferencesTabPage.Name = "CrossReferencesTabPage";
			this.CrossReferencesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 166, true);
			this.CrossReferencesTabPage.TabIndex = 1;
			// 
			// CrossReferencesGrid
			// 
			this.CrossReferencesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CrossReferencesGrid, "CrossReferences");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).CrossReferences)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ViewUNDGAttribute)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).CrossReferences)).SyncRoot)).DA_Language)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ViewUNDGAttribute)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).CrossReferences)).SyncRoot)).DA_Descriptor)));
			this.CrossReferencesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo2.ColumnName = "DA_Language";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zMultiLineTextBoxColumnInfo1.ColumnName = "DA_Descriptor";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(800);
			this.CrossReferencesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.CrossReferencesGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.CrossReferencesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CrossReferencesGrid.GridId = "cf8defc9-9faa-4e3d-bd91-770f5f6be77c";
			this.CrossReferencesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CrossReferencesGrid.LayoutKey = "zGrid3";
			this.CrossReferencesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CrossReferencesGrid.Name = "CrossReferencesGrid";
			this.CrossReferencesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 250, true);
			this.CrossReferencesGrid.TabIndex = 0;
			// 
			// QualifyingDescriptiveTextTabPage
			// 
			this.QualifyingDescriptiveTextTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceIATAForm|BD21CECB-A66C-4519-9232-9F36DB1A944D", "Qualifying Descriptive Text", "Qualifying descriptive text of the Proper Shipping Name. This text is not part of the Proper Shipping Name but may be used in addition to it (i.e. for explosives of Class 1 to indicate commercial or military names).");
			this.QualifyingDescriptiveTextTabPage.Controls.Add(this.QualifyingDescriptiveTextGrid);
			this.QualifyingDescriptiveTextTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.QualifyingDescriptiveTextTabPage.Name = "QualifyingDescriptiveTextTabPage";
			this.QualifyingDescriptiveTextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 166, true);
			this.QualifyingDescriptiveTextTabPage.TabIndex = 2;
			// 
			// QualifyingDescriptiveTextGrid
			// 
			this.QualifyingDescriptiveTextGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.QualifyingDescriptiveTextGrid, "QualifyingDescriptiveTexts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).QualifyingDescriptiveTexts)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ViewUNDGAttribute)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).QualifyingDescriptiveTexts)).SyncRoot)).DA_Language)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ViewUNDGAttribute)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).QualifyingDescriptiveTexts)).SyncRoot)).DA_Descriptor)));
			this.QualifyingDescriptiveTextGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo3.ColumnName = "DA_Language";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zMultiLineTextBoxColumnInfo2.ColumnName = "DA_Descriptor";
			zMultiLineTextBoxColumnInfo2.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(800);
			this.QualifyingDescriptiveTextGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.QualifyingDescriptiveTextGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo2);
			this.QualifyingDescriptiveTextGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.QualifyingDescriptiveTextGrid.GridId = "b6e1bb48-1149-4a1e-919a-ad6d8b9edeb8";
			this.QualifyingDescriptiveTextGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.QualifyingDescriptiveTextGrid.LayoutKey = "zGrid2";
			this.QualifyingDescriptiveTextGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.QualifyingDescriptiveTextGrid.Name = "QualifyingDescriptiveTextGrid";
			this.QualifyingDescriptiveTextGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 250, true);
			this.QualifyingDescriptiveTextGrid.TabIndex = 0;
			// 
			// DG_PGDownEdit
			// 
			this.DG_PGDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DG_PGDownEdit, "DG_PG");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_PG)));
			this.DG_PGDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(493, 52, true);
			this.DG_PGDownEdit.Name = "DG_PGDownEdit";
			this.DG_PGDownEdit.PreBoundMaxLength = 4;
			this.DG_PGDownEdit.ShouldResizeByMaxLength = true;
			this.DG_PGDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.DG_PGDownEdit.TabIndex = 3;
			// 
			// DG_ExceptedQuantitiesDropDown
			// 
			this.DG_ExceptedQuantitiesDropDown.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DG_ExceptedQuantitiesDropDown, "DG_ExceptedQuantityCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_ExceptedQuantityCode)));
			this.DG_ExceptedQuantitiesDropDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(493, 130, true);
			this.DG_ExceptedQuantitiesDropDown.Name = "DG_ExceptedQuantitiesDropDown";
			this.DG_ExceptedQuantitiesDropDown.PreBoundMaxLength = 4;
			this.DG_ExceptedQuantitiesDropDown.ShouldResizeByMaxLength = true;
			this.DG_ExceptedQuantitiesDropDown.ShowDescriptionBox = true;
			this.DG_ExceptedQuantitiesDropDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(477, 20, true);
			this.DG_ExceptedQuantitiesDropDown.TabIndex = 10;
			// 
			// DG_HazardsTextBox
			// 
			this.BindingSource.SetBindingMember(this.DG_HazardsTextBox, "DG_Hazards");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_Hazards)));
			this.DG_HazardsTextBox.CaptionResourceString = null;
			this.DG_HazardsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DG_HazardsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(182, 104, true);
			this.DG_HazardsTextBox.Name = "DG_HazardsTextBox";
			this.DG_HazardsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(788, 20, true);
			this.DG_HazardsTextBox.TabIndex = 5;
			// 
			// DG_NOSCheckBox
			// 
			this.DG_NOSCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.DG_NOSCheckBox, "DG_IsNotOtherwiseSpecified");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_IsNotOtherwiseSpecified)));
			this.DG_NOSCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.DG_NOSCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DG_NOSCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(182, 158, true);
			this.DG_NOSCheckBox.Name = "DG_NOSCheckBox";
			this.DG_NOSCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.DG_NOSCheckBox.TabIndex = 7;
			// 
			// DG_EmergencyResponseGuideDropDown
			// 
			this.DG_EmergencyResponseGuideDropDown.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DG_EmergencyResponseGuideDropDown, "DG_EmergencyResponseGuide");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_EmergencyResponseGuide)));
			this.DG_EmergencyResponseGuideDropDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(493, 156, true);
			this.DG_EmergencyResponseGuideDropDown.Name = "DG_EmergencyResponseGuideDropDown";
			this.DG_EmergencyResponseGuideDropDown.PreBoundMaxLength = 4;
			this.DG_EmergencyResponseGuideDropDown.ShouldResizeByMaxLength = true;
			this.DG_EmergencyResponseGuideDropDown.ShowDescriptionBox = false;
			this.DG_EmergencyResponseGuideDropDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.DG_EmergencyResponseGuideDropDown.TabIndex = 11;
			this.DG_EmergencyResponseGuideDropDown.CharacterCasing = CharacterCasing.Normal;
			// 
			// DG_SpecialHandlingCodesTextBox2
			// 
			this.BindingSource.SetBindingMember(this.DG_SpecialHandlingCodesTextBox2, "DG_SpecialHandlingCode2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_SpecialHandlingCode2)));
			this.DG_SpecialHandlingCodesTextBox2.CaptionResourceString = null;
			this.DG_SpecialHandlingCodesTextBox2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DG_SpecialHandlingCodesTextBox2, false);
			this.DG_SpecialHandlingCodesTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(556, 182, true);
			this.DG_SpecialHandlingCodesTextBox2.Name = "DG_SpecialHandlingCodesTextBox2";
			this.DG_SpecialHandlingCodesTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.DG_SpecialHandlingCodesTextBox2.TabIndex = 13;
			// 
			// DG_SpecialHandlingCodesTextBox3
			// 
			this.BindingSource.SetBindingMember(this.DG_SpecialHandlingCodesTextBox3, "DG_SpecialHandlingCode3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_SpecialHandlingCode3)));
			this.DG_SpecialHandlingCodesTextBox3.CaptionResourceString = null;
			this.DG_SpecialHandlingCodesTextBox3.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DG_SpecialHandlingCodesTextBox3, false);
			this.DG_SpecialHandlingCodesTextBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(619, 182, true);
			this.DG_SpecialHandlingCodesTextBox3.Name = "DG_SpecialHandlingCodesTextBox3";
			this.DG_SpecialHandlingCodesTextBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.DG_SpecialHandlingCodesTextBox3.TabIndex = 14;
			// 
			// PassengerAndCargoGroupBox
			// 
			this.PassengerAndCargoGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceIATAForm|BF0232FE-C9FD-4236-96F2-BDA6E2A1EA0E", "Passenger and Cargo Aircraft");
			this.PassengerAndCargoGroupBox.Controls.Add(this.DG_PaxPackInsTextBox1);
			this.PassengerAndCargoGroupBox.Controls.Add(this.DG_PaxPackInsSecTextBox1);
			this.PassengerAndCargoGroupBox.Controls.Add(this.DG_PaxPackInsTextBox2);
			this.PassengerAndCargoGroupBox.Controls.Add(this.DG_PaxPackInsSecTextBox2);
			this.PassengerAndCargoGroupBox.Controls.Add(this.DG_LQ2OrPaxMaxAmtTypeDropDown);
			this.PassengerAndCargoGroupBox.Controls.Add(this.DG_LQ2OrPaxMaxAmtUQTextBox1);
			this.PassengerAndCargoGroupBox.Controls.Add(this.DG_LQ2OrPaxMaxAmtUQTextBox2);
			this.PassengerAndCargoGroupBox.Controls.Add(this.DG_LQ2OrPaxMaxAmtCalcEdit1);
			this.PassengerAndCargoGroupBox.Controls.Add(this.DG_LQ2OrPaxMaxAmtCalcEdit2);
			this.PassengerAndCargoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 250, true);
			this.PassengerAndCargoGroupBox.Name = "PassengerAndCargoGroupBox";
			this.PassengerAndCargoGroupBox.TabIndex = 35;
			this.PassengerAndCargoGroupBox.TabStop = false;
			// 
			// DG_LQ2OrPaxMaxAmtTypeDropDown
			// 
			this.DG_LQ2OrPaxMaxAmtTypeDropDown.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DG_LQ2OrPaxMaxAmtTypeDropDown, "DG_LQ2OrPaxMaxAmtType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_LQ2OrPaxMaxAmtType)));
			this.DG_LQ2OrPaxMaxAmtTypeDropDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 19, true);
			this.DG_LQ2OrPaxMaxAmtTypeDropDown.Name = "DG_LQ2OrPaxMaxAmtTypeDropDown";
			this.DG_LQ2OrPaxMaxAmtTypeDropDown.PreBoundMaxLength = 4;
			this.DG_LQ2OrPaxMaxAmtTypeDropDown.ShouldResizeByMaxLength = true;
			this.DG_LQ2OrPaxMaxAmtTypeDropDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.DG_LQ2OrPaxMaxAmtTypeDropDown.TabIndex = 0;
			// 
			// DG_PaxPackInsTextBox1
			// 
			this.BindingSource.SetBindingMember(this.DG_PaxPackInsTextBox1, "DG_PaxPackIns");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_PaxPackIns)));
			this.DG_PaxPackInsTextBox1.CaptionResourceString = null;
			this.DG_PaxPackInsTextBox1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DG_PaxPackInsTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 45, true);
			this.DG_PaxPackInsTextBox1.Name = "DG_PaxPackInsTextBox1";
			this.DG_PaxPackInsTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			this.DG_PaxPackInsTextBox1.TabIndex = 1;
			// 
			// DG_PaxPackInsSecTextBox1
			// 
			this.BindingSource.SetBindingMember(this.DG_PaxPackInsSecTextBox1, "PaxPackInsSec1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).PaxPackInsSec1)));
			this.DG_PaxPackInsSecTextBox1.CaptionResourceString = null;
			this.DG_PaxPackInsSecTextBox1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DG_PaxPackInsSecTextBox1, false);
			this.DG_PaxPackInsSecTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(252, 45, true);
			this.DG_PaxPackInsSecTextBox1.Name = "DG_PaxPackInsSecTextBox1";
			this.DG_PaxPackInsSecTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.DG_PaxPackInsSecTextBox1.TabIndex = 5;
			// 
			// DG_LQ2OrPaxMaxAmtCalcEdit1
			// 
			this.BindingSource.SetBindingMember(this.DG_LQ2OrPaxMaxAmtCalcEdit1, "PaxMaxAmt1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).PaxMaxAmt1)));
			this.DG_LQ2OrPaxMaxAmtCalcEdit1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("E78E45AB-1452-4979-B1B6-1453881A47D5", "Pack Max. Qty", "The maximum quantity per pack that this substance may be transported on passenger and cargo aircraft.");
			this.DG_LQ2OrPaxMaxAmtCalcEdit1.DecimalPlaces = 2;
			this.DG_LQ2OrPaxMaxAmtCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 71, true);
			this.DG_LQ2OrPaxMaxAmtCalcEdit1.Name = "DG_LQ2OrPaxMaxAmtCalcEdit";
			this.DG_LQ2OrPaxMaxAmtCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			this.DG_LQ2OrPaxMaxAmtCalcEdit1.TabIndex = 2;
			this.DG_LQ2OrPaxMaxAmtCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DG_LQ2OrPaxMaxAmtUQTextBox1
			// 
			this.BindingSource.SetBindingMember(this.DG_LQ2OrPaxMaxAmtUQTextBox1, "PaxMaxAmtUQ1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).PaxMaxAmtUQ1)));
			this.DG_LQ2OrPaxMaxAmtUQTextBox1.CaptionResourceString = null;
			this.DG_LQ2OrPaxMaxAmtUQTextBox1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DG_LQ2OrPaxMaxAmtUQTextBox1, false);
			this.DG_LQ2OrPaxMaxAmtUQTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(252, 71, true);
			this.DG_LQ2OrPaxMaxAmtUQTextBox1.Name = "DG_LQ2OrPaxMaxAmtUQTextBox1";
			this.DG_LQ2OrPaxMaxAmtUQTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.DG_LQ2OrPaxMaxAmtUQTextBox1.TabIndex = 6;
			// 
			// DG_PaxPackInsTextBox2
			// 
			this.BindingSource.SetBindingMember(this.DG_PaxPackInsTextBox2, "DG_PaxPackIns");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_PaxPackIns)));
			this.DG_PaxPackInsTextBox2.CaptionResourceString = null;
			this.DG_PaxPackInsTextBox2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DG_PaxPackInsTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 97, true);
			this.DG_PaxPackInsTextBox2.Name = "DG_PaxPackInsTextBox2";
			this.DG_PaxPackInsTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			this.DG_PaxPackInsTextBox2.TabIndex = 3;
			// 
			// DG_PaxPackInsSecTextBox2
			// 
			this.BindingSource.SetBindingMember(this.DG_PaxPackInsSecTextBox2, "PaxPackInsSec2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).PaxPackInsSec2)));
			this.DG_PaxPackInsSecTextBox2.CaptionResourceString = null;
			this.DG_PaxPackInsSecTextBox2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DG_PaxPackInsSecTextBox2, false);
			this.DG_PaxPackInsSecTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(252, 97, true);
			this.DG_PaxPackInsSecTextBox2.Name = "DG_PaxPackInsSecTextBox2";
			this.DG_PaxPackInsSecTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.DG_PaxPackInsSecTextBox2.TabIndex = 7;
			// 
			// DG_LQ2OrPaxMaxAmtCalcEdit2
			// 
			this.BindingSource.SetBindingMember(this.DG_LQ2OrPaxMaxAmtCalcEdit2, "PaxMaxAmt2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).PaxMaxAmt2)));
			this.DG_LQ2OrPaxMaxAmtCalcEdit2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("E78E45AB-1452-4979-B1B6-1453881A47D5", "Pack Max. Qty", "The maximum quantity per pack that this substance may be transported on passenger and cargo aircraft.");
			this.DG_LQ2OrPaxMaxAmtCalcEdit2.DecimalPlaces = 2;
			this.DG_LQ2OrPaxMaxAmtCalcEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 118, true);
			this.DG_LQ2OrPaxMaxAmtCalcEdit2.Name = "DG_LQ2OrPaxMaxAmtCalcEdit2";
			this.DG_LQ2OrPaxMaxAmtCalcEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			this.DG_LQ2OrPaxMaxAmtCalcEdit2.TabIndex = 4;
			this.DG_LQ2OrPaxMaxAmtCalcEdit2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DG_LQ2OrPaxMaxAmtUQTextBox2
			// 
			this.BindingSource.SetBindingMember(this.DG_LQ2OrPaxMaxAmtUQTextBox2, "PaxMaxAmtUQ2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).PaxMaxAmtUQ2)));
			this.DG_LQ2OrPaxMaxAmtUQTextBox2.CaptionResourceString = null;
			this.DG_LQ2OrPaxMaxAmtUQTextBox2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DG_LQ2OrPaxMaxAmtUQTextBox2, false);
			this.DG_LQ2OrPaxMaxAmtUQTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(252, 118, true);
			this.DG_LQ2OrPaxMaxAmtUQTextBox2.Name = "DG_LQ2OrPaxMaxAmtUQTextBox2";
			this.DG_LQ2OrPaxMaxAmtUQTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.DG_LQ2OrPaxMaxAmtUQTextBox2.TabIndex = 8;
			// 
			// CargoOnlyGroupBox
			// 
			this.CargoOnlyGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceIATAForm|DD6CA3F8-F541-4B64-AA95-99542CD516C6", "Cargo Aircraft Only");
			this.CargoOnlyGroupBox.Controls.Add(this.DG_CargoPackInsTextBox1);
			this.CargoOnlyGroupBox.Controls.Add(this.DG_CargoPackInsTextBox2);
			this.CargoOnlyGroupBox.Controls.Add(this.DG_CargoPackInsSecTextBox1);
			this.CargoOnlyGroupBox.Controls.Add(this.DG_CargoPackInsSecTextBox2);
			this.CargoOnlyGroupBox.Controls.Add(this.DG_CargoPackAmtTypeDropDown);
			this.CargoOnlyGroupBox.Controls.Add(this.DG_CargoMaxAmtUQTextBox1);
			this.CargoOnlyGroupBox.Controls.Add(this.DG_CargoMaxAmtUQTextBox2);
			this.CargoOnlyGroupBox.Controls.Add(this.DG_CargoMaxAmtCalcEdit1);
			this.CargoOnlyGroupBox.Controls.Add(this.DG_CargoMaxAmtCalcEdit2);
			this.CargoOnlyGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(678, 250, true);
			this.CargoOnlyGroupBox.Name = "CargoOnlyGroupBox";
			this.CargoOnlyGroupBox.TabIndex = 36;
			this.CargoOnlyGroupBox.TabStop = false;
			// 
			// DG_CargoPackAmtTypeDropDown
			// 
			this.DG_CargoPackAmtTypeDropDown.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DG_CargoPackAmtTypeDropDown, "DG_CargoPackAmtType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_CargoPackAmtType)));
			this.DG_CargoPackAmtTypeDropDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 19, true);
			this.DG_CargoPackAmtTypeDropDown.Name = "DG_CargoPackAmtTypeDropDown";
			this.DG_CargoPackAmtTypeDropDown.PreBoundMaxLength = 4;
			this.DG_CargoPackAmtTypeDropDown.ShouldResizeByMaxLength = true;
			this.DG_CargoPackAmtTypeDropDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.DG_CargoPackAmtTypeDropDown.TabIndex = 0;
			// 
			// DG_CargoPackInsTextBox1
			// 
			this.BindingSource.SetBindingMember(this.DG_CargoPackInsTextBox1, "DG_CargoPackIns");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_CargoPackIns)));
			this.DG_CargoPackInsTextBox1.CaptionResourceString = null;
			this.DG_CargoPackInsTextBox1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DG_CargoPackInsTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 45, true);
			this.DG_CargoPackInsTextBox1.Name = "DG_CargoPackInsTextBox1";
			this.DG_CargoPackInsTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			this.DG_CargoPackInsTextBox1.TabIndex = 1;
			// 
			// DG_CargoPackInsSecTextBox1
			// 
			this.BindingSource.SetBindingMember(this.DG_CargoPackInsSecTextBox1, "CaoPackInsSec1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).CaoPackInsSec1)));
			this.DG_CargoPackInsSecTextBox1.CaptionResourceString = null;
			this.DG_CargoPackInsSecTextBox1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DG_CargoPackInsSecTextBox1, false);
			this.DG_CargoPackInsSecTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(252, 45, true);
			this.DG_CargoPackInsSecTextBox1.Name = "DG_CargoPackInsSecTextBox1";
			this.DG_CargoPackInsSecTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.DG_CargoPackInsSecTextBox1.TabIndex = 5;
			// 
			// DG_CargoPackInsTextBox2
			// 
			this.BindingSource.SetBindingMember(this.DG_CargoPackInsTextBox2, "DG_CargoPackIns");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_CargoPackIns)));
			this.DG_CargoPackInsTextBox2.CaptionResourceString = null;
			this.DG_CargoPackInsTextBox2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DG_CargoPackInsTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 97, true);
			this.DG_CargoPackInsTextBox2.Name = "DG_CargoPackInsTextBox2";
			this.DG_CargoPackInsTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			this.DG_CargoPackInsTextBox2.TabIndex = 2;
			// 
			// DG_CargoPackInsSecTextBox2
			// 
			this.BindingSource.SetBindingMember(this.DG_CargoPackInsSecTextBox2, "CaoPackInsSec2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).CaoPackInsSec2)));
			this.DG_CargoPackInsSecTextBox2.CaptionResourceString = null;
			this.DG_CargoPackInsSecTextBox2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DG_CargoPackInsSecTextBox2, false);
			this.DG_CargoPackInsSecTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(252, 97, true);
			this.DG_CargoPackInsSecTextBox2.Name = "DG_CargoPackInsSecTextBox2";
			this.DG_CargoPackInsSecTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.DG_CargoPackInsSecTextBox2.TabIndex = 6;
			// 
			// DG_CargoMaxAmtCalcEdit1
			// 
			this.BindingSource.SetBindingMember(this.DG_CargoMaxAmtCalcEdit1, "CaoMaxAmt1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).CaoMaxAmt1)));
			this.DG_CargoMaxAmtCalcEdit1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("52E21611-DEE7-4C13-BF77-710170CDABBB", "Pack Max. Qty", "The maximum quantity per pack that this substance may be transported on cargo aircraft only.");
			this.DG_CargoMaxAmtCalcEdit1.DecimalPlaces = 2;
			this.DG_CargoMaxAmtCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 71, true);
			this.DG_CargoMaxAmtCalcEdit1.Name = "DG_CargoMaxAmtCalcEdit";
			this.DG_CargoMaxAmtCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			this.DG_CargoMaxAmtCalcEdit1.TabIndex = 3;
			this.DG_CargoMaxAmtCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DG_CargoMaxAmtUQTextBox1
			// 
			this.BindingSource.SetBindingMember(this.DG_CargoMaxAmtUQTextBox1, "CaoMaxAmtUQ1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).CaoMaxAmtUQ1)));
			this.DG_CargoMaxAmtUQTextBox1.CaptionResourceString = null;
			this.DG_CargoMaxAmtUQTextBox1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DG_CargoMaxAmtUQTextBox1, false);
			this.DG_CargoMaxAmtUQTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(252, 71, true);
			this.DG_CargoMaxAmtUQTextBox1.Name = "DG_CargoMaxAmtUQTextBox1";
			this.DG_CargoMaxAmtUQTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.DG_CargoMaxAmtUQTextBox1.TabIndex = 7;
			// 
			// DG_CargoMaxAmtCalcEdit2
			// 
			this.BindingSource.SetBindingMember(this.DG_CargoMaxAmtCalcEdit2, "CaoMaxAmt2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).CaoMaxAmt2)));
			this.DG_CargoMaxAmtCalcEdit2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("52E21611-DEE7-4C13-BF77-710170CDABBB", "Pack Max. Qty", "The maximum quantity per pack that this substance may be transported on cargo aircraft only.");
			this.DG_CargoMaxAmtCalcEdit2.DecimalPlaces = 2;
			this.DG_CargoMaxAmtCalcEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 118, true);
			this.DG_CargoMaxAmtCalcEdit2.Name = "DG_CargoMaxAmtCalcEdit2";
			this.DG_CargoMaxAmtCalcEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			this.DG_CargoMaxAmtCalcEdit2.TabIndex = 4;
			this.DG_CargoMaxAmtCalcEdit2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DG_CargoMaxAmtUQTextBox2
			// 
			this.BindingSource.SetBindingMember(this.DG_CargoMaxAmtUQTextBox2, "CaoMaxAmtUQ2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).CaoMaxAmtUQ2)));
			this.DG_CargoMaxAmtUQTextBox2.CaptionResourceString = null;
			this.DG_CargoMaxAmtUQTextBox2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DG_CargoMaxAmtUQTextBox2, false);
			this.DG_CargoMaxAmtUQTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(252, 118, true);
			this.DG_CargoMaxAmtUQTextBox2.Name = "DG_CargoMaxAmtUQTextBox2";
			this.DG_CargoMaxAmtUQTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.DG_CargoMaxAmtUQTextBox2.TabIndex = 8;
			// 
			// DetailsLanguageDropEdit
			// 
			this.DetailsLanguageDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DetailsLanguageDropEdit, "DetailsLanguage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DetailsLanguage)));
			this.DetailsLanguageDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceForm|da962885-31ca-4af9-a6c2-c77d35d6302c", "Language Filter", "Filters the Names, Observations, Properties, Qualifying Descriptive Texts, Special Provisions and Stowage/Segmentation requirements to only show those that are in the language you choose.");
			this.DetailsLanguageDropEdit.Name = "DetailsLanguageDropEdit";
			this.DetailsLanguageDropEdit.PreBoundMaxLength = 4;
			this.DetailsLanguageDropEdit.ShouldResizeByMaxLength = true;
			this.DetailsLanguageDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 20, true);
			this.DetailsLanguageDropEdit.TabIndex = 37;
			// 
			// HeaderLabel
			// 
			this.HeaderLabel.AutoSize = true;
			this.HeaderLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesIATAForm|9D8F4563-3991-4FF3-A3D8-4304F1CF9A88", "AIR FREIGHT");
			this.HeaderLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Header)));
			this.HeaderLabel.IsFontBold = true;
			this.HeaderLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 20, true);
			this.HeaderLabel.Name = "HeaderLabel";
			this.HeaderLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 30, true);
			this.HeaderLabel.TabIndex = 39;
			// 
			// IconPictureBox
			// 
			this.IconPictureBox.BackColor = System.Drawing.Color.Transparent;
			this.IconPictureBox.Image = Properties.Resources.airfreight;
			this.IconPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.IconPictureBox.Name = "IconPictureBox";
			this.IconPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 40, true);
			this.IconPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.IconPictureBox.TabIndex = 40;
			this.IconPictureBox.TabStop = false;
			// 
			// DG_TechNameDropDownEdit
			// 
			this.DG_TechNameDropDownEdit.AllowDrop = true;
			this.DG_TechNameDropDownEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.DG_TechNameDropDownEdit, "DG_Calc_TechnicalNameRequirement");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_Calc_TechnicalNameRequirement)));
			this.DG_TechNameDropDownEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesIATAForm|1a024988-c33d-4fa9-b5fb-1ec39e6d636b", "Tech Name", "Technical Name", "Technical Name Required", "Specifies whether a Technical Name is required when this substance is used.");
			this.DG_TechNameDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(182, 130, true);
			this.DG_TechNameDropDownEdit.Name = "DG_TechNameDropDownEdit";
			this.DG_TechNameDropDownEdit.PreBoundMaxLength = 4;
			this.DG_TechNameDropDownEdit.ShouldResizeByMaxLength = true;
			this.DG_TechNameDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 20, true);
			this.DG_TechNameDropDownEdit.TabIndex = 6;
			//
			// CountryReferencesTabPage
			//
			this.CountryReferencesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesIATAForm|2f7d0c15-7c4a-de8c-4f26-c05c7330ed8c", "Country/Region References");
			this.CountryReferencesTabPage.Controls.Add(this.CountryReferencesModuleButtonGrid);
			this.CountryReferencesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CountryReferencesTabPage.Name = "CountryReferencesTabPage";
			this.CountryReferencesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(905, 203, true);
			this.CountryReferencesTabPage.TabIndex = 9;
			//
			// CountryReferencesModuleButtonGrid
			//
			this.BindingSource.SetBindingMember(this.CountryReferencesModuleButtonGrid, "UNDGCountryReferences");
			this.CountryReferencesModuleButtonGrid.Name = "CountryReferencesModuleButtonGrid";
			this.CountryReferencesModuleButtonGrid.BindToFindBoxList = "Lookups.UNDGCountryReferences";
			this.CountryReferencesModuleButtonGrid.GridId = "5ff35f09-646a-76a2-490b-e46cc8c6a8cb";
			this.CountryReferencesModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CountryReferencesModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(905, 183, true);
			// 
			// UNDGSubstanceIATAForm
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DetailsLanguageDropEdit);
			this.Controls.Add(this.CargoOnlyGroupBox);
			this.Controls.Add(this.PassengerAndCargoGroupBox);
			this.Controls.Add(this.DG_SpecialHandlingCodesTextBox3);
			this.Controls.Add(this.DG_SpecialHandlingCodesTextBox2);
			this.Controls.Add(this.DG_EmergencyResponseGuideDropDown);
			this.Controls.Add(this.DG_NOSCheckBox);
			this.Controls.Add(this.DG_HazardsTextBox);
			this.Controls.Add(this.DG_ExceptedQuantitiesDropDown);
			this.Controls.Add(this.DG_PGDownEdit);
			this.Controls.Add(this.UNDGSubstanceTabControl);
			this.Controls.Add(this.DG_SpecialHandlingCodesTextBox1);
			this.Controls.Add(this.DG_IsSystemCheckBox);
			this.Controls.Add(this.IsActiveCheckBox);
			this.Controls.Add(this.LimitedQuantityGroupBox);
			this.Controls.Add(this.DG_UNNumberTextBox);
			this.Controls.Add(this.DG_SubLabel1TextBox);
			this.Controls.Add(this.DG_SubLabel2TextBox);
			this.Controls.Add(this.SpecialProvisionsTextBox);
			this.Controls.Add(this.DG_VariantTextBox);
			this.Controls.Add(this.DG_ClassDropDownEdit);
			this.Controls.Add(this.DG_ProperShippingNameTextBox);
			this.Controls.Add(this.HeaderLabel);
			this.Controls.Add(this.IconPictureBox);
			this.Controls.Add(this.DG_TechNameDropDownEdit);
			this.Controls.Add(this.DG_BorderPanel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 532, true);
			this.Name = "UNDGSubstanceIATAControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.DG_ProperShippingNameTextBox, 0);
			this.Controls.SetChildIndex(this.DG_ClassDropDownEdit, 0);
			this.Controls.SetChildIndex(this.DG_VariantTextBox, 0);
			this.Controls.SetChildIndex(this.DG_SubLabel2TextBox, 0);
			this.Controls.SetChildIndex(this.SpecialProvisionsTextBox, 0);
			this.Controls.SetChildIndex(this.DG_SubLabel1TextBox, 0);
			this.Controls.SetChildIndex(this.DG_UNNumberTextBox, 0);
			this.Controls.SetChildIndex(this.LimitedQuantityGroupBox, 0);
			this.Controls.SetChildIndex(this.IsActiveCheckBox, 0);
			this.Controls.SetChildIndex(this.DG_IsSystemCheckBox, 0);
			this.Controls.SetChildIndex(this.DG_SpecialHandlingCodesTextBox1, 0);
			this.Controls.SetChildIndex(this.UNDGSubstanceTabControl, 0);
			this.Controls.SetChildIndex(this.DG_PGDownEdit, 0);
			this.Controls.SetChildIndex(this.DG_ExceptedQuantitiesDropDown, 0);
			this.Controls.SetChildIndex(this.DG_HazardsTextBox, 0);
			this.Controls.SetChildIndex(this.DG_NOSCheckBox, 0);
			this.Controls.SetChildIndex(this.DG_EmergencyResponseGuideDropDown, 0);
			this.Controls.SetChildIndex(this.DG_SpecialHandlingCodesTextBox2, 0);
			this.Controls.SetChildIndex(this.DG_SpecialHandlingCodesTextBox3, 0);
			this.Controls.SetChildIndex(this.PassengerAndCargoGroupBox, 0);
			this.Controls.SetChildIndex(this.CargoOnlyGroupBox, 0);
			this.Controls.SetChildIndex(this.DetailsLanguageDropEdit, 0);
			this.Controls.SetChildIndex(this.DG_TechNameDropDownEdit, 0);
			this.Controls.SetChildIndex(this.DG_BorderPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.IconPictureBox)).EndInit();
			this.LimitedQuantityGroupBox.ResumeLayout(false);
			this.LimitedQuantityGroupBox.PerformLayout();
			this.DG_LQMaxAmtTypeDropDown.ResumeLayout(true);
			this.DG_LQMaxAmtTypeDropDown.PerformLayout();
			this.DG_ClassDropDownEdit.ResumeLayout(true);
			this.DG_ClassDropDownEdit.PerformLayout();
			this.UNDGSubstanceTabControl.ResumeLayout(false);
			this.UNDGSubstanceTabControl.PerformLayout();
			this.NamesTabPage.ResumeLayout(false);
			this.NamesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.NamesGrid)).EndInit();
			this.NamesGrid.ResumeLayout(false);
			this.NamesGrid.PerformLayout();
			this.CrossReferencesTabPage.ResumeLayout(false);
			this.CrossReferencesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CrossReferencesGrid)).EndInit();
			this.CrossReferencesGrid.ResumeLayout(false);
			this.CrossReferencesGrid.PerformLayout();
			this.QualifyingDescriptiveTextTabPage.ResumeLayout(false);
			this.QualifyingDescriptiveTextTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.QualifyingDescriptiveTextGrid)).EndInit();
			this.QualifyingDescriptiveTextGrid.ResumeLayout(false);
			this.QualifyingDescriptiveTextGrid.PerformLayout();
			this.DG_PGDownEdit.ResumeLayout(true);
			this.DG_PGDownEdit.PerformLayout();
			this.DG_ExceptedQuantitiesDropDown.ResumeLayout(true);
			this.DG_ExceptedQuantitiesDropDown.PerformLayout();
			this.DG_EmergencyResponseGuideDropDown.ResumeLayout(true);
			this.DG_EmergencyResponseGuideDropDown.PerformLayout();
			this.PassengerAndCargoGroupBox.ResumeLayout(false);
			this.PassengerAndCargoGroupBox.PerformLayout();
			this.DG_LQ2OrPaxMaxAmtTypeDropDown.ResumeLayout(true);
			this.DG_LQ2OrPaxMaxAmtTypeDropDown.PerformLayout();
			this.CargoOnlyGroupBox.ResumeLayout(false);
			this.CargoOnlyGroupBox.PerformLayout();
			this.DG_CargoPackAmtTypeDropDown.ResumeLayout(true);
			this.DG_CargoPackAmtTypeDropDown.PerformLayout();
			this.DetailsLanguageDropEdit.ResumeLayout(true);
			this.DetailsLanguageDropEdit.PerformLayout();
			this.DG_TechNameDropDownEdit.ResumeLayout(true);
			this.DG_TechNameDropDownEdit.PerformLayout();
			this.CountryReferencesTabPage.ResumeLayout(false);
			this.CountryReferencesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CountryReferencesModuleButtonGrid.InnerGrid)).EndInit();
			this.CountryReferencesModuleButtonGrid.ResumeLayout(true);
			this.CountryReferencesModuleButtonGrid.PerformLayout();
			this.DG_BorderPanel.ResumeLayout(true);
			this.DG_BorderPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
