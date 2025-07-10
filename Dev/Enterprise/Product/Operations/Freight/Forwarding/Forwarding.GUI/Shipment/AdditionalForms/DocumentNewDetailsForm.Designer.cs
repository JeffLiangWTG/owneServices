using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class DocumentNewDetailsForm : ZChildForm
	{
		ZButton CancelPrintButton;
		ZButton PrintButton;
		ZGroupBox ChangeFromGroupBox;
		ZGroupBox ChangeToGroupBox;
		ZArchitecture.ZTextBox OldMarksAndNumbersTextBox;
		ZArchitecture.ZTextBox OldGoodsDescriptionTextBox;
		ZCheckBox NewMarksAndNumbersCheckBox;
		ZCheckBox NewGoodsDescriptionCheckBox;
		ZCheckBox NewWeightCheckBox;
		ZCheckBox NewVolumeCheckBox;
		ZArchitecture.ZTextBox NewMarksAndNumbersTextBox;
		ZArchitecture.ZTextBox NewGoodsDescriptionTextBox;
		ZCalcDropEdit OldVolumeDropDown;
		ZCalcDropEdit OldWeightDropDown;
		ZCalcDropEdit NewVolumeDropDown;
		ZCalcDropEdit NewWeightDropDown;

		protected new void InitializeComponent()
		{
			this.CancelPrintButton = new ZButton();
			this.PrintButton = new ZButton();
			this.ChangeFromGroupBox = new ZGroupBox();
			this.OldVolumeDropDown = new ZCalcDropEdit();
			this.OldWeightDropDown = new ZCalcDropEdit();
			this.OldGoodsDescriptionTextBox = new ZArchitecture.ZTextBox();
			this.OldMarksAndNumbersTextBox = new ZArchitecture.ZTextBox();
			this.ChangeToGroupBox = new ZGroupBox();
			this.NewGoodsDescriptionTextBox = new ZArchitecture.ZTextBox();
			this.NewMarksAndNumbersTextBox = new ZArchitecture.ZTextBox();
			this.NewVolumeDropDown = new ZCalcDropEdit();
			this.NewVolumeCheckBox = new ZCheckBox();
			this.NewWeightDropDown = new ZCalcDropEdit();
			this.NewWeightCheckBox = new ZCheckBox();
			this.NewGoodsDescriptionCheckBox = new ZCheckBox();
			this.NewMarksAndNumbersCheckBox = new ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ChangeFromGroupBox.SuspendLayout();
			this.ChangeToGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 359, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 25, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 3;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(389);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(389);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(DocumentShipment);
			// 
			// CancelPrintButton
			// 
			this.CancelPrintButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentNewDetailsForm|55924697-8b53-40ff-83e7-8e53372c9afc", "Cancel");
			this.CancelPrintButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelPrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(688, 305, true);
			this.CancelPrintButton.Name = "CancelPrintButton";
			this.CancelPrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CancelPrintButton.TabIndex = 2;
			this.CancelPrintButton.Click += new EventHandler(this.CancelPrintButton_Click);
			// 
			// PrintButton
			// 
			this.PrintButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentNewDetailsForm|ef70c807-3ee2-45db-9366-2e350b96154a", "Print");
			this.PrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(608, 305, true);
			this.PrintButton.Name = "PrintButton";
			this.PrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.PrintButton.TabIndex = 1;
			this.PrintButton.Click += new EventHandler(this.PrintButton_Click);
			// 
			// ChangeFromGroupBox
			// 
			this.ChangeFromGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentNewDetailsForm|a5d19d37-21a3-4d66-a679-c264e8a8a68a", "Change From");
			this.ChangeFromGroupBox.Controls.Add(this.OldVolumeDropDown);
			this.ChangeFromGroupBox.Controls.Add(this.OldWeightDropDown);
			this.ChangeFromGroupBox.Controls.Add(this.OldGoodsDescriptionTextBox);
			this.ChangeFromGroupBox.Controls.Add(this.OldMarksAndNumbersTextBox);
			this.ChangeFromGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.ChangeFromGroupBox.Name = "ChangeFromGroupBox";
			this.ChangeFromGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 290, true);
			this.ChangeFromGroupBox.TabIndex = 0;
			this.ChangeFromGroupBox.TabStop = false;
			// 
			// OldVolumeDropDown
			// 
			this.BindingSource.SetBindingMember(this.OldVolumeDropDown, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((DocumentShipment)(null)).OldVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DocumentShipment)(null)).OldVolumeUnit)));
			this.OldVolumeDropDown.BindToAmount = "OldVolume";
			this.OldVolumeDropDown.BindToUnit = "OldVolumeUnit";
			this.OldVolumeDropDown.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentNewDetailsForm|9cf28e44-894d-4320-93cb-bb132812acd8", "Volume");
			this.OldVolumeDropDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 260, true);
			this.OldVolumeDropDown.Name = "OldVolumeDropDown";
			this.OldVolumeDropDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.OldVolumeDropDown.TabIndex = 3;
			this.OldVolumeDropDown.UnitPreBoundMaxLength = 2;
			// 
			// OldWeightDropDown
			// 
			this.BindingSource.SetBindingMember(this.OldWeightDropDown, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((DocumentShipment)(null)).OldWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DocumentShipment)(null)).OldWeightUnit)));
			this.OldWeightDropDown.BindToAmount = "OldWeight";
			this.OldWeightDropDown.BindToUnit = "OldWeightUnit";
			this.OldWeightDropDown.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentNewDetailsForm|ea46a292-ae50-4f54-b5cb-7bacd4eb6c72", "Weight");
			this.OldWeightDropDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 238, true);
			this.OldWeightDropDown.Name = "OldWeightDropDown";
			this.OldWeightDropDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.OldWeightDropDown.TabIndex = 2;
			this.OldWeightDropDown.UnitPreBoundMaxLength = 2;
			// 
			// OldGoodsDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.OldGoodsDescriptionTextBox, "OldGoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DocumentShipment)(null)).OldGoodsDescription)));
			this.OldGoodsDescriptionTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentNewDetailsForm|ba8dd67a-224d-46ba-9acf-453ad83be847", "Description of Goods");
			this.OldGoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 149, true);
			this.OldGoodsDescriptionTextBox.Multiline = true;
			this.OldGoodsDescriptionTextBox.Name = "OldGoodsDescriptionTextBox";
			this.OldGoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 81, true);
			this.OldGoodsDescriptionTextBox.TabIndex = 1;
			// 
			// OldMarksAndNumbersTextBox
			// 
			this.BindingSource.SetBindingMember(this.OldMarksAndNumbersTextBox, "OldMarksAndNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DocumentShipment)(null)).OldMarksAndNumbers)));
			this.OldMarksAndNumbersTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentNewDetailsForm|09e4c7dc-648e-40f5-8583-d9d4421a2392", "Marks And Numbers");
			this.OldMarksAndNumbersTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 37, true);
			this.OldMarksAndNumbersTextBox.Multiline = true;
			this.OldMarksAndNumbersTextBox.Name = "OldMarksAndNumbersTextBox";
			this.OldMarksAndNumbersTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 82, true);
			this.OldMarksAndNumbersTextBox.TabIndex = 0;
			// 
			// ChangeToGroupBox
			// 
			this.ChangeToGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentNewDetailsForm|768f08d2-8a48-4aaf-b8e9-249fc8444a48", "Change To");
			this.ChangeToGroupBox.Controls.Add(this.NewGoodsDescriptionTextBox);
			this.ChangeToGroupBox.Controls.Add(this.NewMarksAndNumbersTextBox);
			this.ChangeToGroupBox.Controls.Add(this.NewVolumeDropDown);
			this.ChangeToGroupBox.Controls.Add(this.NewVolumeCheckBox);
			this.ChangeToGroupBox.Controls.Add(this.NewWeightDropDown);
			this.ChangeToGroupBox.Controls.Add(this.NewWeightCheckBox);
			this.ChangeToGroupBox.Controls.Add(this.NewGoodsDescriptionCheckBox);
			this.ChangeToGroupBox.Controls.Add(this.NewMarksAndNumbersCheckBox);
			this.ChangeToGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(392, 7, true);
			this.ChangeToGroupBox.Name = "ChangeToGroupBox";
			this.ChangeToGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 290, true);
			this.ChangeToGroupBox.TabIndex = 4;
			this.ChangeToGroupBox.TabStop = false;
			// 
			// NewGoodsDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.NewGoodsDescriptionTextBox, "NewGoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DocumentShipment)(null)).NewGoodsDescription)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.NewGoodsDescriptionTextBox, false);
			this.NewGoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 149, true);
			this.NewGoodsDescriptionTextBox.Multiline = true;
			this.NewGoodsDescriptionTextBox.Name = "NewGoodsDescriptionTextBox";
			this.NewGoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 81, true);
			this.NewGoodsDescriptionTextBox.TabIndex = 3;
			// 
			// NewMarksAndNumbersTextBox
			// 
			this.BindingSource.SetBindingMember(this.NewMarksAndNumbersTextBox, "NewMarksAndNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DocumentShipment)(null)).NewMarksAndNumbers)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.NewMarksAndNumbersTextBox, false);
			this.NewMarksAndNumbersTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 37, true);
			this.NewMarksAndNumbersTextBox.Multiline = true;
			this.NewMarksAndNumbersTextBox.Name = "NewMarksAndNumbersTextBox";
			this.NewMarksAndNumbersTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 82, true);
			this.NewMarksAndNumbersTextBox.TabIndex = 1;
			// 
			// NewVolumeDropDown
			// 
			this.BindingSource.SetBindingMember(this.NewVolumeDropDown, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((DocumentShipment)(null)).NewVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DocumentShipment)(null)).NewVolumeUnit)));
			this.NewVolumeDropDown.BindToAmount = "NewVolume";
			this.NewVolumeDropDown.BindToUnit = "NewVolumeUnit";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.NewVolumeDropDown, false);
			this.NewVolumeDropDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 260, true);
			this.NewVolumeDropDown.Name = "NewVolumeDropDown";
			this.NewVolumeDropDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.NewVolumeDropDown.TabIndex = 7;
			this.NewVolumeDropDown.UnitPreBoundMaxLength = 2;
			// 
			// NewVolumeCheckBox
			// 
			this.BindingSource.SetBindingMember(this.NewVolumeCheckBox, "ChangeVolume");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((DocumentShipment)(null)).ChangeVolume)));
			this.NewVolumeCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentNewDetailsForm|bec0ad3b-b918-4711-8980-ce5c23af4928", "Volume");
			this.NewVolumeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NewVolumeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 260, true);
			this.NewVolumeCheckBox.Name = "NewVolumeCheckBox";
			this.NewVolumeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 22, true);
			this.NewVolumeCheckBox.TabIndex = 6;
			// 
			// NewWeightDropDown
			// 
			this.BindingSource.SetBindingMember(this.NewWeightDropDown, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((DocumentShipment)(null)).NewWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DocumentShipment)(null)).NewWeightUnit)));
			this.NewWeightDropDown.BindToAmount = "NewWeight";
			this.NewWeightDropDown.BindToUnit = "NewWeightUnit";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.NewWeightDropDown, false);
			this.NewWeightDropDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 238, true);
			this.NewWeightDropDown.Name = "NewWeightDropDown";
			this.NewWeightDropDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.NewWeightDropDown.TabIndex = 5;
			this.NewWeightDropDown.UnitPreBoundMaxLength = 2;
			// 
			// NewWeightCheckBox
			// 
			this.BindingSource.SetBindingMember(this.NewWeightCheckBox, "ChangeWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((DocumentShipment)(null)).ChangeWeight)));
			this.NewWeightCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentNewDetailsForm|acbda9f5-6b9b-4150-b7b2-840525003c3f", "Weight");
			this.NewWeightCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NewWeightCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 238, true);
			this.NewWeightCheckBox.Name = "NewWeightCheckBox";
			this.NewWeightCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 22, true);
			this.NewWeightCheckBox.TabIndex = 4;
			// 
			// NewGoodsDescriptionCheckBox
			// 
			this.BindingSource.SetBindingMember(this.NewGoodsDescriptionCheckBox, "ChangeGoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((DocumentShipment)(null)).ChangeGoodsDescription)));
			this.NewGoodsDescriptionCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentNewDetailsForm|3619ea41-bffe-429d-8311-5a6981c18a40", "Description of Goods");
			this.NewGoodsDescriptionCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NewGoodsDescriptionCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 126, true);
			this.NewGoodsDescriptionCheckBox.Name = "NewGoodsDescriptionCheckBox";
			this.NewGoodsDescriptionCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 23, true);
			this.NewGoodsDescriptionCheckBox.TabIndex = 2;
			// 
			// NewMarksAndNumbersCheckBox
			// 
			this.BindingSource.SetBindingMember(this.NewMarksAndNumbersCheckBox, "ChangeMarksAndNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((DocumentShipment)(null)).ChangeMarksAndNumbers)));
			this.NewMarksAndNumbersCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentNewDetailsForm|af7da6d9-c0fb-4651-8363-6ab30eb7312a", "Marks And Numbers");
			this.NewMarksAndNumbersCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NewMarksAndNumbersCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.NewMarksAndNumbersCheckBox.Name = "NewMarksAndNumbersCheckBox";
			this.NewMarksAndNumbersCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 22, true);
			this.NewMarksAndNumbersCheckBox.TabIndex = 0;
			// 
			// DocumentNewDetailsForm
			// 

			this.CancelButton = this.CancelPrintButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 384, true);
			this.ControlBox = false;
			this.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentNewDetailsForm|386d7507-a4b5-4c90-890c-3331c274fb7d", "Details");
			this.Controls.Add(this.ChangeToGroupBox);
			this.Controls.Add(this.ChangeFromGroupBox);
			this.Controls.Add(this.CancelPrintButton);
			this.Controls.Add(this.PrintButton);
			this.DataSourceAssemblyName = "Enterprise.Freight";
			this.DataSourceType = typeof(DocumentShipment);
			this.DataSourceTypeName = "Enterprise.Freight.Business.DocumentShipment";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.Name = "DocumentNewDetailsForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PrintButton, 0);
			this.Controls.SetChildIndex(this.CancelPrintButton, 0);
			this.Controls.SetChildIndex(this.ChangeFromGroupBox, 0);
			this.Controls.SetChildIndex(this.ChangeToGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ChangeFromGroupBox.ResumeLayout(false);
			this.ChangeFromGroupBox.PerformLayout();
			this.ChangeToGroupBox.ResumeLayout(false);
			this.ChangeToGroupBox.PerformLayout();
			this.ResumeLayout(false);
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
