using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.GUI
{
	public class VoteCampaignPlugIn : VoteExamSurveyPlugIn
	{
		public VoteCampaignPlugIn(GlbCompanyCampaign campaign)
			: base(campaign)
		{
		}

		public override string Name
		{
			get { return "VoteCampaign"; }
		}

		protected override string SupportedCampaignType
		{
			get { return CampaignTypeList.Codes.Voting; }
		}

		protected override QuestionsUserControl GetNewQuestionsControl()
		{
			return new VoteQuestionsUserControl();
		}

		protected override ResultsByQuestionUserControl GetNewResultsByQuestionControl()
		{
			return new VoteResultsByQuestionUserControl();
		}

		protected override ResultsByRecipientUserControl GetNewResultsByRecipientControl()
		{
			return new VoteResultsByRecipientUserControl(BusinessEntity);
		}
	}
}
