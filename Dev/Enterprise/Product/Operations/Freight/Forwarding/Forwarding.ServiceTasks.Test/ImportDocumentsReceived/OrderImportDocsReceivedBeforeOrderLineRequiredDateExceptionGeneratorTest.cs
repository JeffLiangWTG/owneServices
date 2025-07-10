using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Test
{
	internal class OrderImportDocsReceivedBeforeOrderLineRequiredDateExceptionGeneratorTest : ImportDocsReceivedBeforeOrderLineRequiredDateExceptionGeneratorTest
	{
		protected override IWorkflowProvider WorkflowProviderWithMilestoneToComplete
		{
			get
			{
				return Order;
			}
		}
	}
}
