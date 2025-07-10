using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignClickCollection : ActiveBusinessObjectCollection<GlbCompanyCampaignClick>
	{
		public GlbCompanyCampaignClickCollection(GlbCompanyCampaignItem campaignItem)
			: base(campaignItem.Factory, campaignItem, new ZQuery(), GlbCompanyCampaignClickSchema.GCC_G8_Recipient)
		{
		}

		public GlbCompanyCampaignClickCollection(GlbCompanyCampaign campaign)
			: base(campaign.Factory, GetCampaignRelationshipQuery(campaign))
		{
		}

		public GlbCompanyCampaignClickCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		static ZQuery GetCampaignRelationshipQuery(GlbCompanyCampaign campaign)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(GlbCompanyCampaignClick));
			ZDBOnlySubQuery campaignItemQuery = new ZDBOnlySubQuery(typeof(GlbCompanyCampaignItem), GlbCompanyCampaignItemSchema.PK);
			campaignItemQuery.AddToFilter(GlbCompanyCampaignItemSchema.G8_G0, campaign.PK);
			query.AddSubQuery(GlbCompanyCampaignClickSchema.GCC_G8_Recipient, campaignItemQuery, JoinCondition.And);
			return query;
		}
	}
}
