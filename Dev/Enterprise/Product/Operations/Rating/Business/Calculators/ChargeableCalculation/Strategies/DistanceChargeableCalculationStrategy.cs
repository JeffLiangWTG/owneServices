using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.Rating.Business
{
	public class DistanceChargeableCalculationStrategy : ChargeableCalculationStrategyBase
	{
		public DistanceChargeableCalculationStrategy(Calculator calculator)
			: base(calculator)
		{
		}

		protected override Quantity GetChargeableAmountCore(AutoRatingCalculatorParameters parameters, ZString unit)
		{
			if (QuantityUnit.IsDistance(unit))
			{
				var log = new ZStringBuilder();
				var distance = new RatingDistanceCalculationHelper(parameters.Criteria, RateLine).GetDistance(log);

				if (unit != QuantityUnit.KM && Core.Constants.Length.ContainsCode(unit))
				{
					distance = Core.Constants.Length.Convert(distance, Core.Constants.Length.Kilometres, unit);
				}

				var distanceLog = ZString.Format(@"{0} {1} {2}{3}{4}", RateLine.DisplayInfo(), distance, unit, System.Environment.NewLine, log.ToString());
				parameters.Logger.Information(distanceLog);

				return new Quantity(distance, unit);
			}

			return new Quantity(ZDecimal.Zero, unit);
		}
	}
}

