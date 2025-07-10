using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class AgencyControl
	{
		private ZCheckBox HideFeeLineTypeOnQuoteCheckBox;
		private ZDropEdit MessageTypeDropDown;
		private ZDropEdit MessageSubTypeDropDown;
		private ZCheckBox HideMessageTypeOnQuoteCheckBox;
		private ZGroupBox FeeTypeGroupBox;
		private ZArchitecture.ZCalcEdit AdditionalCostCalcEdit;
		private ZDropEditWithFixedWidth AgencyFeeTypeDropDown;
		private ZGroupBox LineTypeGroupBox;
		private ZArchitecture.ZCalcEdit AgencyMaxLineCalcEdit;
		private ZArchitecture.ZCalcEdit AgencyCostPerAddlLineCalcEdit;
		private ZArchitecture.ZCalcEdit AgencyFreeLinesCalcEdit;
		private ZDropEditWithFixedWidth AgencyLineTypeDropDownEdit;
		private ZArchitecture.ZCalcEdit IncludedCalcEdit;
		private ZArchitecture.ZCalcEdit AgencyRateCalcEdit;
		private ZArchitecture.ZCalcEdit MaximumCalcEdit;
		private System.ComponentModel.Container components = null;

		private void InitializeComponent()
		{
			this.HideFeeLineTypeOnQuoteCheckBox = new ZCheckBox();
			this.MessageTypeDropDown = new ZDropEditWithFixedWidth();
			this.MessageSubTypeDropDown = new ZDropEditWithFixedWidth();
			this.HideMessageTypeOnQuoteCheckBox = new ZCheckBox();
			this.FeeTypeGroupBox = new ZGroupBox();
			this.IncludedCalcEdit = new ZArchitecture.ZCalcEdit();
			this.AdditionalCostCalcEdit = new ZArchitecture.ZCalcEdit();
			this.AgencyFeeTypeDropDown = new ZDropEditWithFixedWidth();
			this.LineTypeGroupBox = new ZGroupBox();
			this.AgencyMaxLineCalcEdit = new ZArchitecture.ZCalcEdit();
			this.AgencyCostPerAddlLineCalcEdit = new ZArchitecture.ZCalcEdit();
			this.AgencyFreeLinesCalcEdit = new ZArchitecture.ZCalcEdit();
			this.AgencyLineTypeDropDownEdit = new ZDropEditWithFixedWidth();
			this.AgencyRateCalcEdit = new ZArchitecture.ZCalcEdit();
			this.MaximumCalcEdit = new ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FeeTypeGroupBox.SuspendLayout();
			this.LineTypeGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.Calculator);
			// 
			// HideFeeLineTypeOnQuoteCheckBox
			// 
			this.HideFeeLineTypeOnQuoteCheckBox.AutoSize = true;
			this.HideFeeLineTypeOnQuoteCheckBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("AgencyControl|4c721ca4-37c7-44bd-afd0-74c4702ff54f", "Hide Fee and Line Type on Quotation");
			this.HideFeeLineTypeOnQuoteCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.HideFeeLineTypeOnQuoteCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(210, 117, true);
			this.HideFeeLineTypeOnQuoteCheckBox.Name = "HideFeeLineTypeOnQuoteCheckBox";
			this.HideFeeLineTypeOnQuoteCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 17, true);
			this.HideFeeLineTypeOnQuoteCheckBox.TabIndex = 7;
			// 
			// MessageTypeDropDown
			// 
			this.MessageTypeDropDown.AllowDrop = true;
			this.MessageTypeDropDown.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("AgencyControl|d256970e-b38e-414d-9fa3-ab6638590991", "Type");
			this.MessageTypeDropDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(44, 93, true);
			this.MessageTypeDropDown.Name = "MessageTypeDropDown";
			this.MessageTypeDropDown.PreBoundMaxLength = 3;
			this.MessageTypeDropDown.ShowDescriptionBox = false;
			this.MessageTypeDropDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.MessageTypeDropDown.TabIndex = 2;
			// 
			// MessageSubTypeDropDown
			// 
			this.MessageSubTypeDropDown.AllowDrop = true;
			this.MessageSubTypeDropDown.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("AgencyControl|b0857799-a95d-4ea2-a333-350359ffd1dd", "Style");
			this.MessageSubTypeDropDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 93, true);
			this.MessageSubTypeDropDown.Name = "MessageSubTypeDropDown";
			this.MessageSubTypeDropDown.PreBoundMaxLength = 3;
			this.MessageSubTypeDropDown.ShowDescriptionBox = false;
			this.MessageSubTypeDropDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.MessageSubTypeDropDown.TabIndex = 3;
			// 
			// HideMessageTypeOnQuoteCheckBox
			// 
			this.HideMessageTypeOnQuoteCheckBox.AutoSize = true;
			this.HideMessageTypeOnQuoteCheckBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("AgencyControl|c3addf7d-5405-487c-a5a5-6ca43d64b241", "Hide Type and Style on Quotation");
			this.HideMessageTypeOnQuoteCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.HideMessageTypeOnQuoteCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 117, true);
			this.HideMessageTypeOnQuoteCheckBox.Name = "HideMessageTypeOnQuoteCheckBox";
			this.HideMessageTypeOnQuoteCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 17, true);
			this.HideMessageTypeOnQuoteCheckBox.TabIndex = 4;
			// 
			// FeeTypeGroupBox
			// 
			this.FeeTypeGroupBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("AgencyControl|219fc521-0a46-453c-ab9f-c26474d70174", "Fee Type");
			this.FeeTypeGroupBox.Controls.Add(this.IncludedCalcEdit);
			this.FeeTypeGroupBox.Controls.Add(this.AdditionalCostCalcEdit);
			this.FeeTypeGroupBox.Controls.Add(this.AgencyFeeTypeDropDown);
			this.FeeTypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 29, true);
			this.FeeTypeGroupBox.Name = "FeeTypeGroupBox";
			this.FeeTypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 60, true);
			this.FeeTypeGroupBox.TabIndex = 1;
			this.FeeTypeGroupBox.TabStop = false;
			// 
			// IncludedCalcEdit
			// 
			this.IncludedCalcEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("AgencyControl|02198f95-6a96-4c83-9e71-b08488885030", "Incl.");
			this.IncludedCalcEdit.DecimalPlaces = 0;
			this.IncludedCalcEdit.Decimals = 0;
			this.IncludedCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(41, 35, true);
			this.IncludedCalcEdit.MaxValue = new decimal(new int[] {
			0,
			0,
			0,
			0 });
			this.IncludedCalcEdit.Name = "IncludedCalcEdit";
			this.IncludedCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 20, true);
			this.IncludedCalcEdit.TabIndex = 2;
			this.IncludedCalcEdit.Text = "0";
			this.IncludedCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AdditionalCostCalcEdit
			// 
			this.AdditionalCostCalcEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("AgencyControl|0b0628e5-c420-4b83-bddd-625d50f45d4f", "Rate per Add");
			this.AdditionalCostCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 35, true);
			this.AdditionalCostCalcEdit.MaxValue = new decimal(new int[] {
			0,
			0,
			0,
			0 });
			this.AdditionalCostCalcEdit.Name = "AdditionalCostCalcEdit";
			this.AdditionalCostCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 20, true);
			this.AdditionalCostCalcEdit.TabIndex = 3;
			this.AdditionalCostCalcEdit.Text = "120.000";
			this.AdditionalCostCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AgencyFeeTypeDropDown
			// 
			this.AgencyFeeTypeDropDown.AllowDrop = true;
			this.AgencyFeeTypeDropDown.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("AgencyControl|ac47f163-1b17-430c-8e54-b33511897b5d", "Type");
			this.AgencyFeeTypeDropDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 12, true);
			this.AgencyFeeTypeDropDown.Name = "AgencyFeeTypeDropDown";
			this.AgencyFeeTypeDropDown.PreBoundMaxLength = 3;
			this.AgencyFeeTypeDropDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(122, 20, true);
			this.AgencyFeeTypeDropDown.TabIndex = 0;
			// 
			// LineTypeGroupBox
			// 
			this.LineTypeGroupBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("AgencyControl|dc4bb0c1-cfb5-4228-980f-ad947191a1c8", "Line Type");
			this.LineTypeGroupBox.Controls.Add(this.AgencyMaxLineCalcEdit);
			this.LineTypeGroupBox.Controls.Add(this.AgencyCostPerAddlLineCalcEdit);
			this.LineTypeGroupBox.Controls.Add(this.AgencyFreeLinesCalcEdit);
			this.LineTypeGroupBox.Controls.Add(this.AgencyLineTypeDropDownEdit);
			this.LineTypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(211, 29, true);
			this.LineTypeGroupBox.Name = "LineTypeGroupBox";
			this.LineTypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 85, true);
			this.LineTypeGroupBox.TabIndex = 6;
			this.LineTypeGroupBox.TabStop = false;
			// 
			// AgencyMaxLineCalcEdit
			// 
			this.AgencyMaxLineCalcEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("AgencyControl|c2a6b427-5aa8-4594-9f45-fc026c61aebf", "Max. Lines");
			this.AgencyMaxLineCalcEdit.DecimalPlaces = 0;
			this.AgencyMaxLineCalcEdit.Decimals = 0;
			this.AgencyMaxLineCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(187, 38, true);
			this.AgencyMaxLineCalcEdit.MaxValue = new decimal(new int[] {
			0,
			0,
			0,
			0 });
			this.AgencyMaxLineCalcEdit.Name = "AgencyMaxLineCalcEdit";
			this.AgencyMaxLineCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 20, true);
			this.AgencyMaxLineCalcEdit.TabIndex = 2;
			this.AgencyMaxLineCalcEdit.Text = "9999";
			this.AgencyMaxLineCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AgencyCostPerAddlLineCalcEdit
			// 
			this.AgencyCostPerAddlLineCalcEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("AgencyControl|da795439-af3a-4a97-8933-9733051d8b28", "Rate per Add. Line");
			this.AgencyCostPerAddlLineCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 61, true);
			this.AgencyCostPerAddlLineCalcEdit.MaxValue = new decimal(new int[] {
			0,
			0,
			0,
			0 });
			this.AgencyCostPerAddlLineCalcEdit.Name = "AgencyCostPerAddlLineCalcEdit";
			this.AgencyCostPerAddlLineCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 20, true);
			this.AgencyCostPerAddlLineCalcEdit.TabIndex = 3;
			this.AgencyCostPerAddlLineCalcEdit.Text = "0.000";
			this.AgencyCostPerAddlLineCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AgencyFreeLinesCalcEdit
			// 
			this.AgencyFreeLinesCalcEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("AgencyControl|9c932960-9a00-47a1-96a8-beca59e9a4f9", "Included Lines");
			this.AgencyFreeLinesCalcEdit.DecimalPlaces = 0;
			this.AgencyFreeLinesCalcEdit.Decimals = 0;
			this.AgencyFreeLinesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 38, true);
			this.AgencyFreeLinesCalcEdit.MaxValue = new decimal(new int[] {
			0,
			0,
			0,
			0 });
			this.AgencyFreeLinesCalcEdit.Name = "AgencyFreeLinesCalcEdit";
			this.AgencyFreeLinesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.AgencyFreeLinesCalcEdit.TabIndex = 1;
			this.AgencyFreeLinesCalcEdit.Text = "0";
			this.AgencyFreeLinesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AgencyLineTypeDropDownEdit
			// 
			this.AgencyLineTypeDropDownEdit.AllowDrop = true;
			this.AgencyLineTypeDropDownEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("AgencyControl|00ea0b8d-d5c8-4939-807C-32034d3cafx6", "Type");
			this.AgencyLineTypeDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(42, 15, true);
			this.AgencyLineTypeDropDownEdit.Name = "AgencyLineTypeDropDownEdit";
			this.AgencyLineTypeDropDownEdit.PreBoundMaxLength = 3;
			this.AgencyLineTypeDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 20, true);
			this.AgencyLineTypeDropDownEdit.TabIndex = 0;
			// 
			// AgencyRateCalcEdit
			// 
			this.AgencyRateCalcEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("AgencyControl|1c5c62b1-3c40-4db0-968e-71200991016f", "Base Rate");
			this.AgencyRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 7, true);
			this.AgencyRateCalcEdit.MaxValue = new decimal(new int[] {
			0,
			0,
			0,
			0 });
			this.AgencyRateCalcEdit.Name = "AgencyRateCalcEdit";
			this.AgencyRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.AgencyRateCalcEdit.TabIndex = 0;
			this.AgencyRateCalcEdit.Text = "0.000";
			this.AgencyRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MaximumCalcEdit
			// 
			this.MaximumCalcEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("AgencyControl|c3afb971-6035-494c-a576-5a6069010842", "Maximum Amount");
			this.MaximumCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(329, 7, true);
			this.MaximumCalcEdit.MaxValue = new decimal(new int[] {
			0,
			0,
			0,
			0 });
			this.MaximumCalcEdit.Name = "MaximumCalcEdit";
			this.MaximumCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 20, true);
			this.MaximumCalcEdit.TabIndex = 5;
			this.MaximumCalcEdit.Text = "0.000";
			this.MaximumCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AgencyControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MaximumCalcEdit);
			this.Controls.Add(this.AgencyRateCalcEdit);
			this.Controls.Add(this.LineTypeGroupBox);
			this.Controls.Add(this.FeeTypeGroupBox);
			this.Controls.Add(this.HideMessageTypeOnQuoteCheckBox);
			this.Controls.Add(this.MessageTypeDropDown);
			this.Controls.Add(this.MessageSubTypeDropDown);
			this.Controls.Add(this.HideFeeLineTypeOnQuoteCheckBox);
			this.Name = "AgencyControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 144, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FeeTypeGroupBox.ResumeLayout(false);
			this.FeeTypeGroupBox.PerformLayout();
			this.LineTypeGroupBox.ResumeLayout(false);
			this.LineTypeGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
