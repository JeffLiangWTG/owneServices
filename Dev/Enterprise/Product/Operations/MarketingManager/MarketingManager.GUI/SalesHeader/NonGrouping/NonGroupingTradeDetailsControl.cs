using System;
using Enterprise.MarketingManager.Business;

namespace Enterprise.MarketingManager.GUI
{
	public class NonGroupingTradeDetailsControl : TradeDetailsControl
	{
		public NonGroupingTradeDetailsControl(OrgSalesProduct salesProduct) : base(salesProduct)
		{
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			PopulateNewDetails();
		}

		protected void PopulateNewDetails()
		{
			if (EntitySales != null && EntitySales.Product != null)
			{
				var detailsCollection = EntitySales.EntityTradeDetailsCollection;
				if (!EntitySales.IsInDatabase && !EntitySales.ReadOnly && (TradeDetailsGrid != null && !TradeDetailsGrid.ReadOnly) && EntitySales.EntityTradeDetailsCollection.Count == 0)
				{
					detailsCollection.AddNew();
				}
			}
		}
	}
}
