namespace Enterprise.Customs.US.AIM.Messaging
{
	public interface IArrivalMessageHeader : IAIMMessageHeader
	{
		IAIMCargoControlLocation CargoControlLine { get; }
		IAIMAirWaybill AirWaybill { get; }
		IAIMArrival Arrival { get; }
		IAIMAirlineStatusNotification AirlineStatusNotification { get; }
	}
}
