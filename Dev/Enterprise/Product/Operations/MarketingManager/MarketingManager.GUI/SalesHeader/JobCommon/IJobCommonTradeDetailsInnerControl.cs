using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public interface IJobCommonTradeDetailsInnerControl : IReadOnlyToggleControl
	{
		ZGrid GroupingGrid { get; }
		ZGrid TradeDetailsGrid { get; }
	}
}
