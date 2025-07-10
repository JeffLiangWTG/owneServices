using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public static class OpportunitiesCreation
	{
		public static OpportunityCreationChartData LoadOpportunities(GlbCompanyCampaign campaign)
		{
			if (campaign?.Opportunities == null)
			{
				return new OpportunityCreationChartData();
			}

			var campaignPkList = new List<ZGuid> { campaign.PK };

			if (campaign.IsMasterCampaign)
			{
				campaignPkList.AddRange(campaign.AllTouches.Select(t => t.PK));
			}

			var opportunitiesList = new BusinessObjectFactory().Load<OrgOpportunity>(new ZQuery(OrgOpportunitySchema.P8_G0, campaignPkList));
			var currentCount = opportunitiesList.Count(o => o.P8_Status == "CRT");
			var wonCount = opportunitiesList.Count(o => o.P8_Status == "WON");
			var lostCount = opportunitiesList.Count(o => o.P8_Status == "LOS");
			var totalCount = opportunitiesList.Length;
			var otherCount = totalCount - currentCount - wonCount - lostCount;
			var winRatio = totalCount == 0 ? 0 : (decimal)wonCount / totalCount;

			var chartData = new OpportunityCreationChartData
			{
				CurrentCount = currentCount,
				WonCount = wonCount,
				OtherCount = otherCount,
				LostCount = lostCount,
				TotalCount = totalCount,
				WinRatio = winRatio
			};

			return chartData;
		}
	}
}
