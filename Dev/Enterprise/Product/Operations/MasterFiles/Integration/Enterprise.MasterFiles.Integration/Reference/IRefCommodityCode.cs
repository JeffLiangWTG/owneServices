using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IRefCommodityCode
	{
		ZString RH_Code { get; }
		ZGuid PK { get; }
		ZBool RH_IsHazardous { get; }
	}
}
