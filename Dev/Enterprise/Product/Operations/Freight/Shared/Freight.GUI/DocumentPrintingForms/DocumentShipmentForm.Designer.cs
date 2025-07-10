using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	public partial class DocumentShipmentForm
	{

		#region Component Designer generated code

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		Enterprise.ZArchitecture.GUI.ZRadioButton ConsignorRadioButton;
		Enterprise.ZArchitecture.GUI.ZRadioButton ConsigneeRadioButton;
		Enterprise.ZArchitecture.GUI.ZRadioButton NoneRadioButton;
		Enterprise.ZArchitecture.GUI.ZRadioButton SendingAgentRadioButton;
		ZCalcEdit NumberOfLabelsToPrintEdit;
		ZLabel TotalNumberOfLabelsLabel;
		ZPanel NumberOfLabelsToPrintPanel;
		System.ComponentModel.Container components = null;

		new void InitializeComponent()
		{
			this.ConsignorRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.ConsigneeRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.NoneRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.SendingAgentRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.NumberOfLabelsToPrintEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalNumberOfLabelsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.NumberOfLabelsToPrintPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OptionsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.NumberOfLabelsToPrintPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// OptionsGroupBox
			// 
			this.OptionsGroupBox.Controls.Add(this.NumberOfLabelsToPrintPanel);
			this.OptionsGroupBox.Controls.Add(this.SendingAgentRadioButton);
			this.OptionsGroupBox.Controls.Add(this.NoneRadioButton);
			this.OptionsGroupBox.Controls.Add(this.ConsigneeRadioButton);
			this.OptionsGroupBox.Controls.Add(this.ConsignorRadioButton);
			this.OptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.OptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(282, 275, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 344, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 24, true);
			// 
			// ConsignorRadioButton
			// 
			this.ConsignorRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.ConsignorRadioButton, "IncludeConsignor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.DocumentShipment)(null)).IncludeConsignor)));
			this.ConsignorRadioButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentShipmentForm|cd1527fc-229f-4447-94a0-4988357fb86d", "Consignor");
			this.ConsignorRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ConsignorRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.ConsignorRadioButton.Name = "ConsignorRadioButton";
			this.ConsignorRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 24, true);
			this.ConsignorRadioButton.TabIndex = 7;
			this.ConsignorRadioButton.Visible = false;
			// 
			// ConsigneeRadioButton
			// 
			this.ConsigneeRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.ConsigneeRadioButton, "IncludeConsignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.DocumentShipment)(null)).IncludeConsignee)));
			this.ConsigneeRadioButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentShipmentForm|ce01f19c-ca90-495a-9801-a608567a2888", "Consignee");
			this.ConsigneeRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ConsigneeRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 37, true);
			this.ConsigneeRadioButton.Name = "ConsigneeRadioButton";
			this.ConsigneeRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 24, true);
			this.ConsigneeRadioButton.TabIndex = 8;
			this.ConsigneeRadioButton.Visible = false;
			// 
			// NoneRadioButton
			// 
			this.NoneRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.NoneRadioButton, "IncludeNone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.DocumentShipment)(null)).IncludeNone)));
			this.NoneRadioButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentShipmentForm|c00113b1-1077-4833-8443-5298f4b293a5", "None");
			this.NoneRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NoneRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 59, true);
			this.NoneRadioButton.Name = "NoneRadioButton";
			this.NoneRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 25, true);
			this.NoneRadioButton.TabIndex = 9;
			this.NoneRadioButton.Visible = false;
			// 
			// SendingAgentRadioButton
			// 
			this.SendingAgentRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.SendingAgentRadioButton, "IncludeSendingAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.DocumentShipment)(null)).IncludeSendingAgent)));
			this.SendingAgentRadioButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentShipmentForm|df62e9b6-78b9-441c-a86a-adadfb231a25", "Sending Agent");
			this.SendingAgentRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SendingAgentRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 82, true);
			this.SendingAgentRadioButton.Name = "SendingAgentRadioButton";
			this.SendingAgentRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 24, true);
			this.SendingAgentRadioButton.TabIndex = 10;
			this.SendingAgentRadioButton.Visible = false;
			// 
			// NumberOfLabelsToPrintEdit
			// 
			this.BindingSource.SetBindingMember(this.NumberOfLabelsToPrintEdit, "NumberOfLabelsToPrint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.DocumentShipment)(null)).NumberOfLabelsToPrint)));
			this.NumberOfLabelsToPrintEdit.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentShipmentForm|484ff92a-091f-4582-9999-d1f87ce8fd69", "Number of Labels to Print");
			this.NumberOfLabelsToPrintEdit.Decimals = 0;
			this.NumberOfLabelsToPrintEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 3, true);
			this.NumberOfLabelsToPrintEdit.Name = "NumberOfLabelsToPrintEdit";
			this.NumberOfLabelsToPrintEdit.ShowGroupSeparators = false;
			this.NumberOfLabelsToPrintEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 18, true);
			this.NumberOfLabelsToPrintEdit.TabIndex = 13;
			this.NumberOfLabelsToPrintEdit.Text = "0";
			this.NumberOfLabelsToPrintEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalNumberOfLabelsLabel
			// 
			this.TotalNumberOfLabelsLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.TotalNumberOfLabelsLabel, "TotalNumberOfLabelsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.DocumentShipment)(null)).TotalNumberOfLabelsDescription)));
			this.TotalNumberOfLabelsLabel.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentShipmentForm|380d5b78-a6b3-43fb-a441-f641f881901d", "Of 0");
			this.TotalNumberOfLabelsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 6, true);
			this.TotalNumberOfLabelsLabel.Name = "TotalNumberOfLabelsLabel";
			this.TotalNumberOfLabelsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 14, true);
			this.TotalNumberOfLabelsLabel.TabIndex = 14;
			// 
			// NumberOfLabelsToPrintPanel
			// 
			this.NumberOfLabelsToPrintPanel.Controls.Add(this.TotalNumberOfLabelsLabel);
			this.NumberOfLabelsToPrintPanel.Controls.Add(this.NumberOfLabelsToPrintEdit);
			this.NumberOfLabelsToPrintPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 110, true);
			this.NumberOfLabelsToPrintPanel.Name = "NumberOfLabelsToPrintPanel";
			this.NumberOfLabelsToPrintPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(274, 24, true);
			this.NumberOfLabelsToPrintPanel.TabIndex = 15;
			// 
			// DocumentShipmentForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 368, true);
			this.Name = "DocumentShipmentForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OptionsGroupBox, 0);
			this.OptionsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.NumberOfLabelsToPrintPanel.ResumeLayout(false);
			this.NumberOfLabelsToPrintPanel.PerformLayout();
			this.ResumeLayout(false);
		}

		#endregion

	}
}
