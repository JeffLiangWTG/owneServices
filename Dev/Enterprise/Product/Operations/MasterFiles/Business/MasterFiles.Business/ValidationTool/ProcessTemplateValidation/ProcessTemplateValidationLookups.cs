using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTemplateValidationLookups : AutoProcessTemplateValidationLookups
	{
		public ProcessTemplateValidationLookups(AutoProcessTemplateValidation parent) : base(parent)
		{
		}

		new ProcessTemplateValidation Parent => (ProcessTemplateValidation)base.Parent;

		public ProcessTemplateValidationSeverityList SeverityList => Factory.GetCachedValue<ProcessTemplateValidationSeverityList>();

		public CodeDescriptionPairList Condition1List => Parent.WorkflowDescriptor?.ValidationToolSettings.GetProcessTemplateValidationCondition1List() ?? new CodeDescriptionPairList();

		public CodeDescriptionPairList Condition2List
		{
			get
			{
				var list = Parent.WorkflowDescriptor?.ValidationToolSettings.GetProcessTemplateValidationCondition2List() ?? new CodeDescriptionPairList();
				if (list.Count > 0)
				{
					list.AddPair("", "");
				}
				list.AddPair(ProcessTasksLookups.MacroCondition, ProcessTasksLookups.MacroConditionDescription);
				return list;
			}
		}

		public CodeDescriptionPairList ContextTypeList
		{
			get
			{
				return Factory.GetCachedValue("ProcessTemplateValidationLookups|ContextTypeList", () =>
				{
					var list = new CodeDescriptionPairList();
					list.AddPair(ProcessTemplateValidationContextType.Codes.CargoWise, ProcessTemplateValidationContextType.Descriptions.CargoWise);
					list.AddPair(ProcessTemplateValidationContextType.Codes.GlowPortal, ProcessTemplateValidationContextType.Descriptions.GlowPortal);
					return list;
				});
			}
		}

		public ExternalRequestTypeCollection RequestTypesOnFailure
		{
			get
			{
				var jobType = Parent.WorkflowDescriptor?.ValidationToolSettings.RequestTypeJobType;
				jobType = string.IsNullOrEmpty(jobType) ? ExternalRequestTypeJobTypes.Codes.ALL : jobType;
				return new ExternalRequestTypeCollection(Factory, new ZQuery(ExternalRequestTypeSchema.RQT_JobType, new string[] { jobType, ExternalRequestTypeJobTypes.Codes.ALL }).AddToFilter(ExternalRequestTypeSchema.RQT_IsActive, true));
			}
		}
	}
}
