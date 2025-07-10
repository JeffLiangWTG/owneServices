using System;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CountrySpecificRegistryDefaultValue
{
	public class ShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency_RegistryDescriptor : CountrySpecificDefaultRegistryDescriptor<bool>
	{
		protected override MultilingualString GetDefaultTypeCaptionCore()
		{
			return ResString.GetMultilingualString("e5a68d82-a212-46cb-ac55-76f501125e8f", "Show Local Currency Equivalent Totals on AR Invoice in OS Currency");
		}

		protected override Func<IDefaultValuesForCountrySpecificRegistryItems, bool> DefaultValueGetter
		{
			get
			{
				return defaultRegistryValuesForCountry => defaultRegistryValuesForCountry.GetShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency();
			}
		}
	}
}
