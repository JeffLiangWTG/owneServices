using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Telematics.Module.Filters
{
	public class TelPreDriveChecklistTemplateHeaderFilterBusinessObject : FilterStripBusinessObject
	{
		#region FilterStripBusinessObject

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			var descriptionFilter = filters.AddTextFilter("Description", TelPreDriveChecklistTemplateHeaderSchema.TTH_Description);
			descriptionFilter.MultilingualDescription = ResString.GetMultilingualString("8ea03518-d6f4-43ce-82f6-8f911461c60a", "Description");
			descriptionFilter.Category = FilterCategories.TextSearch;

			return filters;
		}

		#endregion
	}
}
