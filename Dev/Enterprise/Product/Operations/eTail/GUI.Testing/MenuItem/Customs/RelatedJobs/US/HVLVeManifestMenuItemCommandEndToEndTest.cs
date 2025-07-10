using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.eTail.Business;
using Enterprise.eTail.DataTransfer;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;
using ShipmentTypes = Enterprise.Core.Constants.ShipmentTypes;
using TransportModes = Enterprise.Core.Constants.TransportModes;

namespace Enterprise.eTail.GUI.Testing
{
	sealed class HVLVeManifestMenuItemCommandEndToEndTest : RelatedJobCommandMenuItemEndToEndBaseTest
	{
		public void TestMenuItemVisibility_Open()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.GetOrCreateHVLVConsignmentHeader();

			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Air;

			AssertHVLVeManifestVisibility(shipment, "HVLV e-Manifest", "Open", CountryCodes.UnitedStates, false, "Expected menu item not to be visible as the shipment transportation isn't road");

			shipment.JS_TransportMode = TransportModes.Road;
			shipment.JS_RL_NKDestination = "CATOR";

			AssertHVLVeManifestVisibility(shipment, "HVLV e-Manifest", "Open", CountryCodes.UnitedStates, false, "Expected menu item not to be visible as the shipment destination isn't US");

			shipment.JS_RL_NKDestination = "USLAX";

			AssertHVLVeManifestVisibility(shipment, "HVLV e-Manifest", "Open", CountryCodes.UnitedStates, false, "Expected menu item to not be visible as there are no eManifest jobs");

			HVLVMenuItemTestHelper.CreateGenPivotRecord(shipment, typeof(Enterprise.Integration.Customs.US.eManifest.ICusInBondHeader));

			AssertHVLVeManifestVisibility(shipment, "HVLV e-Manifest", "Open", CountryCodes.UnitedStates, true, "Expected menu item to be visible");
			AssertHVLVeManifestVisibility(shipment, "HVLV e-Manifest", "Open", CountryCodes.Canada, true, "Expected menu item to be visible");
		}

		public void TestMenuItemAction_ShowEManifestForm_Open()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();

				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Road;
				shipment.JS_RL_NKDestination = "USLAX";

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

				HVLVMenuItemTestHelper.CreateGenPivotRecord(shipment, typeof(Enterprise.Integration.Customs.US.eManifest.ICusInBondHeader));

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var commandJobTypeMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "HVLV e-Manifest");

					commandJobTypeMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Open HVLV e-Manifest").PerformClick();

					using (var eManifestForm = ZFormModaliser.LastFormShownForTest as Customs.US.eManifest.GUI.ManifestForm)
					{
						AssertNotNull("Should open eManifestForm", eManifestForm);
					}
				}
			}
		}

		public void TestMenuItemAction_WhenRelatedJobsRowIsClicked_ShouldOpenEManifestForm()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();

				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Road;
				shipment.JS_RL_NKDestination = "USLAX";

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

				HVLVMenuItemTestHelper.CreateGenPivotRecord(shipment, typeof(Enterprise.Integration.Customs.US.eManifest.ICusInBondHeader));

				Factory.Save();

				var eManifestFormCount = 0;
				using (var form = new ZForm(shipment))
				{
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(obj =>
					{
						if (obj is ZChildForm metaHeaderForm && !(obj is ProgressForm))
						{
							var relatedJobsGrid = metaHeaderForm.Controls.Find("RelatedJobsGrid", true).Single() as ZGrid;
							relatedJobsGrid.Select(0);
							var viewJobButton = metaHeaderForm.Controls.Find("EditJobButton", true).Single() as ZButton;
							viewJobButton.PerformClick();
						}
						else if (obj is Customs.US.eManifest.GUI.ManifestForm)
						{
							eManifestFormCount++;
						}
					});

					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var commandJobTypeMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "HVLV e-Manifest");

					commandJobTypeMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Open HVLV e-Manifest").PerformClick();

					AssertEquals(1, eManifestFormCount);
				}
			}
		}
		public void TestMenuItemVisibility_Create()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Air;

			shipment.GetOrCreateHVLVConsignmentHeader();

			AssertHVLVeManifestVisibility(shipment, "HVLV e-Manifest", "Create", CountryCodes.UnitedStates, false, "Expected menu item not to be visible as the shipment transportation isn't road");

			shipment.JS_TransportMode = TransportModes.Road;
			shipment.JS_RL_NKDestination = "CATOR";

			AssertHVLVeManifestVisibility(shipment, "HVLV e-Manifest", "Create", CountryCodes.UnitedStates, false, "Expected menu item not to be visible as the shipment destination isn't US");

			shipment.JS_RL_NKDestination = "USLAX";

			AssertHVLVeManifestVisibility(shipment, "HVLV e-Manifest", "Create", CountryCodes.UnitedStates, true, "Expected menu item to be visible for US");
			AssertHVLVeManifestVisibility(shipment, "HVLV e-Manifest", "Create", CountryCodes.Canada, true, "Expected menu item to be visible for CA");

			HVLVMenuItemTestHelper.CreateGenPivotRecord(shipment, typeof(Enterprise.Integration.Customs.US.eManifest.ICusInBondHeader));

			AssertHVLVeManifestVisibility(shipment, "HVLV e-Manifest", "Create", CountryCodes.UnitedStates, false, "Expected menu item to not be visible after adding eManifest");
		}

		public void TestMenuItemAction_ShowEManifestForm_Create()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Road;
				var shipment = consol.Shipments.AddNew();

				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Road;
				shipment.JS_RL_NKDestination = "USLAX";

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
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
					var commandJobTypeMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(x => x.Caption == "HVLV e-Manifest");
					commandJobTypeMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Create HVLV e-Manifest").PerformClick();

					var emanifest = ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity;
					Assert("Should not save eManifest when create", !emanifest.IsInDatabase);

					using (var eManifestForm = ZFormModaliser.LastFormShownForTest as Customs.US.eManifest.GUI.ManifestForm)
					{
						AssertNotNull("Should open eManifestForm", eManifestForm);
					}
				}
			}
		}

		public void TestMenuItemAction_WhenNoConsol_ThenShowErrorMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Road;
				shipment.JS_RL_NKDestination = "USLAX";

				var header = shipment.GetOrCreateHVLVConsignmentHeader();
				header.Consignments.AddNew();

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var commandJobTypeMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(x => x.Caption == "HVLV e-Manifest");
					commandJobTypeMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Create HVLV e-Manifest").PerformClick();

					Assert("precondition: no consolidation attached to this shipment", !shipment.Consols.Any());

					AssertEquals("Please make sure there is a consolidation attached to this shipment.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestMenuItemAction_WhenNoActiveConsignments_ThenShowErrorMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Road;
				shipment.JS_RL_NKDestination = "USLAX";
				var header = shipment.GetOrCreateHVLVConsignmentHeader();

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_IsActive = false;
				header.Consignments.Add(consignment);

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var commandJobTypeMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(x => x.Caption == "HVLV e-Manifest");
					commandJobTypeMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Create HVLV e-Manifest").PerformClick();

					AssertEquals("Please make sure there's at least one active consignment on this shipment.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestMenuItemAction_WhenRegistryEnabledFilingForNonUSBranchAndNonUSBranch_ShoweManifestForm()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			using (HVLVDataRegistry.Instance.EnableSecurityFilingsForNonUSACompanies.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertMenuItemAction_ShouldShoweManifestForm();
			}
		}

		public void TestMenuItemAction_WhenRegistryEnabledFilingForNonUSBranchAndUSBranch_ShoweManifestForm()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			using (HVLVDataRegistry.Instance.EnableSecurityFilingsForNonUSACompanies.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertMenuItemAction_ShouldShoweManifestForm();
			}
		}

		public void TestMenuItemAction_WhenRegistryDisabledFilingForNonUSBranchAndUSBranch_ShoweManifestForm()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			using (HVLVDataRegistry.Instance.EnableSecurityFilingsForNonUSACompanies.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertMenuItemAction_ShouldShoweManifestForm();
			}
		}

		void AssertMenuItemAction_ShouldShoweManifestForm()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Road;
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Road;
			shipment.JS_RL_NKDestination = "USLAX";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
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
				var commandJobTypeMenuGroup = customsMenu.MenuItems.OfType<CommandJobTypeMenuGroup<USRoadEManifestCommand>>().Single(x => x.Caption == "HVLV e-Manifest");
				commandJobTypeMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Create HVLV e-Manifest").PerformClick();

				var eManifestForm = ZFormModaliser.LastFormShownForTest as Customs.US.eManifest.GUI.ManifestForm;
				AssertNotNull(eManifestForm);

				var emanifest = eManifestForm.BusinessEntity;
				Assert("Should not save eManifest when create", !emanifest.IsInDatabase);
			}
		}

		public void TestMenuItemAction_WhenConsignmentWaybillHasNoSCACCodeAndIsOver12Characters_ShowsMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var country = RefCountry.LoadFromCountryCode(Factory, "US");
				GlbCompany.CurrentCompany.OrgProxy.SetCustomsCode(OrgCusCode.USACodeTypes.ACASOriginatorCode, country, "Code");

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Road;
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
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var commandJobTypeMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(x => x.Caption == "HVLV e-Manifest");
					commandJobTypeMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Create HVLV e-Manifest").PerformClick();

					var waybillValidationErrorMessage = UnitTestUserNotification.Instance.LastMessage.Text;
					AssertEquals("There are waybill(s) that exceed 12 in length, proceeding may cause message errors on HVLV e-Manifest.\r\nWould you like to proceed?", waybillValidationErrorMessage);
				}
			}
		}

		public void TestMenuItemAction_WhenConsignmentWaybillHasSCACCodeAndIsOver16Characters_ShowsMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Road;
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
				consignment_WithWaybillLength17.HVC_WaybillNumber = "ABCD5678901234567";
				consignment_WithWaybillLength17.HVC_HCH_Header = consignmentHeader.PK;
				consignment_WithWaybillLength17.Items.AddNew();

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var commandJobTypeMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(x => x.Caption == "HVLV e-Manifest");
					commandJobTypeMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Create HVLV e-Manifest").PerformClick();

					var waybillValidationErrorMessage = UnitTestUserNotification.Instance.LastMessage.Text;
					AssertEquals("There are waybill(s) that exceed 12 in length, proceeding may cause message errors on HVLV e-Manifest.\r\nWould you like to proceed?", waybillValidationErrorMessage);
				}
			}
		}

		void AssertHVLVeManifestVisibility(ForwardingShipment shipment, string menuItemCaption, string actionName, string countryCode, bool expectedVisibility, string assertionFailureMessage)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var menuHVLV = plugin.TopLevelMenu;
				menuHVLV.PerformSelect();

				var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				var commandJobTypeMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().SingleOrDefault(x => x.Caption == menuItemCaption);
				commandJobTypeMenuGroup?.OnPopup(EventArgs.Empty);

				var menuItem = commandJobTypeMenuGroup?.MenuItems.Cast<ZMenuItem>().SingleOrDefault(x => x.Caption == string.Format("{0} {1}", actionName, menuItemCaption));

				AssertEquals(assertionFailureMessage, expectedVisibility, menuItem != null && menuItem.Visible);
			}
		}

		protected override ForwardingShipment PrepareShipment()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Road;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Road;
			shipment.JS_RL_NKDestination = "USLAX";

			return shipment;
		}

		protected override ConsigneeInfo GetConsigneeInfoFromJob(IBusiness job)
		{
			var consignee = ((Trip)job).Shipments[0].Consignee;

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
		protected override string RelatedJobName => "HVLV e-Manifest";
		protected override string ExpectedAppLockKey => "USRoadEManifestCommand";
		protected override BooleanRegistryItem RemoveNonEuropeanWesternCharactersRegistry => HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersUSeManifest;
	}
}
