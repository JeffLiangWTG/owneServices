using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business.AsnMapping;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsReceiveASNHelperTest : WhsTestCaseWithFactory
	{
		#region TestMapInventories_DoNothingIfNoASNLinesExists

		public void TestMapInventories_DoNothingIfNoASNLinesExists()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);

			AssertNotNull(data.Receive11);
			AssertEquals(1, data.Receive11.Inventory.Count);

			WhsReceiveASNHelper.ReconcileASN(data.Receive11);

			AssertNotNull(data.Receive11);
			AssertEquals(1, data.Receive11.Inventory.Count);
		}

		#endregion

		#region TestMapInventories_DoNothingIfInventoryHasPutawayTransfer

		public void TestMapInventories_DoNothingIfInventoryHasPutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation1 = data.Whs1.FindLocation("A-1");
			var nonDockDoorLocation = data.Whs1.FindLocation("A-3");
			dockDoorLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inv = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation1, "A", 20m);
			inv.WI_LineNo = 0;
			Factory.Save();

			var asnLine1 = receive.AsnLines.AddNew();
			asnLine1.WN_OP = data.Part1.PK;
			asnLine1.WN_LineNo = 3;
			asnLine1.WN_SubLineNo = 1;
			asnLine1.WN_Quantity = 10;

			var asnLine2 = receive.AsnLines.AddNew();
			asnLine2.WN_OP = data.Part1.PK;
			asnLine2.WN_LineNo = 4;
			asnLine2.WN_SubLineNo = 1;
			asnLine2.WN_Quantity = 10;
			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation1, nonDockDoorLocation, "A", 20m);
			transfer.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("Precondition", 1, receive.Inventory.Count);
			AssertEquals("Precondition", true, receive.Inventory[0].HasPutawayTransfer);

			WhsReceiveASNHelper.ReconcileASN(receive);
			AssertEquals("Inventory count should not be increased.", 1, receive.Inventory.Count);
			AssertEquals("ASN lines are allocated to the inventory.", 20m, receive.Inventory[0].WI_ExpectedReceiptQuantity);
		}

		#endregion

		#region TestSingleProductMapping

		public void TestSingleProductMapping()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			//21
			var inv1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 15, ZGuid.Empty, "", "");
			var inv2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 6, ZGuid.Empty, "", "");
			inv1.WI_LineNo = 0;
			inv2.WI_LineNo = 0;

			var asnLine1 = receive.AsnLines.AddNew();
			asnLine1.WN_OP = data.Part1.PK;
			asnLine1.WN_LineNo = 3;
			asnLine1.WN_SubLineNo = 1;
			asnLine1.WN_Quantity = 10;

			var asnLine2 = receive.AsnLines.AddNew();
			asnLine2.WN_OP = data.Part1.PK;
			asnLine2.WN_LineNo = 4;
			asnLine2.WN_SubLineNo = 1;
			asnLine2.WN_Quantity = 7;
			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;

			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals(10m, receive.Inventory.Cast<WhsInventoryView>().Where(x => x.WI_LineNo == 3 && x.WI_SubLineNo == 1).Sum(x => x.WI_InDocketLineUnits));
			AssertEquals(7m, receive.Inventory.Cast<WhsInventoryView>().Where(x => x.WI_LineNo == 4 && x.WI_SubLineNo == 1).Sum(x => x.WI_InDocketLineUnits));
			AssertEquals(4m, receive.Inventory.Cast<WhsInventoryView>().Where(x => x.WI_LineNo == 0 && x.WI_SubLineNo == 0).Sum(x => x.WI_InDocketLineUnits));
		}

		#endregion

		#region TestTwoProductMapping

		public void TestTwoProductMapping()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);

			var inv1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 20, ZGuid.Empty, "", "");
			var inv2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2.PK, 5, ZGuid.Empty, "", "");
			inv1.WI_LineNo = 0;
			inv2.WI_LineNo = 0;

			var asnLine1 = receive.AsnLines.AddNew();
			asnLine1.WN_OP = data.Part1.PK;
			asnLine1.WN_LineNo = 3;
			asnLine1.WN_SubLineNo = 1;
			asnLine1.WN_Quantity = 10;

			var asnLine2 = receive.AsnLines.AddNew();
			asnLine2.WN_OP = data.Part1.PK;
			asnLine2.WN_LineNo = 4;
			asnLine2.WN_SubLineNo = 1;
			asnLine2.WN_Quantity = 7;

			var asnLine3 = receive.AsnLines.AddNew();
			asnLine3.WN_OP = data.Part2.PK;
			asnLine3.WN_LineNo = 4;
			asnLine3.WN_SubLineNo = 1;
			asnLine3.WN_Quantity = 8;

			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals(4, receive.Inventory.Count);
			// 20 units for product1
			Assert(receive.Inventory.Cast<WhsInventoryView>().Any(x => x.WI_OP == data.Part1.PK && x.WI_LineNo == 3 && x.WI_SubLineNo == 1 && x.WI_InDocketLineUnits == 10));
			Assert(receive.Inventory.Cast<WhsInventoryView>().Any(x => x.WI_OP == data.Part1.PK && x.WI_LineNo == 4 && x.WI_SubLineNo == 1 && x.WI_InDocketLineUnits == 7));
			Assert(receive.Inventory.Cast<WhsInventoryView>().Any(x => x.WI_OP == data.Part1.PK && x.WI_LineNo == 0 && x.WI_SubLineNo == 0 && x.WI_InDocketLineUnits == 3));

			// 5 units for product2
			Assert(receive.Inventory.Cast<WhsInventoryView>().Any(x => x.WI_OP == data.Part2.PK && x.WI_LineNo == 4 && x.WI_SubLineNo == 1 && x.WI_InDocketLineUnits == 5));
		}

		#endregion

		#region TestReconcileAsn_NewInventoryCreatedFromAsnLineHasNoLocation

		public void TestReconcileAsn_NewInventoryCreatedFromAsnLineHasNoLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateAsnLine(receive, data.Part1, 10m);

			AssertEquals("Precondition", 0, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("New inventory is created.", 1, receive.Lines.Count);
			var newReceiveLine = receive.Lines.Single();
			AssertEquals("New receive line has no location.", true, newReceiveLine.WE_WL.IsEmpty);
			AssertEquals("New receive line's inventory status is Arrived.", InventoryStatus.Codes.Arrived, newReceiveLine.WE_OriginalInventoryStatus);
		}

		#endregion

		#region TestAsnWithAttributes

		#region TestAsnWithAttributes_WithExactMatchedInventory

		public void TestAsnWithAttributes_WithExactMatchedInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "", "");
			line1.WE_LineNo = 0;
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 30m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "PA3", "");
			line2.WE_LineNo = 0;

			Helper.CreateAsnLine(receive, data.Part1, 50m, 1, 0, "", ZDate.Empty, ZDate.Empty, "PA1", "PA2", "PA3", "");

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("No new inventory is added.", 2, receive.Lines.Count);

			var inv1 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 10);
			AssertReceiveLineAfterAsnMapping(inv1, expectedTxQty: 10, expectedExpectedQty: 0, expectedLineNo: 0, expectedSubLineNo: 0,
				expectedPartAttrib1: "PA1", expectedPartAttrib2: "PA2", expectedPartAttrib3: "", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);

			var inv2 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 30);
			AssertReceiveLineAfterAsnMapping(inv2, expectedTxQty: 30, expectedExpectedQty: 50, expectedLineNo: 1, expectedSubLineNo: 0,
				expectedPartAttrib1: "PA1", expectedPartAttrib2: "PA2", expectedPartAttrib3: "PA3", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);
		}

		#endregion

		#region TestAsnMapping_ExactMatchedInventoryWithNoTransactionQty

		public void TestAsnMapping_ExactMatchedInventoryWithNoTransactionQty()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 30m);
			receiveLine.WE_TransactionQuantity = 0m;
			receiveLine.WE_LineNo = 0;

			Helper.CreateAsnLine(receive, data.Part1, 30m, 1, 0, "", ZDate.Empty, ZDate.Empty, "", "", "", "");

			AssertEquals("Precondition", 1, receive.Lines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);
			AssertEquals("No new inventory is created.", 1, receive.Lines.Count);

			AssertReceiveLineAfterAsnMapping(receiveLine, expectedTxQty: 0, expectedExpectedQty: 30, expectedLineNo: 1, expectedSubLineNo: 0,
				expectedPartAttrib1: "", expectedPartAttrib2: "", expectedPartAttrib3: "", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);
		}

		#endregion

		#region TestAsnWithAttributes_NoMatchedInventory

		public void TestAsnWithAttributes_NoMatchedInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "", "PA2", "", "");
			line1.WE_LineNo = 0;
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 30m, ZDate.Empty, ZDate.Empty, "", "PA2", "PA3", "");
			line2.WE_LineNo = 0;

			Helper.CreateAsnLine(receive, data.Part1, 20m, 1, 0, "", ZDate.Empty, ZDate.Empty, "PA1", "", "", "");

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("New inventory is added.", 3, receive.Lines.Count);

			var inv1 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 0);
			AssertReceiveLineAfterAsnMapping(inv1, expectedTxQty: 0, expectedExpectedQty: 20, expectedLineNo: 1, expectedSubLineNo: 0,
				expectedPartAttrib1: "PA1", expectedPartAttrib2: "", expectedPartAttrib3: "", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);

			var inv2 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 10);
			AssertReceiveLineAfterAsnMapping(inv2, expectedTxQty: 10, expectedExpectedQty: 0, expectedLineNo: 0, expectedSubLineNo: 0,
				expectedPartAttrib1: "", expectedPartAttrib2: "PA2", expectedPartAttrib3: "", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);

			var inv3 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 30);
			AssertReceiveLineAfterAsnMapping(inv3, expectedTxQty: 30, expectedExpectedQty: 0, expectedLineNo: 0, expectedSubLineNo: 0,
				expectedPartAttrib1: "", expectedPartAttrib2: "PA2", expectedPartAttrib3: "PA3", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);
		}

		#endregion

		#region TestAsnWithAttributes_MultipleMatchingInventories_ExactMatchGetsAllocatedFirst

		public void TestAsnWithAttributes_MultipleMatchingInventories_ExactMatchGetsAllocatedFirst()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "", "");
			line1.WE_LineNo = 0;
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 30m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "PA3", "");
			line2.WE_LineNo = 0;

			Helper.CreateAsnLine(receive, data.Part1, 20m, 1, 0, "", ZDate.Empty, ZDate.Empty, "PA1", "PA2", "", "");

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("New inventory is added.", 3, receive.Lines.Count);

			var inv1 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 20);
			AssertReceiveLineAfterAsnMapping(inv1, expectedTxQty: 20, expectedExpectedQty: 0, expectedLineNo: 0, expectedSubLineNo: 0,
				expectedPartAttrib1: "PA1", expectedPartAttrib2: "PA2", expectedPartAttrib3: "PA3", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);

			var inv2 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 10 && line.Inventory[0].UsedAttributesAndPalletIdCount == 2);
			AssertReceiveLineAfterAsnMapping(inv2, expectedTxQty: 10, expectedExpectedQty: 10, expectedLineNo: 1, expectedSubLineNo: 0,
				expectedPartAttrib1: "PA1", expectedPartAttrib2: "PA2", expectedPartAttrib3: "", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);

			var inv3 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 10 && line.Inventory[0].UsedAttributesAndPalletIdCount == 3);
			AssertReceiveLineAfterAsnMapping(inv3, expectedTxQty: 10, expectedExpectedQty: 10, expectedLineNo: 1, expectedSubLineNo: 0,
				expectedPartAttrib1: "PA1", expectedPartAttrib2: "PA2", expectedPartAttrib3: "PA3", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);
		}

		#endregion

		#region TestAsnWithAttributes_MultipleMatchingInventories_MatchedLineNumberGetsAllocatedFirst

		public void TestAsnWithAttributes_MultipleMatchingInventories_MatchedLineNumberGetsAllocatedFirst()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "", "");
			line1.WE_LineNo = 0;
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 30m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "PA3", "");
			line2.WE_LineNo = 1;

			Helper.CreateAsnLine(receive, data.Part1, 20m, 1, 0, "", ZDate.Empty, ZDate.Empty, "PA1", "PA2", "", "");

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("No new inventory is added.", 2, receive.Lines.Count);

			var inv1 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.Inventory[0].UsedAttributesAndPalletIdCount == 2);
			AssertReceiveLineAfterAsnMapping(inv1, expectedTxQty: 10, expectedExpectedQty: 0, expectedLineNo: 0, expectedSubLineNo: 0,
				expectedPartAttrib1: "PA1", expectedPartAttrib2: "PA2", expectedPartAttrib3: "", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);

			var inv2 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.Inventory[0].UsedAttributesAndPalletIdCount == 3);
			AssertReceiveLineAfterAsnMapping(inv2, expectedTxQty: 30, expectedExpectedQty: 20, expectedLineNo: 1, expectedSubLineNo: 0,
				expectedPartAttrib1: "PA1", expectedPartAttrib2: "PA2", expectedPartAttrib3: "PA3", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);
		}

		#endregion

		#region TestAsnWithAttributes_MultipleMatchingInventories_InventoryWithLeastNumberOfAttributesGetsAllocatedFirst

		public void TestAsnWithAttributes_MultipleMatchingInventories_InventoryWithLeastNumberOfAttributesGetsAllocatedFirst()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "", "");
			line1.WE_LineNo = 0;
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 30m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "PA3", "");
			line2.WE_LineNo = 0;

			Helper.CreateAsnLine(receive, data.Part1, 20m, 1, 0, "", ZDate.Empty, ZDate.Empty, "", "PA2", "", "");

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("New inventory is added.", 3, receive.Lines.Count);

			var inv1 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 20);
			AssertReceiveLineAfterAsnMapping(inv1, expectedTxQty: 20, expectedExpectedQty: 0, expectedLineNo: 0, expectedSubLineNo: 0,
				expectedPartAttrib1: "PA1", expectedPartAttrib2: "PA2", expectedPartAttrib3: "PA3", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);

			var inv2 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 10 && line.Inventory[0].UsedAttributesAndPalletIdCount == 2);
			AssertReceiveLineAfterAsnMapping(inv2, expectedTxQty: 10, expectedExpectedQty: 10, expectedLineNo: 1, expectedSubLineNo: 0,
				expectedPartAttrib1: "PA1", expectedPartAttrib2: "PA2", expectedPartAttrib3: "", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);

			var inv3 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 10 && line.Inventory[0].UsedAttributesAndPalletIdCount == 3);
			AssertReceiveLineAfterAsnMapping(inv3, expectedTxQty: 10, expectedExpectedQty: 10, expectedLineNo: 1, expectedSubLineNo: 0,
				expectedPartAttrib1: "PA1", expectedPartAttrib2: "PA2", expectedPartAttrib3: "PA3", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);
		}

		#endregion

		#region TestAsnWithNoAttibutes_MatchedToInventoriesWithAttributes

		public void TestAsnWithNoAttibutes_MatchedToInventoriesWithAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "", "");
			line1.WE_LineNo = 0;
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, ZDate.Empty, ZDate.Empty, "", "PA2", "", "");
			line2.WE_LineNo = 0;

			Helper.CreateAsnLine(receive, data.Part1, 10m, 1, 0, "", ZDate.Empty, ZDate.Empty, "", "", "", "");

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("New inventory is added.", 3, receive.Lines.Count);

			var inv1 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 10 && line.WE_ClientOrderedUnits == 0 && line.Inventory[0].UsedAttributesAndPalletIdCount == 1);
			AssertReceiveLineAfterAsnMapping(inv1, expectedTxQty: 10, expectedExpectedQty: 0, expectedLineNo: 0, expectedSubLineNo: 0,
				expectedPartAttrib1: "", expectedPartAttrib2: "PA2", expectedPartAttrib3: "", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);

			var inv2 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 10 && line.WE_ClientOrderedUnits == 0 && line.Inventory[0].UsedAttributesAndPalletIdCount == 2);
			AssertReceiveLineAfterAsnMapping(inv2, expectedTxQty: 10, expectedExpectedQty: 0, expectedLineNo: 0, expectedSubLineNo: 0,
				expectedPartAttrib1: "PA1", expectedPartAttrib2: "PA2", expectedPartAttrib3: "", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);

			var inv3 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 10 && line.WE_ClientOrderedUnits == 10);
			AssertReceiveLineAfterAsnMapping(inv3, expectedTxQty: 10, expectedExpectedQty: 10, expectedLineNo: 1, expectedSubLineNo: 0,
				expectedPartAttrib1: "", expectedPartAttrib2: "PA2", expectedPartAttrib3: "", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);
		}

		#endregion

		#region TestMultipleAsn_AsnWithMostNumberOfAttributesGetsAllocatedFirst

		public void TestMultipleAsn_AsnWithMostNumberOfAttributesGetsAllocatedFirst()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "", "");
			line1.WE_LineNo = 0;
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 30m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "PA3", "");
			line2.WE_LineNo = 0;

			Helper.CreateAsnLine(receive, data.Part1, 40m, 1, 0, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			Helper.CreateAsnLine(receive, data.Part1, 10m, 2, 0, "", ZDate.Empty, ZDate.Empty, "", "PA2", "PA3", "");

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition", 2, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("2 new inventories are added.", 4, receive.Lines.Count);

			var inv1 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 10 && line.Inventory[0].UsedAttributesAndPalletIdCount == 3);
			AssertReceiveLineAfterAsnMapping(inv1, expectedTxQty: 10, expectedExpectedQty: 10, expectedLineNo: 2, expectedSubLineNo: 0,
				expectedPartAttrib1: "PA1", expectedPartAttrib2: "PA2", expectedPartAttrib3: "PA3", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);

			var inv2 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 10 && line.Inventory[0].UsedAttributesAndPalletIdCount == 2);
			AssertReceiveLineAfterAsnMapping(inv2, expectedTxQty: 10, expectedExpectedQty: 10, expectedLineNo: 1, expectedSubLineNo: 0,
				expectedPartAttrib1: "PA1", expectedPartAttrib2: "PA2", expectedPartAttrib3: "", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);

			var inv3 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 20);
			AssertReceiveLineAfterAsnMapping(inv3, expectedTxQty: 20, expectedExpectedQty: 20, expectedLineNo: 1, expectedSubLineNo: 0,
				expectedPartAttrib1: "PA1", expectedPartAttrib2: "PA2", expectedPartAttrib3: "PA3", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);

			var inv4 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 0 && line.WE_ClientOrderedUnits == 10 && line.Inventory[0].UsedAttributesAndPalletIdCount == 0);
			AssertReceiveLineAfterAsnMapping(inv4, expectedTxQty: 0, expectedExpectedQty: 10, expectedLineNo: 1, expectedSubLineNo: 0,
				expectedPartAttrib1: "", expectedPartAttrib2: "", expectedPartAttrib3: "", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);
		}

		#endregion

		#region TestMultipleAsn_AsnWithLeastNumberOfInventoryMatchesGetsAllocatedFirst

		public void TestMultipleAsn_AsnWithLeastNumberOfInventoryMatchesGetsAllocatedFirst()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "", "PA2", "", "");
			line1.WE_LineNo = 0;
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "PA3", "");
			line2.WE_LineNo = 0;

			Helper.CreateAsnLine(receive, data.Part1, 30m, 1, 0, "", ZDate.Empty, ZDate.Empty, "", "PA2", "PA3", "");
			Helper.CreateAsnLine(receive, data.Part1, 10m, 2, 0, "", ZDate.Empty, ZDate.Empty, "", "PA2", "", "");

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition", 2, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("New inventory with expected quanity only is added.", 3, receive.Lines.Count);

			var inv1 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 10);
			AssertReceiveLineAfterAsnMapping(inv1, expectedTxQty: 10, expectedExpectedQty: 10, expectedLineNo: 2, expectedSubLineNo: 0,
				expectedPartAttrib1: "", expectedPartAttrib2: "PA2", expectedPartAttrib3: "", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);

			var inv2 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 20);
			AssertReceiveLineAfterAsnMapping(inv2, expectedTxQty: 20, expectedExpectedQty: 20, expectedLineNo: 1, expectedSubLineNo: 0,
				expectedPartAttrib1: "PA1", expectedPartAttrib2: "PA2", expectedPartAttrib3: "PA3", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);

			var inv3 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 0);
			AssertReceiveLineAfterAsnMapping(inv3, expectedTxQty: 0, expectedExpectedQty: 10, expectedLineNo: 1, expectedSubLineNo: 0,
				expectedPartAttrib1: "", expectedPartAttrib2: "PA2", expectedPartAttrib3: "PA3", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);
		}

		#endregion

		#region TestMultipleAsn_AsnWithHigherLineNumberGetsAllocatedFirst

		public void TestMultipleAsn_AsnWithHigherLineNumberGetsAllocatedFirst()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "", "PA2", "", "");
			line1.WE_LineNo = 0;
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "", "");
			line2.WE_LineNo = 0;

			Helper.CreateAsnLine(receive, data.Part1, 30m, 1, 0, "", ZDate.Empty, ZDate.Empty, "", "PA2", "", "");
			Helper.CreateAsnLine(receive, data.Part1, 10m, 2, 0, "", ZDate.Empty, ZDate.Empty, "", "PA2", "", "");

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition", 2, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("1 new inventory is added.", 3, receive.Lines.Count);

			var inv1 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 0);
			AssertReceiveLineAfterAsnMapping(inv1, expectedTxQty: 0, expectedExpectedQty: 10, expectedLineNo: 2, expectedSubLineNo: 0,
				expectedPartAttrib1: "", expectedPartAttrib2: "PA2", expectedPartAttrib3: "", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);

			var inv2 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 10);
			AssertReceiveLineAfterAsnMapping(inv2, expectedTxQty: 10, expectedExpectedQty: 10, expectedLineNo: 1, expectedSubLineNo: 0,
				expectedPartAttrib1: "", expectedPartAttrib2: "PA2", expectedPartAttrib3: "", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);

			var inv3 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 20);
			AssertReceiveLineAfterAsnMapping(inv3, expectedTxQty: 20, expectedExpectedQty: 20, expectedLineNo: 1, expectedSubLineNo: 0,
				expectedPartAttrib1: "PA1", expectedPartAttrib2: "PA2", expectedPartAttrib3: "", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);
		}

		#endregion

		#region TestAsnWithPalletId_PalletIdNeedsToMatch

		public void TestAsnWithPalletId_PalletIdNeedsToMatch()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, null, "DEF", ZDate.Empty, ZDate.Empty, "PA1", "PA2", "", "");
			line1.WE_LineNo = 0;
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, null, "EFG", ZDate.Empty, ZDate.Empty, "", "PA2", "", "");
			line2.WE_LineNo = 0;

			Helper.CreateAsnLine(receive, data.Part1, 10m, 1, 0, "ABC", ZDate.Empty, ZDate.Empty, "", "", "", "");

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("New inventory is added from unallocated ASN line.", 3, receive.Lines.Count);

			var inv1 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_PalletID == "ABC");
			AssertReceiveLineAfterAsnMapping(inv1, expectedTxQty: 0, expectedExpectedQty: 10, expectedLineNo: 1, expectedSubLineNo: 0,
				expectedPartAttrib1: "", expectedPartAttrib2: "", expectedPartAttrib3: "", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);

			var inv2 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_PalletID == "DEF");
			AssertReceiveLineAfterAsnMapping(inv2, expectedTxQty: 10, expectedExpectedQty: 0, expectedLineNo: 0, expectedSubLineNo: 0,
				expectedPartAttrib1: "PA1", expectedPartAttrib2: "PA2", expectedPartAttrib3: "", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);

			var inv3 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_PalletID == "EFG");
			AssertReceiveLineAfterAsnMapping(inv3, expectedTxQty: 20, expectedExpectedQty: 0, expectedLineNo: 0, expectedSubLineNo: 0,
				expectedPartAttrib1: "", expectedPartAttrib2: "PA2", expectedPartAttrib3: "", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);
		}

		#endregion

		#region TestAsnWithoutPalletId_GetsMatchedToInventories

		public void TestAsnWithoutPalletId_GetsMatchedToInventories()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, null, "DEF", ZDate.Empty, ZDate.Empty, "", "PA2", "", "");
			line1.WE_LineNo = 0;
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, null, "EFG", ZDate.Empty, ZDate.Empty, "PA1", "PA2", "", "");
			line2.WE_LineNo = 0;

			Helper.CreateAsnLine(receive, data.Part1, 20m, 1, 0, "", ZDate.Empty, ZDate.Empty, "", "", "", "");

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("New inventory is added from inventory 2 excess.", 3, receive.Lines.Count);

			var inv1 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_ClientOrderedUnits == 0 && line.WE_PalletID == "EFG");
			AssertReceiveLineAfterAsnMapping(inv1, expectedTxQty: 10, expectedExpectedQty: 0, expectedLineNo: 0, expectedSubLineNo: 0,
				expectedPartAttrib1: "PA1", expectedPartAttrib2: "PA2", expectedPartAttrib3: "", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);

			var inv2 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_ClientOrderedUnits == 10 && line.WE_PalletID == "DEF");
			AssertReceiveLineAfterAsnMapping(inv2, expectedTxQty: 10, expectedExpectedQty: 10, expectedLineNo: 1, expectedSubLineNo: 0,
				expectedPartAttrib1: "", expectedPartAttrib2: "PA2", expectedPartAttrib3: "", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);

			var inv3 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_ClientOrderedUnits == 10 && line.WE_PalletID == "EFG");
			AssertReceiveLineAfterAsnMapping(inv3, expectedTxQty: 10, expectedExpectedQty: 10, expectedLineNo: 1, expectedSubLineNo: 0,
				expectedPartAttrib1: "PA1", expectedPartAttrib2: "PA2", expectedPartAttrib3: "", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);
		}

		#endregion

		#region TestAsnWithoutPalletId_InventoryWithoutPalletIdGetsAllocatedFirst

		public void TestAsnWithoutPalletId_InventoryWithoutPalletIdGetsAllocatedFirst()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, null, "DEF", ZDate.Empty, ZDate.Empty, "PA1", "", "", "");
			line1.WE_LineNo = 0;
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, null, "", ZDate.Empty, ZDate.Empty, "", "PA2", "", "");
			line2.WE_LineNo = 0;

			Helper.CreateAsnLine(receive, data.Part1, 20m, 1, 0, "", ZDate.Empty, ZDate.Empty, "", "", "", "");

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("No new inventory is added.", 2, receive.Lines.Count);

			var inv1 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_PalletID.IsEmpty);
			AssertReceiveLineAfterAsnMapping(inv1, expectedTxQty: 20, expectedExpectedQty: 20, expectedLineNo: 1, expectedSubLineNo: 0,
				expectedPartAttrib1: "", expectedPartAttrib2: "PA2", expectedPartAttrib3: "", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);

			var inv2 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_PalletID == "DEF");
			AssertReceiveLineAfterAsnMapping(inv2, expectedTxQty: 10, expectedExpectedQty: 0, expectedLineNo: 0, expectedSubLineNo: 0,
				expectedPartAttrib1: "PA1", expectedPartAttrib2: "", expectedPartAttrib3: "", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);
		}

		#endregion

		#region TestInventoriesWithLineNumbers_LineNumbersAreRetained

		public void TestInventoriesWithLineNumbers_LineNumbersAreRetained()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "", "PA2", "", "");
			line1.WE_LineNo = 2;
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "", "");
			line2.WE_LineNo = 3;

			Helper.CreateAsnLine(receive, data.Part1, 20m, 1, 0, "", ZDate.Empty, ZDate.Empty, "", "", "", "");

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("No new inventory is added.", 2, receive.Lines.Count);

			var inv1 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 10 && line.Inventory[0].UsedAttributesAndPalletIdCount == 1);
			AssertReceiveLineAfterAsnMapping(inv1, expectedTxQty: 10, expectedExpectedQty: 10, expectedLineNo: 2, expectedSubLineNo: 0,
				expectedPartAttrib1: "", expectedPartAttrib2: "PA2", expectedPartAttrib3: "", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);

			var inv2 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 20 && line.Inventory[0].UsedAttributesAndPalletIdCount == 2);
			AssertReceiveLineAfterAsnMapping(inv2, expectedTxQty: 20, expectedExpectedQty: 10, expectedLineNo: 3, expectedSubLineNo: 0,
				expectedPartAttrib1: "PA1", expectedPartAttrib2: "PA2", expectedPartAttrib3: "", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);
		}

		#endregion

		#region TestPackingDateMatching

		public void TestPackingDateMatching()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);

			var matchingDate = ZDate.Today;
			var mismatchedDate = ZDate.Today.AddDays(-1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, ZDate.Empty, matchingDate, "PA1", "PA2", "PA3", "");
			line1.WE_LineNo = 0;
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 30m, ZDate.Empty, mismatchedDate, "PA1", "PA2", "PA3", "");
			line2.WE_LineNo = 0;

			Helper.CreateAsnLine(receive, data.Part1, 20m, 1, 0, "", matchingDate, ZDate.Empty, "PA1", "PA2", "PA3", "");

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("No new inventory is added.", 2, receive.Lines.Count);

			var inv1 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 10);
			AssertReceiveLineAfterAsnMapping(inv1, expectedTxQty: 10, expectedExpectedQty: 20, expectedLineNo: 1, expectedSubLineNo: 0,
				expectedPartAttrib1: "PA1", expectedPartAttrib2: "PA2", expectedPartAttrib3: "PA3", expectedSerialNumber: "",
				expectedPackingDate: matchingDate, expectedExpiryDate: ZDate.Empty);

			var inv2 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 30);
			AssertReceiveLineAfterAsnMapping(inv2, expectedTxQty: 30, expectedExpectedQty: 0, expectedLineNo: 0, expectedSubLineNo: 0,
				expectedPartAttrib1: "PA1", expectedPartAttrib2: "PA2", expectedPartAttrib3: "PA3", expectedSerialNumber: "",
				expectedPackingDate: mismatchedDate, expectedExpiryDate: ZDate.Empty);
		}

		#endregion

		#region TestExpiryDateMatching

		public void TestExpiryDateMatching()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			var matchingDate = ZDate.Today;
			var mismatchedDate = ZDate.Today.AddDays(-1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, matchingDate, ZDate.Empty, "PA1", "PA2", "PA3", "");
			line1.WE_LineNo = 0;
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 30m, mismatchedDate, ZDate.Empty, "PA1", "PA2", "PA3", "");
			line2.WE_LineNo = 0;

			Helper.CreateAsnLine(receive, data.Part1, 20m, 1, 0, "", ZDate.Empty, matchingDate, "PA1", "PA2", "PA3", "");

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("No new inventory is added.", 2, receive.Lines.Count);

			var inv1 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 10);
			AssertReceiveLineAfterAsnMapping(inv1, expectedTxQty: 10, expectedExpectedQty: 20, expectedLineNo: 1, expectedSubLineNo: 0,
				expectedPartAttrib1: "PA1", expectedPartAttrib2: "PA2", expectedPartAttrib3: "PA3", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: matchingDate);

			var inv2 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 30);
			AssertReceiveLineAfterAsnMapping(inv2, expectedTxQty: 30, expectedExpectedQty: 0, expectedLineNo: 0, expectedSubLineNo: 0,
				expectedPartAttrib1: "PA1", expectedPartAttrib2: "PA2", expectedPartAttrib3: "PA3", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: mismatchedDate);
		}

		#endregion

		#region TestSerialNumberMatching

		public void TestSerialNumberMatching()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "PA2", "", "");
			line1.WE_SerialNumber = "SN1";
			line1.WE_LineNo = 0;
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "PA2", "", "");
			line2.WE_SerialNumber = "SN2";
			line2.WE_LineNo = 0;

			Helper.CreateAsnLine(receive, data.Part1, 1m, 1, 0, "", ZDate.Empty, ZDate.Empty, "", "PA2", "", "SN1");

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("No new inventory is added.", 2, receive.Lines.Count);

			AssertReceiveLineAfterAsnMapping(line1, expectedTxQty: 1, expectedExpectedQty: 1, expectedLineNo: 1, expectedSubLineNo: 0,
				expectedPartAttrib1: "", expectedPartAttrib2: "PA2", expectedPartAttrib3: "", expectedSerialNumber: "SN1",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);

			AssertReceiveLineAfterAsnMapping(line2, expectedTxQty: 1, expectedExpectedQty: 0, expectedLineNo: 0, expectedSubLineNo: 0,
				expectedPartAttrib1: "", expectedPartAttrib2: "PA2", expectedPartAttrib3: "", expectedSerialNumber: "SN2",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);
		}

		public void TestSerialNumberMatching_DoesNotIgnoreOtherAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "PA1", "", "");
			line1.WE_SerialNumber = "SN1";
			line1.WE_LineNo = 0;
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "PA2", "", "");
			line2.WE_SerialNumber = "SN2";
			line2.WE_LineNo = 0;

			Helper.CreateAsnLine(receive, data.Part1, 1m, 1, 0, "", ZDate.Empty, ZDate.Empty, "", "PA2", "", "SN1");

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("New inventory is added.", 3, receive.Lines.Count);

			AssertReceiveLineAfterAsnMapping(line1, expectedTxQty: 1, expectedExpectedQty: 0, expectedLineNo: 0, expectedSubLineNo: 0,
				expectedPartAttrib1: "", expectedPartAttrib2: "PA1", expectedPartAttrib3: "", expectedSerialNumber: "SN1",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);
			AssertNoErrors(line1.WE_PartAttrib1Info);

			AssertReceiveLineAfterAsnMapping(line2, expectedTxQty: 1, expectedExpectedQty: 0, expectedLineNo: 0, expectedSubLineNo: 0,
				expectedPartAttrib1: "", expectedPartAttrib2: "PA2", expectedPartAttrib3: "", expectedSerialNumber: "SN2",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);

			var newInventory = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_ClientOrderedUnits != 0);
			AssertReceiveLineAfterAsnMapping(newInventory, expectedTxQty: 0, expectedExpectedQty: 1, expectedLineNo: 1, expectedSubLineNo: 0,
				expectedPartAttrib1: "", expectedPartAttrib2: "PA2", expectedPartAttrib3: "", expectedSerialNumber: "SN1",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);
			AssertNoErrors(newInventory.WE_PartAttrib1Info);
		}

		#endregion

		#region TestReconcileAsnDeletesZeroTransactionAndExpectedQtyInventories

		public void TestReconcileAsnDeletesZeroTransactionAndExpectedQtyInventories()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 0m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "", "");
			line1.WE_LineNo = 0;
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 0m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "PA3", "");
			line2.WE_LineNo = 0;

			Helper.CreateAsnLine(receive, data.Part1, 50m, 1, 0, "", ZDate.Empty, ZDate.Empty, "PA1", "PA2", "PA3", "");

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("Original inventories are deleted, inventory on receive is created from unallocated ASN.", 1, receive.Lines.Count);

			var inv1 = receive.Lines.Cast<WhsReceiveLine>().Single();
			AssertReceiveLineAfterAsnMapping(inv1, expectedTxQty: 0, expectedExpectedQty: 50, expectedLineNo: 1, expectedSubLineNo: 0,
				expectedPartAttrib1: "PA1", expectedPartAttrib2: "PA2", expectedPartAttrib3: "PA3", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);
		}

		#endregion

		#region TestReconcileAsn_DifferentProductsOnAsnAndInventory

		public void TestReconcileAsn_DifferentProductsOnAsnAndInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 30m);
			line1.WE_LineNo = 0;

			Helper.CreateAsnLine(receive, data.Part2, 50m, 1, 0, "", ZDate.Empty, ZDate.Empty, "", "", "", "");

			AssertEquals("Precondition", 1, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("New inventory is created from the unallocated asn line.", 2, receive.Lines.Count);

			var inv1 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 30);
			AssertReceiveLineAfterAsnMapping(inv1, expectedTxQty: 30, expectedExpectedQty: 0, expectedLineNo: 0, expectedSubLineNo: 0,
				expectedPartAttrib1: "", expectedPartAttrib2: "", expectedPartAttrib3: "", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);

			var inv2 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 0);
			AssertReceiveLineAfterAsnMapping(inv2, expectedTxQty: 0, expectedExpectedQty: 50, expectedLineNo: 1, expectedSubLineNo: 0,
				expectedPartAttrib1: "", expectedPartAttrib2: "", expectedPartAttrib3: "", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);
		}

		#endregion

		#region TestInventoriesWithCrossDockedQuantities

		public void TestInventoriesWithCrossDockedQuantities()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 30m);
			receiveLine.WE_LineNo = 0;

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 30m);
			Helper.CreateReservePickLine(orderLine, receiveLine.Inventory[0], 30m);
			Factory.Save();

			AssertEquals("Precondition", 30m, receiveLine.ReservedQuantity);
			AssertEquals("Precondition", 30m, orderLine.WE_CrossDockQuantity);

			Helper.CreateAsnLine(receive, data.Part1, 20m, 1, 0, "", ZDate.Empty, ZDate.Empty, "", "", "", "");

			WhsReceiveASNHelper.ReconcileASN(receive);
			AssertEquals("New inventory is created from the receive line split.", 2, receive.Lines.Count);

			var inv1 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 20);
			AssertReceiveLineAfterAsnMapping(inv1, expectedTxQty: 20, expectedExpectedQty: 20, expectedLineNo: 1, expectedSubLineNo: 0,
				expectedPartAttrib1: "", expectedPartAttrib2: "", expectedPartAttrib3: "", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);

			AssertEquals(20m, inv1.ReservedQuantity);
			AssertEquals(false, inv1.HasErrors);

			var inv2 = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 10);
			AssertReceiveLineAfterAsnMapping(inv2, expectedTxQty: 10, expectedExpectedQty: 0, expectedLineNo: 0, expectedSubLineNo: 0,
				expectedPartAttrib1: "", expectedPartAttrib2: "", expectedPartAttrib3: "", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);

			AssertEquals(10m, inv2.ReservedQuantity);
			AssertEquals(false, inv2.HasErrors);
		}

		#endregion

		#region TestInventoriesWithCrossDockedQuantities_TransactionAndExpectedQtyZeroedOut

		public void TestInventoriesWithCrossDockedQuantities_TransactionAndExpectedQtyZeroedOut()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 30m);
			receiveLine.WE_TransactionQuantity = 0m;
			receiveLine.WE_LineNo = 0;

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 30m);
			Helper.CreateReservePickLine(orderLine, receiveLine.Inventory[0], 30m);
			Factory.Save();

			AssertEquals("Precondition", 30m, receiveLine.ReservedQuantity);
			AssertEquals("Precondition", 30m, orderLine.WE_CrossDockQuantity);

			Helper.CreateAsnLine(receive, data.Part1, 30m, 1, 0, "", ZDate.Empty, ZDate.Empty, "", "", "", "");

			WhsReceiveASNHelper.ReconcileASN(receive);
			AssertEquals("No new inventory is created.", 1, receive.Lines.Count);
			AssertEquals("Receive line is not deleted.", false, receiveLine.IsDeleted);
			AssertEquals("Reserved Quantity", 30m, receiveLine.ReservedQuantity);
			AssertEquals("Transaction Quantity", 0m, receiveLine.WE_TransactionQuantity);
			AssertEquals("Expected Quantity", 30m, receiveLine.WE_ClientOrderedUnits);
			AssertEquals("Receive line has no errors.", false, receiveLine.HasErrors);
		}

		#endregion

		#region TestAsnMapping_MatchedToInventoriesWithMoreAttributes_ExcessUnAssignedAsnQtyCreatesUnderReceiveLine

		public void TestAsnMapping_MatchedToInventoriesWithMoreAttributes_ExcessUnAssignedAsnQtyCreatesUnderReceiveLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "", "");
			receiveLine.WE_LineNo = 0;

			Helper.CreateAsnLine(receive, data.Part1, 20m, 1, 0, "", ZDate.Empty, ZDate.Empty, "PA1", "", "", "");

			AssertEquals("Precondition", 1, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("New inventory is added.", 2, receive.Lines.Count);

			AssertReceiveLineAfterAsnMapping(receiveLine, expectedTxQty: 10, expectedExpectedQty: 10, expectedLineNo: 1, expectedSubLineNo: 0,
				expectedPartAttrib1: "PA1", expectedPartAttrib2: "PA2", expectedPartAttrib3: "", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);

			var newReceiveLine = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 0 && line.WE_ClientOrderedUnits == 10 && line.Inventory[0].UsedAttributesAndPalletIdCount == 1);
			AssertReceiveLineAfterAsnMapping(newReceiveLine, expectedTxQty: 0, expectedExpectedQty: 10, expectedLineNo: 1, expectedSubLineNo: 0,
				expectedPartAttrib1: "PA1", expectedPartAttrib2: "", expectedPartAttrib3: "", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);
		}

		#endregion

		#region TestAsnMapping_MatchedToInventoriesWithMoreAttributes_ExactMatchGetsOverAssigned

		public void TestAsnMapping_MatchedToInventoriesWithMoreAttributes_ExactMatchGetsOverAssigned()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "", "");
			receiveLine1.WE_LineNo = 0;
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, ZDate.Empty, ZDate.Empty, "PA1", "", "", "");
			receiveLine2.WE_LineNo = 0;

			Helper.CreateAsnLine(receive, data.Part1, 40m, 1, 0, "", ZDate.Empty, ZDate.Empty, "PA1", "", "", "");

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("No new inventory is added.", 2, receive.Lines.Count);

			AssertReceiveLineAfterAsnMapping(receiveLine1, expectedTxQty: 10, expectedExpectedQty: 10, expectedLineNo: 1, expectedSubLineNo: 0,
				expectedPartAttrib1: "PA1", expectedPartAttrib2: "PA2", expectedPartAttrib3: "", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);

			AssertReceiveLineAfterAsnMapping(receiveLine2, expectedTxQty: 20, expectedExpectedQty: 30, expectedLineNo: 1, expectedSubLineNo: 0,
				expectedPartAttrib1: "PA1", expectedPartAttrib2: "", expectedPartAttrib3: "", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);
		}

		#endregion

		#region TestAsnMapping_ExactMatchGetsOverAssigned

		public void TestAsnMapping_ExactMatchGetsOverAssigned()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, ZDate.Empty, ZDate.Empty, "PA1", "", "", "");
			receiveLine.WE_LineNo = 0;

			Helper.CreateAsnLine(receive, data.Part1, 30m, 1, 0, "", ZDate.Empty, ZDate.Empty, "PA1", "", "", "");

			AssertEquals("Precondition", 1, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("No new inventory is added.", 1, receive.Lines.Count);

			AssertReceiveLineAfterAsnMapping(receiveLine, expectedTxQty: 20, expectedExpectedQty: 30, expectedLineNo: 1, expectedSubLineNo: 0,
				expectedPartAttrib1: "PA1", expectedPartAttrib2: "", expectedPartAttrib3: "", expectedSerialNumber: "",
				expectedPackingDate: ZDate.Empty, expectedExpiryDate: ZDate.Empty);
		}

		#endregion

		void AssertReceiveLineAfterAsnMapping(WhsReceiveLine line, ZDecimal expectedTxQty, ZDecimal expectedExpectedQty, ZInt expectedLineNo, ZInt expectedSubLineNo,
			ZString expectedPartAttrib1, ZString expectedPartAttrib2, ZString expectedPartAttrib3, ZString expectedSerialNumber, ZDate expectedPackingDate, ZDate expectedExpiryDate)
		{
			AssertEquals("WE_TransactionQuantity", expectedTxQty, line.WE_TransactionQuantity);
			AssertEquals("WE_ClientOrderedUnits", expectedExpectedQty, line.WE_ClientOrderedUnits);
			AssertEquals("WE_PartAttrib1", expectedPartAttrib1, line.WE_PartAttrib1);
			AssertEquals("WE_PartAttrib2", expectedPartAttrib2, line.WE_PartAttrib2);
			AssertEquals("WE_PartAttrib3", expectedPartAttrib3, line.WE_PartAttrib3);
			AssertEquals("WE_SerialNumber", expectedSerialNumber, line.WE_SerialNumber);
			AssertEquals("WE_PackingDate", expectedPackingDate, line.WE_PackingDate);
			AssertEquals("WE_ExpiryDate", expectedExpiryDate, line.WE_ExpiryDate);
			AssertEquals("WE_LineNo", expectedLineNo, line.WE_LineNo);
			AssertEquals("WE_SubLineNo", expectedSubLineNo, line.WE_SubLineNo);
		}

		#endregion

		#region TestRemappingReservedStock

		#region TestRemappingReservedStock

		public void TestRemappingReservedStock()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLineWithNoTxnQty = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLineWithNoTxnQty.WE_TransactionQuantity = 0m;
			var receiveLineWithTxnQty = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateReservePickLine(orderLine, receiveLineWithNoTxnQty.Inventory[0], 10m);
			Factory.Save();

			Helper.CreateAsnLine(receive, data.Part1, 10m, 1, 0);

			AssertEquals("Precondition", 10m, receiveLineWithNoTxnQty.ReservedQuantity);
			AssertEquals("Precondition", 0m, receiveLineWithTxnQty.ReservedQuantity);
			AssertEquals("Precondition", 10m, orderLine.WE_CrossDockQuantity);
			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("Inventory with 0 transaction quantity is deleted.", 1, receive.Lines.Count);
			AssertEquals("Receive line with no transaction quantity is deleted.", true, receiveLineWithNoTxnQty.IsDeleted);
			AssertEquals("Reserved stock is remapped to receive line with transaction quantity.", 10m, receiveLineWithTxnQty.ReservedQuantity);
		}

		#endregion

		#region TestRemappingReservedStock_MultipleReservedPickLines

		public void TestRemappingReservedStock_MultipleReservedPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLineWithNoTxnQty = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLineWithNoTxnQty.WE_TransactionQuantity = 0m;
			var receiveLineWithTxnQty = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Helper.CreateReservePickLine(orderLine1, receiveLineWithNoTxnQty.Inventory[0], 5m);
			Helper.CreateReservePickLine(orderLine2, receiveLineWithNoTxnQty.Inventory[0], 5m);
			Factory.Save();

			Helper.CreateAsnLine(receive, data.Part1, 10m, 1, 0);

			AssertEquals("Precondition", 10m, receiveLineWithNoTxnQty.ReservedQuantity);
			AssertEquals("Precondition", 2, receiveLineWithNoTxnQty.ReservedPickLines.Count);
			AssertEquals("Precondition", 0m, receiveLineWithTxnQty.ReservedQuantity);
			AssertEquals("Precondition", 5m, orderLine1.WE_CrossDockQuantity);
			AssertEquals("Precondition", 5m, orderLine2.WE_CrossDockQuantity);
			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("Inventory with 0 transaction quantity is deleted.", 1, receive.Lines.Count);
			AssertEquals("Receive line with no transaction quantity is deleted.", true, receiveLineWithNoTxnQty.IsDeleted);
			AssertEquals("Reserved stock is remapped to receive line with transaction quantity.", 10m, receiveLineWithTxnQty.ReservedQuantity);
			AssertEquals("Reserved pick lines are remapped to receive line with transaction quantity.", 2, receiveLineWithTxnQty.ReservedPickLines.Count);
		}

		#endregion

		#region TestRemappingReservedStock_StockNotRemappedIfLineHasSufficientQty

		public void TestRemappingReservedStock_StockNotRemappedIfLineHasSufficientQty()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLineWithTxnQty1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			var receiveLineWithTxnQty2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateReservePickLine(orderLine, receiveLineWithTxnQty1.Inventory[0], 10m);
			Factory.Save();

			Helper.CreateAsnLine(receive, data.Part1, 10m, 1, 0);

			AssertEquals("Precondition", 10m, receiveLineWithTxnQty1.ReservedQuantity);
			AssertEquals("Precondition", 0m, receiveLineWithTxnQty2.ReservedQuantity);
			AssertEquals("Precondition", 10m, orderLine.WE_CrossDockQuantity);
			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals(2, receive.Lines.Count);
			AssertEquals("Reserved stock stays with original receive line.", 10m, receiveLineWithTxnQty1.ReservedQuantity);
			AssertEquals("Reserved stock stays with original receive line.", 0m, receiveLineWithTxnQty2.ReservedQuantity);
		}

		#endregion

		#region TestRemappingReservedStock_StaysWithOriginalLineWhenNoOtherInventoriesToRemapTo

		public void TestRemappingReservedStock_StaysWithOriginalLineWhenNoOtherInventoriesToRemapTo()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLineWithNoTxnQty = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLineWithNoTxnQty.WE_TransactionQuantity = 0m;

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateReservePickLine(orderLine, receiveLineWithNoTxnQty.Inventory[0], 10m);
			Factory.Save();

			Helper.CreateAsnLine(receive, data.Part1, 10m, 1, 0);

			AssertEquals("Precondition", 10m, receiveLineWithNoTxnQty.ReservedQuantity);
			AssertEquals("Precondition", 10m, orderLine.WE_CrossDockQuantity);
			AssertEquals("Precondition", 1, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("No new inventory is created.", 1, receive.Lines.Count);
			AssertEquals("Receive line with no transaction quantity is not deleted as reserved stock is not remapped.", false, receiveLineWithNoTxnQty.IsDeleted);
			AssertEquals("Reserved Quantity", 10m, receiveLineWithNoTxnQty.ReservedQuantity);
			AssertEquals("Expected Quantity", 10m, receiveLineWithNoTxnQty.WE_ClientOrderedUnits);
		}

		#endregion

		#region TestRemappingReservedStock_SplittingReceiveLineOnAsnReconcilliationSplitsReservedStock

		public void TestRemappingReservedStock_SplittingReceiveLineOnAsnReconcilliationSplitsReservedStock()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m);
			receiveLine.WE_LineNo = 0;

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			Helper.CreateReservePickLine(orderLine, receiveLine.Inventory[0], 20m);
			Factory.Save();

			Helper.CreateAsnLine(receive, data.Part1, 10m, 1, 0);

			AssertEquals("Precondition", 20m, receiveLine.ReservedQuantity);
			AssertEquals("Precondition", 20m, orderLine.WE_CrossDockQuantity);
			AssertEquals("Precondition", 1, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("New inventory is created from the split.", 2, receive.Lines.Count);
			AssertEquals("ReservedQuantity", 10m, receiveLine.ReservedQuantity);
			AssertEquals("LineNo", (short)1, receiveLine.WE_LineNo);

			var newReceiveLine = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_ClientOrderedUnits == 0);
			AssertEquals("Reserved Quantity", 10m, newReceiveLine.ReservedQuantity);
			AssertEquals("LineNo", (short)0, newReceiveLine.WE_LineNo);
		}

		#endregion

		#region TestRemappingReservedStock_LineWithTransactionQtyGetsPriority

		public void TestRemappingReservedStock_LineWithTransactionQtyGetsPriority()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLineWithNoTxnQty = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLineWithNoTxnQty.WE_TransactionQuantity = 0m;
			var receiveLineWithTxnQty = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "PA1", "", "", "");

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateReservePickLine(orderLine, receiveLineWithNoTxnQty.Inventory[0], 10m);
			Factory.Save();

			Helper.CreateAsnLine(receive, data.Part1, 10m, 1, 0, "", ZDate.Empty, ZDate.Empty, "", "PA2", "", "");

			AssertEquals("Precondition", 10m, receiveLineWithNoTxnQty.ReservedQuantity);
			AssertEquals("Precondition", 10m, orderLine.WE_CrossDockQuantity);
			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("Zero transaction qty line is deleted but new inventory is created from the asn reconcilliation.", 2, receive.Lines.Count);
			AssertEquals("Reserved Quantity", 10m, receiveLineWithTxnQty.ReservedQuantity);

			var newReceiveLine = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_TransactionQuantity == 0);
			AssertEquals("Reserved Quantity", 0m, newReceiveLine.ReservedQuantity);
		}

		#endregion

		#region TestRemappingReservedStock_LineWithGreaterTransactionQtyGetsPriority

		public void TestRemappingReservedStock_LineWithGreaterTransactionQtyGetsPriority()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLineWithNoTxnQty = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "PA1", "", "", "");
			receiveLineWithNoTxnQty.WE_TransactionQuantity = 0m;
			var receiveLineWithTxnQty1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "PA1", "", "", "");
			var receiveLineWithTxnQty2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, ZDate.Empty, ZDate.Empty, "PA1", "", "", "");

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateReservePickLine(orderLine, receiveLineWithNoTxnQty.Inventory[0], 10m);
			Factory.Save();

			Helper.CreateAsnLine(receive, data.Part1, 10m, 1, 0, "", ZDate.Empty, ZDate.Empty, "PA1", "", "", "");

			AssertEquals("Precondition", 10m, receiveLineWithNoTxnQty.ReservedQuantity);
			AssertEquals("Precondition", 10m, orderLine.WE_CrossDockQuantity);
			AssertEquals("Precondition", 3, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("Zero transaction qty line is deleted.", 2, receive.Lines.Count);
			AssertEquals("Reserved Quantity", 0m, receiveLineWithTxnQty1.ReservedQuantity);
			AssertEquals("Reserved Quantity", 10m, receiveLineWithTxnQty2.ReservedQuantity);
		}

		#endregion

		#region TestRemappingReservedStock_ExpectedQuantityGreaterThanTransactionQuantity

		public void TestRemappingReservedStock_ExpectedQuantityGreaterThanTransactionQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLineWithNoTxnQty = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m);
			receiveLineWithNoTxnQty.WE_TransactionQuantity = 0m;
			var receiveLineWithTxnQty = Helper.CreateWhsReceiveLine(receive, data.Part1, 15m, ZDate.Empty, ZDate.Empty, "PA1", "", "", "");

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			Helper.CreateReservePickLine(orderLine, receiveLineWithNoTxnQty.Inventory[0], 20m);
			Factory.Save();

			Helper.CreateAsnLine(receive, data.Part1, 20m, 1, 0, "", ZDate.Empty, ZDate.Empty, "PA1", "", "", "");

			AssertEquals("Precondition", 20m, receiveLineWithNoTxnQty.ReservedQuantity);
			AssertEquals("Precondition", 20m, orderLine.WE_CrossDockQuantity);
			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("Zero transaction qty line is not deleted as not all reserved quantity is remapped.", 2, receive.Lines.Count);
			AssertEquals("Reserved Quantity", 5m, receiveLineWithNoTxnQty.ReservedQuantity);
			AssertHasError(receiveLineWithNoTxnQty.WE_TransactionQuantityInfo, WhsValidationHelper.UnfulfilledCrossDockedUnitsError);
			AssertHasError(receiveLineWithNoTxnQty.WE_ClientOrderedUnitsInfo, WhsValidationHelper.UnfulfilledCrossDockedUnitsError);

			AssertEquals("Expected Quantity", 20m, receiveLineWithTxnQty.WE_ClientOrderedUnits);
			AssertEquals("Transaction Quantity", 15m, receiveLineWithTxnQty.WE_TransactionQuantity);
			AssertEquals("Reserved Quantity", 15m, receiveLineWithTxnQty.ReservedQuantity);
		}

		#endregion

		#region TestRemappingReservedStock_ExpectedQuantityGreaterThanTransactionQuantity_WithOtherInventoryToRemapTo

		public void TestRemappingReservedStock_ExpectedQuantityGreaterThanTransactionQuantity_WithOtherInventoryToRemapTo()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLineWithNoTxnQty = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m);
			receiveLineWithNoTxnQty.WE_TransactionQuantity = 0m;
			var receiveLineWithTxnQty1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "PA1A", "", "", "");
			var receiveLineWithTxnQty2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 15m, ZDate.Empty, ZDate.Empty, "PA1", "", "", "");

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			Helper.CreateReservePickLine(orderLine, receiveLineWithNoTxnQty.Inventory[0], 20m);
			Factory.Save();

			Helper.CreateAsnLine(receive, data.Part1, 20m, 1, 0, "", ZDate.Empty, ZDate.Empty, "PA1", "", "", "");

			AssertEquals("Precondition", 20m, receiveLineWithNoTxnQty.ReservedQuantity);
			AssertEquals("Precondition", 20m, orderLine.WE_CrossDockQuantity);
			AssertEquals("Precondition", 3, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("Zero transaction qty line is deleted.", 2, receive.Lines.Count);
			AssertEquals("Expected Quantity", 0m, receiveLineWithTxnQty1.WE_ClientOrderedUnits);
			AssertEquals("Transaction Quantity", 10m, receiveLineWithTxnQty1.WE_TransactionQuantity);
			AssertEquals("Reserved Quantity", 5m, receiveLineWithTxnQty1.ReservedQuantity);

			AssertEquals("Expected Quantity", 20m, receiveLineWithTxnQty2.WE_ClientOrderedUnits);
			AssertEquals("Transaction Quantity", 15m, receiveLineWithTxnQty2.WE_TransactionQuantity);
			AssertEquals("Reserved Quantity", 15m, receiveLineWithTxnQty2.ReservedQuantity);
		}

		#endregion

		#region TestRemappingReservedStock_ReservedStockOnZeroTxnAndExpectedQtyLine_NoLinesToRemapTo

		public void TestRemappingReservedStock_ReservedStockOnZeroTxnAndExpectedQtyLine_NoLinesToRemapTo()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLineWithNoTxnQty = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "PA1", "", "", "");
			receiveLineWithNoTxnQty.WE_TransactionQuantity = 0m;
			var receiveLineWithTxnQty = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "PA1", "", "", "", "");
			Helper.CreateReservePickLine(orderLine, receiveLineWithNoTxnQty.Inventory[0], 10m);
			Factory.Save();

			Helper.CreateAsnLine(receive, data.Part1, 10m, 1, 0, "ABC", ZDate.Empty, ZDate.Empty, "", "", "", "");

			AssertEquals("Precondition", 10m, receiveLineWithNoTxnQty.ReservedQuantity);
			AssertEquals("Precondition", 10m, orderLine.WE_CrossDockQuantity);
			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("Zero transaction qty line is deleted but new inventory is created from the asn reconcilliation.", 3, receive.Lines.Count);
			AssertEquals("Reserved Quantity", 0m, receiveLineWithTxnQty.ReservedQuantity);

			var newReceiveLine = receive.Lines.Cast<WhsReceiveLine>().Single(line => line.WE_ClientOrderedUnits > 0);
			AssertEquals("Reserved Quantity", 0m, newReceiveLine.ReservedQuantity);

			AssertEquals("Reserved Quantity", 10m, receiveLineWithNoTxnQty.ReservedQuantity);
			AssertEquals(true, receiveLineWithNoTxnQty.HasErrors);
			AssertHasError(receiveLineWithNoTxnQty.WE_TransactionQuantityInfo, WhsValidationHelper.UnfulfilledCrossDockedUnitsError);
			AssertHasError(receiveLineWithNoTxnQty.WE_ClientOrderedUnitsInfo, WhsValidationHelper.UnfulfilledCrossDockedUnitsError);
		}

		#endregion

		#region TestRemappingReservedStock_ReservedStockWithAttributes_ExactAttributeMatch

		public void TestRemappingReservedStock_ReservedStockWithAttributes_ExactAttributeMatch()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "", "");
			line1.WE_LineNo = 0;
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "PA3", "");
			line2.WE_LineNo = 0;
			line2.WE_TransactionQuantity = 0;
			var line3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "PA3", "");
			line3.WE_LineNo = 0;

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "PA3", "", "");
			Helper.CreateReservePickLine(orderLine, line2.Inventory[0], 50m);
			Factory.Save();

			Helper.CreateAsnLine(receive, data.Part1, 30m, 1, 0, "", ZDate.Empty, ZDate.Empty, "PA1", "PA2", "PA3", "");

			AssertEquals("Precondition", 3, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("1 line deleted but 1 line added from reconcilliation split.", 3, receive.Lines.Count);
			AssertEquals("Receive line with no transaction quantity is deleted.", true, line2.IsDeleted);

			AssertEquals("Quantity on line3 split during reconcilliation.", 30m, line3.WE_TransactionQuantity);
			AssertEquals("Quantity on line3 split during reconcilliation.", 30m, line3.WE_ClientOrderedUnits);
			AssertEquals("Some reserved stock remapped to line3.", 30m, line3.ReservedQuantity);

			var newLine = receive.Lines.Single(line => line.WE_ClientOrderedUnits == 0m && line.WE_TransactionQuantity == 20m);
			AssertEquals("Some reserved stock remapped to new line.", 20m, newLine.ReservedQuantity);

			AssertEquals("Transaction Quantity", 50m, line1.WE_TransactionQuantity);
			AssertEquals("Expected Quantity", 0m, line1.WE_ClientOrderedUnits);
			AssertEquals("No reserved stock is remapped to line1.", 0m, line1.ReservedQuantity);
		}

		#endregion

		#region TestRemappingReservedStock_ReservedStockWithAttributes_NoAttributesSpecifiedOnOrderLine

		public void TestRemappingReservedStock_ReservedStockWithAttributes_NoAttributesSpecifiedOnOrderLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "", "");
			line1.WE_LineNo = 0;
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "PA3", "");
			line2.WE_LineNo = 0;
			line2.WE_TransactionQuantity = 0;

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m);
			Helper.CreateReservePickLine(orderLine, line2.Inventory[0], 50m);
			Factory.Save();

			Helper.CreateAsnLine(receive, data.Part1, 50m, 1, 0, "", ZDate.Empty, ZDate.Empty, "PA1", "PA2", "", "");

			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("Line with no transaction quantity gets deleted.", 1, receive.Lines.Count);
			AssertEquals("Receive line with no transaction quantity is deleted.", true, line2.IsDeleted);

			AssertEquals("Transaction Quantity", 50m, line1.WE_TransactionQuantity);
			AssertEquals("Expected Quantity", 50m, line1.WE_ClientOrderedUnits);
			AssertEquals("Reserved stock is remapped to line1 as the order line has no attributes and could match to line1.", 50m, line1.ReservedQuantity);
		}

		#endregion

		#region TestRemappingReservedStock_ReservedStockWithAttributes_LeastNumberOfAttributesGetsMatchedFirst

		public void TestRemappingReservedStock_ReservedStockWithAttributes_LeastNumberOfAttributesGetsMatchedFirst()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, ZDate.Empty, ZDate.Empty, "PA1", "", "", "");
			line1.WE_LineNo = 0;
			line1.WE_TransactionQuantity = 0;
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 40m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "", "");
			line2.WE_LineNo = 0;
			var line3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "PA3", "");
			line3.WE_LineNo = 0;

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m, ZDate.Empty, ZDate.Empty, "PA1", "", "", "", "");
			Helper.CreateReservePickLine(orderLine, line1.Inventory[0], 50m);
			Factory.Save();

			Helper.CreateAsnLine(receive, data.Part1, 50m, 1, 0, "", ZDate.Empty, ZDate.Empty, "PA1", "PA2", "PA3", "");

			AssertEquals("Precondition", 3, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("1 line deleted from reconcilliation.", 2, receive.Lines.Count);
			AssertEquals("Receive line with no transaction quantity is deleted.", true, line1.IsDeleted);

			AssertEquals("Some reserved stock is remapped to line2.", 40m, line2.ReservedQuantity);
			AssertEquals("Some reserved stock remapped to line3.", 10m, line3.ReservedQuantity);
		}

		#endregion

		#region TestRemappingReservedStock_ReservedStockWithAttributes_ExactMatchGetsReservedStockFirst

		public void TestRemappingReservedStock_ReservedStockWithAttributes_ExactMatchGetsReservedStockFirst()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "", "");
			line1.WE_LineNo = 0;
			line1.WE_TransactionQuantity = 0;
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 40m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "", "");
			line2.WE_LineNo = 0;
			var line3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "PA3", "");
			line3.WE_LineNo = 0;

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "", "", "");
			Helper.CreateReservePickLine(orderLine, line1.Inventory[0], 50m);
			Factory.Save();

			Helper.CreateAsnLine(receive, data.Part1, 50m, 1, 0, "", ZDate.Empty, ZDate.Empty, "PA1", "PA2", "PA3", "");

			AssertEquals("Precondition", 3, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("1 line deleted from reconcilliation.", 2, receive.Lines.Count);
			AssertEquals("Receive line with no transaction quantity is deleted.", true, line1.IsDeleted);

			AssertEquals("Some reserved stock is remapped to line2.", 40m, line2.ReservedQuantity);
			AssertEquals("Some reserved stock remapped to line3.", 10m, line3.ReservedQuantity);
		}

		#endregion

		#region TestRemappingReservedStock_ReservedStockWithAttributes_SerialNumberMatch

		public void TestRemappingReservedStock_ReservedStockWithAttributes_SerialNumberMatch()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "PA2", "", "");
			line1.WE_SerialNumber = "SN1";
			line1.WE_LineNo = 0;
			line1.WE_TransactionQuantity = 0;
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "PA2", "", "");
			line2.WE_SerialNumber = "SN1";
			line2.WE_LineNo = 0;
			var line3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "PA2", "", "");
			line3.WE_SerialNumber = "SN2";
			line3.WE_LineNo = 0;

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "PA2", "", "", "");
			orderLine.WE_SerialNumber = "SN1";
			Helper.CreateReservePickLine(orderLine, line1.Inventory[0], 1m);
			Factory.Save();

			Helper.CreateAsnLine(receive, data.Part1, 1m, 1, 0, "", ZDate.Empty, ZDate.Empty, "", "PA2", "", "SN2");

			AssertEquals("Precondition", 3, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("Receive line with 0 transaction quantity is deleted from reconcilliation.", 2, receive.Lines.Count);
			AssertEquals("Receive line with no transaction quantity is not deleted.", true, line1.IsDeleted);

			AssertEquals("Reserved stock is remapped to line2.", 1m, line2.ReservedQuantity);
			AssertEquals("No reserved stock is remapped to line3.", 0m, line3.ReservedQuantity);
		}

		public void TestRemappingReservedStock_ReservedStockWithAttributes_SerialNumberMatch_DoesNotIgnoreOtherAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "PA2", "PA3", "");
			line1.WE_SerialNumber = "SN1";
			line1.WE_LineNo = 0;
			line1.WE_TransactionQuantity = 0;
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "PA2", "", "");
			line2.WE_SerialNumber = "SN1";
			line2.WE_LineNo = 0;
			var line3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "PA2", "PA3", "");
			line3.WE_SerialNumber = "SN2";
			line3.WE_LineNo = 0;

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "PA2", "PA3", "", "");
			orderLine.WE_SerialNumber = "SN1";
			Helper.CreateReservePickLine(orderLine, line1.Inventory[0], 1m);
			Factory.Save();

			Helper.CreateAsnLine(receive, data.Part1, 1m, 1, 0, "", ZDate.Empty, ZDate.Empty, "", "PA2", "PA3", "SN2");

			AssertEquals("Precondition", 3, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("No line is deleted from reconcilliation.", 3, receive.Lines.Count);
			AssertEquals("Receive line with no transaction quantity is not deleted.", false, line1.IsDeleted);

			AssertEquals("Reserved stock stays with line1.", 1m, line1.ReservedQuantity);
			AssertHasError(line1.WE_TransactionQuantityInfo, WhsValidationHelper.UnfulfilledCrossDockedUnitsError);
			AssertHasError(line1.WE_ClientOrderedUnitsInfo, WhsValidationHelper.UnfulfilledCrossDockedUnitsError);

			AssertEquals("No reserved stock is remapped to line2.", 0m, line2.ReservedQuantity);
			AssertEquals("No reserved stock is remapped to line3.", 0m, line3.ReservedQuantity);
		}

		#endregion

		#region TestRemappingReservedStock_ReservedStockWithAttributes_PackingDateMatch

		public void TestRemappingReservedStock_ReservedStockWithAttributes_PackingDateMatch()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);

			var matchingDate = ZDate.Today;
			var mismatchedDate = ZDate.Today.AddDays(-1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, ZDate.Empty, matchingDate, "", "", "", "");
			line1.WE_LineNo = 0;
			line1.WE_TransactionQuantity = 0;
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, ZDate.Empty, matchingDate, "", "", "", "");
			var line3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, ZDate.Empty, mismatchedDate, "", "", "", "");

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m, ZDate.Empty, matchingDate, "", "", "", "", "");
			Helper.CreateReservePickLine(orderLine, line1.Inventory[0], 1m);
			Factory.Save();

			Helper.CreateAsnLine(receive, data.Part1, 1m, 1, 0, "", matchingDate, ZDate.Empty, "", "", "", "");

			AssertEquals("Precondition", 3, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("Inventory with 0 transaction quantity is deleted.", 2, receive.Lines.Count);
			AssertEquals("Receive line with no transaction quantity is deleted.", true, line1.IsDeleted);

			AssertEquals("Reserved stock is remapped to line2.", 1m, line2.ReservedQuantity);
			AssertEquals("No reserved stock remapped to line3.", 0m, line3.ReservedQuantity);
		}

		#endregion

		#region TestRemappingReservedStock_ReservedStockWithAttributes_ExpiryDateMatch

		public void TestRemappingReservedStock_ReservedStockWithAttributes_ExpiryDateMatch()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			var matchingDate = ZDate.Today;
			var mismatchedDate = ZDate.Today.AddDays(-1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, matchingDate, ZDate.Empty, "", "", "", "");
			line1.WE_LineNo = 0;
			line1.WE_TransactionQuantity = 0;
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, matchingDate, ZDate.Empty, "", "", "", "");
			var line3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, mismatchedDate, ZDate.Empty, "", "", "", "");

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m, matchingDate, ZDate.Empty, "", "", "", "", "");
			Helper.CreateReservePickLine(orderLine, line1.Inventory[0], 1m);
			Factory.Save();

			Helper.CreateAsnLine(receive, data.Part1, 1m, 1, 0, "", ZDate.Empty, matchingDate, "", "", "", "");

			AssertEquals("Precondition", 3, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("Inventory with 0 transaction quantity is deleted.", 2, receive.Lines.Count);
			AssertEquals("Receive line with no transaction quantity is deleted.", true, line1.IsDeleted);

			AssertEquals("Reserved stock is remapped to line2.", 1m, line2.ReservedQuantity);
			AssertEquals("No reserved stock remapped to line3.", 0m, line3.ReservedQuantity);
		}

		#endregion

		#region TestRemappingReservedStock_ReservedStockWithAttributes_PalletIdDoesNotNeedToMatch

		public void TestRemappingReservedStock_ReservedStockWithAttributes_PalletIdDoesNotNeedToMatch()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLineWithNoTxnQty = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, null, "ABC123");
			receiveLineWithNoTxnQty.WE_TransactionQuantity = 0m;
			var receiveLineWithTxnQty = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, null, "DEF456");

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateReservePickLine(orderLine, receiveLineWithNoTxnQty.Inventory[0], 10m);
			Factory.Save();

			Helper.CreateAsnLine(receive, data.Part1, 10m, 1, 0, "ABC123", ZDate.Empty, ZDate.Empty, "", "", "", "");

			AssertEquals("Precondition", 10m, receiveLineWithNoTxnQty.ReservedQuantity);
			AssertEquals("Precondition", 0m, receiveLineWithTxnQty.ReservedQuantity);
			AssertEquals("Precondition", 10m, orderLine.WE_CrossDockQuantity);
			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("Inventory with 0 transaction quantity is deleted but new line is created from unmatched asn line.", 2, receive.Lines.Count);
			AssertEquals("Reserved stock is remapped to receive line with transaction quantity even though pallet id does not match.", 10m, receiveLineWithTxnQty.ReservedQuantity);
		}

		#endregion

		#region TestRemappingReservedStock_MultipleReservedStocks_RemappedToSameInventory

		public void TestRemappingReservedStock_MultipleReservedStocks_RemappedToSameInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLineWithNoTxnQty1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 15m);
			receiveLineWithNoTxnQty1.WE_TransactionQuantity = 0m;
			var receiveLineWithNoTxnQty2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			receiveLineWithNoTxnQty2.WE_TransactionQuantity = 0m;
			var receiveLineWithTxnQty = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Helper.CreateReservePickLine(orderLine1, receiveLineWithNoTxnQty1.Inventory[0], 15m);
			Helper.CreateReservePickLine(orderLine2, receiveLineWithNoTxnQty2.Inventory[0], 5m);
			Factory.Save();

			Helper.CreateAsnLine(receive, data.Part1, 20m, 1, 0, "", ZDate.Empty, ZDate.Empty, "", "", "", "");

			AssertEquals("Precondition", 15m, receiveLineWithNoTxnQty1.ReservedQuantity);
			AssertEquals("Precondition", 5m, receiveLineWithNoTxnQty2.ReservedQuantity);
			AssertEquals("Precondition", 0m, receiveLineWithTxnQty.ReservedQuantity);
			AssertEquals("Precondition", 15m, orderLine1.WE_CrossDockQuantity);
			AssertEquals("Precondition", 5m, orderLine2.WE_CrossDockQuantity);
			AssertEquals("Precondition", 3, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("Inventories with 0 transaction quantity is deleted.", 1, receive.Lines.Count);
			AssertEquals("Receive line with no transaction quantity is deleted.", true, receiveLineWithNoTxnQty1.IsDeleted);
			AssertEquals("Receive line with no transaction quantity is deleted.", true, receiveLineWithNoTxnQty2.IsDeleted);
			AssertEquals("Reserved stocks is remapped to receive line with transaction quantity.", 20m, receiveLineWithTxnQty.ReservedQuantity);
		}

		#endregion

		#region TestRemappingReservedStock_MultipleReservedStocks_ReservedStockWithMoreAttributesGetsRemappedFirst

		public void TestRemappingReservedStock_MultipleReservedStocks_ReservedStockWithMoreAttributesGetsRemappedFirst()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLineWithNoTxnQty1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "PA1", "", "", "");
			receiveLineWithNoTxnQty1.WE_TransactionQuantity = 0m;
			var receiveLineWithNoTxnQty2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m);
			receiveLineWithNoTxnQty2.WE_TransactionQuantity = 0m;
			var receiveLineWithTxnQty1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "", "");
			var receiveLineWithTxnQty2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 30m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "PA3", "");

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "PA1", "", "", "", "");
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			Helper.CreateReservePickLine(orderLine1, receiveLineWithNoTxnQty1.Inventory[0], 10m);
			Helper.CreateReservePickLine(orderLine2, receiveLineWithNoTxnQty2.Inventory[0], 20m);
			Factory.Save();

			Helper.CreateAsnLine(receive, data.Part1, 20m, 1, 0, "", ZDate.Empty, ZDate.Empty, "", "", "", "");

			AssertEquals("Precondition", 10m, receiveLineWithNoTxnQty1.ReservedQuantity);
			AssertEquals("Precondition", 20m, receiveLineWithNoTxnQty2.ReservedQuantity);
			AssertEquals("Precondition", 0m, receiveLineWithTxnQty1.ReservedQuantity);
			AssertEquals("Precondition", 0m, receiveLineWithTxnQty2.ReservedQuantity);
			AssertEquals("Precondition", 10m, orderLine1.WE_CrossDockQuantity);
			AssertEquals("Precondition", 20m, orderLine2.WE_CrossDockQuantity);
			AssertEquals("Precondition", 4, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("Inventories with 0 transaction quantity is deleted.", 2, receive.Lines.Count);
			AssertEquals("Receive line with no transaction quantity is deleted.", true, receiveLineWithNoTxnQty1.IsDeleted);
			AssertEquals("Receive line with no transaction quantity is deleted.", true, receiveLineWithNoTxnQty2.IsDeleted);

			AssertEquals("Reserved stocks is remapped to receive lines with transaction quantity.", 20m, receiveLineWithTxnQty1.ReservedQuantity);
			AssertEquals("Reserved stocks is remapped to receive lines with transaction quantity.", 2, receiveLineWithTxnQty1.ReservedPickLines.Count);
			AssertEquals("Reserved stocks is remapped to receive lines with transaction quantity.", 10m, receiveLineWithTxnQty2.ReservedQuantity);
			AssertEquals("Reserved stocks is remapped to receive lines with transaction quantity.", 1, receiveLineWithTxnQty2.ReservedPickLines.Count);
		}

		#endregion

		#region TestRemappingReservedStock_MultipleReservedStocks_ReservedStockWithGreaterQtyGetsRemappedFirst

		public void TestRemappingReservedStock_MultipleReservedStocks_ReservedStockWithGreaterQtyGetsRemappedFirst()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLineWithNoTxnQty1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "PA1", "", "", "");
			receiveLineWithNoTxnQty1.WE_TransactionQuantity = 0m;
			var receiveLineWithNoTxnQty2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, ZDate.Empty, ZDate.Empty, "PA1", "", "", "");
			receiveLineWithNoTxnQty2.WE_TransactionQuantity = 0m;
			var receiveLineWithTxnQty1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "", "");
			var receiveLineWithTxnQty2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 30m, ZDate.Empty, ZDate.Empty, "PA1", "PA2", "PA3", "");

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "PA1", "", "", "", "");
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 20m, ZDate.Empty, ZDate.Empty, "PA1", "", "", "", "");
			Helper.CreateReservePickLine(orderLine1, receiveLineWithNoTxnQty1.Inventory[0], 10m);
			Helper.CreateReservePickLine(orderLine2, receiveLineWithNoTxnQty2.Inventory[0], 20m);
			Factory.Save();

			Helper.CreateAsnLine(receive, data.Part1, 20m, 1, 0, "", ZDate.Empty, ZDate.Empty, "", "", "", "");

			AssertEquals("Precondition", 10m, receiveLineWithNoTxnQty1.ReservedQuantity);
			AssertEquals("Precondition", 20m, receiveLineWithNoTxnQty2.ReservedQuantity);
			AssertEquals("Precondition", 0m, receiveLineWithTxnQty1.ReservedQuantity);
			AssertEquals("Precondition", 0m, receiveLineWithTxnQty2.ReservedQuantity);
			AssertEquals("Precondition", 10m, orderLine1.WE_CrossDockQuantity);
			AssertEquals("Precondition", 20m, orderLine2.WE_CrossDockQuantity);
			AssertEquals("Precondition", 4, receive.Lines.Count);
			AssertEquals("Precondition", 1, receive.AsnLines.Count);
			WhsReceiveASNHelper.ReconcileASN(receive);

			AssertEquals("Inventories with 0 transaction quantity is deleted.", 2, receive.Lines.Count);
			AssertEquals("Receive line with no transaction quantity is deleted.", true, receiveLineWithNoTxnQty1.IsDeleted);
			AssertEquals("Receive line with no transaction quantity is deleted.", true, receiveLineWithNoTxnQty2.IsDeleted);

			AssertEquals("Reserved stocks is remapped to receive lines with transaction quantity.", 20m, receiveLineWithTxnQty1.ReservedQuantity);
			AssertEquals("Reserved stocks is remapped to receive lines with transaction quantity.", 1, receiveLineWithTxnQty1.ReservedPickLines.Count);
			AssertEquals("Reserved stocks is remapped to receive lines with transaction quantity.", 10m, receiveLineWithTxnQty2.ReservedQuantity);
			AssertEquals("Reserved stocks is remapped to receive lines with transaction quantity.", 1, receiveLineWithTxnQty2.ReservedPickLines.Count);
		}

		#endregion

		#endregion

		#region TestDeleteBOMLinksForPartialReturnedLines

		public void TestDeleteBOMLinksForPartialReturnedLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory, "Attr1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Attr2");

			var receiveForComponents = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receiveForComponents, data.Part1, 30m);
			Helper.CreateWhsReceiveInventoryLine(receiveForComponents, data.Part2, 20m);
			receiveForComponents.AllocateLocationsWithMock();
			receiveForComponents.FinaliseDocketWithoutUserConfirmation();

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			part3.OP_IsComponentPickedOnSalesOrder = true;
			Helper.CreateProductBOM(part3, data.Part1, 1m, "UNT");
			Helper.CreateProductBOM(part3, data.Part2, 1m, "UNT");

			var receiveForPart3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			Helper.CreateWhsReceiveInventoryLine(receiveForPart3, part3, 1m);
			receiveForPart3.AllocateLocationsWithMock();
			receiveForPart3.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "work order 1");
			Helper.CreateWhsWorkOrderLine(workOrder, part3, 1m);
			Helper.CreatePickNew(workOrder);

			workOrder.FinaliseDocketAlwaysFinalisingPick();
			workOrder.Receive.FinaliseDocketWithoutUserConfirmation();

			Helper.Factory.Save();

			var receiveLine = workOrder.Receive.Lines[0];
			AssertEquals(receiveLine.WE_TransactionQuantity, receiveLine.WE_ClientOrderedUnits);
			Assert(receiveLine.BOMComponentLinks.Any());

			receiveLine.WE_TransactionQuantity = 20m;

			Helper.CreateAsnLine(workOrder.Receive, part3, 2m, 1, 0, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			WhsReceiveASNHelper.ReconcileASN(workOrder.Receive);

			AssertEquals("BOMLinks should be deleted when WE_ClientOrderedUnits ！= WE_TransactionQuantity", false, receiveLine.BOMComponentLinks.Any());
		}

		#endregion
	}
}
