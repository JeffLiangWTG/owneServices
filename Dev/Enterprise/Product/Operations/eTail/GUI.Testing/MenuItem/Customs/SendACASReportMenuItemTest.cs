using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.Business;
using Enterprise.eTail.Business;
using Enterprise.eTail.Business.Testing;
using Enterprise.eTail.Module;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.GUI.Testing
{
	public class SendACASReportMenuItemTest : SendACASMessageMenuItemTest<SendACASReportMenuItem>
	{
		public void TestSendACASReportVisibility()
		{
			var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, true);
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
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
				var sendACASReportMenuItem = sendACASMenuGroup.MenuItems.OfType<SendACASReportMenuItem>().Single();
				Assert("Show sendACASReportMenuItem if the shipment is AIR and US import", sendACASReportMenuItem.Visible);

				shipment.JS_TransportMode = TransportModes.Sea;
				sendACASMenuGroup.OnPopup(EventArgs.Empty);
				sendACASReportMenuItem = sendACASMenuGroup.MenuItems.OfType<SendACASReportMenuItem>().Single();
				Assert("Hide sendACASReportMenuItem if the shipment transport mode is not AIR", !sendACASReportMenuItem.Visible);

				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKDestination = "AUSYD";
				sendACASMenuGroup.OnPopup(EventArgs.Empty);
				sendACASReportMenuItem = sendACASMenuGroup.MenuItems.OfType<SendACASReportMenuItem>().Single();
				Assert("Hide sendACASReportMenuItem if the shipment transport mode is not US import", !sendACASReportMenuItem.Visible);
			}
		}

		public void TestMenuItemAction_WhenNoConsignmentsAndWhenRegistryDisabledFilingForNonUSBranchAndNonUSBranch_ShowError()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			using (HVLVDataRegistry.Instance.EnableSecurityFilingsForNonUSACompanies.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertMenuItemAction_WhenNoConsignments_ShowsMessage(@"US Security Filings are not enabled for this branch.

To enable go to Registry -> Freight -> HVLV -> Customs -> United States of America -> Enable Security Filings for non USA based Companies");
			}
		}

		public void TestMenuItemAction_WhenNoConsignmentsAndWhenRegistryEnabledFilingForNonUSBranchAndNonUSBranch_ShowMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			using (HVLVDataRegistry.Instance.EnableSecurityFilingsForNonUSACompanies.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertMenuItemAction_WhenNoConsignments_ShowsMessage();
			}
		}

		public void TestMenuItemAction_WhenNoConsignmentsAndWhenRegistryEnabledFilingForNonUSBranchAndUSBranch_ShowMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			using (HVLVDataRegistry.Instance.EnableSecurityFilingsForNonUSACompanies.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertMenuItemAction_WhenNoConsignments_ShowsMessage();
			}
		}

		public void TestMenuItemAction_WhenNoConsignmentsAndWhenRegistryDisabledFilingForNonUSBranchAndUSBranch_ShowMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			using (HVLVDataRegistry.Instance.EnableSecurityFilingsForNonUSACompanies.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertMenuItemAction_WhenNoConsignments_ShowsMessage();
			}
		}

		void AssertMenuItemAction_WhenNoConsignments_ShowsMessage(string expectedMessage = "Please make sure there's at least one active consignment on this shipment.")
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RS_NKServiceLevel = "STD";
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKDestination = "USLAX";

			HVLVConsignmentHeader.GetOrCreate(shipment);

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
				var sendACASReportMenuItem = sendACASMenuGroup.MenuItems.OfType<SendACASReportMenuItem>().Single();

				sendACASReportMenuItem.PerformClick();
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMenuItemAction_WhenAllConsignmentACASReportHasAlreadyBeenSent_ShowsMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_RS_NKServiceLevel = "STD";
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKDestination = "USLAX";

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				var consignment = consignmentHeader.Consignments.AddNew();
				consignment.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.OriginalSent;

				var item = consignment.Items.AddNew();
				item.HVI_JS_LoadedOnShipment = shipment.PK;

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
					var sendACASReportMenuItem = sendACASMenuGroup.MenuItems.OfType<SendACASReportMenuItem>().Single();

					sendACASReportMenuItem.PerformClick();
					AssertEquals("Consignment ACAS reports have already been sent, please wait for further response.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestSendACASReport_WhenShipmentHasMultipleConsignments_OnlyHitsHVLVItemAndItemLineTableOnce()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var country = RefCountry.LoadFromCountryCode(Factory, "US");
				GlbCompany.CurrentCompany.OrgProxy.SetCustomsCode(OrgCusCode.USACodeTypes.ACASOriginatorCode, country, "Code");

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_RS_NKServiceLevel = "STD";
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKDestination = "USLAX";

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				Factory.Save();

				var consignment1 = consignmentHeader.Consignments.AddNew();
				consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment1.HVC_WaybillNumber = "Waybill";
				var item1 = consignment1.Items.AddNew();
				item1.HVI_JS_LoadedOnShipment = shipment.PK;
				var itemLine1 = item1.Lines.AddNew();
				itemLine1.HVS_Quantity = 1;
				itemLine1.HVS_GoodsDescription = "I'm a purse";

				var consignment2 = consignmentHeader.Consignments.AddNew();
				consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment2.HVC_WaybillNumber = "Waybill2";
				var item2 = consignment2.Items.AddNew();
				item2.HVI_JS_LoadedOnShipment = shipment.PK;
				var itemLine2 = item2.Lines.AddNew();
				itemLine2.HVS_Quantity = 1;
				itemLine2.HVS_GoodsDescription = "I'm a pair of sneakers";

				Factory.Save();

				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var shipmentInNewFactory = newFactory.Load<ForwardingShipment>(shipment.PK);

				using (var form = new ZForm(shipmentInNewFactory))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var hVLVMenu = plugin.TopLevelMenu;
					hVLVMenu.PerformSelect();

					var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var sendACASGroupMenu = customsMenu.MenuItems.OfType<SendACASMenuGroup>().Single();
					var sendACASReportMenuItem = sendACASGroupMenu.MenuItems.OfType<SendACASReportMenuItem>().Single();

					HVLVItemCollection.SetCollectionCountForTest(newFactory, consignmentHeader.HCH_ClusterKey, 5);
					HVLVItemLineCollection.SetCollectionCountForTest(newFactory, consignmentHeader.HCH_ClusterKey, 5);

					var expectedDbHits = new Dictionary<string, int>()
					{
						{ HVLVItemSchema.Constants.TableName, 1 },
						{ HVLVItemLineSchema.Constants.TableName, 1 },
					};

					using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory, true))
					{
						sendACASReportMenuItem.PerformClick();
					}
				}
			}
		}

		public void TestSendACASReport_OnlyShowsConsignments_WithEmptyMessageStatus()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var country = RefCountry.LoadFromCountryCode(Factory, "US");
				GlbCompany.CurrentCompany.OrgProxy.SetCustomsCode(OrgCusCode.USACodeTypes.ACASOriginatorCode, country, "Code");

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_RS_NKServiceLevel = "STD";
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKDestination = "USLAX";

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_OA_ArrivalCTOAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
				shipment.Consols.Add(consol);

				var ctoAddress = Factory.New<OrgHeader>();
				ctoAddress.OH_FullName = "CTO";
				ctoAddress.OH_RL_NKClosestPort = "USLAX";
				ctoAddress.MainAddress.Address1 = "House 16777214";
				ctoAddress.MainAddress.Address2 = "Coelosis inermis";
				ctoAddress.MainAddress.City = "The Big City";
				ctoAddress.MainAddress.Postcode = "1234";
				ctoAddress.MainAddress.OA_RN_NKCountryCode = "US";
				ctoAddress.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "123", "US");
				consol.JK_OA_ArrivalCTOAddress = ctoAddress.MainAddress.PK;

				var consignment1 = consignmentHeader.Consignments.AddNew();
				consignment1.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.OriginalSent;
				consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment1.HVC_GoodsDescription = "AAAA";
				consignment1.HVC_WaybillNumber = "Waybill";
				consignment1.HVC_IsActive = true;

				var item = consignment1.Items.AddNew();
				item.HVI_JS_LoadedOnShipment = shipment.PK;

				var consignment2 = consignmentHeader.Consignments.AddNew();
				consignment2.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.AmendmentRequired;
				consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment2.HVC_GoodsDescription = "BBBB";
				consignment2.HVC_WaybillNumber = "Waybill2";

				consignment2.HVC_IsActive = true;

				var item2 = consignment2.Items.AddNew();
				item2.HVI_JS_LoadedOnShipment = shipment.PK;

				var consignment3 = consignmentHeader.Consignments.AddNew();
				consignment3.HVC_ACASMessageStatus = ZString.Empty;
				consignment3.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment3.HVC_GoodsDescription = "CCCC";
				consignment3.HVC_IsActive = true;
				consignment3.HVC_WaybillNumber = "Waybill3";

				var item3 = consignment3.Items.AddNew();
				item3.HVI_JS_LoadedOnShipment = shipment.PK;

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment) as ETailShipmentPlugin;
					var hVLVMenu = plugin.TopLevelMenu;
					hVLVMenu.PerformSelect();

					var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var sendACASMenuGroup = customsMenu.MenuItems.OfType<SendACASMenuGroup>().Single();
					var sendACASReportMenuItem = sendACASMenuGroup.MenuItems.OfType<SendACASReportMenuItem>().Single();

					sendACASReportMenuItem.PerformClick();
					Application.DoEvents();
					AssertEquals("Precondition: Should be HVLVConsignmentACASValidationForm", typeof(HVLVConsignmentACASValidationForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

					var consignmentWrappers = ZFormModaliser.LastIBusinessShownOnDialogForTest as HVLVConsignmentForACASWrapperCollection;
					AssertEquals("Send ACAS Report should only show consignments with empty message status", 1, consignmentWrappers.Count);
					AssertContainsExactElementsInAnyOrder(new[] { consignment3 }, consignmentWrappers.Select(x => x.Consignment));
				}
			}
		}

		public void TestGivenUSACASShipment_WhenSendingEDIMessage_AndRegistryIsTrue_ThenNotRemoveEuropeanNonWesternCharactersFromEDIMessage()
		{
			var country = RefCountry.LoadFromCountryCode(Factory, "US");
			GlbCompany.CurrentCompany.OrgProxy.SetCustomsCode(OrgCusCode.USACodeTypes.ACASOriginatorCode, country, "Code");
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_OA_ArrivalCTOAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			shipment.Consols.Add(consol);
			var ctoAddress = Factory.New<OrgHeader>();
			ctoAddress.OH_FullName = "CTO";
			ctoAddress.OH_RL_NKClosestPort = "USLAX";
			ctoAddress.MainAddress.Address1 = "House 16777214";
			ctoAddress.MainAddress.Address2 = "Coelosis inermis";
			ctoAddress.MainAddress.City = "The Big City";
			ctoAddress.MainAddress.Postcode = "1234";
			ctoAddress.MainAddress.OA_RN_NKCountryCode = "US";
			ctoAddress.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "123", "US");
			consol.JK_OA_ArrivalCTOAddress = ctoAddress.MainAddress.PK;
			var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);
			var consignment1 = consignmentHeader.Consignments.AddNew();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_WaybillNumber = "Waybill";
			consignment1.HVC_GoodsDescription = "Description";
			consignment1.HVC_RN_NKConsigneeCountryCode = "US";
			consignment1.HVC_ConsigneeName = "上海Name";
			consignment1.HVC_ConsigneePostcode = "はいPostcode";
			consignment1.HVC_ConsigneeState = "아NSW";
			consignment1.HVC_ConsigneeAddress1 = "House 16777214";
			consignment1.HVC_ConsigneeCity = "The Big City";
			consignment1.HVC_ConsigneeMobile = "+1234567890";
			consignment1.HVC_ConsigneeEmail = "Name@email.com";
			consignment1.HVC_ShipperName = "Test Company";
			consignment1.HVC_ShipperAddress1 = "Test Address";
			consignment1.HVC_ShipperState = "NSW";
			consignment1.HVC_ShipperPostcode = "123456";
			consignment1.HVC_ShipperCity = "Sydney";
			consignment1.HVC_RN_NKShipperCountryCode = "AU";
			consignment1.HVC_ShipperMobile = "+9876543210";
			consignment1.HVC_ShipperEmail = "TestCompany@email.com";
			var item = consignment1.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;
			Factory.Save();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			using (var form = new ZForm(shipment))
			{
				HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersUSACAS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var hVLVMenu = plugin.TopLevelMenu;
				hVLVMenu.PerformSelect();

				var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				var sendACASMenuGroup = customsMenu.MenuItems.OfType<SendACASMenuGroup>().Single();
				form.FireSaveButton();
				plugin.OnSaveCompletedOrAborted(true);
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					if (obj is HVLVConsignmentACASValidationForm form1)
					{
						form1.Show();
						form1.GetControl<ZToolStrip>("toolStrip").Items.Find("SendMessageButton", true).First().PerformClick();
					}
				});
				sendACASMenuGroup.MenuItems.OfType<SendACASReportMenuItem>().Single().PerformClick();
				var interchanges = Factory.Load<EDIMessage>(new ZQuery());
				CombineAssertions("Expected EDIMessages to be created", () =>
				{
					AssertEquals("Expected 1 EDIMessage generated", 1, interchanges.Length);
					AssertEquals("Expected EDIMessage to be an ACAS report", "ADVANCE_AIR_CARGO_REPORT", interchanges[0].EM_InterchangeReceiver);
					Assert("Expected EDIMessage text body to not contain the non-English characters from consignment name", !interchanges[0].EM_MessageText.Contains("上海"));
					Assert("Expected EDIMessage text body to not contain the non-English characters from consignment postcode", !interchanges[0].EM_MessageText.Contains("はい"));
					Assert("Expected EDIMessage text body to not contain the non-English characters from consignment state", !interchanges[0].EM_MessageText.Contains("아"));
				});
			}
		}

		public void TestGivenUSACASShipment_WhenSendingEDIMessage_AndRegistryIsFalse_ThenRemoveEuropeanNonWesternCharactersFromEDIMessage()
		{
			var country = RefCountry.LoadFromCountryCode(Factory, "US");
			GlbCompany.CurrentCompany.OrgProxy.SetCustomsCode(OrgCusCode.USACodeTypes.ACASOriginatorCode, country, "Code");
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_OA_ArrivalCTOAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			shipment.Consols.Add(consol);
			var ctoAddress = Factory.New<OrgHeader>();
			ctoAddress.OH_FullName = "CTO";
			ctoAddress.OH_RL_NKClosestPort = "USLAX";
			ctoAddress.MainAddress.Address1 = "House 16777214";
			ctoAddress.MainAddress.Address2 = "Coelosis inermis";
			ctoAddress.MainAddress.City = "The Big City";
			ctoAddress.MainAddress.Postcode = "1234";
			ctoAddress.MainAddress.OA_RN_NKCountryCode = "US";
			ctoAddress.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "123", "US");
			consol.JK_OA_ArrivalCTOAddress = ctoAddress.MainAddress.PK;
			var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);
			var consignment1 = consignmentHeader.Consignments.AddNew();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_WaybillNumber = "Waybill";
			consignment1.HVC_GoodsDescription = "Description";
			consignment1.HVC_RN_NKConsigneeCountryCode = "US";
			consignment1.HVC_ConsigneeName = "上海Name";
			consignment1.HVC_ConsigneePostcode = "はいPostcode";
			consignment1.HVC_ConsigneeState = "아NSW";
			consignment1.HVC_ConsigneeAddress1 = "House 16777214";
			consignment1.HVC_ConsigneeCity = "The Big City";
			consignment1.HVC_ConsigneeMobile = "+1234567890";
			consignment1.HVC_ConsigneeEmail = "Name@email.com";
			consignment1.HVC_ShipperName = "Test Company";
			consignment1.HVC_ShipperAddress1 = "Test Address";
			consignment1.HVC_ShipperState = "NSW";
			consignment1.HVC_ShipperPostcode = "123456";
			consignment1.HVC_ShipperCity = "Sydney";
			consignment1.HVC_RN_NKShipperCountryCode = "AU";
			consignment1.HVC_ShipperMobile = "+9876543210";
			consignment1.HVC_ShipperEmail = "TestCompany@email.com";
			var item = consignment1.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;
			Factory.Save();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			using (var form = new ZForm(shipment))
			{
				HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersUSACAS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var hVLVMenu = plugin.TopLevelMenu;
				hVLVMenu.PerformSelect();

				var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				var sendACASMenuGroup = customsMenu.MenuItems.OfType<SendACASMenuGroup>().Single();
				form.FireSaveButton();
				plugin.OnSaveCompletedOrAborted(true);
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					if (obj is HVLVConsignmentACASValidationForm form1)
					{
						form1.Show();
						form1.GetControl<ZToolStrip>("toolStrip").Items.Find("SendMessageButton", true).First().PerformClick();
					}
				});
				sendACASMenuGroup.MenuItems.OfType<SendACASReportMenuItem>().Single().PerformClick();
				var interchanges = Factory.Load<EDIMessage>(new ZQuery());
				CombineAssertions("Expected EDIMessages to be created", () =>
				{
					AssertEquals("Expected 1 EDIMessage generated", 1, interchanges.Length);
					AssertEquals("Expected EDIMessage to be an ACAS report", "ADVANCE_AIR_CARGO_REPORT", interchanges[0].EM_InterchangeReceiver);
					Assert("Expected EDIMessage text body to contain the non-English characters from consignment name", interchanges[0].EM_MessageText.Contains("上海Name"));
					Assert("Expected EDIMessage text body to contain the non-English characters from consignment postcode", interchanges[0].EM_MessageText.Contains("はいPostcode"));
					Assert("Expected EDIMessage text body to contain the non-English characters from consignment state", interchanges[0].EM_MessageText.Contains("아NSW"));
				});
			}
		}
	}
}
