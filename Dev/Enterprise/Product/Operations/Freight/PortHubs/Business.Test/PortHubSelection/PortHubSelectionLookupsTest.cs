using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.PortHubs.Business;

namespace Enterprise.Freight.PortHubs.Testing
{
	public class PortHubSelectionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPackModes()
		{
			var selection = Factory.New<PortHubSelection>();
			AssertType<PortHubSelectionPackModeList>(selection.Lookups.PackModeList);
		}

		public void TestFreightModes()
		{
			var selection = Factory.New<PortHubSelection>();
			AssertType<TransportModeList>(selection.Lookups.FreightModeList);
		}

		public void TestDirectionList()
		{
			var selection = Factory.New<PortHubSelection>();
			AssertType<PortHubSelectionDirectionList>(selection.Lookups.DirectionList);
		}
	}
}
