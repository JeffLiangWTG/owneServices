using System;
using System.Collections.Generic;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.EntityFramework;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;

namespace Enterprise.Freight.Business
{
	public static class FreightEventsHelper
	{
		public static void CascadeIfApplicable(EnterpriseBusinessObject logsParent, string freightEventCode, IEnumerable<BusinessObject> targets)
		{
			if (logsParent is null)
			{
				throw new ArgumentNullException(nameof(logsParent));
			}
			if (targets is null)
			{
				throw new ArgumentNullException(nameof(targets));
			}

			var unsavedFreightEventLogs = FindUnsavedEvents(logsParent, freightEventCode);
			foreach (var freightEventLog in unsavedFreightEventLogs)
			{
				CascadeEventLogIfApplicable(freightEventLog, targets);
			}
		}

		static void CascadeEventLogIfApplicable(StmALog eventLog, IEnumerable<BusinessObject> targets)
		{
			foreach (var target in targets.Where(target => target != null))
			{
				if (eventLog.Parameters.ContainsKey(Constants.EventReferenceParameters.Codes.Partial))
				{
					continue;
				}

				var canUpdate = true;

				var eventDatePropertyChecker = target as IEventDatePropertyChecker;
				if (eventDatePropertyChecker != null)
				{
					canUpdate = eventDatePropertyChecker.CanUpdateProperty(eventLog);
				}

				if (canUpdate)
				{
					target.GetLogs().AddNew(Events.All[eventLog.SL_SE_NKEvent], eventLog.SL_Reference, eventLog.SL_EventTimeOffset, eventLog.SL_IsEstimate);
				}
			}
		}

		static IEnumerable<StmALog> FindUnsavedEvents(EnterpriseBusinessObject logsParent, string eventCode)
		{
			var freshEvents = logsParent.Logs.LogsNotInDB
				.Where(log =>
					log.SL_SE_NKEvent == eventCode
					&& !log.SL_IsCancelled
					&& !PropagationHandler.IsPropagatedEventLog(log))
				.DistinctBy(log => log.SL_Reference);

			return freshEvents;
		}

		public static void PropagateArivalDepartureEventToMatchedTransportLeg(IStmALog log, TransportCollection transports)
		{
			if (log is null)
			{
				throw new ArgumentNullException(nameof(log));
			}
			if (transports is null)
			{
				throw new ArgumentNullException(nameof(transports));
			}

			if (log.Parameters.ContainsKey(Constants.EventReferenceParameters.Codes.Facility)
					&& log.Parameters.ContainsKey(Constants.EventReferenceParameters.Codes.Location)
					&& log.Parameters[Constants.EventReferenceParameters.Codes.Facility] == Constants.Facilities.Code.Terminal
					&& transports.Any())
			{
				var eventCode = log.SL_SE_NKEvent;
				var estimateOrActual = log.SL_IsEstimate ? EstimateActual.Estimate : EstimateActual.Actual;
				var logTimeStamp = log.SL_EventTime.ToSmallDateTime();

				IEnumerable<Transport> matchedTransportlLegs = null;
				if (eventCode == Events.ArrivalCode || eventCode == Events.DepartureCode)
				{
					var transportList = transports.Cast<Transport>();
					matchedTransportlLegs = GetTransportLegsForMatching(log, transportList);
				}

				if (matchedTransportlLegs != null && matchedTransportlLegs.Any())
				{
					if (eventCode == Events.ArrivalCode)
					{
						var arrivalTransport = matchedTransportlLegs.FirstOrDefault(x => x.JW_RL_NKDiscPort == log.Parameters[Constants.EventReferenceParameters.Codes.Location]);

						if (arrivalTransport != null)
						{
							bool canUpdateLeg = estimateOrActual == EstimateActual.Estimate ? arrivalTransport.JW_ETA != logTimeStamp : arrivalTransport.JW_ATA != logTimeStamp;

							if (canUpdateLeg)
							{
								arrivalTransport.Logs.CreateOrRecreateEventLog(Events.Arrival, estimateOrActual, log.SL_EventTimeOffset, ZString.Empty, log.Parameters.ToArray());
							}
						}
					}
					else if (eventCode == Events.DepartureCode)
					{
						var departureTransport = matchedTransportlLegs.FirstOrDefault(x => x.JW_RL_NKLoadPort == log.Parameters[Constants.EventReferenceParameters.Codes.Location]);

						if (departureTransport != null)
						{
							bool canUpdateLeg = estimateOrActual == EstimateActual.Estimate ? departureTransport.JW_ETD != logTimeStamp : departureTransport.JW_ATD != logTimeStamp;

							if (canUpdateLeg)
							{
								departureTransport.Logs.CreateOrRecreateEventLog(Events.Departure, estimateOrActual, log.SL_EventTimeOffset, ZString.Empty, log.Parameters.ToArray());
							}
						}
					}
				}
			}
		}

		public static ZString GetLoadPort(IContainerParent containerParent)
		{
			var result = ZString.Empty;

			if (containerParent != null)
			{
				var firstLeg = new TransportOrderHelper(containerParent.Transports).FirstLeg;
				if (firstLeg != null)
				{
					result = firstLeg.JW_RL_NKLoadPort;
				}

				if (string.IsNullOrEmpty(result))
				{
					result = containerParent.LoadPort?.RL_Code ?? ZString.Empty;
				}
			}

			return result;
		}

		public static ZString GetLoadTransportMode(IContainerParent containerParent)
		{
			var result = ZString.Empty;

			if (containerParent != null)
			{
				var firstLeg = new TransportOrderHelper(containerParent.Transports).FirstLeg;
				if (firstLeg != null)
				{
					result = firstLeg.JW_TransportMode;
				}

				if (string.IsNullOrEmpty(result))
				{
					result = containerParent.TransportMode;
				}
			}

			return result;
		}

		public static ZString GetDischargePort(IContainerParent containerParent)
		{
			var result = ZString.Empty;

			if (containerParent != null)
			{
				var lastLeg = new TransportOrderHelper(containerParent.Transports).LastLeg;
				if (lastLeg != null)
				{
					result = lastLeg.JW_RL_NKDiscPort;
				}

				if (string.IsNullOrEmpty(result))
				{
					result = containerParent.DischargePort?.RL_Code ?? ZString.Empty;
				}
			}

			return result;
		}

		public static ZString GetDischargeTransportMode(IContainerParent containerParent)
		{
			var result = ZString.Empty;

			if (containerParent != null)
			{
				var lastLeg = new TransportOrderHelper(containerParent.Transports).LastLeg;
				if (lastLeg != null)
				{
					result = lastLeg.JW_TransportMode;
				}

				if (string.IsNullOrEmpty(result))
				{
					result = containerParent.TransportMode;
				}
			}

			return result;
		}

		#region GetTransportLegsForMatching

		public static IEnumerable<Transport> GetTransportLegsForMatching(IXmlEventValueObject xmlEvent, IEnumerable<Transport> transports)
		{
			var eventCode = xmlEvent.EventType;
			var fligthDate = xmlEvent.Context.FlightDate.Date;
			var flightNumber = xmlEvent.Context.FlightNumber.GetValueOrDefault();

			var universalEvent = xmlEvent as UniversalDataBuss.DataObjects.Universal.Event;
			var transportMode = universalEvent?.EventParameters?.TransportMode ?? ZString.Empty;

			return GetTransportLegsForMatching(eventCode, fligthDate, flightNumber, transportMode, transports);
		}

		public static IEnumerable<Transport> GetTransportLegsForMatching(IStmALog log, IEnumerable<Transport> transports)
		{
			var eventCode = log.SL_SE_NKEvent;
			log.Parameters.TryGetValue(Constants.EventReferenceParameters.Codes.VoyageFlightNumber, out var voyageFlightNumber);

			var flightDate = log.Parameters.TryGetValue(Constants.EventReferenceParameters.Codes.FlightDate, out var flightDateString)
				&& ZDateTime.TryParseISO8601Date(flightDateString, out var flightDateTime)
				? flightDateTime.Date
				: ZDate.Empty;

			log.Parameters.TryGetValue(Constants.EventReferenceParameters.Codes.Mode, out var transportMode);

			return GetTransportLegsForMatching(eventCode, flightDate, voyageFlightNumber, transportMode ?? ZString.Empty, transports);
		}

		static IEnumerable<Transport> GetTransportLegsForMatching(ZString eventCode, ZDate flightDate, ZString flightNumber, ZString transportMode, IEnumerable<Transport> transports)
		{
			var xmlParams = new IZType[] { flightDate, flightNumber };
			var countParams = xmlParams.Count(x => !x.IsEmpty);

			if (countParams == 1)
			{
				return Enumerable.Empty<Transport>();
			}

			if (countParams == 2)
			{
				var isCompareETA = (eventCode == Events.ArrivalCode);
				var filteredTransports = transports.Where(x => x.IsAir && x.IsFlightDateMatched(isCompareETA, flightDate));
				var matchedTransports = filteredTransports.Where(x => x.IsVoyageFlightMatched(flightNumber));

				return GetTransportLegsForMatchingTransportMode(
					transportMode,
					matchedTransports.Any() ? matchedTransports : filteredTransports.Where(x => x.IsVoyageFlightFuzzyMatched(flightNumber)));
			}

			return GetTransportLegsForMatchingTransportMode(
				transportMode,
				transports.Where(x => !x.IsAir));
		}

		static IEnumerable<Transport> GetTransportLegsForMatchingTransportMode(ZString transportMode, IEnumerable<Transport> transports)
		{
			return transportMode.IsEmpty ? transports : transports.Where(x => x.JW_TransportMode == transportMode || x.JW_AdditionalTransportMode == transportMode);
		}

		#endregion
	}
}
