using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NZ.Manifest.Business.Testing
{
	sealed class BillNumberWrapperTest : TestCaseWithFactory
	{
		public void TestBillNumberWrapper()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "Test123";
			var wrapper = new BillNumberWrapper(bill);
			AssertEquals("Test123", wrapper.BillNumber);
			AssertEquals(NZ.TradeSingleWindow.BillTypeList.Codes.MB, wrapper.BillType);
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			wrapper = new BillNumberWrapper(bill);
			AssertEquals(NZ.TradeSingleWindow.BillTypeList.Codes.BM, wrapper.BillType);
		}
	}
}
