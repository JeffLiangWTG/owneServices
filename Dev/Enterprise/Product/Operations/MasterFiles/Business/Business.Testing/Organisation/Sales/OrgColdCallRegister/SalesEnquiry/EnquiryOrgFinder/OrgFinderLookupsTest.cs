using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgFinderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOrganisations()
		{
			OrgHeaderForEnquiryMatching orgForMatching = Factory.New<OrgHeaderForEnquiryMatching>();
			EnquiryOrgFinder finder = new EnquiryOrgFinder(Factory, orgForMatching, true, true);
			OrgFinderLookups lookups = new OrgFinderLookups(finder);

			AssertNotNull(lookups.Organisations);
		}
	}
}
