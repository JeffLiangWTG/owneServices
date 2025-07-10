using System;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CountrySpecificRegistryDefaultValue
{
	public class TaxMessageIsMandatoryReceivables_RegistryDescriptor : CountrySpecificDefaultRegistryDescriptor<string>
	{
		protected override MultilingualString GetDefaultTypeCaptionCore()
			=> ResString.GetMultilingualString("0AA60BE3-38D8-44B2-98B4-D7DD2AB57876", "Invoice Tax Message is Mandatory Receivables");

		protected override Func<IDefaultValuesForCountrySpecificRegistryItems, string> DefaultValueGetter
			=> defaultRegistryValuesForCountry => defaultRegistryValuesForCountry.TaxMessageIsMandatoryReceivables ?? string.Empty;
	}
}
