using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.GUI.Testing
{
	public class SendACASMenuGroupTest : TestCaseWithFactory
	{
		public void TestSubMenuItemsAndThisMenuItemIsVisible()
		{
			var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, true);
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKDestination = "USLAX";
			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var hVLVMenu = plugin.TopLevelMenu;
				hVLVMenu.PerformSelect();

				var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				var sendACASMenuGroup = customsMenu.MenuItems.OfType<SendACASMenuGroup>().Single();

				sendACASMenuGroup.OnPopup(EventArgs.Empty);

				AssertEquals(3, sendACASMenuGroup.MenuItems.Count);
				AssertContainsExactElementsInAnyOrder(new[]
				{
					typeof(SendACASReportMenuItem),
					typeof(SendACASAmendmentMenuItem),
					typeof(SendACASAcknowledgementMenuItem),
				}, sendACASMenuGroup.MenuItems.OfType<ZMenuItem>().Select(x => x.GetType()).ToList());
				AssertEquals("The visible of ACASMenuGroup is True", true, sendACASMenuGroup.Visible);

				shipment.JS_TransportMode = "SEA";
				sendACASMenuGroup.OnPopup(EventArgs.Empty);
				AssertEquals("The visible of ACASMenuGroup is False", false, sendACASMenuGroup.Visible);
			}
		}
	}
}
