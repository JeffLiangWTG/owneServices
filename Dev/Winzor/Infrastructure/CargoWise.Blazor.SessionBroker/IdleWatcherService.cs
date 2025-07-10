using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Yarp.ReverseProxy.Configuration;

namespace CargoWise.Blazor.SessionBroker
{
	public class IdleWatcherService : IHostedService, IDisposable
	{
		readonly ILogger<IdleWatcherService> logger;
		readonly IOptions<ShutdownOptions> shutdownOptions;
		readonly RequestTracker requestTracker;
		readonly IProxyConfigProvider proxyConfigProvider;
		Timer _timer;

		public IdleWatcherService(ILogger<IdleWatcherService> logger, IOptions<ShutdownOptions> shutdownOptions, RequestTracker requestTracker, IProxyConfigProvider proxyConfigProvider)
		{
			this.logger = logger;
			this.shutdownOptions = shutdownOptions;
			this.requestTracker = requestTracker;
			this.proxyConfigProvider = proxyConfigProvider;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public Task StartAsync(CancellationToken stoppingToken)
		{
			logger.LogInformation("Timed Hosted Service running.");
			_timer = new Timer(KillIfIdle, null, (int)shutdownOptions.Value.IdleWatcherStartDueTime.TotalMilliseconds, (int)shutdownOptions.Value.IdleWatcherCheckPeriod.TotalMilliseconds);
			return Task.CompletedTask;
		}

		/// <summary>
		/// This method shuts down the SessionBroker if it's idle
		/// We define idle as:
		///  1. No requests have been received in the last 3 minutes
		///  2. AND there aren't any active AppServers
		///  3. AND there aren't any in-flight requests
		/// AppServers have a separate shutdown on idle method
		/// The SessionBroker will automatically remove them from the cluster once they shutdown after the next health check
		/// </summary>
		/// <param name="state"></param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		void KillIfIdle(object state)
		{
			var elapsed = requestTracker.TimeElapsedSinceLastActionCompleted();

			if (
				requestTracker.ActiveActions == 0
				&& elapsed > shutdownOptions.Value.ShutdownThresholdSinceLastActiveAction
				&& proxyConfigProvider.GetConfig().Clusters.All(d => d.Destinations?.Any() == false))
			{
				logger.LogInformation($"Server has been idle for {elapsed}. Shutting down...");
				Environment.Exit(0);
			}
			else
			{
				logger.LogTrace("Server is active. Not shutting down.");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public Task StopAsync(CancellationToken stoppingToken)
		{
			logger.LogInformation("Timed Hosted Service is stopping.");

			_timer?.Change(Timeout.Infinite, 0);

			return Task.CompletedTask;
		}

		public void Dispose()
		{
			_timer?.Dispose();
		}
	}
}
