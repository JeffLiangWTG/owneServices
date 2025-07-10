using System;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CountrySpecificRegistryDefaultValue
{
	public class EInvoicingReversalCodes_RegistryDescriptor : CountrySpecificDefaultRegistryDescriptor<ReadOnlyCodeDescriptionPairList>
	{
		protected override MultilingualString GetDefaultTypeCaptionCore()
			=> (NoResString)"E-Invoicing Reversal Codes (CargoWise Support Only)";

		protected override Func<IDefaultValuesForCountrySpecificRegistryItems, ReadOnlyCodeDescriptionPairList> DefaultValueGetter
			=> defaultRegistryValuesForCountry => defaultRegistryValuesForCountry.EInvoicingReversalCodes ?? new ReadOnlyCodeDescriptionPairList();

		protected override string GetDisplayValue(ReadOnlyCodeDescriptionPairList value)
		{
			return value.GetHumanReadableListOfElements("; ");
		}
	}
}
