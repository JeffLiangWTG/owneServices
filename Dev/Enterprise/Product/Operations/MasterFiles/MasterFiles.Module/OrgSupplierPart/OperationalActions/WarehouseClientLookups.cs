using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.MasterFiles.Module
{
	public class WarehouseClientLookups : ZLookups
	{
		public WarehouseClientLookups(BusinessObject parent)
			: base(parent)
		{
		}

		#region Warehouses

		public IWhsWarehouseCollection Warehouses
			=> Factory.GetCachedValue("WarehouseClientLookups|Warehouses", () => ObjectFactory.Get<IWhsWarehouseCollection>(nameof(IWhsWarehouseCollection), Factory, WarehouseCollectionType.ProductWarehouse));

		#endregion

		#region Clients

		public OrgHeaderCollection Clients
			=> Factory.GetCachedValue("WarehouseClientLookups|Clients", () => new OrgHeaderCollection(Factory));

		#endregion
	}
}
