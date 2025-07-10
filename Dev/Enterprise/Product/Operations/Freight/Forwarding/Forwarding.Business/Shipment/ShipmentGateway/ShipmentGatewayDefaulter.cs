using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business
{
	static class ShipmentGatewayDefaulter
	{
		internal static void ReorderGateways(this ForwardingShipment shipment)
		{
			if (HasRequiredPortsToDefault(shipment))
			{
				var parameters = new GatewayDefaultSearchParameters
				{
					Factory = shipment.Factory,
					Load = shipment.JS_RL_NKLoadPort,
					Discharge = shipment.JS_RL_NKDischargePort,
					Origin = shipment.JS_RL_NKOrigin,
					Destination = shipment.JS_RL_NKDestination,
					ZoneModes = GetValidZoneModes(shipment.JS_TransportMode, shipment.JS_PackingMode)
				};

				var matches = FindBestMatchingForwarders(parameters);
				if (matches != null)
				{
					var destinationForwarder = GetDestinationFromBestMatches(matches);
					if (destinationForwarder != null)
					{
						var matchingForwarderAddress = GetMatchingForwarderAddress(shipment, destinationForwarder);
						ReorderGatewaysSoDestinationIsAtTheEnd(shipment, matchingForwarderAddress?.JSG_OA_ForwarderAddress ?? ZGuid.Empty);
					}

					var originForwarder = GetOriginFromBestMatches(matches);
					if (originForwarder != null)
					{
						var matchingForwarderAddress = GetMatchingForwarderAddress(shipment, originForwarder);
						ReorderGatewaysSoOriginIsAtTheStart(shipment, matchingForwarderAddress?.JSG_OA_ForwarderAddress ?? ZGuid.Empty);
					}
				}
			}
		}

		internal static void UpdateGateways(this ForwardingShipment shipment)
		{
			if (HasRequiredPortsToDefault(shipment))
			{
				var parameters = new GatewayDefaultSearchParameters
				{
					Factory = shipment.Factory,
					Load = shipment.JS_RL_NKLoadPort,
					Discharge = shipment.JS_RL_NKDischargePort,
					Origin = shipment.JS_RL_NKOrigin,
					Destination = shipment.JS_RL_NKDestination,
					ZoneModes = GetValidZoneModes(shipment.JS_TransportMode, shipment.JS_PackingMode)
				};

				var matches = FindBestMatchingForwarders(parameters);

				if (matches != null)
				{
					AddBestMatchingGatewayToShipment(shipment, GetOriginFromBestMatches(matches), Position.Start);
					AddBestMatchingGatewayToShipment(shipment, GetDestinationFromBestMatches(matches), Position.End);
				}
			}
		}

		static bool HasRequiredPortsToDefault(ForwardingShipment shipment)
		{
			return !shipment.IsDeleted && !string.IsNullOrEmpty(shipment.JS_TransportMode)
				&& (!string.IsNullOrEmpty(shipment.JS_RL_NKLoadPort) || !string.IsNullOrEmpty(shipment.JS_RL_NKOrigin))
				&& (!string.IsNullOrEmpty(shipment.JS_RL_NKDischargePort) || !string.IsNullOrEmpty(shipment.JS_RL_NKDestination));
		}

		static void AddBestMatchingGatewayToShipment(ForwardingShipment shipment, OrgHeader gatewayAgent, Position position)
		{
			if (gatewayAgent != null)
			{
				var appointedAgentAddress = GetAppointedAgent(gatewayAgent, shipment, position);
				var matchingForwarderAddress = GetMatchingForwarderAddress(shipment, gatewayAgent);
				var matchingForwarderAddressPK = matchingForwarderAddress?.JSG_OA_ForwarderAddress ?? ZGuid.Empty;

				var shouldAddGateway = appointedAgentAddress != ZGuid.Empty && matchingForwarderAddress == null;

				if (shouldAddGateway)
				{
					var newGateway = shipment.Gateways.AddNew();
					newGateway.JSG_OA_ForwarderAddress = appointedAgentAddress;
					matchingForwarderAddressPK = appointedAgentAddress;
				}

				var shouldRepositionGateway = shouldAddGateway
					|| !shipment.Consols.Cast<ForwardingConsol>().Any(c => IsGatewayFromConsol(c, matchingForwarderAddressPK));

				if (shouldRepositionGateway)
				{
					if (position == Position.Start)
					{
						ReorderGatewaysSoOriginIsAtTheStart(shipment, matchingForwarderAddressPK);
					}
					else
					{
						ReorderGatewaysSoDestinationIsAtTheEnd(shipment, matchingForwarderAddressPK);
					}
				}
			}
		}

		static ShipmentGateway GetMatchingForwarderAddress(ForwardingShipment shipment, OrgHeader gatewayAgent)
		{
			var matchingForwarderAddress = shipment
				.Gateways
				.FirstOrDefault(gateway => gateway.ForwarderAddress.OA_OH == gatewayAgent.PK);

			return matchingForwarderAddress;
		}

		static ZGuid GetAppointedAgent(OrgHeader gatewayAgentOrgHeader, ForwardingShipment shipment, Position position)
		{
			if (position == Position.Start)
			{
				var appointedAgent = GetAppointedAgentWithPortOrCountryFromOrgHeaderIfExists(gatewayAgentOrgHeader, shipment.JS_RL_NKLoadPort, shipment.TransportMode, position);

				if (appointedAgent == ZGuid.Empty)
				{
					appointedAgent = GetAppointedAgentWithPortOrCountryFromOrgHeaderIfExists(gatewayAgentOrgHeader, shipment.JS_RL_NKOrigin, shipment.TransportMode, position);
				}
				if (appointedAgent == ZGuid.Empty)
				{
					appointedAgent = GetAppointedAgentWithPortOrCountryFromOrgHeaderIfExists(gatewayAgentOrgHeader, shipment.JS_RL_NKLoadPort.SubstringSafe(0, 2), shipment.TransportMode, position);
				}
				if (appointedAgent == ZGuid.Empty)
				{
					appointedAgent = GetAppointedAgentWithPortOrCountryFromOrgHeaderIfExists(gatewayAgentOrgHeader, shipment.JS_RL_NKOrigin.SubstringSafe(0, 2), shipment.TransportMode, position);
				}
				if (appointedAgent == ZGuid.Empty)
				{
					appointedAgent = gatewayAgentOrgHeader.MainAddress.PK;
				}
				return appointedAgent;
			}
			else
			{
				var appointedAgent = GetAppointedAgentWithPortOrCountryFromOrgHeaderIfExists(gatewayAgentOrgHeader, shipment.JS_RL_NKDischargePort, shipment.TransportMode, position);

				if (appointedAgent == ZGuid.Empty)
				{
					appointedAgent = GetAppointedAgentWithPortOrCountryFromOrgHeaderIfExists(gatewayAgentOrgHeader, shipment.JS_RL_NKDestination, shipment.TransportMode, position);
				}
				if (appointedAgent == ZGuid.Empty)
				{
					appointedAgent = GetAppointedAgentWithPortOrCountryFromOrgHeaderIfExists(gatewayAgentOrgHeader, shipment.JS_RL_NKDischargePort.SubstringSafe(0, 2), shipment.TransportMode, position);
				}
				if (appointedAgent == ZGuid.Empty)
				{
					appointedAgent = GetAppointedAgentWithPortOrCountryFromOrgHeaderIfExists(gatewayAgentOrgHeader, shipment.JS_RL_NKDestination.SubstringSafe(0, 2), shipment.TransportMode, position);
				}
				if (appointedAgent == ZGuid.Empty)
				{
					appointedAgent = gatewayAgentOrgHeader.MainAddress.PK;
				}
				return appointedAgent;
			}
		}

		static ZGuid GetAppointedAgentWithPortOrCountryFromOrgHeaderIfExists(OrgHeader orgHeader, ZString portOrCountry, ZString transportMode, Position position)
		{
			var appointedAgents = orgHeader
				.AppointedGatewayAgentPorts
				.Cast<OrgAppointedAgentPorts>()
				.Where(agentPort => agentPort.O5_PortOrCountry == portOrCountry);

			if (position == Position.Start)
			{
				appointedAgents = appointedAgents.Where(agentPort => agentPort.O5_AgentDirection == "BTH" || agentPort.O5_AgentDirection == "EXP");
			}
			else if (position == Position.End)
			{
				appointedAgents = appointedAgents.Where(agentPort => agentPort.O5_AgentDirection == "BTH" || agentPort.O5_AgentDirection == "IMP");
			}

			OrgAppointedAgentPorts appointedAgent = null;

			switch (transportMode)
			{
				case TransportModes.Air:
					var airGTT = appointedAgents
						.FirstOrDefault(agentPort => agentPort.O5_AirAgentStatus == GatewayPreviousSendingAgent.Codes.GatewayAgentWithFT);

					var airGTA = appointedAgents
						.FirstOrDefault(agentPort => agentPort.O5_AirAgentStatus == GatewayPreviousSendingAgent.Codes.GatewayAgent);

					appointedAgent = airGTT ?? airGTA;
					break;

				case TransportModes.Sea:
					var seaGTT = appointedAgents
						.FirstOrDefault(agentPort => agentPort.O5_SeaAgentStatus == GatewayPreviousSendingAgent.Codes.GatewayAgentWithFT);

					var seaGTA = appointedAgents
						.FirstOrDefault(agentPort => agentPort.O5_SeaAgentStatus == GatewayPreviousSendingAgent.Codes.GatewayAgent);

					appointedAgent = seaGTT ?? seaGTA;
					break;

				case TransportModes.Rail:
					var railGTT = appointedAgents
						.FirstOrDefault(agentPort => agentPort.O5_RailAgentStatus == GatewayPreviousSendingAgent.Codes.GatewayAgentWithFT);

					var railGTA = appointedAgents
						.FirstOrDefault(agentPort => agentPort.O5_RailAgentStatus == GatewayPreviousSendingAgent.Codes.GatewayAgent);

					appointedAgent = railGTT ?? railGTA;
					break;

				case TransportModes.Road:
					var roadGTT = appointedAgents
						.FirstOrDefault(agentPort => agentPort.O5_RoadAgentStatus == GatewayPreviousSendingAgent.Codes.GatewayAgentWithFT);

					var roadGTA = appointedAgents
						.FirstOrDefault(agentPort => agentPort.O5_RoadAgentStatus == GatewayPreviousSendingAgent.Codes.GatewayAgent);

					appointedAgent = roadGTT ?? roadGTA;
					break;
			}

			return appointedAgent?.O5_OA_AgentOfficeAddress ?? ZGuid.Empty;
		}

		static bool IsGatewayFromConsol(ForwardingConsol consol, ZGuid forwarderAddress)
		{
			return (!consol.JK_SendingForwarderHandlingType.IsEmpty && consol.JK_OA_SendingForwarderAddress == forwarderAddress)
				|| (!consol.JK_ReceivingForwarderHandlingType.IsEmpty && consol.JK_OA_ReceivingForwarderAddress == forwarderAddress);
		}

		static void ReorderGatewaysSoOriginIsAtTheStart(ForwardingShipment shipment, ZGuid address)
		{
			var gatewayToMove = shipment.Gateways.FirstOrDefault(g => g.JSG_OA_ForwarderAddress == address);

			if (gatewayToMove != null)
			{
				var gatewaySequences = shipment.Gateways
				.Where(g => g.JSG_Sequence <= gatewayToMove.JSG_Sequence)
				.Select(g => g.JSG_Sequence)
				.OrderByDescending(g => g)
				.ToList();

				for (var i = 0; i < gatewaySequences.Count - 1; i++)
				{
					shipment.Gateways.SwapGateways(gatewaySequences[i], gatewaySequences[i + 1]);
				}
			}
		}

		static void ReorderGatewaysSoDestinationIsAtTheEnd(ForwardingShipment shipment, ZGuid address)
		{
			var gatewayToMove = shipment.Gateways.FirstOrDefault(g => g.JSG_OA_ForwarderAddress == address);

			if (gatewayToMove != null)
			{
				var gatewaySequences = shipment.Gateways
				   .Where(g => g.JSG_Sequence >= gatewayToMove.JSG_Sequence)
				   .Select(g => g.JSG_Sequence)
				   .OrderBy(g => g)
				   .ToList();

				for (var i = 0; i < gatewaySequences.Count - 1; i++)
				{
					shipment.Gateways.SwapGateways(gatewaySequences[i], gatewaySequences[i + 1]);
				}
			}
		}

		static OrgHeader GetOriginFromBestMatches(IEnumerable<ForwarderPositionPair> matches)
		{
			return matches.FirstOrDefault(m => m.Position == Position.Start)?.Org;
		}

		static OrgHeader GetDestinationFromBestMatches(IEnumerable<ForwarderPositionPair> matches)
		{
			return matches.FirstOrDefault(m => m.Position == Position.End)?.Org;
		}

		#region FindBestMatchingForwarders

#if DEBUG
		internal
#endif
		static IEnumerable<ForwarderPositionPair> FindBestMatchingForwarders(GatewayDefaultSearchParameters parameters)
		{
			var matches = FindBestMatchingForwardersWithBothOriginAndDestinationZones(parameters)
				?? FindBestMatchingForwardersWithEitherOriginOrDestinationZones(parameters);

			return matches;
		}

		static IEnumerable<ForwarderPositionPair> FindBestMatchingForwardersWithBothOriginAndDestinationZones(GatewayDefaultSearchParameters parameters)
		{
			return GetBestForwardersWithBothOriginAndDestinationZones(parameters, searchOriginByCountry: false, searchDestinationByCountry: false)
			   ?? GetBestForwardersWithBothOriginAndDestinationZones(parameters, searchOriginByCountry: true, searchDestinationByCountry: false)
			   ?? GetBestForwardersWithBothOriginAndDestinationZones(parameters, searchOriginByCountry: false, searchDestinationByCountry: true)
			   ?? GetBestForwardersWithBothOriginAndDestinationZones(parameters, searchOriginByCountry: true, searchDestinationByCountry: true);
		}

		static IEnumerable<ForwarderPositionPair> FindBestMatchingForwardersWithEitherOriginOrDestinationZones(GatewayDefaultSearchParameters parameters)
		{
			var originMatchesByPort = GetBestForwardersWithOriginZoneOnly(parameters, searchOriginByCountry: false);
			var originMatchesByCountry = GetBestForwardersWithOriginZoneOnly(parameters, searchOriginByCountry: true);
			var originMatches = originMatchesByPort ?? originMatchesByCountry;

			var destinationMatchesByPort = GetBestForwardersWithDestinationZoneOnly(parameters, searchDestinationByCountry: false);
			var destinationMatchesByCountry = GetBestForwardersWithDestinationZoneOnly(parameters, searchDestinationByCountry: true);
			var destinationMatches = destinationMatchesByPort ?? destinationMatchesByCountry;

			var bestMatchingForwarders = new List<ForwarderPositionPair>();
			if (originMatches != null && originMatches.Count() == 1)
			{
				bestMatchingForwarders.AddRange(originMatches);
			}
			if (destinationMatches != null && destinationMatches.Count() == 1)
			{
				bestMatchingForwarders.AddRange(destinationMatches);
			}

			return bestMatchingForwarders;
		}

		static IEnumerable<ForwarderPositionPair> GetBestForwardersWithDestinationZoneOnly(GatewayDefaultSearchParameters parameters, bool searchDestinationByCountry)
		{
			IEnumerable<ForwarderPositionPair> destinationMatches = null;
			if (!string.IsNullOrEmpty(parameters.Discharge))
			{
				destinationMatches = FindMatchingForwardersFromZones(parameters.Factory, string.Empty, parameters.Discharge, false, searchDestinationByCountry, parameters.ZoneModes);
			}

			if (destinationMatches == null && !string.IsNullOrEmpty(parameters.Destination))
			{
				destinationMatches = FindMatchingForwardersFromZones(parameters.Factory, string.Empty, parameters.Destination, false, searchDestinationByCountry, parameters.ZoneModes);
			}

			return destinationMatches;
		}

		static IEnumerable<ForwarderPositionPair> GetBestForwardersWithOriginZoneOnly(GatewayDefaultSearchParameters parameters, bool searchOriginByCountry)
		{
			IEnumerable<ForwarderPositionPair> originMatches = null;

			if (!string.IsNullOrEmpty(parameters.Load))
			{
				originMatches = FindMatchingForwardersFromZones(parameters.Factory, parameters.Load, string.Empty, searchOriginByCountry, false, parameters.ZoneModes);
			}

			if (originMatches == null && !string.IsNullOrEmpty(parameters.Origin))
			{
				originMatches = FindMatchingForwardersFromZones(parameters.Factory, parameters.Origin, string.Empty, searchOriginByCountry, false, parameters.ZoneModes);
			}

			return originMatches;
		}

		static IEnumerable<ForwarderPositionPair> GetBestForwardersWithBothOriginAndDestinationZones(GatewayDefaultSearchParameters parameters, bool searchOriginByCountry, bool searchDestinationByCountry)
		{
			var matches = FindMatchingForwardersFromBothZonesIfNotEmpty(parameters.Factory, parameters.Load, parameters.Discharge, searchOriginByCountry, searchDestinationByCountry, parameters.ZoneModes)
				?? FindMatchingForwardersFromBothZonesIfNotEmpty(parameters.Factory, parameters.Origin, parameters.Discharge, searchOriginByCountry, searchDestinationByCountry, parameters.ZoneModes)
				?? FindMatchingForwardersFromBothZonesIfNotEmpty(parameters.Factory, parameters.Load, parameters.Destination, searchOriginByCountry, searchDestinationByCountry, parameters.ZoneModes)
				?? FindMatchingForwardersFromBothZonesIfNotEmpty(parameters.Factory, parameters.Origin, parameters.Destination, searchOriginByCountry, searchDestinationByCountry, parameters.ZoneModes);

			return matches;
		}

		#endregion

		#region FindMatchingForwardersFromZones

		static IEnumerable<ForwarderPositionPair> FindMatchingForwardersFromBothZonesIfNotEmpty(BusinessObjectFactory factory, string originZone, string destinationZone, bool searchOriginByCountry, bool searchDestinationByCountry, IEnumerable<ZString> zoneModes)
		{
			if (string.IsNullOrEmpty(originZone) || string.IsNullOrEmpty(destinationZone))
			{
				return null;
			}

			return FindMatchingForwardersFromZones(factory, originZone, destinationZone, searchOriginByCountry, searchDestinationByCountry, zoneModes);
		}

#if DEBUG
		internal
#endif
		static IEnumerable<ForwarderPositionPair> FindMatchingForwardersFromZones(BusinessObjectFactory factory, string originZone, string destinationZone, bool searchOriginByCountry, bool searchDestinationByCountry, IEnumerable<ZString> zoneModes)
		{
			var orgQuery = new ZDBOnlyQuery(typeof(OrgHeader));

			if (!string.IsNullOrEmpty(originZone))
			{
				var zoneQuery = new ZDBOnlySubQuery(typeof(RefZoneHeader), RefZoneHeaderSchema.FZ_OH_RelatedParty);
				var originQuery = new ZQuery(RefZoneHeaderSchema.FZ_ZoneType, RefZoneHeaderLookups.ZoneTypeCodes.OriginGateway);
				originQuery.AddToFilter(RefZoneHeaderSchema.FZ_ZoneMode, zoneModes);
				var refZonePivotSubQuery = new ZDBOnlySubQuery(typeof(RefZonePivot), RefZonePivotSchema.F2_FZ);
				AddZonePivots(refZonePivotSubQuery, originZone, searchOriginByCountry);
				zoneQuery.AddToFilter(originQuery);
				zoneQuery.AddSubQuery(refZonePivotSubQuery, JoinCondition.And);
				orgQuery.AddSubQuery(zoneQuery, JoinCondition.And);
			}

			if (!string.IsNullOrEmpty(destinationZone))
			{
				var zoneQuery = new ZDBOnlySubQuery(typeof(RefZoneHeader), RefZoneHeaderSchema.FZ_OH_RelatedParty);
				var destinationQuery = new ZQuery(RefZoneHeaderSchema.FZ_ZoneType, RefZoneHeaderLookups.ZoneTypeCodes.DestinationGateway);
				destinationQuery.AddToFilter(RefZoneHeaderSchema.FZ_ZoneMode, zoneModes);
				var refZonePivotSubQuery = new ZDBOnlySubQuery(typeof(RefZonePivot), RefZonePivotSchema.F2_FZ);
				AddZonePivots(refZonePivotSubQuery, destinationZone, searchDestinationByCountry);
				zoneQuery.AddToFilter(destinationQuery);
				zoneQuery.AddSubQuery(refZonePivotSubQuery, JoinCondition.And);
				orgQuery.AddSubQuery(zoneQuery, JoinCondition.And);
			}

			var zones = factory.Load<OrgHeader>(orgQuery);
			if (zones.Any())
			{
				var position = string.IsNullOrEmpty(originZone) && !string.IsNullOrEmpty(destinationZone)
					? Position.End
					: Position.Start;
				return zones.Select(z => new ForwarderPositionPair(z, position));
			}

			return null;
		}

		static void AddZonePivots(ZDBOnlySubQuery refZonePivotSubQuery, string port, bool searchCountryCode)
		{
			if (searchCountryCode)
			{
				var countryCode = port.Length >= 2 ? port.Substring(0, 2) : port;
				refZonePivotSubQuery.AddToFilter(RefZonePivotSchema.F2_ParentTableCode, RefCountrySchema.Constants.Prefix);
				var countryQuery = new ZDBOnlySubQuery(typeof(RefCountry), RefCountrySchema.PK);
				countryQuery.AddToFilter(RefCountrySchema.RN_Code, countryCode);
				refZonePivotSubQuery.AddSubQuery(RefZonePivotSchema.F2_ParentID, countryQuery, JoinCondition.And);
			}
			else
			{
				refZonePivotSubQuery.AddToFilter(RefZonePivotSchema.F2_ParentTableCode, RefUNLOCOSchema.Constants.Prefix);
				var portQuery = new ZDBOnlySubQuery(typeof(RefUNLOCO), RefUNLOCOSchema.PK);
				portQuery.AddToFilter(RefUNLOCOSchema.RL_Code, port);
				refZonePivotSubQuery.AddSubQuery(RefZonePivotSchema.F2_ParentID, portQuery, JoinCondition.And);
			}
		}

		#endregion

		#region GetValidZoneModes

		internal static IEnumerable<ZString> GetValidZoneModes(ZString transportMode, ZString containerMode)
		{
			transportMode = transportMode.ToUpper();
			containerMode = containerMode.ToUpper();

			var results = new List<ZString>() { RateMode.ALL };

			switch (transportMode)
			{
				case (TransportModes.Air):
					results.AddRange(GetAirZoneModes(containerMode));
					break;
				case (TransportModes.Sea):
					results.AddRange(GetSeaZoneModes(containerMode));
					break;
				case (TransportModes.Road):
					results.AddRange(GetRoadZoneModes(containerMode));
					break;
				case (TransportModes.Rail):
					results.AddRange(GetRailZoneModes(containerMode));
					break;
				case (TransportModes.SeaAir):
				case (TransportModes.AirSea):
					results.AddRange(GetAirZoneModes(containerMode));
					results.AddRange(GetSeaZoneModes(containerMode));
					break;
			}

			return results;
		}

		static IEnumerable<ZString> GetRailZoneModes(ZString containerMode)
		{
			yield return RateMode.RAI;

			if (containerMode == ContainerModes.FCL)
			{
				yield return RateMode.FRA;
			}
			if (containerMode == ContainerModes.LCL)
			{
				yield return RateMode.LRA;
			}
		}

		static IEnumerable<ZString> GetRoadZoneModes(ZString containerMode)
		{
			yield return RateMode.ROA;

			if (containerMode == ContainerModes.FCL)
			{
				yield return RateMode.FRO;
			}
			if (containerMode == ContainerModes.LCL)
			{
				yield return RateMode.LRO;
			}
			if (containerMode == ContainerModes.FTL)
			{
				yield return RateMode.FTL;
			}
		}

		static IEnumerable<ZString> GetAirZoneModes(ZString containerMode)
		{
			yield return RateMode.AIR;

			if (containerMode == ContainerModes.ULD)
			{
				yield return RateMode.ULD;
			}
			if (containerMode == ContainerModes.Loose)
			{
				yield return RateMode.LSE;
			}
		}

		static IEnumerable<ZString> GetSeaZoneModes(ZString containerMode)
		{
			yield return RateMode.SEA;

			if (containerMode == ContainerModes.FCL)
			{
				yield return RateMode.FCL;
			}
			if (containerMode == ContainerModes.LCL)
			{
				yield return RateMode.LCL;
			}
		}

		#endregion
	}

	struct GatewayDefaultSearchParameters
	{
		public BusinessObjectFactory Factory { get; set; }
		public ZString Load { get; set; }
		public ZString Discharge { get; set; }
		public ZString Origin { get; set; }
		public ZString Destination { get; set; }
		public IEnumerable<ZString> ZoneModes { get; set; }
	}

	class ForwarderPositionPair
	{
		public ForwarderPositionPair(OrgHeader org, Position position)
		{
			Position = position;
			Org = org;
		}

		internal Position Position { get; set; }
		internal OrgHeader Org { get; set; }
	}

	enum Position
	{
		Start,
		End
	}
}
