using System;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.GUI;
using Enterprise.MarketingManager.GUI.Testing;
using Enterprise.Recruiter.Business;

namespace Enterprise.Recruiter.GUI.Testing
{
	public class LearningCentreScaledTestPlugInTest : VoteExamSurveyPlugInTestCase
	{
		protected override Type ExpectedQuestionsControlType
		{
			get
			{
				return typeof(ScaledTestQuestionsUserControl);
			}
		}

		protected override Type ExpectedQuestionDetailsControlType
		{
			get
			{
				return typeof(ScaledTestQuestionDetailsUserControl);
			}
		}

		protected override Type ExpectedResultsControlOverrideType
		{
			get
			{
				return typeof(ScaledTestResultsUserControl);
			}
		}

		protected override Type ExpectedResultsByQuestionControlType
		{
			get
			{
				return null;
			}
		}

		protected override Type ExpectedResultsByRecipientControlType
		{
			get
			{
				return null;
			}
		}

		protected override Type ExpectedResultsSummaryControlType
		{
			get
			{
				return null;
			}
		}

		protected override VoteExamSurveyPlugIn GetPlugIn()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			return new LearningCentreScaledTestPlugIn(campaign);
		}

		protected override void TogglePlugInEnabled(bool enabled, GlbCompanyCampaign campaign)
		{
			campaign.G0_Type = (enabled) ? LearningCentreTestTypes.Codes.Scaled : LearningCentreTestTypes.Codes.Exam;
		}
	}
}
