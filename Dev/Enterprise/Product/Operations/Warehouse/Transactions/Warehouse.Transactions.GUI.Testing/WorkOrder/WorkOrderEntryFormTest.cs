using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	[TestedType(typeof(WorkOrderEntryFormForTest))]
	public class WorkOrderEntryFormTest : ZFormBasherTest
	{
		#region TestConstructor

		public void TestConstructor()
		{
			using (var form = new WorkOrderEntryForm(Factory.New<WhsWorkOrder>(), new NotificationSubscriberGuiHelper()))
			{
				AssertNotNull(form.ActionsMenuItem.MenuItems.FindByText("Create all Work Orders by their Bills of Materials", false));

				// find the BOM menu item
				MenuItem bomMenuItem = null;
				foreach (MenuItem menuItem in form.ActionsMenuItem.MenuItems)
				{
					if (menuItem.GetType() == typeof(BOMMenuItem))
					{
						bomMenuItem = menuItem;
						break;
					}
				}

				AssertNotNull(bomMenuItem);
			}
			AssertExceptionThrown<ArgumentNullException>("Exception should be thrown if null INotificationSubscriberQueryUser is passed in.",
				() => new WorkOrderEntryForm(Factory.New<WhsWorkOrder>(), null));
		}

		#endregion

		#region TestFinaliseButtonIsDisabledForNonPickedWorkOrder

		public void TestFinaliseButtonIsDisabledForNonPickedWorkOrder()
		{
			using (var form = (WorkOrderEntryFormForTest)GetFormToBashCore())
			{
				var data = new TestDataForBOM(Factory);
				var dummyClient = Factory.NewWithValidTestData<OrgHeader>();
				var dummyWarehouse = Factory.NewWithValidTestData<WhsWarehouse>();
				Docket.WD_OH_Client = dummyClient.PK;
				Docket.WD_WW_Whs = dummyWarehouse.PK;
				data.CreateBOMComponentsInInventory(saveFactoryForWarehouse: true);

				Docket.WD_OH_Client = data.Org1.PK;
				Docket.WD_WW_Whs = data.Whs1.PK;
				Docket.WD_RequiredDate = ZDateTimeOffset.Now;
				Docket.ConsigneeAddressPK = data.Org1.MainAddress.PK;
				Factory.Save();

				form.Show();
				AssertEquals("Docket has no Pick, Finalise Button should be enabled.", false, form.FinalizeButton.Enabled);

				var pick = Factory.New<WhsPick>();
				var line = Helper.CreateWhsWorkOrderLine(Docket, data.BOM.Bike.PK, 10);

				pick.PickOrders(new WhsWorkOrder[] { Docket });
				AssertEquals("Precondition", true, Docket.IsAttachedToPickButNotFinalised);
				AssertEquals(true, form.FinalizeButton.Enabled);

				Docket.WD_FinalisedDate = ZDateTimeOffset.Now;
				AssertEquals("Docket is already finalised, Finalise Button should be disabled.", false, form.FinalizeButton.Enabled);
			}
		}

		#endregion

		#region TestFinaliseAlsoFinalisesPick

		public void TestFinaliseAlsoFinalisesPick()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMComponentsInInventory();
			Factory.Save(); // shortfall check will hit the db

			using (var form = (WorkOrderEntryFormForTest)GetFormToBashCore())
			{
				form.Show();

				Docket.WD_OH_Client = data.Org1.PK;
				Docket.WD_WW_Whs = data.Whs1.PK;
				Docket.WD_RequiredDate = ZDateTimeOffset.Now;
				Docket.ConsigneeAddressPK = data.Org1.MainAddress.PK;
				var line = Helper.CreateWhsWorkOrderLine(Docket, data.BOM.Bike, 10m);

				var pick = Factory.New<WhsPick>();
				pick.PickOrders(Docket);
				AssertEquals("Precondition", true, Docket.IsAttachedToPickButNotFinalised);
				AssertEquals("Precondition", false, pick.IsFinalised);

				form.FinalizeButton.PerformClick();
				AssertEquals(true, pick.IsFinalised);
			}
		}

		#endregion

		#region TestFinaliseDocket_CreatesReceive_ShowsReceiveFormOnSave

		public void TestFinaliseDocket_CreatesReceive_ShowsReceiveFormOnSave()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMComponentsInInventory();
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.BOM.Bike, 10m);
			Helper.CreatePickNew(workOrder);

			using (var form = new WorkOrderEntryForm(workOrder, new NotificationSubscriberGuiHelper()))
			{
				workOrder.FinaliseDocketAlwaysFinalisingPick();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(workOrder);

				var receiveQuery = new ZQuery();
				receiveQuery.AddToFilter(WhsDocketSchema.WD_WD_ParentDocket, workOrder.PK);
				receiveQuery.AddToFilter(WhsDocketSchema.WD_DocketType, DocketType.Codes.Receive);
				var receive = Factory.LoadTop1<WhsReceive>(receiveQuery);

				var controller = ZControllerFactory.Create(ControllerIDs.WhsReceive);
				AssertNull("Precondition - Receive Controller should not be created till work order get saved to the DB.", controller.GetOpenedForm(receive));

				workOrder.Factory.Save();

				var lastShownForm1 = (ZForm)controller.GetOpenedForm(receive);
				AssertEquals(ControllerIDs.WhsReceive, lastShownForm1.ControllerID);
				AssertEquals(receive.PK, ((WhsReceive)lastShownForm1.BusinessEntity).PK);
				lastShownForm1.Dispose();

				workOrder.WD_TotalCubic = 5m;
				AssertNoExceptionThrown(() => workOrder.Factory.Save());

				var lastShownForm2 = (ZForm)controller.GetOpenedForm(receive);
				AssertEquals(ControllerIDs.WhsReceive, lastShownForm2.ControllerID);
				AssertEquals("There should be only one related job created.", receive.PK, ((WhsReceive)workOrder.RelatedJobs.Single()).PK);
				AssertEquals(receive.PK, ((WhsReceive)((ReceiveEntryForm)lastShownForm2).BusinessEntity).PK);
				lastShownForm2.Dispose();
			}
		}

		#endregion

		#region TestFinalizeWorkorder_Concurrency

		public void TestFinalizeWorkorder_Concurrency()
		{
			var whs = Helper.CreateWarehouse("whs", "A", 2, 1);
			var location1 = whs.FindLocation("A-1");
			var location2 = whs.FindLocation("A-2");
			var client = Helper.CreateClient();
			var partFinished = Helper.CreateProduct(client, "finished");
			var partRaw = Helper.CreateProduct(client, "raw");
			// 1 partFinished made from 1 partRaw
			Helper.CreateProductBOM(partFinished, partRaw, 1m, Constants.PkgUnit.Unit);
			Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(client, whs, "R1", partFinished, 1m, location1, "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(client, whs, "R2", partFinished, 10m, location2, "");
			var receiveline1 = receive1.Lines[0];
			var receiveline2 = receive2.Lines[0];
			AssertEquals("Precondition", 1m, receiveline1.AvailableToPickQuantity);
			AssertEquals("Precondition", 10m, receiveline2.AvailableToPickQuantity);

			// hold line 
			receiveline2.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			receiveline2.HeldCodeChangeQuantity = 10;
			receiveline2.ChangeInventoryHeldCode(true);
			AssertEquals("Precondition", 1m, receiveline1.AvailableToPickQuantity);
			AssertEquals("Precondition", 0m, receiveline2.AvailableToPickQuantity);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(client, whs, "W1", WorkOrderType.Codes.Disassemble);
			Helper.CreateWhsWorkOrderLine(workOrder, partFinished, 1m);
			Helper.CreatePickNew(workOrder);
			Factory.Save();

			AssertEquals("Precondition - Pick failed.", true, workOrder.IsAttachedToPickButNotFinalised);

			using (var form = new WorkOrderEntryForm(workOrder, new NotificationSubscriberGuiHelper()))
			{
				OpenPickFormAssertHasOneAvailableLine(workOrder.Pick);
				ChangeInventoryStatusToAvailableInNewFactory(receive2.Lines[0]);
				OpenPickFormAndChangeAllocationToSecondAvailableLine(workOrder.Pick.PK);

				workOrder.FinaliseDocketAlwaysFinalisingPick();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(workOrder);
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(workOrder.Pick);

				var controller = ZControllerFactory.Create(ControllerIDs.WhsReceive);
				AssertNull("Precondition - Receive Controller should not be created till work order get saved to the DB.", controller.GetOpenedForm(workOrder.Receive));
				form.FireSaveButton();
				AssertStartsWith("The concurrency resolver dialog should be shown.", "While you have been working with this form, another user has made changes.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			var newFactory = new BusinessObjectFactory { RefreshEnabled = true };
			var inventory1 = newFactory.Load<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WD, receive1.PK)).Single();
			var inventory2 = newFactory.Load<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WD, receive2.PK)).Single();
			AssertEquals("Original allocated inventory should not be modified.", 1m, inventory1.WE_StockOnHand);
			AssertEquals("Should not reduce by finalise.", 10m, inventory2.WE_StockOnHand);
		}

		void OpenPickFormAssertHasOneAvailableLine(WhsPick pick)
		{
			using (var form = new PickEntryForm(pick, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals("Precondition: Should be 1 item", 1, pick.OrderedInventories[0].AvailableInventories.Count);
				pick.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>().Single(l => l.QuantityAvailableToPick == 1m && l.PickLineQuantity == 1m);
			}
		}

		void ChangeInventoryStatusToAvailableInNewFactory(WhsReceiveLine receiveLine)
		{
			var newFactory = new BusinessObjectFactory { RefreshEnabled = true };
			var receivelineInNewFactory = newFactory.Load<WhsReceiveLine>(receiveLine.PK);
			receivelineInNewFactory.HeldCodeToChangeTo = ""; // InventoryStatus.Codes.Available;
			receivelineInNewFactory.HeldCodeChangeQuantity = 10;
			receivelineInNewFactory.ChangeInventoryHeldCode(true);
			newFactory.Save();

			AssertEquals("After changing status stock should become available.", 10m, receivelineInNewFactory.AvailableToPickQuantity);
		}

		void OpenPickFormAndChangeAllocationToSecondAvailableLine(ZGuid pickPK)
		{
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var pickInNewFactory = newFactory.Load<WhsPick>(pickPK);
			using (var form = new PickEntryForm(pickInNewFactory, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				var orderedInventories = pickInNewFactory.OrderedInventories[0];
				var availableInventories = orderedInventories.AvailableInventories.Cast<WhsPickAvailableInventory>();

				availableInventories.Single(l => l.QuantityAvailableToPick == 1m).Allocate = false;
				availableInventories.Single(l => l.QuantityAvailableToPick == 10m).Allocate = true;
				newFactory.Save();
				availableInventories.Single(l => l.QuantityAvailableToPick == 1m && l.PickLineQuantity == 0m);
				availableInventories.Single(l => l.QuantityAvailableToPick == 10m && l.PickLineQuantity == 1m);
			}
		}

		#endregion

		#region TestCreatePickAfterChangingAllProductDefinitions

		public void TestCreatePickAfterChangingAllProductDefinitions()
		{
			// create bom product and inventory
			var data = new TestDataSimpleEnvironment(Factory);
			var bomProduct = data.Part1;
			var componentProduct = Helper.CreateProduct(data.Org1, "C1");
			var bomPart = Helper.CreateProductBOM(bomProduct, componentProduct, 1m, Constants.PkgUnit.Unit);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", componentProduct, 1m, true, true);
			Factory.Save();

			// create the work order and change component product pack type
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1.PK, data.Whs1.PK, "W1", WorkOrderType.Codes.Assemble);
			Helper.CreateWhsWorkOrderLine(workOrder, bomProduct, 1m);
			bomPart.OE_F3_NKPackType = Constants.PkgUnit.Carton;
			Factory.Save();

			using (var form = new WorkOrderEntryFormForTest(workOrder, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertNoExceptionThrown("No exception should be thrown when generating the pick.", () => form.PickButton.PerformClick());
				AssertEquals(string.Format("One of the component products has been changed. Please cancel the work order {0} and recreate it.", workOrder.WD_ExternalReference), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region TestCreatePickAfterChangingFewProductDefinitions

		public void TestCreatePickAfterChangingFewProductDefinitions()
		{
			// create bom products and inventory
			var data = new TestDataSimpleEnvironment(Factory);
			var bomProduct = data.Part1;
			var componentProduct1 = Helper.CreateProduct(data.Org1, "C1");
			var componentProduct2 = Helper.CreateProduct(data.Org1, "C2");
			var bomPart1 = Helper.CreateProductBOM(bomProduct, componentProduct1, 1m, Constants.PkgUnit.Unit);
			var bomPart2 = Helper.CreateProductBOM(bomProduct, componentProduct2, 1m, Constants.PkgUnit.Unit);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "W1", new TestNotificationBuffer());
			Helper.CreateWhsReceiveInventoryLine(receive, componentProduct1, 1m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, componentProduct2, 1m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();
			Factory.Save();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			// create work order and change one bom product pack type
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "W1", WorkOrderType.Codes.Assemble);
			Helper.CreateWhsWorkOrderLine(workOrder, bomProduct, 1m);
			bomPart2.OE_F3_NKPackType = Constants.PkgUnit.Carton;
			Factory.Save();

			using (var form = new WorkOrderEntryFormForTest(workOrder, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertNoExceptionThrown("No exception should be thrown when generating the pick", () => form.PickButton.PerformClick());
				AssertEquals(string.Format("One of the component products has been changed. Please cancel the work order {0} and recreate it.", workOrder.WD_ExternalReference), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region TestCreatingPickForWorkorderWithUnitConversions

		public void TestCreatingPickForWorkorderWithUnitConversions()
		{
			var warehouse = Helper.CreateWarehouse("WH1", "A", 1, 1);
			var client = Helper.CreateClient("O1");
			var parentProduct = CreateProductWithoutUnitConversions(client, "BOM");
			var childProduct = CreateProductWithoutUnitConversions(client, "CHD");
			Helper.CreateProductUnit(parentProduct, Constants.PkgUnit.Unit, Constants.PkgUnit.Carton, 4);
			Helper.CreateProductUnit(parentProduct, Constants.PkgUnit.Carton, Constants.PkgUnit.Pallet, 10);
			Helper.CreateProductUnit(childProduct, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 100);

			Helper.CreateProductBOM(parentProduct, childProduct, 1, Constants.PkgUnit.Unit);

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", childProduct, 100);
			Factory.Save();

			// create the work order and pick
			var workOrder = Helper.CreateWhsWorkOrder(client, warehouse, "W1");
			var line = Helper.CreateWhsWorkOrderLine(workOrder, parentProduct, 2m, "PLT");
			Factory.Save();

			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			using (var form = new WorkOrderEntryFormForTest(workOrder, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				form.PickButton.PerformClick();
				AssertEquals("Precondition", true, workOrder.IsAttachedToPickButNotFinalised);
				AssertEquals("Sum of units met on the work order online should be 80", 80m, line.SumOfUnitsMet);
			}
		}

		OrgSupplierPart CreateProductWithoutUnitConversions(OrgHeader owner, string code)
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = code;
			part.OP_Desc = code;
			part.OP_StockKeepingUnit = Core.Constants.PkgUnit.Unit;
			Helper.CreateProductClientRelationShip(owner, part, OrgPartRelation.RelationshipTypes.Owner);
			return part;
		}

		#endregion

		#region TestLoadWorkOrderFormAfterPickingAndChangingProductDefinitions

		public void TestLoadWorkOrderFormAfterPickingAndChangingProductDefinitions()
		{
			// create bom product and inventory
			var data = new TestDataSimpleEnvironment(Factory);
			var bomProduct = data.Part1;
			var componentProduct1 = Helper.CreateProduct(data.Org1, "C1");
			var componentProduct2 = Helper.CreateProduct(data.Org1, "C2");
			var bomPart1 = Helper.CreateProductBOM(bomProduct, componentProduct1, 1m, Constants.PkgUnit.Unit);
			var bomPart2 = Helper.CreateProductBOM(bomProduct, componentProduct2, 1m, Constants.PkgUnit.Unit);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "W1", new TestNotificationBuffer());
			Helper.CreateWhsReceiveInventoryLine(receive, componentProduct1, 1m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, componentProduct2, 1m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();
			Factory.Save();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			// create work order and pick
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			Helper.CreateWhsWorkOrderLine(workOrder, bomProduct, 1m);
			Factory.Save();
			var pick = Helper.CreatePickNew(workOrder);
			AssertNotNull("Precondition", pick);
			AssertEquals("Precondition", true, workOrder.IsAttachedToPickButNotFinalised);

			// change bom product pack type
			bomPart2.OE_F3_NKPackType = Constants.PkgUnit.Carton;
			Factory.Save();

			using (var form = new WorkOrderEntryFormForTest(workOrder, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertNoExceptionThrown(() => form.WorkOrderEntryControl.DocketLinesGridControl.Refresh());
				AssertNoExceptionThrown("No exception should be thrown when generating the pick", () => form.PickButton.PerformClick());
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		#endregion

		#region TestHandleSaveException_ShowsMessageWhenTriggersFail

		public void TestHandleSaveException_ShowsMessageWhenTriggersFail()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProductsInInventory();
			data.CreateBOMComponentsInInventory();
			Factory.Save();

			var helper = new WhsTestHelperFunctions(Factory);
			var order = helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.BOM.Bike, 7m);
			GUITestHelper.AssertHandleSaveException_ShowsMessageWhenTriggersFail(() => new WorkOrderEntryForm(order, new NotificationSubscriberGuiHelper()));
		}

		#endregion

		#region TestIsInwardProcessingJobCheckBoxVisible

		public void TestIsInwardsProcessingJobCheckBoxVisible_InwardProcessingSupported()
		{
			TestIsInwardProcessingJobCheckBoxVisible(supportsInwardProcessing: true);
		}

		public void TestIsInwardsProcessingJobCheckBoxVisible_InwardProcessingNotSupported()
		{
			TestIsInwardProcessingJobCheckBoxVisible(supportsInwardProcessing: false);
		}

		void TestIsInwardProcessingJobCheckBoxVisible(bool supportsInwardProcessing)
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProductsInInventory();
			data.CreateBOMComponentsInInventory();
			data.Whs1.WarehouseAddress.OA_RN_NKCountryCode = "FR";
			Factory.Save();

			var helper = new WhsTestHelperFunctions(Factory);
			var workOrder = helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.BOM.Bike, 7m);

			var mock = new Mock<Enterprise.Integration.Customs.ISupportedForProcessing>();
			mock.Setup(m => m.IsSupportedForProcessing()).Returns(supportsInwardProcessing);

			using (ObjectFactory.Substitute(mock.Object))
			{
				using (var form = new WorkOrderEntryFormForTest(workOrder, new NotificationSubscriberGuiHelper()))
				{
					form.Show();

					var control = form.WorkOrderEntryControl;
					var cs = control.Controls.OfType<ZCheckBox>().ToArray();
					var inwardProcessingCheckBox = control.Find(c => c is ZCheckBox checkBox && checkBox.BindTo == nameof(WhsWorkOrder.WD_IsInwardsProcessingJob)).Cast<ZCheckBox>().Single();
					AssertNotNull("Precondition", inwardProcessingCheckBox);
					AssertEquals(supportsInwardProcessing, inwardProcessingCheckBox.Visible);
				}
			}
		}

		public void TestIsInwardProcessingJobCheckBoxVisible_WarehouseChanged()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProductsInInventory();
			data.CreateBOMComponentsInInventory();
			data.Whs1.WarehouseAddress.OA_RN_NKCountryCode = "AU";

			var warehouse2 = Helper.CreateWarehouse("WH2", "A", 1, 1);
			warehouse2.WarehouseAddress.OA_RN_NKCountryCode = "FR";
			Factory.Save();

			var helper = new WhsTestHelperFunctions(Factory);
			var workOrder = helper.CreateWhsWorkOrderWithLine(data.Org1, warehouse2, data.BOM.Bike, 7m);

			var mock = new Mock<Enterprise.Integration.Customs.ISupportedForProcessing>();
			mock.Setup(m => m.IsSupportedForProcessing()).Returns(true);

			using (ObjectFactory.Substitute(mock.Object))
			{
				using (var form = new WorkOrderEntryFormForTest(workOrder, new NotificationSubscriberGuiHelper()))
				{
					form.Show();

					var control = form.WorkOrderEntryControl;
					var cs = control.Controls.OfType<ZCheckBox>().ToArray();
					var inwardProcessingCheckBox = control.Find(c => c is ZCheckBox checkBox && checkBox.BindTo == nameof(WhsWorkOrder.WD_IsInwardsProcessingJob)).Cast<ZCheckBox>().Single();
					AssertNotNull("Precondition", inwardProcessingCheckBox);
					AssertEquals(true, inwardProcessingCheckBox.Visible);

					workOrder.WD_WW_Whs = data.Whs1.PK;
					AssertEquals(true, inwardProcessingCheckBox.Visible);

					workOrder.WD_WW_Whs = warehouse2.PK;
					AssertEquals(true, inwardProcessingCheckBox.Visible);
				}
			}
		}

		#endregion

		#region INotifications Members

		public void TestQueryUser()
		{
			var workOrder = Factory.New<WhsWorkOrder>();
			var helperMock = new Mock<INotificationSubscriberQueryUser>();
			using (var form = new WorkOrderEntryForm(workOrder, helperMock.Object))
			{
				GUITestHelper.VerifyQueryUserMethodInNotificationSubscriberGuiHelperIsBeingCalled(form, helperMock);
			}
		}

		#endregion

		#region TestINotificationSubscriberQueryUser

		public void TestINotificationSubscriberQueryUser()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2, 2m, "UNT");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.Part1, 5m);
			Helper.CreatePickNew(workOrder);

			using (var form = new WorkOrderEntryFormForTest(workOrder, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var args = new QueryUserYesNoEventArgs("cap", "mess", false);
				form.QueryUser(args);
				AssertEquals("User Response should update args.", true, args.Response);

				var lastMessage = UnitTestUserNotification.Instance.LastMessage;
				AssertEquals("Caption should be correct.", "cap", lastMessage.Caption);
				AssertEquals("Message should be correct.", "mess", lastMessage.Text);
				AssertEquals("Notification should be question.", true, lastMessage.WasQuestion);
				AssertEquals("Answer should be correct.", ZDialogResult.Yes, lastMessage.Answer);
			}
		}

		public void TestINotificationSubscriberQueryUser_OkCancelArgs_WithNoReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2, 2m, "UNT");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.Part1, 5m);
			Helper.CreatePickNew(workOrder);

			using (var form = new WorkOrderEntryFormForTest(workOrder, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.ShowDialogsInTest = true;
				var args = new AssemblyConfirmationQueryUserEventArgs(false);
				form.QueryUser(args);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("User Response should not have updated.", false, args.Response);
			}
		}

		public void TestINotificationSubscriberQueryUser_OkCancelArgs_WithReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2, 2m, "UNT");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.Part1, 5m);
			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(workOrder);

			using (var form = new WorkOrderEntryFormForTest(workOrder, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.ShowDialogsInTest = true;
				var args = new AssemblyConfirmationQueryUserEventArgs(false);
				form.QueryUser(args);
				AssertEquals("User Response should update args.", true, args.Response);
				AssertType<WhsWorkOrderAssemblyConfirmationDialog>("Form should have shown.", ZFormModaliser.LastFormShownDialogForTest);
				ZFormModaliser.LastFormShownDialogForTest = null;
			}
		}

		public void TestINotificationSubscriberQueryUser_OkCancelArgs_WithReceive_Cancelled()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2, 2m, "UNT");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.Part1, 5m);
			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(workOrder);

			using (var form = new WorkOrderEntryFormForTest(workOrder, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				ZFormModaliser.ShowDialogsInTest = true;
				var args = new AssemblyConfirmationQueryUserEventArgs(true);
				form.QueryUser(args);
				AssertEquals("User Response should update args.", false, args.Response);
				AssertType<WhsWorkOrderAssemblyConfirmationDialog>("Form should have shown.", ZFormModaliser.LastFormShownDialogForTest);
				ZFormModaliser.LastFormShownDialogForTest = null;
			}
		}

		public void TestINotificationSubscriberQueryUser_OkCancelArgs_WithReceive_ValidationSuspended()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2, 2m, "UNT");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.Part1, 5m);
			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(workOrder);

			using (var form = new WorkOrderEntryFormForTest(workOrder, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				ZFormModaliser.ShowDialogsInTest = true;
				var validationWasSuspended = false;
				using (ZFormModaliser.SetTemporaryDelegateToCallBeforeShowingFormsOrDialogs(f => validationWasSuspended = Factory.IsValidationSuspended))
				{
					var args = new AssemblyConfirmationQueryUserEventArgs(true);
					form.QueryUser(args);
					AssertEquals("User Response should update args.", false, args.Response);
					AssertEquals("Factory Validation should be suspended while showing the Dialog.", true, validationWasSuspended);
					AssertEquals("Factory Validation should be no longer be suspended after showing the Dialog.", false, Factory.IsValidationSuspended);
					AssertType<WhsWorkOrderAssemblyConfirmationDialog>("Form should have shown.", ZFormModaliser.LastFormShownDialogForTest);
					ZFormModaliser.LastFormShownDialogForTest = null;
				}
			}
		}

		#endregion

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var result = new WorkOrderEntryFormForTest(Docket, new NotificationSubscriberGuiHelper());
			result.ControllerID = ControllerIDs.WhsWorkOrder;
			return result;
		}

		protected WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		WhsWorkOrder Docket => docket ?? (docket = Factory.New<WhsWorkOrder>());
		WhsWorkOrder docket;

		#region class WorkOrderEntryFormForTest

		class WorkOrderEntryFormForTest : WorkOrderEntryForm
		{
			public WorkOrderEntryFormForTest(WhsWorkOrder docket, NotificationSubscriberGuiHelper whsNotificationSubscriberGuiHelper)
				: base(docket, whsNotificationSubscriberGuiHelper)
			{
			}

			public new ZButton PickButton => base.PickButton;

			public new ZButton FinalizeButton => base.FinalizeButton;

			public new WorkOrderEntryUserControl WorkOrderEntryControl => base.WorkOrderEntryControl;
		}

		#endregion

		#endregion
	}
}
