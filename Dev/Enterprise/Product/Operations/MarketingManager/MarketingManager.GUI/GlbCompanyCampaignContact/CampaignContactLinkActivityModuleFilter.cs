using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	public class CampaignContactLinkActivityModuleFilter : LinkActivityModuleFilter
	{
		public CampaignContactLinkActivityModuleFilter(ZString description, FilterStripBusinessObject filterStripBusinessObject, GlbCompanyCampaign campaign)
			: base(description, filterStripBusinessObject, campaign)
		{
		}

		public override ZQuery GetLinkActivityFilterQuery()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));
			ZDBOnlySubQuery campaignItemSubQuery = new ZDBOnlySubQuery(typeof(GlbCompanyCampaignItem), GlbCompanyCampaignItemSchema.G8_RecipientID);
			campaignItemSubQuery.AddToFilter(GlbCompanyCampaignItemSchema.G8_G0, Campaign.TouchSourceCampaignPKs);

			ZDBOnlySubQuery campaignClickSubQuery = new ZDBOnlySubQuery(typeof(GlbCompanyCampaignClick), GlbCompanyCampaignClickSchema.GCC_G8_Recipient);

			if (!IsEmpty)
			{
				if (FromDate.IsValid)
				{
					campaignClickSubQuery.AddToFilter(GlbCompanyCampaignClickSchema.GCC_ClickTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, FromDate);
				}

				if (ToDate.IsValid)
				{
					campaignClickSubQuery.AddToFilter(GlbCompanyCampaignClickSchema.GCC_ClickTimeUtc, SQLComparisonOperator.LessThan, ToDate);
				}
			}

			if (!TypeProperty.IsEmpty)
			{
				ZDBOnlySubQuery linkQuery = new ZDBOnlySubQuery(typeof(GlbCompanyCampaignLink), GlbCompanyCampaignLinkSchema.PK);
				linkQuery.AddToFilter(QueryColumn, SQLComparisonOperator.Equal, TypeProperty);

				campaignClickSubQuery.AddSubQuery(GlbCompanyCampaignClickSchema.GCC_GCL, linkQuery, JoinCondition.And);
			}

			campaignItemSubQuery.AddSubQuery(campaignClickSubQuery, JoinCondition.And);
			query.AddSubQuery(ViewCampaignContactSchema.PK, campaignItemSubQuery, JoinCondition.And);
			return query;
		}
	}
}
