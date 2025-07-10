using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgAppointedAgentPortsLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAgentStatusList()
		{
			OrgAppointedAgentPorts appAgPort = Factory.New<OrgAppointedAgentPorts>();
			appAgPort.O5_SeaAirCarrierOrForwarderType = string.Empty;
			Assert("AgentStatusList.Count = 3", appAgPort.Lookups.AgentStatuses.Count == 3);
			Assert("AgentStatusList should have option " + AgentStatusList.Codes.Handles, appAgPort.Lookups.AgentStatuses.ContainsCode(AgentStatusList.Codes.Handles));
			Assert("AgentStatusList should have option " + AgentStatusList.Codes.Appointed, appAgPort.Lookups.AgentStatuses.ContainsCode(AgentStatusList.Codes.Appointed));
			Assert("AgentStatusList should have option " + AgentStatusList.Codes.Published, appAgPort.Lookups.AgentStatuses.ContainsCode(AgentStatusList.Codes.Published));

			appAgPort.O5_SeaAirCarrierOrForwarderType = OrgAppointedAgentPorts.Forwarder;
			Assert("AgentStatusList.Count = 3", appAgPort.Lookups.AgentStatuses.Count == 3);
			Assert("AgentStatusList should have option " + AgentStatusList.Codes.Handles, appAgPort.Lookups.AgentStatuses.ContainsCode(AgentStatusList.Codes.Handles));
			Assert("AgentStatusList should have option " + AgentStatusList.Codes.Appointed, appAgPort.Lookups.AgentStatuses.ContainsCode(AgentStatusList.Codes.Appointed));
			Assert("AgentStatusList should have option " + AgentStatusList.Codes.Published, appAgPort.Lookups.AgentStatuses.ContainsCode(AgentStatusList.Codes.Published));

			appAgPort.O5_SeaAirCarrierOrForwarderType = OrgAppointedAgentPorts.GatewayAgent;
			Assert("AgentStatusList.Count = 2", appAgPort.Lookups.AgentStatuses.Count == 2);
			Assert("AgentStatusList should have option " + AgentStatusList.Codes.GatewayAgent, appAgPort.Lookups.AgentStatuses.ContainsCode(AgentStatusList.Codes.GatewayAgent));
			Assert("AgentStatusList should have option " + AgentStatusList.Codes.GatewayAgentWithTariff, appAgPort.Lookups.AgentStatuses.ContainsCode(AgentStatusList.Codes.GatewayAgentWithTariff));
		}

		public void TestLocations()
		{
			OrgAppointedAgentPorts appAgPorts = Factory.New<OrgAppointedAgentPorts>();
			AssertNotNull("Locations list should not be null", appAgPorts.Lookups.Locations);
		}

		public void TestAddresses()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "Test Org 1 2 3";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.MainAddress.OA_Address1 = "26 Myrtle Street";
			org.MainAddress.OA_City = "Prospect";
			Factory.Save();

			OrgAppointedAgentPorts appAgPorts = org.AppointedAgentPorts.AddNew();

			AssertNotNull("Addresses list should not be null", org.AppointedAgentPorts[0].Lookups.Addresses);
		}

		public void TestActiveAddresses()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "Test Org 1 2 3 4";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.MainAddress.OA_Address1 = "26 Myrtle Street";
			org.MainAddress.OA_City = "Prospect";

			var address = Factory.New<OrgAddress>();
			address.Address1 = "10 fake st";
			address.City = "Prospect";

			address.OA_IsActive = false;

			org.Addresses.Add(address);

			Factory.Save();

			OrgAppointedAgentPorts appAgPorts = org.AppointedAgentPorts.AddNew();

			AssertNotNull("ActiveAddresses list should not be null", org.AppointedAgentPorts[0].Lookups.ActiveAddresses);
			AssertEquals("ActiveAddresses list should equal 1", 1, org.AppointedAgentPorts[0].Lookups.ActiveAddresses.Count);
			AssertEquals("ActiveAddress element should be org main address", org.MainAddress.Address1, org.AppointedAgentPorts[0].Lookups.ActiveAddresses[0].Address1);
		}

		public void TestAgentDirections()
		{
			string expected = ""
				+ "BTH - Both Import and Export\r\n"
				+ "EXP - Export Only\r\n"
				+ "IMP - Import Only"
				+ "";

			OrgAppointedAgentPorts agentPorts = Factory.New<OrgAppointedAgentPorts>();
			AssertEquals(expected, agentPorts.Lookups.AgentDirections.ElementsAsString);
		}
	}
}
