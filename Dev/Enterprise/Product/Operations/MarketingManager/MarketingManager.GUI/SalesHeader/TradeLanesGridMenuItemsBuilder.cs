using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	class TradeLanesGridMenuItemsBuilder : TradeLaneAndDetailsMenuItemsBuilder
	{
		public TradeLanesGridMenuItemsBuilder(OrgSalesProduct salesProduct, ZGrid tradeLanesGrid)
			: base(salesProduct, tradeLanesGrid)
		{
		}

		protected override IEnumerable<OrgTradeDetail> GetSelectedTradeDetailsForQuote()
		{
			if (grid.ListManager != null)
			{
				if (grid.SelectedElements.Length > 0)
				{
					return
						grid.SelectedElements.Cast<EntitySalesWrapper>()
							.SelectMany(x => x.EntityTradeDetails);
				}
				else
				{
					var current = grid.ListManager.GetCurrent() as EntitySalesWrapper;
					if (current != null)
					{
						return current.EntityTradeDetails;
					}
				}
			}

			return Enumerable.Empty<OrgTradeDetail>();
		}

		protected override OrgTradeDetail GetSelectedTradeDetailForSpotQuote()
		{
			if (grid.ListManager != null)
			{
				var sales = grid.ListManager.GetCurrent() as EntitySalesWrapper;
				if (sales != null)
				{
					if (sales.EntityTradeDetailsCollection.Count == 1)
					{
						return sales.EntityTradeDetailsCollection[0];
					}
					else if (sales.EntityTradeDetailsCollection.Count > 0)
					{
						var form = new TradeDetailSelectionForm(sales.EntityTradeDetailsCollection);
						form.ShowTradeLaneColumns = false;
						var dialogResult = ZFormModaliser.ShowDialogAndDispose(form, grid.FindForm());
						if (dialogResult == DialogResult.OK)
						{
							return form.SelectedTradeDetail;
						}
						else
						{
							return null;
						}
					}
				}
			}

			Globals.Message.ShowInformation(Res.GetString("b59ab26c-2ca6-4a03-bfd8-a07db3813dca", "Please select the trade lane details you wish to create a quote for."));
			return null;
		}
	}
}
