using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class SingleMessageManagerNonInheritedTestCase : TestCaseWithFactory
	{
		public void TestRequiresAmendmentIfNotInDatabase()
		{
			AssertEquals(false, MessageManagerThatIsNotInDatabase.RequiresAmendment());
		}

		public void TestRequiresAmendmentIfNotDeclared()
		{
			AssertEquals(false, MessageManagerThatIsNotDeclared.RequiresAmendment());
		}

		public void TestRequiresAmendmentWithDifferentNumberOfMessages()
		{
			AssertEquals(true, MessageManagerThatReturnsDifferentNumberOfMessages.RequiresAmendment());
		}

		public void TestRequiresAmendmentWithDifferences()
		{
			AssertEquals(true, MessageManagerThatHasDifferences.RequiresAmendment());
		}

		public void TestRequiresAmendmentWithNoDifferences()
		{
			AssertEquals(false, MessageManagerThatHasNoDifferences.RequiresAmendment());
		}

		public void TestRequiresAmendmentDoesntPersistAnyMessages()
		{
			SingleMessageManager messageManager = MessageManagerThatHasDifferences;
			EDIMessage[] preTestMessages = (EDIMessage[])messageManager.BusinessObject.Factory.Load(typeof(EDIMessage), new ZQuery());
			messageManager.RequiresAmendment();
			EDIMessage[] postTestMessages = (EDIMessage[])messageManager.BusinessObject.Factory.Load(typeof(EDIMessage), new ZQuery());
			AssertEquals("PostTestMessages", preTestMessages.Length, postTestMessages.Length);
		}

		class TestDummyBusinessObjectWithInterface : DummyBusinessObject, IMayRequireAmendment
		{
			public TestDummyBusinessObjectWithInterface(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool MayRequireAmendment
			{
				get;
				set;
			}
			protected override DummyBizoValidation GetNewValidation()
			{
				return new ValidationDummyValidation(this);
			}
		}

		class ValidationDummyValidation : DummyBizoValidation
		{
			public ValidationDummyValidation(TestDummyBusinessObjectWithInterface parent) : base(parent) { }

			protected override void CheckZ0_Code()
			{
				base.CheckZ0_Code();
				Parent.Z0_CodeInfo.AddMessageError("Message Error");
			}
		}

		public void TestRequiresAmendmentUtilizesIMayRequireAmendmentInterfaceForPerformance()
		{
			TestDummyBusinessObjectWithInterface dummy = Factory.New<TestDummyBusinessObjectWithInterface>();
			dummy.FillWithValidTestData();
			Factory.Save();

			TestHelperSingleMessageManagerForTest messageManager = new TestHelperSingleMessageManagerForTest(dummy, "Name");
			messageManager.ReturnDifferentNumberOfMessages = false;
			messageManager.ReturnDifferentOriginalMessages = true;

			dummy.MayRequireAmendment = false;
			AssertEquals("RequiresAmendmentCore", false, messageManager.RequiresAmendmentCore());

			dummy.MayRequireAmendment = true;
			AssertEquals("RequiresAmendmentCore", true, messageManager.RequiresAmendmentCore());

			messageManager.ReturnDifferentOriginalMessages = false;
			AssertEquals("RequiresAmendmentCore", false, messageManager.RequiresAmendmentCore());
		}

		public void TestResetToOriginalWithLog()
		{
			DummyEnterpriseBusinessObject cuckooSqueaker = Factory.New<DummyEnterpriseBusinessObject>();
			AssertEquals("We have no logs yet", 0, cuckooSqueaker.GetLogs().GetAllLogs().Count);
			MessageManager.ResetToOriginalWithLog(cuckooSqueaker);
			AssertEquals("Cuckoo Squeaker now has a log", 1, cuckooSqueaker.GetLogs().GetAllLogs().Count);
			AssertEquals(MessageManager.MessageFriendlyName + " (Reset Original Completed after user acknowledged responsibility)", cuckooSqueaker.GetLogs().GetAllLogs()[0].SL_Reference);
		}

		public void TestReallyLongMessageFriendlyName()
		{
			DummyEnterpriseBusinessObject cuckooSqueaker = Factory.New<DummyEnterpriseBusinessObject>();
			AssertEquals("We have no logs yet", 0, cuckooSqueaker.GetLogs().GetAllLogs().Count);
			new TestHelperSingleMessageManagerForTest(cuckooSqueaker, @"This is a long string that will overflow the maximum length of the message manager log posting 01234567890
This is a long string that will overflow the maximum length of the message manager log posting 01234567890
This is a long string that will overflow the maximum length of the message manager log posting 01234567890
This is a long string that will overflow the maximum length of the message manager log posting 01234567890
This is a long string that will overflow the maximum length of the message manager log posting 01234567890
This is a long string that will overflow the maximum length of the message manager log posting 01234567890
This is a long string that will overflow the maximum length of the message manager log posting 01234567890
This is a long string that will overflow the maximum length of the message manager log posting 01234567890
This is a long string that will overflow the maximum length of the message manager log posting 01234567890
This is a long string that will overflow the maximum length of the message manager log posting 01234567890
This is a long string that will overflow the maximum length of the message manager log posting 01234567890
This is a long string that will overflow the maximum length of the message manager log posting 01234567890").ResetToOriginalWithLog(cuckooSqueaker);
			AssertEquals("Cuckoo Squeaker now has a log", 1, cuckooSqueaker.GetLogs().GetAllLogs().Count);
			AssertEquals(@"This is a long string that will overflow the maximum length of the message manager log posting 01234567890
This is a long string that will overflow the maximum length of the message manager log posting 01234567890
This is a long string that will overflow the maximum length of the message manager log posting 01234567890
This is a long string that will overflow the maximum length of the message manager log posting 01234567890
This is a long string that will overflow the maximum length of the message manager log posting 01234567890
This is a long string that will overflow the maximum length of the message manager log posting 01234567890
This is a long string that will overflow the maximum length of the message manager log posting 01234567890
This is a long string that will overflow the maximum length of the message manager log posting 01234567890
This is a long string that will overflow the maximum length of the message manager log posting (Reset Original Completed after user acknowledged responsibility)", cuckooSqueaker.GetLogs().GetAllLogs()[0].SL_Reference);
		}

		public void TestGetCommonNotificationsForSendingContainsMessageErrorsAsWarnings()
		{
			AssertEquals(false, MessageManager.GetCommonNotificationsForSending().ContainsWarning());
			MessageManager.AddMessageError();
			AssertEquals(true, MessageManager.GetCommonNotificationsForSending().ContainsWarning());
		}

		public void TestGetCommonNotificationsForSendingContainsErrorsAsErrors()
		{
			AssertEquals(false, MessageManager.GetCommonNotificationsForSending().ContainsError());
			MessageManager.AddError();
			AssertEquals(true, MessageManager.GetCommonNotificationsForSending().ContainsError());
		}

		public void TestGetNotificationsForSendingAnOriginalIncludesCommonReasons()
		{
			AssertEquals(false, MessageManager.GetNotificationsForSendingAnOriginal().ContainsError());
			MessageManager.AddError();
			AssertEquals(true, MessageManager.GetNotificationsForSendingAnOriginal().ContainsError());
		}

		public void TestShouldSendMessagesInTestMode()
		{
			AssertEquals(false, MessageManager.GetCommonNotificationsForSending().ContainsWarning());
			MessageManager.shouldSendMessagesInTestMode = true;
			MessageSendingNotificationCollection coll = MessageManager.GetCommonNotificationsForSending();
			AssertEquals(true, coll.ContainsWarning());
			AssertEquals(1, coll.Count);
			AssertEquals(MessageSendingValidation.WarningAndConfirmationWhenInTestModeText, coll[0].Message);
		}

		public void TestQueryForSending()
		{
			TestHelperSingleMessageManagerForTest manager = new TestHelperSingleMessageManagerForTest();
			MessageSendingQuery queryResult = new MessageSendingQuery();
			foreach (MessageSendingQuery query in manager.GetQueriesForSending())
			{
				queryResult = query;
			}
			AssertEquals("Do you think cuckoo squeakers are awesome?", queryResult.Question);
			AssertEquals("Do you not want to delay the cuckoo squeaker?", queryResult.Caption);
			Assert("Hasn't been called yet", !manager.QueryActionWasHit);
			queryResult.Delegate(true);
			Assert("Has been called", manager.QueryActionWasHit);
		}

		public void TestPreventSendBool()
		{
			TestHelperSingleMessageManagerForTest manager = new TestHelperSingleMessageManagerForTest();
			manager.PreventSendTestBool = true;
			Assert(manager.PreventSend);
			manager.PreventSendTestBool = false;
			Assert(!manager.PreventSend);
		}

		public void TestGetNotificationsForSendingAReplacementIncludesCommonReasons()
		{
			AssertEquals(false, MessageManager.GetNotificationsForSendingAReplacement().ContainsError());
			MessageManager.AddError();
			AssertEquals(true, MessageManager.GetNotificationsForSendingAReplacement().ContainsError());
		}

		public void TestGetNotificationsForSendingAWithdrawalIncludesCommonReasons()
		{
			AssertEquals(false, MessageManager.GetNotificationsForSendingAWithdrawal().ContainsError());
			MessageManager.AddError();
			AssertEquals(true, MessageManager.GetNotificationsForSendingAWithdrawal().ContainsError());
		}

		public void TestHasActiveMessagesReturnsNotCanSendOriginal()
		{
			MessageManager.canSendOriginal = true;
			AssertEquals("HasActiveMessages", false, MessageManager.HasActiveMessages);
			MessageManager.canSendOriginal = false;
			AssertEquals("HasActiveMessages", true, MessageManager.HasActiveMessages);
		}

		public void TestGetBusinessObjectInNewFactory()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			Factory.Save();
			DummyBusinessObject dummy2 = (DummyBusinessObject)MessageManager.GetBusinessObjectInNewFactory(dummy);
			AssertEquals(dummy.PK, dummy2.PK);
			AssertEquals("Factorys Different", true, dummy.Factory != dummy2.Factory);
		}

		public void TestShouldSendDeveloperExceptionForFalsePositiveCore()
		{
			TestHelperSingleMessageManagerForTest manager = new TestHelperSingleMessageManagerForTest();
			AssertEquals("Same message. doesnt need to send a silent exception", false, manager.ShouldSendDeveloperExceptionForFalsePositiveCore("A", "A"));
			AssertEquals("Different message.", true, manager.ShouldSendDeveloperExceptionForFalsePositiveCore("A", "B"));
		}

		public void TestAmendmentMessagesSentWithErrorAreFlagged()
		{
			AssertSendWithMessageErrors_ReValidate(
				() => MessageManagerInDatabase.GenerateAmendmentMessages(MessageManagerInDatabase.BusinessObject),
				() => MessageManagerInDatabase.OnAmendmentSent(true));

			AssertSendWithMessageErrors_NoValidate(
				() => MessageManager.GenerateAmendmentMessages(MessageManager.BusinessObject),
				() => MessageManager.OnAmendmentSent());
		}

		public void TestOriginalMessagesSentWithErrorAreFlagged()
		{
			AssertSendWithMessageErrors_ReValidate(
				() => MessageManagerInDatabase.GenerateOriginalMessages(MessageManagerInDatabase.BusinessObject),
				() => MessageManagerInDatabase.OnOriginalSent(true));

			AssertSendWithMessageErrors_NoValidate(
				() => MessageManager.GenerateOriginalMessages(MessageManager.BusinessObject),
				() => MessageManager.OnOriginalSent());
		}

		public void TestWithdrawalMessagesSentWithErrorAreFlagged()
		{
			AssertSendWithMessageErrors_ReValidate(
				() => MessageManagerInDatabase.GenerateWithdrawalMessages(MessageManagerInDatabase.BusinessObject),
				() => MessageManagerInDatabase.OnWithdrawalSent(true));
			AssertSendWithMessageErrors_NoValidate(
				() => MessageManager.GenerateWithdrawalMessages(MessageManager.BusinessObject),
				() => MessageManager.OnWithdrawalSent());
		}

		public void TestOverdueCargoReportExceptionReturnsNullInBaseManager()
		{
			AssertNull("Base manager should return null", MessageManager.OverdueCargoReportException);
		}

		#region Implementation

		TestHelperSingleMessageManagerForTest MessageManagerThatIsNotInDatabase
		{
			get { return new TestHelperSingleMessageManagerForTest(); }
		}

		TestHelperSingleMessageManagerForTest MessageManagerThatIsNotDeclared
		{
			get
			{
				TestHelperSingleMessageManagerForTest result = MessageManagerThatIsNotInDatabase;
				result.BusinessObject.Factory.Save();
				result.canSendWithdrawal = false;
				return result;
			}
		}

		TestHelperSingleMessageManagerForTest MessageManagerThatReturnsDifferentNumberOfMessages
		{
			get
			{
				TestHelperSingleMessageManagerForTest result = MessageManagerThatIsNotDeclared;
				result.canSendWithdrawal = true;
				result.ReturnDifferentNumberOfMessages = true;
				return result;
			}
		}

		TestHelperSingleMessageManagerForTest MessageManagerThatHasDifferences
		{
			get
			{
				TestHelperSingleMessageManagerForTest result = MessageManagerThatReturnsDifferentNumberOfMessages;
				result.ReturnDifferentNumberOfMessages = false;
				result.ReturnDifferentOriginalMessages = true;
				return result;
			}
		}

		TestHelperSingleMessageManagerForTest MessageManagerThatHasNoDifferences
		{
			get
			{
				TestHelperSingleMessageManagerForTest result = MessageManagerThatHasDifferences;
				result.ReturnDifferentOriginalMessages = false;
				return result;
			}
		}

		TestHelperSingleMessageManagerForTest messageManager;
		TestHelperSingleMessageManagerForTest MessageManager
		{
			get
			{
				if (messageManager == null)
				{
					messageManager = new TestHelperSingleMessageManagerForTest();
				}
				return messageManager;
			}
		}

		TestHelperSingleMessageManagerForTest MessageManagerInDatabase
		{
			get
			{
				if (messageManagerInDatabase == null)
				{
					TestDummyBusinessObjectWithInterface dummy = Factory.New<TestDummyBusinessObjectWithInterface>();
					dummy.FillWithValidTestData();
					Factory.Save();

					messageManagerInDatabase = new TestHelperSingleMessageManagerForTest(dummy, "Name");
				}
				return messageManagerInDatabase;
			}
		}
		TestHelperSingleMessageManagerForTest messageManagerInDatabase;

		void AssertSendWithMessageErrors_ReValidate(Func<EDIMessage[]> generateMessages, Action onSent)
		{
			CombineAssertions("Re-run validate after messages sent", () =>
			{
				var messages = generateMessages.Invoke();
				AssertNotEquals("Messages generated", 0, messages.Length);
				onSent.Invoke();
				foreach (var message in messages)
				{
					AssertEquals(FormattableString.Invariant($"{message.EM_MessageNum} Send with Message Errors"), true, message.EM_SendWithMessageErrors);
				}
			});
		}

		void AssertSendWithMessageErrors_NoValidate(Func<EDIMessage[]> generateMessages, Action onSent)
		{
			CombineAssertions("no run validate after messages sent", () =>
			{
				MessageManager.BusinessObject.AddRowMessageError("dummy error");
				AssertEquals("Error Notifications", true, MessageManager.MessageErrorNotificationCollector.Any());
				var messages = generateMessages.Invoke();
				AssertNotEquals("Messages generated", 0, messages.Length);
				onSent.Invoke();
				foreach (var message in messages)
				{
					AssertEquals(FormattableString.Invariant($"{message.EM_MessageNum} Send with Message Errors"), true, message.EM_SendWithMessageErrors);
				}
			});
		}

		#endregion
	}

	class TestHelperSingleMessageManagerForTest : TestHelperSingleMessageManager
	{
		public TestHelperSingleMessageManagerForTest()
		{
		}

		public TestHelperSingleMessageManagerForTest(DummyBusinessObject dummyBusinessObject, string messageFriendlyName) : base(dummyBusinessObject, messageFriendlyName)
		{
		}

		new internal bool RequiresAmendmentCore() => base.RequiresAmendmentCore();
		new internal bool ShouldSendDeveloperExceptionForFalsePositiveCore(ZString factoryMessages, ZString databaseMessages) => base.ShouldSendDeveloperExceptionForFalsePositiveCore(factoryMessages, databaseMessages);
	}
}
