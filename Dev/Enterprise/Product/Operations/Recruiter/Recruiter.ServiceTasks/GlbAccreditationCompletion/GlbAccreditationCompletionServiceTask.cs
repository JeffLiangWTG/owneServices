using System;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Recruiter.ServiceTasks.GlbAccreditationCompletionServiceTask.Code,
	Enterprise.Recruiter.ServiceTasks.GlbAccreditationCompletionServiceTask.Description,
	"SYS",
	typeof(Enterprise.Recruiter.ServiceTasks.GlbAccreditationCompletionServiceTask),
	IsMandatory = true,
	MinimumPeriod = "5Minutes",
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Recruiter.ServiceTasks.GlbAccreditationCompletionServiceTask.Code, ExamAttemptSchema.Constants.TableName, new[] { ExamAttemptSchema.Constants.EXA_Status + "=" + ExamAttempt.StatusCodes.Queued }, null)]

namespace Enterprise.Recruiter.ServiceTasks
{
	public class GlbAccreditationCompletionServiceTask : ServiceProviderImpl
	{
		public const string Code = "ACS"; // Service Task Code
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Task Description")]
		public const string Description = "Accreditation Completion Service Task";

		public override void RunTask(CancellationToken token)
		{
			ServiceLogger.Log(LogType.Information, "Begin processing");

			var query = new ZQuery(ExamAttemptSchema.EXA_Status, ExamAttempt.StatusCodes.Queued);
			query.AddToFilter(ExamAttemptSchema.EXA_TestCompletedUtc, SQLComparisonOperator.NotEqual, DBNull.Value);
			query.OrderBy = ExamAttemptSchema.Constants.EXA_TestCompletedUtc;

			var reader = new FilteredBusinessObjectReader<ExamAttempt>(query, new BusinessObjectFactory() { RefreshEnabled = false });
			reader.BatchSize = MaxBatchSize;
			reader.SaveBeforeLoadNextEnabled = false;

			var attemptCount = 0;
			foreach (ExamAttempt examAttempt in reader)
			{
				token.ThrowIfCancellationRequested();
				if (!CompleteAccreditationsIfRequiredSafe(examAttempt.PK, token))
				{
					MarkFailedStatus(examAttempt.PK);
				}
				attemptCount++;
			}

			ServiceLogger.Log(LogType.Information, FormattableString.Invariant($"{attemptCount} Exam Attempt(s) processed"));
			ServiceLogger.Log(LogType.Information, "End processing");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		bool CompleteAccreditationsIfRequiredSafe(ZGuid examAttemptPk, CancellationToken token)
		{
			Exception lastException = null;

			for (var retry = 0; retry < MaxRetries; retry++)
			{
				token.ThrowIfCancellationRequested();
				try
				{
					var examAttempt = new BusinessObjectFactory() { RefreshEnabled = false }.Load<ExamAttempt>(examAttemptPk);
					if (examAttempt != null)
					{
						examAttempt.CampaignItem.CompleteAccreditationsIfRequired();
						examAttempt.EXA_Status = ExamAttempt.StatusCodes.Processed;
						examAttempt.Factory.Save();
					}
					return true;
				}
				catch (ZSaveConcurrencyException ex)
				{
					lastException = ex;
					Thread.Sleep(100);
				}
				catch (Exception ex)
				{
					lastException = ex;
					break;
				}
			}

			ServiceLogger.Log(LogType.Error, FormattableString.Invariant($"ExamAttemptPk:{examAttemptPk} - {lastException.Message}"), lastException);
			return false;
		}

		void MarkFailedStatus(ZGuid examAttemptPk)
		{
			var examAttempt = new BusinessObjectFactory() { RefreshEnabled = false }.Load<ExamAttempt>(examAttemptPk);
			if (examAttempt != null)
			{
				examAttempt.EXA_Status = ExamAttempt.StatusCodes.Failed;
				examAttempt.Factory.Save();
			}
		}

		const int MaxRetries = 3;
		const int MaxBatchSize = 100;
	}
}
