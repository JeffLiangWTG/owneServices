using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	[DefaultDataSourceBindingMember(null)]
	public partial class ForwardingShipmentTradeDetailsInnerControl : ZUserControl, IJobCommonTradeDetailsInnerControl
	{
		public ForwardingShipmentTradeDetailsInnerControl()
		{
			InitializeComponent();
		}

		public ZGrid GroupingGrid
		{
			get { return groupingGrid; }
		}

		public ZGrid TradeDetailsGrid
		{
			get { return tradeDetailsGrid; }
		}

		public bool ReadOnly { get; set; }
	}
}
