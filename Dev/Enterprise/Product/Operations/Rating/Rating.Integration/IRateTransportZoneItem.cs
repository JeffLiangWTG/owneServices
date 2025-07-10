using CargoWise.Types;

namespace Enterprise.Rating.Integration
{
	public interface IRateTransportZoneItem
	{
		ZGuid TQ_R9_CityTown { get; set; }
		ZString TQ_FromPostCode { get; set; }
		ZString TQ_ToPostCode { get; set; }
		ZString TQ_RN_NKCountry { get; }
		ZInt TQ_BeyondHours { get; }
		ZGuid TQ_TZ_DomesticZone { get; }
	}
}
