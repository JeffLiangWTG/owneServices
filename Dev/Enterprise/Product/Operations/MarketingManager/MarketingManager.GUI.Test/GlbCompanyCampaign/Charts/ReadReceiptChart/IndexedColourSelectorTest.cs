using System.Windows.Media;
using CargoWise.Common;
using Enterprise.MarketingManager.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public class IndexedColourSelectorTest : TestCase
	{
		public void TestSelectBrush()
		{
			var data = new[]
			{
				new TestDataItem(Brushes.Orange, new TrackingStatusChartData { ClientsCount = 2, EmailsCount = 4, Status = TrackingStatusCodes.Codes.NDR }),
				new TestDataItem(Brushes.LightSkyBlue, new TrackingStatusChartData { ClientsCount = 1, EmailsCount = 6, Status = TrackingStatusCodes.Codes.VER }),
				new TestDataItem(Brushes.Silver, new TrackingStatusChartData { ClientsCount = 4, EmailsCount = 4, Status = TrackingStatusCodes.Codes.UNV }),
				new TestDataItem(Brushes.YellowGreen, new TrackingStatusChartData { ClientsCount = 5, EmailsCount = 5, Status = TrackingStatusCodes.Codes.QUE }),
				new TestDataItem(Brushes.WhiteSmoke, new TrackingStatusChartData { ClientsCount = 0, EmailsCount = 0, Status = TrackingStatusChartData.NotSentCampaignStatus }),
				new TestDataItem(Brushes.DarkSalmon, new TrackingStatusChartData { ClientsCount = 7, EmailsCount = 8, Status = TrackingStatusChartData.UnsubscribeStatus }),
				new TestDataItem(Brushes.Black, new TrackingStatusChartData { ClientsCount = 1, EmailsCount = 6, Status = "Non Existent" })
			};

			var selector = new IndexedColourSelector();
			data.ForEach(item => AssertEquals(item.ExpectedBrush, selector.SelectBrush(item.ChartData)));
		}

		class TestDataItem
		{
			public TestDataItem(SolidColorBrush expectedBrush, TrackingStatusChartData chartData)
			{
				ExpectedBrush = expectedBrush;
				ChartData = chartData;
			}

			public SolidColorBrush ExpectedBrush { get; }
			public TrackingStatusChartData ChartData { get; }
		}
	}
}
