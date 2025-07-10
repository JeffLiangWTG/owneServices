using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class WorkOrderDocketLinesGridUserControl
	{
		ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo12;
		ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo13;
		ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo14;
		ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo15;
		ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo16;
		ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17;
		ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo18;
		ZCalcEditColumnStyleInfo bomParentLineNoColumnStyleInfo;

		void InitializeComponent()
		{
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// LinesGrid
			// 
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WorkOrderDocketLinesGridUserControl|6273bbb3-6a52-462e-9a57-a2fe9f38a763", "Lvl.", "Level", "BOM Level", "The level in the BOM product hierarchy at which the current BOM component sits.");
			zCalcEditColumnStyleInfo1.ColumnName = "WE_Level";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WorkOrderDocketLinesGridUserControl|e8bdefd6-f2a7-4d40-a5dc-378ed1616e3c", "Line Comment");
			zTextBoxColumnStyleInfo1.ColumnName = "WE_LineComment";
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WorkOrderDocketLinesGridUserControl|63b5087f-2c28-4a55-beca-247deed363bf", "Short.", "Shortfall", "Shortfall Qty", "");
			zCalcEditColumnStyleInfo3.ColumnName = "WE_ShortfallQuantityCached";
			zCalcEditColumnStyleInfo3.Decimals = 0;
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(WhsWorkOrder);
			// 
			// WorkOrderDocketLinesGridUserControl
			// 
			this.Name = "WorkOrderDocketLinesGridUserControl";
			((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
