using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCreditorGroupLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestHoldOptions()
		{
			var creditorGroup = Factory.NewWithValidTestData<OrgCreditorGroup>();
			Assert(creditorGroup.Lookups.HoldOptions.Count == 2);
			AssertEquals("DNM, ALM", creditorGroup.Lookups.HoldOptions.CodesAsString);
		}
	}
}
