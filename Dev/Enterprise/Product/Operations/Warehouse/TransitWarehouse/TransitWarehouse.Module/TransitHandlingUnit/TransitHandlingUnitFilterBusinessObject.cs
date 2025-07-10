using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Module
{
	public class TransitHandlingUnitFilterBusinessObject : WhsTransitFilterBusinessObject
	{
		public override ZQuery Filter
		{
			get
			{
				var query = base.Filter;
				query.AddToFilter(PkgHandlingUnitSchema.KPU_JobContext, "TWH");

				return query;
			}
		}

		protected override SchemaGuidColumn WarehouseFKSchemaColumn => null;

		protected override ModuleFilterSubGroup GetWarehouseSubGroupQuery() => new WarehouseSubGroup();

		class WarehouseSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var warehouseSubQuery = new ZDBOnlySubQuery(typeof(WhsWarehouse), WhsWarehouseSchema.WW_GB_RelatedCompanyBranch);
				warehouseSubQuery.AddToFilter(WhsWarehouseSchema.WW_WarehouseType, WarehouseTypes.Codes.Transit);
				warehouseSubQuery.AddToFilter(WhsWarehouseSchema.WW_IsActive, true);
				warehouseSubQuery.AddToFilter(filter);

				var query = new ZDBOnlyQuery(typeof(PkgHandlingUnit));
				query.AddSubQuery(PkgHandlingUnitSchema.KPU_GB_Branch, warehouseSubQuery, JoinCondition.And);
				return query;
			}
		}
	}
}
