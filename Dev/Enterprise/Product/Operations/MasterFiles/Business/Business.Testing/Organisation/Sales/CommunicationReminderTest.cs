using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Services.Calendar;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CommunicationReminderTest : TestCaseWithFactory
	{
		public void TestLogReference_Confirmed()
		{
			var recipient1 = new CommunicationReminderRecipient(ZGuid.NewZGuid(), "Andrew", "andrew@test.com");
			var recipient2 = new CommunicationReminderRecipient(ZGuid.NewZGuid(), "Luong", "luong@test.com");

			var parent = Factory.NewWithValidTestData<ProcessTask>();
			Factory.Save();
			var parentPk = parent.PK;
			var slTable = parent.TableName;
			var reminder = new CommunicationReminder("MyIdentifier", parentPk, slTable, DateTimeKind.Utc, new ZDateTime(2013, 1, 1), new ZDateTime(2013, 1, 1, 0, 30, 0), "My subject", "My body", "My html body");
			reminder.ReminderType = ReminderType.Confirmed;
			reminder.Recipients.Add(recipient1);
			reminder.Recipients.Add(recipient2);
			reminder.CreateAppointment();

			var logs = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, parentPk).AddToFilter(StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.NotEqual, Events.AddedARecordToTheSystemCode));
			var expectedLogReferences = new[]
			{
				string.Format("REMINDER:FollowUpCall Recipient:{0} Type:{1} Date:{2} Seq:{3}", recipient1.ParentPk, CommunicationReminder.Constants.TypeRequestCode, new ZDateTime(2013, 1, 1).ToLongTimeString(), 0),
				string.Format("REMINDER:FollowUpCall Recipient:{0} Type:{1} Date:{2} Seq:{3}", recipient2.ParentPk, CommunicationReminder.Constants.TypeRequestCode, new ZDateTime(2013, 1, 1).ToLongTimeString(), 0)
			};
			AssertContainsExactElementsInAnyOrder(expectedLogReferences, logs.Select(i => i.SL_Reference.ToString()));

			var log = logs[0];
			AssertEquals(true, CommunicationReminder.IsReminderSentLog(log));
			AssertEquals(new ZDateTime(2013, 1, 1), CommunicationReminder.GetReminderDateFromLog(log));
		}

		public void TestLogReference_Cancellation()
		{
			var recipient1 = new CommunicationReminderRecipient(ZGuid.NewZGuid(), "Andrew", "andrew@test.com");
			var recipient2 = new CommunicationReminderRecipient(ZGuid.NewZGuid(), "Luong", "luong@test.com");

			var parent = Factory.NewWithValidTestData<ProcessTask>();
			Factory.Save();
			var parentPk = parent.PK;
			var slTable = parent.TableName;
			var reminder = new CommunicationReminder("MyIdentifier", parentPk, slTable, DateTimeKind.Utc, ZDateTime.Empty, new ZDateTime(2013, 1, 1, 0, 30, 0), "My subject", "My body", "My html body");
			reminder.ReminderType = ReminderType.Cancellation;
			reminder.Recipients.Add(recipient1);
			reminder.Recipients.Add(recipient2);
			reminder.CreateAppointment();

			var logs = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, parentPk).AddToFilter(StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.NotEqual, Events.AddedARecordToTheSystemCode));
			var expectedLogReferences = new[]
			{
				string.Format("REMINDER:FollowUpCall Recipient:{0} Type:{1} Date:{2} Seq:{3}", recipient1.ParentPk, CommunicationReminder.Constants.TypeCancelCode, ZDateTime.Empty.ToLongTimeString(), 0),
				string.Format("REMINDER:FollowUpCall Recipient:{0} Type:{1} Date:{2} Seq:{3}", recipient2.ParentPk, CommunicationReminder.Constants.TypeCancelCode, ZDateTime.Empty.ToLongTimeString(), 0)
			};
			AssertContainsExactElementsInAnyOrder(expectedLogReferences, logs.Select(i => i.SL_Reference.ToString()));

			var log = logs[0];
			AssertEquals(false, CommunicationReminder.IsReminderSentLog(log));
			AssertEquals(ZDateTime.Empty, CommunicationReminder.GetReminderDateFromLog(log));
		}

		[TestDate(2006, 6, 6)]  // test uses daylight savings
		public void TestNew_WithSalesCall()
		{
			Env.Registry.SetOrgAllowMixedCase(true);
			#region Test Data

			OrgHeader org = Factory.NewWithPrimaryKey<OrgHeader>(new Guid("479520de-ba34-4339-b9ae-03ed90c5c864"));
			org.FillWithValidTestData();
			org.OH_FullName = "Some Special Organisation";
			org.OH_RL_NKClosestPort = "INBOM";
			org.OH_Code = "SOMEORG1";

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Zubin Appoo";
			contact.OC_Title = "Blah blah";
			contact.OC_JobCategory = "Hello";
			contact.OC_Email = "zubin.appoo@edi.com.au";
			contact.OC_Fax = "+61 2 9025 1199";
			contact.OC_Phone = "+61 2 9025 1180";
			contact.OC_Mobile = "+61 (14) 8112-3456";

			GlbStaff staffCoordinator = Factory.New<GlbStaff>();
			staffCoordinator.GS_Code = "ADL";
			staffCoordinator.GS_FullName = "Andrew Luong";

			org.MainAddress.OA_Address1 = "18 Henricks Avenue";
			org.MainAddress.OA_Address2 = "My House";
			org.MainAddress.OA_City = "Newington";
			org.MainAddress.OA_State = "NSW";
			org.MainAddress.OA_PostCode = "2127";
			org.MainAddress.OA_Email = "mainorg@example.com";
			org.MainAddress.OA_Phone = "+61 2 9911 1199";
			org.MainAddress.OA_Fax = "+61 2 9911 9911";

			OrgSalesCall salesCall = org.SalesCalls.AddNew();
			salesCall.OQ_CommunicationID = "CM00001074";
			salesCall.OQ_CallDate = new ZDateTime(1981, 2, 24);
			salesCall.OQ_NextCall = new ZDateTime(2006, 3, 10, 9, 0, 0);
			salesCall.OQ_TypeOfCall = "1ST";
			salesCall.OQ_CallSummary = "Really good call";
			salesCall.OQ_SalesCallNotes = ZBlob.FromUTF8("We had a good time, it was a good successful conversation");
			salesCall.ClientVisibleNote = "hello world";
			salesCall.OQ_FollowupNotes = ZBlob.FromUTF8("Gotta find out how to add 1 and 3 together...");
			salesCall.OQ_OC = contact.PK;
			salesCall.OQ_GS_NKSalesRep = "ADL";
			Factory.Save();

			string plainTextExpectedForInternalSchedule =
@"Client: [SOMEORG1] Some Special Organisation
Communication ID: CM00001074

Method: First call to prospective client.
Staff Coordinator: Andrew Luong

Contact: Zubin Appoo
Title: Blah blah
Job Category: Hello
Email: zubin.appoo@edi.com.au
Phone: +61 2 9025 1180
Mobile: +61 (14) 8112-3456

Client Address:
18 Henricks Avenue
My House
Newington NSW 2127
India
Office Email: mainorg@example.com
Office Phone: +61 2 9911 1199

Last Communication Date: 24-Feb-81

Internal Notes:
We had a good time, it was a good successful conversation

Follow Up Notes:
Gotta find out how to add 1 and 3 together...";

			string expectedUrl = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.Communication, salesCall.PK.ToGuid());

			string htmlExpectedForInternalSchedule =
string.Format(@"<HTML><HEAD><TITLE></TITLE></HEAD><BODY><FONT face=""Arial""><strong>Client</strong>: [<a href=""edient:Command=ShowEditForm&ControllerID=Organisation&BusinessEntityPK=479520de-ba34-4339-b9ae-03ed90c5c864&VersionNumber={0}&Hash=%2b7mh4ylQc5igNAh6FqQqdX6pro2teONbq"">SOMEORG1</a>] Some Special Organisation
<strong>Communication ID</strong>: <a href=""{1}"">CM00001074</a>

<strong>Method</strong>: First call to prospective client.
<strong>Staff Coordinator</strong>: Andrew Luong

<strong>Contact</strong>: Zubin Appoo
<strong>Title</strong>: Blah blah
<strong>Job Category</strong>: Hello
<strong>Email</strong>: zubin.appoo@edi.com.au
<strong>Phone</strong>: +61 2 9025 1180
<strong>Mobile</strong>: +61 (14) 8112-3456

<strong>Client Address</strong>:
18 Henricks Avenue
My House
Newington NSW 2127
India
<strong>Office Email</strong>: mainorg@example.com
<strong>Office Phone</strong>: +61 2 9911 1199

<strong>Last Communication Date</strong>: 24-Feb-81

<u><strong>Internal Notes</strong></u>:
We had a good time, it was a good successful conversation

<u><strong>Follow Up Notes</strong></u>:
Gotta find out how to add 1 and 3 together...</FONT></BODY></HTML>", new EnterpriseInformationRetriever().VersionNumber, expectedUrl);

			string plainTextExpectedForInternalCancel = plainTextExpectedForInternalSchedule.Replace("scheduled", "canceled");
			string htmlExpectedForInternalCancel = htmlExpectedForInternalSchedule.Replace("scheduled", "canceled");

			string plainTextExpectedForPublic =
@"hello world";

			string htmlExpectedForPublic =
@"<HTML><HEAD><TITLE></TITLE></HEAD><BODY><FONT face=""Arial"">hello world
</FONT></BODY></HTML>";

			var expectedUTCFromDate = new ZDateTime(2006, 3, 10, 9, 0, 0).ToDateTime();
			var expectedUTCToDate = new ZDateTime(2006, 3, 10, 9, 30, 0).ToDateTime();

			#endregion

			Reminder internalScheduleReminder = CommunicationReminder.New(salesCall.OQ_NextCall, salesCall.OQ_NextCall, salesCall, new[] { new CommunicationReminderRecipient(ZGuid.Empty, "", "") }, false);
			AssertReminderProperties(salesCall, internalScheduleReminder, "[1ST] Some Special Organisation | Really good call", plainTextExpectedForInternalSchedule, htmlExpectedForInternalSchedule, expectedUTCFromDate, expectedUTCToDate);
			Reminder internalCancelReminder = CommunicationReminder.New(salesCall.OQ_NextCall, ZDateTime.Empty, salesCall, new[] { new CommunicationReminderRecipient(ZGuid.Empty, "", "") }, false);
			AssertReminderProperties(salesCall, internalCancelReminder, "[1ST] Some Special Organisation | Really good call", plainTextExpectedForInternalCancel, htmlExpectedForInternalCancel, expectedUTCFromDate, expectedUTCToDate);

			salesCall.OQ_TypeOfCall = "FUP";
			internalScheduleReminder = CommunicationReminder.New(salesCall.OQ_NextCall, salesCall.OQ_NextCall, salesCall, new[] { new CommunicationReminderRecipient(ZGuid.Empty, "", "") }, false);
			Assert("TypeOfCall code should correspond with the description", plainTextExpectedForInternalSchedule != internalScheduleReminder.Body);

			Reminder publicScheduleReminder = CommunicationReminder.New(salesCall.OQ_NextCall, salesCall.OQ_NextCall, salesCall, new[] { new CommunicationReminderRecipient(ZGuid.Empty, "", "") }, true);
			AssertReminderProperties(salesCall, publicScheduleReminder, "Really good call", plainTextExpectedForPublic, htmlExpectedForPublic, expectedUTCFromDate, expectedUTCToDate);
			Reminder publicCancelReminder = CommunicationReminder.New(salesCall.OQ_NextCall, ZDateTime.Empty, salesCall, new[] { new CommunicationReminderRecipient(ZGuid.Empty, "", "") }, true);
			AssertReminderProperties(salesCall, publicCancelReminder, "Really good call", plainTextExpectedForPublic, htmlExpectedForPublic, expectedUTCFromDate, expectedUTCToDate);
		}

		public void TestNew_WithInquiryLinkedSalesCall()
		{
			Env.Registry.SetOrgAllowMixedCase(true);

			#region Test Data

			GlbStaff staffCoordinator = Factory.New<GlbStaff>();
			staffCoordinator.GS_Code = "ADL";
			staffCoordinator.GS_FullName = "Andrew Luong";

			SalesEnquiry inquiry = Factory.New<SalesEnquiry>();
			inquiry.O1_CompanyName = "Some Special Organisation";
			inquiry.O1_Address1 = "18 Henricks Avenue";
			inquiry.O1_Address2 = "My House";
			inquiry.O1_City = "Newington";
			inquiry.O1_State = "NSW";
			inquiry.O1_PostCode = "2127";

			inquiry.O1_ContactName = "Zubin Appoo";
			inquiry.O1_JobCategory = "Hello";
			inquiry.O1_Email = "zubin.appoo@edi.com.au";
			inquiry.O1_Fax = "+61 2 9025 1199";
			inquiry.O1_Phone = "+61 2 9025 1180";
			inquiry.O1_Mobile = "+61 (14) 8112-3456";

			OrgSalesCall salesCall = Factory.New<OrgSalesCall>();
			salesCall.OQ_CommunicationID = "CM00001074";
			salesCall.OQ_CallDate = new ZDateTime(1981, 2, 24);
			salesCall.OQ_NextCall = new ZDateTime(2006, 3, 10, 9, 0, 0);
			salesCall.OQ_TypeOfCall = "1ST";
			salesCall.OQ_CallSummary = "Really good call";
			salesCall.OQ_SalesCallNotes = ZBlob.FromUTF8("We had a good time, it was a good successful conversation");
			salesCall.ClientVisibleNote = "hello world";
			salesCall.OQ_FollowupNotes = ZBlob.FromUTF8("Gotta find out how to add 1 and 3 together...");
			salesCall.OQ_GS_NKSalesRep = "ADL";
			salesCall.LinkedInquiry = inquiry;
			Factory.Save();

			string plainTextExpectedForInternalSchedule =
@"Client: Some Special Organisation
Communication ID: CM00001074

Method: First call to prospective client.
Staff Coordinator: Andrew Luong

Contact: Zubin Appoo
Job Category: Hello
Email: zubin.appoo@edi.com.au
Phone: +61 2 9025 1180
Mobile: +61 (14) 8112-3456

Client Address:
18 Henricks Avenue
My House
Newington NSW 2127

Internal Notes:
We had a good time, it was a good successful conversation

Follow Up Notes:
Gotta find out how to add 1 and 3 together...";

			string expectedUrl = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.Communication, salesCall.PK.ToGuid());

			string htmlExpectedForInternalSchedule =
string.Format(@"<HTML><HEAD><TITLE></TITLE></HEAD><BODY><FONT face=""Arial""><strong>Client</strong>: Some Special Organisation
<strong>Communication ID</strong>: <a href=""{0}"">CM00001074</a>

<strong>Method</strong>: First call to prospective client.
<strong>Staff Coordinator</strong>: Andrew Luong

<strong>Contact</strong>: Zubin Appoo
<strong>Job Category</strong>: Hello
<strong>Email</strong>: zubin.appoo@edi.com.au
<strong>Phone</strong>: +61 2 9025 1180
<strong>Mobile</strong>: +61 (14) 8112-3456

<strong>Client Address</strong>:
18 Henricks Avenue
My House
Newington NSW 2127

<u><strong>Internal Notes</strong></u>:
We had a good time, it was a good successful conversation

<u><strong>Follow Up Notes</strong></u>:
Gotta find out how to add 1 and 3 together...</FONT></BODY></HTML>", expectedUrl);

			string plainTextExpectedForInternalCancel = plainTextExpectedForInternalSchedule.Replace("scheduled", "canceled");
			string htmlExpectedForInternalCancel = htmlExpectedForInternalSchedule.Replace("scheduled", "canceled");

			string plainTextExpectedForPublic =
@"hello world";

			string htmlExpectedForPublic =
@"<HTML><HEAD><TITLE></TITLE></HEAD><BODY><FONT face=""Arial"">hello world
</FONT></BODY></HTML>";

			var expectedUTCFromDate = new ZDateTime(2006, 3, 10, 9, 0, 0).ToDateTime();
			var expectedUTCToDate = new ZDateTime(2006, 3, 10, 9, 30, 0).ToDateTime();

			#endregion

			Reminder internalScheduleReminder = CommunicationReminder.New(salesCall.OQ_NextCall, salesCall.OQ_NextCall, salesCall, new[] { new CommunicationReminderRecipient(ZGuid.Empty, "", "") }, false);
			AssertReminderProperties(salesCall, internalScheduleReminder, "[1ST] Some Special Organisation | Really good call", plainTextExpectedForInternalSchedule, htmlExpectedForInternalSchedule, expectedUTCFromDate, expectedUTCToDate);
			Reminder internalCancelReminder = CommunicationReminder.New(salesCall.OQ_NextCall, ZDateTime.Empty, salesCall, new[] { new CommunicationReminderRecipient(ZGuid.Empty, "", "") }, false);
			AssertReminderProperties(salesCall, internalCancelReminder, "[1ST] Some Special Organisation | Really good call", plainTextExpectedForInternalCancel, htmlExpectedForInternalCancel, expectedUTCFromDate, expectedUTCToDate);

			Reminder publicScheduleReminder = CommunicationReminder.New(salesCall.OQ_NextCall, salesCall.OQ_NextCall, salesCall, new[] { new CommunicationReminderRecipient(ZGuid.Empty, "", "") }, true);
			AssertReminderProperties(salesCall, publicScheduleReminder, "Really good call", plainTextExpectedForPublic, htmlExpectedForPublic, expectedUTCFromDate, expectedUTCToDate);
			Reminder publicCancelReminder = CommunicationReminder.New(salesCall.OQ_NextCall, ZDateTime.Empty, salesCall, new[] { new CommunicationReminderRecipient(ZGuid.Empty, "", "") }, true);
			AssertReminderProperties(salesCall, publicCancelReminder, "Really good call", plainTextExpectedForPublic, htmlExpectedForPublic, expectedUTCFromDate, expectedUTCToDate);
		}

		void AssertReminderProperties(OrgSalesCall salesCall, Reminder reminder, string subject, string plainTextExpected, string htmlExpected, ZDateTime uTCFromDate, ZDateTime uTCToDate)
		{
			AssertEquals(subject, reminder.Subject);
			AssertEquals("ID should be specific to the call", salesCall.PK.ToString() + CommunicationReminder.Constants.ReminderIdentifier, reminder.ID);
			Assert("Reminder should have html body", reminder.HasHtmlBody);
			AssertMultilineASCIIEquals("Calendar reminder created with correct plain text body", plainTextExpected, reminder.Body);
			AssertMultilineASCIIEquals("Calendar reminder created with correct html body", htmlExpected, reminder.HtmlBody);
			AssertEquals("Correct UTC from date", uTCFromDate, reminder.UTCDateFrom);
			AssertEquals("Correct UTC to date", uTCToDate, reminder.UTCDateTo);
		}

		public void TestNew_Address()
		{
			Env.Registry.SetOrgAllowMixedCase(true);

			#region Test Data

			OrgHeader org = Factory.NewWithPrimaryKey<OrgHeader>(new Guid("479520de-ba34-4339-b9ae-03ed90c5c864"));
			org.FillWithValidTestData();
			org.OH_FullName = "Some Special Organisation";
			org.OH_RL_NKClosestPort = "INBOM";
			org.OH_Code = "SOMEORG1";

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Zubin Appoo";
			contact.OC_Email = "zubin.appoo@edi.com.au";
			contact.OC_Fax = "+61 2 9025 1199";
			contact.OC_Phone = "+61 2 9025 1180";

			GlbStaff staffCoordinator = Factory.New<GlbStaff>();
			staffCoordinator.GS_Code = "ADL";
			staffCoordinator.GS_FullName = "Andrew Luong";

			org.MainAddress.OA_Address1 = "18 Henricks Avenue";
			org.MainAddress.OA_Address2 = "My House";
			org.MainAddress.OA_City = "Newington";
			org.MainAddress.OA_State = "NSW";
			org.MainAddress.OA_PostCode = "2127";
			org.MainAddress.OA_Email = "mainorg@example.com";
			org.MainAddress.OA_Phone = "+61 2 9911 1199";
			org.MainAddress.OA_Fax = "+61 2 9911 9911";

			OrgSalesCall lastSalesCall = org.SalesCalls.AddNew();
			lastSalesCall.OQ_CallDate = new ZDateTime(2006, 3, 5);

			OrgSalesCall salesCall = org.SalesCalls.AddNew();
			salesCall.OQ_CommunicationID = "CM00001074";
			salesCall.OQ_NextCall = new ZDateTime(2006, 3, 10, 9, 0, 0);
			salesCall.OQ_TypeOfCall = "1ST";
			salesCall.OQ_CallSummary = "Really good call";
			salesCall.OQ_Duration = new ZDateTime(ZDate.Today.Year, 1, 1, 0, 0, 0);
			salesCall.OQ_SalesCallNotes = ZBlob.FromUTF8("We had a good time, it was a good successful conversation");
			salesCall.OQ_FollowupNotes = ZBlob.FromUTF8("Gotta find out how to add 1 and 3 together...");
			salesCall.OQ_OC = contact.PK;
			salesCall.OQ_GS_NKSalesRep = "ADL";
			Factory.Save();

			var orgOverride = Factory.NewWithValidTestData<OrgHeader>();
			orgOverride.OH_FullName = "Some Other Special Organisation";
			orgOverride.OH_RL_NKClosestPort = "INBOM";
			orgOverride.OH_Code = "SOMEORG2";

			orgOverride.MainAddress.OA_Address1 = "20 Henricks Avenue";
			orgOverride.MainAddress.OA_Address2 = "Other House";
			orgOverride.MainAddress.OA_City = "Newington";
			orgOverride.MainAddress.OA_State = "NSW";
			orgOverride.MainAddress.OA_PostCode = "2128";
			orgOverride.MainAddress.OA_Email = "otherorg@example.com";
			orgOverride.MainAddress.OA_Phone = "+61 2 9922 2299";
			orgOverride.MainAddress.OA_Fax = "+61 2 9922 9922";

			contact.OC_OH_AddressOverride = orgOverride.PK;

			string plainTextExpected =
@"Client: [SOMEORG1] Some Special Organisation
Communication ID: CM00001074

Method: First call to prospective client.
Staff Coordinator: Andrew Luong

Contact: Zubin Appoo
Job Category: Employee (Undefined)
Email: zubin.appoo@edi.com.au
Phone: +61 2 9025 1180

Client Address:
20 Henricks Avenue
Other House
Newington NSW 2128
India
Office Email: otherorg@example.com
Office Phone: +61 2 9922 2299

Last Communication Date: 05-Mar-06

Internal Notes:
We had a good time, it was a good successful conversation

Follow Up Notes:
Gotta find out how to add 1 and 3 together...";

			string expectedUrl = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.Communication, salesCall.PK.ToGuid());

			string htmlExpected =
string.Format(@"<HTML><HEAD><TITLE></TITLE></HEAD><BODY><FONT face=""Arial""><strong>Client</strong>: [<a href=""edient:Command=ShowEditForm&ControllerID=Organisation&BusinessEntityPK=479520de-ba34-4339-b9ae-03ed90c5c864&VersionNumber={0}&Hash=%2b7mh4ylQc5igNAh6FqQqdX6pro2teONbq"">SOMEORG1</a>] Some Special Organisation
<strong>Communication ID</strong>: <a href=""{1}"">CM00001074</a>

<strong>Method</strong>: First call to prospective client.
<strong>Staff Coordinator</strong>: Andrew Luong

<strong>Contact</strong>: Zubin Appoo
<strong>Job Category</strong>: Employee (Undefined)
<strong>Email</strong>: zubin.appoo@edi.com.au
<strong>Phone</strong>: +61 2 9025 1180

<strong>Client Address</strong>:
20 Henricks Avenue
Other House
Newington NSW 2128
India
<strong>Office Email</strong>: otherorg@example.com
<strong>Office Phone</strong>: +61 2 9922 2299

<strong>Last Communication Date</strong>: 05-Mar-06

<u><strong>Internal Notes</strong></u>:
We had a good time, it was a good successful conversation

<u><strong>Follow Up Notes</strong></u>:
Gotta find out how to add 1 and 3 together...</FONT></BODY></HTML>", new EnterpriseInformationRetriever().VersionNumber, expectedUrl);

			#endregion

			Reminder reminder = CommunicationReminder.New(salesCall.OQ_NextCall, salesCall.OQ_NextCall, salesCall, new[] { new CommunicationReminderRecipient(ZGuid.Empty, "", "") }, false);
			AssertMultilineASCIIEquals("Calendar reminder with contact address created with correct plain text body", plainTextExpected, reminder.Body);
			AssertMultilineASCIIEquals("Calendar reminder with contact address created with correct html body", htmlExpected, reminder.HtmlBody);
		}

		[TestDate(2023, 12, 23)]
		public void TestNew_WithNotes()
		{
			Env.Registry.SetOrgAllowMixedCase(true);

			#region Test Data

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Test Organisation";
			org.OH_RL_NKClosestPort = "INBOM";
			org.OH_Code = "TSTORG";

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Test Contact";
			contact.OC_Title = "Blah blah";
			contact.OC_JobCategory = "Hello";
			contact.OC_Email = "test@test.com";
			contact.OC_Fax = "+61 2 2345 6789";
			contact.OC_Phone = "+61 2 2345 7891";
			contact.OC_Mobile = "+61 (14) 1234-5678";

			GlbStaff staffCoordinator = Factory.NewWithValidTestData<GlbStaff>();
			staffCoordinator.GS_Code = "TST";
			staffCoordinator.GS_FullName = "Test Staff";

			var orgsalesCall = org.SalesCalls.AddNew();
			orgsalesCall.OQ_CommunicationID = "CM01000001";
			orgsalesCall.OQ_NextCall = new ZDateTime(2023, 12, 24, 12, 0, 0);
			orgsalesCall.OQ_Duration = TimeSpan.FromHours(2);
			orgsalesCall.OQ_TypeOfCall = "MTG";
			orgsalesCall.OQ_CallSummary = "Test call";
			orgsalesCall.OQ_SalesCallNotes = ZBlob.FromUTF8("Test for communication notes");
			orgsalesCall.ClientVisibleNote = "hello world";
			orgsalesCall.OQ_FollowupNotes = ZBlob.FromUTF8("Test for follow up notes");
			orgsalesCall.OQ_OC = contact.PK;
			orgsalesCall.OQ_GS_NKSalesRep = "TST";
			Factory.Save();

			var body = "Test for insert notes";
			var htmlbody = @"<HTML><HEAD><TITLE></TITLE></HEAD><BODY><FONT face=""Arial"">Test for insert notes
</FONT></BODY></HTML>";

			#endregion

			Reminder reminder = CommunicationReminder.New(orgsalesCall.OQ_NextCall, orgsalesCall.OQ_NextCall, orgsalesCall, new[] { new CommunicationReminderRecipient(ZGuid.Empty, "", "") }, body);
			AssertReminderProperties(orgsalesCall, reminder, "Test call", body, htmlbody, new ZDateTime(2023, 12, 24, 12, 0, 0).ToDateTime(), new ZDateTime(2023, 12, 24, 14, 0, 0).ToDateTime());
		}

		public void TestDeliveringReminderYieldsSequentialSequenceNumbers()
		{
			Env.Registry.SetOrgAllowMixedCase(true);

			#region Test Data

			OrgHeader org = Factory.NewWithPrimaryKey<OrgHeader>(new Guid("479520de-ba34-4339-b9ae-03ed90c5c864"));
			org.FillWithValidTestData();
			org.OH_FullName = "Some Special Organisation";
			org.OH_RL_NKClosestPort = "INBOM";
			org.OH_Code = "SOMEORG1";

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Zubin Appoo";
			contact.OC_Title = "Blah blah";
			contact.OC_JobCategory = "Hello";
			contact.OC_Email = "zubin.appoo@edi.com.au";
			contact.OC_Fax = "+61 2 9025 1199";
			contact.OC_Phone = "+61 2 9025 1180";
			contact.OC_Mobile = "+61 (14) 8112-3456";

			GlbStaff staffCoordinator = Factory.New<GlbStaff>();
			staffCoordinator.GS_Code = "ADL";
			staffCoordinator.GS_FullName = "Andrew Luong";

			org.MainAddress.OA_Address1 = "18 Henricks Avenue";
			org.MainAddress.OA_Address2 = "My House";
			org.MainAddress.OA_City = "Newington";
			org.MainAddress.OA_State = "NSW";
			org.MainAddress.OA_PostCode = "2127";
			org.MainAddress.OA_Email = "mainorg@example.com";
			org.MainAddress.OA_Phone = "+61 2 9911 1199";
			org.MainAddress.OA_Fax = "+61 2 9911 9911";

			OrgSalesCall salesCall = org.SalesCalls.AddNew();
			salesCall.OQ_CallDate = new ZDateTime(1981, 2, 24);
			salesCall.OQ_NextCall = new ZDateTime(2006, 3, 10, 9, 0, 0);
			salesCall.OQ_TypeOfCall = "1ST";
			salesCall.OQ_CallSummary = "Really good call";
			salesCall.OQ_SalesCallNotes = ZBlob.FromUTF8("We had a good time, it was a good successful conversation");
			salesCall.OQ_FollowupNotes = ZBlob.FromUTF8("Gotta find out how to add 1 and 3 together...");
			salesCall.OQ_OC = contact.PK;
			salesCall.OQ_GS_NKSalesRep = "ADL";

			#endregion

			salesCall.Logs.AddNew(Events.QuotationAccepted);
			Factory.Save();

			AssertEquals("Making Logs collection subscribed to DataRefreshBus", true, salesCall.Logs.GetAllLogs().IsLoaded);
			AssertEquals("Precondition", false, salesCall.HasChanges);

			Reminder reminder = CommunicationReminder.New(salesCall.OQ_NextCall, salesCall.OQ_NextCall, salesCall, new[] { new CommunicationReminderRecipient(ZGuid.Empty, "name", "test@test.com") }, false);
			reminder.Recipients.Add("me", "me@you.com");

			AssertNull("Precondition: should be no matching log", GetLogForIdentifiers(salesCall.PK));

			// sequence goes up by the number of recipients
			CreateAppointmentAndAssertSequence(reminder, 0u);
			CreateAppointmentAndAssertSequence(reminder, 2u);

			reminder.Recipients.Add("me2", "me2@you.com");
			CreateAppointmentAndAssertSequence(reminder, 4u);
			CreateAppointmentAndAssertSequence(reminder, 7u);

			AssertNotNullOrEmpty(GetLogForIdentifiers(salesCall.PK).SL_Table);
			AssertEquals("Appointment reference logs saved in isolated factory; Parent.HasChanges not affected", false, salesCall.HasChanges);
		}

		public void TestDeliveringReminderYieldsSequentialSequenceNumbers_CreateReminderWithFactory()
		{
			Env.Registry.SetOrgAllowMixedCase(true);

			#region Test Data

			var org = Factory.NewWithPrimaryKey<OrgHeader>(new Guid("479520de-ba34-4339-b9ae-03ed90c5c864"));
			org.FillWithValidTestData();
			org.OH_FullName = "Some Special Organisation";
			org.OH_RL_NKClosestPort = "INBOM";
			org.OH_Code = "SOMEORG1";

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Zubin Appoo";
			contact.OC_Title = "Blah blah";
			contact.OC_JobCategory = "Hello";
			contact.OC_Email = "zubin.appoo@edi.com.au";
			contact.OC_Fax = "+61 2 9025 1199";
			contact.OC_Phone = "+61 2 9025 1180";
			contact.OC_Mobile = "+61 (14) 8112-3456";

			var staffCoordinator = Factory.New<GlbStaff>();
			staffCoordinator.GS_Code = "ADL";
			staffCoordinator.GS_FullName = "Andrew Luong";

			org.MainAddress.OA_Address1 = "18 Henricks Avenue";
			org.MainAddress.OA_Address2 = "My House";
			org.MainAddress.OA_City = "Newington";
			org.MainAddress.OA_State = "NSW";
			org.MainAddress.OA_PostCode = "2127";
			org.MainAddress.OA_Email = "mainorg@example.com";
			org.MainAddress.OA_Phone = "+61 2 9911 1199";
			org.MainAddress.OA_Fax = "+61 2 9911 9911";

			var salesCall = org.SalesCalls.AddNew();
			salesCall.OQ_CallDate = new ZDateTime(1981, 2, 24);
			salesCall.OQ_NextCall = new ZDateTime(2006, 3, 10, 9, 0, 0);
			salesCall.OQ_TypeOfCall = "1ST";
			salesCall.OQ_CallSummary = "Really good call";
			salesCall.OQ_SalesCallNotes = ZBlob.FromUTF8("We had a good time, it was a good successful conversation");
			salesCall.OQ_FollowupNotes = ZBlob.FromUTF8("Gotta find out how to add 1 and 3 together...");
			salesCall.OQ_OC = contact.PK;
			salesCall.OQ_GS_NKSalesRep = "ADL";

			#endregion

			salesCall.Logs.AddNew(Events.QuotationAccepted);

			Assert("Making Logs collection subscribed to DataRefreshBus", salesCall.Logs.GetAllLogs().IsLoaded);

			var reminder = CommunicationReminder.New(salesCall.OQ_NextCall, salesCall.OQ_NextCall, salesCall, new[] { new CommunicationReminderRecipient(ZGuid.Empty, "name", "test@test.com") }, false, Factory);
			reminder.Recipients.Add("me", "me@you.com");
			AssertNull("Precondition: should be no matching log", GetLogForIdentifiers(salesCall.PK));

			// sequence doesn't go up by the number of recipients because log parent was not saved to database
			CreateAppointmentAndAssertSequence(reminder, 0u);
			CreateAppointmentAndAssertSequence(reminder, 0u);

			AssertNotNullOrEmpty(GetLogForIdentifiers(salesCall.PK).SL_Table);
			Assert("Appointment reference logs didn't save in isolated factory; Parent.HasChanges affected", salesCall.HasChanges);

			Factory.Save();
			reminder.Recipients.Add("me2", "me2@you.com");

			// sequence goes up by the number of recipients
			CreateAppointmentAndAssertSequence(reminder, 4u);
			CreateAppointmentAndAssertSequence(reminder, 7u);

			AssertNotNullOrEmpty(GetLogForIdentifiers(salesCall.PK).SL_Table);
			Assert("Appointment reference logs saved in isolated factory; Parent.HasChanges not affected", !salesCall.HasChanges);
		}

		void CreateAppointmentAndAssertSequence(Reminder reminder, uint expectedSequence)
		{
			reminder.CreateAppointment();
			AssertEquals("Sequence", expectedSequence, reminder.Sequence);
		}

		StmALog GetLogForIdentifiers(ZGuid parentPK)
		{
			var filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Reminder.Constants.EventCode);
			filter.AddToFilter(new ZQuery(StmALogSchema.SL_Parent, parentPK));
			filter.AddToFilter(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, Reminder.Constants.ReferencePrefix + CommunicationReminder.Constants.ReminderIdentifier));
			return Factory.LoadTop1<StmALog>(filter);
		}

		public void TestGetInternalSubject()
		{
			Env.Registry.SetOrgAllowMixedCase(true);
			#region Test Data

			OrgHeader org = Factory.NewWithPrimaryKey<OrgHeader>(new Guid("479520de-ba34-4339-b9ae-03ed90c5c864"));
			org.FillWithValidTestData();
			org.OH_FullName = "Some Special Organisation";
			org.OH_RL_NKClosestPort = "INBOM";
			org.OH_Code = "SOMEORG1";

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Zubin Appoo";
			contact.OC_Title = "Blah blah";
			contact.OC_JobCategory = "Hello";
			contact.OC_Email = "zubin.appoo@edi.com.au";
			contact.OC_Fax = "+61 2 9025 1199";
			contact.OC_Phone = "+61 2 9025 1180";
			contact.OC_Mobile = "+61 (14) 8112-3456";

			GlbStaff staffCoordinator = Factory.New<GlbStaff>();
			staffCoordinator.GS_Code = "ADL";
			staffCoordinator.GS_FullName = "Jason.Zhu";

			org.MainAddress.OA_Address1 = "18 Henricks Avenue";
			org.MainAddress.OA_Address2 = "My House";
			org.MainAddress.OA_City = "Newington";
			org.MainAddress.OA_State = "NSW";
			org.MainAddress.OA_PostCode = "2127";
			org.MainAddress.OA_Email = "mainorg@example.com";
			org.MainAddress.OA_Phone = "+61 2 9911 1199";
			org.MainAddress.OA_Fax = "+61 2 9911 9911";

			OrgSalesCall salesCall = org.SalesCalls.AddNew();
			salesCall.OQ_CommunicationID = "CM00001074";
			salesCall.OQ_CallDate = new ZDateTime(1981, 2, 24);
			salesCall.OQ_NextCall = new ZDateTime(2006, 3, 10, 9, 0, 0);
			salesCall.OQ_TypeOfCall = "1ST";
			salesCall.OQ_CallSummary = "Really good call";
			salesCall.OQ_SalesCallNotes = ZBlob.FromUTF8("We had a good time, it was a good successful conversation");
			salesCall.ClientVisibleNote = "hello world";
			salesCall.OQ_FollowupNotes = ZBlob.FromUTF8("Gotta find out how to add 1 and 3 together...");
			salesCall.OQ_OC = contact.PK;
			salesCall.OQ_GS_NKSalesRep = "ADL";
			salesCall.OQ_Category = "C1";
			Factory.Save();
			#endregion

			var template = new NotificationEmailTemplate()
			{
				EmailSubject = "Communication [(*MethodCode*) - (*Method*)] (*OrganizationCode*) - (*CommunicationSubject*) - (*PurposeCode*) - (*OrganizationFullName*) - (*PrimaryContact*) - (*StaffCoordinatorCode*) - (*StaffCoordinator*)",
				EmailBody = "",
				DocSourceType = ObjectFactory.GetType<DocumentWrappers.IDocSalesCall>()
			};
			OrganisationsDataRegistry.Instance.CommunicationInternalCalendarReminderSubjectTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, template);

			Reminder reminder = CommunicationReminder.New(salesCall.OQ_NextCall, ZDateTime.Empty, salesCall, new[] { new CommunicationReminderRecipient(ZGuid.Empty, "", "") }, false);
			string expectedSubject = "Communication [1ST - First call to prospective client.] SOMEORG1 - Really good call - C1 - Some Special Organisation - Zubin Appoo - Adl - Jason.Zhu";
			AssertEquals("Correct Subject", expectedSubject, reminder.Subject);
		}

		IDisposable instanceDetailsDisposable;

		protected override void SetUp()
		{
			base.SetUp();
			instanceDetailsDisposable = InstanceDetails.SetUpCurrentForTest();
		}

		protected override void TearDown()
		{
			instanceDetailsDisposable?.Dispose();
			base.TearDown();
		}
	}
}
