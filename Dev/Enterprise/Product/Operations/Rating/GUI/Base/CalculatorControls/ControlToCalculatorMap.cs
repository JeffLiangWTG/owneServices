using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI
{
	public sealed class ControlToCalculatorMap
	{
		public ControlToCalculatorMap() { }

		internal Type GetControlType(Type calculatorType)
		{
			Type result;
			return CalculatorToControlDictionary.TryGetValue(calculatorType, out result) ? result : null;
		}

		internal Type GetCalculatorType(Type controlType)
		{
			return CalculatorToControlDictionary.Values.FirstOrDefault(o => o == controlType);
		}

		readonly Dictionary<Type, Type> CalculatorToControlDictionary = new Dictionary<Type, Type>()
			{
				{ typeof(AgencyCalculator), typeof(AgencyControl) },
				{ typeof(CartageCalculator), typeof(CartageControl) },
				{ typeof(CartageZoneDistanceCalculator), typeof(CartageZoneDistanceControl) },
				{ typeof(CombinedCalculator), typeof(CombinedControl) },
				{ typeof(CombinedBreaksWithIncrementCalculator), typeof(CBIControl) },
				{ typeof(CompanyTariffOrCostBasedCalculator), typeof(CompanyTariffOrCostBasedCalculatorControl) },
				{ typeof(DisbursementInterestCalculator), typeof(DisbursementInterestControl) },
				{ typeof(EquipmentHireCalculator), typeof(EquipmentHireControl) },
				{ typeof(EqualizationCalculator), typeof(EqualizationCalculatorControl) },
				{ typeof(ExcludeCompanyTariffsCalculator), typeof(ExcludeCompanyTariffsCalculatorUserControl) },
				{ typeof(FirstPlusAdditionalCalculator), typeof(FirstPlusAdditionalControl) },
				{ typeof(FlatPlusPerUnitCalculator), typeof(FlatPlusPerUnitControl) },
				{ typeof(FlatCalculator), typeof(FlatControl) },
				{ typeof(HighestChargeCalculator), typeof(HighestChargeControl) },
				{ typeof(HighestRateCalculator), typeof(HighestRateControl) },
				{ typeof(HousebillReleaseTypeCalculator), typeof(HousebillCalculatorUserControl) },
				{ typeof(FreightInclusiveCalculator), typeof(FreightInclusiveCalculatorUserControl) },
				{ typeof(MinimumCalculator), typeof(MinimumControl) },
				{ typeof(MinimumOrPerUnitCalculator), typeof(MinimumOrPerUnitControl) },
				{ typeof(NoteCalculator), typeof(NoteCalculatorUserControl) },
				{ typeof(PackageCountCalculator) , typeof(ItalianAirportTaxControl) },
				{ typeof(PercentageBreaksCalculator), typeof(PercentageBreaksControl) },
				{ typeof(PercentageCalculator), typeof(PercentageControl) },
				{ typeof(ProfitShareRebateCalculator), typeof(ProfitShareRebateControl) },
				{ typeof(SplitMonthBillingCalculator), typeof(SplitMonthBillingControl) },
				{ typeof(TimeCalculator), typeof(TimeControl) },
				{ typeof(UnitCalculator), typeof(UnitControl) },
				{ typeof(ValueRangeCalculator), typeof(ValueRangeControl) },
				{ typeof(WarehouseLocationTypeCalculator), typeof(WarehouseLocationTypeCalculatorControl) },
				{ typeof(WarehousePackCalculator), typeof(WarehousePackCalculatorControl) },
			};
	}
}

