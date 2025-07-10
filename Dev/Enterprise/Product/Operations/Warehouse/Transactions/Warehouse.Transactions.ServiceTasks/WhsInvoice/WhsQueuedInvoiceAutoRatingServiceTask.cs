using System;
using System.Threading;
using CargoWise.Application;
using Enterprise.Integration;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"WPI",
	"Auto Rate Queued Periodic Invoice",
	"WHS",
	typeof(Enterprise.Warehouse.Transactions.ServiceTasks.WhsQueuedInvoiceAutoRatingServiceTask),
	MinimumPeriod = "1week",
	AllowsMultipleInstances = true,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1week",
	DefaultScheduleDaysOfWeek = new DayOfWeek[] { DayOfWeek.Saturday },
	DefaultScheduleStartAtLocal = "4hours"
	)]

[assembly: HostedServiceBusinessObjectBinding("WPI",
	JobStorageSchema.Constants.TableName,
	new[] { JobStorageSchema.Constants.ET_OffBandProcessingStatus + "=" + StorageOffBandProcessingStatus.Codes.QUE },
	"Invoice in queue")]
namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	public class WhsQueuedInvoiceAutoRatingServiceTask : ServiceProviderImpl
	{
		#region RunTask

		public override void RunTask(CancellationToken token)
		{
			ServiceLogger.Log(LogType.Information, "Autorating Queued Periodic Invoices Service Task started."); // Service Task Logging
			ObjectFactory.Get<IWhsAutoRateQueuedInvoicesProcessSynchronously>().Process(new BillingAutomationServiceLogger(ServiceLogger), token);
			ServiceLogger.Log(LogType.Information, "Autorating Queued Periodic Invoices Service Task completed."); // Service Task Logging
		}

		#endregion
	}
}
