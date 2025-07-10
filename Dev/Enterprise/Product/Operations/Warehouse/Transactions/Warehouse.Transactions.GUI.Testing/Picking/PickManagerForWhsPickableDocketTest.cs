using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration.Customs.PermitService;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Printing;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class PickManagerForWhsPickableDocketTest : WhsTestCaseWithFactory
	{
		#region TestPickOrders_WhenDocumentPackCannotPrint

		public void TestPickOrders_WhenDocumentPackCannotPrint()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderWarehouseDocumentsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			//Unpublish the DocBuilder Pick Documents Pack
			CargoWise.Database.TestFramework.ObjectModel.StmMenuItem.UpdateWhere(Guid.Parse("15F5C2FC-0CEC-4D4F-8818-461592DD97BE"))
					.Set(l => l.SU_IsPublished, false)
					.Post(TestConnection);

			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order Ref #1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				var manager = new PickManagerForWhsPickableDocket(form, order);
				AssertEquals("Precondition", true, UnitTestUserNotification.Instance.LastMessage.WasNone);

				manager.PickOrders();
				AssertEquals("The Order should be picked.", true, order.IsAttachedToPickButNotFinalised);
				AssertEquals("The Order.Pick should be saved to the DB.", true, order.Pick.IsInDatabase);
				AssertEquals("Error: Unable to find Document to print. Please make sure the 'Pick Documents Pack' Document is published.", UnitTestUserNotification.Instance.LastMessage.Text.Trim());
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		#endregion

		#region TestPickOrders_CreatesNewPickInstance

		/// <summary>
		/// Without this fix, the user could:
		/// 
		///		Open a picked non-finalised order.
		///		Click Pick.
		///		Make some changes, eg "Cancel Pick" button.
		///		Click Cancel to return to the Order without saving the Pick.
		///		Click Pick again -- the previous changes that were cancelled still exist (they were not thrown away).
		///
		/// </summary>
		public void TestPickOrders_CreatesNewPickInstance()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order Ref #1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				var manager = new PickManagerForWhsPickableDocket(form, order);

				manager.PickOrders();
				var pickForm1 = (PickEntryForm)manager.LastUsedPickControllerForTest.LastShownForm;
				var pick1 = pickForm1.Pick;
				pickForm1.Close(); // otherwise the next call to PickOrders() will just set focus to the existing form!
				AssertNotEquals("Just making sure the ZArchitecture is doing it's job and loading the BizO in a new factory.", order.Pick, pick1);

				manager.PickOrders();
				var pickForm2 = (PickEntryForm)manager.LastUsedPickControllerForTest.LastShownForm;
				var pick2 = pickForm2.Pick;
				pickForm2.Close();

				AssertNotEquals("The Pick was reused. Any cancelled changes to the first pick will be retained the next time the user clicks the Pick button.", pick1, pick2);
				AssertEquals(PickType.Codes.Order, pick2.WP_PickType);
			}
		}

		#endregion

		#region TestPickOrders_EnsuresDocketSaved

		public void TestPickOrders_EnsuresDocketSaved()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order Ref #1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				var manager = new PickManagerForWhsPickableDocket(form, order);
				AssertEquals("Precondition", true, UnitTestUserNotification.Instance.LastMessage.WasNone);

				manager.PickOrders();
				AssertEquals("Order should not be picked.", false, order.IsAttachedToPickButNotFinalised);
				AssertNull("Order should have no Pick attached.", order.Pick);
				AssertEquals("Please save this Order before Picking it.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region TestPickOrders_WhenAutoPick_OrderDeleted

		public void TestPickOrders_WhenAutoPick_OrderDeleted()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 50m);
			Factory.Save();

			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				var manager = new PickManagerForWhsPickableDocket(form, order);
				AssertEquals("Precondition", true, UnitTestUserNotification.Instance.LastMessage.WasNone);

				var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
				var order_InOtherFactory = factory2.Load<WhsOrder>(order.PK);
				order_InOtherFactory.Delete();
				factory2.Save();

				AssertNoExceptionThrown(() => manager.PickOrders());
				AssertEquals("Order has been deleted by another user.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Form should be (force) closed to prevent further exceptions.", form.IsDisposed);
			}
		}

		#endregion

		#region TestPickOrders_WhenAutoPick_UnsavedEmptyOrder

		public void TestPickOrders_WhenAutoPick_UnsavedEmptyOrder()
		{
			var order = Factory.New<WhsOrder>();

			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				var manager = new PickManagerForWhsPickableDocket(form, order);
				AssertEquals("Precondition", true, UnitTestUserNotification.Instance.LastMessage.WasNone);

				AssertNoExceptionThrown(() => manager.PickOrders());
				AssertEquals("Please save this Order before Picking it.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Form should not be closed, user should be able to start to enter Order details.", !form.IsDisposed);
			}
		}

		#endregion

		#region TestPickOrders_WhenAutoPickAndStockAllocated

		public void TestPickOrders_WhenAutoPickAndStockAllocated()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order Ref #1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				var manager = new PickManagerForWhsPickableDocket(form, order);
				AssertEquals("Precondition", true, UnitTestUserNotification.Instance.LastMessage.WasNone);

				manager.PickOrders();
				AssertEquals("The Order should be picked.", true, order.IsAttachedToPickButNotFinalised);
				AssertEquals("The Order.Pick should be saved to the DB.", true, order.Pick.IsInDatabase);
				AssertEquals("Pick " + order.Pick.WP_PickNo + " has been created.", UnitTestUserNotification.Instance.LastMessage.Text);

				var printer = new WhsPickDocumentsAutoPrinter(order.Pick, new TestNotificationBuffer());
				AssertEquals(printer.DocumentMenuName, WhsDocumentPrinter.LastPrintedDocumentName);
			}
		}

		#endregion

		#region TestPickOrders_WhenAutoPickAndStockAllocated_WithFulfillmentRuleNotMet

		public void TestPickOrders_WhenAutoPickAndStockAllocated_WithFulfillmentRuleNotMet()
		{
			WhsDocumentPrinter.LastPrintedDocumentName = "";
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order Ref #1");
			order.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			Helper.CreateWhsOrderLine(order, data.Part1, 120m); // only 100 in stock
			Factory.Save();

			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				var manager = new PickManagerForWhsPickableDocket(form, order);
				AssertEquals("Precondition", true, UnitTestUserNotification.Instance.LastMessage.WasNone);

				manager.PickOrders();
				AssertEquals("The Order should be picked.", true, order.IsAttachedToPickButNotFinalised);
				AssertEquals("The Order.Pick should be saved to the DB.", true, order.Pick.IsInDatabase);
				AssertEquals("Pick " + order.Pick.WP_PickNo + " has been created." + pickBuildingMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				var printer = new WhsPickDocumentsAutoPrinter(order.Pick, new TestNotificationBuffer());
				AssertEquals("", WhsDocumentPrinter.LastPrintedDocumentName);

				var pickForm = (PickEntryForm)manager.LastUsedPickControllerForTest.LastShownForm;
				AssertNotNull("Pick form should be displayed", pickForm);
			}
		}

		const string pickBuildingMessage =
				"\r\n\r\nThis Pick has been created with a status of 'BUILDING'.\r\n" +
				"The Fulfillment Rules for all Orders on the Pick have not been met.\r\n" +
				"Pick Documentation cannot be printed until the Fulfillment Rules have been satisfied or overridden.";

		#endregion

		#region TestPickOrders_WhenAutoPickAndNoStockAllocated

		public void TestPickOrders_WhenAutoPickAndNoStockAllocated()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order Ref #1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				var manager = new PickManagerForWhsPickableDocket(form, order);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				WhsDocumentPrinter.LastPrintedDocumentName = "";

				manager.PickOrders();
				AssertEquals("The Order should be picked.", true, order.IsAttachedToPickButNotFinalised);
				AssertEquals("Stock was not allocated, the Order.Pick should not be saved to the DB.", false, order.Pick.IsInDatabase);
				AssertEquals(WhsPick.NoStockWarningMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Stock was not allocated, should not have printed any Pick Docs.", "", WhsDocumentPrinter.LastPrintedDocumentName);
			}
		}

		#endregion

		#region TestPickOrders_AutoPickWhenPartiallyPicked

		public void TestPickOrders_AutoPickWhenPartiallyPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Factory.Save();
			Helper.CreatePickNew_WithoutAllocationEngineMock(order);

			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				var manager = new PickManagerForWhsPickableDocket(form, order);
				manager.PickOrders();

				using (var pickForm = manager.LastUsedPickControllerForTest.LastShownForm)
				{
					AssertEquals("When it is in order page it should open pick page.", true, pickForm is PickEntryForm);
				}
			}
		}

		#endregion

		#region TestPickOrders_WhenJobAlreadyPicked

		public void TestPickOrders_WhenJobAlreadyPicked()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order Ref #1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);
			Factory.Save(); // must save Inventory data before picking

			pick.PickOrders();

			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				var manager = new PickManagerForWhsPickableDocket(form, order);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				manager.PickOrders();
				AssertEquals("The Order was already picked and should not have changes.", false, order.HasChanges);
				AssertEquals("The Pick was already created/picked and should not have changes.", false, pick.HasChanges);
				AssertEquals("No messages should be shown when opening an existing Pick.", true, UnitTestUserNotification.Instance.LastMessage.WasNone);

				var pickForm = (PickEntryForm)manager.LastUsedPickControllerForTest.LastShownForm;
				var pickFormTabControl = (ZTemplateTabControl)pickForm.PickSlipTabPage.Parent;
				AssertEquals("When picking an existing job the pick form should be opened at the PickSlip tab.", pickForm.PickSlipTabPage, pickFormTabControl.SelectedTab);
			}
		}

		#endregion

		#region TestPickOrders_WhenConcurrentUsersTryToCreatePicks

		public void TestPickOrders_WhenOneUserGeneratesPickAfterSecondUserLoadedOrderForm()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			// create order and save it in one factory
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", data.Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			// load the order in another factory
			var differentFactory = new BusinessObjectFactory();
			differentFactory.RefreshEnabled = false;
			var orderLoadedInDifferentFactory = differentFactory.Load<WhsOrder>(order.PK);

			// create the pick in one pick manager instance
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				var manager = new PickManagerForWhsPickableDocket(form, order);
				manager.PickOrders();
				AssertNotNull("Pick should be created.", order.Pick);
				AssertEquals("Pick should be saved to the database.", true, order.Pick.IsInDatabase);
			}

			// try to create the pick in another pick manager instance
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			using (var form = new OrderEntryForm(orderLoadedInDifferentFactory, new NotificationSubscriberGuiHelper()))
			{
				var manager = new PickManagerForWhsPickableDocket(form, orderLoadedInDifferentFactory);
				AssertNoExceptionThrown("No exception should be thrown when pick button is clicked when another user has already created a pick while working on the order.", () => manager.PickOrders());
				AssertEquals("Loaded pick should not have any changes.", false, orderLoadedInDifferentFactory.Pick.HasChanges);
				AssertEquals("Picks should be same in both factories.", order.Pick.PK, orderLoadedInDifferentFactory.Pick.PK);
			}
		}

		#endregion

		#region TestPickOrders_WhenOneUserDeletesPickAfterSecondUserLoadedOrderForm

		public void TestPickOrders_WhenOneUserCancelsPickAfterSecondUserLoadedPickFormFromOrderForm()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			// create order and save it in one factory
			var order = Helper.CreateWhsOrder(data.Org1.PK, data.Whs1.PK, data.Org1.PK, "O1", ZDateTimeOffset.Now, data.Notify, pickOption: "MAN");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				// create the pick in the pick manager instance
				var manager = new PickManagerForWhsPickableDocket(form, order);
				manager.PickOrders();
				var firstPick = order.Pick;

				// Simulate second User Deleting pick while first user has form open
				order.WD_WP = ZGuid.Empty;
				order.WD_DocketStatus = DocketStatus.Codes.Entered;
				Factory.Save();
				AssertNoExceptionThrown(() => manager.PickOrders()); // Clicking Pick again should not throw exception.
				AssertNotEquals("Should create a new pick", firstPick, order.Pick);
			}
		}

		#endregion

		#region TestPickOrdersForWaitingReplenishPicks

		#region TestPickOrdersForWaitingReplenishPicks_AutoPrintPickSlipAndAllowPicksToBeSetToWaitingReplenishment

		public void TestPickOrdersForWaitingReplenishmentPicks_AutoPrintPickSlipAndAllowPicksToBeSetToWaitingReplenishment()
		{
			AssertPickOrdersForWaitingReplenishmentPicks(true);
		}

		#endregion

		#region TestPickOrdersForWaitingReplenishmentPicks_NoAutoPrintPickSlipAndAllowPicksToBeSetToWaitingReplenishment

		public void TestPickOrdersForWaitingReplenishmentPicks_NoAutoPrintPickSlipAndAllowPicksToBeSetToWaitingReplenishment()
		{
			AssertPickOrdersForWaitingReplenishmentPicks(false);
		}

		#endregion

		void AssertPickOrdersForWaitingReplenishmentPicks(bool autoPrintDocuments)
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			data.Whs1.WW_AutoPrintPickingSlip = autoPrintDocuments;
			data.Whs1.WW_AutoPrintOrderSummaryOnPick = autoPrintDocuments;
			data.Whs1.WW_AutoPrintOrderCopyForMOPOnPick = autoPrintDocuments;
			data.Whs1.WW_AutoPrintPickingNonPickedItems = autoPrintDocuments;
			data.Whs1.WW_AutoPrintPickingShortfallItems = autoPrintDocuments;

			var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
			AllocationRulesHelper.AddFullPalletRule(ruleSet, 5);
			AllocationRulesHelper.AddPickFaceRules(ruleSet, 10);
			AllocationRulesHelper.AddFifoRule(ruleSet, 20, preventPickingPickFacesFromBulk: true);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Pallet, 2m); // Part1 : 1PLT = 2 UNT

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocationWithOldItems = data.Whs1.FindLocation("A-2");
			var bulkLocationWithNewItems = data.Whs1.FindLocation("A-3");
			var bulkLocationForPartWithEnoughStock = data.Whs1.FindLocation("A-4");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Today.AddDays(-1), data.Part1, 3m, pickFaceLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Today.AddDays(-4), data.Part1, 2m, bulkLocationWithOldItems, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", ZDateTimeOffset.Today.AddDays(-1), data.Part1, 1m, bulkLocationWithNewItems, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", ZDateTimeOffset.Today.AddDays(-1), data.Part2, 1m, bulkLocationForPartWithEnoughStock, "");

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			Helper.CreateWhsOrderLine(order, data.Part1, 6m); // This will short fall since there is a pick face and can't pick the full pallet
			Helper.CreateWhsOrderLine(order, data.Part2, 1m);
			Factory.Save();

			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				var manager = new PickManagerForWhsPickableDocket(form, order);
				AssertEquals("Precondition", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				WhsDocumentPrinter.LastPrintedDocumentName = "";

				manager.PickOrders();
				AssertEquals("The Order should be picked eventhough the pick is waiting for replenishment.", true, order.IsAttachedToPickButNotFinalised);
				AssertEquals(true, order.Pick.WP_IsAwaitingReplenishment);
				AssertEquals("Pick " + order.Pick.WP_PickNo + " has been created and is awaiting replenishment.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasWarning);

				AssertEquals("Pick slip should not be printed.", "", WhsDocumentPrinter.LastPrintedDocumentName);
			}
		}

		#endregion

		#region TestCreatePick_IsPermitAvailable

		[TestDate(2017, 9, 4)]
		public void TestCreatePick_IsPermitAvailable()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS();
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs, "R1", data.Part1, 5m, whs.DefaultLocation, "");

			var order = Helper.CreateWhsOrder(data.Org1, whs);
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
			var orderLineWithPermitErrors = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			orderLineWithPermitErrors.CustomsData.WB_CustomsQty = 50m;
			orderLineWithPermitErrors.CustomsData.WB_Tariff = "1020304050";
			Factory.Save();

			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				var permitService = new Mock<IPermitService>();
				var responseForIsAvailablePermits = new WhsPermitWithdrawRequestResponseForTest(orderLineWithPermitErrors, SuccessOrFailure.Failure, 0m, null, null);
				permitService.Setup(m => m.IsPermitAvailable(It.IsAny<IEnumerable<IPermitWithdrawRequest>>())).
					Returns(new WhsPermitWithdrawRequestResponseResultForTest() { Responses = new IPermitWithdrawRequestResponse[] { responseForIsAvailablePermits } });
				var manager = new PickManagerForWhsPickableDocket(form, order);
				AssertEquals("Precondition", true, UnitTestUserNotification.Instance.LastMessage.WasNone);

				using (ObjectFactory.Substitute(permitService.Object))
				{
					using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
					{
						manager.PickOrders();
					}

					AssertEquals("Not every Order Line on this Pick could be matched to a weekly estimate. Errors below:\r\nNo weekly estimate found for Order Line 1 (5 UNT of Product P1) (Date: 04-Sep-17, ).",
					UnitTestUserNotification.Instance.LastMessage.Text);
				}

				permitService.Verify(m => m.TryGetPermits(It.IsAny<IEnumerable<IPermitWithdrawRequest>>()), Times.Never);
				permitService.Verify(m => m.RelinquishPermitTransactions(It.IsAny<IEnumerable<IPermitTransactionDetail>>()), Times.Never);
				permitService.VerifyAll();
				AssertEquals("The Order must not be picked.", false, order.IsAttachedToPickButNotFinalised);
				AssertHasRowWarning("Order Has Warning.", order, "Not all Order Lines could be granted a weekly estimate for this Order.");
				AssertHasRowWarning("OrderLine Has Warning.", orderLineWithPermitErrors, "No weekly estimate found for Order Line 1 (5 UNT of Product P1) (Date: 04-Sep-17, ).");
			}
		}

		#endregion

		#region TestCreatePick_OrderLocked

		public void TestCreatePick_OrderLocked()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			// create order and save it in one factory
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", data.Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			var mutex = new WhsPickCreationMutex(order);
			mutex.Lock();
			AssertEquals("Precondition", true, mutex.HasLock);

			try
			{
				using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
				{
					var manager = new PickManagerForWhsPickableDocket(form, order);
					manager.PickOrders();
					AssertNull("Pick should not be created.", order.Pick);
					AssertEquals("Should have displayed a dialog", "Error " + GlbStaff.CurrentUser.GS_FullName + " is currently in the process of creating a new Pick for this Order. Try again later.", UnitTestUserNotification.Instance.LastMessage.ToString());
				}
			}
			finally
			{
				mutex.Unlock();
			}
		}

		#endregion

		#region TestCreatePick_OrderCreatedWhileUserVerifiesNoPickAndBeforeGrantingTheLock

		public void TestCreatePick_OrderCreatedWhileUserVerifiesNoPickAndBeforeGrantingTheLock()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			// create order and save it in one factory
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", data.Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				var manager = new PickManagerForWhsPickableDocket(form, order);
				manager.CreateNewPick = () =>
				{
					var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
					Factory.Save();
				};

				using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
				{
					manager.PickOrders();
				}

				AssertNotNull("Pick should be should be loaded which was created by another user.", order.Pick);
				AssertEquals("Pick should be in the database.", true, order.Pick.IsInDatabase);
				AssertEquals("No messages should be shown to the user.", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		#endregion
	}
}
