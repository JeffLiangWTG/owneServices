using System;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CountrySpecificRegistryDefaultValue
{
	public class TaxMessageIsMandatoryPayables_RegistryDescriptor : CountrySpecificDefaultRegistryDescriptor<string>
	{
		protected override MultilingualString GetDefaultTypeCaptionCore()
			=> ResString.GetMultilingualString("1EF93917-C45C-4F12-AF65-21964EFD44D7", "Invoice Tax Message is Mandatory Payables");

		protected override Func<IDefaultValuesForCountrySpecificRegistryItems, string> DefaultValueGetter
			=> defaultRegistryValuesForCountry => defaultRegistryValuesForCountry.TaxMessageIsMandatoryPayables ?? string.Empty;
	}
}
