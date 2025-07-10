using System;
using CargoWise.EntityFramework;

namespace Enterprise.Telematics.ServiceTasks.TelematicsEquipment
{
	internal interface IDbTreeGenerator
	{
		void GenerateTree(string hardwareId, DateTimeOffset time, string vehicleInfoJson);
		void GenerateTree(BusinessObjectFactory factory, string hardwareId, DateTimeOffset time, string vehicleInfoJson);
	}
}
