using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	class OpportunityCreationChartLegend : ZUserControl
	{
		public ZTextBox CurrentOpportunitiesTotal { get; }
		public ZTextBox WonOpportunitiesTotal { get; }
		public ZTextBox LostOpportunitiesTotal { get; }
		public ZTextBox OtherOpportunitiesTotal { get; }
		public ZTextBox LinkedOpportunitiesTotal { get; }
		public ZTextBox WinRatioOpportunitiesTotal { get; }
	}
}
