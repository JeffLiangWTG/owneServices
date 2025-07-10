using Enterprise.Integration;

namespace Enterprise.MasterFiles.Business
{
	class DefaultTemplateConditionValueStyleDecider : ITemplateConditionValueStyleDecider
	{
		internal DefaultTemplateConditionValueStyleDecider(ProcessTaskTemplate template)
		{
			this.template = template;
		}

		readonly ProcessTaskTemplate template;

		TemplateConditionValueStyle ITemplateConditionValueStyleDecider.Decide(ITemplateConditionalWorkflowItem workflowItem)
		{
			return template?.WorkflowDescriptor?.GetCondition2ValueStyle(workflowItem) ?? TemplateConditionValueStyle.Unused;
		}
	}
}
