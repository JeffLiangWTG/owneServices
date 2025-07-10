using System.Collections.Generic;
using System.Linq;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	class TradeDetailsGridMenuItemsBuilder : TradeLaneAndDetailsMenuItemsBuilder
	{
		public TradeDetailsGridMenuItemsBuilder(OrgSalesProduct salesProduct, ZGrid tradeDetailsGrid)
			: base(salesProduct, tradeDetailsGrid)
		{
		}

		protected override IEnumerable<OrgTradeDetail> GetSelectedTradeDetailsForQuote()
		{
			if (grid.ListManager != null)
			{
				if (grid.SelectedElements.Length > 0)
				{
					return grid.SelectedElements.Cast<OrgTradeDetail>();
				}
				else
				{
					var current = grid.ListManager.GetCurrent() as OrgTradeDetail;
					if (current != null)
					{
						return new[] { current };
					}
				}
			}

			return Enumerable.Empty<OrgTradeDetail>();
		}

		protected override OrgTradeDetail GetSelectedTradeDetailForSpotQuote()
		{
			if (grid.ListManager != null)
			{
				if (grid.SelectedElements.Length == 1)
				{
					return (OrgTradeDetail)grid.SelectedElements[0];
				}
				else
				{
					var current = grid.ListManager.GetCurrent() as OrgTradeDetail;
					if (current != null)
					{
						return current;
					}
				}
			}

			Globals.Message.ShowInformation(Res.GetString("496cd74d-534a-40d0-b502-a6e6e4071365", "Please select a trade lane detail you wish to create a spot quote for."));
			return null;
		}
	}
}
