using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	[TestedType(typeof(CusUSLVConsignmentForMessaging))]
	public class CusUSLVConsignmentForMessagingTest : NonPersistentBusinessObjectTestCase
	{
		public void TestReasonCode_MaxLength()
		{
			var consignmentForMessaging = new CusUSLVConsignmentForMessaging(Factory.NewWithValidTestData<CusUSLVConsignment>());
			AssertEquals(2, consignmentForMessaging.ReasonCodeInfo.MaxLength);
		}

		public void TestRequiredReference_WhenReasonCodeIsEntryReplacedBy7512()
		{
			var consignmentForMessaging = new CusUSLVConsignmentForMessaging(Factory.NewWithValidTestData<CusUSLVConsignment>());
			AssertEquals(string.Empty, consignmentForMessaging.RequiredReference);

			consignmentForMessaging.ReasonCode = ReasonCodeList.Codes.EntryReplacedBy7512;
			AssertEquals("Replacement In-Bond Number", consignmentForMessaging.RequiredReference);

			consignmentForMessaging.ReasonCode = string.Empty;
			AssertEquals(string.Empty, consignmentForMessaging.RequiredReference);
		}

		public void TestRequiredReference_WhenReasonCodeIsMerchandiseClearedByAnother()
		{
			var consignmentForMessaging = new CusUSLVConsignmentForMessaging(Factory.NewWithValidTestData<CusUSLVConsignment>());
			AssertEquals(string.Empty, consignmentForMessaging.RequiredReference);

			consignmentForMessaging.ReasonCode = ReasonCodeList.Codes.MerchandiseClearedByAnother;
			AssertEquals("Replacement Entry Number", consignmentForMessaging.RequiredReference);

			consignmentForMessaging.ReasonCode = string.Empty;
			AssertEquals(string.Empty, consignmentForMessaging.RequiredReference);
		}

		public void TestRequiredReference_WhenReasonCodeIsEntryReplacedByFTZ()
		{
			var consignmentForMessaging = new CusUSLVConsignmentForMessaging(Factory.NewWithValidTestData<CusUSLVConsignment>());
			AssertEquals(string.Empty, consignmentForMessaging.RequiredReference);

			consignmentForMessaging.ReasonCode = ReasonCodeList.Codes.EntryReplacedByFTZ;
			AssertEquals("Replacement FTZ ADM. Num.", consignmentForMessaging.RequiredReference);

			consignmentForMessaging.ReasonCode = string.Empty;
			AssertEquals(string.Empty, consignmentForMessaging.RequiredReference);
		}

		public void TestRequiredReference_IsReadOnly()
		{
			var consignmentForMessaging = new CusUSLVConsignmentForMessaging(Factory.NewWithValidTestData<CusUSLVConsignment>());
			AssertEquals("RequiredReference should always be read-only", true, consignmentForMessaging.RequiredReferenceInfo.ReadOnly);
		}

		public void TestReferenceNumber_WhenNotRequired_IsCleared()
		{
			var consignmentForMessaging = new CusUSLVConsignmentForMessaging(Factory.NewWithValidTestData<CusUSLVConsignment>());
			consignmentForMessaging.ReasonCode = ReasonCodeList.Codes.EntryReplacedByFTZ;
			consignmentForMessaging.ReferenceNumber = "12345";

			AssertEquals(true, ReasonCodeList.IsReferenceNoRequired(consignmentForMessaging.ReasonCode));

			consignmentForMessaging.ReasonCode = ReasonCodeList.Codes.EntryReplacedBy7512;
			AssertEquals(true, ReasonCodeList.IsReferenceNoRequired(consignmentForMessaging.ReasonCode));
			AssertEquals("12345", consignmentForMessaging.ReferenceNumber);

			consignmentForMessaging.ReasonCode = ReasonCodeList.Codes.MerchandiseDestroyed;
			AssertEquals(false, ReasonCodeList.IsReferenceNoRequired(consignmentForMessaging.ReasonCode));
			AssertEquals(string.Empty, consignmentForMessaging.ReferenceNumber);
		}

		public void TestReferenceNumber_WhenNotRequired_IsReadOnly()
		{
			var consignmentForMessaging = new CusUSLVConsignmentForMessaging(Factory.NewWithValidTestData<CusUSLVConsignment>());
			consignmentForMessaging.ReasonCode = ReasonCodeList.Codes.EntryReplacedByFTZ;

			AssertEquals(true, ReasonCodeList.IsReferenceNoRequired(consignmentForMessaging.ReasonCode));
			AssertEquals("ReferenceNumber should not be read-only when it is required", false, consignmentForMessaging.ReferenceNumberInfo.ReadOnly);

			consignmentForMessaging.ReasonCode = ReasonCodeList.Codes.MerchandiseDestroyed;
			AssertEquals(false, ReasonCodeList.IsReferenceNoRequired(consignmentForMessaging.ReasonCode));
			AssertEquals("ReferenceNumber should be read-only when it is required", true, consignmentForMessaging.ReferenceNumberInfo.ReadOnly);
		}

		public void TestFilesSubmittedToDIS_DefaultsToConsignmentDISIndicatorValue()
		{
			var consignment1 = Factory.NewWithValidTestData<CusUSLVConsignment>();
			var consignment2 = Factory.NewWithValidTestData<CusUSLVConsignment>();

			consignment1.ULB_DISIndicator = true;
			consignment2.ULB_DISIndicator = false;

			var consignmentForMessaging1 = new CusUSLVConsignmentForMessaging(consignment1);
			var consignmentForMessaging2 = new CusUSLVConsignmentForMessaging(consignment2);

			CombineAssertions("FilesSubmittedToDIS should default from consignment values", () =>
			{
				AssertEquals("Consignment1 - DIS Enabled", true, consignmentForMessaging1.FilesSubmittedToDIS);
				AssertEquals("Consignment2 - DIS Disabled", false, consignmentForMessaging2.FilesSubmittedToDIS);
			});
		}

		public void TestDISReference_WhenFilesNotSubmittedToDIS_IsCleared()
		{
			var consignmentForMessaging = new CusUSLVConsignmentForMessaging(Factory.NewWithValidTestData<CusUSLVConsignment>());
			consignmentForMessaging.FilesSubmittedToDIS = true;
			consignmentForMessaging.DISReference = "TESTREF";

			consignmentForMessaging.FilesSubmittedToDIS = false;
			AssertEquals(string.Empty, consignmentForMessaging.DISReference);
		}

		public void TestDISReference_WhenFilesNotSubmittedToDIS_IsReadonly()
		{
			var consignmentForMessaging = new CusUSLVConsignmentForMessaging(Factory.NewWithValidTestData<CusUSLVConsignment>());
			consignmentForMessaging.FilesSubmittedToDIS = true;
			AssertEquals("DISReference should not be read-only when FilesSubmittedToDIS", false, consignmentForMessaging.DISReferenceInfo.ReadOnly);

			consignmentForMessaging.FilesSubmittedToDIS = false;
			AssertEquals("DISReference should be read-only when FilesSubmittedToDIS", true, consignmentForMessaging.DISReferenceInfo.ReadOnly);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject() => new CusUSLVConsignmentForMessaging(Factory.NewWithValidTestData<CusUSLVConsignment>());

		#endregion
	}
}
