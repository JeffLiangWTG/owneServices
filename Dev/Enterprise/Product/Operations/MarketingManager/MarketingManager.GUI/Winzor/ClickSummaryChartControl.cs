using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	class ClickSummaryChartControl : ZUserControl
	{
		public ClickSummaryChartControl(ClickStatChartViewModel viewModel)
		{
		}

		public PlotView TotalClicksChart { get; }

		public ClickStatChartViewModel DataContext { get; set; }
	}
}
