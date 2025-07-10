using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.GUI.Testing
{
	public class CommandJobTypeMenuGroupTest : TestCaseWithFactory
	{
		public void TestSubMenuItemsAndThisMenuItemIsVisible()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_HouseBill = "HouseBill001";
				shipment.JS_RL_NKDestination = "AUSYD";
				shipment.JS_RL_NKOrigin = "USLAX";

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_MasterBillNum = "MAWB1234";
				consol.Transports.MostInterestingTransport.JW_VoyageFlight = "Flight100";
				consol.Shipments.Add(shipment);

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var menuItem = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menuItem_ => menuItem_.Caption == "HVLV AirCargo Report");
					menuItem.OnPopup(EventArgs.Empty);

					AssertNotNull("A job type menu item has been created", menuItem);
					AssertEquals("There is 3 menu items under this menu item", 3, menuItem.MenuItems.Count);
					AssertEquals("There is 1 submenu item could be visible", 1, menuItem.MenuItems.OfType<ZMenuItem>().Where(menuItem_ => menuItem_.Visible).ToList().Count);
				}
			}
		}
	}
}
