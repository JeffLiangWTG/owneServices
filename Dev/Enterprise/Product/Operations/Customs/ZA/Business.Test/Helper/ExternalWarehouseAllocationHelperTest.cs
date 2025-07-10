using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Customs.ZA.Business.Testing
{
	public class ExternalWarehouseAllocationHelperTest : TestCaseWithFactory
	{
		public void TestBondedGoodsAllocation()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, isVirtualWarehouse: true, "N10");
			var whsReceive1 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK, "RCV1", ZDateTimeOffset.Today.AddDays(-3));
			var whsReceive2 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK, "RCV2", ZDateTimeOffset.Today.AddDays(-2));
			var whsReceive3 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK, "RCV3", ZDateTimeOffset.Today.AddDays(-1));
			var whsReceive4 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK, "RCV4", ZDateTimeOffset.Today.AddDays(-1));
			var whsReceive5 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK, "RCV5", ZDateTimeOffset.Today.AddDays(-1));
			var whsInventory1 = Helper.GetNewReceiveInventory(whsReceive1, Helper.Part, "PACKAGE1", 10m, 90m, 90m, bondedEntryKey: "EN00123-1");
			var whsInventory2 = Helper.GetNewReceiveInventory(whsReceive2, Helper.Part, "PACKAGE2", 20m, 180m, 180m, bondedEntryKey: "EN00123-1");
			var whsInventory3 = Helper.GetNewReceiveInventory(whsReceive2, Helper.Part2, "PACKAGE2", 20m, 180m, 180m, bondedEntryKey: "EN00123-1");
			var whsInventory4 = Helper.GetNewReceiveInventory(whsReceive3, Helper.Part, "PACKAGE3", 20m, 180m, 180m, bondedEntryKey: "EN00123-1");
			var whsInventory5 = Helper.GetNewReceiveInventory(whsReceive4, Helper.Part, "PACKAGE4", 20m, 160m, 160m, bondedEntryKey: "NOTAMATCH");
			var whsInventory6 = Helper.GetNewReceiveInventory(whsReceive5, Helper.Part, "PACKAGE5", 20m, 160m, 160m, bondedEntryKey: "EN00123-1");

			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive1.PK);
			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive2.PK);
			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive4.PK);
			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive5.PK);
			whsReceive1.FinaliseDocketWithoutUserConfirmation();
			whsReceive2.FinaliseDocketWithoutUserConfirmation();
			whsReceive4.FinaliseDocketWithoutUserConfirmation();
			whsReceive5.FinaliseDocketWithoutUserConfirmation();

			GetNewWhsBondedWarehouseAttribute(whsInventory1.PK, 135m, 90m, "KGM", 4m, "GRM", 5m, "GRM", 6m, "GRM", "ROOCert=Cert 1", "EN00123-1", 1, "10000001", "101", "1122");
			GetNewWhsBondedWarehouseAttribute(whsInventory2.PK, 90m, 180m, "KGM", 4m, "GRM", 5m, "GRM", 6m, "GRM", "ROOCert=Cert 2*OriginalProcedureCode=22", "EN00123-1", 2, "10000002", "102");
			GetNewWhsBondedWarehouseAttribute(whsInventory3.PK, 135m, 180m, "KGM", 4m, "GRM", 5m, "GRM", 6m, "GRM", "ROOCert=Cert 3*OriginalProcedureCode=33", "EN00123-1", 3, "10000003", "103");
			GetNewWhsBondedWarehouseAttribute(whsInventory4.PK, 90m, 180m, "KGM", 4m, "GRM", 5m, "GRM", 6m, "GRM", "ROOCert=Cert 4*OriginalProcedureCode=44", "EN00123-1", 4, "10000004", "104");
			GetNewWhsBondedWarehouseAttribute(whsInventory5.PK, 80m, 160m, "KGM", 4m, "GRM", 5m, "GRM", 6m, "", "ROOCert=Cert 5*OriginalProcedureCode=55", "NOTAMATCH", 5, "10000005", "105");
			GetNewWhsBondedWarehouseAttribute(whsInventory6.PK, 80m, 160m, "KGM", 4m, "GRM", 5m, "GRM", 6m, "GRM", "ROOCert=Cert 6*OriginalProcedureCode=66", "EN00123-1", 6, "10000006", "106");

			Factory.Save();

			var docketLine = Factory.Load<IWhsDocketLine>(whsInventory5.WI_WE_InDocketLine);
			if (docketLine != null)
			{
				docketLine.WE_F3_NKPackType = "KGM";
			}

			var batch = Factory.New<CusWHSOperatorTransactionBatch>();
			batch.WOB_Batch = "Batch1";
			batch.WOB_GC_Company = GlbCompany.CurrentCompany.PK;
			batch.WOB_OA_Warehouse = whsWarehouse.WW_OA_WarehouseAddress;

			var order = Factory.New<CusWHSOperatorTransaction>();
			order.WOT_BatchLineNo = 1;
			order.WOT_ExportType = "";
			order.WOT_IsCustomsControlled = false;
			order.WOT_OH_ProductOwner = Helper.Importer.PK;
			order.WOT_OP_Product = Helper.Part.PK;
			order.WOT_OwnerReference = "ORDER1";
			order.WOT_Quantity = 350m;
			order.WOT_RN_NKOrigin = "";
			order.WOT_RX_NKCurrency = "ZAR";
			order.WOT_Status = "VAL";
			order.WOT_TotalValue = 1000m;
			order.WOT_TransactionDate = ZDate.Today;
			order.WOT_TransactionType = "ORD";
			order.WOT_WOB_CusWHSTransactionBatch = batch.PK;

			var receipt = Factory.New<CusWHSOperatorTransaction>();
			receipt.WOT_BatchLineNo = 2;
			receipt.WOT_ExportType = "";
			receipt.WOT_IsCustomsControlled = true;
			receipt.WOT_OH_ProductOwner = Helper.Importer.PK;
			receipt.WOT_OP_Product = Helper.Part.PK;
			receipt.WOT_OwnerReference = "RECEIPT1";
			receipt.WOT_Quantity = 350m;
			receipt.WOT_RN_NKOrigin = "ZA";
			receipt.WOT_RX_NKCurrency = "ZAR";
			receipt.WOT_Status = "VAL";
			receipt.WOT_TotalValue = 1000m;
			receipt.WOT_TransactionDate = ZDate.Today.AddDays(-1);
			receipt.WOT_TransactionType = "REC";
			receipt.WOT_WOB_CusWHSTransactionBatch = batch.PK;
			receipt.WOT_CustomsEntryNumber = "EN00123-1";

			var transactionLine1 = Factory.New<CusWHSOperatorTransactionLine>();
			transactionLine1.WOL_Quantity = 300m;
			transactionLine1.WOL_WOT_WHSOperatorTransactionOrder = order.PK;
			transactionLine1.WOL_WOT_WHSOperatorTransactionReceipt = receipt.PK;
			transactionLine1.WOL_CustomsEntryLineNo = 10011;

			var transactionLine2 = Factory.New<CusWHSOperatorTransactionLine>();
			transactionLine2.WOL_Quantity = 30m;
			transactionLine2.WOL_WOT_WHSOperatorTransactionOrder = order.PK;
			transactionLine2.WOL_WOT_WHSOperatorTransactionReceipt = receipt.PK;
			transactionLine2.WOL_CustomsEntryLineNo = 10012;

			var transactionLine3 = Factory.New<CusWHSOperatorTransactionLine>();
			transactionLine3.WOL_Quantity = 20m;
			transactionLine3.WOL_WOT_WHSOperatorTransactionOrder = order.PK;
			transactionLine3.WOL_WOT_WHSOperatorTransactionReceipt = receipt.PK;
			transactionLine3.WOL_CustomsEntryLineNo = 10013;

			Factory.Save();

			var helper = new ExternalWarehouseAllocationHelper(Factory);
			var exbondEntryLines1 = helper.Allocate(new[] { transactionLine1 });

			AssertEquals(3, exbondEntryLines1.Count);
			AssertEquals(300m, exbondEntryLines1.Sum(line => line.Quantity));
			AssertEquals(240m, exbondEntryLines1.Sum(line => line.PriceExbond));
			AssertExbondEntryLine(useExportPrice: false, exbondEntryLines1[0], transactionLine1.PK, receipt, order, 90m, 135m, batch.WOB_OA_Warehouse, "10000001", "101", "Cert 1", "EN00123-1", 1, 90m, 4m, 5m, 90m, "GRM", "11");
			AssertExbondEntryLine(useExportPrice: false, exbondEntryLines1[1], transactionLine1.PK, receipt, order, 180m, 90m, batch.WOB_OA_Warehouse, "10000002", "102", "Cert 2", "EN00123-1", 2, 180m, 4m, 5m, 180m, "GRM", "22");
			AssertExbondEntryLine(useExportPrice: false, exbondEntryLines1[2], transactionLine1.PK, receipt, order, 30m, 15m, batch.WOB_OA_Warehouse, "10000006", "106", "Cert 6", "EN00123-1", 6, 30m, 0.75m, 0.9375m, 30m, "GRM", "66");

			Factory.Save();
			var exbondEntryLines2 = helper.Allocate(new[] { transactionLine1, transactionLine2, transactionLine3 });

			AssertEquals(5, exbondEntryLines2.Count);
			AssertEquals(350m, exbondEntryLines2.Sum(line => line.Quantity));
			AssertEquals(265m, exbondEntryLines2.Sum(line => line.PriceExbond));
			AssertExbondEntryLine(useExportPrice: false, exbondEntryLines2[3], transactionLine2.PK, receipt, order, 30m, 15m, batch.WOB_OA_Warehouse, "10000006", "106", "Cert 6", "EN00123-1", 6, 30m, 0.75m, 0.9375m, 30m, "GRM", "66");
			AssertExbondEntryLine(useExportPrice: false, exbondEntryLines2[4], transactionLine3.PK, receipt, order, 20m, 10m, batch.WOB_OA_Warehouse, "10000006", "106", "Cert 6", "EN00123-1", 6, 20m, 0.5m, 0.625m, 20m, "GRM", "66");
		}

		public void TestBondedGoodsAllocationWillAllocateEvenIfStockOnHandIsZero()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, isVirtualWarehouse: true, "N10");
			var whsReceive1 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK, "RCV1", ZDateTimeOffset.Today.AddDays(-3));
			var whsInventory1 = Helper.GetNewReceiveInventory(whsReceive1, Helper.Part, "PACKAGE1", 10m, 0m, 0m, bondedEntryKey: "EN00123-1");

			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive1.PK);
			whsReceive1.FinaliseDocketWithoutUserConfirmation();

			GetNewWhsBondedWarehouseAttribute(whsInventory1.PK, 135m, 90m, "KGM", 4m, "GRM", 5m, "GRM", 6m, "GRM", "ROOCert=Cert 1", "EN00123-1", 1, "10000001", "101", "1122");

			Factory.Save();

			var batch = Factory.New<CusWHSOperatorTransactionBatch>();
			batch.WOB_Batch = "Batch1";
			batch.WOB_GC_Company = GlbCompany.CurrentCompany.PK;
			batch.WOB_OA_Warehouse = whsWarehouse.WW_OA_WarehouseAddress;

			var order = Factory.New<CusWHSOperatorTransaction>();
			order.WOT_BatchLineNo = 1;
			order.WOT_ExportType = "";
			order.WOT_IsCustomsControlled = false;
			order.WOT_OH_ProductOwner = Helper.Importer.PK;
			order.WOT_OP_Product = Helper.Part.PK;
			order.WOT_OwnerReference = "ORDER1";
			order.WOT_Quantity = 350m;
			order.WOT_RN_NKOrigin = "";
			order.WOT_RX_NKCurrency = "ZAR";
			order.WOT_Status = "VAL";
			order.WOT_TotalValue = 1000m;
			order.WOT_TransactionDate = ZDate.Today;
			order.WOT_TransactionType = "ORD";
			order.WOT_WOB_CusWHSTransactionBatch = batch.PK;

			var receipt = Factory.New<CusWHSOperatorTransaction>();
			receipt.WOT_BatchLineNo = 2;
			receipt.WOT_ExportType = "";
			receipt.WOT_IsCustomsControlled = true;
			receipt.WOT_OH_ProductOwner = Helper.Importer.PK;
			receipt.WOT_OP_Product = Helper.Part.PK;
			receipt.WOT_OwnerReference = "RECEIPT1";
			receipt.WOT_Quantity = 350m;
			receipt.WOT_RN_NKOrigin = "ZA";
			receipt.WOT_RX_NKCurrency = "ZAR";
			receipt.WOT_Status = "VAL";
			receipt.WOT_TotalValue = 1000m;
			receipt.WOT_TransactionDate = ZDate.Today.AddDays(-1);
			receipt.WOT_TransactionType = "REC";
			receipt.WOT_WOB_CusWHSTransactionBatch = batch.PK;
			receipt.WOT_CustomsEntryNumber = "EN00123-1";

			var transactionLine1 = Factory.New<CusWHSOperatorTransactionLine>();
			transactionLine1.WOL_Quantity = 350m;
			transactionLine1.WOL_WOT_WHSOperatorTransactionOrder = order.PK;
			transactionLine1.WOL_WOT_WHSOperatorTransactionReceipt = receipt.PK;
			transactionLine1.WOL_CustomsEntryLineNo = 10011;

			Factory.Save();

			var helper = new ExternalWarehouseAllocationHelper(Factory);
			var exbondEntryLines1 = helper.Allocate(new[] { transactionLine1 });

			AssertEquals(1, exbondEntryLines1.Count);
			AssertEquals(350m, exbondEntryLines1.Sum(line => line.Quantity));
			AssertExbondEntryLine(useExportPrice: false, exbondEntryLines1[0], transactionLine1.PK, receipt, order, 350m, 135m, batch.WOB_OA_Warehouse, "10000001", "101", "Cert 1", "EN00123-1", 1, 90m, 4m, 5m, 350m, "GRM", "11");
		}

		public void TestBondedGoodsAllocationWillOverAllocate()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, isVirtualWarehouse: true, "N10");
			var whsReceive1 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK, "RCV1", ZDateTimeOffset.Today.AddDays(-3));
			var whsReceive2 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK, "RCV2", ZDateTimeOffset.Today.AddDays(-2));
			var whsInventory1 = Helper.GetNewReceiveInventory(whsReceive1, Helper.Part, "PACKAGE1", 10m, 90m, 90m, bondedEntryKey: "EN00123-1");
			var whsInventory2 = Helper.GetNewReceiveInventory(whsReceive2, Helper.Part, "PACKAGE2", 20m, 180m, 180m, bondedEntryKey: "EN00123-1");

			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive1.PK);
			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive2.PK);
			whsReceive1.FinaliseDocketWithoutUserConfirmation();
			whsReceive2.FinaliseDocketWithoutUserConfirmation();

			GetNewWhsBondedWarehouseAttribute(whsInventory1.PK, 135m, 90m, "KGM", 4m, "GRM", 5m, "GRM", 6m, "GRM", "ROOCert=Cert 1", "EN00123-1", 1, "10000001", "101", "1122");
			GetNewWhsBondedWarehouseAttribute(whsInventory2.PK, 90m, 180m, "KGM", 4m, "GRM", 5m, "GRM", 6m, "GRM", "ROOCert=Cert 2*OriginalProcedureCode=22", "EN00123-1", 2, "10000002", "102");

			Factory.Save();

			var batch = Factory.New<CusWHSOperatorTransactionBatch>();
			batch.WOB_Batch = "Batch1";
			batch.WOB_GC_Company = GlbCompany.CurrentCompany.PK;
			batch.WOB_OA_Warehouse = whsWarehouse.WW_OA_WarehouseAddress;

			var order = Factory.New<CusWHSOperatorTransaction>();
			order.WOT_BatchLineNo = 1;
			order.WOT_ExportType = "";
			order.WOT_IsCustomsControlled = false;
			order.WOT_OH_ProductOwner = Helper.Importer.PK;
			order.WOT_OP_Product = Helper.Part.PK;
			order.WOT_OwnerReference = "ORDER1";
			order.WOT_Quantity = 350m;
			order.WOT_RN_NKOrigin = "";
			order.WOT_RX_NKCurrency = "ZAR";
			order.WOT_Status = "VAL";
			order.WOT_TotalValue = 1000m;
			order.WOT_TransactionDate = ZDate.Today;
			order.WOT_TransactionType = "ORD";
			order.WOT_WOB_CusWHSTransactionBatch = batch.PK;

			var receipt = Factory.New<CusWHSOperatorTransaction>();
			receipt.WOT_BatchLineNo = 2;
			receipt.WOT_ExportType = "";
			receipt.WOT_IsCustomsControlled = true;
			receipt.WOT_OH_ProductOwner = Helper.Importer.PK;
			receipt.WOT_OP_Product = Helper.Part.PK;
			receipt.WOT_OwnerReference = "RECEIPT1";
			receipt.WOT_Quantity = 350m;
			receipt.WOT_RN_NKOrigin = "ZA";
			receipt.WOT_RX_NKCurrency = "ZAR";
			receipt.WOT_Status = "VAL";
			receipt.WOT_TotalValue = 1000m;
			receipt.WOT_TransactionDate = ZDate.Today.AddDays(-1);
			receipt.WOT_TransactionType = "REC";
			receipt.WOT_WOB_CusWHSTransactionBatch = batch.PK;
			receipt.WOT_CustomsEntryNumber = "EN00123-1";

			var transactionLine1 = Factory.New<CusWHSOperatorTransactionLine>();
			transactionLine1.WOL_Quantity = 300m;
			transactionLine1.WOL_WOT_WHSOperatorTransactionOrder = order.PK;
			transactionLine1.WOL_WOT_WHSOperatorTransactionReceipt = receipt.PK;
			transactionLine1.WOL_CustomsEntryLineNo = 10011;

			Factory.Save();

			var helper = new ExternalWarehouseAllocationHelper(Factory);
			var exbondEntryLines1 = helper.Allocate(new[] { transactionLine1 });

			AssertEquals(2, exbondEntryLines1.Count);
			AssertEquals(300m, exbondEntryLines1.Sum(line => line.Quantity));
			AssertExbondEntryLine(useExportPrice: false, exbondEntryLines1[0], transactionLine1.PK, receipt, order, 90m, 135m, batch.WOB_OA_Warehouse, "10000001", "101", "Cert 1", "EN00123-1", 1, 90m, 4m, 5m, 90m, "GRM", "11");
			AssertExbondEntryLine(useExportPrice: false, exbondEntryLines1[1], transactionLine1.PK, receipt, order, 210m, 105m, batch.WOB_OA_Warehouse, "10000002", "102", "Cert 2", "EN00123-1", 2, 210m, 4.66667m, 5.83333m, 210m, "GRM", "22");
		}

		public void TestBondedGoodsAllocationExportPrice()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, isVirtualWarehouse: true, "N10");
			var whsReceive1 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK, "RCV1", ZDateTimeOffset.Today);
			var whsInventory1 = Helper.GetNewReceiveInventory(whsReceive1, Helper.Part, "PACKAGE1", 10m, 90m, 90m, bondedEntryKey: "EN00123-1");
			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive1.PK);
			whsReceive1.FinaliseDocketWithoutUserConfirmation();
			GetNewWhsBondedWarehouseAttribute(whsInventory1.PK, 135m, 90m, "KGM", 4m, "GRM", 5m, "GRM", 6m, "GRM", "ROOCert=Cert 1", "EN00123-1", 1, "10000001", "101", "1122");
			Factory.Save();

			var batch = Factory.New<CusWHSOperatorTransactionBatch>();
			batch.WOB_Batch = "Batch1";
			batch.WOB_GC_Company = GlbCompany.CurrentCompany.PK;
			batch.WOB_OA_Warehouse = whsWarehouse.WW_OA_WarehouseAddress;

			var order = Factory.New<CusWHSOperatorTransaction>();
			order.WOT_BatchLineNo = 1;
			order.WOT_ExportType = "EXP";
			order.WOT_IsCustomsControlled = false;
			order.WOT_OH_ProductOwner = Helper.Importer.PK;
			order.WOT_OP_Product = Helper.Part.PK;
			order.WOT_OwnerReference = "ORDER1";
			order.WOT_Quantity = 90m;
			order.WOT_RN_NKOrigin = "";
			order.WOT_RX_NKCurrency = "ZAR";
			order.WOT_Status = "VAL";
			order.WOT_TotalValue = 900m;
			order.WOT_TransactionDate = ZDate.Today;
			order.WOT_TransactionType = "ORD";
			order.WOT_WOB_CusWHSTransactionBatch = batch.PK;

			var receipt = Factory.New<CusWHSOperatorTransaction>();
			receipt.WOT_BatchLineNo = 2;
			receipt.WOT_ExportType = "";
			receipt.WOT_IsCustomsControlled = true;
			receipt.WOT_OH_ProductOwner = Helper.Importer.PK;
			receipt.WOT_OP_Product = Helper.Part.PK;
			receipt.WOT_OwnerReference = "RECEIPT1";
			receipt.WOT_Quantity = 100m;
			receipt.WOT_RN_NKOrigin = "ZA";
			receipt.WOT_RX_NKCurrency = "ZAR";
			receipt.WOT_Status = "VAL";
			receipt.WOT_TotalValue = 1000m;
			receipt.WOT_TransactionDate = ZDate.Today.AddDays(-1);
			receipt.WOT_TransactionType = "REC";
			receipt.WOT_WOB_CusWHSTransactionBatch = batch.PK;
			receipt.WOT_CustomsEntryNumber = "EN00123-1";

			var transactionLine1 = Factory.New<CusWHSOperatorTransactionLine>();
			transactionLine1.WOL_Quantity = 90m;
			transactionLine1.WOL_WOT_WHSOperatorTransactionOrder = order.PK;
			transactionLine1.WOL_WOT_WHSOperatorTransactionReceipt = receipt.PK;
			transactionLine1.WOL_CustomsEntryLineNo = 10011;

			Factory.Save();

			var helper = new ExternalWarehouseAllocationHelper(Factory);
			var exbondEntryLines1 = helper.Allocate(new[] { transactionLine1 });
			AssertEquals(1, exbondEntryLines1.Count);
			AssertExbondEntryLine(useExportPrice: true, exbondEntryLines1[0], transactionLine1.PK, receipt, order, 90m, 900m, batch.WOB_OA_Warehouse, "10000001", "101", "Cert 1", "EN00123-1", 1, 90m, 4m, 5m, 90m, "GRM", "11");
			AssertEquals(135m, exbondEntryLines1[0].PriceExbond);
		}

		public void TestLocalGoodsAllocation()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, isVirtualWarehouse: true, "N10");

			var batch = Factory.New<CusWHSOperatorTransactionBatch>();
			batch.WOB_Batch = "Batch1";
			batch.WOB_GC_Company = GlbCompany.CurrentCompany.PK;
			batch.WOB_OA_Warehouse = whsWarehouse.WW_OA_WarehouseAddress;

			var order = Factory.New<CusWHSOperatorTransaction>();
			order.WOT_BatchLineNo = 1;
			order.WOT_ExportType = "EXP";
			order.WOT_IsCustomsControlled = false;
			order.WOT_OH_ProductOwner = Helper.Importer.PK;
			order.WOT_OP_Product = Helper.Part.PK;
			order.WOT_OwnerReference = "ORDER1";
			order.WOT_Quantity = 400m;
			order.WOT_RN_NKOrigin = "";
			order.WOT_RX_NKCurrency = "ZAR";
			order.WOT_Status = "VAL";
			order.WOT_TotalValue = 1000m;
			order.WOT_TransactionDate = ZDate.Today;
			order.WOT_TransactionType = "ORD";
			order.WOT_WOB_CusWHSTransactionBatch = batch.PK;

			var receipt = Factory.New<CusWHSOperatorTransaction>();
			receipt.WOT_BatchLineNo = 2;
			receipt.WOT_ExportType = "";
			receipt.WOT_IsCustomsControlled = false;
			receipt.WOT_OH_ProductOwner = Helper.Importer.PK;
			receipt.WOT_OP_Product = Helper.Part.PK;
			receipt.WOT_OwnerReference = "RECEIPT1";
			receipt.WOT_Quantity = 400m;
			receipt.WOT_RN_NKOrigin = "ZA";
			receipt.WOT_RX_NKCurrency = "ZAR";
			receipt.WOT_Status = "VAL";
			receipt.WOT_TotalValue = 1000m;
			receipt.WOT_TransactionDate = ZDate.Today;
			receipt.WOT_TransactionType = "REC";
			receipt.WOT_WOB_CusWHSTransactionBatch = batch.PK;

			var transactionLine1 = Factory.New<CusWHSOperatorTransactionLine>();
			transactionLine1.WOL_Quantity = 200m;
			transactionLine1.WOL_WOT_WHSOperatorTransactionOrder = order.PK;
			transactionLine1.WOL_WOT_WHSOperatorTransactionReceipt = receipt.PK;

			var transactionLine2 = Factory.New<CusWHSOperatorTransactionLine>();
			transactionLine2.WOL_Quantity = 100m;
			transactionLine2.WOL_WOT_WHSOperatorTransactionOrder = order.PK;
			transactionLine2.WOL_WOT_WHSOperatorTransactionReceipt = receipt.PK;

			Factory.Save();

			var helper = new ExternalWarehouseAllocationHelper(Factory);
			var exbondEntryLines = helper.Allocate(new[] { transactionLine1, transactionLine2 });

			AssertEquals(2, exbondEntryLines.Count);
			AssertExbondEntryLine(useExportPrice: true, exbondEntryLines[0], transactionLine1.PK, receipt, order, 200m, 500m);
			AssertExbondEntryLine(useExportPrice: true, exbondEntryLines[1], transactionLine2.PK, receipt, order, 100m, 250m);
		}

		public void TestNoAllocationForEmptyProduct()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, isVirtualWarehouse: true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK, "RCV1", ZDateTimeOffset.Today.AddDays(-3));
			var whsInventory = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, "PACKAGE1", 10m, 90m, 90m, bondedEntryKey: "EN00123-1");

			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();

			GetNewWhsBondedWarehouseAttribute(whsInventory.PK, 135m, 90m, "KGM", 4m, "GRM", 5m, "GRM", 6m, "GRM", "ROOCert=Cert 1", "EN00123-1", 1, "10000001", "101", "1122");

			Factory.Save();

			var batch = Factory.New<CusWHSOperatorTransactionBatch>();
			batch.WOB_Batch = "Batch1";
			batch.WOB_GC_Company = GlbCompany.CurrentCompany.PK;
			batch.WOB_OA_Warehouse = whsWarehouse.WW_OA_WarehouseAddress;

			var order = Factory.New<CusWHSOperatorTransaction>();
			order.WOT_BatchLineNo = 1;
			order.WOT_ExportType = "EXP";
			order.WOT_IsCustomsControlled = false;
			order.WOT_OH_ProductOwner = Helper.Importer.PK;
			order.WOT_OP_Product = ZGuid.Empty;
			order.WOT_OwnerReference = "ORDER1";
			order.WOT_Quantity = 350m;
			order.WOT_RN_NKOrigin = "";
			order.WOT_RX_NKCurrency = "ZAR";
			order.WOT_Status = "VAL";
			order.WOT_TotalValue = 1000m;
			order.WOT_TransactionDate = ZDate.Today;
			order.WOT_TransactionType = "ORD";
			order.WOT_WOB_CusWHSTransactionBatch = batch.PK;

			var receipt = Factory.New<CusWHSOperatorTransaction>();
			receipt.WOT_BatchLineNo = 2;
			receipt.WOT_ExportType = "";
			receipt.WOT_IsCustomsControlled = true;
			receipt.WOT_OH_ProductOwner = Helper.Importer.PK;
			receipt.WOT_OP_Product = ZGuid.Empty;
			receipt.WOT_OwnerReference = "RECEIPT1";
			receipt.WOT_Quantity = 350m;
			receipt.WOT_RN_NKOrigin = "ZA";
			receipt.WOT_RX_NKCurrency = "ZAR";
			receipt.WOT_Status = "VAL";
			receipt.WOT_TotalValue = 1000m;
			receipt.WOT_TransactionDate = ZDate.Today;
			receipt.WOT_TransactionType = "REC";
			receipt.WOT_WOB_CusWHSTransactionBatch = batch.PK;

			var transactionLine = Factory.New<CusWHSOperatorTransactionLine>();
			transactionLine.WOL_Quantity = 10m;
			transactionLine.WOL_WOT_WHSOperatorTransactionOrder = order.PK;
			transactionLine.WOL_WOT_WHSOperatorTransactionReceipt = receipt.PK;

			Factory.Save();

			var helper = new ExternalWarehouseAllocationHelper(Factory);
			var exbondEntryLines = helper.Allocate(new[] { transactionLine });
			AssertEquals(0, exbondEntryLines.Count);
		}

		public void TestIsEntryMatch()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, isVirtualWarehouse: true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK, "RCV1", ZDateTimeOffset.Today.AddDays(-3));
			var whsInventory1 = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, "PACKAGE1", 1m, 91m, 91m, bondedEntryKey: "EN00123-1");
			var whsInventory2 = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, "PACKAGE1", 1m, 92m, 92m, bondedEntryKey: "EN00123-2");
			var whsInventory3 = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, "PACKAGE1", 1m, 93m, 93m, bondedEntryKey: "EN00123-3");

			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
			whsReceive.FinaliseDocketWithoutUserConfirmation();

			var whsAttrib1 = GetNewWhsBondedWarehouseAttribute(whsInventory1.PK, 101m, 91m, "KGM", 1m, "UNT", 4m, "UNT", 57m, "UNT", "ROOCert=Cert 1", "EN00123", 1, "10000001", "101", "1122");
			var whsAttrib2 = GetNewWhsBondedWarehouseAttribute(whsInventory2.PK, 102m, 92m, "KGM", 2m, "UNT", 5m, "UNT", 58m, "UNT", "ROOCert=Cert 2", "EN00123", 2, "10000001", "101", "1122");
			var whsAttrib3 = GetNewWhsBondedWarehouseAttribute(whsInventory3.PK, 103m, 93m, "KGM", 3m, "UNT", 6m, "UNT", 59m, "UNT", "ROOCert=Cert 3", "EN00123", 3, "10000001", "101", "1122");

			Factory.Save();

			var batch = Factory.New<CusWHSOperatorTransactionBatch>();
			batch.WOB_Batch = "Batch1";
			batch.WOB_GC_Company = GlbCompany.CurrentCompany.PK;
			batch.WOB_OA_Warehouse = whsWarehouse.WW_OA_WarehouseAddress;

			var order = Factory.New<CusWHSOperatorTransaction>();
			order.WOT_BatchLineNo = 1;
			order.WOT_ExportType = "EXP";
			order.WOT_IsCustomsControlled = false;
			order.WOT_OH_ProductOwner = Helper.Importer.PK;
			order.WOT_OP_Product = Helper.Part.PK;
			order.WOT_OwnerReference = "ORDER1";
			order.WOT_Quantity = 10m;
			order.WOT_RN_NKOrigin = "";
			order.WOT_RX_NKCurrency = "ZAR";
			order.WOT_Status = "VAL";
			order.WOT_TotalValue = 1000m;
			order.WOT_TransactionDate = ZDate.Today;
			order.WOT_TransactionType = "ORD";
			order.WOT_WOB_CusWHSTransactionBatch = batch.PK;

			var receipt = Factory.New<CusWHSOperatorTransaction>();
			receipt.WOT_BatchLineNo = 2;
			receipt.WOT_ExportType = "";
			receipt.WOT_IsCustomsControlled = true;
			receipt.WOT_OH_ProductOwner = Helper.Importer.PK;
			receipt.WOT_OP_Product = Helper.Part.PK;
			receipt.WOT_OwnerReference = "RECEIPT1";
			receipt.WOT_Quantity = 10m;
			receipt.WOT_RN_NKOrigin = "ZA";
			receipt.WOT_RX_NKCurrency = "ZAR";
			receipt.WOT_Status = "VAL";
			receipt.WOT_TotalValue = 1000m;
			receipt.WOT_TransactionDate = ZDate.Today;
			receipt.WOT_TransactionType = "REC";
			receipt.WOT_CustomsEntryNumber = "EN00123";
			receipt.WOT_WOB_CusWHSTransactionBatch = batch.PK;

			var transactionLine = Factory.New<CusWHSOperatorTransactionLine>();
			transactionLine.WOL_Quantity = 10m;
			transactionLine.WOL_WOT_WHSOperatorTransactionOrder = order.PK;
			transactionLine.WOL_WOT_WHSOperatorTransactionReceipt = receipt.PK;
			transactionLine.WOL_CustomsEntryLineNo = 2;

			Factory.Save();

			var helper = new ExternalWarehouseAllocationHelper(Factory);
			var exbondEntryLines = helper.Allocate(new[] { transactionLine });

			CombineAssertions("Match Entry & Line No", () =>
			{
				AssertEquals("1 exBondEntryLine created", 1, exbondEntryLines.Count);
				AssertEquals("MRN Line No", (ZShort)2, exbondEntryLines[0].MRNLine);
			});

			transactionLine.WOL_CustomsEntryLineNo = 5;
			Factory.Save();
			exbondEntryLines = helper.Allocate(new[] { transactionLine });

			CombineAssertions("Match Entry only", () =>
			{
				AssertEquals("1 exBondEntryLine created", 1, exbondEntryLines.Count);
				AssertGreaterThan<ZShort>("MRN Line No", exbondEntryLines[0].MRNLine.Value, 0);
			});
		}

		public IWhsBondedWarehouseAttribute GetNewWhsBondedWarehouseAttribute(ZGuid receiveLinePK, ZDecimal valueForDuty,
			ZDecimal customsQty, ZString customsUnitOfQty, ZDecimal customsSecondQty, ZString customsSecondUnitQty, ZDecimal customsThirdQty, ZString customsThirdUnitQty,
			ZDecimal bondedWhsQty, ZString bondedWhsUnitOfQty, ZString addInfo, ZString entryKey, ZInt entryLineNo, ZString tariffCode, ZString preference, string inwardProcedure = "")
		{
			var result = Helper.GetNewWhsBondedWarehouseAttribute(receiveLinePK, valueForDuty,
				customsQty, customsUnitOfQty, customsSecondQty, customsSecondUnitQty, customsThirdQty, customsThirdUnitQty,
				Core.Constants.CountryCodes.SouthAfrica, bondedWhsQty, bondedWhsUnitOfQty, addInfo, entryKey, (ZShort)entryLineNo);
			result.WB_InwardProcedure = inwardProcedure;
			result.WB_Tariff = tariffCode;
			result.WB_PrimaryPreference = preference;
			return result;
		}

		void AssertExbondEntryLine(bool useExportPrice, ExbondEntryLine line, ZGuid wolGuid,
			CusWHSOperatorTransaction receipt, CusWHSOperatorTransaction order, ZDecimal quantity, ZDecimal price,
			ZGuid? warehouse = null, ZString? tariffCode = null, ZString? preference = null, ZString? rooCert = null, ZString? mrn = null, ZShort? mrnLine = null,
			ZDecimal? customsQuantity = null, ZDecimal? additionalQty1 = null, ZDecimal? additionalQty2 = null, ZDecimal? countableQty = null, ZString? countableUom = null, ZString? previousProcedure = null)
		{
			CombineAssertions(() =>
			{
				AssertEquals(receipt.WOT_IsCustomsControlled, line.IsCustomsControlled);
				AssertEquals(receipt.WOT_OH_ProductOwner, line.Owner);
				AssertEquals("OwnerReference", order.WOT_OwnerReference, line.OwnerReference);
				AssertEquals("Currency", order.WOT_RX_NKCurrency, line.Currency);
				AssertEquals("InvoiceDate", order.WOT_TransactionDate, line.InvoiceDate);
				AssertEquals("CountryOfOrigin", receipt.WOT_RN_NKOrigin, line.CountryOfOrigin);
				AssertEquals("Quantity", quantity, line.Quantity);
				AssertEquals("PriceExbond", price, useExportPrice ? line.Price : line.PriceExbond);
				AssertEquals("Warehouse", warehouse, line.Warehouse);
				AssertEquals("TariffCode", tariffCode, line.TariffCode);
				AssertEquals("Preference", preference, line.Preference);
				AssertEquals("ROOCert", rooCert, line.ROOCert);
				AssertEquals("MRN", mrn, line.MRN);
				AssertEquals("MRNLine", mrnLine, line.MRNLine);
				AssertEquals("CustomsQuantity", customsQuantity, line.CustomsQuantity);
				AssertEquals("AdditionalQty1", additionalQty1, line.AdditionalQty1);
				AssertEquals("AdditionalQty2", additionalQty2, line.AdditionalQty2);
				AssertEquals("CountableQty", countableQty, line.CountableQty);
				AssertEquals("CountableUom", countableUom, line.CountableUom);
				AssertEquals("PreviousProcedure", previousProcedure, line.PreviousProcedure);
				AssertEquals("DataImportMatchingKey", wolGuid.ToString(), line.DataImportMatchingKey);
			});
		}
		WhsDataTestHelper Helper => helper ?? (helper = new WhsDataTestHelper(Factory));
		WhsDataTestHelper helper;
	}
}
