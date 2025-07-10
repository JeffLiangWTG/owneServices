using System;
using WTG.Glow.Data.Annotations;
using WTG.ProductionRules.Core;

namespace Enterprise.ProductionRules.Business
{
	public sealed class UnitConverter : IUnitConverter
	{
		public decimal Convert(MeasurePropertyType measureType, decimal value, string fromUnit, string toUnit)
		{
			return GetUnitConverterStrategy(measureType).Convert(value, fromUnit, toUnit);
		}

		public bool IsValidUnit(MeasurePropertyType measureType, string unit)
		{
			return GetUnitConverterStrategy(measureType).IsValidUnit(unit);
		}

		IUnitConverterStrategy GetUnitConverterStrategy(MeasurePropertyType measureType)
		{
			switch (measureType)
			{
				case MeasurePropertyType.Dimension:
					return DimensionConverterStrategy;
				case MeasurePropertyType.Distance:
					return DistanceConverterStrategy;
				case MeasurePropertyType.Length:
					return LengthConverterStrategy;
				case MeasurePropertyType.Temperature:
					return TemperatureConverterStrategy;
				case MeasurePropertyType.Volume:
					return VolumeConverterStrategy;
				case MeasurePropertyType.Weight:
					return WeightConverterStrategy;
				default:
					throw new ArgumentException($"Invalid {nameof(MeasurePropertyType)}: {measureType}");
			}
		}

		DimensionConverterStrategy DimensionConverterStrategy { get; } = new DimensionConverterStrategy();
		DistanceConverterStrategy DistanceConverterStrategy { get; } = new DistanceConverterStrategy();
		LengthConverterStrategy LengthConverterStrategy { get; } = new LengthConverterStrategy();
		TemperatureConverterStrategy TemperatureConverterStrategy { get; } = new TemperatureConverterStrategy();
		VolumeConverterStrategy VolumeConverterStrategy { get; } = new VolumeConverterStrategy();
		WeightConverterStrategy WeightConverterStrategy { get; } = new WeightConverterStrategy();
	}
}
