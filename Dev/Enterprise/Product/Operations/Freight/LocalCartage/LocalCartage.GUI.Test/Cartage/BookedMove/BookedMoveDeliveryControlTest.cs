using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	public class BookedMoveDeliveryControlTest : TestCaseWithFactory
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
			using (var form = new ZForm())
			using (var control = new BookedMoveDeliveryControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(move, "");
				move.EW_E2WaitPointAddressID = ZGuid.Empty;
				AssertNull(control.AddressHelper.ContextForTest);
				control.AddressSelectionDropDown.PerformClick();
				var contextMenu = control.AddressHelper.ContextForTest;
				AssertNotNull(contextMenu);
				AssertEquals(8, contextMenu.MenuItems.Count);
				var ctoMenu = control.AddressHelper.ContextForTest.MenuItems.FindByText("Container Terminal Operator");
				var cneMenu = control.AddressHelper.ContextForTest.MenuItems.FindByText("Consignee");
				var cfsMenu = control.AddressHelper.ContextForTest.MenuItems.FindByText("Container Freight Station");
				var openMenu = control.AddressHelper.ContextForTest.MenuItems.FindByText("Open Organization");
				var clearMenu = control.AddressHelper.ContextForTest.MenuItems.FindByText("Clear");
				var newMenu = control.AddressHelper.ContextForTest.MenuItems.FindByText("New Address...");
				AssertEquals(2, ctoMenu.MenuItems.Count);
				AssertEquals(2, cneMenu.MenuItems.Count);
				AssertEquals(2, cfsMenu.MenuItems.Count);
				AssertEquals(0, openMenu.MenuItems.Count);
				AssertEquals(0, clearMenu.MenuItems.Count);
				AssertEquals(0, newMenu.MenuItems.Count);
				var cne1Menu = (AddressSelectionMenu)cneMenu.MenuItems.FindByText("Org2 :: org2Address1 :: ORG2ADDRESS1 SYDNEY");
				var pk = cne1Menu.Element.DocOrOrgAddressPK;
				cne1Menu.PerformClick();
				AssertEquals(pk, move.EW_E2WaitPointAddressID);
				control.AddressSelectionDropDown.PerformClick();
				clearMenu = control.AddressHelper.ContextForTest.MenuItems.FindByText("Clear");
				clearMenu.PerformClick();
				AssertEquals(ZGuid.Empty, move.EW_E2WaitPointAddressID);
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
