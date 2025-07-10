using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.ACEManifest.Business;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.GUI.Testing
{
	sealed class USSeaAMSMenuItemCommandEndToEndTest : RelatedJobCommandMenuItemEndToEndBaseTest
	{
		public void TestMenuItem_DependOnTransportMode_Open()
		{
			var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, true);
			shipment.JS_RL_NKDestination = "USCHI";
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				using (var form = new ZForm(shipment))
				{
					shipment.JS_TransportMode = TransportModes.Sea;
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					customsMenu.OnPopup(EventArgs.Empty);

					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "US Sea AMS (Import)");
					var createUSSeaAMSMenuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Create US Sea AMS (Import)");
					Assert("Show Create US AMS menuitem when transport mode is SEA", createUSSeaAMSMenuItem.Visible);
				}

				using (var form = new ZForm(shipment))
				{
					shipment.JS_TransportMode = TransportModes.Air;
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					customsMenu.OnPopup(EventArgs.Empty);

					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "US Air AMS (Import)");
					var createUSAirAMSMenuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Create US Air AMS (Import)");
					Assert("Show Create US Air AMS (Import) menuitem when transport mode is AIR", createUSAirAMSMenuItem.Visible);
				}

				using (var form = new ZForm(shipment))
				{
					shipment.JS_TransportMode = TransportModes.Road;
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					customsMenu.OnPopup(EventArgs.Empty);
					Assert(!customsMenu.MenuItems.OfType<ZMenuItem>().Any(menu => menu.Caption == "US Sea AMS (Import)"));
					Assert(!customsMenu.MenuItems.OfType<ZMenuItem>().Any(menu => menu.Caption == "US Air AMS (Import)"));
				}
			}
		}

		public void TestMenuItem_DependOnDestination_Open()
		{
			AssertMenuItemExistsDependOnDestination(TransportModes.Sea);
			AssertMenuItemExistsDependOnDestination(TransportModes.Air);

			void AssertMenuItemExistsDependOnDestination(string transportMode)
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, true);
				shipment.JS_TransportMode = transportMode;
				Factory.Save();

				var relatedJobName = transportMode == TransportModes.Air ? "US Air AMS (Import)" : "US Sea AMS (Import)";
				var menuCaption = string.Format("Create {0}", relatedJobName);

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
				{
					using (var form = new ZForm(shipment))
					{
						shipment.JS_RL_NKDestination = "USCHI";
						form.PlugIns.Add(ControllerIDs.ETailShipment);
						form.Show();

						var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
						var hVLVMenu = plugin.TopLevelMenu;
						hVLVMenu.PerformSelect();

						var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
						customsMenu.OnPopup(EventArgs.Empty);

						var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == relatedJobName);
						var createUSAMSMenuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == menuCaption);
						Assert("Show menuitem when destination is US", createUSAMSMenuItem.Visible);
					}

					using (var form = new ZForm(shipment))
					{
						shipment.JS_RL_NKDestination = "AUSYD";
						form.PlugIns.Add(ControllerIDs.ETailShipment);
						form.Show();

						var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
						var hVLVMenu = plugin.TopLevelMenu;
						hVLVMenu.PerformSelect();

						var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
						customsMenu.OnPopup(EventArgs.Empty);

						var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().FirstOrDefault(menu => menu.Caption == relatedJobName);
						var createUSAMSMenuItem = jobCommandMenuGroup?.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == menuCaption);
						AssertNull("No menuitem when destination is AU", jobCommandMenuGroup);
						AssertNull("No menuitem when destination is AU", createUSAMSMenuItem);
					}
				}
			}
		}

		public void TestMenuItem_GenPivot_Open()
		{
			AssertMenuItemExistsDependOnGenPivot(TransportModes.Air);
			AssertMenuItemExistsDependOnGenPivot(TransportModes.Sea);

			void AssertMenuItemExistsDependOnGenPivot(string transportMode)
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, true);
				shipment.JS_TransportMode = transportMode;
				shipment.JS_RL_NKDestination = "USCHI";
				Factory.Save();

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var hVLVMenu = plugin.TopLevelMenu;
					hVLVMenu.PerformSelect();

					var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					customsMenu.OnPopup(EventArgs.Empty);

					var relatedJobName = transportMode == TransportModes.Air ? "US Air AMS (Import)" : "US Sea AMS (Import)";
					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == relatedJobName);
					var menuCaption = string.Format("Create {0}", relatedJobName);
					var createUSAMSMenuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == menuCaption);
					Assert("Show menu item", createUSAMSMenuItem.Visible);

					var customsJobInterfaceType = transportMode == TransportModes.Air ?
																	typeof(Enterprise.Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader) :
																	typeof(Enterprise.Integration.Customs.US.USAMS.ICusInBondHeader);
					HVLVMenuItemTestHelper.CreateGenPivotRecord(shipment, customsJobInterfaceType);

					customsMenu.OnPopup(EventArgs.Empty);
					createUSAMSMenuItem = customsMenu.MenuItems.OfType<ZMenuItem>().FirstOrDefault(menu => menu.Caption == menuCaption);
					AssertNull("No menu item afert Gen Pivot record was created", createUSAMSMenuItem);
				}
			}
		}

		public void TestMenuItemAction_WhenShipmentHasChanges_ShowError()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, true);
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_UniqueConsignRef = "S001";
				shipment.JS_RL_NKDestination = "USCHI";
				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					shipment.HasChanges = true;

					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "US Sea AMS (Import)");
					var menuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Create US Sea AMS (Import)");

					Assert(menuItem.Visible);
					menuItem.PerformClick();

					var saveFirstMessage = UnitTestUserNotification.Instance.LastMessage.Text;
					AssertEquals("Please save the form before converting to US Sea AMS (Import).", saveFirstMessage);
				}
			}
		}

		public void TestMenuItemAction_Sea_WhenRegistryEnabledFilingForNonUSBranchAndNonUSBranch_ShowAMSForm()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			using (HVLVDataRegistry.Instance.EnableSecurityFilingsForNonUSACompanies.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertMenuItemAction_Sea_ShouldShowAMSForm();
			}
		}

		public void TestMenuItemAction_Sea_WhenRegistryEnabledFilingForNonUSBranchAndUSBranch_ShowAMSForm()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			using (HVLVDataRegistry.Instance.EnableSecurityFilingsForNonUSACompanies.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertMenuItemAction_Sea_ShouldShowAMSForm();
			}
		}

		public void TestMenuItemAction_Sea_WhenRegistryDisabledFilingForNonUSBranchAndUSBranch_ShowAMSForm()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			using (HVLVDataRegistry.Instance.EnableSecurityFilingsForNonUSACompanies.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertMenuItemAction_Sea_ShouldShowAMSForm();
			}
		}

		void AssertMenuItemAction_Sea_ShouldShowAMSForm()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_UniqueConsignRef = "S002";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.Consols.AddNew();

			var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_HCH_Header = consignmentHeader.PK;
			consignment.Items.AddNew();

			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var menuHVLV = plugin.TopLevelMenu;
				menuHVLV.PerformSelect();

				var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "US Sea AMS (Import)");
				var menuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Create US Sea AMS (Import)");

				Assert(menuItem.Visible);
				menuItem.PerformClick();

				var amsForm = ZFormModaliser.LastFormShownForTest as Customs.US.AMS.GUI.USAMSForm;
				AssertNotNull(amsForm);

				var amsHeader = amsForm.BusinessEntity;
				Assert("would not save AMS Header when create", !amsHeader.IsInDatabase);
			}
		}

		void AssertConsignmentWaybillHasNoSCACCodeAndIsOver12Characters_ShowsMessage(ZString transportMode)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = transportMode;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = transportMode;
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

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var relatedJobName = transportMode == TransportModes.Sea ? "US Sea AMS (Import)" : "US Air AMS (Import)";
					var menuItemCaption = string.Format("Create {0}", relatedJobName);
					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == relatedJobName);
					var menuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == menuItemCaption);

					Assert(menuItem.Visible);
					menuItem.PerformClick();

					using (var amsForm = ZFormModaliser.LastFormShownDialogForTest as Customs.US.AMS.GUI.USAMSForm)
					{
						AssertNull(amsForm);
						var waybillValidationErrorMessage = UnitTestUserNotification.Instance.LastMessage.Text;
						var expectedErrorMessage = string.Format("There are waybill(s) that exceed 12 in length, proceeding may cause message errors on {0}.\r\nWould you like to proceed?",
																relatedJobName);
						AssertEquals(expectedErrorMessage, waybillValidationErrorMessage);
					}
				}
			}
		}

		public void TestMenuItem_WhenConsignmentWaybillHasNoSCACCodeAndIsOver12Characters_ShowsMessage()
		{
			AssertConsignmentWaybillHasNoSCACCodeAndIsOver12Characters_ShowsMessage(TransportModes.Sea);
			AssertConsignmentWaybillHasNoSCACCodeAndIsOver12Characters_ShowsMessage(TransportModes.Air);
		}

		void AssertConsignmentWaybillHasSCACCodeAndIsOver16Characters_ShowsMessage(ZString transportMode)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = transportMode;
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
				consol.JK_TransportMode = transportMode;

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				var consignment_WithWaybillLength17 = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment_WithWaybillLength17.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment_WithWaybillLength17.HVC_WaybillNumber = "ABCD5678901234567";
				consignment_WithWaybillLength17.HVC_HCH_Header = consignmentHeader.PK;

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var relatedJobName = transportMode == TransportModes.Sea ? "US Sea AMS (Import)" : "US Air AMS (Import)";
					var menuItemCaption = string.Format("Create {0}", relatedJobName);
					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == relatedJobName);
					var menuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == menuItemCaption);

					Assert(menuItem.Visible);
					menuItem.PerformClick();

					using (var amsForm = ZFormModaliser.LastFormShownDialogForTest as Customs.US.AMS.GUI.USAMSForm)
					{
						AssertNull(amsForm);
						var waybillValidationErrorMessage = UnitTestUserNotification.Instance.LastMessage.Text;
						var expectedErrorMessage = string.Format("There are waybill(s) that exceed 12 in length, proceeding may cause message errors on {0}.\r\nWould you like to proceed?",
																relatedJobName);
						AssertEquals(expectedErrorMessage, waybillValidationErrorMessage);
					}
				}
			}
		}

		public void TestMenuItem_WhenConsignmentWaybillHasSCACCodeAndIsOver16Characters_ShowsMessage()
		{
			AssertConsignmentWaybillHasSCACCodeAndIsOver16Characters_ShowsMessage(TransportModes.Sea);
			AssertConsignmentWaybillHasSCACCodeAndIsOver16Characters_ShowsMessage(TransportModes.Air);
		}

		public void TestMenuItemAction_WhenNoConsignments_ShowsMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_UniqueConsignRef = "S002";
				shipment.JS_RL_NKDestination = "USLAX";

				HVLVConsignmentHeader.GetOrCreate(shipment);

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "US Sea AMS (Import)");
					var menuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Create US Sea AMS (Import)");

					Assert(menuItem.Visible);
					menuItem.PerformClick();

					using (var amsForm = ZFormModaliser.LastFormShownDialogForTest as Customs.US.AMS.GUI.USAMSForm)
					{
						AssertNull(amsForm);
						var noActiveConsignmentsMessage = UnitTestUserNotification.Instance.LastMessage.Text;
						AssertEquals("Please make sure there's at least one active consignment on this shipment.", noActiveConsignmentsMessage);
					}
				}
			}
		}

		public void TestMenuItemAction_WhenNoActiveConsignments_ShowsMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_UniqueConsignRef = "S002";
				shipment.JS_RL_NKDestination = "USLAX";

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_HCH_Header = consignmentHeader.PK;

				Factory.Save();

				var factory2 = new BusinessObjectFactory();
				var reloadedConsignment = factory2.Load<HVLVConsignment>(consignment.PK);
				reloadedConsignment.HVC_IsActive = false;
				factory2.Save();

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "US Sea AMS (Import)");
					var menuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Create US Sea AMS (Import)");

					Assert(menuItem.Visible);
					menuItem.PerformClick();

					using (var amsForm = ZFormModaliser.LastFormShownDialogForTest as Customs.US.AMS.GUI.USAMSForm)
					{
						AssertNull(amsForm);
						var noActiveConsignmentsMessage = UnitTestUserNotification.Instance.LastMessage.Text;
						AssertEquals("Please make sure there's at least one active consignment on this shipment.", noActiveConsignmentsMessage);
					}
				}
			}
		}

		public void TestMenuItemAction_Air_ShouldShowManifestForm()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				var shipment = consol.Shipments.AddNew();

				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKDestination = "USLAX";

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_HCH_Header = consignmentHeader.PK;
				consignment.Items.AddNew();

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "US Air AMS (Import)");
					var menuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Create US Air AMS (Import)");

					Assert(menuItem.Visible);
					menuItem.PerformClick();

					var manifestForm = ZFormModaliser.LastFormShownForTest as Customs.ASYCUDA.GUI.ManifestForm;
					AssertNotNull(manifestForm);

					var manifest = manifestForm.BusinessEntity;
					Assert("would not save AMS Header when create", !manifest.IsInDatabase);
				}
			}
		}

		[TestDate(2021, 2, 2, 2, 2, 0)]
		public void TestMenuItemAction_Sea_WillPopulateHVI_SecurityFilingFirstUsageTimeUtc()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_UniqueConsignRef = "S001";
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.Consols.AddNew();

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				var consignment = consignmentHeader.Consignments.AddNew();
				consignment.HVC_WaybillNumber = "LessThan12";

				var item1 = consignment.Items.AddNew();
				var item2 = Factory.NewWithValidTestData<HVLVItem>();
				item2.HVI_JS_LoadedOnShipment = shipment.PK;

				Factory.Save();

				var factory2 = new BusinessObjectFactory();
				var loadedShipment = factory2.Load<ForwardingShipment>(shipment.PK);
				var loadedItem1 = factory2.Load<HVLVItem>(item1.PK);
				var loadedItem2 = factory2.Load<HVLVItem>(item2.PK);

				using (var form = new ZForm(loadedShipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "US Sea AMS (Import)");
					var menuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Create US Sea AMS (Import)");

					Assert(menuItem.Visible);

					Assert("pre-condition item HVI_SecurityFilingFirstUsageTimeUtc should be empty", loadedItem1.HVI_SecurityFilingFirstUsageTimeUtc.IsEmpty);
					Assert("pre-condition item HVI_SecurityFilingFirstUsageTimeUtc should be empty", loadedItem2.HVI_SecurityFilingFirstUsageTimeUtc.IsEmpty);

					Factory.Save();

					menuItem.PerformClick();

					var ams = ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity;
					AssertNotNull(ams);
					ams.Factory.Save();

					loadedItem1.Reload();
					loadedItem2.Reload();

					CombineAssertions(() =>
					{
						AssertEquals(new ZDateTime(2021, 2, 2, 2, 2, 0), loadedItem1.HVI_SecurityFilingFirstUsageTimeUtc);
						AssertEquals(new ZDateTime(2021, 2, 2, 2, 2, 0), loadedItem2.HVI_SecurityFilingFirstUsageTimeUtc);
					});
				}
			}
		}

		[TestDate(2021, 2, 2, 2, 2, 2)]
		public void TestMenuItemAction_Sea_WillNotRepopulateHVI_SecurityFilingFirstUsageTimeUtc()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_UniqueConsignRef = "S002";
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.Consols.AddNew();

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_HCH_Header = consignmentHeader.PK;
				var item1 = consignment.Items.AddNew();
				var item2 = Factory.NewWithValidTestData<HVLVItem>();
				item2.HVI_JS_LoadedOnShipment = shipment.PK;

				item1.HVI_SecurityFilingFirstUsageTimeUtc = new ZDateTime(2019, 5, 3);
				item2.HVI_SecurityFilingFirstUsageTimeUtc = new ZDateTime(2019, 5, 3);

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "US Sea AMS (Import)");
					var menuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Create US Sea AMS (Import)");

					Assert(menuItem.Visible);

					Factory.Save();
					menuItem.PerformClick();

					var ams = ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity;
					AssertNotNull(ams);
					ams.Factory.Save();

					item1.Reload();
					item2.Reload();

					CombineAssertions("HVI_SecurityFilingFirstUsageTimeUtc should not be repopulated if a value already exists", () =>
					{
						AssertNotEquals(new ZDateTime(2021, 2, 2, 2, 2, 2), item1.HVI_SecurityFilingFirstUsageTimeUtc);
						AssertNotEquals(new ZDateTime(2021, 2, 2, 2, 2, 2), item2.HVI_SecurityFilingFirstUsageTimeUtc);
					});
				}
			}
		}

		public void TestMenuItemAction_Air_WhenNoConsol_ThenShowErrorMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKDestination = "USLAX";

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_HCH_Header = consignmentHeader.PK;
				consignment.HVC_IsActive = true;

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "US Air AMS (Import)");
					var menuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Create US Air AMS (Import)");

					Assert(menuItem.Visible);
					menuItem.PerformClick();

					Assert("precondition: no consolidation attached to this shipment", !shipment.Consols.Any());

					AssertEquals("Please make sure there is a consolidation attached to this shipment.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestMenuItemAction_Air_StripNonWesternEuropeanChararcters_DependOnRegistry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				var shipment = consol.Shipments.AddNew();

				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKDestination = "USLAX";

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_HCH_Header = consignmentHeader.PK;
				consignment.HVC_GoodsDescription = "中文TestGoods";
				consignment.Items.AddNew();

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "US Air AMS (Import)");
					var menuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Create US Air AMS (Import)");

					Assert(menuItem.Visible);
					menuItem.PerformClick();

					var manifest = ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity as AsycudaManifestHeader;
					var bill = manifest.Bills.AsEnumerable().Single();
					AssertEquals("Should not strip non western european characters when registry is not set", "中文TESTGOODS", bill.ABL_GoodsDescription);

					using (HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersUSAirAMS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
					{
						menuItem.PerformClick();
						manifest = ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity as AsycudaManifestHeader;
						bill = manifest.Bills.AsEnumerable().Single();
						AssertEquals("Should strip non western european characters when registry is set", "TESTGOODS", bill.ABL_GoodsDescription);
					}
				}
			}
		}

		public void TestMenuItemAction_CloseAMSFormWithoutSaving_ThenSaveShipment_WouldNotSaveAMSAndLog()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, true);
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_UniqueConsignRef = "S001";
				shipment.JS_RL_NKDestination = "USCHI";
				shipment.Consols.AddNew();
				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "US Sea AMS (Import)");
					var menuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Create US Sea AMS (Import)");

					Assert(menuItem.Visible);
					UnitTestUserNotification.Instance.AddYesAnswer();
					menuItem.PerformClick();

					var amsForm = ZFormModaliser.LastFormShownForTest as Customs.US.AMS.GUI.USAMSForm;
					amsForm.Close();

					Factory.Save();

					var amsHeader = amsForm.BusinessEntity;
					AssertNotNull(amsHeader);
					Assert(!amsHeader.IsInDatabase);

					Assert(!shipment.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.TransferredCode).Any());
				}
			}
		}

		public void TestMenuItemAction_ClosesProgressForm_BeforeShowingAMSForm()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Sea;
				var shipment = consol.Shipments.AddNew();

				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_RL_NKDestination = "USLAX";

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_HCH_Header = consignmentHeader.PK;
				consignment.Items.AddNew();

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					var progressFormClosed = false;
					var asserted = false;

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(obj =>
					{
						if (obj is Customs.US.AMS.GUI.USAMSForm)
						{
							Assert("Progress Form should be closed", progressFormClosed);
							asserted = true;
						}
						else if (obj is ProgressForm)
						{
							var progressForm = obj as ProgressForm;
							progressForm.FormClosed += delegate
							{ progressFormClosed = true; };
						}
					});

					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "US Sea AMS (Import)");
					var menuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Create US Sea AMS (Import)");

					Assert(menuItem.Visible);
					menuItem.PerformClick();

					Assert(asserted);
				}
			}
		}

		public void TestMenuItem_DependOnTransportMode_Create()
		{
			var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, true);
			shipment.JS_RL_NKDestination = "USCHI";
			shipment.Logs.AddNew(AutoEvents.Transferred, "|TYP=AMS|MOD=SEA");
			shipment.Logs.AddNew(AutoEvents.Transferred, "|TYP=AMS|MOD=AIR");
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				using (var form = new ZForm(shipment))
				{
					shipment.JS_TransportMode = TransportModes.Sea;
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var hVLVMenu = plugin.TopLevelMenu;
					hVLVMenu.PerformSelect();

					var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					customsMenu.OnPopup(EventArgs.Empty);

					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "US Sea AMS (Import)");
					var openUSSeaAMSMenuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Open US Sea AMS (Import)");
					Assert("Show Open US AMS menuitem when transport mode is SEA", openUSSeaAMSMenuItem.Visible);
				}

				using (var form = new ZForm(shipment))
				{
					shipment.JS_TransportMode = TransportModes.Air;
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var hVLVMenu = plugin.TopLevelMenu;
					hVLVMenu.PerformSelect();

					var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					customsMenu.OnPopup(EventArgs.Empty);

					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "US Air AMS (Import)");
					var openUSAirAMSMenuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Open US Air AMS (Import)");
					Assert("Show Open US AMS menuitem when transport mode is AIR", openUSAirAMSMenuItem.Visible);
				}

				using (var form = new ZForm(shipment))
				{
					shipment.JS_TransportMode = TransportModes.Road;
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var hVLVMenu = plugin.TopLevelMenu;
					hVLVMenu.PerformSelect();

					var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					customsMenu.OnPopup(EventArgs.Empty);

					Assert(!customsMenu.MenuItems.OfType<ZMenuItem>().Any(menu => menu.Caption == "US Sea AMS (Import)" && menu.Visible));
					Assert(!customsMenu.MenuItems.OfType<ZMenuItem>().Any(menu => menu.Caption == "US Air AMS (Import)" && menu.Visible));
				}
			}
		}

		public void TestMenuItem_DependOnDestination_Create()
		{
			AssertMenuItemExistsDependOnDestination(TransportModes.Air);
			AssertMenuItemExistsDependOnDestination(TransportModes.Sea);

			void AssertMenuItemExistsDependOnDestination(string transportMode)
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, true);
				shipment.JS_TransportMode = transportMode;

				var logReferences = transportMode == TransportModes.Air ? "|TYP=AMS|MOD=AIR" : "|TYP=AMS|MOD=SEA";
				shipment.Logs.AddNew(AutoEvents.Transferred, logReferences);

				Factory.Save();

				var relatedJobName = transportMode == TransportModes.Air ? "US Air AMS (Import)" : "US Sea AMS (Import)";
				var menuCaption = string.Format("Open {0}", relatedJobName);

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
				{
					using (var form = new ZForm(shipment))
					{
						shipment.JS_RL_NKDestination = "USCHI";
						form.PlugIns.Add(ControllerIDs.ETailShipment);
						form.Show();

						var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
						var hVLVMenu = plugin.TopLevelMenu;
						hVLVMenu.PerformSelect();

						var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
						customsMenu.OnPopup(EventArgs.Empty);

						var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == relatedJobName);
						var openUSAMSMenuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().FirstOrDefault(menu => menu.Caption == menuCaption);
						Assert("Show Open US AMS menuitem when destination is US", openUSAMSMenuItem.Visible);
					}

					using (var form = new ZForm(shipment))
					{
						shipment.JS_RL_NKDestination = "AUSYD";
						form.PlugIns.Add(ControllerIDs.ETailShipment);
						form.Show();

						var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
						var hVLVMenu = plugin.TopLevelMenu;
						hVLVMenu.PerformSelect();

						var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
						customsMenu.OnPopup(EventArgs.Empty);

						var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().FirstOrDefault(menu => menu.Caption == relatedJobName);
						var openUSAMSMenuItem = jobCommandMenuGroup?.MenuItems.OfType<ZMenuItem>().FirstOrDefault(menu => menu.Caption == menuCaption);
						Assert("No Open US AMS menuitem when destination is AU", openUSAMSMenuItem == null || !openUSAMSMenuItem.Visible);
					}
				}
			}
		}

		public void TestMenuItem_GenPivot_Create()
		{
			AssertMenuItemExistsDependOnGenPivot(TransportModes.Air);
			AssertMenuItemExistsDependOnGenPivot(TransportModes.Sea);

			void AssertMenuItemExistsDependOnGenPivot(string transportMode)
			{
				var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, true);
				shipment.JS_RL_NKDestination = "USCHI";
				shipment.JS_TransportMode = transportMode;
				Factory.Save();
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var hVLVMenu = plugin.TopLevelMenu;
					hVLVMenu.PerformSelect();

					var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var relatedJobName = transportMode == TransportModes.Air ? "US Air AMS (Import)" : "US Sea AMS (Import)";
					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == relatedJobName);
					jobCommandMenuGroup.OnPopup(EventArgs.Empty);

					var menuCaption = string.Format("Open {0}", relatedJobName);
					var openUSAMSMenuItem = jobCommandMenuGroup?.MenuItems.OfType<ZMenuItem>().FirstOrDefault(menu => menu.Caption == menuCaption);
					Assert("No Open US AMS menuitem when there is no Gen Pivot record", !openUSAMSMenuItem.Visible);

					var customsJobInterfaceType = transportMode == TransportModes.Air ?
																	typeof(Enterprise.Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader) :
																	typeof(Enterprise.Integration.Customs.US.USAMS.ICusInBondHeader);
					HVLVMenuItemTestHelper.CreateGenPivotRecord(shipment, customsJobInterfaceType);

					jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == relatedJobName);
					jobCommandMenuGroup.OnPopup(EventArgs.Empty);
					openUSAMSMenuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().First(menu => menu.Caption == menuCaption);
					Assert("Show Open US AMS menuitem when there is any Gen Pivot record", openUSAMSMenuItem.Visible);
				}
			}
		}

		public void TestMenuItemAction_WhenAMSHeaderExists_ShouldShowAMSForm()
		{
			var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, true);
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_UniqueConsignRef = "S002";
			shipment.JS_RL_NKDestination = "USLAX";

			HVLVMenuItemTestHelper.CreateGenPivotRecord(shipment, typeof(Enterprise.Integration.Customs.US.USAMS.ICusInBondHeader));
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var menuHVLV = plugin.TopLevelMenu;
				menuHVLV.PerformSelect();

				var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "US Sea AMS (Import)");
				var menuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Open US Sea AMS (Import)");

				Assert(menuItem.Visible);
				menuItem.PerformClick();

				using (var amsForm = ZFormModaliser.LastFormShownForTest as Customs.US.AMS.GUI.USAMSForm)
				{
					AssertNotNull(amsForm);

					var amsHeader = amsForm.BusinessEntity;
					var amsHeaderFromGenPivot = shipment.GetHVLVConsignmentHeader().GenPivotCollection.CustomsJobs.First();
					AssertEquals(amsHeaderFromGenPivot.PK, amsHeader.PK);
				}
			}
		}

		public void TestMenuItemAction_WhenTransportModeIsAir_ShouldShowManifestForm()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;

				var shipment = consol.Shipments.AddNew();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_UniqueConsignRef = "S002";
				shipment.JS_RL_NKDestination = "USLAX";

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_HCH_Header = consignmentHeader.PK;
				consignment.Items.AddNew();

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "US Air AMS (Import)");
					jobCommandMenuGroup.OnPopup(EventArgs.Empty);

					var createAMSMenuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().FirstOrDefault(menu => menu.Caption == "Create US Air AMS (Import)");
					var openUSAMSMenuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().FirstOrDefault(menu => menu.Caption == "Open US Air AMS (Import)");

					Assert(createAMSMenuItem.Visible);
					Assert(!openUSAMSMenuItem.Visible);

					createAMSMenuItem.PerformClick();

					var manifestForm = ZFormModaliser.LastFormShownForTest as Customs.ASYCUDA.GUI.ManifestForm;
					AssertNotNull(manifestForm);

					var manifest = manifestForm.BusinessEntity as BusinessObject;
					manifest.Factory.Save();
					Assert(manifest.IsInDatabase);

					Assert(shipment.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.TransferredCode).Any());
					jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "US Air AMS (Import)");
					jobCommandMenuGroup.OnPopup(EventArgs.Empty);
					createAMSMenuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().FirstOrDefault(menu => menu.Caption == "Create US Air AMS (Import)");
					openUSAMSMenuItem = jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().FirstOrDefault(menu => menu.Caption == "Open US Air AMS (Import)");
					Assert(openUSAMSMenuItem.Visible);
					Assert(!createAMSMenuItem.Visible);

					openUSAMSMenuItem.PerformClick();
					manifestForm = ZFormModaliser.LastFormShownForTest as Customs.ASYCUDA.GUI.ManifestForm;
					AssertNotNull(manifestForm);
					var manifest2 = manifestForm.BusinessEntity as BusinessObject;
					AssertEquals(manifest.PK, manifest2.PK);
				}
			}
		}

		protected override ForwardingShipment PrepareShipment()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_RL_NKDestination = "USLAX";

			return shipment;
		}

		protected override ConsigneeInfo GetConsigneeInfoFromJob(IBusiness job)
		{
			var consignee = ((Customs.US.AMS.Business.CusInBondHeader)job).Bills.Single().Consignee;

			return new ConsigneeInfo
			{
				Name = consignee.Addressee,
				State = consignee.State,
				City = consignee.City,
				Address1 = consignee.Address1,
				Address2 = consignee.Address2,
				PostCode = consignee.Postcode
			};
		}

		protected override string TestingCountry => CountryCodes.UnitedStates;
		protected override string RelatedJobName => "US Sea AMS (Import)";
		protected override string ExpectedAppLockKey => "USSeaAMSCommand";
		protected override BooleanRegistryItem RemoveNonEuropeanWesternCharactersRegistry => HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersUSSeaAMS;
	}
}
