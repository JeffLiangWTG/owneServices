using CargoWise.Schema;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.Module
{
	public class MNRSurveyFilterBusinessObject : FilterStripBusinessObject
	{
		public override SchemaGuidColumn PKSchemaColumn => MNRSurveySchema.PK;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			return filters;
		}
	}
}
