#nullable enable
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Blazor.Common;
using CargoWise.Winzor.Telemetry;
using Enterprise.ZArchitecture.Core;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CargoWise.Winzor.AppServer;

interface IShutDownCircuitHandler;

public class ShutDownCircuitHandler : CircuitHandler, IShutDownCircuitHandler
{
	public ShutDownCircuitHandler(
		ILogger<ShutDownCircuitHandler> logger,
		Application application,
		IOptions<CargoWiseOptions> cwOptions,
		CircuitTelemetry telemetry)
	{
		tracker = new CircuitTracker(logger, application, cwOptions);
		this.telemetry = telemetry;
	}

	readonly CircuitTracker tracker;
	readonly CircuitTelemetry telemetry;

	public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
	{
		return tracker.OnCircuitOpenedAsync(circuit.Id);
	}

	public override Task OnCircuitClosedAsync(Circuit? circuit, CancellationToken cancellationToken)
	{
		telemetry.IncrementCloseCounter();
		return tracker.OnCircuitClosedAsync(circuit?.Id ?? string.Empty);
	}

	public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
	{
		telemetry.IncrementDisconnectCounter();
		return base.OnConnectionDownAsync(circuit, cancellationToken);
	}
}

public class CircuitTracker
{
	public CircuitTracker(
		ILogger<ShutDownCircuitHandler> logger,
		Application application,
		IOptions<CargoWiseOptions> cwOptions)
	{
		this.logger = logger;
		this.application = application;
		_ = InitiateShutdownTimerAsync(cwOptions.Value.CircuitNeverOpenedShutdownTimeLimit, shutdownTimerCts); // don't want to (and in fact can't) await
	}

	readonly ILogger<ShutDownCircuitHandler> logger;
	readonly Application application;
	CancellationTokenSource? shutdownTimerCts = new();
	readonly ConcurrentSet<string> openCircuits = new();

	public async Task OnCircuitOpenedAsync(string circuitId)
	{
		if (!openCircuits.Contains(circuitId))
		{
			logger.LogDebug($"Circuit opened ({circuitId})");
			AddCircuit(circuitId);
			logger.LogTrace((NoResString)"New circuit, abandoning shutdown timer (if running)");
			var cts = Interlocked.Exchange(ref shutdownTimerCts, null);
			if (cts is not null)
			{
				await cts.CancelAsync();
				cts.Dispose();
			}
		}
		else
		{
			logger.LogDebug($"Circuit re-opened ({circuitId})");
		}
	}

	readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
	public async Task OnCircuitClosedAsync(string circuitId)
	{
		try
		{
			await semaphore.WaitAsync();
			if (RemoveCircuit(circuitId))
			{
				logger.LogDebug($"Circuit closed ({circuitId})");
				if (openCircuits.Count == 0)
				{
					logger.LogInformation($"All circuits closed");
					await application.ShutdownAsync();
				}
			}
			else
			{
				logger.LogDebug($"Circuit closed but was never opened ({circuitId})");
			}
		}
		finally
		{
			semaphore.Release();
		}
	}

	void AddCircuit(string circuitId)
	{
		openCircuits.Add(circuitId);
		logger.LogTrace($"Circuit {circuitId} connected!");
		LogCircuits();
	}

	bool RemoveCircuit(string circuitId)
	{
		bool isCircuitRemoved = openCircuits.TryRemove(circuitId);
		logger.LogTrace($"Circuit {circuitId} disconnected!");
		LogCircuits();
		return isCircuitRemoved;
	}

	void LogCircuits()
	{
		logger.LogTrace($"Open circuits: {openCircuits.Count}");

		logger.LogTrace("[");
		foreach (var circuit in openCircuits)
		{
			logger.LogTrace($"    {circuit},");
		}
		logger.LogTrace("]");
	}

	async Task InitiateShutdownTimerAsync(TimeSpan timeLimit, CancellationTokenSource cts)
	{
		logger.LogWarning((NoResString)"Initiating shutdown timer ({0}) because there are no open circuits...", timeLimit);
		try
		{
			await StartTimeoutAsync(timeLimit, cts);
			logger.LogDebug((NoResString)"Shutdown timer complete, exiting...");
			await application.ShutdownAsync();
		}
		catch (OperationCanceledException)
		{
			// Another circuit connected, don't need to do nothing
		}
	}

	async Task StartTimeoutAsync(TimeSpan timeLimit, CancellationTokenSource cts)
	{
		var timer = default(Timer);
		try
		{
			var watch = new Stopwatch();
			watch.Start();
			timer = new Timer(o => logger.LogTrace($"Time to exit: {timeLimit - watch.Elapsed:hh\\:mm\\:ss}"));

			timer.Change(0, 1000);
			await Task.Delay(timeLimit, cts.Token);
		}
		finally
		{
			if (timer is not null)
			{
				timer.Change(Timeout.Infinite, Timeout.Infinite);
				await timer.DisposeAsync();
			}
		}
	}
}
