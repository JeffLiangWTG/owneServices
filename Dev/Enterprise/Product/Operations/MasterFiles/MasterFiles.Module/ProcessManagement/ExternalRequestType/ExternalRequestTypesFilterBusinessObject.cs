using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	internal class ExternalRequestTypesFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			var codeFilter = filters.AddTextFilter("Code #", ExternalRequestTypeSchema.RQT_Code);
			codeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ExternalRequestTypeFilter|RQT_Code", "Code");
			codeFilter.MaxLength = ExternalRequestTypeSchema.RQT_Code.MaxLength;

			var descriptionFilter = filters.AddTextFilter("Description", ExternalRequestTypeSchema.RQT_Description);
			descriptionFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ExternalRequestTypeFilter|RQT_Description", "Description");
			descriptionFilter.MaxLength = ExternalRequestTypeSchema.RQT_Description.MaxLength;

			var jobTypeFilter = filters.AddTextFilter("Job Type", ExternalRequestTypeSchema.RQT_JobType, new ExternalRequestTypeJobTypes());
			jobTypeFilter.Category = FilterCategories.StatusAndFlags;
			jobTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ExternalRequestTypeFilter|RQT_JobType", "Job Type");

			return filters;
		}
	}
}
