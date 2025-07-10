using System.Drawing;
using System.Windows.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.GUI.Testing
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	class ExamQuestionDetailsUserControlTest : ZFormBasherTest
	{
		public void TestControlsVisibility()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			using (ZForm form = GetTestForm(campaign))
			{
				form.Show();
				ExamQuestionDetailsUserControl userControl = (ExamQuestionDetailsUserControl)form.Controls.Find("ExamQuestionDetailsUserControl", true)[0];
				Control optionalCheckBox = form.Controls.Find("OptionalCheckBox", true)[0];
				Control correctAnswerCalcEdit = form.Controls.Find("CorrectAnswerCalcEdit", true)[0];
				Control correctAnswerDropEdit = form.Controls.Find("CorrectAnswerDropEdit", true)[0];
				VoteExamSurveyQuestion question1 = campaign.Questions.AddNew();
				question1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
				VoteExamSurveyQuestion question2 = campaign.Questions.AddNew();
				question2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.YesNo;
				VoteExamSurveyQuestion question3 = campaign.Questions.AddNew();
				question3.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.NumericScale;
				CurrencyManager manager = (CurrencyManager)userControl.BindingContext[campaign, "Questions"];
				manager.Position = 0;
				Assert(!optionalCheckBox.Visible);
				Assert(!correctAnswerCalcEdit.Visible);
				Assert(!correctAnswerDropEdit.Visible);
				manager.Position = 1;
				Assert(!optionalCheckBox.Visible);
				Assert(!correctAnswerCalcEdit.Visible);
				Assert(correctAnswerDropEdit.Visible);
				manager.Position = 2;
				Assert(!optionalCheckBox.Visible);
				Assert(correctAnswerCalcEdit.Visible);
				Assert(!correctAnswerDropEdit.Visible);
				question3.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.TrueFalse;
				Assert(!optionalCheckBox.Visible);
				Assert(!correctAnswerCalcEdit.Visible);
				Assert(correctAnswerDropEdit.Visible);
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
			ExamQuestionDetailsUserControl userControl = new ExamQuestionDetailsUserControl();
			userControl.Name = "ExamQuestionDetailsUserControl";
			userControl.Dock = DockStyle.Fill;
			result.Controls.Add(userControl);
			userControl.SetDataBinding(campaign, "Questions");
			result.Size = new Size(1200, 700);
			return result;
		}
	}
}
