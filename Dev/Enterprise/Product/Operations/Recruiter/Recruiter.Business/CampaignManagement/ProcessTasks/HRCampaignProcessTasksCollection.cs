using Enterprise.MasterFiles.Business;

namespace Enterprise.Recruiter.Business
{
	public class HRCampaignProcessTasksCollection : ProcessTaskCollection
	{
		public HRCampaignProcessTasksCollection(HRGlbCompanyCampaign campaign)
			: base(campaign)
		{
		}

		public new HRCampaignProcessTasks this[int index]
		{
			get { return (HRCampaignProcessTasks)Elements[index]; }
		}

		public new HRCampaignProcessTasks AddNew()
		{
			return (HRCampaignProcessTasks)base.AddNew();
		}
	}
}
