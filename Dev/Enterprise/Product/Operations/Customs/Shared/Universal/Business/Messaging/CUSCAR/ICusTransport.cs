using CargoWise.Types;

namespace Enterprise.Customs.Universal.Messaging.CUSCAR
{
	public interface ICusTransport
	{
		ZString VoyageFlightNumber { get; }
		ZString TranportMode { get; }
		ZString CarrierCode { get; }
		ZString CarrierName { get; }
		ZString CallSign { get; }
		ZDateTime ETA { get; }
		ZDateTime ETD { get; }
	}
}
