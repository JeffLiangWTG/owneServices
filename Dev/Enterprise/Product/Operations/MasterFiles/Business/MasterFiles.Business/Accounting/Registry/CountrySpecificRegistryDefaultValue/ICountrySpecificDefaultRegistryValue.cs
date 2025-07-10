using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CountrySpecificRegistryDefaultValue
{
	public interface ICountrySpecificDefaultRegistryValue
	{
		MultilingualString GetCaption();
		string GetDefaultValueForDisplay(ZString countryCode);
	}
}
