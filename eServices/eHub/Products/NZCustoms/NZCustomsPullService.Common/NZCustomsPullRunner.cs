using System;
using System.Threading;
using Common.Logging;

namespace CargoWise.eHub.Products.NZCustoms.PullService
{
	public class NZCustomsPullRunner : IDisposable
	{
		IConfigurationProvider configurationProvider;
		INZCustomsPullManager manager;
		ILog logger;

		public NZCustomsPullRunner(IConfigurationProvider configurationProvider, INZCustomsPullManager manager, ILog logger)
		{
			if (configurationProvider == null) throw new ArgumentNullException("configurationProvider");
			this.configurationProvider = configurationProvider;

			if (manager == null) throw new ArgumentNullException("manager");
			this.manager = manager;

			if (logger == null) throw new ArgumentNullException("logger");
			this.logger = logger;
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
				while (DoWorkCore()) { };
				if (mStop.WaitOne(configurationProvider.PullInterval)) return;
			}
		}

		bool DoWorkCore()
		{
			try
			{
				if (mStop.WaitOne(0))
				{
					mStop.Set();
					logger.Debug("Requested Stop");
					return false;
				}

				return manager.PullNZCustomsServiceForNewMessageAndPushItToBiztalk();
			}
			catch (Exception ex)
			{
				logger.Error("Some error during DoWorkCoreCore.", ex);
			}

			return false;
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

		~NZCustomsPullRunner()
		{
			Dispose(false);
		}

		#endregion
	}
}
