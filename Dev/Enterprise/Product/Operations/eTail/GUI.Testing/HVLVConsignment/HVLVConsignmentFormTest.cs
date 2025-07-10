using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.eTail.Business;
using Enterprise.eTail.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.PortHubs.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.GUI.Testing
{
	[TestedType(typeof(HVLVConsignmentForm))]
	class HVLVConsignmentFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();

			Factory.Save();

			using (var form = new HVLVConsignmentForm(consignment))
			{
				form.Show();

				AssertEquals($"Consignment {consignment.HVC_ConsignmentId}", form.FormCaption);
			}
		}

		public void TestAllowNew()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();

			Factory.Save();

			using (var form = new HVLVConsignmentFormForTest(consignment))
			{
				form.Show();
				Assert(!form.AllowNewForTest);
			}
		}

		public void TestVesselDetails_WhenSeaShipment_IsVisible()
		{
			var seaShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			seaShipment.JS_TransportMode = TransportModes.Sea;
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = seaShipment.PK;

			using (var form = new HVLVConsignmentForm(consignment))
			{
				form.Show();

				var vesselTextBox = form.Controls.Find("vesselTextBox", true).Single() as ZTextBox;
				AssertEquals("Should be visible", true, vesselTextBox.Visible);
			}
		}

		public void TestVesselDetails_WhenNotSeaShipment_IsNotVisible()
		{
			var airShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			airShipment.JS_TransportMode = TransportModes.Air;
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = airShipment.PK;

			using (var form = new HVLVConsignmentForm(consignment))
			{
				form.Show();

				var vesselTextBox = form.Controls.Find("vesselTextBox", true).Single() as ZTextBox;
				AssertEquals("Should not be visible", false, vesselTextBox.Visible);
			}
		}

		public void TestFormPlugins()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			using (var form = new HVLVConsignmentForm(consignment))
			{
				form.Show();

				CombineAssertions("Required PlugIns should have been added", () =>
				{
					AssertNotNull("DocData PlugIn", form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
				});
			}
		}

		public void TestChargeableMenuItemClick()
		{
			var bookingHeader = Factory.New<HVLVBookingHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_WeightUQ = Weight.Kilograms;
			consignment.HVC_VolumeUQ = Volume.CubicCentimeters;
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			AssertEquals("Chargeable for display not returning correct value for null input", "Not Calculated", consignment.ChargeableForDisplay);
			AssertEquals("Pre-condition", ConversionFactor.Standard.Metric.Air, FreightDataRegistry.Instance.InternationalChargeableFactorAir.Value.MetricFactor);

			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();

			using (var form = new HVLVConsignmentForm(consignment))
			using (HVLVDataRegistry.Instance.CalculateHVLVChargeablePerItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				item1.HVI_ManifestedWeight = 8;
				item1.HVI_ActualWeight = 5.3;
				item1.HVI_ActualVolume = 36000; // 6 KG chargeable
				item2.HVI_ManifestedWeight = 7.4;
				item2.HVI_ManifestedVolume = 27000; // 4.5 KG chargeable

				form.Show();

				var menuItem = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.OfType<MenuItem>().Single(item => item.Text == HVLVMenuItemHelper.Captions.CalculateChargeable);
				menuItem.PerformClick();

				AssertEquals("Chargeable calculated from item1 actual volume and item2 manifested weight", "13.4 KG", consignment.ChargeableForDisplay);
			}
		}

		public void TestCalculateLMCDepotDetailsContextMenuItemClick()
		{
			var creator = new PortHubSelectionTestDataCreator(Factory);

			var billToParty = creator.GenerateAddress("AD1", "XY1", "XY 1");
			var dispatchDepotAddress = creator.GenerateAddress("AD2", "XY2", "XY 2");
			var depotAddress = creator.GenerateAddress("AD3", "XY3", "XY 3");
			var carrier = creator.GenerateOrganisation("XY4", "XY 4");
			var agent = creator.GenerateOrganisation("ZZ5", "ZZ 5");

			var portHubSelectionPK = creator.CreatePortAndDepotSelectionWithUndgClass(depotAddress.PK, dispatchDepotAddress.PK, "EXP", "DLV", "ALL", "AAA", "ALL");
			var portHubSelection = Factory.Load<PortHubSelection>(portHubSelectionPK);
			portHubSelection.TY_OH_CarrierBookingAgent = agent.PK;
			var zonePK = creator.AddZone(portHubSelectionPK, "Z2", carrier.PK, "EXP");
			creator.AddZoneItem(zonePK, "Melbourne Metro", "VIC", "AU");

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.HVH_BookingReference = "M00000050";
			bookingHeader.HVH_OA_BillToParty = billToParty.PK;
			bookingHeader.HVH_RS_NKBookingServiceLevel = "EXP";
			bookingHeader.HVH_OA_DispatchAddress = dispatchDepotAddress.PK;

			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_ConsignmentId = "CONSIGN100";
			consignment.HVC_UndgClass = "6";
			consignment.HVC_ConsigneeAddress1 = "Test Address 11";
			consignment.HVC_ConsigneeCity = "Melbourne Metro";
			consignment.HVC_ConsigneeState = "VIC";
			consignment.HVC_ConsigneePostcode = "3560";
			consignment.HVC_RN_NKConsigneeCountryCode = "AU";
			Factory.Save();

			CombineAssertions("Precondition: LMC Depot Details should be empty", () =>
			{
				AssertEquals("HVC_OA_DestinationDepot", ZGuid.Empty, consignment.HVC_OA_DestinationDepot);
				AssertEquals("HVC_OH_LastMileCarrier", ZGuid.Empty, consignment.HVC_OH_LastMileCarrier);
				AssertEquals("HVC_PL_NKLastMileCarrierServiceLevel", ZString.Empty, consignment.HVC_PL_NKLastMileCarrierServiceLevel);
				AssertEquals("HVC_OH_LastMileCarrierBookingAgent", ZGuid.Empty, consignment.HVC_OH_LastMileCarrierBookingAgent);
			});

			using (var form = new HVLVConsignmentForm(consignment))
			{
				form.Show();

				var menuItem = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.CalculateLMCDepotDetails);
				AssertNotNull(menuItem);
				menuItem.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals(depotAddress.PK, consignment.HVC_OA_DestinationDepot);
					AssertEquals(carrier.PK, consignment.HVC_OH_LastMileCarrier);
					AssertEquals("EXP", consignment.HVC_PL_NKLastMileCarrierServiceLevel);
					AssertEquals(agent.PK, consignment.HVC_OH_LastMileCarrierBookingAgent);
				});

				PortHubSelectionTestDataCreator.ClearUpAllZoneItems(Factory);
				menuItem.PerformClick();

				CombineAssertions("LMC Depot Details should be set back to empty", () =>
				{
					AssertEquals("HVC_OA_DestinationDepot", ZGuid.Empty, consignment.HVC_OA_DestinationDepot);
					AssertEquals("HVC_OH_LastMileCarrier", ZGuid.Empty, consignment.HVC_OH_LastMileCarrier);
					AssertEquals("HVC_PL_NKLastMileCarrierServiceLevel", ZString.Empty, consignment.HVC_PL_NKLastMileCarrierServiceLevel);
					AssertEquals("HVC_OH_LastMileCarrierBookingAgent", ZGuid.Empty, consignment.HVC_OH_LastMileCarrierBookingAgent);
				});
			}
		}

		public void TestDestinationDepotInBookingHeaderForm_WhenLMCDepotDetailsAreChanged_UpdatesWithoutReloadingTheForm()
		{
			var creator = new PortHubSelectionTestDataCreator(Factory);

			var billToParty = creator.GenerateAddress("AD1", "XY1", "XY 1");
			var dispatchDepotAddress = creator.GenerateAddress("AD2", "XY2", "XY 2");
			var depotAddress = creator.GenerateAddress("AD3", "XY3", "XY 3");
			var carrier = creator.GenerateOrganisation("XY4", "XY 4");
			var agent = creator.GenerateOrganisation("ZZ5", "ZZ 5");

			var portHubSelectionPK = creator.CreatePortAndDepotSelectionWithUndgClass(depotAddress.PK, dispatchDepotAddress.PK, "EXP", "DLV", "ALL", "AAA", "ALL");
			var portHubSelection = Factory.Load<PortHubSelection>(portHubSelectionPK);
			portHubSelection.TY_OH_CarrierBookingAgent = agent.PK;
			var zonePK = creator.AddZone(portHubSelectionPK, "Z2", carrier.PK, "EXP");
			creator.AddZoneItem(zonePK, "Melbourne Metro", "VIC", "AU");

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.HVH_BookingReference = "M00000050";
			bookingHeader.HVH_OA_BillToParty = billToParty.PK;
			bookingHeader.HVH_RS_NKBookingServiceLevel = "EXP";
			bookingHeader.HVH_OA_DispatchAddress = dispatchDepotAddress.PK;

			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_ConsignmentId = "CONSIGN100";
			consignment.HVC_UndgClass = "6";
			consignment.HVC_ConsigneeAddress1 = "Test Address 11";
			consignment.HVC_ConsigneeCity = "Melbourne Metro";
			consignment.HVC_ConsigneeState = "VIC";
			consignment.HVC_ConsigneePostcode = "3560";
			consignment.HVC_RN_NKConsigneeCountryCode = "AU";
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var consignmentInConsignmentForm = factory.Load<HVLVConsignment>(consignment.PK);

			using (var bookingHeaderForm = new HVLVBookingHeaderForm(bookingHeader))
			using (var consignmentForm = new HVLVConsignmentForm(consignmentInConsignmentForm))
			{
				consignmentForm.Show();
				AssertEquals("Precondition: DestinationDepot is empty", Guid.Empty, consignmentInConsignmentForm.HVC_OA_DestinationDepot_ZAddress.AddressFK);

				var menuItem = ((IFileMenuItemsProvider)consignmentForm).ActionsMenuItem.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.CalculateLMCDepotDetails);
				AssertNotNull(menuItem);
				menuItem.PerformClick();

				AssertEquals("DestinationDepot of consignment in the form is calculated", depotAddress.PK, consignmentInConsignmentForm.HVC_OA_DestinationDepot_ZAddress.AddressFK);
				CombineAssertions("DestinationDepot of consignment in the booking header is updated", () =>
				{
					AssertEquals(consignment.HVC_OA_DestinationDepot_ZAddress.AddressFK, consignmentInConsignmentForm.HVC_OA_DestinationDepot_ZAddress.AddressFK);
					AssertEquals(consignment.HVC_OA_DestinationDepot_ZAddress.OrgPK, consignmentInConsignmentForm.HVC_OA_DestinationDepot_ZAddress.OrgPK);
				});
			}
		}

		public void TestCalculateLMCDetails_WillPopulateConsignmentInAnotherFactory()
		{
			var creator = new PortHubSelectionTestDataCreator(Factory);

			var billToParty = creator.GenerateAddress("AD1", "XY1", "XY 1");
			var dispatchDepotAddress = creator.GenerateAddress("AD2", "XY2", "XY 2");
			var depotAddress = creator.GenerateAddress("AD3", "XY3", "XY 3");
			var carrier = creator.GenerateOrganisation("XY4", "XY 4");
			var agent = creator.GenerateOrganisation("ZZ5", "ZZ 5");

			var portHubSelectionPK = creator.CreatePortAndDepotSelectionWithUndgClass(depotAddress.PK, dispatchDepotAddress.PK, "EXP", "DLV", "ALL", "AAA", "ALL");
			var portHubSelection = Factory.Load<PortHubSelection>(portHubSelectionPK);
			portHubSelection.TY_OH_CarrierBookingAgent = agent.PK;
			var zonePK = creator.AddZone(portHubSelectionPK, "Z2", carrier.PK, "EXP");
			creator.AddZoneItem(zonePK, "Melbourne Metro", "VIC", "AU");

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.HVH_BookingReference = "M00000050";
			bookingHeader.HVH_OA_BillToParty = billToParty.PK;
			bookingHeader.HVH_RS_NKBookingServiceLevel = "EXP";
			bookingHeader.HVH_OA_DispatchAddress = dispatchDepotAddress.PK;

			var bookingHeaderConsignment = bookingHeader.Consignments.AddNew();
			bookingHeaderConsignment.HVC_ConsignmentId = "CONSIGN100";
			bookingHeaderConsignment.HVC_UndgClass = "6";
			bookingHeaderConsignment.HVC_ConsigneeAddress1 = "Test Address 11";
			bookingHeaderConsignment.HVC_ConsigneeCity = "Melbourne Metro";
			bookingHeaderConsignment.HVC_ConsigneeState = "VIC";
			bookingHeaderConsignment.HVC_ConsigneePostcode = "3560";
			bookingHeaderConsignment.HVC_RN_NKConsigneeCountryCode = "AU";
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var consignmentFormConsignment = factory2.Load<HVLVConsignment>(bookingHeaderConsignment.PK);

			using (var bookingHeaderForm = new HVLVBookingHeaderForm(bookingHeader))
			using (var consignmentForm = new HVLVConsignmentForm(consignmentFormConsignment))
			{
				consignmentForm.Show();
				AssertLMCDepotDetailsAreEmpty("Precondition: LMC Depot Details should be empty", bookingHeaderConsignment);
				AssertLMCDepotDetailsAreEmpty("Precondition: LMC Depot Details should be empty", consignmentFormConsignment);

				var menuItem = ((IFileMenuItemsProvider)consignmentForm).ActionsMenuItem.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.CalculateLMCDepotDetails);
				AssertNotNull(menuItem);
				menuItem.PerformClick();
				AssertLMCDepotDetailsArePopulated("LMC Depot Details should be populated for consignment from original factories", bookingHeaderConsignment);
				AssertLMCDepotDetailsArePopulated("LMC Depot Details should be populated for second consignment from second factories", consignmentFormConsignment);

				PortHubSelectionTestDataCreator.ClearUpAllZoneItems(Factory);
				menuItem.PerformClick();
				AssertLMCDepotDetailsAreEmpty("LMC Depot Details should be empty", bookingHeaderConsignment);
				AssertLMCDepotDetailsAreEmpty("LMC Depot Details should be empty", consignmentFormConsignment);
			}

			void AssertLMCDepotDetailsAreEmpty(string message, HVLVConsignment c)
			{
				CombineAssertions(message, () =>
				{
					AssertEquals("HVC_OA_DestinationDepot", ZGuid.Empty, c.HVC_OA_DestinationDepot);
					AssertEquals("HVC_OH_LastMileCarrier", ZGuid.Empty, c.HVC_OH_LastMileCarrier);
					AssertEquals("HVC_PL_NKLastMileCarrierServiceLevel", ZString.Empty, c.HVC_PL_NKLastMileCarrierServiceLevel);
					AssertEquals("HVC_OH_LastMileCarrierBookingAgent", ZGuid.Empty, c.HVC_OH_LastMileCarrierBookingAgent);
				});
			}

			void AssertLMCDepotDetailsArePopulated(string message, HVLVConsignment c)
			{
				CombineAssertions(message, () =>
				{
					AssertEquals(depotAddress.PK, c.HVC_OA_DestinationDepot);
					AssertEquals(carrier.PK, c.HVC_OH_LastMileCarrier);
					AssertEquals("EXP", c.HVC_PL_NKLastMileCarrierServiceLevel);
					AssertEquals(agent.PK, c.HVC_OH_LastMileCarrierBookingAgent);
				});
			}
		}

		public void TestConvertToStandAloneDeclaration_MenuItem_NotVisible_NoShipment()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_ConsignmentId = "CONSIGN100";
			Factory.Save();

			using (var form = new HVLVConsignmentForm(consignment))
			{
				form.Show();

				var actionsMenu = ((IFileMenuItemsProvider)form).ActionsMenuItem;
				actionsMenu.ShowPopupMenu();
				var menuItem = actionsMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ConvertToStandAloneDeclarationAction);

				AssertEquals("The 'Convert to Stand Alone Declaration' menu item should not be visible on a consignment that is not attached to a shipment", false, menuItem.Visible);
			}
		}

		public void TestConvertToStandAloneDeclaration_MenuItem_NotVisible_DeclarationAlreadyCreated()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_GoodsDescription = "Some test goods";

			consignment.HVC_RN_NKShipperCountryCode = CountryCodes.UnitedStates;
			consignment.HVC_RN_NKConsigneeCountryCode = CountryCodes.Australia;

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "Test Ref";
			consignment.HVC_JE_ImportDeclaration = declaration.PK;

			using (var form = new HVLVConsignmentForm(consignment))
			{
				form.Show();

				var actionsMenu = ((IFileMenuItemsProvider)form).ActionsMenuItem;
				actionsMenu.ShowPopupMenu();
				var menuItem = actionsMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ConvertToStandAloneDeclarationAction);

				AssertEquals("The 'Convert to Stand Alone Declaration' menu item should not be visible on a consignment that already has a stand alone declaration created", false, menuItem.Visible);
			}
		}

		public void TestClickTransportBookingMenuItem_WhenConsignmentIsSelfBooked_ShowDialog()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_IsSelfBooked = true;
			consignment.Items.AddNew();

			Factory.Save();

			using (var form = new HVLVConsignmentForm(consignment))
			{
				var transportBookingMenuItem = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.Cast<MenuItem>().Single(x => x.Text == "Transport Booking");
				transportBookingMenuItem.OnPopup(EventArgs.Empty);

				var menuItem = transportBookingMenuItem.MenuItems.Cast<ZMenuItem>().FirstOrDefault();
				menuItem.PerformClick();

				var expectMessage = "Consignment has been marked as Self-Booked. Do you wish to proceed with Transport Booking?";
				AssertEquals(expectMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				var expectCaption = "Warning";
				AssertEquals(expectCaption, UnitTestUserNotification.Instance.LastMessage.Caption);
			}
		}

		public void TestClickDeactivateMenuItem_WhenConsignmentHasCustomsStatus_ShowMessage()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_ConsignmentId = "LTTSTORE01";
			consignment.HVC_ImportReleaseStatus = HVLVReleaseStatus.Held;

			Factory.Save();

			using (var form = new HVLVConsignmentForm(consignment))
			{
				var menuItem = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.Cast<MenuItem>().Single(x => x.Text == "Make Inactive");
				AssertNotNull(menuItem);

				menuItem.PerformClick();

				AssertEquals("Cannot deactivate Consignment LTTSTORE01 as it has a Customs release status.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestTransportValueTextBox_IsVisible()
		{
			var airShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			airShipment.JS_TransportMode = TransportModes.Air;
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = airShipment.PK;

			using var form = new HVLVConsignmentForm(consignment);
			form.Show();
			var transportValueTextBox = form.Controls.Find("zTextBoxTransportValue", true).Single() as ZTextBox;
			AssertEquals("Should be visible", true, transportValueTextBox.Visible);
		}

		public void TestInsuranceValueTextBox_IsVisible()
		{
			var airShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			airShipment.JS_TransportMode = TransportModes.Air;
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = airShipment.PK;

			using var form = new HVLVConsignmentForm(consignment);
			form.Show();
			var insuranceValueTextBox = form.Controls.Find("zTextBoxInsuranceValue", true).Single() as ZTextBox;
			AssertEquals("Should be visible", true, insuranceValueTextBox.Visible);
		}

		public void TestAuditTabpage()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			using (var form = new HVLVConsignmentForm(consignment))
			{
				var propertyInfo = form.GetType().GetProperty("ShowAuditTab", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
				var propertyValue = (bool)propertyInfo.GetValue(form);
				AssertEquals(true, propertyValue);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var form = new HVLVConsignmentForm(Factory.NewWithValidTestData<HVLVConsignment>());
			form.Size = form.MinimumSize;
			form.ControllerID = ControllerIDs.HVLVConsignment;
			return form;
		}

		protected override bool AllowHasChangesOnFormOpen => true;

		#endregion
	}

	public class HVLVConsignmentFormForTest : HVLVConsignmentForm
	{
		public HVLVConsignmentFormForTest(HVLVConsignment consignment) : base(consignment)
		{
		}

		public bool AllowNewForTest => base.AllowNew;
	}
}
