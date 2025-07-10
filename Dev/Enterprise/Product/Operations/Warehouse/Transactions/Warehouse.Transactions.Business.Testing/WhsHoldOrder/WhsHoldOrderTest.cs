using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsHoldOrder))]
	public class WhsHoldOrderTest : WhsNonPersistentBusinessObjectTestCase
	{
		#region Related Entities

		#region TestClient

		public void TestClient()
		{
			var holdOrder = (WhsHoldOrder)GetNewBusinessObject();
			AssertNull("Precondition", holdOrder.Client);
			AssertEquals("Precondition", ZGuid.Empty, holdOrder.ClientPK);

			var data = new TestDataSimpleEnvironment(Factory);
			holdOrder.ClientPK = data.Org1.PK;
			AssertEquals(data.Org1, holdOrder.Client);
		}

		#endregion

		#region TestLines

		public void TestLines()
		{
			var holdOrder = (WhsHoldOrder)GetNewBusinessObject();
			AssertEquals("Collection should be of type 'WhsHoldOrderLineCollection'", typeof(WhsHoldOrderLineCollection), holdOrder.Lines.GetType());
			AssertEquals("Collection should be registered child editable", true, holdOrder.IsRegisteredEditableChildObject(holdOrder.Lines));
		}

		#endregion

		#region TestLogs

		public void TestLogs()
		{
			var holdOrder = (WhsHoldOrder)GetNewBusinessObject();
			var log = holdOrder.GetLogs().AddNew(Events.DataImport);
			Factory.Save();
			AssertEquals("Logs should be non persistent", false, log.IsInDatabase);
		}

		#endregion

		#region TestWarehouse

		public void TestWarehouse()
		{
			var holdOrder = (WhsHoldOrder)GetNewBusinessObject();
			AssertNull("Precondition", holdOrder.Warehouse);
			AssertEquals("Precondition", ZGuid.Empty, holdOrder.WarehousePK);

			var data = new TestDataSimpleEnvironment(Factory);
			holdOrder.WarehousePK = data.Whs1.PK;
			AssertEquals(data.Whs1, holdOrder.Warehouse);
		}

		#endregion

		#endregion

		#region TestFinalise

		// NOTE: Behaviour specific to properties on WhsHoldOrderLine are tested in WhsHoldOrderLineTest

		#region TestFinalise

		public void TestFinalise()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var finalisedReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var holdOrder = Helper.CreateWhsHoldOrder(data.Whs1, data.Org1);
			Helper.CreateWhsHoldOrderLine(holdOrder, data.Part1, fromHoldCode: "", toHoldCode: InventoryHoldCodes.Codes.Damaged, quantity: 50m);

			var inventoryLine = finalisedReceive.Lines[0];
			AssertEquals("Precondition", InventoryStatus.Codes.Available, inventoryLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", "", inventoryLine.WE_WHC_NKCurrentInventoryHeldCode);

			var result = holdOrder.Finalise();
			AssertEquals("Should have succesfully finalised hold order", true, result);
			AssertEquals("Should have changed inventory to be held", InventoryStatus.Codes.Held, inventoryLine.WE_CurrentInventoryStatus);
			AssertEquals("Should have added damaged hold code to the inventory", InventoryHoldCodes.Codes.Damaged, inventoryLine.WE_WHC_NKCurrentInventoryHeldCode);
		}

		#endregion

		#region TestFinalise_OneFailedLine

		public void TestFinalise_OneFailedLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var finalisedReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var holdOrder = Helper.CreateWhsHoldOrder(data.Whs1, data.Org1);
			var line1 = Helper.CreateWhsHoldOrderLine(holdOrder, data.Part1, fromHoldCode: "", toHoldCode: InventoryHoldCodes.Codes.Damaged, quantity: 40m);
			var line2 = Helper.CreateWhsHoldOrderLine(holdOrder, data.Part2, fromHoldCode: "", toHoldCode: InventoryHoldCodes.Codes.Damaged, quantity: 20m);
			var line3 = Helper.CreateWhsHoldOrderLine(holdOrder, data.Part1, fromHoldCode: "", toHoldCode: InventoryHoldCodes.Codes.Damaged, quantity: 10m);

			var inventoryLine = finalisedReceive.Lines[0];
			AssertEquals("Precondition", InventoryStatus.Codes.Available, inventoryLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", "", inventoryLine.WE_WHC_NKCurrentInventoryHeldCode);

			var result1 = holdOrder.Finalise();
			AssertEquals("Should *not* have succesfully finalised hold order", false, result1);
			AssertEquals(InventoryStatus.Codes.Available, inventoryLine.WE_CurrentInventoryStatus);
			AssertEquals("", inventoryLine.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals(0m, inventoryLine.WE_StockOnHand);

			var splitInventory = new ZQuery(WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode, InventoryHoldCodes.Codes.Damaged) { FetchOnlyFromLocalCache = true };
			splitInventory.AddToFilter(WhsDocketLineSchema.WE_WD, finalisedReceive.PK);
			splitInventory.AddToFilter(WhsDocketLineSchema.WE_IsOriginalInventory, false);
			splitInventory.AddToFilter(WhsDocketLineSchema.WE_WE_ParentDocketLine, inventoryLine.PK);
			var result = Factory.Load<WhsDocketLine>(splitInventory);
			AssertEquals("Should have created two new records", 2, result.Length);
			AssertEquals("50 units should have been split", 50m, result.Sum(l => l.WE_StockOnHand));

			AssertEquals(0, line1.RowErrors.Count());
			AssertEquals(1, line2.RowErrors.Count());
			AssertEquals("Should not short circuit after 1 failed change", 0, line3.RowErrors.Count());
		}

		#endregion

		#region TestFinalise_OneFailedLine_SameInventory

		public void TestFinalise_OneFailedLine_SameInventory_WithPartialSplit()
		{
			TestFinalise_OneFailedLine_SameInventory(partialSplitFirstLine: true);
		}

		public void TestFinalise_OneFailedLine_SameInventory()
		{
			TestFinalise_OneFailedLine_SameInventory(partialSplitFirstLine: false);
		}

		void TestFinalise_OneFailedLine_SameInventory(bool partialSplitFirstLine)
		{
			// Hold Code Changes wont be committed until the end of the Universal Import, must check in memory too
			var data = new TestDataSimpleEnvironment(Factory);
			var finalisedReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var holdOrder = Helper.CreateWhsHoldOrder(data.Whs1, data.Org1);
			var line1 = Helper.CreateWhsHoldOrderLine(holdOrder, data.Part1, fromHoldCode: "", toHoldCode: InventoryHoldCodes.Codes.Damaged, quantity: partialSplitFirstLine ? 45m : 50m);
			var line2 = Helper.CreateWhsHoldOrderLine(holdOrder, data.Part1, fromHoldCode: "", toHoldCode: InventoryHoldCodes.Codes.Damaged, quantity: 10m);

			var inventoryLine = finalisedReceive.Lines[0];
			AssertEquals("Precondition", InventoryStatus.Codes.Available, inventoryLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", "", inventoryLine.WE_WHC_NKCurrentInventoryHeldCode);

			var result1 = holdOrder.Finalise();
			AssertEquals("Should *not* have succesfully finalised hold order", false, result1);

			var lineToTest = partialSplitFirstLine ? Factory.LoadTop1<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WE_ParentDocketLine, inventoryLine.PK)) : inventoryLine;
			AssertEquals("Should have changed inventory to be held", InventoryStatus.Codes.Held, lineToTest.WE_CurrentInventoryStatus);
			AssertEquals("Should have added damaged hold code to the inventory", InventoryHoldCodes.Codes.Damaged, lineToTest.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals(partialSplitFirstLine ? 45m : 50m, lineToTest.WE_StockOnHand);

			AssertEquals(0, line1.RowErrors.Count());
			AssertEquals(1, line2.RowErrors.Count());
		}

		#endregion

		#endregion

		#region TestIJobNumber

		public void TestIJobNumber()
		{
			var holdOrder = GetNewBusinessObject();
			AssertEquals("NON PERSISTENT HOLD ORDER", ((IJobNumber)holdOrder).JobNumber);
		}

		#endregion

	}
}
