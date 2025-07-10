using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageManagers.Testing;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Customs.US.eManifest.Messaging.MessageProcessors.Testing;

namespace Enterprise.Customs.US.eManifest.Messaging.Testing
{
	sealed class eManifestMessageManagerHelperTest : TestCaseWithFactory
	{
		public void TestCanSendThisMessage()
		{
			var trip = Factory.New<Trip>();
			var dataWrapper = new eManifestMessageWrapper(trip, MessageTypes.Codes.UnassociatedShipments);
			var result = eManifestMessageManagerHelper.CanSendThisMessage(dataWrapper, MessageTypes.Codes.UnassociatedShipments, MessageSubTypes.Create);
			AssertEquals("there are no shipments entered.", result.messageText);
			result = eManifestMessageManagerHelper.CanSendThisMessage(dataWrapper, MessageTypes.Codes.UnassociatedShipments, MessageSubTypes.Create, true);
			AssertEquals("there are no shipments entered.", result.messageText);
			trip.Shipments.AddNew();
			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedComplete;
			dataWrapper = new eManifestMessageWrapper(trip, MessageTypes.Codes.UnassociatedShipments);
			result = eManifestMessageManagerHelper.CanSendThisMessage(dataWrapper, MessageTypes.Codes.UnassociatedShipments, MessageSubTypes.Confirmation);
			AssertEquals("the e-Manifest has already been marked as complete.", result.messageText);
			result = eManifestMessageManagerHelper.CanSendThisMessage(dataWrapper, MessageTypes.Codes.UnassociatedShipments, MessageSubTypes.Confirmation, true);
			AssertEquals("the e-Manifest has already been marked as complete.", result.messageText);
			string expectedMessage = @"the e-Manifest has already been
marked as complete and preliminary manifest messages cannot be used. Please submit
Complete e-Manifest to amend the manifest or send a cancellation message if necessary.";
			dataWrapper = new eManifestMessageWrapper(trip, MessageTypes.Codes.UnassociatedShipments);
			result = eManifestMessageManagerHelper.CanSendThisMessage(dataWrapper, MessageTypes.Codes.UnassociatedShipments, MessageSubTypes.Undefined);
			AssertEquals(expectedMessage, result.messageText);
			result = eManifestMessageManagerHelper.CanSendThisMessage(dataWrapper, MessageTypes.Codes.UnassociatedShipments, MessageSubTypes.Undefined, true);
			AssertEquals(expectedMessage, result.messageText);
			expectedMessage = @"previously lodged equipment
cannot be deleted but can only be replaced with another equipment. In order to delete
equipment completely you have to cancel the job and resubmit it again without equipment.";
			trip.Messages.Add(MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.PreliminaryTrip, "1", eManifestOriginalMessageWrapperTest.PreliminaryTripMessageText));
			trip.Messages.Add(MessagingTestHelper.GetReceivedEDIMessage(Factory, MessageTypes.Codes.PreliminaryTrip, TripEntryStatusList.Codes.AcceptedPreliminary, "1"));
			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedPreliminary;
			dataWrapper = new eManifestMessageWrapper(trip, MessageTypes.Codes.PreliminaryTrip);
			result = eManifestMessageManagerHelper.CanSendThisMessage(dataWrapper, MessageTypes.Codes.PreliminaryTrip, MessageSubTypes.Undefined);
			AssertEquals(expectedMessage, result.messageText);
			Assert(!result.result);
			result = eManifestMessageManagerHelper.CanSendThisMessage(dataWrapper, MessageTypes.Codes.PreliminaryTrip, MessageSubTypes.Undefined, true);
			AssertEquals(ZString.Empty, result.messageText);
			Assert(result.result);
			expectedMessage = @"preliminary trip has not been lodged yet.";
			trip.BH_ReleaseStatus = MessageTypes.Codes.SyntaxError;
			dataWrapper = new eManifestMessageWrapper(trip, MessageTypes.Codes.CrewAndPassenger);
			result = eManifestMessageManagerHelper.CanSendThisMessage(dataWrapper, MessageTypes.Codes.CrewAndPassenger, MessageSubTypes.Confirmation);
			AssertEquals(expectedMessage, result.messageText);
			Assert(!result.result);
			result = eManifestMessageManagerHelper.CanSendThisMessage(dataWrapper, MessageTypes.Codes.CrewAndPassenger, MessageSubTypes.Confirmation, true);
			AssertEquals(ZString.Empty, result.messageText);
			Assert(result.result);
			expectedMessage = @"crew/passengers information has not been lodged yet.";
			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedPreliminary;
			result = eManifestMessageManagerHelper.CanSendThisMessage(dataWrapper, MessageTypes.Codes.CrewAndPassenger, MessageSubTypes.Confirmation);
			AssertEquals(expectedMessage, result.messageText);
			Assert(!result.result);
			result = eManifestMessageManagerHelper.CanSendThisMessage(dataWrapper, MessageTypes.Codes.CrewAndPassenger, MessageSubTypes.Confirmation, true);
			AssertEquals(ZString.Empty, result.messageText);
			Assert(result.result);
			expectedMessage = "original message has not been lodged yet.";
			trip.BH_ReleaseStatus = MessageTypes.Codes.SyntaxError;
			dataWrapper = new eManifestMessageWrapper(trip, MessageTypes.Codes.eManifest);
			result = eManifestMessageManagerHelper.CanSendThisMessage(dataWrapper, MessageTypes.Codes.eManifest, MessageSubTypes.ReplaceHeader);
			AssertEquals(expectedMessage, result.messageText);
			Assert(!result.result);
			result = eManifestMessageManagerHelper.CanSendThisMessage(dataWrapper, MessageTypes.Codes.eManifest, MessageSubTypes.ReplaceHeader, true);
			AssertEquals(ZString.Empty, result.messageText);
			Assert(result.result);
			expectedMessage = @"some of the crew members are not registered.
If registration is not applicable for a crew member, use preliminary manifest and submit complete Crew/Passengers Details.";
			var crew = trip.CrewMembers.AddNew();
			crew.CP_Type = CrewTypes.Codes.CrewMember;
			dataWrapper = new eManifestMessageWrapper(trip, MessageTypes.Codes.eManifest);
			result = eManifestMessageManagerHelper.CanSendThisMessage(dataWrapper, MessageTypes.Codes.eManifest, MessageSubTypes.Undefined);
			AssertEquals(expectedMessage, result.messageText);
			Assert(!result.result);
			result = eManifestMessageManagerHelper.CanSendThisMessage(dataWrapper, MessageTypes.Codes.eManifest, MessageSubTypes.Undefined, true);
			AssertEquals(expectedMessage, result.messageText);
			Assert(!result.result);

			var aceID = crew.Certificates.AddNew();
			aceID.XZ_Type = CrewACEIdTypes.Codes.Id;
			aceID.XZ_RefNumber = "ACE";
			dataWrapper = new eManifestMessageWrapper(trip, MessageTypes.Codes.CrewOrEquipmentRegistration);
			result = eManifestMessageManagerHelper.CanSendThisMessage(dataWrapper, MessageTypes.Codes.CrewOrEquipmentRegistration, MessageSubTypes.Undefined);
			expectedMessage = @"all Crew members already have ACE or APC IDs.";
			AssertEquals(expectedMessage, result.messageText);
			Assert(!result.result);

			crew = trip.CrewMembers.AddNew();
			dataWrapper = new eManifestMessageWrapper(trip, MessageTypes.Codes.CrewOrEquipmentRegistration);
			result = eManifestMessageManagerHelper.CanSendThisMessage(dataWrapper, MessageTypes.Codes.CrewOrEquipmentRegistration, MessageSubTypes.Undefined);
			AssertEquals(ZString.Empty, result.messageText);
			Assert(result.result);
		}

		public void TestShowSendWithoutShipmentsIfRequired()
		{
			var notification = new TestUserNotification();
			var trip = Factory.New<Trip>();
			trip.Shipments.AddNew();
			var dataWrapper = new eManifestMessageWrapper(trip, MessageTypes.Codes.PreliminaryTrip);
			var result = eManifestMessageManagerHelper.ShowSendWithoutShipmentsIfRequired(dataWrapper, notification, "Warnning");
			AssertEquals(@"The entered shipments are not lodged with Customs and not going to be linked to the trip.
In order to lodge the shipments you have to submit unassociated shipments first.
Are you sure you want to submit the empty trip without shipments?", notification.LastMessage);
		}

		public void TestShowSendWithoutCrewInfo()
		{
			var notification = new TestUserNotification();
			var result = eManifestMessageManagerHelper.ShowSendWithoutCrewInfo(notification, "Warnning");
			AssertEquals(@"Crew information is not going to be updated because some crew members are not registered.
Only pre-registered crew can be amended using the Complete e-Manifest message.
Unregistered Crew can be submitted or amended in the preliminary manifest.
Are you sure you want to amend all e-Manifest data not including crew details?", notification.LastMessage);
		}

		public void TestGetMessageBuilder()
		{
			var trip = Factory.New<Trip>();
			var sendingObjectParent = new eManifestMessageSendingObjectParent(trip);
			var wrapper = new eManifestMessageSendingObject(sendingObjectParent, MessageTypes.Codes.CrewOrEquipmentRegistration, MessageSubTypes.Create, 1);
			var messageBuilder = eManifestMessageManagerHelper.GetMessageBuilder(wrapper, wrapper.MessageType, MessageSubTypes.Create);
			AssertType<CrewOrEquipmentRegistrationMessageBuilder>(messageBuilder);
			wrapper = new eManifestMessageSendingObject(sendingObjectParent, MessageTypes.Codes.CrewAndPassenger, MessageSubTypes.Create, 1);
			messageBuilder = eManifestMessageManagerHelper.GetMessageBuilder(wrapper, wrapper.MessageType, MessageSubTypes.Create);
			AssertType<CrewAndPassengersMessageBuilder>(messageBuilder);
			wrapper = new eManifestMessageSendingObject(sendingObjectParent, MessageTypes.Codes.CompleteTrip, MessageSubTypes.Create, 1);
			messageBuilder = eManifestMessageManagerHelper.GetMessageBuilder(wrapper, wrapper.MessageType, MessageSubTypes.Create);
			AssertType<TripReportMessageBuilder>(messageBuilder);
			wrapper = new eManifestMessageSendingObject(sendingObjectParent, MessageTypes.Codes.PreliminaryTrip, MessageSubTypes.Create, 1);
			messageBuilder = eManifestMessageManagerHelper.GetMessageBuilder(wrapper, wrapper.MessageType, MessageSubTypes.Create);
			AssertType<TripReportMessageBuilder>(messageBuilder);
			wrapper = new eManifestMessageSendingObject(sendingObjectParent, MessageTypes.Codes.UnassociatedShipments, MessageSubTypes.Create, 1);
			messageBuilder = eManifestMessageManagerHelper.GetMessageBuilder(wrapper, wrapper.MessageType, MessageSubTypes.Create);
			AssertType<CompleteManifestMessageBuilder>(messageBuilder);
		}

		public void TestGetActionCodeDescription()
		{
			AssertEquals(MessageActionCodes.Descriptions.Change, eManifestMessageManagerHelper.GetActionCodeDescription(MessageSubTypes.Change));
			AssertEquals(MessageActionCodes.Descriptions.Cancellation, eManifestMessageManagerHelper.GetActionCodeDescription(MessageSubTypes.Withdraw));
			AssertEquals(MessageActionCodes.Descriptions.ChangeHeaderOnly, eManifestMessageManagerHelper.GetActionCodeDescription(MessageSubTypes.ReplaceHeader));
			AssertEquals(MessageActionCodes.Descriptions.Confirmation, eManifestMessageManagerHelper.GetActionCodeDescription(MessageSubTypes.Confirmation));
			AssertEquals(MessageActionCodes.Descriptions.Original, eManifestMessageManagerHelper.GetActionCodeDescription(MessageSubTypes.AddLines));
		}
	}
}
