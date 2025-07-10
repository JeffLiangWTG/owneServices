using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.GUI.Testing
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	class ExamResultsByQuestionUserControlTest : ZFormBasherTest
	{
		public void TestResultItemAnswersGrid_ColourDecidingEvent()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			campaign.G0_DefaultAnswerType = VoteExamSurveyAnswerTypeList.Codes.TrueFalse;
			LearningCentreQuestion question1 = campaign.Questions.AddNew();
			question1.HY_ExamCorrectAnswer = "1";
			LearningCentreQuestion question2 = campaign.Questions.AddNew();
			question2.HY_ExamCorrectAnswer = "1";
			LearningCentreCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			var examAnswerSet = new LearningCentreVoteExamSurveyAnswerSet(Factory, campaignItem);
			examAnswerSet.PersistedAnswers.LoadOrCreateNew(question1, campaignItem).HZ_Answer = "1"; //correct
			LearningCentreCampaignItem campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			examAnswerSet = new LearningCentreVoteExamSurveyAnswerSet(Factory, campaignItem2);
			examAnswerSet.PersistedAnswers.LoadOrCreateNew(question1, campaignItem2).HZ_Answer = "2"; //incorrect
			using (ZForm form = GetTestForm(campaign))
			{
				form.Show();
				ExamResultsByQuestionUserControl userControl = (ExamResultsByQuestionUserControl)form.Controls.Find("ExamResultsByQuestionUserControl", true)[0];
				ZGrid grid = ((IExamResultsControl)userControl).AnswersGrid;
				MethodInfo getColourMethod = typeof(ZGrid).GetMethod("GetCustomRowBackgroundColour", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(int) }, null);
				AssertEquals(Color.LightGreen, getColourMethod.Invoke(grid, new object[] { 0 }));
				AssertEquals(Color.LightPink, getColourMethod.Invoke(grid, new object[] { 1 }));
			}
		}

		protected override Form GetFormToBashCore()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			return GetTestForm(campaign);
		}

		ZEmptyFormForBasherTest GetTestForm(LearningCentreCampaign campaign)
		{
			ZEmptyFormForBasherTest result = new ZEmptyFormForBasherTest();
			result.CaptionRenderingEnabled = true;
			ExamResultsByQuestionUserControl userControl = new ExamResultsByQuestionUserControl();
			userControl.Name = "ExamResultsByQuestionUserControl";
			userControl.Dock = DockStyle.Fill;
			result.Controls.Add(userControl);
			userControl.SetDataBinding(campaign, null);
			result.Size = new Size(1000, 700);
			return result;
		}
	}
}
