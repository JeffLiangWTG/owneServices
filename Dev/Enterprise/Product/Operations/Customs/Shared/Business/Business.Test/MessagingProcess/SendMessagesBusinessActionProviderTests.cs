using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing.MessagingProcess.Helpers;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.Business.MessagingProcess.Testing
{
	sealed class SendMessagesBusinessActionProviderTests : TestCaseWithFactory
	{
		public void TestConstructorWithNullParams()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new SendMessagesBusinessActionProvider(null));
		}

		public void TestCommonPreSendValidation()
		{
			var prev = new ActionResult(true);
			var result = actionInterface.PreSendValidation(prev);

			AssertEquals("Success", true, result.Success);
			AssertEquals("No Notifications", 0, result.Notifications.Count);

			messagingProvider.IsInTestMode = true;
			messagingProvider.EnableTestModeValidation = true;

			result = actionInterface.PreSendValidation(prev);

			AssertEquals("Success", true, result.Success);
			AssertEquals("1 Notification", 1, result.Notifications.Count);
		}

		public void TestPreSendValidation()
		{
			var prev = new ActionResult(true);
			var result = actionInterface.PreSendValidation(prev);

			AssertEquals("Success", true, result.Success);
			AssertEquals("No Notifications", 0, result.Notifications.Count);

			var msg1 = new MessageSendingError("sim declaration msgs");

			messagingProvider.PreSendValidationMessagesForTest = new MessageSendingNotification[] { msg1 };

			result = actionInterface.PreSendValidation(prev);

			AssertEquals("Success", false, result.Success);
			AssertEquals("1 Notification", 1, result.Notifications.Count);
			AssertContainsExactElementsInAnyOrder(new MessageSendingError[] { msg1 }, result.Notifications);
		}

		public void TestMessengerCommonPreSendValidation()
		{
			var prev = new ActionResult(true);
			var result = actionInterface.PreSendValidation(prev);

			AssertEquals("Success", true, result.Success);
			AssertEquals("No Notifications", 0, result.Notifications.Count);
			AssertEquals("AddPermits called", true, permitProcessor.AddPermitRecordsAndLockMutexIfNeededCalled);

			permitProcessor.ErrorsForTest = new ZString[] { "Permit Msg 1" };

			result = actionInterface.PreSendValidation(prev);

			AssertEquals("Not successful", false, result.Success);
			AssertEquals("1 notification", 1, result.Notifications.Count);
		}

		public void TestPreSendValidationForBusinessObjects()
		{
			var topBO = Factory.New<DummyEnterpriseBusinessObject>();

			var messagingProvider = new CustomsMessagingProviderWithAdditionalValidationImplForTest(Factory, new[] { customsMessenger1 });
			var supporter = new CustomsMessagingSupporter(topBO, new CustomsMessagingProviderFactoryImplForTest(messagingProvider));
			ISendMessagesBusinessActionProvider baProvider = new SendMessagesBusinessActionProvider(supporter);

			var result = baProvider.PreSendValidation(new ActionResult(true));
			AssertEquals("Success", true, result.Success);
			AssertEquals("No Notifications", 0, result.Notifications.Count);

			topBO.Z0_Description = "message error";
			result = baProvider.PreSendValidation(new ActionResult(true));
			AssertEquals("Success", true, result.Success);
			AssertEquals("1 Notifications", 1, result.Notifications.Count);

			topBO.Z0_Description = "Something else";
			messagingProvider.LocalBO.Z0_Description = "message error";
			result = baProvider.PreSendValidation(new ActionResult(true));
			AssertEquals("Success", true, result.Success);
			AssertEquals("1 Notifications", 1, result.Notifications.Count);
		}

		public void TestCreateMessages()
		{
			var prev = new ActionResult(true);
			customsMessenger1.CreateMessageForTest = null;
			customsMessenger2.CreateMessageForTest = null;
			var result = actionInterface.CreateMessages(prev);

			AssertEquals("Success", false, result.Success);
			AssertEquals("1 Notifications", 1, result.Notifications?.Count ?? 0);
			AssertContains("No messages were created", result.Notifications[0].Message);

			var msg1 = Factory.New<EDIMessage>();
			var msg2 = Factory.New<EDIMessage>();

			customsMessenger1.CreateMessageForTest = msg1;
			customsMessenger2.CreateMessageForTest = msg2;

			prev = new ActionResult(true);
			result = actionInterface.CreateMessages(prev);

			AssertEquals("Success", true, result.Success);
			AssertEquals("0 Notifications", 0, result.Notifications?.Count ?? 0);
			AssertEquals("2 Messages", 2, result.EDIMessages?.Count ?? 0);
			AssertContainsExactElementsInAnyOrder(new[] { msg1, msg2 }, result.EDIMessages);
			AssertEquals("Msg1", childBO1.PK, msg1.EM_LinkUniqueID);
			AssertEquals("Captured", EDIMessage.Status.Captured, msg1.EM_Status);
			AssertEquals("Msg2", childBO2.PK, msg2.EM_LinkUniqueID);
		}

		public void TestCreateMessagesWithNotifications()
		{
			var msg1 = Factory.New<EDIMessage>();

			customsMessenger1.CreateMessageForTest = msg1;
			customsMessenger2.CreateMessageForTest = null;

			var prev = new ActionResult(true, new MessageSendingNotification[] { new MessageSendingWarning("We had some validation warning which we are ignoring") });

			AssertEquals("Pre-Req: 1 Notifications", 1, prev.Notifications?.Count ?? 0);

			var result = actionInterface.CreateMessages(prev);

			AssertEquals("Success", true, result.Success);
			AssertEquals("0 Notifications", 0, result.Notifications?.Count ?? 0);
			AssertEquals("1 Previous Notification", 1, result.PreviousNotifications?.Count ?? 0);
		}

		public void TestCreateMessageProperties_IsTestMessage()
		{
			var msg1 = Factory.New<EDIMessage>();

			customsMessenger1.CreateMessageForTest = msg1;
			customsMessenger2.CreateMessageForTest = null;

			CombineAssertions("Is in Test mode", () =>
			{
				var prev = new ActionResult(true);
				messagingProvider.IsInTestMode = true;

				var result = actionInterface.CreateMessages(prev);
				AssertEquals("Success", true, result.Success);
				AssertEquals("1 Messages", 1, result.EDIMessages?.Count ?? 0);

				AssertEquals("Msg Is Test", true, msg1.EM_IsTestMessage);
			});

			CombineAssertions("Is in Test mode", () =>
			{
				var prev = new ActionResult(true);
				messagingProvider.IsInTestMode = false;

				var result = actionInterface.CreateMessages(prev);
				AssertEquals("Success", true, result.Success);
				AssertEquals("1 Messages", 1, result.EDIMessages?.Count ?? 0);

				AssertEquals("Msg Is not Test", false, msg1.EM_IsTestMessage);
			});
		}

		public void TestCreateMessageProperties_SendMessageWithErrors()
		{
			var msg1 = Factory.New<EDIMessage>();

			customsMessenger1.CreateMessageForTest = msg1;
			customsMessenger2.CreateMessageForTest = null;

			var prev = new ActionResult(true);
			var result = actionInterface.CreateMessages(prev);
			AssertEquals("Success", true, result.Success);
			AssertEquals("1 Messages", 1, result.EDIMessages?.Count ?? 0);
			AssertEquals("No Errors", false, msg1.EM_SendWithMessageErrors);

			prev.PreviousNotifications.Add(new MessageSendingInformation("Just an info message, no warning"));
			result = actionInterface.CreateMessages(prev);
			AssertEquals("Success", true, result.Success);
			AssertEquals("1 Messages", 1, result.EDIMessages?.Count ?? 0);
			AssertEquals("No Errors", false, msg1.EM_SendWithMessageErrors);

			prev.PreviousNotifications.Add(new MessageSendingWarning("Warning message so sending with errors"));
			result = actionInterface.CreateMessages(prev);
			AssertEquals("Success", true, result.Success);
			AssertEquals("1 Messages", 1, result.EDIMessages?.Count ?? 0);
			AssertEquals("Sending with Errors", true, msg1.EM_SendWithMessageErrors);

			prev.PreviousNotifications.Clear();
			prev.PreviousNotifications.Add(new MessageSendingError("Error message so sending with errors"));
			result = actionInterface.CreateMessages(prev);
			AssertEquals("Success", true, result.Success);
			AssertEquals("1 Messages", 1, result.EDIMessages?.Count ?? 0);
			AssertEquals("Sending with Errors", true, msg1.EM_SendWithMessageErrors);
		}

		public void TestSignMessages()
		{
			var signingMessenger1 = new CustomsMessengerWithISupportMessageSigning(childBO1);
			var signingMessenger2 = new CustomsMessengerWithISupportMessageSigning(childBO2);
			var provider = new CustomsMessagingProviderAllImplForTest(new[] { signingMessenger1, signingMessenger2 });
			var messagingSupporter = new CustomsMessagingSupporter(topLevelBO, new CustomsMessagingProviderFactoryImplForTest(provider));
			boActionProvider = new SendMessagesBusinessActionProvider(messagingSupporter);
			var prev = new ActionResult(true);
			var result = actionInterface.SignMessages(prev);

			AssertEquals("Success", false, result.Success);
			AssertEquals("1 Notifications", 1, result.Notifications?.Count ?? 0);
			AssertContains("No messages to sign", result.Notifications[0].Message);

			var msg1 = Factory.New<EDIMessage>();
			msg1.EM_MessageText = "Hello";
			msg1.EM_LinkedObject = (signingMessenger1 as ICustomsMessenger).Owner.MessageOwner;
			var msg2 = Factory.New<EDIMessage>();
			msg2.EM_MessageText = "World";
			msg2.EM_LinkedObject = (signingMessenger2 as ICustomsMessenger).Owner.MessageOwner;

			prev = new ActionResult(true);
			prev.EDIMessages = new List<EDIMessage> { msg1, msg2 };

			Func<IEnumerable<EDIMessage>, ZString> signingFunc = (msgs) =>
			{
				foreach (var m in msgs)
				{
					m.EM_MessageText = m.EM_MessageText + " : SIGNED";
				}

				return ZString.Empty;
			};
			Func<IEnumerable<EDIMessage>, ZString> noSigningFunc = (msgs) => "No Signing today";

			signingMessenger1.SignMessagesForTest = signingFunc;
			signingMessenger2.SignMessagesForTest = signingFunc;

			result = actionInterface.SignMessages(prev);

			AssertEquals("Success", true, result.Success);
			AssertEquals("0 Notifications", 0, result.Notifications?.Count ?? 0);
			AssertEquals("Msgs exist", 2, result.EDIMessages.Count);
			AssertEquals("Msg1", "Hello : SIGNED", result.EDIMessages[0].EM_MessageText);
			AssertEquals("Msg2", "World : SIGNED", result.EDIMessages[1].EM_MessageText);

			signingMessenger1.SignMessagesForTest = noSigningFunc;

			prev = new ActionResult(true);
			prev.EDIMessages = new List<EDIMessage> { msg1, msg2 };
			result = actionInterface.SignMessages(prev);

			AssertEquals("Success", false, result.Success);
			AssertEquals("1 Notifications", 1, result.Notifications?.Count ?? 0);
			AssertEquals("Notifcation content", "No Signing today", result.Notifications[0].Message);
			AssertEquals("Deleted", false, msg1.IsDeleted);
		}

		public void TestPreviewDialogFailure()
		{
			var prev = new ActionResult(false);

			var msg1 = Factory.NewMoq<EDIMessage>();
			msg1.Object.EM_MessageText = "Hello";
			var msg2 = Factory.NewMoq<EDIMessage>();
			msg2.Object.EM_MessageText = "World";

			prev.EDIMessages = new List<EDIMessage> { msg1.Object, msg2.Object };

			var result = actionInterface.PreviewDialogFailure(prev);

			AssertEquals("Success", false, result.Success);
			AssertEquals("Deleted", true, msg1.Object.IsDeleted);
			AssertEquals("Deleted", true, msg2.Object.IsDeleted);
		}

		public void TestSignMessagesFailure()
		{
			var prev = new ActionResult(false);

			var msg1 = Factory.New<EDIMessage>();
			msg1.EM_MessageText = "Hello";
			var msg2 = Factory.New<EDIMessage>();
			msg2.EM_MessageText = "World";

			prev.EDIMessages = new List<EDIMessage> { msg1, msg2 };

			var result = actionInterface.SignMessagesFailure(prev);

			AssertEquals("Success", false, result.Success);
			AssertEquals("Deleted", true, msg1.IsDeleted);
			AssertEquals("Deleted", true, msg2.IsDeleted);
		}

		public void TestProcessUpdates()
		{
			var prev = new ActionResult(true) { EDIMessages = new List<EDIMessage> { } };

			customsMessenger1.ProcessUpdatesForTest = true;
			customsMessenger2.ProcessUpdatesForTest = true;

			var result = actionInterface.ProcessUpdates(prev);

			AssertEquals("Success", true, result.Success);
			AssertEquals("1 Notification", 1, result.Notifications?.Count ?? 0);
			AssertEquals("Notification", "0 Message(s) queued for sending", result.Notifications[0].Message);

			var msg1 = Factory.New<DummyEDIMessage_SendMessagesBusinessActionProviderTests>();
			msg1.Object.EM_LinkedObject = (customsMessenger1 as ICustomsMessenger).Owner.MessageOwner;
			msg1.GetMessageReferenceNumberReturns = "1";
			var msg2 = Factory.New<DummyEDIMessage_SendMessagesBusinessActionProviderTests>();
			msg2.Object.EM_LinkedObject = (customsMessenger2 as ICustomsMessenger).Owner.MessageOwner;
			msg2.GetMessageReferenceNumberReturns = "2";

			prev = new ActionResult(true) { EDIMessages = new List<EDIMessage> { msg1.Object, msg2.Object } };

			customsMessenger1.ProcessUpdatesForTest = false;
			customsMessenger2.ProcessUpdatesForTest = true;

			result = actionInterface.ProcessUpdates(prev);

			AssertEquals("Success", false, result.Success);
			AssertEquals("MessengerCommonUpdate called", true, permitProcessor.AddPermitTransactionsCalled);
			AssertEquals("Msg1 Discarded", EDIMessage.Status.Discarded, msg1.Object.EM_Status);
			AssertEquals("Msg2 Queued", EDIMessage.Status.Queued, msg2.Object.EM_Status);

			permitProcessor.AddPermitTransactionsCalled = false;
			customsMessenger1.ProcessUpdatesForTest = true;
			customsMessenger2.ProcessUpdatesForTest = false;

			result = actionInterface.ProcessUpdates(prev);
			AssertEquals("MessengerCommonUpdate not called", false, permitProcessor.AddPermitTransactionsCalled);

			prev = new ActionResult(true) { EDIMessages = new List<EDIMessage> { msg1.Object, msg2.Object } };

			customsMessenger1.ProcessUpdatesForTest = true;
			customsMessenger2.ProcessUpdatesForTest = true;

			result = actionInterface.ProcessUpdates(prev);

			AssertEquals("Success", true, result.Success);
			AssertEquals("1 Notification", 1, result.Notifications?.Count ?? 0);
			AssertEquals("Notification", "2 Message(s) queued for sending", result.Notifications[0].Message);
			AssertEquals("Msg1 saved", true, msg1.Object.IsInDatabase);
			AssertEquals("Msg2 saved", true, msg2.Object.IsInDatabase);
		}

		public void TestRefreshBusinessObjects()
		{
			(customsMessenger1 as ICustomsMessenger).Owner.Messages.Clear();
			var msg1 = Factory.New<DummyEDIMessage_SendMessagesBusinessActionProviderTests>();
			msg1.Object.EM_LinkedObject = (customsMessenger1 as ICustomsMessenger).Owner.MessageOwner;
			msg1.GetMessageReferenceNumberReturns = "1";

			var prev = new ActionResult(true) { EDIMessages = new List<EDIMessage> { msg1.Object } };

			AssertEquals("Msg count", 0, (customsMessenger1 as ICustomsMessenger).Owner.Messages.Count);
			var result = actionInterface.RefreshBusinessObjects(prev);

			AssertEquals("Success", true, result.Success);
			AssertEquals("Msg count", 1, (customsMessenger1 as ICustomsMessenger).Owner.Messages.Count);
			AssertEquals("Messenger cleanup called", true, permitProcessor.UnlockPermitMutexesCalled);
		}

		public void TestRefreshBusinessObjectsParentOwner()
		{
			var topBO = Factory.New<DummyBizObjWithNonDependentMessages>();
			var messengerBO = Factory.New<DummyBizObjWithMessages>();

			topBO.Child = messengerBO;

			(topBO as IEDIMessageCollectionOwner).Messages.Clear();
			(messengerBO as IEDIMessageCollectionOwner).Messages.Clear();

			var messenger = new CustomsMessengerImplForTest(messengerBO);
			var messagingProvider = new CustomsMessagingProviderImplForTest(new[] { messenger });
			var supporter = new CustomsMessagingSupporter(topBO, new CustomsMessagingProviderFactoryImplForTest(messagingProvider));
			ISendMessagesBusinessActionProvider provider = new SendMessagesBusinessActionProvider(supporter);

			var msg1 = Factory.New<DummyEDIMessage_SendMessagesBusinessActionProviderTests>();
			msg1.Object.EM_LinkedObject = (messenger as ICustomsMessenger).Owner.MessageOwner;
			msg1.GetMessageReferenceNumberReturns = "1";

			var prev = new ActionResult(true) { EDIMessages = new List<EDIMessage> { msg1.Object } };

			CombineAssertions("Pre-Req", () =>
			{
				AssertEquals("Msg count on Messenger", 0, (messenger as ICustomsMessenger).Owner.Messages.Count);
				AssertEquals("Msg count on Parent", 0, (topBO as IEDIMessageCollectionOwner).Messages.Count);
			});

			var result = provider.RefreshBusinessObjects(prev);

			CombineAssertions("Refreshed", () =>
			{
				AssertEquals("Success", true, result.Success);
				AssertEquals("Msg count on Messenger", 1, (messenger as ICustomsMessenger).Owner.Messages.Count);
				AssertEquals("Msg count on Parent", 1, (topBO as IEDIMessageCollectionOwner).Messages.Count);
			});
		}

		public void TestProcessUpdatesZSaveException()
		{
			customsMessenger1.ProcessUpdatesForTest = true;
			customsMessenger2.ProcessUpdatesForTest = true;

			var msg1 = Factory.New<DummyEDIMessage_SendMessagesBusinessActionProviderTests>();
			msg1.Object.EM_LinkedObject = (customsMessenger1 as ICustomsMessenger).Owner.MessageOwner;
			msg1.GetMessageReferenceNumberReturns = "1";
			var msg2 = Factory.New<DummyEDIMessage_SendMessagesBusinessActionProviderTests>();
			msg2.Object.EM_LinkedObject = (customsMessenger2 as ICustomsMessenger).Owner.MessageOwner;
			var row = ((INeedRow)topLevelBO).Row;
			msg2.GetMessageReferenceNumberShouldThrow = true;

			var prev = new ActionResult(true) { EDIMessages = new List<EDIMessage> { msg1.Object, msg2.Object } };
			var result = actionInterface.ProcessUpdates(prev);
			AssertEquals("Success", false, result.Success);
			AssertEquals("1 Notifications", 1, result.Notifications?.Count ?? 0);
			AssertEquals("Notifcation content", "An error was encountered while saving changes.", result.Notifications[0].Message);

			AssertEquals("Msg1 rolled back", false, msg1.Object.IsInDatabase);
		}

		public void TestProcessUpdatesForPlacehoderGenerator()
		{
			var genPlaceholder = new CustomsMessageWithPlaceholdersGeneratorImplForTest(Factory);
			ICustomsMessenger messengerWithPlaceholderGen = new CustomsMessengerImplForTest(childBO1, genPlaceholder);

			var genMessagingProvider = new CustomsMessagingProviderImplForTest(new[] { messengerWithPlaceholderGen });
			var genMessagingSupporter = new CustomsMessagingSupporter(topLevelBO, new CustomsMessagingProviderFactoryImplForTest(genMessagingProvider));
			ISendMessagesBusinessActionProvider actionProv = new SendMessagesBusinessActionProvider(genMessagingSupporter);

			var msg = genPlaceholder.GenerateMessage();
			msg.EM_LinkedObject = messengerWithPlaceholderGen.Owner.MessageOwner;
			var prev = new ActionResult(true) { EDIMessages = new List<EDIMessage> { msg } };

			var result = actionProv.ProcessUpdates(prev);
			AssertEquals("Success", true, result.Success);
			AssertEquals("1 Notification", 1, result.Notifications?.Count ?? 0);
			AssertEquals("Notification", "1 Message(s) queued for sending", result.Notifications[0].Message);
			AssertEquals("Msg1 saved", true, msg.IsInDatabase);
			AssertEquals("Message Placeholder updated", "Hello World: (123)", msg.EM_MessageText);
		}

		public void TestConfigureProcess()
		{
			var sendProc = new SendMessagesProcess();
			var sendChain = sendProc.SendProcessChain;

			var messagingProvider = new CustomsMessagingProviderAllImplForTest(new[] { customsMessenger1 });
			var supporter = new CustomsMessagingSupporter(topLevelBO, new CustomsMessagingProviderFactoryImplForTest(messagingProvider));
			var provider = new SendMessagesBusinessActionProvider(supporter) as ISendMessagesBusinessActionProvider;

			messagingProvider.ConfigProcessForTesting = (chain) => chain.FindAction("CreateMessages").InsertActionAfter("FromSupporter", null, ActionLink.Success);

			provider.ConfigureProcess(sendChain);

			AssertContains("New step in chain", "success: { CreateMessages: success: { FromSupporter:", sendChain.GetChainAsString());
		}

		public void TestSendMessagesSecurityCheckPoint()
		{
			var result = actionInterface.SendMessagesSecurityCheckpoint(new ActionResult(true));
			AssertEquals("No Security Checkpoint", true, result.Success);

			messagingProvider.SendMessagesSecurityCheckpointForTesting = Env.Security.CustomsDeclarationSendWithMessageErrors;
			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = false;

			result = actionInterface.SendMessagesSecurityCheckpoint(new ActionResult(true));
			AssertEquals("Security Checkpoint not allowed", false, result.Success);
			AssertEquals("Notificatoin count", 1, result.Notifications.Count);
			AssertEquals("Security message", Env.Security.CustomsDeclarationSendWithMessageErrors.ErrorMessageForNotAllowed, result.Notifications[0].Message);

			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;

			result = actionInterface.SendMessagesSecurityCheckpoint(new ActionResult(true));
			AssertEquals("Security Checkpoint allowed", true, result.Success);
		}

		public void TestSendMessagesWithErrorsSecurityCheckPoint()
		{
			var result = actionInterface.SendMessagesWithErrorsSecurityCheckpoint(new ActionResult(true));
			AssertEquals("No Security Checkpoint", true, result.Success);

			messagingProvider.SendMessagesWithErrorsSecurityCheckpointForTesting = Env.Security.CustomsDeclarationSendWithMessageErrors;
			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = false;

			result = actionInterface.SendMessagesWithErrorsSecurityCheckpoint(new ActionResult(true));
			AssertEquals("Security Checkpoint not allowed but no errors", true, result.Success);

			result = actionInterface.SendMessagesWithErrorsSecurityCheckpoint(new ActionResult(true, new[] { new MessageSendingWarning(MessageSendingValidation.MessageErrorsExistHeaderText) }));
			AssertEquals("Security Checkpoint not allowed", false, result.Success);
			AssertEquals("Security notification", 2, result.Notifications.Count);
			AssertEquals("Security message", Env.Security.CustomsDeclarationSendWithMessageErrors.ErrorMessageForNotAllowed, result.Notifications[1].Message);

			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;

			result = actionInterface.SendMessagesWithErrorsSecurityCheckpoint(new ActionResult(true, new[] { new MessageSendingWarning(MessageSendingValidation.MessageErrorsExistHeaderText) }));
			AssertEquals("Security Checkpoint allowed", true, result.Success);
		}

		public void TestCreditAndDPSCheck()
		{
			var messagingProviderNoCreditCheck = new CustomsMessagingProviderImplForTest(new[] { customsMessenger1, customsMessenger2 });
			var messagingSupporterNoCreditCheck = new CustomsMessagingSupporter(topLevelBO, new CustomsMessagingProviderFactoryImplForTest(messagingProviderNoCreditCheck));
			var providerNoCreditCheck = new SendMessagesBusinessActionProvider(messagingSupporterNoCreditCheck) as ISendMessagesBusinessActionProvider;

			var docObj = Factory.New<CreditControlledDocumentDeliveryImplForTest>();

			messagingProviderNoCreditCheck.DocumentDeliveryObjectForTesting = docObj;
			messagingProvider.DocumentDeliveryObjectForTesting = docObj;

			var resultNoSupport = providerNoCreditCheck.CreditAndDPSCheck(new ActionResult(true));
			var resultWithSupport = actionInterface.CreditAndDPSCheck(new ActionResult(true));

			CombineAssertions(() =>
			{
				AssertNull("No Support, no Credit Check Result", resultNoSupport.PassThroughData);
				AssertType<CreditCheckResult>("Credit Check supported and result returned", resultWithSupport.PassThroughData);
			});

			var result = resultWithSupport.PassThroughData as CreditCheckResult;
			CombineAssertions(() =>
			{
				AssertEquals("No message", string.Empty, result.Message);
				AssertEquals("Is allowed", true, result.IsAllowedToProceed);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			topLevelBO = Factory.New<DummyEnterpriseBusinessObject>();
			childBO1 = Factory.New<DummyBizObjWithMessages>();
			childBO2 = Factory.New<DummyBizObjWithMessages>();
			customsMessenger1 = new CustomsMessengerImplForTest(childBO1);
			permitProcessor = new PermitProcessorForTest();
			customsMessenger2 = new CustomsMessengerWithPermitSupportImplForTest(childBO2, permitProcessor);
			messagingProvider = new CustomsMessagingProviderAllImplForTest(new[] { customsMessenger1, customsMessenger2 });
			messagingSupporter = new CustomsMessagingSupporter(topLevelBO, new CustomsMessagingProviderFactoryImplForTest(messagingProvider));
			boActionProvider = new SendMessagesBusinessActionProvider(messagingSupporter);
		}

		DummyEnterpriseBusinessObject topLevelBO;
		DummyBizObjWithMessages childBO1;
		DummyBizObjWithMessages childBO2;

		ISendMessagesBusinessActionProvider actionInterface => boActionProvider;
		SendMessagesBusinessActionProvider boActionProvider;
		CustomsMessagingProviderAllImplForTest messagingProvider;
		CustomsMessagingSupporter messagingSupporter;
		CustomsMessengerImplForTest customsMessenger1;
		CustomsMessengerImplForTest customsMessenger2;
		PermitProcessorForTest permitProcessor;

		sealed class DummyEDIMessage_SendMessagesBusinessActionProviderTests : EDIMessage
		{
			public DummyEDIMessage_SendMessagesBusinessActionProviderTests(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
				Object = this;
			}

			internal DummyEDIMessage_SendMessagesBusinessActionProviderTests Object { get; set; }

			internal string GetMessageReferenceNumberReturns { get; set; } = string.Empty;

			internal bool GetMessageReferenceNumberShouldThrow { get; set; }

			protected override string GetMessageReferenceNumber()
			{
				if (GetMessageReferenceNumberShouldThrow)
				{
					throw new ZCannotSaveException("Mock Save Exception", "Testing", false, null);
				}
				return GetMessageReferenceNumberReturns;
			}
		}
	}
}
