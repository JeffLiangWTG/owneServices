using System;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CountrySpecificRegistryDefaultValue
{
	public class PrintWatermarkForTransactionAwaitingApproval_RegistryDescriptor : CountrySpecificDefaultRegistryDescriptor<string>
	{
		protected override MultilingualString GetDefaultTypeCaptionCore()
			=> ResString.GetMultilingualString("b19fb940-beec-4c22-a980-9ce006093dba",
				"Print Watermark for Transactions awaiting Approval");

		protected override Func<IDefaultValuesForCountrySpecificRegistryItems, string> DefaultValueGetter
			=> defaultRegistryValuesForCountry => defaultRegistryValuesForCountry.PrintWatermarkForTransactionAwaitingApproval ?? string.Empty;
	}
}
