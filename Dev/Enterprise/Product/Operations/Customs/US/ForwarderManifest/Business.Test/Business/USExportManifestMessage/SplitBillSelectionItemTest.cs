using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	class SplitBillSelectionItemTest : TestCaseWithFactory
	{
		public void TestSelectionDescription()
		{
			var header = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "123";
			var ex = AssertExceptionThrown<System.ArgumentNullException>(() => new SplitBillSelectionItem(null));
#if NETFRAMEWORK
			AssertEquals("Value cannot be null.\r\nParameter name: bill", ex.Message);
#else
			AssertEquals("Value cannot be null. (Parameter 'bill')", ex.Message);
#endif
			var item2 = new SplitBillSelectionItem(bill);
			AssertEquals("123", item2.SelectionDescription(false));
		}

		public void TestActionType()
		{
			var manifestHeader = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			bill.ABL_BillNumber = "123";
			var splitBillSelectionItem = new SplitBillSelectionItem(bill);
			AssertEquals(USExportBillOfLadingActionCodeType.Codes.A, splitBillSelectionItem.ActionType);
			splitBillSelectionItem.ActionType = USExportBillOfLadingActionCodeType.Codes.D;
			AssertEquals(USExportBillOfLadingActionCodeType.Codes.D, splitBillSelectionItem.ActionType);
		}
	}
}
