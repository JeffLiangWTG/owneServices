using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class UNDGCountryReferenceLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTypes()
		{
			AssertEquals("PSA, ICPE", UNDGCountryReferenceLookups.Types.CodesAsString);
		}
	}
}
