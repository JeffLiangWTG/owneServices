using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	public partial class DocumentShipmentWithUniqueIDForm
	{

		#region Windows Form Designer generated code

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		ZRadioButton consignorRadioButton;
		ZRadioButton consigneeRadioButton;
		ZRadioButton noneRadioButton;

		ZCalcEdit fromLabelCalcEdit;
		ZCalcEdit toLabelCalcEdit;
		ZLabel totalNumberOfLabelsLabel;
		ZPanel labelsToPrintPanel;

		System.ComponentModel.Container components = null;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.consignorRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.consigneeRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.noneRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.fromLabelCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.toLabelCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.totalNumberOfLabelsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.labelsToPrintPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OptionsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.labelsToPrintPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// OptionsGroupBox
			// 
			this.OptionsGroupBox.Controls.Add(this.labelsToPrintPanel);
			this.OptionsGroupBox.Controls.Add(this.noneRadioButton);
			this.OptionsGroupBox.Controls.Add(this.consigneeRadioButton);
			this.OptionsGroupBox.Controls.Add(this.consignorRadioButton);
			this.OptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.OptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(282, 125, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 214, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 24, true);
			// 
			// ConsignorRadioButton
			// 
			this.consignorRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.consignorRadioButton, "IncludeConsignor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.DocumentShipment)(null)).IncludeConsignor)));
			this.consignorRadioButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentShipmentWithUniqueIDForm|cd1527fc-229f-4447-94a0-4988357fb86d", "Consignor");
			this.consignorRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.consignorRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.consignorRadioButton.Name = "ConsignorRadioButton";
			this.consignorRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 24, true);
			this.consignorRadioButton.TabIndex = 7;
			this.consignorRadioButton.Visible = false;
			// 
			// ConsigneeRadioButton
			// 
			this.consigneeRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.consigneeRadioButton, "IncludeConsignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.DocumentShipment)(null)).IncludeConsignee)));
			this.consigneeRadioButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentShipmentWithUniqueIDForm|ce01f19c-ca90-495a-9801-a608567a2888", "Consignee");
			this.consigneeRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.consigneeRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 37, true);
			this.consigneeRadioButton.Name = "ConsigneeRadioButton";
			this.consigneeRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 24, true);
			this.consigneeRadioButton.TabIndex = 8;
			this.consigneeRadioButton.Visible = false;
			// 
			// NoneRadioButton
			// 
			this.noneRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.noneRadioButton, "IncludeNone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.DocumentShipment)(null)).IncludeNone)));
			this.noneRadioButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentShipmentWithUniqueIDForm|c00113b1-1077-4833-8443-5298f4b293a5", "None");
			this.noneRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.noneRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 59, true);
			this.noneRadioButton.Name = "NoneRadioButton";
			this.noneRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 25, true);
			this.noneRadioButton.TabIndex = 9;
			this.noneRadioButton.Visible = false;
			// 
			// FromLabelCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.fromLabelCalcEdit, "LabelRangeFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.DocumentShipment)(null)).LabelRangeFrom)));
			this.fromLabelCalcEdit.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentShipmentWithUniqueIDForm|45e9b4ce-fb09-4f9f-9741-418c432d20c0", "Range");
			this.fromLabelCalcEdit.DecimalPlaces = 0;
			this.fromLabelCalcEdit.Decimals = 0;
			this.fromLabelCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(46, 3, true);
			this.fromLabelCalcEdit.Name = "FromLabelCalcEdit";
			this.fromLabelCalcEdit.ShowGroupSeparators = false;
			this.fromLabelCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.fromLabelCalcEdit.TabIndex = 13;
			this.fromLabelCalcEdit.Text = "0";
			this.fromLabelCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ToLabelCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.toLabelCalcEdit, "LabelRangeTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.DocumentShipment)(null)).LabelRangeTo)));
			this.toLabelCalcEdit.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentShipmentWithUniqueIDForm|783da10f-4d3a-4c71-8e37-4dbf9e804a64", "to");
			this.toLabelCalcEdit.DecimalPlaces = 0;
			this.toLabelCalcEdit.Decimals = 0;
			this.toLabelCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 3, true);
			this.toLabelCalcEdit.Name = "ToLabelCalcEdit";
			this.toLabelCalcEdit.ShowGroupSeparators = false;
			this.toLabelCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.toLabelCalcEdit.TabIndex = 13;
			this.toLabelCalcEdit.Text = "0";
			this.toLabelCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalNumberOfLabelsLabel
			// 
			this.totalNumberOfLabelsLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.totalNumberOfLabelsLabel, "TotalNumberOfLabelsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.DocumentShipment)(null)).TotalNumberOfLabelsDescription)));
			this.totalNumberOfLabelsLabel.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentShipmentForm|380d5b78-a6b3-43fb-a441-f641f881901d", "Of 0");
			this.totalNumberOfLabelsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.totalNumberOfLabelsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(181, 6, true);
			this.totalNumberOfLabelsLabel.Name = "TotalNumberOfLabelsLabel";
			this.totalNumberOfLabelsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(27, 13, true);
			this.totalNumberOfLabelsLabel.TabIndex = 14;
			// 
			// LabelsToPrintPanel
			// 
			this.labelsToPrintPanel.Controls.Add(this.fromLabelCalcEdit);
			this.labelsToPrintPanel.Controls.Add(this.toLabelCalcEdit);
			this.labelsToPrintPanel.Controls.Add(this.totalNumberOfLabelsLabel);
			this.labelsToPrintPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 94, true);
			this.labelsToPrintPanel.Name = "LabelsToPrintPanel";
			this.labelsToPrintPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(274, 24, true);
			this.labelsToPrintPanel.TabIndex = 15;
			// 
			// DocumentShipmentWithUniqueIDForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DocumentShipmentWithUniqueIDForm|7d76002b-68f1-444d-b5cb-8e937525df2e", "Label Range for Printing");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 238, true);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 240, true);
			this.Name = "DocumentShipmentWithUniqueIDForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OptionsGroupBox, 0);
			this.OptionsGroupBox.ResumeLayout(false);
			this.OptionsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.labelsToPrintPanel.ResumeLayout(false);
			this.labelsToPrintPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}
