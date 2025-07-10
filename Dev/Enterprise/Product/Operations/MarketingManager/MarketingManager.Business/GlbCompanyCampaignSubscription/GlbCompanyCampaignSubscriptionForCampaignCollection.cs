using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignSubscriptionForCampaignCollection : ActiveBusinessObjectCollection<GlbCompanyCampaignSubscription>
	{
		public GlbCompanyCampaignSubscriptionForCampaignCollection(GlbCompanyCampaign parent)
			: base(parent.Factory, parent)
		{
		}

		protected override bool AllowNew => false;
	}
}
