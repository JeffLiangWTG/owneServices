using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.Business.WarehouseIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business.OperationalActions
{
	class ClearExpiredStockOperationalActionRunner : ExportOperationalActionRunner
	{
		public ClearExpiredStockOperationalActionRunner(IOperationalActionSectionLog log, BusinessObjectFactory factory) : base(log, factory)
		{
		}

		protected override string ExportType => throw new NotImplementedException();

		protected override bool SortAscending => throw new NotImplementedException();

		public override void Run(IEnumerable<OperatorTransactionSelection> selections)
		{
			var linesPerWarehouse = ExpireStockAndGetEntryLines(selections);
			if (linesPerWarehouse != null)
			{
				foreach (var lines in linesPerWarehouse)
				{
					var messageNumber = ExbondShipmentUsxml.CreateEDIMessage(factory, warehousePK: lines.Key, lines.Value, ExbondShipmentUsxml.Mode.UnderReceipts);
					LogResultForSingleMessage(messageNumber);
				}
			}
		}

		Dictionary<Guid, List<ExbondEntryLine>> ExpireStockAndGetEntryLines(IEnumerable<OperatorTransactionSelection> selections)
		{
			var warehouseSelections = selections.ToLookup(x => x.WarehouseAddress.ToGuid(), x => x);
			if (warehouseSelections.Count == 0)
			{
				log.Notify(OperationalActionLogErrorLevel.Error, "No records were selected.");
				return null;
			}

			var helper = new ExternalWarehouseAllocationHelper(factory);
			var entryLines = new Dictionary<Guid, List<ExbondEntryLine>>();

			foreach (var warehouse in warehouseSelections)
			{
				CusWHSOperatorTransactionBatch batch = null;
				var batchLineNumber = 0;
				var transactionLines = new List<CusWHSOperatorTransactionLine>();

				foreach (var transactionGroup in warehouse)
				{
					var transactions = GetIndividualTransactions(transactionGroup);
					foreach (DynamicBusinessObject transaction in transactions)
					{
						var outstandingReceiptQty = (ZDecimal)transaction["OutstandingReceiptQty"];
						if (outstandingReceiptQty > 0)
						{
							if (batch == null)
							{
								batch = CreateNewBatch(warehouse.Key, GlbCompany.CurrentCompany.PK);
							}

							var transactionPk = (ZGuid)transaction[CusWHSOperatorTransaction.Schema.PK];
							var originalTransaction = factory.Load<CusWHSOperatorTransaction>(transactionPk);
							originalTransaction.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.CLS;

							var newOrder = CreateNewOrder(batch.PK, originalTransaction, outstandingReceiptQty, ++batchLineNumber);
							transactionLines.Add(CreateNewTransactionLine(transactionPk, newOrder.PK, outstandingReceiptQty));
							CreateNewReceipt(batch.PK, originalTransaction, outstandingReceiptQty, ++batchLineNumber);
						}
					}
				}

				var newEntryLines = helper.Allocate(transactionLines);
				if (newEntryLines.Count > 0)
				{
					entryLines.Add(warehouse.Key, newEntryLines);
				}
			}
			factory.Save();

			if (entryLines.Count == 0)
			{
				log.Notify(OperationalActionLogErrorLevel.Warning, "No Bond store records affected");
				return null;
			}

			return entryLines;
		}

		internal static string TransactionsQuerySql => @"SELECT WOT_PK, WOT_Quantity - SUM(ISNULL(WOL_Quantity, 0)) AS OutstandingReceiptQty
FROM dbo.CusWHSOperatorTransaction
LEFT OUTER JOIN dbo.CusWHSOperatorTransactionLine ON WOT_PK = WOL_WOT_WHSOperatorTransactionReceipt 
WHERE WOT_IsCustomsControlled = 1 AND WOT_Status <> 'CLS' AND WOT_TransactionType = 'REC'
	AND WOT_OwnerReference = @OwnerReference
	AND WOT_CustomsEntryNumber = @CustomsEntryNumber
	AND WOT_IntoBondDate = @IntoBondDate
	AND WOT_WOB_CusWHSTransactionBatch = @Batch
GROUP BY WOT_PK, WOT_Quantity";

		protected internal DynamicBusinessObjectCollection GetIndividualTransactions(OperatorTransactionSelection selection)
		{
			var sqlParameters = new ZSqlParameter[]
			{
				ZSqlParameter.New("@OwnerReference", selection.OwnerReference, CusWHSOperatorTransactionSchema.WOT_OwnerReference),
				ZSqlParameter.New("@CustomsEntryNumber", selection.CustomsEntryNumber, CusWHSOperatorTransactionSchema.WOT_CustomsEntryNumber),
				ZSqlParameter.New("@IntoBondDate", selection.IntoBondDate, CusWHSOperatorTransactionSchema.WOT_IntoBondDate),
				ZSqlParameter.New("@Batch", selection.Batch, CusWHSOperatorTransactionSchema.WOT_WOB_CusWHSTransactionBatch),
			};
			var collection = new DynamicBusinessObjectCollection(factory);
			collection.Load(TransactionsQuerySql, sqlParameters);
			return collection;
		}

		CusWHSOperatorTransactionBatch CreateNewBatch(ZGuid oaWarehouse, ZGuid gcCompany)
		{
			var batch = factory.New<CusWHSOperatorTransactionBatch>();
			batch.WOB_Batch = $"Expired Stock Batch {ZDate.Today:yyyy-MMM-dd HH:mm}";
			batch.WOB_GC_Company = gcCompany;
			batch.WOB_OA_Warehouse = oaWarehouse;
			batch.WOB_IsSystemCreated = true;
			return batch;
		}

		CusWHSOperatorTransaction CreateNewOrder(ZGuid batchPK, CusWHSOperatorTransaction originalTransaction, ZDecimal outstandingReceiptQty, int batchLineNumber)
		{
			var transaction = CreateNewTransaction(batchPK, originalTransaction, outstandingReceiptQty, batchLineNumber);
			transaction.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.ORD;
			transaction.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.CLS;
			transaction.WOT_CustomsEntryNumber = originalTransaction.WOT_CustomsEntryNumber;
			return transaction;
		}

		CusWHSOperatorTransaction CreateNewReceipt(ZGuid batchPK, CusWHSOperatorTransaction originalTransaction, ZDecimal outstandingReceiptQty, int batchLineNumber)
		{
			var transaction = CreateNewTransaction(batchPK, originalTransaction, outstandingReceiptQty, batchLineNumber);
			transaction.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.REC;
			transaction.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.VAL;
			transaction.WOT_CustomsEntryNumber = ZString.Empty;
			return transaction;
		}

		CusWHSOperatorTransaction CreateNewTransaction(ZGuid batchPK, CusWHSOperatorTransaction originalTransaction, ZDecimal outstandingReceiptQty, int batchLineNumber)
		{
			var transaction = factory.New<CusWHSOperatorTransaction>();
			transaction.WOT_WOB_CusWHSTransactionBatch = batchPK;
			transaction.WOT_TransactionDate = ZDate.Today;
			transaction.WOT_OwnerReference = originalTransaction.WOT_OwnerReference;
			transaction.WOT_OP_Product = originalTransaction.WOT_OP_Product;
			transaction.WOT_OH_ProductOwner = originalTransaction.WOT_OH_ProductOwner;
			transaction.WOT_Quantity = outstandingReceiptQty;
			transaction.WOT_TotalValue = 0;
			transaction.WOT_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
			transaction.WOT_IsCustomsControlled = false;
			transaction.WOT_RN_NKOrigin = ZString.Empty;
			transaction.WOT_BatchLineNo = batchLineNumber;
			transaction.WOT_LineReference = originalTransaction.WOT_LineReference;
			return transaction;
		}

		CusWHSOperatorTransactionLine CreateNewTransactionLine(ZGuid originalTransactionPK, ZGuid newOrderPK, ZDecimal outstandingReceiptQty)
		{
			var line = factory.New<CusWHSOperatorTransactionLine>();
			line.WOL_WOT_WHSOperatorTransactionReceipt = originalTransactionPK;
			line.WOL_WOT_WHSOperatorTransactionOrder = newOrderPK;
			line.WOL_Quantity = outstandingReceiptQty;
			return line;
		}
	}
}
