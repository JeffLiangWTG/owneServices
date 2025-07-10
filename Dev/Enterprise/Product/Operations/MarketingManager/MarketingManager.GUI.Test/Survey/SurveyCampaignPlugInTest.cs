using System;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public class SurveyCampaignPlugInTest : VoteExamSurveyPlugInTestCase
	{
		protected override Type ExpectedQuestionsControlType
		{
			get { return typeof(SurveyQuestionsUserControl); }
		}

		protected override Type ExpectedQuestionDetailsControlType
		{
			get { return typeof(SurveyQuestionDetailsUserControl); }
		}

		protected override Type ExpectedResultsByQuestionControlType
		{
			get { return typeof(SurveyResultsByQuestionUserControl); }
		}

		protected override Type ExpectedResultsByRecipientControlType
		{
			get { return typeof(SurveyResultsByRecipientUserControl); }
		}

		protected override VoteExamSurveyPlugIn GetPlugIn()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			return new SurveyCampaignPlugIn(campaign);
		}

		protected override void TogglePlugInEnabled(bool enabled, GlbCompanyCampaign campaign)
		{
			campaign.G0_BroadcastVoteSurveyExam = (enabled) ? CampaignTypeList.Codes.Survey : CampaignTypeList.Codes.Broadcast;
		}
	}
}
