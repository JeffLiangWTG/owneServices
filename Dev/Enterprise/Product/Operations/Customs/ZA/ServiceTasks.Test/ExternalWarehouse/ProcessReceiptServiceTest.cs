using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.ZA.ServiceTasks.Testing
{
	[TestedType(typeof(ProcessReceiptService))]
	sealed class ProcessReceiptServiceTest : ServiceTaskTestCase<ProcessReceiptService>
	{
		[TestDate(2023, 5, 1)]
		public void TestProcess()
		{
			GlbCompany newCompany;
			CusWHSOperatorTransaction receipt1, receipt2, receipt3, receipt4, receipt5, receipt6, receipt7, receipt8;
			SetupData(out newCompany, out receipt1, out receipt2, out receipt3, out receipt4, out receipt5, out receipt6, out receipt7, out receipt8);

			ZACustomsRegistry.Instance.WOTReceiptsMatchingPeriod.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 120);
			var logger = InitialiseAndRunTaskSchedule(new ProcessReceiptService());

			CombineAssertions(() =>
			{
				AssertReceipt("Receipt 1", receipt1, true, "ZA", WarehouseOperatorTransactionStatusList.Codes.VAL, "001202304255000522", new ZDate(2023, 4, 25));
				AssertReceipt("Receipt 2", receipt2, false, "AU", WarehouseOperatorTransactionStatusList.Codes.VAL, "MRN123", ZDate.Empty);
				AssertReceipt("Receipt 3", receipt3, false, ZString.Empty, WarehouseOperatorTransactionStatusList.Codes.CLS, ZString.Empty, ZDate.Empty);
				AssertReceipt("Receipt 4", receipt4, true, "ZA", WarehouseOperatorTransactionStatusList.Codes.VAL, "002202304255000522", new ZDate(2023, 4, 25));
				AssertReceipt("Receipt 5", receipt5, false, "ZA", WarehouseOperatorTransactionStatusList.Codes.VAL, ZString.Empty, ZDate.Empty);
				AssertReceipt("Receipt 6", receipt6, true, "ZA", WarehouseOperatorTransactionStatusList.Codes.VAL, "NotAnEntryNumberWithDate", ZDate.Empty);
				AssertReceipt("Receipt 7", receipt7, false, "ZA", WarehouseOperatorTransactionStatusList.Codes.VAL, ZString.Empty, ZDate.Empty);
				AssertReceipt("Receipt 8", receipt8, false, "ZA", WarehouseOperatorTransactionStatusList.Codes.VAL, ZString.Empty, ZDate.Empty);

				var logs = logger.ToString();
				AssertContains("Logger 1", $"Processing receipts for Company {GlbCompany.CurrentCompany.GC_Code}", logs);
				AssertContains("Logger 2", $"Processing receipts for Company {newCompany.GC_Code}", logs);
				AssertContains("Logger 3", "6 Receipt(s) processed", logs);
				AssertContains("Logger 4", "Information|3 Receipt(s) matched to warehouse entries", logs);
				AssertContains("Logger 5", "Information|1 Receipt(s) matched to customs entries", logs);
				AssertContains("Logger 6", "1 Receipt(s) processed", logs);
				AssertContains("Logger 7", "Information|0 Receipt(s) matched to warehouse entries", logs);
				AssertContains("Logger 8", "Information|0 Receipt(s) matched to customs entries", logs);
			});
		}

		public void TestProcessWithNoRestriction()
		{
			GlbCompany newCompany;
			CusWHSOperatorTransaction receipt1, receipt2, receipt3, receipt4, receipt5, receipt6, receipt7, receipt8;
			SetupData(out newCompany, out receipt1, out receipt2, out receipt3, out receipt4, out receipt5, out receipt6, out receipt7, out receipt8);

			ZACustomsRegistry.Instance.WOTReceiptsMatchingPeriod.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 0);
			var logger = InitialiseAndRunTaskSchedule(new ProcessReceiptService());

			CombineAssertions(() =>
			{
				AssertReceipt("Receipt 1", receipt1, true, "ZA", WarehouseOperatorTransactionStatusList.Codes.VAL, "001202304255000522", new ZDate(2023, 4, 25));
				AssertReceipt("Receipt 2", receipt2, false, "AU", WarehouseOperatorTransactionStatusList.Codes.VAL, "MRN123", ZDate.Empty);
				AssertReceipt("Receipt 3", receipt3, false, ZString.Empty, WarehouseOperatorTransactionStatusList.Codes.CLS, ZString.Empty, ZDate.Empty);
				AssertReceipt("Receipt 4", receipt4, true, "ZA", WarehouseOperatorTransactionStatusList.Codes.VAL, "002202304255000522", new ZDate(2023, 4, 25));
				AssertReceipt("Receipt 5", receipt5, false, "ZA", WarehouseOperatorTransactionStatusList.Codes.VAL, ZString.Empty, ZDate.Empty);
				AssertReceipt("Receipt 6", receipt6, true, "ZA", WarehouseOperatorTransactionStatusList.Codes.VAL, "NotAnEntryNumberWithDate", ZDate.Empty);
				AssertReceipt("Receipt 7", receipt7, false, "ZA", WarehouseOperatorTransactionStatusList.Codes.VAL, ZString.Empty, ZDate.Empty);
				AssertReceipt("Receipt 8", receipt8, true, "ZA", WarehouseOperatorTransactionStatusList.Codes.VAL, "002202301255000522", new ZDate(2023, 1, 25));

				var logs = logger.ToString();
				AssertContains("Logger 1", $"Processing receipts for Company {GlbCompany.CurrentCompany.GC_Code}", logs);
				AssertContains("Logger 2", $"Processing receipts for Company {newCompany.GC_Code}", logs);
				AssertContains("Logger 3", "6 Receipt(s) processed", logs);
				AssertContains("Logger 4", "Information|4 Receipt(s) matched to warehouse entries", logs);
				AssertContains("Logger 5", "Information|1 Receipt(s) matched to customs entries", logs);
				AssertContains("Logger 6", "1 Receipt(s) processed", logs);
				AssertContains("Logger 7", "Information|0 Receipt(s) matched to warehouse entries", logs);
				AssertContains("Logger 8", "Information|0 Receipt(s) matched to customs entries", logs);
			});
		}

		void SetupData(out GlbCompany newCompany, out CusWHSOperatorTransaction receipt, out CusWHSOperatorTransaction receipt2, out CusWHSOperatorTransaction receipt3, out CusWHSOperatorTransaction receipt4, out CusWHSOperatorTransaction receipt5, out CusWHSOperatorTransaction receipt6, out CusWHSOperatorTransaction receipt7, out CusWHSOperatorTransaction receipt8)
		{
			var owner1 = CreateOrganisation("OWNER1", true, false);
			var owner2 = CreateOrganisation("OWNER2", true, false);
			var owner3 = CreateOrganisation("OWNER3", true, false);
			var supplier1 = CreateOrganisation("SUPPLIER1", false, true);
			var supplier2 = CreateOrganisation("SUPPLIER2", false, true);
			var part1 = CreatePart("PART123", owner1, supplier1);
			part1.OP_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-10);
			var part2 = CreatePart("PART123", owner2, supplier2);
			part2.OP_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-9);

			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_GB = GlbBranch.CurrentBranch.PK;
			dec.JE_GC = GlbBranch.CurrentBranch.GB_GC;
			var invoice = dec.Invoices.AddNew();

			var invLine1 = invoice.InvoiceLines.AddNew();
			invLine1.JI_MatchingKey = "NOT-IT";
			invLine1.JI_CountryOfOrigin = "JP";
			invLine1.JI_PartNo = "PART123";
			invLine1.JI_OP = part2.PK;

			var entry1 = dec.ActiveEntryHeaders.AddNew();
			entry1.CH_EntryReleaseDate = ZDateTime.Today.AddDays(-10);
			entry1.MovementReferenceNumberSetter("ABC123");
			var entryLine1 = entry1.MergedLines.AddNew();
			invLine1.JI_CL = entryLine1.PK;

			var invLine2 = invoice.InvoiceLines.AddNew();
			invLine2.JI_MatchingKey = "00002NOT-IT-EITHER";
			invLine2.JI_CountryOfOrigin = "CN";
			invLine2.JI_PartNo = "PART123";
			invLine2.JI_OP = part2.PK;
			invLine2.JI_Procedure = "4000";

			var entry2 = dec.ActiveEntryHeaders.AddNew();
			entry2.CH_EntryReleaseDate = ZDateTime.Today.AddDays(-10);
			entry2.MovementReferenceNumberSetter("ABC234");
			var entryLine2 = entry1.MergedLines.AddNew();
			invLine2.JI_CL = entryLine2.PK;

			var invLine3 = invoice.InvoiceLines.AddNew();
			invLine3.JI_MatchingKey = "00002BBBBB";
			invLine3.JI_CountryOfOrigin = "AU";
			invLine3.JI_PartNo = "PART123";
			invLine3.JI_OP = part2.PK;
			invLine3.JI_Procedure = "8000";

			var entry3 = dec.ActiveEntryHeaders.AddNew();
			entry3.CH_EntryReleaseDate = ZDateTime.Today.AddDays(-10);
			entry3.MovementReferenceNumberSetter("MRN123");
			var entryLine3 = entry3.MergedLines.AddNew();
			invLine3.JI_CL = entryLine3.PK;

			var receive = Helper.GetNewWhsReceive(Helper.WhsWarehouse.PK, Helper.Importer.PK);
			var receiveLine = Helper.GetNewWhsReceiveLine(receive.PK, part1.PK, ZString.Empty, 1m, 100m, 100m, "NO", bondedEntryKey: "ENT1234-1", arrivalDate: ZDateTime.Today.AddMonths(-1));
			var receiveLineCustomsData = Helper.GetNewWhsBondedWarehouseAttribute(receiveLine.PK, 1000m, 50m, "KG", "ZA", 100m, "NO", ZString.Empty, "001202304255000522", 1);
			receiveLineCustomsData.WB_MatchingKey = "00001AAAAA";

			var receiveLine2 = Helper.GetNewWhsReceiveLine(receive.PK, part1.PK, ZString.Empty, 1m, 100m, 100m, "NO", bondedEntryKey: "ENT2345-1", arrivalDate: ZDateTime.Today.AddMonths(-1));
			var receiveLineCustomsData2 = Helper.GetNewWhsBondedWarehouseAttribute(receiveLine2.PK, 1000m, 50m, "KG", "ZA", 100m, "NO", ZString.Empty, "002202304255000522", 1);
			((WhsBondedWarehouseAttribute)receiveLineCustomsData2).WB_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(-1);
			receiveLineCustomsData2.WB_MatchingKey = "00003BBBBB";

			var receiveLine3 = Helper.GetNewWhsReceiveLine(receive.PK, part1.PK, ZString.Empty, 1m, 100m, 100m, "NO", bondedEntryKey: "ENT2345-1", arrivalDate: ZDateTime.Today.AddMonths(-1));
			var receiveLineCustomsData3 = Helper.GetNewWhsBondedWarehouseAttribute(receiveLine3.PK, 1000m, 50m, "KG", "ZA", 100m, "NO", ZString.Empty, "003202204255000522", 1);
			((WhsBondedWarehouseAttribute)receiveLineCustomsData3).WB_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(-1);
			receiveLineCustomsData3.WB_MatchingKey = "00004CCCCC";

			var receiveLine4 = Helper.GetNewWhsReceiveLine(receive.PK, part1.PK, ZString.Empty, 1m, 100m, 100m, "NO", bondedEntryKey: "ENT1234-1", arrivalDate: ZDateTime.Today.AddMonths(-1));
			var receiveLineCustomsData4 = Helper.GetNewWhsBondedWarehouseAttribute(receiveLine4.PK, 1000m, 50m, "KG", "ZA", 100m, "NO", ZString.Empty, "<PendingCustomsResponse>", 1);
			receiveLineCustomsData4.WB_MatchingKey = "00009ZZZZZ";

			var receiveLine5 = Helper.GetNewWhsReceiveLine(receive.PK, part1.PK, ZString.Empty, 1m, 100m, 100m, "NO", bondedEntryKey: "ENT2345-1", arrivalDate: ZDateTime.Today.AddMonths(-2));
			var receiveLineCustomsData5 = Helper.GetNewWhsBondedWarehouseAttribute(receiveLine5.PK, 1000m, 50m, "KG", "ZA", 100m, "NO", ZString.Empty, "002202301255000522", 1);
			((WhsBondedWarehouseAttribute)receiveLineCustomsData5).WB_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(-5);
			receiveLineCustomsData5.WB_MatchingKey = "00006DDDDD";

			_ = Helper.GetNewWhsBondedWarehouseAttribute(receiveLine3.PK, 911m, 91m, "KG", "ZA", 911m, "NO", ZString.Empty, "NotAnEntryNumberWithDate", 1);

			Factory.Save();

			var batch = Factory.New<CusWHSOperatorTransactionBatch>();
			batch.WOB_Batch = "Batch1";
			batch.WOB_GC_Company = GlbCompany.CurrentCompany.PK;
			batch.WOB_OA_Warehouse = Helper.WhsWarehouse.WW_OA_WarehouseAddress;

			newCompany = Factory.NewWithValidTestData<GlbCompany>();
			newCompany.GC_Code = "AAA";
			var newBranch = newCompany.Branches.AddNew();
			newBranch.GB_Code = "AAA";
			Helper.WhsWarehouse2.WW_OA_WarehouseAddress = Helper.Warehouse2.MainAddress.PK;
			Helper.WhsWarehouse2.WW_GB_RelatedCompanyBranch = newBranch.PK;

			var batch2 = Factory.New<CusWHSOperatorTransactionBatch>();
			batch2.WOB_Batch = "Batch2";
			batch2.WOB_GC_Company = newCompany.PK;
			batch2.WOB_OA_Warehouse = Helper.WhsWarehouse2.WW_OA_WarehouseAddress;

			receipt = Factory.New<CusWHSOperatorTransaction>();
			receipt.WOT_BatchLineNo = 1;
			receipt.WOT_ExportType = ZString.Empty;
			receipt.WOT_IsCustomsControlled = false;
			receipt.WOT_OH_ProductOwner = Helper.Importer.PK;
			receipt.WOT_OP_Product = part1.PK;
			receipt.WOT_OwnerReference = "00001";
			receipt.WOT_Quantity = 350;
			receipt.WOT_RN_NKOrigin = ZString.Empty;
			receipt.WOT_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
			receipt.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.QUE;
			receipt.WOT_TotalValue = 1000m;
			receipt.WOT_TransactionDate = ZDateTime.Today.Date;
			receipt.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.REC;
			receipt.WOT_IsFinal = true;
			receipt.WOT_WOB_CusWHSTransactionBatch = batch.PK;

			receipt2 = Factory.New<CusWHSOperatorTransaction>();
			receipt2.WOT_BatchLineNo = 2;
			receipt2.WOT_ExportType = ZString.Empty;
			receipt2.WOT_IsCustomsControlled = false;
			receipt2.WOT_OH_ProductOwner = Helper.Importer.PK;
			receipt2.WOT_OP_Product = part2.PK;
			receipt2.WOT_OwnerReference = "00002";
			receipt2.WOT_Quantity = 350;
			receipt2.WOT_RN_NKOrigin = ZString.Empty;
			receipt2.WOT_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
			receipt2.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.QUE;
			receipt2.WOT_TotalValue = 1000m;
			receipt2.WOT_TransactionDate = ZDateTime.Today.Date;
			receipt2.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.REC;
			receipt2.WOT_IsFinal = true;
			receipt2.WOT_WOB_CusWHSTransactionBatch = batch.PK;

			receipt3 = Factory.New<CusWHSOperatorTransaction>();
			receipt3.WOT_BatchLineNo = 3;
			receipt3.WOT_ExportType = ZString.Empty;
			receipt3.WOT_IsCustomsControlled = false;
			receipt3.WOT_OH_ProductOwner = Helper.Importer.PK;
			receipt3.WOT_OP_Product = part1.PK;
			receipt3.WOT_OwnerReference = "00001";
			receipt3.WOT_Quantity = 350;
			receipt3.WOT_RN_NKOrigin = ZString.Empty;
			receipt3.WOT_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
			receipt3.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.CLS;
			receipt3.WOT_TotalValue = 1000m;
			receipt3.WOT_TransactionDate = ZDateTime.Today.Date;
			receipt3.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.REC;
			receipt3.WOT_IsFinal = true;
			receipt3.WOT_WOB_CusWHSTransactionBatch = batch.PK;

			receipt4 = Factory.New<CusWHSOperatorTransaction>();
			receipt4.WOT_BatchLineNo = 4;
			receipt4.WOT_ExportType = ZString.Empty;
			receipt4.WOT_IsCustomsControlled = false;
			receipt4.WOT_OH_ProductOwner = Helper.Importer.PK;
			receipt4.WOT_OP_Product = part1.PK;
			receipt4.WOT_OwnerReference = "00003";
			receipt4.WOT_Quantity = 350;
			receipt4.WOT_RN_NKOrigin = ZString.Empty;
			receipt4.WOT_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
			receipt4.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.QUE;
			receipt4.WOT_TotalValue = 1000m;
			receipt4.WOT_TransactionDate = ZDateTime.Today.Date;
			receipt4.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.REC;
			receipt4.WOT_IsFinal = true;
			receipt4.WOT_WOB_CusWHSTransactionBatch = batch.PK;

			receipt5 = Factory.New<CusWHSOperatorTransaction>();
			receipt5.WOT_BatchLineNo = 5;
			receipt5.WOT_ExportType = ZString.Empty;
			receipt5.WOT_IsCustomsControlled = false;
			receipt5.WOT_OH_ProductOwner = Helper.Importer.PK;
			receipt5.WOT_OP_Product = part1.PK;
			receipt5.WOT_OwnerReference = "00003";
			receipt5.WOT_Quantity = 350;
			receipt5.WOT_RN_NKOrigin = ZString.Empty;
			receipt5.WOT_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
			receipt5.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.QUE;
			receipt5.WOT_TotalValue = 1000m;
			receipt5.WOT_TransactionDate = ZDateTime.Today.Date;
			receipt5.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.REC;
			receipt5.WOT_IsFinal = true;
			receipt5.WOT_WOB_CusWHSTransactionBatch = batch2.PK;

			receipt6 = Factory.New<CusWHSOperatorTransaction>();
			receipt6.WOT_BatchLineNo = 6;
			receipt6.WOT_ExportType = ZString.Empty;
			receipt6.WOT_IsCustomsControlled = false;
			receipt6.WOT_OH_ProductOwner = Helper.Importer.PK;
			receipt6.WOT_OP_Product = part1.PK;
			receipt6.WOT_OwnerReference = "00004";
			receipt6.WOT_Quantity = 350;
			receipt6.WOT_RN_NKOrigin = ZString.Empty;
			receipt6.WOT_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
			receipt6.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.QUE;
			receipt6.WOT_TotalValue = 1000m;
			receipt6.WOT_TransactionDate = ZDateTime.Today.Date;
			receipt6.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.REC;
			receipt6.WOT_IsFinal = true;
			receipt6.WOT_WOB_CusWHSTransactionBatch = batch.PK;

			receipt7 = Factory.New<CusWHSOperatorTransaction>();
			receipt7.WOT_BatchLineNo = 7;
			receipt7.WOT_ExportType = ZString.Empty;
			receipt7.WOT_IsCustomsControlled = false;
			receipt7.WOT_OH_ProductOwner = Helper.Importer.PK;
			receipt7.WOT_OP_Product = part1.PK;
			receipt7.WOT_OwnerReference = "00009";
			receipt7.WOT_Quantity = 350;
			receipt7.WOT_RN_NKOrigin = ZString.Empty;
			receipt7.WOT_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
			receipt7.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.QUE;
			receipt7.WOT_TotalValue = 1000m;
			receipt7.WOT_TransactionDate = ZDateTime.Today.Date;
			receipt7.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.REC;
			receipt7.WOT_IsFinal = true;
			receipt7.WOT_WOB_CusWHSTransactionBatch = batch.PK;

			receipt8 = Factory.New<CusWHSOperatorTransaction>();
			receipt8.WOT_BatchLineNo = 8;
			receipt8.WOT_ExportType = ZString.Empty;
			receipt8.WOT_IsCustomsControlled = false;
			receipt8.WOT_OH_ProductOwner = Helper.Importer.PK;
			receipt8.WOT_OP_Product = part1.PK;
			receipt8.WOT_OwnerReference = "00006";
			receipt8.WOT_Quantity = 350;
			receipt8.WOT_RN_NKOrigin = ZString.Empty;
			receipt8.WOT_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
			receipt8.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.QUE;
			receipt8.WOT_TotalValue = 1000m;
			receipt8.WOT_TransactionDate = ZDateTime.Today.Date;
			receipt8.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.REC;
			receipt8.WOT_IsFinal = true;
			receipt8.WOT_WOB_CusWHSTransactionBatch = batch.PK;

			Factory.Save();
		}

		void AssertReceipt(ZString name, CusWHSOperatorTransaction receipt, ZBool isCustomsControlled, ZString origin, ZString status, ZString customsEntryNumber, ZDate intoBondDate)
		{
			var reloadedReceipt = new BusinessObjectFactory().Load<CusWHSOperatorTransaction>(receipt.PK);
			AssertEquals($"{name} - WOT_IsCustomsControlled", isCustomsControlled, reloadedReceipt.WOT_IsCustomsControlled);
			AssertEquals($"{name} - WOT_RN_NKOrigin", origin, reloadedReceipt.WOT_RN_NKOrigin);
			AssertEquals($"{name} - WOT_Status", status, reloadedReceipt.WOT_Status);
			AssertEquals($"{name} - WOT_CustomsEntryNumber", customsEntryNumber, reloadedReceipt.WOT_CustomsEntryNumber);
			AssertEquals($"{name} - WOT_IntoBondDate", intoBondDate, reloadedReceipt.WOT_IntoBondDate);
		}

		[TestDate(2023, 5, 1)]
		public void TestProcessUnderReceipt()
		{
			GlbCompany newCompany;
			CusWHSOperatorTransaction receipt1, receipt2, receipt3, receipt4, receipt5, receipt6, receipt7, receipt8;
			SetupData(out newCompany, out receipt1, out receipt2, out receipt3, out receipt4, out receipt5, out receipt6, out receipt7, out receipt8);
			receipt1.WOT_Quantity = 80;
			Factory.Save();

			var logger = InitialiseAndRunTaskSchedule(new ProcessReceiptService());

			CombineAssertions(() =>
			{
				AssertReceipt("Receipt 1", receipt1, true, "ZA", WarehouseOperatorTransactionStatusList.Codes.VAL, "001202304255000522", new ZDate(2023, 4, 25));
				AssertReceipt("Receipt 2", receipt2, false, "AU", WarehouseOperatorTransactionStatusList.Codes.VAL, "MRN123", ZDate.Empty);
				AssertReceipt("Receipt 3", receipt3, false, ZString.Empty, WarehouseOperatorTransactionStatusList.Codes.CLS, ZString.Empty, ZDate.Empty);
				AssertReceipt("Receipt 4", receipt4, true, "ZA", WarehouseOperatorTransactionStatusList.Codes.VAL, "002202304255000522", new ZDate(2023, 4, 25));
				AssertReceipt("Receipt 5", receipt5, false, "ZA", WarehouseOperatorTransactionStatusList.Codes.VAL, ZString.Empty, ZDate.Empty);
				AssertReceipt("Receipt 6", receipt6, true, "ZA", WarehouseOperatorTransactionStatusList.Codes.VAL, "NotAnEntryNumberWithDate", ZDate.Empty);
				AssertReceipt("Receipt 7", receipt7, false, "ZA", WarehouseOperatorTransactionStatusList.Codes.VAL, ZString.Empty, ZDate.Empty);

				var newBatchQuery = new ZDBOnlyQuery(typeof(CusWHSOperatorTransactionBatch));
				newBatchQuery.AddToFilter(CusWHSOperatorTransactionBatchSchema.WOB_IsSystemCreated, true);
				var newBatch = Factory.Load<CusWHSOperatorTransactionBatch>(newBatchQuery).FirstOrDefault();
				AssertNotNull("System created batch should be created", newBatch);
				AssertContains("Under Receipt", newBatch.WOB_Batch);

				var adjReceiptQuery = new ZDBOnlyQuery(typeof(CusWHSOperatorTransaction));
				adjReceiptQuery.AddToFilter(CusWHSOperatorTransactionSchema.WOT_TransactionType, WarehouseOperatorTransactionTypeList.Codes.ADJ);
				var adjReceipts = Factory.Load<CusWHSOperatorTransaction>(adjReceiptQuery);
				AssertEquals("Should be 1 'ADJ' receipt", 1, adjReceipts.Length);
				AssertEquals("New batch PK", newBatch.PK, adjReceipts[0].WOT_WOB_CusWHSTransactionBatch);
				AssertEquals("AdjRec[0] Is Customs controlled", true, adjReceipts[0].WOT_IsCustomsControlled);
				AssertEquals("AdjRec[0] Origin", ZString.Empty, adjReceipts[0].WOT_RN_NKOrigin);
				AssertEquals("AdjRec[0] Status", WarehouseOperatorTransactionStatusList.Codes.VAL, adjReceipts[0].WOT_Status);
				AssertEquals("AdjRec[0] IsFinal", false, adjReceipts[0].WOT_IsFinal);
				AssertEquals("AdjRec[0] Total Value", 0m, adjReceipts[0].WOT_TotalValue);
				AssertEquals("AdjRec[0] Qty", 20m, adjReceipts[0].WOT_Quantity);

				var logs = logger.ToString();
				AssertContains("Logger 1", $"Processing receipts for Company {GlbCompany.CurrentCompany.GC_Code}", logs);
				AssertContains("Logger 2", $"Processing receipts for Company {newCompany.GC_Code}", logs);
				AssertContains("Logger 3", "6 Receipt(s) processed", logs);
				AssertContains("Logger 4", "Information|3 Receipt(s) matched to warehouse entries", logs);
				AssertContains("Logger 5", "Information|1 Receipt(s) matched to customs entries", logs);
				AssertContains("Logger 6", "1 Receipt(s) processed", logs);
				AssertContains("Logger 7", "Information|0 Receipt(s) matched to warehouse entries", logs);
				AssertContains("Logger 8", "Information|0 Receipt(s) matched to customs entries", logs);
				AssertContains("Logger 9", "Creating system batch for under receipts", logs);
				AssertContains("Logger 10", "Creating under receipt", logs);
			});
		}

		Customs.Business.OrgSupplierPart CreatePart(ZString partNo, OrgHeader owner, OrgHeader supplier)
		{
			var part = Factory.New<Customs.Business.OrgSupplierPart>();
			part.OP_PartNum = partNo;
			part.OP_Desc = partNo + " DESC";
			part.OP_StockKeepingUnit = "NO";
			if (owner != null)
			{
				part.RelatedOrganisations.AddOwner(owner);
			}

			if (supplier != null)
			{
				part.RelatedOrganisations.AddSupplier(supplier);
			}

			return part;
		}

		OrgHeader CreateOrganisation(ZString code, bool isConsignee, bool isConsignor)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = code;
			org.OH_FullName = code + " NAME";
			org.OH_IsConsignee = isConsignee;
			org.OH_IsConsignor = isConsignor;
			org.MainAddress.Address1 = code + " ADDRESS 1";
			return org;
		}

		WhsDataTestHelper Helper => helper ?? (helper = new WhsDataTestHelper(Factory));
		WhsDataTestHelper helper;

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						CusWHSOperatorTransactionSchema.Constants.TableName,
						"ZA External Warehouse Receipt Processor",
						CusWHSOperatorTransactionSchema.Constants.WOT_TransactionType + "=" + WarehouseOperatorTransactionTypeList.Codes.REC,
						CusWHSOperatorTransactionSchema.Constants.WOT_Status + "=" + WarehouseOperatorTransactionStatusList.Codes.QUE)
				};
			}
		}
	}
}
