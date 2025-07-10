using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business.Testing
{
	[TestedType(typeof(PortMessagingStatusRetriever))]
	sealed class PortMessagingStatusRetrieverTest : NonPersistentBusinessObjectTestCase
	{
		#region Message Is Waiting For Reply

		[TestDate(2021, 6, 1)]
		public void TestIsWaitingForReply()
		{
			SetupTestCase((parent, statusRetriever) =>
			{
				AssertEquals(false, statusRetriever.IsWaitingForReply(MessageType.PortOrderWithHDS));
			});

			SetupTestCase((parent, statusRetriever) =>
			{
				AssertIsWaitingForReply(parent, statusRetriever, Events.MessageSent, MessageType.PortOrderWithHDS, true);
				AssertIsWaitingForReply(parent, statusRetriever, Events.MessageAccepted, MessageType.PortOrderWithHDS, false);
				AssertIsWaitingForReply(parent, statusRetriever, Events.MessageSent, MessageType.PortOrderWithHDS, true);
				AssertIsWaitingForReply(parent, statusRetriever, Events.MessageAccepted, MessageType.PortOrderWithHDS, false);
			});

			SetupTestCase((parent, statusRetriever) =>
			{
				AssertIsWaitingForReply(parent, statusRetriever, Events.MessageSent, MessageType.PortOrderWithHDS, true);
				AssertIsWaitingForReply(parent, statusRetriever, Events.InterchangeRejected, MessageType.PortOrderWithHDS, false);
				AssertIsWaitingForReply(parent, statusRetriever, Events.MessageSent, MessageType.PortOrderWithHDS, true);

				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
				AddNewDakosyEventLog(parent, Events.InterchangeRejected, string.Empty);
				Factory.Save();

				AssertEquals(false, statusRetriever.IsWaitingForReply(MessageType.PortOrderWithHDS));
			});

			SetupTestCase((parent, statusRetriever) =>
			{
				AssertIsWaitingForReply(parent, statusRetriever, Events.MessageSent, MessageType.PortOrderWithHDS, true);
				AssertIsWaitingForReply(parent, statusRetriever, Events.MessageRejected, MessageType.PortOrderWithHDS, false);
				AssertIsWaitingForReply(parent, statusRetriever, Events.MessageSent, MessageType.PortOrderWithHDS, true);
				AssertIsWaitingForReply(parent, statusRetriever, Events.MessageAccepted, MessageType.PortOrderWithHDS, false);
			});

			SetupTestCase((parent, statusRetriever) =>
			{
				AssertIsWaitingForReply(parent, statusRetriever, Events.MessageSent, MessageType.PortOrderWithHDS, true);
				AssertIsWaitingForReply(parent, statusRetriever, Events.MessageAccepted, MessageType.PortOrderWithHDS, false);
				AssertIsWaitingForReply(parent, statusRetriever, Events.MessageWithdrawCancelRequest, MessageType.PortOrderWithHDS, true);
				AssertIsWaitingForReply(parent, statusRetriever, Events.MessageWithdrawCancelAccepted, MessageType.PortOrderWithHDS, false);
			});

			SetupTestCase((parent, statusRetriever) =>
			{
				AddLog(parent, statusRetriever, Events.MessageSent, MessageType.PortOrderWithHDS);
				AssertEquals(true, statusRetriever.IsWaitingForReply(MessageType.PortOrderWithHDS));

				AddLog(parent, statusRetriever, Events.MessageSent, MessageType.GatePass);
				AssertEquals(true, statusRetriever.IsWaitingForReply(MessageType.GatePass));

				AssertEquals(true, statusRetriever.IsWaitingForReply(MessageType.PortOrderWithHDS));
			});

			SetupTestCase((parent, statusRetriever) =>
			{
				AddLog(parent, statusRetriever, Events.MessagePendingProcessing, MessageType.PortOrderWithHDS);
				AssertEquals(true, statusRetriever.IsMessageStillPending(MessageType.PortOrderWithHDS));

				AddLog(parent, statusRetriever, Events.MessageAccepted, MessageType.PortOrderWithHDS);
				AssertEquals(false, statusRetriever.IsMessageStillPending(MessageType.PortOrderWithHDS));
			});

			SetupTestCase((parent, statusRetriever) =>
			{
				AddLog(parent, statusRetriever, Events.MessageWithdrawCancelRequest, MessageType.PortOrderWithHDS);
				AddLog(parent, statusRetriever, Events.MessagePendingProcessing, MessageType.PortOrderWithHDS);
				AssertEquals(true, statusRetriever.IsMessageCancellationStillPending(MessageType.PortOrderWithHDS));

				AddLog(parent, statusRetriever, Events.MessageWithdrawCancelRequest, MessageType.GatePass);
				AddLog(parent, statusRetriever, Events.MessagePendingProcessing, MessageType.GatePass);
				AssertEquals(true, statusRetriever.IsMessageCancellationStillPending(MessageType.GatePass));

				AssertEquals(true, statusRetriever.IsMessageCancellationStillPending(MessageType.PortOrderWithHDS));
			});

			SetupTestCase((parent, statusRetriever) =>
			{
				AddLog(parent, statusRetriever, Events.MessageSent, MessageType.PortOrderWithHDS);
				AddLog(parent, statusRetriever, Events.MessageAccepted, MessageType.PortOrderWithHDS);
				AssertEquals(true, statusRetriever.HasMessageToCancel(MessageType.PortOrderWithHDS));

				AddLog(parent, statusRetriever, Events.MessageSent, MessageType.GatePass);
				AddLog(parent, statusRetriever, Events.MessageAccepted, MessageType.GatePass);
				AssertEquals(true, statusRetriever.HasMessageToCancel(MessageType.GatePass));

				AssertEquals(true, statusRetriever.HasMessageToCancel(MessageType.PortOrderWithHDS));
			});

			void SetupTestCase(Action<BusinessObject, PortMessagingStatusRetriever> assertAction)
			{
				var parent = Factory.New<DummyEnterpriseBusinessObject>();
				var statusRetriever = new PortMessagingStatusRetriever(parent);
				assertAction(parent, statusRetriever);
			}

			void AssertIsWaitingForReply(BusinessObject parent, PortMessagingStatusRetriever statusRetriever, Event @event, MessageType messageType, bool expectedResult)
			{
				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
				AddNewDakosyEventLog(parent, @event, PortMessagingStatusRetriever.GetLogReferenceFromMessageType(messageType));
				Factory.Save();
				AssertEquals(expectedResult, statusRetriever.IsWaitingForReply(messageType));
			}

			void AddLog(BusinessObject parent, PortMessagingStatusRetriever statusRetriever, Event @event, MessageType messageType)
			{
				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
				AddNewDakosyEventLog(parent, @event, PortMessagingStatusRetriever.GetLogReferenceFromMessageType(messageType));
				Factory.Save();
			}
		}

		#endregion

		#region Message Sent Status

		[TestDate(2013, 10, 1)]
		public void TestSentStatus()
		{
			var parent = Factory.New<DummyEnterpriseBusinessObject>();
			var messagingStatus = new PortMessagingStatusRetriever(parent);
			AssertEquals("Not Sent", messagingStatus.SentStatus);

			var portOrderText = "Port Order with HDS";
			AddNewDakosyEventLog(parent, Events.MessageSent, portOrderText);
			Factory.Save();

			AssertEquals(portOrderText + " Message Sent to Dakosy", messagingStatus.SentStatus);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			AddNewDakosyEventLog(parent, Events.MessageWithdrawCancelRequest, portOrderText + " Cancellation");
			Factory.Save();

			AssertContains(portOrderText + " Cancellation Message Withdraw/Cancel Request sent to Dakosy", messagingStatus.SentStatus);
		}

		public void TestSentDate()
		{
			var parent = Factory.New<DummyEnterpriseBusinessObject>();
			var log = AddNewDakosyEventLog(parent, Events.MessageSent);
			Factory.Save();

			var messagingStatus = new PortMessagingStatusRetriever(parent);
			AssertEquals(log.SL_EventTime, messagingStatus.SentDate);
		}

		public void TestSentBy()
		{
			var parent = Factory.New<DummyEnterpriseBusinessObject>();
			var log = AddNewDakosyEventLog(parent, Events.MessageSent);
			Factory.Save();

			var messagingStatus = new PortMessagingStatusRetriever(parent);
			AssertEquals(log.SL_UserNameAndInitials, messagingStatus.SentBy);
		}

		#endregion

		#region Message Received Status

		[TestDate(2013, 10, 1)]
		public void TestMessageStatus()
		{
			var parent = Factory.New<DummyEnterpriseBusinessObject>();
			var messagingStatus = new PortMessagingStatusRetriever(parent);
			AssertEquals(ZString.Empty, messagingStatus.MessageStatus);

			parent.GetLogs().AddNew(Events.MessageAccepted, "blah");
			Factory.Save();
			AssertEquals(ZString.Empty, messagingStatus.MessageStatus);

			AddNewDakosyEventLog(parent, Events.MessageAccepted);
			Factory.Save();

			AssertEquals(Events.MessageAccepted.Description, messagingStatus.MessageStatus);

			Action<Event> assertMessageStatus = mostRecentEvent =>
				{
					TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

					AddNewDakosyEventLog(parent, mostRecentEvent);
					Factory.Save();

					AssertEquals(mostRecentEvent.Description, messagingStatus.MessageStatus);
				};

			assertMessageStatus(Events.InterchangeReceiptAcknowledged);
			assertMessageStatus(Events.MessageWithdrawCancelAccepted);
			assertMessageStatus(Events.InterchangeRejected);
			assertMessageStatus(Events.MessagePendingProcessing);
			assertMessageStatus(Events.MessageRejected);
			assertMessageStatus(Events.InterchangeReceiptAcknowledged);
		}

		[TestDate(2013, 10, 1)]
		public void TestMessageStatusDescription()
		{
			var parent = Factory.New<DummyEnterpriseBusinessObject>();
			var messagingStatus = new PortMessagingStatusRetriever(parent);
			AssertEquals(ZString.Empty, messagingStatus.MessageStatusDescription);

			AddNewDakosyEventLog(parent, Events.MessageAccepted);
			Factory.Save();

			AssertContains(Events.MessageAccepted.Description + " by Dakosy", messagingStatus.MessageStatusDescription);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			AddNewDakosyEventLog(parent, Events.MessageWithdrawCancelAccepted);
			Factory.Save();

			AssertContains(Events.MessageWithdrawCancelAccepted.Description + " by Dakosy", messagingStatus.MessageStatusDescription);
		}

		[TestDate(2013, 11, 1)]
		public void TestMessageStatusDescriptionMSN_MWR()
		{
			var parent = Factory.New<DummyEnterpriseBusinessObject>();
			var messagingStatus = new PortMessagingStatusRetriever(parent);
			AssertEquals(ZString.Empty, messagingStatus.MessageStatusDescription);

			AddNewDakosyEventLog(parent, Events.MessageSent);
			Factory.Save();

			AssertContains(Events.MessageSent.Description + " to Dakosy", messagingStatus.SentStatus);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			AddNewDakosyEventLog(parent, Events.MessageWithdrawCancelRequest);
			Factory.Save();

			AssertContains(Events.MessageWithdrawCancelRequest.Description + " sent to Dakosy", messagingStatus.SentStatus);
		}

		public void TestMessageStatusDate()
		{
			var parent = Factory.New<DummyEnterpriseBusinessObject>();
			var log = AddNewDakosyEventLog(parent, Events.InterchangeRejected);
			Factory.Save();

			var messagingStatus = new PortMessagingStatusRetriever(parent);
			AssertEquals(log.SL_EventTime, messagingStatus.MessageStatusDate);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PortMessagingStatusRetriever(Factory.New<DummyBusinessObject>());
		}

		static StmALog AddNewDakosyEventLog(BusinessObject parent, Event eventType, string messageType = "")
		{
			var parameters = new[]
			{
				new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.MessageType, messageType),
				new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Department, "Dakosy")
			};

			return parent.GetLogs().AddNew(eventType, parameters);
		}

		#endregion
	}
}
