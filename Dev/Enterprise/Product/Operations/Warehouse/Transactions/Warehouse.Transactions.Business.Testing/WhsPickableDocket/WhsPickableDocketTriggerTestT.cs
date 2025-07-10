using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsPickableDocketTriggerTest<T> : WhsDocketTriggerTest<T> where T : WhsPickableDocket
	{
		#region TestTG_WhsDocket_PreventDetachedOrderPickLines

		public void TestTG_WhsDocket_PreventDetachedOrderPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var pickableDocket = GetNewDocket(data.Org1, data.Whs1);
			var pickableDocketLine = GetNewDocketLine(pickableDocket, data.Part1, null);
			var pick = Helper.CreatePickNew(pickableDocket);
			Factory.Save();
			AssertEquals("Precondition", 10m, pickableDocketLine.PickLines.Sum(pl => pl.WZ_Units));

			// Detach pick to trigger
			pickableDocket.WD_WP = ZGuid.Empty;
			pickableDocket.WD_DocketStatus = "ENT";
			var exception = AssertExceptionThrown<ZSaveException>("Trigger should have prevented save.", Factory.Save);
			AssertEquals(WhsExceptionHandler.WhsDocket_PreventDetachedOrderPickLinesTriggerMsgForUser, exception.InnerException.InnerException.Message);
		}

		#endregion

		#region TestTG_WhsDocket_HasCorrectWarehouse

		public void TestTG_WhsDocket_HasCorrectWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("WH2");

			var pickableDocket = GetNewDocket(data.Org1, data.Whs1);
			var pickableDocketLine = GetNewDocketLine(pickableDocket, data.Part1, null);
			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = whs2.PK;

			Factory.Save();

			// change whs of order to ensure trigger will prevent the change
			pickableDocket.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
			pickableDocket.WD_WP = pick.PK;
			var exception = AssertExceptionThrown<ZSaveException>("Trigger should have prevented save.", Factory.Save);
			AssertEquals(WhsDocket.PreventOrderAndWorkOrderWarehouseNotMatchingPicksWarehouse, exception.InnerException.InnerException.Message);
		}

		#endregion

		#region TestTG_WhsPickLine_PreventTransactionsPickingFromDifferentWarehouse

		[ExpectNoExceptions]
		public void TestTG_WhsPickLine_PreventTransactionsPickingFromDifferentWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("WH2");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, whs2, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = Helper.CreateWhsPickLine(order.Lines.Single(), receive.Inventory[0], 10m);

			AssertNotEquals("Precondition: Warehouse of inventory must be different to Warehouse in order.", receive.Inventory[0].Location.Warehouse, order.Warehouse);
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), WhsPickLine.PreventTransactionsPickingFromDifferentWarehouseTriggerID, true), "Expected trigger to prevent this operation.");
		}

		#endregion

		#region Implementation

		protected override WhsPick CreateNewPick(WhsDocket docket)
		{
			var pickableDocket = docket as WhsPickableDocket;
			return (pickableDocket != null) ? Helper.CreatePickByAttachingOrders(pickableDocket) : null;
		}

		#endregion
	}
}
