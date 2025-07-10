using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	class TransitionProgressChartControl : ZUserControl
	{
		public TransitionProgressChartControl(TransitionProgressViewModel dataContext)
		{
		}

		public TransitionProgressViewModel DataContext { get; }
	}
}
