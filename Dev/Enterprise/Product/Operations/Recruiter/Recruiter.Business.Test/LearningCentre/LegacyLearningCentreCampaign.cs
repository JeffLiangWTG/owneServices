using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class LegacyLearningCentreCampaign : LearningCentreCampaign
	{
		public LegacyLearningCentreCampaign(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			factory.AllowMultipleBusinessObjectsAroundOneRow = false;
		}

		protected override GlbCompanyCampaignItemCampaignDependentCollection GetNewCampaignItemCollection()
		{
			return new LegacyLearningCentreCampaignItemCollection(this);
		}

		public override Type TypeOfSentItem => typeof(LegacyLearningCentreCampaignItem);

		public new LegacyLearningCentreCampaignItemCollection CampaignsItemsSent
		{
			get { return (LegacyLearningCentreCampaignItemCollection)base.CampaignsItemsSent; }
		}
	}
}
