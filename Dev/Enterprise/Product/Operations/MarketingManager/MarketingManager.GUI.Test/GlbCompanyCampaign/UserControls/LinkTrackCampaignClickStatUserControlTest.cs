using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	public class LinkTrackCampaignClickStatUserControlTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			var result = new ZEmptyFormForBasherTest();
			result.CaptionRenderingEnabled = true;
			var userControl = new LinkTrackCampaignClickStatUserControl();
			userControl.Dock = DockStyle.Fill;
			result.Controls.Add(userControl);
			userControl.SetDataBinding(campaign.StatModel, null);
			result.Size = ControlDpiScalingHelper.NewScaledSize(1000, 700);
			return result;
		}

		public void TestRearrangeColumnsByReportBy()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			var statModel = new ClickStatModel(campaign);

			using (ZForm form = new ZForm(campaign))
			using (LinkTrackCampaignClickStatUserControlForTest control = new LinkTrackCampaignClickStatUserControlForTest())
			{
				control.SetDataBinding(statModel, null);
				form.Controls.Add(control);
				form.Show();

				statModel.ReportBy = ReportByList.Codes.Context;
				Assert("Destination URL column should be visible", control.LinksGridExposed.Columns[ClickStatData.Schema.URL].IsVisible);

				statModel.ReportBy = ReportByList.Codes.Url;
				Assert("Destination URL column should be visible", control.LinksGridExposed.Columns[ClickStatData.Schema.URL].IsVisible);
			}
		}

		#region Implementation

		class LinkTrackCampaignClickStatUserControlForTest : LinkTrackCampaignClickStatUserControl
		{
			public LinkTrackCampaignClickStatUserControlForTest()
				: base()
			{
			}

			public ZGrid LinksGridExposed { get { return base.LinksGrid; } }
			public KSplitContainer ChartContainerExposed { get { return base.chartContainer; } }
		}

		#endregion
	}
}
