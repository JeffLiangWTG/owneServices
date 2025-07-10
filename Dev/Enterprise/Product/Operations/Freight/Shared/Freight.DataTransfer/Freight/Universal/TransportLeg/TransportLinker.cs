using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using AutoEvents = Enterprise.ZArchitecture.Business.AutoEvents;
using CommonShipment = Enterprise.Freight.Business.CommonShipment;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;
using Events = Enterprise.ZArchitecture.Business.Events;

namespace Enterprise.Freight.DataTransfer.Universal
{
	sealed class TransportLinker
	{
		public Transport GetLogParent(ITransportParent parent, IXmlEventValueObject xmlEvent, EventLegPorts eventLegPorts)
		{
			var eventType = xmlEvent.EventType.ToString();

			if (eventType == Events.ReceivedCode
				|| eventType == Events.FreightUnloadedCode
				|| IsDepartureOrArrivalEventWithNonTerminalFacility(xmlEvent))
			{
				return null;
			}

			var transports = GetAllPossibleTransportLegs(parent);

			transports = FilterByEventTransportMode(xmlEvent, transports);

			if (eventLegPorts.HasAPort)
			{
				return GetBestMatchingTransportLegBasedOnPorts(transports, eventLegPorts, xmlEvent);
			}

			return eventType == AutoEvents.FreightLoadedCode
				? null
				: GetBestMatchingTransportLeg(transports, xmlEvent) ?? GetFirstTransportLegWithoutThisEvent(xmlEvent, transports);
		}

		IEnumerable<Transport> FilterByEventTransportMode(IXmlEventValueObject xmlEvent, IEnumerable<Transport> transports)
		{
			if (xmlEvent is Event concreteEvent && !string.IsNullOrEmpty(concreteEvent.EventParameters?.TransportMode.GetValueOrDefault()))
			{
				transports = transports.Where(transport => transport.TransportMode.EqualsIgnoringCase(concreteEvent.EventParameters.TransportMode));
			}

			return transports;
		}

		bool IsDepartureOrArrivalEventWithNonTerminalFacility(IXmlEventValueObject xmlEvent)
		{
			var eventType = xmlEvent.EventType.ToString();
			var isDepartureOrArrivalEvent = eventType == Events.DepartureCode || eventType == Events.ArrivalCode;
			var facility = EventLegPorts.GetParameter(xmlEvent, Constants.EventReferenceParameters.Codes.Facility);

			return isDepartureOrArrivalEvent && (!string.IsNullOrWhiteSpace(facility) && facility != Constants.Facilities.Code.Terminal);
		}

		static IEnumerable<Transport> GetAllPossibleTransportLegs(ITransportParent parent)
		{
			var shipmentParent = parent as CommonShipment;
			var transports = shipmentParent == null ? parent.Transports.OfType<Transport>() : shipmentParent.TransportsIncludingRelated.OfType<Transport>();
			return transports.OrderBy(o => o.JW_LegOrder);
		}

		Transport GetBestMatchingTransportLegBasedOnPorts(IEnumerable<Transport> transports, EventLegPorts eventLegPorts, IXmlEventValueObject xmlEvent)
		{
			string eventCode = xmlEvent.EventType;

			if (EventLegPorts.IsDepartureEvents(xmlEvent) && !eventLegPorts.LegOrigins.IsNullOrEmpty())
			{
				var matchedTransportLegs = FreightEventsHelper.GetTransportLegsForMatching(xmlEvent, transports);
				var matchedLeg = matchedTransportLegs.FirstOrDefault(transport => eventLegPorts.LegOrigins.Any(origin => origin.RL_Code == transport.JW_RL_NKLoadPort));

				if (matchedLeg != null && matchedLeg.IsAir)
				{
					matchedLeg.JW_IsLinked = false;
				}

				CheckTransportLegAgainstEventVessel(matchedLeg, xmlEvent);

				if (eventCode == AutoEvents.FreightLoadedCode)
				{
					return null;
				}

				return matchedLeg;
			}

			if (eventCode == AutoEvents.ArrivalCode && !eventLegPorts.LegDestinations.IsNullOrEmpty())
			{
				var matchedTransportlLegs = FreightEventsHelper.GetTransportLegsForMatching(xmlEvent, transports);
				var matchedLeg = matchedTransportlLegs.FirstOrDefault(transport => eventLegPorts.LegDestinations.Any(destination => destination.RL_Code == transport.JW_RL_NKDiscPort));

				if (matchedLeg != null && matchedLeg.IsAir)
				{
					matchedLeg.JW_IsLinked = false;
				}

				CheckTransportLegAgainstEventVessel(matchedLeg, xmlEvent);

				return matchedLeg;
			}

			Transport result = null;
			foreach (var transport in transports)
			{
				var originMatch = eventLegPorts.LegOrigins.Any(x => x.RL_Code == transport.JW_RL_NKLoadPort);
				var destinationMatch = eventLegPorts.LegDestinations.Any(x => x.RL_Code == transport.JW_RL_NKDiscPort);

				if (result == null && (originMatch || destinationMatch))
				{
					result = transport;
				}

				if (originMatch && destinationMatch)
				{
					return transport;
				}
			}

			return result;
		}

		void CheckTransportLegAgainstEventVessel(Transport transport, IXmlEventValueObject xmlEvent)
		{
			if (transport == null || !transport.IsSea)
			{
				return;
			}

			if (EventLegPorts.GetParameter(xmlEvent, Constants.EventReferenceParameters.Codes.Mode) != Core.Constants.TransportModes.Sea ||
				EventLegPorts.GetParameter(xmlEvent, Constants.EventReferenceParameters.Codes.Facility) != Constants.Facilities.Code.Terminal)
			{
				return;
			}

			var eventCode = xmlEvent.EventType;

			if (eventCode != AutoEvents.DepartureCode &&
				eventCode != AutoEvents.ArrivalCode &&
				eventCode != AutoEvents.FreightLoadedCode)
			{
				return;
			}

			var eventVesselName = xmlEvent.Context.VesselName.GetValueOrDefault();
			var eventLloyds = xmlEvent.Context.LloydsNumber.GetValueOrDefault();

			if (!transport.IsVesselMatched(eventLloyds, eventVesselName) && (!eventVesselName.IsEmpty || !eventLloyds.IsEmpty))
			{
				transport.JW_IsLinked = false;
				AddContainerAutomationSTULog(transport, (NoResString)"Change of Vessel Detected", transport.JW_RL_NKLoadPort, eventVesselName);
				return;
			}

			if (eventCode == AutoEvents.DepartureCode && !xmlEvent.IsEstimate)
			{
				transport.JW_IsLinked = false;
			}
		}

		void AddContainerAutomationSTULog(Transport bizObj, ZString type, ZString location, ZString vesselName)
		{
			bizObj.Logs.AddNew(
				AutoEvents.StatusUpdated,
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, type),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.Facility, Constants.Facilities.Code.Terminal),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.Location, location),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.MessageType, (NoResString)"Container Automation"),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.VoyageFlightNumber, vesselName));
		}

		Transport GetBestMatchingTransportLeg(IEnumerable<Transport> transports, IXmlEventValueObject xmlEvent)
		{
			var matchingTransports = from transport in transports
									 let matchingScore = CalculateMatchingScore(xmlEvent, transport)
									 where matchingScore > 0
									 orderby matchingScore descending
									 select transport;

			return matchingTransports.FirstOrDefault();
		}

		int CalculateMatchingScore(IXmlEventValueObject xmlEvent, Transport transport)
		{
			var result = 0;
			var factory = transport.Factory;
			var airport = FindUNLOCOByIATACode(xmlEvent.Context.IATAAirportCode, factory);
			var originPort = FindUNLOCOByIATACode(xmlEvent.Context.OriginIATAAirportCode, factory) ?? airport;
			var destinationPort = FindUNLOCOByIATACode(xmlEvent.Context.DestinationIATAAirportCode, factory) ?? airport;

			if (transport.IsAir)
			{
				var flightNumber = xmlEvent.Context.FlightNumber.GetValueOrDefault();
				var voyageFlightMatchedScore = transport.IsVoyageFlightMatched(flightNumber)
										? 2
										: transport.IsVoyageFlightFuzzyMatched(flightNumber) ? 1 : 0;

				if (voyageFlightMatchedScore > 0)
				{
					result += voyageFlightMatchedScore;
					if (transport.LoadPort == originPort || transport.DiscPort == destinationPort)
					{
						result += 8;
					}

					var flightDate = xmlEvent.Context.FlightDate.Date;
					var eventType = xmlEvent.EventType.ToString();

					if (flightDate.IsValid && (eventType == Events.ArrivalCode || eventType == Events.DepartureCode))
					{
						var isCompareETA = (eventType == Events.ArrivalCode);
						if (transport.IsFlightDateMatched(isCompareETA, flightDate))
						{
							result += 8;
						}
					}
				}
			}
			else
			{
				var voyageNumber = xmlEvent.Context.VoyageNumber.GetValueOrDefault();
				var vesselName = xmlEvent.Context.VesselName.GetValueOrDefault();

				if (voyageNumber == transport.JW_VoyageFlight)
				{
					result += 1;
				}

				if (vesselName == transport.JW_Vessel)
				{
					result += 1;
				}
			}

			if (transport.LoadPort == originPort)
			{
				result += 2;
			}

			if (transport.DiscPort == destinationPort)
			{
				result += 4;
			}

			return result;
		}

		RefUNLOCO FindUNLOCOByIATACode(ZString iATACode, BusinessObjectFactory factory)
		{
			if (iATACode.IsEmpty)
			{
				return null;
			}

			var query = new ZQuery(RefUNLOCOSchema.RL_IATA, iATACode);
			var unlocos = factory.Load<RefUNLOCO>(query);
			return unlocos.FirstOrDefault();
		}

		Transport GetFirstTransportLegWithoutThisEvent(IXmlEventValueObject xmlEvent, IEnumerable<Transport> transports)
		{
			var airTransports = transports.Where(transport => transport.IsAir);

			var firstAirTransportThatDoesNotHaveEventType = airTransports.FirstOrDefault(t => !t.HasLogWith(xmlEvent.EventType, xmlEvent.IsEstimate));

			return firstAirTransportThatDoesNotHaveEventType ?? airTransports.FirstOrDefault();
		}
	}
}
