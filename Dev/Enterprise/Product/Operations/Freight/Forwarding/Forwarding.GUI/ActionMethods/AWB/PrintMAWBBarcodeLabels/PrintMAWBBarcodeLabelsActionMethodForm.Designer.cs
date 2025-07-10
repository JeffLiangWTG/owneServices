using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	partial class PrintMAWBBarcodeLabelsActionMethodForm
	{
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		Enterprise.ZArchitecture.GUI.ZButton OKButton;
		Enterprise.ZArchitecture.GUI.ZButton Cancel_Button;
		Enterprise.ZArchitecture.GUI.ZGroupBox BarcodeLabelsGroupBox;
		Enterprise.ZArchitecture.GUI.ZGuidDropEdit LabelPrinterGuidDropEdit;
		Enterprise.ZArchitecture.GUI.ZRadioButton FiveInchRadioButton;
		Enterprise.ZArchitecture.GUI.ZRadioButton SixInchRadioButton;
		private ZCheckBox PrintOptionalInformationCheckBox;
		System.ComponentModel.Container components = null;

		protected new void InitializeComponent()
		{
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Cancel_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.BarcodeLabelsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LabelUseEPrintCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PackagesFromGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AWBPackagesRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.ConsolPackagesRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.LabelSizeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FiveInchRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.SixInchRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.PrintOptionalInformationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CopiesNumericUpDown = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
			this.LabelPrinterGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BarcodeLabelsGroupBox.SuspendLayout();
			this.PackagesFromGroupBox.SuspendLayout();
			this.LabelSizeGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CopiesNumericUpDown)).BeginInit();
			this.CopiesNumericUpDown.SuspendLayout();
			this.LabelPrinterGuidDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 232, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 5;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = 227;
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = 227;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.AWB.BulkMAWBLabelActions);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBPrintForm|86ce7357-c74f-45e6-94ec-2a7306e7ec61", "&OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 204, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 28;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel_Button.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBPrintForm|b27d8114-9967-4696-b949-ae396cc52e7a", "&Cancel");
			this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(332, 204, true);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.Cancel_Button.TabIndex = 29;
			// 
			// BarcodeLabelsGroupBox
			// 
			this.BarcodeLabelsGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("18444529-42f1-432a-afe6-60f5b182c490", "Barcode Labels");
			this.BarcodeLabelsGroupBox.Controls.Add(this.LabelUseEPrintCheckBox);
			this.BarcodeLabelsGroupBox.Controls.Add(this.PackagesFromGroupBox);
			this.BarcodeLabelsGroupBox.Controls.Add(this.LabelSizeGroupBox);
			this.BarcodeLabelsGroupBox.Controls.Add(this.PrintOptionalInformationCheckBox);
			this.BarcodeLabelsGroupBox.Controls.Add(this.CopiesNumericUpDown);
			this.BarcodeLabelsGroupBox.Controls.Add(this.LabelPrinterGuidDropEdit);
			this.BarcodeLabelsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 10, true);
			this.BarcodeLabelsGroupBox.Name = "BarcodeLabelsGroupBox";
			this.BarcodeLabelsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(401, 185, true);
			this.BarcodeLabelsGroupBox.TabIndex = 8;
			this.BarcodeLabelsGroupBox.TabStop = false;
			// 
			// LabelUseEPrintCheckBox
			// 
			this.BindingSource.SetBindingMember(this.LabelUseEPrintCheckBox, "AWBLabelUseEPrint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.AWB.BulkMAWBLabelActions)(null)).AWBLabelUseEPrint)));
			this.LabelUseEPrintCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBPrintForm|b467d510-f712-4903-a886-5afaeae11057", "ePrint");
			this.LabelUseEPrintCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LabelUseEPrintCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 151, true);
			this.LabelUseEPrintCheckBox.Name = "LabelUseEPrintCheckBox";
			this.LabelUseEPrintCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.LabelUseEPrintCheckBox.TabIndex = 30;
			// 
			// PackagesFromGroupBox
			// 
			this.PackagesFromGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("b9b1e0af-c683-4cc2-a318-7ac98d68d27f", "Packages From");
			this.PackagesFromGroupBox.Controls.Add(this.AWBPackagesRadioButton);
			this.PackagesFromGroupBox.Controls.Add(this.ConsolPackagesRadioButton);
			this.PackagesFromGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 48, true);
			this.PackagesFromGroupBox.Name = "PackagesFromGroupBox";
			this.PackagesFromGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 71, true);
			this.PackagesFromGroupBox.TabIndex = 20;
			this.PackagesFromGroupBox.TabStop = false;
			// 
			// AWBPackagesRadioButton
			// 
			this.AWBPackagesRadioButton.AutoCheck = false;
			this.AWBPackagesRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AWBPackagesRadioButton, "PackagesFromAWB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.AWB.BulkMAWBLabelActions)(null)).PackagesFromAWB)));
			this.AWBPackagesRadioButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("878500e6-dc97-4e3b-95de-2bbf80dcae80", "AWB");
			this.AWBPackagesRadioButton.Checked = true;
			this.AWBPackagesRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AWBPackagesRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 19, true);
			this.AWBPackagesRadioButton.Name = "AWBPackagesRadioButton";
			this.AWBPackagesRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 17, true);
			this.AWBPackagesRadioButton.TabIndex = 11;
			this.AWBPackagesRadioButton.TabStop = true;
			// 
			// ConsolPackagesRadioButton
			// 
			this.ConsolPackagesRadioButton.AutoCheck = false;
			this.ConsolPackagesRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ConsolPackagesRadioButton, "PackagesFromConsol");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.AWB.BulkMAWBLabelActions)(null)).PackagesFromConsol)));
			this.ConsolPackagesRadioButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("08bec701-8f10-4e8f-816a-dc79e14bb0be", "Consol");
			this.ConsolPackagesRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ConsolPackagesRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 42, true);
			this.ConsolPackagesRadioButton.Name = "ConsolPackagesRadioButton";
			this.ConsolPackagesRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 17, true);
			this.ConsolPackagesRadioButton.TabIndex = 12;
			// 
			// LabelSizeGroupBox
			// 
			this.LabelSizeGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("2c57dee4-b427-45e5-bf2a-50aa7823fa69", "Label Size");
			this.LabelSizeGroupBox.Controls.Add(this.FiveInchRadioButton);
			this.LabelSizeGroupBox.Controls.Add(this.SixInchRadioButton);
			this.LabelSizeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 48, true);
			this.LabelSizeGroupBox.Name = "LabelSizeGroupBox";
			this.LabelSizeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 71, true);
			this.LabelSizeGroupBox.TabIndex = 19;
			this.LabelSizeGroupBox.TabStop = false;
			// 
			// FiveInchRadioButton
			// 
			this.FiveInchRadioButton.AutoCheck = false;
			this.FiveInchRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.FiveInchRadioButton, "FiveInchLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.AWB.BulkMAWBLabelActions)(null)).FiveInchLabel)));
			this.FiveInchRadioButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("4d52f320-5e57-451f-b80c-a0d67eb2f961", "5\" Labels", "Five Inch", "Specifies that 5\" barcode labels should be printed.");
			this.FiveInchRadioButton.Checked = true;
			this.FiveInchRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.FiveInchRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 19, true);
			this.FiveInchRadioButton.Name = "FiveInchRadioButton";
			this.FiveInchRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 17, true);
			this.FiveInchRadioButton.TabIndex = 11;
			this.FiveInchRadioButton.TabStop = true;
			// 
			// SixInchRadioButton
			// 
			this.SixInchRadioButton.AutoCheck = false;
			this.SixInchRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SixInchRadioButton, "SixInchLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.AWB.BulkMAWBLabelActions)(null)).SixInchLabel)));
			this.SixInchRadioButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("6c9e2688-34ff-4e5d-812e-efd2764b861b", "Print 6\"", "Six Inch", "Specifies that 6\" barcode labels should be printed.");
			this.SixInchRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SixInchRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 42, true);
			this.SixInchRadioButton.Name = "SixInchRadioButton";
			this.SixInchRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 17, true);
			this.SixInchRadioButton.TabIndex = 12;
			// 
			// PrintOptionalInformationCheckBox
			// 
			this.PrintOptionalInformationCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PrintOptionalInformationCheckBox, "PrintOptionalInformation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.AWB.BulkMAWBLabelActions)(null)).PrintOptionalInformation)));
			this.PrintOptionalInformationCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBPrintForm|1f9c544f-3a95-49b4-bc8c-9158507241e8", "Print Optional Information", "Print the Optional Information section on the IATA 606 barcode label.\r\nThis section can be configured in the system registry.");
			this.PrintOptionalInformationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PrintOptionalInformationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 24, true);
			this.PrintOptionalInformationCheckBox.Name = "PrintOptionalInformationCheckBox";
			this.PrintOptionalInformationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 17, true);
			this.PrintOptionalInformationCheckBox.TabIndex = 10;
			this.PrintOptionalInformationCheckBox.UseVisualStyleBackColor = true;
			// 
			// CopiesNumericUpDown
			// 
			this.BindingSource.SetBindingMember(this.CopiesNumericUpDown, "NumberOfCopies");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((decimal)(((Enterprise.Freight.Forwarding.Business.AWB.BulkMAWBLabelActions)(null)).NumberOfCopies)));
			this.CopiesNumericUpDown.BindTo = "NumberOfCopies";
			this.CopiesNumericUpDown.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("61c2ec0a-4ef0-4138-887d-b66ad370783a", "Copies", "The number of copies to be produced from each AWB/consol.");
			this.CopiesNumericUpDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(199, 151, true);
			this.CopiesNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.CopiesNumericUpDown.Name = "CopiesNumericUpDown";
			this.CopiesNumericUpDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.CopiesNumericUpDown.TabIndex = 31;
			this.CopiesNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// LabelPrinterGuidDropEdit
			// 
			this.LabelPrinterGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LabelPrinterGuidDropEdit, "AWBLabelPrinter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.BulkMAWBLabelActions)(null)).AWBLabelPrinter)));
			this.LabelPrinterGuidDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBPrintForm|e91af524-62b3-4cf5-95f8-6271ad439f80", "Printer", "Label Printer", "The printer to use for printing AWB barcode labels.");
			this.LabelPrinterGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(54, 125, true);
			this.LabelPrinterGuidDropEdit.Name = "LabelPrinterGuidDropEdit";
			this.LabelPrinterGuidDropEdit.PreBoundMaxLength = 52;
			this.LabelPrinterGuidDropEdit.ShowDescriptionBox = false;
			this.LabelPrinterGuidDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.LabelPrinterGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 20, true);
			this.LabelPrinterGuidDropEdit.TabIndex = 18;
			// 
			// PrintMAWBBarcodeLabelsActionMethodForm
			// 
			this.CancelButton = this.Cancel_Button;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("0bc1201b-460f-42fa-b046-29f84bf13cd8", "Print MAWB Barcode Labels");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 256, true);
			this.ControlBox = false;
			this.Controls.Add(this.BarcodeLabelsGroupBox);
			this.Controls.Add(this.Cancel_Button);
			this.Controls.Add(this.OKButton);
			this.DataSourceAssemblyName = "Enterprise.Freight.Forwarding.Business";
			this.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.AWB.BulkMAWBLabelActions);
			this.DataSourceTypeName = "Enterprise.Freight.Forwarding.Business.AWB.BulkMAWBLabelActions";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.Name = "PrintMAWBBarcodeLabelsActionMethodForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.Cancel_Button, 0);
			this.Controls.SetChildIndex(this.BarcodeLabelsGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BarcodeLabelsGroupBox.ResumeLayout(false);
			this.BarcodeLabelsGroupBox.PerformLayout();
			this.PackagesFromGroupBox.ResumeLayout(false);
			this.PackagesFromGroupBox.PerformLayout();
			this.LabelSizeGroupBox.ResumeLayout(false);
			this.LabelSizeGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CopiesNumericUpDown)).EndInit();
			this.CopiesNumericUpDown.ResumeLayout(false);
			this.CopiesNumericUpDown.PerformLayout();
			this.LabelPrinterGuidDropEdit.ResumeLayout(true);
			this.LabelPrinterGuidDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private ZGroupBox LabelSizeGroupBox;
		private ZGroupBox PackagesFromGroupBox;
		private ZRadioButton AWBPackagesRadioButton;
		private ZRadioButton ConsolPackagesRadioButton;
		private ZNumericUpDown CopiesNumericUpDown;
		private ZCheckBox LabelUseEPrintCheckBox;
	}
}