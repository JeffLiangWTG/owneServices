using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	abstract class ExportOperationalActionRunner
	{
		public ExportOperationalActionRunner(IOperationalActionSectionLog log, BusinessObjectFactory factory)
		{
			this.log = log;
			this.factory = factory;
		}

		public abstract void Run(IEnumerable<OperatorTransactionSelection> selections);

		protected Dictionary<Guid, List<ExbondEntryLine>> ProcessAndGetEntryLines(IEnumerable<OperatorTransactionSelection> selections)
		{
			var warehouseSelections = selections.ToLookup(x => x.WarehouseAddress.ToGuid(), x => x);
			if (warehouseSelections.Count == 0)
			{
				log.Notify(OperationalActionLogErrorLevel.Error, "No records were selected.");
				return null;
			}

			if (CheckSufficientQuantitiesForAllocation && !HaveSufficientQuantitiesForAllocation(selections))
			{
				if (!AskForConfirmationToContinueWithInsufficientStock())
				{
					log.Notify(OperationalActionLogErrorLevel.Error, "Operation cancelled.");
					return null;
				}
			}

			var helper = new ExternalWarehouseAllocationHelper(factory);
			var entryLines = new Dictionary<Guid, List<ExbondEntryLine>>();

			using (var transactionManager = ((CargoWise.Data.IDbConnected)factory).Connection.BeginTransactionWithManager())
			{
				foreach (var warehouse in warehouseSelections)
				{
					var lines = CreateOrSelectLinesForSelections(warehouse);
					var newEntryLines = helper.Allocate(lines);
					if (newEntryLines.Count > 0)
					{
						entryLines.Add(warehouse.Key, newEntryLines);
					}
				}
				factory.Save();

				transactionManager.CommitTransaction();
			}

			if (entryLines.Count == 0)
			{
				log.Notify(OperationalActionLogErrorLevel.Warning, "No Bond store records affected");
				return null;
			}

			return entryLines;
		}

		protected virtual IReadOnlyCollection<CusWHSOperatorTransactionNettOffProcedure.TransactionLine> CreateOrSelectLinesForSelections(IGrouping<Guid, OperatorTransactionSelection> warehouse)
		{
			var refs = warehouse.AsEnumerable().Select(x => (string)x.OwnerReference).ToList();
			if (string.IsNullOrEmpty(ExportType) && refs.Count == 1 && string.IsNullOrEmpty(refs[0]))
			{
				refs.Clear();
			}
			var productOwner = warehouse.AsEnumerable().First().ProductOwner;
			return CusWHSOperatorTransactionNettOffProcedure.Execute(factory, GlbCompany.CurrentCompany.PK.ToGuid(),
				warehouse.Key, productOwner.ToGuid(), ExportType, refs, SortAscending, GlbStaff.CurrentUser.GS_Code);
		}

		protected abstract string ExportType { get; }
		protected abstract bool SortAscending { get; }
		protected virtual bool CheckSufficientQuantitiesForAllocation => false;
		protected virtual bool AskForConfirmationToContinueWithInsufficientStock()
		{
			throw new NotImplementedException("This must be overridden if CheckSufficientQuantitiesForAllocation is true");
		}

		protected virtual bool HaveSufficientQuantitiesForAllocation(IEnumerable<OperatorTransactionSelection> selections)
		{
			var firstSelection = selections.FirstOrDefault();
			var ownerReferences = selections.Select(x => x.OwnerReference).ToArray();
			var ordersQuery = new ZDBOnlyQuery(typeof(CusWHSOperatorTransaction));
			ordersQuery.AddToFilter(CusWHSOperatorTransactionSchema.WOT_OwnerReference, ownerReferences);
			ordersQuery.AddToFilter(CusWHSOperatorTransactionSchema.WOT_TransactionType, WarehouseOperatorTransactionTypeList.Codes.ORD);
			ordersQuery.AddToFilter(CusWHSOperatorTransactionSchema.WOT_OH_ProductOwner, firstSelection.ProductOwner);
			var warehouseSubQuery = new ZDBOnlySubQuery(typeof(CusWHSOperatorTransactionBatch), CusWHSOperatorTransactionSchema.WOT_WOB_CusWHSTransactionBatch);
			warehouseSubQuery.AddToFilter(CusWHSOperatorTransactionBatchSchema.WOB_OA_Warehouse, firstSelection.WarehouseAddress);
			ordersQuery.AddSubQuery(warehouseSubQuery, JoinCondition.And);
			var orders = factory.Load<CusWHSOperatorTransaction>(ordersQuery);
			var requiredProducts = CalculateOutstandingQuantities(orders);

			var productCodes = requiredProducts.Keys;
			var receiptsQuery = new ZDBOnlyQuery(typeof(CusWHSOperatorTransaction));
			receiptsQuery.AddToFilter(CusWHSOperatorTransactionSchema.WOT_TransactionType, WarehouseOperatorTransactionTypeList.Codes.REC);
			receiptsQuery.AddToFilter(CusWHSOperatorTransactionSchema.WOT_Status, SQLComparisonOperator.NotEqual, WarehouseOperatorTransactionStatusList.Codes.CLS);
			receiptsQuery.AddToFilter(CusWHSOperatorTransactionSchema.WOT_OH_ProductOwner, firstSelection.ProductOwner);
			receiptsQuery.AddSubQuery(warehouseSubQuery, JoinCondition.And);
			var subQuery = new ZDBOnlySubQuery(typeof(Customs.Business.OrgSupplierPart), CusWHSOperatorTransactionSchema.WOT_OP_Product);
			subQuery.AddToFilter(OrgSupplierPartSchema.OP_PartNum, productCodes);
			receiptsQuery.AddSubQuery(subQuery, JoinCondition.And);
			var receipts = factory.Load<CusWHSOperatorTransaction>(receiptsQuery);
			var availableProducts = CalculateOutstandingQuantities(receipts);

			var result = true;
			foreach (var product in requiredProducts)
			{
				var productCode = product.Key;
				var requiredQuantity = product.Value;
				if (!availableProducts.TryGetValue(productCode, out var availableQuantity) || requiredQuantity > availableQuantity)
				{
					log.Notify(OperationalActionLogErrorLevel.Warning, $"Product code: {productCode} Qty Requested of {requiredQuantity} is greater than Qty available of {availableQuantity}");
					result = false;
				}
			}
			return result;
		}

		IDictionary<ZString, decimal> CalculateOutstandingQuantities(IEnumerable<CusWHSOperatorTransaction> transactions)
		{
			return transactions.GroupBy(tx => tx.Product.OP_PartNum, tx => tx.WOT_Quantity - tx.TransactionLines.Select(line => (decimal)line.WOL_Quantity).Sum())
				.ToDictionary(product => product.Key, product => product.Sum());
		}

		protected void LogResultForSingleMessage(string[] messageNumber)
		{
			if (messageNumber?.Length == 0)
			{
				log.Notify(OperationalActionLogErrorLevel.Error, "Failed to create the EDI message");
			}
			else if (messageNumber.Length == 1)
			{
				log.Notify(OperationalActionLogErrorLevel.Warning, $"Created message {messageNumber[0]}");
			}
			else
			{
				log.Notify(OperationalActionLogErrorLevel.Warning, $"Created messages {string.Join(", ", messageNumber)}");
			}
		}

		protected readonly IOperationalActionSectionLog log;
		protected readonly BusinessObjectFactory factory;
	}
}
