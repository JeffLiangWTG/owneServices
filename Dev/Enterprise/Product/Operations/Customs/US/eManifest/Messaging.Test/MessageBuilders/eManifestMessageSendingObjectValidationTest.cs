using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.US.eManifest.Business;

namespace Enterprise.Customs.US.eManifest.Messaging.Testing
{
	sealed class eManifestMessageSendingObjectValidationTest : TestCaseWithFactory
	{
		public void TestCheckShouldSend()
		{
			var clearedUnassociatedMessage = Factory.New<EDIMessage>();
			clearedUnassociatedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USeManifest;
			clearedUnassociatedMessage.EM_MessageType = MessageTypes.Codes.UnassociatedShipments;
			clearedUnassociatedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			clearedUnassociatedMessage.EM_Status = EDIMessage.Status.Received;
			clearedUnassociatedMessage.EM_MessageSubType = EntryStatusList.Codes.Clear;
			var clearedPreliminaryTripMessage = Factory.New<EDIMessage>();
			clearedPreliminaryTripMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USeManifest;
			clearedPreliminaryTripMessage.EM_MessageType = MessageTypes.Codes.PreliminaryTrip;
			clearedPreliminaryTripMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			clearedPreliminaryTripMessage.EM_Status = EDIMessage.Status.Received;
			clearedPreliminaryTripMessage.EM_MessageSubType = EntryStatusList.Codes.Clear;
			var clearedCrewAndPassengerMessage = Factory.New<EDIMessage>();
			clearedCrewAndPassengerMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USeManifest;
			clearedCrewAndPassengerMessage.EM_MessageType = MessageTypes.Codes.CrewAndPassenger;
			clearedCrewAndPassengerMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			clearedCrewAndPassengerMessage.EM_Status = EDIMessage.Status.Received;
			clearedCrewAndPassengerMessage.EM_MessageSubType = EntryStatusList.Codes.Clear;
			var trip = Factory.New<Trip>();
			var shipment = trip.Shipments.AddNew();
			shipment.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Accepted;
			var shipment2 = trip.Shipments.AddNew();
			shipment2.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.ArrivalOfInbondBillOfLading;
			var sendingParent = new eManifestMessageSendingObjectParent(trip);
			AssertEquals(4, sendingParent.SendingObjectsCollection.Count);
			var unassociatedShipments = sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.UnassociatedShipments && x.ActionCode == MessageSubTypes.Create);
			var preliminaryTrip = sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.PreliminaryTrip && x.ActionCode == MessageSubTypes.Create);
			var completeTrip = sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.PreliminaryTrip && x.ActionCode == MessageSubTypes.Confirmation);
			unassociatedShipments.ShouldSend = false;
			preliminaryTrip.ShouldSend = true;
			AssertHasError(preliminaryTrip.ShouldSendInfo, "Unassociated Shipments message should be sent before this message.");
			trip.BH_MessageStatus = "CLO";
			trip.Messages.Add(clearedUnassociatedMessage);
			sendingParent = new eManifestMessageSendingObjectParent(trip);
			preliminaryTrip = sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.PreliminaryTrip && x.ActionCode == MessageSubTypes.Create);
			unassociatedShipments = sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.UnassociatedShipments && x.ActionCode == MessageSubTypes.Create);
			preliminaryTrip.Validation.ValidateShouldSend();
			AssertNoError(preliminaryTrip.ShouldSendInfo, "Unassociated Shipments message should be sent before this message.");
			unassociatedShipments.ShouldSend = true;
			AssertHasError(unassociatedShipments.ShouldSendInfo, "Unassociated Shipments message should not be sent from this menu since there are shipments with different release status, please send via 'Submit Unassociated Shipments' instead.");
			shipment2.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Accepted;
			sendingParent = new eManifestMessageSendingObjectParent(trip);
			unassociatedShipments = sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.UnassociatedShipments && x.ActionCode == MessageSubTypes.Create);
			preliminaryTrip = sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.PreliminaryTrip && x.ActionCode == MessageSubTypes.Create);
			unassociatedShipments.Validation.ValidateShouldSend();
			AssertNoError(unassociatedShipments.ShouldSendInfo, "Unassociated Shipments message should not be sent from this menu since there are shipments with different release status, please send via 'Submit Unassociated Shipments' instead.");
			preliminaryTrip.Validation.ValidateShouldSend();
			AssertNoError(preliminaryTrip.ShouldSendInfo, "Unassociated Shipments message should be sent before this message.");
			var crewAndPassenger = sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.CrewAndPassenger && x.ActionCode == MessageSubTypes.Create);
			preliminaryTrip.ShouldSend = false;
			crewAndPassenger.ShouldSend = true;
			AssertHasError(crewAndPassenger.ShouldSendInfo, "Preliminary Trip Details message should be sent before this message.");
			trip.Messages.Add(clearedPreliminaryTripMessage);
			sendingParent = new eManifestMessageSendingObjectParent(trip);
			crewAndPassenger = sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.CrewAndPassenger && x.ActionCode == MessageSubTypes.Create);
			crewAndPassenger.Validation.ValidateShouldSend();
			AssertNoError(crewAndPassenger.ShouldSendInfo, "Preliminary Trip Details message should be sent before this message.");
			crewAndPassenger.ShouldSend = false;
			completeTrip = sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.PreliminaryTrip && x.ActionCode == MessageSubTypes.Confirmation);
			completeTrip.ShouldSend = true;
			AssertHasError(completeTrip.ShouldSendInfo, "Crew/Passengers Details message should be sent before this message.");
			trip.Messages.Add(clearedCrewAndPassengerMessage);
			sendingParent = new eManifestMessageSendingObjectParent(trip);
			completeTrip = sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.PreliminaryTrip && x.ActionCode == MessageSubTypes.Confirmation);
			completeTrip.Validation.ValidateShouldSend();
			AssertNoError(completeTrip.ShouldSendInfo, "Crew/Passengers Details message should be sent before this message.");
		}

		public void TestCheckShouldSendWhenExistShipmentConsistsOfEmptyIIT()
		{
			var trip = Factory.New<Trip>();
			var sendingParent = new eManifestMessageSendingObjectParent(trip);
			AssertEquals(4, sendingParent.SendingObjectsCollection.Count);
			var unassociatedShipments = sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.UnassociatedShipments && x.ActionCode == MessageSubTypes.Create);
			var preliminaryTrip = sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.PreliminaryTrip && x.ActionCode == MessageSubTypes.Create);
			unassociatedShipments.ShouldSend = false;
			preliminaryTrip.ShouldSend = true;
			AssertHasError(preliminaryTrip.ShouldSendInfo, "Unassociated Shipments message should be sent before this message.");

			var equipment = trip.AllEquipmentIncludingMainConveyance.AddNew();
			equipment.BJ_EmptyIITsCoveredByCarrier = true;
			sendingParent = new eManifestMessageSendingObjectParent(trip);
			preliminaryTrip = sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.PreliminaryTrip && x.ActionCode == MessageSubTypes.Create);
			unassociatedShipments = sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.UnassociatedShipments && x.ActionCode == MessageSubTypes.Create);
			unassociatedShipments.ShouldSend = false;
			preliminaryTrip.ShouldSend = true;
			AssertNoError(preliminaryTrip.ShouldSendInfo, "Unassociated Shipments message should be sent before this message.");

			equipment.BJ_EmptyIITsCoveredByImporter = true;
			sendingParent = new eManifestMessageSendingObjectParent(trip);
			preliminaryTrip = sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.PreliminaryTrip && x.ActionCode == MessageSubTypes.Create);
			unassociatedShipments = sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.UnassociatedShipments && x.ActionCode == MessageSubTypes.Create);
			unassociatedShipments.ShouldSend = false;
			preliminaryTrip.ShouldSend = true;
			AssertNoError(preliminaryTrip.ShouldSendInfo, "Unassociated Shipments message should be sent before this message.");

			equipment.BJ_EmptyIITsCoveredByCarrier = false;
			sendingParent = new eManifestMessageSendingObjectParent(trip);
			preliminaryTrip = sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.PreliminaryTrip && x.ActionCode == MessageSubTypes.Create);
			unassociatedShipments = sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.UnassociatedShipments && x.ActionCode == MessageSubTypes.Create);
			unassociatedShipments.ShouldSend = false;
			preliminaryTrip.ShouldSend = true;
			AssertNoError(preliminaryTrip.ShouldSendInfo, "Unassociated Shipments message should be sent before this message.");

			equipment.BJ_EmptyIITsCoveredByImporter = false;
			sendingParent = new eManifestMessageSendingObjectParent(trip);
			preliminaryTrip = sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.PreliminaryTrip && x.ActionCode == MessageSubTypes.Create);
			unassociatedShipments = sendingParent.SendingObjectsCollection.Cast<eManifestMessageSendingObject>().First(x => x.MessageType == MessageTypes.Codes.UnassociatedShipments && x.ActionCode == MessageSubTypes.Create);
			unassociatedShipments.ShouldSend = false;
			preliminaryTrip.ShouldSend = true;
			AssertHasError(preliminaryTrip.ShouldSendInfo, "Unassociated Shipments message should be sent before this message.");
		}
	}
}
