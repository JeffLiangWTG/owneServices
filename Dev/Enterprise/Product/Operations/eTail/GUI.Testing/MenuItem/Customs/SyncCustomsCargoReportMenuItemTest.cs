using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs.NZ;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.eTail.GUI.Testing
{
	public class SyncCustomsCargoReportMenuItemTest : TestCaseWithFactory
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
					shipment.Logs.AddNew(AutoEvents.HVLVReady, new KeyValuePair<string, string>(EventReferenceParameters.Codes.Reason, EventReferenceParameterReasons.CargoReportCreated));
					Factory.Save();
					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Low Value Entries");
					jobCommandMenuGroup.OnPopup(EventArgs.Empty);

					CombineAssertions("CargoReport has been created and has not been amend", () =>
					{
						Assert("Precondition : CargoReport has been created", shipment.IsCargoReportCreated());
						var createCargoReportMenuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menuItem => menuItem.Caption == "Create Low Value Entries");
						Assert("Create menu item should be hidden", !createCargoReportMenuItem.Visible);
						var syncCargoReportMenuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Sync Low Value Entries");
						Assert("Sync menu item should be visible regardless the HLR event", syncCargoReportMenuItem.Visible);
						var openCargoReportMenuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menuItem => menuItem.Caption == "Open Low Value Entries");
						Assert("Open menu item should be visible regardless the HLR event", openCargoReportMenuItem.Visible);
					});
				}
			}
		}

		public void TestMenuItemVisibility_AU_Export_IsHidden()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
				shipment.JS_TransportMode = TransportModes.Sea;
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

					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().FirstOrDefault(menu => menu.Caption == "HVLV SeaCargo Report");
					var menuItems = jobCommandMenuGroup?.MenuItems.Cast<ZMenuItem>();
					var syncMenuItem = menuItems?.OfType<ZMenuItem>().SingleOrDefault(menuItem => menuItem.Caption == "Sync HVLV SeaCargo Report");
					AssertEquals("Menu item is hidden for AU export", false, syncMenuItem != null);
				}
			}
		}

		public void TestMenuVisibility_US_ShipmentWithUSDestination_IsVisible()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
				shipment.JS_RL_NKDestination = "USLAX";

				HVLVMenuItemTestHelper.AssertCargoReportMenuItemVisibility(true, "Low Value Entries", "Sync", shipment, HVLVMenuItemTestHelper.CargoReportAction.Sync);
			}
		}

		public void TestMenuVisibility_US_ShipmentWithUSDestination_ModeIsAir_IsVisible()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_TransportMode = TransportModes.Air;

				HVLVMenuItemTestHelper.CreateGenPivotRecord(shipment, typeof(Enterprise.Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader));
				HVLVMenuItemTestHelper.AssertCargoReportMenuItemVisibility(true, "US Air AMS (Import)", "Sync", shipment, HVLVMenuItemTestHelper.CargoReportAction.Sync);
			}
		}

		public void TestMenuVisibility_US_ShipmentWithUSDestination_ModeIsSea_IsVisible()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_TransportMode = TransportModes.Sea;

				HVLVMenuItemTestHelper.CreateGenPivotRecord(shipment, typeof(Enterprise.Integration.Customs.US.USAMS.ICusInBondHeader));
				HVLVMenuItemTestHelper.AssertCargoReportMenuItemVisibility(true, "US Sea AMS (Import)", "Sync", shipment, HVLVMenuItemTestHelper.CargoReportAction.Sync);
			}
		}

		public void TestMenuVisibility_US_ShipmentWithNonUSDestination_IsHidden()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
				shipment.JS_RL_NKDestination = "AUSYD";

				HVLVMenuItemTestHelper.AssertCargoReportMenuItemVisibility(false, "Low Value Entries", "Sync", shipment, HVLVMenuItemTestHelper.CargoReportAction.Sync);
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
				HVLVMenuItemTestHelper.AssertCargoReportMenuItemVisibility(true, "SG ACCESS Export Manifest", "Sync", shipment, HVLVMenuItemTestHelper.CargoReportAction.Sync);
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
				HVLVMenuItemTestHelper.AssertCargoReportMenuItemVisibility(true, "SG ACCESS Export Manifest", "Sync", shipment, HVLVMenuItemTestHelper.CargoReportAction.Sync);
			}
		}

		public void TestMenuItemVisibility_TW_ModeIsNotSeaOrAir_IsHidden()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
				shipment.JS_TransportMode = TransportModes.Road;
				shipment.JS_RL_NKOrigin = "SGSIN";
				shipment.JS_RL_NKDestination = "TWACH";
				HVLVMenuItemTestHelper.AssertCargoReportMenuItemVisibility(false, "TW Forwarder Manifest (Import)", "Sync", shipment, HVLVMenuItemTestHelper.CargoReportAction.Sync);
			}
		}

		public void TestMenuItemVisibility_TW_ModeIsNotImportOrExport_IsHidden()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKOrigin = "SGSIN";
				shipment.JS_RL_NKDestination = "AUSYD";
				HVLVMenuItemTestHelper.AssertCargoReportMenuItemVisibility(false, "TW Forwarder Manifest (Import)", "Sync", shipment, HVLVMenuItemTestHelper.CargoReportAction.Sync);
			}
		}

		public void TestMenuItemVisibility_TW_ModeIsDomestic_IsHidden()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKOrigin = "TWACH";
				shipment.JS_RL_NKDestination = "TWANG";
				HVLVMenuItemTestHelper.AssertCargoReportMenuItemVisibility(false, "TW Forwarder Manifest (Import)", "Sync", shipment, HVLVMenuItemTestHelper.CargoReportAction.Sync);
			}
		}

		public void TestMenuItemVisibility_TW_ModeIsAir_IsVisible()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKOrigin = "SGSIN";
				shipment.JS_RL_NKDestination = "TWACH";
				HVLVMenuItemTestHelper.AssertCargoReportMenuItemVisibility(true, "TW Forwarder Manifest (Import)", "Sync", shipment, HVLVMenuItemTestHelper.CargoReportAction.Sync);
			}
		}

		public void TestMenuItemVisibility_TW_ModeIsSea_IsVisible()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_RL_NKOrigin = "SGSIN";
				shipment.JS_RL_NKDestination = "TWACH";
				HVLVMenuItemTestHelper.AssertCargoReportMenuItemVisibility(true, "TW Forwarder Manifest (Import)", "Sync", shipment, HVLVMenuItemTestHelper.CargoReportAction.Sync);
			}
		}

		public void TestMenuItemAction_WhenConsolFlightorVesselHasChanges_DisplayPrompt()
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
				var hVLVMenu = plugin.TopLevelMenu;
				hVLVMenu.PerformSelect();

				var customsMenu = hVLVMenu.MenuItems.Cast<MenuItem>().SingleOrDefault(x => x.Text == HVLVMenuItemHelper.Captions.NavigateToCustoms);
				var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menu => menu.Caption == "HVLV AirCargo Report");

				UnitTestUserNotification.Instance.AddOKAnswer();
				jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Create HVLV AirCargo Report").PerformClick();

				var cargoReport = ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity;
				cargoReport.Factory.Save();
			}

			using (var form = new ConsolForm(consol))
			{
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);

				consol.Transports.MostInterestingTransport.JW_VoyageFlight = "Flight101";

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				var showPreSaveDialogForm = (IShowPreSaveDialog)form;
				var continueWithSaveResult = showPreSaveDialogForm.ShowPreSaveDialogs();

				AssertEquals(continueWithSaveResult, ContinueWithSave.Yes);
				AssertEquals(string.Format("Customs job(s) have already been created for the following shipment(s), saving the consolidation may negatively affect existing Customs job(s) due to primary field(s) update.\r\n{0}", shipment.JS_UniqueConsignRef), UnitTestUserNotification.Instance.PreviousMessages[0].Text);
			}
		}

		public void TestMenuItemAction_WhenConsolMasterBillNumberHasChanges_DisplayPrompt()
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
					var hVLVMenu = plugin.TopLevelMenu;
					hVLVMenu.PerformSelect();

					var customsMenu = hVLVMenu.MenuItems.Cast<MenuItem>().SingleOrDefault(x => x.Text == HVLVMenuItemHelper.Captions.NavigateToCustoms);
					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menu => menu.Caption == "HVLV AirCargo Report");

					UnitTestUserNotification.Instance.AddOKAnswer();
					jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Create HVLV AirCargo Report").PerformClick();

					var cargoReport = ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity;
					cargoReport.Factory.Save();
				}

				using (var form = new ConsolForm(consol))
				{
					Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);

					consol.JK_MasterBillNum = "MAWB5678";
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

					var showPreSaveDialogForm = (IShowPreSaveDialog)form;
					var continueWithSaveResult = showPreSaveDialogForm.ShowPreSaveDialogs();

					AssertEquals(continueWithSaveResult, ContinueWithSave.Yes);
					AssertEquals(string.Format("Customs job(s) have already been created for the following shipment(s), saving the consolidation may negatively affect existing Customs job(s) due to primary field(s) update.\r\n{0}", shipment.JS_UniqueConsignRef), UnitTestUserNotification.Instance.PreviousMessages[0].Text);
				}
			}
		}

		public void TestMenuItemAction_WhenConsignmentWaybillHasNoSCACCodeAndIsOver12Characters_ShowsMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.Shipments.Add(shipment);

				var sendingAgentOrg = Factory.NewWithValidTestData<OrgHeader>();
				var cusCode = sendingAgentOrg.ConfigOrg.CustomsCodes.AddNew();
				cusCode.OK_CodeType = "CCC";
				cusCode.OK_CustomsRegNo = "ABCD";
				cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;

				consol.JK_OA_SendingForwarderAddress = sendingAgentOrg.MainAddress.PK;

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);
				var consignment_WithWaybillLength13 = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment_WithWaybillLength13.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment_WithWaybillLength13.HVC_WaybillNumber = "1234567890123";
				consignment_WithWaybillLength13.HVC_HCH_Header = consignmentHeader.PK;
				consignment_WithWaybillLength13.Items.AddNew();

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var hVLVMenu = plugin.TopLevelMenu;
					hVLVMenu.PerformSelect();

					var customsMenu = hVLVMenu.MenuItems.Cast<MenuItem>().SingleOrDefault(x => x.Text == HVLVMenuItemHelper.Captions.NavigateToCustoms);

					UnitTestUserNotification.Instance.AddOKAnswer();
					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menu => menu.Caption == "Low Value Entries");
					jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Create Low Value Entries").PerformClick();

					var uslvClearance = ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity;
					uslvClearance.Factory.Save();

					consignment_WithWaybillLength13.HVC_WaybillNumber = "ABCD56789091234567";

					Factory.Save();

					customsMenu.OnPopup(EventArgs.Empty);
					jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menu => menu.Caption == "Low Value Entries");
					jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Sync Low Value Entries").PerformClick();

					var waybillValidationErrorMessage = UnitTestUserNotification.Instance.LastMessage.Text;
					AssertEquals("There are waybill(s) that exceed 12 in length, proceeding may cause message errors on Low Value Entries.\r\nWould you like to proceed?", waybillValidationErrorMessage);
				}
			}
		}

		public void TestMenuItemAction_WhenConsignmentWaybillHasSCACCodeAndIsOver16Characters_ShowsMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.Shipments.Add(shipment);

				var sendingAgentOrg = Factory.NewWithValidTestData<OrgHeader>();
				var cusCode = sendingAgentOrg.ConfigOrg.CustomsCodes.AddNew();
				cusCode.OK_CodeType = "CCC";
				cusCode.OK_CustomsRegNo = "ABCD";
				cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;

				consol.JK_OA_SendingForwarderAddress = sendingAgentOrg.MainAddress.PK;

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);
				var consignment_WithWaybillLength17 = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment_WithWaybillLength17.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment_WithWaybillLength17.HVC_WaybillNumber = "1234567890123";
				consignment_WithWaybillLength17.HVC_HCH_Header = consignmentHeader.PK;
				consignment_WithWaybillLength17.Items.AddNew();

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var hVLVMenu = plugin.TopLevelMenu;
					hVLVMenu.PerformSelect();

					var customsMenu = hVLVMenu.MenuItems.Cast<MenuItem>().SingleOrDefault(x => x.Text == HVLVMenuItemHelper.Captions.NavigateToCustoms);

					UnitTestUserNotification.Instance.AddOKAnswer();
					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menu => menu.Caption == "Low Value Entries");
					jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Create Low Value Entries").PerformClick();

					var uslvClearance = ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity;
					uslvClearance.Factory.Save();

					consignment_WithWaybillLength17.HVC_WaybillNumber = "ABCD56789091234567";

					Factory.Save();

					customsMenu.OnPopup(EventArgs.Empty);
					jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menu => menu.Caption == "Low Value Entries");
					jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Sync Low Value Entries").PerformClick();

					var waybillValidationErrorMessage = UnitTestUserNotification.Instance.LastMessage.Text;
					AssertEquals("There are waybill(s) that exceed 12 in length, proceeding may cause message errors on Low Value Entries.\r\nWould you like to proceed?", waybillValidationErrorMessage);
				}
			}
		}

		public void TestMenuItemAction_WhenShipmentHouseBillHasChanges_DisplayPrompt()
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
					var hVLVMenu = plugin.TopLevelMenu;
					hVLVMenu.PerformSelect();

					var customsMenu = hVLVMenu.MenuItems.Cast<MenuItem>().SingleOrDefault(x => x.Text == HVLVMenuItemHelper.Captions.NavigateToCustoms);

					UnitTestUserNotification.Instance.AddOKAnswer();
					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menu => menu.Caption == "HVLV AirCargo Report");
					jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Create HVLV AirCargo Report").PerformClick();

					var cargoReport = ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity;
					cargoReport.Factory.Save();

					var list = shipment.Factory.GetCachedValue("ETailShipmentPlugin|ShipmentPKsWithPrimaryFieldChanges", () => new List<ZGuid>());
					list.Add(shipment.PK);

					shipment.JS_HouseBill = "MAWB5678";
					plugin.ShowPreSaveDialogs();

					AssertEquals("Customs job(s) have already been created, saving the shipment may negatively affect existing Customs job(s) due to primary field(s) update.", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
				}
			}
		}

		public void TestMenuItemAction_WhenCreateNewHouseBillFromAddedConsignment_ShouldAddToHouseBillsCollection_AUSeaCargoReport()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_HouseBill = "HouseBill001";
				shipment.JS_RL_NKDestination = "AUSYD";
				shipment.JS_RL_NKOrigin = "USLAX";

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Sea;
				consol.JK_MasterBillNum = "MAWB1234";
				consol.Transports.MostInterestingTransport.JW_VoyageFlight = "Flight100";
				consol.Shipments.Add(shipment);
				var container = consol.Containers.AddNew();
				container.ContainerNumberForBinding = "Container001";

				foreach (var hvlvItem in shipment.HVLVItems)
				{
					hvlvItem.HVI_ContainerNumber = container.ContainerNumberForBinding;
				}

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var hVLVMenu = plugin.TopLevelMenu;
					hVLVMenu.PerformSelect();

					var customsMenu = hVLVMenu.MenuItems.Cast<MenuItem>().SingleOrDefault(x => x.Text == HVLVMenuItemHelper.Captions.NavigateToCustoms);

					UnitTestUserNotification.Instance.AddOKAnswer();
					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menu => menu.Caption == "HVLV SeaCargo Report");
					jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Create HVLV SeaCargo Report").PerformClick();

					var cargoReport = ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity as CusSCAOceanBill;
					cargoReport.Factory.Save();

					CombineAssertions("pre-condition", () =>
					{
						AssertEquals("Collection count", 1, cargoReport.HouseBills.Count);
						AssertContainsExactElementsInAnyOrder("CA_HouseBill", new[] { "1234567890" }, cargoReport.HouseBills.OfType<CusSCAHouse>().Select(house => house.CA_HouseBill));
					});

					var header = shipment.GetOrCreateHVLVConsignmentHeader();
					var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
					consignment.HVC_HCH_Header = header.PK;
					consignment.HVC_WaybillNumber = "AddedConsignment";
					var item = consignment.Items.AddNew();
					item.HVI_ContainerNumber = container.ContainerNumberForBinding;

					Factory.Save();

					customsMenu.OnPopup(EventArgs.Empty);
					jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menu => menu.Caption == "HVLV SeaCargo Report");
					jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Sync HVLV SeaCargo Report").PerformClick();
					cargoReport = ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity as CusSCAOceanBill;

					CombineAssertions("HouseBills collection should contain 2 house bills after synchronisation", () =>
					{
						AssertEquals("Collection count", 2, cargoReport.HouseBills.Count);
						AssertContainsExactElementsInAnyOrder("CA_HouseBill", new[] { "1234567890", "ADDEDCONSIGNMENT" }, cargoReport.HouseBills.OfType<CusSCAHouse>().Select(house => house.CA_HouseBill));
					});
				}
			}
		}

		public void TestMenuItemAction_AUAirCargoReport_MatchExistingJob()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_MasterBillNum = "08138374491";

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_UniqueConsignRef = "S0000001";
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "AUSYD";

				consol.Shipments.Add(shipment);

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				var consignment1 = consignmentHeader.Consignments.AddNew();
				consignment1.HVC_IsActive = true;

				var item = consignment1.Items.AddNew();
				item.HVI_JS_LoadedOnShipment = shipment.PK;

				var existingCargoReport = Factory.NewWithValidTestData<CusMAWB>();
				existingCargoReport.CM_MessageReference = "TestReference";

				var pivot = Factory.New<GenPivot>();
				pivot.XX_RelationType = GenPivotTypes.HighVolumeLowValue;
				pivot.XX_Relation1TableCode = HVLVConsignmentHeaderSchema.Constants.Prefix;
				pivot.XX_Relation1ID = consignmentHeader.PK;
				pivot.XX_Relation2TableCode = CusMAWBSchema.Constants.Prefix;
				pivot.XX_Relation2ID = existingCargoReport.PK;

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var loadedShipment = newFactory.Load<ForwardingShipment>(shipment.PK);
				using (var form = new ZForm(loadedShipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var hVLVMenu = plugin.TopLevelMenu;
					hVLVMenu.PerformSelect();

					var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					customsMenu.OnPopup(EventArgs.Empty);

					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().FirstOrDefault(menu => menu.Caption == "HVLV AirCargo Report");
					var menuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Sync HVLV AirCargo Report");
					Assert(menuItem.Visible);

					menuItem.PerformClick();

					var loadedCargoReports = newFactory.Load<CusMAWB>(new ZQuery());
					CombineAssertions("Should update existing cargo report", () =>
					{
						AssertEquals("Find only 1 customs job", 1, loadedCargoReports.Length);
						AssertEquals("The only job is the existing job", existingCargoReport.PK, loadedCargoReports.Single().PK);
						AssertEquals("The only job is shown in a form", loadedCargoReports.Single().PK, ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity.Identifier);
					});
				}
			}
		}

		public void TestMenuItemAction_AUSeaCargoReport_MatchExistingJob()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Sea;
				consol.JK_MasterBillNum = "08138374491";

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_UniqueConsignRef = "S0000001";
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "AUSYD";

				consol.Shipments.Add(shipment);

				var container = Factory.NewWithValidTestData<ForwardingContainer>();
				container.JC_ContainerNum = "TestContainer";

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_HCH_Header = consignmentHeader.PK;
				consignment.HVC_IsActive = true;

				var item = consignment.Items.AddNew();
				item.HVI_JS_LoadedOnShipment = shipment.PK;
				item.HVI_ContainerNumber = "TestContainer";

				var existingCargoReport = Factory.NewWithValidTestData<CusSCAOceanBill>();
				existingCargoReport.CB_MessageReference = "TestReference";

				var existingContainer = existingCargoReport.Containers.AddNew();
				existingContainer.CN_ContainerNumber = "TestContainer";
				existingContainer.CN_CB = existingCargoReport.PK;

				var pivot = Factory.New<GenPivot>();
				pivot.XX_RelationType = GenPivotTypes.HighVolumeLowValue;
				pivot.XX_Relation1TableCode = HVLVConsignmentHeaderSchema.Constants.Prefix;
				pivot.XX_Relation1ID = consignmentHeader.PK;
				pivot.XX_Relation2TableCode = CusSCAOceanBillSchema.Constants.Prefix;
				pivot.XX_Relation2ID = existingCargoReport.PK;

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var loadedShipment = newFactory.Load<ForwardingShipment>(shipment.PK);
				using (var form = new ZForm(loadedShipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var hVLVMenu = plugin.TopLevelMenu;
					hVLVMenu.PerformSelect();

					var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					customsMenu.OnPopup(EventArgs.Empty);

					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().FirstOrDefault(menu => menu.Caption == "HVLV SeaCargo Report");
					var menuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Sync HVLV SeaCargo Report");
					Assert(menuItem.Visible);

					menuItem.PerformClick();

					var loadedCargoReports = newFactory.Load<CusSCAOceanBill>(new ZQuery());
					CombineAssertions("Should update existing cargo report", () =>
					{
						AssertEquals("Find only 1 customs job", 1, loadedCargoReports.Length);
						AssertEquals("The only job is the existing job", existingCargoReport.PK, loadedCargoReports.Single().PK);
						AssertEquals("The only job is shown in a form", loadedCargoReports.Single().PK, ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity.Identifier);
					});
				}
			}
		}

		public void TestMenuItemAction_NZAirCargoReport_MatchExistingJob()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.NewZealand))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_MasterBillNum = "TestMAWB";

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_UniqueConsignRef = "S0000001";
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "NZAKL";

				consol.Shipments.Add(shipment);

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_HCH_Header = consignmentHeader.PK;
				consignment.HVC_IsActive = true;

				var item = consignment.Items.AddNew();
				item.HVI_JS_LoadedOnShipment = shipment.PK;

				var existingCargoReport = Factory.New<ICusMAWB>();
				existingCargoReport.CM_MAWB = "TestMAWB";

				var pivot = Factory.New<GenPivot>();
				pivot.XX_RelationType = GenPivotTypes.HighVolumeLowValue;
				pivot.XX_Relation1TableCode = HVLVConsignmentHeaderSchema.Constants.Prefix;
				pivot.XX_Relation1ID = consignmentHeader.PK;
				pivot.XX_Relation2TableCode = CusMAWBSchema.Constants.Prefix;
				pivot.XX_Relation2ID = existingCargoReport.PK;

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var loadedShipment = newFactory.Load<ForwardingShipment>(shipment.PK);
				using (var form = new ZForm(loadedShipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var hVLVMenu = plugin.TopLevelMenu;
					hVLVMenu.PerformSelect();

					var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					customsMenu.OnPopup(EventArgs.Empty);

					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().FirstOrDefault(menu => menu.Caption == "HVLV AirCargo ICR");

					var menuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Sync HVLV AirCargo ICR");
					Assert(menuItem.Visible);

					menuItem.PerformClick();

					var loadedCargoReports = newFactory.Load<ICusMAWB>(new ZQuery());
					CombineAssertions("Should update existing cargo report", () =>
					{
						AssertEquals("Find only 1 customs job", 1, loadedCargoReports.Length);
						AssertEquals("The only job is the existing job", existingCargoReport.PK, loadedCargoReports.Single().PK);
						AssertEquals("The only job is shown in a form", loadedCargoReports.Single().PK, ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity.Identifier);
					});
				}
			}
		}

		public void TestMenuItemAction_NZSeaCargoReort_MatchExistingJob()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.NewZealand))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Sea;
				consol.JK_MasterBillNum = "TestMasterBill";

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_UniqueConsignRef = "S0000001";
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "NZAKL";

				consol.Shipments.Add(shipment);

				var container = Factory.NewWithValidTestData<ForwardingContainer>();
				container.JC_ContainerNum = "TestContainer";

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_HCH_Header = consignmentHeader.PK;
				consignment.HVC_IsActive = true;

				var item = consignment.Items.AddNew();
				item.HVI_JS_LoadedOnShipment = shipment.PK;
				item.HVI_ContainerNumber = "TestContainer";

				var existingCargoReport = Factory.New<ICusSCAOceanBill>();
				existingCargoReport.CB_OceanBill = "TestMasterBill";

				var existingContainer = existingCargoReport.Containers.AddNew();
				existingContainer.CN_ContainerNumber = "TestContainer";
				existingContainer.CN_CB = existingCargoReport.PK;

				var pivot = Factory.New<GenPivot>();
				pivot.XX_RelationType = GenPivotTypes.HighVolumeLowValue;
				pivot.XX_Relation1TableCode = HVLVConsignmentHeaderSchema.Constants.Prefix;
				pivot.XX_Relation1ID = consignmentHeader.PK;
				pivot.XX_Relation2TableCode = CusSCAOceanBillSchema.Constants.Prefix;
				pivot.XX_Relation2ID = existingCargoReport.PK;

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var loadedShipment = newFactory.Load<ForwardingShipment>(shipment.PK);
				using (var form = new ZForm(loadedShipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var hVLVMenu = plugin.TopLevelMenu;
					hVLVMenu.PerformSelect();

					var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					customsMenu.OnPopup(EventArgs.Empty);

					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().FirstOrDefault(menu => menu.Caption == "HVLV SeaCargo ICR");
					var menuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Sync HVLV SeaCargo ICR");
					Assert(menuItem.Visible);

					menuItem.PerformClick();

					var loadedCargoReports = newFactory.Load<ICusSCAOceanBill>(new ZQuery());
					CombineAssertions("Should update existing cargo report", () =>
					{
						AssertEquals("Find only 1 customs job", 1, loadedCargoReports.Length);
						AssertEquals("The only job is the existing job", existingCargoReport.PK, loadedCargoReports.Single().PK);
						AssertEquals("The only job is shown in a form", loadedCargoReports.Single().PK, ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity.Identifier);
					});
				}
			}
		}

		public void TestMenuItemAction_EUICS2Manifest_MatchExistingJob()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_MasterBillNum = "08138374491";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_RL_NKDestination = "FRBRU";

			consol.Shipments.Add(shipment);

			var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

			var item = Factory.NewWithValidTestData<HVLVItem>();
			item.HVI_JS_LoadedOnShipment = shipment.PK;
			var consignment = item.Consignment;
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_HCH_Header = consignmentHeader.PK;

			var existingManifestHeader = (Enterprise.Integration.Customs.ASYCUDA.EUICS2.IAsycudaManifestHeader)Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.Customs.ASYCUDA.EUICS2.IAsycudaManifestHeader>());
			existingManifestHeader.AMA_JobReference = "TestReference";
			existingManifestHeader.AMA_ManifestType = "ENS";

			var pivot = Factory.New<GenPivot>();
			pivot.XX_RelationType = GenPivotTypes.HighVolumeLowValue;
			pivot.XX_Relation1TableCode = HVLVConsignmentHeaderSchema.Constants.Prefix;
			pivot.XX_Relation1ID = consignmentHeader.PK;
			pivot.XX_Relation2TableCode = AsycudaManifestHeaderSchema.Constants.Prefix;
			pivot.XX_Relation2ID = existingManifestHeader.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedShipment = newFactory.Load<ForwardingShipment>(shipment.PK);
			using (var form = new ZForm(loadedShipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var hVLVMenu = plugin.TopLevelMenu;
				hVLVMenu.PerformSelect();

				var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				customsMenu.OnPopup(EventArgs.Empty);

				var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menu => menu.Caption == "EU ICS2 Manifest");
				var syncMenuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Sync EU ICS2 Manifest");

				syncMenuItem.PerformClick();

				var loadedManifestHeaders = newFactory.Load<Enterprise.Integration.Customs.ASYCUDA.EUICS2.IAsycudaManifestHeader>(new ZQuery());
				CombineAssertions("Should update existing manifest", () =>
				{
					AssertEquals("Find only 1 customs job", 1, loadedManifestHeaders.Length);
					AssertEquals("The only job is the existing job", existingManifestHeader.PK, loadedManifestHeaders.Single().PK);
					AssertEquals("The only job is shown in a form", loadedManifestHeaders.Single().PK, ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity.Identifier);
				});
			}
		}

		[Licensing.Billing.Business.Testing.FeatureDataTest(LicenceFeatureCodeList.Codes.EcommerceH7Feature)]
		public void TestMenuItemAction_IEH7Declaration_MatchExistingJob()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Ireland))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Sea;
				consol.JK_MasterBillNum = "08138374491";

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_RL_NKOrigin = "FRBRU";
				shipment.JS_RL_NKDestination = "IEDUB";

				consol.Shipments.Add(shipment);

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				var item = Factory.NewWithValidTestData<HVLVItem>();
				item.HVI_JS_LoadedOnShipment = shipment.PK;
				var consignment = item.Consignment;
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_HCH_Header = consignmentHeader.PK;

				var existingManifestHeader = Factory.New<Enterprise.Integration.Customs.ASYCUDA.EUH7.IAsycudaManifestHeader>();
				existingManifestHeader.AMA_JobReference = "TestReference";
				existingManifestHeader.AMA_ManifestType = "EH7";

				var pivot = Factory.New<GenPivot>();
				pivot.XX_RelationType = GenPivotTypes.HighVolumeLowValue;
				pivot.XX_Relation1TableCode = HVLVConsignmentHeaderSchema.Constants.Prefix;
				pivot.XX_Relation1ID = consignmentHeader.PK;
				pivot.XX_Relation2TableCode = AsycudaManifestHeaderSchema.Constants.Prefix;
				pivot.XX_Relation2ID = existingManifestHeader.PK;

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var loadedShipment = newFactory.Load<ForwardingShipment>(shipment.PK);
				using (var form = new ZForm(loadedShipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var hVLVMenu = plugin.TopLevelMenu;
					hVLVMenu.PerformSelect();

					var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					customsMenu.OnPopup(EventArgs.Empty);

					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menu => menu.Caption == "Low Value (H7)");
					var syncMenuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Sync Low Value (H7)");

					syncMenuItem.PerformClick();

					var loadedManifestHeaders = newFactory.Load<Enterprise.Integration.Customs.ASYCUDA.EUH7.IAsycudaManifestHeader>(new ZQuery());
					CombineAssertions("Should update existing H7 declaration", () =>
					{
						AssertEquals("Find only 1 customs job", 1, loadedManifestHeaders.Length);
						AssertEquals("The only job is the existing job", existingManifestHeader.PK, loadedManifestHeaders.Single().PK);
						AssertEquals("The only job is shown in a form", loadedManifestHeaders.Single().PK, ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity.Identifier);
					});
				}
			}
		}

		public void TestMenuItemAction_SGAccessManifest_MatchExistingJob()
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

				consol.Shipments.Add(shipment);

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				var item = Factory.NewWithValidTestData<HVLVItem>();
				item.HVI_JS_LoadedOnShipment = shipment.PK;
				var consignment = item.Consignment;
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_HCH_Header = consignmentHeader.PK;

				var existingManifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();

				var pivot = Factory.New<GenPivot>();
				pivot.XX_RelationType = GenPivotTypes.HighVolumeLowValue;
				pivot.XX_Relation1TableCode = HVLVConsignmentHeaderSchema.Constants.Prefix;
				pivot.XX_Relation1ID = consignmentHeader.PK;
				pivot.XX_Relation2TableCode = AsycudaManifestHeaderSchema.Constants.Prefix;
				pivot.XX_Relation2ID = existingManifestHeader.PK;

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var loadedShipment = newFactory.Load<ForwardingShipment>(shipment.PK);
				using (var form = new ZForm(loadedShipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var hVLVMenu = plugin.TopLevelMenu;
					hVLVMenu.PerformSelect();

					var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					customsMenu.OnPopup(EventArgs.Empty);

					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menu => menu.Caption == "SG ACCESS Export Manifest");
					var syncMenuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Sync SG ACCESS Export Manifest");

					syncMenuItem.PerformClick();

					var loadedManifestHeaders = newFactory.Load<AsycudaManifestHeader>(new ZQuery());
					CombineAssertions("Should update existing manifest", () =>
					{
						AssertEquals("Find only 1 customs job", 1, loadedManifestHeaders.Length);
						AssertEquals("The only job is the existing job", existingManifestHeader.PK, loadedManifestHeaders.Single().PK);
						AssertEquals("The only job is shown in a form", loadedManifestHeaders.Single().PK, ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity.Identifier);
					});
				}
			}
		}

		public void TestMenuItemAction_TRETradeManifest_MatchExistingJob()
		{
			var trRegistry = ObjectFactory.Get<Enterprise.Integration.Customs.TR.ITRCustomsDataRegistry>().ExposeETradeModule as BooleanRegistryItem;
			using (trRegistry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Turkey))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Sea;
				consol.JK_MasterBillNum = "08138374491";

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_RL_NKDestination = "TRKAL";
				shipment.JS_RL_NKOrigin = "AUSYD";

				consol.Shipments.Add(shipment);

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				var item = Factory.NewWithValidTestData<HVLVItem>();
				item.HVI_JS_LoadedOnShipment = shipment.PK;
				var consignment = item.Consignment;
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_HCH_Header = consignmentHeader.PK;

				var existingManifestHeader = (Enterprise.Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader)Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>());
				existingManifestHeader.AMA_JobReference = "TestReference";
				existingManifestHeader.AMA_ManifestType = "ETR";

				var pivot = Factory.New<GenPivot>();
				pivot.XX_RelationType = GenPivotTypes.HighVolumeLowValue;
				pivot.XX_Relation1TableCode = HVLVConsignmentHeaderSchema.Constants.Prefix;
				pivot.XX_Relation1ID = consignmentHeader.PK;
				pivot.XX_Relation2TableCode = AsycudaManifestHeaderSchema.Constants.Prefix;
				pivot.XX_Relation2ID = existingManifestHeader.PK;

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var loadedShipment = newFactory.Load<ForwardingShipment>(shipment.PK);
				using (var form = new ZForm(loadedShipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var hVLVMenu = plugin.TopLevelMenu;
					hVLVMenu.PerformSelect();

					var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					customsMenu.OnPopup(EventArgs.Empty);

					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menu => menu.Caption == "TR E-Trade Manifest");
					var syncMenuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Sync TR E-Trade Manifest");

					syncMenuItem.PerformClick();

					var loadedManifestHeaders = newFactory.Load<Enterprise.Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>(new ZQuery());
					CombineAssertions("Should update existing manifest", () =>
					{
						AssertEquals("Find only 1 customs job", 1, loadedManifestHeaders.Length);
						AssertEquals("The only job is the existing job", existingManifestHeader.PK, loadedManifestHeaders.Single().PK);
						AssertEquals("The only job is shown in a form", loadedManifestHeaders.Single().PK, ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity.Identifier);
					});
				}
			}
		}

		public void TestMenuItemAction_TWBriefCustomsDeclaration_MatchExistingJob()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Sea;
				consol.JK_MasterBillNum = "08138374491";

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_RL_NKDestination = "TWTPE";
				shipment.JS_RL_NKOrigin = "AUSYD";

				consol.Shipments.Add(shipment);

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				var item = Factory.NewWithValidTestData<HVLVItem>();
				item.HVI_JS_LoadedOnShipment = shipment.PK;
				var consignment = item.Consignment;
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_HCH_Header = consignmentHeader.PK;

				var existingManifestHeader = Factory.NewWithValidTestData<Customs.TW.BriefCustomsDeclaration.Business.AsycudaManifestHeader>();
				existingManifestHeader.AMA_JobReference = "TestReference";

				var pivot = Factory.New<GenPivot>();
				pivot.XX_RelationType = GenPivotTypes.HighVolumeLowValue;
				pivot.XX_Relation1TableCode = HVLVConsignmentHeaderSchema.Constants.Prefix;
				pivot.XX_Relation1ID = consignmentHeader.PK;
				pivot.XX_Relation2TableCode = AsycudaManifestHeaderSchema.Constants.Prefix;
				pivot.XX_Relation2ID = existingManifestHeader.PK;

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var loadedShipment = newFactory.Load<ForwardingShipment>(shipment.PK);
				using (var form = new ZForm(loadedShipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var hVLVMenu = plugin.TopLevelMenu;
					hVLVMenu.PerformSelect();

					var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					customsMenu.OnPopup(EventArgs.Empty);

					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menu => menu.Caption == "Taiwan Brief Customs Declaration Import");
					var syncMenuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Sync Taiwan Brief Customs Declaration Import");

					syncMenuItem.PerformClick();

					var loadedManifestHeaders = newFactory.Load<Customs.TW.BriefCustomsDeclaration.Business.AsycudaManifestHeader>(new ZQuery());
					CombineAssertions("Should update existing manifest", () =>
					{
						AssertEquals("Find only 1 customs job", 1, loadedManifestHeaders.Length);
						AssertEquals("The only job is the existing job", existingManifestHeader.PK, loadedManifestHeaders.Single().PK);
						AssertEquals("The only job is shown in a form", loadedManifestHeaders.Single().PK, ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity.Identifier);
					});
				}
			}
		}

		public void TestMenuItemAction_TWForwarderManifest_MatchExistingJob()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Sea;
				consol.JK_MasterBillNum = "08138374491";

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_RL_NKDestination = "TWTPE";
				shipment.JS_RL_NKOrigin = "AUSYD";

				consol.Shipments.Add(shipment);

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				var item = Factory.NewWithValidTestData<HVLVItem>();
				item.HVI_JS_LoadedOnShipment = shipment.PK;
				var consignment = item.Consignment;
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_HCH_Header = consignmentHeader.PK;

				var existingManifestHeader = Factory.NewWithValidTestData<Customs.TW.Manifest.Business.AsycudaManifestHeader>();
				existingManifestHeader.AMA_JobReference = "TestReference";
				existingManifestHeader.AMA_ManifestType = "MAN";

				var pivot = Factory.New<GenPivot>();
				pivot.XX_RelationType = GenPivotTypes.HighVolumeLowValue;
				pivot.XX_Relation1TableCode = HVLVConsignmentHeaderSchema.Constants.Prefix;
				pivot.XX_Relation1ID = consignmentHeader.PK;
				pivot.XX_Relation2TableCode = AsycudaManifestHeaderSchema.Constants.Prefix;
				pivot.XX_Relation2ID = existingManifestHeader.PK;

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var loadedShipment = newFactory.Load<ForwardingShipment>(shipment.PK);
				using (var form = new ZForm(loadedShipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var hVLVMenu = plugin.TopLevelMenu;
					hVLVMenu.PerformSelect();

					var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					customsMenu.OnPopup(EventArgs.Empty);

					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menu => menu.Caption == "TW Forwarder Manifest (Import)");
					var syncMenuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Sync TW Forwarder Manifest (Import)");

					syncMenuItem.PerformClick();

					var loadedManifestHeaders = newFactory.Load<Customs.TW.Manifest.Business.AsycudaManifestHeader>(new ZQuery());
					CombineAssertions("Should update existing manifest", () =>
					{
						AssertEquals("Find only 1 customs job", 1, loadedManifestHeaders.Length);
						AssertEquals("The only job is the existing job", existingManifestHeader.PK, loadedManifestHeaders.Single().PK);
						AssertEquals("The only job is shown in a form", loadedManifestHeaders.Single().PK, ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity.Identifier);
					});
				}
			}
		}

		public void TestMenuItemAction_USAirAMS_MatchExistingJob()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_MasterBillNum = "08138374491";
				consol.JK_RL_NKLoadPort = "NZAKL";

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_HouseBill = "UWM97N872947";
				shipment.JS_UniqueConsignRef = "S0000001";

				consol.Shipments.Add(shipment);

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				var item = Factory.NewWithValidTestData<HVLVItem>();
				item.HVI_JS_LoadedOnShipment = shipment.PK;
				var consignment = item.Consignment;
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_HCH_Header = consignmentHeader.PK;

				var existingAMSHeader = Factory.NewWithValidTestData<Customs.US.ACEManifest.Business.AsycudaManifestHeader>();

				var pivot = Factory.New<GenPivot>();
				pivot.XX_RelationType = GenPivotTypes.HighVolumeLowValue;
				pivot.XX_Relation1TableCode = HVLVConsignmentHeaderSchema.Constants.Prefix;
				pivot.XX_Relation1ID = consignmentHeader.PK;
				pivot.XX_Relation2TableCode = AsycudaManifestHeaderSchema.Constants.Prefix;
				pivot.XX_Relation2ID = existingAMSHeader.PK;

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var loadedShipment = newFactory.Load<ForwardingShipment>(shipment.PK);
				using (var form = new ZForm(loadedShipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var hVLVMenu = plugin.TopLevelMenu;
					hVLVMenu.PerformSelect();

					var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					customsMenu.OnPopup(EventArgs.Empty);

					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menu => menu.Caption == "US Air AMS (Import)");
					var syncMenuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Sync US Air AMS (Import)");
					UnitTestUserNotification.Instance.AddOKAnswer();
					syncMenuItem.PerformClick();

					var loadedAMSHeaders = newFactory.Load<Customs.US.ACEManifest.Business.AsycudaManifestHeader>(new ZQuery());
					CombineAssertions("Should update existing Air AMS", () =>
					{
						AssertEquals("Find only 1 customs job", 1, loadedAMSHeaders.Length);
						AssertEquals("The only job is the existing job", existingAMSHeader.PK, loadedAMSHeaders.Single().PK);
						AssertEquals("The only job is shown in a form", loadedAMSHeaders.Single().PK, ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity.Identifier);
					});
				}
			}
		}

		public void TestMenuItemAction_USSeaAMS_MatchExistingJob()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Sea;
				consol.JK_MasterBillNum = "08138374491";
				consol.JK_RL_NKLoadPort = "NZAKL";

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_HouseBill = "UWM97N872947";
				shipment.JS_UniqueConsignRef = "S0000001";

				consol.Shipments.Add(shipment);

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				var item = Factory.NewWithValidTestData<HVLVItem>();
				item.HVI_JS_LoadedOnShipment = shipment.PK;

				var consignment = item.Consignment;
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_HCH_Header = consignmentHeader.PK;

				var existingAMSHeader = Factory.NewWithValidTestData<CusInBondHeader>();
				existingAMSHeader.BH_TransitDirection = "N";

				var pivot = Factory.New<GenPivot>();
				pivot.XX_RelationType = GenPivotTypes.HighVolumeLowValue;
				pivot.XX_Relation1TableCode = HVLVConsignmentHeaderSchema.Constants.Prefix;
				pivot.XX_Relation1ID = consignmentHeader.PK;
				pivot.XX_Relation2TableCode = CusInBondHeaderSchema.Constants.Prefix;
				pivot.XX_Relation2ID = existingAMSHeader.PK;

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var loadedShipment = newFactory.Load<ForwardingShipment>(shipment.PK);
				using (var form = new ZForm(loadedShipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var hVLVMenu = plugin.TopLevelMenu;
					hVLVMenu.PerformSelect();

					var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					customsMenu.OnPopup(EventArgs.Empty);

					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menu => menu.Caption == "US Sea AMS (Import)");
					var allmenu = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().ToArray();
					var syncMenuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Sync US Sea AMS (Import)");
					UnitTestUserNotification.Instance.AddOKAnswer();
					syncMenuItem.PerformClick();

					var loadedAMSHeaders = newFactory.Load<CusInBondHeader>(new ZQuery());
					CombineAssertions("Should update existing Sea AMS", () =>
					{
						AssertEquals("Find only 1 customs job", 1, loadedAMSHeaders.Length);
						AssertEquals("The only job is the existing job", existingAMSHeader.PK, loadedAMSHeaders.Single().PK);
						AssertEquals("The only job is shown in a form", loadedAMSHeaders.Single().PK, ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity.Identifier);
					});
				}
			}
		}

		public void TestMenuItemAction_USRoadEManifest_MatchExistingJob()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();

				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Road;
				shipment.JS_RL_NKDestination = "USLAX";

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_HCH_Header = consignmentHeader.PK;
				consignment.Items.AddNew();

				var existingEManifest = Factory.NewWithValidTestData<Customs.US.eManifest.Business.Trip>();
				existingEManifest.BH_JobReference = "TestReference";

				var pivot = Factory.New<GenPivot>();
				pivot.XX_RelationType = GenPivotTypes.HighVolumeLowValue;
				pivot.XX_Relation1TableCode = HVLVConsignmentHeaderSchema.Constants.Prefix;
				pivot.XX_Relation1ID = consignmentHeader.PK;
				pivot.XX_Relation2TableCode = CusInBondHeaderSchema.Constants.Prefix;
				pivot.XX_Relation2ID = existingEManifest.PK;

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var loadedShipment = newFactory.Load<ForwardingShipment>(shipment.PK);
				using (var form = new ZForm(loadedShipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var commandJobTypeMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(x => x.Caption == "HVLV e-Manifest");
					commandJobTypeMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Sync HVLV e-Manifest").PerformClick();

					var loadedEManifests = newFactory.Load<Customs.US.eManifest.Business.Trip>(new ZQuery());
					CombineAssertions("Should update existing US eManifest", () =>
					{
						AssertEquals("Find only 1 customs job", 1, loadedEManifests.Length);
						AssertEquals("The only job is the existing job", existingEManifest.PK, loadedEManifests.Single().PK);
						AssertEquals("The only job is shown in a form", loadedEManifests.Single().PK, ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity.Identifier);
					});
				}
			}
		}

		public void TestMenuItemAction_USLowValueEntries_MatchExistingJob()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();

				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Road;
				shipment.JS_RL_NKDestination = "USLAX";

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_HCH_Header = consignmentHeader.PK;
				consignment.Items.AddNew();

				var existingClearance = Factory.NewWithValidTestData<CusUSLVClearance>();
				existingClearance.ULH_JobNumber = "TestReference";

				var pivot = Factory.New<GenPivot>();
				pivot.XX_RelationType = GenPivotTypes.HighVolumeLowValue;
				pivot.XX_Relation1TableCode = HVLVConsignmentHeaderSchema.Constants.Prefix;
				pivot.XX_Relation1ID = consignmentHeader.PK;
				pivot.XX_Relation2TableCode = CusUSLVClearanceSchema.Constants.Prefix;
				pivot.XX_Relation2ID = existingClearance.PK;

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var loadedShipment = newFactory.Load<ForwardingShipment>(shipment.PK);
				using (var form = new ZForm(loadedShipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var commandJobTypeMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(x => x.Caption == "Low Value Entries");
					commandJobTypeMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Sync Low Value Entries").PerformClick();

					var loadedClearances = newFactory.Load<CusUSLVClearance>(new ZQuery());
					CombineAssertions("Should update existing clearance", () =>
					{
						AssertEquals("Find only 1 customs job", 1, loadedClearances.Length);
						AssertEquals("The only job is the existing job", existingClearance.PK, loadedClearances.Single().PK);
						AssertEquals("The only job is shown in a form", loadedClearances.Single().PK, ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity.Identifier);
					});
				}
			}
		}

		public void TestMenuItemAction_SGAccessManifest()
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
				consignment.HVC_WaybillNumber = "TESTWAYBILL";
				consignment.HVC_GoodsDescription = "New Description";

				var existingManifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				var existingBill = existingManifestHeader.Bills.AddNew();
				existingBill.ABL_BillNumber = consignment.HVC_WaybillNumber;
				existingBill.ABL_GoodsDescription = "Old Description";

				var pivot = Factory.New<GenPivot>();
				pivot.XX_RelationType = GenPivotTypes.HighVolumeLowValue;
				pivot.XX_Relation1TableCode = HVLVConsignmentHeaderSchema.Constants.Prefix;
				pivot.XX_Relation1ID = consignmentHeader.PK;
				pivot.XX_Relation2TableCode = AsycudaManifestHeaderSchema.Constants.Prefix;
				pivot.XX_Relation2ID = existingManifestHeader.PK;

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

					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menu => menu.Caption == "SG ACCESS Export Manifest");
					var syncMenuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Sync SG ACCESS Export Manifest");
					UnitTestUserNotification.Instance.AddOKAnswer();
					syncMenuItem.PerformClick();

					var manifestHeader = ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity as AsycudaManifestHeader;
					Assert("Type should be Enterprise.Customs.SG.Access.Business.AsycudaManifestHeader", manifestHeader.GetType().FullName == "Enterprise.Customs.SG.Access.Business.AsycudaManifestHeader");

					var updatedBill = manifestHeader.Bills.Cast<AsycudaBill>().Single(bill => bill.ABL_BillNumber == existingBill.ABL_BillNumber);
					Assert("Type should be Enterprise.Customs.SG.Access.Business.AsycudaBill", updatedBill.GetType().FullName == "Enterprise.Customs.SG.Access.Business.AsycudaBill");
					AssertEquals("Bill goods description should be updated", "New Description", updatedBill.ABL_GoodsDescription);
				}
			}
		}

		public void TestMenuItemAction_USSeaAMS()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Sea;
				consol.JK_MasterBillNum = "08138374491";
				consol.JK_RL_NKLoadPort = "NZAKL";

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_HouseBill = "UWM97N872947";
				shipment.JS_UniqueConsignRef = "S0000001";

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				consol.Shipments.Add(shipment);

				var item = Factory.NewWithValidTestData<HVLVItem>();
				item.HVI_JS_LoadedOnShipment = shipment.PK;

				var consignment = item.Consignment;
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_HCH_Header = consignmentHeader.PK;
				consignment.HVC_WaybillNumber = "TESTWAYBILL";
				consignment.HVC_WeightUQ = "G";

				var existingManifestHeader = Factory.NewWithValidTestData<CusInBondHeader>();
				existingManifestHeader.BH_RL_NKImportLoadPort = "AUSYD";
				existingManifestHeader.BH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				existingManifestHeader.BH_ParentID = shipment.PK;
				existingManifestHeader.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;

				var existingBill = existingManifestHeader.Bills.AddNew();
				existingBill.B0_MasterBillNumber = consignment.HVC_WaybillNumber;
				existingBill.B0_WeightUQ = "KG";

				var pivot = Factory.New<GenPivot>();
				pivot.XX_RelationType = GenPivotTypes.HighVolumeLowValue;
				pivot.XX_Relation1TableCode = HVLVConsignmentHeaderSchema.Constants.Prefix;
				pivot.XX_Relation1ID = consignmentHeader.PK;
				pivot.XX_Relation2TableCode = CusInBondHeaderSchema.Constants.Prefix;
				pivot.XX_Relation2ID = existingManifestHeader.PK;

				var shipmentParameters = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, "AMS"),
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.Mode, TransportModes.Sea),
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.ReferenceNumber, existingManifestHeader.BH_JobReference)
				};
				shipment.Logs.AddNew(AutoEvents.Transferred, shipmentParameters.ToArray());

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
					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menu => menu.Caption == "US Sea AMS (Import)");
					var allmenu = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().ToArray();
					var syncMenuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Sync US Sea AMS (Import)");
					UnitTestUserNotification.Instance.AddOKAnswer();
					syncMenuItem.PerformClick();

					var manifestHeader = ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity as CusInBondHeader;
					AssertEquals("Precondition: manifestHeader should be the same object", existingManifestHeader.PK, manifestHeader.PK);
					AssertEquals("Load port should be updated", "NZAKL", manifestHeader.BH_RL_NKImportLoadPort);

					var updatedBill = manifestHeader.Bills.Cast<CusInBondBill>().Single(bill => bill.B0_MasterBillNumber == existingBill.B0_MasterBillNumber);

					AssertEquals("Precondition: bill should be the same object", existingBill.PK, updatedBill.PK);
					AssertEquals("Bill weight UQ should be updated", "G", updatedBill.B0_WeightUQ);
				}
			}
		}

		public void TestMenuItemAction_USAirAMS()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_MasterBillNum = "08138374491";
				consol.JK_RL_NKLoadPort = "NZAKL";

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_HouseBill = "UWM97N872947";
				shipment.JS_UniqueConsignRef = "S0000001";

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				consol.Shipments.Add(shipment);

				var item = Factory.NewWithValidTestData<HVLVItem>();
				item.HVI_JS_LoadedOnShipment = shipment.PK;
				var consignment = item.Consignment;
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_HCH_Header = consignmentHeader.PK;
				consignment.HVC_WaybillNumber = "TESTWAYBILL";
				consignment.HVC_GoodsDescription = "New Description";

				var existingManifestHeader = Factory.NewWithValidTestData<Customs.US.ACEManifest.Business.AsycudaManifestHeader>();
				existingManifestHeader.AMA_RL_NKPortOfLoading = "AUSYD";
				var existingBill = existingManifestHeader.Bills.AddNew();
				existingBill.ABL_BillNumber = consignment.HVC_WaybillNumber;
				existingBill.ABL_GoodsDescription = "Old Description";

				var pivot = Factory.New<GenPivot>();
				pivot.XX_RelationType = GenPivotTypes.HighVolumeLowValue;
				pivot.XX_Relation1TableCode = HVLVConsignmentHeaderSchema.Constants.Prefix;
				pivot.XX_Relation1ID = consignmentHeader.PK;
				pivot.XX_Relation2TableCode = AsycudaManifestHeaderSchema.Constants.Prefix;
				pivot.XX_Relation2ID = existingManifestHeader.PK;

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

					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menu => menu.Caption == "US Air AMS (Import)");
					var syncMenuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Sync US Air AMS (Import)");
					UnitTestUserNotification.Instance.AddOKAnswer();
					syncMenuItem.PerformClick();

					var manifestHeader = ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity as Customs.US.ACEManifest.Business.AsycudaManifestHeader;
					AssertEquals("Loading port should be updated", "NZAKL", manifestHeader.AMA_RL_NKPortOfLoading);

					var updatedBill = manifestHeader.Bills.Cast<Customs.US.ACEManifest.Business.AsycudaBill>().Single(bill => bill.ABL_BillNumber == existingBill.ABL_BillNumber);
					Assert("Bill goods description should be updated", "New Description".Equals(updatedBill.ABL_GoodsDescription, StringComparison.OrdinalIgnoreCase));
				}
			}
		}

		public void TestNonWesternEuropeanCharactersAreRemovedWhenUSLowValueEntriesRegistryIsTrue()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Road;
			consol.JK_MasterBillNum = "08138374491";

			var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Road;
			shipment.JS_HouseBill = "UWM97N872947";

			var destination = Factory.NewWithValidTestData<RefUNLOCO>();
			destination.RL_RN_NKCountryCode = CountryCodes.UnitedStates;
			shipment.JS_RL_NKDestination = destination.Code;

			consol.Shipments.Add(shipment);

			var item1 = Factory.NewWithValidTestData<HVLVItem>();
			item1.HVI_JS_LoadedOnShipment = shipment.PK;
			var consignment1 = item1.Consignment;
			consignment1.HVC_WaybillNumber = "HVC00001";
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_ConsigneeAddress1 = "Address1";
			consignment1.HVC_ConsigneeAddress2 = "Address2";
			consignment1.HVC_ConsigneeCity = "CiTy";
			consignment1.HVC_ConsigneeName = "Name";
			consignment1.HVC_ConsigneePostcode = "postCODE";
			consignment1.HVC_ConsigneeState = "Statë";

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			using (HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersUSLowValueEntries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var hVLVMenu = plugin.TopLevelMenu;
				hVLVMenu.PerformSelect();

				var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menu => menu.Caption == "Low Value Entries");

				UnitTestUserNotification.Instance.AddOKAnswer();
				jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Create Low Value Entries").PerformClick();

				var report = ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity as CusUSLVClearance;
				var uslvConsignment = (CusUSLVConsignment)report.CusUSLVConsignments.Find(new ZQuery(CusUSLVConsignmentSchema.ULB_HouseBill, "HVC00001"))[0];

				report.Factory.Save();

				CombineAssertions("Precondition", () =>
				{
					AssertEquals("ADDRESS1", uslvConsignment.ULB_ConsigneeAddress1);
					AssertEquals("ADDRESS2", uslvConsignment.ULB_ConsigneeAddress2);
					AssertEquals("CITY", uslvConsignment.ULB_ConsigneeCity);
					AssertEquals("NAME", uslvConsignment.ULB_ConsigneeName);
					AssertEquals("POSTCODE", uslvConsignment.ULB_ConsigneePostCode);
					AssertEquals("STATË", uslvConsignment.ULB_ConsigneeState);
				});

				consignment1.HVC_ConsigneeAddress1 = "Address1用户的数据";
				consignment1.HVC_ConsigneeAddress2 = "用户的数据Address2";
				consignment1.HVC_ConsigneeCity = "CiT用户y";
				consignment1.HVC_ConsigneeName = "Na用户me";
				consignment1.HVC_ConsigneePostcode = "p用户ostCODE";
				consignment1.HVC_ConsigneeState = "S用户tatë";

				form.FireSaveButton();
				plugin.OnSaveCompletedOrAborted(true);

				customsMenu.OnPopup(EventArgs.Empty);
				jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menu => menu.Caption == "Low Value Entries");
				jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Sync Low Value Entries").PerformClick();
				report = ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity as CusUSLVClearance;
				uslvConsignment = (CusUSLVConsignment)report.CusUSLVConsignments.Find(new ZQuery(CusUSLVConsignmentSchema.ULB_HouseBill, "HVC00001"))[0];

				CombineAssertions("Non-western European characters are removed when synchronizing", () =>
				{
					AssertEquals("ADDRESS1", uslvConsignment.ULB_ConsigneeAddress1);
					AssertEquals("ADDRESS2", uslvConsignment.ULB_ConsigneeAddress2);
					AssertEquals("CITY", uslvConsignment.ULB_ConsigneeCity);
					AssertEquals("NAME", uslvConsignment.ULB_ConsigneeName);
					AssertEquals("POSTCODE", uslvConsignment.ULB_ConsigneePostCode);
					AssertEquals("STATË", uslvConsignment.ULB_ConsigneeState);
				});
			}
		}

		public void TestNonWesternEuropeanCharactersNotRemovedWhenUSLowValueEntriesRegistryIsFalse()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Road;
			consol.JK_MasterBillNum = "08138374491";

			var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Road;
			shipment.JS_HouseBill = "UWM97N872947";

			var destination = Factory.NewWithValidTestData<RefUNLOCO>();
			destination.RL_RN_NKCountryCode = CountryCodes.UnitedStates;
			shipment.JS_RL_NKDestination = destination.Code;

			consol.Shipments.Add(shipment);

			var item1 = Factory.NewWithValidTestData<HVLVItem>();
			item1.HVI_JS_LoadedOnShipment = shipment.PK;
			var consignment1 = item1.Consignment;
			consignment1.HVC_WaybillNumber = "HVC00001";
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_ConsigneeAddress1 = "Address1";
			consignment1.HVC_ConsigneeAddress2 = "Address2";
			consignment1.HVC_ConsigneeCity = "CiTy";
			consignment1.HVC_ConsigneeName = "Name";
			consignment1.HVC_ConsigneePostcode = "postCODE";
			consignment1.HVC_ConsigneeState = "Statë";

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			using (HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersUSLowValueEntries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var hVLVMenu = plugin.TopLevelMenu;
				hVLVMenu.PerformSelect();

				var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menu => menu.Caption == "Low Value Entries");

				UnitTestUserNotification.Instance.AddOKAnswer();
				jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Create Low Value Entries").PerformClick();

				var report = ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity as CusUSLVClearance;
				var uslvConsignment = (CusUSLVConsignment)report.CusUSLVConsignments.Find(new ZQuery(CusUSLVConsignmentSchema.ULB_HouseBill, "HVC00001"))[0];

				report.Factory.Save();

				CombineAssertions("Precondition", () =>
				{
					AssertEquals("ADDRESS1", uslvConsignment.ULB_ConsigneeAddress1);
					AssertEquals("ADDRESS2", uslvConsignment.ULB_ConsigneeAddress2);
					AssertEquals("CITY", uslvConsignment.ULB_ConsigneeCity);
					AssertEquals("NAME", uslvConsignment.ULB_ConsigneeName);
					AssertEquals("POSTCODE", uslvConsignment.ULB_ConsigneePostCode);
					AssertEquals("STATË", uslvConsignment.ULB_ConsigneeState);
				});

				consignment1.HVC_ConsigneeAddress1 = "Address1用户的数据";
				consignment1.HVC_ConsigneeAddress2 = "用户的数据Address2";
				consignment1.HVC_ConsigneeCity = "CiT用户y";
				consignment1.HVC_ConsigneeName = "Na用户me";
				consignment1.HVC_ConsigneePostcode = "p用户ostCODE";
				consignment1.HVC_ConsigneeState = "S用户tatë";

				Factory.Save();

				customsMenu.OnPopup(EventArgs.Empty);
				jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menu => menu.Caption == "Low Value Entries");
				jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Sync Low Value Entries").PerformClick();
				report = ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity as CusUSLVClearance;
				uslvConsignment = (CusUSLVConsignment)report.CusUSLVConsignments.Find(new ZQuery(CusUSLVConsignmentSchema.ULB_HouseBill, "HVC00001"))[0];

				CombineAssertions("Non-western European characters are not removed when synchronizing", () =>
				{
					AssertEquals("ADDRESS1用户的数据", uslvConsignment.ULB_ConsigneeAddress1);
					AssertEquals("用户的数据ADDRESS2", uslvConsignment.ULB_ConsigneeAddress2);
					AssertEquals("CIT用户Y", uslvConsignment.ULB_ConsigneeCity);
					AssertEquals("NA用户ME", uslvConsignment.ULB_ConsigneeName);
					AssertEquals("P用户OSTCODE", uslvConsignment.ULB_ConsigneePostCode);
					AssertEquals("S用户TATË", uslvConsignment.ULB_ConsigneeState);
				});
			}
		}

		public void TestGivenAUAirShipment_WhenSyncShipmentToAirCargoReport_AndRegistryIsTrue_ThenRemoveNonEuropeanWesternCharactersFromCargoReport()
		{
			var australianAirCargoShipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
			australianAirCargoShipment.JS_TransportMode = TransportModes.Air;
			australianAirCargoShipment.JS_RL_NKOrigin = "CNSHA";
			australianAirCargoShipment.JS_RL_NKDestination = "AUSYD";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_MasterBillNum = "MAWB1234";
			consol.Shipments.Add(australianAirCargoShipment);

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = australianAirCargoShipment.PK;
			consignment.HVC_WaybillNumber = "1234";
			consignment.HVC_ConsigneeName = "AIRName";
			consignment.HVC_ConsigneeCity = "AIRCity";
			consignment.HVC_ConsigneePostcode = "AIRPost";
			consignment.Items.AddNew();

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			using (var form = new ZForm(australianAirCargoShipment))
			{
				HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersAUAirCargoReport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var hVLVMenu = plugin.TopLevelMenu;
				hVLVMenu.PerformSelect();

				var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menu => menu.Caption == "HVLV AirCargo Report");

				UnitTestUserNotification.Instance.AddOKAnswer();
				jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Create HVLV AirCargo Report").PerformClick();

				var cargoReport = ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity;
				cargoReport.Factory.Save();
				var airCargoCuHAWBReport = Factory.LoadTop1<CusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, "1234"));

				CombineAssertions(() =>
				{
					AssertEquals("AIRNAME", airCargoCuHAWBReport.CS_ConsigneeName);
					AssertEquals("AIRCITY", airCargoCuHAWBReport.CS_ConsigneeCity);
					AssertEquals("AIRPOST", airCargoCuHAWBReport.CS_ConsigneePostcode);
				});

				consignment.HVC_ConsigneeName = "上海AIRName";
				consignment.HVC_ConsigneeCity = "はいAIRCity";
				consignment.HVC_ConsigneePostcode = "아니오AIRPost";

				form.FireSaveButton();
				plugin.OnSaveCompletedOrAborted(true);

				customsMenu.OnPopup(EventArgs.Empty);
				jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menu => menu.Caption == "HVLV AirCargo Report");
				jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Sync HVLV AirCargo Report").PerformClick();

				var syncedCargoReport = ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity as CusMAWB;
				var syncedHouseBill = syncedCargoReport.CurrentHouseBills[0];

				CombineAssertions(() =>
				{
					AssertEquals("Expected the european non-western characters to be removed", "AIRNAME", syncedHouseBill.CS_ConsigneeName);
					AssertEquals("Expected the european non-western characters to be removed", "AIRCITY", syncedHouseBill.CS_ConsigneeCity);
					AssertEquals("Expected the european non-western characters to be removed", "AIRPOST", syncedHouseBill.CS_ConsigneePostcode);
				});
			}
		}

		public void TestGivenAUAirShipment_WhenSyncShipmentToAirCargoReport_AndRegistryIsFalse_ThenRemoveNonEuropeanWesternCharactersFromCargoReport()
		{
			var australianAirCargoShipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
			australianAirCargoShipment.JS_TransportMode = TransportModes.Air;
			australianAirCargoShipment.JS_RL_NKOrigin = "CNSHA";
			australianAirCargoShipment.JS_RL_NKDestination = "AUSYD";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_MasterBillNum = "MAWB1234";
			consol.Transports.MostInterestingTransport.JW_VoyageFlight = "Flight100";
			consol.Shipments.Add(australianAirCargoShipment);

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = australianAirCargoShipment.PK;
			consignment.HVC_WaybillNumber = "1234";
			consignment.HVC_ConsigneeName = "AIRName";
			consignment.HVC_ConsigneeCity = "AIRCity";
			consignment.HVC_ConsigneePostcode = "AIRPost";

			consignment.Items.AddNew();

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			using (var form = new ZForm(australianAirCargoShipment))
			{
				HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersAUAirCargoReport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var hVLVMenu = plugin.TopLevelMenu;
				hVLVMenu.PerformSelect();

				var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menu => menu.Caption == "HVLV AirCargo Report");

				UnitTestUserNotification.Instance.AddOKAnswer();
				jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Create HVLV AirCargo Report").PerformClick();

				var cargoReport = ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity;
				cargoReport.Factory.Save();
				var airCargoCuHAWBReport = Factory.LoadTop1<CusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, "1234"));

				CombineAssertions(() =>
				{
					AssertEquals("AIRNAME", airCargoCuHAWBReport.CS_ConsigneeName);
					AssertEquals("AIRCITY", airCargoCuHAWBReport.CS_ConsigneeCity);
					AssertEquals("AIRPOST", airCargoCuHAWBReport.CS_ConsigneePostcode);
				});

				consignment.HVC_ConsigneeName = "上海AIRName";
				consignment.HVC_ConsigneeCity = "はいAIRCity";
				consignment.HVC_ConsigneePostcode = "아니오AIRPost";

				Factory.Save();

				customsMenu.OnPopup(EventArgs.Empty);
				jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menu => menu.Caption == "HVLV AirCargo Report");
				jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Sync HVLV AirCargo Report").PerformClick();
				var syncedCargoReport = ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity as CusMAWB;
				var syncedHouseBill = syncedCargoReport.CurrentHouseBills[0];

				CombineAssertions(() =>
				{
					AssertEquals("Expected the european non-western characters to be not removed", "上海AIRNAME", syncedHouseBill.CS_ConsigneeName);
					AssertEquals("Expected the european non-western characters to be not removed", "はいAIRCITY", syncedHouseBill.CS_ConsigneeCity);
					AssertEquals("Expected the european non-western characters to be not removed", "아니오AIRPOST", syncedHouseBill.CS_ConsigneePostcode);
				});
			}
		}

		public void TestGivenAUSeaShipment_WhenSyncShipmentToSeaCargoReport_AndRegistryIsTrue_ThenRemoveNonEuropeanWesternCharactersFromCargoReport()
		{
			var australianSeaCargoShipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
			australianSeaCargoShipment.JS_TransportMode = TransportModes.Sea;
			australianSeaCargoShipment.JS_RL_NKOrigin = "CNSHA";
			australianSeaCargoShipment.JS_RL_NKDestination = "AUSYD";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_MasterBillNum = "MAWB1234";
			consol.Shipments.Add(australianSeaCargoShipment);
			var container = consol.Containers.AddNew();
			container.ContainerNumberForBinding = "Container001";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = australianSeaCargoShipment.PK;
			consignment.HVC_WaybillNumber = "1234";
			consignment.HVC_ConsigneeName = "SEAName";
			consignment.HVC_ConsigneeAddress1 = "SEAAddress1";
			consignment.HVC_ConsigneeAddress2 = "SEAAddress2";
			consignment.HVC_ConsigneePostcode = "SEAPost";
			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = australianSeaCargoShipment.PK;

			foreach (var hvlvItem in australianSeaCargoShipment.HVLVItems)
			{
				hvlvItem.HVI_ContainerNumber = container.ContainerNumberForBinding;
			}

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			using (var form = new ZForm(australianSeaCargoShipment))
			{
				HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersAUSeaCargoReport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var hVLVMenu = plugin.TopLevelMenu;
				hVLVMenu.PerformSelect();

				var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menu => menu.Caption == "HVLV SeaCargo Report");

				UnitTestUserNotification.Instance.AddOKAnswer();
				jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Create HVLV SeaCargo Report").PerformClick();

				var cargoReport = ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity;
				cargoReport.Factory.Save();
				var seaCargoCusSCAHouseReport = Factory.LoadTop1<CusSCAHouse>(new ZQuery(CusSCAHouseSchema.CA_HouseBill, "1234"));

				cargoReport.Factory.Save();

				CombineAssertions(() =>
				{
					AssertEquals("SEANAME", seaCargoCusSCAHouseReport.CA_ConsigneeName);
					AssertEquals("SEAADDRESS1", seaCargoCusSCAHouseReport.CA_ConsigneeAddress1);
					AssertEquals("SEAADDRESS2", seaCargoCusSCAHouseReport.CA_ConsigneeAddress2);
					AssertEquals("SEAPOST", seaCargoCusSCAHouseReport.CA_ConsigneePostcode);
				});

				consignment.HVC_ConsigneeName = "上海SEAName";
				consignment.HVC_ConsigneeAddress1 = "はいSEAAddress1";
				consignment.HVC_ConsigneeAddress2 = "户的SEAAddress2";
				consignment.HVC_ConsigneePostcode = "아니오SEAPost";

				form.FireSaveButton();
				plugin.OnSaveCompletedOrAborted(true);

				customsMenu.OnPopup(EventArgs.Empty);
				jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menu => menu.Caption == "HVLV SeaCargo Report");
				jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Sync HVLV SeaCargo Report").PerformClick();

				seaCargoCusSCAHouseReport = Factory.LoadTop1<CusSCAHouse>(new ZQuery(CusSCAHouseSchema.CA_HouseBill, "1234"));

				CombineAssertions(() =>
				{
					AssertEquals("Expected the european non-western characters to be removed", "SEANAME", seaCargoCusSCAHouseReport.CA_ConsigneeName);
					AssertEquals("Expected the european non-western characters to be removed", "SEAADDRESS1", seaCargoCusSCAHouseReport.CA_ConsigneeAddress1);
					AssertEquals("Expected the european non-western characters to be removed", "SEAADDRESS2", seaCargoCusSCAHouseReport.CA_ConsigneeAddress2);
					AssertEquals("Expected the european non-western characters to be removed", "SEAPOST", seaCargoCusSCAHouseReport.CA_ConsigneePostcode);
				});
			}
		}

		public void TestGivenAUSeaShipment_WhenSyncShipmentToSeaCargoReport_AndRegistryIsFalse_ThenRemoveNonEuropeanWesternCharactersFromCargoReport()
		{
			var australianSeaCargoShipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
			australianSeaCargoShipment.JS_TransportMode = TransportModes.Sea;
			australianSeaCargoShipment.JS_RL_NKOrigin = "CNSHA";
			australianSeaCargoShipment.JS_RL_NKDestination = "AUSYD";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_MasterBillNum = "MAWB1234";
			consol.Shipments.Add(australianSeaCargoShipment);
			var container = consol.Containers.AddNew();
			container.ContainerNumberForBinding = "Container001";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = australianSeaCargoShipment.PK;
			consignment.HVC_WaybillNumber = "1234";
			consignment.HVC_ConsigneeName = "SEAName";
			consignment.HVC_ConsigneeAddress1 = "SEAAddress1";
			consignment.HVC_ConsigneeAddress2 = "SEAAddress2";
			consignment.HVC_ConsigneePostcode = "SEAPost";
			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = australianSeaCargoShipment.PK;

			foreach (var hvlvItem in australianSeaCargoShipment.HVLVItems)
			{
				hvlvItem.HVI_ContainerNumber = container.ContainerNumberForBinding;
			}

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			using (var form = new ZForm(australianSeaCargoShipment))
			{
				HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersAUSeaCargoReport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var hVLVMenu = plugin.TopLevelMenu;
				hVLVMenu.PerformSelect();

				var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menu => menu.Caption == "HVLV SeaCargo Report");

				UnitTestUserNotification.Instance.AddOKAnswer();
				jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Create HVLV SeaCargo Report").PerformClick();

				var cargoReport = ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity;
				cargoReport.Factory.Save();
				var seaCargoCusSCAHouseReport = Factory.LoadTop1<CusSCAHouse>(new ZQuery(CusSCAHouseSchema.CA_HouseBill, "1234"));

				CombineAssertions(() =>
				{
					AssertEquals("SEANAME", seaCargoCusSCAHouseReport.CA_ConsigneeName);
					AssertEquals("SEAADDRESS1", seaCargoCusSCAHouseReport.CA_ConsigneeAddress1);
					AssertEquals("SEAADDRESS2", seaCargoCusSCAHouseReport.CA_ConsigneeAddress2);
					AssertEquals("SEAPOST", seaCargoCusSCAHouseReport.CA_ConsigneePostcode);
				});

				consignment.HVC_ConsigneeName = "上海SEAName";
				consignment.HVC_ConsigneeAddress1 = "はいSEAAddress1";
				consignment.HVC_ConsigneeAddress2 = "户的SEAAddress2";
				consignment.HVC_ConsigneePostcode = "아니오SEAPost";

				Factory.Save();

				customsMenu.OnPopup(EventArgs.Empty);
				jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menu => menu.Caption == "HVLV SeaCargo Report");
				jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Sync HVLV SeaCargo Report").PerformClick();
				var syncedCargoReport = ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity as CusSCAOceanBill;
				var consigneeNames = syncedCargoReport.HouseBills.OfType<CusSCAHouse>().Select(house => house.CA_ConsigneeName);
				var consigneeAddress1s = syncedCargoReport.HouseBills.OfType<CusSCAHouse>().Select(house => house.CA_ConsigneeAddress1);
				var consigneeAddress2s = syncedCargoReport.HouseBills.OfType<CusSCAHouse>().Select(house => house.CA_ConsigneeAddress2);
				var consigneePostcodes = syncedCargoReport.HouseBills.OfType<CusSCAHouse>().Select(house => house.CA_ConsigneePostcode);

				CombineAssertions(() =>
				{
					AssertContainsExactElementsInAnyOrder("Expected the european non-western characters to be not removed", new[] { "", "上海SEANAME" }, consigneeNames);
					AssertContainsExactElementsInAnyOrder("Expected the european non-western characters to be not removed", new[] { "", "はいSEAADDRESS1" }, consigneeAddress1s);
					AssertContainsExactElementsInAnyOrder("Expected the european non-western characters to be not removed", new[] { "", "户的SEAADDRESS2" }, consigneeAddress2s);
					AssertContainsExactElementsInAnyOrder("Expected the european non-western characters to be not removed", new[] { "", "아니오SEAPOST" }, consigneePostcodes);
				});
			}
		}

		public void TestGivenAUSeaShipment_WhenPreScreeningStatusCodeIsUnKnownOrPassed_ThenClickSyncShipmentToCustomsCargoReportMenuItem()
		{
			var australianSeaCargoShipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
			australianSeaCargoShipment.JS_TransportMode = TransportModes.Sea;
			australianSeaCargoShipment.JS_RL_NKOrigin = "CNSHA";
			australianSeaCargoShipment.JS_RL_NKDestination = "AUSYD";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_MasterBillNum = "MAWB1234";
			consol.Shipments.Add(australianSeaCargoShipment);
			var container = consol.Containers.AddNew();
			container.ContainerNumberForBinding = "Container001";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = australianSeaCargoShipment.PK;
			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = australianSeaCargoShipment.PK;

			foreach (var hvlvItem in australianSeaCargoShipment.HVLVItems)
			{
				hvlvItem.HVI_ContainerNumber = container.ContainerNumberForBinding;
			}

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			using (HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersAUAirCargoReport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new ZForm(australianSeaCargoShipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var topLevelMenu = plugin.TopLevelMenu;
				topLevelMenu.PerformSelect();

				var customsMenu = topLevelMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menu => menu.Caption == "HVLV SeaCargo Report");
				jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Create HVLV SeaCargo Report").PerformClick();
				var convertionError = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertNullOrEmpty(convertionError);
				var cargoReport = ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity;
				cargoReport.Factory.Save();

				var preScreeningConfiguration = new HVLVDetailsPreScreeningConfiguration() { IsEnabled = true };
				using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, preScreeningConfiguration))
				{
					consignment.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown;

					form.FireSaveButton();
					plugin.OnSaveCompletedOrAborted(true);

					customsMenu.OnPopup(EventArgs.Empty);
					jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(menu => menu.Caption == "HVLV SeaCargo Report");
					jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Sync HVLV SeaCargo Report").PerformClick();

					var messageWithUnknownCode = UnitTestUserNotification.Instance.LastMessage.Text;
					AssertContains("HVLV Pre-Screening has been enabled but not run on this shipment.", messageWithUnknownCode);
					UnitTestUserNotification.Instance.ClearMessages();

					var preScreenMenu = topLevelMenu.MenuItems.OfType<HVLVPreScreenMenuGroup>().Single();
					var preScreenMenuItem = preScreenMenu.MenuItems.OfType<PreScreenMenuItem>().Single();
					preScreenMenuItem.OnPopup(EventArgs.Empty);

					AssertNotNull(preScreenMenuItem);
					preScreenMenuItem.PerformClick();
					Factory.Save();
					jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menuItem => menuItem.Caption == "Sync HVLV SeaCargo Report").PerformClick();
					var messageWithPassedCode = UnitTestUserNotification.Instance.LastMessage.Text;
					AssertNullOrEmpty("HVLV Pre-Screening has been enabled to run on this shipment", messageWithPassedCode);
				}
			}
		}
	}
}
