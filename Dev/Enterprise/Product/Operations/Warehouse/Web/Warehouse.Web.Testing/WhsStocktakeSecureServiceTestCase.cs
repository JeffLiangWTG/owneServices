using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	abstract class WhsStocktakeSecureServiceTestCase : WhsSecureServiceTestCase
	{
		protected WhsStocktakeTestData TestData { get; set; }

		protected void SetupEnvironmentDataForStocktake()
		{
			TestData = new WhsStocktakeTestData();

			// Warehouses / Areas / Locations

			TestData.Warehouse1 = Helper.CreateWarehouse("W1");
			var warehouse1Area1 = Helper.CreateArea(TestData.Warehouse1, "A1", Warehouse.Environment.CodeLists.AreaTypes.Codes.FreeStore);
			var warehouse1Area2 = Helper.CreateArea(TestData.Warehouse1, "A2", Warehouse.Environment.CodeLists.AreaTypes.Codes.FreeStore);

			var row11 = Helper.CreateRowAndGenerateLocations(TestData.Warehouse1, "Row11", 3, 1);
			var row12 = Helper.CreateRowAndGenerateLocations(TestData.Warehouse1, "Row12", 3, 1);

			row11.Locations[1].WLV_WA_PickingArea = warehouse1Area1.PK;
			row11.Locations[2].WLV_WA_PickingArea = warehouse1Area1.PK;
			row11.Locations[0].WLV_WA_PickingArea = warehouse1Area1.PK;

			row12.Locations[0].WLV_WA_PickingArea = warehouse1Area2.PK;
			row12.Locations[1].WLV_WA_PickingArea = warehouse1Area2.PK;
			row12.Locations[2].WLV_WA_PickingArea = warehouse1Area2.PK;

			TestData.Warehouse2 = Helper.CreateWarehouse("W2");
			var warehouse2Area1 = Helper.CreateArea(TestData.Warehouse2, "A1", Warehouse.Environment.CodeLists.AreaTypes.Codes.FreeStore);
			var warehouse2Area2 = Helper.CreateArea(TestData.Warehouse2, "A2", Warehouse.Environment.CodeLists.AreaTypes.Codes.FreeStore);
			var warehouse2Area3 = Helper.CreateArea(TestData.Warehouse2, "A3", Warehouse.Environment.CodeLists.AreaTypes.Codes.FreeStore);

			var row2 = Helper.CreateRowAndGenerateLocations(TestData.Warehouse2, "Row2", 3, 1);
			row2.Locations[0].WLV_WA_PickingArea = warehouse2Area1.PK;
			row2.Locations[1].WLV_WA_PickingArea = warehouse2Area2.PK;
			row2.Locations[2].WLV_WA_PickingArea = warehouse2Area3.PK;

			// Client

			var client = Helper.CreateClient("Client");

			// Parts

			TestData.Part1 = Helper.CreateProduct(client, "Part1");
			TestData.Part2 = Helper.CreateProduct(client, "Part2");
			TestData.Part3 = Helper.CreateProduct(client, "Part3");

			// Receives

			var receive1 = Helper.CreateWhsReceive(client, TestData.Warehouse1, "Receive1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, TestData.Part1, 100m, row11.Locations[2]);
			Helper.CreateWhsReceiveInventoryLine(receive1, TestData.Part2, 100m, row11.Locations[1]);
			Helper.CreateWhsReceiveInventoryLine(receive1, TestData.Part3, 100m, row11.Locations[0]);
			receive1.FinaliseDocket();
			AssertEquals(true, receive1.IsFinalised);

			var receive2 = Helper.CreateWhsReceive(client, TestData.Warehouse2, "Receive2", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive2, TestData.Part1, 200m, row2.Locations[0], "LP1");
			Helper.CreateWhsReceiveInventoryLine(receive2, TestData.Part2, 200m, row2.Locations[0], "LP1");
			Helper.CreateWhsReceiveInventoryLine(receive2, TestData.Part3, 200m, row2.Locations[0]);
			receive2.FinaliseDocket();
			AssertEquals(true, receive2.IsFinalised);

			var receive3 = Helper.CreateWhsReceive(client, TestData.Warehouse1, "Receive3", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive3, TestData.Part1, 300m, row12.Locations[2]);
			Helper.CreateWhsReceiveInventoryLine(receive3, TestData.Part2, 300m, row12.Locations[1]);
			Helper.CreateWhsReceiveInventoryLine(receive3, TestData.Part3, 300m, row12.Locations[0]);
			receive3.FinaliseDocket();
			AssertEquals(true, receive3.IsFinalised);

			// Stocktakes

			TestData.Stocktake1 = Helper.CreateWhsStocktake(client, TestData.Warehouse1);
			TestData.Stocktake1.WS_StocktakeDate = ZDateTime.Today.AddDays(-1);
			TestData.Stocktake2 = Helper.CreateWhsStocktake(client, TestData.Warehouse1);
			TestData.Stocktake2.WS_StocktakeDate = ZDateTime.Today;
			TestData.Stocktake3 = Helper.CreateWhsStocktake(client, TestData.Warehouse2);

			// Staff

			TestData.Staff1 = Helper.CreateGlbStaff("ST1", "ST1");
			TestData.Staff2 = Helper.CreateGlbStaff("ST2", "ST2");

			Helper.Factory.Save();
		}

		protected void LoadStocktakes()
		{
			TestData.Stocktake1.Load();
			TestData.Stocktake2.Load();
			TestData.Stocktake3.Load();

			Helper.Factory.Save();
		}

		protected void AssertStocktakeWebServiceResponse(string message, string expectedStocktakeNumber, int expectedLocationsCount, int expectedLinesToCount, SecureService webService, WhsStocktakeWebServiceResponse actualResponse)
		{
			AssertSuccessfulResponse(actualResponse, webService);
			AssertNotNull(message + " - Stocktake", actualResponse.Stocktake);
			AssertEquals(message + " - Stocktake number", expectedStocktakeNumber, actualResponse.Stocktake.Number);
			AssertEquals(message + " - LocationsToCount Count", expectedLocationsCount, actualResponse.LocationsToCount.Count);
			AssertEquals(message + " - LinesToCount Count", expectedLinesToCount, actualResponse.LinesToCount.Count);
			foreach (var line in actualResponse.LinesToCount)
			{
				if (actualResponse.LocationsToCount.Any())
				{
					AssertEquals("Lines Location", actualResponse.LocationsToCount[0], line.LocationString);
				}
			}
		}

		protected void AssertStocktakeLineDidCount(string message, ZDecimal expectedCount, GlbStaff expectedVerifiedBy, ZDateTime expectedVeridiedDate, Guid linePk)
		{
			var line = Helper.Factory.Load<WhsStocktakeLine>(new ZGuid(linePk));
			AssertNotNull(line);
			AssertEquals(message + " - CurrentCount", expectedCount, line.CurrentCount);
			AssertEquals(message + " - VerifiedBy", expectedVerifiedBy.PK, line.VerifiedBy.PK);
			AssertNotNull(message + " - DateVerified should not be null", line.WU_DateVerified);
			AssertEquals(message + " - DateVerified", expectedVeridiedDate.ToDateTime(), line.WU_DateVerified.ToDateTime());
		}

		#region WhsStocktakeTestData

		internal class WhsStocktakeTestData
		{
			public OrgSupplierPart Part1 { get; set; }
			public OrgSupplierPart Part2 { get; set; }
			public OrgSupplierPart Part3 { get; set; }
			public WhsStocktake Stocktake1 { get; set; }
			public WhsStocktake Stocktake2 { get; set; }
			public WhsStocktake Stocktake3 { get; set; }
			public GlbStaff Staff1 { get; set; }
			public GlbStaff Staff2 { get; set; }
			public WhsWarehouse Warehouse1 { get; set; }
			public WhsWarehouse Warehouse2 { get; set; }
		}

		#endregion

	}
}
