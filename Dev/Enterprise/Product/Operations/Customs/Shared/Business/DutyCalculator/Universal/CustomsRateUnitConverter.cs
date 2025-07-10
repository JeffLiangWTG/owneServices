using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.DutyCalculator;

public class CustomsRateUnitConvertersProvider : ICustomsRateUnitConvertersProvider
{
	ICustomsRateUnitConverter ICustomsRateUnitConvertersProvider.GetCustomsRateAlcoholConverter()
	{
		return new CustomsRateAlcoholConverter();
	}

	ICustomsRateUnitConverter ICustomsRateUnitConvertersProvider.GetCustomsRateEnergyConverter()
	{
		return new CustomsRateEnergyConverter();
	}

	ICustomsRateUnitConverter ICustomsRateUnitConvertersProvider.GetCustomsRateLengthConverter()
	{
		return new CustomsRateLengthConverter();
	}

	ICustomsRateUnitConverter ICustomsRateUnitConvertersProvider.GetCustomsRateNumberConverter()
	{
		return new CustomsRateNumberConverter();
	}

	ICustomsRateUnitConverter ICustomsRateUnitConvertersProvider.GetCustomsRateSurfaceConverter()
	{
		return new CustomsRateSurfaceConverter();
	}

	ICustomsRateUnitConverter ICustomsRateUnitConvertersProvider.GetCustomsRateVolumeConverter()
	{
		return new CustomsRateVolumeConverter();
	}

	ICustomsRateUnitConverter ICustomsRateUnitConvertersProvider.GetCustomsRateWeightConverter()
	{
		return new CustomsRateWeightConverter();
	}
}

public class CustomsRateWeightConverter : CustomsRateUnitConverter
{
	protected sealed override IDictionary<string, decimal> ConversionDictionaryCore => ConvertibleUnitsOfMeasure.WeightConversionDictionary;
}

public class CustomsRateVolumeConverter : CustomsRateUnitConverter
{
	protected sealed override IDictionary<string, decimal> ConversionDictionaryCore => ConvertibleUnitsOfMeasure.VolumeConversionDictionary;
}

public class CustomsRateAlcoholConverter : CustomsRateUnitConverter
{
	protected sealed override IDictionary<string, decimal> ConversionDictionaryCore => ConvertibleUnitsOfMeasure.AlcoholConversionDictionary;
}

public class CustomsRateNumberConverter : CustomsRateUnitConverter
{
	protected sealed override IDictionary<string, decimal> ConversionDictionaryCore => new Dictionary<string, decimal>();
}

public class CustomsRateLengthConverter : CustomsRateUnitConverter
{
	protected sealed override IDictionary<string, decimal> ConversionDictionaryCore => new Dictionary<string, decimal>();
}

public class CustomsRateSurfaceConverter : CustomsRateUnitConverter
{
	protected sealed override IDictionary<string, decimal> ConversionDictionaryCore => new Dictionary<string, decimal>();
}

public class CustomsRateEnergyConverter : CustomsRateUnitConverter
{
	protected sealed override IDictionary<string, decimal> ConversionDictionaryCore => new Dictionary<string, decimal>();
}
