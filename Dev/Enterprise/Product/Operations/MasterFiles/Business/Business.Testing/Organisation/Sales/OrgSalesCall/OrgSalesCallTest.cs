using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Services.Calendar;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSalesCall))]
	sealed class OrgSalesCallTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		[TestedType(typeof(OrgSalesCall))]
		class CustomFieldsTest : TestICustomFieldProvider
		{
		}

		public void TestUniversalCopyAttributes()
		{
			AssertEquals(true, typeof(OrgSalesCall).IsDefined(typeof(UniversalCopyWithExtendedEntitiesAttribute), false));
			var copyAttributes = typeof(OrgSalesCall).GetProperty("OQ_OH").GetCustomAttributes(typeof(UniversalCopyAlwaysCopyPropertyAttribute), false);
			AssertEquals(1, copyAttributes.Length);
			AssertEquals(UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning, ((UniversalCopyAlwaysCopyPropertyAttribute)copyAttributes[0]).Mode);
		}

		#region IJobNumber

		public void TestJobNumber()
		{
			var communication = Factory.New<OrgSalesCall>();
			communication.OQ_CommunicationID = "CM00001234";
			AssertEquals("CM00001234", ((IJobNumber)communication).JobNumber);
		}

		#endregion

		#region Calendar Reminders

		public void TestPropertyInfosThatTriggerAutoSend()
		{
			var communication = Factory.New<OrgSalesCall>();
			AssertContainsExactElementsInAnyOrder(info => info.Name,
				new[] {
					communication.OQ_OHInfo,
					communication.OQ_OCInfo,
					communication.OQ_NextCallInfo,
					communication.OQ_DurationInfo,
					communication.OQ_OA_LocationAddressInfo,
					communication.OQ_GS_NKLocationResourceInfo,
					communication.OQ_LocationTextInfo,
					communication.OQ_GS_NKSalesRepInfo
				},
				communication.PropertyInfosThatTriggerAutoSend);
		}

		[TestDateIncremental(0, 0, 0, 1)]
		public void TestSendCalendarReminder()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var salesCall = Factory.New<OrgSalesCallForTesting>();
			salesCall.OQ_OH = org.PK;

			var salesRep = Factory.NewWithValidTestData<GlbStaff>();
			salesRep.GS_EmailAddress = "sales.rep@test.com";
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "staff.1@test.com";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_EmailAddress = "staff.2@test.com";
			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "contact.1@test.com";

			Env.Security.CommunicationManagerAllowSendInvitation.IsAllowed = false;
			var sendResult = salesCall.SendCalendarReminder();
			AssertEquals(false, sendResult.Sent);
			AssertEquals(Env.Security.CommunicationManagerAllowSendInvitation.ErrorMessageForNotAllowed, sendResult.Message);

			Env.Security.CommunicationManagerAllowSendInvitation.IsAllowed = true;
			sendResult = salesCall.SendCalendarReminder();
			AssertEquals(false, sendResult.Sent);
			AssertEquals("Please save changes before sending invitation.", sendResult.Message);

			Factory.Save();
			sendResult = salesCall.SendCalendarReminder();
			AssertEquals(false, sendResult.Sent);
			AssertEquals("Cannot send invitation because scheduled date is blank.", sendResult.Message);

			salesCall.OQ_NextCall = new ZDateTime(2013, 1, 1);
			salesCall.OQ_CallDate = new ZDateTime(2013, 1, 1);
			Factory.Save();
			sendResult = salesCall.SendCalendarReminder();
			AssertEquals(false, sendResult.Sent);
			AssertEquals("Cannot send invitation because there is already an actual date.", sendResult.Message);

			salesCall.OQ_CallDate = ZDateTime.Empty;
			Factory.Save();
			sendResult = salesCall.SendCalendarReminder();
			AssertEquals(false, sendResult.Sent);
			AssertEquals("Cannot send invitation because there is no recipient or all recipients have no email addresses.", sendResult.Message);

			salesCall.OQ_GS_NKSalesRep = salesRep.GS_Code;
			var staffAttendee1 = salesCall.AdditionalAttendeesStaff.AddNew();
			staffAttendee1.O6_AttendeeID = staff1.PK;
			staffAttendee1.O6_ReceiverReminder = true;
			var staffAttendee2 = salesCall.AdditionalAttendeesStaff.AddNew();
			staffAttendee2.O6_AttendeeID = staff2.PK;
			staffAttendee2.O6_ReceiverReminder = false;
			var contactAttendee = salesCall.AdditionalAttendeesContact.AddNew();
			contactAttendee.O6_AttendeeID = contact1.PK;
			contactAttendee.O6_ReceiverReminder = true;

			Factory.Save();
			sendResult = salesCall.SendCalendarReminder();
			AssertEquals(true, sendResult.Sent);
			AssertEquals(ReminderType.Confirmed, sendResult.Reminder.ReminderType);
			AssertContainsExactElementsInAnyOrder(
				new[] { "sales.rep@test.com", "contact.1@test.com", "staff.1@test.com" },
				sendResult.Reminder.Recipients.Select(recipient => recipient.Email));

			Factory.Save();
			sendResult = salesCall.SendCalendarReminder();
			AssertEquals(false, sendResult.Sent);
			AssertEquals("Cannot send invitation because communication details have not changed.", sendResult.Message);

			salesCall.OQ_NextCall = new ZDateTime(2013, 1, 2);
			Factory.Save();
			sendResult = salesCall.SendCalendarReminder();
			AssertEquals(true, sendResult.Sent);
			AssertEquals("Should allow resend reminder because communication has been edited", ReminderType.Confirmed, sendResult.Reminder.ReminderType);
			AssertContainsExactElementsInAnyOrder(
				new[] { "sales.rep@test.com", "contact.1@test.com", "staff.1@test.com" },
				salesCall.LastReminderSent.Recipients.Select(recipient => recipient.Email));
		}

		public void TestSendCalendarReminder_Auto()
		{
			OrganisationsDataRegistry.Instance.CommunicationAutoSendUponSave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var salesCall = Factory.New<OrgSalesCallForTesting>();
			salesCall.OQ_OH = org.PK;

			var salesRep = Factory.NewWithValidTestData<GlbStaff>();
			salesRep.GS_EmailAddress = "sales.rep@test.com";
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "staff.1@test.com";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_EmailAddress = "staff.2@test.com";
			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "contact.1@test.com";

			Env.Security.CommunicationManagerAllowSendInvitation.IsAllowed = false;
			Factory.Save();
			AssertNull(salesCall.LastReminderSent);

			Env.Security.CommunicationManagerAllowSendInvitation.IsAllowed = true;
			Factory.Save();
			AssertNull("Cannot send invitation because scheduled date is blank.", salesCall.LastReminderSent);

			salesCall.OQ_NextCall = new ZDateTime(2013, 1, 1);
			salesCall.OQ_CallDate = new ZDateTime(2013, 1, 1);
			Factory.Save();
			AssertNull("Cannot send invitation because there is already an actual date.", salesCall.LastReminderSent);

			salesCall.OQ_CallDate = ZDateTime.Empty;
			Factory.Save();
			AssertNull("Cannot send invitation because there is no recipient or all recipients have no email addresses.", salesCall.LastReminderSent);

			salesCall.OQ_GS_NKSalesRep = salesRep.GS_Code;
			var staffAttendee1 = salesCall.AdditionalAttendeesStaff.AddNew();
			staffAttendee1.O6_AttendeeID = staff1.PK;
			staffAttendee1.O6_ReceiverReminder = true;
			var staffAttendee2 = salesCall.AdditionalAttendeesStaff.AddNew();
			staffAttendee2.O6_AttendeeID = staff2.PK;
			staffAttendee2.O6_ReceiverReminder = false;
			var contactAttendee = salesCall.AdditionalAttendeesContact.AddNew();
			contactAttendee.O6_AttendeeID = contact1.PK;
			contactAttendee.O6_ReceiverReminder = true;

			salesCall.LastReminderSent = null;
			Factory.Save();
			AssertEquals(ReminderType.Confirmed, salesCall.LastReminderSent.ReminderType);
			AssertContainsExactElementsInAnyOrder(
				new[] { "sales.rep@test.com", "contact.1@test.com", "staff.1@test.com" },
				salesCall.LastReminderSent.Recipients.Select(recipient => recipient.Email));

			staffAttendee2.O6_ReceiverReminder = true;
			salesCall.LastReminderSent = null;
			Factory.Save();
			AssertEquals("Should send reminder to recipients who haven't been sent previously", ReminderType.Confirmed, salesCall.LastReminderSent.ReminderType);
			AssertContainsExactElementsInAnyOrder(
				new[] { "staff.2@test.com" },
				salesCall.LastReminderSent.Recipients.Select(recipient => recipient.Email));

			salesCall.OQ_NextCall = new ZDateTime(2013, 1, 2);
			salesCall.LastReminderSent = null;
			Factory.Save();
			AssertEquals("Should resend reminder to all recipients when scheduled date changes", ReminderType.Confirmed, salesCall.LastReminderSent.ReminderType);
			AssertContainsExactElementsInAnyOrder(
				new[] { "sales.rep@test.com", "contact.1@test.com", "staff.1@test.com", "staff.2@test.com" },
				salesCall.LastReminderSent.Recipients.Select(recipient => recipient.Email));

			contactAttendee.O6_ReceiverReminder = false;
			salesCall.LastReminderSent = null;
			Factory.Save();
			AssertEquals("Should send cancellation when 'Receive Reminder' disabled", ReminderType.Cancellation, salesCall.LastReminderSent.ReminderType);
			AssertContainsExactElementsInAnyOrder(
				new[] { "contact.1@test.com" },
				salesCall.LastReminderSent.Recipients.Select(recipient => recipient.Email));

			salesCall.OQ_GS_NKSalesRep = ZString.Empty;
			salesCall.AdditionalAttendeesStaff.RemoveAndDeleteAll();
			salesCall.LastReminderSent = null;
			Factory.Save();
			AssertEquals("Should send cancellation when recipient removed", ReminderType.Cancellation, salesCall.LastReminderSent.ReminderType);
			AssertContainsExactElementsInAnyOrder(
				new[] { "sales.rep@test.com", "staff.1@test.com", "staff.2@test.com" },
				salesCall.LastReminderSent.Recipients.Select(recipient => recipient.Email));
		}

		[TestDate(2013, 1, 1, 1, 1, 1)]
		public void TestChangingStaffCoordinator_WhenAutoSendIsFalse_ShouldCancelReminderForAllRecipients()
		{
			OrganisationsDataRegistry.Instance.CommunicationAutoSendUponSave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var salesCall = Factory.New<OrgSalesCallForTesting>();
			salesCall.OQ_OH = org.PK;
			salesCall.OQ_NextCall = new ZDateTime(2022, 2, 2);

			var salesRep = Factory.NewWithValidTestData<GlbStaff>();
			salesRep.GS_EmailAddress = "sales.rep@test.com";
			var anotherSalesRep = Factory.NewWithValidTestData<GlbStaff>();
			anotherSalesRep.GS_EmailAddress = "another.sales.rep@test.com";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "staff.1@test.com";
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "contact.1@test.com";

			salesCall.OQ_GS_NKSalesRep = salesRep.GS_Code;

			var staffAttendee = salesCall.AdditionalAttendeesStaff.AddNew();
			staffAttendee.O6_AttendeeID = staff.PK;
			staffAttendee.O6_AttendeeTableCode = staff.TablePrefix;
			staffAttendee.O6_ReceiverReminder = true;

			var contactAttendee = salesCall.AdditionalAttendeesContact.AddNew();
			contactAttendee.O6_AttendeeID = contact.PK;
			contactAttendee.O6_AttendeeTableCode = contact.TablePrefix;
			contactAttendee.O6_ReceiverReminder = true;

			Factory.Save();

			salesCall.SendCalendarReminder();
			AssertEquals("Precondition", ReminderType.Confirmed, salesCall.LastReminderSent.ReminderType);
			AssertContainsExactElementsInAnyOrder("Precondition",
				new[] { "sales.rep@test.com", "staff.1@test.com", "contact.1@test.com" },
				salesCall.LastReminderSent.Recipients.Select(recipient => recipient.Email));
			AssertEquals("Precondition", "Invitation sent: 01-Jan-13 01:01", salesCall.LastInvitationActionDescription);

			salesCall.LastReminderSent = null;
			TestDateAttribute.Date = new DateTime(2013, 2, 2, 2, 2, 2);
			salesCall.OQ_GS_NKSalesRep = anotherSalesRep.GS_Code;
			Factory.Save();
			AssertEquals(ReminderType.Cancellation, salesCall.LastReminderSent.ReminderType);
			AssertContainsExactElementsInAnyOrder(
				new[] { "sales.rep@test.com", "staff.1@test.com", "contact.1@test.com" },
				salesCall.LastReminderSent.Recipients.Select(recipient => recipient.Email));
			AssertEquals("Invitation canceled: 02-Feb-13 02:02", salesCall.LastInvitationActionDescription);
		}

		[TestDate(2013, 1, 1, 1, 1, 1)]
		public void TestChangingStaffCoordinator_WhenAutoSendIsTrue_ShouldCancelReminderForRecipientsThatHaveBeenRemoved_AndResendToAllCurrent()
		{
			OrganisationsDataRegistry.Instance.CommunicationAutoSendUponSave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var salesCall = Factory.New<OrgSalesCallForTesting>();
			salesCall.OQ_OH = org.PK;
			salesCall.OQ_NextCall = new ZDateTime(2022, 2, 2);

			var salesRep = Factory.NewWithValidTestData<GlbStaff>();
			salesRep.GS_EmailAddress = "sales.rep@test.com";
			var anotherSalesRep = Factory.NewWithValidTestData<GlbStaff>();
			anotherSalesRep.GS_EmailAddress = "another.sales.rep@test.com";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "staff.1@test.com";
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "contact.1@test.com";

			salesCall.OQ_GS_NKSalesRep = salesRep.GS_Code;

			var staffAttendee = salesCall.AdditionalAttendeesStaff.AddNew();
			staffAttendee.O6_AttendeeID = staff.PK;
			staffAttendee.O6_AttendeeTableCode = staff.TablePrefix;
			staffAttendee.O6_ReceiverReminder = true;

			var contactAttendee = salesCall.AdditionalAttendeesContact.AddNew();
			contactAttendee.O6_AttendeeID = contact.PK;
			contactAttendee.O6_AttendeeTableCode = contact.TablePrefix;
			contactAttendee.O6_ReceiverReminder = true;

			Factory.Save();

			salesCall.SendCalendarReminder();
			AssertEquals("Precondition", ReminderType.Confirmed, salesCall.LastReminderSent.ReminderType);
			AssertContainsExactElementsInAnyOrder("Precondition",
				new[] { "sales.rep@test.com", "staff.1@test.com", "contact.1@test.com" },
				salesCall.LastReminderSent.Recipients.Select(recipient => recipient.Email));
			AssertEquals("Precondition", "Invitation sent: 01-Jan-13 01:01", salesCall.LastInvitationActionDescription);

			salesCall.LastReminderSent = null;
			salesCall.RemindersSent.Clear();
			TestDateAttribute.Date = new DateTime(2013, 2, 2, 2, 2, 2);
			salesCall.OQ_GS_NKSalesRep = anotherSalesRep.GS_Code;
			Factory.Save();

			var cancellationReminder = salesCall.RemindersSent[0];
			AssertEquals("Should have sent cancellation only to old sales rep", ReminderType.Cancellation, cancellationReminder.ReminderType);
			AssertContainsExactElementsInAnyOrder(
				new[] { "sales.rep@test.com" },
				cancellationReminder.Recipients.Select(recipient => recipient.Email));

			var confirmReminder = salesCall.RemindersSent[1];
			AssertEquals(ReminderType.Confirmed, confirmReminder.ReminderType);
			AssertContainsExactElementsInAnyOrder(
				new[] { "another.sales.rep@test.com", "staff.1@test.com", "contact.1@test.com" },
				confirmReminder.Recipients.Select(recipient => recipient.Email));

			AssertEquals("Should only be 2 reminders sent", 2, salesCall.RemindersSent.Count);

			AssertEquals("Invitation sent: 02-Feb-13 02:02", salesCall.LastInvitationActionDescription);
		}

		public void TestCalendarReminder_Disabled()
		{
			OrganisationsDataRegistry.Instance.CommunicationAutoSendUponSave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			GlbStaff salesRep = Factory.NewWithValidTestData<GlbStaff>();
			salesRep.GS_EmailAddress = "andrew.luong@cargowise.com";

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Some Special Organisation";
			org.OH_RL_NKClosestPort = "INBOM";
			org.OH_Code = "SOMEORG1";

			OrgSalesCallForTesting salesCall = Factory.New<OrgSalesCallForTesting>();
			salesCall.OQ_NextCall = new ZDateTime(2006, 3, 10, 9, 0, 0);
			salesCall.OQ_TypeOfCall = "1ST";
			salesCall.OQ_CallSummary = "Really good call";
			salesCall.OQ_SalesCallNotes = ZBlob.FromUTF8("We had a good time, it was a good successful conversation");
			salesCall.OQ_OH = org.PK;
			salesCall.OQ_GS_NKSalesRep = salesRep.GS_Code;

			OrganisationsDataRegistry.Instance.CommunicationAutoSendUponSave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			salesCall.ShouldSendInvitation = false;
			Factory.Save();
			AssertEquals("Calendar reminder NOT created", null, salesCall.LastReminderSent);

			salesCall.OQ_NextCall = new ZDateTime(2013, 3, 10, 9, 0, 0);
			salesCall.ShouldSendInvitation = true;
			Factory.Save();
			AssertNotNull("Calendar reminder WILL be created", salesCall.LastReminderSent);
		}

		public void TestCancelCalendarReminder()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Organisation DDD";
			org.OH_Code = "DDDORG1";

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "test@example.com";
			staff.GS_FullName = "sam";

			OrgSalesCall salesCall = org.SalesCalls.AddNew();
			salesCall.OQ_NextCall = new ZDateTime(2012, 12, 5, 10, 0, 0);
			salesCall.OQ_TypeOfCall = "1ST";
			salesCall.OQ_CallSummary = "Really good call";
			salesCall.OQ_GS_NKSalesRep = staff.GS_Code;

			var sendResult = salesCall.CancelCalendarReminder();
			AssertEquals(false, sendResult.Sent);
			AssertEquals("Please save changes before canceling invitation.", sendResult.Message);

			Factory.Save();

			sendResult = salesCall.CancelCalendarReminder();
			AssertEquals(false, sendResult.Sent);
			AssertEquals("Cannot cancel invitation because it has not been sent yet.", sendResult.Message);

			salesCall.SendCalendarReminder();
			Assert("Precondition: Invitation Sent", salesCall.IsInvitationSent);

			Factory.Save();
			sendResult = salesCall.CancelCalendarReminder();
			AssertEquals(true, sendResult.Sent);
			AssertEquals(ReminderType.Cancellation, sendResult.Reminder.ReminderType);
		}

		public void TestCancelCalendarReminderAutoSend()
		{
			OrganisationsDataRegistry.Instance.CommunicationAutoSendUponSave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Organisation DDD";
			org.OH_Code = "DDDORG1";

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "test@example.com";
			staff.GS_FullName = "sam";

			OrgSalesCall salesCall = org.SalesCalls.AddNew();
			salesCall.OQ_NextCall = new ZDateTime(2012, 12, 5, 10, 0, 0);
			salesCall.OQ_TypeOfCall = "1ST";
			salesCall.OQ_CallSummary = "Really good call";
			salesCall.OQ_GS_NKSalesRep = staff.GS_Code;

			var sendResult = salesCall.CancelCalendarReminder();
			AssertEquals(false, sendResult.Sent);
			AssertEquals("Please save changes before canceling invitation.", sendResult.Message);

			Factory.Save();

			salesCall.SendCalendarReminder();
			Assert("Precondition: Invitation Sent", salesCall.IsInvitationSent);

			Factory.Save();
			sendResult = salesCall.CancelCalendarReminder();
			AssertEquals(true, sendResult.Sent);
			AssertEquals(ReminderType.Cancellation, sendResult.Reminder.ReminderType);

			var filterCancelledLogQuery = new ZQuery();
			filterCancelledLogQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Reminder.Constants.EventCode);
			filterCancelledLogQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "Type:" + CommunicationReminder.Constants.TypeCancelCode);
			AssertEquals("Should have 1 cancel log", 1, Factory.Load<StmALog>(filterCancelledLogQuery).Length);

			var filterRequestedLogQuery = new ZQuery();
			filterRequestedLogQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Reminder.Constants.EventCode);
			filterRequestedLogQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "Type:" + CommunicationReminder.Constants.TypeRequestCode);
			AssertEquals("Should have 1 request log", 1, Factory.Load<StmALog>(filterRequestedLogQuery).Length);
		}

		#endregion

		#region Associated Trade Lanes

		OrgSales CreateNewOrgSales(ZString originCode, ZString destinationCode)
		{
			var origin = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, originCode);
			var destination = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, destinationCode);
			OrgSales tradeLane = Organisation.SalesCollection.AddNew();
			if (origin != null)
			{
				tradeLane.OW_OriginID = origin.PK;
			}
			if (destination != null)
			{
				tradeLane.OW_DestinationID = destination.PK;
			}

			return tradeLane;
		}

		public void TestAssociatedTradeLanesPivots()
		{
			OrgSalesCall salesCall2 = Organisation.SalesCalls.AddNew();
			salesCall2.FillWithValidTestData();

			AssertEquals("No TradeLanes on Header", 0, Organisation.SalesCollection.Count);
			OrgSales tradeLane1 = CreateNewOrgSales("USLAX", "AUSYD");
			OrgSales tradeLane2 = CreateNewOrgSales("PAACU", "GAAKE");
			OrgSales tradeLane3 = CreateNewOrgSales("IDABU", "OMBYB");
			OrgSales tradeLane4 = CreateNewOrgSales("LAAOU", "SAAAK");
			AssertEquals("TradeLanes added to Header", 4, Organisation.SalesCollection.Count);

			AssertEquals("There should be no tradelanes for SalesCall", 0, SalesCall.AssociatedTradeLanesPivots.Count);
			AssertEquals("There should be no tradelanes for SalesCall2", 0, salesCall2.AssociatedTradeLanesPivots.Count);
			SalesCall.AssociatedTradeLanesPivots.AddPivotFor(tradeLane1);
			SalesCall.AssociatedTradeLanesPivots.AddPivotFor(tradeLane2);
			salesCall2.AssociatedTradeLanesPivots.AddPivotFor(tradeLane3);
			salesCall2.AssociatedTradeLanesPivots.AddPivotFor(tradeLane4);

			AssertEquals("There should be 2 tradelanes for SalesCall", 2, SalesCall.AssociatedTradeLanesPivots.Count);
			AssertEquals("There should be 2 tradelanes for SalesCall2", 2, salesCall2.AssociatedTradeLanesPivots.Count);

			SalesCall.AssociatedTradeLanesPivots.DeletePivotFor(tradeLane1);
			Assert("There should be 1 tradelane for SalesCall", SalesCall.AssociatedTradeLanesPivots.Contains(tradeLane2));
			Assert("Organisation still contains TradeLane1", Organisation.SalesCollection.Contains(tradeLane1));

			SalesCall.AssociatedTradeLanesPivots.DeletePivotFor(tradeLane2);
			AssertEquals("There should be 0 tradelanes for SalesCall", 0, SalesCall.AssociatedTradeLanesPivots.Count);
			Assert("Organisation still contains TradeLane2", Organisation.SalesCollection.Contains(tradeLane2));
		}

		public void TestDeletingLinksDoesNotRemoveTheTradeLane()
		{
			AssertEquals("No TradeLanes on Header", 0, Organisation.SalesCollection.Count);
			OrgSales tradeLane1 = CreateNewOrgSales("USLAX", "AUSYD");
			AssertEquals("TradeLanes added to Header", 1, Organisation.SalesCollection.Count);

			AssertEquals("There should be no tradelanes for SalesCall", 0, SalesCall.AssociatedTradeLanesPivots.Count);
			SalesCall.AssociatedTradeLanesPivots.AddPivotFor(tradeLane1);
			AssertEquals("There should be 1 tradelane for SalesCall", 1, SalesCall.AssociatedTradeLanesPivots.Count);

			Factory.Save();

			SalesCall.Delete();
			Assert("Organisation should still contain TradeLane1", Organisation.SalesCollection.Contains(tradeLane1));
		}

		#endregion

		#region Properties

		public void TestOQ_CallDate()
		{
			Organisation.MiscServ.OM_CMLastCallDate = ZDateTime.Now.AddDays(-2);
			SalesCall.OQ_CallDate = ZDateTime.UtcNow;
			Factory.Save();
			AssertEquals("Call Date", SalesCall.OQ_CallDate, Organisation.MiscServ.OM_CMLastCallDate);

			OrgSalesCall salesCall2 = Organisation.SalesCalls.AddNew();
			salesCall2.OQ_CallDate = ZDateTime.UtcNow.AddDays(-5);
			Factory.Save();
			AssertEquals("Date Last Call not updated - as new calls is in the past", SalesCall.OQ_CallDate, Organisation.MiscServ.OM_CMLastCallDate);

			salesCall2.OQ_CallDate = ZDateTime.UtcNow.AddDays(55);
			Factory.Save();
			AssertEquals("Date Last Call IS updated - as new calls is more recent", salesCall2.OQ_CallDate, Organisation.MiscServ.OM_CMLastCallDate);

			AssertNotEquals(ZDate.Empty, Organisation.MiscServ.OM_CMLastCallDate);
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestOQ_CallDateLocal()
		{
			var communication = Factory.New<OrgSalesCall>();
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.SalesCalls.Add(communication);
			communication.OQ_CallDate = new ZDateTime(2013, 10, 13, 13, 0, 0);
			Factory.Save();
			AssertEquals(new ZDateTime(2013, 10, 13, 23, 0, 0), communication.OQ_CallDateLocal);
			AssertEquals(communication.OQ_CallDateLocal, header.MiscServ.OM_CMLastCallDateLocal);

			communication.OQ_CallDateLocal = new ZDateTime(2013, 10, 13, 13, 0, 0);
			Factory.Save();
			AssertEquals(new ZDateTime(2013, 10, 13, 3, 0, 0), communication.OQ_CallDate);
			AssertEquals(communication.OQ_CallDateLocal, header.MiscServ.OM_CMLastCallDateLocal);
		}

		public void TestOQ_NextCall()
		{
			SalesCall.OQ_NextCall = ZDateTime.UtcNow.AddDays(1);
			Factory.Save();
			AssertEquals("Date Next Call", SalesCall.OQ_NextCall, Organisation.MiscServ.FollowUpDate);

			OrgSalesCall salesCall2 = Organisation.SalesCalls.AddNew();
			salesCall2.OQ_NextCall = ZDateTime.UtcNow.AddDays(-5);
			OrgSalesCall salesCall3 = Organisation.SalesCalls.AddNew();
			salesCall3.OQ_NextCall = ZDateTime.UtcNow.AddDays(-3);
			Factory.Save();
			AssertEquals("Date Next Call not updated - as new calls is in the past", SalesCall.OQ_NextCall, Organisation.MiscServ.FollowUpDate);

			SalesCall.OQ_NextCall = ZDateTime.UtcNow.AddDays(30);
			salesCall2.OQ_NextCall = ZDateTime.UtcNow.AddDays(5);
			Factory.Save();
			AssertEquals("Date Next Call IS updated - as new calls is more recent", salesCall2.OQ_NextCall, Organisation.MiscServ.FollowUpDate);

			salesCall3.OQ_NextCall = ZDateTime.UtcNow.AddDays(3);
			Factory.Save();
			AssertEquals("Date Next Call IS updated - as sales call 3 is more recent", salesCall3.OQ_NextCall, Organisation.MiscServ.FollowUpDate);

			AssertNotEquals(ZDate.Empty, Organisation.MiscServ.FollowUpDate);
		}

		[TestDate(2013, 10, 10), TestUtcOffset(10, 0, 0)]
		public void TestOQ_NextCallLocal()
		{
			var communication = Factory.New<OrgSalesCall>();
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.SalesCalls.Add(communication);
			communication.OQ_NextCall = new ZDateTime(2013, 10, 13, 13, 0, 0);
			Factory.Save();
			AssertEquals(new ZDateTime(2013, 10, 13, 23, 0, 0), communication.OQ_NextCallLocal);
			AssertEquals(communication.OQ_NextCallLocal, header.MiscServ.FollowUpDateLocal);

			communication.OQ_NextCallLocal = new ZDateTime(2013, 10, 13, 13, 0, 0);
			Factory.Save();
			AssertEquals(new ZDateTime(2013, 10, 13, 3, 0, 0), communication.OQ_NextCall);
			AssertEquals(communication.OQ_NextCallLocal, header.MiscServ.FollowUpDateLocal);
		}

		public void TestDateLocal()
		{
			var salesCall = Factory.New<OrgSalesCall>();
			AssertEquals(ZDateTime.Empty, salesCall.DateLocal);

			salesCall.OQ_NextCallLocal = new ZDateTime(2013, 7, 25, 14, 0, 0);
			AssertEquals(new ZDateTime(2013, 7, 25, 14, 0, 0), salesCall.DateLocal);

			salesCall.OQ_CallDateLocal = new ZDateTime(2013, 7, 31, 12, 0, 0);
			AssertEquals(new ZDateTime(2013, 7, 31, 12, 0, 0), salesCall.DateLocal);
		}

		[TestDate(2013, 7, 25)]
		public void TestDefaultDuration()
		{
			var salesCall = Factory.New<OrgSalesCall>();
			AssertEquals((ZDateTime)TimeSpan.FromMinutes(30), salesCall.OQ_Duration);
		}

		public void TestCommunicationID()
		{
			var salesCall = Factory.New<OrgSalesCall>();
			AssertEquals("", salesCall.OQ_CommunicationID);

			salesCall.FillWithValidTestData();
			Factory.Save();
			AssertNotEquals("", salesCall.OQ_CommunicationID);
		}

		[TestDate(2003, 1, 1)]
		public void TestDefaultFollowUpNotes()
		{
			Organisation.SalesCalls.DeleteAll();

			OrgSalesCall salesCallInitial = Organisation.SalesCalls.AddNew();
			salesCallInitial.OQ_CallDate = ZDateTime.Now.AddDays(-10);
			AssertEquals("New calls call notes blank as no previous follow up notes exist.", ZBlob.Empty, salesCallInitial.OQ_SalesCallNotes);

			OrgSalesCall salesCallVeryOld = Organisation.SalesCalls.AddNew();
			salesCallVeryOld.OQ_CallDate = ZDateTime.Now.AddDays(-5);
			salesCallVeryOld.OQ_FollowupNotes = ZBlob.FromAscii("These are my follow up notes.");

			OrgSalesCall salesCallOld = Organisation.SalesCalls.AddNew();
			salesCallOld.OQ_CallDate = ZDateTime.Now.AddDays(-2);
			salesCallOld.OQ_FollowupNotes = ZBlob.FromAscii("These are some more follow up notes.");

			OrgSalesCall salesCallNew = Organisation.SalesCalls.AddNew();
			AssertEquals("New calls call notes = previous call follow up notes", salesCallOld.OQ_FollowupNotes, salesCallNew.OQ_SalesCallNotes);

			OrgSalesCall salesCall1 = Organisation.SalesCalls.AddNew();
			salesCall1.OQ_CallDate = new ZDateTime(2004, 5, 28, 9, 30, 5);
			salesCall1.OQ_FollowupNotes = ZBlob.FromAscii("Older by a second");

			OrgSalesCall salesCall2 = Organisation.SalesCalls.AddNew();
			salesCall2.OQ_CallDate = new ZDateTime(2004, 5, 28, 9, 30, 6);
			salesCall2.OQ_FollowupNotes = ZBlob.FromAscii("This is the right one that should be picked up.");

			OrgSalesCall salesCall3 = Organisation.SalesCalls.AddNew();
			AssertEquals("New calls call notes = previous call follow up notes", salesCall2.OQ_FollowupNotes, salesCall3.OQ_SalesCallNotes);
		}

		public void TestClientVisibleNote()
		{
			var communication = Factory.New<OrgSalesCall>();
			communication.ClientVisibleNote = "hello from the other side";
			AssertEquals("hello from the other side", communication.ClientVisibleNote);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedCommunication = newFactory.Load<OrgSalesCall>(communication.PK);
			AssertEquals("hello from the other side", reloadedCommunication.ClientVisibleNote);
		}

		public void TestLogMessage()
		{
			SalesCall.OQ_CallDate = Env.Time.CurrentUtcDateTime;

			Factory.Save();
			AssertEquals("Autolog event reference description", "Call Date " + SalesCall.OQ_CallDate.ToString(), SalesCall.Logs.AutoCreatedLog.SL_Reference);
		}

		public void TestSalesCallsAreDefaultedToCurrentUser()
		{
			AssertEquals("Sales Call defaults to current user", GlbStaff.CurrentUser.GS_Code, SalesCall.OQ_GS_NKSalesRep);
		}

		#region Contact

		public void TestContactName()
		{
			AssertEquals("", SalesCall.ContactName);

			var contact = Organisation.Contacts.AddNew();
			contact.OC_ContactName = "Andrew";
			SalesCall.OQ_OC = contact.PK;
			AssertEquals("Andrew", SalesCall.ContactName);

			var inquiry = Factory.New<SalesEnquiry>();
			inquiry.O1_ContactName = "Samuel";
			SalesCall.LinkedInquiry = inquiry;
			AssertEquals("Samuel", SalesCall.ContactName);
		}

		public void TestContactWorkPhone()
		{
			AssertEquals("", SalesCall.ContactWorkPhone);

			Organisation.Addresses[0].OA_Phone = "02 8888 8888";
			AssertEquals("", SalesCall.ContactWorkPhone);

			var contact = Organisation.Contacts.AddNew();
			SalesCall.OQ_OC = contact.PK;
			AssertEquals("02 8888 8888", SalesCall.ContactWorkPhone);

			contact.OC_Phone = "02 1234 5678";
			AssertEquals("02 1234 5678", SalesCall.ContactWorkPhone);

			var inquiry = Factory.New<SalesEnquiry>();
			inquiry.O1_Phone = "02 9876 5432";
			SalesCall.LinkedInquiry = inquiry;
			AssertEquals("02 9876 5432", SalesCall.ContactWorkPhone);

			var anotherSalesCall = Factory.New<OrgSalesCall>();
			AssertEquals("", anotherSalesCall.ContactWorkPhone);
		}

		public void TestContactMobile()
		{
			AssertEquals("", SalesCall.ContactMobile);

			var contact = Organisation.Contacts.AddNew();
			contact.OC_Mobile = "04 1234 5678";
			SalesCall.OQ_OC = contact.PK;
			AssertEquals("04 1234 5678", SalesCall.ContactMobile);

			var inquiry = Factory.New<SalesEnquiry>();
			inquiry.O1_Mobile = "04 9876 5432";
			SalesCall.LinkedInquiry = inquiry;
			AssertEquals("04 9876 5432", SalesCall.ContactMobile);
		}

		public void TestContactFax()
		{
			AssertEquals("", SalesCall.ContactFax);

			var contact = Organisation.Contacts.AddNew();
			contact.OC_Fax = "03 1234 5678";
			SalesCall.OQ_OC = contact.PK;
			AssertEquals("03 1234 5678", SalesCall.ContactFax);

			var inquiry = Factory.New<SalesEnquiry>();
			inquiry.O1_Fax = "03 9876 5432";
			SalesCall.LinkedInquiry = inquiry;
			AssertEquals("03 9876 5432", SalesCall.ContactFax);
		}

		public void TestContactEmail()
		{
			AssertEquals("", SalesCall.ContactEmail);

			var contact = Organisation.Contacts.AddNew();
			contact.OC_Email = "andrew@wisetechglobal.com";
			SalesCall.OQ_OC = contact.PK;
			AssertEquals("andrew@wisetechglobal.com", SalesCall.ContactEmail);

			var inquiry = Factory.New<SalesEnquiry>();
			inquiry.O1_Email = "andrew@cargowise.com";
			SalesCall.LinkedInquiry = inquiry;
			AssertEquals("andrew@cargowise.com", SalesCall.ContactEmail);
		}

		#endregion

		public void TestLocation()
		{
			var orgAddress = Organisation.Addresses.AddNew();
			orgAddress.OA_Address1 = "30 Long Street";
			orgAddress.OA_Code = "Pickup Address";

			var meetingRoom = Factory.New<GlbStaff>();
			meetingRoom.GS_IsResource = true;
			meetingRoom.GS_Code = "$R1";
			meetingRoom.GS_FullName = "Sydney Meeting Room 1";
			meetingRoom.GS_ResourceType = "ROM";

			AssertEquals("", SalesCall.Location);
			AssertEquals(ZGuid.Empty, SalesCall.OQ_OA_LocationAddress);
			AssertEquals("", SalesCall.OQ_GS_NKLocationResource);
			AssertEquals("", SalesCall.OQ_LocationText);

			SalesCall.Location = orgAddress.AddressAsASingleLineWithoutCompanyName;
			AssertEquals(orgAddress.AddressAsASingleLineWithoutCompanyName, SalesCall.Location);
			AssertEquals(orgAddress.PK, SalesCall.OQ_OA_LocationAddress);
			AssertEquals("", SalesCall.OQ_GS_NKLocationResource);
			AssertEquals("", SalesCall.OQ_LocationText);

			SalesCall.Location = "Sydney Meeting Room 1";
			AssertEquals("Sydney Meeting Room 1", SalesCall.Location);
			AssertEquals(ZGuid.Empty, SalesCall.OQ_OA_LocationAddress);
			AssertEquals("$R1", SalesCall.OQ_GS_NKLocationResource);
			AssertEquals("", SalesCall.OQ_LocationText);

			SalesCall.Location = "Eat Me Cafe";
			AssertEquals("Eat Me Cafe", SalesCall.Location);
			AssertEquals(ZGuid.Empty, SalesCall.OQ_OA_LocationAddress);
			AssertEquals("", SalesCall.OQ_GS_NKLocationResource);
			AssertEquals("Eat Me Cafe", SalesCall.OQ_LocationText);

			var longLocation = "Eat Me Cafe".PadRight(50, 'z');
			var expected = longLocation.Substring(0, 50);
			AssertNoExceptionThrown(delegate
			{ SalesCall.Location = longLocation; });
			AssertEquals(expected, SalesCall.Location);
			AssertEquals(ZGuid.Empty, SalesCall.OQ_OA_LocationAddress);
			AssertEquals("", SalesCall.OQ_GS_NKLocationResource);
			AssertEquals(expected, SalesCall.OQ_LocationText);
		}

		public void TestChangeOrgPk()
		{
			var org = Factory.New<OrgHeader>();
			var address = org.Addresses[0];
			var contact = org.Contacts.AddNew();

			var meetingRoom = Factory.New<GlbStaff>();
			meetingRoom.GS_IsResource = true;
			meetingRoom.GS_Code = "$R1";
			meetingRoom.GS_ResourceType = "ROM";

			var salesCall = Factory.New<OrgSalesCall>();

			salesCall.OQ_OH = org.PK;
			salesCall.OQ_OA_LocationAddress = address.PK;
			salesCall.OQ_OC = contact.PK;

			salesCall.OQ_OH = ZGuid.Empty;
			AssertEquals("Org address link should be removed", ZGuid.Empty, salesCall.OQ_OA_LocationAddress);
			AssertEquals("Org contact link should be removed", ZGuid.Empty, salesCall.OQ_OC);

			salesCall.OQ_GS_NKLocationResource = meetingRoom.GS_Code;
			salesCall.OQ_OH = org.PK;
			AssertEquals("Location resource link should remain", meetingRoom.GS_Code, salesCall.OQ_GS_NKLocationResource);
		}

		public void TestOrgName()
		{
			SalesCall.OQ_OH = ZGuid.Empty;
			AssertEquals("", SalesCall.OrgName);

			SalesCall.OQ_OH = Organisation.PK;
			Organisation.OH_FullName = "Wisetech Global";
			SalesCall.OQ_OH = Organisation.PK;
			AssertEquals("Wisetech Global", SalesCall.OrgName);

			var inquiry = Factory.New<SalesEnquiry>();
			inquiry.O1_CompanyName = "Wise Global";
			SalesCall.LinkedInquiry = inquiry;
			AssertEquals("Wise Global", SalesCall.OrgName);
		}

		[TestDate(2013, 1, 1, 1, 1, 1)]
		public void TestLastInvitationActionDescription()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ADL";
			staff1.GS_EmailAddress = "andrew.luong@cargowise.com";
			var additionalStaffAttendee = SalesCall.AdditionalAttendeesStaff.AddNew();
			additionalStaffAttendee.O6_ReceiverReminder = true;
			additionalStaffAttendee.O6_AttendeeID = staff1.PK;

			AssertEquals("", SalesCall.LastInvitationActionDescription);

			TestDateAttribute.Date = new DateTime(2013, 1, 1, 1, 1, 1);
			SalesCall.OQ_NextCall = new ZDateTime(2013, 1, 1);
			Factory.Save();
			SalesCall.SendCalendarReminder();
			AssertEquals("Invitation sent: 01-Jan-13 01:01", SalesCall.LastInvitationActionDescription);

			TestDateAttribute.Date = new DateTime(2013, 2, 2, 2, 2, 2);
			Factory.Save();
			SalesCall.CancelCalendarReminder();
			AssertEquals("Invitation canceled: 02-Feb-13 02:02", SalesCall.LastInvitationActionDescription);

			TestDateAttribute.Date = new DateTime(2013, 3, 3, 3, 3, 3);
			SalesCall.OQ_NextCall = new ZDateTime(2013, 1, 2);
			Factory.Save();
			SalesCall.SendCalendarReminder();
			AssertEquals("Invitation sent: 03-Mar-13 03:03", SalesCall.LastInvitationActionDescription);

			TestDateAttribute.Date = new DateTime(2013, 5, 5, 5, 5, 5);
			additionalStaffAttendee.O6_ReceiverReminder = false;
			Factory.Save();
			AssertEquals("No longer any recipients", "Invitation canceled: 05-May-13 05:05", SalesCall.LastInvitationActionDescription);
		}

		public void TestIsClosedAndOverallDisposition()
		{
			var statusCollection = new CommunicationStatusCollection(false, true);
			statusCollection.Add("XXX", (NoResString)"XXX Description", false, true);
			statusCollection.Add("YYY", (NoResString)"YYY Description", true, true);
			OrganisationsDataRegistry.Instance.CommunicationStatusList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, statusCollection);

			var call = Factory.New<OrgSalesCall>();

			call.OQ_Status = "XXX";
			AssertEquals(false, call.IsClosed);
			AssertEquals(OrgSalesCallOverallDispositionList.Codes.Open, call.OverallDisposition);

			call.OQ_Status = "YYY";
			AssertEquals(true, call.IsClosed);
			AssertEquals(OrgSalesCallOverallDispositionList.Codes.Closed, call.OverallDisposition);

			call.OQ_Status = "ZZZ";
			AssertEquals(false, call.IsClosed);
			AssertEquals(OrgSalesCallOverallDispositionList.Codes.Open, call.OverallDisposition);

			call.OQ_Status = "";
			AssertEquals(false, call.IsClosed);
			AssertEquals(OrgSalesCallOverallDispositionList.Codes.Open, call.OverallDisposition);
		}

		public void TestHumanReadableName()
		{
			OrgSalesCall communication = Factory.New<OrgSalesCall>();
			communication.OQ_CommunicationID = "CommunicationID";
			AssertEquals("Communication (CommunicationID)", communication.HumanReadableName);
			communication.OQ_CommunicationID = "";
			AssertEquals("Communication", communication.HumanReadableName);
		}

		public void TestRelatedActivityLinkCollection()
		{
			var communication = Factory.New<OrgSalesCall>();
			var relatedActivityLinkCollection = communication.RelatedActivityLinkCollection;

			AssertNotNull(relatedActivityLinkCollection);

			var link = relatedActivityLinkCollection.AddNew();
			AssertEquals("New links should have the communication the child by default", communication, link.Pivot.ChildActivity);
			AssertNull(link.Pivot.ParentActivity);
		}

		public void TestShouldSendInvitation()
		{
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			AssertEquals("Should be true by default", true, communication.ShouldSendInvitation);

			communication.ShouldSendInvitation = false;
			AssertEquals("Boolean value should be set to false", false, communication.ShouldSendInvitation);

			Factory.Save();

			var reloadedCommunication = new BusinessObjectFactory().Load<OrgSalesCall>(communication.PK);
			AssertEquals(true, reloadedCommunication.ShouldSendInvitation);
		}

		public void TestOQ_IsReminderClientFacing()
		{
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			AssertEquals("Default value", false, communication.OQ_IsReminderClientFacing);

			communication.OQ_IsReminderClientFacing = true;
			Factory.Save();
			var reloadedCommunication = new BusinessObjectFactory().Load<OrgSalesCall>(communication.PK);
			AssertEquals("Persisted value", true, reloadedCommunication.OQ_IsReminderClientFacing);

			reloadedCommunication.OQ_IsReminderClientFacing = false;
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var attendee = reloadedCommunication.AdditionalAttendeesContact.AddNew();
			attendee.O6_AttendeeID = contact.PK;
			AssertEquals("Adding a attendee does not set it to true...yet", false, reloadedCommunication.OQ_IsReminderClientFacing);

			attendee.O6_ReceiverReminder = true;
			AssertEquals("Setting the attendee to be reminded sets it to true", true, reloadedCommunication.OQ_IsReminderClientFacing);

			reloadedCommunication.OQ_IsReminderClientFacing = false;
			attendee.O6_ReceiverReminder = false;
			AssertEquals("Setting the attendee to be NOT reminded keeps it as false", false, reloadedCommunication.OQ_IsReminderClientFacing);

			attendee.O6_ReceiverReminder = true;
			reloadedCommunication.OQ_IsReminderClientFacing = false;
			AssertEquals("Precondition", false, reloadedCommunication.OQ_IsReminderClientFacing);
			reloadedCommunication.AdditionalAttendeesContact.RefreshBindingIncludingChildren(); // One of the actions taken when clicking the save/new button on the form. See ZForm.OnApplyButtonClick()
			AssertEquals("Trying to save the communication should keep value as false", false, reloadedCommunication.OQ_IsReminderClientFacing);

			reloadedCommunication.AdditionalAttendeesContact.RemoveAll();
			AssertEquals("Removing all contacts should keep value as false", false, reloadedCommunication.OQ_IsReminderClientFacing);
		}

		public void TestHasRiskOfSendingInternalNotesToAttendees()
		{
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			var attendee = communication.AdditionalAttendeesContact.AddNew();
			attendee.O6_AttendeeName = "B1";
			attendee.O6_ReceiverReminder = false;
			AssertEquals("No invited attendee", false, communication.HasRiskOfSendingInternalNotesToAttendees);

			attendee.O6_ReceiverReminder = true;
			communication.OQ_IsReminderClientFacing = false;
			AssertEquals("1 invited attendee but unticked client visible invitation", true, communication.HasRiskOfSendingInternalNotesToAttendees);

			communication.OQ_IsReminderClientFacing = true;
			AssertEquals("1 invited attendee and ticked client visible invitation", false, communication.HasRiskOfSendingInternalNotesToAttendees);

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var reloadedCommunication = newFactory.Load<OrgSalesCall>(communication.PK);
			reloadedCommunication.OQ_IsReminderClientFacing = false;
			AssertEquals("1 invited attendee but unticked client version invitation", true, reloadedCommunication.HasRiskOfSendingInternalNotesToAttendees);

			var reloadedAttendee = newFactory.Load<OrgSalesCallAdditionalAttendee>(attendee.PK);
			reloadedAttendee.O6_ReceiverReminder = false;
			AssertEquals("0 UNSAVED invited attendee and unticked client visible invitation", true, reloadedCommunication.HasRiskOfSendingInternalNotesToAttendees);
		}

		public void TestFixRiskOfSendingInternalNotesToAttendees()
		{
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			var attendee = communication.AdditionalAttendeesContact.AddNew();
			attendee.O6_ReceiverReminder = true;
			communication.OQ_IsReminderClientFacing = false;
			communication.FixRiskOfSendingInternalNotesToAttendees();
			AssertEquals("Changed", true, communication.OQ_IsReminderClientFacing);
			AssertEquals("Unchanged", true, attendee.O6_ReceiverReminder);
		}

		#endregion

		#region Workflow

		public void TestCurrentTask()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var salesCall = org.SalesCalls.AddNew();

			AssertNull("NULL is returned when there is no workflow.", salesCall.CurrentTask);

			var cancelledWorkflow = salesCall.WorkflowItems.AddNew();
			cancelledWorkflow.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			cancelledWorkflow.P9_Sequence = 10;

			AssertNull("CAN workflow is not returned as current task.", salesCall.CurrentTask);

			var closedWorkflow = salesCall.WorkflowItems.AddNew();
			closedWorkflow.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			closedWorkflow.P9_Sequence = 9;

			AssertNull("CLS workflow is not returned as current task.", salesCall.CurrentTask);

			var assignedWorkflow = salesCall.WorkflowItems.AddNew();
			assignedWorkflow.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			assignedWorkflow.P9_Sequence = 8;

			AssertEquals(assignedWorkflow, salesCall.CurrentTask);

			var assignedWorkflow2 = salesCall.WorkflowItems.AddNew();
			assignedWorkflow2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			assignedWorkflow2.P9_Sequence = 7;

			AssertEquals("If same workflow exist for any of the WRK, SUS, or ASN, the workflow with the lowest sequence number is returned.", assignedWorkflow2, salesCall.CurrentTask);

			var workingWorkflow = salesCall.WorkflowItems.AddNew();
			workingWorkflow.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			workingWorkflow.P9_Sequence = 6;

			AssertEquals(workingWorkflow, salesCall.CurrentTask);

			var suspendedWorkflow = salesCall.WorkflowItems.AddNew();
			suspendedWorkflow.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			suspendedWorkflow.P9_Sequence = 5;

			AssertEquals(suspendedWorkflow, salesCall.CurrentTask);

			var openWorkflow = salesCall.WorkflowItems.AddNew();
			openWorkflow.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			openWorkflow.P9_Sequence = 4;

			AssertEquals("If any of the WRK, SUS, or ASN workflows exist, they take precedence over the OPN workflow.", suspendedWorkflow, salesCall.CurrentTask);

			assignedWorkflow.Delete();
			assignedWorkflow2.Delete();
			workingWorkflow.Delete();
			suspendedWorkflow.Delete();

			AssertEquals(openWorkflow, salesCall.CurrentTask);
		}

		public void TestTaskStatus()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var salesCall = org.SalesCalls.AddNew();

			AssertEquals("Empty string is returned when there is workflow for communication." ,"", salesCall.TaskStatus);

			var assignedWorkflow = salesCall.WorkflowItems.AddNew();
			assignedWorkflow.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			assignedWorkflow.P9_Sequence = 10;

			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, salesCall.TaskStatus);

			var workingWorkflow = salesCall.WorkflowItems.AddNew();
			workingWorkflow.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			workingWorkflow.P9_Sequence = 8;

			AssertEquals("If any of the WRK, SUS, or ASN workflows exist, the workflow with the lowest sequence number is returned.", ProcessTaskStatusCodeList.Codes.Working, salesCall.TaskStatus);

			var suspendedWorkflow = salesCall.WorkflowItems.AddNew();
			suspendedWorkflow.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			suspendedWorkflow.P9_Sequence = 6;

			AssertEquals("If any of the WRK, SUS, or ASN workflows exist, the workflow with the lowest sequence number is returned.", ProcessTaskStatusCodeList.Codes.Suspended, salesCall.TaskStatus);

			var openWorkflow = salesCall.WorkflowItems.AddNew();
			openWorkflow.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			openWorkflow.P9_Sequence = 4;

			AssertEquals("If any of the WRK, SUS, or ASN workflows exist, they take precedence over the OPN workflow.", ProcessTaskStatusCodeList.Codes.Suspended, salesCall.TaskStatus);

			assignedWorkflow.Delete();
			workingWorkflow.Delete();
			suspendedWorkflow.Delete();

			AssertEquals("If none of the WRK, SUS, or ASN workflows exist, the OPN workflow is returned.", ProcessTaskStatusCodeList.Codes.Open, salesCall.TaskStatus);

			var closedWorkflow = salesCall.WorkflowItems.AddNew();
			closedWorkflow.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			closedWorkflow.P9_Sequence = 3;

			AssertEquals("OPN workflow takes precedence over the CAN and CLS workflow.", ProcessTaskStatusCodeList.Codes.Open, salesCall.TaskStatus);

			var cancelledWorkflow = salesCall.WorkflowItems.AddNew();
			cancelledWorkflow.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			cancelledWorkflow.P9_Sequence = 2;

			AssertEquals("OPN workflow takes precedence over the CAN and CLS workflow.", ProcessTaskStatusCodeList.Codes.Open, salesCall.TaskStatus);

			openWorkflow.Delete();

			AssertEquals("CLS workflow takes precedence over the CAN workflow.", ProcessTaskStatusCodeList.Codes.Closed, salesCall.TaskStatus);

			closedWorkflow.Delete();

			AssertEquals("CAN workflow is returned when no other workflow status types exist.", ProcessTaskStatusCodeList.Codes.Cancelled, salesCall.TaskStatus);
		}

		#endregion

		#region LinkedInquiry

		public void TestLinkedInquiry()
		{
			var parentOpportunity = Factory.New<OrgOpportunity>();
			var childOpportunity = Factory.New<OrgOpportunity>();
			var communication = Factory.New<OrgSalesCall>();
			communication.RelatedChildActivityPivotCollection.AddNewPivot(childOpportunity);
			communication.RelatedParentActivityPivotCollection.AddNewPivot(parentOpportunity);

			communication.LinkedInquiry = null;
			AssertEquals(false, communication.IsLinkedToInquiry);
			AssertContainsExactElementsInAnyOrder("Should not remove related activities when removing linked inquiry",
				BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer,
				new BusinessObject[] { childOpportunity },
				communication.RelatedChildActivityPivotCollection.Activities.Cast<BusinessObject>());
			AssertContainsExactElementsInAnyOrder("Should not remove related activities when removing linked inquiry",
				BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer,
				new BusinessObject[] { parentOpportunity },
				communication.RelatedParentActivityPivotCollection.Activities.Cast<BusinessObject>());

			var inquiry = Factory.New<SalesEnquiry>();
			communication.LinkedInquiry = inquiry;
			AssertEquals(true, communication.IsLinkedToInquiry);
			AssertContainsExactElementsInAnyOrder("Should remove all related activities except linked inquiry",
				BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer,
				Enumerable.Empty<BusinessObject>(),
				communication.RelatedChildActivityPivotCollection.Activities.Cast<BusinessObject>());
			AssertContainsExactElementsInAnyOrder("Should remove all related activities except linked inquiry",
				BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer,
				new BusinessObject[] { inquiry },
				communication.RelatedParentActivityPivotCollection.Activities.Cast<BusinessObject>());
		}

		public void TestLinkedInquiry_WhenPivotParentActivityIsCampaignItem()
		{
			var inquiry = Factory.New<SalesEnquiry>();
			BusinessObject campaign = (BusinessObject)Factory.New<IGlbCompanyCampaign>();
			BusinessObject campaignItem = (BusinessObject)Factory.New<IGlbCompanyCampaignItem>();
			campaignItem[GlbCompanyCampaignItemSchema.G8_RecipientID.Name] = inquiry.PK;
			campaignItem[GlbCompanyCampaignItemSchema.G8_G0.Name] = campaign.PK;
			var communication = Factory.New<OrgSalesCall>();
			communication.RelatedParentActivityPivotCollection.AddNewPivot((IRelatableActivity)campaignItem);

			communication.RunPreSaveValidation();
			AssertEquals(true, communication.IsLinkedToInquiry);
		}

		public void TestLinkedInquiry_OnReload()
		{
			var inquiry = Factory.New<SalesEnquiry>();
			var linkedCommunication = Factory.New<OrgSalesCall>();
			linkedCommunication.LinkedInquiry = inquiry;

			var relatedCommunication = Factory.New<OrgSalesCall>();
			relatedCommunication.OQ_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			inquiry.RelatedChildActivityPivotCollection.AddNewPivot(relatedCommunication);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var linkedCommunicationInOtherFactory = otherFactory.Load<OrgSalesCall>(linkedCommunication.PK);
			AssertNotNull(linkedCommunicationInOtherFactory.LinkedInquiry);
			AssertEquals(inquiry.PK, linkedCommunicationInOtherFactory.LinkedInquiry.PK);

			var relatedCommunicationInOtherFactory = otherFactory.Load<OrgSalesCall>(relatedCommunication.PK);
			AssertNull(relatedCommunicationInOtherFactory.LinkedInquiry);
		}

		public void TestLinkedInquiry_WhenInquiryHasClientIntelligenceButNotSavedYet()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TESTADL";
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var linkedCommunication = Factory.New<OrgSalesCall>();
			linkedCommunication.LinkedInquiry = inquiry;

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var inquiryInFactory2 = factory2.Load<SalesEnquiry>(inquiry.PK);
			inquiryInFactory2.O1_OH_ConvertedToQualifiedLead = org.PK;

			var communciationInFactory2 = factory2.Load<OrgSalesCall>(linkedCommunication.PK);
			AssertEquals("Should still be linked to inquiry (because the client intelligence on inquiry is not saved yet)", true, communciationInFactory2.IsLinkedToInquiry);
			AssertEquals("Should still be linked to inquiry (because the client intelligence on inquiry is not saved yet)", inquiryInFactory2.PK, communciationInFactory2.LinkedInquiry.PK);

			factory2.Save();

			var factory3 = new BusinessObjectFactory();
			var communicationInFactory3 = factory3.Load<OrgSalesCall>(linkedCommunication.PK);
			AssertEquals("Should no longer be linked to inquiry (because the client intelligence on inquiry is now saved)", false, communicationInFactory3.IsLinkedToInquiry);
		}

		public void TestLinkedInquiry_WhenChildPivotAddedButNotYetSaved()
		{
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var linkedCommunication = Factory.New<OrgSalesCall>();
			linkedCommunication.LinkedInquiry = inquiry;

			linkedCommunication.RelatedChildActivityPivotCollection.AddNewPivot(Factory.New<OrgOpportunity>());

			linkedCommunication.RunPreSaveValidation(); // Force LinkedInquiry to refresh

			AssertEquals("Should still be linked to Inquiry", true, linkedCommunication.IsLinkedToInquiry);
			AssertEquals("Should still be linked to Inquiry", inquiry.PK, linkedCommunication.LinkedInquiry.PK);
		}

		public void TestLinkedInquiry_WhenParentPivotAddedButNotYetSaved()
		{
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var linkedCommunication = Factory.New<OrgSalesCall>();
			linkedCommunication.LinkedInquiry = inquiry;

			linkedCommunication.RelatedParentActivityPivotCollection.AddNewPivot(Factory.New<OrgOpportunity>());

			linkedCommunication.RunPreSaveValidation(); // Force LinkedInquiry to refresh

			AssertEquals("Should still be linked to Inquiry", true, linkedCommunication.IsLinkedToInquiry);
			AssertEquals("Should still be linked to Inquiry", inquiry.PK, linkedCommunication.LinkedInquiry.PK);
		}

		public void TestSettingOrgRemovesLinkedInquiry()
		{
			var inquiry = Factory.New<SalesEnquiry>();
			var communication = Factory.New<OrgSalesCall>();

			communication.LinkedInquiry = inquiry;
			AssertEquals(true, communication.IsLinkedToInquiry);

			communication.OQ_OH = ZGuid.Empty;
			AssertEquals(true, communication.IsLinkedToInquiry);

			communication.OQ_OH = Factory.New<OrgHeader>().PK;
			AssertEquals(false, communication.IsLinkedToInquiry);
		}

		public void TestSettingOrgRemovesLinkedInquiry_InAnotherFactory()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TESTADL";
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var communication = Factory.New<OrgSalesCall>();
			communication.LinkedInquiry = inquiry;

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var communicationInAnotherFactory = anotherFactory.Load<OrgSalesCall>(communication.PK);
			communicationInAnotherFactory.OQ_OH = org.PK;
			anotherFactory.Save();

			AssertEquals(false, communication.IsLinkedToInquiry);
		}

		public void TestRelatedActivityPivotCollectionsReadOnlyWhenLinkedToInquiry()
		{
			var inquiry = Factory.New<SalesEnquiry>();
			var communication = Factory.New<OrgSalesCall>();

			communication.LinkedInquiry = inquiry;
			AssertEquals(true, communication.RelatedActivityLinkCollection.ReadOnly);
			AssertEquals(true, communication.RelatedChildActivityPivotCollection.ReadOnly);
			AssertEquals(true, communication.RelatedParentActivityPivotCollection.ReadOnly);

			communication.LinkedInquiry = null;
			AssertEquals(false, communication.RelatedActivityLinkCollection.ReadOnly);
			AssertEquals(false, communication.RelatedChildActivityPivotCollection.ReadOnly);
			AssertEquals(false, communication.RelatedParentActivityPivotCollection.ReadOnly);
		}

		public void TestDeleteWhileLinkedToInquiry()
		{
			var inquiry = Factory.New<SalesEnquiry>();
			var communication = Factory.New<OrgSalesCall>();
			communication.LinkedInquiry = inquiry;

			AssertNoExceptionThrown(() => communication.Delete());
		}

		public void TestSettingLinkedInquiryClientIntelligenceBeforeSaveNew()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TESTADL";
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiry.O1_OH_ConvertedToQualifiedLead = ZGuid.Empty;

			Factory.Save();

			var newCommunication = Factory.New<OrgSalesCall>();
			newCommunication.LinkedInquiry = inquiry;

			var anotherFactory = new BusinessObjectFactory();
			anotherFactory.Load<SalesEnquiry>(inquiry.PK).O1_OH_ConvertedToQualifiedLead = org.PK;
			anotherFactory.Save();

			newCommunication.RunPreSaveValidation();
			AssertHasErrorContaining(newCommunication.OQ_OHInfo, MandatoryValidation.MustBeEntered);
		}

		#endregion

		public void TestTradeProfileDescriptionList()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgSalesCall salesCall = org.SalesCalls.AddNew();
			AssertNotNull("The TradeProfileList should not be null", salesCall.TradeProfileDescriptionList);
		}

		public void TestExtraCategoryLabel()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgSalesCall call = org.SalesCalls.AddNew();

			OrganisationsDataRegistry.Instance.CategoryListLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Edward");
			AssertEquals("Edward", call.ExtraCategoryDescription);
		}

		[TestDate(2014, 5, 15)]
		public void TestUpdateHeaderSalesCalls_OnSaving()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var call1 = org1.SalesCalls.AddNew();
			var call2 = org1.SalesCalls.AddNew();

			call1.OQ_NextCall = new ZDateTime(2014, 5, 13);
			call1.OQ_CallDate = ZDateTime.Empty;
			call1.OQ_Status = "SCH";
			call2.OQ_NextCall = new ZDateTime(2014, 4, 24);
			call2.OQ_CallDate = ZDateTime.Empty;
			call2.OQ_Status = "SCH";
			Factory.Save();

			AssertEquals(new ZDateTime(2014, 5, 13), org1.MiscServ.GetLastUnactionedCallDateSalesCall().OQ_NextCall);

			call1.OQ_OH = org2.PK;
			Factory.Save();

			AssertEquals(new ZDateTime(2014, 4, 24), org1.MiscServ.GetLastUnactionedCallDateSalesCall().OQ_NextCall);
			AssertEquals(new ZDateTime(2014, 5, 13), org2.MiscServ.GetLastUnactionedCallDateSalesCall().OQ_NextCall);
		}

		[TestDate(2014, 5, 15)]
		public void TestUpdateHeaderSalesCalls_Delete()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var call1 = org1.SalesCalls.AddNew();
			var call21 = org2.SalesCalls.AddNew();
			var call22 = org2.SalesCalls.AddNew();

			call1.OQ_NextCall = ZDateTime.Empty;
			call1.OQ_CallDate = new ZDateTime(2014, 5, 13);
			call1.OQ_Status = "SCH";
			call21.OQ_NextCall = ZDateTime.Empty;
			call21.OQ_CallDate = new ZDateTime(2014, 5, 13);
			call21.OQ_Status = "SCH";
			call22.OQ_NextCall = ZDateTime.Empty;
			call22.OQ_CallDate = new ZDateTime(2014, 4, 13);
			call22.OQ_Status = "SCH";
			Factory.Save();

			AssertEquals(new ZDateTime(2014, 5, 13), org1.MiscServ.OM_CMLastCallDate);
			AssertEquals(new ZDateTime(2014, 5, 13), org2.MiscServ.OM_CMLastCallDate);

			call1.Delete();
			call21.Delete();
			Factory.Save();

			AssertEquals(ZDateTime.Empty, org1.MiscServ.OM_CMLastCallDate);
			AssertEquals(new ZDateTime(2014, 4, 13), org2.MiscServ.OM_CMLastCallDate);
		}

		public void TestRelinkRelatedParentActivity_WithCampaignItem()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABC";

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Edward";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "John";

			GlbStaff campaignStaff = Factory.NewWithValidTestData<GlbStaff>();
			campaignStaff.GS_Code = "CMS";
			campaignStaff.GS_EmailAddress = "cms@test.org";

			BusinessObject campaign = (BusinessObject)Factory.New<IGlbCompanyCampaign>();
			campaign[GlbCompanyCampaignSchema.G0_CampaignName.Name] = "Test Campaign";
			campaign[GlbCompanyCampaignSchema.G0_Category.Name] = "PRINT";
			campaign[GlbCompanyCampaignSchema.G0_EmailSubject.Name] = "Email Subject";
			((IGlbCompanyCampaign)campaign).HtmlDocumentBlob = new ZBlob(Encoding.ASCII.GetBytes("(*CampaignURL*)"));
			campaign[GlbCompanyCampaignSchema.G0_EstimatedStartedDate.Name] = ZDateTime.Now.AddDays(-1);
			campaign[GlbCompanyCampaignSchema.G0_GS_NKCampaignCoordinator.Name] = campaignStaff.GS_Code;
			campaign[GlbCompanyCampaignSchema.G0_GS_NKCampaignManager.Name] = campaignStaff.GS_Code;
			campaign[GlbCompanyCampaignSchema.G0_Type.Name] = "EXIST";

			BusinessObject campaignItem = (BusinessObject)Factory.New<IGlbCompanyCampaignItem>();
			campaignItem[GlbCompanyCampaignItemSchema.G8_G0.Name] = campaign.PK;
			campaignItem[GlbCompanyCampaignItemSchema.G8_RecipientTableCode.Name] = OrgContactSchema.Constants.Prefix;
			campaignItem[GlbCompanyCampaignItemSchema.G8_RecipientID.Name] = contact.PK;

			BusinessObject campaignItem2 = (BusinessObject)Factory.New<IGlbCompanyCampaignItem>();
			campaignItem2[GlbCompanyCampaignItemSchema.G8_G0.Name] = campaign.PK;
			campaignItem2[GlbCompanyCampaignItemSchema.G8_RecipientTableCode.Name] = OrgContactSchema.Constants.Prefix;
			campaignItem2[GlbCompanyCampaignItemSchema.G8_RecipientID.Name] = contact1.PK;

			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			((IImportParentRelatedActivityInfoOnNew)communication).ImportParentInfo((IRelatableActivity)campaignItem, new ImportRelatedActivityNoDecisionFactory());
			Factory.Save();

			var collection = (OrgSalesCallRelatedParentActivityPivotCollection)communication.RelatedParentActivityPivotCollection;
			collection.AddNewPivot((IRelatableActivity)campaignItem);
			AssertEquals(collection[0].ParentActivity.PK, campaignItem.PK);

			communication.OQ_OC = contact1.PK;
			Factory.Save();

			AssertEquals(collection[0].ParentActivity.PK, campaignItem2.PK);
			Factory.Save();

			AssertEquals(collection[0].ParentActivity.PK, campaignItem2.PK);
		}

		#region IImportParentRelatedActivityInfoOnNew

		public void TestImportParentRelatedActivityInfoOnNew()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTAA";
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "test@test.com";
			var inquiry = Factory.New<SalesEnquiry>();
			inquiry.O1_OH_ConvertedToQualifiedLead = org.PK;
			inquiry.O1_OC_LinkedContact = contact.PK;

			var communication = Factory.New<OrgSalesCall>();
			((IImportParentRelatedActivityInfoOnNew)communication).ImportParentInfo(inquiry, new ImportRelatedActivityNoDecisionFactory());

			CombineAssertions(() =>
			{
				AssertEquals("OQ_OH", org.PK, communication.OQ_OH);
				AssertEquals("OQ_OC", contact.PK, communication.OQ_OC);
				AssertNull("LinkedInquiry", communication.LinkedInquiry);
			});

			inquiry.O1_OH_ConvertedToQualifiedLead = ZGuid.Empty;
			inquiry.O1_OC_LinkedContact = ZGuid.Empty;
			var communication2 = Factory.New<OrgSalesCall>();
			((IImportParentRelatedActivityInfoOnNew)communication2).ImportParentInfo(inquiry, new ImportRelatedActivityNoDecisionFactory());

			CombineAssertions(() =>
			{
				AssertEquals("OQ_OH", ZGuid.Empty, communication2.OQ_OH);
				AssertEquals("OQ_OC", ZGuid.Empty, communication2.OQ_OC);
				AssertEquals("LinkedInquiry", inquiry, communication2.LinkedInquiry);
			});
		}

		#endregion

		public void TestOQ_GS_NKSalesRep_ReadOnly()
		{
			Env.Security.CommunicationManagerEditModifyStaffAssignment.IsAllowed = true;
			var orgSalesCall = Factory.NewWithValidTestData<OrgSalesCall>();
			AssertEquals(false, orgSalesCall.OQ_GS_NKSalesRepInfo.ReadOnly);

			Env.Security.CommunicationManagerEditModifyStaffAssignment.IsAllowed = false;
			AssertEquals(false, orgSalesCall.OQ_GS_NKSalesRepInfo.ReadOnly);

			Factory.Save();

			Env.Security.CommunicationManagerEditModifyStaffAssignment.IsAllowed = true;
			AssertEquals(false, orgSalesCall.OQ_GS_NKSalesRepInfo.ReadOnly);

			Env.Security.CommunicationManagerEditModifyStaffAssignment.IsAllowed = false;
			AssertEquals(true, orgSalesCall.OQ_GS_NKSalesRepInfo.ReadOnly);
		}

		public void TestDocumentSupporter()
		{
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			AssertEquals(typeof(OrgSalesCallDocumentSupporter), communication.DocumentSupporter.GetType());
		}

		public void TestHtmlProperty()
		{
			var call = Factory.New<OrgSalesCall>();
			AssertEquals(ZBlob.Empty, call.OQ_SalesCallNotes);
			AssertEquals(ZBlob.Empty, call.OQ_SalesCallNotes_HTML);
			AssertEquals(ZBlob.Empty, call.OQ_FollowupNotes);
			AssertEquals(ZBlob.Empty, call.OQ_FollowupNotes_HTML);

			call.OQ_SalesCallNotes_HTML = ZBlob.FromUTF8("<p>123</p>");
			call.OQ_FollowupNotes_HTML = ZBlob.FromUTF8("<p>123</p>");

			AssertEquals(@"{\rtf1\ansi\ansicpg1252\deflang3081\nouicompat\uc0{\fonttbl}{\colortbl}{{123}\par}}", ORtfTextUtil.GeneratorInfoRegex.Replace(call.OQ_SalesCallNotes.ToUTF8(), string.Empty));
			AssertEquals("<p>123</p>", call.OQ_SalesCallNotes_HTML.ToUTF8());
			AssertEquals(@"{\rtf1\ansi\ansicpg1252\deflang3081\nouicompat\uc0{\fonttbl}{\colortbl}{{123}\par}}", ORtfTextUtil.GeneratorInfoRegex.Replace(call.OQ_FollowupNotes.ToUTF8(), string.Empty));
			AssertEquals("<p>123</p>", call.OQ_FollowupNotes_HTML.ToUTF8());
		}

		public void TestHtmlFromTextProperty()
		{
			var call = Factory.New<OrgSalesCall>();
			AssertEquals(ZBlob.Empty, call.OQ_SalesCallNotes);
			AssertEquals(ZBlob.Empty, call.OQ_SalesCallNotes_HTML);
			AssertEquals(ZBlob.Empty, call.OQ_FollowupNotes);
			AssertEquals(ZBlob.Empty, call.OQ_FollowupNotes_HTML);

			call.OQ_SalesCallNotes = ZBlob.FromUTF8("1234\r\n5678");
			call.OQ_FollowupNotes = ZBlob.FromUTF8("1234\r\n5678");

			AssertEquals("<p>1234</p><p>5678</p>", call.OQ_SalesCallNotes_HTML.ToUTF8());
			AssertEquals("<p>1234</p><p>5678</p>", call.OQ_FollowupNotes_HTML.ToUTF8());

			call.OQ_SalesCallNotes = ZBlob.FromUTF8("{\\rtf1\\test\\ansi\\ansicpg1252\\nouicompat\\deflang3081\r\n{\\*\\generator Riched20 10.0.19041}\\viewkind4\\uc1 \\pard rtf\\par\r\n}\r\n");
			call.OQ_FollowupNotes = ZBlob.FromUTF8("{\\rtf1\\test\\ansi\\ansicpg1252\\nouicompat\\deflang3081\r\n{\\*\\generator Riched20 10.0.19041}\\viewkind4\\uc1 \\pard rtf\\par\r\n}\r\n");

			AssertEquals("<p>rtf</p>", call.OQ_SalesCallNotes_HTML.ToUTF8());
			AssertEquals("<p>rtf</p>", call.OQ_FollowupNotes_HTML.ToUTF8());
		}

		#region Implementation

		public class OrgSalesCallForTesting : OrgSalesCall
		{
			public OrgSalesCallForTesting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void OnReminderSent(CommunicationReminder reminder)
			{
				base.OnReminderSent(reminder);
				LastReminderSent = reminder;
				RemindersSent.Add(reminder);
			}

			public Reminder LastReminderSent;
			public List<Reminder> RemindersSent = new List<Reminder>();
		}

		OrgSalesCall SalesCall;
		OrgHeader Organisation;

		protected override void SetUp()
		{
			base.SetUp();
			Organisation = Factory.NewWithValidTestData<OrgHeader>();
			SalesCall = Organisation.SalesCalls.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			return org.SalesCalls.AddNew();
		}

		#endregion
	}
}
