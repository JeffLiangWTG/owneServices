using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public static class WorkflowProviderHelper
	{
		public static ZController GetControllerForWorkflowType(string workflowTypeCode)
		{
			ControllerID controllerId = null;

			if (WorkflowDescriptors.Instance.TryGetValue(workflowTypeCode, out var descriptor))
			{
				controllerId = descriptor.ControllerID;
			}

			if (controllerId == null)
			{
				var type = JobInvoicingConsumerTypes.New()[workflowTypeCode];
				controllerId = type?.ControllerID;
			}

			return controllerId == null ? null : ZControllerFactory.Create(controllerId);
		}
	}
}
