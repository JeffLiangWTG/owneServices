using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	public static class TaxCoreCountryHelper
	{
		public static ZString[] GetTaxCoreSupportedCountries()
		{
			return new ZString[]
					{
						CountryCodes.Fiji,
						CountryCodes.WesternSamoa
					};
		}

		public static bool IsThisCountrySupportedByTaxCore(ZString countryCode) => GetTaxCoreSupportedCountries().Contains(countryCode);

		public static bool IsInTaxCoreSupportedCountry(this GlbCompany company) => company != null && IsThisCountrySupportedByTaxCore(company.GC_RN_NKCountryCode);
	}
}
