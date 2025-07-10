using System.Collections.Generic;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class OpportunityCreationAssignment
	{
		public OpportunityCreationAssignment(GlbCompanyCampaign campaign)
		{
			this.campaign = campaign;
		}

		readonly GlbCompanyCampaign campaign;

		public void SetStaffPoolAssignments(List<GlbCompanyCampaignItem> campaignItemsList)
		{
			var assignmentPoolData = GetAssignmentPoolData();

			foreach (var campaignItem in campaignItemsList)
			{
				ZString salesPerson;

				var count = assignmentPoolData.CurrentPoolRatio.Sum(pair => pair.Value);
				if (count == 0)
				{
					salesPerson = assignmentPoolData.NormalizedRatio.MaxBy(pair => pair.Value).Key;
				}
				else
				{
					salesPerson = assignmentPoolData.NormalizedRatio.Select(item =>
							new
							{
								item.Key,
								SendRatioDelta = item.Value - (assignmentPoolData.CurrentPoolRatio.ContainsKey(item.Key) ? assignmentPoolData.CurrentPoolRatio[item.Key] : 0.0) / count
							})
						.MaxBy(arg => arg.SendRatioDelta).Key;
				}

				var orgOpportunity = campaignItem.Factory.LoadTop1<OrgOpportunity>(new ZQuery(new ZQuery(OrgOpportunitySchema.P8_OC, campaignItem.G8_RecipientID), JoinCondition.And, new ZQuery(OrgOpportunitySchema.P8_G0, campaignItem.G8_G0)));
				if (orgOpportunity == null)
				{
					continue;
				}

				orgOpportunity.P8_GS_NKPrimarySalesPerson = salesPerson;

				if (assignmentPoolData.CurrentPoolRatio.ContainsKey(salesPerson))
				{
					assignmentPoolData.CurrentPoolRatio[salesPerson]++;
				}
				else
				{
					assignmentPoolData.CurrentPoolRatio[salesPerson] = 1;
				}
			}
		}

		internal OpportunityCreationAssignmentPoolData GetAssignmentPoolData()
		{
			var poolRatioDictionary = new Dictionary<ZString, int>();
			var totalPoolRatio = (double)campaign.SenderPool.Sum(item => item.GCP_SendRatio);
			var pool = campaign.SenderPool.Select(item => new { item.GCP_GS_NKSender, SendRatio = item.GCP_SendRatio / totalPoolRatio }).ToDictionary(arg => arg.GCP_GS_NKSender, arg => arg.SendRatio);

			return new OpportunityCreationAssignmentPoolData(poolRatioDictionary, pool);
		}
	}

	class OpportunityCreationAssignmentPoolData
	{
		public OpportunityCreationAssignmentPoolData(IDictionary<ZString, int> currentPoolRatio, IReadOnlyDictionary<ZString, double> normalizedRatio)
		{
			CurrentPoolRatio = currentPoolRatio;
			NormalizedRatio = normalizedRatio;
		}

		public IDictionary<ZString, int> CurrentPoolRatio { get; }

		public IReadOnlyDictionary<ZString, double> NormalizedRatio { get; }
	}
}
