using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.GUI.WhsPicksAwaitingReplenishment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class PickHeaderUserControlTest : WhsGuiTestCaseWithFactory
	{
		public void TestCartonisationControlsHidden()
		{
			using (var form = new ZForm(Factory.New<WhsPick>()))
			{
				var control = new PickHeaderUserControl();
				form.Controls.Add(control);

				form.Show();
				AssertEquals("Precondition", true, GUITestHelper.FindControl<ZCheckBox>(control.Controls, "PickCasesByLabelCheckBox").Visible);
				AssertEquals("Precondition", true, GUITestHelper.FindControl<ZCheckBox>(control.Controls, "PickPalletsByLabelCheckBox").Visible);
				AssertEquals("Precondition", true, GUITestHelper.FindControl<ZCheckBox>(control.Controls, "CartoniseCheckBox").Visible);
				AssertEquals("Precondition", true, GUITestHelper.FindControl<ZCheckBox>(control.Controls, "ForcePickByCaseCheckBox").Visible);
				AssertEquals("Precondition", true, GUITestHelper.FindControl<ZCheckBox>(control.Controls, "ForcePickSplitCaseCheckBox").Visible);
			}

			GlbStaff.CurrentUser.GS_LoginName = "test_notsupport";
			using (var form = new ZForm(Factory.New<WhsPick>()))
			{
				var control = new PickHeaderUserControl();
				form.Controls.Add(control);

				form.Show();
				AssertEquals("Should NOT be hidden to non support users", true, GUITestHelper.FindControl<ZCheckBox>(control.Controls, "PickCasesByLabelCheckBox").Visible);
				AssertEquals("Should NOT be hidden to non support users", true, GUITestHelper.FindControl<ZCheckBox>(control.Controls, "PickPalletsByLabelCheckBox").Visible);
				AssertEquals("Should NOT be hidden to non support users", true, GUITestHelper.FindControl<ZCheckBox>(control.Controls, "CartoniseCheckBox").Visible);
				AssertEquals("Should NOT be hidden to non support users", true, GUITestHelper.FindControl<ZCheckBox>(control.Controls, "ForcePickByCaseCheckBox").Visible);
				AssertEquals("Should NOT be hidden to non support users", true, GUITestHelper.FindControl<ZCheckBox>(control.Controls, "ForcePickSplitCaseCheckBox").Visible);
			}
		}

		public void TestLocationControls()
		{
			using (var form = new ZForm(Factory.New<WhsPick>()))
			{
				var control = new PickHeaderUserControl();
				form.Controls.Add(control);
				form.Show();

				var dockDoorControl = (ZGuidFindBox)form.Controls.Find("DockDoorGuidFindBox", true)[0];
				Assert("Dock door find box should have auto complete disabled", dockDoorControl.AutoCompleteDisabled);
				Assert("Dock door find box should not show description", !dockDoorControl.ShowDescriptionBox);

				var warehouseControl = (IFindBoxUserControl)form.Controls.Find("zGuidFindBox1", true)[0];
				Assert("Warehouse find box should not have auto complete disabled",
					!warehouseControl.AutoCompleteDisabled);
			}
		}

		public void TestDisplayAwaitingReplenishmentPicksButton_Visibility_PickAwaitingReplenishment() =>
			TestDisplayAwaitingReplenishmentPicksButton_VisibilityCore(true);

		public void TestDisplayAwaitingReplenishmentPicksButton_Visibility_PickNotAwaitingReplenishment() =>
			TestDisplayAwaitingReplenishmentPicksButton_VisibilityCore(false);

		void TestDisplayAwaitingReplenishmentPicksButton_VisibilityCore(bool isAwaitingReplenishment)
		{
			var pick = Factory.New<WhsPick>();
			pick.WP_IsAwaitingReplenishment = isAwaitingReplenishment;
			using (var form = new ZForm(pick))
			{
				var control = new PickHeaderUserControl();
				form.Controls.Add(control);
				form.Show();

				var displayAwaitingReplenishmentPicksButton = (ZButton)form.Controls.Find("DisplayAwaitingReplenishmentPicksButton", true)[0];
				AssertEquals("DisplayAwaitingReplenishmentPicksButton should be visible", isAwaitingReplenishment, displayAwaitingReplenishmentPicksButton.Visible);
			}
		}

		public void TestDisplayAwaitingReplenishmentPicksButton()
		{
			var data = new PickFaceViewTestData(Factory, clients: 1, locationsPerWarehouse: 2);
			var locations = data.Locations[data.Warehouses[0]];
			var normalLocationType =
				Helper.CreateLocationType("NOR", "NormalLocation", false, 1, LocationClasses.Codes.NOR);
			locations[0].WLV_WLT_LocationType = normalLocationType.PK;

			Helper.CreateProductPickFace(data.Parts[0], data.Clients[0], locations[1], 1m, 2m, 2m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Clients[0].PK, data.Warehouses[0].PK, "R0", Helper.Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Parts[0], 100m, locations[0]);
			receive.FinaliseDocket();
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Clients[0].PK, data.Warehouses[0].PK, "T0", Helper.Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Parts[0], 100m, locations[0].PK, locations[1].PK);
			transferLine.RunPreSaveValidation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Clients[0].PK, data.Warehouses[0].PK, data.Clients[0].PK, "O0");
			Helper.CreateWhsOrderLine(order, data.Parts[0], 20m);
			var pick = Helper.CreatePickNew(false, false, order);
			Factory.Save();

			using (var form = new ZForm(pick))
			{
				var control = new PickHeaderUserControlForTest();
				form.Controls.Add(control);
				form.Show();

				control.DisplayAwaitingReplenishmentPicksButton.PerformClick();
				using (var picksAwaitingReplenishmentForm = (WhsPicksAwaitingReplenishmentForm)ZFormModaliser.LastFormShownForTest)
				{
					AssertNotNull(picksAwaitingReplenishmentForm);
					AssertEquals(true, picksAwaitingReplenishmentForm.Visible);
					AssertEquals(pick.PK, ((BusinessObject)picksAwaitingReplenishmentForm.BusinessEntity).PK);
				}
			}
		}

		public void TestPackingStationControl()
		{
			using (var form = new ZForm(Factory.New<WhsPick>()))
			{
				var control = new PickHeaderUserControl();
				form.Controls.Add(control);

				form.Show();
				Assert(GUITestHelper.FindControl<ZGuidFindBox>(control.Controls, "PackingStationGuidFindBox").Visible);
			}
		}

		#region Implementation

		class PickHeaderUserControlForTest : PickHeaderUserControl
		{
			public new ZButton DisplayAwaitingReplenishmentPicksButton => base.DisplayAwaitingReplenishmentPicksButton;
		}

		#endregion
	}
}
