using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.eTail.GUI.Testing
{
	public class DeclarationMenuGroupTest : TestCaseWithFactory
	{
		public void TestSubMenuItems()
		{
			var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, true);
			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var menuHVLV = plugin.TopLevelMenu;
				menuHVLV.PerformSelect();

				var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				var menuItem = customsMenu.MenuItems.OfType<DeclarationMenuGroup>().Single();
				menuItem.OnPopup(EventArgs.Empty);

				AssertEquals(2, menuItem.MenuItems.Count);
				AssertContainsExactElementsInAnyOrder(new[]
				{
					typeof(ConvertConsignmentsToStandAloneDeclarationMenuItem),
					typeof(ViewEditCustomsDeclarationsMenuItem)
				}, menuItem.MenuItems.OfType<ZMenuItem>().Select(x => x.GetType()).ToList());
				AssertEquals("The visible of USISFMenuGroup is True", true, menuItem.Visible);
			}
		}
	}
}
