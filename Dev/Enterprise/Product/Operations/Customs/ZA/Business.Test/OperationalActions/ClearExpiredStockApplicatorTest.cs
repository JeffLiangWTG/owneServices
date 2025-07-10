using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.OperationalActions.Testing
{
	[TestedType(typeof(ClearExpiredStockApplicator))]
	class ClearExpiredStockApplicatorTest : LockedOperationalActionMethodApplicatorTest<ClearExpiredStockApplicator>
	{
		public void TestRecordFilter()
		{
			var today = ZDate.Today;
			var startDate = today.AddMonths(-22);
			var whsHelper = new WhsDataTestHelper(Factory);
			var warehouse = whsHelper.GetNewWhsWarehouse(whsHelper.Warehouse.MainAddress.PK, isVirtualWarehouse: true, "N10");

			var batch = Factory.New<CusWHSOperatorTransactionBatch>();
			batch.WOB_Batch = "Test Batch 1";
			batch.WOB_GC_Company = GlbCompany.CurrentCompany.PK;
			batch.WOB_OA_Warehouse = warehouse.WW_OA_WarehouseAddress;

			var transactionTypeList = new List<string>
			{
				WarehouseOperatorTransactionTypeList.Codes.REC,
				WarehouseOperatorTransactionTypeList.Codes.ADJ,
				WarehouseOperatorTransactionTypeList.Codes.ORD
			};
			var statusList = new List<string>
			{
				WarehouseOperatorTransactionStatusList.Codes.VAL,
				WarehouseOperatorTransactionStatusList.Codes.CLS
			};

			var transactions = new List<CusWHSOperatorTransaction>();
			var batchLineNo = 1;
			foreach (var transactionType in transactionTypeList)
			{
				foreach (var status in statusList)
				{
					for (var cc = 0; cc < (transactionType == WarehouseOperatorTransactionTypeList.Codes.REC ? 2 : 1); ++cc)
					{
						for (var i = 1; i < 10; ++i)
						{
							var transaction = Factory.New<CusWHSOperatorTransaction>();
							transaction.WOT_WOB_CusWHSTransactionBatch = batch.PK;
							transaction.WOT_BatchLineNo = batchLineNo++;
							transaction.WOT_TransactionType = transactionType;
							transaction.WOT_TransactionDate = today;
							transaction.WOT_OwnerReference = $"Ref{i}";
							transaction.WOT_OH_ProductOwner = whsHelper.Importer.PK;
							transaction.WOT_OP_Product = whsHelper.Part.PK;
							transaction.WOT_Quantity = 1;
							transaction.WOT_TotalValue = 100m;
							transaction.WOT_RX_NKCurrency = "ZAR";
							transaction.WOT_Status = status;
							transaction.WOT_SystemLastEditTimeUtc = ZDateTime.UtcNow;
							transaction.WOT_IntoBondDate = startDate.AddDays(-i);
							transaction.WOT_IsCustomsControlled = cc == 1;
							transaction.WOT_CustomsEntryNumber = "";
							transactions.Add(transaction);
						}
					}
				}
			}
			Factory.Save();

			dateEntered = startDate.AddDays(-5);
			Applicator.Build(transactions.Select(x => x.PK).ToArray());
			try
			{
				AssertEquals("Applicator.Records.Count", 4, Applicator.Records.Count);
				AssertEquals("Applicator.Records.Where(Select).Count", 4, Applicator.Records.Where(x => x.Select).Count());
				AssertContainsExactElementsInExactOrder("OwnerReferences", new List<ZString> { "Ref6", "Ref7", "Ref8", "Ref9" }, Applicator.Records.Select(x => x.OwnerReference));
			}
			finally
			{
				Applicator.Unlock();
			}
		}

		public void TestQuerySql()
		{
			var fields = new List<string>
			{
				CusWHSOperatorTransaction.Schema.WOT_CustomsEntryNumber,
				CusWHSOperatorTransaction.Schema.WOT_OwnerReference,
				CusWHSOperatorTransaction.Schema.WOT_IntoBondDate,
				CusWHSOperatorTransaction.Schema.WOT_WOB_CusWHSTransactionBatch,
				CusWHSOperatorTransaction.Schema.WOT_TransactionType,
				CusWHSOperatorTransaction.Schema.WOT_Status,
				CusWHSOperatorTransaction.Schema.WOT_IsCustomsControlled,
				CusWHSOperatorTransaction.Schema.WOT_OH_ProductOwner,
				CusWHSOperatorTransactionBatch.Schema.WOB_OA_Warehouse,
				CusWHSOperatorTransactionBatch.Schema.PK,
				CusWHSOperatorTransactionBatch.Schema.WOB_GC_Company,
			};

			var querySql = ClearExpiredStockApplicator.QuerySql;

			foreach (var fieldName in fields)
			{
				AssertContains(fieldName, querySql);
			}
		}

		protected override OperationalActionMethodApplicator GetOtherApplicator() => new ExWarehouseApplicator(Factory, null, null);

		protected override BusinessObject GetNewBusinessObject() => new ClearExpiredStockApplicator(Factory, () => dateEntered);
		ZDate dateEntered;
	}
}
