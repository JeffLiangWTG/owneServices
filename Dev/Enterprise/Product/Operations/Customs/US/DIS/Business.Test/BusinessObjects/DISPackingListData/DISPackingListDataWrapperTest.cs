using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class DISPackingListDataWrapperTest : TestCaseWithFactory
	{
		public void TestDISPackingListDataWrapper()
		{
			var data = new DISPackingListData(Factory);
			data.PackingListNumber = "1";
			data.InvoiceNumber = "2";
			data.PurchaseOrderNumber = "3";
			var iData = (IDISPackingList)new DISPackingListDataWrapper(data);
			AssertEquals("1", iData.PackingListNumber);
			AssertEquals("2", iData.InvoiceNumber);
			AssertEquals("3", iData.PurchaseOrderNumber);
		}
	}
}
