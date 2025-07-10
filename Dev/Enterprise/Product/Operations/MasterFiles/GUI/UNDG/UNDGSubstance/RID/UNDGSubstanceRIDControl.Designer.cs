using System.Drawing;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class UNDGSubstanceRIDControl
	{
		ZLabel HeaderLabel;
		KPictureBox IconPictureBox;
		ZCheckBox IsActiveCheckBox;
		ZCheckBox IsSystemDefinedCheckBox;
		ZTextBox RID_UNNOTextBox;
		ZTextBox RID_VariantTextBox;
		ZTextBox RID_ProperShippingNameTextBox;
		ZDropEdit RID_ClassDropEdit;
		ZTextBox ClassificationCode1TextBox;
		ZTextBox ClassificationCode2TextBox;
		ZDropEdit RID_PGDropEdit;
		ZTextBox RID_Label1TextBox;
		ZTextBox RID_Label2TextBox;
		ZTextBox RID_Label3TextBox;
		ZTextBox RID_Label4TextBox;
		ZTextBox RID_SpecialProvisionsTextBox;
		ZCalcEdit RID_LQMaxAmtCalcEdit;
		ZTextBox RID_LQMaxAmtUQTextBox;
		ZCalcEdit RID_LQ2MaxAmtCalcEdit;
		ZTextBox RID_LQ2MaxAmtUQTextBox;
		ZDropEdit RID_ExceptedQuantityCodeDropEdit;
		ZTextBox RID_PackInsTextBox;
		ZTextBox RID_IBCInsTextBox;
		ZTextBox RID_PackProvTextBox;
		ZTextBox RID_MixedPackProvTextBox;
		ZGroupBox RID_BulkContainerTankGroupBox;
		ZTextBox RID_BulkContainerTankInsTextBox;
		ZTextBox RID_BulkContainerTankProvTextBox;
		ZTextBox RID_TankCodeTextBox;
		ZTextBox RID_TankSpecProvTextBox;
		ZTextBox RID_TransportCategoryTextBox;
		ZTextBox RID_CarriagePackagesSpecialProvTextBox;
		ZTextBox RID_CarriageBulkSpecialProvTextBox;
		ZTextBox RID_CarriageLoadingSpecialProvTextBox;
		ZTextBox RID_ColisExpressCodeTextBox;
		ZTextBox RID_HazardIDNumberTextBox;

		ZTemplateTabControl UNDGSubstanceTabControl;
		ZTabPage NamesTabPage;
		ZTabPage QualifyingDescriptiveTextTabPage;
		ZTabPage OtherProvisionsTabPage;
		ZGrid NamesGrid;
		ZGrid QualifyingDescriptiveTextGrid;
		ZDropEdit DetailsLanguageDropEdit;
		private System.ComponentModel.IContainer components;
		ZGroupBox PackingProvisionsGroupBox;
		ZPanel DG_BorderPanel;
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
			this.RID_BulkContainerTankGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RID_BulkContainerTankInsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RID_BulkContainerTankProvTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RID_TankCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RID_TankSpecProvTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PackingProvisionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RID_SpecialProvisionsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RID_PackInsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RID_IBCInsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RID_PackProvTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RID_MixedPackProvTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HeaderLabel = new Enterprise.ZArchitecture.ZLabel();
			this.IconPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsSystemDefinedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RID_UNNOTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RID_VariantTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RID_ProperShippingNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RID_ClassDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ClassificationCode1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ClassificationCode2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RID_PGDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RID_Label1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RID_Label2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RID_Label3TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RID_Label4TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RID_LQMaxAmtCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RID_LQMaxAmtUQTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RID_LQ2MaxAmtCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RID_LQ2MaxAmtUQTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RID_ExceptedQuantityCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RID_TransportCategoryTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RID_CarriagePackagesSpecialProvTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RID_CarriageBulkSpecialProvTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RID_CarriageLoadingSpecialProvTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RID_ColisExpressCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RID_HazardIDNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
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
			this.RID_BulkContainerTankGroupBox.SuspendLayout();
			this.PackingProvisionsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.IconPictureBox)).BeginInit();
			this.RID_ClassDropEdit.SuspendLayout();
			this.RID_PGDropEdit.SuspendLayout();
			this.RID_ExceptedQuantityCodeDropEdit.SuspendLayout();
			this.DetailsLanguageDropEdit.SuspendLayout();
			this.CountryReferencesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CountryReferencesModuleButtonGrid.InnerGrid)).BeginInit();
			this.CountryReferencesModuleButtonGrid.SuspendLayout();
			this.DG_BorderPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(UNDGSubstanceRID);
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
			this.UNDGSubstanceTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 226, true);
			this.UNDGSubstanceTabControl.TabIndex = 40;
			// 
			// NamesTabPage
			// 
			this.NamesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceRIDForm|c77ac0ee-1a4f-e795-418a-6dcf750bfde5", "Names");
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).Names)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGAttributeZZ)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).Names)).SyncRoot)).DAZ_Language)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGAttributeZZ)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).Names)).SyncRoot)).DAZ_Descriptor)));
			this.NamesGrid.CaptionVisible = false;
			nameLanguageColumnStyleInfo.ColumnName = "DAZ_Language";
			nameLanguageColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			nameLanguageColumnStyleInfo.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesRIDForm|aa7f4333-d5c7-56b3-4460-b329fbe046c8", "Lang", "Language", "The Language of this detail.");
			nameDescriptorColumnStyleInfo.ColumnName = "DAZ_Descriptor";
			nameDescriptorColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(800);
			nameDescriptorColumnStyleInfo.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesRIDForm|3af9943a-c368-6b93-4e9b-9d92deddde66", "Detail");
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
			// QualifyingDescriptiveTextTabPage
			// 
			this.QualifyingDescriptiveTextTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceRIDForm|23f7b87f-5093-efb2-4c83-dc2dec71124c", "Qualifying Descriptive Text");
			this.QualifyingDescriptiveTextTabPage.Controls.Add(this.QualifyingDescriptiveTextGrid);
			this.QualifyingDescriptiveTextTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.QualifyingDescriptiveTextTabPage.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(545, 175, true);
			this.QualifyingDescriptiveTextTabPage.Name = "QualifyingDescriptiveTextTabPage";
			this.QualifyingDescriptiveTextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(799, 203, true);
			this.QualifyingDescriptiveTextTabPage.TabIndex = 2;
			// 
			// DG_BorderPanel
			//
			this.DG_BorderPanel.Name = "DG_BorderPanel";
			this.DG_BorderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 1, true);
			this.DG_BorderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 40, true);
			this.DG_BorderPanel.BackColor = Color.DarkGray;
			// 
			// QualifyingDescriptiveTextGrid
			// 
			this.QualifyingDescriptiveTextGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.QualifyingDescriptiveTextGrid, "QualifyingDescriptiveTexts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).QualifyingDescriptiveTexts)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGAttributeZZ)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).QualifyingDescriptiveTexts)).SyncRoot)).DAZ_Language)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGAttributeZZ)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).QualifyingDescriptiveTexts)).SyncRoot)).DAZ_Descriptor)));
			this.QualifyingDescriptiveTextGrid.CaptionVisible = false;
			qualifyingLanguageColumnStyleInfo.ColumnName = "DAZ_Language";
			qualifyingLanguageColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			qualifyingLanguageColumnStyleInfo.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesRIDForm|5c5c1f8c-3238-a199-4f07-bb29faace64c", "Lang", "Language", "The Language of this detail.");
			qualifyingDescriptorColumnStyleInfo.ColumnName = "DAZ_Descriptor";
			qualifyingDescriptorColumnStyleInfo.MinimumEditControlWidth = 300;
			qualifyingDescriptorColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(800);
			qualifyingDescriptorColumnStyleInfo.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesRIDForm|6141586f-4c45-71b1-4378-416f0e2f819c", "Detail");
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
			this.OtherProvisionsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceRIDForm|556e9577-8d81-ecbc-4ebd-4c2a52e6f214", "Other Provisions");
			this.OtherProvisionsTabPage.Controls.Add(this.RID_BulkContainerTankGroupBox);
			this.OtherProvisionsTabPage.Controls.Add(this.RID_TankCodeTextBox);
			this.OtherProvisionsTabPage.Controls.Add(this.RID_TankSpecProvTextBox);
			this.OtherProvisionsTabPage.Controls.Add(this.RID_IBCInsTextBox);
			this.OtherProvisionsTabPage.Controls.Add(this.RID_CarriageBulkSpecialProvTextBox);
			this.OtherProvisionsTabPage.Controls.Add(this.RID_CarriageLoadingSpecialProvTextBox);
			this.OtherProvisionsTabPage.Controls.Add(this.RID_ColisExpressCodeTextBox);
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).DetailsLanguage)));
			this.DetailsLanguageDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesRIDForm|787f09d8-10db-7ea3-4789-acc204945c35", "Language Filter", "Filters the Names, Qualifying Descriptive Texts, Special Provisions and Stowage/Segmentation requirements to only show those that are in the language you choose.");
			this.DetailsLanguageDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 286, true);
			this.DetailsLanguageDropEdit.Name = "DetailsLanguageDropEdit";
			this.DetailsLanguageDropEdit.ShouldResizeByMaxLength = true;
			this.DetailsLanguageDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.DetailsLanguageDropEdit.TabIndex = 39;
			//
			// RID_BulkContainerTankGroupBox
			//
			this.RID_BulkContainerTankGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesRIDForm|74af7afd-3b75-4ee1-9f9b-f1f4243cacae", "Portable Tank and Bulk Container");
			this.RID_BulkContainerTankGroupBox.Controls.Add(this.RID_BulkContainerTankInsTextBox);
			this.RID_BulkContainerTankGroupBox.Controls.Add(this.RID_BulkContainerTankProvTextBox);
			this.RID_BulkContainerTankGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 130, true);
			this.RID_BulkContainerTankGroupBox.Name = "RID_BulkContainerTankGroupBox";
			this.RID_BulkContainerTankGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 80, true);
			this.RID_BulkContainerTankGroupBox.TabIndex = 5;
			this.RID_BulkContainerTankGroupBox.TabStop = false;
			// 
			// RID_BulkContainerTankInsTextBox
			// 
			this.BindingSource.SetBindingMember(this.RID_BulkContainerTankInsTextBox, "RID_BulkContainerTankIns");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).RID_BulkContainerTankIns)));
			this.RID_BulkContainerTankInsTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesRIDForm|3236fe2d-da21-9dbc-43b0-b77293548352", "Tank/Bulk Instr.", "Tank/Bulk Instructions", "Portable Tank/Bulk Container Instructions");
			this.RID_BulkContainerTankInsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RID_BulkContainerTankInsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 22, true);
			this.RID_BulkContainerTankInsTextBox.Name = "RID_BulkContainerTankInsTextBox";
			this.RID_BulkContainerTankInsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 18, true);
			this.RID_BulkContainerTankInsTextBox.TabIndex = 1;
			this.RID_BulkContainerTankInsTextBox.ReadOnly = true;
			// 
			// RID_BulkContainerTankProvTextBox
			// 
			this.BindingSource.SetBindingMember(this.RID_BulkContainerTankProvTextBox, "RID_BulkContainerTankProv");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).RID_BulkContainerTankProv)));
			this.RID_BulkContainerTankProvTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesRIDForm|0429186b-d8f9-a79f-4513-73e58b5974f6", "Tank/Bulk Spec. Prov.", "Tank/Bulk Special Provisions", "Portable Tank/Bulk Container Special Provisions");
			this.RID_BulkContainerTankProvTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RID_BulkContainerTankProvTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 48, true);
			this.RID_BulkContainerTankProvTextBox.Name = "RID_BulkContainerTankProvTextBox";
			this.RID_BulkContainerTankProvTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 18, true);
			this.RID_BulkContainerTankProvTextBox.TabIndex = 1;
			this.RID_BulkContainerTankProvTextBox.ReadOnly = true;
			// 
			// RID_TankCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.RID_TankCodeTextBox, "RID_TankCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).RID_TankCode)));
			this.RID_TankCodeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesRIDForm|4dc60f08-3c4b-71aa-44f8-71a23a0ea279", "RID Tank Code");
			this.RID_TankCodeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RID_TankCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 78, true);
			this.RID_TankCodeTextBox.Name = "RID_TankCodeTextBox";
			this.RID_TankCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 18, true);
			this.RID_TankCodeTextBox.TabIndex = 4;
			this.RID_TankCodeTextBox.ReadOnly = true;
			// 
			// RID_TankSpecProvTextBox
			// 
			this.BindingSource.SetBindingMember(this.RID_TankSpecProvTextBox, "RID_TankSpecProv");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).RID_TankSpecProv)));
			this.RID_TankSpecProvTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesRIDForm|cff81109-a87b-1b8a-46b1-554c5b71a7b3", "RID Tank Spec. Prov.", "RID Tank Special Provisions", "The Special Provisions assigned to this substance concerning carriage in RID Tanks.");
			this.RID_TankSpecProvTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RID_TankSpecProvTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 104, true);
			this.RID_TankSpecProvTextBox.Name = "RID_TankSpecProvTextBox";
			this.RID_TankSpecProvTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 18, true);
			this.RID_TankSpecProvTextBox.TabIndex = 4;
			this.RID_TankSpecProvTextBox.ReadOnly = true;
			// 
			// RID_IBCInsTextBox
			// 
			this.BindingSource.SetBindingMember(this.RID_IBCInsTextBox, "RID_IBCIns");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).RID_IBCIns)));
			this.RID_IBCInsTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesRIDForm|d7dc2254-0abf-3db4-4d0c-aee88c2d4410", "IBC Packing Instr.", "IBC Packing Instructions");
			this.RID_IBCInsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RID_IBCInsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 52, true);
			this.RID_IBCInsTextBox.Name = "RID_IBCInsTextBox";
			this.RID_IBCInsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
			this.RID_IBCInsTextBox.TabIndex = 2;
			this.RID_IBCInsTextBox.ReadOnly = true;
			// 
			// RID_CarriageBulkSpecialProvTextBox
			// 
			this.BindingSource.SetBindingMember(this.RID_CarriageBulkSpecialProvTextBox, "RID_CarriageBulkSpecialProv");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).RID_CarriageBulkSpecialProv)));
			this.RID_CarriageBulkSpecialProvTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesRIDForm|1fa2b683-a20b-9295-4a8a-11e7b62acaea", "Bulk Spec. Prov.", "Bulk Special Provisions");
			this.RID_CarriageBulkSpecialProvTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RID_CarriageBulkSpecialProvTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(539, 26, true);
			this.RID_CarriageBulkSpecialProvTextBox.Name = "RID_CarriageBulkSpecialProvTextBox";
			this.RID_CarriageBulkSpecialProvTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 18, true);
			this.RID_CarriageBulkSpecialProvTextBox.TabIndex = 3;
			this.RID_CarriageBulkSpecialProvTextBox.ReadOnly = true;
			// 
			// RID_CarriageLoadingSpecialProvTextBox
			// 
			this.BindingSource.SetBindingMember(this.RID_CarriageLoadingSpecialProvTextBox, "RID_CarriageLoadingSpecialProv");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).RID_CarriageLoadingSpecialProv)));
			this.RID_CarriageLoadingSpecialProvTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesRIDForm|5ac87784-0c31-c6b9-47c3-7bee5f5f85f4", "Handling Spec. Prov.", "Handling Special Provisions");
			this.RID_CarriageLoadingSpecialProvTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RID_CarriageLoadingSpecialProvTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(539, 52, true);
			this.RID_CarriageLoadingSpecialProvTextBox.Name = "RID_CarriageLoadingSpecialProvTextBox";
			this.RID_CarriageLoadingSpecialProvTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 18, true);
			this.RID_CarriageLoadingSpecialProvTextBox.TabIndex = 3;
			this.RID_CarriageLoadingSpecialProvTextBox.ReadOnly = true;
			// 
			// RID_ColisExpressCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.RID_ColisExpressCodeTextBox, "RID_ColisExpressCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).RID_ColisExpressCode)));
			this.RID_ColisExpressCodeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesRIDForm|223b5da7-3fb4-e9aa-4e90-0ef2b0ed1ba2", "Colis Express (Parcels)");
			this.RID_ColisExpressCodeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RID_ColisExpressCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 26, true);
			this.RID_ColisExpressCodeTextBox.Name = "RID_ColisExpressCodeTextBox";
			this.RID_ColisExpressCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
			this.RID_ColisExpressCodeTextBox.TabIndex = 1;
			this.RID_ColisExpressCodeTextBox.ReadOnly = true;
			// 
			// PackingProvisionsGroupBox
			// 
			this.PackingProvisionsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceRIDForm|9838115d-2bab-d086-4acd-d54b824648f6", "Packing Provisions");
			this.PackingProvisionsGroupBox.Controls.Add(this.RID_SpecialProvisionsTextBox);
			this.PackingProvisionsGroupBox.Controls.Add(this.RID_PackInsTextBox);
			this.PackingProvisionsGroupBox.Controls.Add(this.RID_CarriagePackagesSpecialProvTextBox);
			this.PackingProvisionsGroupBox.Controls.Add(this.RID_PackProvTextBox);
			this.PackingProvisionsGroupBox.Controls.Add(this.RID_MixedPackProvTextBox);
			this.PackingProvisionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(447, 100, true);
			this.PackingProvisionsGroupBox.Name = "PackingProvisionsGroupBox";
			this.PackingProvisionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 156, true);
			this.PackingProvisionsGroupBox.TabIndex = 30;
			this.PackingProvisionsGroupBox.TabStop = false;
			// 
			// RID_SpecialProvisionsTextBox
			// 
			this.RID_SpecialProvisionsTextBox.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left;
			this.BindingSource.SetBindingMember(this.RID_SpecialProvisionsTextBox, "RID_SpecialProvisions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).RID_SpecialProvisions)));
			this.RID_SpecialProvisionsTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesRIDForm|1cbd46b6-41d0-5aba-43ee-235226c2f920", "Special Provisions");
			this.RID_SpecialProvisionsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RID_SpecialProvisionsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 26, true);
			this.RID_SpecialProvisionsTextBox.Name = "RID_SpecialProvisionsTextBox";
			this.RID_SpecialProvisionsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 18, true);
			this.RID_SpecialProvisionsTextBox.TabIndex = 6;
			this.RID_SpecialProvisionsTextBox.ReadOnly = true;
			// 
			// RID_PackInsTextBox
			// 
			this.BindingSource.SetBindingMember(this.RID_PackInsTextBox, "RID_PackIns");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).RID_PackIns)));
			this.RID_PackInsTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesRIDForm|6d80e8df-d44a-1b87-441c-28bc4c5620e3", "Packing Instruction", "The packing instruction code that relates to the transportation of this substance in limited quantities.");
			this.RID_PackInsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RID_PackInsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 130, true);
			this.RID_PackInsTextBox.Name = "RID_PackInsTextBox";
			this.RID_PackInsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 18, true);
			this.RID_PackInsTextBox.TabIndex = 35;
			this.RID_PackInsTextBox.ReadOnly = true;
			// 
			// RID_CarriagePackagesSpecialProvTextBox
			// 
			this.BindingSource.SetBindingMember(this.RID_CarriagePackagesSpecialProvTextBox, "RID_CarriagePackagesSpecialProv");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).RID_CarriagePackagesSpecialProv)));
			this.RID_CarriagePackagesSpecialProvTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesRIDForm|756bb941-45db-dc9b-4d63-8124f5a21277", "Packing Special Provisions");
			this.RID_CarriagePackagesSpecialProvTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RID_CarriagePackagesSpecialProvTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 104, true);
			this.RID_CarriagePackagesSpecialProvTextBox.Name = "RID_CarriagePackagesSpecialProvTextBox";
			this.RID_CarriagePackagesSpecialProvTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 18, true);
			this.RID_CarriagePackagesSpecialProvTextBox.TabIndex = 34;
			this.RID_CarriagePackagesSpecialProvTextBox.ReadOnly = true;
			// 
			// RID_PackProvTextBox
			// 
			this.BindingSource.SetBindingMember(this.RID_PackProvTextBox, "RID_PackProv");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).RID_PackProv)));
			this.RID_PackProvTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesRIDForm|7f5a896c-b9ce-aaa8-4c0f-826d4be0ef81", "Packing Provisions");
			this.RID_PackProvTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RID_PackProvTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 52, true);
			this.RID_PackProvTextBox.Name = "RID_PackProvTextBox";
			this.RID_PackProvTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 18, true);
			this.RID_PackProvTextBox.TabIndex = 31;
			this.RID_PackProvTextBox.ReadOnly = true;
			// 
			// RID_MixedPackingProvTextBox
			// 
			this.BindingSource.SetBindingMember(this.RID_MixedPackProvTextBox, "RID_MixedPackProv");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).RID_MixedPackProv)));
			this.RID_MixedPackProvTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesRIDForm|dfdc316b-db35-cdb7-494b-5fd7c58a0ca2", "Mixed Packing Provisions");
			this.RID_MixedPackProvTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RID_MixedPackProvTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 78, true);
			this.RID_MixedPackProvTextBox.Name = "RID_MixedPackProvTextBox";
			this.RID_MixedPackProvTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 18, true);
			this.RID_MixedPackProvTextBox.TabIndex = 32;
			this.RID_MixedPackProvTextBox.ReadOnly = true;
			// 
			// HeaderLabel
			// 
			this.HeaderLabel.AutoSize = true;
			this.HeaderLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesRIDForm|4612c800-47c3-1788-4508-e89ffa99e5bd", "RAIL FREIGHT (RID)");
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
			this.IconPictureBox.Image = global::Enterprise.MasterFiles.GUI.Properties.Resources.railfreight;
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
			this.BindingSource.SetBindingMember(this.IsActiveCheckBox, "RID_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).RID_IsActive)));
			this.IsActiveCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceRIDForm|947feb68-d5d8-7c85-4cb5-06325b52c3c2", "Active");
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
			this.BindingSource.SetBindingMember(this.IsSystemDefinedCheckBox, "RID_IsSystemDefined");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).RID_IsSystemDefined)));
			this.IsSystemDefinedCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceRIDForm|842fd27b-d2e7-cab4-465a-4126bc3072e5", "System Defined");
			this.IsSystemDefinedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsSystemDefinedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsSystemDefinedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(805, 55, true);
			this.IsSystemDefinedCheckBox.Name = "IsSystemDefinedCheckBox";
			this.IsSystemDefinedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 14, true);
			this.IsSystemDefinedCheckBox.TabIndex = 8;
			this.IsSystemDefinedCheckBox.ReadOnly = true;
			// 
			// RID_UNNOTextBox
			// 
			this.BindingSource.SetBindingMember(this.RID_UNNOTextBox, "RID_UNNO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).RID_UNNO)));
			this.RID_UNNOTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceRIDForm|113bcfb1-1acd-9fbb-4e3a-a4b4695fa10d", "UN No.", "UN No.", "The United Nations (UN) number for this substance.");
			this.RID_UNNOTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 52, true);
			this.RID_UNNOTextBox.Name = "RID_UNNOTextBox";
			this.RID_UNNOTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 18, true);
			this.RID_UNNOTextBox.TabIndex = 0;
			this.RID_UNNOTextBox.ReadOnly = true;
			// 
			// RID_VariantTextBox
			// 
			this.BindingSource.SetBindingMember(this.RID_VariantTextBox, "RID_Variant");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).RID_Variant)));
			this.RID_VariantTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceRIDForm|39b4f550-603f-f6bf-4318-bac850b9c316", "Variant");
			this.RID_VariantTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 52, true);
			this.RID_VariantTextBox.Name = "RID_VariantTextBox";
			this.RID_VariantTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 18, true);
			this.RID_VariantTextBox.TabIndex = 1;
			this.RID_VariantTextBox.ReadOnly = true;
			// 
			// RID_ProperShippingNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.RID_ProperShippingNameTextBox, "RID_PSN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).RID_PSN)));
			this.RID_ProperShippingNameTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceRIDForm|c7d4c900-dbe8-a4a8-45b6-31aa21114e47", "PSN", "Proper Shipping Name", "The Proper Shipping Name for this substance.");
			this.RID_ProperShippingNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RID_ProperShippingNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 78, true);
			this.RID_ProperShippingNameTextBox.Name = "RID_ProperShippingNameTextBox";
			this.RID_ProperShippingNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 18, true);
			this.RID_ProperShippingNameTextBox.TabIndex = 10;
			this.RID_ProperShippingNameTextBox.ReadOnly = true;
			// 
			// RID_ClassDropEdit
			// 
			this.RID_ClassDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RID_ClassDropEdit, "RID_Class");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).RID_Class)));
			this.RID_ClassDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceRIDForm|e1eedc5b-8822-e898-4ab5-a034b0a739dd", "Class", "The RID Class number assigned to this substance.");
			this.RID_ClassDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(290, 52, true);
			this.RID_ClassDropEdit.Name = "RID_ClassDropEdit";
			this.RID_ClassDropEdit.PreBoundMaxLength = 4;
			this.RID_ClassDropEdit.ShouldResizeByMaxLength = true;
			this.RID_ClassDropEdit.ShowDescriptionBox = false;
			this.RID_ClassDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 18, true);
			this.RID_ClassDropEdit.TabIndex = 2;
			this.RID_ClassDropEdit.ReadOnly = true;
			// 
			// ClassificationCode1TextBox
			// 
			this.BindingSource.SetBindingMember(this.ClassificationCode1TextBox, "ClassificationCode1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).RID_ClassificationCode)));
			this.ClassificationCode1TextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesRIDForm|72a181f4-a33d-6193-4bae-c2669698cd9e", "Class. Code", "The Classification Code assigned to this substance.");
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).ClassificationCode2)));
			this.ClassificationCode2TextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceRIDForm|94dc216a-518d-e288-45ad-8b65742483ce", "Class. Code 2");
			this.ClassificationCode2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ClassificationCode2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(457, 52, true);
			this.ClassificationCode2TextBox.Name = "ClassificationCode2TextBox";
			this.ClassificationCode2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 18, true);
			this.ClassificationCode2TextBox.TabIndex = 4;
			this.ClassificationCode2TextBox.ReadOnly = true;
			// 
			// RID_PGDropEdit
			// 
			this.RID_PGDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RID_PGDropEdit, "RID_PG");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).RID_PG)));
			this.RID_PGDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesRIDForm|18f28f4b-aee1-1a81-4066-1976e9ba8bc9", "PG", "Packing Group", "The UN Packing Group (either I, II or III).");
			this.RID_PGDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(575, 52, true);
			this.RID_PGDropEdit.Name = "RID_PGDropEdit";
			this.RID_PGDropEdit.PreBoundMaxLength = 4;
			this.RID_PGDropEdit.ShouldResizeByMaxLength = true;
			this.RID_PGDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 18, true);
			this.RID_PGDropEdit.TabIndex = 5;
			this.RID_PGDropEdit.ReadOnly = true;
			// 
			// RID_LabelsTextBox
			// 
			this.RID_Label1TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.RID_Label1TextBox, "RID_Label1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).RID_Label1)));
			this.RID_Label1TextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesRIDForm|156bb046-4e80-cd8b-4026-4f51851f6227", "Label(s)", "The subsidiary hazard label(s) assigned to this substance.");
			this.RID_Label1TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RID_Label1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 130, true);
			this.RID_Label1TextBox.Name = "RID_Label1TextBox";
			this.RID_Label1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 18, true);
			this.RID_Label1TextBox.TabIndex = 11;
			this.RID_Label1TextBox.ReadOnly = true;
			// 
			// RID_LabelsTextBox
			// 
			this.RID_Label2TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.RID_Label2TextBox, "RID_Label2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).RID_Label2)));
			this.RID_Label2TextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RID_Label2TextBox, false);
			this.RID_Label2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RID_Label2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 130, true);
			this.RID_Label2TextBox.Name = "RID_Label2TextBox";
			this.RID_Label2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 18, true);
			this.RID_Label2TextBox.TabIndex = 12;
			this.RID_Label2TextBox.ReadOnly = true;
			// 
			// RID_LabelsTextBox
			// 
			this.RID_Label3TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.RID_Label3TextBox, "RID_Label3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).RID_Label3)));
			this.RID_Label3TextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RID_Label3TextBox, false);
			this.RID_Label3TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RID_Label3TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 130, true);
			this.RID_Label3TextBox.Name = "RID_Label3TextBox";
			this.RID_Label3TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 18, true);
			this.RID_Label3TextBox.TabIndex = 13;
			this.RID_Label3TextBox.ReadOnly = true;
			// 
			// RID_Label4TextBox
			// 
			this.RID_Label4TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.RID_Label4TextBox, "RID_Label4");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).RID_Label4)));
			this.RID_Label4TextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RID_Label4TextBox, false);
			this.RID_Label4TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RID_Label4TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 130, true);
			this.RID_Label4TextBox.Name = "RID_Label4TextBox";
			this.RID_Label4TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 18, true);
			this.RID_Label4TextBox.TabIndex = 14;
			this.RID_Label4TextBox.ReadOnly = true;
			// 
			// RID_LQMaxAmtCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.RID_LQMaxAmtCalcEdit, "RID_LQMaxAmt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).RID_LQMaxAmt)));
			this.RID_LQMaxAmtCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesRIDForm|7128fd64-6244-ad96-4c44-e32cc2c59ef9", "Limited Quantity", "The Limited Quantity Code assigned to this substance.");
			this.RID_LQMaxAmtCalcEdit.DecimalPlaces = 2;
			this.RID_LQMaxAmtCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 156, true);
			this.RID_LQMaxAmtCalcEdit.Name = "RID_LQMaxAmtCalcEdit";
			this.RID_LQMaxAmtCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 18, true);
			this.RID_LQMaxAmtCalcEdit.TabIndex = 15;
			this.RID_LQMaxAmtCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.RID_LQMaxAmtCalcEdit.ReadOnly = true;
			// 
			// RID_LQMaxAmtUQTextBox
			// 
			this.BindingSource.SetBindingMember(this.RID_LQMaxAmtUQTextBox, "RID_LQMaxAmtUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).RID_LQMaxAmtUQ)));
			this.RID_LQMaxAmtUQTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesRIDForm|798c32d3-476b-7b90-4294-26596f65a29c", "Units");
			this.RID_LQMaxAmtUQTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RID_LQMaxAmtUQTextBox, false);
			this.RID_LQMaxAmtUQTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 156, true);
			this.RID_LQMaxAmtUQTextBox.Name = "RID_LQMaxAmtUQTextBox";
			this.RID_LQMaxAmtUQTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 18, true);
			this.RID_LQMaxAmtUQTextBox.TabIndex = 16;
			this.RID_LQMaxAmtUQTextBox.ReadOnly = true;
			// 
			// RID_LQ2MaxAmtCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.RID_LQ2MaxAmtCalcEdit, "RID_LQ2MaxAmt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).RID_LQ2MaxAmt)));
			this.RID_LQ2MaxAmtCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesRIDForm|f55419cf-803b-7bb3-4895-312c0ddaec49", "Limited Quantity 2", "The Limited Quantity Code assigned to this substance.");
			this.RID_LQ2MaxAmtCalcEdit.DecimalPlaces = 3;
			this.RID_LQ2MaxAmtCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 182, true);
			this.RID_LQ2MaxAmtCalcEdit.Name = "RID_LQ2MaxAmtCalcEdit";
			this.RID_LQ2MaxAmtCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 18, true);
			this.RID_LQ2MaxAmtCalcEdit.TabIndex = 17;
			this.RID_LQ2MaxAmtCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.RID_LQ2MaxAmtCalcEdit.ReadOnly = true;
			// 
			// RID_LQ2MaxAmtUQTextBox
			// 
			this.BindingSource.SetBindingMember(this.RID_LQ2MaxAmtUQTextBox, "RID_LQ2MaxAmtUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).RID_LQ2MaxAmtUQ)));
			this.RID_LQ2MaxAmtUQTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesRIDForm|57cc8da4-2156-4b99-4303-3acf29179f54", "Units");
			this.RID_LQ2MaxAmtUQTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RID_LQ2MaxAmtUQTextBox, false);
			this.RID_LQ2MaxAmtUQTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 182, true);
			this.RID_LQ2MaxAmtUQTextBox.Name = "RID_LQ2MaxAmtUQTextBox";
			this.RID_LQ2MaxAmtUQTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 18, true);
			this.RID_LQ2MaxAmtUQTextBox.TabIndex = 18;
			this.RID_LQ2MaxAmtUQTextBox.ReadOnly = true;
			// 
			// RID_HazardIDNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.RID_HazardIDNumberTextBox, "RID_HazardIDNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).RID_HazardIDNumber)));
			this.RID_HazardIDNumberTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesRIDForm|a85cc047-985c-51b2-414d-e159896c7e57", "Hazard ID Number", "The Hazard Identification Number identifies the substance's hazard qualities.");
			this.RID_HazardIDNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RID_HazardIDNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 208, true);
			this.RID_HazardIDNumberTextBox.Name = "RID_HazardIDNumberTextBox";
			this.RID_HazardIDNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 18, true);
			this.RID_HazardIDNumberTextBox.TabIndex = 19;
			this.RID_HazardIDNumberTextBox.ReadOnly = true;
			// 
			// RID_TransportCategoryTextBox
			// 
			this.BindingSource.SetBindingMember(this.RID_TransportCategoryTextBox, "RID_TransportCategory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).RID_TransportCategory)));
			this.RID_TransportCategoryTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesRIDForm|fd20acd2-1445-e399-4e35-2fb793aef2a5", "Transport Category", "The Transport Category (Tunnel Restriction) code assigned to this substance.");
			this.RID_TransportCategoryTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RID_TransportCategoryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 234, true);
			this.RID_TransportCategoryTextBox.Name = "RID_TransportCategoryTextBox";
			this.RID_TransportCategoryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 18, true);
			this.RID_TransportCategoryTextBox.TabIndex = 20;
			this.RID_TransportCategoryTextBox.ReadOnly = true;
			// 
			// RID_ExceptedQuantityCodeDropEdit
			// 
			this.RID_ExceptedQuantityCodeDropEdit.AllowDrop = true;
			this.RID_ExceptedQuantityCodeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.RID_ExceptedQuantityCodeDropEdit, "RID_ExceptedQuantityCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UNDGSubstanceRID)(null)).RID_ExceptedQuantityCode)));
			this.RID_ExceptedQuantityCodeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesRIDForm|36329d14-9117-5b9f-4054-009622f17d13", "Excepted Quantity", "The Excepted Quantity Code assigned to this substance.");
			this.RID_ExceptedQuantityCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 260, true);
			this.RID_ExceptedQuantityCodeDropEdit.Name = "RID_ExceptedQuantityCodeDropEdit";
			this.RID_ExceptedQuantityCodeDropEdit.PreBoundMaxLength = 4;
			this.RID_ExceptedQuantityCodeDropEdit.ShouldResizeByMaxLength = true;
			this.RID_ExceptedQuantityCodeDropEdit.ShowDescriptionBox = true;
			this.RID_ExceptedQuantityCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(682, 18, true);
			this.RID_ExceptedQuantityCodeDropEdit.TabIndex = 21;
			this.RID_ExceptedQuantityCodeDropEdit.ReadOnly = true;
			//
			// CountryReferencesTabPage
			//
			this.CountryReferencesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesRIDForm|4195e81e-92c9-2eb7-496c-700d761b7292", "Country/Region References");
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
			this.CountryReferencesModuleButtonGrid.GridId = "ae356ae6-a083-75b4-4227-bd4283047e76";
			this.CountryReferencesModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CountryReferencesModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(905, 183, true);
			// 
			// UNDGSubstanceRIDForm
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RID_ExceptedQuantityCodeDropEdit);
			this.Controls.Add(this.PackingProvisionsGroupBox);
			this.Controls.Add(this.UNDGSubstanceTabControl);
			this.Controls.Add(this.RID_Label1TextBox);
			this.Controls.Add(this.RID_Label2TextBox);
			this.Controls.Add(this.RID_Label3TextBox);
			this.Controls.Add(this.RID_Label4TextBox);
			this.Controls.Add(this.RID_PGDropEdit);
			this.Controls.Add(this.ClassificationCode1TextBox);
			this.Controls.Add(this.ClassificationCode2TextBox);
			this.Controls.Add(this.RID_ClassDropEdit);
			this.Controls.Add(this.RID_ProperShippingNameTextBox);
			this.Controls.Add(this.RID_VariantTextBox);
			this.Controls.Add(this.RID_UNNOTextBox);
			this.Controls.Add(this.HeaderLabel);
			this.Controls.Add(this.IconPictureBox);
			this.Controls.Add(this.RID_LQMaxAmtCalcEdit);
			this.Controls.Add(this.RID_LQMaxAmtUQTextBox);
			this.Controls.Add(this.RID_LQ2MaxAmtCalcEdit);
			this.Controls.Add(this.RID_LQ2MaxAmtUQTextBox);
			this.Controls.Add(this.RID_TransportCategoryTextBox);
			this.Controls.Add(this.RID_HazardIDNumberTextBox);
			this.Controls.Add(this.IsActiveCheckBox);
			this.Controls.Add(this.IsSystemDefinedCheckBox);
			this.Controls.Add(this.DetailsLanguageDropEdit);
			this.Controls.Add(this.DG_BorderPanel);
			this.DataSourceAssemblyName = "Enterprise.MasterFiles.Business";
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.UNDGSubstanceRID);
			this.DataSourceTypeName = "Enterprise.MasterFiles.Business.UNDGSubstanceRID";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(922, 583, true);
			this.Name = "UNDGSubstanceRIDControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.RID_HazardIDNumberTextBox, 0);
			this.Controls.SetChildIndex(this.RID_TransportCategoryTextBox, 0);
			this.Controls.SetChildIndex(this.RID_LQ2MaxAmtUQTextBox, 0);
			this.Controls.SetChildIndex(this.RID_LQ2MaxAmtCalcEdit, 0);
			this.Controls.SetChildIndex(this.RID_LQMaxAmtUQTextBox, 0);
			this.Controls.SetChildIndex(this.RID_LQMaxAmtCalcEdit, 0);
			this.Controls.SetChildIndex(this.IconPictureBox, 0);
			this.Controls.SetChildIndex(this.HeaderLabel, 0);
			this.Controls.SetChildIndex(this.RID_UNNOTextBox, 0);
			this.Controls.SetChildIndex(this.RID_VariantTextBox, 0);
			this.Controls.SetChildIndex(this.RID_ProperShippingNameTextBox, 0);
			this.Controls.SetChildIndex(this.RID_ClassDropEdit, 0);
			this.Controls.SetChildIndex(this.ClassificationCode1TextBox, 0);
			this.Controls.SetChildIndex(this.ClassificationCode2TextBox, 0);
			this.Controls.SetChildIndex(this.RID_PGDropEdit, 0);
			this.Controls.SetChildIndex(this.RID_Label1TextBox, 0);
			this.Controls.SetChildIndex(this.RID_Label2TextBox, 0);
			this.Controls.SetChildIndex(this.RID_Label3TextBox, 0);
			this.Controls.SetChildIndex(this.RID_Label4TextBox, 0);
			this.Controls.SetChildIndex(this.DetailsLanguageDropEdit, 0);
			this.Controls.SetChildIndex(this.UNDGSubstanceTabControl, 0);
			this.Controls.SetChildIndex(this.PackingProvisionsGroupBox, 0);
			this.Controls.SetChildIndex(this.RID_ExceptedQuantityCodeDropEdit, 0);			
			this.Controls.SetChildIndex(this.DG_BorderPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.UNDGSubstanceTabControl.ResumeLayout(false);
			this.UNDGSubstanceTabControl.PerformLayout();
			this.NamesTabPage.ResumeLayout(false);
			this.NamesTabPage.PerformLayout();
			this.OtherProvisionsTabPage.ResumeLayout(false);
			this.OtherProvisionsTabPage.PerformLayout();
			this.RID_BulkContainerTankGroupBox.ResumeLayout(false);
			this.RID_BulkContainerTankGroupBox.PerformLayout();
			this.PackingProvisionsGroupBox.ResumeLayout(false);
			this.PackingProvisionsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.IconPictureBox)).EndInit();
			this.RID_ClassDropEdit.ResumeLayout(true);
			this.RID_ClassDropEdit.PerformLayout();
			this.RID_PGDropEdit.ResumeLayout(true);
			this.RID_PGDropEdit.PerformLayout();
			this.DetailsLanguageDropEdit.ResumeLayout(true);
			this.DetailsLanguageDropEdit.PerformLayout();
			this.RID_ExceptedQuantityCodeDropEdit.ResumeLayout(true);
			this.RID_ExceptedQuantityCodeDropEdit.PerformLayout();
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
