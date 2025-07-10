using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCommissionAgreementLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCustomers()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgParent = Factory.NewWithValidTestData<OrgHeader>();
			var orgSubsidiary = Factory.NewWithValidTestData<OrgHeader>();
			org.AddRelatedParty(orgParent.PK, RelatedPartyTypeList.Codes.ManagementGrouping, RelatedPartyDirectionList.Codes.Forwarder, Core.Constants.TransportModes.All, ZString.Empty, null);
			orgSubsidiary.AddRelatedParty(org.PK, RelatedPartyTypeList.Codes.ManagementGrouping, RelatedPartyDirectionList.Codes.Forwarder, Core.Constants.TransportModes.Sea, ZString.Empty, null);
			Factory.Save();

			var opportunity = org.SalesOpportunities.AddNew();
			var agreement = opportunity.CommissionAgreements.AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { orgParent.PK, org.PK, orgSubsidiary.PK }, agreement.Lookups.Customers.Select(c => c.PK));

			opportunity.P8_OH = ZGuid.Empty;
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<ZGuid>(), agreement.Lookups.Customers.Select(c => c.PK));
		}

		public void TestCustomers_Cycle()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.AddRelatedParty(org.PK, RelatedPartyTypeList.Codes.ManagementGrouping, RelatedPartyDirectionList.Codes.Forwarder, Core.Constants.TransportModes.All, ZString.Empty, null);
			Factory.Save();

			var opportunity = org.SalesOpportunities.AddNew();
			var agreement = opportunity.CommissionAgreements.AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { org.PK }, agreement.Lookups.Customers.Select(c => c.PK));
		}
	}
}
