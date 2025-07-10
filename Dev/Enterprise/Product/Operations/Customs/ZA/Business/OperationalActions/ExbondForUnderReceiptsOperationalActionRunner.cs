using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.Business.WarehouseIntegration;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business.OperationalActions
{
	class ExbondForUnderReceiptsOperationalActionRunner : ExportOperationalActionRunner
	{
		public ExbondForUnderReceiptsOperationalActionRunner(IOperationalActionSectionLog log, BusinessObjectFactory factory) : base(log, factory)
		{
		}

		public override void Run(IEnumerable<OperatorTransactionSelection> selections)
		{
			var linesPerWarehouse = ProcessAndGetEntryLines(selections);
			if (linesPerWarehouse != null)
			{
				foreach (var lines in linesPerWarehouse)
				{
					var messageNumber = ExbondShipmentUsxml.CreateEDIMessage(factory, warehousePK: lines.Key, lines.Value, ExbondShipmentUsxml.Mode.UnderReceipts);
					LogResultForSingleMessage(messageNumber);
				}
			}
		}

		protected override string ExportType => "%";
		protected override bool SortAscending => false;

		protected override IReadOnlyCollection<CusWHSOperatorTransactionNettOffProcedure.TransactionLine> CreateOrSelectLinesForSelections(IGrouping<Guid, OperatorTransactionSelection> warehouse)
		{
			return warehouse.AsEnumerable().SelectMany(x => GetLinesFromSelection(x)).ToList();
		}

		IEnumerable<CusWHSOperatorTransactionNettOffProcedure.TransactionLine> GetLinesFromSelection(OperatorTransactionSelection selection)
		{
			var query = new ZQuery(CusWHSOperatorTransactionSchema.WOT_Status, "VAL");
			query.AddToFilter(CusWHSOperatorTransactionSchema.WOT_TransactionType, selection.TransactionType);
			query.AddToFilter(CusWHSOperatorTransactionSchema.WOT_TransactionDate, selection.TransactionDate);
			query.AddToFilter(CusWHSOperatorTransactionSchema.WOT_OwnerReference, selection.OwnerReference);
			query.AddToFilter(CusWHSOperatorTransactionSchema.WOT_WOB_CusWHSTransactionBatch, selection.Batch);
			var records = factory.Load<CusWHSOperatorTransaction>(query);
			foreach (var record in records)
			{
				record.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.CLS;
			}
			return records.Select(x => new CusWHSOperatorTransactionNettOffProcedure.TransactionLine(x.PK.ToGuid(), x.PK.ToGuid(), x.PK.ToGuid(), x.WOT_Quantity, 0));
		}
	}
}
