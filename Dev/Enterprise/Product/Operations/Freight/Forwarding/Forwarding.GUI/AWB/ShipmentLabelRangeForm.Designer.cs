namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public partial class ShipmentLabelRangeForm
	{
		protected new void InitializeComponent()
		{
			this.buttonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.okButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.contentPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.barcodeLabelsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.consolPiecesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.consolToRangeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.consolTotalLabel = new Enterprise.ZArchitecture.ZLabel();
			this.consolRangeOfLabel = new Enterprise.ZArchitecture.ZLabel();
			this.consolRangeToLabel = new Enterprise.ZArchitecture.ZLabel();
			this.consolFromLabelCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.consolPiecesFromConsol = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.consolPiecesFromMAWB = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.consolPiecesSupress = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.consolChooserDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.labelPrinterGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.shipmentPiecesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.shipmentTotalPacksCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.parentPackagesRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.awbPackagesRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.shipmentRangeOfLabel = new Enterprise.ZArchitecture.ZLabel();
			this.shipmentRangeToLabel = new Enterprise.ZArchitecture.ZLabel();
			this.shipmentToLabelCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.shipmentFromLabelCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.printOptionalInformationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.labelSizeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.sixInchRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.fiveInchRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.labelUseEPrintCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.buttonPanel.SuspendLayout();
			this.contentPanel.SuspendLayout();
			this.barcodeLabelsGroupBox.SuspendLayout();
			this.consolPiecesGroupBox.SuspendLayout();
			this.consolChooserDropEdit.SuspendLayout();
			this.labelPrinterGuidDropEdit.SuspendLayout();
			this.shipmentPiecesGroupBox.SuspendLayout();
			this.labelSizeGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 468, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.AWB.ShipmentAWBActions);
			// 
			// buttonPanel
			// 
			this.buttonPanel.Controls.Add(this.cancelButton);
			this.buttonPanel.Controls.Add(this.okButton);
			this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 427, true);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 41, true);
			this.buttonPanel.TabIndex = 1;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentLabelRangeForm|9705385e-9e01-47ef-ac53-7ffae28b4617", "&Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 10, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 1;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentLabelRangeForm|11086124-18fc-40ea-a5dc-2cabfd049ef7", "&OK");
			this.okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(295, 10, true);
			this.okButton.Name = "okButton";
			this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.okButton.TabIndex = 0;
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// contentPanel
			// 
			this.contentPanel.Controls.Add(this.barcodeLabelsGroupBox);
			this.contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.contentPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.contentPanel.Name = "contentPanel";
			this.contentPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 427, true);
			this.contentPanel.TabIndex = 2;
			// 
			// barcodeLabelsGroupBox
			// 
			this.barcodeLabelsGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentLabelRangeForm|6c3b20d4-802d-403d-ac04-60159f859ab4", "AWB Barcode Labels", "AWB Barcode Labels print options.");
			this.barcodeLabelsGroupBox.Controls.Add(this.labelUseEPrintCheckBox);
			this.barcodeLabelsGroupBox.Controls.Add(this.consolPiecesGroupBox);
			this.barcodeLabelsGroupBox.Controls.Add(this.labelPrinterGuidDropEdit);
			this.barcodeLabelsGroupBox.Controls.Add(this.shipmentPiecesGroupBox);
			this.barcodeLabelsGroupBox.Controls.Add(this.printOptionalInformationCheckBox);
			this.barcodeLabelsGroupBox.Controls.Add(this.labelSizeGroupBox);
			this.barcodeLabelsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 3, true);
			this.barcodeLabelsGroupBox.Name = "barcodeLabelsGroupBox";
			this.barcodeLabelsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(422, 380, true);
			this.barcodeLabelsGroupBox.TabIndex = 2;
			this.barcodeLabelsGroupBox.TabStop = false;
			// 
			// consolPiecesGroupBox
			// 
			this.consolPiecesGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentLabelRangeForm|2533e87a-d4d4-401e-a613-34c0ee558b2c", "Consol");
			this.consolPiecesGroupBox.Controls.Add(this.consolToRangeLabel);
			this.consolPiecesGroupBox.Controls.Add(this.consolTotalLabel);
			this.consolPiecesGroupBox.Controls.Add(this.consolRangeOfLabel);
			this.consolPiecesGroupBox.Controls.Add(this.consolRangeToLabel);
			this.consolPiecesGroupBox.Controls.Add(this.consolFromLabelCalcEdit);
			this.consolPiecesGroupBox.Controls.Add(this.consolPiecesFromConsol);
			this.consolPiecesGroupBox.Controls.Add(this.consolPiecesFromMAWB);
			this.consolPiecesGroupBox.Controls.Add(this.consolPiecesSupress);
			this.consolPiecesGroupBox.Controls.Add(this.consolChooserDropEdit);
			this.consolPiecesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 197, true);
			this.consolPiecesGroupBox.Name = "consolPiecesGroupBox";
			this.consolPiecesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(399, 119, true);
			this.consolPiecesGroupBox.TabIndex = 13;
			this.consolPiecesGroupBox.TabStop = false;
			// 
			// consolToRangeLabel
			// 
			this.consolToRangeLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.consolToRangeLabel, "ConsolTotalRangeTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Freight.Forwarding.Business.AWB.ShipmentAWBActions)(null)).ConsolTotalRangeTo)));
			this.consolToRangeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 85, true);
			this.consolToRangeLabel.Name = "consolToRangeLabel";
			this.consolToRangeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 13, true);
			this.consolToRangeLabel.TabIndex = 12;
			this.consolToRangeLabel.Text = "0";
			this.consolToRangeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// consolTotalLabel
			// 
			this.consolTotalLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.consolTotalLabel, "ConsolTotalPacks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Freight.Forwarding.Business.AWB.ShipmentAWBActions)(null)).ConsolTotalPacks)));
			this.consolTotalLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(204, 85, true);
			this.consolTotalLabel.Name = "consolTotalLabel";
			this.consolTotalLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 13, true);
			this.consolTotalLabel.TabIndex = 11;
			this.consolTotalLabel.Text = "0";
			// 
			// consolRangeOfLabel
			// 
			this.consolRangeOfLabel.AutoSize = true;
			this.consolRangeOfLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentLabelRangeForm|47ab40e9-e716-4ccf-89c5-1ef454b2509e", "of");
			this.consolRangeOfLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 85, true);
			this.consolRangeOfLabel.Name = "consolRangeOfLabel";
			this.consolRangeOfLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(17, 13, true);
			this.consolRangeOfLabel.TabIndex = 9;
			// 
			// consolRangeToLabel
			// 
			this.consolRangeToLabel.AutoSize = true;
			this.consolRangeToLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentLabelRangeForm|89a6f920-be62-4687-baca-3ffc47ec39ef", "to");
			this.consolRangeToLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 85, true);
			this.consolRangeToLabel.Name = "consolRangeToLabel";
			this.consolRangeToLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(17, 13, true);
			this.consolRangeToLabel.TabIndex = 10;
			// 
			// consolFromLabelCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.consolFromLabelCalcEdit, "ConsolTotalRangeFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ShipmentAWBActions)(null)).ConsolTotalRangeFrom)));
			this.consolFromLabelCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentLabelRangeForm|a3f36f02-09ee-4a3b-88c8-ad451eb5b905", "Range");
			this.consolFromLabelCalcEdit.DecimalPlaces = 2;
			this.consolFromLabelCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 82, true);
			this.consolFromLabelCalcEdit.Name = "consolFromLabelCalcEdit";
			this.consolFromLabelCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.consolFromLabelCalcEdit.TabIndex = 18;
			this.consolFromLabelCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// consolPiecesFromConsol
			// 
			this.consolPiecesFromConsol.AutoCheck = false;
			this.consolPiecesFromConsol.AutoSize = true;
			this.BindingSource.SetBindingMember(this.consolPiecesFromConsol, "ConsolTotalPiecesFromConsol");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.AWB.ShipmentAWBActions)(null)).ConsolTotalPiecesFromConsol)));
			this.consolPiecesFromConsol.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentLabelRangeForm|efa8e300-021f-4b1a-b647-36e0343dc613", "Consol");
			this.consolPiecesFromConsol.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.consolPiecesFromConsol.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 51, true);
			this.consolPiecesFromConsol.Name = "consolPiecesFromConsol";
			this.consolPiecesFromConsol.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 17, true);
			this.consolPiecesFromConsol.TabIndex = 16;
			this.consolPiecesFromConsol.TabStop = true;
			this.consolPiecesFromConsol.UseVisualStyleBackColor = true;
			// 
			// consolPiecesFromMAWB
			// 
			this.consolPiecesFromMAWB.AutoCheck = false;
			this.consolPiecesFromMAWB.AutoSize = true;
			this.BindingSource.SetBindingMember(this.consolPiecesFromMAWB, "ConsolTotalPiecesFromMAWB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.AWB.ShipmentAWBActions)(null)).ConsolTotalPiecesFromMAWB)));
			this.consolPiecesFromMAWB.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentLabelRangeForm|4879b46b-9db8-465e-b34e-6236eee04c92", "MAWB");
			this.consolPiecesFromMAWB.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.consolPiecesFromMAWB.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 51, true);
			this.consolPiecesFromMAWB.Name = "consolPiecesFromMAWB";
			this.consolPiecesFromMAWB.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
			this.consolPiecesFromMAWB.TabIndex = 15;
			this.consolPiecesFromMAWB.TabStop = true;
			this.consolPiecesFromMAWB.UseVisualStyleBackColor = true;
			// 
			// consolPiecesSupress
			// 
			this.consolPiecesSupress.AutoSize = true;
			this.BindingSource.SetBindingMember(this.consolPiecesSupress, "ConsolTotalPiecesSuppress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.AWB.ShipmentAWBActions)(null)).ConsolTotalPiecesSuppress)));
			this.consolPiecesSupress.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentLabelRangeForm|6e1b2197-9696-4d87-bf5d-5340cfbe6edc", "Suppress");
			this.consolPiecesSupress.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.consolPiecesSupress.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(243, 51, true);
			this.consolPiecesSupress.Name = "consolPiecesSupress";
			this.consolPiecesSupress.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 17, true);
			this.consolPiecesSupress.TabIndex = 17;
			this.consolPiecesSupress.UseVisualStyleBackColor = true;
			// 
			// consolChooserDropEdit
			// 
			this.consolChooserDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.consolChooserDropEdit, "LabelConsol");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ShipmentAWBActions)(null)).LabelConsol)));
			this.consolChooserDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentLabelRangeForm|38eb8da3-166d-4b9b-baaf-db19d94c16bf", "Consol", "Select consol.");
			this.consolChooserDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(55, 19, true);
			this.consolChooserDropEdit.Name = "consolChooserDropEdit";
			this.consolChooserDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.consolChooserDropEdit.TabIndex = 14;
			// 
			// labelPrinterGuidDropEdit
			// 
			this.labelPrinterGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.labelPrinterGuidDropEdit, "LabelPrinter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ShipmentAWBActions)(null)).LabelPrinter)));
			this.labelPrinterGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 322, true);
			this.labelPrinterGuidDropEdit.Name = "labelPrinterGuidDropEdit";
			this.labelPrinterGuidDropEdit.PreBoundMaxLength = 52;
			this.labelPrinterGuidDropEdit.ShowDescriptionBox = false;
			this.labelPrinterGuidDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.labelPrinterGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 20, true);
			this.labelPrinterGuidDropEdit.TabIndex = 19;
			// 
			// shipmentPiecesGroupBox
			// 
			this.shipmentPiecesGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentLabelRangeForm|64d7b4ef-0f8a-4a29-a72e-7e45c3c592e4", "Shipment Pieces");
			this.shipmentPiecesGroupBox.Controls.Add(this.shipmentTotalPacksCalcEdit);
			this.shipmentPiecesGroupBox.Controls.Add(this.parentPackagesRadioButton);
			this.shipmentPiecesGroupBox.Controls.Add(this.awbPackagesRadioButton);
			this.shipmentPiecesGroupBox.Controls.Add(this.shipmentRangeOfLabel);
			this.shipmentPiecesGroupBox.Controls.Add(this.shipmentRangeToLabel);
			this.shipmentPiecesGroupBox.Controls.Add(this.shipmentToLabelCalcEdit);
			this.shipmentPiecesGroupBox.Controls.Add(this.shipmentFromLabelCalcEdit);
			this.shipmentPiecesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 105, true);
			this.shipmentPiecesGroupBox.Name = "shipmentPiecesGroupBox";
			this.shipmentPiecesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(399, 86, true);
			this.shipmentPiecesGroupBox.TabIndex = 7;
			this.shipmentPiecesGroupBox.TabStop = false;
			// 
			// shipmentTotalPacksCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.shipmentTotalPacksCalcEdit, "TotalPacks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ShipmentAWBActions)(null)).TotalPacks)));
			this.shipmentTotalPacksCalcEdit.DecimalPlaces = 0;
			this.shipmentTotalPacksCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.shipmentTotalPacksCalcEdit, false);
			this.shipmentTotalPacksCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 50, true);
			this.shipmentTotalPacksCalcEdit.Name = "shipmentTotalPacksCalcEdit";
			this.shipmentTotalPacksCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.shipmentTotalPacksCalcEdit.TabIndex = 12;
			this.shipmentTotalPacksCalcEdit.Text = "0";
			this.shipmentTotalPacksCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// parentPackagesRadioButton
			// 
			this.parentPackagesRadioButton.AutoCheck = false;
			this.parentPackagesRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.parentPackagesRadioButton, "ParentPackagesLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.AWB.ShipmentAWBActions)(null)).ParentPackagesLabel)));
			this.parentPackagesRadioButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentLabelRangeForm|4c4bbcab-9f47-4472-bee6-816f01cee975", "Shipment");
			this.parentPackagesRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.parentPackagesRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 19, true);
			this.parentPackagesRadioButton.Name = "parentPackagesRadioButton";
			this.parentPackagesRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 17, true);
			this.parentPackagesRadioButton.TabIndex = 9;
			this.parentPackagesRadioButton.TabStop = true;
			// 
			// awbPackagesRadioButton
			// 
			this.awbPackagesRadioButton.AutoCheck = false;
			this.awbPackagesRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.awbPackagesRadioButton, "AWBPackagesLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.AWB.ShipmentAWBActions)(null)).AWBPackagesLabel)));
			this.awbPackagesRadioButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentLabelRangeForm|7c422d55-448a-468a-a009-b7eabfbc30b7", "HAWB");
			this.awbPackagesRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.awbPackagesRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 19, true);
			this.awbPackagesRadioButton.Name = "awbPackagesRadioButton";
			this.awbPackagesRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 17, true);
			this.awbPackagesRadioButton.TabIndex = 8;
			this.awbPackagesRadioButton.TabStop = true;
			// 
			// shipmentRangeOfLabel
			// 
			this.shipmentRangeOfLabel.AutoSize = true;
			this.shipmentRangeOfLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentLabelRangeForm|2d20fe77-ca24-413e-b37c-04332f017f96", "of");
			this.shipmentRangeOfLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(217, 53, true);
			this.shipmentRangeOfLabel.Name = "shipmentRangeOfLabel";
			this.shipmentRangeOfLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(17, 13, true);
			this.shipmentRangeOfLabel.TabIndex = 7;
			// 
			// shipmentRangeToLabel
			// 
			this.shipmentRangeToLabel.AutoSize = true;
			this.shipmentRangeToLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentLabelRangeForm|783da10f-4d3a-4c71-8e37-4dbf9e804a64", "to");
			this.shipmentRangeToLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 53, true);
			this.shipmentRangeToLabel.Name = "shipmentRangeToLabel";
			this.shipmentRangeToLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(17, 13, true);
			this.shipmentRangeToLabel.TabIndex = 5;
			// 
			// shipmentToLabelCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.shipmentToLabelCalcEdit, "LabelRangeTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ShipmentAWBActions)(null)).LabelRangeTo)));
			this.shipmentToLabelCalcEdit.DecimalPlaces = 0;
			this.shipmentToLabelCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.shipmentToLabelCalcEdit, false);
			this.shipmentToLabelCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 50, true);
			this.shipmentToLabelCalcEdit.Name = "shipmentToLabelCalcEdit";
			this.shipmentToLabelCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.shipmentToLabelCalcEdit.TabIndex = 11;
			this.shipmentToLabelCalcEdit.Text = "0";
			this.shipmentToLabelCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// shipmentFromLabelCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.shipmentFromLabelCalcEdit, "LabelRangeFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.AWB.ShipmentAWBActions)(null)).LabelRangeFrom)));
			this.shipmentFromLabelCalcEdit.DecimalPlaces = 0;
			this.shipmentFromLabelCalcEdit.Decimals = 0;
			this.shipmentFromLabelCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 50, true);
			this.shipmentFromLabelCalcEdit.Name = "shipmentFromLabelCalcEdit";
			this.shipmentFromLabelCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.shipmentFromLabelCalcEdit.TabIndex = 10;
			this.shipmentFromLabelCalcEdit.Text = "0";
			this.shipmentFromLabelCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// printOptionalInformationCheckBox
			// 
			this.printOptionalInformationCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.printOptionalInformationCheckBox, "PrintOptionalInformation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.AWB.ShipmentAWBActions)(null)).PrintOptionalInformation)));
			this.printOptionalInformationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.printOptionalInformationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(32, 19, true);
			this.printOptionalInformationCheckBox.Name = "printOptionalInformationCheckBox";
			this.printOptionalInformationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 17, true);
			this.printOptionalInformationCheckBox.TabIndex = 3;
			this.printOptionalInformationCheckBox.UseVisualStyleBackColor = true;
			// 
			// labelSizeGroupBox
			// 
			this.labelSizeGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentLabelRangeForm|3c49d460-e6a4-4816-8156-f0224ec1408c", "Label Size");
			this.labelSizeGroupBox.Controls.Add(this.sixInchRadioButton);
			this.labelSizeGroupBox.Controls.Add(this.fiveInchRadioButton);
			this.labelSizeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 54, true);
			this.labelSizeGroupBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.labelSizeGroupBox.Name = "labelSizeGroupBox";
			this.labelSizeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(399, 48, true);
			this.labelSizeGroupBox.TabIndex = 4;
			this.labelSizeGroupBox.TabStop = false;
			// 
			// sixInchRadioButton
			// 
			this.sixInchRadioButton.AutoCheck = false;
			this.sixInchRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.sixInchRadioButton, "SixInchLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.AWB.ShipmentAWBActions)(null)).SixInchLabel)));
			this.sixInchRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.sixInchRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 19, true);
			this.sixInchRadioButton.Name = "sixInchRadioButton";
			this.sixInchRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 17, true);
			this.sixInchRadioButton.TabIndex = 6;
			this.sixInchRadioButton.TabStop = true;
			// 
			// fiveInchRadioButton
			// 
			this.fiveInchRadioButton.AutoCheck = false;
			this.fiveInchRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.fiveInchRadioButton, "FiveInchLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.AWB.ShipmentAWBActions)(null)).FiveInchLabel)));
			this.fiveInchRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.fiveInchRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 19, true);
			this.fiveInchRadioButton.Name = "fiveInchRadioButton";
			this.fiveInchRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 17, true);
			this.fiveInchRadioButton.TabIndex = 5;
			this.fiveInchRadioButton.TabStop = true;
			// 
			// labelUseEPrintCheckBox
			// 
			this.BindingSource.SetBindingMember(this.labelUseEPrintCheckBox, "LabelUseEPrint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.AWB.ShipmentAWBActions)(null)).LabelUseEPrint)));
			this.labelUseEPrintCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentLabelRangeForm|95536968-d8cc-45f9-aba3-50a605c5086e", "ePrint");
			this.labelUseEPrintCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.labelUseEPrintCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(32, 349, true);
			this.labelUseEPrintCheckBox.Name = "labelUseEPrintCheckBox";
			this.labelUseEPrintCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.labelUseEPrintCheckBox.TabIndex = 20;
			// 
			// ShipmentLabelRangeForm
			// 
			this.AcceptButton = this.okButton;
			this.CancelButton = this.cancelButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentLabelRangeForm|d8097f73-089c-4c76-bcf8-3e6106c8167b", "Label Range for Printing");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 492, true);
			this.ControlBox = false;
			this.Controls.Add(this.contentPanel);
			this.Controls.Add(this.buttonPanel);
			this.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.AWB.ShipmentAWBActions);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 494, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 494, true);
			this.Name = "ShipmentLabelRangeForm";
			this.RememberFormSize = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.buttonPanel, 0);
			this.Controls.SetChildIndex(this.contentPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.buttonPanel.ResumeLayout(false);
			this.buttonPanel.PerformLayout();
			this.contentPanel.ResumeLayout(false);
			this.contentPanel.PerformLayout();
			this.barcodeLabelsGroupBox.ResumeLayout(false);
			this.barcodeLabelsGroupBox.PerformLayout();
			this.consolPiecesGroupBox.ResumeLayout(false);
			this.consolPiecesGroupBox.PerformLayout();
			this.consolChooserDropEdit.ResumeLayout(true);
			this.consolChooserDropEdit.PerformLayout();
			this.labelPrinterGuidDropEdit.ResumeLayout(true);
			this.labelPrinterGuidDropEdit.PerformLayout();
			this.shipmentPiecesGroupBox.ResumeLayout(false);
			this.shipmentPiecesGroupBox.PerformLayout();
			this.labelSizeGroupBox.ResumeLayout(false);
			this.labelSizeGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZArchitecture.GUI.ZPanel buttonPanel;
		private ZArchitecture.GUI.ZButton cancelButton;
		private ZArchitecture.GUI.ZButton okButton;
		private ZArchitecture.GUI.ZPanel contentPanel;
		private ZArchitecture.GUI.ZGroupBox barcodeLabelsGroupBox;
		private ZArchitecture.GUI.ZCheckBox printOptionalInformationCheckBox;
		private ZArchitecture.GUI.ZGroupBox labelSizeGroupBox;
		private ZArchitecture.GUI.ZRadioButton sixInchRadioButton;
		private ZArchitecture.GUI.ZRadioButton fiveInchRadioButton;
		private ZArchitecture.GUI.ZRadioButton parentPackagesRadioButton;
		private ZArchitecture.GUI.ZRadioButton awbPackagesRadioButton;
		private ZArchitecture.ZLabel shipmentRangeOfLabel;
		private ZArchitecture.ZCalcEdit shipmentToLabelCalcEdit;
		private ZArchitecture.ZCalcEdit shipmentFromLabelCalcEdit;
		private ZArchitecture.ZLabel shipmentRangeToLabel;
		private ZArchitecture.GUI.ZGuidDropEdit labelPrinterGuidDropEdit;
		private ZArchitecture.GUI.ZGroupBox shipmentPiecesGroupBox;
		private ZArchitecture.GUI.ZGroupBox consolPiecesGroupBox;
		private ZArchitecture.GUI.ZGuidDropEdit consolChooserDropEdit;
		private ZArchitecture.GUI.ZRadioButton consolPiecesFromConsol;
		private ZArchitecture.GUI.ZRadioButton consolPiecesFromMAWB;
		private ZArchitecture.GUI.ZCheckBox consolPiecesSupress;
		private ZArchitecture.ZCalcEdit consolFromLabelCalcEdit;
		private ZArchitecture.ZLabel consolRangeOfLabel;
		private ZArchitecture.ZLabel consolRangeToLabel;
		private ZArchitecture.ZLabel consolTotalLabel;
		private ZArchitecture.ZLabel consolToRangeLabel;
		private ZArchitecture.ZCalcEdit shipmentTotalPacksCalcEdit;
		private ZArchitecture.GUI.ZCheckBox labelUseEPrintCheckBox;
	}
}
