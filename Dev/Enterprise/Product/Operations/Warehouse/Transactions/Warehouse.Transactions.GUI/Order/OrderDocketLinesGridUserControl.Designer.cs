using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class OrderDocketLinesGridUserControl
	{
		ZTextBoxColumnStyleInfo orderedHoldCodeEntry;
		ZTextBoxColumnStyleInfo packIDEntry;
		ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10;
		ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9;
		ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10;
		ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo12;
		ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo13;
		ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo14;
		ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo15;
		ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo16;
		ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo10;
		ZDropEditColumnStyleInfo PickGroupDropEditColumnStyleInfo;

		void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(WhsOrder);
			// 
			// OrderDocketLinesGridUserControl
			// 
			this.Name = "OrderDocketLinesGridUserControl";
			this.BindingSource.SetBindingMember(this.LinesGrid, "ParentLines");

			((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
