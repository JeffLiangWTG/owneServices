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
	[TestedType(typeof(WhsTransfer))]
	internal class WhsTransfer_OldTest : WhsBusinessObjectTestCase
	{
		#region Metadata

		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.Business.WhsTransfer);
			}
		}

		#endregion

		public void TestCreateAndSaveState()
		{
			// test create state
			WhsTransfer docket = (WhsTransfer)GetNewBusinessObject();
			AssertEquals("Docket status must be 'New'", DocketStatus.Codes.New, docket.WD_DocketStatus);
			AssertEquals("Docket type must be 'Transfer'", DocketType.Codes.Transfer, docket.WD_DocketType);
			Assert("Booking Date is null because warehouse was not set.", docket.WD_BookingDate == ZDateTimeOffset.Empty);
			AssertEquals("Reference = 'TRANSFER'", "TRANSFER", docket.WD_ExternalReference);

			WhsWarehouse whs = Helper.CreateWarehouse("AAAA");
			OrgHeader org = Helper.CreateClient();
			docket.WD_OH_Client = org.PK;
			docket.WD_WW_Whs = whs.PK;
			Assert("Booking Date is set.", docket.WD_BookingDate != ZDateTimeOffset.Empty);
			Factory.Save();

			// test saved state
			Assert("Auto Log must be created", docket.Logs.GetAllLogs().Count > 0);
		}

		#region Finalisation Tests

		#region Tests for conditions which stop finalise

		public void TestFinaliseConfirmationCancelAbortsFinalise()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "1", Notify);
			var line1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");

			Notify.DefaultResponse = false;  // similate user clicking cancel
			transfer.FinaliseDocket();
			AssertEquals("Docket should not be finalised", false, transfer.IsFinalised);
			AssertEquals("Finalization Confirmation", ((QueryUserMsgBoxEventArgs)Notify.LastQueryUserEventArgs).Caption);
		}

		#endregion

		#region Finalise

		public void TestFinaliseDocket()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);

			// create Receive docket for inventory
			// backdate arrival date to test if arrival date is carried into new transfer inventory
			var backDate = ZDateTimeOffset.Today.AddDays(-1);
			var receive1 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "1", backDate, Notify);
			var receive1Line1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 20, data.Whs1.FindLocation("A-1-1"));
			var receive1Line2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2, 20, data.Whs1.FindLocation("A-1-2"));
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			// warehouse2 is used for multi-warehouse testing
			// that is, ensure stock is not picked from the wrong warehouse
			var whs2 = Helper.CreateWarehouse("2", "A", 2, 2);
			Factory.Save();

			var receive2 = Helper.CreateWhsReceive(data.Org1, whs2, "2", Notify);
			var receive2Line1 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 20, whs2.FindLocation("A-1-1"));
			var receive2Line2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 20, whs2.FindLocation("A-1-2"));
			receive2.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive2);

			Factory.Save();

			// construct transfer
			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer1, data.Part1.PK, 10, "A-1-1", "A-2-1");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer1, data.Part2.PK, 30, "A-1-2", "A-2-2");
			transfer1.FinaliseDocket();

			// finalisation should fail because there is not enought stock
			Assert("Transfer should haver errors", transfer1.HasErrors);
			Assert("Transfer should NOT be finalised", !transfer1.IsFinalised);

			// now fix the line so docket can be finalised
			transferLine2.WE_TransactionQuantity = 20;
			transfer1.FinaliseDocket();
			Factory.Save();
			Assert("Transfer should NOT haver errors", !transfer1.HasErrors);
			Assert("Transfer should be finalised", transfer1.IsFinalised);
			Assert("Transfer1 date should be set", !transfer1.WD_FinalisedDate.IsEmpty);

			var inventories1 = Helper.LoadInventory(receive1Line1.InDocketLine);
			AssertEquals("Stock not correct for Part1 Locn A-1-1", 10m, inventories1.UnitsAvailable);

			var inventories2 = Helper.LoadInventory(receive1Line2.InDocketLine);
			AssertEquals("Stock not correct for Part2 Locn A-1-2", 0m, inventories2.UnitsAvailable);

			var inventories3 = Helper.LoadInventory(data.Org1, receive1Line1.InDocketLine.SupplierPart, transferLine1.Location);
			AssertEquals("Stock not correct for Part1 Locn A-2-1", 10m, inventories3.UnitsAvailable);
			AssertEquals("Arrival Date must be carried from Locn A-1-1 to A-2-1", backDate, inventories3.Inventory.First().WI_ArrivalDate);

			var inventories4 = Helper.LoadInventory(data.Org1, receive1Line2.InDocketLine.SupplierPart, transferLine2.Location);
			AssertEquals("Stock not correct for Part2 Locn A-2-2", 20m, inventories4.UnitsAvailable);
			AssertEquals("Arrival Date must be carried from Locn A-1-2 to A-2-2", backDate, inventories4.Inventory.First().WI_ArrivalDate);
		}

		public void TestTransferInWithAttributes()
		{
			var data = new TestDataForInventory(Factory, Notify);
			data.CreateMultiWarehouseClientProductInventory();

			var fromLocation = data.LineNonBonded.Location;
			var toLocation = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations[2];

			var transfer = Helper.CreateWhsTransfer(data.Org2, data.Whs1, "1", data.Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part2, 30, fromLocation, toLocation);
			Helper.SetDocketLineAttributes(transferLine, ZDate.Today.AddMonths(1), ZDate.Today.AddMonths(-1), "PA1Non", "PA2Non", "PANon", "");

			var preTransferInventories = Helper.LoadInventory(transferLine, fromLocation);
			AssertEquals("Precondition Stock Level", 100m, preTransferInventories.UnitsAvailable);

			transfer.FinaliseDocket();
			AssertEquals("Transfer could not be finalised", true, transfer.IsFinalised);

			Factory.Save(); // need to save as finalise uses a 2nd factory

			var postTransferSourceInventories = Helper.LoadInventory(transferLine, fromLocation);
			AssertEquals("Stock Level after Transfer (Source)", 70m, postTransferSourceInventories.UnitsAvailable);

			var postTransferDestInventories = Helper.LoadInventory(transferLine, toLocation);
			AssertEquals("Stock Level after Transfer (Dest)", 30m, postTransferDestInventories.UnitsAvailable);
		}

		public void TestRollback()
		{
			Assert("incomplete test", true);
		}

		#endregion

		#endregion
	}
}
