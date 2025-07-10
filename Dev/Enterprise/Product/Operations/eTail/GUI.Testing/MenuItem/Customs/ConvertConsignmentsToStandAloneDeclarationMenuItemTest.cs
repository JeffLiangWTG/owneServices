using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.GUI.Testing
{
	[TestedType(typeof(ConvertConsignmentsToStandAloneDeclarationMenuItem))]
	sealed class ConvertConsignmentsToStandAloneDeclarationMenuItemTest : TestCaseWithFactory
	{
		public void TestConvertConsignmentsToStandAloneDeclarationMenuItemDisplay()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_GoodsDescription = "Various merchandise";

			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var menuHVLV = plugin.TopLevelMenu;
				menuHVLV.PerformSelect();

				var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				var declarationMenuGroup = customsMenu.MenuItems.OfType<DeclarationMenuGroup>().Single();

				customsMenu.OnPopup(EventArgs.Empty);
				var menuItem = declarationMenuGroup.MenuItems.OfType<ConvertConsignmentsToStandAloneDeclarationMenuItem>().Single();

				AssertEquals("The 'Convert Consignments to Stand Alone Declarations' menu item should always be visible", true, menuItem.Visible);
			}
		}

		public void TestConvertConsignmentsToStandAloneDeclarations_WhenShipmentHasNotAttachedToConsol_ThenDisplayError()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_GoodsDescription = "Various merchandise";

			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var menuHVLV = plugin.TopLevelMenu;
				menuHVLV.PerformSelect();

				var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				var declarationMenuGroup = customsMenu.MenuItems.OfType<DeclarationMenuGroup>().Single();

				customsMenu.OnPopup(EventArgs.Empty);
				var menuItem = declarationMenuGroup.MenuItems.OfType<ConvertConsignmentsToStandAloneDeclarationMenuItem>().Single();

				menuItem.PerformClick();

				AssertEquals("A Stand Alone Declaration cannot be created for a Consignment without transport details. Attach a Shipment to a Consolidation to create the Stand Alone Declaration.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConvertConsignmentsToStandAloneDeclarations_WhenAllConsignmentsAreDomestic_ThenDisplayNoEligibleConsignmentsPrompt()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "AUSYD";
				shipment.Consols.AddNew();

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);
				var consignment1 = BuildConsignmentWithReleaseStatusForCurrentDirection(shipment.PK, consignmentHeader.PK, "AU", "AU", HVLVReleaseStatus.Held, HVLVReleaseStatus.None);
				var consignment2 = BuildConsignmentWithReleaseStatusForCurrentDirection(shipment.PK, consignmentHeader.PK, "AU", "AU", HVLVReleaseStatus.Held, HVLVReleaseStatus.None);

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();
					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);

					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var declarationMenuGroup = customsMenu.MenuItems.OfType<DeclarationMenuGroup>().Single();

					customsMenu.OnPopup(EventArgs.Empty);

					var menuItem = declarationMenuGroup.MenuItems.OfType<ConvertConsignmentsToStandAloneDeclarationMenuItem>().Single();
					menuItem.PerformClick();

					AssertEquals("The 'Convert Consignments to Stand Alone Declarations' menu item should display this message when there are no consignments eligible for conversion to declaration. To be eligible, the shipment must have a cargo report through a shipment log and there needs to be at least one consignment with a release status that isn't NON or UNK and the consignment must not have a job declaration on it", "There are no eligible consignments to convert to Stand Alone Declarations.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestConvertConsignmentsToStandAloneDeclarations_WhenAllConsignmentsAddressAreEmpty_ThenDoNotDisplayNoEligibleConsignmentsPrompt()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "AUSYD";
				shipment.Consols.AddNew();

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);
				var consignment1 = BuildConsignmentWithReleaseStatusForCurrentDirection(shipment.PK, consignmentHeader.PK, "", "", HVLVReleaseStatus.Held, HVLVReleaseStatus.None);
				var consignment2 = BuildConsignmentWithReleaseStatusForCurrentDirection(shipment.PK, consignmentHeader.PK, "", "", HVLVReleaseStatus.Held, HVLVReleaseStatus.None);

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();
					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);

					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var declarationMenuGroup = customsMenu.MenuItems.OfType<DeclarationMenuGroup>().Single();

					customsMenu.OnPopup(EventArgs.Empty);

					var menuItem = declarationMenuGroup.MenuItems.OfType<ConvertConsignmentsToStandAloneDeclarationMenuItem>().Single();
					menuItem.PerformClick();

					AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestConvertConsignmentsToStandAloneDeclarations_WhenAllConsignmentsHaveDeclaration_ThenDisplayNoEligibleConsignmentsPrompt()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "AUSYD";
				shipment.Consols.AddNew();

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				var consignment1 = BuildConsignmentWithReleaseStatusForCurrentDirection(shipment.PK, consignmentHeader.PK, "NZ", "AU", HVLVReleaseStatus.Held, HVLVReleaseStatus.None);
				consignment1.HVC_GoodsDescription = "Consignment1 with Job Declaration";

				var consignment2 = BuildConsignmentWithReleaseStatusForCurrentDirection(shipment.PK, consignmentHeader.PK, "NZ", "AU", HVLVReleaseStatus.Held, HVLVReleaseStatus.None);
				consignment2.HVC_GoodsDescription = "Consignment2 with Job Declaration";

				var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
				consignment1.HVC_JE_ImportDeclaration = declaration1.PK;

				var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
				consignment2.HVC_JE_ImportDeclaration = declaration2.PK;

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();
					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);

					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var declarationMenuGroup = customsMenu.MenuItems.OfType<DeclarationMenuGroup>().Single();

					customsMenu.OnPopup(EventArgs.Empty);

					var menuItem = declarationMenuGroup.MenuItems.OfType<ConvertConsignmentsToStandAloneDeclarationMenuItem>().Single();
					menuItem.PerformClick();

					AssertEquals("The 'Convert Consignments to Stand Alone Declarations' menu item should display this message when there are no consignments eligible for conversion to declaration. To be eligible, the shipment must have a cargo report through a shipment log and there needs to be at least one consignment with a release status that isn't NON or UNK and the consignment must not have a job declaration on it", "There are no eligible consignments to convert to Stand Alone Declarations.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestMenuItemAction_WhenConsignmentWaybillHasNoSCACCodeAndIsOver12Characters_ShowsMessage()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.HasChanges = false;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Shipments.Add(shipment);

			var sendingAgentOrg = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = sendingAgentOrg.ConfigOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = "CCC";
			cusCode.OK_CustomsRegNo = "ABCD";
			cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;

			consol.JK_OA_SendingForwarderAddress = sendingAgentOrg.MainAddress.PK;

			var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

			var consignment_WithWaybillLength13 = BuildConsignmentWithReleaseStatusForCurrentDirection(shipment.PK, consignmentHeader.PK, "NZ", "AU", HVLVReleaseStatus.Held, HVLVReleaseStatus.None);
			consignment_WithWaybillLength13.HVC_WaybillNumber = "1234567890123";

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
				var declarationMenuGroup = customsMenu.MenuItems.OfType<DeclarationMenuGroup>().Single();
				var menuItem = declarationMenuGroup.MenuItems.OfType<ConvertConsignmentsToStandAloneDeclarationMenuItem>().Single();

				Assert(menuItem.Visible);
				menuItem.PerformClick();

				var waybillValidationErrorMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals("There are waybill(s) that exceed 12 in length, proceeding may cause message errors on Stand Alone Declarations.\r\nWould you like to proceed?", waybillValidationErrorMessage);
			}
		}

		public void TestMenuItemAction_WhenConsignmentWaybillHasSCACCodeAndIsOver16Characters_ShowsMessage()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.HasChanges = false;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Shipments.Add(shipment);

			var sendingAgentOrg = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = sendingAgentOrg.ConfigOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = "CCC";
			cusCode.OK_CustomsRegNo = "ABCD";
			cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;

			consol.JK_OA_SendingForwarderAddress = sendingAgentOrg.MainAddress.PK;

			var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

			var consignment_WithWaybillLength17 = BuildConsignmentWithReleaseStatusForCurrentDirection(shipment.PK, consignmentHeader.PK, "", "", HVLVReleaseStatus.Held, HVLVReleaseStatus.Held);
			consignment_WithWaybillLength17.HVC_WaybillNumber = "ABCD5678901234567";

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
				var declarationMenuGroup = customsMenu.MenuItems.OfType<DeclarationMenuGroup>().Single();
				var menuItem = declarationMenuGroup.MenuItems.OfType<ConvertConsignmentsToStandAloneDeclarationMenuItem>().Single();

				Assert(menuItem.Visible);
				menuItem.PerformClick();

				var waybillValidationErrorMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals("There are waybill(s) that exceed 12 in length, proceeding may cause message errors on Stand Alone Declarations.\r\nWould you like to proceed?", waybillValidationErrorMessage);
			}
		}

		public void TestOpenConvertConsignmentsToStandAloneDeclarations_WhenShipmentDestinationIsSupportedCountries_ThenDisplayForm()
		{
			void TestCase(string countryCode, ZString destination)
			{
				var shipmentAU = Factory.NewWithValidTestData<ForwardingShipment>();
				shipmentAU.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipmentAU.JS_RL_NKOrigin = "NZAKL";
				shipmentAU.JS_RL_NKDestination = destination;
				shipmentAU.Consols.AddNew();

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipmentAU);
				BuildConsignmentWithReleaseStatusForCurrentDirection(shipmentAU.PK, consignmentHeader.PK, "", "", HVLVReleaseStatus.Held, HVLVReleaseStatus.Held);

				Factory.Save();

				using (var form = new ZForm(shipmentAU))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var declarationMenuGroup = customsMenu.MenuItems.OfType<DeclarationMenuGroup>().Single();

					customsMenu.OnPopup(EventArgs.Empty);

					var menuItem = declarationMenuGroup.MenuItems.OfType<ConvertConsignmentsToStandAloneDeclarationMenuItem>().Single();
					menuItem.PerformClick();

					AssertEquals("The convert consignments to stand alone declarations form should be shown for an " + countryCode + " destination shipment", typeof(HVLVConsignmentsToStandAloneDeclarationsForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				}
			}

			var countryDestinations = new Dictionary<string, string>
			{
				{ CountryCodes.Australia, "AUSYD" },
				{ CountryCodes.Canada, "CAVAN" },
				{ CountryCodes.NewZealand, "NZAKL" },
				{ CountryCodes.UnitedStates, "USLAX" },
				{ CountryCodes.Singapore, "SGSIN" },
				{ CountryCodes.Taiwan, "TWTPE" },
				{ CountryCodes.Turkey, "TRIST" },
				{ CountryCodes.SouthAfrica, "ZADUR" },
				{ CountryCodes.Belgium, "BEBRU" },
				{ CountryCodes.Switzerland, "CHZRH" },
				{ CountryCodes.Germany, "DEHAM" },
				{ CountryCodes.Spain, "ESBCN" },
				{ CountryCodes.France, "FRPAR" },
				{ CountryCodes.UnitedKingdom, "GBLON" },
				{ CountryCodes.Ireland, "IEDUB" },
				{ CountryCodes.Italy, "ITGOA" },
				{ CountryCodes.Netherlands, "NLRTM" },
				{ CountryCodes.Sweden, "SESTO" },
				{ CountryCodes.Brazil, "BRSSA" },
				{ CountryCodes.China, "CNSHA" },
				{ CountryCodes.Poland, "PLWAW" },
				{ CountryCodes.UnitedArabEmirates, "AEDXB" },
			};

			CombineAssertions(() =>
			{
				var serviceType = typeof(ConvertToStandAloneDeclarationService);
				var propertyInfo = serviceType.GetProperty("ConvertToStandAloneDeclarationSupportedCountries", BindingFlags.Static | BindingFlags.NonPublic);

				AssertNotNull(propertyInfo);

				var supportedCountries = propertyInfo.GetValue(null) as IEnumerable<string>;

				AssertNotNull(supportedCountries);

				AssertContainsExactElementsInAnyOrder(supportedCountries, countryDestinations.Keys.ToArray());
				foreach (var countryDestination in countryDestinations)
				{
					TestCase(countryDestination.Key, countryDestination.Value);
				}
			});
		}

		public void TestOpenConvertConsignmentsToStandAloneDeclarations_WhenShipmentDestinationIsNotSupported_ThenUnsupportedErrorPrompt()
		{
			var shipmentDE = Factory.NewWithValidTestData<ForwardingShipment>();
			shipmentDE.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipmentDE.JS_RL_NKOrigin = "AUSYD";
			shipmentDE.JS_RL_NKDestination = "INBDI";
			shipmentDE.Consols.AddNew();

			var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipmentDE);
			BuildConsignmentWithReleaseStatusForCurrentDirection(shipmentDE.PK, consignmentHeader.PK, "", "", HVLVReleaseStatus.Held, HVLVReleaseStatus.Held);

			Factory.Save();

			using (var form = new ZForm(shipmentDE))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var menuHVLV = plugin.TopLevelMenu;
				menuHVLV.PerformSelect();

				var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				var declarationMenuGroup = customsMenu.MenuItems.OfType<DeclarationMenuGroup>().Single();

				customsMenu.OnPopup(EventArgs.Empty);

				var menuItem = declarationMenuGroup.MenuItems.OfType<ConvertConsignmentsToStandAloneDeclarationMenuItem>().Single();
				menuItem.PerformClick();

				AssertEquals("Stand Alone Declaration creation is not currently supported for this shipment destination.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConvertConsignmentsToStandAloneDeclarations_WhenShipmentHasNoDestination_ThenDisplayError()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.HasChanges = false;
			shipment.Consols.AddNew();

			var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

			var consignment = BuildConsignmentWithReleaseStatusForCurrentDirection(shipment.PK, consignmentHeader.PK, "", "", HVLVReleaseStatus.Held, HVLVReleaseStatus.Held);
			consignment.HVC_GoodsDescription = "Some test goods";

			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var menuHVLV = plugin.TopLevelMenu;
				menuHVLV.PerformSelect();

				var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				var declarationMenuGroup = customsMenu.MenuItems.OfType<DeclarationMenuGroup>().Single();

				customsMenu.OnPopup(EventArgs.Empty);

				var menuItem = declarationMenuGroup.MenuItems.OfType<ConvertConsignmentsToStandAloneDeclarationMenuItem>().Single();
				menuItem.PerformClick();

				AssertEquals("No destination set for the current shipment.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConvertConsignmentsToStandAloneDeclarations_WhenShipmentHasNoUnclearedConsignments_ThenDisplayError()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "AUSYD";
				shipment.Consols.AddNew();

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				var consignment1 = BuildConsignmentWithReleaseStatusForCurrentDirection(shipment.PK, consignmentHeader.PK, "NZ", "AU", HVLVReleaseStatus.Cleared, HVLVReleaseStatus.None);
				var consignment2 = BuildConsignmentWithReleaseStatusForCurrentDirection(shipment.PK, consignmentHeader.PK, "AU", "NZ", HVLVReleaseStatus.None, HVLVReleaseStatus.Cleared);
				var consignment3 = BuildConsignmentWithReleaseStatusForCurrentDirection(shipment.PK, consignmentHeader.PK, "NZ", "AU", HVLVReleaseStatus.Cleared, HVLVReleaseStatus.None);

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var declarationMenuGroup = customsMenu.MenuItems.OfType<DeclarationMenuGroup>().Single();

					customsMenu.OnPopup(EventArgs.Empty);

					var menuItem = declarationMenuGroup.MenuItems.OfType<ConvertConsignmentsToStandAloneDeclarationMenuItem>().Single();
					menuItem.PerformClick();

					AssertEquals("The 'Convert Consignments to Stand Alone Declarations' menu item should display this message when all consignments have been cleared", "All consignments are cleared. There are no uncleared consignments to convert to Stand Alone Declarations.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestConvertConsignmentsToStandAloneDeclarations_WhenShipmentHasChanges_ThenPromptUserToSave()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";
			BuildConsignmentWithReleaseStatusForCurrentDirection(shipment.PK, ZGuid.Empty, "NZ", "AU", HVLVReleaseStatus.Held, HVLVReleaseStatus.None);

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var menuHVLV = plugin.TopLevelMenu;
				menuHVLV.PerformSelect();
				menuHVLV.OnPopup(EventArgs.Empty);

				var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				var declarationMenuGroup = customsMenu.MenuItems.OfType<DeclarationMenuGroup>().Single();

				customsMenu.OnPopup(EventArgs.Empty);

				shipment.JS_GoodsDescription = "New description";

				var menuItem = declarationMenuGroup.MenuItems.OfType<ConvertConsignmentsToStandAloneDeclarationMenuItem>().Single();
				menuItem.PerformClick();

				AssertEquals("Please save any changes made before creating Stand Alone Declarations", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConvertConsignmentsToStandAloneDeclarations_WhenShipmentConsolHasChangesThenPromptUserToSave()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			BuildConsignmentWithReleaseStatusForCurrentDirection(shipment.PK, ZGuid.Empty, "NZ", "AU", HVLVReleaseStatus.Held, HVLVReleaseStatus.None);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_MasterBillNum = "MAWB1234";
			consol.Transports.MostInterestingTransport.JW_VoyageFlight = "Flight100";
			consol.Shipments.Add(shipment);

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var menuHVLV = plugin.TopLevelMenu;
				menuHVLV.PerformSelect();

				var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				var declarationMenuGroup = customsMenu.MenuItems.OfType<DeclarationMenuGroup>().Single();

				customsMenu.OnPopup(EventArgs.Empty);

				consol.JK_MasterBillNum = "New MasterBill";

				var menuItem = declarationMenuGroup.MenuItems.OfType<ConvertConsignmentsToStandAloneDeclarationMenuItem>().Single();
				menuItem.PerformClick();

				AssertEquals("Please save any changes made before creating Stand Alone Declarations", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConvertConsignmentsToStandAloneDeclarations_DoesNotRequireCargoReportCreated()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.Consols.AddNew();

			var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

			var consignment1 = BuildConsignmentWithReleaseStatusForCurrentDirection(shipment.PK, consignmentHeader.PK, "NZ", "AU", HVLVReleaseStatus.Held, HVLVReleaseStatus.None);
			consignment1.HVC_GoodsDescription = "Consignment1 with Job Declaration";

			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var menuHVLV = plugin.TopLevelMenu;
				menuHVLV.PerformSelect();

				var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				var declarationMenuGroup = customsMenu.MenuItems.OfType<DeclarationMenuGroup>().Single();

				customsMenu.OnPopup(EventArgs.Empty);
				var menuItem = declarationMenuGroup.MenuItems.OfType<ConvertConsignmentsToStandAloneDeclarationMenuItem>().Single();
				menuItem.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals("Pre-Condition: cargo report not created", shipment.IsCargoReportCreated(), false);
					AssertEquals("The convert consignments to stand alone declarations form should be now have opened", typeof(HVLVConsignmentsToStandAloneDeclarationsForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				});
			}
		}

		HVLVConsignment BuildConsignmentWithReleaseStatusForCurrentDirection(ZGuid shipmentPK, ZGuid headerPK, string shipperCountry, string consigneeCountry, string importReleaseStatus, string exportReleaseStatus)
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_RN_NKShipperCountryCode = shipperCountry;
			consignment.HVC_RN_NKConsigneeCountryCode = consigneeCountry;
			consignment.HVC_ImportReleaseStatus = importReleaseStatus;
			consignment.HVC_ExportReleaseStatus = exportReleaseStatus;
			consignment.HVC_JS_ManifestedOnShipment = shipmentPK;
			consignment.HVC_HCH_Header = headerPK;
			consignment.HVC_ReleaseStatus = importReleaseStatus;
			return consignment;
		}
	}
}
