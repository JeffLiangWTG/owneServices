using System.Linq;
using CargoWise.Integration;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class GenApprovalRequestHelper
	{
		public static ReadOnlyCodeDescriptionPairList GetReasonCodeList(GenApprovalRequest approvalRequest)
		{
			if (approvalRequest.IsChildRequest())
			{
				var parentRequest = approvalRequest.Factory.Load<GenApprovalRequest>(approvalRequest.XP_ParentID);
				return GetReasonCodeList(parentRequest);
			}
			else if (approvalRequest.IsTransactionAmendmentRequest())
			{
				return AmendmentReasonCodesList;
			}
			else if (approvalRequest.IsTransactionReversalRequest())
			{
				return ReversalReasonCodesList;
			}
			else
			{
				return CreditNoteReasonCodesList;
			}
		}

		public static CodeDescriptionPairList GetAllReasonCodeList
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddPairsIfNotExist(AmendmentReasonCodesList.Cast<ICodeDescription>());
				list.AddPairsIfNotExist(ReversalReasonCodesList.Cast<ICodeDescription>());
				list.AddPairsIfNotExist(CreditNoteReasonCodesList.Cast<ICodeDescription>());
				list.Sort();

				return list;
			}
		}

		static ReadOnlyCodeDescriptionPairList AmendmentReasonCodesList => AccountingMasterFilesRegistry.Instance.AmendmentReasonCodesList.Value;
		static ReadOnlyCodeDescriptionPairList ReversalReasonCodesList => AccountingMasterFilesRegistry.Instance.ReversalReasonCodesList.Value;
		static ReadOnlyCodeDescriptionPairList CreditNoteReasonCodesList => AccountingMasterFilesRegistry.Instance.CreditNoteReasonCodesList.Value;

		static bool IsTransactionAmendmentRequest(this GenApprovalRequest approvalRequest)
		{
			return approvalRequest.XP_ApprovalType == Constants.GenApprovalRequestApprovalType.ARCreditNote && approvalRequest.XP_ParentTableCode == AccTransactionHeaderSchema.Constants.Prefix;
		}

		static bool IsTransactionReversalRequest(this GenApprovalRequest approvalRequest)
		{
			return approvalRequest.XP_ApprovalType == Constants.GenApprovalRequestApprovalType.ARCreditNoteForReversal && approvalRequest.XP_ParentTableCode == AccTransactionHeaderSchema.Constants.Prefix;
		}

		static bool IsChildRequest(this GenApprovalRequest approvalRequest)
		{
			return approvalRequest.XP_ParentTableCode == GenApprovalRequestSchema.Constants.Prefix;
		}
	}
}
