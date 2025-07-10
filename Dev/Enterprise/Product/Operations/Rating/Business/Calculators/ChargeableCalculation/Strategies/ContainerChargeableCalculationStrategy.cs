using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture;

namespace Enterprise.Rating.Business
{
	public class ContainerChargeableCalculationStrategy : ChargeableCalculationStrategyBase
	{
		public ContainerChargeableCalculationStrategy(Calculator calculator)
			: base(calculator)
		{
		}

		protected override Quantity GetChargeableAmountCore(AutoRatingCalculatorParameters parameters, ZString unit)
		{
			var line = RateLine;
			if (line.ChargeCode == null)
			{
				return new Quantity(0, unit);
			}

			var containers = Enumerable.Empty<IRateableContainer>();
			string reference = null;
			bool shouldContainersBeEmpty = false;
			var rateEntry = line.ParentRateEntry;
			var serviceCode = line.ChargeCode.AC_ChargeSubGroup;
			if (!serviceCode.IsEmpty && parameters.ServiceRater.IsEnabled)
			{
				(containers, reference, shouldContainersBeEmpty) = parameters.GetContainersForContainerServices(line);
			}

			if (!containers.Any() && !shouldContainersBeEmpty)
			{
				containers = parameters.GetChargeableContainers(line);
				if (rateEntry != null && !rateEntry.ContainerPKForSpotEntry.IsEmpty)
				{
					bool isCosting = line.IsCosting();
					containers = containers.Where(c => isCosting ? c.ContainerSpotRates.CostSpotRateIsValid : c.ContainerSpotRates.SellSpotRateIsValid);
				}
			}

			var wiseRateEntry = rateEntry as WiseEntry;
			var containerQuality = wiseRateEntry?.ContainerQuality ?? string.Empty;

			if (parameters.Criteria.IsLooseRateSearchForCarrierConnect && wiseRateEntry != null)
			{
				if (!parameters.Criteria.JobID.IsEmpty)
				{
					containers = containers.Where(c => c.ContainerQuality == containerQuality);
				}
			}
			else if (parameters.Criteria.IsManualCostSelectMode && wiseRateEntry != null)
			{
				containers = containers.Where(c => string.Equals(containerQuality, c.ContainerQuality));
			}
			else
			{
				containers = containers.Where(c =>
					string.Equals(containerQuality, c.ContainerQuality) ||
					(string.IsNullOrEmpty(containerQuality) &&
					!parameters.Results.Any(r => r.Entry is WiseEntry wiseEntry && string.Equals(wiseEntry.ContainerQuality, c.ContainerQuality))));
			}

			reference = reference ?? string.Join(", ", containers.Select(c => c.ContainerNumber).Where(n => !string.IsNullOrEmpty(n)).Distinct());

			var count = unit == QuantityUnit.TU
				? containers.Sum(c => c.TEU)
				: containers.Sum(c => GetContainerCount(parameters, c));

			var chargeableUnit = unit;
			if (unit == QuantityUnit.CN && rateEntry?.Container != null)
			{
				chargeableUnit = rateEntry.Container.RC_Code;
			}
			return new Quantity(count, chargeableUnit, reference: reference);
		}

		int GetContainerCount(AutoRatingCalculatorParameters parameters, IRateableContainer info)
		{
			var count = 0;

			if (Calculator.MultipleEquipmentsOverMaxWeightVolume)
			{
				count = parameters.GetOccupiedContainerCount(Calculator.Line, info);
			}

			if (count == 0)
			{
				count = info.ContainerCount;
			}

			if (count == 0)
			{
				count = 1;
			}

			return count;
		}
	}
}

