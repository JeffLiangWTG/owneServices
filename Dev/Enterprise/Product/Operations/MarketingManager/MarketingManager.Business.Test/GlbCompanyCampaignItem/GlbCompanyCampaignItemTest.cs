using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(GlbCompanyCampaignItem))]
	public class GlbCompanyCampaignItemTest : EnterpriseBusinessObjectTestCase
	{
		#region Set Fields Readonly

		public void TestSetFieldsReadonly()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			GlbCompanyCampaignItem item = campaign.CampaignsItemsSent.AddNew();
			item.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item.G8_RecipientID = contact.PK;

			Assert("OC should be readonly on new object", item.G8_RecipientIDInfo.ReadOnly);
			Assert("Create time should be readonly on new object", item.G8_SystemCreateTimeUtcInfo.ReadOnly);
			Assert("Create user should be readonly on new object", item.G8_SystemCreateUserInfo.ReadOnly);
			Assert("Delivery Method should be readonly on new object", item.G8_DeliveryMethodInfo.ReadOnly);
			Assert("Stage should be readonly on new object", item.G8_StageInfo.ReadOnly);

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			GlbCompanyCampaignItem itemFromNewFactory = newFactory.Load<GlbCompanyCampaignItem>(item.PK);

			Assert("OC should be readonly on loaded object", itemFromNewFactory.G8_RecipientIDInfo.ReadOnly);
			Assert("Create time should be readonly on loaded object", itemFromNewFactory.G8_SystemCreateTimeUtcInfo.ReadOnly);
			Assert("Create user should be readonly on loaded object", itemFromNewFactory.G8_SystemCreateUserInfo.ReadOnly);
			Assert("Delivery Method should be readonly on loaded object", item.G8_DeliveryMethodInfo.ReadOnly);
			Assert("Stage should be readonly on loaded object", item.G8_StageInfo.ReadOnly);
		}

		#endregion

		#region Populate Task

		public void TestGetPopulatedTask()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			GlbCompanyCampaignItem item = campaign.CampaignsItemsSent.AddNew();
			item.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item.G8_RecipientID = contact.PK;

			Assert("Pre Condition: Org has at least one valid address", org.Addresses.Count > 0);

			var task = item.GetPopulatedTask();

			AssertEquals(task.P9_ParentTableCode, GlbCompanyCampaignItemSchema.Constants.Prefix);
			AssertEquals(contact.PK, task.P9_OC);
			AssertEquals(org.PK, task.OrganisationPK);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, task.P9_GS_NKAssignedStaffMember);
			AssertEquals(("Campaign Task - ___" + campaign.G0_CampaignName).Substring(0, 50), task.P9_Description);

			Factory.Save();

			ProcessTask loadedTask = new BusinessObjectFactory().Load<ProcessTask>(task.PK);
			AssertEquals("Should be the same organisation", org.PK, loadedTask.Organisation.PK);
			AssertEquals("Should be the same contact", contact.PK, loadedTask.Contact.PK);
		}

		public void TestGetPopulatedTask_NonOrgContact()
		{
			IGlbCompanyCampaignItemRecipient recipient = (IGlbCompanyCampaignItemRecipient)Factory.New<IHRJobApplicant>();
			GlbCompanyCampaignItem campaignItem = Factory.New<GlbCompanyCampaignItem>();
			campaignItem.G8_RecipientID = recipient.PK;
			campaignItem.G8_RecipientTableCode = HRJobApplicantSchema.Constants.Prefix;
			var task = campaignItem.GetPopulatedTask();

			AssertEquals(task.P9_ParentTableCode, GlbCompanyCampaignItemSchema.Constants.Prefix);
			AssertEquals(ZGuid.Empty, task.P9_OC);
			AssertEquals(ZGuid.Empty, task.OrganisationPK);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, task.P9_GS_NKAssignedStaffMember);
			AssertEquals("Campaign Task -", task.P9_Description);

			var staffRecipient = Factory.NewWithValidTestData<GlbStaff>();
			campaignItem = Factory.New<GlbCompanyCampaignItem>();
			campaignItem.G8_RecipientID = staffRecipient.PK;
			campaignItem.G8_RecipientTableCode = GlbStaffSchema.Constants.Prefix;
			var staffTask = campaignItem.GetPopulatedTask();

			AssertEquals(staffTask.P9_ParentTableCode, GlbCompanyCampaignItemSchema.Constants.Prefix);
			AssertEquals(ZGuid.Empty, staffTask.P9_OC);
			AssertEquals(ZGuid.Empty, staffTask.OrganisationPK);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, staffTask.P9_GS_NKAssignedStaffMember);
			AssertEquals("Campaign Task -", staffTask.P9_Description);
		}

		public void TestPopulateTask_DescriptionMaxLength()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_Category = "PREAP";
			campaign.G0_Type = "PRINT";
			campaign.G0_CampaignName = "Trimming Campaign Manager";
			campaign.G0_EstimatedStartedDate = new ZDateTime(2014, 11, 14);
			GlbCompanyCampaignItem item = campaign.CampaignsItemsSent.AddNew();
			item.G8_RecipientID = contact.PK;

			ProcessTask task = item.GetPopulatedTask();

			AssertEquals("Campaign Task - 141114_PREAP_PRINT_Trimming Campai", task.P9_Description);
		}

		#endregion

		#region Follow up

		public void TestFollowUp()
		{
			GlbCompanyCampaignItem campaignItem = Factory.New<GlbCompanyCampaignItem>();

			AssertEquals("Followed up by should be empty", ZString.Empty, campaignItem.G8_GS_NKFollowedUpBy);
			Assert("Followed up time should be empty", campaignItem.G8_FollowedUp.IsEmpty);

			campaignItem.FollowUp();

			AssertEquals("Followed up by", GlbStaff.CurrentUser.GS_Code, campaignItem.G8_GS_NKFollowedUpBy);
			Assert("Followed up time", !campaignItem.G8_FollowedUp.IsEmpty);
		}

		#endregion

		#region Properties

		public void TestCode()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "OrgCode";
			var contact = Factory.New<OrgContact>();
			contact.OC_OH = org.PK;
			contact.OC_ContactName = "OrgContact";

			var inquiry = Factory.New<SalesEnquiry>();
			inquiry.O1_OH_ConvertedToQualifiedLead = org.PK;
			inquiry.O1_ContactName = "SalesEnquiry";

			var staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "Staff";

			var campaign = Factory.New<GlbCompanyCampaign>();

			var item1 = Factory.New<GlbCompanyCampaignItem>();

			item1.G8_RecipientTableCode = contact.TablePrefix;
			item1.G8_RecipientID = contact.PK;
			item1.G8_G0 = campaign.PK;
			AssertEquals("Code", "Contact OrgContact (OrgCode)", item1.Code);

			var item2 = Factory.New<GlbCompanyCampaignItem>();

			item2.G8_RecipientTableCode = inquiry.TablePrefix;
			item2.G8_RecipientID = inquiry.PK;
			item2.G8_G0 = campaign.PK;
			AssertEquals("Code", "Contact SalesEnquiry (OrgCode)", item2.Code);

			var item3 = Factory.New<GlbCompanyCampaignItem>();

			item3.G8_RecipientTableCode = staff.TablePrefix;
			item3.G8_RecipientID = staff.PK;
			item3.G8_G0 = campaign.PK;
			AssertEquals("Code", "Staff Staff", item3.Code);
		}

		public void TestCodeWithoutOrganisation()
		{
			var contact = Factory.New<OrgContact>();
			contact.OC_ContactName = "OrgContact";

			var inquiry = Factory.New<SalesEnquiry>();
			inquiry.O1_ContactName = "SalesEnquiry";

			var campaign = Factory.New<GlbCompanyCampaign>();

			var item1 = Factory.New<GlbCompanyCampaignItem>();

			item1.G8_RecipientTableCode = contact.TablePrefix;
			item1.G8_RecipientID = contact.PK;
			item1.G8_G0 = campaign.PK;
			AssertEquals("Code", "Contact OrgContact", item1.Code);

			var item2 = Factory.New<GlbCompanyCampaignItem>();

			item2.G8_RecipientTableCode = inquiry.TablePrefix;
			item2.G8_RecipientID = inquiry.PK;
			item2.G8_G0 = campaign.PK;
			AssertEquals("Code", "Contact SalesEnquiry", item2.Code);
		}

		public void TestCompanyCampaign()
		{
			GlbCompanyCampaignItem item = Factory.New<GlbCompanyCampaignItem>();
			AssertNull("Company campaign should be null", item.CompanyCampaign);

			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			item.G8_G0 = campaign.PK;
			AssertEquals("Company campaign should be set to Campaign", campaign, item.CompanyCampaign);
		}

		public void TestSystemCreateUser()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			GlbCompanyCampaignItem campaignItem = Factory.New<GlbCompanyCampaignItem>();
			campaignItem.G8_SystemCreateUser = staff.GS_Code;
			AssertEquals("Staff should be system create user", staff, campaignItem.SystemCreateUser);
		}

		public void TestWorkPhone()
		{
			OrgContact contact = Factory.New<OrgContact>();
			contact.OC_Phone = "12345";
			GlbCompanyCampaignItem item = Factory.New<GlbCompanyCampaignItem>();
			AssertEquals("Phone should not be set", ZString.Empty, item.WorkPhone);
			item.G8_RecipientID = contact.PK;
			AssertEquals("Phone should match", contact.OC_Phone, item.WorkPhone);
		}

		public void TestEmailAddress()
		{
			OrgContact contact = Factory.New<OrgContact>();
			contact.OC_Email = "email!";
			GlbCompanyCampaignItem item = Factory.New<GlbCompanyCampaignItem>();
			AssertEquals("Email should not be set", ZString.Empty, item.EmailAddress);
			item.G8_RecipientID = contact.PK;
			AssertEquals("Email should match", contact.OC_Email, item.EmailAddress);
		}

		public void TestCampaignName()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Bob's campaign";
			GlbCompanyCampaignItem item = Factory.New<GlbCompanyCampaignItem>();
			AssertEquals("Campaign name should not be set", ZString.Empty, item.CampaignName);
			item.G8_G0 = campaign.PK;
			AssertEquals("Campaign name should match", campaign.G0_CampaignName, item.CampaignName);
		}

		public void TestCampaignID()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "mary jane smith";
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Bob's campaign";
			campaign.G0_EstimatedStartedDate = new ZDateTime(2005, 10, 14, 09, 32, 00);
			GlbCompanyCampaignItem item = campaign.CampaignsItemsSent.AddNew();
			item.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item.G8_RecipientID = contact.PK;
			Factory.Save();

			AssertEquals("Campaign ID should be set", "051014___Bob's campaign", item.CampaignID);
		}

		public void TestContactName()
		{
			OrgContact contact = Factory.New<OrgContact>();
			contact.OC_ContactName = "Bert";
			contact.OC_Salutation = "Ber";
			GlbCompanyCampaignItem item = Factory.New<GlbCompanyCampaignItem>();
			AssertEquals("Contact name should not be set", ZString.Empty, item.ContactName);
			item.G8_RecipientID = contact.PK;
			AssertEquals("Contact name should match", contact.OC_ContactName, item.ContactName);
		}

		public void TestTrackingStatusDescription()
		{
			GlbCompanyCampaignItem glbCompanyCampaignItem = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			glbCompanyCampaignItem.G8_TrackingStatus = "";
			Assertion.AssertEquals("", glbCompanyCampaignItem.TrackingStatusDescription);
			glbCompanyCampaignItem.G8_TrackingStatus = "VER";
			Assertion.AssertEquals("Verified", glbCompanyCampaignItem.TrackingStatusDescription);
		}

		public void TestBounceBackEmail()
		{
			GlbCompanyCampaignItem glbCompanyCampaignItem = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			glbCompanyCampaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			glbCompanyCampaignItem.G8_RecipientID = ZGuid.NewZGuid();
			glbCompanyCampaignItem.G8_TrackingStatus = "";
			Assertion.AssertEquals("", glbCompanyCampaignItem.BounceBackEmail);
			glbCompanyCampaignItem.G8_TrackingStatus = "";
			StmNoteCollection stmNoteCollection = (StmNoteCollection)glbCompanyCampaignItem.Notes.GetAllNotes();
			Assertion.AssertEquals(0, stmNoteCollection.Count);
			Assertion.AssertEquals("", glbCompanyCampaignItem.BounceBackEmail);
			string text = "MailEnable: Message could not be delivered to some recipients.\r\nThe following recipient(s) could not be reached:\r\n\r\n\tRecipient: [SMTP:neddy110@co.cz.au.com]\r\n\tReason: The message could not be delivered because the domain name (co.cz.au.com) does not appear to be registered.\r\n\r\nReason Code: Invalid Domain Name\r\nError Number: 9003";
			StmNote stmNote = stmNoteCollection.AddNew();
			stmNote.ST_NoteDataAsText = text;
			Factory.Save();
			stmNoteCollection = (StmNoteCollection)glbCompanyCampaignItem.Notes.GetAllNotes();
			Assertion.AssertEquals(1, stmNoteCollection.Count);
			Assertion.AssertEquals("MailEnable: Message could not be delivered to some recipients.\r\nThe following recipient(s) could not be reached:\r\n\r\n\tRecipient: [SMTP:neddy110@co.cz.au.com]\r\n\tReason: The message could not be delivered because the domain name (co.cz.au.com) does not appear to be registered.\r\n\r\nReason Code: Invalid Domain Name\r\nError Number: 9003", stmNoteCollection[0].ST_NoteDataAsText);

			StmNote nonConformanceNote = stmNoteCollection.AddNew();
			nonConformanceNote.ST_Description = "Handheld";

			glbCompanyCampaignItem.G8_TrackingStatus = "NDR";
			StmNote stmNote2 = stmNoteCollection.AddNew();
			stmNote2.ST_NoteDataAsText = "Message identified as bounce back note";
			stmNote2.ST_ParentID = glbCompanyCampaignItem.PK;
			stmNote2.ST_Description = "Email bounce back";
			Factory.Save();
			stmNoteCollection = (StmNoteCollection)glbCompanyCampaignItem.Notes.GetAllNotes();
			AssertEquals(3, stmNoteCollection.Count);
			glbCompanyCampaignItem.ReloadNotesFromDB();
			AssertEquals("Message identified as bounce back note", glbCompanyCampaignItem.BounceBackEmail);

			glbCompanyCampaignItem.G8_TrackingStatus = "VER";
			StmNote stmNote3 = stmNoteCollection.AddNew();
			stmNote3.ST_NoteDataAsText = "Message identified as bounce back note_3";
			stmNote3.ST_ParentID = glbCompanyCampaignItem.PK;
			stmNote3.ST_Description = "Email bounce back";
			Factory.Save();
			stmNoteCollection = (StmNoteCollection)glbCompanyCampaignItem.Notes.GetAllNotes();
			AssertEquals(4, stmNoteCollection.Count);
			glbCompanyCampaignItem.ReloadNotesFromDB();
			AssertEquals("", glbCompanyCampaignItem.BounceBackEmail);
		}

		public void TestOrgPK()
		{
			var org = Factory.New<OrgHeader>();
			var contact = org.Contacts.AddNew();

			var item = Factory.New<GlbCompanyCampaignItem>();
			item.G8_RecipientID = contact.PK;

			AssertEquals("PK should match Org PK", org.PK, item.OrgPK);

			item = Factory.New<GlbCompanyCampaignItem>();
			AssertEquals("Org PK should be empty", ZGuid.Empty, item.OrgPK);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var applicant = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IHRJobApplicant)));

			item.G8_RecipientID = staff.PK;
			AssertEquals("Org PK should be empty", ZGuid.Empty, item.OrgPK);

			item.G8_RecipientID = applicant.PK;
			AssertEquals("Org PK should be empty", ZGuid.Empty, item.OrgPK);
		}

		public void TestClientOrg()
		{
			var org = Factory.New<OrgHeader>();
			var contact = org.Contacts.AddNew();

			var item = Factory.New<GlbCompanyCampaignItem>();
			item.G8_RecipientID = contact.PK;

			AssertEquals("ClientOrg should match Org", org, item.ClientOrg);

			item = Factory.New<GlbCompanyCampaignItem>();
			AssertNull("ClientOrg should be null", item.ClientOrg);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var applicant = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IHRJobApplicant)));

			item.G8_RecipientID = staff.PK;
			AssertNull("ClientOrg should be null", item.ClientOrg);

			item.G8_RecipientID = applicant.PK;
			AssertNull("ClientOrg should be null", item.ClientOrg);
		}

		public void TestFollowedUpStaffName()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Ernie";
			GlbCompanyCampaignItem campaignItem = Factory.New<GlbCompanyCampaignItem>();
			campaignItem.G8_SystemCreateUser = staff.GS_Code;
			AssertEquals("System create user name should be staff", staff.GS_FullName, campaignItem.SenderStaffName);
		}

		public void TestSenderStaffName()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Ernie";
			GlbCompanyCampaignItem campaignItem = Factory.New<GlbCompanyCampaignItem>();
			campaignItem.G8_GS_NKFollowedUpBy = staff.GS_Code;
			AssertEquals("followed up by user name should be staff", staff.GS_FullName, campaignItem.FollowedUpStaffName);
		}

		public void TestRecipient()
		{
			var campaignItem = Factory.New<GlbCompanyCampaignItem>();
			var jobApplicant = (IGlbCompanyCampaignItemRecipient)Factory.New<IHRJobApplicant>();
			campaignItem.G8_RecipientID = jobApplicant.PK;
			campaignItem.G8_RecipientTableCode = HRJobApplicantSchema.Constants.Prefix;
			AssertEquals(jobApplicant, campaignItem.Recipient);
			AssertEquals(jobApplicant, campaignItem.RecipientAsHRJobApplicant);
			AssertNull(campaignItem.RecipientAsOrgContact);

			var contact = Factory.New<OrgContact>();
			campaignItem.G8_RecipientID = contact.PK;
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			AssertEquals(contact, campaignItem.Recipient);
			AssertEquals(contact, campaignItem.RecipientAsOrgContact);

			var staff = Factory.New<GlbStaff>();
			campaignItem.G8_RecipientID = staff.PK;
			campaignItem.G8_RecipientTableCode = GlbStaffSchema.Constants.Prefix;
			AssertEquals(staff, campaignItem.Recipient);
			AssertEquals(staff, campaignItem.RecipientAsGlbStaff);
			AssertNull(campaignItem.RecipientAsOrgContact);

			campaignItem.G8_RecipientID = ZGuid.Empty;
			campaignItem.G8_RecipientTableCode = "";
			AssertNull(campaignItem.Recipient);

			campaignItem.G8_RecipientID = contact.PK;
			AssertEquals(contact, campaignItem.Recipient);
		}

		public void TestRecipientAsSalesEnquiry()
		{
			var campaignItem = Factory.New<GlbCompanyCampaignItem>();
			var jobApplicant = (IGlbCompanyCampaignItemRecipient)Factory.New<IHRJobApplicant>();
			campaignItem.G8_RecipientID = jobApplicant.PK;
			campaignItem.G8_RecipientTableCode = HRJobApplicantSchema.Constants.Prefix;
			AssertEquals(jobApplicant, campaignItem.Recipient);
			AssertNull(campaignItem.RecipientAsSalesEnquiry);

			var contact = Factory.New<OrgContact>();
			campaignItem.G8_RecipientID = contact.PK;
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			AssertEquals(contact, campaignItem.Recipient);
			AssertNull(campaignItem.RecipientAsSalesEnquiry);

			var enquiry = Factory.New<SalesEnquiry>();
			campaignItem.G8_RecipientID = enquiry.PK;
			campaignItem.G8_RecipientTableCode = OrgColdCallRegisterSchema.Constants.Prefix;
			AssertEquals(enquiry, campaignItem.Recipient);
			AssertEquals(enquiry, campaignItem.RecipientAsSalesEnquiry);

			var staff = Factory.New<GlbStaff>();
			campaignItem.G8_RecipientID = staff.PK;
			campaignItem.G8_RecipientTableCode = GlbStaffSchema.Constants.Prefix;
			AssertEquals(staff, campaignItem.Recipient);
			AssertNull(campaignItem.RecipientAsSalesEnquiry);
		}

		public void TestLastCommunicationDate()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();

			var refDate = new ZDateTime(2014, 4, 4, 4, 4, 4);
			var refDate1 = new ZDateTime(2014, 5, 5, 5, 5, 5);

			var orgSalesCall = Factory.NewWithValidTestData<OrgSalesCall>();
			orgSalesCall.OQ_OC = contact.PK;
			orgSalesCall.OQ_OH = org.PK;
			orgSalesCall.OQ_CallDate = refDate;

			var orgSalesCall1 = Factory.NewWithValidTestData<OrgSalesCall>();
			orgSalesCall1.OQ_OC = contact.PK;
			orgSalesCall1.OQ_OH = org.PK;
			orgSalesCall1.OQ_CallDate = refDate1;

			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;

			var relatedActivity = campaignItem.RelatedChildActivityPivotCollection.AddNew();
			relatedActivity.ChildActivity = orgSalesCall;
			var relatedActivity1 = campaignItem.RelatedChildActivityPivotCollection.AddNew();
			relatedActivity1.ChildActivity = orgSalesCall1;
			Factory.Save();

			AssertEquals("Last Communication date should be present", refDate1.Date, campaignItem.LastCommunicationDate.Date);

			orgSalesCall.OQ_CallDate = new ZDateTime();
			var relatedActivity2 = campaignItem.RelatedChildActivityPivotCollection.AddNew();
			relatedActivity2.ChildActivity = Factory.NewWithValidTestData<OrgOpportunity>();
			Factory.Save();
			AssertEquals("Last Communication date should still be the same", refDate1.Date, campaignItem.LastCommunicationDate.Date);
		}

		public void TestLastCommunicationStaffName()
		{
			var refDate = new ZDateTime(2014, 4, 4, 4, 4, 4);
			var refDate1 = new ZDateTime(2014, 5, 5, 5, 5, 5);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "EJ";
			staff.GS_FullName = "Edward Jones";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "JS";
			staff2.GS_FullName = "John Smith";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();

			var orgSalesCall = Factory.NewWithValidTestData<OrgSalesCall>();
			orgSalesCall.OQ_OC = contact.PK;
			orgSalesCall.OQ_OH = org.PK;
			orgSalesCall.OQ_CallDate = new ZDateTime(2014, 4, 4, 4, 4, 4);
			orgSalesCall.OQ_GS_NKSalesRep = staff.GS_Code;

			var orgSalesCall1 = Factory.NewWithValidTestData<OrgSalesCall>();
			orgSalesCall1.OQ_OC = contact.PK;
			orgSalesCall1.OQ_OH = org.PK;
			orgSalesCall1.OQ_CallDate = refDate1;
			orgSalesCall1.OQ_GS_NKSalesRep = staff2.GS_Code;

			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;

			var relatedActivity = campaignItem.RelatedChildActivityPivotCollection.AddNew();
			relatedActivity.ChildActivity = orgSalesCall;
			Factory.Save();
			AssertEquals("Last Communication should be present", "EJ", campaignItem.LastCommunicationStaffCode);

			var relatedActivity2 = campaignItem.RelatedChildActivityPivotCollection.AddNew();
			relatedActivity2.ChildActivity = orgSalesCall1;
			Factory.Save();
			AssertEquals("Last Communication should have updated", "JS", campaignItem.LastCommunicationStaffCode);
		}

		public void TestTrackingStatus()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "ed";
			contact.OC_Email = "ed@ed.com";
			GlbEmailAddress email = Factory.NewWithValidTestData<GlbEmailAddress>();
			email.GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.NonDeliveryReport;
			email.GI_DeliveryReportTimeUtc = ZDateTime.UtcNow;
			email.GI_EmailAddress = contact.OC_Email;
			GlbCompanyCampaignItem glbCompanyCampaignItem = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			glbCompanyCampaignItem.G8_RecipientID = contact.PK;
			glbCompanyCampaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			glbCompanyCampaignItem.G8_TrackingStatus = TrackingStatusCodes.Codes.VER;

			Factory.Save();

			AssertEquals("GlbEmail should remove NDR Status", "VLD", email.GI_DeliveryStatus);

			glbCompanyCampaignItem.G8_TrackingStatus = TrackingStatusCodes.Codes.NDR;
			AssertEquals("GlbEmail status should now become NDR", "NDR", email.GI_DeliveryStatus);
		}

		public void TestRecipientShouldReturnNullBusinessObjectForBindingIfCannotBeLoaded()
		{
			GlbCompanyCampaignItem campaignItem = Factory.New<GlbCompanyCampaignItem>();
			Assert(((BusinessObject)campaignItem.Recipient).IsNull);

			campaignItem.G8_RecipientID = ZGuid.NewZGuid();
			Assert(((BusinessObject)campaignItem.Recipient).IsNull);
		}

		public void TestScheduleTimeRecipientTime()
		{
			var contact = Factory.New<CampaignContact>();
			var item = Factory.New<GlbCompanyCampaignItem>();
			item.G8_ScheduleTimeUtc = new ZDateTime(2012, 2, 2, 12, 0, 0);
			item.G8_RecipientID = contact.PK;

			contact.VCC_RelatedPortCode = "AUSYD";
			AssertEquals(new ZDateTime(2012, 2, 2, 23, 0, 0), item.ScheduleTimeRecipientTime);

			contact.VCC_RelatedPortCode = "USNYC";
			AssertEquals(new ZDateTime(2012, 2, 2, 7, 0, 0), item.ScheduleTimeRecipientTime);

			contact.VCC_RelatedPortCode = "";
			AssertEquals(new ZDateTime(2012, 2, 2, 22, 0, 0), item.ScheduleTimeRecipientTime);
		}

		#endregion

		#region Calculated Properties

		public void TestSenderEmailAddress()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "NUK";
			staff.GS_EmailAddress = "xwinter@yahoo.com";
			staff.GS_FullName = "John Smizz";
			staff.GS_WorkPhone = "32899832";
			staff.GS_MobilePhone = "0449743938";
			staff.GS_Title = "Engr";

			GlbCompanyCampaignItem item = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			item.G8_G0 = campaign.PK;

			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
			AssertEquals("Email sender email address should be blank", "", item.SenderEmailAddress);
			AssertEquals("Sent By Email should be blank until sent", ZString.Empty, item.G8_SenderEmailAddress);
			campaign.G0_GS_NKCampaignCoordinator = "NUK";
			AssertEquals("Email sender email address should no longer be blank", "xwinter@yahoo.com", item.SenderEmailAddress);
			AssertEquals("Sent By Email should be blank until sent", ZString.Empty, item.G8_SenderEmailAddress);
			AssertEquals("Sender Staff code is empty until sent", ZString.Empty, item.G8_GS_NKSender);

			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
			AssertEquals("Email sender email address should be blank when sender option changes to EML", "", item.SenderEmailAddress);
			AssertEquals("Sent By Email should be blank until sent", ZString.Empty, item.G8_SenderEmailAddress);
			campaign.G0_SenderEmail = "cargo@cargo.com";
			AssertEquals("Email sender email address should no longer be blank", "cargo@cargo.com", item.SenderEmailAddress);
			AssertEquals("Sent By Email should be blank until sent", ZString.Empty, item.G8_SenderEmailAddress);
			AssertEquals("Sender Staff code is empty until sent", ZString.Empty, item.G8_GS_NKSender);

			RefCountry country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US");

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Name = "Eaglets Pty Ltd";
			company.GC_RN_NKCountryCode = country.Code;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = "AUBNE";

			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.OC_ContactName = "John Smiths";
			orgContact.OC_OH = org.PK;

			item.G8_RecipientID = orgContact.PK;

			OrgStaffAssignmentsCollection staffAssignmentCollection = new OrgStaffAssignmentsCollection(org);

			var assignment1 = org.StaffAssignments.AddNew();
			assignment1.O8_Role = "SAL";
			assignment1.O8_GS_NKPersonResponsible = "NUK";
			assignment1.O8_GC = company.PK;
			assignment1.O8_OH = org.PK;
			staffAssignmentCollection.Add(assignment1);

			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.ORG;
			AssertEquals("Sender email address should be empty", staff.GS_EmailAddress, item.SenderEmailAddress);
			AssertEquals("Sent By Email should be blank until sent", ZString.Empty, item.G8_SenderEmailAddress);
			AssertEquals("Sender Staff code is empty until sent", ZString.Empty, item.G8_GS_NKSender);
			campaign.G0_EmailSenderRole = "SAL";
			campaign.G0_GC = company.PK;
			AssertEquals("Staff email should be returned", staff.GS_EmailAddress, item.SenderEmailAddress);
			AssertEquals("Sent By Email should be blank until sent", ZString.Empty, item.G8_SenderEmailAddress);
			AssertEquals("Sender Staff code is empty until sent", ZString.Empty, item.G8_GS_NKSender);

			campaign.G0_EmailSenderRole = "ACC";
			AssertEquals("Staff email should be returned", "xwinter@yahoo.com", item.SenderEmailAddress);
			AssertEquals("Sent By Email should be blank until sent", ZString.Empty, item.G8_SenderEmailAddress);
			AssertEquals("Sender Staff code is empty until sent", ZString.Empty, item.G8_GS_NKSender);

			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.SPS;
			var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			glbStaff.GS_EmailAddress = "e@ma.il";
			AssertEquals("Sent By Email should be blank until sent", ZString.Empty, item.G8_SenderEmailAddress);
			AssertEquals("Sender Staff code is empty until sent", ZString.Empty, item.G8_GS_NKSender);

			// emulate sending
			item.G8_GS_NKSender = glbStaff.GS_Code;
			AssertEquals("Sender Staff code", glbStaff.GS_Code, item.G8_GS_NKSender);
			AssertEquals("Sender Staff email", glbStaff.GS_EmailAddress, item.SenderEmailAddress);
		}

		public void TestSenderName()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "NUK";
			staff.GS_EmailAddress = "xwinter@yahoo.com";
			staff.GS_FullName = "John Smizz";
			staff.GS_WorkPhone = "32899832";
			staff.GS_MobilePhone = "0449743938";
			staff.GS_Title = "Engr";

			GlbCompanyCampaignItem item = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			item.G8_G0 = campaign.PK;

			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
			AssertEquals("Email sender name should be blank", "", item.SenderName);
			AssertEquals("Sender Staff code is empty", "", item.G8_GS_NKSender);
			campaign.G0_GS_NKCampaignCoordinator = "NUK";
			AssertEquals("Email sender name should no longer be blank", "John Smizz", item.SenderName);
			AssertEquals("Sender Staff code is empty", "", item.G8_GS_NKSender);

			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
			AssertEquals("Email sender name should be the same as Coordinator", "John Smizz", item.SenderName);
			AssertEquals("Sender Staff code is empty", "", item.G8_GS_NKSender);
			campaign.G0_EmailSenderName = "John Edwards";
			AssertEquals("Email sender name should no longer be blank", "John Edwards", item.SenderName);
			AssertEquals("Sender Staff code is empty", "", item.G8_GS_NKSender);

			RefCountry country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US");

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Name = "Eaglets Pty Ltd";
			company.GC_RN_NKCountryCode = country.Code;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = "AUBNE";

			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.OC_ContactName = "John Smiths";
			orgContact.OC_OH = org.PK;

			item.G8_RecipientID = orgContact.PK;

			OrgStaffAssignmentsCollection staffAssignmentCollection = new OrgStaffAssignmentsCollection(org);

			var assignment1 = org.StaffAssignments.AddNew();
			assignment1.O8_Role = "SAL";
			assignment1.O8_GS_NKPersonResponsible = "NUK";
			assignment1.O8_GC = company.PK;
			assignment1.O8_OH = org.PK;
			staffAssignmentCollection.Add(assignment1);

			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.ORG;
			AssertEquals("Sender name should be empty", staff.GS_FullName, item.SenderName);
			AssertEquals("Sender Staff code is empty", "", item.G8_GS_NKSender);
			campaign.G0_EmailSenderRole = "SAL";
			campaign.G0_GC = company.PK;
			AssertEquals("Staff name should be returned", staff.GS_FullName, item.SenderName);
			AssertEquals("Sender Staff code is empty", "", item.G8_GS_NKSender);

			campaign.G0_EmailSenderRole = "ACC";
			AssertEquals("Staff name should be returned", "John Smizz", item.SenderName);
			AssertEquals("Sender Staff code is empty", "", item.G8_GS_NKSender);

			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.SPS;
			AssertEquals("Sender Staff code is empty", "", item.G8_GS_NKSender);
			AssertEquals("Email sender name should no longer be blank", staff.GS_FullName, item.SenderName);

			var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			glbStaff.GS_EmailAddress = "e@ma.il";
			glbStaff.GS_FullName = nameof(glbStaff);
			item.G8_GS_NKSender = glbStaff.GS_Code;
			AssertEquals("Sender Staff code", glbStaff.GS_Code, item.G8_GS_NKSender);
			AssertEquals("Sender Staff name", glbStaff.GS_FullName, item.SenderName);

			item.CompanyCampaign.G0_UseLastEmailSenderAddress = true;
			AssertEquals("Sender Staff name", glbStaff.GS_FullName, item.SenderName);

			item.G8_EmailSenderName = "ahaha";
			AssertEquals("Sender Staff name", item.G8_EmailSenderName, item.SenderName);
		}

		public void TestSenderTitle()
		{
			var setupValues = CalculatedPropertiesImplementation();
			var campaign = setupValues.Item1;
			var item = setupValues.Item2;
			var staff = setupValues.Item3;
			var company = setupValues.Item4;

			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
			AssertEquals("", item.SenderTitle);
			AssertEquals("Sender Staff code is empty", "", item.G8_GS_NKSender);

			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.ORG;
			campaign.G0_EmailSenderRole = "SAL";
			AssertEquals("", item.SenderTitle);
			AssertEquals("Sender Staff code is empty", "", item.G8_GS_NKSender);

			campaign.G0_GC = company.PK;
			AssertEquals("Engr", item.SenderTitle);
			AssertEquals("Sender Staff code is empty", "", item.G8_GS_NKSender);

			campaign.G0_GS_NKCampaignCoordinator = "NUK";
			AssertEquals("Engr", item.SenderTitle);
			AssertEquals("Sender Staff code is empty", "", item.G8_GS_NKSender);

			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.SPS;
			AssertEquals("Sender Staff code is empty", "", item.G8_GS_NKSender);
			AssertEquals("Coordinator Staff title", staff.GS_Title, item.SenderTitle);

			var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			glbStaff.GS_EmailAddress = "e@ma.il";
			glbStaff.GS_Title = "Dr.";
			item.G8_GS_NKSender = glbStaff.GS_Code;
			AssertEquals("Sender Staff code", glbStaff.GS_Code, item.G8_GS_NKSender);
			AssertEquals("Sender Staff title", glbStaff.GS_Title, item.SenderTitle);
		}

		Tuple<GlbCompanyCampaign, GlbCompanyCampaignItem, GlbStaff, GlbCompany> CalculatedPropertiesImplementation()
		{
			RefCountry country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US");

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Name = "Eaglets Pty Ltd";
			company.GC_RN_NKCountryCode = country.Code;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = "AUBNE";

			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.OC_ContactName = "John Smiths";
			orgContact.OC_OH = org.PK;

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "NUK";
			staff.GS_EmailAddress = "xwinter@yahoo.com";
			staff.GS_FullName = "John Smizz";
			staff.GS_WorkPhone = "32899832";
			staff.GS_MobilePhone = "0449743938";
			staff.GS_Title = "Engr";

			GlbCompanyCampaignItem item = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			item.G8_G0 = campaign.PK;
			item.G8_RecipientID = orgContact.PK;

			OrgStaffAssignmentsCollection staffAssignmentCollection = new OrgStaffAssignmentsCollection(org);

			var assignment1 = org.StaffAssignments.AddNew();
			assignment1.O8_Role = "SAL";
			assignment1.O8_GS_NKPersonResponsible = "NUK";
			assignment1.O8_GC = company.PK;
			assignment1.O8_OH = org.PK;
			staffAssignmentCollection.Add(assignment1);

			return new Tuple<GlbCompanyCampaign, GlbCompanyCampaignItem, GlbStaff, GlbCompany>(campaign, item, staff, company);
		}

		public void TestSenderWorkPhone()
		{
			var setupValues = CalculatedPropertiesImplementation();
			var campaign = setupValues.Item1;
			var item = setupValues.Item2;
			var staff = setupValues.Item3;
			var company = setupValues.Item4;

			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
			AssertEquals("", item.SenderWorkPhone);
			AssertEquals("Sender Staff code is empty", "", item.G8_GS_NKSender);

			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.ORG;
			campaign.G0_EmailSenderRole = "SAL";
			AssertEquals("", item.SenderWorkPhone);
			AssertEquals("Sender Staff code is empty", "", item.G8_GS_NKSender);

			campaign.G0_GC = company.PK;
			AssertEquals("32899832", item.SenderWorkPhone);
			AssertEquals("Sender Staff code is empty", "", item.G8_GS_NKSender);

			campaign.G0_GS_NKCampaignCoordinator = "NUK";
			AssertEquals("32899832", item.SenderWorkPhone);
			AssertEquals("Sender Staff code is empty", "", item.G8_GS_NKSender);

			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.SPS;
			AssertEquals("Sender Staff code is empty", "", item.G8_GS_NKSender);
			AssertEquals("Coordinator Staff work phone", staff.GS_WorkPhone, item.SenderWorkPhone);

			var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			glbStaff.GS_EmailAddress = "e@ma.il";
			glbStaff.GS_WorkPhone = "9876543210";
			item.G8_GS_NKSender = glbStaff.GS_Code;
			AssertEquals("Sender Staff code", glbStaff.GS_Code, item.G8_GS_NKSender);
			AssertEquals("Sender Staff work phone", glbStaff.GS_WorkPhone, item.SenderWorkPhone);
		}

		public void TestSenderMobilePhone()
		{
			var setupValues = CalculatedPropertiesImplementation();
			var campaign = setupValues.Item1;
			var item = setupValues.Item2;
			var staff = setupValues.Item3;
			var company = setupValues.Item4;

			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
			AssertEquals("", item.SenderMobilePhone);
			AssertEquals("Sender Staff code is empty", "", item.G8_GS_NKSender);

			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.ORG;
			campaign.G0_EmailSenderRole = "SAL";
			AssertEquals("", item.SenderMobilePhone);
			AssertEquals("Sender Staff code is empty", "", item.G8_GS_NKSender);

			campaign.G0_GC = company.PK;
			AssertEquals("0449743938", item.SenderMobilePhone);
			AssertEquals("Sender Staff code is empty", "", item.G8_GS_NKSender);

			campaign.G0_GS_NKCampaignCoordinator = "NUK";
			AssertEquals("0449743938", item.SenderMobilePhone);
			AssertEquals("Sender Staff code is empty", "", item.G8_GS_NKSender);

			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.SPS;
			AssertEquals("Sender Staff code is empty", "", item.G8_GS_NKSender);
			AssertEquals("Coordinator Staff mobile phone", staff.GS_MobilePhone, item.SenderMobilePhone);

			var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			glbStaff.GS_EmailAddress = "e@ma.il";
			glbStaff.GS_MobilePhone = "9876543210";
			item.G8_GS_NKSender = glbStaff.GS_Code;
			AssertEquals("Sender Staff code", glbStaff.GS_Code, item.G8_GS_NKSender);
			AssertEquals("Sender Staff mobile phone", glbStaff.GS_MobilePhone, item.SenderMobilePhone);
		}

		public void TestReplyToEmailAddress()
		{
			var setupValues = CalculatedPropertiesImplementation();
			var campaign = setupValues.Item1;
			var item = setupValues.Item2;
			var staff = setupValues.Item3;
			var company = setupValues.Item4;

			campaign.G0_GS_NKCampaignCoordinator = "NUK";
			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
			AssertEquals("xwinter@yahoo.com", item.ReplyToEmailAddress);
			AssertEquals("Sender Staff code is empty", "", item.G8_GS_NKSender);

			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
			campaign.G0_SenderEmail = "default@gmail.com";
			AssertEquals("default@gmail.com", item.ReplyToEmailAddress);
			AssertEquals("Sender Staff code is empty", "", item.G8_GS_NKSender);

			campaign.G0_GC = company.PK;
			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.ORG;
			campaign.G0_EmailSenderRole = "SAL";
			AssertEquals("xwinter@yahoo.com", item.ReplyToEmailAddress);
			AssertEquals("Sender Staff code is empty", "", item.G8_GS_NKSender);

			campaign.G0_GS_NKCampaignCoordinator = "";
			campaign.UseEmailSenderAddressAsReplyTo = false;
			campaign.G0_ReplyToEmail = "replyto@cargowise.com";
			AssertEquals("replyto@cargowise.com", item.ReplyToEmailAddress);
			AssertEquals("Sender Staff code is empty", "", item.G8_GS_NKSender);

			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.SPS;
			campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			campaign.UseEmailSenderAddressAsReplyTo = true;

			AssertEquals("Sender Staff code is empty", "", item.G8_GS_NKSender);
			AssertEquals("Coordinator Staff email address", staff.GS_EmailAddress, item.ReplyToEmailAddress);

			var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			glbStaff.GS_EmailAddress = "e@ma.il";
			item.G8_GS_NKSender = glbStaff.GS_Code;
			AssertEquals("Sender Staff code", glbStaff.GS_Code, item.G8_GS_NKSender);
			AssertEquals("Sender Staff email", glbStaff.GS_EmailAddress, item.ReplyToEmailAddress);

			var secondaryEmail = glbStaff.EmailAddresses.AddNew();
			secondaryEmail.GSE_EmailAddress = "secondary@gmail.com";
			secondaryEmail.GSE_Type = "FIR";

			campaign.G0_GS_NKCampaignCoordinator = glbStaff.GS_Code;
			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
			campaign.CoordinatorEmailAddress = "secondary@gmail.com";
			AssertEquals("Secondary Staff email should be set to ReplyToEmailAddress", secondaryEmail.GSE_EmailAddress, item.ReplyToEmailAddress);
		}

		public void TestRecipientOrgFullName()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ASX";
			org.OH_FullName = "Australian Society Exchange";
			var contact = org.Contacts.AddNew();

			var inquiry = Factory.New<SalesEnquiry>();
			inquiry.O1_CompanyName = "Globes Ltd";

			var campaignItem = Factory.New<GlbCompanyCampaignItem>();
			campaignItem.G8_RecipientTableCode = "OC";
			campaignItem.G8_RecipientID = contact.PK;

			var campaignItem1 = Factory.New<GlbCompanyCampaignItem>();
			campaignItem1.G8_RecipientTableCode = "O1";
			campaignItem1.G8_RecipientID = inquiry.PK;

			AssertEquals("Australian Society Exchange", campaignItem.RecipientOrgFullName);
			AssertEquals("Globes Ltd", campaignItem1.RecipientOrgFullName);
		}

		public void TestRecipientCountryOrPort()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ASX";
			org.OH_RL_NKClosestPort = "AUBNE";
			var orgAddress = org.Addresses.AddNew();
			orgAddress.OA_Address1 = "Milky way";
			orgAddress.OA_RL_NKRelatedPortCode = "HKABD";
			var contact = org.Contacts.AddNew();

			contact.OC_OA_OrgAddress = orgAddress.PK;

			var inquiry = Factory.New<SalesEnquiry>();
			inquiry.O1_PortOrCountry = "US";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = "OC";
			campaignItem.G8_RecipientID = contact.PK;

			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = "O1";
			campaignItem1.G8_RecipientID = inquiry.PK;

			AssertEquals("Address of contact is used if present", "HK", campaignItem.RecipientCountryOrPort);

			contact.OC_OA_OrgAddress = ZGuid.Empty;
			AssertEquals("AU", campaignItem.RecipientCountryOrPort);
			AssertEquals("US", campaignItem1.RecipientCountryOrPort);

			inquiry.O1_PortOrCountry = "HKHKG";
			AssertEquals("HK", campaignItem1.RecipientCountryOrPort);

			RefCountry country = Factory.NewWithValidTestData<RefCountry>();
			country.Code = "YY";

			RefUNLOCO unloco = Factory.NewWithValidTestData<RefUNLOCO>();
			unloco.RL_Code = "YYYYY";

			Factory.Save();

			org.OH_RL_NKClosestPort = "YYYYY";
			AssertEquals("YY", campaignItem.RecipientCountryOrPort);

			country.Delete();
			Factory.Save();
			AssertEquals("", campaignItem.RecipientCountryOrPort);
		}

		public void TestRecipientCountryOrPort_NoBranchCountry()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ASX";
			var orgAddress = org.Addresses.AddNew();
			orgAddress.OA_Address1 = "Milky way";
			orgAddress.OA_RN_NKCountryCode = "~~";
			var contact = org.Contacts.AddNew();

			contact.OC_OA_OrgAddress = orgAddress.PK;

			var inquiry = Factory.New<SalesEnquiry>();
			inquiry.O1_PortOrCountry = "US";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = "OC";
			campaignItem.G8_RecipientID = contact.PK;

			AssertNoExceptionThrown(delegate
			{
				var x = campaignItem.RecipientCountryOrPort;
			});
		}

		public void TestIsUnsubscribed()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			var contact2 = org.Contacts.AddNew();
			var contact3 = org.Contacts.AddNew();

			contact1.OC_ContactName = nameof(contact1);
			contact2.OC_ContactName = nameof(contact2);
			contact3.OC_ContactName = nameof(contact3);

			contact1.OC_Email = $"{nameof(contact1)}@abc.net";
			contact2.OC_Email = $"{nameof(contact2)}@abc.net";
			contact3.OC_Email = $"{nameof(contact3)}@abc.net";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			var campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			var campaignItem3 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			campaignItem2.G8_RecipientID = contact2.PK;
			campaignItem3.G8_RecipientID = contact3.PK;

			var unsubscribeFromThisCampaign = Factory.New<GlbCompanyCampaignSubscription>();
			unsubscribeFromThisCampaign.GCS_Email = contact1.OC_Email;
			unsubscribeFromThisCampaign.GCS_MediaCategory = campaign.G0_Category;
			unsubscribeFromThisCampaign.GCS_MediaType = campaign.G0_Type;
			unsubscribeFromThisCampaign.GCS_IsSubscribed = false;
			unsubscribeFromThisCampaign.GCS_G0 = campaign.PK;

			var unsubscribe = Factory.New<GlbCompanyCampaignSubscription>();
			unsubscribe.GCS_Email = contact2.OC_Email;
			unsubscribe.GCS_MediaCategory = campaign.G0_Category;
			unsubscribe.GCS_MediaType = campaign.G0_Type;
			unsubscribe.GCS_IsSubscribed = false;

			Factory.Save();

			Assert("Unsubscribed from this campaign", campaignItem1.IsUnsubscribed);
			Assert("Unsubscribed from somewhere else", !campaignItem2.IsUnsubscribed);
			Assert("Not Unsubscribed at all", !campaignItem3.IsUnsubscribed);
		}

		public void TestIsUnsubscribedForStaffRecipient()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			staff1.GS_FullName = nameof(staff1);
			staff2.GS_FullName = nameof(staff2);

			staff1.GS_EmailAddress = $"{nameof(staff1)}@abc.net";
			staff2.GS_EmailAddress = $"{nameof(staff2)}@abc.net";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			var campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = GlbStaffSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = staff1.PK;
			campaignItem2.G8_RecipientTableCode = GlbStaffSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = staff2.PK;

			Factory.Save();

			var unsubscribeFromThisCampaign = Factory.New<GlbCompanyCampaignSubscription>();
			unsubscribeFromThisCampaign.GCS_Email = staff1.GS_EmailAddress;
			unsubscribeFromThisCampaign.GCS_MediaCategory = campaign.G0_Category;
			unsubscribeFromThisCampaign.GCS_MediaType = campaign.G0_Type;
			unsubscribeFromThisCampaign.GCS_IsSubscribed = false;
			unsubscribeFromThisCampaign.GCS_G0 = campaign.PK;

			var unsubscribe = Factory.New<GlbCompanyCampaignSubscription>();
			unsubscribe.GCS_Email = staff2.GS_EmailAddress;
			unsubscribe.GCS_MediaCategory = campaign.G0_Category;
			unsubscribe.GCS_MediaType = campaign.G0_Type;
			unsubscribe.GCS_IsSubscribed = false;

			Factory.Save();

			Assert("Unsubscribed from this campaign", campaignItem1.IsUnsubscribed);
			Assert("Unsubscribed from somewhere else", !campaignItem2.IsUnsubscribed);

			unsubscribe.GCS_G0 = campaign.PK;

			Factory.Save();

			Assert("Unsubscribed from this campaign", campaignItem2.IsUnsubscribed);
		}

		#endregion

		#region Logging

		public void TestIsAutologged()
		{
			GlbCompanyCampaignItem item = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			item.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item.G8_RecipientID = ZGuid.NewZGuid();
			Factory.Save();

			AssertEquals("Should have 0 log", 0, item.Logs.GetAllLogs().Count);
			item.G8_TrackingStatus = TrackingStatusCodes.Codes.UNV;
			Factory.Save();
			AssertEquals("Should have 0 logs", 0, item.Logs.GetAllLogs().Count);
		}

		#endregion

		#region ImportChildRelatedActivityInfo

		public void TestImportChildInfoOnAttach()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			campaignItem.G8_G0 = campaign.PK;
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = ZGuid.NewZGuid();

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			Factory.Save();

			AssertEquals(ZGuid.Empty, opportunity.P8_G0);

			((IImportChildRelatedActivityInfoOnAttach)campaignItem).ImportChildInfo(opportunity, new ImportRelatedActivityNoDecisionFactory());
			AssertEquals(campaign.PK, opportunity.P8_G0);
		}

		public void TestImportChildInfoOnDetach()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			campaignItem.G8_G0 = campaign.PK;
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = ZGuid.NewZGuid();

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_G0 = campaignItem.G8_G0;
			Factory.Save();

			AssertEquals(campaign.PK, opportunity.P8_G0);

			((IImportChildRelatedActivityInfoOnDetach)campaignItem).ImportChildInfo(opportunity, new ImportRelatedActivityNoDecisionFactory());
			AssertEquals(ZGuid.Empty, opportunity.P8_G0);
		}

		#endregion

		#region Doc Manager

		public void TestImplementsIDocManagerSupport()
		{
			GlbCompanyCampaignItem campaign = Factory.New<GlbCompanyCampaignItem>();
			Assert("it is an IDocManagerSupport", typeof(IDocManagerSupport).IsAssignableFrom(typeof(GlbCompanyCampaignItem)));
			AssertEquals("GlbCompanyCampaignItem.DocManagerCode = GCI", "GCI", campaign.DocManagerInfo.DocManagerCode);
		}

		#endregion

		#region Other

		[TestDate(2002, 2, 2, 16, 15, 20)]
		public void TestAnyChangeShouldReactivateBatchSendRecurrenceTaskIfRequired()
		{
			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch = masterCampaign.AllTouches.AddNew();
			touch.G0_CampaignName = "touch";
			var settings = touch.SendSettings;
			settings.IsBatchSchedule = true;
			var scheduleTask = settings.ScheduleTask;

			Factory.Save();
			AssertEquals("Precondition", false, settings.ScheduleTask.S5_IsActive);

			var sentItem = touch.CampaignsItemsSent.AddNew();
			sentItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			sentItem.G8_RecipientID = Factory.NewWithValidTestData<OrgContact>().PK;
			sentItem.G8_TrackingStatus = TrackingStatusCodes.Codes.UNV;
			Factory.Save();
			AssertEquals("Should remain inactive", false, settings.ScheduleTask.S5_IsActive);

			var queuedItem = touch.CampaignsItemsSent.AddNew();
			queuedItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			queuedItem.G8_RecipientID = Factory.NewWithValidTestData<OrgContact>().PK;
			queuedItem.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			queuedItem.G8_ScheduleTimeUtc = new ZDateTime(2002, 2, 2);
			Factory.Save();
			AssertEquals("Should remain inactive", false, settings.ScheduleTask.S5_IsActive);
			settings.ScheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.Date.AddDays(1);
			var previousScheduleRunTime = settings.ScheduleTask.S5_NextScheduledPrintRunTimeUtc;

			var queuedForSchedulingItem = touch.CampaignsItemsSent.AddNew();
			queuedForSchedulingItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			queuedForSchedulingItem.G8_RecipientID = Factory.NewWithValidTestData<OrgContact>().PK;
			queuedForSchedulingItem.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			queuedForSchedulingItem.G8_ScheduleTimeUtc = ZDateTime.Empty;
			Factory.Save();
			AssertEquals("Should set to active", true, settings.ScheduleTask.S5_IsActive);
			AssertEquals("Should not have updated next run time because S5_NextScheduledPrintRunTimeUtc is in future", previousScheduleRunTime, settings.ScheduleTask.S5_NextScheduledPrintRunTimeUtc);

			settings.ScheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddHours(-settings.ScheduleTask.UtcOffsetOverride.Value.Hours - 1);
			Factory.Save();
			AssertNotEquals("Should have updated next run time", previousScheduleRunTime, settings.ScheduleTask.S5_NextScheduledPrintRunTimeUtc);

			settings.ScheduleTask.S5_IsActive = false;
			Factory.Save();
			AssertEquals("Precondition", false, settings.ScheduleTask.S5_IsActive);
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(2);
			queuedForSchedulingItem = touch.CampaignsItemsSent.AddNew();
			queuedForSchedulingItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			queuedForSchedulingItem.G8_RecipientID = Factory.NewWithValidTestData<OrgContact>().PK;
			queuedForSchedulingItem.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			queuedForSchedulingItem.G8_ScheduleTimeUtc = ZDateTime.Empty;
			Factory.Save();
			AssertEquals("Should set to active", true, settings.ScheduleTask.S5_IsActive);
			AssertNotEquals("Should have updated next run time", previousScheduleRunTime, settings.ScheduleTask.S5_NextScheduledPrintRunTimeUtc);
		}

		[TestDate(2015, 6, 11, 9, 8, 7)]
		[TestUtcOffset(10, 0, 0)]
		public void TestResetTrackingInfo()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ENW";

			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "XYZ";
			branch.GB_RL_NKHomePort = "USAAZ";  // -7 hours
			branch.GB_GC = GlbCompany.CurrentCompany.PK;

			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiry.O1_ContactName = "Edward Johns";
			inquiry.O1_PortOrCountry = "";

			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientID = inquiry.PK;
			campaignItem.G8_RecipientTableCode = "O1";

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff1.PK.ToGuid(), branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				campaignItem.ResetTrackingInfo(false);
				AssertEquals(new ZDateTime(2015, 6, 11, 9, 8, 7), campaignItem.G8_SystemLastEditTimeUtc);
				AssertEquals("UNV", campaignItem.G8_TrackingStatus);

				campaignItem.ResetTrackingInfo(true);
				AssertEquals(new ZDateTime(2015, 6, 11, 9, 8, 7), campaignItem.G8_SystemLastEditTimeUtc);
				AssertEquals("QUE", campaignItem.G8_TrackingStatus);
			}
		}

		public void TestIScheduleItemsProviderAsCampaignItem()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "GHI";
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "AAA";
			contact.OC_Email = "a@test.com";
			GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;

			Factory.Save();

			IScheduleItemsProvider provider = campaign.CampaignsItemsSent[0];
			AssertEquals("QUE", provider.ScheduleStatus);
			AssertEquals("G8", provider.TableCode);

			provider = campaign.CampaignsItemsSent[0];
			campaignItem.G8_TrackingStatus = "UNV";
			AssertEquals("SNT", provider.ScheduleStatus);
		}

		public void TestIScheduleItemsProviderAsCampaignItemForInsideSales()
		{
			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;

			var touch = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			masterCampaign.AllTouches.Add(touch);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "AAA";
			contact.OC_Email = "a@test.com";

			var campaignItem = touch.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;
			campaignItem.G8_TrackingStatus = TrackingStatusCodes.Codes.OPQ;
			Factory.Save();

			var provider = (IScheduleItemsProvider)touch.CampaignsItemsSent[0];
			AssertEquals(TrackingStatusCodes.Codes.QUE, provider.ScheduleStatus);
			AssertEquals(GlbCompanyCampaignItemSchema.Constants.Prefix, provider.TableCode);

			campaignItem.G8_TrackingStatus = TrackingStatusCodes.Codes.UNV;
			AssertEquals(ScheduleItemDataLoader.Schema.SentStatus, provider.ScheduleStatus);
		}

		public void TestVoteExamSurveyCampaignURL()
		{
			WebDataRegistry.Instance.WebCampaignUrl.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "http://meh");
			string expectedURL = VoteExamSurveyUrlHelper.GetCampaignUrl(CampaignItem);
			AssertEquals(expectedURL, CampaignItem.VoteExamSurveyCampaignURL);
		}

		public void TestTypeDeciderForLoad()
		{
			var itemFromOrgContact = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			itemFromOrgContact.G8_RecipientID = Factory.LoadTop1<OrgContact>(new ZQuery()).PK;
			itemFromOrgContact.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;

			var itemFromGlbStaff = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			itemFromGlbStaff.G8_RecipientID = Factory.NewWithValidTestData<GlbStaff>().PK;
			itemFromGlbStaff.G8_RecipientTableCode = GlbStaffSchema.Constants.Prefix;

			var itemFromJobApplicant = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			itemFromJobApplicant.G8_RecipientID = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IHRJobApplicant))).PK;
			itemFromJobApplicant.G8_RecipientTableCode = HRJobApplicantSchema.Constants.Prefix;

			Factory.Save();

			AssertEquals("Not a Job Applicant recipient", typeof(GlbCompanyCampaignItem), new BusinessObjectFactory().Load<GlbCompanyCampaignItem>(itemFromOrgContact.PK).GetType());
			AssertEquals("Not a Job Applicant recipient", typeof(GlbCompanyCampaignItem), new BusinessObjectFactory().Load<GlbCompanyCampaignItem>(itemFromGlbStaff.PK).GetType());
			AssertEquals("Not a Learning Centre Campaign", typeof(GlbCompanyCampaignItem), new BusinessObjectFactory().Load<GlbCompanyCampaignItem>(itemFromJobApplicant.PK).GetType());

			var campaign = (GlbCompanyCampaign)Factory.New<ILearningCentreCampaign>();
			campaign.FillWithValidTestData();
			itemFromJobApplicant.G8_G0 = campaign.PK;
			Factory.Save();
			AssertEquals(ObjectFactory.GetType<ILearningCentreCampaignItem>(), new BusinessObjectFactory().Load<GlbCompanyCampaignItem>(itemFromJobApplicant.PK).GetType());
		}

		public void TestTypeDeciderForNewAndBinding()
		{
			AssertEquals(typeof(GlbCompanyCampaignItem), new GlbCompanyCampaignItemTypeDecider().GetTypeForNew());
			AssertEquals(typeof(GlbCompanyCampaignItem), new GlbCompanyCampaignItemTypeDecider().GetTypeForBinding());
		}

		[TestDate]
		public void TestGetSeedForQuestionRandomiser()
		{
			GlbCompanyCampaignItem campaignItem = SetupTestDataForTestGetSeedForQuestionRandomiser(ZDateTime.BrettsBirthday);
			TimeSpan span = ZDateTime.BrettsBirthday - new ZDateTime(2008, 1, 1);
			int expected = (int)(span.TotalMilliseconds % int.MaxValue);
			AssertEquals(expected, campaignItem.GetSeedForQuestionRandomiser());
		}

		public void TestRecipientFromView()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "WXE";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "AA";
			contact.OC_Email = "a@test.com";
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;
			Factory.Save();

			var campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact }, campaign);
			AssertEquals(campaignContactCollection[0].PK, campaignItem.RecipientFromView.PK);
		}

		public void TestRecipientFromViewForStaff()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "AA";
			staff.GS_EmailAddress = "a@test.com";
			var campaign = (GlbCompanyCampaign)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IHRGlbCompanyCampaign)));
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = GlbStaffSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = staff.PK;
			Factory.Save();

			var campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { staff }, campaign);
			AssertEquals(campaignContactCollection[0].PK, campaignItem.RecipientFromView.PK);
		}

		protected virtual GlbCompanyCampaignItem SetupTestDataForTestGetSeedForQuestionRandomiser(ZDateTime referenceDate)
		{
			TestDateAttribute.Date = referenceDate.ToDateTime();

			return Factory.New<GlbCompanyCampaignItem>();
		}

		public void TestGetMyAccountUserAgreementUrlUnavailableWhenNotEDI()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			AssertEquals("Macro shouldn't work when not on EDI", "MACRO-ONLY-AVAILABLE-ON-EDI", campaignItem.GetMyAccountUserAgreementUrl("", false));
		}

		#endregion

		#region Delete

		public void TestDelete()
		{
			VoteExamSurveyAnswer answer1 = CampaignItem.PersistedAnswers.AddNew();
			VoteExamSurveyAnswer answer2 = CampaignItem.PersistedAnswers.AddNew();
			Assert(!answer1.IsDeleted);
			Assert(!answer2.IsDeleted);

			CampaignItem.Delete();
			Assert(CampaignItem.IsDeleted);
			Assert(answer1.IsDeleted);
			Assert(answer2.IsDeleted);
		}

		public void TestDelete_MovesAllRelatedActivityPivotsToCampaignHeader()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = ZGuid.NewZGuid();
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			Factory.Save();

			campaignItem.RelatedParentActivityPivotCollection.AddNewPivot(inquiry);
			campaignItem.RelatedChildActivityPivotCollection.AddNewPivot(opportunity);
			AssertContainsExactElementsInAnyOrder("Precondition", Enumerable.Empty<IRelatableActivity>(), campaign.RelatedParentActivityPivotCollection.Activities);
			AssertContainsExactElementsInAnyOrder("Precondition", Enumerable.Empty<IRelatableActivity>(), campaign.RelatedChildActivityPivotCollection.Activities);
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { inquiry }, campaignItem.RelatedParentActivityPivotCollection.Activities);
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { opportunity }, campaignItem.RelatedChildActivityPivotCollection.Activities);

			campaignItem.Delete();
			AssertContainsExactElementsInAnyOrder(new[] { inquiry }, campaign.RelatedParentActivityPivotCollection.Activities);
			AssertContainsExactElementsInAnyOrder(new[] { opportunity }, campaign.RelatedChildActivityPivotCollection.Activities);
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<IRelatableActivity>(), campaignItem.RelatedParentActivityPivotCollection.Activities);
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<IRelatableActivity>(), campaignItem.RelatedChildActivityPivotCollection.Activities);
		}

		public void TestDelete_DeletesAllRelatedActivityPivotsIfNoCampaignHeader()
		{
			var campaignItem = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = ZGuid.NewZGuid();
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			Factory.Save();

			campaignItem.RelatedParentActivityPivotCollection.AddNewPivot(inquiry);
			campaignItem.RelatedChildActivityPivotCollection.AddNewPivot(opportunity);
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { inquiry }, campaignItem.RelatedParentActivityPivotCollection.Activities);
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { opportunity }, campaignItem.RelatedChildActivityPivotCollection.Activities);

			campaignItem.Delete();
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<IRelatableActivity>(), campaignItem.RelatedParentActivityPivotCollection.Activities);
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<IRelatableActivity>(), campaignItem.RelatedChildActivityPivotCollection.Activities);
		}

		#endregion

		#region Email Sender

		public void TestGetEmailSenderStaff()
		{
			GlbStaff sCoordinator = Factory.NewWithValidTestData<GlbStaff>();
			sCoordinator.GS_EmailAddress = "florenzo@cargowise.com";
			sCoordinator.GS_Code = "FL";

			RefCountry country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US");

			RefCountry country2 = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = "AUBNE";

			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.OC_ContactName = "John Smiths";
			orgContact.OC_OH = org.PK;

			var campaignItem = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			campaignItem.G8_RecipientID = orgContact.PK;

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Name = "Eaglets Pty Ltd";
			company.GC_RN_NKCountryCode = country.Code;

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Name = "Fly Pty Ltd";
			company2.GC_OH_OrgProxy = org.PK;
			company2.GC_RN_NKCountryCode = country2.Code;

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.ORG;
			campaign.G0_EmailSenderRole = "SAL";
			campaign.G0_GC = company.PK;
			campaign.G0_GS_NKCampaignCoordinator = sCoordinator.GS_Code;

			campaignItem.G8_G0 = campaign.PK;

			OrgStaffAssignmentsCollection staffAssignmentCollection = new OrgStaffAssignmentsCollection(org);

			var assignment1 = org.StaffAssignments.AddNew();
			assignment1.O8_Role = "SAL";
			assignment1.O8_GS_NKPersonResponsible = "NED";
			assignment1.O8_GC = company.PK;
			assignment1.O8_OH = org.PK;
			staffAssignmentCollection.Add(assignment1);

			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "eddy@cargowise.com";
			staff1.GS_Code = "NED";

			Assert("Staff1 fulfils first condition", campaignItem.GetEmailSenderStaff() == staff1);

			staffAssignmentCollection.RemoveAll();
			staffAssignmentCollection = new OrgStaffAssignmentsCollection(org);
			var assignment12 = org.StaffAssignments.AddNew();
			assignment12.O8_Role = "SAL";
			assignment12.O8_GS_NKPersonResponsible = "SAM";
			assignment12.O8_GC = company2.PK;
			assignment12.O8_OH = org.PK;
			staffAssignmentCollection.Add(assignment12);

			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_EmailAddress = "sam@cargowise.com";
			staff2.GS_Code = "SAM";

			Assert("Staff2 satisfies the condition ", campaignItem.GetEmailSenderStaff() == staff2);

			staffAssignmentCollection.RemoveAll();
			staffAssignmentCollection = new OrgStaffAssignmentsCollection(org);
			var assignment2 = org.StaffAssignments.AddNew();
			assignment2.O8_Role = "SAL";
			assignment2.O8_GS_NKPersonResponsible = "RIS";
			assignment2.O8_GC = ZGuid.Empty;
			staffAssignmentCollection.Add(assignment2);

			GlbStaff staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_EmailAddress = "richard@cargowise.com";
			staff3.GS_Code = "RIS";

			Assert("Staff3 satisfies the condition ", campaignItem.GetEmailSenderStaff() == staff3);

			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
			campaign.G0_EmailSenderRole = ZString.Empty;
			AssertEquals(true, campaignItem.GetEmailSenderStaff() == sCoordinator);

			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.ORG;
			campaign.G0_EmailSenderRole = "SAL";
			staffAssignmentCollection.RemoveAll();
			staffAssignmentCollection = new OrgStaffAssignmentsCollection(org);
			var assignment3 = org.StaffAssignments.AddNew();
			assignment3.O8_Role = "ACT";
			assignment3.O8_GS_NKPersonResponsible = "RIS";
			assignment3.O8_GC = ZGuid.Empty;
			staffAssignmentCollection.Add(assignment3);

			Assert("Campaign coordinator satisfies the condition ", campaignItem.GetEmailSenderStaff() == sCoordinator);

			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
			AssertNull(campaignItem.GetEmailSenderStaff());
		}

		#endregion

		#region Drip Marketing

		public void TestCanTransition()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = nameof(contact);
			contact.OC_Email = $"{nameof(contact)}@abc.net";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var item = campaign.CampaignsItemsSent.AddNew();
			item.G8_RecipientID = contact.PK;

			AssertEquals(true, item.CanTransition);

			item.G8_IsSuspended = true;
			AssertEquals(false, item.CanTransition);

			item.G8_IsSuspended = false;
			item.G8_IsBlocked = true;
			AssertEquals(false, item.CanTransition);

			item.G8_IsBlocked = false;
			item.G8_TrackingStatus = "NDR";
			AssertEquals(false, item.CanTransition);

			item.G8_TrackingStatus = "QUE";
			AssertEquals(false, item.CanTransition);

			item.G8_TrackingStatus = "UNV";
			AssertEquals(true, item.CanTransition);

			item.G8_RecipientID = contact.PK;

			var unsubscribeFromThisCampaign = Factory.New<GlbCompanyCampaignSubscription>();
			unsubscribeFromThisCampaign.GCS_Email = contact.OC_Email;
			unsubscribeFromThisCampaign.GCS_MediaCategory = campaign.G0_Category;
			unsubscribeFromThisCampaign.GCS_MediaType = campaign.G0_Type;
			unsubscribeFromThisCampaign.GCS_IsSubscribed = false;
			unsubscribeFromThisCampaign.GCS_G0 = campaign.PK;

			Assert("Unsubscribed from this campaign", item.IsUnsubscribed);
		}

		public void TestItemsInNextTouchesDeletedIfNDR()
		{
			var helper = new GlbCompanyCampaignTestHelper(Factory);
			helper.SetupDripCampaign();

			helper.Item2_1a.G8_TrackingStatus = "NDR";
			AssertEquals(false, helper.Item2_1a.G8_IsSuspended);
			AssertEquals(true, helper.Item1_2a.IsDeleted);

			helper.Item1_1b.G8_TrackingStatus = "NDR";
			AssertEquals(false, helper.Item1_1b.G8_IsSuspended);
			AssertEquals(false, helper.Item1_2b.IsDeleted);
			AssertEquals(false, helper.Item1_2b.G8_IsSuspended);
		}

		public void TestIsPendingTransition()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch = master.AllTouches.AddNew();

			var item = Factory.New<GlbCompanyCampaignItem>();

			AssertEquals(false, item.IsPendingTransition);
			(item as IRelatableActivity).OnRelatedActivitySaving(Factory.New<OrgOpportunity>());

			item.G8_G0 = touch.PK;
			AssertEquals(true, item.IsPendingTransition);

			item.G8_G0 = master.PK;
			AssertEquals(true, item.IsPendingTransition);
		}

		public void TestIsPendingTransition_IsSent()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch = master.AllTouches.AddNew();
			var item = Factory.New<GlbCompanyCampaignItem>();
			item.G8_G0 = touch.PK;
			item.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item.G8_RecipientID = ZGuid.NewZGuid();

			AssertEquals(false, item.IsPendingTransition);

			item.ResetTrackingInfo(false);
			AssertEquals(true, item.IsPendingTransition);
		}

		public void TestSalesActivityTriggersTransition()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch = master.AllTouches.AddNew();
			touch.G0_CampaignName = "touch";
			var item = Factory.New<GlbCompanyCampaignItemForTest>();
			item.G8_G0 = touch.PK;
			item.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item.G8_RecipientID = ZGuid.NewZGuid();

			(item as IRelatableActivity).OnRelatedActivitySaving(Factory.NewWithValidTestData<OrgOpportunity>());
			AssertEquals(true, item.IsPendingTransition);

			Factory.Save();
			AssertEquals(1, (item.CompanyCampaign as GlbCompanyCampaignForTest).TransitionCounter);
			AssertEquals(1, (item.CompanyCampaign as GlbCompanyCampaignForTest).TransferItemPKs.Length);
			AssertEquals(item.PK, (item.CompanyCampaign as GlbCompanyCampaignForTest).TransferItemPKs[0]);
		}

		public void TestSendingDoesNotTriggerTransition()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch = master.AllTouches.AddNew();
			touch.G0_CampaignName = "touch";
			var item = Factory.New<GlbCompanyCampaignItemForTest>();
			item.G8_G0 = touch.PK;
			item.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item.G8_RecipientID = ZGuid.NewZGuid();

			item.ResetTrackingInfo(false);
			AssertEquals(true, item.IsPendingTransition);

			Factory.Save();
			AssertEquals(0, (item.CompanyCampaign as GlbCompanyCampaignForTest).TransitionCounter);
			AssertNull((item.CompanyCampaign as GlbCompanyCampaignForTest).TransferItemPKs);
		}

		public class GlbCompanyCampaignItemForTest : GlbCompanyCampaignItem
		{
			public GlbCompanyCampaignItemForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override GlbCompanyCampaign CompanyCampaign
			{
				get { return Factory.Load<GlbCompanyCampaignForTest>(G8_G0); }
			}
		}

		class GlbCompanyCampaignForTest : GlbCompanyCampaign
		{
			public ZGuid[] TransferItemPKs { get; private set; }
			public GlbCompanyCampaignForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override List<Tuple<ZGuid, TransitionStatus>> TransitionAndSchedule(IEnumerable<ZGuid> transferItemPK, List<String> errorList = null)
			{
				TransferItemPKs = transferItemPK.ToArray();
				TransitionCounter++;
				return new List<Tuple<ZGuid, TransitionStatus>>();
			}

			public int TransitionCounter;
		}

		public void TestGetPreviousTransitionCampaignItem()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABCD";

			var contact = org.Contacts.AddNew();
			contact.OC_Email = "testcontct@test.com";
			contact.OC_ContactName = "Test Contact";

			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;

			var touch1A = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1A.G0_HorizontalId = 1;
			touch1A.G0_VerticalId = "A";
			touch1A.G0_G0_Master = masterCampaign.PK;
			touch1A.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.PreApproachEmail;
			masterCampaign.AllTouches.Add(touch1A);

			var campaignItem1A = touch1A.CampaignsItemsSent.AddNew();
			campaignItem1A.G8_TrackingStatus = TrackingStatusCodes.Codes.UNV;
			campaignItem1A.G8_ScheduleTimeUtc = ZDateTime.UtcNow.AddDays(-2);
			campaignItem1A.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1A.G8_RecipientID = contact.PK;

			var touch2A = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2A.G0_HorizontalId = 2;
			touch2A.G0_VerticalId = "A";
			touch2A.G0_G0_Master = masterCampaign.PK;
			touch2A.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			masterCampaign.AllTouches.Add(touch2A);

			var campaignItem2A = touch2A.CampaignsItemsSent.AddNew();
			campaignItem2A.G8_TrackingStatus = TrackingStatusCodes.Codes.OPQ;
			campaignItem2A.G8_ScheduleTimeUtc = ZDateTime.UtcNow.AddDays(-2);
			campaignItem2A.G8_RecipientTableCode = contact.TablePrefix;
			campaignItem2A.G8_RecipientID = contact.PK;
			Factory.Save();

			AssertEquals(campaignItem1A.PK, campaignItem2A.GetPreviousTransitionCampaignItem().PK);
		}

		#endregion

		#region Implementation

		public GlbCompanyCampaignItem CampaignItem
		{
			get { return (GlbCompanyCampaignItem)base.CachedBusinessObject; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Testing")]
		GlbCompanyCampaignItem NewCampaignItem()
		{
			return (GlbCompanyCampaignItem)Factory.New(GetExpectedBusinessObjectType());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var campaignItem = (GlbCompanyCampaignItem)base.GetNewBusinessObject();
			campaignItem.G8_G0 = Factory.NewWithValidTestData<GlbCompanyCampaign>().PK;
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = ZGuid.NewZGuid();
			return campaignItem;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var campaignItem = (GlbCompanyCampaignItem)base.GetNewBusinessObjectForDeleteTest(factory);
			campaignItem.G8_G0 = factory.NewWithValidTestData<GlbCompanyCampaign>().PK;
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = ZGuid.NewZGuid();
			return campaignItem;
		}

		#endregion
	}
}
