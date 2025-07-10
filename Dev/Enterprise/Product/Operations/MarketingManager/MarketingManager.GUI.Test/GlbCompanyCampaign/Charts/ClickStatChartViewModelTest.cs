using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using OxyPlot.Series;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public class ClickStatChartViewModelTest : TestCaseWithFactory
	{
		public void TestLoadDataAndBuildChartModel()
		{
			var dataModel = PopulateDataModel();
			dataModel.ReportBy = ReportByList.Codes.Context;

			var viewModel = new ClickStatChartViewModel(dataModel);

			AssertEquals(3, viewModel.TotalClicksChartModel.Series.Count);

			var totalSeries1 = viewModel.TotalClicksChartModel.Series[0] as BarSeries;
			AssertEquals("Series title", "CCC", totalSeries1.Title);
			AssertEquals("Series value", (double)14, totalSeries1.Items[0].Value);

			var totalSeries2 = viewModel.TotalClicksChartModel.Series[1] as BarSeries;
			AssertEquals("Series title", "BBB", totalSeries2.Title);
			AssertEquals("Series value", (double)13, totalSeries2.Items[0].Value);

			var totalSeries3 = viewModel.TotalClicksChartModel.Series[2] as BarSeries;
			AssertEquals("Series title", "AAA", totalSeries3.Title);
			AssertEquals("Series value", (double)9, totalSeries3.Items[0].Value);

			AssertEquals(2, viewModel.ClicksPerTimeIntervalChartModel.Series.Count);

			var timeSeries1 = viewModel.ClicksPerTimeIntervalChartModel.Series[0] as BarSeries;
			AssertEquals("First Opened", timeSeries1.Title);
			AssertEquals("Series items", 24, timeSeries1.Items.Count);
			AssertEquals("Series item category index", 0, timeSeries1.Items[0].CategoryIndex);
			AssertEquals("Series item category index", 23, timeSeries1.Items[23].CategoryIndex);
			AssertEquals("Series item value", (double)2, timeSeries1.Items[0].Value);
			AssertEquals("Series item value", (double)1, timeSeries1.Items[7].Value);

			var timeSeries2 = viewModel.ClicksPerTimeIntervalChartModel.Series[1] as BarSeries;
			AssertEquals("Clicks", timeSeries2.Title);
			AssertEquals("Series items", 24, timeSeries2.Items.Count);
			AssertEquals("Series item category index", 0, timeSeries2.Items[0].CategoryIndex);
			AssertEquals("Series item category index", 23, timeSeries2.Items[23].CategoryIndex);
			AssertEquals("Series item value", (double)6, timeSeries2.Items[0].Value);
			AssertEquals("Series item value", (double)9, timeSeries2.Items[7].Value);
			AssertEquals("Series item value", (double)13, timeSeries2.Items[15].Value);
			AssertEquals("Series item value", (double)8, timeSeries2.Items[23].Value);

			dataModel.ReportBy = ReportByList.Codes.Url;
			viewModel = new ClickStatChartViewModel(dataModel);

			AssertEquals(2, viewModel.TotalClicksChartModel.Series.Count);

			totalSeries1 = viewModel.TotalClicksChartModel.Series[0] as BarSeries;
			AssertEquals("Series title", "http://UrlOne.net.au", totalSeries1.Title);
			AssertEquals("Series value", (double)23, totalSeries1.Items[0].Value);

			totalSeries2 = viewModel.TotalClicksChartModel.Series[1] as BarSeries;
			AssertEquals("Series title", "http://UrlTwo.net.au", totalSeries2.Title);
			AssertEquals("Series value", (double)13, totalSeries2.Items[0].Value);

			AssertEquals(2, viewModel.ClicksPerTimeIntervalChartModel.Series.Count);

			timeSeries1 = viewModel.ClicksPerTimeIntervalChartModel.Series[0] as BarSeries;
			AssertEquals("First Opened", timeSeries1.Title);
			AssertEquals("Series items", 24, timeSeries1.Items.Count);
			AssertEquals("Series item category index", 0, timeSeries1.Items[0].CategoryIndex);
			AssertEquals("Series item category index", 23, timeSeries1.Items[23].CategoryIndex);
			AssertEquals("Series item value", (double)2, timeSeries1.Items[0].Value);
			AssertEquals("Series item value", (double)1, timeSeries1.Items[7].Value);

			timeSeries2 = viewModel.ClicksPerTimeIntervalChartModel.Series[1] as BarSeries;
			AssertEquals("Clicks", timeSeries2.Title);
			AssertEquals("Series items", 24, timeSeries2.Items.Count);
			AssertEquals("Series item category index", 0, timeSeries2.Items[0].CategoryIndex);
			AssertEquals("Series item category index", 23, timeSeries2.Items[23].CategoryIndex);
			AssertEquals("Series item value", (double)6, timeSeries2.Items[0].Value);
			AssertEquals("Series item value", (double)9, timeSeries2.Items[7].Value);
			AssertEquals("Series item value", (double)13, timeSeries2.Items[15].Value);
			AssertEquals("Series item value", (double)8, timeSeries2.Items[23].Value);
		}

		public void TestRefreshChartModel()
		{
			var dataModel = PopulateDataModel();
			dataModel.ReportBy = ReportByList.Codes.Context;

			var viewModel = new ClickStatChartViewModel(dataModel);
			AssertEquals(3, viewModel.TotalClicksChartModel.Series.Count);
			AssertEquals(2, viewModel.ClicksPerTimeIntervalChartModel.Series.Count);

			dataModel.LinkClicks[1].ViewInChart = false;
			viewModel.RefreshChartModel();
			AssertEquals(2, viewModel.TotalClicksChartModel.Series.Count);
			AssertEquals(2, viewModel.ClicksPerTimeIntervalChartModel.Series.Count);

			dataModel.LinkClicks[2].ViewInChart = false;
			viewModel.RefreshChartModel();
			AssertEquals(1, viewModel.TotalClicksChartModel.Series.Count);
			AssertEquals(2, viewModel.ClicksPerTimeIntervalChartModel.Series.Count);

			dataModel.ReportBy = ReportByList.Codes.Url;
			dataModel.LinkClicks[0].ViewInChart = true;
			dataModel.LinkClicks[1].ViewInChart = true;

			viewModel = new ClickStatChartViewModel(dataModel);
			AssertEquals(2, viewModel.TotalClicksChartModel.Series.Count);
			AssertEquals(2, viewModel.ClicksPerTimeIntervalChartModel.Series.Count);

			dataModel.LinkClicks[0].ViewInChart = false;
			viewModel.RefreshChartModel();
			AssertEquals(1, viewModel.TotalClicksChartModel.Series.Count);
			AssertEquals(2, viewModel.ClicksPerTimeIntervalChartModel.Series.Count);

			dataModel.LinkClicks[1].ViewInChart = false;
			viewModel.RefreshChartModel();
			AssertEquals(0, viewModel.TotalClicksChartModel.Series.Count);
			AssertEquals(2, viewModel.ClicksPerTimeIntervalChartModel.Series.Count);

			dataModel.LinkClicks[1].ViewInChart = false;
			viewModel.RefreshChartModel();
			AssertEquals(0, viewModel.TotalClicksChartModel.Series.Count);
			AssertEquals(2, viewModel.ClicksPerTimeIntervalChartModel.Series.Count);
		}

		ClickStatModel PopulateDataModel()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var link1 = campaign.TrackedLinks.AddNew();
			link1.GCL_Context = "AAA";
			link1.GCL_URL = "http://UrlOne.net.au";

			var link2 = campaign.TrackedLinks.AddNew();
			link2.GCL_Context = "BBB";
			link2.GCL_URL = "http://UrlTwo.net.au";

			var link3 = campaign.TrackedLinks.AddNew();
			link3.GCL_Context = "CCC";
			link3.GCL_URL = "http://UrlOne.net.au";

			var linkImage = campaign.TrackedLinks.AddNew();
			linkImage.GCL_Context = "banner.png";
			linkImage.GCL_URL = "http://UrlOne.net.au/images/promotion/banner.png";
			linkImage.GCL_IsImage = true;

			var dataModel = new ClickStatModelForTest(campaign);
			dataModel.Link1 = link1;
			dataModel.Link2 = link2;
			dataModel.Link3 = link3;
			dataModel.FromDateTime = ZDateTime.Today;
			dataModel.ToDateTime = dataModel.FromDateTime.AddDays(1);

			return dataModel;
		}

		class ClickStatModelForTest : ClickStatModel
		{
			public ClickStatModelForTest(GlbCompanyCampaign campaign)
				: base(campaign)
			{
			}

			public GlbCompanyCampaignLink Link1 { get; set; }
			public GlbCompanyCampaignLink Link2 { get; set; }
			public GlbCompanyCampaignLink Link3 { get; set; }

			protected override void LoadRawStatData()
			{
				ClicksPerIntervalData.Clear();
				ClicksPerIntervalData.Add(new CampaignClicksPerInterval() { IntervalIndex = 0, LinkPk = Link1.PK, ClickCount = 3 });
				ClicksPerIntervalData.Add(new CampaignClicksPerInterval() { IntervalIndex = 0, LinkPk = Link2.PK, ClickCount = 2 });
				ClicksPerIntervalData.Add(new CampaignClicksPerInterval() { IntervalIndex = 0, LinkPk = Link3.PK, ClickCount = 1 });
				ClicksPerIntervalData.Add(new CampaignClicksPerInterval() { IntervalIndex = 0, LinkPk = ZGuid.Empty, ClickCount = 2 });
				ClicksPerIntervalData.Add(new CampaignClicksPerInterval() { IntervalIndex = 7, LinkPk = Link2.PK, ClickCount = 4 });
				ClicksPerIntervalData.Add(new CampaignClicksPerInterval() { IntervalIndex = 7, LinkPk = Link3.PK, ClickCount = 5 });
				ClicksPerIntervalData.Add(new CampaignClicksPerInterval() { IntervalIndex = 7, LinkPk = ZGuid.Empty, ClickCount = 1 });
				ClicksPerIntervalData.Add(new CampaignClicksPerInterval() { IntervalIndex = 15, LinkPk = Link1.PK, ClickCount = 6 });
				ClicksPerIntervalData.Add(new CampaignClicksPerInterval() { IntervalIndex = 15, LinkPk = Link2.PK, ClickCount = 7 });
				ClicksPerIntervalData.Add(new CampaignClicksPerInterval() { IntervalIndex = 23, LinkPk = Link3.PK, ClickCount = 8 });

				ClicksByUrlData.Clear();
				ClicksByUrlData.Add(new CampaignClicksByUrl() { LinkURL = "http://UrlOne.net.au", UniqueClickCount = 1 });
				ClicksByUrlData.Add(new CampaignClicksByUrl() { LinkURL = "http://UrlTwo.net.au", UniqueClickCount = 1 });
			}
		}
	}
}
