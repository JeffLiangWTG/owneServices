using System.Collections.Generic;
using Enterprise.Telematics.Business;

namespace Enterprise.Telematics.ServiceTasks.Rim
{
	public interface IDeviceData
	{
		string DeviceId { get; }
		IEnumerable<GlbDeviceLocation> Locations { get; }
	}
}
