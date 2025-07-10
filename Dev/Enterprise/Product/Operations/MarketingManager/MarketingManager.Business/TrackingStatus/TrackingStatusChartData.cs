using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Business
{
	public class TrackingStatusChartData
	{
		public string Status { get; set; }

		public MultilingualString MultilingualTrackingStatus { get; set; }

		public double EmailsCount { get; set; }

		public double ClientsCount { get; set; }

		public static string TotalStatus => "TOTALS";
		public static string NotSentCampaignStatus => "NEW";
		public static string UnsubscribeStatus => "UNS";

		public static MultilingualString UnsubscribeDescription { get; } =
			ResString.GetMultilingualString("491619c1-c03b-4587-a92b-573db67e0e1c", "Unsubscribed");
	}
}
