using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	public class CommunicationControllerTest : TestCaseWithFactory
	{
		#region TestCommunication_SendReminder

		[TestDate(2025, 1, 1)]
		public void TestCommunication_SendReminder()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_Code = "TST";
			var orgSalesCall = Factory.NewWithValidTestData<OrgSalesCall>();
			orgSalesCall.OQ_OH = header.PK;
			orgSalesCall.OQ_CallSummary = "Test Subject";
			orgSalesCall.OQ_SalesCallNotes = ZBlob.FromUTF8("Test Calendar");
			orgSalesCall.OQ_NextCall = ZDateTime.UtcNow;
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var attendee = Factory.NewWithValidTestData<OrgSalesCallAdditionalAttendee>();
			attendee.O6_OQ = orgSalesCall.PK;
			attendee.O6_AttendeeID = contact.PK;
			attendee.O6_AttendeeTableCode = "OC";
			attendee.O6_EmailAddress = "test@test.com";
			attendee.O6_ReceiverReminder = true;
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			var result = controller.SendReminder(orgSalesCall.PK.ToGuid());

			AssertEquals(HttpStatusCode.OK, result.StatusCode);

			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Test Subject", sentEmail.Subject);
			AssertContains("Test Calendar", sentEmail.Body);

			var dtStartValue = Regex.Match(sentEmail.Body, @"DTSTART:(\d{8})T").Groups[1].Value;
			AssertEquals("20250101", dtStartValue);
		}

		public void TestCommunication_SendReminderWithMultipleAttendees()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_Code = "TST";
			var orgSalesCall = Factory.NewWithValidTestData<OrgSalesCall>();
			orgSalesCall.OQ_OH = header.PK;
			orgSalesCall.OQ_CallSummary = "Test Subject";
			orgSalesCall.OQ_SalesCallNotes = ZBlob.FromUTF8("Test Calendar");
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var attendee1 = Factory.NewWithValidTestData<OrgSalesCallAdditionalAttendee>();
			attendee1.O6_OQ = orgSalesCall.PK;
			attendee1.O6_AttendeeID = contact.PK;
			attendee1.O6_AttendeeTableCode = "OC";
			attendee1.O6_EmailAddress = "test1@test.com";
			attendee1.O6_ReceiverReminder = true;
			var attendee2 = Factory.NewWithValidTestData<OrgSalesCallAdditionalAttendee>();
			attendee2.O6_OQ = orgSalesCall.PK;
			attendee2.O6_AttendeeID = contact.PK;
			attendee2.O6_AttendeeTableCode = "OC";
			attendee2.O6_EmailAddress = "test2@test.com";
			attendee2.O6_ReceiverReminder = true;
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			var result = controller.SendReminder(orgSalesCall.PK.ToGuid());

			AssertEquals(HttpStatusCode.OK, result.StatusCode);

			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Test Subject", sentEmail.Subject);
			AssertContains("Test Calendar", sentEmail.Body);
			AssertEquals(2, sentEmail.Recipients.Count);
		}

		public void TestCommunication_SendReminderWithoutData()
		{
			var orgSalesCall = Factory.NewWithValidTestData<OrgSalesCall>();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var result = controller.SendReminder(orgSalesCall.PK.ToGuid());

			AssertEquals(HttpStatusCode.InternalServerError, result.StatusCode);

			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestCommunication_SendReminderWithException()
		{
			var result = controller.SendReminder(Guid.NewGuid());

			AssertEquals(HttpStatusCode.InternalServerError, result.StatusCode);
		}

		public void TestCommunication_SendReminderWithDisabledCalendarIntegration()
		{
			Env.Registry.CalendarIntegration = false;

			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_Code = "TST";
			var orgSalesCall = Factory.NewWithValidTestData<OrgSalesCall>();
			orgSalesCall.OQ_OH = header.PK;
			orgSalesCall.OQ_CallSummary = "Test Subject";
			orgSalesCall.OQ_SalesCallNotes = ZBlob.FromUTF8("Test Calendar");
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var attendee = Factory.NewWithValidTestData<OrgSalesCallAdditionalAttendee>();
			attendee.O6_OQ = orgSalesCall.PK;
			attendee.O6_AttendeeID = contact.PK;
			attendee.O6_AttendeeTableCode = "OC";
			attendee.O6_EmailAddress = "test@test.com";
			attendee.O6_ReceiverReminder = true;
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			var result = controller.SendReminder(orgSalesCall.PK.ToGuid());

			AssertEquals(HttpStatusCode.Forbidden, result.StatusCode);

			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		#endregion

		#region implementation

		protected override void SetUp()
		{
			base.SetUp();

			controller = new CommunicationController();
			var controllerContext = new HttpControllerContext(new HttpConfiguration(), new HttpRouteData(new HttpRoute()), new HttpRequestMessage());
			controller.ControllerContext = controllerContext;
		}

		protected override void TearDown()
		{
			base.TearDown();

			controller?.Dispose();
		}

		CommunicationController controller;

		#endregion
	}
}
