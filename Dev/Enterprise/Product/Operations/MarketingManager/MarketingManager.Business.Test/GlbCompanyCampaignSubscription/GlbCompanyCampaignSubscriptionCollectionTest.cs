
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(GlbCompanyCampaignSubscriptionCollectionForTest))]
	sealed class GlbCompanyCampaignSubscriptionCollectionTest : ActiveBusinessObjectCollectionTestCase<GlbCompanyCampaignSubscriptionCollectionForTest>
	{
		protected override GlbCompanyCampaignSubscriptionCollectionForTest GetCollectionToTest()
		{
			return new GlbCompanyCampaignSubscriptionCollectionForTest(Factory.NewWithValidTestData<OrgHeader>());
		}
	}
}
