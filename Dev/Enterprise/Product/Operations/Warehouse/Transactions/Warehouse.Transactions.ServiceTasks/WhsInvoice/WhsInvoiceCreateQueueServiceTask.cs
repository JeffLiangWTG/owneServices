using System;
using System.Threading;
using Enterprise.Integration;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"ARI",
	"Create Periodic Invoice Queue",
	"WHS",
	typeof(Enterprise.Warehouse.Transactions.ServiceTasks.WhsInvoiceCreateQueueServiceTask),
	MinimumPeriod = "1week",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1week",
	DefaultScheduleDaysOfWeek = new DayOfWeek[] { DayOfWeek.Saturday },
	DefaultScheduleStartAtLocal = "4hours"
	)]

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	public class WhsInvoiceCreateQueueServiceTask : ServiceProviderImpl
	{
		#region RunTask

		public override void RunTask(CancellationToken token)
		{
			ServiceLogger.Log(LogType.Information, "Create Queue Invoices Service Task started."); // Service Task Logging
			var processingManager = new CreateAutoRatePeriodicInvoiceManager(ServiceLogger);
			processingManager.CreateQueuePeriodicInvoice(token);
			ServiceLogger.Log(LogType.Information, "Create Queue Invoices Service Task completed."); // Service Task Logging
		}

		#endregion
	}
}
