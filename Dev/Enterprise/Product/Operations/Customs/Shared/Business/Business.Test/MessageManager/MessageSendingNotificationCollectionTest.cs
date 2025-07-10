using System.Collections;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	class MessageSendingNotificationCollectionTest : TestCase
	{
		public void TestConstructor()
		{
			AssertNotNull(Collection);
		}

		public void TestIEnumrable()
		{
			AssertNotNull(((IEnumerable)Collection).GetEnumerator());
		}

		public void TestAdd()
		{
			Collection.Add(Error);
			AssertEquals("Count", 1, Collection.Count);
		}

		public void TestAddWarning()
		{
			AssertEquals("Precondition: Collection.WarningCount", 0, Collection.WarningCount);
			Collection.AddWarning("Warning");
			AssertEquals("Collection.WarningCount", 1, Collection.WarningCount);
		}

		public void TestAddError()
		{
			AssertEquals("Precondition: Collection.ErrorCount", 0, Collection.ErrorCount);
			Collection.AddError("Error");
			AssertEquals("Collection.ErrorCount", 1, Collection.ErrorCount);
		}

		public void TestAddInformation()
		{
			AssertEquals("Precondition: Collection.InformationCount", 0, Collection.InformationCount);
			Collection.AddInformation("Information");
			AssertEquals("Collection.InformationCount", 1, Collection.InformationCount);
		}

		public void TestAddRange()
		{
			MessageSendingNotificationCollection secondCollection = new MessageSendingNotificationCollection();
			secondCollection.Add(Error);
			secondCollection.Add(Warning);
			Collection.AddRange(secondCollection);
			AssertEquals("Count", 2, Collection.Count);
		}

		public void TestIndex()
		{
			Collection.Add(Error);
			AssertEquals(Error, Collection[0]);
		}

		public void TestCount()
		{
			AssertEquals("Count", 0, Collection.Count);
			Collection.Add(Error);
			AssertEquals("Count", 1, Collection.Count);
		}

		public void TestContainsError()
		{
			AssertEquals("ContainsError", false, Collection.ContainsError());
			Collection.Add(Error);
			AssertEquals("ContainsError", true, Collection.ContainsError());
		}

		public void TestContainsWarning()
		{
			AssertEquals("ContainsWarning", false, Collection.ContainsWarning());
			Collection.Add(Warning);
			AssertEquals("ContainsWarning", true, Collection.ContainsWarning());
		}

		public void TestContainsInformation()
		{
			AssertEquals("ContainsWarning", false, Collection.ContainsInformation());
			Collection.Add(Information);
			AssertEquals("ContainsWarning", true, Collection.ContainsInformation());
		}

		public void TestNotificationsAsString()
		{
			AssertEquals("NotificationsAsString", "", Collection.NotificationsAsString());
			Collection.Add(Warning);
			AssertEquals("NotificationsAsString", Warning.Message + "\r\n", Collection.NotificationsAsString());
			Collection.Add(Warning);
			Collection.Add(Warning);
			Collection.Add(Warning);
			AssertEquals("NotificationsAsString", Warning.Message + "\r\n" + Warning.Message + "\r\n" + Warning.Message + "\r\n" + Warning.Message + "\r\n", Collection.NotificationsAsString());
			AssertEquals("NotificationsAsString", Warning.Message + "\r\n" + Warning.Message + "\r\n" + Warning.Message + "\r\nMore notifications (not listed)...\r\n", Collection.NotificationsAsString(3));
		}

		public void TestErrorCount()
		{
			AssertEquals("ErrorCount", 0, Collection.ErrorCount);
			Collection.Add(Warning);
			AssertEquals("ErrorCount", 0, Collection.ErrorCount);
			Collection.Add(Error);
			AssertEquals("ErrorCount", 1, Collection.ErrorCount);
			Collection.Add(Information);
			AssertEquals("ErrorCount", 1, Collection.ErrorCount);
		}

		public void TestWarningCount()
		{
			AssertEquals("WarningCount", 0, Collection.WarningCount);
			Collection.Add(Error);
			AssertEquals("WarningCount", 0, Collection.WarningCount);
			Collection.Add(Warning);
			AssertEquals("WarningCount", 1, Collection.WarningCount);
			Collection.Add(Information);
			AssertEquals("WarningCount", 1, Collection.WarningCount);
		}

		public void TestInformationCount()
		{
			AssertEquals("InformationCount", 0, Collection.InformationCount);
			Collection.Add(Warning);
			AssertEquals("InformationCount", 0, Collection.InformationCount);
			Collection.Add(Error);
			AssertEquals("InformationCount", 0, Collection.InformationCount);
			Collection.Add(Information);
			AssertEquals("InformationCount", 1, Collection.InformationCount);
		}

		public void TestCreateSummaryNotification()
		{
			var col = new MessageSendingNotificationCollection();

			AssertNull("No msgs = null summary", col.CreateSummaryNotification());
			col.AddError("Not Null test");
			AssertNotNull("Should have a msg", col.CreateSummaryNotification());

			CombineAssertions("Summary Types", () =>
			{
				col.Clear();
				col.AddInformation("Info");
				AssertType<MessageSendingInformation>("Only Info", col.CreateSummaryNotification());
				col.AddWarning("Warning");
				AssertType<MessageSendingWarning>("Warn & Info", col.CreateSummaryNotification());
				col.AddError("error");
				AssertType<MessageSendingError>("Err, Warn & Info", col.CreateSummaryNotification());
			});

			CombineAssertions("Msg Content", () =>
			{
				col.Clear();
				col.AddInformation("Info msg 1");
				AssertEquals("1 Info msg", "Info msg 1", col.CreateSummaryNotification().Message);
				col.AddInformation("Info msg 2");
				AssertEquals("2 Info msg", "Info msg 1\r\nInfo msg 2", col.CreateSummaryNotification().Message);
				col.AddWarning("Warn msg 1");
				AssertEquals("1-W 2-I msg", "Warnings:\r\nWarn msg 1\r\nInformation:\r\nInfo msg 1\r\nInfo msg 2", col.CreateSummaryNotification().Message);
				col.AddError("Err msg 1");
				AssertEquals("1-E 1-W 2-I msg", "Errors:\r\nErr msg 1\r\nWarnings:\r\nWarn msg 1\r\nInformation:\r\nInfo msg 1\r\nInfo msg 2", col.CreateSummaryNotification().Message);
			});
		}

		public void TestCreateSummaryNotificationWithIncludeErrorsAloneFlag()
		{
			var col = new MessageSendingNotificationCollection();
			col.AddWarning("Warn msg 1");
			col.AddInformation("Info msg 1");
			CombineAssertions("No Errors", () =>
			{
				AssertEquals("IncludeErrorsAlone = TRUE", "Warnings:\r\nWarn msg 1\r\nInformation:\r\nInfo msg 1", col.CreateSummaryNotification(true).Message);
				AssertEquals("IncludeErrorsAlone = FALSE", "Warnings:\r\nWarn msg 1\r\nInformation:\r\nInfo msg 1", col.CreateSummaryNotification(false).Message);
			});
			col.AddError("Err msg 1");
			CombineAssertions("With Errors", () =>
			{
				AssertEquals("IncludeErrorsAlone = TRUE", "Err msg 1", col.CreateSummaryNotification(true).Message);
				AssertEquals("IncludeErrorsAlone = FALSE", "Errors:\r\nErr msg 1\r\nWarnings:\r\nWarn msg 1\r\nInformation:\r\nInfo msg 1", col.CreateSummaryNotification(false).Message);
			});
		}

		#region Implementation

		MessageSendingNotificationCollection collection;
		MessageSendingNotificationCollection Collection => collection ?? (collection = new MessageSendingNotificationCollection());

		MessageSendingWarning warning;
		MessageSendingWarning Warning => warning ?? (warning = new MessageSendingWarning("message"));

		MessageSendingError error;
		MessageSendingError Error => error ?? (error = new MessageSendingError("message"));

		MessageSendingInformation information;
		MessageSendingInformation Information => information ?? (information = new MessageSendingInformation("message"));

		#endregion

	}
}
