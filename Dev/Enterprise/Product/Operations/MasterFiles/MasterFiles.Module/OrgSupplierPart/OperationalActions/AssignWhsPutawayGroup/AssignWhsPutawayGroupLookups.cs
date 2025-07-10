using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;

namespace Enterprise.MasterFiles.Module
{
	public class AssignWhsPutawayGroupLookups : ZLookups
	{
		public AssignWhsPutawayGroupLookups(BusinessObject parent)
			: base(parent)
		{
		}

		#region WhsPutawayGroups

		public IWhsPutawayGroupCollection WhsPutawayGroups => Factory.GetCachedValue("AssignWhsPutawayGroupLookups|WhsPutawayGroups", () => ObjectFactory.New<IWhsPutawayGroupCollection>(Factory));

		#endregion

		#region Warehouses

		public IWhsWarehouseCollection Warehouses => Factory.GetCachedValue("AssignWhsPutawayGroupLookups|Warehouses", () => ObjectFactory.Get<IWhsWarehouseCollection>(nameof(IWhsWarehouseCollection), Factory, WarehouseCollectionType.ProductWarehouse));

		#endregion
	}
}
