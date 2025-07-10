using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Module
{
	public class EDIMessageDeliveryContextFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddProcessTypeFilter(filters);
			AddCodeFilter(filters);
			AddDescriptionFilter(filters);
			return filters;
		}

		void AddProcessTypeFilter(ModuleFilterCollection filter)
		{
			var result = filter.AddTextFilter(EDIMessageDeliveryContextSelectorSchema.Constants.ECS_ProcessType, EDIMessageDeliveryContextSelectorSchema.ECS_ProcessType, () => new WorkflowDescriptorList());
			result.MultilingualDescription = ResString.GetMultilingualString("ECSFilter|ECS_ProcessType", "Workflow Type");
		}

		void AddDescriptionFilter(ModuleFilterCollection filter)
		{
			var result = filter.AddTextFilter(EDIMessageDeliveryContextSelectorSchema.Constants.ECS_Description, EDIMessageDeliveryContextSelectorSchema.ECS_Description);
			result.MultilingualDescription = ResString.GetMultilingualString("ECSFilter|ECS_Description", "Description");
		}

		void AddCodeFilter(ModuleFilterCollection filter)
		{
			var result = filter.AddTextFilter(EDIMessageDeliveryContextSelectorSchema.Constants.ECS_Code, EDIMessageDeliveryContextSelectorSchema.ECS_Code);
			result.MultilingualDescription = ResString.GetMultilingualString("ECSFilter|ECS_Code", "Code");
		}
	}
}
