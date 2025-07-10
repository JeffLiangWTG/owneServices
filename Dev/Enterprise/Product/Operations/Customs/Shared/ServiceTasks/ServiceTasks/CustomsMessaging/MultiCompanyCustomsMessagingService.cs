using System;
using System.Globalization;
using System.Threading;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.Customs.ServiceTasks
{
	public abstract class MultiCompanyCustomsMessagingService : ServiceProviderImpl
	{
		protected MultiCompanyCustomsMessagingService() { }

		protected abstract ICustomsServiceTaskProcess GetNewProcess();
		protected abstract string RequiredCountry { get; }

		public sealed override void RunTask(CancellationToken token)
		{
			if (!LegacyBatchProcessorCode.IsEmpty && ObjectFactory.Get<IServiceManagerQuerier>().CheckStateOfNamedServiceTask(LegacyBatchProcessorCode) == ServiceTaskStatus.AtLeastOneHostIsRunningHealthily)
			{
				throw new HostedServiceException("Legacy Director Batch Processor (" + LegacyBatchProcessorCode + ") Service Task is running");
			}
			else
			{
				foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany(RequiredCountry))
				{
					try
					{
						using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
						{
							ProcessOneCompanyCore(token);
						}
					}
					catch (EmailSendFailedException ex)
					{
						ServiceLogger.Log(LogType.Error, ex.Message);
					}
				}
			}
		}

		protected virtual ZString LegacyBatchProcessorCode
		{
			get { return ZString.Empty; }
		}

		protected virtual void ProcessOneCompanyCore(CancellationToken token)
		{
			using (ICustomsServiceTaskProcess serviceTaskProcess = GetNewProcessHookedUpToLogger())
			{
				if (Env.Registry.EnableCustomsDiagnostics)
				{
					serviceTaskProcess.Logger.Log(string.Format(CultureInfo.InvariantCulture, "Processing company {0}...", Env.CurrentCompany.Code));
				}
				serviceTaskProcess.ExecuteBatch(token);
			}
		}

		protected ICustomsServiceTaskProcess GetNewProcessHookedUpToLogger()
		{
			ICustomsServiceTaskProcess result = GetNewProcess();
			result.Logger = GetNewLogger();
			return result;
		}

		protected LoggingInformation GetNewLogger()
		{
			LoggingInformation result = new LoggingInformation();
			result.OnLogInfoAdded += new LoggingInformation.LogInfoAdded(Logger_OnLogInfoAdded);
			return result;
		}

		void Logger_OnLogInfoAdded(string log, LogType logType)
		{
			ServiceLogger.Log(logType, log.Trim());
		}

		protected static string[] CheckCustomsEnvironmentIsValidCore() =>
			new ServiceEnvironmentChecker().CheckEverythingRequiredToRunIsInPlace();

#if DEBUG
		public ZString LegacyBatchProcessorCodeForTesting
		{
			get { return LegacyBatchProcessorCode; }
		}

		public Type ProcessTypeForTesting
		{
			get
			{
				Type result;
				using (ICustomsServiceTaskProcess serviceTaskProcess = GetNewProcess())
				{
					result = serviceTaskProcess.GetType();
				}
				return result;
			}
		}

#endif

	}
}
