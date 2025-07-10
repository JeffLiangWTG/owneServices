using System.Drawing;
using System.Windows.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	public class TransitionProgressChartUserControlTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			var result = new ZEmptyFormForBasherTest();
			result.CaptionRenderingEnabled = true;
			var userControl = new TransitionProgressChartUserControl();
			userControl.Dock = DockStyle.Fill;
			result.Controls.Add(userControl);
			userControl.SetDataContext(campaign);
			result.Size = new Size(1000, 700);
			return result;
		}
	}
}
