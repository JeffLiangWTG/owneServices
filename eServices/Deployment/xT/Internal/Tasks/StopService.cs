using System;
using System.Threading;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using static System.ServiceProcess.ServiceControllerStatus;

namespace XT.Internal.API.tasks
{
	public class StopService : Task, ICancelableTask
	{
		bool isCancellationRequested = false;

		public override bool Execute()
		{
			try
			{
				SharedObjects.xtService.MachineName = MachineName;
				SharedObjects.xtService.ServiceName = ServiceName;
				if (SharedObjects.xtService.Status == Stopped)
				{
					Log.LogMessage(MessageImportance.High, $"Service {ServiceName} on {MachineName} is already Stopped; Skip.");
				}
				else
				{
					Log.LogMessage(MessageImportance.High, $"Stopping Service {ServiceName} on {MachineName}.");
					SharedObjects.xtService.Stop();
					SharedObjects.xtService.Refresh();
					while (SharedObjects.xtService.Status == Running || SharedObjects.xtService.Status == StopPending)
					{
						if (isCancellationRequested)
						{
							Log.LogMessage(MessageImportance.High, $"\tBreaking the wait.");
							break;
						}
						Log.LogMessage(MessageImportance.High, $"\tWaiting for 5 second.");
						Thread.Sleep(5000);
						SharedObjects.xtService.Refresh();
					}
					SharedObjects.xtService.Refresh();
					if (SharedObjects.xtService.Status == Stopped)
					{
						Log.LogMessage(MessageImportance.High, $"\tService {ServiceName} on {MachineName} was stopped.");
					}
				}
			}
			finally
			{
				SharedObjects.xtService.Refresh();
				if (SharedObjects.xtService.Status != Stopped)
				{
					Log.LogMessage(MessageImportance.High, $"\tCouldn't Stop Service {ServiceName} on {MachineName}.");
					if (ThrowExceptionOnFailure)
					{
						throw new InvalidOperationException($"Couldn't Stop Service {ServiceName} on {MachineName}.");
					}
				}
			}
			return SharedObjects.xtService.Status == Stopped;
		}

		public void Cancel()
		{
			Log.LogMessage(MessageImportance.High, $"\tCancellation was requested.");
			isCancellationRequested = true;
		}

		public string MachineName { get; set; }
		public string ServiceName { get; set; }
		public bool ThrowExceptionOnFailure { get; set; } = false;
	}
}
