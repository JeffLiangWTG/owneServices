using System;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.MasterFiles.Business.CountrySpecificRegistryDefaultValue;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class FakeCountrySpecificRegistryDescriptor_String : CountrySpecificDefaultRegistryDescriptor<string>
	{
		readonly MultilingualString caption;
		readonly Func<IDefaultValuesForCountrySpecificRegistryItems, string> defaultValueGetter;
		public FakeCountrySpecificRegistryDescriptor_String(MultilingualString caption, Func<IDefaultValuesForCountrySpecificRegistryItems, string> defaultValueGetter)
		{
			this.caption = caption;
			this.defaultValueGetter = defaultValueGetter;
		}

		protected override MultilingualString GetDefaultTypeCaptionCore()
		{
			return (NoResString)caption;
		}

		protected override Func<IDefaultValuesForCountrySpecificRegistryItems, string> DefaultValueGetter
		{
			get
			{
				return defaultValueGetter;
			}
		}
	}
}
