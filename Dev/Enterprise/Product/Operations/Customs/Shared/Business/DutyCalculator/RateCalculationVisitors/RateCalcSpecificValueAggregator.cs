using System.Globalization;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DutyCalculator;

public class RateCalcSpecificValueAggregator
{
	public IDictionary<string, decimal> GetSpecificValueDictionary(CusEntryLine entryLine)
	{
		var result = new Dictionary<string, decimal>();
		AddConvertibleSpecificValueDictionary(result, entryLine);
		AddNilCountrySpecificKeyIfNotPresent(result);
		return result;
	}

	public IDictionary<string, decimal> GetSpecificValueDictionary(BaseJobComInvoiceLine invoiceLine)
	{
		var result = new Dictionary<string, decimal>();
		AddNilCountrySpecificKeyIfNotPresent(result);
		return result;
	}

	void AddNilCountrySpecificKeyIfNotPresent(Dictionary<string, decimal> valueDictionary)
	{
		if (!valueDictionary.TryGetValue(UniversalReferenceConstants.NilCountrySpecificKey, out decimal value))
		{
			valueDictionary.Add(UniversalReferenceConstants.NilCountrySpecificKey, value);
		}
	}

	void AddConvertibleSpecificValueDictionary(IDictionary<string, decimal> specificValueDictionary, CusEntryLine entryLine)
	{
		var provider = GetCustomsRateConvertersProviderByCountryCode(entryLine.Factory, entryLine.CountryCode);
		foreach (var item in provider.GetCountrySpecificValueList(entryLine))
		{
			specificValueDictionary.Add(item);
		}
	}

	Integration.Customs.ICountrySpecificValueProvider GetCustomsRateConvertersProviderByCountryCode(BusinessObjectFactory factory, string countryCode)
	{
		return factory.GetCachedValue(string.Format(CultureInfo.InvariantCulture, "GetCustomsRateConvertersProvider_{0}", countryCode),
			() => ObjectFactoryCustomProvider.GetValueForCountryOrDefault<Integration.Customs.ICountrySpecificValueProvider>(
																	dictionaryName: nameof(Integration.Customs.ICountrySpecificValueProvider),
																	countryCode: countryCode));
	}
}
