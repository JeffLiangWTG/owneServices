using System;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.eHub.Shared.ServiceTaskHost.Integration;
using Common.Logging;

namespace CargoWise.eHub.Shared.ServiceTaskHost.Core
{
	public class ServiceTaskRunner : IServiceTaskRunner
	{
		readonly IServiceTask task;
		readonly ILog logger;
		readonly Configuration configuration;
		CancellationTokenSource cancellationTokenSource;
		ManualResetEventSlim manualResetEventSlim;
		Task workerThread;

		public ServiceTaskRunner(IServiceTask task, ILog logger, Configuration configuration,  string serviceTaskName)
		{
			if (task == null) throw new ArgumentNullException("task");
			this.task = task;

			if (logger == null) throw new ArgumentNullException("logger");
			this.logger = logger;

			if (configuration == null) throw new ArgumentNullException("configuration");
			this.configuration = configuration;

			if (string.IsNullOrWhiteSpace(serviceTaskName)) throw new ArgumentException("serviceTaskName");
			ServiceTaskName = serviceTaskName;
		}

		public string ServiceTaskName { get; private set; }

		public void Start()
		{
			try
			{
				cancellationTokenSource = new CancellationTokenSource();
				manualResetEventSlim = new ManualResetEventSlim(false);
				workerThread = new Task(DoWork, cancellationTokenSource.Token);

				workerThread.Start();
				logger.InfoFormat("{0} started", ServiceTaskName);
			}
			catch (Exception ex)
			{
				logger.FatalFormat("Error during {0} starting", ex, ServiceTaskName);
				EventLog.WriteEntry(ServiceTaskName, ex.ToString(), EventLogEntryType.Error);
			}
		}

		public void Stop()
		{
			cancellationTokenSource.Cancel();
			workerThread.Wait();
			logger.InfoFormat("{0} stopped", ServiceTaskName);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public int RunIntervalInSeconds
		{
			get { return RunIntervalInSecondsCore; }
		}

		#region Implementation

		protected virtual int RunIntervalInSecondsCore
		{
			get
			{
				if (runIntervalInSecondsCore == 0)
				{
					string runIntervalInSecondsCoreString = configuration.AppSettings.Settings["RunIntervalInSeconds"].Value;
					if (string.IsNullOrWhiteSpace(runIntervalInSecondsCoreString))
						throw new ConfigurationErrorsException(
							string.Format("Config file for {0} doesn't have RunIntervalInSeconds appSetting", task.GetType()));
					if (!int.TryParse(runIntervalInSecondsCoreString, out runIntervalInSecondsCore))
						throw new ConfigurationErrorsException(
							string.Format("Config file {0} appSetting RunIntervalInSeconds {1} is not valid integer", task.GetType(),
								runIntervalInSecondsCoreString));
					if (runIntervalInSecondsCore == 0)
						throw new ConfigurationErrorsException(
							string.Format("In config file for {0} RunIntervalInSeconds appSetting should be more than 0", task.GetType()));
				}

				return runIntervalInSecondsCore;
			}
		}

		int runIntervalInSecondsCore = 0;

		void DoWork()
		{
			try
			{
				while (!cancellationTokenSource.Token.IsCancellationRequested)
				{
					while (DoWorkCore())
					{
						if (cancellationTokenSource.Token.IsCancellationRequested) return;
					}

					manualResetEventSlim.Wait(RunIntervalInSeconds*1000, cancellationTokenSource.Token);
				}
			}
			catch (OperationCanceledException)
			{
			}
		}

		bool DoWorkCore()
		{
			try
			{
				return task.Run(logger, configuration, cancellationTokenSource.Token);
			}
			catch (Exception ex)
			{
				logger.Error(string.Format("{0} some error during DoWorkCoreCore.", ServiceTaskName), ex);
			}

			return false;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					if (workerThread != null)
					{
						workerThread.Dispose();
						workerThread = null;
					}

					if (manualResetEventSlim != null)
					{
						manualResetEventSlim.Dispose();
						manualResetEventSlim = null;
					}

					if (cancellationTokenSource != null)
					{
						cancellationTokenSource.Dispose();
						cancellationTokenSource = null;
					}
				}
				disposed = true;
			}
		}

		bool disposed;

		~ServiceTaskRunner()
		{
			Dispose(false);
		}

		#endregion

		public ILog SomeClass { get; set; }
	}
}