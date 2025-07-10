namespace Enterprise.Telematics.ServiceTasks.TelematicsEquipment
{
	internal interface IJsonPlanner
	{
		EquipmentFlatJson FlattenJson(string deviceIdentifier, string vehicleInfoJson);
	}
}
