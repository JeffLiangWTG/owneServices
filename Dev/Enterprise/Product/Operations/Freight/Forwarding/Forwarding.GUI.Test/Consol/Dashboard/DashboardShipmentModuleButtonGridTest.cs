using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class DashboardShipmentModuleButtonGridTest : TestCaseWithFactory
	{
		public void TestNewButtonIsNotShown()
		{
			using (var moduleButtonGrid = new DashboardShipmentModuleButtonGrid(new ForwardingShipmentCollection(Factory)))
			{
				Assert(!moduleButtonGrid.ShowNewButton);
			}
		}

		public void TestEditButtonIsNotShown()
		{
			using (var moduleButtonGrid = new DashboardShipmentModuleButtonGrid(new ForwardingShipmentCollection(Factory)))
			{
				Assert(!moduleButtonGrid.ShowEditButton);
			}
		}

		public void TestTotalsPanelIsShown()
		{
			using (var moduleButtonGrid = new DashboardShipmentModuleButtonGrid(new ForwardingShipmentCollection(Factory)))
			{
				Assert(moduleButtonGrid.ShowTotalsPanel);
				Assert(moduleButtonGrid.TotalsPanel.Visible);
			}
		}
	}
}
