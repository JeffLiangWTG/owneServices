using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class vw_report_WhsTransactionsTest : WhsTestCaseWithFactory
	{
		#region TestView_Transfers

		[TestDate(2012, 3, 15, 5, 5, 0)]
		public void TestView_Transfers()
		{
			var whs1 = Helper.CreateWarehouse("WH1", "A", 2, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			var client = Helper.CreateClient("CLIENT");
			var part = Helper.CreateProduct(client, "P1");
			Factory.Save();

			var now = ZDateTimeOffset.Now;

			var receive =
				Helper.CreateWhsReceiveWithInventory(client, whs1, "R1", part, 100m, whs1.FindLocation("A-1"), "");

			var transferInner = Helper.CreateWhsTransfer(client, whs1, "TR1", Notify, TransferType.Codes.Internal);
			var transferInnerLine = Helper.CreateWhsTransferLine(transferInner, part, 10m, "A-1", "A-2");
			transferInner.FinaliseDocket();
			AssertIsFinalisedPrecondition(transferInner);

			transferInnerLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			transferInnerLine.HeldCodeChangeQuantity = 4m;
			transferInnerLine.ChangeInventoryHeldCode(true);

			var transferInterWhsSource =
				Helper.CreateWhsTransfer(client, whs1, "TR2", Notify, TransferType.Codes.InterWhsSource);
			var transferInterWhsSourceLine1 =
				Helper.CreateWhsTransferLine(transferInterWhsSource, part, 15m, "A-1", whs2.PK, "B");
			var transferInterWhsSourceLine2 =
				Helper.CreateWhsTransferLine(transferInterWhsSource, part, 20m, "A-1", whs2.PK, "B");
			transferInterWhsSourceLine1.RunPreSaveValidation(); // to commit inventory
			transferInterWhsSourceLine2.FinaliseDocketLine();
			AssertEquals("Precondition - ensure transfer line is not finalised.", false,
				transferInterWhsSourceLine1.IsFinalised);
			AssertIsFinalisedPrecondition(transferInterWhsSourceLine2);

			var transferInterWhsDest =
				Helper.CreateWhsTransfer(client, whs2, "TR3", Notify, TransferType.Codes.InterWhsDest);
			var transferInterWhsDestLine =
				Helper.CreateWhsTransferLine(transferInterWhsDest, part, 25m, "A-1", whs1.PK, "B");
			transferInterWhsDest.FinaliseDocket();
			AssertIsFinalisedPrecondition(transferInterWhsDest);

			transferInterWhsDestLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			transferInterWhsDestLine.HeldCodeChangeQuantity = 8m;
			transferInterWhsDestLine.ChangeInventoryHeldCode(true);

			// hack-change transfers finalised date to ensure that finalised dates from lines are used.
			transferInner.WD_FinalisedDate = now.AddDays(1);
			transferInterWhsDest.WD_FinalisedDate = now.AddDays(1);

			Factory.Save();

			var statusChangeLine =
				Factory.LoadTop1<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_TransactionQuantity, 8m));

			var results = LoadSQLFunction();
			AssertEquals("Should find 1 receive + 2*2 inter whs transactions + 1 status change of inter whs dest.", 6,
				results.Length);
			AssertLineMatch(results, receive.Lines[0], "", "");
			AssertLineMatch(results, transferInterWhsSourceLine2, "", "");
			AssertLineMatch(results, transferInterWhsSourceLine2.ChildTransferLine, "", "");
			AssertLineMatch(results, transferInterWhsDestLine, "", "");
			AssertLineMatch(results, transferInterWhsDestLine.ChildTransferLine, "", "");
			AssertLineMatch(results, statusChangeLine, "", "");
		}

		#endregion

		#region TestView_ReleaseCapturedAttribs

		public void TestView_ReleaseCapturedAttribs()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			data.Part2.Delete(); // don't want an unused product to appear in the results.
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, inventoryLocation,
				ZDate.Empty, ZDate.Empty, "", "BATCH123", "", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.Quantity = 6m;
			releaseLine1.PartAttribute1 = "RED";

			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			releaseLine2.Quantity = 4m;
			releaseLine2.PartAttribute2 = "BATCH123";
			releaseLine2.PartAttribute1 = "BLUE";

			order.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(order);

			Factory.Save();

			var results = LoadSQLFunction();
			AssertEquals("Should be 3 results, 1 receive Line + 2 Release Captured Attributes.", 3, results.Length);
			AssertLineMatch(results, inventory.InDocketLine, "", "BATCH123", 10m);
			AssertLineMatch(results, orderLine, "BLUE", "BATCH123", -4m);
			AssertLineMatch(results, orderLine, "RED", "BATCH123", -6m);
		}

		#endregion

		#region TestReport_SerialNumber

		public void TestReport_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, true,
				setReleaseCaptured: true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			inventory1.WI_SerialNumber = "SN1";
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			orderLine1.WE_SerialNumber = "SN1";
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part2, 1m);

			var pick = Helper.CreatePickNew(order1, order2);
			var releaseLine = ((WhsReleaseLine)orderLine2.ReleaseLines.Single()).SerialNumber = "SN2";

			pick.FinaliseAllOrders();
			AssertEquals("Order1 should be finalized", true, order1.IsFinalised);
			AssertEquals("Order2 should be finalized", true, order2.IsFinalised);
			pick.FinalisePick();
			AssertEquals("Pick should be finalized", true, pick.IsFinalised);

			Factory.Save();

			var results = LoadSQLFunction();
			AssertEquals(
				"Should be 4 results, 2 Receive Lines + 2 Order Lines (1 with values from orderline and 1 from release line ",
				4, results.Length);
			AssertLineMatch(results, inventory1.InDocketLine, "", "", 1m, "SN1");
			AssertLineMatch(results, inventory2.InDocketLine, "", "", 1m, "");
			AssertLineMatch(results, orderLine1, "", "", -1m, "SN1");
			AssertLineMatch(results, orderLine2, "", "", -1m, "SN2");
		}

		#endregion

		#region LoadSQLFunction

		DynamicBusinessObject[] LoadSQLFunction()
		{
			var sql = "select * from dbo.vw_report_WhsTransactions order by Units";
			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(sql);

			return result.ToArray();
		}

		#endregion

		#region AssertLineMatch

		void AssertLineMatch(DynamicBusinessObject[] results, WhsDocketLine expectedLine, string partAttrib1 = "",
			string partAttrib2 = "", ZDecimal? expectedUnits = null, string expectedSerialNumber = "")
		{
			var docket = expectedLine.Docket;
			var client = docket.Client;
			var part = expectedLine.SupplierPart;

			if (!expectedUnits.HasValue)
			{
				switch (docket.WD_DocketType)
				{
					case DocketType.Codes.Order:
						expectedUnits = -expectedLine.WE_TransactionQuantity;
						break;
					case DocketType.Codes.Transfer:
						expectedUnits = docket.WD_DocketSubType == TransferType.Codes.InterWhsSource
							? (ZDecimal)(-expectedLine.WE_TransactionQuantity)
							: expectedLine.WE_TransactionQuantity;
						break;
					default:
						expectedUnits = expectedLine.WE_TransactionQuantity;
						break;
				}
			}

			var expectedFinalisedDate = (docket.WD_DocketType == DocketType.Codes.Transfer)
				? expectedLine.WE_FinalisedDate
				: docket.WD_FinalisedDate;

			var actualLine = results.Single(d =>
				(ZDecimal)d["Units"] == expectedUnits && (ZString)d["SerialNumberMet"] == expectedSerialNumber);

			AssertEquals("WarehousePK", docket.WD_WW_Whs, actualLine["WarehousePK"]);
			AssertEquals("WarehouseName", docket.Warehouse.WW_WarehouseName, actualLine["WarehouseName"]);
			AssertEquals("ClientPK", docket.WD_OH_Client, actualLine["ClientPK"]);
			AssertEquals("ClientCode", client.OH_Code, actualLine["ClientCode"]);
			AssertEquals("Client", client.OH_FullName, actualLine["Client"]);
			AssertEquals("Reference", docket.WD_ExternalReference, actualLine["Reference"]);
			AssertEquals("TranType", docket.WD_DocketType, actualLine["TranType"]);
			AssertEquals("Product", part.OP_PartNum, actualLine["Product"]);
			AssertEquals("ProductDesc", part.OP_Desc, actualLine["ProductDesc"]);
			AssertEquals("ProductBrandName", part.OP_Brand, actualLine["ProductBrandName"]);
			AssertEquals("ProductModel", part.OP_Model, actualLine["ProductModel"]);
			AssertEquals("ProductPK", expectedLine.WE_OP, actualLine["ProductPK"]);
			AssertEquals("PartAttrib1Met", partAttrib1, actualLine["PartAttrib1Met"]);
			AssertEquals("PartAttrib2Met", partAttrib2, actualLine["PartAttrib2Met"]);
			AssertEquals("PartAttrib3Met", expectedLine.WE_PartAttrib3, actualLine["PartAttrib3Met"]);
			AssertEquals("SerialNumberMet", expectedSerialNumber, actualLine["SerialNumberMet"]);
			AssertEquals("FinalisedDate", expectedFinalisedDate.Date, ((ZDateTime)actualLine["FinalisedDate"]).Date);
			AssertEquals("LineComment", expectedLine.WE_LineComment, actualLine["LineComment"]);
			AssertEquals("Units", expectedUnits, actualLine["Units"]);

			// Add custom line and docket attributes here.
		}

		#endregion
	}
}
