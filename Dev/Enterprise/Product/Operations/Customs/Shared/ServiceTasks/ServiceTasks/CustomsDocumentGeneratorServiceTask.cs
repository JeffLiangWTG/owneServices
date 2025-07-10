using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.Customs.ServiceTasks;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(CustomsDocumentGeneratorServiceTask.ServiceTaskCode,
	CustomsDocumentGeneratorServiceTask.ServiceTaskDescription,
	CustomsDocumentGeneratorServiceTask.ServiceTaskCategory,
	typeof(CustomsDocumentGeneratorServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "30Seconds",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(CustomsDocumentGeneratorServiceTask.ServiceTaskCode,
	StmProcessQueueSchema.Constants.TableName,
	new[]
	{
		StmProcessQueueSchema.Constants.SW_ApplicationCode + "=" + CustomsStmProcessQueueLoader.Constants.DocumentGeneratorApplicationCode,
		StmProcessQueueSchema.Constants.SW_JobTypeCode + "=" + CustomsStmProcessQueueLoader.Constants.JobTypeCode
	},
	"Customs Document Generator")]

namespace Enterprise.Customs.ServiceTasks
{
	public class CustomsDocumentGeneratorServiceTask : CustomsStmProcessQueueServiceTask
	{
		public const string ServiceTaskCode = "CDG";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "service task description")]
		public const string ServiceTaskDescription = "Customs Document Generator Service Task";

		protected override string CurrentServiceTaskCode => ServiceTaskCode;

		protected override BaseCustomsStmProcessQueueBatchProcessor GetProcessQueueProcessor(LoggingInformation logger)
		{
			return new CustomsDocumentGeneratorProcessor(logger);
		}
	}
}
