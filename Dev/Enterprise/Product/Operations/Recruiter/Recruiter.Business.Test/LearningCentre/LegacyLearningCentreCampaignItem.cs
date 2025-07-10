using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class LegacyLearningCentreCampaignItem : LearningCentreCampaignItem
	{
		public LegacyLearningCentreCampaignItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
