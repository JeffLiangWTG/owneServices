namespace Enterprise.Rating.Integration
{
	public interface IRateTransportZone
	{
		IRateTransportZoneItemCollection Items { get; }
		IRateTransportProvider TransportProvider { get; }
	}
}
