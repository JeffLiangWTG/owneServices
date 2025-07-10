using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Messaging.MessageBuilders;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.Messaging
{
	public class eManifestMessageManager : EDIFACTMessageManager
	{
		public eManifestMessageManager(IEDIFACTMessageAttachee dataWrapper, string messageType, IUserNotification notification, bool shouldSkipNotification = false)
			: base(dataWrapper, new eManifestStatusCalculator(messageType), notification)
		{
			this.messageType = messageType;
			this.shouldSkipNotification = shouldSkipNotification;
			var provider = DataWrapper.TopLevelBusinessObject as IShipmentActionsProvider;
			if (provider != null)
			{
				provider.ResetShipmentsActionsIfMessageTypeChanged(messageType);
			}
		}

		readonly bool shouldSkipNotification;

		#region Overrides of SingleMessageManager

		protected override bool ShowAwaitingCustomsResponse()
		{
			return shouldSkipNotification || base.ShowAwaitingCustomsResponse();
		}

		public override string MessageFriendlyName
		{
			get { return StatusCalculator.MessageTypeDescription; }
		}

		protected override bool ShouldSendMessagesInTestMode
		{
			get { return shouldSendMessagesInTestMode; }
		}
		public bool shouldSendMessagesInTestMode;

		#endregion

		#region Overrides of MessageManager

		protected override Enterprise.Messaging.Business.EDIMessage[] PopulateMessage(MessageSubTypes actionCode)
		{
			var messages = base.PopulateMessage(actionCode);

			if (messages.Length > 1 && messages.All(x => x.EM_MessageType == MessageTypes.Codes.eManifest && (x.EM_MessageSubType == MessageActionCodes.Codes.Original || x.EM_MessageSubType == MessageActionCodes.Codes.Change)))
			{
				firstMessage = null;
				foreach (var message in messages)
				{
					if (firstMessage != null)
					{
						message.EM_Status = EDIMessage.Status.Pending;
					}
					else
					{
						firstMessage = message;
					}
					message.Saving -= SavingMessage;
					message.Saving += SavingMessage;
					message.Saved -= SavedMessage;
					message.Saved += SavedMessage;
				}
				if (StatusCalculator != null)
				{
					DataWrapper.MessageStatus = StatusCalculator.GetMessageAwaitingStatus(firstMessage);
				}
			}
			return messages;
		}
		Enterprise.Messaging.Business.EDIMessage firstMessage;

		void SavingMessage(Enterprise.Messaging.Business.EDIMessage message)
		{
			if (firstMessage != null)
			{
				message.EM_ApplicationReference = firstMessage.EM_MessageNum;
			}
		}

		void SavedMessage(Enterprise.Messaging.Business.EDIMessage message, bool saveSucceeded)
		{
			if (saveSucceeded)
			{
				message.Saving -= SavingMessage;
				message.Saved -= SavedMessage;
			}
		}

		protected override bool CanSendThisMessage(MessageSubTypes actionCode, out ZString messageText)
		{
			var result = base.CanSendThisMessage(actionCode, out messageText);
			if (result)
			{
				var canSendResult = eManifestMessageManagerHelper.CanSendThisMessage(DataWrapper, messageType, actionCode);
				result = canSendResult.result;
				messageText = canSendResult.messageText;

				if (result)
				{
					if ((messageType == MessageTypes.Codes.PreliminaryTrip && actionCode != MessageSubTypes.Confirmation) && !eManifestMessageManagerHelper.ShowSendWithoutShipmentsIfRequired(DataWrapper, notification, WarningCaption))
					{
						result = false;
					}
					else if (messageType == MessageTypes.Codes.eManifest && DataWrapper.CrewMembers.Any(crew => crew.CrewId.IsEmpty) && DataWrapper.IsFinalized)
					{
						result = eManifestMessageManagerHelper.ShowSendWithoutCrewInfo(notification, WarningCaption);
					}
				}
			}
			return result;
		}

		protected override string GetMessageFriendlyNameForNotSentPopup(MessageSubTypes actionCodeToSend)
		{
			var result = base.GetMessageFriendlyNameForNotSentPopup(actionCodeToSend);
			if (actionCodeToSend != MessageSubTypes.Undefined)
			{
				result = string.Format(CultureInfo.CurrentCulture, "{0} {1}", result, GetActionCodeDescription(actionCodeToSend));
			}
			return result;
		}

		protected override bool DefineActionCodeIfUndefined(ref MessageSubTypes actionCode)
		{
			var result = base.DefineActionCodeIfUndefined(ref actionCode);
			if (!shouldSkipNotification)
			{
				if (messageType == MessageTypes.Codes.CrewAndPassenger)
				{
					actionCode = DataWrapper.IsFinalized ? MessageSubTypes.Change : MessageSubTypes.Create;
				}
				if (ShouldShowAmendmentReasonDialog(actionCode))
				{
					var message = Res.GetString("65aa1629-8963-4c28-9032-849a190e8fb2", "Please specify the reason code for this amendment message.");
					var caption = Res.GetString("5445f0a2-cc07-4796-bc5a-7be011a0e155", "Amendment Reason");
					var reason = notification.ShowQuestion(message, caption, 2, new AmendmentReasonCodes());
					DataWrapper.AmendmentReasonCode = reason;
					result = !string.IsNullOrEmpty(reason);
				}
				if (result && ShouldShowShipmentsActionsDialog(actionCode))
				{
					var provider = DataWrapper.TopLevelBusinessObject as IShipmentActionsProvider;
					if (provider != null)
					{
						result = ((IUserNotification)notification).ShowShipmentsActionsDialog(provider, StatusCalculator.MessageTypeDescription);
					}
				}
				if (result && messageType == MessageTypes.Codes.UnassociatedShipments)
				{
					var shipmentsToBeSent = DataWrapper.Shipments.Where(s => !s.ShipmentActionCode.IsEmpty);
					actionCode = !StatusCalculator.IsLodged(DataWrapper.JobStatus)
								 && shipmentsToBeSent.All(s => s.ShipmentActionCode == MessageActionCodes.Codes.Original)
									? MessageSubTypes.Create : MessageSubTypes.Change;
				}
			}
			return result;
		}

		bool ShouldShowAmendmentReasonDialog(MessageSubTypes actionCode)
		{
			return messageType != MessageTypes.Codes.UnassociatedShipments
				   && (actionCode == MessageSubTypes.Change
					   || actionCode == MessageSubTypes.ReplaceHeader)
				   && DataWrapper.IsFinalized;
		}

		bool ShouldShowShipmentsActionsDialog(MessageSubTypes actionCode)
		{
			return messageType == MessageTypes.Codes.UnassociatedShipments
				   || (actionCode == MessageSubTypes.Change
					   && ((messageType == MessageTypes.Codes.eManifest && DataWrapper.Shipments.Any(s => s.IsLodged))
						   || ((messageType == MessageTypes.Codes.PreliminaryTrip || messageType == MessageTypes.Codes.CompleteTrip)
							   && DataWrapper.Shipments.Any(s => s.IsLodged || s.IsSplit))));
		}

		protected override IMessageBuilder GetMessageBuilder(MessageSubTypes actionCode)
		{
			return eManifestMessageManagerHelper.GetMessageBuilder(DataWrapper, messageType, actionCode);
		}

		protected override string GetActionCodeDescription(MessageSubTypes actionCodeToSend)
		{
			return eManifestMessageManagerHelper.GetActionCodeDescription(actionCodeToSend);
		}

		protected new ICompleteManifest DataWrapper
		{
			get { return (ICompleteManifest)base.DataWrapper; }
		}

		protected new eManifestStatusCalculator StatusCalculator
		{
			get { return (eManifestStatusCalculator)base.StatusCalculator; }
		}

		readonly string messageType;

		protected override void ShowQueuedForSending(MessageSubTypes actionCodeToSend)
		{
			if (!shouldSkipNotification)
			{
				base.ShowQueuedForSending(actionCodeToSend);
			}
		}

		#endregion
	}
}
