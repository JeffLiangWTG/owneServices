using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.GUI.Testing
{
	public class USISFMenuGroupTest : TestCaseWithFactory
	{
		public void TestSubMenuItemsAndThisMenuItemIsVisible()
		{
			var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, true);
			shipment.JS_RL_NKDestination = "USCHI";
			shipment.JS_TransportMode = TransportModes.Sea;
			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var hVLVMenu = plugin.TopLevelMenu;
				hVLVMenu.PerformSelect();

				var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				var uSISFMenuGroup = customsMenu.MenuItems.OfType<USISFMenuGroup>().Single();

				uSISFMenuGroup.OnPopup(EventArgs.Empty);

				AssertEquals(3, uSISFMenuGroup.MenuItems.Count);
				AssertContainsExactElementsInAnyOrder(new[]
				{
					typeof(CreateUSISFMenuItem),
					typeof(OpenUSISFMenuItem),
					typeof(SyncUSISFMenuItem)
				}, uSISFMenuGroup.MenuItems.OfType<ZMenuItem>().Select(x => x.GetType()).ToList());
				AssertEquals("The visible of USISFMenuGroup is True", true, uSISFMenuGroup.Visible);

				shipment.JS_TransportMode = TransportModes.Air;
				uSISFMenuGroup.OnPopup(EventArgs.Empty);
				AssertEquals("The visible of USISFMenuGroup is False", false, uSISFMenuGroup.Visible);
			}
		}
	}
}
