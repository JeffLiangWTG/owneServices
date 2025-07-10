using System;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CountrySpecificRegistryDefaultValue
{
	public class EInvoicingAmendmentCodes_RegistryDescriptor : CountrySpecificDefaultRegistryDescriptor<ReadOnlyCodeDescriptionPairList>
	{
		protected override MultilingualString GetDefaultTypeCaptionCore()
			=> (NoResString)"E-Invoicing Amendment Codes (CargoWise Support Only)";

		protected override Func<IDefaultValuesForCountrySpecificRegistryItems, ReadOnlyCodeDescriptionPairList> DefaultValueGetter
			=> defaultRegistryValuesForCountry => defaultRegistryValuesForCountry.EInvoicingAmendmentCodes ?? new ReadOnlyCodeDescriptionPairList();

		protected override string GetDisplayValue(ReadOnlyCodeDescriptionPairList value) => value.GetHumanReadableListOfElements("; ");
	}
}
