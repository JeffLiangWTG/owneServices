using OxyPlot;
using OxyPlot.Series;

namespace Enterprise.MarketingManager.GUI
{
	class OpportunityCreationPieSlice : PieSlice
	{
		public OpportunityCreationPieSlice(int value, OxyColor color)
			: base(string.Empty, value)
		{
			Fill = color;
			IsExploded = true;
		}
	}
}
