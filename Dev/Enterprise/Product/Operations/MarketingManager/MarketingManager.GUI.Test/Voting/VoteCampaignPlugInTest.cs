using System;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public class VoteCampaignPlugInTest : VoteExamSurveyPlugInTestCase
	{
		protected override Type ExpectedQuestionsControlType
		{
			get { return typeof(VoteQuestionsUserControl); }
		}

		protected override Type ExpectedResultsByQuestionControlType
		{
			get { return typeof(VoteResultsByQuestionUserControl); }
		}

		protected override Type ExpectedResultsByRecipientControlType
		{
			get { return typeof(VoteResultsByRecipientUserControl); }
		}

		protected override VoteExamSurveyPlugIn GetPlugIn()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			return new VoteCampaignPlugIn(campaign);
		}

		protected override void TogglePlugInEnabled(bool enabled, GlbCompanyCampaign campaign)
		{
			campaign.G0_BroadcastVoteSurveyExam = (enabled) ? CampaignTypeList.Codes.Voting : CampaignTypeList.Codes.Broadcast;
		}
	}
}
