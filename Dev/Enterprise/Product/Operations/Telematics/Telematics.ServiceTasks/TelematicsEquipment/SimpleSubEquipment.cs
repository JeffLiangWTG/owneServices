using CargoWise.Types;

namespace Enterprise.Telematics.ServiceTasks.TelematicsEquipment
{
	class SimpleSubEquipment
	{
		public SimpleSubEquipment(ZGuid pk, string id, string type)
		{
			Pk = pk;
			Id = id;
			Type = type;
		}

		public ZGuid Pk { get; }
		public string Id { get; }
		public string Type { get; }
	}
}
