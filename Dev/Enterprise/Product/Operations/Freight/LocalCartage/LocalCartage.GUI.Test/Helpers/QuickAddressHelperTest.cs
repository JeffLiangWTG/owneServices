using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.LocalCartage.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	public class QuickAddressHelperTest : TestCaseWithFactory
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
			cartage.FirstDocAddress.E2_OA_Address = org1.MainAddress.PK;
			cartage.SecondDocAddress.E2_OA_Address = org2.MainAddress.PK;
			cartage.ThirdDocAddress.E2_OA_Address = org3.MainAddress.PK;
			var move = cartage.GetBookedMoves(cartage.Containers.First())[0];
			using (var form = new ZForm())
			using (var button = new ZDropButtonOnly())
			{
				form.Controls.Add(button);
				form.Show();
				var addressHelper = new QuickAddressHelper(button);
				var strategy = new TestAddressHelperStrategy(move.Lookups.GetCartageAddressElements(), cartage, move.EW_E2PickupAddressIDInfo, DocAddressType.LocalCartageCFS, Factory);
				addressHelper.Parent = strategy;
				move.EW_E2PickupAddressID = ZGuid.Empty;
				AssertNull(addressHelper.ContextForTest);
				button.PerformClick();
				var contextMenu = addressHelper.ContextForTest;
				AssertNotNull(contextMenu);
				AssertEquals(8, contextMenu.MenuItems.Count);
				var ctoMenu = addressHelper.ContextForTest.MenuItems.FindByText("Container Terminal Operator");
				var cneMenu = addressHelper.ContextForTest.MenuItems.FindByText("Consignee");
				var cfsMenu = addressHelper.ContextForTest.MenuItems.FindByText("Container Freight Station");
				var cydMenu = addressHelper.ContextForTest.MenuItems.FindByText("Container Yard");
				var openMenu = addressHelper.ContextForTest.MenuItems.FindByText("Open Organization");
				var clearMenu = addressHelper.ContextForTest.MenuItems.FindByText("Clear");
				var newMenu = addressHelper.ContextForTest.MenuItems.FindByText("New Address...");
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
				AssertEquals(pk, move.EW_E2PickupAddressID);
				button.PerformClick();
				clearMenu = addressHelper.ContextForTest.MenuItems.FindByText("Clear");
				clearMenu.PerformClick();
				AssertEquals(ZGuid.Empty, move.EW_E2PickupAddressID);
			}
		}

		public void TestNewAddress()
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
			cartage.FirstDocAddress.E2_OA_Address = org1.MainAddress.PK;
			cartage.SecondDocAddress.E2_OA_Address = org2.MainAddress.PK;
			cartage.ThirdDocAddress.E2_OA_Address = org3.MainAddress.PK;
			var move = cartage.GetBookedMoves(cartage.Containers.First())[0];
			using (var form = new ZForm())
			using (var button = new ZDropButtonOnly())
			{
				form.Controls.Add(button);
				form.Show();
				var addressHelper = new QuickAddressHelper(button);
				var strategy = new TestAddressHelperStrategy(move.Lookups.GetCartageAddressElements(), cartage, move.EW_E2PickupAddressIDInfo, DocAddressType.LocalCartageCFS, Factory);
				addressHelper.Parent = strategy;
				move.EW_E2PickupAddressID = ZGuid.Empty;
				AssertNull(addressHelper.ContextForTest);
				button.PerformClick();
				var contextMenu = addressHelper.ContextForTest;
				AssertNotNull(contextMenu);
				AssertEquals(8, contextMenu.MenuItems.Count);
				var ctoMenu = addressHelper.ContextForTest.MenuItems.FindByText("Container Terminal Operator");
				var cneMenu = addressHelper.ContextForTest.MenuItems.FindByText("Consignee");
				var cfsMenu = addressHelper.ContextForTest.MenuItems.FindByText("Container Freight Station");
				var cydMenu = addressHelper.ContextForTest.MenuItems.FindByText("Container Yard");
				var openMenu = addressHelper.ContextForTest.MenuItems.FindByText("Open Organization");
				var clearMenu = addressHelper.ContextForTest.MenuItems.FindByText("Clear");
				var newMenu = addressHelper.ContextForTest.MenuItems.FindByText("New Address...");
				AssertEquals(2, ctoMenu.MenuItems.Count);
				AssertEquals(2, cneMenu.MenuItems.Count);
				AssertEquals(2, cfsMenu.MenuItems.Count);
				AssertEquals(2, cydMenu.MenuItems.Count);
				AssertEquals(0, openMenu.MenuItems.Count);
				AssertEquals(0, clearMenu.MenuItems.Count);
				AssertEquals(0, newMenu.MenuItems.Count);
				newMenu.PerformClick();
				AssertNotNull(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestOpenOrganisation()
		{
			var org1 = Helper.CreateOrgHeader("Org1", "org1Address1");
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			cartage.FirstDocAddress.E2_OA_Address = org1.MainAddress.PK;
			var move = cartage.GetBookedMoves(cartage.Containers.First())[0];
			using (var form = new ZForm())
			using (var button = new ZDropButtonOnly())
			{
				form.Controls.Add(button);
				form.Show();
				var addressHelper = new QuickAddressHelper(button);
				var strategy = new TestAddressHelperStrategy(move.Lookups.GetCartageAddressElements(), cartage, move.EW_E2PickupAddressIDInfo, DocAddressType.LocalCartageCFS, Factory);
				addressHelper.Parent = strategy;
				move.EW_E2PickupAddressID = cartage.FirstDocAddress.PK;
				Factory.Save();
				button.PerformClick();
				var contextMenu = addressHelper.ContextForTest;
				var openMenu = addressHelper.ContextForTest.MenuItems.FindByText("Open Organization");
				openMenu.PerformClick();
				AssertNotNull(addressHelper.ControllerForTest.LastShownForm);
				addressHelper.ControllerForTest.LastShownForm.Dispose();
				var overriddenDocAddress = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCFS);
				overriddenDocAddress.E2_AddressOverride = true;
				move.EW_E2PickupAddressID = overriddenDocAddress.PK;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				button.PerformClick();
				contextMenu = addressHelper.ContextForTest;
				openMenu = addressHelper.ContextForTest.MenuItems.FindByText("Open Organization");
				openMenu.PerformClick();
				AssertEquals("This address is either blank or an overridden free text address. No Organization record exists to open.", UnitTestUserNotification.Instance.LastMessage.Text);
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
