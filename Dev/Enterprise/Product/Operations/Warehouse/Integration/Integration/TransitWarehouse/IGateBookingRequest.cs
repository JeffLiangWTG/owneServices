namespace Enterprise.Warehouse.Integration
{
	public interface IGateBookingRequest
	{
		string ContainerNumber { get; }

		string CargoReferenceNumber { get; }

		string FacilityCode { get; }
		string TransporterCode { get; }
		bool IsDropOffNotificationType { get; }
	}
}
