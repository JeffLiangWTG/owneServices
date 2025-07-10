using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business.ServiceTasks;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService("HFN", "Hourly Freight Notification", "FRT", typeof(HourlyFreightNotificationEmailSenderTask),
	MinimumPeriod = "15minutes",
	MaximumPeriod = "1day",
	DefaultScheduleRunEvery = "1hour",
	CanRunInAnyBranch = true,
	ActiveByDefault = true)
	]

// Can't apply HostedServiceBusinessObjectBinding
// This service task gathers container information which happened during some period of time and presents in an email.
namespace Enterprise.Freight.Business.ServiceTasks
{
	class HourlyFreightNotificationEmailSenderTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			var timeTaskStarts = string.Empty;
			var timeTaskEnds = string.Empty;
			var logMessage = string.Format(CultureInfo.InvariantCulture,
					(NoResString)"HFN service task for CT Freight has successfully run.");

			try
			{
				timeTaskStarts = ZDateTime.Now.ToString();
				new HourlyFreightNotificationEmailSender().SendEmailsIfRequired(ServiceLogger.GetTaskNotificationSubscriber(), token);
				timeTaskEnds = ZDateTime.Now.ToString();
			}
			catch (ZSaveConcurrencyException)
			{
				logMessage = string.Format(CultureInfo.InvariantCulture,
					(NoResString)"Concurrency error with HFN service task for CT Freight has occurred.");
				throw;
			}
			finally
			{
				if (GlbCompany.CurrentCompany.LicenceEnterpriseCode == "CTF")
				{
					#region SuppressResourceStringsCheckRegion

					var logMessageDetails = FormattableString.Invariant($@"Task was started at {timeTaskStarts}, and ended at {timeTaskEnds}
Current process ID is {Process.GetCurrentProcess().Id}
Current machine name is {System.Environment.MachineName}
Current thread ID is {Thread.CurrentThread.ManagedThreadId}");

					#endregion

					ServiceLogger.Log(LogType.Information, logMessage + logMessageDetails);
				}
			}
		}
	}
}
