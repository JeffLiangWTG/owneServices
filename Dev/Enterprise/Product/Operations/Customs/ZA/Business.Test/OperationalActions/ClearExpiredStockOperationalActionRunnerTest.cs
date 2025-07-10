using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business.OperationalActions.Testing
{
	class ClearExpiredStockOperationalActionRunnerTest : ExportOperationalActionRunnerTest
	{
		public void TestRunner()
		{
			var startDate = today.AddMonths(-22).AddDays(-1);
			var transaction = CreateReceipt("Ref1", 1m, "EN00123-1");
			transaction.WOT_IntoBondDate = startDate;
			Factory.Save();

			var runner = new ClearExpiredStockOperationalActionRunner(log, Factory);
			var selections = new List<OperatorTransactionSelection>
			{
				new OperatorTransactionSelection
				{
					CustomsEntryNumber = transaction.WOT_CustomsEntryNumber,
					OwnerReference = transaction.WOT_OwnerReference,
					IntoBondDate = transaction.WOT_IntoBondDate,
					WarehouseAddress = warehouse.WW_OA_WarehouseAddress,
					ProductOwner = transaction.WOT_OH_ProductOwner,
					Batch = batch.PK,
				}
			};
			runner.Run(selections);
			var shipment = LoadShipmentByLoggedMessageNumber();

			AssertNotNull("CommercialInfo", shipment.CommercialInfo);
			AssertNotNull("CommercialInfo/CommercialInvoiceCollection", shipment.CommercialInfo.CommercialInvoiceCollection);
			var invoice1 = shipment.CommercialInfo.CommercialInvoiceCollection[0];
			AssertNotNull("CommercialInfo/CommercialInvoiceCollection", invoice1.CommercialInvoiceLineCollection);
			var invoiceLine = invoice1.CommercialInvoiceLineCollection[0];
			AssertEquals("CommercialInvoice1/CommercialInvoiceLine1/BondedWarehouseQuantity", 1m, invoiceLine.BondedWarehouseQuantity);

			transaction.Reload();
			AssertEquals("Original receipt Status", WarehouseOperatorTransactionStatusList.Codes.CLS, transaction.WOT_Status);

			var batchQuery = new ZQuery(CusWHSOperatorTransactionBatchSchema.WOB_Batch, SQLComparisonOperator.Like, "Expired Stock Batch %");
			var expiryBatch = Factory.Load<CusWHSOperatorTransactionBatch>(batchQuery);
			AssertEquals("Should find an 'Expired Stock Batch'", 1, expiryBatch.Length);

			var expiryBatchPk = expiryBatch[0].PK;
			var transactionsQuery = new ZQuery(CusWHSOperatorTransactionSchema.WOT_WOB_CusWHSTransactionBatch, expiryBatchPk);
			var batchTransactions = Factory.Load<CusWHSOperatorTransaction>(transactionsQuery);
			AssertContainsExactElementsInAnyOrder("Batch transactions: type", new List<ZString> { WarehouseOperatorTransactionTypeList.Codes.ORD, WarehouseOperatorTransactionTypeList.Codes.REC }, batchTransactions.Select(x => x.WOT_TransactionType));

			var expiryReceipt = batchTransactions[0].WOT_TransactionType == WarehouseOperatorTransactionTypeList.Codes.REC ? batchTransactions[0] : batchTransactions[1];
			var expiryOrder = batchTransactions[0].WOT_TransactionType == WarehouseOperatorTransactionTypeList.Codes.ORD ? batchTransactions[0] : batchTransactions[1];

			AssertEquals("Expiry receipt Status", WarehouseOperatorTransactionStatusList.Codes.VAL, expiryReceipt.WOT_Status);
			AssertEquals("Expiry order Status", WarehouseOperatorTransactionStatusList.Codes.CLS, expiryOrder.WOT_Status);

			var transactionLineQuery = new ZQuery(CusWHSOperatorTransactionLineSchema.WOL_WOT_WHSOperatorTransactionReceipt, transaction.PK);
			transactionLineQuery.AddToFilter(CusWHSOperatorTransactionLineSchema.WOL_WOT_WHSOperatorTransactionOrder, expiryOrder.PK);
			var expiryTransactionLine = Factory.Load<CusWHSOperatorTransactionLine>(transactionLineQuery);

			AssertEquals("Should find a CusWHSOperatorTransactionLine for the original receipt and new order", 1, expiryTransactionLine.Length);
			AssertEquals("CusWHSOperatorTransactionLine Quantity", 1m, expiryTransactionLine[0].WOL_Quantity);
		}

		public void TestTransactionsQuerySql()
		{
			var fields = new List<string>
			{
				CusWHSOperatorTransaction.Schema.PK,
				CusWHSOperatorTransaction.Schema.WOT_Quantity,
				CusWHSOperatorTransactionLine.Schema.WOL_Quantity,
				CusWHSOperatorTransactionLine.Schema.WOL_WOT_WHSOperatorTransactionReceipt,
				CusWHSOperatorTransaction.Schema.WOT_IsCustomsControlled,
				CusWHSOperatorTransaction.Schema.WOT_Status,
				CusWHSOperatorTransaction.Schema.WOT_TransactionType,
				CusWHSOperatorTransaction.Schema.WOT_OwnerReference,
				CusWHSOperatorTransaction.Schema.WOT_CustomsEntryNumber,
				CusWHSOperatorTransaction.Schema.WOT_IntoBondDate,
				CusWHSOperatorTransaction.Schema.WOT_WOB_CusWHSTransactionBatch,
			};

			var querySql = ClearExpiredStockOperationalActionRunner.TransactionsQuerySql;

			foreach (var fieldName in fields)
			{
				AssertContains(fieldName, querySql);
			}
		}

		public void TestGetIndividualTransactions()
		{
			var date = today.AddMonths(-22).AddDays(-1);
			var transaction = CreateReceipt("Ref1", 10m, "EN00123-1");
			transaction.WOT_IntoBondDate = date;
			Factory.Save();

			var runner = new ClearExpiredStockOperationalActionRunnerForTesting(log, Factory);
			var selection = new OperatorTransactionSelection
			{
				CustomsEntryNumber = "EN00123-1",
				OwnerReference = "Ref1",
				IntoBondDate = date,
				WarehouseAddress = warehouse.WW_OA_WarehouseAddress,
				ProductOwner = transaction.WOT_OH_ProductOwner,
				Batch = batch.PK,
			};
			var results = runner.GetIndividualTransactions(selection);

			AssertEquals(1, results.Count);
			AssertEquals("Result PK should match the original receipt PK", transaction.PK, results[0][CusWHSOperatorTransaction.Schema.PK]);
			AssertEquals(10m, results[0]["OutstandingReceiptQty"]);

			var line1 = Factory.New<CusWHSOperatorTransactionLine>();
			line1.WOL_WOT_WHSOperatorTransactionOrder = CreateOrder(ZString.Empty, "Ref1", 1m).PK;
			line1.WOL_WOT_WHSOperatorTransactionReceipt = transaction.PK;
			line1.WOL_Quantity = 1m;
			var line2 = Factory.New<CusWHSOperatorTransactionLine>();
			line2.WOL_WOT_WHSOperatorTransactionOrder = CreateOrder(ZString.Empty, "Ref1", 2.5m).PK;
			line2.WOL_WOT_WHSOperatorTransactionReceipt = transaction.PK;
			line2.WOL_Quantity = 2.5m;
			Factory.Save();

			results = runner.GetIndividualTransactions(selection);
			AssertEquals(1, results.Count);
			AssertEquals("Result PK should match the original receipt PK", transaction.PK, results[0][CusWHSOperatorTransaction.Schema.PK]);
			AssertEquals("After two lines added", 6.5m, results[0]["OutstandingReceiptQty"]);
		}

		class ClearExpiredStockOperationalActionRunnerForTesting : ClearExpiredStockOperationalActionRunner
		{
			public ClearExpiredStockOperationalActionRunnerForTesting(IOperationalActionSectionLog log, BusinessObjectFactory factory) : base(log, factory)
			{
			}

			public new DynamicBusinessObjectCollection GetIndividualTransactions(OperatorTransactionSelection selection) => base.GetIndividualTransactions(selection);
		}
	}
}
