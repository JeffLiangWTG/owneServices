namespace Enterprise.Customs.US.GUI
{
	partial class FWSEditForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.LicensesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LicensesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CommodityDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PGAValueTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.InvCurrValueCalcEdit1 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.US_NetCommodityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.US_CommoditySpecificNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.US_CommodityGeneralNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.US_CartonQtyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ScientificDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.US_ScientificSubSpeciesNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.US_ScientificGenusNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.US_ScientificSpeciesNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ProductDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IsVenomousCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.US_WildlifeSourceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.US_WildlifeDescriptionCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.US_WildlifeCategoryCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.US_HybridDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.US_ProductTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.US_ProductNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FWSGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.US_IntendedUseCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.US_FIRMSCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.US_ProcessingCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.US_IsDocSubmittedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.US_SpeciesOriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DeclarationCertificationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.US_OA_FWSExporterAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.US_OA_FWSImporterAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.HybridSecondScientificDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.US_Scientific2SubSpeciesNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.US_Scientific2GenusNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.US_Scientific2SpeciesNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PackageLabelGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.US_RemarksTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DocumentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ConfirmationNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ButtonPanel.SuspendLayout();
			this.LicensesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LicensesGrid)).BeginInit();
			this.LicensesGrid.SuspendLayout();
			this.CommodityDetailsGroupBox.SuspendLayout();
			this.US_NetCommodityCalcDropEdit.SuspendLayout();
			this.ScientificDetailsGroupBox.SuspendLayout();
			this.ProductDetailsGroupBox.SuspendLayout();
			this.US_WildlifeSourceDropEdit.SuspendLayout();
			this.US_WildlifeDescriptionCodeDropEdit.SuspendLayout();
			this.US_WildlifeCategoryCodeDropEdit.SuspendLayout();
			this.US_HybridDropEdit.SuspendLayout();
			this.US_ProductTypeDropEdit.SuspendLayout();
			this.FWSGroupBox.SuspendLayout();
			this.US_IntendedUseCodeDropEdit.SuspendLayout();
			this.US_FIRMSCodeFindBox.SuspendLayout();
			this.US_ProcessingCodeDropEdit.SuspendLayout();
			this.US_SpeciesOriginCodeFindBox.SuspendLayout();
			this.DeclarationCertificationGroupBox.SuspendLayout();
			this.US_OA_FWSExporterAddressControl.SuspendLayout();
			this.US_OA_FWSImporterAddressControl.SuspendLayout();
			this.HybridSecondScientificDetailsGroupBox.SuspendLayout();
			this.PackageLabelGroupBox.SuspendLayout();
			this.DocumentsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 426, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 24, true);
			this.MainStatusBar.TabIndex = 10;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.FWSHeader);
			// 
			// ButtonPanel
			// 
			this.ButtonPanel.Controls.Add(this.OKButton);
			this.ButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 450, true);
			this.ButtonPanel.Name = "ButtonPanel";
			this.ButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 30, true);
			this.ButtonPanel.TabIndex = 9;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("201592b4-0370-44d4-aa7e-c252b6f478d4", "&OK");
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.OKButton.IsCaptionOverridden = false;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(778, 4, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 0;
			this.OKButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// LicensesGroupBox
			// 
			this.LicensesGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("51464cd3-6ec6-4e98-b7c3-db35ccff6aaf", "Licenses Details");
			this.LicensesGroupBox.Controls.Add(this.LicensesGrid);
			this.LicensesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 330, true);
			this.LicensesGroupBox.Name = "LicensesGroupBox";
			this.LicensesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 88, true);
			this.LicensesGroupBox.TabIndex = 8;
			this.LicensesGroupBox.TabStop = false;
			// 
			// LicensesGrid
			// 
			this.LicensesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.LicensesGrid, "Licenses");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.FWSHeader)(null)).Licenses)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FWSLicense)(((System.Collections.IList)(((Enterprise.Customs.US.Business.FWSHeader)(null)).Licenses)).SyncRoot)).US_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FWSLicense)(((System.Collections.IList)(((Enterprise.Customs.US.Business.FWSHeader)(null)).Licenses)).SyncRoot)).US_Number)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FWSLicense)(((System.Collections.IList)(((Enterprise.Customs.US.Business.FWSHeader)(null)).Licenses)).SyncRoot)).US_TypeDesc)));
			this.LicensesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "US_Type";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(47);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "US_Number";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(84);
			zTextBoxColumnStyleInfo2.ColumnName = "US_TypeDesc";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.LicensesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.LicensesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LicensesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.LicensesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LicensesGrid.GridId = "1592ddd7-a636-46f2-b111-4a445b83ce83";
			this.LicensesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LicensesGrid.LayoutKey = "CigarsGrid";
			this.LicensesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.LicensesGrid.Name = "LicensesGrid";
			this.LicensesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 69, true);
			this.LicensesGrid.TabIndex = 0;
			// 
			// CommodityDetailsGroupBox
			// 
			this.CommodityDetailsGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d467f708-7434-4bec-aa2e-49cee0334f53", "Commodity Details");
			this.CommodityDetailsGroupBox.Controls.Add(this.PGAValueTextBox);
			this.CommodityDetailsGroupBox.Controls.Add(this.InvCurrValueCalcEdit1);
			this.CommodityDetailsGroupBox.Controls.Add(this.US_NetCommodityCalcDropEdit);
			this.CommodityDetailsGroupBox.Controls.Add(this.US_CommoditySpecificNameTextBox);
			this.CommodityDetailsGroupBox.Controls.Add(this.US_CommodityGeneralNameTextBox);
			this.CommodityDetailsGroupBox.Controls.Add(this.US_CartonQtyCalcEdit);
			this.CommodityDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(390, 138, true);
			this.CommodityDetailsGroupBox.Name = "CommodityDetailsGroupBox";
			this.CommodityDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 102, true);
			this.CommodityDetailsGroupBox.TabIndex = 5;
			this.CommodityDetailsGroupBox.TabStop = false;
			// 
			// PGAValueTextBox
			// 
			this.BindingSource.SetBindingMember(this.PGAValueTextBox, "PGAValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FWSHeader)(null)).PGAValue)));
			this.PGAValueTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("30e6b4b7-3465-4425-b73e-09215c9f304b", "Value USD");
			this.PGAValueTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(307, 67, true);
			this.PGAValueTextBox.Name = "PGAValueTextBox";
			this.PGAValueTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.PGAValueTextBox.TabIndex = 5;
			// 
			// InvCurrValueCalcEdit1
			// 
			this.BindingSource.SetBindingMember(this.InvCurrValueCalcEdit1, "US_InvCurrPGAValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_InvCurrPGAValue)));
			this.InvCurrValueCalcEdit1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("679de0e6-c9ed-49fb-9bd0-90137c289f39", "Inv. Curr. Value");
			this.InvCurrValueCalcEdit1.DecimalPlaces = 2;
			this.InvCurrValueCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 67, true);
			this.InvCurrValueCalcEdit1.Name = "InvCurrValueCalcEdit1";
			this.InvCurrValueCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.InvCurrValueCalcEdit1.TabIndex = 4;
			this.InvCurrValueCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// US_NetCommodityCalcDropEdit
			// 
			this.US_NetCommodityCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_NetCommodityCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_NetCommodity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_NetCommodityUQ)));
			this.US_NetCommodityCalcDropEdit.BindToAmount = "US_NetCommodity";
			this.US_NetCommodityCalcDropEdit.BindToUnit = "US_NetCommodityUQ";
			this.US_NetCommodityCalcDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("758dc77c-de8c-4aba-a24c-079ae44684ba", "Net Qty");
			this.US_NetCommodityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(307, 41, true);
			this.US_NetCommodityCalcDropEdit.Name = "US_NetCommodityCalcDropEdit";
			this.US_NetCommodityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.US_NetCommodityCalcDropEdit.TabIndex = 3;
			this.US_NetCommodityCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// US_CommoditySpecificNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.US_CommoditySpecificNameTextBox, "US_CommoditySpecificName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_CommoditySpecificName)));
			this.US_CommoditySpecificNameTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("205f4630-4948-4c4c-a2f6-380fb24e5c52", "Specific Name");
			this.US_CommoditySpecificNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 15, true);
			this.US_CommoditySpecificNameTextBox.Name = "US_CommoditySpecificNameTextBox";
			this.US_CommoditySpecificNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.US_CommoditySpecificNameTextBox.TabIndex = 0;
			// 
			// US_CommodityGeneralNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.US_CommodityGeneralNameTextBox, "US_CommodityGeneralName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_CommodityGeneralName)));
			this.US_CommodityGeneralNameTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("0f8c3d36-93ac-46eb-af83-f8e1edf7194a", "General Name");
			this.US_CommodityGeneralNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 41, true);
			this.US_CommodityGeneralNameTextBox.Name = "US_CommodityGeneralNameTextBox";
			this.US_CommodityGeneralNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.US_CommodityGeneralNameTextBox.TabIndex = 2;
			// 
			// US_CartonQtyCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.US_CartonQtyCalcEdit, "US_CartonQty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_CartonQty)));
			this.US_CartonQtyCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("47edb706-7361-4e1f-8384-5adf79796eed", "Carton Qty");
			this.US_CartonQtyCalcEdit.DecimalPlaces = 2;
			this.US_CartonQtyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(307, 15, true);
			this.US_CartonQtyCalcEdit.Name = "US_CartonQtyCalcEdit";
			this.US_CartonQtyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.US_CartonQtyCalcEdit.TabIndex = 1;
			this.US_CartonQtyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ScientificDetailsGroupBox
			// 
			this.ScientificDetailsGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("aa3676e7-2235-465d-8d67-6f0288888c37", "Scientific Names");
			this.ScientificDetailsGroupBox.Controls.Add(this.US_ScientificSubSpeciesNameTextBox);
			this.ScientificDetailsGroupBox.Controls.Add(this.US_ScientificGenusNameTextBox);
			this.ScientificDetailsGroupBox.Controls.Add(this.US_ScientificSpeciesNameTextBox);
			this.ScientificDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(390, 255, true);
			this.ScientificDetailsGroupBox.Name = "ScientificDetailsGroupBox";
			this.ScientificDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 66, true);
			this.ScientificDetailsGroupBox.TabIndex = 6;
			this.ScientificDetailsGroupBox.TabStop = false;
			// 
			// US_ScientificSubSpeciesNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.US_ScientificSubSpeciesNameTextBox, "US_ScientificSubSpeciesName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_ScientificSubSpeciesName)));
			this.US_ScientificSubSpeciesNameTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("27c06311-a839-4e95-8ba4-4a596035ccac", "Sub Species");
			this.US_ScientificSubSpeciesNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(316, 31, true);
			this.US_ScientificSubSpeciesNameTextBox.Name = "US_ScientificSubSpeciesNameTextBox";
			this.US_ScientificSubSpeciesNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.US_ScientificSubSpeciesNameTextBox.TabIndex = 2;
			// 
			// US_ScientificGenusNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.US_ScientificGenusNameTextBox, "US_ScientificGenusName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_ScientificGenusName)));
			this.US_ScientificGenusNameTextBox.CaptionResourceString = null;
			this.US_ScientificGenusNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 15, true);
			this.US_ScientificGenusNameTextBox.Name = "US_ScientificGenusNameTextBox";
			this.US_ScientificGenusNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.US_ScientificGenusNameTextBox.TabIndex = 0;
			// 
			// US_ScientificSpeciesNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.US_ScientificSpeciesNameTextBox, "US_ScientificSpeciesName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_ScientificSpeciesName)));
			this.US_ScientificSpeciesNameTextBox.CaptionResourceString = null;
			this.US_ScientificSpeciesNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 41, true);
			this.US_ScientificSpeciesNameTextBox.Name = "US_ScientificSpeciesNameTextBox";
			this.US_ScientificSpeciesNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.US_ScientificSpeciesNameTextBox.TabIndex = 1;
			// 
			// ProductDetailsGroupBox
			// 
			this.ProductDetailsGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("430a2940-957d-4004-85ff-2d474b14eb1d", "Product Details");
			this.ProductDetailsGroupBox.Controls.Add(this.IsVenomousCheckBox);
			this.ProductDetailsGroupBox.Controls.Add(this.US_WildlifeSourceDropEdit);
			this.ProductDetailsGroupBox.Controls.Add(this.US_WildlifeDescriptionCodeDropEdit);
			this.ProductDetailsGroupBox.Controls.Add(this.US_WildlifeCategoryCodeDropEdit);
			this.ProductDetailsGroupBox.Controls.Add(this.US_HybridDropEdit);
			this.ProductDetailsGroupBox.Controls.Add(this.US_ProductTypeDropEdit);
			this.ProductDetailsGroupBox.Controls.Add(this.US_ProductNumberTextBox);
			this.ProductDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(390, 6, true);
			this.ProductDetailsGroupBox.Name = "ProductDetailsGroupBox";
			this.ProductDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 120, true);
			this.ProductDetailsGroupBox.TabIndex = 4;
			this.ProductDetailsGroupBox.TabStop = false;
			// 
			// IsVenomousCheckBox
			// 
			this.IsVenomousCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsVenomousCheckBox, "US_IsLiveVenomous");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_IsLiveVenomous)));
			this.IsVenomousCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsVenomousCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsVenomousCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(244, 96, true);
			this.IsVenomousCheckBox.Name = "IsVenomousCheckBox";
			this.IsVenomousCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 17, true);
			this.IsVenomousCheckBox.TabIndex = 6;
			this.IsVenomousCheckBox.Text = "Is Venomous";
			// 
			// US_WildlifeSourceDropEdit
			// 
			this.US_WildlifeSourceDropEdit.AllowDrop = true;
			this.US_WildlifeSourceDropEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.US_WildlifeSourceDropEdit, "US_WildlifeSource");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_WildlifeSource)));
			this.US_WildlifeSourceDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("187714ea-a806-4644-ae2f-561f36d84a90", "Wildlife Source");
			this.US_WildlifeSourceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 67, true);
			this.US_WildlifeSourceDropEdit.Name = "US_WildlifeSourceDropEdit";
			this.US_WildlifeSourceDropEdit.PreBoundMaxLength = 3;
			this.US_WildlifeSourceDropEdit.ShouldResizeByMaxLength = true;
			this.US_WildlifeSourceDropEdit.ShowDescriptionBox = false;
			this.US_WildlifeSourceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.US_WildlifeSourceDropEdit.TabIndex = 4;
			// 
			// US_WildlifeDescriptionCodeDropEdit
			// 
			this.US_WildlifeDescriptionCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_WildlifeDescriptionCodeDropEdit, "US_WildlifeDescriptionCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_WildlifeDescriptionCode)));
			this.US_WildlifeDescriptionCodeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("c16bdfc5-a412-4def-8173-6eac3479a01f", "FWS Description");
			this.US_WildlifeDescriptionCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(316, 67, true);
			this.US_WildlifeDescriptionCodeDropEdit.Name = "US_WildlifeDescriptionCodeDropEdit";
			this.US_WildlifeDescriptionCodeDropEdit.PreBoundMaxLength = 3;
			this.US_WildlifeDescriptionCodeDropEdit.ShouldResizeByMaxLength = true;
			this.US_WildlifeDescriptionCodeDropEdit.ShowDescriptionBox = false;
			this.US_WildlifeDescriptionCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.US_WildlifeDescriptionCodeDropEdit.TabIndex = 5;
			// 
			// US_WildlifeCategoryCodeDropEdit
			// 
			this.US_WildlifeCategoryCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_WildlifeCategoryCodeDropEdit, "US_WildlifeCategoryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_WildlifeCategoryCode)));
			this.US_WildlifeCategoryCodeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("19dede72-8564-4714-8551-32d4068223dc", "Wildlife Category");
			this.US_WildlifeCategoryCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 41, true);
			this.US_WildlifeCategoryCodeDropEdit.Name = "US_WildlifeCategoryCodeDropEdit";
			this.US_WildlifeCategoryCodeDropEdit.PreBoundMaxLength = 3;
			this.US_WildlifeCategoryCodeDropEdit.ShouldResizeByMaxLength = true;
			this.US_WildlifeCategoryCodeDropEdit.ShowDescriptionBox = false;
			this.US_WildlifeCategoryCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.US_WildlifeCategoryCodeDropEdit.TabIndex = 2;
			// 
			// US_HybridDropEdit
			// 
			this.US_HybridDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_HybridDropEdit, "US_Hybrid");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_Hybrid)));
			this.US_HybridDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f1280159-ce2a-4417-9689-34fc03018167", "Hybrid");
			this.US_HybridDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(316, 41, true);
			this.US_HybridDropEdit.Name = "US_HybridDropEdit";
			this.US_HybridDropEdit.PreBoundMaxLength = 4;
			this.US_HybridDropEdit.ShouldResizeByMaxLength = true;
			this.US_HybridDropEdit.ShowDescriptionBox = false;
			this.US_HybridDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.US_HybridDropEdit.TabIndex = 3;
			// 
			// US_ProductTypeDropEdit
			// 
			this.US_ProductTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_ProductTypeDropEdit, "US_ProductType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_ProductType)));
			this.US_ProductTypeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("a94f4399-3c2c-4e9d-aee6-043f6a4fa8a5", "Type");
			this.US_ProductTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 15, true);
			this.US_ProductTypeDropEdit.Name = "US_ProductTypeDropEdit";
			this.US_ProductTypeDropEdit.PreBoundMaxLength = 3;
			this.US_ProductTypeDropEdit.ShouldResizeByMaxLength = true;
			this.US_ProductTypeDropEdit.ShowDescriptionBox = false;
			this.US_ProductTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.US_ProductTypeDropEdit.TabIndex = 0;
			// 
			// US_ProductNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.US_ProductNumberTextBox, "US_ProductNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_ProductNumber)));
			this.US_ProductNumberTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("a9297d9e-a395-4ec2-b3e4-243b48173813", "Number");
			this.US_ProductNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(316, 15, true);
			this.US_ProductNumberTextBox.Name = "US_ProductNumberTextBox";
			this.US_ProductNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.US_ProductNumberTextBox.TabIndex = 1;
			// 
			// FWSGroupBox
			// 
			this.FWSGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("54af7ed3-8212-476e-95a6-26b003d4bb63", "General Details");
			this.FWSGroupBox.Controls.Add(this.US_IntendedUseCodeDropEdit);
			this.FWSGroupBox.Controls.Add(this.US_FIRMSCodeFindBox);
			this.FWSGroupBox.Controls.Add(this.US_ProcessingCodeDropEdit);
			this.FWSGroupBox.Controls.Add(this.US_IsDocSubmittedCheckBox);
			this.FWSGroupBox.Controls.Add(this.US_SpeciesOriginCodeFindBox);
			this.FWSGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.FWSGroupBox.Name = "FWSGroupBox";
			this.FWSGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(378, 120, true);
			this.FWSGroupBox.TabIndex = 0;
			this.FWSGroupBox.TabStop = false;
			// 
			// US_IntendedUseCodeDropEdit
			// 
			this.US_IntendedUseCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_IntendedUseCodeDropEdit, "US_IntendedUseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_IntendedUseCode)));
			this.US_IntendedUseCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 38, true);
			this.US_IntendedUseCodeDropEdit.Name = "US_IntendedUseCodeDropEdit";
			this.US_IntendedUseCodeDropEdit.PreBoundMaxLength = 7;
			this.US_IntendedUseCodeDropEdit.ShouldResizeByMaxLength = true;
			this.US_IntendedUseCodeDropEdit.ShowDescriptionBox = false;
			this.US_IntendedUseCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.US_IntendedUseCodeDropEdit.TabIndex = 2;
			// 
			// US_FIRMSCodeFindBox
			// 
			this.US_FIRMSCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_FIRMSCodeFindBox, "US_FIRMS");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_FIRMS)));
			this.US_FIRMSCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("a67fd651-4587-4ed5-baec-69905e8dc543", "Inspection Location");
			this.US_FIRMSCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 90, true);
			this.US_FIRMSCodeFindBox.Name = "US_FIRMSCodeFindBox";
			this.US_FIRMSCodeFindBox.PreBoundMaxLength = 4;
			this.US_FIRMSCodeFindBox.ShowDescriptionBox = false;
			this.US_FIRMSCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.US_FIRMSCodeFindBox.TabIndex = 6;
			// 
			// US_ProcessingCodeDropEdit
			// 
			this.US_ProcessingCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_ProcessingCodeDropEdit, "US_ProcessingCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_ProcessingCode)));
			this.US_ProcessingCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 15, true);
			this.US_ProcessingCodeDropEdit.Name = "US_ProcessingCodeDropEdit";
			this.US_ProcessingCodeDropEdit.PreBoundMaxLength = 1;
			this.US_ProcessingCodeDropEdit.ShouldResizeByMaxLength = true;
			this.US_ProcessingCodeDropEdit.ShowDescriptionBox = false;
			this.US_ProcessingCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.US_ProcessingCodeDropEdit.TabIndex = 0;
			// 
			// US_IsDocSubmittedCheckBox
			// 
			this.US_IsDocSubmittedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.US_IsDocSubmittedCheckBox, "US_IsDocSubmitted");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_IsDocSubmitted)));
			this.US_IsDocSubmittedCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("45ce4455-9d42-493f-b706-be6c6e608f0f", "Is Doc Submitted?");
			this.US_IsDocSubmittedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.US_IsDocSubmittedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.US_IsDocSubmittedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(254, 18, true);
			this.US_IsDocSubmittedCheckBox.Name = "US_IsDocSubmittedCheckBox";
			this.US_IsDocSubmittedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 17, true);
			this.US_IsDocSubmittedCheckBox.TabIndex = 1;
			this.US_IsDocSubmittedCheckBox.Text = "Is Doc Submitted?";
			// 
			// US_SpeciesOriginCodeFindBox
			// 
			this.US_SpeciesOriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_SpeciesOriginCodeFindBox, "US_SpeciesOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_SpeciesOrigin)));
			this.US_SpeciesOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 64, true);
			this.US_SpeciesOriginCodeFindBox.Name = "US_SpeciesOriginCodeFindBox";
			this.US_SpeciesOriginCodeFindBox.PreBoundMaxLength = 2;
			this.US_SpeciesOriginCodeFindBox.ShowDescriptionBox = false;
			this.US_SpeciesOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.US_SpeciesOriginCodeFindBox.TabIndex = 4;
			// 
			// DeclarationCertificationGroupBox
			// 
			this.DeclarationCertificationGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ac3a1db2-c3ef-4dea-8696-4f6aa4fe049e", "Certification Detail");
			this.DeclarationCertificationGroupBox.Controls.Add(this.US_OA_FWSExporterAddressControl);
			this.DeclarationCertificationGroupBox.Controls.Add(this.US_OA_FWSImporterAddressControl);
			this.DeclarationCertificationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 132, true);
			this.DeclarationCertificationGroupBox.Name = "DeclarationCertificationGroupBox";
			this.DeclarationCertificationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(378, 66, true);
			this.DeclarationCertificationGroupBox.TabIndex = 1;
			this.DeclarationCertificationGroupBox.TabStop = false;
			// 
			// US_OA_FWSExporterAddressControl
			// 
			this.US_OA_FWSExporterAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_OA_FWSExporterAddressControl, "US_OA_FWSExporterAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_OA_FWSExporterAddress)));
			this.US_OA_FWSExporterAddressControl.BindToOrgList = "AddInfoLookups+Organisations";
			this.US_OA_FWSExporterAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("8f0df9a3-7b13-49cc-9fce-cec4b525d36f", "Exporter");
			this.US_OA_FWSExporterAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 14, true);
			this.US_OA_FWSExporterAddressControl.Name = "US_OA_FWSExporterAddressControl";
			this.US_OA_FWSExporterAddressControl.PopupCaption = "FDA Importer";
			this.US_OA_FWSExporterAddressControl.ReadOnly = false;
			this.US_OA_FWSExporterAddressControl.ShowAddress = false;
			this.US_OA_FWSExporterAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.US_OA_FWSExporterAddressControl.TabIndex = 0;
			// 
			// US_OA_FWSImporterAddressControl
			// 
			this.US_OA_FWSImporterAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_OA_FWSImporterAddressControl, "US_OA_FWSImporterAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_OA_FWSImporterAddress)));
			this.US_OA_FWSImporterAddressControl.BindToOrgList = "AddInfoLookups+Organisations";
			this.US_OA_FWSImporterAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("aefe1786-f3d5-4f01-af6c-3b35cd469db8", "Importer");
			this.US_OA_FWSImporterAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 40, true);
			this.US_OA_FWSImporterAddressControl.Name = "US_OA_FWSImporterAddressControl";
			this.US_OA_FWSImporterAddressControl.PopupCaption = "FDA Importer";
			this.US_OA_FWSImporterAddressControl.ReadOnly = false;
			this.US_OA_FWSImporterAddressControl.ShowAddress = false;
			this.US_OA_FWSImporterAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.US_OA_FWSImporterAddressControl.TabIndex = 1;
			// 
			// HybridSecondScientificDetailsGroupBox
			// 
			this.HybridSecondScientificDetailsGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("b6cf3745-3d57-4b15-ae95-f6f53f17949c", "Hybrid Second Scientific Names");
			this.HybridSecondScientificDetailsGroupBox.Controls.Add(this.US_Scientific2SubSpeciesNameTextBox);
			this.HybridSecondScientificDetailsGroupBox.Controls.Add(this.US_Scientific2GenusNameTextBox);
			this.HybridSecondScientificDetailsGroupBox.Controls.Add(this.US_Scientific2SpeciesNameTextBox);
			this.HybridSecondScientificDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(390, 330, true);
			this.HybridSecondScientificDetailsGroupBox.Name = "HybridSecondScientificDetailsGroupBox";
			this.HybridSecondScientificDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 66, true);
			this.HybridSecondScientificDetailsGroupBox.TabIndex = 7;
			this.HybridSecondScientificDetailsGroupBox.TabStop = false;
			// 
			// US_Scientific2SubSpeciesNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.US_Scientific2SubSpeciesNameTextBox, "US_Scientific2SubSpeciesName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_Scientific2SubSpeciesName)));
			this.US_Scientific2SubSpeciesNameTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("5d90ece0-6652-4768-9fc0-48a5dc23df4d", "Sub Species");
			this.US_Scientific2SubSpeciesNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(316, 40, true);
			this.US_Scientific2SubSpeciesNameTextBox.Name = "US_Scientific2SubSpeciesNameTextBox";
			this.US_Scientific2SubSpeciesNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.US_Scientific2SubSpeciesNameTextBox.TabIndex = 3;
			// 
			// US_Scientific2GenusNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.US_Scientific2GenusNameTextBox, "US_Scientific2GenusName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_Scientific2GenusName)));
			this.US_Scientific2GenusNameTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("1b9621b0-baa4-4537-b21d-e14ff8ca7bc5", "Genus Name");
			this.US_Scientific2GenusNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 16, true);
			this.US_Scientific2GenusNameTextBox.Name = "US_Scientific2GenusNameTextBox";
			this.US_Scientific2GenusNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.US_Scientific2GenusNameTextBox.TabIndex = 0;
			// 
			// US_Scientific2SpeciesNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.US_Scientific2SpeciesNameTextBox, "US_Scientific2SpeciesName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_Scientific2SpeciesName)));
			this.US_Scientific2SpeciesNameTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("9238f1da-3a5a-4eaa-b42d-29f6ea72a455", "Species Name");
			this.US_Scientific2SpeciesNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 42, true);
			this.US_Scientific2SpeciesNameTextBox.Name = "US_Scientific2SpeciesNameTextBox";
			this.US_Scientific2SpeciesNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.US_Scientific2SpeciesNameTextBox.TabIndex = 1;
			// 
			// PackageLabelGroupBox
			// 
			this.PackageLabelGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("9e56dd0f-9551-4b6b-887e-82a88fea6889", "Package Marking/Labeling");
			this.PackageLabelGroupBox.Controls.Add(this.US_RemarksTextTextBox);
			this.PackageLabelGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 256, true);
			this.PackageLabelGroupBox.Name = "PackageLabelGroupBox";
			this.PackageLabelGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(378, 68, true);
			this.PackageLabelGroupBox.TabIndex = 3;
			this.PackageLabelGroupBox.TabStop = false;
			// 
			// US_RemarksTextTextBox
			// 
			this.BindingSource.SetBindingMember(this.US_RemarksTextTextBox, "US_RemarksText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_RemarksText)));
			this.US_RemarksTextTextBox.CaptionResourceString = null;
			this.US_RemarksTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.US_RemarksTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.US_RemarksTextTextBox.Multiline = true;
			this.US_RemarksTextTextBox.Name = "US_RemarksTextTextBox";
			this.US_RemarksTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.US_RemarksTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 49, true);
			this.US_RemarksTextTextBox.TabIndex = 0;
			// 
			// DocumentsGroupBox
			// 
			this.DocumentsGroupBox.Controls.Add(this.ConfirmationNumberTextBox);
			this.DocumentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 204, true);
			this.DocumentsGroupBox.Name = "DocumentsGroupBox";
			this.DocumentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(378, 46, true);
			this.DocumentsGroupBox.TabIndex = 3;
			this.DocumentsGroupBox.TabStop = false;
			this.DocumentsGroupBox.Text = "Documents";
			// 
			// ConfirmationNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConfirmationNumberTextBox, "US_ConfirmationNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.FWSHeader)(null)).US_ConfirmationNum)));
			this.ConfirmationNumberTextBox.CaptionResourceString = null;
			this.ConfirmationNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 16, true);
			this.ConfirmationNumberTextBox.Name = "ConfirmationNumberTextBox";
			this.ConfirmationNumberTextBox.ReadOnly = true;
			this.ConfirmationNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 20, true);
			this.ConfirmationNumberTextBox.TabIndex = 3;
			// 
			// FWSEditForm
			// 
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CancelButton = this.OKButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 480, true);
			this.Controls.Add(this.DocumentsGroupBox);
			this.Controls.Add(this.PackageLabelGroupBox);
			this.Controls.Add(this.HybridSecondScientificDetailsGroupBox);
			this.Controls.Add(this.DeclarationCertificationGroupBox);
			this.Controls.Add(this.CommodityDetailsGroupBox);
			this.Controls.Add(this.ScientificDetailsGroupBox);
			this.Controls.Add(this.ProductDetailsGroupBox);
			this.Controls.Add(this.FWSGroupBox);
			this.Controls.Add(this.LicensesGroupBox);
			this.Controls.Add(this.ButtonPanel);
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.FWSHeader);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "FWSEditForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.ButtonPanel, 0);
			this.Controls.SetChildIndex(this.LicensesGroupBox, 0);
			this.Controls.SetChildIndex(this.FWSGroupBox, 0);
			this.Controls.SetChildIndex(this.ProductDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.ScientificDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.CommodityDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.DeclarationCertificationGroupBox, 0);
			this.Controls.SetChildIndex(this.HybridSecondScientificDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.PackageLabelGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.DocumentsGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ButtonPanel.ResumeLayout(false);
			this.ButtonPanel.PerformLayout();
			this.LicensesGroupBox.ResumeLayout(false);
			this.LicensesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LicensesGrid)).EndInit();
			this.LicensesGrid.ResumeLayout(false);
			this.LicensesGrid.PerformLayout();
			this.CommodityDetailsGroupBox.ResumeLayout(false);
			this.CommodityDetailsGroupBox.PerformLayout();
			this.US_NetCommodityCalcDropEdit.ResumeLayout(true);
			this.US_NetCommodityCalcDropEdit.PerformLayout();
			this.ScientificDetailsGroupBox.ResumeLayout(false);
			this.ScientificDetailsGroupBox.PerformLayout();
			this.ProductDetailsGroupBox.ResumeLayout(false);
			this.ProductDetailsGroupBox.PerformLayout();
			this.US_WildlifeSourceDropEdit.ResumeLayout(true);
			this.US_WildlifeSourceDropEdit.PerformLayout();
			this.US_WildlifeDescriptionCodeDropEdit.ResumeLayout(true);
			this.US_WildlifeDescriptionCodeDropEdit.PerformLayout();
			this.US_WildlifeCategoryCodeDropEdit.ResumeLayout(true);
			this.US_WildlifeCategoryCodeDropEdit.PerformLayout();
			this.US_HybridDropEdit.ResumeLayout(true);
			this.US_HybridDropEdit.PerformLayout();
			this.US_ProductTypeDropEdit.ResumeLayout(true);
			this.US_ProductTypeDropEdit.PerformLayout();
			this.FWSGroupBox.ResumeLayout(false);
			this.FWSGroupBox.PerformLayout();
			this.US_IntendedUseCodeDropEdit.ResumeLayout(true);
			this.US_IntendedUseCodeDropEdit.PerformLayout();
			this.US_FIRMSCodeFindBox.ResumeLayout(true);
			this.US_FIRMSCodeFindBox.PerformLayout();
			this.US_ProcessingCodeDropEdit.ResumeLayout(true);
			this.US_ProcessingCodeDropEdit.PerformLayout();
			this.US_SpeciesOriginCodeFindBox.ResumeLayout(true);
			this.US_SpeciesOriginCodeFindBox.PerformLayout();
			this.DeclarationCertificationGroupBox.ResumeLayout(false);
			this.DeclarationCertificationGroupBox.PerformLayout();
			this.US_OA_FWSExporterAddressControl.ResumeLayout(true);
			this.US_OA_FWSExporterAddressControl.PerformLayout();
			this.US_OA_FWSImporterAddressControl.ResumeLayout(true);
			this.US_OA_FWSImporterAddressControl.PerformLayout();
			this.HybridSecondScientificDetailsGroupBox.ResumeLayout(false);
			this.HybridSecondScientificDetailsGroupBox.PerformLayout();
			this.PackageLabelGroupBox.ResumeLayout(false);
			this.PackageLabelGroupBox.PerformLayout();
			this.DocumentsGroupBox.ResumeLayout(false);
			this.DocumentsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel ButtonPanel;
		private ZArchitecture.GUI.ZButton OKButton;
		private ZArchitecture.GUI.ZGroupBox LicensesGroupBox;
		private ZArchitecture.ZGrid LicensesGrid;
		private ZArchitecture.GUI.ZGroupBox CommodityDetailsGroupBox;
		private ZArchitecture.ZTextBox US_CommoditySpecificNameTextBox;
		private ZArchitecture.ZTextBox US_CommodityGeneralNameTextBox;
		private ZArchitecture.GUI.ZGroupBox ScientificDetailsGroupBox;
		private ZArchitecture.ZTextBox US_ScientificGenusNameTextBox;
		private ZArchitecture.ZTextBox US_ScientificSpeciesNameTextBox;
		private ZArchitecture.GUI.ZGroupBox ProductDetailsGroupBox;
		private ZArchitecture.GUI.ZDropEdit US_WildlifeCategoryCodeDropEdit;
		private ZArchitecture.GUI.ZDropEdit US_HybridDropEdit;
		private ZArchitecture.GUI.ZDropEdit US_ProductTypeDropEdit;
		private ZArchitecture.ZTextBox US_ProductNumberTextBox;
		private ZArchitecture.GUI.ZGroupBox FWSGroupBox;
		private ZArchitecture.GUI.ZDropEdit US_ProcessingCodeDropEdit;
		private ZArchitecture.GUI.ZCheckBox US_IsDocSubmittedCheckBox;
		private ZArchitecture.GUI.ZCodeFindBox US_SpeciesOriginCodeFindBox;
		private ZArchitecture.GUI.ZGroupBox DeclarationCertificationGroupBox;
		private ZArchitecture.ZCalcEdit US_CartonQtyCalcEdit;
		private ZArchitecture.GUI.ZDropEdit US_WildlifeDescriptionCodeDropEdit;
		private ZArchitecture.GUI.ZGroupBox HybridSecondScientificDetailsGroupBox;
		private ZArchitecture.ZTextBox US_Scientific2GenusNameTextBox;
		private ZArchitecture.ZTextBox US_Scientific2SpeciesNameTextBox;
		private ZArchitecture.GUI.ZCalcDropEdit US_NetCommodityCalcDropEdit;
		private ZArchitecture.GUI.ZCodeFindBox US_FIRMSCodeFindBox;
		internal ZArchitecture.GUI.ZAddressControl US_OA_FWSExporterAddressControl;
		internal ZArchitecture.GUI.ZAddressControl US_OA_FWSImporterAddressControl;
		private ZArchitecture.GUI.ZGroupBox PackageLabelGroupBox;
		private ZArchitecture.ZTextBox US_RemarksTextTextBox;
		private ZArchitecture.GUI.ZDropEdit US_WildlifeSourceDropEdit;
		private ZArchitecture.GUI.ZDropEdit US_IntendedUseCodeDropEdit;
		private ZArchitecture.ZTextBox US_ScientificSubSpeciesNameTextBox;
		private ZArchitecture.ZTextBox US_Scientific2SubSpeciesNameTextBox;
		private ZArchitecture.GUI.ZCheckBox IsVenomousCheckBox;
		private ZArchitecture.ZCalcEdit InvCurrValueCalcEdit1;
		private ZArchitecture.ZTextBox PGAValueTextBox;
		private ZArchitecture.GUI.ZGroupBox DocumentsGroupBox;
		private ZArchitecture.ZTextBox ConfirmationNumberTextBox;
	}
}
