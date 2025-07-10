using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class JobDocumentDeliveryValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckMenuItemAndDocGroupAreMutuallyExclusive()
		{
			var documentDelivery = Factory.New<JobDocumentDelivery>();
			documentDelivery.JDC_DocumentGroup = ContactType.All.Code;
			AssertNoErrors("Only JDC_DocumentGroup is provided, there should be no errors on JDC_DocumentGroup", documentDelivery.JDC_DocumentGroupInfo);
			AssertNoErrors("Only JDC_DocumentGroup is provided, there should be no errors on JDC_SU_MenuItem", documentDelivery.JDC_SU_MenuItemInfo);

			documentDelivery.JDC_SU_MenuItem = ZGuid.NewZGuid();
			AssertHasErrors("Both JDC_SU_MenuItem and JDC_DocumentGroup are empty, there should be an error on JDC_DocumentGroup", documentDelivery.JDC_DocumentGroupInfo);
			AssertHasErrors("Both JDC_SU_MenuItem and JDC_DocumentGroup are empty, there should be an error on JDC_SU_MenuItem", documentDelivery.JDC_SU_MenuItemInfo);

			documentDelivery.JDC_DocumentGroup = ZString.Empty;
			AssertNoErrors("Only JDC_SU_MenuItem is provided, there should be no errors on JDC_DocumentGroup", documentDelivery.JDC_DocumentGroupInfo);
			AssertNoErrors("Only JDC_SU_MenuItem is provided, there should be no errors on JDC_SU_MenuItem", documentDelivery.JDC_SU_MenuItemInfo);

			documentDelivery.JDC_SU_MenuItem = ZGuid.Empty;
			AssertHasErrors("Both JDC_SU_MenuItem and JDC_DocumentGroup are empty JDC_DocumentGroup", documentDelivery.JDC_DocumentGroupInfo);
			AssertHasErrors("Both JDC_SU_MenuItem and JDC_DocumentGroup are emptyJDC_SU_MenuItem", documentDelivery.JDC_SU_MenuItemInfo);

			documentDelivery.JDC_DocumentGroup = ContactType.CustomerService.Code;
			AssertNoErrors("Only JDC_DocumentGroup is provided, there should be no errors on JDC_DocumentGroup", documentDelivery.JDC_DocumentGroupInfo);
			AssertNoErrors("Only JDC_DocumentGroup is provided, there should be no errors on JDC_SU_MenuItem", documentDelivery.JDC_SU_MenuItemInfo);
		}

		public void TestCheckJDC_DeliveryMethod()
		{
			var documentDelivery = Factory.New<JobDocumentDelivery>();
			documentDelivery.JDC_DeliveryMethod = ZString.Empty;
			AssertHasErrors("Delivery Method is mandatory, there should be an error", documentDelivery.JDC_DeliveryMethodInfo);

			documentDelivery.JDC_DeliveryMethod = "XXX";
			AssertHasErrors("XXX is an invalid Delivery Method, there should be an error", documentDelivery.JDC_DeliveryMethodInfo);

			documentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			AssertNoErrors("Email is a valid Delivery Method, there should be no errors", documentDelivery.JDC_DeliveryMethodInfo);

			documentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			AssertNoErrors("Print is a valid Delivery Method, there should be no errors", documentDelivery.JDC_DeliveryMethodInfo);

			documentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.DoNotDeliver;
			AssertHasError(documentDelivery.JDC_DeliveryMethodInfo, "The delivery method DND can only be used when a valid organization contact is selected.");

			var orgContact = Factory.New<OrgContact>();
			documentDelivery.JDC_OC_Contact = orgContact.PK;
			AssertNoErrors(documentDelivery.JDC_DeliveryMethodInfo);
		}

		public void TestCheckJDC_AttachmentType()
		{
			var documentDelivery = Factory.New<JobDocumentDelivery>();
			documentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;

			documentDelivery.JDC_AttachmentType = "ZZZ";
			AssertHasErrors("ZZZ is an invalid Attachment Type, there should be an error", documentDelivery.JDC_AttachmentTypeInfo);

			documentDelivery.JDC_AttachmentType = OrgConstants.AttachmentType.PDF;
			AssertNoErrors("PDF is valid for email delivery, there should be no errors", documentDelivery.JDC_AttachmentTypeInfo);
			documentDelivery.JDC_AttachmentType = OrgConstants.AttachmentType.PDFC;
			AssertNoErrors("PDFC is valid for email delivery, there should be no errors", documentDelivery.JDC_AttachmentTypeInfo);
			documentDelivery.JDC_AttachmentType = OrgConstants.AttachmentType.HTML;
			AssertNoErrors("HTML is valid for email delivery, there should be no errors", documentDelivery.JDC_AttachmentTypeInfo);
			documentDelivery.JDC_AttachmentType = OrgConstants.AttachmentType.HTMF;
			AssertNoErrors("HTMF is valid for email delivery, there should be no errors", documentDelivery.JDC_AttachmentTypeInfo);

			documentDelivery.JDC_AttachmentType = ZString.Empty;
			AssertHasErrors("Attachment Type is mandatory for email delivery, there should be an error", documentDelivery.JDC_AttachmentTypeInfo);

			documentDelivery.JDC_AttachmentType = OrgConstants.AttachmentType.XLS;
			AssertNoErrors("XLS is valid for email delivery, there should be no errors", documentDelivery.JDC_AttachmentTypeInfo);

			documentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.EPrint;
			documentDelivery.JDC_AttachmentType = ZString.Empty;
			AssertHasErrors("Attachment Type is mandatory for ePrint delivery, there should be an error", documentDelivery.JDC_AttachmentTypeInfo);

			documentDelivery.JDC_AttachmentType = OrgConstants.AttachmentType.XLS;
			AssertNoErrors("XLS is valid for ePrint delivery, there should be no errors", documentDelivery.JDC_AttachmentTypeInfo);
			documentDelivery.JDC_AttachmentType = OrgConstants.AttachmentType.PDF;
			AssertNoErrors("PDF is valid for ePrint delivery, there should be an error", documentDelivery.JDC_AttachmentTypeInfo);
			documentDelivery.JDC_AttachmentType = OrgConstants.AttachmentType.PDFC;
			AssertHasErrors("PDFC is invalid for ePrint delivery, there should be an error", documentDelivery.JDC_AttachmentTypeInfo);
			documentDelivery.JDC_AttachmentType = OrgConstants.AttachmentType.HTML;
			AssertHasErrors("HTML is invalid for ePrint delivery, there should be an error", documentDelivery.JDC_AttachmentTypeInfo);
			documentDelivery.JDC_AttachmentType = OrgConstants.AttachmentType.HTMF;
			AssertHasErrors("HTMF is invalid for ePrint delivery, there should be an error", documentDelivery.JDC_AttachmentTypeInfo);

			documentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			documentDelivery.JDC_AttachmentType = ZString.Empty;
			AssertNoErrors("Attachment Type is not required for FAX delivery, there should be no errors", documentDelivery.JDC_AttachmentTypeInfo);
		}

		public void TestCheckJDC_FaxNumber()
		{
			var documentDelivery = Factory.New<JobDocumentDelivery>();
			documentDelivery.JDC_FaxNumber = ZString.Empty;

			documentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			AssertNoErrors(documentDelivery.JDC_FaxNumberInfo);

			documentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			AssertHasErrors("FaxNumber should be mandatory for FAX delivery", documentDelivery.JDC_FaxNumberInfo);

			documentDelivery.JDC_FaxNumber = "123456";
			AssertHasErrors("FaxNumber should be valid for FAX delivery", documentDelivery.JDC_FaxNumberInfo);

			documentDelivery.JDC_FaxNumber = "+61 2 1234 5678";
			AssertNoErrors(documentDelivery.JDC_FaxNumberInfo);
		}

		public void TestCheckJDC_EmailToCopyRecipientsAsString()
		{
			var jobDocumentDelivery = Factory.NewWithValidTestData<JobDocumentDelivery>();
			jobDocumentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;

			jobDocumentDelivery.JDC_EmailToRecipientsAsString = ZString.Empty;
			AssertHasErrors("At least one Email To Recipient is required for Email delivery method, there should be an error", jobDocumentDelivery.JDC_EmailToRecipientsAsStringInfo);

			jobDocumentDelivery.JDC_EmailToRecipientsAsString = "  dexter@morgan.com,   a@b.com ";
			AssertNoErrors(jobDocumentDelivery.JDC_EmailToRecipientsAsStringInfo);

			jobDocumentDelivery.JDC_EmailToRecipientsAsString = "debra@morgan.com, dexter.morgan.com";
			AssertHasErrors(jobDocumentDelivery.JDC_EmailToRecipientsAsStringInfo);

			jobDocumentDelivery.JDC_EmailToRecipientsAsString = "debra@morgan.com; dexter.morgan.com";
			AssertHasErrors(jobDocumentDelivery.JDC_EmailToRecipientsAsStringInfo);

			jobDocumentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			jobDocumentDelivery.JDC_EmailToRecipientsAsString = ZString.Empty;
			AssertNoErrors("No Email To Recipient is required for Printing, there should be no errors", jobDocumentDelivery.JDC_EmailToRecipientsAsStringInfo);
		}

		public void TestCheckJDC_CarbonCopyRecipientsAsString()
		{
			var jobDocumentDelivery = Factory.NewWithValidTestData<JobDocumentDelivery>();
			jobDocumentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			AssertNoErrors(jobDocumentDelivery.JDC_CarbonCopyRecipientsAsStringInfo);

			jobDocumentDelivery.JDC_CarbonCopyRecipientsAsString = "  dexter@morgan.com,   a@b.com ";
			AssertNoErrors(jobDocumentDelivery.JDC_CarbonCopyRecipientsAsStringInfo);

			jobDocumentDelivery.JDC_CarbonCopyRecipientsAsString = "debra@morgan.com, dexter.morgan.com";
			AssertHasErrors(jobDocumentDelivery.JDC_CarbonCopyRecipientsAsStringInfo);

			jobDocumentDelivery.JDC_CarbonCopyRecipientsAsString = "debra@morgan.com; dexter.morgan.com";
			AssertHasErrors(jobDocumentDelivery.JDC_CarbonCopyRecipientsAsStringInfo);
		}

		public void TestCheckJDC_BlindCarbonCopyRecipientsAsString()
		{
			var jobDocumentDelivery = Factory.NewWithValidTestData<JobDocumentDelivery>();
			jobDocumentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			AssertNoErrors(jobDocumentDelivery.JDC_BlindCarbonCopyRecipientsAsStringInfo);

			jobDocumentDelivery.JDC_BlindCarbonCopyRecipientsAsString = "  dexter@morgan.com,   a@b.com ";
			AssertNoErrors(jobDocumentDelivery.JDC_BlindCarbonCopyRecipientsAsStringInfo);

			jobDocumentDelivery.JDC_BlindCarbonCopyRecipientsAsString = "debra@morgan.com, dexter.morgan.com";
			AssertHasErrors(jobDocumentDelivery.JDC_BlindCarbonCopyRecipientsAsStringInfo);

			jobDocumentDelivery.JDC_BlindCarbonCopyRecipientsAsString = "debra@morgan.com; dexter.morgan.com";
			AssertHasErrors(jobDocumentDelivery.JDC_BlindCarbonCopyRecipientsAsStringInfo);
		}
	}
}