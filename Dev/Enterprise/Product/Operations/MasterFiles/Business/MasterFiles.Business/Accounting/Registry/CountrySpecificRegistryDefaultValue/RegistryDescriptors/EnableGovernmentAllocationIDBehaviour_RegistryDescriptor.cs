using System;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CountrySpecificRegistryDefaultValue
{
	public class EnableGovernmentAllocatedNumberBehavior_RegistryDescriptor : CountrySpecificDefaultRegistryDescriptor<bool>
	{
		protected override MultilingualString GetDefaultTypeCaptionCore()
			=> (NoResString)"Enable Government Allocated Number Behavior";

		protected override Func<IDefaultValuesForCountrySpecificRegistryItems, bool> DefaultValueGetter
		{
			get
			{
				return defaultRegistryValuesForCountry => defaultRegistryValuesForCountry.EnableGovernmentAllocatedNumberBehavior;
			}
		}
	}
}
