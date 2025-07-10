using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Customs.US.eManifest.Messaging
{
	public class eManifestMessageSendingObjectParent : BaseMessageSendingObjectParent<eManifestMessageSendingObject>
	{
		public eManifestMessageSendingObjectParent(Trip topBusinessObject)
			: base(topBusinessObject.Factory)
		{
			this.Trip = topBusinessObject;
		}

		public Trip Trip { get; }

		public bool AllShipmentsHaveSameReleaseStatus
		{
			get
			{
				if (!allShipmentsHaveSameReleaseStatus.HasValue)
				{
					allShipmentsHaveSameReleaseStatus = Trip.ShipmentsReleaseStatuses.AllSame(x => x.B0_ReleaseStatus);
				}
				return allShipmentsHaveSameReleaseStatus.Value;
			}
		}
		bool? allShipmentsHaveSameReleaseStatus;

		public bool IsExistShipmentConsistsOfEmptyIIT
		{
			get
			{
				if (!isExistShipmentConsistsOfEmptyIIT.HasValue)
				{
					isExistShipmentConsistsOfEmptyIIT = Trip.AllEquipmentIncludingMainConveyance.Any(x => x.BJ_EmptyIITsCoveredByCarrier || x.BJ_EmptyIITsCoveredByImporter);
				}
				return isExistShipmentConsistsOfEmptyIIT.Value;
			}
		}
		bool? isExistShipmentConsistsOfEmptyIIT;

		public void DefaultShouldSend()
		{
			_ = SendingObjectsCollection; // ensure this collection is created and hence stop any stack overflow.
			if (HasClearUnassociatedShipmentsMessage)
			{
				if (HasClearPreliminaryTripMessage)
				{
					if (!HasClearCrewAndPassengerMessage)
					{
						CrewAndPassengerSendingObject.ShouldSend = true;
					}
				}
				else
				{
					PreliminaryTripSendingObject.ShouldSend = true;
					CrewAndPassengerSendingObject.ShouldSend = true;
				}
			}
			else
			{
				UnassociatedShipmentsSendingObject.ShouldSend = !IsExistShipmentConsistsOfEmptyIIT;
				PreliminaryTripSendingObject.ShouldSend = true;
				CrewAndPassengerSendingObject.ShouldSend = true;
			}
			CompleteTripSendingObject.ShouldSend = true;
		}
		public bool HasClearUnassociatedShipmentsMessage => HasMessage(ref hasClearUnassociatedShipmentsMessage, MessageTypes.Codes.UnassociatedShipments);
		bool? hasClearUnassociatedShipmentsMessage;

		public bool HasClearPreliminaryTripMessage => HasMessage(ref hasClearPreliminaryTripMessage, MessageTypes.Codes.PreliminaryTrip);
		bool? hasClearPreliminaryTripMessage;

		public bool HasClearCrewAndPassengerMessage => HasMessage(ref hasClearCrewAndPassengerMessage, MessageTypes.Codes.CrewAndPassenger);
		bool? hasClearCrewAndPassengerMessage;

		bool HasMessage(ref bool? cachedValue, ZString messageType)
		{
			if (!cachedValue.HasValue)
			{
				var messageSubType = Trip.Messages.GetLastMessage(EDIMessage.ApplicationCodes.USeManifest, messageType, EDIMessage.Direction.Receive, EDIMessage.Status.Received)?.EM_MessageSubType ?? ZString.Empty;
				cachedValue = !messageSubType.IsEmpty && messageSubType != EntryStatusList.Codes.Cancelled && messageSubType != TripEntryStatusList.Codes.Error;
			}
			return cachedValue.Value;
		}
		public override BusinessObject TopLevelBusinessObject => Trip;

		protected override NonPersistentBusinessObjectCollection<eManifestMessageSendingObject> GetSendingObjectsCollectionCore()
		{
			var collection = new eManifestMessageSendingObjectCollection(Factory);
			collection.Add(UnassociatedShipmentsSendingObject);
			collection.Add(PreliminaryTripSendingObject);
			collection.Add(CrewAndPassengerSendingObject);
			collection.Add(CompleteTripSendingObject);
			return collection;
		}

		eManifestMessageSendingObject UnassociatedShipmentsSendingObject => unassociatedShipmentsSendingObject ?? (unassociatedShipmentsSendingObject = new eManifestMessageSendingObject(this, MessageTypes.Codes.UnassociatedShipments, MessageSubTypes.Create, 1));
		eManifestMessageSendingObject unassociatedShipmentsSendingObject;

		eManifestMessageSendingObject PreliminaryTripSendingObject => preliminaryTripSendingObject ?? (preliminaryTripSendingObject = new eManifestMessageSendingObject(this, MessageTypes.Codes.PreliminaryTrip, MessageSubTypes.Create, 2));
		eManifestMessageSendingObject preliminaryTripSendingObject;

		eManifestMessageSendingObject CrewAndPassengerSendingObject => crewAndPassengerSendingObject ?? (crewAndPassengerSendingObject = new eManifestMessageSendingObject(this, MessageTypes.Codes.CrewAndPassenger, MessageSubTypes.Create, 3));
		eManifestMessageSendingObject crewAndPassengerSendingObject;

		eManifestMessageSendingObject CompleteTripSendingObject => completeTripSendingObject ?? (completeTripSendingObject = new eManifestMessageSendingObject(this, MessageTypes.Codes.PreliminaryTrip, MessageSubTypes.Confirmation, 4));
		eManifestMessageSendingObject completeTripSendingObject;
		public override SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.CustomsDeclarationSendWithMessageErrors;
	}
}
