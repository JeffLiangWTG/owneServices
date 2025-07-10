using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.eTail.Business;
using Enterprise.eTail.Module;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.GUI.Testing
{
	public abstract class SendACASMessageMenuItemTest<T> : TestCaseWithFactory where T : SendACASMessageMenuItem
	{
		protected string ACASMessageStatus
		{
			get
			{
				if (acasMessageStatus == null)
				{
					var propertyInfo = typeof(T).GetProperty("ACASMessageStatus", BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Instance);
					var menuItem = Activator.CreateInstance(typeof(T), Factory.NewWithValidTestData<ForwardingShipment>());

					acasMessageStatus = propertyInfo.GetValue(menuItem).ToString();
				}

				return acasMessageStatus;
			}
		}

		string acasMessageStatus;

		protected void AssertMenuItemVisibilityGivenMessageStatus(string errorMessage, string acasMessageStatus, bool expectedVisibility)
		{
			var shipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory, true);
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKDestination = "USLAX";

			using (var plugin = new ETailShipmentPlugin(shipment))
			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				var consignment = plugin.ConsignmentHeader.Consignments.AddNew();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_ACASMessageStatus = acasMessageStatus;

				var item = Factory.NewWithValidTestData<HVLVItem>();
				item.HVI_HVC_Consignment = consignment.PK;
				item.HVI_JS_LoadedOnShipment = shipment.PK;

				Factory.Save();

				form.Show();
				var hVLVMenu = plugin.TopLevelMenu;
				hVLVMenu.PerformSelect();

				var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				customsMenu.OnPopup(EventArgs.Empty);
				var sendACASMenuGroup = customsMenu.MenuItems.OfType<SendACASMenuGroup>().SingleOrDefault();

				var sendACASMessageMenuItem = sendACASMenuGroup.MenuItems.OfType<T>().Single();
				AssertEquals(errorMessage, expectedVisibility, sendACASMessageMenuItem.Visible);
			}
		}

		public void TestSendACASMessage_ConsignmentFilter_OnlyShowsActiveConsignments()
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

				var consignmentInActive = consignmentHeader.Consignments.AddNew();
				consignmentInActive.HVC_ACASMessageStatus = ACASMessageStatus;
				consignmentInActive.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignmentInActive.HVC_GoodsDescription = "AAAA";
				consignmentInActive.HVC_WaybillNumber = "Waybill";

				var item = consignmentInActive.Items.AddNew();
				item.HVI_JS_LoadedOnShipment = shipment.PK;

				var consignmentActive = consignmentHeader.Consignments.AddNew();
				consignmentActive.HVC_ACASMessageStatus = ACASMessageStatus;
				consignmentActive.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignmentActive.HVC_GoodsDescription = "BBBB";
				consignmentActive.HVC_WaybillNumber = "Waybill2";
				consignmentActive.HVC_IsActive = true;

				var item2 = consignmentActive.Items.AddNew();
				item2.HVI_JS_LoadedOnShipment = shipment.PK;

				Factory.Save();

				var factory2 = new BusinessObjectFactory();
				var reloadedConsignment = factory2.Load<HVLVConsignment>(consignmentInActive.PK);
				reloadedConsignment.HVC_IsActive = false;
				factory2.Save();

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment) as ETailShipmentPlugin;
					var hVLVMenu = plugin.TopLevelMenu;
					hVLVMenu.PerformSelect();

					var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var sendACASGroupMenu = customsMenu.MenuItems.OfType<SendACASMenuGroup>().Single();
					var sendACASMessageMenuItem = sendACASGroupMenu.MenuItems.OfType<T>().Single();

					sendACASMessageMenuItem.PerformClick();
					Application.DoEvents();
					AssertEquals("Precondition: Should be HVLVConsignmentACASValidationForm", typeof(HVLVConsignmentACASValidationForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

					var consignmentWrappers = ZFormModaliser.LastIBusinessShownOnDialogForTest as HVLVConsignmentForACASWrapperCollection;
					AssertEquals("Send ACAS Messages should only show active consignments", 1, consignmentWrappers.Count);
					AssertEquals("Should only contain the active consignment", consignmentActive, (consignmentWrappers.Single() as HVLVConsignmentForACASWrapper).Consignment);
				}
			}
		}
	}
}
