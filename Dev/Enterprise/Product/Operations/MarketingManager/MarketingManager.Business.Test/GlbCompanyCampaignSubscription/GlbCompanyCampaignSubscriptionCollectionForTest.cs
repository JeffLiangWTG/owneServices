using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class GlbCompanyCampaignSubscriptionCollectionForTest : GlbCompanyCampaignSubscriptionCollection<BusinessObject, GlbCompanyCampaignSubscription>
	{
		public GlbCompanyCampaignSubscriptionCollectionForTest(BusinessObject parent)
			: base(parent, parent.Factory)
		{
		}
	}
}
