using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.GUI.Testing
{
	public class OpenCustomsCargoReportMenuItemTest : TestCaseWithFactory
	{
		public void TestMenuItemVisibility()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, true);
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKDestination = "USLAX";

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var hVLVMenu = plugin.TopLevelMenu;
					hVLVMenu.PerformSelect();

					var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					HVLVMenuItemTestHelper.CreateGenPivotRecord(shipment);
					shipment.Logs.AddNew(AutoEvents.HVLVReady, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, EventReferenceParameterReasons.CargoReportCreated));
					Factory.Save();

					var commandJobTypeMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Low Value Entries");
					commandJobTypeMenuGroup.OnPopup(EventArgs.Empty);
					CombineAssertions("CargoReport has been created and has not been modify", () =>
					{
						Assert("Precondition : CargoReport has been created", shipment.IsCargoReportCreated());
						var createCargoReportMenuItem = commandJobTypeMenuGroup.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menuItem => menuItem.Caption == "Create Low Value Entries");
						Assert("Create menu item should be hidden", !createCargoReportMenuItem.Visible);
						var syncCargoReportMenuItem = commandJobTypeMenuGroup.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menuItem => menuItem.Caption == "Sync Low Value Entries");
						Assert("Sync menu item should be visible regardless the HLR event", syncCargoReportMenuItem.Visible);
						var openCargoReportMenuItem = commandJobTypeMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Open Low Value Entries");
						Assert("Open menu item should be visible regardless the HLR event", openCargoReportMenuItem.Visible);
					});

					shipment.Logs.AddNew(AutoEvents.HVLVReady, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, EventReferenceParameterReasons.CargoReportAmended));
					Factory.Save();
					customsMenu.OnPopup(EventArgs.Empty);
					commandJobTypeMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Low Value Entries");
					commandJobTypeMenuGroup.OnPopup(EventArgs.Empty);

					CombineAssertions("CargoReport has been created and has been amend", () =>
					{
						Assert("Precondition : CargoReport has been created", shipment.IsCargoReportCreated());
						var createCargoReportMenuItem = commandJobTypeMenuGroup.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menuItem => menuItem.Caption == "Create Low Value Entries");
						Assert("Create menu item should be hidden", !createCargoReportMenuItem.Visible);
						var syncCargoReportMenuItem = commandJobTypeMenuGroup.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menuItem => menuItem.Caption == "Sync Low Value Entries");
						Assert("Sync menu item should be visible regardless the HLR event", syncCargoReportMenuItem.Visible);
						var openCargoReportMenuItem = commandJobTypeMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Open Low Value Entries");
						Assert("Open menu item should be visible regardless the HLR event", openCargoReportMenuItem.Visible);
					});
				}
			}
		}

		public void TestMenuVisibility_US_ShipmentWithUSDestination_IsVisible()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
				shipment.JS_RL_NKDestination = "USLAX";

				HVLVMenuItemTestHelper.AssertCargoReportMenuItemVisibility(true, "Low Value Entries", "Open", shipment, HVLVMenuItemTestHelper.CargoReportAction.Open);
			}
		}

		public void TestMenuVisibility_US_ShipmentWithNonUSDestination_IsHidden()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
				shipment.JS_RL_NKDestination = "AUSYD";

				HVLVMenuItemTestHelper.AssertCargoReportMenuItemVisibility(false, "Low Value Entries", "Open", shipment, HVLVMenuItemTestHelper.CargoReportAction.Open);
			}
		}

		public void TestMenuItemVisibility_AU_Export_IsHidden()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";

				AssertEquals("Precondition:", Directions.Export, shipment.JobDirection);

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var hVLVMenu = plugin.TopLevelMenu;
					hVLVMenu.PerformSelect();

					var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					customsMenu.OnPopup(EventArgs.Empty);

					var menuItems = customsMenu.MenuItems.Cast<ZMenuItem>();
					var openMenuItem = menuItems.OfType<ZMenuItem>().SingleOrDefault(menu => menu.Caption == "Open HVLV SeaCargo Report");
					Assert("Menu item is hidden for AU export", openMenuItem == null || !openMenuItem.Visible);
				}
			}
		}

		public void TestMenuItemVisibility_SG_ModeIsRoad_IsVisible()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Singapore))
			{
				var sgRegistry = ObjectFactory.Get<Enterprise.Integration.Customs.SG.ISGCustomsRegistry>();
				sgRegistry.ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
				shipment.JS_TransportMode = TransportModes.Road;
				HVLVMenuItemTestHelper.AssertCargoReportMenuItemVisibility(true, "SG ACCESS Export Manifest", "Open", shipment, HVLVMenuItemTestHelper.CargoReportAction.Open);
			}
		}

		public void TestMenuItemVisibility_SG_ModeIsAir_IsVisible()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Singapore))
			{
				var sgRegistry = ObjectFactory.Get<Enterprise.Integration.Customs.SG.ISGCustomsRegistry>();
				sgRegistry.ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
				shipment.JS_TransportMode = TransportModes.Air;
				HVLVMenuItemTestHelper.AssertCargoReportMenuItemVisibility(true, "SG ACCESS Export Manifest", "Open", shipment, HVLVMenuItemTestHelper.CargoReportAction.Open);
			}
		}

		public void TestMenuItemVisibility_SG_ModeIsNotRoadOrAir_IsHidden()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Singapore))
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
				shipment.JS_TransportMode = TransportModes.Sea;
				HVLVMenuItemTestHelper.AssertCargoReportMenuItemVisibility(false, "SG ACCESS Export Manifest", "Open", shipment, HVLVMenuItemTestHelper.CargoReportAction.Open);
			}
		}

		public void TestWarningWhenCargoReportNotFoundForOpen_US()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKDestination = "USLAX";
				Factory.Save();

				HVLVMenuItemTestHelper.AssertErrorMessageWhenCannotFindCargoReport("Low Value Entries", "Open", shipment, "No Low Value Entries has been created for Shipment " + shipment.JS_UniqueConsignRef);
			}
		}

		public void TestOpenLowValueEntries_WhenRoadHVLShipmentHasCreatedLowValueEntries_CanBeFoundForOpen_US()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_MasterBillNum = "08138374491";

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Road;
				shipment.JS_HouseBill = "UWM97N872947";
				shipment.JS_UniqueConsignRef = "S0000001";
				shipment.JS_RL_NKDestination = "USCHI";

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				consol.Shipments.Add(shipment);

				var item = Factory.NewWithValidTestData<HVLVItem>();
				item.HVI_JS_LoadedOnShipment = shipment.PK;
				var consignment = item.Consignment;
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_HCH_Header = consignmentHeader.PK;

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();
					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);

					var hVLVMenu = plugin.TopLevelMenu;
					hVLVMenu.PerformSelect();

					var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();

					customsMenu.OnPopup(EventArgs.Empty);
					var commandJobTypeMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Low Value Entries");

					UnitTestUserNotification.Instance.AddOKAnswer();
					commandJobTypeMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Create Low Value Entries").PerformClick();

					var cargoReportForm = ZFormModaliser.LastFormShownForTest as ZForm;
					var cargoReport = cargoReportForm.BusinessEntity as CusUSLVClearance;

					cargoReport.Factory.Save();
					cargoReportForm.Close();

					customsMenu.OnPopup(EventArgs.Empty);
					UnitTestUserNotification.Instance.AddOKAnswer();
					commandJobTypeMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Low Value Entries");
					commandJobTypeMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Open Low Value Entries").PerformClick();

					cargoReportForm = ZFormModaliser.LastFormShownForTest as ZForm;
					cargoReport = cargoReportForm.BusinessEntity as CusUSLVClearance;

					AssertNotNull(cargoReportForm);
					AssertNotNull(cargoReport);
					AssertType<CusUSLVClearance>(cargoReport);

					cargoReportForm.Close();
				}
			}
		}

		public void TestOpenSGAccessManifest()
		{
			var sgRegistry = ObjectFactory.Get<Enterprise.Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable as BooleanRegistryItem;
			using (sgRegistry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Singapore))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_MasterBillNum = "08138374491";

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_HouseBill = "UWM97N872947";
				shipment.JS_UniqueConsignRef = "S0000001";

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				consol.Shipments.Add(shipment);

				var item = Factory.NewWithValidTestData<HVLVItem>();
				item.HVI_JS_LoadedOnShipment = shipment.PK;
				var consignment = item.Consignment;
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_HCH_Header = consignmentHeader.PK;

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var hVLVMenu = plugin.TopLevelMenu;
					hVLVMenu.PerformSelect();

					var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					customsMenu.OnPopup(EventArgs.Empty);

					var commandJobTypeMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "SG ACCESS Export Manifest");
					var createMenuItem = commandJobTypeMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Create SG ACCESS Export Manifest");
					UnitTestUserNotification.Instance.AddOKAnswer();
					createMenuItem.PerformClick();

					var cargoReportForm = ZFormModaliser.LastFormShownForTest as ZForm;
					var cargoReport = cargoReportForm.BusinessEntity;

					cargoReport.Factory.Save();
					cargoReportForm.Close();

					customsMenu.OnPopup(EventArgs.Empty);
					UnitTestUserNotification.Instance.AddOKAnswer();
					commandJobTypeMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "SG ACCESS Export Manifest");
					var openMenuItem = commandJobTypeMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Open SG ACCESS Export Manifest");
					openMenuItem.PerformClick();

					cargoReportForm = ZFormModaliser.LastFormShownForTest as ZForm;
					cargoReport = cargoReportForm.BusinessEntity;

					AssertNotNull(cargoReportForm);
					AssertNotNull(cargoReport);
					var expectedType = ObjectFactory.GetType<Enterprise.Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
					AssertEquals($"cargoReport should be of type {expectedType.FullName}", expectedType, cargoReport?.GetType());
					cargoReportForm.Close();
				}
			}
		}

		public void TestWarningWhenCargoReportNotFoundForOpen_AU_Import()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
				shipment.JS_RL_NKOrigin = "USLAX";
				shipment.JS_RL_NKDestination = "AUSYD";
				Factory.Save();

				AssertEquals("Precondition:", Directions.Import, shipment.JobDirection);

				CombineAssertions("AU Cargo Report Menu Item", () =>
				{
					AssertErrorMessageWhenCannotFindCargoReportDependOnTransportMode_AU(TransportModes.Air, "No HVLV AirCargo Report has been created for Shipment " + shipment.JS_UniqueConsignRef);
					AssertErrorMessageWhenCannotFindCargoReportDependOnTransportMode_AU(TransportModes.Sea, "No HVLV SeaCargo Report has been created for Shipment " + shipment.JS_UniqueConsignRef);
				});

				void AssertErrorMessageWhenCannotFindCargoReportDependOnTransportMode_AU(string transportMode, string errorMessage)
				{
					shipment.JS_TransportMode = transportMode;
					HVLVMenuItemTestHelper.AssertErrorMessageWhenCannotFindCargoReport(string.Format("HVLV {0} Report", transportMode == "AIR" ? "AirCargo" : "SeaCargo"), "Open", shipment, errorMessage);
				}
			}
		}

		public void TestWarningWhenCargoReportNotFoundForOpen_NZ_Import()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.NewZealand))
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
				shipment.JS_RL_NKOrigin = "USMEM";
				shipment.JS_RL_NKDestination = "NZABY";
				Factory.Save();

				CombineAssertions("NZ Cargo Report Menu Item", () =>
				{
					AssertErrorMessageWhenCannotFindCargoReportDependOnTransportMode_NZ(TransportModes.Air, "No HVLV AirCargo ICR has been created for Shipment " + shipment.JS_UniqueConsignRef);
					AssertErrorMessageWhenCannotFindCargoReportDependOnTransportMode_NZ(TransportModes.Sea, "No HVLV SeaCargo ICR has been created for Shipment " + shipment.JS_UniqueConsignRef);
				});

				void AssertErrorMessageWhenCannotFindCargoReportDependOnTransportMode_NZ(string transportMode, string expectErrorMessage)
				{
					shipment.JS_TransportMode = transportMode;
					HVLVMenuItemTestHelper.AssertErrorMessageWhenCannotFindCargoReport(string.Format("HVLV {0} ICR", transportMode == "AIR" ? "AirCargo" : "SeaCargo"), "Open", shipment, expectErrorMessage);
				}
			}
		}

		public void TestWarningWhenCargoReportNotFoundForOpen_NZ_Export()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.NewZealand))
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
				shipment.JS_RL_NKOrigin = "NZABY";
				shipment.JS_RL_NKDestination = "USLAX";
				Factory.Save();

				CombineAssertions("NZ Cargo Report Menu Item", () =>
				{
					AssertErrorMessageWhenCannotFindCargoReportDependOnTransportMode_NZ(TransportModes.Air, "No HVLV AirCargo CRE has been created for Shipment " + shipment.JS_UniqueConsignRef);
					AssertErrorMessageWhenCannotFindCargoReportDependOnTransportMode_NZ(TransportModes.Sea, "No HVLV SeaCargo CRE has been created for Shipment " + shipment.JS_UniqueConsignRef);
				});

				void AssertErrorMessageWhenCannotFindCargoReportDependOnTransportMode_NZ(string transportMode, string expectErrorMessage)
				{
					shipment.JS_TransportMode = transportMode;
					HVLVMenuItemTestHelper.AssertErrorMessageWhenCannotFindCargoReport(string.Format("HVLV {0} CRE", transportMode == "AIR" ? "AirCargo" : "SeaCargo"), "Open", shipment, expectErrorMessage);
				}
			}
		}

		public void TestWarningWhenCargoReportNotFoundForOpen_SG()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Singapore))
			{
				var sgRegistry = ObjectFactory.Get<Enterprise.Integration.Customs.SG.ISGCustomsRegistry>();
				sgRegistry.ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
				shipment.JS_TransportMode = TransportModes.Air;
				Factory.Save();
				HVLVMenuItemTestHelper.AssertErrorMessageWhenCannotFindCargoReport("SG ACCESS Export Manifest", "Open", shipment, "No SG ACCESS Export Manifest has been created for Shipment " + shipment.JS_UniqueConsignRef);
			}
		}

		public void TestMenuItemAction_ShouldNotUpdateCargoReportData()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_MasterBillNum = "08138374491";

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_HouseBill = "UWM97N872947";
				shipment.JS_UniqueConsignRef = "S0000001";
				shipment.JS_RL_NKDestination = "USCHI";

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				consol.Shipments.Add(shipment);

				var item = Factory.NewWithValidTestData<HVLVItem>();
				item.HVI_JS_LoadedOnShipment = shipment.PK;
				var consignment = item.Consignment;
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_HCH_Header = consignmentHeader.PK;

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var hVLVMenu = plugin.TopLevelMenu;
					hVLVMenu.PerformSelect();

					var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					customsMenu.OnPopup(EventArgs.Empty);
					var commandJobTypeMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Low Value Entries");

					AssertEquals("Precondition : now shipment has 1 consignments", 1, shipment.HVLVConsignments.Count());

					UnitTestUserNotification.Instance.AddOKAnswer();
					commandJobTypeMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Create Low Value Entries").PerformClick();

					var cargoReportForm = ZFormModaliser.LastFormShownForTest as ZForm;
					var cargoReport = cargoReportForm.BusinessEntity as CusUSLVClearance;
					AssertEquals("CargoReport has only 1 housebill", 1, cargoReport.CusUSLVConsignments.Count);

					cargoReport.Factory.Save();
					cargoReportForm.Close();

					var item2 = Factory.NewWithValidTestData<HVLVItem>();
					item2.HVI_JS_LoadedOnShipment = shipment.PK;
					var consignment2 = item2.Consignment;
					consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;

					Factory.Save();
					consignmentHeader.ConsignmentsFilteredView.Reload(true);

					AssertEquals("Precondition : now shipment has 2 consignments", 2, shipment.HVLVConsignments.Count());

					customsMenu.OnPopup(EventArgs.Empty);
					UnitTestUserNotification.Instance.AddOKAnswer();

					commandJobTypeMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Low Value Entries");
					commandJobTypeMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Open Low Value Entries").PerformClick();

					cargoReportForm = ZFormModaliser.LastFormShownForTest as ZForm;
					cargoReport = cargoReportForm.BusinessEntity as CusUSLVClearance;
					AssertEquals("open cargo report would not update house bill", 1, cargoReport.CusUSLVConsignments.Count);

					cargoReportForm.Close();
				}
			}
		}
	}
}
