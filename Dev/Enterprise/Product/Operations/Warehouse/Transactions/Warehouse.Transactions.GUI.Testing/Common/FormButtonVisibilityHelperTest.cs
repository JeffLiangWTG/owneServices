using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.GUI.Common;
using Moq;

namespace Warehouse.Transactions.GUI.Testing
{
	public class FormButtonVisibilityHelperTest : WhsTestCaseWithFactory
	{
		#region TestShowButton_Created

		void TestShowButton_Created_Core(Func<WhsPick, bool> visibilityMethodUnderTest, bool expectedResult)
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", data.Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			order.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.None;
			pick.UpdatePickStatusIfRequired();

			AssertEquals("Precondition.", PickStatus.Codes.Created, pick.WP_PickStatus);
			AssertEquals($"{visibilityMethodUnderTest.Method.Name} should return {expectedResult} after pick creation", expectedResult, visibilityMethodUnderTest(pick));
		}

		public void TestShowAttachButton_Created()
		{
			TestShowButton_Created_Core(FormButtonVisibilityHelper.ShowAttachButton, true);
		}

		public void TestShowDetachButton_Created()
		{
			TestShowButton_Created_Core(FormButtonVisibilityHelper.ShowDetachButton, true);
		}
		public void TestShowEditButton_Created()
		{
			TestShowButton_Created_Core(FormButtonVisibilityHelper.ShowEditButton, true);
		}
		public void TestShowNewButton_Created()
		{
			TestShowButton_Created_Core(FormButtonVisibilityHelper.ShowNewButton, true);
		}
		public void TestShowAutoPickButton_Created()
		{
			TestShowButton_Created_Core(FormButtonVisibilityHelper.ShowAutoPickButton, true);
		}

		public void TestShowCancelPickButton_Created()
		{
			TestShowButton_Created_Core(FormButtonVisibilityHelper.ShowCancelPickButton, true);
		}

		public void TestShowSelectAllButton_Created()
		{
			TestShowButton_Created_Core(FormButtonVisibilityHelper.ShowSelectAllButton, true);
		}

		public void TestShowClearAllButton_Created()
		{
			TestShowButton_Created_Core(FormButtonVisibilityHelper.ShowClearAllButton, true);
		}

		public void TestShowReleaseButton_Created()
		{
			TestShowButton_Created_Core(FormButtonVisibilityHelper.ShowReleaseButton, true);
		}

		#endregion

		#region TestShowButton_Building

		void TestShowButton_Building_Core(Func<WhsPick, bool> visibilityMethodUnderTest, bool expectedResult)
		{
			var pick = Helper.CreatePickNew();
			AssertEquals("Precondition", PickStatus.Codes.Building, pick.WP_PickStatus);
			AssertEquals($"{visibilityMethodUnderTest.Method.Name} should return {expectedResult} when pick is being built.", expectedResult, visibilityMethodUnderTest(pick));
		}

		public void TestShowAttachButton_Building()
		{
			TestShowButton_Building_Core(FormButtonVisibilityHelper.ShowAttachButton, true);
		}

		public void TestShowDetachButton_Building()
		{
			TestShowButton_Building_Core(FormButtonVisibilityHelper.ShowDetachButton, true);
		}

		public void TestShowEditButton_Building()
		{
			TestShowButton_Building_Core(FormButtonVisibilityHelper.ShowEditButton, true);
		}

		public void TestShowNewButton_Building()
		{
			TestShowButton_Building_Core(FormButtonVisibilityHelper.ShowNewButton, true);
		}

		public void TestShowAutoPickButton_Building()
		{
			TestShowButton_Building_Core(FormButtonVisibilityHelper.ShowAutoPickButton, true);
		}

		public void TestShowCancelPickButton_Building()
		{
			TestShowButton_Building_Core(FormButtonVisibilityHelper.ShowCancelPickButton, true);
		}

		public void TestShowSelectAllButton_Building()
		{
			TestShowButton_Building_Core(FormButtonVisibilityHelper.ShowSelectAllButton, true);
		}

		public void TestShowClearAllButton_Building()
		{
			TestShowButton_Building_Core(FormButtonVisibilityHelper.ShowClearAllButton, true);
		}

		public void TestShowReleaseButton_Building()
		{
			TestShowButton_Building_Core(FormButtonVisibilityHelper.ShowReleaseButton, true);
		}

		#endregion

		#region TestShowButton_Cancelled

		void TestShowButton_Cancelled_Core(Func<WhsPick, bool> visibilityMethodUnderTest, bool expectedResult)
		{
			var pick = Helper.CreatePickNew();
			pick.CancelPick();
			AssertEquals("Precondition.", PickStatus.Codes.Cancelled, pick.WP_PickStatus);
			AssertEquals($"{visibilityMethodUnderTest.Method.Name} should return {expectedResult} when pick is cancelled.", expectedResult, visibilityMethodUnderTest(pick));
		}

		public void TestShowAttachButton_Cancelled()
		{
			TestShowButton_Cancelled_Core(FormButtonVisibilityHelper.ShowAttachButton, false);
		}

		public void TestShowDetachButton_Cancelled()
		{
			TestShowButton_Cancelled_Core(FormButtonVisibilityHelper.ShowDetachButton, false);
		}

		public void TestShowEditButton_Cancelled()
		{
			TestShowButton_Cancelled_Core(FormButtonVisibilityHelper.ShowEditButton, false);
		}

		public void TestShowNewButton_Cancelled()
		{
			TestShowButton_Cancelled_Core(FormButtonVisibilityHelper.ShowNewButton, false);
		}

		public void TestShowAutoPickButton_Cancelled()
		{
			TestShowButton_Cancelled_Core(FormButtonVisibilityHelper.ShowAutoPickButton, false);
		}

		public void TestShowCancelPickButton_Cancelled()
		{
			TestShowButton_Cancelled_Core(FormButtonVisibilityHelper.ShowCancelPickButton, false);
		}

		public void TestShowSelectAllButton_Cancelled()
		{
			TestShowButton_Cancelled_Core(FormButtonVisibilityHelper.ShowSelectAllButton, false);
		}

		public void TestShowClearAllButton_Cancelled()
		{
			TestShowButton_Cancelled_Core(FormButtonVisibilityHelper.ShowClearAllButton, false);
		}

		public void TestShowReleaseButton_Cancelled()
		{
			TestShowButton_Cancelled_Core(FormButtonVisibilityHelper.ShowReleaseButton, true);
		}

		#endregion

		#region TestShowButton_Finalised

		void TestShowButton_Finalised_Core(Func<WhsPick, bool> visibilityMethodUnderTest, bool expectedResult)
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", data.Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertEquals("Precondition", PickStatus.Codes.Finalised, pick.WP_PickStatus);
			AssertEquals($"{visibilityMethodUnderTest.Method.Name} should return {expectedResult} when pick is finalised.", expectedResult, visibilityMethodUnderTest(pick));
		}

		public void TestShowAttachButton_Finalised()
		{
			TestShowButton_Finalised_Core(FormButtonVisibilityHelper.ShowAttachButton, false);
		}

		public void TestShowDetachButton_Finalised()
		{
			TestShowButton_Finalised_Core(FormButtonVisibilityHelper.ShowDetachButton, false);
		}

		public void TestShowEditButton_Finalised()
		{
			TestShowButton_Finalised_Core(FormButtonVisibilityHelper.ShowEditButton, false);
		}

		public void TestShowNewButton_Finalised()
		{
			TestShowButton_Finalised_Core(FormButtonVisibilityHelper.ShowNewButton, false);
		}

		public void TestShowAutoPickButton_Finalised()
		{
			TestShowButton_Finalised_Core(FormButtonVisibilityHelper.ShowAutoPickButton, false);
		}

		public void TestShowCancelPickButton_Finalised()
		{
			TestShowButton_Finalised_Core(FormButtonVisibilityHelper.ShowCancelPickButton, false);
		}

		public void TestShowSelectAllButton_Finalised()
		{
			TestShowButton_Finalised_Core(FormButtonVisibilityHelper.ShowSelectAllButton, false);
		}

		public void TestShowClearAllButton_Finalised()
		{
			TestShowButton_Finalised_Core(FormButtonVisibilityHelper.ShowClearAllButton, false);
		}

		public void TestShowReleaseButton_Finalised()
		{
			TestShowButton_Finalised_Core(FormButtonVisibilityHelper.ShowReleaseButton, true);
		}

		#endregion

		#region TestShowButton_PickSlip

		void TestShowButton_PickSlip_Core(Func<WhsPick, bool> visibilityMethodUnderTest, bool expectedResult)
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			data.Whs1.WW_AutoPrintPickingSlip = true;

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", data.Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("Precondition", PickStatus.Codes.PickSlip, pick.WP_PickStatus);
			AssertEquals($"{visibilityMethodUnderTest.Method.Name} should return {expectedResult} when pick slip is printed.", expectedResult, visibilityMethodUnderTest(pick));
		}

		public void TestShowAttachButton_PickSlip()
		{
			TestShowButton_PickSlip_Core(FormButtonVisibilityHelper.ShowAttachButton, true);
		}

		public void TestShowDetachButton_PickSlip()
		{
			TestShowButton_PickSlip_Core(FormButtonVisibilityHelper.ShowDetachButton, true);
		}

		public void TestShowEditButton_PickSlip()
		{
			TestShowButton_PickSlip_Core(FormButtonVisibilityHelper.ShowEditButton, true);
		}

		public void TestShowNewButton_PickSlip()
		{
			TestShowButton_PickSlip_Core(FormButtonVisibilityHelper.ShowNewButton, true);
		}

		public void TestShowAutoPickButton_PickSlip()
		{
			TestShowButton_PickSlip_Core(FormButtonVisibilityHelper.ShowAutoPickButton, true);
		}

		public void TestShowCancelPickButton_PickSlip()
		{
			TestShowButton_PickSlip_Core(FormButtonVisibilityHelper.ShowCancelPickButton, true);
		}

		public void TestShowSelectAllButton_PickSlip()
		{
			TestShowButton_PickSlip_Core(FormButtonVisibilityHelper.ShowSelectAllButton, true);
		}

		public void TestShowClearAllButton_PickSlip()
		{
			TestShowButton_PickSlip_Core(FormButtonVisibilityHelper.ShowClearAllButton, true);
		}

		public void TestShowReleaseButton_PickSlip()
		{
			TestShowButton_PickSlip_Core(FormButtonVisibilityHelper.ShowReleaseButton, true);
		}

		#endregion

		#region TestShowButton_WaitingReplenishment

		void TestShowButton_WaitingReplenishment_Core(Func<WhsPick, bool> visibilityMethodUnderTest, bool expectedResult)
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
			AllocationRulesHelper.AddFullPalletRule(ruleSet, 5);
			AllocationRulesHelper.AddPickFaceRules(ruleSet, 10);
			AllocationRulesHelper.AddFifoRule(ruleSet, 20, preventPickingPickFacesFromBulk: true);

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Today.AddDays(-1), data.Part1, 3m, pickFaceLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Today.AddDays(-4), data.Part1, 3m, bulkLocation, "");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			Helper.CreateWhsOrderLine(order, data.Part1, 6m);

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);
			pick.PickOrders();
			Factory.Save();
			AssertEquals("Precondition", true, pick.WP_IsAwaitingReplenishment);
			AssertEquals($"{visibilityMethodUnderTest.Method.Name} should return {expectedResult} when pick is awaiting replenishment.", expectedResult, visibilityMethodUnderTest(pick));
		}

		public void TestShowAttachButton_WaitingReplenishment()
		{
			TestShowButton_WaitingReplenishment_Core(FormButtonVisibilityHelper.ShowAttachButton, true);
		}

		public void TestShowDetachButton_WaitingReplenishment()
		{
			TestShowButton_WaitingReplenishment_Core(FormButtonVisibilityHelper.ShowDetachButton, true);
		}

		public void TestShowEditButton_WaitingReplenishment()
		{
			TestShowButton_WaitingReplenishment_Core(FormButtonVisibilityHelper.ShowEditButton, true);
		}

		public void TestShowNewButton_WaitingReplenishment()
		{
			TestShowButton_WaitingReplenishment_Core(FormButtonVisibilityHelper.ShowNewButton, true);
		}

		public void TestShowAutoPickButton_WaitingReplenishment()
		{
			TestShowButton_WaitingReplenishment_Core(FormButtonVisibilityHelper.ShowAutoPickButton, true);
		}

		public void TestShowCancelPickButton_WaitingReplenishment()
		{
			TestShowButton_WaitingReplenishment_Core(FormButtonVisibilityHelper.ShowCancelPickButton, true);
		}

		public void TestShowSelectAllButton_WaitingReplenishment()
		{
			TestShowButton_WaitingReplenishment_Core(FormButtonVisibilityHelper.ShowSelectAllButton, true);
		}

		public void TestShowClearAllButton_WaitingReplenishment()
		{
			TestShowButton_WaitingReplenishment_Core(FormButtonVisibilityHelper.ShowClearAllButton, true);
		}

		public void TestShowReleaseButton_WaitingReplenishment()
		{
			TestShowButton_WaitingReplenishment_Core(FormButtonVisibilityHelper.ShowReleaseButton, true);
		}

		#endregion

		#region TestShowButton_Cartonised

		void TestShowButton_Cartonised_Core(Func<WhsPick, bool> visibilityMethodUnderTest, bool expectedResult)
		{
			var packingHelper = new PackingTestHelper(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var org = data.Org1;
			var part = Helper.CreateProduct(org, "PR1");

			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var whsSize = Helper.CreateWhsCartonSize("WH", 10, 20, 40, 80, 160, 320, new ZByte(100), Constants.Length.Metres, Constants.Weight.Kilograms);
			whsGroup.CartonSizes.Add(whsSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", part, 5);
			var inventoryLine2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", part, 5);
			var splitCaseRefType = packingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.SplitCase);
			part.OP_StockKeepingUnit = splitCaseRefType.F3_Code;
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, part, 10);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;

			AssertEquals(DocketStatus.Codes.AttachedToPick, order.WD_DocketStatus);
			AssertEquals("Precondition", true, pick.WP_CartoniseSplitCases);
			Factory.Save();

			pick.AllocatePackageLabels();
			AssertEquals("Precondition - Cartonised", true, pick.WP_IsCartonised);
			AssertEquals($"{visibilityMethodUnderTest.Method.Name} should return {expectedResult} when pick is cartonised.", expectedResult, visibilityMethodUnderTest(pick));
		}

		public void TestShowAttachButton_Cartonised()
		{
			TestShowButton_Cartonised_Core(FormButtonVisibilityHelper.ShowAttachButton, false);
		}

		public void TestShowDetachButton_Cartonised()
		{
			TestShowButton_Cartonised_Core(FormButtonVisibilityHelper.ShowDetachButton, true);
		}

		public void TestShowEditButton_Cartonised()
		{
			TestShowButton_Cartonised_Core(FormButtonVisibilityHelper.ShowEditButton, false);
		}

		public void TestShowNewButton_Cartonised()
		{
			TestShowButton_Cartonised_Core(FormButtonVisibilityHelper.ShowNewButton, false);
		}

		public void TestShowAutoPickButton_Cartonised()
		{
			TestShowButton_Cartonised_Core(FormButtonVisibilityHelper.ShowAutoPickButton, false);
		}

		public void TestShowCancelPickButton_Cartonised()
		{
			TestShowButton_Cartonised_Core(FormButtonVisibilityHelper.ShowCancelPickButton, false);
		}

		public void TestShowSelectAllButton_Cartonised()
		{
			TestShowButton_Cartonised_Core(FormButtonVisibilityHelper.ShowSelectAllButton, false);
		}

		public void TestShowClearAllButton_Cartonised()
		{
			TestShowButton_Cartonised_Core(FormButtonVisibilityHelper.ShowClearAllButton, false);
		}

		public void TestShowReleaseButton_Cartonised()
		{
			TestShowButton_Cartonised_Core(FormButtonVisibilityHelper.ShowReleaseButton, true);
		}

		#endregion

		#region TestShowButton_Cartonising

		void TestShowButton_Cartonising_Core(Func<WhsPick, bool> visibiltyMethodUnderTest, bool expectedResut)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			pick.WP_PickCasesByLabel = false;
			pick.WP_PickPalletsByLabel = false;
			Factory.Save();

			var assertionRan = false;
			var mock = new Mock<IAllocatePackageLabelsStrategy>();
			mock.Setup(m => m.RunChecksPriorToCartonisingOrPickingByLabel(It.IsAny<WhsPick>(), It.IsAny<bool>())).Returns(true);
			mock.Setup(m => m.CartoniseSplitCases(It.IsAny<WhsPick>(), It.IsAny<bool>())).Returns(() =>
			{
				AssertEquals("Should be locked.", true, pick.IsCartonising);
				AssertEquals($"{visibiltyMethodUnderTest.Method.Name} should return {expectedResut} when pick is cartonising.", expectedResut, visibiltyMethodUnderTest(pick));

				assertionRan = true;
				return CartonisationResult.Cartonised;
			});

			pick.SetAllocatePackageLabelsStrategyForTest(mock.Object);
			pick.AllocatePackageLabels();
			AssertEquals(true, assertionRan);
			mock.VerifyAll();
		}

		public void TestShowAttachButton_Cartonising()
		{
			TestShowButton_Cartonising_Core(FormButtonVisibilityHelper.ShowAttachButton, false);
		}

		public void TestShowDetachButton_Cartonising()
		{
			TestShowButton_Cartonising_Core(FormButtonVisibilityHelper.ShowDetachButton, true);
		}

		public void TestShowEditButton_Cartonising()
		{
			TestShowButton_Cartonising_Core(FormButtonVisibilityHelper.ShowEditButton, false);
		}

		public void TestShowNewButton_Cartonising()
		{
			TestShowButton_Cartonising_Core(FormButtonVisibilityHelper.ShowNewButton, false);
		}

		public void TestShowAutoPickButton_Cartonising()
		{
			TestShowButton_Cartonising_Core(FormButtonVisibilityHelper.ShowAutoPickButton, false);
		}

		public void TestShowCancelPickButton_Cartonising()
		{
			TestShowButton_Cartonising_Core(FormButtonVisibilityHelper.ShowCancelPickButton, false);
		}

		public void TestShowSelectAllButton_Cartonising()
		{
			TestShowButton_Cartonising_Core(FormButtonVisibilityHelper.ShowSelectAllButton, false);
		}

		public void TestShowClearAllButton_Cartonising()
		{
			TestShowButton_Cartonising_Core(FormButtonVisibilityHelper.ShowClearAllButton, false);
		}

		public void TestShowReleaseButton_Cartonising()
		{
			TestShowButton_Cartonising_Core(FormButtonVisibilityHelper.ShowReleaseButton, true);
		}

		#endregion

		#region TestShowButton_WorkOrderPick

		void TestShowButton_WorkOrderPick_Core(Func<WhsPick, bool> visibilityMethodUnderTest, bool expectedResult)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m);
			Factory.Save();

			var order = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "WO1", data.Part1, 1m);
			var pick = Helper.CreatePickNew(order); // this saves during pick allocation

			Assert("Precondition", pick.IsWorkOrderPick);
			AssertEquals($"{visibilityMethodUnderTest.Method.Name} should return {expectedResult} after pick creation", expectedResult, visibilityMethodUnderTest(pick));
		}

		public void TestShowAttachButton_WorkOrderPick()
		{
			TestShowButton_WorkOrderPick_Core(FormButtonVisibilityHelper.ShowAttachButton, true);
		}

		public void TestShowDetachButton_WorkOrderPick()
		{
			TestShowButton_WorkOrderPick_Core(FormButtonVisibilityHelper.ShowDetachButton, true);
		}

		public void TestShowEditButton_WorkOrderPick()
		{
			TestShowButton_WorkOrderPick_Core(FormButtonVisibilityHelper.ShowEditButton, true);
		}

		public void TestShowNewButton_WorkOrderPick()
		{
			TestShowButton_WorkOrderPick_Core(FormButtonVisibilityHelper.ShowNewButton, true);
		}

		public void TestShowAutoPickButton_WorkOrderPick()
		{
			TestShowButton_WorkOrderPick_Core(FormButtonVisibilityHelper.ShowAutoPickButton, true);
		}

		public void TestShowCancelPickButton_WorkOrderPick()
		{
			TestShowButton_WorkOrderPick_Core(FormButtonVisibilityHelper.ShowCancelPickButton, true);
		}

		public void TestShowSelectAllButton_WorkOrderPick()
		{
			TestShowButton_WorkOrderPick_Core(FormButtonVisibilityHelper.ShowSelectAllButton, true);
		}

		public void TestShowClearAllButton_WorkOrderPick()
		{
			TestShowButton_WorkOrderPick_Core(FormButtonVisibilityHelper.ShowClearAllButton, true);
		}

		public void TestShowReleaseButton_WorkOrderPick()
		{
			TestShowButton_WorkOrderPick_Core(FormButtonVisibilityHelper.ShowReleaseButton, false);
		}

		#endregion

		#region TestShowButton_TaskPlanningStatus

		#region TestShowButton_TaskPlanningStatus_ReadyForPlanning

		void TestShowButton_TaskPlanningStatus_ReadyForPlanning_Core(Func<WhsPick, bool> visibilityMethodUnderTest, bool expectedResult)
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", data.Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;

			AssertEquals($"{visibilityMethodUnderTest.Method.Name} should return {expectedResult} after pick creation", expectedResult, visibilityMethodUnderTest(pick));
		}

		public void TestShowAttachButton_TaskPlanningStatus_ReadyForPlanning()
		{
			TestShowButton_TaskPlanningStatus_ReadyForPlanning_Core(FormButtonVisibilityHelper.ShowAttachButton, false);
		}

		public void TestShowDetachButton_TaskPlanningStatus_ReadyForPlanning()
		{
			TestShowButton_TaskPlanningStatus_ReadyForPlanning_Core(FormButtonVisibilityHelper.ShowDetachButton, false);
		}

		public void TestShowEditButton_TaskPlanningStatus_ReadyForPlanning()
		{
			TestShowButton_TaskPlanningStatus_ReadyForPlanning_Core(FormButtonVisibilityHelper.ShowEditButton, false);
		}

		public void TestShowNewButton_TaskPlanningStatus_ReadyForPlanning()
		{
			TestShowButton_TaskPlanningStatus_ReadyForPlanning_Core(FormButtonVisibilityHelper.ShowNewButton, false);
		}

		public void TestShowAutoPickButton_TaskPlanningStatus_ReadyForPlanning()
		{
			TestShowButton_TaskPlanningStatus_ReadyForPlanning_Core(FormButtonVisibilityHelper.ShowAutoPickButton, false);
		}

		public void TestShowCancelPickButton_TaskPlanningStatus_ReadyForPlanning()
		{
			TestShowButton_TaskPlanningStatus_ReadyForPlanning_Core(FormButtonVisibilityHelper.ShowCancelPickButton, false);
		}

		public void TestShowSelectAllButton_TaskPlanningStatus_ReadyForPlanning()
		{
			TestShowButton_TaskPlanningStatus_ReadyForPlanning_Core(FormButtonVisibilityHelper.ShowSelectAllButton, false);
		}

		public void TestShowClearAllPickButton_TaskPlanningStatus_ReadyForPlanning()
		{
			TestShowButton_TaskPlanningStatus_ReadyForPlanning_Core(FormButtonVisibilityHelper.ShowClearAllButton, false);
		}

		public void TestShowReleaseButton_TaskPlanningStatus_ReadyForPlanning()
		{
			TestShowButton_TaskPlanningStatus_ReadyForPlanning_Core(FormButtonVisibilityHelper.ShowReleaseButton, true);
		}

		#endregion

		#region TestShowButton_TaskPlanningStatus_NotReadyForPlanning

		void TestShowButton_TaskPlanningStatus_NotReadyForPlanning_Core(Func<WhsPick, bool> visibilityMethodUnderTest, bool expectedResult)
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", data.Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;

			AssertEquals($"{visibilityMethodUnderTest.Method.Name} should return {expectedResult} after pick creation", expectedResult, visibilityMethodUnderTest(pick));
		}

		public void TestShowAttachButton_TaskPlanningStatus_NotReadyForPlanning()
		{
			TestShowButton_TaskPlanningStatus_NotReadyForPlanning_Core(FormButtonVisibilityHelper.ShowAttachButton, true);
		}

		public void TestShowDetachButton_TaskPlanningStatus_NotReadyForPlanning()
		{
			TestShowButton_TaskPlanningStatus_NotReadyForPlanning_Core(FormButtonVisibilityHelper.ShowDetachButton, true);
		}

		public void TestShowEditButton_TaskPlanningStatus_NotReadyForPlanning()
		{
			TestShowButton_TaskPlanningStatus_NotReadyForPlanning_Core(FormButtonVisibilityHelper.ShowEditButton, true);
		}

		public void TestShowNewButton_TaskPlanningStatus_NotReadyForPlanning()
		{
			TestShowButton_TaskPlanningStatus_NotReadyForPlanning_Core(FormButtonVisibilityHelper.ShowNewButton, true);
		}

		public void TestShowAutoPickButton_TaskPlanningStatus_NotReadyForPlanning()
		{
			TestShowButton_TaskPlanningStatus_NotReadyForPlanning_Core(FormButtonVisibilityHelper.ShowAutoPickButton, true);
		}

		public void TestShowCancelPickButton_TaskPlanningStatus_NotReadyForPlanning()
		{
			TestShowButton_TaskPlanningStatus_NotReadyForPlanning_Core(FormButtonVisibilityHelper.ShowCancelPickButton, true);
		}

		public void TestShowSelectAllButton_TaskPlanningStatus_NotReadyForPlanning()
		{
			TestShowButton_TaskPlanningStatus_NotReadyForPlanning_Core(FormButtonVisibilityHelper.ShowSelectAllButton, true);
		}

		public void TestShowClearAllPickButton_TaskPlanningStatus_NotReadyForPlanning()
		{
			TestShowButton_TaskPlanningStatus_NotReadyForPlanning_Core(FormButtonVisibilityHelper.ShowClearAllButton, true);
		}

		public void TestShowReleaseButton_TaskPlanningStatus_NotReadyForPlanning()
		{
			TestShowButton_TaskPlanningStatus_NotReadyForPlanning_Core(FormButtonVisibilityHelper.ShowReleaseButton, true);
		}

		#endregion

		#region TestShowButton_TaskPlanningStatus_Planned

		void TestShowButton_TaskPlanningStatus_Planned_Core(Func<WhsPick, bool> visibilityMethodUnderTest, bool expectedResult)
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", data.Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;

			AssertEquals($"{visibilityMethodUnderTest.Method.Name} should return {expectedResult} after pick creation", expectedResult, visibilityMethodUnderTest(pick));
		}

		public void TestShowAttachButton_TaskPlanningStatus_Planned()
		{
			TestShowButton_TaskPlanningStatus_Planned_Core(FormButtonVisibilityHelper.ShowAttachButton, false);
		}

		public void TestShowDetachButton_TaskPlanningStatus_Planned()
		{
			TestShowButton_TaskPlanningStatus_Planned_Core(FormButtonVisibilityHelper.ShowDetachButton, false);
		}

		public void TestShowEditButton_TaskPlanningStatus_Planned()
		{
			TestShowButton_TaskPlanningStatus_Planned_Core(FormButtonVisibilityHelper.ShowEditButton, false);
		}

		public void TestShowNewButton_TaskPlanningStatus_Planned()
		{
			TestShowButton_TaskPlanningStatus_Planned_Core(FormButtonVisibilityHelper.ShowNewButton, false);
		}

		public void TestShowAutoPickButton_TaskPlanningStatus_Planned()
		{
			TestShowButton_TaskPlanningStatus_Planned_Core(FormButtonVisibilityHelper.ShowAutoPickButton, false);
		}

		public void TestShowCancelPickButton_TaskPlanningStatus_Planned()
		{
			TestShowButton_TaskPlanningStatus_Planned_Core(FormButtonVisibilityHelper.ShowCancelPickButton, false);
		}

		public void TestShowSelectAllButton_TaskPlanningStatus_Planned()
		{
			TestShowButton_TaskPlanningStatus_Planned_Core(FormButtonVisibilityHelper.ShowSelectAllButton, false);
		}

		public void TestShowClearAllPickButton_TaskPlanningStatus_Planned()
		{
			TestShowButton_TaskPlanningStatus_Planned_Core(FormButtonVisibilityHelper.ShowClearAllButton, false);
		}

		public void TestShowReleaseButton_TaskPlanningStatus_Planned()
		{
			TestShowButton_TaskPlanningStatus_Planned_Core(FormButtonVisibilityHelper.ShowReleaseButton, true);
		}

		#endregion

		#endregion
	}
}
