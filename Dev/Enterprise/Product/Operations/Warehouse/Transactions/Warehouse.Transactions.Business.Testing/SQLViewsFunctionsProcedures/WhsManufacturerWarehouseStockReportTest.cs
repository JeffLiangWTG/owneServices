using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsManufacturerWarehouseStockReportTest : WhsBondedInventoryStockReportTest
	{
		[GuiTest]
		public void TestView_OutwardsForWorkOrderShouldBeIncluded()
		{
			var productData = new TestDataForBOM(Factory);
			productData.CreateBOMProducts(true);
			productData.Whs1.WW_IsVirtualWarehouse = true;
			Helper.EnableWarehouseForBond(productData.Whs1, true);
			var area = Helper.CreateArea(productData.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(productData.Whs1, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			Factory.Save();

			var bondedInwardsKey = WhsBondedWarehouseAttribute.BuildKey("LINEENTRYKEY", 1);
			var receive1 = CreateReceiveWithInventoryAndCustomsData(productData.Whs1, productData.Org1, "R3", productData.BOM.Polish, 10m, ZDateTimeOffset.Now.AddDays(-3), bondedInwardsKey, 10m, 10m, isInwardProcessingJob: true);
			var receive2 = CreateReceiveWithInventoryAndCustomsData(productData.Whs1, productData.Org1, "R2", productData.BOM.WheelRim, 30m, ZDateTimeOffset.Now.AddDays(-3), bondedInwardsKey, 20m, 30m, isInwardProcessingJob: true);
			var receive3 = CreateReceiveWithInventoryAndCustomsData(productData.Whs1, productData.Org1, "R1", productData.BOM.WheelTyre, 10m, ZDateTimeOffset.Now.AddDays(-3), bondedInwardsKey, 500m, 10m, isInwardProcessingJob: true);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(productData.Org1, productData.Whs1, "ExtRef");
			Helper.CreateWhsWorkOrderLine(workOrder, productData.BOM.BikeWheel, 2m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			workOrder.WD_IsInwardsProcessingJob = true;

			var productParam = workOrder.Lines[0].Product.ParamsByWhsAndClient.AddNew();
			productParam.W3_OH = productData.Org1.PK;
			productParam.W3_WW = productData.Whs1.PK;
			productParam.W3_WL_InwardsProcessingStagingLocationBOM = location.PK;

			var pick = Helper.CreatePickNew(WhsPickOption.Codes.Auto, workOrder);
			workOrder.FinaliseDocket();
			Factory.Save();

			var receiveLines = workOrder.Receive.Lines;
			receiveLines[0].WE_AllocationKey = "ABC";
			receiveLines[0].WE_WL = location.PK;
			workOrder.Receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(workOrder.Receive);
			Factory.Save();
			var results = LoadView();

			AssertEquals("Should return 6 results, 3 for receives and 3 for the work order.", 6, results.Count);

			CombineAssertions("The 1sy line of this report, represents the inwards action of 10 Polish." ,() =>
			{
				var result1 = results[0];
				AssertEquals("Direction is inwards.", "INWARDS", (ZString)result1["Direction"].ToString());
				AssertEquals("InventoryLine is correct.", (ZString)receive1.Lines[0].PK.ToString(), (ZString)result1["InventoryLine"].ToString());
				AssertEquals("InvoiceQty is correct and integer.", "10", (ZString)result1["RelativeInvoiceQtyIn"]);
			});

			CombineAssertions("The 2nd line of this report, represents the outwards action of 2 Polish, for the assembly of the work order.", () =>
			{
				var result2 = results[1];
				AssertEquals("Direction is outwards.", "OUTWARDS", (ZString)result2["Direction"].ToString());
				AssertEquals("InventoryLine is correct.", (ZString)receive1.Lines[0].PK.ToString(), (ZString)result2["InventoryLine"].ToString());
				AssertEquals("InvoiceQty is correct and integer.", "2", (ZString)result2["RelativeInvoiceQtyOut"]);
			});

			CombineAssertions("The 3rd line of this report, represents the inwards action of 30 WheelRim.", () =>
			{
				var result3 = results[2];
				AssertEquals("Direction is inwards.", "INWARDS", (ZString)result3["Direction"].ToString());
				AssertEquals("InventoryLine is correct.", (ZString)receive2.Lines[0].PK.ToString(), (ZString)result3["InventoryLine"].ToString());
				AssertEquals("InvoiceQty is correct and integer.", "30", (ZString)result3["RelativeInvoiceQtyIn"]);
			});

			CombineAssertions("The 4th line of this report, represents the outwards action of 2 WheelRim, for the assembly of the work order.", () =>
			{
				var result4 = results[3];
				AssertEquals("Direction is outwards.", "OUTWARDS", (ZString)result4["Direction"].ToString());
				AssertEquals("InventoryLine is correct.", (ZString)receive2.Lines[0].PK.ToString(), (ZString)result4["InventoryLine"].ToString());
				AssertEquals("InvoiceQty is correct and integer.", "2", (ZString)result4["RelativeInvoiceQtyOut"]);
			});

			CombineAssertions("The 5th line of this report, represents the inwards action of 2 WheelTyre.", () =>
			{
				var result5 = results[4];
				AssertEquals("Direction is inwards.", "INWARDS", (ZString)result5["Direction"].ToString());
				AssertEquals("InventoryLine is correct.", (ZString)receive3.Lines[0].PK.ToString(), (ZString)result5["InventoryLine"].ToString());
				AssertEquals("InvoiceQty is correct and integer.", "10", (ZString)result5["RelativeInvoiceQtyIn"]);
			});

			CombineAssertions("The 6th line of this report, represents the outwards action of 2 WheelTyre, for the assembly of the work order.", () =>
			{
				var result6 = results[5];
				AssertEquals("Direction is outwards.", "OUTWARDS", (ZString)result6["Direction"].ToString());
				AssertEquals("InventoryLine is correct.", (ZString)receive3.Lines[0].PK.ToString(), (ZString)result6["InventoryLine"].ToString());
				AssertEquals("InvoiceQty is correct and integer.", "2", (ZString)result6["RelativeInvoiceQtyOut"]);
			});
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

			var sql = @"select * from WhsManufacturerWarehouseStockReport(";
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
			sql += " order by InventoryProdCode, PK, EntryRow";

			result.Load(sql);

			return result;
		}

		#endregion
	}
}
