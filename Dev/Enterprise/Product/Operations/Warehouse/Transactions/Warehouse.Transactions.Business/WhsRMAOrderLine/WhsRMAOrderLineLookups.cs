using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsRMAOrderLineLookups : ZLookups
	{
		public WhsRMAOrderLineLookups(BusinessObject parent)
			: base(parent)
		{
		}

		#region Warehouses

		public IWhsWarehouseCollection Warehouses
			=> Factory.GetCachedValue("WhsRMAOrderLine|Warehouses", () => ObjectFactory.Get<IWhsWarehouseCollection>(nameof(IWhsWarehouseCollection), Factory, WarehouseCollectionType.ProductWarehouse));

		#endregion
	}
}
