using System.Drawing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class UNDGSubstanceControl
	{
		protected Enterprise.ZArchitecture.GUI.ZTemplateTabControl UNDGSubstanceTabControl;
		Enterprise.ZArchitecture.ZTextBox DG_ProperShippingNameTextBox;
		Enterprise.ZArchitecture.ZTextBox DG_VariationTextBox;
		Enterprise.ZArchitecture.ZTextBox DG_VariantTextBox;
		Enterprise.ZArchitecture.ZTextBox DG_UNNumberTextBox;
		protected Enterprise.ZArchitecture.GUI.ZTabPage PropertiesTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTabPage ObservationsTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTabPage SpecialProvisionsTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTabPage StowageSegregationTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTabPage QualifyingDescriptiveTextTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTabPage CountryReferencesTabPage;
		Enterprise.ZArchitecture.GUI.ZTabPage AdditionalStowageTabPage;
		ZCheckBox IsActiveCheckBox;
		ZCheckBox DG_IsSystemCheckBox;
		private ZTabPage NamesTabPage;
		private ZTextBox zTextBox1;
		private ZDropEdit DetailsLanguageDropEdit;
		private ZTextBox DG_StowageCategoryTextBox;
		private ZTextBox DG_TankProvisionsTextBox;
		private ZTextBox DG_UNTankInstructionTextBox;
		private ZTextBox DG_IMOTankInstructionTextBox;
		internal ZTabPage OtherProvisionsTabPage;
		private ZTextBox DG_SeperateCompartmentTextBox;
		private ZTextBox DG_NotOnSameShipAsTextBox;
		private ZTextBox DG_SeperatedLongitudinallyFromTextBox;
		private ZTextBox DG_SeperateFromTextBox;
		private ZTextBox DG_AwayFromTextBox;
		private ZTextBox SegregationGroupsTextBox;
		private ZTextBox SegregationCodesTextBox;
		private ZTextBox DG_StowageRequirementsTextBox;
		private ZTextBox DG_PackingProvisionsTextBox;
		private ZTextBox DG_PackingInstructionTextBox;
		private ZTextBox DG_IBCInstructionTextBox;
		private ZTextBox DG_IBCProvisionsTextBox;
		private ZCheckBox UnderlinedEMSCheckBox;
		private ZTextBox DG_PointersTextBox;
		private ZTextBox DG_MarkersTextBox;
		private ZTextBox DG_EMSTextBox;
		private ZTextBox DG_TreatAsTextBox;
		private ZTextBox DG_ExplosiveLimitsTextBox;
		private ZTextBox DG_FlashPointTextBox;
		private ZGroupBox LimitedQuantityGroupBox;
		private ZTextBox zLQIndexTextBox;
		private ZTextBox zTextBox2;
		private ZCalcEdit LimitedQuantityCalcEdit;
		private ZDropEdit MarinePollutantDropEdit;
		private ZDropEdit DG_StateDropDownEdit;
		private ZDropEdit DG_ClassDropDownEdit;
		private ZTextBox DG_SubLabel1TextBox;
		private ZTextBox DG_SubLabel2TextBox;
		private ZGrid NamesGrid;
		private ZGrid ObservationsGrid;
		private ZGrid PropertiesGrid;
		private ZGrid QualifyingDescriptiveTextGrid;
		private ZModuleButtonGrid SpecialProvisionsModuleButtonGrid;
		private ZModuleButtonGrid StowageSegregationModuleButtonGrid;
		private ZModuleButtonGrid AdditionalStowageModuleButtonGrid;
		private ZDropEdit DG_PGDownEdit;
		private ZTextBox zTextBox3;
		private ZCalcEdit LimitedQuantity2CalcEdit;
		private ZDropEdit DG_ExceptedQuantitiesDropDown;
		private ZLabel HeaderLabel;
		private KPictureBox IconPictureBox;
		private ZDropEdit DG_TechNameDropDownEdit;
		private ZCheckBox DG_NOSCheckBox;
		private ZModuleButtonGrid CountryReferencesModuleButtonGrid;
		private ZPanel DG_BorderPanel;

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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo3 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo4 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo5 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo6 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			this.UnderlinedEMSCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_PointersTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_MarkersTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_EMSTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_TreatAsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_ExplosiveLimitsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_FlashPointTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LimitedQuantityGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zTextBox3 = new Enterprise.ZArchitecture.ZTextBox();
			this.LimitedQuantity2CalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zLQIndexTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.LimitedQuantityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MarinePollutantDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DG_StateDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DG_SubLabel1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_SubLabel2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_IsSystemCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DG_ProperShippingNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_VariationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_ClassDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DG_VariantTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_UNNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UNDGSubstanceTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.NamesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.NamesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ObservationsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ObservationsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PropertiesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PropertiesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.QualifyingDescriptiveTextTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CountryReferencesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.QualifyingDescriptiveTextGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SpecialProvisionsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SpecialProvisionsModuleButtonGrid = new Enterprise.ZArchitecture.GUI.ZModuleButtonGrid();
			this.StowageSegregationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DG_SeperateCompartmentTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_StowageCategoryTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_NotOnSameShipAsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_SeperatedLongitudinallyFromTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_SeperateFromTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_AwayFromTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SegregationGroupsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SegregationCodesTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_StowageRequirementsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StowageSegregationModuleButtonGrid = new Enterprise.ZArchitecture.GUI.ZModuleButtonGrid();
			this.CountryReferencesModuleButtonGrid = new CountryReferencesModuleButtonGrid();
			this.AdditionalStowageTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AdditionalStowageModuleButtonGrid = new Enterprise.ZArchitecture.GUI.ZModuleButtonGrid();
			this.OtherProvisionsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DG_PackingProvisionsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_PackingInstructionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_IBCInstructionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_IBCProvisionsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_UNTankInstructionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_IMOTankInstructionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DG_TankProvisionsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DetailsLanguageDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DG_PGDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DG_ExceptedQuantitiesDropDown = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.HeaderLabel = new Enterprise.ZArchitecture.ZLabel();
			this.IconPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.DG_TechNameDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DG_NOSCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DG_BorderPanel = new ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.IconPictureBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LimitedQuantityGroupBox.SuspendLayout();
			this.MarinePollutantDropEdit.SuspendLayout();
			this.DG_StateDropDownEdit.SuspendLayout();
			this.DG_ClassDropDownEdit.SuspendLayout();
			this.UNDGSubstanceTabControl.SuspendLayout();
			this.NamesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.NamesGrid)).BeginInit();
			this.NamesGrid.SuspendLayout();
			this.ObservationsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ObservationsGrid)).BeginInit();
			this.ObservationsGrid.SuspendLayout();
			this.PropertiesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PropertiesGrid)).BeginInit();
			this.PropertiesGrid.SuspendLayout();
			this.QualifyingDescriptiveTextTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.QualifyingDescriptiveTextGrid)).BeginInit();
			this.CountryReferencesTabPage.SuspendLayout();
			this.QualifyingDescriptiveTextGrid.SuspendLayout();
			this.SpecialProvisionsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SpecialProvisionsModuleButtonGrid.InnerGrid)).BeginInit();
			this.SpecialProvisionsModuleButtonGrid.SuspendLayout();
			this.StowageSegregationTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.StowageSegregationModuleButtonGrid.InnerGrid)).BeginInit();
			this.StowageSegregationModuleButtonGrid.SuspendLayout();
			this.AdditionalStowageTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalStowageModuleButtonGrid.InnerGrid)).BeginInit();
			this.AdditionalStowageModuleButtonGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CountryReferencesModuleButtonGrid.InnerGrid)).BeginInit();
			this.CountryReferencesModuleButtonGrid.SuspendLayout();
			this.OtherProvisionsTabPage.SuspendLayout();
			this.DetailsLanguageDropEdit.SuspendLayout();
			this.DG_PGDownEdit.SuspendLayout();
			this.DG_ExceptedQuantitiesDropDown.SuspendLayout();
			this.DG_BorderPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.UNDGSubstance);
			// 
			// UnderlinedEMSCheckBox
			// 
			this.UnderlinedEMSCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.UnderlinedEMSCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.UnderlinedEMSCheckBox, "DG_ULineEmsCheckBox");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_ULineEmsCheckBox)));
			this.UnderlinedEMSCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceForm|3f32f404-0b45-427e-b6fc-a9411a311870", "Underlined", "Underlined", "Underlined Emergency Schedule");
			this.UnderlinedEMSCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UnderlinedEMSCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 328, true);
			this.UnderlinedEMSCheckBox.Name = "UnderlinedEMSCheckBox";
			this.UnderlinedEMSCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 17, true);
			this.UnderlinedEMSCheckBox.TabIndex = 14;
			// 
			// zTextBox1
			//
			this.BindingSource.SetBindingMember(this.zTextBox1, "DG_UsrUSDOTShippingName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_UsrUSDOTShippingName)));
			this.zTextBox1.CaptionResourceString = null;
			this.zTextBox1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 130, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 20, true);
			this.zTextBox1.TabIndex = 6;
			// 
			// DG_BorderPanel
			//
			this.DG_BorderPanel.Name = "DG_BorderPanel";
			this.DG_BorderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 1, true);
			this.DG_BorderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 40, true);
			this.DG_BorderPanel.BackColor = Color.DarkGray;
			// 
			// DG_PointersTextBox
			// 
			this.DG_PointersTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.DG_PointersTextBox, "DG_Pointers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_Pointers)));
			this.DG_PointersTextBox.CaptionResourceString = null;
			this.DG_PointersTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DG_PointersTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(702, 404, true);
			this.DG_PointersTextBox.Name = "DG_PointersTextBox";
			this.DG_PointersTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.DG_PointersTextBox.TabIndex = 26;
			// 
			// DG_MarkersTextBox
			// 
			this.DG_MarkersTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.DG_MarkersTextBox, "DG_Markers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_Markers)));
			this.DG_MarkersTextBox.CaptionResourceString = null;
			this.DG_MarkersTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DG_MarkersTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(702, 378, true);
			this.DG_MarkersTextBox.Name = "DG_MarkersTextBox";
			this.DG_MarkersTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.DG_MarkersTextBox.TabIndex = 25;
			// 
			// DG_EMSTextBox
			// 
			this.DG_EMSTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.DG_EMSTextBox, "DG_EMS");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_EMS)));
			this.DG_EMSTextBox.CaptionResourceString = null;
			this.DG_EMSTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 325, true);
			this.DG_EMSTextBox.Name = "DG_EMSTextBox";
			this.DG_EMSTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 20, true);
			this.DG_EMSTextBox.TabIndex = 13;
			// 
			// DG_TreatAsTextBox
			// 
			this.DG_TreatAsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.DG_TreatAsTextBox, "DG_TreatAs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_TreatAs)));
			this.DG_TreatAsTextBox.CaptionResourceString = null;
			this.DG_TreatAsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DG_TreatAsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(702, 352, true);
			this.DG_TreatAsTextBox.Name = "DG_TreatAsTextBox";
			this.DG_TreatAsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.DG_TreatAsTextBox.TabIndex = 24;
			// 
			// DG_ExplosiveLimitsTextBox
			// 
			this.DG_ExplosiveLimitsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.DG_ExplosiveLimitsTextBox, "DG_ExpLim");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_ExpLim)));
			this.DG_ExplosiveLimitsTextBox.CaptionResourceString = null;
			this.DG_ExplosiveLimitsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DG_ExplosiveLimitsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(702, 300, true);
			this.DG_ExplosiveLimitsTextBox.Name = "DG_ExplosiveLimitsTextBox";
			this.DG_ExplosiveLimitsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.DG_ExplosiveLimitsTextBox.TabIndex = 22;
			// 
			// DG_FlashPointTextBox
			// 
			this.DG_FlashPointTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.DG_FlashPointTextBox, "DG_FlashPoint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_FlashPoint)));
			this.DG_FlashPointTextBox.CaptionResourceString = null;
			this.DG_FlashPointTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DG_FlashPointTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(702, 326, true);
			this.DG_FlashPointTextBox.Name = "DG_FlashPointTextBox";
			this.DG_FlashPointTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.DG_FlashPointTextBox.TabIndex = 23;
			// 
			// LimitedQuantityGroupBox
			// 
			this.LimitedQuantityGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceForm|84da71a1-d3d9-411f-aaef-03b69d2ace57", "Limited Quantity");
			this.LimitedQuantityGroupBox.Controls.Add(this.zTextBox3);
			this.LimitedQuantityGroupBox.Controls.Add(this.LimitedQuantity2CalcEdit);
			this.LimitedQuantityGroupBox.Controls.Add(this.zLQIndexTextBox);
			this.LimitedQuantityGroupBox.Controls.Add(this.zTextBox2);
			this.LimitedQuantityGroupBox.Controls.Add(this.LimitedQuantityCalcEdit);
			this.LimitedQuantityGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(556, 156, true);
			this.LimitedQuantityGroupBox.Name = "LimitedQuantityGroupBox";
			this.LimitedQuantityGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 100, true);
			this.LimitedQuantityGroupBox.TabIndex = 17;
			this.LimitedQuantityGroupBox.TabStop = false;
			// 
			// zTextBox3
			// 
			this.BindingSource.SetBindingMember(this.zTextBox3, "DG_LQ2OrPaxMaxAmtUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_LQ2OrPaxMaxAmtUQ)));
			this.zTextBox3.CaptionResourceString = null;
			this.zTextBox3.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox3, false);
			this.zTextBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(281, 46, true);
			this.zTextBox3.Name = "zTextBox3";
			this.zTextBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.zTextBox3.TabIndex = 19;
			// 
			// LimitedQuantity2CalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LimitedQuantity2CalcEdit, "DG_LQ2OrPaxMaxAmt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_LQ2OrPaxMaxAmt)));
			this.LimitedQuantity2CalcEdit.CaptionResourceString = null;
			this.LimitedQuantity2CalcEdit.DecimalPlaces = 2;
			this.LimitedQuantity2CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 46, true);
			this.LimitedQuantity2CalcEdit.Name = "LimitedQuantity2CalcEdit";
			this.LimitedQuantity2CalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			this.LimitedQuantity2CalcEdit.TabIndex = 18;
			this.LimitedQuantity2CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zLQIndexTextBox
			// 
			this.BindingSource.SetBindingMember(this.zLQIndexTextBox, "DG_LQSpecProvIndex");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_LQSpecProvIndex)));
			this.zLQIndexTextBox.CaptionResourceString = null;
			this.zLQIndexTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 72, true);
			this.zLQIndexTextBox.Name = "zLQIndexTextBox";
			this.zLQIndexTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.zLQIndexTextBox.TabIndex = 20;
			this.zLQIndexTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zTextBox2
			// 
			this.BindingSource.SetBindingMember(this.zTextBox2, "DG_LQMaxAmtUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_LQMaxAmtUQ)));
			this.zTextBox2.CaptionResourceString = null;
			this.zTextBox2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox2, false);
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(281, 20, true);
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.zTextBox2.TabIndex = 17;
			// 
			// LimitedQuantityCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LimitedQuantityCalcEdit, "DG_LQMaxAmt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_LQMaxAmt)));
			this.LimitedQuantityCalcEdit.CaptionResourceString = null;
			this.LimitedQuantityCalcEdit.DecimalPlaces = 2;
			this.LimitedQuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 20, true);
			this.LimitedQuantityCalcEdit.Name = "LimitedQuantityCalcEdit";
			this.LimitedQuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			this.LimitedQuantityCalcEdit.TabIndex = 16;
			this.LimitedQuantityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MarinePollutantDropEdit
			// 
			this.MarinePollutantDropEdit.AllowDrop = true;
			this.MarinePollutantDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.MarinePollutantDropEdit, "DG_MP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_MP)));
			this.MarinePollutantDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 300, true);
			this.MarinePollutantDropEdit.Name = "MarinePollutantDropEdit";
			this.MarinePollutantDropEdit.PreBoundMaxLength = 4;
			this.MarinePollutantDropEdit.ShouldResizeByMaxLength = true;
			this.MarinePollutantDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 20, true);
			this.MarinePollutantDropEdit.TabIndex = 12;
			// 
			// DG_StateDropDownEdit
			// 
			this.DG_StateDropDownEdit.AllowDrop = true;
			this.DG_StateDropDownEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.DG_StateDropDownEdit, "DG_State");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_State)));
			this.DG_StateDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 248, true);
			this.DG_StateDropDownEdit.Name = "DG_StateDropDownEdit";
			this.DG_StateDropDownEdit.PreBoundMaxLength = 4;
			this.DG_StateDropDownEdit.ShouldResizeByMaxLength = true;
			this.DG_StateDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 20, true);
			this.DG_StateDropDownEdit.TabIndex = 10;
			// 
			// DG_SubLabel1TextBox
			// 
			this.DG_SubLabel1TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.DG_SubLabel1TextBox, "DG_SubLabel1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_SubLabel1)));
			this.DG_SubLabel1TextBox.CaptionResourceString = null;
			this.DG_SubLabel1TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DG_SubLabel1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 196, true);
			this.DG_SubLabel1TextBox.Name = "DG_SubLabel1TextBox";
			this.DG_SubLabel1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.DG_SubLabel1TextBox.TabIndex = 8;
			// 
			// DG_SubLabel2TextBox
			// 
			this.DG_SubLabel2TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.DG_SubLabel2TextBox, "DG_SubLabel2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_SubLabel2)));
			this.DG_SubLabel2TextBox.CaptionResourceString = null;
			this.DG_SubLabel2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DG_SubLabel2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 222, true);
			this.DG_SubLabel2TextBox.Name = "DG_SubLabel2TextBox";
			this.DG_SubLabel2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.DG_SubLabel2TextBox.TabIndex = 9;
			// 
			// DG_IsSystemCheckBox
			// 
			this.DG_IsSystemCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.DG_IsSystemCheckBox, "DG_IsSystem");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_IsSystem)));
			this.DG_IsSystemCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.DG_IsSystemCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DG_IsSystemCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(781, 55, true);
			this.DG_IsSystemCheckBox.Name = "DG_IsSystemCheckBox";
			this.DG_IsSystemCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.DG_IsSystemCheckBox.TabIndex = 3;
			// 
			// IsActiveCheckBox
			// 
			this.IsActiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsActiveCheckBox, "DG_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_IsActive)));
			this.IsActiveCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(862, 55, true);
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
			this.DG_ProperShippingNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 78, true);
			this.DG_ProperShippingNameTextBox.Name = "DG_ProperShippingNameTextBox";
			this.DG_ProperShippingNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 20, true);
			this.DG_ProperShippingNameTextBox.TabIndex = 4;
			// 
			// DG_VariationTextBox
			// 
			this.BindingSource.SetBindingMember(this.DG_VariationTextBox, "DG_Variation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_Variation)));
			this.DG_VariationTextBox.CaptionResourceString = null;
			this.DG_VariationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DG_VariationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 104, true);
			this.DG_VariationTextBox.Name = "DG_VariationTextBox";
			this.DG_VariationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 20, true);
			this.DG_VariationTextBox.TabIndex = 5;
			// 
			// DG_ClassDropDownEdit
			// 
			this.DG_ClassDropDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DG_ClassDropDownEdit, "DG_Class");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_Class)));
			this.DG_ClassDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(314, 52, true);
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
			this.DG_VariantTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 52, true);
			this.DG_VariantTextBox.Name = "DG_VariantTextBox";
			this.DG_VariantTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 20, true);
			this.DG_VariantTextBox.TabIndex = 1;
			// 
			// DG_UNNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.DG_UNNumberTextBox, "DG_UNNO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_UNNO)));
			this.DG_UNNumberTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceForm|5BD7023E-1B02-4345-8ED4-6F0EB6436D57", "UN No.", "UN No.", "The United Nations (UN) number for this substance.");
			this.DG_UNNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 52, true);
			this.DG_UNNumberTextBox.Name = "DG_UNNumberTextBox";
			this.DG_UNNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.DG_UNNumberTextBox.TabIndex = 0;
			// 
			// DG_TechNameDropDownEdit
			// 
			this.DG_TechNameDropDownEdit.AllowDrop = true;
			this.DG_TechNameDropDownEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.DG_TechNameDropDownEdit, "DG_Calc_TechnicalNameRequirement");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_Calc_TechnicalNameRequirement)));
			this.DG_TechNameDropDownEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceForm|1a024988-c33d-4fa9-b5fb-1ec39e6d636b", "Tech Name", "Technical Name", "Technical Name Required", "Specifies whether a Technical Name is required when this substance is used.");
			this.DG_TechNameDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 154, true);
			this.DG_TechNameDropDownEdit.Name = "DG_TechNameDropDownEdit";
			this.DG_TechNameDropDownEdit.PreBoundMaxLength = 4;
			this.DG_TechNameDropDownEdit.ShouldResizeByMaxLength = true;
			this.DG_TechNameDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 20, true);
			this.DG_TechNameDropDownEdit.TabIndex = 7;
			// 
			// UNDGSubstanceTabControl
			// 
			this.UNDGSubstanceTabControl.Controls.Add(this.NamesTabPage);
			this.UNDGSubstanceTabControl.Controls.Add(this.ObservationsTabPage);
			this.UNDGSubstanceTabControl.Controls.Add(this.PropertiesTabPage);
			this.UNDGSubstanceTabControl.Controls.Add(this.QualifyingDescriptiveTextTabPage);
			this.UNDGSubstanceTabControl.Controls.Add(this.SpecialProvisionsTabPage);
			this.UNDGSubstanceTabControl.Controls.Add(this.StowageSegregationTabPage);
			this.UNDGSubstanceTabControl.Controls.Add(this.AdditionalStowageTabPage);
			this.UNDGSubstanceTabControl.Controls.Add(this.OtherProvisionsTabPage);
			this.UNDGSubstanceTabControl.Controls.Add(this.CountryReferencesTabPage);
			this.UNDGSubstanceTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 465, true);
			this.UNDGSubstanceTabControl.Name = "UNDGSubstanceTabControl";
			this.UNDGSubstanceTabControl.SelectedIndex = 0;
			this.UNDGSubstanceTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(855, 152, true);
			this.UNDGSubstanceTabControl.TabIndex = 28;
			// 
			// NamesTabPage
			// 
			this.NamesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceForm|f7104fd9-bf95-4f7d-89c1-26d4c97c8cec", "Names");
			this.NamesTabPage.Controls.Add(this.NamesGrid);
			this.NamesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.NamesTabPage.Name = "NamesTabPage";
			this.NamesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(905, 203, true);
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
			this.NamesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(905, 183, true);
			this.NamesGrid.TabIndex = 1;
			// 
			// ObservationsTabPage
			// 
			this.ObservationsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceForm|b3d7463d-55b0-465e-9f0e-7db65a4ffbd7", "Observations");
			this.ObservationsTabPage.Controls.Add(this.ObservationsGrid);
			this.ObservationsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ObservationsTabPage.Name = "ObservationsTabPage";
			this.ObservationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(905, 203, true);
			this.ObservationsTabPage.TabIndex = 1;
			// 
			// ObservationsGrid
			// 
			this.ObservationsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ObservationsGrid, "Observations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).Observations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ViewUNDGAttribute)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).Observations)).SyncRoot)).DA_Language)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ViewUNDGAttribute)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).Observations)).SyncRoot)).DA_Descriptor)));
			this.ObservationsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo2.ColumnName = "DA_Language";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zMultiLineTextBoxColumnInfo1.ColumnName = "DA_Descriptor";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(800);
			this.ObservationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ObservationsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.ObservationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ObservationsGrid.GridId = "cf8defc9-9faa-4e3d-bd91-770f5f6be77c";
			this.ObservationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ObservationsGrid.LayoutKey = "zGrid3";
			this.ObservationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ObservationsGrid.Name = "ObservationsGrid";
			this.ObservationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(905, 183, true);
			this.ObservationsGrid.TabIndex = 0;
			// 
			// PropertiesTabPage
			// 
			this.PropertiesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceForm|b04c5c5e-7d4c-4576-87dc-e398e2be36c3", "Properties");
			this.PropertiesTabPage.Controls.Add(this.PropertiesGrid);
			this.PropertiesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PropertiesTabPage.Name = "PropertiesTabPage";
			this.PropertiesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(905, 203, true);
			this.PropertiesTabPage.TabIndex = 0;
			// 
			// PropertiesGrid
			// 
			this.PropertiesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PropertiesGrid, "Properties");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).Properties)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ViewUNDGAttribute)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).Properties)).SyncRoot)).DA_Language)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ViewUNDGAttribute)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).Properties)).SyncRoot)).DA_Descriptor)));
			this.PropertiesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo3.ColumnName = "DA_Language";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zMultiLineTextBoxColumnInfo2.ColumnName = "DA_Descriptor";
			zMultiLineTextBoxColumnInfo2.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(800);
			this.PropertiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.PropertiesGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo2);
			this.PropertiesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PropertiesGrid.GridId = "1b470ab2-e2d6-4f97-a17e-c8c2bdcd96c3";
			this.PropertiesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PropertiesGrid.LayoutKey = "Properties";
			this.PropertiesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PropertiesGrid.Name = "PropertiesGrid";
			this.PropertiesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(905, 183, true);
			this.PropertiesGrid.TabIndex = 0;
			// 
			// QualifyingDescriptiveTextTabPage
			// 
			this.QualifyingDescriptiveTextTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceForm|dd9e3650-7ff8-4a7d-ad63-fec300288628", "Qualifying Descriptive Text");
			this.QualifyingDescriptiveTextTabPage.Controls.Add(this.QualifyingDescriptiveTextGrid);
			this.QualifyingDescriptiveTextTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.QualifyingDescriptiveTextTabPage.Name = "QualifyingDescriptiveTextTabPage";
			this.QualifyingDescriptiveTextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(905, 203, true);
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
			zDropEditColumnStyleInfo4.ColumnName = "DA_Language";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zMultiLineTextBoxColumnInfo3.ColumnName = "DA_Descriptor";
			zMultiLineTextBoxColumnInfo3.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(800);
			this.QualifyingDescriptiveTextGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.QualifyingDescriptiveTextGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo3);
			this.QualifyingDescriptiveTextGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.QualifyingDescriptiveTextGrid.GridId = "b6e1bb48-1149-4a1e-919a-ad6d8b9edeb8";
			this.QualifyingDescriptiveTextGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.QualifyingDescriptiveTextGrid.LayoutKey = "zGrid2";
			this.QualifyingDescriptiveTextGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.QualifyingDescriptiveTextGrid.Name = "QualifyingDescriptiveTextGrid";
			this.QualifyingDescriptiveTextGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(905, 183, true);
			this.QualifyingDescriptiveTextGrid.TabIndex = 0;
			//
			// CountryReferencesTabPage
			//
			this.CountryReferencesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceForm|82DDD2C2-142A-48AB-B655-CE44FC105C79", "Country/Region References");
			this.CountryReferencesTabPage.Controls.Add(this.CountryReferencesModuleButtonGrid);
			this.CountryReferencesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CountryReferencesTabPage.Name = "CountryReferencesTabPageTabPage";
			this.CountryReferencesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(905, 203, true);
			this.CountryReferencesTabPage.TabIndex = 9;
			//
			// CountryReferencesModuleButtonGrid
			//
			this.BindingSource.SetBindingMember(this.CountryReferencesModuleButtonGrid, "UNDGCountryReferences");
			this.CountryReferencesModuleButtonGrid.Name = "CountryReferencesModuleButtonGrid";
			this.CountryReferencesModuleButtonGrid.BindToFindBoxList = "Lookups.UNDGCountryReferences";
			this.CountryReferencesModuleButtonGrid.GridId = "f8cb1291-2b3a-7099-46d4-1de59a7dc51a";
			this.CountryReferencesModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CountryReferencesModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(905, 183, true);
			// 
			// SpecialProvisionsTabPage
			// 
			this.SpecialProvisionsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceForm|bb7236c2-4af7-4f5f-a715-14c329a648eb", "Special Provisions", "The Special Provision applied to this substance.");
			this.SpecialProvisionsTabPage.Controls.Add(this.SpecialProvisionsModuleButtonGrid);
			this.SpecialProvisionsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SpecialProvisionsTabPage.Name = "SpecialProvisionsTabPage";
			this.SpecialProvisionsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(905, 203, true);
			this.SpecialProvisionsTabPage.TabIndex = 3;
			// 
			// SpecialProvisionsModuleButtonGrid
			// 
			this.SpecialProvisionsModuleButtonGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SpecialProvisionsModuleButtonGrid, "SpecialProvisions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).SpecialProvisions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).Lookups.SpecialProvisions)));
			this.SpecialProvisionsModuleButtonGrid.BindToFindBoxList = "Lookups.SpecialProvisions";
			zTextBoxColumnStyleInfo2.ColumnName = "DC_Language";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo3.ColumnName = "DC_Index";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zMultiLineTextBoxColumnInfo4.ColumnName = "DC_Descriptor";
			zMultiLineTextBoxColumnInfo4.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(800);
			this.SpecialProvisionsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SpecialProvisionsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.SpecialProvisionsModuleButtonGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo4);
			this.SpecialProvisionsModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SpecialProvisionsModuleButtonGrid.GridId = "d6c9866d-7b65-4326-9518-7cac83bdfe33";
			// 
			// 
			// 
			this.SpecialProvisionsModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.SpecialProvisionsModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.SpecialProvisionsModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.SpecialProvisionsModuleButtonGrid.InnerGrid.GridId = "d6c9866d-7b65-4326-9518-7cac83bdfe33";
			this.SpecialProvisionsModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SpecialProvisionsModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.SpecialProvisionsModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SpecialProvisionsModuleButtonGrid.InnerGrid.Name = "Grid";
			this.SpecialProvisionsModuleButtonGrid.InnerGrid.ReadOnly = true;
			this.SpecialProvisionsModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(899, 145, true);
			this.SpecialProvisionsModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.SpecialProvisionsModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SpecialProvisionsModuleButtonGrid.Name = "SpecialProvisionsModuleButtonGrid";
			this.SpecialProvisionsModuleButtonGrid.NameOfAGridElement = Enterprise.MasterFiles.GUI.Res.GetData("EB381718-9789-41FA-BB69-9BAF321D224D", "Common Provision");
			this.SpecialProvisionsModuleButtonGrid.ReadOnly = true;
			this.SpecialProvisionsModuleButtonGrid.ShowEditButton = false;
			this.SpecialProvisionsModuleButtonGrid.ShowNewButton = false;
			this.SpecialProvisionsModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(905, 183, true);
			this.SpecialProvisionsModuleButtonGrid.TabIndex = 0;
			// 
			// StowageSegregationTabPage
			// 
			this.StowageSegregationTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceForm|cb26016b-84b5-47f7-aa4a-e99c80979074", "Stowage and Segregation", "Stowage and Segregation Requirements.");
			this.StowageSegregationTabPage.Controls.Add(this.DG_SeperateCompartmentTextBox);
			this.StowageSegregationTabPage.Controls.Add(this.DG_StowageCategoryTextBox);
			this.StowageSegregationTabPage.Controls.Add(this.DG_NotOnSameShipAsTextBox);
			this.StowageSegregationTabPage.Controls.Add(this.DG_SeperatedLongitudinallyFromTextBox);
			this.StowageSegregationTabPage.Controls.Add(this.DG_SeperateFromTextBox);
			this.StowageSegregationTabPage.Controls.Add(this.DG_AwayFromTextBox);
			this.StowageSegregationTabPage.Controls.Add(this.SegregationGroupsTextBox);
			this.StowageSegregationTabPage.Controls.Add(this.SegregationCodesTextBox);
			this.StowageSegregationTabPage.Controls.Add(this.DG_StowageRequirementsTextBox);
			this.StowageSegregationTabPage.Controls.Add(this.StowageSegregationModuleButtonGrid);
			this.StowageSegregationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.StowageSegregationTabPage.Name = "StowageSegregationTabPage";
			this.StowageSegregationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(905, 203, true);
			this.StowageSegregationTabPage.TabIndex = 4;
			// 
			// DG_SeperateCompartmentTextBox
			// 
			this.BindingSource.SetBindingMember(this.DG_SeperateCompartmentTextBox, "DG_SeperateCompartmentFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_SeperateCompartmentFrom)));
			this.DG_SeperateCompartmentTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceForm|ba892266-ef26-4718-aafe-eea6e4977b75", "Separate Compartment/Hold", "Separated by a complete compartment or hold from", "");
			this.DG_SeperateCompartmentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 127, true);
			this.DG_SeperateCompartmentTextBox.Name = "DG_SeperateCompartmentTextBox";
			this.DG_SeperateCompartmentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 20, true);
			this.DG_SeperateCompartmentTextBox.TabIndex = 6;
			// 
			// DG_StowageCategoryTextBox
			// 
			this.BindingSource.SetBindingMember(this.DG_StowageCategoryTextBox, "DG_StowCat");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_StowCat)));
			this.DG_StowageCategoryTextBox.CaptionResourceString = null;
			this.DG_StowageCategoryTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DG_StowageCategoryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 64, true);
			this.DG_StowageCategoryTextBox.Name = "DG_StowageCategoryTextBox";
			this.DG_StowageCategoryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.DG_StowageCategoryTextBox.TabIndex = 3;
			// 
			// DG_NotOnSameShipAsTextBox
			// 
			this.BindingSource.SetBindingMember(this.DG_NotOnSameShipAsTextBox, "DG_NotOnSameShipAs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_NotOnSameShipAs)));
			this.DG_NotOnSameShipAsTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceForm|3dbd2b96-2669-4a76-a3a2-ab133c874e38", "Different Ship From", "Not allowed on the same ship as", "");
			this.DG_NotOnSameShipAsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 169, true);
			this.DG_NotOnSameShipAsTextBox.Name = "DG_NotOnSameShipAsTextBox";
			this.DG_NotOnSameShipAsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 20, true);
			this.DG_NotOnSameShipAsTextBox.TabIndex = 8;
			// 
			// DG_SeperatedLongitudinallyFromTextBox
			// 
			this.BindingSource.SetBindingMember(this.DG_SeperatedLongitudinallyFromTextBox, "DG_SeperatedLongitudinallyFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_SeperatedLongitudinallyFrom)));
			this.DG_SeperatedLongitudinallyFromTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceForm|22d59785-26af-4b38-adc5-6110091197af", "Separate Longitudinally", "Separated longitudinally by an intervening complete compartment or hold from", "");
			this.DG_SeperatedLongitudinallyFromTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 148, true);
			this.DG_SeperatedLongitudinallyFromTextBox.Name = "DG_SeperatedLongitudinallyFromTextBox";
			this.DG_SeperatedLongitudinallyFromTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 20, true);
			this.DG_SeperatedLongitudinallyFromTextBox.TabIndex = 7;
			// 
			// DG_SeperateFromTextBox
			// 
			this.BindingSource.SetBindingMember(this.DG_SeperateFromTextBox, "DG_SeperateFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_SeperateFrom)));
			this.DG_SeperateFromTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceForm|55226224-d7cd-4063-9f39-515ea41e30d1", "Separate From");
			this.DG_SeperateFromTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 106, true);
			this.DG_SeperateFromTextBox.Name = "DG_SeperateFromTextBox";
			this.DG_SeperateFromTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 20, true);
			this.DG_SeperateFromTextBox.TabIndex = 5;
			// 
			// DG_AwayFromTextBox
			// 
			this.DG_AwayFromTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.DG_AwayFromTextBox, "DG_AwayFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_AwayFrom)));
			this.DG_AwayFromTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceForm|5bef9d03-52b4-4a1c-a224-2b364668a3c2", "Away From");
			this.DG_AwayFromTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 85, true);
			this.DG_AwayFromTextBox.Name = "DG_AwayFromTextBox";
			this.DG_AwayFromTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 20, true);
			this.DG_AwayFromTextBox.TabIndex = 4;
			// 
			// SegregationGroupsTextBox
			// 
			this.BindingSource.SetBindingMember(this.SegregationGroupsTextBox, "SegregationGroupsForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).SegregationGroupsForBinding)));
			this.SegregationGroupsTextBox.CaptionResourceString = null;
			this.SegregationGroupsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SegregationGroupsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 1, true);
			this.SegregationGroupsTextBox.Name = "SegregationGroupsTextBox";
			this.SegregationGroupsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 20, true);
			this.SegregationGroupsTextBox.TabIndex = 0;
			// 
			// SegregationCodesTextBox
			// 
			this.BindingSource.SetBindingMember(this.SegregationCodesTextBox, "SegregationCodesForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).SegregationCodesForBinding)));
			this.SegregationCodesTextBox.CaptionResourceString = null;
			this.SegregationCodesTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SegregationCodesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 22, true);
			this.SegregationCodesTextBox.Name = "SegregationCodesTextBox";
			this.SegregationCodesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 20, true);
			this.SegregationCodesTextBox.TabIndex = 1;
			// 
			// DG_StowageRequirementsTextBox
			// 
			this.BindingSource.SetBindingMember(this.DG_StowageRequirementsTextBox, "DG_CodedStow");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_CodedStow)));
			this.DG_StowageRequirementsTextBox.CaptionResourceString = null;
			this.DG_StowageRequirementsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DG_StowageRequirementsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 43, true);
			this.DG_StowageRequirementsTextBox.Name = "DG_StowageRequirementsTextBox";
			this.DG_StowageRequirementsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 20, true);
			this.DG_StowageRequirementsTextBox.TabIndex = 2;
			// 
			// StowageSegregationModuleButtonGrid
			// 
			this.StowageSegregationModuleButtonGrid.AllowDrop = true;
			this.StowageSegregationModuleButtonGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.StowageSegregationModuleButtonGrid, "StowageSegmentationDangerous");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).StowageSegmentationDangerous)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).Lookups.StowageSegmentationRequirements)));
			this.StowageSegregationModuleButtonGrid.BindToFindBoxList = "Lookups.StowageSegmentationRequirements";
			zTextBoxColumnStyleInfo4.ColumnName = "DC_Language";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo5.ColumnName = "DC_Index";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zMultiLineTextBoxColumnInfo5.ColumnName = "DC_Descriptor";
			zMultiLineTextBoxColumnInfo5.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(800);
			this.StowageSegregationModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.StowageSegregationModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.StowageSegregationModuleButtonGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo5);
			this.StowageSegregationModuleButtonGrid.GridId = "e418105f-15e6-4ca6-9e1f-4c8c52030fba";
			// 
			// 
			// 
			this.StowageSegregationModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.StowageSegregationModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.StowageSegregationModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.StowageSegregationModuleButtonGrid.InnerGrid.GridId = "e418105f-15e6-4ca6-9e1f-4c8c52030fba";
			this.StowageSegregationModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.StowageSegregationModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.StowageSegregationModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.StowageSegregationModuleButtonGrid.InnerGrid.Name = "Grid";
			this.StowageSegregationModuleButtonGrid.InnerGrid.ReadOnly = true;
			this.StowageSegregationModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 139, true);
			this.StowageSegregationModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.StowageSegregationModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(423, 3, true);
			this.StowageSegregationModuleButtonGrid.Name = "StowageSegregationModuleButtonGrid";
			this.StowageSegregationModuleButtonGrid.NameOfAGridElement = Enterprise.MasterFiles.GUI.Res.GetData("EB381718-9789-41FA-BB69-9BAF321D224D", "Common Provision");
			this.StowageSegregationModuleButtonGrid.ReadOnly = true;
			this.StowageSegregationModuleButtonGrid.ShowEditButton = false;
			this.StowageSegregationModuleButtonGrid.ShowNewButton = false;
			this.StowageSegregationModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 177, true);
			this.StowageSegregationModuleButtonGrid.TabIndex = 7;
			// 
			// AdditionalStowageTabPage
			// 
			this.AdditionalStowageTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceForm|311a90be-aa18-4ed5-ab33-5b0f29bdace5", "Cargo Stowage", "Additional Stowage Requirements for Cargo.");
			this.AdditionalStowageTabPage.Controls.Add(this.AdditionalStowageModuleButtonGrid);
			this.AdditionalStowageTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AdditionalStowageTabPage.Name = "AdditionalStowageTabPage";
			this.AdditionalStowageTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(905, 203, true);
			this.AdditionalStowageTabPage.TabIndex = 6;
			// 
			// AdditionalStowageModuleButtonGrid
			// 
			this.AdditionalStowageModuleButtonGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AdditionalStowageModuleButtonGrid, "StowageSegmentationCargo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).StowageSegmentationCargo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).Lookups.StowageSegmentationRequirements)));
			this.AdditionalStowageModuleButtonGrid.BindToFindBoxList = "Lookups.StowageSegmentationRequirements";
			zTextBoxColumnStyleInfo6.ColumnName = "DC_Language";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo7.ColumnName = "DC_Index";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zMultiLineTextBoxColumnInfo6.ColumnName = "DC_Descriptor";
			zMultiLineTextBoxColumnInfo6.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(800);
			this.AdditionalStowageModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.AdditionalStowageModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.AdditionalStowageModuleButtonGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo6);
			this.AdditionalStowageModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalStowageModuleButtonGrid.GridId = "cf4ccb39-70a3-4940-a9fd-04b3ff52d1f7";
			// 
			// 
			// 
			this.AdditionalStowageModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.AdditionalStowageModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.AdditionalStowageModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.AdditionalStowageModuleButtonGrid.InnerGrid.GridId = "cf4ccb39-70a3-4940-a9fd-04b3ff52d1f7";
			this.AdditionalStowageModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdditionalStowageModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.AdditionalStowageModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AdditionalStowageModuleButtonGrid.InnerGrid.Name = "Grid";
			this.AdditionalStowageModuleButtonGrid.InnerGrid.ReadOnly = true;
			this.AdditionalStowageModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(899, 165, true);
			this.AdditionalStowageModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.AdditionalStowageModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalStowageModuleButtonGrid.Name = "AdditionalStowageModuleButtonGrid";
			this.AdditionalStowageModuleButtonGrid.NameOfAGridElement = Enterprise.MasterFiles.GUI.Res.GetData("EB381718-9789-41FA-BB69-9BAF321D224D", "Common Provision");
			this.AdditionalStowageModuleButtonGrid.ReadOnly = true;
			this.AdditionalStowageModuleButtonGrid.ShowEditButton = false;
			this.AdditionalStowageModuleButtonGrid.ShowNewButton = false;
			this.AdditionalStowageModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(905, 203, true);
			this.AdditionalStowageModuleButtonGrid.TabIndex = 1;
			// 
			// OtherProvisionsTabPage
			// 
			this.OtherProvisionsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceForm|c720975b-0c4b-43a3-ada9-2b3d1f99b481", "Other Provisions");
			this.OtherProvisionsTabPage.Controls.Add(this.DG_PackingProvisionsTextBox);
			this.OtherProvisionsTabPage.Controls.Add(this.DG_PackingInstructionTextBox);
			this.OtherProvisionsTabPage.Controls.Add(this.DG_IBCInstructionTextBox);
			this.OtherProvisionsTabPage.Controls.Add(this.DG_IBCProvisionsTextBox);
			this.OtherProvisionsTabPage.Controls.Add(this.DG_UNTankInstructionTextBox);
			this.OtherProvisionsTabPage.Controls.Add(this.DG_IMOTankInstructionTextBox);
			this.OtherProvisionsTabPage.Controls.Add(this.DG_TankProvisionsTextBox);
			this.OtherProvisionsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.OtherProvisionsTabPage.Name = "OtherProvisionsTabPage";
			this.OtherProvisionsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.OtherProvisionsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(905, 202, true);
			this.OtherProvisionsTabPage.TabIndex = 8;
			// 
			// DG_PackingProvisionsTextBox
			// 
			this.BindingSource.SetBindingMember(this.DG_PackingProvisionsTextBox, "DG_PackProv");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_PackProv)));
			this.DG_PackingProvisionsTextBox.CaptionResourceString = null;
			this.DG_PackingProvisionsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DG_PackingProvisionsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 123, true);
			this.DG_PackingProvisionsTextBox.Name = "DG_PackingProvisionsTextBox";
			this.DG_PackingProvisionsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(454, 20, true);
			this.DG_PackingProvisionsTextBox.TabIndex = 6;
			// 
			// DG_PackingInstructionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DG_PackingInstructionTextBox, "DG_PackIns");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_PackIns)));
			this.DG_PackingInstructionTextBox.CaptionResourceString = null;
			this.DG_PackingInstructionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DG_PackingInstructionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 97, true);
			this.DG_PackingInstructionTextBox.Name = "DG_PackingInstructionTextBox";
			this.DG_PackingInstructionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(454, 20, true);
			this.DG_PackingInstructionTextBox.TabIndex = 5;
			// 
			// DG_IBCInstructionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DG_IBCInstructionTextBox, "DG_IBCIns");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_IBCIns)));
			this.DG_IBCInstructionTextBox.CaptionResourceString = null;
			this.DG_IBCInstructionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DG_IBCInstructionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(508, 19, true);
			this.DG_IBCInstructionTextBox.Name = "DG_IBCInstructionTextBox";
			this.DG_IBCInstructionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.DG_IBCInstructionTextBox.TabIndex = 3;
			// 
			// DG_IBCProvisionsTextBox
			// 
			this.BindingSource.SetBindingMember(this.DG_IBCProvisionsTextBox, "DG_IBCProv");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_IBCProv)));
			this.DG_IBCProvisionsTextBox.CaptionResourceString = null;
			this.DG_IBCProvisionsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DG_IBCProvisionsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(508, 43, true);
			this.DG_IBCProvisionsTextBox.Name = "DG_IBCProvisionsTextBox";
			this.DG_IBCProvisionsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.DG_IBCProvisionsTextBox.TabIndex = 4;
			// 
			// DG_UNTankInstructionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DG_UNTankInstructionTextBox, "DG_UNTankIns");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_UNTankIns)));
			this.DG_UNTankInstructionTextBox.CaptionResourceString = null;
			this.DG_UNTankInstructionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DG_UNTankInstructionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 19, true);
			this.DG_UNTankInstructionTextBox.Name = "DG_UNTankInstructionTextBox";
			this.DG_UNTankInstructionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			this.DG_UNTankInstructionTextBox.TabIndex = 0;
			// 
			// DG_IMOTankInstructionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DG_IMOTankInstructionTextBox, "DG_IMOTankIns");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_IMOTankIns)));
			this.DG_IMOTankInstructionTextBox.CaptionResourceString = null;
			this.DG_IMOTankInstructionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DG_IMOTankInstructionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 45, true);
			this.DG_IMOTankInstructionTextBox.Name = "DG_IMOTankInstructionTextBox";
			this.DG_IMOTankInstructionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			this.DG_IMOTankInstructionTextBox.TabIndex = 1;
			// 
			// DG_TankProvisionsTextBox
			// 
			this.BindingSource.SetBindingMember(this.DG_TankProvisionsTextBox, "DG_TankProv");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_TankProv)));
			this.DG_TankProvisionsTextBox.CaptionResourceString = null;
			this.DG_TankProvisionsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DG_TankProvisionsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 71, true);
			this.DG_TankProvisionsTextBox.Name = "DG_TankProvisionsTextBox";
			this.DG_TankProvisionsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			this.DG_TankProvisionsTextBox.TabIndex = 2;
			// 
			// DetailsLanguageDropEdit
			// 
			this.DetailsLanguageDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DetailsLanguageDropEdit, "DetailsLanguage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DetailsLanguage)));
			this.DetailsLanguageDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceForm|da962885-31ca-4af9-a6c2-c77d35d6302c", "Language Filter", "Filters the Names, Observations, Properties, Qualifying Descriptive Texts, Special Provisions and Stowage/Segmentation requirements to only show those that are in the language you choose.");
			this.DetailsLanguageDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 439, true);
			this.DetailsLanguageDropEdit.Name = "DetailsLanguageDropEdit";
			this.DetailsLanguageDropEdit.ShouldResizeByMaxLength = true;
			this.DetailsLanguageDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 20, true);
			this.DetailsLanguageDropEdit.TabIndex = 15;
			// 
			// DG_PGDownEdit
			// 
			this.DG_PGDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DG_PGDownEdit, "DG_PG");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_PG)));
			this.DG_PGDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(502, 52, true);
			this.DG_PGDownEdit.Name = "DG_PGDownEdit";
			this.DG_PGDownEdit.PreBoundMaxLength = 4;
			this.DG_PGDownEdit.ShouldResizeByMaxLength = true;
			this.DG_PGDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.DG_PGDownEdit.TabIndex = 3;
			// 
			// DG_ExceptedQuantitiesDropDown
			// 
			this.DG_ExceptedQuantitiesDropDown.AllowDrop = true;
			this.DG_ExceptedQuantitiesDropDown.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.DG_ExceptedQuantitiesDropDown, "DG_ExceptedQuantityCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_ExceptedQuantityCode)));
			this.DG_ExceptedQuantitiesDropDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 274, true);
			this.DG_ExceptedQuantitiesDropDown.Name = "DG_ExceptedQuantitiesDropDown";
			this.DG_ExceptedQuantitiesDropDown.PreBoundMaxLength = 4;
			this.DG_ExceptedQuantitiesDropDown.ShouldResizeByMaxLength = true;
			this.DG_ExceptedQuantitiesDropDown.ShowDescriptionBox = true;
			this.DG_ExceptedQuantitiesDropDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 20, true);
			this.DG_ExceptedQuantitiesDropDown.TabIndex = 11;
			// 
			// HeaderLabel
			// 
			this.HeaderLabel.AutoSize = true;
			this.HeaderLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesForm|328F7D55-000E-42DB-8F1B-09473E714899", "SEA FREIGHT");
			this.HeaderLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Header)));
			this.HeaderLabel.IsFontBold = true;
			this.HeaderLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 20, true);
			this.HeaderLabel.Name = "HeaderLabel";
			this.HeaderLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 30, true);
			this.HeaderLabel.TabIndex = 29;
			// 
			// IconPictureBox
			// 
			this.IconPictureBox.BackColor = System.Drawing.Color.Transparent;
			this.IconPictureBox.Image = Properties.Resources.seafreight;
			this.IconPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.IconPictureBox.Name = "IconPictureBox";
			this.IconPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 40, true);
			this.IconPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.IconPictureBox.TabIndex = 30;
			this.IconPictureBox.TabStop = false;
			// 
			// DG_NOSCheckBox
			// 
			this.DG_NOSCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.DG_NOSCheckBox, "DG_IsNotOtherwiseSpecified");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).DG_IsNotOtherwiseSpecified)));
			this.DG_NOSCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.DG_NOSCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DG_NOSCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 178, true);
			this.DG_NOSCheckBox.Name = "DG_NOSCheckBox";
			this.DG_NOSCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.DG_NOSCheckBox.TabIndex = 6;
			// 
			// UNDGSubstanceControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DG_ExceptedQuantitiesDropDown);
			this.Controls.Add(this.DG_PGDownEdit);
			this.Controls.Add(this.UnderlinedEMSCheckBox);
			this.Controls.Add(this.DG_PointersTextBox);
			this.Controls.Add(this.zTextBox1);
			this.Controls.Add(this.DG_MarkersTextBox);
			this.Controls.Add(this.DetailsLanguageDropEdit);
			this.Controls.Add(this.DG_EMSTextBox);
			this.Controls.Add(this.UNDGSubstanceTabControl);
			this.Controls.Add(this.DG_TreatAsTextBox);
			this.Controls.Add(this.DG_ExplosiveLimitsTextBox);
			this.Controls.Add(this.DG_FlashPointTextBox);
			this.Controls.Add(this.DG_IsSystemCheckBox);
			this.Controls.Add(this.IsActiveCheckBox);
			this.Controls.Add(this.MarinePollutantDropEdit);
			this.Controls.Add(this.LimitedQuantityGroupBox);
			this.Controls.Add(this.DG_StateDropDownEdit);
			this.Controls.Add(this.DG_UNNumberTextBox);
			this.Controls.Add(this.DG_SubLabel1TextBox);
			this.Controls.Add(this.DG_SubLabel2TextBox);
			this.Controls.Add(this.DG_VariantTextBox);
			this.Controls.Add(this.DG_ClassDropDownEdit);
			this.Controls.Add(this.DG_ProperShippingNameTextBox);
			this.Controls.Add(this.DG_VariationTextBox);
			this.Controls.Add(this.HeaderLabel);
			this.Controls.Add(this.IconPictureBox);
			this.Controls.Add(this.DG_TechNameDropDownEdit);
			this.Controls.Add(this.DG_NOSCheckBox);
			this.Controls.Add(this.DG_BorderPanel);
			this.DataSourceAssemblyName = "Enterprise.MasterFiles.Business";
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.UNDGSubstance);
			this.DataSourceTypeName = "Enterprise.MasterFiles.Business.UNDGSubstance";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(922, 620);
			this.Name = "UNDGSubstanceControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.DG_VariationTextBox, 0);
			this.Controls.SetChildIndex(this.DG_ProperShippingNameTextBox, 0);
			this.Controls.SetChildIndex(this.DG_ClassDropDownEdit, 0);
			this.Controls.SetChildIndex(this.DG_VariantTextBox, 0);
			this.Controls.SetChildIndex(this.DG_SubLabel2TextBox, 0);
			this.Controls.SetChildIndex(this.DG_SubLabel1TextBox, 0);
			this.Controls.SetChildIndex(this.DG_UNNumberTextBox, 0);
			this.Controls.SetChildIndex(this.DG_StateDropDownEdit, 0);
			this.Controls.SetChildIndex(this.LimitedQuantityGroupBox, 0);
			this.Controls.SetChildIndex(this.MarinePollutantDropEdit, 0);
			this.Controls.SetChildIndex(this.IsActiveCheckBox, 0);
			this.Controls.SetChildIndex(this.DG_IsSystemCheckBox, 0);
			this.Controls.SetChildIndex(this.DG_FlashPointTextBox, 0);
			this.Controls.SetChildIndex(this.DG_ExplosiveLimitsTextBox, 0);
			this.Controls.SetChildIndex(this.DG_TreatAsTextBox, 0);
			this.Controls.SetChildIndex(this.UNDGSubstanceTabControl, 0);
			this.Controls.SetChildIndex(this.DG_EMSTextBox, 0);
			this.Controls.SetChildIndex(this.DetailsLanguageDropEdit, 0);
			this.Controls.SetChildIndex(this.DG_MarkersTextBox, 0);
			this.Controls.SetChildIndex(this.zTextBox1, 0);
			this.Controls.SetChildIndex(this.DG_PointersTextBox, 0);
			this.Controls.SetChildIndex(this.UnderlinedEMSCheckBox, 0);
			this.Controls.SetChildIndex(this.DG_PGDownEdit, 0);
			this.Controls.SetChildIndex(this.DG_ExceptedQuantitiesDropDown, 0);
			this.Controls.SetChildIndex(this.DG_TechNameDropDownEdit, 0);
			this.Controls.SetChildIndex(this.DG_NOSCheckBox, 0);
			this.Controls.SetChildIndex(this.DG_BorderPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.IconPictureBox)).EndInit();
			this.LimitedQuantityGroupBox.ResumeLayout(false);
			this.LimitedQuantityGroupBox.PerformLayout();
			this.MarinePollutantDropEdit.ResumeLayout(true);
			this.MarinePollutantDropEdit.PerformLayout();
			this.DG_StateDropDownEdit.ResumeLayout(true);
			this.DG_StateDropDownEdit.PerformLayout();
			this.DG_ClassDropDownEdit.ResumeLayout(true);
			this.DG_ClassDropDownEdit.PerformLayout();
			this.UNDGSubstanceTabControl.ResumeLayout(false);
			this.UNDGSubstanceTabControl.PerformLayout();
			this.NamesTabPage.ResumeLayout(false);
			this.NamesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.NamesGrid)).EndInit();
			this.NamesGrid.ResumeLayout(false);
			this.NamesGrid.PerformLayout();
			this.ObservationsTabPage.ResumeLayout(false);
			this.ObservationsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ObservationsGrid)).EndInit();
			this.ObservationsGrid.ResumeLayout(false);
			this.ObservationsGrid.PerformLayout();
			this.PropertiesTabPage.ResumeLayout(false);
			this.PropertiesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PropertiesGrid)).EndInit();
			this.PropertiesGrid.ResumeLayout(false);
			this.PropertiesGrid.PerformLayout();
			this.QualifyingDescriptiveTextTabPage.ResumeLayout(false);
			this.QualifyingDescriptiveTextTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.QualifyingDescriptiveTextGrid)).EndInit();
			this.QualifyingDescriptiveTextGrid.ResumeLayout(false);
			this.QualifyingDescriptiveTextGrid.PerformLayout();
			this.SpecialProvisionsTabPage.ResumeLayout(false);
			this.SpecialProvisionsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SpecialProvisionsModuleButtonGrid.InnerGrid)).EndInit();
			this.SpecialProvisionsModuleButtonGrid.ResumeLayout(true);
			this.SpecialProvisionsModuleButtonGrid.PerformLayout();
			this.StowageSegregationTabPage.ResumeLayout(false);
			this.StowageSegregationTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.StowageSegregationModuleButtonGrid.InnerGrid)).EndInit();
			this.StowageSegregationModuleButtonGrid.ResumeLayout(true);
			this.StowageSegregationModuleButtonGrid.PerformLayout();
			this.AdditionalStowageTabPage.ResumeLayout(false);
			this.AdditionalStowageTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalStowageModuleButtonGrid.InnerGrid)).EndInit();
			this.AdditionalStowageModuleButtonGrid.ResumeLayout(true);
			this.AdditionalStowageModuleButtonGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CountryReferencesModuleButtonGrid.InnerGrid)).EndInit();
			this.CountryReferencesModuleButtonGrid.ResumeLayout(true);
			this.CountryReferencesModuleButtonGrid.PerformLayout();
			this.OtherProvisionsTabPage.ResumeLayout(false);
			this.OtherProvisionsTabPage.PerformLayout();
			this.CountryReferencesTabPage.ResumeLayout(false);
			this.CountryReferencesTabPage.PerformLayout();
			this.DetailsLanguageDropEdit.ResumeLayout(true);
			this.DetailsLanguageDropEdit.PerformLayout();
			this.DG_PGDownEdit.ResumeLayout(true);
			this.DG_PGDownEdit.PerformLayout();
			this.DG_ExceptedQuantitiesDropDown.ResumeLayout(true);
			this.DG_ExceptedQuantitiesDropDown.PerformLayout();
			this.DG_TechNameDropDownEdit.ResumeLayout(true);
			this.DG_TechNameDropDownEdit.PerformLayout();
			this.DG_BorderPanel.ResumeLayout(true);
			this.DG_BorderPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
