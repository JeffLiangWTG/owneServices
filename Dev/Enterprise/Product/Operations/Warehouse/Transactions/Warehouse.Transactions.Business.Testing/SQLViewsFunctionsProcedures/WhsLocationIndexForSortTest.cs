using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsLocationIndexForSortTest : WhsTestCaseWithFactory
	{
		#region TestLocationIndexForSort

		public void TestLocationIndexForSort()
		{
			var whs = Helper.CreateWarehouse("1");
			var rowA = Helper.CreateRow(whs, "A", 15, 15, 15);
			var rowB = Helper.CreateRow(whs, "B", 2, 3, 4);
			Factory.Save();

			var resultsUsingFunction = new DynamicBusinessObjectCollection(Factory);
			var sql1 = @"SELECT
							WL_PK
						FROM
							dbo.WhsLocation
							JOIN dbo.WhsRow ON WL_WR = WR_PK
						CROSS APPLY dbo.WhsLocationIndexForSort(WL_Column, WL_Level, WL_Tray, WR_Levels, WR_Trays) as LocationIndexForSort
						ORDER BY
							WR_Name, LocationIndexForSort.LocationIndex";
			resultsUsingFunction.Load(sql1);

			var resultUsingSeparateOrderBys = new DynamicBusinessObjectCollection(Factory);
			var sql2 = @"SELECT
							WL_PK
						FROM
							dbo.WhsLocation
							JOIN dbo.WhsRow ON WL_WR = WR_PK
						ORDER BY
							WR_Name, WL_Column, WL_Level, WL_Tray";
			resultUsingSeparateOrderBys.Load(sql2);

			AssertArrayEqualsByElements(resultUsingSeparateOrderBys.Select(r => r["WL_PK"]).ToArray(),
				resultsUsingFunction.Select(r => r["WL_PK"]).ToArray());
		}

		#endregion

		#region TestGetLocationIndexForSort

		public void TestGetLocationIndexForSort()
		{
			var whs = Helper.CreateWarehouse("1");
			var row = Helper.CreateRow(whs, "A", 2, 1);
			Factory.Save();
			AssertLocationIndex(whs.FindLocation("A-2"));

			row.WR_Levels = 2;
			Factory.Save();
			AssertLocationIndex(whs.FindLocation("A-1-2"));

			row.WR_Trays = 2;
			Factory.Save();
			AssertLocationIndex(whs.FindLocation("A-1-1-2"));

			row.WR_Columns = 3;
			row.WR_Levels = 4;
			row.WR_Trays = 5;
			Factory.Save();
			AssertLocationIndex(whs.FindLocation("A-3-4-5"));
		}

		void AssertLocationIndex(WhsLocation location)
		{
			var expectedIndex = WhsSqlViewHelper.GetLocationIndexForSort(location);
			var result = new DynamicBusinessObjectCollection(Factory);
			var sql =
				@"select LocationIndex from dbo.WhsLocationIndexForSort(@Column, @Level, @Tray, @LevelsInRow, @TraysInRow)";

			var queryParams = new ZSqlParameterCollection();
			queryParams.Add("@Column", location.WLV_Column, WhsLocationSchema.WL_Column);
			queryParams.Add("@Level", location.WLV_Level, WhsLocationSchema.WL_Level);
			queryParams.Add("@Tray", location.WLV_Tray, WhsLocationSchema.WL_Tray);
			queryParams.Add("@LevelsInRow", location.Row.WR_Levels, WhsRowSchema.WR_Levels);
			queryParams.Add("@TraysInRow", location.Row.WR_Trays, WhsRowSchema.WR_Trays);

			result.Load(sql, queryParams);
			var resultTopRow = result[0];
			AssertEquals(expectedIndex, (ZInt)(resultTopRow["LocationIndex"]));
		}

		#endregion
	}
}
