using System.Threading;
using Enterprise.Customs.US.eManifest.Messaging.Interchange;
using Enterprise.Customs.US.eManifest.ServiceTasks;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	InterchangeProcessorServiceTask.ServiceTaskCode,
	InterchangeProcessorServiceTask.ServiceTaskDescription,
	InterchangeProcessorServiceTask.ServiceTaskCategory,
	typeof(InterchangeProcessorServiceTask),
	MinimumPeriod = "60Seconds",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding(InterchangeProcessorServiceTask.ServiceTaskCode,
	EDIInterchangeSchema.Constants.TableName,
	new[] { EDIInterchangeSchema.Constants.EI_Status + "=" + Enterprise.Messaging.Business.EDIInterchange.Status.Queued,
				 EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + Enterprise.Messaging.Business.EDIInterchange.Direction.Receive,
				 EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
				 EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + CBPEDIInterchange.ApplicationCodes.USeManifest },
	"US Customs eManifest interchanges inbound")]

namespace Enterprise.Customs.US.eManifest.ServiceTasks
{
	public class InterchangeProcessorServiceTask : Customs.ServiceTasks.CustomsServiceTask
	{
		public const string ServiceTaskCategory = "USC";
		public const string ServiceTaskCode = "UMP";
		public const string ServiceTaskDescription = "US e-Manifest Interchange Processor";

		protected override void RunTaskCore(CancellationToken token)
		{
			this.RunProcessForEachActiveCompanyWithValidLicense<InterchangeProcessor>(token);
		}
	}
}
