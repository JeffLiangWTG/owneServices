using System;
using System.Threading;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using static System.ServiceProcess.ServiceControllerStatus;

namespace XT.Internal.API.tasks
{
	public class StartService : Task, ICancelableTask
	{
		bool isCancellationRequested = false;

		public override bool Execute()
		{
			try
			{
				SharedObjects.xtService.MachineName = MachineName;
				SharedObjects.xtService.ServiceName = ServiceName;
				if (SharedObjects.xtService.Status == Running)
				{
					Log.LogMessage(MessageImportance.High, $"Service {ServiceName} on {MachineName} is already Running; Skip.");
				}
				else
				{
					Log.LogMessage(MessageImportance.High, $"Starting Service {ServiceName} on {MachineName}.");
					SharedObjects.xtService.Start();
					SharedObjects.xtService.Refresh();
					while (SharedObjects.xtService.Status == Stopped || SharedObjects.xtService.Status == StartPending)
					{
						if (isCancellationRequested)
						{
							Log.LogMessage(MessageImportance.High, $"\tBreaking the wait.");
							break;
						}
						Log.LogMessage(MessageImportance.High, $"\tWaiting for 1 second.");
						Thread.Sleep(1000);
						SharedObjects.xtService.Refresh();
					}
					SharedObjects.xtService.Refresh();
					if (SharedObjects.xtService.Status == Running)
					{
						Log.LogMessage(MessageImportance.High, $"\tService {ServiceName} on {MachineName} was started.");
					}
				}
			}
			finally
			{
				SharedObjects.xtService.Refresh();
				if (SharedObjects.xtService.Status != Running)
				{
					Log.LogMessage(MessageImportance.High, $"\tCouldn't Start Service {ServiceName} on {MachineName}.");
					if (ThrowExceptionOnFailure)
					{
						throw new InvalidOperationException($"Couldn't Start Service {ServiceName} on {MachineName}.");
					}
				}
			}
			return SharedObjects.xtService.Status == Running;
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
