using System.Threading;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"CDX", 
	"Container Detention Exception Generator", 
	"WFL", 
	typeof(Enterprise.Freight.Forwarding.ServiceTasks.ContainerDetentionExceptionGeneratorServiceTask), 
	MinimumPeriod = "20minutes", 
	IsMandatory = true,
	DefaultScheduleRunEvery = "20minutes",
	CanRunInAnyBranch = true
	)]

// Can't apply HostedServiceBusinessObjectBinding
// The service task uses 'overdue' logic, so it does the processing when an event didn't happen before current time.
namespace Enterprise.Freight.Forwarding.ServiceTasks
{
	internal class ContainerDetentionExceptionGeneratorServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			ContainerDetentionExceptionGenerator processor = new ContainerDetentionExceptionGenerator();
			processor.Process(ServiceLogger.GetTaskNotificationSubscriber(), token);
		}
	}
}
