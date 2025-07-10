
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(GlbCompanyCampaignLinkCollection))]
	sealed class GlbCompanyCampaignLinkCollectionTest : ActiveBusinessObjectCollectionTestCase<GlbCompanyCampaignLinkCollection>
	{
		protected override GlbCompanyCampaignLinkCollection GetCollectionToTest()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			return new GlbCompanyCampaignLinkCollection(campaign);
		}
	}
}
