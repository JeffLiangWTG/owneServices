using System.Threading;
using CargoWise.EntityFramework;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.Telematics.ServiceTasks
{
	public class TelematicsDataProcessingServiceTask : ServiceProviderImpl
	{
		public const string Code = "TEL";

		public override void RunTask(CancellationToken token)
		{
			MessageProcessor.Run(token);
		}

		public EHubMessageProcessor MessageProcessor => messageProcessor ?? (messageProcessor = new EHubMessageProcessor(ServiceLogger, new BusinessObjectFactory { NameForDebugging = "Telematics Data Processing Service Task" }));

		EHubMessageProcessor messageProcessor;
	}
}
