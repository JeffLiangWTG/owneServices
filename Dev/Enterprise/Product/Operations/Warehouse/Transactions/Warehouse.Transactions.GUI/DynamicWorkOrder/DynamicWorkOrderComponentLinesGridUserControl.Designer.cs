using CargoWiseOne.ResourceStrings;

namespace Enterprise.Warehouse.Transactions.GUI
{
	partial class DynamicWorkOrderComponentLinesGridUserControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo11 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// LinesGrid
			//
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderDocketLinesGridUserControl|FDC7C4D2-7A1D-4FFB-A893-F9ABD0F81CF3", "Inwards Entry Key");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "WE_BondedEntryKey";
			zTextBoxColumnStyleInfo1.GroupName = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderDocketLinesGridUserControl|85fed39d-ec7e-42fa-ba38-28da686f59b4", "Customs Data");
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo11.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderDocketLinesGridUserControl|e2b0ee42-3c88-4d3b-a90c-3ae50ed2d795", "Qty Met", "Quantity Met", "");
			zCalcEditColumnStyleInfo11.BindToDecimalPlaces = "SupplierPart+OP_CountDecimalPlaces";
			zCalcEditColumnStyleInfo11.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo11.ColumnName = "SumOfUnitsMet";
			zCalcEditColumnStyleInfo11.Decimals = 0;
			zCalcEditColumnStyleInfo11.GroupName = ResourceStringData.Empty;
			zCalcEditColumnStyleInfo11.IsMandatory = true;
			this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo11);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Transactions.Business.WhsDynamicWorkOrder);
			// 
			// OrderDocketLinesGridUserControl
			// 
			this.Name = "DynamicWorkOrderComponentLinesGridUserControl";
			this.BindingSource.SetBindingMember(this.LinesGrid, "Lines.ChildComponentLinesCollection");
			((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
