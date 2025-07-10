using System;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.ProductionRules.ServiceTasks;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	ScheduledProductionRuleQueueConsumerTask.ServiceTaskCode,
	ScheduledProductionRuleQueueConsumerTask.ServiceTaskDescription,
	"SYS",
	typeof(ScheduledProductionRuleQueueConsumerTask),
	CanRunInAnyBranch = true,
	IsMandatory = true,
	AllowsMultipleInstances = true,
	MinimumPeriod = "15minutes",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true)
]
[assembly: HostedServiceQueueProvider(ScheduledProductionRuleQueueConsumerTask.ServiceTaskCode, ScheduledProductionRuleQueueConsumerTask.ServiceTaskDescription, typeof(ScheduledProductionRuleQueueConsumerTask))]

// Table/index scans are ok on this queue, we delete items after processing, and all items in the table are ready to be consumed by this service task
[assembly: HostedServiceBusinessObjectBinding(ScheduledProductionRuleQueueConsumerTask.ServiceTaskCode, ProductionRuleScheduleQueueSchema.Constants.TableName, new string[] { }, "Scheduled Rules")]
namespace Enterprise.ProductionRules.ServiceTasks
{
	class ScheduledProductionRuleQueueConsumerTask : ServiceProviderImpl, IHostedServiceQueueProvider
	{
		public override void RunTask(CancellationToken token)
		{
			QueueConsumer.ProcessQueue(ServiceLogger.GetTaskNotificationSubscriber(), token);
		}

		IScheduledProductionRuleQueueConsumer QueueConsumer => ObjectFactory.Get<IScheduledProductionRuleQueueConsumer>();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Sql query")]
		const string queueQuery = @"
SELECT
	COUNT(*),
	ISNULL(MAX(MaxAge), 0)
FROM
	dbo.ProductionRuleSet
	CROSS APPLY
	(
		SELECT 
			DATEDIFF(second, MIN(PRQ_SystemCreateTimeUtc), GETUTCDATE()) as MaxAge
		FROM
			dbo.ProductionRuleScheduleQueue
	) as Queue
WHERE
	PRS_IsLive = 1 AND
	EXISTS
	(
		SELECT
			NULL
		FROM
			dbo.ProductionRule
			JOIN dbo.ProductionRuleScheduleQueue ON PRQ_PRL_Rule = PRL_PK
		WHERE
			PRL_PRS_RuleSet = PRS_PK
	)";

		QueueResult IHostedServiceQueueProvider.QueueResult
		{
			get
			{
				var result = QueueResult.Error;
				Db.Connection.ExecuteReader(queueQuery,	reader => result = new QueueResult(reader.GetInt32(0), TimeSpan.FromSeconds(reader.GetInt32(1))));
				return result;
			}
		}

		public const string ServiceTaskCode = "SPC";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Service task name")]
		public const string ServiceTaskDescription = "Scheduled Production Rule Queue Consumer";
	}
}
