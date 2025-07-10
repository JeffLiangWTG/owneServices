using CargoWise.Common;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using static CargoWise.EventReference.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public static class SubscriptionRequesteEventTransformer
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal Strings")]
		public static EventValue Transform(EventValue sourceEventValue, UniversalEvent sourceUniversalEvent)
		{
			Argument.NotNull(sourceEventValue, nameof(sourceEventValue));

			var parameters = EventParameters.GetEventParameters(sourceUniversalEvent.EventParameters, sourceUniversalEvent.EventReference);
			parameters[EventReferenceParameters.Codes.Type] = "Shipment Visibility";
			parameters[EventReferenceParameters.Codes.MessageType] = null;

			var contextCollection = sourceUniversalEvent.ContextCollection;
			if (contextCollection != null)
			{
				var referenceContext = sourceUniversalEvent.ContextCollection.Find(contextItem => contextItem.Type.Type.GetValueOrDefault() == "Reference");
				parameters[EventReferenceParameters.Codes.InterchangeNumber] = referenceContext.Value;

				var c1cContext = sourceUniversalEvent.ContextCollection.Find(contextItem => contextItem.Type.Type.GetValueOrDefault() == "CarrierC1CCode");
				parameters[EventReferenceParameters.Codes.Organization] = c1cContext.Value;
			}

			sourceEventValue = new EventValue(sourceEventValue.EventType,
				isEstimate: sourceUniversalEvent.IsEstimate.GetValueOrDefault(),
				eventTime: sourceUniversalEvent.EventTime.GetValueOrDefault(),
				reference: sourceUniversalEvent.EventReference.GetValueOrDefault(),
				parameters: parameters);

			return sourceEventValue;
		}
	}
}
