using System;
using CargoWise.EntityFramework;
using Enterprise.Telematics.Business;
using Newtonsoft.Json.Linq;

namespace Enterprise.Telematics.ServiceTasks.TelematicsEquipment
{
	public interface IDbDeviceLinker
	{
		void LinkDeviceToSubEquipment(BusinessObjectFactory factory, GlbDevice device, JObject vehicleInfoJson, DateTimeOffset time);
	}
}
