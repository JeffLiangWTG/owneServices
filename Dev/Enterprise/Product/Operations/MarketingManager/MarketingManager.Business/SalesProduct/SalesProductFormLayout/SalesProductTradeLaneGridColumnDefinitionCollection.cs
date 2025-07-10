
using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.Business
{
	public class SalesProductTradeLaneGridColumnDefinitionCollection : SalesProductGridColumnDefinitionCollection<SalesProductTradeLaneGridColumnDefinition>
	{
		public SalesProductTradeLaneGridColumnDefinitionCollection(OrgSalesProduct salesProduct)
			: base(salesProduct)
		{
		}

		#region Import / Export

		protected override SalesProductGridColumnDefinitionDataCollection GetGridColumnDefinitionDataCollection(SalesProductFormLayoutData data)
		{
			return data.TradeLaneGridColumnDefinitionData;
		}

		#endregion

		#region Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SalesProductTradeLaneGridColumnDefinition(SalesProduct);
		}

		#endregion
	}
}
