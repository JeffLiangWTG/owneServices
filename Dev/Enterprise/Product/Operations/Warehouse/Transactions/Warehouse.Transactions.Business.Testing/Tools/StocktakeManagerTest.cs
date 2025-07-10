using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business.Tools;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class StocktakeManagerTest : WhsTestCaseWithFactory
	{
		#region Constructors

		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructorWithNullWarehouse()
		{
			StocktakeManager =
				new StocktakeManager(Factory, ZString.Empty, ZString.Empty, null, Factory.New<GlbStaff>());
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructorWithNullStaff()
		{
			StocktakeManager =
				new StocktakeManager(Factory, ZString.Empty, ZString.Empty, Factory.New<WhsWarehouse>(), null);
		}

		[ExpectNoExceptions]
		public void TestConstructor()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var staff = Factory.New<GlbStaff>();
			StocktakeManager = new StocktakeManager(Factory, "Area ", "Man ", warehouse, staff);
			AssertNotNull(StocktakeManager);
			AssertEquals("AREA", StocktakeManager.Area);
			AssertEquals("MAN", StocktakeManager.PickMethod);
			AssertEquals(warehouse, StocktakeManager.Warehouse);
			AssertEquals(staff, StocktakeManager.Staff);
			AssertNull(StocktakeManager.Stocktake);
			AssertNotNull(StocktakeManager.LinesToCount);
			AssertEquals(0, StocktakeManager.LinesToCount.Count);
		}

		#endregion

		#region TestFindAndAssignNextStocktake

		public void TestFindAndAssignNextStocktake_NoLoadedStocktake()
		{
			SetupEnvironmentData();
			var area = "ANY";
			var pickMethod = "ANY";
			//// Warehouse 1
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse1, Staff1);
			AssertEquals(false, StocktakeManager.FindAndAssignNextStocktake(ZString.Empty));
			AssertEquals(false, StocktakeManager.FindAndAssignNextStocktake(Stocktake1.WS_StocktakeNumber));
			AssertEquals(false, StocktakeManager.FindAndAssignNextStocktake(Stocktake2.WS_StocktakeNumber));
			AssertEquals(false, StocktakeManager.FindAndAssignNextStocktake(Stocktake3.WS_StocktakeNumber));
			//// Warehouse 2
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse2, Staff1);
			AssertEquals(false, StocktakeManager.FindAndAssignNextStocktake(ZString.Empty));
			AssertEquals(false, StocktakeManager.FindAndAssignNextStocktake(Stocktake1.WS_StocktakeNumber));
			AssertEquals(false, StocktakeManager.FindAndAssignNextStocktake(Stocktake2.WS_StocktakeNumber));
			AssertEquals(false, StocktakeManager.FindAndAssignNextStocktake(Stocktake3.WS_StocktakeNumber));
		}

		public void TestFindAndAssignNextStocktake_NextFreeUnfinalizedLoadedStocktake()
		{
			SetupEnvironmentData();
			LoadStocktakes();
			var area = "ANY";
			var pickMethod = "ANY";
			//// Warehouse 1
			////// Staff 1 for a new stocktake
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse1, Staff1);
			AssertEquals("Staff 1 should find an available stocktake", true,
				StocktakeManager.FindAndAssignNextStocktake(ZString.Empty));
			AssertAssignedStocktake("Staff 1 should take first available stocktake, ie stoctake 1", Stocktake1,
				Stocktake1.Lines.Count, Warehouse1, area, pickMethod, Staff1, StocktakeManager);
			////// Staff 1 for a began stocktake
			var line = Stocktake1.Lines[0];
			line.CurrentCount = 0;
			line.WU_DateVerified = ZDateTime.Now;
			line.WU_Status = CodeLists.StocktakeLineStatus.Codes.Closed;
			Factory.Save();
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse1, Staff1);
			AssertEquals("Staff 1 should find an available stocktake (already assigned to him)", true,
				StocktakeManager.FindAndAssignNextStocktake(ZString.Empty));
			AssertAssignedStocktake("Staff 1 should take first available stocktake, ie stoctake 1", Stocktake1,
				Stocktake1.Lines.Count - 1, Warehouse1, area, pickMethod, Staff1, StocktakeManager);
			////// Staff 2 for next available stocktake
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse1, Staff2);
			AssertEquals(true, StocktakeManager.FindAndAssignNextStocktake(ZString.Empty));
			AssertAssignedStocktake("Staff 2 should take the next available stocktake, ie stoctake 2", Stocktake2,
				Stocktake2.Lines.Count, Warehouse1, area, pickMethod, Staff2, StocktakeManager);
			UnassignOpenLines(
				Stocktake1); // Now stocktake 1 is available but staff 2 is already assigned to stocktake 2
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse1, Staff2);
			AssertEquals("Staff 2 should find an available stocktake", true,
				StocktakeManager.FindAndAssignNextStocktake(ZString.Empty));
			AssertAssignedStocktake("Staff 2 should take its began stocktake, ie stoctake 2", Stocktake2,
				Stocktake2.Lines.Count, Warehouse1, area, pickMethod, Staff2, StocktakeManager);
			UnassignOpenLines(Stocktake2); // Now stocktake 1 and stocktake 2 are available
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse1, Staff2);
			AssertEquals("Staff 2 should find an available stocktake", true,
				StocktakeManager.FindAndAssignNextStocktake(ZString.Empty));
			AssertAssignedStocktake("Staff 2 should take the first available stocktake, ie stoctake 1", Stocktake1,
				Stocktake1.Lines.Count - 1, Warehouse1, area, pickMethod, Staff2, StocktakeManager);
			UnassignOpenLines(Stocktake1); // Now stocktake 1 and stocktake 2 are available
			Stocktake1.WS_StocktakeDate = ZDateTime.Today;
			Stocktake2.WS_StocktakeDate = ZDateTime.Today.AddDays(-1); // Now stocktake 2 has priority on stocktake 1
			Factory.Save();
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse1, Staff2);
			AssertEquals("Staff 2 should find an available stocktake", true,
				StocktakeManager.FindAndAssignNextStocktake(ZString.Empty));
			AssertAssignedStocktake("Staff 2 should take the first available stocktake, ie stoctake 2", Stocktake2,
				Stocktake2.Lines.Count, Warehouse1, area, pickMethod, Staff2, StocktakeManager);
			//// Warehouse 2
			////// Staff 1
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse2, Staff1);
			AssertEquals("Staff 1 should find an available stocktake", true,
				StocktakeManager.FindAndAssignNextStocktake(ZString.Empty));
			AssertAssignedStocktake("First staff should take first stocktake, ie stoctake 3", Stocktake3,
				Stocktake3.Lines.Count, Warehouse2, area, pickMethod, Staff1, StocktakeManager);
			////// Staff 2
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse2, Staff2);
			AssertEquals("Staff 2 shoudl not find any available stocktake", false,
				StocktakeManager.FindAndAssignNextStocktake(ZString.Empty));
		}

		public void TestFindAndAssignNextStocktake_ByStocktakeNumber()
		{
			SetupEnvironmentData();
			LoadStocktakes();
			var area = "ANY";
			var pickMethod = "ANY";
			// Stocktake number in current warehouse
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse1, Staff1);
			AssertEquals("Staff 1 should find the available stocktake 1", true,
				StocktakeManager.FindAndAssignNextStocktake(Stocktake1.WS_StocktakeNumber));
			AssertAssignedStocktake("Staff 1 should be assigned to stocktake 1", Stocktake1, Stocktake1.Lines.Count,
				Warehouse1, area, pickMethod, Staff1, StocktakeManager);
			// Stocktake number in current warehouse but already assigned to someone else
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse1, Staff2);
			AssertEquals("Staff 2 should not find stocktake 1 (already assigned to staff 1)", false,
				StocktakeManager.FindAndAssignNextStocktake(Stocktake1.WS_StocktakeNumber));
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse1, Staff2);
			AssertEquals("Staff 2 should find the available stocktake 2", true,
				StocktakeManager.FindAndAssignNextStocktake(Stocktake2.WS_StocktakeNumber));
			AssertAssignedStocktake("Staff 2 should be assigned to stocktake 2", Stocktake2, Stocktake2.Lines.Count,
				Warehouse1, area, pickMethod, Staff2, StocktakeManager);
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse1, Staff1);
			AssertEquals("Staff 1 should not find stocktake 2 (already assigned to staff 2)", false,
				StocktakeManager.FindAndAssignNextStocktake(Stocktake2.WS_StocktakeNumber));
			// Stocktake number in another warehouse
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse1, Staff1);
			AssertEquals("Staff 1 should not find stocktake 3 which is in another warehouse", false,
				StocktakeManager.FindAndAssignNextStocktake(Stocktake3.WS_StocktakeNumber));
		}

		public void TestFindAndAssignNextStocktake_WithSpecificArea()
		{
			SetupEnvironmentData();
			LoadStocktakes();
			UpdateAreaForStocktake(Warehouse1Area1.PK, Stocktake1);
			UpdateAreaForStocktake(Warehouse1Area1.PK, Stocktake2);
			var pickMethod = "ANY";
			// Available stocktake in any area
			var area = "ANY";
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse1, Staff1);
			AssertEquals("Staff 1 should find an available stocktake in any area", true,
				StocktakeManager.FindAndAssignNextStocktake(ZString.Empty));
			// Available stocktake in valid area
			area = Warehouse1Area1.WA_Name;
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse1, Staff1);
			AssertEquals("Staff 1 should find an available stocktake in area 1", true,
				StocktakeManager.FindAndAssignNextStocktake(ZString.Empty));
			// No available stocktake in another area
			area = Warehouse1Area2.WA_Name;
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse1, Staff1);
			AssertEquals("Staff 1 should not find any available stocktake in area 2", false,
				StocktakeManager.FindAndAssignNextStocktake(ZString.Empty));
		}

		public void TestFindAndAssignNextStocktake_WithSpecificPickMethod()
		{
			SetupEnvironmentData();
			LoadStocktakes();
			var pickMethodManual = "MAN";
			UpdatePickMethodForStocktake(pickMethodManual, Stocktake1);
			UpdatePickMethodForStocktake(pickMethodManual, Stocktake2);
			var area = "ANY";
			// Available stocktake in any pick method
			var pickMethod = "ANY";
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse1, Staff1);
			AssertEquals("Staff 1 should find an available stocktake in any area", true,
				StocktakeManager.FindAndAssignNextStocktake(ZString.Empty));
			// Available stocktake in valid pick method
			pickMethod = pickMethodManual;
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse1, Staff1);
			AssertEquals("Staff 1 should find an available stocktake in area 1", true,
				StocktakeManager.FindAndAssignNextStocktake(ZString.Empty));
			// No available stocktake in another pick method
			pickMethod = "CAR";
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse1, Staff1);
			AssertEquals("Staff 1 should not find any available stocktake in area 2", false,
				StocktakeManager.FindAndAssignNextStocktake(ZString.Empty));
		}

		public void TestFindAndAssignNextStocktake_StocktakeLinesAscending()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Staff1 = Helper.CreateGlbStaff("ST1", "ST1");
			Helper.CreateRowAndGenerateLocations(data.Whs1, "10", 2, 1, 1, 1);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "20", 2, 1, 1, 3);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "30", 2, 1, 1, 2);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "100", 2, 12, 1, 10);
			row.SortPickPathMethod = SortPathMethods.Codes.LevelThenColumn;
			row.UpdatePathSequenceOnLocations();
			Factory.Save();
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m,
				data.Whs1.FindLocation("10-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m,
				data.Whs1.FindLocation("10-2"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m,
				data.Whs1.FindLocation("20-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part1, 10m,
				data.Whs1.FindLocation("20-2"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 10m,
				data.Whs1.FindLocation("30-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R6", data.Part1, 10m,
				data.Whs1.FindLocation("30-2"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R7", data.Part1, 10m,
				data.Whs1.FindLocation("100-1-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R8", data.Part1, 10m,
				data.Whs1.FindLocation("100-2-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R9", data.Part1, 10m,
				data.Whs1.FindLocation("100-1-2"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R10", data.Part1, 10m,
				data.Whs1.FindLocation("100-1-10"), "");
			Factory.Save();
			stocktake.Load();
			Factory.Save();
			var line10_1 = stocktake.Lines.First(l => l.LocationString == "10-1");
			var line10_2 = stocktake.Lines.First(l => l.LocationString == "10-2");
			var line20_1 = stocktake.Lines.First(l => l.LocationString == "20-1");
			var line20_2 = stocktake.Lines.First(l => l.LocationString == "20-2");
			var line30_1 = stocktake.Lines.First(l => l.LocationString == "30-1");
			var line30_2 = stocktake.Lines.First(l => l.LocationString == "30-2");
			var line100_1_1 = stocktake.Lines.First(l => l.LocationString == "100-1-1");
			var line100_1_2 = stocktake.Lines.First(l => l.LocationString == "100-1-2");
			var line100_1_10 = stocktake.Lines.First(l => l.LocationString == "100-1-10");
			var line100_2_1 = stocktake.Lines.First(l => l.LocationString == "100-2-1");
			var stocktakeManager = new StocktakeManager(Factory, "ANY", "ANY", data.Whs1, Staff1);
			AssertEquals(true, stocktakeManager.FindAndAssignNextStocktake(stocktake.WS_StocktakeNumber));
			AssertStockTakeLinesInExactOrder(
				new[]
				{
					line10_1, line10_2, line30_1, line30_2, line20_1, line20_2, line100_1_1, line100_2_1,
					line100_1_2, line100_1_10
				}, stocktakeManager.LinesToCount);
		}

		public void TestFindAndAssignNextStocktake_DoesNotSortStocktakeLinesMultipleTimes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Staff1 = Helper.CreateGlbStaff("ST1", "ST1");
			Helper.CreateRowAndGenerateLocations(data.Whs1, "10", 2, 1, 1, 1);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "20", 2, 1, 1, 3);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "30", 2, 1, 1, 2);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "100", 2, 12, 1, 10);
			row.SortPickPathMethod = SortPathMethods.Codes.LevelThenColumn;
			row.UpdatePathSequenceOnLocations();
			Factory.Save();
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m,
				data.Whs1.FindLocation("10-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m,
				data.Whs1.FindLocation("10-2"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m,
				data.Whs1.FindLocation("20-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part1, 10m,
				data.Whs1.FindLocation("20-2"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 10m,
				data.Whs1.FindLocation("30-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R6", data.Part1, 10m,
				data.Whs1.FindLocation("30-2"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R7", data.Part1, 10m,
				data.Whs1.FindLocation("100-1-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R8", data.Part1, 10m,
				data.Whs1.FindLocation("100-2-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R9", data.Part1, 10m,
				data.Whs1.FindLocation("100-1-2"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R10", data.Part1, 10m,
				data.Whs1.FindLocation("100-1-10"), "");
			Factory.Save();
			stocktake.Load();
			Factory.Save();
			var listChangedCount = 0;
			((IActiveBusinessObjectCollection)stocktake.Lines).ListChanged += delegate
			{
				listChangedCount++;
			};
			var stocktakeManager = new StocktakeManager(Factory, "ANY", "ANY", data.Whs1, Staff1);
			AssertEquals(true, stocktakeManager.FindAndAssignNextStocktake(stocktake.WS_StocktakeNumber));
			AssertEquals(
				"List changed event should only be called 3 times. - 1. ApplySort 2. Dispose for OnSuspendListChanged() 3. Factory Save 4. SystemCreate and SystemLastEdit columns populated",
				4, listChangedCount);
		}

		public void TestFindAndAssignNextStocktake_DBHitsTest()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Staff1 = Helper.CreateGlbStaff("ST1", "ST1");
			Helper.CreateRowAndGenerateLocations(data.Whs1, "10", 2, 1, 1, 1);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "20", 2, 1, 1, 3);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "30", 2, 1, 1, 2);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "100", 2, 12, 1, 10);
			row.SortPickPathMethod = SortPathMethods.Codes.LevelThenColumn;
			row.UpdatePathSequenceOnLocations();
			var org2 = Helper.CreateClient("2");
			var part3 = Helper.CreateProduct(data.Org1, "P3");
			var part4 = Helper.CreateProduct(org2, "P4");
			Factory.Save();
			var stocktake = Helper.CreateWhsStocktake(data.Whs1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m,
				data.Whs1.FindLocation("10-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m,
				data.Whs1.FindLocation("10-2"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m,
				data.Whs1.FindLocation("20-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part1, 10m,
				data.Whs1.FindLocation("20-2"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 10m,
				data.Whs1.FindLocation("30-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R6", data.Part2, 10m,
				data.Whs1.FindLocation("10-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R7", data.Part2, 10m,
				data.Whs1.FindLocation("10-2"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R8", data.Part2, 10m,
				data.Whs1.FindLocation("20-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R9", data.Part2, 10m,
				data.Whs1.FindLocation("20-2"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R10", data.Part2, 10m,
				data.Whs1.FindLocation("30-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R11", part3, 10m,
				data.Whs1.FindLocation("10-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R12", part3, 10m,
				data.Whs1.FindLocation("10-2"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R13", part3, 10m,
				data.Whs1.FindLocation("20-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R14", part3, 10m,
				data.Whs1.FindLocation("20-2"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R15", part3, 10m,
				data.Whs1.FindLocation("30-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R16", data.Part2, 10m,
				data.Whs1.FindLocation("30-2"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R17", data.Part2, 10m,
				data.Whs1.FindLocation("100-1-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R18", data.Part2, 10m,
				data.Whs1.FindLocation("100-2-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R19", data.Part2, 10m,
				data.Whs1.FindLocation("100-1-2"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R20", data.Part2, 10m,
				data.Whs1.FindLocation("100-1-10"), "");
			Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R21", part4, 10m, data.Whs1.FindLocation("100-1-10"),
				"");
			Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R22", part4, 10m, data.Whs1.FindLocation("10-1"),
				"");
			Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R23", part4, 10m, data.Whs1.FindLocation("20-1"),
				"");
			Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R24", part4, 10m, data.Whs1.FindLocation("30-1"),
				"");
			Factory.Save();
			stocktake.Load();
			Factory.Save();
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var stocktakeManager = new StocktakeManager(newFactory, "ANY", "ANY", data.Whs1, Staff1);
			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GlbStaffSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsStocktakeSchema.Constants.TableName, 1 },
				{ WhsStocktakeLineSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 }
			};
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			{
				stocktakeManager.FindAndAssignNextStocktake(stocktake.WS_StocktakeNumber);
			}
		}

		void AssertStockTakeLinesInExactOrder(WhsStocktakeLine[] expected, WhsStocktakeLineCollectionND result)
		{
			AssertEquals(expected.Length, result.Count);
			var index = 0;
			foreach (var expectedStockTakeLine in expected)
			{
				AssertEquals(string.Format("Unexpected line at index {0}", index), expectedStockTakeLine,
					result[index++]);
			}
		}

		#endregion

		#region TestSetStocktakeLineCountQuantity

		[TestDate(2012, 3, 15)]
		public void TestSetStocktakeLineCountQuantity()
		{
			SetupEnvironmentData();
			LoadStocktakes();
			var area = "ANY";
			var pickMethod = "ANY";
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse1, Staff1);
			AssertEquals("Staff 1 should find an available stocktake", true,
				StocktakeManager.FindAndAssignNextStocktake(ZString.Empty));
			AssertAssignedStocktake("Staff 1 should take first available stocktake, ie stoctake 1", Stocktake1,
				Stocktake1.Lines.Count, Warehouse1, area, pickMethod, Staff1, StocktakeManager);
			var line1 = Stocktake1.Lines[0];
			var count = 1;
			var packsType = line1.SupplierPart.OP_StockKeepingUnit;
			var error = StocktakeManager.SetStocktakeLineCountQuantity(line1, packsType, count);
			Assert(error.IsNullOrEmpty());
			AssertCountStocktakeLine("Stocktake line should be count", count, ZDateTime.Now, Staff1, line1);
		}

		public void TestSetStocktakeLineCountQuantity_InvalidOperator()
		{
			SetupEnvironmentData();
			LoadStocktakes();
			var area = "ANY";
			var pickMethod = "ANY";
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse1, Staff1);
			AssertEquals("Staff 1 should find an available stocktake", true,
				StocktakeManager.FindAndAssignNextStocktake(ZString.Empty));
			AssertAssignedStocktake("Staff 1 should take first available stocktake, ie stoctake 1", Stocktake1,
				Stocktake1.Lines.Count, Warehouse1, area, pickMethod, Staff1, StocktakeManager);
			var line1 = Stocktake1.Lines[0];
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse1, Staff2);
			var count = 1;
			var packsType = line1.SupplierPart.OP_StockKeepingUnit;
			var error = StocktakeManager.SetStocktakeLineCountQuantity(line1, packsType, count);
			AssertEquals(
				"Unable to update stocktake line count quantity. This stocktake line is not assigned or assigned to another operator.",
				error);
		}

		public void TestSetStocktakeLineCountQuantity_AlreadyCounted()
		{
			SetupEnvironmentData();
			LoadStocktakes();
			var area = "ANY";
			var pickMethod = "ANY";
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse1, Staff1);
			AssertEquals("Staff 1 should find an available stocktake", true,
				StocktakeManager.FindAndAssignNextStocktake(ZString.Empty));
			AssertAssignedStocktake("Staff 1 should take first available stocktake, ie stoctake 1", Stocktake1,
				Stocktake1.Lines.Count, Warehouse1, area, pickMethod, Staff1, StocktakeManager);
			var line1 = Stocktake1.Lines[0];
			var count = 1;
			var packsType = line1.SupplierPart.OP_StockKeepingUnit;
			var error = StocktakeManager.SetStocktakeLineCountQuantity(line1, packsType, count);
			Assert(error.IsNullOrEmpty());
			var error2 = StocktakeManager.SetStocktakeLineCountQuantity(line1, packsType, count);
			AssertEquals(
				"Unable to update stocktake line count quantity. This stocktake line has already been counted.",
				error2);
		}

		public void TestSetStocktakeLineCountQuantity_InvalidQuantity()
		{
			SetupEnvironmentData();
			LoadStocktakes();
			var area = "ANY";
			var pickMethod = "ANY";
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse1, Staff1);
			AssertEquals("Staff 1 should find an available stocktake", true,
				StocktakeManager.FindAndAssignNextStocktake(ZString.Empty));
			AssertAssignedStocktake("Staff 1 should take first available stocktake, ie stoctake 1", Stocktake1,
				Stocktake1.Lines.Count, Warehouse1, area, pickMethod, Staff1, StocktakeManager);
			var line1 = Stocktake1.Lines[0];
			var count = -1;
			var packsType = line1.SupplierPart.OP_StockKeepingUnit;
			var error = StocktakeManager.SetStocktakeLineCountQuantity(line1, packsType, count);
			AssertEquals("The count quantity cannot be negative.", error);
		}

		public void TestSetStocktakeLineCountQuantity_InvalidStocktakeLine()
		{
			SetupEnvironmentData();
			LoadStocktakes();
			var area = "ANY";
			var pickMethod = "ANY";
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse1, Staff1);
			AssertEquals("Staff 1 should find an available stocktake", true,
				StocktakeManager.FindAndAssignNextStocktake(ZString.Empty));
			AssertAssignedStocktake("Staff 1 should take first available stocktake, ie stoctake 1", Stocktake1,
				Stocktake1.Lines.Count, Warehouse1, area, pickMethod, Staff1, StocktakeManager);
			var line1 = Stocktake1.Lines[0];
			var count = 1;
			var packsType = line1.SupplierPart.OP_StockKeepingUnit;
			line1.WU_WL = ZGuid.Empty;
			var error = StocktakeManager.SetStocktakeLineCountQuantity(line1, packsType, count);
			Assert(error.Contains("Invalid stocktake line"));
		}

		#endregion

		#region TestAddStocktakeLineCountQuantity

		public void TestAddStocktakeLineCountQuantity()
		{
			SetupEnvironmentData();
			LoadStocktakes();
			var area = "ANY";
			var pickMethod = "ANY";
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse1, Staff1);
			AssertEquals("Staff 1 should find an available stocktake", true,
				StocktakeManager.FindAndAssignNextStocktake(ZString.Empty));
			AssertAssignedStocktake("Staff 1 should take first available stocktake, ie stoctake 1", Stocktake1,
				Stocktake1.Lines.Count, Warehouse1, area, pickMethod, Staff1, StocktakeManager);
			var line1 = Stocktake1.Lines[0];
			var count = 1;
			var packsType = line1.SupplierPart.OP_StockKeepingUnit;
			StocktakeManager.AddStocktakeLineCountQuantity(line1, packsType, count);
			AssertCountStocktakeLine("Stocktake line should be count", count, ZDateTime.Now, Staff1, line1);
			StocktakeManager.AddStocktakeLineCountQuantity(line1, packsType, count);
			AssertCountStocktakeLine("Stocktake line should be 2*count", count + count, ZDateTime.Now, Staff1, line1);
		}

		public void TestAddStocktakeLineCountQuantity_InvalidOperator()
		{
			SetupEnvironmentData();
			LoadStocktakes();
			var area = "ANY";
			var pickMethod = "ANY";
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse1, Staff1);
			AssertEquals("Staff 1 should find an available stocktake", true,
				StocktakeManager.FindAndAssignNextStocktake(ZString.Empty));
			AssertAssignedStocktake("Staff 1 should take first available stocktake, ie stoctake 1", Stocktake1,
				Stocktake1.Lines.Count, Warehouse1, area, pickMethod, Staff1, StocktakeManager);
			var line1 = Stocktake1.Lines[0];
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse1, Staff2);
			var count = 1;
			var packsType = line1.SupplierPart.OP_StockKeepingUnit;
			var error = StocktakeManager.AddStocktakeLineCountQuantity(line1, packsType, count);
			AssertEquals(
				"Unable to update stocktake line count quantity. This stocktake line is not assigned or assigned to another operator.",
				error);
		}

		public void TestAddStocktakeLineCountQuantity_InvalidQuantity()
		{
			SetupEnvironmentData();
			LoadStocktakes();
			var area = "ANY";
			var pickMethod = "ANY";
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse1, Staff1);
			AssertEquals("Staff 1 should find an available stocktake", true,
				StocktakeManager.FindAndAssignNextStocktake(ZString.Empty));
			AssertAssignedStocktake("Staff 1 should take first available stocktake, ie stoctake 1", Stocktake1,
				Stocktake1.Lines.Count, Warehouse1, area, pickMethod, Staff1, StocktakeManager);
			var line1 = Stocktake1.Lines[0];
			var count = -1;
			var packsType = line1.SupplierPart.OP_StockKeepingUnit;
			var error = StocktakeManager.AddStocktakeLineCountQuantity(line1, packsType, count);
			AssertEquals("The count quantity cannot be negative.", error);
		}

		public void TestAddStocktakeLineCountQuantity_InvalidStocktakeLine()
		{
			SetupEnvironmentData();
			LoadStocktakes();
			var area = "ANY";
			var pickMethod = "ANY";
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse1, Staff1);
			AssertEquals("Staff 1 should find an available stocktake", true,
				StocktakeManager.FindAndAssignNextStocktake(ZString.Empty));
			AssertAssignedStocktake("Staff 1 should take first available stocktake, ie stoctake 1", Stocktake1,
				Stocktake1.Lines.Count, Warehouse1, area, pickMethod, Staff1, StocktakeManager);
			var line1 = Stocktake1.Lines[0];
			var count = 1;
			var packsType = line1.SupplierPart.OP_StockKeepingUnit;
			line1.WU_WL = ZGuid.Empty;
			var error = StocktakeManager.AddStocktakeLineCountQuantity(line1, packsType, count);
			Assert(error.Contains("Invalid stocktake line"));
		}

		#endregion

		#region TestAddNewStocktakeLine

		public void TestAddNewStocktakeLine()
		{
			SetupEnvironmentData();
			LoadStocktakes();
			var area = "ANY";
			var pickMethod = "ANY";
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse1, Staff1);
			var packType = Part1.OP_StockKeepingUnit;
			var attr1 = string.Empty;
			var attr2 = string.Empty;
			var attr3 = string.Empty;
			var serial = string.Empty;
			var expiryDate = ZDate.Empty;
			var packingDate = ZDate.Empty;
			var palletId = "LP1";
			var count = 1;
			var originalStocktakeLinesCount = Stocktake1.Lines.Count;
			// to make sure that clientPK for a stocktake line is not taken automatically from the header
			// we will pass it directly and clear it from StockTake
			var clientPK = Stocktake1.WS_OH_Client;
			Stocktake1.WS_OH_Client = ZGuid.Empty;
			var line = StocktakeManager.AddNewStocktakeLine(Stocktake1.PK, Part1.PK, Warehouse1.DefaultLocation.PK,
				clientPK, packType, attr1, attr2, attr3, serial, expiryDate, packingDate, palletId, count,
				CodeLists.InventoryStatus.Codes.Available);
			var updatedStocktake = Factory.Load<WhsStocktake>(Stocktake1.PK);
			AssertEquals("The stocktake should have one more line", originalStocktakeLinesCount + 1,
				updatedStocktake.Lines.Count);
			var newLine = Stocktake1.Lines.First(l => l.PK == line.StocktakeLine.PK);
			AssertNotNull("The new line should be in the stocktake", newLine);
			AssertCountStocktakeLine("New line should be counted", count, ZDateTime.Now, Staff1, newLine);
			AssertEquals("Part", Part1.PK, newLine.SupplierPart.PK);
			AssertEquals("PackType", packType, newLine.WU_F3_NKPackType);
			AssertEquals("attr1", attr1, newLine.WU_PartAttrib1);
			AssertEquals("attr2", attr2, newLine.WU_PartAttrib2);
			AssertEquals("attr3", attr3, newLine.WU_PartAttrib3);
			AssertEquals("serial", serial, newLine.WU_SerialNumber);
			AssertEquals("expiryDate", expiryDate, newLine.WU_ExpiryDate);
			AssertEquals("packingDate", packingDate, newLine.WU_PackingDate);
			AssertEquals("palletId", palletId, newLine.WU_PalletID);
			AssertEquals("inventoryStatus", CodeLists.InventoryStatus.Codes.Available, newLine.WU_InventoryStatus);
			AssertEquals("WU_OH_Client", clientPK, newLine.WU_OH_Client);
			// Automatically set by add line
			AssertEquals("WU_WS", Stocktake1.PK, newLine.Stocktake.PK);
			AssertEquals("WU_LineNo", Stocktake1.Lines.Count, newLine.WU_LineNo);
			AssertEquals("WU_Status", CodeLists.StocktakeLineStatus.Codes.Open, newLine.WU_Status);
			AssertEquals("WU_IsManuallyAdded", true, newLine.WU_IsManuallyAdded);
			line = StocktakeManager.AddNewStocktakeLine(Stocktake1.PK, Part1.PK, Warehouse1.DefaultLocation.PK,
				clientPK, packType, attr1, attr2, attr3, serial, expiryDate, packingDate, palletId, count,
				CodeLists.InventoryStatus.Codes.Available);
			newLine = Stocktake1.Lines.First(l => l.PK == line.StocktakeLine.PK);
			AssertCountStocktakeLine("New line should be counted two times", count + count, ZDateTime.Now, Staff1,
				newLine);
		}

		public void TestAddNewStocktakeLine_AddAttributes()
		{
			SetupEnvironmentData();
			LoadStocktakes();
			var area = "ANY";
			var pickMethod = "ANY";
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse1, Staff1);
			var packType = Part1.OP_StockKeepingUnit;
			var attr1 = "ATT1";
			var attr2 = "ATT2";
			var attr3 = "ATT3";
			var serial = "SERIAL";
			var expiryDate = ZDate.Today;
			var packingDate = ZDate.Today.AddDays(1);
			var palletId = "LP1";
			var count = 1;
			var originalStocktakeLinesCount = Stocktake1.Lines.Count;
			// to make sure that clientPK for a stocktake line is not taken automatically from the header
			// we will pass it directly and clear it from StockTake
			var clientPK = Stocktake1.WS_OH_Client;
			Stocktake1.WS_OH_Client = ZGuid.Empty;
			var line = StocktakeManager.AddNewStocktakeLine(Stocktake1.PK, Part1.PK, Warehouse1.DefaultLocation.PK,
				clientPK, packType, attr1, attr2, attr3, serial, expiryDate, packingDate, palletId, count,
				CodeLists.InventoryStatus.Codes.Available);
			var updatedStocktake = Factory.Load<WhsStocktake>(Stocktake1.PK);
			AssertEquals("The stocktake should have one more line", originalStocktakeLinesCount + 1,
				updatedStocktake.Lines.Count);
			var newLine = Stocktake1.Lines.First(l => l.PK == line.StocktakeLine.PK);
			AssertNotNull("The new line should be in the stocktake", newLine);
			AssertCountStocktakeLine("New line should be counted", count, ZDateTime.Now, Staff1, newLine);
			AssertEquals("Part", Part1.PK, newLine.SupplierPart.PK);
			AssertEquals("PackType", packType, newLine.WU_F3_NKPackType);
			AssertEquals("attr1", attr1, newLine.WU_PartAttrib1);
			AssertEquals("attr2", attr2, newLine.WU_PartAttrib2);
			AssertEquals("attr3", attr3, newLine.WU_PartAttrib3);
			AssertEquals("serial", serial, newLine.WU_SerialNumber);
			AssertEquals("expiryDate", expiryDate, newLine.WU_ExpiryDate);
			AssertEquals("packingDate", packingDate, newLine.WU_PackingDate);
			AssertEquals("palletId", palletId, newLine.WU_PalletID);
			AssertEquals("inventoryStatus", CodeLists.InventoryStatus.Codes.Available, newLine.WU_InventoryStatus);
			AssertEquals("WU_OH_Client", clientPK, newLine.WU_OH_Client);
			// Automatically set by add line
			AssertEquals("WU_WS", Stocktake1.PK, newLine.Stocktake.PK);
			AssertEquals("WU_LineNo", Stocktake1.Lines.Count, newLine.WU_LineNo);
			AssertEquals("WU_Status", CodeLists.StocktakeLineStatus.Codes.Open, newLine.WU_Status);
			AssertEquals("WU_IsManuallyAdded", true, newLine.WU_IsManuallyAdded);
			line = StocktakeManager.AddNewStocktakeLine(Stocktake1.PK, Part1.PK, Warehouse1.DefaultLocation.PK,
				clientPK, packType, attr1, attr2, attr3, serial, expiryDate, packingDate, palletId, count,
				CodeLists.InventoryStatus.Codes.Available);
			newLine = Stocktake1.Lines.First(l => l.PK == line.StocktakeLine.PK);
			AssertCountStocktakeLine("New line should be counted two times", count + count, ZDateTime.Now, Staff1,
				newLine);
		}

		public void TestAddNewStocktakeLine_StocktakeUnfound()
		{
			SetupEnvironmentData();
			LoadStocktakes();
			var area = "ANY";
			var pickMethod = "ANY";
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse1, Staff1);
			var packUQ = Part1.OP_StockKeepingUnit;
			var attr1 = "ATT1";
			var attr2 = "ATT2";
			var attr3 = "ATT3";
			var serial = "SERIAL";
			var expiryDate = ZDate.Today;
			var packingDate = ZDate.Today.AddDays(1);
			var palletId = "LP1";
			var count = 1;
			var result = StocktakeManager.AddNewStocktakeLine(new ZGuid(), Part1.PK, Warehouse1.DefaultLocation.PK,
				ZGuid.Empty, packUQ, attr1, attr2, attr3, serial, expiryDate, packingDate, palletId, count,
				CodeLists.InventoryStatus.Codes.Available);
			AssertNull(result.StocktakeLine);
			AssertEquals("Stocktake not found", result.ErrorMessage);
		}

		public void TestAddNewStocktakeLine_PartUnfound()
		{
			SetupEnvironmentData();
			LoadStocktakes();
			var area = "ANY";
			var pickMethod = "ANY";
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse1, Staff1);
			var packUQ = Part1.OP_StockKeepingUnit;
			var attr1 = "ATT1";
			var attr2 = "ATT2";
			var attr3 = "ATT3";
			var serial = "SERIAL";
			var expiryDate = ZDate.Today;
			var packingDate = ZDate.Today.AddDays(1);
			var palletId = "LP1";
			var count = 1;
			var result = StocktakeManager.AddNewStocktakeLine(Stocktake1.PK, new ZGuid(), Warehouse1.DefaultLocation.PK,
				ZGuid.Empty, packUQ, attr1, attr2, attr3, serial, expiryDate, packingDate, palletId, count,
				CodeLists.InventoryStatus.Codes.Available);
			AssertNull(result.StocktakeLine);
			AssertEquals("Part not found", result.ErrorMessage);
		}

		public void TestAddNewStocktakeLine_InvalidStocktakeLine()
		{
			SetupEnvironmentData();
			LoadStocktakes();
			var area = "ANY";
			var pickMethod = "ANY";
			StocktakeManager = new StocktakeManager(Factory, area, pickMethod, Warehouse1, Staff1);
			var packType = Part1.OP_StockKeepingUnit;
			var attr1 = string.Empty;
			var attr2 = string.Empty;
			var attr3 = string.Empty;
			var serial = string.Empty;
			var expiryDate = ZDate.Empty;
			var packingDate = ZDate.Today;
			var palletId = "LP1";
			var count = 1;
			var result = StocktakeManager.AddNewStocktakeLine(Stocktake1.PK, Part1.PK, ZGuid.Empty, ZGuid.Empty,
				packType, attr1, attr2, attr3, serial, expiryDate, packingDate, palletId, count,
				CodeLists.InventoryStatus.Codes.Available);
			AssertNull(result.StocktakeLine);
			Assert(result.ErrorMessage.Contains("Invalid stocktake line"));
		}

		[TestDate(2012, 3, 15)]
		public void TestSetStocktakeLineEmptyLocationConfirm()
		{
			var stuff = Helper.CreateGlbStaff("ST", "ST");
			var whs = Helper.CreateWarehouse("Whs1");
			var area = Helper.CreateArea(whs, "AAA", AreaTypes.Codes.FreeStore);
			var row = Helper.CreateRowAndGenerateLocations(whs, "Row", 3, 1);
			row.Locations[0].WLV_WA_PickingArea = area.PK;
			row.Locations[1].WLV_WA_PickingArea = area.PK;
			row.Locations[2].WLV_WA_PickingArea = area.PK;
			Factory.Save();
			var stocktake = Helper.CreateWhsStocktake(null, whs);
			stocktake.WS_CountEmptyLocationsCategory = CountEmptyLocationCategory.Codes.OnlyCountEmptyLocations;
			stocktake.WS_StocktakeDate = ZDateTime.Today.AddDays(-1);
			stocktake.Load();
			Factory.Save();
			var pickArea = "ANY";
			var pickMethod = "ANY";
			var stManager = new StocktakeManager(Factory, pickArea, pickMethod, whs, stuff);
			AssertEquals("Staff 1 should find an available stocktake", true,
				stManager.FindAndAssignNextStocktake(ZString.Empty));
			AssertAssignedStocktake("should take first available stocktake", stocktake, stocktake.Lines.Count, whs,
				pickArea, pickMethod, stuff, stManager);
			var line1 = stocktake.Lines[0];
			stManager.SetStocktakeLineEmptyLocationConfirm(line1, true);
			AssertCountStocktakeLine("Stocktake line should be count", 0, ZDateTime.Now, stuff, line1);
			stManager.SetStocktakeLineEmptyLocationConfirm(line1, false);
			AssertEquals("Stocktake line should be deleted", true, line1.IsDeleted);
		}

		#endregion

		#region TestAddNewStocktakeLine_StocktakeWithCycleCount

		public void TestAddNewStocktakeLine_StocktakeWithCycleCountButNotClient()
		{
			var staff = Helper.CreateGlbStaff("ST", "ST");
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1, 0, "T1");
			var stocktake = Helper.CreateWhsStocktake(null, data.Whs1);
			stocktake.WS_StocktakeCycle = "T1";
			var stocktakeManager = new StocktakeManager(Factory, data.Whs1, staff);
			var newLine = stocktakeManager.AddNewStocktakeLine(stocktake.PK, data.Part1.PK,
				data.Whs1.DefaultLocation.PK, data.Org1.PK, data.Part1.OP_StockKeepingUnit, "", "", "", "", ZDate.Empty,
				ZDate.Empty, "", 1m, InventoryStatus.Codes.Available);
			AssertNull("No client is set on stocktake header.", stocktake.Client);
			AssertNotNull("New Line must be created.", newLine.StocktakeLine);
			Assert(newLine.ErrorMessage.IsNullOrEmpty());
			AssertEquals("Client must be set on stocktake line.", data.Org1, newLine.StocktakeLine.Client);
		}

		public void TestAddNewStocktakeLine_StocktakeWithCycleCountAndWithoutClient_InValidClient()
		{
			var staff = Helper.CreateGlbStaff("ST", "ST");
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1, 0, "T1");
			var stocktake = Helper.CreateWhsStocktake(null, data.Whs1);
			stocktake.WS_StocktakeCycle = "T1";
			var stocktakeManager = new StocktakeManager(Factory, data.Whs1, staff);
			var result = stocktakeManager.AddNewStocktakeLine(stocktake.PK, data.Part1.PK, data.Whs1.DefaultLocation.PK,
				ZGuid.Empty, data.Part1.OP_StockKeepingUnit, "", "", "", "", ZDate.Empty, ZDate.Empty, "", 1m,
				InventoryStatus.Codes.Available);
			AssertNull(result.StocktakeLine);
			AssertEquals("Client not found", result.ErrorMessage);
		}

		public void TestAddNewStocktakeLine_StocktakeWithCycleCountAndWithClient_InValidClient()
		{
			var staff = Helper.CreateGlbStaff("ST", "ST");
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1, 0, "T1");
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			stocktake.WS_StocktakeCycle = "T1";
			var stocktakeManager = new StocktakeManager(Factory, data.Whs1, staff);
			var newLine = stocktakeManager.AddNewStocktakeLine(stocktake.PK, data.Part1.PK,
				data.Whs1.DefaultLocation.PK, ZGuid.Empty, data.Part1.OP_StockKeepingUnit, "", "", "", "", ZDate.Empty,
				ZDate.Empty, "", 1m, InventoryStatus.Codes.Available);
			AssertEquals(data.Org1, stocktake.Client);
			AssertNotNull("New Line must be created.", newLine.StocktakeLine);
			Assert(newLine.ErrorMessage.IsNullOrEmpty());
			AssertEquals("Stocktake header client must be used.", data.Org1, newLine.StocktakeLine.Client);
		}

		public void
			TestAddNewStocktakeLine_StocktakeWithCycleCount_TryingToAddStocktakeLineWithAProductInDifferentCycle()
		{
			var staff = Helper.CreateGlbStaff("ST", "ST");
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductParamsByWhsAndClient(data.Part2, data.Org1, data.Whs1, 0, "T2");
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			stocktake.WS_StocktakeCycle = "T2";
			var stocktakeManager = new StocktakeManager(Factory, data.Whs1, staff);
			var newLine = stocktakeManager.AddNewStocktakeLine(stocktake.PK, data.Part1.PK,
				data.Whs1.DefaultLocation.PK, ZGuid.Empty, data.Part1.OP_StockKeepingUnit, "", "", "", "", ZDate.Empty,
				ZDate.Empty, "", 1m, InventoryStatus.Codes.Available);
			AssertEquals(data.Org1, stocktake.Client);
			AssertNotNull("New Line must be created.", newLine);
			Assert(newLine.ErrorMessage.IsNullOrEmpty());
			AssertEquals("Stocktake header client must be used.", data.Org1, newLine.StocktakeLine.Client);
		}

		#endregion

		#region Implementation

		void AssertAssignedStocktake(string message, WhsStocktake expectedStocktake, int expectedLinesCount,
			WhsWarehouse expectedWarehouse, ZString expectedArea, ZString expectedPickMethod, GlbStaff expectedStaff,
			StocktakeManager actualStocktakeManager)
		{
			var actualStocktake = actualStocktakeManager.Stocktake;
			AssertEquals(message, expectedStocktake, actualStocktake);
			AssertEquals(message + "- Status", StocktakeStatus.Codes.Loaded, actualStocktake.WS_StocktakeStatus);
			AssertEquals(message + "- Warehouse", expectedWarehouse.PK, actualStocktake.WS_WW_Whs);
			var actualLines = actualStocktakeManager.LinesToCount;
			AssertEquals(message + "- Lines Count", expectedLinesCount, actualLines.Count);
			foreach (WhsStocktakeLine line in actualLines)
			{
				AssertAssignedStocktakeLine(message, expectedWarehouse, expectedArea, expectedPickMethod, expectedStaff,
					line);
			}
		}

		void AssertAssignedStocktakeLine(string message, WhsWarehouse expectedWarehouse, ZString expectedArea,
			ZString expectedPickMethod, GlbStaff expectedStaff, WhsStocktakeLine actualStocktakeLine)
		{
			AssertEquals(message + "- line - Verified by", expectedStaff.GS_Code,
				actualStocktakeLine.WU_GS_NKVerifiedBy);
			AssertEquals(message + "- line - Verified date", ZDateTime.Empty, actualStocktakeLine.WU_DateVerified);
			AssertEquals(message + "- line - Status", StocktakeLineStatus.Codes.Open, actualStocktakeLine.WU_Status);
			var location = actualStocktakeLine.Location;
			if (expectedArea != "ANY" && !expectedArea.IsEmpty)
			{
				AssertEquals(message + "- line - Area", expectedArea.ToUpper(), location.PickingArea.WA_Name.ToUpper());
			}

			if (expectedPickMethod != "ANY" && !expectedPickMethod.IsEmpty)
			{
				AssertEquals(message + "- line - Pick Method", expectedPickMethod.ToUpper(),
					location.WLV_PickMethod.ToUpper());
			}

			AssertEquals(message + "- line - Warehouse", expectedWarehouse, location.Row.Warehouse);
		}

		void AssertCountStocktakeLine(string message, ZDecimal expectedCount, ZDateTime expectedVerifiedDate,
			GlbStaff expectedVerifiedBy, WhsStocktakeLine actualStocktakeLine)
		{
			AssertEquals(message + " - Count", expectedCount, actualStocktakeLine.CurrentCount);
			// use [TestDate]
			AssertDateTimeWithinOneSecond(message + " - Verified Date", expectedVerifiedDate.ToDateTime(),
				actualStocktakeLine.WU_DateVerified.ToDateTime());
			AssertEquals(message + " - Verified By", expectedVerifiedBy, actualStocktakeLine.VerifiedBy);
		}

		protected void SetupEnvironmentData()
		{
			// Warehouses / Areas / Locations
			Warehouse1 = Helper.CreateWarehouse("Whs1");
			Warehouse1Area1 = Helper.CreateArea(Warehouse1, "A1", AreaTypes.Codes.FreeStore);
			Warehouse1Area2 = Helper.CreateArea(Warehouse1, "A2", AreaTypes.Codes.FreeStore);
			var row11 = Helper.CreateRowAndGenerateLocations(Warehouse1, "Row11", 3, 1);
			var row12 = Helper.CreateRowAndGenerateLocations(Warehouse1, "Row12", 3, 1);
			row11.Locations[0].WLV_WA_PickingArea = Warehouse1Area1.PK;
			row11.Locations[1].WLV_WA_PickingArea = Warehouse1Area1.PK;
			row11.Locations[2].WLV_WA_PickingArea = Warehouse1Area1.PK;
			row12.Locations[0].WLV_WA_PickingArea = Warehouse1Area2.PK;
			row12.Locations[1].WLV_WA_PickingArea = Warehouse1Area2.PK;
			row12.Locations[2].WLV_WA_PickingArea = Warehouse1Area2.PK;
			Warehouse2 = Helper.CreateWarehouse("Whs2");
			Warehouse2Area1 = Helper.CreateArea(Warehouse2, "A1", AreaTypes.Codes.FreeStore);
			Warehouse2Area2 = Helper.CreateArea(Warehouse2, "A2", AreaTypes.Codes.FreeStore);
			Warehouse2Area3 = Helper.CreateArea(Warehouse2, "A3", AreaTypes.Codes.FreeStore);
			var row2 = Helper.CreateRowAndGenerateLocations(Warehouse2, "Row2", 3, 1);
			row2.Locations[0].WLV_WA_PickingArea = Warehouse2Area1.PK;
			row2.Locations[1].WLV_WA_PickingArea = Warehouse2Area2.PK;
			row2.Locations[2].WLV_WA_PickingArea = Warehouse2Area3.PK;
			// Client
			var client = Helper.CreateClient("Cient");
			// Parts
			Part1 = Helper.CreateProduct(client, "Part1");
			Part2 = Helper.CreateProduct(client, "Part2");
			Part3 = Helper.CreateProduct(client, "Part3");
			// Receives
			var receive1 = Helper.CreateWhsReceive(client, Warehouse1, "Receive1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, Part1, 100m, row11.Locations[2]);
			Helper.CreateWhsReceiveInventoryLine(receive1, Part2, 100m, row11.Locations[1]);
			Helper.CreateWhsReceiveInventoryLine(receive1, Part3, 100m, row11.Locations[0]);
			receive1.FinaliseDocket();
			AssertEquals(true, receive1.IsFinalised);
			var receive2 = Helper.CreateWhsReceive(client, Warehouse2, "Receive2", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive2, Part1, 200m, row2.Locations[2]);
			Helper.CreateWhsReceiveInventoryLine(receive2, Part2, 200m, row2.Locations[1]);
			Helper.CreateWhsReceiveInventoryLine(receive2, Part3, 200m, row2.Locations[0]);
			receive2.FinaliseDocket();
			AssertEquals(true, receive2.IsFinalised);
			var receive3 = Helper.CreateWhsReceive(client, Warehouse1, "Receive3", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive3, Part1, 300m, row12.Locations[2]);
			Helper.CreateWhsReceiveInventoryLine(receive3, Part2, 300m, row12.Locations[1]);
			Helper.CreateWhsReceiveInventoryLine(receive3, Part3, 300m, row12.Locations[0]);
			receive3.FinaliseDocket();
			AssertEquals(true, receive3.IsFinalised);
			// Stocktakes
			Stocktake1 = Helper.CreateWhsStocktake(client, Warehouse1);
			Stocktake1.WS_StocktakeDate = ZDateTime.Today.AddDays(-1);
			Stocktake2 = Helper.CreateWhsStocktake(client, Warehouse1);
			Stocktake2.WS_StocktakeDate = ZDateTime.Today;
			Stocktake3 = Helper.CreateWhsStocktake(client, Warehouse2);
			// Staff
			Staff1 = Helper.CreateGlbStaff("ST1", "ST1");
			Staff2 = Helper.CreateGlbStaff("ST2", "ST2");
			Factory.Save();
		}

		void LoadStocktakes()
		{
			Stocktake1.Load();
			Stocktake2.Load();
			Stocktake3.Load();
			Factory.Save();
		}

		void UpdateAreaForStocktake(ZGuid areaPK, WhsStocktake stocktake)
		{
			foreach (var line in stocktake.Lines)
			{
				line.Location.WLV_WA_PickingArea = areaPK;
			}

			Factory.Save();
		}

		void UpdatePickMethodForStocktake(ZString pickMethod, WhsStocktake stocktake)
		{
			foreach (var line in stocktake.Lines)
			{
				line.Location.WLV_PickMethod = pickMethod;
			}

			Factory.Save();
		}

		void UnassignOpenLines(WhsStocktake stocktake)
		{
			foreach (var line in stocktake.Lines)
			{
				if (!line.IsClosed)
				{
					line.WU_GS_NKVerifiedBy = ZString.Empty;
				}
			}

			Factory.Save();
		}

		StocktakeManager StocktakeManager;
		WhsWarehouse Warehouse1;
		WhsWarehouse Warehouse2;
		WhsArea Warehouse1Area1;
		WhsArea Warehouse1Area2;
		WhsArea Warehouse2Area1;
		WhsArea Warehouse2Area2;
		WhsArea Warehouse2Area3;
		WhsStocktake Stocktake1;
		WhsStocktake Stocktake2;
		WhsStocktake Stocktake3;
		OrgSupplierPart Part1;
		OrgSupplierPart Part2;
		OrgSupplierPart Part3;
		GlbStaff Staff1;
		GlbStaff Staff2;

		#endregion
	}
}
