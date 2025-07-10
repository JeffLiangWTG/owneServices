using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business;

static class CalculatorHelper
{
	internal static void CloneAndUpdateRateLineItemsWithPercentAndBaseRate<T>(
		CompanyTariffOrCostBasedCalculator ctbCalc,
		RateLine clone,
		RateLineItemsView rateLineItems,
		RateLineItem.RateTypeToUpdate rateTypeToUpdate)
		where T : Calculator =>
		UpdateRateLineItems<T>(clone, rateLineItems, rateTypeToUpdate, ctbCalc.Percent, ctbCalc.BaseRateWithApplicableIncrease);

	internal static void CloneAndUpdateRateLineItemsWithPerUnitPercentAndBaseRate<T>(
		CompanyTariffOrCostBasedCalculator ctbCalc,
		RateLine clone,
		RateLineItemsView rateLineItems,
		RateLineItem.RateTypeToUpdate rateTypeToUpdate)
		where T : Calculator =>
		UpdateRateLineItems<T>(clone, rateLineItems, rateTypeToUpdate, ctbCalc.PerUnitPercent, ctbCalc.PerUnitWithApplicableIncrease);

	static void UpdateRateLineItems<T>(
		RateLine clone,
		RateLineItemsView rateLineItems,
		RateLineItem.RateTypeToUpdate rateTypeToUpdate,
		ZDecimal percent,
		ZDecimal baseRate)
		where T : Calculator
	{
		if (clone.Calculator is T clonedCalc)
		{
			foreach (var item in rateLineItems.Cast<RateLineItem>())
			{
				clone.RateLineItems.CloneItem(item).UpdateRateValue(
					(x) => Utilities.Round(x * (percent + 100) / 100 + baseRate, 2),
					rateTypeToUpdate);
			}
		}
	}
}
