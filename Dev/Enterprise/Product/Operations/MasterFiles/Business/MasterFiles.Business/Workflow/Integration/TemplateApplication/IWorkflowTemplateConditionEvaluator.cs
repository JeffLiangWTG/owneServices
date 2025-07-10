using Enterprise.Integration;

namespace Enterprise.MasterFiles.Business
{
	public interface IWorkflowTemplateConditionEvaluator
	{
		bool AreConditionsMetForTemplateApplication(IWorkflowProvider workflowProvider, ITemplateConditionalWorkflowItem workflowItem);
		bool HasNewConditionBeenMetSinceLastSave(IWorkflowProvider workflowProvider);
		bool IsCondition2Met(IWorkflowProvider workflowProvider, ITemplateConditionalWorkflowItem workflowItem);
	}
}
