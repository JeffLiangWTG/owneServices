using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgMatchApprovalLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMatchOrg1s()
		{
			OrgMatchApproval orgMatchApproval = Factory.New<OrgMatchApproval>();
			OrgMatchApprovalLookups orgMatchApprovalLookups = new OrgMatchApprovalLookups(orgMatchApproval);
			OrganisationsFindBoxCollection organisationsFindBoxCollection = orgMatchApprovalLookups.MatchOrg1s;
			AssertEquals(typeof(OrganisationsFindBoxCollection), organisationsFindBoxCollection.GetType());
		}
	}
}
