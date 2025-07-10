using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.Freight.Forwarding.Documents.Testing.IL
{
	public abstract class ILBaseMessagingExtensionsTest<T> : TestCaseWithFactory where T : ILBaseMessagingExtensions
	{
		public void TestContinueWithSendingMessage_ValidateEvents()
		{
			var messagingExtension = CreateNewMessagingExtension(shipment);
			var notifications = new Mock<IUserNotifications>();

			CombineAssertions("When Message Reference Is Empty => IsEnabled", () =>
			{
				UpdateMessageReference(string.Empty);
				shipment.Factory.Save();

				var result = messagingExtension.ContinueWithSendingMessage(notifications.Object);

				AssertNotNull("Result should not be null", result);
				Assert("Sending message should be Allowed", result.Value);
			});

			UpdateMessageReference("10003");
			CombineAssertions("When the latest event is not MessageRejected, and Message Reference Is not Empty => !IsEnabled", () =>
			{
				shipment.Logs.AddNew(Events.MessageRejected, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-101));
				shipment.Logs.AddNew(Events.MessageSent, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-100));
				shipment.Factory.Save();

				var result = messagingExtension.ContinueWithSendingMessage(notifications.Object);

				AssertNotNull("Result should not be null", result);
				Assert("Sending message should be Disallowed", !result.Value);
			});

			CombineAssertions("When MessageRejected>MessageWithdrawCancelRequest>MessageSent, and Message Reference Is not Empty => !IsEnabled", () =>
			{
				shipment.Logs.AddNew(Events.MessageSent, GetEventParameters(), ZDateTimeOffset.Now.AddMinutes(-52));
				shipment.Logs.AddNew(Events.MessageWithdrawCancelRequest, GetEventParameters(), ZDateTimeOffset.Now.AddMinutes(-51));
				shipment.Logs.AddNew(Events.MessageRejected, GetEventParameters(), ZDateTimeOffset.Now.AddMinutes(-50));
				shipment.Factory.Save();

				var result = messagingExtension.ContinueWithSendingMessage(notifications.Object);

				AssertNotNull("Result should not be null", result);
				Assert("Sending message should be Disallowed", !result.Value);
			});

			CombineAssertions("When MessageRejected>MessageSent>MessageWithdrawCancelRequest, and Message Reference Is not Empty => IsEnabled", () =>
			{
				shipment.Logs.AddNew(Events.MessageWithdrawCancelRequest, GetEventParameters(), ZDateTimeOffset.Now.AddMinutes(-31));
				shipment.Logs.AddNew(Events.MessageSent, GetEventParameters(), ZDateTimeOffset.Now.AddMinutes(-30));
				shipment.Logs.AddNew(Events.MessageRejected, GetEventParameters(), ZDateTimeOffset.Now.AddMinutes(-29));
				shipment.Factory.Save();

				var result = messagingExtension.ContinueWithSendingMessage(notifications.Object);

				AssertNotNull("Result should not be null", result);
				Assert("Sending message should be allowed", result.Value);
			});
		}

		public void TestGetMessageStatus_WhenNoMessageSent()
		{
			var factory = Factory.CreateNewFactory();
			var shipment1 = factory.Load<ForwardingShipment>(shipment.PK);
			var messagingExtension1 = CreateNewMessagingExtension(shipment1);

			var messagingExtension = CreateNewMessagingExtension(shipment);
			var messageStatus = messagingExtension.GetMessageStatus();
			AssertNullOrEmpty("Message status should be null or empty", messageStatus);

			var messageStatus1 = messagingExtension1.GetMessageStatus();
			AssertNullOrEmpty("Message status should be null or empty with reloading", messageStatus1);

			shipment.Logs.AddNew(Events.MessageSent, GetEventParameters());
			Factory.Save();

			messageStatus = messagingExtension.GetMessageStatus();
			messageStatus1 = messagingExtension1.GetMessageStatus();
			AssertEquals("Message status should be correct", $"{GetMessageName()} message has been sent", messageStatus);
			AssertEquals("Message status should be correct with reloading", $"{GetMessageName()} message has been sent", messageStatus1);
		}

		public void TestGetMessageStatus_WhenMessageSent()
		{
			var factory = Factory.CreateNewFactory();
			var shipment1 = factory.Load<ForwardingShipment>(shipment.PK);
			var messagingExtension1 = CreateNewMessagingExtension(shipment1);
			var messagingExtension = CreateNewMessagingExtension(shipment);

			shipment.Logs.AddNew(Events.MessageSent, GetEventParameters());
			Factory.Save();

			var messageStatus = messagingExtension.GetMessageStatus();
			var messageStatus1 = messagingExtension1.GetMessageStatus();
			AssertEquals("Message status should be correct", $"{GetMessageName()} message has been sent", messageStatus);
			AssertEquals("Message status should be correct with reloading", $"{GetMessageName()} message has been sent", messageStatus1);
		}

		public void TestGetMessageStatus_WhenMessageSent_PreviousltWasRejected()
		{
			var factory = Factory.CreateNewFactory();
			var shipment1 = factory.Load<ForwardingShipment>(shipment.PK);
			var messagingExtension1 = CreateNewMessagingExtension(shipment1);
			var messagingExtension = CreateNewMessagingExtension(shipment);

			shipment.Logs.AddNew(Events.MessageSent, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-2));
			shipment.Logs.AddNew(Events.MessageRejected, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-1));
			shipment.Logs.AddNew(Events.MessageSent, GetEventParameters());
			Factory.Save();

			var messageStatus = messagingExtension.GetMessageStatus();
			var messageStatus1 = messagingExtension1.GetMessageStatus();
			AssertEquals("Message status should be correct", $"{GetMessageName()} message has been sent", messageStatus);
			AssertEquals("Message status should be correct with reloading", $"{GetMessageName()} message has been sent", messageStatus1);
		}

		public void TestGetMessageStatus_WhenMessageSent_PreviousltWasAcceptedAndWithdrawn()
		{
			var factory = Factory.CreateNewFactory();
			var shipment1 = factory.Load<ForwardingShipment>(shipment.PK);
			var messagingExtension1 = CreateNewMessagingExtension(shipment1);
			var messagingExtension = CreateNewMessagingExtension(shipment);

			shipment.Logs.AddNew(Events.MessageSent, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-4));
			shipment.Logs.AddNew(Events.MessageAccepted, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-3));
			shipment.Logs.AddNew(Events.MessageWithdrawCancelRequest, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-2));
			shipment.Logs.AddNew(Events.MessageWithdrawCancelAccepted, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-1));
			shipment.Logs.AddNew(Events.MessageSent, GetEventParameters());
			Factory.Save();

			var messageStatus = messagingExtension.GetMessageStatus();
			var messageStatus1 = messagingExtension1.GetMessageStatus();
			AssertEquals("Message status should be correct", $"{GetMessageName()} message has been sent", messageStatus);
			AssertEquals("Message status should be correct with reloading", $"{GetMessageName()} message has been sent", messageStatus1);
		}

		public void TestGetMessageStatus_WhenMessageAccepted()
		{
			var factory = Factory.CreateNewFactory();
			var shipment1 = factory.Load<ForwardingShipment>(shipment.PK);
			var messagingExtension1 = CreateNewMessagingExtension(shipment1);
			var messagingExtension = CreateNewMessagingExtension(shipment);

			shipment.Logs.AddNew(Events.MessageSent, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-1));
			shipment.Logs.AddNew(Events.MessageAccepted, GetEventParameters());
			Factory.Save();

			var messageStatus = messagingExtension.GetMessageStatus();
			var messageStatus1 = messagingExtension1.GetMessageStatus();
			AssertEquals("Message status should be correct", $"{GetMessageName()} message has been accepted", messageStatus);
			AssertEquals("Message status should be correct with reloading", $"{GetMessageName()} message has been accepted", messageStatus1);
		}

		public void TestGetMessageStatus_WhenMessageRejected()
		{
			var factory = Factory.CreateNewFactory();
			var shipment1 = factory.Load<ForwardingShipment>(shipment.PK);
			var messagingExtension1 = CreateNewMessagingExtension(shipment1);
			var messagingExtension = CreateNewMessagingExtension(shipment);

			shipment.Logs.AddNew(Events.MessageSent, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-1));
			shipment.Logs.AddNew(Events.MessageRejected, GetEventParameters());
			Factory.Save();

			var messageStatus = messagingExtension.GetMessageStatus();
			var messageStatus1 = messagingExtension1.GetMessageStatus();
			AssertEquals("Message status should be correct", $"{GetMessageName()} message has been rejected", messageStatus);
			AssertEquals("Message status should be correct with reloading", $"{GetMessageName()} message has been rejected", messageStatus1);
		}

		public void TestGetMessageStatus_WhenWithdrawalSent()
		{
			var factory = Factory.CreateNewFactory();
			var shipment1 = factory.Load<ForwardingShipment>(shipment.PK);
			var messagingExtension1 = CreateNewMessagingExtension(shipment1);
			var messagingExtension = CreateNewMessagingExtension(shipment);

			shipment.Logs.AddNew(Events.MessageSent, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-2));
			shipment.Logs.AddNew(Events.MessageAccepted, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-1));
			shipment.Logs.AddNew(Events.MessageWithdrawCancelRequest, GetEventParameters());
			Factory.Save();

			var messageStatus = messagingExtension.GetMessageStatus();
			var messageStatus1 = messagingExtension1.GetMessageStatus();
			AssertEquals("Message status should be correct", $"{GetMessageName()} message withdraw/cancel has been sent", messageStatus);
			AssertEquals("Message status should be correct with reloading", $"{GetMessageName()} message withdraw/cancel has been sent", messageStatus1);
		}

		public void TestGetMessageStatus_WhenWithdrawalAccepted()
		{
			var factory = Factory.CreateNewFactory();
			var shipment1 = factory.Load<ForwardingShipment>(shipment.PK);
			var messagingExtension1 = CreateNewMessagingExtension(shipment1);
			var messagingExtension = CreateNewMessagingExtension(shipment);

			shipment.Logs.AddNew(Events.MessageSent, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-3));
			shipment.Logs.AddNew(Events.MessageAccepted, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-2));
			shipment.Logs.AddNew(Events.MessageWithdrawCancelRequest, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-1));
			shipment.Logs.AddNew(Events.MessageWithdrawCancelAccepted, GetEventParameters());
			Factory.Save();

			var messageStatus = messagingExtension.GetMessageStatus();
			var messageStatus1 = messagingExtension1.GetMessageStatus();
			AssertEquals("Message status should be correct", $"{GetMessageName()} message withdrawal has been accepted", messageStatus);
			AssertEquals("Message status should be correct with reloading", $"{GetMessageName()} message withdrawal has been accepted", messageStatus1);
		}

		public void TestGetMessageStatus_WhenWithdrawalRejected()
		{
			var factory = Factory.CreateNewFactory();
			var shipment1 = factory.Load<ForwardingShipment>(shipment.PK);
			var messagingExtension1 = CreateNewMessagingExtension(shipment1);
			var messagingExtension = CreateNewMessagingExtension(shipment);

			shipment.Logs.AddNew(Events.MessageSent, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-3));
			shipment.Logs.AddNew(Events.MessageAccepted, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-2));
			shipment.Logs.AddNew(Events.MessageWithdrawCancelRequest, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-1));
			shipment.Logs.AddNew(Events.MessageRejected, GetEventParameters());
			Factory.Save();

			var messageStatus = messagingExtension.GetMessageStatus();
			var messageStatus1 = messagingExtension1.GetMessageStatus();
			AssertEquals("Message status should be correct", $"{GetMessageName()} message has been rejected", messageStatus);
			AssertEquals("Message status should be correct with reloading", $"{GetMessageName()} message has been rejected", messageStatus1);
		}

		public void TestContinueWithSendingMessageWithdrawal_ValidateEvents()
		{
			var messagingExtension = CreateNewMessagingExtension(shipment);
			var notifications = new Mock<IUserNotifications>();
			CombineAssertions("When MessageAccepted > MessageSent, Message Reference Is Empty => !IsEnabled", () =>
			{
				UpdateMessageReference(string.Empty);
				shipment.Logs.AddNew(Events.MessageSent, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-301));
				shipment.Logs.AddNew(Events.MessageAccepted, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-300));
				shipment.Factory.Save();

				var result = messagingExtension.ContinueWithSendingMessageWithdrawal(notifications.Object);

				AssertNotNull("Result should not be null", result);
				Assert("Sending message should be disallowed", !result.Value);
			});

			CombineAssertions("When MessageAccepted > MessageSent, Message Reference Is not Empty => IsEnabled", () =>
			{
				UpdateMessageReference("10003");
				shipment.Logs.AddNew(Events.MessageSent, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-201));
				shipment.Logs.AddNew(Events.MessageAccepted, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-200));
				shipment.Factory.Save();

				var result = messagingExtension.ContinueWithSendingMessageWithdrawal(notifications.Object);

				AssertNotNull("Result should not be null", result);
				Assert("Sending message should be allowed", result.Value);
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

				var result = messagingExtension.ContinueWithSendingMessageWithdrawal(notifications.Object);

				AssertNotNull("Result should not be null", result);
				Assert("Sending message should be disallowed", !result.Value);
			});

			CombineAssertions("When MessageRejected > MessageWithdrawCancelRequest > MessageSent, Message Reference Is not Empty => IsEnabled", () =>
			{
				UpdateMessageReference("10006");
				shipment.Logs.AddNew(Events.MessageSent, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-99));
				shipment.Logs.AddNew(Events.MessageWithdrawCancelRequest, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-98));
				shipment.Logs.AddNew(Events.MessageRejected, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-97));
				shipment.Factory.Save();

				var result = messagingExtension.ContinueWithSendingMessageWithdrawal(notifications.Object);

				AssertNotNull("Result should not be null", result);
				Assert("Sending message should be allowed", result.Value);
			});

			CombineAssertions("When MessageRejected > MessageWithdrawCancelRequest > MessageSent, Message Reference Is Empty => !IsEnabled", () =>
			{
				UpdateMessageReference(string.Empty);
				shipment.Factory.Save();

				var result = messagingExtension.ContinueWithSendingMessageWithdrawal(notifications.Object);

				AssertNotNull("Result should not be null", result);
				Assert("Sending message should be disallowed", !result.Value);
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

				var result = messagingExtension.ContinueWithSendingMessageWithdrawal(notifications.Object);

				AssertNotNull("Result should not be null", result);
				Assert("Sending message should be disallowed", !result.Value);
			});

			CombineAssertions("When latest is MessageWithdrawCancelRequest, Message Reference Is not Empty => IsEnabled", () =>
			{
				UpdateMessageReference("10007");
				shipment.Logs.AddNew(Events.MessageWithdrawCancelRequest, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-7));
				shipment.Logs.AddNew(Events.MessageRejected, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-6));
				shipment.Logs.AddNew(Events.MessageWithdrawCancelAccepted, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-5));
				shipment.Factory.Save();

				var result = messagingExtension.ContinueWithSendingMessageWithdrawal(notifications.Object);

				AssertNotNull("Result should not be null", result);
				Assert("Sending message should be allowed", result.Value);
			});
		}

		public void TestContinueWithResetToOriginal_ValidateEvents()
		{
			var messagingExtension = CreateNewMessagingExtension(shipment);
			var notifications = new Mock<IUserNotifications>();

			CombineAssertions("When Message Reference Is Empty => !IsEnabled", () =>
			{
				UpdateMessageReference(string.Empty);
				shipment.Logs.AddNew(Events.MessageSent, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-10));
				shipment.Logs.AddNew(Events.MessageRejected, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-20));
				shipment.Factory.Save();

				var result = messagingExtension.ContinueWithResetToOriginal(notifications.Object);

				AssertNotNull("Result should not be null", result);
				Assert("Sending message should be disallowed", !result.Value);
			});

			CombineAssertions("When the latest event is MessageSent, and Message Reference Is not Empty => IsEnabled", () =>
			{
				UpdateMessageReference("10003");
				shipment.Logs.AddNew(Events.MessageSent, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-1));
				shipment.Logs.AddNew(Events.MessageRejected, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-2));
				shipment.Logs.AddNew(Events.MessageAccepted, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-3));
				shipment.Factory.Save();

				var result = messagingExtension.ContinueWithResetToOriginal(notifications.Object);

				AssertNotNull("Result should not be null", result);
				Assert("Sending message should be allowed", result.Value);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			shipment = Factory.New<ForwardingShipment>();
			Factory.Save();
		}

		protected abstract ZString GetMessageType();

		protected abstract T CreateNewMessagingExtension(ForwardingShipment shipment);

		protected string GetEventParameters() => $"|DEP=Customs|MST={GetDocumentName()}" + GetRFN();

		protected abstract string GetDocumentName();

		protected abstract string GetMessageName();

		protected abstract void UpdateMessageReference(string reference);

		protected abstract ZString MessageReference { get; }

		protected ForwardingShipment shipment;

		string GetRFN()
			=> MessageReference.IsEmpty ? string.Empty : $"|RFN={MessageReference}";
	}
}
