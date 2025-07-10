using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.MarketingManager.GUI
{
	public partial class TransportTradeDetailsControl : NonGroupingTradeDetailsControl
	{
		public TransportTradeDetailsControl(OrgSalesProduct salesProduct)
			: base(salesProduct)
		{
			InitializeComponent();
		}

		public override ZGrid TradeDetailsGrid
		{
			get { return tradeDetailsGrid; }
		}
	}
}
