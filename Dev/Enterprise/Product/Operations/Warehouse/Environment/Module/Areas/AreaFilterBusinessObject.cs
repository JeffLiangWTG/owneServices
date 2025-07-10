using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Module
{
	public class AreaFilterBusinessObject : FilterStripBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			filters.AddFiltersForTranslatableText(WhsAreaCollection.FilterConstants.AreaName, WhsAreaSchema.WA_Name, typeof(WhsArea), ResString.GetMultilingualString("Warehouse|AreaFilter|AreaName", "Area Name"));
			filters.AddTextFilter(WhsAreaCollection.FilterConstants.AreaType, WhsAreaSchema.WA_AreaType, new CodeLists.AreaTypes()).MultilingualDescription = ResString.GetMultilingualString("Warehouse|AreaFilter|AreaType", "Area Type");
			AddWarehouseFilter(filters);
			AddIsPickingAreaFilter(filters);
			AddIsPutawayAreaFilter(filters);

			return filters;
		}

		#region Warehouse Filter

		void AddWarehouseFilter(ModuleFilterCollection filters)
		{
			ModuleGuidFilter warehouseFilter = filters.AddGuidFilter(WhsAreaCollection.FilterConstants.Warehouse, ModuleIDs.WhsConfigWarehouse, WhsAreaSchema.WA_WW_Whs, Warehouses);

			warehouseFilter.MultilingualDescription = ResString.GetMultilingualString("Warehouse|AreaFilter|Warehouse", "Warehouse");

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
				info.AddError(Res.GetString("65c2aedb-253d-425a-9c44-efb0abed899c", "Please select a warehouse to filter by"));
			}
		}

		#endregion

		#region IsPickingOrPutawayArea Filter

		#region AddIsPickingAreaFilter

		void AddIsPickingAreaFilter(ModuleFilterCollection filters)
		{
			AddingPickingOrPutawayAreaFilter(filters, WhsAreaCollection.FilterConstants.IsPickingArea,
				Res.GetString("99be7792-9ead-4336-9d2e-49ddb9812f62", "Pick Area"),
				ResString.GetMultilingualString("99be7792-9ead-4336-9d2e-49ddb9812f62", "Pick Area"),
				GetPickingAreaOnlyQuery);
		}

		#endregion

		#region AddIsPutawayAreaFilter

		void AddIsPutawayAreaFilter(ModuleFilterCollection filters)
		{
			AddingPickingOrPutawayAreaFilter(filters, WhsAreaCollection.FilterConstants.IsPutawayArea,
				Res.GetString("be3187df-0130-47eb-ae62-624621682f84", "Putaway Area"),
				ResString.GetMultilingualString("be3187df-0130-47eb-ae62-624621682f84", "Putaway Area"),
				GetPutawayAreaOnlyQuery);
		}

		#endregion

		void AddingPickingOrPutawayAreaFilter(ModuleFilterCollection filters, string filterName,
			string filterDescription, MultilingualString multilingualFilterDescrition, GetFlagsQuery query)
		{
			var filterQuery = new[] { query };
			var filter = filters.AddFlagsFilter(filterName, new[] { filterDescription }, filterQuery);
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = multilingualFilterDescrition;
		}

		ZQuery GetPickingAreaOnlyQuery(ZBool value)
		{
			return GetPutawayOrPickingAreaQuery(WhsAreaSchema.WA_IsPickingArea, value);
		}

		ZQuery GetPutawayAreaOnlyQuery(ZBool value)
		{
			return GetPutawayOrPickingAreaQuery(WhsAreaSchema.WA_IsPutawayArea, value);
		}

		ZQuery GetPutawayOrPickingAreaQuery(SchemaBoolColumn boolColumn, ZBool value)
		{
			var query = new ZQuery();

			if (value)
			{
				query.AddToFilter(boolColumn, SQLComparisonOperator.Equal, true);
			}

			return query;
		}

		#endregion

		#endregion

		#region Properties

		public ZGuid WA_WW_Whs => WarehouseFilter != null ? WarehouseFilter.Property : ZGuid.Empty;

		ModuleGuidFilter WarehouseFilter => (ModuleGuidFilter)this[WhsAreaCollection.FilterConstants.Warehouse];

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

		#endregion
	}
}
