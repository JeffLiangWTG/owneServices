using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	public abstract class BaseExportApplicator : OperationalActionMethodApplicator, IOverrideSelectionCount, ILockedOperationalActionMethodApplicator
	{
		public OperatorTransactionSelectionCollection Records { get; } = new OperatorTransactionSelectionCollection();

		public bool SelectAll
		{
			get => Records.Count > 0 && Records.All(x => ((OperatorTransactionSelection)x).Select);
			set
			{
				foreach (OperatorTransactionSelection rec in Records)
				{
					rec.Select = value;
				}
			}
		}

		public int SelectionCount => Records.Count(record => ((OperatorTransactionSelection)record).Select);

		public bool SupportRunningOnAllMatchedRecords => false;

		public ZString RequiredTransactionType { get; set; } = "ORD";

		protected BaseExportApplicator(string name, BusinessObjectFactory factory, ZString requiredExportType)
			: base(name, factory)
		{
			this.requiredExportType = requiredExportType;
		}

		internal static string QuerySql => @"SELECT WOT_TransactionType, WOT_ExportType, WOT_TransactionDate, WOT_OwnerReference, WOB_PK
FROM dbo.CusWHSOperatorTransaction LEFT JOIN dbo.CusWHSOperatorTransactionBatch ON WOB_PK = WOT_WOB_CusWHSTransactionBatch
WHERE WOB_OA_Warehouse = @Warehouse AND WOT_OH_ProductOwner = @ProductOwner
      AND WOT_TransactionType = @RequiredTransactionType AND WOT_ExportType LIKE @RequiredExportType AND WOT_Status = 'VAL'
GROUP BY WOT_OwnerReference, WOT_TransactionDate, WOT_TransactionType, WOT_ExportType, WOB_PK
ORDER BY WOT_TransactionDate";

		protected CusWHSOperatorTransaction LoadCusWHSOperatorTransaction(ZGuid[] selectItemPKs)
		{
			CusWHSOperatorTransaction transaction = null;
			if (selectItemPKs.Length > 0)
			{
				transaction = Factory.Load<CusWHSOperatorTransaction>(selectItemPKs[0]);
			}
			return transaction;
		}

		protected override void BuildCore(ZGuid[] selectItemPKs)
		{
			Mutex.Lock();
			if (HasLock)
			{
				base.BuildCore(selectItemPKs);

				var transaction = LoadCusWHSOperatorTransaction(selectItemPKs);
				if (transaction != null)
				{
					var sqlParameters = new ZSqlParameter[]
					{
						ZSqlParameter.New("@Warehouse", transaction.Batch.WOB_OA_Warehouse, CusWHSOperatorTransactionBatchSchema.WOB_OA_Warehouse),
						ZSqlParameter.New("@ProductOwner", transaction.WOT_OH_ProductOwner, CusWHSOperatorTransactionSchema.WOT_OH_ProductOwner),
						ZSqlParameter.New("@RequiredTransactionType", RequiredTransactionType, CusWHSOperatorTransactionSchema.WOT_TransactionType),
						ZSqlParameter.New("@RequiredExportType", requiredExportType, CusWHSOperatorTransactionSchema.WOT_ExportType),
					};
					var collection = new DynamicBusinessObjectCollection(Factory);
					collection.Load(QuerySql, sqlParameters);

					foreach (DynamicBusinessObject result in collection)
					{
						var r = Records.AddNew();
						r.TransactionType = (ZString)result[CusWHSOperatorTransaction.Schema.WOT_TransactionType];
						r.TransactionDate = (ZDateTime)result[CusWHSOperatorTransaction.Schema.WOT_TransactionDate];
						r.ExportType = (ZString)result[CusWHSOperatorTransaction.Schema.WOT_ExportType];
						r.OwnerReference = (ZString)result[CusWHSOperatorTransaction.Schema.WOT_OwnerReference];
						r.WarehouseAddress = transaction.Batch.WOB_OA_Warehouse;
						r.ProductOwner = transaction.WOT_OH_ProductOwner;
						r.Batch = (ZGuid)result[CusWHSOperatorTransactionBatch.Schema.PK];
					}
				}
			}
		}

		readonly ZString requiredExportType;

		public ZString LockInfoLabel
			=> Res.GetString(
				"D1E175DC-CD54-4DFD-8F5C-87BCC3AFE5ED",
				"Sorry, the process is currently being executed for '{0}', by user '{1}' on computer '{2}' since '{3}'.\nPlease wait for them to finish, then re-start this action if needed.",
				GlbCompany.CurrentCompany.CompanyName, Info?.GetUserWithLock(), Info?.HostName, Info?.LockStartTime);
		LockInfo Info => Mutex?.GetLockInfo();
		public ZBool HasLock => Mutex?.HasLock ?? false;
		public ZGlobalMutex Mutex => mutex ?? (mutex = new ZGlobalMutex(MutexIDs.CustomsWarehouseAllocationForCompany, GlbCompany.CurrentCompany.PK.ToString()));
		ZGlobalMutex mutex;
		public bool Lock() => Mutex.Lock();
		public void Unlock()
		{
			if (mutex != null)
			{
				if (mutex.IsLocked && mutex.HasLock)
				{
					mutex.Unlock();
					((IDisposable)mutex).Dispose();
				}
				mutex = null;
			}
		}
	}
}
