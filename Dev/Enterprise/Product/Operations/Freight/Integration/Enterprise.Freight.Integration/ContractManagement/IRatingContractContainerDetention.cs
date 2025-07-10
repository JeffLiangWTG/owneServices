using CargoWise.Types;

namespace Enterprise.Freight.Integration
{
	public interface IRatingContractContainerDetention
	{
		ZGuid PK { get; }
		ZString RCD_ContainerType { get; set; }
		ZString RCD_PenaltyType { get; set; }
		ZString RCD_Direction { get; set; }
		ZString RCD_OriginPortOrCountry { get; set; }
		ZString RCD_DetentionPortOrCountry { get; set; }
		ZDate RCD_StartDateOverride { get; set; }
		ZDate RCD_EndDateOverride { get; set; }
		ZGuid RCD_OH_Client { get; set; }
		ZGuid RCD_OH_CTO { get; set; }
		ZGuid RCD_RCT { get; set; }
		ZByte RCD_FreeDays { get; set; }
		ZString RCD_FreeDayType { get; set; }
	}
}
