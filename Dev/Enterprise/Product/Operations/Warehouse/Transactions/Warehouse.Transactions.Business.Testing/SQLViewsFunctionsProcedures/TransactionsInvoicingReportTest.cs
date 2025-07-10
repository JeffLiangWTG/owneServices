using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class TransactionsInvoicingReportTest : WhsTestCaseWithFactory
	{
		public void TestLoadView()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, inventoryLocation);

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();

			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var results = LoadView(data.Part1.PK);
			AssertEquals("Should return 2 record.", 2, results.Count);
			AssertColumnValues("INW", 10m, results[0]);
			AssertColumnValues("ORD", -10m, results[1]);
		}

		public void TestLoadView_DynamicWorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var inwardProcessingArea = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var inwardProcessingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "IPR", 1, 1).Locations[0];
			inwardProcessingLocation.WLV_WA_PutawayArea = inwardProcessingArea.PK;
			inwardProcessingLocation.WLV_WA_PickingArea = inwardProcessingArea.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_IsInwardsProcessingJob = true;
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, inwardProcessingLocation.PK, "ENT1", "", 10m);

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var dynamicWorkOrder = Factory.New<WhsDynamicWorkOrder>();
			dynamicWorkOrder.WD_OH_Client = data.Org1.PK;
			dynamicWorkOrder.WD_WW_Whs = data.Whs1.PK;
			dynamicWorkOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var kitLine1 = Factory.New<WhsDynamicWorkOrderLine>();
			kitLine1.WE_WD = dynamicWorkOrder.PK;
			kitLine1.WE_OP = data.Part2.PK;
			kitLine1.WE_TransactionQuantity = 10m;
			kitLine1.WE_F3_NKPackType = data.Part2.OP_StockKeepingUnit;
			kitLine1.CustomsData.WB_IsMainInwardsProcessedItem = true;

			var componentLine1 = Factory.New<WhsDynamicWorkOrderLine>();
			componentLine1.WE_WD = dynamicWorkOrder.PK;
			componentLine1.WE_OP = data.Part1.PK;
			componentLine1.WE_TransactionQuantity = 10m;
			componentLine1.WE_F3_NKPackType = data.Part1.OP_StockKeepingUnit;
			componentLine1.WE_WE_ParentDocketLine = kitLine1.PK;

			Factory.Save();

			var pick = Helper.CreatePickNew(dynamicWorkOrder);
			pick.FinaliseAllOrders();
			pick.FinalisePick();

			AssertIsFinalisedPrecondition(dynamicWorkOrder);
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var results = LoadView(data.Part1.PK);
			AssertEquals("Should return 2 record.", 2, results.Count);
			AssertColumnValues("INW", 10m, results.Single(r => (ZString)r["TranType"] == "INW"));
			AssertColumnValues("DWO", -10m, results.Single(r => (ZString)r["TranType"] == "DWO"));
		}

		void AssertColumnValues(string tranType, ZDecimal totalUnits, DynamicBusinessObject resultRow)
		{
			AssertEquals("TranType does not match with expected value.", tranType, resultRow["TranType"]);
			AssertEquals("TotalUnits does not match with expected value.", totalUnits, resultRow["Units"]);
		}

		#region LoadView

		DynamicBusinessObjectCollection LoadView(ZGuid supplierPartPK)
		{
			var result = new DynamicBusinessObjectCollection(Factory);

			var sql =
				$@"Select * From dbo.vw_report_WhsTransactions Where ProductPK = '{supplierPartPK}' Order By TranType";
			result.Load(sql);

			return result;
		}

		#endregion
	}
}
