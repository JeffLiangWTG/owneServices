namespace Enterprise.Customs.US.AIM.Messaging
{
	public interface IDepartureMessageHeader : IAIMMessageHeader
	{
		IAIMDeparture Departure { get; }
	}
}
