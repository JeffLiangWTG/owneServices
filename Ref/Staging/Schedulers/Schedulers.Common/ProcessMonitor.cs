using System;
using System.Collections.Concurrent;
using System.Threading;
using CargoWise.RefDbRepo.Staging.ProcessorRunner;

namespace CargoWise.RefDbRepo.Staging.Schedulers.Common
{
	public class ProcessMonitor : IDisposable
	{
		public static ProcessMonitor Instance => fInstance ??= new ProcessMonitor(new Job());
		static ProcessMonitor fInstance;
		readonly Job jobManager;
		readonly ConcurrentDictionary<int, (ILogHelper, IProcessWrapper)> monitoredProcesses = new();
		CancellationTokenSource cancellationTokenSource = new();

		public ProcessMonitor(Job jobManager)
		{
			this.jobManager = jobManager;
			var monitoringThread = new Thread(MonitorProcesses);
			monitoringThread.Start();
		}

		public void AddProcessToJobManager(IProcessWrapper process, ILogHelper logHelper)
		{
			if (!jobManager.AddProcess(process.Handle, out var errorCode))
			{
				logHelper.LogError($"Error adding job handle to Process Monitor: {errorCode}");
			}
		}

		public void AddProcessToMemoryMonitor(IProcessWrapper process, ILogHelper logHelper)
		{
			monitoredProcesses.TryAdd(process.Id, (logHelper, process));
		}

		public void KillAll(ILogHelper logHelper)
		{
			monitoredProcesses.Clear();
			if (!jobManager.Close(out var errorCode))
			{
				logHelper.LogError($"Error closing the handle: {errorCode}");
			}
			cancellationTokenSource.Cancel();
		}

		void MonitorProcesses()
		{
			while (cancellationTokenSource != null && !cancellationTokenSource.Token.IsCancellationRequested)
			{
				foreach (var (processId, (logHelper, process)) in monitoredProcesses)
				{
					if (process.HasExited)
					{
						monitoredProcesses.TryRemove(processId, out _);
					}
					else
					{
						process.Refresh();
						logHelper.LogMemoryInfo(process.WorkingSet64 / 1024.0 / 1024.0);
					}
				}
				Thread.Sleep(TimeSpan.FromSeconds(ConfigurationProvider.MemoryMonitorIntervalInSeconds));
			}
		}

		public void Dispose()
		{
			jobManager?.Dispose();
			cancellationTokenSource?.Dispose();
			cancellationTokenSource = null;
		}
	}
}
