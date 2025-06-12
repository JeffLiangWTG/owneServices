using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading;
using CargoWise.eServices.USCustoms.MQConfiguration;
using Common.Logging;
using ServiceBroker.Interface;

namespace CargoWise.eServices.USCustoms.OutboundProcessingService
{
	class Runner
	{
		readonly ILog logger;
		readonly CancellationTokenSource cancellationTokenSource;

		public Runner(ILog logger)
		{
			if (logger == null) throw new ArgumentNullException("logger");
			this.logger = logger;
			cancellationTokenSource = new CancellationTokenSource();
		}

		#region Start / Stop

		public void StartThread()
		{
			logger.Info("Service started");
			controllerThread = new Thread(Run) {Name = "CargoWise.eServices.USCustoms.OutboundProcessingService"};
			controllerThread.Start();
		}

		public void StopThread()
		{
			Thread.MemoryBarrier();
			stop = true;
			cancellationTokenSource.Cancel();
			if (service != null) service.Stop();
			controllerThread.Join();
			logger.Info("Service was stopped");
		}

		bool stop = false;
		Thread controllerThread;

		#endregion

		void Run()
		{
			var connectionString = ConfigurationManager.ConnectionStrings["USCustoms"];
			if (connectionString == null) throw new ApplicationException("Application configuration missing or connection string with key 'USCustoms' could not be found.");

			do
			{
				try
				{
					DoWorkCore(connectionString);
					Thread.Sleep(2000);
				}
				catch (Exception ex)
				{
					logger.Error(string.Format("Run method exception: {0}", ex));
				}

			}
			while (!stop);
		}

		void DoWorkCore(ConnectionStringSettings connectionString)
		{
			using (var connection = new SqlConnection(connectionString.ConnectionString))
			{
				using (var queueManagerProvider = new MQQueueManagerProvider(logger))
				{
					var messageSender = new MQMessageSender(logger, queueManagerProvider, cancellationTokenSource.Token);

					try
					{
						connection.Open();
						// Instantiate the Service Broker service "TargetService"
						service = new OutboundMessageProcessingService(connection, TimeSpan.FromSeconds(20), logger, messageSender)
						{
							WaitforTimeout = TimeSpan.FromSeconds(30),
							FetchSize = 1
						};

						// Run the message loop of the service
						service.Run(true, connection, null);
					}
					catch (ServiceException ex)
					{
						if (ex.Transaction != null) ex.Transaction.Rollback();
						logger.Error(string.Format("DoWorkCore exception: {0}", ex));
					}
				}
			}
		}

		Service service;
	}
}
