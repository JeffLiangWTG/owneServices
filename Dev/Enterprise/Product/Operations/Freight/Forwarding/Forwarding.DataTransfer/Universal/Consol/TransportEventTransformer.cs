using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Freight.Universal;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public static class TransportEventTransformer
	{
		public static EventValue Transform(EventValue sourceEventValue, UniversalEvent sourceUniversalEvent, Transport transport)
		{
			Argument.NotNull(sourceEventValue, nameof(sourceEventValue));
			Argument.NotNull(transport, nameof(transport));

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

			if (!sourceEventValue.IsEstimate &&
					(sourceEventValue.Code == Events.StatusUpdatedCode
					|| sourceEventValue.Code == Events.DepartureCode
					|| sourceEventValue.Code == Events.ArrivalCode))
			{
				UpdateOnlineScheduleStatus(sourceEventValue, transport);
			}

			if (sourceEventValue.Code == Events.BookingConfirmedCode && sourceUniversalEvent != null)
			{
				var estimatedArrival = sourceUniversalEvent.ContextCollection
					.FirstOrDefault(c => c.Type != null && c.Type.Type.HasValue && c.Type.Type.Value == nameof(UniversalEvent.ContextTypes.EstimatedTimeOfArrival)
						&& !c.Value.GetValueOrDefault().IsEmpty);
				if (estimatedArrival != null)
				{
					parameters[Constants.EventReferenceParameters.Codes.EstimatedTimeOfArrival] = estimatedArrival.Value;
					reference = RegenerateReference(reference, parameters);
				}
			}

			UpdateVesselEventDates(sourceEventValue, transport);

			if (!NeedTransform(sourceEventValue, sourceEventValue.IsEstimate, transport))
			{
				return new EventValue(sourceEventValue.EventType,
					sourceEventValue.IsEstimate,
					sourceEventValue.DeferFiringWorkflow,
					sourceEventValue.EventTime,
					reference,
					parameters);
			}

			var location = sourceEventValue.Code == Events.DepartureCode
				? transport.JW_RL_NKLoadPort
				: transport.JW_RL_NKDiscPort;

			return EventTransformerHelper.DepartureOrArrivalToStatusUpdated(sourceEventValue, sourceEventValue.Reference, location);
		}

		static ZString RegenerateReference(ZString reference, IDictionary<string, string> parameters)
		{
			var freeText = StmALog.GetFreeTextFromReference(reference);
			return StmALog.GenerateEventReferenceToFitInReferenceMaxLength(freeText, parameters);
		}

		static bool NeedTransform(EventValue sourceEventValue, ZBool isEstimate, Transport transport)
		{
			if (isEstimate && (sourceEventValue.Code == Events.DepartureCode || sourceEventValue.Code == Events.ArrivalCode))
			{
				return sourceEventValue.Code == Events.DepartureCode
					? !transport.JW_ATD.IsEmpty
					: !transport.JW_ATA.IsEmpty;
			}

			return false;
		}

		#region UpdateOnlineScheduleStatus

		public static void UpdateOnlineScheduleStatus(EventValue sourceEventValue, Transport transport)
		{
			var parameters = sourceEventValue.Parameters;
			if (parameters == null || !parameters.Any())
			{
				parameters = StmALog.GetParametersFromReference(sourceEventValue.Reference);
			}

			if (transport.IsAir
				&& parameters.TryGetValue(Constants.EventReferenceParameters.Codes.Location, out var location)
				&& parameters.TryGetValue(Constants.EventReferenceParameters.Codes.VoyageFlightNumber, out var voyageFlightNumber)
				&& !string.IsNullOrEmpty(location)
				&& (transport.IsVoyageFlightMatched(voyageFlightNumber)
					|| transport.IsVoyageFlightFuzzyMatched(voyageFlightNumber)))
			{
				bool compareETA = sourceEventValue.Code == Events.ArrivalCode;

				if ((sourceEventValue.Code == Events.ArrivalCode || sourceEventValue.Code == Events.DepartureCode)
					&& parameters.TryGetValue(Constants.EventReferenceParameters.Codes.Facility, out var facility)
					&& facility == Constants.Facilities.Code.Terminal
					&& parameters.TryGetValue(Constants.EventReferenceParameters.Codes.FlightDate, out var flightDate)
					&& ZDateTime.TryParseISO8601Date(flightDate, out var flightDateTime)
					&& transport.IsFlightDateMatched(compareETA, flightDateTime.Date))
				{
					transport.JW_OnlineScheduleStatus = sourceEventValue.Code == Events.ArrivalCode
						? Core.Constants.FlightScheduleStatus.Arrived
						: Core.Constants.FlightScheduleStatus.Departed;
				}
				else if (sourceEventValue.Code == Events.StatusUpdatedCode
						 && parameters.TryGetValue(Constants.EventReferenceParameters.Codes.MessageType, out var messageType)
						 && parameters.TryGetValue(Constants.EventReferenceParameters.Codes.Type, out var type)
						 && !string.IsNullOrEmpty(messageType)
						 && !string.IsNullOrEmpty(type))
				{
					var statusUpdatedTypes = new Dictionary<string, string>()
					{
						{ StatusUpdatedTypeMessages.ActiveFlightAdvice, Core.Constants.FlightScheduleStatus.Active },
						{ StatusUpdatedTypeMessages.FlightCancelationAlert, Core.Constants.FlightScheduleStatus.Cancelled },
						{ StatusUpdatedTypeMessages.FlightCancellationAlert, Core.Constants.FlightScheduleStatus.Cancelled },
						{ StatusUpdatedTypeMessages.PreDepartureAdvice, Core.Constants.FlightScheduleStatus.PreDeparture },
						{ StatusUpdatedTypeMessages.DepartureDelayAlert, Core.Constants.FlightScheduleStatus.DepartureDelay },
						{ StatusUpdatedTypeMessages.BookedFlightDeparted, Core.Constants.FlightScheduleStatus.Departed },
						{ StatusUpdatedTypeMessages.FlightDiversionAlert, Core.Constants.FlightScheduleStatus.Diversion },
						{ StatusUpdatedTypeMessages.ArrivalDelayAlert, Core.Constants.FlightScheduleStatus.ArrivalDelay },
						{ StatusUpdatedTypeMessages.PreArrivalAdvice, Core.Constants.FlightScheduleStatus.PreArrival },
						{ StatusUpdatedTypeMessages.BookedFlightArrived, Core.Constants.FlightScheduleStatus.Arrived }
					};

					if (statusUpdatedTypes.ContainsKey(type)
						&& (type == StatusUpdatedTypeMessages.ArrivalDelayAlert
							|| type == StatusUpdatedTypeMessages.PreArrivalAdvice
							|| parameters.TryGetValue(Constants.EventReferenceParameters.Codes.Facility, out var facilityStatusUpdated)
							&& facilityStatusUpdated == Constants.Facilities.Code.Terminal
							&& parameters.TryGetValue(Constants.EventReferenceParameters.Codes.FlightDate, out var flightDateStatusUpdated)
							&& ZDateTime.TryParseISO8601Date(flightDateStatusUpdated, out var flightDateTimeStatusUpdated)
							&& transport.IsFlightDateMatched(compareETA, flightDateTimeStatusUpdated.Date)))
					{
						transport.JW_OnlineScheduleStatus = statusUpdatedTypes[type];
					}
				}
			}
		}

		#region SuppressResourceStringsCheckRegion

		static class StatusUpdatedTypeMessages
		{
			public const string ActiveFlightAdvice = "Active Flight Advice";
			public const string FlightCancelationAlert = "Flight Cancelation Alert";
			public const string FlightCancellationAlert = "Flight Cancellation Alert";
			public const string PreDepartureAdvice = "Pre-Departure Advice";
			public const string DepartureDelayAlert = "Departure Delay Alert";
			public const string BookedFlightDeparted = "Booked Flight Departed";
			public const string FlightDiversionAlert = "Flight Diversion Alert";
			public const string ArrivalDelayAlert = "Arrival Delay Alert";
			public const string PreArrivalAdvice = "Pre-Arrival Advice";
			public const string BookedFlightArrived = "Booked Flight Arrived";
		}

		#endregion

		#endregion

		#region UpdateVesselEventDates

		public static void UpdateVesselEventDates(EventValue sourceEventValue, Transport transport)
		{
			if (!EventTransformerHelper.IsVesselEvent(sourceEventValue.Code))
			{
				return;
			}

			var parameters = sourceEventValue.Parameters;
			if (parameters == null || !parameters.Any())
			{
				parameters = StmALog.GetParametersFromReference(sourceEventValue.Reference);
			}

			parameters.TryGetValue(Constants.EventReferenceParameters.Codes.Location, out var location);
			parameters.TryGetValue(Constants.EventReferenceParameters.Codes.Facility, out var facility);
			parameters.TryGetValue(Constants.EventReferenceParameters.Codes.Type, out var eventType);

			if (string.IsNullOrEmpty(location) || string.IsNullOrEmpty(facility) || !DoesEventLocationMatchTransportPort(sourceEventValue.Code, location, transport))
			{
				return;
			}

			var eventDate = sourceEventValue.EventTime.ToZDateTime();
			if (facility == Constants.Facilities.Code.Terminal)
			{
				switch (sourceEventValue.Code)
				{
					case Events.CargoAvailableCode:
						transport.JW_TerminalAvailabilityDate = eventDate;
						break;
					case Events.CutOffDateCode:
						if (OneStopEventHandler.IsEventTypeAllowedForUpdatingDates(eventType))
						{
							transport.JW_TerminalCutOff = eventDate;
						}
						break;
					case Events.ReceiptCommencedCode:
						if (OneStopEventHandler.IsEventTypeAllowedForUpdatingDates(eventType))
						{
							transport.JW_TerminalReceivalCommences = eventDate;
						}
						break;
					case Events.StorageCommencedCode:
						transport.JW_TerminalStorageDate = eventDate;
						break;
				}
			}
			else if (facility == Constants.Facilities.Code.Depot)
			{
				switch (sourceEventValue.Code)
				{
					case Events.CargoAvailableCode:
						transport.JW_DepotAvailabilityDate = eventDate;
						break;
					case Events.CutOffDateCode:
						transport.JW_DepotCutOff = eventDate;
						break;
					case Events.ReceiptCommencedCode:
						transport.JW_DepotReceivalCommences = eventDate;
						break;
					case Events.StorageCommencedCode:
						transport.JW_DepotStorageDate = eventDate;
						break;
				}
			}
		}

		static ZBool DoesEventLocationMatchTransportPort(string eventType, string location, Transport transport)
			=> eventType == Events.CargoAvailableCode || eventType == Events.StorageCommencedCode
				? location == transport.JW_RL_NKDiscPort
				: location == transport.JW_RL_NKLoadPort;

		#endregion
	}
}
