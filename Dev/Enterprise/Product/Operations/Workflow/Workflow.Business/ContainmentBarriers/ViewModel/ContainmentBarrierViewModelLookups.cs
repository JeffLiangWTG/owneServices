using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Workflow.Business
{
	public class ContainmentBarrierViewModelLookups : ZLookups
	{
		public ContainmentBarrierViewModelLookups(ContainmentBarrierViewModel parent)
			: base(parent)
		{
		}

		public ProcessTaskFriendlyViewCollectionView ProcessTaskList
		{
			get { return processTaskList ?? (processTaskList = new ProcessTaskFriendlyViewCollectionView(ContainmentBarrierViewModel.JobTasksWithWorkflowFiltering)); }
			set { processTaskList = value; }
		}
		ProcessTaskFriendlyViewCollectionView processTaskList;

		public IBusinessObjectCollection JobWorkflows => jobWorkflows ?? (jobWorkflows = ((IterateFromProcessTaskCollectionView)ContainmentBarrierViewModel.JobTasksWithWorkflowFiltering).GetWorkflows());
		IBusinessObjectCollection jobWorkflows;

		public WorkflowIterationReasonCollection IterationReasonsRegistryLists => iterationReasonsRegistryList ?? (iterationReasonsRegistryList = WorkflowDataRegistry.Instance.IterationReasons.Value.GetIterationReasonsFromWorkflowCode(ContainmentBarrierViewModel.ContainmentBarrierTask.Parent.WorkflowType));
		WorkflowIterationReasonCollection iterationReasonsRegistryList;

		public ResourceUnderReviewCollection ResourceUnderReviewList => resourceUnderReviewList ?? (resourceUnderReviewList = new ResourceUnderReviewCollection(ContainmentBarrierViewModel.ContainmentBarrierTask, ContainmentBarrierViewModel.Factory));
		ResourceUnderReviewCollection resourceUnderReviewList;

		#region Implementation

		ContainmentBarrierViewModel ContainmentBarrierViewModel => (ContainmentBarrierViewModel)Parent;

		#endregion
	}
}
