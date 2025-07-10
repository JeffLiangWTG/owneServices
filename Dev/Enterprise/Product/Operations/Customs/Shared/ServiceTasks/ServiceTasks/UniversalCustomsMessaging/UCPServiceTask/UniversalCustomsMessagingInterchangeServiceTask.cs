using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging
{
	public abstract class UniversalCustomsMessagingInterchangeServiceTask<T> : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			var logger = GetNewLogger(ServiceLogger);

			try
			{
				logger.Log($"{ServiceTaskCode} Service Task start");

				var branch = GlbBranch.GetFirstActiveBranch();
				using (Environment.DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					RunTaskCore(logger, token);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger.LogWarning($"{ServiceTaskCode} Service Task Error. Message: {ex.Message}");
			}
		}

		protected void RunTaskCore(LoggingInformation logger, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();

			var subscribers = GetUCMPSubscribers();
			var keysInRandom = ShuffleListInRandomOrder(subscribers.Keys.ToList());
			var connection = Db.Connection;
			var lockName = GetType().Name;
			foreach (var key in keysInRandom)
			{
				token.ThrowIfCancellationRequested();
				var applicationCode = key;
				var handler = subscribers[key];
				SqlApplicationLock sqlAppLock = null;

				try
				{
					if (connection.TryGetLock(applicationCode + lockName, out sqlAppLock))
					{
						var processor = GetProcessor(logger, applicationCode, handler, token);
						processor.Process();
					}
					else
					{
						logger.Log($"{applicationCode} record(s) locked by other task and will not be processed in this run.");
					}
				}
				finally
				{
					if (sqlAppLock != null)
					{
						sqlAppLock.Dispose();
					}
				}
			}
		}

		internal static LoggingInformation GetNewLogger(ILogger serviceLogger)
		{
			var logger = new LoggingInformation();
			logger.OnLogInfoAdded += new LoggingInformation.LogInfoAdded((string log, LogType logType) => { serviceLogger?.Log(logType, log.Trim()); });
			return logger;
		}

		protected abstract Dictionary<string, T> GetUCMPSubscribers();

		string[] ShuffleListInRandomOrder(List<string> list) => list.ShuffleListInRandomOrder();

		protected abstract IUniversalCustomsMessagingInterchangeProcessor GetProcessor(LoggingInformation logger, string applicationCode, T handler, CancellationToken token);

		protected abstract string ServiceTaskCode { get; }
	}
}
