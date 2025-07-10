namespace Enterprise.MarketingManager.Business
{
	public class SalesProductTradeLaneGridColumnDefinition : SalesProductGridColumnDefinition
	{
		public SalesProductTradeLaneGridColumnDefinition(OrgSalesProduct salesProduct)
			: base(salesProduct)
		{
		}

		protected override OrgSalesProductCustomColumnDefinitionCollection GetColumns(OrgSalesProduct salesProduct)
		{
			return salesProduct.FormLayout.TradeLaneCustomColumnDefinitionCollection;
		}
	}
}
