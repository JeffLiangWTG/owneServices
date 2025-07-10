using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.Business.OperationalActions;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ZA.Business
{
	public class ExWarehouseApplicator : OperationalActionMethodApplicator, ILockedOperationalActionMethodApplicator
	{
		public ExWarehouseApplicator(BusinessObjectFactory factory, Func<bool> showConfirmationDialogMethod, Action<ZString> showProcessLockedInfo) : base("Ex-Warehouse (Home Consumption)", factory)
		{
			this.showConfirmationDialogMethod = showConfirmationDialogMethod;
			this.showProcessLockedInfo = showProcessLockedInfo;
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			Lock();
			if (HasLock)
			{
				if (!(targets is CusWHSOperatorTransaction[] transactions) || transactions.Length == 0)
				{
					log.Notify(OperationalActionLogErrorLevel.Error, "No records available, please check filters and fetch before trying to run this action.");
					Unlock();
				}
				else if (showConfirmationDialogMethod())
				{
					var selections = GetTransactionSelections(transactions);
					if (selections.Count == 0)
					{
						log.Notify(OperationalActionLogErrorLevel.Warning, "No Bond store records affected");
						Unlock();
					}
					else
					{
						var runner = new ExWarehouseOperationalActionRunner(log, Factory);
						Lock();
						if (HasLock)
						{
							runner.Run(selections.Select(x => x));
							Unlock();
						}
						else
						{
							showProcessLockedInfo(LockInfoLabel);
						}
					}
				}
				else
				{
					Unlock();
				}
			}
			else
			{
				showProcessLockedInfo(LockInfoLabel);
			}
		}

		protected OperatorTransactionSelectionCollection GetTransactionSelections(CusWHSOperatorTransaction[] targets)
		{
			var records = new OperatorTransactionSelectionCollection();
			if (targets.Length > 0)
			{
				var transaction = targets[0];
				var record = records.AddNew();
				record.WarehouseAddress = transaction.Batch.WOB_OA_Warehouse;
				record.ProductOwner = transaction.WOT_OH_ProductOwner;
			}
			return records;
		}

		protected override void BuildCore(ZGuid[] selectItemPKs)
		{
			Lock();
			base.BuildCore(selectItemPKs);
		}

		readonly Func<bool> showConfirmationDialogMethod;
		readonly Action<ZString> showProcessLockedInfo;

		ZString LockInfoLabel
			=> Res.GetString(
				"A7A3C34F-E982-4E93-886F-E6FFB9F271DB",
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
