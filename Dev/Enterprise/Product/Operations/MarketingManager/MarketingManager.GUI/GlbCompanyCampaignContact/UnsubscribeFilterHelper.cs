using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	static class UnsubscribeFilterHelper
	{
		public static ZQuery GetUnsubscribeFilter(this GlbCompanyCampaign campaign)
		{
			if (campaign == null)
			{
				throw new ArgumentNullException(nameof(campaign));
			}

			var query = new ZQuery();

			query.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_MediaCategory, new object[] { ZString.Empty, campaign.G0_Category });
			query.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_MediaType, new object[] { ZString.Empty, campaign.G0_Type });

			return query;
		}
	}
}
