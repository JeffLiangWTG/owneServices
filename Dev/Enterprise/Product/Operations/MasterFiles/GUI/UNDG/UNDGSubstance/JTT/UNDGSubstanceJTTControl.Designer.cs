using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class UNDGSubstanceJTTControl
	{
		ZLabel HeaderLabel;
		KPictureBox IconPictureBox;
		ZCheckBox IsActiveCheckBox;
		ZCheckBox IsSystemDefinedCheckBox;
		ZTextBox JTT_UNNOTextBox;
		ZTextBox JTT_VariantTextBox;
		ZTextBox JTT_ProperShippingNameTextBox;
		ZDropEdit JTT_ClassDropEdit;
		ZTextBox ClassificationCode1TextBox;
		ZTextBox ClassificationCode2TextBox;
		ZDropEdit JTT_PGDropEdit;
		ZTextBox JTT_Label1TextBox;
		ZTextBox JTT_Label2TextBox;
		ZTextBox JTT_Label3TextBox;
		ZTextBox JTT_SpecialProvisionsTextBox;
		ZCalcEdit JTT_LQMaxAmtCalcEdit;
		ZTextBox JTT_LQMaxAmtUQTextBox;
		ZCalcEdit JTT_LQ2MaxAmtCalcEdit;
		ZTextBox JTT_LQ2MaxAmtUQTextBox;
		ZDropEdit JTT_ExceptedQuantityCodeDropEdit;
		ZTextBox JTT_PackInsTextBox;
		ZTextBox JTT_PackProvTextBox;
		ZTextBox JTT_MixedPackingProvTextBox;
		ZTextBox JTT_BulkTankInsTextBox;
		ZTextBox JTT_BulkTankSpecProv;
		ZTextBox JTT_TankCodeTextBox;
		ZTextBox JTT_TankSpecProv;
		ZTextBox JTT_TankVehicleTextBox;
		ZTextBox JTT_TransportCategoryTextBox;
		ZTextBox JTT_PackingSpecialProvTextBox;
		ZTextBox JTT_BulkSpecialProvTextBox;
		ZTextBox JTT_LoadingSpecialProvTextBox;
		ZTextBox JTT_OperationSpecialProvTextBox;
		ZTextBox JTT_HazardIDNumberTextBox;

		ZTemplateTabControl UNDGSubstanceTabControl;
		ZTabPage NamesTabPage;
		ZTabPage QualifyingDescriptiveTextTabPage;
		ZTabPage OtherProvisionsTabPage;
		ZGrid NamesGrid;
		ZGrid QualifyingDescriptiveTextGrid;
		ZDropEdit DetailsLanguageDropEdit;
		ZPanel DG_BorderPanel;
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
			this.JTT_BulkTankInsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JTT_BulkTankSpecProv = new Enterprise.ZArchitecture.ZTextBox();
			this.JTT_TankCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JTT_TankSpecProv = new Enterprise.ZArchitecture.ZTextBox();
			this.JTT_TankVehicleTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JTT_BulkSpecialProvTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JTT_LoadingSpecialProvTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JTT_OperationSpecialProvTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PackingProvisionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JTT_SpecialProvisionsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JTT_PackInsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JTT_PackProvTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JTT_MixedPackingProvTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JTT_PackingSpecialProvTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HeaderLabel = new Enterprise.ZArchitecture.ZLabel();
			this.IconPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsSystemDefinedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.JTT_UNNOTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JTT_VariantTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JTT_ProperShippingNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JTT_ClassDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ClassificationCode1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ClassificationCode2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JTT_PGDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JTT_Label1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JTT_Label2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JTT_Label3TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JTT_LQMaxAmtCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JTT_LQMaxAmtUQTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JTT_LQ2MaxAmtCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JTT_LQ2MaxAmtUQTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JTT_ExceptedQuantityCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JTT_TransportCategoryTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JTT_HazardIDNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
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
			this.JTT_ClassDropEdit.SuspendLayout();
			this.JTT_PGDropEdit.SuspendLayout();
			this.JTT_ExceptedQuantityCodeDropEdit.SuspendLayout();
			this.DetailsLanguageDropEdit.SuspendLayout();
			this.CountryReferencesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CountryReferencesModuleButtonGrid.InnerGrid)).BeginInit();
			this.CountryReferencesModuleButtonGrid.SuspendLayout();
			this.DG_BorderPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(UNDGSubstanceJTT);
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
			this.NamesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceJTTForm|75935a63-f6a2-483e-a714-7867a4223c22", "Names");
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstanceJTT)(null)).Names)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGAttributeZZ)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).Names)).SyncRoot)).DAZ_Language)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGAttributeZZ)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).Names)).SyncRoot)).DAZ_Descriptor)));
			this.NamesGrid.CaptionVisible = false;
			nameLanguageColumnStyleInfo.ColumnName = "DAZ_Language";
			nameLanguageColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			nameLanguageColumnStyleInfo.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesJTTForm|cd892cd1-631a-89a3-4b33-cb722dab0d2b", "Lang", "Language", "The Language of this detail.");
			nameDescriptorColumnStyleInfo.ColumnName = "DAZ_Descriptor";
			nameDescriptorColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(800);
			nameDescriptorColumnStyleInfo.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesJTTForm|7a17abf3-336d-473b-9bea-190511c48d79", "Detail");
			this.NamesGrid.ColumnStyles.Add(nameLanguageColumnStyleInfo);
			this.NamesGrid.ColumnStyles.Add(nameDescriptorColumnStyleInfo);
			this.NamesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NamesGrid.GridId = "3c676465-77c5-4975-be31-7b905c79df48";
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
			this.QualifyingDescriptiveTextTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceJTTForm|2124b7ee-9166-426a-864b-34883d9530bb", "Qualifying Descriptive Text");
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstanceJTT)(null)).QualifyingDescriptiveTexts)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGAttributeZZ)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).QualifyingDescriptiveTexts)).SyncRoot)).DAZ_Language)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGAttributeZZ)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstance)(null)).QualifyingDescriptiveTexts)).SyncRoot)).DAZ_Descriptor)));
			this.QualifyingDescriptiveTextGrid.CaptionVisible = false;
			qualifyingLanguageColumnStyleInfo.ColumnName = "DAZ_Language";
			qualifyingLanguageColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			qualifyingLanguageColumnStyleInfo.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesJTTForm|abdb4c11-e574-4428-bd47-b73a8416c607b", "Lang", "Language", "The Language of this detail.");
			qualifyingDescriptorColumnStyleInfo.ColumnName = "DAZ_Descriptor";
			qualifyingDescriptorColumnStyleInfo.MinimumEditControlWidth = 300;
			qualifyingDescriptorColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(800);
			qualifyingDescriptorColumnStyleInfo.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesJTTForm|9a299ff0-86d3-43c5-8a87-a36588e9ba3b", "Detail");
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
			this.OtherProvisionsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceJTTForm|b9d8d090-85f0-473d-83d4-1a7c6520462ed", "Other Provisions");
			this.OtherProvisionsTabPage.Controls.Add(this.JTT_BulkTankInsTextBox);
			this.OtherProvisionsTabPage.Controls.Add(this.JTT_BulkTankSpecProv);
			this.OtherProvisionsTabPage.Controls.Add(this.JTT_TankCodeTextBox);
			this.OtherProvisionsTabPage.Controls.Add(this.JTT_TankSpecProv);
			this.OtherProvisionsTabPage.Controls.Add(this.JTT_TankVehicleTextBox);
			this.OtherProvisionsTabPage.Controls.Add(this.JTT_BulkSpecialProvTextBox);
			this.OtherProvisionsTabPage.Controls.Add(this.JTT_LoadingSpecialProvTextBox);
			this.OtherProvisionsTabPage.Controls.Add(this.JTT_OperationSpecialProvTextBox);
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UNDGSubstanceJTT)(null)).DetailsLanguage)));
			this.DetailsLanguageDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesJTTForm|e9aac5d6-3b34-49fd-9756-1fca00d459cf", "Language Filter", "Filters the Names, Qualifying Descriptive Texts, Special Provisions and Stowage/Segmentation requirements to only show those that are in the language you choose.");
			this.DetailsLanguageDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 286, true);
			this.DetailsLanguageDropEdit.Name = "DetailsLanguageDropEdit";
			this.DetailsLanguageDropEdit.ShouldResizeByMaxLength = true;
			this.DetailsLanguageDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.DetailsLanguageDropEdit.TabIndex = 39;
			// 
			// JTT_BulkTankInsTextBox
			// 
			this.BindingSource.SetBindingMember(this.JTT_BulkTankInsTextBox, "JTT_BulkTankIns");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceJTT)(null)).JTT_BulkTankIns)));
			this.JTT_BulkTankInsTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesJTTForm|89609909-d92f-4866-834a-ae3dafe032fbf", "Bulk Container Instructions", "The Portable Tank/Bulk Container Instructions.");
			this.JTT_BulkTankInsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JTT_BulkTankInsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(406, 26, true);
			this.JTT_BulkTankInsTextBox.Name = "JTT_BulkTankInsTextBox";
			this.JTT_BulkTankInsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 18, true);
			this.JTT_BulkTankInsTextBox.TabIndex = 2;
			this.JTT_BulkTankInsTextBox.ReadOnly = true;
			// 
			// JTT_BulkTankSpecProv
			// 
			this.BindingSource.SetBindingMember(this.JTT_BulkTankSpecProv, "JTT_BulkTankSpecProv");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceJTT)(null)).JTT_BulkTankSpecProv)));
			this.JTT_BulkTankSpecProv.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesJTTForm|39014577-c225-42a8-ab9c-74fdea435a75", "Bulk Container Special Prov.", "The Portable Tank/Bulk Container Special Provisions.");
			this.JTT_BulkTankSpecProv.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JTT_BulkTankSpecProv.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(406, 52, true);
			this.JTT_BulkTankSpecProv.Name = "JTT_BulkTankSpecProv";
			this.JTT_BulkTankSpecProv.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 18, true);
			this.JTT_BulkTankSpecProv.TabIndex = 5;
			this.JTT_BulkTankSpecProv.ReadOnly = true;
			// 
			// JTT_TankCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.JTT_TankCodeTextBox, "JTT_TankCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceJTT)(null)).JTT_PackProv)));
			this.JTT_TankCodeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesJTTForm|4179fe6a-aab0-466a-8c3c-7715c492b02e", "JTT Tank Code");
			this.JTT_TankCodeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JTT_TankCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 52, true);
			this.JTT_TankCodeTextBox.Name = "JTT_TankCodeTextBox";
			this.JTT_TankCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 18, true);
			this.JTT_TankCodeTextBox.TabIndex = 4;
			this.JTT_TankCodeTextBox.ReadOnly = true;
			// 
			// JTT_TankSpecProv
			// 
			this.BindingSource.SetBindingMember(this.JTT_TankSpecProv, "JTT_TankSpecProv");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceJTT)(null)).JTT_TankSpecProv)));
			this.JTT_TankSpecProv.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesJTTForm|32a175e4-8065-4b95-96d1-4402815211bd", "JTT Tank Special Prov.", "JTT Tank Special Provisions", "The Special Provisions assigned to this substance concerning carriage in JTT Tanks.");
			this.JTT_TankSpecProv.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JTT_TankSpecProv.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 78, true);
			this.JTT_TankSpecProv.Name = "JTT_TankSpecProv";
			this.JTT_TankSpecProv.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(398, 18, true);
			this.JTT_TankSpecProv.TabIndex = 7;
			this.JTT_TankSpecProv.ReadOnly = true;
			// 
			// JTT_TankVehicleTextBox
			// 
			this.BindingSource.SetBindingMember(this.JTT_TankVehicleTextBox, "JTT_TankVehicle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceJTT)(null)).JTT_TankVehicle)));
			this.JTT_TankVehicleTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesJTTForm|5d9b4c66-07e4-499e-8732-f99140fd469c", "Vehicle for Tank Carriage");
			this.JTT_TankVehicleTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JTT_TankVehicleTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 26, true);
			this.JTT_TankVehicleTextBox.Name = "JTT_TankVehicleTextBox";
			this.JTT_TankVehicleTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 18, true);
			this.JTT_TankVehicleTextBox.TabIndex = 1;
			this.JTT_TankVehicleTextBox.ReadOnly = true;
			// 
			// JTT_BulkSpecialProvTextBox
			// 
			this.BindingSource.SetBindingMember(this.JTT_BulkSpecialProvTextBox, "JTT_BulkSpecialProv");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceJTT)(null)).JTT_BulkSpecialProv)));
			this.JTT_BulkSpecialProvTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesJTTForm|8ffd28fc-c2b9-4525-9361-d6ab468d1abe", "Bulk Special Prov.", "Bulk Special Provisions", "The Special Provisions assigned to this substance concerning carriage in bulk.");
			this.JTT_BulkSpecialProvTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JTT_BulkSpecialProvTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(653, 26, true);
			this.JTT_BulkSpecialProvTextBox.Name = "JTT_BulkSpecialProvTextBox";
			this.JTT_BulkSpecialProvTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 18, true);
			this.JTT_BulkSpecialProvTextBox.TabIndex = 3;
			this.JTT_BulkSpecialProvTextBox.ReadOnly = true;
			// 
			// JTT_LoadingSpecialProvTextBox
			// 
			this.BindingSource.SetBindingMember(this.JTT_LoadingSpecialProvTextBox, "JTT_LoadingSpecialProv");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceJTT)(null)).JTT_LoadingSpecialProv)));
			this.JTT_LoadingSpecialProvTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesJTTForm|89fb8841-28a3-409c-a35c-d410f69d63a8", "Loading Special Prov.", "Loading Special Provisions", "The Special Provisions assigned to this substance concerning loading.");
			this.JTT_LoadingSpecialProvTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JTT_LoadingSpecialProvTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(653, 52, true);
			this.JTT_LoadingSpecialProvTextBox.Name = "JTT_LoadingSpecialProvTextBox";
			this.JTT_LoadingSpecialProvTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 18, true);
			this.JTT_LoadingSpecialProvTextBox.TabIndex = 6;
			this.JTT_LoadingSpecialProvTextBox.ReadOnly = true;
			// 
			// JTT_OperationSpecialProvTextBox
			// 
			this.BindingSource.SetBindingMember(this.JTT_OperationSpecialProvTextBox, "JTT_OperationSpecialProv");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceJTT)(null)).JTT_OperationSpecialProv)));
			this.JTT_OperationSpecialProvTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesJTTForm|beb47746-3a6a-492f-ab75-23cbbb72278d", "Operation Special Prov.", "Operation Special Provisions", "The Special Provisions assigned to this substance concerning operations.");
			this.JTT_OperationSpecialProvTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JTT_OperationSpecialProvTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(653, 78, true);
			this.JTT_OperationSpecialProvTextBox.Name = "JTT_OperationSpecialProvTextBox";
			this.JTT_OperationSpecialProvTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 18, true);
			this.JTT_OperationSpecialProvTextBox.TabIndex = 8;
			this.JTT_OperationSpecialProvTextBox.ReadOnly = true;
			// 
			// PackingProvisionsGroupBox
			// 
			this.PackingProvisionsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceJTTForm|67cbc35f-083d-402e-94d0-bf265a1a37ac", "Packing Provisions");
			this.PackingProvisionsGroupBox.Controls.Add(this.JTT_SpecialProvisionsTextBox);
			this.PackingProvisionsGroupBox.Controls.Add(this.JTT_PackInsTextBox);
			this.PackingProvisionsGroupBox.Controls.Add(this.JTT_PackProvTextBox);
			this.PackingProvisionsGroupBox.Controls.Add(this.JTT_MixedPackingProvTextBox);
			this.PackingProvisionsGroupBox.Controls.Add(this.JTT_PackingSpecialProvTextBox);
			this.PackingProvisionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(506, 100, true);
			this.PackingProvisionsGroupBox.Name = "PackingProvisionsGroupBox";
			this.PackingProvisionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 154, true);
			this.PackingProvisionsGroupBox.TabIndex = 30;
			this.PackingProvisionsGroupBox.TabStop = false;
			// 
			// JTT_SpecialProvisionsTextBox
			// 
			this.JTT_SpecialProvisionsTextBox.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left;
			this.BindingSource.SetBindingMember(this.JTT_SpecialProvisionsTextBox, "JTT_SpecialProvisions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceJTT)(null)).JTT_SpecialProvisions)));
			this.JTT_SpecialProvisionsTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesJTTForm|89aae203-e036-4bcd-874a-d92da2024f39", "Special Provisions");
			this.JTT_SpecialProvisionsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JTT_SpecialProvisionsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 26, true);
			this.JTT_SpecialProvisionsTextBox.Name = "JTT_SpecialProvisionsTextBox";
			this.JTT_SpecialProvisionsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 18, true);
			this.JTT_SpecialProvisionsTextBox.TabIndex = 6;
			this.JTT_SpecialProvisionsTextBox.ReadOnly = true;
			// 
			// JTT_PackInsTextBox
			// 
			this.BindingSource.SetBindingMember(this.JTT_PackInsTextBox, "JTT_PackIns");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceJTT)(null)).JTT_PackIns)));
			this.JTT_PackInsTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesJTTForm|5f2a803b-c719-45b2-b165-77176d5f60af", "Packing Instruction", "The packing instruction code that relates to the transportation of this substance in limited quantities.");
			this.JTT_PackInsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JTT_PackInsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 130, true);
			this.JTT_PackInsTextBox.Name = "JTT_PackInsTextBox";
			this.JTT_PackInsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 18, true);
			this.JTT_PackInsTextBox.TabIndex = 34;
			this.JTT_PackInsTextBox.ReadOnly = true;
			// 
			// JTT_PackProvTextBox
			// 
			this.BindingSource.SetBindingMember(this.JTT_PackProvTextBox, "JTT_PackProv");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceJTT)(null)).JTT_PackProv)));
			this.JTT_PackProvTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesJTTForm|4283c523-af4e-42bb-9fd6-37bd4bda5047", "Pack Prov.", "Pack Provisions");
			this.JTT_PackProvTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JTT_PackProvTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 52, true);
			this.JTT_PackProvTextBox.Name = "JTT_PackProvTextBox";
			this.JTT_PackProvTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 18, true);
			this.JTT_PackProvTextBox.TabIndex = 31;
			this.JTT_PackProvTextBox.ReadOnly = true;
			// 
			// JTT_MixedPackingProvTextBox
			// 
			this.BindingSource.SetBindingMember(this.JTT_MixedPackingProvTextBox, "JTT_MixedPackingProv");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceJTT)(null)).JTT_MixedPackingProv)));
			this.JTT_MixedPackingProvTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesJTTForm|e3b3267d-76cc-4728-9512-9b9df4ce0420", "Mixed Packing Provisions");
			this.JTT_MixedPackingProvTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JTT_MixedPackingProvTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 78, true);
			this.JTT_MixedPackingProvTextBox.Name = "JTT_MixedPackingProvTextBox";
			this.JTT_MixedPackingProvTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 18, true);
			this.JTT_MixedPackingProvTextBox.TabIndex = 32;
			this.JTT_MixedPackingProvTextBox.ReadOnly = true;
			// 
			// JTT_PackingSpecialProvTextBox
			// 
			this.BindingSource.SetBindingMember(this.JTT_PackingSpecialProvTextBox, "JTT_PackingSpecialProv");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceJTT)(null)).JTT_PackingSpecialProv)));
			this.JTT_PackingSpecialProvTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesJTTForm|ea785332-8e33-416f-ae1b-7d9943152677", "Packing Special Provisions");
			this.JTT_PackingSpecialProvTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JTT_PackingSpecialProvTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 104, true);
			this.JTT_PackingSpecialProvTextBox.Name = "JTT_PackingSpecialProvTextBox";
			this.JTT_PackingSpecialProvTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 18, true);
			this.JTT_PackingSpecialProvTextBox.TabIndex = 33;
			this.JTT_PackingSpecialProvTextBox.ReadOnly = true;
			// 
			// HeaderLabel
			// 
			this.HeaderLabel.AutoSize = true;
			this.HeaderLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesJTTForm|9e462c03-24f5-4c11-a47f-534c0f236955", "ROAD FREIGHT (JTT)");
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
			this.BindingSource.SetBindingMember(this.IsActiveCheckBox, "JTT_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.UNDGSubstanceJTT)(null)).JTT_IsActive)));
			this.IsActiveCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceJTTForm|c8e15add-fbcc-4b94-8919-40f04b6cf39f", "Active");
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
			this.BindingSource.SetBindingMember(this.IsSystemDefinedCheckBox, "JTT_IsSystemDefined");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.UNDGSubstanceJTT)(null)).JTT_IsSystemDefined)));
			this.IsSystemDefinedCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceJTTForm|6425cbe2-8d37-4d5d-b803-429e99bdee81", "System Defined");
			this.IsSystemDefinedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsSystemDefinedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsSystemDefinedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(805, 55, true);
			this.IsSystemDefinedCheckBox.Name = "IsSystemDefinedCheckBox";
			this.IsSystemDefinedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 14, true);
			this.IsSystemDefinedCheckBox.TabIndex = 8;
			this.IsSystemDefinedCheckBox.ReadOnly = true;
			// 
			// JTT_UNNOTextBox
			// 
			this.BindingSource.SetBindingMember(this.JTT_UNNOTextBox, "JTT_UNNO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceJTT)(null)).JTT_UNNO)));
			this.JTT_UNNOTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceJTTForm|c7f825d4-3c48-43b6-9d54-e17008789043", "UN No.", "UN No.", "The United Nations (UN) number for this substance.");
			this.JTT_UNNOTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 52, true);
			this.JTT_UNNOTextBox.Name = "JTT_UNNOTextBox";
			this.JTT_UNNOTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 18, true);
			this.JTT_UNNOTextBox.TabIndex = 0;
			this.JTT_UNNOTextBox.ReadOnly = true;
			// 
			// JTT_VariantTextBox
			// 
			this.BindingSource.SetBindingMember(this.JTT_VariantTextBox, "JTT_Variant");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceJTT)(null)).JTT_Variant)));
			this.JTT_VariantTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceJTTForm|d79a7825-cce7-49ba-a296-4670bc5238c7", "Variant");
			this.JTT_VariantTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 52, true);
			this.JTT_VariantTextBox.Name = "JTT_VariantTextBox";
			this.JTT_VariantTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 18, true);
			this.JTT_VariantTextBox.TabIndex = 1;
			this.JTT_VariantTextBox.ReadOnly = true;
			// 
			// JTT_ProperShippingNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.JTT_ProperShippingNameTextBox, "JTT_PSN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceJTT)(null)).JTT_PSN)));
			this.JTT_ProperShippingNameTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceJTTForm|f4065db1-92b4-47b6-b738-80d17a2e8ee8", "PSN", "Proper Shipping Name", "The Proper Shipping Name for this substance.");
			this.JTT_ProperShippingNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JTT_ProperShippingNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 78, true);
			this.JTT_ProperShippingNameTextBox.Name = "JTT_ProperShippingNameTextBox";
			this.JTT_ProperShippingNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 18, true);
			this.JTT_ProperShippingNameTextBox.TabIndex = 10;
			this.JTT_ProperShippingNameTextBox.ReadOnly = true;
			// 
			// JTT_ClassDropEdit
			// 
			this.JTT_ClassDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JTT_ClassDropEdit, "JTT_Class");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UNDGSubstanceJTT)(null)).JTT_Class)));
			this.JTT_ClassDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceJTTForm|c98a6299-c4d8-48d8-8f81-c675c798e23c", "Class", "The JTT Class number assigned to this substance.");
			this.JTT_ClassDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(314, 52, true);
			this.JTT_ClassDropEdit.Name = "JTT_ClassDropEdit";
			this.JTT_ClassDropEdit.PreBoundMaxLength = 4;
			this.JTT_ClassDropEdit.ShouldResizeByMaxLength = true;
			this.JTT_ClassDropEdit.ShowDescriptionBox = false;
			this.JTT_ClassDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 18, true);
			this.JTT_ClassDropEdit.TabIndex = 2;
			this.JTT_ClassDropEdit.ReadOnly = true;
			// 
			// ClassificationCode1TextBox
			// 
			this.BindingSource.SetBindingMember(this.ClassificationCode1TextBox, "ClassificationCode1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceJTT)(null)).JTT_ClassificationCode)));
			this.ClassificationCode1TextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesJTTForm|a6f16cf8-cdf8-474f-b83e-833b8d760f84", "Class. Code", "The Classification Code assigned to this substance.");
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceJTT)(null)).ClassificationCode2)));
			this.ClassificationCode2TextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceJTTForm|767a1a5d-29eb-43f3-bb1b-60ab59fc8cca", "Class. Code 2");
			this.ClassificationCode2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ClassificationCode2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(457, 52, true);
			this.ClassificationCode2TextBox.Name = "ClassificationCode2TextBox";
			this.ClassificationCode2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 18, true);
			this.ClassificationCode2TextBox.TabIndex = 4;
			this.ClassificationCode2TextBox.ReadOnly = true;
			// 
			// JTT_PGDropEdit
			// 
			this.JTT_PGDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JTT_PGDropEdit, "JTT_PG");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UNDGSubstanceJTT)(null)).JTT_PG)));
			this.JTT_PGDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesJTTForm|66ceefa5-8915-4956-ae86-8621e6cc5c2c", "PG", "Packing Group", "The UN Packing Group (either I, II or III).");
			this.JTT_PGDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(575, 52, true);
			this.JTT_PGDropEdit.Name = "JTT_PGDropEdit";
			this.JTT_PGDropEdit.PreBoundMaxLength = 4;
			this.JTT_PGDropEdit.ShouldResizeByMaxLength = true;
			this.JTT_PGDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 18, true);
			this.JTT_PGDropEdit.TabIndex = 5;
			this.JTT_PGDropEdit.ReadOnly = true;
			// 
			// JTT_LabelsTextBox
			// 
			this.JTT_Label1TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.JTT_Label1TextBox, "JTT_Label1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceJTT)(null)).JTT_Label1)));
			this.JTT_Label1TextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesJTTForm|6fa9a4f8-9338-4b6c-ba77-9db88e434db8", "Label(s)", "The subsidiary hazard label(s) assigned to this substance.");
			this.JTT_Label1TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JTT_Label1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 130, true);
			this.JTT_Label1TextBox.Name = "JTT_Label1TextBox";
			this.JTT_Label1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 18, true);
			this.JTT_Label1TextBox.TabIndex = 11;
			this.JTT_Label1TextBox.ReadOnly = true;
			// 
			// JTT_LabelsTextBox
			// 
			this.JTT_Label2TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.JTT_Label2TextBox, "JTT_Label2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceJTT)(null)).JTT_Label2)));
			this.JTT_Label2TextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JTT_Label2TextBox, false);
			this.JTT_Label2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JTT_Label2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 130, true);
			this.JTT_Label2TextBox.Name = "JTT_Label2TextBox";
			this.JTT_Label2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 18, true);
			this.JTT_Label2TextBox.TabIndex = 12;
			this.JTT_Label2TextBox.ReadOnly = true;
			// 
			// JTT_LabelsTextBox
			// 
			this.JTT_Label3TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.JTT_Label3TextBox, "JTT_Label3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceJTT)(null)).JTT_Label3)));
			this.JTT_Label3TextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JTT_Label3TextBox, false);
			this.JTT_Label3TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JTT_Label3TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 130, true);
			this.JTT_Label3TextBox.Name = "JTT_Label3TextBox";
			this.JTT_Label3TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 18, true);
			this.JTT_Label3TextBox.TabIndex = 13;
			this.JTT_Label3TextBox.ReadOnly = true;
			// 
			// JTT_LQMaxAmtCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JTT_LQMaxAmtCalcEdit, "JTT_LQMaxAmt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.UNDGSubstanceJTT)(null)).JTT_LQMaxAmt)));
			this.JTT_LQMaxAmtCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesJTTForm|133fb272-53af-4f7e-be95-aa2cc6914f06", "Limited Quantity", "The Limited Quantity Code assigned to this substance.");
			this.JTT_LQMaxAmtCalcEdit.DecimalPlaces = 2;
			this.JTT_LQMaxAmtCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 156, true);
			this.JTT_LQMaxAmtCalcEdit.Name = "JTT_LQMaxAmtCalcEdit";
			this.JTT_LQMaxAmtCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 18, true);
			this.JTT_LQMaxAmtCalcEdit.TabIndex = 15;
			this.JTT_LQMaxAmtCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.JTT_LQMaxAmtCalcEdit.ReadOnly = true;
			// 
			// JTT_LQMaxAmtUQTextBox
			// 
			this.BindingSource.SetBindingMember(this.JTT_LQMaxAmtUQTextBox, "JTT_LQMaxAmtUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceJTT)(null)).JTT_LQMaxAmtUQ)));
			this.JTT_LQMaxAmtUQTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesJTTForm|6d01cfda-c694-45ec-a7f4-d45d6d290a8f", "Units");
			this.JTT_LQMaxAmtUQTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JTT_LQMaxAmtUQTextBox, false);
			this.JTT_LQMaxAmtUQTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 156, true);
			this.JTT_LQMaxAmtUQTextBox.Name = "JTT_LQMaxAmtUQTextBox";
			this.JTT_LQMaxAmtUQTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 18, true);
			this.JTT_LQMaxAmtUQTextBox.TabIndex = 16;
			this.JTT_LQMaxAmtUQTextBox.ReadOnly = true;
			// 
			// JTT_LQ2MaxAmtCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JTT_LQ2MaxAmtCalcEdit, "JTT_LQ2MaxAmt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.UNDGSubstanceJTT)(null)).JTT_LQ2MaxAmt)));
			this.JTT_LQ2MaxAmtCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesJTTForm|1f2dd799-5f04-48a1-8916-211a14b17435", "Limited Quantity 2", "The Limited Quantity Code assigned to this substance.");
			this.JTT_LQ2MaxAmtCalcEdit.DecimalPlaces = 2;
			this.JTT_LQ2MaxAmtCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 182, true);
			this.JTT_LQ2MaxAmtCalcEdit.Name = "JTT_LQ2MaxAmtCalcEdit";
			this.JTT_LQ2MaxAmtCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 18, true);
			this.JTT_LQ2MaxAmtCalcEdit.TabIndex = 17;
			this.JTT_LQ2MaxAmtCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.JTT_LQ2MaxAmtCalcEdit.ReadOnly = true;
			// 
			// JTT_LQ2MaxAmtUQTextBox
			// 
			this.BindingSource.SetBindingMember(this.JTT_LQ2MaxAmtUQTextBox, "JTT_LQ2MaxAmtUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceJTT)(null)).JTT_LQ2MaxAmtUQ)));
			this.JTT_LQ2MaxAmtUQTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesJTTForm|63f4b3cd-1202-4be9-b7f4-0246006c1665", "Units");
			this.JTT_LQ2MaxAmtUQTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JTT_LQ2MaxAmtUQTextBox, false);
			this.JTT_LQ2MaxAmtUQTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 182, true);
			this.JTT_LQ2MaxAmtUQTextBox.Name = "JTT_LQ2MaxAmtUQTextBox";
			this.JTT_LQ2MaxAmtUQTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 18, true);
			this.JTT_LQ2MaxAmtUQTextBox.TabIndex = 18;
			this.JTT_LQ2MaxAmtUQTextBox.ReadOnly = true;
			// 
			// JTT_HazardIDNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.JTT_HazardIDNumberTextBox, "JTT_HazardIDNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceJTT)(null)).JTT_HazardIDNumber)));
			this.JTT_HazardIDNumberTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesJTTForm|f4d83c78-31d1-40d6-adb8-539b158202f2", "Hazard ID Number", "The Hazard Identification Number identifies the substance's hazard qualities.");
			this.JTT_HazardIDNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JTT_HazardIDNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 208, true);
			this.JTT_HazardIDNumberTextBox.Name = "JTT_HazardIDNumberTextBox";
			this.JTT_HazardIDNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 18, true);
			this.JTT_HazardIDNumberTextBox.TabIndex = 19;
			this.JTT_HazardIDNumberTextBox.ReadOnly = true;
			// 
			// JTT_TransportCategoryTextBox
			// 
			this.BindingSource.SetBindingMember(this.JTT_TransportCategoryTextBox, "JTT_TransportCategory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceJTT)(null)).JTT_TransportCategory)));
			this.JTT_TransportCategoryTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesJTTForm|9993f8b2-88dd-40c8-95cf-bb1defd1d868", "Tunnel Code", "The Transport Category (Tunnel Restriction) code assigned to this substance.");
			this.JTT_TransportCategoryTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JTT_TransportCategoryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 234, true);
			this.JTT_TransportCategoryTextBox.Name = "JTT_TransportCategoryTextBox";
			this.JTT_TransportCategoryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 18, true);
			this.JTT_TransportCategoryTextBox.TabIndex = 20;
			this.JTT_TransportCategoryTextBox.ReadOnly = true;
			// 
			// JTT_ExceptedQuantityCodeDropEdit
			// 
			this.JTT_ExceptedQuantityCodeDropEdit.AllowDrop = true;
			this.JTT_ExceptedQuantityCodeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.JTT_ExceptedQuantityCodeDropEdit, "JTT_ExceptedQuantityCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UNDGSubstanceJTT)(null)).JTT_ExceptedQuantityCode)));
			this.JTT_ExceptedQuantityCodeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesJTTForm|8238f676-648e-42b9-ac9e-ed7881707cad", "Excepted Quantity", "The Excepted Quantity Code assigned to this substance.");
			this.JTT_ExceptedQuantityCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 260, true);
			this.JTT_ExceptedQuantityCodeDropEdit.Name = "JTT_ExceptedQuantityCodeDropEdit";
			this.JTT_ExceptedQuantityCodeDropEdit.PreBoundMaxLength = 4;
			this.JTT_ExceptedQuantityCodeDropEdit.ShouldResizeByMaxLength = true;
			this.JTT_ExceptedQuantityCodeDropEdit.ShowDescriptionBox = true;
			this.JTT_ExceptedQuantityCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 18, true);
			this.JTT_ExceptedQuantityCodeDropEdit.TabIndex = 21;
			this.JTT_ExceptedQuantityCodeDropEdit.ReadOnly = true;
			//
			// CountryReferencesTabPage
			//
			this.CountryReferencesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesJTTForm|51258054-4e94-4703-868d-8738152e55d7", "Country/Region References");
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
			this.CountryReferencesModuleButtonGrid.GridId = "78d5c797-4581-4af0-a398-d133e12cf104";
			this.CountryReferencesModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CountryReferencesModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(905, 183, true);
			// 
			// UNDGSubstanceJTTControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.JTT_ExceptedQuantityCodeDropEdit);
			this.Controls.Add(this.PackingProvisionsGroupBox);
			this.Controls.Add(this.UNDGSubstanceTabControl);
			this.Controls.Add(this.JTT_Label1TextBox);
			this.Controls.Add(this.JTT_Label2TextBox);
			this.Controls.Add(this.JTT_Label3TextBox);
			this.Controls.Add(this.JTT_PGDropEdit);
			this.Controls.Add(this.ClassificationCode1TextBox);
			this.Controls.Add(this.ClassificationCode2TextBox);
			this.Controls.Add(this.JTT_ClassDropEdit);
			this.Controls.Add(this.JTT_ProperShippingNameTextBox);
			this.Controls.Add(this.JTT_VariantTextBox);
			this.Controls.Add(this.JTT_UNNOTextBox);
			this.Controls.Add(this.HeaderLabel);
			this.Controls.Add(this.IconPictureBox);
			this.Controls.Add(this.JTT_LQMaxAmtCalcEdit);
			this.Controls.Add(this.JTT_LQMaxAmtUQTextBox);
			this.Controls.Add(this.JTT_LQ2MaxAmtCalcEdit);
			this.Controls.Add(this.JTT_LQ2MaxAmtUQTextBox);
			this.Controls.Add(this.JTT_TransportCategoryTextBox);
			this.Controls.Add(this.JTT_HazardIDNumberTextBox);
			this.Controls.Add(this.IsActiveCheckBox);
			this.Controls.Add(this.IsSystemDefinedCheckBox);
			this.Controls.Add(this.DetailsLanguageDropEdit);
			this.Controls.Add(this.DG_BorderPanel);
			this.DataSourceAssemblyName = "Enterprise.MasterFiles.Business";
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.UNDGSubstanceJTT);
			this.DataSourceTypeName = "Enterprise.MasterFiles.Business.UNDGSubstanceJTT";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(922, 583, true);
			this.Name = "UNDGSubstanceJTTControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.JTT_HazardIDNumberTextBox, 0);
			this.Controls.SetChildIndex(this.JTT_TransportCategoryTextBox, 0);
			this.Controls.SetChildIndex(this.JTT_LQ2MaxAmtUQTextBox, 0);
			this.Controls.SetChildIndex(this.JTT_LQ2MaxAmtCalcEdit, 0);
			this.Controls.SetChildIndex(this.JTT_LQMaxAmtUQTextBox, 0);
			this.Controls.SetChildIndex(this.JTT_LQMaxAmtCalcEdit, 0);
			this.Controls.SetChildIndex(this.IconPictureBox, 0);
			this.Controls.SetChildIndex(this.HeaderLabel, 0);
			this.Controls.SetChildIndex(this.JTT_UNNOTextBox, 0);
			this.Controls.SetChildIndex(this.JTT_VariantTextBox, 0);
			this.Controls.SetChildIndex(this.JTT_ProperShippingNameTextBox, 0);
			this.Controls.SetChildIndex(this.JTT_ClassDropEdit, 0);
			this.Controls.SetChildIndex(this.ClassificationCode1TextBox, 0);
			this.Controls.SetChildIndex(this.ClassificationCode2TextBox, 0);
			this.Controls.SetChildIndex(this.JTT_PGDropEdit, 0);
			this.Controls.SetChildIndex(this.JTT_Label1TextBox, 0);
			this.Controls.SetChildIndex(this.JTT_Label2TextBox, 0);
			this.Controls.SetChildIndex(this.JTT_Label3TextBox, 0);
			this.Controls.SetChildIndex(this.DetailsLanguageDropEdit, 0);
			this.Controls.SetChildIndex(this.UNDGSubstanceTabControl, 0);
			this.Controls.SetChildIndex(this.PackingProvisionsGroupBox, 0);
			this.Controls.SetChildIndex(this.JTT_ExceptedQuantityCodeDropEdit, 0);
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
			this.JTT_ClassDropEdit.ResumeLayout(true);
			this.JTT_ClassDropEdit.PerformLayout();
			this.JTT_PGDropEdit.ResumeLayout(true);
			this.JTT_PGDropEdit.PerformLayout();
			this.DetailsLanguageDropEdit.ResumeLayout(true);
			this.DetailsLanguageDropEdit.PerformLayout();
			this.JTT_ExceptedQuantityCodeDropEdit.ResumeLayout(true);
			this.JTT_ExceptedQuantityCodeDropEdit.PerformLayout();
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
