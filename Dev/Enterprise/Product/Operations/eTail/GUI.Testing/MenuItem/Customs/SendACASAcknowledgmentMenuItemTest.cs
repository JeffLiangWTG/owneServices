using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.eTail.Module;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.GUI.Testing
{
	public class SendACASAcknowledgmentMenuItemTest : SendACASMessageMenuItemTest<SendACASAcknowledgementMenuItem>
	{
		public void TestSendACASAcknowledgementMenuItem_WhenAcknowledgementRequired()
		{
			AssertMenuItemVisibilityGivenMessageStatus("sendACASAcknowledgementMenuItem should be visible", HVLVACASMessageStatusList.Codes.AcknowledgementRequired, true);
		}

		public void TestSendACASAcknowledgementMenuItem_WhenAcknowledgementNotRequired()
		{
			AssertMenuItemVisibilityGivenMessageStatus("sendACASAcknowledgementMenuItem should be hidden", HVLVACASMessageStatusList.Codes.OriginalSent, false);
		}

		public void TestSendACASAcknowledgement_FiltersConsignments()
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
				consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment1.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.OriginalSent;
				consignment1.HVC_GoodsDescription = "AAAA";
				consignment1.HVC_IsActive = true;

				var item = consignment1.Items.AddNew();
				item.HVI_JS_LoadedOnShipment = shipment.PK;

				var consignment2 = consignmentHeader.Consignments.AddNew();
				consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment2.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.AcknowledgementRequired;
				consignment2.HVC_GoodsDescription = "BBBB";
				consignment2.HVC_IsActive = true;

				var item2 = consignment2.Items.AddNew();
				item2.HVI_JS_LoadedOnShipment = shipment.PK;

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
					var sendACASAcknowledgementMenuItem = sendACASMenuGroup.MenuItems.OfType<SendACASAcknowledgementMenuItem>().Single();

					sendACASAcknowledgementMenuItem.PerformClick();
					Application.DoEvents();
					AssertEquals("Precondition: Should be HVLVConsignmentACASValidationForm", typeof(HVLVConsignmentACASValidationForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

					var consignmentWrappers = ZFormModaliser.LastIBusinessShownOnDialogForTest as HVLVConsignmentForACASWrapperCollection;
					var wrapper = consignmentWrappers.Single() as HVLVConsignmentForACASWrapper;
					AssertEquals("There is only one consignment in the view, and it has a goods description of BBBB", new ZString("BBBB"), wrapper.Consignment.HVC_GoodsDescription);
				}
			}
		}
	}
}
