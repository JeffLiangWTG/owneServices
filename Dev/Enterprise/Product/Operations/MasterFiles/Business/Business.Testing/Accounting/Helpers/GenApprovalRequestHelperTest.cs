using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GenApprovalRequestHelperTest : TestCaseWithFactory
	{
		public void TestGetAllReasonCodeList()
		{
			var list = GenApprovalRequestHelper.GetAllReasonCodeList;
			AssertEquals("Count", 12, list.Count);
			AssertEquals("DAM, DSC, IAM, ICH, IDA, IDE, IJD, IOB, IRA, LDL, TXT, WOR", list.CodesAsString);
		}

		public void TestGetReasonCodeList()
		{
			var approvalRequest = Factory.New<GenApprovalRequest>();
			approvalRequest.XP_ApprovalType = Constants.GenApprovalRequestApprovalType.ARCreditNote;
			approvalRequest.XP_ParentTableCode = JobHeaderSchema.Constants.Prefix;

			var list = GenApprovalRequestHelper.GetReasonCodeList(approvalRequest);
			AssertEquals("Count", 8, list.Count);
			AssertEquals("IOB, IRA, ICH, IJD, IDA, DAM, DSC, LDL", list.CodesAsString);

			approvalRequest.XP_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			list = GenApprovalRequestHelper.GetReasonCodeList(approvalRequest);
			AssertEquals("Count", 3, list.Count);
			AssertEquals("IDE, IAM, TXT", list.CodesAsString);

			approvalRequest.XP_ApprovalType = Constants.GenApprovalRequestApprovalType.ARCreditNoteForReversal;
			list = GenApprovalRequestHelper.GetReasonCodeList(approvalRequest);
			AssertEquals("Count", 4, list.Count);
			AssertEquals("IDE, WOR, IAM, TXT", list.CodesAsString);

			list = GenApprovalRequestHelper.GetReasonCodeList(Factory.New<GenApprovalRequest>());
			AssertEquals("default list", 8, list.Count);
			AssertEquals("IOB, IRA, ICH, IJD, IDA, DAM, DSC, LDL", list.CodesAsString);
		}

		public void TestTestGetReasonCodeListForChildRequest()
		{
			var parentRequest = Factory.New<GenApprovalRequest>();
			parentRequest.XP_ApprovalType = Constants.GenApprovalRequestApprovalType.ARCreditNote;
			parentRequest.XP_ParentTableCode = JobHeaderSchema.Constants.Prefix;

			var childRequest = Factory.New<GenApprovalRequest>();
			childRequest.XP_ParentTableCode = GenApprovalRequestSchema.Constants.Prefix;
			childRequest.XP_ParentID = parentRequest.PK;

			var list = GenApprovalRequestHelper.GetReasonCodeList(childRequest);
			AssertEquals("Count", 8, list.Count);
			AssertEquals("IOB, IRA, ICH, IJD, IDA, DAM, DSC, LDL", list.CodesAsString);

			parentRequest.XP_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			list = GenApprovalRequestHelper.GetReasonCodeList(childRequest);
			AssertEquals("Count", 3, list.Count);
			AssertEquals("IDE, IAM, TXT", list.CodesAsString);

			parentRequest.XP_ApprovalType = Constants.GenApprovalRequestApprovalType.ARCreditNoteForReversal;
			list = GenApprovalRequestHelper.GetReasonCodeList(childRequest);
			AssertEquals("Count", 4, list.Count);
			AssertEquals("IDE, WOR, IAM, TXT", list.CodesAsString);
		}
	}
}
