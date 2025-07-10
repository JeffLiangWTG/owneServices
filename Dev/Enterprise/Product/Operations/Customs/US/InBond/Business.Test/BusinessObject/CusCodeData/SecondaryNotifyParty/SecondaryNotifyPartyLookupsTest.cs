using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class SecondaryNotifyPartyLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCY_CodeList()
		{
			var snp = Factory.New<SecondaryNotifyParty>();
			var list = snp.Lookups.CY_CodeList;
			AssertEquals(typeof(SecondaryNotifyPartyCodeList), list.GetType());
			AssertEquals(new SecondaryNotifyPartyCodeList().Count, list.Count);
		}
	}
}
