namespace Enterprise.Recruiter.Business.Testing
{
	sealed class LegacyLearningCentreCampaignItemCollection : LearningCentreCampaignItemCollection
	{
		public LegacyLearningCentreCampaignItemCollection(LegacyLearningCentreCampaign campaign)
		  : base(campaign)
		{
		}

		public new LegacyLearningCentreCampaignItem this[int index]
		{
			get { return (LegacyLearningCentreCampaignItem)Elements[index]; }
		}

		public new LegacyLearningCentreCampaignItem AddNew()
		{
			return (LegacyLearningCentreCampaignItem)base.AddNew();
		}
	}
}
