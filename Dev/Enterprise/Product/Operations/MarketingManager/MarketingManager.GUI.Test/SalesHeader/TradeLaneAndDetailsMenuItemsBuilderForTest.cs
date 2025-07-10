using System.Collections.Generic;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class TradeLaneAndDetailsMenuItemsBuilderForTest : TradeLaneAndDetailsMenuItemsBuilder
	{
		public TradeLaneAndDetailsMenuItemsBuilderForTest(OrgSalesProduct salesProduct, ZGrid grid)
			: base(salesProduct, grid)
		{
		}

		protected override IEnumerable<OrgTradeDetail> GetSelectedTradeDetailsForQuote()
		{
			return SelectedTradeDetailsForQuote;
		}

		public IEnumerable<OrgTradeDetail> SelectedTradeDetailsForQuote;

		protected override OrgTradeDetail GetSelectedTradeDetailForSpotQuote()
		{
			return SelectedTradeDetailForSpotQuote;
		}

		public OrgTradeDetail SelectedTradeDetailForSpotQuote;
	}
}
