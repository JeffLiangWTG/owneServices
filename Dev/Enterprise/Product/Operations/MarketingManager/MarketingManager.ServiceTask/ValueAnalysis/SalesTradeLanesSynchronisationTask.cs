using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Integration;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.ServiceTask;
using Enterprise.MarketingManager.ServiceTask.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	SalesTradeLanesSynchronisationTask.Code,
	SalesTradeLanesSynchronisationTask.Description,
	"SAL",
	typeof(SalesTradeLanesSynchronisationTask),
	MinimumPeriod = "1day",
	CanRunInAnyBranch = true,
	ConfigControlType = typeof(SalesTradeLanesSynchronisationControl),
	DefaultScheduleRunEvery = "1day",
	DefaultScheduleStartAtLocal = "2hours",
	ActiveByDefault = true
	)]

namespace Enterprise.MarketingManager.ServiceTask
{
	public class SalesTradeLanesSynchronisationTask : ServiceProviderImpl, IServiceTaskConfigurationUser
	{
		public const string Code = "TLS";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Task Description")]
		public const string Description = "Sales Trade Lanes Synchronization Task";

		public override void RunTask(CancellationToken token)
		{
			ServiceLogger.Log(LogType.Information, Description + " started.");
			Run(token);
			ServiceLogger.Log(LogType.Information, Description + " finished.");
		}

		protected virtual int BatchSize
		{
			get { return 100; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is logged")]
		void Run(CancellationToken token)
		{
			try
			{
				if (!ObjectFactory.Get<IJCDServiceTaskStatusChecker>().IsJCDServiceTaskComplete)
				{
					ServiceLogger.Log(LogType.Warning, "Before you can synchronize sales values, you must finish processing all Transaction Lines through the Job Costing Data Queue Service Task (https://myaccount-portal.cargowise.com/my-account/Documents/UpdateNotes/CargoWiseOneUpdateNote20180319d.pdf).");
					return;
				}

				var shouldDoFullTradeLanesSyncIfRequired = OrganisationRegistry.Instance.ShouldDoFullTradeLanesSyncIfRequired.Value;
				var now = ZDateTime.UtcNow;
				var currentPeriod = new ZDate(now.Year, now.Month, 1);
				var to = currentPeriod.AddMonths(1);
				var fullFromSyncDate = new ZDate(OrganisationRegistry.Instance.FullTradeLanesSyncFromDate);

				var partialFromSyncDate = currentPeriod.AddMonths(-Config.SyncMonthsOnMainSchedule);
				bool isYearly = false;
				if (Config.SyncYearly
					&& now.AddMonths(-Config.SyncYearlyEveryMonths).ToDateTime().Date >= SystemDataRegistry.Instance.LastYearlySyncTLS.Value.Date
					&&
						(
							now.Day >= Config.RunYearlyOnDayOfMonth
							|| (now.Day == DateTime.DaysInMonth(now.Year, now.Month) && Config.RunYearlyOnDayOfMonth == 31)
						)
					)
				{
					partialFromSyncDate = currentPeriod.AddYears(-1);
					isYearly = true;
				}

				var factoryProvider = new BusinessObjectFactoryProvider();
				factoryProvider.Current.RefreshEnabled = false;

				var initialSyncResult = Sync(factoryProvider, shouldDoFullTradeLanesSyncIfRequired, fullFromSyncDate, partialFromSyncDate, to, token);

#if DEBUG
				ThrowError_ForTesting();
#endif

				if (initialSyncResult.FullSkipped.Count > 0 || initialSyncResult.PartialSkipped.Count > 0)
				{
					ServiceLogger.Log(LogType.Debug, "Retrying synchronization for skipped organizations.");

					var secondSyncShouldDoFullTradeLanesSyncIfRequired = shouldDoFullTradeLanesSyncIfRequired && initialSyncResult.FullSkipped.Count > 0;
					var secondSyncResult = Sync(factoryProvider, secondSyncShouldDoFullTradeLanesSyncIfRequired, fullFromSyncDate, partialFromSyncDate, to, token, initialSyncResult);

					ServiceLogger.Log(LogType.Debug,
						string.Format(CultureInfo.InvariantCulture, @"Total synchronized organizations: {0}   Skipped organizations: {1}", initialSyncResult.TotalSuccuess + secondSyncResult.TotalSuccuess, secondSyncResult.GetAllSkipped().Length));
				}

				if (isYearly)
				{
					SystemDataRegistry.Instance.LastYearlySyncTLS.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, now.ToDateTime());
				}
			}
			catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.InvalidObjectName && ex.Message.Contains(TradeLinesSummaryProviderCommon.TradeLineCacheTableName))
			{
				ServiceLogger.Log(LogType.Warning, "The database connection was dropped. This run has been terminated.");
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ServiceLogger.Log(LogType.Error, "Exception occured", ex);
				ErrorReporter.ReportOnce(ex.Message, ex);
			}
		}

		SynchroniseResult Sync(BusinessObjectFactoryProvider factoryProvider, bool shouldDoFullTradeLanesSyncIfRequired, ZDate fullFromSyncDate, ZDate partialFromSyncDate, ZDate to, CancellationToken token, SynchroniseResult previousSyncResult = null)
		{
			var syncResult = new SynchroniseResult();
			var fromSyncDate = shouldDoFullTradeLanesSyncIfRequired ? fullFromSyncDate : partialFromSyncDate;
			var monthlySyncToDate = to;

			if (shouldDoFullTradeLanesSyncIfRequired)
			{
				var latestFullSyncMonth = OrganisationRegistry.Instance.FullTradeLanesSyncLatestSyncMonth.Value;
				if (latestFullSyncMonth > OrganisationRegistry.Instance.FullTradeLanesSyncFromDate)
				{
					monthlySyncToDate = new ZDate(latestFullSyncMonth);
				}
			}

			var monthlySyncFromDate = monthlySyncToDate.AddMonths(-1);

			while (fromSyncDate < monthlySyncToDate)
			{
				using (var tradeLinesSummaryProvider = new CachedTradeLinesSummaryProvider(Db.Connection, monthlySyncFromDate, monthlySyncToDate, ServiceLogger, true, true, previousSyncResult?.GetAllSkipped()))
				{
					ServiceLogger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture,
						"Building cache (From:{0} To:{1})",
						ToFullDateString(tradeLinesSummaryProvider.CacheFrom),
						ToFullDateString(tradeLinesSummaryProvider.CacheTo)));

					BuildCache(tradeLinesSummaryProvider);

					var orgQueue = (previousSyncResult != null)
						? new RetrySynchronisationOrgQueue(previousSyncResult.FullSkipped, previousSyncResult.PartialSkipped)
						: (ISynchronisationOrgQueue)new SalesTradeLanesSynchronisationOrgQueue(tradeLinesSummaryProvider);
					if (shouldDoFullTradeLanesSyncIfRequired)
					{
						// do full sync on orgs that have never been full synced before
						var fullSyncCount = SyncInBatches(tradeLinesSummaryProvider, orgQueue, SalesTradeLanesSynchronisationTaskSyncType.Full, monthlySyncFromDate, monthlySyncToDate, factoryProvider, token);
						if (fullSyncCount.Skipped.Count == 0)
						{
							OrganisationRegistry.Instance.FullTradeLanesSyncLatestSyncMonth.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, monthlySyncFromDate.ToDateTime());
							if (monthlySyncFromDate <= fromSyncDate)
							{
								// all orgs with data have been full synced. set flag to false
								OrganisationRegistry.Instance.ShouldDoFullTradeLanesSyncIfRequired.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
							}
						}
						syncResult.Add(fullSyncCount, SalesTradeLanesSynchronisationTaskSyncType.Full);
					}
					else
					{
						var partialSyncCount = SyncInBatches(tradeLinesSummaryProvider, orgQueue, SalesTradeLanesSynchronisationTaskSyncType.Partial, monthlySyncFromDate, monthlySyncToDate, factoryProvider, token);
						syncResult.Add(partialSyncCount, SalesTradeLanesSynchronisationTaskSyncType.Partial);
					}

					ServiceLogger.Log(LogType.Debug,
						string.Format(CultureInfo.InvariantCulture,
							@"Synchronization Date Range: From:{0} To:{1}   Synchronized organizations: {2}   Skipped organizations: {3}",
							ToFullDateString(tradeLinesSummaryProvider.CacheFrom),
							ToFullDateString(tradeLinesSummaryProvider.CacheTo),
							syncResult.TotalSuccuess,
							syncResult.GetAllSkipped().Length));
				}

				monthlySyncFromDate = monthlySyncFromDate.AddMonths(-1);
				monthlySyncToDate = monthlySyncToDate.AddMonths(-1);
				syncResult.Reset();
			}

			return syncResult;
		}

		protected virtual void BuildCache(CachedTradeLinesSummaryProvider tradeLinesSummaryProvider)
		{
			tradeLinesSummaryProvider.BuildCache();
		}

		protected virtual TradeLinesSynchroniser CreateTradeLinesSynchroniser(ZGuid orgPk, bool isFullSync)
		{
			return new TradeLinesSynchroniser(orgPk, null, isFullSync);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is logged")]
		SynchroniseCount SyncInBatches(CachedTradeLinesSummaryProvider tradeLinesSummaryProvider, ISynchronisationOrgQueue orgQueue, SalesTradeLanesSynchronisationTaskSyncType syncType, ZDate from, ZDate to, BusinessObjectFactoryProvider factoryProvider, CancellationToken token)
		{
			var synchronisedCount = new SynchroniseCount();
			var isFullSync = syncType == SalesTradeLanesSynchronisationTaskSyncType.Full;

			var totalCount = orgQueue.GetOrgsToProcessTotalCount(isFullSync);
			if (totalCount > 0)
			{
				ServiceLogger.Log(LogType.Debug, "Total organizations to " + syncType.ToString() + " sync: " + totalCount);

				Guid[] orgPksToSync = null;
				while ((orgPksToSync = orgQueue.GetOrgPksToSync(isFullSync, BatchSize)).Length > 0)
				{
					token.ThrowIfCancellationRequested();
					var orgBatch = factoryProvider.Current.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, orgPksToSync));
#if DEBUG
					DeleteOrgs_ForTesting();
#endif
					foreach (var org in orgBatch)
					{
						if (!org.IsSystemDefinedOrganisation)
						{
							try
							{
								var tradeLinesSynchroniser = CreateTradeLinesSynchroniser(org.PK, isFullSync);
								if (tradeLinesSynchroniser.OrganisationFound)
								{
									tradeLinesSynchroniser.Execute(tradeLinesSummaryProvider, from, to);
									synchronisedCount.Success++;
								}
								else
								{
									ServiceLogger.Log(LogType.Debug, $"Organization, PK {org.PK}, Code {org.OH_Code}, Name {org.OH_FullName} was not found"); // Service Task Logging
									synchronisedCount.Skipped.Add(org.PK.ToGuid());
								}
#if DEBUG
								ThrowError_ForTesting();
#endif
							}
							catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.InvalidObjectName && ex.Message.Contains(TradeLinesSummaryProviderCommon.TradeLineCacheTableName))
							{
								throw;
							}
							catch (Exception ex) when (!ex.IsCriticalException())
							{
								synchronisedCount.Skipped.Add(org.PK.ToGuid());
								ServiceLogger.Log(LogType.Error, ex.Message);
								ErrorReporter.ReportOnce(ex.Message, ex);
							}
						}
						else
						{
							synchronisedCount.Success++;
						}
					}

					tradeLinesSummaryProvider.ResetFactory();
					ServiceLogger.Log(LogType.Debug, "Processed and saved " + synchronisedCount.Success + " Organizations");

#if DEBUG
					BatchComplete_ForTesting(orgBatch);
#endif
				}

#if DEBUG
				ResetExpectedOrgsInBatch_ForTesting();
#endif
			}

			return synchronisedCount;
		}

		#region Config

		public string ConfigString { get; set; }

		public SalesTradeLanesSyncTaskConfig Config
		{
			get { return new SalesTradeLanesSyncTaskConfig(ConfigString); }
		}

		#endregion

		#region Exposed For Testing

#if DEBUG
		protected virtual void BatchComplete_ForTesting(OrgHeader[] processedBatch)
		{
		}

		protected virtual void ThrowError_ForTesting()
		{
		}

		protected virtual void ResetExpectedOrgsInBatch_ForTesting()
		{
		}

		protected virtual void DeleteOrgs_ForTesting()
		{
		}

#endif

		#endregion

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Service Task Logging")]
		static string ToFullDateString(ZDate date)
		{
			return date.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
		}

		#endregion

		#region Classes

		class SynchroniseCount
		{
			public int Success;
			public readonly List<Guid> Skipped = new List<Guid>();
		}

		class SynchroniseResult
		{
			public int FullSuccess;
			public int PartialSuccess;
			public readonly Queue<Guid> FullSkipped = new Queue<Guid>();
			public readonly Queue<Guid> PartialSkipped = new Queue<Guid>();

			public int TotalSuccuess
			{
				get { return FullSuccess + PartialSuccess; }
			}

			public Guid[] GetAllSkipped()
			{
				return FullSkipped.Union(PartialSkipped).ToArray();
			}

			public void Add(SynchroniseCount syncCount, SalesTradeLanesSynchronisationTaskSyncType syncType)
			{
				if (syncType == SalesTradeLanesSynchronisationTaskSyncType.Full)
				{
					FullSuccess += syncCount.Success;
					foreach (var skipped in syncCount.Skipped)
					{
						FullSkipped.Enqueue(skipped);
					}
				}
				else
				{
					PartialSuccess += syncCount.Success;
					foreach (var skipped in syncCount.Skipped)
					{
						PartialSkipped.Enqueue(skipped);
					}
				}
			}

			public void Reset()
			{
				FullSuccess = 0;
				PartialSuccess = 0;
				FullSkipped.Clear();
				PartialSkipped.Clear();
			}
		}

		#endregion
	}

	public enum SalesTradeLanesSynchronisationTaskSyncType
	{
		Full,
		Partial
	}
}
