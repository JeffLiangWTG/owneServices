using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsLocationFormatterTest : WhsTestCaseWithFactory
	{
		public void TestFunction()
		{
			var whs = Helper.CreateWarehouse("1");
			var row1 = Helper.CreateRow(whs, "A", 1, 1);
			AssertEquals("A", "A", GetLocationString(whs, row1, 1, 1, 1, false));

			var row2 = Helper.CreateRow(whs, "A", 2, 1);
			AssertEquals("A-2", "A-2", GetLocationString(whs, row2, 2, 1, 1, false));

			var row3 = Helper.CreateRow(whs, "A", 1, 2);
			AssertEquals("A-1-2", "A-1-2", GetLocationString(whs, row3, 1, 2, 1, false));

			var row4 = Helper.CreateRow(whs, "A", 1, 1, 2);
			AssertEquals("A-1-1-2", "A-1-1-2", GetLocationString(whs, row4, 1, 1, 2, false));
			AssertEquals("A-1-1-2", "A-1-1-2", GetLocationString(whs, row4, 1, 1, 2, true));

			var row5 = Helper.CreateRow(whs, "A", 3, 4, 5);
			AssertEquals("A-3-4-5", "A-3-4-5", GetLocationString(whs, row5, 3, 4, 5, false));

			whs.WW_LocationComponentDelimiter = ".";
			AssertEquals("A.3.4.5", "A.3.4.5", GetLocationString(whs, row5, 3, 4, 5, false));

			whs.WW_LocationColumnsAlpha = true;
			AssertEquals("A.C.4.5", "A.C.4.5", GetLocationString(whs, row5, 3, 4, 5, false));

			whs.WW_LocationLevelsAlpha = true;
			AssertEquals("A.C.D.5", "A.C.D.5", GetLocationString(whs, row5, 3, 4, 5, false));

			whs.WW_LocationTraysAlpha = true;
			AssertEquals("A.C.D.E", "A.C.D.E", GetLocationString(whs, row5, 3, 4, 5, false));

			whs.WW_LocationColumnsAlpha = false;
			whs.WW_LocationLevelsAlpha = false;
			whs.WW_LocationTraysAlpha = false;
			whs.WW_LocationComponentDelimiter = "-";
			var row6 = Helper.CreateRow(whs, "A", 100, 20, 10);
			AssertEquals("A-5-6-7", "A-5-6-7", GetLocationString(whs, row6, 5, 6, 7, false));

			whs.WW_LocationsHaveLeadingZeros = true;
			AssertEquals("A-005-06-07", "A-005-06-07", GetLocationString(whs, row6, 5, 6, 7, false));
			AssertEquals("A-035-16-01", "A-035-16-01", GetLocationString(whs, row6, 35, 16, 1, false));
			AssertEquals("A-100-20-10", "A-100-20-10", GetLocationString(whs, row6, 100, 20, 10, false));
		}

		public void TestFunction_FixedWidthLocationWarehouse()
		{
			var whs = Helper.CreateFixedWidthLocationWarehouse("1", 3, 3, 2);
			var row1 = Helper.CreateRow(whs, "A", 100, 2, 2);
			AssertEquals("Location string is correct", "A10000202", GetLocationString(whs, row1, 100, 2, 2, false));
			AssertEquals("Location string is correct", "A-100-002-02", GetLocationString(whs, row1, 100, 2, 2, true));
			AssertEquals("Location string is correct", "A01000202", GetLocationString(whs, row1, 10, 2, 2, false));
			AssertEquals("Location string is correct", "A-010-002-02", GetLocationString(whs, row1, 10, 2, 2, true));
			AssertEquals("Location string is correct", "A00100202", GetLocationString(whs, row1, 1, 2, 2, false));
			AssertEquals("Location string is correct", "A-001-002-02", GetLocationString(whs, row1, 1, 2, 2, true));

			var row2 = Helper.CreateRow(whs, "B", 2, 100, 2);
			AssertEquals("Location string is correct", "B00210002", GetLocationString(whs, row2, 2, 100, 2, false));
			AssertEquals("Location string is correct", "B-002-100-02", GetLocationString(whs, row2, 2, 100, 2, true));
			AssertEquals("Location string is correct", "B00201002", GetLocationString(whs, row2, 2, 10, 2, false));
			AssertEquals("Location string is correct", "B-002-010-02", GetLocationString(whs, row2, 2, 10, 2, true));
			AssertEquals("Location string is correct", "B00200102", GetLocationString(whs, row2, 2, 1, 2, false));
			AssertEquals("Location string is correct", "B-002-001-02", GetLocationString(whs, row2, 2, 1, 2, true));

			var row3 = Helper.CreateRow(whs, "C", 2, 2, 10);
			AssertEquals("Location string is correct", "C00200210", GetLocationString(whs, row3, 2, 2, 10, false));
			AssertEquals("Location string is correct", "C-002-002-10", GetLocationString(whs, row3, 2, 2, 10, true));
			AssertEquals("Location string is correct", "C00200201", GetLocationString(whs, row3, 2, 2, 1, false));
			AssertEquals("Location string is correct", "C-002-002-01", GetLocationString(whs, row3, 2, 2, 1, true));
		}

		ZString GetLocationString(WhsWarehouse whs, WhsRow row, short column, short level, short tray,
			bool includeDelimiterIfFixedWidth)
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var sql = $@"select dbo.WhsLocationFormatter(
				'{row.WR_Name}', {column}, {level}, {tray},
				{row.WR_Columns}, {row.WR_Levels}, {row.WR_Trays},
				{(whs.WW_LocationColumnsAlpha ? 1 : 0)},
				{(whs.WW_LocationLevelsAlpha ? 1 : 0)},
				{(whs.WW_LocationTraysAlpha ? 1 : 0)},
				'{whs.WW_LocationComponentDelimiter}',
				{(whs.WW_LocationsHaveLeadingZeros ? 1 : 0)},
				{(whs.WW_LocationColumnsZeroBased ? 1 : 0)},
				{(whs.WW_LocationLevelsZeroBased ? 1 : 0)},
				{(whs.WW_LocationTraysZeroBased ? 1 : 0)},
				{whs.WW_LocationColumnsFixedWidth},
				{whs.WW_LocationLevelsFixedWidth},
				{whs.WW_LocationTraysFixedWidth},
				{(includeDelimiterIfFixedWidth ? 1 : 0)})";

			return (string)(Db.Connection.ExecuteScalar(sql));
		}
	}
}
