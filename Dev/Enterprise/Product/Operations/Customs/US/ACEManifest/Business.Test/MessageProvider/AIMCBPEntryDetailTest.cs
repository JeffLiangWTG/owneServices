using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.AIM.Messaging;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class AIMCBPEntryDetailTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.CustomsEntryNumberType = ACEManifestBillEntryNumberTypes.Codes.Informal;
			bill.CustomsEntryNumber = "12345678901";
			var cbpEntryDetail = new AIMCBPEntryDetail(bill) as IAIMCBPEntryDetail;
			AssertEquals(ACEManifestBillEntryNumberTypes.Codes.Informal, cbpEntryDetail.EntryType);
			AssertEquals("12345678901", cbpEntryDetail.EntryNumber);
		}
	}
}
