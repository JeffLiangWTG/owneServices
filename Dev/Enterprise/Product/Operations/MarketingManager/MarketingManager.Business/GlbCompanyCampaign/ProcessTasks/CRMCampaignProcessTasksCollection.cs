using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business
{
	public class CRMCampaignProcessTasksCollection : ProcessTaskCollection
	{
		public CRMCampaignProcessTasksCollection(GlbCompanyCampaign campaign)
			: base(campaign)
		{
		}

		public new CRMCampaignProcessTasks this[int index]
		{
			get { return (CRMCampaignProcessTasks)Elements[index]; }
		}

		public new CRMCampaignProcessTasks AddNew()
		{
			return (CRMCampaignProcessTasks)base.AddNew();
		}
	}
}
