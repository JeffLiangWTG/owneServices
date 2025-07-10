using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.US.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.GUI.US.Testing
{
	internal class ApprovedKnownColumnManagerTest : WhsTestCaseWithFactoryUS
	{
		#region Column Control

		public void TestSetColumn()
		{
			using (TestGrid testGridInstance = new TestGrid())
			{
				TSAKnownColumnManager testManager = new TSAKnownColumnManager(testGridInstance, WhsLocationViewSchema.WLV_ApprovedKnownLocation.Name);

				testManager.SetColumn(WhsLocationViewSchema.WLV_ApprovedKnownLocation.Name, false);
				AssertTSAColumnProperties(testGridInstance, false);

				testManager.SetColumn(WhsLocationViewSchema.WLV_ApprovedKnownLocation.Name, true);
				AssertTSAColumnProperties(testGridInstance, true);
			}
		}

		public void TestSetColumns()
		{
			WhsWarehouse whs = Helper.CreateWarehouse("WHS");
			WhsRow row = Helper.CreateRow(whs, "ROW");

			using (TestGrid testGridInstance = new TestGrid())
			{
				TSAKnownColumnManager testManager = new TSAKnownColumnManager(testGridInstance, WhsLocationViewSchema.WLV_ApprovedKnownLocation.Name);

				Helper.SetWarehouseTSAStatus(whs, CodeLists.US.TSAStatus.Codes.Unknown);
				testManager.SetColumns(row);
				AssertTSAColumnProperties(testGridInstance, false);

				Helper.SetWarehouseTSAStatus(whs, CodeLists.US.TSAStatus.Codes.Known);
				testManager.SetColumns(row);
				AssertTSAColumnProperties(testGridInstance, true);
			}
		}

		#endregion

		#region Implementation

		void AssertTSAColumnProperties(TestGrid grid, bool isExpected)
		{
			AssertEquals(isExpected, grid.Columns[WhsLocationViewSchema.WLV_ApprovedKnownLocation.Name].IsVisible);
			AssertEquals(!isExpected, grid.Columns[WhsLocationViewSchema.WLV_ApprovedKnownLocation.Name].IsUnavailable);
			if (isExpected)
			{
				AssertEquals("", grid.Columns[WhsLocationViewSchema.WLV_ApprovedKnownLocation.Name].ErrorMessageWhenUnavailable);
			}
			else
			{
				AssertEquals("TSA column can be selected only for TSA Known warehouse", grid.Columns[WhsLocationViewSchema.WLV_ApprovedKnownLocation.Name].ErrorMessageWhenUnavailable);
			}
		}

		internal class TestGrid : ZGrid
		{
			public TestGrid()
				: base()
			{
				this.Columns.AddTextColumn(WhsLocationViewSchema.WLV_ApprovedKnownLocation.Name, 10);
				this.Columns[WhsLocationViewSchema.WLV_ApprovedKnownLocation.Name].IsVisible = true;
				this.Columns[WhsLocationViewSchema.WLV_ApprovedKnownLocation.Name].IsUnavailable = false;
				this.Columns[WhsLocationViewSchema.WLV_ApprovedKnownLocation.Name].ErrorMessageWhenUnavailable = "";
			}
		}

		#endregion
	}
}
