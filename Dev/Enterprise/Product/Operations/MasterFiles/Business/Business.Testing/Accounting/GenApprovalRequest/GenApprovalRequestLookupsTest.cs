using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class GenApprovalRequestLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestApprovalStatusCodeDescriptionList()
		{
			AssertEquals("Count", 6, GenApprovalRequestLookups.ApprovalStatusCodeDescriptionList.Count);
			Assert("Contains code Requested", GenApprovalRequestLookups.ApprovalStatusCodeDescriptionList.ContainsCode(Constants.GenApprovalRequestApprovalStatus.Requested));
			Assert("Contains code Cancelled", GenApprovalRequestLookups.ApprovalStatusCodeDescriptionList.ContainsCode(Constants.GenApprovalRequestApprovalStatus.Cancelled));
			Assert("Contains code Rejected", GenApprovalRequestLookups.ApprovalStatusCodeDescriptionList.ContainsCode(Constants.GenApprovalRequestApprovalStatus.Rejected));
			Assert("Contains code Approved", GenApprovalRequestLookups.ApprovalStatusCodeDescriptionList.ContainsCode(Constants.GenApprovalRequestApprovalStatus.Approved));
			Assert("Contains code Posted", GenApprovalRequestLookups.ApprovalStatusCodeDescriptionList.ContainsCode(Constants.GenApprovalRequestApprovalStatus.Posted));
			Assert("Contains code Error", GenApprovalRequestLookups.ApprovalStatusCodeDescriptionList.ContainsCode(Constants.GenApprovalRequestApprovalStatus.Error));
		}

		public virtual void TestApprovalStatusList()
		{
			GenApprovalRequestLookups testLookups = new GenApprovalRequestLookups(Factory.New<GenApprovalRequest>());
			AssertEquals("Count", GenApprovalRequestLookups.ApprovalStatusCodeDescriptionList.Count - 1, testLookups.ApprovalStatusList.Count);
			Assert("Contains code Requested", testLookups.ApprovalStatusList.ContainsCode(Constants.GenApprovalRequestApprovalStatus.Requested));
			Assert("Contains code Cancelled", testLookups.ApprovalStatusList.ContainsCode(Constants.GenApprovalRequestApprovalStatus.Cancelled));
			Assert("Contains code Rejected", testLookups.ApprovalStatusList.ContainsCode(Constants.GenApprovalRequestApprovalStatus.Rejected));
			Assert("Contains code Approved", testLookups.ApprovalStatusList.ContainsCode(Constants.GenApprovalRequestApprovalStatus.Approved));
			Assert("Contains code Posted", testLookups.ApprovalStatusList.ContainsCode(Constants.GenApprovalRequestApprovalStatus.Posted));
			Assert("Doesn't contains code Error", !testLookups.ApprovalStatusList.ContainsCode(Constants.GenApprovalRequestApprovalStatus.Error));
		}

		public void TestCreditNoteReasonCodeList()
		{
			var approvalRequest = Factory.New<GenApprovalRequest>();
			approvalRequest.XP_ApprovalType = Constants.GenApprovalRequestApprovalType.ARCreditNote;
			approvalRequest.XP_ParentTableCode = JobHeaderSchema.Constants.Prefix;

			var testLookups = new GenApprovalRequestLookups(approvalRequest);
			Assert("Contains code IOB", testLookups.ReasonCodeList.ContainsCode(Constants.GenApprovalRequestReasonCode.Code.IncorrectOrganisationBilled));
			Assert("Contains code IRA", testLookups.ReasonCodeList.ContainsCode(Constants.GenApprovalRequestReasonCode.Code.IncorrectRating));
			Assert("Contains code ICH", testLookups.ReasonCodeList.ContainsCode(Constants.GenApprovalRequestReasonCode.Code.IncorrectCharges));
			Assert("Contains code IJD", testLookups.ReasonCodeList.ContainsCode(Constants.GenApprovalRequestReasonCode.Code.IncorrectJobDetails));
			Assert("Contains code IDA", testLookups.ReasonCodeList.ContainsCode(Constants.GenApprovalRequestReasonCode.Code.IncorrectDate));
			Assert("Contains code DAM", testLookups.ReasonCodeList.ContainsCode(Constants.GenApprovalRequestReasonCode.Code.DamagedGoods));
			Assert("Contains code DSC", testLookups.ReasonCodeList.ContainsCode(Constants.GenApprovalRequestReasonCode.Code.Discount));
			Assert("Contains code LDL", testLookups.ReasonCodeList.ContainsCode(Constants.GenApprovalRequestReasonCode.Code.LateDelivery));

			approvalRequest.XP_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			testLookups = new GenApprovalRequestLookups(approvalRequest);
			Assert("Contains code IDE", testLookups.ReasonCodeList.ContainsCode("IDE"));
			Assert("Contains code IAM", testLookups.ReasonCodeList.ContainsCode("IAM"));
			Assert("Contains code TXT", testLookups.ReasonCodeList.ContainsCode("TXT"));

			approvalRequest.XP_ApprovalType = Constants.GenApprovalRequestApprovalType.ARCreditNoteForReversal;
			testLookups = new GenApprovalRequestLookups(approvalRequest);
			Assert("Contains code IDE", testLookups.ReasonCodeList.ContainsCode("IDE"));
			Assert("Contains code WOR", testLookups.ReasonCodeList.ContainsCode("WOR"));
			Assert("Contains code IAM", testLookups.ReasonCodeList.ContainsCode("IAM"));
			Assert("Contains code TXT", testLookups.ReasonCodeList.ContainsCode("TXT"));

			testLookups = new GenApprovalRequestLookups(Factory.New<GenApprovalRequest>());
			Assert("Contains code IOB", testLookups.ReasonCodeList.ContainsCode(Constants.GenApprovalRequestReasonCode.Code.IncorrectOrganisationBilled));
			Assert("Contains code IRA", testLookups.ReasonCodeList.ContainsCode(Constants.GenApprovalRequestReasonCode.Code.IncorrectRating));
			Assert("Contains code ICH", testLookups.ReasonCodeList.ContainsCode(Constants.GenApprovalRequestReasonCode.Code.IncorrectCharges));
			Assert("Contains code IJD", testLookups.ReasonCodeList.ContainsCode(Constants.GenApprovalRequestReasonCode.Code.IncorrectJobDetails));
			Assert("Contains code IDA", testLookups.ReasonCodeList.ContainsCode(Constants.GenApprovalRequestReasonCode.Code.IncorrectDate));
			Assert("Contains code DAM", testLookups.ReasonCodeList.ContainsCode(Constants.GenApprovalRequestReasonCode.Code.DamagedGoods));
			Assert("Contains code DSC", testLookups.ReasonCodeList.ContainsCode(Constants.GenApprovalRequestReasonCode.Code.Discount));
			Assert("Contains code LDL", testLookups.ReasonCodeList.ContainsCode(Constants.GenApprovalRequestReasonCode.Code.LateDelivery));
		}

		public void TestReasonCodeDescriptionList()
		{
			AssertEquals(GenApprovalRequestLookups.ReasonCodeDescriptionList(Factory.New<GenApprovalRequest>()).CodesAsString, "IOB, IRA, ICH, IJD, IDA, DAM, DSC, LDL");
			AssertCodeDescription("IOB", "IOB - Incorrect Organization Billed");
			AssertCodeDescription("IRA", "IRA - Incorrect Rating");
			AssertCodeDescription("ICH", "ICH - Incorrect Charges");
			AssertCodeDescription("IJD", "IJD - Incorrect Job Details");
			AssertCodeDescription("IDA", "IDA - Incorrect Date/s");
			AssertCodeDescription("DAM", "DAM - Damaged Goods");
			AssertCodeDescription("DSC", "DSC - Discount");
			AssertCodeDescription("LDL", "LDL - Late Delivery");
		}

		void AssertCodeDescription(string code, string expectedCodeDescription)
		{
			AssertEquals(GenApprovalRequestLookups.ReasonCodeDescriptionList(Factory.New<GenApprovalRequest>()).GetDescriptionFromCode(code), expectedCodeDescription);
		}
	}
}
