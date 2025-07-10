using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Utils;
using Common.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;


namespace CargoWise.RefDbRepo.Common.Web
{
	public class MemoryUsageHostedServiceOptions
	{
		public int IntervalSeconds { get; set; } = 60;
	}

	public class MemoryUsageHostedService : IHostedService, IDisposable
	{
		Timer timer;
		readonly ILog log;
		readonly MemoryUsageHostedServiceOptions options;

		public MemoryUsageHostedService(ILogWrapper logWrapper, IOptions<MemoryUsageHostedServiceOptions> options)
		{
			log = logWrapper?.GetLog<MemoryUsageHostedService>();
			this.options = options.Value;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			log?.Info($"MemoryUsageHostedService starting - interval {options.IntervalSeconds} seconds");
			timer = new Timer(LogMemoryUsage, null, TimeSpan.Zero, TimeSpan.FromSeconds(options.IntervalSeconds));
			return Task.CompletedTask;
		}

		void LogMemoryUsage(object state)
		{
			try
			{
				var process = Process.GetCurrentProcess();

				if (process == null)
				{
					log?.Error("MemoryUsageHostedService - unable to get current process");
					return;
				}

				var workingSetMB = 0.0;
				if (process.WorkingSet64 > 0)
				{
					workingSetMB = Convert.ToDouble(process.WorkingSet64) / 1024 / 1024;
				}


				var memoryLog = new WebServiceMemoryUsageLog()
				{
					ProcessId = process.Id,
					Memory = workingSetMB,
				};

				log?.Info(memoryLog);
			}
			catch (InvalidOperationException ex)
			{
				log?.Error("MemoryUsageHostedService - error logging memory usage", ex);
			}
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			timer?.Change(Timeout.Infinite, 0);
			return Task.CompletedTask;
		}

		public void Dispose()
		{
			timer?.Dispose();
			log?.Debug("MemoryUsageHostedService disposed");
		}
	}
}


