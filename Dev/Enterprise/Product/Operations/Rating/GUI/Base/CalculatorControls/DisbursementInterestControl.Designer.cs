using Enterprise.ZArchitecture;

namespace Enterprise.Rating.GUI
{
	public partial class DisbursementInterestControl
	{
		private ZLabel zLabel3;
		private ZCalcEdit CurrentPrimeRateCalcEdit;
		private ZCalcEdit UpliftCalcEdit;
		private ZLabel CreditTermsLabel;
		private ZCalcEdit EffectiveRateCalcEdit;
		private ZCalcEdit AdjustmentDaysCalcEdit;
		private ZArchitecture.GUI.ZCheckBox OutstandingDaysCheckBox;
		internal ZGrid ApplyToRateLineItemsGrid;
		private ZArchitecture.GUI.ZCheckBox IncludeTaxCheckbox;
		private System.ComponentModel.Container components = null;

		private void InitializeComponent()
		{
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			this.zLabel3 = new ZLabel();
			this.CurrentPrimeRateCalcEdit = new ZCalcEdit();
			this.UpliftCalcEdit = new ZCalcEdit();
			this.CreditTermsLabel = new ZLabel();
			this.EffectiveRateCalcEdit = new ZCalcEdit();
			this.AdjustmentDaysCalcEdit = new ZCalcEdit();
			this.OutstandingDaysCheckBox = new ZArchitecture.GUI.ZCheckBox();
			this.ApplyToRateLineItemsGrid = new ZGrid();
			this.IncludeTaxCheckbox = new ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ApplyToRateLineItemsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = DataSourceTypeForBinding;
			// 
			// zLabel3
			// 
			this.zLabel3.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			this.zLabel3.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("DisbursementInterestControl|9360e929-77fd-4166-878e-98df3494533a", "Credit Terms:");
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 90, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 23, true);
			this.zLabel3.TabIndex = 8;
			this.zLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// CurrentPrimeRateCalcEdit
			// 
			this.CurrentPrimeRateCalcEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("DisbursementInterestControl|984ae02d-979c-46f1-aff2-94d2b7e3abf9", "Current Prime Rate");
			this.CurrentPrimeRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 6, true);
			this.CurrentPrimeRateCalcEdit.Name = "CurrentPrimeRateCalcEdit";
			this.CurrentPrimeRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			this.CurrentPrimeRateCalcEdit.TabIndex = 5;
			this.CurrentPrimeRateCalcEdit.Text = "0.000";
			this.CurrentPrimeRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// UpliftCalcEdit
			// 
			this.UpliftCalcEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("DisbursementInterestControl|1c92cd05-c0c5-4412-b2c7-5822e4add0c0", "Uplift");
			this.UpliftCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 32, true);
			this.UpliftCalcEdit.Name = "UpliftCalcEdit";
			this.UpliftCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			this.UpliftCalcEdit.TabIndex = 9;
			this.UpliftCalcEdit.Text = "0.000";
			this.UpliftCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CreditTermsLabel
			// 
			this.CreditTermsLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			this.CreditTermsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 90, true);
			this.CreditTermsLabel.Name = "CreditTermsLabel";
			this.CreditTermsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 23, true);
			this.CreditTermsLabel.TabIndex = 10;
			// 
			// EffectiveRateCalcEdit
			// 
			this.EffectiveRateCalcEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("DisbursementInterestControl|fa3c97f4-1b7a-4c28-837e-cc2b37a0c8c4", "Effective Rate");
			this.EffectiveRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 58, true);
			this.EffectiveRateCalcEdit.Name = "EffectiveRateCalcEdit";
			this.EffectiveRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			this.EffectiveRateCalcEdit.TabIndex = 12;
			this.EffectiveRateCalcEdit.Text = "0.000";
			this.EffectiveRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AdjustmentDaysCalcEdit
			// 
			this.AdjustmentDaysCalcEdit.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			this.AdjustmentDaysCalcEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("DisbursementInterestControl|ad7f0a7c-6cae-439c-9190-150a909de38a", "Adjustment Days");
			this.AdjustmentDaysCalcEdit.DecimalPlaces = 0;
			this.AdjustmentDaysCalcEdit.Decimals = 0;
			this.AdjustmentDaysCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 111, true);
			this.AdjustmentDaysCalcEdit.Name = "AdjustmentDaysCalcEdit";
			this.AdjustmentDaysCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			this.AdjustmentDaysCalcEdit.TabIndex = 16;
			this.AdjustmentDaysCalcEdit.Text = "0";
			this.AdjustmentDaysCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OutstandingDaysCheckBox
			// 
			this.OutstandingDaysCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			this.OutstandingDaysCheckBox.AutoSize = true;
			this.OutstandingDaysCheckBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("DisbursementInterestControl|d448848f-0768-40b2-a381-1e34b402582b", "Apply to Outstanding Days Only");
			this.OutstandingDaysCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OutstandingDaysCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(201, 114, true);
			this.OutstandingDaysCheckBox.Name = "OutstandingDaysCheckBox";
			this.OutstandingDaysCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.OutstandingDaysCheckBox.TabIndex = 20;
			// 
			// ApplyToRateLineItemsGrid
			// 
			this.ApplyToRateLineItemsGrid.AllowNavigation = false;
			this.ApplyToRateLineItemsGrid.AllowSorting = false;
			this.ApplyToRateLineItemsGrid.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
						| System.Windows.Forms.AnchorStyles.Left
						| System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.ApplyToRateLineItemsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.RateLineItem)(null)).TM_Text);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.RateLineItem)(null)).Lookups.PercentageApplyToList);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.RateLineItem)(null)).ApplyToDescription);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.RateLineItem)(null)).CalculationOrderOrPercentOf);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.RateLineItem)(null)).CalculationOrderOrPercentOfFieldType);
			this.ApplyToRateLineItemsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "Lookups.PercentageApplyToList";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("DisbursementInterestControl|343d5f40-298a-4b5c-b78f-967180b95eda", "Type");
			zDropEditColumnStyleInfo1.ColumnName = "TM_Text";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("DisbursementInterestControl|f3e82947-928b-4cfc-a163-584cf56012c8", "Apply To");
			zTextBoxColumnStyleInfo1.ColumnName = "ApplyToDescription";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zMultiControlColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("DisbursementInterestControl|73711f13-b418-4542-bf83-b366dc0f6767", "Charge");
			zMultiControlColumnStyleInfo1.ColumnName = "CalculationOrderOrPercentOf";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "CalculationOrderOrPercentOfFieldType";
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.ApplyToRateLineItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ApplyToRateLineItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ApplyToRateLineItemsGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.ApplyToRateLineItemsGrid.CopySelectedRowsAllowed = true;
			this.ApplyToRateLineItemsGrid.GridId = "bf464db1-99b2-4727-8824-9580ed799ddc";
			this.ApplyToRateLineItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ApplyToRateLineItemsGrid.LayoutKey = "RateLineItemsGridPercentage";
			this.ApplyToRateLineItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(201, 6, true);
			this.ApplyToRateLineItemsGrid.Name = "ApplyToRateLineItemsGrid";
			this.ApplyToRateLineItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 81, true);
			this.ApplyToRateLineItemsGrid.TabIndex = 12;
			// 
			// IncludeTaxCheckbox
			// 
			this.IncludeTaxCheckbox.AutoSize = true;
			this.IncludeTaxCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IncludeTaxCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 84, true);
			this.IncludeTaxCheckbox.Name = "IncludeTaxCheckbox";
			this.IncludeTaxCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IncludeTaxCheckbox.TabIndex = 18;
			// 
			// DisbursementInterestControl
			// 
			this.Controls.Add(this.IncludeTaxCheckbox);
			this.Controls.Add(this.OutstandingDaysCheckBox);
			this.Controls.Add(this.AdjustmentDaysCalcEdit);
			this.Controls.Add(this.EffectiveRateCalcEdit);
			this.Controls.Add(this.CreditTermsLabel);
			this.Controls.Add(this.UpliftCalcEdit);
			this.Controls.Add(this.CurrentPrimeRateCalcEdit);
			this.Controls.Add(this.zLabel3);
			this.Controls.Add(this.ApplyToRateLineItemsGrid);
			this.Name = "DisbursementInterestControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ApplyToRateLineItemsGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
