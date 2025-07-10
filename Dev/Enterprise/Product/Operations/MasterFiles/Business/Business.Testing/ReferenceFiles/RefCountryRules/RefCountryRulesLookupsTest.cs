using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefCountryRulesLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUltimateConsigneeRule_List()
		{
			var list = new RefCountryRulesLookups(null).UltimateConsigneeRule_List;
			Assert("NON", list.ContainsCode("NON"));
			Assert("MAN", list.ContainsCode("MAN"));
			Assert("VER", list.ContainsCode("VER"));
			AssertEquals(3, list.Count);
		}
	}
}
