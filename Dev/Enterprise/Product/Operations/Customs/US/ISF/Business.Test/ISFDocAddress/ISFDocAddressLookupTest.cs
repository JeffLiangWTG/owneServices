using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	public class ISFDocAddressLookupTest : BusinessObjectLookupsTestCase
	{
		public void TestGovRegNumTypes()
		{
			var address = Factory.New<ISFDocAddress>();
			address.E2_AddressOverride = true;

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = true;

			var list = address.Lookups.GovRegNumTypes;
			AssertEquals("Should have 9 items.", 9, list.Count);
			AssertEquals("GovRegNumTypes", "DEF, EIN, DUN, DN4, FRN, CBN, SSN, PAS, DRV", list.CodesAsString);

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			list = address.Lookups.GovRegNumTypes;
			AssertEquals("Should have 8 items.", 8, list.Count);
			AssertEquals("GovRegNumTypes", "DEF, EIN, DUN, DN4, FRN, CBN, PAS, DRV", list.CodesAsString);
		}
	}
}
