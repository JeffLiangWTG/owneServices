using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public class OpportunityCreationChartUserControlTest : TestCaseWithFactory
	{
		public void TestLoadData()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var org1 = Factory.NewWithValidTestData<OrgOpportunity>();
			org1.P8_Status = "WON";
			org1.P8_G0 = campaign.PK;

			var org2 = Factory.NewWithValidTestData<OrgOpportunity>();
			org2.P8_Status = "WON";
			org2.P8_G0 = campaign.PK;

			var org3 = Factory.NewWithValidTestData<OrgOpportunity>();
			org3.P8_Status = "CRT";
			org3.P8_G0 = campaign.PK;

			var org4 = Factory.NewWithValidTestData<OrgOpportunity>();
			org4.P8_Status = "LOS";
			org4.P8_G0 = campaign.PK;

			var org5 = Factory.NewWithValidTestData<OrgOpportunity>();
			org5.P8_Status = "WON";
			org5.P8_G0 = campaign.PK;

			var org6 = Factory.NewWithValidTestData<OrgOpportunity>();
			org6.P8_Status = "ABA";
			org6.P8_G0 = campaign.PK;

			var org7 = Factory.NewWithValidTestData<OrgOpportunity>();
			org7.P8_Status = "WON";
			org7.P8_G0 = campaign.PK;

			var org8 = Factory.NewWithValidTestData<OrgOpportunity>();
			org8.P8_Status = "SUS";
			org8.P8_G0 = campaign.PK;

			Factory.Save();

			using (var opportunityCreationChartUserControl = new OpportunityCreationChartUserControl())
			{
				opportunityCreationChartUserControl.SetDataContext(campaign);

				AssertEquals(opportunityCreationChartUserControl.CurrentOpportunitiesTotal.Text, "1");
				AssertEquals(opportunityCreationChartUserControl.WonOpportunitiesTotal.Text, "4");
				AssertEquals(opportunityCreationChartUserControl.OtherOpportunitiesTotal.Text, "2");
				AssertEquals(opportunityCreationChartUserControl.LostOpportunitiesTotal.Text, "1");
				AssertEquals(opportunityCreationChartUserControl.LinkedOpportunitiesTotal.Text, "8");
				AssertEquals(opportunityCreationChartUserControl.WinRatioOpportunitiesTotal.Text, "50%");
				AssertEquals(opportunityCreationChartUserControl.CurrentLabel.BackColor, System.Drawing.Color.Orange);
				AssertEquals(opportunityCreationChartUserControl.WonLabel.BackColor, System.Drawing.Color.Green);
				AssertEquals(opportunityCreationChartUserControl.LostLabel.BackColor, System.Drawing.Color.Red);
				AssertEquals(opportunityCreationChartUserControl.OtherLabel.BackColor, System.Drawing.Color.Black);
			}
		}

		public void TestRefreshOpportunityCreationChartData()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp1.P8_Status = "WON";
			opp1.P8_G0 = campaign.PK;

			var opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp2.P8_Status = "WON";
			opp2.P8_G0 = campaign.PK;
			Factory.Save();

			using (var opportunityCreationChartUserControl = new OpportunityCreationChartUserControl())
			{
				opportunityCreationChartUserControl.SetDataContext(campaign);

				AssertEquals("0", opportunityCreationChartUserControl.CurrentOpportunitiesTotal.Text);
				AssertEquals("2", opportunityCreationChartUserControl.WonOpportunitiesTotal.Text);
				AssertEquals("0", opportunityCreationChartUserControl.LostOpportunitiesTotal.Text);
				AssertEquals("0", opportunityCreationChartUserControl.OtherOpportunitiesTotal.Text);
				AssertEquals("100%", opportunityCreationChartUserControl.WinRatioOpportunitiesTotal.Text);

				var opp3 = Factory.NewWithValidTestData<OrgOpportunity>();
				opp3.P8_Status = "CRT";
				opp3.P8_G0 = campaign.PK;

				var opp4 = Factory.NewWithValidTestData<OrgOpportunity>();
				opp4.P8_Status = "LOS";
				opp4.P8_G0 = campaign.PK;

				var opp5 = Factory.NewWithValidTestData<OrgOpportunity>();
				opp5.P8_Status = "WON";
				opp5.P8_G0 = campaign.PK;

				var opp6 = Factory.NewWithValidTestData<OrgOpportunity>();
				opp6.P8_Status = "ABA";
				opp6.P8_G0 = campaign.PK;
				Factory.Save();

				opportunityCreationChartUserControl.RefreshOpportunityCreationChart();

				AssertEquals("1", opportunityCreationChartUserControl.CurrentOpportunitiesTotal.Text);
				AssertEquals("3", opportunityCreationChartUserControl.WonOpportunitiesTotal.Text);
				AssertEquals("1", opportunityCreationChartUserControl.LostOpportunitiesTotal.Text);
				AssertEquals("1", opportunityCreationChartUserControl.OtherOpportunitiesTotal.Text);
				AssertEquals("50%", opportunityCreationChartUserControl.WinRatioOpportunitiesTotal.Text);
			}
		}
	}
}
