using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class ConsolByEventMatcher
	{
		public CommonConsol MatchConsolByEvent(CommonShipment shipment, IStmALog eventLog)
		{
			if (eventLog == null || shipment == null || !shipment.Consols.Any())
			{
				return null;
			}

			var result = MatchByEventParent(shipment, eventLog.SL_Table, eventLog.SL_Parent)
				?? MatchByPropagationSource(shipment, eventLog)
				?? MatchByEventReference(shipment, eventLog.SL_SE_NKEvent, eventLog.SL_Reference)
				?? MatchByEventTime(shipment, eventLog.SL_EventTime);

			return result;
		}

		#region Implementation

		#region By parent

		CommonConsol MatchByEventParent(CommonShipment shipment, ZString eventParentTableName, ZGuid eventParentPK)
		{
			CommonConsol result = null;

			var allConsols = shipment.Consols.Cast<CommonConsol>();
			switch (eventParentTableName)
			{
				case JobConsolSchema.Constants.TableName:
					result = allConsols.FirstOrDefault(consol => consol.PK == eventParentPK);
					break;

				case JobConsolTransportSchema.Constants.TableName:
					result = allConsols.FirstOrDefault(consol => consol.Transports.Any(transport => transport.PK == eventParentPK));
					break;
			}

			return result;
		}

		#endregion

		#region By Propagation Source

		CommonConsol MatchByPropagationSource(CommonShipment shipment, IStmALog eventLog)
		{
			if (!PropagationHandler.IsPropagatedEventLog(eventLog))
			{
				return null;
			}

			foreach (var container in shipment.Containers)
			{
				if (HasPropagatedEventSource(container, eventLog))
				{
					return container.Consol;
				}
			}

			return null;
		}

		bool HasPropagatedEventSource(CommonContainer container, IStmALog eventLog)
		{
			var containerLogs = container.Logs.GetAllLogs();
			foreach (var containerLog in containerLogs.Cast<IStmALog>())
			{
				if (containerLog.SL_EventTime.Equals(eventLog.EventTime) && containerLog.SL_SE_NKEvent.EqualsIgnoringCase(eventLog.SL_SE_NKEvent))
				{
					return true;
				}
			}

			return false;
		}

		#endregion

		#region By reference

		CommonConsol MatchByEventReference(CommonShipment shipment, ZString eventCode, ZString eventReference)
		{
			var eventParameters = StmALog.GetParametersFromReference(eventReference) as IDictionary<string, string>;
			if (eventParameters == null || !eventParameters.Any())
			{
				return null;
			}

			var result = MatchByFacility(shipment, eventParameters)
				?? MatchByLocationToConsolLegs(shipment, eventParameters)
				?? MatchByFlightDetails(shipment, eventCode, eventParameters)
				?? MatchByLocationToShipmentOrConsolPorts(shipment, eventParameters);

			return result;
		}

		CommonConsol MatchByFacility(CommonShipment shipment, IDictionary<string, string> parameters)
		{
			string facility;
			parameters.TryGetValue(Constants.EventReferenceParameters.Codes.Facility, out facility);

			if (!string.IsNullOrEmpty(facility))
			{
				if (facility.Equals(Constants.Facilities.Code.Consignor, StringComparison.OrdinalIgnoreCase))
				{
					return shipment.Consols.GetEarliestConsol();
				}
				else if (facility.Equals(Constants.Facilities.Code.Consignee, StringComparison.OrdinalIgnoreCase))
				{
					return shipment.Consols.GetLatestConsol();
				}
			}

			return null;
		}

		CommonConsol MatchByLocationToConsolLegs(CommonShipment shipment, IDictionary<string, string> parameters)
		{
			string location;
			parameters.TryGetValue(Constants.EventReferenceParameters.Codes.Location, out location);

			if (!string.IsNullOrEmpty(location) && location.Length == 5)
			{
				location = location.ToUpperInvariant();

				Func<CommonConsol, bool> isConsolLegMatched = (consol) =>
					consol.Transports.Cast<Transport>()
					.Any(transport => transport.JW_RL_NKLoadPort == location || transport.JW_RL_NKDiscPort == location);

				var matchedConsols = shipment.Consols.Cast<CommonConsol>()
					.Where(isConsolLegMatched)
					.Take(2)
					.ToArray();

				if (matchedConsols.Length == 1)
				{
					return matchedConsols[0];
				}
			}

			return null;
		}

		CommonConsol MatchByFlightDetails(CommonShipment shipment, ZString eventCode, IDictionary<string, string> parameters)
		{
			if (eventCode != Events.ArrivalCode && eventCode != Events.DepartureCode)
			{
				return null;
			}

			string voyageFlightNumber;
			parameters.TryGetValue(Constants.EventReferenceParameters.Codes.VoyageFlightNumber, out voyageFlightNumber);

			string flightDateAsString;
			parameters.TryGetValue(Constants.EventReferenceParameters.Codes.FlightDate, out flightDateAsString);

			ZDateTime flightDate = ZDateTime.Invalid;
			if (!string.IsNullOrEmpty(flightDateAsString))
			{
				if (!ZDateTime.TryParseISO8601Date(flightDateAsString, out flightDate))
				{
					ZDateTime.TryParseExact(flightDateAsString, out flightDate, ZDateTime.ShortDateFormat);
				}
			}

			if (!string.IsNullOrEmpty(voyageFlightNumber) && flightDate.Date.IsValid && !flightDate.Date.IsEmpty)
			{
				bool compareETA = (eventCode == Events.ArrivalCode);

				Func<CommonConsol, bool> isConsolAirLegMatched = (consol) =>
					consol.Transports.Cast<Transport>()
					.Any(transport => transport.IsAir
										&& transport.IsVoyageFlightMatched(voyageFlightNumber)
										&& transport.IsFlightDateMatched(compareETA, flightDate.Date));

				var matchedConsols = shipment.Consols.Cast<CommonConsol>()
					.Where(isConsolAirLegMatched)
					.Take(2)
					.ToArray();

				if (matchedConsols.Length == 1)
				{
					return matchedConsols[0];
				}
			}

			return null;
		}

		CommonConsol MatchByLocationToShipmentOrConsolPorts(CommonShipment shipment, IDictionary<string, string> parameters)
		{
			string location;
			parameters.TryGetValue(Constants.EventReferenceParameters.Codes.Location, out location);

			if (string.IsNullOrEmpty(location))
			{
				return null;
			}

			location = location.ToUpperInvariant();

			var earliestConsol = shipment.Consols.GetEarliestConsol();
			var latestConsol = shipment.Consols.GetLatestConsol();

			var result = MatchByLocationToShipmentPorts(shipment, location, earliestConsol, latestConsol)
				?? MatchByLocationToConsolPorts(location, earliestConsol, latestConsol)
				?? MatchByLocationCountryToShipmentPorts(shipment, location, earliestConsol, latestConsol)
				?? MatchByLocationCountryToConsolPorts(shipment, location, earliestConsol, latestConsol);

			return result;
		}

		CommonConsol MatchByLocationToShipmentPorts(CommonShipment shipment, string location, CommonConsol earliestConsol, CommonConsol latestConsol)
		{
			if (location.Length == 5)
			{
				if (location == shipment.JS_RL_NKOrigin)
				{
					return earliestConsol;
				}
				else if (location == shipment.JS_RL_NKDestination)
				{
					return latestConsol;
				}
			}

			return null;
		}

		CommonConsol MatchByLocationToConsolPorts(string location, CommonConsol earliestConsol, CommonConsol latestConsol)
		{
			if (location.Length == 5)
			{
				if (earliestConsol != null && location == earliestConsol.JK_RL_NKLoadPort)
				{
					return earliestConsol;
				}

				if (latestConsol != null && location == latestConsol.JK_RL_NKDischargePort)
				{
					return latestConsol;
				}
			}

			return null;
		}

		CommonConsol MatchByLocationCountryToShipmentPorts(CommonShipment shipment, string location, CommonConsol earliestConsol, CommonConsol latestConsol)
		{
			string locationCountry = TryGetCountryFromLocation(location);
			if (string.IsNullOrEmpty(locationCountry))
			{
				return null;
			}

			string shipmentOriginCountry = TryGetCountryFromLocation(shipment.JS_RL_NKOrigin);
			string shipmentDestinationCountry = TryGetCountryFromLocation(shipment.JS_RL_NKDestination);

			if (shipmentOriginCountry != shipmentDestinationCountry)
			{
				if (locationCountry == shipmentOriginCountry)
				{
					return earliestConsol;
				}

				if (locationCountry == shipmentDestinationCountry)
				{
					return latestConsol;
				}
			}

			return null;
		}

		CommonConsol MatchByLocationCountryToConsolPorts(CommonShipment shipment, string location, CommonConsol earliestConsol, CommonConsol latestConsol)
		{
			string locationCountry = TryGetCountryFromLocation(location);
			if (string.IsNullOrEmpty(locationCountry))
			{
				return null;
			}

			if (earliestConsol != null)
			{
				string loadCountry = TryGetCountryFromLocation(earliestConsol.JK_RL_NKLoadPort);
				if (!string.IsNullOrEmpty(loadCountry) && locationCountry == loadCountry)
				{
					bool anotherConsolWithSameCountryFound = shipment.Consols.Cast<CommonConsol>()
						.Where(consol => consol.PK != earliestConsol.PK)
						.Any(consol => TryGetCountryFromLocation(consol.JK_RL_NKLoadPort) == loadCountry
										|| TryGetCountryFromLocation(consol.JK_RL_NKDischargePort) == loadCountry);

					return anotherConsolWithSameCountryFound
						? null
						: earliestConsol;
				}
			}

			if (latestConsol != null)
			{
				string dischargeCountry = TryGetCountryFromLocation(latestConsol.JK_RL_NKDischargePort);
				if (!string.IsNullOrEmpty(dischargeCountry) && locationCountry == dischargeCountry)
				{
					bool anotherConsolWithSameCountryFound = shipment.Consols.Cast<CommonConsol>()
						.Where(consol => consol.PK != latestConsol.PK)
						.Any(consol => TryGetCountryFromLocation(consol.JK_RL_NKLoadPort) == dischargeCountry
										|| TryGetCountryFromLocation(consol.JK_RL_NKDischargePort) == dischargeCountry);

					if (!anotherConsolWithSameCountryFound)
					{
						return latestConsol;
					}
				}
			}

			return null;
		}

		string TryGetCountryFromLocation(string location)
		{
			if (!string.IsNullOrEmpty(location))
			{
				location = location.ToUpperInvariant();

				if (location.Length == 2)
				{
					return location;
				}

				if (location.Length == 5)
				{
					return location.Substring(0, 2);
				}
			}

			return string.Empty;
		}

		#endregion

		#region By time

		CommonConsol MatchByEventTime(CommonShipment shipment, ZDateTime eventTime)
		{
			if (!eventTime.IsValid || eventTime.IsEmpty)
			{
				return null;
			}

			var earliestConsol = shipment.Consols.GetEarliestConsol();
			var latestConsol = shipment.Consols.GetLatestConsol();

			Func<CommonConsol, bool> isConsolMatchedByTime = (consol) =>
			{
				var earliestDeparture = consol == earliestConsol && earliestConsol != latestConsol
					? ZDateTime.MinSmallDateTimeValue
					: FindEarliestDeparture(consol);

				if (earliestDeparture.IsEmpty)
				{
					earliestDeparture = ZDateTime.MinSmallDateTimeValue;
				}

				var latestArrival = consol == latestConsol && earliestConsol != latestConsol
					? ZDateTime.MaxSmallDateTimeValue
					: FindLatestArrival(consol);

				if (latestArrival.IsEmpty)
				{
					latestArrival = ZDateTime.MaxSmallDateTimeValue;
				}

				return eventTime >= earliestDeparture && eventTime <= latestArrival;
			};

			var matchedConsols = shipment.Consols.Cast<CommonConsol>()
				.Where(isConsolMatchedByTime)
				.Take(2)
				.ToArray();

			if (matchedConsols.Length == 1)
			{
				return matchedConsols[0];
			}

			return null;
		}

		ZDateTime FindEarliestDeparture(CommonConsol consol)
		{
			ZDateTime earliestDeparture = ZDateTime.Empty;

			foreach (Transport transport in consol.Transports)
			{
				var departureDate = transport.JW_ATD.IsValid && !transport.JW_ATD.IsEmpty
					? transport.JW_ATD
					: transport.JW_ETD;

				if (departureDate.IsValid && !departureDate.IsEmpty && (departureDate < earliestDeparture || earliestDeparture.IsEmpty))
				{
					earliestDeparture = departureDate;
				}
			}

			return earliestDeparture;
		}

		ZDateTime FindLatestArrival(CommonConsol consol)
		{
			ZDateTime latestArrival = ZDateTime.Empty;

			foreach (Transport transport in consol.Transports)
			{
				var arrivalDate = transport.JW_ATA.IsValid && !transport.JW_ATA.IsEmpty
					? transport.JW_ATA
					: transport.JW_ETA;

				if (arrivalDate.IsValid && !arrivalDate.IsEmpty && (arrivalDate > latestArrival || latestArrival.IsEmpty))
				{
					latestArrival = arrivalDate;
				}
			}

			return latestArrival;
		}

		#endregion

		#endregion
	}
}
