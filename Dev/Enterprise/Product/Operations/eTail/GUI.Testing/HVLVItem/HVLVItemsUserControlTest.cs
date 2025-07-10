using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.eTail.Business;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.eTail.GUI.Testing
{
	class HVLVItemsUserControlTest : TestCaseWithFactory
	{
		public void TestCharacterCasing_ForItemGridDropEditColumnsAndUnitColumns_ShouldAlwaysBeUpper()
		{
			using (var form = new ConsignmentUserControlTestForm())
			{
				form.Show();

				var itemsGrid = form.ItemsGrid;
				var itemsGridColumnStyles = itemsGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				var columnNames = new string[] { "HVI_UnitOfDimension", "HVI_F3_NKPackType", "HVI_ContainerNumber", "Consignment+HVC_WeightUQ", "Consignment+HVC_WeightUQ" };

				CombineAssertions("Item grid columns should have correct casing", () =>
				{
					foreach (var columnName in columnNames)
					{
						var columnCasing = itemsGridColumnStyles.Single(columnStyle => columnStyle.ColumnName == columnName).CharacterCasing;
						AssertEquals($"${columnName} should always be Upper Case", CharacterCasing.Upper, columnCasing);
					}
				});
			}
		}

		public void TestItemDeleteMenuItem_WhenItemIsSaved_IsDisabled()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			var item = consignment.Items.AddNew();

			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var grid = form.ItemsGrid;
				grid.SelectSingleElement(item);

				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				AssertEquals(false, grid.DeleteMenuItem.Enabled);
			}
		}

		public void TestItemDeleteMenuItem_WhenConsignmentIsNotYetSaved_IsActive()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.ConsignmentsFilteredView.AddNew();

			Factory.Save();

			var unsavedItem = consignment.Items.AddNew();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var grid = form.ItemsGrid;

				grid.SelectSingleElement(unsavedItem);
				var contextMenu = grid.ContextMenu;
				contextMenu.ShowPopupMenu();

				AssertEquals("In DB", false, unsavedItem.IsInDatabase);
				AssertEquals(true, grid.DeleteMenuItem.Enabled);
			}
		}

		public void TestItemActiveStatusMenuItem_WhenSelectedItemsHaveAnySavedRecords_EnableMenuItem()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			var item1 = consignment.Items.AddNew();
			Factory.Save();
			var item2 = consignment.Items.AddNew();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var consignmentGrid = form.ConsignmentsGrid;
				consignmentGrid.SelectSingleElement(consignment);

				var itemGrid = form.ItemsGrid;
				itemGrid.SelectAllElements();

				CombineAssertions("pre condition", () =>
				{
					AssertEquals("First item is saved", true, itemGrid.SelectedElements[0].IsInDatabase);
					AssertEquals("Second item is not saved", false, itemGrid.SelectedElements[1].IsInDatabase);
				});

				var contextMenu = itemGrid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.Deactivate);
				AssertEquals("Menu item should be enabled", true, menuItem.Enabled);
			}
		}

		public void TestItemActiveStatusMenuItem_WhenSelectedItemsAreAllNotSaved_DisableMenuItem()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var consignmentGrid = form.ConsignmentsGrid;
				consignmentGrid.SelectSingleElement(consignment);

				var itemGrid = form.ItemsGrid;
				itemGrid.SelectAllElements();

				CombineAssertions("pre condition", () =>
				{
					AssertEquals("First item is not saved", false, itemGrid.SelectedElements[0].IsInDatabase);
					AssertEquals("Second item is not saved", false, itemGrid.SelectedElements[1].IsInDatabase);
				});

				var contextMenu = itemGrid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.Deactivate);
				AssertEquals("Menu item should be disabled", false, menuItem.Enabled);
			}
		}

		public void TestItemActiveStatusMenuItemCaption_WhenSelectedItemsAreAllInactive_ShowsActivateCaption()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();

			consignment.HVC_IsActive = true;
			item1.HVI_IsActive = false;
			item2.HVI_IsActive = false;

			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var consignmentGrid = form.ConsignmentsGrid;
				consignmentGrid.SelectSingleElement(consignment);

				var itemGrid = form.ItemsGrid;
				itemGrid.SelectAllElements();

				var contextMenu = itemGrid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var menuItemCaptions = contextMenu.MenuItems.Cast<MenuItem>().Select(x => x.Text).ToList();
				CombineAssertions("Menu Item Captions should contain correct option for activating items", () =>
				{
					AssertEquals("Activate Caption: Shown", true, menuItemCaptions.Contains(HVLVMenuItemHelper.Captions.Activate));
					AssertEquals("Deactivate Caption: Hidden", false, menuItemCaptions.Contains(HVLVMenuItemHelper.Captions.Deactivate));
					AssertEquals("Toggle Caption: Hidden", false, menuItemCaptions.Contains(HVLVMenuItemHelper.Captions.ToggleActiveStatus));
				});
			}
		}

		public void TestItemActiveStatusMenuItemCaption_WhenSelectedItemsAreMixOfActiveAndInactive_ShowsToggleCaption()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();

			consignment.HVC_IsActive = true;
			item1.HVI_IsActive = true;
			item2.HVI_IsActive = false;

			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var consignmentGrid = form.ConsignmentsGrid;
				consignmentGrid.SelectSingleElement(consignment);

				var itemGrid = form.ItemsGrid;
				itemGrid.SelectAllElements();

				var contextMenu = itemGrid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var menuItemCaptions = contextMenu.MenuItems.Cast<MenuItem>().Select(x => x.Text).ToList();
				CombineAssertions("Menu Item Captions should contain correct option for toggling item active status", () =>
				{
					AssertEquals("Toggle Caption: Shown", true, menuItemCaptions.Contains(HVLVMenuItemHelper.Captions.ToggleActiveStatus));
					AssertEquals("Activate Caption: Hidden", false, menuItemCaptions.Contains(HVLVMenuItemHelper.Captions.Activate));
					AssertEquals("Deactivate Caption: Hidden", false, menuItemCaptions.Contains(HVLVMenuItemHelper.Captions.Deactivate));
				});
			}
		}

		public void TestItemActiveStatusMenuItemClick_WhenSelectedItemsAreMixOfSavedAndNotSaved_NotSavedRecordsAreSkipped()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			var item1 = consignment.Items.AddNew();
			Factory.Save();
			var item2 = consignment.Items.AddNew();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var consignmentGrid = form.ConsignmentsGrid;
				consignmentGrid.SelectSingleElement(consignment);

				var itemGrid = form.ItemsGrid;
				itemGrid.SelectAllElements();

				CombineAssertions("pre condition", () =>
				{
					AssertEquals("First item is saved", true, itemGrid.SelectedElements[0].IsInDatabase);
					AssertEquals("Second item is not saved", false, itemGrid.SelectedElements[1].IsInDatabase);
				});

				var contextMenu = itemGrid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.Deactivate);
				menuItem.PerformClick();

				CombineAssertions("Menu action should only be applied to saved item, not saved item is skipped", () =>
				{
					AssertEquals("First item is deactivated", false, item1.HVI_IsActive);
					AssertEquals("Second item is skipped", true, item2.HVI_IsActive);
				});
			}
		}

		public void TestItemActiveStatusMenuItemClick_WhenSelectedConsignmentsAreAllActive_Deactivates()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();

			consignment.HVC_IsActive = true;
			item1.HVI_IsActive = true;
			item2.HVI_IsActive = true;

			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var consignmentGrid = form.ConsignmentsGrid;
				consignmentGrid.SelectSingleElement(consignment);

				var itemGrid = form.ItemsGrid;
				itemGrid.SelectAllElements();

				var contextMenu = itemGrid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.Deactivate);
				menuItem.PerformClick();

				CombineAssertions("Items should be deactivated", () =>
				{
					AssertEquals("Item 1", false, item1.HVI_IsActive);
					AssertEquals("Item 2", false, item2.HVI_IsActive);
				});
			}
		}

		public void TestItemActiveStatusMenuItemClick_WhenSelectedItemsAreAllInactive_Activates()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();

			consignment.HVC_IsActive = true;
			item1.HVI_IsActive = false;
			item2.HVI_IsActive = false;

			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var consignmentGrid = form.ConsignmentsGrid;
				consignmentGrid.SelectSingleElement(consignment);

				var itemGrid = form.ItemsGrid;
				itemGrid.SelectAllElements();

				var contextMenu = itemGrid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.Activate);
				menuItem.PerformClick();

				CombineAssertions("Items should be activated", () =>
				{
					AssertEquals("Item 1", true, item1.HVI_IsActive);
					AssertEquals("Item 2", true, item2.HVI_IsActive);
				});
			}
		}

		public void TestItemActiveStatusMenuItemClick_WhenSelectedItemsAreMixOfActiveAndInactive_TogglesActiveStatus()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();

			consignment.HVC_IsActive = true;
			item1.HVI_IsActive = false;
			item2.HVI_IsActive = true;

			Factory.Save();

			using (var form = new ConsignmentUserControlTestForm(bookingHeader))
			{
				form.Show();

				var consignmentGrid = form.ConsignmentsGrid;
				consignmentGrid.SelectSingleElement(consignment);

				var itemGrid = form.ItemsGrid;
				itemGrid.SelectAllElements();

				var contextMenu = itemGrid.ContextMenu;
				contextMenu.ShowPopupMenu();

				var menuItem = contextMenu.MenuItems.Cast<MenuItem>().Single(x => x.Text == HVLVMenuItemHelper.Captions.ToggleActiveStatus);
				menuItem.PerformClick();

				CombineAssertions("Items' Active Status should have been toggled", () =>
				{
					AssertEquals("Item 1, Inactive -> Active", true, item1.HVI_IsActive);
					AssertEquals("Item 2, Active -> Inactive", false, item2.HVI_IsActive);
				});
			}
		}

		public void TestDGColumnsArePresent()
		{
			using (var form = new ConsignmentUserControlTestForm())
			{
				var itemsGridColumnStyles = form.ItemsGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				var dgSubsColumn = itemsGridColumnStyles.SingleOrDefault(columnStyle => columnStyle.ColumnName.Contains("UNDGSubstanceManager"));
				var classManagerColumn = itemsGridColumnStyles.SingleOrDefault(columnStyle => columnStyle.ColumnName.Contains("UNDGClassManager"));
				var flashpointColumn = itemsGridColumnStyles.SingleOrDefault(columnStyle => columnStyle.ColumnName.Contains("UNDGFlashPointManager"));
				var contactColumn = itemsGridColumnStyles.SingleOrDefault(columnStyle => columnStyle.ColumnName.Contains("UNDGContactManager"));
				var pollutantColumn = itemsGridColumnStyles.SingleOrDefault(columnStyle => columnStyle.ColumnName.Contains("UNDGMarinePollutantManager"));
				var technicalColumn = itemsGridColumnStyles.SingleOrDefault(columnStyle => columnStyle.ColumnName.Contains("UNDGTechnicalNameManager"));
				var properShippingNameColumn = itemsGridColumnStyles.SingleOrDefault(columnStyle => columnStyle.ColumnName.Contains("UNDGProperShippingNameManager"));
				var weightColumn = itemsGridColumnStyles.SingleOrDefault(columnStyle => columnStyle.ColumnName.Contains("UNDGWeightManager"));
				var dgUWColumn = itemsGridColumnStyles.SingleOrDefault(columnStyle => columnStyle.ColumnName.Contains("UNDGWeightUnitManager"));

				var volumeColumn = itemsGridColumnStyles.SingleOrDefault(columnStyle => columnStyle.ColumnName.Contains("UNDGVolumeManager"));
				var dgUVColumn = itemsGridColumnStyles.SingleOrDefault(columnStyle => columnStyle.ColumnName.Contains("UNDGVolumeUnitManager"));
				var hasOverpackColumn = itemsGridColumnStyles.SingleOrDefault(columnStyle => columnStyle.ColumnName.Contains("UNDGHasOverpackManager"));
				var overpackIdColumn = itemsGridColumnStyles.SingleOrDefault(columnStyle => columnStyle.ColumnName.Contains("UNDGOverpackIDManager"));
				var quantityColumn = itemsGridColumnStyles.SingleOrDefault(columnStyle => columnStyle.ColumnName.Contains("UNDGSubstanceExceptedQuantityManager"));
				var psaGroupColumn = itemsGridColumnStyles.SingleOrDefault(columnStyle => columnStyle.ColumnName.Contains("UNDGPSAGroupsManager"));

				CombineAssertions(() =>
				{
					var shown = "column should be shown";
					var notShown = "column should not be shown";

					AssertNotNull($"DG Subs {shown}", dgSubsColumn);
					AssertNotNull($"DG Class {shown}", classManagerColumn);
					AssertNotNull($"Flash Point {shown}", flashpointColumn);
					AssertNotNull($"DG Contact {shown}", contactColumn);
					AssertNotNull($"Marine Pollutant {shown}", pollutantColumn);
					AssertNotNull($"Technical Name {shown}", technicalColumn);
					AssertNotNull($"DG Weight name {shown}", weightColumn);
					AssertNotNull($"DG UW name {shown}", dgUWColumn);
					AssertNotNull($"DG Volume {shown}", volumeColumn);
					AssertNotNull($"DG UV {shown}", dgUVColumn);
					AssertNotNull($"Has Overpack {shown}", hasOverpackColumn);
					AssertNotNull($"Overpack ID {shown}", overpackIdColumn);

					AssertNull($"Excepted Quantity {notShown}", quantityColumn);
					AssertNull($"Proper Shipping name {notShown}", properShippingNameColumn);
					AssertNull($"PSA Group {notShown}", psaGroupColumn);
				});
			}
		}

		public void TestDensityFactorColumnsArePresent()
		{
			using (var form = new ConsignmentUserControlTestForm())
			{
				var itemsGridColumnStyles = form.ItemsGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				var volumeWeightColumn = itemsGridColumnStyles.SingleOrDefault(columnStyle => columnStyle.ColumnName.Contains("VolumeWeight"));
				var chargeableWeightColumn = itemsGridColumnStyles.SingleOrDefault(columnStyle => columnStyle.ColumnName.Contains("ChargeableForDisplay"));
				var densityFactorColumn = itemsGridColumnStyles.SingleOrDefault(columnStyle => columnStyle.ColumnName.Contains("DensityFactor"));

				CombineAssertions(() =>
				{
					AssertNotNull("Volume Weight column should be shown", volumeWeightColumn);
					AssertNotNull("Chargeable Weight column should be shown", chargeableWeightColumn);
					AssertNotNull("Density Factory column should be shown", densityFactorColumn);
				});
			}
		}
	}
}
