using CargoWise.Types;

namespace Enterprise.Rating.Integration
{
	public interface IRateTransportProvider
	{
		ZGuid PK { get; }
		ZString TP_RN_NKCountry { get; set; }
		ZString TP_ZoneType { get; set; }
		IRateTransportZonesCollection Zones { get; }
	}
}
