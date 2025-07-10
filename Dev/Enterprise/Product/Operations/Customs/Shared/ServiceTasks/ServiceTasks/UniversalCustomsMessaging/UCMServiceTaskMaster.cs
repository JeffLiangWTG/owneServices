using Enterprise.Customs.Common;
using Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	UniversalCustomsMessagingConstants.ServiceTaskCodes.Master,
	"Universal Customs Messaging Inbound Releaser",
	UCMServiceTaskMaster.ServiceTaskCategory,
	typeof(UCMServiceTaskMaster),
	AllowsMultipleInstances = true,
	IsMandatory = true,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "1minute"
	)]
namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging
{
	public class UCMServiceTaskMaster : UCMServiceTask
	{
		protected override CommonProcessingManager CreatNewManager() => new MessageReleasingManager();
	}
}
