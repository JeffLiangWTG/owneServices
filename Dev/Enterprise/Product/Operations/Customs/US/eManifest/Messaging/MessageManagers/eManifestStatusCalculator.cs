using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Customs.US.eManifest.Messaging.MessageProcessors;

namespace Enterprise.Customs.US.eManifest.Messaging
{
	public class eManifestStatusCalculator : EDIFACTMessageStatusCalculator
	{
		internal eManifestStatusCalculator(string messageType)
		{
			messageTypeDescription = new MessageTypes().GetDescriptionFromCode(messageType);
		}

		public override ZString MessageTypeDescription
		{
			get { return messageTypeDescription; }
		}

		public void SetJobStatus(IEDIFACTMessageAttachee linkedObject, eManifestOriginalMessageWrapper originalWrapper)
		{
			if (!originalWrapper.IsUnassociatedShipments)
			{
				linkedObject.JobStatus = CalculatedJobStatus(linkedObject);
			}
		}

		public void UpdateReleaseStatus(IEDIFACTMessageAttachee linkedObject, eManifestResponseMessageWrapper responseWrapper, eManifestOriginalMessageWrapper originalWrapper)
		{
			var statusAttachee = linkedObject as IReleaseStatusAttachee;
			if (statusAttachee != null)
			{
				var releaseDate = responseWrapper.ProcessingDate;
				if (releaseDate.IsEmpty)
				{
					releaseDate = ZDateTime.Now;
				}

				if (responseWrapper.IsAcceptedResponse)
				{
					if (originalWrapper.IsCancelation)
					{
						statusAttachee.UpdateReleaseStatusOnAllLinkedShipments(ShipmentEntryStatusList.Codes.Cancelled, releaseDate);
					}
					else
					{
						foreach (var shipment in originalWrapper.Shipments)
						{
							statusAttachee.UpdateReleaseStatus(shipment.ShipmentControlNumber, shipment.RequestedShipmentStatus, releaseDate);
						}
					}
				}
				else if (responseWrapper.IsStatusUpdate)
				{
					foreach (var shipment in responseWrapper.Shipments)
					{
						var releaseStatus = ShipmentEntryStatusList.GetStatusFromNotificationCode(responseWrapper.Factory, shipment.StatusNotificationCode);
						statusAttachee.UpdateReleaseStatus(shipment.ShipmentControlNumber, releaseStatus, shipment.StatusNotificationDate);
					}
				}
				else
				{
					var invalidShipments = responseWrapper.Shipments.ToArray();
					foreach (var invalidShipment in invalidShipments)
					{
						statusAttachee.UpdateReleaseStatus(invalidShipment.ShipmentControlNumber, EntryStatusList.Codes.Error, invalidShipment.StatusNotificationDate);
					}

					if (invalidShipments.Length > 0 && (responseWrapper.IsAcceptedWithErrors || originalWrapper.IsUnassociatedShipments))
					{
						foreach (var acceptedShipment in from sentShipment in originalWrapper.Shipments
														 where !(from invalidShipment in invalidShipments
																 where invalidShipment.ShipmentControlNumber == sentShipment.ShipmentControlNumber
																 select invalidShipment).Any()
														 select sentShipment)
						{
							statusAttachee.UpdateReleaseStatus(acceptedShipment.ShipmentControlNumber, acceptedShipment.RequestedShipmentStatus, releaseDate);
						}
					}
				}
			}
		}

		public override ZString CalculatedJobStatus(IEDIFACTMessageAttachee linkedObject)
		{
			var resultStatus = ZString.Empty;
			var acceptedCompleteReplyFound = false;
			var acceptedPreliminaryReplyFound = false;
			var errorFound = false;
			var syntaxErrorFound = false;

			var messageTypes = new ZString[] { MessageTypes.Codes.eManifest, MessageTypes.Codes.CompleteTrip, MessageTypes.Codes.PreliminaryTrip, MessageTypes.Codes.SyntaxError };
			foreach (EDIMessage inMessage in linkedObject.Messages.GetMatchingMessages(EDIMessage.ApplicationCodes.USeManifest, messageTypes, EDIMessage.Direction.Receive, true, ListSortDirection.Descending))
			{
				var subType = inMessage.EM_MessageSubType;
				switch (inMessage.EM_MessageType)
				{
					case MessageTypes.Codes.eManifest:
					case MessageTypes.Codes.CompleteTrip:
					case MessageTypes.Codes.PreliminaryTrip:
						{
							switch (subType)
							{
								case TripEntryStatusList.Codes.ShipmentsStatusUpdate:
									break;
								case TripEntryStatusList.Codes.AcceptedComplete:
									acceptedCompleteReplyFound = true;
									break;
								case TripEntryStatusList.Codes.AcceptedPreliminary:
									acceptedPreliminaryReplyFound = true;
									break;
								case TripEntryStatusList.Codes.Cancelled:
									resultStatus = TripEntryStatusList.Codes.Cancelled;
									acceptedCompleteReplyFound = true;
									break;
								case TripEntryStatusList.Codes.Error:
									errorFound = true;
									break;
								default:
									if (new TripEntryStatusList().ContainsCode(subType))
									{
										resultStatus = subType;
									}
									break;
							}
						}
						break;
					case MessageTypes.Codes.SyntaxError:
						switch (subType)
						{
							case MessageTypes.Codes.eManifest:
							case MessageTypes.Codes.CompleteTrip:
							case MessageTypes.Codes.PreliminaryTrip:
								syntaxErrorFound = true;
								break;
						}
						break;
				}
				if (!resultStatus.IsEmpty)
				{
					break;
				}
			}

			return !resultStatus.IsEmpty ? resultStatus
					: acceptedCompleteReplyFound ? new ZString(TripEntryStatusList.Codes.AcceptedComplete)
						: acceptedPreliminaryReplyFound ? new ZString(TripEntryStatusList.Codes.AcceptedPreliminary)
							: errorFound ? new ZString(EntryStatusList.Codes.Error)
								: syntaxErrorFound ? new ZString(MessageTypes.Codes.SyntaxError)
									: linkedObject.JobStatus;
		}

		internal ZString GetStatusUpdateMessageSubType(eManifestResponseMessageWrapper responseWrapper)
		{
			var result = ZString.Empty;
			if (!responseWrapper.NotificationCode.IsEmpty)
			{
				//TODO: Check if there will be multiple status notifications, if so check with Brendon what to do.
				result = TripEntryStatusList.GetStatusFromNotificationCode(responseWrapper.NotificationCode);
			}
			else if (responseWrapper.Shipments.Any())
			{
				var firstStatus = responseWrapper.Shipments.First().Notifications.FirstOrDefault();
				result = firstStatus != null && responseWrapper.Shipments.All(s => s.Notifications.Any(n => n.Code == firstStatus.Code))
							? ShipmentEntryStatusList.GetStatusFromNotificationCode(responseWrapper.Factory, firstStatus.Code) : TripEntryStatusList.Codes.ShipmentsStatusUpdate;
			}
			return result;
		}

		internal ZString GetAcceptedMessageSubType(eManifestOriginalMessageWrapper originalWrapper)
		{
			return originalWrapper.IsCancelation ? EntryStatusList.Codes.Cancelled
					: !originalWrapper.IsPreliminary || originalWrapper.IsConfirmation ? TripEntryStatusList.Codes.AcceptedComplete
						: TripEntryStatusList.Codes.AcceptedPreliminary;
		}

		internal ZString GetErrorMessageSubType(eManifestResponseMessageWrapper responseWrapper)
		{
			return responseWrapper.IsAcceptedWithErrors ? TripEntryStatusList.Codes.AcceptedPreliminary : TripEntryStatusList.Codes.Error;
		}

		public override ZString GetMessageAwaitingStatus(ZString messageSubType)
		{
			return messageSubType == MessageActionCodes.Codes.Confirmation
					? (ZString)MessageStatusList.Codes.AwaitingChange : base.GetMessageAwaitingStatus(messageSubType);
		}

		public override bool IsLodged(ZString currentJobStatus)
		{
			return base.IsLodged(currentJobStatus)
				   && currentJobStatus != EntryStatusList.Codes.Error
				   && currentJobStatus != MessageTypes.Codes.SyntaxError;
		}

		internal bool IsCrewInfoLodged(IEDIFACTMessageAttachee linkedObject)
		{
			var messages = GetMessagesOrderedDescending(linkedObject);
			var acceptedMessage = messages.SkipWhile(m => !((m.EM_MessageType == MessageTypes.Codes.CrewAndPassenger
															 && m.EM_MessageSubType == TripEntryStatusList.Codes.AcceptedPreliminary)
															|| m.EM_MessageSubType == TripEntryStatusList.Codes.Cancelled)).FirstOrDefault();

			return acceptedMessage != null && acceptedMessage.EM_MessageSubType == TripEntryStatusList.Codes.AcceptedPreliminary;
		}

		internal bool IsEquipmentInfoLodged(IEDIFACTMessageAttachee linkedObject)
		{
			var messages = GetMessagesOrderedDescending(linkedObject);
			var acceptedMessage = messages.SkipWhile(m => !(m.EM_MessageSubType == TripEntryStatusList.Codes.Cancelled
															|| ((m.EM_MessageType == MessageTypes.Codes.eManifest
																 && m.EM_MessageSubType == TripEntryStatusList.Codes.AcceptedComplete)
																|| (m.EM_MessageType == MessageTypes.Codes.PreliminaryTrip
																	&& m.EM_MessageSubType == TripEntryStatusList.Codes.AcceptedPreliminary)))).FirstOrDefault();

			return acceptedMessage != null && acceptedMessage.EM_MessageSubType != TripEntryStatusList.Codes.Cancelled
				   && new eManifestOriginalMessageWrapper(acceptedMessage.OriginalMessage).ContainsEquipmentInfo;
		}

		static IEnumerable<EDIMessage> GetMessagesOrderedDescending(IEDIFACTMessageAttachee linkedObject)
		{
			return (from EDIMessage message in linkedObject.Messages
					orderby ZInt.ParseEmptyAsZero(message.EM_MessageNum) descending
					select message);
		}

		readonly string messageTypeDescription;
	}
}
