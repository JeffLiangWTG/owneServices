using System;
using System.Globalization;
using System.Threading;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.Customs.ServiceTasks
{
	public abstract class BranchCustomsMessagingService : ServiceProviderImpl
	{
		protected BranchCustomsMessagingService() { }

		protected abstract ICustomsServiceTaskProcess GetNewProcess();
		protected abstract Guid[] GetBranchPKsHavingDataToProcess();

		public sealed override void RunTask(CancellationToken token)
		{
			SetupPreProcessData();
			foreach (var branchPK in GetBranchPKsHavingDataToProcess())
			{
				try
				{
					using (DisposableEnvironment.ForBranch(branchPK))
					{
						Process(token);
					}
				}
				catch (EmailSendFailedException ex)
				{
					ServiceLogger.Log(LogType.Error, ex.Message);
				}
			}
		}

		protected virtual void SetupPreProcessData() { }

		protected virtual void Process(CancellationToken token)
		{
			using (ICustomsServiceTaskProcess serviceTaskProcess = GetNewProcessHookedUpToLogger())
			{
				if (Env.Registry.EnableCustomsDiagnostics)
				{
					serviceTaskProcess.Logger.Log(string.Format(CultureInfo.InvariantCulture, "Processing company '{0}' branch '{1}'...", GlbCompany.CurrentCompany.GC_Code, GlbBranch.CurrentBranch.GB_Code));
				}
				serviceTaskProcess.ExecuteBatch(token);
			}
		}

		protected ICustomsServiceTaskProcess GetNewProcessHookedUpToLogger()
		{
			var result = GetNewProcess();
			result.Logger = GetNewLogger();
			return result;
		}

		LoggingInformation GetNewLogger()
		{
			var result = new LoggingInformation();
			result.OnLogInfoAdded += new LoggingInformation.LogInfoAdded(Logger_OnLogInfoAdded);
			return result;
		}

		void Logger_OnLogInfoAdded(string log, LogType logType)
		{
			ServiceLogger.Log(logType, log.Trim());
		}

		protected static string[] CheckCustomsEnvironmentIsValidCore() =>
			new ServiceEnvironmentChecker().CheckEverythingRequiredToRunIsInPlace();
	}
}
