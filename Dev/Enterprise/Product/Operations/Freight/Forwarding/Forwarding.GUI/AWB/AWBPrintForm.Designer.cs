using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public partial class AWBPrintForm : ZChildForm
	{
		ZButton OKButton;
		ZButton Cancel_Button;
		ZGroupBox BarcodeLabelsGroupBox;
		ZCheckBox PrintBarcodeLabelsCheckBox;
		ZGuidDropEdit LabelPrinterGuidDropEdit;
		ZArchitecture.ZCalcEdit FromLabelCalcEdit;
		ZArchitecture.ZCalcEdit ToLabelCalcEdit;
		ZArchitecture.ZLabel zLabel7;
		ZArchitecture.ZLabel TotalLabel;
		ZRadioButton FiveInchRadioButton;
		ZRadioButton SixInchRadioButton;
		private ZCheckBox PrintOptionalInformationCheckBox;
		private ZGroupBox HAWBBarcodeLabelsGroupBox;
		private ZGuidDropEdit HAWBLabelPrinterGUIDDropEdit;
		private ZCheckBox PrintHAWBBarcodeLabelsCheckBox;
		private ZNumericUpDown AWBLabelCopiesNumericUpDown;
		private ZNumericUpDown HAWBLabelCopiesNumericUpDown;
		private MAWBPrintOptionsControl MAWBPrintOptionsControl;
		private CIMPOptionsControl cimpOptionsControl1;
		private ZCheckBox HAWBLabelUseEPrintCheckBox;
		private ZCheckBox LabelUseEPrintCheckBox;

		protected new void InitializeComponent()
		{
			this.OKButton = new ZButton();
			this.Cancel_Button = new ZButton();
			this.PrintBarcodeLabelsCheckBox = new ZCheckBox();
			this.BarcodeLabelsGroupBox = new ZGroupBox();
			this.LabelUseEPrintCheckBox = new ZCheckBox();
			this.AWBLabelCopiesNumericUpDown = new ZNumericUpDown();
			this.PrintOptionalInformationCheckBox = new ZCheckBox();
			this.SixInchRadioButton = new ZRadioButton();
			this.FiveInchRadioButton = new ZRadioButton();
			this.TotalLabel = new ZArchitecture.ZLabel();
			this.zLabel7 = new ZArchitecture.ZLabel();
			this.ToLabelCalcEdit = new ZArchitecture.ZCalcEdit();
			this.FromLabelCalcEdit = new ZArchitecture.ZCalcEdit();
			this.LabelPrinterGuidDropEdit = new ZGuidDropEdit();
			this.HAWBBarcodeLabelsGroupBox = new ZGroupBox();
			this.HAWBLabelUseEPrintCheckBox = new ZCheckBox();
			this.HAWBLabelCopiesNumericUpDown = new ZNumericUpDown();
			this.HAWBLabelPrinterGUIDDropEdit = new ZGuidDropEdit();
			this.PrintHAWBBarcodeLabelsCheckBox = new ZCheckBox();
			this.cimpOptionsControl1 = new CIMPOptionsControl();
			this.MAWBPrintOptionsControl = new MAWBPrintOptionsControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BarcodeLabelsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AWBLabelCopiesNumericUpDown)).BeginInit();
			this.AWBLabelCopiesNumericUpDown.SuspendLayout();
			this.LabelPrinterGuidDropEdit.SuspendLayout();
			this.HAWBBarcodeLabelsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.HAWBLabelCopiesNumericUpDown)).BeginInit();
			this.HAWBLabelCopiesNumericUpDown.SuspendLayout();
			this.HAWBLabelPrinterGUIDDropEdit.SuspendLayout();
			this.cimpOptionsControl1.SuspendLayout();
			this.MAWBPrintOptionsControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 558, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(459, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 6;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(227);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(227);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ConsolAWBActions);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBPrintForm|86ce7357-c74f-45e6-94ec-2a7306e7ec61", "&OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(291, 532, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 4;
			this.OKButton.Click += new EventHandler(this.OKButton_Click);
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel_Button.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBPrintForm|b27d8114-9967-4696-b949-ae396cc52e7a", "&Cancel");
			this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(375, 532, true);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.Cancel_Button.TabIndex = 5;
			// 
			// PrintBarcodeLabelsCheckBox
			// 
			this.PrintBarcodeLabelsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PrintBarcodeLabelsCheckBox, "PrintBarcodeLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((ConsolAWBActions)(null)).PrintBarcodeLabel)));
			this.PrintBarcodeLabelsCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBPrintForm|8e7ec61f-d2d5-4e4d-ba82-3d7fa3efb0a0", "Print Barcode Label", "Specifies whether barcode labels should also be printed.");
			this.PrintBarcodeLabelsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PrintBarcodeLabelsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 24, true);
			this.PrintBarcodeLabelsCheckBox.Name = "PrintBarcodeLabelsCheckBox";
			this.PrintBarcodeLabelsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 17, true);
			this.PrintBarcodeLabelsCheckBox.TabIndex = 0;
			// 
			// BarcodeLabelsGroupBox
			// 
			this.BarcodeLabelsGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBPrintForm|4fa0fbba-2b6d-4af3-a97a-aa55c68bbdde", "AWB Barcode Labels", "AWB Barcode Labels print options.");
			this.BarcodeLabelsGroupBox.Controls.Add(this.LabelUseEPrintCheckBox);
			this.BarcodeLabelsGroupBox.Controls.Add(this.AWBLabelCopiesNumericUpDown);
			this.BarcodeLabelsGroupBox.Controls.Add(this.PrintOptionalInformationCheckBox);
			this.BarcodeLabelsGroupBox.Controls.Add(this.SixInchRadioButton);
			this.BarcodeLabelsGroupBox.Controls.Add(this.FiveInchRadioButton);
			this.BarcodeLabelsGroupBox.Controls.Add(this.TotalLabel);
			this.BarcodeLabelsGroupBox.Controls.Add(this.zLabel7);
			this.BarcodeLabelsGroupBox.Controls.Add(this.ToLabelCalcEdit);
			this.BarcodeLabelsGroupBox.Controls.Add(this.FromLabelCalcEdit);
			this.BarcodeLabelsGroupBox.Controls.Add(this.LabelPrinterGuidDropEdit);
			this.BarcodeLabelsGroupBox.Controls.Add(this.PrintBarcodeLabelsCheckBox);
			this.BarcodeLabelsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 161, true);
			this.BarcodeLabelsGroupBox.Name = "BarcodeLabelsGroupBox";
			this.BarcodeLabelsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 161, true);
			this.BarcodeLabelsGroupBox.TabIndex = 1;
			this.BarcodeLabelsGroupBox.TabStop = false;
			// 
			// LabelUseEPrintCheckBox
			// 
			this.BindingSource.SetBindingMember(this.LabelUseEPrintCheckBox, "LabelUseEPrint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((ConsolAWBActions)(null)).LabelUseEPrint)));
			this.LabelUseEPrintCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBPrintForm|9391357e-7ce8-4e6f-a80a-c94408c1ce31", "ePrint");
			this.LabelUseEPrintCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LabelUseEPrintCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 133, true);
			this.LabelUseEPrintCheckBox.Name = "LabelUseEPrintCheckBox";
			this.LabelUseEPrintCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.LabelUseEPrintCheckBox.TabIndex = 10;
			// 
			// AWBLabelCopiesNumericUpDown
			// 
			this.BindingSource.SetBindingMember(this.AWBLabelCopiesNumericUpDown, "AWBLabelCopies");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((decimal)(((ConsolAWBActions)(null)).AWBLabelCopies)));
			this.AWBLabelCopiesNumericUpDown.BindTo = "AWBLabelCopies";
			this.AWBLabelCopiesNumericUpDown.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBPrintForm|fb007bbe-b06c-4dc6-97f1-666cc9deaa18", "Copies", "No of Copies", "Number of Copies", "The number of copies of the AWB barcode labels to print.");
			this.AWBLabelCopiesNumericUpDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(347, 84, true);
			this.AWBLabelCopiesNumericUpDown.Name = "AWBLabelCopiesNumericUpDown";
			this.AWBLabelCopiesNumericUpDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.AWBLabelCopiesNumericUpDown.TabIndex = 8;
			this.AWBLabelCopiesNumericUpDown.Value = new decimal(new int[] {
			1,
			0,
			0,
			0 });
			// 
			// PrintOptionalInformationCheckBox
			// 
			this.PrintOptionalInformationCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PrintOptionalInformationCheckBox, "PrintOptionalInformation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((ConsolAWBActions)(null)).PrintOptionalInformation)));
			this.PrintOptionalInformationCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBPrintForm|1f9c544f-3a95-49b4-bc8c-9158507241e8", "Print Optional Information", "Print the Optional Information section on the IATA 606 barcode label.\r\nThis section can be configured in the system registry.");
			this.PrintOptionalInformationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PrintOptionalInformationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(172, 24, true);
			this.PrintOptionalInformationCheckBox.Name = "PrintOptionalInformationCheckBox";
			this.PrintOptionalInformationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 17, true);
			this.PrintOptionalInformationCheckBox.TabIndex = 1;
			this.PrintOptionalInformationCheckBox.UseVisualStyleBackColor = true;
			// 
			// SixInchRadioButton
			// 
			this.SixInchRadioButton.AutoCheck = false;
			this.SixInchRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SixInchRadioButton, "SixInchLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((ConsolAWBActions)(null)).SixInchLabel)));
			this.SixInchRadioButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBPrintForm|5109d0f7-7927-4d19-b136-b16123155db6", "Print 6\"", "Print 6\" Barcode Label", "Specifies that 6\" barcode labels should be printed.");
			this.SixInchRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SixInchRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(172, 52, true);
			this.SixInchRadioButton.Name = "SixInchRadioButton";
			this.SixInchRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 17, true);
			this.SixInchRadioButton.TabIndex = 3;
			// 
			// FiveInchRadioButton
			// 
			this.FiveInchRadioButton.AutoCheck = false;
			this.FiveInchRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.FiveInchRadioButton, "FiveInchLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((ConsolAWBActions)(null)).FiveInchLabel)));
			this.FiveInchRadioButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBPrintForm|c74d4b2b-e440-453b-ba82-2dd0dddba3c1", "5\" Labels", "Print 5\" Barcode Labels", "Specifies that 5\" barcode labels should be printed.");
			this.FiveInchRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.FiveInchRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 52, true);
			this.FiveInchRadioButton.Name = "FiveInchRadioButton";
			this.FiveInchRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 17, true);
			this.FiveInchRadioButton.TabIndex = 2;
			// 
			// TotalLabel
			// 
			this.TotalLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.TotalLabel, "ConsolOuterPacksCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ConsolAWBActions)(null)).ConsolOuterPacksCount)));
			this.TotalLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBPrintForm|61818801-55cd-441c-80d3-75566d616478", "total");
			this.TotalLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(245, 87, true);
			this.TotalLabel.Name = "TotalLabel";
			this.TotalLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(29, 13, true);
			this.TotalLabel.TabIndex = 7;
			// 
			// zLabel7
			// 
			this.zLabel7.AutoSize = true;
			this.zLabel7.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBPrintForm|d51b116d-db9a-428a-9080-587304c2dc93", "of");
			this.zLabel7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 87, true);
			this.zLabel7.Name = "zLabel7";
			this.zLabel7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(17, 13, true);
			this.zLabel7.TabIndex = 6;
			// 
			// ToLabelCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ToLabelCalcEdit, "LabelRangeTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((ConsolAWBActions)(null)).LabelRangeTo)));
			this.ToLabelCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBPrintForm|c5916c34-dbbb-44fd-9e5d-f4b4abffaf31", "To", "Label Range To", "The ending count for the label range printed.");
			this.ToLabelCalcEdit.DecimalPlaces = 0;
			this.ToLabelCalcEdit.Decimals = 0;
			this.ToLabelCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 85, true);
			this.ToLabelCalcEdit.Name = "ToLabelCalcEdit";
			this.ToLabelCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.ToLabelCalcEdit.TabIndex = 5;
			this.ToLabelCalcEdit.Text = "0";
			this.ToLabelCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// FromLabelCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.FromLabelCalcEdit, "LabelRangeFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((ConsolAWBActions)(null)).LabelRangeFrom)));
			this.FromLabelCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBPrintForm|1e988cd0-ab84-4374-868d-c60d4e20b19f", "From", "Label Range From", "The starting count for the label range printed.");
			this.FromLabelCalcEdit.DecimalPlaces = 0;
			this.FromLabelCalcEdit.Decimals = 0;
			this.FromLabelCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 84, true);
			this.FromLabelCalcEdit.Name = "FromLabelCalcEdit";
			this.FromLabelCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.FromLabelCalcEdit.TabIndex = 4;
			this.FromLabelCalcEdit.Text = "0";
			this.FromLabelCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LabelPrinterGuidDropEdit
			// 
			this.LabelPrinterGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LabelPrinterGuidDropEdit, "LabelPrinter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ConsolAWBActions)(null)).LabelPrinter)));
			this.LabelPrinterGuidDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBPrintForm|e91af524-62b3-4cf5-95f8-6271ad439f80", "Printer", "Label Printer", "The printer to use for printing AWB barcode labels.");
			this.LabelPrinterGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 110, true);
			this.LabelPrinterGuidDropEdit.Name = "LabelPrinterGuidDropEdit";
			this.LabelPrinterGuidDropEdit.PreBoundMaxLength = 52;
			this.LabelPrinterGuidDropEdit.ShowDescriptionBox = false;
			this.LabelPrinterGuidDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.LabelPrinterGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 20, true);
			this.LabelPrinterGuidDropEdit.TabIndex = 9;
			// 
			// HAWBBarcodeLabelsGroupBox
			// 
			this.HAWBBarcodeLabelsGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBPrintForm|a5a99377-9254-4040-ac17-7484c548ef72", "HAWB Barcode Labels", "Options for printing HAWB barcode labels.");
			this.HAWBBarcodeLabelsGroupBox.Controls.Add(this.HAWBLabelUseEPrintCheckBox);
			this.HAWBBarcodeLabelsGroupBox.Controls.Add(this.HAWBLabelCopiesNumericUpDown);
			this.HAWBBarcodeLabelsGroupBox.Controls.Add(this.HAWBLabelPrinterGUIDDropEdit);
			this.HAWBBarcodeLabelsGroupBox.Controls.Add(this.PrintHAWBBarcodeLabelsCheckBox);
			this.HAWBBarcodeLabelsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 328, true);
			this.HAWBBarcodeLabelsGroupBox.Name = "HAWBBarcodeLabelsGroupBox";
			this.HAWBBarcodeLabelsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 99, true);
			this.HAWBBarcodeLabelsGroupBox.TabIndex = 2;
			this.HAWBBarcodeLabelsGroupBox.TabStop = false;
			// 
			// HAWBLabelUseEPrintCheckBox
			// 
			this.BindingSource.SetBindingMember(this.HAWBLabelUseEPrintCheckBox, "HAWBLabelUseEPrint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((ConsolAWBActions)(null)).HAWBLabelUseEPrint)));
			this.HAWBLabelUseEPrintCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBPrintForm|9391357e-7ce8-4e6f-a80a-c94408c1ce31", "ePrint");
			this.HAWBLabelUseEPrintCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.HAWBLabelUseEPrintCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 71, true);
			this.HAWBLabelUseEPrintCheckBox.Name = "HAWBLabelUseEPrintCheckBox";
			this.HAWBLabelUseEPrintCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.HAWBLabelUseEPrintCheckBox.TabIndex = 3;
			// 
			// HAWBLabelCopiesNumericUpDown
			// 
			this.BindingSource.SetBindingMember(this.HAWBLabelCopiesNumericUpDown, "HAWBLabelCopies");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((decimal)(((ConsolAWBActions)(null)).HAWBLabelCopies)));
			this.HAWBLabelCopiesNumericUpDown.BindTo = "HAWBLabelCopies";
			this.HAWBLabelCopiesNumericUpDown.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBPrintForm|5c292014-25cc-419a-b0e6-25084372c564", "Copies", "No of Copies", "Number of Copies", "The number of copies of the HAWB barcode labels to print.");
			this.HAWBLabelCopiesNumericUpDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(347, 22, true);
			this.HAWBLabelCopiesNumericUpDown.Name = "HAWBLabelCopiesNumericUpDown";
			this.HAWBLabelCopiesNumericUpDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.HAWBLabelCopiesNumericUpDown.TabIndex = 1;
			this.HAWBLabelCopiesNumericUpDown.Value = new decimal(new int[] {
			1,
			0,
			0,
			0 });
			// 
			// HAWBLabelPrinterGUIDDropEdit
			// 
			this.HAWBLabelPrinterGUIDDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HAWBLabelPrinterGUIDDropEdit, "HAWBLabelPrinter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ConsolAWBActions)(null)).HAWBLabelPrinter)));
			this.HAWBLabelPrinterGUIDDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBPrintForm|8a613630-4850-4c43-b4ae-9fa08178aa5f", "Printer", "Label Printer", "The Printer to use for printing HAWB barcode labels.");
			this.HAWBLabelPrinterGUIDDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 48, true);
			this.HAWBLabelPrinterGUIDDropEdit.Name = "HAWBLabelPrinterGUIDDropEdit";
			this.HAWBLabelPrinterGUIDDropEdit.PreBoundMaxLength = 52;
			this.HAWBLabelPrinterGUIDDropEdit.ShowDescriptionBox = false;
			this.HAWBLabelPrinterGUIDDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.HAWBLabelPrinterGUIDDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 20, true);
			this.HAWBLabelPrinterGUIDDropEdit.TabIndex = 2;
			// 
			// PrintHAWBBarcodeLabelsCheckBox
			// 
			this.PrintHAWBBarcodeLabelsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PrintHAWBBarcodeLabelsCheckBox, "PrintHAWBBarcodeLabels");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((ConsolAWBActions)(null)).PrintHAWBBarcodeLabels)));
			this.PrintHAWBBarcodeLabelsCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBPrintForm|7c1ea604-681e-4835-8015-9e8c16fa67ee", "Print 5\" HAWB Barcode Labels", "Specifies whether HAWB barcode labels should also be printed.");
			this.PrintHAWBBarcodeLabelsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PrintHAWBBarcodeLabelsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 24, true);
			this.PrintHAWBBarcodeLabelsCheckBox.Name = "PrintHAWBBarcodeLabelsCheckBox";
			this.PrintHAWBBarcodeLabelsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.PrintHAWBBarcodeLabelsCheckBox.TabIndex = 0;
			// 
			// cimpOptionsControl1
			// 
			this.cimpOptionsControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cimpOptionsControl1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ISendCIMP)(((ConsolAWBActions)(null)))));
			this.cimpOptionsControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 431, true);
			this.cimpOptionsControl1.Name = "cimpOptionsControl1";
			this.cimpOptionsControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(454, 92, true);
			this.cimpOptionsControl1.TabIndex = 3;
			// 
			// MAWBPrintOptionsControl
			// 
			this.MAWBPrintOptionsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MAWBPrintOptionsControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((IPrintMAWB)(((ConsolAWBActions)(null)))));
			this.MAWBPrintOptionsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.MAWBPrintOptionsControl.Name = "MAWBPrintOptionsControl";
			this.MAWBPrintOptionsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(454, 153, true);
			this.MAWBPrintOptionsControl.TabIndex = 0;
			// 
			// AWBPrintForm
			// 
			this.CancelButton = this.Cancel_Button;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBPrintForm|b87708f8-b0fe-4084-a4cc-f2319b7c570b", "Master AWB Options");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(459, 582, true);
			this.ControlBox = false;
			this.Controls.Add(this.HAWBBarcodeLabelsGroupBox);
			this.Controls.Add(this.cimpOptionsControl1);
			this.Controls.Add(this.MAWBPrintOptionsControl);
			this.Controls.Add(this.BarcodeLabelsGroupBox);
			this.Controls.Add(this.Cancel_Button);
			this.Controls.Add(this.OKButton);
			this.DataSourceAssemblyName = "Enterprise.Freight.Forwarding.Business";
			this.DataSourceType = typeof(ConsolAWBActions);
			this.DataSourceTypeName = "Enterprise.Freight.Forwarding.Business.AWB.ConsolAWBActions";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.Name = "AWBPrintForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.Cancel_Button, 0);
			this.Controls.SetChildIndex(this.BarcodeLabelsGroupBox, 0);
			this.Controls.SetChildIndex(this.MAWBPrintOptionsControl, 0);
			this.Controls.SetChildIndex(this.cimpOptionsControl1, 0);
			this.Controls.SetChildIndex(this.HAWBBarcodeLabelsGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BarcodeLabelsGroupBox.ResumeLayout(false);
			this.BarcodeLabelsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AWBLabelCopiesNumericUpDown)).EndInit();
			this.AWBLabelCopiesNumericUpDown.ResumeLayout(false);
			this.AWBLabelCopiesNumericUpDown.PerformLayout();
			this.LabelPrinterGuidDropEdit.ResumeLayout(true);
			this.LabelPrinterGuidDropEdit.PerformLayout();
			this.HAWBBarcodeLabelsGroupBox.ResumeLayout(false);
			this.HAWBBarcodeLabelsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.HAWBLabelCopiesNumericUpDown)).EndInit();
			this.HAWBLabelCopiesNumericUpDown.ResumeLayout(false);
			this.HAWBLabelCopiesNumericUpDown.PerformLayout();
			this.HAWBLabelPrinterGUIDDropEdit.ResumeLayout(true);
			this.HAWBLabelPrinterGUIDDropEdit.PerformLayout();
			this.cimpOptionsControl1.ResumeLayout(true);
			this.cimpOptionsControl1.PerformLayout();
			this.MAWBPrintOptionsControl.ResumeLayout(true);
			this.MAWBPrintOptionsControl.PerformLayout();
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
