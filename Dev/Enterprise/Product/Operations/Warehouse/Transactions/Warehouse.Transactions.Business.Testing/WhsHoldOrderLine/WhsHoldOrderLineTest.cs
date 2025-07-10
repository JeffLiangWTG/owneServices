using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsHoldOrderLine))]
	public class WhsHoldOrderLineTest : WhsNonPersistentBusinessObjectTestCase
	{
		#region TestFinalise

		#region TestFinalise

		public void TestFinalise()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var finalisedReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var holdOrder = Helper.CreateWhsHoldOrder(data.Whs1, data.Org1);
			var line = Helper.CreateWhsHoldOrderLine(holdOrder, data.Part1, fromHoldCode: "", toHoldCode: InventoryHoldCodes.Codes.Damaged, quantity: 50m);
			line.HoldReason = "Whatever";

			var inventoryLine = finalisedReceive.Lines[0];
			AssertEquals("Precondition", InventoryStatus.Codes.Available, inventoryLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", "", inventoryLine.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Precondition", "", inventoryLine.WE_CurrentHoldReason);

			var result = line.Finalise(data.Whs1, data.Org1);
			AssertEquals("Should have succesfully finalised hold order", true, result);
			AssertEquals("Should have changed inventory to be held", InventoryStatus.Codes.Held, inventoryLine.WE_CurrentInventoryStatus);
			AssertEquals("Should have added damaged hold code to the inventory", InventoryHoldCodes.Codes.Damaged, inventoryLine.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Should have added hold reason to the inventory", "Whatever", inventoryLine.WE_CurrentHoldReason);
			AssertEquals("Should *not* be in error", 0, line.RowErrors.Count());
		}

		#endregion

		#region TestFinalise_CommittedAndPickedUnits

		public void TestFinalise_CommittedAndPickedUnits()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			var finalisedReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var holdOrder = Helper.CreateWhsHoldOrder(data.Whs1, data.Org1);
			var line = Helper.CreateWhsHoldOrderLine(holdOrder, data.Part1, fromHoldCode: "", toHoldCode: InventoryHoldCodes.Codes.Held, quantity: 1m);

			var inventoryLine = finalisedReceive.Lines[0];
			AssertEquals("Precondition", InventoryStatus.Codes.Available, inventoryLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", "", inventoryLine.WE_WHC_NKCurrentInventoryHeldCode);

			// Create order and pick 10 units
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick1 = Helper.CreatePickByAttachingOrders(order1);
			pick1.FinaliseAllOrders();
			pick1.FinalisePick();
			AssertEquals(true, pick1.AllOrdersAreFinalised);
			AssertEquals(true, pick1.IsFinalised);
			Factory.Save();

			// Create order and commit 10 units to pick *DONT FINALISE*
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			var pick2 = Helper.CreatePickByAttachingOrders(order2);
			AssertEquals(false, pick2.AllOrdersAreFinalised);
			AssertEquals(false, pick2.IsFinalised);
			Factory.Save();

			// Create transfer and commit 10 units to transfer *DONT FINALISE* 
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, data.Whs1.DefaultLocation, data.Whs1.FindLocation("A-1-2"));
			transfer.RunPreSaveValidation(); // commit
			Factory.Save();

			// Create adjustment and commit -10 units to adjustment *DONT FINALISE* 
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10m, data.Whs1.DefaultLocation);
			adjustment.RunPreSaveValidation(); // commit
			Factory.Save();

			// Change hold code of remaining 10 units to 'DAM'
			var receiveLine = finalisedReceive.Lines[0];
			receiveLine.HeldCodeChangeQuantity = 10m;
			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			AssertEquals("Precondition: 30 total units", 50m - 2 * 10, receiveLine.WE_StockOnHand);
			AssertEquals("Precondition: 30 units committed", 3 * 10m, receiveLine.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Precondition: 0 units available to transfer", 50m - 5 * 10, receiveLine.Inventory[0].WI_AvailableToTransferQuantity);

			var result = line.Finalise(data.Whs1, data.Org1);
			AssertEquals("Should *not* have succesfully finalised hold order", false, result);
			AssertEquals("Should be in error", 1, line.RowErrors.Count());
			AssertEquals("Should not have changed any stock to be 'HEL'", 0, Factory.Load<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode, InventoryHoldCodes.Codes.Held)).Length);
		}

		#endregion

		#region TestFinalise_FailedLine_NoPartAttributesShownInLog

		public void TestFinalise_FailedLine_NoPartAttributesShownInLog()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var holdOrder = Helper.CreateWhsHoldOrder(data.Whs1, data.Org1);
			var line = Helper.CreateWhsHoldOrderLine(holdOrder, data.Part1, fromHoldCode: "", toHoldCode: InventoryHoldCodes.Codes.Damaged, quantity: 50m);

			var result = line.Finalise(data.Whs1, data.Org1);
			AssertEquals("Should have an error", false, result);
			var rowError = line.RowErrors.Single();
			var expectedErrorMessage = "Could not match inventory for\r\nProduct:P1\r\nQuantity:50\r\nCurrent Hold Code:";
			AssertEquals(expectedErrorMessage, rowError.Message);
		}

		#endregion

		#region TestFinalise_FailedLine_WithPartAttributes

		public void TestFinalise_FailedLine_WithPartAttributes()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			var holdOrder = Helper.CreateWhsHoldOrder(data.Whs1, data.Org1);
			var line = Helper.CreateWhsHoldOrderLine(holdOrder, data.Part1, fromHoldCode: "", toHoldCode: InventoryHoldCodes.Codes.Damaged, quantity: 50m, pa1: "1", pa2: "2", pa3: "3", expiryDate: today, packingDate: today.AddDays(1), serialNumber: "SEN");

			var result = line.Finalise(data.Whs1, data.Org1);
			AssertEquals("Should have an error", false, result);
			var rowError = line.RowErrors.Single();
			var expectedErrorMessage = "Could not match inventory for\r\nProduct:P1\r\nQuantity:50\r\nCurrent Hold Code:\r\nPart Attribute 1:1\r\nPart Attribute 2:2\r\nPart Attribute 3:3\r\nSerial Number:SEN\r\nPacking Date:{0}\r\nExpiry Date:{1}";
			AssertEquals(string.Format(expectedErrorMessage, today.AddDays(1).ToShortDateString(), today.ToShortDateString()), rowError.Message);
		}

		#endregion

		#region TestFinalise_MatchingOfDocketLines

		#region TestFinalise_MatchingOfDocketLines_Product

		public void TestFinalise_MatchingOfDocketLines_Product()
		{
			TestFinalise_MatchingOfDocketLines_Core(line => line.ProductPK, (line, pk) => line.ProductPK = pk, targetIncorrectValue: Factory.NewWithValidTestData<OrgSupplierPart>().PK);
		}

		#endregion

		#region TestFinalise_MatchingOfDocketLines_PartAttrib1

		public void TestFinalise_MatchingOfDocketLines_PartAttrib1()
		{
			TestFinalise_MatchingOfDocketLines_Core(line => line.PartAttrib1, (line, value) => line.PartAttrib1 = value, (ZString)"OTHER");
		}

		#endregion

		#region TestFinalise_MatchingOfDocketLines_PartAttrib2

		public void TestFinalise_MatchingOfDocketLines_PartAttrib2()
		{
			TestFinalise_MatchingOfDocketLines_Core(line => line.PartAttrib2, (line, value) => line.PartAttrib2 = value, (ZString)"OTHER");
		}

		#endregion

		#region TestFinalise_MatchingOfDocketLines_PartAttrib3

		public void TestFinalise_MatchingOfDocketLines_PartAttrib3()
		{
			TestFinalise_MatchingOfDocketLines_Core(line => line.PartAttrib3, (line, value) => line.PartAttrib3 = value, (ZString)"OTHER");
		}

		#endregion

		#region TestFinalise_MatchingOfDocketLines_SerialNumber

		public void TestFinalise_MatchingOfDocketLines_SerialNumber()
		{
			TestFinalise_MatchingOfDocketLines_Core(line => line.SerialNumber, (line, value) => line.SerialNumber = value, (ZString)"OTHER");
		}

		#endregion

		#region TestFinalise_MatchingOfDocketLines_ExpiryDate

		[TestDate(2024, 12, 6)]
		public void TestFinalise_MatchingOfDocketLines_ExpiryDate()
		{
			TestFinalise_MatchingOfDocketLines_Core(line => line.ExpiryDate, (line, value) => line.ExpiryDate = value, ZDate.BrettsBirthday);
		}

		[TestDate(2024, 12, 6)]
		public void TestFinalise_MatchingOfDocketLines_ExpiryDate_NextDay()
		{
			TestFinalise_MatchingOfDocketLines_Core(line => line.ExpiryDate, (line, value) => line.ExpiryDate = value, ZDate.Today.AddDays(1));
		}

		[TestDate(2024, 12, 6)]
		public void TestFinalise_MatchingOfDocketLines_ExpiryDate_PreviousDay()
		{
			TestFinalise_MatchingOfDocketLines_Core(line => line.ExpiryDate, (line, value) => line.ExpiryDate = value, ZDate.Today.AddDays(-1));
		}

		#endregion

		#region TestFinalise_MatchingOfDocketLines_FromHoldCode

		public void TestFinalise_MatchingOfDocketLines_FromHoldCode()
		{
			TestFinalise_MatchingOfDocketLines_Core(line => line.FromHoldCode, (line, value) => line.FromHoldCode = value, (ZString)"ABC");
		}

		#endregion

		#region TestFinalise_MatchingOfDocketLines_PackingDate
		
		[TestDate(2024, 12, 6)]
		public void TestFinalise_MatchingOfDocketLines_PackingDate()
		{
			TestFinalise_MatchingOfDocketLines_Core(line => line.PackingDate, (line, value) => line.PackingDate = value, ZDate.BrettsBirthday);
		}

		[TestDate(2024, 12, 6)]
		public void TestFinalise_MatchingOfDocketLines_PackingDate_NextDay()
		{
			TestFinalise_MatchingOfDocketLines_Core(line => line.PackingDate, (line, value) => line.PackingDate = value, ZDate.Today);
		}

		[TestDate(2024, 12, 6)]
		public void TestFinalise_MatchingOfDocketLines_PackingDate_PreviousDay()
		{
			TestFinalise_MatchingOfDocketLines_Core(line => line.PackingDate, (line, value) => line.PackingDate = value, ZDate.Today.AddDays(-2));
		}

		#endregion

		#region TestFinalise_MatchingOfDocketLines_WarehouseAndClient

		public void TestFinalise_MatchingOfDocketLines_WarehouseAndClient()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var holdOrder = Helper.CreateWhsHoldOrder(data.Whs1, data.Org1);
			var line = Helper.CreateWhsHoldOrderLine(holdOrder, data.Part1, fromHoldCode: "", toHoldCode: InventoryHoldCodes.Codes.Damaged, quantity: 50m);

			var inventoryLine = receive.Lines[0];
			AssertEquals("Precondition", InventoryStatus.Codes.Available, inventoryLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", "", inventoryLine.WE_WHC_NKCurrentInventoryHeldCode);

			var result = line.Finalise(Factory.NewWithValidTestData<WhsWarehouse>(), data.Org1);
			AssertEquals("Should have an error", false, result);
			var rowError = line.RowErrors.Single();
			AssertContains("Could not match inventory for", rowError.Message);
			line.RemoveRowError(rowError.Message);

			result = line.Finalise(data.Whs1, Factory.NewWithValidTestData<OrgHeader>());
			AssertEquals("Should have an error", false, result);
			rowError = line.RowErrors.Single();
			AssertContains("Could not match inventory for", rowError.Message);

			line.RemoveRowError(rowError.Message);
			result = line.Finalise(data.Whs1, data.Org1);
			AssertEquals("Should have succesfully finalised hold order", true, result);
			AssertEquals("Should have changed inventory to be held", InventoryStatus.Codes.Held, inventoryLine.WE_CurrentInventoryStatus);
			AssertEquals("Should have added damaged hold code to the inventory", InventoryHoldCodes.Codes.Damaged, inventoryLine.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Should *not* be in error", 0, line.RowErrors.Count());
		}

		#endregion

		#region TestFinalise_MatchingOfDocketLines_UnfinalisedReceive

		public void TestFinalise_MatchingOfDocketLines_UnfinalisedReceive()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, finalise: false);
			Factory.Save();

			var holdOrder = Helper.CreateWhsHoldOrder(data.Whs1, data.Org1);
			var line = Helper.CreateWhsHoldOrderLine(holdOrder, data.Part1, fromHoldCode: "", toHoldCode: InventoryHoldCodes.Codes.Damaged, quantity: 50m);

			var inventoryLine = receive.Lines[0];
			AssertEquals("Precondition", InventoryStatus.Codes.Putaway, inventoryLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", "", inventoryLine.WE_WHC_NKCurrentInventoryHeldCode);

			var result = line.Finalise(data.Whs1, data.Org1);
			AssertEquals("Should have an error", false, result);
			var rowError = line.RowErrors.Single();
			AssertContains("Could not match inventory for", rowError.Message);
			line.RemoveRowError(rowError.Message);

			receive.NotificationManager.Push(Helper.Notify);
			receive.FinaliseDocket();
			AssertEquals(true, receive.IsFinalised);
			Factory.Save();

			line.RemoveRowError(rowError.Message);
			result = line.Finalise(data.Whs1, data.Org1);
			AssertEquals("Should have succesfully finalised hold order", true, result);
			AssertEquals("Should have changed inventory to be held", InventoryStatus.Codes.Held, inventoryLine.WE_CurrentInventoryStatus);
			AssertEquals("Should have added damaged hold code to the inventory", InventoryHoldCodes.Codes.Damaged, inventoryLine.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Should *not* be in error", 0, line.RowErrors.Count());
		}

		#endregion

		#region TestFinalise_MatchingOfDocketLines_Core

		void TestFinalise_MatchingOfDocketLines_Core<T>(Func<WhsHoldOrderLine, T> getValue, Action<WhsHoldOrderLine, T> setValue, T targetIncorrectValue)
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, finalise: false);
			receive.NotificationManager.Push(Helper.Notify);
			Helper.SetInventoryAttributes(receive.Inventory[0], eD: today, pD: today.AddDays(-1), pA1: "1", pA2: "2", pA3: "3", bEK: "", serialNum: "SN");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var holdOrder = Helper.CreateWhsHoldOrder(data.Whs1, data.Org1);
			var line = Helper.CreateWhsHoldOrderLine(holdOrder, data.Part1, fromHoldCode: "", toHoldCode: InventoryHoldCodes.Codes.Damaged, quantity: 1m, pa1: "1", pa2: "2", pa3: "3", expiryDate: today, packingDate: today.AddDays(-1), serialNumber: "SN");

			var inventoryLine = receive.Lines[0];
			AssertEquals("Precondition", InventoryStatus.Codes.Available, inventoryLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", "", inventoryLine.WE_WHC_NKCurrentInventoryHeldCode);

			var originalValue = getValue(line);
			setValue(line, targetIncorrectValue);

			var result = line.Finalise(data.Whs1, data.Org1);
			AssertEquals("Should have an error", false, result);
			var rowError = line.RowErrors.Single();
			AssertContains("Could not match inventory for", rowError.Message);
			AssertEquals("Should *not* have changed inventory to be held", InventoryStatus.Codes.Available, inventoryLine.WE_CurrentInventoryStatus);
			AssertEquals("Should *not* have added damaged hold code to the inventory", "", inventoryLine.WE_WHC_NKCurrentInventoryHeldCode);

			line.RemoveRowError(rowError.Message);
			setValue(line, originalValue);

			result = line.Finalise(data.Whs1, data.Org1);
			AssertEquals("Should have succesfully finalised hold order", true, result);
			AssertEquals("Should have changed inventory to be held", InventoryStatus.Codes.Held, inventoryLine.WE_CurrentInventoryStatus);
			AssertEquals("Should have added damaged hold code to the inventory", InventoryHoldCodes.Codes.Damaged, inventoryLine.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Should *not* be in error", 0, line.RowErrors.Count());
		}

		#endregion

		#endregion

		#endregion

		#region TestProduct

		public void TestProduct()
		{
			var holdOrderLine = (WhsHoldOrderLine)GetNewBusinessObject();
			AssertNull("Precondition", holdOrderLine.Product);
			AssertEquals("Precondition", ZGuid.Empty, holdOrderLine.ProductPK);

			var data = new TestDataSimpleEnvironment(Factory);
			holdOrderLine.ProductPK = data.Part1.PK;
			AssertEquals(data.Part1, holdOrderLine.Product);
		}

		#endregion
	}
}
