using System;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class PickLinePairTest : WhsTestCaseWithFactory
	{
		#region TestNew_InvalidArguments

		public void TestNew_InvalidArguments()
		{
			AssertExceptionThrown<ArgumentNullException>(() => PickLinePair.New(null));
		}

		#endregion

		#region TestNew

		public void TestNew()
		{
			// the detailed Testing of New() is in TestPickLineOnOrder & TestPickLineForPickingDetails
			var pickLine = Factory.New<WhsPickLine>();
			var pair = PickLinePair.New(pickLine);
			AssertEquals(pickLine, pair.PickLineForPickingDetails);
			AssertEquals(pickLine, pair.PickLineOnOrder);
		}

		#endregion

		#region TestPickLineOnOrder

		public void TestPickLineOnOrder()
		{
			var originalInventoryPK = ZGuid.NewZGuid();
			var transferLine = Factory.New<WhsTransferLine>();
			var pickLine1 = Factory.New<WhsPickLine>();
			var pickLine2 = Factory.New<WhsPickLine>();
			pickLine1.WZ_WE_InventoryLine = transferLine.PK;
			pickLine1.WZ_WE_OriginalPickedInventoryLine = originalInventoryPK;
			pickLine2.WZ_WE_TransactionLine = transferLine.PK;
			pickLine2.WZ_WE_InventoryLine = originalInventoryPK;
			AssertEquals(pickLine1, PickLinePair.New(pickLine1).PickLineOnOrder);
			AssertEquals(pickLine2, PickLinePair.New(pickLine2).PickLineOnOrder);
		}

		#endregion

		#region TestPickLineForPickingDetails

		public void TestPickLineForPickingDetails()
		{
			var originalInventoryPK = ZGuid.NewZGuid();
			var transferLine = Factory.New<WhsTransferLine>();
			var pickLine1 = Factory.New<WhsPickLine>();
			var pickLine2 = Factory.New<WhsPickLine>();
			pickLine1.WZ_WE_InventoryLine = transferLine.PK;
			pickLine1.WZ_WE_OriginalPickedInventoryLine = originalInventoryPK;
			pickLine2.WZ_WE_TransactionLine = transferLine.PK;
			pickLine2.WZ_WE_InventoryLine = originalInventoryPK;
			AssertEquals(pickLine2, PickLinePair.New(pickLine1).PickLineForPickingDetails);
			AssertEquals(pickLine2, PickLinePair.New(pickLine2).PickLineForPickingDetails);
		}

		#endregion

		#region AssignedTo

		public void TestAssignedTo()
		{
			var pickLine = Factory.New<WhsPickLine>();

			var pair = PickLinePair.New(pickLine);
			AssertNull(pair.AssignedTo);

			pickLine.WZ_GS_NKAssignedTo = "E";
			AssertEquals("E", pair.AssignedTo.GS_Code);
		}

		public void TestAssignedTo_TwoPickLines()
		{
			var originalInventoryPK = ZGuid.NewZGuid();
			var transferLine = Factory.New<WhsTransferLine>();
			var pickLine1 = Factory.New<WhsPickLine>();
			var pickLine2 = Factory.New<WhsPickLine>();
			pickLine1.WZ_WE_InventoryLine = transferLine.PK;
			pickLine1.WZ_WE_OriginalPickedInventoryLine = originalInventoryPK;
			pickLine2.WZ_WE_TransactionLine = transferLine.PK;
			pickLine2.WZ_WE_InventoryLine = originalInventoryPK;

			var pair = PickLinePair.New(pickLine1);
			AssertNull(pair.AssignedTo);

			pickLine1.WZ_GS_NKAssignedTo = "E";
			AssertNull(pair.AssignedTo);

			pickLine2.WZ_GS_NKAssignedTo = "E";
			AssertEquals("E", pair.AssignedTo.GS_Code);
		}

		#endregion

		#region AssignedToCode

		public void TestAssignedToCode()
		{
			var pickLine = Factory.New<WhsPickLine>();

			var pair = PickLinePair.New(pickLine);
			AssertEquals("", pair.AssignedToCode);

			pickLine.WZ_GS_NKAssignedTo = "E";
			AssertEquals("E", pair.AssignedToCode);

			pair.AssignedToCode = "";
			AssertEquals("", pickLine.WZ_GS_NKAssignedTo);
		}

		public void TestAssignedToCode_TwoPickLines()
		{
			var originalInventoryPK = ZGuid.NewZGuid();
			var transferLine = Factory.New<WhsTransferLine>();
			var pickLine1 = Factory.New<WhsPickLine>();
			var pickLine2 = Factory.New<WhsPickLine>();
			pickLine1.WZ_WE_InventoryLine = transferLine.PK;
			pickLine1.WZ_WE_OriginalPickedInventoryLine = originalInventoryPK;
			pickLine2.WZ_WE_TransactionLine = transferLine.PK;
			pickLine2.WZ_WE_InventoryLine = originalInventoryPK;

			var pair = PickLinePair.New(pickLine1);
			AssertEquals("", pair.AssignedToCode);

			pickLine1.WZ_GS_NKAssignedTo = "E";
			AssertEquals("", pair.AssignedToCode);

			pickLine2.WZ_GS_NKAssignedTo = "E";
			AssertEquals("E", pair.AssignedToCode);

			pair.AssignedToCode = "";
			AssertEquals("", pickLine2.WZ_GS_NKAssignedTo);
		}

		#endregion

		#region PickedDateTime

		public void TestPickedDateTime()
		{
			var pickLine = Factory.New<WhsPickLine>();

			var pair = PickLinePair.New(pickLine);
			AssertEquals(ZDateTimeOffset.Empty, pair.PickedDateTime);

			var now = ZDateTimeOffset.TruncateMilliseconds(ZDateTimeOffset.Now);
			pickLine.WZ_PickedDateTime = now;
			AssertEquals(now, pair.PickedDateTime);

			pair.PickedDateTime = ZDateTimeOffset.Empty;
			AssertEquals(ZDateTimeOffset.Empty, pickLine.WZ_PickedDateTime);
		}

		public void TestPickedDateTime_TwoPickLines()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 15m);
			var pick = helper.CreatePickNew(order);

			var pickLine1 = orderLine.PickLines.Single();
			Factory.Save();

			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			AssertEquals("Precondition", true, pickLine1.WZ_WE_OriginalPickedInventoryLine.IsValid);
			var pickLine2 = transferLine.PickLines[0];
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Empty;

			var pair = PickLinePair.New(pickLine1);
			AssertEquals(ZDateTimeOffset.Empty, pair.PickedDateTime);

			var now = ZDateTimeOffset.TruncateMilliseconds(ZDateTimeOffset.Now);
			pickLine1.WZ_PickedDateTime = now;
			AssertEquals(ZDateTimeOffset.Empty, pair.PickedDateTime);

			pickLine2.WZ_PickedDateTime = now;
			AssertEquals(now, pair.PickedDateTime);

			pair.PickedDateTime = ZDateTimeOffset.Empty;
			AssertEquals(ZDateTimeOffset.Empty, pickLine2.WZ_PickedDateTime);
		}

		#endregion
	}
}
