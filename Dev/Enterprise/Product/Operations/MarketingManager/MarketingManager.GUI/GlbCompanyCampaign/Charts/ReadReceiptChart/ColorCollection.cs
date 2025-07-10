using System.Windows.Media;
using Enterprise.MarketingManager.Business;
using OxyPlot;

namespace Enterprise.MarketingManager.GUI
{
	static class ColorCollection
	{
		public static SolidColorBrush GetColorBrush(string status)
		{
			if (string.IsNullOrEmpty(status))
			{
				return Brushes.Black;
			}

			if (status == TrackingStatusCodes.Codes.VER)
			{
				return Brushes.LightSkyBlue;
			}
			if (status == TrackingStatusCodes.Codes.NDR)
			{
				return Brushes.Orange;
			}
			if (status == TrackingStatusCodes.Codes.UNV)
			{
				return Brushes.Silver;
			}
			if (status == TrackingStatusCodes.Codes.QUE)
			{
				return Brushes.YellowGreen;
			}
			if (status == TrackingStatusCodes.Codes.OPC)
			{
				return Brushes.LimeGreen;
			}
			if (status == TrackingStatusCodes.Codes.OPQ)
			{
				return Brushes.IndianRed;
			}
			if (status == TrackingStatusChartData.NotSentCampaignStatus)
			{
				return Brushes.WhiteSmoke;
			}
			if (status == TrackingStatusChartData.UnsubscribeStatus)
			{
				return Brushes.DarkSalmon;
			}
			if (status == TrackingSummaryConstants.Codes.FAI)
			{
				return Brushes.Crimson;
			}
			if (status == TrackingSummaryConstants.Codes.SCH)
			{
				return Brushes.MediumSeaGreen;
			}

			return Brushes.Black;
		}

		public static OxyColor GetOxyColor(string status)
		{
			var color = GetColorBrush(status).Color;
			return OxyColor.FromArgb(color.A, color.R, color.G, color.B);
		}
	}
}
