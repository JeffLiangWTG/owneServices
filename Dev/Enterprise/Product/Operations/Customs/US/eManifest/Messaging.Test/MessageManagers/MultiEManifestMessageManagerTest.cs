using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageManagers.DocumentSending.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Customs.US.eManifest.Messaging.MessageProcessors.Testing;
using Enterprise.ZArchitecture.Data.Mutex;

namespace Enterprise.Customs.US.eManifest.Messaging.Testing
{
	sealed class MultiEManifestMessageManagerTest : TestCaseWithFactory
	{
		public void TestSendMessageWithMutexLock()
		{
			var trip = Factory.New<Trip>();
			trip.BH_MessageStatus = MessageStatusList.Codes.AwaitingChange;
			trip.BH_MessageStatus = ZString.Empty;
			var shipment = trip.Shipments.AddNew();
			shipment.B0_MasterBillNumber = "1234567890";
			var shipment2 = trip.Shipments.AddNew();
			shipment2.B0_MasterBillNumber = "1234567891";
			Factory.Save();
			var mutex = new ZGlobalMutex(Enterprise.ZArchitecture.Modules.MutexIDs.SendCustomsMessage, trip.PK.ToString());
			Assert(mutex.Lock());
			try
			{
				var notification = new MessageNotificationCollector_ForTest();
				var wrapper = new eManifestMessageSendingObjectParent(trip);
				var manager = new MultiEManifestMessageManager(wrapper, notification);
				manager.SendMessages();
				AssertContains(" is sending messages via Submit eManifest (No ACE ID) for this Trip, please try again later.", notification.LastMessage);
			}
			finally
			{
				mutex.Unlock();
			}
		}

		public void TestCanSendThisMessage()
		{
			var trip = Factory.New<Trip>();
			trip.BH_MessageStatus = MessageStatusList.Codes.AwaitingChange;
			var notification = new MessageNotificationCollector_ForTest();
			var wrapper = new eManifestMessageSendingObjectParent(trip);
			wrapper.DefaultShouldSend();
			var manager = new MultiEManifestMessageManager(wrapper, notification);
			manager.SendMessages();
			AssertMultilineASCIIEquals("Error of Sending", @"System cannot send an Unassociated Shipments Original message as there are no shipments entered.", notification.LastMessage);
			trip.BH_MessageStatus = ZString.Empty;
			var shipment = trip.Shipments.AddNew();
			shipment.B0_MasterBillNumber = "1234567890";
			var shipment2 = trip.Shipments.AddNew();
			shipment2.B0_MasterBillNumber = "1234567891";
			shipment2.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Accepted;
			Factory.Save();
			notification = new MessageNotificationCollector_ForTest();
			wrapper = new eManifestMessageSendingObjectParent(trip);
			wrapper.DefaultShouldSend();
			manager = new MultiEManifestMessageManager(wrapper, notification);
			manager.SendMessages();
			AssertMultilineASCIIEquals("Submit eManifest (No ACE ID) only available for jobs with all shipments have same release status.", notification.LastMessage);
			shipment2.B0_ReleaseStatus = ZString.Empty;
			Factory.Save();
			notification = new MessageNotificationCollector_ForTest();
			wrapper = new eManifestMessageSendingObjectParent(trip);
			wrapper.DefaultShouldSend();
			manager = new MultiEManifestMessageManager(wrapper, notification);
			manager.SendMessages();
			AssertMultilineASCIIEquals("Error of Sending", @"
1 Unassociated Shipments message queued for sending.
1 Preliminary Trip Details message set to pending on acceptance of previous message.
1 Crew/Passengers Details message set to pending on acceptance of previous message.
1 Confirm Preliminary Trip Details message set to pending on acceptance of previous message.", notification.LastMessage);
			notification = new MessageNotificationCollector_ForTest();
			wrapper = new eManifestMessageSendingObjectParent(trip);
			wrapper.DefaultShouldSend();
			manager = new MultiEManifestMessageManager(wrapper, notification);
			manager.SendMessages(true);
			AssertNull("Should skip all notifications.", notification.LastMessage);
		}

		public void TestResultMessages()
		{
			var trip = Factory.New<Trip>();
			Factory.Save();
			var shipment = trip.Shipments.AddNew();
			shipment.B0_MasterBillNumber = "1234567890";
			shipment.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Accepted;
			Factory.Save();
			var notification = new MessageNotificationCollector_ForTest();
			var wrapper = new eManifestMessageSendingObjectParent(trip);
			var manager = new MultiEManifestMessageManager(wrapper, notification);
			wrapper.DefaultShouldSend();
			manager.SendMessages();
			AssertContains("1 Unassociated Shipments message queued for sending.", notification.LastMessage);
			AssertContains("1 Preliminary Trip Details message set to pending on acceptance of previous message.", notification.LastMessage);
			AssertContains("1 Crew/Passengers Details message set to pending on acceptance of previous message.", notification.LastMessage);
			AssertContains("1 Confirm Preliminary Trip Details message set to pending on acceptance of previous message.", notification.LastMessage);
			trip.Messages.Load();
			AssertEquals("Should have sent a message", 4, trip.Messages.Count);
			AssertContains(MessageTypes.Codes.UnassociatedShipments, trip.Messages[0].EM_MessageType);
			AssertEquals(ZDateTime.Empty, trip.Messages[0].EM_HeldUntilDate);
			AssertContains("UnassociatedShipments Message", "BGM+87:::STANDARD+SYSTEM+2", trip.Messages[0].EM_MessageText);
			AssertContains(MessageTypes.Codes.PreliminaryTrip, trip.Messages[1].EM_MessageType);
			AssertContains(MessageActionCodes.Codes.Original, trip.Messages[1].EM_MessageSubType);
			AssertNotNull(trip.Messages[1].EM_HeldUntilDate);
			AssertContains("PreliminaryTrip Message", eManifestMessageManagerHelper.MessagePlaceHolder, trip.Messages[1].EM_MessageText);
			AssertContains(MessageTypes.Codes.CrewAndPassenger, trip.Messages[2].EM_MessageType);
			AssertContains(MessageActionCodes.Codes.Original, trip.Messages[2].EM_MessageSubType);
			AssertNotNull(trip.Messages[2].EM_HeldUntilDate);
			AssertContains("CrewAndPassenger Message", eManifestMessageManagerHelper.MessagePlaceHolder, trip.Messages[2].EM_MessageText);
			AssertContains(MessageTypes.Codes.PreliminaryTrip, trip.Messages[3].EM_MessageType);
			AssertContains(MessageActionCodes.Codes.Confirmation, trip.Messages[3].EM_MessageSubType);
			AssertNotNull(trip.Messages[3].EM_HeldUntilDate);
			AssertContains("CompleteTrip Message", eManifestMessageManagerHelper.MessagePlaceHolder, trip.Messages[3].EM_MessageText);
		}

		public void TestSkipNotification()
		{
			var trip = Factory.New<Trip>();
			var shipment = trip.Shipments.AddNew();
			shipment.B0_MasterBillNumber = "1234567890";
			shipment.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Accepted;
			Factory.Save();
			var notification = new MessageNotificationCollector_ForTest();
			var wrapper = new eManifestMessageSendingObjectParent(trip);
			var manager = new MultiEManifestMessageManager(wrapper, notification);
			wrapper.DefaultShouldSend();
			manager.SendMessages();
			AssertNotNull("Should show notification.", notification.LastMessage);
			notification = new MessageNotificationCollector_ForTest();
			wrapper = new eManifestMessageSendingObjectParent(trip);
			manager = new MultiEManifestMessageManager(wrapper, notification);
			wrapper.DefaultShouldSend();
			manager.SendMessages(true);
			AssertNull("Should show no notification at the end of sending.", notification.LastMessage);
		}

		public void TestResetActionCodeOnAcceptedTrip()
		{
			var trip = Factory.New<Trip>();
			var shipment = trip.Shipments.AddNew();
			shipment.B0_MasterBillNumber = "1234567890";
			shipment.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Accepted;
			trip.Messages.Add(MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.UnassociatedShipments, "0", eManifestOriginalMessageWrapperTest.UnassociatedShipmentsMessageText));
			trip.Messages.Add(MessagingTestHelper.GetReceivedEDIMessage(Factory, MessageTypes.Codes.UnassociatedShipments, EntryStatusList.Codes.Clear, "0"));
			Factory.Save();
			var notification = new MessageNotificationCollector_ForTest();
			var wrapper = new eManifestMessageSendingObjectParent(trip);
			var manager = new MultiEManifestMessageManager(wrapper, notification);
			wrapper.DefaultShouldSend();
			manager.SendMessages();
			AssertNotContains("1 Unassociated Shipments message queued for sending.", notification.LastMessage);
			AssertContains("1 Preliminary Trip Details message queued for sending.", notification.LastMessage);
			AssertContains("1 Crew/Passengers Details message set to pending on acceptance of previous message.", notification.LastMessage);
			AssertContains("1 Confirm Preliminary Trip Details message set to pending on acceptance of previous message.", notification.LastMessage);
			trip.Messages.Load();
			var tripMessage = trip.Messages.Cast<EDIMessage>().FirstOrDefault(x => x.EM_MessageType == MessageTypes.Codes.PreliminaryTrip);
			AssertNotNull("Trip message sent", tripMessage);
			AssertEquals("Trip message not hold", ZDateTime.Empty, tripMessage.EM_HeldUntilDate);
			AssertContains("shipmentActionCode", "DOC+700+:23", tripMessage.EM_MessageText);
		}
	}
}
