using Enterprise.MarketingManager.Business;

namespace Enterprise.Recruiter.Business
{
	public class LearningCentreCampaignItemCollection : GlbCompanyCampaignItemCampaignDependentCollection
	{
		public LearningCentreCampaignItemCollection(LearningCentreCampaign campaign)
			: base(campaign)
		{
		}

		public new LearningCentreCampaignItem this[int index]
		{
			get { return (LearningCentreCampaignItem)Elements[index]; }
		}

		public new LearningCentreCampaignItem AddNew()
		{
			return (LearningCentreCampaignItem)base.AddNew();
		}
	}
}
