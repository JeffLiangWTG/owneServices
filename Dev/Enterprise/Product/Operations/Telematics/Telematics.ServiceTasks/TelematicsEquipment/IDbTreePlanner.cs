using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Telematics.ServiceTasks.TelematicsEquipment
{
	internal interface IDbTreePlanner
	{
		bool TryFlattenTree(string hardwareId, string equipmentType, DateTimeOffset time, out IList<TelEdgeEquipmentTreeNode> dbTree);
		bool TryFlattenTree(BusinessObjectFactory factory, string hardwareId, string equipmentType, DateTimeOffset time, out IList<TelEdgeEquipmentTreeNode> dbTree);
		bool CanAddEntry(string hardwareId, DateTimeOffset time);
		IDictionary<string, SimpleSubEquipment> GetSubEquipmentFromTree(BusinessObjectFactory factory, string deviceId, DateTimeOffset dateTimeOffset, string equipmentType);
	}
}
