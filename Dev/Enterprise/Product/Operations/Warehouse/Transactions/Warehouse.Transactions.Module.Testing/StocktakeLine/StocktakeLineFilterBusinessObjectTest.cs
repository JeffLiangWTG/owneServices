using System.Linq;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(StocktakeLineFilterBusinessObject))]
	public class StocktakeLineFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region TestRowFilter

		public void TestRowFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var row = data.Whs1.Rows[0];
			var row1 = Helper.CreateRowAndGenerateLocations(data.Whs1, "ROW1", 10, 10);
			var row2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "ROW2", 5, 5);

			// Setup one stocktake and three lines.

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);
			var lineInRow1 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, row1.Locations[0]); // Row1 on line
			var lineInRow2 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, row2.Locations[0]); // Row2 on line
			var lineWithNoRow = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, row.Locations[0]);

			AddStocktakeLinesToScope(Asserter, lineInRow1, lineInRow2, lineWithNoRow);

			Factory.Save(); // For Db only query.

			// test the filter

			var filter = GetStocktakeLineFilterBusinessObject(stocktake);
			var rowFilter = (ModuleTextFilter)filter[StocktakeLineFilterBusinessObject.FilterNames.Row];
			AssertEquals("Row filter category should be Locations.", rowFilter.Category, FilterCategories.Locations);
			Asserter.AssertMatches("Since the filter is empty it should return all stocktake lines.", rowFilter, lineInRow1, lineInRow2, lineWithNoRow);

			rowFilter.Property = "ROW";
			Asserter.AssertMatches("Since the filter value is Row it should return lines which have got Rows starts with 'Row'.", rowFilter, lineInRow1, lineInRow2);

			rowFilter.Property = "ROW2";
			Asserter.AssertMatches("Since the filter value is Row2 it should return lines in Row2.", rowFilter, lineInRow2);

			rowFilter.Property = "ROW4";
			Asserter.AssertMatches("There are no stocktake items associated with ROW4.", rowFilter);
		}

		#endregion

		#region TestAreaName

		public void TestAreaName()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var area1 = Helper.CreateArea(data.Whs1, "Area1");
			var area2 = Helper.CreateArea(data.Whs1, "Area2");
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			locations[0].WLV_WA_PickingArea = area1.PK;
			locations[1].WLV_WA_PickingArea = area2.PK;

			// Setup stocktake and lines

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);
			var lineInArea1 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, locations[0]); // Area1 on line
			var lineInArea2 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, locations[1]); // Area2 on line
			var lineWithNoArea = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, locations[2]);

			AddStocktakeLinesToScope(Asserter, lineInArea1, lineInArea2, lineWithNoArea);

			Factory.Save();

			// test the filter

			var filter = GetStocktakeLineFilterBusinessObject(stocktake);
			var areaFilter = (ModuleTextFilter)filter[StocktakeLineFilterBusinessObject.FilterNames.PickArea];
			AssertEquals("Area filter category should be Locations.", areaFilter.Category, FilterCategories.Locations);
			Asserter.AssertMatches("Since the filter is empty it should return all stocktake lines.", areaFilter, lineInArea1, lineInArea2, lineWithNoArea);

			areaFilter.Property = "Area";
			Asserter.AssertMatches("Since the filter value is Area it should return lines which have got Area name starts with 'Area'.", areaFilter, lineInArea1, lineInArea2);

			areaFilter.Property = "Area1";
			Asserter.AssertMatches("Since the filter value is Area1 it should return lines in Area1.", areaFilter, lineInArea1);

			areaFilter.Property = "Area3";
			Asserter.AssertMatches("There are no stocktake items associated with Area3.", areaFilter);
		}

		#endregion

		#region TestPickMethodFilter

		public void TestPickMethodFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			locations[0].WLV_PickMethod = "M1";
			//locations[1] doesn't have a pick method

			// Setup stocktake and lines

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);
			var lineWithPickMethod = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, locations[0]); // Pick method M1 
			var lineWithNoPickMethod = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, locations[1]);

			AddStocktakeLinesToScope(Asserter, lineWithPickMethod, lineWithNoPickMethod);

			Factory.Save();

			// test the filter

			var filter = GetStocktakeLineFilterBusinessObject(stocktake);
			var pickMethodFilter = (ModuleTextFilter)filter[StocktakeLineFilterBusinessObject.FilterNames.PickMethod];
			AssertEquals("PickMethod filter category should be StatusAndFlags.", pickMethodFilter.Category, FilterCategories.StatusAndFlags);
			Asserter.AssertMatches("Since the filter is empty it should return all stocktake lines.", pickMethodFilter, lineWithPickMethod, lineWithNoPickMethod);

			pickMethodFilter.Property = "M1";
			Asserter.AssertMatches("Since the filter value is M1 it should line with M1 pick method.", pickMethodFilter, lineWithPickMethod);

			pickMethodFilter.Property = "M3";
			Asserter.AssertMatches("There are no stocktake items associated with M3.", pickMethodFilter);

			pickMethodFilter.Property = "ANY";
			Asserter.AssertMatches("All stocktake items are returned.", pickMethodFilter, lineWithPickMethod, lineWithNoPickMethod);
		}

		#endregion

		#region TestStatusFilter

		public void TestStatusFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			// Setup stocktake and lines

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);
			var openLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, locations[0]);
			var closedLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, locations[1]);
			var emptyLine = Helper.CreateEmptyWhsStocktakeLine(stocktake, locations[3]);

			openLine.WU_Status = StocktakeLineStatus.Codes.Open;
			closedLine.WU_Status = StocktakeLineStatus.Codes.Closed;

			AddStocktakeLinesToScope(Asserter, openLine, closedLine, emptyLine);

			// test the filter

			var filter = GetStocktakeLineFilterBusinessObject(stocktake);
			var statusFilter = (ModuleTextFilter)filter[StocktakeLineFilterBusinessObject.FilterNames.Status];
			AssertEquals("Status filter category should be Locations.", statusFilter.Category, FilterCategories.StatusAndFlags);
			Asserter.AssertMatches("Since the filter is empty it should return all stocktake lines.", statusFilter, openLine, closedLine, emptyLine);

			statusFilter.Property = StocktakeLineStatus.Codes.Closed;
			Asserter.AssertMatches("Since the filter value is Closed it should return closed lines.", statusFilter, closedLine);

			statusFilter.Property = StocktakeLineStatus.Codes.Open;
			Asserter.AssertMatches("Since the filter value is Open it should be only open lines.", statusFilter, openLine);

			statusFilter.Property = StocktakeLineStatus.Codes.Empty;
			Asserter.AssertMatches("Since the filter value is Empty it should be only empty lines.", statusFilter, emptyLine);

			statusFilter.Property = StocktakeLineStatus.Codes.All;
			Asserter.AssertMatches("Since the filter value is Open it should return all lines.", statusFilter, openLine, closedLine, emptyLine);
		}

		#endregion

		#region TestEmptyLocationFilter

		public void TestEmptyLocationFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			// Setup stocktake and lines

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);
			stocktake.WS_CountEmptyLocationsCategory = CountEmptyLocationCategory.Codes.IncludeEmptyLocations;
			var openLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, locations[0]);
			var closedLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, locations[1]);
			var emptyLine = Helper.CreateEmptyWhsStocktakeLine(stocktake, locations[3]);

			openLine.WU_Status = StocktakeLineStatus.Codes.Open;
			closedLine.WU_Status = StocktakeLineStatus.Codes.Closed;

			AddStocktakeLinesToScope(Asserter, openLine, closedLine, emptyLine);

			// test the filter
			var filter = GetStocktakeLineFilterBusinessObject(stocktake);
			var emptyLocationFilter = (ModuleFlagsFilter)filter[StocktakeLineFilterBusinessObject.FilterNames.EmptyLocations];

			emptyLocationFilter.Property0 = ZBool.True;
			Asserter.AssertMatches("Since the filter value is True it should return Opened line with last count zero.", emptyLocationFilter, emptyLine);

			emptyLocationFilter.Property0 = ZBool.False;
			Asserter.AssertMatches("Since the filter value is False it should return all lines.", emptyLocationFilter, openLine, closedLine, emptyLine);
		}

		#endregion

		#region TestNotCountedFilter

		#region TestNotCountedFilterType

		public void TestNotCountedFilterType()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);
			var filter = GetStocktakeLineFilterBusinessObject(stocktake);
			var notCountedFilter = (ModuleFlagsFilter)filter[StocktakeLineFilterBusinessObject.FilterNames.NotCounted];
			AssertEquals("Count filter category should be NumbersAndReferences.", notCountedFilter.Category, FilterCategories.NumbersAndReferences);
		}

		#endregion

		#region TestNotCountedFilter_CountOne

		public void TestNotCountedFilter_CountOne()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// Setup stocktakes and lines

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);

			var openLineWithZeroCountOne = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, 1, StocktakeLineStatus.Codes.Open);
			var closedLineWithZeroCountOne = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, 1, StocktakeLineStatus.Codes.Closed);
			var openLineWithNonZeroCountOne = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 1, 1, StocktakeLineStatus.Codes.Open);
			var closedLineWithNonZeroCountOne = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 1, 1, StocktakeLineStatus.Codes.Closed);

			// test the filter

			AddStocktakeLinesToScope(Asserter, openLineWithZeroCountOne, closedLineWithZeroCountOne, openLineWithNonZeroCountOne, closedLineWithNonZeroCountOne);
			var filter = GetStocktakeLineFilterBusinessObject(stocktake);
			var notCountedFilter = (ModuleFlagsFilter)filter[StocktakeLineFilterBusinessObject.FilterNames.NotCounted];

			notCountedFilter.Property0 = ZBool.True;
			Asserter.AssertMatches("Since the filter value is True it should return Opened line with last count zero.", notCountedFilter, openLineWithZeroCountOne);

			notCountedFilter.Property0 = ZBool.False;
			Asserter.AssertMatches("Since the filter value is False it should return all lines.", notCountedFilter, openLineWithZeroCountOne, closedLineWithZeroCountOne, openLineWithNonZeroCountOne, closedLineWithNonZeroCountOne);
		}

		#endregion

		#region TestNotCountedFilter_CountTwo

		public void TestNotCountedFilter_CountTwo()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// Setup stocktakes and lines

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);

			var openLineWithNonZeroCountOneAndZeroCountTwo = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, 2, StocktakeLineStatus.Codes.Open);
			openLineWithNonZeroCountOneAndZeroCountTwo.WU_LastCount = 1;

			var openLineWithZeroCountOneAndNonZeroCountTwo = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 1, 2, StocktakeLineStatus.Codes.Open);
			openLineWithZeroCountOneAndNonZeroCountTwo.WU_LastCount = 0;

			var openLineWithNonZeroCountOneAndNonZeroCountTwo = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 1, 2, StocktakeLineStatus.Codes.Open);
			openLineWithNonZeroCountOneAndNonZeroCountTwo.WU_LastCount = 1;

			var closedLineWithZeroCountTwo = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, 2, StocktakeLineStatus.Codes.Closed);

			// test the filter

			AddStocktakeLinesToScope(Asserter, openLineWithNonZeroCountOneAndZeroCountTwo, openLineWithZeroCountOneAndNonZeroCountTwo, openLineWithNonZeroCountOneAndNonZeroCountTwo, closedLineWithZeroCountTwo);

			var filter = GetStocktakeLineFilterBusinessObject(stocktake);
			var notCountedFilter = (ModuleFlagsFilter)filter[StocktakeLineFilterBusinessObject.FilterNames.NotCounted];

			notCountedFilter.Property0 = ZBool.True;
			Asserter.AssertMatches("Since the filter value is True it should return Opened line with last count zero.", notCountedFilter,
				openLineWithNonZeroCountOneAndZeroCountTwo, openLineWithZeroCountOneAndNonZeroCountTwo);

			notCountedFilter.Property0 = ZBool.False;
			Asserter.AssertMatches("Since the filter value is False it should return all lines.", notCountedFilter,
				openLineWithNonZeroCountOneAndZeroCountTwo, openLineWithZeroCountOneAndNonZeroCountTwo,
				openLineWithNonZeroCountOneAndNonZeroCountTwo, closedLineWithZeroCountTwo);
		}

		#endregion

		#region TestNotCountedFilter_CountThree

		public void TestNotCountedFilter_CountThree()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// Setup stocktakes and lines

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);

			var openLineWithZeroCountThree = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, 3, StocktakeLineStatus.Codes.Open);
			var closedLineWithZeroCountThree = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, 3, StocktakeLineStatus.Codes.Closed);
			var closedLineWithNonZeroCountThree = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 1, 3, StocktakeLineStatus.Codes.Closed);

			var openLineWithZeroCountOneZeroCountTwoNonZeroCountThree = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 1, 3, StocktakeLineStatus.Codes.Open);
			var openLineWithNonZeroCountOneZeroCountTwoNonZeroCountThree = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 1, 3, StocktakeLineStatus.Codes.Open);
			var openLineWithZeroCountOneNonZeroCountTwoNonZeroCountThree = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 1, 3, StocktakeLineStatus.Codes.Open);
			var openLineWithNonZeroCountOneNonZeroCountTwoNonZeroCountThree = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 1, 3, StocktakeLineStatus.Codes.Open);

			openLineWithZeroCountOneZeroCountTwoNonZeroCountThree.WU_LastCount = 0;
			openLineWithZeroCountOneZeroCountTwoNonZeroCountThree.WU_Count2 = 0;

			openLineWithNonZeroCountOneZeroCountTwoNonZeroCountThree.WU_LastCount = 1;
			openLineWithNonZeroCountOneZeroCountTwoNonZeroCountThree.WU_Count2 = 0;

			openLineWithZeroCountOneNonZeroCountTwoNonZeroCountThree.WU_LastCount = 0;
			openLineWithZeroCountOneNonZeroCountTwoNonZeroCountThree.WU_Count2 = 1;

			openLineWithNonZeroCountOneNonZeroCountTwoNonZeroCountThree.WU_LastCount = 1;
			openLineWithNonZeroCountOneNonZeroCountTwoNonZeroCountThree.WU_Count2 = 1;

			// test the filter

			AddStocktakeLinesToScope(Asserter, openLineWithZeroCountThree, closedLineWithZeroCountThree, closedLineWithNonZeroCountThree,
				openLineWithZeroCountOneZeroCountTwoNonZeroCountThree, openLineWithNonZeroCountOneZeroCountTwoNonZeroCountThree,
				openLineWithZeroCountOneNonZeroCountTwoNonZeroCountThree, openLineWithNonZeroCountOneNonZeroCountTwoNonZeroCountThree);

			var filter = GetStocktakeLineFilterBusinessObject(stocktake);
			var notCountedFilter = (ModuleFlagsFilter)filter[StocktakeLineFilterBusinessObject.FilterNames.NotCounted];

			notCountedFilter.Property0 = ZBool.True;
			Asserter.AssertMatches("Since the filter value is True it should return Opened line with last count zero.", notCountedFilter,
				openLineWithZeroCountThree, openLineWithZeroCountOneZeroCountTwoNonZeroCountThree, openLineWithNonZeroCountOneZeroCountTwoNonZeroCountThree,
				openLineWithZeroCountOneNonZeroCountTwoNonZeroCountThree);

			notCountedFilter.Property0 = ZBool.False;
			Asserter.AssertMatches("Since the filter value is False it should return all lines.", notCountedFilter,
				openLineWithZeroCountThree, closedLineWithZeroCountThree, closedLineWithNonZeroCountThree,
				openLineWithZeroCountOneZeroCountTwoNonZeroCountThree, openLineWithNonZeroCountOneZeroCountTwoNonZeroCountThree,
				openLineWithZeroCountOneNonZeroCountTwoNonZeroCountThree, openLineWithNonZeroCountOneNonZeroCountTwoNonZeroCountThree);
		}

		#endregion

		#endregion

		#region TestOddEvenFilter

		public void TestOddEvenFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			// setup stocktake and lines

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);
			var lineInOddColumn = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, locations[0]); // odd column; column 1
			var lineInEvenColumn = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, locations[1]);// even column; column 2

			AddStocktakeLinesToScope(Asserter, lineInOddColumn, lineInEvenColumn);

			Factory.Save();

			//test the filter

			var filter = GetStocktakeLineFilterBusinessObject(stocktake);
			var oddEvenFilter = (ModuleTextFilter)filter[StocktakeLineFilterBusinessObject.FilterNames.OddEven];
			AssertEquals("OddEvenColumns filter category should be Locations.", oddEvenFilter.Category, FilterCategories.Locations);
			Asserter.AssertMatches("Since the filter is empty it should return all stocktake lines.", oddEvenFilter, lineInOddColumn, lineInEvenColumn);

			oddEvenFilter.Property = LocationColumnNumber.Codes.Odd;
			Asserter.AssertMatches("Since the filter value is Odd it should return line1 and line3 only.", oddEvenFilter, lineInOddColumn);

			oddEvenFilter.Property = LocationColumnNumber.Codes.Even;
			Asserter.AssertMatches("Since the filter value is Odd it should return line2 only.", oddEvenFilter, lineInEvenColumn);

			oddEvenFilter.Property = LocationColumnNumber.Codes.All;
			Asserter.AssertMatches("Since the filter value is All it should return all stocktake lines.", oddEvenFilter, lineInOddColumn, lineInEvenColumn);
		}

		public void TestOddEvenFilter_ZeroBased()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			data.Whs1.WW_LocationColumnsZeroBased = true; // Since columns are zero based they are started from zero and zero is considered even
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			// setup stocktake and lines

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);
			var lineInEvenColumn = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, locations[0]); // even column; column Zero
			var lineInOddColumn = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, locations[1]);// odd column; column one

			AddStocktakeLinesToScope(Asserter, lineInEvenColumn, lineInOddColumn);

			Factory.Save();

			//test the filter

			var filter = GetStocktakeLineFilterBusinessObject(stocktake);
			var oddEvenFilter = (ModuleTextFilter)filter[StocktakeLineFilterBusinessObject.FilterNames.OddEven];
			AssertEquals("OddEvenColumns filter category should be Locations.", oddEvenFilter.Category, FilterCategories.Locations);
			Asserter.AssertMatches("Since the filter is empty it should return all stocktake lines.", oddEvenFilter, lineInEvenColumn, lineInOddColumn);

			oddEvenFilter.Property = LocationColumnNumber.Codes.Odd;
			Asserter.AssertMatches("Since the filter value is Odd it should return line1 and line3 only.", oddEvenFilter, lineInOddColumn);

			oddEvenFilter.Property = LocationColumnNumber.Codes.Even;
			Asserter.AssertMatches("Since the filter value is Odd it should return line2 only.", oddEvenFilter, lineInEvenColumn);

			oddEvenFilter.Property = LocationColumnNumber.Codes.All;
			Asserter.AssertMatches("Since the filter value is All it should return all stocktake lines.", oddEvenFilter, lineInEvenColumn, lineInOddColumn);
		}

		#endregion

		#region TestManuallyAddedLinesFilter

		public void TestManuallyAddedLinesFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// Setup stocktakes and lines

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);

			var automaticallyAddedLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1);
			var manuallyAddedLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1);
			automaticallyAddedLine.WU_IsManuallyAdded = false;
			manuallyAddedLine.WU_IsManuallyAdded = true;

			AddStocktakeLinesToScope(Asserter, automaticallyAddedLine, manuallyAddedLine);
			var filter = GetStocktakeLineFilterBusinessObject(stocktake);

			var manuallyAddedLinesFilter = (ModuleFlagsFilter)filter[StocktakeLineFilterBusinessObject.FilterNames.ManuallyAddedLines];

			manuallyAddedLinesFilter.Property0 = false;
			Asserter.AssertMatches("Since the filter value is 1 it should return lines with Last Count 1.", manuallyAddedLinesFilter, automaticallyAddedLine);

			manuallyAddedLinesFilter.Property0 = true;
			Asserter.AssertMatches("Since the filter value is 1 it should return lines with Last Count 1.", manuallyAddedLinesFilter, manuallyAddedLine);
		}

		#endregion

		#region TestCountComparisonFilter

		#region TestCountComparisonFilter_WithNonZeroSystemCountAndWithLastVerifiedDate

		[TestDate(2012, 03, 21)]
		public void TestCountComparisonFilter_WithNonZeroSystemCountAndWithLastVerifiedDate()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// Setup stocktakes and lines

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);

			var lineWithCountOneGreaterThanSystemCount = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 2, 3, ZDateTime.Now, 1, "");
			var lineWithCountOneEqualToSystemCount = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 2, 2, ZDateTime.Now, 1, "");
			var lineWithCountOneLessThanToSystemCount = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 2, 1, ZDateTime.Now, 1, "");

			var lineWithCountTwoGreaterThanSystemCount = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 2, 3, ZDateTime.Now, 2, "");
			var lineWithCountTwoEqualToSystemCount = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 2, 2, ZDateTime.Now, 2, "");
			var lineWithCountTwoLessThanToSystemCount = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 2, 1, ZDateTime.Now, 2, "");

			var lineWithCountThreeGreaterThanSystemCount = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 2, 3, ZDateTime.Now, 3, "");
			var lineWithCountThreeEqualToSystemCount = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 2, 2, ZDateTime.Now, 3, "");
			var lineWithCountThreeLessThanToSystemCount = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 2, 1, ZDateTime.Now, 3, "");

			Factory.Save();

			AddStocktakeLinesToScope(Asserter, lineWithCountOneGreaterThanSystemCount, lineWithCountOneEqualToSystemCount, lineWithCountOneLessThanToSystemCount,
				lineWithCountTwoGreaterThanSystemCount, lineWithCountTwoEqualToSystemCount, lineWithCountTwoLessThanToSystemCount,
				lineWithCountThreeGreaterThanSystemCount, lineWithCountThreeEqualToSystemCount, lineWithCountThreeLessThanToSystemCount);

			var filter = GetStocktakeLineFilterBusinessObject(stocktake);

			var countComparisonFilter = (ModuleTextFilter)filter[StocktakeLineFilterBusinessObject.FilterNames.CountComparison];

			countComparisonFilter.Property = "Last Count > System Count";
			Asserter.AssertMatches("Lines should be Last Count > System Count.", countComparisonFilter, lineWithCountOneGreaterThanSystemCount, lineWithCountTwoGreaterThanSystemCount, lineWithCountThreeGreaterThanSystemCount);

			countComparisonFilter.Property = "Last Count = System Count";
			Asserter.AssertMatches("Lines should be Last Count = System Count.", countComparisonFilter, lineWithCountOneEqualToSystemCount, lineWithCountTwoEqualToSystemCount, lineWithCountThreeEqualToSystemCount);

			countComparisonFilter.Property = "Last Count < System Count";
			Asserter.AssertMatches("Lines should be Last Count < System Count.", countComparisonFilter, lineWithCountOneLessThanToSystemCount, lineWithCountTwoLessThanToSystemCount, lineWithCountThreeLessThanToSystemCount);
		}

		#endregion

		#region TestCountComparisonFilter_WithNonZeroSystemCountAndWithoutLastVerifiedDate

		public void TestCountComparisonFilter_WithNonZeroSystemCountAndWithoutLastVerifiedDate()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// Setup stocktakes and lines

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);

			var lineWithCountOneGreaterThanSystemCount = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 2, 3, ZDateTime.Empty, 1, "");
			var lineWithCountOneEqualToSystemCount = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 2, 2, ZDateTime.Empty, 1, "");
			var lineWithCountOneLessThanToSystemCount = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 2, 1, ZDateTime.Empty, 1, "");

			var lineWithCountTwoGreaterThanSystemCount = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 2, 3, ZDateTime.Empty, 2, "");
			var lineWithCountTwoEqualToSystemCount = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 2, 2, ZDateTime.Empty, 2, "");
			var lineWithCountTwoLessThanToSystemCount = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 2, 1, ZDateTime.Empty, 2, "");

			var lineWithCountThreeGreaterThanSystemCount = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 2, 3, ZDateTime.Empty, 3, "");
			var lineWithCountThreeEqualToSystemCount = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 2, 2, ZDateTime.Empty, 3, "");
			var lineWithCountThreeLessThanToSystemCount = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 2, 1, ZDateTime.Empty, 3, "");

			Factory.Save();

			AddStocktakeLinesToScope(Asserter, lineWithCountOneGreaterThanSystemCount, lineWithCountOneEqualToSystemCount, lineWithCountOneLessThanToSystemCount,
				lineWithCountTwoGreaterThanSystemCount, lineWithCountTwoEqualToSystemCount, lineWithCountTwoLessThanToSystemCount,
				lineWithCountThreeGreaterThanSystemCount, lineWithCountThreeEqualToSystemCount, lineWithCountThreeLessThanToSystemCount);

			var filter = GetStocktakeLineFilterBusinessObject(stocktake);

			var countComparisonFilter = (ModuleTextFilter)filter[StocktakeLineFilterBusinessObject.FilterNames.CountComparison];

			countComparisonFilter.Property = "Last Count > System Count";
			Asserter.AssertMatches("Lines should be Last Count > System Count.", countComparisonFilter, lineWithCountOneGreaterThanSystemCount, lineWithCountTwoGreaterThanSystemCount, lineWithCountThreeGreaterThanSystemCount);

			countComparisonFilter.Property = "Last Count = System Count";
			Asserter.AssertMatches("Lines should be Last Count = System Count.", countComparisonFilter, lineWithCountOneEqualToSystemCount, lineWithCountTwoEqualToSystemCount, lineWithCountThreeEqualToSystemCount);

			countComparisonFilter.Property = "Last Count < System Count";
			Asserter.AssertMatches("Lines should be Last Count < System Count.", countComparisonFilter, lineWithCountOneLessThanToSystemCount, lineWithCountTwoLessThanToSystemCount, lineWithCountThreeLessThanToSystemCount);
		}

		#endregion

		#region TestCountComparisonFilter_WithZeroSystemCountAndWithLastVerifiedDate

		[TestDate(2012, 03, 21)]
		public void TestCountComparisonFilter_WithZeroSystemCountAndWithLastVerifiedDate()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// Setup stocktakes and lines

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);

			var lineWithCountOneGreaterThanSystemCount = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, 2, ZDateTime.Now, 1, "");
			var lineWithCountOneEqualToSystemCount = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, 0, ZDateTime.Now, 1, "");

			var lineWithCountTwoGreaterThanSystemCount = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, 2, ZDateTime.Now, 2, "");
			var lineWithCountTwoEqualToSystemCount = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, 0, ZDateTime.Now, 2, "");

			var lineWithCountThreeGreaterThanSystemCount = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, 2, ZDateTime.Now, 3, "");
			var lineWithCountThreeEqualToSystemCount = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, 0, ZDateTime.Now, 3, "");

			Factory.Save();

			AddStocktakeLinesToScope(Asserter, lineWithCountOneGreaterThanSystemCount, lineWithCountOneEqualToSystemCount,
				lineWithCountTwoGreaterThanSystemCount, lineWithCountTwoEqualToSystemCount,
				lineWithCountThreeGreaterThanSystemCount, lineWithCountThreeEqualToSystemCount);

			var filter = GetStocktakeLineFilterBusinessObject(stocktake);

			var countComparisonFilter = (ModuleTextFilter)filter[StocktakeLineFilterBusinessObject.FilterNames.CountComparison];

			countComparisonFilter.Property = "Last Count > System Count";
			Asserter.AssertMatches("Lines should be Last Count > System Count.", countComparisonFilter, lineWithCountOneGreaterThanSystemCount, lineWithCountTwoGreaterThanSystemCount, lineWithCountThreeGreaterThanSystemCount);

			countComparisonFilter.Property = "Last Count = System Count";
			Asserter.AssertMatches("Lines should be Last Count = System Count.", countComparisonFilter, lineWithCountOneEqualToSystemCount, lineWithCountTwoEqualToSystemCount, lineWithCountThreeEqualToSystemCount);

			countComparisonFilter.Property = "Last Count < System Count";
			Asserter.AssertMatches("Lines should be Last Count < System Count.", countComparisonFilter);
		}

		#endregion

		#region TestCountComparisonFilter_WithZeroSystemCountAndWithoutLastVerifiedDate

		public void TestCountComparisonFilter_WithZeroSystemCountAndWithoutLastVerifiedDate()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// Setup stocktakes and lines

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);

			var lineWithCountOneGreaterThanSystemCount = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, 2, ZDateTime.Empty, 1, "");
			var lineWithCountOneEqualToSystemCount = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, 0, ZDateTime.Empty, 1, StocktakeLineStatus.Codes.Open);

			var lineWithCountTwoGreaterThanSystemCount = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, 2, ZDateTime.Empty, 2, "");
			var lineWithCountTwoEqualToSystemCount = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, 0, ZDateTime.Empty, 2, StocktakeLineStatus.Codes.Open);

			var lineWithCountThreeGreaterThanSystemCount = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, 2, ZDateTime.Empty, 3, "");
			var lineWithCountThreeEqualToSystemCount = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, 0, ZDateTime.Empty, 3, StocktakeLineStatus.Codes.Open);

			Factory.Save();

			AddStocktakeLinesToScope(Asserter, lineWithCountOneGreaterThanSystemCount, lineWithCountOneEqualToSystemCount,
				lineWithCountTwoGreaterThanSystemCount, lineWithCountTwoEqualToSystemCount,
				lineWithCountThreeGreaterThanSystemCount, lineWithCountThreeEqualToSystemCount);

			var filter = GetStocktakeLineFilterBusinessObject(stocktake);

			var countComparisonFilter = (ModuleTextFilter)filter[StocktakeLineFilterBusinessObject.FilterNames.CountComparison];

			countComparisonFilter.Property = "Last Count > System Count";
			Asserter.AssertMatches("Lines should be Last Count > System Count.", countComparisonFilter, lineWithCountOneGreaterThanSystemCount, lineWithCountTwoGreaterThanSystemCount, lineWithCountThreeGreaterThanSystemCount);

			countComparisonFilter.Property = "Last Count = System Count";
			Asserter.AssertMatches("Lines should be Last Count = System Count.", countComparisonFilter);

			countComparisonFilter.Property = "Last Count < System Count";
			Asserter.AssertMatches("Lines should be Last Count < System Count.", countComparisonFilter);
		}

		#endregion

		#region TestCountComparisonFilter_WithClosedLines

		public void TestCountComparisonFilter_WithClosedLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// Setup stocktakes and lines

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);

			var lineWithCountOneEqualToSystemCount = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, 0, ZDateTime.Empty, 1, StocktakeLineStatus.Codes.Closed);
			var lineWithCountTwoEqualToSystemCount = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, 0, ZDateTime.Empty, 2, StocktakeLineStatus.Codes.Closed);
			var lineWithCountThreeEqualToSystemCount = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, 0, ZDateTime.Empty, 3, StocktakeLineStatus.Codes.Closed);

			Factory.Save();

			AddStocktakeLinesToScope(Asserter, lineWithCountOneEqualToSystemCount,
												lineWithCountTwoEqualToSystemCount,
												lineWithCountThreeEqualToSystemCount);

			var filter = GetStocktakeLineFilterBusinessObject(stocktake);

			var countComparisonFilter = (ModuleTextFilter)filter[StocktakeLineFilterBusinessObject.FilterNames.CountComparison];

			countComparisonFilter.Property = "Last Count > System Count";
			Asserter.AssertMatches("Lines should be Last Count > System Count.", countComparisonFilter);

			countComparisonFilter.Property = "Last Count = System Count";
			Asserter.AssertMatches("Lines should be Last Count = System Count.",
				countComparisonFilter, lineWithCountOneEqualToSystemCount, lineWithCountTwoEqualToSystemCount, lineWithCountThreeEqualToSystemCount);

			countComparisonFilter.Property = "Last Count < System Count";
			Asserter.AssertMatches("Lines should be Last Count < System Count.", countComparisonFilter);

			countComparisonFilter.Property = "Testing Blah Blah.";
			Asserter.AssertMatches("Random comparator entered should return all stocktake lines",
				countComparisonFilter, lineWithCountOneEqualToSystemCount, lineWithCountTwoEqualToSystemCount, lineWithCountThreeEqualToSystemCount);
		}

		#endregion

		#endregion

		#region TestFilterForColorScheme

		public void TestFilterForColorScheme()
		{
			var filter = new StocktakeLineFilterBusinessObject();
			var rowFilter = (ModuleTextFilter)filter[StocktakeLineFilterBusinessObject.FilterNames.Row];
			var areaFilter = (ModuleTextFilter)filter[StocktakeLineFilterBusinessObject.FilterNames.PickArea];
			var countComparisonFilter = (ModuleTextFilter)filter[StocktakeLineFilterBusinessObject.FilterNames.CountComparison];
			var manuallyAddedLinesFilter = (ModuleFlagsFilter)filter[StocktakeLineFilterBusinessObject.FilterNames.ManuallyAddedLines];
			var notCountedFilter = (ModuleFlagsFilter)filter[StocktakeLineFilterBusinessObject.FilterNames.NotCounted];
			var oddEvenFilter = (ModuleTextFilter)filter[StocktakeLineFilterBusinessObject.FilterNames.OddEven];
			var pickMethodFilter = (ModuleTextFilter)filter[StocktakeLineFilterBusinessObject.FilterNames.PickMethod];
			var statusFilter = (ModuleTextFilter)filter[StocktakeLineFilterBusinessObject.FilterNames.Status];
			rowFilter.IsActive = true;
			areaFilter.IsActive = true;
			countComparisonFilter.IsActive = true;
			manuallyAddedLinesFilter.IsActive = true;
			notCountedFilter.IsActive = true;
			oddEvenFilter.IsActive = true;
			pickMethodFilter.IsActive = true;
			statusFilter.IsActive = true;

			AssertNoExceptionThrown("No exception should be thrown when Row filter is selected.", () => rowFilter.Property = "");
			AssertNoExceptionThrown("No exception should be thrown when Row filter value is changed.", () => rowFilter.Property = "Row");

			AssertNoExceptionThrown("No exception should be thrown when Area filter is selected.", () => areaFilter.Property = "");
			AssertNoExceptionThrown("No exception should be thrown when Area filter value is changed.", () => areaFilter.Property = "Area");

			AssertNoExceptionThrown("No exception should be thrown when CountComparison filter is selected.", () => countComparisonFilter.Property = "");
			AssertNoExceptionThrown("No exception should be thrown when CountComparison filter value is changed.", () => countComparisonFilter.Property = StocktakeLineFilterBusinessObject.CountValuesComparisonOptions.LastCountGreaterThanSystemCount);

			AssertNoExceptionThrown("No exception should be thrown when Manually Added Lines filter is selected.", () => manuallyAddedLinesFilter.Property0 = false);
			AssertNoExceptionThrown("No exception should be thrown when Manually Added Lines filter value is changed.", () => manuallyAddedLinesFilter.Property0 = true);

			AssertNoExceptionThrown("No exception should be thrown when Not Counted Filter is selected.", () => notCountedFilter.Property0 = false);
			AssertNoExceptionThrown("No exception should be thrown when Not Counted Filter value is changed.", () => notCountedFilter.Property0 = true);

			AssertNoExceptionThrown("No exception should be thrown when Odd/Even filter is selected.", () => oddEvenFilter.Property = "");
			AssertNoExceptionThrown("No exception should be thrown when Odd/Even filter value is changed.", () => oddEvenFilter.Property = "Odd");

			AssertNoExceptionThrown("No exception should be thrown when Pick method filter is selected.", () => pickMethodFilter.Property = "");
			AssertNoExceptionThrown("No exception should be thrown when Pick method filter value is changed.", () => pickMethodFilter.Property = "Method1");

			AssertNoExceptionThrown("No exception should be thrown when when Stocktake line status filter is selected.", () => statusFilter.Property = "");
			AssertNoExceptionThrown("No exception should be thrown when when Stocktake line status filter value is changed.", () => statusFilter.Property = StocktakeLineStatus.Codes.Open);
		}

		#endregion

		#region TestColumnFilter

		public void TestColumnFilterType()
		{
			var stocktake = Factory.New<WhsStocktake>();
			var filter = GetStocktakeLineFilterBusinessObject(stocktake);
			var columnFilter = (ModuleTextFilter)filter[StocktakeLineFilterBusinessObject.FilterNames.Column];

			AssertEquals("Column filter category should be Locations.", columnFilter.Category, FilterCategories.Locations);
			AssertEquals(1, columnFilter.ComparisonOperator_List.Count);
			AssertEquals(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact, columnFilter.ComparisonOperator_List[0].Code);
		}

		public void TestColumnFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			AssertLocationComponent(data, data.Whs1.Rows.Single(r => r.WR_Name == "A"), StocktakeLineFilterBusinessObject.FilterNames.Column, "2", "3");
		}

		public void TestColumnFilter_WithAlphaColumns()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_LocationColumnsAlpha = true;

			AssertLocationComponent(data, data.Whs1.Rows.Single(r => r.WR_Name == "A"), StocktakeLineFilterBusinessObject.FilterNames.Column, "B", "C");
		}

		public void TestColumnFilter_WithZeroBasedLocationColumns()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_LocationColumnsZeroBased = true;

			AssertLocationComponent(data, data.Whs1.Rows.Single(r => r.WR_Name == "A"), StocktakeLineFilterBusinessObject.FilterNames.Column, "1", "2");
		}

		#endregion

		#region TestLevelFilter

		public void TestLevelFilterType()
		{
			var stocktake = Factory.New<WhsStocktake>();
			var filter = GetStocktakeLineFilterBusinessObject(stocktake);

			var levelFilter = (ModuleTextFilter)filter[StocktakeLineFilterBusinessObject.FilterNames.Level];
			AssertEquals("Level filter category should be Locations.", levelFilter.Category, FilterCategories.Locations);
			AssertEquals(1, levelFilter.ComparisonOperator_List.Count);
			AssertEquals(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact, levelFilter.ComparisonOperator_List[0].Code);
		}

		public void TestLevelFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 3);
			AssertLocationComponent(data, data.Whs1.Rows.Single(r => r.WR_Name == "A"), StocktakeLineFilterBusinessObject.FilterNames.Level, "2", "3");
		}

		public void TestLevelFilter_WithAlphaColumns()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 3);
			data.Whs1.WW_LocationLevelsAlpha = true;

			AssertLocationComponent(data, data.Whs1.Rows.Single(r => r.WR_Name == "A"), StocktakeLineFilterBusinessObject.FilterNames.Level, "B", "C");
		}

		public void TestLevelFilter_WithZeroBasedLocationColumns()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 3);
			data.Whs1.WW_LocationLevelsZeroBased = true;

			AssertLocationComponent(data, data.Whs1.Rows.Single(r => r.WR_Name == "A"), StocktakeLineFilterBusinessObject.FilterNames.Level, "1", "2");
		}

		#endregion

		#region TestTrayFilter

		public void TestTrayFilterType()
		{
			var stocktake = Factory.New<WhsStocktake>();
			var filter = GetStocktakeLineFilterBusinessObject(stocktake);
			var trayFilter = (ModuleTextFilter)filter[StocktakeLineFilterBusinessObject.FilterNames.Tray];

			AssertEquals("Tray filter category should be Locations.", trayFilter.Category, FilterCategories.Locations);
			AssertEquals(1, trayFilter.ComparisonOperator_List.Count);
			AssertEquals(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact, trayFilter.ComparisonOperator_List[0].Code);
		}

		public void TestTrayFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "R", 1, 1, 2);

			AssertLocationComponent(data, row, StocktakeLineFilterBusinessObject.FilterNames.Tray, "2", "3");
		}

		public void TestTrayFilter_WithAlphaColumns()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "R", 1, 1, 2);
			data.Whs1.WW_LocationTraysAlpha = true;

			AssertLocationComponent(data, row, StocktakeLineFilterBusinessObject.FilterNames.Tray, "B", "C");
		}

		public void TestTrayFilter_WithZeroBasedLocationColumns()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "R", 1, 1, 2);
			data.Whs1.WW_LocationTraysZeroBased = true;

			AssertLocationComponent(data, row, StocktakeLineFilterBusinessObject.FilterNames.Tray, "1", "2");
		}

		#endregion

		#region Test Filter Max Length

		public void TestFilterMaxLength()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);
			var filter = GetStocktakeLineFilterBusinessObject(stocktake);

			CombineAssertions(() =>
			{
				AssertEquals("MaxLength of Pick Area Name should be set correctly.", WhsAreaSchema.WA_Name.MaxLength, filter[StocktakeLineFilterBusinessObject.FilterNames.PickArea].MaxLength);
				AssertEquals("MaxLength of Row Filter should be set correctly.", WhsRowSchema.WR_Name.MaxLength, filter[StocktakeLineFilterBusinessObject.FilterNames.Row].MaxLength);
				AssertEquals("MaxLength of Pick Method should be set correctly.", WhsLocationViewSchema.WLV_PickMethod.MaxLength, filter[StocktakeLineFilterBusinessObject.FilterNames.PickMethod].MaxLength);
			});
		}

		#endregion

		#region Implementation

		protected WhsTestHelperFunctions Helper => helper = helper ?? new WhsTestHelperFunctions(Factory);
		WhsTestHelperFunctions helper;

		void AssertLocationComponent(TestDataSimpleEnvironment data, WhsRow row, ZString schemaColumn, ZString validValue, ZString invalidValue)
		{
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);

			var lineInLocationComponent1 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, row.Locations[0]);
			var lineInLocationComponent2 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, row.Locations[1]);

			AddStocktakeLinesToScope(Asserter, lineInLocationComponent1, lineInLocationComponent2);

			Factory.Save();

			// Test the filter

			var lineFilters = GetStocktakeLineFilterBusinessObject(stocktake);

			var filter = (ModuleTextFilter)lineFilters[schemaColumn];
			Asserter.AssertMatches("Since the filter is empty it should return all stocktake lines.", filter, lineInLocationComponent1, lineInLocationComponent2);

			filter.Property = validValue;
			Asserter.AssertMatches(string.Format("It should return stocktake lines which are in {0} - {1}.", schemaColumn, validValue), filter, lineInLocationComponent2);

			filter.Property = invalidValue;
			Asserter.AssertMatches(string.Format("There should be no stocktake lines associated with {0} - {1}.", schemaColumn, invalidValue), filter);
		}

		FilterStripAsserter<WhsStocktakeLine> Asserter => asserter ?? (asserter = new FilterStripAsserter<WhsStocktakeLine>(Factory, (s) => s.WU_LineNo.ToString()));
		FilterStripAsserter<WhsStocktakeLine> asserter;

		StocktakeLineFilterBusinessObject GetStocktakeLineFilterBusinessObject(WhsStocktake stocktake)
		{
			return new StocktakeLineFilterBusinessObject(stocktake);
		}

		void AddStocktakeLinesToScope(FilterStripAsserter<WhsStocktakeLine> whsStocktakeLine, params WhsStocktakeLine[] stocktakeLines)
		{
			foreach (var stocktakeLine in stocktakeLines)
			{
				whsStocktakeLine.AddToScope(stocktakeLine);
			}
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			var stockTake = Factory.NewWithValidTestData<WhsStocktake>();
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			stockTake.WS_WW_Whs = warehouse.PK;
			return new StocktakeLineFilterBusinessObject(stockTake);
		}

		#endregion
	}
}
