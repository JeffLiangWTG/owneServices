using Enterprise.ZArchitecture;

namespace Enterprise.Rating.GUI
{
	public partial class EqualizationCalculatorControl
	{
		internal EqualizationGrid RateLineItemsGrid;
		internal ZArchitecture.GUI.ZCheckBox UseInclusiveBreaksCheckBox;
		internal System.ComponentModel.Container components = null;
		internal ZLabel contractTypeHint;

		private void InitializeComponent()
		{
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZCheckBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			this.RateLineItemsGrid = new EqualizationGrid();
			this.UseInclusiveBreaksCheckBox = new ZArchitecture.GUI.ZCheckBox();
			this.contractTypeHint = new ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RateLineItemsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = DataSourceTypeForBinding;
			// 
			// RateLineItemsGrid
			// 
			this.RateLineItemsGrid.AllowNavigation = false;
			this.RateLineItemsGrid.AllowSorting = false;
			this.RateLineItemsGrid.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left
						| System.Windows.Forms.AnchorStyles.Right;
			this.RateLineItemsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "TM_Type";
			zDropEditColumnStyleInfo1.IsVisible = false;
			zDropEditColumnStyleInfo1.IsUnavailable = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("EqualisationControl|ef49f041-44ee-4b84-8a57-7fcf17b87ec7", "Pivot Break");
			zCalcEditColumnStyleInfo1.ColumnName = "TM_Break";
			zCalcEditColumnStyleInfo1.Decimals = 1;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("EqualisationControl|dad39b95-59df-4c1f-8617-15a2fea2a601", "Rate");
			zCalcEditColumnStyleInfo2.ColumnName = "TM_RelevantValue";
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("EqualisationControl|cf1d3554-a5a1-429c-aeec-3673a1b241a9", "Flat Amount");
			zCalcEditColumnStyleInfo3.ColumnName = "TM_FlatAmount";
			zCheckBoxColumnStyleInfo1.ColumnName = "TM_CallForPricing";
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("EqualisationControl|c8bff0a0-9bc8-4369-9ede-81c652920c21", "Reason");
			zTextBoxColumnStyleInfo1.ColumnName = "TM_Text";
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.RateLineItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.RateLineItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.RateLineItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.RateLineItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.RateLineItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.RateLineItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RateLineItemsGrid.GridId = "b079fe18-e827-42b9-b1b6-54adc415cc5c";
			this.RateLineItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RateLineItemsGrid.LayoutKey = "RateLineItemsGridEqualisation";
			this.RateLineItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RateLineItemsGrid.MaximumRows = 2;
			this.RateLineItemsGrid.Name = "RateLineItemsGrid";
			this.RateLineItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 53, true);
			this.RateLineItemsGrid.TabIndex = 0;
			// 
			// UseInclusiveBreaksCheckBox
			// 
			this.UseInclusiveBreaksCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			this.UseInclusiveBreaksCheckBox.AutoSize = true;
			this.UseInclusiveBreaksCheckBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("EqualisationControl|c7f64818-7e7f-4ca5-9af7-8813a855d178", "Inclusive Breaks");
			this.UseInclusiveBreaksCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UseInclusiveBreaksCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 127, true);
			this.UseInclusiveBreaksCheckBox.Name = "UseInclusiveBreaksCheckBox";
			this.UseInclusiveBreaksCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 17, true);
			this.UseInclusiveBreaksCheckBox.TabIndex = 1;
			// 
			// contractTypeHint
			// 
			this.contractTypeHint.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			this.contractTypeHint.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("7e2b2e97-2a72-446b-8a95-5457b0020bac", "Pivot Measure is the Chargeable Measure:  Regardless if the pivot measure is exceeded all the containers of that type are charged for the entire pivot measure regardless of the chargeable/actual measure of the cargo packed into the containers. ");
			this.contractTypeHint.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 80, true);
			this.contractTypeHint.Name = "contractTypeHint";
			this.contractTypeHint.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(510, 42, true);
			this.contractTypeHint.TabIndex = 2;
			// 
			// EqualizationCalculatorControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.contractTypeHint);
			this.Controls.Add(this.RateLineItemsGrid);
			this.Controls.Add(this.UseInclusiveBreaksCheckBox);
			this.Name = "EqualizationCalculatorControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 144, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RateLineItemsGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
