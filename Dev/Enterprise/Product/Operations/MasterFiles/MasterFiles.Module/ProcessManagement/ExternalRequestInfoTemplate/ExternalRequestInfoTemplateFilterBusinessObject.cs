using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	internal class ExternalRequestInfoTemplateFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			var codeFilter = filters.AddTextFilter("Code #", ExternalRequestInfoTemplateSchema.RIT_Code);
			codeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ExternalRequestInfoTemplateFilter|RQT_Code", "Code");
			codeFilter.MaxLength = ExternalRequestInfoTemplateSchema.RIT_Code.MaxLength;

			var descriptionFilter = filters.AddTextFilter("Description", ExternalRequestInfoTemplateSchema.RIT_Description);
			descriptionFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ExternalRequestInfoTemplateFilter|RQT_Description", "Description");
			descriptionFilter.MaxLength = ExternalRequestInfoTemplateSchema.RIT_Description.MaxLength;

			var jobTypeFilter = filters.AddTextFilter("Job Type", ExternalRequestInfoTemplateSchema.RIT_JobType, new ExternalRequestTypeJobTypes());
			jobTypeFilter.Category = FilterCategories.StatusAndFlags;
			jobTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ExternalRequestInfoTemplateFilter|RQT_JobType", "Job Type");

			return filters;
		}
	}
}
