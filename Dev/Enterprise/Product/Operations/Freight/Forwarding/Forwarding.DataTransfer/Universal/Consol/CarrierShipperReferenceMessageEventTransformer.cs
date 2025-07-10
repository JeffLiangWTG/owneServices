using System.Collections.Generic;
using System.Linq;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public static class CarrierShipperReferenceMessageEventTransformer
	{
		public static bool IsVersioningEnabledMessage(ITopLevelDataObject dataObject)
		{
			var documentName = dataObject?.DataContext?.DocumentaryOverride?.DocumentName ?? ZString.Empty;

			return documentName == ConsolDocumentNames.BookingRequest
					|| documentName == ConsolDocumentNames.ShippingInstruction
					|| documentName == ConsolDocumentNames.ShippingOrder
					|| documentName == ConsolDocumentNames.VerifiedGrossContainerWeight
					|| documentName == ConsolDocumentNames.BookingConfirmation;
		}

		public static bool IsLowerCarrierShipperReferenceNumberReceived(ITopLevelDataObject dataObject, IStmALogParent logParent)
		{
			var consol = GetConsolFromLogParent(logParent);
			if (dataObject == null || consol == null)
			{
				return false;
			}

			var dataTarget = dataObject.GetMatchingDataTarget(DataContextType.ForwardingConsol);
			var key = dataTarget?.Key ?? ZString.Empty;

			if (!key.IsEmpty)
			{
				var currentCSR = ConsolCarrierShipperReferenceNumberCalculator.GetCarrierShipperReferenceNumber(consol);
				var currentVersion = ConsolCarrierShipperReferenceNumberCalculator.GetVersionFromCarrierShipperReferenceNumber(currentCSR);
				var xmlVersion = ConsolCarrierShipperReferenceNumberCalculator.GetVersionFromCarrierShipperReferenceNumber(key);

				return currentVersion > xmlVersion;
			}

			return false;
		}

		public static EventValue Transform(EventValue sourceEventValue, UniversalEvent universalEvent, IStmALogParent logParent)
		{
			return IsLowerCarrierShipperReferenceNumberReceived(universalEvent, logParent)
				? GetTransformedStatusUpdatedEvent(sourceEventValue, universalEvent)
				: sourceEventValue;
		}

		static ForwardingConsol GetConsolFromLogParent(IStmALogParent logParent)
		{
			if (logParent is ForwardingConsol consol)
			{
				return consol;
			}

			if (logParent is ForwardingContainer container)
			{
				return container.Consol;
			}

			if (logParent is IVisualizerDocumentData visualizerDocumentData)
			{
				return visualizerDocumentData.Parent as ForwardingConsol;
			}

			return null;
		}

		static EventValue GetTransformedStatusUpdatedEvent(EventValue sourceEventValue, UniversalEvent universalEvent)
		{
			var documentName = universalEvent.DataContext?.DocumentaryOverride?.DocumentName ?? ZString.Empty;
			var eventCode = universalEvent.EventType ?? ZString.Empty;
			var isRejectionEvent = MessageEventCodes.RejectionEventCodes.Any(x => x == eventCode);
			var messageType = documentName + " " + (isRejectionEvent ? (NoResString)"Rejection" : (NoResString)"Confirmation"); // Event Parameter Value
			var reason = isRejectionEvent ? (NoResString)"Received for previous 'reset to original' version." : (NoResString)"Received for previous 'reset to original' version. Check with carrier for possible duplication."; // Event Parameter Value

			var eventParameters = new Dictionary<string, string>();
			eventParameters[Constants.EventReferenceParameters.Codes.Type] = (NoResString)"Message Discarded"; // Event Parameter Value
			eventParameters[Constants.EventReferenceParameters.Codes.MessageType] = messageType;
			eventParameters[Constants.EventReferenceParameters.Codes.Reason] = reason;

			return new EventValue(Events.StatusUpdated,
				sourceEventValue.IsEstimate,
				sourceEventValue.DeferFiringWorkflow,
				sourceEventValue.EventTime,
				sourceEventValue.Reference,
				eventParameters);
		}
	}
}
