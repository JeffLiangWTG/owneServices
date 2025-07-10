using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.Testing.IL;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.IL
{
	abstract class ILSendWithdrawalCustomCommandBaseTest : ILSendCustomCommandBaseTest
	{
		public override void TestInvoke_LogMessageSentEvent()
		{
			var @event = Events.MessageWithdrawCancelRequest;
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
					"DEP=Customs",
					"RFN=100000"
				},
				mostRecentLogAfter.Parameters.Select(p => $"{p.Key}={p.Value}"));
		}

		public override void TestIsEnabled()
		{
			CombineAssertions("When MessageAccepted > MessageSent, Message Reference Is Empty => !IsEnabled", () =>
			{
				UpdateMessageReference(string.Empty);
				shipment.Logs.AddNew(Events.MessageSent, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-301));
				shipment.Logs.AddNew(Events.MessageAccepted, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-300));
				shipment.Factory.Save();

				Assert(!command.IsEnabled);
			});

			CombineAssertions("When MessageAccepted > MessageSent, Message Reference Is not Empty => IsEnabled", () =>
			{
				UpdateMessageReference("10003");
				shipment.Logs.AddNew(Events.MessageSent, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-201));
				shipment.Logs.AddNew(Events.MessageAccepted, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-200));
				shipment.Factory.Save();

				Assert(command.IsEnabled);
			});

			shipment.Logs.AddNew(Events.MessageAccepted, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-199));
			shipment.Logs.AddNew(Events.MessageSent, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-198));

			CombineAssertions("When MessageRejected > MessageSent > MessageWithdrawCancelRequest, Message Reference Is not Empty => !IsEnabled", () =>
			{
				shipment.Logs.AddNew(Events.MessageWithdrawCancelRequest, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-102));
				UpdateMessageReference("10005");
				shipment.Logs.AddNew(Events.MessageSent, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-101));
				shipment.Logs.AddNew(Events.MessageRejected, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-100));
				shipment.Factory.Save();

				Assert(!command.IsEnabled);
			});

			CombineAssertions("When MessageRejected > MessageWithdrawCancelRequest > MessageSent, Message Reference Is not Empty => IsEnabled", () =>
			{
				UpdateMessageReference("10006");
				shipment.Logs.AddNew(Events.MessageSent, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-99));
				shipment.Logs.AddNew(Events.MessageWithdrawCancelRequest, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-98));
				shipment.Logs.AddNew(Events.MessageRejected, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-97));
				shipment.Factory.Save();

				Assert(command.IsEnabled);
			});

			CombineAssertions("When MessageRejected > MessageWithdrawCancelRequest > MessageSent, Message Reference Is Empty => !IsEnabled", () =>
			{
				UpdateMessageReference(string.Empty);
				shipment.Factory.Save();

				Assert(!command.IsEnabled);
			});

			shipment.Logs.AddNew(Events.MessageWithdrawCancelRequest, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-40));
			shipment.Logs.AddNew(Events.MessageRejected, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-41));

			CombineAssertions("When latest is MessageWithdrawCancelRequest, Message Reference Is Empty => !IsEnabled", () =>
			{
				UpdateMessageReference(string.Empty);
				shipment.Logs.AddNew(Events.MessageWithdrawCancelRequest, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-10));
				shipment.Logs.AddNew(Events.MessageRejected, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-9));
				shipment.Logs.AddNew(Events.MessageWithdrawCancelAccepted, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-8));
				shipment.Factory.Save();

				Assert(!command.IsEnabled);
			});

			CombineAssertions("When latest is MessageWithdrawCancelRequest, Message Reference Is not Empty => IsEnabled", () =>
			{
				UpdateMessageReference("10007");
				shipment.Logs.AddNew(Events.MessageWithdrawCancelRequest, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-7));
				shipment.Logs.AddNew(Events.MessageRejected, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-6));
				shipment.Logs.AddNew(Events.MessageWithdrawCancelAccepted, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-5));
				shipment.Factory.Save();

				Assert(command.IsEnabled);
			});
		}

		public override void TestInvoke_CheckBeforeInvoke()
		{
			(var documentInfo, var notificationService, _) = CreateDocumentInfo(canSendMessage: true, hasChanges: false, hasErrors: false, hasMessageErrors: false);
			command.NotifyDocumentInfoCreated(documentInfo.Object);
			UpdateMessageReference(string.Empty);
			shipment.Logs.AddNew(Events.MessageWithdrawCancelRequest, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-203));
			shipment.Logs.AddNew(Events.MessageRejected, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-202));
			shipment.Logs.AddNew(Events.MessageWithdrawCancelAccepted, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-201));
			var result = SaveAndInvokeCommand();

			AssertMessageNotCreated(notificationService, result, title: "When Continue With Sending Message return false", messageNotification: "Due to changes in the state of the entity, please reopen the form before sending a withdrawal message.");

			UpdateMessageReference("10007");
			shipment.Logs.AddNew(Events.MessageWithdrawCancelRequest, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-7));
			shipment.Logs.AddNew(Events.MessageRejected, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-6));
			shipment.Logs.AddNew(Events.MessageWithdrawCancelAccepted, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-5));
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

		protected override string CommandId => "SendWithdrawal";

		protected override string CommandCaption => "Withdraw/Cancel Message";

		protected override void SetUp()
		{
			base.SetUp();

			SetupShipmentForWithdrawal();
		}

		void SetupShipmentForWithdrawal()
		{
			UpdateMessageReference("100000");
			shipment.Logs.AddNew(Events.MessageWithdrawCancelRequest, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-705));
			shipment.Logs.AddNew(Events.MessageRejected, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-704));
			shipment.Logs.AddNew(Events.MessageWithdrawCancelAccepted, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-703));
		}
	}
}
