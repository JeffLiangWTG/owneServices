using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.Business
{
	public class SalesProductTradeDetailFieldLayoutDefinitionCollection : SalesProductFieldLayoutDefinitionCollection
	{
		public SalesProductTradeDetailFieldLayoutDefinitionCollection(OrgSalesProduct salesProduct)
			: base(salesProduct)
		{
		}

		protected override SalesProductFieldLayoutDefinitionDataCollection GetDefinitionDataCollection(SalesProductFormLayoutData data)
		{
			return data.TradeLaneFieldLayoutDefinitionData;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SalesProductTradeDetailFieldLayoutDefinition(salesProduct);
		}
	}
}
