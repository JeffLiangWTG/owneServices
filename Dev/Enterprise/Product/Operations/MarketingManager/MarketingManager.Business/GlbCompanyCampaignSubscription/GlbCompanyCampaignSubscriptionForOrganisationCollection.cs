using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignSubscriptionForOrganisationCollection : GlbCompanyCampaignSubscriptionCollection<OrgHeader, GlbCompanyCampaignSubscription>, IGlbCompanyCampaignSubscriptionForOrganisationCollection
	{
		public GlbCompanyCampaignSubscriptionForOrganisationCollection(OrgHeader parent)
			: base(parent)
		{
		}

		IGlbCompanyCampaignSubscription IGlbCompanyCampaignSubscriptionForOrganisationCollection.this[int index] => base[index];
	}
}
