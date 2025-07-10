using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyShipmentContainerModeListTest : TestCaseWithFactory
	{
		public void TestContainerModeForFCL()
		{
			const string expected = "BCN - Buyer's Consolidation\r\n" + "FCL - Full Container Load\r\n" + "GRP - Groupage / Freight All Kinds\r\n" + "LCL - Less Container Load";
			AssertMultilineASCIIEquals("", expected, new AgencyShipmentContainerModeList(Constants.ContainerModes.FCL).ElementsAsString);
		}

		public void TestContainerModeForROR()
		{
			AssertMultilineASCIIEquals("", "ROR - Roll On/Roll Off", new AgencyShipmentContainerModeList(Constants.ContainerModes.RollOnRollOff).ElementsAsString);
		}

		public void TestContainerModeForBBK()
		{
			AssertMultilineASCIIEquals("", "BBK - Break Bulk", new AgencyShipmentContainerModeList(Constants.ContainerModes.BreakBulk).ElementsAsString);
		}

		public void TestContainerModeForBLK()
		{
			AssertMultilineASCIIEquals("", "BLK - Bulk", new AgencyShipmentContainerModeList(Constants.ContainerModes.Bulk).ElementsAsString);
		}

		public void TestContainerModeForLQD()
		{
			AssertMultilineASCIIEquals("", "LQD - Liquid", new AgencyShipmentContainerModeList(Constants.ContainerModes.Liquid).ElementsAsString);
		}

		public void TestContainerModeForEmpty()
		{
			const string expected = @"BCN - Buyer's Consolidation
FCL - Full Container Load
GRP - Groupage / Freight All Kinds
LCL - Less Container Load
ROR - Roll On/Roll Off
BBK - Break Bulk
BLK - Bulk
LQD - Liquid";
			AssertMultilineASCIIEquals("", expected, new AgencyShipmentContainerModeList(null).ElementsAsString);
		}
	}
}
