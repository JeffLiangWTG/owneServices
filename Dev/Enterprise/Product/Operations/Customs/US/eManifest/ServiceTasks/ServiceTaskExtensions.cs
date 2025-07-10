using System;
using System.Threading;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.eManifest.ServiceTasks
{
	static class ServiceTaskExtensions
	{
		internal static void RunProcessForEachActiveCompanyWithValidLicense<T>(this Customs.ServiceTasks.CustomsServiceTask serviceTask, CancellationToken token) where T : BatchProcess
		{
			foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany())
			{
				token.ThrowIfCancellationRequested();

				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					serviceTask.RunTaskHandleEmailSendFailure(() =>
					{
						var process = (T)Activator.CreateInstance(typeof(T));
						process.Logger.OnLogInfoAdded += (log, logType) => serviceTask.ServiceLogger.Log(logType, log);
						process.ExecuteBatch(token);
					});
				}
			}
		}
	}
}
