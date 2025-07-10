using CargoWise.Schema;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.Module
{
	public class CYDTransportationUnitFilterBusinessObject : CYDFilterBusinessObject
	{
		public override SchemaGuidColumn PKSchemaColumn => CYDTransportationUnitSchema.PK;

		protected override SchemaGuidColumn WarehouseFKSchemaColumn => CYDTransportationUnitSchema.YTU_WW_Yard;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			AddJobNumberFilter(filters);

			return filters;
		}

		#region Filters

		void AddJobNumberFilter(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter(Schema.TransportationReference, CYDTransportationUnitSchema.YTU_TransportationReference).MultilingualDescription = ResString.GetMultilingualString("CYDTransportationUnit|CYDTransportationUnitFilterBusinessObject|TransportationReference", "Transportation Reference");
		}

		#endregion
	}
}
