using Enterprise.Integration;

namespace Enterprise.MasterFiles.Business
{
	public interface ITemplateConditionValueStyleDecider
	{
		TemplateConditionValueStyle Decide(ITemplateConditionalWorkflowItem workflowItem);
	}
}
