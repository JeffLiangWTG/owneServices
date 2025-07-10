using System.Threading;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"IMX", 
	"Import Docs Exception Generator", 
	"WFL", 
	typeof(Enterprise.Freight.Forwarding.ServiceTasks.ImportDocsReceivedBeforeOrderLineRequiredDateExceptionGeneratorServiceTask), 
	MinimumPeriod = "20minutes", 
	IsMandatory = true,
	DefaultScheduleRunEvery = "20minutes",
	CanRunInAnyBranch = true
	)]

// Can't apply HostedServiceBusinessObjectBinding
// The service task uses 'overdue' logic, so it does the processing when an event didn't happen before current time.
namespace Enterprise.Freight.Forwarding.ServiceTasks
{
	internal class ImportDocsReceivedBeforeOrderLineRequiredDateExceptionGeneratorServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			ImportDocsReceivedBeforeOrderLineRequiredDateExceptionGenerator processor = new ImportDocsReceivedBeforeOrderLineRequiredDateExceptionGenerator();
			processor.Process(ServiceLogger.GetTaskNotificationSubscriber(), token);
		}
	}
}
