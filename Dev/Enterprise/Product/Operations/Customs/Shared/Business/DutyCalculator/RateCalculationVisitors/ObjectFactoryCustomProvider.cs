using System.Collections;
using CargoWise.Application;

namespace Enterprise.Customs.DutyCalculator;

static class ObjectFactoryCustomProvider
{
	public static TType GetValueForCountryOrDefault<TType>(string dictionaryName, string countryCode)
	{
		var types = ObjectFactory.Get<Hashtable>(dictionaryName);
		TType result = default;

		if (!string.IsNullOrEmpty(countryCode))
		{
			var objectHandle = (ObjectHandle)types[countryCode];
			result = (TType)objectHandle?.GetObject();
		}

		if (result == null)
		{
			var objectHandle = (ObjectHandle)types[DefaultRegistryKeyName];
			result = (TType)objectHandle?.GetObject();
		}

		return result;
	}

	const string DefaultRegistryKeyName = "DEFAULT";
}
