using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsInvoiceInventoryTest : WhsTestCaseWithFactory
	{
		#region TestView_Transfers

		public void TestView_Transfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m,
				data.Whs1.FindLocation("A-1"), "P01");

			var transferUnfinalized = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			Helper.CreateWhsTransferLine(transferUnfinalized, data.Part1, 10m, "A-1", "P01", "A-2", "P02");
			transferUnfinalized.RunPreSaveValidation(); // to commit inventory

			var transferWithFinalizedLine = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify);
			var transferLineFinalised = Helper.CreateWhsTransferLine(transferWithFinalizedLine, data.Part1, 15m, "A-1",
				"P01", "A-3", "P03");
			transferLineFinalised.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLineFinalised);

			var transferFinalized = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR3", Notify);
			var transferLineFromFinalisedTransfer =
				Helper.CreateWhsTransferLine(transferFinalized, data.Part1, 20m, "A-1", "P01", "A-4", "P04");
			transferFinalized.FinaliseDocket();
			AssertIsFinalisedPrecondition(transferFinalized);

			Factory.Save();

			var result = LoadSQLFunction();
			AssertEquals("Inventory should be split between 3 locations.", 3, result.Length);
			AssertLineMatch(receive.Lines[0], 65m, result);
			AssertLineMatch(transferLineFinalised, 15m, result);
			AssertLineMatch(transferLineFromFinalisedTransfer, 20m, result);
		}

		public void TestView_Transfers_Attributes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var loc1 = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = CreateReceiveLine("SERN1");
			var receiveLine2 = CreateReceiveLine("SERN2");
			CreateReceiveLine("SERN3");
			CreateReceiveLine("SERN4");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var transferUnfinalized = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			CreateTransferLine(transferUnfinalized, "A-2", "P02", "SERN2");
			transferUnfinalized.RunPreSaveValidation(); // to commit inventory

			var transferWithFinalizedLine = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify);
			var transferLineFinalised = CreateTransferLine(transferWithFinalizedLine, "A-3", "P03", "SERN3");
			transferLineFinalised.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLineFinalised);

			var transferFinalized = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR3", Notify);
			var transferLineFromFinalisedTransfer = CreateTransferLine(transferFinalized, "A-4", "P04", "SERN4");
			transferFinalized.FinaliseDocket();
			AssertIsFinalisedPrecondition(transferFinalized);
			Factory.Save();

			var result = LoadSQLFunction();
			AssertEquals("Inventory should be split between 4 locations and serial number combinations.", 4,
				result.Length);
			AssertLineMatch(receiveLine1, 1m, result);
			AssertLineMatch(receiveLine2, 1m, result);
			AssertLineMatch(transferLineFinalised, 1m, result);
			AssertLineMatch(transferLineFromFinalisedTransfer, 1m, result);

			WhsReceiveLine CreateReceiveLine(string serialNumber)
			{
				var line = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, loc1, "P01");
				line.WE_PartAttrib1 = "PA1";
				line.WE_PartAttrib1 = "PA2";
				line.WE_PartAttrib1 = "PA3";
				line.WE_SerialNumber = serialNumber;

				return line;
			}

			WhsTransferLine CreateTransferLine(WhsTransfer transfer, string destinationLocString,
				string destinationPIDString, string serialNumber)
			{
				var line = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A-1", "P01", destinationLocString,
					destinationPIDString);
				line.WE_PartAttrib1 = "PA1";
				line.WE_PartAttrib1 = "PA2";
				line.WE_PartAttrib1 = "PA3";
				line.WE_SerialNumber = serialNumber;

				return line;
			}
		}

		#endregion

		#region TestView_FinalisedDockDoorTransfer

		public void TestView_FinalisedDockDoorTransfer()
		{
			var year = ZDateTime.Now.Year;

			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var location = data.Whs1.DefaultLocation;
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 1);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 8m);
			var pick = Helper.CreatePickNew(order);

			// Pick the line / create In-Transit Transfer
			var pickLine = pick.GetAllPickLines().Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, new ZDateTimeOffset(year, 1, 2));
			Factory.Save();

			// Finalise transfer, pick/finalise the dock door stock
			transferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine);
			Factory.Save();

			// InvoiceInventory should not show stock created in the dock door as for StockBalances and Rating purposes, we currently pretend the old datashape exists
			var result = LoadSQLFunction();
			AssertEquals("Inventory from dock door transfer should not be shown in the report.", 1, result.Length);
			AssertLineMatch(receive.Lines[0], 12m, result);
		}

		#endregion

		#region TestView_SerialNumber

		public void TestView_SerialNumber()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var location = data.Whs1.DefaultLocation;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1.PK, 1m, location.PK, "", ZDate.Empty, ZDate.Empty, "", "",
				"", "SN1", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			receive.WD_FinalisedDate = new ZDateTimeOffset(year, 1, 1);
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var result = LoadSQLFunction();
			AssertEquals(1, result.Length);
			AssertLineMatch(receive.Lines[0], 1m, result);
		}

		#endregion

		#region LoadSQLFunction

		DynamicBusinessObject[] LoadSQLFunction()
		{
			var sql = "SELECT * FROM dbo.WhsInvoiceInventory ORDER BY SerialNumber"; // If using a serial number, ensure it is deterministic order.
			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(sql);
			return result.ToArray();
		}

		#endregion

		#region AssertLineMatch

		void AssertLineMatch(WhsDocketLine expectedDocketLine, ZDecimal expectedQuantity,
			DynamicBusinessObject[] results)
		{
			var matchingLine = results.Single(d =>
				(ZDecimal)d["Quantity"] == expectedQuantity &&
				(ZString)d["SerialNumber"] == expectedDocketLine.WE_SerialNumber);
			var docket = expectedDocketLine.Docket;
			var inventory = expectedDocketLine.Inventory[0];

			AssertEquals("ClientPK", docket.WD_OH_Client, matchingLine["ClientPK"]);
			AssertEquals("WarehousePK", docket.WD_WW_Whs, matchingLine["WarehousePK"]);
			AssertEquals("LocationPK", inventory.WI_WL, matchingLine["LocationPK"]);
			AssertEquals("ProductPK", expectedDocketLine.WE_OP, matchingLine["ProductPK"]);
			AssertEquals("PartAttrib1", expectedDocketLine.WE_PartAttrib1, matchingLine["PartAttrib1"]);
			AssertEquals("PartAttrib2", expectedDocketLine.WE_PartAttrib2, matchingLine["PartAttrib2"]);
			AssertEquals("PartAttrib3", expectedDocketLine.WE_PartAttrib3, matchingLine["PartAttrib3"]);
		}

		#endregion
	}
}
