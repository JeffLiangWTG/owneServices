using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Customs.Business.MessageManagers.Testing;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Customs.US.eManifest.Messaging.MessageProcessors.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Messaging.Testing
{
	[TestedType(typeof(eManifestMessageManager))]
	sealed class eManifestMessageManagerTest : EDIFACTMessageManagerTestCase
	{
		public void TestEM_ApplicationReferenceAndEM_StatusOfMultipleCompleteManifestMessages_Original()
		{
			using (USeManifestDataRegistry.Instance.MaximumShipmentsToSendInOneMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				trip.Shipments.AddNew();
				trip.Shipments.AddNew();
				trip.Shipments.AddNew();
				Factory.Save();

				dataWrapper = new eManifestMessageWrapper(trip, MessageTypes.Codes.eManifest);
				var manager = new eManifestMessageManagerForTesting(dataWrapper, MessageTypes.Codes.eManifest);
				manager.SendMessage(MessageSubTypes.Create, false);

				AssertEquals(MessageStatusList.Codes.AwaitingOriginal, dataWrapper.MessageStatus);
				var messages = dataWrapper.Messages.Cast<EDIMessage>().OrderBy(x => x.EM_MessageNum).ToArray();
				var message1 = messages[0];
				AssertEquals(EDIMessage.Status.Queued, message1.EM_Status);
				AssertEquals(message1.EM_MessageNum, message1.EM_ApplicationReference);
				var message2 = messages[1];
				AssertEquals(EDIMessage.Status.Pending, message2.EM_Status);
				AssertEquals(message1.EM_MessageNum, message2.EM_ApplicationReference);
			}
		}
		public void TestEM_ApplicationReferenceAndEM_StatusOfMultipleCompleteManifestMessages_Change()
		{
			using (USeManifestDataRegistry.Instance.MaximumShipmentsToSendInOneMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				trip.Shipments.AddNew();
				trip.Shipments.AddNew();
				trip.Shipments.AddNew();
				Factory.Save();

				dataWrapper = new eManifestMessageWrapper(trip, MessageTypes.Codes.eManifest);
				var manager = new eManifestMessageManagerForTesting(dataWrapper, MessageTypes.Codes.eManifest);
				manager.SendMessage(MessageSubTypes.Change, false);

				AssertEquals(MessageStatusList.Codes.AwaitingChange, dataWrapper.MessageStatus);
				var messages = dataWrapper.Messages.Cast<EDIMessage>().OrderBy(x => x.EM_MessageNum).ToArray();
				var message1 = messages[0];
				AssertEquals(EDIMessage.Status.Queued, message1.EM_Status);
				AssertEquals(message1.EM_MessageNum, message1.EM_ApplicationReference);
				var message2 = messages[1];
				AssertEquals(EDIMessage.Status.Pending, message2.EM_Status);
				AssertEquals(message1.EM_MessageNum, message2.EM_ApplicationReference);
			}
		}

		public override void TestCanSendThisMessage()
		{
			ZString messageText;
			var manager = (eManifestMessageManagerForTesting)messageManager;
			Assert("Job not yet saved", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
			AssertEquals("Job not yet saved, Please save before sending.", messageText);
			var usCompany = Factory.NewWithValidTestData<GlbCompany>();
			usCompany.GC_RN_NKCountryCode = Constants.CountryCodes.UnitedStates;
			usCompany.Branches.Add(Factory.NewWithValidTestData<GlbBranch>());
			Factory.Save();
			using (DisposableEnvironment.ForBranch(usCompany.FirstActiveBranch.PK.ToGuid()))
			{
				manager = (eManifestMessageManagerForTesting)GetMessageManager();
				Assert(manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
			}
		}

		public void TestCanSendUnassociatedShipments()
		{
			ZString messageText;
			dataWrapper = new eManifestMessageWrapper(trip, MessageTypes.Codes.UnassociatedShipments);
			var manager = new eManifestMessageManagerForTesting(dataWrapper, MessageTypes.Codes.UnassociatedShipments);
			Factory.Save();
			Assert("No shipments to send", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
			AssertEquals("there are no shipments entered.", messageText);
			trip.Shipments.AddNew();
			Factory.Save();
			dataWrapper = new eManifestMessageWrapper(trip, MessageTypes.Codes.UnassociatedShipments);
			manager = new eManifestMessageManagerForTesting(dataWrapper, MessageTypes.Codes.UnassociatedShipments);
			Assert("Shipment can be sent", manager.CanSendThisMessage_Exposed(MessageSubTypes.Create, out messageText));
		}

		public void TestCanSendReplaceHeaderOnlyOrCancellation()
		{
			ZString messageText;
			dataWrapper = new eManifestMessageWrapper(trip, MessageTypes.Codes.eManifest);
			var manager = new eManifestMessageManagerForTesting(dataWrapper, MessageTypes.Codes.eManifest);
			Factory.Save();
			const string errorMessage = "original message has not been lodged yet.";
			Assert("Job has not been lodged", !manager.CanSendThisMessage_Exposed(MessageSubTypes.ReplaceHeader, out messageText));
			AssertEquals(errorMessage, messageText);
			Assert("Job has not been lodged", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Withdraw, out messageText));
			AssertEquals(errorMessage, messageText);
			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.HoldTrip;
			Factory.Save();
			Assert("Job has been lodged", manager.CanSendThisMessage_Exposed(MessageSubTypes.ReplaceHeader, out messageText));
			Assert("Job has been lodged", manager.CanSendThisMessage_Exposed(MessageSubTypes.Withdraw, out messageText));
		}

		public void TestCanSendPreliminaryTripWithoutLodgedShipments()
		{
			ZString messageText;
			dataWrapper = new eManifestMessageWrapper(trip, MessageTypes.Codes.PreliminaryTrip);
			var manager = new eManifestMessageManagerForTesting(dataWrapper, MessageTypes.Codes.PreliminaryTrip);
			Factory.Save();
			manager.Notification.NextAnswer = false;
			Assert("No shipments", manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
			trip.Shipments.AddNew();
			Factory.Save();
			dataWrapper = new eManifestMessageWrapper(trip, MessageTypes.Codes.PreliminaryTrip);
			manager = new eManifestMessageManagerForTesting(dataWrapper, MessageTypes.Codes.PreliminaryTrip);
			manager.Notification.NextAnswer = false;
			Assert("No lodged shipments", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
			AssertEquals(string.Empty, messageText);
			AssertEquals(@"The entered shipments are not lodged with Customs and not going to be linked to the trip.
In order to lodge the shipments you have to submit unassociated shipments first.
Are you sure you want to submit the empty trip without shipments?", manager.Notification.LastMessage);
			manager.Notification.NextAnswer = true;
			Assert("User confirms sending empty trip", manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
			var shipment = trip.Shipments.AddNew();
			shipment.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Accepted;
			Factory.Save();
			dataWrapper = new eManifestMessageWrapper(trip, MessageTypes.Codes.PreliminaryTrip);
			manager = new eManifestMessageManagerForTesting(dataWrapper, MessageTypes.Codes.PreliminaryTrip);
			manager.Notification.NextAnswer = false;
			Assert("There are some lodged shipments", manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
		}

		public void TestCanSendPreliminaryTripConfirmation()
		{
			ZString messageText;
			trip.Shipments.AddNew();
			dataWrapper = new eManifestMessageWrapper(trip, MessageTypes.Codes.PreliminaryTrip);
			var manager = new eManifestMessageManagerForTesting(dataWrapper, MessageTypes.Codes.PreliminaryTrip);
			manager.Notification.NextAnswer = false;
			Factory.Save();
			var errorMessage = "preliminary trip has not been lodged yet.";
			Assert("Job has not been lodged", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Confirmation, out messageText));
			AssertEquals(errorMessage, messageText);
			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedPreliminary;
			Factory.Save();
			errorMessage = "crew/passengers information has not been lodged yet.";
			Assert("Crew info has not been lodged", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Confirmation, out messageText));
			AssertEquals(errorMessage, messageText);
			trip.Messages.Add(MessagingTestHelper.GetReceivedEDIMessage(Factory, MessageTypes.Codes.CrewAndPassenger, TripEntryStatusList.Codes.AcceptedPreliminary));
			Factory.Save();
			Assert("Job has been lodged", manager.CanSendThisMessage_Exposed(MessageSubTypes.Confirmation, out messageText));
			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedComplete;
			Factory.Save();
			errorMessage = "the e-Manifest has already been marked as complete.";
			Assert("Job has been completed", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Confirmation, out messageText));
			AssertEquals(errorMessage, messageText);
		}

		public void TestCanSendCrewAndPassengerDetails()
		{
			ZString messageText;
			dataWrapper = new eManifestMessageWrapper(trip, MessageTypes.Codes.CrewAndPassenger);
			var manager = new eManifestMessageManagerForTesting(dataWrapper, MessageTypes.Codes.CrewAndPassenger);
			Factory.Save();
			Assert("Job has not been lodged", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
			AssertEquals("preliminary trip has not been lodged yet.", messageText);
			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedPreliminary;
			Factory.Save();
			Assert("Job has been lodged", manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
		}

		public void TestCanSendPreliminaryMessagesWhenManifestIsComplete()
		{
			const string expectedMessage = @"the e-Manifest has already been
marked as complete and preliminary manifest messages cannot be used. Please submit
Complete e-Manifest to amend the manifest or send a cancellation message if necessary.";
			ZString messageText;
			trip.Shipments.AddNew();
			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedComplete;
			dataWrapper = new eManifestMessageWrapper(trip, MessageTypes.Codes.UnassociatedShipments);
			var manager = new eManifestMessageManagerForTesting(dataWrapper, MessageTypes.Codes.UnassociatedShipments);
			Factory.Save();
			Assert("Job has been completed", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
			AssertEquals(expectedMessage, messageText);
			dataWrapper = new eManifestMessageWrapper(trip, MessageTypes.Codes.PreliminaryTrip);
			manager = new eManifestMessageManagerForTesting(dataWrapper, MessageTypes.Codes.PreliminaryTrip);
			Assert("Job has been completed", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
			AssertEquals(expectedMessage, messageText);
			dataWrapper = new eManifestMessageWrapper(trip, MessageTypes.Codes.CrewAndPassenger);
			manager = new eManifestMessageManagerForTesting(dataWrapper, MessageTypes.Codes.CrewAndPassenger);
			Assert("Job has been completed", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
			AssertEquals(expectedMessage, messageText);
		}

		public void TestCanSendIfCrewNotRegistered()
		{
			ZString messageText;
			var crew = trip.CrewMembers.AddNew();
			crew.CP_Type = CrewTypes.Codes.CrewMember;
			dataWrapper = new eManifestMessageWrapper(trip, MessageTypes.Codes.eManifest);
			var manager = new eManifestMessageManagerForTesting(dataWrapper, MessageTypes.Codes.eManifest);
			Factory.Save();
			Assert("Crew is not registered", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
			AssertEquals(@"some of the crew members are not registered.
If registration is not applicable for a crew member, use preliminary manifest and submit complete Crew/Passengers Details.", messageText);
			eManifestMessageWrapperTest.AddDocOrNumber(crew.Certificates, CrewACEIdTypes.Codes.Id, "14135");
			Factory.Save();
			Assert("Crew is registered", manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
			crew.Certificates.DeleteAll();
			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedComplete;
			manager.Notification.NextAnswer = false;
			Factory.Save();
			Assert("User cancels sending of manifest without crew info", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
			AssertEquals(string.Empty, messageText);
			AssertEquals(@"Crew information is not going to be updated because some crew members are not registered.
Only pre-registered crew can be amended using the Complete e-Manifest message.
Unregistered Crew can be submitted or amended in the preliminary manifest.
Are you sure you want to amend all e-Manifest data not including crew details?", manager.Notification.LastMessage);
			manager.Notification.NextAnswer = true;
			Assert("User confirms sending of manifest without crew info", manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
		}

		public void TestCanSendIfEquipmentIsDeleted()
		{
			const string expectedMessage = @"previously lodged equipment
cannot be deleted but can only be replaced with another equipment. In order to delete
equipment completely you have to cancel the job and resubmit it again without equipment.";
			ZString messageText;
			trip.Messages.Add(MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.PreliminaryTrip, "1", eManifestOriginalMessageWrapperTest.PreliminaryTripMessageText));
			trip.Messages.Add(MessagingTestHelper.GetReceivedEDIMessage(Factory, MessageTypes.Codes.PreliminaryTrip, TripEntryStatusList.Codes.AcceptedPreliminary, "1"));
			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedPreliminary;
			dataWrapper = new eManifestMessageWrapper(trip, MessageTypes.Codes.PreliminaryTrip);
			var manager = new eManifestMessageManagerForTesting(dataWrapper, MessageTypes.Codes.PreliminaryTrip);
			Factory.Save();
			Assert("Equipment cannot be deleted", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
			AssertEquals(expectedMessage, messageText);
			dataWrapper = new eManifestMessageWrapper(trip, MessageTypes.Codes.eManifest);
			manager = new eManifestMessageManagerForTesting(dataWrapper, MessageTypes.Codes.eManifest);
			Assert("Equipment cannot be deleted", !manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
			AssertEquals(expectedMessage, messageText);
			trip.Equipment.AddNew().BJ_RQ_Equipment = Factory.NewWithValidTestData<RefEquipment>().PK;
			Factory.Save();
			Assert("Equipment can be replaced", manager.CanSendThisMessage_Exposed(MessageSubTypes.Undefined, out messageText));
		}

		public void TestSkipNotification()
		{
			trip.Shipments.AddNew();
			Factory.Save();
			dataWrapper = new eManifestMessageWrapper(trip, MessageTypes.Codes.UnassociatedShipments);
			var manager = new eManifestMessageManagerForTesting(dataWrapper, MessageTypes.Codes.UnassociatedShipments);
			manager.SendMessage(MessageSubTypes.Undefined, false);
			AssertNotNull("Should show notifications.", manager.Notification.LastMessage);
			trip.BH_MessageStatus = ZString.Empty;
			Factory.Save();
			dataWrapper = new eManifestMessageWrapper(trip, MessageTypes.Codes.UnassociatedShipments);
			manager = new eManifestMessageManagerForTesting(dataWrapper, MessageTypes.Codes.UnassociatedShipments, true);
			manager.SendMessage(MessageSubTypes.Undefined, false);
			AssertNull("Should skip notifications.", manager.Notification.LastMessage);
		}

		public void TestSkipShowAwaitingCustomsResponseConfirmation_WhenShouldSkipNotificationIsTrue()
		{
			trip.Shipments.AddNew();
			trip.BH_MessageStatus = MessageStatusList.Codes.AwaitingChange;
			Factory.Save();
			dataWrapper = new eManifestMessageWrapper(trip, MessageTypes.Codes.UnassociatedShipments);
			var manager = new eManifestMessageManagerForTesting(dataWrapper, MessageTypes.Codes.UnassociatedShipments, false);
			manager.Notification.NextAnswer = false;
			manager.SendMessage(MessageSubTypes.Undefined, false);
			AssertNotNull("Should show awaiting customs response confirmation when should skip notification is false.", manager.Notification.LastMessage);
			dataWrapper = new eManifestMessageWrapper(trip, MessageTypes.Codes.UnassociatedShipments);
			manager = new eManifestMessageManagerForTesting(dataWrapper, MessageTypes.Codes.UnassociatedShipments, true);
			manager.Notification.NextAnswer = false;
			manager.SendMessage(MessageSubTypes.Undefined, false);
			AssertNull("Shouldn't show awaiting customs response confirmation when should skip notification is true. ", manager.Notification.LastMessage);
		}

		public override void TestGetMessageBuilder()
		{
			var manager = ((eManifestMessageManagerForTesting)messageManager);
			AssertEquals(typeof(CompleteManifestMessageBuilder), manager.GetMessageBuilder_Exposed(MessageSubTypes.Undefined).GetType());
			manager = new eManifestMessageManagerForTesting(dataWrapper, MessageTypes.Codes.CrewAndPassenger);
			AssertEquals(typeof(CrewAndPassengersMessageBuilder), manager.GetMessageBuilder_Exposed(MessageSubTypes.Undefined).GetType());
			manager = new eManifestMessageManagerForTesting(dataWrapper, MessageTypes.Codes.UnassociatedShipments);
			AssertEquals(typeof(CompleteManifestMessageBuilder), manager.GetMessageBuilder_Exposed(MessageSubTypes.Undefined).GetType());
			manager = new eManifestMessageManagerForTesting(dataWrapper, MessageTypes.Codes.CompleteTrip);
			AssertEquals(typeof(TripReportMessageBuilder), manager.GetMessageBuilder_Exposed(MessageSubTypes.Undefined).GetType());
			manager = new eManifestMessageManagerForTesting(dataWrapper, MessageTypes.Codes.PreliminaryTrip);
			AssertEquals(typeof(TripReportMessageBuilder), manager.GetMessageBuilder_Exposed(MessageSubTypes.Undefined).GetType());
			manager = new eManifestMessageManagerForTesting(dataWrapper, MessageTypes.Codes.CrewOrEquipmentRegistration);
			AssertEquals(typeof(CrewOrEquipmentRegistrationMessageBuilder), manager.GetMessageBuilder_Exposed(MessageSubTypes.Undefined).GetType());
		}

		public override void TestMessageFriendlyName()
		{
			AssertEquals("MessageFriendlyName", "Complete e-Manifest w/ACE ID", messageManager.MessageFriendlyName);
		}

		public override void TestPopulateMessages()
		{
			((eManifestMessageManagerForTesting)messageManager).PopulateMessage_Exposed(MessageSubTypes.Create);
			AssertEquals("1 message", 1, trip.Messages.Count);
			AssertEquals("EM_MessageSubType", MessageSubTypeCodes.Codes.Original, trip.Messages[0].EM_MessageSubType);
			AssertEquals("Message status", MessageStatusList.Codes.AwaitingOriginal, trip.BH_MessageStatus);
		}

		public override void SetTestMode(bool testMode)
		{
			((eManifestMessageManagerForTesting)messageManager).shouldSendMessagesInTestMode = testMode;
		}

		protected override EDIFACTMessageManager GetMessageManager()
		{
			return new eManifestMessageManagerForTesting(dataWrapper, MessageTypes.Codes.eManifest);
		}

		protected override IEDIFACTMessageAttachee GetDataWrapper()
		{
			trip = Factory.New<Trip>();
			trip.OnSaving();
			return new eManifestMessageWrapper(trip, MessageTypes.Codes.eManifest);
		}

		Trip trip;
		sealed class eManifestMessageManagerForTesting : eManifestMessageManager
		{
			internal eManifestMessageManagerForTesting(IEDIFACTMessageAttachee dataWrapper, string messageType, bool shouldSkipNotification = false) : base(dataWrapper, messageType, new TestUserNotification(), shouldSkipNotification)
			{
			}

			internal IMessageBuilder GetMessageBuilder_Exposed(MessageSubTypes actionCode)
			{
				return GetMessageBuilder(actionCode);
			}

			internal void PopulateMessage_Exposed(MessageSubTypes actionCode)
			{
				PopulateMessage(actionCode);
			}

			internal bool CanSendThisMessage_Exposed(MessageSubTypes actionCode, out ZString messageText)
			{
				return CanSendThisMessage(actionCode, out messageText);
			}

			internal TestUserNotification Notification => (TestUserNotification)notification;

			internal class TestUserNotification : Customs.Business.MessageManagers.Testing.TestUserNotification, IUserNotification
			{
				public bool ShowShipmentsActionsDialog(IShipmentActionsProvider provider, ZString messageDescription) => true;
			}
		}
	}
}
