using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Workflow.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTaskIterationLinkViewModelLookups : ZLookups
	{
		public ProcessTaskIterationLinkViewModelLookups(ProcessTaskIterationLinkViewModel parent)
			: base(parent)
		{
		}

		public ResourceUnderReviewCollection ResourceUnderReviewList => resourceUnderReviewList ?? (resourceUnderReviewList = new ResourceUnderReviewCollection((ProcessTask)ProcessTaskIterationLink.ContainmentBarrierTask, ProcessTaskIterationLink.Factory));
		ResourceUnderReviewCollection resourceUnderReviewList;

		public WorkflowIterationReasonCollection IterationReasons => iterationReasons ?? (iterationReasons = Factory.GetCachedValue("IterationReasons", () => WorkflowDataRegistry.Instance.IterationReasons.Value.GetIterationReasonsFromWorkflowCode(ProcessTaskIterationLinkViewModel.WorkflowType)));
		WorkflowIterationReasonCollection iterationReasons;

		#region Implementation

		ProcessTaskIterationLinkViewModel ProcessTaskIterationLinkViewModel => (ProcessTaskIterationLinkViewModel)Parent;

		IProcessTaskIterationLink ProcessTaskIterationLink => ((ProcessTaskIterationLinkViewModel)Parent).ProcessTaskIterationLink;

		#endregion
	}
}
