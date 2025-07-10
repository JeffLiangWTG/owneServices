using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Workflow;
using Enterprise.Workflow.Integration;

namespace Enterprise.MasterFiles.Business
{
	class MacroValueReplacementProvider : IMacroValueReplacementProvider
	{
		readonly ProcessTask processTask;

		public MacroValueReplacementProvider(ProcessTask processTask)
		{
			this.processTask = processTask;
		}

		public object GetDataContext()
		{
			var parentBizo = processTask.Parent as BusinessObject;
			var workflowDescriptor = processTask.WorkflowDescriptor;
			return parentBizo == null ? null
				: workflowDescriptor != null && !processTask.IsTemplate ? workflowDescriptor.GetEventDataModel(parentBizo)
				: new BusinessObjectEventDataModel(parentBizo);
		}

		public bool CanEvaluateMacros => GetDataContext() != null;

		public T GetMacroValue<T>(string macro)
		{
			var context = GetDataContext();
			return ObjectFactory.Get<IWorkflowMacroValueEvaluator>().GetMacroValue<T>(processTask.Factory, context, MacroStringHelper.WrapWithQuotes(macro));
		}
	}
}
