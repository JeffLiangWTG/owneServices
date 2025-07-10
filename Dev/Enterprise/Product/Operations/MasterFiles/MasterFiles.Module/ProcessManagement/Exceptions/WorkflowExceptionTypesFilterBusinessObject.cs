using CargoWise.Application;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class WorkflowExceptionTypesFilterBusinessObject : FilterStripBusinessObject
	{
		public WorkflowExceptionTypesFilterBusinessObject()
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			filters.AddTextFilter("Code", ProcessWorkflowExceptionTypeSchema.WET_Code)
				.MultilingualDescription = ResString.GetMultilingualString("WorkflowExceptionTypesFilter|Code", "Code");

			filters.AddFiltersForTranslatableText("Description", ProcessWorkflowExceptionTypeSchema.WET_Description, typeof(ProcessWorkflowExceptionType), ResString.GetMultilingualString("WorkflowExceptionTypesFilter|Description", "Description"));

			filters.AddTextFilter("Category", ProcessWorkflowExceptionTypeSchema.WET_Category, WorkflowDataRegistry.Instance.ExceptionCategories.Value)
				.MultilingualDescription = ResString.GetMultilingualString("WorkflowExceptionTypesFilter|Category", "Category");

			filters.AddFlagFilter("Cause Required", ResString.GetMultilingualString("DCF8A4A2-D087-4CE3-AE0E-7192A7F47BB1", "Cause Required"), ProcessWorkflowExceptionTypeSchema.WET_IsCauseRequired, ModuleFilterSubGroup.Default)
				.MultilingualDescription = ResString.GetMultilingualString("WorkflowExceptionTypesFilter|CauseRequired", "Cause Required");

			filters.AddFlagFilter("Resolution Required", ResString.GetMultilingualString("B135BE98-1DEA-49A4-8B69-E48EB9D7628C", "Resolution Required"), ProcessWorkflowExceptionTypeSchema.WET_IsResolutionRequired, ModuleFilterSubGroup.Default)
				.MultilingualDescription = ResString.GetMultilingualString("WorkflowExceptionTypesFilter|ResolutionRequired", "Resolution Required");

			filters.AddTextFilter("Job Type", ProcessWorkflowExceptionTypeSchema.WET_JobType, JobTypes)
				.MultilingualDescription = ResString.GetMultilingualString("WorkflowExceptionTypesFilter|JobType", "Job Type");

			var causeSubGroup = new ExceptionTypeModuleFilterSubGroup(typeof(ProcessWorkflowExceptionCause), ProcessWorkflowExceptionCauseSchema.WEC_WET_Type);
			var resolutionSubGroup = new ExceptionTypeModuleFilterSubGroup(typeof(ProcessWorkflowExceptionResolution), ProcessWorkflowExceptionResolutionSchema.WER_WET_Type);

			var causeCodeFilter = filters.AddTextFilter("Cause Code", ProcessWorkflowExceptionCauseSchema.WEC_Code);
			causeCodeFilter.MultilingualDescription = ResString.GetMultilingualString("WorkflowExceptionTypesFilter|CauseCode", "Cause Code");
			causeCodeFilter.SubGroup = causeSubGroup;

			var causeDescriptionFilter = filters.AddTextFilter("Cause Description", ProcessWorkflowExceptionCauseSchema.WEC_Description);
			causeDescriptionFilter.MultilingualDescription = ResString.GetMultilingualString("WorkflowExceptionTypesFilter|CauseDescription", "Cause Description");
			causeDescriptionFilter.SubGroup = causeSubGroup;

			var resolutionCodeFilter = filters.AddTextFilter("Resolution Code", ProcessWorkflowExceptionResolutionSchema.WER_Code);
			resolutionCodeFilter.MultilingualDescription = ResString.GetMultilingualString("WorkflowExceptionTypesFilter|ResolutionCode", "Resolution Code");
			resolutionCodeFilter.SubGroup = resolutionSubGroup;

			var resolutionDescriptionFilter = filters.AddTextFilter("Resolution Description", ProcessWorkflowExceptionResolutionSchema.WER_Description);
			resolutionDescriptionFilter.MultilingualDescription = ResString.GetMultilingualString("WorkflowExceptionTypesFilter|ResolutionDescription", "Resolution Description");
			resolutionDescriptionFilter.SubGroup = resolutionSubGroup;

			return filters;
		}

		CodeDescriptionPairList JobTypes => types ??= (CodeDescriptionPairList)ObjectFactory.Get<IWorkflowDescriptorList>();
		CodeDescriptionPairList types;
	}
}
