using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Messaging.Testing
{
	[TestedType(typeof(eManifestMessageSendingObjectParent))]
	class eManifestMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaultShouldSend()
		{
			var trip = Factory.New<Trip>();
			trip.BH_MessageStatus = ZString.Empty;
			var sendingParent = new eManifestMessageSendingObjectParent(trip);
			sendingParent.DefaultShouldSend();
			Assert(sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.UnassociatedShipments && x.ActionCode == MessageSubTypes.Create).ShouldSend);
			Assert(sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.PreliminaryTrip && x.ActionCode == MessageSubTypes.Create).ShouldSend);
			Assert(sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.CrewAndPassenger && x.ActionCode == MessageSubTypes.Create).ShouldSend);
			Assert(sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.PreliminaryTrip && x.ActionCode == MessageSubTypes.Confirmation).ShouldSend);
			var clearedUnassociatedMessage = trip.Messages.AddNew();
			clearedUnassociatedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USeManifest;
			clearedUnassociatedMessage.EM_MessageType = MessageTypes.Codes.UnassociatedShipments;
			clearedUnassociatedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			clearedUnassociatedMessage.EM_Status = EDIMessage.Status.Received;
			clearedUnassociatedMessage.EM_MessageSubType = EntryStatusList.Codes.Clear;
			sendingParent = new eManifestMessageSendingObjectParent(trip);
			sendingParent.DefaultShouldSend();
			Assert(!sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.UnassociatedShipments && x.ActionCode == MessageSubTypes.Create).ShouldSend);
			Assert(sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.PreliminaryTrip && x.ActionCode == MessageSubTypes.Create).ShouldSend);
			Assert(sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.CrewAndPassenger && x.ActionCode == MessageSubTypes.Create).ShouldSend);
			Assert(sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.PreliminaryTrip && x.ActionCode == MessageSubTypes.Confirmation).ShouldSend);
			var clearedPreliminaryTripMessage = trip.Messages.AddNew();
			clearedPreliminaryTripMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USeManifest;
			clearedPreliminaryTripMessage.EM_MessageType = MessageTypes.Codes.PreliminaryTrip;
			clearedPreliminaryTripMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			clearedPreliminaryTripMessage.EM_Status = EDIMessage.Status.Received;
			clearedPreliminaryTripMessage.EM_MessageSubType = EntryStatusList.Codes.Clear;
			sendingParent = new eManifestMessageSendingObjectParent(trip);
			sendingParent.DefaultShouldSend();
			Assert(!sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.UnassociatedShipments && x.ActionCode == MessageSubTypes.Create).ShouldSend);
			Assert(!sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.PreliminaryTrip && x.ActionCode == MessageSubTypes.Create).ShouldSend);
			Assert(sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.CrewAndPassenger && x.ActionCode == MessageSubTypes.Create).ShouldSend);
			Assert(sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.PreliminaryTrip && x.ActionCode == MessageSubTypes.Confirmation).ShouldSend);
			var clearedCrewAndPassengerMessage = trip.Messages.AddNew();
			clearedCrewAndPassengerMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USeManifest;
			clearedCrewAndPassengerMessage.EM_MessageType = MessageTypes.Codes.CrewAndPassenger;
			clearedCrewAndPassengerMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			clearedCrewAndPassengerMessage.EM_Status = EDIMessage.Status.Received;
			clearedCrewAndPassengerMessage.EM_MessageSubType = EntryStatusList.Codes.Clear;
			sendingParent = new eManifestMessageSendingObjectParent(trip);
			sendingParent.DefaultShouldSend();
			Assert(!sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.UnassociatedShipments && x.ActionCode == MessageSubTypes.Create).ShouldSend);
			Assert(!sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.PreliminaryTrip && x.ActionCode == MessageSubTypes.Create).ShouldSend);
			Assert(!sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.CrewAndPassenger && x.ActionCode == MessageSubTypes.Create).ShouldSend);
			Assert(sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.PreliminaryTrip && x.ActionCode == MessageSubTypes.Confirmation).ShouldSend);
		}

		public void TestDefaultShouldSendWhenExistShipmentConsistsOfEmptyIIT()
		{
			var trip = Factory.New<Trip>();
			trip.BH_MessageStatus = ZString.Empty;
			var sendingParent = new eManifestMessageSendingObjectParent(trip);
			sendingParent.DefaultShouldSend();
			Assert(sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.UnassociatedShipments && x.ActionCode == MessageSubTypes.Create).ShouldSend);

			var equipment = trip.AllEquipmentIncludingMainConveyance.AddNew();
			equipment.BJ_EmptyIITsCoveredByCarrier = true;
			sendingParent = new eManifestMessageSendingObjectParent(trip);
			sendingParent.DefaultShouldSend();
			Assert(!sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.UnassociatedShipments && x.ActionCode == MessageSubTypes.Create).ShouldSend);

			equipment.BJ_EmptyIITsCoveredByImporter = true;
			sendingParent = new eManifestMessageSendingObjectParent(trip);
			sendingParent.DefaultShouldSend();
			Assert(!sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.UnassociatedShipments && x.ActionCode == MessageSubTypes.Create).ShouldSend);

			equipment.BJ_EmptyIITsCoveredByCarrier = false;
			sendingParent = new eManifestMessageSendingObjectParent(trip);
			sendingParent.DefaultShouldSend();
			Assert(!sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.UnassociatedShipments && x.ActionCode == MessageSubTypes.Create).ShouldSend);

			equipment.BJ_EmptyIITsCoveredByImporter = false;
			sendingParent = new eManifestMessageSendingObjectParent(trip);
			sendingParent.DefaultShouldSend();
			Assert(sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.UnassociatedShipments && x.ActionCode == MessageSubTypes.Create).ShouldSend);
		}

		public void TestAllShipmentsHaveSameReleaseStatus()
		{
			var trip = Factory.New<Trip>();
			trip.BH_MessageStatus = ZString.Empty;
			var sendingParent = new eManifestMessageSendingObjectParent(trip);
			Assert(sendingParent.AllShipmentsHaveSameReleaseStatus);
			var shipment = trip.Shipments.AddNew();
			shipment.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Accepted;
			sendingParent = new eManifestMessageSendingObjectParent(trip);
			Assert(sendingParent.AllShipmentsHaveSameReleaseStatus);
			var shipment2 = trip.Shipments.AddNew();
			shipment2.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.ArrivalOfInbondBillOfLading;
			sendingParent = new eManifestMessageSendingObjectParent(trip);
			Assert(!sendingParent.AllShipmentsHaveSameReleaseStatus);
			shipment.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.ArrivalOfInbondBillOfLading;
			sendingParent = new eManifestMessageSendingObjectParent(trip);
			Assert(sendingParent.AllShipmentsHaveSameReleaseStatus);
		}

		public void TestIsExistShipmentConsistsOfEmptyIIT()
		{
			var trip = Factory.New<Trip>();
			trip.BH_MessageStatus = ZString.Empty;
			var sendingParent = new eManifestMessageSendingObjectParent(trip);
			Assert(!sendingParent.IsExistShipmentConsistsOfEmptyIIT);
			var equipment = trip.AllEquipmentIncludingMainConveyance.AddNew();
			equipment.BJ_EmptyIITsCoveredByCarrier = true;
			sendingParent = new eManifestMessageSendingObjectParent(trip);
			Assert(sendingParent.IsExistShipmentConsistsOfEmptyIIT);
			equipment.BJ_EmptyIITsCoveredByImporter = true;
			sendingParent = new eManifestMessageSendingObjectParent(trip);
			Assert(sendingParent.IsExistShipmentConsistsOfEmptyIIT);
			equipment.BJ_EmptyIITsCoveredByCarrier = false;
			sendingParent = new eManifestMessageSendingObjectParent(trip);
			Assert(sendingParent.IsExistShipmentConsistsOfEmptyIIT);
			equipment.BJ_EmptyIITsCoveredByImporter = false;
			sendingParent = new eManifestMessageSendingObjectParent(trip);
			Assert(!sendingParent.IsExistShipmentConsistsOfEmptyIIT);
		}

		public void TestSendingObjectsCollection()
		{
			var sendingParent = new eManifestMessageSendingObjectParent(Factory.New<Trip>());
			AssertEquals(4, sendingParent.SendingObjectsCollection.Count);
			var child0 = sendingParent.SendingObjectsCollection[0];
			var child1 = sendingParent.SendingObjectsCollection[1];
			var child2 = sendingParent.SendingObjectsCollection[2];
			var child3 = sendingParent.SendingObjectsCollection[3];
			AssertEquals(MessageTypes.Codes.UnassociatedShipments, child0.MessageType);
			AssertEquals(MessageSubTypes.Create, child0.ActionCode);
			AssertEquals((short)1, child0.Order);
			AssertEquals(MessageTypes.Codes.PreliminaryTrip, child1.MessageType);
			AssertEquals(MessageSubTypes.Create, child1.ActionCode);
			AssertEquals((short)2, child1.Order);
			AssertEquals(MessageTypes.Codes.CrewAndPassenger, child2.MessageType);
			AssertEquals(MessageSubTypes.Create, child2.ActionCode);
			AssertEquals((short)3, child2.Order);
			AssertEquals(MessageTypes.Codes.PreliminaryTrip, child3.MessageType);
			AssertEquals(MessageSubTypes.Confirmation, child3.ActionCode);
			AssertEquals((short)4, child3.Order);
		}

		public void TestSecurityRightToSendWithMessageErrors()
		{
			var testWrapper = new eManifestMessageSendingObjectParent(Factory.New<Trip>());
			AssertEquals(Env.Security.CustomsDeclarationSendWithMessageErrors, testWrapper.SecurityCheckpointToSendWithMessageError);
		}

		protected override BusinessObject GetNewBusinessObject() => new eManifestMessageSendingObjectParent(Factory.New<Trip>());
	}
}
