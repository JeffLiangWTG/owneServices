using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class GenerateRMAByOrdersActionLookups : ZLookups
	{
		public GenerateRMAByOrdersActionLookups(BusinessObject parent)
			: base(parent)
		{
		}

		#region Warehouses

		public IWhsWarehouseCollection Warehouses => Factory.GetCachedValue("GenerateRMAByOrdersActionMethodApplicator|Warehouses", () => ObjectFactory.Get<IWhsWarehouseCollection>(nameof(IWhsWarehouseCollection), Factory, WarehouseCollectionType.ProductWarehouse));

		#endregion
	}
}
