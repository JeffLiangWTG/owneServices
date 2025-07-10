using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class CBIControl
	{
		private ZArchitecture.ZGrid RateLineItemsGrid;
		private System.ComponentModel.Container components = null;
		ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditRelevantValueColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
		ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditHoursColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
		ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditHourRateColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
		private bool MaterialCalculationLayoutEnabled = false;

		private void InitializeComponent()
		{
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
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

			this.RateLineItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.RateLineItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);

			CombinedBreaksWithIncrementCalculator calc = ViewCalculatorForBinding.Calculator as CombinedBreaksWithIncrementCalculator;

			zCalcEditRelevantValueColumnStyleInfo.BindToDecimalPlaces = null;
			zCalcEditRelevantValueColumnStyleInfo.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CombinedControl|574d825a-5881-48f3-8746-a1c951c1cb68", "Rate");
			zCalcEditRelevantValueColumnStyleInfo.ColumnName = "TM_RelevantValue";

			zCalcEditHoursColumnStyleInfo.BindToDecimalPlaces = null;
			zCalcEditHoursColumnStyleInfo.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CombinedControl|f3d6e3f4-8c58-4bc6-94a9-812fbe66ecf1", "Hours");
			zCalcEditHoursColumnStyleInfo.ColumnName = "TM_BreakHour";

			zCalcEditHourRateColumnStyleInfo.BindToDecimalPlaces = null;
			zCalcEditHourRateColumnStyleInfo.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CombinedControl|4d0b92e2-3fd7-4891-b54f-7b3a0aa9b3dd", "Hour Rate");
			zCalcEditHourRateColumnStyleInfo.ColumnName = "TM_BreakHourRate";

			zCheckBoxColumnStyleInfo1.ColumnName = "TM_CallForPricing";
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo1.ColumnName = "TM_Text";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CombinedControl|7cba3a1c-04d3-40bf-8aa4-fc28b2654700", "Reason");
			zTextBoxColumnStyleInfo1.IsVisible = false;

			this.RateLineItemsGrid.ColumnStyles.Add(zCalcEditRelevantValueColumnStyleInfo);
			this.RateLineItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.RateLineItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);

			this.RateLineItemsGrid.GridId = "b8f3a90a-2c83-4c27-8c1e-df9948db6818";
			this.RateLineItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RateLineItemsGrid.LayoutKey = "RateLineItemsGridCBICombined";
			this.RateLineItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RateLineItemsGrid.Name = "RateLineItemsGrid";
			this.RateLineItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 120, true);
			this.RateLineItemsGrid.TabIndex = 0;

			//
			// UseAccumulatedCheckbox
			//
			this.UseAccumulatedCheckbox.Visible = false;
			//
			// HigherChargeableLowerRateCheckBox
			//
			this.HigherChargeableLowerRateCheckBox.Visible = false;
			//
			// BreaksPerDropDown
			//
			this.BreaksPerDropDown.Visible = false;

			// 
			// CBIControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RateLineItemsGrid);
			this.Name = "CBIControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 144, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RateLineItemsGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

			MaterialCalculationLayoutEnabled = true;
		}

		void ShowLabourRateComponent()
		{
			if (MaterialCalculationLayoutEnabled)
			{
				MaterialCalculationLayoutEnabled = false;
				this.RateLineItemsGrid.RemoveAndDisposeColumn(zCalcEditRelevantValueColumnStyleInfo.ColumnName);
				this.RateLineItemsGrid.AddColumn(zCalcEditHoursColumnStyleInfo);
				this.RateLineItemsGrid.AddColumn(zCalcEditHourRateColumnStyleInfo);
				this.RateLineItemsGrid.RefreshTableStyles();
			}
		}

		void ShowMaterialRateComponent()
		{
			if (!MaterialCalculationLayoutEnabled)
			{
				MaterialCalculationLayoutEnabled = true;
				this.RateLineItemsGrid.RemoveAndDisposeColumn(zCalcEditHoursColumnStyleInfo.ColumnName);
				this.RateLineItemsGrid.RemoveAndDisposeColumn(zCalcEditHourRateColumnStyleInfo.ColumnName);
				this.RateLineItemsGrid.AddColumn(zCalcEditRelevantValueColumnStyleInfo);
				this.RateLineItemsGrid.RefreshTableStyles();
			}
		}
	}
}
