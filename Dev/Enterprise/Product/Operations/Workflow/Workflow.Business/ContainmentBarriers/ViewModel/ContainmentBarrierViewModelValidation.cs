using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Workflow.Integration;

namespace Enterprise.Workflow.Business
{
	public class ContainmentBarrierViewModelValidation : ZValidation
	{
		public ContainmentBarrierViewModelValidation(ContainmentBarrierViewModel parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly ContainmentBarrierViewModel parent;

		public void ValidateIterateFromTaskPK()
		{
			ValidateCalculatedProperty(parent.IterateFromTaskPKInfo);
		}

		public void ValidateIterateFromWorkflowPK()
		{
			ValidateCalculatedProperty(parent.IterateFromWorkflowPKInfo);
		}

		public void ValidateIterateReasonPK()
		{
			ValidateCalculatedProperty(parent.IterateReasonPKInfo);
		}

		public void ValidateResourceUnderReviewNK()
		{
			ValidateCalculatedProperty(parent.ResourceUnderReviewNKInfo);
		}

		protected void CheckIterateFromTaskPK()
		{
			if (!parent.IterateFromTaskPK.IsEmpty && parent.IterateFromTaskPK != parent.ContainmentBarrierTask.PK && !parent.JobTasksWithWorkflowFiltering.Contains(parent.IterateFromTaskPK))
			{
				parent.IterateFromTaskPKInfo.AddError(Res.GetString("85bb0255-5df0-4e67-b447-1849534612b9", "Please select a valid Iterate From Task."));
			}
			if (parent.Response != null && parent.Response.Value == ContainmentBarrierResponses.IterationRequired)
			{
				MandatoryValidation.CheckEntered(parent.IterateFromTaskPKInfo);
			}
		}

		protected void CheckIterateFromWorkflowPK()
		{
			if (!parent.IterateFromWorkflowPK.IsEmpty && !parent.Lookups.JobWorkflows.Contains(parent.IterateFromWorkflowPK))
			{
				parent.IterateFromWorkflowPKInfo.AddError(Res.GetString("cbfe30ae-30da-48b4-ade8-be5849e88c90", "Please select a valid Iterate From Workflow."));
			}

			if (parent.Response != null && parent.Response.Value == ContainmentBarrierResponses.IterationRequired && ((IterateFromProcessTaskCollectionView)(parent.JobTasksWithWorkflowFiltering)).SelectedWorkflow != null)
			{
				MandatoryValidation.CheckEntered(parent.IterateFromWorkflowPKInfo);
			}
		}

		protected void CheckIterateReasonPK()
		{
			ProcessTaskIterationLinkViewModelValidation.ValidateIterateReasonByPK(
				parent.IterateReasonPK,
				parent.IterateReasonPKInfo,
				parent?.Response ?? ContainmentBarrierResponses.None,
				parent.IterationReasonValidation,
				parent.Lookups.IterationReasonsRegistryLists);
		}

		protected void CheckResourceUnderReviewNK()
		{
			if (parent.Response == null || parent.Response.Value != ContainmentBarrierResponses.Canceled)
			{
				ProcessTaskIterationLinkViewModelValidation.ValidateResourceUnderReview(
					reviewerNK: parent.ContainmentBarrierTask.P9_GS_NKAssignedStaffMember,
					resourceUnderReviewPKInfo: parent.ResourceUnderReviewNKInfo,
					resourceUnderReviewNK: parent.ResourceUnderReviewNK,
					resourceUnderReviewList: parent.Lookups.ResourceUnderReviewList,
					isModified: true); // ContaintmentBarrierViewModel always has the ResourceUnderReview modified
			}
		}

		public override Type AutoValidationType
		{
			get { return typeof(ContainmentBarrierViewModel); }
		}

		public override void ValidateAll()
		{
			ValidateIterateFromTaskPK();
			ValidateResourceUnderReviewNK();
		}
	}
}
