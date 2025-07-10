using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbCapabilityLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCapabilityScopeCodes()
		{
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			AssertEquals(2, capability.Lookups.ScopeCodes.Count);
		}
	}
}
