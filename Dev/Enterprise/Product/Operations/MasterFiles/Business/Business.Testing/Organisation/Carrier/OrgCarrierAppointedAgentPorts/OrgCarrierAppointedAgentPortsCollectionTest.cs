using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCarrierAppointedAgentPortsDependentCollection))]
	public class OrgCarrierAppointedAgentPortsDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestOrgCarrierAppointedPortsCollection()
		{
			OrgHeader headerForTest = NewTestHeaderForTests();
			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_FullName = "Test Org 1 2 3";
			header.OH_RL_NKClosestPort = "AUSYD";
			header.MainAddress.OA_Address1 = "26 Myrtle Street";
			header.MainAddress.OA_City = "Prospect";
			header.OH_Code = "TestOrg123";
			OrgAppointedAgentPorts appAgPorts = header.AppointedAgentPorts.AddNew();
			appAgPorts.O5_PortOrCountry = "USLAX";
			appAgPorts.O5_AirAgentStatus = AgentStatusList.Codes.Appointed;
			appAgPorts.O5_OA_AgentOfficeAddress = header.MainAddress.PK;
			OrgCarrierAppointedAgentPorts carrAppPorts = header.CarrierAppointedAgentPorts_AirCTO.AddNew();
			carrAppPorts.O5_PortOrCountry = "USLAX";

			OrgHeaderCollection organisations = carrAppPorts.Lookups.OrganisationList;
			ZQuery query = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.Equal, "Test123");
			organisations.Load(query);
			carrAppPorts.OrganisationPK = organisations[0].PK;
			carrAppPorts.O5_OA_AgentOfficeAddress = organisations[0].MainAddress.PK;

			header.Factory.Save();

			organisations = new OrgHeaderCollection(Factory);
			organisations.Load(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.Equal, "TestOrg123"));
			Assert(!organisations[0].CarrierAppointedAgentPorts_AirCTO.Contains(appAgPorts.PK));
			Assert(organisations[0].CarrierAppointedAgentPorts_AirCTO.Contains(carrAppPorts.PK));
		}

		public void TestFindAddress()
		{
			OrgHeader carrier = Factory.NewWithValidTestData<OrgHeader>();

			OrgAddress sydCFS = AddAppointedAgent(carrier.CarrierAppointedAgentPorts_Stevedore, "AUSYD", StevedoreTerminalType.Codes.ContainerTerminal);
			OrgAddress melCFS = AddAppointedAgent(carrier.CarrierAppointedAgentPorts_RailHeadDepot, "AUMEL", "");
			OrgAddress bneCFS = AddAppointedAgent(carrier.CarrierAppointedAgentPorts_Stevedore, "AU", StevedoreTerminalType.Codes.ContainerTerminal);
			bneCFS.Header.OH_RL_NKClosestPort = "AUBNE";

			AssertEquals(sydCFS, carrier.CarrierAppointedAgentPorts_Stevedore.FindAddress("AUSYD", StevedoreTerminalType.Codes.ContainerTerminal));
			AssertEquals(null, carrier.CarrierAppointedAgentPorts_RailHeadDepot.FindAddress("AUSYD"));
			AssertEquals(melCFS, carrier.CarrierAppointedAgentPorts_RailHeadDepot.FindAddress("AUMEL"));
			AssertEquals(bneCFS, carrier.CarrierAppointedAgentPorts_Stevedore.FindAddress("AUMEL", StevedoreTerminalType.Codes.ContainerTerminal));
		}

		public void TestFindContainerYardAddress()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = Factory.New<OrgAddress>();
			var address2 = Factory.New<OrgAddress>();

			var carrierConfig1 = carrier.CarrierAppointedAgentPorts_ContainerYardPark.AddNew();
			carrierConfig1.O5_PortOrCountry = "AUSYD";
			carrierConfig1.O5_OA_AgentOfficeAddress = address1.PK;
			var config1Type1 = carrierConfig1.ContainerTypes.AddNew();
			config1Type1.PT_ContainerStorageClass = "20F";

			var config1Type2 = carrierConfig1.ContainerTypes.AddNew();
			config1Type2.PT_ContainerStorageClass = "40F";

			var carrierConfig2 = carrier.CarrierAppointedAgentPorts_ContainerYardPark.AddNew();
			carrierConfig2.O5_PortOrCountry = "AU";
			carrierConfig2.O5_OA_AgentOfficeAddress = address2.PK;
			var config2Type1 = carrierConfig2.ContainerTypes.AddNew();
			config2Type1.PT_ContainerStorageClass = "ZUB";

			var container = Factory.New<RefContainer>();
			container.RC_StorageClass = "20F";
			AssertEquals(address1, carrier.CarrierAppointedAgentPorts_ContainerYardPark.FindContainerYardAddress("AUSYD", container));

			container.RC_StorageClass = "20G";
			AssertEquals(null, carrier.CarrierAppointedAgentPorts_ContainerYardPark.FindContainerYardAddress("AUSYD", container));

			container.RC_StorageClass = "40F";
			AssertEquals(null, carrier.CarrierAppointedAgentPorts_ContainerYardPark.FindContainerYardAddress("AUMEL", container));

			container.RC_StorageClass = "ZUB";
			AssertEquals(address2, carrier.CarrierAppointedAgentPorts_ContainerYardPark.FindContainerYardAddress("AUMEL", container));
		}

		public void TestFindAddressWithDirection()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();

			var sydArrivalCTO = AddAppointedAgent(carrier.CarrierAppointedAgentPorts_Stevedore, "AUSYD", StevedoreTerminalType.Codes.ContainerTerminal, OrgConstants.CarrierAgentDirections.Code.Arrival);
			var sydCTO = AddAppointedAgent(carrier.CarrierAppointedAgentPorts_Stevedore, "AUSYD", StevedoreTerminalType.Codes.ContainerTerminal);
			var auArrivalCTO = AddAppointedAgent(carrier.CarrierAppointedAgentPorts_Stevedore, "AU", StevedoreTerminalType.Codes.ContainerTerminal, OrgConstants.CarrierAgentDirections.Code.Arrival);
			var auDepartureCTO = AddAppointedAgent(carrier.CarrierAppointedAgentPorts_Stevedore, "AU", StevedoreTerminalType.Codes.ContainerTerminal, OrgConstants.CarrierAgentDirections.Code.Departure);
			var bneCNTCTO = AddAppointedAgent(carrier.CarrierAppointedAgentPorts_Stevedore, "AUBNE", StevedoreTerminalType.Codes.ContainerTerminal, OrgConstants.CarrierAgentDirections.Code.Arrival);
			var bneRORCTO = AddAppointedAgent(carrier.CarrierAppointedAgentPorts_Stevedore, "AUBNE", StevedoreTerminalType.Codes.ROROTerminal, OrgConstants.CarrierAgentDirections.Code.Arrival);

			AssertEquals("Port matches and takes precedence over Direction", sydArrivalCTO, carrier.CarrierAppointedAgentPorts_Stevedore.FindAddress("AUSYD", StevedoreTerminalType.Codes.ContainerTerminal, OrgConstants.CarrierAgentDirections.Code.Arrival));
			AssertEquals("Port matches and takes precedence over Direction", sydCTO, carrier.CarrierAppointedAgentPorts_Stevedore.FindAddress("AUSYD", StevedoreTerminalType.Codes.ContainerTerminal, OrgConstants.CarrierAgentDirections.Code.Departure));
			AssertEquals("No port matches, match by Country, Type and Direction", auArrivalCTO, carrier.CarrierAppointedAgentPorts_Stevedore.FindAddress("AUMEL", StevedoreTerminalType.Codes.ContainerTerminal, OrgConstants.CarrierAgentDirections.Code.Arrival));
			AssertEquals("No port matches, match by Country, Type  and Direction", auDepartureCTO, carrier.CarrierAppointedAgentPorts_Stevedore.FindAddress("AUMEL", StevedoreTerminalType.Codes.ContainerTerminal, OrgConstants.CarrierAgentDirections.Code.Departure));
			AssertEquals("Port, Type, Direction matches", bneCNTCTO, carrier.CarrierAppointedAgentPorts_Stevedore.FindAddress("AUBNE", StevedoreTerminalType.Codes.ContainerTerminal, OrgConstants.CarrierAgentDirections.Code.Arrival));
			AssertEquals("Port, Type, Direction matches", bneRORCTO, carrier.CarrierAppointedAgentPorts_Stevedore.FindAddress("AUBNE", StevedoreTerminalType.Codes.ROROTerminal, OrgConstants.CarrierAgentDirections.Code.Arrival));
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgCarrierAppointedAgentPortsDependentCollection(OrgHeader.New(Factory), CarrierOrForwarderType.Codes.AirCTO);
		}

		protected OrgHeader NewTestHeaderForTests()
		{
			OrgHeader headerForTest = Factory.New<OrgHeader>();
			headerForTest.OH_FullName = "Test Org 1 2 3";
			headerForTest.OH_RL_NKClosestPort = "AUSYD";
			headerForTest.MainAddress.OA_Address1 = "26 Test Street";
			headerForTest.MainAddress.OA_City = "Prospect";
			headerForTest.OH_Code = "Test123";
			headerForTest.OH_IsShippingProvider = ZBool.True;
			headerForTest.OH_IsAirCTO = ZBool.True;
			headerForTest.OH_IsSeaCTO = ZBool.True;
			headerForTest.OH_IsRailHead = ZBool.True;
			headerForTest.OH_IsRoadFreightDepot = ZBool.True;
			headerForTest.Factory.Save();
			return headerForTest;
		}

		OrgAddress AddAppointedAgent(OrgCarrierAppointedAgentPortsDependentCollection collection, string port, string terminalType, string direction = OrgConstants.CarrierAgentDirections.Code.Both)
		{
			OrgHeader agent = Factory.NewWithValidTestData<OrgHeader>();
			agent.OH_RL_NKClosestPort = port;

			OrgAppointedAgentPorts map = collection.AddNew();
			map.O5_PortOrCountry = port;
			map.O5_OA_AgentOfficeAddress = agent.MainAddress.PK;
			map.O5_TerminalType = terminalType;
			map.O5_AgentDirection = direction;

			return agent.MainAddress;
		}

		#endregion
	}
}
