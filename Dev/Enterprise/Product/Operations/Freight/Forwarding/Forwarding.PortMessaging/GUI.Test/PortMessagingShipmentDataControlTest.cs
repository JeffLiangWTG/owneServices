using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.Forwarding.PortMessaging.GUI.Testing
{
	sealed class PortMessagingShipmentDataControlTest : TestCaseWithFactory
	{
		public void TestDGVisibleColumnReadOnly()
		{
			using (var control = new PortMessagingShipmentControl())
			{
				var grid = control.Controls.Find("PackLinesGrid", true).First() as ZGrid;
				var column = grid.GetColumnStyle("UNDGs+UNDGTechnicalNameManager+Value");
				AssertNotNull("DG Technical Name column should be in grid", column);
				Assert("DG Technical Name should be read only", column.IsReadOnly);

				column = grid.GetColumnStyle("UNDGs+UNDGClassManager+Value");
				AssertNotNull("DG Class column should be in grid", column);
				Assert("DG Class should not be read only", !column.IsReadOnly);

				column = grid.GetColumnStyle("UNDGs+UNDGSubstanceManager+Value");
				AssertNotNull("DG Subs column should be in grid", column);
				Assert("DG Subs should not be read only", !column.IsReadOnly);
			}
		}
	}
}
