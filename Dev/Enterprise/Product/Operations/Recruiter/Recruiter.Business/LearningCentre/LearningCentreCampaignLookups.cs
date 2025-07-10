using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Recruiter.Business
{
	public class LearningCentreCampaignLookups : GlbCompanyCampaignLookups
	{
		public LearningCentreCampaignLookups(LearningCentreCampaign parent)
			: base(parent)
		{
		}

		protected override CampaignTypeList GetNewCampaignTypeList()
		{
			CampaignTypeList result = base.GetNewCampaignTypeList();
			result.AddPair(Core.Constants.Recruiter.LearningCentreCampaignType);
			return result;
		}

		public override VoteExamSurveyAnswerTypeList DefaultAnswerTypes
		{
			get { return new LearningCentreAnswerTypeList((LearningCentreCampaign)Parent); }
		}

		public LearningCentreTestTypes TestTypes
		{
			get { return new LearningCentreTestTypes(); }
		}
	}
}
