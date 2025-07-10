using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business.OperationalActions
{
	public class ClearExpiredStockApplicator : BaseExportApplicator
	{
		public ClearExpiredStockApplicator(BusinessObjectFactory factory, Func<ZDate> getExpiryCutoffDateFunc) : base("Clear Expired Stock", factory, ZString.Empty)
		{
			this.getExpiryCutoffDateFunc = getExpiryCutoffDateFunc;
		}

		internal new static string QuerySql => @"SELECT WOT_CustomsEntryNumber, WOT_OwnerReference, WOT_IntoBondDate, WOB_OA_Warehouse, WOB_PK
FROM dbo.CusWHSOperatorTransaction LEFT JOIN dbo.CusWHSOperatorTransactionBatch ON WOB_PK = WOT_WOB_CusWHSTransactionBatch
WHERE WOB_OA_Warehouse = @Warehouse AND WOT_OH_ProductOwner = @ProductOwner
      AND WOT_TransactionType = 'REC' AND WOT_Status <> 'CLS' AND WOT_IsCustomsControlled = 1 AND WOT_IntoBondDate < @ExpiryCutoffDate
      AND WOB_GC_Company = @Company
GROUP BY WOT_CustomsEntryNumber, WOT_OwnerReference, WOT_IntoBondDate, WOB_OA_Warehouse, WOB_PK";

		protected override void BuildCore(ZGuid[] selectItemPKs)
		{
			Mutex.Lock();
			if (HasLock)
			{
				var transaction = LoadCusWHSOperatorTransaction(selectItemPKs);
				if (transaction != null)
				{
					var date = getExpiryCutoffDateFunc();
					if (date.IsValid)
					{
						var thresholdDate = ZDate.Today.AddMonths(-22);
						if (date <= thresholdDate)
						{
							var sqlParameters = new ZSqlParameter[]
							{
							ZSqlParameter.New("@ExpiryCutoffDate", date, CusWHSOperatorTransactionSchema.WOT_IntoBondDate),
							ZSqlParameter.New("@Company", GlbCompany.CurrentCompany.PK, CusWHSOperatorTransactionBatchSchema.PK),
							ZSqlParameter.New("@Warehouse", transaction.Batch.WOB_OA_Warehouse, CusWHSOperatorTransactionBatchSchema.WOB_OA_Warehouse),
							ZSqlParameter.New("@ProductOwner", transaction.WOT_OH_ProductOwner, CusWHSOperatorTransactionSchema.WOT_OH_ProductOwner),
							};
							var collection = new DynamicBusinessObjectCollection(Factory);
							collection.Load(QuerySql, sqlParameters);

							foreach (DynamicBusinessObject result in collection)
							{
								var r = Records.AddNew();
								r.CustomsEntryNumber = (ZString)result[CusWHSOperatorTransaction.Schema.WOT_CustomsEntryNumber];
								r.OwnerReference = (ZString)result[CusWHSOperatorTransaction.Schema.WOT_OwnerReference];
								r.IntoBondDate = (ZDateTime)result[CusWHSOperatorTransaction.Schema.WOT_IntoBondDate];
								r.WarehouseAddress = (ZGuid)result[CusWHSOperatorTransactionBatch.Schema.WOB_OA_Warehouse];
								r.ProductOwner = transaction.WOT_OH_ProductOwner;
								r.Batch = (ZGuid)result[CusWHSOperatorTransactionBatch.Schema.PK];
								r.Select = true;
							}
						}
					}
				}
			}
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			var runner = new ClearExpiredStockOperationalActionRunner(log, Factory);
			Lock();
			if (HasLock)
			{
				try
				{
					runner.Run(Records.Cast<OperatorTransactionSelection>());
				}
				finally
				{
					Unlock();
				}
			}
		}

		readonly Func<ZDate> getExpiryCutoffDateFunc;
	}
}
