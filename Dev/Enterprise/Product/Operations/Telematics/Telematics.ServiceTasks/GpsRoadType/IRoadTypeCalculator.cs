using System.Collections.Generic;
using System.Threading;
using Enterprise.Telematics.Business;

namespace Enterprise.Telematics.ServiceTasks.GpsRoadType
{
	interface IRoadTypeCalculator
	{
		void MarkLocationRoadType(IEnumerable<GlbDeviceLocation> locations, CancellationToken cancellationToken);
	}
}
