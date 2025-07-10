using System.Windows.Forms;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.GUI.Testing
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	class ExamQuestionsUserControlTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			return GetTestForm(campaign);
		}

		ZEmptyFormForBasherTest GetTestForm(LearningCentreCampaign campaign)
		{
			ZEmptyFormForBasherTest result = new ZEmptyFormForBasherTest();
			result.CaptionRenderingEnabled = true;
			ExamQuestionsUserControl userControl = new ExamQuestionsUserControl();
			userControl.Name = "ExamQuestionsUserControl";
			userControl.Dock = DockStyle.Fill;
			result.Controls.Add(userControl);
			userControl.SetDataBinding(campaign, null);
			result.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 700);
			return result;
		}
	}
}
