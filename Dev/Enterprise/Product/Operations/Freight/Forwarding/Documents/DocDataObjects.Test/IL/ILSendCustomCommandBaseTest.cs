using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.IL;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Freight.Forwarding.Documents.Testing.IL
{
	abstract class ILSendCustomCommandBaseTest : ILCustomCommandBaseTest
	{
		public void TestInvoke_WhenHasMessageErrors()
		{
			(var documentInfo, var notificationService, _) = CreateDocumentInfo(canSendMessage: true, hasChanges: false, hasErrors: false, hasMessageErrors: true);

			command.NotifyDocumentInfoCreated(documentInfo.Object);
			var result = SaveAndInvokeCommand();

			AssertMessageNotCreated(notificationService, result, title: "When Has Changed", messageNotification: "This document contains message errors. Please fix all message errors before sending.");
		}

		public void TestInvoke_WhenHasErrors()
		{
			(var documentInfo, var notificationService, _) = CreateDocumentInfo(canSendMessage: true, hasChanges: false, hasErrors: true, hasMessageErrors: false);

			command.NotifyDocumentInfoCreated(documentInfo.Object);
			var result = SaveAndInvokeCommand();

			AssertMessageNotCreated(notificationService, result, title: "When Has Errors", messageNotification: "This document contains errors. Please fix all errors before sending.");
		}

		public void TestInvoke_WhenBuilderReturnHSMError()
		{
			const string messageNotification = "No Valid digital sign certificate found for signing this message – please review your staff or company configuration";
			(var documentInfo, var notificationService, var broker) = CreateDocumentInfo(canSendMessage: true, hasChanges: false, hasErrors: false, hasMessageErrors: false);
			command.NotifyDocumentInfoCreated(documentInfo.Object);

			shipment.Factory.Save();
			var result = command.Invoke();

			CombineAssertions("When sign is must", () =>
			{
				AssertEquals("result", false, result);
				notificationService.Verify(s => s.ShowMessage(messageNotification, GetDocumentName()), Times.Once);
			});
		}

		public void TestInvoke_SendMessage()
		{
			var hardRefreshCalled = false;
			(var documentInfo, var notificationService, var broker) = CreateDocumentInfo(canSendMessage: true, hasChanges: false, hasErrors: false, hasMessageErrors: false);

			command.NotifyDocumentInfoCreated(documentInfo.Object);

			var disposableDocumentHardRefresh = broker.GetEvent<DocumentHardRefreshEvent>().Subscribe(_ =>
			{
				hardRefreshCalled = true;
			});

			var result = SaveAndInvokeCommand();
			try
			{
				CombineAssertions("When Message is Valid", () =>
				{
					AssertEquals("result", true, result);
					Assert("Hard refresh should be called, Due we want that all the screen will refresh", hardRefreshCalled);

					var message = GetEDIMessagePerShipment();

					AssertNotNull("Message was created", message);
					AssertEquals("EM_ApplicationCode", "ILC", message.EM_ApplicationCode);
					AssertEquals("EM_MessageType", GetMessageType(), message.EM_MessageType);
					AssertEquals("EM_MessageSubType", GetMessageSubType(), message.EM_MessageSubType);
					AssertEquals("EM_Status", "QUE", message.EM_Status);
					AssertEquals("EM_ReceiveTransmit", "TRX", message.EM_ReceiveTransmit);
					AssertEquals("EM_GP", ZGuid.Empty, message.EM_GP);
					AssertEquals("the message connected to shipment", 1, shipment.Messages.Count);
				});
			}
			finally
			{
				disposableDocumentHardRefresh.Dispose();
			}
		}

		public void TestInvoke_WhenHasChanged()
		{
			(var documentInfo, var notificationService, _) = CreateDocumentInfo(canSendMessage: true, hasChanges: true, hasErrors: false, hasMessageErrors: false);

			command.NotifyDocumentInfoCreated(documentInfo.Object);
			var result = SaveAndInvokeCommand();

			AssertMessageNotCreated(notificationService, result, title: "When Has Message Errors", messageNotification: "Please save changes before sending message.");
		}

		public void TestInvoke_WhenCanNotSendMessage()
		{
			(var documentInfo, var notificationService, _) = CreateDocumentInfo(canSendMessage: false, hasChanges: false, hasErrors: false, hasMessageErrors: false);

			command.NotifyDocumentInfoCreated(documentInfo.Object);
			var result = SaveAndInvokeCommand();

			AssertMessageNotCreated(notificationService, result, title: "When Can Not Send Message", messageNotification: null);
		}

		public virtual void TestInvoke_CheckBeforeInvoke()
		{
			(var documentInfo, var notificationService, _) = CreateDocumentInfo(canSendMessage: true, hasChanges: false, hasErrors: false, hasMessageErrors: false);
			command.NotifyDocumentInfoCreated(documentInfo.Object);
			UpdateMessageReference("1000001");
			shipment.Logs.AddNew(Events.MessageRejected, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-101));
			shipment.Logs.AddNew(Events.MessageSent, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-100));
			var result = SaveAndInvokeCommand();

			AssertMessageNotCreated(notificationService, result, title: "When Continue With Sending Message return false", messageNotification: "Due to changes in the state of the entity, please reopen the form before sending a message.");

			shipment.Logs.AddNew(Events.MessageWithdrawCancelRequest, GetEventParameters(), ZDateTimeOffset.Now.AddMinutes(-31));
			shipment.Logs.AddNew(Events.MessageSent, GetEventParameters(), ZDateTimeOffset.Now.AddMinutes(-30));
			shipment.Logs.AddNew(Events.MessageRejected, GetEventParameters(), ZDateTimeOffset.Now.AddMinutes(-29));
			result = SaveAndInvokeCommand();
			CombineAssertions("When Continue With Sending Message return true", () =>
			{
				AssertEquals("Message should be created successfully", true, result);
				var messages = GetEDIMessagesPerShipment();
				AssertEquals("There should be exactly 1 message created", 1, messages.Length);
				AssertEquals("Message Type", GetMessageType(), messages[0].EM_MessageType);
				AssertEquals("Message Sub Type", GetMessageSubType(), messages[0].EM_MessageSubType);
			});
		}

		public virtual void TestInvoke_LogMessageSentEvent()
		{
			var @event = Events.MessageSent;
			var mostRecentLogBefore = shipment.Logs.MostRecentLogByEventTime(@event);
			AssertNull("Prerequisite: no message sent event should be logged", mostRecentLogBefore);

			(var documentInfo, var notificationService, _) = CreateDocumentInfo(canSendMessage: true, hasChanges: false, hasErrors: false, hasMessageErrors: false);

			command.NotifyDocumentInfoCreated(documentInfo.Object);
			var result = SaveAndInvokeCommand();

			AssertEquals("Prerequisite: result should be true", true, result);
			var mostRecentLogAfter = shipment.Logs.MostRecentLogByEventTime(@event);

			AssertNotNull("A message sent event should be logged", mostRecentLogAfter);

			AssertContainsExactElementsInAnyOrder("Event parameters",
				new[]
							{
					$"MST={GetDocumentName()}",
					"DEP=Customs"
				},
				mostRecentLogAfter.Parameters.Select(p => $"{p.Key}={p.Value}"));
		}

		public void TestIsVisible()
		{
			AssertEquals("IsVisible", true, command.IsVisible);
		}

		public virtual void TestIsEnabled()
		{
			CombineAssertions("When Message Reference Is Empty => IsEnabled", () =>
			{
				UpdateMessageReference(string.Empty);
				shipment.Factory.Save();

				Assert(command.IsEnabled);
			});

			UpdateMessageReference("10003");
			CombineAssertions("When the latest event is not MessageRejected, and Message Reference Is not Empty => !IsEnabled", () =>
			{
				shipment.Logs.AddNew(Events.MessageRejected, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-101));
				shipment.Logs.AddNew(Events.MessageSent, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-100));
				shipment.Factory.Save();

				Assert(!command.IsEnabled);
			});

			CombineAssertions("When MessageRejected>MessageWithdrawCancelRequest>MessageSent, and Message Reference Is not Empty => !IsEnabled", () =>
			{
				shipment.Logs.AddNew(Events.MessageSent, GetEventParameters(), ZDateTimeOffset.Now.AddMinutes(-52));
				shipment.Logs.AddNew(Events.MessageWithdrawCancelRequest, GetEventParameters(), ZDateTimeOffset.Now.AddMinutes(-51));
				shipment.Logs.AddNew(Events.MessageRejected, GetEventParameters(), ZDateTimeOffset.Now.AddMinutes(-50));
				shipment.Factory.Save();

				Assert(!command.IsEnabled);
			});

			CombineAssertions("When MessageRejected>MessageSent>MessageWithdrawCancelRequest, and Message Reference Is not Empty => IsEnabled", () =>
			{
				shipment.Logs.AddNew(Events.MessageWithdrawCancelRequest, GetEventParameters(), ZDateTimeOffset.Now.AddMinutes(-31));
				shipment.Logs.AddNew(Events.MessageSent, GetEventParameters(), ZDateTimeOffset.Now.AddMinutes(-30));
				shipment.Logs.AddNew(Events.MessageRejected, GetEventParameters(), ZDateTimeOffset.Now.AddMinutes(-29));
				shipment.Factory.Save();

				Assert(command.IsEnabled);
			});
		}

		protected abstract string GetMessageType();

		protected abstract string GetMessageSubType();

		protected override string CommandId => "SendMessage";

		protected override string CommandCaption => "Send Message";

		protected override void SetUp()
		{
			base.SetUp();
			var currentCompany = GlbCompany.CurrentCompany;
			disposableCurrentCompany = currentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Israel);

			var wrapper = (IILGlbCompanyWrapper)GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(currentCompany);
			var externalPassword = wrapper.GetGlbExternalPasswordOrCreateNew();

			currentCompany.Factory.Save();
		}

		protected override void TearDown()
		{
			disposableCurrentCompany.Dispose();
			base.TearDown();
		}

		protected void AssertMessageNotCreated(Mock<IUserNotificationService> notificationService, bool result, string title, string messageNotification)
		{
			CombineAssertions(title, () =>
			{
				AssertEquals("result", false, result);
				if (messageNotification != null)
				{
					notificationService.Verify(s => s.ShowMessage(messageNotification, GetDocumentName()), Times.Once);
				}
				var message = GetEDIMessagePerShipment();

				AssertNull("Message was not created", message);
			});
		}

		protected EDIMessage[] GetEDIMessagesPerShipment()
		{
			var query = new ZQuery();
			query.AddToFilter(EDIMessageSchema.EM_LinkTable, ForwardingShipment.Schema.TableName);
			query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, shipment.PK);
			var messages = Factory.Load<EDIMessage>(query);
			return messages;
		}

		EDIMessage GetEDIMessagePerShipment()
		{
			var query = new ZQuery();
			query.AddToFilter(EDIMessageSchema.EM_LinkTable, ForwardingShipment.Schema.TableName);
			query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, shipment.PK);
			var message = Factory.LoadTop1<EDIMessage>(query);
			return message;
		}

		IDisposable disposableCurrentCompany;
	}
}
