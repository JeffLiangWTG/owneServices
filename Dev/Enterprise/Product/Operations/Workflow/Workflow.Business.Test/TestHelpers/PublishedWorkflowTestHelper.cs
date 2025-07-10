using System.Linq;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Workflow.Integration;

namespace Enterprise.Workflow.Business.Test
{
	public static class PublishedWorkflowTestHelper
	{
		public static IProcessHeader CreateQualityIteration(IProcessTask containmentBarrierTask, IProcessTask iterateFromTask, string resourceUnderReviewStaffCode, string iterationReasonCode, bool shouldCreateWorkflowForIteration = true)
		{
			using (var viewModel = new ContainmentBarrierViewModel(containmentBarrierTask, ProcessTaskStatusCodeList.Codes.Closed, deselectCancelledTasksFromIteration: true))
			{
				viewModel.Response = ContainmentBarrierResponses.IterationRequired;
				viewModel.IterateFromTaskPK = iterateFromTask.PK;
				viewModel.ShouldCreateWorkflowForIteration = shouldCreateWorkflowForIteration;

				if (resourceUnderReviewStaffCode != null)
				{
					viewModel.ResourceUnderReviewNK = resourceUnderReviewStaffCode;
				}

				if (iterationReasonCode != null)
				{
					var iterationReason = viewModel.Lookups.IterationReasonsRegistryLists.Cast<WorkflowIterationReason>().FirstOrDefault(reason => reason.Code == iterationReasonCode);

					if (iterationReason != null)
					{
						viewModel.IterateReasonPK = iterationReason.PK;
					}
				}

				viewModel.CommitResponse();
			}

			var workflow = ((ProcessTask)containmentBarrierTask).ProcessHeader;

			if (workflow != null && shouldCreateWorkflowForIteration)
			{
				return (
					from childWorkflow in workflow.GetChildWorkflowsDownTheHierarchy()
					where childWorkflow.FH_CompletionStatement.Contains(" (Quality Iteration ")
					orderby workflow.FH_CompletionStatement descending
					select childWorkflow
				).First();
			}

			return null;
		}

		public static void CreatePassedContainmentBarrierRecord(IProcessTask containmentBarrierTask, string resourceUnderReviewStaffCode)
		{
			using (var viewModel = new ContainmentBarrierViewModel(containmentBarrierTask, ProcessTaskStatusCodeList.Codes.Closed, deselectCancelledTasksFromIteration: true))
			{
				viewModel.Response = ContainmentBarrierResponses.Passed;

				if (resourceUnderReviewStaffCode != null)
				{
					viewModel.ResourceUnderReviewNK = resourceUnderReviewStaffCode;
				}

				viewModel.CommitResponse();
			}
		}
	}
}
