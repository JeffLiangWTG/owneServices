using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.eTail.GUI.Testing
{
	class HVLVPreScreenMenuGroupTest : TestCaseWithFactory
	{
		public void TestSubMenuItems()
		{
			var shipment = HVLVMenuItemTestHelper.CreateShipmentWithAddress(Factory, out _, out _, out _);
			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var topLevelMenu = plugin.TopLevelMenu;
				topLevelMenu.PerformSelect();

				var menuItem = topLevelMenu.MenuItems.OfType<HVLVPreScreenMenuGroup>().Single();
				menuItem.OnPopup(EventArgs.Empty);

				AssertEquals(2, menuItem.MenuItems.Count);
				AssertContainsExactElementsInAnyOrder(new[]
				{
					typeof(PreScreenMenuItem),
					typeof(AcceptAllPreScreeningErrorsAsAcceptedMenuItem),
				}, menuItem.MenuItems.OfType<ZMenuItem>().Select(x => x.GetType()).ToList());
			}
		}
	}
}
