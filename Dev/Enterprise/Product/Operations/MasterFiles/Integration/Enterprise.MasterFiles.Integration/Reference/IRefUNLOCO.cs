using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IRefUNLOCO
	{
		ZGuid PK { get; }
		ZString RL_Code { get; set; }
		ZString RL_PortName { get; set; }
		ZString RL_IATA { get; set; }
		ZString RL_IATARegionCode { get; set; }
		ZString RL_RN_NKCountryCode { get; set; }
		ZGuid RL_RW { get; set; }
		ZDecimal StandardZoneUTCOffset { get; }
	}
}
