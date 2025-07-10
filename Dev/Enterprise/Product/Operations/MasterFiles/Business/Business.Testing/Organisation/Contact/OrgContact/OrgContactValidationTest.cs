using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.ServiceManager;
using Enterprise.MasterFiles.Business.Common;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgContactValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOC_PER_FirstSave()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();

			contact.OC_ContactName = "aaa";
			contact.Validation.ValidateAll();

			AssertNoErrors(contact.OC_PERInfo);
		}

		public void TestCheckOC_PER_NonFirstSave()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();

			contact.OC_ContactName = "aaa";

			Factory.Save();
			contact.OC_PER = ZGuid.Empty;
			contact.Validation.ValidateAll();
			AssertHasErrors(contact.OC_PERInfo);
		}

		public void TestCheckOC_IsActive()
		{
			var message = "You cannot De-activate this contact because it is used in the allocated contacts as a PGA or USF Contact.";

			var org = Factory.New<OrgHeader>();
			var contact = org.Contacts.AddNew();
			var alloc = contact.Allocations.AddNew();

			alloc.PC_Type = OrgConstants.ContactAllocationType.CAPGA;
			contact.OC_IsActive = false;
			AssertHasError(contact.OC_IsActiveInfo, message);

			alloc.PC_Type = OrgConstants.ContactAllocationType.USPGA;
			contact.Validation.ValidateOC_IsActive();

			AssertHasError(contact.OC_IsActiveInfo, message);

			alloc.PC_Type = OrgConstants.ContactAllocationType.USFSV;
			contact.Validation.ValidateOC_IsActive();

			AssertHasError(contact.OC_IsActiveInfo, message);

			contact.OC_IsActive = true;
			AssertNoError(contact.OC_IsActiveInfo, message);

			alloc.PC_Type = OrgConstants.ContactAllocationType.NZCustoms;
			contact.OC_IsActive = false;
			AssertNoError(contact.OC_IsActiveInfo, message);
		}

		public void TestCheckOC_ContactSource()
		{
			OrgContact contact = Factory.New<OrgContact>();
			contact.OC_ContactSource = "...";
			AssertHasErrors("Contact Source", contact.OC_ContactSourceInfo);

			contact.OC_ContactSource = "Website";
			AssertNoErrors("Contact Source", contact.OC_ContactSourceInfo);
		}

		public void TestCheckOC_JobCategory()
		{
			var jobCategories = new CodeDescriptionBoolCollection(OrgContactSchema.OC_JobCategory.MaxLength);
			jobCategories.Add("TWW", (NoResString)"The White Wizard", true);
			jobCategories.Add("TGW", (NoResString)"The Grey Wizard", false);
			OrganisationsDataRegistry.Instance.ContactJobCategories.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, jobCategories);

			Contact.OC_JobCategory = "Ringbearer";
			AssertNoErrors("Job category is invalid but only a warning is shown.", Contact.OC_JobCategoryInfo);
			AssertHasWarning("Job Category invalid warning.", Contact.OC_JobCategoryInfo, "Chosen Job Category is not defined in the Registry. Please define it or use the an Operation Action to change all Organization Contacts with this category to valid one.");

			Contact.OC_JobCategory = "";
			AssertNoErrors("Job Category not mandatory by default", Contact.OC_JobCategoryInfo);

			Contact.OC_JobCategory = "TGW";
			AssertNoErrors("Job category is completely valid", Contact.OC_JobCategoryInfo);
			AssertNoWarnings("Job category is completely valid", Contact.OC_JobCategoryInfo);

			OrganisationsDataRegistry.Instance.ContactJobCategoryMandatory = true;
			Contact.OC_JobCategory = "";
			AssertHasErrors("Now mandatory in registry", Contact.OC_JobCategoryInfo);

			Contact.OC_JobCategory = "TWW";
			AssertNoErrors("Job category is completely valid", Contact.OC_JobCategoryInfo);
			AssertNoWarnings("Job category is completely valid", Contact.OC_JobCategoryInfo);
		}

		public void TestCheckOC_Language()
		{
			Contact.OC_Language = "";
			Assert("An error is given when language is left blank", Contact.OC_LanguageInfo.HasErrors());

			Contact.OC_Language = "ZZZ";
			Assert("An error is given when language is invalid", Contact.OC_LanguageInfo.HasErrors());
		}

		public void TestValidateOC_Birthday()
		{
			Contact.OC_Birthday = ZDateTime.Now.Date.AddYears(-20);
			AssertNoErrors("No error expected for today minus 20 years", Contact.OC_BirthdayInfo);

			Contact.OC_Birthday = ZDateTime.Now.Date;
			AssertNoErrors("No error expected for todays date", Contact.OC_BirthdayInfo);

			Contact.OC_Birthday = ZDateTime.Now.Date.AddDays(1);
			AssertHasError("Date of Birth should have errors", Contact.OC_BirthdayInfo, "Birthdate cannot be in the future.");

			Contact.OC_Birthday = ZDate.Invalid;
			AssertHasError("Date of Birth should have errors", Contact.OC_BirthdayInfo, "Please enter a valid Birthday.");
		}

		public void TestCheckOC_AttachmentType()
		{
			Contact.OC_NotifyMode = Constants.ContactNotifyModes.Fax;
			Contact.OC_AttachmentType = "WWW";
			Assert("No attachment type required as notify mode not email. No error expected", !Contact.OC_AttachmentTypeInfo.HasErrors());

			Contact.OC_NotifyMode = Constants.ContactNotifyModes.Email;
			Contact.OC_AttachmentType = "";
			Assert("Valid attachment type required as notify mode is email. Error expected", Contact.OC_AttachmentTypeInfo.HasErrors());

			Contact.OC_AttachmentType = "XXX";
			Assert("Valid attachment type required as notify mode is email. Error expected", Contact.OC_AttachmentTypeInfo.HasErrors());

			Contact.OC_AttachmentType = OrgConstants.AttachmentType.TIF;
			Assert("Valid attachment type required as notify mode is email. No error expected", !Contact.OC_AttachmentTypeInfo.HasErrors());

			Contact.OC_NotifyMode = Constants.ContactNotifyModes.EPrint;
			Contact.OC_AttachmentType = "";
			Assert("Valid attachment type required as notify mode is email. Error expected", Contact.OC_AttachmentTypeInfo.HasErrors());

			Contact.OC_AttachmentType = "YYY";
			Assert("Valid attachment type required as notify mode is email. Error expected", Contact.OC_AttachmentTypeInfo.HasErrors());

			Contact.OC_AttachmentType = OrgConstants.AttachmentType.TIF;
			Assert("Valid attachment type required as notify mode is email. No error expected", !Contact.OC_AttachmentTypeInfo.HasErrors());
		}

		public void TestCheckOC_ContactName()
		{
			Contact.OC_ContactName = "X";
			Contact.OC_ContactName = "";
			var info = Contact.OC_ContactNameInfo;
			AssertHasErrors("No contact name entered, error expected", info);

			Contact.OC_ContactName = "Test";
			AssertNoNotifications("Contact name entered, no notification expected", info);

			Contact.OC_Language = Core.Constants.Languages.English;
			Contact.OC_ContactName = (ZString)"abc";
			AssertNoNotifications("No notifications as value was english", info);

			Contact.OC_ContactName = (ZString)char.ToString((char)300);
			AssertHasErrors("Errors as value was not english and contact was marked as english", info);

			Factory.Save();
			Contact.Validation.ValidateOC_ContactName();
			AssertNoErrors("No errors if value is in database", info);
			AssertHasWarnings("Warning as value is unchanged, and was not english and contact was marked as english", info);

			Contact.OC_ContactName = new ZString("a" + (char)300);
			AssertHasErrors("Errors as value was not english, has changes, and contact was marked as english", info);

			Contact.OC_Language = Core.Constants.Languages.Gujarati;
			Contact.OC_ContactName = (ZString)char.ToString((char)269);
			AssertNoNotifications("No errors as value was not english, but neither was contact", info);

			Contact.Person.ValidateDuplicationResult(true);
			Contact.Validation.ValidateOC_ContactName();
			AssertHasWarnings("The contact details you have entered resulted in potential duplicates. Please confirm that they are actual duplicates.", info);
		}

		public void TestCheckContactNameIsUnique()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Contact1";
			OrgContact contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Contact2";
			OrgContact contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "Contact3";

			Assert("Contact1 - no error expected", !contact1.OC_ContactNameInfo.HasErrors());
			Assert("Contact2 - no error expected", !contact2.OC_ContactNameInfo.HasErrors());
			Assert("Contact3 - no error expected", !contact3.OC_ContactNameInfo.HasErrors());

			contact3.OC_ContactName = "Contact1";
			Assert("Contact1 - error expected (same name)", contact1.OC_ContactNameInfo.HasErrors());
			Assert("Contact2 - no error expected", !contact2.OC_ContactNameInfo.HasErrors());
			Assert("Contact3 - error expected (same name)", contact3.OC_ContactNameInfo.HasErrors());

			contact3.OC_ContactName = "Contact3";
			Assert("Contact1 - no error expected", !contact1.OC_ContactNameInfo.HasErrors());
			Assert("Contact2 - no error expected", !contact2.OC_ContactNameInfo.HasErrors());
			Assert("Contact3 - no error expected", !contact3.OC_ContactNameInfo.HasErrors());

			using (contact3.GetValidationSuspender())
			{
				contact3.OC_ContactName = "Contact1";
				Assert("Contact1 - no error expected since validation suspended", !contact1.OC_ContactNameInfo.HasErrors());
				Assert("Contact3 - no error expected since validation suspended", !contact3.OC_ContactNameInfo.HasErrors());
			}
			contact1.Validation.ValidateOC_ContactName();
			contact3.Validation.ValidateOC_ContactName();
			Assert("Contact1 - error expected (same name)", contact1.OC_ContactNameInfo.HasErrors());
			Assert("Contact3 - error expected (same name)", contact3.OC_ContactNameInfo.HasErrors());
		}

		public void TestCheckOC_Email()
		{
			OrgDocument doc = Contact.Documents.AddNew();
			doc.OD_DeliverBy = Constants.ContactNotifyModes.DoNotDeliver;
			Contact.Validation.ValidateOC_Email();
			AssertNoRowErrors("Email address not required so no errors", Contact);

			doc.OD_DeliverBy = Constants.ContactNotifyModes.Email;
			Contact.Validation.ValidateOC_Email();
			AssertHasRowError("Email address required so error expected", Contact, "Please enter an email address to be used for correspondence with this Organization.");

			Contact.OC_Email = "test invalid email";
			AssertHasErrors("Invalid email address so error expected", Contact.OC_EmailInfo);

			Contact.OC_Email = "test@test.com";
			AssertNoRowErrors("Valid email address so no error expected", Contact);

			Contact.ParentOrg.MainAddress.OA_Email = "orgemail@test.com";
			Contact.OC_Email = "";
			AssertHasRowWarning("No email address, but org has one, so warning expected", Contact, "The main email address from the details page of this organization will be used for correspondence with this Organization.");

			Contact.Documents.RemoveAndDeleteAll();
			AssertNoRowErrors("No documents and no email address, should be no errors", Contact);

			Contact.OC_Email = "invalid email";
			AssertHasErrors("Invalid email address so error expected", Contact.OC_EmailInfo);

			Contact.OC_Email = "zappoo@example.com";
			AssertNoRowErrors("Valid email address, should have no error", Contact);

			Contact.Subscriptions.AddNew();
			var subscription = Contact.Subscriptions.AddNew();
			Contact.Validation.ValidateOC_Email();
			AssertHasErrors("Subscriptions error", Contact.OC_EmailInfo);
			AssertHasRowError("Subscriptions error", Contact, "You cannot change main e-mail address with errors in Subscriptions.");

			subscription.Delete();
			Contact.Validation.ValidateOC_Email();
			AssertNoErrors("Subscriptions are fine!", Contact.OC_EmailInfo);
			AssertNoRowErrors("Subscriptions are fine!", Contact);

			var scheduleTaskRecipient = (IStmScheduleTaskRecipient)Factory.NewWithValidTestData(ObjectFactory.GetType<IStmScheduleTaskRecipient>());
			scheduleTaskRecipient.S6_DeliveryMethod = Constants.ContactNotifyModes.Email;
			scheduleTaskRecipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Contact;
			scheduleTaskRecipient.S6_OC = Contact.PK;
			scheduleTaskRecipient.S6_EmailToRecipientsAsString = "";

			Factory.Save();

			Contact.OC_Email = ZString.Empty;
			AssertHasErrors("Can not set email address to empty when it was one of the Schedule Task Recipient", Contact.OC_EmailInfo);

			Contact.OC_Email = "test@test.com";
			AssertNoErrors(Contact.OC_EmailInfo);
		}

		public void TestCheckOC_NotifyMode()
		{
			Contact.OC_NotifyMode = "";
			Assert("OC_NotifyMode not entered, error expected", Contact.OC_NotifyModeInfo.HasErrors());

			Contact.OC_NotifyMode = "WWW";
			Assert("Invalid code entered, error expected", Contact.OC_NotifyModeInfo.HasErrors());

			Contact.OC_NotifyMode = Constants.ContactNotifyModes.Fax;
			Assert("Valid code entered, no error expected", !Contact.OC_NotifyModeInfo.HasErrors());

			Contact.OC_NotifyMode = Constants.ContactNotifyModes.DoNotDeliver;
			Assert("Valid code entered, no error expected", !Contact.OC_NotifyModeInfo.HasErrors());

			Contact.OC_NotifyMode = Constants.ContactNotifyModes.Electronic;
			Assert("Code only valid for ContactToSuppressDocuments, error expected", Contact.OC_NotifyModeInfo.HasErrors());
		}

		public void TestCheckOC_Phone_Formatted()
		{
			Contact.OC_Phone_Formatted = "0296";
			AssertHasErrors("Incorrect format", Contact.OC_Phone_FormattedInfo);

			Contact.OC_Phone_Formatted = "0296654455";
			AssertNoErrors("Valid phone number, no errors expected", Contact.OC_Phone_FormattedInfo);

			Contact.OC_Phone_Formatted = "5435JJJ";
			AssertHasErrors("Contains invalid characters, errors expected", Contact.OC_Phone_FormattedInfo);

			Contact.OC_Phone_IsManuallyVerified = true;
			AssertNoErrors("Number is invalid but manually verified, no errors expected", Contact.OC_Phone_FormattedInfo);
		}

		public void TestCheckOC_Fax_Formatted()
		{
			Contact.OC_Fax_Formatted = "0296";
			AssertHasErrors("Incorrect format", Contact.OC_Fax_FormattedInfo);

			Contact.OC_Fax_Formatted = "0296654455";
			AssertNoErrors("Valid Fax number, no error expected", Contact.OC_Fax_FormattedInfo);

			Contact.OC_Fax_Formatted = "0296654455 SYD";
			AssertHasErrors("Invalid Fax number, error expected", Contact.OC_Fax_FormattedInfo);

			Contact.OC_Fax_IsManuallyVerified = true;
			AssertNoErrors("Number is invalid but manually verified, no errors expected", Contact.OC_Fax_FormattedInfo);
		}

		public void TestCheckOC_Fax_Formatted_Required()
		{
			var document = Contact.Documents.AddNew();
			document.OD_DeliverBy = Constants.ContactNotifyModes.DoNotDeliver;
			Contact.Validation.ValidateOC_Fax_Formatted();
			AssertNoRowErrors("Fax number not required so no errors", Contact);

			document.OD_DeliverBy = Constants.ContactNotifyModes.Fax;
			Contact.Validation.ValidateOC_Fax_Formatted();
			AssertHasRowError("Fax number required so error expected", Contact, "Please enter a fax number to be used for delivering documents to this Organization.");

			Contact.OC_Fax_Formatted = "0299993333";
			AssertNoRowErrors("Fax number entered so no error expected", Contact);

			Contact.ParentOrg.MainAddress.OA_Fax_Formatted = "0299991111";
			Contact.OC_Fax_Formatted = "";
			AssertHasRowWarning("No Fax number, but org has one, so warning expected", Contact, "The fax number from the main details page of this organization will be used for delivering documents to this Organization.");
		}

		public void TestCheckOC_Mobile_Formatted()
		{
			Contact.OC_Mobile_Formatted = "0296";
			AssertHasErrors("Incorrect format", Contact.OC_Mobile_FormattedInfo);

			Contact.OC_Mobile_Formatted = "0404555666";
			AssertNoErrors("Valid Mobile number", Contact.OC_Mobile_FormattedInfo);

			Contact.OC_Mobile_Formatted = "0296654455 SYD";
			AssertHasErrors("Invalid characters entered", Contact.OC_Mobile_FormattedInfo);

			Contact.OC_Mobile_IsManuallyVerified = true;
			AssertNoErrors("Number is invalid but manually verified, no errors expected", Contact.OC_Mobile_FormattedInfo);
		}

		public void TestCheckOC_HomePhone_Formatted()
		{
			Contact.OC_HomePhone_Formatted = "0296";
			AssertHasErrors("Incorrect format", Contact.OC_HomePhone_FormattedInfo);

			Contact.OC_HomePhone_Formatted = "0296654455";
			AssertNoErrors("Valid HomePhone number", Contact.OC_HomePhone_FormattedInfo);

			Contact.OC_HomePhone_Formatted = "0296654455 SYD";
			AssertHasErrors("Invalid characters entered", Contact.OC_HomePhone_FormattedInfo);

			Contact.OC_HomePhone_IsManuallyVerified = true;
			AssertNoErrors("Number is invalid but manually verified, no errors expected", Contact.OC_HomePhone_FormattedInfo);
		}

		public void TestCheckOC_Pager_Formatted()
		{
			Contact.OC_Pager_Formatted = "0296";
			AssertHasErrors("Incorrect format", Contact.OC_Pager_FormattedInfo);

			Contact.OC_Pager_Formatted = "0296654455";
			AssertNoErrors("Valid Pager number", Contact.OC_Pager_FormattedInfo);

			Contact.OC_Pager_Formatted = "0296654455 SYD";
			AssertHasErrors("Invalid characters entered", Contact.OC_Pager_FormattedInfo);

			Contact.OC_Pager_IsManuallyVerified = true;
			AssertNoErrors("Number is invalid but manually verified, no errors expected", Contact.OC_Pager_FormattedInfo);
		}

		public void TestCheckOC_OtherPhone_Formatted()
		{
			Contact.OC_OtherPhone_Formatted = "0296";
			AssertHasErrors("Incorrect format", Contact.OC_OtherPhone_FormattedInfo);

			Contact.OC_OtherPhone_Formatted = "0296654455";
			AssertNoErrors("Valid OtherPhone number", Contact.OC_OtherPhone_FormattedInfo);

			Contact.OC_OtherPhone_Formatted = "0296654455 SYD";
			AssertHasErrors("Invalid characters entered", Contact.OC_OtherPhone_FormattedInfo);

			Contact.OC_OtherPhone_IsManuallyVerified = true;
			AssertNoErrors("Number is invalid but manually verified, no errors expected", Contact.OC_OtherPhone_FormattedInfo);
		}

		public void TestOC_YearJoinedCompanyIsValidZDateTimeRange()
		{
			OrgContactValidationForTest validation = new OrgContactValidationForTest(Contact);

			Contact.OC_YearJoinedCompany = ZDateTime.Today;
			validation.ValidateOC_YearJoinedCompany();
			AssertNoErrors(Contact.OC_YearJoinedCompanyInfo);

			Contact.OC_YearJoinedCompany = ZDateTime.Today.AddYears(-50);
			validation.ValidateOC_YearJoinedCompany();
			AssertNoErrors(Contact.OC_YearJoinedCompanyInfo);

			Contact.OC_YearJoinedCompany = ZDateTime.Today.AddYears(-51);
			validation.ValidateOC_YearJoinedCompany();
			AssertHasError(Contact.OC_YearJoinedCompanyInfo, OrgContactValidationForTest.CannotHaveYearJoined50YearsInPast);

			Contact.OC_YearJoinedCompany = ZDateTime.Today;
			validation.ValidateOC_YearJoinedCompany();
			AssertNoErrors(Contact.OC_YearJoinedCompanyInfo);

			Contact.OC_YearJoinedCompany = ZDateTime.Today.AddMonths(6);
			validation.ValidateOC_YearJoinedCompany();
			AssertNoErrors(Contact.OC_YearJoinedCompanyInfo);

			Contact.OC_YearJoinedCompany = ZDateTime.Today.AddYears(7);
			validation.ValidateOC_YearJoinedCompany();
			AssertHasError(Contact.OC_YearJoinedCompanyInfo, OrgContactValidationForTest.CannotHaveYearJoined6MonthsInFuture);
		}

		public void TestOC_YearJoinedIndustryIsValidZDateTimeRange()
		{
			OrgContactValidationForTest validation = new OrgContactValidationForTest(Contact);

			Contact.OC_YearJoinedIndustry = ZDateTime.Today;
			validation.ValidateOC_YearJoinedIndustry();
			AssertNoErrors(Contact.OC_YearJoinedIndustryInfo);

			Contact.OC_YearJoinedIndustry = ZDateTime.Today.AddYears(-50);
			validation.ValidateOC_YearJoinedIndustry();
			AssertNoErrors(Contact.OC_YearJoinedIndustryInfo);

			Contact.OC_YearJoinedIndustry = ZDateTime.Today.AddYears(-51);
			validation.ValidateOC_YearJoinedIndustry();
			AssertHasError(Contact.OC_YearJoinedIndustryInfo, OrgContactValidationForTest.CannotHaveYearJoined50YearsInPast);

			Contact.OC_YearJoinedIndustry = ZDateTime.Today;
			validation.ValidateOC_YearJoinedIndustry();
			AssertNoErrors(Contact.OC_YearJoinedIndustryInfo);

			Contact.OC_YearJoinedIndustry = ZDateTime.Today.AddMonths(6);
			validation.ValidateOC_YearJoinedIndustry();
			AssertNoErrors(Contact.OC_YearJoinedIndustryInfo);

			Contact.OC_YearJoinedIndustry = ZDateTime.Today.AddYears(7);
			validation.ValidateOC_YearJoinedIndustry();
			AssertHasError(Contact.OC_YearJoinedIndustryInfo, OrgContactValidationForTest.CannotHaveYearJoined6MonthsInFuture);
		}

		public void TestYearJoinedMessagesMatchConstantValues()
		{
			AssertEquals("If this fails, then you need to update the const message CannotHaveYearJoined50YearsInPast to match the const YearJoinedPastMaxYears", "Year Joined Company can be at most 50 years prior to today's date.", OrgContactValidationForTest.CannotHaveYearJoined50YearsInPast);
			AssertEquals("If this fails, then you need to update the const message CannotHaveYearJoined6MonthsInFuture to match the const YearJoinedFutureMaxMonths", "Year Joined Company cannot be set to more than 6 months from today's date.", OrgContactValidationForTest.CannotHaveYearJoined6MonthsInFuture);
		}

		public void TestCheckOC_OA_OrgAddress()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgAddress orgAddress = org.Addresses.AddNew();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Contact1";
			contact.OC_OA_OrgAddress = orgAddress.PK;
			OrgContactValidation validation = new OrgContactValidation(contact);

			orgAddress.OA_IsActive = true;
			validation.ValidateOC_OA_OrgAddress();
			Assert("Contact - no error expected", !contact.OC_OA_OrgAddressInfo.HasErrors());

			orgAddress.OA_IsActive = false;
			validation.ValidateOC_OA_OrgAddress();
			Assert("Contact - error expected (Only Active address can be set as Contact's address)", contact.OC_OA_OrgAddressInfo.HasErrors());
		}

		public void TestCheckActiveWebUserEmailIsUnique()
		{
			var expectedError = (NoResString)"This email is already in use by another active Contact with web access.";
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG1";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "email1@emailserver.zz";
			contact1.OC_WebAccessEnabled = true;
			contact1.OC_IsActive = true;
			contact1.OC_ContactName = "contact1";

			var contact2 = org.Contacts.AddNew();
			contact2.OC_Email = "email2@emailserver.zz";
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_IsActive = true;
			contact2.OC_ContactName = "contact2";

			Assert(!contact1.OC_WebAccessEnabledInfo.HasErrors());
			Assert(!contact1.OC_EmailInfo.HasErrors());
			Assert(!contact1.OC_IsActiveInfo.HasErrors());
			Assert(!contact2.OC_WebAccessEnabledInfo.HasErrors());
			Assert(!contact2.OC_EmailInfo.HasErrors());
			Assert(!contact2.OC_IsActiveInfo.HasErrors());

			contact2.OC_Email = "email1@emailserver.zz";
			Assert(contact2.OC_EmailInfo.HasError(expectedError));

			contact2.OC_WebAccessEnabled = false;
			contact2.Validation.ValidateAll();
			Assert(!contact2.OC_WebAccessEnabledInfo.HasErrors());
			Assert(!contact2.OC_EmailInfo.HasErrors());
			Assert(!contact2.OC_IsActiveInfo.HasErrors());

			contact2.OC_WebAccessEnabled = true;
			contact2.OC_IsActive = false;
			contact2.Validation.ValidateAll();
			Assert(!contact2.OC_WebAccessEnabledInfo.HasErrors());
			Assert(!contact2.OC_EmailInfo.HasErrors());
			Assert(!contact2.OC_IsActiveInfo.HasErrors());

			contact2.OC_IsActive = true;
			contact2.OC_Email = "email2@emailserver.zz";
			contact2.Validation.ValidateAll();
			Assert(!contact2.OC_WebAccessEnabledInfo.HasErrors());
			Assert(!contact2.OC_EmailInfo.HasErrors());
			Assert(!contact2.OC_IsActiveInfo.HasErrors());

			contact1.OC_WebAccessEnabled = false;
			contact2.OC_Email = "email1@emailserver.zz";
			contact1.Validation.ValidateAll();
			contact2.Validation.ValidateAll();
			Assert(!contact1.OC_WebAccessEnabledInfo.HasErrors());
			Assert(!contact1.OC_EmailInfo.HasErrors());
			Assert(!contact1.OC_IsActiveInfo.HasErrors());
			Assert(!contact2.OC_WebAccessEnabledInfo.HasErrors());
			Assert(!contact2.OC_EmailInfo.HasErrors());
			Assert(!contact2.OC_IsActiveInfo.HasErrors());

			contact2.OC_WebAccessEnabled = true;
			contact2.OC_IsActive = true;
			contact1.Validation.ValidateAll();
			contact2.Validation.ValidateAll();
			Assert(!contact1.OC_WebAccessEnabledInfo.HasErrors());
			Assert(!contact1.OC_EmailInfo.HasErrors());
			Assert(!contact1.OC_IsActiveInfo.HasErrors());
			Assert(!contact2.OC_WebAccessEnabledInfo.HasErrors());
			Assert(!contact2.OC_EmailInfo.HasErrors());
			Assert(!contact2.OC_IsActiveInfo.HasErrors());

			Factory.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var contact1InFactory2 = factory2.Load<OrgContact>(contact1.PK);
			AssertEquals(false, contact1InFactory2.ParentOrg.ContactsLoaded.Any());
			contact1InFactory2.OC_WebAccessEnabled = true;
			Assert(contact1InFactory2.OC_WebAccessEnabledInfo.HasError(expectedError));
			AssertEquals(false, contact1InFactory2.ParentOrg.ContactsLoaded.Any());
		}

		public void TestCheckOC_Gender()
		{
			var contact = Factory.New<OrgContact>();
			contact.OC_Gender = ZString.Empty;
			AssertNoErrors("Blank gender should be permitted", contact.OC_GenderInfo);

			contact.OC_Gender = "Y";
			AssertHasErrors("Invalid gender should have error", contact.OC_GenderInfo);

			contact.OC_Gender = Core.Constants.Genders.Woman;
			AssertNoErrors("Valid gender should not have error", contact.OC_GenderInfo);

			Env.Security.OrgContactViewPersonalInformation.IsAllowed = false;
			contact.Validation.ValidateOC_Gender();
			AssertNoErrors("Gender viewed denied message should not have error", contact.OC_GenderInfo);
		}

		public void TestCheckOC_RN_NKNationalityCodeISO()
		{
			var contact = Factory.New<OrgContact>();
			contact.OC_RN_NKNationality = ZString.Empty;
			AssertNoErrors("Blank nationality should be permitted", contact.OC_RN_NKNationalityInfo);

			contact.OC_RN_NKNationality = "ZZ";
			AssertHasErrors("Invalid nationality should have error", contact.OC_RN_NKNationalityInfo);

			contact.OC_RN_NKNationality = Core.Constants.CountryCodes.France;
			AssertNoErrors("Valid nationality should not have error", contact.OC_RN_NKNationalityInfo);

			Env.Security.PersonIntelligenceViewNationality.IsAllowed = false;
			contact.Validation.ValidateOC_RN_NKNationality();
			AssertNoErrors("Nationality viewed denied message should not have error", contact.OC_RN_NKNationalityInfo);
		}

		public void TestCheck_GenderAndNationality_NonWesternEuropean()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Gender = "F";
			contact.OC_RN_NKNationality = "AU";

			var nonWesternEuropeanUser = Factory.NewWithValidTestData<GlbStaff>();
			nonWesternEuropeanUser.GS_WorkingLanguage = SharedConstants.Languages.Turkish;

			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(nonWesternEuropeanUser, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				Env.Security.OrgContactViewPersonalInformation.IsAllowed = false;

				contact.Validation.ValidateOC_RN_NKNationality();
				AssertNoErrors("should not have western european character error of gender", contact.OC_GenderInfo);
				AssertNoErrors("should not have western european character error of nationality", contact.OC_RN_NKNationalityInfo);
			}
		}

		public void TestDeletionNotAllPasswordHashingFieldsShouldReportError()
		{
			var orgContact1 = GetNewContactWithHashedPassword("password");
			orgContact1.OC_PasswordSalt = ZBlob.Empty;
			orgContact1.Validation.ValidateOC_PasswordSalt();
			AssertHasErrors(orgContact1.OC_PasswordSaltInfo);

			var orgContact2 = GetNewContactWithHashedPassword("password");
			orgContact2.OC_PasswordHashIterations = ZInt.Zero;
			orgContact2.Validation.ValidateOC_PasswordHashIterations();
			AssertHasErrors(orgContact2.OC_PasswordHashIterationsInfo);

			var orgContact3 = GetNewContactWithHashedPassword("password");
			orgContact3.OC_PasswordHash = ZBlob.Empty;
			orgContact3.Validation.ValidateOC_PasswordHash();
			AssertHasErrors(orgContact3.OC_PasswordHashInfo);
		}

		public void TestCheckOC_IsActiveIfWebAccessSuperseded()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Assert("Precondition", contact.OC_IsActive);
			Assert("Precondition", !contact.WebAccessSuperseded);

			contact.Validation.ValidateOC_IsActive();
			AssertNoErrors(contact.OC_IsActiveInfo);

			contact.SupersedeWebAccess();
			Assert("Precondition", !contact.OC_IsActive);
			AssertNoErrors(contact.OC_IsActiveInfo);
			Factory.Save();

			var sql = FormattableString.Invariant($"UPDATE dbo.OrgContact SET OC_IsActive = 1 WHERE OC_PK = '{contact.PK}'");
			Db.Connection.ExecuteNonQuery(sql);

			contact.Reload();
			contact.Validation.ValidateOC_IsActive();
			AssertHasError("Should not allow active superseded contact", contact.OC_IsActiveInfo, "Contacts with their Web Access Superseded cannot be Active.");
		}

		public void TestCheckInactiveAddressWhenContactIsActive()
		{
			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_Address1 = "TEST";

			OrgContact orgContact1 = orgHeader.Contacts.AddNew();
			orgContact1.OC_ContactName = "Contact 1";
			orgContact1.OC_OA_OrgAddress = orgAddress.PK;

			OrgContact orgContact2 = orgHeader.Contacts.AddNew();
			orgContact2.OC_ContactName = "Contact 2";
			orgContact2.OC_OA_OrgAddress = orgAddress.PK;

			Factory.Save();

			CombineAssertions(() =>
			{
				Assert("Precondition: Active Address", orgAddress.OA_IsActive);
				Assert("Precondition: Active Contact 1", orgContact1.OC_IsActive);
				Assert("Precondition: Active Contact 2", orgContact2.OC_IsActive);
				AssertNoError(orgHeader.Contacts[0].OC_OA_OrgAddressInfo, "This Organization Address is inactive.");
				AssertNoError(orgHeader.Contacts[0].OC_OA_OrgAddressInfo, "Only Active address can be set as Contact's address");
				AssertNoError(orgHeader.Contacts[1].OC_OA_OrgAddressInfo, "This Organization Address is inactive.");
				AssertNoError(orgHeader.Contacts[1].OC_OA_OrgAddressInfo, "Only Active address can be set as Contact's address");
			});

			orgHeader.Contacts[0].OC_IsActive = false;
			orgAddress.OA_IsActive = false;

			CombineAssertions(() =>
			{
				AssertHasWarning(orgHeader.Contacts[0].OC_OA_OrgAddressInfo, "This Organization Address is inactive.");
				AssertNoError(orgHeader.Contacts[0].OC_OA_OrgAddressInfo, "Only Active address can be set as Contact's address");
				AssertHasError(orgHeader.Contacts[1].OC_OA_OrgAddressInfo, "Only Active address can be set as Contact's address");
			});

			orgHeader.Contacts[0].OC_IsActive = true;
			orgAddress.OA_IsActive = false;

			AssertHasError(orgHeader.Contacts[0].OC_OA_OrgAddressInfo, "Only Active address can be set as Contact's address");
		}

		#region Implementation

		OrgContact GetNewContactWithHashedPassword(string password)
		{
			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.SetHashedPassword(password);
			Factory.Save();
			return orgContact;
		}

		protected override void SetUp()
		{
			base.SetUp();
			Org = Factory.NewWithValidTestData<OrgHeader>();
			Contact = Org.Contacts.AddNew();
			Org.OH_RL_NKClosestPort = "AUSYD";
		}

		OrgHeader Org;
		OrgContact Contact;

		#endregion

		#region OrgContactValidationForTest

		public class OrgContactValidationForTest : OrgContactValidation
		{
			public OrgContactValidationForTest(AutoOrgContact parent)
				: base(parent)
			{
			}

			#region Constants

			public const int YearJoinedPastMaxYearsForTest = YearJoinedPastMaxYears;
			public const int YearJoinedFutureMaxMonthsForTest = YearJoinedFutureMaxMonths;

			#endregion
		}

		#endregion
	}
}
