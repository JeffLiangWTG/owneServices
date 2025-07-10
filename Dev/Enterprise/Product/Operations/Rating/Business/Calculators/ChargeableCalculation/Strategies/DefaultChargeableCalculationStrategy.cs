using System.Linq;
using CargoWise.Types;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.Rating.Business
{
	public class DefaultChargeableCalculationStrategy : ChargeableCalculationStrategyBase
	{
		public DefaultChargeableCalculationStrategy(Calculator calculator)
			: base(calculator)
		{
		}

		protected override Quantity GetChargeableAmountCore(AutoRatingCalculatorParameters parameters, ZString unit)
		{
			var measureType = parameters.GetMeasureTypes(RateLine).FirstOrDefault();

			switch (measureType)
			{
				case MeasureType.Unidentified:
				case MeasureType.JobVolume:
				case MeasureType.JobWeight:
				case MeasureType.Volume:
				case MeasureType.StorageVolume:
				case MeasureType.Weight:
				case MeasureType.StorageWeight:
				case MeasureType.LoadingMeters:
				case MeasureType.InnerPacksWeight:
				case MeasureType.InnerPacksVolume:
					{
						var chargeable = parameters.GetChargeableAmount(RateLine);
						var targetUnit = unit.IsEmpty ? chargeable.Unit : unit;
						return chargeable.AmountFor(targetUnit);
					}
				case MeasureType.WarehousePackage:
				case MeasureType.WarehousePackageWeight:
				case MeasureType.WarehousePackageVolume:
					return parameters.CalculateChargeableForWarehouse(RateLine, unit, measureType);
				case MeasureType.Unit:
				case MeasureType.InnerPacksUnit:
					{
						// Would be useful to work out why only this particular case wants container references
						return GetAmount(parameters, unit, measureType, useContainerReferences: true);
					}

				default:
					{
						return GetAmount(parameters, unit, measureType, useContainerReferences: false);
					}
			}
		}

		protected virtual Quantity GetAmount(AutoRatingCalculatorParameters parameters, ZString unit, MeasureType measureType, bool useContainerReferences)
		{
			var unitIsPackage = measureType == MeasureType.Unit || measureType == MeasureType.StorageUnit || measureType == MeasureType.InnerPacksUnit;
			return parameters.GetQuantity(measureType, RateLine, unit, unitIsPackage, useContainerReferences: useContainerReferences);
		}
	}
}

