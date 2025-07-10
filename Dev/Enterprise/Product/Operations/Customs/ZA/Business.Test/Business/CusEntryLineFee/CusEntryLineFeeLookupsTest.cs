using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class CusEntryLineFeeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRateOverrideReasonList()
		{
			var entryLineFee = Factory.New<CusEntryLineFee>();
			AssertEquals("ADD, OVR", entryLineFee.Lookups.RateOverrideReasonList.CodesAsString);
		}
	}
}
