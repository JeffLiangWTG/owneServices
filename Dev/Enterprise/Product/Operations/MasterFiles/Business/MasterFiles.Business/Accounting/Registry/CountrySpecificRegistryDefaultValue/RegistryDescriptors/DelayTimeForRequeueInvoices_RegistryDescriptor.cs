using System;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CountrySpecificRegistryDefaultValue
{
	public class DelayTimeForRequeueInvoices_RegistryDescriptor : CountrySpecificDefaultRegistryDescriptor<int>
	{
		protected override MultilingualString GetDefaultTypeCaptionCore()
		{
			return (NoResString)"E-Reporting Delay Time For Re-queue Invoice With SNT Status (CW1 Support Only)";
		}

		protected override Func<IDefaultValuesForCountrySpecificRegistryItems, int> DefaultValueGetter
		{
			get
			{
				return defaultRegistryValuesForCountry => defaultRegistryValuesForCountry.EInvoicingRequeueDelayTime;
			}
		}
	}
}
