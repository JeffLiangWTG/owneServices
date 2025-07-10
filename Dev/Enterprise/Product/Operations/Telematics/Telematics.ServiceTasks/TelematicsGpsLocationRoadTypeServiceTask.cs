using System;
using System.Threading;
using Enterprise.Telematics.ServiceTasks.GpsRoadType;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.Telematics.ServiceTasks
{
	public class TelematicsGpsLocationRoadTypeServiceTask : ServiceProviderImpl
	{
		public TelematicsGpsLocationRoadTypeServiceTask()
		{
			gpsLocationRoadTypeProcessor = new Lazy<GpsLocationRoadTypeProcessor>(() => new GpsLocationRoadTypeProcessor(ServiceLogger));
		}

		public override void RunTask(CancellationToken cancellationToken)
		{
			gpsLocationRoadTypeProcessor.Value.Run(cancellationToken);
		}

		internal const string Code = "TGP";
		readonly Lazy<GpsLocationRoadTypeProcessor> gpsLocationRoadTypeProcessor;
	}
}
