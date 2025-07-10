using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.Business
{
	public class OrgTradeDetailJobCommonGroupingCollection : NonPersistentBusinessObjectCollection<OrgTradeDetailJobCommonGrouping>
	{
		public OrgTradeDetailJobCommonGroupingCollection(EntityTradeDetailWrapperCollectionCompanyView entityTradeDetails, OrgSalesProduct salesProduct)
			: base(entityTradeDetails.Factory)
		{
			Argument.NotNull(entityTradeDetails, "entityTradeDetailCollection");
			Argument.NotNull(salesProduct, "salesProduct");

			this.entityTradeDetails = entityTradeDetails;
			this.Product = salesProduct;
		}

		readonly EntityTradeDetailWrapperCollectionCompanyView entityTradeDetails;
		public readonly OrgSalesProduct Product;

		public void PopulateDefaultElements()
		{
			using (SuspendListChanged())
			{
				var groupedTradeDetails = entityTradeDetails.Cast<EntityTradeDetailWrapper>().GroupBy(x => new
				{
					x.PA_TradeMode,
					x.PA_TradeType
				});

				foreach (var tradeDetailGrouping in groupedTradeDetails)
				{
					var grouping = new OrgTradeDetailJobCommonGrouping(
						entityTradeDetails,
						Product,
						tradeDetailGrouping.Key.PA_TradeMode,
						tradeDetailGrouping.Key.PA_TradeType,
						tradeDetailGrouping);

					Add(grouping);
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var grouping = new OrgTradeDetailJobCommonGrouping(entityTradeDetails, Product);
			grouping.Elements.AddNew();
			return grouping;
		}
	}
}
