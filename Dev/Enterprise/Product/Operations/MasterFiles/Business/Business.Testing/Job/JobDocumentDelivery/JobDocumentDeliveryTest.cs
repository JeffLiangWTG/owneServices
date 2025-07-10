using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobDocumentDelivery))]
	public class JobDocumentDeliveryTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetDocDeliveryDetails()
		{
			var jobDocumentDelivery = Factory.New<JobDocumentDelivery>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			jobDocumentDelivery.JDC_OH = org.PK;
			jobDocumentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			jobDocumentDelivery.JDC_AttachmentType = OrgConstants.AttachmentType.PDF;
			jobDocumentDelivery.JDC_EmailSubjectMacro = "Hello <JobNumber>";
			jobDocumentDelivery.JDC_ContactName = "Just1n";
			jobDocumentDelivery.JDC_DocumentGroup = "CNE";
			jobDocumentDelivery.EmailToRecipients.Value = "i@just1n.net, just1n@outlook.com";
			jobDocumentDelivery.CarbonCopyRecipients.Value = "hello@world.com";

			var contact = jobDocumentDelivery.GetDocDeliveryDetails(null);
			AssertEquals(org.PK, contact.OrgHeaderPK);
			AssertEquals(jobDocumentDelivery.JDC_DeliveryMethod, contact.DeliveryMethod);
			AssertEquals(jobDocumentDelivery.JDC_AttachmentType, contact.AttachmentType);
			AssertEquals(jobDocumentDelivery.JDC_EmailSubjectMacro, contact.EmailSubjectMacro);
			AssertEquals(jobDocumentDelivery.JDC_ContactName, contact.Name);
			AssertEquals(jobDocumentDelivery.EmailToRecipients.Value, contact.EmailToRecipients.Value);
			AssertEquals(jobDocumentDelivery.CarbonCopyRecipients.Value, contact.EmailCarbonCopyRecipients.Value);

			var orgContact = org.Contacts.AddNew();
			orgContact.OC_ContactName = "Another Just1n";
			orgContact.OC_Email = "another@just1n.net";
			jobDocumentDelivery.JDC_OC_Contact = orgContact.PK;
			contact = jobDocumentDelivery.GetDocDeliveryDetails(null);

			AssertEquals("Another Just1n", contact.Name);
			AssertEquals("another@just1n.net", contact.EmailToRecipients.Value);
		}

		public void TestDefaultValues()
		{
			var jobDocumentDelivery = Factory.New<JobDocumentDelivery>();
			AssertEquals(Core.Constants.ContactNotifyModes.Email, jobDocumentDelivery.JDC_DeliveryMethod);
			AssertEquals(OrgConstants.AttachmentType.PDF, jobDocumentDelivery.JDC_AttachmentType);
		}

		public void TestJDC_OC_Contact()
		{
			var dexter = Factory.NewWithValidTestData<OrgContact>();
			dexter.OC_Email = "dexter@morgan.com";

			var debra = Factory.NewWithValidTestData<OrgContact>();
			debra.OC_Email = "debra@morgan.com";

			var jobDocumentDelivery = Factory.New<JobDocumentDelivery>();
			jobDocumentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			Assert("Pre-condition", jobDocumentDelivery.JDC_EmailToRecipientsAsString.IsEmpty);

			jobDocumentDelivery.JDC_OC_Contact = dexter.PK;
			Assert(jobDocumentDelivery.JDC_EmailToRecipientsAsString.IsEmpty);

			jobDocumentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			jobDocumentDelivery.JDC_OC_Contact = debra.PK;
			AssertEquals("debra@morgan.com", jobDocumentDelivery.JDC_EmailToRecipientsAsString);
		}

		public void TestJDC_EmailSubjectMacro_Readonly()
		{
			var jobDocumentDelivery = Factory.New<JobDocumentDelivery>();
			jobDocumentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			jobDocumentDelivery.JDC_EmailSubjectMacro = "Some <Macro>";
			jobDocumentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			Assert(jobDocumentDelivery.JDC_EmailSubjectMacroInfo.ReadOnly);

			jobDocumentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			Assert(!jobDocumentDelivery.JDC_EmailSubjectMacroInfo.ReadOnly);
			AssertEquals("Some <Macro>", jobDocumentDelivery.JDC_EmailSubjectMacro);
		}

		public void TestJDC_AttachmentType()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var jobDocumentDelivery = Factory.New<JobDocumentDelivery>();
			jobDocumentDelivery.JDC_OC_Contact = contact.PK;
			jobDocumentDelivery.Contact.OC_AttachmentType = OrgConstants.AttachmentType.XLS;

			jobDocumentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			Assert("JDC_AttachmentType should be readonly", jobDocumentDelivery.JDC_AttachmentTypeInfo.ReadOnly);
			AssertEquals("JDC_AttachmentType", ZString.Empty, jobDocumentDelivery.JDC_AttachmentType);

			jobDocumentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			Assert("JDC_AttachmentType should not be readonly", !jobDocumentDelivery.JDC_AttachmentTypeInfo.ReadOnly);
			AssertEquals("JDC_AttachmentType", OrgConstants.AttachmentType.XLS, jobDocumentDelivery.JDC_AttachmentType);

			jobDocumentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.EPrint;
			Assert("JDC_AttachmentType should not be readonly", !jobDocumentDelivery.JDC_AttachmentTypeInfo.ReadOnly);
			AssertEquals("JDC_AttachmentType", OrgConstants.AttachmentType.XLS, jobDocumentDelivery.JDC_AttachmentType);

			jobDocumentDelivery.Contact.OC_AttachmentType = OrgConstants.AttachmentType.PDF;

			jobDocumentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			Assert("JDC_AttachmentType should not be readonly", !jobDocumentDelivery.JDC_AttachmentTypeInfo.ReadOnly);
			AssertEquals("JDC_AttachmentType", OrgConstants.AttachmentType.PDF, jobDocumentDelivery.JDC_AttachmentType);

			jobDocumentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.EPrint;
			Assert("JDC_AttachmentType should not be readonly", !jobDocumentDelivery.JDC_AttachmentTypeInfo.ReadOnly);
			AssertEquals("JDC_AttachmentType", OrgConstants.AttachmentType.PDF, jobDocumentDelivery.JDC_AttachmentType);
		}

		public void TestJDC_FaxNumber_ReadOnly()
		{
			var jobDocumentDelivery = Factory.New<JobDocumentDelivery>();
			jobDocumentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			Assert(jobDocumentDelivery.JDC_FaxNumberInfo.ReadOnly);

			jobDocumentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			Assert(!jobDocumentDelivery.JDC_FaxNumberInfo.ReadOnly);

			jobDocumentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			Assert(jobDocumentDelivery.JDC_FaxNumberInfo.ReadOnly);
		}

		public void TestJDC_EmailToRecipientsAsString()
		{
			var jobDocumentDelivery = Factory.New<JobDocumentDelivery>();
			AssertRecipientsAsString(jobDocumentDelivery, nameof(jobDocumentDelivery.JDC_EmailToRecipientsAsString), jobDocumentDelivery.EmailToRecipients);
		}

		public void TestJDC_CarbonCopyRecipientsAsString()
		{
			var jobDocumentDelivery = Factory.New<JobDocumentDelivery>();
			AssertRecipientsAsString(jobDocumentDelivery, nameof(jobDocumentDelivery.JDC_CarbonCopyRecipientsAsString), jobDocumentDelivery.CarbonCopyRecipients);
		}

		public void TestJDC_BlindCarbonCopyRecipientsAsString()
		{
			var jobDocumentDelivery = Factory.New<JobDocumentDelivery>();
			AssertRecipientsAsString(jobDocumentDelivery, nameof(jobDocumentDelivery.JDC_BlindCarbonCopyRecipientsAsString), jobDocumentDelivery.BlindCarbonCopyRecipients);
		}

		void AssertRecipientsAsString(JobDocumentDelivery jobDocumentDelivery, string propertyName, JobDocumentDeliveryCopyRecipientCollection collection)
		{
			jobDocumentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			jobDocumentDelivery[propertyName] = "dexter@morgan.com, debra@morgan.com";
			Assert($"{propertyName} should be Readonly", jobDocumentDelivery.FindPropertyInfo(propertyName).ReadOnly);
			AssertEquals(ZString.Empty, jobDocumentDelivery[propertyName]);

			jobDocumentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			Assert($"{propertyName} should not be Readonly", !jobDocumentDelivery.FindPropertyInfo(propertyName).ReadOnly);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(collection, r => r.JDR_EmailAddress == "dexter@morgan.com");
			AssertCollectionContains(collection, r => r.JDR_EmailAddress == "debra@morgan.com");
		}

		public void TestEmailToRecipientsAreDeletedUponSavingWhenDisabled()
		{
			var jobDocumentDelivery = Factory.New<JobDocumentDelivery>();
			AssertRecipientsAreDeletedUponSavingWhenDisabled(jobDocumentDelivery, nameof(jobDocumentDelivery.JDC_EmailToRecipientsAsString), jobDocumentDelivery.EmailToRecipients);
		}

		public void TestCarbonCopyRecipientsAreDeletedUponSavingWhenDisabled()
		{
			var jobDocumentDelivery = Factory.New<JobDocumentDelivery>();
			AssertRecipientsAreDeletedUponSavingWhenDisabled(jobDocumentDelivery, nameof(jobDocumentDelivery.JDC_CarbonCopyRecipientsAsString), jobDocumentDelivery.CarbonCopyRecipients);
		}

		public void TestBlindCarbonCopyRecipientsAreDeletedUponSavingWhenDisabled()
		{
			var jobDocumentDelivery = Factory.New<JobDocumentDelivery>();
			AssertRecipientsAreDeletedUponSavingWhenDisabled(jobDocumentDelivery, nameof(jobDocumentDelivery.JDC_BlindCarbonCopyRecipientsAsString), jobDocumentDelivery.BlindCarbonCopyRecipients);
		}

		void AssertRecipientsAreDeletedUponSavingWhenDisabled(JobDocumentDelivery jobDocumentDelivery, string propertyName, JobDocumentDeliveryCopyRecipientCollection collection)
		{
			jobDocumentDelivery.JDC_DocumentGroup = ContactType.All.ToString();
			jobDocumentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			jobDocumentDelivery[propertyName] = "dexter@morgan.com, debra@morgan.com";
			Factory.Save();
			Assert($"{propertyName} should not be Readonly", !jobDocumentDelivery.FindPropertyInfo(propertyName).ReadOnly);
			AssertEquals(2, collection.Count);

			jobDocumentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			Assert($"{propertyName} should be Readonly", jobDocumentDelivery.FindPropertyInfo(propertyName).ReadOnly);
			Factory.Save();
			AssertEquals(0, collection.Count);
		}

		public void TestEmailToCheckRecipientsWhensWhenSettingDeliveryMethod()
		{
			var jobDocumentDelivery = Factory.New<JobDocumentDelivery>();
			AssertCheckRecipientsWhenSettingDeliveryMethod(jobDocumentDelivery, nameof(jobDocumentDelivery.JDC_EmailToRecipientsAsString), true);
		}

		public void TestCarbonCopyCheckRecipientsWhensWhenSettingDeliveryMethod()
		{
			var jobDocumentDelivery = Factory.New<JobDocumentDelivery>();
			AssertCheckRecipientsWhenSettingDeliveryMethod(jobDocumentDelivery, nameof(jobDocumentDelivery.JDC_CarbonCopyRecipientsAsString), false);
		}

		public void TestBlindCarbonCopyCheckRecipientsWhensWhenSettingDeliveryMethod()
		{
			var jobDocumentDelivery = Factory.New<JobDocumentDelivery>();
			AssertCheckRecipientsWhenSettingDeliveryMethod(jobDocumentDelivery, nameof(jobDocumentDelivery.JDC_BlindCarbonCopyRecipientsAsString), false);
		}

		void AssertCheckRecipientsWhenSettingDeliveryMethod(JobDocumentDelivery jobDocumentDelivery, string propertyName, bool errorIfEmpty)
		{
			jobDocumentDelivery.JDC_DocumentGroup = ContactType.All.ToString();
			jobDocumentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			jobDocumentDelivery[propertyName] = ZString.Empty;

			var not = errorIfEmpty ? string.Empty : "not ";
			AssertEquals($"{propertyName} should {not} have validation errors if the email address is empty", errorIfEmpty, jobDocumentDelivery.FindPropertyInfo(propertyName).HasErrors());

			jobDocumentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			AssertEquals($"{propertyName} should have no validation errors when the delivery method is not Email", false, jobDocumentDelivery.FindPropertyInfo(propertyName).HasErrors());
		}

		public void TestDelete()
		{
			var jobDocumentDelivery = Factory.NewWithValidTestData<JobDocumentDelivery>();
			jobDocumentDelivery.EmailToRecipients.AddNew();
			jobDocumentDelivery.CarbonCopyRecipients.AddNew();
			jobDocumentDelivery.BlindCarbonCopyRecipients.AddNew();

			AssertEquals(1, jobDocumentDelivery.EmailToRecipients.Count);
			AssertEquals(1, jobDocumentDelivery.CarbonCopyRecipients.Count);
			AssertEquals(1, jobDocumentDelivery.BlindCarbonCopyRecipients.Count);

			jobDocumentDelivery.Delete();
			AssertEquals(0, jobDocumentDelivery.EmailToRecipients.Count);
			AssertEquals(0, jobDocumentDelivery.CarbonCopyRecipients.Count);
			AssertEquals(0, jobDocumentDelivery.BlindCarbonCopyRecipients.Count);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var jobDocumentDelivery = Factory.NewWithValidTestData<JobDocumentDelivery>();
			jobDocumentDelivery.JDC_DocumentGroup = ContactType.All.ToString();
			return jobDocumentDelivery;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		#endregion
	}
}
