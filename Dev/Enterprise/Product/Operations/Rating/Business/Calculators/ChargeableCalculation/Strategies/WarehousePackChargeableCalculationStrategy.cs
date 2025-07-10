using CargoWise.Types;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.Rating.Business.Calculators.ChargeableCalculation.Strategies
{
	class WarehousePackChargeableCalculationStrategy : DefaultChargeableCalculationStrategy
	{
		public WarehousePackChargeableCalculationStrategy(Calculator calculator) : base(calculator)
		{
		}

		protected override Quantity GetAmount(AutoRatingCalculatorParameters parameters, ZString unit, MeasureType measureType, bool useContainerReferences)
		{
			return parameters.GetQuantity(measureType, Calculator.Line, unit, unitIsPackage: false, useContainerReferences: useContainerReferences);
		}
	}
}
