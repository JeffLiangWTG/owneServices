using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignProcessTaskLoadStrategy : IProcessTaskLoadStrategy
	{
		public Type GetTypeForLoad(string parentTablePrefix, ZGuid parentID, BusinessObjectFactory factory)
		{
			var campaign = factory.Load<GlbCompanyCampaign>(parentID);

			return campaign is IHRGlbCompanyCampaign ? ObjectFactory.GetType<IHRCampaignProcessTasks>() : typeof(CRMCampaignProcessTasks);
		}

		public void AddAdditionalParentFilters(WorkflowDescriptor workflowDescriptor, ZDBOnlySubQuery subQuery)
		{
			var workflowProviderType = workflowDescriptor.WorkflowProviderType;

			subQuery.AddToFilter(GlbCompanyCampaignSchema.G0_IsSalesAndMarketing,
				!ObjectFactory.GetType<IHRGlbCompanyCampaign>()
					.IsAssignableFrom(workflowProviderType));
		}
	}
}
