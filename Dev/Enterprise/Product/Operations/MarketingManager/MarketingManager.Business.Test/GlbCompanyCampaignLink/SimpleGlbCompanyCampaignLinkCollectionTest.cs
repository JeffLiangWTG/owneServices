using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(SimpleGlbCompanyCampaignLinkCollection))]
	sealed class SimpleGlbCompanyCampaignLinkCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetDefaultsForNewChild()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var collection = new SimpleGlbCompanyCampaignLinkCollection(campaign, Factory);
			var link = collection.AddNew();
			AssertEquals(link.GCL_G0_Campaign, campaign.PK);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			return new SimpleGlbCompanyCampaignLinkCollection(campaign, Factory);
		}
	}
}
