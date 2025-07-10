using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	public class MessageSendingOptionTest : TestCaseWithFactory
	{
		#region ISEAdditionalData

		public void TestContactName()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var option = new MessageSendingOption(new CusUSLVConsignmentForMessaging(shipment.CusUSLVConsignments.AddNew()));

			shipment.ULH_ContactName = "Bob";

			AssertEquals("Contact Name", "Bob", ((ISEAdditionalData)option).ContactName);
		}

		public void TestContactPhone()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var option = new MessageSendingOption(new CusUSLVConsignmentForMessaging(shipment.CusUSLVConsignments.AddNew()));

			shipment.ULH_ContactPhone = "123";

			AssertEquals("Contact Phone", "123", ((ISEAdditionalData)option).ContactPhone);
		}

		public void TestReasonCode()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignmentForMessaging = new CusUSLVConsignmentForMessaging(shipment.CusUSLVConsignments.AddNew());
			var option = new MessageSendingOption(consignmentForMessaging);

			consignmentForMessaging.ReasonCode = ReasonCodeList.Codes.EntryReplacedByFTZ;

			AssertEquals("ReasonCode", ReasonCodeList.Codes.EntryReplacedByFTZ, ((ISEAdditionalData)option).ReasonCode);
		}

		public void TestReferenceIdentifier()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			shipment.ULH_JobNumber = "SHIPMENT001";

			var consignmentForMessaging = new CusUSLVConsignmentForMessaging(shipment.CusUSLVConsignments.AddNew());
			consignmentForMessaging.ReasonCode = ReasonCodeList.Codes.EntryReplacedBy7512;
			consignmentForMessaging.ReferenceNumber = "456";

			var option = new MessageSendingOption(consignmentForMessaging);

			AssertEquals("Reference Identifier", "456", ((ISEAdditionalData)option).ReferenceIdentifier);
		}

		public void TestWhenReferenceIdentifierQualifierIsCR_ThenUseConsignmentJobReferenceNumber()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			shipment.ULH_JobNumber = "SHIPMENT001";

			var consignment = shipment.CusUSLVConsignments.AddNew();
			consignment.ULB_OwnerReferenceNumber = "CONSIGNMENT001";

			var consignmentForMessaging = new CusUSLVConsignmentForMessaging(consignment);
			consignmentForMessaging.ReasonCode = ReasonCodeList.Codes.MerchandiseDestroyed;

			var option = new MessageSendingOption(consignmentForMessaging);

			AssertEquals("Expected ReferenceIdentifierQaulifier to be CR", ReferenceIdentifierCodeList.Codes.UserDefinedReferenceNumber, ((ISEAdditionalData)option).ReferenceIdentifierQualifier);
			AssertEquals("Expected ReferenceIdentifier to use consignment's clearnace's job reference number", "SHIPMENT001", ((ISEAdditionalData)option).ReferenceIdentifier);
		}

		public void TestReferenceIdentifierQualifier()
		{
			var shipment = Factory.New<CusUSLVClearance>();

			var replacementInBondConsignmentForMessaging = new CusUSLVConsignmentForMessaging(shipment.CusUSLVConsignments.AddNew());
			var replacementEntryNumberConsignmentForMessaging = new CusUSLVConsignmentForMessaging(shipment.CusUSLVConsignments.AddNew());
			var replacementFTZAdmissionNumberConsignmentForMessaging = new CusUSLVConsignmentForMessaging(shipment.CusUSLVConsignments.AddNew());
			var noReferenceQualifierConsignmentForMessaging = new CusUSLVConsignmentForMessaging(shipment.CusUSLVConsignments.AddNew());

			replacementInBondConsignmentForMessaging.ReasonCode = ReasonCodeList.Codes.EntryReplacedBy7512;
			replacementEntryNumberConsignmentForMessaging.ReasonCode = ReasonCodeList.Codes.MerchandiseClearedByAnother;
			replacementFTZAdmissionNumberConsignmentForMessaging.ReasonCode = ReasonCodeList.Codes.EntryReplacedByFTZ;
			noReferenceQualifierConsignmentForMessaging.ReasonCode = ReasonCodeList.Codes.MerchandiseDestroyed;

			var replacementInBondOption = new MessageSendingOption(replacementInBondConsignmentForMessaging);
			var replacementEntryNumberOption = new MessageSendingOption(replacementEntryNumberConsignmentForMessaging);
			var replacementFTZAdmissionNumberOption = new MessageSendingOption(replacementFTZAdmissionNumberConsignmentForMessaging);
			var noReferenceQualifierOption = new MessageSendingOption(noReferenceQualifierConsignmentForMessaging);

			CombineAssertions("ReferenceIdentifierQualifier should be set based on ReasonCode", () =>
			{
				AssertEquals("ReasonCode: EntryReplacedBy7512", ReferenceIdentifierCodeList.Codes.ReplacementInBondNumber, ((ISEAdditionalData)replacementInBondOption).ReferenceIdentifierQualifier);
				AssertEquals("ReasonCode: MerchandiseClearedByAnother", ReferenceIdentifierCodeList.Codes.ReplacementEntryNumber, ((ISEAdditionalData)replacementEntryNumberOption).ReferenceIdentifierQualifier);
				AssertEquals("ReasonCode: EntryReplacedByFTZ", ReferenceIdentifierCodeList.Codes.ReplacementFTZAdmissionNumber, ((ISEAdditionalData)replacementFTZAdmissionNumberOption).ReferenceIdentifierQualifier);
				AssertEquals("Other ReasonCode", ReferenceIdentifierCodeList.Codes.UserDefinedReferenceNumber, ((ISEAdditionalData)noReferenceQualifierOption).ReferenceIdentifierQualifier);
			});
		}

		public void TestDISIndicator()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var submitToDISConsignmentForMessaging = new CusUSLVConsignmentForMessaging(shipment.CusUSLVConsignments.AddNew());
			var doNotSubmitToDISConsignmentForMessaging = new CusUSLVConsignmentForMessaging(shipment.CusUSLVConsignments.AddNew());

			var submitToDISOption = new MessageSendingOption(submitToDISConsignmentForMessaging);
			var doNotSubmitToDISOption = new MessageSendingOption(doNotSubmitToDISConsignmentForMessaging);

			submitToDISConsignmentForMessaging.FilesSubmittedToDIS = true;
			doNotSubmitToDISConsignmentForMessaging.FilesSubmittedToDIS = false;

			CombineAssertions("ReferenceIdentifierQualifier should be set based on ReasonCode", () =>
			{
				AssertEquals("Submit to DIS", true, ((ISEAdditionalData)submitToDISOption).DISIndicator);
				AssertEquals("Do not submit to DIS", false, ((ISEAdditionalData)doNotSubmitToDISOption).DISIndicator);
			});
		}

		public void TestDISIDRefNo()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var consignmentForMessaging = new CusUSLVConsignmentForMessaging(shipment.CusUSLVConsignments.AddNew());
			var option = new MessageSendingOption(consignmentForMessaging);

			consignmentForMessaging.DISReference = "789";

			AssertEquals("DIS ID Ref. No.", "789", ((ISEAdditionalData)option).DISIDRefNo);
		}

		#endregion
	}
}
