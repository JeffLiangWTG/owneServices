using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IRefContainer
	{
		ZGuid PK { get; }
		ZString RC_Code { get; set; }
		ZString RC_ContainerType { get; set; }
		ZString RC_Description { get; set; }
		ZString RC_ISOType { get; set; }
		ZString RC_StorageClass { get; set; }
		ZString RC_FreightRateClass { get; set; }
	}
}
