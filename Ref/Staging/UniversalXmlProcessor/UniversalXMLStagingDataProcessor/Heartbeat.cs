using System;
using System.Threading;
using CargoWise.RefDbRepo.Common.Utils;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public class Heartbeat : IDisposable
	{
		bool isRunning = true;
		static Thread heartbeatThread;
		DateTime? notProcessedUntil;

		readonly double heartbeatInterval;
		readonly Guid sourceDataPk;
		readonly IStagingDataProvider stagingDataProvider;
		readonly IDateTimeProvider dateTimeProvider;

		public Heartbeat(double heartbeatInterval, Guid sourceDataPk, IStagingDataProvider stagingDataProvider, IDateTimeProvider dateTimeProvider, DateTime? notProcessedUntil)
		{
			this.heartbeatInterval = heartbeatInterval;
			this.sourceDataPk = sourceDataPk;
			this.stagingDataProvider = stagingDataProvider;
			this.dateTimeProvider = dateTimeProvider;
			this.notProcessedUntil = notProcessedUntil;
		}

		static int ProcessId => Environment.ProcessId;

		public void Start()
		{
			heartbeatThread = new Thread(async () =>
			{
				while (isRunning)
				{
					Console.WriteLine($"Heartbeat sent [ProcessId:{ProcessId}]");

					var source = stagingDataProvider.GetSingleSourceData(sourceDataPk);
					if (source != null && source.SDA_NotProcessedUntil != notProcessedUntil)
					{
						var errorMessage = $"SourceData {sourceDataPk} has unexpected SDA_NotProcessedUntil. Expected: {notProcessedUntil:yyyy-MM-dd HH:mm:ss.fffffff}, actual: {source.SDA_NotProcessedUntil:yyyy-MM-dd HH:mm:ss.fffffff}";
						Console.Error.WriteLine(errorMessage);
					}

					notProcessedUntil = dateTimeProvider.GetUTCNow().AddMinutes(heartbeatInterval * 2);
					await stagingDataProvider.UpdateSDA_NotProcessedUntil(sourceDataPk, notProcessedUntil.Value);

					if (isRunning)
					{
						Thread.Sleep(TimeSpan.FromMinutes(heartbeatInterval));
					}
				}
			});

			heartbeatThread.Start();
		}

		public void Dispose()
		{
			isRunning = false;
			heartbeatThread?.Join();
		}
	}
}
