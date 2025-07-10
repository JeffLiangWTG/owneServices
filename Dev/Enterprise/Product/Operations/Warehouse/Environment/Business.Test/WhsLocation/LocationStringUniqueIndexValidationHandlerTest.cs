using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	class LocationStringUniqueIndexValidationHandlerTest : TestCase
	{
		public void TestHandledUniqueIndexNames()
		{
			AssertContainsExactElementsInAnyOrder(["NR_UC__WhsLocationView_DoNotUse"], new LocationStringUniqueIndexValidationHandler("").HandledUniqueIndexNames);
		}

		public void TestNotifyUserAndAttemptToResolve()
		{
			var handler = new LocationStringUniqueIndexValidationHandler("Error!");
			var notifier = new TestNotifier();
			handler.NotifyUserAndAttemptToResolve(notifier, "RANDOMINDEX");
			AssertEquals(0, notifier.Messages.Count());

			handler.NotifyUserAndAttemptToResolve(notifier, "NR_UC__WhsLocationView_DoNotUse");
			var message = notifier.Messages.Single();
			AssertEquals("Duplicate Location Barcode Detected", message.Caption);
			AssertEquals("Error!", message.Message);
			AssertEquals(false, message.IsError);
		}

		class TestNotifier : INotificationHandler
		{
			public IEnumerable<(string Caption, string Message, bool IsError)> Messages => messages;
			readonly List<(string Caption, string Message, bool IsError)> messages = new();

			public void ReportError(string message, string caption, string errorContext = null, Exception exception = null)
			{
				messages.Add((caption, message, true));
			}

			public void ReportInformation(string message, string caption)
			{
				messages.Add((caption, message, false));
			}
		}
	}
}
