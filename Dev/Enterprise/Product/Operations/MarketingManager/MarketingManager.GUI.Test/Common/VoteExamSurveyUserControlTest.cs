using System.Windows.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	class VoteExamSurveyUserControlTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			var result = new ZEmptyFormForBasherTest();
			result.CaptionRenderingEnabled = true;
			var userControl = new VoteExamSurveyUserControl();
			userControl.QuestionsControl = new QuestionsUserControl();
			userControl.QuestionDetailsControl = new QuestionDetailsUserControl();
			userControl.ResultsByQuestionControl = new ResultsByQuestionUserControl();
			userControl.ResultsByRecipientControl = new ResultsByRecipientUserControl(campaign);
			userControl.ResultsSummaryControl = new ResultsSummaryUserControl();
			userControl.Dock = DockStyle.Fill;
			result.Controls.Add(userControl);
			userControl.SetDataBinding(campaign, null);
			result.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 700);
			return result;
		}
	}
}
