using System.Collections.Immutable;

namespace Enterprise.Customs.DutyCalculator;

public abstract class CustomsRateUnitConverter : Integration.Customs.ICustomsRateUnitConverter
{
	public void AddConvertableUnitValuesToDictionary(IDictionary<string, decimal> unitQtyValueDictionary)
	{
		var baseUnit = unitQtyValueDictionary.Keys.Intersect(ConversionDictionary.Keys).FirstOrDefault();

		if (baseUnit != null)
		{
			var applicableUnitValuePairs = ConversionDictionary.Where(kvp => !unitQtyValueDictionary.ContainsKey(kvp.Key));

			if (applicableUnitValuePairs.Any())
			{
				var baseValue = unitQtyValueDictionary[baseUnit] * ConversionDictionary[baseUnit];

				foreach (var convertableUnitsAndConvertedValues in applicableUnitValuePairs)
				{
					unitQtyValueDictionary.Add(convertableUnitsAndConvertedValues.Key, baseValue / convertableUnitsAndConvertedValues.Value);
				}
			}
		}
	}

	protected IDictionary<string, decimal> ConversionDictionary => ConversionDictionaryCore.Concat(CountrySpecificConversionDictionary)
		.ToLookup(pair => pair.Key, x => x.Value)
		.ToDictionary(group => group.Key, x => x.First()).ToImmutableDictionary();

	protected abstract IDictionary<string, decimal> ConversionDictionaryCore { get; }

	protected virtual IDictionary<string, decimal> CountrySpecificConversionDictionary => new Dictionary<string, decimal>();

	bool Integration.Customs.ICustomsRateUnitConverter.CanConvertUnit(string unit) => ConversionDictionary.ContainsKey(unit);
}
