using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Business;
using EventConstants = CargoWise.EventReference.Constants;
using EventReferenceParameterTypes = Enterprise.Core.Constants.EventReferenceParameterTypes;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public static class ForwardingConsolEventTransformer
	{
		public static EventValue Transform(EventValue sourceEventValue, UniversalEvent sourceUniversalEvent = null, ForwardingConsol consol = null)
		{
			Argument.NotNull(sourceEventValue, nameof(sourceEventValue));

			if (!NeedTransform(sourceEventValue, sourceUniversalEvent, consol))
			{
				return sourceEventValue;
			}

			var reference = sourceEventValue.Reference;

			IDictionary<string, string> parameters;
			if (sourceEventValue.Parameters.Count == 0)
			{
				parameters = StmALog.GetParametersFromReference(reference);
			}
			else
			{
				parameters = sourceEventValue.Parameters.ToDictionary(s => s.Key, s => s.Value);
			}

			AddTypeParameterIfNecessary(sourceEventValue, sourceUniversalEvent, parameters);

			if (!IncludesEventsForTransformation(sourceEventValue))
			{
				return new EventValue(sourceEventValue.EventType,
					sourceEventValue.IsEstimate,
					sourceEventValue.DeferFiringWorkflow,
					sourceEventValue.EventTime,
					reference,
					parameters);
			}

			AddFromOrToParameterIfNecessary(sourceUniversalEvent, consol, parameters, EventConstants.EventReferenceParameters.Codes.From, "OriginIATAAirportCode");
			AddFromOrToParameterIfNecessary(sourceUniversalEvent, consol, parameters, EventConstants.EventReferenceParameters.Codes.To, "DestinationIATAAirportCode");

			if (sourceEventValue.Code == Events.BookingConfirmedCode && sourceUniversalEvent != null)
			{
				var estimatedArrival = sourceUniversalEvent.ContextCollection
					.FirstOrDefault(c => c.Type != null && c.Type.Type.HasValue && c.Type.Type.Value == nameof(UniversalEvent.ContextTypes.EstimatedTimeOfArrival)
						&& !c.Value.GetValueOrDefault().IsEmpty);
				if (estimatedArrival != null)
				{
					parameters[EventConstants.EventReferenceParameters.Codes.EstimatedTimeOfArrival] = estimatedArrival.Value;
				}
			}

			AddTotalParameterIfNecessary(sourceUniversalEvent, parameters);
			UpdateFlightDateWithTime(sourceEventValue, sourceUniversalEvent, parameters);

			reference = RegenerateReference(reference, parameters);

			return new EventValue(sourceEventValue.EventType,
				sourceEventValue.IsEstimate,
				sourceEventValue.DeferFiringWorkflow,
				sourceEventValue.EventTime,
				reference,
				parameters);
		}

		#region Implementation

		static bool NeedTransform(EventValue sourceEventValue, UniversalEvent sourceUniversalEvent, ForwardingConsol consol)
		{
			if (sourceUniversalEvent?.ContextCollection == null
				|| consol == null
				|| !consol.IsAir)
			{
				return false;
			}

			return IncludesEventsForTransformation(sourceEventValue) ||
				   IncludesEventForAddingTypeParameter(sourceEventValue);
		}

		static bool IncludesEventsForTransformation(EventValue sourceEventValue)
		{
			return (sourceEventValue.Code == Events.BookingConfirmedCode && !sourceEventValue.IsEstimate)
				|| (sourceEventValue.Code == Events.FreightUnloadedCode && !sourceEventValue.IsEstimate)
				|| (sourceEventValue.Code == Events.DepartureCode && !sourceEventValue.IsEstimate)
				|| sourceEventValue.Code == Events.ArrivalCode;
		}

		static bool IncludesEventForAddingTypeParameter(EventValue sourceEventValue)
		{
			var eventsToIncludeTypeParameter = new List<ZString>() { Events.FreightLoadedCode, Events.FreightUnloadedCode, Events.ReceivedCode };
			return eventsToIncludeTypeParameter.Contains(sourceEventValue.Code);
		}

		static ZString RegenerateReference(ZString reference, IDictionary<string, string> parameters)
		{
			var freeText = StmALog.GetFreeTextFromReference(reference);
			return StmALog.GenerateEventReferenceToFitInReferenceMaxLength(freeText, parameters);
		}

		static void AddFromOrToParameterIfNecessary(UniversalEvent sourceUniversalEvent, ForwardingConsol consol, IDictionary<string, string> parameters, string key, string contextType)
		{
			if (!parameters.ContainsKey(key))
			{
				var iataContext = sourceUniversalEvent.ContextCollection
					.FirstOrDefault(c => c.Type == contextType
						&& !c.Value.GetValueOrDefault().IsEmpty);
				if (iataContext != null)
				{
					var unlocoCode = consol.Factory.GetUNLocoFromSuppliedLocationCode(iataContext.Value.GetValueOrDefault());
					if (!unlocoCode.IsEmpty)
					{
						parameters[key] = unlocoCode;
					}
				}
			}
		}

		static void AddTotalParameterIfNecessary(UniversalEvent sourceUniversalEvent, IDictionary<string, string> parameters)
		{
			var key = EventConstants.EventReferenceParameters.Codes.Total;

			if (!parameters.ContainsKey(key))
			{
				var totalPieces = sourceUniversalEvent.ContextCollection
					.FirstOrDefault(c => c.Type == "MAWBNumberOfPieces"
						&& !c.Value.GetValueOrDefault().IsEmpty);
				if (totalPieces != null)
				{
					var totalValue = totalPieces.Value.GetValueOrDefault();
					if (int.TryParse(totalValue, out _))
					{
						parameters[key] = totalValue;
					}
				}
			}
		}

		static void UpdateFlightDateWithTime(EventValue sourceEventValue, UniversalEvent sourceUniversalEvent, IDictionary<string, string> parameters)
		{
			if (sourceEventValue.Code == Events.BookingConfirmedCode)
			{
				var flightDateContext = sourceUniversalEvent.ContextCollection
					.FirstOrDefault(c => c.Type == "FlightDate"
						&& !c.Value.GetValueOrDefault().IsEmpty);
				if (flightDateContext != null
					&& ZDateTime.TryParseISO8601Date(flightDateContext.Value.GetValueOrDefault(), out var flightDateAndTime))
				{
					parameters[EventConstants.EventReferenceParameters.Codes.FlightDate] = flightDateAndTime.ToLongTimeString();
				}
			}
		}

		static void AddTypeParameterIfNecessary(EventValue sourceEventValue, UniversalEvent sourceUniversalEvent, IDictionary<string, string> parameters)
		{
			if (!IncludesEventForAddingTypeParameter(sourceEventValue))
			{
				return;
			}

			var hasPtlTtlEventParameters = parameters != null &&
										   parameters.ContainsKey(EventConstants.EventReferenceParameters.Codes.Partial) &&
										   parameters.ContainsKey(EventConstants.EventReferenceParameters.Codes.Total);

			var contextParamIsPartial = sourceUniversalEvent.ContextCollection?.FirstOrDefault(c => c.Type == "IsPartial" && !c.Value.GetValueOrDefault().IsEmpty);
			var isMarkedPartial = contextParamIsPartial != null && contextParamIsPartial.Value.GetValueOrDefault() == "Y";

			if (hasPtlTtlEventParameters || isMarkedPartial)
			{
				parameters[EventConstants.EventReferenceParameters.Codes.Type] = EventReferenceParameterTypes.Partial;
			}
		}

		#endregion
	}
}
