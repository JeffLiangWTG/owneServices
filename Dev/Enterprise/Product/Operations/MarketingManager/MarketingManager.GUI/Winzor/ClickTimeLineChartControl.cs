using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	class ClickTimeLineChartControl : ZUserControl
	{
		public ClickTimeLineChartControl(ClickStatChartViewModel viewModel)
		{
		}

		public PlotView ClicksPerTimeIntervalChart { get; }

		public ClickStatChartViewModel DataContext { get; set; }
	}
}
