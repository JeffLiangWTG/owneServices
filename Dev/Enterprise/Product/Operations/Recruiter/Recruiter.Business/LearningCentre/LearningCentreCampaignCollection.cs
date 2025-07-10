using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	[ModuleID(ModuleId.LearningCentreCampaign)]
	public class LearningCentreCampaignCollection : ActiveBusinessObjectCollection<LearningCentreCampaign>,
		Enterprise.Integration.Recruiter.ILearningCentreCampaignCollection
	{
		public LearningCentreCampaignCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		public LearningCentreCampaignCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery query = base.CreateRelationshipFilter();
			query.AddToFilter(GlbCompanyCampaignSchema.G0_BroadcastVoteSurveyExam, Core.Constants.Recruiter.LearningCentreCampaignType);
			return query;
		}
	}
}
