using System.Collections.Generic;

namespace Enterprise.Telematics.ServiceTasks.TelematicsEquipment
{
	internal sealed class EquipmentFlatJson : Dictionary<TelSubEquipmentKey, (TelSubEquipmentData from, TelSubEquipmentData to)>
	{
	}
}
