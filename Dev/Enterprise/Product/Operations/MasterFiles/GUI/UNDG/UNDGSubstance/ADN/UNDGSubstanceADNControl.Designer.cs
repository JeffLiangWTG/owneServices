using System.Drawing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class UNDGSubstanceADNControl
	{
		ZLabel HeaderLabel;
		KPictureBox IconPictureBox;
		ZCheckBox IsActiveCheckBox;
		ZCheckBox IsSystemDefinedCheckBox;
		ZTextBox ADN_UNNOTextBox;
		ZTextBox ADN_VariantTextBox;
		ZTextBox ADN_ProperShippingNameTextBox;
		ZDropEdit ADN_ClassDropEdit;
		ZTextBox ClassificationCode1TextBox;
		ZTextBox ClassificationCode2TextBox;
		ZDropEdit ADN_PGDropEdit;
		ZTextBox ADN_Label1TextBox;
		ZTextBox ADN_Label2TextBox;
		ZTextBox ADN_Label3TextBox;
		ZTextBox ADN_Label4TextBox;
		ZTextBox ADN_SpecialProvisionsTextBox;
		ZCalcEdit ADN_LQMaxAmtCalcEdit;
		ZTextBox ADN_LQMaxAmtUQTextBox;
		ZCalcEdit ADN_LQ2MaxAmtCalcEdit;
		ZTextBox ADN_LQ2MaxAmtUQTextBox;
		ZDropEdit ADN_ExceptedQuantityCodeDropEdit;
		ZTemplateTabControl UNDGSubstanceTabControl;
		ZTabPage NamesTabPage;
		ZTabPage QualifyingDescriptiveTextTabPage;
		ZTabPage OtherProvisionsTabPage;
		ZGrid NamesGrid;
		ZGrid QualifyingDescriptiveTextGrid;
		ZDropEdit DetailsLanguageDropEdit;
		ZGroupBox CarriagePermittedGroupBox;
		ZGroupBox SpecialProvisionsGroupBox;
		ZTextBox ADN_CarriagePermittedDetailsTextBox;
		ZCheckBox ADN_CarriagePermittedTanksCheckBox;
		ZCheckBox ADN_CarriagePermittedBulkCheckBox;
		ZCheckBox ADN_CarriagePermittedPacksCheckBox;
		ZGroupBox EquipmentGroupBox;
		ZTextBox ADN_EquipmentDetailsTextBox;
		ZCheckBox ADN_EquipBreathingAppCheckBox;
		ZCheckBox ADN_EquipToximeterCheckBox;
		ZCheckBox ADN_EquipGasDetectorCheckBox;
		ZCheckBox ADN_EquipEscapeDevCheckBox;
		ZCheckBox ADN_EquipPPECheckBox;
		ZTextBox ADN_VentilationTextBox;
		ZTextBox ADN_OperatingSpecialProvNoteTextBox;
		ZTextBox ADN_UnloadingSpecialProvNoteTextBox;
		ZTextBox ADN_LoadingSpecialProvNoteTextBox;
		ZTextBox ADN_OperatingSpecialProvTextBox;
		ZTextBox ADN_UnloadingSpecialProvTextBox;
		ZTextBox ADN_LoadingSpecialProvTextBox;
		ZTextBox ADN_BlueConesTextBox;
		ZPanel DG_BorderPanel;
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
			this.UNDGSubstanceTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.NamesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.NamesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.QualifyingDescriptiveTextTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.QualifyingDescriptiveTextGrid = new Enterprise.ZArchitecture.ZGrid();
			this.OtherProvisionsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ADN_OperatingSpecialProvNoteTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADN_UnloadingSpecialProvNoteTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADN_LoadingSpecialProvNoteTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADN_OperatingSpecialProvTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADN_UnloadingSpecialProvTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADN_LoadingSpecialProvTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADN_BlueConesTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADN_VentilationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CarriagePermittedGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SpecialProvisionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ADN_CarriagePermittedDetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADN_CarriagePermittedTanksCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ADN_CarriagePermittedBulkCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ADN_CarriagePermittedPacksCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ADN_SpecialProvisionsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HeaderLabel = new Enterprise.ZArchitecture.ZLabel();
			this.IconPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsSystemDefinedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ADN_UNNOTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADN_VariantTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADN_ProperShippingNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADN_ClassDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ClassificationCode1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ClassificationCode2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADN_PGDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ADN_Label1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADN_Label2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADN_Label3TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADN_Label4TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADN_LQMaxAmtCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ADN_LQMaxAmtUQTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADN_LQ2MaxAmtCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ADN_LQ2MaxAmtUQTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADN_ExceptedQuantityCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DetailsLanguageDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EquipmentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ADN_EquipmentDetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ADN_EquipBreathingAppCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ADN_EquipToximeterCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ADN_EquipGasDetectorCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ADN_EquipEscapeDevCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ADN_EquipPPECheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CountryReferencesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CountryReferencesModuleButtonGrid = new CountryReferencesModuleButtonGrid();
			this.DG_BorderPanel = new ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.UNDGSubstanceTabControl.SuspendLayout();
			this.NamesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.NamesGrid)).BeginInit();
			this.NamesGrid.SuspendLayout();
			this.QualifyingDescriptiveTextTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.QualifyingDescriptiveTextGrid)).BeginInit();
			this.QualifyingDescriptiveTextGrid.SuspendLayout();
			this.OtherProvisionsTabPage.SuspendLayout();
			this.CarriagePermittedGroupBox.SuspendLayout();
			this.SpecialProvisionsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.IconPictureBox)).BeginInit();
			this.ADN_ClassDropEdit.SuspendLayout();
			this.ADN_PGDropEdit.SuspendLayout();
			this.ADN_ExceptedQuantityCodeDropEdit.SuspendLayout();
			this.DetailsLanguageDropEdit.SuspendLayout();
			this.EquipmentGroupBox.SuspendLayout();
			this.CountryReferencesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CountryReferencesModuleButtonGrid.InnerGrid)).BeginInit();
			this.CountryReferencesModuleButtonGrid.SuspendLayout();
			this.DG_BorderPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.UNDGSubstanceADN);
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
			this.UNDGSubstanceTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(806, 219, true);
			this.UNDGSubstanceTabControl.TabIndex = 40;
			// 
			// NamesTabPage
			// 
			this.NamesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADNForm|3045a839-61fd-16a1-4a47-03b38cef06e8", "Names");
			this.NamesTabPage.Controls.Add(this.NamesGrid);
			this.NamesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.NamesTabPage.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(545, 175, true);
			this.NamesTabPage.Name = "NamesTabPage";
			this.NamesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(798, 192, true);
			this.NamesTabPage.TabIndex = 1;
			// 
			// NamesGrid
			// 
			this.NamesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.NamesGrid, "Names");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).Names)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGAttributeZZ)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).Names)).SyncRoot)).DAZ_Language)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGAttributeZZ)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).Names)).SyncRoot)).DAZ_Descriptor)));
			this.NamesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADNForm|cd892cd1-631a-89a3-4b33-cb722dab0d2b", "Lang", "Language", "The Language of this detail.");
			zDropEditColumnStyleInfo1.ColumnName = "DAZ_Language";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADNForm|5b719e01-df95-3681-48c9-5f83ce974550", "Detail");
			zTextBoxColumnStyleInfo1.ColumnName = "DAZ_Descriptor";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(800);
			this.NamesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.NamesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.NamesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NamesGrid.GridId = "00115742-3f0c-7599-4b71-cd3cdb803d73";
			this.NamesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.NamesGrid.LayoutKey = "zGrid3";
			this.NamesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NamesGrid.Name = "NamesGrid";
			this.NamesGrid.ReadOnly = true;
			this.NamesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(798, 192, true);
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
			this.QualifyingDescriptiveTextTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADNForm|9b9a7f19-ebd1-ef8e-4485-b9ead673fd19", "Qualifying Descriptive Text");
			this.QualifyingDescriptiveTextTabPage.Controls.Add(this.QualifyingDescriptiveTextGrid);
			this.QualifyingDescriptiveTextTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.QualifyingDescriptiveTextTabPage.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(545, 175, true);
			this.QualifyingDescriptiveTextTabPage.Name = "QualifyingDescriptiveTextTabPage";
			this.QualifyingDescriptiveTextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(798, 192, true);
			this.QualifyingDescriptiveTextTabPage.TabIndex = 2;
			// 
			// QualifyingDescriptiveTextGrid
			// 
			this.QualifyingDescriptiveTextGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.QualifyingDescriptiveTextGrid, "QualifyingDescriptiveTexts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).QualifyingDescriptiveTexts)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGAttributeZZ)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).QualifyingDescriptiveTexts)).SyncRoot)).DAZ_Language)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGAttributeZZ)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).QualifyingDescriptiveTexts)).SyncRoot)).DAZ_Descriptor)));
			this.QualifyingDescriptiveTextGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADNForm|cd892cd1-631a-89a3-4b33-cb722dab0d2b", "Lang", "Language", "The Language of this detail.");
			zDropEditColumnStyleInfo2.ColumnName = "DAZ_Language";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zMultiLineTextBoxColumnInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADNForm|5b719e01-df95-3681-48c9-5f83ce974550", "Detail");
			zMultiLineTextBoxColumnInfo1.ColumnName = "DAZ_Descriptor";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(800);
			this.QualifyingDescriptiveTextGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.QualifyingDescriptiveTextGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.QualifyingDescriptiveTextGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.QualifyingDescriptiveTextGrid.GridId = "9b9bdb6c-62f2-6c98-4a81-bebf16a402eb";
			this.QualifyingDescriptiveTextGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.QualifyingDescriptiveTextGrid.LayoutKey = "zGrid2";
			this.QualifyingDescriptiveTextGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.QualifyingDescriptiveTextGrid.Name = "QualifyingDescriptiveTextGrid";
			this.QualifyingDescriptiveTextGrid.ReadOnly = true;
			this.QualifyingDescriptiveTextGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(798, 192, true);
			this.QualifyingDescriptiveTextGrid.TabIndex = 0;
			// 
			// OtherProvisionsTabPage
			// 
			this.OtherProvisionsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADNForm|63abf686-4ff9-ae94-49bc-be772ef18282", "Other Provisions");
			this.OtherProvisionsTabPage.Controls.Add(this.SpecialProvisionsGroupBox);
			this.OtherProvisionsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.OtherProvisionsTabPage.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(545, 175, true);
			this.OtherProvisionsTabPage.Name = "OtherProvisionsTabPage";
			this.OtherProvisionsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.OtherProvisionsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(798, 192, true);
			this.OtherProvisionsTabPage.TabIndex = 3;
			// 
			// ADN_OperatingSpecialProvNoteTextBox
			// 
			this.BindingSource.SetBindingMember(this.ADN_OperatingSpecialProvNoteTextBox, "ADN_OperationSpecialProvNote");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ADN_OperationSpecialProvNote)));
			this.ADN_OperatingSpecialProvNoteTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADRForm|25b75bd7-78e8-f894-4735-d2f042079b46", "Contains additional requirements or observations concerning the carriage of this substance.");
			this.ADN_OperatingSpecialProvNoteTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ADN_OperatingSpecialProvNoteTextBox, false);
			this.ADN_OperatingSpecialProvNoteTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(226, 88, true);
			this.ADN_OperatingSpecialProvNoteTextBox.Name = "ADN_OperatingSpecialProvNoteTextBox";
			this.ADN_OperatingSpecialProvNoteTextBox.ReadOnly = true;
			this.ADN_OperatingSpecialProvNoteTextBox.Multiline = true;
			this.ADN_OperatingSpecialProvNoteTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 35, true);
			this.ADN_OperatingSpecialProvNoteTextBox.TabIndex = 18;
			// 
			// ADN_UnloadingSpecialProvNoteTextBox
			// 
			this.BindingSource.SetBindingMember(this.ADN_UnloadingSpecialProvNoteTextBox, "ADN_UnloadingSpecialProvNote");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ADN_UnloadingSpecialProvNote)));
			this.ADN_UnloadingSpecialProvNoteTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADRForm|b2fbc93c-52f7-c7b0-489e-87e01209fb93", "Contains additional requirements or observations concerning the unloading of this substance.");
			this.ADN_UnloadingSpecialProvNoteTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ADN_UnloadingSpecialProvNoteTextBox, false);
			this.ADN_UnloadingSpecialProvNoteTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(226, 52, true);
			this.ADN_UnloadingSpecialProvNoteTextBox.Name = "ADN_UnloadingSpecialProvNoteTextBox";
			this.ADN_UnloadingSpecialProvNoteTextBox.ReadOnly = true;
			this.ADN_UnloadingSpecialProvNoteTextBox.Multiline = true;
			this.ADN_UnloadingSpecialProvNoteTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 35, true);
			this.ADN_UnloadingSpecialProvNoteTextBox.TabIndex = 17;
			// 
			// ADN_LoadingSpecialProvNoteTextBox
			// 
			this.BindingSource.SetBindingMember(this.ADN_LoadingSpecialProvNoteTextBox, "ADN_LoadingSpecialProvNote");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ADN_LoadingSpecialProvNote)));
			this.ADN_LoadingSpecialProvNoteTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADRForm|fbf8028a-4854-b7b0-4a30-0b778d60b013", "Contains additional requirements or observations concerning the loading of this substance.");
			this.ADN_LoadingSpecialProvNoteTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ADN_LoadingSpecialProvNoteTextBox, false);
			this.ADN_LoadingSpecialProvNoteTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(226, 16, true);
			this.ADN_LoadingSpecialProvNoteTextBox.Name = "ADN_LoadingSpecialProvNoteTextBox";
			this.ADN_LoadingSpecialProvNoteTextBox.ReadOnly = true;
			this.ADN_LoadingSpecialProvNoteTextBox.Multiline = true;
			this.ADN_LoadingSpecialProvNoteTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 35, true);
			this.ADN_LoadingSpecialProvNoteTextBox.TabIndex = 16;
			// 
			// SpecialProvisionsGroupBox
			// 
			this.SpecialProvisionsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADNForm|417f2387-be61-1c94-4f95-4435fd664912", "Special Provisions");
			this.SpecialProvisionsGroupBox.Controls.Add(this.ADN_LoadingSpecialProvTextBox);
			this.SpecialProvisionsGroupBox.Controls.Add(this.ADN_UnloadingSpecialProvTextBox);
			this.SpecialProvisionsGroupBox.Controls.Add(this.ADN_OperatingSpecialProvTextBox);
			this.SpecialProvisionsGroupBox.Controls.Add(this.ADN_LoadingSpecialProvNoteTextBox);
			this.SpecialProvisionsGroupBox.Controls.Add(this.ADN_UnloadingSpecialProvNoteTextBox);
			this.SpecialProvisionsGroupBox.Controls.Add(this.ADN_OperatingSpecialProvNoteTextBox);
			this.SpecialProvisionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 5, true);
			this.SpecialProvisionsGroupBox.Name = "SpecialProvisionsGroupBox";
			this.SpecialProvisionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 165, true);
			this.SpecialProvisionsGroupBox.TabIndex = 30;
			this.SpecialProvisionsGroupBox.TabStop = false;
			// 
			// ADN_OperatingSpecialProvTextBox
			// 
			this.BindingSource.SetBindingMember(this.ADN_OperatingSpecialProvTextBox, "ADN_OperationSpecialProv");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ADN_OperationSpecialProv)));
			this.ADN_OperatingSpecialProvTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADNForm|f90b85c1-b0ba-c481-4662-91b215150db5", "Operation Spec. Prov.", "Operation Special Provisions", "Code(s) of the special requirements applicable to carriage of this substance.");
			this.ADN_OperatingSpecialProvTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ADN_OperatingSpecialProvTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 88, true);
			this.ADN_OperatingSpecialProvTextBox.Name = "ADN_OperatingSpecialProvTextBox";
			this.ADN_OperatingSpecialProvTextBox.ReadOnly = true;
			this.ADN_OperatingSpecialProvTextBox.Multiline = true;
			this.ADN_OperatingSpecialProvTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 35, true);
			this.ADN_OperatingSpecialProvTextBox.TabIndex = 15;
			// 
			// ADN_UnloadingSpecialProvTextBox
			// 
			this.BindingSource.SetBindingMember(this.ADN_UnloadingSpecialProvTextBox, "ADN_UnloadingSpecialProv");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ADN_UnloadingSpecialProv)));
			this.ADN_UnloadingSpecialProvTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADNForm|c4661d09-0511-68a7-48b6-7e6128afa731", "Unloading Spec. Prov.", "Unloading Special Provisions", "Code(s) of the special requirements applicable to unloading of this substance.");
			this.ADN_UnloadingSpecialProvTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ADN_UnloadingSpecialProvTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 52, true);
			this.ADN_UnloadingSpecialProvTextBox.Name = "ADN_UnloadingSpecialProvTextBox";
			this.ADN_UnloadingSpecialProvTextBox.ReadOnly = true;
			this.ADN_UnloadingSpecialProvTextBox.Multiline = true;
			this.ADN_UnloadingSpecialProvTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 35, true);
			this.ADN_UnloadingSpecialProvTextBox.TabIndex = 14;
			// 
			// ADN_LoadingSpecialProvTextBox
			// 
			this.BindingSource.SetBindingMember(this.ADN_LoadingSpecialProvTextBox, "ADN_LoadingSpecialProv");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ADN_LoadingSpecialProv)));
			this.ADN_LoadingSpecialProvTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADNForm|551c816f-0bb8-d8bb-4ff6-124a7aeaa408", "Loading Spec. Prov.", "Loading Special Provisions", "Code(s) of the special requirements applicable to loading of this substance.");
			this.ADN_LoadingSpecialProvTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ADN_LoadingSpecialProvTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 16, true);
			this.ADN_LoadingSpecialProvTextBox.Name = "ADN_LoadingSpecialProvTextBox";
			this.ADN_LoadingSpecialProvTextBox.ReadOnly = true;
			this.ADN_LoadingSpecialProvTextBox.Multiline = true;
			this.ADN_LoadingSpecialProvTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 35, true);
			this.ADN_LoadingSpecialProvTextBox.TabIndex = 13;
			// 
			// CarriagePermittedGroupBox
			// 
			this.CarriagePermittedGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADNForm|d2862d8f-e236-f8a3-4919-a65e003396bb", "Carriage Permitted");
			this.CarriagePermittedGroupBox.Controls.Add(this.ADN_CarriagePermittedDetailsTextBox);
			this.CarriagePermittedGroupBox.Controls.Add(this.ADN_CarriagePermittedTanksCheckBox);
			this.CarriagePermittedGroupBox.Controls.Add(this.ADN_CarriagePermittedBulkCheckBox);
			this.CarriagePermittedGroupBox.Controls.Add(this.ADN_CarriagePermittedPacksCheckBox);
			this.CarriagePermittedGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(500, 99, true);
			this.CarriagePermittedGroupBox.Name = "CarriagePermittedGroupBox";
			this.CarriagePermittedGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(355, 83, true);
			this.CarriagePermittedGroupBox.TabIndex = 30;
			this.CarriagePermittedGroupBox.TabStop = false;
			// 
			// ADN_CarriagePermittedDetailsTextBox
			// 
			this.ADN_CarriagePermittedDetailsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.ADN_CarriagePermittedDetailsTextBox, "ADN_CarriagePermittedDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ADN_CarriagePermittedDetails)));
			this.ADN_CarriagePermittedDetailsTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADNForm|2f6f5af3-c96a-41b5-4fc3-b8ca39c4f78a", "Details", "Code(s) concerning the permitted form of carriage of this substance in inland navigation vessels.");
			this.ADN_CarriagePermittedDetailsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ADN_CarriagePermittedDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 59, true);
			this.ADN_CarriagePermittedDetailsTextBox.Name = "ADN_CarriagePermittedDetailsTextBox";
			this.ADN_CarriagePermittedDetailsTextBox.ReadOnly = true;
			this.ADN_CarriagePermittedDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(193, 20, true);
			this.ADN_CarriagePermittedDetailsTextBox.TabIndex = 3;
			// 
			// ADN_CarriagePermittedTanksCheckBox
			// 
			this.ADN_CarriagePermittedTanksCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ADN_CarriagePermittedTanksCheckBox, "ADN_CarriagePermittedTanks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ADN_CarriagePermittedTanks)));
			this.ADN_CarriagePermittedTanksCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADNForm|a095d762-a29f-61b2-4ede-63aaf031bae8", "Tanks", "Code(s) concerning the permitted form of carriage of this substance in inland navigation vessels.");
			this.ADN_CarriagePermittedTanksCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ADN_CarriagePermittedTanksCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ADN_CarriagePermittedTanksCheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.ADN_CarriagePermittedTanksCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 39, true);
			this.ADN_CarriagePermittedTanksCheckBox.Name = "ADN_CarriagePermittedTanksCheckBox";
			this.ADN_CarriagePermittedTanksCheckBox.ReadOnly = true;
			this.ADN_CarriagePermittedTanksCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ADN_CarriagePermittedTanksCheckBox.TabIndex = 2;
			// 
			// ADN_CarriagePermittedBulkCheckBox
			// 
			this.ADN_CarriagePermittedBulkCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ADN_CarriagePermittedBulkCheckBox, "ADN_CarriagePermittedBulk");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ADN_CarriagePermittedBulk)));
			this.ADN_CarriagePermittedBulkCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADNForm|345ba99c-5ed5-77ae-44bd-b3c1bc8095a5", "Bulk", "Code(s) concerning the permitted form of carriage of this substance in inland navigation vessels.");
			this.ADN_CarriagePermittedBulkCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ADN_CarriagePermittedBulkCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ADN_CarriagePermittedBulkCheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.ADN_CarriagePermittedBulkCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(303, 19, true);
			this.ADN_CarriagePermittedBulkCheckBox.Name = "ADN_CarriagePermittedBulkCheckBox";
			this.ADN_CarriagePermittedBulkCheckBox.ReadOnly = true;
			this.ADN_CarriagePermittedBulkCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ADN_CarriagePermittedBulkCheckBox.TabIndex = 1;
			// 
			// ADN_CarriagePermittedPacksCheckBox
			// 
			this.ADN_CarriagePermittedPacksCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ADN_CarriagePermittedPacksCheckBox, "ADN_CarriagePermittedPacks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ADN_CarriagePermittedPacks)));
			this.ADN_CarriagePermittedPacksCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADNForm|6c991596-2750-bfa8-4734-531d7b204568", "Packs", "Code(s) concerning the permitted form of carriage of this substance in inland navigation vessels.");
			this.ADN_CarriagePermittedPacksCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ADN_CarriagePermittedPacksCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ADN_CarriagePermittedPacksCheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.ADN_CarriagePermittedPacksCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 19, true);
			this.ADN_CarriagePermittedPacksCheckBox.Name = "ADN_CarriagePermittedPacksCheckBox";
			this.ADN_CarriagePermittedPacksCheckBox.ReadOnly = true;
			this.ADN_CarriagePermittedPacksCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ADN_CarriagePermittedPacksCheckBox.TabIndex = 0;
			// 
			// HeaderLabel
			// 
			this.HeaderLabel.AutoSize = true;
			this.HeaderLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADNForm|97c510c0-b7d7-07bc-477f-193f2113eed7", "INLAND WATER (ADN)");
			this.HeaderLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.HeaderLabel.IsFontBold = true;
			this.HeaderLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 20, true);
			this.HeaderLabel.Name = "HeaderLabel";
			this.HeaderLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 13, true);
			this.HeaderLabel.TabIndex = 17;
			// 
			// IconPictureBox
			// 
			this.IconPictureBox.BackColor = System.Drawing.Color.Transparent;
			this.IconPictureBox.Image = global::Enterprise.MasterFiles.GUI.Properties.Resources.inlandwaterfreight;
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
			this.BindingSource.SetBindingMember(this.IsActiveCheckBox, "ADN_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ADN_IsActive)));
			this.IsActiveCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADNForm|860a7eac-b8a9-f5a6-46d6-c1e0c680c1ef", "Active");
			this.IsActiveCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsActiveCheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(862, 55, true);
			this.IsActiveCheckBox.Name = "IsActiveCheckBox";
			this.IsActiveCheckBox.ReadOnly = true;
			this.IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsActiveCheckBox.TabIndex = 9;
			// 
			// IsSystemDefinedCheckBox
			// 
			this.IsSystemDefinedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsSystemDefinedCheckBox, "ADN_IsSystemDefined");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ADN_IsSystemDefined)));
			this.IsSystemDefinedCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceRIDForm|8349aa0c-f556-378b-423a-06c31c270838", "System Defined");
			this.IsSystemDefinedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsSystemDefinedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsSystemDefinedCheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.IsSystemDefinedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(805, 55, true);
			this.IsSystemDefinedCheckBox.Name = "IsSystemDefinedCheckBox";
			this.IsSystemDefinedCheckBox.ReadOnly = true;
			this.IsSystemDefinedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsSystemDefinedCheckBox.TabIndex = 8;
			// 
			// ADN_UNNOTextBox
			// 
			this.BindingSource.SetBindingMember(this.ADN_UNNOTextBox, "ADN_UNNO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ADN_UNNO)));
			this.ADN_UNNOTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADNForm|cf9d01f0-0ed9-1cad-43a2-2141309d38a2", "UN No.", "UN No.", "The United Nations (UN) number for this substance.");
			this.ADN_UNNOTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 52, true);
			this.ADN_UNNOTextBox.Name = "ADN_UNNOTextBox";
			this.ADN_UNNOTextBox.ReadOnly = true;
			this.ADN_UNNOTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.ADN_UNNOTextBox.TabIndex = 0;
			// 
			// ADN_VariantTextBox
			// 
			this.BindingSource.SetBindingMember(this.ADN_VariantTextBox, "ADN_Variant");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ADN_Variant)));
			this.ADN_VariantTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADNForm|39b4f550-603f-f6bf-4318-bac850b9c316", "Variant", "The Variant assigned to this substance.");
			this.ADN_VariantTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 52, true);
			this.ADN_VariantTextBox.Name = "ADN_VariantTextBox";
			this.ADN_VariantTextBox.ReadOnly = true;
			this.ADN_VariantTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 20, true);
			this.ADN_VariantTextBox.TabIndex = 1;
			// 
			// ADN_ProperShippingNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ADN_ProperShippingNameTextBox, "ADN_PSN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ADN_PSN)));
			this.ADN_ProperShippingNameTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADNForm|3d1650ec-01f7-c9aa-4400-8b92c7acc5b1", "PSN", "Proper Shipping Name", "The Proper Shipping Name for this substance.");
			this.ADN_ProperShippingNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ADN_ProperShippingNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 78, true);
			this.ADN_ProperShippingNameTextBox.Name = "ADN_ProperShippingNameTextBox";
			this.ADN_ProperShippingNameTextBox.ReadOnly = true;
			this.ADN_ProperShippingNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 20, true);
			this.ADN_ProperShippingNameTextBox.TabIndex = 10;
			// 
			// ADN_ClassDropEdit
			// 
			this.ADN_ClassDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ADN_ClassDropEdit, "ADN_Class");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ADN_Class)));
			this.ADN_ClassDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADNForm|4a291e82-a1cd-1ea4-4e72-fece418db2ff", "Class", "The Class of this substance.");
			this.ADN_ClassDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(290, 52, true);
			this.ADN_ClassDropEdit.Name = "ADN_ClassDropEdit";
			this.ADN_ClassDropEdit.PreBoundMaxLength = 4;
			this.ADN_ClassDropEdit.ReadOnly = true;
			this.ADN_ClassDropEdit.ShouldResizeByMaxLength = true;
			this.ADN_ClassDropEdit.ShowDescriptionBox = false;
			this.ADN_ClassDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.ADN_ClassDropEdit.TabIndex = 2;
			// 
			// ClassificationCode1TextBox
			// 
			this.BindingSource.SetBindingMember(this.ClassificationCode1TextBox, "ClassificationCode1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ClassificationCode1)));
			this.ClassificationCode1TextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADNForm|476e5375-45da-988f-4789-2518e39f2a8d", "Class. Code", "The Classification Code of this substance.");
			this.ClassificationCode1TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ClassificationCode1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(425, 52, true);
			this.ClassificationCode1TextBox.Name = "ClassificationCode1TextBox";
			this.ClassificationCode1TextBox.ReadOnly = true;
			this.ClassificationCode1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.ClassificationCode1TextBox.TabIndex = 3;
			// 
			// ClassificationCode2TextBox
			// 
			this.BindingSource.SetBindingMember(this.ClassificationCode2TextBox, "ClassificationCode2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ClassificationCode2)));
			this.ClassificationCode2TextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADNForm|94dc216a-518d-e288-45ad-8b65742483ce", "Class. Code 2", "The Classification Code of this substance.");
			this.ClassificationCode2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ClassificationCode2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(457, 52, true);
			this.ClassificationCode2TextBox.Name = "ClassificationCode2TextBox";
			this.ClassificationCode2TextBox.ReadOnly = true;
			this.ClassificationCode2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.ClassificationCode2TextBox.TabIndex = 4;
			// 
			// ADN_PGDropEdit
			// 
			this.ADN_PGDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ADN_PGDropEdit, "ADN_PG");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ADN_PG)));
			this.ADN_PGDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADNForm|18f28f4b-aee1-1a81-4066-1976e9ba8bc9", "PG", "Packing Group", "The UN Packing Group (either I, II or III) of this substance.");
			this.ADN_PGDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(575, 52, true);
			this.ADN_PGDropEdit.Name = "ADN_PGDropEdit";
			this.ADN_PGDropEdit.PreBoundMaxLength = 4;
			this.ADN_PGDropEdit.ReadOnly = true;
			this.ADN_PGDropEdit.ShouldResizeByMaxLength = true;
			this.ADN_PGDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.ADN_PGDropEdit.TabIndex = 5;
			// 
			// ADN_Label1TextBox
			// 
			this.ADN_Label1TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.ADN_Label1TextBox, "ADN_Label1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ADN_Label1)));
			this.ADN_Label1TextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADNForm|aa56e659-99de-48ac-4fbb-c9024ce8d59d", "Label(s)", "The label(s) assigned to this substance to be affixed to packages, containers, tank-containers, portable tanks, MEGCs, vehicles and wagons.");
			this.ADN_Label1TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ADN_Label1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 130, true);
			this.ADN_Label1TextBox.Name = "ADN_Label1TextBox";
			this.ADN_Label1TextBox.ReadOnly = true;
			this.ADN_Label1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 20, true);
			this.ADN_Label1TextBox.TabIndex = 11;
			// 
			// ADN_Label2TextBox
			// 
			this.ADN_Label2TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.ADN_Label2TextBox, "ADN_Label2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ADN_Label2)));
			this.ADN_Label2TextBox.CaptionResourceString = null;
			this.ADN_Label2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ADN_Label2TextBox, false);
			this.ADN_Label2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 130, true);
			this.ADN_Label2TextBox.Name = "ADN_Label2TextBox";
			this.ADN_Label2TextBox.ReadOnly = true;
			this.ADN_Label2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 20, true);
			this.ADN_Label2TextBox.TabIndex = 12;
			// 
			// ADN_Label3TextBox
			// 
			this.ADN_Label3TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.ADN_Label3TextBox, "ADN_Label3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ADN_Label3)));
			this.ADN_Label3TextBox.CaptionResourceString = null;
			this.ADN_Label3TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ADN_Label3TextBox, false);
			this.ADN_Label3TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 130, true);
			this.ADN_Label3TextBox.Name = "ADN_Label3TextBox";
			this.ADN_Label3TextBox.ReadOnly = true;
			this.ADN_Label3TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 20, true);
			this.ADN_Label3TextBox.TabIndex = 13;
			// 
			// ADN_Label4TextBox
			// 
			this.ADN_Label4TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.ADN_Label4TextBox, "ADN_Label4");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ADN_Label4)));
			this.ADN_Label4TextBox.CaptionResourceString = null;
			this.ADN_Label4TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ADN_Label4TextBox, false);
			this.ADN_Label4TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 130, true);
			this.ADN_Label4TextBox.Name = "ADN_Label4TextBox";
			this.ADN_Label4TextBox.ReadOnly = true;
			this.ADN_Label4TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 20, true);
			this.ADN_Label4TextBox.TabIndex = 14;
			// 
			// ADN_LQMaxAmtCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ADN_LQMaxAmtCalcEdit, "ADN_LQMaxAmt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ADN_LQMaxAmt)));
			this.ADN_LQMaxAmtCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADNForm|408145a6-4e66-74b8-4857-1d566be6a2e1", "Limited Quantity", "The maximum quantity per inner packaging allowed to carry this substance in limited quantities.");
			this.ADN_LQMaxAmtCalcEdit.DecimalPlaces = 2;
			this.ADN_LQMaxAmtCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 156, true);
			this.ADN_LQMaxAmtCalcEdit.Name = "ADN_LQMaxAmtCalcEdit";
			this.ADN_LQMaxAmtCalcEdit.ReadOnly = true;
			this.ADN_LQMaxAmtCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.ADN_LQMaxAmtCalcEdit.TabIndex = 15;
			this.ADN_LQMaxAmtCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ADN_LQMaxAmtUQTextBox
			// 
			this.BindingSource.SetBindingMember(this.ADN_LQMaxAmtUQTextBox, "ADN_LQMaxAmtUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ADN_LQMaxAmtUQ)));
			this.ADN_LQMaxAmtUQTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADNForm|6805bd73-7a40-138d-4a3d-77a77a1ed21f", "Units", "The maximum quantity per inner packaging allowed to carry this substance in limited quantities.");
			this.ADN_LQMaxAmtUQTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ADN_LQMaxAmtUQTextBox, false);
			this.ADN_LQMaxAmtUQTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(251, 156, true);
			this.ADN_LQMaxAmtUQTextBox.Name = "ADN_LQMaxAmtUQTextBox";
			this.ADN_LQMaxAmtUQTextBox.ReadOnly = true;
			this.ADN_LQMaxAmtUQTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 20, true);
			this.ADN_LQMaxAmtUQTextBox.TabIndex = 16;
			// 
			// ADN_LQ2MaxAmtCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ADN_LQ2MaxAmtCalcEdit, "ADN_LQ2MaxAmt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ADN_LQ2MaxAmt)));
			this.ADN_LQ2MaxAmtCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADNForm|36995919-8b18-a3ba-4efd-aefc7f8646d3", "Limited Quantity 2", "The maximum quantity per inner packaging allowed to carry this substance in limited quantities.");
			this.ADN_LQ2MaxAmtCalcEdit.DecimalPlaces = 2;
			this.ADN_LQ2MaxAmtCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 182, true);
			this.ADN_LQ2MaxAmtCalcEdit.Name = "ADN_LQ2MaxAmtCalcEdit";
			this.ADN_LQ2MaxAmtCalcEdit.ReadOnly = true;
			this.ADN_LQ2MaxAmtCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.ADN_LQ2MaxAmtCalcEdit.TabIndex = 17;
			this.ADN_LQ2MaxAmtCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ADN_LQ2MaxAmtUQTextBox
			// 
			this.BindingSource.SetBindingMember(this.ADN_LQ2MaxAmtUQTextBox, "ADN_LQ2MaxAmtUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ADN_LQ2MaxAmtUQ)));
			this.ADN_LQ2MaxAmtUQTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADNForm|87c346a6-29f8-7a91-45ac-052552c82ce4", "Units", "The maximum quantity per inner packaging allowed to carry this substance in limited quantities.");
			this.ADN_LQ2MaxAmtUQTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ADN_LQ2MaxAmtUQTextBox, false);
			this.ADN_LQ2MaxAmtUQTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(251, 182, true);
			this.ADN_LQ2MaxAmtUQTextBox.Name = "ADN_LQ2MaxAmtUQTextBox";
			this.ADN_LQ2MaxAmtUQTextBox.ReadOnly = true;
			this.ADN_LQ2MaxAmtUQTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 20, true);
			this.ADN_LQ2MaxAmtUQTextBox.TabIndex = 18;
			// 
			// ADN_BlueConesTextBox
			// 
			this.BindingSource.SetBindingMember(this.ADN_BlueConesTextBox, "ADN_BlueCones");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZByte)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ADN_BlueCones)));
			this.ADN_BlueConesTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADNForm|acd24d03-cd59-5399-408b-5e78212ff8ea", "Blue Cones/Lights", "The number of cones/lights which constitute the marking of the vessel during carriage of this substance.");
			this.ADN_BlueConesTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ADN_BlueConesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 208, true);
			this.ADN_BlueConesTextBox.Name = "ADN_BlueConesTextBox";
			this.ADN_BlueConesTextBox.ReadOnly = true;
			this.ADN_BlueConesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.ADN_BlueConesTextBox.TabIndex = 19;
			// 
			// ADN_VentilationTextBox
			// 
			this.BindingSource.SetBindingMember(this.ADN_VentilationTextBox, "ADN_Ventilation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ADN_Ventilation)));
			this.ADN_VentilationTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADNForm|d4f70e9b-abda-1f94-4e95-e0ba1d7b5368", "Ventilation", "Code(s) for special requirements concerning ventilation of the carriage of this substance.");
			this.ADN_VentilationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ADN_VentilationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 234, true);
			this.ADN_VentilationTextBox.Name = "ADN_VentilationTextBox";
			this.ADN_VentilationTextBox.ReadOnly = true;
			this.ADN_VentilationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 20, true);
			this.ADN_VentilationTextBox.TabIndex = 20;
			// 
			// ADN_SpecialProvisionsTextBox
			// 
			this.ADN_SpecialProvisionsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.ADN_SpecialProvisionsTextBox, "ADN_SpecialProvisions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ADN_SpecialProvisions)));
			this.ADN_SpecialProvisionsTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADNForm|80058959-fd70-09ab-4852-3e4716b481e3", "Special Provisions", "The Special Provisions assigned to this substance.");
			this.ADN_SpecialProvisionsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ADN_SpecialProvisionsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 260, true);
			this.ADN_SpecialProvisionsTextBox.Name = "ADN_SpecialProvisionsTextBox";
			this.ADN_SpecialProvisionsTextBox.ReadOnly = true;
			this.ADN_SpecialProvisionsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.ADN_SpecialProvisionsTextBox.TabIndex = 21;
			// 
			// ADN_ExceptedQuantityCodeDropEdit
			// 
			this.ADN_ExceptedQuantityCodeDropEdit.AllowDrop = true;
			this.ADN_ExceptedQuantityCodeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.ADN_ExceptedQuantityCodeDropEdit, "ADN_ExceptedQuantityCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ADN_ExceptedQuantityCode)));
			this.ADN_ExceptedQuantityCodeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADNForm|02653dd0-534d-809e-4af8-eb53330645fb", "Excepted Quantity", "The Excepted Quantity code assigned to this substance.");
			this.ADN_ExceptedQuantityCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 286, true);
			this.ADN_ExceptedQuantityCodeDropEdit.Name = "ADN_ExceptedQuantityCodeDropEdit";
			this.ADN_ExceptedQuantityCodeDropEdit.PreBoundMaxLength = 4;
			this.ADN_ExceptedQuantityCodeDropEdit.ReadOnly = true;
			this.ADN_ExceptedQuantityCodeDropEdit.ShouldResizeByMaxLength = true;
			this.ADN_ExceptedQuantityCodeDropEdit.ShowDescriptionBox = true;
			this.ADN_ExceptedQuantityCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 20, true);
			this.ADN_ExceptedQuantityCodeDropEdit.TabIndex = 22;
			// 
			// DetailsLanguageDropEdit
			// 
			this.DetailsLanguageDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DetailsLanguageDropEdit, "DetailsLanguage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).DetailsLanguage)));
			this.DetailsLanguageDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstancesADNForm|b5bb3569-d855-97a1-4ad7-8ac4c8d0080c", "Language Filter", "Filters the Names, Qualifying Descriptive Texts, Special Provisions and Stowage/Segmentation requirements to only show those that are in the language you choose.");
			this.DetailsLanguageDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 312, true);
			this.DetailsLanguageDropEdit.Name = "DetailsLanguageDropEdit";
			this.DetailsLanguageDropEdit.ShouldResizeByMaxLength = true;
			this.DetailsLanguageDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.DetailsLanguageDropEdit.TabIndex = 39;
			// 
			// EquipmentGroupBox
			// 
			this.EquipmentGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADNForm|bc8ab818-9f5b-1d99-496b-c68803a3101b", "Equipment");
			this.EquipmentGroupBox.Controls.Add(this.ADN_EquipmentDetailsTextBox);
			this.EquipmentGroupBox.Controls.Add(this.ADN_EquipBreathingAppCheckBox);
			this.EquipmentGroupBox.Controls.Add(this.ADN_EquipToximeterCheckBox);
			this.EquipmentGroupBox.Controls.Add(this.ADN_EquipGasDetectorCheckBox);
			this.EquipmentGroupBox.Controls.Add(this.ADN_EquipEscapeDevCheckBox);
			this.EquipmentGroupBox.Controls.Add(this.ADN_EquipPPECheckBox);
			this.EquipmentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(500, 183, true);
			this.EquipmentGroupBox.Name = "EquipmentGroupBox";
			this.EquipmentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(355, 102, true);
			this.EquipmentGroupBox.TabIndex = 31;
			this.EquipmentGroupBox.TabStop = false;
			// 
			// ADN_EquipmentDetailsTextBox
			// 
			this.ADN_EquipmentDetailsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.ADN_EquipmentDetailsTextBox, "ADN_EquipmentDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ADN_EquipmentDetails)));
			this.ADN_EquipmentDetailsTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADNForm|c6b9b3a8-b4c6-1985-46b4-dbcb9763dfb9", "Details", "Equipment for personal and general protection to carry out general actions and hazard specific emergency actions to be carried on board the vessel, as required for the carriage of the substance.");
			this.ADN_EquipmentDetailsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ADN_EquipmentDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 79, true);
			this.ADN_EquipmentDetailsTextBox.Name = "ADN_EquipmentDetailsTextBox";
			this.ADN_EquipmentDetailsTextBox.ReadOnly = true;
			this.ADN_EquipmentDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(193, 20, true);
			this.ADN_EquipmentDetailsTextBox.TabIndex = 5;
			// 
			// ADN_EquipBreathingAppCheckBox
			// 
			this.ADN_EquipBreathingAppCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ADN_EquipBreathingAppCheckBox, "ADN_EquipBreathingApparatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ADN_EquipBreathingApparatus)));
			this.ADN_EquipBreathingAppCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADNForm|5ab625d0-d97b-1aa5-489c-be208f30a958", "Breathing Apparatus", "When ticked, this substance requires a breathing apparatus.");
			this.ADN_EquipBreathingAppCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ADN_EquipBreathingAppCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ADN_EquipBreathingAppCheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.ADN_EquipBreathingAppCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 59, true);
			this.ADN_EquipBreathingAppCheckBox.Name = "ADN_EquipBreathingAppCheckBox";
			this.ADN_EquipBreathingAppCheckBox.ReadOnly = true;
			this.ADN_EquipBreathingAppCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ADN_EquipBreathingAppCheckBox.TabIndex = 4;
			// 
			// ADN_EquipToximeterCheckBox
			// 
			this.ADN_EquipToximeterCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ADN_EquipToximeterCheckBox, "ADN_EquipToximeter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ADN_EquipToximeter)));
			this.ADN_EquipToximeterCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADNForm|89545eb1-5a5a-2db4-4a83-068acea97c57", "Toximeter", "When ticked, this substance requires a toximeter.");
			this.ADN_EquipToximeterCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ADN_EquipToximeterCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ADN_EquipToximeterCheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.ADN_EquipToximeterCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(303, 39, true);
			this.ADN_EquipToximeterCheckBox.Name = "ADN_EquipToximeterCheckBox";
			this.ADN_EquipToximeterCheckBox.ReadOnly = true;
			this.ADN_EquipToximeterCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ADN_EquipToximeterCheckBox.TabIndex = 3;
			// 
			// ADN_EquipGasDetectorCheckBox
			// 
			this.ADN_EquipGasDetectorCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ADN_EquipGasDetectorCheckBox, "ADN_EquipGasDetector");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ADN_EquipGasDetector)));
			this.ADN_EquipGasDetectorCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADNForm|a23d9a62-832c-8eba-425f-edef172c4b7e", "Gas Detector", "When ticked, this substance requires a gas detector.");
			this.ADN_EquipGasDetectorCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ADN_EquipGasDetectorCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ADN_EquipGasDetectorCheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.ADN_EquipGasDetectorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 39, true);
			this.ADN_EquipGasDetectorCheckBox.Name = "ADN_EquipGasDetectorCheckBox";
			this.ADN_EquipGasDetectorCheckBox.ReadOnly = true;
			this.ADN_EquipGasDetectorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ADN_EquipGasDetectorCheckBox.TabIndex = 2;
			// 
			// ADN_EquipEscapeDevCheckBox
			// 
			this.ADN_EquipEscapeDevCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ADN_EquipEscapeDevCheckBox, "ADN_EquipEscapeDevice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ADN_EquipEscapeDevice)));
			this.ADN_EquipEscapeDevCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADNForm|0a32dde0-3e2d-2c8f-4a9b-264a100fa6c6", "Escape Device", "When ticked, this substance requires an escape device.");
			this.ADN_EquipEscapeDevCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ADN_EquipEscapeDevCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ADN_EquipEscapeDevCheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.ADN_EquipEscapeDevCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(303, 19, true);
			this.ADN_EquipEscapeDevCheckBox.Name = "ADN_EquipEscapeDevCheckBox";
			this.ADN_EquipEscapeDevCheckBox.ReadOnly = true;
			this.ADN_EquipEscapeDevCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ADN_EquipEscapeDevCheckBox.TabIndex = 1;
			// 
			// ADN_EquipPPECheckBox
			// 
			this.ADN_EquipPPECheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ADN_EquipPPECheckBox, "ADN_EquipPPE");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.UNDGSubstanceADN)(null)).ADN_EquipPPE)));
			this.ADN_EquipPPECheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADNForm|d401ee58-ca84-c0a6-457a-cec3437f3251", "Personal Protective Equipment", "When ticked, this substance requires personal protection equipment.");
			this.ADN_EquipPPECheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ADN_EquipPPECheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ADN_EquipPPECheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.ADN_EquipPPECheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 19, true);
			this.ADN_EquipPPECheckBox.Name = "ADN_EquipPPECheckBox";
			this.ADN_EquipPPECheckBox.ReadOnly = true;
			this.ADN_EquipPPECheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ADN_EquipPPECheckBox.TabIndex = 0;
			//
			// CountryReferencesTabPage
			//
			this.CountryReferencesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("UNDGSubstanceADNForm|90266bf5-0e7a-90bd-476f-aa02ffa30fa3", "Country/Region References");
			this.CountryReferencesTabPage.Controls.Add(this.CountryReferencesModuleButtonGrid);
			this.CountryReferencesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CountryReferencesTabPage.Name = "CountryReferencesTabPage";
			this.CountryReferencesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(798, 192, true);
			this.CountryReferencesTabPage.TabIndex = 9;
			//
			// CountryReferencesModuleButtonGrid
			//
			this.BindingSource.SetBindingMember(this.CountryReferencesModuleButtonGrid, "UNDGCountryReferences");
			this.CountryReferencesModuleButtonGrid.Name = "CountryReferencesModuleButtonGrid";
			this.CountryReferencesModuleButtonGrid.BindToFindBoxList = "Lookups.UNDGCountryReferences";
			this.CountryReferencesModuleButtonGrid.GridId = "1f3c535e-f5fa-b490-489a-8689e4d341cd";
			this.CountryReferencesModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CountryReferencesModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(798, 192, true);
			// 
			// UNDGSubstanceADNControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EquipmentGroupBox);
			this.Controls.Add(this.ADN_ExceptedQuantityCodeDropEdit);
			this.Controls.Add(this.CarriagePermittedGroupBox);
			this.Controls.Add(this.UNDGSubstanceTabControl);
			this.Controls.Add(this.DetailsLanguageDropEdit);
			this.Controls.Add(this.ADN_PGDropEdit);
			this.Controls.Add(this.ADN_ClassDropEdit);
			this.Controls.Add(this.ADN_ProperShippingNameTextBox);
			this.Controls.Add(this.ADN_VariantTextBox);
			this.Controls.Add(this.ADN_UNNOTextBox);
			this.Controls.Add(this.HeaderLabel);
			this.Controls.Add(this.IconPictureBox);
			this.Controls.Add(this.ADN_LQMaxAmtCalcEdit);
			this.Controls.Add(this.ADN_LQMaxAmtUQTextBox);
			this.Controls.Add(this.ADN_LQ2MaxAmtCalcEdit);
			this.Controls.Add(this.ADN_LQ2MaxAmtUQTextBox);
			this.Controls.Add(this.IsActiveCheckBox);
			this.Controls.Add(this.ClassificationCode2TextBox);
			this.Controls.Add(this.ClassificationCode1TextBox);
			this.Controls.Add(this.ADN_Label1TextBox);
			this.Controls.Add(this.ADN_Label2TextBox);
			this.Controls.Add(this.ADN_Label3TextBox);
			this.Controls.Add(this.ADN_Label4TextBox);
			this.Controls.Add(this.ADN_BlueConesTextBox);
			this.Controls.Add(this.ADN_VentilationTextBox);
			this.Controls.Add(this.ADN_SpecialProvisionsTextBox);
			this.Controls.Add(this.IsSystemDefinedCheckBox);
			this.Controls.Add(this.DG_BorderPanel);
			this.DataSourceAssemblyName = "Enterprise.MasterFiles.Business";
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.UNDGSubstanceADN);
			this.DataSourceTypeName = "Enterprise.MasterFiles.Business.UNDGSubstanceADN";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(922, 583, true);
			this.Name = "UNDGSubstanceADNControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.IsSystemDefinedCheckBox, 0);
			this.Controls.SetChildIndex(this.ADN_Label4TextBox, 0);
			this.Controls.SetChildIndex(this.ADN_Label3TextBox, 0);
			this.Controls.SetChildIndex(this.ADN_Label2TextBox, 0);
			this.Controls.SetChildIndex(this.ADN_Label1TextBox, 0);
			this.Controls.SetChildIndex(this.ClassificationCode1TextBox, 0);
			this.Controls.SetChildIndex(this.ClassificationCode2TextBox, 0);
			this.Controls.SetChildIndex(this.IsActiveCheckBox, 0);
			this.Controls.SetChildIndex(this.ADN_LQ2MaxAmtUQTextBox, 0);
			this.Controls.SetChildIndex(this.ADN_LQ2MaxAmtCalcEdit, 0);
			this.Controls.SetChildIndex(this.ADN_LQMaxAmtUQTextBox, 0);
			this.Controls.SetChildIndex(this.ADN_LQMaxAmtCalcEdit, 0);
			this.Controls.SetChildIndex(this.IconPictureBox, 0);
			this.Controls.SetChildIndex(this.HeaderLabel, 0);
			this.Controls.SetChildIndex(this.ADN_UNNOTextBox, 0);
			this.Controls.SetChildIndex(this.ADN_VariantTextBox, 0);
			this.Controls.SetChildIndex(this.ADN_ProperShippingNameTextBox, 0);
			this.Controls.SetChildIndex(this.ADN_ClassDropEdit, 0);
			this.Controls.SetChildIndex(this.ADN_PGDropEdit, 0);
			this.Controls.SetChildIndex(this.DetailsLanguageDropEdit, 0);
			this.Controls.SetChildIndex(this.UNDGSubstanceTabControl, 0);
			this.Controls.SetChildIndex(this.CarriagePermittedGroupBox, 0);
			this.Controls.SetChildIndex(this.ADN_ExceptedQuantityCodeDropEdit, 0);
			this.Controls.SetChildIndex(this.EquipmentGroupBox, 0);
			this.Controls.SetChildIndex(this.DG_BorderPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.UNDGSubstanceTabControl.ResumeLayout(false);
			this.UNDGSubstanceTabControl.PerformLayout();
			this.NamesTabPage.ResumeLayout(false);
			this.NamesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.NamesGrid)).EndInit();
			this.NamesGrid.ResumeLayout(false);
			this.NamesGrid.PerformLayout();
			this.QualifyingDescriptiveTextTabPage.ResumeLayout(false);
			this.QualifyingDescriptiveTextTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.QualifyingDescriptiveTextGrid)).EndInit();
			this.QualifyingDescriptiveTextGrid.ResumeLayout(false);
			this.QualifyingDescriptiveTextGrid.PerformLayout();
			this.OtherProvisionsTabPage.ResumeLayout(false);
			this.OtherProvisionsTabPage.PerformLayout();
			this.CarriagePermittedGroupBox.ResumeLayout(false);
			this.CarriagePermittedGroupBox.PerformLayout();
			this.SpecialProvisionsGroupBox.ResumeLayout(false);
			this.SpecialProvisionsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.IconPictureBox)).EndInit();
			this.ADN_ClassDropEdit.ResumeLayout(true);
			this.ADN_ClassDropEdit.PerformLayout();
			this.ADN_PGDropEdit.ResumeLayout(true);
			this.ADN_PGDropEdit.PerformLayout();
			this.ADN_ExceptedQuantityCodeDropEdit.ResumeLayout(true);
			this.ADN_ExceptedQuantityCodeDropEdit.PerformLayout();
			this.DetailsLanguageDropEdit.ResumeLayout(true);
			this.DetailsLanguageDropEdit.PerformLayout();
			this.EquipmentGroupBox.ResumeLayout(false);
			this.EquipmentGroupBox.PerformLayout();
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
