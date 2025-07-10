namespace Enterprise.Rating.GUI
{
	public partial class CombinedControl
	{
		private ZArchitecture.ZGrid RateLineItemsGrid;
		private System.ComponentModel.Container components = null;

		private void InitializeComponent()
		{
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.RateLineItemsGrid = new ZArchitecture.ZGrid();
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
			this.RateLineItemsGrid.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
						| System.Windows.Forms.AnchorStyles.Left
						| System.Windows.Forms.AnchorStyles.Right;
			this.RateLineItemsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "TM_Type";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "TM_Break";
			zCalcEditColumnStyleInfo1.Decimals = 1;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CombinedControl|b2139430-fbd5-4fac-82d0-fcd5a511aedc", "Units");
			zDropEditColumnStyleInfo2.ColumnName = "TM_BreakWeightVolume";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CombinedControl|574d825a-5881-48f3-8746-a1c951c1cb68", "Rate");
			zCalcEditColumnStyleInfo2.ColumnName = "TM_RelevantValue";
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CombinedControl|39285812-3c94-450b-bd73-f31143d6f03c", "Flat Amount");
			zCalcEditColumnStyleInfo3.ColumnName = "TM_FlatAmount";
			zCheckBoxColumnStyleInfo1.ColumnName = "TM_CallForPricing";
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo1.ColumnName = "TM_Text";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CombinedControl|7cba3a1c-04d3-40bf-8aa4-fc28b2654700", "Reason");
			zTextBoxColumnStyleInfo1.IsVisible = false;
			this.RateLineItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.RateLineItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.RateLineItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.RateLineItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.RateLineItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.RateLineItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.RateLineItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RateLineItemsGrid.GridId = "1043e018-dcdb-4b5a-afbd-26af84e0a4a6";
			this.RateLineItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RateLineItemsGrid.LayoutKey = "RateLineItemsGridCombined";
			this.RateLineItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RateLineItemsGrid.Name = "RateLineItemsGrid";
			this.RateLineItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 120, true);
			this.RateLineItemsGrid.TabIndex = 0;
			// 
			// CombinedControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RateLineItemsGrid);
			this.Name = "CombinedControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 144, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RateLineItemsGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
