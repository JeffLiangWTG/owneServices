using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	class TrackingStatusUserControl : ZUserControl
	{
		public TrackingStatusUserControl(TrackingStatusChartViewModel dataContext)
		{
		}

		public TrackingStatusChartViewModel DataContext { get; }

		public PieChartLayout piePlotter { get; }
	}
}
