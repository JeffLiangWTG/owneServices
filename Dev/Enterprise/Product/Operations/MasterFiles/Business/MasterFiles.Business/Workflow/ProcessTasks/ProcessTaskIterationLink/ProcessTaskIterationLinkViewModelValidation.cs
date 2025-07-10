using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.WorkflowManager;
using Enterprise.Workflow.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTaskIterationLinkViewModelValidation : ZValidation
	{
		public ProcessTaskIterationLinkViewModelValidation(ProcessTaskIterationLinkViewModel parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly ProcessTaskIterationLinkViewModel parent;

		public void ValidateResourceUnderReviewNK()
		{
			ValidateCalculatedProperty(parent.ResourceUnderReviewNKInfo);
		}

		public void ValidateIterationReason()
		{
			ValidateCalculatedProperty(parent.IterationReasonInfo);
		}

		protected void CheckResourceUnderReviewNK()
		{
			ValidateResourceUnderReview(
				reviewerNK: parent.ProcessTaskIterationLink.ContainmentBarrierTask?.P9_GS_NKAssignedStaffMember ?? ZString.Empty,
				resourceUnderReviewPKInfo: parent.ResourceUnderReviewNKInfo,
				resourceUnderReviewNK: parent.ResourceUnderReviewNK,
				resourceUnderReviewList: parent.Lookups.ResourceUnderReviewList,
				isModified: parent.ResourceUnderReviewNK != parent.ProcessTaskIterationLink.P9I_GS_NKResourceUnderReview);
		}

		protected void CheckIterationReason()
		{
			var iterationReasonValidation = WorkflowDataRegistry.Instance.IterationReasons.Value.GetIterationReasonValidationFromWorkflowCode(parent.WorkflowType);

			ContainmentBarrierResponses response = ContainmentBarrierResponses.None;
			switch (parent.P9I_Outcome)
			{
				case IterationLinkOutcomeList.Codes.Passed:
					response = ContainmentBarrierResponses.Passed;
					break;
				case IterationLinkOutcomeList.Codes.IterationRequired:
					response = ContainmentBarrierResponses.IterationRequired;
					break;
				case IterationLinkOutcomeList.Codes.Deferred:
					response = ContainmentBarrierResponses.DeferredToAnotherResource;
					break;
			}

			ValidateIterateReasonByNK(
				parent.IterationReason,
				parent.IterationReasonInfo,
				response,
				iterationReasonValidation,
				parent.Lookups.IterationReasons);
		}

		public override Type AutoValidationType
		{
			get { return typeof(ProcessTaskIterationLinkViewModel); }
		}

		public override void ValidateAll()
		{
			CheckIterationReason();
			CheckResourceUnderReviewNK();
		}

		public static void ValidateResourceUnderReview(ZString reviewerNK, ZPropertyInfo resourceUnderReviewPKInfo, ZString resourceUnderReviewNK, ResourceUnderReviewCollection resourceUnderReviewList, bool isModified)
		{
			if (WorkflowDataRegistry.Instance.RequireResourceUnderReview.Value && resourceUnderReviewNK.IsEmpty && resourceUnderReviewList.Count > 0)
			{
				MandatoryValidation.CheckEntered(resourceUnderReviewPKInfo);
			}

			if (!resourceUnderReviewNK.IsEmpty)
			{
				if (isModified)
				{
					ListValidation.ErrorIfInvalidCode(resourceUnderReviewPKInfo);
				}
				else
				{
					ListValidation.WarnIfInvalidCode(resourceUnderReviewPKInfo);
				}

				if (resourceUnderReviewNK == reviewerNK)
				{
					resourceUnderReviewPKInfo.AddWarning(Res.GetString("D05B9F10-C8B4-4C4B-8857-C4F3BB89FEF1", "Resource should not review own work."));
				}
			}
		}

		#region Validate Iterate Reason

		public static void ValidateIterateReasonByPK(ZGuid iterationReasonPK, ZPropertyInfo iterateReasonInfo, ContainmentBarrierResponses response, ZString iterationReasonValidation, WorkflowIterationReasonCollection iterationReasonsRegistryList)
		{
			if (!iterationReasonPK.IsEmpty && iterationReasonsRegistryList.FindByPK(iterationReasonPK) == null)
			{
				iterateReasonInfo.AddError(InvalidIterateReasonErrorMessage);
			}

			ValidateIterateReasonCore(iterateReasonInfo, response, iterationReasonValidation);
		}

		public static void ValidateIterateReasonByNK(ZString iterationReasonNK, ZPropertyInfo iterateReasonInfo, ContainmentBarrierResponses response, ZString iterationReasonValidation, WorkflowIterationReasonCollection iterationReasonsRegistryList)
		{
			if (!iterationReasonNK.IsEmpty && iterationReasonsRegistryList.FindByCode(iterationReasonNK) == null)
			{
				iterateReasonInfo.AddError(InvalidIterateReasonErrorMessage);
			}

			ValidateIterateReasonCore(iterateReasonInfo, response, iterationReasonValidation);
		}

		static void ValidateIterateReasonCore(ZPropertyInfo iterateReasonInfo, ContainmentBarrierResponses response, ZString iterationReasonValidation)
		{
			if (response == ContainmentBarrierResponses.IterationRequired && iterationReasonValidation != IterationReasonValidationList.Codes.None)
			{
				if (iterationReasonValidation == IterationReasonValidationList.Codes.Warning)
				{
					MandatoryValidation.WarnIfNotEntered(iterateReasonInfo);
				}
				else
				{
					MandatoryValidation.CheckEntered(iterateReasonInfo);
				}
			}
		}

		#endregion

		#region Implementation

		static string InvalidIterateReasonErrorMessage => Res.GetString("cff8b77c-615d-427c-b3b8-01b08b65dc9d", "Please select a valid iteration reason.");

		#endregion
	}
}
