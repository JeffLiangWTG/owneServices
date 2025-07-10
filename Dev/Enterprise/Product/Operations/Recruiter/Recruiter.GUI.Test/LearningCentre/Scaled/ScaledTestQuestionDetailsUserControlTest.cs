using System.Drawing;
using System.Windows.Forms;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.GUI.Testing
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	class ScaledTestQuestionDetailsUserControlTest : ZFormBasherTest
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
			ScaledTestQuestionDetailsUserControl userControl = new ScaledTestQuestionDetailsUserControl();
			userControl.Name = "ScaledTestQuestionDetailsUserControl";
			userControl.Dock = DockStyle.Fill;
			result.Controls.Add(userControl);
			userControl.SetDataBinding(campaign, "Questions");
			result.Size = new Size(1000, 700);
			return result;
		}
	}
}
