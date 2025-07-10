using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.eTail.Business;
using Enterprise.eTail.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.PortHubs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.GUI.Testing
{
	public class HVLVMenuItemTestHelper : TestCase
	{
		public static ForwardingShipment CreateShipmentValidForNewETailData(BusinessObjectFactory factory, bool setConsignorAddresses = true, bool createItem = true, bool createItemLine = false, int numOfRecords = 1)
		{
			var shipment = factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RS_NKServiceLevel = "STD";

			var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

			for (var i = 0; i < numOfRecords; i++)
			{
				var consignment = consignmentHeader.Consignments.AddNew();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_WaybillNumber = i == 0 ? "1234567890" : "54321" + i.ToString("D7");

				if (createItem)
				{
					var item = consignment.Items.AddNew();
					item.HVI_JS_LoadedOnShipment = shipment.PK;

					if (createItemLine)
					{
						var itemLine = item.Lines.AddNew();
						itemLine.HVS_Quantity = 1;
					}
				}
			}

			if (setConsignorAddresses)
			{
				var consignorDocumentaryAddress = shipment.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsignorDocumentaryAddress);
				consignorDocumentaryAddress.E2_OA_Address = factory.NewWithValidTestData<OrgAddress>().PK;

				var consignorPickupAddress = shipment.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsignorPickupDeliveryAddress);
				consignorPickupAddress.E2_OA_Address = factory.NewWithValidTestData<OrgAddress>().PK;
			}
			factory.Save();

			return shipment;
		}

		public static ForwardingShipment CreateShipmentWithAddress(BusinessObjectFactory factory, out OrgAddress depotAddress, out OrgHeader carrier, out OrgHeader agent)
		{
			var creator = new PortHubSelectionTestDataCreator(factory);

			var billToParty = creator.GenerateAddress("AD1", "XY1", "XY 1");
			var dispatchDepotAddress1 = creator.GenerateAddress("AD2", "XY2", "XY 2");
			depotAddress = creator.GenerateAddress("AD3", "XY3", "XY 3");
			carrier = creator.GenerateOrganisation("XY4", "XY 4");
			agent = creator.GenerateOrganisation("ZZ5", "ZZ 5");

			var portHubSelectionPK = creator.CreatePortAndDepotSelectionWithUndgClass(depotAddress.PK, dispatchDepotAddress1.PK, "EXP", "DLV", "ALL", "AAA", "ALL");
			var portHubSelection = factory.Load<PortHubSelection>(portHubSelectionPK);
			portHubSelection.TY_OH_CarrierBookingAgent = agent.PK;
			var zonePK = creator.AddZone(portHubSelectionPK, "Z2", carrier.PK, "EXP");
			creator.AddZoneItem(zonePK, "Melbourne Metro", "VIC", "AU");

			factory.Save();

			var shipment = factory.NewWithValidTestData<HVLVForwardingShipment>();
			shipment.JS_RS_NKServiceLevel = "EXP";
			var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

			var consignment1 = factory.New<HVLVConsignment>();
			consignment1.HVC_ConsignmentId = "CONSIGN100";
			consignment1.HVC_UndgClass = "6";
			consignment1.HVC_ConsigneeAddress1 = "Test Address 11";
			consignment1.HVC_ConsigneeCity = "Melbourne Metro";
			consignment1.HVC_ConsigneeState = "VIC";
			consignment1.HVC_ConsigneePostcode = "3560";
			consignment1.HVC_RN_NKConsigneeCountryCode = "AU";
			consignment1.Items.AddNew().HVI_JS_LoadedOnShipment = shipment.PK;
			consignment1.HVC_HCH_Header = consignmentHeader.PK;

			var consignment2 = factory.New<HVLVConsignment>();
			consignment2.HVC_ConsignmentId = "CONSIGN101";
			consignment2.HVC_UndgClass = "6";
			consignment2.HVC_ConsigneeAddress1 = "Test Address 12";
			consignment2.HVC_ConsigneeCity = "Melbourne Metro";
			consignment2.HVC_ConsigneeState = "VIC";
			consignment2.HVC_ConsigneePostcode = "3561";
			consignment2.HVC_RN_NKConsigneeCountryCode = "AU";
			consignment2.Items.AddNew().HVI_JS_LoadedOnShipment = shipment.PK;
			consignment2.HVC_HCH_Header = consignmentHeader.PK;

			return shipment;
		}

		public enum CargoReportAction { Create, Open, Sync }

		static void SetupCargoReportFormAndAssertMenuItem(string menuItemCaption, string actionName, ForwardingShipment shipment, CargoReportAction cargoReportAction, Action<ZMenuItem> assertionsToRun, bool setupGenPivotForTesting = true)
		{
			SetupForCargoReport(shipment, cargoReportAction, setupGenPivotForTesting);

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var hVLVMenu = plugin.TopLevelMenu;
				hVLVMenu.PerformSelect();

				var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				customsMenu.OnPopup(EventArgs.Empty);

				var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().FirstOrDefault(menu => menu.Caption == menuItemCaption);
				var menuItem = jobCommandMenuGroup?.MenuItems.Cast<ZMenuItem>().SingleOrDefault(x => x.Caption == string.Format("{0} {1}", actionName, menuItemCaption));
				assertionsToRun.Invoke(menuItem);
			}
		}

		static void SetupForCargoReport(ForwardingShipment shipment, CargoReportAction cargoReportAction, bool createGenPivotRecord)
		{
			if (cargoReportAction != CargoReportAction.Create)
			{
				if (createGenPivotRecord)
				{
					CreateGenPivotRecord(shipment);
				}

				if (cargoReportAction == CargoReportAction.Open)
				{
					shipment.Logs.AddNew(AutoEvents.HVLVReady, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, EventReferenceParameterReasons.CargoReportCreated));
				}
				else if (cargoReportAction == CargoReportAction.Sync)
				{
					shipment.Logs.AddNew(AutoEvents.HVLVReady, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, EventReferenceParameterReasons.CargoReportAmendPending));
				}
			}
		}

		public static void CreateGenPivotRecord(ForwardingShipment shipment, Type customsJobInterfaceType = null)
		{
			if (customsJobInterfaceType == null)
			{
				customsJobInterfaceType = GlbBranch.CurrentBranch.Country.Code.ToString() switch
				{
					CountryCodes.UnitedStates => typeof(Enterprise.Integration.Customs.US.LVS.ICusUSLVClearance),
					CountryCodes.Australia => shipment.TransportMode == TransportModes.Air ? typeof(Enterprise.Integration.Customs.Shared.ICusMAWB) : typeof(Enterprise.Integration.Customs.AU.ICusSCAOceanBill),
					CountryCodes.NewZealand => shipment.TransportMode == TransportModes.Air ? typeof(Enterprise.Integration.Customs.Shared.ICusMAWB) : typeof(Enterprise.Integration.Customs.NZ.ICusSCAOceanBill),
					CountryCodes.Singapore or CountryCodes.Taiwan or CountryCodes.Turkey => typeof(Enterprise.Integration.Customs.ASYCUDA.IAsycudaManifestHeader),
					_ => default,
				};
			}

			var genPivot = shipment.Factory.NewWithValidTestData(ObjectFactory.GetType(customsJobInterfaceType));

			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			header.GenPivotCollection.AddRelatedIfNotExist(genPivot);

			shipment.Factory.Save();
		}

		public static void AssertCargoReportMenuItemVisibility(bool expectVisible, string menuItemCaption, string actionName, ForwardingShipment shipment, CargoReportAction cargoReportAction)
		{
			SetupCargoReportFormAndAssertMenuItem(menuItemCaption, actionName, shipment, cargoReportAction, (menuItem) =>
			{
				AssertEquals(expectVisible, menuItem != null);
			});

			SetupCargoReportFormAndAssertMenuItem(menuItemCaption, actionName, shipment, cargoReportAction, (menuItem) =>
			{
				AssertEquals(expectVisible, menuItem != null);
			},
			setupGenPivotForTesting: false);
		}

		public static void AssertErrorMessageWhenCannotFindCargoReport(string menuItemCaption, string actionName, ForwardingShipment shipment, string expectErrorMessage)
		{
			SetupCargoReportFormAndAssertMenuItem(menuItemCaption, actionName, shipment, CargoReportAction.Open, (menuItem) =>
			{
				Assert("Precondition : currently show open cargo report menu item", menuItem.Visible);

				UnitTestUserNotification.Instance.AddOKAnswer();
				menuItem.PerformClick();

				var errorMessage = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals(expectErrorMessage, errorMessage);
			},
			setupGenPivotForTesting: false);
		}

		public static void AssertHLRLogs(ForwardingShipment shipment, string[] expectedReasons)
		{
			var hlrReasons = shipment.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.HVLVReadyCode)
				.OrderBy(x => x.SL_EventTime)
				.Select(x => x.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason])
				.ToArray();

			AssertEquals(expectedReasons.Length, hlrReasons.Length);

			for (var i = 0; i < expectedReasons.Length; i++)
			{
				AssertEquals(expectedReasons[i], hlrReasons[i]);
			}
		}

		public static void AssertEcommercePortalNavigated(string expectedPortalCode, MenuItem navigateToEcommercePortalMenuItem)
		{
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/"))
			using (var response = new HttpResponseMessage())
			{
				var clientMock = new Mock<IGlowServiceClient>();
				clientMock.Setup(c => c.GetAsync("api")).ReturnsAsync(response);

				var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
				clientFactoryMock.Setup(f => f.Create(new Uri("https://address/"))).Returns(clientMock.Object);
				ObjectFactory.Substitute(clientFactoryMock.Object);

				navigateToEcommercePortalMenuItem.PerformClick();

				var launchedUrl = WebUrlLauncher.LastUrlLaunched;
				var uri = new Uri(launchedUrl);
				var queryKeyValuePairs = HttpUtility.ParseQueryString(uri.Query);
				var accessToken = queryKeyValuePairs["sso_otp"];

				AssertEquals("https", uri.Scheme);
				AssertEquals("address", uri.Host);
				AssertEquals("/" + expectedPortalCode, uri.AbsolutePath);
				AssertEquals(string.Empty, uri.Fragment);

				var consumed = ObjectFactory.Get<ITokenizedAccessControl>().TryConsume(accessToken, AccessTokenTypes.LocalIdentity, out var tokenInfo);
				Assert(nameof(consumed), consumed);
				AssertEquals(Env.CurrentUserPK, tokenInfo.ParentId);
				AssertEquals(GlbStaffSchema.Constants.Prefix, tokenInfo.ParentTableCode);
			}
		}

		public static void AssertWebPortalMenuItemShowsErrorMessage(MenuItem navigateToEcommercePortalMenuItem)
		{
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			using (var response = new HttpResponseMessage())
			{
				navigateToEcommercePortalMenuItem.PerformClick();

				AssertEquals(@"This module cannot be opened in a browser as GLOW has not been configured for this client.
Registry: GLOW/Services/GLOW Portals Root URL", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
