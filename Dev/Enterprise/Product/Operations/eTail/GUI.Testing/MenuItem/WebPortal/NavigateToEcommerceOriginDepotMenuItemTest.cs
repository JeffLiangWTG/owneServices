using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.eTail.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.eTail.GUI.Testing
{
	public class NavigateToEcommerceOriginDepotMenuItemTest : TestCaseWithFactory
	{
		public void TestMenuItemAction_NavigationToPortalsBaseUrlWithOriginCode()
		{
			var shipment = HVLVMenuItemTestHelper.CreateShipmentWithAddress(Factory, out _, out _, out _);
			Factory.Save();

			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var topLevelMenu = plugin.TopLevelMenu;
				topLevelMenu.PerformSelect();

				var menuItem = topLevelMenu.MenuItems.OfType<NavigateToEcommerceWebPortalsMenuGroup>().Single();
				var originDepot = menuItem.MenuItems.OfType<NavigateToEcommerceOriginDepotMenuItem>().Single();

				HVLVMenuItemTestHelper.AssertEcommercePortalNavigated(EcommercePortals.Codes.EOS, originDepot);
			}
		}

		public void TestMenuItemAction_WhenWebPortalHasNotBeenConfigured_ShowsWarningMessage()
		{
			var shipment = HVLVMenuItemTestHelper.CreateShipmentWithAddress(Factory, out _, out _, out _);
			Factory.Save();

			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var topLevelMenu = plugin.TopLevelMenu;
				topLevelMenu.PerformSelect();

				var menuItem = topLevelMenu.MenuItems.OfType<NavigateToEcommerceWebPortalsMenuGroup>().Single();
				var originDepot = menuItem.MenuItems.OfType<NavigateToEcommerceOriginDepotMenuItem>().Single();
				originDepot.PerformClick();

				HVLVMenuItemTestHelper.AssertWebPortalMenuItemShowsErrorMessage(originDepot);
			}
		}
	}
}
