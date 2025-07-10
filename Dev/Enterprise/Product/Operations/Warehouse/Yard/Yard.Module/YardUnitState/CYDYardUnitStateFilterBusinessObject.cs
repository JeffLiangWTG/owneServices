using CargoWise.Schema;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.Module
{
	public class CYDYardUnitStateFilterBusinessObject : CYDFilterBusinessObject
	{
		public override SchemaGuidColumn PKSchemaColumn => CYDYardUnitStateSchema.PK;

		protected override SchemaGuidColumn WarehouseFKSchemaColumn => CYDYardUnitStateSchema.YUS_WW_CurrentYard;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			AddUnitNumberFilter(filters);
			return filters;
		}

		#region Filters

		void AddUnitNumberFilter(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter(Schema.UnitNumber, CYDYardUnitStateSchema.YUS_UnitID).MultilingualDescription = ResString.GetMultilingualString("CYDYardUnitState|CYDYardUnitStateFilterBusinessObject|UnitNumber", "Unit Number");
		}

		#endregion
	}
}
