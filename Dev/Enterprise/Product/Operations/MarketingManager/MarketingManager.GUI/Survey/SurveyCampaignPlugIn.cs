using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.GUI
{
	public class SurveyCampaignPlugIn : VoteExamSurveyPlugIn
	{
		public SurveyCampaignPlugIn(GlbCompanyCampaign campaign)
			: base(campaign)
		{
		}

		public override string Name
		{
			get { return "SurveyCampaign"; }
		}

		protected override string SupportedCampaignType
		{
			get { return CampaignTypeList.Codes.Survey; }
		}

		protected override QuestionsUserControl GetNewQuestionsControl()
		{
			return new SurveyQuestionsUserControl();
		}

		protected override QuestionDetailsUserControl GetNewQuestionDetailsControl()
		{
			return new SurveyQuestionDetailsUserControl();
		}

		protected override ResultsByQuestionUserControl GetNewResultsByQuestionControl()
		{
			return new SurveyResultsByQuestionUserControl();
		}

		protected override ResultsByRecipientUserControl GetNewResultsByRecipientControl()
		{
			return new SurveyResultsByRecipientUserControl(BusinessEntity);
		}
	}
}
