using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing.MessagingProcess.Helpers;

namespace Enterprise.Customs.Business.MessagingProcess.Testing
{
	sealed class CustomsMessengerExtensionsTest : TestCaseWithFactory
	{
		public void TestCreateMessage()
		{
			var msg = IntCusMessenger.CreateMessage();
			AssertNotNull("Msg created", msg);
			AssertEquals("Msg Text", "Hello World: (<<MSGNO PLACEHOLDER>>)", msg.EM_MessageText);
		}

		public void TestHasPlaceholderGenerator()
		{
			var genPlaceholder = new CustomsMessageWithPlaceholdersGeneratorImplForTest(Factory);

			ICustomsMessenger messengerWithPlaceholderGen = new CustomsMessengerImplForTest(bOWithMessages, genPlaceholder);

			AssertEquals("Normal Generator", false, IntCusMessenger.HasMessagePlaceholderGenerator());
			AssertEquals("Placeholder Generator", true, messengerWithPlaceholderGen.HasMessagePlaceholderGenerator());
		}

		public void TestOnEDIMessageSaving()
		{
			var genPlaceholder = new CustomsMessageWithPlaceholdersGeneratorImplForTest(Factory);

			ICustomsMessenger messengerWithPlaceholderGen = new CustomsMessengerImplForTest(bOWithMessages, genPlaceholder);

			var msg = genPlaceholder.GenerateMessage();

			messengerWithPlaceholderGen.OnEDIMessageSaving(msg);

			AssertEquals("Message updated (no EM_MessageNum before saving)", "Hello World: ()", msg.EM_MessageText);
		}

		public void TestRunCommonPreSendValidation()
		{
			var notifications = messenger.RunCommonPreSendValidation().ToList();
			AssertEquals("No notifications for default messenger", 0, notifications.Count);

			var processor = new PermitProcessorForTest();
			var permitMessenger = new CustomsMessengerWithPermitSupportImplForTest(bOWithMessages, processor);

			notifications = permitMessenger.RunCommonPreSendValidation().ToList();
			AssertEquals("No notifications for permitMessenger", 0, notifications.Count);
			AssertEquals("AddPermitRecordsAndLockMutexIfNeeded called", true, processor.AddPermitRecordsAndLockMutexIfNeededCalled);

			processor.ErrorsForTest = new ZString[] { "Msg1", "Msg2" };
			notifications = permitMessenger.RunCommonPreSendValidation().ToList();
			AssertEquals("2 Errors from permitMessenger", 2, notifications.Count);
			var combinedErrors = string.Join(", ", notifications.Select(x => x.MessageIncludingPrefix));
			AssertEquals("Errors", "Error: Msg1, Error: Msg2", combinedErrors);
		}

		public void TestProcessCommonUpdates()
		{
			var processor = new PermitProcessorForTest();
			var permitMessenger = new CustomsMessengerWithPermitSupportImplForTest(bOWithMessages, processor);
			permitMessenger.ProcessCommonUpdates(null);

			AssertEquals("AddPermitTransactions called", true, processor.AddPermitTransactionsCalled);
		}

		public void TestCleanUp()
		{
			var processor = new PermitProcessorForTest();
			var permitMessenger = new CustomsMessengerWithPermitSupportImplForTest(bOWithMessages, processor);
			permitMessenger.CleanUp();

			AssertEquals("UnlockMutexes called", true, processor.UnlockPermitMutexesCalled);
		}

		public void TestSignMessages()
		{
			var messengerWithSigning = new CustomsMessengerWithISupportMessageSigning(bOWithMessages);
			messengerWithSigning.SignMessagesForTest = (msgs) => "I dont want to sign these messages";

			CombineAssertions(() =>
			{
				var msgCollection = Array.Empty<Messaging.Business.EDIMessage>();
				var prevResult = new ActionResult(true);
				AssertEquals("Non-Signing Messenger", string.Empty, messenger.SignMessages(msgCollection, prevResult));
				AssertEquals("Signing Messenger with Error", "I dont want to sign these messages", messengerWithSigning.SignMessages(msgCollection, prevResult));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			bOWithMessages = Factory.New<DummyBizObjWithMessages>();
			msgGenerator = new CustomsMessageGeneratorImplForTest(Factory);
			messenger = new CustomsMessengerImplForTest(bOWithMessages, msgGenerator);
		}

		DummyBizObjWithMessages bOWithMessages;
		CustomsMessageGeneratorImplForTest msgGenerator;
		CustomsMessengerImplForTest messenger;
		ICustomsMessenger IntCusMessenger => messenger;
	}
}
