using System;
using System.Threading;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.Telematics.ServiceTasks
{
	class TelematicsCleanObsoleteDeviceDataServiceTask : ServiceProviderImpl
	{
		public TelematicsCleanObsoleteDeviceDataServiceTask()
		{
			obsoleteDataCleaner = new Lazy<ObsoleteDataCleaner>(() => new ObsoleteDataCleaner(ServiceLogger));
		}

		public override void RunTask(CancellationToken token)
		{
			obsoleteDataCleaner.Value.Run(token);
		}

		public const string Code = "TCL";

		readonly Lazy<ObsoleteDataCleaner> obsoleteDataCleaner;
	}
}
