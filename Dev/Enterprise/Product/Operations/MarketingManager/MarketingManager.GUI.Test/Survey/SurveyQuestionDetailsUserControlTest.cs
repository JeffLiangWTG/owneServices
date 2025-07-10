using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	class SurveyQuestionDetailsUserControlTest : ZFormBasherTest
	{
		public void TestControlsVisibility()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			using (var form = GetTestForm(campaign))
			{
				form.Show();

				var userControl = (SurveyQuestionDetailsUserControl)form.Controls.Find("SurveyQuestionDetailsUserControl", true)[0];
				var minCalcEdit = form.Controls.Find("MinCalcEdit", true)[0];
				var optionsBox = (ZGroupBox)form.Controls.Find("OptionsBox", true).FirstOrDefault();

				var question1 = campaign.Questions.AddNew();
				question1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
				var question2 = campaign.Questions.AddNew();
				question2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.NumericScale;

				var manager = (CurrencyManager)userControl.BindingContext[campaign, "Questions"];
				manager.Position = 0;
				Assert(!minCalcEdit.Visible);
				optionsBox = (ZGroupBox)form.Controls.Find("OptionsBox", true).FirstOrDefault();
				AssertEquals(true, optionsBox.Visible);

				manager.Position = 1;
				optionsBox = (ZGroupBox)form.Controls.Find("OptionsBox", true).FirstOrDefault();
				Assert(minCalcEdit.Visible);
				AssertEquals(false, optionsBox.Visible);

				question2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.LikertScale;
				optionsBox = (ZGroupBox)form.Controls.Find("OptionsBox", true).FirstOrDefault();
				Assert(!minCalcEdit.Visible);
				AssertEquals(false, optionsBox.Visible);
			}
		}

		protected override Form GetFormToBashCore()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			return GetTestForm(campaign);
		}

		ZForm GetTestForm(GlbCompanyCampaign campaign)
		{
			var result = new ZEmptyFormForBasherTest();
			result.CaptionRenderingEnabled = true;
			var userControl = new SurveyQuestionDetailsUserControl();
			userControl.Name = "SurveyQuestionDetailsUserControl";
			userControl.Dock = DockStyle.Fill;
			result.Controls.Add(userControl);
			userControl.SetDataBinding(campaign, "Questions");
			result.Size = ControlDpiScalingHelper.NewScaledSize(1000, 700);
			return result;
		}
	}
}
