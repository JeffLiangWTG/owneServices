using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Telematics.Business;

namespace Enterprise.Telematics.ServiceTasks.GpsRoadType
{
	interface IGpsLocationAccessor
	{
		ICollection<GlbDeviceLocation> GetLocations(BusinessObjectFactory factory);
	}
}
