using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ShipmentGatewayDefaulterTest : TestCaseWithFactory
	{
		#region TestFindMatchingForwardersFromZones_OriginAndDestinationZones

		public void TestFindMatchingForwardersFromZones_OriginAndDestinationZones_FindsValidMatch()
		{
			CreateNewOrgHeaderWithOriginAndDestinationGateway(Factory, "GATEWAY1", "AUSYD", "AUBNE");
			CreateNewOrgHeaderWithOriginAndDestinationGateway(Factory, "GATEWAY2", "AUBNE", "AUSYD");

			var matches = ShipmentGatewayDefaulter.FindMatchingForwardersFromZones(Factory, "AUSYD", "AUBNE", false, false, new ZString[] { "ALL" });

			AssertEquals("Should return valid match", 1, matches.Count());
			AssertEquals("Org Code", "GATEWAY1", matches.First().Org.OH_Code);
			AssertEquals("Position", Position.Start, matches.First().Position);
		}

		public void TestFindMatchingForwardersFromZones_OriginAndDestinationZones_IgnoresOriginOnlyMatch()
		{
			CreateNewOrgHeaderWithOriginGateway(Factory, "GATEWAY1", "ALL", "AUSYD");

			var matches = ShipmentGatewayDefaulter.FindMatchingForwardersFromZones(Factory, "AUSYD", "AUBNE", false, false, new ZString[] { "ALL" });

			AssertNull("Should not return match as there is no destination matched", matches);
		}

		public void TestFindMatchingForwardersFromZones_OriginAndDestinationZones_IgnoresDestinationOnlyMatch()
		{
			CreateNewOrgHeaderWithDestinationGateway(Factory, "GATEWAY1", "ALL", "AUBNE");

			var matches = ShipmentGatewayDefaulter.FindMatchingForwardersFromZones(Factory, "AUSYD", "AUBNE", false, false, new ZString[] { "ALL" });

			AssertNull("Should not return match as there is no origin matched", matches);
		}

		public void TestFindMatchingForwardersFromZones_OriginAndDestinationZones_FindsMultipleMatches()
		{
			CreateNewOrgHeaderWithOriginAndDestinationGateway(Factory, "GATEWAY1", "AUSYD", "AUBNE");
			CreateNewOrgHeaderWithOriginAndDestinationGateway(Factory, "GATEWAY2", "AUSYD", "AUBNE");

			var matches = ShipmentGatewayDefaulter.FindMatchingForwardersFromZones(Factory, "AUSYD", "AUBNE", false, false, new ZString[] { "ALL" });

			AssertEquals("Should return both matches", 2, matches.Count());
		}

		#endregion

		#region TestFindMatchingForwardersFromZones_Origin

		public void TestFindMatchingForwardersFromZones_Origin_FindsValidMatch()
		{
			CreateNewOrgHeaderWithOriginGateway(Factory, "GATEWAY1", "ALL", "AUSYD");
			CreateNewOrgHeaderWithOriginGateway(Factory, "GATEWAY2", "ALL", "AUBNE");
			CreateNewOrgHeaderWithDestinationGateway(Factory, "GATEWAY3", "ALL", "AUSYD");

			var matches = ShipmentGatewayDefaulter.FindMatchingForwardersFromZones(Factory, "AUSYD", string.Empty, false, false, new ZString[] { "ALL" });

			AssertEquals("Should return valid match only", 1, matches.Count());
			AssertEquals("Org Code", "GATEWAY1", matches.First().Org.OH_Code);
			AssertEquals("Position", Position.Start, matches.First().Position);
		}

		public void TestFindMatchingForwardersFromZones_Origin_MultipleOrigins()
		{
			CreateNewOrgHeaderWithOriginGateway(Factory, "GATEWAY1", "ALL", "AUBNE");
			CreateNewOrgHeaderWithOriginGateway(Factory, "GATEWAY2", "ALL", "AUBNE");

			var matches = ShipmentGatewayDefaulter.FindBestMatchingForwarders(GetStandardTestParameters());

			AssertEquals("Should not return any match", 0, matches.Count());
		}

		public void TestFindMatchingForwardersFromZones_Origin_DestinationAndOrigin()
		{
			CreateNewOrgHeaderWithOriginAndDestinationGateway(Factory, "GATEWAY1", "AUSYD", "AUBNE");
			CreateNewOrgHeaderWithOriginGateway(Factory, "GATEWAY2", "ALL", "AUSYD");

			var matches = ShipmentGatewayDefaulter.FindMatchingForwardersFromZones(Factory, "AUSYD", string.Empty, false, false, new ZString[] { "ALL" });

			AssertEquals("Should return both matches", 2, matches.Count());
		}

		#endregion

		#region TestFindMatchingForwardersFromZones_Destination

		public void TestFindMatchingForwardersFromZones_Destination_FindsValidMatch()
		{
			CreateNewOrgHeaderWithDestinationGateway(Factory, "GATEWAY1", "ALL", "AUSYD");
			CreateNewOrgHeaderWithDestinationGateway(Factory, "GATEWAY2", "ALL", "AUBNE");
			CreateNewOrgHeaderWithOriginGateway(Factory, "GATEWAY3", "ALL", "AUSYD");

			var matches = ShipmentGatewayDefaulter.FindMatchingForwardersFromZones(Factory, string.Empty, "AUSYD", false, false, new ZString[] { "ALL" });

			AssertEquals("Should return valid match only", 1, matches.Count());
			AssertEquals("Org Code", "GATEWAY1", matches.First().Org.OH_Code);
			AssertEquals("Position", Position.End, matches.First().Position);
		}

		public void TestFindMatchingForwardersFromZones_Destination_MultipleDestinations()
		{
			CreateNewOrgHeaderWithDestinationGateway(Factory, "GATEWAY1", "ALL", "NZAKL");
			CreateNewOrgHeaderWithDestinationGateway(Factory, "GATEWAY2", "ALL", "NZAKL");

			var matches = ShipmentGatewayDefaulter.FindBestMatchingForwarders(GetStandardTestParameters());

			AssertEquals("Should not return any match", 0, matches.Count());
		}

		#endregion

		#region TestFindMatchingForwardersFromZones_OriginOrDestination

		public void TestFindMatchingForwardersFromZones_OriginOrDestination_FindsValidMatch()
		{
			CreateNewOrgHeaderWithOriginGateway(Factory, "GATEWAY1", "ALL", "AUBNE");
			CreateNewOrgHeaderWithDestinationGateway(Factory, "GATEWAY2", "ALL", "NZCHC");

			var matches = ShipmentGatewayDefaulter.FindBestMatchingForwarders(GetStandardTestParameters());

			AssertEquals("Should return a match", 2, matches.Count());

			var startGateWay = matches.First();
			var endGateWay = matches.ToList()[1];

			AssertEquals("Org Code", "GATEWAY1", startGateWay.Org.OH_Code);
			AssertEquals("Position", Position.Start, startGateWay.Position);

			AssertEquals("Org Code", "GATEWAY2", endGateWay.Org.OH_Code);
			AssertEquals("Position", Position.End, endGateWay.Position);
		}

		#endregion

		#region TestFindMatchingForwardersFromZones_MatchesCountryCode

		public void TestFindMatchingForwardersFromZones_MatchesCountryCode()
		{
			CreateNewOrgHeaderWithOriginGateway(Factory, "GATEWAY1", "ALL", "AU");

			var matches = ShipmentGatewayDefaulter.FindMatchingForwardersFromZones(Factory, "AUBNE", string.Empty, true, true, new ZString[] { "ALL" });

			AssertEquals("Should return valid match", 1, matches.Count());
			AssertEquals("Org Code", "GATEWAY1", matches.First().Org.OH_Code);
			AssertEquals("Position", Position.Start, matches.First().Position);
		}

		public void TestFindMatchingForwardersFromZones_MatchesPortAndCountryTogether()
		{
			CreateNewOrgHeaderWithOriginAndDestinationGateway(Factory, "GATEWAY1", "AUSYD", "NZ");

			var matches = ShipmentGatewayDefaulter.FindBestMatchingForwarders(GetStandardTestParameters());

			AssertEquals("Should return valid match", 1, matches.Count());
			AssertEquals("Org Code", "GATEWAY1", matches.First().Org.OH_Code);
			AssertEquals("Position", Position.Start, matches.First().Position);
		}

		public void TestFindMatchingForwardersFromZones_MatchesCountryAndPortTogether()
		{
			CreateNewOrgHeaderWithOriginAndDestinationGateway(Factory, "GATEWAY1", "AU", "NZAKL");
			var matches = ShipmentGatewayDefaulter.FindBestMatchingForwarders(GetStandardTestParameters());

			AssertEquals("Should return valid match", 1, matches.Count());
			AssertEquals("Org Code", "GATEWAY1", matches.First().Org.OH_Code);
			AssertEquals("Position", Position.Start, matches.First().Position);
		}

		public void TestFindMatchingForwardersFromZones_MatchesCountryCode_PrioritiesPorts()
		{
			CreateNewOrgHeaderWithOriginAndDestinationGateway(Factory, "GATEWAY1", "AU", "NZ");
			CreateNewOrgHeaderWithOriginAndDestinationGateway(Factory, "GATEWAY2", "AUSYD", "NZ");

			var matches = ShipmentGatewayDefaulter.FindBestMatchingForwarders(GetStandardTestParameters());

			AssertEquals("Should return port>country match", 1, matches.Count());
			AssertEquals("Org Code port>country", "GATEWAY2", matches.First().Org.OH_Code);
			AssertEquals("Position port>country", Position.Start, matches.First().Position);

			CreateNewOrgHeaderWithOriginAndDestinationGateway(Factory, "GATEWAY3", "AU", "NZAKL");

			matches = ShipmentGatewayDefaulter.FindBestMatchingForwarders(GetStandardTestParameters());

			AssertEquals("Should return country>port match", 1, matches.Count());
			AssertEquals("Org Code country>port", "GATEWAY3", matches.First().Org.OH_Code);
			AssertEquals("Position country>port", Position.Start, matches.First().Position);

			CreateNewOrgHeaderWithOriginAndDestinationGateway(Factory, "GATEWAY4", "AUSYD", "NZAKL");

			matches = ShipmentGatewayDefaulter.FindBestMatchingForwarders(GetStandardTestParameters());

			AssertEquals("Should return port>port match", 1, matches.Count());
			AssertEquals("Org Code port>port", "GATEWAY4", matches.First().Org.OH_Code);
			AssertEquals("Position port>port", Position.Start, matches.First().Position);
		}

		public void TestFindMatchingForwardersFromZones_MatchesCountryCode_PrioritiesMultiCountryOverSinglePort()
		{
			CreateNewOrgHeaderWithOriginAndDestinationGateway(Factory, "GATEWAY1", "AU", "NZ");
			CreateNewOrgHeaderWithOriginGateway(Factory, "GATEWAY2", "ALL", "AUSYD");

			var matches = ShipmentGatewayDefaulter.FindBestMatchingForwarders(GetStandardTestParameters());

			AssertEquals("Should return valid match", 1, matches.Count());
			AssertEquals("Org Code", "GATEWAY1", matches.First().Org.OH_Code);
			AssertEquals("Position", Position.Start, matches.First().Position);
		}

		#endregion

		#region TestFindMatchingForwarderFromZones_TransportMode

		public void TestFindMatchingForwardersFromZones_MatchesCorrectMode()
		{
			CreateNewOrgHeaderWithOriginGateway(Factory, "GATEWAY1", "SEA", "AUSYD");
			CreateNewOrgHeaderWithOriginGateway(Factory, "GATEWAY2", "AIR", "AUSYD");

			var matches = ShipmentGatewayDefaulter.FindBestMatchingForwarders(GetSeaTestParameters());

			AssertEquals("Should return valid match", 1, matches.Count());
			AssertEquals("Org Code", "GATEWAY1", matches.First().Org.OH_Code);
			AssertEquals("Position", Position.Start, matches.First().Position);
		}

		public void TestGetValidZoneModes()
		{
			var matches = ShipmentGatewayDefaulter.GetValidZoneModes("SEA", "FCL");

			AssertEquals("matches.Count", 3, matches.Count());
			Assert("FCL", matches.Any(z => z == "FCL"));
			Assert("SEA", matches.Any(z => z == "SEA"));
			Assert("ALL", matches.Any(z => z == "ALL"));
		}

		public void TestFindMatchingForwardersFromZones_IncorrectTransportModeNotAdded()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDischargePort = "NZAKL";
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_TransportMode = TransportModes.Sea;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "GATEWAY1";

			CreateNewOriginGatewayZone(Factory, org, "ALL", "AUBNE");
			var agentPort = CreateAppointedAgents(Factory, org, "AUBNE", TransportModes.Air);

			shipment.UpdateGateways();

			AssertEquals("Gateway is added to shipment", 1, shipment.Gateways.Count);
			AssertEquals("Main address is used", org.MainAddress.PK, shipment.Gateways[0].JSG_OA_ForwarderAddress);
		}

		public void TestFindMatchingForwardersFromZones_CorrectTransportModeIsAdded()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDischargePort = "NZAKL";
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_TransportMode = TransportModes.Sea;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "GATEWAY1";

			CreateNewOriginGatewayZone(Factory, org, "ALL", "AUBNE");
			var agentPort = CreateAppointedAgents(Factory, org, "AUBNE", TransportModes.Sea);

			shipment.UpdateGateways();

			AssertEquals("Gateway is added to shipment", 1, shipment.Gateways.Count);
			AssertEquals("gateway from default", agentPort.O5_OA_AgentOfficeAddress, shipment.Gateways[0].JSG_OA_ForwarderAddress);
		}

		#endregion

		#region TestFindMatchingForwarders_CorrectOrder

		public void TestFindMatchingForwarders_CorrectOrder_OriginAndDestination()
		{
			// Planned Load = AUSYD
			// Planned Discharge = NZAKL
			// Origin = AUBNE
			// Destination = NZCHC

			CreateNewOrgHeaderWithOriginAndDestinationGateway(Factory, "GATEWAY1", "AUBNE", "NZCHC");

			var matches = ShipmentGatewayDefaulter.FindBestMatchingForwarders(GetStandardTestParameters());

			AssertEquals("Should return single match for origin-destination", 1, matches.Count());
			AssertEquals("OrgCode: origin-destination", "GATEWAY1", matches.First().Org.OH_Code);
			AssertEquals("Position: origin-destination", Position.Start, matches.First().Position);

			CreateNewOrgHeaderWithOriginAndDestinationGateway(Factory, "GATEWAY2", "AUSYD", "NZCHC");

			matches = ShipmentGatewayDefaulter.FindBestMatchingForwarders(GetStandardTestParameters());

			AssertEquals("Should return single match for planned load-destination", 1, matches.Count());
			AssertEquals("OrgCode: planned load-destination", "GATEWAY2", matches.First().Org.OH_Code);
			AssertEquals("Position: planned load-destination", Position.Start, matches.First().Position);

			CreateNewOrgHeaderWithOriginAndDestinationGateway(Factory, "GATEWAY3", "AUBNE", "NZAKL");

			matches = ShipmentGatewayDefaulter.FindBestMatchingForwarders(GetStandardTestParameters());

			AssertEquals("Should return single match for origin-planned discharge", 1, matches.Count());
			AssertEquals("OrgCode: origin-planned discharge", "GATEWAY3", matches.First().Org.OH_Code);
			AssertEquals("Position: origin-planned discharge", Position.Start, matches.First().Position);

			CreateNewOrgHeaderWithOriginAndDestinationGateway(Factory, "GATEWAY4", "AUSYD", "NZAKL");

			matches = ShipmentGatewayDefaulter.FindBestMatchingForwarders(GetStandardTestParameters());

			AssertEquals("Should return single match for planned load-planned discharge", 1, matches.Count());
			AssertEquals("OrgCode: planned load-planned discharge", "GATEWAY4", matches.First().Org.OH_Code);
			AssertEquals("Position: planned load-planned discharge", Position.Start, matches.First().Position);
		}

		public void TestFindMatchingForwarders_CorrectOrder_OriginOnly()
		{
			// Planned Load = AUSYD
			// Origin = AUBNE

			CreateNewOrgHeaderWithOriginGateway(Factory, "GATEWAY1", "ALL", "AUBNE");

			var matches = ShipmentGatewayDefaulter.FindBestMatchingForwarders(GetStandardTestParameters());

			AssertEquals("Should return single match for origin", 1, matches.Count());
			AssertEquals("Org Code: origin", "GATEWAY1", matches.First().Org.OH_Code);
			AssertEquals("Position: origin", Position.Start, matches.First().Position);

			CreateNewOrgHeaderWithOriginGateway(Factory, "GATEWAY2", "ALL", "AUSYD");

			matches = ShipmentGatewayDefaulter.FindBestMatchingForwarders(GetStandardTestParameters());

			AssertEquals("Should return single match for planned load", 1, matches.Count());
			AssertEquals("OrgCode: planned load", "GATEWAY2", matches.First().Org.OH_Code);
			AssertEquals("Position: planned load", Position.Start, matches.First().Position);
		}

		public void TestFindMatchingForwarders_CorrectOrder_DestinationOnly()
		{
			// Planned Load = NZAKL
			// Destination = NZCHC

			CreateNewOrgHeaderWithDestinationGateway(Factory, "GATEWAY1", "ALL", "NZCHC");

			var matches = ShipmentGatewayDefaulter.FindBestMatchingForwarders(GetStandardTestParameters());

			AssertEquals("Should return single match for destination", 1, matches.Count());
			AssertEquals("Org Code: destination", "GATEWAY1", matches.First().Org.OH_Code);
			AssertEquals("Position: destination", Position.End, matches.First().Position);

			CreateNewOrgHeaderWithDestinationGateway(Factory, "GATEWAY2", "ALL", "NZAKL");

			matches = ShipmentGatewayDefaulter.FindBestMatchingForwarders(GetStandardTestParameters());

			AssertEquals("Should return single match for planned load", 1, matches.Count());
			AssertEquals("OrgCode: planned load", "GATEWAY2", matches.First().Org.OH_Code);
			AssertEquals("Position: planned load", Position.End, matches.First().Position);
		}

		public void TestFindMatchingForwarders_CorrectOrder_OriginAndDestinationGatewaysTakePriority()
		{
			// Planned Load = AUSYD
			// Planned Discharge = NZAKL
			// Origin = AUBNE
			// Destination = NZCHC

			CreateNewOrgHeaderWithOriginAndDestinationGateway(Factory, "GATEWAY1", "AUBNE", "NZCHC");
			CreateNewOrgHeaderWithOriginGateway(Factory, "GATEWAY2", "ALL", "AUSYD");
			CreateNewOrgHeaderWithDestinationGateway(Factory, "GATEWAY3", "ALL", "NZAKL");

			var matches = ShipmentGatewayDefaulter.FindBestMatchingForwarders(GetStandardTestParameters());

			AssertEquals("Should return single match", 1, matches.Count());
			AssertEquals("Gateway with both origin and destination should match", "GATEWAY1", matches.First().Org.OH_Code);
			AssertEquals("Position:", Position.Start, matches.First().Position);
		}

		public void TestFindMatchingForwarders_CanReturnTwoMatches()
		{
			// Planned Load = AUSYD
			// Planned Discharge = NZAKL

			CreateNewOrgHeaderWithOriginGateway(Factory, "GATEWAY1", "ALL", "AUSYD");
			CreateNewOrgHeaderWithDestinationGateway(Factory, "GATEWAY2", "ALL", "NZAKL");

			var matches = ShipmentGatewayDefaulter.FindBestMatchingForwarders(GetStandardTestParameters());

			AssertEquals("Should return both matches", 2, matches.Count());

			var originMatch = matches.FirstOrDefault(m => m.Org.OH_Code == "GATEWAY1");
			AssertNotNull("Should have found GATEWAY1", originMatch);
			AssertEquals("origin gateway Position", Position.Start, originMatch.Position);

			var destinationMatch = matches.FirstOrDefault(m => m.Org.OH_Code == "GATEWAY2");
			AssertNotNull("Should have found GATEWAY2", destinationMatch);
			AssertEquals("destination gateway Position", Position.End, destinationMatch.Position);
		}

		#endregion

		#region TestUpdateGateways_Direction

		public void TestUpdateGateways_ExportWithCorrectDirection()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDischargePort = "NZAKL";
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "NZCHC";
			shipment.JS_TransportMode = TransportModes.Air;

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "GATEWAY1";

			CreateNewOriginGatewayZone(Factory, orgHeader, "ALL", "AUBNE");
			var agentPort = CreateAppointedAgents(Factory, orgHeader, "AUBNE");
			agentPort.O5_AgentDirection = "EXP";

			shipment.UpdateGateways();

			AssertEquals("expected one gateway", 1, shipment.Gateways.Count);
			AssertNotEquals("Expected a gateway agent port", agentPort.PK, shipment.Gateways.First().JSG_OA_ForwarderAddress);
			AssertEquals("expected main address", orgHeader.AppointedGatewayAgentPorts.Cast<OrgAppointedAgentPorts>().First().O5_OA_AgentOfficeAddress, shipment.Gateways.First().JSG_OA_ForwarderAddress);
		}

		public void TestUpdateGateways_ExportWithIncorrectDirection()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDischargePort = "NZAKL";
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "NZCHC";
			shipment.JS_TransportMode = TransportModes.Air;

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "GATEWAY1";

			CreateNewOriginGatewayZone(Factory, orgHeader, "ALL", "AUBNE");
			var agentPort = CreateAppointedAgents(Factory, orgHeader, "AUBNE");
			agentPort.O5_AgentDirection = "IMP";

			shipment.UpdateGateways();

			AssertEquals("expected one gateway", 1, shipment.Gateways.Count);
			AssertNotEquals("Expected a gateway agent port", agentPort.PK, shipment.Gateways.First().JSG_OA_ForwarderAddress);
			AssertEquals("expected main address", orgHeader.MainAddress.PK, shipment.Gateways.First().JSG_OA_ForwarderAddress);
		}

		public void TestUpdateGateways_ImportWithCorrectDirection()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDischargePort = "NZAKL";
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "NZCHC";
			shipment.JS_TransportMode = TransportModes.Air;

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "GATEWAY1";

			CreateNewDestinationGatewayZone(Factory, orgHeader, "ALL", "NZCHC");
			var agentPort = CreateAppointedAgents(Factory, orgHeader, "NZCHC");
			agentPort.O5_AgentDirection = "IMP";

			shipment.UpdateGateways();

			AssertEquals("expected one gateway", 1, shipment.Gateways.Count);
			AssertNotEquals("Expected a gateway agent port", agentPort.PK, shipment.Gateways.First().JSG_OA_ForwarderAddress);
			AssertEquals("Expected shipment gateway to use office address", orgHeader.AppointedGatewayAgentPorts.Cast<OrgAppointedAgentPorts>().First().O5_OA_AgentOfficeAddress, shipment.Gateways.First().JSG_OA_ForwarderAddress);
		}

		public void TestUpdateGateways_ImportWithIncorrectDirection()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDischargePort = "NZAKL";
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "NZCHC";
			shipment.JS_TransportMode = TransportModes.Air;

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "GATEWAY1";

			CreateNewDestinationGatewayZone(Factory, orgHeader, "ALL", "NZCHC");
			var agentPort = CreateAppointedAgents(Factory, orgHeader, "NZCHC");
			agentPort.O5_AgentDirection = "EXP";

			shipment.UpdateGateways();

			AssertEquals("expected one gateway", 1, shipment.Gateways.Count);
			AssertNotEquals("Expected a gateway agent port", agentPort.PK, shipment.Gateways.First().JSG_OA_ForwarderAddress);
			AssertEquals("Expected shipment gateway to use office address", orgHeader.MainAddress.PK, shipment.Gateways.First().JSG_OA_ForwarderAddress);
		}

		public void TestUpdateGateways_Both()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDischargePort = "NZAKL";
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "NZCHC";
			shipment.JS_TransportMode = TransportModes.Air;

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "GATEWAY1";

			CreateNewOriginGatewayZone(Factory, orgHeader, "ALL", "AUBNE");
			var agentPort = CreateAppointedAgents(Factory, orgHeader, "AUBNE");
			agentPort.O5_AgentDirection = "BTH";

			shipment.UpdateGateways();

			AssertEquals("Precondition: a appointed agents", 1, orgHeader.AppointedGatewayAgentPorts.Count);

			AssertEquals("expected one gateway", 1, shipment.Gateways.Count);
			AssertNotEquals("Expected a gateway agent port", agentPort.PK, shipment.Gateways.First().JSG_OA_ForwarderAddress);
			AssertEquals("Expected shipment gateway to use office address", orgHeader.AppointedGatewayAgentPorts.Cast<OrgAppointedAgentPorts>().First().O5_OA_AgentOfficeAddress, shipment.Gateways.First().JSG_OA_ForwarderAddress);
		}

		#endregion

		#region TestUpdateDefaultGatewayShipmentFromInternationalZones_DeletesExistingGatewaysIfNeeded

		public void TestUpdateDefaultGatewayShipmentFromInternationalZones_AddsNewZone()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";
			CreateNewOrgHeaderWithOriginGateway(Factory, "GATEWAY1", "ALL", "AUSYD");
			CreateNewOrgHeaderWithOriginGateway(Factory, "GATEWAY2", "ALL", "AUBNE");

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "GATEWAY3";

			var agentPort = CreateAppointedAgents(Factory, org, "AUBNE");

			var manualGateway = Factory.New<ShipmentGateway>();
			manualGateway.JSG_OA_ForwarderAddress = agentPort.O5_OA_AgentOfficeAddress;
			shipment.Gateways.Add(manualGateway);

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUMEL";
			Factory.Save();

			AssertEquals("PreCondition: Gateway got added to shipment", 2, shipment.Gateways.Count);
			AssertEquals("PreCondition: gateway from default", "GATEWAY1", shipment.Gateways[0].ForwarderAddress.Header.OH_Code);
			AssertEquals("PreCondition: manually added gateway is on shipment", "GATEWAY3", shipment.Gateways[1].ForwarderAddress.Header.OH_Code);

			shipment.JS_RL_NKOrigin = "AUBNE";
			Factory.Save();

			AssertEquals("Gateways.Count", 3, shipment.Gateways.Count);
			AssertEquals("Defaulted Gateway has been updated", "GATEWAY2", shipment.Gateways[0].ForwarderAddress.Header.OH_Code);
			AssertEquals("Existing Default still on gateway", "GATEWAY1", shipment.Gateways[1].ForwarderAddress.Header.OH_Code);
			AssertEquals("manually added gateway is still on shipment", "GATEWAY3", shipment.Gateways[2].ForwarderAddress.Header.OH_Code);
		}

		public void TestUpdateDefaultGatewayShipmentFromInternationalZones_AddsExistingOriginAndDestinationZone()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";
			CreateNewOrgHeaderWithOriginAndDestinationGateway(Factory, "GATEWAY1", "AUSYD", "NZAKL");
			CreateNewOrgHeaderWithOriginGateway(Factory, "GATEWAY2", "ALL", "AUBNE");
			CreateNewOrgHeaderWithDestinationGateway(Factory, "GATEWAY3", "ALL", "NZCHC");

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "GATEWAY4";
			var agentPort = CreateAppointedAgents(Factory, org, "AUBNE");

			var manualGateway = Factory.New<ShipmentGateway>();
			manualGateway.JSG_OA_ForwarderAddress = agentPort.O5_OA_AgentOfficeAddress;
			shipment.Gateways.Add(manualGateway);

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			Factory.Save();

			AssertEquals("PreCondition: Gateway got added to shipment", 2, shipment.Gateways.Count);
			AssertEquals("PreCondition: gateway from default", "GATEWAY1", shipment.Gateways[0].ForwarderAddress.Header.OH_Code);
			AssertEquals("PreCondition: manually added gateway is on shipment", "GATEWAY4", shipment.Gateways[1].ForwarderAddress.Header.OH_Code);

			shipment.JS_RL_NKDestination = "NZCHC";
			shipment.JS_RL_NKOrigin = "AUBNE";
			Factory.Save();

			AssertEquals("Gateways.Count", 4, shipment.Gateways.Count);
			AssertEquals("Defaulted origin Gateway has been updated", "GATEWAY2", shipment.Gateways[0].ForwarderAddress.Header.OH_Code);
			AssertEquals("Existing Defaulted gateway is still on shipment", "GATEWAY1", shipment.Gateways[1].ForwarderAddress.Header.OH_Code);
			AssertEquals("manually added gateway is still on shipment", "GATEWAY4", shipment.Gateways[2].ForwarderAddress.Header.OH_Code);
			AssertEquals("Defaulted destination Gateway has been updated", "GATEWAY3", shipment.Gateways[3].ForwarderAddress.Header.OH_Code);
		}

		public void TestUpdateDefaultGatewayShipmentFromInternationalZones_AddsBothOriginAndDestinationGateways()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";
			CreateNewOrgHeaderWithOriginAndDestinationGateway(Factory, "GATEWAY1", "AUSYD", "NZAKL");
			CreateNewOrgHeaderWithOriginGateway(Factory, "GATEWAY2", "ALL", "AUBNE");
			CreateNewOrgHeaderWithDestinationGateway(Factory, "GATEWAY3", "ALL", "NZCHC");

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "GATEWAY4";

			var agentPort = CreateAppointedAgents(Factory, org, "AUBNE");

			var manualGateway = Factory.New<ShipmentGateway>();
			manualGateway.JSG_OA_ForwarderAddress = agentPort.O5_OA_AgentOfficeAddress;
			shipment.Gateways.Add(manualGateway);

			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "NZCHC";
			Factory.Save();

			AssertEquals("PreCondition: Gateways defaulted onto shipment", 3, shipment.Gateways.Count);
			AssertEquals("PreCondition: First gateway from default", "GATEWAY2", shipment.Gateways[0].ForwarderAddress.Header.OH_Code);
			AssertEquals("PreCondition: manually added gateway is on shipment", "GATEWAY4", shipment.Gateways[1].ForwarderAddress.Header.OH_Code);
			AssertEquals("PreCondition: Last gateway from default", "GATEWAY3", shipment.Gateways[2].ForwarderAddress.Header.OH_Code);

			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_RL_NKOrigin = "AUSYD";
			Factory.Save();

			AssertEquals("Gateways.Count", 4, shipment.Gateways.Count);
			AssertEquals("Defaulted Gateway has been updated", "GATEWAY1", shipment.Gateways[0].ForwarderAddress.Header.OH_Code);
			AssertEquals("Existing Origin Default is still on shipment", "GATEWAY2", shipment.Gateways[1].ForwarderAddress.Header.OH_Code);
			AssertEquals("manually added gateway is still on shipment", "GATEWAY4", shipment.Gateways[2].ForwarderAddress.Header.OH_Code);
			AssertEquals("Existing Dest Default is still on shipment", "GATEWAY3", shipment.Gateways[3].ForwarderAddress.Header.OH_Code);
		}

		#endregion

		#region TestAgentOfficeAddressUsed

		public void TestAgentOfficeAddressUsed()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDischargePort = "NZAKL";
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "NZCHC";
			shipment.JS_TransportMode = TransportModes.Air;

			var orgHeader = CreateNewOrgHeaderWithOriginGateway(Factory, "GATEWAY1", "ALL", "AUBNE");
			shipment.UpdateGateways();

			AssertEquals("expected one gateway", 1, shipment.Gateways.Count);
			AssertEquals("expected one appointed agent", 1, orgHeader.AppointedGatewayAgentPorts.Count);
			AssertEquals("Expected shipment gateway to use office address", orgHeader.AppointedGatewayAgentPorts.Cast<OrgAppointedAgentPorts>().First().O5_OA_AgentOfficeAddress, shipment.Gateways.First().JSG_OA_ForwarderAddress);
		}

		#endregion

		#region TestUpdateGateways_Defaults

		public void TestUpdateGateways_DefaultsMainAddress()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDischargePort = "NZAKL";
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "NZCHC";
			shipment.JS_TransportMode = TransportModes.Air;

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "GATEWAY1";
			CreateNewOriginGatewayZone(Factory, orgHeader, "ALL", "AUBNE");

			shipment.UpdateGateways();

			AssertEquals("Precondition no appointed agents", 0, orgHeader.AppointedGatewayAgentPorts.Count);
			AssertEquals("expected one gateway", 1, shipment.Gateways.Count);
			AssertEquals("Expected shipment gateway to use office address", orgHeader.MainAddress.PK, shipment.Gateways.First().JSG_OA_ForwarderAddress);
		}

		public void TestGatewayAppointedAddressesOnly()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDischargePort = "NZAKL";
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "NZCHC";
			shipment.JS_TransportMode = TransportModes.Air;

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "GATEWAY1";

			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_Address1 = "AUBNE - asdf";
			orgAddress.OA_OH = orgHeader.PK;

			var agentPort = orgHeader.AppointedAgentPorts.AddNew();
			agentPort.O5_PortOrCountry = "AUBNE";
			agentPort.O5_OA_AgentOfficeAddress = orgAddress.PK;

			CreateNewOriginGatewayZone(Factory, orgHeader, "ALL", "AUBNE");
			CreateAppointedAgents(Factory, orgHeader, "AUBNE");

			shipment.UpdateGateways();

			AssertEquals("Precondition: 2 appointed agents", 1, orgHeader.AppointedAgentPorts.Count);
			AssertEquals("Precondition: a appointed agents", 1, orgHeader.AppointedGatewayAgentPorts.Count);

			AssertEquals("expected one gateway", 1, shipment.Gateways.Count);
			AssertNotEquals("Expected a gateway agent port", agentPort.PK, shipment.Gateways.First().JSG_OA_ForwarderAddress);
			AssertEquals("Expected shipment gateway to use office address", orgHeader.AppointedGatewayAgentPorts.Cast<OrgAppointedAgentPorts>().First().O5_OA_AgentOfficeAddress, shipment.Gateways.First().JSG_OA_ForwarderAddress);
		}

		#endregion

		#region TestDefaultingIgnoresInactiveZones

		public void TestDefaultingIgnoresInactiveZones()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "GATEWAY1";
			var zone = Factory.New<RefZoneHeader>();
			zone.FZ_OH_RelatedParty = org.PK;
			zone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.OriginGateway;
			zone.FZ_Code = "GATO";
			zone.FZ_Description = "GATEWAY ORIGIN ZONE";
			zone.FZ_ZoneMode = "ALL";
			zone.FZ_IsActive = false;

			var unloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			zone.UNLOCOs.Add(unloco);

			Factory.Save();

			AssertNull("shouldn't match an inactive gateway", ShipmentGatewayDefaulter.FindMatchingForwardersFromZones(Factory, "AUSYD", "AUBNE", false, false, new ZString[] { "ALL" }));
		}

		#endregion

		#region TestHasRequiredPortsToDefault

		public void TestHasRequiredPortsToDefaultWithDeletedShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDischargePort = "NZAKL";
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_TransportMode = TransportModes.Sea;

			shipment.Delete();
			shipment.ReorderGateways();
			AssertNoExceptionThrown("Accessing property on a deleted shipment in HasRequiredPortsToDefault", () => shipment.ReorderGateways());
		}

		#endregion

		#region Create Methods

		static OrgAppointedAgentPorts CreateAppointedAgents(BusinessObjectFactory factory, OrgHeader orgHeader, ZString portOrCountry, string transportStatus = "AIR")
		{
			var orgAddress = factory.New<OrgAddress>();
			orgAddress.OA_Address1 = portOrCountry + " asdf";
			orgAddress.OA_OH = orgHeader.PK;

			var agentPort = orgHeader.AppointedGatewayAgentPorts.AddNew();
			agentPort.O5_PortOrCountry = portOrCountry;
			agentPort.O5_OA_AgentOfficeAddress = orgAddress.PK;

			switch (transportStatus)
			{
				case TransportModes.Air:
					agentPort.O5_AirAgentStatus = GatewayPreviousSendingAgent.Codes.GatewayAgentWithFT;
					break;

				case TransportModes.Sea:
					agentPort.O5_SeaAgentStatus = GatewayPreviousSendingAgent.Codes.GatewayAgentWithFT;
					break;

				case TransportModes.Rail:
					agentPort.O5_RailAgentStatus = GatewayPreviousSendingAgent.Codes.GatewayAgentWithFT;
					break;
			}

			return agentPort;
		}

		internal static OrgHeader CreateNewOrgHeaderWithOriginGateway(BusinessObjectFactory factory, string code, string mode, params string[] origins)
		{
			var org = factory.New<OrgHeader>();
			org.OH_Code = code;

			CreateNewOriginGatewayZone(factory, org, mode, origins);
			foreach (var origin in origins)
			{
				CreateAppointedAgents(factory, org, origin);
			}

			return org;
		}

		internal static OrgHeader CreateNewOrgHeaderWithDestinationGateway(BusinessObjectFactory factory, string code, string mode, params string[] destinations)
		{
			var org = factory.New<OrgHeader>();
			org.OH_Code = code;

			CreateNewDestinationGatewayZone(factory, org, mode, destinations);
			foreach (var destination in destinations)
			{
				CreateAppointedAgents(factory, org, destination);
			}

			return org;
		}

		internal static OrgHeader CreateNewOrgHeaderWithOriginAndDestinationGateway(BusinessObjectFactory factory, string code, string origin, string destination, string mode = "ALL")
		{
			var org = factory.New<OrgHeader>();
			org.OH_Code = code;

			CreateNewOriginGatewayZone(factory, org, mode, origin);
			CreateNewDestinationGatewayZone(factory, org, mode, destination);
			CreateAppointedAgents(factory, org, origin);
			CreateAppointedAgents(factory, org, destination);

			return org;
		}

		static RefZoneHeader CreateNewOriginGatewayZone(BusinessObjectFactory factory, OrgHeader forwarder, string mode, params string[] ports)
		{
			return CreateNewGatewayZone(factory, forwarder, isOrigin: true, mode, ports);
		}

		static RefZoneHeader CreateNewDestinationGatewayZone(BusinessObjectFactory factory, OrgHeader forwarder, string mode, params string[] ports)
		{
			return CreateNewGatewayZone(factory, forwarder, isOrigin: false, mode, ports);
		}

		static RefZoneHeader CreateNewGatewayZone(BusinessObjectFactory factory, OrgHeader forwarder, bool isOrigin, string mode, params string[] locations)
		{
			var zone = factory.New<RefZoneHeader>();
			zone.FZ_OH_RelatedParty = forwarder.PK;
			zone.FZ_ZoneType = isOrigin
				? RefZoneHeaderLookups.ZoneTypeCodes.OriginGateway
				: RefZoneHeaderLookups.ZoneTypeCodes.DestinationGateway;
			zone.FZ_Code = zone.FZ_ZoneType + forwarder.OH_Code.Substring(forwarder.OH_Code.Length - 1, 1);
			zone.FZ_Description = zone.FZ_Code;
			zone.FZ_ZoneMode = mode;

			foreach (var location in locations)
			{
				if (location.Length == 5)
				{
					var unloco = factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, location);
					zone.UNLOCOs.Add(unloco);
				}
				else if (location.Length == 2)
				{
					var country = factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, location);
					zone.Countries.Add(country);
				}
			}

			factory.Save();

			return zone;
		}

		public GatewayDefaultSearchParameters GetStandardTestParameters()
		{
			return new GatewayDefaultSearchParameters
			{
				Factory = Factory,
				Load = "AUSYD",
				Discharge = "NZAKL",
				Origin = "AUBNE",
				Destination = "NZCHC",
				ZoneModes = new ZString[] { "ALL" }
			};
		}

		public GatewayDefaultSearchParameters GetSeaTestParameters()
		{
			var parameters = GetStandardTestParameters();
			parameters.ZoneModes = new ZString[] { "SEA" };
			return parameters;
		}
		#endregion
	}
}
