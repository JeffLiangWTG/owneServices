using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.PortHubs.Module;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.PortHubs.GUI.Testing
{
	sealed class PortDepotCarrierFilterStripControlTest : TestCaseWithFactory
	{
		public new BusinessObjectFactory Factory { get; private set; }

		public void TestStripControl()
		{
			using (var form = new ZForm())
			using (var module = new PortDepotCarrierSelectionModule())
			{
				var filter = (PortDepotCarrierFilterStripControl)module.EmbeddedControl;
				form.Controls.Add(filter);
				form.Show();

				AssertNotNull("The column Direction Received at Origin should be added", filter.FilteredGrid.GetColumnStyle("TY_Direction"));
			}
		}
	}
}
