using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class WarehouseOperatorTransactionsListsTest : TestCaseWithFactory
	{
		public void TestWarehouseOperatorTransactionExportTypeList()
		{
			var testList = new WarehouseOperatorTransactionExportTypeList();
			AssertEquals(2, testList.Count);
			Assert(testList.ContainsCode(WarehouseOperatorTransactionExportTypeList.Codes.EXP));
			Assert(testList.ContainsCode(WarehouseOperatorTransactionExportTypeList.Codes.BLN));
			AssertEquals("Export Non-Blns", WarehouseOperatorTransactionExportTypeList.Descriptions.EXP);
			AssertEquals("Export BLN", WarehouseOperatorTransactionExportTypeList.Descriptions.BLN);
		}

		public void TestWarehouseOperatorTransactionStatusList()
		{
			var testList = new WarehouseOperatorTransactionStatusList();
			AssertEquals(4, testList.Count);
			Assert(testList.ContainsCode(WarehouseOperatorTransactionStatusList.Codes.QUE));
			Assert(testList.ContainsCode(WarehouseOperatorTransactionStatusList.Codes.VAL));
			Assert(testList.ContainsCode(WarehouseOperatorTransactionStatusList.Codes.CLS));
			Assert(testList.ContainsCode(WarehouseOperatorTransactionStatusList.Codes.CAN));
			AssertEquals("Queued", WarehouseOperatorTransactionStatusList.Descriptions.QUE);
			AssertEquals("Validated", WarehouseOperatorTransactionStatusList.Descriptions.VAL);
			AssertEquals("Closed", WarehouseOperatorTransactionStatusList.Descriptions.CLS);
			AssertEquals("Cancelled", WarehouseOperatorTransactionStatusList.Descriptions.CAN);
		}

		public void TestWarehouseOperatorTransactionTypeList()
		{
			var testList = new WarehouseOperatorTransactionTypeList();
			AssertEquals(3, testList.Count);
			Assert(testList.ContainsCode(WarehouseOperatorTransactionTypeList.Codes.ORD));
			Assert(testList.ContainsCode(WarehouseOperatorTransactionTypeList.Codes.REC));
			Assert(testList.ContainsCode(WarehouseOperatorTransactionTypeList.Codes.ADJ));
			AssertEquals("Order", WarehouseOperatorTransactionTypeList.Descriptions.ORD);
			AssertEquals("Receipt", WarehouseOperatorTransactionTypeList.Descriptions.REC);
			AssertEquals("Adjustment", WarehouseOperatorTransactionTypeList.Descriptions.ADJ);
		}
	}
}
