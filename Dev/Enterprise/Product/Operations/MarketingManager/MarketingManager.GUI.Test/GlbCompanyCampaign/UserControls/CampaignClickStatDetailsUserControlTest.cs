using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	public class CampaignClickStatDetailsUserControlTest : ZFormBasherTest
	{
		public void TestSummaryControlCollapsed()
		{
			using (var control = new CampaignClickStatDetailsUserControlForTest())
			{
				Assert(!control.SummaryControlCollapsed);
				Assert(!control.InnerSplitContainer_Exposed.Panel2Collapsed);

				control.SummaryControlCollapsed = true;
				Assert(control.SummaryControlCollapsed);
				Assert(control.InnerSplitContainer_Exposed.Panel2Collapsed);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			var result = new ZEmptyFormForBasherTest();
			result.CaptionRenderingEnabled = true;
			var userControl = new CampaignClickStatDetailsUserControl();
			userControl.Dock = DockStyle.Fill;
			result.Controls.Add(userControl);
			userControl.SetDataBinding(campaign.StatModel, null);
			result.Size = ControlDpiScalingHelper.NewScaledSize(1000, 700);
			return result;
		}

		class CampaignClickStatDetailsUserControlForTest : CampaignClickStatDetailsUserControl
		{
			public KSplitContainer InnerSplitContainer_Exposed
			{
				get { return base.innerSplitContainer; }
			}
		}
	}
}
