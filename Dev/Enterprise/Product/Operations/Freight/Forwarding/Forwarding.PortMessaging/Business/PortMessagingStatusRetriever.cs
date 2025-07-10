using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business
{
	public class PortMessagingStatusRetriever : NonPersistentBusinessObject, IObsoleteValidation
	{
		public PortMessagingStatusRetriever(BusinessObject parent, bool validateRelatedObjects = true)
			: base(parent.Factory)
		{
			this.Parent = parent;
			this.validateRelatedObjects = validateRelatedObjects;
		}

		readonly BusinessObject Parent;
		readonly bool validateRelatedObjects;

		#region Message Sent and Received Status

		public bool IsWaitingForReply(PortMessagingManager.MessageType messageType)
		{
			var latestEvent = FindMostRecentEvent(PortMessagingSentEvents.Concat(PortMessagingReceivedEvents));
			if (latestEvent != null && latestEvent.SL_SE_NKEvent == Events.InterchangeRejectedCode)
			{
				return false;
			}

			var latestDakosyEvent = FindMostRecentEvent(PortMessagingSentEvents.Concat(PortMessagingReceivedEvents), messageType);
			return latestDakosyEvent != null && new List<string>() { Events.MessageSentCode, Events.MessageWithdrawCancelRequestCode }.Contains(latestDakosyEvent.SL_SE_NKEvent)
				|| IsRelatedMessagesFlagged(x => x.IsWaitingForReply(messageType));
		}

		public bool IsMessageStillPending(PortMessagingManager.MessageType messageType)
		{
			var latestEvent = FindMostRecentEvent(PortMessagingSentEvents.Concat(PortMessagingReceivedEvents), messageType);
			return latestEvent != null && new List<string>() { Events.MessagePendingProcessingCode, Events.InterchangeReceiptAcknowledgedCode }.Contains(latestEvent.SL_SE_NKEvent)
				|| IsRelatedMessagesFlagged(x => x.IsMessageStillPending(messageType));
		}

		public bool IsMessageCancellationStillPending(PortMessagingManager.MessageType messageType)
		{
			var latestEventPair = FindMostRecentSentAndReceivedEventPair(messageType);
			return latestEventPair.SentEvent != null && latestEventPair.SentEvent.SL_SE_NKEvent == Events.MessageWithdrawCancelRequestCode
				&& (latestEventPair.ReceivedEvent != null && latestEventPair.ReceivedEvent.SL_SE_NKEvent == Events.MessagePendingProcessingCode)
				|| IsRelatedMessagesFlagged(x => x.IsMessageCancellationStillPending(messageType));
		}

		public bool HasMessageToCancel(PortMessagingManager.MessageType messageType)
		{
			var latestEventPair = FindMostRecentSentAndReceivedEventPair(messageType);
			return latestEventPair.SentEvent != null && latestEventPair.ReceivedEvent != null
				&& (latestEventPair.SentEvent.SL_SE_NKEvent == Events.MessageSentCode && latestEventPair.ReceivedEvent.SL_SE_NKEvent == Events.MessageAcceptedCode
				|| latestEventPair.SentEvent.SL_SE_NKEvent == Events.MessageWithdrawCancelRequestCode && new List<string> { Events.InterchangeRejectedCode, Events.MessageRejectedCode }.Contains(latestEventPair.ReceivedEvent.SL_SE_NKEvent));
		}

		bool IsRelatedMessagesFlagged(Func<PortMessagingStatusRetriever, bool> isFlagged)
		{
			if (!validateRelatedObjects)
			{
				return false;
			}

			if (Parent is CommonConsol pConsol)
			{
				return pConsol.Shipments.Any(shipment => isFlagged(new PortMessagingStatusRetriever(shipment, false)));
			}

			if (Parent is CommonShipment pShipment)
			{
				return pShipment.Consols.Any(consol => isFlagged(new PortMessagingStatusRetriever(consol, false)));
			}

			return false;
		}

		class EventPair
		{
			public EventPair(StmALog sentEvent, StmALog receivedEvent)
			{
				SentEvent = sentEvent;
				ReceivedEvent = receivedEvent;
			}

			public StmALog SentEvent { get; set; }
			public StmALog ReceivedEvent { get; set; }
		}

		#endregion

		#region Message Sent Status

		[ResourceStringData("PortMessagingStatus|SentStatus", Caption = "Sent Status")]
		public ZString SentStatus
		{
			get
			{
				var latestEvent = FindMostRecentSentEvent();
				if (latestEvent != null)
				{
					return latestEvent.DisplayEventReference;
				}

				return ResString.GetMultilingualString("0d8302dd-e57b-4784-a44d-bd122685d53d", "Not Sent");
			}
		}

		public ZPropertyInfo SentStatusInfo
		{
			get { return GetZPropertyInfo(nameof(SentStatus)); }
		}

		[ResourceStringData("PortMessagingStatus|SentBy", Caption = "Sent By")]
		public ZString SentBy
		{
			get
			{
				var latestEvent = FindMostRecentSentEvent();
				if (latestEvent != null)
				{
					return latestEvent.SL_UserNameAndInitials;
				}

				return ZString.Empty;
			}
		}

		public ZPropertyInfo SentByInfo
		{
			get { return GetZPropertyInfo(nameof(SentBy)); }
		}

		[ResourceStringData("PortMessagingStatus|SentDate", Caption = "Date Sent")]
		public ZDateTime SentDate
		{
			get
			{
				var latestEvent = FindMostRecentSentEvent();
				if (latestEvent != null)
				{
					return latestEvent.SL_EventTime;
				}

				return ZDateTime.Empty;
			}
		}

		public ZPropertyInfo SentDateInfo
		{
			get { return GetZPropertyInfo(nameof(SentDate)); }
		}

		#endregion

		#region Message Received Status

		[ResourceStringData("PortMessagingStatus|MessageStatus", Caption = "Message Status")]
		public ZString MessageStatus
		{
			get
			{
				var latestEvent = FindMostRecentReceivedEvent();
				if (latestEvent != null && latestEvent.Event != null)
				{
					return latestEvent.Event.SE_DescMultilingual;
				}

				return ZString.Empty;
			}
		}

		public ZPropertyInfo MessageStatusInfo
		{
			get { return GetZPropertyInfo(nameof(MessageStatus)); }
		}

		[ResourceStringData("PortMessagingStatus|MessageStatusDescription", Caption = "Description")]
		public ZString MessageStatusDescription
		{
			get
			{
				var latestEvent = FindMostRecentReceivedEvent();
				if (latestEvent != null)
				{
					return latestEvent.DisplayEventReference;
				}

				return ZString.Empty;
			}
		}

		public ZPropertyInfo MessageStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(MessageStatusDescription)); }
		}

		[ResourceStringData("PortMessagingStatus|MessageStatusDate", Caption = "Date Received")]
		public ZDateTime MessageStatusDate
		{
			get
			{
				var latestEvent = FindMostRecentReceivedEvent();
				if (latestEvent != null)
				{
					return latestEvent.SL_EventTime;
				}

				return ZDateTime.Empty;
			}
		}

		public ZPropertyInfo MessageStatusDateInfo
		{
			get { return GetZPropertyInfo(nameof(MessageStatusDate)); }
		}

		#endregion

		#region Events

		public static IEnumerable<Event> PortMessagingSentEvents
		{
			get
			{
				yield return Events.MessageSent;
				yield return Events.MessageWithdrawCancelRequest;
			}
		}

		public static IEnumerable<Event> PortMessagingReceivedEvents
		{
			get
			{
				yield return Events.MessageAccepted;
				yield return Events.InterchangeReceiptAcknowledged;
				yield return Events.MessageWithdrawCancelAccepted;
				yield return Events.MessagePendingProcessing;
				yield return Events.MessageRejected;
				yield return Events.InterchangeRejected;
			}
		}

		#endregion

		#region Implementation

		StmALog FindMostRecentSentEvent(PortMessagingManager.MessageType? messageType = null)
		{
			return FindMostRecentEvent(PortMessagingSentEvents, messageType);
		}

		StmALog FindMostRecentReceivedEvent(PortMessagingManager.MessageType? messageType = null)
		{
			return FindMostRecentEvent(PortMessagingReceivedEvents, messageType);
		}

		StmALog FindMostRecentEvent(IEnumerable<Event> portMessagingEvents, PortMessagingManager.MessageType? messageType = null)
		{
			return portMessagingEvents
				.Select(portMessagingEvent => GetMostRecentRelatedEvent(portMessagingEvent, messageType))
				.Where(log => log != null)
				.OrderByDescending(recentlog => recentlog.SL_EventTime)
				.FirstOrDefault();
		}

		bool IsMessageTypeMatches(StmALog log, PortMessagingManager.MessageType? messageType)
		{
			if (messageType == null || !StmALog.GetParametersFromReference(log.SL_Reference, StmALog.ParseReferenceError.None).TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, out string value))
			{
				return false;
			}

			var logReferenceMessageType = GetLogReferenceFromMessageType(messageType);
			if (string.IsNullOrEmpty(logReferenceMessageType) || string.IsNullOrEmpty(value) || !value.Contains(logReferenceMessageType))
			{
				return false;
			}

			return true;
		}

		EventPair FindMostRecentSentAndReceivedEventPair(PortMessagingManager.MessageType? messageType)
		{
			var sentEvent = FindMostRecentEvent(PortMessagingSentEvents, messageType);
			var receivedEvent = FindMostRecentEvent(PortMessagingReceivedEvents, messageType);
			var isWaitingReceipt = (sentEvent?.SL_EventTime ?? ZDateTime.MinSmallDateTimeValue) > (receivedEvent?.SL_EventTime ?? ZDateTime.MinSmallDateTimeValue);
			return new EventPair(sentEvent, isWaitingReceipt ? null : receivedEvent);
		}

		StmALog GetMostRecentRelatedEvent(Event portMessagingEvent, PortMessagingManager.MessageType? messageType)
		{
			Func<StmALog, bool> isDakosyLog = x => x.Parameters.ContainsKey(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department)
				&& x.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department].Equals((NoResString)"Dakosy", StringComparison.OrdinalIgnoreCase); // Checking event reference from non-translatable strings

			Func<Event, StmALog, bool> isEhubDakosyLog = (e, x) => x.Parameters.ContainsKey(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department)
				&& x.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department].Equals("eHub", StringComparison.OrdinalIgnoreCase) // Checking event reference from non-translatable strings
				&& x.Parameters.ContainsKey(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason)
				&& x.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason].Contains((NoResString)"Dakosy", StringComparison.OrdinalIgnoreCase) // Checking event reason from non-translatable strings
				&& e.Code == Events.InterchangeRejectedCode;

			Func<StmALog, bool> isLogIncluded = x => (messageType == null || IsMessageTypeMatches(x, messageType)) && (isDakosyLog(x) || isEhubDakosyLog(portMessagingEvent, x)) && !PropagationHandler.IsPropagatedEventLog(x);

			return Parent.GetLogs().MostRecentLogByEventTime(portMessagingEvent, isLogIncluded);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Logging")]
		public static string GetLogReferenceFromMessageType(PortMessagingManager.MessageType? messageType)
		{
			var result = "";

			switch (messageType)
			{
				case PortMessagingManager.MessageType.PortOrderWithHDS:
				case PortMessagingManager.MessageType.PortOrderWithHDSCancellationBecauseOfErrors:
				case PortMessagingManager.MessageType.PortOrderWithHDSCancellationOnExit:
				case PortMessagingManager.MessageType.PortOrderWithHDSForwardingCancellation:
					result = "Port Order with HDS";
					break;

				case PortMessagingManager.MessageType.PortOrderInbound:
					result = "Port Order for Inbound Delivery";
					break;

				case PortMessagingManager.MessageType.PortOrderOutbound:
					result = "Port Order for Outbound Delivery";
					break;

				case PortMessagingManager.MessageType.GatePass:
					result = "Gate Pass";
					break;

				case PortMessagingManager.MessageType.RequestForPortServices:
					result = "Request for Port Services";
					break;
				case PortMessagingManager.MessageType.CertificateOfObligation:
					result = "Certificate of Obligation";
					break;
				case PortMessagingManager.MessageType.RequestForRailDischarge:
					result = "Request for Rail Discharge";
					break;

				case PortMessagingManager.MessageType.StopRequest:
					result = "Stop Request";
					break;
			}

			return result;
		}

		#endregion
	}
}
