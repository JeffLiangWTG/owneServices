using OxyPlot.Series;

namespace Enterprise.MarketingManager.GUI
{
	class TrackingStatusPieSlice : PieSlice
	{
		public TrackingStatusPieSlice(string label, double value)
			: base(label, value)
		{
		}

		public string Tag { get; set; }
	}
}
