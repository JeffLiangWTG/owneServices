using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class GatewayShipmentRateLineConditionsSupporterTest : TestCaseWithFactory
	{
		public void TestSendingAgent()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consol1 = shipment.Consols.AddNew();
			consol1.JK_OA_SendingForwarderAddress = org.MainAddress.PK;

			var consol2 = shipment.Consols.AddNew();

			consol2.JK_AgentType = Core.Constants.AgentType.Agent;
			consol2.JK_RL_NKLoadPort = "AUBNE";
			consol2.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.First().PK;
			consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			var orgAppointedAgentPorts1 = Factory.New<OrgAppointedAgentPorts>();
			orgAppointedAgentPorts1.O5_PortOrCountry = "AUBNE";
			consol2.SendingForwarder.AppointedGatewayAgentPorts.Add(orgAppointedAgentPorts1);

			var gatewayRatingAdapter = new GatewayShipmentRatingAdapter(consol2, shipment);
			AssertEquals(GlbCompany.CurrentCompany.OrgProxy.PK, gatewayRatingAdapter.ConditionsSupporter.SendingAgent.PK);
		}

		public void TestReceivingAgent()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consol1 = shipment.Consols.AddNew();
			consol1.JK_OA_SendingForwarderAddress = org.MainAddress.PK;
			consol1.JK_OA_ReceivingForwarderAddress = org.MainAddress.PK;

			var consol2 = shipment.Consols.AddNew();

			consol2.JK_AgentType = Core.Constants.AgentType.Agent;
			consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol2.JK_RL_NKLoadPort = "AUBNE";
			consol2.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.First().PK;
			consol2.JK_OA_ReceivingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.First().PK;
			var orgAppointedAgentPorts1 = Factory.New<OrgAppointedAgentPorts>();
			orgAppointedAgentPorts1.O5_PortOrCountry = "AUBNE";
			consol2.SendingForwarder.AppointedGatewayAgentPorts.Add(orgAppointedAgentPorts1);

			var gatewayRatingAdapter = new GatewayShipmentRatingAdapter(consol2, shipment);
			AssertEquals(GlbCompany.CurrentCompany.OrgProxy.PK, gatewayRatingAdapter.ConditionsSupporter.ReceivingAgent.PK);
		}
	}
}
