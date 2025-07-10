using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class GlbBranchDefaultPortLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDefaultToList()
		{
			var defaultPort = Factory.NewWithValidTestData<GlbBranchDefaultPort>();
			var defaultToList = defaultPort.Lookups.DefaultToList;
			AssertEquals(4, defaultToList.Count);
			CombineAssertions("DefaultToList should contain expected codes.", () =>
			{
				Assert(defaultToList.ContainsCode(GlbBranchDefaultToList.Codes.ShipmentOrigin));
				Assert(defaultToList.ContainsCode(GlbBranchDefaultToList.Codes.ShipmentDestination));
				Assert(defaultToList.ContainsCode(GlbBranchDefaultToList.Codes.ConsolFirstLoad));
				Assert(defaultToList.ContainsCode(GlbBranchDefaultToList.Codes.ConsolLastDischarge));
			});
		}

		public void TestTransportModeList()
		{
			var defaultPort = Factory.NewWithValidTestData<GlbBranchDefaultPort>();
			defaultPort.GBP_DefaultTo = GlbBranchDefaultToList.Codes.ShipmentDestination;
			var transportModeList = defaultPort.Lookups.TransportModeList;
			AssertEquals(6, transportModeList.Count);
			CombineAssertions("TransportModeList should contain expected codes for Shipment.", () =>
			{
				Assert(transportModeList.ContainsCode(Core.Constants.TransportModes.Air));
				Assert(transportModeList.ContainsCode(Core.Constants.TransportModes.Sea));
				Assert(transportModeList.ContainsCode(Core.Constants.TransportModes.Road));
				Assert(transportModeList.ContainsCode(Core.Constants.TransportModes.Rail));
				Assert(transportModeList.ContainsCode(Core.Constants.TransportModes.Courier));
				Assert(transportModeList.ContainsCode(Core.Constants.TransportModes.All));
			});

			defaultPort.GBP_DefaultTo = GlbBranchDefaultToList.Codes.ConsolFirstLoad;
			transportModeList = defaultPort.Lookups.TransportModeList;
			AssertEquals(5, transportModeList.Count);
			Assert("TransportModeList should not contain Courier for Consol.", !transportModeList.ContainsCode(Core.Constants.TransportModes.Courier));
		}

		public void TestContainerModeList_DefaultToConsol()
		{
			var defaultPort = Factory.NewWithValidTestData<GlbBranchDefaultPort>();
			defaultPort.GBP_DefaultTo = GlbBranchDefaultToList.Codes.ConsolFirstLoad;
			defaultPort.GBP_TransportMode = Core.Constants.TransportModes.Air;
			var containerModeList = defaultPort.Lookups.ContainerModeList;
			AssertEquals(4, containerModeList.Count);
			CombineAssertions("ContainerModeList should contain expected codes for Air Consol.", () =>
			{
				Assert(containerModeList.ContainsCode("ALL"));
				Assert(containerModeList.ContainsCode(Core.Constants.ContainerModes.ULD));
				Assert(containerModeList.ContainsCode(Core.Constants.ContainerModes.Loose));
				Assert(containerModeList.ContainsCode(Core.Constants.ContainerModes.Other));
			});

			defaultPort.GBP_TransportMode = Core.Constants.TransportModes.All;
			containerModeList = defaultPort.Lookups.ContainerModeList;
			AssertEquals(1, containerModeList.Count);
			Assert(containerModeList.ContainsCode("ALL"));
		}

		public void TestContainerModeList_DefaultToShipment()
		{
			var defaultPort = Factory.NewWithValidTestData<GlbBranchDefaultPort>();
			defaultPort.GBP_DefaultTo = GlbBranchDefaultToList.Codes.ShipmentDestination;
			defaultPort.GBP_TransportMode = Core.Constants.TransportModes.Air;
			var containerModeList = defaultPort.Lookups.ContainerModeList;
			AssertEquals(4, containerModeList.Count);
			CombineAssertions("ContainerModeList should contain expected codes for Air Shipment.", () =>
			{
				Assert(containerModeList.ContainsCode("ALL"));
				Assert(containerModeList.ContainsCode(Core.Constants.ContainerModes.ULD));
				Assert(containerModeList.ContainsCode(Core.Constants.ContainerModes.Loose));
				Assert(containerModeList.ContainsCode(Core.Constants.ContainerModes.AgentConsol));
			});

			defaultPort.GBP_TransportMode = Core.Constants.TransportModes.All;
			containerModeList = defaultPort.Lookups.ContainerModeList;
			AssertEquals(1, containerModeList.Count);
			Assert(containerModeList.ContainsCode("ALL"));
		}
	}
}
