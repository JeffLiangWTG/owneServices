using System.Threading;
using Enterprise.Customs.US.eManifest.Messaging.Interchange;
using Enterprise.Customs.US.eManifest.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	InterchangeComposerServiceTask.ServiceTaskCode,
	InterchangeComposerServiceTask.ServiceTaskDescription,
	InterchangeComposerServiceTask.ServiceTaskCategory,
	typeof(InterchangeComposerServiceTask),
	MinimumPeriod = "60Seconds",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding(InterchangeComposerServiceTask.ServiceTaskCode,
	EDIMessageSchema.Constants.TableName,
	new[] { EDIMessageSchema.Constants.EM_Status + "=" + Enterprise.Messaging.Business.EDIMessage.Status.Queued,
				 EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Transmit,
				 EDIMessageSchema.Constants.EM_IsActive + "=Y",
				 EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.USeManifest,
				 EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL" },
	"US Customs eManifest messages outbound"
	)]

namespace Enterprise.Customs.US.eManifest.ServiceTasks
{
	public class InterchangeComposerServiceTask : Customs.ServiceTasks.CustomsServiceTask
	{
		public const string ServiceTaskCategory = "USC";
		public const string ServiceTaskCode = "UMS";
		public const string ServiceTaskDescription = "US e-Manifest Interchange Composer";

		protected override void RunTaskCore(CancellationToken token)
		{
			this.RunProcessForEachActiveCompanyWithValidLicense<InterchangeComposer>(token);
		}
	}
}
