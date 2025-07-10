using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IRefCountry
	{
		ZGuid PK { get; }
		ZString RN_Code { get; set; }
		ZString RN_Desc { get; set; }
	}
}
