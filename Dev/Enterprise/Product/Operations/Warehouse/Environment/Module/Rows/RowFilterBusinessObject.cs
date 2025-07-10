using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Module
{
	public class RowFilterBusinessObject : FilterStripBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			filters.AddTextFilter("Row Name", WhsRowSchema.WR_Name).MultilingualDescription = ResString.GetMultilingualString("Warehouse|RowFilter|RowName", "Row Name");

			// this needs to use a list delegate to prevent a stack overflow
			filters.AddGuidFilter("Area", ModuleIDs.WhsConfigArea, GetAreaFilter, GetAreas).MultilingualDescription = ResString.GetMultilingualString("Warehouse|RowFilter|Area", "Area");
			AddWarehouseFilter(filters);

			return filters;
		}

		#region Warehouse Filter

		void AddWarehouseFilter(ModuleFilterCollection filters)
		{
			ModuleGuidFilter warehouseFilter = filters.AddGuidFilter("Warehouse", ModuleIDs.WhsConfigWarehouse, WhsRowSchema.WR_WW_Whs, Warehouses);

			warehouseFilter.MultilingualDescription = ResString.GetMultilingualString("Warehouse|RowFilter|Warehouse", "Warehouse");

			if (!Env.Security.WhsAllowedWarehouses.IsAllowed && !Globals.IsWeb)
			{
				warehouseFilter.Visibility = FilterVisibility.AlwaysVisible;
				warehouseFilter.PropertyValidation = WarehouseFilterValidation;
			}
		}

		void WarehouseFilterValidation(ZPropertyInfo info)
		{
			if (info.Value.IsEmpty)
			{
				info.AddError(Res.GetString("71030A2E-EFBA-4a1a-AE99-AF8831950873", "Please select a warehouse to filter by"));
			}
		}

		#endregion

		#endregion

		#region Properties

		public ZGuid WR_WW_Whs
		{
			get { return WarehouseFilter != null ? WarehouseFilter.Property : ZGuid.Empty; }
		}

		public WhsWarehouse Warehouse
		{
			get { return Factory.Load<WhsWarehouse>(WR_WW_Whs); }
		}

		ModuleGuidFilter WarehouseFilter
		{
			get { return (ModuleGuidFilter)this["Warehouse"]; }
		}

		#endregion

		#region Filters

		ZQuery GetAreaFilter(ZGuid value)
		{
			var query = new ZDBOnlyQuery(typeof(WhsRow));
			var subQuery = new ZDBOnlySubQuery(typeof(WhsLocation), WhsLocationViewSchema.WLV_WR);
			subQuery.AddToFilter(WhsLocationViewSchema.WLV_WA_PickingArea, value);
			subQuery.AddToFilter(JoinCondition.Or, WhsLocationViewSchema.WLV_WA_PutawayArea, value);
			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#region Lookups

		public WhsWarehouseCollection Warehouses
		{
			get
			{
				var result = new WhsWarehouseCollectionWithSecurityCheck(Factory);
				((IWhsWarehouseCollection)result).WarehouseCollectionType = WarehouseCollectionType.All;
				return result;
			}
		}

		public WhsAreaCollection GetAreas()
		{
			return new WhsAreaCollection(Factory, Warehouse);
		}

		#endregion
	}
}
