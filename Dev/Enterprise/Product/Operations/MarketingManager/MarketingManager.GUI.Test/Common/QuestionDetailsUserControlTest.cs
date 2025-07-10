using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	class QuestionDetailsUserControlTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			ZEmptyFormForBasherTest result = new ZEmptyFormForBasherTest();
			result.CaptionRenderingEnabled = true;
			QuestionDetailsUserControl userControl = new QuestionDetailsUserControl();
			userControl.Dock = DockStyle.Fill;
			result.Controls.Add(userControl);
			userControl.SetDataBinding(campaign, "Questions");
			result.Size = new Size(1000, 700);
			return result;
		}

		public void TestOptionsBoxShouldBeInvisibleByDefault()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				AssertEquals(false, form.Controls.Find("OptionsBox", true).Single().Visible);
			}
		}
	}
}
