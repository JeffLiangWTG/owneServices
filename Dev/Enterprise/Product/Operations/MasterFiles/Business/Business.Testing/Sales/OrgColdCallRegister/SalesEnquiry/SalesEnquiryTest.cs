using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(SalesEnquiry))]
	public class SalesEnquiryTest : EnterpriseBusinessObjectTestCase
	{
		[TestDate(2019, 11, 29)]
		public void TestEditingNoteSetsLastEditTime()
		{
			var enquiry = Factory.New<SalesEnquiry>();
			Factory.Save();
			var lastEdit = enquiry.O1_SystemLastEditTimeUtc;

			TestDateAttribute.Date = ZDateTime.UtcNow.AddDays(1).ToDateTime();
			enquiry.EnquiryNotesContent = new ZBlob(new byte[] { 1, 1, 1, 1 });

			var lastEdit2 = enquiry.O1_SystemLastEditTimeUtc;
			AssertNotEquals(lastEdit, lastEdit2);

			//same value
			enquiry.EnquiryNotesContent = new ZBlob(new byte[] { 1, 1, 1, 1 });
			AssertEquals(lastEdit2, enquiry.O1_SystemLastEditTimeUtc);
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestO1_FollowupDateLocal()
		{
			var coldCall = Factory.New<SalesEnquiry>();
			coldCall.O1_FollowupDate = new ZDateTime(2013, 10, 13, 13, 0, 0);
			AssertEquals(new ZDateTime(2013, 10, 13, 23, 0, 0), coldCall.O1_FollowupDateLocal);

			coldCall.O1_FollowupDateLocal = new ZDateTime(2013, 10, 13, 13, 0, 0);
			AssertEquals(new ZDateTime(2013, 10, 13, 3, 0, 0), coldCall.O1_FollowupDate);
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestO1_LeadCalledDateLocal()
		{
			var coldCall = Factory.New<SalesEnquiry>();
			coldCall.O1_LeadCalledDate = new ZDateTime(2013, 10, 13, 13, 0, 0);
			AssertEquals(new ZDateTime(2013, 10, 13, 23, 0, 0), coldCall.O1_LeadCalledDateLocal);

			coldCall.O1_LeadCalledDateLocal = new ZDateTime(2013, 10, 13, 13, 0, 0);
			AssertEquals(new ZDateTime(2013, 10, 13, 3, 0, 0), coldCall.O1_LeadCalledDate);
		}

		public void TestEnquiryTypeDescription()
		{
			var list = new CodeDescriptionBoolCollection();
			list.Add("CCR", (NoResString)"CCR desc");
			list.Add("AAA", (NoResString)"A desc");
			list.Add("BBB", (NoResString)"B desc");
			OrganisationsDataRegistry.Instance.SalesEnquiryTypeList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var bizo = Factory.New<SalesEnquiry>();
			bizo.O1_EnquiryType = "CCR";
			AssertEquals("CCR desc", bizo.EnquiryTypeDescription);

			bizo.O1_EnquiryType = "BBB";
			AssertEquals("B desc", bizo.EnquiryTypeDescription);
		}

		public void TestAssignedSalesRepBranchCode()
		{
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "AAA";
			branch.GB_BranchName = "Test Name";
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "REP";

			var enquiry = Factory.New<SalesEnquiry>();
			AssertEquals("Should be empty as sales rep has not been set", "", enquiry.AssignedSalesRepBranchCode);

			enquiry.O1_GS_NKRepAssigned = staff.GS_Code;
			AssertEquals("Should be empty as branch has not been set for sales rep", "", enquiry.AssignedSalesRepBranchCode);

			staff.GS_GB_HomeBranch = branch.PK;
			AssertEquals("[AAA]", enquiry.AssignedSalesRepBranchCode);
		}

		public void TestOnSaving_SetLinkedCommunicationClientAndContactWhenClientIntelligenceIsSet()
		{
			#region Test Data

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "Andrew";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_ContactName = "Samuel";

			var inquiry = Factory.New<SalesEnquiry>();
			var linkedCommunication = Factory.New<OrgSalesCall>();
			linkedCommunication.LinkedInquiry = inquiry;

			var relatedCommunication = Factory.NewWithValidTestData<OrgSalesCall>();
			relatedCommunication.OQ_OH = org2.PK;
			relatedCommunication.OQ_OC = contact2.PK;
			inquiry.RelatedChildActivityPivotCollection.AddNewPivot(relatedCommunication);

			Factory.Save();

			#endregion

			inquiry.O1_OH_ConvertedToQualifiedLead = org1.PK;
			inquiry.O1_OC_LinkedContact = contact1.PK;
			Factory.Save();

			AssertEquals("Should no longer be linked to inquiry", false, linkedCommunication.IsLinkedToInquiry);
			CombineAssertions("Should have set client and contact of previously linked communications", () =>
			{
				AssertEquals("OQ_OH", org1.PK, linkedCommunication.OQ_OH);
				AssertEquals("OQ_OC", contact1.PK, linkedCommunication.OQ_OC);
			});

			CombineAssertions("Should not set client and contact of previously related communications that weren't linked to inquiry", () =>
			{
				AssertEquals("OQ_OH", org2.PK, relatedCommunication.OQ_OH);
				AssertEquals("OQ_OC", contact2.PK, relatedCommunication.OQ_OC);
			});
		}

		public void TestOnSaving_SetLinkedCommunicationClientAndContactWhenClientIntelligenceIsSetFromCampaignItem()
		{
			#region Test Data

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "Andrew";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_ContactName = "Samuel";

			var inquiry = Factory.New<SalesEnquiry>();
			var linkedCommunication = Factory.New<OrgSalesCall>();

			var campaign = Factory.New<IGlbCompanyCampaign>();
			campaign.G0_CampaignName = ZGuid.NewZGuid().ToString();
			campaign.G0_CampaignID = "42";

			BusinessObject campaignItem = (BusinessObject)Factory.New<IGlbCompanyCampaignItem>();
			campaignItem[GlbCompanyCampaignItemSchema.G8_RecipientTableCode.Name] = OrgColdCallRegisterSchema.Constants.Prefix;
			campaignItem[GlbCompanyCampaignItemSchema.G8_RecipientID.Name] = inquiry.PK;
			campaignItem[GlbCompanyCampaignItemSchema.G8_G0.Name] = campaign.PK;

			var relatedCommunication = Factory.NewWithValidTestData<OrgSalesCall>();
			relatedCommunication.OQ_OH = org2.PK;
			relatedCommunication.OQ_OC = contact2.PK;
			((IRelatableActivity)campaignItem).RelatedChildActivityPivotCollection.AddNewPivot(linkedCommunication);

			Factory.Save();

			#endregion

			inquiry.O1_OH_ConvertedToQualifiedLead = org1.PK;
			inquiry.O1_OC_LinkedContact = contact1.PK;
			Factory.Save();

			AssertEquals("Should no longer be linked to inquiry", false, linkedCommunication.IsLinkedToInquiry);
			CombineAssertions("Should have set client and contact of linked communications", () =>
			{
				AssertEquals("OQ_OH", org1.PK, linkedCommunication.OQ_OH);
				AssertEquals("OQ_OC", contact1.PK, linkedCommunication.OQ_OC);
			});

			CombineAssertions("Should not set client and contact of related communications which was not linked to inquiry", () =>
			{
				AssertEquals("OQ_OH", org2.PK, relatedCommunication.OQ_OH);
				AssertEquals("OQ_OC", contact2.PK, relatedCommunication.OQ_OC);
			});
		}

		public void TestDelete_AlsoDeletesInquiryLinkedCommunications()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TESTADL";
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var linkedCommunication = Factory.New<OrgSalesCall>();
			linkedCommunication.LinkedInquiry = inquiry;
			var relatedCommunication = Factory.NewWithValidTestData<OrgSalesCall>();
			relatedCommunication.OQ_OH = org.PK;
			inquiry.RelatedChildActivityPivotCollection.AddNewPivot(relatedCommunication);

			Factory.Save();

			inquiry.Delete();
			AssertEquals(true, linkedCommunication.IsDeleted);
			AssertEquals(false, relatedCommunication.IsDeleted);
		}

		public void TestOnLoaded_SetDefaultLinkedAddress()
		{
			var inquiry = Factory.New<SalesEnquiry>();
			var org = Factory.New<OrgHeader>();
			inquiry.O1_OH_ConvertedToQualifiedLead = org.PK;
			inquiry.O1_OA_LinkedAddress = ZGuid.Empty;

			inquiry.OnLoaded();

			AssertEquals(org.MainAddress.PK, inquiry.O1_OA_LinkedAddress);
		}

		public void TestPreSaveValidation_CreateContactFromManualValuesIfLinkedContactMissing()
		{
			var jobCategories = new CodeDescriptionBoolCollection(OrgContactSchema.OC_JobCategory.MaxLength);
			jobCategories.Add("Developer");
			OrganisationsDataRegistry.Instance.ContactJobCategories.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, jobCategories);

			var org = Factory.New<OrgHeader>();
			var inquiry = Factory.New<SalesEnquiry>();
			inquiry.O1_OH_ConvertedToQualifiedLead = org.PK;
			inquiry.O1_OC_LinkedContact = ZGuid.Empty;
			inquiry.O1_ContactName = "Andrew";
			inquiry.O1_Phone = "0212345678";
			inquiry.O1_Fax = "031245678";
			inquiry.O1_Mobile = "0412345678";
			inquiry.O1_JobCategory = "Developer";
			inquiry.O1_Email = "INVALID";

			AssertEquals("Precondition", 0, org.Contacts.Count);
			AssertEquals("Precondition", true, inquiry.HasContactErrors);
			inquiry.RunPreSaveValidation();
			AssertEquals("Should not create contact due to invalid email", 0, org.Contacts.Count);

			inquiry.O1_Email = "andrew@cargowise.com";
			AssertEquals(false, inquiry.HasContactErrors);
			inquiry.RunPreSaveValidation();
			AssertEquals("Contact should have been created from manual values", 1, org.Contacts.Count);
			var contact = org.Contacts[0];
			AssertEquals(contact.PK, inquiry.O1_OC_LinkedContact);
			AssertEquals("Andrew", contact.OC_ContactName);
			AssertEquals("0212345678", contact.OC_Phone);
			AssertEquals("031245678", contact.OC_Fax);
			AssertEquals("0412345678", contact.OC_Mobile);
			AssertEquals("Developer", contact.OC_JobCategory);
			AssertEquals("andrew@cargowise.com", contact.OC_Email);
		}

		public void TestPreSaveValidation_LinkedContactWithoutSecurityRights()
		{
			var org = Factory.New<OrgHeader>();
			var addr = org.Addresses.AddNew();
			org.OH_FullName = "Test Org";
			var inquiry = Factory.New<SalesEnquiry>();
			inquiry.O1_EnquiryType = SalesEnquiry.Codes.SalesEnquiry;
			inquiry.O1_OH_ConvertedToQualifiedLead = org.PK;
			inquiry.O1_OC_LinkedContact = ZGuid.Empty;
			inquiry.O1_OA_LinkedAddress = addr.PK;
			inquiry.O1_ContactName = "Andrew";

			var referringOrg = Factory.NewWithValidTestData<OrgHeader>();

			var referToOrg = Factory.NewWithValidTestData<OrgHeader>();

			inquiry.O1_OH_SourceOfLead = referringOrg.PK;
			inquiry.O1_OH_ReferTo = referToOrg.PK;

			inquiry.ReferringContactName = "Jenny";
			inquiry.ReferToContactName = "Anthony";

			AssertEquals("Precondition", 0, org.Contacts.Count);
			AssertEquals("Precondition", 0, referringOrg.Contacts.Count);
			AssertEquals("Precondition", 0, referToOrg.Contacts.Count);
			AssertNoErrors("Pre-condition: inquiry contact has no validation errors", inquiry.O1_ContactNameInfo);
			AssertNoErrors("Pre-condition: referring contact has no validation errors", inquiry.ReferringContactNameInfo);
			AssertNoErrors("Pre-condition: refer to contact has no validation errors", inquiry.ReferToContactNameInfo);

			Env.Security.OrgContactModify.IsAllowed = false;

			inquiry.Validation.ValidateO1_ContactName();
			inquiry.Validation.ValidateReferringContactName();
			inquiry.Validation.ValidateReferToContactName();
			AssertHasError("Has error with inquiry contact.", inquiry.O1_ContactNameInfo, @"You do not have the security right to add a Contact.
Please select from the list or check with your system administrator if you require access to the security right
Maintain -> Master Data -> Organization Contacts -> Edit");
			AssertHasError("Has error with referring contact.", inquiry.ReferringContactNameInfo, @"You do not have the security right to add a Contact.
Please select from the list or check with your system administrator if you require access to the security right
Maintain -> Master Data -> Organization Contacts -> Edit");
			AssertHasError("Has error with refer to contact.", inquiry.ReferToContactNameInfo, @"You do not have the security right to add a Contact.
Please select from the list or check with your system administrator if you require access to the security right
Maintain -> Master Data -> Organization Contacts -> Edit");
			inquiry.RunPreSaveValidation();

			AssertEquals("Should not create org contact due to no security rights", 0, org.Contacts.Count);
			AssertEquals("Should not create referring contact due to no security rights", 0, referringOrg.Contacts.Count);
			AssertEquals("Should not create referTo contact due to no security rights", 0, referToOrg.Contacts.Count);

			Env.Security.OrgContactModify.IsAllowed = true;
			Env.Security.OrgContactModifyContactDetails.IsAllowed = false;

			inquiry.Validation.ValidateO1_ContactName();
			inquiry.Validation.ValidateReferringContactName();
			inquiry.Validation.ValidateReferToContactName();
			AssertHasError("Has error with inquiry contact.", inquiry.O1_ContactNameInfo, @"You do not have the security right to add a Contact.
Please select from the list or check with your system administrator if you require access to the security right
Maintain -> Master Data -> Organization Contacts -> Edit -> Modify Contact Details");
			AssertHasError("Has error with referring contact.", inquiry.ReferringContactNameInfo, @"You do not have the security right to add a Contact.
Please select from the list or check with your system administrator if you require access to the security right
Maintain -> Master Data -> Organization Contacts -> Edit -> Modify Contact Details");
			AssertHasError("Has error with refer to contact.", inquiry.ReferToContactNameInfo, @"You do not have the security right to add a Contact.
Please select from the list or check with your system administrator if you require access to the security right
Maintain -> Master Data -> Organization Contacts -> Edit -> Modify Contact Details");
			inquiry.RunPreSaveValidation();

			AssertEquals("Should not create org contact due to no security rights", 0, org.Contacts.Count);
			AssertEquals("Should not create referring contact due to no security rights", 0, referringOrg.Contacts.Count);
			AssertEquals("Should not create referTo contact due to no security rights", 0, referToOrg.Contacts.Count);

			Env.Security.OrgContactModifyContactDetails.IsAllowed = true;
			Env.Security.OrgContactNew.IsAllowed = false;

			inquiry.Validation.ValidateO1_ContactName();
			inquiry.Validation.ValidateReferringContactName();
			inquiry.Validation.ValidateReferToContactName();
			AssertHasError("Has error with inquiry contact.", inquiry.O1_ContactNameInfo, @"You do not have the security right to add a Contact.
Please select from the list or check with your system administrator if you require access to the security right
Maintain -> Master Data -> Organization Contacts -> New");
			AssertHasError("Has error with referring contact.", inquiry.ReferringContactNameInfo, @"You do not have the security right to add a Contact.
Please select from the list or check with your system administrator if you require access to the security right
Maintain -> Master Data -> Organization Contacts -> New");
			AssertHasError("Has error with refer to contact.", inquiry.ReferToContactNameInfo, @"You do not have the security right to add a Contact.
Please select from the list or check with your system administrator if you require access to the security right
Maintain -> Master Data -> Organization Contacts -> New");
			inquiry.RunPreSaveValidation();

			AssertEquals("Should not create org contact due to no security rights", 0, org.Contacts.Count);
			AssertEquals("Should not create referring contact due to no security rights", 0, referringOrg.Contacts.Count);
			AssertEquals("Should not create referTo contact due to no security rights", 0, referToOrg.Contacts.Count);

			Env.Security.OrgContactNew.IsAllowed = true;

			inquiry.Validation.ValidateO1_ContactName();
			inquiry.Validation.ValidateReferringContactName();
			inquiry.Validation.ValidateReferToContactName();
			AssertNoErrors("No error with inquiry contact.", inquiry.O1_ContactNameInfo);
			AssertNoErrors("No error with referring contact.", inquiry.ReferringContactNameInfo);
			AssertNoErrors("No error with refer to contact.", inquiry.ReferToContactNameInfo);
			inquiry.RunPreSaveValidation();

			AssertEquals("Org contact should have been created from manual values", 1, org.Contacts.Count);
			AssertEquals("Andrew", org.Contacts[0].OC_ContactName);
			AssertEquals("Referring contact should have been created", 1, referringOrg.Contacts.Count);
			AssertEquals("Jenny", referringOrg.Contacts[0].OC_ContactName);
			AssertEquals("ReferTo contact should have been created", 1, referToOrg.Contacts.Count);
			AssertEquals("Anthony", referToOrg.Contacts[0].OC_ContactName);
		}

		public void TestSettingContactName_UpdatesLinkedContactAndContactManualValues()
		{
			var org = Factory.New<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Andrew";
			var inquiry = Factory.New<SalesEnquiry>();
			inquiry.O1_OC_LinkedContact = ZGuid.Empty;
			inquiry.O1_Phone = "0212345678";
			inquiry.O1_Fax = "031245678";
			inquiry.O1_Email = "andrew@cargowise.com";
			inquiry.O1_Mobile = "0412345678";
			inquiry.O1_JobCategory = "Developer";

			inquiry.O1_ContactName = "Andrew";
			AssertEquals(ZGuid.Empty, inquiry.O1_OC_LinkedContact);
			AssertEquals("0212345678", inquiry.O1_Phone);
			AssertEquals("031245678", inquiry.O1_Fax);
			AssertEquals("andrew@cargowise.com", inquiry.O1_Email);
			AssertEquals("0412345678", inquiry.O1_Mobile);
			AssertEquals("Developer", inquiry.O1_JobCategory);
			AssertEquals("Andrew", inquiry.O1_ContactName);

			inquiry.O1_ContactName = "XXX";
			AssertEquals(ZGuid.Empty, inquiry.O1_OC_LinkedContact);
			AssertEquals("0212345678", inquiry.O1_Phone);
			AssertEquals("031245678", inquiry.O1_Fax);
			AssertEquals("andrew@cargowise.com", inquiry.O1_Email);
			AssertEquals("0412345678", inquiry.O1_Mobile);
			AssertEquals("Developer", inquiry.O1_JobCategory);
			AssertEquals("XXX", inquiry.O1_ContactName);

			inquiry.O1_OH_ConvertedToQualifiedLead = org.PK;
			inquiry.O1_ContactName = "Andrew";
			AssertEquals(contact.PK, inquiry.O1_OC_LinkedContact);

			inquiry.O1_ContactName = "Andrew Luong";
			AssertEquals(ZGuid.Empty, inquiry.O1_OC_LinkedContact);
			AssertEquals(ZString.Empty, inquiry.O1_Phone);
			AssertEquals(ZString.Empty, inquiry.O1_Fax);
			AssertEquals(ZString.Empty, inquiry.O1_Email);
			AssertEquals(ZString.Empty, inquiry.O1_Mobile);
			AssertEquals(ZString.Empty, inquiry.O1_JobCategory);
			AssertEquals("Andrew Luong", inquiry.O1_ContactName);
		}

		public void TestSettingContactName_UpdatesLinkedContactIgnoringCase()
		{
			var org = Factory.New<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Andrew";
			var inquiry = Factory.New<SalesEnquiry>();
			inquiry.O1_OH_ConvertedToQualifiedLead = org.PK;

			inquiry.O1_OC_LinkedContact = ZGuid.Empty;
			inquiry.O1_ContactName = "ANDREW";
			AssertEquals(contact.PK, inquiry.O1_OC_LinkedContact);

			inquiry.O1_OC_LinkedContact = ZGuid.Empty;
			inquiry.O1_ContactName = "andrew";
			AssertEquals(contact.PK, inquiry.O1_OC_LinkedContact);

			inquiry.O1_OC_LinkedContact = ZGuid.Empty;
			inquiry.O1_ContactName = "Andrew";
			AssertEquals(contact.PK, inquiry.O1_OC_LinkedContact);
		}

		#region Link To Organization

		public void TestLinkToOrganizationByLinkingToAddress()
		{
			var orgToLinkTo = Factory.New<OrgHeader>();
			var addressToLinkTo = orgToLinkTo.Addresses.AddNew();

			var inquiry = Factory.New<SalesEnquiry>();
			inquiry.O1_CompanyName = "Shapes Incorporated";
			inquiry.O1_Address1 = "Box Street";
			inquiry.O1_Address2 = "Ball Round";
			inquiry.O1_City = "3D City";
			inquiry.O1_State = "3D State";
			inquiry.O1_PostCode = "3D";
			inquiry.O1_PortOrCountry = "SHAPE";
			inquiry.O1_ContactName = "Mr Box";

			inquiry.LinkToOrganizationByLinkingToAddress(addressToLinkTo.PK);

			AssertEquals(orgToLinkTo.PK, inquiry.O1_OH_ConvertedToQualifiedLead);
			AssertEquals(addressToLinkTo.PK, inquiry.O1_OA_LinkedAddress);
			AssertEquals("Mr Box", orgToLinkTo.Contacts[0].OC_ContactName);

			AssertNoteCreated("Should have created Original Inquiry Registration Details note", inquiry, StmNoteVisibility.INT,
"Original Inquiry Registration Details",
@"Company Name: Shapes Incorporated
Address 1: Box Street
Address 2: Ball Round
City: 3D City
State: 3D State
Post Code: 3D
Port/Country/Region: SHAPE");
		}

		public void TestLinkToOrganizationByAddingInquiryAddress()
		{
			var orgToLinkTo = Factory.New<OrgHeader>();
			orgToLinkTo.OH_FullName = "Shapes!";
			orgToLinkTo.MainAddress.OA_PostCode = "2D";
			orgToLinkTo.MainAddress.OA_Address1 = "Box Street";
			orgToLinkTo.MainAddress.OA_Address2 = "Ball Not-Round";

			var inquiry = Factory.New<SalesEnquiry>();
			inquiry.O1_CompanyName = "Shapes Incorporated";
			inquiry.O1_Address1 = "Box Street";
			inquiry.O1_Address2 = "Ball Round";
			inquiry.O1_City = "3D City";
			inquiry.O1_State = "3D State";
			inquiry.O1_PostCode = "3D";
			inquiry.O1_PortOrCountry = "SHAPE";
			inquiry.O1_ContactName = "Mr Box";

			AssertEquals("Precondition", 1, orgToLinkTo.Addresses.Count);

			inquiry.LinkToOrganizationByAddingInquiryAddress(orgToLinkTo.PK);

			AssertEquals("Should have added another address", 2, orgToLinkTo.Addresses.Count);
			var newAddress = orgToLinkTo.Addresses.Cast<OrgAddress>().First(address => address.OA_PostCode == "3D");
			AssertEquals(1, newAddress.AddressCapability.Cast<OrgAddressCapabilityWrapper>().Count(capability => capability.Enabled));
			var officeCapability = newAddress.GetAddressCapability(OrgConstants.AddressType.Office);
			AssertNotNull(officeCapability);
			AssertEquals(false, officeCapability.PZ_IsMainAddress);

			AssertEquals(orgToLinkTo.PK, inquiry.O1_OH_ConvertedToQualifiedLead);
			AssertEquals(newAddress.PK, inquiry.O1_OA_LinkedAddress);
			AssertEquals("Mr Box", orgToLinkTo.Contacts[0].OC_ContactName);

			AssertNoteCreated("Should have created Original Inquiry Registration Details note", inquiry, StmNoteVisibility.INT,
"Original Inquiry Registration Details",
@"Company Name: Shapes Incorporated
Address 1: Box Street
Address 2: Ball Round
City: 3D City
State: 3D State
Post Code: 3D
Port/Country/Region: SHAPE");
		}

		public void TestLinkToOrganizationByAddingInquiryAddress_CreateNoteWithCustomDescription()
		{
			var enquiryModule = new CustomNoteModuleAndCountry();
			enquiryModule.ModuleIDName = ModuleIDs.SalesEnquiry.Name;
			enquiryModule.CountryCode = CustomNoteModuleAndCountry.ModuleAndCountryCodes.CountryALL;

			var item = enquiryModule.CustomNoteTypesList.AddNew();
			item.IsTextOnly = true;
			item.IsAppendingNote = true;
			item.IsReadOnlyAfterAdd = false;
			item.ForceRead = false;
			item.DefaultVisibility = nameof(StmNoteVisibility.PUB);
			item.NoteName = "BarcodeNote";

			var collection = new CustomNoteTypes();
			collection.NoteModuleAndCountryList.Add(enquiryModule);

			SystemDataRegistry.Instance.CustomNotes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();

			((IStmNoteParent)inquiry).CustomNoteTypesDelegate = new GetValueDelegate<NoteTypeCollection>(() => CustomNotesProvider.Instance.CustomNoteTypesForModuleAndThisCountry(ModuleIDs.SalesEnquiry.Name));

			StmNote noteToSave = inquiry.Notes.AddNew(false, "Custom registry note", "TEST");
			noteToSave.ST_NoteContextModule = nameof(StmNoteContextModule.A);
			noteToSave.ST_NoteContextDirection = nameof(StmNoteContextModule.A);
			noteToSave.ST_NoteContextFreightMode = nameof(StmNoteContextModule.A);
			noteToSave.ST_NoteType = nameof(StmNoteVisibility.INT);

			var orgToLinkTo = Factory.NewWithValidTestData<OrgHeader>();
			orgToLinkTo.Addresses.AddNew();

			inquiry.LinkToOrganizationByAddingInquiryAddress(orgToLinkTo.PK);

			var notes = inquiry.Notes.GetAllNotes().ToArray<StmNote>();
			var note = notes.FirstOrDefault(n => n.ST_Description == "Original Inquiry Registration Details");
			AssertNotNull("Should find a note", note);
			AssertEquals("IsCustomDescription", true, note.ST_IsCustomDescription);
		}

		public void TestCopyNotesToOrganization_LinkToOrganizationByAddingInquiryAddress()
		{
			var enquiryModule = new CustomNoteModuleAndCountry();
			enquiryModule.ModuleIDName = ModuleIDs.SalesEnquiry.Name;
			enquiryModule.CountryCode = CustomNoteModuleAndCountry.ModuleAndCountryCodes.CountryALL;

			var item = enquiryModule.CustomNoteTypesList.AddNew();
			item.IsTextOnly = true;
			item.IsAppendingNote = true;
			item.IsReadOnlyAfterAdd = false;
			item.ForceRead = false;
			item.DefaultVisibility = nameof(StmNoteVisibility.PUB);
			item.NoteName = "BarcodeNote";

			var collection = new CustomNoteTypes();
			collection.NoteModuleAndCountryList.Add(enquiryModule);

			SystemDataRegistry.Instance.CustomNotes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiry.O1_Address1 = "Box Street";
			inquiry.O1_City = "3D City";
			inquiry.O1_PostCode = "3D";

			((IStmNoteParent)inquiry).CustomNoteTypesDelegate = new GetValueDelegate<NoteTypeCollection>(() => CustomNotesProvider.Instance.CustomNoteTypesForModuleAndThisCountry(ModuleIDs.SalesEnquiry.Name));

			StmNote noteToSave = inquiry.Notes.AddNew(false, "Custom registry note", "TEST");
			noteToSave.ST_NoteContextModule = nameof(StmNoteContextModule.A);
			noteToSave.ST_NoteContextDirection = nameof(StmNoteContextModule.A);
			noteToSave.ST_NoteContextFreightMode = nameof(StmNoteContextModule.A);
			noteToSave.ST_NoteType = nameof(StmNoteVisibility.INT);

			var orgToLinkTo = Factory.NewWithValidTestData<OrgHeader>();
			orgToLinkTo.Addresses.AddNew();

			inquiry.LinkToOrganizationByAddingInquiryAddress(orgToLinkTo.PK);
			Factory.Save();

			AssertEquals("Should have 2 notes", 2, inquiry.Notes.GetAllNotes().Count);
			AssertEquals("Should have just one transferred note", 1, orgToLinkTo.Notes.GetAllNotes().Count);
		}

		public void TestCopyNotesToOrganization_LinkToOrganizationByLinkingToAddress()
		{
			var enquiryModule = new CustomNoteModuleAndCountry();
			enquiryModule.ModuleIDName = ModuleIDs.SalesEnquiry.Name;
			enquiryModule.CountryCode = CustomNoteModuleAndCountry.ModuleAndCountryCodes.CountryALL;

			var item = enquiryModule.CustomNoteTypesList.AddNew();
			item.IsTextOnly = true;
			item.IsAppendingNote = true;
			item.IsReadOnlyAfterAdd = false;
			item.ForceRead = false;
			item.DefaultVisibility = nameof(StmNoteVisibility.PUB);
			item.NoteName = "BarcodeNote";

			var collection = new CustomNoteTypes();
			collection.NoteModuleAndCountryList.Add(enquiryModule);

			SystemDataRegistry.Instance.CustomNotes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiry.O1_Address1 = "Box Street";
			inquiry.O1_City = "3D City";
			inquiry.O1_State = "3D State";

			((IStmNoteParent)inquiry).CustomNoteTypesDelegate = new GetValueDelegate<NoteTypeCollection>(() => CustomNotesProvider.Instance.CustomNoteTypesForModuleAndThisCountry(ModuleIDs.SalesEnquiry.Name));

			StmNote noteToSave = inquiry.Notes.AddNew(false, "Custom registry note", "TEST");
			noteToSave.ST_NoteContextModule = nameof(StmNoteContextModule.A);
			noteToSave.ST_NoteContextDirection = nameof(StmNoteContextModule.A);
			noteToSave.ST_NoteContextFreightMode = nameof(StmNoteContextModule.A);
			noteToSave.ST_NoteType = nameof(StmNoteVisibility.INT);

			var orgToLinkTo = Factory.NewWithValidTestData<OrgHeader>();
			var addressToLinkTo = orgToLinkTo.Addresses.AddNew();
			addressToLinkTo.Address1 = "Box Street";
			addressToLinkTo.City = "3D City";
			addressToLinkTo.State = "3D State";

			inquiry.LinkToOrganizationByLinkingToAddress(addressToLinkTo.PK);
			Factory.Save();

			AssertEquals("Should have 2 notes", 2, inquiry.Notes.GetAllNotes().Count);
			AssertEquals("Should have just one transferred note", 1, orgToLinkTo.Notes.GetAllNotes().Count);
		}

		#endregion

		#region Address

		public void TestShouldLinkToMainAddressOnSetOrg()
		{
			var org = Factory.New<OrgHeader>();
			org.MainAddress.OA_Address1 = "Address 1 St";

			var inquiry = Factory.New<SalesEnquiry>();

			CombineAssertions(() =>
			{
				inquiry.OrgPk = org.PK;
				AssertEquals("Should link to main address by default", org.MainAddress.PK, inquiry.O1_OA_LinkedAddress);

				inquiry.OrgPk = ZGuid.Invalid;
				AssertEquals("Should clear linked address when set invalid org", ZGuid.Empty, inquiry.O1_OA_LinkedAddress);

				inquiry.OrgPk = ZGuid.Empty;
				AssertEquals("Should clear linked address when clearing org", ZGuid.Empty, inquiry.O1_OA_LinkedAddress);
			});
		}

		public void TestAddedInquiryAddressIsRemovedWhenOrgLinkIsRemovedBeforeSaving()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_Address1 = "Main Address 1";

			Factory.Save();

			var enquiry = Factory.New<SalesEnquiry>();
			enquiry.O1_Address1 = "New Address 1";
			enquiry.LinkToOrganizationByAddingInquiryAddress(org.PK);
			AssertEquals(2, org.Addresses.Count);

			enquiry.OrgPk = ZGuid.Empty;
			AssertEquals(1, org.Addresses.Count);
		}

		#endregion

		public void TestSetDefaultValues()
		{
			SalesEnquiry enquiry = Factory.New<SalesEnquiry>();
			AssertEquals("O1_LeadStatus", SalesEnquiryStatusCodeList.Codes.Open, enquiry.O1_LeadStatus);
			AssertEquals("O1_EnquiryType", ZString.Empty, enquiry.O1_EnquiryType);
		}

		public void TestLinkToExistingOrg()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "SAMTESSYD";
			org.OH_FullName = "Sam Test Org";
			org.MainAddress.OA_Address1 = "111 Test Drive";
			org.MainAddress.OA_Address2 = "A4";
			org.MainAddress.OA_City = "Sydney";
			org.MainAddress.OA_State = "NSW";
			org.MainAddress.OA_PostCode = "2000";
			org.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			org.PrimaryRegistrationNumber.Number = "53 004 085 616";
			org.MainWebURL.PU_URL = "www.test.org";

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Sam";
			contact.OC_Phone = "02 9999 8888";
			contact.OC_Mobile = "0423 999 888";
			contact.OC_Email = "sam@test.com";
			contact.OC_Fax = "02 7777 4444";
			contact.OC_JobCategory = "System Tester";

			Factory.Save();

			SalesEnquiry enquiry = Factory.New<SalesEnquiry>();
			enquiry.OrgPk = org.PK;
			enquiry.O1_OC_LinkedContact = contact.PK;
			AssertEquals("Sam Test Org", enquiry.O1_CompanyName);
			AssertEquals("111 Test Drive", enquiry.O1_Address1);
			AssertEquals("A4", enquiry.O1_Address2);
			AssertEquals("Sydney", enquiry.O1_City);
			AssertEquals("NSW", enquiry.O1_State);
			AssertEquals("2000", enquiry.O1_PostCode);
			AssertEquals("53 004 085 616", enquiry.O1_BusinessRegNo);
			AssertEquals("www.test.org", enquiry.O1_WebAddress);
			AssertEquals("Sam", enquiry.O1_ContactName);
			AssertEquals("02 9999 8888", enquiry.O1_Phone);
			AssertEquals("0423 999 888", enquiry.O1_Mobile);
			AssertEquals("sam@test.com", enquiry.O1_Email);
			AssertEquals("02 7777 4444", enquiry.O1_Fax);
			AssertEquals("System Tester", enquiry.O1_JobCategory);

			enquiry.O1_LeadSource = "AAA";
			enquiry.O1_OpportunitySourceDetails = "AAA Details";
			enquiry.O1_GS_NKRepAssigned = "SCW";
			var noteContent = new ZBlob(new byte[] { 1, 1, 1, 1 });
			enquiry.EnquiryNotesContent = noteContent;

			Factory.Save();

			SalesEnquiry loadedEnquiry = new BusinessObjectFactory().Load<SalesEnquiry>(enquiry.PK);
			AssertEquals("Sam Test Org", loadedEnquiry.O1_CompanyName);
			AssertEquals("111 Test Drive", loadedEnquiry.O1_Address1);
			AssertEquals("A4", loadedEnquiry.O1_Address2);
			AssertEquals("Sydney", loadedEnquiry.O1_City);
			AssertEquals("NSW", loadedEnquiry.O1_State);
			AssertEquals("2000", loadedEnquiry.O1_PostCode);
			AssertEquals("53 004 085 616", loadedEnquiry.O1_BusinessRegNo);
			AssertEquals("www.test.org", loadedEnquiry.O1_WebAddress);
			AssertEquals("Sam", loadedEnquiry.O1_ContactName);
			AssertEquals("02 9999 8888", loadedEnquiry.O1_Phone);
			AssertEquals("0423 999 888", loadedEnquiry.O1_Mobile);
			AssertEquals("02 7777 4444", loadedEnquiry.O1_Fax);
			AssertEquals("sam@test.com", loadedEnquiry.O1_Email);
			AssertEquals("System Tester", loadedEnquiry.O1_JobCategory);
			AssertEquals("AAA", loadedEnquiry.O1_LeadSource);
			AssertEquals("AAA Details", loadedEnquiry.O1_OpportunitySourceDetails);
			AssertEquals("SCW", loadedEnquiry.O1_GS_NKRepAssigned);
			AssertEquals(ZBlob.FromUTF8(ORtfTextUtil.TextToRtf(noteContent.ToUTF8())), loadedEnquiry.EnquiryNotesContent);
		}

		public void TestHeader()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "XYZAAA";

			SalesEnquiry enquiry = Factory.New<SalesEnquiry>();
			AssertNull(enquiry.Header);
			enquiry.OrgPk = org.PK;
			AssertEquals("Header is org", org, enquiry.Header);
		}

		public void TestDefaultAssignedSalesRep()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var otherOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			var currentCompany = GlbCompany.CurrentCompany;
			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			Factory.Save();

			using (OrganisationsDataRegistry.Instance.SalesEnquiryDefaultAssignedSalesRep.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AddNewSalesStaff(organisation, otherCompany.PK);
				var enquiry = Factory.New<SalesEnquiry>();
				enquiry.OrgPk = organisation.PK;
				AssertEquals("Should be left blank if there is no sales rep assignment for organisation", "", enquiry.O1_GS_NKRepAssigned);

				var globalStaff = AddNewSalesStaff(organisation, ZGuid.Empty);
				enquiry = Factory.New<SalesEnquiry>();
				enquiry.OrgPk = organisation.PK;
				AssertEquals("Should be defaulted to global sales rep for organisation if there is none assigned for current company", globalStaff.GS_Code, enquiry.O1_GS_NKRepAssigned);

				var staffForCurrentCompany = AddNewSalesStaff(organisation, currentCompany.PK);
				enquiry = Factory.New<SalesEnquiry>();
				enquiry.OrgPk = organisation.PK;
				AssertEquals("Should be defaulted to current company's sales rep for the organisation", staffForCurrentCompany.GS_Code, enquiry.O1_GS_NKRepAssigned);

				enquiry.OrgPk = otherOrganisation.PK;
				AssertEquals("Should not overwrite previous value if there is no sales rep assignment for organisation", staffForCurrentCompany.GS_Code, enquiry.O1_GS_NKRepAssigned);
			}

			using (OrganisationsDataRegistry.Instance.SalesEnquiryDefaultAssignedSalesRep.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var enquiry = Factory.New<SalesEnquiry>();
				enquiry.OrgPk = organisation.PK;
				AssertEquals("Should be left blank if registry is set to false", "", enquiry.O1_GS_NKRepAssigned);
			}
		}

		public void TestDocManagerInfo_ReadOnly()
		{
			var enquiry = Factory.New<SalesEnquiry>();
			AssertEquals("DocManagerInfo.ReadOnly when Open", false, ((IDocManagerSupport)enquiry).DocManagerInfo.ReadOnly);
			enquiry.O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Closed;
			AssertEquals("DocManagerInfo.ReadOnly when Closed", false, ((IDocManagerSupport)enquiry).DocManagerInfo.ReadOnly);
			enquiry.O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Converted;
			AssertEquals("DocManagerInfo.ReadOnly when Converted", false, ((IDocManagerSupport)enquiry).DocManagerInfo.ReadOnly);
		}

		public void TestReadOnly()
		{
			SalesEnquiry enquiry = Factory.New<SalesEnquiry>();
			AssertEquals("ReadOnly", false, enquiry.ReadOnly);

			enquiry.O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Closed;
			AssertEquals("ReadOnly when Closed", true, enquiry.ReadOnly);
			AssertEquals("ReadOnly when Closed", false, enquiry.O1_CloseReasonInfo.ReadOnly);

			enquiry.O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Converted;
			AssertEquals("ReadOnly when Converted", true, enquiry.ReadOnly);
			AssertEquals("ReadOnly when Closed", true, enquiry.O1_CloseReasonInfo.ReadOnly);
		}

		public void TestReadOnly_ContactFields()
		{
			var org = Factory.New<OrgHeader>();
			var contact = org.Contacts.AddNew();
			var inquiry = Factory.New<SalesEnquiry>();
			inquiry.O1_OH_ConvertedToQualifiedLead = org.PK;

			inquiry.O1_OC_LinkedContact = ZGuid.Empty;
			AssertEquals(false, inquiry.O1_ContactNameInfo.ReadOnly);
			AssertEquals(false, inquiry.O1_PhoneInfo.ReadOnly);
			AssertEquals(false, inquiry.O1_EmailInfo.ReadOnly);
			AssertEquals(false, inquiry.O1_MobileInfo.ReadOnly);
			AssertEquals(false, inquiry.O1_JobCategoryInfo.ReadOnly);

			inquiry.O1_OC_LinkedContact = contact.PK;
			AssertEquals(false, inquiry.O1_ContactNameInfo.ReadOnly);
			AssertEquals(true, inquiry.O1_PhoneInfo.ReadOnly);
			AssertEquals(true, inquiry.O1_EmailInfo.ReadOnly);
			AssertEquals(true, inquiry.O1_MobileInfo.ReadOnly);
			AssertEquals(true, inquiry.O1_JobCategoryInfo.ReadOnly);
		}

		public void TestStatusDescription()
		{
			AssertCodesAndDescriptions(new SalesEnquiryStatusCodeList(), "O1_LeadStatus", "StatusDescription");
		}

		public void TestSourceDescription()
		{
			var collection = new CodeDescriptionBoolRelatedItemCollection();
			collection.Add("WTF", (NoResString)"Wise Tech Forum", true);
			collection.Add("ENC", (NoResString)"Encyclopedia", false);
			OrganisationsDataRegistry.Instance.OpportunitySource.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			AssertCodesAndDescriptions((CodeDescriptionPairList)new SalesEnquiryLookups(null).Source_List, "O1_LeadSource", "SourceDescription");
		}

		public void TestLeadInterestDescription()
		{
			var collection = new CodeDescriptionBoolCollection();
			collection.Add("HOT", (NoResString)"Hot", true);
			collection.Add("CLD", (NoResString)"Cold", false);
			OrganisationsDataRegistry.Instance.SalesEnquiryLeadInterests.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			AssertCodesAndDescriptions(new SalesEnquiryLookups(null).LeadInterest_List, "O1_InterestLevel", "InterestLevelDescription");
		}

		void AssertCodesAndDescriptions(CodeDescriptionPairList codeList, string codeProperty, string descriptionProperty)
		{
			SalesEnquiry enquiry = Factory.New<SalesEnquiry>();
			foreach (ICodeDescription pair in codeList)
			{
				enquiry[codeProperty] = pair.Code;
				AssertEquals(pair.Code, pair.Description, enquiry[descriptionProperty]);
			}
		}

		public void TestOverallDisposition()
		{
			SalesEnquiry enquiry = Factory.New<SalesEnquiry>();
			enquiry.O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Open;
			AssertEquals(enquiry.O1_LeadStatus, SalesEnquiryStatusCodeList.Descriptions.Open, enquiry.OverallDispositionDescription);
			enquiry.O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Converted;
			AssertEquals(enquiry.O1_LeadStatus, SalesEnquiryStatusCodeList.Descriptions.Closed, enquiry.OverallDispositionDescription);
			enquiry.O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Closed;
			AssertEquals(enquiry.O1_LeadStatus, SalesEnquiryStatusCodeList.Descriptions.Closed, enquiry.OverallDispositionDescription);
		}

		public void TestIsNotOpen()
		{
			SalesEnquiry enquiry = Factory.New<SalesEnquiry>();
			enquiry.O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Open;
			Assert("OPN should be considered open", !enquiry.IsNotOpen);
			enquiry.O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Converted;
			Assert("CNV should be considered closed", enquiry.IsNotOpen);
			enquiry.O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Closed;
			Assert("CLS should be considered closed", enquiry.IsNotOpen);
		}

		public void TestReferringContactNameReadOnlyAndGetsReset()
		{
			var referringOrg = Factory.New<OrgHeader>();
			var enquiry = Factory.New<SalesEnquiry>();
			Assert("Should be read-only when referring org has not been set", enquiry.ReferringContactNameInfo.ReadOnly);

			enquiry.O1_OH_SourceOfLead = ZGuid.Invalid;
			Assert("Should be read-only when referring org is invalid", enquiry.ReferringContactNameInfo.ReadOnly);

			enquiry.O1_OH_SourceOfLead = referringOrg.PK;
			Assert("Should be writeable when referring org is valid", !enquiry.ReferringContactNameInfo.ReadOnly);

			enquiry.ReferringContactName = "Jenny";
			enquiry.O1_OH_SourceOfLead = ZGuid.Empty;

			Assert("ReferringContactName should be reset when referring org is erased", enquiry.ReferringContactName.IsEmpty);
		}

		public void TestReferToContactNameReadOnlyAndGetsReset()
		{
			var referToOrg = Factory.New<OrgHeader>();
			var enquiry = Factory.New<SalesEnquiry>();
			Assert("Should be read-only when refer to org has not been set", enquiry.ReferToContactNameInfo.ReadOnly);

			enquiry.O1_OH_ReferTo = ZGuid.Invalid;
			Assert("Should be read-only when refer to org is invalid", enquiry.ReferToContactNameInfo.ReadOnly);

			enquiry.O1_OH_ReferTo = referToOrg.PK;
			Assert("Should be editable when refer to org is valid", !enquiry.ReferToContactNameInfo.ReadOnly);

			enquiry.ReferToContactName = "Jenny";
			enquiry.O1_OH_ReferTo = ZGuid.Empty;

			Assert("ReferToContactName should be reset when refer to org is erased", enquiry.ReferToContactName.IsEmpty);
		}

		public void TestPreSaveValidation_CreateReferralContactsIfLinkedContactMissing()
		{
			CodeDescriptionBoolCollection leadInterests = new CodeDescriptionBoolCollection();
			leadInterests.Add("HEY", null, true);
			OrganisationsDataRegistry.Instance.SalesEnquiryLeadInterests.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, leadInterests);

			var existingOrg = Factory.NewWithValidTestData<OrgHeader>();
			existingOrg.Contacts.AddNew().OC_ContactName = "Jenny";

			var referringOrg = Factory.NewWithValidTestData<OrgHeader>();
			var activeReferringContact = referringOrg.Contacts.AddNew();
			activeReferringContact.OC_ContactName = "Active Contact";
			activeReferringContact.OC_IsActive = true;
			var inactiveReferringContact = referringOrg.Contacts.AddNew();
			inactiveReferringContact.OC_ContactName = "Inactive Contact";
			inactiveReferringContact.OC_IsActive = false;

			var referToOrg = Factory.NewWithValidTestData<OrgHeader>();
			var activeReferToContact = referToOrg.Contacts.AddNew();
			activeReferToContact.OC_ContactName = "Active Contact";
			activeReferToContact.OC_IsActive = true;
			var inactiveReferToContact = referToOrg.Contacts.AddNew();
			inactiveReferToContact.OC_ContactName = "Inactive Contact";
			inactiveReferToContact.OC_IsActive = false;

			AssertEquals("Precondition: 1 active and 1 inactive contact created", 2, referringOrg.Contacts.Count);

			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry.O1_EnquiryType = SalesEnquiry.Codes.SalesEnquiry;
			enquiry.O1_OH_SourceOfLead = referringOrg.PK;
			enquiry.O1_OH_ReferTo = referToOrg.PK;

			enquiry.ReferringContactName = activeReferringContact.OC_ContactName;
			enquiry.ReferToContactName = activeReferToContact.OC_ContactName;
			enquiry.RunPreSaveValidation();
			AssertEquals("Should not have created another Active Contact for Referring Org", 2, referringOrg.Contacts.Count);
			AssertEquals("Should not have created another Active Contact for Refer To Org", 2, referToOrg.Contacts.Count);
			AssertEquals("Link exists between Active Referring Contact and enquiry", activeReferringContact.PK, enquiry.O1_OC_ReferringContact);
			AssertEquals("Link exists between Active Refer To Contact and enquiry", activeReferToContact.PK, enquiry.O1_OC_ReferToContact);

			enquiry.ReferringContactName = inactiveReferringContact.OC_ContactName;
			enquiry.ReferToContactName = inactiveReferToContact.OC_ContactName;
			enquiry.RunPreSaveValidation();
			AssertEquals("Should not have created another Inactive Contact for Referring Org", 2, referringOrg.Contacts.Count);
			AssertEquals("Should not have created another Inactive Contact for Refer To Org", 2, referToOrg.Contacts.Count);
			AssertEquals("Link should (temporarily in the factory) exist between Inactive Referring Contact and enquiry", inactiveReferringContact.PK, enquiry.O1_OC_ReferringContact);
			AssertEquals("Link should (temporarily in the factory) exist between Inactive Refer To Contact and enquiry", inactiveReferToContact.PK, enquiry.O1_OC_ReferToContact);

			enquiry.ReferringContactName = "Jenny's twin";
			enquiry.ReferToContactName = "Jenny's other twin";
			enquiry.O1_InterestLevel = "HAI";
			AssertEquals("Pre-condition: enquiry has validation errors", true, enquiry.HasErrors());
			enquiry.RunPreSaveValidation();
			AssertEquals("Should not have created the referring contact when there's a validation error", 2, referringOrg.Contacts.Count);
			AssertEquals("Should not have created the refer to contact when there's a validation error", 2, referToOrg.Contacts.Count);

			enquiry.ReferringContactName = "Jenny";
			enquiry.ReferToContactName = "Jenny";
			enquiry.O1_InterestLevel = "HEY";
			AssertEquals("Pre-condition: enquiry has no validation errors", false, enquiry.HasErrors());
			enquiry.RunPreSaveValidation();
			AssertEquals("Should have created the referring contact", 3, referringOrg.Contacts.Count);
			AssertEquals("Should have created the refer to contact", 3, referToOrg.Contacts.Count);
			var newReferringContact = referringOrg.Contacts.Find(new ZQuery(OrgContactSchema.OC_ContactName, "Jenny")).ToArray()[0];
			var newReferToContact = referToOrg.Contacts.Find(new ZQuery(OrgContactSchema.OC_ContactName, "Jenny")).ToArray()[0];
			AssertEquals("New contact should be binded to enquiry as referring contact", newReferringContact.PK, enquiry.O1_OC_ReferringContact);
			AssertEquals("New contact should be binded to enquiry as refer to contact", newReferToContact.PK, enquiry.O1_OC_ReferToContact);

			int contactCount = new OrgContactCollection(Factory).Count;
			enquiry.O1_OH_SourceOfLead = ZGuid.Empty;
			enquiry.O1_OH_ReferTo = ZGuid.Empty;
			enquiry.ReferringContactName = "Anthony";
			enquiry.ReferToContactName = "Maria";
			enquiry.RunPreSaveValidation();
			AssertEquals("Should not have created another contact because there is no referral org", contactCount, new OrgContactCollection(Factory).Count);
		}

		public void TestPreSaveValidation_CreateReferralContactsIfLinkedContactMissing_WhenRelatedToMainOrg()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Master name";

			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry.O1_OH_ConvertedToQualifiedLead = org.PK;
			enquiry.O1_OH_SourceOfLead = org.PK;
			enquiry.O1_OH_ReferTo = org.PK;
			enquiry.O1_ContactName = "Andrew";
			enquiry.ReferringContactName = "Referring name";
			enquiry.ReferToContactName = "Refer To name";

			AssertEquals("Precondition: No contacts created", 0, org.Contacts.Count);
			Assert("Precondition: Enquiry has no contact errors", !enquiry.HasContactErrors);

			enquiry.RunPreSaveValidation();
			AssertEquals("Should have created contacts during RunPreSaveValidation", 3, org.Contacts.Count);

			Factory.Save();
			AssertEquals("Should have same number of contacts after saving", 3, org.Contacts.Count);
		}

		public void TestSettingReferringContactName_UpdatesLink()
		{
			var referringOrg = Factory.NewWithValidTestData<OrgHeader>();
			var referringContact = referringOrg.Contacts.AddNew();
			referringContact.OC_ContactName = "Jenny";

			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry.O1_OH_SourceOfLead = referringOrg.PK;
			enquiry.ReferringContactName = "Jenny";
			Factory.Save();

			AssertEquals("Link is present", referringContact.PK, enquiry.O1_OC_ReferringContact);
			Factory.Save();
			AssertEquals("Link is still present after saving", referringContact.PK, enquiry.O1_OC_ReferringContact);

			enquiry.ReferringContactName = ZString.Empty;
			AssertEquals("Link is empty", ZGuid.Empty, enquiry.O1_OC_ReferringContact);
			Factory.Save();
			AssertEquals("Link is still empty after saving", ZGuid.Empty, enquiry.O1_OC_ReferringContact);
		}

		public void TestSettingReferringContactName_UpdatesReferringContactIgnoringCase()
		{
			var referringOrg = Factory.New<OrgHeader>();
			var referringContact = referringOrg.Contacts.AddNew();
			referringContact.OC_ContactName = "Jenny";
			var enquiry = Factory.New<SalesEnquiry>();
			enquiry.O1_OH_SourceOfLead = referringOrg.PK;

			enquiry.O1_OC_ReferringContact = ZGuid.Empty;
			enquiry.ReferringContactName = "JENNY";
			AssertEquals(referringContact.PK, enquiry.O1_OC_ReferringContact);

			enquiry.O1_OC_ReferringContact = ZGuid.Empty;
			enquiry.ReferringContactName = "jenny";
			AssertEquals(referringContact.PK, enquiry.O1_OC_ReferringContact);

			enquiry.O1_OC_ReferringContact = ZGuid.Empty;
			enquiry.ReferringContactName = "Jenny";
			AssertEquals(referringContact.PK, enquiry.O1_OC_ReferringContact);

			enquiry.O1_OC_ReferringContact = ZGuid.Empty;
			enquiry.ReferringContactName = "Anthony";
			AssertEquals(ZGuid.Empty, enquiry.O1_OC_ReferringContact);
		}

		public void TestSettingReferToContactName_UpdatesLink()
		{
			var referToOrg = Factory.NewWithValidTestData<OrgHeader>();
			var referToContact = referToOrg.Contacts.AddNew();
			referToContact.OC_ContactName = "Jenny";

			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry.O1_OH_ReferTo = referToOrg.PK;
			enquiry.ReferToContactName = "Jenny";
			Factory.Save();

			AssertEquals("Link is present", referToContact.PK, enquiry.O1_OC_ReferToContact);

			enquiry.ReferToContactName = ZString.Empty;
			AssertEquals("Link is empty", ZGuid.Empty, enquiry.O1_OC_ReferToContact);
			Factory.Save();
			AssertEquals("Link is still empty after saving", ZGuid.Empty, enquiry.O1_OC_ReferToContact);

			enquiry.ReferToContactName = "JENNY";
			AssertEquals("Link is present ignoring case", referToContact.PK, enquiry.O1_OC_ReferToContact);

			enquiry.ReferToContactName = "Anthony";
			AssertEquals("Link is empty", ZGuid.Empty, enquiry.O1_OC_ReferToContact);
		}

		public void TestDoClose()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "Some Enquiry Org";
			org.MainAddress.OA_Address1 = "1 Street";
			org.OH_RL_NKClosestPort = "AUSYD";

			SalesEnquiry enquiry = Factory.New<SalesEnquiry>();
			enquiry.OrgPk = org.PK;
			var task1 = enquiry.WorkflowItems.AddNew();
			var task2 = enquiry.WorkflowItems.AddNew();
			var task3 = enquiry.WorkflowItems.AddNew();
			var task4 = enquiry.WorkflowItems.AddNew();
			var task5 = enquiry.WorkflowItems.AddNew();
			var task6 = enquiry.WorkflowItems.AddNew();
			var taskException = enquiry.WorkflowItems.AddNew();
			var taskException2 = enquiry.WorkflowItems.AddNew();
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task5.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			task6.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			taskException.P9_Status = ExceptionStatusCodeList.Codes.Actioned;
			taskException.P9_Type = Core.Constants.Workflow.ExceptionType;
			taskException2.P9_Status = ExceptionStatusCodeList.Codes.Open;
			taskException2.P9_Type = Core.Constants.Workflow.ExceptionType;
			Factory.Save();

			AssertEquals("initial status", SalesEnquiryStatusCodeList.Codes.Open, enquiry.O1_LeadStatus);
			AssertEquals("OH_IsActive", true, enquiry.Header.OH_IsActive);

			enquiry.DoClose();
			AssertEquals("status", SalesEnquiryStatusCodeList.Codes.Closed, enquiry.O1_LeadStatus);
			AssertEquals("Working task should be closed", ProcessTaskStatusCodeList.Codes.Closed, task1.P9_Status);
			AssertEquals("Assigned task should be cancelled", ProcessTaskStatusCodeList.Codes.Cancelled, task2.P9_Status);
			AssertEquals("Cancelled task should remain cancelled", ProcessTaskStatusCodeList.Codes.Cancelled, task3.P9_Status);
			AssertEquals("Closed task should remain closed", ProcessTaskStatusCodeList.Codes.Closed, task4.P9_Status);
			AssertEquals("Suspended task should be closed", ProcessTaskStatusCodeList.Codes.Closed, task5.P9_Status);
			AssertEquals("Open task should be cancelled", ProcessTaskStatusCodeList.Codes.Cancelled, task6.P9_Status);
			AssertEquals("Exception status does not change", ExceptionStatusCodeList.Codes.Actioned, taskException.P9_Status);
			AssertEquals("Exception status does not change", ExceptionStatusCodeList.Codes.Open, taskException2.P9_Status);
		}

		public void TestCanDoClose()
		{
			SalesEnquiry enquiry = Factory.New<SalesEnquiry>();
			enquiry.O1_CompanyName = "Some Enquiry Org";
			enquiry.O1_Address1 = "1 Street";
			AssertEquals("CanDoClose", false, enquiry.CanDoClose);

			Factory.Save();
			AssertEquals("CanDoClose", true, enquiry.CanDoClose);

			enquiry.DoClose();
			AssertEquals("CanDoClose when in DB and already closed", false, enquiry.CanDoClose);

			enquiry.O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Converted;
			AssertEquals("CanDoClose when in DB and Converted", false, enquiry.CanDoClose);
		}

		public void TestReopen()
		{
			SalesEnquiry enquiry = Factory.New<SalesEnquiry>();
			enquiry.DoClose();
			AssertEquals(SalesEnquiryStatusCodeList.Codes.Closed, enquiry.O1_LeadStatus);

			enquiry.Reopen();
			AssertEquals(SalesEnquiryStatusCodeList.Codes.Open, enquiry.O1_LeadStatus);
		}

		public void TestReopenMany()
		{
			Env.Security.InquiryManagerEdit.IsAllowed = true;

			var enquiry1 = Factory.New<SalesEnquiry>();
			var enquiry2 = Factory.New<SalesEnquiry>();
			var enquiry3 = Factory.New<SalesEnquiry>();
			var enquiry4 = Factory.New<SalesEnquiry>();

			enquiry1.DoClose();
			enquiry2.DoClose();
			enquiry4.DoClose();

			AssertEquals(SalesEnquiryStatusCodeList.Codes.Closed, enquiry1.O1_LeadStatus);
			AssertEquals(SalesEnquiryStatusCodeList.Codes.Closed, enquiry2.O1_LeadStatus);
			AssertEquals(SalesEnquiryStatusCodeList.Codes.Open, enquiry3.O1_LeadStatus);
			AssertEquals(SalesEnquiryStatusCodeList.Codes.Closed, enquiry4.O1_LeadStatus);

			var notification = SalesEnquiry.Reopen(Factory, new ZGuid[] { enquiry1.PK, enquiry2.PK, enquiry3.PK });
			AssertEquals("notification", CargoWise.EntityFramework.NotificationType.Information, notification.Type);
			AssertEquals("message", "Selected closed Inquiries are reopened.", notification.Message);

			AssertEquals(SalesEnquiryStatusCodeList.Codes.Open, enquiry1.O1_LeadStatus);
			AssertEquals(SalesEnquiryStatusCodeList.Codes.Open, enquiry2.O1_LeadStatus);
			AssertEquals(SalesEnquiryStatusCodeList.Codes.Open, enquiry3.O1_LeadStatus);
			AssertEquals(SalesEnquiryStatusCodeList.Codes.Closed, enquiry4.O1_LeadStatus);

			Env.Security.InquiryManagerEdit.IsAllowed = false;
			enquiry1.DoClose();
			enquiry2.DoClose();
			enquiry4.DoClose();
			notification = SalesEnquiry.Reopen(Factory, new ZGuid[] { enquiry1.PK, enquiry2.PK, enquiry3.PK });
			AssertEquals("notification", CargoWise.EntityFramework.NotificationType.Error, notification.Type);
			AssertEquals("notification", SalesEnquiry.NoEditSecurityMsg, notification.Message);
			AssertEquals(SalesEnquiryStatusCodeList.Codes.Closed, enquiry1.O1_LeadStatus);
			AssertEquals(SalesEnquiryStatusCodeList.Codes.Closed, enquiry2.O1_LeadStatus);
			AssertEquals(SalesEnquiryStatusCodeList.Codes.Open, enquiry3.O1_LeadStatus);
			AssertEquals(SalesEnquiryStatusCodeList.Codes.Closed, enquiry4.O1_LeadStatus);
		}

		public void TestSetLeadStatus()
		{
			var enquiry = Factory.New<SalesEnquiry>();
			enquiry.O1_CompanyName = "Some Enquiry Org";
			enquiry.O1_Address1 = "1 Street";

			var task1 = enquiry.WorkflowItems.AddNew();
			var task2 = enquiry.WorkflowItems.AddNew();
			var task3 = enquiry.WorkflowItems.AddNew();
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			AssertEquals("CanDoClose", false, enquiry.CanDoClose);
			enquiry.LeadStatus = SalesEnquiryStatusCodeList.Codes.Closed;
			AssertNotEquals("O1_LeadStatus not set", SalesEnquiryStatusCodeList.Codes.Closed, enquiry.O1_LeadStatus);
			AssertNotEquals("task 1 not set", ProcessTaskStatusCodeList.Codes.Closed, task1.P9_Status);
			AssertNotEquals("task 2 not set", ProcessTaskStatusCodeList.Codes.Closed, task2.P9_Status);

			Factory.Save();
			AssertEquals("CanDoClose", true, enquiry.CanDoClose);
			enquiry.LeadStatus = SalesEnquiryStatusCodeList.Codes.Closed;
			AssertEquals("O1_LeadStatus set", SalesEnquiryStatusCodeList.Codes.Closed, enquiry.O1_LeadStatus);
			AssertEquals("task 1 set", ProcessTaskStatusCodeList.Codes.Closed, task1.P9_Status);
			AssertEquals("task 2 set", ProcessTaskStatusCodeList.Codes.Cancelled, task2.P9_Status);
			AssertEquals("task 3 not set", ProcessTaskStatusCodeList.Codes.Cancelled, task3.P9_Status);

			enquiry.LeadStatus = SalesEnquiryStatusCodeList.Codes.Open;
			AssertEquals("O1_LeadStatus set", SalesEnquiryStatusCodeList.Codes.Open, enquiry.O1_LeadStatus);
		}

		public void TestCreateOrg()
		{
			AssertCreateOrg(isStaffAssignmentRightAllowed: true);
		}

		public void TestCreateOrg_WithStaffAssignmentRightDenied()
		{
			AssertCreateOrg(isStaffAssignmentRightAllowed: false);
		}

		void AssertCreateOrg(bool isStaffAssignmentRightAllowed)
		{
			Env.Security.OrgDetailsModifyStaffAssignmentsLookup[StaffAssignmentRoles.Codes.SalesRep].IsAllowed = isStaffAssignmentRightAllowed;

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "SCW";

			SalesEnquiry enquiry = Factory.New<SalesEnquiry>();
			enquiry.O1_CompanyName = "Sam Test Org";
			enquiry.O1_Address1 = "111 Test Drive";
			enquiry.O1_Address2 = "A4";
			enquiry.O1_City = "Sydney";
			enquiry.O1_State = "NSW";
			enquiry.O1_PostCode = "2000";
			enquiry.O1_PortOrCountry = "AUSYD";
			enquiry.O1_BusinessRegNo = "53 004 085 616";
			enquiry.O1_WebAddress = "www.test.org";
			enquiry.O1_ContactName = "Sam";
			enquiry.O1_Phone = "02 9999 8888";
			enquiry.O1_Mobile = "0423 999 888";
			enquiry.O1_Fax = "02 8888 7777";
			enquiry.O1_Email = "sam@test.com";
			enquiry.O1_JobCategory = "System Tester";
			enquiry.O1_LeadSource = "AAA";
			enquiry.O1_OpportunitySourceDetails = "AAA Details";
			enquiry.O1_GS_NKRepAssigned = "SCW";
			var noteContent = new ZBlob(new byte[] { 1, 1, 1, 1 });
			enquiry.EnquiryNotesContent = noteContent;

			Factory.Save();

			var org = enquiry.CreateOrg(Factory);
			AssertEquals("SAM TEST ORG", org.OH_FullName);
			AssertEquals("111 TEST DRIVE", org.MainAddress.OA_Address1);
			AssertEquals("A4", org.MainAddress.OA_Address2);
			AssertEquals("SYDNEY", org.MainAddress.OA_City);
			AssertEquals("NSW", org.MainAddress.OA_State);
			AssertEquals("2000", org.MainAddress.OA_PostCode);
			AssertEquals("AUSYD", org.OH_RL_NKClosestPort);
			AssertEquals("53 004 085 616", org.PrimaryRegistrationNumber.Number);
			AssertEquals("WWW.TEST.ORG", org.MainWebURL.PU_URL);

			if (isStaffAssignmentRightAllowed)
			{
				AssertEquals("SCW", org.StaffAssignments.OverallSalesRepStaff.GS_Code);
			}
			else
			{
				AssertEquals("No staff assignments should be created when the Modify Staff Assignments security right is denied", 0, org.StaffAssignments.Count);
			}

			AssertEquals(1, org.Contacts.Count);
			var contact = org.Contacts[0];
			AssertEquals("Sam", contact.OC_ContactName);
			AssertEquals("02 9999 8888", contact.OC_Phone);
			AssertEquals("0423 999 888", contact.OC_Mobile);
			AssertEquals("02 8888 7777", contact.OC_Fax);
			AssertEquals("sam@test.com", contact.OC_Email);
			AssertEquals("System Tester", contact.OC_JobCategory);
			Assert(contact.OC_WebAccessEnabled_ReadOnly);
		}

		public void TestPopulateOrg()
		{
			SalesEnquiry enquiry = Factory.New<SalesEnquiry>();
			enquiry.O1_CompanyName = "Sam Test Org";
			enquiry.O1_Address1 = "111 Test Drive";
			enquiry.O1_Address2 = "A4";
			enquiry.O1_City = "Sydney";
			enquiry.O1_State = "NSW";
			enquiry.O1_PostCode = "2000";
			enquiry.O1_PortOrCountry = "AUSYD";
			enquiry.O1_BusinessRegNo = "53 004 085 616";
			enquiry.O1_WebAddress = "www.test.org";
			enquiry.O1_ContactName = "Sam";
			enquiry.O1_Phone = "02 9999 8888";
			enquiry.O1_Mobile = "0423 999 888";
			enquiry.O1_Fax = "02 8888 7777";
			enquiry.O1_Email = "sam@test.com";
			enquiry.O1_JobCategory = "System Tester";
			enquiry.O1_LeadSource = "AAA";
			enquiry.O1_OpportunitySourceDetails = "AAA Details";
			enquiry.O1_GS_NKRepAssigned = "SCW";
			var noteContent = new ZBlob(new byte[] { 1, 1, 1, 1 });
			enquiry.EnquiryNotesContent = noteContent;

			Factory.Save();

			var org = Factory.New<OrgHeader>();
			enquiry.PopulateOrg(org);
			AssertEquals("SAM TEST ORG", org.OH_FullName);
			AssertEquals("111 TEST DRIVE", org.MainAddress.OA_Address1);
			AssertEquals("A4", org.MainAddress.OA_Address2);
			AssertEquals("SYDNEY", org.MainAddress.OA_City);
			AssertEquals("NSW", org.MainAddress.OA_State);
			AssertEquals("2000", org.MainAddress.OA_PostCode);
			AssertEquals("AUSYD", org.OH_RL_NKClosestPort);
			AssertEquals("53 004 085 616", org.PrimaryRegistrationNumber.Number);
			AssertEquals("WWW.TEST.ORG", org.MainWebURL.PU_URL);

			AssertEquals(0, org.Contacts.Count);
		}

		public void TestCanConvertToOpportunity()
		{
			var enquiry = Factory.New<SalesEnquiry>();
			AssertEquals("CanConvertToOpportunity when Open", true, enquiry.CanConvertToOpportunity);

			enquiry.O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Closed;
			AssertEquals("CanConvertToOpportunity when Closed", false, enquiry.CanConvertToOpportunity);

			enquiry.O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Converted;
			AssertEquals("CanConvertToOpportunity when Converted", false, enquiry.CanConvertToOpportunity);
		}

		public void TestIsEnquiryNotesContentInRTF()
		{
			SalesEnquiry enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry.EnquiryNotesContent = ZBlob.FromUTF8("test note ");
			AssertEquals(true, ORtfTextUtil.IsRtf(enquiry.EnquiryNotesContent.ToUTF8()));
		}

		// If this test fails because the ST_Description, ST_NoteType, etc, has changed,
		// Report_SalesMarketingInquiryV2 will need to be modified so EnquiryNotes continues to be selected at StmNote join
		public void TestEnquiryNotesProperties()
		{
			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry.EnquiryNotesContent = ZBlob.FromUTF8("notes on details tab");
			Factory.Save();

			var query = new ZQuery(StmNoteSchema.ST_ParentID, enquiry.PK);
			query.AddToFilter(StmNoteSchema.ST_Table, enquiry.TableName);
			query.AddToFilter(StmNoteSchema.ST_Description, "Blob Details");
			query.AddToFilter(StmNoteSchema.ST_NoteType, nameof(StmNoteVisibility.DOC));

			var expectedNote = Factory.LoadTop1<StmNote>(query);
			AssertNotNull(expectedNote);
			AssertEquals("notes on details tab", expectedNote.ST_NoteDataAsText);
		}

		public void TestOnConvertedToOpportunity()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "Some Enquiry Org";
			org.MainAddress.OA_Address1 = "1 Street";
			org.OH_RL_NKClosestPort = "AUSYD";
			var opp = org.SalesOpportunities.AddNew();

			var enquiry = Factory.New<SalesEnquiry>();
			enquiry.OrgPk = org.PK;

			enquiry.OnConvertedToOpportunity(opp);

			AssertEquals("O1_LeadStatus", SalesEnquiryStatusCodeList.Codes.Converted, enquiry.O1_LeadStatus);
			AssertEquals("factory saved", true, enquiry.IsInDatabase);
		}

		public void TestDelete()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var enquiry = Factory.New<SalesEnquiry>();
			enquiry.OrgPk = org.PK;

			var opportunity = Factory.New<OrgOpportunity>();
			opportunity.P8_OH = org.PK;
			opportunity.P8_O1_Enquiry = enquiry.PK;

			Factory.Save();

			BusinessObjectFactory deleteFactory = new BusinessObjectFactory();
			var loadedEnquiry = deleteFactory.Load<SalesEnquiry>(enquiry.PK);
			loadedEnquiry.Delete();
			deleteFactory.Save();

			AssertEquals(ZGuid.Empty, opportunity.P8_O1_Enquiry);
		}

		[TestDate(2015, 2, 6)]
		public void TestIsNDR()
		{
			SalesEnquiry contact1 = Factory.NewWithValidTestData<SalesEnquiry>();
			contact1.O1_ContactName = "Tiny Small";

			AssertEquals(false, contact1.IsNDR);

			contact1.IsNDR = false;
			Assert(!contact1.IsNDR);
			AssertNotNull(contact1.EmailAddress);
			AssertEquals("VLD Status should be set", "VLD", contact1.EmailAddress.GI_DeliveryStatus);
			AssertEquals("VLD Date is set", new ZDateTime(2015, 2, 6), contact1.EmailAddress.GI_DeliveryReportTimeUtc);

			contact1.IsNDR = true;
			Assert(contact1.IsNDR);
			AssertNotNull(contact1.EmailAddress);
			AssertEquals("NDR Status should be set", "NDR", contact1.EmailAddress.GI_DeliveryStatus);
			AssertEquals("NDR Date is set", new ZDateTime(2015, 2, 6), contact1.EmailAddress.GI_DeliveryReportTimeUtc);
		}

		#region Create or Update Contact

		public void TestCreateOrUpdateContact_CreateContact()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "SAMTESSYD";
			org.OH_FullName = "Sam Test Org";

			Factory.Save();

			SalesEnquiry enquiry = Factory.New<SalesEnquiry>();
			enquiry.O1_ContactName = "Tester";
			enquiry.O1_Phone = "02 1111 2222";
			enquiry.O1_Mobile = "0401 222 333";
			enquiry.O1_Fax = "02 3333 4444";
			enquiry.O1_Email = "tester@sam.com";
			enquiry.O1_JobCategory = "System Admin";

			enquiry.OrgPk = org.PK;
			AssertNotEquals(ZGuid.Empty, enquiry.O1_OC_LinkedContact);
			AssertEquals("Tester", enquiry.O1_ContactName);
			AssertEquals("02 1111 2222", enquiry.O1_Phone);
			AssertEquals("0401 222 333", enquiry.O1_Mobile);
			AssertEquals("02 3333 4444", enquiry.O1_Fax);
			AssertEquals("tester@sam.com", enquiry.O1_Email);
			AssertEquals("System Admin", enquiry.O1_JobCategory);

			Factory.Save();

			var loadFactory = new BusinessObjectFactory();
			var loadedOrg = loadFactory.Load<OrgHeader>(org.PK);
			var loadedContact = loadFactory.Load<OrgContact>(enquiry.O1_OC_LinkedContact);
			AssertNotNull(loadedContact);
			AssertCollectionContains(loadedContact, loadedOrg.Contacts);
		}

		public void TestCreateOrUpdateContact_CreateContactButSelectExisting()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "SAMTESSYD";
			org.OH_FullName = "Sam Test Org";

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Sam";
			contact.OC_Phone = "02 9999 8888";
			contact.OC_Mobile = "0423 999 888";
			contact.OC_Email = "sam@test.com";
			contact.OC_Fax = "02 7777 4444";
			contact.OC_JobCategory = "System Tester";

			Factory.Save();

			SalesEnquiry enquiry = Factory.New<SalesEnquiry>();
			enquiry.O1_ContactName = "Tester";
			enquiry.O1_Phone = "02 1111 2222";
			enquiry.O1_Mobile = "0401 222 333";
			enquiry.O1_Fax = "02 3333 4444";
			enquiry.O1_Email = "tester@sam.com";
			enquiry.O1_JobCategory = "System Admin";

			enquiry.OrgPk = org.PK;
			AssertNotEquals(ZGuid.Empty, enquiry.O1_OC_LinkedContact);
			AssertEquals("Tester", enquiry.O1_ContactName);
			AssertEquals("02 1111 2222", enquiry.O1_Phone);
			AssertEquals("0401 222 333", enquiry.O1_Mobile);
			AssertEquals("02 3333 4444", enquiry.O1_Fax);
			AssertEquals("tester@sam.com", enquiry.O1_Email);
			AssertEquals("System Admin", enquiry.O1_JobCategory);

			enquiry.O1_OC_LinkedContact = contact.PK;
			AssertEquals("Sam", enquiry.O1_ContactName);
			AssertEquals("02 9999 8888", enquiry.O1_Phone);
			AssertEquals("0423 999 888", enquiry.O1_Mobile);
			AssertEquals("02 7777 4444", enquiry.O1_Fax);
			AssertEquals("sam@test.com", enquiry.O1_Email);
			AssertEquals("System Tester", enquiry.O1_JobCategory);

			Factory.Save();

			var loadFactory = new BusinessObjectFactory();
			var loadedOrg = loadFactory.Load<OrgHeader>(org.PK);
			AssertEquals(1, loadedOrg.Contacts.Count);
		}

		public void TestCreateOrUpdateContact_NewContactSwitchOrgs()
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "SAMTESSY1D";
			org1.OH_FullName = "Sam Test Org 111";

			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "SAMTESSYD2";
			org2.OH_FullName = "Sam Test Org 222";

			Factory.Save();

			SalesEnquiry enquiry = Factory.New<SalesEnquiry>();
			enquiry.O1_ContactName = "Tester";
			enquiry.O1_Phone = "02 1111 2222";
			enquiry.O1_Mobile = "0401 222 333";
			enquiry.O1_Fax = "02 3333 4444";
			enquiry.O1_Email = "tester@sam.com";
			enquiry.O1_JobCategory = "System Admin";

			enquiry.OrgPk = org1.PK;
			AssertNotEquals(ZGuid.Empty, enquiry.O1_OC_LinkedContact);
			AssertEquals("Tester", enquiry.O1_ContactName);
			AssertEquals("02 1111 2222", enquiry.O1_Phone);
			AssertEquals("0401 222 333", enquiry.O1_Mobile);
			AssertEquals("02 3333 4444", enquiry.O1_Fax);
			AssertEquals("tester@sam.com", enquiry.O1_Email);
			AssertEquals("System Admin", enquiry.O1_JobCategory);

			enquiry.OrgPk = org2.PK;
			AssertEquals(ZGuid.Empty, enquiry.O1_OC_LinkedContact);
			AssertEquals("", enquiry.O1_ContactName);
			AssertEquals("", enquiry.O1_Phone);
			AssertEquals("", enquiry.O1_Mobile);
			AssertEquals("", enquiry.O1_Fax);
			AssertEquals("", enquiry.O1_Email);
			AssertEquals("", enquiry.O1_JobCategory);

			Factory.Save();

			var loadFactory = new BusinessObjectFactory();
			var loadedOrg1 = loadFactory.Load<OrgHeader>(org1.PK);
			var loadedOrg2 = loadFactory.Load<OrgHeader>(org2.PK);
			AssertEquals(0, loadedOrg1.Contacts.Count);
			AssertEquals(0, loadedOrg2.Contacts.Count);
		}

		public void TestCreateOrUpdateContact_MatchingInactiveContactWhenSwitchOrgs()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "SAMTESSY1D";
			org.OH_FullName = "Sam Test Org 111";

			var inactiveContact = org.Contacts.AddNew();
			inactiveContact.OC_ContactName = "Sam";
			inactiveContact.OC_IsActive = false;

			Factory.Save();

			var enquiry = Factory.New<SalesEnquiry>();
			enquiry.O1_ContactName = inactiveContact.OC_ContactName;

			Assert("Precondition: Contact is inactive", !inactiveContact.OC_IsActive);
			Assert("Precondition: O1_ContactName should not have error", !enquiry.O1_ContactNameInfo.HasErrors());

			enquiry.OrgPk = org.PK;

			Assert("Contact should still be inactive", !inactiveContact.OC_IsActive);
			Assert("O1_ContactName should have inactive error now", enquiry.O1_ContactNameInfo.HasError("Select an active contact belonging to the organization."));
		}

		public void TestCreateOrUpdateContact_LoadContact()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "SAMTESSYD";
			org.OH_FullName = "Sam Test Org";

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Sam";
			contact.OC_Phone = "02 9999 8888";
			contact.OC_Mobile = "0423 999 888";
			contact.OC_Email = "sam@test.com";
			contact.OC_Fax = "02 7777 4444";
			contact.OC_JobCategory = "System Tester";

			Factory.Save();

			SalesEnquiry enquiry = Factory.New<SalesEnquiry>();
			enquiry.O1_ContactName = "Sam";
			enquiry.O1_Phone = "02 1111 2222";
			enquiry.O1_Mobile = "0401 222 333";
			enquiry.O1_Fax = "02 3333 4444";
			enquiry.O1_Email = "tester@sam.com";
			enquiry.O1_JobCategory = "System Admin";

			enquiry.OrgPk = org.PK;
			AssertEquals(contact.PK, enquiry.O1_OC_LinkedContact);
			AssertEquals("Sam", enquiry.O1_ContactName);
			AssertEquals("02 9999 8888", enquiry.O1_Phone);
			AssertEquals("0423 999 888", enquiry.O1_Mobile);
			AssertEquals("02 7777 4444", enquiry.O1_Fax);
			AssertEquals("sam@test.com", enquiry.O1_Email);
			AssertEquals("System Tester", enquiry.O1_JobCategory);

			Factory.Save();

			var loadFactory = new BusinessObjectFactory();
			var loadedOrg = loadFactory.Load<OrgHeader>(org.PK);
			AssertEquals(1, loadedOrg.Contacts.Count);
		}

		public void TestCreateOrUpdateContact_LoadContactUpdateDetails()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "SAMTESSYD";
			org.OH_FullName = "Sam Test Org";

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Sam";
			contact.OC_Phone = "02 9999 8888";
			contact.OC_Mobile = "";
			contact.OC_Email = "sam@test.com";
			contact.OC_Fax = "02 7777 4444";
			contact.OC_JobCategory = "System Tester";

			Factory.Save();

			SalesEnquiry enquiry = Factory.New<SalesEnquiry>();
			enquiry.O1_ContactName = "Sam";
			enquiry.O1_Phone = "";
			enquiry.O1_Mobile = "0401 222 333";
			enquiry.O1_Fax = "";
			enquiry.O1_Email = "tester@sam.com";
			enquiry.O1_JobCategory = "System Admin";

			enquiry.OrgPk = org.PK;
			AssertEquals(contact.PK, enquiry.O1_OC_LinkedContact);
			AssertEquals("Sam", enquiry.O1_ContactName);
			AssertEquals("02 9999 8888", enquiry.O1_Phone);
			AssertEquals("0401 222 333", enquiry.O1_Mobile);
			AssertEquals("02 7777 4444", enquiry.O1_Fax);
			AssertEquals("sam@test.com", enquiry.O1_Email);
			AssertEquals("System Tester", enquiry.O1_JobCategory);

			Factory.Save();

			var loadFactory = new BusinessObjectFactory();
			var loadedContact = loadFactory.Load<OrgContact>(enquiry.O1_OC_LinkedContact);
			AssertEquals("02 9999 8888", loadedContact.OC_Phone);
			AssertEquals("0401 222 333", loadedContact.OC_Mobile);
			AssertEquals("02 7777 4444", loadedContact.OC_Fax);
			AssertEquals("sam@test.com", loadedContact.OC_Email);
			AssertEquals("System Tester", loadedContact.OC_JobCategory);
		}

		public void TestCreateOrUpdateContact_OperationalAction()
		{
			// In a situation where the user does an operational action on O1_OC_LinkedContact
			// on an enquiry that is not linked to an existing org (so the linked contact is invalid
			// but is saved successfully because the validation error doesn't abort the operational action). 
			// Should be able to override O1_OC_LinkedContact with O1_ContactName to fix the mistake.

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Donald Duck";
			Factory.Save();

			SalesEnquiry enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry.O1_OC_LinkedContact = contact.PK;
			using (enquiry.GetValidationSuspender())
			{
				Factory.Save();

				AssertEquals("Pre-condition", "Donald Duck", enquiry.O1_ContactName);

				enquiry.O1_ContactName = "Mickey Mouse";
				AssertEquals("Should be Mickey Mouse, not Donald Duck", "Mickey Mouse", enquiry.O1_ContactName);
			}
		}

		#endregion

		public void TestJobCategoryDescription_SetsAndGetsCorrectly()
		{
			var jobCategories = new CodeDescriptionBoolCollection(OrgContactSchema.OC_JobCategory.MaxLength)
						{
								{ "LEA", (NoResString)"CEO/Managing Director", true },
								{ "CSM", (NoResString)"Customer Service Manager", true }
						};
			OrganisationsDataRegistry.Instance.ContactJobCategories.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, jobCategories);

			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();

			enquiry.JobCategoryDescription = "Customer Service Manager";
			AssertEquals("O1_JobCategory is set to the correct code value.", "CSM", enquiry.O1_JobCategory);
			AssertEquals("JobCategoryDescription returns correct description value", "Customer Service Manager", enquiry.JobCategoryDescription);
		}

		public void TestJobCategoryDescriptionMaxLength()
		{
			var salesEnquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			AssertEquals(AutoOrgColdCallRegister.Schema.O1_JobCategoryMaxLength, salesEnquiry.JobCategoryDescriptionInfo.MaxLength);
		}

		public void TestGetTemplateSelectionCriteria()
		{
			var enquiry = Factory.New<SalesEnquiry>();
			enquiry.O1_EnquiryType = "EEE";
			enquiry.O1_LeadSource = "LLL";
			IWorkflowProviderCore provider = enquiry;
			var criteria = (ColumnValueRanker)provider.GetTemplateSelectionCriteria();
			AssertEquals("P0_SubType1 is O1_EnquiryType", "EEE", criteria.GetValues(ProcessTaskTemplateSchema.P0_SubType1)[0].ToString());
			AssertEquals("P0_SubType2 is O1_LeadSource", "LLL", criteria.GetValues(ProcessTaskTemplateSchema.P0_SubType2)[0].ToString());
			AssertEquals("P0_SubType3 is SalesEnquiryOrgRelationshipCodeList.Codes.NewOrg", SalesEnquiryOrgRelationshipCodeList.Codes.NewOrg, criteria.GetValues(ProcessTaskTemplateSchema.P0_SubType3)[0].ToString());
		}

		public void TestHasContactErrors()
		{
			var enquiry = Factory.New<SalesEnquiry>();
			AssertEquals("No contact details yet", false, enquiry.HasContactErrors);

			enquiry.O1_Email = "123";
			AssertEquals("Contact name is blank", false, enquiry.HasContactErrors);

			enquiry.O1_ContactName = "Sam";
			AssertEquals("Invalid email address", true, enquiry.HasContactErrors);

			enquiry.O1_Email = "123@test.com";
			AssertEquals("Valid email address", false, enquiry.HasContactErrors);
		}

		[TestedType(typeof(SalesEnquiry))]
		class CustomFieldsTest : TestICustomFieldProvider
		{
		}

		#region IImportParentRelatedActivityInfoOnNew

		public void TestImportParentRelatedActivityInfoOnNew_Inquiry()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ADL";
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTAA";
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "test@test.com";
			var inquiry = Factory.New<SalesEnquiry>();
			inquiry.O1_EnquiryType = SalesEnquiry.Codes.SalesEnquiry;
			inquiry.O1_OH_ConvertedToQualifiedLead = org.PK;
			inquiry.O1_OC_LinkedContact = contact.PK;
			inquiry.O1_GS_NKRepAssigned = staff.GS_Code;
			inquiry.EnquiryNotesContent = ZBlob.FromAscii("hello notes");

			var inquiry2 = Factory.New<SalesEnquiry>();
			((IImportParentRelatedActivityInfoOnNew)inquiry2).ImportParentInfo(inquiry, new ImportRelatedActivityNoDecisionFactory());

			CombineAssertions(() =>
			{
				AssertEquals("O1_EnquiryType", SalesEnquiry.Codes.SalesEnquiry, inquiry.O1_EnquiryType);
				AssertEquals("O1_OH_ConvertedToQualifiedLead", org.PK, inquiry2.O1_OH_ConvertedToQualifiedLead);
				AssertEquals("O1_OA_LinkedAddress", org.MainAddress.PK, inquiry2.O1_OA_LinkedAddress);
				AssertEquals("O1_OC_LinkedContact", contact.PK, inquiry2.O1_OC_LinkedContact);
				AssertEquals("O1_GS_NKRepAssigned", staff.GS_Code, inquiry2.O1_GS_NKRepAssigned);
				AssertEquals("EnquiryNotesContent", ORtfTextUtil.TextToRtf("hello notes"), inquiry2.EnquiryNotesContent.ToAscii());
			});
		}

		public void TestImportParentRelatedActivityInfoOnNew_Communication()
		{
			var org = Factory.New<OrgHeader>();
			var primaryContact = Factory.New<OrgContact>();
			var communication = Factory.New<OrgSalesCall>();
			communication.OQ_OH = org.PK;
			communication.OQ_OC = primaryContact.PK;
			communication.OQ_GS_NKSalesRep = "ADL";
			communication.OQ_SalesCallNotes = ZBlob.FromUTF8("Hello");

			var inquiry = Factory.New<SalesEnquiry>();
			((IImportParentRelatedActivityInfoOnNew)inquiry).ImportParentInfo(communication, new ImportRelatedActivityNoDecisionFactory());

			CombineAssertions(() =>
			{
				AssertEquals("O1_OH_ConvertedToQualifiedLead", org.PK, inquiry.O1_OH_ConvertedToQualifiedLead);
				AssertEquals("O1_OA_LinkedAddress", org.MainAddress.PK, inquiry.O1_OA_LinkedAddress);
				AssertEquals("O1_OC_LinkedContact", primaryContact.PK, inquiry.O1_OC_LinkedContact);
				AssertEquals("O1_GS_NKRepAssigned", "ADL", inquiry.O1_GS_NKRepAssigned);
				AssertEquals("EnquiryNotesContent", ORtfTextUtil.TextToRtf("Hello"), inquiry.EnquiryNotesContent.ToUTF8());
			});
		}

		#endregion

		#region IImportChildRelatedActivityInfoOnAttach

		public void TestImportChildRelatedActivityInfoOnAttach_Opportunity()
		{
			var inquiry = Factory.New<SalesEnquiry>();
			var opportunity = Factory.New<OrgOpportunity>();

			var importDeciderFactory = new ImportRelatedActivityNoDecisionFactory();
			var mockYesNoDecider = new Mock<IImportRelatedActivityYesNoDecider>();
			mockYesNoDecider
				.Setup(m => m.GetDecision(string.Format("Convert {0} to Opportunity?", inquiry.HumanReadableName)))
				.Returns(true);
			importDeciderFactory.AddDecider(typeof(IImportRelatedActivityYesNoDecider), mockYesNoDecider.Object);

			((IImportChildRelatedActivityInfoOnAttach)inquiry).ImportChildInfo(opportunity, importDeciderFactory);

			CombineAssertions(() =>
			{
				AssertEquals("O1_LeadStatus", SalesEnquiryStatusCodeList.Codes.Converted, inquiry.O1_LeadStatus);
				AssertEquals("ReadOnly", true, inquiry.ReadOnly);
				AssertEquals("P8_O1_Enquiry", inquiry.PK, opportunity.P8_O1_Enquiry);
			});
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			var enquiry = Factory.New<SalesEnquiry>();
			enquiry.O1_LeadUniqueReference = string.Empty;
			AssertEquals("HumanReadableName", "Inquiry", enquiry.HumanReadableName);
			enquiry.O1_LeadUniqueReference = "code";
			AssertEquals("HumanReadableName", "Inquiry" + " (code)", enquiry.HumanReadableName);
		}

		#endregion

		#region TestHumanReadableShortcutName

		public void TestHumanReadableShortcutName()
		{
			var enquiry = Factory.New<SalesEnquiry>();

			enquiry.O1_LeadUniqueReference = "code";
			AssertEquals("code", enquiry.HumanReadableShortcutName);

			enquiry.O1_EnquiryType = "WEB";
			AssertEquals("code - WEB", enquiry.HumanReadableShortcutName);

			enquiry.O1_CompanyName = "company name";
			AssertEquals("code - WEB - company name", enquiry.HumanReadableShortcutName);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "orgcode";
			enquiry.O1_OH_ConvertedToQualifiedLead = org.PK;
			AssertEquals("code - WEB - orgcode", enquiry.HumanReadableShortcutName);
		}

		#endregion

		public void TestIGlbCompanyCampaignItemRecipientMembers()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_Code = "Org";
			SalesEnquiry salesEnquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			salesEnquiry.O1_Phone = "123YES";
			salesEnquiry.O1_Fax = "293999";
			salesEnquiry.O1_ContactName = "Alan";

			IGlbCompanyCampaignItemRecipient recipient = salesEnquiry;
			AssertEquals("123YES", recipient.Phone);
			AssertEquals(null, recipient.Organisation);
			AssertEquals("", recipient.Salutation);
			AssertEquals("", recipient.Title);
			AssertEquals("293999", recipient.Fax);
			AssertEquals("Contact Alan", recipient.RelatedDocName);

			salesEnquiry.O1_OH_ConvertedToQualifiedLead = header.PK;
			AssertEquals(header, recipient.Organisation);
			AssertEquals("Contact Alan (Org)", recipient.RelatedDocName);
		}

		public void TestPhoneNumbers()
		{
			var salesEnquiry = Factory.New<SalesEnquiry>();
			salesEnquiry.O1_Phone = "Phone101";
			salesEnquiry.O1_Mobile = "Mobile101";
			salesEnquiry.O1_Fax = "Fax101";

			AssertEquals("Phone101", salesEnquiry.PhoneNumber.FormattedForBinding);
			AssertEquals("Mobile101", salesEnquiry.MobilePhoneNumber.FormattedForBinding);
			AssertEquals("Fax101", salesEnquiry.FaxNumber.FormattedForBinding);

			AssertEquals("", salesEnquiry.PhoneNumber.FormattedLocalNumberIfLoggedInSameCountryForBinding);
			AssertEquals("", salesEnquiry.MobilePhoneNumber.FormattedLocalNumberIfLoggedInSameCountryForBinding);
			AssertEquals("", salesEnquiry.FaxNumber.FormattedLocalNumberIfLoggedInSameCountryForBinding);
		}

		#region Address Validation

		public void TestValidationStatus_WhenChangingAddressFieldWhileValueIsCna_ShouldKeepItAsCna()
		{
			// Arrange.

			Env.Registry.EnableAddressValidationWebService = true;

			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry.O1_ValidationStatus = AddressValidationStatus.CountryNotAvailable;

			// Act & Assert.

			enquiry.O1_Address1 = "[_MOCK_ADDRESS_1_]";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, enquiry.O1_ValidationStatus);

			enquiry.O1_Address2 = "[_MOCK_ADDRESS_2_]";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, enquiry.O1_ValidationStatus);

			enquiry.O1_City = "[_MOCK_CITY_]";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, enquiry.O1_ValidationStatus);

			enquiry.O1_PostCode = "0000";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, enquiry.O1_ValidationStatus);

			enquiry.O1_State = "[_MOCK_STATE_]";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, enquiry.O1_ValidationStatus);

			enquiry.O1_PortOrCountry = "XY";
			AssertEquals(AddressValidationStatus.ToBeVerified, enquiry.O1_ValidationStatus);
		}

		public void TestChangingAddressResetsValidationStatus()
		{
			Env.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());
			var address = Factory.NewWithValidTestData<SalesEnquiry>();
			address.O1_PortOrCountry = "AU";
			address.ValidationStatus = AddressValidationStatus.Verified;

			AssertValidationStatusIsReset(address, () => address.Address1 += "A");
			AssertValidationStatusIsReset(address, () => address.Address2 += "A");
			AssertValidationStatusIsReset(address, () => address.City += "A");
			AssertValidationStatusIsReset(address, () => address.Postcode += "A");
			AssertValidationStatusIsReset(address, () => address.State += "A");
		}

		void AssertValidationStatusIsReset(ISupportWebAddressValidation address, Action action)
		{
			address.ValidationStatus = AddressValidationStatus.Verified;
			action.Invoke();
			AssertEquals(AddressValidationStatus.ToBeVerified, address.ValidationStatus);
		}

		public void TestRaiseAddressValidationStatusChanged()
		{
			var address = Factory.NewWithValidTestData<SalesEnquiry>();
			address.ValidationStatus = AddressValidationStatus.ToBeVerified;
			address.Address2 = "";

			address.AddressValidationStatusChanged += address_AddressValidationStatusChanged;
			address.ValidationStatus = AddressValidationStatus.Verified;
			AssertEquals("It happened", address.CompanyName);
		}

		void address_AddressValidationStatusChanged(object sender, EventArgs e)
		{
			((SalesEnquiry)sender).CompanyName = "It happened";
		}

		public void TestValidationStatus()
		{
			var country = Factory.LoadTop1<RefCountry>(new ZQuery());
			country.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;

			var address = Factory.NewWithValidTestData<SalesEnquiry>();
			address.O1_PortOrCountry = country.Code;
			address.ValidationStatus = AddressValidationStatus.ToBeVerified;
			AssertEquals(AddressValidationStatus.ToBeVerified, address.ValidationStatus);

			country.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.NotAvailable;
			AssertEquals(AddressValidationStatus.ToBeVerified, address.ValidationStatus);
		}

		public void TestState()
		{
			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU"));
			country.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;

			var address = Factory.NewWithValidTestData<SalesEnquiry>();
			address.O1_PortOrCountry = country.Code;
			address.StateCode = "NSW";
			AssertEquals("New South Wales", address.State);

			address.State = "Victoria";
			AssertEquals("VIC", address.StateCode);
		}

		public void TestNeedValidation()
		{
			var factory = new BusinessObjectFactory();

			var australia = factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			australia.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;
			var china = factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "CN");
			china.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.NotAvailable;

			var address = factory.NewWithValidTestData<SalesEnquiry>();
			address.Address1 = "A1";
			address.Address2 = "A2";
			address.Postcode = "1234";
			address.City = "Syd";
			address.State = "NSW";
			address.O1_PortOrCountry = "CN";
			Assert(address.NeedValidation);

			address.O1_PortOrCountry = "AU";
			Assert(address.NeedValidation);

			factory.Save();
			Assert(address.IsInDatabase);
			Assert(!address.NeedValidation);

			address.Address1 += "A";
			Assert(address.NeedValidation);

			address.Address2 = "";
			Assert(address.NeedValidation);

			address.Address1 = "";
			Assert(!address.NeedValidation);
		}

		public void TestResetAddressMap()
		{
			Env.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			var address = Factory.NewWithValidTestData<SalesEnquiry>();
			address.O1_PortOrCountry = "AU";
			AssertAddressMap(address, address.O1_Address1Info);
			AssertAddressMap(address, address.O1_Address2Info);
			AssertAddressMap(address, address.O1_CityInfo);
			AssertAddressMap(address, address.O1_PostCodeInfo);
			AssertAddressMap(address, address.O1_StateInfo);
			AssertAddressMap(address, address.O1_PortOrCountryInfo);

			address.O1_PortOrCountry = "AU";
			var usPK = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "US")).PK.ToGuid();
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DisabledCountryItemCollectionTestHelper.GetCollection(usPK, disabledForSalesInquiry: true));
			AssertAddressMap(address, address.O1_Address1Info);
			AssertAddressMap(address, address.O1_Address2Info);
			AssertAddressMap(address, address.O1_CityInfo);
			AssertAddressMap(address, address.O1_PostCodeInfo);
			AssertAddressMap(address, address.O1_StateInfo);
			AssertAddressMap(address, address.O1_PortOrCountryInfo);
		}

		void AssertAddressMap(SalesEnquiry address, ZPropertyInfo propertyInfo)
		{
			address.AddressMap = "ABCDE";
			propertyInfo.Value = (ZString)(propertyInfo.Name == nameof(SalesEnquiry.O1_PortOrCountry) ? "US" : (ZString)propertyInfo.Value + "1");
			Assert(string.IsNullOrEmpty(address.AddressMap));
		}

		public void TestValidationSection()
		{
			AssertEquals(AddressValidationSection.SalesInquiry, Factory.New<SalesEnquiry>().ValidationSection);
		}

		#endregion

		public void TestO1_GS_NKRepAssigned_ReadOnly()
		{
			Env.Security.InquiryManagerEditModifyStaffAssignment.IsAllowed = true;
			var saleEnquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			AssertEquals(false, saleEnquiry.O1_GS_NKRepAssignedInfo.ReadOnly);

			Env.Security.InquiryManagerEditModifyStaffAssignment.IsAllowed = false;
			AssertEquals(false, saleEnquiry.O1_GS_NKRepAssignedInfo.ReadOnly);

			Factory.Save();

			Env.Security.InquiryManagerEditModifyStaffAssignment.IsAllowed = true;
			AssertEquals(false, saleEnquiry.O1_GS_NKRepAssignedInfo.ReadOnly);

			Env.Security.InquiryManagerEditModifyStaffAssignment.IsAllowed = false;
			AssertEquals(true, saleEnquiry.O1_GS_NKRepAssignedInfo.ReadOnly);
		}

		public void TestOnSaving_WhenClosingEnquiry_ShouldGenerateEvent()
		{
			// Arrange.

			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry.O1_CloseReason = "FOO";
			enquiry.DoClose();

			// Act.

			Factory.Save();

			// Assert.

			var eventQuery = new ZQuery()
				.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.StatusChangeCode)
				.AddToFilter(StmALogSchema.SL_Reference, "Close Reason: FOO");

			var closedEvents = new BusinessObjectFactory()
				.Load<SalesEnquiry>(enquiry.PK)
				.Logs
				.Find(eventQuery);

			AssertEquals(1, closedEvents.Length);
		}

		public void TestOnSaving_WhenModifyingEnquiryWithoutClosing_ShouldNotGenerateEvent()
		{
			// Arrange.

			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry.O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Converted;

			// Act.

			Factory.Save();

			// Assert.

			var eventQuery = new ZQuery()
				.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.StatusChangeCode);

			var statusChangedEvents = new BusinessObjectFactory()
				.Load<SalesEnquiry>(enquiry.PK)
				.Logs
				.Find(eventQuery);

			AssertEquals(0, statusChangedEvents.Length);
		}

		#region Test HTML Properties

		public void TestHtmlProperty()
		{
			var salesEnquiry = (SalesEnquiry)GetNewBusinessObject();
			AssertEquals(@"{\rtf1\ansi\ansicpg1252\deflang3081\nouicompat\uc0{\fonttbl}{\colortbl}}", ORtfTextUtil.GeneratorInfoRegex.Replace(salesEnquiry.EnquiryNotesContent.ToUTF8(), string.Empty));
			AssertEquals(ZBlob.Empty, salesEnquiry.EnquiryNotesContent_HTML);

			salesEnquiry.EnquiryNotesContent_HTML = ZBlob.FromUTF8("<p>123</p>");

			AssertEquals(@"{\rtf1\ansi\ansicpg1252\deflang3081\nouicompat\uc0{\fonttbl}{\colortbl}{{123}\par}}", ORtfTextUtil.GeneratorInfoRegex.Replace(salesEnquiry.EnquiryNotesContent.ToUTF8(), string.Empty));
			AssertEquals("<p>123</p>", salesEnquiry.EnquiryNotesContent_HTML.ToUTF8());
		}

		public void TestHtmlFromTextProperty()
		{
			var salesEnquiry = (SalesEnquiry)GetNewBusinessObject();
			var tempObj = salesEnquiry.EnquiryNotesContent.ToUTF8();
			AssertEquals(@"{\rtf1\ansi\ansicpg1252\deflang3081\nouicompat\uc0{\fonttbl}{\colortbl}}", ORtfTextUtil.GeneratorInfoRegex.Replace(salesEnquiry.EnquiryNotesContent.ToUTF8(), string.Empty));
			AssertEquals(ZBlob.Empty, salesEnquiry.EnquiryNotesContent_HTML);

			salesEnquiry.EnquiryNotesContent = ZBlob.FromUTF8("1234\r\n5678");

			AssertEquals("<p><span style=\"font-family: &#39;Microsoft Sans Serif&#39;, sans-serif; font-size: 10pt;\">1234</span></p><p><span style=\"font-family: &#39;Microsoft Sans Serif&#39;, sans-serif; font-size: 10pt;\">5678</span></p>", salesEnquiry.EnquiryNotesContent_HTML.ToUTF8());

			salesEnquiry.EnquiryNotesContent = ZBlob.FromUTF8("{\\rtf1\\test\\ansi\\ansicpg1252\\nouicompat\\deflang3081\r\n{\\*\\generator Riched20 10.0.19041}\\viewkind4\\uc1 \\pard rtf\\par\r\n}\r\n");

			AssertEquals("<p>rtf</p>", salesEnquiry.EnquiryNotesContent_HTML.ToUTF8());
		}

		#endregion

		#region Implementation

		GlbStaff AddNewSalesStaff(OrgHeader organisation, ZGuid companyPk)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var staffAssignment = organisation.StaffAssignments.AddNew();
			staffAssignment.O8_GS_NKPersonResponsible = staff.GS_Code;
			staffAssignment.O8_Role = "SAL";
			staffAssignment.O8_Department = OrgStaffAssignmentsLookups.AllServices;
			staffAssignment.O8_GC = companyPk;

			return staff;
		}

		void AssertNoteCreated(string message, SalesEnquiry inquiry, StmNoteVisibility visibility, string description, string text)
		{
			CombineAssertions(message, () =>
			{
				var notes = inquiry.Notes.GetAllNotes().ToArray<StmNote>();
				AssertEquals("length", 1, notes.Length);
				var note = notes[0];
				AssertEquals("description", description, note.ST_Description);
				AssertEquals("visibility", visibility.ToString(), note.ST_NoteType);
				AssertMultilineASCIIEquals("text", text, note.ST_NoteDataAsText);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			Env.Security.OrgContactNew.IsAllowed = true;
			Env.Security.OrgAddressModify.IsAllowed = true;
			Env.Security.OrgContactModifyContactDetails.IsAllowed = true;
			Env.Security.OrgAddressListModify.IsAllowed = true;
			Env.Security.OrgAddressCapabilitiesNonARAP.IsAllowed = true;
			Env.Security.OrgAddressDetailsModify.IsAllowed = true;
			Env.Security.OrgAddressDetailsNonARAP.IsAllowed = true;
			Env.Security.OrganisationCRMSecurity.EditByStaffNotAssigned.IsAllowed = true;
		}
		#endregion
	}
}
