using Enterprise.MasterFiles.Integration;

namespace Enterprise.Workflow.Business
{
	public class EventPublisherWorkflowDescriptorList : EventPublisherWorkflowDescriptorListAuto, IEventPublisherWorkflowDescriptorList
	{
		public EventPublisherWorkflowDescriptorList()
		{
			WorkflowDescriptorDynamicFilters.EnsureProductivitiyWiseAndTestCompatibility(this, w => w.GetType().GetInterface("IEventPublisher") != null);
		}
	}
}
