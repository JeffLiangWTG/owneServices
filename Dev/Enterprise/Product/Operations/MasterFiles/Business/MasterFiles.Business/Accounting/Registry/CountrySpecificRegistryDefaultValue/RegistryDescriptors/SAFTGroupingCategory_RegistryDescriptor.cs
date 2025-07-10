using System;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CountrySpecificRegistryDefaultValue
{
	public class SAFTGroupingCategory_RegistryDescriptor : CountrySpecificDefaultRegistryDescriptor<string>
	{
		protected override MultilingualString GetDefaultTypeCaptionCore() =>
			ResString.GetMultilingualString("E66E9A0C-F8C8-4E7F-A7DE-D73D2023CADF", "SAF-T Grouping Category");

		protected override Func<IDefaultValuesForCountrySpecificRegistryItems, string> DefaultValueGetter =>
			defaultRegistryValuesForCountry => defaultRegistryValuesForCountry.SAFTGroupingCategory ?? string.Empty;
	}
}
