namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	using CargoWise.Types;

	public interface IFTZNF90
	{
		ZString AdmissionType { get; }
		ZString ZoneID { get; }
		ZInt CalendarYear { get; }
		ZString ControlNumber { get; }
		ZString PortCode { get; }
		ZString DirectDeliveryIndicator { get; }
	}
}
