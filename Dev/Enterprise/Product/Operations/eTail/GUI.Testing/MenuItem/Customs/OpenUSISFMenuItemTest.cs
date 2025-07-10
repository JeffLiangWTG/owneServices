using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.GUI.Testing
{
	public class OpenUSISFMenuItemTest : TestCaseWithFactory
	{
		public void TestMenuItemCreatedByDefault()
		{
			var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, true);
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

				var menuItem = uSISFMenuGroup.MenuItems.OfType<OpenUSISFMenuItem>().Single();
				Assert("Open US ISF menuitem should be created by default", menuItem.Visible);
			}
		}

		public void TestMenuItemVisibility_DependOnShipmentLog()
		{
			var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, true);
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_RL_NKDestination = "USCHI";
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
				var openUSISFMenuItem = uSISFMenuGroup.MenuItems.OfType<OpenUSISFMenuItem>().Single();
				Assert("Hide Open US ISF menuitem when shipment has no ISF log", !openUSISFMenuItem.Visible);

				shipment.Logs.AddNew(AutoEvents.Transferred, "|TYP=ISF");
				uSISFMenuGroup = customsMenu.MenuItems.OfType<USISFMenuGroup>().Single();
				uSISFMenuGroup.OnPopup(EventArgs.Empty);
				openUSISFMenuItem = uSISFMenuGroup.MenuItems.OfType<OpenUSISFMenuItem>().Single();
				Assert("Show Open US ISF menuitem when shipment has ISF log", openUSISFMenuItem.Visible);
			}
		}

		public void TestMenuItemVisibility_DependOnTransportMode()
		{
			var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, true);
			shipment.JS_RL_NKDestination = "USCHI";
			shipment.Logs.AddNew(AutoEvents.Transferred, "|TYP=ISF");
			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var hVLVMenu = plugin.TopLevelMenu;
				hVLVMenu.PerformSelect();

				var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();

				shipment.JS_TransportMode = TransportModes.Sea;
				var uSISFMenuGroup = customsMenu.MenuItems.OfType<USISFMenuGroup>().Single();
				uSISFMenuGroup.OnPopup(EventArgs.Empty);
				var openUSISFMenuItem = uSISFMenuGroup.MenuItems.OfType<OpenUSISFMenuItem>().Single();
				Assert("Show Open US ISF menuitem when transport mode is SEA", openUSISFMenuItem.Visible);

				shipment.JS_TransportMode = TransportModes.Air;
				uSISFMenuGroup.OnPopup(EventArgs.Empty);
				uSISFMenuGroup = customsMenu.MenuItems.OfType<USISFMenuGroup>().Single();
				openUSISFMenuItem = uSISFMenuGroup.MenuItems.OfType<OpenUSISFMenuItem>().Single();
				Assert("Hide Open US ISF menuitem when transport mode is AIR", !openUSISFMenuItem.Visible);

				shipment.JS_TransportMode = TransportModes.Road;
				uSISFMenuGroup.OnPopup(EventArgs.Empty);
				uSISFMenuGroup = customsMenu.MenuItems.OfType<USISFMenuGroup>().Single();
				openUSISFMenuItem = uSISFMenuGroup.MenuItems.OfType<OpenUSISFMenuItem>().Single();
				Assert("Hide Open US ISF menuitem when transport mode is not AIR or SEA", !openUSISFMenuItem.Visible);
			}
		}

		public void TestMenuItemVisibility_DependOnDestination()
		{
			var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, true);
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.Logs.AddNew(AutoEvents.Transferred, "|TYP=ISF");
			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var hVLVMenu = plugin.TopLevelMenu;
				hVLVMenu.PerformSelect();

				var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				shipment.JS_RL_NKDestination = "USCHI";

				var uSISFMenuGroup = customsMenu.MenuItems.OfType<USISFMenuGroup>().Single();
				uSISFMenuGroup.OnPopup(EventArgs.Empty);
				var openUSISFMenuItem = uSISFMenuGroup.MenuItems.OfType<OpenUSISFMenuItem>().Single();
				Assert("Show Open US ISF menuitem when destination is US", openUSISFMenuItem.Visible);

				shipment.JS_RL_NKDestination = "AUSYD";
				uSISFMenuGroup = customsMenu.MenuItems.OfType<USISFMenuGroup>().Single();
				uSISFMenuGroup.OnPopup(EventArgs.Empty);
				openUSISFMenuItem = uSISFMenuGroup.MenuItems.OfType<OpenUSISFMenuItem>().Single();
				Assert("Hide Open US ISF menuitem when destination is AU", !openUSISFMenuItem.Visible);
			}
		}

		public void TestMenuItemVisibility_GenPivot()
		{
			var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, true);
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_RL_NKDestination = "USCHI";
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
				var openUSISFMenuItem = uSISFMenuGroup.MenuItems.OfType<OpenUSISFMenuItem>().Single();
				Assert("Hide Open US ISF menuitem when there is no Gen Pivot record", !openUSISFMenuItem.Visible);

				HVLVMenuItemTestHelper.CreateGenPivotRecord(shipment, typeof(Enterprise.Integration.Customs.US.ISF.ICusISFHeader));

				uSISFMenuGroup = customsMenu.MenuItems.OfType<USISFMenuGroup>().Single();
				uSISFMenuGroup.OnPopup(EventArgs.Empty);
				openUSISFMenuItem = uSISFMenuGroup.MenuItems.OfType<OpenUSISFMenuItem>().Single();
				Assert("Show Open US ISF menuitem when there is any Gen Pivot record", openUSISFMenuItem.Visible);
			}
		}
	}
}
