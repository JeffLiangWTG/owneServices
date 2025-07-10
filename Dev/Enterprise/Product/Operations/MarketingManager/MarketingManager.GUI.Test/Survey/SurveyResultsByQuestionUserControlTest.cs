using System.Drawing;
using System.Windows.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	class SurveyResultsByQuestionUserControlTest : ZFormBasherTest
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
			SurveyResultsByQuestionUserControl userControl = new SurveyResultsByQuestionUserControl();
			userControl.Name = "SurveyResultsByQuestionUserControl";
			userControl.Dock = DockStyle.Fill;
			result.Controls.Add(userControl);
			userControl.SetDataBinding(campaign, null);
			result.Size = new Size(1000, 700);
			return result;
		}
	}
}
