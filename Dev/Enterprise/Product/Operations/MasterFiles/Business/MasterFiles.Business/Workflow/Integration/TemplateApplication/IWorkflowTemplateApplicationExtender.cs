using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IWorkflowTemplateApplicationExtender
	{
		bool IsCondition1Met(IWorkflowProvider workflowProvider, ZString conditionCode);
		bool IsCondition2Met(IWorkflowProvider workflowProvider, ZString conditionCode, ZString value);
		bool HasNewConditionBeenMetSinceLastSave(IWorkflowProvider workflowProvider);
	}
}
