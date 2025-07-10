using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public class EntryTabPage : ZTabPage, IEntryTabPage
	{
		public ZGrid RateEntryGrid { get; set; }
		public RateLinesAndItemsControl RateLinesAndItemsControl { get; set; }
		public CostingRateLineAndItemsControl CostingRateLineAndItemsControl { get; set; }
	}

	public interface IEntryTabPage
	{
		ZGrid RateEntryGrid { get; }
		RateLinesAndItemsControl RateLinesAndItemsControl { get; }
		CostingRateLineAndItemsControl CostingRateLineAndItemsControl { get; }
		object Tag { get; }
	}
}
