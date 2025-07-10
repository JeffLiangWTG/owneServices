using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ReminderTest : TestCaseWithFactory
	{
		public void TestCreateAppointment_ExceptionIsHandled_WhenUserInteractive()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "aaa.bbb.com ";
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var parentFactory = new BusinessObjectFactory();
				var parent = parentFactory.New<DummyWithWorkflow>();
				parentFactory.Save();

				var notificationHandler = NotificationHandler.Instance;
				try
				{
					NotificationHandler.Instance = new ZGUINotificationHandler();
					var reminder = new Reminder("Reminder ID", parent.PK, new ZString(parent.TableName), DateTimeKind.Local, ZDateTime.Now, ZDateTime.Now, "", "");
					reminder.Recipients.Add("me", "me@you.com");
					reminder.CreateAppointment();

					var expectedMessage =
						"Could not create appointment due to the following error:\r\n\r\nInvalid email address for sender 'aaa.bbb.com'";
					AssertEquals("Error Message", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					Assert("Message Type Should Be Error", UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertNullOrEmpty("InvalidSender Error should only be reported to user",
						ErrorReporter.LastKeyReported);
				}
				finally
				{
					NotificationHandler.Instance = notificationHandler;
				}
			}
		}

		public void TestCreateAppointment_ExceptionIsNotCaught_WhenNotUserInteractive()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "aaa.bbb.com ";
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var parentFactory = new BusinessObjectFactory();
				var parent = parentFactory.New<DummyWithWorkflow>();
				parentFactory.Save();

				var oldIsUserInteractive = Globals.IsUserInteractive;

				try
				{
					Globals.IsUserInteractive = false; // Simulate non-user interactive environment
					var reminder = new Reminder("Reminder ID", parent.PK, new ZString(parent.TableName), DateTimeKind.Local, ZDateTime.Now, ZDateTime.Now, "", "");
					reminder.Recipients.Add("me", "me@you.com");
					AssertExceptionThrown<EmailSendFailedException>(() => reminder.CreateAppointment());
				}
				finally
				{
					Globals.IsUserInteractive = oldIsUserInteractive;
				}
			}
		}

		public void TestDeliveringReminderYieldsSequentialSequenceNumbers()
		{
			var parentFactory = new BusinessObjectFactory();
			var parent = parentFactory.New<DummyWithWorkflow>();
			parent.Logs.AddNew(Events.QuotationAccepted);
			parentFactory.Save();

			AssertEquals("Making Logs collection subscribed to DataRefreshBus", true, parent.Logs.GetAllLogs().IsLoaded);
			AssertEquals("Precondition", false, parent.HasChanges);

			var reminder1ID = parent.PK.ToString() + "Blah blah blah";
			var reminder1 = new Reminder(reminder1ID, parent.PK, new ZString(parent.TableName), DateTimeKind.Local, ZDateTime.Now, ZDateTime.Now, "", "");
			reminder1.Recipients.Add("me", "me@you.com");

			AssertNull("Precondition: should be no matching log", GetLogForIdentifiers(parent.PK, reminder1ID));

			CreateAppointmentAndAssertSequence(reminder1, 0u);
			CreateAppointmentAndAssertSequence(reminder1, 1u);
			CreateAppointmentAndAssertSequence(reminder1, 2u);

			AssertEquals("Appointment reference logs saved in isolated factory; Parent.HasChanges not affected", false, parent.HasChanges);

			var reminder2ID = parent.PK.ToString() + "only one blah";
			var reminder2 = new Reminder(reminder2ID, parent.PK, new ZString(parent.TableName), DateTimeKind.Local, ZDateTime.Now, ZDateTime.Now, "", "");
			reminder2.Recipients.Add("me", "me@you.com");

			AssertNull("Should not match same log as reminder1", GetLogForIdentifiers(parent.PK, reminder2ID));

			CreateAppointmentAndAssertSequence(reminder2, 0u);
			CreateAppointmentAndAssertSequence(reminder2, 1u);
			CreateAppointmentAndAssertSequence(reminder2, 2u);

			var reminder3ID = parent.PK.ToString() + "0 blah";
			var reminder3 = new Reminder(reminder3ID, parent.PK, new ZString(parent.TableName), DateTimeKind.Local, ZDateTime.Now, ZDateTime.Now, "", "");
			reminder3.Recipients.Add("me", "me@you.com");
			reminder3.CreateAppointment();
			AssertNotNullOrEmpty(GetLogForIdentifiers(parent.PK, reminder3ID).SL_Table);
		}

		public void TestDeliveringReminderYieldsSequentialSequenceNumbers_CreateReminderWithFactory()
		{
			var parentFactory = new BusinessObjectFactory();
			var parent = parentFactory.New<DummyWithWorkflow>();
			parent.Logs.AddNew(Events.QuotationAccepted);

			Assert("Making Logs collection subscribed to DataRefreshBus", parent.Logs.GetAllLogs().IsLoaded);

			var reminder1ID = parent.PK.ToString() + "Blah blah blah";
			var reminder1 = new Reminder(reminder1ID, parent.PK, new ZString(parent.TableName), DateTimeKind.Local, ZDateTime.Now, ZDateTime.Now, "", "", "", null, parentFactory);
			reminder1.Recipients.Add("me", "me@you.com");

			AssertNull("Precondition: should be no matching log", GetLogForIdentifiers(parent.PK, reminder1ID));

			// sequence doesn't go up by the number of recipients because log parent was not saved to database
			CreateAppointmentAndAssertSequence(reminder1, 0u);
			CreateAppointmentAndAssertSequence(reminder1, 0u);
			CreateAppointmentAndAssertSequence(reminder1, 0u);

			Assert("Appointment reference logs didn't save in isolated factory; Parent.HasChanges affected", parent.HasChanges);

			parentFactory.Save();
			var reminder2ID = parent.PK.ToString() + "only one blah";
			var reminder2 = new Reminder(reminder2ID, parent.PK, new ZString(parent.TableName), DateTimeKind.Local, ZDateTime.Now, ZDateTime.Now, "", "", "", null, parentFactory);
			reminder2.Recipients.Add("me", "me@you.com");

			AssertNull("Should not match same log as reminder1", GetLogForIdentifiers(parent.PK, reminder2ID));

			// sequence goes up by the number of recipients
			CreateAppointmentAndAssertSequence(reminder2, 0u);
			CreateAppointmentAndAssertSequence(reminder2, 1u);
			CreateAppointmentAndAssertSequence(reminder2, 2u);

			var reminder3ID = parent.PK.ToString() + "0 blah";
			var reminder3 = new Reminder(reminder3ID, parent.PK, new ZString(parent.TableName), DateTimeKind.Local, ZDateTime.Now, ZDateTime.Now, "", "", "", null, parentFactory);
			reminder3.Recipients.Add("me", "me@you.com");
			reminder3.CreateAppointment();
			AssertNotNullOrEmpty(GetLogForIdentifiers(parent.PK, reminder3ID).SL_Table);
		}

		void CreateAppointmentAndAssertSequence(Reminder reminder, uint expectedSequence)
		{
			reminder.CreateAppointment();
			var actualSequence = reminder.Sequence;
			AssertEquals(string.Format("Expected sequence number to be {0} but was {1}", expectedSequence, actualSequence), expectedSequence, actualSequence);
		}

		StmALog GetLogForIdentifiers(ZGuid parentPK, string reminderID)
		{
			var filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Reminder.Constants.EventCode);
			filter.AddToFilter(new ZQuery(StmALogSchema.SL_Parent, parentPK));
			filter.AddToFilter(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, Reminder.Constants.ReferencePrefix + reminderID));
			return Factory.LoadTop1<StmALog>(filter);
		}
	}
}
