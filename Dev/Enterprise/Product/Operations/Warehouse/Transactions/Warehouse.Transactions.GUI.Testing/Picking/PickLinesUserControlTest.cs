using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class PickLinesUserControlTest : WhsGuiTestCaseWithFactory
	{
		#region Constructor

		public void TestConstructor()
		{
			using (var userControl = new PickLinesUserControl())
			{
				AssertEquals(RemoveAction.NoRemovePossible, userControl.ItemsGrid.RemoveAction);
				AssertEquals(RemoveAction.NoRemovePossible, userControl.InventoryGridForTest.RemoveAction);
			}
		}

		#endregion

		#region ZUserControl Overloads

		public void TestBind()
		{
			SetupBasicData();
			Helper.CreateWhsOrderLine(Order, InvData.Part1, 100m, "BEK1", "DummyOutward-1", "");

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(Order);
			AssertEquals("Precondition: Should be 2 items", 2, pick.OrderedInventories.Count);

			// need to swap items around because adding the order would of sorted them
			if (pick.OrderedInventories[1].BondedEntryKey.IsEmpty)
			{
				pick.OrderedInventories.Sort("BondedEntryKey", ListSortDirection.Ascending);
			}

			AssertEquals("Precondition: OrderedInventories are not sorted.", "", pick.OrderedInventories[0].BondedEntryKey);
			AssertEquals("Precondition: OrderedInventories are not sorted.", "BEK1", pick.OrderedInventories[1].BondedEntryKey);

			using (var form = new TestForm(pick))
			{
				form.Show();
				AssertEquals("Items have not been sorted by Bind()", "BEK1", pick.OrderedInventories[0].BondedEntryKey);
			}
		}

		public void TestColumnControl()
		{
			Assert("incomplete test", true);
			//WhsWarehouse Whs = Helper.CreateWarehouse("1");
			//OrgHeader Org1 = Helper.CreateClient("1");
			//OrgHeader Org2 = Helper.CreateClient("2");
			//OrgSupplierPart Prod1 = Helper.CreateProduct(Org1, "P1");
			//OrgSupplierPart Prod2 = Helper.CreateProduct(Org2, "P2");
			//WhsOrder Order1 = Helper.CreateWhsOrder(Org1, Whs, "1");
			//WhsOrder Order2 = Helper.CreateWhsOrder(Org2, Whs, "2");
			//Helper.CreateWhsOrderLine(Order1, Prod1, 10m);
			//Helper.CreateWhsOrderLine(Order2, Prod2, 10m);

			//Org1.MiscServ.OM_IMPartAttrib1Name = "Batch#";
			//Org1.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.BatchNumber;
			//Org2.MiscServ.OM_IMPartAttrib1Name = "Lot#";
			//Org2.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.BatchNumber;
			//Org2.MiscServ.OM_IMPartAttrib3Name = "Serial#";
			//Org2.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.SerialNumber;
			//Org2.MiscServ.OM_IMUsePackingDate = true;

			//Pick = Factory.New<WhsPick>();
			//Pick.Orders.Add(Order1);
			//Pick.Orders.Add(Order2);
			//Pick.OrderedInventories.Sort("ProductCode", ListSortDirection.Ascending);

			//using (TestForm Form = new TestForm(Pick))
			//{
			//    Form.Show();
			//    ZGrid ItemsGrid = Form.UserControl.ItemsGrid;
			//    ZGrid InventoryGrid = Form.UserControl.InventoryGrid;

			//    AssertAttributeVisibility(ItemsGrid, false, false, true, false, false, false, false, false);
			//    AssertAttributeVisibility(InventoryGrid, false, false, true, false, false, false, false, false);
			//    AssertAttributeTitles(ItemsGrid, "Batch#", "", "");
			//    AssertAttributeTitles(InventoryGrid, "Batch#", "", "");

			//    ItemsGrid.Select(1);
			//    AssertAttributeVisibility(ItemsGrid, false, true, true, false, true, false, false, false);
			//    AssertAttributeVisibility(InventoryGrid, false, false, true, false, false, false, false, false);
			//    AssertAttributeTitles(ItemsGrid, "Batch# / Lot#", "", "Serial#");
			//    AssertAttributeTitles(InventoryGrid, "Batch#", "", "");

			//    ItemsGrid.Select(0);
			//    AssertAttributeVisibility(InventoryGrid, false, false, true, false, false, false, false, false);
			//    AssertAttributeTitles(InventoryGrid, "Batch#", "", "");

			//    ItemsGrid.Select(1);
			//    AssertAttributeVisibility(InventoryGrid, false, true, true, false, true, false, false, false);
			//    AssertAttributeTitles(InventoryGrid, "Lot#", "", "Serial#");
			//}
		}

		#endregion

		#region Properties

		#region TestSerialNumberColumnInitialised

		public void TestSerialNumberColumnInitialised_OrderedInventoryGrid()
		{
			var pick = Factory.New<WhsPick>();
			using (var form = new TestForm(pick))
			{
				form.Show();

				AssertEquals(false,
					form.UserControl.ItemsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(c => c.ColumnName == nameof(WhsPickOrderedInventory.SerialNumber)).IsUnavailable);
			}
		}

		public void TestSerialNumberColumnInitialised_AvailableInventoryGrid()
		{
			var pick = Factory.New<WhsPick>();
			using (var form = new TestForm(pick))
			{
				form.Show();

				AssertEquals(false,
					form.UserControl.InventoryGridForTest.ColumnStyles.Cast<ZGridColumnInfo>().Single(c => c.ColumnName == nameof(WhsPickAvailableInventory.SerialNumber)).IsUnavailable);
			}
		}

		#endregion

		#region TestPalletIDColumn

		public void TestOrderedPalletIDColumnAvailability()
		{
			var pick = Factory.New<WhsPick>();
			using (var form = new TestForm(pick))
			{
				form.Show();

				AssertEquals(false,
					form.UserControl.ItemsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(c => c.ColumnName == nameof(WhsPickOrderedInventory.PalletIDOrdered)).IsUnavailable);
			}
		}

		#endregion

		#region TestCustomAttributesVisibility

		public void TestCustomAttributesVisibility_CustomAttrib1_RegistryOn()
		{
			TestCustomAttributesVisibilityCore("CustomAttrib1", registryOn: true);
		}

		public void TestCustomAttributesVisibility_CustomAttrib1_RegistryOff()
		{
			TestCustomAttributesVisibilityCore("CustomAttrib1", registryOn: false);
		}

		public void TestCustomAttributesVisibility_CustomFlag1_RegistryOn()
		{
			TestCustomAttributesVisibilityCore("CustomFlag1", registryOn: true);
		}

		public void TestCustomAttributesVisibility_CustomFlag1_RegistryOff()
		{
			TestCustomAttributesVisibilityCore("CustomFlag1", registryOn: false);
		}

		void TestCustomAttributesVisibilityCore(string columnName, bool registryOn)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RN_NKCountryCode = "FR";

			var newCustomLabel = data.Org1.CustomFormLabels.AddNew();
			newCustomLabel.OT_FieldName = $"WhsDocketLine.{columnName}";
			newCustomLabel.OT_Caption = "TESTING";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 3);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5);
			var pick = Helper.CreatePickNew(order1, order2);

			using (WarehouseDataRegistry.Instance.GroupOrderedInventoryByCustomAttributes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryOn))
			using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
			{
				form.ShowPickSlipTab();
				Application.DoEvents();
				form.Show();
				var pickLinesUserControl = (PickLinesUserControl)form.PickSlipTabPageForTest.Find(c => c.Name == "PickLinesUserControl").First(); //
				var grid = (ZGrid)pickLinesUserControl.Find(c => c.Name == "ItemsGrid").First();

				var column = grid.Columns[columnName, false];
				if (registryOn)
				{
					Assert($"Column {columnName} should exist", column != null);
					AssertEquals($"Column {columnName} should be visible", true, column.IsVisible);
					AssertEquals($"Column {columnName} should be available", false, column.IsUnavailable);
				}
				else
				{
					AssertNull(column);
				}
			}
		}

		#endregion

		#region TestPick

		public void TestPick()
		{
			WhsPick pick = Factory.New<WhsPick>();
			using (TestForm form = new TestForm(pick))
			{
				form.Show();
				AssertEquals(pick, form.UserControl.PickForTest);
			}
		}

		#endregion

		#region TestCheckIfWeCanPerformActionOnPickAndShowErrorIfNot

		public void TestCheckIfWeCanPerformActionOnPickAndShowErrorIfNot()
		{
			SetupBasicData();
			Pick = Factory.New<WhsPick>();
			Pick.Orders.Add(Order);

			using (TestForm form = new TestForm(Pick))
			{
				form.Show();
				PickLinesUserControl userControl1 = form.UserControl;

				Pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				userControl1.ClearAllStockAllocationsButton_Click(null, EventArgs.Empty);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(PickLinesUserControl.NoChangesCanBeMadeToThisPickBecauseItIsEitherReadyForPlanningOrPlannedMsg));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);

				Pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				userControl1.ClearAllStockAllocationsButton_Click(null, EventArgs.Empty);
				AssertNullOrEmptyOrWhitespace(UnitTestUserNotification.Instance.LastMessage.Text);

				Pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				userControl1.ClearAllStockAllocationsButton_Click(null, EventArgs.Empty);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(PickLinesUserControl.NoChangesCanBeMadeToThisPickBecauseItIsEitherReadyForPlanningOrPlannedMsg));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);

				Pick.WP_TaskPlanningStatus = string.Empty;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				userControl1.ClearAllStockAllocationsButton_Click(null, EventArgs.Empty);
				AssertNullOrEmptyOrWhitespace(UnitTestUserNotification.Instance.LastMessage.Text);

				Pick.WP_PickStatus = CodeLists.PickStatus.Codes.PickSlip;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				userControl1.ClearAllStockAllocationsButton_Click(null, EventArgs.Empty);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(PickLinesUserControl.NoChangesCanBeMadeToThisPickBecausePickSlipHasAlreadyBeenPrintedMsg));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasWarning);

				Pick.WP_PickStatus = CodeLists.PickStatus.Codes.Created;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				userControl1.ClearAllStockAllocationsButton_Click(null, EventArgs.Empty);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);

				Pick.WP_PickStatus = CodeLists.PickStatus.Codes.PickSlip;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				userControl1.AutoAllocateStock_Click(null, EventArgs.Empty);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(PickLinesUserControl.NoChangesCanBeMadeToThisPickBecausePickSlipHasAlreadyBeenPrintedMsg));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasWarning);

				Pick.WP_PickStatus = CodeLists.PickStatus.Codes.Finalised;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				userControl1.ClearAllStockAllocationsButton_Click(null, EventArgs.Empty);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(PickLinesUserControl.NoChangesCanBeMadeToThisPickBecauseItIsEitherFinalisedOrCancelledMsg));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);

				Pick.WP_PickStatus = CodeLists.PickStatus.Codes.Cancelled;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				userControl1.ClearAllStockAllocationsButton_Click(null, EventArgs.Empty);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(PickLinesUserControl.NoChangesCanBeMadeToThisPickBecauseItIsEitherFinalisedOrCancelledMsg));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);

				Pick.WP_PickStatus = CodeLists.PickStatus.Codes.PickSlip;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var cartonisationMutex = new ZGlobalMutex(MutexIDs.WhsPickAllocatingPackageLabels, Pick.PK.ToString());
				cartonisationMutex.Lock();

				using (cartonisationMutex)
				{
					AssertEquals("Precondition", true, Pick.IsCartonising);
					userControl1.ClearAllStockAllocationsButton_Click(null, EventArgs.Empty);
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(PickLinesUserControl.NoChangesCanBeMadeToThisPickBecauseItIsEitherCartonisedOrCartonisingMsg));
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				userControl1.ClearAllStockAllocationsButton_Click(null, EventArgs.Empty);
				AssertEquals("Should only show the warning for Pick Slip Printed", true,
						UnitTestUserNotification.Instance.LastMessage.Contains(PickLinesUserControl.NoChangesCanBeMadeToThisPickBecausePickSlipHasAlreadyBeenPrintedMsg));
				AssertEquals("Should only show the warning for Pick Slip Printed", true, UnitTestUserNotification.Instance.LastMessage.WasWarning);

				Pick.WP_IsCartonised = true;
				userControl1.ClearAllStockAllocationsButton_Click(null, EventArgs.Empty);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(PickLinesUserControl.NoChangesCanBeMadeToThisPickBecauseItIsEitherCartonisedOrCartonisingMsg));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);

				Pick.WP_IsCartonised = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				userControl1.ClearAllStockAllocationsButton_Click(null, EventArgs.Empty);
				AssertEquals("Should only show the warning for Pick Slip Printed", true,
						UnitTestUserNotification.Instance.LastMessage.Contains(PickLinesUserControl.NoChangesCanBeMadeToThisPickBecausePickSlipHasAlreadyBeenPrintedMsg));
				AssertEquals("Should only show the warning for Pick Slip Printed", true, UnitTestUserNotification.Instance.LastMessage.WasWarning);

				using (var userControl = new PickLinesUserControl())
				{
					userControl.AutoAllocateStock_Click(null, EventArgs.Empty);
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(PickLinesUserControl.PleaseOpenThePickSlipTabToPerformThisFunction));
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);

					userControl.ClearAllStockAllocationsButton_Click(null, EventArgs.Empty);
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(PickLinesUserControl.PleaseOpenThePickSlipTabToPerformThisFunction));
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				}
			}
		}

		#endregion

		#region TestAutoAllocateRemainingLines_NoStockAllocated

		public void TestAutoAllocateRemainingLines_NoStockAllocated()
		{
			var org = Helper.CreateClient("1", "1");
			var whs = Helper.CreateWarehouse("IL1", "A", 3, 2);
			var product = Helper.CreateProduct(org, "P1");

			Factory.Save();
			var order = Helper.CreateWhsOrder(org, whs, "1");
			Helper.CreateWhsOrderLine(order, product, 5m);

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);
			AssertEquals("Precondition: Should be one item", 1, pick.OrderedInventories.Count);
			var orderedInventory = pick.OrderedInventories[0];

			var allocationEngineMock = new Mock<IAllocationEngineManager>(MockBehavior.Strict);
			allocationEngineMock.
				Setup(ae =>
					ae.Allocate(
						It.Is<WhsPick>(p => p == pick),
						It.Is<INotifications>(n => n == pick.NotificationSubscriber),
						It.IsNotNull<IPickStrategy>(),
						It.Is<IEnumerable<WhsPickOrderedInventory>>(ordInv => pick.OrderedInventories.Cast<WhsPickOrderedInventory>().All(i => ordInv.Contains(i)))))
				.Returns(AllocationResult.NoStockAllocated)
				.Verifiable();

			using (ObjectFactory.Substitute(allocationEngineMock.Object))
			using (var form = new TestForm(pick))
			{
				form.Show();
				var userControl = form.UserControl;
				userControl.AutoAllocateStock_Click(null, EventArgs.Empty);
				AssertEquals("No units should be allocated.", 0m, orderedInventory.PickLineQuantity);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("No stock could be found for this pick either because there is no available stock in the warehouse or there was no stock which meets configured allocation rules."));
			}
		}

		#endregion

		#region TestAutoAllocateRemainingLines_NoRemainingStock

		public void TestAutoAllocateRemainingLines_NoRemainingStock()
		{
			var org = Helper.CreateClient("1", "1");
			var whs = Helper.CreateWarehouse("IL1", "A", 3, 2);
			var product = Helper.CreateProduct(org, "P1");

			var receive = Helper.CreateWhsReceive(org, whs, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, product, 5m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();

			Factory.Save();
			var order = Helper.CreateWhsOrder(org, whs, "1");
			Helper.CreateWhsOrderLine(order, product, 5m);

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);
			AssertEquals("Precondition: Should be one item", 1, pick.OrderedInventories.Count);
			var orderedInventory = pick.OrderedInventories[0];

			using (var form = new TestForm(pick))
			{
				form.Show();
				var userControl = form.UserControl;
				userControl.AutoAllocateStock_Click(null, EventArgs.Empty);
				UnitTestUserNotification.Instance.ClearMessages();

				AssertEquals("All units should be allocated.", 5m, orderedInventory.PickLineQuantity);

				userControl.AutoAllocateStock_Click(null, EventArgs.Empty);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("No new stock could be allocated for this pick as all stock has already been allocated."));
			}
		}

		#endregion

		#endregion

		#region Methods

		#region USBondedColumnsVisibility

		public void TestUSBondedColumnsVisibility()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var pick = Factory.NewWithValidTestData<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;

			using (var form = new TestForm(pick))
			{
				form.Show();

				AssertGridColumnAvailability(form.UserControl.ItemsGrid, "PackageGroupId", false);
				AssertGridColumnAvailability(form.UserControl.InventoryGridForTest, "PackageGroupId", false);
				AssertGridColumnAvailability(form.UserControl.InventoryGridForTest, "PerPackageQty", false);

				form.Close();
			}

			Helper.EnableWarehouseForBond(data.Whs1, true);
			using (var form = new TestForm(pick))
			{
				form.Show();

				AssertGridColumnAvailability(form.UserControl.ItemsGrid, "PackageGroupId", true);
				AssertGridColumnAvailability(form.UserControl.InventoryGridForTest, "PackageGroupId", true);
				AssertGridColumnAvailability(form.UserControl.InventoryGridForTest, "PerPackageQty", true);

				form.Close();
			}
		}

		void AssertGridColumnAvailability(ZGrid grid, string column, bool available)
		{
			AssertEquals(available, grid.Columns.Contains(column));
		}

		#endregion

		#endregion

		#region UOM view changes

		public void TestUOMStuffWhenPickByUOMEnabled()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			Assert("Precondition: Pick by UOM is enabled", pick.IsPickByUOMEnabled);

			using (var form = new TestForm(pick))
			{
				form.Show();

				Assert("UOM group box with a grid should be visible", form.UserControl.UOMGroupBoxForTest.Visible);
			}
		}

		public void TestUOMStuffWhenPickByUOMDisabled()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = false;

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			Assert("Precondition: Pick by UOM is disabled", !pick.IsPickByUOMEnabled);

			using (var form = new TestForm(pick))
			{
				form.Show();

				Assert("UOM group box with a grid should be invisible", !form.UserControl.UOMGroupBoxForTest.Visible);
			}
		}

		#endregion

		#region Pick By Picked Details view changes

		public void TestStuffPickByPickedDetailsWhenPickByUOMEnabled()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			Assert("Precondition: Pick by UOM is enabled", pick.IsPickByUOMEnabled);

			using (var form = new TestForm(pick))
			{
				form.Show();

				Assert("group box pick by picked details with a grid should be invisible", !form.UserControl.GroupBoxPickByPickedDetailsForTest.Visible);
			}
		}

		public void TestStuffPickByPickedDetailsWhenPickByUOMDisabled()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = false;

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			Assert("Precondition: Pick by UOM is disabled", !pick.IsPickByUOMEnabled);

			using (var form = new TestForm(pick))
			{
				form.Show();

				Assert("group box pick by picked details with a grid should be visible", form.UserControl.GroupBoxPickByPickedDetailsForTest.Visible);
			}
		}

		#endregion

		#region TestPickedDetailsShouldBeEnabledWhenOnlyHaveReservedLines

		public void TestPickedDetailsShouldBeEnabledWhenOnlyHaveReservedLines_Enable()
		{
			PickedDetailsShouldBeEnabledWhenOnlyHaveReservedLinesCore(true);
		}

		public void TestPickedDetailsShouldBeEnabledWhenOnlyHaveReservedLines_Disable()
		{
			PickedDetailsShouldBeEnabledWhenOnlyHaveReservedLinesCore(false);
		}

		void PickedDetailsShouldBeEnabledWhenOnlyHaveReservedLinesCore(bool isPickByUOM)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;
			var client = data.Org1;
			var part = data.Part1;
			whs.WW_IsPickByUOMEnabled = isPickByUOM;

			var receive = Helper.CreateWhsReceiveWithInventory(client, whs, "R1", part, 1000m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var orderLine = order.Lines[0];
			var pickLine = orderLine.ReserveStockIfAbleTo(receive.Inventory[0], 3m);
			AssertEquals("Precondition - WZ_F3_NKAllocatedPackType must be empty", "", pickLine.WZ_F3_NKAllocatedPackType);

			if (!isPickByUOM)
			{
				pickLine.WZ_F3_NKAllocatedPackType = "UNT"; // to make sure system is not looking at reserved line to determine Pick By UOM is enabled or disabled.
			}
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var pickInNewFactory = newFactory.Load<WhsPick>(pick.PK);
			using (var form = new TestForm(pickInNewFactory))
			{
				form.Show();
				AssertEquals($"Group box with pick by UOM details grid must {(isPickByUOM ? "not " : "")}be visible.", isPickByUOM, form.UserControl.UOMGridForTest.Visible);
			}
		}

		#endregion

		#region Events

		#region TestClearAllStockAllocations_Click

		#region TestClearAllStockAllocations_Click

		public void TestClearAllStockAllocations_Click()
		{
			SetupBasicData();
			Pick = Factory.New<WhsPick>();
			Pick.Orders.Add(Order);
			AssertEquals("Precondition: Should be one item", 1, Pick.OrderedInventories.Count);
			WhsPickOrderedInventory orderedInventory = Pick.OrderedInventories[0];

			using (TestForm form = new TestForm(Pick))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				form.Show();
				PickLinesUserControl userControl = form.UserControl;
				userControl.AutoAllocateStock_Click(null, EventArgs.Empty);
				Assert("Should be some units allocated", orderedInventory.PickLineQuantity > 0);
				userControl.ClearAllStockAllocationsButton_Click(null, EventArgs.Empty);
				AssertEquals("Should be no units allocated", 0m, orderedInventory.PickLineQuantity);
			}
		}

		#endregion

		#region TestClearAllStockAllocations_Click_CannotClearPartiallyPickedPicks

		public void TestClearAllStockAllocations_Click_CannotClearPartiallyPickedPicks()
		{
			TestClearAllStockAllocations_Click_CannotClearPartiallyPickedPicks_Core(usingInTransitTransfer: false);
		}

		public void TestClearAllStockAllocations_Click_CannotClearPartiallyPickedPicks_InTransit()
		{
			TestClearAllStockAllocations_Click_CannotClearPartiallyPickedPicks_Core(usingInTransitTransfer: true);
		}

		void TestClearAllStockAllocations_Click_CannotClearPartiallyPickedPicks_Core(bool usingInTransitTransfer)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();

			if (usingInTransitTransfer)
			{
				Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
				Factory.Save();
			}
			else
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;

				using (Business.Testing.OutboundDockDoorHelper.MockOutboundDockDoorCreator())
				{
					Factory.Save();
					AssertEquals("Precondition: Picked Directly.", ZGuid.Empty, order.Lines[0].PickLines.Single().WZ_WE_OriginalPickedInventoryLine);
				}
			}

			AssertEquals("Precondition", 10m, Helper.GetTotalPickLineQuantity(pick));

			using (var form = new TestForm(pick))
			{
				form.Show();

				var userControl = form.UserControl;
				userControl.ClearAllStockAllocationsButton_Click(null, EventArgs.Empty);
				AssertEquals("When pick is partially or fully picked, then user should not be able to clear all allocations.", 10m, Helper.GetTotalPickLineQuantity(pick));
				AssertEquals(PickLinesUserControl.CannotPerformThisOperationBecausePickIsPartiallyOrFullyPickedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region TestClearAllStockAllocations_Click_ShowErrorWhenPickingCommenced

		public void TestClearAllStockAllocations_Click_ShowErrorWhenPickingCommenced()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);

			using (TestForm form = new TestForm(pick))
			{
				form.Show();
				PickLinesUserControl userControl1 = form.UserControl;

				var pickLine = pick.GetAllPickLines().First();
				pickLine.WZ_IsPicking = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				userControl1.ClearAllStockAllocationsButton_Click(null, EventArgs.Empty);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(PickLinesUserControl.NoChangesCanBeMadeToThisPickBecausePickingHasCommenced));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		#endregion

		#endregion

		#region TestAutoAllocateStock_Click

		public void TestAutoAllocateStock_Click()
		{
			SetupBasicData();
			Pick = Factory.New<WhsPick>();
			Pick.Orders.Add(Order);
			AssertEquals("Precondition: Should be one item", 1, Pick.OrderedInventories.Count);
			WhsPickOrderedInventory orderedInventory = Pick.OrderedInventories[0];

			using (TestForm form = new TestForm(Pick))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				form.Show();
				PickLinesUserControl userControl = form.UserControl;

				AssertEquals("Precondition: Should be no units allocated", 0m, orderedInventory.PickLineQuantity);
				userControl.AutoAllocateStock_Click(null, EventArgs.Empty);
				Assert("Should be some units allocated", orderedInventory.PickLineQuantity > 0);
			}
		}

		#endregion

		#region TestSortItemsForPicking_Click

		public void TestSortItemsForPicking_Click()
		{
			SetupBasicData();
			Helper.CreateWhsOrderLine(Order, InvData.Part1, 100m, "BEK1", "DummyOutward-1", "");

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(Order);
			AssertEquals("Precondition: Should be 2 items", 2, pick.OrderedInventories.Count);

			using (var userControl = new PickLinesUserControl())
			{
				userControl.SortItemsForPicking_Click(null, EventArgs.Empty);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(PickLinesUserControl.PleaseOpenThePickSlipTabBeforeSortingItemsMsg));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
			}

			using (var form = new TestForm(pick))
			{
				form.Show();

				// need to swap items around because Bind() performs a sort
				if (pick.OrderedInventories[1].BondedEntryKey.IsEmpty)
				{
					pick.OrderedInventories.Sort("BondedEntryKey", ListSortDirection.Ascending);
				}

				form.UserControl.SortItemsForPicking_Click(null, EventArgs.Empty);
				AssertEquals("Items have not been sorted", "BEK1", pick.OrderedInventories[0].BondedEntryKey);
			}
		}

		#endregion

		#region TestAssignAllPickLinesToUser

		[TestDate(2012, 05, 01)]
		public void TestAssignAllPickLinesToUser()
		{
			// Setup test data
			var data = new TestDataSimpleEnvironment(Factory);

			var activeStaff1 = Helper.CreateGlbStaff("01", "T1", true);
			var activeStaff2 = Helper.CreateGlbStaff("02", "T2", true);
			var activeStaff3 = Helper.CreateGlbStaff("03", "T3", true);
			var inactiveStaff = Helper.CreateGlbStaff("04", "T4", false);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 100m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();

			Factory.Save();

			AssertEquals(true, receive.IsFinalised);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order", Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);

			var pick = Helper.CreatePickNew(order);

			AssertEquals("Precondition: Should be 2 items", 2, pick.OrderedInventories.Count);

			var availableInventory1 = pick.OrderedInventories[0].AvailableInventories[0];
			var availableInventory2 = pick.OrderedInventories[1].AvailableInventories[0];
			availableInventory1.PickLineQuantity = 10;
			availableInventory2.PickLineQuantity = 10;
			availableInventory1.Allocate = true;
			availableInventory2.Allocate = true;
			// availableInventory1.PickedDate is empty
			// availableInventory2.PickedDate is empty

			Factory.Save();

			using (var form = new TestForm(pick))
			{
				form.UserControl.AssignAllPickLinesToUser(pick);

				var staffCollectionPropertyInfo = typeof(ZRecordAttacher).GetField("originalFindBoxList", BindingFlags.NonPublic | BindingFlags.Instance);
				var staffCollection = (GlbStaffCollection)staffCollectionPropertyInfo.GetValue(form.UserControl.PickLineAttacherForTesting);
				AssertCollectionContains("Collection should contain active staff.", activeStaff1, staffCollection);
				AssertCollectionContains("Collection should contain active staff.", activeStaff2, staffCollection);
				AssertCollectionContains("Collection should contain active staff.", activeStaff3, staffCollection);
				AssertCollectionNotContains("Collection should not contain inactive staff.", inactiveStaff, staffCollection);

				// All unassigned pick lines with Allocate checked.

				var embeddedModulePopup = form.UserControl.LastShownPickLineAttachPopupForTesting;
				Testing.TestEmbeddedModulePopupExtension.SelectStaffForEmbeddedModuleSelection(embeddedModulePopup, activeStaff1);
				AssertEquals(true, availableInventory1.PickLines.All(pl => pl.AssignedTo == activeStaff1));
				AssertEquals(true, availableInventory2.PickLines.All(pl => pl.AssignedTo == activeStaff1));

				// PickLines in Available are IsPicking.
				SetIsPicking(availableInventory1, true);
				SetIsPicking(availableInventory2, true);

				form.UserControl.AssignAllPickLinesToUser(pick);
				AssertEquals(PickLinesUserControl.NoUnAssignedLinesErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, availableInventory1.PickLines.All(pl => pl.AssignedTo == activeStaff1));
				AssertEquals(true, availableInventory2.PickLines.All(pl => pl.AssignedTo == activeStaff1));

				SetIsPicking(availableInventory1, false);
				SetIsPicking(availableInventory2, false);

				// line with picked date and a line without pick date allocate checked.

				Helper.SetPickedDate(availableInventory1, ZDateTimeOffset.Now);
				form.UserControl.AssignAllPickLinesToUser(pick);
				Testing.TestEmbeddedModulePopupExtension.SelectStaffForEmbeddedModuleSelection(embeddedModulePopup, activeStaff2);

				AssertEquals(true, availableInventory1.PickLines.All(pl => pl.AssignedTo == activeStaff1));
				AssertEquals(true, availableInventory2.PickLines.All(pl => pl.AssignedTo == activeStaff2));

				// All lines with pick date and allocate checked.

				Helper.SetPickedDate(availableInventory1, ZDateTimeOffset.Now);
				Helper.SetPickedDate(availableInventory2, ZDateTimeOffset.Now);
				form.UserControl.AssignAllPickLinesToUser(pick);

				AssertEquals(PickLinesUserControl.NoUnAssignedLinesErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, availableInventory1.PickLines.All(pl => pl.AssignedTo == activeStaff1));
				AssertEquals(true, availableInventory2.PickLines.All(pl => pl.AssignedTo == activeStaff2));

				// Allocated and non-allocated pick lines with no pick dates.
				Helper.SetPickedDate(availableInventory1, ZDateTimeOffset.Empty);
				availableInventory1.Allocate = true;
				Helper.SetPickedDate(availableInventory2, ZDateTimeOffset.Empty);
				availableInventory2.Allocate = false;
				form.UserControl.AssignAllPickLinesToUser(pick);
				Testing.TestEmbeddedModulePopupExtension.SelectStaffForEmbeddedModuleSelection(embeddedModulePopup, activeStaff3);

				AssertEquals(true, availableInventory1.PickLines.All(pl => pl.AssignedTo == activeStaff3));
				AssertEquals(true, availableInventory2.PickLines.All(pl => pl.AssignedTo == null));

				// Allocated and non-allocated pick line, but the allocated line has a picked date.
				Helper.SetPickedDate(availableInventory1, ZDateTimeOffset.Now);
				availableInventory1.Allocate = true;
				Helper.SetPickedDate(availableInventory2, ZDateTimeOffset.Empty);
				availableInventory2.Allocate = false;
				form.UserControl.AssignAllPickLinesToUser(pick);

				AssertEquals(PickLinesUserControl.NoUnAssignedLinesErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, availableInventory1.PickLines.All(pl => pl.AssignedTo == activeStaff3));
				AssertEquals(true, availableInventory2.PickLines.All(pl => pl.AssignedTo == null));

				// All Non allocated pick lines
				Helper.SetPickedDate(availableInventory1, ZDateTimeOffset.Empty);
				availableInventory1.Allocate = false;
				Helper.SetPickedDate(availableInventory2, ZDateTimeOffset.Empty);
				availableInventory2.Allocate = false;
				form.UserControl.AssignAllPickLinesToUser(pick);

				AssertEquals(PickLinesUserControl.NoUnAssignedLinesErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, availableInventory1.PickLines.All(pl => pl.AssignedTo == null));
				AssertEquals(true, availableInventory2.PickLines.All(pl => pl.AssignedTo == null));
			}
		}

		void SetIsPicking(WhsPickAvailableInventory availableInventory, bool isPicking)
		{
			foreach (var pickLine in availableInventory.PickLines)
			{
				pickLine.WZ_IsPicking = isPicking;
			}
		}

		#endregion

		#region TestUnassign All Lines

		public void TestUnassignAllLines()
		{
			var user = Helper.CreateGlbStaff("XYZ", "XYZ");

			var createdPick = PickSampleData(user);

			var pickLines = createdPick.GetAllPickLines();

			AssertNotNull("CreatedDocket cannot be null", createdPick);

			AssertEquals("Precondition: Pick lines are assigned", true, pickLines.All(pl => pl.AssignedTo == user));

			using (var form = new TestForm(createdPick))
			{
				form.UserControl.UnAssignAllPickLines(createdPick);

				AssertEquals("Pick lines have been un-assigned", true, pickLines.All(pl => pl.AssignedTo == null));
			}
		}

		WhsPick PickSampleData(GlbStaff user)
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 5);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var notify = new TestNotificationBuffer();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive", notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locations[0]);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 5m, locations[1]);

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();

			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order1, data.Part2, 5m);

			var createdPick = Helper.CreatePickByAttachingOrders(order1);

			var orderedInvetoryForCreatedPick1 = createdPick.OrderedInventories[0];
			var orderedInvetoryForCreatedPick2 = createdPick.OrderedInventories[1];

			var allocatedForCreatedPick1 = Helper.SetAvailableInventory(orderedInvetoryForCreatedPick1.AvailableInventories[0], true, ZDateTimeOffset.Empty);
			var allocatedForCreatedPick2 = Helper.SetAvailableInventory(orderedInvetoryForCreatedPick2.AvailableInventories[0], true, ZDateTimeOffset.Empty);

			createdPick.OrderedInventories.Add(orderedInvetoryForCreatedPick1);
			createdPick.OrderedInventories.Add(orderedInvetoryForCreatedPick2);
			Factory.Save();

			createdPick.WP_PickStatus = PickStatus.Codes.Created;

			foreach (var line in createdPick.GetAllPickLines())
			{
				line.WZ_GS_NKAssignedTo = user.GS_Code;
			}

			Factory.Save();

			return createdPick;
		}

		#endregion

		#region TestContextMenuItems

		public void TestContextMenuItems()
		{
			using (TestForm form = new TestForm(Pick))
			{
				form.Show();
				Assert("Action menuitem not added", form.UserControl.InventoryGridForTest.ContextMenu.MenuItems.FindByText("Assign Selected Lines to User") != null);
			}
		}

		#endregion

		#region TestAssignSelectedLinesToUser_ContextMenuClick

		[TestDate(2012, 05, 29)]
		public void TestAssignSelectedLinesToUser_ContextMenuClick()
		{
			// Setup test data
			var data = new TestDataSimpleEnvironment(Factory, 1, 9);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var activeStaff1 = Helper.CreateGlbStaff("01", "T1", true);
			var activeStaff2 = Helper.CreateGlbStaff("02", "T2", true);
			var activeStaff3 = Helper.CreateGlbStaff("03", "T3", true);
			var activeStaff4 = Helper.CreateGlbStaff("05", "T5", true);
			var inactiveStaff = Helper.CreateGlbStaff("04", "T4", false);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locations[0]);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locations[1]);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locations[2]);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locations[3]);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locations[4]);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locations[5]);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locations[6]);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locations[7]);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locations[8]);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();

			Factory.Save();

			AssertEquals(true, receive.IsFinalised);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order", Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 100m);

			var pick = Helper.CreatePickNew(order);

			AssertEquals("Precondition: Should be 3 items", 1, pick.OrderedInventories.Count);

			var selectedAllocatedAndPickedLine = pick.OrderedInventories[0].AvailableInventories[0];
			var selectedNotAllocatedAndPickedLine = pick.OrderedInventories[0].AvailableInventories[1];
			var selectedNotAllocatedAndNotPickedLine = pick.OrderedInventories[0].AvailableInventories[2];
			var selectedAllocatedAndNotPickedLine = pick.OrderedInventories[0].AvailableInventories[3];
			var selectedAllocatedAndPickedLineIsPicking = pick.OrderedInventories[0].AvailableInventories[8];

			var notSelectedAllocatedAndPickedLine = pick.OrderedInventories[0].AvailableInventories[4];
			var notSelectedNotAllocatedAndPickedLine = pick.OrderedInventories[0].AvailableInventories[5];
			var notSelectedNotAllocatedAndNotPickedLine = pick.OrderedInventories[0].AvailableInventories[6];
			var notSelectedAllocatedAndNotPickedLine = pick.OrderedInventories[0].AvailableInventories[7];

			selectedAllocatedAndPickedLine.Allocate = true;
			selectedNotAllocatedAndPickedLine.Allocate = false;
			selectedNotAllocatedAndNotPickedLine.Allocate = false;
			selectedAllocatedAndNotPickedLine.Allocate = true;
			selectedAllocatedAndPickedLineIsPicking.Allocate = true;

			notSelectedAllocatedAndPickedLine.Allocate = true;
			notSelectedNotAllocatedAndPickedLine.Allocate = false;
			notSelectedNotAllocatedAndNotPickedLine.Allocate = false;
			notSelectedAllocatedAndNotPickedLine.Allocate = true;

			selectedAllocatedAndPickedLine.AvailableInventoriesSplitByPickedDetails.Cast<WhsPickAvailableInventorySplitByPickedDetails>().Single().PickedDate = ZDateTimeOffset.Now;
			selectedAllocatedAndNotPickedLine.AvailableInventoriesSplitByPickedDetails.Cast<WhsPickAvailableInventorySplitByPickedDetails>().Single().PickedDate = ZDateTimeOffset.Empty;

			notSelectedAllocatedAndPickedLine.AvailableInventoriesSplitByPickedDetails.Cast<WhsPickAvailableInventorySplitByPickedDetails>().Single().PickedDate = ZDateTimeOffset.Now;
			Factory.Save();

			// Clear defaulted Pickers
			selectedAllocatedAndPickedLine.AvailableInventoriesSplitByPickedDetails[0].AssignedToPK = ZGuid.Empty;
			notSelectedAllocatedAndPickedLine.AvailableInventoriesSplitByPickedDetails[0].AssignedToPK = ZGuid.Empty;
			selectedAllocatedAndPickedLineIsPicking.AvailableInventoriesSplitByPickedDetails[0].AssignedToPK = activeStaff4.PK;

			using (var form = new TestForm(pick))
			{
				form.Show();

				var grid = form.UserControl.InventoryGridForTest;
				var menuItem = grid.ContextMenu.MenuItems.FindByText("Assign Selected Lines to User");
				Testing.GUITestHelper.SelectElements(grid, new BusinessObject[] { selectedAllocatedAndPickedLine, selectedNotAllocatedAndPickedLine, selectedNotAllocatedAndNotPickedLine, selectedAllocatedAndNotPickedLine });
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals(true, menuItem.Enabled);

				menuItem.PerformClick();

				var staffCollectionPropertyInfo = typeof(ZRecordAttacher).GetField("originalFindBoxList", BindingFlags.NonPublic | BindingFlags.Instance);
				var staffCollection = (GlbStaffCollection)staffCollectionPropertyInfo.GetValue(form.UserControl.PickLineAttacherForTesting);
				AssertCollectionContains("Collection should contain active staff.", activeStaff1, staffCollection);
				AssertCollectionContains("Collection should contain active staff.", activeStaff2, staffCollection);
				AssertCollectionContains("Collection should contain active staff.", activeStaff3, staffCollection);
				AssertCollectionContains("Collection should contain active staff.", activeStaff4, staffCollection);
				AssertCollectionNotContains("Collection should not contain inactive staff.", inactiveStaff, staffCollection);

				// Non-Finalised pick.

				Testing.TestEmbeddedModulePopupExtension.SelectStaffForEmbeddedModuleSelection(form.UserControl.LastShownPickLineAttachPopupForTesting, activeStaff1);
				AssertEquals(true, selectedAllocatedAndPickedLine.PickLines.All(pl => pl.AssignedTo == null));
				AssertEquals(true, selectedNotAllocatedAndPickedLine.PickLines.All(pl => pl.AssignedTo == null));
				AssertEquals(true, selectedNotAllocatedAndNotPickedLine.PickLines.All(pl => pl.AssignedTo == null));
				AssertEquals(true, selectedAllocatedAndNotPickedLine.PickLines.All(pl => pl.AssignedTo == activeStaff1));
				AssertEquals(true, selectedAllocatedAndPickedLineIsPicking.PickLines.All(pl => pl.AssignedTo == activeStaff4));

				AssertEquals(true, notSelectedAllocatedAndPickedLine.PickLines.All(pl => pl.AssignedTo == null));
				AssertEquals(true, notSelectedNotAllocatedAndPickedLine.PickLines.All(pl => pl.AssignedTo == null));
				AssertEquals(true, notSelectedNotAllocatedAndNotPickedLine.PickLines.All(pl => pl.AssignedTo == null));
				AssertEquals(true, notSelectedAllocatedAndNotPickedLine.PickLines.All(pl => pl.AssignedTo == null));

				Testing.GUITestHelper.SelectElements(grid, new[] { selectedAllocatedAndPickedLine });
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals("Since line is allocated and picked, context menu item should be disabled.", false, menuItem.Enabled);

				Testing.GUITestHelper.SelectElements(grid, new[] { selectedNotAllocatedAndPickedLine });
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals("Since line is not allocated, context menu item should be disabled.", false, menuItem.Enabled);

				Testing.GUITestHelper.SelectElements(grid, new[] { selectedNotAllocatedAndNotPickedLine });
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals("Since line is not allocated, context menu item should be enabled.", false, menuItem.Enabled);

				Testing.GUITestHelper.SelectElements(grid, new[] { selectedAllocatedAndNotPickedLine });
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals("Since line allocated and doesn't have a picked date, context menu item should be enabled.", true, menuItem.Enabled);

				Testing.GUITestHelper.SelectElements(grid, Array.Empty<BusinessObject>());
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals("Since no line is selected, context menu item should be disabled.", false, menuItem.Enabled);

				// Set Is Picking
				SetIsPicking(selectedAllocatedAndPickedLineIsPicking, true);

				Testing.GUITestHelper.SelectElements(grid, new[] { selectedAllocatedAndPickedLineIsPicking });
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals("Since line is Picking, context menu item should be disabled.", false, menuItem.Enabled);
			}
		}

		#endregion

		#region TestAssignLinesErrorMessages

		public void TestAssignLinesErrorMessages()
		{
			AssertEquals("No lines can be assigned. There are no Allocated lines with an empty Picked Date, or Picking has already commenced.", PickLinesUserControl.NoUnAssignedLinesErrorMessage);
		}

		#endregion

		#region TestAvailableInventoryGrid_ColourDeciding

		[TestDate(2013, 3, 5)]
		public void TestAvailableInventoryGrid_ColourDeciding()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			Helper.SetClientAttributeType(data.Org1, Enterprise.Warehouse.Transactions.Business.Testing.AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(data.Org1, Enterprise.Warehouse.Transactions.Business.Testing.AttributeNumber.Two, PartAttributeTypeList.Codes.JulianBatchNumber); // Will turn on usage of Expiry Date
			Helper.SetProductAttributeUse(data.Org1, data.Part1, Enterprise.Warehouse.Transactions.Business.Testing.AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, Enterprise.Warehouse.Transactions.Business.Testing.AttributeNumber.Two, true); // Will turn on usage of Expiry Date
			data.Part2.RelatedOrganisations[0].OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.YDDD_BatchNumber;

			Helper.CreateProductParamsByWhsAndClient(data.Part2, data.Org1, data.Whs1).W3_MaximumShelfLife = 30;

			var consignee = Helper.CreateClient("CONSIGNEE");
			consignee.MiscServ.OM_MinimumShelfLifeAccepted = 10;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, ZDate.Empty, ZDate.Empty, "PA1", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 5m, ZDate.Empty, ZDate.Empty, "", "3030ABC", "", ""); // Expiry date < Min Shelf Life
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 7m, ZDate.Empty, ZDate.Empty, "", "3045ABC", "", ""); // Expiry date = Min Shelf Life
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, ZDate.Empty, ZDate.Empty, "", "3060ABC", "", ""); // Expiry date > Min Shelf Life
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1.PK, data.Whs1.PK, consignee.PK, "O1", ZDateTimeOffset.Today, Notify, WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 3m);
			Helper.CreateWhsOrderLine(order, data.Part2, 22m);

			var pick = Helper.CreatePickNew(order);

			using (var form = new TestForm(pick))
			{
				form.Show();

				var userControl = form.UserControl;
				userControl.SetMinimumShelfLifeForTest(10);

				// Normal Attribute - No color codding expected.
				var orderedInventory_NormalAttribute = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(l => l.SupplierPart == data.Part1);
				AssertGridLineColorSyncronized(userControl, orderedInventory_NormalAttribute.AvailableInventories[0], Color.Empty);

				// Julian Batch Number Attribute - Only available inventory with Expiry Date < Min Shelf Life should be marked with red color.
				var orderedInventory_JulianBatchNumber = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(l => l.SupplierPart == data.Part2);
				var availableInventories = orderedInventory_JulianBatchNumber.AvailableInventories.Cast<WhsPickAvailableInventory>();
				AssertGridLineColorSyncronized(userControl, availableInventories.Single(l => l.QuantityAvailableToPick == 5m), Color.LightSalmon);
				AssertGridLineColorSyncronized(userControl, availableInventories.Single(l => l.QuantityAvailableToPick == 7m), Color.Empty);
				AssertGridLineColorSyncronized(userControl, availableInventories.Single(l => l.QuantityAvailableToPick == 10m), Color.Empty);

				// Allocate all stock and finalised orderd with Pick. This way all availableInventories will be visible after pick is finalised.
				foreach (WhsPickOrderedInventory orderedInventory in pick.OrderedInventories)
				{
					foreach (WhsPickAvailableInventory availableInventory in orderedInventory.AvailableInventories)
					{
						availableInventory.Allocate = true;
					}
				}
				pick.FinaliseAllOrders();
				pick.FinalisePick();
				AssertIsFinalisedPrecondition(order);
				AssertIsFinalisedPrecondition(pick);

				AssertGridLineColorSyncronized(userControl, orderedInventory_NormalAttribute.AvailableInventories[0], Color.Empty);
				AssertGridLineColorSyncronized(userControl, availableInventories.Single(l => l.PickLineQuantity == 5m && l.QuantityAvailableToPick == 0m), Color.Empty);
				AssertGridLineColorSyncronized(userControl, availableInventories.Single(l => l.PickLineQuantity == 7m && l.QuantityAvailableToPick == 0m), Color.Empty);
				AssertGridLineColorSyncronized(userControl, availableInventories.Single(l => l.PickLineQuantity == 10m && l.QuantityAvailableToPick == 0m), Color.Empty);
			}
		}

		void AssertGridLineColorSyncronized(PickLinesUserControl userControl, WhsPickAvailableInventory availableInventory, Color expectedColor)
		{
			var args = new ColourDecidingEventArgs(availableInventory);
			userControl.AvailableInventoryGrid_ColourDecidingForTest(null, args);
			AssertEquals(expectedColor, args.Colour);
		}

		#endregion

		#endregion

		#region TestNoDeleteFromSplitByTable

		#region TestNoDeleteInSplitByPackTypeTable

		public void TestNoDeleteInSplitByPackTypeTable()
		{
			CantDeleteFromTable(true);
		}

		#endregion

		#region TestNoDeleteInSplitByPickerDetailsTable

		public void TestNoDeleteInSplitByPickerDetailsTable()
		{
			CantDeleteFromTable(false);
		}

		#endregion

		void CantDeleteFromTable(bool isPickByUOMEnabled)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = isPickByUOMEnabled;
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 25m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 25m);
			var pick = Helper.CreatePickNew(order);

			using (var form = new TestForm(pick))
			{
				form.Show();
				var contextMenu = new ContextMenu();
				if (isPickByUOMEnabled)
				{
					contextMenu = form.UserControl.UOMGridForTest.ContextMenu;
				}
				else
				{
					contextMenu = form.UserControl.GridPickByPickedDetailsForTest.ContextMenu;
				}

				contextMenu.ShowPopupMenu();
				AssertEquals(contextMenu.MenuItems.FindByText("Delete").Visible, false);
			}
		}

		#endregion

		#region Implementation

		protected void AssertAttributeVisibility(ZGrid grid, bool expiry, bool packing, bool part1, bool part2, bool part3, bool custom1, bool custom2, bool custom3)
		{
			AssertEquals("Expiry visibility incorrect", expiry, grid.Columns["ExpiryDate"].IsVisible);
			AssertEquals("Packing visibility incorrect", packing, grid.Columns["PackingDate"].IsVisible);
			AssertEquals("PartAttrib1 visibility incorrect", part1, grid.Columns["PartAttrib1"].IsVisible);
			AssertEquals("PartAttrib2 visibility incorrect", part2, grid.Columns["PartAttrib2"].IsVisible);
			AssertEquals("PartAttrib3 visibility incorrect", part3, grid.Columns["PartAttrib3"].IsVisible);
		}

		protected void AssertAttributeTitles(ZGrid grid, string part1, string part2, string part3)
		{
			if (!string.IsNullOrEmpty(part1))
			{
				AssertEquals("PartAttrib1 title incorrect", part1, grid.Columns["PartAttrib1"].ColumnStyle.HeaderText);
			}

			if (!string.IsNullOrEmpty(part2))
			{
				AssertEquals("PartAttrib2 title incorrect", part2, grid.Columns["PartAttrib2"].ColumnStyle.HeaderText);
			}

			if (!string.IsNullOrEmpty(part3))
			{
				AssertEquals("PartAttrib3 title incorrect", part3, grid.Columns["PartAttrib3"].ColumnStyle.HeaderText);
			}
		}

		void SetupBasicData()
		{
			InvData = new TestDataForInventory(Factory);
			InvData.CreateSimpleInventoryManyLines();
			InvData.Receive11.Inventory[1].WI_InventoryStatus = InventoryStatus.Codes.Held;
			InvData.Receive11.Inventory[1].InDocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Held;
			InvData.Receive11.Inventory[1].InDocketLine.WE_WHC_NKCurrentInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;
			InvData.Receive11.Inventory[3].WI_InventoryStatus = InventoryStatus.Codes.Held;
			InvData.Receive11.Inventory[3].InDocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Held;
			InvData.Receive11.Inventory[3].InDocketLine.WE_WHC_NKCurrentInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;
			Factory.Save();

			Order = Helper.CreateWhsOrder(InvData.Org1, InvData.Whs1, "1");
			Helper.CreateWhsOrderLine(Order, InvData.Part1, 100m);
		}

		protected class TestForm : ZForm
		{
			public TestForm(WhsPick pick) : base(pick) { }
			public PickLinesUserControl UserControl;

			protected override void InitializeComponent()
			{
				this.UserControl = new PickLinesUserControl();
				this.Controls.Add(this.UserControl);
				this.DataSourceAssemblyName = "Enterprise.Warehouse.Transactions.Business";
				this.DataSourceTypeName = "Enterprise.Warehouse.Transactions.Business.WhsPick";
			}
		}

		WhsPick Pick;
		WhsOrder Order;
		TestDataForInventory InvData;

		#endregion

		#region TestQuantityInventoryQuantityInformationShouldHideAfterFinalisingPick

		public void TestQuantityInventoryQuantityInformationShouldHideAfterFinalisingPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 3);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5);
			var pick = Helper.CreatePickNew(order1, order2);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Precondition", 20m, availableInventory.QuantityAvailableToPick);

			AssertAvailableAndAllocateColumnsVisibility(pick, isAvailable: true);

			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);

			AssertAvailableAndAllocateColumnsVisibility(pick, isAvailable: false);
		}

		static void AssertAvailableAndAllocateColumnsVisibility(WhsPick pick, bool isAvailable)
		{
			using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
			{
				form.ShowPickSlipTab();
				Application.DoEvents();
				form.Show();
				var pickLinesUserControl = (PickLinesUserControl)form.PickSlipTabPageForTest.Find(c => c.Name == "PickLinesUserControl").First();
				var inventoryGrid = (ZGrid)pickLinesUserControl.Find(c => c.Name == "InventoryGrid").First();

				AssertEquals($"Pick is {(pick.IsFinalised ? "" : "not ")} finalised, column should be {(!pick.IsFinalised ? "" : "not ")}available.", isAvailable, inventoryGrid.Columns.Contains("QuantityTotal"));
				AssertEquals($"Pick is {(pick.IsFinalised ? "" : "not ")} finalised, column should be {(!pick.IsFinalised ? "" : "not ")}available.", isAvailable, inventoryGrid.Columns.Contains("QuantityCommitted"));
				AssertEquals($"Pick is {(pick.IsFinalised ? "" : "not ")} finalised, column should be {(!pick.IsFinalised ? "" : "not ")}available.", isAvailable, inventoryGrid.Columns.Contains("QuantityCrossDocked"));
				AssertEquals($"Pick is {(pick.IsFinalised ? "" : "not ")} finalised, column should be {(!pick.IsFinalised ? "" : "not ")}available.", isAvailable, inventoryGrid.Columns.Contains("QuantityAvailableToPick"));
				AssertEquals($"Pick is {(pick.IsFinalised ? "" : "not ")} finalised, column should be {(!pick.IsFinalised ? "" : "not ")}available.", isAvailable, inventoryGrid.Columns.Contains("Allocate"));

				AssertEquals("Although when pick is finalise is not show the columns, but always should have correct value.", pick.IsFinalised ? 12m : 20m, pick.OrderedInventories[0].AvailableInventories[0].QuantityAvailableToPick);
			}
		}

		#endregion

		#region TestAllocatedQtyColumnVisibility

		public void TestAllocatedQtyColumnVisibility()
		{
			AssertColumnVisibility("PickLineQuantity", "Allocated Qty", "PickLinesUserControl", "ItemsGrid");
		}

		#endregion

		#region TestPickedQtyColumnVisibility

		public void TestPickedQtyColumnVisibility()
		{
			AssertColumnVisibility("PickQuantity", "Picked Qty", "PickLinesUserControl", "ItemsGrid");
		}

		#endregion

		#region TestAllocationKeyColumn_OrderedInventoryGrid

		public void TestAllocationKeyColumn_OrderedInventoryGrid_InwardProcessingEnabled() => TestAllocationKeyColumn_OrderedInventoryGrid(inwardProcessingEnabled: true);

		public void TestAllocationKeyColumn_OrderedInventoryGrid_InwardProcessingDisabled() => TestAllocationKeyColumn_OrderedInventoryGrid(inwardProcessingEnabled: false);

		void TestAllocationKeyColumn_OrderedInventoryGrid(bool inwardProcessingEnabled)
		{
			var mock = new Mock<Enterprise.Integration.Customs.ISupportedForProcessing>();
			mock.Setup(m => m.IsSupportedForProcessing()).Returns(inwardProcessingEnabled);

			using (ObjectFactory.Substitute(mock.Object))
			{
				AssertColumnVisibility("AllocationKey", "Allocation Key", "PickLinesUserControl", "ItemsGrid", visible: inwardProcessingEnabled);
			}
		}

		#endregion

		#region TestHoldCodeColumnAvailable

		public void TestHoldCodeColumnAvailable_GivenHLDCode() => TestHoldCodeColumnAvailable_Core("HLD", true);
		public void TestHoldCodeColumnAvailable_GivenDMGCode() => TestHoldCodeColumnAvailable_Core("DMG", true);
		public void TestHoldCodeColumnAvailable_GivenEmpty() => TestHoldCodeColumnAvailable_Core("", false);
		public void TestHoldCodeColumnAvailable_GivenEmpty_HeldGoodsForOrdersDisabled() => TestHoldCodeColumnAvailable_Core("", false, enableHeldGoodsForOrders: false);

		void TestHoldCodeColumnAvailable_Core(string holdCode, bool expectIsColumnAvailable, bool enableHeldGoodsForOrders = true)
		{
			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableHeldGoodsForOrders))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				data.Whs1.WarehouseAddress.OA_RN_NKCountryCode = "FR";

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
				if (!string.IsNullOrEmpty(holdCode))
				{
					Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20, data.Whs1.DefaultLocation, InventoryStatus.Codes.Held, holdCode);
				}
				else
				{
					Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20, data.Whs1.DefaultLocation);
				}
				receive.FinaliseDocketWithoutUserConfirmation();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
				var line1 = Helper.CreateWhsOrderLine(order1, data.Part1, 3);
				var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
				var line2 = Helper.CreateWhsOrderLine(order2, data.Part1, 5);

				if (!string.IsNullOrEmpty(holdCode))
				{
					line1.WE_WHC_NKOrderedHeldCode = holdCode;
					line2.WE_WHC_NKOrderedHeldCode = holdCode;
				}
				var pick = Helper.CreatePickNew(order1, order2);

				using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
				{
					form.ShowPickSlipTab();
					Application.DoEvents();
					form.Show();
					var mainControl = (PickLinesUserControl)form.PickSlipTabPageForTest.Find(c => c.Name == "PickLinesUserControl").First();

					var pickLinesGrid = (ZGrid)mainControl.Find(c => c.Name == "ItemsGrid").First();
					var inventoryLinesGrid = (ZGrid)mainControl.Find(c => c.Name == "InventoryGrid").First();

					AssertEquals(expectIsColumnAvailable, pickLinesGrid.Columns.Contains(nameof(WhsPickOrderedInventory.OrderedHeldCode)));
					AssertEquals(expectIsColumnAvailable, inventoryLinesGrid.Columns.Contains(nameof(WhsPickAvailableInventory.HoldCode)));
				}
			}
		}

		#endregion

		#region TestInventoryToPickGroupBoxCaption

		public void TestInventoryToPickGroupBoxCaption_GivenHLDCode() => TestInventoryToPickGroupBoxCaptionCore("HLD", "Held Inventory");
		public void TestInventoryToPickGroupBoxCaption_GivenDMGCode() => TestInventoryToPickGroupBoxCaptionCore("DMG", "Held Inventory");
		public void TestInventoryToPickGroupBoxCaption_GivenEmpty() => TestInventoryToPickGroupBoxCaptionCore("", "Available Inventory");

		public void TestInventoryToPickGroupBoxCaption_GivenHLDCode_HeldGoodsForOrdersRegistryDisable() => TestInventoryToPickGroupBoxCaptionCore("HLD", "Available Inventory", enableHeldGoodsForOrders: false);
		public void TestInventoryToPickGroupBoxCaption_GivenDMGCode_HeldGoodsForOrdersRegistryDisable() => TestInventoryToPickGroupBoxCaptionCore("DMG", "Available Inventory", enableHeldGoodsForOrders: false);
		public void TestInventoryToPickGroupBoxCaption_GivenEmpty_HeldGoodsForOrdersRegistryDisable() => TestInventoryToPickGroupBoxCaptionCore("", "Available Inventory", enableHeldGoodsForOrders: false);

		void TestInventoryToPickGroupBoxCaptionCore(string holdCode, string expectedCaption, bool enableHeldGoodsForOrders = true)
		{
			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableHeldGoodsForOrders))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				data.Whs1.WarehouseAddress.OA_RN_NKCountryCode = "FR";

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
				if (!string.IsNullOrEmpty(holdCode))
				{
					Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20, data.Whs1.DefaultLocation, InventoryStatus.Codes.Held, holdCode);
				}
				else
				{
					Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20, data.Whs1.DefaultLocation);
				}
				receive.FinaliseDocketWithoutUserConfirmation();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
				var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
				var line1 = Helper.CreateWhsOrderLine(order1, data.Part1, 3);
				var line2 = Helper.CreateWhsOrderLine(order2, data.Part1, 5);

				if (!string.IsNullOrEmpty(holdCode))
				{
					line1.WE_WHC_NKOrderedHeldCode = holdCode;
					line2.WE_WHC_NKOrderedHeldCode = holdCode;
				}

				var pick = Helper.CreatePickNew(order1, order2);

				using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
				{
					form.ShowPickSlipTab();
					Application.DoEvents();
					form.Show();
					var inventoryToPickGroupBoxControl = (ZGroupBox)form.PickSlipTabPageForTest.Find(c => c.Name == "InventoryToPickGroupBox").First();
					AssertEquals(expectedCaption, inventoryToPickGroupBoxControl.CaptionResourceString.Caption);
				}
			}
		}

		#endregion

		#region TestAllocationKeyColumn_AvailableInventoryGrid

		public void TestAllocationKeyColumn_AvailableInventoryGrid_InwardProcessingEnabled() => TestAllocationKeyColumn_AvailableInventoryGrid(inwardProcessingEnabled: true);

		public void TestAllocationKeyColumn_AvailableInventoryGrid_InwardProcessingDisabled() => TestAllocationKeyColumn_AvailableInventoryGrid(inwardProcessingEnabled: false);

		void TestAllocationKeyColumn_AvailableInventoryGrid(bool inwardProcessingEnabled)
		{
			var mock = new Mock<Enterprise.Integration.Customs.ISupportedForProcessing>();
			mock.Setup(m => m.IsSupportedForProcessing()).Returns(inwardProcessingEnabled);

			using (ObjectFactory.Substitute(mock.Object))
			{
				AssertColumnVisibility("AllocationKey", "Allocation Key", "PickLinesUserControl", "InventoryGrid", visible: inwardProcessingEnabled);
			}
		}

		#endregion

		#region TestDeclarantsReferenceColumnInitialised_AvailableInventoryGrid

		public void TestDeclarantsReferenceColumnInitialised_AvailableInventoryGrid()
		{
			var pick = Factory.New<WhsPick>();
			using (var form = new TestForm(pick))
			{
				form.Show();

				var column = form.UserControl.InventoryGridForTest.Columns["DeclarantsReference"];
				CombineAssertions(() =>
				{
					AssertNotNull("Column is Available in Grid", column);
					AssertEquals("Not Visible by default", false, column.IsVisible);
				});
			}
		}

		#endregion

		#region TestPickLinesUserControl_SerialNumberSelector

		public void TestPickLinesUserControl_SerialNumberSelector()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var helper = new WhsTestHelperFunctions(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R01");
				var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
				receiveLine.WE_SerialNumber = "SN0";
				for (int i = 0; i < 10; i++)
				{
					var pivot = receiveLine.SerialNumbers.AddNew();
					pivot.SerialNumberValue = $"SN{i}";
				}
				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
				Factory.Save();

				var pick = Helper.CreatePickNew(order);
				var pickLine = pick.GetAllPickLines().Single();

				AssertSerialNumberGridIsVisible(enableSchemaRedesignChanges: true);

				AssertSerialNumberGridIsVisible(enableSchemaRedesignChanges: false);

				void AssertSerialNumberGridIsVisible(bool enableSchemaRedesignChanges)
				{
					using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableSchemaRedesignChanges))
					{
						using (var form = new TestForm(pick))
						{
							form.Show();

							AssertEquals("Precondition - Should have serial number", false, pick.OrderedInventories[0].AvailableInventories[0].Inventory[0].WI_SerialNumber.IsEmpty);
							AssertEquals("Precondition - Serial number showed not show if EnableSchemaRedesignChanges is enabled",
								enableSchemaRedesignChanges, pick.OrderedInventories[0].AvailableInventories[0].SerialNumber.IsEmpty);

							var serialNumberSelector = (SerialNumberSelectorUserControl)form.UserControl.Find(c => c.Name == "SerialNumberSelector").First();
							var inventoryGrid = (ZGrid)serialNumberSelector.Find(c => c.Name == "SerialNumberGrid").First();

							AssertEquals("Grid should be visible if EnableSchemaRedesignChanges is enabled.", enableSchemaRedesignChanges, inventoryGrid.Visible);
						}
					}
				}
			}
		}

		#endregion

		#region AssertColumnVisibility

		void AssertColumnVisibility(string columnName, string captionName, string controlName, string gridName, bool visible = true)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RN_NKCountryCode = "FR";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 3);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5);
			var pick = Helper.CreatePickNew(order1, order2);

			using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
			{
				form.ShowPickSlipTab();
				Application.DoEvents();
				form.Show();
				var pickLinesUserControl = (PickLinesUserControl)form.PickSlipTabPageForTest.Find(c => c.Name == controlName).First();
				var grid = (ZGrid)pickLinesUserControl.Find(c => c.Name == gridName).First();

				AssertEquals(visible, grid.Columns.Contains(columnName));
				AssertEquals(visible ? captionName : null, grid.GetColumnCaption(columnName));
			}
		}

		#endregion
	}
}
