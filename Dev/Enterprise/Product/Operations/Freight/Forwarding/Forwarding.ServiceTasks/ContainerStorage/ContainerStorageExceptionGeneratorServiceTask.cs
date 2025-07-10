using System.Threading;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"CSX", 
	"Container Storage Exception Generator", 
	"WFL", 
	typeof(Enterprise.Freight.Forwarding.ServiceTasks.ContainerStorageExceptionGeneratorServiceTask), 
	MinimumPeriod = "20minutes", 
	IsMandatory = true,
	DefaultScheduleRunEvery = "20minutes",
	CanRunInAnyBranch = true
	)]

// Can't apply HostedServiceBusinessObjectBinding
// The service task uses 'overdue' logic, so it does the processing when an event didn't happen before current time.
namespace Enterprise.Freight.Forwarding.ServiceTasks
{
	internal class ContainerStorageExceptionGeneratorServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			ContainerStorageExceptionGenerator processor = new ContainerStorageExceptionGenerator();
			processor.Process(ServiceLogger.GetTaskNotificationSubscriber(), token);
		}
	}
}
