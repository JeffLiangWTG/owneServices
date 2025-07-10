using CargoWise.Schema;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.Module
{
	public class CYDReceiveAdviceFilterBusinessObject : CYDFilterBusinessObject
	{
		public override SchemaGuidColumn PKSchemaColumn => CYDReceiveAdviceSchema.PK;

		protected override SchemaGuidColumn WarehouseFKSchemaColumn => CYDReceiveAdviceSchema.YRA_WW_Yard;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			AddJobNumberFilter(filters);

			return filters;
		}

		#region Filters

		void AddJobNumberFilter(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter(Schema.JobNumber, CYDReceiveAdviceSchema.YRA_JobNumber).MultilingualDescription = ResString.GetMultilingualString("CYDReceiveAdvice|CYDReceiveAdviceFilterBusinessObject|JobNumber", "Job Number");
		}

		#endregion
	}
}
