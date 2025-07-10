using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.GUI.WhsPicksAwaitingReplenishment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class PickHeaderReleaseUserControlTest : WhsGuiTestCaseWithFactory
	{
		public void TestPackingStationControl()
		{
			using (var form = new ZForm(Factory.New<WhsPick>()))
			{
				var control = new PickHeaderReleaseUserControl();
				form.Controls.Add(control);

				form.Show();
				Assert(GUITestHelper.FindControl<ZGuidFindBox>(control.Controls, "PackingStationGuidFindBox").Visible);
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
				var control = new PickHeaderReleaseUserControl();
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
				var control = new PickHeaderReleaseUserControlForTest();
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

		class PickHeaderReleaseUserControlForTest : PickHeaderReleaseUserControl
		{
			public new ZButton DisplayAwaitingReplenishmentPicksButton => base.DisplayAwaitingReplenishmentPicksButton;
		}
	}
}
