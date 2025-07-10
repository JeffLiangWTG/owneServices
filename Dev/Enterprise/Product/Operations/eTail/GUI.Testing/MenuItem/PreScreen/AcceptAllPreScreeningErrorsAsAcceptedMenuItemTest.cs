using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.eTail.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.eTail.GUI.Testing
{
	public class AcceptAllPreScreeningErrorsAsAcceptedMenuItemTest : TestCaseWithFactory
	{
		public void TestMenuItemAction()
		{
			var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment3 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment3.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Failed;
			consignment2.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Failed;
			consignment3.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.FailedAcceptedAsEntered;
			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var topLevelMenu = plugin.TopLevelMenu;
				topLevelMenu.PerformSelect();

				var topLevelMenuItem = topLevelMenu.MenuItems.OfType<HVLVPreScreenMenuGroup>().Single();
				var menuItem = topLevelMenuItem.MenuItems.OfType<AcceptAllPreScreeningErrorsAsAcceptedMenuItem>().Single();

				AssertNotNull("Precondition: AcceptAllPreScreeningErrorsAsEntered menu item should be found", menuItem);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				menuItem.PerformClick();

				AssertEquals(HVLVConsignmentPreScreeningStatusCodes.Codes.FailedAcceptedAsEntered, consignment1.HVC_PreScreeningStatus);
				AssertEquals(HVLVConsignmentPreScreeningStatusCodes.Codes.FailedAcceptedAsEntered, consignment2.HVC_PreScreeningStatus);
				AssertEquals(HVLVConsignmentPreScreeningStatusCodes.Codes.FailedAcceptedAsEntered, consignment3.HVC_PreScreeningStatus);
			}
		}

		public void TestMenuItemAction_DisplayUserConfirmationPrompt()
		{
			var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_ConsigneeAddress1 = "Address";
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Failed;

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var topLevelMenu = plugin.TopLevelMenu;
				topLevelMenu.PerformSelect();

				var topLevelMenuItem = topLevelMenu.MenuItems.OfType<HVLVPreScreenMenuGroup>().Single();
				var menuItem = topLevelMenuItem.MenuItems.OfType<AcceptAllPreScreeningErrorsAsAcceptedMenuItem>().Single();

				AssertNotNull(menuItem);
				menuItem.PerformClick();

				AssertEquals("Should display a user confirmation prompt", "Click OK to proceed with updating all Consignments with FAL Pre-Screening Status to FAE: Failed Accepted as Entered", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMenuItemAction_WhenNoConsignmentsHavePrescreeningFailed()
		{
			var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_ConsigneeAddress1 = "Address";
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Passed;
			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();
				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);

				var hVLVMenu = plugin.TopLevelMenu;
				hVLVMenu.PerformSelect();
				hVLVMenu.OnPopup(EventArgs.Empty);

				var topLevelMenuItem = plugin.TopLevelMenu.MenuItems.OfType<HVLVPreScreenMenuGroup>().Single();
				var menuItem = topLevelMenuItem.MenuItems.OfType<AcceptAllPreScreeningErrorsAsAcceptedMenuItem>().Single();

				AssertNotNull(menuItem);
				menuItem.PerformClick();

				AssertEquals("There's no Consignment within this shipment containing FAL Pre-Screening Status. This action will not proceed.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMenuItemAction_WhenConsignmentsHaveNotBeenPrescreened()
		{
			var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_ConsigneeAddress1 = "Address";
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown;
			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();
				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);

				var hVLVMenu = plugin.TopLevelMenu;
				hVLVMenu.PerformSelect();
				hVLVMenu.OnPopup(EventArgs.Empty);

				var topLevelMenuItem = plugin.TopLevelMenu.MenuItems.OfType<HVLVPreScreenMenuGroup>().Single();
				var menuItem = topLevelMenuItem.MenuItems.OfType<AcceptAllPreScreeningErrorsAsAcceptedMenuItem>().Single();

				AssertNotNull(menuItem);
				menuItem.PerformClick();

				AssertEquals("HVLV Pre-Screening hasn't been run on this shipment yet. Please run Pre-Screen HVLV Details action first.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMenuItemAction_WhenAtLeastOneConsignmentHasPrescreeningFailed()
		{
			var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_ConsigneeAddress1 = "Address";
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Failed;
			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();
				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);

				var hVLVMenu = plugin.TopLevelMenu;
				hVLVMenu.PerformSelect();
				hVLVMenu.OnPopup(EventArgs.Empty);

				var topLevelMenuItem = plugin.TopLevelMenu.MenuItems.OfType<HVLVPreScreenMenuGroup>().Single();
				var menuItem = topLevelMenuItem.MenuItems.OfType<AcceptAllPreScreeningErrorsAsAcceptedMenuItem>().Single();

				AssertNotNull(menuItem);
				menuItem.PerformClick();

				AssertEquals("Should display a user confirmation prompt", "Click OK to proceed with updating all Consignments with FAL Pre-Screening Status to FAE: Failed Accepted as Entered", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
