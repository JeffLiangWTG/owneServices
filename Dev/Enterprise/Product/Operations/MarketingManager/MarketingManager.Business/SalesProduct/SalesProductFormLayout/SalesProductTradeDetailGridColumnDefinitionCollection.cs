using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.Business
{
	public class SalesProductTradeDetailGridColumnDefinitionCollection : SalesProductGridColumnDefinitionCollection<SalesProductTradeDetailGridColumnDefinition>
	{
		public SalesProductTradeDetailGridColumnDefinitionCollection(OrgSalesProduct salesProduct)
			: base(salesProduct)
		{
		}

		#region Import / Export

		protected override SalesProductGridColumnDefinitionDataCollection GetGridColumnDefinitionDataCollection(SalesProductFormLayoutData data)
		{
			return data.TradeDetailGridColumnDefinitionData;
		}

		#endregion

		#region Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SalesProductTradeDetailGridColumnDefinition(SalesProduct);
		}

		#endregion
	}
}
