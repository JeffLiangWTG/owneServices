namespace Enterprise.Rating.GUI
{
	public partial class WarehouseLocationTypeCalculatorControl
	{
		ZArchitecture.ZGrid RateLineItemsGrid;
		System.ComponentModel.Container components = null;

		void InitializeComponent()
		{
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
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
			zDropEditColumnStyleInfo1.BindToList = "Lookups.WarehouseLocationTypes";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("WarehouseLocationTypeCalculatorControl|84481713-2933-4e64-b192-3e3b0e4b7a49", "Code");
			zDropEditColumnStyleInfo1.ColumnName = "TM_Type";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("WarehouseLocationTypeCalculatorControl|acd7474e-7335-4f55-af9a-af0233289d31", "Location Type");
			zTextBoxColumnStyleInfo1.ColumnName = "WarehouseLocationTypeDesc";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("WarehouseLocationTypeCalculatorControl|b11beb49-9a9f-449a-a727-5e861aff7120", "Rate");
			zCalcEditColumnStyleInfo1.ColumnName = "TM_RelevantValue";
			this.RateLineItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.RateLineItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RateLineItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.RateLineItemsGrid.GridId = "5325fd58-3498-4e01-8392-11bfabfb3389";
			this.RateLineItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RateLineItemsGrid.LayoutKey = "RateLineItemsGridCombined";
			this.RateLineItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RateLineItemsGrid.Name = "RateLineItemsGrid";
			this.RateLineItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 120, true);
			this.RateLineItemsGrid.TabIndex = 0;
			// 
			// WarehouseLocationTypeCalculatorControl
			// 
			this.Controls.Add(this.RateLineItemsGrid);
			this.Name = "WarehouseLocationTypeCalculatorControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 144, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RateLineItemsGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
