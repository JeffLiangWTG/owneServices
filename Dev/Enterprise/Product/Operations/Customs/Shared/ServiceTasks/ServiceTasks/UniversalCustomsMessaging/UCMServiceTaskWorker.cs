using Enterprise.Customs.Common;
using Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	UniversalCustomsMessagingConstants.ServiceTaskCodes.Worker,
	"Universal Customs Messaging Inbound Worker",
	UCMServiceTaskWorker.ServiceTaskCategory,
	typeof(UCMServiceTaskWorker),
	AllowsMultipleInstances = true,
	IsMandatory = true,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "1minute"
	)]
namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging
{
	public class UCMServiceTaskWorker : UCMServiceTask
	{
		protected override CommonProcessingManager CreatNewManager() => new WorkerProcessingManager();
	}
}
