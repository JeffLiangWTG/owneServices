using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.eTail.Business;
using Enterprise.eTail.DataTransfer;
using Enterprise.eTail.ServiceTasks;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Integration.Forwarding;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;

[assembly: HostedService(
	HVLVOriginLoadListProcessorServiceTask.TaskCode,
	"HVLV Origin Load List Processor",
	"FRT",
	typeof(HVLVOriginLoadListProcessorServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "15minutes")]
[assembly: HostedServiceBusinessObjectBinding(
	"HLP",
	HVLVOriginLoadListSchema.Constants.TableName,
	new[]
	{
		HVLVOriginLoadListSchema.Constants.HVL_Status + "=" + HVLVOriginLoadListStatus.Codes.Lodged
	},
	"HVLV Origin Load List Processor")]
namespace Enterprise.eTail.ServiceTasks
{
	public class HVLVOriginLoadListProcessorServiceTask : ServiceProviderImpl
	{
		public const string TaskCode = "HLP";

		HVLVOriginLoadListHelper OriginLoadListHelper => originLoadListHelper ??= new HVLVOriginLoadListHelper(ServiceLogger);
		HVLVOriginLoadListHelper originLoadListHelper;

		public override void RunTask(CancellationToken token)
		{
			ServiceLogger.Log(LogType.Debug, "The HLP task starts to run...\nThe Task starts to get HVLV origin loadlists...");
			var originLoadLists = GetHVLVOriginLoadListsToProcess(BatchSize);
			if (originLoadLists.Any())
			{
				ServiceLogger.Log(LogType.Debug, string.Format("Successfully found {0} HVLV origin loadlist(s)\nThe HLP task begins to process each origin loadlist", originLoadLists.Count()));
				var originLoadListPKs = originLoadLists.Select(o => o.PK).ToList();
				if (!originLoadListPKs.TryAcquireApplicationLocks<HVLVOriginLoadList>("HVLVOriginLoadListProcessingQueue",
					(lockedPKs) =>
					{
						var originLoadListsWithAppLock = originLoadLists.Where(o => lockedPKs.Any(x => x == o.PK));
						foreach (var currentLoadList in originLoadListsWithAppLock)
						{
							var loadListFactory = new BusinessObjectFactory() { NameForDebugging = "Load List Processor" };
							var originLoadList = (HVLVOriginLoadList)loadListFactory.ImportFromAnotherFactory(currentLoadList);

							var branch = GetBranchFromHVLVOriginLoadListWithFallback(originLoadList);
							if (branch != null)
							{
								using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
								{
									token.ThrowIfCancellationRequested();
									ProcessHVLVOriginLoadList(originLoadList);
								}
							}
							else
							{
								var message = Res.GetString("4509e9fc-8706-49e8-9b68-5821037d5391", "Could not find the creating branch for HVLV Origin Load List {0}. Load list status has been set to 'FAL - Failed'.", originLoadList.HVL_UniqueReference);
								originLoadList.ReportProcessingErrorAndUpdateStatus(message);
								ServiceLogger.Error(message);

								originLoadList.Factory.Save();
							}
						}
					},
					out var errorMessage))
				{
					ServiceLogger.Warning(errorMessage);
				}
			}
			ServiceLogger.Log(LogType.Debug, "The HLP task ends");
		}

		#region Implementation

		const int BatchSize = 100;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "StmALog reference values should not be translated, do not use Res.GetString()")]
		const string HLPSaveConcurrencyErrorLogReasonText = "Concurrency";
		const string HLPSaveConcurrencyErrorLogServiceTaskText = "HLP"; // StmALog reference values should not be translated, do not use Res.GetString()
		const int MaxAutoRetryAttemptsOnConcurrencyError = 5;

		IEnumerable<HVLVOriginLoadList> GetHVLVOriginLoadListsToProcess(int count)
		{
			var factory = new BusinessObjectFactory();
			var query = new ZDBOnlyQuery(typeof(HVLVOriginLoadList));

			query.AddToFilter(HVLVOriginLoadListSchema.HVL_Status, HVLVOriginLoadListStatus.Codes.Lodged);
			query.MaximumRows = count;
			query.OrderBy = HVLVOriginLoadListSchema.HVL_SystemCreateTimeUtc.Name;

			return factory.Load<HVLVOriginLoadList>(query);
		}

		void ProcessHVLVOriginLoadList(HVLVOriginLoadList originLoadList)
		{
			var success = false;
			Exception exceptionOnSaving = null;
			var retryOnNextRun = false;

			ServiceLogger.Log(LogType.Debug, string.Format("Successfully found origin loadlist {0}, try to attach to a consol...", originLoadList.HVL_UniqueReference));

			if (TryAttachToConsol(originLoadList, out var consol, out var errorMessage))
			{
				try
				{
					originLoadList.Factory.Save();
					success = true;
				}
				catch (ZSaveConcurrencyException saveConcurrencyException)
				{
					ServiceLogger.Warning($"Concurrency Error:\r\n{saveConcurrencyException.Message}"); // Service Task logs is English Only

					var errorLogs = originLoadList.Logs.GetAllLogs().Where(l => l.SL_SE_NKEvent == AutoEvents.ErrorReportCode);
					var retriedAttempts = errorLogs.Count(l => l.Parameters.ContainsSameElementsInAnyOrder(
						new[]
						{
								new KeyValuePair<string, string>(EventReferenceParameters.Codes.ServiceTask, HLPSaveConcurrencyErrorLogServiceTaskText),
								new KeyValuePair<string, string>(EventReferenceParameters.Codes.Reason, HLPSaveConcurrencyErrorLogReasonText)
						}));

					if (retriedAttempts < MaxAutoRetryAttemptsOnConcurrencyError)
					{
						retryOnNextRun = true;
					}
					else
					{
						exceptionOnSaving = new Exception($"Load list saving failed due to concurrency error, retry attempts exceeded the threshold of {MaxAutoRetryAttemptsOnConcurrencyError}.", saveConcurrencyException);
					}
				}
				catch (ZSaveException exceptionOnFirstSave)
				{
					try
					{
						ZExceptionReporting.HandleSaveException(exceptionOnFirstSave, new NotificationHandler(ServiceLogger));
						originLoadList.Factory.Save();
						success = true;
					}
					catch (ZSaveConcurrencyException saveConcurrencyException) when (saveConcurrencyException.BusinessObjects.All(x => x is HVLVItemLine))
					{
						retryOnNextRun = true;
					}
					catch (Exception exceptionOnAttemptedFix)
					{
						if (exceptionOnFirstSave.BusinessObjects != null)
						{
							ServiceLogger.Error(Res.GetString("49007e7d-7d3e-4103-b96e-3f15c80fd3b6", "An error occurred when processing the HVLV Origin Load List '{0}'. Load List status has been set to 'FAL - Failed'", originLoadList.HVL_UniqueReference));

							foreach (var bizoHasError in exceptionOnFirstSave.BusinessObjects.Where(bizo => bizo.HasErrors))
							{
								ServiceLogger.ErrorWithSummary(bizoHasError.HumanReadableName, bizoHasError.Notifications.Select(n => n.Message));
							}
						}

						exceptionOnSaving = exceptionOnAttemptedFix;
					}
				}
				catch (MAWBAllocationException mawbAllocationException)
				{
					ServiceLogger.Warning(mawbAllocationException.Message);

					consol.JK_IsNeutralMaster = false;
					originLoadList.Factory.Save();
					success = true;
				}
				catch (Exception ex)
				{
					exceptionOnSaving = ex;
					if (ex.IsCriticalException())
					{
						throw;
					}
				}
			}

			if (success)
			{
				var isConsolInDatabase = ((BusinessObject)consol).IsInDatabase;

				var shipments = string.Join(",", consol.Shipments.Select(x => x.JS_ShipmentType + ":" + x.JS_UniqueConsignRef).OrderByDescending(x => x));

				ServiceLogger.Log(LogType.Debug, string.Format("Shipment(s) saved successfully.\n{0}", shipments));

				var msg = isConsolInDatabase
					? Res.GetString(
						"534c3d34-3a87-49d1-91fb-c155a33d5ab4",
						"The eLoadList '{0}' is attached to a consol '{1}'",
						originLoadList.HVL_UniqueReference,
						consol.JK_UniqueConsignRef)
					: Res.GetString(
						"413340ce-5380-4312-8dff-a90ba2cfb11e",
						"The consol '{1}' is created from eLoadList '{0}'",
						originLoadList.HVL_UniqueReference,
						consol.JK_UniqueConsignRef);

				ServiceLogger.Information(msg);
			}
			else
			{
				var anotherFactory = new BusinessObjectFactory();
				originLoadList = anotherFactory.Load<HVLVOriginLoadList>(originLoadList.PK);

				if (retryOnNextRun)
				{
					originLoadList.Logs.AddNew(AutoEvents.ErrorReport,
								new KeyValuePair<string, string>(EventReferenceParameters.Codes.ServiceTask, HLPSaveConcurrencyErrorLogServiceTaskText),
								new KeyValuePair<string, string>(EventReferenceParameters.Codes.Reason, HLPSaveConcurrencyErrorLogReasonText));

					ServiceLogger.Warning($"Another process has made changes related to Load List '{originLoadList.HVL_UniqueReference}', an attempt to merge the changes will be made in the next schedule."); // Service Task logs is English Only
				}
				else
				{
					var failureReason = (exceptionOnSaving as ZSaveException)?.FriendlyMessage ?? (errorMessage.IsNullOrEmpty() ? Res.GetString("a03e4999-59e9-4fd2-915e-0893eee9d3b3", "Unknown Failure Reason") : errorMessage);

					originLoadList.ReportProcessingErrorAndUpdateStatus(failureReason);

					var message = Res.GetString("16d7c22d-062d-4fd9-93af-c1677fa059af", "An error occurred when processing the HVLV Origin Load List '{0}'", originLoadList.HVL_UniqueReference);
					ServiceLogger.Error(message, exceptionOnSaving);

					if (exceptionOnSaving != null)
					{
						ErrorReporter.ReportOnce(message, exceptionOnSaving);
					}
				}

				anotherFactory.Save();
			}
		}

		bool TryAttachToConsol(HVLVOriginLoadList originLoadList, out IForwardingConsol consol, out string processingFailureReason)
		{
			var succeed = false;
			Exception exceptionOnXUSProcessing = null;
			consol = null;
			processingFailureReason = string.Empty;

			if (HVLVDataRegistry.Instance.ProcessLoadListUsingUniversalXML.Value)
			{
				try
				{
					succeed = TryAttachToConsolXUS(originLoadList, out consol, out processingFailureReason);
				}
				catch (Exception ex)
				{
					exceptionOnXUSProcessing = ex;
					(originLoadList.Factory as IBusinessObjectFactoryInternals).Rollback();
				}
			}

			if (!succeed)
			{
				succeed = TryAttachToConsolOld(originLoadList, out consol, out processingFailureReason);
			}

			if (succeed && exceptionOnXUSProcessing != null)
			{
				ErrorReporter.ReportOnce("Error detected while trying to use Universal XML for load list processing, completed using direct data access instead.", exceptionOnXUSProcessing);
			}

			return succeed;
		}

		bool TryAttachToConsolXUS(HVLVOriginLoadList originLoadList, out IForwardingConsol consol, out string processingFailureReason)
		{
			var allocator = new HVLVOriginLoadListConsolXUSAllocator(ServiceLogger);
			consol = default;

			var allocationSucceed = allocator.TryAttachToConsol(null, new[] { originLoadList }, out processingFailureReason);
			if (allocationSucceed)
			{
				consol = allocator.AllocatedConsol;
			}

			return allocationSucceed;
		}

		bool TryAttachToConsolOld(HVLVOriginLoadList originLoadList, out IForwardingConsol consol, out string errorMessage)
		{
			consol = null;

			if (!originLoadList.Items.Any())
			{
				errorMessage = Res.GetString("ce7e3bae-01f5-4208-9250-1df4344915cd", "HVLV Origin Load List '{0}' does not have any items attached", originLoadList.HVL_UniqueReference);
				ServiceLogger.Error(errorMessage);

				return false;
			}

			ServiceLogger.Log(LogType.Debug, string.Format("Trying to attach a matching consol for origin loadlist {0}", originLoadList.HVL_UniqueReference));

			if (originLoadList != null && !originLoadList.HVL_MasterBillNumber.IsEmpty)
			{
				var query = new ZQuery(JobConsolSchema.JK_MasterBillNum, originLoadList.HVL_MasterBillNumber).AddToFilter(JobConsolSchema.JK_IsCancelled, ZBool.False);
				consol = originLoadList.Factory.LoadTop1<IForwardingConsol>(query);
			}

			if (consol != null)
			{
				ServiceLogger.Log(LogType.Debug, string.Format("Found a matching consol {0} for origin loadlist {1}", consol.JK_UniqueConsignRef, originLoadList.HVL_UniqueReference));
			}
			else
			{
				consol = OriginLoadListHelper.CreateConsol(originLoadList);
			}

			if (!OriginLoadListHelper.TryAttachToConsol(consol, new[] { originLoadList }, out errorMessage))
			{
				return false;
			}

			return true;
		}

		GlbBranch GetBranchFromHVLVOriginLoadListWithFallback(HVLVOriginLoadList originLoadList)
		{
			GlbBranch branch = null;
			var latestLodgeLog = originLoadList.Logs.MostRecentLogByEventTime(
									AutoEvents.StatusUpdated,
									x => x.Parameters.TryGetValue(EventReferenceParameters.Codes.New, out var logTypeValue)
									&& logTypeValue == HVLVOriginLoadListStatus.Codes.Lodged);

			if (latestLodgeLog != null)
			{
				branch = originLoadList.Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, latestLodgeLog.SL_GB_NKBranch);

				if (branch == null)
				{
					var message = Res.GetString("d5c099f9-aaa5-4ec4-84d4-44248b783b81", "Could not find matching branch from latest lodge log. Falling back to creating branch of load list...");
					ServiceLogger.Log(LogType.Warning, message);

					var query = GetBranchQueryFromCreatingUser(originLoadList);
					branch = originLoadList.Factory.LoadTop1<GlbBranch>(query);
				}
			}

			return branch;
		}

		ZDBOnlyQuery GetBranchQueryFromCreatingUser(HVLVOriginLoadList originLoadList)
		{
			var query = new ZDBOnlyQuery(typeof(GlbBranch));
			var staffSubQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.GS_GB_HomeBranch);
			staffSubQuery.AddToFilter(GlbStaffSchema.GS_Code, originLoadList.HVL_SystemCreateUser);
			query.AddSubQuery(GlbBranchSchema.PK, staffSubQuery, JoinCondition.And);

			return query;
		}

		class NotificationHandler : INotificationHandler
		{
			public NotificationHandler(ILogger logger)
			{
				this.logger = logger;
			}

			readonly ILogger logger;

			public void ReportError(string message, string caption, string errorContext = null, Exception exception = null)
			{
				logger.Error(message, exception);
			}

			public void ReportInformation(string message, string caption)
			{
				logger.Warning(message);
			}
		}

		#endregion Implementation
	}
}
