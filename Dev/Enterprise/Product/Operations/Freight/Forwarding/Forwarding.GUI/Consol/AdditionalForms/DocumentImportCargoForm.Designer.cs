using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class DocumentImportCargoForm : ZChildForm
	{
		ZButton CancelPrintButton;
		ZButton PrintButton;
		ZGroupBox IncludeGroupBox;
		ZCheckBox IncludeConsigneeCheckBox;
		ZCheckBox IncludeConsignorCheckBox;
		ZCheckBox IncludeHouseBillCheckBox;
		ZCheckBox InlcudeCFSNameCheckBox;
		ZArchitecture.ZCalcEdit LabelsToPrintTextBox;

		protected new void InitializeComponent()
		{
			this.CancelPrintButton = new ZButton();
			this.PrintButton = new ZButton();
			this.IncludeGroupBox = new ZGroupBox();
			this.InlcudeCFSNameCheckBox = new ZCheckBox();
			this.IncludeHouseBillCheckBox = new ZCheckBox();
			this.IncludeConsignorCheckBox = new ZCheckBox();
			this.IncludeConsigneeCheckBox = new ZCheckBox();
			this.LabelsToPrintTextBox = new ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.IncludeGroupBox.SuspendLayout();
			this.SuspendLayout();
			//
			// MainStatusBar
			//
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 215, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 25, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 5;
			//
			// MessageStatusBarPanel
			//
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(119);
			//
			// ErrorStatusBarPanel
			//
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(119);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(DocumentImportCargoLabel);
			//
			// CancelPrintButton
			//
			this.CancelPrintButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentImportCargoForm|2bb041a1-3734-43cc-9984-c5979ef8d29d", "Cancel");
			this.CancelPrintButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelPrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 149, true);
			this.CancelPrintButton.Name = "CancelPrintButton";
			this.CancelPrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CancelPrintButton.TabIndex = 4;
			this.CancelPrintButton.Click += new System.EventHandler(this.CancelPrintButton_Click);
			//
			// PrintButton
			//
			this.PrintButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentImportCargoForm|5c9f7f71-fde4-4ea1-9378-588fea072cdf", "Print");
			this.PrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 149, true);
			this.PrintButton.Name = "PrintButton";
			this.PrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.PrintButton.TabIndex = 3;
			this.PrintButton.Click += new System.EventHandler(this.PrintButton_Click);
			//
			// IncludeGroupBox
			//
			this.IncludeGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentImportCargoForm|97e4e8b2-7b33-4088-ad2a-4a7f399e5a7b", "Include");
			this.IncludeGroupBox.Controls.Add(this.InlcudeCFSNameCheckBox);
			this.IncludeGroupBox.Controls.Add(this.IncludeHouseBillCheckBox);
			this.IncludeGroupBox.Controls.Add(this.IncludeConsignorCheckBox);
			this.IncludeGroupBox.Controls.Add(this.IncludeConsigneeCheckBox);
			this.IncludeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.IncludeGroupBox.Name = "IncludeGroupBox";
			this.IncludeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 112, true);
			this.IncludeGroupBox.TabIndex = 0;
			this.IncludeGroupBox.TabStop = false;
			//
			// InlcudeCFSNameCheckBox
			//
			this.BindingSource.SetBindingMember(this.InlcudeCFSNameCheckBox, "IncludeCFSName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((DocumentImportCargoLabel)(null)).IncludeCFSName)));
			this.InlcudeCFSNameCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentImportCargoForm|17cd6ee7-7884-42d5-b120-ece1cac79972", "CFS from Consol (name only)");
			this.InlcudeCFSNameCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.InlcudeCFSNameCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 82, true);
			this.InlcudeCFSNameCheckBox.Name = "InlcudeCFSNameCheckBox";
			this.InlcudeCFSNameCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 22, true);
			this.InlcudeCFSNameCheckBox.TabIndex = 3;
			//
			// IncludeHouseBillCheckBox
			//
			this.BindingSource.SetBindingMember(this.IncludeHouseBillCheckBox, "IncludeHouseBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((DocumentImportCargoLabel)(null)).IncludeHouseBill)));
			this.IncludeHouseBillCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentImportCargoForm|4e9be134-45a8-431b-9740-2267b7e7d7e6", "House Bill Number (including Barcode)");
			this.IncludeHouseBillCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IncludeHouseBillCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 59, true);
			this.IncludeHouseBillCheckBox.Name = "IncludeHouseBillCheckBox";
			this.IncludeHouseBillCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 23, true);
			this.IncludeHouseBillCheckBox.TabIndex = 2;
			//
			// IncludeConsignorCheckBox
			//
			this.BindingSource.SetBindingMember(this.IncludeConsignorCheckBox, "IncludeConsignor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((DocumentImportCargoLabel)(null)).IncludeConsignor)));
			this.IncludeConsignorCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentImportCargoForm|2a1a5e5a-e3f0-4c31-89d3-f38043790e18", "Consignor");
			this.IncludeConsignorCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IncludeConsignorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 37, true);
			this.IncludeConsignorCheckBox.Name = "IncludeConsignorCheckBox";
			this.IncludeConsignorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 22, true);
			this.IncludeConsignorCheckBox.TabIndex = 1;
			//
			// IncludeConsigneeCheckBox
			//
			this.BindingSource.SetBindingMember(this.IncludeConsigneeCheckBox, "IncludeConsignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((DocumentImportCargoLabel)(null)).IncludeConsignee)));
			this.IncludeConsigneeCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentImportCargoForm|9c6bf9c8-f2b6-434c-9aeb-d0fa2cb9d5fe", "Consignee");
			this.IncludeConsigneeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IncludeConsigneeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.IncludeConsigneeCheckBox.Name = "IncludeConsigneeCheckBox";
			this.IncludeConsigneeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 22, true);
			this.IncludeConsigneeCheckBox.TabIndex = 0;
			//
			// LabelsToPrintTextBox
			//
			this.BindingSource.SetBindingMember(this.LabelsToPrintTextBox, "NoOfLabelsToPrint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((DocumentImportCargoLabel)(null)).NoOfLabelsToPrint)));
			this.LabelsToPrintTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentImportCargoForm|8384bf7f-1186-418d-b81b-e9f1135d9bbd", "Number of labels to print");
			this.LabelsToPrintTextBox.Decimals = 0;
			this.LabelsToPrintTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 123, true);
			this.LabelsToPrintTextBox.Name = "LabelsToPrintTextBox";
			this.LabelsToPrintTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.LabelsToPrintTextBox.TabIndex = 2;
			this.LabelsToPrintTextBox.Text = "0";
			this.LabelsToPrintTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// DocumentImportCargoForm
			//

			this.CancelButton = this.CancelPrintButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 240, true);
			this.ControlBox = false;
			this.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentImportCargoForm|ec231fc1-dc2a-417e-8e36-e038443b0dbc", "Import Cargo Label");
			this.Controls.Add(this.LabelsToPrintTextBox);
			this.Controls.Add(this.IncludeGroupBox);
			this.Controls.Add(this.CancelPrintButton);
			this.Controls.Add(this.PrintButton);
			this.DataSourceAssemblyName = "Enterprise.Freight";
			this.DataSourceType = typeof(DocumentImportCargoLabel);
			this.DataSourceTypeName = "DocumentImportCargoLabel";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.Name = "DocumentImportCargoForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PrintButton, 0);
			this.Controls.SetChildIndex(this.CancelPrintButton, 0);
			this.Controls.SetChildIndex(this.IncludeGroupBox, 0);
			this.Controls.SetChildIndex(this.LabelsToPrintTextBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.IncludeGroupBox.ResumeLayout(false);
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
