using System.Threading;
using CargoWise.Application;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.ServiceTasks;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	WhsTaskCreationServiceTask.ServiceTaskCode,
	WhsTaskCreationServiceTask.ServiceTaskDescription,
	"SYS",
	typeof(WhsTaskCreationServiceTask),
	CanRunInAnyBranch = true,
	IsMandatory = true,
	AllowsMultipleInstances = true,
	MinimumPeriod = "15minutes",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true)
]

[assembly: HostedServiceBusinessObjectBinding(
	WhsTaskCreationServiceTask.ServiceTaskCode,
	WhsPickSchema.Constants.TableName,
	new[] { WhsPickSchema.Constants.WP_TaskPlanningStatus + "=" + TaskPlanningStatus.Codes.Ready },
	"Picks")]

[assembly: HostedServiceBusinessObjectBinding(
	WhsTaskCreationServiceTask.ServiceTaskCode,
	WhsDocketSchema.Constants.TableName,
	new[] { WhsDocketSchema.Constants.WD_TaskPlanningStatus + "=" + TaskPlanningStatus.Codes.Ready },
	"Dockets")]

[assembly: HostedServiceBusinessObjectBinding(
	WhsTaskCreationServiceTask.ServiceTaskCode,
	WhsCycleCountLocationSchema.Constants.TableName,
	new[] { WhsCycleCountLocationSchema.Constants.WCL_TaskPlanningStatus + "=" + TaskPlanningStatus.Codes.Ready },
	"Cycle Count Locations")]

[assembly: HostedServiceBusinessObjectBinding(
	WhsTaskCreationServiceTask.ServiceTaskCode,
	WhsLoadSchema.Constants.TableName,
	new[] { WhsLoadSchema.Constants.WLO_TaskPlanningStatus + "=" + TaskPlanningStatus.Codes.Ready },
	"Loads")]

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	public class WhsTaskCreationServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			Processor.ProcessQueue(ServiceLogger.GetTaskNotificationSubscriber(), token);
		}

		IWhsTaskCreationProcessor Processor => ObjectFactory.Get<IWhsTaskCreationProcessor>();

		[HostedServiceRequirement]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Service task log")]
		public static string CheckTaskManagementEnabled() => HostedServiceRequirementAttribute.CheckValueIsEqualTo(WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement, true);

		public const string ServiceTaskCode = "WTC";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Service task name")]
		public const string ServiceTaskDescription = "Product Warehouse Task Creation";
	}
}
