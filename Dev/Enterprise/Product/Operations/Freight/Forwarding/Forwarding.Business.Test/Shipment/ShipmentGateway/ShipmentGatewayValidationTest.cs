using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ShipmentGatewayValidationTest : BusinessObjectValidationTestCase
	{
		#region JSG_OA_ForwarderAddress

		public void TestValidateConsistencyWithConsolGatewayAgents_NotMatch()
		{
			var expectedError = "This Gateway does not match any of the Gateway Agents on attached Consol/s.";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			var sendingGatewayPorts = sendingForwarder.AppointedGatewayAgentPorts.AddNew();
			sendingGatewayPorts.O5_PortOrCountry = "AU";
			sendingGatewayPorts.O5_RoadAgentStatus = "GTT";
			sendingGatewayPorts.O5_OA_AgentOfficeAddress = sendingForwarder.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			var receivingGatewayPorts = receivingForwarder.AppointedGatewayAgentPorts.AddNew();
			receivingGatewayPorts.O5_PortOrCountry = "NZ";
			receivingGatewayPorts.O5_RoadAgentStatus = "GTT";
			receivingGatewayPorts.O5_OA_AgentOfficeAddress = receivingForwarder.MainAddress.PK;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			consol.Shipments.Add(shipment);
			AssertEquals("Gateways count", 2, shipment.Gateways.Count);

			var gateway = shipment.Gateways.AddNew();
			var agent = Factory.NewWithValidTestData<OrgHeader>();
			gateway.JSG_OA_ForwarderAddress = agent.MainAddress.PK;
			AssertHasWarning("Gateway: Not match the gateway agents from linked consol", gateway.JSG_OA_ForwarderAddressInfo, expectedError);

			var gateway1 = shipment.Gateways.AddNew();
			var agent1 = sendingForwarder;
			gateway1.JSG_OA_ForwarderAddress = agent1.MainAddress.PK;
			AssertNoWarning("Gateway: Match the gateway agents from linked consol", gateway1.JSG_OA_ForwarderAddressInfo, expectedError);
		}

		public void TestValidateConsistencyWithConsolGatewayAgents_WrongOrder_ConsolGateways()
		{
			var expectedError = "Gateways must be listed in their correct order from origin to destination. This Gateway is out of order; please check the order of attached Consols and their Sending and Receiving Gateway Agents.";
			var shipment = IniitializeShipmentGateways();
			var gateway1 = shipment.Gateways[0];
			var gateway2 = shipment.Gateways[1];
			var gateway3 = shipment.Gateways[2];
			var gateway4 = shipment.Gateways[2];

			#region ReceivingForwarder should be after SendingForwarder in order

			shipment.Gateways.SwapGateways(1, 2);
			AssertHasError("Gateway: has order error when ReceivingForwarder is before SendingForwarder", gateway1.JSG_OA_ForwarderAddressInfo, expectedError);

			shipment.Gateways.SwapGateways(1, 2);
			AssertNoError("Gateway: no order error", gateway1.JSG_OA_ForwarderAddressInfo, expectedError);
			AssertNoError("Gateway: no order error", gateway2.JSG_OA_ForwarderAddressInfo, expectedError);
			AssertNoError("Gateway: no order error", gateway3.JSG_OA_ForwarderAddressInfo, expectedError);

			#endregion

			#region Consols gateway agents should follow consol order

			shipment.Gateways.SwapGateways(2, 3);
			AssertHasError("Gateway: has order error when consol agent order is not listed in the order of Consols", gateway2.JSG_OA_ForwarderAddressInfo, expectedError);

			shipment.Gateways.SwapGateways(3, 2);
			AssertNoError("Gateway: no order error", gateway1.JSG_OA_ForwarderAddressInfo, expectedError);
			AssertNoError("Gateway: no order error", gateway2.JSG_OA_ForwarderAddressInfo, expectedError);
			AssertNoError("Gateway: no order error", gateway3.JSG_OA_ForwarderAddressInfo, expectedError);

			#endregion
		}

		public void TestValidateConsistencyWithConsolGatewayAgents_WrongOrder_ManualGateways()
		{
			var expectedError = "Gateways must be listed in their correct order from origin to destination. This Gateway is out of order; please check the order of attached Consols and their Sending and Receiving Gateway Agents.";
			var shipment = IniitializeShipmentGateways();
			var gateway1 = shipment.Gateways[0];
			var gateway2 = shipment.Gateways[1];
			var gateway3 = shipment.Gateways[2];
			var gateway4 = shipment.Gateways[3];

			#region Manually added gateway should not be added between ReceivingForwarder and SendingForwarder

			shipment.Gateways.SwapGateways(2, 4);
			AssertHasError("Gateway: has order error when manually adding a gateway between SendingForwarder and ReceivingForwarder", gateway2.JSG_OA_ForwarderAddressInfo, expectedError);

			shipment.Gateways.SwapGateways(4, 2);
			AssertNoError("Gateway: no order error", gateway1.JSG_OA_ForwarderAddressInfo, expectedError);
			AssertNoError("Gateway: no order error", gateway2.JSG_OA_ForwarderAddressInfo, expectedError);
			AssertNoError("Gateway: no order error", gateway3.JSG_OA_ForwarderAddressInfo, expectedError);
			AssertNoError("Gateway: no order error", gateway4.JSG_OA_ForwarderAddressInfo, expectedError);

			shipment.Gateways.SwapGateways(1, 2);
			shipment.Gateways.SwapGateways(2, 4);
			AssertHasError("Gateway: has order error when manually adding a gateway between ReceivingForwarder and SendingForwarder", gateway2.JSG_OA_ForwarderAddressInfo, expectedError);

			#endregion
		}

		ForwardingShipment IniitializeShipmentGateways()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			var sendingGatewayPorts = sendingForwarder.AppointedGatewayAgentPorts.AddNew();
			sendingGatewayPorts.O5_PortOrCountry = "AU";
			sendingGatewayPorts.O5_RoadAgentStatus = "GTT";
			sendingGatewayPorts.O5_OA_AgentOfficeAddress = sendingForwarder.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			var receivingGatewayPorts = receivingForwarder.AppointedGatewayAgentPorts.AddNew();
			receivingGatewayPorts.O5_PortOrCountry = "NZ";
			receivingGatewayPorts.O5_RoadAgentStatus = "GTT";
			receivingGatewayPorts.O5_OA_AgentOfficeAddress = receivingForwarder.MainAddress.PK;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			consol.Shipments.Add(shipment);

			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();

			var sendingForwarder2 = Factory.NewWithValidTestData<OrgHeader>();
			var sendingGatewayPorts2 = sendingForwarder2.AppointedGatewayAgentPorts.AddNew();
			sendingGatewayPorts2.O5_PortOrCountry = "DE";
			sendingGatewayPorts2.O5_RoadAgentStatus = "GTT";
			sendingGatewayPorts2.O5_OA_AgentOfficeAddress = sendingForwarder2.MainAddress.PK;
			consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol2.JK_OA_SendingForwarderAddress = sendingForwarder2.MainAddress.PK;

			shipment.Consols.Add(consol2);

			var agent = Factory.NewWithValidTestData<OrgHeader>();
			var agentGatewayPorts = agent.AppointedGatewayAgentPorts.AddNew();
			agentGatewayPorts.O5_PortOrCountry = "CN";
			agentGatewayPorts.O5_RoadAgentStatus = "GTT";
			agentGatewayPorts.O5_OA_AgentOfficeAddress = agent.MainAddress.PK;
			var manualAddedGateway = shipment.Gateways.AddNew();
			manualAddedGateway.JSG_OA_ForwarderAddress = agent.MainAddress.PK;

			return shipment;
		}

		public void TestDuplicateAddressValidation()
		{
			var expectedError = "The Gateway must be unique.";

			var org = Factory.New<OrgHeader>();
			var address1 = org.Addresses.AddNew();
			var address2 = org.Addresses.AddNew();

			var shipment = Factory.New<ForwardingShipment>();
			var gateway1 = shipment.Gateways.AddNew();
			gateway1.JSG_OA_ForwarderAddress = address1.PK;
			AssertNoError(gateway1.JSG_OA_ForwarderAddressInfo, expectedError);

			var gateway2 = shipment.Gateways.AddNew();
			gateway2.JSG_OA_ForwarderAddress = address2.PK;
			AssertNoError(gateway2.JSG_OA_ForwarderAddressInfo, expectedError);

			var gateway3 = shipment.Gateways.AddNew();
			gateway3.JSG_OA_ForwarderAddress = address1.PK;
			AssertHasError(gateway3.JSG_OA_ForwarderAddressInfo, expectedError);

			gateway1.Validation.ValidateJSG_OA_ForwarderAddress();
			gateway2.Validation.ValidateJSG_OA_ForwarderAddress();

			AssertHasError(gateway1.JSG_OA_ForwarderAddressInfo, expectedError);
			AssertNoError(gateway2.JSG_OA_ForwarderAddressInfo, expectedError);
		}

		public void TestAddressIsValidGateway()
		{
			var expectedError = @"The address is not a valid Gateway.

Gateway addresses can be configured in Organization -> Fwd/Agent -> Details -> Gateway Agent.";

			var org = Factory.New<OrgHeader>();
			var address1 = org.Addresses.AddNew();
			var address2 = org.Addresses.AddNew();

			var gatewayPorts = org.AppointedGatewayAgentPorts.AddNew();
			gatewayPorts.O5_PortOrCountry = "AU";
			gatewayPorts.O5_RoadAgentStatus = "GTT";
			gatewayPorts.O5_OA_AgentOfficeAddress = address1.PK;

			var shipment = Factory.New<ForwardingShipment>();
			var gateway1 = shipment.Gateways.AddNew();
			gateway1.JSG_OA_ForwarderAddress = address1.PK;
			AssertNoError("Address1 is configured as Gateway", gateway1.JSG_OA_ForwarderAddressInfo, expectedError);

			var gateway2 = shipment.Gateways.AddNew();
			gateway2.JSG_OA_ForwarderAddress = address2.PK;
			AssertHasError("Address2 is not configured as Gateway", gateway2.JSG_OA_ForwarderAddressInfo, expectedError);
		}

		#endregion

		#region Forwarder

		public void TestValidateForwarderPK()
		{
			var expectedMandatoryValidationError = "Please enter a Gateway Agent.";
			var expectedInvalidValidationError = "Enter a valid Gateway Agent.";

			var shipment = Factory.New<ForwardingShipment>();
			var gateway = shipment.Gateways.AddNew();
			gateway.ForwarderPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			AssertNoError(gateway.ForwarderPKInfo, expectedMandatoryValidationError);
			AssertNoError(gateway.ForwarderPKInfo, expectedInvalidValidationError);

			gateway.ForwarderPK = ZGuid.Empty;
			AssertHasError(gateway.ForwarderPKInfo, expectedMandatoryValidationError);
			AssertNoError(gateway.ForwarderPKInfo, expectedInvalidValidationError);

			gateway.ForwarderPK = ZGuid.Invalid;
			gateway.Validation.ValidateForwarderPK();
			AssertNoError(gateway.ForwarderPKInfo, expectedMandatoryValidationError);
			AssertHasError(gateway.ForwarderPKInfo, expectedInvalidValidationError);
		}

		#endregion
	}
}
