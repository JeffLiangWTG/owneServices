using Enterprise.ProcessManagement.Integration;

namespace Enterprise.ProcessManagement.Business
{
	public interface IWorkItemRelatedItem : IWorkTaskRelatedItem
	{
		bool OnRelatedWorkItemClosed(WorkItem workItem);
		void OnRelatedWorkItemReOpened(WorkItem workItem);
		void OnWorkItemAdded(WorkItem workItem);
		void OnWorkItemRemoved(WorkItem workItem);
	}
}
