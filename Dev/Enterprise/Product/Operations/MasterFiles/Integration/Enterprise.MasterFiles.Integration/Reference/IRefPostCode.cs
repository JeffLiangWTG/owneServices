using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IRefPostCode
	{
		ZString RK_CityTownPostCode { get; }
		ZString RK_RN_NKCountry { get; }
	}
}
