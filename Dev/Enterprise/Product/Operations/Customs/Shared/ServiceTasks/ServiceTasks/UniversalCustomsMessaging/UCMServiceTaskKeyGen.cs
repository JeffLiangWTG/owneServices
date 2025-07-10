using Enterprise.Customs.Common;
using Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	UniversalCustomsMessagingConstants.ServiceTaskCodes.KeyGen,
	"Universal Customs Messaging Inbound Key Generator",
	UCMServiceTaskKeyGen.ServiceTaskCategory,
	typeof(UCMServiceTaskKeyGen),
	AllowsMultipleInstances = true,
	IsMandatory = true,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "1minute"
	)]
namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging
{
	public class UCMServiceTaskKeyGen : UCMServiceTask
	{
		protected override CommonProcessingManager CreatNewManager() => new KeyGenProcessingManager();
	}
}
