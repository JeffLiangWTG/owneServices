using System;
using System.Threading;
using Enterprise.Telematics.ServiceTasks.PreDriveChecklist;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.Telematics.ServiceTasks
{
	class TelematicsPreDriveChecklistNotificationTask : ServiceProviderImpl
	{
		public TelematicsPreDriveChecklistNotificationTask()
		{
			processor = new Lazy<TelematicsPreDriveChecklistAlertProcessor>(() => new TelematicsPreDriveChecklistAlertProcessor(ServiceLogger));
		}

		public override void RunTask(CancellationToken cancellationToken)
		{
			processor.Value.Run(cancellationToken);
		}

		internal const string Code = "TCH";
		readonly Lazy<TelematicsPreDriveChecklistAlertProcessor> processor;
	}
}
