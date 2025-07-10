using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NZ.Manifest.Business.Testing
{
	sealed class OCRBillWrapperTest : TestCaseWithFactory
	{
		public void TestOCRBillWrapper()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.CustomsEntryNumber = "Test123";
			var wrapper = new OCRBillWrapper(bill);
			AssertEquals("Test123", wrapper.CustomsClearanceNo);
		}
	}
}
