using System;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Workflow.Business
{
	public static class ExtensionMethods
	{
		public static void RequireContainmentBarrierTask(this IProcessTask task, string message = null)
		{
			if (!task.IsQualityContainmentBarrierTask())
			{
				throw new ArgumentException(message ?? "Task must be a quality containment barrier. " + ((IWorkflowItem)task).GetDiagnosticLogInfo());
			}
		}

		public static ProcessTaskIterationLinkCollection GetContainmentBarrierIterationLinks(this IProcessTask task)
		{
			return (ProcessTaskIterationLinkCollection)((ProcessTask)task).IterationLinks;
		}
	}
}
