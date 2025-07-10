using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsTransactionHistoryTest : WhsTestCaseWithFactory
	{
		#region TestView_WithProductCategoryFilter

		public void TestView_WithProductCategoryFilter()
		{
			var categoryBeverages = Helper.CreateProductCategory("BEVERAGE", "All Beverages");
			var categorySoftDrinks = Helper.CreateProductCategory("SOFTDRK", "All Soft-Drinks", categoryBeverages);
			var categoryBeers = Helper.CreateProductCategory("BEERS", "All Beers", categoryBeverages);

			var warehouse = Helper.CreateWarehouse("W1", "A", 2, 2);
			var client = Helper.CreateClient("C1");

			var productTea = Helper.CreateProduct(client, "Tea");
			var teaRelationship =
				productTea.RelatedOrganisations.FindByOrganisationAndRelationship(client,
					OrgPartRelation.RelationshipTypes.Owner);
			teaRelationship.OU_OPC_Category = categorySoftDrinks.PK;

			var productCoke = Helper.CreateProduct(client, "Coke");
			var cokeRelationship =
				productCoke.RelatedOrganisations.FindByOrganisationAndRelationship(client,
					OrgPartRelation.RelationshipTypes.Owner);
			cokeRelationship.OU_OPC_Category = categorySoftDrinks.PK;

			var productVB = Helper.CreateProduct(client, "VB");
			var vbRelationship =
				productVB.RelatedOrganisations.FindByOrganisationAndRelationship(client,
					OrgPartRelation.RelationshipTypes.Owner);
			vbRelationship.OU_OPC_Category = categoryBeers.PK;

			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", productVB, 100m, true, true);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R2", productCoke, 200m, true, true);
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R3", productTea, 10m, true, true);

			Factory.Save();

			var result1 = LoadView_WithProductCategory(categoryBeers.PK);
			AssertEquals("ResultSet Count", 1, result1.Count);

			var result2 = LoadView_WithProductCategory(categorySoftDrinks.PK);
			AssertEquals("ResultSet Count", 2, result2.Count);

			var result3 = LoadView_WithProductCategory(categoryBeverages.PK);
			AssertEquals("ResultSet Count", 3, result3.Count);

			var result4 = LoadView_WithProductCategory(ZGuid.Empty);
			AssertEquals("ResultSet Count", 3, result4.Count);
		}

		DynamicBusinessObjectCollection LoadView_WithProductCategory(ZGuid categoryPK)
		{
			var result = new DynamicBusinessObjectCollection(Factory);

			var sql = categoryPK.IsEmpty
				? @"SELECT * FROM Report_WhsTransactions(null, null, null, null, null, null, null, null, null, 0)"
				: @"SELECT * FROM Report_WhsTransactions(null, null, null, null, @ProductCategoryPK, null, null, null, null, 0)";
			var sqlParams = new ZSqlParameterCollection();
			if (!categoryPK.IsEmpty)
			{
				sqlParams.Add("@ProductCategoryPK", categoryPK, OrgPartCategorySchema.PK);
			}

			result.Load(sql, sqlParams);

			return result;
		}

		#endregion

		#region TestView_WithProductStatusFilter

		public void TestView_WithProductStatusFilter()
		{
			Helper.CreateWarehouse("W1", "A", 2, 2);
			var client = Helper.CreateClient("C1");

			var productTea = Helper.CreateProduct(client, "Tea");
			productTea.OP_IsActive = true;

			var productCoke = Helper.CreateProduct(client, "Coke");
			productCoke.OP_IsActive = false;

			var productVB = Helper.CreateProduct(client, "VB");
			productVB.OP_IsActive = true;

			Factory.Save();

			var viewWithActiveProducts = LoadView_WithProductStatus(showInActiveProducts: false);
			AssertEquals("ResultSet Count", 2, viewWithActiveProducts.Count);

			var viewWithInActiveProducts = LoadView_WithProductStatus(showInActiveProducts: true);
			AssertEquals("ResultSet Count", 3, viewWithInActiveProducts.Count);
		}

		#endregion

		#region LoadView_WithProductStatus

		DynamicBusinessObjectCollection LoadView_WithProductStatus(bool showInActiveProducts)
		{
			var result = new DynamicBusinessObjectCollection(Factory);

			var sql =
				@"SELECT * FROM Report_WhsTransactions(null, null, null, null, null, null, null, null, null, @showInActiveProducts)";
			var sqlParams = new ZSqlParameterCollection
			{
				{ "@showInActiveProducts", showInActiveProducts, OrgSupplierPartSchema.OP_IsActive }
			};

			result.Load(sql, sqlParams);

			return result;
		}

		#endregion

		#region TestReport_WhsTransactions_PalletId

		[TestDate(2014, 12, 20)]
		public void TestReport_WhsTransactions_PalletId()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"), "A");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-2"), "");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			AssertEquals(true, receive.IsFinalised);

			var result = LoadView_WithProductCategory(ZGuid.Empty);
			AssertEquals("Should only return 2 rows", 2, result.Count);
		}

		#endregion

		#region TestFunctionFiltersOutTransitWarehouses

		public void TestFunctionFiltersOutTransitWarehouses()
		{
			var client = Helper.CreateClient();
			var productWarehouse = Helper.CreateWarehouse("WHS");
			var transitWarehouse = Helper.CreateWarehouse("TRA");
			transitWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			Helper.CreateProduct(client, "P1");
			Factory.Save();

			var results = Load_Report_WhsTransactions(ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			AssertEquals("Should only return 1 row, for Product P1, for Product Warehouse.", 1, results.Count);
			AssertEquals("WarehousePK", productWarehouse.PK, results[0]["WarehousePK"]);
		}

		#endregion

		#region TestReport_WhsTransactions_InternalAdjustments

		[TestDate(2014, 6, 14, 18, 29, 59)]
		public void TestReport_WhsTransactions_InternalAdjustmentsCounted()
		{
			var results = Report_WhsTransactions_BothAdjustments(includeInternalAdjustment: true);
			AssertEquals("Internal adjustment should be included in the report", 2, results.Count);
		}

		[TestDate(2014, 6, 15, 18, 29, 59)]
		public void TestReport_WhsTransactions_InternalAdjustmentsIgnored()
		{
			var results = Report_WhsTransactions_BothAdjustments(includeInternalAdjustment: false);
			AssertEquals("Internal adjustment should be ignored by the report", 1, results.Count);
			var actualLine = results[0];
			AssertEquals("Only normal adjustment returned", 8m, actualLine["Units"]);
		}

		protected DynamicBusinessObjectCollection Report_WhsTransactions_BothAdjustments(bool includeInternalAdjustment)
		{
			var whs1 = Helper.CreateWarehouse("WH1", "A", 2, 1);
			var client = Helper.CreateClient("CLIENT");
			var part = Helper.CreateProduct(client, "P1");
			Factory.Save();
			var now = ZDateTime.Now;
			var nowOffset = whs1.GetWarehouseBranchDateTimeOffset(now);

			// ADJ +8 (normal adjustment)
			var adjustment = Helper.CreateWhsAdjustment(client, whs1, "adj123", Notify);
			Helper.CreateWhsAdjustmentLine(adjustment, part, 8m, whs1.FindLocation("A-1"));
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Adjustment;
			adjustment.FinaliseDocket();
			adjustment.WD_FinalisedDate = nowOffset;
			AssertIsFinalisedPrecondition(adjustment);

			// ADJ +10  (internal adjustment)
			var adjustmentInternal = Helper.CreateWhsAdjustment(client, whs1, "adj234", Notify);
			Helper.CreateWhsAdjustmentLine(adjustmentInternal, part, 8m, whs1.FindLocation("A-1"));
			adjustmentInternal.WD_DocketSubType = AdjustmentType.Codes.InternalWarehouseAdjustment;
			adjustmentInternal.FinaliseDocket();
			adjustmentInternal.WD_FinalisedDate = nowOffset;
			AssertIsFinalisedPrecondition(adjustmentInternal);

			Factory.Save();

			return Load_Report_WhsTransactions(now.AddHours(-1), now.AddHours(1), includeInternalAdjustment);
		}

		#endregion

		#region TestReport_WhsTransactions_Filter_FinalisedOrderWithUnfinalisedPickShown

		[TestDate(2024, 4, 25, 6, 0, 0)]
		public void TestReport_WhsTransactions_Filter_FinalisedOrderWithUnfinalisedPickShown()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2, 2m, data.Part2.OP_StockKeepingUnit);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part2, 10m);
			var orderPick = Helper.CreatePickNew(order);
			orderPick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order);
			AssertEquals("Precondition - ensure Pick is NOT Finalised.", false, orderPick.IsFinalised);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.Part1, 20m);
			Factory.Save();

			var workOrderPick = Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocket();
			AssertIsFinalisedPrecondition(workOrder);
			AssertEquals("Precondition - ensure Pick is NOT Finalised.", false, workOrderPick.IsFinalised);

			Factory.Save();

			var results = Load_Report_WhsTransactions(ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			AssertEquals(
				"2 transactions expected: receive and empty line for Part1. Picks are not Finalised but Orders are, so still should be shown.",
				4, results.Count);
			results.Cast<DynamicBusinessObject>().Single(l =>
				(ZDecimal)l["Units"] == 0m && (ZGuid)l["ProductPK"] == data.Part1.PK && (ZString)l["Reference"] == "");
			AssertReport_WhsTransactionsFilterLineMatch(receive.Lines[0], results);
			AssertReport_WhsTransactionsFilterLineMatch(order.Lines[0], results);
			AssertReport_WhsTransactionsFilterLineMatch(workOrder.AllLines.Single(l => l.WE_OP == data.Part2.PK),
				results);
		}

		[TestDate(2024, 4, 25, 6, 0, 0)]
		public void TestReport_WhsTransactions_Filter_FinalisedOrderWithUnfinalisedPickShown_DynamicWorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var inwardProcessingArea = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var inwardProcessingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "IPR", 1, 1).Locations[0];
			inwardProcessingLocation.WLV_WA_PutawayArea = inwardProcessingArea.PK;
			inwardProcessingLocation.WLV_WA_PickingArea = inwardProcessingArea.PK;

			var receiveCus = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receiveCus.WD_IsInwardsProcessingJob = true;
			receiveCus.WD_DocketSubType = ReceiveType.Codes.Customs;
			var recLine1 = Helper.CreateWhsReceiveLine(receiveCus, data.Part2, 100m, inwardProcessingLocation);
			recLine1.CustomsData.WB_EntryKey = "ENT1";
			receiveCus.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receiveCus);

			var receiveOrd = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 80m);
			Factory.Save();

			var workOrder = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1);
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			var kitLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 20m);
			kitLine.WE_F3_NKPackType = data.Part1.OP_StockKeepingUnit;
			kitLine.IsMainInwardProcessedItem = true;

			var componentLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part2, 40m);
			componentLine.WE_F3_NKPackType = data.Part2.OP_StockKeepingUnit;
			componentLine.WE_WE_ParentDocketLine = kitLine.PK;

			workOrder.WD_AutoFinaliseBOMIntoInventory = false; // hack to avoid finalizing receive

			var workOrderPick = Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocket();
			AssertIsFinalisedPrecondition(workOrder);
			AssertEquals("Precondition - ensure Pick is NOT Finalised.", false, workOrderPick.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part2, 10m);
			var orderPick = Helper.CreatePickNew(order);
			orderPick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order);
			AssertEquals("Precondition - ensure Pick is NOT Finalised.", false, orderPick.IsFinalised);
			Factory.Save();

			var results = Load_Report_WhsTransactions(ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			AssertEquals(
				"5 transactions expected: receives and empty line for Part1. Picks are not Finalised but Orders are, so still should be shown.",
				5, results.Count);
			results.Cast<DynamicBusinessObject>().Single(l =>
				(ZDecimal)l["Units"] == 0m && (ZGuid)l["ProductPK"] == data.Part1.PK && (ZString)l["Reference"] == "");
			AssertReport_WhsTransactionsFilterLineMatch(receiveCus.Lines[0], results);
			AssertReport_WhsTransactionsFilterLineMatch(receiveOrd.Lines[0], results);
			AssertReport_WhsTransactionsFilterLineMatch(order.Lines[0], results);
			AssertReport_WhsTransactionsFilterLineMatch(workOrder.AllLines.Single(l => l.WE_OP == data.Part2.PK),
				results);
		}

		#endregion

		#region TestReport_WhsTransactions_Filter_WorkOrders_AttribsAreCorrect

		[TestDate(2019, 1, 1)]
		public void TestReport_WhsTransactions_Filter_WorkOrders_AttribsAreCorrect()
		{
			var year = ZDateTime.Now.Year;

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, true, useSerialNumber: false);
			Helper.CreateProductBOM(data.Part1, data.Part2, 2m, data.Part2.OP_StockKeepingUnit);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 100m, new ZDate(year, 1, 2),
				new ZDate(year + 1, 1, 1), "PA1", "PA2", "PA3", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.Part1, 20m);
			var pick = Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);
			AssertIsFinalisedPrecondition(pick);

			Factory.Save();

			var results = Load_Report_WhsTransactions(ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			AssertEquals("3 transactions expected, 1x receive, 1x work order and 1x empty line for Part1.", 3,
				results.Count);
			var workOrderLine = workOrder.AllLines.Single(l => l.WE_OP == data.Part2.PK);
			results.Cast<DynamicBusinessObject>().Single(l =>
				(ZDecimal)l["Units"] == 0m && (ZGuid)l["ProductPK"] == data.Part1.PK && (ZString)l["Reference"] == "");
			AssertReport_WhsTransactionsFilterLineMatch(receive.Lines[0], results);
			AssertReport_WhsTransactionsFilterLineMatch(workOrderLine, results);
		}

		#endregion

		#region TestReport_WhsTransactions_FilterNoDivideByZeroExceptionDueToPalletConversion

		[TestDate(2015, 03, 27)]
		public void TestReport_WhsTransactions_FilterNoDivideByZeroExceptionDueToPalletConversion()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, "UNT", "CAS", 0.0001);
			Helper.CreateProductUnit(data.Part1, "CAS", "PLT", 0.0001);
			// so after all rounding we will get 0 PLT in a UNT

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R", data.Part1, 1m);
			Factory.Save();

			var result = Load_OrgSupplierPartUnitsPerPallet(data.Part1.PK);
			AssertEquals("Precondition: OrgSupplierPartUnitsPerPallet should return 0", 0m,
				result[0]["NumPalletsPerUnit"]);

			//OutsideContext = "All Transactions";
			AssertNoExceptionThrown("No Division by zero exception should be thrown",
				() => Load_Report_WhsTransactions(ZDateTime.Today.AddDays(-2), ZDateTime.Today));
			var results = Load_Report_WhsTransactions(ZDateTime.Today.AddDays(-2), ZDateTime.Today);
			Assert(results.Count > 0);
		}

		DynamicBusinessObjectCollection Load_OrgSupplierPartUnitsPerPallet(ZGuid productPK)
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var sql =
				@"SELECT UnitsPerPallet as NumPalletsPerUnit FROM dbo.OrgSupplierPartUnitsPerPallet(@ProductPK) tableResult";

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@ProductPK", productPK, WhsDocketLineSchema.WE_OP);
			result.Load(sql, sqlParams);

			return result;
		}

		#endregion

		#region TestReport_WhsTransactions

		#region TestReport_WhsTransactions

		[SnailTest]
		[TestDate(2012, 3, 16, 5, 5, 0)]
		public void TestReport_WhsTransactions()
		{
			Report_WhsTransactions();
		}

		#endregion

		#region TestReport_WhsTransactions_Transfers

		[TestDate(2012, 3, 16, 5, 5, 0)]
		public void TestReport_WhsTransactions_Transfers()
		{
			var whs1 = Helper.CreateWarehouse("WH1", "A", 2, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "B");
			var client = Helper.CreateClient("CLIENT");
			var part = Helper.CreateProduct(client, "P1");
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(client, whs1, "R1", part, 200m, whs1.FindLocation("A-1"), "");

			var transferInner = Helper.CreateWhsTransfer(client, whs1, "TR1", Notify, TransferType.Codes.Internal);
			Helper.CreateWhsTransferLine(transferInner, part, 10m, "A-1", "A-2");
			transferInner.FinaliseDocket();
			AssertIsFinalisedPrecondition(transferInner);

			var transferInterWhsSource =
				Helper.CreateWhsTransfer(client, whs1, "TR2", Notify, TransferType.Codes.InterWhsSource);
			var transferInterWhsSourceLine1 =
				Helper.CreateWhsTransferLine(transferInterWhsSource, part, 15m, "A-1", whs2.PK, "B");
			var transferInterWhsSourceLine2 =
				Helper.CreateWhsTransferLine(transferInterWhsSource, part, 20m, "A-1", whs2.PK, "B");
			var transferInterWhsSourceLine3 =
				Helper.CreateWhsTransferLine(transferInterWhsSource, part, 25m, "A-1", whs2.PK, "B");
			transferInterWhsSourceLine1.RunPreSaveValidation(); // to commit inventory
			transferInterWhsSourceLine2.FinaliseDocketLine();
			transferInterWhsSourceLine3.FinaliseDocketLine();
			AssertEquals("Precondition - ensure transfer line is not finalised.", false,
				transferInterWhsSourceLine1.IsFinalised);
			AssertIsFinalisedPrecondition(transferInterWhsSourceLine2);
			AssertIsFinalisedPrecondition(transferInterWhsSourceLine3);

			var transferInterWhsDest =
				Helper.CreateWhsTransfer(client, whs2, "TR3", Notify, TransferType.Codes.InterWhsDest);
			Helper.CreateWhsTransferLine(transferInterWhsDest, part, 35m, "A-1", whs1.PK, "B");
			transferInterWhsDest.FinaliseDocket();
			AssertIsFinalisedPrecondition(transferInterWhsDest);

			var now = ZDateTime.Now;
			// hack-change transfers finalised date to ensure that finalised dates from lines are used.
			transferInner.WD_FinalisedDate = whs1.GetWarehouseBranchDateTimeOffset(now.AddDays(4));
			transferInterWhsDest.WD_FinalisedDate = whs2.GetWarehouseBranchDateTimeOffset(now.AddDays(4));

			// hack-change transfer line finalised date to ensure date range is applied correctly.
			transferInterWhsSourceLine3.WE_FinalisedDate =
				transferInterWhsSourceLine3.ChildTransferLine.WE_FinalisedDate = whs2.GetWarehouseBranchDateTimeOffset(now.AddDays(4));
			transferInterWhsSourceLine3.PickLines.Single().WZ_PickedDateTime = whs2.GetWarehouseBranchDateTimeOffset(now.AddDays(4));

			Factory.Save();

			var results = Load_Report_WhsTransactions(now.AddDays(2), now.AddDays(5));
			AssertEquals("The only new transaction should be 1 transfer source Line + 1 matching transfer dest Line", 2,
				results.Count);
			AssertReport_WhsTransactionsLineMatch(transferInterWhsSourceLine3, 145m, results);
			AssertReport_WhsTransactionsLineMatch(transferInterWhsSourceLine3.ChildTransferLine, 55m, results);
		}

		#endregion

		#region TestReport_WhsTransactions_FinalisedOrderAndNotFinalisedPick

		[TestDate(2012, 8, 10, 5, 5, 0)]
		public void TestReport_WhsTransactions_FinalisedOrderAndNotFinalisedPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			Helper.CreateProductBOM(data.Part1, data.Part2, 2m, data.Part2.OP_StockKeepingUnit);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "OR1", new ZDateTimeOffset(2012, 1, 1),
				data.Part2, 10m);
			var orderPick = Helper.CreatePickNew(order);
			orderPick.FinaliseAllOrders();
			var orderLine = order.Lines.Single();
			orderLine.PickLines.Single().WZ_PickedDateTime = data.Whs1.GetWarehouseBranchDateTimeOffset(new ZDateTime(2012, 1, 1));

			orderLine.WE_FinalisedDate = order.WD_FinalisedDate;
			Factory.Save();

			AssertIsFinalisedPrecondition(order);
			AssertEquals("Precondition - ensure Pick is NOT Finalised.", false, orderPick.IsFinalised);

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.Part1, 20m);
			var workOrderPick = Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocket();
			AssertIsFinalisedPrecondition(workOrder);
			AssertEquals("Precondition - ensure Pick is NOT Finalised.", false, workOrderPick.IsFinalised);

			var finalisedDate = data.Whs1.GetWarehouseBranchDateTimeOffset(new ZDateTime(2012, 1, 1));
			workOrder.WD_FinalisedDate = finalisedDate; // hack to move Finalised Date outside of range.
			var pickLine = workOrderPick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = finalisedDate;
			foreach (var line in workOrder.AllLines)
			{
				line.WE_FinalisedDate = finalisedDate;
			}

			Factory.Save();

			// Although order/ work order are finalised, pick is not therefore doesn't matter when order / work order were finalised
			// those order / work order will be considered as still be in progress. Therefore, within current transaction history report period
			// Opening Balance = SOH (50) + 40 (WOR) + 10 (ORD) - 100 (INW) = 0
			var results = Load_Report_WhsTransactions(ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			AssertEquals("2 transactions expected: receive and empty line for Part1.", 2, results.Count);
			results.Cast<DynamicBusinessObject>().Single(l =>
				(ZDecimal)l["Units"] == 0m && (ZGuid)l["ProductPK"] == data.Part1.PK && (ZString)l["Reference"] == "");
			AssertReport_WhsTransactionsLineMatch(receive.Lines[0], 0m, results);
		}

		[TestDate(2012, 8, 10, 5, 5, 0)]
		public void TestReport_WhsTransactions_FinalisedOrderAndNotFinalisedPick_DymamicWorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			data.Whs1.WW_IsVirtualWarehouse = true;
			var inwardProcessingArea = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var inwardProcessingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "IPR", 1, 1).Locations[0];
			inwardProcessingLocation.WLV_WA_PutawayArea = inwardProcessingArea.PK;
			inwardProcessingLocation.WLV_WA_PickingArea = inwardProcessingArea.PK;
			var part3 = Helper.CreateProduct("P3234", data.Org1);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive1.WD_IsInwardsProcessingJob = true;
			receive1.WD_DocketSubType = ReceiveType.Codes.Customs;
			var recLine1 = Helper.CreateWhsReceiveLine(receive1, part3, 105m, inwardProcessingLocation);
			recLine1.CustomsData.WB_EntryKey = "ENT1";
			receive1.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 80m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "OR1", new ZDateTimeOffset(2012, 1, 1),
				data.Part2, 10m);
			var orderPick = Helper.CreatePickNew(order);
			orderPick.FinaliseAllOrders();
			var orderLine = order.Lines.Single();
			orderLine.PickLines.Single().WZ_PickedDateTime = data.Whs1.GetWarehouseBranchDateTimeOffset(new ZDateTime(2012, 1, 1));

			orderLine.WE_FinalisedDate = order.WD_FinalisedDate;
			Factory.Save();

			AssertIsFinalisedPrecondition(order);
			AssertEquals("Precondition - ensure Pick is NOT Finalised.", false, orderPick.IsFinalised);

			var workOrder = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1);
			workOrder.WD_RequiredDate = new ZDateTimeOffset(2012, 1, 1);

			var kitLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, data.Part1, 20m);
			kitLine.WE_F3_NKPackType = data.Part1.OP_StockKeepingUnit;
			kitLine.CustomsData.WB_IsMainInwardsProcessedItem = true;

			var componentLine = Helper.CreateWhsDynamicWorkOrderLine(workOrder, part3, 40m);
			componentLine.WE_F3_NKPackType = data.Part2.OP_StockKeepingUnit;
			componentLine.WE_WE_ParentDocketLine = kitLine.PK;

			var workOrderPick = Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocket();
			AssertIsFinalisedPrecondition(workOrder);
			AssertEquals("Precondition - ensure Pick is NOT Finalised.", false, workOrderPick.IsFinalised);

			var finalisedDate = data.Whs1.GetWarehouseBranchDateTimeOffset(new ZDateTime(2012, 1, 1));

			var pickLine = workOrderPick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = finalisedDate;

			workOrder.WD_FinalisedDate = finalisedDate; // hack to move Finalised Date outside of range.
			foreach (var line in workOrder.AllLines)
			{
				line.WE_FinalisedDate = finalisedDate;
			}

			workOrder.Receive.WD_FinalisedDate = finalisedDate; // hack to move Finalised Date outside of range.

			Factory.Save();

			// Although order/ work order are finalised, pick is not therefore doesn't matter when order / work order were finalised
			// those order / work order will be considered as still be in progress. Therefore, within current transaction history report period
			var results = Load_Report_WhsTransactions(ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			AssertEquals("3 transactions expected: 2 receives and empty line for Part1.", 3, results.Count);
			results.Cast<DynamicBusinessObject>().Single(l =>
				(ZDecimal)l["Units"] == 0m && (ZGuid)l["ProductPK"] == data.Part1.PK && (ZString)l["Reference"] == "");
			AssertReport_WhsTransactionsLineMatch(receive1.Lines[0], 0m, results);
			AssertReport_WhsTransactionsLineMatch(receive2.Lines[0], 0m, results);
		}

		#endregion

		#endregion

		#region TestReport_WhsTransaction_HideProductsWithNoTransactions

		[TestDate(2024, 4, 25, 6, 0, 0)]
		public void TestReport_WhsTransaction_HideProductsWithNoTransactions()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var result = Load_Report_WhsTransactions(ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10),
				hideProductsWithNoTransactions: false);
			AssertEquals("Include products with no transactions.", 2, result.Count);

			var result2 = Load_Report_WhsTransactions(ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10),
				hideProductsWithNoTransactions: true);
			AssertEquals("Exclude products with no transactions.", 0, result2.Count);
		}

		#endregion

		#region TestReport_WhsTransaction_DoesntShowReserveLines

		[TestDate(2024, 4, 25, 6, 0, 0)]
		public void TestReport_WhsTransaction_DoesntShowReserveLines()
		{
			var today = ZDateTime.Today;

			var client = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("WHS", "A", 1, 1);
			var part = Helper.CreateProduct(client, "P1");
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(client, whs, "R1", part, 10m);
			receive.WD_FinalisedDate = today.AddDays(-7).ToOffset(); // hack so it doesnt appear in the report
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(client, whs, part, 10m);
			AssertNotNull("Precondition - divot is created.",
				order.Lines[0].ReserveStockIfAbleTo(receive.Inventory[0]));
			Factory.Save();

			// Hack to test filter out lines with no Pick
			order.WD_DocketStatus = DocketStatus.Codes.Entered;
			Factory.Save();

			var results = Load_Report_WhsTransactions(today.AddDays(-1), today.AddDays(1));
			AssertEquals("Precondition", 1, results.Count);
			AssertEquals("Should not include cross docked divot.", 0m, results[0]["Units"]);
		}

		#endregion

		#region TestReport_WhsTransactions_ReleaseCapturedAttribs

		[TestDate(2024, 4, 25, 6, 0, 0)]
		public void TestReport_WhsTransactions_ReleaseCapturedAttribs()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, inventoryLocation,
				ZDate.Empty, ZDate.Empty, "", "BATCH123", "", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.Quantity = 6m;
			releaseLine1.PartAttribute1 = "RED";

			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			releaseLine2.Quantity = 4m;
			releaseLine2.PartAttribute2 = "BATCH123";
			releaseLine2.PartAttribute1 = "BLUE";

			order.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(order);
			Factory.Save();

			var now = ZDateTime.Now;
			var results =
				Load_Report_WhsTransactions(now.AddDays(-1), now.AddDays(1), hideProductsWithNoTransactions: true);
			AssertEquals("Should be 3 results, 1 receive Line + 2 Release Captured Attributes.", 3, results.Count);

			var receiveLine = results.Single(d => (ZDecimal)d["Units"] == 10m);
			var releaseCapturedLine1 = results.Single(d => (ZDecimal)d["Units"] == -6m);
			var releaseCapturedLine2 = results.Single(d => (ZDecimal)d["Units"] == -4m);
			AssertTransactionLine(receiveLine, receive, inventory.InDocketLine, string.Empty);
			AssertTransactionLineCore(releaseCapturedLine1, order, orderLine, 6m, string.Empty);
			AssertTransactionLineCore(releaseCapturedLine2, order, orderLine, 4m, string.Empty);
			AssertEquals("Part Attribute 1 does not match expected result.", "", receiveLine["PartAttrib1Met"]);
			AssertEquals("Part Attribute 1 does not match expected result.", "RED",
				releaseCapturedLine1["PartAttrib1Met"]);
			AssertEquals("Part Attribute 1 does not match expected result.", "BLUE",
				releaseCapturedLine2["PartAttrib1Met"]);
			AssertEquals("Part Attribute 2 does not match expected result.", "BATCH123", receiveLine["PartAttrib2Met"]);
			AssertEquals("Part Attribute 2 does not match expected result.", "BATCH123",
				releaseCapturedLine1["PartAttrib2Met"]);
			AssertEquals("Part Attribute 2 does not match expected result.", "BATCH123",
				releaseCapturedLine2["PartAttrib2Met"]);
		}

		[TestDate(2024, 4, 25, 6, 0, 0)]
		public void TestReport_WhsTransactions_ReleaseCapturedAttribs_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true,
				setReleaseCaptured: true);
			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, inventoryLocation,
				ZDate.Empty, ZDate.Empty, "", "BATCH123", "", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.Quantity = 1m;
			releaseLine1.PartAttribute1 = "RED";
			releaseLine1.SerialNumber = "SERA1";

			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			releaseLine2.Quantity = 1m;
			releaseLine2.PartAttribute2 = "BATCH123";
			releaseLine2.PartAttribute1 = "BLUE";
			releaseLine2.SerialNumber = "SERA2";

			order.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(order);
			Factory.Save();

			var now = ZDateTime.Now;
			var results =
				Load_Report_WhsTransactions(now.AddDays(-1), now.AddDays(1), hideProductsWithNoTransactions: true);
			AssertEquals("Should be 3 results, 1 receive Line + 2 Release Captured Attributes.", 3, results.Count);

			var receiveLine = results.Single(d => (ZDecimal)d["Units"] == 2m);
			var releaseCapturedLine1 =
				results.Single(d => (ZDecimal)d["Units"] == -1m && (ZString)d["SerialNumberMet"] == "SERA1");
			var releaseCapturedLine2 =
				results.Single(d => (ZDecimal)d["Units"] == -1m && (ZString)d["SerialNumberMet"] == "SERA2");
			AssertTransactionLine(receiveLine, receive, inventory.InDocketLine, string.Empty);
			AssertTransactionLineCore(releaseCapturedLine1, order, orderLine, 1m, string.Empty);
			AssertTransactionLineCore(releaseCapturedLine2, order, orderLine, 1m, string.Empty);
			AssertEquals("Part Attribute 1 does not match expected result.", "", receiveLine["PartAttrib1Met"]);
			AssertEquals("Part Attribute 1 does not match expected result.", "RED",
				releaseCapturedLine1["PartAttrib1Met"]);
			AssertEquals("Part Attribute 1 does not match expected result.", "BLUE",
				releaseCapturedLine2["PartAttrib1Met"]);
			AssertEquals("Part Attribute 2 does not match expected result.", "BATCH123", receiveLine["PartAttrib2Met"]);
			AssertEquals("Serial Number does not match expected result.", "SERA1",
				releaseCapturedLine1["SerialNumberMet"]);
			AssertEquals("Serial Number does not match expected result.", "SERA2",
				releaseCapturedLine2["SerialNumberMet"]);
			AssertEquals("Part Attribute 2 does not match expected result.", "BATCH123",
				releaseCapturedLine1["PartAttrib2Met"]);
			AssertEquals("Part Attribute 2 does not match expected result.", "BATCH123",
				releaseCapturedLine2["PartAttrib2Met"]);
		}

		#endregion

		#region TestReport_WhsTransaction_SerialNumber

		[TestDate(2024, 4, 25, 6, 0, 0)]
		public void TestReport_WhsTransaction_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);
			Factory.Save();

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			CreateReceiveLine("SN1");
			CreateReceiveLine("SN2");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var now = ZDateTime.Now;
			var results =
				Load_Report_WhsTransactions(now.AddDays(-1), now.AddDays(1), hideProductsWithNoTransactions: true);
			AssertEquals("Correct count", 2, results.Count);

			var actualLine1 = results[0];
			AssertEquals("Correct units", 1m, actualLine1["Units"]);
			AssertEquals("Correct SerialNumberMet", "SN1", actualLine1["SerialNumberMet"]);

			var actualLine2 = results[1];
			AssertEquals("Correct units", 1m, actualLine2["Units"]);
			AssertEquals("Correct SerialNumberMet", "SN2", actualLine2["SerialNumberMet"]);

			WhsReceiveLine CreateReceiveLine(string serialNumber)
			{
				var line = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, inventoryLocation);
				line.WE_SerialNumber = serialNumber;

				return line;
			}
		}

		#endregion

		#region TestReport_WhsTransactions_OpeningBalanceMatchWithStockBalanceReport

		[TestDate(2024, 4, 25, 6, 0, 0)]
		public void TestReport_WhsTransactions_OpeningBalanceMatchWithStockBalanceReport()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();
			var receiveLine = receive.Lines.Single();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 6m);

			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketAlwaysFinalisingPick();
			Factory.Save();

			AssertIsFinalisedPrecondition(pick);
			AssertEquals("Precondition: stock reduced by pick", 4m, receiveLine.WE_StockOnHand);
			Factory.Save();

			var now = ZDateTime.Now;
			var transactionsHistoryReport =
				Load_Report_WhsTransactions(now.AddDays(1), now.AddDays(2));
			AssertEquals("OpenBalUnits must be same for Transactions history report and stock balance report.", 4m,
				transactionsHistoryReport[0]["OpenBalUnits"]);

			var stockBalanceReport = LoadStockBalanceReport(now.AddDays(1));
			AssertEquals("OpenBalUnits must be same for Transactions history report and stock balance report.", 4m,
				stockBalanceReport.Single()["Quantity"]);

			DynamicBusinessObjectCollection LoadStockBalanceReport(ZDateTime date)
			{
				var result = new DynamicBusinessObjectCollection(Factory);

				var sqlParams = new ZSqlParameterCollection { { "@Date", date, WhsDocketSchema.WD_FinalisedDate } };
				var sql = "SELECT * FROM WhsStockBalancesReport(@Date, null) order by WarehouseName, ClientCode, ProductCode";
				result.Load(sql, sqlParams);

				return result;
			}
		}

		#endregion

		#region Test Implementations

		void Report_WhsTransactions()
		{
			var data = new WhsTransactionHistoryDataSetup();
			SetupTransactions(data);

			var outsideContext1 = "All Transactions";
			var results1 = Load_Report_WhsTransactions(data.Week[1].Date, data.Week[4].AddDays(-1).Date);
			Assert_Report_WhsTransactions(1, 3, data, results1, outsideContext1);

			var outsideContext2 = "2nd Week of Transactions only";
			var results2 = Load_Report_WhsTransactions(data.Week[2].Date, data.Week[3].AddDays(-1).Date);
			Assert_Report_WhsTransactions(2, 2, data, results2, outsideContext2);

			var outsideContext3 = "3rd Week of Transactions only";
			var results3 = Load_Report_WhsTransactions(data.Week[3].Date, data.Week[4].AddDays(-1).Date);
			Assert_Report_WhsTransactions(3, 3, data, results3, outsideContext3);

			var outsideContext4 = "Before First Date (No Transactions)";
			var results4 = Load_Report_WhsTransactions(data.Week[1].Date, data.Week[1].AddDays(-1).Date);
			AssertNullTransactions(1, data, results4, outsideContext4);

			var outsideContext5 = "After Last Date (No Transactions - Opening Balances)";
			var results5 = Load_Report_WhsTransactions(data.Week[4].Date, data.Week[4].Date);
			AssertNullTransactions(4, data, results5, outsideContext5);
		}

		#endregion

		#region Asserting Results

		void Assert_Report_WhsTransactions(int fromWeek, int toWeek, WhsTransactionHistoryDataSetup data,
			DynamicBusinessObjectCollection results, string outsideContext)
		{
			var resultCount = (toWeek - fromWeek + 1) * 32;
			AssertEquals(outsideContext + " Result Count", resultCount, results.Count);

			var cnt = 0;
			for (var whsID = 1; whsID <= 2; whsID++)
			{
				for (var clientID = 1; clientID <= 2; clientID++)
				{
					for (var prodID = 1; prodID <= 2; prodID++)
					{
						var openingBal = CalcOpeningBalance(whsID, clientID, prodID, fromWeek, data);

						for (var weekID = fromWeek; weekID <= toWeek; weekID++)
						{
							AssertTransactionLines(whsID, clientID, prodID, weekID, openingBal, ref cnt, data, results,
								outsideContext);
						}
					}
				}
			}
		}

		void AssertNullTransactions(int openBalBeforeWeek, WhsTransactionHistoryDataSetup data,
			DynamicBusinessObjectCollection results, string outsideContext)
		{
			AssertEquals(outsideContext + " Result count", 8, results.Count);

			var cnt = 0;
			for (var whsID = 1; whsID <= 2; whsID++)
			{
				for (var clientID = 1; clientID <= 2; clientID++)
				{
					for (var prodID = 1; prodID <= 2; prodID++)
					{
						var prod = (prodID == 1) ? clientID : 3;

						if (openBalBeforeWeek > 0)
						{
							var openingBal = CalcOpeningBalance(whsID, clientID, prodID, openBalBeforeWeek, data);
							AssertNullTransaction(results[cnt++], data.Warehouse[whsID].PK, data.Client[clientID].PK,
								data.Part[prod].PK, openingBal, outsideContext);
						}
						else
						{
							AssertNullTransaction(results[cnt++], data.Warehouse[whsID].PK, data.Client[clientID].PK,
								data.Part[prod].PK, outsideContext);
						}
					}
				}
			}
		}

		void AssertTransactionLines(int whsID, int clientID, int prodID, int weekID, decimal openingBal, ref int cnt,
			WhsTransactionHistoryDataSetup data, DynamicBusinessObjectCollection results, string outsideContext)
		{
			AssertTransactionLine(results[cnt++], data.Receive[whsID, clientID, weekID],
				data.ReceiveLine[whsID, clientID, weekID, prodID].InDocketLine, openingBal, outsideContext);
			AssertTransactionLine(results[cnt++], data.Order[whsID, clientID, weekID],
				data.OrderLine[whsID, clientID, weekID, prodID], openingBal, outsideContext);
			AssertTransactionLine(results[cnt++], data.Adjustment[whsID, clientID, weekID, 1],
				data.AdjustmentLine[whsID, clientID, weekID, 1, prodID], openingBal, outsideContext);
			AssertTransactionLine(results[cnt++], data.Adjustment[whsID, clientID, weekID, 2],
				data.AdjustmentLine[whsID, clientID, weekID, 2, prodID], openingBal, outsideContext);
		}

		void AssertTransactionLine(DynamicBusinessObject result, WhsDocket docket, WhsDocketLine line,
			string outsideContext)
		{
			AssertTransactionLineCore(result, docket, line, line.SumOfUnitsMet, outsideContext);
		}

		void AssertTransactionLineCore(DynamicBusinessObject result, WhsDocket docket, WhsDocketLine line,
			ZDecimal expectedQty, string outsideContext)
		{
			var sign = (docket.WD_DocketType == DocketType.Codes.Order) ? -1m : 1m;
			var context = outsideContext + " " + docket.WD_DocketType + " " + docket.WD_ExternalReference + " ";

			AssertEquals(context + "WarehousePK", docket.WD_WW_Whs, result["WarehousePK"]);
			AssertEquals(context + "ClientPK", docket.WD_OH_Client, result["ClientPK"]);
			AssertEquals(context + "ProductPK", line.WE_OP, result["ProductPK"]);
			AssertEquals(context + "Units", expectedQty * sign, result["Units"]);
		}

		void AssertTransactionLine(DynamicBusinessObject result, WhsDocket docket, WhsDocketLine line,
			ZDecimal openingBal, string outsideContext)
		{
			var context = outsideContext + " ";
			AssertTransactionLine(result, docket, line, outsideContext);
			AssertEquals(context + "OpenBalUnits", openingBal, result["OpenBalUnits"]);
		}

		void AssertNullTransaction(DynamicBusinessObject result, ZGuid whsPK, ZGuid clientPK, ZGuid prodPK,
			string outsideContext)
		{
			var context = outsideContext + " ";
			AssertEquals(context + "WarehousePK", whsPK, result["WarehousePK"]);
			AssertEquals(context + "ClientPK", clientPK, result["ClientPK"]);
			AssertEquals(context + "ProductPK", prodPK, result["ProductPK"]);
			AssertEquals(context + "Reference", "", result["Reference"]);
			AssertEquals(context + "FinalisedDate", ZDateTime.Empty, result["FinalisedDate"]);
			AssertEquals(context + "TranType", "", result["TranType"]);
			AssertEquals(context + "Units", 0m, result["Units"]);
		}

		void AssertNullTransaction(DynamicBusinessObject result, ZGuid whsPK, ZGuid clientPK, ZGuid prodPK,
			ZDecimal openingBal, string outsideContext)
		{
			string context = outsideContext + " ";
			AssertNullTransaction(result, whsPK, clientPK, prodPK, outsideContext);
			AssertEquals(context + "OpenBalUnits", openingBal, result["OpenBalUnits"]);
		}

		#region AssertLineMatch

		void AssertReport_WhsTransactionsLineMatch(WhsDocketLine expectedLine, ZDecimal expectedOpeningBalance,
			DynamicBusinessObjectCollection results)
		{
			var actualLine = results.Single(d =>
				(ZDecimal)d["OpenBalUnits"] == expectedOpeningBalance && (ZGuid)d["ProductPK"] == expectedLine.WE_OP);

			AssertEquals("OpenBalUnits", expectedOpeningBalance, actualLine["OpenBalUnits"]);
			AssertReport_WhsTransactionsFilterLineMatch(expectedLine, results);
		}

		void AssertReport_WhsTransactionsFilterLineMatch(WhsDocketLine expectedLine,
			DynamicBusinessObjectCollection results)
		{
			var docket = expectedLine.Docket;
			var client = docket.Client;
			var part = expectedLine.SupplierPart;
			var partRelation =
				part.RelatedOrganisations.FindByOrganisationAndRelationship(client,
					OrgPartRelation.RelationshipTypes.Owner);
			var category = partRelation.Category;
			var expectedCategoryCode = category != null ? category.OPC_CategoryCode : ZString.Empty;

			ZDecimal expectedUnits;
			IPartAttributes expectedAttributes;
			if (docket.WD_DocketType == DocketType.Codes.Order
				|| docket.WD_DocketType == DocketType.Codes.WorkOrder
				|| docket.WD_DocketType == DocketType.Codes.DynamicWorkOrder)
			{
				expectedAttributes = ((WhsPickableDocketLine)expectedLine).ReleaseLines[0];
				expectedUnits = -expectedLine.WE_TransactionQuantity;
			}
			else
			{
				expectedAttributes = expectedLine;
				expectedUnits =
					(docket.WD_DocketType == DocketType.Codes.Transfer &&
					 docket.WD_DocketSubType == TransferType.Codes.InterWhsSource)
						? (ZDecimal)(-expectedLine.WE_TransactionQuantity)
						: expectedLine.WE_TransactionQuantity;
			}

			var expectedPallets = (part.OP_StockKeepingUnitPerPallet != 0)
				? expectedUnits / part.OP_StockKeepingUnitPerPallet
				: 0m;
			var expectedFinalisedDate = (docket.WD_DocketType == DocketType.Codes.Transfer)
				? expectedLine.WE_FinalisedDate
				: docket.WD_FinalisedDate;
			var actualLine = results.Single(d => (ZDecimal)d["Units"] == expectedUnits);

			AssertEquals("WarehousePK", docket.WD_WW_Whs, actualLine["WarehousePK"]);
			AssertEquals("WarehouseName", docket.Warehouse.WW_WarehouseName, actualLine["WarehouseName"]);
			AssertEquals("ClientPK", docket.WD_OH_Client, actualLine["ClientPK"]);
			AssertEquals("ClientCode", client.OH_Code, actualLine["ClientCode"]);
			AssertEquals("Client", client.OH_FullName, actualLine["Client"]);
			AssertEquals("Reference", docket.WD_ExternalReference, actualLine["Reference"]);
			AssertEquals("TranType", docket.WD_DocketType, actualLine["TranType"]);
			AssertEquals("Product", part.OP_PartNum, actualLine["Product"]);
			AssertEquals("ProductDesc", part.OP_Desc, actualLine["ProductDesc"]);
			AssertEquals("ProductBrandName", part.OP_Brand, actualLine["ProductBrandName"]);
			AssertEquals("ProductModel", part.OP_Model, actualLine["ProductModel"]);
			AssertEquals("ProductPK", expectedLine.WE_OP, actualLine["ProductPK"]);
			AssertEquals("ProductCategoryCode", expectedCategoryCode, actualLine["ProductCategoryCode"]);
			AssertEquals("CommodityCode", part.OP_RH_NKCommodityCode, actualLine["CommodityCode"]);
			AssertEquals("PartAttrib1Met", expectedAttributes.PartAttrib1, actualLine["PartAttrib1Met"]);
			AssertEquals("PartAttrib2Met", expectedAttributes.PartAttrib2, actualLine["PartAttrib2Met"]);
			AssertEquals("PartAttrib3Met", expectedAttributes.PartAttrib3, actualLine["PartAttrib3Met"]);
			AssertEquals("PackingDateMet", expectedAttributes.PackingDate, actualLine["PackingDateMet"]);
			AssertEquals("SerialNumberMet", expectedAttributes.SerialNumber, actualLine["SerialNumberMet"]);
			AssertEquals("ExpiryDateMet", expectedAttributes.ExpiryDate, actualLine["ExpiryDateMet"]);
			AssertEquals("FinalisedDate", expectedFinalisedDate.ToString("dd-MMM-yyyy hh:mm"), ((ZDateTime)actualLine["FinalisedDate"]).ToString("dd-MMM-yyyy hh:mm"));
			AssertEquals("LineComment", expectedLine.WE_LineComment, actualLine["LineComment"]);
			AssertEquals("Units", expectedUnits, actualLine["Units"]);
			AssertEquals("Pallets", expectedPallets, actualLine["Pallets"]);
			AssertEquals("DocketId", docket.WD_DocketID, actualLine["DocketId"]);

			// Add custom line and docket attributes here.
		}

		#endregion

		#endregion

		#region Setup Data

		void SetupTransactions(WhsTransactionHistoryDataSetup data)
		{
			var now = ZDateTime.Now;
			for (var whsID = 1; whsID <= 2; whsID++)
			{
				// clients
				for (var clientID = 1; clientID <= 2; clientID++)
				{
					// weeks of transactions (for each client for each warehouse)
					for (var weekID = 1; weekID <= 3; weekID++)
					{
						var productID = clientID;
						var units = whsID * 1000m + clientID * 100m + weekID * 10m;
						var reference = whsID.ToString() + clientID.ToString() + weekID.ToString();

						// a set of finalised and unfinalised tranactions are created for each type
						// unfinalised tranactions are created to ensure relevant queries do not include them
						SetupReceive(whsID, clientID, weekID, productID, units, reference, data);
						SetupOrder(whsID, clientID, weekID, productID, units, reference, data);
						SetupAdjustment(whsID, clientID, weekID, productID, units, reference, data);
						SetupTransfer(whsID, clientID, weekID, productID, units, reference, data);
					}
				}
			}

			Factory.Save();
		}

		void SetupReceive(int whsID, int clientID, int weekID, int productID, decimal units, string reference,
			WhsTransactionHistoryDataSetup data)
		{
			// create finalised transaction
			var finalised = data.Receive[whsID, clientID, weekID] =
				Helper.CreateWhsReceive(data.Client[clientID].PK, data.Warehouse[whsID].PK, reference, Notify);
			var finaliseLine1 = data.ReceiveLine[whsID, clientID, weekID, 1] =
				Helper.CreateWhsReceiveInventoryLine(finalised, data.Part[productID], (units + 1m) * 2,
					data.Warehouse[whsID].FindLocation("A-1"));
			var finaliseLine2 = data.ReceiveLine[whsID, clientID, weekID, 2] =
				Helper.CreateWhsReceiveInventoryLine(finalised, data.Part[3], (units + 2m) * 2,
					data.Warehouse[whsID].FindLocation("A-1"));
			finalised.WD_ArrivalDate = data.Warehouse[whsID].GetWarehouseBranchDateTimeOffset(data.Week[weekID]);
			finalised.FinaliseDocket();
			finalised.WD_FinalisedDate = data.Warehouse[whsID].GetWarehouseBranchDateTimeOffset(data.Week[weekID]);

			// Add status changed inventory
			finaliseLine1.InDocketLine.HeldCodeChangeQuantity = units + 1m;
			finaliseLine1.InDocketLine.HeldCodeToChangeTo = "";
			finaliseLine1.InDocketLine.ChangeInventoryHeldCode(true);

			finaliseLine2.InDocketLine.HeldCodeChangeQuantity = units + 2m;
			finaliseLine2.InDocketLine.HeldCodeToChangeTo = "";
			finaliseLine2.InDocketLine.ChangeInventoryHeldCode(true);

			// create entered transaction (unfinalised)
			var entered = Helper.CreateWhsReceive(data.Client[clientID].PK, data.Warehouse[whsID].PK,
				reference + "(ENT)", Notify);
			Helper.CreateWhsReceiveInventoryLine(entered, data.Part[productID].PK, units + 1m);
			Helper.CreateWhsReceiveInventoryLine(entered, data.Part[3].PK, units + 2m);

			// create putaway transaction (unfinalised)
			var putaway = Helper.CreateWhsReceive(data.Client[clientID].PK, data.Warehouse[whsID].PK,
				reference + "(PUT)", Notify);
			Helper.CreateWhsReceiveInventoryLine(putaway, data.Part[productID].PK, units + 1m);
			Helper.CreateWhsReceiveInventoryLine(putaway, data.Part[3].PK, units + 2m);
			putaway.WD_ArrivalDate = data.Warehouse[whsID].GetWarehouseBranchDateTimeOffset(data.Week[weekID]);
			putaway.AllocateLocationsWithMock();
			Factory.Save();
		}

		void SetupOrder(int whsID, int clientID, int weekID, int productID, decimal units, string reference,
			WhsTransactionHistoryDataSetup data)
		{
			// create finalised transaction
			data.Order[whsID, clientID, weekID] = Helper.CreateWhsOrder(data.Client[clientID].PK,
				data.Warehouse[whsID].PK, reference, Notify);
			data.OrderLine[whsID, clientID, weekID, 1] = Helper.CreateWhsOrderLine(data.Order[whsID, clientID, weekID],
				data.Part[productID].PK, (units + 1m) / 2m);
			data.OrderLine[whsID, clientID, weekID, 2] = Helper.CreateWhsOrderLine(data.Order[whsID, clientID, weekID],
				data.Part[3].PK, (units + 2m) / 2m);
			Factory.Save();

			data.Order[whsID, clientID, weekID].WD_RequiredDate = ZDateTimeOffset.Now;
			var orders = new WhsOrder[1] { data.Order[whsID, clientID, weekID] };
			var pick = Helper.CreatePickNew(orders);
			pick.FinaliseOrder(data.Order[whsID, clientID, weekID]);
			pick.FinalisePick();
			data.Order[whsID, clientID, weekID].WD_RequiredDate = data.Week[1].AddDays(-1).ToOffset();
			data.Order[whsID, clientID, weekID].WD_FinalisedDate = data.Week[weekID].AddDays(1).ToOffset();
			Factory.Save();

			// create entered transaction (unfinalised)
			var entered = Helper.CreateWhsOrder(data.Client[clientID].PK, data.Warehouse[whsID].PK, reference + "(ENT)",
				Notify);
			Helper.CreateWhsOrderLine(entered, data.Part[productID].PK, (units + 1m) / 2m);
			Helper.CreateWhsOrderLine(entered, data.Part[3].PK, (units + 2m) / 2m);

			// create picked transaction (unfinalised)
			var picked = Helper.CreateWhsOrder(data.Client[clientID].PK, data.Warehouse[whsID].PK, reference + "(PICK)",
				Notify);
			Helper.CreateWhsOrderLine(picked, data.Part[productID].PK, (units + 1m) / 2m);
			Helper.CreateWhsOrderLine(picked, data.Part[3].PK, (units + 2m) / 2m);
			picked.WD_RequiredDate = ZDateTimeOffset.Now;
			orders[0] = picked;
			pick = Helper.CreatePickNew(orders);
			Factory.Save();
		}

		void SetupAdjustment(int whsID, int clientID, int weekID, int productID, decimal units, string reference,
			WhsTransactionHistoryDataSetup data)
		{
			// create finalised transaction (adjustment in)
			data.Adjustment[whsID, clientID, weekID, 1] = Helper.CreateWhsAdjustment(data.Client[clientID].PK,
				data.Warehouse[whsID].PK, reference + "1", Notify);
			data.AdjustmentLine[whsID, clientID, weekID, 1, 1] = Helper.CreateWhsAdjustmentLine(
				data.Adjustment[whsID, clientID, weekID, 1], data.Part[productID].PK, (units + 1m) / 2m, "A-1");
			data.AdjustmentLine[whsID, clientID, weekID, 1, 2] =
				Helper.CreateWhsAdjustmentLine(data.Adjustment[whsID, clientID, weekID, 1], data.Part[3].PK,
					(units + 2m) / 2m, "A-1");
			data.Adjustment[whsID, clientID, weekID, 1].FinaliseDocket();
			data.Adjustment[whsID, clientID, weekID, 1].WD_FinalisedDate = data.Warehouse[whsID].GetWarehouseBranchDateTimeOffset(data.Week[weekID]).AddDays(2);
			Factory.Save();

			// create finalised transaction (adjustment out)
			data.Adjustment[whsID, clientID, weekID, 2] = Helper.CreateWhsAdjustment(data.Client[clientID].PK,
				data.Warehouse[whsID].PK, reference + "2", Notify);
			data.AdjustmentLine[whsID, clientID, weekID, 2, 1] = Helper.CreateWhsAdjustmentLine(
				data.Adjustment[whsID, clientID, weekID, 2], data.Part[productID].PK, (units + 1m) / -2m, "A-1");
			data.AdjustmentLine[whsID, clientID, weekID, 2, 2] =
				Helper.CreateWhsAdjustmentLine(data.Adjustment[whsID, clientID, weekID, 2], data.Part[3].PK,
					(units + 2m) / -2m, "A-1");
			data.Adjustment[whsID, clientID, weekID, 2].FinaliseDocket();
			data.Adjustment[whsID, clientID, weekID, 2].WD_FinalisedDate = data.Warehouse[whsID].GetWarehouseBranchDateTimeOffset(data.Week[weekID]).AddDays(3);
			Factory.Save();

			// create entered transaction (unfinalised)
			var entered = Helper.CreateWhsAdjustment(data.Client[clientID].PK, data.Warehouse[whsID].PK,
				reference + "(ENT)", Notify);
			Helper.CreateWhsAdjustmentLine(entered, data.Part[productID].PK, (units + 1m) / 2m, "A-1");
			Helper.CreateWhsAdjustmentLine(entered, data.Part[3].PK, (units + 2m) / 2m, "A-1");
			Helper.CreateWhsAdjustmentLine(entered, data.Part[3].PK, (units + 2m) / 2m, "A-1").WE_IsOriginalInventory =
				true;
			Factory.Save();
		}

		void SetupTransfer(int whsID, int clientID, int weekID, int productID, decimal units, string reference,
			WhsTransactionHistoryDataSetup data)
		{
			data.Transfer[whsID, clientID, weekID, 1] = Helper.CreateWhsTransfer(data.Client[clientID].PK,
				data.Warehouse[whsID].PK, reference + "3", Notify);
			data.TransferLine[whsID, clientID, weekID, 1, 1] = Helper.CreateWhsTransferLine(
				data.Transfer[whsID, clientID, weekID, 1], data.Part[productID].PK, (units + 1m) / 2m, "A-1", "A-2");
			data.TransferLine[whsID, clientID, weekID, 1, 2] = Helper.CreateWhsTransferLine(
				data.Transfer[whsID, clientID, weekID, 1], data.Part[3].PK, (units + 2m) / 2m, "A-1", "A-2");
			data.Transfer[whsID, clientID, weekID, 1].FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(data.Transfer[whsID, clientID, weekID, 1]);

			data.Transfer[whsID, clientID, weekID, 1].WD_FinalisedDate = data.Warehouse[whsID].GetWarehouseBranchDateTimeOffset(data.Week[whsID]).AddDays(2);
			data.Transfer[whsID, clientID, weekID, 2] = Helper.CreateWhsTransfer(data.Client[clientID].PK,
				data.Warehouse[whsID].PK, reference + "4", Notify);
			Factory.Save();

			// create entered transaction (unfinalised)
			var entered = Helper.CreateWhsTransfer(data.Client[clientID].PK, data.Warehouse[whsID].PK,
				reference + "(ENT)", Notify);
			Helper.CreateWhsTransferLine(entered, data.Part[productID].PK, (units + 1m) / 2m, "A-1", "A-2");
			Helper.CreateWhsTransferLine(entered, data.Part[3].PK, (units + 2m) / 2m, "A-1", "A-2")
				.WE_IsOriginalInventory = true;
			entered.RunPreSaveValidation(); // to commit inventory
			Factory.Save();
		}

		#endregion

		#region Loading Views

		DynamicBusinessObjectCollection Load_Report_WhsTransactions(ZDateTime fromDate, ZDateTime toDate,
			bool ignoreInternalAdjustments = false, bool hideProductsWithNoTransactions = false)
		{
			var results = new DynamicBusinessObjectCollection(Factory);
			var sql = @"
SELECT
	*
FROM
	Report_WhsTransactions(null, null, null, null, null, @FromDate, @ToDate, @IgnoreInternalAdjustments, @HideProductsWithNoTransactions, 0)
ORDER BY
	WarehouseName,
	Client,
	Product,
	FinalisedDate,
	SerialNumberMet";

			var sqlParams = new ZSqlParameterCollection
			{
				{ "@FromDate", fromDate.AddHours(-1), WhsDocketSchema.WD_FinalisedDate },
				{ "@ToDate", toDate.AddHours(1), WhsDocketSchema.WD_FinalisedDate },
				{ "@IgnoreInternalAdjustments", ignoreInternalAdjustments, WhsDocketSchema.WD_CustomFlag1 },
				{ "@HideProductsWithNoTransactions", hideProductsWithNoTransactions, WhsDocketSchema.WD_CustomFlag1 }
			};

			results.Load(sql, sqlParams);
			return results;
		}

		#endregion

		#region Implementation

		decimal CalcOpeningBalance(int whsID, int clientID, int prodID, int beforeWeek,
			WhsTransactionHistoryDataSetup data)
		{
			var openingBal = 0m;

			for (var week = 1; week < beforeWeek; week++)
			{
				openingBal += data.ReceiveLine[whsID, clientID, week, prodID].InDocketLine.WE_TransactionQuantity;
				openingBal -= data.OrderLine[whsID, clientID, week, prodID].SumOfUnitsMet;
				openingBal += data.AdjustmentLine[whsID, clientID, week, 1, prodID].WE_TransactionQuantity;
				openingBal += data.AdjustmentLine[whsID, clientID, week, 2, prodID].WE_TransactionQuantity;
			}

			return openingBal;
		}

		#endregion

		#region WhsTransactionHistoryDataSetup class

		class WhsTransactionHistoryDataSetup : WhsTestCaseWithFactory
		{
			public WhsTransactionHistoryDataSetup()
			{
				var lastYear = ZDateTime.Now.AddYears(-1);
				Week = new ZDateTime[5]
				{
					ZDateTime.Empty, lastYear, lastYear.AddDays(7), lastYear.AddDays(14), lastYear.AddDays(21)
				};

				Warehouse = new WhsWarehouse[3];
				Warehouse[1] = Helper.CreateWarehouse("1", "A", 2, 1);
				Warehouse[2] = Helper.CreateWarehouse("2", "A", 2, 1);

				Client = new OrgHeader[3];
				Client[1] = Helper.CreateClient("WHSTC1", "CLIENT1");
				Client[2] = Helper.CreateClient("WHSTC2", "CLIENT2");

				Part = new OrgSupplierPart[4];
				Part[1] = Helper.CreateProduct(Client[1], "C1", OrgPartRelation.RelationshipTypes.Both);
				Part[2] = Helper.CreateProduct(Client[2], "C2");
				Part[3] = Helper.CreateProduct(Client[1], "C3");

				var relation = Part[3].RelatedOrganisations.AddNew();
				relation.OU_Relationship = "OWN";
				relation.OU_OP = Part[3].PK;
				relation.OU_OH = Client[2].PK;

				Receive = new WhsReceive[3, 3, 4];
				ReceiveLine = new WhsInventoryView[3, 3, 4, 3];

				Order = new WhsOrder[3, 3, 4];
				OrderLine = new WhsOrderLine[3, 3, 4, 3];

				Adjustment = new WhsAdjustment[3, 3, 4, 3];
				AdjustmentLine = new WhsAdjustmentLine[3, 3, 4, 3, 3];

				Transfer = new WhsTransfer[3, 3, 4, 3];
				TransferLine = new WhsTransferLine[3, 3, 4, 3, 3];

				Factory.Save();
			}

			public ZDateTime[] Week;
			public WhsWarehouse[] Warehouse;
			public OrgHeader[] Client;
			public OrgSupplierPart[] Part;
			public WhsReceive[,,] Receive;
			public WhsInventoryView[,,,] ReceiveLine;
			public WhsOrder[,,] Order;
			public WhsOrderLine[,,,] OrderLine;
			public WhsAdjustment[,,,] Adjustment;
			public WhsAdjustmentLine[,,,,] AdjustmentLine;
			public WhsTransfer[,,,] Transfer;
			public WhsTransferLine[,,,,] TransferLine;
		}

		#endregion
	}
}
