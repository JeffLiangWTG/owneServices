using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class UNDGSubstanceADRControl
	{
		ZLabel HeaderLabel;
		KPictureBox IconPictureBox;
		ZCheckBox IsActiveCheckBox;
		ZCheckBox IsSystemDefinedCheckBox;
		ZTextBox ADR_UNNOTextBox;
		ZTextBox ADR_VariantTextBox;
		ZTextBox ADR_ProperShippingNameTextBox;
		ZDropEdit ADR_ClassDropEdit;
		ZTextBox ClassificationCode1TextBox;
		ZTextBox ClassificationCode2TextBox;
		ZDropEdit ADR_PGDropEdit;
		ZTextBox ADR_Label1TextBox;
		ZTextBox ADR_Label2TextBox;
		ZTextBox ADR_Label3TextBox;
		ZTextBox ADR_Label4TextBox;
		ZTextBox ADR_SpecialProvisionsTextBox;
		ZCalcEdit ADR_LQMaxAmtCalcEdit;
		ZTextBox ADR_LQMaxAmtUQTextBox;
		ZCalcEdit ADR_LQ2MaxAmtCalcEdit;
		ZTextBox ADR_LQ2MaxAmtUQTextBox;
		ZDropEdit ADR_ExceptedQuantityCodeDropEdit;
		ZTextBox ADR_PackInsTextBox;
		ZTextBox ADR_PackProvTextBox;
		ZTextBox ADR_MixedPackingProvTextBox;
		ZTextBox ADR_BulkTankInsTextBox;
		ZTextBox ADR_BulkTankSpecProv;
		ZTextBox ADR_ADRTankCodeTextBox;
		ZTextBox ADR_ADRTankSpecProv;
		ZTextBox ADR_TankVehicleTextBox;
		ZTextBox ADR_TransportCategoryTextBox;
		ZTextBox ADR_PackingSpecialProvTextBox;
		ZTextBox ADR_BulkSpecialProvTextBox;
		ZTextBox ADR_LoadingSpecialProvTextBox;
		ZTextBox ADR_OperationSpecialProvTextBox;
		ZTextBox ADR_HazardIDNumberTextBox;
		ZPanel DG_BorderPanel;
		ZTemplateTabControl UNDGSubstanceTabControl;
		ZTabPage NamesTabPage;
		ZTabPage QualifyingDescriptiveTextTabPage;
		ZTabPage OtherProvisionsTabPage;
		ZGrid NamesGrid;
		ZGrid QualifyingDescriptiveTextGrid;
		ZDropEdit DetailsLanguageDropEdit;
		private System.ComponentModel.IContainer components;
		ZGroupBox PackingProvisionsGroupBox;
		Enterprise.ZArchitecture.GUI.ZTabPage CountryReferencesTabPage;
		ZModuleButtonGrid CountryReferencesModuleButtonGrid;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo nameLanguageColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo nameDescriptorColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo qualifyingLanguageColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo qualifyingDescriptorColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();

			this.components = new System.ComponentModel.Container();
			this.UNDGSubstanceTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.NamesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.QualifyingDescriptiveTextTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.OtherProvisionsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ADR_BulkTankInsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADR_BulkTankSpecProv = new Enterprise.ZArchitecture.ZTextBox();
			this.ADR_ADRTankCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADR_ADRTankSpecProv = new Enterprise.ZArchitecture.ZTextBox();
			this.ADR_TankVehicleTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADR_BulkSpecialProvTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADR_LoadingSpecialProvTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADR_OperationSpecialProvTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PackingProvisionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ADR_SpecialProvisionsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADR_PackInsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADR_PackProvTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADR_MixedPackingProvTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADR_PackingSpecialProvTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HeaderLabel = new Enterprise.ZArchitecture.ZLabel();
			this.IconPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsSystemDefinedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ADR_UNNOTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADR_VariantTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADR_ProperShippingNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADR_ClassDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ClassificationCode1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ClassificationCode2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADR_PGDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ADR_Label1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADR_Label2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADR_Label3TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADR_Label4TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADR_LQMaxAmtCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ADR_LQMaxAmtUQTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADR_LQ2MaxAmtCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ADR_LQ2MaxAmtUQTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADR_ExceptedQuantityCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ADR_TransportCategoryTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADR_HazardIDNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DetailsLanguageDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NamesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.QualifyingDescriptiveTextGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CountryReferencesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CountryReferencesModuleButtonGrid = new CountryReferencesModuleButtonGrid();
			this.DG_BorderPanel = new ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.NamesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.NamesGrid)).BeginInit();
			this.NamesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.QualifyingDescriptiveTextGrid)).BeginInit();
			this.QualifyingDescriptiveTextGrid.SuspendLayout();
			this.UNDGSubstanceTabControl.SuspendLayout();
			this.OtherProvisionsTabPage.SuspendLayout();
			this.PackingProvisionsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.IconPictureBox)).BeginInit();
			this.ADR_ClassDropEdit.SuspendLayout();
			this.ADR_PGDropEdit.SuspendLayout();
			this.ADR_ExceptedQuantityCodeDropEdit.SuspendLayout();
			this.DetailsLanguageDropEdit.SuspendLayout();
			this.CountryReferencesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CountryReferencesModuleButtonGrid.InnerGrid)).BeginInit();
			this.CountryReferencesModuleButtonGrid.SuspendLayout();
			this.DG_BorderPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(UNDGSubstanceADR);
			// 
			// UNDGSubstanceTabControl
			// 
			this.UNDGSubstanceTabControl.Controls.Add(this.NamesTabPage);
			this.UNDGSubstanceTabControl.Controls.Add(this.QualifyingDescriptiveTextTabPage);
			this.UNDGSubstanceTabControl.Controls.Add(this.OtherProvisionsTabPage);
			this.UNDGSubstanceTabControl.Controls.Add(this.CountryReferencesTabPage);
			this.UNDGSubstanceTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(50, 345, true);
			this.UNDGSubstanceTabControl.Name = "UNDGSubstanceTabControl";
			this.UNDGSubstanceTabControl.SelectedIndex = 0;
			this.UNDGSubstanceTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(806, 226, true);
			this.UNDGSubstanceTabControl.TabIndex = 40;
			// 
			// NamesTabPage
			// 
			this.NamesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADRForm|be0d3486-d619-ab83-44d8-e796d97a6e23", "Names");
			this.NamesTabPage.Controls.Add(this.NamesGrid);
			this.NamesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.NamesTabPage.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(545, 175, true);
			this.NamesTabPage.Name = "NamesTabPage";
			this.NamesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(799, 203, true);
			this.NamesTabPage.TabIndex = 1;
			// 
			// NamesGrid
			// 
			this.NamesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.NamesGrid, "Names");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).Names)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGAttributeZZ)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).Names)).SyncRoot)).DAZ_Language)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGAttributeZZ)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).Names)).SyncRoot)).DAZ_Descriptor)));
			this.NamesGrid.CaptionVisible = false;
			nameLanguageColumnStyleInfo.ColumnName = "DAZ_Language";
			nameLanguageColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			nameLanguageColumnStyleInfo.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADRForm|cd892cd1-631a-89a3-4b33-cb722dab0d2b", "Lang", "Language", "The Language of this detail.");
			nameDescriptorColumnStyleInfo.ColumnName = "DAZ_Descriptor";
			nameDescriptorColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(800);
			nameDescriptorColumnStyleInfo.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADRForm|5b719e01-df95-3681-48c9-5f83ce974550", "Detail");
			this.NamesGrid.ColumnStyles.Add(nameLanguageColumnStyleInfo);
			this.NamesGrid.ColumnStyles.Add(nameDescriptorColumnStyleInfo);
			this.NamesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NamesGrid.GridId = "00115742-3f0c-7599-4b71-cd3cdb803d73";
			this.NamesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.NamesGrid.LayoutKey = "zGrid3";
			this.NamesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NamesGrid.Name = "NamesGrid";
			this.NamesGrid.ReadOnly = true;
			this.NamesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(798, 177, true);
			this.NamesGrid.TabIndex = 1;
			// 
			// DG_BorderPanel
			//
			this.DG_BorderPanel.Name = "DG_BorderPanel";
			this.DG_BorderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 1, true);
			this.DG_BorderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 40, true);
			this.DG_BorderPanel.BackColor = Color.DarkGray;
			// 
			// QualifyingDescriptiveTextTabPage
			// 
			this.QualifyingDescriptiveTextTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADRForm|a428f676-ee4b-1d9f-451f-4a31d1718b50", "Qualifying Descriptive Text");
			this.QualifyingDescriptiveTextTabPage.Controls.Add(this.QualifyingDescriptiveTextGrid);
			this.QualifyingDescriptiveTextTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.QualifyingDescriptiveTextTabPage.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(545, 175, true);
			this.QualifyingDescriptiveTextTabPage.Name = "QualifyingDescriptiveTextTabPage";
			this.QualifyingDescriptiveTextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(799, 203, true);
			this.QualifyingDescriptiveTextTabPage.TabIndex = 2;
			// 
			// QualifyingDescriptiveTextGrid
			// 
			this.QualifyingDescriptiveTextGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.QualifyingDescriptiveTextGrid, "QualifyingDescriptiveTexts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).QualifyingDescriptiveTexts)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGAttributeZZ)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).QualifyingDescriptiveTexts)).SyncRoot)).DAZ_Language)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGAttributeZZ)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).QualifyingDescriptiveTexts)).SyncRoot)).DAZ_Descriptor)));
			this.QualifyingDescriptiveTextGrid.CaptionVisible = false;
			qualifyingLanguageColumnStyleInfo.ColumnName = "DAZ_Language";
			qualifyingLanguageColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			qualifyingLanguageColumnStyleInfo.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADRForm|cd892cd1-631a-89a3-4b33-cb722dab0d2b", "Lang", "Language", "The Language of this detail.");
			qualifyingDescriptorColumnStyleInfo.ColumnName = "DAZ_Descriptor";
			qualifyingDescriptorColumnStyleInfo.MinimumEditControlWidth = 300;
			qualifyingDescriptorColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(800);
			qualifyingDescriptorColumnStyleInfo.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADRForm|5b719e01-df95-3681-48c9-5f83ce974550", "Detail");
			this.QualifyingDescriptiveTextGrid.ColumnStyles.Add(qualifyingLanguageColumnStyleInfo);
			this.QualifyingDescriptiveTextGrid.ColumnStyles.Add(qualifyingDescriptorColumnStyleInfo);
			this.QualifyingDescriptiveTextGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.QualifyingDescriptiveTextGrid.GridId = "9b9bdb6c-62f2-6c98-4a81-bebf16a402eb";
			this.QualifyingDescriptiveTextGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.QualifyingDescriptiveTextGrid.LayoutKey = "zGrid2";
			this.QualifyingDescriptiveTextGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.QualifyingDescriptiveTextGrid.Name = "QualifyingDescriptiveTextGrid";
			this.QualifyingDescriptiveTextGrid.ReadOnly = true;
			this.QualifyingDescriptiveTextGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(905, 183, true);
			this.QualifyingDescriptiveTextGrid.TabIndex = 0;
			// 
			// OtherProvisionsTabPage
			// 
			this.OtherProvisionsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADRForm|29993b0a-3257-b1a7-44ad-e9c344a7651d", "Other Provisions");
			this.OtherProvisionsTabPage.Controls.Add(this.ADR_BulkTankInsTextBox);
			this.OtherProvisionsTabPage.Controls.Add(this.ADR_BulkTankSpecProv);
			this.OtherProvisionsTabPage.Controls.Add(this.ADR_ADRTankCodeTextBox);
			this.OtherProvisionsTabPage.Controls.Add(this.ADR_ADRTankSpecProv);
			this.OtherProvisionsTabPage.Controls.Add(this.ADR_TankVehicleTextBox);
			this.OtherProvisionsTabPage.Controls.Add(this.ADR_BulkSpecialProvTextBox);
			this.OtherProvisionsTabPage.Controls.Add(this.ADR_LoadingSpecialProvTextBox);
			this.OtherProvisionsTabPage.Controls.Add(this.ADR_OperationSpecialProvTextBox);
			this.OtherProvisionsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.OtherProvisionsTabPage.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(545, 175, true);
			this.OtherProvisionsTabPage.Name = "OtherProvisionsTabPage";
			this.OtherProvisionsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.OtherProvisionsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(799, 203, true);
			this.OtherProvisionsTabPage.TabIndex = 3;
			// 
			// DetailsLanguageDropEdit
			// 
			this.DetailsLanguageDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DetailsLanguageDropEdit, "DetailsLanguage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).DetailsLanguage)));
			this.DetailsLanguageDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADRForm|b5bb3569-d855-97a1-4ad7-8ac4c8d0080c", "Language Filter", "Filters the Names, Qualifying Descriptive Texts, Special Provisions and Stowage/Segmentation requirements to only show those that are in the language you choose.");
			this.DetailsLanguageDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 286, true);
			this.DetailsLanguageDropEdit.Name = "DetailsLanguageDropEdit";
			this.DetailsLanguageDropEdit.ShouldResizeByMaxLength = true;
			this.DetailsLanguageDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.DetailsLanguageDropEdit.TabIndex = 39;
			// 
			// ADR_BulkTankInsTextBox
			// 
			this.BindingSource.SetBindingMember(this.ADR_BulkTankInsTextBox, "ADR_BulkTankIns");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).ADR_BulkTankIns)));
			this.ADR_BulkTankInsTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADRForm|18b30a2a-f0f4-6fb5-4006-5e5410c5919f", "Bulk Container Instructions", "The Portable Tank/Bulk Container Instructions.");
			this.ADR_BulkTankInsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ADR_BulkTankInsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(406, 26, true);
			this.ADR_BulkTankInsTextBox.Name = "ADR_BulkTankInsTextBox";
			this.ADR_BulkTankInsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 18, true);
			this.ADR_BulkTankInsTextBox.TabIndex = 2;
			this.ADR_BulkTankInsTextBox.ReadOnly = true;
			// 
			// ADR_BulkTankSpecProv
			// 
			this.BindingSource.SetBindingMember(this.ADR_BulkTankSpecProv, "ADR_BulkTankSpecProv");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).ADR_BulkTankSpecProv)));
			this.ADR_BulkTankSpecProv.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADRForm|9756f784-7e32-59b1-4e50-3337d2554a63", "Bulk Container Special Prov.", "The Portable Tank/Bulk Container Special Provisions.");
			this.ADR_BulkTankSpecProv.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ADR_BulkTankSpecProv.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(406, 52, true);
			this.ADR_BulkTankSpecProv.Name = "ADR_BulkTankSpecProv";
			this.ADR_BulkTankSpecProv.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 18, true);
			this.ADR_BulkTankSpecProv.TabIndex = 6;
			this.ADR_BulkTankSpecProv.ReadOnly = true;
			// 
			// ADR_ADRTankCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ADR_ADRTankCodeTextBox, "ADR_ADRTankCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).ADR_PackProv)));
			this.ADR_ADRTankCodeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADRForm|b61594ef-5eb8-51b8-4ca0-f028a95e7d84", "ADR Tank Code");
			this.ADR_ADRTankCodeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ADR_ADRTankCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 104, true);
			this.ADR_ADRTankCodeTextBox.Name = "ADR_ADRTankCodeTextBox";
			this.ADR_ADRTankCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(398, 18, true);
			this.ADR_ADRTankCodeTextBox.TabIndex = 5;
			this.ADR_ADRTankCodeTextBox.ReadOnly = true;
			// 
			// ADR_ADRTankSpecProv
			// 
			this.BindingSource.SetBindingMember(this.ADR_ADRTankSpecProv, "ADR_ADRTankSpecProv");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).ADR_ADRTankSpecProv)));
			this.ADR_ADRTankSpecProv.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADRForm|7e84eaa3-e4dc-a799-4958-a9c3910128b7", "ADR Tank Special Prov.", "ADR Tank Special Provisions", "The Special Provisions assigned to this substance concerning carriage in ADR Tanks.");
			this.ADR_ADRTankSpecProv.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ADR_ADRTankSpecProv.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 78, true);
			this.ADR_ADRTankSpecProv.Name = "ADR_ADRTankSpecProv";
			this.ADR_ADRTankSpecProv.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(398, 18, true);
			this.ADR_ADRTankSpecProv.TabIndex = 8;
			this.ADR_ADRTankSpecProv.ReadOnly = true;
			// 
			// ADR_TankVehicleTextBox
			// 
			this.BindingSource.SetBindingMember(this.ADR_TankVehicleTextBox, "ADR_TankVehicle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).ADR_TankVehicle)));
			this.ADR_TankVehicleTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADRForm|a1bd81bc-a90a-3cad-4f6e-f45cc0241dcb", "Vehicle for Tank Carriage");
			this.ADR_TankVehicleTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ADR_TankVehicleTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 26, true);
			this.ADR_TankVehicleTextBox.Name = "ADR_TankVehicleTextBox";
			this.ADR_TankVehicleTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 18, true);
			this.ADR_TankVehicleTextBox.TabIndex = 1;
			this.ADR_TankVehicleTextBox.ReadOnly = true;
			// 
			// ADR_BulkSpecialProvTextBox
			// 
			this.BindingSource.SetBindingMember(this.ADR_BulkSpecialProvTextBox, "ADR_BulkSpecialProv");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).ADR_BulkSpecialProv)));
			this.ADR_BulkSpecialProvTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADRForm|00c9b220-78ec-8d8b-4ecc-0e73ceddd64e", "Bulk Special Prov.", "Bulk Special Provisions", "The Special Provisions assigned to this substance concerning carriage in bulk.");
			this.ADR_BulkSpecialProvTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ADR_BulkSpecialProvTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(653, 26, true);
			this.ADR_BulkSpecialProvTextBox.Name = "ADR_BulkSpecialProvTextBox";
			this.ADR_BulkSpecialProvTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 18, true);
			this.ADR_BulkSpecialProvTextBox.TabIndex = 3;
			this.ADR_BulkSpecialProvTextBox.ReadOnly = true;
			// 
			// ADR_LoadingSpecialProvTextBox
			// 
			this.BindingSource.SetBindingMember(this.ADR_LoadingSpecialProvTextBox, "ADR_LoadingSpecialProv");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).ADR_LoadingSpecialProv)));
			this.ADR_LoadingSpecialProvTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADRForm|3d7e970d-45a4-4699-400d-4ed2085e9fb2", "Loading Special Prov.", "Loading Special Provisions", "The Special Provisions assigned to this substance concerning loading.");
			this.ADR_LoadingSpecialProvTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ADR_LoadingSpecialProvTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(653, 52, true);
			this.ADR_LoadingSpecialProvTextBox.Name = "ADR_LoadingSpecialProvTextBox";
			this.ADR_LoadingSpecialProvTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 18, true);
			this.ADR_LoadingSpecialProvTextBox.TabIndex = 7;
			this.ADR_LoadingSpecialProvTextBox.ReadOnly = true;
			// 
			// ADR_OperationSpecialProvTextBox
			// 
			this.BindingSource.SetBindingMember(this.ADR_OperationSpecialProvTextBox, "ADR_OperationSpecialProv");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).ADR_OperationSpecialProv)));
			this.ADR_OperationSpecialProvTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADRForm|231b4c9e-e1f6-48a2-4147-9ddd22cbda6a", "Operation Special Prov.", "Operation Special Provisions", "The Special Provisions assigned to this substance concerning operations.");
			this.ADR_OperationSpecialProvTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ADR_OperationSpecialProvTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 52, true);
			this.ADR_OperationSpecialProvTextBox.Name = "ADR_OperationSpecialProvTextBox";
			this.ADR_OperationSpecialProvTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 18, true);
			this.ADR_OperationSpecialProvTextBox.TabIndex = 4;
			this.ADR_OperationSpecialProvTextBox.ReadOnly = true;
			// 
			// PackingProvisionsGroupBox
			// 
			this.PackingProvisionsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADRForm|9e1c2e47-a70c-70bd-485c-cb7b4f01ddf9", "Packing Provisions");
			this.PackingProvisionsGroupBox.Controls.Add(this.ADR_SpecialProvisionsTextBox);
			this.PackingProvisionsGroupBox.Controls.Add(this.ADR_PackInsTextBox);
			this.PackingProvisionsGroupBox.Controls.Add(this.ADR_PackProvTextBox);
			this.PackingProvisionsGroupBox.Controls.Add(this.ADR_MixedPackingProvTextBox);
			this.PackingProvisionsGroupBox.Controls.Add(this.ADR_PackingSpecialProvTextBox);
			this.PackingProvisionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(440, 100, true);
			this.PackingProvisionsGroupBox.Name = "PackingProvisionsGroupBox";
			this.PackingProvisionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 154, true);
			this.PackingProvisionsGroupBox.TabIndex = 30;
			this.PackingProvisionsGroupBox.TabStop = false;
			// 
			// ADR_SpecialProvisionsTextBox
			// 
			this.ADR_SpecialProvisionsTextBox.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left;
			this.BindingSource.SetBindingMember(this.ADR_SpecialProvisionsTextBox, "ADR_SpecialProvisions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).ADR_SpecialProvisions)));
			this.ADR_SpecialProvisionsTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADRForm|323ee623-be9c-009c-45a5-6ee1f0d755ce", "Special Provisions");
			this.ADR_SpecialProvisionsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ADR_SpecialProvisionsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 26, true);
			this.ADR_SpecialProvisionsTextBox.Name = "ADR_SpecialProvisionsTextBox";
			this.ADR_SpecialProvisionsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 18, true);
			this.ADR_SpecialProvisionsTextBox.TabIndex = 6;
			this.ADR_SpecialProvisionsTextBox.ReadOnly = true;
			// 
			// ADR_PackInsTextBox
			// 
			this.BindingSource.SetBindingMember(this.ADR_PackInsTextBox, "ADR_PackIns");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).ADR_PackIns)));
			this.ADR_PackInsTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADRForm|e5c31c13-929f-c6a4-40a0-a4dbb51d9bbd", "Packing Instruction", "The packing instruction code that relates to the transportation of this substance in limited quantities.");
			this.ADR_PackInsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ADR_PackInsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 130, true);
			this.ADR_PackInsTextBox.Name = "ADR_PackInsTextBox";
			this.ADR_PackInsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 18, true);
			this.ADR_PackInsTextBox.TabIndex = 34;
			this.ADR_PackInsTextBox.ReadOnly = true;
			// 
			// ADR_PackProvTextBox
			// 
			this.BindingSource.SetBindingMember(this.ADR_PackProvTextBox, "ADR_PackProv");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).ADR_PackProv)));
			this.ADR_PackProvTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADRForm|da7fa671-857e-fca4-473b-49011049e214", "Pack Prov.", "ADR Packing Provisions", "Packing Provisions");
			this.ADR_PackProvTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ADR_PackProvTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 52, true);
			this.ADR_PackProvTextBox.Name = "ADR_PackProvTextBox";
			this.ADR_PackProvTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 18, true);
			this.ADR_PackProvTextBox.TabIndex = 31;
			this.ADR_PackProvTextBox.ReadOnly = true;
			// 
			// ADR_MixedPackingProvTextBox
			// 
			this.BindingSource.SetBindingMember(this.ADR_MixedPackingProvTextBox, "ADR_MixedPackingProv");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).ADR_MixedPackingProv)));
			this.ADR_MixedPackingProvTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADRForm|448fa3fc-3e8f-75b0-4fac-98f847749d10", "Mixed Packing Provisions");
			this.ADR_MixedPackingProvTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ADR_MixedPackingProvTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 78, true);
			this.ADR_MixedPackingProvTextBox.Name = "ADR_MixedPackingProvTextBox";
			this.ADR_MixedPackingProvTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 18, true);
			this.ADR_MixedPackingProvTextBox.TabIndex = 32;
			this.ADR_MixedPackingProvTextBox.ReadOnly = true;
			// 
			// ADR_PackingSpecialProvTextBox
			// 
			this.BindingSource.SetBindingMember(this.ADR_PackingSpecialProvTextBox, "ADR_PackingSpecialProv");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).ADR_PackingSpecialProv)));
			this.ADR_PackingSpecialProvTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADRForm|ca6f73e7-dfc9-be9a-44c6-c258f2786f7d", "Packing Special Provisions");
			this.ADR_PackingSpecialProvTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ADR_PackingSpecialProvTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 104, true);
			this.ADR_PackingSpecialProvTextBox.Name = "ADR_PackingSpecialProvTextBox";
			this.ADR_PackingSpecialProvTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 18, true);
			this.ADR_PackingSpecialProvTextBox.TabIndex = 33;
			this.ADR_PackingSpecialProvTextBox.ReadOnly = true;
			// 
			// HeaderLabel
			// 
			this.HeaderLabel.AutoSize = true;
			this.HeaderLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADRForm|94b47f83-ded2-ac9c-49eb-de8162d9a515", "ROAD FREIGHT (ADR)");
			this.HeaderLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.HeaderLabel.IsFontBold = true;
			this.HeaderLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 20, true);
			this.HeaderLabel.Name = "HeaderLabel";
			this.HeaderLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 14, true);
			this.HeaderLabel.TabIndex = 17;
			// 
			// IconPictureBox
			// 
			this.IconPictureBox.BackColor = System.Drawing.Color.Transparent;
			this.IconPictureBox.Image = global::Enterprise.MasterFiles.GUI.Properties.Resources.roadfreight;
			this.IconPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 5, true);
			this.IconPictureBox.Name = "IconPictureBox";
			this.IconPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 40, true);
			this.IconPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.IconPictureBox.TabIndex = 18;
			this.IconPictureBox.TabStop = false;
			// 
			// IsActiveCheckBox
			// 
			this.IsActiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsActiveCheckBox, "ADR_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).ADR_IsActive)));
			this.IsActiveCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADRForm|b8e2553c-4e8c-cbb7-4814-5cc49f9c1cd7", "Active");
			this.IsActiveCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(862, 55, true);
			this.IsActiveCheckBox.Name = "IsActiveCheckBox";
			this.IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 14, true);
			this.IsActiveCheckBox.TabIndex = 9;
			this.IsActiveCheckBox.ReadOnly = true;
			// 
			// IsSystemDefinedCheckBox
			// 
			this.IsSystemDefinedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsSystemDefinedCheckBox, "ADR_IsSystemDefined");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).ADR_IsSystemDefined)));
			this.IsSystemDefinedCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADRForm|77091267-d6a6-0ea2-4606-30d0889110c6", "System Defined");
			this.IsSystemDefinedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsSystemDefinedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsSystemDefinedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(805, 55, true);
			this.IsSystemDefinedCheckBox.Name = "IsSystemDefinedCheckBox";
			this.IsSystemDefinedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 14, true);
			this.IsSystemDefinedCheckBox.TabIndex = 8;
			this.IsSystemDefinedCheckBox.ReadOnly = true;
			// 
			// ADR_UNNOTextBox
			// 
			this.BindingSource.SetBindingMember(this.ADR_UNNOTextBox, "ADR_UNNO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).ADR_UNNO)));
			this.ADR_UNNOTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADRForm|87b58265-e921-52ac-4642-7d8fe1a610ca", "UN No.", "UN No.", "The United Nations (UN) number for this substance.");
			this.ADR_UNNOTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 52, true);
			this.ADR_UNNOTextBox.Name = "ADR_UNNOTextBox";
			this.ADR_UNNOTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 18, true);
			this.ADR_UNNOTextBox.TabIndex = 0;
			this.ADR_UNNOTextBox.ReadOnly = true;
			// 
			// ADR_VariantTextBox
			// 
			this.BindingSource.SetBindingMember(this.ADR_VariantTextBox, "ADR_Variant");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).ADR_Variant)));
			this.ADR_VariantTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADRForm|5c72a9d2-f8c8-1aaf-4bdb-78709fc0608f", "Variant");
			this.ADR_VariantTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 52, true);
			this.ADR_VariantTextBox.Name = "ADR_VariantTextBox";
			this.ADR_VariantTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 18, true);
			this.ADR_VariantTextBox.TabIndex = 1;
			this.ADR_VariantTextBox.ReadOnly = true;
			// 
			// ADR_ProperShippingNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ADR_ProperShippingNameTextBox, "ADR_PSN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).ADR_PSN)));
			this.ADR_ProperShippingNameTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADRForm|e8ee8599-07fe-b8b2-4ef4-7ce59fc3bea9", "PSN", "Proper Shipping Name", "The Proper Shipping Name for this substance.");
			this.ADR_ProperShippingNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ADR_ProperShippingNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 78, true);
			this.ADR_ProperShippingNameTextBox.Name = "ADR_ProperShippingNameTextBox";
			this.ADR_ProperShippingNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 18, true);
			this.ADR_ProperShippingNameTextBox.TabIndex = 10;
			this.ADR_ProperShippingNameTextBox.ReadOnly = true;
			// 
			// ADR_ClassDropEdit
			// 
			this.ADR_ClassDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ADR_ClassDropEdit, "ADR_Class");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).ADR_Class)));
			this.ADR_ClassDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADRForm|b711ba2f-30c3-b297-4ea1-b37d5a20ff1f", "Class", "The ADR Class number assigned to this substance.");
			this.ADR_ClassDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(290, 52, true);
			this.ADR_ClassDropEdit.Name = "ADR_ClassDropEdit";
			this.ADR_ClassDropEdit.PreBoundMaxLength = 4;
			this.ADR_ClassDropEdit.ShouldResizeByMaxLength = true;
			this.ADR_ClassDropEdit.ShowDescriptionBox = false;
			this.ADR_ClassDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 18, true);
			this.ADR_ClassDropEdit.TabIndex = 2;
			this.ADR_ClassDropEdit.ReadOnly = true;
			// 
			// ClassificationCode1TextBox
			// 
			this.BindingSource.SetBindingMember(this.ClassificationCode1TextBox, "ClassificationCode1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).ADR_ClassificationCode)));
			this.ClassificationCode1TextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADRForm|854e8a61-2d1e-abaa-4642-c660cfc041c7", "Class. Code", "The Classification Code assigned to this substance.");
			this.ClassificationCode1TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ClassificationCode1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(425, 52, true);
			this.ClassificationCode1TextBox.Name = "ClassificationCode1TextBox";
			this.ClassificationCode1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 18, true);
			this.ClassificationCode1TextBox.TabIndex = 3;
			this.ClassificationCode1TextBox.ReadOnly = true;
			// 
			// ClassificationCode2TextBox
			// 
			this.BindingSource.SetBindingMember(this.ClassificationCode2TextBox, "ClassificationCode2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).ClassificationCode2)));
			this.ClassificationCode2TextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADRForm|643a3dca-8f77-4132-b9d6-fb683d9e64a9", "Class. Code 2");
			this.ClassificationCode2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ClassificationCode2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(457, 52, true);
			this.ClassificationCode2TextBox.Name = "ClassificationCode2TextBox";
			this.ClassificationCode2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 18, true);
			this.ClassificationCode2TextBox.TabIndex = 4;
			this.ClassificationCode2TextBox.ReadOnly = true;
			// 
			// ADR_PGDropEdit
			// 
			this.ADR_PGDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ADR_PGDropEdit, "ADR_PG");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).ADR_PG)));
			this.ADR_PGDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADRForm|f64035ba-df57-dcac-416f-0bb89fde0890", "PG", "Packing Group", "The UN Packing Group (either I, II or III).");
			this.ADR_PGDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(575, 52, true);
			this.ADR_PGDropEdit.Name = "ADR_PGDropEdit";
			this.ADR_PGDropEdit.PreBoundMaxLength = 4;
			this.ADR_PGDropEdit.ShouldResizeByMaxLength = true;
			this.ADR_PGDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 18, true);
			this.ADR_PGDropEdit.TabIndex = 5;
			this.ADR_PGDropEdit.ReadOnly = true;
			// 
			// ADR_LabelsTextBox
			// 
			this.ADR_Label1TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.ADR_Label1TextBox, "ADR_Label1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).ADR_Label1)));
			this.ADR_Label1TextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADRForm|3f41f5c7-2067-e8a3-43e9-cc7ecdf88be8", "Label(s)", "The subsidiary hazard label(s) assigned to this substance.");
			this.ADR_Label1TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ADR_Label1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 130, true);
			this.ADR_Label1TextBox.Name = "ADR_Label1TextBox";
			this.ADR_Label1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 18, true);
			this.ADR_Label1TextBox.TabIndex = 11;
			this.ADR_Label1TextBox.ReadOnly = true;
			// 
			// ADR_LabelsTextBox
			// 
			this.ADR_Label2TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.ADR_Label2TextBox, "ADR_Label2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).ADR_Label2)));
			this.ADR_Label2TextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ADR_Label2TextBox, false);
			this.ADR_Label2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ADR_Label2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 130, true);
			this.ADR_Label2TextBox.Name = "ADR_Label2TextBox";
			this.ADR_Label2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 18, true);
			this.ADR_Label2TextBox.TabIndex = 12;
			this.ADR_Label2TextBox.ReadOnly = true;
			// 
			// ADR_LabelsTextBox
			// 
			this.ADR_Label3TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.ADR_Label3TextBox, "ADR_Label3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).ADR_Label3)));
			this.ADR_Label3TextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ADR_Label3TextBox, false);
			this.ADR_Label3TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ADR_Label3TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 130, true);
			this.ADR_Label3TextBox.Name = "ADR_Label3TextBox";
			this.ADR_Label3TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 18, true);
			this.ADR_Label3TextBox.TabIndex = 13;
			this.ADR_Label3TextBox.ReadOnly = true;
			// 
			// ADR_LabelsTextBox
			// 
			this.ADR_Label4TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.ADR_Label4TextBox, "ADR_Label4");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).ADR_Label4)));
			this.ADR_Label4TextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ADR_Label4TextBox, false);
			this.ADR_Label4TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ADR_Label4TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 130, true);
			this.ADR_Label4TextBox.Name = "ADR_Label4TextBox";
			this.ADR_Label4TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 18, true);
			this.ADR_Label4TextBox.TabIndex = 14;
			this.ADR_Label4TextBox.ReadOnly = true;
			// 
			// ADR_LQMaxAmtCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ADR_LQMaxAmtCalcEdit, "ADR_LQMaxAmt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).ADR_LQMaxAmt)));
			this.ADR_LQMaxAmtCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADRForm|408145a6-4e66-74b8-4857-1d566be6a2e1", "Limited Quantity", "The Limited Quantity Code assigned to this substance.");
			this.ADR_LQMaxAmtCalcEdit.DecimalPlaces = 2;
			this.ADR_LQMaxAmtCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 156, true);
			this.ADR_LQMaxAmtCalcEdit.Name = "ADR_LQMaxAmtCalcEdit";
			this.ADR_LQMaxAmtCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 18, true);
			this.ADR_LQMaxAmtCalcEdit.TabIndex = 15;
			this.ADR_LQMaxAmtCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ADR_LQMaxAmtCalcEdit.ReadOnly = true;
			// 
			// ADR_LQMaxAmtUQTextBox
			// 
			this.BindingSource.SetBindingMember(this.ADR_LQMaxAmtUQTextBox, "ADR_LQMaxAmtUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).ADR_LQMaxAmtUQ)));
			this.ADR_LQMaxAmtUQTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADRForm|6805bd73-7a40-138d-4a3d-77a77a1ed21f", "Units");
			this.ADR_LQMaxAmtUQTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ADR_LQMaxAmtUQTextBox, false);
			this.ADR_LQMaxAmtUQTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 156, true);
			this.ADR_LQMaxAmtUQTextBox.Name = "ADR_LQMaxAmtUQTextBox";
			this.ADR_LQMaxAmtUQTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 18, true);
			this.ADR_LQMaxAmtUQTextBox.TabIndex = 16;
			this.ADR_LQMaxAmtUQTextBox.ReadOnly = true;
			// 
			// ADR_LQ2MaxAmtCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ADR_LQ2MaxAmtCalcEdit, "ADR_LQ2MaxAmt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).ADR_LQ2MaxAmt)));
			this.ADR_LQ2MaxAmtCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADRForm|36995919-8b18-a3ba-4efd-aefc7f8646d3", "Limited Quantity 2", "The Limited Quantity Code assigned to this substance.");
			this.ADR_LQ2MaxAmtCalcEdit.DecimalPlaces = 2;
			this.ADR_LQ2MaxAmtCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 182, true);
			this.ADR_LQ2MaxAmtCalcEdit.Name = "ADR_LQ2MaxAmtCalcEdit";
			this.ADR_LQ2MaxAmtCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 18, true);
			this.ADR_LQ2MaxAmtCalcEdit.TabIndex = 17;
			this.ADR_LQ2MaxAmtCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ADR_LQ2MaxAmtCalcEdit.ReadOnly = true;
			// 
			// ADR_LQ2MaxAmtUQTextBox
			// 
			this.BindingSource.SetBindingMember(this.ADR_LQ2MaxAmtUQTextBox, "ADR_LQ2MaxAmtUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).ADR_LQ2MaxAmtUQ)));
			this.ADR_LQ2MaxAmtUQTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADRForm|87c346a6-29f8-7a91-45ac-052552c82ce4", "Units");
			this.ADR_LQ2MaxAmtUQTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ADR_LQ2MaxAmtUQTextBox, false);
			this.ADR_LQ2MaxAmtUQTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 182, true);
			this.ADR_LQ2MaxAmtUQTextBox.Name = "ADR_LQ2MaxAmtUQTextBox";
			this.ADR_LQ2MaxAmtUQTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 18, true);
			this.ADR_LQ2MaxAmtUQTextBox.TabIndex = 18;
			this.ADR_LQ2MaxAmtUQTextBox.ReadOnly = true;
			// 
			// ADR_HazardIDNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ADR_HazardIDNumberTextBox, "ADR_HazardIDNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).ADR_HazardIDNumber)));
			this.ADR_HazardIDNumberTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADRForm|0981ac5c-3b1f-9497-40b0-ec00e8b7c564", "Hazard ID Number", "The Hazard Identification Number identifies the substance's hazard qualities.");
			this.ADR_HazardIDNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ADR_HazardIDNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 208, true);
			this.ADR_HazardIDNumberTextBox.Name = "ADR_HazardIDNumberTextBox";
			this.ADR_HazardIDNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 18, true);
			this.ADR_HazardIDNumberTextBox.TabIndex = 19;
			this.ADR_HazardIDNumberTextBox.ReadOnly = true;
			// 
			// ADR_TransportCategoryTextBox
			// 
			this.BindingSource.SetBindingMember(this.ADR_TransportCategoryTextBox, "ADR_TransportCategory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).ADR_TransportCategory)));
			this.ADR_TransportCategoryTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADRForm|46ad71b4-a34b-5eb0-43bc-a211aeb84b8f", "Tunnel Code", "The Transport Category (Tunnel Restriction) code assigned to this substance.");
			this.ADR_TransportCategoryTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ADR_TransportCategoryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 234, true);
			this.ADR_TransportCategoryTextBox.Name = "ADR_TransportCategoryTextBox";
			this.ADR_TransportCategoryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 18, true);
			this.ADR_TransportCategoryTextBox.TabIndex = 20;
			this.ADR_TransportCategoryTextBox.ReadOnly = true;
			// 
			// ADR_ExceptedQuantityCodeDropEdit
			// 
			this.ADR_ExceptedQuantityCodeDropEdit.AllowDrop = true;
			this.ADR_ExceptedQuantityCodeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.ADR_ExceptedQuantityCodeDropEdit, "ADR_ExceptedQuantityCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UNDGSubstanceADR)(null)).ADR_ExceptedQuantityCode)));
			this.ADR_ExceptedQuantityCodeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADRForm|02653dd0-534d-809e-4af8-eb53330645fb", "Excepted Quantity", "The Excepted Quantity Code assigned to this substance.");
			this.ADR_ExceptedQuantityCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 260, true);
			this.ADR_ExceptedQuantityCodeDropEdit.Name = "ADR_ExceptedQuantityCodeDropEdit";
			this.ADR_ExceptedQuantityCodeDropEdit.PreBoundMaxLength = 4;
			this.ADR_ExceptedQuantityCodeDropEdit.ShouldResizeByMaxLength = true;
			this.ADR_ExceptedQuantityCodeDropEdit.ShowDescriptionBox = true;
			this.ADR_ExceptedQuantityCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 18, true);
			this.ADR_ExceptedQuantityCodeDropEdit.TabIndex = 21;
			this.ADR_ExceptedQuantityCodeDropEdit.ReadOnly = true;
			//
			// CountryReferencesTabPage
			//
			this.CountryReferencesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADRForm|76a6c359-ef83-a18c-4a8f-d4129cd1d8c8", "Country/Region References");
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
			this.CountryReferencesModuleButtonGrid.GridId = "ca5572e6-deda-e99a-4f2b-e5475ba10691";
			this.CountryReferencesModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CountryReferencesModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(905, 183, true);
			// 
			// UNDGSubstanceADRControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ADR_ExceptedQuantityCodeDropEdit);
			this.Controls.Add(this.PackingProvisionsGroupBox);
			this.Controls.Add(this.UNDGSubstanceTabControl);
			this.Controls.Add(this.ADR_Label1TextBox);
			this.Controls.Add(this.ADR_Label2TextBox);
			this.Controls.Add(this.ADR_Label3TextBox);
			this.Controls.Add(this.ADR_Label4TextBox);
			this.Controls.Add(this.ADR_PGDropEdit);
			this.Controls.Add(this.ClassificationCode1TextBox);
			this.Controls.Add(this.ClassificationCode2TextBox);
			this.Controls.Add(this.ADR_ClassDropEdit);
			this.Controls.Add(this.ADR_ProperShippingNameTextBox);
			this.Controls.Add(this.ADR_VariantTextBox);
			this.Controls.Add(this.ADR_UNNOTextBox);
			this.Controls.Add(this.HeaderLabel);
			this.Controls.Add(this.IconPictureBox);
			this.Controls.Add(this.ADR_LQMaxAmtCalcEdit);
			this.Controls.Add(this.ADR_LQMaxAmtUQTextBox);
			this.Controls.Add(this.ADR_LQ2MaxAmtCalcEdit);
			this.Controls.Add(this.ADR_LQ2MaxAmtUQTextBox);
			this.Controls.Add(this.ADR_TransportCategoryTextBox);
			this.Controls.Add(this.ADR_HazardIDNumberTextBox);
			this.Controls.Add(this.IsActiveCheckBox);
			this.Controls.Add(this.IsSystemDefinedCheckBox);
			this.Controls.Add(this.DetailsLanguageDropEdit);
			this.Controls.Add(this.DG_BorderPanel);
			this.DataSourceAssemblyName = "Enterprise.MasterFiles.Business";
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.UNDGSubstanceADR);
			this.DataSourceTypeName = "Enterprise.MasterFiles.Business.UNDGSubstanceADR";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(922, 583, true);
			this.Name = "UNDGSubstanceADRControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.ADR_HazardIDNumberTextBox, 0);
			this.Controls.SetChildIndex(this.ADR_TransportCategoryTextBox, 0);
			this.Controls.SetChildIndex(this.ADR_LQ2MaxAmtUQTextBox, 0);
			this.Controls.SetChildIndex(this.ADR_LQ2MaxAmtCalcEdit, 0);
			this.Controls.SetChildIndex(this.ADR_LQMaxAmtUQTextBox, 0);
			this.Controls.SetChildIndex(this.ADR_LQMaxAmtCalcEdit, 0);
			this.Controls.SetChildIndex(this.IconPictureBox, 0);
			this.Controls.SetChildIndex(this.HeaderLabel, 0);
			this.Controls.SetChildIndex(this.ADR_UNNOTextBox, 0);
			this.Controls.SetChildIndex(this.ADR_VariantTextBox, 0);
			this.Controls.SetChildIndex(this.ADR_ProperShippingNameTextBox, 0);
			this.Controls.SetChildIndex(this.ADR_ClassDropEdit, 0);
			this.Controls.SetChildIndex(this.ClassificationCode1TextBox, 0);
			this.Controls.SetChildIndex(this.ClassificationCode2TextBox, 0);
			this.Controls.SetChildIndex(this.ADR_PGDropEdit, 0);
			this.Controls.SetChildIndex(this.ADR_Label1TextBox, 0);
			this.Controls.SetChildIndex(this.ADR_Label2TextBox, 0);
			this.Controls.SetChildIndex(this.ADR_Label3TextBox, 0);
			this.Controls.SetChildIndex(this.ADR_Label4TextBox, 0);
			this.Controls.SetChildIndex(this.DetailsLanguageDropEdit, 0);
			this.Controls.SetChildIndex(this.UNDGSubstanceTabControl, 0);
			this.Controls.SetChildIndex(this.PackingProvisionsGroupBox, 0);
			this.Controls.SetChildIndex(this.ADR_ExceptedQuantityCodeDropEdit, 0);
			this.Controls.SetChildIndex(this.DG_BorderPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.UNDGSubstanceTabControl.ResumeLayout(false);
			this.UNDGSubstanceTabControl.PerformLayout();
			this.NamesTabPage.ResumeLayout(false);
			this.NamesTabPage.PerformLayout();
			this.OtherProvisionsTabPage.ResumeLayout(false);
			this.OtherProvisionsTabPage.PerformLayout();
			this.PackingProvisionsGroupBox.ResumeLayout(false);
			this.PackingProvisionsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.IconPictureBox)).EndInit();
			this.ADR_ClassDropEdit.ResumeLayout(true);
			this.ADR_ClassDropEdit.PerformLayout();
			this.ADR_PGDropEdit.ResumeLayout(true);
			this.ADR_PGDropEdit.PerformLayout();
			this.DetailsLanguageDropEdit.ResumeLayout(true);
			this.DetailsLanguageDropEdit.PerformLayout();
			this.ADR_ExceptedQuantityCodeDropEdit.ResumeLayout(true);
			this.ADR_ExceptedQuantityCodeDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.NamesGrid)).EndInit();
			this.NamesGrid.ResumeLayout(false);
			this.NamesGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.QualifyingDescriptiveTextGrid)).EndInit();
			this.QualifyingDescriptiveTextGrid.ResumeLayout(false);
			this.QualifyingDescriptiveTextGrid.PerformLayout();
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
