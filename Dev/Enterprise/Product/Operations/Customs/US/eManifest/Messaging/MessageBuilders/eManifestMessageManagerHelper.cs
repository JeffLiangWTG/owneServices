using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.US.eManifest.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.Messaging
{
	public static class eManifestMessageManagerHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static (ZBool result, ZString messageText) CanSendThisMessage(ICompleteManifest dataWrapper, ZString messageType, MessageSubTypes actionCode, bool ignoreLodged = false)
		{
			var statusCalculator = new eManifestStatusCalculator(messageType);
			var result = true;
			var messageText = ZString.Empty;
			var jobStatus = ignoreLodged ? ZString.Empty : dataWrapper.JobStatus;
			var isLodged = !ignoreLodged && statusCalculator.IsLodged(jobStatus);
			var isNotLodged = !ignoreLodged && !statusCalculator.IsLodged(jobStatus);
			var isEquipmentInfoLodged = !ignoreLodged && statusCalculator.IsEquipmentInfoLodged(dataWrapper);
			var isCrewInfoNotLodged = !ignoreLodged && !statusCalculator.IsCrewInfoLodged(dataWrapper);

			if (messageType == MessageTypes.Codes.UnassociatedShipments && !dataWrapper.Shipments.Any())
			{
				result = false;
				messageText = Res.GetString("10519f27-ce76-42b0-9908-f6303f6a4a73", "there are no shipments entered.");
			}
			else if (actionCode == MessageSubTypes.Confirmation && dataWrapper.IsFinalized)
			{
				result = false;
				messageText = Res.GetString("81442089-0bc1-4bc2-9bb5-1b461d401059", "the e-Manifest has already been marked as complete.");
			}
			else if ((messageType == MessageTypes.Codes.UnassociatedShipments
					  || messageType == MessageTypes.Codes.PreliminaryTrip
					  || messageType == MessageTypes.Codes.CrewAndPassenger)
					 && dataWrapper.IsFinalized)
			{
				result = false;
				messageText = Res.GetString("14318f25-b7ca-48c6-838e-613acaa5c67a", @"the e-Manifest has already been
marked as complete and preliminary manifest messages cannot be used. Please submit
Complete e-Manifest to amend the manifest or send a cancellation message if necessary.");
			}
			else if ((messageType == MessageTypes.Codes.eManifest || messageType == MessageTypes.Codes.PreliminaryTrip)
				 && isLodged && !dataWrapper.Equipment.Any() && isEquipmentInfoLodged)
			{
				result = false;
				messageText = Res.GetString("c5637dd0-9aa9-402b-bce1-c1da130f7113", @"previously lodged equipment
cannot be deleted but can only be replaced with another equipment. In order to delete
equipment completely you have to cancel the job and resubmit it again without equipment.");
			}
			else if ((messageType == MessageTypes.Codes.CrewAndPassenger || actionCode == MessageSubTypes.Confirmation)
					 && isNotLodged)
			{
				result = false;
				messageText = Res.GetString("a03abe2b-4832-45fb-84ce-5f93550e574a", "preliminary trip has not been lodged yet.");
			}
			else if (actionCode == MessageSubTypes.Confirmation && isCrewInfoNotLodged)
			{
				result = false;
				messageText = Res.GetString("57eea8c6-f5e0-4201-b53f-c038c2849463", "crew/passengers information has not been lodged yet.");
			}
			else if ((actionCode == MessageSubTypes.ReplaceHeader || actionCode == MessageSubTypes.Withdraw) && isNotLodged)
			{
				result = false;
				messageText = Res.GetString("ef193ed2-d695-484c-be9c-f6e9cf41428f", "original message has not been lodged yet.");
			}
			else if (messageType == MessageTypes.Codes.eManifest && dataWrapper.CrewMembers.Any(crew => crew.CrewId.IsEmpty) && !dataWrapper.IsFinalized)
			{
				result = false;
				messageText = Res.GetString("ad1ded10-b001-4662-9fef-2d1b5d88d6a8", @"some of the crew members are not registered.
If registration is not applicable for a crew member, use preliminary manifest and submit complete Crew/Passengers Details.");
			}
			else if (messageType == MessageTypes.Codes.CrewOrEquipmentRegistration && dataWrapper.CrewMembers.All(x => !x.CrewId.IsEmpty))
			{
				result = false;
				messageText = Res.GetString("28A8E14B-1E79-4074-B2C4-E6C5D3AAA61D", @"all Crew members already have ACE or APC IDs.");
			}
			return (result, messageText);
		}

		public static bool ShowSendWithoutShipmentsIfRequired(ICompleteManifest dataWrapper, Customs.Business.MessageManagers.IUserNotification notification, ZString warningCaption)
		{
			var result = dataWrapper.Shipments.Any(s => s.IsLodged || s.IsSplit) || !dataWrapper.Shipments.Any();
			if (!result)
			{
				result = notification.ShowConfirmation(
					Res.GetString("86e78726-8414-4739-9f48-e747a6a59243", @"The entered shipments are not lodged with Customs and not going to be linked to the trip.
In order to lodge the shipments you have to submit unassociated shipments first.
Are you sure you want to submit the empty trip without shipments?"), warningCaption);
			}
			return result;
		}

		public static bool ShowSendWithoutCrewInfo(Customs.Business.MessageManagers.IUserNotification notification, ZString warningCaption)
		{
			return notification.ShowConfirmation(Res.GetString("1bf7e26d-fd3f-4d9f-99e2-637140f1950f", @"Crew information is not going to be updated because some crew members are not registered.
Only pre-registered crew can be amended using the Complete e-Manifest message.
Unregistered Crew can be submitted or amended in the preliminary manifest.
Are you sure you want to amend all e-Manifest data not including crew details?"), warningCaption);
		}

		public static ZString GetActionCodeDescription(MessageSubTypes actionCodeToSend)
		{
			switch (actionCodeToSend)
			{
				case MessageSubTypes.Change:
					return MessageActionCodes.Descriptions.Change;
				case MessageSubTypes.Withdraw:
					return MessageActionCodes.Descriptions.Cancellation;
				case MessageSubTypes.ReplaceHeader:
					return MessageActionCodes.Descriptions.ChangeHeaderOnly;
				case MessageSubTypes.Confirmation:
					return MessageActionCodes.Descriptions.Confirmation;
				default:
					return MessageActionCodes.Descriptions.Original;
			}
		}

		public static ICompleteManifestMessageBuilder GetMessageBuilder(ICompleteManifest wrapper, ZString messageType, MessageSubTypes actionCode, bool lazyGenerateMessageContent = false)
		{
			switch (messageType)
			{
				case MessageTypes.Codes.CrewOrEquipmentRegistration:
					return new CrewOrEquipmentRegistrationMessageBuilder((ICrewOrEquipmentRegistration)wrapper, actionCode, lazyGenerateMessageContent);
				case MessageTypes.Codes.CrewAndPassenger:
					return new CrewAndPassengersMessageBuilder(wrapper, messageType, actionCode, lazyGenerateMessageContent);
				case MessageTypes.Codes.CompleteTrip:
				case MessageTypes.Codes.PreliminaryTrip:
					return new TripReportMessageBuilder(wrapper, messageType, actionCode, lazyGenerateMessageContent);
				default:
					return new CompleteManifestMessageBuilder(wrapper, messageType, actionCode, lazyGenerateMessageContent);
			}
		}

		public const string MessagePlaceHolder = "<<MESSAGE PLACEHOLDER>>";
	}
}
