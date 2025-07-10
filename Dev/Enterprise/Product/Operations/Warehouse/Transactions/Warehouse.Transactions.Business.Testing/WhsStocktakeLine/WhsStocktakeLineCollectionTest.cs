using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsStocktakeLineCollection))]
	internal class WhsStocktakeLineCollectionTest : WhsActiveBusinessObjectCollectionTestCase<WhsStocktakeLineCollection>
	{
		#region TestSetDefalutsForNewChild

		public void TestSetDefalutsForNewChild()
		{
			// setup test data
			var data = new TestDataSimpleEnvironment(Factory);
			var locationA1 = data.Whs1.FindLocation("A");
			// Setup stocktake and a line

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1, locationA1);
			var autoAddedLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation);
			autoAddedLine.WU_LineNo = 1;

			// Add a new line manually

			var newLine = stocktake.Lines.AddNew();

			AssertEquals(2, newLine.WU_LineNo);
			AssertEquals(ZGuid.Empty, newLine.WU_OP);
			AssertEquals(locationA1.PK, newLine.WU_WL);
			AssertEquals(data.Org1, newLine.Client);
			AssertEquals(new ZDecimal(0), newLine.WU_SystemUnits);
			AssertEquals(StocktakeLineStatus.Codes.Open, newLine.WU_Status);
			AssertEquals(data.Whs1.PK, newLine.LocationWhsGuid);
			AssertEquals(true, newLine.WU_IsManuallyAdded);
			AssertEquals("", newLine.WU_LineComment);
			AssertEquals(InventoryStatus.Codes.Available, newLine.WU_InventoryStatus);
		}

		public void TestSetDefalutsForNewChild_MaximumCountColumn()
		{
			// setup test data

			var data = new TestDataSimpleEnvironment(Factory);

			var stocktakeWithMaximumCountOne = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var stocktakeWithMaximumCountTwo = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var stocktakeWithMaximumCountThree = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);

			var autoAddedLineWithCountOne = Helper.CreateWhsStocktakeLine(stocktakeWithMaximumCountOne, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, 1, StocktakeLineStatus.Codes.Open);
			var autoAddedLineWithCountTwo = Helper.CreateWhsStocktakeLine(stocktakeWithMaximumCountTwo, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, 2, StocktakeLineStatus.Codes.Open);
			var autoAddedLineWithCountThree = Helper.CreateWhsStocktakeLine(stocktakeWithMaximumCountThree, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, 3, StocktakeLineStatus.Codes.Open);

			var newLineWithCountOne = stocktakeWithMaximumCountOne.Lines.AddNew();
			var newLineWithCountTwo = stocktakeWithMaximumCountTwo.Lines.AddNew();
			var newLineWithCountThree = stocktakeWithMaximumCountThree.Lines.AddNew();

			AssertEquals(new ZByte(1), newLineWithCountOne.WU_TotalCounts);
			AssertEquals(new ZDecimal(0), newLineWithCountOne.WU_LastCount);

			AssertEquals(new ZByte(2), newLineWithCountTwo.WU_TotalCounts);
			AssertEquals(new ZDecimal(0), newLineWithCountOne.WU_Count2);

			AssertEquals(new ZByte(3), newLineWithCountThree.WU_TotalCounts);
			AssertEquals(new ZDecimal(0), newLineWithCountThree.WU_Count3);
		}

		#endregion

		#region TestGetDuplicateStocktakeLine

		[TestDate(2012, 03, 01)]
		public void TestGetDuplicateStocktakeLine()
		{
			// setup test data
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			// Setup stocktake and a line

			var stocktake = Helper.CreateWhsStocktake(null, data.Whs1, data.Part1);
			var openLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, today.AddDays(2), today.AddDays(3), "PA1", "PA2", "PA3", StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);
			var closedLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, today.AddDays(3), today.AddDays(4), "PA2", "PA3", "PA4", StocktakeLineStatus.Codes.Closed, InventoryStatus.Codes.Available);
			var lineWithoutMatchingLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);
			var lineWithMatchingOpenLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, today.AddDays(2), today.AddDays(3), "PA1", "PA2", "PA3", StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);
			var lineWithMatchingClosedLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, today.AddDays(3), today.AddDays(4), "PA2", "PA3", "PA4", StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);

			AssertEquals(lineWithMatchingOpenLine, stocktake.Lines.GetDuplicateStocktakeLine(openLine));
			AssertEquals(lineWithMatchingClosedLine, stocktake.Lines.GetDuplicateStocktakeLine(closedLine));

			AssertNull(stocktake.Lines.GetDuplicateStocktakeLine(lineWithoutMatchingLine));
			AssertEquals(openLine, stocktake.Lines.GetDuplicateStocktakeLine(lineWithMatchingOpenLine));
			AssertNull(stocktake.Lines.GetDuplicateStocktakeLine(lineWithMatchingClosedLine));

			var otherClient = Helper.CreateClient();
			openLine.WU_OH_Client = otherClient.PK;
			AssertNull("If client is different, lines should not match", stocktake.Lines.GetDuplicateStocktakeLine(lineWithMatchingOpenLine));
		}

		public void TestGetDuplicateStocktakeLine_WithDifferentPalletId()
		{
			// setup test data

			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			// Setup stocktake and a line

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);

			var openLineWithNoPalletId = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available); // openLineWithNoPalletId.WU_PalletID = ""
			var duplicatedOpenLineWithNoPalletId = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available); // duplicatedOpenLineWithNoPalletId = ""
			var openLineWithPalletId = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);
			var duplicatedOpenLineWithPalletId = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);
			var openLineWithNoMatchingPalletId = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);

			var closeLineWithNoPalletId = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Closed, InventoryStatus.Codes.Available); // closeLineWithNoPalletId.WU_PalletID=""
			var closeLineWithPalletId = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Closed, InventoryStatus.Codes.Available);

			openLineWithPalletId.WU_PalletID = "1";
			duplicatedOpenLineWithPalletId.WU_PalletID = "1";
			openLineWithNoMatchingPalletId.WU_PalletID = "2";
			closeLineWithPalletId.WU_PalletID = "2";

			AssertEquals(duplicatedOpenLineWithNoPalletId, stocktake.Lines.GetDuplicateStocktakeLine(openLineWithNoPalletId));
			AssertEquals(openLineWithNoPalletId, stocktake.Lines.GetDuplicateStocktakeLine(duplicatedOpenLineWithNoPalletId));
			AssertEquals(duplicatedOpenLineWithPalletId, stocktake.Lines.GetDuplicateStocktakeLine(openLineWithPalletId));
			AssertEquals(openLineWithPalletId, stocktake.Lines.GetDuplicateStocktakeLine(duplicatedOpenLineWithPalletId));
			AssertNull(stocktake.Lines.GetDuplicateStocktakeLine(openLineWithNoMatchingPalletId));
		}

		public void TestGetDuplicateStocktakeLine_WithInventoryStatus()
		{
			// setup test data

			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			// Setup stocktake and a line

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);

			var openLineWithInventoryStatus = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);
			var openLineWithMatchingInventoryStatus = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);
			var openLineWithoutMatchingInventoryStatus = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Held);
			var closedLineWithMatchingInventoryStatus = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Closed, InventoryStatus.Codes.Held);

			AssertEquals(openLineWithMatchingInventoryStatus, stocktake.Lines.GetDuplicateStocktakeLine(openLineWithInventoryStatus));
			AssertEquals(openLineWithInventoryStatus, stocktake.Lines.GetDuplicateStocktakeLine(openLineWithMatchingInventoryStatus));
			AssertNull(stocktake.Lines.GetDuplicateStocktakeLine(openLineWithoutMatchingInventoryStatus));
		}

		#endregion

		#region TestValidateDuplicateLine

		public void TestValidateDuplicateLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);

			var openLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);
			var duplicatedManuallyAddedLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available, true);
			var manuallyAddedLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part2, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available, true);
			var manuallyAddedLineDuplicatedWithAnotherManuallyAddedLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part2, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available, true);
			var uniqueLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part2, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Held);

			AssertNoErrors(openLine);
			AssertNoErrors(duplicatedManuallyAddedLine);
			AssertNoErrors(manuallyAddedLine);
			AssertNoErrors(manuallyAddedLineDuplicatedWithAnotherManuallyAddedLine);
			AssertNoErrors(uniqueLine);

			stocktake.Lines.ValidateDuplicateLine();

			AssertNoRowErrors(openLine);
			AssertHasRowError(duplicatedManuallyAddedLine, "Product P1 in Location A with status AVL is already on this stocktake on Line 1. You must edit the existing stock take line. If you cannot see the line clear all filters.");
			AssertHasRowError(manuallyAddedLine, "Product P2 in Location A with status AVL is already on this stocktake on Line 4. You must edit the existing stock take line. If you cannot see the line clear all filters.");
			AssertHasRowError(manuallyAddedLineDuplicatedWithAnotherManuallyAddedLine, "Product P2 in Location A with status AVL is already on this stocktake on Line 3. You must edit the existing stock take line. If you cannot see the line clear all filters.");
			AssertNoRowErrors(uniqueLine);
		}

		#endregion

		#region TestLocationStringSortedProperly

		public void TestLocationStringSortedProperly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "X", 10, 10, 10);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "Y", 10, 10, 10);
			Factory.Save();

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var lineX_1_1 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.FindLocation("X-1-1-1"));
			var lineX_1_1_10 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.FindLocation("X-1-1-10"));
			var lineX_1_1_2 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.FindLocation("X-1-1-2"));
			var lineX_1_10 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.FindLocation("X-1-10-1"));
			var lineX_1_2 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.FindLocation("X-1-2-1"));
			var lineX_10_1 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.FindLocation("X-10-1-1"));
			var lineX_2_1 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.FindLocation("X-2-1-1"));
			var lineY_1_1 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.FindLocation("Y-1-1-1"));

			// Check sort ascending
			stocktake.Lines.ApplySort(WhsStocktakeLine.Schema.LocationString, ListSortDirection.Ascending);
			AssertStockTakeLinesInExactOrder(new[] { lineX_1_1, lineX_1_1_2, lineX_1_1_10, lineX_1_2, lineX_1_10, lineX_2_1, lineX_10_1, lineY_1_1 }, stocktake.Lines);

			// Check sort descending
			stocktake.Lines.ApplySort(WhsStocktakeLine.Schema.LocationString, ListSortDirection.Descending);
			AssertStockTakeLinesInExactOrder(new[] { lineY_1_1, lineX_10_1, lineX_2_1, lineX_1_10, lineX_1_2, lineX_1_1_10, lineX_1_1_2, lineX_1_1 }, stocktake.Lines);
		}

		public void TestLocationStringSortedProperly_RowPathSequence()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "10", 2, 1, 1, 1);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "20", 2, 1, 1, 3);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "30", 2, 1, 1, 2);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "100", 2, 1, 1, 10);
			Factory.Save();

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var line10_1 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.FindLocation("10-1"));
			var line10_2 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.FindLocation("10-2"));
			var line20_1 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.FindLocation("20-1"));
			var line20_2 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.FindLocation("20-2"));
			var line30_1 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.FindLocation("30-1"));
			var line30_2 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.FindLocation("30-2"));
			var line100_1 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.FindLocation("100-1"));
			var line100_2 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.FindLocation("100-2"));

			// Check sort ascending -- Should just sort row like a string
			stocktake.Lines.ApplySort(WhsStocktakeLine.Schema.LocationString, ListSortDirection.Ascending);
			AssertStockTakeLinesInExactOrder(new[] { line10_1, line10_2, line100_1, line100_2, line20_1, line20_2, line30_1, line30_2 }, stocktake.Lines);

			// Check sort descending -- Should just sort row like a string
			stocktake.Lines.ApplySort(WhsStocktakeLine.Schema.LocationString, ListSortDirection.Descending);
			AssertStockTakeLinesInExactOrder(new[] { line30_2, line30_1, line20_2, line20_1, line100_2, line100_1, line10_2, line10_1 }, stocktake.Lines);
		}

		void AssertStockTakeLinesInExactOrder(WhsStocktakeLine[] expected, WhsStocktakeLineCollection result)
		{
			AssertEquals(expected.Length, result.Count);
			int index = 0;
			foreach (var expectedStockTakeLine in expected)
			{
				AssertEquals(string.Format("Unexpected line at index {0}", index), expectedStockTakeLine, result[index++]);
			}
		}

		#endregion

		#region Implementation

		protected override WhsStocktakeLineCollection GetCollectionToTest()
		{
			var stocktake = Factory.New<WhsStocktake>();
			return new WhsStocktakeLineCollection(stocktake);
		}

		protected new WhsTestHelperFunctions Helper { get { return new WhsTestHelperFunctions(Factory); } }

		#endregion
	}
}
