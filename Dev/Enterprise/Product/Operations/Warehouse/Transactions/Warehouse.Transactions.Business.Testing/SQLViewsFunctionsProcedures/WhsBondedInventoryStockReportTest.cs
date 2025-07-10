using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsBondedInventoryStockReportTest : WhsTestCaseWithFactory
	{
		public void TestNoSuffixToTARIC()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var bondedInwardsKey = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 1);
			CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R1", data.Part1, 12m, ZDateTimeOffset.Now.AddDays(-3), bondedInwardsKey, 120m, 360m, tariff: "1111111111");
			var results = LoadView();

			AssertEquals("Taric should not feature any suffix.", "1111111111", results[0]["TARIC"]);
		}

		public void TestAbsoluteValueForDutyUnsigned()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var bondedInwardsKey = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 1);
			CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R1", data.Part1, 12m, ZDateTimeOffset.Now.AddDays(-3), bondedInwardsKey, 120m, 360m);
			var order = CreateOrderWithOrderLineAndCustomsData(data.Whs1, data.Org1, "O1", data.Part1, 9m, bondedInwardsKey, 90m, 270m);
			CreatePickAndFinalise(order, ZDateTimeOffset.Today);

			var results = LoadView();

			AssertEquals("AbsoluteValueForDuty for order is correct and is unsigned integer.", 90m, results[1]["AbsoluteValueForDuty"]);
		}

		public void TestView_CustomsInwardsAndOutwardsActions()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var bondedInwardsKey = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 1);
			var receive1 = CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R1", data.Part1, 10m, ZDateTimeOffset.Now.AddDays(-3), bondedInwardsKey, 100m, 400m);
			Thread.Sleep(1000);
			var receive2 = CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R2", data.Part1, 8m, ZDateTimeOffset.Now.AddDays(-2), bondedInwardsKey, 80m, 320m);
			var order = CreateOrderWithOrderLineAndCustomsData(data.Whs1, data.Org1, "O1", data.Part1, 12m, bondedInwardsKey, 9m, 9m);

			CreatePickAndFinalise(order, ZDateTimeOffset.Today);

			var results = LoadView();

			AssertEquals("Should return 4 results, 2 for receives and 2 for the order.", 4, results.Count);
			AssertEquals("Direction is inwards.", "INWARDS", (ZString)results[0]["Direction"].ToString());
			AssertEquals("InventoryLine is correct.", (ZString)receive1.Lines[0].PK.ToString(), (ZString)results[0]["InventoryLine"].ToString());
			AssertEquals("InvoiceQty is correct and integer.", "10", (ZString)results[0]["RelativeInvoiceQtyIn"]);
			AssertEquals("Direction is outwards.", "OUTWARDS", (ZString)results[1]["Direction"].ToString());
			AssertEquals("InventoryLine is correct.", (ZString)receive1.Lines[0].PK.ToString(), (ZString)results[1]["InventoryLine"].ToString());
			AssertEquals("InvoiceQty is correct and integer.", "10", (ZString)results[1]["RelativeInvoiceQtyOut"]);
			AssertEquals("Direction is inwards.", "INWARDS", (ZString)results[2]["Direction"].ToString());
			AssertEquals("InventoryLine is correct.", (ZString)receive2.Lines[0].PK.ToString(), (ZString)results[2]["InventoryLine"].ToString());
			AssertEquals("InvoiceQty is correct and integer.", "8", (ZString)results[2]["RelativeInvoiceQtyIn"]);
			AssertEquals("Direction is outwards.", "OUTWARDS", (ZString)results[3]["Direction"].ToString());
			AssertEquals("InventoryLine is correct.", (ZString)receive2.Lines[0].PK.ToString(), (ZString)results[3]["InventoryLine"].ToString());
			AssertEquals("InvoiceQty is correct and integer.", "2", (ZString)results[3]["RelativeInvoiceQtyOut"]);
		}

		public void TestView_FilterByWarehouseAddress()
		{
			var whsOrg1 = Helper.CreateClient("C1");
			var whsOrg2 = Helper.CreateClient("C2");
			var client = Helper.CreateClient("C3");

			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			address1.OA_Address1 = "1 BOND ST";
			address1.OA_OH = whsOrg1.PK;

			var address2 = Factory.NewWithValidTestData<OrgAddress>();
			address2.OA_Address1 = "2 BOND ST";
			address2.OA_OH = whsOrg2.PK;

			var whs1 = Helper.CreateWarehouse("WH1");
			whs1.WW_OA_WarehouseAddress = address1.PK;
			Helper.EnableWarehouseForBond(whs1, true);

			var whs2 = Helper.CreateWarehouse("WH2");
			whs2.WW_OA_WarehouseAddress = address2.PK;
			Helper.EnableWarehouseForBond(whs2, true);

			var product = Helper.CreateProduct(client, "P1");

			var receive = Helper.CreateWhsReceive(client, whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, product, 10m, whs1.DefaultLocation);
			receive.RunPreSaveValidation();
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var receiveLine = inventory.InDocketLine;

			var bondedAttribute = Factory.New<WhsBondedWarehouseAttribute>();
			bondedAttribute.WB_EntryKey = "ABC123";
			bondedAttribute.SetParent(receiveLine);
			bondedAttribute.WB_AddInfo = "EntryStatus=ABC";
			Factory.Save();
			var result = LoadView(address1.PK);
			AssertEquals("There should be no lines as there are no lines that are finalized.", 0, result.Count);

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();
			result = LoadView(address1.PK);
			AssertEquals("There should be one receive line related to whs1.", 1, result.Count);

			bondedAttribute.WB_AddInfo = "";
			Factory.Save();
			result = LoadView(address1.PK);
			AssertEquals("There should be no lines as entry status of all lines is empty.", 0, result.Count);

			bondedAttribute.WB_AddInfo = "EntryStatus=ABC";
			Factory.Save();

			result = LoadView(address1.PK);
			AssertEquals("There should be one receive line related to whs1.", 1, result.Count);

			result = LoadView(address2.PK);
			AssertEquals("There should be no lines as we specified address of a different warehouse.", 0, result.Count);

			result = LoadView();
			AssertEquals("There should be one lines as we didn't specified filter by address.", 1, result.Count);
		}

		public void TestView_FilterByProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var bondedInwardsKey = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 1);
			CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R1", data.Part1, 10m, ZDateTimeOffset.Today, bondedInwardsKey, 100m, 100m);
			var order = CreateOrderWithOrderLineAndCustomsData(data.Whs1, data.Org1, "O1", data.Part1, 8m, bondedInwardsKey, 99m, 99m);
			CreatePickAndFinalise(order, ZDateTimeOffset.Today);
			var attribute = order.Lines.FirstOrDefault(x => x.WE_BondedEntryKey == bondedInwardsKey).CustomsData;
			var inwardProcedure = "ABC";
			attribute.WB_InwardProcedure = inwardProcedure;

			var bondedInwardsKey2 = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 2);
			CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R2", data.Part2, 10m, ZDateTimeOffset.Today, bondedInwardsKey2, 100m, 100m);
			var order2 = CreateOrderWithOrderLineAndCustomsData(data.Whs1, data.Org1, "O2", data.Part2, 8m, bondedInwardsKey2, 99m, 99m);
			CreatePickAndFinalise(order2, ZDateTimeOffset.Today);
			var attribute2 = order2.Lines.FirstOrDefault(x => x.WE_BondedEntryKey == bondedInwardsKey2).CustomsData;
			var inwardProcedure2 = "123";
			attribute2.WB_InwardProcedure = inwardProcedure2;

			Factory.Save();

			var result = LoadView(productPK: data.Part1.PK);
			AssertEquals("There should be one receive line related to whs1.", 2, result.Count);

			AssertEquals("LINEENTRYKEY-1", (ZString)result[0]["EntryNumber"]);
			AssertEquals((ZShort)1, (ZShort)result[0]["InwardsEntryLineNo"]);
			AssertEquals("LINEENTRYKEY-1", (ZString)result[1]["EntryNumber"]);
			AssertEquals((ZShort)1, (ZShort)result[1]["InwardsEntryLineNo"]);
		}

		public void TestView_FilterByInwardEntry()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var bondedInwardsKey = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 1);
			CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R1", data.Part1, 10m, ZDateTimeOffset.Today, bondedInwardsKey, 100m, 100m);
			var order = CreateOrderWithOrderLineAndCustomsData(data.Whs1, data.Org1, "O1", data.Part1, 8m, bondedInwardsKey, 99m, 99m);
			CreatePickAndFinalise(order, ZDateTimeOffset.Today);
			var attribute = order.Lines.FirstOrDefault(x => x.WE_BondedEntryKey == bondedInwardsKey).CustomsData;
			var inwardProcedure = "ABC";
			attribute.WB_InwardProcedure = inwardProcedure;

			var bondedInwardsKey2 = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 2);
			CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R2", data.Part2, 10m, ZDateTimeOffset.Today, bondedInwardsKey2, 100m, 100m);
			var order2 = CreateOrderWithOrderLineAndCustomsData(data.Whs1, data.Org1, "O2", data.Part2, 8m, bondedInwardsKey2, 99m, 99m);
			CreatePickAndFinalise(order2, ZDateTimeOffset.Today);
			var attribute2 = order2.Lines.FirstOrDefault(x => x.WE_BondedEntryKey == bondedInwardsKey2).CustomsData;
			var inwardProcedure2 = "123";
			attribute2.WB_InwardProcedure = inwardProcedure2;

			Factory.Save();

			var result = LoadView(inwardsEntryKey: bondedInwardsKey);
			AssertEquals("There should be one receive line related to whs1.", 2, result.Count);

			AssertEquals("LINEENTRYKEY-1", (ZString)result[0]["EntryNumber"]);
			AssertEquals((ZShort)1, (ZShort)result[0]["InwardsEntryLineNo"]);
			AssertEquals("LINEENTRYKEY-1", (ZString)result[1]["EntryNumber"]);
			AssertEquals((ZShort)1, (ZShort)result[1]["InwardsEntryLineNo"]);
		}

		public void TestView_FilterByOutwardEntry()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var bondedInwardsKey = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 1);
			CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R1", data.Part1, 10m, ZDateTimeOffset.Today, bondedInwardsKey, 100m, 100m);
			var order = CreateOrderWithOrderLineAndCustomsData(data.Whs1, data.Org1, "O1", data.Part1, 8m, bondedInwardsKey, 99m, 99m);
			CreatePickAndFinalise(order, ZDateTimeOffset.Today);
			var attribute = order.Lines.FirstOrDefault(x => x.WE_BondedEntryKey == bondedInwardsKey).CustomsData;
			var inwardProcedure = "ABC";
			attribute.WB_InwardProcedure = inwardProcedure;

			var bondedInwardsKey2 = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 2);
			CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R2", data.Part2, 10m, ZDateTimeOffset.Today, bondedInwardsKey2, 100m, 100m);
			var order2 = CreateOrderWithOrderLineAndCustomsData(data.Whs1, data.Org1, "O2", data.Part2, 8m, bondedInwardsKey2, 99m, 99m);
			CreatePickAndFinalise(order2, ZDateTimeOffset.Today);
			var attribute2 = order2.Lines.FirstOrDefault(x => x.WE_BondedEntryKey == bondedInwardsKey2).CustomsData;
			var inwardProcedure2 = "123";
			attribute2.WB_InwardProcedure = inwardProcedure2;

			Factory.Save();

			var result = LoadView(outwardsEntryKey: bondedInwardsKey);
			AssertEquals("There should be no lines as the outward line should not be printed separately when the related inward line does not meet the requirements.", 0, result.Count);
		}

		public void TestView_FilterByJob()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var bondedInwardsKey = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 1);
			CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R1", data.Part1, 10m, ZDateTimeOffset.Today, bondedInwardsKey, 100m, 100m, "B1001");
			var order = CreateOrderWithOrderLineAndCustomsData(data.Whs1, data.Org1, "O1", data.Part1, 8m, bondedInwardsKey, 99m, 99m);
			CreatePickAndFinalise(order, ZDateTimeOffset.Today);
			var attribute = order.Lines.FirstOrDefault(x => x.WE_BondedEntryKey == bondedInwardsKey).CustomsData;
			var inwardProcedure = "ABC";
			attribute.WB_InwardProcedure = inwardProcedure;

			var bondedInwardsKey2 = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 2);
			CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R2", data.Part2, 10m, ZDateTimeOffset.Today, bondedInwardsKey2, 100m, 100m, "B1002");
			var order2 = CreateOrderWithOrderLineAndCustomsData(data.Whs1, data.Org1, "O2", data.Part2, 8m, bondedInwardsKey2, 99m, 99m);
			CreatePickAndFinalise(order2, ZDateTimeOffset.Today);
			var attribute2 = order2.Lines.FirstOrDefault(x => x.WE_BondedEntryKey == bondedInwardsKey2).CustomsData;
			var inwardProcedure2 = "123";
			attribute2.WB_InwardProcedure = inwardProcedure2;

			Factory.Save();

			var result = LoadView(jobNo: "B1001");
			AssertEquals("There should be one receive line related to whs1.", 1, result.Count);
			AssertEquals("LINEENTRYKEY-1", (ZString)result[0]["EntryNumber"]);
		}

		public void TestView_FilterByCustomsDeadline()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var bondedInwardsKey = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 1);
			var receive1 = CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R1", data.Part1, 10m, ZDateTimeOffset.Now.AddDays(-3), bondedInwardsKey, 100m, 400m);
			var order1 = CreateOrderWithOrderLineAndCustomsData(data.Whs1, data.Org1, "O1", data.Part1, 12m, bondedInwardsKey, 9m, 9m);
			CreatePickAndFinalise(order1, ZDateTimeOffset.Now.AddDays(-2));

			var bondedInwardsKey2 = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 2);
			var receive2 = CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R2", data.Part2, 10m, ZDateTimeOffset.Now.AddHours(-2), bondedInwardsKey2, 200m, 800m);
			var order2 = CreateOrderWithOrderLineAndCustomsData(data.Whs1, data.Org1, "O2", data.Part2, 20m, bondedInwardsKey2, 9m, 9m);
			CreatePickAndFinalise(order2, ZDateTimeOffset.Now);

			var bondedInwardsKey3 = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 3);
			var receive3 = CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R3", data.Part2, 10m, ZDateTimeOffset.Now.AddDays(-10), bondedInwardsKey3, 200m, 800m);
			var order3 = CreateOrderWithOrderLineAndCustomsData(data.Whs1, data.Org1, "O3", data.Part2, 20m, bondedInwardsKey3, 9m, 9m);
			CreatePickAndFinalise(order3, ZDateTimeOffset.Now);

			Factory.Save();

			var startDate = ZSqlParameter.New("@p792", ZDate.Today.AddDays(-1),
				WhsBondedWarehouseAttributeSchema.WB_CustomsDeadline);
			var endDate = ZSqlParameter.New("@p793", ZDate.Today.AddDays(1),
				WhsBondedWarehouseAttributeSchema.WB_CustomsDeadline);
			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(
				"select * from WhsBondedInventoryStockReport(null, null, null, null, null, null, null, null, null, null, null , null, null, @p792, @p793) ORDER BY MinEntryDate, PK, EntryRow",
				new[] { startDate, endDate });
			AssertEquals("There should be one receive line related to whs1.", 4, result.Count);

			AssertEquals((ZString)"LINEENTRYKEY-3", (ZString)result[0]["InwardsEntryNo"]);
			AssertEquals((ZString)"LINEENTRYKEY-3", (ZString)result[1]["OutwardsEntryNo"]);
			AssertEquals((ZString)"LINEENTRYKEY-2", (ZString)result[2]["InwardsEntryNo"]);
			AssertEquals((ZString)"LINEENTRYKEY-2", (ZString)result[3]["OutwardsEntryNo"]);
		}

		public void TestView_FilterByInwardStyle()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var bondedInwardsKey = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 1);
			var receive1 = CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R1", data.Part1, 10m, ZDateTimeOffset.Now.AddDays(-3), bondedInwardsKey, 100m, 400m);
			Thread.Sleep(1000);
			var receive2 = CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R2", data.Part1, 8m, ZDateTimeOffset.Now.AddDays(-2), bondedInwardsKey, 80m, 320m);
			var order = CreateOrderWithOrderLineAndCustomsData(data.Whs1, data.Org1, "O1", data.Part1, 12m, bondedInwardsKey, 9m, 9m);
			CreatePickAndFinalise(order, ZDateTimeOffset.Today);
			var inwardStyle = "ABC";
			receive1.Lines[0].CustomsData.WB_InwardStyle = inwardStyle;
			receive1.Lines[0].WE_DocketLineStatus = "FIN";
			var inwardStyle2 = "123";
			receive2.Lines[0].CustomsData.WB_InwardStyle = inwardStyle2;
			receive2.Lines[0].WE_DocketLineStatus = "FIN";

			Factory.Save();

			var parameterForInwardStyle = ZSqlParameter.New("@CustomsInwardStyle", inwardStyle, WhsBondedWarehouseAttributeSchema.WB_InwardStyle);
			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(
				"select * from WhsBondedInventoryStockReport(null, null, null, null, null, null, null, null, null, null, null , @CustomsInwardStyle, null, null, null)",
				new[] { parameterForInwardStyle });
			AssertEquals("There should be one receive line related to whs1.", 1, result.Count);

			AssertEquals("LINEENTRYKEY-1", (ZString)result.Single()["InwardsEntryNo"]);
			AssertEquals((ZShort)1, (ZShort)result.Single()["InwardsEntryLineNo"]);
		}

		public void TestView_FilterByInwardProcedure()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var bondedInwardsKey = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 1);
			var receive1 = CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R1", data.Part1, 10m, ZDateTimeOffset.Now.AddDays(-3), bondedInwardsKey, 100m, 400m);
			Thread.Sleep(1000);
			var receive2 = CreateReceiveWithInventoryAndCustomsData(data.Whs1, data.Org1, "R2", data.Part1, 8m, ZDateTimeOffset.Now.AddDays(-2), bondedInwardsKey, 80m, 320m);
			var order = CreateOrderWithOrderLineAndCustomsData(data.Whs1, data.Org1, "O1", data.Part1, 12m, bondedInwardsKey, 9m, 9m);
			CreatePickAndFinalise(order, ZDateTimeOffset.Today);
			var inwardProcedure = "ABC";
			receive1.Lines[0].CustomsData.WB_InwardProcedure = inwardProcedure;
			receive1.Lines[0].WE_DocketLineStatus = "FIN";
			var inwardProcedure2 = "123";
			receive2.Lines[0].CustomsData.WB_InwardProcedure = inwardProcedure2;
			receive2.Lines[0].WE_DocketLineStatus = "FIN";

			Factory.Save();

			var result = LoadView(inwardProcedure: inwardProcedure2);
			AssertEquals("There should be one receive line related to whs1.", 1, result.Count);

			AssertEquals("LINEENTRYKEY-1", (ZString)result.Single()["InwardsEntryNo"]);
			AssertEquals((ZShort)1, (ZShort)result.Single()["InwardsEntryLineNo"]);
		}

		WhsOrder CreateOrderWithOrderLineAndCustomsData(WhsWarehouse warehouse, OrgHeader org, string docketNumber,
			OrgSupplierPart part, decimal units, string bondedEntryKey, decimal bondedValueForDuty,
			decimal bondedCustomsQty)
		{
			var order = Helper.CreateWhsOrder(org, warehouse, docketNumber);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			CreateOrderLineWithCustomsData(order, part, units, bondedEntryKey, bondedValueForDuty, bondedCustomsQty);
			Factory.Save();
			return order;
		}

		WhsOrderLine CreateOrderLineWithCustomsData(WhsOrder order, OrgSupplierPart part, decimal units,
			string bondedEntryKey, decimal bondedValueForDuty, decimal bondedCustomsQty)
		{
			var orderLine = Helper.CreateWhsOrderLine(order, part, units);
			orderLine.CustomsData.WB_EntryLineNo = 1;
			orderLine.CustomsData.WB_EntryKey = bondedEntryKey;
			orderLine.CustomsData.WB_ValueForDuty = bondedValueForDuty;
			orderLine.CustomsData.WB_CustomsQty = bondedCustomsQty;
			orderLine.CustomsData.WB_AddInfo = "EntryStatus=ABC";
			orderLine.WE_BondedEntryKey = bondedEntryKey;

			return orderLine;
		}

		void CreatePickAndFinalise(WhsOrder order, ZDateTimeOffset orderFinalisedDate)
		{
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseOrder(order);
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);
			order.WD_FinalisedDate = orderFinalisedDate;
			Factory.Save();
		}

		protected WhsReceive CreateReceiveWithInventoryAndCustomsData(WhsWarehouse warehouse, OrgHeader org, string docketNumber,
			OrgSupplierPart part, decimal units, ZDateTimeOffset finalisedDate, string bondedEntryKey,
			decimal bondedValueOfDuty, decimal bondedCustomsQty, string declarationReference = "", string tariff = "", bool isInwardProcessingJob = false)
		{
			var areaType = isInwardProcessingJob ? AreaTypes.Codes.InwardProcessing : AreaTypes.Codes.Bonded;
			var location = warehouse.DefaultLocation;
			var area = warehouse.Areas.Single(a => a.WA_AreaType == areaType);
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(org.PK, warehouse.PK, docketNumber, finalisedDate);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive.WD_IsInwardsProcessingJob = isInwardProcessingJob;
			CreateReceiveLineWithCustomsData(receive, part, units, bondedEntryKey, bondedValueOfDuty, bondedCustomsQty, location, declarationReference, tariff);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			receive.WD_FinalisedDate = finalisedDate;
			Factory.Save();

			return receive;
		}

		WhsReceiveLine CreateReceiveLineWithCustomsData(WhsReceive receive, OrgSupplierPart part, decimal units,
			string bondedEntryKey, decimal bondedValueOfDuty, decimal bondedCustomsQty, WhsLocation location, ZString declarationReference, ZString tariff)
		{
			var receiveLine = Helper.CreateWhsReceiveLine(receive, part, units, location);
			receiveLine.CustomsData.WB_EntryLineNo = 1;
			receiveLine.CustomsData.WB_EntryKey = bondedEntryKey;
			receiveLine.CustomsData.WB_ValueForDuty = bondedValueOfDuty;
			receiveLine.CustomsData.WB_CustomsQty = bondedCustomsQty;
			receiveLine.CustomsData.WB_DeclarationReference = declarationReference;
			receiveLine.CustomsData.WB_Tariff = tariff;
			receiveLine.CustomsData.WB_AddInfo = "EntryStatus=ABC";
			receiveLine.WE_BondedEntryKey = bondedEntryKey;
			return receiveLine;
		}

		#region LoadView

		DynamicBusinessObjectCollection LoadView(
			ZGuid? warehouseAddressPK = null,
			ZGuid? warehousePK = null,
			ZGuid? clientPK = null,
			string inwardsEntryKey = null,
			int? inwardsEntryLineNo = null,
			string outwardsEntryKey = null,
			int? outwardsEntryLineNo = null,
			ZGuid? productPK = null,
			ZGuid? commodityPK = null,
			string jobNo = null,
			string partAttr = null,
			string inwardStyle = null,
			string inwardProcedure = null,
			ZDateTimeOffset? from = null,
			ZDateTimeOffset? to = null)
		{
			var result = new DynamicBusinessObjectCollection(Factory);

			var sql = @"select * from WhsBondedInventoryStockReport(";
			sql += (!warehouseAddressPK.HasValue || warehouseAddressPK.Value.IsEmpty ? "null, " : $"{warehouseAddressPK.Value.ToSqlGuid()}, ");
			sql += (!warehousePK.HasValue || warehousePK.Value.IsEmpty ? "null, " : $"{warehousePK.Value.ToSqlGuid()}, ");
			sql += (!clientPK.HasValue || clientPK.Value.IsEmpty ? "null, " : $"{clientPK.Value.ToSqlGuid()}, ");
			sql += (string.IsNullOrEmpty(inwardsEntryKey) ? "null, " : $"'{inwardsEntryKey}', ");
			sql += (!inwardsEntryLineNo.HasValue ? "null, " : $"{inwardsEntryLineNo}, ");
			sql += (string.IsNullOrEmpty(outwardsEntryKey) ? "null, " : $"'{outwardsEntryKey}', ");
			sql += (!outwardsEntryLineNo.HasValue ? "null, " : $"{outwardsEntryLineNo}, ");
			sql += (!productPK.HasValue || productPK.Value.IsEmpty ? "null, " : $"{productPK.Value.ToSqlGuid()}, ");
			sql += (!commodityPK.HasValue || commodityPK.Value.IsEmpty ? "null, " : $"{commodityPK.Value.ToSqlGuid()}, ");
			sql += (string.IsNullOrEmpty(jobNo) ? "null, " : $"'{jobNo}', ");
			sql += (string.IsNullOrEmpty(partAttr) ? "null, " : $"'{partAttr}', ");
			sql += (string.IsNullOrEmpty(inwardStyle) ? "null, " : $"'{inwardStyle}', ");
			sql += (string.IsNullOrEmpty(inwardProcedure) ? "null," : $"'{inwardProcedure}', ");
			sql += (!from.HasValue ? "null," : $"'{from.Value.ToShortDateString()}', ");
			sql += (!to.HasValue ? "null)" : $"'{to.Value.ToShortDateString()}')");
			sql += " order by MinEntryDate, PK, EntryRow";

			result.Load(sql);

			return result;
		}

		#endregion
	}
}
