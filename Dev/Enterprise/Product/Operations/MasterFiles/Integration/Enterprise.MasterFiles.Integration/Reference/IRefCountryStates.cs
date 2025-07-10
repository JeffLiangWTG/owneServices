using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IRefCountryStates
	{
		ZGuid PK { get; }
		ZString RW_Code { get; set; }
		ZString RW_Description { get; set; }
		ZString RW_RegionName { get; set; }
	}
}
