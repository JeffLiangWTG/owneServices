using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsDynamicWorkOrderLineValidationTest : WhsComponentOrderLineValidationTest<WhsDynamicWorkOrderLine, WhsDynamicWorkOrder>
	{
		#region Shortfall

		protected override ZString ExpectedShortfallWarning1 => ZString.Empty;

		protected override ZString ExpectedShortfallWarning2 => ZString.Empty;

		protected override void TestValidateWE_ShortfallQuantityCachedCore()
		{
			// Dynamic Work Order does not care about displaying shortfalls
			Assert(true);
		}

		#endregion

		#region TestCheckWE_OP

		public void TestCheckWE_OP_MainProduct_CannotBeBOM()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			var initialPart = Helper.CreateProduct("P3", data.Org1);
			Helper.CreateProductClientRelationShip(data.Org1, initialPart);
			Factory.Save();

			var workOrder = GetNewDocket();
			workOrder.WD_OH_Client = data.Org1.PK;
			var workOrderLine = GetNewDocketLine();
			workOrderLine.WE_WD = workOrder.PK;
			workOrderLine.WE_OP = initialPart.PK;
			workOrderLine.CustomsData.WB_IsMainInwardsProcessedItem = true;
			AssertNoErrors(workOrderLine.WE_OPInfo);

			workOrderLine.WE_OP = data.Part2.PK;
			AssertHasError(workOrderLine.WE_OPInfo, "Main Product cannot be a Bill of Materials.");

			workOrderLine.WE_OP = initialPart.PK;
			AssertNoErrors(workOrderLine.WE_OPInfo);
		}

		public void TestCheckWE_OP_SecondaryProduct_CannotBeBOM()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			var initialPart = Helper.CreateProduct("P3", data.Org1);
			Helper.CreateProductClientRelationShip(data.Org1, initialPart);
			Factory.Save();

			var workOrder = GetNewDocket();
			workOrder.WD_OH_Client = data.Org1.PK;
			var workOrderLineMain = GetNewDocketLine();
			workOrderLineMain.WE_WD = workOrder.PK;
			workOrderLineMain.CustomsData.WB_IsMainInwardsProcessedItem = true;

			var workOrderLineSec = GetNewDocketLine();
			workOrderLineSec.WE_WD = workOrder.PK;
			workOrderLineSec.WE_OP = initialPart.PK;
			workOrderLineSec.CustomsData.WB_IsSecondaryInwardsProcessedItem = true;
			AssertNoErrors(workOrderLineSec.WE_OPInfo);

			workOrderLineSec.WE_OP = data.Part2.PK;
			AssertHasError(workOrderLineSec.WE_OPInfo, "Secondary Product cannot be a Bill of Materials.");

			workOrderLineSec.WE_OP = initialPart.PK;
			AssertNoErrors(workOrderLineSec.WE_OPInfo);
		}

		#endregion

		#region TestCheckWE_TransactionQuantity

		protected override void TestCheckWE_TransactionQuantityCore()
		{
			var workOrder = GetNewDocket();
			var workOrderLineMain = GetNewDocketLine();
			workOrderLineMain.WE_WD = workOrder.PK;
			workOrderLineMain.CustomsData.WB_IsMainInwardsProcessedItem = true;
			workOrderLineMain.WE_TransactionQuantity = 2m;

			var workOrderLineComponent = GetNewDocketLine();
			workOrderLineComponent.WE_WD = workOrder.PK;
			workOrderLineComponent.WE_WE_ParentDocketLine = workOrderLineMain.PK;
			workOrderLineComponent.WE_TransactionQuantity = 2m;

			var workOrderLineSecondary = GetNewDocketLine();
			workOrderLineSecondary.WE_WD = workOrder.PK;
			workOrderLineSecondary.CustomsData.WB_IsSecondaryInwardsProcessedItem = true;
			workOrderLineSecondary.WE_TransactionQuantity = 1m;

			AssertNoErrors(workOrderLineMain.WE_TransactionQuantityInfo);
			AssertNoErrors(workOrderLineComponent.WE_TransactionQuantityInfo);
			AssertNoErrors(workOrderLineSecondary.WE_TransactionQuantityInfo);

			workOrderLineMain.WE_TransactionQuantity = 0m;
			AssertHasError(workOrderLineMain.WE_TransactionQuantityInfo, "Quantity must be greater than zero.");

			workOrderLineComponent.WE_TransactionQuantity = 0m;
			AssertHasError(workOrderLineComponent.WE_TransactionQuantityInfo, "Quantity must be greater than zero.");

			workOrderLineSecondary.WE_TransactionQuantity = 0m;
			AssertHasError(workOrderLineSecondary.WE_TransactionQuantityInfo, "Quantity must be greater than zero.");

			workOrderLineMain.WE_TransactionQuantity = 2m;
			workOrderLineComponent.WE_TransactionQuantity = 2m;
			workOrderLineSecondary.WE_TransactionQuantity = 0.5m;

			AssertNoErrors(workOrderLineMain.WE_TransactionQuantityInfo);
			AssertNoErrors(workOrderLineComponent.WE_TransactionQuantityInfo);
			AssertNoErrors(workOrderLineSecondary.WE_TransactionQuantityInfo);
		}

		public void TestCheckWE_TransactionQuantity_MainProduct_MustBeInteger()
		{
			var workOrderLine = GetNewDocketLine();
			workOrderLine.CustomsData.WB_IsMainInwardsProcessedItem = true;
			workOrderLine.WE_TransactionQuantity = 1m;
			AssertNoErrors(workOrderLine.WE_TransactionQuantityInfo);

			workOrderLine.WE_TransactionQuantity = 1.2m;
			AssertHasError(workOrderLine.WE_TransactionQuantityInfo, "Quantity of main product must be an integer.");

			workOrderLine.WE_TransactionQuantity = 1m;
			AssertNoErrors(workOrderLine.WE_TransactionQuantityInfo);
		}

		public void TestCheckWE_TransactionQuantity_SumOfSecondaryProductComponentsLessThanMainProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;
			Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var secondaryPart1 = Helper.CreateProduct("P3", data.Org1);
			var secondaryPart2 = Helper.CreateProduct("P4", data.Org1);
			Helper.CreateProductClientRelationShip(data.Org1, secondaryPart1);
			Helper.CreateProductClientRelationShip(data.Org1, secondaryPart2);
			Factory.Save();

			var workOrder = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1, "D1");
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			workOrder.WD_IsInwardsProcessingJob = true;

			var workOrderLineMain = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part2, 5m);
			workOrderLineMain.CustomsData.WB_IsMainInwardsProcessedItem = true;
			var workOrderLineMainComp = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 10m);
			workOrderLineMainComp.WE_WE_ParentDocketLine = workOrderLineMain.PK;

			var workOrderLineSecondary1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, secondaryPart1, 1m);
			workOrderLineSecondary1.CustomsData.WB_IsSecondaryInwardsProcessedItem = true;
			var workOrderLineSecondary1Comp = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 2m);
			workOrderLineSecondary1Comp.WE_WE_ParentDocketLine = workOrderLineSecondary1.PK;

			var workOrderLineSecondary2 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, secondaryPart2, 1m);
			workOrderLineSecondary2.CustomsData.WB_IsSecondaryInwardsProcessedItem = true;
			var workOrderLineSecondary2Comp = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 2m);
			workOrderLineSecondary2Comp.WE_WE_ParentDocketLine = workOrderLineSecondary2.PK;

			AssertNoErrors(workOrderLineSecondary1Comp.WE_TransactionQuantityInfo);
			AssertNoErrors(workOrderLineSecondary2Comp.WE_TransactionQuantityInfo);

			workOrderLineSecondary1Comp.WE_TransactionQuantity = 12m;
			AssertNoErrors(workOrderLineSecondary1Comp.WE_TransactionQuantityInfo);
			AssertNoErrors(workOrderLineSecondary2Comp.WE_TransactionQuantityInfo);

			workOrder.RunPreSaveValidation();
			AssertHasError(
				workOrderLineSecondary1Comp.WE_TransactionQuantityInfo,
				"The sum of a component across all secondary products must be less than or equal to the quantity of that component on the main product.");
			AssertHasError(
				workOrderLineSecondary2Comp.WE_TransactionQuantityInfo,
				"The sum of a component across all secondary products must be less than or equal to the quantity of that component on the main product.");

			workOrderLineSecondary1Comp.WE_TransactionQuantity = 4m;
			workOrder.RunPreSaveValidation();
			AssertNoErrors(workOrderLineSecondary1Comp.WE_TransactionQuantityInfo);
			AssertNoErrors(workOrderLineSecondary2Comp.WE_TransactionQuantityInfo);

			workOrderLineSecondary2Comp.WE_TransactionQuantity = 8m;
			workOrder.RunPreSaveValidation();
			AssertHasError(
				workOrderLineSecondary1Comp.WE_TransactionQuantityInfo,
				"The sum of a component across all secondary products must be less than or equal to the quantity of that component on the main product.");
			AssertHasError(
				workOrderLineSecondary2Comp.WE_TransactionQuantityInfo,
				"The sum of a component across all secondary products must be less than or equal to the quantity of that component on the main product.");

			workOrderLineSecondary2Comp.WE_TransactionQuantity = 1m;
			workOrder.RunPreSaveValidation();
			AssertNoErrors(workOrderLineSecondary1Comp.WE_TransactionQuantityInfo);
			AssertNoErrors(workOrderLineSecondary2Comp.WE_TransactionQuantityInfo);
		}

		#endregion

		#region TestValidateSumOfUnitsMet

		protected override void TestValidateSumOfUnitsMetCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			Factory.Save();

			var setup = DynamicWorkOrderTestSetup.CreateDynamicWorkOrderForTest(Helper, data.Org1, data.Whs1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_DocketSubType = "CUS";
			receive.WD_IsInwardsProcessingJob = true;
			// Orange in total shortfall (that's bad!), water fully available (that's good!), potassium benzoate in partial shortfall (that's bad!)
			var line1 = Helper.CreateWhsReceiveLine(receive, setup.Water_ComponentProduct, 100m, data.Whs1.DefaultLocationInInwardProcessingArea);
			var line2 = Helper.CreateWhsReceiveLine(receive, setup.PotassiumBenzoate_ComponentProduct, 5m, data.Whs1.DefaultLocationInInwardProcessingArea);
			line1.CustomsData.WB_EntryKey = "ABC";
			line2.CustomsData.WB_EntryKey = "ABC";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			Helper.CreatePickNew(setup.WorkOrder);
			Factory.Save();

			setup.WorkOrder.RunPreSaveValidation();
			AssertHasWarning(setup.MainComponentLine_Orange.SumOfUnitsMetInfo, "Shortfall");
			AssertHasWarning(setup.MainComponentLine_PotassiumBenzoate.SumOfUnitsMetInfo, "Shortfall");
			AssertNoNotifications(setup.MainProductLine_Juice.SumOfUnitsMetInfo);
			AssertNoNotifications(setup.SecondaryProductLine_Peel.SumOfUnitsMetInfo);
			AssertNoNotifications(setup.MainComponentLine_Water.SumOfUnitsMetInfo);
			AssertNoNotifications(setup.SecondaryComponentLine_Orange.SumOfUnitsMetInfo);
			AssertNoNotifications(setup.SecondaryComponentLine_PotassiumBenzoate.SumOfUnitsMetInfo);

			using (new SemaphoreManager(setup.WorkOrder.FinaliseDocketSemaphore))
			{
				AssertEquals("Precondition.", true, setup.WorkOrder.IsFinalising);

				setup.WorkOrder.RunPreSaveValidation();

				const string expectedError = "To finalize a Dynamic Work Order, main product component lines must be fully allocated.";
				AssertHasError(setup.MainComponentLine_Orange.SumOfUnitsMetInfo, expectedError);
				AssertHasError(setup.MainComponentLine_PotassiumBenzoate.SumOfUnitsMetInfo, expectedError);

				AssertNoNotifications(setup.MainProductLine_Juice.SumOfUnitsMetInfo);
				AssertNoNotifications(setup.SecondaryProductLine_Peel.SumOfUnitsMetInfo);
				AssertNoNotifications(setup.MainComponentLine_Water.SumOfUnitsMetInfo);
				AssertNoNotifications(setup.SecondaryComponentLine_Orange.SumOfUnitsMetInfo);
				AssertNoNotifications(setup.SecondaryComponentLine_PotassiumBenzoate.SumOfUnitsMetInfo);
			}
		}

		#endregion

		#region TestCheckWE_OP

		public void TestCheckWE_OP_NoDuplicationOfComponents()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;
			Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var componentPart2 = Helper.CreateProduct("P3", data.Org1);
			Helper.CreateProductClientRelationShip(data.Org1, componentPart2);
			Factory.Save();

			var workOrder = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1);
			var workOrderLineMain = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part2, 5m);
			workOrderLineMain.CustomsData.WB_IsMainInwardsProcessedItem = true;

			var workOrderLineMainComp1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 10m);
			workOrderLineMainComp1.WE_WE_ParentDocketLine = workOrderLineMain.PK;

			var workOrderLineMainComp2 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, componentPart2, 10m);
			workOrderLineMainComp2.WE_WE_ParentDocketLine = workOrderLineMain.PK;

			AssertNoErrors(workOrderLineMainComp1.WE_OPInfo);
			AssertNoErrors(workOrderLineMainComp2.WE_OPInfo);

			workOrderLineMainComp1.WE_OP = componentPart2.PK;
			AssertHasError(workOrderLineMainComp1.WE_OPInfo, "Each component line must have a unique product.");
			AssertHasError(workOrderLineMainComp2.WE_OPInfo, "Each component line must have a unique product.");

			workOrderLineMainComp2.WE_OP = data.Part1.PK;
			AssertNoErrors(workOrderLineMainComp1.WE_OPInfo);
			AssertNoErrors(workOrderLineMainComp2.WE_OPInfo);

			workOrderLineMainComp1.WE_OP = data.Part1.PK;
			AssertHasError(workOrderLineMainComp1.WE_OPInfo, "Each component line must have a unique product.");
			AssertHasError(workOrderLineMainComp2.WE_OPInfo, "Each component line must have a unique product.");

			workOrderLineMainComp1.WE_OP = componentPart2.PK;
			AssertNoErrors(workOrderLineMainComp1.WE_OPInfo);
			AssertNoErrors(workOrderLineMainComp2.WE_OPInfo);
		}

		#endregion

		#region Implementation

		protected override FinalisableDocketHelper<WhsDynamicWorkOrder> GetNewDocketHelper()
		{
			return new FinalisableDynamicWorkOrderHelper(Factory);
		}

		#endregion
	}
}
