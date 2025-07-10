using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusWHSOperatorTransactionNettOffProcedureTest : TestCaseWithFactory
	{
		[TestDate(2023, 3, 9, 11, 12, 13)]
		public void TestExecute()
		{
			var selections = new string[] { "OwnRef001" };
			TestTransactions(selections);
		}

		[TestDate(2023, 3, 9, 11, 12, 13)]
		public void TestExecute_IdenticalReferences()
		{
			var selections = new string[] { "OwnRef001", "OwnRef001" };
			TestTransactions(selections);
		}

		void TestTransactions(string[] selections)
		{
			var whs = PrepareStock();

			var batch = Factory.New<CusWHSOperatorTransactionBatch>();
			batch.WOB_Batch = "Test Batch 1";
			batch.WOB_GC_Company = GlbCompany.CurrentCompany.PK;
			batch.WOB_OA_Warehouse = whs.WW_OA_WarehouseAddress;

			var order1 = Factory.New<CusWHSOperatorTransaction>();
			order1.WOT_WOB_CusWHSTransactionBatch = batch.PK;
			order1.WOT_BatchLineNo = 1;
			order1.WOT_TransactionType = "ORD";
			order1.WOT_ExportType = "EXP";
			order1.WOT_TransactionDate = ZDateTime.Today.Date;
			order1.WOT_OwnerReference = "OwnRef001";
			order1.WOT_OP_Product = Helper.Part.PK;
			order1.WOT_OH_ProductOwner = Helper.Importer.PK;
			order1.WOT_Quantity = 25m;
			order1.WOT_TotalValue = 3000m;
			order1.WOT_RX_NKCurrency = "ZAR";
			order1.WOT_Status = "VAL";
			order1.WOT_SystemLastEditTimeUtc = new ZDateTime(2023, 3, 9, DateTimeKind.Utc);

			var order2 = Factory.New<CusWHSOperatorTransaction>();
			order2.WOT_WOB_CusWHSTransactionBatch = batch.PK;
			order2.WOT_BatchLineNo = 2;
			order2.WOT_TransactionType = "ORD";
			order2.WOT_ExportType = "EXP";
			order2.WOT_TransactionDate = ZDateTime.Today.Date;
			order2.WOT_OwnerReference = "OwnRef002";
			order2.WOT_OP_Product = Helper.Part.PK;
			order2.WOT_OH_ProductOwner = Helper.Importer.PK;
			order2.WOT_Quantity = 20m;
			order2.WOT_TotalValue = 2000m;
			order2.WOT_RX_NKCurrency = "ZAR";
			order2.WOT_Status = "VAL";

			var receipt1 = Factory.New<CusWHSOperatorTransaction>();
			receipt1.WOT_WOB_CusWHSTransactionBatch = batch.PK;
			receipt1.WOT_BatchLineNo = 3;
			receipt1.WOT_TransactionType = "REC";
			receipt1.WOT_ExportType = "";
			receipt1.WOT_TransactionDate = ZDateTime.Today.Date;
			receipt1.WOT_OwnerReference = "OwnRef001";
			receipt1.WOT_OP_Product = Helper.Part.PK;
			receipt1.WOT_OH_ProductOwner = Helper.Importer.PK;
			receipt1.WOT_Quantity = 40m;
			receipt1.WOT_TotalValue = 5000m;
			receipt1.WOT_RX_NKCurrency = "ZAR";
			receipt1.WOT_Status = "VAL";
			receipt1.WOT_IsCustomsControlled = true;
			receipt1.WOT_CustomsEntryNumber = "EN00123";

			Factory.Save();

			var transactions = CusWHSOperatorTransactionNettOffProcedure.Execute(Factory, GlbCompany.CurrentCompany.PK.ToGuid(),
				whs.WW_OA_WarehouseAddress.ToGuid(), Helper.Importer.PK.ToGuid(), "EXP", selections, true, GlbStaff.CurrentUser.GS_Code);

			CombineAssertions(() =>
			{
				AssertNotNull("Transactions should not be null", transactions);
				AssertEquals("1 Result", 1, transactions.Count);

				var transaction = transactions.First();

				AssertNotEquals("PK", Guid.Empty, transaction.PK);
				AssertEquals("OrderPK", order1.PK, transaction.OrderPK);
				AssertEquals("ReceiptPK", receipt1.PK, transaction.ReceiptPK);
				AssertEquals("Quantity", 25m, transaction.Quantity);
				AssertEquals("EntryLineNo", (short)4, transaction.EntryLineNo);

				order1.Reload();

				AssertEquals("WOT_Status", "CLS", order1.WOT_Status);
				AssertEquals("EditDate", new ZDateTime(2023, 3, 9, 11, 12, 0), order1.WOT_SystemLastEditTimeUtc);
			});
		}

		Warehouse.Integration.IWhsWarehouse PrepareStock()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, isVirtualWarehouse: true, "N10");
			var whsReceive1 = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK, "RCV1", ZDateTimeOffset.Today.AddDays(-3));
			_ = Helper.GetNewReceiveInventory(whsReceive1, Helper.Part, "PACKAGE1", 100m, 100m, 100m, bondedEntryKey: "EN00123-4");
			Helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive1.PK);
			whsReceive1.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			return whsWarehouse;
		}

		WhsDataTestHelper Helper => helper ?? (helper = new WhsDataTestHelper(Factory));
		WhsDataTestHelper helper;
	}
}
