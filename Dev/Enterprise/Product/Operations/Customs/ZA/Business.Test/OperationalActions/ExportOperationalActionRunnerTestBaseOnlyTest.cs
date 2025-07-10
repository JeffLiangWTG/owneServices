using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;

namespace Enterprise.Customs.ZA.Business.OperationalActions.Testing
{
	sealed class ExportOperationalActionRunnerTestBaseOnlyTest : TestCaseWithFactory
	{
		public void TestHaveSufficientQuantitiesForAllocation()
		{
			helper = new WhsDataTestHelper(Factory);
			var otherOwner = Factory.NewWithValidTestData<OrgHeader>();
			var otherPart = helper.CreateProduct(otherOwner.PK, helper.Part.OP_PartNum);

			var batch = CreateBatch(helper.Warehouse);
			CreateOrder(batch.PK, 1, helper.Importer.PK, helper.Part.PK, 11.0);
			CreateReceipt(batch.PK, 2, helper.Importer.PK, helper.Part.PK, 10.0);

			var batch2 = CreateBatch(helper.Warehouse);
			CreateOrder(batch2.PK, 1, otherOwner.PK, otherPart.PK, 7.0);
			CreateReceipt(batch2.PK, 2, otherOwner.PK, otherPart.PK, 1.0);

			var batch3 = CreateBatch(helper.Warehouse2);
			CreateOrder(batch3.PK, 1, helper.Importer.PK, helper.Part.PK, 3.0);
			CreateReceipt(batch3.PK, 2, helper.Importer.PK, helper.Part.PK, 2.0);

			Factory.Save();

			var log = new DummyOperationalActionSectionLog();
			var runner = new ExportOperationalActionRunnerForTest(log, Factory);

			var selection = new OperatorTransactionSelection
			{
				Select = true,
				WarehouseAddress = helper.Warehouse.MainAddress.PK,
				ProductOwner = helper.Importer.PK,
				ProductCode = helper.Part.OP_PartNum,
				OwnerReference = "OwnerRef123",
			};
			var selections = new List<OperatorTransactionSelection> { selection };

			runner.Run(selections);
			var messages = log.MessagesString();
			AssertNotNullOrEmpty("Warning should have been logged", messages);
			AssertContains("Order Quantity is 11", "Qty Requested of 11", messages);
			AssertContains("Receipt Quantity is 10", "Qty available of 10", messages);
		}

		CusWHSOperatorTransactionBatch CreateBatch(OrgHeader warehouse)
		{
			var batch = Factory.New<CusWHSOperatorTransactionBatch>();
			batch.WOB_Batch = "Test Batch 1";
			batch.WOB_GC_Company = GlbCompany.CurrentCompany.PK;
			batch.WOB_OA_Warehouse = warehouse.MainAddress.PK;
			return batch;
		}

		void CreateOrder(ZGuid batchPK, ZInt batchLineNo, ZGuid productOwnerPK, ZGuid productPK, ZDecimal quantity)
		{
			var transaction = CreateTransaction(batchPK, batchLineNo, productOwnerPK, productPK, quantity);
			transaction.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.ORD;
		}

		void CreateReceipt(ZGuid batchPK, ZInt batchLineNo, ZGuid productOwnerPK, ZGuid productPK, ZDecimal quantity)
		{
			var transaction = CreateTransaction(batchPK, batchLineNo, productOwnerPK, productPK, quantity);
			transaction.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.REC;
		}

		CusWHSOperatorTransaction CreateTransaction(ZGuid batchPK, ZInt batchLineNo, ZGuid productOwnerPK, ZGuid productPK, ZDecimal quantity)
		{
			var transaction = Factory.New<CusWHSOperatorTransaction>();
			transaction.WOT_WOB_CusWHSTransactionBatch = batchPK;
			transaction.WOT_BatchLineNo = batchLineNo;
			transaction.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.REC;
			transaction.WOT_TransactionDate = ZDate.Today;
			transaction.WOT_OwnerReference = "OwnerRef123";
			transaction.WOT_OH_ProductOwner = productOwnerPK;
			transaction.WOT_OP_Product = productPK;
			transaction.WOT_Quantity = quantity;
			transaction.WOT_TotalValue = quantity * 100m;
			transaction.WOT_RX_NKCurrency = "ZAR";
			transaction.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.VAL;
			transaction.WOT_SystemLastEditTimeUtc = ZDateTime.UtcNow;

			return transaction;
		}

		WhsDataTestHelper helper;
	}

	class ExportOperationalActionRunnerForTest : ExportOperationalActionRunner
	{
		public ExportOperationalActionRunnerForTest(IOperationalActionSectionLog log, BusinessObjectFactory factory) : base(log, factory)
		{
		}

		protected override string ExportType => throw new NotImplementedException();

		protected override bool SortAscending => throw new NotImplementedException();

		public override void Run(IEnumerable<OperatorTransactionSelection> selections)
		{
			_ = HaveSufficientQuantitiesForAllocation(selections);
		}
	}
}
