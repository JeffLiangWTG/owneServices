using System;
using CargoWise.Types;
using Enterprise.Rating.Business.Calculators.ChargeableCalculation.Strategies;
using Enterprise.ZArchitecture;

namespace Enterprise.Rating.Business
{
	public class ChargeableValueProvider
	{
		readonly Calculator calculator;

		public ChargeableValueProvider(Calculator calculator)
		{
			this.calculator = calculator;
		}

		public Quantity CalculateChargeable(AutoRatingCalculatorParameters parameters, string unit = "")
		{
			if (string.IsNullOrEmpty(unit))
			{
				unit = calculator.GetUnit(parameters);
			}

			var calculationStrategy = GetChargeableCalculationStrategy(unit)
				?? throw new InvalidOperationException("calculationStrategy");

			return calculationStrategy.GetChargeableAmount(parameters, unit);
		}

		ChargeableCalculationStrategyBase GetChargeableCalculationStrategy(ZString unit)
		{
			if (calculator is WarehousePackCalculator)
			{
				return new WarehousePackChargeableCalculationStrategy(calculator);
			}

			if (calculator is HighestRateCalculator)
			{
				return new HRCChargeableCalculationStrategy(calculator);
			}

			if (QuantityUnit.IsTime(unit))
			{
				return new TimeChargeableCalculationStrategy(calculator);
			}

			if (unit == RatingConstants.Units.CN || unit == RatingConstants.Units.TU)
			{
				return new ContainerChargeableCalculationStrategy(calculator);
			}

			if (QuantityUnit.IsDistance(unit))
			{
				return new DistanceChargeableCalculationStrategy(calculator);
			}

			if (unit == QuantityUnit.SV)
			{
				return new ServiceOccurrenceChargeableCalculationStrategy(calculator);
			}
			return new DefaultChargeableCalculationStrategy(calculator);
		}
	}
}

