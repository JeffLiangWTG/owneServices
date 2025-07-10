using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	class OpportunityCreationUserControl : ZUserControl
	{
		public OpportunityCreationUserControl(OpportunityCreationChartViewModel dataContext)
		{
		}

		public OpportunityCreationChartViewModel DataContext { get; set; }

		public OpportunityCreationPieChartLayout piePlotter { get; }
	}
}
