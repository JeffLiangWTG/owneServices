using CargoWise.EntityFramework;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class WhsHandlingUnitFilterBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string Warehouse = "Warehouse";
		}

		public override ZQuery Filter
		{
			get
			{
				var query = base.Filter;
				query.AddToFilter(PkgHandlingUnitSchema.KPU_JobContext, "3PL");

				return query;
			}
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			AddWarehouseFilter(result);
			return result;
		}

		void AddWarehouseFilter(ModuleFilterCollection filters)
		{
			var warehouseCollection = new WhsWarehouseCollection(Factory, WarehouseCollectionType.ProductWarehouse);

			ModuleGuidFilter filter;

			filter = filters.AddGuidFilter(Schema.Warehouse, ModuleIDs.WhsConfigWarehouse, (warehousePK) => new ZQuery(WhsWarehouseSchema.PK, warehousePK), warehouseCollection);
			filter.SubGroup = new WarehouseSubGroup();

			filter.MultilingualDescription = ResString.GetMultilingualString("db30df62-4691-47a3-b072-66bab84af19f", "Warehouse");
			filter.Category = FilterCategories.Other;
		}

		class WarehouseSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var warehouseSubQuery = new ZDBOnlySubQuery(typeof(WhsWarehouse), WhsWarehouseSchema.WW_GB_RelatedCompanyBranch);
				warehouseSubQuery.AddToFilter(WhsWarehouseSchema.WW_WarehouseType, WarehouseTypes.Codes.Product);
				warehouseSubQuery.AddToFilter(WhsWarehouseSchema.WW_IsActive, true);
				warehouseSubQuery.AddToFilter(filter);

				var query = new ZDBOnlyQuery(typeof(PkgHandlingUnit));
				query.AddSubQuery(PkgHandlingUnitSchema.KPU_GB_Branch, warehouseSubQuery, JoinCondition.And);
				return query;
			}
		}
	}
}
