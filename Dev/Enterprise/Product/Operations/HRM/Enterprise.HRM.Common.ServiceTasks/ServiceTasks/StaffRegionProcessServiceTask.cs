using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
#if NET48
using System.Web;
#else
using System.Net.Http;
#endif
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.Definitions.LeaveEngine;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.HRM.Common.ServiceTasks.StaffRegionProcessServiceTask.Code,
	"Process Staff Leave By Region Service Task",
	"HRM",
	typeof(Enterprise.HRM.Common.ServiceTasks.StaffRegionProcessServiceTask),
	MinimumPeriod = "1minute",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1day"
	)]
namespace Enterprise.HRM.Common.ServiceTasks
{
	#region SuppressResourceStringsCheckRegion

	public class StaffRegionProcessServiceTask : ServiceProviderImpl
	{
		public const string Code = "SRP";
		const int MaxBatchSize = 100;
		const int MaxProcessingRunRows = 10;

		const int MaxSqlLockLostExceptionFailureCount = 2;
		const int MaxConsecutiveBatchFailureCount = 2;
		const int MaxConsecutiveStaffFailureCount = 3;
		const int MaxProcessStaffFailureCount = 3;

		readonly ILeaveForecaster leaveForecaster;

		public StaffRegionProcessServiceTask()
			: this(new LeaveForecaster()) { }

		public StaffRegionProcessServiceTask(ILeaveForecaster leaveForecaster)
		{
			this.leaveForecaster = leaveForecaster ?? throw new ArgumentNullException(nameof(leaveForecaster));
		}

		public virtual int BatchSize => MaxBatchSize;

		public ZQuery OldestProcessingRunQuery => new ZQuery(HrlProcessingRunSchema.LLR_Status,
			new[]
			{
				LeaveProcessingRunStatusCodes.Queued,
				LeaveProcessingRunStatusCodes.InProgress
			})
		{
			OrderBy = $"{nameof(HrlProcessingRunSchema.LLR_SystemCreateTimeUtc)} ASC",
			IgnoreDbQueryCache = true,
			MaximumRows = MaxProcessingRunRows,
		};

		IDisposable GetProcessingRun(out HrlProcessingRun run)
		{
			var factory = new BusinessObjectFactory();
			var processingRuns = factory.Load<HrlProcessingRun>(OldestProcessingRunQuery);

			SqlApplicationLock @lock = null;
			run = processingRuns.FirstOrDefault(r => TryGetTaskLock(Db.Connection, r, out @lock));
			return @lock;
		}

		public override void RunTask(CancellationToken cancellationToken)
		{
			for (var tries = 0; tries <= MaxSqlLockLostExceptionFailureCount; tries++)
			{
				try
				{
					using (GetProcessingRun(out var run))
					{
						if (run == null)
						{
							ServiceLogger.Information("No processable processing run found.");
							return;
						}

						ServiceLogger.Information($"Start Processing Run {run.PK}.");
						RunTaskCore(run, cancellationToken);
						ServiceLogger.Information($"Processing Run {run.PK} is {run.LLR_Status}.");

						return;
					}
				}
				catch (SqlLockLostException ex)
				{
					ServiceLogger.Warning($"SqlLockLostException: {ex.Message}");
				}
			}
		}

		public static bool TryGetTaskLock(DbConnection connection, HrlProcessingRun processingRun, out SqlApplicationLock appLock)
		{
			var key = "StaffRegionProcessServiceTask:" + processingRun.PK.ToString().ToUpperInvariant();
			return connection.TryGetLock(key, out appLock);
		}

		void RunTaskCore(HrlProcessingRun processingRun, CancellationToken cancellationToken)
		{
			if (processingRun.LLR_Status != LeaveProcessingRunStatusCodes.InProgress)
			{
				processingRun.LLR_Status = LeaveProcessingRunStatusCodes.InProgress;
				processingRun.Factory.Save();
			}

			try
			{
				ProcessRun(processingRun, cancellationToken);

				processingRun.LLR_Status = processingRun.LLR_Status != LeaveProcessingRunStatusCodes.InProgressWithErrors
					? LeaveProcessingRunStatusCodes.Completed
					: LeaveProcessingRunStatusCodes.CompletedWithErrors;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ServiceLogger.Error("Error occurred: " + ex.Message);

				processingRun.LLR_Status = LeaveProcessingRunStatusCodes.CompletedWithErrors;
			}

			processingRun.Factory.Save();
		}

		void ProcessRun(HrlProcessingRun processingRun, CancellationToken cancellationToken)
		{
			var staffQueue = new DbOnlyBusinessObjectQueue<GlbStaff>(GetStaffQueueInProcessingRunQuery(processingRun));

			var totalBatchesCount = 0;
			var consecutiveBatchesFailureCount = 0;

			staffQueue.ProcessBatch((staffs, e) =>
			{
				totalBatchesCount++;
				if (ProcessStaffQueueBatch(staffs, processingRun, e))
				{
					consecutiveBatchesFailureCount = 0;
				}
				else
				{
					consecutiveBatchesFailureCount++;
					if (consecutiveBatchesFailureCount >= MaxConsecutiveBatchFailureCount)
					{
						e.Cancel = true;

						ServiceLogger.Error($"{totalBatchesCount} batch(es) processed. {consecutiveBatchesFailureCount} consecutive batch(es) had failed. Processing Run failed.");
					}
				}
			}, BatchSize, cancellationToken);
		}

		public static ZQuery GetStaffQueueInProcessingRunQuery(HrlProcessingRun processingRun)
		{
			var query = new ZDBOnlyQuery(typeof(GlbStaff));

			var sql = $@"GS_PK IN (
SELECT DISTINCT S.GS_PK
FROM dbo.HrlProcessingRun R
INNER JOIN dbo.HrlPolicy P ON R.LLR_RN_NKCountry = P.LLP_RN_NKCountry
INNER JOIN dbo.HrlStaffPolicy SP ON SP.LLS_LLP_Policy = P.LLP_PK
INNER JOIN dbo.GlbStaff S ON S.GS_PK = SP.LLS_GS_Staff
LEFT JOIN dbo.HrlBalanceTransaction T ON T.LLT_GS_Staff = S.GS_PK AND T.LLT_LLR_ProcessingRun = R.LLR_PK
OUTER APPLY (
	SELECT TOP 1 AccrualDate, DateRun FROM
	(
		SELECT BT.LLT_Accrual AS AccrualDate, BT.LLT_SystemCreateTimeUTC as DateRun
		FROM dbo.HrlBalanceTransaction BT INNER JOIN dbo.HrlProcessingRun PT
		ON BT.LLT_LLR_ProcessingRun = PT.LLR_PK
		WHERE PT.LLR_RN_NKCountry = R.LLR_RN_NKCountry AND LLT_Accrual IS NOT NULL AND BT.LLT_GS_Staff = S.GS_PK
		UNION
		SELECT GS_EmploymentDate AS AccrualDate, NULL AS DateRun
		FROM dbo.GlbStaff
		WHERE GS_PK = S.GS_PK
	) AccrualDates
	ORDER BY AccrualDate DESC
) LastAccrual
WHERE R.LLR_PK = @ProcessingRunPK AND T.LLT_PK IS NULL
	AND SP.LLS_EffectiveDate <= R.LLR_ProcessTo AND (SP.LLS_AutoEffectiveEndDate IS NULL OR SP.LLS_AutoEffectiveEndDate >= LastAccrual.AccrualDate)
	AND (CAST(LastAccrual.AccrualDate AS DATE) <= R.LLR_ProcessTo OR EXISTS(SELECT NULL FROM dbo.HrlBalanceAffectingLog WHERE LLB_GS_Staff = S.GS_PK AND LLB_SystemCreateTimeUtc >= LastAccrual.DateRun))
)";
			var parameters = new ZSqlParameterCollection();
			parameters.Add("@ProcessingRunPK", processingRun.PK, HrlProcessingRunSchema.PK);

			query.AddFilterAndZSQLParameterCollection(sql, parameters);

			return query;
		}

		bool ProcessStaffQueueBatch(GlbStaff[] staffs, HrlProcessingRun processingRun, CancelEventArgs e)
		{
			ServiceLogger.Information($"Batched [{staffs.Length}] staff to process");

			if (staffs.Length == 0)
			{
				return true;
			}

			if (e.Cancel)
			{
				ServiceLogger.Information($"Processing staff cancelled.");

				return true;
			}

			return ProcessStaffQueueBatchCore(staffs, processingRun);
		}

		bool ProcessStaffQueueBatchCore(GlbStaff[] staffs, HrlProcessingRun processingRun)
		{
			var success = 0;
			var consecutiveStaffFailureCount = 0;

			foreach (var staff in staffs)
			{
				if (TryProcessStaff(staff, processingRun))
				{
					success++;
					consecutiveStaffFailureCount = 0;
				}
				else
				{
					consecutiveStaffFailureCount++;
					if (consecutiveStaffFailureCount >= MaxConsecutiveStaffFailureCount || consecutiveStaffFailureCount == staffs.Length)
					{
						ServiceLogger.Error($"[{success}] staff processed successfully, [{consecutiveStaffFailureCount}] consecutive staff had errors. Batch processing failed.");
						return false;
					}
				}
			}
			LogSuccessfulBatch(staffs.Length, success);
			return true;
		}

		void LogSuccessfulBatch(int batchSize, int successCount)
		{
			ServiceLogger.Information($"Batched [{successCount}] staff were successfully processed.");

			if (batchSize > successCount)
			{
				ServiceLogger.Error($"[{batchSize - successCount}] staff had errors.");
			}
		}

		bool TryProcessStaff(GlbStaff staff, HrlProcessingRun processingRun)
		{
			var retryCount = 0;
			while (!ProcessStaff(staff, processingRun, out var errMsg))
			{
				retryCount++;
				if (retryCount >= MaxProcessStaffFailureCount)
				{
					processingRun.LLR_Status = LeaveProcessingRunStatusCodes.InProgressWithErrors;
					MarkRowAsBad(staff, processingRun, errMsg);
					ServiceLogger.Error($"Processing {staff.GS_Code} Failed");
					return false;
				}
			}

			return true;
		}

		bool ProcessStaff(GlbStaff staff, HrlProcessingRun processingRun, out string errMsg)
		{
			try
			{
				ProcessStaffCore(staff, processingRun);
			}
#if NET48
catch (HttpException ex)
{
			errMsg = ex.Message;
			ServiceLogger.Error($"Processing {staff.GS_Code} HTTP Error: {ex.Message}");
			return false;
}
#else
			catch (HttpIOException ex)
			{
				errMsg = ex.Message;
				ServiceLogger.Error($"Processing {staff.GS_Code} HTTP Error: {ex.Message}");
				return false;
			}
#endif
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				errMsg = ex.Message;
				ServiceLogger.Error($"Processing {staff.GS_Code} Error: {ex.Message}");
				return false;
			}

			errMsg = string.Empty;

			return true;
		}

		protected virtual BusinessObjectFactory GetStaffQueueFactory() => new BusinessObjectFactory();

		readonly string[] FinanceTransactionTypes = BalanceTransactionTypes.GetFinanceTypes();
		readonly string[] ProcessingTransactionTypes = BalanceTransactionTypes.GetProcessingTypes();

		void ProcessStaffCore(GlbStaff staff, HrlProcessingRun processingRun)
		{
			var factory = GetStaffQueueFactory();
			var transactions = factory.Load<HrlBalanceTransaction>(new ZQuery(HrlBalanceTransactionSchema.LLT_GS_Staff, staff.PK));

			var employmentDate = ToStaffTimezone(factory, staff, staff.GS_EmploymentDate.ToDateTime());
			var processTo = processingRun.LLR_ProcessTo.ToDateTime();

			var lastAccrual = transactions.MaxOrDefault(t => t.LLT_Accrual);

			var queuedChanges = factory.Load<HrlBalanceAffectingQueue>(new ZQuery(HrlBalanceAffectingQueueSchema.LLQ_GS_Staff, staff.PK));

			if (queuedChanges.Length == 0 && (lastAccrual.IsValid && lastAccrual.Date >= processingRun.LLR_ProcessTo || employmentDate >= processTo))
			{
				ServiceLogger.Information("Skip " + staff.GS_Code);
				return;
			}
			var processingRunAccrual = ToStaffTimezone(factory, staff, new DateTime(processTo.Year, processTo.Month, processTo.Day, 23, 59, 59));
			ServiceLogger.Information("Processing " + staff.GS_Code);

			GenerateBalanceTransactions(factory, processingRun, processingRunAccrual, staff, transactions, queuedChanges, isFinance: false);
			GenerateBalanceTransactions(factory, processingRun, processingRunAccrual, staff, transactions, queuedChanges, isFinance: true);

			foreach (var item in queuedChanges)
			{
				item.Delete();
			}

			factory.Save();
			ServiceLogger.Debug($"Processing {staff.GS_Code} succeed to {processingRunAccrual}");
		}

		void GenerateBalanceTransactions(BusinessObjectFactory factory, HrlProcessingRun processingRun, ZDateTimeOffset processingRunAccrual, GlbStaff staff, HrlBalanceTransaction[] transactions, HrlBalanceAffectingQueue[] queuedChanges, bool isFinance)
		{
			var processedAccruals = transactions.Select(t => t.LLT_Accrual).Distinct().OrderByDescending(d => d).ToList();
			var lastAccrual = processedAccruals.FirstOrDefault();
			var processTo = processingRun.LLR_ProcessTo.ToDateTime();
			var forecastResult = leaveForecaster.Forecast(staff.GS_Code, processTo, lastAccrual.IsValid ? lastAccrual.Date.ToDateTime() : null, isFinance, onlyIncludeRequestedSnapshots: true);

			var relevantTypes = isFinance ? FinanceTransactionTypes : ProcessingTransactionTypes;
			var relevantTransactions = transactions
			  .Where(t => relevantTypes.Contains(t.LLT_TransactionType.ToString()));

			var buckets = GetBucketFromTransactions(relevantTransactions.ToArray());

			var forecastBalanceTransactions = forecastResult.BalanceTransactions.Where(t => t.EffectiveDate == processTo).ToList();
			if (queuedChanges.Length > 0)
			{
				buckets = ReprocessPriorTransactions(staff, processingRun, queuedChanges, lastAccrual, processedAccruals, buckets.ToArray(), forecastBalanceTransactions, factory, isFinance);
			}

			if (!lastAccrual.IsValid || processTo > lastAccrual)
			{
				foreach (var result in LocateTransactions(buckets, GetBucketFromBalance(forecastBalanceTransactions)))
				{
					var transaction = factory.New<HrlBalanceTransaction>();
					transaction.LLT_TransactionType = isFinance ? BalanceTransactionTypes.FinanceProcessing : BalanceTransactionTypes.Processing;
					transaction.LLT_LeaveType = result.Type;
					transaction.LLT_Forfeiture = result.Forfeiture == null
						? ZDateTimeOffset.Empty
						: ToStaffTimezone(factory, staff, result.Forfeiture.Value);
					transaction.LLT_DeltaValueHours = result.Value;

					transaction.LLT_Accrual = processingRunAccrual;
					transaction.LLT_GS_Staff = staff.PK;
					transaction.LLT_Comment = "Processing";
					transaction.LLT_LLR_ProcessingRun = processingRun.PK;
				}
			}

			if (!isFinance)
			{
				AddLeaveProcessedLogs(staff, processingRun, factory, forecastResult.LeaveProcessed);
			}
		}

		void AddLeaveProcessedLogs(GlbStaff staff, HrlProcessingRun processingRun, BusinessObjectFactory factory, IEnumerable<LeaveProcessed> leaveProcesseds)
		{
			foreach (var leaveProcessed in leaveProcesseds)
			{
				var staffHolidayPK = new ZGuid(leaveProcessed.FreeText);
				var leaveProcessedProcessedFrom = ToStaffTimezone(factory, staff, leaveProcessed.ProcessedFrom);
				var leaveProcessedProcessedTo = ToStaffTimezone(factory, staff, leaveProcessed.ProcessedTo);

				var processedLeaveLogsFilter = new ZQuery(HrlLeaveProcessedLogSchema.LLD_GA_StaffHoliday, staffHolidayPK);
				processedLeaveLogsFilter.AddToFilter(JoinCondition.And, HrlLeaveProcessedLogSchema.LLD_SupersededOn, SQLComparisonOperator.Equal, DBNull.Value);
				processedLeaveLogsFilter.AddToFilter(JoinCondition.And, HrlLeaveProcessedLogSchema.LLD_ProcessedFrom, SQLComparisonOperator.LessThanOrEqualTo, leaveProcessedProcessedTo);
				processedLeaveLogsFilter.AddToFilter(JoinCondition.And, HrlLeaveProcessedLogSchema.LLD_ProcessedTo, SQLComparisonOperator.GreaterThanOrEqualTo, leaveProcessedProcessedFrom);
				var isValidProcessedLogExists = factory.Exists(typeof(HrlLeaveProcessedLog), processedLeaveLogsFilter);

				if (!isValidProcessedLogExists)
				{
					var transaction = factory.New<HrlLeaveProcessedLog>();
					transaction.LLD_ProcessedThisPeriodHours = leaveProcessed.HoursProcessedInThisPeriod;
					transaction.LLD_ProcessedFrom = ToStaffTimezone(factory, staff, leaveProcessed.ProcessedFrom);
					transaction.LLD_ProcessedTo = leaveProcessedProcessedTo;
					transaction.LLD_GA_StaffHoliday = staffHolidayPK;
					transaction.LLD_LLR_ProcessingRun = processingRun.PK;
				}
			}
		}

		Bucket[] ReprocessPriorTransactions(GlbStaff staff, HrlProcessingRun run, HrlBalanceAffectingQueue[] queue, ZDateTimeOffset lastAccrual, List<ZDateTimeOffset> processedAccruals, Bucket[] existing, IEnumerable<BalanceTransaction> newBalance, BusinessObjectFactory factory, bool isFinance)
		{
			var bucket = existing;

			if (lastAccrual.IsValid)
			{
				var balanceAffecting = queue.Where(q => q.LLQ_EffectiveDate <= lastAccrual).ToArray();
				if (balanceAffecting.Length > 0)
				{
					if (!isFinance)
					{
						ServiceLogger.Debug($"Reprocessing {staff.GS_Code} to {lastAccrual}.");
					}

					var earliestBAQEffectiveDate = queue.Min(b => b.LLQ_EffectiveDate);
					SupersedeLeaveProcessLogs(staff, factory, earliestBAQEffectiveDate);

					var updated = leaveForecaster.Forecast(staff.GS_Code, lastAccrual.ToDateTime(), null, isFinance, onlyIncludeRequestedSnapshots: true).BalanceTransactions;
					bucket = GetBucketFromBalance(updated).ToArray();

					foreach (var result in LocateTransactions(existing, bucket))
					{
						var transaction = factory.New<HrlBalanceTransaction>();
						transaction.LLT_TransactionType = isFinance ? BalanceTransactionTypes.FinanceReprocessing : BalanceTransactionTypes.Reprocessing;
						transaction.LLT_LeaveType = result.Type;
						transaction.LLT_Forfeiture = result.Forfeiture == null
							? ZDateTimeOffset.Empty
							: ToStaffTimezone(factory, staff, result.Forfeiture.Value);
						transaction.LLT_DeltaValueHours = result.Value;
						transaction.LLT_Accrual = lastAccrual;
						transaction.LLT_GS_Staff = staff.PK;
						transaction.LLT_Comment = "Reprocessed due to changes: " + string.Join(", ", balanceAffecting.Select(q => q.LLQ_Reference));
						transaction.LLT_LLR_ProcessingRun = run.PK;
					}
				}
			}

			return bucket;
		}

		void SupersedeLeaveProcessLogs(GlbStaff staff, BusinessObjectFactory factory, ZDateTimeOffset earliestBAQEffectiveDate)
		{
			var processedLeaveLogsFilter = new ZDBOnlyQuery(typeof(HrlLeaveProcessedLog));
			processedLeaveLogsFilter.AddToFilter(JoinCondition.And, HrlLeaveProcessedLogSchema.LLD_SupersededOn, SQLComparisonOperator.Equal, DBNull.Value);

			var staffHolidaySubQuery = new ZDBOnlySubQuery(typeof(GlbStaffHoliday), GlbStaffHolidaySchema.PK);
			staffHolidaySubQuery.AddToFilter(JoinCondition.And, GlbStaffHolidaySchema.GA_GS, SQLComparisonOperator.Equal, staff.PK);
			staffHolidaySubQuery.AddToFilter(JoinCondition.And, GlbStaffHolidaySchema.GA_EndTime, SQLComparisonOperator.GreaterThanOrEqualTo, earliestBAQEffectiveDate.ToDateTime());

			processedLeaveLogsFilter.AddSubQuery(HrlLeaveProcessedLogSchema.LLD_GA_StaffHoliday, staffHolidaySubQuery, JoinCondition.And);

			var supersededLeaveProcessedLogs = factory.Load<HrlLeaveProcessedLog>(processedLeaveLogsFilter);
			if (supersededLeaveProcessedLogs.Length > 0)
			{
				ServiceLogger.Debug("Supersede Leave Process Logs.");

				foreach (var log in supersededLeaveProcessedLogs)
				{
					log.LLD_SupersededOn = ToStaffTimezone(factory, staff, ZDateTime.Now.ToDateTime());
				}
			}
		}

		IEnumerable<Bucket> LocateTransactions(IEnumerable<Bucket> original, IEnumerable<Bucket> proposed)
		{
			var sourceLookup = original.ToDictionary(b => (b.Type, b.Forfeiture));
			foreach (var bucket in proposed)
			{
				var value = bucket.Value;
				if (sourceLookup.TryGetValue((bucket.Type, bucket.Forfeiture), out var existing))
				{
					value -= existing.Value;
				}

				yield return new Bucket(bucket.Type, bucket.Forfeiture, value);
			}
		}

		public void MarkRowAsBad(GlbStaff staff, HrlProcessingRun processingRun, string errMsg)
		{
			var factory = new BusinessObjectFactory();
			var accrual = ToStaffTimezone(factory, staff, new DateTime(processingRun.LLR_ProcessTo.Year, processingRun.LLR_ProcessTo.Month, processingRun.LLR_ProcessTo.Day, 23, 59, 59));

			var transaction = factory.New<HrlBalanceTransaction>();
			transaction.LLT_Accrual = accrual;
			transaction.LLT_GS_Staff = staff.PK;
			transaction.LLT_DeltaValueHours = 0;
			transaction.LLT_Comment = "Error: Row could not be processed, it will be processed in the next run";
			transaction.LLT_LLR_ProcessingRun = processingRun.PK;
			transaction.LLT_LeaveType = BalanceTransactionTypes.Error;
			transaction.LLT_TransactionType = BalanceTransactionTypes.Error;

			var log = factory.New<HrlBalanceAffectingLog>();
			log.LLB_EffectiveDate = accrual;
			log.LLB_GS_Staff = staff.PK;
			log.LLB_Reference = errMsg.Length <= HrlBalanceAffectingLogSchema.LLB_Reference.MaxLength
				? errMsg
				: errMsg.Substring(0, HrlBalanceAffectingLogSchema.LLB_Reference.MaxLength);

			factory.Save();
		}

		ZDateTimeOffset ToStaffTimezone(BusinessObjectFactory factory, GlbStaff staff, DateTime datetime)
		{
			var offset = StaffTimezoneOffsetProvider.GetOffset(factory, staff.PK, datetime);

			return new ZDateTimeOffset(datetime.Ticks, offset);
		}

		static IEnumerable<Bucket> GetBucketFromTransactions(HrlBalanceTransaction[] balances)
			=> balances
				.GroupBy(t => (t.LLT_LeaveType, t.LLT_Forfeiture.ToDateTimeOffsetSafe()?.DateTime))
				.Select(g => new Bucket(g.Key.LLT_LeaveType, g.Key.DateTime, g.Sum(t => t.LLT_DeltaValueHours)));

		static IEnumerable<Bucket> GetBucketFromBalance(IEnumerable<BalanceTransaction> balances)
		{
			foreach (var balance in balances)
			{
				foreach (var forfeiture in balance.ForfeitureBuckets)
				{
					yield return new Bucket(balance.LeaveType, forfeiture.Expiry, forfeiture.Amount);
				}

				yield return new Bucket(balance.LeaveType, null, balance.NonForfeitureHours);
			}
		}
	}

	#endregion
}
