using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class SecondaryNotifyPartyLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSCACOrFIRMSList()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var snp = bill.SecondaryNotifyParties.AddNew();
			AssertEquals(typeof(USCCarrierAndFIRMSCollection), snp.Lookups.SCACOrFIRMSList.GetType());
		}
	}
}
