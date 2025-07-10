using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.eTail.Business;
using Enterprise.eTail.DataTransfer.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;
using ShipmentTypes = Enterprise.Core.Constants.ShipmentTypes;
using TransportModes = Enterprise.Core.Constants.TransportModes;

namespace Enterprise.eTail.GUI.Testing
{
	public class RelatedJobCommandMenuItemTest : TestCaseWithFactory
	{
		public void TestMenuItemVisibility_EnableByDirections()
		{
			var shipment = PrepareShipmentForTesting(TransportModes.Air);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				AssertEquals("Pre-Condition: Shipment direction is unknown", Directions.Unknown, shipment.JobDirection);
				AssertMenuItemVisibility("Not supported when shipment direction is unknown", shipment, "Always Can Create Job (With Direction)", MenuItemVisbility.Invisible);

				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "USNYC";
				AssertEquals("Pre-Condition: Shipment direction is Import", Directions.Import, shipment.JobDirection);
				AssertMenuItemVisibility("visible for Login Country = US, Direction = Import, Transport Mode = Air", shipment, "Always Can Create Job (With Direction)", MenuItemVisbility.CanCreate);

				shipment.JS_RL_NKOrigin = "USNYC";
				AssertEquals("Pre-Condition: Shipment direction is Domestic", Directions.Domestic, shipment.JobDirection);
				AssertMenuItemVisibility("visible for Login Country = US, Direction = Domestic, Transport Mode = Air", shipment, "Always Can Create Job (With Direction)", MenuItemVisbility.CanCreate);

				shipment.JS_RL_NKDestination = "AUSYD";
				AssertEquals("Pre-Condition: Shipment direction is Export", Directions.Export, shipment.JobDirection);
				AssertMenuItemVisibility("visible for Login Country = US, Direction = Export, Transport Mode = Air", shipment, "Always Can Create Job (With Direction)", MenuItemVisbility.CanCreate);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.NewZealand))
			{
				AssertEquals("Pre-Condition: Shipment direction is Cross Trade when logging in from NZ", Directions.CrossTrade, shipment.JobDirection);
				AssertMenuItemVisibility("Invisible for Login Country = NZ, Direction = CrossTrade, Transport Mode = Air", shipment, "Always Can Create Job (With Direction)", MenuItemVisbility.Invisible);

				shipment.JS_RL_NKOrigin = "NZAKL";
				AssertEquals("Pre-Condition: Shipment direction is Export", Directions.Export, shipment.JobDirection);
				AssertMenuItemVisibility("visible for Login Country = NZ, Direction = Export, Transport Mode = Air", shipment, "Always Can Create Job (With Direction)", MenuItemVisbility.CanCreate);
			}
		}

		public void TestMenuItemVisibility_EnableByTransportModes()
		{
			var shipment = PrepareShipmentForTesting();
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "USNYC";
			var consol = shipment.Consols[0];

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				shipment.JS_TransportMode = TransportModes.Air;
				consol.JK_TransportMode = TransportModes.Air;
				AssertMenuItemVisibility("visible for Air", shipment, "Always Can Create Job (With Direction)", MenuItemVisbility.CanCreate);

				shipment.JS_TransportMode = TransportModes.Sea;
				consol.JK_TransportMode = TransportModes.Sea;
				AssertMenuItemVisibility("visible for Sea", shipment, "Always Can Create Job (With Direction)", MenuItemVisbility.CanCreate);

				shipment.JS_TransportMode = TransportModes.Road;
				consol.JK_TransportMode = TransportModes.Road;
				AssertMenuItemVisibility("visible for Road", shipment, "Always Can Create Job (With Direction)", MenuItemVisbility.CanCreate);

				shipment.JS_TransportMode = TransportModes.Rail;
				consol.JK_TransportMode = TransportModes.Rail;
				AssertMenuItemVisibility("Invisible for Rail", shipment, "Always Can Create Job (With Direction)", MenuItemVisbility.Invisible);
			}
		}

		public void TestMenuItemVisibility_EnableByAllowLoginToDifferentCountry()
		{
			var shipment = PrepareShipmentForTesting(TransportModes.Air);
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USNYC";

			using (HVLVDataRegistry.Instance.EnableSecurityFilingsForNonUSACompanies.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
				{
					AssertMenuItemVisibility("Pre-condition: visible when logging in to US", shipment, "Always Can Create Job (With Direction)", MenuItemVisbility.CanCreate);
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
				{
					AssertMenuItemVisibility("Invisible when logging to non-applicable country", shipment, "Always Can Create Job (With Direction)", MenuItemVisbility.Invisible);
				}
			}

			using (HVLVDataRegistry.Instance.EnableSecurityFilingsForNonUSACompanies.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				AssertMenuItemVisibility("visible when logging to non-applicable country and allow login to different country", shipment, "Always Can Create Job (With Direction)", MenuItemVisbility.CanCreate);
			}
		}

		public void TestMenuItemVisibility_EnableByDestination()
		{
			var shipment = PrepareShipmentForTesting(TransportModes.Sea);
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "USNYC";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				AssertMenuItemVisibility("Invisible when destination is not in AU", shipment, "Always Can Create Job (With Destination)", MenuItemVisbility.Invisible);

				shipment.JS_RL_NKDestination = "AUSYD";
				AssertMenuItemVisibility("Visible when destination is in AU", shipment, "Always Can Create Job (With Destination)", MenuItemVisbility.CanCreate);
			}
		}

		public void TestMenuItemVisibility_ChangeByCustomsJobExistOrNot()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipmentForCommandTesting>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_UniqueConsignRef = "TestShipment";
			shipment.JS_HouseBill = "TestShipment";
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Shipments.Add(shipment);
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_MasterBillNum = "TestConsol";

			var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_HCH_Header = consignmentHeader.PK;
			consignment.HVC_WaybillNumber = "1234567890";
			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				shipment.AllowOpen = true;
				shipment.AllowSync = true;
				AssertMenuItemVisibility("No customs job, create button is visable", shipment, "Always Enabled Job", MenuItemVisbility.CanCreate);

				shipment.HasCustomsRelatedJob = true;
				AssertMenuItemVisibility("Should show open and sync menu item when customs job exists", shipment, "Always Enabled Job", MenuItemVisbility.CanOpen | MenuItemVisbility.CanSync);
			}
		}

		public void TestMenuItemVisibility_ChangeByAllowOpenOrSync()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipmentForCommandTesting>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_UniqueConsignRef = "TestShipment";
			shipment.JS_HouseBill = "TestShipment";
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Shipments.Add(shipment);
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_MasterBillNum = "TestConsol";

			var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_HCH_Header = consignmentHeader.PK;
			consignment.HVC_WaybillNumber = "1234567890";
			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				shipment.AllowOpen = true;
				shipment.AllowSync = true;
				shipment.HasCustomsRelatedJob = true;
				AssertMenuItemVisibility("Should show open and sync menu item when customs job exists", shipment, "Always Enabled Job", MenuItemVisbility.CanOpen | MenuItemVisbility.CanSync);

				shipment.AllowOpen = false;
				AssertMenuItemVisibility("Not allow open", shipment, "Always Enabled Job", MenuItemVisbility.CanSync);

				shipment.AllowOpen = true;
				shipment.AllowSync = false;
				AssertMenuItemVisibility("Not allow sync", shipment, "Always Enabled Job", MenuItemVisbility.CanOpen);

				shipment.AllowOpen = false;
				AssertMenuItemVisibility("Not allow open or sync", shipment, "Always Enabled Job", MenuItemVisbility.Invisible);
			}
		}

		public void TestMenuItemAction()
		{
			var shipment = PrepareShipmentForTesting(TransportModes.Air);
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					AssertMenuItemVisibility("Pre-condition: Create menu item is visible", shipment, "HVLV AirCargo Report(Testing)", MenuItemVisbility.CanCreate);

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var hVLVMenu = plugin.TopLevelMenu;
					hVLVMenu.PerformSelect();

					var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var commandJobTypeMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(x => x.Caption == "HVLV AirCargo Report(Testing)");
					var menuItem = commandJobTypeMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Create HVLV AirCargo Report(Testing)");
					menuItem.PerformClick();

					var customsJob = ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity;
					customsJob.Factory.Save();
					AssertMenuItemVisibility("Menu items changed to open and sync after related customs job is created", shipment, "HVLV AirCargo Report(Testing)", MenuItemVisbility.CanOpen | MenuItemVisbility.CanSync);
				}
			}
		}

		public void TestMenuItemAction_ShouldNotifyWhenShipmentHasChanges()
		{
			var shipment = PrepareShipmentForTesting(TransportModes.Air);
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			Assert("precondition: shipment is dirty", shipment.HasChanges);

			ClickMenuItemToCreateCustomsJob_ExpectToSeeMessage(shipment, "Please save the form before converting to HVLV AirCargo Report(Testing).");
		}

		public void TestMenuItemAction_ShouldNotifyWhenShipmentHasNoConsignment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			HVLVConsignmentHeader.GetOrCreate(shipment);

			Factory.Save();

			ClickMenuItemToCreateCustomsJob_ExpectToSeeMessage(shipment, "Please make sure there's at least one active consignment on this shipment.");
		}

		public void TestMenuItemAction_ShouldNotifyWhenShipmentHasNoActiveConsignment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_HCH_Header = consignmentHeader.PK;
			consignment.HVC_IsActive = false;
			consignmentHeader.Consignments.Add(consignment);

			Factory.Save();

			ClickMenuItemToCreateCustomsJob_ExpectToSeeMessage(shipment, "Please make sure there's at least one active consignment on this shipment.");
		}

		public void TestMenuItemAction_ShouldNotifyWhenShipmentHasNoConsol()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_HCH_Header = consignmentHeader.PK;
			consignmentHeader.Consignments.Add(consignment);

			Factory.Save();

			ClickMenuItemToCreateCustomsJob_ExpectToSeeMessage(shipment, "Please make sure there is a consolidation attached to this shipment.");
		}

		public void TestMenuItemAction_WhenPreScreeningRegistryIsEnabledAndHasNoPreScreenedEvent_ShowNotificationMessage()
		{
			var shipment = PrepareShipmentForTesting(TransportModes.Air);
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";
			Factory.Save();

			var preScreeningConfiguration = new HVLVDetailsPreScreeningConfiguration() { IsEnabled = true };
			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, preScreeningConfiguration))
			{
				ClickMenuItemToCreateCustomsJob_ExpectToSeeMessage(shipment, $@"HVLV Pre-Screening has been enabled but not run on this shipment.
Proceeding with the HVLV AirCargo Report(Testing) will bypass these Pre-Screening rules, would you like to proceed? ");
			}
		}

		public void TestMenuItemAction_WhenRelatedJobExistsAndCriticalFieldsModified_ShouldNotCreateNewRelatedJob()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_HouseBill = "HouseBill001";
				shipment.JS_RL_NKOrigin = "USLAX";
				shipment.JS_RL_NKDestination = "AUSYD";

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
					var hVLVMenu = plugin.TopLevelMenu;
					hVLVMenu.PerformSelect();

					var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();

					form.FireSaveButton();
					plugin.OnSaveCompletedOrAborted(true);

					UnitTestUserNotification.Instance.AddOKAnswer();
					var commandJobTypeMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(x => x.Caption == "HVLV AirCargo Report(Testing)");
					commandJobTypeMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Create HVLV AirCargo Report(Testing)").PerformClick();

					var cargoReport = ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity as BusinessObject;
					cargoReport.Factory.Save();

					shipment.JS_HouseBill = "HouseBill002";
					plugin.OnSaving();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					var continueWithSaveResult = plugin.ShowPreSaveDialogsCore();
					AssertEquals(continueWithSaveResult, ContinueWithSave.Yes);
					Factory.Save();

					Assert("Pre-condition: cargo report is not deactivated when there is critical info changed", !((ICancellable)cargoReport).IsCancelled);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					customsMenu.OnPopup(EventArgs.Empty);
					commandJobTypeMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(x => x.Caption == "HVLV AirCargo Report(Testing)");
					commandJobTypeMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Sync HVLV AirCargo Report(Testing)").PerformClick();

					var newCargoReport = ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity as BusinessObject;
					AssertEquals("New cargo report should be not created", cargoReport.PK, newCargoReport.PK);
				}
			}
		}

		public void TestMenuItemAction_WhenPreScreeningRegistryIsEnabledAndHasPassedPreScreenedEvent_NoNotificationMessage()
		{
			var shipment = PrepareShipmentForTesting(TransportModes.Air);
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";
			Factory.Save();

			var preScreeningConfiguration = new HVLVDetailsPreScreeningConfiguration() { IsEnabled = true };
			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, preScreeningConfiguration))
			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();
				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);

				var topLevelMenu = plugin.TopLevelMenu;
				topLevelMenu.PerformSelect();
				var preScreenMenu = topLevelMenu.MenuItems.OfType<HVLVPreScreenMenuGroup>().Single();
				var preScreenMenuItem = preScreenMenu.MenuItems.OfType<PreScreenMenuItem>().Single();
				preScreenMenuItem.OnPopup(EventArgs.Empty);

				AssertNotNull(preScreenMenuItem);

				preScreenMenuItem.PerformClick();
				Factory.Save();

				var hVLVMenu = plugin.TopLevelMenu;
				hVLVMenu.PerformSelect();

				var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				customsMenu.OnPopup(EventArgs.Empty);

				var commandJobTypeMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(x => x.Caption == "HVLV AirCargo Report(Testing)");
				commandJobTypeMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Create HVLV AirCargo Report(Testing)").PerformClick();

				var notificationMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertNullOrEmpty("HVLV Pre-Screening has been enabled to run on this shipment", notificationMessage);
			}
		}

		ForwardingShipment PrepareShipmentForTesting(string transportMode = null)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var testShipment = consol.Shipments.AddNew();
			testShipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			testShipment.JS_UniqueConsignRef = "TestShipment";
			testShipment.JS_HouseBill = "TestShipment";

			if (!string.IsNullOrEmpty(transportMode))
			{
				consol.JK_TransportMode = transportMode;
				consol.JK_MasterBillNum = "TestConsol";
				testShipment.JS_TransportMode = transportMode;
			}

			var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(testShipment);

			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = testShipment.PK;
			consignment.HVC_HCH_Header = consignmentHeader.PK;
			consignment.HVC_WaybillNumber = "1234567890";
			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = testShipment.PK;

			return testShipment;
		}

		void AssertMenuItemVisibility(string assertionDetails, ForwardingShipment shipment, string relatedJobName, MenuItemVisbility menuItemVisbility)
		{
			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var hVLVMenu = plugin.TopLevelMenu;
				hVLVMenu.PerformSelect();

				var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				var commandJobTypeMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(x => x.Caption == relatedJobName);
				var relatedJobMenuItems = commandJobTypeMenuGroup?.MenuItems.OfType<ZMenuItem>().Where(m => m.Caption.GetUnresolvedString().Contains(relatedJobName));
				commandJobTypeMenuGroup?.OnPopup(EventArgs.Empty);

				if (menuItemVisbility == MenuItemVisbility.Invisible)
				{
					Assert($"None of the menu items for {relatedJobName} should be visible.\r\nAssertion details: {assertionDetails}", relatedJobMenuItems == null || relatedJobMenuItems.All(m => !m.Visible));
				}
				else if (menuItemVisbility.HasFlag(MenuItemVisbility.CanCreate))
				{
					CombineAssertions($"Create {relatedJobName} should be visible.\r\nAssertion details: {assertionDetails}", () =>
					{
						Assert("Create menu item is visible", relatedJobMenuItems.FirstOrDefault(m => m.Caption == $"Create {relatedJobName}").Visible);
						Assert("Open menu item is not visible\"", !relatedJobMenuItems.FirstOrDefault(m => m.Caption == $"Open {relatedJobName}").Visible);
						Assert("Sync menu item is not visible\"", !relatedJobMenuItems.FirstOrDefault(m => m.Caption == $"Sync {relatedJobName}").Visible);
					});
				}
				else
				{
					Assert("Create menu item should be invisivble when open or sync is visible.", !relatedJobMenuItems.FirstOrDefault(m => m.Caption == $"Create {relatedJobName}").Visible);
					if (menuItemVisbility.HasFlag(MenuItemVisbility.CanOpen))
					{
						Assert("Open menu item should be visible", relatedJobMenuItems.FirstOrDefault(m => m.Caption == $"Open {relatedJobName}").Visible);
					}
					if (menuItemVisbility.HasFlag(MenuItemVisbility.CanSync))
					{
						Assert("Sync menu item should be visible", relatedJobMenuItems.FirstOrDefault(m => m.Caption == $"Sync {relatedJobName}").Visible);
					}
				}
			}
		}

		void ClickMenuItemToCreateCustomsJob_ExpectToSeeMessage(ForwardingShipment shipment, string expectedMessage)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var hVLVMenu = plugin.TopLevelMenu;
				hVLVMenu.PerformSelect();

				var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				customsMenu.OnPopup(EventArgs.Empty);

				var commandJobTypeMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(x => x.Caption == "HVLV AirCargo Report(Testing)");
				commandJobTypeMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Create HVLV AirCargo Report(Testing)").PerformClick();

				var notificationMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals(expectedMessage, notificationMessage);
			}
		}
	}

	enum MenuItemVisbility
	{
		Invisible = 0,
		CanCreate = 1,
		CanOpen = 2,
		CanSync = 4
	}
}
