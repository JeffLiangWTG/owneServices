using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.Customs.ServiceTasks.CustomsMessaging;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(AutoSendCustomsMessagingServiceTask.ServiceTaskCode,
	AutoSendCustomsMessagingServiceTask.ServiceTaskDescription,
	AutoSendCustomsMessagingServiceTask.ServiceTaskCategory,
	typeof(AutoSendCustomsMessagingServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "30Seconds",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(AutoSendCustomsMessagingServiceTask.ServiceTaskCode,
	StmProcessQueueSchema.Constants.TableName,
	new[]
	{
		StmProcessQueueSchema.Constants.SW_ApplicationCode + "=" + CustomsStmProcessQueueLoader.Constants.AutoSendCustomsMessaging,
		StmProcessQueueSchema.Constants.SW_JobTypeCode + "=" + CustomsStmProcessQueueLoader.Constants.JobTypeCode
	},
	"Auto Send Customs Messaging")]

namespace Enterprise.Customs.ServiceTasks.CustomsMessaging
{
	public class AutoSendCustomsMessagingServiceTask : CustomsStmProcessQueueServiceTask
	{
		public const string ServiceTaskCode = "ASC";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "service task description")]
		public const string ServiceTaskDescription = "Auto Send Customs Messaging Service Task";

		protected override string CurrentServiceTaskCode => ServiceTaskCode;

		protected override BaseCustomsStmProcessQueueBatchProcessor GetProcessQueueProcessor(LoggingInformation logger)
		{
			return new AutoSendCustomsMessagingBatchProcessor(logger);
		}
	}
}
