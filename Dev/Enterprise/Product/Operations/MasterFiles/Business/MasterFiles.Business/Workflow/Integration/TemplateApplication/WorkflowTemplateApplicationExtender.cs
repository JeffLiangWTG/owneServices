using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public abstract class WorkflowTemplateApplicationExtender<TWorkflowProvider> : IWorkflowTemplateApplicationExtender
		where TWorkflowProvider : BusinessObject, IWorkflowProvider
	{
		#region IWorkflowTemplateApplicationExtender
		bool IWorkflowTemplateApplicationExtender.IsCondition1Met(IWorkflowProvider workflowProvider, ZString conditionCode)
		{
			return IsCondition1Met((TWorkflowProvider)workflowProvider, conditionCode);
		}
		bool IWorkflowTemplateApplicationExtender.IsCondition2Met(IWorkflowProvider workflowProvider, ZString conditionCode, ZString value)
		{
			return IsCondition2Met((TWorkflowProvider)workflowProvider, conditionCode, value);
		}
		bool IWorkflowTemplateApplicationExtender.HasNewConditionBeenMetSinceLastSave(IWorkflowProvider workflowProvider)
		{
			return HasNewConditionBeenMetSinceLastSave((TWorkflowProvider)workflowProvider);
		}
		#endregion IWorkflowTemplateApplicationExtender

		protected virtual bool IsCondition1Met(TWorkflowProvider workflowProvider, ZString conditionCode)
		{
			return false;
		}
		protected virtual bool IsCondition2Met(TWorkflowProvider workflowProvider, ZString conditionCode, ZString value)
		{
			return false;
		}
		protected virtual bool HasNewConditionBeenMetSinceLastSave(TWorkflowProvider workflowProvider)
		{
			return false;
		}
	}
}
