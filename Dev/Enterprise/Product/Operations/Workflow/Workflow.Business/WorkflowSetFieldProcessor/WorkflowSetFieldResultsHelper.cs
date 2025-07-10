using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Workflow.Integration;

namespace Enterprise.Workflow.Business
{
	public class WorkflowSetFieldResultsHelper : IWorkflowSetFieldResultsHelper
	{
		public bool IsSettingProperty(BusinessObjectFactory factory, ZPropertyInfo info)
			=> WorkflowSetFieldResultsHelperImpl.IsSettingProperty(factory, info);
	}

	public static class WorkflowSetFieldResultsHelperImpl
	{
		public static bool IsSettingProperty(BusinessObjectFactory factory, ZPropertyInfo info)
		{
			//This is a hack used in internal setter logic to prevent clobbering a property value that is intentially being set by a set field trigger action
			if (!WorkflowTriggerActionTracker.TryGetTracker(factory, out SetFieldTriggerResults setFieldResults))
			{
				return false;
			}

			return setFieldResults.IsSettingProperty(info);
		}
	}
}
