using System;
using System.Threading;
using CargoWise.eHub.Products.JPCustoms.Common;
using Common.Logging;

namespace CargoWise.eHub.Products.JPCustoms.PullService.Common
{
	public class JPCustomsPullRunner : IDisposable
	{
		readonly ILog logger;
		readonly IPullRunnerConfiguration pullRunnerConfiguration;
		readonly IJPCustomsPullManager manager;

		public JPCustomsPullRunner(ILog logger, IPullRunnerConfiguration pullRunnerConfiguration, IJPCustomsPullManager manager)
		{
			if (logger == null) throw new ArgumentNullException("logger");
			this.logger = logger;

			if (pullRunnerConfiguration == null) throw new ArgumentNullException("pullRunnerConfiguration");
			this.pullRunnerConfiguration = pullRunnerConfiguration;

			if (manager == null) throw new ArgumentNullException("manager");
			this.manager = manager;
		}

		public void Start()
		{
			try
			{
				mStop = new AutoResetEvent(false);
				mWorker = new Thread(DoWork);
				mWorker.Start();
				logger.Info("Service started");
			}
			catch (Exception ex)
			{
				logger.Fatal("Error during service starting", ex);
			}
		}

		public void Stop()
		{
			mStop.Set();
			mWorker.Join();
			logger.Info("Service stopped");
		}

		public void Dispose()
		{
			Dispose(true);
		}

		#region Implementation

		Thread mWorker;
		AutoResetEvent mStop = null;

		void DoWork()
		{
			while (true)
			{
				DoWorkCore();
				if (mStop.WaitOne(pullRunnerConfiguration.PullInterval)) return;
			}
		}

		void DoWorkCore()
		{
			try
			{
				manager.PullCustomsForNewMessageAndPushToEHub();
			}
			catch (Exception ex)
			{
				logger.Error("Some error during DoWorkCoreCore.", ex);
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					if (mWorker != null)
					{
						mWorker.Abort();
						mWorker = null;
					}

					if (mStop != null)
					{
						mStop.Dispose();
						mStop = null;
					}
				}

				disposed = true;
			}
		}

		bool disposed = false;

		~JPCustomsPullRunner()
		{
			Dispose(false);
		}

		#endregion
	}
}
