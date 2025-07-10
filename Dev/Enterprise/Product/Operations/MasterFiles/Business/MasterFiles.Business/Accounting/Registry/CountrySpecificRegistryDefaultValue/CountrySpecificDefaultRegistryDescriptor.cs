using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CountrySpecificRegistryDefaultValue
{
	public abstract class CountrySpecificDefaultRegistryDescriptor<T> : ICountrySpecificDefaultRegistryValue
	{
		MultilingualString ICountrySpecificDefaultRegistryValue.GetCaption()
		{
			return GetDefaultTypeCaption();
		}

		string ICountrySpecificDefaultRegistryValue.GetDefaultValueForDisplay(ZString countryCode)
		{
			if (!dictionaryOfCountryCodeAndDefaultValue.ContainsKey(countryCode))
			{
				dictionaryOfCountryCodeAndDefaultValue[countryCode] = GetDisplayValue(GetDefaultValue(countryCode));
			}
			return dictionaryOfCountryCodeAndDefaultValue[countryCode];
		}

		public T GetDefaultValue(ZString countryCode)
		{
			return DefaultValueGetter(ObjectFactory.Get<ICountrySpecificRegistryDefaultValuesProvider>().Get(countryCode));
		}

		protected virtual string GetDisplayValue(T value) => value.ToString();

		public MultilingualString GetDefaultTypeCaption() => GetDefaultTypeCaptionCore();

		protected abstract Func<IDefaultValuesForCountrySpecificRegistryItems, T> DefaultValueGetter { get; }

		protected abstract MultilingualString GetDefaultTypeCaptionCore();

		readonly Dictionary<string, string> dictionaryOfCountryCodeAndDefaultValue = new Dictionary<string, string>();
	}
}
