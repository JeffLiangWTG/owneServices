using System.Linq;
using System.Web.UI.WebControls;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class InventoryTest : WarehousingPageBaseTest
	{
		[ExpectNoExceptions]
		public void TestDetailedExportToExcelHasNoExceptionWhenColumnsAreSorted()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var helper = new WhsTestHelperFunctions(Factory);

			var receive1 = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, data.Whs1.FindLocation("A-1"), "");
			AssertNotNull(receive1.Inventory[0]);

			var receive2 = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 200m, data.Whs1.FindLocation("A-1"), "");
			AssertNotNull(receive2.Inventory[0]);

			var contact = data.Org1.Contacts.AddNew();
			contact.OC_ContactName = "Test Contact";
			contact.OC_Email = "test@test.com";
			contact.SetHashedPassword("abrakadabra");
			contact.OC_WebAccessEnabled = true;

			Factory.Save();

			using (var inventoryPage = new InventoryForTest())
			{
				inventoryPage.SiteUser.Login(data.Org1.OH_Code, "test@test.com", "abrakadabra");
				inventoryPage.OnLoadForTest();

				var searchControl = (ZSearchControl)inventoryPage.SearchControl;
				searchControl.SearchResultsDataGrid.Collapsed = false;
				searchControl.InitResultsGridForTesting();
				searchControl.RePopulateGrid();

				using (var module = ZWebModuleFactory.Create(WebModuleIDs.TrackingInventoryDetails, Factory, inventoryPage))
				{
					var exportCollection = (TrackingWhsInventoryCollection)module?.LoadExcelCollection(searchControl.FilterBusinessObject);
					AssertNotNull(exportCollection);
					AssertNotEquals(0, exportCollection.Count);
				}

				searchControl.SearchResultsDataGrid.AllowSorting = true;
				var columns = ((ZFilterStripGridModule)searchControl.Module).ColumnProvider.AllColumns;
				foreach (var column in columns)
				{
					searchControl.SetSortExpressionForTesting(column.SortExpression);
					inventoryPage.DetailedExportButtonClickForTesting();
				}
			}
		}

		public void TestQuantityIsResetToZeroAfterAddToOrderButtonClick()
		{
			using (var inventoryPage = new InventoryForTest())
			{
				inventoryPage.OnLoadForTest();

				var searchControl = (ZSearchControl)inventoryPage.SearchControl;
				searchControl.SearchResultsDataGrid.Collapsed = false;
				searchControl.InitResultsGridForTesting();
				searchControl.RePopulateGrid();

				var summaries = (TrackingInventorySummaryCollection)searchControl.Module.GridCollection;
				var summary = TrackingInventorySummaryTest.GetSummary(Factory);
				summaries.Add(summary);

				var inventory1 = Factory.NewWithValidTestData<TrackingWhsInventory>();
				inventory1.WI_TotalUnits = 11;
				inventory1.Quantity = 10;

				var inventory2 = Factory.NewWithValidTestData<TrackingWhsInventory>();
				inventory2.Docket.WD_WW_Whs = inventory1.WI_WW_Whs;
				inventory2.WI_TotalUnits = 21;
				inventory2.Quantity = 20;

				summary.LoadInventories();
				summary.Inventories.Add(inventory1);
				summary.Inventories.Add(inventory2);

				searchControl.RePopulateGrid();
				AssertEquals(1, searchControl.SearchResultsDataGrid.Items.Count);

				var columns = searchControl.SearchResultsDataGrid.Items[0].Controls;
				var inventoryGrid = columns[columns.Count - 1].Controls[1] as ZDataGrid;
				AssertNotNull(inventoryGrid);

				AssertEquals(2, inventoryGrid.Items.Count);
				var inventoryAllocateControl = inventoryGrid.Items[0].Controls.OfType<TableCell>().SelectMany(t => t.Controls.OfType<ZNumericTextBox>()).SingleOrDefault(c => c.BindTo == "Quantity");
				AssertNotNull(inventoryAllocateControl);
				AssertEquals("10", inventoryAllocateControl.Text);

				var inventory2AllocateControl = inventoryGrid.Items[1].Controls.OfType<TableCell>().SelectMany(t => t.Controls.OfType<ZNumericTextBox>()).SingleOrDefault(c => c.BindTo == "Quantity");
				AssertNotNull(inventory2AllocateControl);
				AssertEquals("20", inventory2AllocateControl.Text);

				inventoryPage.CustomNewButtonClickForTesting();

				AssertEquals(1, searchControl.SearchResultsDataGrid.Items.Count);

				columns = searchControl.SearchResultsDataGrid.Items[0].Controls;
				inventoryGrid = columns[columns.Count - 1].Controls[1] as ZDataGrid;
				AssertNotNull(inventoryGrid);

				AssertEquals(2, inventoryGrid.Items.Count);
				inventoryAllocateControl = inventoryGrid.Items[0].Controls.OfType<TableCell>().SelectMany(t => t.Controls.OfType<ZNumericTextBox>()).SingleOrDefault(c => c.BindTo == "Quantity");
				AssertNotNull(inventoryAllocateControl);
				AssertEquals(0m, inventory1.Quantity);
				AssertEquals("", inventoryAllocateControl.Text);

				inventory2AllocateControl = inventoryGrid.Items[1].Controls.OfType<TableCell>().SelectMany(t => t.Controls.OfType<ZNumericTextBox>()).SingleOrDefault(c => c.BindTo == "Quantity");
				AssertNotNull(inventory2AllocateControl);
				AssertEquals(0m, inventory2.Quantity);
				AssertEquals("", inventory2AllocateControl.Text);
			}
		}

		public void TestShoppingCartLinksRemovedWhenClearAllocatedLinesButtonClicked()
		{
			using (var inventoryPage = new InventoryForTest())
			{
				inventoryPage.OnLoadForTest();

				var searchControl = (ZSearchControl)inventoryPage.SearchControl;
				searchControl.SearchResultsDataGrid.Collapsed = false;
				searchControl.InitResultsGridForTesting();
				searchControl.RePopulateGrid();
				inventoryPage.SetupBusinessObjectForValidationForTest();

				var summaries = (TrackingInventorySummaryCollection)searchControl.Module.GridCollection;
				var summary = TrackingInventorySummaryTest.GetSummary(Factory);
				summaries.Add(summary);

				var inventory1 = Factory.NewWithValidTestData<TrackingWhsInventory>();
				inventory1.WI_TotalUnits = 11;
				inventory1.Quantity = 10;

				var inventory2 = Factory.NewWithValidTestData<TrackingWhsInventory>();
				inventory2.Docket.WD_WW_Whs = inventory1.WI_WW_Whs;
				inventory2.WI_TotalUnits = 21;
				inventory2.Quantity = 20;

				var inventory3 = Factory.NewWithValidTestData<TrackingWhsInventory>();
				inventory1.WI_TotalUnits = 195;
				inventory1.Quantity = 198;

				summary.LoadInventories();
				summary.Inventories.Add(inventory1);
				summary.Inventories.Add(inventory2);
				summary.Inventories.Add(inventory3);

				inventoryPage.ShoppingCartHelper.AddOrderLine(inventory1);
				inventoryPage.ShoppingCartHelper.AddOrderLine(inventory2);

				AssertNotNull(inventory1.ShoppingCart);
				AssertNotNull(inventory2.ShoppingCart);
				AssertNull(inventory3.ShoppingCart);

				inventoryPage.ShoppingCartControl_ClearButtonClickedForTesting();

				AssertNull(inventory1.ShoppingCart);
				AssertNull(inventory2.ShoppingCart);
				AssertNull(inventory3.ShoppingCart);
			}
		}

		#region Overrides

		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.WarehouseInventory;
		}

		protected override System.Web.UI.Control GetNewControl()
		{
			return new InventoryForTest();
		}

		protected override BooleanRegistryItem UseWebModule
		{
			get { return WebDataRegistry.Instance.UseWebWarehouseInventoryModule; }
		}

		protected override WebSecurityRight SiteUserSecurityRight
		{
			get { return WebSecurityRightsList.WebInventoryView; }
		}

		#endregion

		#region TestSearchControlAsyncPostBack

		public void TestSearchControlAsyncPostBack()
		{
			using (var inventoryPage = new InventoryForTest())
			{
				inventoryPage.ShoppingCartHelper.ShoppingCart.Lines.AddNew();
				inventoryPage.OnLoadForTest();

				Assert("Search Control Find should cause async postback", ((ZSearchControl)inventoryPage.SearchControl).ShouldFindCauseAsyncPostBack);
			}
		}

		#endregion

		#region TestDetailedExcelExportCollectionType

		public void TestDetailedExcelExportCollectionType()
		{
			using (ZFilterGridModule module = ZWebModuleFactory.Create(WebModuleIDs.TrackingInventoryDetails, Factory, TestPage))
			{
				Assert(module.GridCollectionType.IsAssignableFrom(typeof(TrackingWhsInventoryCollection)));
			}
		}

		#endregion

		#region TestOrderLinesGridBindTo_TrackingWhsOrderLineCollection

		public void TestOrderLinesGridBindTo_TrackingWhsOrderLineCollection()
		{
			using (var inventoryPage = new InventoryForTest())
			{
				inventoryPage.ShoppingCartHelper.ShoppingCart.Lines.AddNew();
				inventoryPage.OnLoadForTest();

				AssertType<TrackingWhsOrderLineCollection>("OrderLinesGrid should bind to type TrackingWhsOrderLineCollection.", inventoryPage.ShoppingCartControlExposed.OrderLinesGrid.DataSource);
			}
		}

		#endregion
	}
}
