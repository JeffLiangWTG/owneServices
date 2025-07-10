using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsStocktakeCollection : BusinessObjectCollection<WhsStocktake>
	{
		public WhsStocktakeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WhsStocktakeCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
