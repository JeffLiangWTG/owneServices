using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsAdjustment))]
	internal class WhsAdjustment_OldTest : WhsBusinessObjectTestCase
	{
		#region Metadata

		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.Business.WhsDocket);
			}
		}

		#endregion

		public void TestLines()
		{
			Docket = Factory.New<WhsAdjustment>();
			AssertNotNull(Docket.Lines);
		}

		public void TestCreateAndSaveState()
		{
			// test create state
			WhsAdjustment docket = (WhsAdjustment)GetNewBusinessObject();
			AssertEquals("Docket status must be 'New'", CodeLists.DocketStatus.Codes.New, docket.WD_DocketStatus);
			AssertEquals("Docket type must be CodeLists.DocketType.Codes.Adjustment", CodeLists.DocketType.Codes.Adjustment, docket.WD_DocketType);
			Assert("Booking Date is null because warehouse was not set.", docket.WD_BookingDate == ZDateTimeOffset.Empty);
			AssertEquals("Reference = 'ADJUSTMENT'", "ADJUSTMENT", docket.WD_ExternalReference);

			WhsWarehouse whs = Helper.CreateWarehouse("AAAA");
			OrgHeader org = Helper.CreateClient();
			docket.WD_OH_Client = org.PK;
			docket.WD_WW_Whs = whs.PK;
			Assert("Booking Date is set.", docket.WD_BookingDate != ZDateTimeOffset.Empty);

			Factory.Save();
			Assert("Auto Log must be created", docket.Logs.GetAllLogs().Count > 0);
		}

		#region Finalisation Tests

		#region Tests for conditions which stop finalise

		public void TestFinaliseConfirmationCancelAbortsFinalise()
		{
			SetupAdjustment();
			var row1A = Helper.CreateRowAndGenerateLocations(Whs1, "A", 1, 1);
			Factory.Save();
			var line = Docket.Lines.AddNew();
			line.WE_OP = Part1.PK;
			line.WE_TransactionQuantity = 10;
			line.LocationString = "A";
			line.WE_LineComment = "TEST";
			line.WE_ReasonCode = "CLI";
			line.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;

			Notify.DefaultResponse = false;  // similate user clicking cancel
			Docket.FinaliseDocket();
			AssertEquals("Docket should not be finalised", false, Docket.IsFinalised);
			AssertEquals("Finalization Confirmation", ((QueryUserMsgBoxEventArgs)Notify.LastQueryUserEventArgs).Caption);
		}

		#endregion

		#region Finalise

		public void TestAdustment()
		{
			var prod = new OrgSupplierPart[1];
			var adjLine = new WhsAdjustmentLine[1];

			var whs = Helper.CreateWarehouse("1");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 2, 2);

			var org = Helper.CreateClient();
			prod[0] = Helper.CreateProduct(org, "AA1");
			Factory.Save();

			var docket = (WhsAdjustment)GetNewBusinessObject();
			docket.WD_OH_Client = org.PK;
			docket.WD_WW_Whs = whs.PK;
			docket.WD_ExternalReference = "AA1";
			docket.NotificationManager.Push(Notify);

			adjLine[0] = docket.Lines.AddNew();
			adjLine[0].WE_OP = prod[0].PK;
			adjLine[0].WE_TransactionQuantity = 20;
			adjLine[0].LocationString = "A-1-1";
			adjLine[0].WE_LineComment = "TEST";
			adjLine[0].WE_ReasonCode = "CLI";
			adjLine[0].WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;

			docket.FinaliseDocket();

			Assert("Adjustment should NOT haver errors", !docket.HasErrors);
			Assert("Adjustment should be finalised", docket.IsFinalised);
			Assert("Finalised date should be set", !docket.WD_FinalisedDate.IsEmpty);

			Factory.Save(); // need to save as finalise uses a 2nd factory

			// now check we have units in stock
			var inventories = Helper.LoadInventory(null, null, row.Locations[0]);
			AssertEquals("Correct stock on hand", 20.0m, inventories.UnitsTotal);

			// now do a negative adjustment
			docket = (WhsAdjustment)GetNewBusinessObject();
			docket.WD_OH_Client = org.PK;
			docket.WD_WW_Whs = whs.PK;
			docket.WD_ExternalReference = "AA2";
			docket.NotificationManager.Push(Notify);

			adjLine[0] = docket.Lines.AddNew();
			adjLine[0].WE_OP = prod[0].PK;
			adjLine[0].WE_TransactionQuantity = -30;
			adjLine[0].LocationString = "A-1-1";
			adjLine[0].WE_LineComment = "TEST";
			adjLine[0].WE_ReasonCode = "CLI";
			adjLine[0].WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;

			docket.FinaliseDocket();

			// finalisation should fail because there is not enuf stock
			Assert("Adjustment should haver errors", docket.HasErrors);
			Assert("Adjustment should NOT be finalised", !docket.IsFinalised);

			// now fix the line so docket can be finalised
			adjLine[0].WE_TransactionQuantity = -10;
			docket.ClearAllNotifications();
			docket.FinaliseDocket();

			Assert("Adjustment should NOT haver errors", !docket.HasErrors);
			Assert("Adjustment should be finalised", docket.IsFinalised);
			Assert("Finalised date should be set", !docket.WD_FinalisedDate.IsEmpty);

			Factory.Save(); // need to save as finalise uses a 2nd factory

			// now check we have 10 units in stock
			AssertEquals("Correct stock on hand", 10.0m, inventories.UnitsTotal);
		}

		public void TestAdjustmentInWithAttributes()
		{
			TestDataForInventory data = new TestDataForInventory(Factory, Notify);
			data.CreateMultiWarehouseClientProductInventory();

			var location = data.Whs1.Areas.Single(a => a.WA_AreaType == "FRE").PutawayLocations[0];
			var adjustment = Helper.CreateWhsAdjustment(data.Org2, data.Whs1, "1", data.Notify);
			var line1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part2, 30, data.LineNonBonded.Location);
			Helper.SetDocketLineAttributes(line1, ZDate.Today.AddMonths(1), ZDate.Today.AddMonths(-1), "PA1Non", "PA2Non", "PANon", "");

			var preAdjustmentInventories = Helper.LoadInventory(line1);
			AssertEquals("Precondition Stock Level", 100m, preAdjustmentInventories.UnitsAvailable);

			adjustment.FinaliseDocket();
			AssertEquals("Adjustment could not be finalised", true, adjustment.IsFinalised);

			Factory.Save(); // need to save as finalise uses a 2nd factory

			var postAdjustmentInventories = Helper.LoadInventory(line1);
			AssertEquals("Stock Level after adjustment", 130m, postAdjustmentInventories.UnitsAvailable);
		}

		public void TestAdjustmentOutWithAttributes()
		{
			TestDataForInventory data = new TestDataForInventory(Factory, Notify);
			data.CreateMultiWarehouseClientProductInventory(typeOfStockToCreate: TestDataForInventory.StockType.WithoutBondEntryKeys);

			var adjustment = Helper.CreateWhsAdjustment(data.Org2, data.Whs1, "1", data.Notify);
			var line1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part2, -30, data.Line215.Location);
			Helper.SetDocketLineAttributes(line1, ZDate.Today.AddMonths(1), ZDate.Today.AddMonths(-1), "PA12", "PA2", "PA3", "");

			var preAdjustmentInventories = Helper.LoadInventory(line1);
			AssertEquals("Precondition Stock Level", 100m, preAdjustmentInventories.UnitsAvailable);

			adjustment.FinaliseDocket();
			AssertEquals("Adjustment should be finalised", true, adjustment.IsFinalised);

			Factory.Save(); // need to save as finalise uses a 2nd factory

			var postAdjustmentInventories = Helper.LoadInventory(line1);
			AssertEquals("Stock Level after adjustment", 70m, postAdjustmentInventories.UnitsAvailable);
		}

		public void TestAdjustmentOutWithAttributes_WithBondEntryKey()
		{
			var data = new TestDataForInventory(Factory, Notify);
			data.CreateMultiWarehouseClientProductInventory();
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var adjustment = Helper.CreateWhsAdjustment(data.Org2, data.Whs1, "1", data.Notify);
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;
			var line1 = Helper.CreateWhsAdjustmentLine(adjustment, data.Part2, -30, data.Line215.Location);
			Helper.SetDocketLineAttributes(line1, ZDate.Today.AddMonths(1), ZDate.Today.AddMonths(-1), "PA12", "PA2", "PA3", "", "BEK21-3");

			var preAdjustmentInventories = Helper.LoadInventory(line1);
			AssertEquals("Precondition Stock Level", 100m, preAdjustmentInventories.UnitsAvailable);

			adjustment.FinaliseDocket();
			AssertEquals("Adjustment should be finalised", true, adjustment.IsFinalised);

			Factory.Save(); // need to save as finalise uses a 2nd factory

			var postAdjustmentInventories = Helper.LoadInventory(line1);
			AssertEquals("Stock Level after adjustment", 70m, postAdjustmentInventories.UnitsAvailable);
		}

		#endregion

		#endregion

		#region Implementation

		protected void SetupAdjustment()
		{
			// create environment
			Whs1 = Helper.CreateWarehouse("1");

			// create product
			Org = Helper.CreateClient();
			Part1 = Helper.CreateProduct(Org, "AAA");

			// create receive docket
			Docket = Factory.New<WhsAdjustment>();
			Docket.WD_OH_Client = Org.PK;
			Docket.WD_WW_Whs = Whs1.PK;
			Docket.WD_ExternalReference = "TEST";
			Docket.NotificationManager.Push(Notify);
		}

		WhsWarehouse Whs1;
		OrgHeader Org;
		WhsAdjustment Docket;
		OrgSupplierPart Part1;

		#endregion
	}
}
