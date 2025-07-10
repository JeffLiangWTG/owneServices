using System;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.GUI;
using Enterprise.MarketingManager.GUI.Testing;
using Enterprise.Recruiter.Business;

namespace Enterprise.Recruiter.GUI.Testing
{
	public class LearningCentreExamPlugInTest : VoteExamSurveyPlugInTestCase
	{
		protected override Type ExpectedQuestionsControlType
		{
			get
			{
				return typeof(ExamQuestionsUserControl);
			}
		}

		protected override Type ExpectedQuestionDetailsControlType
		{
			get
			{
				return typeof(ExamQuestionDetailsUserControl);
			}
		}

		protected override Type ExpectedResultsByQuestionControlType
		{
			get
			{
				return typeof(ExamResultsByQuestionUserControl);
			}
		}

		protected override Type ExpectedResultsByRecipientControlType
		{
			get
			{
				return typeof(SurveyResultsByRecipientUserControl);
			}
		}

		protected override VoteExamSurveyPlugIn GetPlugIn()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			return new LearningCentreExamPlugIn(campaign);
		}

		protected override void TogglePlugInEnabled(bool enabled, GlbCompanyCampaign campaign)
		{
			campaign.G0_Type = (enabled) ? LearningCentreTestTypes.Codes.Exam : LearningCentreTestTypes.Codes.Scaled;
		}

		protected override Type ExpectedResultsSummaryControlType
		{
			get
			{
				return typeof(ExamSummaryUserControl);
			}
		}
	}
}
