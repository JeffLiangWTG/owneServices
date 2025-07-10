using System.Collections.Generic;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class TradeDetailsGridMenuItemsBuilderForTest : TradeDetailsGridMenuItemsBuilder
	{
		public TradeDetailsGridMenuItemsBuilderForTest(OrgSalesProduct salesProduct, ZGrid tradeDetailsGrid)
			: base(salesProduct, tradeDetailsGrid)
		{
		}

		public IEnumerable<OrgTradeDetail> GetSelectedTradeDetailsForQuote_Exposed()
		{
			return base.GetSelectedTradeDetailsForQuote();
		}

		public OrgTradeDetail GetSelectedTradeDetailForSpotQuote_Exposed()
		{
			return base.GetSelectedTradeDetailForSpotQuote();
		}
	}
}
