using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Rating.Business
{
	public abstract class ChargeableCalculationStrategyBase
	{
		protected ChargeableCalculationStrategyBase(Calculator calculator)
		{
			Argument.NotNull(calculator, "Calculator");

			Calculator = calculator;
		}

		#region Properties

		protected Calculator Calculator { get; set; }

		protected IRateLine RateLine => Calculator.Line;

		#endregion

		#region Methods

		public Quantity GetChargeableAmount(AutoRatingCalculatorParameters parameters, ZString unit)
		{
			if (string.IsNullOrEmpty(unit))
			{
				unit = Calculator.GetUnit(parameters);
			}

			return GetChargeableAmountCore(parameters, unit);
		}

		protected abstract Quantity GetChargeableAmountCore(AutoRatingCalculatorParameters parameters, ZString unit);

		#endregion
	}
}
