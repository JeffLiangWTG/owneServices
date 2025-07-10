using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Module
{
	public class USCDataVersionFilterStripBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			result.AddTextFilter("Name", USCDataVersionSchema.UZ_Name);
			result.AddTextFilter("Note", USCDataVersionSchema.UZ_Note);
			result.AddNumberRangeFilter("Version", USCDataVersionSchema.UZ_Version);

			return result;
		}
	}
}
