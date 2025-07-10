using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.MessagingProcess.Testing
{
	class ActionResultTest : TestCaseWithFactory
	{
		public void TestConstructors()
		{
			var ar = new ActionResult(true, new MessageSendingInformation[] { new MessageSendingInformation("First Message"), new MessageSendingInformation("Second Message") });
			AssertNotNull("AR", ar);
			AssertEquals("Success", true, ar.Success);
			AssertEquals("Has 2 Msgs", 2, ar.Notifications.Count);
			AssertEquals("1st Msg", "First Message", ar.Notifications[0].Message);
			AssertEquals("2nd Msg", "Second Message", ar.Notifications[1].Message);

			ar = new ActionResult(true);
			AssertNotNull("ar.Notifications", ar.Notifications);
			AssertEquals("Has No Msgs", 0, ar.Notifications.Count);

			ar = new ActionResult();
			AssertEquals("Success", false, ar.Success);
			AssertNotNull("ar.Notifications", ar.Notifications);
			AssertEquals("Has No Msgs", 0, ar.Notifications.Count);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructorWithNullArg()
		{
			MessageSendingInformation[] msgs = null;
			var ar = new ActionResult(true, msgs);
		}

		public void TestSuccess()
		{
			var ar = new ActionResult();

			CombineAssertions("Success Property", () =>
			{
				ar.Success = true;
				AssertEquals("True", true, ar.Success);
				ar.Success = false;
				AssertEquals("False", false, ar.Success);
			});
		}

		public void TestNotifications()
		{
			var ar = new ActionResult();

			var data = new MessageSendingNotificationCollection();
			CombineAssertions("Notifications Property", () =>
			{
				ar.Notifications = data;
				AssertEquals("With Values", data, ar.Notifications);
				ar.Notifications = null;
				AssertEquals("Set to Null", null, ar.Notifications);
			});
		}

		public void TestEDIMessages()
		{
			var ar = new ActionResult();

			var msgs = new List<EDIMessage>
			{
				Factory.New<EDIMessage>(),
				Factory.New<EDIMessage>()
			};

			CombineAssertions("EDIMessages Property", () =>
			{
				ar.EDIMessages = msgs;
				AssertEquals("With Values", msgs, ar.EDIMessages);
				ar.EDIMessages = null;
				AssertEquals("Set to Null", null, ar.EDIMessages);
			});
		}

		public void TestAppendErrorNotification()
		{
			var ar = new ActionResult();
			ar.AppendErrorNotification("New Error");
			AssertNotNull("ar.Notifications", ar.Notifications);
			AssertEquals("Has 1 Msg", 1, ar.Notifications.Count);
			AssertEquals("1st Msg", "New Error", ar.Notifications[0].Message);
			AssertEquals("Is Error", true, ar.Notifications[0].IsError);
		}

		public void TestAppendWarningNotification()
		{
			var ar = new ActionResult();
			ar.AppendWarningNotification("New Warning");
			AssertNotNull("ar.Notifications", ar.Notifications);
			AssertEquals("Has 1 Msg", 1, ar.Notifications.Count);
			AssertEquals("1st Msg", "New Warning", ar.Notifications[0].Message);
			AssertEquals("Is Warning", true, ar.Notifications[0].IsWarning);
		}

		public void TestAppendInformationNotification()
		{
			var ar = new ActionResult();
			ar.AppendInformationNotification("New Info");
			AssertNotNull("ar.Notifications", ar.Notifications);
			AssertEquals("Has 1 Msg", 1, ar.Notifications.Count);
			AssertEquals("1st Msg", "New Info", ar.Notifications[0].Message);
			AssertEquals("Is Info", true, ar.Notifications[0].IsInformation);
		}

		public void TestAppendNotification()
		{
			var ar = new ActionResult();

			ar.AppendInformationNotification("First Message");
			AssertNotNull("ar.Notifications", ar.Notifications);
			AssertEquals("Has 1 Msg", 1, ar.Notifications.Count);
			AssertEquals("1st Msg", "First Message", ar.Notifications[0].Message);

			ar.AppendInformationNotification("Second Message");
			AssertEquals("Has 2 Msgs", 2, ar.Notifications.Count);
			AssertEquals("2nd Msg", "Second Message", ar.Notifications[1].Message);
		}

		public void TestAppendNotifications()
		{
			var ar = new ActionResult();

			ar.AppendNotifications(new MessageSendingInformation[] { new MessageSendingInformation("First Message"), new MessageSendingInformation("Second Message") });
			AssertNotNull("ar.Notifications", ar.Notifications);
			AssertEquals("Has 2 Msg", 2, ar.Notifications.Count);
			AssertEquals("1st Msg", "First Message", ar.Notifications[0].Message);
			AssertEquals("2nd Msg", "Second Message", ar.Notifications[1].Message);

			ar.AppendNotifications(new MessageSendingInformation[] { new MessageSendingInformation("Third Message") });
			AssertEquals("Has 3 Msgs", 3, ar.Notifications.Count);
			AssertEquals("3rd Msg", "Third Message", ar.Notifications[2].Message);
		}

		public void TestUpdateSuccess()
		{
			var ar = new ActionResult(false);
			ar.UpdateSuccess();
			AssertEquals(false, ar.Success);

			ar = new ActionResult(true);
			ar.UpdateSuccess();
			AssertEquals(true, ar.Success);

			ar = new ActionResult(true) { Notifications = null };
			ar.UpdateSuccess();
			AssertEquals(true, ar.Success);

			ar = new ActionResult(true);
			ar.AppendInformationNotification("Should still be true with Info");
			ar.UpdateSuccess();
			AssertEquals(true, ar.Success);
			ar.AppendWarningNotification("Should still be true with Warning");
			ar.UpdateSuccess();
			AssertEquals(true, ar.Success);
			ar.AppendErrorNotification("Now no longer success after ERROR");
			ar.UpdateSuccess();
			AssertEquals(false, ar.Success);
		}

		public void TestContainsWarning()
		{
			var ar1 = new ActionResult(true, new MessageSendingNotification[] { new MessageSendingError("Error not Warning"), new MessageSendingInformation("Info not Warning") });
			var ar2 = new ActionResult(true);
			ar2.PreviousNotifications.Add(new MessageSendingError("Error not Warning"));
			ar2.PreviousNotifications.Add(new MessageSendingInformation("Info not Warning"));

			CombineAssertions("No Warnings", () =>
			{
				AssertEquals("AR1 Notifications", false, ar1.ContainsWarning());
				AssertEquals("AR2 Prev Notifications", false, ar2.ContainsWarning());
			});

			ar1.Notifications.Add(new MessageSendingWarning("Be warned"));
			ar2.PreviousNotifications.Add(new MessageSendingWarning("Be warned"));

			CombineAssertions("Has Warnings", () =>
			{
				AssertEquals("AR1 Notifications", true, ar1.ContainsWarning());
				AssertEquals("AR2 Prev Notifications", true, ar2.ContainsWarning());
			});
		}
	}
}
