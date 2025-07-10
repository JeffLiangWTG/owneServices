using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	public class CartageLegControlTest : TestCaseWithFactory
	{
		public void TestContextMenus()
		{
			var org1 = Helper.CreateOrgHeader("Org1", "org1Address1");
			var org2 = Helper.CreateOrgHeader("Org2", "org2Address1");
			var org3 = Helper.CreateOrgHeader("Org3", "org3Address1");
			var org4 = Helper.CreateOrgHeader("Org4", "org4Address1");
			var org1add2 = Helper.AddOrgAddress(org1, "org1Address2");
			var org2add2 = Helper.AddOrgAddress(org2, "org2Address2");
			var org3add2 = Helper.AddOrgAddress(org3, "org3Address2");
			var org4add2 = Helper.AddOrgAddress(org4, "org4Address2");
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = org4.PK;
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			AssertEquals("3 Main Addresses + 2 Org Proxy", 5, new CartageBindToLists(Factory).CartageAddressList(cartage).Count);
			cartage.FirstDocAddress.E2_OA_Address = org1.MainAddress.PK;
			cartage.SecondDocAddress.E2_OA_Address = org2.MainAddress.PK;
			cartage.ThirdDocAddress.E2_OA_Address = org3.MainAddress.PK;
			AssertEquals(8, new CartageBindToLists(Factory).CartageAddressList(cartage).Count);
			var move = cartage.GetBookedMoves(cartage.Containers.First())[0];
			var leg = move.CartageLegs[0];
			using (var form = new FormWithINotifications())
			using (var control = new CartageLegControl())
			{
				leg.HasWaitPoint = true;
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(leg, "");
				TestContext(control.PickupAddressSelectionDropDown, leg.JU_E2PickupAddressIDInfo, control.PickupAddressHelperForTest);
				TestContext(control.WaitPointAddressSelectionDropDown, leg.JU_E2WaitPointAddressIDInfo, control.WaitPointAddressHelperForTest);
				TestContext(control.DeliveryAddressSelectionDropDown, leg.JU_E2DeliveryAddressIDInfo, control.DeliveryAddressHelperForTest);
			}
		}

		void TestContext(ZDropButtonOnly button, ZPropertyInfo info, QuickAddressHelper helper)
		{
			info.Value = ZGuid.Empty;
			button.PerformClick();
			var contextMenu = helper.ContextForTest;
			AssertNotNull(contextMenu);
			AssertEquals(8, contextMenu.MenuItems.Count);
			var ctoMenu = helper.ContextForTest.MenuItems.FindByText("Container Terminal Operator");
			var cneMenu = helper.ContextForTest.MenuItems.FindByText("Consignee");
			var cfsMenu = helper.ContextForTest.MenuItems.FindByText("Container Freight Station");
			var cydMenu = helper.ContextForTest.MenuItems.FindByText("Container Yard");
			var openMenu = helper.ContextForTest.MenuItems.FindByText("Open Organization");
			var clearMenu = helper.ContextForTest.MenuItems.FindByText("Clear");
			var newMenu = helper.ContextForTest.MenuItems.FindByText("New Address...");
			AssertEquals(2, ctoMenu.MenuItems.Count);
			AssertEquals(2, cneMenu.MenuItems.Count);
			AssertEquals(2, cfsMenu.MenuItems.Count);
			AssertEquals(2, cydMenu.MenuItems.Count);
			AssertEquals(0, openMenu.MenuItems.Count);
			AssertEquals(0, clearMenu.MenuItems.Count);
			AssertEquals(0, newMenu.MenuItems.Count);
			var cne1Menu = (AddressSelectionMenu)cneMenu.MenuItems[1];
			var pk = cne1Menu.Element.DocOrOrgAddressPK;
			cne1Menu.PerformClick();
			AssertEquals(pk, info.Value);
			button.PerformClick();
			clearMenu = helper.ContextForTest.MenuItems.FindByText("Clear");
			clearMenu.PerformClick();
			AssertEquals(ZGuid.Empty, info.Value);
		}

		public void TestGroupBoxCaptionsAreBlank()
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			var move = cartage.GetBookedMoves(cartage.Containers.First())[0];
			var leg = move.CartageLegs[0];
			using (var form = new FormWithINotifications())
			using (var control = new CartageLegControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(leg, "");
				AssertNull("Pickup Groupbox Caption Needs to be blank", control.CartageLegPickupGroupBox.CaptionResourceString.Caption);
				AssertNull("Delivery Groupbox Caption Needs to be blank", control.CartageLegDeliveryGroupBox.CaptionResourceString.Caption);
				AssertNull("Wait Point Groupbox Caption Needs to be blank", control.CartageLegWaitPointGroupBox.CaptionResourceString.Caption);
			}
		}

		public class FormWithINotifications : ZForm, INotifications
		{
			void INotifications.Add(INotification notification)
			{
				throw new NotImplementedException();
			}
		}

		LocalCartageTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new LocalCartageTestHelper(Factory));
			}
		}

		LocalCartageTestHelper helper;
	}
}
