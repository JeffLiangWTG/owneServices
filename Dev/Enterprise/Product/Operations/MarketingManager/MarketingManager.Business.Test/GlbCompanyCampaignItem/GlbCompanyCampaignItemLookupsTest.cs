using CargoWise.EntityFramework.Testing;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class GlbCompanyCampaignItemLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCampaigns()
		{
			var item = Factory.New<GlbCompanyCampaignItem>();
			AssertNotNull("Campaigns", item.Lookups.Campaigns);
		}

		public void TestClientOrganisations()
		{
			var item = Factory.New<GlbCompanyCampaignItem>();
			AssertNotNull("ClientOrganisations", item.Lookups.ClientOrganisations);
		}
	}
}
