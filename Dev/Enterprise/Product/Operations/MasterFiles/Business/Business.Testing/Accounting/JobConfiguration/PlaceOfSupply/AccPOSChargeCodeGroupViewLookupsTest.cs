using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business.Testing.Accounting
{
	sealed class AccPOSChargeCodeGroupViewLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCompanies()
		{
			var group = Factory.New<AccPOSChargeCodeGroup>();
			AssertEquals("Pre-condition: Set to the Current Company by default", Env.CurrentCompanyPK, group.GRO_GC);
			var lookups = group.Lookups;
			AssertNotNull(lookups);

			AssertNotNull(lookups.Companies);
			Assert("No filtering", lookups.Companies.CompleteFilter.IsEmpty);
		}
	}
}
