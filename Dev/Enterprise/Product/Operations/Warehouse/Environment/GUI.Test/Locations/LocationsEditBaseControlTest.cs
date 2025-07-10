using System.Drawing;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.GUI.Testing
{
	public class LocationsEditBaseControlTest : WhsGuiTestCaseWithFactory
	{
		#region TestChangingSortPathMethodUpdatesReadOnlyOfSequenceColumn

		#region TestChangingSortPathMethodUpdatesReadOnlyOfSequenceColumn_PickPathSequence

		public void TestChangingSortPathMethodUpdatesReadOnlyOfSequenceColumn_PickPathSequence()
		{
			var row = Factory.NewWithValidTestData<WhsRow>();

			using (var form = new ZForm(row))
			{
				var control = new LocationsEditBaseControl();
				form.Controls.Add(control);
				form.Show();

				var columnStyle = control.LocationGrid.Columns[WhsLocationViewSchema.Constants.WLV_PickPathSequence].ColumnStyle;
				AssertEquals("Defaults to Column then Level", true, columnStyle.ReadOnly);

				row.SortPickPathMethod = CodeLists.SortPathMethods.Codes.ColumnThenLevel;
				AssertEquals(true, columnStyle.ReadOnly);

				row.SortPickPathMethod = CodeLists.SortPathMethods.Codes.UserDefined;
				AssertEquals(false, columnStyle.ReadOnly);

				row.SortPickPathMethod = CodeLists.SortPathMethods.Codes.LevelThenColumn;
				AssertEquals(true, columnStyle.ReadOnly);

				row.SortPickPathMethod = "";
				AssertEquals(true, columnStyle.ReadOnly);
			}
		}

		#endregion

		#region TestChangingSortPathMethodUpdatesReadOnlyOfSequenceColumn_PutawayPathSequence

		public void TestChangingSortPathMethodUpdatesReadOnlyOfSequenceColumn_PutawayPathSequence()
		{
			var row = Factory.NewWithValidTestData<WhsRow>();

			using (var form = new ZForm(row))
			{
				var control = new LocationsEditBaseControl();
				form.Controls.Add(control);
				form.Show();

				var columnStyle = control.LocationGrid.Columns[WhsLocationViewSchema.Constants.WLV_PutawayPathSequence].ColumnStyle;
				AssertEquals("Defaults to Column then Level", true, columnStyle.ReadOnly);

				row.SortPutawayPathMethod = CodeLists.SortPathMethods.Codes.ColumnThenLevel;
				AssertEquals(true, columnStyle.ReadOnly);

				row.SortPutawayPathMethod = CodeLists.SortPathMethods.Codes.UserDefined;
				AssertEquals(false, columnStyle.ReadOnly);

				row.SortPutawayPathMethod = CodeLists.SortPathMethods.Codes.LevelThenColumn;
				AssertEquals(true, columnStyle.ReadOnly);

				row.SortPutawayPathMethod = "";
				AssertEquals(true, columnStyle.ReadOnly);
			}
		}

		#endregion

		#region TestChangingSortPathMethodUpdatesReadOnlyOfSequenceColumn_CycleCountPathSequence

		public void TestChangingSortPathMethodUpdatesReadOnlyOfSequenceColumn_CycleCountPathSequence()
		{
			var row = Factory.NewWithValidTestData<WhsRow>();

			using (var form = new ZForm(row))
			{
				var control = new LocationsEditBaseControl();
				form.Controls.Add(control);
				form.Show();

				var columnStyle = control.LocationGrid.Columns[WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence].ColumnStyle;
				AssertEquals("Defaults to Column then Level", true, columnStyle.ReadOnly);

				row.SortCycleCountMethod = CodeLists.SortPathMethods.Codes.ColumnThenLevel;
				AssertEquals(true, columnStyle.ReadOnly);

				row.SortCycleCountMethod = CodeLists.SortPathMethods.Codes.UserDefined;
				AssertEquals(false, columnStyle.ReadOnly);

				row.SortCycleCountMethod = CodeLists.SortPathMethods.Codes.LevelThenColumn;
				AssertEquals(true, columnStyle.ReadOnly);

				row.SortCycleCountMethod = "";
				AssertEquals(true, columnStyle.ReadOnly);
			}
		}

		#endregion

		#endregion

		#region TestControlVisibility

		#region TestControlVisibility_ProductWarehouse

		public void TestControlVisibility_ProductWarehouse()
		{
			var prwWarehouse = Helper.CreateWarehouse("3PL");
			prwWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			var row = Helper.CreateRow(prwWarehouse, "A");

			using (var form = new ZForm(row))
			{
				var control = new LocationsEditBaseControl();
				form.Controls.Add(control);
				form.Show();

				AssertControlVisible(control, "LocationTypeGuidFindBox", true);
				AssertControlVisible(control, "StatusDropEdit", true);
				AssertControlVisible(control, "MaxWeightCalcDropEdit", true);
				AssertControlVisible(control, "MaxCubicCalcDropEdit", true);
				AssertControlVisible(control, "PickingAreaGuidFindBox", true);
				AssertControlVisible(control, "PutawayAreaGuidFindBox", true);
				AssertControlVisible(control, "MaximumTouchCountCalcEdit", true);
				AssertControlVisible(control, "PickMethodDropEdit", true);
				AssertControlVisible(control, "SortByPathGroupBox", true);
				AssertControlVisible(control, "MaxQuantityCalcEdit", true);

				AssertControlVisible(control, "MaxHeightCalcEdit", false);
				AssertControlVisible(control, "MaxDepthCalcEdit", false);
				AssertControlVisible(control, "MaxWidthCalcEdit", false);
				AssertControlVisible(control, "MaxDimensionDropEdit", false);
				AssertControlVisible(control, "PalletFloorSpacesCalcEdit", true);
				AssertControlVisible(control, "PalletStackHeightCalcEdit", true);
			}
		}

		#endregion

		#region TestControlVisibility_FTZWarehouse

		public void TestControlVisibility_FTZWarehouse()
		{
			var ftzWarehouse = Helper.CreateWarehouse("FTZ");
			ftzWarehouse.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
			var row = Helper.CreateRow(ftzWarehouse, "A");

			using (var form = new ZForm(row))
			{
				var control = new LocationsEditBaseControl();
				form.Controls.Add(control);
				form.Show();

				AssertControlVisible(control, "LocationTypeGuidFindBox", true);
				AssertControlVisible(control, "StatusDropEdit", true);
				AssertControlVisible(control, "MaxWeightCalcDropEdit", true);
				AssertControlVisible(control, "MaxCubicCalcDropEdit", true);
				AssertControlVisible(control, "PickingAreaGuidFindBox", true);
				AssertControlVisible(control, "PutawayAreaGuidFindBox", true);
				AssertControlVisible(control, "MaximumTouchCountCalcEdit", true);
				AssertControlVisible(control, "PickMethodDropEdit", true);
				AssertControlVisible(control, "SortByPathGroupBox", true);
				AssertControlVisible(control, "MaxQuantityCalcEdit", true);

				AssertControlVisible(control, "MaxHeightCalcEdit", false);
				AssertControlVisible(control, "MaxDepthCalcEdit", false);
				AssertControlVisible(control, "MaxWidthCalcEdit", false);
				AssertControlVisible(control, "MaxDimensionDropEdit", false);
				AssertControlVisible(control, "PalletFloorSpacesCalcEdit", true);
				AssertControlVisible(control, "PalletStackHeightCalcEdit", true);
			}
		}

		#endregion

		#region TestControlVisibility_TransitWarehouse

		public void TestControlVisibility_TransitWarehouse()
		{
			var trwWarehouse = Helper.CreateWarehouse("TRW");
			trwWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			var row = Helper.CreateRow(trwWarehouse, "A");

			using (var form = new ZForm(row))
			{
				var control = new LocationsEditBaseControl();
				form.Controls.Add(control);
				form.Show();

				AssertControlVisible(control, "LocationTypeGuidFindBox", true);
				AssertControlVisible(control, "StatusDropEdit", true);
				AssertControlVisible(control, "MaxWeightCalcDropEdit", true);
				AssertControlVisible(control, "MaxCubicCalcDropEdit", true);
				AssertControlVisible(control, "MaxHeightCalcEdit", true);
				AssertControlVisible(control, "MaxDepthCalcEdit", true);
				AssertControlVisible(control, "MaxWidthCalcEdit", true);
				AssertControlVisible(control, "MaxDimensionDropEdit", true);
				AssertControlVisible(control, "PalletFloorSpacesCalcEdit", true);
				AssertControlVisible(control, "PalletStackHeightCalcEdit", true);
				AssertControlVisible(control, "PickingAreaGuidFindBox", true);
				AssertControlVisible(control, "PutawayAreaGuidFindBox", true);
				AssertControlVisible(control, "SortByPathGroupBox", true);
				AssertControlVisible(control, "PickPathSortMethodDropEdit", true);

				AssertControlVisible(control, "MaximumTouchCountCalcEdit", false);
				AssertControlVisible(control, "PickMethodDropEdit", false);
				AssertControlVisible(control, "MaxQuantityCalcEdit", false);
			}
		}

		public void TestGridColumnVisibility_TransitWarehouse()
		{
			var trwWarehouse = Helper.CreateWarehouse("TRW");
			trwWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			var row = Helper.CreateRow(trwWarehouse, "A");

			using (var form = new ZForm(row))
			{
				var control = new LocationsEditBaseControl();
				form.Controls.Add(control);
				form.Show();

				var gridColumns = control.Grid.Columns;
				AssertEquals("RowName should be visible.", true, gridColumns.Contains("RowName"));
				AssertEquals("FormattedColumn should be visible.", true, gridColumns.Contains("FormattedColumn"));
				AssertEquals("FormattedLevel should be visible.", true, gridColumns.Contains("FormattedLevel"));
				AssertEquals("FormattedTray should be visible.", true, gridColumns.Contains("FormattedTray"));
				AssertEquals("WLV_WA_PickingArea should be visible.", true, gridColumns.Contains(WhsLocationViewSchema.Constants.WLV_WA_PickingArea));
				AssertEquals("WLV_WA_PutawayArea should be visible.", true, gridColumns.Contains(WhsLocationViewSchema.Constants.WLV_WA_PutawayArea));
				AssertEquals("WLV_LocationStatus should be visible.", true, gridColumns.Contains(WhsLocationViewSchema.Constants.WLV_LocationStatus));
				AssertEquals("WLV_WLT_LocationType should be visible.", true, gridColumns.Contains(WhsLocationViewSchema.Constants.WLV_WLT_LocationType));
				AssertEquals("WLV_PickPathSequence should be visible.", true, gridColumns.Contains(WhsLocationViewSchema.Constants.WLV_PickPathSequence));
				AssertEquals("WLV_MaxWeight should be visible.", true, gridColumns.Contains(WhsLocationViewSchema.Constants.WLV_MaxWeight));
				AssertEquals("WLV_MaxWeightUnit should be visible.", true, gridColumns.Contains(WhsLocationViewSchema.Constants.WLV_MaxWeightUnit));
				AssertEquals("WLV_MaxCubic should be visible.", true, gridColumns.Contains(WhsLocationViewSchema.Constants.WLV_MaxCubic));
				AssertEquals("WLV_MaxCubicUnit should be visible.", true, gridColumns.Contains(WhsLocationViewSchema.Constants.WLV_MaxCubicUnit));
				AssertEquals("WLV_TransitDischargeLRC should be visible.", true, gridColumns.Contains(WhsLocationViewSchema.Constants.WLV_TransitDischargeLRC));
				AssertEquals("WLV_RS_NKTransitServiceLevel should be visible.", true, gridColumns.Contains(WhsLocationViewSchema.Constants.WLV_RS_NKTransitServiceLevel));
				AssertEquals("WLV_CycleCountLastPerformedForBinding should be visible.", true, gridColumns.Contains(nameof(WhsLocation.WLV_CycleCountLastPerformedForBinding)));
				AssertEquals("WLV_CycleCountPathSequence should be visible.", true, gridColumns.Contains(nameof(WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence)));

				AssertEquals("WLV_PickMethod should NOT be visible.", false, gridColumns.Contains(WhsLocationViewSchema.Constants.WLV_PickMethod));
				AssertEquals("WLV_MaximumPickCountBeforeAutomatedStocktake should NOT be visible.", false, gridColumns.Contains(WhsLocationViewSchema.Constants.WLV_MaximumPickCountBeforeAutomatedStocktake));
				AssertEquals("WLV_FinalisedPickCount should NOT be visible.", false, gridColumns.Contains(WhsLocationViewSchema.Constants.WLV_FinalisedPickCount));
				AssertEquals("WLV_PutawayPathSequence should NOT be visible.", false, gridColumns.Contains(WhsLocationViewSchema.Constants.WLV_PutawayPathSequence));
				AssertEquals("WLV_LastInventoryChangeDate should NOT be visible.", false, gridColumns.Contains(WhsLocationViewSchema.Constants.WLV_LastInventoryChangeDate));
				AssertEquals("WLV_MaxQuantity should be NOT visible.", false, gridColumns.Contains(WhsLocationViewSchema.Constants.WLV_MaxQuantity));
				AssertEquals("RowLocationSequence should NOT be visible.", false, gridColumns.Contains("RowLocationSequence"));
			}
		}

		#endregion

		#region TestControlVisibility_ContainerYardWarehouse

		public void TestControlVisibility_ContainerYardWarehouse()
		{
			var cydWarehouse = Helper.CreateWarehouse("CYD");
			cydWarehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			var row = Helper.CreateRow(cydWarehouse, "A");

			using (var form = new ZForm(row))
			{
				var control = new LocationsEditBaseControl();
				form.Controls.Add(control);
				form.Show();

				AssertControlVisibleAndLocation(control, "LocationTypeGuidFindBox", true, CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 22, true));
				AssertControlVisibleAndLocation(control, "StatusDropEdit", true, CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 46, true));
				AssertControlVisibleAndLocation(control, "MaxQuantityCalcEdit", true, CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 70, true));
				AssertControlVisible(control, "PutawayAreaGuidFindBox", true);

				AssertControlVisible(control, "MaxHeightCalcEdit", false);
				AssertControlVisible(control, "MaxDepthCalcEdit", false);
				AssertControlVisible(control, "MaxWidthCalcEdit", false);
				AssertControlVisible(control, "MaxDimensionDropEdit", false);
				AssertControlVisible(control, "PalletFloorSpacesCalcEdit", false);
				AssertControlVisible(control, "PalletStackHeightCalcEdit", false);
				AssertControlVisible(control, "MaxWeightCalcDropEdit", false);
				AssertControlVisible(control, "MaxCubicCalcDropEdit", false);
				AssertControlVisible(control, "PickingAreaGuidFindBox", false);
				AssertControlVisible(control, "MaximumTouchCountCalcEdit", false);
				AssertControlVisible(control, "PickMethodDropEdit", false);
			}
		}

		void AssertControlVisible(LocationsEditBaseControl baseControl, string controlName, bool expectedVisible)
		{
			AssertControlVisibleAndLocation(baseControl, controlName, expectedVisible, Point.Empty);
		}

		void AssertControlVisibleAndLocation(LocationsEditBaseControl baseControl, string controlName, bool expectedVisible, Point expectedLocation)
		{
			var control = baseControl.Controls.Find(controlName, true).Single();
			AssertEquals($"{controlName}'s Visible", expectedVisible, control.Visible);
			if (expectedLocation != Point.Empty)
			{
				AssertEquals($"{controlName}'s Visible", expectedLocation, control.Location);
			}
		}

		public void TestGridColumnVisibility_ContainerYardWarehouse()
		{
			var cydWarehouse = Helper.CreateWarehouse("CYD");
			cydWarehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			var row = Helper.CreateRow(cydWarehouse, "A");

			using (var form = new ZForm(row))
			{
				var control = new LocationsEditBaseControl();
				form.Controls.Add(control);
				form.Show();

				var gridColumns = control.Grid.Columns;
				AssertEquals("RowName should be visible.", true, gridColumns.Contains("RowName"));
				AssertEquals("FormattedColumn should be visible.", true, gridColumns.Contains("FormattedColumn"));
				AssertEquals("FormattedLevel should be visible.", true, gridColumns.Contains("FormattedLevel"));
				AssertEquals("WLV_LocationStatus should be visible.", true, gridColumns.Contains(WhsLocationViewSchema.Constants.WLV_LocationStatus));
				AssertEquals("WLV_WLT_LocationType should be visible.", true, gridColumns.Contains(WhsLocationViewSchema.Constants.WLV_WLT_LocationType));
				AssertEquals("WLV_MaxQuantity should be visible.", true, gridColumns.Contains(WhsLocationViewSchema.Constants.WLV_MaxQuantity));
				AssertEquals("WLV_PickPathSequence should be visible.", true, gridColumns.Contains(WhsLocationViewSchema.Constants.WLV_PickPathSequence));
				AssertEquals("WLV_PutawayPathSequence should be visible.", true, gridColumns.Contains(WhsLocationViewSchema.Constants.WLV_PutawayPathSequence));
				AssertEquals("WLV_WA_PutawayArea should be visible.", true, gridColumns.Contains(WhsLocationViewSchema.Constants.WLV_WA_PutawayArea));

				AssertEquals("FormattedTray should NOT be visible.", false, gridColumns.Contains("FormattedTray"));
				AssertEquals("WLV_WA_PickingArea should NOT be visible.", false, gridColumns.Contains(WhsLocationViewSchema.Constants.WLV_WA_PickingArea));
				AssertEquals("WLV_PickMethod should NOT be visible.", false, gridColumns.Contains(WhsLocationViewSchema.Constants.WLV_PickMethod));
				AssertEquals("WLV_MaxWeight should NOT be visible.", false, gridColumns.Contains(WhsLocationViewSchema.Constants.WLV_MaxWeight));
				AssertEquals("WLV_MaxWeightUnit should NOT be visible.", false, gridColumns.Contains(WhsLocationViewSchema.Constants.WLV_MaxWeightUnit));
				AssertEquals("WLV_MaxCubic should NOT be visible.", false, gridColumns.Contains(WhsLocationViewSchema.Constants.WLV_MaxCubic));
				AssertEquals("WLV_MaxCubicUnit should NOT be visible.", false, gridColumns.Contains(WhsLocationViewSchema.Constants.WLV_MaxCubicUnit));
				AssertEquals("WLV_MaximumPickCountBeforeAutomatedStocktake should NOT be visible.", false, gridColumns.Contains(WhsLocationViewSchema.Constants.WLV_MaximumPickCountBeforeAutomatedStocktake));
				AssertEquals("WLV_FinalisedPickCount should NOT be visible.", false, gridColumns.Contains(WhsLocationViewSchema.Constants.WLV_FinalisedPickCount));
				AssertEquals("WLV_CycleCountLastPerformedForBinding should NOT be visible.", false, gridColumns.Contains(nameof(WhsLocation.WLV_CycleCountLastPerformedForBinding)));
				AssertEquals("WLV_TransitDischargeLRC should NOT be visible.", false, gridColumns.Contains(WhsLocationViewSchema.Constants.WLV_TransitDischargeLRC));
				AssertEquals("WLV_RS_NKTransitServiceLevel should NOT be visible.", false, gridColumns.Contains(WhsLocationViewSchema.Constants.WLV_RS_NKTransitServiceLevel));
				AssertEquals("RowLocationSequence should NOT be visible.", false, gridColumns.Contains("RowLocationSequence"));
			}
		}

		#endregion

		#endregion

		#region  TestDischargeAndServiceLevelColumnVisibility

		public void TestDischargeAndServiceLevelColumnVisibility()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRow(warehouse, "row");

			AssertDischargeAndServiceLevelColumnVisibility(warehouse, row, warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit);
			AssertDischargeAndServiceLevelColumnVisibility(warehouse, row, warehouse.WW_WarehouseType = WarehouseTypes.Codes.Product);
		}

		void AssertDischargeAndServiceLevelColumnVisibility(WhsWarehouse warehouse, WhsRow row, string warehouseType)
		{
			warehouse.WW_WarehouseType = warehouseType;
			var isTransitWarehouse = warehouseType == WarehouseTypes.Codes.Transit;
			using (var form = new ZForm(row))
			{
				var control = new LocationsEditBaseControl();
				form.Controls.Add(control);
				form.Show();

				var dischargeVisible = control.Grid.Columns.Contains(WhsLocationViewSchema.Constants.WLV_TransitDischargeLRC);
				var serviceLevelVisible = control.Grid.Columns.Contains(WhsLocationViewSchema.Constants.WLV_RS_NKTransitServiceLevel);

				AssertEquals($"column 'Discharge' visibility should be {isTransitWarehouse}", isTransitWarehouse, dischargeVisible);
				AssertEquals($"column 'Service Level' visibility should be {isTransitWarehouse}", isTransitWarehouse, serviceLevelVisible);
			}
		}

		#endregion

		#region TestCycleCountColumnVisibility

		public void TestControlVisibility_3PLCycleCount()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRow(warehouse, "row", 1, 1);

			using (var form = new ZForm(row))
			{
				var control = new LocationsEditBaseControl();
				form.Controls.Add(control);
				form.Show();

				AssertControlVisible(control, "SortByPathGroupBox", true);
				AssertControlVisible(control, "SelectAllButton", true);
				AssertControlVisible(control, "SelectedUpdateButton", true);
				AssertControlVisible(control, "PickPathSortMethodDropEdit", true);
				AssertControlVisible(control, "PutawayPathSortMethodDropEdit", true);
				AssertControlVisible(control, "CycleCountSortMethodDropEdit", true);
			}
		}

		public void TestControlVisibility_TRWCycleCount()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRow(warehouse, "row", 1, 1);

			using (var form = new ZForm(row))
			{
				var control = new LocationsEditBaseControl();
				form.Controls.Add(control);
				form.Show();

				AssertControlVisible(control, "SortByPathGroupBox", true);
				AssertControlVisible(control, "CycleCountSortMethodDropEdit", true);
				AssertControlVisible(control, "SelectAllButton", true);
				AssertControlVisible(control, "SelectedUpdateButton", true);
				AssertControlVisible(control, "PickPathSortMethodDropEdit", true);
				AssertControlVisible(control, "PutawayPathSortMethodDropEdit", false);
			}
		}

		public void TestCycleCountLastPerformedVisibility()
		{
			TestCycleCountColumnVisibilityCore(nameof(WhsLocation.WLV_CycleCountLastPerformedForBinding));
		}

		public void TestCycleCountPathSequenceVisibility()
		{
			TestCycleCountColumnVisibilityCore(WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence);
		}

		void TestCycleCountColumnVisibilityCore(string columnName)
		{
			TestColumnVisibilityWithWarehouseTypeCore(columnName, WarehouseTypes.Codes.ContainerYard, false);
			TestColumnVisibilityWithWarehouseTypeCore(columnName, WarehouseTypes.Codes.FreeTradeZone, false);
			TestColumnVisibilityWithWarehouseTypeCore(columnName, WarehouseTypes.Codes.Transit, true);
			TestColumnVisibilityWithWarehouseTypeCore(columnName, WarehouseTypes.Codes.Product, true);
		}

		void TestColumnVisibilityWithWarehouseTypeCore(string columnName, string warehouseType, bool expectedVisible)
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			warehouse.WW_WarehouseType = warehouseType;
			var row = Helper.CreateRow(warehouse, "row", 1, 1);

			using (var form = new ZForm(row))
			{
				var control = new LocationsEditBaseControl();
				form.Controls.Add(control);
				form.Show();

				AssertEquals($"Column '{columnName}' visibility for Warehoue Type {warehouseType} should be {expectedVisible}", expectedVisible, control.Grid.Columns.Contains(columnName));
			}
		}

		#endregion

		#region TestPickPathSequenceColumnVisibility

		public void TestPickPathSequenceColumnVisibility()
		{
			var columnName = WhsLocationViewSchema.Constants.WLV_PickPathSequence;
			TestColumnVisibilityWithWarehouseTypeCore(columnName, WarehouseTypes.Codes.Product, true);
			TestColumnVisibilityWithWarehouseTypeCore(columnName, WarehouseTypes.Codes.ContainerYard, true);
			TestColumnVisibilityWithWarehouseTypeCore(columnName, WarehouseTypes.Codes.FreeTradeZone, true);
			TestColumnVisibilityWithWarehouseTypeCore(columnName, WarehouseTypes.Codes.Transit, true);
		}

		#endregion

		#region TestPutawayPathSequenceColumnVisibility

		public void TestPutawayPathSequenceColumnVisibility()
		{
			var columnName = WhsLocationViewSchema.Constants.WLV_PutawayPathSequence;
			TestColumnVisibilityWithWarehouseTypeCore(columnName, WarehouseTypes.Codes.Product, true);
			TestColumnVisibilityWithWarehouseTypeCore(columnName, WarehouseTypes.Codes.ContainerYard, true);
			TestColumnVisibilityWithWarehouseTypeCore(columnName, WarehouseTypes.Codes.FreeTradeZone, true);
			TestColumnVisibilityWithWarehouseTypeCore(columnName, WarehouseTypes.Codes.Transit, false);
		}

		#endregion

		#region TestMaxDimensionColumnVisibility

		public void TestMaxHeightColumnVisibility() => TestMaxDimensionColumnVisibility(WhsLocationViewSchema.Constants.WLV_MaxHeight);
		public void TestMaxDepthColumnVisibility() => TestMaxDimensionColumnVisibility(WhsLocationViewSchema.Constants.WLV_MaxDepth);
		public void TestMaxWidthColumnVisibility() => TestMaxDimensionColumnVisibility(WhsLocationViewSchema.Constants.WLV_MaxWidth);
		public void TestMaxDimensionUnit() => TestMaxDimensionColumnVisibility(WhsLocationViewSchema.Constants.WLV_MaxDimensionUnit);

		void TestMaxDimensionColumnVisibility(string columnName)
		{
			TestColumnVisibilityWithWarehouseTypeCore(columnName, WarehouseTypes.Codes.Product, false);
			TestColumnVisibilityWithWarehouseTypeCore(columnName, WarehouseTypes.Codes.ContainerYard, false);
			TestColumnVisibilityWithWarehouseTypeCore(columnName, WarehouseTypes.Codes.FreeTradeZone, false);
			TestColumnVisibilityWithWarehouseTypeCore(columnName, WarehouseTypes.Codes.Transit, true);
		}

		#endregion

		#region TestPalletFloorSpacesCalcEditColumnVisibility

		public void TestPalletFloorSpacesCalcEditColumnVisibility()
		{
			var columnName = WhsLocationViewSchema.Constants.WLV_PalletFloorSpaces;
			TestColumnVisibilityWithWarehouseTypeCore(columnName, WarehouseTypes.Codes.ContainerYard, false);
			TestColumnVisibilityWithWarehouseTypeCore(columnName, WarehouseTypes.Codes.Product, true);
			TestColumnVisibilityWithWarehouseTypeCore(columnName, WarehouseTypes.Codes.FreeTradeZone, true);
			TestColumnVisibilityWithWarehouseTypeCore(columnName, WarehouseTypes.Codes.Transit, true);
		}

		#endregion

		#region TestPalletStackHeightCalcEditColumnVisibility

		public void TestPalletStackHeightCalcEditColumnVisibility()
		{
			var columnName = WhsLocationViewSchema.Constants.WLV_PalletStackHeight;
			TestColumnVisibilityWithWarehouseTypeCore(columnName, WarehouseTypes.Codes.Product, true);
			TestColumnVisibilityWithWarehouseTypeCore(columnName, WarehouseTypes.Codes.FreeTradeZone, true);
			TestColumnVisibilityWithWarehouseTypeCore(columnName, WarehouseTypes.Codes.ContainerYard, false);
			TestColumnVisibilityWithWarehouseTypeCore(columnName, WarehouseTypes.Codes.Transit, true);
		}

		#endregion

		#region TestCheckDigitColumnVisibility

		public void TestFormattedCheckDigitColumnVisibility()
		{
			var columnName = "FormattedCheckDigit";

			TestColumnVisibilityWithWarehouseTypeCore(columnName, WarehouseTypes.Codes.Product, true);
			TestColumnVisibilityWithWarehouseTypeCore(columnName, WarehouseTypes.Codes.FreeTradeZone, true);
			TestColumnVisibilityWithWarehouseTypeCore(columnName, WarehouseTypes.Codes.ContainerYard, false);
			TestColumnVisibilityWithWarehouseTypeCore(columnName, WarehouseTypes.Codes.Transit, false);
		}

		#endregion

		#region TestCheckDigitCheckBoxVisibility

		public void TestCheckDigitCheckBoxVisibility()
		{
			TestCheckDigitCheckBoxVisibilityCore(WarehouseTypes.Codes.Product, true);
			TestCheckDigitCheckBoxVisibilityCore(WarehouseTypes.Codes.FreeTradeZone, true);
			TestCheckDigitCheckBoxVisibilityCore(WarehouseTypes.Codes.ContainerYard, false);
			TestCheckDigitCheckBoxVisibilityCore(WarehouseTypes.Codes.Transit, false);
		}

		void TestCheckDigitCheckBoxVisibilityCore(string warehouseType, bool expectedVisible)
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			warehouse.WW_WarehouseType = warehouseType;
			var row = Helper.CreateRow(warehouse, "row", 1, 1);

			using (var form = new ZForm(row))
			{
				var control = new LocationsEditBaseControl();
				form.Controls.Add(control);
				form.Show();

				AssertControlVisible(control, "GenerateCheckDigitCheckbox", expectedVisible);
			}
		}

		#endregion

		#region IRowForm Members

		public void TestSelectedLocations()
		{
			var whs = Helper.CreateWarehouse("1", "A", 4, 2);
			var row = whs.Rows.Single(r => r.WR_Name == "A");

			using (var testForm = new ZForm(row))
			using (var control = new LocationsEditBaseControl())
			{
				testForm.Controls.Add(control);
				testForm.Show();
				control.SetDataBinding(row, "");
				AssertEquals(control, row.ParentForm);
				AssertEquals(0, control.SelectedLocations.Count);

				control.LocationGrid.Select(0);
				control.LocationGrid.Select(2);
				control.LocationGrid.Select(4);
				control.LocationGrid.Select(6);

				AssertEquals(4, control.SelectedLocations.Count);
				AssertEquals(row.Locations[0], control.SelectedLocations[0]);
				AssertEquals(row.Locations[2], control.SelectedLocations[1]);
				AssertEquals(row.Locations[4], control.SelectedLocations[2]);
				AssertEquals(row.Locations[6], control.SelectedLocations[3]);
			}
		}

		#endregion

		#region SelectedUpdateButton

		public void TestSelectedUpdateButton()
		{
			var whs = Helper.CreateWarehouse("1", "A", 4, 2);
			var pickingArea = whs.Areas.AddNew();
			pickingArea.WA_Name = "PIC";
			var putawayArea = whs.Areas.AddNew();
			putawayArea.WA_Name = "PUT";
			var row = whs.Rows.Single(r => r.WR_Name == "A");
			var locationType = Helper.CreateLocationType("ROD");

			using (var testForm = new ZForm(row))
			using (var control = new LocationsEditBaseControl())
			{
				testForm.Controls.Add(control);
				testForm.Show();
				control.SetDataBinding(row, "");

				control.LocationGrid.Select(0);
				control.LocationGrid.Select(2);
				control.LocationGrid.Select(4);
				control.LocationGrid.Select(6);

				row.MaxWeight = 250m;
				row.MaxWeightUnit = "KG";
				row.MaxCubic = 0.55m;
				row.MaxCubicUnit = "M3";
				row.LocationType = locationType.PK;
				row.LocationStatus = LocationStatus.Codes.Held;
				row.PickMethod = "TST";
				row.PickingArea_MassUpdate = pickingArea.PK;
				row.PutawayArea_MassUpdate = putawayArea.PK;

				control.SelectedUpdateBtn.PerformClick();

				for (int i = 0; i < 8; i += 2)
				{
					AssertEquals(250m, row.Locations[i].WLV_MaxWeight);
					AssertEquals("KG", row.Locations[i].WLV_MaxWeightUnit);
					AssertEquals(0.55m, row.Locations[i].WLV_MaxCubic);
					AssertEquals("M3", row.Locations[i].WLV_MaxCubicUnit);
					AssertEquals("ROD", row.Locations[i].LocationType.WLT_Code);
					AssertEquals(LocationStatus.Codes.Held, row.Locations[i].WLV_LocationStatus);
					AssertEquals("TST", row.Locations[i].WLV_PickMethod);
					AssertEquals(pickingArea, row.Locations[i].PickingArea);
					AssertEquals(putawayArea, row.Locations[i].PutawayArea);
				}
				for (int j = 1; j < 8; j += 2)
				{
					AssertEquals(0m, row.Locations[j].WLV_MaxWeight);
					AssertEquals("", row.Locations[j].WLV_MaxWeightUnit);
					AssertEquals(0m, row.Locations[j].WLV_MaxCubic);
					AssertEquals("", row.Locations[j].WLV_MaxCubicUnit);
					AssertEquals("RNO", row.Locations[j].LocationType.WLT_Code);
					AssertEquals(LocationStatus.Codes.Normal, row.Locations[j].WLV_LocationStatus);
					AssertEquals("ANY", row.Locations[j].WLV_PickMethod);
					AssertEquals(whs.Areas[0], row.Locations[j].PickingArea);
					AssertEquals(whs.Areas[0], row.Locations[j].PutawayArea);
				}
			}
		}

		#endregion

		#region SelectAll Button

		public void TestSelectAllButton()
		{
			var whs = Helper.CreateWarehouse("1", "A", 4, 2);
			var row = whs.Rows.Single(r => r.WR_Name == "A");

			using (var testForm = new ZForm(row))
			using (var control = new LocationsEditBaseControl())
			{
				testForm.Controls.Add(control);
				testForm.Show();
				control.SetDataBinding(row, "");
				AssertEquals(0, control.LocationGrid.SelectedRowCount);
				control.SelectAllBtn.PerformClick();
				AssertEquals(8, control.LocationGrid.SelectedRowCount);
			}
		}

		#endregion

		#region UpdatePathSequenceOrderButton

		public void TestUpdatePathSequenceOrderButton()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = warehouse.Rows.AddNew();
			row.WR_Name = "A";
			row.WR_Trays = 3;
			AssertEquals("Precondition", 0, row.Locations.Count);

			Factory.Save();
			AssertEquals(3, row.Locations.Count);

			var location1 = GetLocation(row, 1, 1, 1);
			var location2 = GetLocation(row, 1, 1, 2);
			var location3 = GetLocation(row, 1, 1, 3);

			AssertEquals("Precondition", 1, location1.WLV_PickPathSequence);
			AssertEquals("Precondition", 1, location2.WLV_PickPathSequence);
			AssertEquals("Precondition", 1, location3.WLV_PickPathSequence);

			AssertEquals("Precondition", 1, location1.WLV_PutawayPathSequence);
			AssertEquals("Precondition", 1, location2.WLV_PutawayPathSequence);
			AssertEquals("Precondition", 1, location3.WLV_PutawayPathSequence);

			AssertEquals("Precondition", 1, location1.WLV_CycleCountPathSequence);
			AssertEquals("Precondition", 1, location2.WLV_CycleCountPathSequence);
			AssertEquals("Precondition", 1, location3.WLV_CycleCountPathSequence);

			using (var testForm = new ZForm(row))
			using (var control = new LocationsEditBaseControl())
			{
				testForm.Controls.Add(control);
				testForm.Show();
				control.SetDataBinding(row, "");
				row.SortCycleCountMethod = SortPathMethods.Codes.ColumnThenLevel;
				row.SortPickPathMethod = SortPathMethods.Codes.ColumnThenLevel;
				row.SortPutawayPathMethod = SortPathMethods.Codes.ColumnThenLevel;

				control.UpdatePathSequenceOrderButton.PerformClick();

				AssertEquals("Pick path sequence is updated.", 1, location1.WLV_PickPathSequence);
				AssertEquals("Pick path sequence is updated.", 2, location2.WLV_PickPathSequence);
				AssertEquals("Pick path sequence is updated.", 3, location3.WLV_PickPathSequence);
				AssertEquals("Putaway path sequence is updated.", 1, location1.WLV_PutawayPathSequence);
				AssertEquals("Putaway path sequence is updated.", 2, location2.WLV_PutawayPathSequence);
				AssertEquals("Putaway path sequence is updated.", 3, location3.WLV_PutawayPathSequence);
				AssertEquals("Cycle count path sequence is updated.", 1, location1.WLV_CycleCountPathSequence);
				AssertEquals("Cycle count path sequence is updated.", 2, location2.WLV_CycleCountPathSequence);
				AssertEquals("Cycle count path sequence is updated.", 3, location3.WLV_CycleCountPathSequence);
			}
		}

		public void TestUpdatePathSequenceOrderButton_InvalidSortPathMethod()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = warehouse.Rows.AddNew();
			row.WR_Name = "A";
			row.WR_Trays = 3;
			AssertEquals("Precondition", 0, row.Locations.Count);

			Factory.Save();
			AssertEquals(3, row.Locations.Count);

			var location1 = GetLocation(row, 1, 1, 1);
			var location2 = GetLocation(row, 1, 1, 2);
			var location3 = GetLocation(row, 1, 1, 3);

			AssertEquals("Precondition", 1, location1.WLV_PickPathSequence);
			AssertEquals("Precondition", 1, location2.WLV_PickPathSequence);
			AssertEquals("Precondition", 1, location3.WLV_PickPathSequence);
			AssertEquals("Precondition", 1, location1.WLV_PutawayPathSequence);
			AssertEquals("Precondition", 1, location2.WLV_PutawayPathSequence);
			AssertEquals("Precondition", 1, location3.WLV_PutawayPathSequence);
			AssertEquals("Precondition", 1, location1.WLV_CycleCountPathSequence);
			AssertEquals("Precondition", 1, location2.WLV_CycleCountPathSequence);
			AssertEquals("Precondition", 1, location3.WLV_CycleCountPathSequence);

			using (var testForm = new ZForm(row))
			using (var control = new LocationsEditBaseControl())
			{
				testForm.Controls.Add(control);
				testForm.Show();
				control.SetDataBinding(row, "");
				row.SortCycleCountMethod = "ABC";
				row.SortPickPathMethod = SortPathMethods.Codes.ColumnThenLevel;

				control.UpdatePathSequenceOrderButton.PerformClick();

				AssertEquals("Should have error", true, UnitTestUserNotification.Instance.LastMessage.Contains("Invalid sort path method specified."));
				AssertEquals("Should have error", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Pick path sequence is not updated.", 1, location1.WLV_PickPathSequence);
				AssertEquals("Pick path sequence is not updated.", 1, location2.WLV_PickPathSequence);
				AssertEquals("Pick path sequence is not updated.", 1, location3.WLV_PickPathSequence);
				AssertEquals("Putaway path sequence is not updated.", 1, location1.WLV_PutawayPathSequence);
				AssertEquals("Putaway path sequence is not updated.", 1, location2.WLV_PutawayPathSequence);
				AssertEquals("Putaway path sequence is not updated.", 1, location3.WLV_PutawayPathSequence);
				AssertEquals("Cycle count path sequence is not updated.", 1, location1.WLV_CycleCountPathSequence);
				AssertEquals("Cycle count path sequence is not updated.", 1, location2.WLV_CycleCountPathSequence);
				AssertEquals("Cycle count path sequence is not updated.", 1, location3.WLV_CycleCountPathSequence);
			}
		}

		WhsLocation GetLocation(WhsRow row, short column, short level, short tray) => row.Locations.Single(o => o.WLV_Column == column && o.WLV_Level == level && o.WLV_Tray == tray);

		#endregion

		#region TestResetCurrentTouchCount

		public void TestNoResetCurrentTouchCountMenu_WhenWarehouseIsContainerYard()
		{
			var whs = Helper.CreateWarehouse("CYD", "A", 5, 1);
			whs.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			Factory.Save();

			using (var testForm = new ZForm(whs.Rows.Single(r => r.WR_Name == "A")))
			using (var control = new LocationsEditBaseControl())
			{
				testForm.Controls.Add(control);
				testForm.Show();

				var contextMenu = control.Grid.ContextMenu;
				var resetCTCMenuItem = contextMenu.MenuItems.FindByText("Reset Current Touch Count");
				AssertNull(resetCTCMenuItem);
			}
		}

		public void TestResetCurrentTouchCount()
		{
			var whs = Helper.CreateWarehouse("1", "A", 5, 1);
			Factory.Save();

			using (var testForm = new ZForm(whs.Rows.Single(r => r.WR_Name == "A")))
			using (var control = new LocationsEditBaseControl())
			{
				testForm.Controls.Add(control);
				testForm.Show();

				// setup locations
				var locationWithZeroCTC1 = whs.FindLocation("A-1");
				var locationWithNonZeroCTC1 = whs.FindLocation("A-2");
				var locationWithZeroCTC2 = whs.FindLocation("A-3");
				var locationWithNonZeroCTC2 = whs.FindLocation("A-4");
				var locationWithNonZeroCTC3 = whs.FindLocation("A-5");
				locationWithNonZeroCTC1.WLV_FinalisedPickCount = 1;
				locationWithNonZeroCTC2.WLV_FinalisedPickCount = 2;
				locationWithNonZeroCTC3.WLV_FinalisedPickCount = 3;

				// locations with Zero finalised pick counts
				control.Grid.SelectElements(new[] { locationWithZeroCTC1, locationWithZeroCTC2 });
				var contextMenu = control.Grid.ContextMenu;
				var resetCTCMenuItem = contextMenu.MenuItems.FindByText("Reset Current Touch Count");
				contextMenu.ShowPopupMenu();
				AssertEquals("Precondition", false, resetCTCMenuItem.Enabled);
				AssertEquals("Precondition - Finalised pick counts should not be changed.", 0, locationWithZeroCTC1.WLV_FinalisedPickCount);
				AssertEquals("Precondition - Finalised pick counts should not be changed.", 1, locationWithNonZeroCTC1.WLV_FinalisedPickCount);
				AssertEquals("Precondition - Finalised pick counts should not be changed.", 0, locationWithZeroCTC2.WLV_FinalisedPickCount);
				AssertEquals("Precondition - Finalised pick counts should not be changed.", 2, locationWithNonZeroCTC2.WLV_FinalisedPickCount);
				AssertEquals("Precondition - Finalised pick counts should not be changed.", 3, locationWithNonZeroCTC3.WLV_FinalisedPickCount);

				// locations with Zero finalised pick count and non Zero courrent touch count
				control.Grid.SelectElements(new[] { locationWithNonZeroCTC1, locationWithZeroCTC2, locationWithNonZeroCTC2 });
				contextMenu.ShowPopupMenu();
				AssertEquals("Precondition", true, resetCTCMenuItem.Enabled);
				resetCTCMenuItem.PerformClick();
				AssertEquals("Since location current touch count is Zero it should not be changed.", 0, locationWithZeroCTC1.WLV_FinalisedPickCount);
				AssertEquals("Since location is selected, finalised pick count should be reset.", 0, locationWithNonZeroCTC1.WLV_FinalisedPickCount);
				AssertEquals("since location is selected, finalised pick count should be reset.", 0, locationWithZeroCTC2.WLV_FinalisedPickCount);
				AssertEquals("since location is selected, finalised pick count should be reset.", 0, locationWithNonZeroCTC2.WLV_FinalisedPickCount);
				AssertEquals("since location is not selected, finalised pick count should be remain same.", 3, locationWithNonZeroCTC3.WLV_FinalisedPickCount);

				// location with non Zero finalised pick count
				control.Grid.SelectElements(new[] { locationWithNonZeroCTC3 });
				contextMenu.ShowPopupMenu();
				AssertEquals("Precondition", true, resetCTCMenuItem.Enabled);
				resetCTCMenuItem.PerformClick();
				AssertEquals("since location is not selected, finalised pick count should be remain same.", 0, locationWithZeroCTC1.WLV_FinalisedPickCount);
				AssertEquals("since location is not selected, finalised pick count should be remain same.", 0, locationWithNonZeroCTC1.WLV_FinalisedPickCount);
				AssertEquals("since location is not selected, finalised pick count should be remain same.", 0, locationWithZeroCTC2.WLV_FinalisedPickCount);
				AssertEquals("since location is not selected, finalised pick count should be remain same.", 0, locationWithNonZeroCTC2.WLV_FinalisedPickCount);
				AssertEquals("since location is selected, finalised pick count should be reset.", 0, locationWithNonZeroCTC3.WLV_FinalisedPickCount);
			}
		}

		#endregion

		#region Binding

		public void TestBinding()
		{
			using (LocationsEditBaseControl control = new LocationsEditBaseControl())
			{
				bool colIsShown = false;
				bool levelIsShown = false;
				bool trayIsShown = false;
				foreach (ZGridColumnInfo column in control.Grid.ColumnStyles)
				{
					if (!colIsShown && column.ColumnName == "FormattedColumn")
					{
						Assert("column 'FormattedColumn' is hidden", column.IsVisible);
						colIsShown = true;
					}
					if (!levelIsShown && column.ColumnName == "FormattedLevel")
					{
						Assert("column 'FormattedLevel' is hidden", column.IsVisible);
						levelIsShown = true;
					}
					if (!trayIsShown && column.ColumnName == "FormattedTray")
					{
						Assert("column 'FormattedTray' is hidden", column.IsVisible);
						trayIsShown = true;
					}
				}
				Assert("There is no column binded to property 'FormattedColumn'", colIsShown);
				Assert("There is no column binded to property 'FormattedLevel'", levelIsShown);
				Assert("There is no column binded to property 'FormattedTray'", trayIsShown);
			}
		}

		#endregion
	}

	#region ZGridExtension

	static class ZGridExtension
	{
		public static void SelectElements(this ZGrid grid, params BusinessObject[] bizObjs)
		{
			var list = grid.List;
			var listManager = grid.ListManager;

			grid.UnSelectAll();

			if (bizObjs != null && bizObjs.Length > 0)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (bizObjs.Contains(list[i]))
					{
						grid.Select(i);
					}
				}
			}
		}
	}

	#endregion
}
