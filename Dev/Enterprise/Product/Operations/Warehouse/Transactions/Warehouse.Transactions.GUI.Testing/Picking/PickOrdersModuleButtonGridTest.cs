using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class PickOrdersModuleButtonGridTest : WhsGuiTestCaseWithFactory
	{
		#region TestPerformance_DetachingOrders

		public void TestPerformance_DetachingOrders()
		{
			// will be same for Release Form, but fix is in business layer, so not point having 2 tests.
			const int NumberOfOrdersToCreate = 10;
			const int NumberOfPackagesPerOrder = 10;
			const int ExpectedNumberOfPropertyHits = 2200; // a lot of hits but linear growth if you 2x Order Number and 2x Package Number then will be 4x hits.

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, Constants.PkgUnit.Box)).F3_UOMType = UOMPackTypesList.Codes.Case;

			data.Part1.PartUnits.DeleteAll();
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Box, 5m);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, NumberOfOrdersToCreate * NumberOfPackagesPerOrder * 5m);
			Factory.Save();

			var pick = Helper.CreatePickNew();
			pick.WP_PickCasesByLabel = true;
			for (int i = 0; i < NumberOfOrdersToCreate; i++)
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O" + i, data.Part1, NumberOfPackagesPerOrder * 5m);
				pick.Orders.Add(order);
			}
			pick.AutoAllocateItemsWithMock();
			Factory.Save();

			pick.AllocatePackageLabels();
			Factory.Save();
			AssertEquals("Precondition", NumberOfOrdersToCreate * NumberOfPackagesPerOrder, pick.Orders.Cast<WhsOrder>().Sum(o => o.PackageJob.Packages.Count));

			using (var form = new TestForm(pick))
			{
				form.Show();

				form.UserControl.InnerGrid.SelectAllElements();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AssertPersistentPropertiesHitCount("Detaching Orders should have low hit count of PkgPackage properties.", typeof(PkgPackage), ExpectedNumberOfPropertyHits, form.UserControl.DetachButtonForTest.PerformClick);
			}
			AssertEquals("Ensure function worked.", 0, pick.Orders.Count);
		}

		#endregion

		#region Constructor

		public void TestConstructor()
		{
			using (var userControl = new PickOrdersModuleButtonGrid())
			{
				AssertEquals(RemoveAction.NoRemovePossible, userControl.InnerGrid.RemoveAction);
				AssertNotNull(userControl.InnerGrid.ContextMenu.MenuItems.FindByText("Override Fulfillment Rule"));
			}
		}

		#endregion

		#region Overrides

		#region TestRecordAttacher

		public void TestRecordAttacher()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var orders = new List<WhsOrder>();
			for (int i = 0; i < 11; i++)
			{
				orders.Add(Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O" + i, data.Part1, 1m));
			}

			var pick = Helper.CreatePickNew(orders[0]);

			var pickLineQuantityValidationCounter = 0;
			var quantityShortValidationCounter = 0;

			var orderedInventory = pick.OrderedInventories[0];
			orderedInventory.PickLineQuantityInfo.AdditionalValidation += () => pickLineQuantityValidationCounter++;
			orderedInventory.QuantityShortInfo.AdditionalValidation += () => quantityShortValidationCounter++;

			using (var form = new TestForm(pick))
			{
				form.Show();

				form.UserControl.AttachButton_ClickForTest(form.UserControl, EventArgs.Empty);
				using (var popup = form.UserControl.LastShownAttachPopupForTesting)
				{
					popup.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(orders.Skip(1).ToArray());
				}

				AssertContainsExactElementsInAnyOrder("Precondition: Should have attached orders.", orders, pick.Orders);
				AssertEquals("Should have validated PickLineQuantity once.", 1, pickLineQuantityValidationCounter);
				AssertEquals("Should have validated QuantityShort once.", 1, quantityShortValidationCounter);
			}
		}

		#endregion

		public void TestCannotDetachFinalisedOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			using (var form = new TestForm(pick))
			{
				form.Show();
				order.WD_FinalisedDate = ZDateTimeOffset.Now;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.UserControl.DetachButton_ClickForTest(null, EventArgs.Empty);
				AssertEquals("Last Message was error.", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals($"Last Message shown has correct content", true, UnitTestUserNotification.Instance.LastMessage.Text.Contains("Warehouse Order W00000001: This order cannot be detached because it is finalized"));
			}
		}

		public void TestDetachSkipsFinalisedOrders_ButDetachesUnfinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var detachedOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(detachedOrder, data.Part1, 10m);

			var finalisedOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(finalisedOrder, data.Part1, 10m);

			var pick = Helper.CreatePickNew(detachedOrder, finalisedOrder);
			Factory.Save();

			using (var form = new TestForm(pick))
			{
				form.Show();
				finalisedOrder.WD_FinalisedDate = ZDateTimeOffset.Now;

				var orderPksOnPick = pick.Orders.Select(o => o.PK);
				AssertEquals("Precondition: detachedOrder should be on pick", true, orderPksOnPick.Contains(detachedOrder.PK));
				AssertEquals("Precondition: finalisedOrder should be on pick", true, orderPksOnPick.Contains(finalisedOrder.PK));

				form.UserControl.InnerGrid.SelectAllElements();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.UserControl.DetachButton_ClickForTest(null, EventArgs.Empty);
				AssertEquals("Last Message was error.", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals($"Last Message shown has correct content", true, UnitTestUserNotification.Instance.LastMessage.Text.Contains("Warehouse Order W00000002: This order cannot be detached because it is finalized"));

				orderPksOnPick = pick.Orders.Select(o => o.PK);
				AssertEquals("finalisedOrder should still be on pick", true, orderPksOnPick.Contains(finalisedOrder.PK));
				AssertEquals("detachedOrder should NOT be on pick", false, orderPksOnPick.Contains(detachedOrder.PK));
			}
		}

		[ExpectNoExceptions]
		public void TestCanDetachUnfinalisedOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			AssertCanDetatchUnfinalisedOrder(order);
		}

		[ExpectNoExceptions]
		public void TestCanDetachUnfinalisedWorkOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateProductBOM(data.Part1, data.Part2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m);
			Factory.Save();

			var order = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "WO1", data.Part1, 1m);
			AssertCanDetatchUnfinalisedOrder(order);
		}

		[ExpectNoExceptions]
		public void TestCanDetachUnfinalisedDynamicWorkOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m);
			Factory.Save();

			var order = Helper.CreateWhsDynamicWorkOrderWithLine(data.Org1, data.Whs1, "WO1", data.Part1, 1m);
			var line = order.Lines[0];
			line.IsMainInwardProcessedItem = true;

			var componentLine = Helper.CreateWhsDynamicWorkOrderLine(order, data.Part2, 1m);
			componentLine.WE_WE_ParentDocketLine = line.PK;

			AssertCanDetatchUnfinalisedOrder(order);
		}

		void AssertCanDetatchUnfinalisedOrder(WhsPickableDocket order)
		{
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			using (var form = new TestForm(pick))
			{
				form.Show();
				order.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.UserControl.DetachButton_ClickForTest(null, EventArgs.Empty);
			}
		}

		public void TestDetach_UpdatesModuleID()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateProductBOM(data.Part1, data.Part2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m);
			Factory.Save();

			var order = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "WO1", data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			using (var form = new TestForm(pick))
			{
				form.Show();
				order.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				form.UserControl.ModuleID = ModuleIDs.WhsWorkOrder;

				form.UserControl.DetachButton_ClickForTest(null, EventArgs.Empty);

				AssertEquals(ModuleIDs.WhsOrder, form.UserControl.ModuleID);
			}
		}

		public void TestOnPickStatusChanged()
		{
			var pick = Factory.New<WhsPick>();
			using (var form = new TestForm(pick))
			{
				form.Show();
				AssertModuleButtons(form.UserControl, true);
				pick.WP_PickNo = "P1";
				pick.WP_PickStatus = PickStatus.Codes.Finalised;
				form.UserControl.ShowModuleButtons();
				AssertModuleButtons(form.UserControl, false);
			}
		}

		public void TestNewAndAttachButton_WhenPickAlreadyHasWorkOrders() => TestNewAndAttachButton_WhenPickAlreadyHasComponentOrders<WhsWorkOrder>();

		public void TestNewAndAttachButton_WhenPickAlreadyHasDynamicWorkOrders() => TestNewAndAttachButton_WhenPickAlreadyHasComponentOrders<WhsDynamicWorkOrder>();

		void TestNewAndAttachButton_WhenPickAlreadyHasComponentOrders<T>()
			where T : WhsComponentOrder
		{
			var pick = Factory.New<WhsPick>();
			var order = Factory.New<T>();
			pick.Orders.Add(order);

			using (var form = new TestForm(pick))
			{
				form.Show();
				form.UserControl.AttachButton_ClickForTest(null, EventArgs.Empty);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("Orders cannot be attached to a Pick that has Work Orders."));

				form.UserControl.NewButton_ClickForTest(null, EventArgs.Empty);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("Orders cannot be attached to a Pick that has Work Orders."));
			}
		}

		public void TestNewAndAttachButton_WhenPickAlreadyHasChildLineForBOMOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct1 = Helper.CreateProduct(data.Org1, "BOM1");
			var bomPartForMainProduct1 = Helper.CreateProductBOM(mainProduct, bomComponentProduct1, 1m, Enterprise.Core.Constants.PkgUnit.Unit);

			var bomComponentProduct2 = Helper.CreateProduct(data.Org1, "BOM2");
			var bomPartForMainProduct2 = Helper.CreateProductBOM(mainProduct, bomComponentProduct2, 3m, Enterprise.Core.Constants.PkgUnit.Unit);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 5m, inventoryLocation);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct1, 20m, inventoryLocation);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct2, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised", true, receive.IsFinalised);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine11 = Helper.CreateWhsOrderLine(order1, mainProduct, 10m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine21 = Helper.CreateWhsOrderLine(order2, mainProduct, 5m);

			var pick = Helper.CreatePickNew(new WhsPickableDocket[1] { order1 });
			Factory.Save();

			using (var form = new TestForm(pick))
			{
				form.Show();
				form.UserControl.AttachButton_ClickForTest(order2, EventArgs.Empty);
				AssertEquals(false, UnitTestUserNotification.Instance.LastMessage.WasError);
				form.UserControl.LastShownAttachPopupForTesting.Dispose();

				form.UserControl.NewButton_ClickForTest(order2, EventArgs.Empty);
				AssertEquals(false, UnitTestUserNotification.Instance.LastMessage.WasError);
				form.UserControl.LastShownZForm.Dispose();
			}
		}

		public void TestAttachButton_WhenPickHasNoChildLineForBOMOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct1 = Helper.CreateProduct(data.Org1, "BOM1");
			var bomPartForMainProduct1 = Helper.CreateProductBOM(mainProduct, bomComponentProduct1, 1m, Enterprise.Core.Constants.PkgUnit.Unit);

			var bomComponentProduct2 = Helper.CreateProduct(data.Org1, "BOM2");
			var bomPartForMainProduct2 = Helper.CreateProductBOM(mainProduct, bomComponentProduct2, 3m, Enterprise.Core.Constants.PkgUnit.Unit);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 5m, inventoryLocation);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct1, 20m, inventoryLocation);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct2, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised", true, receive.IsFinalised);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, mainProduct, 2m);
			Factory.Save();

			var pick = Helper.CreatePickNew(new WhsPickableDocket[1] { order1 });

			using (var form = new TestForm(pick))
			{
				var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
				var orderLine21 = Helper.CreateWhsOrderLine(order2, mainProduct, 5m);

				form.Show();
				form.UserControl.AttachButton_ClickForTest(order2, EventArgs.Empty);
				AssertEquals(true, !UnitTestUserNotification.Instance.LastMessage.Contains("This pick contains BOM products that have been setup so that the components are picked on the sales order. Additional orders cannot be attached."));
			}
		}

		public void TestEditButton_WithWorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var pick = Factory.New<WhsPick>();
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			Helper.CreateWhsWorkOrderLine(workOrder, data.Part1.PK, 10m);
			pick.Orders.Add(workOrder);

			using (var form = new TestForm(pick))
			{
				form.Show();
				Factory.Save(); // workOrder must exist in DB for the edit form to open it

				form.UserControl.EditButton_ClickForTest(null, EventArgs.Empty);
				var workOrderEditForm = (WorkOrderEntryForm)form.UserControl.ControllerForTest.LastShownForm;
				AssertEquals("Should open Edit Form for Work Order", typeof(WorkOrderEntryForm), workOrderEditForm.GetType());
				workOrderEditForm.Dispose();
			}
		}

		public void TestEditButton_WithDynamicWorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var pick = Factory.New<WhsPick>();
			var order = Helper.CreateWhsDynamicWorkOrderWithLine(data.Org1, data.Whs1, "WO1", data.Part1, 1m);
			pick.Orders.Add(order);

			using (var form = new TestForm(pick))
			{
				form.Show();
				Factory.Save();

				form.UserControl.EditButton_ClickForTest(null, EventArgs.Empty);
				var workOrderEditForm = (DynamicWorkOrderEntryForm)form.UserControl.ControllerForTest.LastShownForm;
				AssertEquals("Should open Edit Form for Work Order", typeof(DynamicWorkOrderEntryForm), workOrderEditForm.GetType());
				workOrderEditForm.Dispose();
			}
		}

		public void TestPickableDocketGridDoubleClick_WithWorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var pick = Factory.New<WhsPick>();
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			Helper.CreateWhsWorkOrderLine(workOrder, data.Part1.PK, 10m);
			pick.Orders.Add(workOrder);

			using (var form = new TestForm(pick))
			{
				form.Show();
				Factory.Save(); // workOrder must exist in DB for the edit form to open it

				form.UserControl.InnerGrid.PerformMouseDownForTest(0, 2); //invokes the OnMouseDown method with a numberOfClicks = 2
				var workOrderEditForm = (WorkOrderEntryForm)form.UserControl.ControllerForTest.LastShownForm;
				AssertEquals("Should open Edit Form for Dynamic Work Order", typeof(WorkOrderEntryForm), workOrderEditForm.GetType());
				workOrderEditForm.Dispose();
			}
		}

		public void TestPickableDocketGridDoubleClick_WithDynamicWorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var pick = Factory.New<WhsPick>();
			var order = Helper.CreateWhsDynamicWorkOrderWithLine(data.Org1, data.Whs1, "WO1", data.Part1, 1m);
			pick.Orders.Add(order);

			using (var form = new TestForm(pick))
			{
				form.Show();
				Factory.Save();

				form.UserControl.InnerGrid.PerformMouseDownForTest(0, 2); //invokes the OnMouseDown method with a numberOfClicks = 2
				var workOrderEditForm = (DynamicWorkOrderEntryForm)form.UserControl.ControllerForTest.LastShownForm;
				AssertEquals("Should open Edit Form for Dynamic Work Order", typeof(DynamicWorkOrderEntryForm), workOrderEditForm.GetType());
				workOrderEditForm.Dispose();
			}
		}

		#endregion

		#region ShowModuleButtons

		public void TestShowModuleButtons()
		{
			var pick = Factory.New<WhsPick>();
			using (var form = new TestForm(pick))
			{
				form.Show();

				var userControl = form.UserControl;

				pick.WP_PickNo = "";
				pick.WP_PickStatus = PickStatus.Codes.Created;
				userControl.ShowModuleButtons();
				AssertModuleButtons(userControl, true);

				pick.WP_PickNo = "P111";
				userControl.ShowModuleButtons();
				AssertModuleButtons(userControl, true);

				pick.WP_PickStatus = PickStatus.Codes.Building;
				userControl.ShowModuleButtons();
				AssertModuleButtons(userControl, true);

				pick.WP_PickStatus = PickStatus.Codes.PickSlip;
				userControl.ShowModuleButtons();
				AssertModuleButtons(userControl, true);

				var cartonisationMutex = new ZGlobalMutex(MutexIDs.WhsPickAllocatingPackageLabels, pick.PK.ToString());
				cartonisationMutex.Lock();

				using (cartonisationMutex)
				{
					AssertEquals("Precondition", true, pick.IsCartonising);
					userControl.ShowModuleButtons();
					AssertModuleButtons(userControl, false, true);
				}

				userControl.ShowModuleButtons();
				AssertModuleButtons(userControl, true);

				pick.WP_IsCartonised = true;
				userControl.ShowModuleButtons();
				AssertModuleButtons(userControl, false, true);

				pick.WP_IsCartonised = false;
				userControl.ShowModuleButtons();
				AssertModuleButtons(userControl, true);

				pick.WP_PickStatus = PickStatus.Codes.Finalised;
				userControl.ShowModuleButtons();
				AssertModuleButtons(userControl, false);

				pick.WP_PickStatus = PickStatus.Codes.Cancelled;
				userControl.ShowModuleButtons();
				AssertModuleButtons(userControl, false);
			}
		}

		#endregion

		#region TestShowAutoPickButton

		public void TestShowAutoPickButton()
		{
			using (var userControl = new PickOrdersModuleButtonGrid())
			{
				AssertEquals(true, userControl.ShowAutoPickButton);
				userControl.ShowAutoPickButton = false;
				AssertEquals(false, userControl.ShowAutoPickButton);
			}
		}

		#endregion

		#region TestCancelPickButton

		public void TestCancelPickButton()
		{
			using (var userControl = new PickOrdersModuleButtonGrid())
			{
				AssertEquals(true, userControl.ShowCancelPickButton);

				userControl.ShowCancelPickButton = false;
				AssertEquals(false, userControl.ShowCancelPickButton);
			}
		}

		public void TestCancelPickButton_ShowErroIfNotIsCancellable()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			var trolley = Helper.CreateTrolley("TR1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, "BLD");
			var package = order.PackageJob.Packages.AddNew();
			var trolleySlot = Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package.PK, 1);
			Factory.Save();

			using (var form = new TestForm(pick))
			{
				form.Show();
				AssertEquals("Pick should not be cancelled", false, pick.IsCancelled);

				form.UserControl.CancelPickButtonForTest.PerformClick();
				AssertEquals("Pick should not be cancelled", false, pick.IsCancelled);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("You cannot cancel pick because this pick has a package with an active Trolley job or was picked by Trolley."));

				trolleyJob.WTJ_Status = "FIN";
				Factory.Save();

				form.UserControl.CancelPickButtonForTest.PerformClick();
				AssertEquals("Pick should not be cancelled", false, pick.IsCancelled);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("You cannot cancel pick because this pick has a package with an active Trolley job or was picked by Trolley."));

				((BusinessObject)trolleySlot).Delete();
				Factory.Save();

				form.UserControl.CancelPickButtonForTest.PerformClick();
				AssertEquals("Pick should be cancelled", true, pick.IsCancelled);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(ReleaseEntryForm.PickCancelledConfirmationMsg));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
			}
		}

		public void TestCancelPickButton_ShowErrorIfNotIsCancellable_PickIsReadyForPlanning()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;

			using (var form = new TestForm(pick))
			{
				form.Show();
				form.UserControl.CancelPickButtonForTest.PerformClick();
				AssertEquals("Pick should not be cancelled", false, pick.IsCancelled);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("You cannot cancel pick because this pick is Ready For Planning or Planned."));

				pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;
				Factory.Save();

				form.UserControl.CancelPickButtonForTest.PerformClick();
				AssertEquals("Pick should be cancelled", true, pick.IsCancelled);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(ReleaseEntryForm.PickCancelledConfirmationMsg));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
			}
		}

		public void TestCancelPickButton_ShowErrorIfNotIsCancellable_PickIsPlanned()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;

			using (var form = new TestForm(pick))
			{
				form.Show();
				form.UserControl.CancelPickButtonForTest.PerformClick();
				AssertEquals("Pick should not be cancelled", false, pick.IsCancelled);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("You cannot cancel pick because this pick is Ready For Planning or Planned."));

				pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;
				Factory.Save();

				form.UserControl.CancelPickButtonForTest.PerformClick();
				AssertEquals("Pick should be cancelled", true, pick.IsCancelled);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(ReleaseEntryForm.PickCancelledConfirmationMsg));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
			}
		}

		public void TestCancelPickButton_DoesNotShowSuccessMessage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			var originalInventory = receive.Lines[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			var dockDoorTransferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			var dockDoorTransfer = dockDoorTransferLine.Docket;
			Factory.Save();

			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			ReleaseLineReductionManager.ReduceStock(releaseLine, 10m, CargoWise.Application.ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason.Lost);
			dockDoorTransfer.WD_WW_Whs = ZGuid.Empty; // Hack to put the job in an invalid state where transfer finalisation will fail

			using (var form = new TestForm(pick))
			{
				form.Show();
				AssertEquals("Pick should not be cancelled", false, pick.IsCancelled);

				form.UserControl.CancelPickButtonForTest.PerformClick();
				AssertEquals("Pick should *not* be cancelled", false, pick.IsCancelled);
				AssertEquals("Should *not* show a success message.", false, UnitTestUserNotification.Instance.LastMessage.Contains(ReleaseEntryForm.PickCancelledConfirmationMsg));
				AssertEquals("Should show generic error message.", true, UnitTestUserNotification.Instance.LastMessage.Contains("An error occurred during Cancellation."));
				AssertEquals("Should show generic error message.", true, UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		#endregion

		#region TestReleaseButton

		public void TestReleaseButton()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			using (var form = new TestForm(pick))
			{
				form.Show();
				form.UserControl.ShowModuleButtons();
				AssertNotNull(form.UserControl.ReleaseButtonForTest);
				Assert("Release button is visible.", form.UserControl.ShowReleaseButton);
			}
		}

		public void TestReleaseButton_WorkOrderPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m);
			Factory.Save();

			var order = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "WO1", data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			Assert("Precondition", pick.IsWorkOrderPick);
			using (var form = new TestForm(pick))
			{
				form.Show();
				form.UserControl.ShowModuleButtons();
				AssertNotNull(form.UserControl.ReleaseButtonForTest);
				Assert("Release button is not visible.", !form.UserControl.ShowReleaseButton);
			}
		}

		public void TestReleaseButton_DynamicWorkOrderPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m);
			Factory.Save();

			var order = Helper.CreateWhsDynamicWorkOrderWithLine(data.Org1, data.Whs1, "WO1", data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			Assert("Precondition", pick.IsWorkOrderPick);
			using (var form = new TestForm(pick))
			{
				form.Show();
				form.UserControl.ShowModuleButtons();
				AssertNotNull(form.UserControl.ReleaseButtonForTest);
				Assert("Release button is not visible.", !form.UserControl.ShowReleaseButton);
			}
		}

		public void TestReleaseButtonClick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			using (var form = new TestForm(pick))
			{
				form.Show();
				form.UserControl.ReleaseButtonForTest.PerformClick();

				var releaseForm = (ReleaseEntryForm)form.UserControl.ControllerForTest.LastShownForm;
				AssertNotNull("Release Form created", releaseForm);
				AssertEquals(true, releaseForm.Visible);
				AssertEquals(pick.PK, ((BusinessObject)releaseForm.BusinessEntity).PK);
				releaseForm.Dispose();
			}
		}

		public void TestReleaseButtonClick_SetCorrectInitialOrderToSelectInGrid()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order1, order2, order3);
			Factory.Save();

			using (var form = new TestForm(pick))
			{
				form.Show();

				var grid = form.UserControl.InnerGrid;
				grid.Select(1);

				form.UserControl.ReleaseButtonForTest.PerformClick();

				var releaseForm = (ReleaseEntryForm)form.UserControl.ControllerForTest.LastShownForm;
				AssertNotNull("Release Form created", releaseForm);
				AssertEquals(true, releaseForm.Visible);
				AssertEquals(order2.PK, releaseForm.CurrentOrder.PK);
				AssertEquals(pick.PK, ((BusinessObject)releaseForm.BusinessEntity).PK);
				releaseForm.Dispose();
			}
		}

		public void TestReleaseButtonClick_SetCorrectInitialOrderToSelectInGrid_MultipleOrdersSelected()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order1, order2, order3);
			Factory.Save();

			using (var form = new TestForm(pick))
			{
				form.Show();

				var grid = form.UserControl.InnerGrid;
				grid.Select(1);
				grid.Select(2);

				form.UserControl.ReleaseButtonForTest.PerformClick();

				var releaseForm = (ReleaseEntryForm)form.UserControl.ControllerForTest.LastShownForm;
				AssertNotNull("Release Form created", releaseForm);
				AssertEquals(true, releaseForm.Visible);
				// Should just fall back to default behaviour for selecting an order which does appear to be deterministic
				AssertEquals(pick.PK, ((BusinessObject)releaseForm.BusinessEntity).PK);
				releaseForm.Dispose();
			}
		}

		public void TestReleaseButtonClick_NoOrderAttached()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var pick = Factory.New<WhsPick>();
			using (var form = new TestForm(pick))
			{
				form.Show();
				AssertNoExceptionThrown("No exception should be thrown", () => form.UserControl.ReleaseButtonForTest.PerformClick());
				AssertEquals("Please attach an order to the Pick before going to the Release Form.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestReleaseButtonClick_PickWithChanges()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			pick.HasChanges = true;
			using (var form = new TestForm(pick))
			{
				form.Show();
				AssertNoExceptionThrown("No exception should be thrown", () => form.UserControl.ReleaseButtonForTest.PerformClick());
				AssertEquals("Please save this Pick before going to the Release Form.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Events

		public void TestAutoPickButton_Click()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var pick = Factory.New<WhsPick>();
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var line = Helper.CreateWhsOrderLine(order, data.Part1.PK, 10m);

			using (var form = new TestForm(pick))
			{
				form.Show();

				pick.WP_PickOption = WhsPickOption.Codes.Manual;
				form.UserControl.AutoPickButton.PerformClick();
				AssertEquals("You cannot auto pick because this pick has a Pick Option of Manual", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				pick.WP_PickOption = WhsPickOption.Codes.ManualWithAutoAllocate;
				form.UserControl.AutoPickButton.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("You cannot auto pick because this pick has a Pick Option of Manual"));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				pick.WP_PickOption = WhsPickOption.Codes.Auto;
				pick.Orders.Add(order);
				form.UserControl.AutoPickButton.PerformClick();
				AssertEquals(WhsPick.NoStockWarningMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAutoPickButton_Click_ShouldNotShowProgressFormWhenNotCartonization()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var pick = Factory.New<WhsPick>();
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var line = Helper.CreateWhsOrderLine(order, data.Part1.PK, 10m);

			using (var form = new TestForm(pick))
			{
				form.Show();

				pick.WP_PickOption = WhsPickOption.Codes.Auto;
				pick.Orders.Add(order);
				form.UserControl.AutoPickButton.PerformClick();

				var lastForm = ZFormModaliser.LastFormShownForTest;
				AssertNull("Should not show progress form.", lastForm);
			}
		}

		public void TestAutoPickButton_Click_ShowShowProgressFormWhenCartonization()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			Factory.Save();

			using (var form = new TestForm(pick))
			{
				form.Show();

				pick.WP_PickOption = WhsPickOption.Codes.Auto;
				pick.Orders.Add(order);
				form.UserControl.AutoPickButton.PerformClick();

				var lastForm = ZFormModaliser.LastFormShownForTest;
				AssertNotNull("Should have shown progress form.", lastForm);
				AssertType<ProgressForm>("Should have shown progress form.", lastForm);
				AssertEquals("Should have shown progress form.", true, lastForm.Enabled);

				var progressBarForm = (ProgressForm)lastForm;
				AssertEquals("Should have no progress bar.", false, progressBarForm.ShowProgressBar);
				AssertEquals("Should have no cancel button.", false, progressBarForm.ShowCancelButton);
				AssertEquals("Current Status is 'Allocating Package Labels...'.", "Allocating Package Labels...", progressBarForm.Status);
			}
		}

		#endregion

		#region TestUpdateFulfillmentRuleMenuItemState

		#region TestUpdateFulfillmentRuleMenuItemState_WithOrderThatCanBeOverriden

		public void TestUpdateFulfillmentRuleMenuItemState_WithOrderThatCanBeOverriden()
		{
			var pick = Factory.New<WhsPick>();
			var order = Factory.New<WhsOrder>();
			order.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			pick.Orders.Add(order);

			using (var frm = new TestForm(pick))
			{
				frm.Show();
				var grid = frm.UserControl.InnerGrid;
				grid.Select(0);

				var overrideItem = frm.FindOverrideFulfillmentRuleMenuItem();

				bool? overrideItemEnabled = null;

				grid.ContextMenu.Popup += delegate
				{
					overrideItemEnabled = overrideItem.Enabled;
					grid.ContextMenu.Dispose(); // cannot use SendKeys.Send("{ESC}") because it fails on DAT due to locked machine.
				};
				grid.ContextMenu.Show(grid, new Point(10, 10));

				AssertEquals(true, overrideItemEnabled);
			}
		}

		#endregion

		#region TestUpdateFulfillmentRuleMenuItemState_WithOrderThatCannotBeOverriden

		public void TestUpdateFulfillmentRuleMenuItemState_WithOrderThatCannotBeOverriden()
		{
			var pick = Factory.New<WhsPick>();
			var order = Factory.New<WhsOrder>();
			order.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.None;
			pick.Orders.Add(order);

			using (var frm = new TestForm(pick))
			{
				frm.Show();
				var grid = frm.UserControl.InnerGrid;
				grid.Select(0);

				var overrideItem = frm.FindOverrideFulfillmentRuleMenuItem();
				AssertEquals(true, overrideItem.Enabled);

				bool? overrideItemEnabled = null;

				grid.ContextMenu.Popup += delegate
				{
					overrideItemEnabled = overrideItem.Enabled;
					grid.ContextMenu.Dispose(); // cannot use SendKeys.Send("{ESC}") because it fails on DAT due to locked machine.
				};
				grid.ContextMenu.Show(grid, new Point(10, 10));

				AssertEquals(false, overrideItemEnabled);
			}
		}

		#endregion

		#region TestUpdateFulfillmentRuleMenuItemState_WithWorkOrder

		public void TestUpdateFulfillmentRuleMenuItemState_WithWorkOrder() => TestUpdateFulfillmentRuleMenuItemState_WithComponentOrder<WhsWorkOrder>();
		public void TestUpdateFulfillmentRuleMenuItemState_WithDynamicWorkOrder() => TestUpdateFulfillmentRuleMenuItemState_WithComponentOrder<WhsDynamicWorkOrder>();

		void TestUpdateFulfillmentRuleMenuItemState_WithComponentOrder<T>()
			where T : WhsComponentOrder
		{
			var pick = Factory.New<WhsPick>();
			var workOrder = Factory.New<T>();
			pick.Orders.Add(workOrder);

			using (var frm = new TestForm(pick))
			{
				frm.Show();
				var grid = frm.UserControl.InnerGrid;
				grid.Select(0);

				var overrideItem = frm.FindOverrideFulfillmentRuleMenuItem();
				AssertEquals(true, overrideItem.Enabled);

				bool? overrideItemEnabled = null;

				grid.ContextMenu.Popup += delegate
				{
					overrideItemEnabled = overrideItem.Enabled;
					grid.ContextMenu.Dispose(); // cannot use SendKeys.Send("{ESC}") because it fails on DAT due to locked machine.
				};
				grid.ContextMenu.Show(grid, new Point(10, 10));

				AssertEquals(false, overrideItemEnabled);
			}
		}

		#endregion

		#endregion

		#region TestOverrideFulfillmentRuleMenuItemClick

		#region TestOverrideFulfillmentRuleMenuItemClick_WithSelectedOrders

		public void TestOverrideFulfillmentRuleMenuItem_Click_WithSelectedOrders()
		{
			var order1 = Factory.New<WhsOrder>();
			order1.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;

			var order2 = Factory.New<WhsOrder>();
			order2.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;

			var order3 = Factory.New<WhsOrder>();
			order3.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;

			var pick = Factory.New<WhsPick>();
			pick.Orders.AddRange(new WhsPickableDocket[] { order1, order2, order3 });

			using (var frm = new TestForm(pick))
			{
				frm.Show();
				var grd = frm.UserControl.InnerGrid;
				grd.Select(0);
				grd.Select(1);

				var overrideItem = frm.FindOverrideFulfillmentRuleMenuItem();
				overrideItem.PerformClick();

				AssertEquals(WhsOrderFulfillmentRuleList.Codes.None, order1.WD_WhsOrderFulfillmentRule);
				AssertEquals(WhsOrderFulfillmentRuleList.Codes.None, order2.WD_WhsOrderFulfillmentRule);
				AssertEquals(WhsOrderFulfillmentRuleList.Codes.All, order3.WD_WhsOrderFulfillmentRule);
			}
		}

		#endregion

		#region TestOverrideFulfillmentRuleMenuItemClick_WithNoSelectedOrders

		public void TestOverrideFulfillmentRuleMenuItemClick_WithNoSelectedOrders()
		{
			var pick = Factory.New<WhsPick>();
			using (var frm = new TestForm(pick))
			{
				frm.Show();
				var grd = frm.UserControl.InnerGrid;

				var overrideItem = frm.FindOverrideFulfillmentRuleMenuItem();
				overrideItem.PerformClick();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("Please select at least one Order before Overriding Fulfillment Rules."));
			}
		}

		#endregion

		#endregion

		#region TestOverrideNewButton_Click

		public void TestOverrideNewButton_Click()
		{
			var pick = Helper.CreatePickNew();
			Factory.Save();

			using (var form = new TestForm(pick))
			{
				form.Show();
				form.UserControl.NewButton_ClickForTest(null, EventArgs.Empty);

				using (var newOrderForm = form.UserControl.LastShownZForm)
				{
					var orderOnNewOrderForm = (WhsOrder)newOrderForm.BusinessEntityForPersistingForm;
					AssertEquals("When the user click New to create a new Order on the Pick form, the new order should have NEW status.", DocketStatus.Codes.New, orderOnNewOrderForm.WD_DocketStatus);
					AssertEquals("When the user click New to create a new Order on the Pick form, the new order should not be picking.", false, orderOnNewOrderForm.IsAttachedToPickButNotFinalised);
					AssertEquals("When the user click New to create a new Order on the Pick form, the new order should not be picked.", false, orderOnNewOrderForm.IsAttachedToPick);
				}
			}
		}

		#endregion

		#region TestTotals

		public void TestTotals_Initial()
		{
			var (data, pick) = GetTestDataForTotals();

			using (var form = new TestForm(pick))
			{
				form.Show();

				AssertEquals("NumberOfOrders should be 3.", 3, pick.NumberOfOrders);
				AssertEquals("TotalWeight should be 12 KG.", 12m, pick.TotalWeight);
				AssertEquals("TotalVolume should be 12 M3.", 12m, pick.TotalVolume);
				AssertEquals("TotalLineQty should be 12.", 12m, pick.TotalLineQty);
				AssertEquals("TotalComponentQty should be 0.", 0m, pick.TotalComponentQty);
			}
		}

		public void TestTotals_RecalculateAfter_NewOrder()
		{
			var (data, pick) = GetTestDataForTotals();

			using (var form = new TestForm(pick))
			{
				form.Show();

				var newOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part2, 5m, WhsPickOption.Codes.Manual);

				form.UserControl.OrderForNewTest = newOrder;
				form.UserControl.NewButton_ClickForTest(null, EventArgs.Empty);
				using (var orderForm = form.UserControl.LastShownZForm as OrderEntryForm)
				{
					var result = orderForm.FireSaveButton();
					AssertEquals("Precondition: New order should be saved", ContinueWithSave.Yes, result);
				}

				AssertEquals("NumberOfOrders should be 4 after adding 1 order.", 4, pick.NumberOfOrders);
				AssertEquals("TotalWeight should still be 12 KG, new order is not allocated.", 12m, pick.TotalWeight);
				AssertEquals("TotalVolume should be 12 M3, new order is not allocated.", 12m, pick.TotalVolume);
				AssertEquals("TotalLineQty should be 12, new order is not allocated.", 12m, pick.TotalLineQty);
				AssertEquals("TotalComponentQty should be 0.", 0m, pick.TotalComponentQty);

				pick.AutoAllocateItemsWithMock();
				AssertEquals("NumberOfOrders should be 4", 4, pick.NumberOfOrders);
				AssertEquals("TotalWeight should be 12.5 KG after allocate.", 12.5m, pick.TotalWeight);
				AssertEquals("TotalVolume should be 12.05 M3 after allocate.", 12.05m, pick.TotalVolume);
				AssertEquals("TotalLineQty should be 17 after allocate.", 17m, pick.TotalLineQty);
				AssertEquals("TotalComponentQty should be 0.", 0m, pick.TotalComponentQty);
			}
		}

		public void TestTotals_RecalculateAfter_EditOrder()
		{
			PackingRegistry.Instance.WeightUnit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "KG");
			PackingRegistry.Instance.VolumeUnit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "M3");
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Weight = 2;
			data.Part1.OP_WeightUQ = "KG";
			data.Part1.OP_Cubic = 1;
			data.Part1.OP_CubicUQ = "M3";

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 3m, WhsPickOption.Codes.Manual);
			var pick = Helper.CreatePickNew(order1);
			pick.AutoAllocateItemsWithMock();
			Factory.Save();

			using (var form = new TestForm(pick))
			{
				form.Show();

				AssertEquals("Precondition: NumberOfOrders should be 1.", 1, pick.NumberOfOrders);
				AssertEquals("Precondition: TotalWeight should be 6 KG.", 6m, pick.TotalWeight);
				AssertEquals("Precondition: TotalVolume should be 3 M3.", 3m, pick.TotalVolume);
				AssertEquals("Precondition: TotalLineQty should be 3.", 3m, pick.TotalLineQty);
				AssertEquals("Precondition: TotalComponentQty should be 0.", 0m, pick.TotalComponentQty);

				form.UserControl.EditButton_ClickForTest(null, EventArgs.Empty);
				using (var orderForm = form.UserControl.LastShownZForm as OrderEntryForm)
				{
					var order = (WhsOrder)orderForm.BusinessEntity;
					order.Lines[0].WE_TransactionQuantity = 2;
					orderForm.FireSaveButton();
					orderForm.Close();
				}

				AssertEquals("NumberOfOrders should still be 1 after editing order.", 1, pick.NumberOfOrders);
				AssertEquals("TotalWeight should be 4 KG after editing order.", 4m, pick.TotalWeight);
				AssertEquals("TotalVolume should be 2 M3 after editing order.", 2m, pick.TotalVolume);
				AssertEquals("TotalLineQty should be 2 after editing order.", 2m, pick.TotalLineQty);
				AssertEquals("TotalComponentQty should be 0.", 0m, pick.TotalComponentQty);

				form.UserControl.EditButton_ClickForTest(null, EventArgs.Empty);
				using (var orderForm = form.UserControl.LastShownZForm as OrderEntryForm)
				{
					var order = (WhsOrder)orderForm.BusinessEntity;
					order.Lines[0].WE_TransactionQuantity = 1;
					orderForm.FireSaveButton();
					orderForm.Close();
				}

				AssertEquals("NumberOfOrders should still be 1 after editing order.", 1, pick.NumberOfOrders);
				AssertEquals("TotalWeight should be 2 KG after editing order.", 2m, pick.TotalWeight);
				AssertEquals("TotalVolume should be 1 M3 after editing order.", 1m, pick.TotalVolume);
				AssertEquals("TotalLineQty should be 1 after editing order.", 1m, pick.TotalLineQty);
				AssertEquals("TotalComponentQty should be 0.", 0m, pick.TotalComponentQty);
			}
		}

		public void TestTotals_RecalculateAfter_DetachOrder()
		{
			var (data, pick) = GetTestDataForTotals();

			using (var form = new TestForm(pick))
			{
				form.Show();
				form.UserControl.InnerGrid.Select(0);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.UserControl.DetachButton_ClickForTest(null, EventArgs.Empty);
			}

			AssertEquals("NumberOfOrders should be 2 after removing 1 order.", 2, pick.NumberOfOrders);
			AssertEquals("TotalWeight should be 9 KG after removing 1 order.", 9m, pick.TotalWeight);
			AssertEquals("TotalVolume should be 9 M3 after removing 1 order.", 9m, pick.TotalVolume);
			AssertEquals("TotalLineQty should be 9 after removing 1 order.", 9m, pick.TotalLineQty);
			AssertEquals("TotalComponentQty should be 0.", 0m, pick.TotalComponentQty);
		}

		public void TestTotals_RecalculateAfter_DetachOrders()
		{
			var (data, pick) = GetTestDataForTotals();

			using (var form = new TestForm(pick))
			{
				form.Show();
				form.UserControl.InnerGrid.SelectElements(new BusinessObject[] { pick.Orders[0], pick.Orders[1] });
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.UserControl.DetachButton_ClickForTest(null, EventArgs.Empty);
			}

			AssertEquals("NumberOfOrders should be 1 after removing 2 orders.", 1, pick.NumberOfOrders);
			AssertEquals("TotalWeight should be 5 KG after removing 2 orders.", 5m, pick.TotalWeight);
			AssertEquals("TotalVolume should be 5 M3 after removing 2 orders.", 5m, pick.TotalVolume);
			AssertEquals("TotalLineQty should be 5 after removing 2 orders.", 5m, pick.TotalLineQty);
			AssertEquals("TotalComponentQty should be 0.", 0m, pick.TotalComponentQty);
		}

		(TestDataSimpleEnvironment, WhsPick) GetTestDataForTotals()
		{
			PackingRegistry.Instance.WeightUnit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "KG");
			PackingRegistry.Instance.VolumeUnit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "M3");
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 1, "KG", 1, "M3");
			Helper.SetProductWeightAndVolume(data.Part2, 100, "G", 10, "D3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 3m, WhsPickOption.Codes.Manual);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 4m, WhsPickOption.Codes.Manual);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 5m, WhsPickOption.Codes.Manual);
			var pick = Helper.CreatePickNew(order1, order2, order3);
			pick.AutoAllocateItemsWithMock();
			Factory.Save();

			return (data, pick);
		}

		#endregion

		#region Implementation

		void AssertModuleButtons(PickOrdersModuleButtonGrid userControl, bool visible, bool? detachVisible = null)
		{
			AssertEquals("Orders Module NEW button has incorrect visibility", visible, userControl.ShowNewButton);
			AssertEquals("Orders Module EDIT button has incorrect visibility", visible, userControl.ShowEditButton);
			AssertEquals("Orders Module ATTACH button has incorrect visibility", visible, userControl.ShowAttachButton);
			AssertEquals("Orders Module DETACH button has incorrect visibility", (detachVisible ?? visible), userControl.ShowDetachButton);
			AssertEquals("Orders Module AutoPick button has incorrect visibility", visible, userControl.ShowAutoPickButton);
		}

		internal class TestForm : ZForm, INotifications
		{
			public TestForm(WhsPick pick)
				: base(pick)
			{
				pick.SetCartonizationProgressForm((status) => { return PickManager.GetCartonizationProgressForm(status, this); });
			}
			public PickOrdersModuleButtonGrid UserControl;
			public PickOrdersUserControl PickOrdersUserControl;

			public List<WhsPickableDocketLine> SelectedOrderLines => throw new NotImplementedException();

			public OverrideFulfillmentRuleMenuItem FindOverrideFulfillmentRuleMenuItem()
			{
				foreach (var item in UserControl.InnerGrid.ContextMenu.MenuItems)
				{
					var overrideItem = item as OverrideFulfillmentRuleMenuItem;
					if (overrideItem != null)
					{
						return overrideItem;
					}
				}
				throw new Exception("Override Fulfillment Rule menu item was not found in the Grid's Context Menu");
			}

			protected override void InitializeComponent()
			{
				this.UserControl = new PickOrdersModuleButtonGrid();
				this.UserControl.InnerGrid.BindTo = "Orders";
				this.UserControl.BindToGridList = "Orders";
				this.UserControl.BindToFindBoxList = "Lookups+OrderFindBoxList";
				this.UserControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.WhsOrder;
				this.Controls.Add(this.UserControl);

				this.PickOrdersUserControl = new PickOrdersUserControl();
				this.Controls.Add(this.PickOrdersUserControl);

				ZTextBoxColumnStyleInfo column = new ZTextBoxColumnStyleInfo();
				column.Caption = "DocketID";
				column.ColumnName = "WD_DocketID";
				this.UserControl.InnerGrid.ColumnStyles.Add(column);

				this.DataSourceAssemblyName = "Enterprise.Warehouse.Transactions.Business";
				this.DataSourceTypeName = "Enterprise.Warehouse.Transactions.Business.WhsPick";
			}

			#region INotifications Members

			void INotifications.Add(INotification notification)
			{
			}

			#endregion
		}

		#endregion
	}
}
