namespace Enterprise.MarketingManager.Business
{
	public class SalesProductTradeDetailGridColumnDefinition : SalesProductGridColumnDefinition
	{
		public SalesProductTradeDetailGridColumnDefinition(OrgSalesProduct salesProduct)
			: base(salesProduct)
		{
		}

		protected override OrgSalesProductCustomColumnDefinitionCollection GetColumns(OrgSalesProduct salesProduct)
		{
			return salesProduct.FormLayout.TradeDetailCustomColumnDefinitionCollection;
		}
	}
}
