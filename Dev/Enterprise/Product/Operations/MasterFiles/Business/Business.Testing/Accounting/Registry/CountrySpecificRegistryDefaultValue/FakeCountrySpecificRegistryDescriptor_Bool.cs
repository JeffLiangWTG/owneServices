using System;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.MasterFiles.Business.CountrySpecificRegistryDefaultValue;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class FakeCountrySpecificRegistryDescriptor_Bool : CountrySpecificDefaultRegistryDescriptor<bool>
	{
		readonly MultilingualString caption;
		readonly Func<IDefaultValuesForCountrySpecificRegistryItems, bool> defaultValueGetter;
		public FakeCountrySpecificRegistryDescriptor_Bool(MultilingualString caption, Func<IDefaultValuesForCountrySpecificRegistryItems, bool> defaultValueGetter)
		{
			this.caption = caption;
			this.defaultValueGetter = defaultValueGetter;
		}

		protected override MultilingualString GetDefaultTypeCaptionCore()
		{
			return (NoResString)caption;
		}

		protected override Func<IDefaultValuesForCountrySpecificRegistryItems, bool> DefaultValueGetter
		{
			get
			{
				return defaultValueGetter;
			}
		}
	}
}
