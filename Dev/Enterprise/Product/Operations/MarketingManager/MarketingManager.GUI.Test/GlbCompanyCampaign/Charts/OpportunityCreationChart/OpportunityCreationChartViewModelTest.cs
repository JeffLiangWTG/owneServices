using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using OxyPlot;
using OxyPlot.Series;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public class OpportunityCreationChartViewModelTest : TestCaseWithFactory
	{
		public void TestPiechartSlicesVisibility()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp1.P8_Status = "WON";
			opp1.P8_G0 = campaign.PK;

			var opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp2.P8_Status = "CRT";
			opp2.P8_G0 = campaign.PK;

			var opp3 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp3.P8_Status = "ABA";
			opp3.P8_G0 = campaign.PK;

			campaign.RelatedChildActivityPivotCollection.AddNewPivot(opp1);
			campaign.RelatedChildActivityPivotCollection.AddNewPivot(opp2);
			campaign.RelatedChildActivityPivotCollection.AddNewPivot(opp3);
			Factory.Save();

			using (var statModel = new OpportunityCreationChartViewModel(campaign))
			{
				statModel.PopulatePieModel();

				var pieSeries = (PieSeries)statModel.PieModel.Series[0];
				AssertEquals(pieSeries.Slices.Count, 3);
				AssertEquals(statModel.LostCount, 0);

				CheckValueAndColor(pieSeries.Slices[0], 1.0, OxyColors.Orange);
				CheckValueAndColor(pieSeries.Slices[1], 1.0, OxyColors.Green);
				CheckValueAndColor(pieSeries.Slices[2], 1.0, OxyColors.Black);
			}
		}

		public void TestLoadData()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var opp1 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp1.P8_Status = "WON";
			opp1.P8_G0 = campaign.PK;

			var opp2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opp2.P8_Status = "WON";
			opp2.P8_G0 = campaign.PK;

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

			campaign.RelatedChildActivityPivotCollection.AddNewPivot(opp1);
			campaign.RelatedChildActivityPivotCollection.AddNewPivot(opp2);
			campaign.RelatedChildActivityPivotCollection.AddNewPivot(opp3);
			campaign.RelatedChildActivityPivotCollection.AddNewPivot(opp4);
			campaign.RelatedChildActivityPivotCollection.AddNewPivot(opp5);
			campaign.RelatedChildActivityPivotCollection.AddNewPivot(opp6);
			Factory.Save();

			using (var statModel = new OpportunityCreationChartViewModel(campaign))
			{
				statModel.PopulatePieModel();

				AssertEquals(statModel.CurrentCount, 1);
				AssertEquals(statModel.WonCount, 3);
				AssertEquals(statModel.OtherCount, 1);
				AssertEquals(statModel.LostCount, 1);
				AssertEquals(statModel.TotalCount, 6);
				AssertEquals(statModel.WinRatio, 0.5m);

				var pieSeries = (PieSeries)statModel.PieModel.Series[0];
				AssertEquals(pieSeries.Slices.Count, 4);
			}
		}

		public void TestLoadDataWithoutOpportunities()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			Factory.Save();

			using (var statModel = new OpportunityCreationChartViewModel(campaign))
			{
				statModel.PopulatePieModel();

				AssertEquals(statModel.CurrentCount, 0);
				AssertEquals(statModel.WonCount, 0);
				AssertEquals(statModel.OtherCount, 0);
				AssertEquals(statModel.LostCount, 0);
				AssertEquals(statModel.TotalCount, 0);
				AssertEquals(statModel.WinRatio, 0m);

				var pieSeries = (PieSeries)statModel.PieModel.Series[0];
				AssertEquals(pieSeries.Slices.Count, 1);
			}
		}

		void CheckValueAndColor(PieSlice slice, double expectedCount, OxyColor expectedColor)
		{
			AssertEquals(expectedCount, slice.Value);
			AssertEquals(expectedColor, slice.ActualFillColor);
		}
	}
}
