namespace Enterprise.MarketingManager.Business
{
	class SalesProductTradeDetailFieldLayoutDefinition : SalesProductFieldLayoutDefinition
	{
		public SalesProductTradeDetailFieldLayoutDefinition(OrgSalesProduct salesProduct)
			: base(salesProduct)
		{
		}

		protected override OrgSalesProductCustomColumnDefinitionCollection GetColumns(OrgSalesProduct salesProduct)
		{
			return salesProduct.FormLayout.TradeDetailCustomColumnDefinitionCollection;
		}
	}
}
