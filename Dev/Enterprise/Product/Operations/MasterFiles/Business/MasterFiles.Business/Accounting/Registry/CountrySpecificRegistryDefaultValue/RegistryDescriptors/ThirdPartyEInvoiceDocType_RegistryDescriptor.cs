using System;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CountrySpecificRegistryDefaultValue
{
	public class ThirdPartyEInvoiceDocType_RegistryDescriptor : CountrySpecificDefaultRegistryDescriptor<string>
	{
		protected override MultilingualString GetDefaultTypeCaptionCore()
			=> ResString.GetMultilingualString("327C5AFA-DC22-47ED-AB48-E5BDD4C27F15", "Third Party e-Invoice Doc Type (CW1 Support Only)");

		protected override Func<IDefaultValuesForCountrySpecificRegistryItems, string> DefaultValueGetter
			=> defaultRegistryValuesForCountry => defaultRegistryValuesForCountry.ThirdPartyEInvoiceDocType ?? string.Empty;
	}
}
