using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Module.Testing
{
	class WarehouseOperatorTransactionsFilterStripControlTest : TestCaseWithFactory
	{
		public void TestWarehouseOperatorTransactionsFilteredGridColumns()
		{
			var transactions = new CusWHSOperatorTransactionCollection(Factory);
			var filterBusinessObject = new WarehouseOperatorTransactionsFilterStripBusinessObject();
			using (var form = new ZForm())
			using (var filterStrip = new WarehouseOperatorTransactionsFilterStripControl(transactions, filterBusinessObject))
			{
				form.Controls.Add(filterStrip);
				form.Show();
				var filteredGrid = filterStrip.FilteredGrid;
				AssertEquals("Transaction Type", filteredGrid.GetColumnStyle(CusWHSOperatorTransaction.Schema.WOT_TransactionType).CaptionResourceString.Caption);
				AssertEquals("Status", filteredGrid.GetColumnStyle(CusWHSOperatorTransaction.Schema.WOT_Status).CaptionResourceString.Caption);
				AssertEquals("Export Type", filteredGrid.GetColumnStyle(CusWHSOperatorTransaction.Schema.WOT_ExportType).CaptionResourceString.Caption);
				AssertEquals("Owner Reference", filteredGrid.GetColumnStyle(CusWHSOperatorTransaction.Schema.WOT_OwnerReference).CaptionResourceString.Caption);
				AssertEquals("Product Code", filteredGrid.GetColumnStyle(nameof(CusWHSOperatorTransaction.Product) + "+" + OrgSupplierPartSchema.Constants.OP_PartNum).CaptionResourceString.Caption);
				AssertEquals("Quantity", filteredGrid.GetColumnStyle(CusWHSOperatorTransaction.Schema.WOT_Quantity).CaptionResourceString.Caption);
				AssertEquals("Is Customs Controlled", filteredGrid.GetColumnStyle(CusWHSOperatorTransaction.Schema.WOT_IsCustomsControlled).CaptionResourceString.Caption);
				AssertEquals("Value", filteredGrid.GetColumnStyle(CusWHSOperatorTransaction.Schema.WOT_TotalValue).CaptionResourceString.Caption);
				AssertEquals("Currency", filteredGrid.GetColumnStyle(CusWHSOperatorTransaction.Schema.WOT_RX_NKCurrency).CaptionResourceString.Caption);
				AssertEquals("Country/Region Origin", filteredGrid.GetColumnStyle(CusWHSOperatorTransaction.Schema.WOT_RN_NKOrigin).CaptionResourceString.Caption);
				AssertEquals("Batch", filteredGrid.GetColumnStyle("Batch+WOB_Batch").CaptionResourceString.Caption);
				AssertEquals("Batch Line No", filteredGrid.GetColumnStyle(CusWHSOperatorTransaction.Schema.WOT_BatchLineNo).CaptionResourceString.Caption);
				AssertEquals("Transaction Date", filteredGrid.GetColumnStyle(CusWHSOperatorTransaction.Schema.WOT_TransactionDate).CaptionResourceString.Caption);
				AssertEquals("Line Reference", filteredGrid.GetColumnStyle(CusWHSOperatorTransaction.Schema.WOT_LineReference).CaptionResourceString.Caption);
				AssertEquals("Customs Reference Number", filteredGrid.GetColumnStyle(CusWHSOperatorTransaction.Schema.WOT_CustomsEntryNumber).CaptionResourceString.Caption);
			}
		}
	}
}
