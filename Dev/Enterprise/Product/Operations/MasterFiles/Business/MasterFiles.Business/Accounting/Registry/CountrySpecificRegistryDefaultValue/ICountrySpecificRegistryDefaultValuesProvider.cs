using CargoWise.Types;
using Enterprise.Accounting.CountryCompliance.Interfaces;

namespace Enterprise.MasterFiles.Business.CountrySpecificRegistryDefaultValue
{
	public interface ICountrySpecificRegistryDefaultValuesProvider
	{
		IDefaultValuesForCountrySpecificRegistryItems Get(ZString countryCode);
	}
}
