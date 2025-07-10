using CargoWise.Schema;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.Module
{
	public class CYDReleaseAdviceFilterBusinessObject : CYDFilterBusinessObject
	{
		public override SchemaGuidColumn PKSchemaColumn => CYDReleaseAdviceSchema.PK;

		protected override SchemaGuidColumn WarehouseFKSchemaColumn => CYDReleaseAdviceSchema.YRE_WW_Yard;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			AddJobNumberFilter(filters);

			return filters;
		}

		#region Filters

		void AddJobNumberFilter(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter(Schema.JobNumber, CYDReleaseAdviceSchema.YRE_JobNumber).MultilingualDescription = ResString.GetMultilingualString("CYDReleaseAdvice|CYDReleaseAdviceFilterBusinessObject|JobNumber", "Job Number");
		}

		#endregion
	}
}
