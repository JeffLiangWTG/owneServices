using System.Threading;
using CargoWise.Application;
using Enterprise.ProductionRules.ServiceTasks;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	ScheduledProductionRuleQueueGeneratorTask.ServiceTaskCode,
	ScheduledProductionRuleQueueGeneratorTask.ServiceTaskDescription,
	"SYS",
	typeof(ScheduledProductionRuleQueueGeneratorTask),
	CanRunInAnyBranch = true,
	IsMandatory = true,
	MinimumPeriod = "5minutes",
	DefaultScheduleRunEvery = "5minutes",
	ActiveByDefault = true)
]
[assembly: HostedServiceQueueProvider(ScheduledProductionRuleQueueGeneratorTask.ServiceTaskCode, ScheduledProductionRuleQueueGeneratorTask.ServiceTaskDescription, typeof(ScheduledProductionRuleQueueGeneratorTask))]
namespace Enterprise.ProductionRules.ServiceTasks
{
	class ScheduledProductionRuleQueueGeneratorTask : ServiceProviderImpl, IHostedServiceQueueProvider
	{
		public override void RunTask(CancellationToken token) => TaskRunner.Process(ProductionRuleSchema.Constants.Prefix, false, ServiceLogger.GetTaskNotificationSubscriber(), token);

		QueueResult IHostedServiceQueueProvider.QueueResult => TaskRunner.GetPendingJobsQueue(ProductionRuleSchema.Constants.Prefix);

		IScheduleTaskRunner TaskRunner => ObjectFactory.Get<IScheduleTaskRunner>();

		public const string ServiceTaskCode = "SPR";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Service task name")]
		public const string ServiceTaskDescription = "Scheduled Production Rule Queue Generator";
	}
}
