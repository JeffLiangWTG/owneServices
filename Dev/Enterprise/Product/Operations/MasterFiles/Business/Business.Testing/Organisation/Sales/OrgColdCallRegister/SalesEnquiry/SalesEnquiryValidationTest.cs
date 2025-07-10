using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class SalesEnquiryValidationTest : BusinessObjectValidationTestCase
	{
		#region Email Address Validation

		public void TestValidateEmailAddress()
		{
			CheckEmail("@", false);
			CheckEmail("1@", false);
			CheckEmail("@1", false);
			CheckEmail("s@", false);
			CheckEmail("@s", false);
			CheckEmail("asd@", false);
			CheckEmail("@asda", false);
			CheckEmail("asd@sad", false);
			CheckEmail("sad21@d3232", false);
			CheckEmail("@@", false);
			CheckEmail("@$@", false);
			CheckEmail("(*&LK@", false);
			CheckEmail("@#@KLJ908", false);
			CheckEmail("=-][@\';,/", false);
			CheckEmail("ds@sds,ds", false);
			CheckEmail("me@.com", false);
			CheckEmail("s@asd#.ds", true);
			CheckEmail("sda@&&.dfdsf.asdasd.))(", true);
			CheckEmail("@asdsa.com", false);
			CheckEmail("@.", false);
			CheckEmail(".@..", false);
			CheckEmail(".@...", true);
		}

		void CheckEmail(string emailAddress, bool expectItToPass)
		{
			EnquiryForTest.O1_Email = emailAddress;
			string assertionMessage = "Testing Email " + emailAddress + " - Errors: " + string.Join(", ", EnquiryForTest.O1_EmailInfo.GetErrors().GetUniqueMessageList());
			AssertEquals(assertionMessage, !expectItToPass, EnquiryForTest.O1_EmailInfo.HasErrors());
		}

		#endregion

		public void TestCheckO1_OA_LinkedAddress()
		{
			var org = Factory.New<OrgHeader>();
			var inquiry = Factory.New<SalesEnquiry>();

			CombineAssertions(() =>
			{
				inquiry.O1_OH_ConvertedToQualifiedLead = ZGuid.Empty;
				inquiry.O1_OA_LinkedAddress = ZGuid.Empty;
				AssertNoErrors("Not linked to org with no address", inquiry.O1_OA_LinkedAddressInfo);

				inquiry.O1_OH_ConvertedToQualifiedLead = org.PK;
				inquiry.O1_OA_LinkedAddress = ZGuid.Empty;
				AssertHasError("Linked to org with no address", inquiry.O1_OA_LinkedAddressInfo, "Please enter a value.");

				inquiry.O1_OH_ConvertedToQualifiedLead = org.PK;
				inquiry.O1_OA_LinkedAddress = org.MainAddress.PK;
				AssertNoErrors("Linked to org with address", inquiry.O1_OA_LinkedAddressInfo);

				inquiry.ReadOnly = true;
				inquiry.O1_OH_ConvertedToQualifiedLead = org.PK;
				inquiry.O1_OA_LinkedAddress = ZGuid.Empty;
				AssertNoErrors("Linked to org with no address but readonly", inquiry.O1_OA_LinkedAddressInfo);
			});
		}

		public void TestCheckO1_Phone()
		{
			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry.O1_PortOrCountry = "AU";

			enquiry.O1_Phone = "0296654455";
			AssertNoNotifications("Valid phone number, no notifications expected", enquiry.O1_PhoneInfo);

			enquiry.O1_Phone = "0296654455 <03>";
			AssertNoNotifications("Valid phone number, no notifications expected", enquiry.O1_PhoneInfo);

			enquiry.O1_Phone = "5435JJJ";
			AssertHasErrors("Contains invalid characters, errors expected", enquiry.O1_PhoneInfo);
		}

		public void TestCheckO1_Mobile()
		{
			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry.O1_PortOrCountry = "AU";

			enquiry.O1_Mobile = "0404555666";
			AssertNoNotifications("Valid Mobile number, no notifications expected", enquiry.O1_MobileInfo);

			enquiry.O1_Mobile = "0404555666 <03>";
			AssertHasErrors("Invalid characters entered, errors expected", enquiry.O1_MobileInfo);

			enquiry.O1_Mobile = "0296654455 SYD";
			AssertHasErrors("Invalid characters entered, errors expected", enquiry.O1_MobileInfo);
		}

		public void TestCheckO1_Fax()
		{
			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry.O1_PortOrCountry = "AU";

			enquiry.O1_Fax = "0404555666";
			AssertNoNotifications("Valid Fax number, no notifications expected", enquiry.O1_FaxInfo);

			enquiry.O1_Fax = "0404555666 <03>";
			AssertHasErrors("Invalid characters entered, errors expected", enquiry.O1_FaxInfo);

			enquiry.O1_Fax = "0296654455 SYD";
			AssertHasErrors("Invalid characters entered, errors expected", enquiry.O1_FaxInfo);
		}

		public void TestCheckO1_EnquiryType()
		{
			var enquiry = Factory.New<SalesEnquiry>();

			enquiry.O1_EnquiryType = "";
			AssertHasErrors("O1_EnquiryType is mandatory", enquiry.O1_EnquiryTypeInfo);

			enquiry.O1_EnquiryType = "ZZZ";
			AssertHasErrors("O1_EnquiryType must be in list", enquiry.O1_EnquiryTypeInfo);

			enquiry.O1_EnquiryType = "CCR";
			AssertNoErrors("Valid type, no notifications expected, O1_EnquiryType can now be CCR", enquiry.O1_EnquiryTypeInfo);

			enquiry.O1_EnquiryType = "INQ";
			AssertNoNotifications("Valid type, no notifications expected", enquiry.O1_EnquiryTypeInfo);
		}

		public void TestCheckO1_CloseReason()
		{
			var collection = new CodeDescriptionBoolCollection(3);
			collection.Add("WAT", (NoResString)"Lacks information to be taken further", true);
			collection.Add("LOL", (NoResString)"Not a serious inquiry", false);
			OrganisationsDataRegistry.Instance.SalesEnquiryCloseReasonList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			Factory.Save();

			EnquiryForTest.O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Closed;

			SetSalesEnquiryFieldsMandatoryRegistry(OrgColdCallRegisterSchema.Constants.O1_CloseReason, true);

			EnquiryForTest.O1_CloseReason = "";
			AssertHasErrors("O1_CloseReason is mandatory", EnquiryForTest.O1_CloseReasonInfo);

			EnquiryForTest.O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Converted;
			EnquiryForTest.O1_CloseReason = "";
			AssertNoErrors("O1_CloseReason is not mandatory", EnquiryForTest.O1_CloseReasonInfo);

			SetSalesEnquiryFieldsMandatoryRegistry(OrgColdCallRegisterSchema.Constants.O1_CloseReason, false);
			EnquiryForTest.O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Closed;
			EnquiryForTest.O1_CloseReason = "";
			AssertNoErrors("O1_CloseReason is not mandatory", EnquiryForTest.O1_CloseReasonInfo);

			EnquiryForTest.O1_CloseReason = "NOP";
			AssertHasErrors("Reason is invalid", EnquiryForTest.O1_CloseReasonInfo);
			AssertNoWarnings("No redundant warnings", EnquiryForTest.O1_CloseReasonInfo);

			EnquiryForTest.O1_CloseReason = "WAT";
			AssertNoErrors("Reason is completely invalid", EnquiryForTest.O1_CloseReasonInfo);
			AssertNoWarnings("Reason is completely valid", EnquiryForTest.O1_CloseReasonInfo);

			EnquiryForTest.O1_CloseReason = "LOL";
			AssertHasErrors("Invalid reason is invalid", EnquiryForTest.O1_CloseReasonInfo);
			Factory.Save();
			EnquiryForTest.Validation.ValidateAll();
			AssertNoErrors("Inactive reason is still valid but should not give an error since it's been saved before", EnquiryForTest.O1_CloseReasonInfo);
			AssertHasWarnings("Inactive reason gives a warning to show it's disabled", EnquiryForTest.O1_CloseReasonInfo);
		}

		public void TestCheckO1_JobCategory()
		{
			var warningMessage = "Chosen Job Category is not defined in the Registry. Please define it or use the an Operation Action to change all Enquiries with this category to valid one.";
			var jobCategories = new CodeDescriptionBoolCollection(OrgContactSchema.OC_JobCategory.MaxLength);
			jobCategories.Add("The White Wizard", null, true);
			jobCategories.Add("The Grey Wizard", null, true);
			OrganisationsDataRegistry.Instance.ContactJobCategories.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, jobCategories);

			EnquiryForTest.O1_JobCategory = "Ringbearer";
			AssertNoErrors("Job category is invalid but only shows and error", EnquiryForTest.O1_JobCategoryInfo);
			AssertHasWarning("Job category is invalid shows warning", EnquiryForTest.O1_JobCategoryInfo, warningMessage);

			EnquiryForTest.O1_JobCategory = "The White Wizard";
			AssertNoErrors("Job category is completely valid", EnquiryForTest.O1_JobCategoryInfo);
			AssertNoWarnings("Job category is completely valid", EnquiryForTest.O1_JobCategoryInfo);

			SalesEnquiry enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Gandalf";
			enquiry.LinkToOrganizationDirectly(org.PK, ZGuid.Empty, contact.PK);

			contact.OC_JobCategory = "Ringbearer";
			enquiry.Validation.ValidateO1_JobCategory();
			AssertNoErrors("Invalid Job category shows warning not error", enquiry.O1_JobCategoryInfo);
			AssertHasWarning("Job category is invalid shows warning", enquiry.O1_JobCategoryInfo, warningMessage);

			contact.OC_JobCategory = "The White Wizard";
			enquiry.Validation.ValidateO1_JobCategory();
			AssertNoErrors("Job category of linked contact is completely valid", enquiry.O1_JobCategoryInfo);
			AssertNoWarnings("Job category of linked contact is completely valid", enquiry.O1_JobCategoryInfo);
		}

		public void TestCheckO1_LeadSource()
		{
			var collection = new CodeDescriptionBoolRelatedItemCollection();
			collection.Add("WTF", (NoResString)"Wise Tech Forum", true);
			collection.Add("ENC", (NoResString)"Encyclopedia", false);
			OrganisationsDataRegistry.Instance.OpportunitySource.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			EnquiryForTest.O1_LeadSource = "WAT";
			AssertHasErrors("Source is invalid", EnquiryForTest.O1_LeadSourceInfo);
			AssertNoWarnings("No redundant warnings", EnquiryForTest.O1_LeadSourceInfo);

			EnquiryForTest.O1_LeadSource = "WTF";
			AssertNoErrors("Lead Source is completely invalid", EnquiryForTest.O1_LeadSourceInfo);
			AssertNoWarnings("Lead Source is completely valid", EnquiryForTest.O1_LeadSourceInfo);

			EnquiryForTest.O1_LeadSource = "ENC";
			AssertHasErrors("Invalid lead source is invalid", EnquiryForTest.O1_LeadSourceInfo);
			Factory.Save();
			EnquiryForTest.Validation.ValidateAll();
			AssertNoErrors("Inactive lead source is still valid but should not give an error since it's been saved before", EnquiryForTest.O1_LeadSourceInfo);
			AssertHasWarnings("Inactive lead source gives a warning to show it's disabled", EnquiryForTest.O1_LeadSourceInfo);
		}

		public void TestCheckO1_InterestLevel()
		{
			CodeDescriptionBoolCollection leadInterests = new CodeDescriptionBoolCollection();
			leadInterests.Add("YEA", null, true);
			leadInterests.Add("NAY", null, false);
			OrganisationsDataRegistry.Instance.SalesEnquiryLeadInterests.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, leadInterests);

			EnquiryForTest.O1_InterestLevel = "HUH";
			AssertHasErrors("Lead Interest is invalid", EnquiryForTest.O1_InterestLevelInfo);
			AssertNoWarnings("No redundant warning", EnquiryForTest.O1_InterestLevelInfo);

			EnquiryForTest.O1_InterestLevel = "YEA";
			AssertNoErrors("Lead Interest is completely valid", EnquiryForTest.O1_InterestLevelInfo);
			AssertNoWarnings("Lead Interest is completely valid", EnquiryForTest.O1_InterestLevelInfo);

			EnquiryForTest.O1_InterestLevel = "NAY";
			AssertHasErrors("Inactive Lead Interest is invalid", EnquiryForTest.O1_InterestLevelInfo);
			Factory.Save();
			EnquiryForTest.Validation.ValidateAll();
			AssertNoErrors("Inactive Lead Interest is still invalid but should not give an error since it's been saved before", EnquiryForTest.O1_InterestLevelInfo);
			AssertHasWarnings("Inactive Lead Interest gives a warning to show it's disabled", EnquiryForTest.O1_InterestLevelInfo);
		}

		public void TestNoValidationIfEnquiryReadOnly()
		{
			var enquiry = Factory.New<SalesEnquiry>();

			enquiry.O1_EnquiryType = "";
			enquiry.O1_LeadSource = "DDD";
			enquiry.O1_CompanyName = "";
			enquiry.O1_ContactName = "";
			enquiry.O1_Phone = "aaa";
			enquiry.O1_Mobile = "bbb";
			enquiry.O1_Fax = "ccc";
			enquiry.O1_Email = "ddd";
			enquiry.O1_JobCategory = "----------------";
			enquiry.O1_GS_NKRepAssigned = "(w)";
			enquiry.O1_InterestLevel = "WUT";

			enquiry.Validation.ValidateAll();
			AssertHasErrors(enquiry.O1_EnquiryTypeInfo);
			AssertHasErrors(enquiry.O1_CompanyNameInfo);
			AssertHasErrors(enquiry.O1_ContactNameInfo);
			AssertHasErrors(enquiry.O1_PhoneInfo);
			AssertHasErrors(enquiry.O1_MobileInfo);
			AssertHasErrors(enquiry.O1_FaxInfo);
			AssertHasErrors(enquiry.O1_EmailInfo);
			AssertHasWarnings(enquiry.O1_JobCategoryInfo);
			AssertHasErrors(enquiry.O1_GS_NKRepAssignedInfo);
			AssertHasErrors(enquiry.O1_InterestLevelInfo);

			enquiry.DoClose();

			enquiry.Validation.ValidateAll();
			AssertNoErrors(enquiry.O1_EnquiryTypeInfo);
			AssertNoErrors(enquiry.O1_CompanyNameInfo);
			AssertNoErrors(enquiry.O1_ContactNameInfo);
			AssertNoErrors(enquiry.O1_PhoneInfo);
			AssertNoErrors(enquiry.O1_MobileInfo);
			AssertNoErrors(enquiry.O1_FaxInfo);
			AssertNoErrors(enquiry.O1_EmailInfo);
			AssertNoErrors(enquiry.O1_JobCategoryInfo);
			AssertNoErrors(enquiry.O1_GS_NKRepAssignedInfo);
			AssertNoErrors(enquiry.O1_InterestLevelInfo);
		}

		public void TestCheckO1_OH_SourceOfLead()
		{
			var referringOrg = Factory.New<OrgHeader>();
			var enquiry = Factory.New<SalesEnquiry>();
			enquiry.O1_OH_SourceOfLead = referringOrg.PK;
			AssertNoErrors("Existing org as referral is valid", enquiry.O1_OH_SourceOfLeadInfo);

			enquiry.O1_OH_SourceOfLead = ZGuid.Invalid;
			AssertHasErrors("Invalid org as referral is invalid", enquiry.O1_OH_SourceOfLeadInfo);

			enquiry.O1_OH_SourceOfLead = ZGuid.NewZGuid();
			AssertHasErrors("New guid as referral is invalid", enquiry.O1_OH_SourceOfLeadInfo);
		}

		public void TestCheckO1_OH_ReferTo()
		{
			var referToOrg = Factory.New<OrgHeader>();
			var enquiry = Factory.New<SalesEnquiry>();
			enquiry.O1_OH_ReferTo = referToOrg.PK;
			AssertNoErrors("Existing org as ReferTo is valid", enquiry.O1_OH_ReferToInfo);

			enquiry.O1_OH_ReferTo = ZGuid.Invalid;
			AssertHasErrors("Invalid org as ReferTo is invalid", enquiry.O1_OH_ReferToInfo);

			enquiry.O1_OH_ReferTo = ZGuid.NewZGuid();
			AssertHasErrors("New guid as ReferTo is invalid", enquiry.O1_OH_ReferToInfo);
		}

		public void TestValidateReferralContactName()
		{
			var referringOrg = Factory.NewWithValidTestData<OrgHeader>();
			var activeReferringContact = referringOrg.Contacts.AddNew();
			activeReferringContact.OC_ContactName = "Jenny";
			var inactiveReferringContact = referringOrg.Contacts.AddNew();
			inactiveReferringContact.OC_ContactName = "Jenny's twin";
			inactiveReferringContact.OC_IsActive = false;

			var referToOrg = Factory.NewWithValidTestData<OrgHeader>();
			var activeReferToContact = referToOrg.Contacts.AddNew();
			activeReferToContact.OC_ContactName = "Billy";
			var inactiveReferToContact = referToOrg.Contacts.AddNew();
			inactiveReferToContact.OC_ContactName = "Billy's brother";
			inactiveReferToContact.OC_IsActive = false;

			Factory.Save();

			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry.O1_OH_SourceOfLead = ZGuid.Empty;
			enquiry.O1_OH_ReferTo = ZGuid.Empty;
			enquiry.ReferringContactName = activeReferringContact.OC_ContactName;
			enquiry.ReferToContactName = activeReferToContact.OC_ContactName;
			AssertHasErrors("Enquiry has no referring org, so cannot add referring contact. This scenario can happen via Operational Actions", enquiry.ReferringContactNameInfo);
			AssertHasErrors("Enquiry has no refer to org, so cannot add refer to contact. This scenario can happen via Operational Actions", enquiry.ReferToContactNameInfo);

			enquiry.O1_OH_SourceOfLead = ZGuid.Invalid;
			enquiry.O1_OH_ReferTo = ZGuid.Invalid;
			enquiry.ReferringContactName = activeReferringContact.OC_ContactName;
			enquiry.ReferToContactName = activeReferToContact.OC_ContactName;
			AssertHasErrors("Enquiry has invalid referring org, so cannot add referring contact", enquiry.ReferringContactNameInfo);
			AssertHasErrors("Enquiry has invalid refer to org, so cannot add refer to contact", enquiry.ReferToContactNameInfo);

			enquiry.O1_OH_SourceOfLead = referringOrg.PK;
			enquiry.O1_OH_ReferTo = referToOrg.PK;
			enquiry.ReferringContactName = activeReferringContact.OC_ContactName;
			enquiry.ReferToContactName = activeReferToContact.OC_ContactName;
			AssertNoErrors("Enquiry has referring org, so can add referring contact", enquiry.ReferringContactNameInfo);
			AssertNoErrors("Enquiry has refer to org, so can add refer to contact", enquiry.ReferToContactNameInfo);

			enquiry.ReferringContactName = inactiveReferringContact.OC_ContactName;
			enquiry.ReferToContactName = inactiveReferToContact.OC_ContactName;
			AssertHasErrors("Enquiry has referring org, but referring contact is inactive", enquiry.ReferringContactNameInfo);
			AssertHasErrors("Enquiry has refer to org, but refer to contact is inactive", enquiry.ReferToContactNameInfo);

			enquiry.ReferringContactName = "New contact";
			enquiry.ReferToContactName = "Another contact";
			AssertNoErrors("Enquiry has referring org, so can add new contact as a referring contact", enquiry.ReferringContactNameInfo);
			AssertNoErrors("Enquiry has refer to org, so can add new contact as a refer to contact", enquiry.ReferToContactNameInfo);
			enquiry.RunPreSaveValidation();
			AssertNoErrors("Still no errors after the link between the new referring contact and enquiry has been set", enquiry.ReferringContactNameInfo);
			AssertNoErrors("Still no errors after the link between the new refer to contact and enquiry has been set", enquiry.ReferToContactNameInfo);

			#region Allow existing Opportunities and Inquiries to be modified with inactive referral contacts.  

			inactiveReferringContact.OC_IsActive = true;
			inactiveReferToContact.OC_IsActive = true;
			enquiry.ReferringContactName = inactiveReferringContact.OC_ContactName;
			enquiry.ReferToContactName = inactiveReferToContact.OC_ContactName;
			Factory.Save();

			inactiveReferringContact.OC_IsActive = false;
			inactiveReferToContact.OC_IsActive = false;
			Factory.Save();

			enquiry.RunPreSaveValidation();
			AssertHasWarning(enquiry.ReferringContactNameInfo, "Select an active contact belonging to the Referring Organization.");
			AssertHasWarning(enquiry.ReferToContactNameInfo, "Select an active contact belonging to the Refer To Organization.");
			Assert(!enquiry.HasChanges);
			enquiry.O1_Email = "enquiry12345@cw1.com";
			Assert(enquiry.HasChanges);
			Factory.Save();

			#endregion
		}

		public void TestValidateAll()
		{
			var referringOrg = Factory.New<OrgHeader>();
			var inactiveReferringContact = referringOrg.Contacts.AddNew();
			inactiveReferringContact.OC_ContactName = "Ynnej";
			inactiveReferringContact.OC_IsActive = false;

			var referToOrg = Factory.New<OrgHeader>();
			var inactiveReferToContact = referToOrg.Contacts.AddNew();
			inactiveReferToContact.OC_ContactName = "Ynnej's twin";
			inactiveReferToContact.OC_IsActive = false;

			using (EnquiryForTest.GetValidationSuspender())
			{
				EnquiryForTest.O1_OC_ReferringContact = inactiveReferringContact.PK;
				EnquiryForTest.O1_OC_ReferToContact = inactiveReferToContact.PK;
				AssertNoErrors("Validation suspended, ReferringContactName should not have errors", EnquiryForTest.ReferringContactNameInfo);
				AssertNoErrors("Validation suspended, ReferToContactName should not have errors", EnquiryForTest.ReferToContactNameInfo);
			}
			EnquiryForTest.Validation.ValidateAll();
			AssertHasErrors("Validate All called, ReferringContactName should have errors", EnquiryForTest.ReferringContactNameInfo);
			AssertHasErrors("Validate All called, ReferToContactName should have errors", EnquiryForTest.ReferToContactNameInfo);
		}

		public void TestCheckReferralContact()
		{
			// In cases where O1_OC_ReferringContact / O1_OC_ReferToContact can be set by operational actions
			// Show error on ReferringContactName / ReferToContactName since that's binded to the field
			// Must be existing contact - because cannot create a new one via that method

			var existingOrg = Factory.NewWithValidTestData<OrgHeader>();
			var unrelatedContact = existingOrg.Contacts.AddNew();
			unrelatedContact.OC_ContactName = "Random Contact";

			var referringOrg = Factory.NewWithValidTestData<OrgHeader>();
			var activeReferringContact = referringOrg.Contacts.AddNew();
			activeReferringContact.OC_ContactName = "Jenny";
			var inactiveReferringContact = referringOrg.Contacts.AddNew();
			inactiveReferringContact.OC_ContactName = "Jenny's twin";
			inactiveReferringContact.OC_IsActive = false;

			var referToOrg = Factory.NewWithValidTestData<OrgHeader>();
			var activeReferToContact = referToOrg.Contacts.AddNew();
			activeReferToContact.OC_ContactName = "Billy";
			var inactiveReferToContact = referToOrg.Contacts.AddNew();
			inactiveReferToContact.OC_ContactName = "Billy's twin";
			inactiveReferToContact.OC_IsActive = false;

			SalesEnquiry enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry.OrgPk = referringOrg.PK;
			enquiry.O1_ContactName = activeReferringContact.OC_ContactName;
			enquiry.O1_OH_SourceOfLead = referringOrg.PK;
			enquiry.O1_OH_ReferTo = referToOrg.PK;

			enquiry.O1_OC_ReferringContact = unrelatedContact.PK;
			enquiry.O1_OC_ReferToContact = unrelatedContact.PK;
			AssertHasErrors("Contact is not linked to referring org. This scenario can happen via Operational Actions", enquiry.ReferringContactNameInfo);
			AssertHasErrors("Contact is not linked to refer to org. This scenario can happen via Operational Actions", enquiry.ReferToContactNameInfo);

			enquiry.O1_OC_ReferringContact = activeReferringContact.PK;
			enquiry.O1_OC_ReferToContact = activeReferToContact.PK;
			AssertNoErrors("Contact is linked to referring org", enquiry.ReferringContactNameInfo);
			AssertNoErrors("Contact is linked to refer to org", enquiry.ReferToContactNameInfo);

			Factory.Save();

			activeReferringContact.OC_IsActive = false;
			activeReferToContact.OC_IsActive = false;
			enquiry.O1_OC_ReferringContact = activeReferringContact.PK;
			enquiry.O1_OC_ReferToContact = activeReferToContact.PK;
			AssertHasWarning(enquiry.ReferringContactNameInfo, "Select an active contact belonging to the Referring Organization.");
			AssertHasWarning(enquiry.ReferToContactNameInfo, "Select an active contact belonging to the Refer To Organization.");

			enquiry.O1_OC_ReferringContact = inactiveReferringContact.PK;
			enquiry.O1_OC_ReferToContact = inactiveReferToContact.PK;
			AssertHasError("Add error for Referring Contact.", enquiry.ReferringContactNameInfo, "Select an active contact belonging to the Referring Organization.");
			AssertEquals("Only one error for Referring Contact.", 1, enquiry.ReferringContactNameInfo.GetErrors().Count());
			AssertHasError("Add error for Refer To Contact.", enquiry.ReferToContactNameInfo, "Select an active contact belonging to the Refer To Organization.");
			AssertEquals("Only one error for Refer To Contact.", 1, enquiry.ReferToContactNameInfo.GetErrors().Count());

			Env.Security.OrgContactNew.IsAllowed = false;
			CheckErrorWithMissingSecurityRightForReferralContact(enquiry, "NewContact", @"You do not have the security right to add a Contact.
Please select from the list or check with your system administrator if you require access to the security right
Maintain -> Master Data -> Organization Contacts -> New");

			Env.Security.OrgContactNew.IsAllowed = true;
			Env.Security.OrgContactModify.IsAllowed = false;
			CheckErrorWithMissingSecurityRightForReferralContact(enquiry, "ModifyContact", @"You do not have the security right to add a Contact.
Please select from the list or check with your system administrator if you require access to the security right
Maintain -> Master Data -> Organization Contacts -> Edit");

			Env.Security.OrgContactModify.IsAllowed = true;
			Env.Security.OrgContactModifyContactDetails.IsAllowed = false;
			CheckErrorWithMissingSecurityRightForReferralContact(enquiry, "ModifyContactDetails", @"You do not have the security right to add a Contact.
Please select from the list or check with your system administrator if you require access to the security right
Maintain -> Master Data -> Organization Contacts -> Edit -> Modify Contact Details");

			Env.Security.OrgContactModifyContactDetails.IsAllowed = true;
			Env.Security.OrganisationCRMSecurity.EditByStaffNotAssigned.IsAllowed = false;
			CheckErrorWithMissingSecurityRightForReferralContact(enquiry, "EditContactByStaff", @"You do not have the security right to add a Contact.
Please select from the list or check with your system administrator if you require access to the security right
Maintain -> Master Data -> Organization -> Edit -> Edit by Staff Not Assigned");
		}

		void CheckErrorWithMissingSecurityRightForReferralContact(SalesEnquiry enquiry, string contactName, string errorMessage)
		{
			enquiry.ReferringContactName = contactName;
			enquiry.ReferToContactName = contactName;
			AssertHasError("Missing security right for Referring Contact.", enquiry.ReferringContactNameInfo, errorMessage);
			AssertHasError("Missing security right for ReferTo Contact.", enquiry.ReferToContactNameInfo, errorMessage);
		}

		public void TestCheckO1_ContactName()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact activeContact = org.Contacts.AddNew();
			activeContact.OC_ContactName = "Donald Duck";
			OrgContact inactiveContact = org.Contacts.AddNew();
			inactiveContact.OC_ContactName = "Inactive Staff";
			inactiveContact.OC_IsActive = false;

			Factory.Save();

			SalesEnquiry enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry.OrgPk = org.PK;
			enquiry.O1_ContactName = activeContact.OC_ContactName;
			AssertNoErrors("Typed out contact is okay; contact exists", enquiry.O1_ContactNameInfo);

			enquiry.O1_ContactName = inactiveContact.OC_ContactName;
			AssertHasErrors("Typed out contact is not okay; contact exists but inactive", enquiry.O1_ContactNameInfo);

			enquiry.O1_ContactName = "New contact";
			AssertNoErrors("Typed out contact is okay; contact can be created", enquiry.O1_ContactNameInfo);
			enquiry.RunPreSaveValidation();
			AssertNoErrors("Still no errors after the link between the new contact and enquiry has been set", enquiry.O1_ContactNameInfo);

			inactiveContact.OC_IsActive = true;
			enquiry.O1_ContactName = inactiveContact.OC_ContactName;
			Factory.Save();

			inactiveContact.OC_IsActive = false;
			Factory.Save();

			enquiry.RunPreSaveValidation();
			AssertHasWarning(enquiry.O1_ContactNameInfo, "Select an active contact belonging to the organization.");

			Env.Security.OrgContactNew.IsAllowed = false;
			enquiry.O1_ContactName = "NewContact";
			AssertHasError("Missing security right for Inquiry Contact.", enquiry.O1_ContactNameInfo, @"You do not have the security right to add a Contact.
Please select from the list or check with your system administrator if you require access to the security right
Maintain -> Master Data -> Organization Contacts -> New");

			Env.Security.OrgContactNew.IsAllowed = true;
			Env.Security.OrgContactModify.IsAllowed = false;
			enquiry.O1_ContactName = "ModifyContact";
			AssertHasError("Missing security right for Inquiry Contact.", enquiry.O1_ContactNameInfo, @"You do not have the security right to add a Contact.
Please select from the list or check with your system administrator if you require access to the security right
Maintain -> Master Data -> Organization Contacts -> Edit");

			Env.Security.OrgContactModify.IsAllowed = true;
			Env.Security.OrgContactModifyContactDetails.IsAllowed = false;
			enquiry.O1_ContactName = "ModifyContactDetails";
			AssertHasError("Missing security right for Inquiry Contact.", enquiry.O1_ContactNameInfo, @"You do not have the security right to add a Contact.
Please select from the list or check with your system administrator if you require access to the security right
Maintain -> Master Data -> Organization Contacts -> Edit -> Modify Contact Details");

			Env.Security.OrgContactModifyContactDetails.IsAllowed = true;
			Env.Security.OrganisationCRMSecurity.EditByStaffNotAssigned.IsAllowed = false;
			enquiry.O1_ContactName = "EditContactByStaff";
			AssertHasError("Missing security right for Inquiry Contact.", enquiry.O1_ContactNameInfo, @"You do not have the security right to add a Contact.
Please select from the list or check with your system administrator if you require access to the security right
Maintain -> Master Data -> Organization -> Edit -> Edit by Staff Not Assigned");
		}

		public void TestCheckO1_OC_LinkedContact()
		{
			// In cases where O1_OC_LinkedContact can be set by operational actions
			// Show error on O1_ContactName since that's binded to the field
			// Must be existing contact - because cannot create a new one via that method

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact org1contact1 = org1.Contacts.AddNew();
			org1contact1.OC_ContactName = "Donald Duck";
			OrgContact org1contact2 = org1.Contacts.AddNew();
			org1contact2.OC_ContactName = "Inactive Staff";
			org1contact2.OC_IsActive = false;

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact org2contact1 = org2.Contacts.AddNew();
			org2contact1.OC_ContactName = "Mickey Mouse";

			Factory.Save();

			SalesEnquiry enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry.OrgPk = org1.PK;
			enquiry.O1_OC_LinkedContact = org2contact1.PK;
			AssertHasErrors("Linked contact does not belong to org", enquiry.O1_ContactNameInfo);

			enquiry.O1_OC_LinkedContact = org1contact1.PK;
			AssertNoErrors("Linked contact does belong to org", enquiry.O1_ContactNameInfo);

			enquiry.O1_OC_LinkedContact = org1contact2.PK;
			AssertHasErrors("Linked contact does belong to org but is inactive", enquiry.O1_ContactNameInfo);
		}

		public void TestCheckO1_PortOrCountry()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "VV";
			country.RN_IsActive = true;

			var enq1 = Factory.NewWithValidTestData<SalesEnquiry>();
			enq1.O1_PortOrCountry = "VV";
			var enq2 = Factory.NewWithValidTestData<SalesEnquiry>();
			enq2.O1_PortOrCountry = "AU";
			Factory.Save();

			country.RN_IsActive = false;
			Factory.Save();

			var enq3 = Factory.NewWithValidTestData<SalesEnquiry>();
			enq3.O1_PortOrCountry = "VV";
			enq2.O1_PortOrCountry = "VV";

			enq1.RunPreSaveValidation();
			enq2.RunPreSaveValidation();
			enq3.RunPreSaveValidation();

			AssertHasWarning("Should add warning to existing record", enq1.O1_PortOrCountryInfo, "This Port / Country / Region is inactive.");
			AssertHasError("Should add error if user picks up inactive location", enq2.O1_PortOrCountryInfo, "This Port / Country / Region is inactive - it may not be used.");
			AssertHasError("Should add error to new record", enq3.O1_PortOrCountryInfo, "This Port / Country / Region is inactive - it may not be used.");
		}

		#region Implementation

		void SetSalesEnquiryFieldsMandatoryRegistry(string field, bool value)
		{
			var mandatoryFieldsCollection = new CodeDescriptionBoolDisallowNewCollection();
			mandatoryFieldsCollection.Add(50, field, (NoResString)field, value);
			OrganisationsDataRegistry.Instance.SalesEnquiryFieldsMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mandatoryFieldsCollection);
		}

		#endregion

		#region Set Up

		SalesEnquiry EnquiryForTest;

		protected override void SetUp()
		{
			base.SetUp();

			Env.Security.OrgContactNew.IsAllowed = true;
			Env.Security.OrgAddressModify.IsAllowed = true;
			Env.Security.OrgContactModifyContactDetails.IsAllowed = true;
			Env.Security.OrganisationCRMSecurity.EditByStaffNotAssigned.IsAllowed = true;
			EnquiryForTest = Factory.NewWithValidTestData<SalesEnquiry>();
		}

		#endregion
	}
}
