using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class SplitBillSelectionItemTest : TestCaseWithFactory
	{
		public void TestSelectionDescription()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "123";
			var arrivalHeader = header.ArrivalHeaders.AddNew();
			arrivalHeader.ATH_Reference = "A";
			var ex = AssertExceptionThrown<System.ArgumentNullException>(() => new SplitBillSelectionItem(null, null));
#if NETFRAMEWORK
			AssertEquals("Value cannot be null.\r\nParameter name: bill", ex.Message);
#else
			AssertEquals("Value cannot be null. (Parameter 'bill')", ex.Message);
#endif
			var item2 = new SplitBillSelectionItem(bill, null);
			AssertEquals("123", item2.SelectionDescription(false));
			var item3 = new SplitBillSelectionItem(bill, arrivalHeader);
			AssertEquals("123 A", item3.SelectionDescription(false));
		}
	}
}
