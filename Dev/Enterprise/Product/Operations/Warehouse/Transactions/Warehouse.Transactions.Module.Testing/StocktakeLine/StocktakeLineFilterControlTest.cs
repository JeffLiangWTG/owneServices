using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public class StocktakeLineFilterControlTest : TestCaseWithFactory
	{
		#region TestSerialNumberColumnInitialised

		public void TestSerialNumberColumnInitialised()
		{
			using (var control = new StocktakeLineFilterControl(Factory.New<WhsStocktake>()))
			{
				AssertEquals(false,
					control.Grid.ColumnStyles.Cast<ZGridColumnInfo>().Single(c => c.ColumnName == WhsStocktakeLineSchema.Constants.WU_SerialNumber).IsUnavailable);
			}
		}

		#endregion

		#region TestGrid

		public void TestGrid()
		{
			using (var control = new StocktakeLineFilterControl(Factory.New<WhsStocktake>()))
			{
				AssertEquals("Grid control should be ZFilterGrid type.", typeof(ZFilterStocktakeLinesGrid), control.Grid.GetType());
				AssertEquals("Grid should not be readonly.", false, control.Grid.ReadOnly);
			}
		}

		#endregion

		#region TestGrid_RowChangedEvent

		public void TestGrid_RowChangedEvent()
		{
			// setup test data

			var now = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			// Setup stocktake and two duplicate lines and one unique line

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var openLineWithAvailableStatus1 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, now.AddDays(2), now.AddDays(3), "PA1", "PA2", "PA3", StocktakeLineStatus.Codes.Open, StocktakeInventoryStatus.Codes.Available);
			var openLineWithAvailableStatus2 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, now.AddDays(2), now.AddDays(3), "PA1", "PA2", "PA3", StocktakeLineStatus.Codes.Open, StocktakeInventoryStatus.Codes.Available, true);
			var openLineWithDamagedStatus = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, now.AddDays(2), now.AddDays(3), "PA1", "PA2", "PA3", StocktakeLineStatus.Codes.Open, StocktakeInventoryStatus.Codes.Damaged);

			using (var form = new ZForm(stocktake))
			{
				var filter = new StocktakeLineFilterControlForTest(stocktake);
				form.Controls.Add(filter);
				form.Show();

				filter.Grid.CurrentRowIndex = 1; // Trigger row change event

				AssertNoRowErrors(openLineWithAvailableStatus1);
				AssertHasRowError(openLineWithAvailableStatus2, "Product P1 in Location A with status AVL is already on this stocktake on Line 1. You must edit the existing stock take line. If you cannot see the line clear all filters.");
				AssertNoRowErrors(openLineWithDamagedStatus);
			}
		}

		#endregion

		public void TestGrid_ShouldBindDecimalPlacesToColumnStyle()
		{
			// Arrange

			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);

			using (var form = new ZForm(stocktake))
			{
				var filter = new StocktakeLineFilterControlForTest(stocktake);
				form.Controls.Add(filter);

				// Act

				form.Show();

				// Assert

				var numericalColumnStyles = filter
					.Grid.ColumnStyles
					.OfType<ZCalcEditColumnStyleInfo>()
					.Distinct()
					.ToArray();

				var decimalColumnStyles = numericalColumnStyles
					.Where(style => style.ColumnName != WhsStocktakeLineSchema.Constants.WU_LineNo)
					.ToArray();

				AssertEquals("Expecting 6 columns with decimal data type", 6, decimalColumnStyles.Length);
				AssertEquals("Expecting all decimal columns (except 'Line No.') to have 'Decimal Places' binding", true, decimalColumnStyles.All(column => column.BindToDecimalPlaces == "DecimalPlaces"));

				var lineNoColumnStyle = numericalColumnStyles.Single(style => style.ColumnName == WhsStocktakeLineSchema.Constants.WU_LineNo);
				AssertEquals("Expecting 'Line No.' to have no 'Decimal Places' binding", null, lineNoColumnStyle.BindToDecimalPlaces);
			}
		}

		#region TestSearchButton

		#region TestPerformSearch

		public void TestPerformSearch()
		{
			var searched = false;
			var data = new TestDataSimpleEnvironment(Factory);
			var whsRow1 = Helper.CreateRowAndGenerateLocations(data.Whs1, "Row1", 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, whsRow1.Locations[0]);
			// whsRow1.Locations[1] is empty

			// Setup test data

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, null, null, StocktakeStatus.Codes.Loaded);
			var line1 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, StocktakeLineStatus.Codes.Open);
			var line2 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, StocktakeLineStatus.Codes.Open);
			var line3 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, StocktakeLineStatus.Codes.Closed);

			// Test perform search

			using (var form = new ZForm(stocktake))
			{
				AssertEquals("All lines in the stocktake should be displayed.", 3, stocktake.StocktakeLinesForFilter.Count);
				var filter = new StocktakeLineFilterControlForTest(stocktake);
				form.Controls.Add(filter);
				form.Show();

				var statusFilter = (ModuleTextFilter)filter.FilterBusinessObject["Status"];
				statusFilter.Property = StocktakeLineStatus.Codes.Open;
				statusFilter.IsActive = true;

				filter.PerformSearch += delegate
				{ searched = true; };
				filter.FirePerformSearch();
				AssertEquals("Search action is not performed since stocktake lines are not saved.", false, searched);
				AssertEquals("Some stocktake lines have been modified. Save before searching again.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("All lines in the stocktake should be displayed.", 3, stocktake.StocktakeLinesForFilter.Count);

				Factory.Save();

				filter.FirePerformSearch();
				AssertEquals("Since all the lines has been saved, search action is performed.", true, searched);
				AssertEquals("Only opened lines are visible.", 2, stocktake.StocktakeLinesForFilter.Count);

				line1.WU_InventoryStatus = StocktakeInventoryStatus.Codes.Damaged;
				statusFilter.Property = StocktakeLineStatus.Codes.Closed;
				searched = false;
				filter.FirePerformSearch();
				AssertEquals("Search action is not performed since stocktake lines are not saved.", false, searched);
				AssertEquals("Previous items should be shown.", 2, stocktake.StocktakeLinesForFilter.Count);

				Factory.Save();
				statusFilter.Property = StocktakeLineStatus.Codes.Open;
				searched = false;
				filter.FirePerformSearch();
				AssertEquals("Since all the lines has been saved, search action is performed.", true, searched);
				AssertEquals("Only opened lines are visible.", 2, stocktake.StocktakeLinesForFilter.Count);
			}
		}

		#endregion

		#region TestPerformSearch_UnloadedStocktake

		public void TestPerformSearch_UnloadedStocktake()
		{
			var searched = false;
			var data = new TestDataSimpleEnvironment(Factory);

			// setup inventory, stocktake and a line

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var line = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			line.WI_WL = data.Whs1.DefaultLocation.PK;
			line.WI_InventoryStatus = InventoryStatus.Codes.Putaway;

			receive.FinaliseDocket();

			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var stocktake = Factory.New<WhsStocktake>(); // An empty stocktake

			using (var form = new ZForm(stocktake))
			{
				var filter = new StocktakeLineFilterControlForTest(stocktake);
				form.Controls.Add(filter);
				filter.PerformSearch += delegate
				{ searched = true; };
				form.Show();

				// Stocktake is not yet created.

				filter.FirePerformSearch();
				var statusFilter = (ModuleTextFilter)filter.FilterBusinessObject["Status"];
				statusFilter.Property = StocktakeLineStatus.Codes.Open;
				statusFilter.IsActive = true;

				AssertEquals("Search action is not performed since stocktake is not loaded.", false, searched);
				AssertEquals("Stocktake Lines should be loaded and saved before searching.", UnitTestUserNotification.Instance.LastMessage.Text);

				// Stocktake is created but not saved or loaded.

				stocktake.WS_WW_Whs = data.Whs1.PK;
				stocktake.WS_OH_Client = data.Org1.PK;
				AssertEquals("Search action is not performed since stocktake is not loaded.", false, searched);
				AssertEquals("Stocktake Lines should be loaded and saved before searching.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("No stocktake lines should be loaded.", 0, stocktake.StocktakeLinesForFilter.Count);

				// Stocktake is created and saved but not loaded.

				Factory.Save();
				filter.FirePerformSearch();
				AssertEquals("Search action is not performed since stocktake is not loaded.", false, searched);
				AssertEquals("Stocktake Lines should be loaded and saved before searching.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("No stocktake lines should be loaded.", 0, stocktake.StocktakeLinesForFilter.Count);

				// Stocktake is created, loaded and saved.

				stocktake.Load();
				Factory.Save();
				filter.FirePerformSearch();
				AssertEquals("Search action is performed since the stocktake is loaded and saved.", true, searched);
				AssertEquals("One stocktake line should be loaded.", 1, stocktake.StocktakeLinesForFilter.Count);
			}
		}

		#endregion

		#region TestPerformSearch_MakeSureGridDoesNotRefreshWhileEditing

		public void TestPerformSearch_MakeSureGridDoesNotRefreshWhileEditing()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// Setup test data

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, null, null, StocktakeStatus.Codes.Loaded);
			var line1 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, StocktakeLineStatus.Codes.Open);
			var line2 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, StocktakeLineStatus.Codes.Open);
			var line3 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, StocktakeLineStatus.Codes.Closed);

			Factory.Save();

			// Test perform search

			using (var form = new ZForm(stocktake))
			{
				AssertEquals("All lines in the stocktake should be displayed.", 3, stocktake.StocktakeLinesForFilter.Count);
				var filter = new StocktakeLineFilterControlForTest(stocktake);
				form.Controls.Add(filter);
				form.Show();

				var statusFilter = (ModuleTextFilter)filter.FilterBusinessObject["Status"];
				statusFilter.Property = StocktakeLineStatus.Codes.Open;
				statusFilter.IsActive = true;

				filter.FirePerformSearch();
				AssertEquals("Only opened lines are displayed.", 2, stocktake.StocktakeLinesForFilter.Count);

				line2.WU_Status = StocktakeLineStatus.Codes.Closed;
				Factory.Save();
				filter.FirePerformSearch();
				AssertEquals("Since search hasn't been performed, all stocktake lines shoudl be displayed.", 1, stocktake.StocktakeLinesForFilter.Count);

				var newLine = stocktake.StocktakeLinesForFilter.AddNew();
				newLine.WU_OP = data.Part2.PK;
				newLine.WU_WL = data.Whs1.DefaultLocation.PK;
				AssertEquals("Newly added line should be displayed along with the exisitng lines.", 2, stocktake.StocktakeLinesForFilter.Count);

				Factory.Save();
				AssertEquals("After saving StocktakeLinesForFilter should keep newly added line and the filtered line.", 2, stocktake.StocktakeLinesForFilter.Count);
			}
		}

		#endregion

		#region TestPerformSearch_ClearAdditionalFilter

		public void TestPerformSearch_ClearAdditionalFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, null, null, StocktakeStatus.Codes.Loaded);
			var line1 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0, StocktakeLineStatus.Codes.Open);

			Factory.Save();

			using (var form = new ZForm(stocktake))
			{
				var filter = new StocktakeLineFilterControlForTest(stocktake);
				form.Controls.Add(filter);
				form.Show();

				stocktake.StocktakeLinesForFilter.AdditionalFilter = ZQuery.NoResultQuery;
				AssertEquals(true, stocktake.StocktakeLinesForFilter.AdditionalFilter.IsNoResultQuery);

				filter.FirePerformSearch();
				AssertEquals(false, stocktake.StocktakeLinesForFilter.AdditionalFilter.IsNoResultQuery);
			}
		}

		#endregion

		#region TestPerformSearch_WithControlFocusOnFilter

		public void TestPerformSearch_WithControlFocusOnFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "Row", 2, 2);
			var testRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "TestRow", 2, 2);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);
			var openLineInRow1 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, row.Locations[0], StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);
			var closedLineInRow1 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, row.Locations[1], StocktakeLineStatus.Codes.Closed, InventoryStatus.Codes.Available);
			var openLineInTestRow = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, testRow.Locations[0], StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);
			var closedLineInTestRow = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, testRow.Locations[1], StocktakeLineStatus.Codes.Closed, InventoryStatus.Codes.Available);

			using (var form = new ZForm(stocktake))
			{
				var filter = new StocktakeLineFilterControlForTest(stocktake);
				form.Controls.Add(filter);
				form.Show();

				Factory.Save();
				var statusFilter = (ModuleTextFilter)filter.FilterBusinessObject[StocktakeLineFilterBusinessObject.FilterNames.Status];
				statusFilter.IsActive = true;
				statusFilter.Property = StocktakeLineStatus.Codes.Open;
				var isControlFocusChanged = false;

				var textControl = FindControl<ZTextBox>(filter);
				var gridControl = FindControl<ZGrid>(filter);

				AssertNotNull("Pre-condition", textControl);
				AssertNotNull("Pre-condition", gridControl);

				EventHandler focusChanged = (s, e) => { isControlFocusChanged = true; };
				gridControl.LostFocus += focusChanged;
				textControl.LostFocus += focusChanged;

				gridControl.Select();
				gridControl.Focus();
				filter.ToolStripFindDropButtonExposed.PerformClick();
				AssertEquals("Since the selected control is a grid, focus should not be changed during the search operation.", false, isControlFocusChanged);

				textControl.Select();
				textControl.Focus();
				filter.ToolStripFindDropButtonExposed.PerformClick();
				AssertEquals("Since the selected control is a text control, focus should be changed during the search operation.", true, isControlFocusChanged);
			}
		}

		Control FindControl<T>(Control container) where T : Control
		{
			if (container is T)
			{
				return container;
			}

			foreach (Control control in container.Controls)
			{
				var foundControl = FindControl<T>(control);
				if (foundControl != null)
				{
					return foundControl;
				}
			}
			return null;
		}

		#endregion

		#region TestPerformSearch_WithAllStatusFilter

		public void TestPerformSearch_WithAllStatusFilter()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			Factory.Save();

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			stocktake.Load(Notify);
			Factory.Save();
			AssertEquals("Precondition", 1, stocktake.StocktakeLinesForFilter.Count);

			using (var form = new ZForm(stocktake))
			{
				var filter = new StocktakeLineFilterControlForTest(stocktake);
				form.Controls.Add(filter);
				var statusFilter = (ModuleTextFilter)filter.FilterBusinessObject[StocktakeLineFilterBusinessObject.FilterNames.Status];
				statusFilter.IsActive = true;
				statusFilter.Property = StocktakeLineStatus.Codes.All;

				form.Show();

				filter.FirePerformSearch();
				AssertEquals("All lines should be returned when filtering for lines with ALL statuses.", 1, stocktake.StocktakeLinesForFilter.Count);
			}

			using (var form = new ZForm(stocktake))
			{
				var filter = new StocktakeLineFilterControlForTest(stocktake);
				form.Controls.Add(filter);
				var statusFilter = (ModuleTextFilter)filter.FilterBusinessObject[StocktakeLineFilterBusinessObject.FilterNames.Status];
				statusFilter.IsActive = true;
				statusFilter.Property = StocktakeLineStatus.Codes.All;
				new DataGridLayoutManager().SavePreconfiguredLayout(filter.FilterBusinessObject, "Status", false, false, SaveColumnLayout.Ignore);

				form.Show();

				filter.FirePerformSearch();
				AssertEquals("All lines should be returned when filtering for lines with ALL statuses.", 1, stocktake.StocktakeLinesForFilter.Count);
			}
		}

		#endregion

		#endregion

		#region TestLocationAutoComplete

		public void TestLocationAutoCompleteDisabled()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);

			using (var form = new ZForm(stocktake))
			{
				var filter = new StocktakeLineFilterControlForTest(stocktake);
				form.Controls.Add(filter);
				form.Show();

				var locationStringFindBoxStyle = (ZCodeFindBoxColumnStyleInfo)filter.Grid.GetColumnStyle("LocationString");
				Assert("Location style should have auto complete disabled", locationStringFindBoxStyle.AutoCompleteDisabled);
			}
		}

		public void TestNonLocationAutoCompleteEnabled()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);

			using (var form = new ZForm(stocktake))
			{
				var filter = new StocktakeLineFilterControlForTest(stocktake);
				form.Controls.Add(filter);
				form.Show();

				var count2VerifiedByStyle = (ZCodeFindBoxColumnStyleInfo)filter.Grid.GetColumnStyle("WU_Count2VerifiedBy");
				Assert("Count2 verified by style should have auto complete enabled", !count2VerifiedByStyle.AutoCompleteDisabled);
			}
		}

		#endregion

		#region Implementation

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		TestNotificationBuffer Notify => notify ?? (notify = new TestNotificationBuffer());
		TestNotificationBuffer notify;

		#endregion

		class StocktakeLineFilterControlForTest : StocktakeLineFilterControl
		{
			public StocktakeLineFilterControlForTest(WhsStocktake stocktake)
				: base(stocktake)
			{
			}

			public new int MaximumAllowableQueriesPerSqlStatement => base.MaximumAllowableQueriesPerSqlStatement;

			internal bool ShouldPerformSearchExposed => ShouldPerformSearch();

			public void HandleFindButtonDropDownItemClickExposed(ToolStripItem item)
			{
				HandleFindButtonDropDownItemClick(item);
			}

			public ToolStripSplitButton ToolStripFindDropButtonExposed => ToolStripFindDropButton;
		}
	}
}
