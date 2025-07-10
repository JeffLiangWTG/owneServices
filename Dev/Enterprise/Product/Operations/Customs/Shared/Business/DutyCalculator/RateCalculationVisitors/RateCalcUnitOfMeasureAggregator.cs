using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DutyCalculator;

public class RateCalcUnitOfMeasureAggregator
{
	public IDictionary<string, decimal> GetUomQtyDictionary(CusEntryLine entryLine)
	{
		_ = Argument.NotNull(entryLine, nameof(entryLine));
		var resultDictionary = new Dictionary<string, decimal>();

		foreach (BaseJobComInvoiceLine invoiceLine in entryLine.InvoiceLines.Cast<BaseJobComInvoiceLine>())
		{
			var invoiceLineUnitList = new List<string>();
			UpdateUnitOfMeasureDictionary(resultDictionary, invoiceLineUnitList, invoiceLine);
		}

		AddConvertibleUnitsToUnitOfMeasureDictionary(resultDictionary, entryLine.Factory, entryLine.CountryCode);
		return resultDictionary;
	}

	public IDictionary<string, decimal> GetUomQtyDictionary(BaseJobComInvoiceLine invoiceLine)
	{
		_ = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		var resultDictionary = new Dictionary<string, decimal>();
		var invoiceLineUnitList = new List<string>();
		UpdateUnitOfMeasureDictionary(resultDictionary, invoiceLineUnitList, invoiceLine);

		var countryCode = invoiceLine?.InvoiceHeader?.InvoiceCountry.RN_Code ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		AddConvertibleUnitsToUnitOfMeasureDictionary(resultDictionary, invoiceLine.Factory, countryCode);
		return resultDictionary;
	}

	public static void AddConvertibleUnitsToUnitOfMeasureDictionary(IDictionary<string, decimal> unitQtyValueDictionary, BusinessObjectFactory factory, ZString countryCode)
	{
		var provider = GetCustomsRateConvertersProviderByCountryCode(factory, countryCode);
		if (provider != null)
		{
			provider.GetCustomsRateWeightConverter().AddConvertableUnitValuesToDictionary(unitQtyValueDictionary);
			provider.GetCustomsRateVolumeConverter().AddConvertableUnitValuesToDictionary(unitQtyValueDictionary);
			provider.GetCustomsRateAlcoholConverter().AddConvertableUnitValuesToDictionary(unitQtyValueDictionary);
			provider.GetCustomsRateNumberConverter().AddConvertableUnitValuesToDictionary(unitQtyValueDictionary);
			provider.GetCustomsRateLengthConverter().AddConvertableUnitValuesToDictionary(unitQtyValueDictionary);
			provider.GetCustomsRateSurfaceConverter().AddConvertableUnitValuesToDictionary(unitQtyValueDictionary);
			provider.GetCustomsRateEnergyConverter().AddConvertableUnitValuesToDictionary(unitQtyValueDictionary);
		}
		unitQtyValueDictionary.Add(UniversalReferenceConstants.FlatRate, 1);
	}

	public static bool IsConvertableFrom(ZString unitQty, IEnumerable<ZString> list, ZString countryCode, BusinessObjectFactory factory)
	{
		var provider = GetCustomsRateConvertersProviderByCountryCode(factory, countryCode);
		return provider != null
				&& (IsConvertableFrom(provider.GetCustomsRateWeightConverter(), unitQty, list)
					|| IsConvertableFrom(provider.GetCustomsRateVolumeConverter(), unitQty, list)
					|| IsConvertableFrom(provider.GetCustomsRateAlcoholConverter(), unitQty, list)
					|| IsConvertableFrom(provider.GetCustomsRateNumberConverter(), unitQty, list)
					|| IsConvertableFrom(provider.GetCustomsRateLengthConverter(), unitQty, list)
					|| IsConvertableFrom(provider.GetCustomsRateSurfaceConverter(), unitQty, list)
					|| IsConvertableFrom(provider.GetCustomsRateEnergyConverter(), unitQty, list));
	}

	void UpdateUnitOfMeasureDictionary(Dictionary<string, decimal> resultDictionary, List<string> invoiceLineUnitList, BaseJobComInvoiceLine invoiceLine)
	{
		UpdateUnitOfMeasureDictionaryIfNeeded(resultDictionary, invoiceLineUnitList, invoiceLine.JI_CustomsUnitQty, invoiceLine.JI_CustomsQuantity);
		UpdateUnitOfMeasureDictionaryIfNeeded(resultDictionary, invoiceLineUnitList, invoiceLine.JI_CustomsSecondUnitQty, invoiceLine.JI_CustomsSecondQuantity);
		UpdateUnitOfMeasureDictionaryIfNeeded(resultDictionary, invoiceLineUnitList, invoiceLine.JI_CustomsThirdUnitQty, invoiceLine.JI_CustomsThirdQuantity);
		UpdateUnitOfMeasureDictionaryIfNeeded(resultDictionary, invoiceLineUnitList, invoiceLine.JI_CustomsFourthUnitQty, invoiceLine.JI_CustomsFourthQuantity);
		UpdateUnitOfMeasureDictionaryIfNeeded(resultDictionary, invoiceLineUnitList, invoiceLine.JI_CustomsFifthUnitQty, invoiceLine.JI_CustomsFifthQuantity);
	}

	void UpdateUnitOfMeasureDictionaryIfNeeded(IDictionary<string, decimal> unitQtyValueDictionary, List<string> alreadyAddedUnits, string unitQty, decimal quantityValue)
	{
		if (!string.IsNullOrWhiteSpace(unitQty) && !alreadyAddedUnits.Contains(unitQty))
		{
			alreadyAddedUnits.Add(unitQty);
			unitQtyValueDictionary.AddNewKeyOrAccumulateValue(unitQty, quantityValue);
		}
	}

	static bool IsConvertableFrom(Integration.Customs.ICustomsRateUnitConverter converter, ZString unitQty, IEnumerable<ZString> list)
	{
		return converter.CanConvertUnit(unitQty) && list.Any(u => converter.CanConvertUnit(u));
	}

	static Integration.Customs.ICustomsRateUnitConvertersProvider GetCustomsRateConvertersProviderByCountryCode(BusinessObjectFactory factory, string countryCode)
	{
		return factory.GetCachedValue(string.Format(CultureInfo.InvariantCulture, "CustomsRateUnitConvertersProvider_{0}", countryCode),
			() => ObjectFactoryCustomProvider.GetValueForCountryOrDefault<Integration.Customs.ICustomsRateUnitConvertersProvider>(
																						dictionaryName: nameof(Integration.Customs.ICustomsRateUnitConvertersProvider),
																						countryCode: countryCode));
	}
}
