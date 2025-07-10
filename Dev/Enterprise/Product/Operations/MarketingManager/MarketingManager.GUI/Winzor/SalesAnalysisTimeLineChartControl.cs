using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	class SalesAnalysisTimeLineChartControl : ZUserControl
	{
		public SalesAnalysisTimeLineChartControl(SalesAnalysisChartViewModel dataContext)
		{
		}

		public PlotView SalesTimeLineChart { get; }
	}
}
