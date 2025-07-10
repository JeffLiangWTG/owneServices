namespace Enterprise.Rating.GUI
{
	public partial class ProfitShareRebateControl
	{
		private ZArchitecture.ZCalcEdit PercentCalcEdit;
		private ZArchitecture.ZCalcEdit MinimumCalcEdit;
		private ZArchitecture.ZCalcEdit BasePriceCalcEdit;
		private ZArchitecture.ZCalcEdit MaximumCalcEdit;
		private ZArchitecture.GUI.ZCheckBox ZeroWhenLoss;
		private ZArchitecture.ZGrid ApplyToRateLineItemsGrid;
		private System.ComponentModel.Container components = null;

		private void InitializeComponent()
		{
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo2 = new ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			this.PercentCalcEdit = new ZArchitecture.ZCalcEdit();
			this.MinimumCalcEdit = new ZArchitecture.ZCalcEdit();
			this.BasePriceCalcEdit = new ZArchitecture.ZCalcEdit();
			this.MaximumCalcEdit = new ZArchitecture.ZCalcEdit();
			this.ApplyToRateLineItemsGrid = new ZArchitecture.ZGrid();
			this.ZeroWhenLoss = new ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ApplyToRateLineItemsGrid)).BeginInit();
			this.ApplyToRateLineItemsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.RateLineItemsView);
			// 
			// PercentCalcEdit
			// 
			this.PercentCalcEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("8b62f0d8-4083-4e5d-9199-0a2d0ddf1d36", "Percentage (%):");
			this.PercentCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 2, true);
			this.PercentCalcEdit.Name = "PercentCalcEdit";
			this.PercentCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 18, true);
			this.PercentCalcEdit.TabIndex = 0;
			this.PercentCalcEdit.Text = "0.0000";
			this.PercentCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// BasePriceCalcEdit
			// 
			this.BasePriceCalcEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("75c9fba2-b750-4356-85f6-a1f1a57df5a9", "Base Price");
			this.BasePriceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 25, true);
			this.BasePriceCalcEdit.Name = "BasePriceCalcEdit";
			this.BasePriceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 18, true);
			this.BasePriceCalcEdit.TabIndex = 2;
			this.BasePriceCalcEdit.Text = "0.000";
			this.BasePriceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MinimumCalcEdit
			// 
			this.MinimumCalcEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("47df7e39-ae61-42f5-ab3b-b9fd685aefc0", "Minimum");
			this.MinimumCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 46, true);
			this.MinimumCalcEdit.Name = "MinimumCalcEdit";
			this.MinimumCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 18, true);
			this.MinimumCalcEdit.TabIndex = 3;
			this.MinimumCalcEdit.Text = "0.000";
			this.MinimumCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MaximumCalcEdit
			// 
			this.MaximumCalcEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("aff58594-f3ac-4bd2-844d-bb3656d070cc", "Maximum");
			this.MaximumCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 69, true);
			this.MaximumCalcEdit.Name = "MaximumCalcEdit";
			this.MaximumCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 18, true);
			this.MaximumCalcEdit.TabIndex = 4;
			this.MaximumCalcEdit.Text = "0.000";
			this.MaximumCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ApplyToRateLineItemsGrid
			// 
			this.ApplyToRateLineItemsGrid.AllowNavigation = false;
			this.ApplyToRateLineItemsGrid.AllowSorting = false;
			this.ApplyToRateLineItemsGrid.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.ApplyToRateLineItemsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.RateLineItem)(null)).TM_Text);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.RateLineItem)(null)).Lookups.ProfitShareRebateApplyToList);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.RateLineItem)(null)).ProfitShareRebateApplyToDescription);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.RateLineItem)(null)).CalculationOrderOrPercentOf);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.RateLineItem)(null)).CalculationOrderOrPercentOfFieldType);
			this.ApplyToRateLineItemsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo2.BindToList = "Lookups.ProfitShareRebateApplyToList";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("343d5f40-298a-4b5c-b78f-967180b95eda", "Type");
			zDropEditColumnStyleInfo2.ColumnName = "TM_Text";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("f3e82947-928b-4cfc-a163-584cf56012c8", "Apply To");
			zTextBoxColumnStyleInfo2.ColumnName = "ProfitShareRebateApplyToDescription";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zMultiControlColumnStyleInfo2.BindToDecimalPlaces = null;
			zMultiControlColumnStyleInfo2.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("7771511b-60f5-4fa4-b986-a9ad487068d6", "Charge", "Apply To Charge Code", "Charge Code this Profit Share Calculator will apply to.");
			zMultiControlColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiControlColumnStyleInfo2.ColumnName = "CalculationOrderOrPercentOf";
			zMultiControlColumnStyleInfo2.FieldTypeColumnName = "CalculationOrderOrPercentOfFieldType";
			zMultiControlColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.ApplyToRateLineItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ApplyToRateLineItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ApplyToRateLineItemsGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo2);
			this.ApplyToRateLineItemsGrid.GridId = "5623e3c1-7ba6-4b90-a2b8-86d56cd84003";
			this.ApplyToRateLineItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ApplyToRateLineItemsGrid.LayoutKey = "RateLineItemsGridProfitShareRebate";
			this.ApplyToRateLineItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 2, true);
			this.ApplyToRateLineItemsGrid.Name = "ApplyToRateLineItemsGrid";
			this.ApplyToRateLineItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(281, 131, true);
			this.ApplyToRateLineItemsGrid.TabIndex = 1;
			// 
			// ZeroWhenLoss
			// 
			this.ZeroWhenLoss.AutoSize = true;
			this.ZeroWhenLoss.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("26d55007-a383-4519-ae72-3dfe050cff92", "Zero When Loss");
			this.ZeroWhenLoss.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ZeroWhenLoss.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ZeroWhenLoss.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 91, true);
			this.ZeroWhenLoss.Name = "ZeroWhenLoss";
			this.ZeroWhenLoss.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 14, true);
			this.ZeroWhenLoss.TabIndex = 5;
			// 
			// ProfitShareRebateControl
			// 
			this.Controls.Add(this.ZeroWhenLoss);
			this.Controls.Add(this.ApplyToRateLineItemsGrid);
			this.Controls.Add(this.MaximumCalcEdit);
			this.Controls.Add(this.BasePriceCalcEdit);
			this.Controls.Add(this.MinimumCalcEdit);
			this.Controls.Add(this.PercentCalcEdit);
			this.Name = "ProfitShareRebateControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ApplyToRateLineItemsGrid)).EndInit();
			this.ApplyToRateLineItemsGrid.ResumeLayout(false);
			this.ApplyToRateLineItemsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
