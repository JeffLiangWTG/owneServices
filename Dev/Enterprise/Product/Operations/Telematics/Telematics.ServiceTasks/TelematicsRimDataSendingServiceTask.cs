using System;
using System.Threading;
using Enterprise.Telematics.ServiceTasks.Rim;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.Telematics.ServiceTasks
{
	public class TelematicsRimDataSendingServiceTask : ServiceProviderImpl
	{
		public TelematicsRimDataSendingServiceTask()
		{
			rimDataSender = new Lazy<RimDataSender>(() => new RimDataSender(ServiceLogger));
		}

		public override void RunTask(CancellationToken token)
		{
			rimDataSender.Value.Run(token);
		}

		internal const string Code = "TES";
		readonly Lazy<RimDataSender> rimDataSender;
	}
}
