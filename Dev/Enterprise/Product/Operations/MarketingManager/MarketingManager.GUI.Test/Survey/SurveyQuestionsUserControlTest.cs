using System.Windows.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	class SurveyQuestionsUserControlTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			return GetTestForm(campaign);
		}

		ZForm GetTestForm(GlbCompanyCampaign campaign)
		{
			ZEmptyFormForBasherTest result = new ZEmptyFormForBasherTest();
			result.CaptionRenderingEnabled = true;
			SurveyQuestionsUserControl userControl = new SurveyQuestionsUserControl();
			userControl.Name = "SurveyQuestionsUserControl";
			userControl.Dock = DockStyle.Fill;
			result.Controls.Add(userControl);
			userControl.SetDataBinding(campaign, null);
			result.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 700);
			return result;
		}
	}
}
