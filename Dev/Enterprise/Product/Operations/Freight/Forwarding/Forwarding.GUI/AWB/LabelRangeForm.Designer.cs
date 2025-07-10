using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public partial class LabelRangeForm : ZChildForm
	{
		ZButton Cancel_Button;
		ZButton OKButton;
		ZGroupBox BarcodeLabelsGroupBox;
		ZLabel zLabel7;
		ZCalcEdit ToLabelCalcEdit;
		ZCalcEdit FromLabelCalcEdit;
		ZLabel zLabel4;
		ZGuidDropEdit LabelPrinterGuidDropEdit;
		ZCheckBox PrintBarcodeLabelsCheckBox;
		ZCalcEdit TotalCalcEdit;
		ZGroupBox PackagesGroupBox;
		ZRadioButton ParentPackagesRadioButton;
		ZRadioButton AWBPackagesRadioButton;
		ZGroupBox LabelSizeGroupBox;
		ZRadioButton SixInchRadioButton;
		ZRadioButton FiveInchRadioButton;
		ZCheckBox PrintOptionalInformationCheckBox;
		ZCheckBox LabelUseEPrintCheckBox;

		protected new void InitializeComponent()
		{
			this.Cancel_Button = new ZButton();
			this.OKButton = new ZButton();
			this.BarcodeLabelsGroupBox = new ZGroupBox();
			this.PrintOptionalInformationCheckBox = new ZCheckBox();
			this.LabelSizeGroupBox = new ZGroupBox();
			this.SixInchRadioButton = new ZRadioButton();
			this.FiveInchRadioButton = new ZRadioButton();
			this.PackagesGroupBox = new ZGroupBox();
			this.ParentPackagesRadioButton = new ZRadioButton();
			this.AWBPackagesRadioButton = new ZRadioButton();
			this.TotalCalcEdit = new ZCalcEdit();
			this.zLabel7 = new ZLabel();
			this.ToLabelCalcEdit = new ZCalcEdit();
			this.FromLabelCalcEdit = new ZCalcEdit();
			this.zLabel4 = new ZLabel();
			this.LabelPrinterGuidDropEdit = new ZGuidDropEdit();
			this.PrintBarcodeLabelsCheckBox = new ZCheckBox();
			this.LabelUseEPrintCheckBox = new ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BarcodeLabelsGroupBox.SuspendLayout();
			this.LabelSizeGroupBox.SuspendLayout();
			this.PackagesGroupBox.SuspendLayout();
			this.LabelPrinterGuidDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 272, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 26, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 3;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(AWBActions);
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel_Button.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("LabelRangeForm|ca59e562-9b93-4b9e-8107-7e0e4ae062cc", "&Cancel");
			this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(368, 242, true);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.Cancel_Button.TabIndex = 2;
			this.Cancel_Button.Click += new EventHandler(this.Cancel_Button_Click);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("LabelRangeForm|cd60803d-f8d1-43eb-9a40-90e0c830eafd", "&OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(284, 242, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 1;
			this.OKButton.Click += new EventHandler(this.OKButton_Click);
			// 
			// BarcodeLabelsGroupBox
			// 
			this.BarcodeLabelsGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("LabelRangeForm|6c3b20d4-802d-403d-ac04-60159f859ab4", "Barcode Labels");
			this.BarcodeLabelsGroupBox.Controls.Add(this.LabelUseEPrintCheckBox);
			this.BarcodeLabelsGroupBox.Controls.Add(this.PrintOptionalInformationCheckBox);
			this.BarcodeLabelsGroupBox.Controls.Add(this.LabelSizeGroupBox);
			this.BarcodeLabelsGroupBox.Controls.Add(this.PackagesGroupBox);
			this.BarcodeLabelsGroupBox.Controls.Add(this.TotalCalcEdit);
			this.BarcodeLabelsGroupBox.Controls.Add(this.zLabel7);
			this.BarcodeLabelsGroupBox.Controls.Add(this.ToLabelCalcEdit);
			this.BarcodeLabelsGroupBox.Controls.Add(this.FromLabelCalcEdit);
			this.BarcodeLabelsGroupBox.Controls.Add(this.zLabel4);
			this.BarcodeLabelsGroupBox.Controls.Add(this.LabelPrinterGuidDropEdit);
			this.BarcodeLabelsGroupBox.Controls.Add(this.PrintBarcodeLabelsCheckBox);
			this.BarcodeLabelsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 4, true);
			this.BarcodeLabelsGroupBox.Name = "BarcodeLabelsGroupBox";
			this.BarcodeLabelsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 232, true);
			this.BarcodeLabelsGroupBox.TabIndex = 0;
			this.BarcodeLabelsGroupBox.TabStop = false;
			// 
			// PrintOptionalInformationCheckBox
			// 
			this.PrintOptionalInformationCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PrintOptionalInformationCheckBox, "PrintOptionalInformation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((AWBActions)(null)).PrintOptionalInformation)));
			this.PrintOptionalInformationCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("LabelRangeForm|1f9c544f-3a95-49b4-bc8c-9158507241e8", "Print Optional Information", "Print the Optional Information section on the IATA 606 barcode label.\r\nThis section can be configured in the system registry.");
			this.PrintOptionalInformationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PrintOptionalInformationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 22, true);
			this.PrintOptionalInformationCheckBox.Name = "PrintOptionalInformationCheckBox";
			this.PrintOptionalInformationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 17, true);
			this.PrintOptionalInformationCheckBox.TabIndex = 3;
			this.PrintOptionalInformationCheckBox.UseVisualStyleBackColor = true;
			// 
			// LabelSizeGroupBox
			// 
			this.LabelSizeGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("LabelRangeForm|3c49d460-e6a4-4816-8156-f0224ec1408c", "Label Size");
			this.LabelSizeGroupBox.Controls.Add(this.SixInchRadioButton);
			this.LabelSizeGroupBox.Controls.Add(this.FiveInchRadioButton);
			this.LabelSizeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 54, true);
			this.LabelSizeGroupBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.LabelSizeGroupBox.Name = "LabelSizeGroupBox";
			this.LabelSizeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 73, true);
			this.LabelSizeGroupBox.TabIndex = 1;
			this.LabelSizeGroupBox.TabStop = false;
			// 
			// SixInchRadioButton
			// 
			this.SixInchRadioButton.AutoCheck = false;
			this.SixInchRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SixInchRadioButton, "SixInchLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((AWBActions)(null)).SixInchLabel)));
			this.SixInchRadioButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("LabelRangeForm|76d3d2cc-b70a-4d68-aa65-e26488828a8a", "Six Inch");
			this.SixInchRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SixInchRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 42, true);
			this.SixInchRadioButton.Name = "SixInchRadioButton";
			this.SixInchRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
			this.SixInchRadioButton.TabIndex = 1;
			// 
			// FiveInchRadioButton
			// 
			this.FiveInchRadioButton.AutoCheck = false;
			this.FiveInchRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.FiveInchRadioButton, "FiveInchLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((AWBActions)(null)).FiveInchLabel)));
			this.FiveInchRadioButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("LabelRangeForm|9d4dc0b5-f851-471e-a93e-fcafc68c4013", "Five Inch");
			this.FiveInchRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.FiveInchRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 17, true);
			this.FiveInchRadioButton.Name = "FiveInchRadioButton";
			this.FiveInchRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
			this.FiveInchRadioButton.TabIndex = 0;
			// 
			// PackagesGroupBox
			// 
			this.PackagesGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("LabelRangeForm|6159de23-69b5-49b0-87f5-fb74f25fff63", "Packages From");
			this.PackagesGroupBox.Controls.Add(this.ParentPackagesRadioButton);
			this.PackagesGroupBox.Controls.Add(this.AWBPackagesRadioButton);
			this.PackagesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 54, true);
			this.PackagesGroupBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.PackagesGroupBox.Name = "PackagesGroupBox";
			this.PackagesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 73, true);
			this.PackagesGroupBox.TabIndex = 2;
			this.PackagesGroupBox.TabStop = false;
			// 
			// ParentPackagesRadioButton
			// 
			this.ParentPackagesRadioButton.AutoCheck = false;
			this.ParentPackagesRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ParentPackagesRadioButton, "ParentPackagesLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((AWBActions)(null)).ParentPackagesLabel)));
			this.ParentPackagesRadioButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("LabelRangeForm|4c4bbcab-9f47-4472-bee6-816f01cee975", "Consol");
			this.ParentPackagesRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ParentPackagesRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 42, true);
			this.ParentPackagesRadioButton.Name = "ParentPackagesRadioButton";
			this.ParentPackagesRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
			this.ParentPackagesRadioButton.TabIndex = 1;
			// 
			// AWBPackagesRadioButton
			// 
			this.AWBPackagesRadioButton.AutoCheck = false;
			this.AWBPackagesRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AWBPackagesRadioButton, "AWBPackagesLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((AWBActions)(null)).AWBPackagesLabel)));
			this.AWBPackagesRadioButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("LabelRangeForm|7c422d55-448a-468a-a009-b7eabfbc30b7", "AWB");
			this.AWBPackagesRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AWBPackagesRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 17, true);
			this.AWBPackagesRadioButton.Name = "AWBPackagesRadioButton";
			this.AWBPackagesRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
			this.AWBPackagesRadioButton.TabIndex = 0;
			// 
			// TotalCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalCalcEdit, "TotalPacks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((AWBActions)(null)).TotalPacks)));
			this.TotalCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("LabelRangeForm|21a8a673-6de2-4bc6-ba7f-7f2b8785348f", "Of");
			this.TotalCalcEdit.DecimalPlaces = 0;
			this.TotalCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TotalCalcEdit, false);
			this.TotalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(260, 142, true);
			this.TotalCalcEdit.Name = "TotalCalcEdit";
			this.TotalCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.TotalCalcEdit.TabIndex = 8;
			this.TotalCalcEdit.Text = "TOTAL";
			this.TotalCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zLabel7
			// 
			this.zLabel7.AutoSize = true;
			this.zLabel7.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("LabelRangeForm|2d20fe77-ca24-413e-b37c-04332f017f96", "of");
			this.zLabel7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 144, true);
			this.zLabel7.Name = "zLabel7";
			this.zLabel7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(17, 13, true);
			this.zLabel7.TabIndex = 7;
			// 
			// ToLabelCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ToLabelCalcEdit, "LabelRangeTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((AWBActions)(null)).LabelRangeTo)));
			this.ToLabelCalcEdit.DecimalPlaces = 0;
			this.ToLabelCalcEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ToLabelCalcEdit, false);
			this.ToLabelCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 142, true);
			this.ToLabelCalcEdit.Name = "ToLabelCalcEdit";
			this.ToLabelCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.ToLabelCalcEdit.TabIndex = 6;
			this.ToLabelCalcEdit.Text = "0";
			this.ToLabelCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// FromLabelCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.FromLabelCalcEdit, "LabelRangeFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((AWBActions)(null)).LabelRangeFrom)));
			this.FromLabelCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("LabelRangeForm|45e9b4ce-fb09-4f9f-9741-418c432d20c0", "Range");
			this.FromLabelCalcEdit.DecimalPlaces = 0;
			this.FromLabelCalcEdit.Decimals = 0;
			this.FromLabelCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 142, true);
			this.FromLabelCalcEdit.Name = "FromLabelCalcEdit";
			this.FromLabelCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.FromLabelCalcEdit.TabIndex = 4;
			this.FromLabelCalcEdit.Text = "0";
			this.FromLabelCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zLabel4
			// 
			this.zLabel4.AutoSize = true;
			this.zLabel4.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("LabelRangeForm|783da10f-4d3a-4c71-8e37-4dbf9e804a64", "to");
			this.zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 144, true);
			this.zLabel4.Name = "zLabel4";
			this.zLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(17, 13, true);
			this.zLabel4.TabIndex = 5;
			// 
			// LabelPrinterGuidDropEdit
			// 
			this.LabelPrinterGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LabelPrinterGuidDropEdit, "LabelPrinter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((AWBActions)(null)).LabelPrinter)));
			this.LabelPrinterGuidDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("LabelRangeForm|8128a8ed-bb2f-4cd5-8c55-cf75523c54bf", "Printer");
			this.LabelPrinterGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 171, true);
			this.LabelPrinterGuidDropEdit.Name = "LabelPrinterGuidDropEdit";
			this.LabelPrinterGuidDropEdit.PreBoundMaxLength = 52;
			this.LabelPrinterGuidDropEdit.ShowDescriptionBox = false;
			this.LabelPrinterGuidDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.LabelPrinterGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 20, true);
			this.LabelPrinterGuidDropEdit.TabIndex = 9;
			// 
			// PrintBarcodeLabelsCheckBox
			// 
			this.PrintBarcodeLabelsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PrintBarcodeLabelsCheckBox, "PrintBarcodeLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((AWBActions)(null)).PrintBarcodeLabel)));
			this.PrintBarcodeLabelsCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("LabelRangeForm|3f207ae4-e2aa-401b-9832-8b76a8c00552", "Print Barcode Labels");
			this.PrintBarcodeLabelsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PrintBarcodeLabelsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 22, true);
			this.PrintBarcodeLabelsCheckBox.Name = "PrintBarcodeLabelsCheckBox";
			this.PrintBarcodeLabelsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 17, true);
			this.PrintBarcodeLabelsCheckBox.TabIndex = 0;
			// 
			// LabelUseEPrintCheckBox
			// 
			this.BindingSource.SetBindingMember(this.LabelUseEPrintCheckBox, "LabelUseEPrint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((AWBActions)(null)).LabelUseEPrint)));
			this.LabelUseEPrintCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("LabelRangeForm|48e38e23-ead3-4187-941f-fb3af47b9a44", "ePrint");
			this.LabelUseEPrintCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LabelUseEPrintCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 197, true);
			this.LabelUseEPrintCheckBox.Name = "LabelUseEPrintCheckBox";
			this.LabelUseEPrintCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.LabelUseEPrintCheckBox.TabIndex = 10;
			// 
			// LabelRangeForm
			// 
			this.AcceptButton = this.OKButton;
			this.CancelButton = this.Cancel_Button;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("LabelRangeForm|7d76002b-68f1-444d-b5cb-8e937525df2e", "Label Range for Printing");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 298, true);
			this.ControlBox = false;
			this.Controls.Add(this.BarcodeLabelsGroupBox);
			this.Controls.Add(this.Cancel_Button);
			this.Controls.Add(this.OKButton);
			this.DataSourceAssemblyName = "Enterprise.Freight.Forwarding.Business";
			this.DataSourceType = typeof(AWBActions);
			this.DataSourceTypeName = "Enterprise.Freight.Forwarding.Business.AWB.AWBActions";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.Name = "LabelRangeForm";
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.Cancel_Button, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BarcodeLabelsGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BarcodeLabelsGroupBox.ResumeLayout(false);
			this.BarcodeLabelsGroupBox.PerformLayout();
			this.LabelSizeGroupBox.ResumeLayout(false);
			this.LabelSizeGroupBox.PerformLayout();
			this.PackagesGroupBox.ResumeLayout(false);
			this.PackagesGroupBox.PerformLayout();
			this.LabelPrinterGuidDropEdit.ResumeLayout(true);
			this.LabelPrinterGuidDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		System.ComponentModel.Container components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
