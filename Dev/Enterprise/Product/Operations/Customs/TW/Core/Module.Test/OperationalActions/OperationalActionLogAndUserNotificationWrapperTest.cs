using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.MessageManagers.Testing;
using Enterprise.Customs.GUI.DocumentSending;
using Enterprise.Customs.TW.Module.OperationalActions;
using Enterprise.Services.OperationalActions.Support.Testing;

namespace Enterprise.Customs.TW.Module.Testing
{
	public sealed class OperationalActionLogAndUserNotificationWrapperTest : TestCaseWithFactory
	{
		public void TestOperationalActionLogAndUserNotificationWrapper()
		{
			var messageNotificationCollector = new MessageNotificationCollector();
			var testUserNotification = new TestUserNotification();
			var testOperationalLog = new DummyOperationalActionSectionLog();
			AssertEquals(0, testOperationalLog.messages.Count);
			var testTarget = new OperationalActionLogAndUserNotificationWrapper(testUserNotification, messageNotificationCollector, testOperationalLog, false, false);
			CombineAssertions("TestShowWarning", () =>
			{
				testTarget.ShowWarning("TestWarningMessage", "TestWarningCaption");
				AssertEquals("TestWarningMessage", testUserNotification.LastMessage);
				AssertEquals(1, testOperationalLog.messages.Count);
				AssertEquals("WARNING: TestWarningMessage", testOperationalLog.messages.Last());
			}

			);
			CombineAssertions("TestShowInformation", () =>
			{
				testTarget.ShowInformation("TestInformationMessage", "TestInformationCaption");
				AssertEquals("TestInformationMessage", testUserNotification.LastMessage);
				AssertEquals(2, testOperationalLog.messages.Count);
				AssertEquals("INFO: TestInformationMessage", testOperationalLog.messages.Last());
			}

			);
			CombineAssertions("TestShowError", () =>
			{
				testTarget.ShowError("TestErrorMessage", "TestErrorCaption");
				AssertEquals("TestErrorMessage", testUserNotification.LastMessage);
				AssertEquals(3, testOperationalLog.messages.Count);
				AssertEquals("ERROR: TestErrorMessage", testOperationalLog.messages[2]);
			}

			);
			CombineAssertions("TestShowQuestion", () =>
			{
				testUserNotification.NextTextAnswer = "TestAnswer";
				testTarget.ShowQuestion("TestQuestionMessage", "TestQuestionCaption", 1, null);
				AssertEquals("TestQuestionMessage", testUserNotification.LastMessage);
				AssertEquals(5, testOperationalLog.messages.Count);
				AssertEquals("INFO: [Awaiting User's Answer]:TestQuestionMessage", testOperationalLog.messages[3]);
				AssertEquals("INFO: [User's Answer]:TestAnswer", testOperationalLog.messages[4]);
			}

			);
			CombineAssertions("TestShowConfirmation", () =>
			{
				testUserNotification.NextAnswer = false;
				testTarget.ShowConfirmation("TestConfirmationMessage_1", "TestConfirmationCaption_1");
				AssertEquals("TestConfirmationMessage_1", testUserNotification.LastMessage);
				AssertEquals(7, testOperationalLog.messages.Count);
				AssertEquals("INFO: [Awaiting User's Confirmation]:TestConfirmationMessage_1", testOperationalLog.messages[5]);
				AssertEquals("INFO: [User's Answer]:No", testOperationalLog.messages[6]);
			}

			);
			CombineAssertions("TestShowConfirmation", () =>
			{
				testUserNotification.NextAnswer = true;
				testTarget.ShowConfirmation("TestConfirmationMessage_2", "TestConfirmationCaption_2");
				AssertEquals("TestConfirmationMessage_2", testUserNotification.LastMessage);
				AssertEquals(9, testOperationalLog.messages.Count);
				AssertEquals("INFO: [Awaiting User's Confirmation]:TestConfirmationMessage_2", testOperationalLog.messages[7]);
				AssertEquals("INFO: [User's Answer]:Yes", testOperationalLog.messages[8]);
			}

			);
			messageNotificationCollector = new MessageNotificationCollector();
			testUserNotification = new TestUserNotification();
			testOperationalLog = new DummyOperationalActionSectionLog();
			AssertEquals(0, testOperationalLog.messages.Count);
			testTarget = new OperationalActionLogAndUserNotificationWrapper(testUserNotification, messageNotificationCollector, testOperationalLog, true, true);
			CombineAssertions("TestShowWarning", () =>
			{
				testTarget.ShowWarning("TestWarningMessage", "TestWarningCaption");
				AssertEquals(null, testUserNotification.LastMessage);
				AssertEquals(0, testOperationalLog.messages.Count);
			}

			);
			CombineAssertions("TestShowInformation", () =>
			{
				testTarget.ShowInformation("TestInformationMessage", "TestInformationCaption");
				AssertEquals(null, testUserNotification.LastMessage);
				AssertEquals(1, testOperationalLog.messages.Count);
				AssertEquals("INFO: TestInformationMessage", testOperationalLog.messages.Last());
			}

			);
			CombineAssertions("TestShowError", () =>
			{
				testTarget.ShowError("TestErrorMessage", "TestErrorCaption");
				AssertEquals(null, testUserNotification.LastMessage);
				AssertEquals(2, testOperationalLog.messages.Count);
				AssertEquals("ERROR: TestErrorMessage", testOperationalLog.messages[1]);
			}

			);
			CombineAssertions("TestShowQuestion", () =>
			{
				testUserNotification.NextTextAnswer = "TestAnswer";
				testTarget.ShowQuestion("TestQuestionMessage", "TestQuestionCaption", 1, null);
				AssertEquals(null, testUserNotification.LastMessage);
				AssertEquals(4, testOperationalLog.messages.Count);
				AssertEquals("INFO: [Awaiting User's Answer]:TestQuestionMessage", testOperationalLog.messages[2]);
				AssertEquals("INFO: [User's Answer]:", testOperationalLog.messages[3]);
			}

			);
			CombineAssertions("TestShowConfirmation", () =>
			{
				testUserNotification.NextAnswer = false;
				testTarget.ShowConfirmation("TestConfirmationMessage_1", "TestConfirmationCaption_1");
				AssertEquals(null, testUserNotification.LastMessage);
				AssertEquals(6, testOperationalLog.messages.Count);
				AssertEquals("INFO: [Awaiting User's Confirmation]:TestConfirmationMessage_1", testOperationalLog.messages[4]);
				AssertEquals("INFO: [Automatic Answer]:Yes", testOperationalLog.messages[5]);
			}

			);
			CombineAssertions("TestShowConfirmation", () =>
			{
				testUserNotification.NextAnswer = true;
				testTarget.ShowConfirmation("TestConfirmationMessage_2", "TestConfirmationCaption_2");
				AssertEquals(null, testUserNotification.LastMessage);
				AssertEquals(8, testOperationalLog.messages.Count);
				AssertEquals("INFO: [Awaiting User's Confirmation]:TestConfirmationMessage_2", testOperationalLog.messages[6]);
				AssertEquals("INFO: [Automatic Answer]:Yes", testOperationalLog.messages[7]);
			}

			);
		}
	}
}
