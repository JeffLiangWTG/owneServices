using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class CartageControl
	{
		private ZDropEdit EquipmentDropEdit;
		private ZArchitecture.ZGrid RateLineItemsGrid;
		private ZDropEdit ConvFactorDropEdit;
		private System.ComponentModel.Container components = null;

		private void InitializeComponent()
		{
			this.UseAccumulatedCheckbox.TabIndex = 3;
			this.HigherChargeableLowerRateCheckBox.TabIndex = 4;
			this.UseInclusiveBreaksCheckBox.TabIndex = 5;
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.EquipmentDropEdit = new ZDropEditWithFixedWidth();
			this.RateLineItemsGrid = new ZArchitecture.ZGrid();
			this.ConvFactorDropEdit = new ZDropEditWithFixedWidth();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RateLineItemsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = DataSourceTypeForBinding;
			// 
			// EquipmentDropEdit
			// 
			this.EquipmentDropEdit.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			this.EquipmentDropEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CartageControl|a24d845c-4656-4314-a90f-4486b3a57856", "Drop Mode");
			this.EquipmentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 103, true);
			this.EquipmentDropEdit.Name = "EquipmentDropEdit";
			this.EquipmentDropEdit.PreBoundMaxLength = 3;
			this.EquipmentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 20, true);
			this.EquipmentDropEdit.TabIndex = 2;
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
			zDropEditColumnStyleInfo2.ColumnName = "TM_BreakWeightVolume";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CartageControl|7a7304db-2661-4f2f-b7b4-3dc8067c3dd6", "Rate");
			zCalcEditColumnStyleInfo2.ColumnName = "TM_RelevantValue";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "TM_FlatAmount";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "TM_BreakMinimum";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo1.ColumnName = "TM_Text";
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CombinedControl|7cba3a1c-04d3-40bf-8aa4-fc28b2654700", "Reason");
			zCheckBoxColumnStyleInfo1.ColumnName = "TM_CallForPricing";
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			this.RateLineItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.RateLineItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.RateLineItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.RateLineItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.RateLineItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.RateLineItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.RateLineItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RateLineItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.RateLineItemsGrid.GridId = "b16a4b3c-3b50-4af3-a7ad-e0c4a8b2e277";
			this.RateLineItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RateLineItemsGrid.LayoutKey = "RateLineItemsGridCartage";
			this.RateLineItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RateLineItemsGrid.Name = "RateLineItemsGrid";
			this.RateLineItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 75, true);
			this.RateLineItemsGrid.TabIndex = 0;
			// 
			// ConvFactorDropEdit
			// 
			this.ConvFactorDropEdit.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			this.ConvFactorDropEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CartageControl|f73ca1ea-89d7-43f2-ab53-3706c9080dd5", "Conv. Factor");
			this.ConvFactorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 79, true);
			this.ConvFactorDropEdit.Name = "ConvFactorDropEdit";
			this.ConvFactorDropEdit.PreBoundMaxLength = 5;
			this.ConvFactorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 20, true);
			this.ConvFactorDropEdit.TabIndex = 1;
			// 
			// CartageControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ConvFactorDropEdit);
			this.Controls.Add(this.RateLineItemsGrid);
			this.Controls.Add(this.EquipmentDropEdit);
			this.Name = "CartageControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RateLineItemsGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
