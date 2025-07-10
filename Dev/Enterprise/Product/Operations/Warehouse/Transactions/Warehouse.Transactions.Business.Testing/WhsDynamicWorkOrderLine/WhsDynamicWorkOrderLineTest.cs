using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsDynamicWorkOrderLine))]
	class WhsDynamicWorkOrderLineTest : WhsComponentOrderLineTest<WhsDynamicWorkOrderLine, WhsDynamicWorkOrder>
	{
		#region TestDynamicWorkOrder

		public void TestDynamicWorkOrder()
		{
			var docket = GetNewWhsDocket();
			var line = docket.Lines.AddNew();

			var result = line.DynamicWorkOrder;
			AssertEquals(docket, result);
			AssertType<WhsDynamicWorkOrder>(result);
		}

		#endregion

		#region TestIsMainInwardProcessedItem

		public void TestIsMainInwardProcessedItem()
		{
			var workOrderLine = GetNewBusinessObject();
			AssertEquals("Precondition.", false, workOrderLine.CustomsData.WB_IsMainInwardsProcessedItem);
			AssertEquals("Precondition.", false, workOrderLine.IsMainInwardProcessedItem);

			workOrderLine.IsMainInwardProcessedItem = true;
			AssertEquals("Should have set IsMainInwardsProcessedItem.", true, workOrderLine.CustomsData.WB_IsMainInwardsProcessedItem);
			AssertEquals("Should have set IsMainInwardsProcessedItem.", true, workOrderLine.IsMainInwardProcessedItem);

			workOrderLine.IsMainInwardProcessedItem = false;
			AssertEquals("Should have reset IsMainInwardsProcessedItem.", false, workOrderLine.CustomsData.WB_IsMainInwardsProcessedItem);
			AssertEquals("Should have reset IsMainInwardsProcessedItem.", false, workOrderLine.IsMainInwardProcessedItem);

			workOrderLine.CustomsData.WB_IsMainInwardsProcessedItem = true;
			AssertEquals("Should have set IsMainInwardsProcessedItem.", true, workOrderLine.CustomsData.WB_IsMainInwardsProcessedItem);
			AssertEquals("Should have set IsMainInwardsProcessedItem.", true, workOrderLine.IsMainInwardProcessedItem);

			workOrderLine.Delete();
			AssertEquals("Should not blow up.", false, workOrderLine.IsMainInwardProcessedItem);
		}

		public void TestIsMainInwardProcessedItem_RefreshesChildCollection()
		{
			var workOrderLine = GetNewBusinessObject();

			var refreshCount = 0;
			workOrderLine.ChildComponentLinesCollection.CountChanged += (s, e) => refreshCount++;

			workOrderLine.IsMainInwardProcessedItem = true;
			AssertEquals("Should have refreshed ChildComponentLinesCollection for grid AllowNew.", 1, refreshCount);

			workOrderLine.IsMainInwardProcessedItem = false;
			AssertEquals("Should have refreshed ChildComponentLinesCollection for grid AllowNew.", 2, refreshCount);
		}

		public void TestIsMainInwardProcessedItem_ReadOnly()
		{
			TestHasComponentLinesOrFinalizedOrCancelled(dl => dl.IsMainInwardProcessedItemInfo);
		}

		void TestHasComponentLinesOrFinalizedOrCancelled(Func<WhsDynamicWorkOrderLine, ZPropertyInfo> getPropertyInfo)
		{
			var workOrderLine = GetNewBusinessObject();
			var propertyInfo = getPropertyInfo(workOrderLine);
			AssertEquals("Should *not* be readonly.", false, propertyInfo.ReadOnly);

			var workOrder = Factory.New<WhsDynamicWorkOrder>();
			workOrderLine.WE_WD = workOrder.PK;
			workOrder.WD_DocketSubType = DynamicWorkOrderType.Codes.Disassemble;
			AssertEquals("Should be readonly.", true, propertyInfo.ReadOnly);

			workOrder.WD_DocketSubType = DynamicWorkOrderType.Codes.Assemble;
			AssertEquals("Should *not* be readonly.", false, propertyInfo.ReadOnly);

			var componentLine = GetNewBusinessObject();
			componentLine.WE_WE_ParentDocketLine = workOrderLine.PK;
			AssertEquals("Should be readonly.", true, propertyInfo.ReadOnly);

			componentLine.Delete();
			AssertEquals("Should *not* be readonly.", false, propertyInfo.ReadOnly);

			workOrderLine.WE_DocketLineStatus = DocketLineStatus.Codes.Finalised;
			workOrderLine.WE_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals("Should be readonly.", true, propertyInfo.ReadOnly);

			workOrderLine.WE_FinalisedDate = ZDateTimeOffset.Empty;
			workOrderLine.WE_DocketLineStatus = DocketLineStatus.Codes.Cancelled;
			AssertEquals("Should be readonly.", true, propertyInfo.ReadOnly);
		}

		#endregion

		#region TestIsSecondaryInwardProcessedItem

		public void TestIsSecondaryInwardProcessedItem()
		{
			var workOrderLine = GetNewBusinessObject();
			AssertEquals("Precondition.", false, workOrderLine.CustomsData.WB_IsSecondaryInwardsProcessedItem);
			AssertEquals("Precondition.", false, workOrderLine.IsSecondaryInwardProcessedItem);

			workOrderLine.IsSecondaryInwardProcessedItem = true;
			AssertEquals("Should have set IsSecondaryInwardsProcessedItem.", true, workOrderLine.CustomsData.WB_IsSecondaryInwardsProcessedItem);
			AssertEquals("Should have set IsSecondaryInwardsProcessedItem.", true, workOrderLine.IsSecondaryInwardProcessedItem);

			workOrderLine.IsSecondaryInwardProcessedItem = false;
			AssertEquals("Should have reset IsSecondaryInwardsProcessedItem.", false, workOrderLine.CustomsData.WB_IsSecondaryInwardsProcessedItem);
			AssertEquals("Should have reset IsSecondaryInwardsProcessedItem.", false, workOrderLine.IsSecondaryInwardProcessedItem);

			workOrderLine.CustomsData.WB_IsSecondaryInwardsProcessedItem = true;
			AssertEquals("Should have set IsSecondaryInwardsProcessedItem.", true, workOrderLine.CustomsData.WB_IsSecondaryInwardsProcessedItem);
			AssertEquals("Should have set IsSecondaryInwardsProcessedItem.", true, workOrderLine.IsSecondaryInwardProcessedItem);

			workOrderLine.Delete();
			AssertEquals("Should not blow up.", false, workOrderLine.IsSecondaryInwardProcessedItem);
		}

		public void TestIsSecondaryInwardProcessedItem_RefreshesChildCollection()
		{
			var workOrderLine = GetNewBusinessObject();

			var refreshCount = 0;
			workOrderLine.ChildComponentLinesCollection.CountChanged += (s, e) => refreshCount++;

			workOrderLine.IsSecondaryInwardProcessedItem = true;
			AssertEquals("Should have refreshed ChildComponentLinesCollection for grid AllowNew.", 1, refreshCount);

			workOrderLine.IsSecondaryInwardProcessedItem = false;
			AssertEquals("Should have refreshed ChildComponentLinesCollection for grid AllowNew.", 2, refreshCount);
		}

		public void TestIsSecondaryInwardProcessedItem_ReadOnly()
		{
			TestHasComponentLinesOrFinalizedOrCancelled(dl => dl.IsSecondaryInwardProcessedItemInfo);
		}

		#endregion

		#region ReadOnly

		protected override void TestPartAttributesReadOnly(ZPropertyInfo info, TestDataSimpleEnvironment data)
		{
			base.TestPartAttributesReadOnly(info, data);

			var workOrder = Factory.New<WhsDynamicWorkOrder>();
			workOrder.WD_OH_Client = data.Org1.PK;
			workOrder.WD_WW_Whs = data.Whs1.PK;
			workOrder.WD_RequiredDate = DateTime.Now;

			var mainLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 10m);
			mainLine.IsMainInwardProcessedItem = true;

			var componentLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 10m);
			componentLine.WE_WE_ParentDocketLine = mainLine.PK;

			var secondaryLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 10m);
			secondaryLine.IsSecondaryInwardProcessedItem = true;

			var secondaryComponentLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 10m);
			secondaryComponentLine.WE_WE_ParentDocketLine = secondaryLine.PK;

			AssertEquals("Part Attributes should *not* be readonly for main items.", false, mainLine.ZPropertyInfoHash[info.Name].ReadOnly);
			AssertEquals("Part Attributes should *not* be readonly for secondary items.", false, componentLine.ZPropertyInfoHash[info.Name].ReadOnly);
			AssertEquals("Part Attributes should *not* be readonly for components on main items.", false, secondaryLine.ZPropertyInfoHash[info.Name].ReadOnly);
			AssertEquals("Part Attributes should be readonly for components on secondary items.", true, secondaryComponentLine.ZPropertyInfoHash[info.Name].ReadOnly);
		}

		#endregion

		#region Lookups

		protected override Type GetExpectedLookupsType() => typeof(WhsDynamicWorkOrderLineLookups);

		#endregion

		#region Validation

		protected override Type GetExpectedValidationType() => typeof(WhsDynamicWorkOrderLineValidation);

		#endregion

		#region TestCanGenerateChildWorkOrder

		protected override void TestCanGenerateChildWorkOrderCore()
		{
			AssertEquals(false, Factory.New<WhsDynamicWorkOrderLine>().CanGenerateChildWorkOrder);
		}

		#endregion

		#region TestRunPreSaveValidation_ClearsOutPackageGroupIDAndPerPackageQtyIfDocketIsNotUSBondedJob

		protected override void TestRunPreSaveValidation_ClearsOutPackageGroupIDAndPerPackageQtyIfDocketIsNotUSBondedJobCore()
		{
			Assert("Dynamic Work Orders does function for US Bonded.", true);
		}

		#endregion

		#region Picking

		public void TestPickLinesForRelease()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();
			Factory.Save();

			var workOrder = Factory.New<WhsDynamicWorkOrder>();
			workOrder.WD_OH_Client = data.Org1.PK;
			workOrder.WD_WW_Whs = data.Whs1.PK;
			workOrder.WD_RequiredDate = DateTime.Now;

			var line = Helper.CreateWhsPickableDocketLine(workOrder, data.BOM.Bike, 10m);
			var childLine = Helper.CreateWhsPickableDocketLine(workOrder, data.BOM.BikeEngine, 10m);
			childLine.WE_WE_ParentDocketLine = line.PK;
			Helper.CreatePickNew(workOrder);
			AssertEquals(typeof(WhsPickLineCollection), line.PickLinesForRelease.GetType());
			AssertEquals(typeof(WhsPickLineCollection), childLine.PickLinesForRelease.GetType());
			AssertEquals(0, line.PickLinesForRelease.Count);
			AssertEquals(0, childLine.PickLinesForRelease.Count);
		}

		protected override void TestSumOfUnitsMetCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var inwardProcessingArea = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var inwardProcessingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "IPR", 1, 1).Locations[0];
			inwardProcessingLocation.WLV_WA_PutawayArea = inwardProcessingArea.PK;
			inwardProcessingLocation.WLV_WA_PickingArea = inwardProcessingArea.PK;
			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive1.WD_IsInwardsProcessingJob = true;
			receive1.WD_DocketSubType = ReceiveType.Codes.Customs;
			var recLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, inwardProcessingLocation);
			recLine1.CustomsData.WB_EntryKey = "ENT- 1";
			receive1.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			receive2.WD_IsInwardsProcessingJob = true;
			receive2.WD_DocketSubType = ReceiveType.Codes.Customs;
			var recLine2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 10m, inwardProcessingLocation);
			recLine2.CustomsData.WB_EntryKey = "ENT- 2";
			receive2.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive2);

			var docket = GetNewWhsDocket();
			docket.WD_WW_Whs = data.Whs1.PK;
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_RequiredDate = ZDateTimeOffset.Now;
			Factory.Save();

			var docketLine = Helper.CreateWhsDynamicWorkOrderLine(docket, data.Part1, 10m);
			docketLine.CustomsData.WB_IsMainInwardsProcessedItem = true;
			var childLine = Helper.CreateWhsPickableDocketLine(docket, data.Part2, 10m);
			childLine.WE_WE_ParentDocketLine = docketLine.PK;
			AssertEquals("Docket is Unpicked, should have no SumOfUnitsMet.", 0m, docketLine.SumOfUnitsMet);

			Helper.CreatePickNew(docket);
			AssertEquals("Docket has 10 Picked, SumOfUnits Met should be equal to PickLineQuantity.", 10m, docketLine.SumOfUnitsMet);
			AssertEquals("Docket has 10 Picked, SumOfUnits Met should be equal to PickLineQuantity.", 10m, childLine.SumOfUnitsMet);

			var releaseLine1 = childLine.ReleaseLines[0];
			releaseLine1.Quantity = 15m;
			AssertEquals("Docket has 10 Picked but the Non-Persistent Wrapper has taken Precedence.", 15m, childLine.SumOfUnitsMet);

			var releaseLine2 = childLine.ReleaseLines.AddNew();
			releaseLine2.Quantity = 35m;
			AssertEquals("Docket has 10 Picked but the Non-Persistent Wrapper has taken Precedence.", 50m, childLine.SumOfUnitsMet);
		}

		protected override void TestPickLineQuantityCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var inwardProcessingLocation = row.Locations.Single();
			inwardProcessingLocation.WLV_WA_PutawayArea = area.PK;
			inwardProcessingLocation.WLV_WA_PickingArea = area.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_IsInwardsProcessingJob = true;
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var recLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, inwardProcessingLocation);
			recLine.CustomsData.WB_EntryKey = "ENT1";
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var workOrder = GetNewWhsDocket(data.Org1, data.Whs1);
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var workOrderLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part2, 100m);
			workOrderLine.CustomsData.WB_IsMainInwardsProcessedItem = true;

			var componentLine = workOrder.Lines.AddNew();
			componentLine.WE_OP = data.Part1.PK;
			componentLine.WE_TransactionQuantity = 100m;
			componentLine.WE_WE_ParentDocketLine = workOrderLine.PK;

			Factory.Save();

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(workOrder);

			pick.AutoAllocateItemsWithMock();
			AssertEquals(100m, componentLine.PickLineQuantity);
		}

		protected override void TestPickedPickLineQuantityCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var inwardProcessingLocation = row.Locations.Single();
			inwardProcessingLocation.WLV_WA_PutawayArea = area.PK;
			inwardProcessingLocation.WLV_WA_PickingArea = area.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_IsInwardsProcessingJob = true;
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var recLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, inwardProcessingLocation);
			recLine.CustomsData.WB_EntryKey = "ENT1";
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var workOrder = GetNewWhsDocket(data.Org1, data.Whs1);
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var workOrderLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part2, 100m);
			workOrderLine.CustomsData.WB_IsMainInwardsProcessedItem = true;

			var componentLine = workOrder.Lines.AddNew();
			componentLine.WE_OP = data.Part1.PK;
			componentLine.WE_TransactionQuantity = 100m;
			componentLine.WE_WE_ParentDocketLine = workOrderLine.PK;

			Factory.Save();

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(workOrder);

			pick.AutoAllocateItemsWithMock();
			AssertEquals(0m, componentLine.PickedPickLineQuantity);

			componentLine.PickLines.ForEach(l => l.WZ_PickedDateTime = ZDateTimeOffset.Now);
			AssertEquals(100m, componentLine.PickedPickLineQuantity);
		}

		protected override void TestQuantityNotPickedCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var inwardProcessingLocation = row.Locations.Single();
			inwardProcessingLocation.WLV_WA_PutawayArea = area.PK;
			inwardProcessingLocation.WLV_WA_PickingArea = area.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_IsInwardsProcessingJob = true;
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var line = Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, inwardProcessingLocation);
			line.CustomsData.WB_EntryKey = "ENT1";
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = GetNewWhsDocket();
			order.WD_OH_Client = data.Org1.PK;
			order.WD_WW_Whs = data.Whs1.PK;

			var parentLine = Helper.CreateWhsPickableDocketLine(order, data.Part2, 103m); // only 100 in stock
			parentLine.CustomsData.WB_IsMainInwardsProcessedItem = true;
			var childLine = Helper.CreateWhsPickableDocketLine(order, data.Part1, 103m);
			childLine.WE_WE_ParentDocketLine = parentLine.PK;
			Factory.Save();

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);

			pick.AutoAllocateItemsWithMock();
			AssertEquals(3m, childLine.QuantityNotPicked);
		}

		protected override void TestIWhsPickableDocketLineCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var inwardProcessingArea = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var inwardProcessingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "IPR", 1, 1).Locations[0];
			inwardProcessingLocation.WLV_WA_PutawayArea = inwardProcessingArea.PK;
			inwardProcessingLocation.WLV_WA_PickingArea = inwardProcessingArea.PK;
			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive1.WD_IsInwardsProcessingJob = true;
			receive1.WD_DocketSubType = ReceiveType.Codes.Customs;
			var line1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 6m, inwardProcessingLocation);
			line1.CustomsData.WB_EntryKey = "ENT1";
			receive1.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			receive2.WD_IsInwardsProcessingJob = true;
			receive2.WD_DocketSubType = ReceiveType.Codes.Customs;
			var line2 = Helper.CreateWhsReceiveLine(receive2, data.Part2, 6m, inwardProcessingLocation);
			line2.CustomsData.WB_EntryKey = "ENT2";
			receive2.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive2);

			var docket = GetNewWhsDocket();
			docket.WD_WW_Whs = data.Whs1.PK;
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_RequiredDate = ZDateTimeOffset.Now;
			Factory.Save();

			var docketLine = Helper.CreateWhsPickableDocketLine(docket, data.Part1, 10m);
			docketLine.CustomsData.WB_IsMainInwardsProcessedItem = true;
			var childLine = docket.Lines.AddNew();
			childLine.WE_OP = data.Part2.PK;
			childLine.WE_TransactionQuantity = 10m;
			childLine.WE_WE_ParentDocketLine = docketLine.PK;

			AssertEquals("Nothing picked yet.", 10m, docketLine.QuantityNotMet);
			AssertEquals("Nothing picked yet.", 10m, childLine.QuantityNotMet);
			AssertEquals("Nothing picked yet.", 10m, ((IWhsPickableDocketLine)docketLine).QuantityNotMet);

			Helper.CreatePickNew(docket);
			AssertEquals("Since Component Line isn't fully met, Parent line is considered completely short.", 0m, docketLine.SumOfUnitsMet);
			AssertEquals("Since Component Line isn't fully met, Parent line is considered completely short.", 10m, docketLine.QuantityNotMet);
			AssertEquals("Since Component Line isn't fully met, Parent line is considered completely short.", 10m, ((IWhsPickableDocketLine)docketLine).QuantityNotMet);

			AssertEquals("Component Line can be partially met, based on how much is allocated.", 4m, childLine.QuantityNotMet);
			AssertEquals("Component Line can be partially met, based on how much is allocated.", 6m, childLine.SumOfUnitsMet);

			childLine.WE_TransactionQuantity = 6m;
			AssertEquals("Since Component Line is fully allocated, Parent line is considered fully met.", 10m, docketLine.SumOfUnitsMet);
			AssertEquals("Since Component Line is fully allocated, Parent line is considered fully met.", 0m, docketLine.QuantityNotMet);
			AssertEquals("Since Component Line is fully allocated, Parent line is considered fully met.", 0m, ((IWhsPickableDocketLine)docketLine).QuantityNotMet);

			AssertEquals("Child Line is fully met.", 0m, childLine.QuantityNotMet);
			AssertEquals("Child Line is fully met.", 6m, childLine.SumOfUnitsMet);
		}

		protected override WhsPickableDocket GetPickableDocket_ForPickLinesTest(TestDataForBOM data) => GetNewWhsDocket();

		#endregion

		#region TestIsBondedTransaction

		protected override void TestIsBondedTransactionCore()
		{
			AssertEquals(true, GetNewBusinessObject(GetNewWhsDocket()).IsCustomsTransaction);
		}

		#endregion

		#region TestReleaseLines

		protected override WhsPickableDocketLine TestReleaseLines_SetUpWhsPickableDocket(TestDataSimpleEnvironment data)
		{
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var inwardProcessingLocation = row.Locations.Single();
			inwardProcessingLocation.WLV_WA_PutawayArea = area.PK;
			inwardProcessingLocation.WLV_WA_PickingArea = area.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_IsInwardsProcessingJob = true;
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var recLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, inwardProcessingLocation);
			recLine.CustomsData.WB_EntryKey = "ENT1";
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var pickableDocket = GetNewWhsDocket(data.Org1, data.Whs1);
			pickableDocket.WD_RequiredDate = ZDateTimeOffset.Now;

			var orderLine = Helper.CreateWhsDynamicWorkOrderLine(pickableDocket, data.Part1, 10m);
			orderLine.CustomsData.WB_IsMainInwardsProcessedItem = true;

			var componentLine = pickableDocket.Lines.AddNew();
			componentLine.WE_OP = data.Part2.PK;
			componentLine.WE_TransactionQuantity = 10m;
			componentLine.WE_WE_ParentDocketLine = orderLine.PK;

			return orderLine;
		}

		#endregion

		#region TestDelete_ReleaseLines

		protected override void TestDelete_ReleaseLinesCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var inwardProcessingArea = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var inwardProcessingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "IPR", 1, 1).Locations[0];
			inwardProcessingLocation.WLV_WA_PutawayArea = inwardProcessingArea.PK;
			inwardProcessingLocation.WLV_WA_PickingArea = inwardProcessingArea.PK;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive1.WD_IsInwardsProcessingJob = true;
			receive1.WD_DocketSubType = ReceiveType.Codes.Customs;
			var recLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part2, 10m, inwardProcessingLocation);
			recLine1.CustomsData.WB_EntryKey = "ENT1";
			receive1.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			receive2.WD_IsInwardsProcessingJob = true;
			receive2.WD_DocketSubType = ReceiveType.Codes.Customs;
			var recLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part2, 10m, inwardProcessingLocation);
			recLine2.CustomsData.WB_EntryKey = "ENT2";
			receive2.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive2);

			var workOrder = GetNewWhsDocket(data.Org1, data.Whs1);
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			workOrder.ConsigneePK = data.Org1.PK;

			var orderLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 20m);
			orderLine.IsMainInwardProcessedItem = true;

			var childLine = workOrder.Lines.AddNew();
			childLine.WE_OP = data.Part2.PK;
			childLine.WE_TransactionQuantity = 20m;
			childLine.WE_WE_ParentDocketLine = orderLine.PK;

			Helper.CreatePickNew(workOrder);

			var pickableDocketLine = orderLine.ChildComponentLines.ElementAt(0);
			AssertEquals("There should be one Release Line.", 1, pickableDocketLine.ReleaseLines.Count);

			pickableDocketLine.Delete();
			using (pickableDocketLine.ReleaseLines.SuspendRebuild())
			{
				AssertEquals("Release Lines should be cleared when Order Line is deleted.", 0, pickableDocketLine.ReleaseLines.Count);
			}

			AssertEquals("Release Lines should be cleared when Order Line is deleted.", 0, pickableDocketLine.ReleaseLines.Count);
		}

		#endregion

		#region TestPickLines

		protected override void TestPickLinesCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var iprArea = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var iprLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "IPR", 1, 1).Locations[0];
			iprLocation.WLV_WA_PutawayArea = iprArea.PK;
			iprLocation.WLV_WA_PickingArea = iprArea.PK;

			var bike = Helper.CreateProduct("Bike", data.Org1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive.WD_IsInwardsProcessingJob = true;
			for (var i = 0; i < 2; i++)
			{
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part2.PK, 1m, iprLocation.PK, palletID: "", entryKey: $"Ent-{i}");
			} // make sure these are on sep inv lines
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 1m, iprLocation.PK, palletID: "", entryKey: $"Ent-3");

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			// order some bikes
			var workOrder = Factory.New<WhsDynamicWorkOrder>();
			workOrder.WD_OH_Client = data.Org1.PK;
			workOrder.WD_WW_Whs = data.Whs1.PK;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var parentLine = workOrder.Lines.AddNew();
			parentLine.WE_OP = bike.PK;
			parentLine.WE_TransactionQuantity = 1m;
			parentLine.CustomsData.WB_IsMainInwardsProcessedItem = true;

			var childLine1 = workOrder.Lines.AddNew();
			childLine1.WE_OP = data.Part1.PK;
			childLine1.WE_TransactionQuantity = 1m;
			childLine1.WE_WE_ParentDocketLine = parentLine.PK;

			var childLine2 = workOrder.Lines.AddNew();
			childLine2.WE_OP = data.Part2.PK;
			childLine2.WE_TransactionQuantity = 2m;
			childLine2.WE_WE_ParentDocketLine = parentLine.PK;
			Factory.Save();

			Helper.CreatePickNew(workOrder);
			AssertEquals("Precondition - Pick failed.", true, workOrder.IsAttachedToPickButNotFinalised);
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var docketInOtherFactory = otherFactory.Load<WhsPickableDocket>(workOrder.PK);
			var line1 = FindPickableDocketLine(docketInOtherFactory, data.Part1);
			var line2 = FindPickableDocketLine(docketInOtherFactory, data.Part2);
			var initialDBHits = otherFactory.DatabaseLoadCount;
			AssertEquals("Should be 1 PickLine (1 Part1).", 1, line1.PickLines.Count);
			AssertEquals("Should be child editable.", true, line1.IsRegisteredEditableChildObject(line1.PickLines));
			AssertEquals("Should be 2 PickLines (1 Part2 per PickLine).", 2, line2.PickLines.Count);
			AssertEquals("Should be child editable.", true, line2.IsRegisteredEditableChildObject(line2.PickLines));
			AssertEquals("Should have used only 1 DB hit to load all PickLines", 1, otherFactory.DatabaseLoadCount - initialDBHits);
		}

		#endregion

		#region TestNotAllocateableInventoryCannotBeAttachedToPickLine_VirtualWarehouse

		public void TestNotAllocateableInventoryCannotBeAttachedToPickLine_VirtualWarehouse_DynamicWorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var inwardProcessingArea = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var inwardProcessingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "IPR", 1, 1).Locations[0];
			inwardProcessingLocation.WLV_WA_PutawayArea = inwardProcessingArea.PK;
			inwardProcessingLocation.WLV_WA_PickingArea = inwardProcessingArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_IsInwardsProcessingJob = true;
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var recLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, inwardProcessingLocation);
			recLine.CustomsData.WB_EntryKey = "ENT1";
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var dynamicWorkOrder = Factory.New<WhsDynamicWorkOrder>();
			dynamicWorkOrder.WD_OH_Client = data.Org1.PK;
			dynamicWorkOrder.WD_WW_Whs = data.Whs1.PK;
			dynamicWorkOrder.WD_ExternalReference = "O1";
			dynamicWorkOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var orderLine = dynamicWorkOrder.Lines.AddNew();
			orderLine.WE_OP = data.Part2.PK;
			orderLine.WE_TransactionQuantity = recLine.WE_StockOnHand;
			orderLine.CustomsData.WB_IsMainInwardsProcessedItem = true;

			var childLine = dynamicWorkOrder.Lines.AddNew();
			childLine.WE_OP = data.Part1.PK;
			childLine.WE_TransactionQuantity = recLine.WE_StockOnHand;
			childLine.WE_WE_ParentDocketLine = orderLine.PK;
			Helper.CreatePickNew(dynamicWorkOrder);

			Assert("Precondition: Stock is Picked.", orderLine.ChildComponentLines.Single().PickLineQuantity > 0);

			recLine.HeldCodeChangeQuantity = recLine.WE_StockOnHand;
			recLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			recLine.ChangeInventoryHeldCode(true);
			AssertEquals(InventoryStatus.Codes.Held, recLine.WE_CurrentInventoryStatus);

			var exceptionThrown = false;
			try
			{
				Factory.Save();
			}
			catch (ZSaveException e)
			{
				AssertEquals(WhsDocketLine.PreventAttemptToMakeAllocatedInventoryUnallocateable, e.InnerException.InnerException.Message);
				exceptionThrown = true;
			}

			AssertEquals("Trigger should have prevented save.", true, exceptionThrown);
		}

		#endregion

		#region TestProductFieldType

		public void TestProductFieldType()
		{
			var parentWorkOrderLine = GetNewBusinessObject();
			AssertEquals(nameof(FieldType.Guid), parentWorkOrderLine.ProductFieldType);

			var workOrderLine = GetNewBusinessObject();
			AssertEquals(nameof(FieldType.Guid), workOrderLine.ProductFieldType);

			workOrderLine.WE_WE_ParentDocketLine = parentWorkOrderLine.PK;
			AssertEquals(nameof(FieldType.Guid), workOrderLine.ProductFieldType);

			parentWorkOrderLine.IsMainInwardProcessedItem = true;
			AssertEquals(nameof(FieldType.Guid), workOrderLine.ProductFieldType);
			AssertEquals(nameof(FieldType.Guid), parentWorkOrderLine.ProductFieldType);

			parentWorkOrderLine.IsMainInwardProcessedItem = false;
			parentWorkOrderLine.IsSecondaryInwardProcessedItem = true;
			AssertEquals(nameof(FieldType.GuidDropEdit), workOrderLine.ProductFieldType);
			AssertEquals(nameof(FieldType.Guid), parentWorkOrderLine.ProductFieldType);
		}

		#endregion

		#region TestWE_BondedEntryKeyReadOnly

		public void TestWE_BondedEntryKeyReadOnly()
		{
			var docket = GetNewWhsDocket();
			var workOrderLine = docket.Lines.AddNew();
			workOrderLine.DynamicWorkOrder.WD_IsInwardsProcessingJob = true;
			AssertEquals("Should be readonly.", true, workOrderLine.WE_BondedEntryKeyInfo.ReadOnly);

			var childLine = workOrderLine.ChildComponentLinesCollection.AddNew();
			AssertEquals("Should be readonly.", true, childLine.WE_BondedEntryKeyInfo.ReadOnly);

			workOrderLine.IsSecondaryInwardProcessedItem = true;
			AssertEquals("Should be readonly.", true, childLine.WE_BondedEntryKeyInfo.ReadOnly);

			workOrderLine.IsSecondaryInwardProcessedItem = false;
			workOrderLine.IsMainInwardProcessedItem = true;
			AssertEquals("Should *not* be readonly.", false, childLine.WE_BondedEntryKeyInfo.ReadOnly);

			workOrderLine.DynamicWorkOrder.WD_IsInwardsProcessingJob = false;
			AssertEquals("Should be readonly.", true, childLine.WE_BondedEntryKeyInfo.ReadOnly);
		}

		#endregion

		#region TestWE_AllocationKeyInfo

		protected override void TestWE_AllocationKeyInfoCore()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();
			Factory.Save();

			var dynamicWorkOrder = Factory.New<WhsDynamicWorkOrder>();
			dynamicWorkOrder.WD_OH_Client = data.Org1.PK;
			dynamicWorkOrder.WD_WW_Whs = data.Whs1.PK;
			dynamicWorkOrder.WD_RequiredDate = DateTime.Now;

			var line = Helper.CreateWhsPickableDocketLine(dynamicWorkOrder, data.BOM.Bike, 1m);

			dynamicWorkOrder.WD_DocketSubType = DynamicWorkOrderType.Codes.Disassemble;
			AssertEquals("Should not be readonly.", false, line.WE_AllocationKeyInfo.ReadOnly);

			var pick = Helper.CreatePickNew(dynamicWorkOrder);
			AssertEquals("Should be readonly.", true, line.WE_AllocationKeyInfo.ReadOnly);

			pick.RemoveOrders(new[] { dynamicWorkOrder });
			AssertEquals("Should be readonly.", false, line.WE_AllocationKeyInfo.ReadOnly);
		}

		#endregion

		#region Shortfall

		protected override bool UsesShortfall => false;

		protected override void TestGetShortfallExistsStatus_AfterPickCore()
		{
			// we don't care about displaying shortfall for Dynamic Work Orders
			Assert(true);
		}

		protected override void TestSuspendShortfallCalculationCore()
		{
			// we don't care about displaying shortfall for Dynamic Work Orders
			Assert(true);
		}

		protected override void TestWE_ShortfallQuantityCached_AfterPickCore()
		{
			// we don't care about displaying shortfall for Dynamic Work Orders
			Assert(true);
		}

		protected override void TestWE_ShortfallQuantityCached_BeforePickCore()
		{
			// we don't care about displaying shortfall for Dynamic Work Orders
			Assert(true);
		}

		#endregion

		#region TestWE_TransactionQuantity_UpdatesTotalWeightAndVolume

		protected override void TestWE_TransactionQuantity_UpdatesTotalWeightAndVolume(ZString type)
		{
			var org = Helper.CreateClient();
			var mainPart = Helper.CreateProduct(org, "MainPart");
			var subPart1 = Helper.CreateProduct(org, "SubPart1");
			var subPart2 = Helper.CreateProduct(org, "SubPart2");
			Helper.SetProductWeightAndVolume(mainPart, 30, "KG", 1, "M3");
			Helper.SetProductWeightAndVolume(subPart1, 5, "KG", 0.5, "M3");
			Helper.SetProductWeightAndVolume(subPart2, 10, "KG", 0.1, "M3");

			var workOrder = GetNewWhsDocket();
			workOrder.WD_DocketSubType = type;
			workOrder.WD_TotalCubicUnit = Constants.Volume.Litre;
			workOrder.WD_TotalWeightUnit = Constants.Weight.Pounds;

			AssertEquals("Precondition - ensure Total Weight is 0", 0m, workOrder.WD_TotalWeight);
			AssertEquals("Precondition - ensure Total Volume is 0", 0m, workOrder.WD_TotalCubic);

			var workOrderLine = GetNewBusinessObject(workOrder);
			workOrderLine.IsMainInwardProcessedItem = true;
			workOrderLine.WE_OP = mainPart.PK;
			workOrderLine.WE_TransactionQuantity = 10m;

			if (type == WorkOrderType.Codes.Assemble)
			{
				var childComponentLine1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, subPart1, 0m);
				childComponentLine1.WE_WE_ParentDocketLine = workOrderLine.PK;
				childComponentLine1.WE_TransactionQuantity = 20m; // Set after the line becomes a component line

				var childComponentLine2 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, subPart2, 0m);
				childComponentLine2.WE_WE_ParentDocketLine = workOrderLine.PK;
				childComponentLine2.WE_TransactionQuantity = 10m; // Set after the line becomes a component line

				AssertEquals("Total Weight should be correct for Assemble", 440.92m, workOrder.WD_TotalWeight);
				AssertEquals("Total Volume should be correct for Assemble", 11000m, workOrder.WD_TotalCubic);
			}
			else
			{
				AssertEquals("Total Weight should be correct for Disassemble", 661.39m, workOrder.WD_TotalWeight);
				AssertEquals("Total Volume should be correct for Disassemble", 10000m, workOrder.WD_TotalCubic);
			}
		}

		public void TestWE_TransactionQuantity_UpdatesTotalWeightAndVolume_IgnoresSecondaryLines_Assembly()
			=> TestWE_TransactionQuantity_UpdatesTotalWeightAndVolume_IgnoresSecondaryLines(type: WorkOrderType.Codes.Assemble);

		public void TestWE_TransactionQuantity_UpdatesTotalWeightAndVolume_IgnoresSecondaryLines_Disassembly()
			=> TestWE_TransactionQuantity_UpdatesTotalWeightAndVolume_IgnoresSecondaryLines(type: WorkOrderType.Codes.Disassemble);

		void TestWE_TransactionQuantity_UpdatesTotalWeightAndVolume_IgnoresSecondaryLines(ZString type)
		{
			var org = Helper.CreateClient();
			var mainPart = Helper.CreateProduct(org, "MainPart");
			var subPart1 = Helper.CreateProduct(org, "SubPart1");
			var subPart2 = Helper.CreateProduct(org, "SubPart2");
			Helper.SetProductWeightAndVolume(mainPart, 30, "KG", 1, "M3");
			Helper.SetProductWeightAndVolume(subPart1, 5, "KG", 0.5, "M3");
			Helper.SetProductWeightAndVolume(subPart2, 10, "KG", 0.1, "M3");

			var workOrder = GetNewWhsDocket();
			workOrder.WD_DocketSubType = type;
			workOrder.WD_TotalCubicUnit = Constants.Volume.Litre;
			workOrder.WD_TotalWeightUnit = Constants.Weight.Pounds;

			AssertEquals("Precondition - ensure Total Weight is 0", 0m, workOrder.WD_TotalWeight);
			AssertEquals("Precondition - ensure Total Volume is 0", 0m, workOrder.WD_TotalCubic);

			var mainWorkOrderLine = GetNewBusinessObject(workOrder);
			mainWorkOrderLine.IsMainInwardProcessedItem = true;
			mainWorkOrderLine.WE_OP = mainPart.PK;
			mainWorkOrderLine.WE_TransactionQuantity = 10m;

			if (type == WorkOrderType.Codes.Assemble)
			{
				var mainChildComponentLine1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, subPart1, 0m);
				mainChildComponentLine1.WE_WE_ParentDocketLine = mainWorkOrderLine.PK;
				mainChildComponentLine1.WE_TransactionQuantity = 20m; // Set after the line becomes a component line

				var mainChildComponentLine2 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, subPart2, 0m);
				mainChildComponentLine2.WE_WE_ParentDocketLine = mainWorkOrderLine.PK;
				mainChildComponentLine2.WE_TransactionQuantity = 10m; // Set after the line becomes a component line

				var secondaryWorkOrderLine = GetNewBusinessObject(workOrder);
				secondaryWorkOrderLine.IsSecondaryInwardProcessedItem = true;
				secondaryWorkOrderLine.WE_OP = mainPart.PK;
				secondaryWorkOrderLine.WE_TransactionQuantity = 10m;

				var secondaryChildComponentLine1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, subPart1, 0m);
				secondaryChildComponentLine1.WE_WE_ParentDocketLine = secondaryWorkOrderLine.PK;
				secondaryChildComponentLine1.WE_TransactionQuantity = 10m; // Set after the line becomes a component line

				var secondaryChildComponentLine2 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, subPart2, 0m);
				secondaryChildComponentLine2.WE_WE_ParentDocketLine = secondaryWorkOrderLine.PK;
				secondaryChildComponentLine2.WE_TransactionQuantity = 5m; // Set after the line becomes a component line

				AssertEquals("Total Weight should be correct for Assemble", 440.92m, workOrder.WD_TotalWeight);
				AssertEquals("Total Volume should be correct for Assemble", 11000m, workOrder.WD_TotalCubic);
			}
			else
			{
				AssertEquals("Total Weight should be correct for Disassemble", 661.39m, workOrder.WD_TotalWeight);
				AssertEquals("Total Volume should be correct for Disassemble", 10000m, workOrder.WD_TotalCubic);
			}
		}

		#endregion

		#region TestWE_TransactionQuantity_UpdatesTotalWeightAndVolume_ProductVolumeCC

		protected override void TestWE_TransactionQuantity_UpdatesTotalWeightAndVolume_ProductVolumeCCCore()
		{
			TestWE_TransactionQuantity_UpdatesTotalWeightAndVolume_ProductVolumeCCCore(type: WorkOrderType.Codes.Assemble);
		}

		public void TestWE_TransactionQuantity_UpdatesTotalWeightAndVolume_ProductVolumeCC_Disassembly()
		{
			TestWE_TransactionQuantity_UpdatesTotalWeightAndVolume_ProductVolumeCCCore(type: WorkOrderType.Codes.Disassemble);
		}

		void TestWE_TransactionQuantity_UpdatesTotalWeightAndVolume_ProductVolumeCCCore(ZString type)
		{
			var org = Helper.CreateClient();
			var mainPart = Helper.CreateProduct(org, "MainPart");
			var subPart1 = Helper.CreateProduct(org, "SubPart1");
			var subPart2 = Helper.CreateProduct(org, "SubPart2");
			Helper.SetProductWeightAndVolume(mainPart, 300, "G", 10, "CC");
			Helper.SetProductWeightAndVolume(subPart1, 50, "G", 5, "CC");
			Helper.SetProductWeightAndVolume(subPart2, 100, "G", 1, "CC");

			var workOrder = GetNewWhsDocket();
			workOrder.WD_DocketSubType = type;
			workOrder.WD_TotalCubicUnit = Constants.Volume.Litre;
			workOrder.WD_TotalWeightUnit = Constants.Weight.Pounds;

			AssertEquals("Precondition - ensure Total Weight is 0", 0m, workOrder.WD_TotalWeight);
			AssertEquals("Precondition - ensure Total Volume is 0", 0m, workOrder.WD_TotalCubic);

			var workOrderLine = GetNewBusinessObject(workOrder);
			workOrderLine.IsMainInwardProcessedItem = true;
			workOrderLine.WE_OP = mainPart.PK;
			workOrderLine.WE_TransactionQuantity = 10m;

			if (type == WorkOrderType.Codes.Assemble)
			{
				var childComponentLine1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, subPart1, 0m);
				childComponentLine1.WE_WE_ParentDocketLine = workOrderLine.PK;
				childComponentLine1.WE_TransactionQuantity = 20m; // Set after the line becomes a component line

				var childComponentLine2 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, subPart2, 0m);
				childComponentLine2.WE_WE_ParentDocketLine = workOrderLine.PK;
				childComponentLine2.WE_TransactionQuantity = 10m; // Set after the line becomes a component line

				AssertEquals("Total Weight should be correct for Assemble", 4.40m, workOrder.WD_TotalWeight);
				AssertEquals("Total Volume should be correct for Assemble", 0.11m, workOrder.WD_TotalCubic);
			}
			else
			{
				AssertEquals("Total Weight should be correct for Disassemble", 6.61m, workOrder.WD_TotalWeight);
				AssertEquals("Total Volume should be correct for Disassemble", 0.100m, workOrder.WD_TotalCubic);
			}
		}

		#endregion

		#region TestClone_UpdateTotals

		protected override void TestClone_UpdateTotalsCore()
		{
			var org = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("1", "A", 1, 1);
			var mainPart = Helper.CreateProduct(org, "MainPart");
			var subPart1 = Helper.CreateProduct(org, "SubPart1");
			var subPart2 = Helper.CreateProduct(org, "SubPart2");
			Helper.SetProductWeightAndVolume(mainPart, 30, "KG", 1, "M3");
			Helper.SetProductWeightAndVolume(subPart1, 5, "KG", 0.5, "M3");
			Helper.SetProductWeightAndVolume(subPart2, 10, "KG", 0.1, "M3");

			// Note the below behaviour does not apply for disassembly
			var workOrder = Helper.CreateWhsDynamicWorkOrder(org, whs, "DW1");
			workOrder.WD_DocketSubType = DynamicWorkOrderType.Codes.Assemble;

			var workOrderLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, mainPart, 1m);
			workOrderLine.IsMainInwardProcessedItem = true;

			AssertEquals("Precondition - ensure Total Weight is correct when type is Assemble", 0m, workOrder.WD_TotalWeight);
			AssertEquals("Precondition - ensure Total Volume is correct when type is Assemble", 0m, workOrder.WD_TotalCubic);
			
			var childComponentLine1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, subPart1, 0m);
			childComponentLine1.WE_WE_ParentDocketLine = workOrderLine.PK;
			childComponentLine1.WE_TransactionQuantity = 2m; // Set after the line becomes a component line

			var childComponentLine2 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, subPart2, 0m);
			childComponentLine2.WE_WE_ParentDocketLine = workOrderLine.PK;
			childComponentLine2.WE_TransactionQuantity = 1m; // Set after the line becomes a component line

			AssertEquals("Precondition - ensure Total Weight is correct when type is Assemble", 20m, workOrder.WD_TotalWeight);
			AssertEquals("Precondition - ensure Total Volume is correct when type is Assemble", 1.1m, workOrder.WD_TotalCubic);

			var clone1 = (WhsDynamicWorkOrderLine)childComponentLine1.Clone();
			var clone2 = (WhsDynamicWorkOrderLine)childComponentLine2.Clone();
			workOrder.Lines.Add(clone1); // in the system it's happens when new Line added to the Grid, but in test we need to simulate it.
			workOrder.Lines.Add(clone2); // in the system it's happens when new Line added to the Grid, but in test we need to simulate it.

			AssertEquals("Total Weight did not change for Assemble", 20m, workOrder.WD_TotalWeight);
			AssertEquals("Total Volume did not change for Assemble", 1.1m, workOrder.WD_TotalCubic);
		}

		#endregion

		#region TestChildComponentLines

		public void TestChildComponentLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;

			var mainProduct = Helper.CreateProduct("BIKE", data.Org1);
			var componentProduct1 = Helper.CreateProduct("WHEEL", data.Org1);
			var componentProduct2 = Helper.CreateProduct("ENGINE", data.Org1);
			Factory.Save();

			var workOrder = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1, "ExtRef");
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			workOrder.WD_IsInwardsProcessingJob = true;

			var workOrderLineMain = Helper.CreateWhsDynamicWorkOrderLine(workOrder, mainProduct, 5m);
			workOrderLineMain.IsMainInwardProcessedItem = true;
			AssertEquals(0, workOrderLineMain.ChildComponentLines.Count);
			AssertEquals(0, workOrderLineMain.ChildComponentLinesCollection.Count);

			var workOrderLineComponent1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, componentProduct1, 5m);
			workOrderLineComponent1.WE_WE_ParentDocketLine = workOrderLineMain.PK;
			AssertContainsExactElementsInAnyOrder(new[] { workOrderLineComponent1 }, workOrderLineMain.ChildComponentLines);
			AssertContainsExactElementsInAnyOrder(new[] { workOrderLineComponent1 }, workOrderLineMain.ChildComponentLinesCollection);

			var workOrderLineComponent2 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, componentProduct2, 10m);
			workOrderLineComponent2.WE_WE_ParentDocketLine = workOrderLineMain.PK;
			AssertContainsExactElementsInAnyOrder(new[] { workOrderLineComponent1, workOrderLineComponent2 }, workOrderLineMain.ChildComponentLines);
			AssertContainsExactElementsInAnyOrder(new[] { workOrderLineComponent1, workOrderLineComponent2 }, workOrderLineMain.ChildComponentLinesCollection);
		}

		#endregion

		#region TestDelete_DeletesChildComponentLines

		public void TestDelete_DeletesChildComponentLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;

			var mainProduct = Helper.CreateProduct("BIKE", data.Org1);
			var componentProduct1 = Helper.CreateProduct("WHEEL", data.Org1);
			var componentProduct2 = Helper.CreateProduct("ENGINE", data.Org1);
			Factory.Save();

			var workOrder = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1, "ExtRef");
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			workOrder.WD_IsInwardsProcessingJob = true;

			var workOrderLineMain = Helper.CreateWhsDynamicWorkOrderLine(workOrder, mainProduct, 5m);
			workOrderLineMain.IsMainInwardProcessedItem = true;

			var workOrderLineComponent1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, componentProduct1, 5m);
			workOrderLineComponent1.WE_WE_ParentDocketLine = workOrderLineMain.PK;

			var workOrderLineComponent2 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, componentProduct2, 10m);
			workOrderLineComponent2.WE_WE_ParentDocketLine = workOrderLineMain.PK;

			workOrderLineMain.Delete();
			AssertEquals("Should be deleted.", true, workOrderLineMain.IsDeleted);
			AssertEquals("Should be deleted.", true, workOrderLineComponent1.IsDeleted);
			AssertEquals("Should be deleted.", true, workOrderLineComponent2.IsDeleted);
		}

		#endregion

		#region TestDelete_DoesNotRecalculateTotalWeightAndVolume

		protected override void TestDelete_DoesNotRecalculateTotalWeightAndVolume(ZString type)
		{
			var org = Helper.CreateClient();
			var mainPart = Helper.CreateProduct(org, "MainPart");
			var subPart1 = Helper.CreateProduct(org, "SubPart1");
			var subPart2 = Helper.CreateProduct(org, "SubPart2");
			Helper.SetProductWeightAndVolume(mainPart, 30, "KG", 1, "M3");
			Helper.SetProductWeightAndVolume(subPart1, 5, "KG", 0.5, "M3");
			Helper.SetProductWeightAndVolume(subPart2, 10, "KG", 0.1, "M3");

			var workOrder = GetNewWhsDocket();
			workOrder.WD_DocketSubType = type;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			workOrder.WD_IsInwardsProcessingJob = true;
			workOrder.WD_TotalCubicUnit = Constants.Volume.Litre;
			workOrder.WD_TotalWeightUnit = Constants.Weight.Pounds;

			workOrder.WD_TotalWeight = 1m;
			workOrder.WD_TotalCubic = 2m;

			var workOrderLine = GetNewBusinessObject(workOrder);
			workOrderLine.IsMainInwardProcessedItem = true;
			workOrderLine.WE_OP = mainPart.PK;
			workOrderLine.WE_TransactionQuantity = 10m;

			if (type == WorkOrderType.Codes.Assemble)
			{
				var childComponentLine1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, subPart1, 0m);
				childComponentLine1.WE_WE_ParentDocketLine = workOrderLine.PK;
				childComponentLine1.WE_TransactionQuantity = 20m; // Set after the line becomes a component line

				var childComponentLine2 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, subPart2, 0m);
				childComponentLine2.WE_WE_ParentDocketLine = workOrderLine.PK;
				childComponentLine2.WE_TransactionQuantity = 10m; // Set after the line becomes a component line

				AssertEquals("Total Weight should be correct for Assemble", 441.92m, workOrder.WD_TotalWeight);
				AssertEquals("Total Volume should be correct for Assemble", 11002m, workOrder.WD_TotalCubic);

				workOrderLine.Delete();
				AssertEquals("Should be deleted.", true, workOrderLine.IsDeleted);
				AssertEquals("Should be deleted.", true, childComponentLine1.IsDeleted);
				AssertEquals("Should be deleted.", true, childComponentLine2.IsDeleted);

				AssertEquals("Total Weight should be recaculated.", 1m, workOrder.WD_TotalWeight);
				AssertEquals("Total Volume should be recaculated.", 2m, workOrder.WD_TotalCubic);
			}
			else
			{
				AssertEquals("Total Weight should be correct for Disassemble", 662.39m, workOrder.WD_TotalWeight);
				AssertEquals("Total Volume should be correct for Disassemble", 10002m, workOrder.WD_TotalCubic);

				workOrderLine.Delete();
				AssertEquals("Should be deleted.", true, workOrderLine.IsDeleted);

				// This test can be fixed once the calculation above works
				AssertEquals("Total Weight should be unchanged", 1m, workOrder.WD_TotalWeight);
				AssertEquals("Total Volume should be unchanged", 2m, workOrder.WD_TotalCubic);
			}
		}

		#endregion

		#region TestWE_WD_UpdatingTotals

		protected override void TestWE_WD_UpdatingTotals(ZString type)
		{
			var org = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("A");
			var mainPart = Helper.CreateProduct(org, "MainPart");
			var subPart1 = Helper.CreateProduct(org, "SubPart1");
			var subPart2 = Helper.CreateProduct(org, "SubPart2");
			Helper.SetProductWeightAndVolume(mainPart, 30, "KG", 1, "M3");
			Helper.SetProductWeightAndVolume(subPart1, 5, "KG", 0.5, "M3");
			Helper.SetProductWeightAndVolume(subPart2, 10, "KG", 0.1, "M3");

			var workOrder1 = GetNewWhsDocket(org, whs);
			workOrder1.WD_DocketSubType = type;

			var workOrder2 = GetNewWhsDocket(org, whs);
			workOrder2.WD_DocketSubType = type;

			var workOrderLine1 = CreateLineWithComponentLinesForAssembly(workOrder1, mainPart.PK, 1m);
			var workOrderLine2 = CreateLineWithComponentLinesForAssembly(workOrder2, mainPart.PK, 1m);
			var migratingDocketLine = CreateLineWithComponentLinesForAssembly(workOrder1, mainPart.PK, 1m);

			workOrder1.WD_TotalWeight = 70m;
			workOrder1.WD_TotalCubic = 1.5m;
			workOrder2.WD_TotalWeight = 60m;
			workOrder2.WD_TotalCubic = 1.3m;

			AssertEquals("Precondition - ensure Total Weight is correct", 70m, workOrder1.WD_TotalWeight);
			AssertEquals("Precondition - ensure Total Cubic is correct", 1.5m, workOrder1.WD_TotalCubic);
			AssertEquals("Precondition - ensure Total Line Units is correct", 2m, workOrder1.WD_TotalUnitsFromLines);

			AssertEquals("Precondition - ensure Total Weight is correct", 60m, workOrder2.WD_TotalWeight);
			AssertEquals("Precondition - ensure Total Cubic is correct", 1.3m, workOrder2.WD_TotalCubic);
			AssertEquals("Precondition - ensure Total Line Units is correct", 1m, workOrder2.WD_TotalUnitsFromLines);

			migratingDocketLine.WE_WD = workOrder2.PK;
			AssertEquals("Total Line Units should be updated.", 1m, workOrder1.WD_TotalUnitsFromLines);
			AssertEquals("Total Line Units should be updated.", 2m, workOrder2.WD_TotalUnitsFromLines);

			if (type == WorkOrderType.Codes.Assemble)
			{
				AssertEquals("Total Weight should be updated, but not recalculated.", 50m, workOrder1.WD_TotalWeight);
				AssertEquals("Total Cubic should be updated, but not recalculated.", 0.4m, workOrder1.WD_TotalCubic);

				AssertEquals("Total Weight should be updated.", 80m, workOrder2.WD_TotalWeight);
				AssertEquals("Total Cubic should be updated.", 2.4m, workOrder2.WD_TotalCubic);
			}
			else
			{
				AssertEquals("Total Weight should be updated, but not recalculated.", 40m, workOrder1.WD_TotalWeight);
				AssertEquals("Total Cubic should be updated, but not recalculated.", 0.5m, workOrder1.WD_TotalCubic);

				AssertEquals("Total Weight is unchanged", 90m, workOrder2.WD_TotalWeight);
				AssertEquals("Total Cubic is unchanged", 2.3m, workOrder2.WD_TotalCubic);
			}

			WhsDynamicWorkOrderLine CreateLineWithComponentLinesForAssembly(WhsDynamicWorkOrder workOrder, ZGuid part, decimal quantity)
			{
				var workOrderLine = GetNewBusinessObject(workOrder);
				workOrderLine.IsMainInwardProcessedItem = true;
				workOrderLine.WE_OP = part;
				workOrderLine.WE_TransactionQuantity = 1m * quantity;

				if (workOrder.IsAssembly)
				{
					var childComponentLine1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, subPart1, 0m);
					childComponentLine1.WE_WE_ParentDocketLine = workOrderLine.PK;
					childComponentLine1.WE_TransactionQuantity = 2m * quantity; // Set after the line becomes a component line

					var childComponentLine2 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, subPart2, 0m);
					childComponentLine2.WE_WE_ParentDocketLine = workOrderLine.PK;
					childComponentLine2.WE_TransactionQuantity = 1m * quantity; // Set after the line becomes a component line
				}

				return workOrderLine;
			}
		}

		#endregion

		#region TestWE_OP_DefaultsPriceFields

		protected override bool DefaultPriceFieldsEnabled => false;

		#endregion

		#region TestMainProductLine_UnsafeAfterFinalization

		public void TestMainProductLine_UnsafeAfterFinalization()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;

			var mainProduct = Helper.CreateProduct("JUICE", data.Org1);
			var secondaryProduct = Helper.CreateProduct("PEEL", data.Org1);
			var componentProduct1 = Helper.CreateProduct("ORANGE", data.Org1);
			var componentProduct2 = Helper.CreateProduct("WATER", data.Org1);
			var componentProduct3 = Helper.CreateProduct("POTASSIUMBENZOATE", data.Org1);
			var invalidProduct = Helper.CreateProduct("FROGURT", data.Org1);
			Factory.Save();

			var workOrder = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1, "Juicy");
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			workOrder.WD_IsInwardsProcessingJob = true;

			var workOrderLineSecondary = Helper.CreateWhsDynamicWorkOrderLine(workOrder, secondaryProduct, 1m);
			workOrderLineSecondary.IsSecondaryInwardProcessedItem = true;

			var workOrderLineSecondaryComponent1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, componentProduct1, 0.1m);
			workOrderLineSecondaryComponent1.WE_WE_ParentDocketLine = workOrderLineSecondary.PK;

			var workOrderLineSecondaryComponent3 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, componentProduct3, 1m);
			workOrderLineSecondaryComponent3.WE_WE_ParentDocketLine = workOrderLineSecondary.PK;

			var workOrderLineMain1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, mainProduct, 1m);
			workOrderLineMain1.IsMainInwardProcessedItem = true;

			var workOrderLineMainComponent1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, componentProduct1, 1m);
			workOrderLineMainComponent1.WE_WE_ParentDocketLine = workOrderLineMain1.PK;

			var workOrderLineMainComponent2 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, componentProduct2, 2m);
			workOrderLineMainComponent2.WE_WE_ParentDocketLine = workOrderLineMain1.PK;

			var workOrderLineMainComponent3 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, componentProduct3, 10m);
			workOrderLineMainComponent3.WE_WE_ParentDocketLine = workOrderLineMain1.PK;

			AssertEquals(workOrderLineMain1, workOrder.MainProductLine_UnsafeAfterFinalization);
		}

		#endregion

		#region TestCalculateShortfall_NoMatchingAllocationKey_Disassemble

		public void TestCalculateShortfall_NoMatchingAllocationKey_Disassemble()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();
			data.CreateBOMComponentsInInventory();
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "ExtRef");
			Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 2m);
			Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 2m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			Helper.CreatePickNew(workOrder);

			workOrder.FinaliseDocketAlwaysFinalisingPick();
			workOrder.Receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(workOrder);

			var receiveLines = workOrder.Receive.Lines;
			receiveLines[0].WE_AllocationKey = "ABC";
			receiveLines[1].WE_AllocationKey = "DEF";

			AssertIsFinalisedPrecondition(workOrder.Receive);
			AssertNoExceptionThrown(() => Factory.Save());

			var workOrder2 = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "WO1", WorkOrderType.Codes.Disassemble);
			var workOrderLine1 = Helper.CreateWhsWorkOrderLine(workOrder2, data.BOM.Bike, 2m);
			workOrderLine1.WE_AllocationKey = "ABC";
			var workOrderLine2 = Helper.CreateWhsWorkOrderLine(workOrder2, data.BOM.Bike, 3m);
			workOrderLine2.WE_AllocationKey = "DEF";
			var workOrderLine3 = Helper.CreateWhsWorkOrderLine(workOrder2, data.BOM.Bike, 3m);
			workOrderLine3.WE_AllocationKey = "GHE";

			AssertEquals("Precondition - number of items in stock is incorrect.", 0m, workOrderLine1.WE_ShortfallQuantityCached);
			AssertEquals("Precondition - number of items in stock is incorrect.", 1m, workOrderLine2.WE_ShortfallQuantityCached);
			AssertEquals("Precondition - number of items in stock is incorrect.", 3m, workOrderLine3.WE_ShortfallQuantityCached);
		}

		#endregion

		#region TestGetQuantityFromComponents

		protected override void TestGetQuantityFromComponentsCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var inwardProcessingLocation = row.Locations.Single();
			inwardProcessingLocation.WLV_WA_PutawayArea = area.PK;
			inwardProcessingLocation.WLV_WA_PickingArea = area.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_IsInwardsProcessingJob = true;
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var recLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, inwardProcessingLocation);
			recLine.CustomsData.WB_EntryKey = "ENT1";
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var workOrder = GetNewWhsDocket(data.Org1, data.Whs1);
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var workOrderLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part2, 100m);
			workOrderLine.CustomsData.WB_IsMainInwardsProcessedItem = true;

			var componentLine = workOrder.Lines.AddNew();
			componentLine.WE_OP = data.Part1.PK;
			componentLine.WE_TransactionQuantity = 100m;
			componentLine.WE_WE_ParentDocketLine = workOrderLine.PK;

			Factory.Save();

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(workOrder);

			pick.AutoAllocateItemsWithMock();
			AssertEquals(0, componentLine.GetQuantityFromComponents(l => l.WE_TransactionQuantity));
		}

		#endregion

		#region SupportsHasProductUnitsOrAttribsChanged

		protected override bool SupportsHasProductUnitsOrAttribsChanged => false;

		#endregion

		#region CustomFieldsSupported

		protected override bool CustomFieldsSupported => false;

		#endregion

		#region Implementation

		protected override bool SupportsBOM => false;

		protected override FinalisableDocketHelper<WhsDynamicWorkOrder> GetNewDocketHelper(BusinessObjectFactory factory)
		{
			return new FinalisableDynamicWorkOrderHelper(factory);
		}

		#endregion
	}
}
