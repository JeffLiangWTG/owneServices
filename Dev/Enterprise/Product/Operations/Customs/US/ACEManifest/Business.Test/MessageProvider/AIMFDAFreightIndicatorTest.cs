using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class AIMFDAFreightIndicatorTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.FDAIndicator = true;
			var aimFDAFreightIndicator = new AIMFDAFreightIndicator(bill);
			AssertEquals(true, aimFDAFreightIndicator.IsFDAFreight);
		}
	}
}
