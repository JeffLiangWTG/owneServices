namespace Enterprise.Rating.Business
{
	using System.Linq;
	using CargoWise.Types;
	using Enterprise.Core;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Rating.Integration;
	using Enterprise.ZArchitecture;

	public class HRCChargeableCalculationStrategy : ChargeableCalculationStrategyBase
	{
		public HRCChargeableCalculationStrategy(Calculator calculator)
			: base(calculator)
		{
		}

		protected override Quantity GetChargeableAmountCore(AutoRatingCalculatorParameters parameters, ZString unit)
		{
			var items = RateLine.ChildRateLineItems.ToArray();
			var unitItemCount = items.Count(item => item.TM_Type == Calculator.Items.Operator.UNT);

			if (unitItemCount == 1 && items.Any(c => c.TM_Type == Calculator.Items.RatePickRule && c.TM_Text == Calculator.Items.AsFreightedHighestRateWhenMin))
			{
				var firstFreightRateInfo = Calculator.GetFirstFreightRateInfo(parameters);
				if (firstFreightRateInfo != null && firstFreightRateInfo.Bases.Calculate().minimum == decimal.MinValue)
				{
					if (!string.IsNullOrWhiteSpace(unit))
					{
						return new Quantity(firstFreightRateInfo.Bases.Sum(x => x.Chargeable.AmountFor(unit).Amount), unit);
					}

					return new Quantity(firstFreightRateInfo.Bases.Sum(x => x.Chargeable.Amount), firstFreightRateInfo.ChargeUnit);
				}
			}

			if (unitItemCount >= 2 || RateLine.UseOnlyActualWeightMeasure())
			{
				MeasureType measureType;

				if (RateLine.TL_IsWhsJobLevelCharge)
				{
					measureType = Constants.Weight.ContainsCode(unit) ? MeasureType.JobWeight : MeasureType.JobVolume;
				}
				else
				{
					measureType = Constants.Weight.ContainsCode(unit) ? MeasureType.Weight : MeasureType.Volume;
				}

				var actualQuantity = parameters.GetQuantity(measureType, RateLine);

				if (!unit.IsEmpty)
				{
					return actualQuantity.AmountFor(unit);
				}

				return actualQuantity;
			}

			return parameters.GetChargeableAmount(RateLine);
		}
	}
}

