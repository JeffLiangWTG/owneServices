using System.Threading;
using Enterprise.Customs.US.eManifest.Messaging.MessageProcessors;
using Enterprise.Customs.US.eManifest.ServiceTasks;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	MessageProcessorServiceTask.ServiceTaskCode,
	MessageProcessorServiceTask.ServiceTaskDescription,
	MessageProcessorServiceTask.ServiceTaskCategory,
	typeof(MessageProcessorServiceTask),
	MinimumPeriod = "60Seconds",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding(MessageProcessorServiceTask.ServiceTaskCode,
	EDIMessageSchema.Constants.TableName,
	new[] { EDIMessageSchema.Constants.EM_Status + "=" + Enterprise.Messaging.Business.EDIMessage.Status.Queued,
				 EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Receive,
				 EDIMessageSchema.Constants.EM_IsActive + "=Y",
				 EDIMessageSchema.Constants.EM_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.USeManifest,
				 EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL" },
	"US Customs eManifest messages inbound"
	)]

namespace Enterprise.Customs.US.eManifest.ServiceTasks
{
	public class MessageProcessorServiceTask : Customs.ServiceTasks.CustomsServiceTask
	{
		public const string ServiceTaskCategory = "USC";
		public const string ServiceTaskCode = "UMR";
		public const string ServiceTaskDescription = "US e-Manifest Message Processor";

		protected override void RunTaskCore(CancellationToken token)
		{
			this.RunProcessForEachActiveCompanyWithValidLicense<MessageProcessor>(token);
		}
	}
}
