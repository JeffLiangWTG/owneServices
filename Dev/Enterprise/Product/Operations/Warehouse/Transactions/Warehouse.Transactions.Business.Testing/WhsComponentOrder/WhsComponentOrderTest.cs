using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	abstract class WhsComponentOrderTest<T> : WhsPickableDocketTest<T>
		where T : WhsComponentOrder
	{
		#region TestCarrierServiceLevel

		protected override bool SupportsCarrierServiceLevel(T docket) => false;

		#endregion

		#region TestIsAssembly

		public void TestIsAssembly()
		{
			var workOrder = GetNewBusinessObject();
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			AssertEquals(false, workOrder.IsAssembly);

			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			AssertEquals(true, workOrder.IsAssembly);
		}

		#endregion

		#region TestConsignee_DbHits

		protected override Dictionary<string, int> ExpectedDbHitsForConsigneeCore => new Dictionary<string, int>
		{
			{ OrgAddressSchema.Constants.TableName, 1 },
			{ OrgHeaderSchema.Constants.TableName, 1 },
		};

		#endregion

		#region TestIsCustomsTransaction

		protected override void TestIsCustomsTransactionCore()
		{
			var workOrder = GetNewBusinessObject();
			workOrder.WD_IsInwardsProcessingJob = false;
			AssertEquals(false, workOrder.IsCustomsTransaction);
			AssertEquals(false, workOrder.IsCustomsDataVisible);

			workOrder.WD_IsInwardsProcessingJob = true;
			AssertEquals(true, workOrder.IsCustomsTransaction);
			AssertEquals("Should not see customs data on an IPR work order.", false, workOrder.IsCustomsDataVisible);
		}

		#endregion

		#region TestIsUSBonded

		protected override bool SupportsCustomsTransactions => false;

		#endregion

		#region TestNoteContextsForRelatedNotesAssertions

		protected override void TestNoteContextsForRelatedNotesAssertions(T docket)
		{
			Assert((docket.GetNoteContextsForRelatedNotes().Module & StmNoteContextModule.W) != 0);
			Assert((docket.GetNoteContextsForRelatedNotes().Direction & StmNoteContextDirection.I) != 0);
			Assert((docket.GetNoteContextsForRelatedNotes().FreightMode & StmNoteContextFreightMode.W) != 0);
		}

		#endregion

		#region TransportIsReadOnly

		protected override bool TransportIsReadOnly => true;

		#endregion

		#region TestWD_RS_NKServiceLevelInfoCore

		protected override void TestWD_RS_NKServiceLevelInfoCore()
		{
			var docket = GetNewBusinessObject();
			AssertEquals("WD_RS_NKServiceLevel should readonly for Component Order", true, docket.WD_RS_NKServiceLevelInfo.ReadOnly);
		}

		#endregion

		#region TestWD_PL_NKCarrierServiceLevelInfoCore

		protected override void TestWD_PL_NKCarrierServiceLevelInfoCore()
		{
			var docket = GetNewBusinessObject();
			AssertEquals("WD_PL_NKCarrierServiceLevel should readonly for Component Order", true, docket.WD_PL_NKCarrierServiceLevelInfo.ReadOnly);
		}

		#endregion

		#region TestFinaliseDocket_CreatesReceive

		#region TestFinaliseDocket_CreatesReceive

		[TestDate(2009, 5, 6)]
		public void TestFinaliseDocket_CreatesReceive() => TestFinaliseDocket_CreatesReceiveCore();
		protected abstract void TestFinaliseDocket_CreatesReceiveCore();

		#endregion

		#region TestFinaliseDocket_CreatesReceive_SetBookingDateBasedOnBranchOfWarehouse

		[TestDate(2009, 5, 6)]
		public void TestFinaliseDocket_CreatesReceive_SetBookingDateBasedOnBranchOfWarehouse() => TestFinaliseDocket_CreatesReceive_SetBookingDateBasedOnBranchOfWarehouseCore();
		protected abstract void TestFinaliseDocket_CreatesReceive_SetBookingDateBasedOnBranchOfWarehouseCore();

		#endregion

		#region TestFinaliseDocket_CreatesReceive_AndFinalises_InformsUserIfFinaliseFails

		[TestDate(2009, 5, 6)]
		public void TestFinaliseDocket_CreatesReceiveAndFinalises_InformsUserIfFinaliseFails()
			=> TestFinaliseDocket_CreatesReceiveAndFinalises_InformsUserIfFinaliseFailsCore();

		protected abstract void TestFinaliseDocket_CreatesReceiveAndFinalises_InformsUserIfFinaliseFailsCore();

		#endregion

		#region TestFinaliseDocket_CreatesReceive_SameExternalNumberAsExistingReceive

		public void TestFinaliseDocket_CreatesReceive_SameExternalNumberAsExistingReceive()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMComponentsInInventory();
			SetupWarehouse(data.Whs1);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive1.WD_ExternalReference = "1234";
			receive1.WD_ExternalReferenceSplit = 1;

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive2.WD_ExternalReference = "1234-1";
			receive2.WD_ExternalReferenceSplit = 1;

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive3.WD_ExternalReference = "1234-2";
			receive3.WD_ExternalReferenceSplit = 1;
			Factory.Save();

			var workOrder = CreateComponentOrderForExternalReferenceTests(data);
			workOrder.WD_ExternalReference = "1234";
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			workOrder.WD_ExternalReferenceSplit = 1;
			Helper.CreatePickNew(workOrder);

			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertEquals("1234-3", workOrder.Receive.WD_ExternalReference);
		}

		protected virtual void SetupWarehouse(WhsWarehouse warehouse)
		{
		}

		protected abstract T CreateComponentOrderForExternalReferenceTests(TestDataForBOM data);

		#endregion

		#region TestFinaliseDocket_CreatesReceive_MaximumLengthExternalReference

		public void TestFinaliseDocket_CreatesReceive_MaximumLengthExternalReference()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMComponentsInInventory();
			SetupWarehouse(data.Whs1);

			var reference = new ZString('1', WhsDocketSchema.WD_ExternalReference.MaxLength);
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive1.WD_ExternalReference = reference;
			receive1.WD_ExternalReferenceSplit = 1;
			Factory.Save();

			var workOrder = CreateComponentOrderForExternalReferenceTests(data);
			workOrder.WD_ExternalReference = reference;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			workOrder.WD_DocketID = "TEST123";
			workOrder.WD_ExternalReferenceSplit = 1;

			Helper.CreatePickNew(workOrder);
			Factory.Save();

			workOrder.FinaliseDocketAlwaysFinalisingPick();
			workOrder.Receive.WD_DocketID = "TEST456";
			Factory.Save(); //Should not try to insert ExternalReference that is greater than the max column length.
			AssertEquals("ExternalReference should be set by the default behaviour (to WD_DocketID).", "TEST456", workOrder.Receive.WD_ExternalReference);
		}

		#endregion

		#region TestFinaliseDocket_CreatesReceive_GapInExternalReference

		public void TestFinaliseDocket_CreatesReceive_GapInExternalReference()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMComponentsInInventory();
			SetupWarehouse(data.Whs1);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive1.WD_ExternalReference = "1234";
			receive1.WD_ExternalReferenceSplit = 1;

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive2.WD_ExternalReference = "1234-4";
			receive2.WD_ExternalReferenceSplit = 1;

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive3.WD_ExternalReference = "1234-5";
			receive3.WD_ExternalReferenceSplit = 1;
			Factory.Save();

			var workOrder = CreateComponentOrderForExternalReferenceTests(data);
			workOrder.WD_ExternalReference = "1234";
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			workOrder.WD_ExternalReferenceSplit = 1;
			Helper.CreatePickNew(workOrder);

			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertEquals("1234-2", workOrder.Receive.WD_ExternalReference); // Should never start at 1 as it does not make logical sense (to a non dev)
		}

		#endregion

		#region TestFinaliseDocket_CreatesReceive_StressTestExternalReference

		[StressTest]
		public void TestFinaliseDocket_CreatesReceive_StressTestExternalReference()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMComponentsInInventory();
			SetupWarehouse(data.Whs1);

			for (var i = 0; i < 101; i++)
			{
				var newReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
				newReceive.WD_ExternalReference = "1234" + (i == 0 ? "" : "-" + i.ToString());
				newReceive.WD_ExternalReferenceSplit = 1;
			}
			Factory.Save();

			var workOrder = CreateComponentOrderForExternalReferenceTests(data);
			workOrder.WD_ExternalReference = "1234";
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			workOrder.WD_ExternalReferenceSplit = 1;
			Helper.CreatePickNew(workOrder);

			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertEquals("1234-101", workOrder.Receive.WD_ExternalReference);
		}

		#endregion

		#region TestFinaliseDocket_CreatesReceive_StressTestExternalReference_FallbackCase

		[StressTest]
		public void TestFinaliseDocket_CreatesReceive_StressTestExternalReference_FallbackCase()
		{
			// If there are 1000 dockets with similar ExternalReference's (appended with sequential numbers)
			// just use the default behaviour from OnFactorySaving
			var data = new TestDataForBOM(Factory);
			data.CreateBOMComponentsInInventory();
			SetupWarehouse(data.Whs1);
			Factory.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var helper2 = new WhsTestHelperFunctions(factory2);

			for (var i = 0; i < 1001; i++)
			{
				var newReceive = helper2.CreateWhsReceive(data.Org1, data.Whs1);
				newReceive.WD_ExternalReference = "1234" + (i == 0 ? "" : "-" + i.ToString());
				newReceive.WD_ExternalReferenceSplit = 1;

				if (i % 100 == 0)
				{
					factory2.Save();

					factory2 = new BusinessObjectFactory { RefreshEnabled = false };
					helper2 = new WhsTestHelperFunctions(factory2);
				}
			}
			factory2.Save();

			var workOrder = CreateComponentOrderForExternalReferenceTests(data);
			workOrder.WD_ExternalReference = "1234";
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			workOrder.WD_ExternalReferenceSplit = 1;
			Helper.CreatePickNew(workOrder);

			workOrder.FinaliseDocketAlwaysFinalisingPick();
			workOrder.Receive.WD_DocketID = "TEST456";
			Factory.Save(); //Should not try to insert ExternalReference that is greater than the max column length.
			AssertEquals("ExternalReference should be set by the default behaviour (to WD_DocketID).", "TEST456", workOrder.Receive.WD_ExternalReference);
		}

		#endregion

		#region TestFinaliseDocket_CreatesReceive_UsesInTransitTransfersDestinationLocation_Assembly

		public void TestFinaliseDocket_CreatesReceive_UsesInTransitTransfersDestinationLocation_Assembly()
			=> TestFinaliseDocket_CreatesReceive_UsesInTransitTransfersDestinationLocation_AssemblyCore();

		protected abstract void TestFinaliseDocket_CreatesReceive_UsesInTransitTransfersDestinationLocation_AssemblyCore();

		#endregion

		#region TestFinaliseDocket_CreatesReceive_PopulateNonDockDoorStagingLocation_WorkOrder

		public void TestFinaliseDocket_CreatesReceive_PopulateNonDockDoorStagingLocation_WorkOrder()
			=> TestFinaliseDocket_CreatesReceive_PopulateNonDockDoorStagingLocation_WorkOrderCore();

		protected abstract void TestFinaliseDocket_CreatesReceive_PopulateNonDockDoorStagingLocation_WorkOrderCore();

		#endregion

		#region TestFinaliseDocket_CreatesReceive_LinksInventory

		public void TestFinaliseDocket_CreatesReceive_LinksInventory()
			=> TestFinaliseDocket_CreatesReceive_LinksInventoryCore();

		protected abstract void TestFinaliseDocket_CreatesReceive_LinksInventoryCore();

		#endregion

		#region TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingSecondaryProducts

		public void TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingSecondaryProducts()
			=> TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingSecondaryProductsCore();

		protected abstract void TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingSecondaryProductsCore();

		#endregion

		#region TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingMultipleSecondaryProducts

		public void TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingMultipleSecondaryProducts()
			=> TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingMultipleSecondaryProductsCore();

		protected abstract void TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingMultipleSecondaryProductsCore();

		#endregion

		#region TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingSecondaryProducts_MultipleComponents

		public void TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingSecondaryProducts_MultipleComponents()
			=> TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingSecondaryProducts_MultipleComponentsCore();

		protected abstract void TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingSecondaryProducts_MultipleComponentsCore();

		#endregion

		#region TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingSecondaryProducts_MultipleComponents_AllUsedBySecondaryProduct

		public void TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingSecondaryProducts_MultipleComponents_AllUsedBySecondaryProduct()
			=> TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingSecondaryProducts_MultipleComponents_AllUsedBySecondaryProductCore();

		protected abstract void TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingSecondaryProducts_MultipleComponents_AllUsedBySecondaryProductCore();

		#endregion

		#region TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingWasteSecondaryProducts

		public void TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingWasteSecondaryProducts()
			=> TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingWasteSecondaryProductsCore();

		protected abstract void TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingWasteSecondaryProductsCore();

		#endregion

		#region TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingSecondaryProducts_ShouldNotFailIfTotalComponentQtyMatchesStockQuantity

		public void TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingSecondaryProducts_ShouldNotFailIfTotalComponentQtyMatchesStockQuantity()
			=> TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingSecondaryProducts_ShouldNotFailIfTotalComponentQtyMatchesStockQuantityCore();

		protected abstract void TestFinaliseDocket_CreatesReceive_LinksInventory_IncludingSecondaryProducts_ShouldNotFailIfTotalComponentQtyMatchesStockQuantityCore();

		#endregion

		#region TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory

		public void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory()
			=> TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventoryCore();

		protected abstract void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventoryCore();

		#endregion

		#region TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_UpdatesPackQuantityCorrectly

		public void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_UpdatesPackQuantityCorrectly()
			=> TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_UpdatesPackQuantityCorrectlyCore();

		protected abstract void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_UpdatesPackQuantityCorrectlyCore();

		#endregion

		#region TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_TwoLinesPickSameInventoryWhenSplit

		public void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_TwoLinesPickSameInventoryWhenSplit()
			=> TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_TwoLinesPickSameInventoryWhenSplitCore();

		protected abstract void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_TwoLinesPickSameInventoryWhenSplitCore();

		#endregion

		#region TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_InventoryIsNotInKitAmounts

		public void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_InventoryIsNotInKitAmounts()
			=> TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_InventoryIsNotInKitAmountsCore();

		protected abstract void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_InventoryIsNotInKitAmountsCore();

		#endregion

		#region TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_InventoryIsLessThanKitAmount

		public void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_InventoryIsLessThanKitAmount()
			=> TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_InventoryIsLessThanKitAmountCore();

		protected abstract void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_InventoryIsLessThanKitAmountCore();

		#endregion

		#region TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_GreedySplitScenario

		public void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_GreedySplitScenario()
			=> TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_GreedySplitScenarioCore();

		protected abstract void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_GreedySplitScenarioCore();

		#endregion

		#region TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_MultipleComponents

		public void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_MultipleComponents()
			=> TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_MultipleComponentsCore();

		protected abstract void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_MultipleComponentsCore();

		#endregion

		#region TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_MultipleComponentsOfDifferingInventoryAmounts

		public void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_MultipleComponentsOfDifferingInventoryAmounts()
			=> TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_MultipleComponentsOfDifferingInventoryAmountsCore();

		protected abstract void TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_MultipleComponentsOfDifferingInventoryAmountsCore();

		#endregion

		#endregion

		#region TestRelatedJobs

		protected override List<IRelatedJob> GetInvalidRelatedJobs(T docket)
		{
			docket.FillWithValidTestData();
			var unrelatedOrder = Factory.NewWithValidTestData<WhsOrder>();
			var unrelatedWorkOrder = Factory.NewWithValidTestData<WhsWorkOrder>();

			Factory.Save();

			return new List<IRelatedJob>() { unrelatedOrder, unrelatedWorkOrder };
		}

		#endregion

		#region TestUpdateTotalWeightAndVolumeWhenLineRemoved

		protected override sealed void TestUpdateTotalWeightAndVolumeWhenLineRemovedCore()
		{
			TestUpdateTotalWeightAndVolumeWhenLineRemovedCore(type: WorkOrderType.Codes.Assemble);
		}

		public void TestUpdateTotalWeightAndVolumeWhenLineRemoved_Disassemble()
		{
			TestUpdateTotalWeightAndVolumeWhenLineRemovedCore(type: WorkOrderType.Codes.Disassemble);
		}

		protected abstract void TestUpdateTotalWeightAndVolumeWhenLineRemovedCore(ZString type);

		#endregion

		#region TestWD_TotalWeightUnitUpdatesTotalWeight

		protected override sealed void TestWD_TotalWeightUnitUpdatesTotalWeightCore()
		{
			TestWD_TotalWeightAndVolumeCore(type: WorkOrderType.Codes.Assemble);
		}

		public void TestWD_TotalWeightUnitUpdatesTotalWeight_Disassemble()
		{
			TestWD_TotalWeightAndVolumeCore(type: WorkOrderType.Codes.Disassemble);
		}

		protected abstract void TestWD_TotalWeightAndVolumeCore(ZString type);

		#endregion

		#region TestWD_TotalWeightUnitUpdatesTotalWeight_MultipleLinesWithTheSameProduct

		protected override sealed void TestWD_TotalWeightUnitUpdatesTotalWeight_MultipleLinesWithTheSameProductCore()
		{
			TestWD_TotalWeightAndVolume_MultipleLinesWithTheSameProductCore(type: WorkOrderType.Codes.Assemble);
		}

		public void TestWD_TotalWeightUnitUpdatesTotalWeight_MultipleLinesWithTheSameProduct_Disassemble()
		{
			TestWD_TotalWeightAndVolume_MultipleLinesWithTheSameProductCore(type: WorkOrderType.Codes.Disassemble);
		}

		protected abstract void TestWD_TotalWeightAndVolume_MultipleLinesWithTheSameProductCore(ZString type);

		#endregion

		#region TestWD_TotalCubicUnitUpdatesTotalCubic

		protected override sealed void TestWD_TotalCubicUnitUpdatesTotalCubicCore()
		{
			Assert("Override to do nothing, covered in TestWD_TotalWeightUnitUpdatesTotalWeightCore", true);
		}

		#endregion

		#region TestWD_TotalCubicUnitUpdatesTotalCubic_MultipleLinesWithTheSameProduct

		protected override sealed void TestWD_TotalCubicUnitUpdatesTotalCubic_MultipleLinesWithTheSameProductCore()
		{
			Assert("Override to do nothing, covered in TestWD_TotalWeightAndVolume_MultipleLinesWithTheSameProductCore", true);
		}

		#endregion

		#region TestCanDeleteRelatedData

		public override void TestCanDeleteRelatedData()
		{
			base.TestCanDeleteRelatedData();

			var docket = GetNewBusinessObject();
			var pick = Factory.New<WhsPick>();
			docket.WD_WP = pick.PK;
			AssertEquals(false, docket.CanDeleteRelatedData);

			docket.WD_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals(false, docket.CanDeleteRelatedData);
		}

		#endregion
	}
}
